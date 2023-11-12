
/*
 The MIT License (MIT)

 Copyright (c) 2015-2016 Microsoft

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE. 

*/

using System;
using System.IO;
using System.Buffers;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

#pragma warning disable CS0809

namespace System.IO
{
    /// <summary>
	/// MemoryStream implementation that deals with pooling and managing memory streams which use potentially large
	/// buffers.
	/// </summary>
	/// <remarks>
	/// This class works in tandem with the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager" /> to supply <c>MemoryStream</c>-derived
	/// objects to callers, while avoiding these specific problems:
	/// <list type="number">
	/// <item>
	/// <term>LOH allocations</term>
	/// <description>Since all large buffers are pooled, they will never incur a Gen2 GC</description>
	/// </item>
	/// <item>
	/// <term>Memory waste</term><description>A standard memory stream doubles its size when it runs out of room. This
	/// leads to continual memory growth as each stream approaches the maximum allowed size.</description>
	/// </item>
	/// <item>
	/// <term>Memory copying</term>
	/// <description>Each time a <c>MemoryStream</c> grows, all the bytes are copied into new buffers.
	/// This implementation only copies the bytes when <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> is called.</description>
	/// </item>
	/// <item>
	/// <term>Memory fragmentation</term>
	/// <description>By using homogeneous buffer sizes, it ensures that blocks of memory
	/// can be easily reused.
	/// </description>
	/// </item>
	/// </list>
	/// <para>
	/// The stream is implemented on top of a series of uniformly-sized blocks. As the stream's length grows,
	/// additional blocks are retrieved from the memory manager. It is these blocks that are pooled, not the stream
	/// object itself.
	/// </para>
	/// <para>
	/// The biggest wrinkle in this implementation is when <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> is called. This requires a single
	/// contiguous buffer. If only a single block is in use, then that block is returned. If multiple blocks
	/// are in use, we retrieve a larger buffer from the memory manager. These large buffers are also pooled,
	/// split by size--they are multiples/exponentials of a chunk size (1 MB by default).
	/// </para>
	/// <para>
	/// Once a large buffer is assigned to the stream the small blocks are NEVER again used for this stream. All operations take place on the
	/// large buffer. The large buffer can be replaced by a larger buffer from the pool as needed. All blocks and large buffers
	/// are maintained in the stream until the stream is disposed (unless AggressiveBufferReturn is enabled in the stream manager).
	/// </para>
	/// <para>
	/// A further wrinkle is what happens when the stream is longer than the maximum allowable array length under .NET. This is allowed
	/// when only blocks are in use, and only the Read/Write APIs are used. Once a stream grows to this size, any attempt to convert it
	/// to a single buffer will result in an exception. Similarly, if a stream is already converted to use a single larger buffer, then
	/// it cannot grow beyond the limits of the maximum allowable array size.
	/// </para>
	/// <para>
	/// Any method that modifies the stream has the potential to throw an <c>OutOfMemoryException</c>, either because
	/// the stream is beyond the limits set in <c>RecyclableStreamManager</c>, or it would result in a buffer larger than
	/// the maximum array size supported by .NET.
	/// </para>
	/// </remarks>
	public sealed class RecyclableMemoryStream : MemoryStream, IBufferWriter<byte>
    {
        private sealed class BlockSegment : ReadOnlySequenceSegment<byte>
        {
            public BlockSegment(Memory<byte> memory)
            {
                base.Memory = memory;
            }

            public BlockSegment Append(Memory<byte> memory)
            {
                return (BlockSegment)(base.Next = new BlockSegment(memory)
                {
                    RunningIndex = base.RunningIndex + base.Memory.Length
                });
            }
        }

        private struct BlockAndOffset
        {
            public int Block;

            public int Offset;

            public BlockAndOffset(int block, int offset)
            {
                Block = block;
                Offset = offset;
            }
        }

        private static readonly byte[] emptyArray = new byte[0];

        /// <summary>
        /// All of these blocks must be the same size.
        /// </summary>
        private readonly List<byte[]> blocks;

        private readonly Guid id;

        private readonly RecyclableMemoryStreamManager memoryManager;

        private readonly string tag;

        private readonly long creationTimestamp;

        /// <summary>
        /// This list is used to store buffers once they're replaced by something larger.
        /// This is for the cases where you have users of this class that may hold onto the buffers longer
        /// than they should and you want to prevent race conditions which could corrupt the data.
        /// </summary>
        private List<byte[]> dirtyBuffers;

        private bool disposed;

        /// <summary>
        /// This is only set by GetBuffer() if the necessary buffer is larger than a single block size, or on
        /// construction if the caller immediately requests a single large buffer.
        /// </summary>
        /// <remarks>If this field is non-null, it contains the concatenation of the bytes found in the individual
        /// blocks. Once it is created, this (or a larger) largeBuffer will be used for the life of the stream.
        /// </remarks>
        private byte[] largeBuffer;

        private long length;

        private long position;

        private byte[] bufferWriterTempBuffer;

        /// <summary>
        /// Unique identifier for this stream across its entire lifetime.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        internal Guid Id
        {
            get
            {
                CheckDisposed();
                return id;
            }
        }

        /// <summary>
        /// A temporary identifier for the current usage of this stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        internal string Tag
        {
            get
            {
                CheckDisposed();
                return tag;
            }
        }

        /// <summary>
        /// Gets the memory manager being used by this stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        internal RecyclableMemoryStreamManager MemoryManager
        {
            get
            {
                CheckDisposed();
                return memoryManager;
            }
        }

        /// <summary>
        /// Callstack of the constructor. It is only set if <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.GenerateCallStacks" /> is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string AllocationStack { get; }

        /// <summary>
        /// Callstack of the <see cref="M:Microsoft.IO.RecyclableMemoryStream.Dispose(System.Boolean)" /> call. It is only set if <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.GenerateCallStacks" /> is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string DisposeStack { get; private set; }

        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Capacity is always in multiples of the memory manager's block size, unless
        /// the large buffer is in use. Capacity never decreases during a stream's lifetime.
        /// Explicitly setting the capacity to a lower value than the current value will have no effect.
        /// This is because the buffers are all pooled by chunks and there's little reason to
        /// allow stream truncation.
        /// </para>
        /// <para>
        /// Writing past the current capacity will cause <see cref="P:Microsoft.IO.RecyclableMemoryStream.Capacity" /> to automatically increase, until MaximumStreamCapacity is reached.
        /// </para>
        /// <para>
        /// If the capacity is larger than <c>int.MaxValue</c>, then <c>InvalidOperationException</c> will be thrown. If you anticipate using
        /// larger streams, use the <see cref="P:Microsoft.IO.RecyclableMemoryStream.Capacity64" /> property instead.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Capacity is larger than int.MaxValue.</exception>
        public override int Capacity
        {
            get
            {
                CheckDisposed();
                if (largeBuffer != null)
                {
                    return largeBuffer.Length;
                }
                long num = (long)blocks.Count * (long)memoryManager.BlockSize;
                if (num > int.MaxValue)
                {
                    throw new InvalidOperationException("Capacity is larger than int.MaxValue. Use Capacity64 instead.");
                }
                return (int)num;
            }
            set
            {
                Capacity64 = value;
            }
        }

        /// <summary>
        /// Returns a 64-bit version of capacity, for streams larger than <c>int.MaxValue</c> in length.
        /// </summary>
        public long Capacity64
        {
            get
            {
                CheckDisposed();
                if (largeBuffer != null)
                {
                    return largeBuffer.Length;
                }
                return (long)blocks.Count * (long)memoryManager.BlockSize;
            }
            set
            {
                CheckDisposed();
                EnsureCapacity(value);
            }
        }

        /// <summary>
        /// Gets the number of bytes written to this stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <remarks>If the buffer has already been converted to a large buffer, then the maximum length is limited by the maximum allowed array length in .NET.</remarks>
        public override long Length
        {
            get
            {
                CheckDisposed();
                return length;
            }
        }

        /// <summary>
        /// Gets the current position in the stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">A negative value was passed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream is in large-buffer mode, but an attempt was made to set the position past the maximum allowed array length.</exception>
        /// <remarks>If the buffer has already been converted to a large buffer, then the maximum length (and thus position) is limited by the maximum allowed array length in .NET.</remarks>
        public override long Position
        {
            get
            {
                CheckDisposed();
                return position;
            }
            set
            {
                CheckDisposed();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be non-negative.");
                }
                if (largeBuffer != null && value > 2147483591)
                {
                    throw new InvalidOperationException($"Once the stream is converted to a single large buffer, position cannot be set past {2147483591}.");
                }
                position = value;
            }
        }

        /// <summary>
        /// Whether the stream can currently read.
        /// </summary>
        public override bool CanRead => !Disposed;

        /// <summary>
        /// Whether the stream can currently seek.
        /// </summary>
        public override bool CanSeek => !Disposed;

        /// <summary>
        /// Always false.
        /// </summary>
        public override bool CanTimeout => false;

        /// <summary>
        /// Whether the stream can currently write.
        /// </summary>
        public override bool CanWrite => !Disposed;

        private bool Disposed => disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
            : this(memoryManager, Guid.NewGuid(), null, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id)
            : this(memoryManager, id, null, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag)
            : this(memoryManager, Guid.NewGuid(), tag, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag)
            : this(memoryManager, id, tag, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize)
            : this(memoryManager, Guid.NewGuid(), tag, requestedSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, long requestedSize)
            : this(memoryManager, Guid.NewGuid(), tag, requestedSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, int requestedSize)
            : this(memoryManager, id, tag, (long)requestedSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, long requestedSize)
            : this(memoryManager, id, tag, requestedSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        /// <param name="initialLargeBuffer">An initial buffer to use. This buffer will be owned by the stream and returned to the memory manager upon Dispose.</param>
        internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, long requestedSize, byte[] initialLargeBuffer)
            : base(emptyArray)
        {
            this.memoryManager = memoryManager;
            this.id = id;
            this.tag = tag;
            blocks = new List<byte[]>();
            creationTimestamp = Stopwatch.GetTimestamp();
            long num = Math.Max(requestedSize, this.memoryManager.BlockSize);
            if (initialLargeBuffer == null)
            {
                EnsureCapacity(num);
            }
            else
            {
                largeBuffer = initialLargeBuffer;
            }
            if (this.memoryManager.GenerateCallStacks)
            {
                AllocationStack = Environment.StackTrace;
            }
            this.memoryManager.ReportStreamCreated(this.id, this.tag, requestedSize, num);
            this.memoryManager.ReportUsageReport();
        }

        /// <summary>
        /// The finalizer will be called when a stream is not disposed properly.
        /// </summary>
        /// <remarks>Failing to dispose indicates a bug in the code using streams. Care should be taken to properly account for stream lifetime.</remarks>
        ~RecyclableMemoryStream()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Returns the memory used by this stream back to the pool.
        /// </summary>
        /// <param name="disposing">Whether we're disposing (true), or being called by the finalizer (false).</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                string disposeStack = null;
                if (memoryManager.GenerateCallStacks)
                {
                    disposeStack = Environment.StackTrace;
                }
                memoryManager.ReportStreamDoubleDisposed(id, tag, AllocationStack, DisposeStack, disposeStack);
                return;
            }
            disposed = true;
            TimeSpan lifetime = TimeSpan.FromTicks((Stopwatch.GetTimestamp() - creationTimestamp) * 10000000 / Stopwatch.Frequency);
            if (memoryManager.GenerateCallStacks)
            {
                DisposeStack = Environment.StackTrace;
            }
            memoryManager.ReportStreamDisposed(id, tag, lifetime, AllocationStack, DisposeStack);
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            else
            {
                memoryManager.ReportStreamFinalized(id, tag, AllocationStack);
                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    base.Dispose(disposing);
                    return;
                }
            }
            memoryManager.ReportStreamLength(length);
            if (largeBuffer != null)
            {
                memoryManager.ReturnLargeBuffer(largeBuffer, id, tag);
            }
            if (dirtyBuffers != null)
            {
                foreach (byte[] dirtyBuffer in dirtyBuffers)
                {
                    memoryManager.ReturnLargeBuffer(dirtyBuffer, id, tag);
                }
            }
            memoryManager.ReturnBlocks(blocks, id, tag);
            memoryManager.ReportUsageReport();
            blocks.Clear();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Equivalent to <c>Dispose</c>.
        /// </summary>
        public override void Close()
        {
            Dispose(disposing: true);
        }

        /// <summary>
        /// Returns a single buffer containing the contents of the stream.
        /// The buffer may be longer than the stream length.
        /// </summary>
        /// <returns>A byte[] buffer.</returns>
        /// <remarks>IMPORTANT: Doing a <see cref="M:Microsoft.IO.RecyclableMemoryStream.Write(System.Byte[],System.Int32,System.Int32)" /> after calling <c>GetBuffer</c> invalidates the buffer. The old buffer is held onto
        /// until <see cref="M:Microsoft.IO.RecyclableMemoryStream.Dispose(System.Boolean)" /> is called, but the next time <c>GetBuffer</c> is called, a new buffer from the pool will be required.</remarks>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.OutOfMemoryException">stream is too large for a contiguous buffer.</exception>
        public override byte[] GetBuffer()
        {
            CheckDisposed();
            if (largeBuffer != null)
            {
                return largeBuffer;
            }
            if (blocks.Count == 1)
            {
                return blocks[0];
            }
            byte[] buffer = memoryManager.GetLargeBuffer(Capacity64, id, tag);
            AssertLengthIsSmall();
            InternalRead(buffer, 0, (int)length, 0L);
            largeBuffer = buffer;
            if (blocks.Count > 0 && memoryManager.AggressiveBufferReturn)
            {
                memoryManager.ReturnBlocks(blocks, id, tag);
                blocks.Clear();
            }
            return largeBuffer;
        }

        /// <summary>Asynchronously reads all the bytes from the current position in this stream and writes them to another stream.</summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">This parameter is ignored.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="destination" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
        /// <exception cref="T:System.NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
        /// <remarks>Similarly to <c>MemoryStream</c>'s behavior, <c>CopyToAsync</c> will adjust the source stream's position by the number of bytes written to the destination stream, as a Read would do.</remarks>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            CheckDisposed();
            if (length == 0L)
            {
                return Task.CompletedTask;
            }
            long num = position;
            long num2 = length - num;
            position += num2;
            if (destination is MemoryStream stream)
            {
                WriteTo(stream, num, num2);
                return Task.CompletedTask;
            }
            if (largeBuffer == null)
            {
                if (blocks.Count == 1)
                {
                    AssertLengthIsSmall();
                    return destination.WriteAsync(blocks[0], (int)num, (int)num2, cancellationToken);
                }
                return CopyToAsyncImpl(destination, GetBlockAndRelativeOffset(num), num2, blocks, cancellationToken);
            }
            AssertLengthIsSmall();
            return destination.WriteAsync(largeBuffer, (int)num, (int)num2, cancellationToken);
            static async Task CopyToAsyncImpl(Stream destination, BlockAndOffset blockAndOffset, long count, List<byte[]> blocks, CancellationToken cancellationToken)
            {
                long bytesRemaining = count;
                int currentBlock = blockAndOffset.Block;
                int num3 = blockAndOffset.Offset;
                while (bytesRemaining > 0)
                {
                    byte[] array = blocks[currentBlock];
                    int amountToCopy = (int)Math.Min(array.Length - num3, bytesRemaining);
                    await destination.WriteAsync(array, num3, amountToCopy, cancellationToken);
                    bytesRemaining -= amountToCopy;
                    int num4 = currentBlock + 1;
                    currentBlock = num4;
                    num3 = 0;
                }
            }
        }

        /// <summary>
        /// Notifies the stream that <paramref name="count" /> bytes were written to the buffer returned by <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetMemory(System.Int32)" /> or <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetSpan(System.Int32)" />.
        /// Seeks forward by <paramref name="count" /> bytes.
        /// </summary>
        /// <remarks>
        /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
        /// </remarks>
        /// <param name="count">How many bytes to advance.</param>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="count" /> is larger than the size of the previously requested buffer.</exception>
        public void Advance(int count)
        {
            CheckDisposed();
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count must be non-negative.");
            }
            byte[] array = bufferWriterTempBuffer;
            if (array != null)
            {
                if (count > array.Length)
                {
                    throw new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {array.Length}.");
                }
                Write(array, 0, count);
                ReturnTempBuffer(array);
                bufferWriterTempBuffer = null;
            }
            else
            {
                long num = ((largeBuffer == null) ? (memoryManager.BlockSize - GetBlockAndRelativeOffset(position).Offset) : (largeBuffer.Length - position));
                if (count > num)
                {
                    throw new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {num}.");
                }
                position += count;
                length = Math.Max(position, length);
            }
        }

        private void ReturnTempBuffer(byte[] buffer)
        {
            if (buffer.Length == memoryManager.BlockSize)
            {
                memoryManager.ReturnBlock(buffer, id, tag);
            }
            else
            {
                memoryManager.ReturnLargeBuffer(buffer, id, tag);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// IMPORTANT: Calling Write(), GetBuffer(), TryGetBuffer(), Seek(), GetLength(), Advance(),
        /// or setting Position after calling GetMemory() invalidates the memory.
        /// </remarks>
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            return GetWritableBuffer(sizeHint);
        }

        /// <inheritdoc />
        /// <remarks>
        /// IMPORTANT: Calling Write(), GetBuffer(), TryGetBuffer(), Seek(), GetLength(), Advance(),
        /// or setting Position after calling GetSpan() invalidates the span.
        /// </remarks>
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetWritableBuffer(sizeHint);
        }

        /// <summary>
        /// When callers to GetSpan() or GetMemory() request a buffer that is larger than the remaining size of the current block
        /// this method return a temp buffer. When Advance() is called, that temp buffer is then copied into the stream.
        /// </summary>
        private ArraySegment<byte> GetWritableBuffer(int sizeHint)
        {
            CheckDisposed();
            if (sizeHint < 0)
            {
                throw new ArgumentOutOfRangeException("sizeHint", "sizeHint must be non-negative.");
            }
            int num = Math.Max(sizeHint, 1);
            EnsureCapacity(position + num);
            if (bufferWriterTempBuffer != null)
            {
                ReturnTempBuffer(bufferWriterTempBuffer);
                bufferWriterTempBuffer = null;
            }
            if (largeBuffer != null)
            {
                return new ArraySegment<byte>(largeBuffer, (int)position, largeBuffer.Length - (int)position);
            }
            BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(position);
            if (MemoryManager.BlockSize - blockAndRelativeOffset.Offset >= num)
            {
                return new ArraySegment<byte>(blocks[blockAndRelativeOffset.Block], blockAndRelativeOffset.Offset, MemoryManager.BlockSize - blockAndRelativeOffset.Offset);
            }
            bufferWriterTempBuffer = ((num > memoryManager.BlockSize) ? memoryManager.GetLargeBuffer(num, id, tag) : memoryManager.GetBlock());
            return new ArraySegment<byte>(bufferWriterTempBuffer);
        }

        /// <summary>
        /// Returns a sequence containing the contents of the stream.
        /// </summary>
        /// <returns>A ReadOnlySequence of bytes.</returns>
        /// <remarks>IMPORTANT: Calling Write(), GetMemory(), GetSpan(), Dispose(), or Close() after calling GetReadOnlySequence() invalidates the sequence.</remarks>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public ReadOnlySequence<byte> GetReadOnlySequence()
        {
            CheckDisposed();
            if (largeBuffer != null)
            {
                AssertLengthIsSmall();
                return new ReadOnlySequence<byte>(largeBuffer, 0, (int)length);
            }
            if (blocks.Count == 1)
            {
                AssertLengthIsSmall();
                return new ReadOnlySequence<byte>(blocks[0], 0, (int)length);
            }
            BlockSegment blockSegment = new BlockSegment(blocks[0]);
            BlockSegment blockSegment2 = blockSegment;
            int num = 1;
            while (blockSegment2.RunningIndex + blockSegment2.Memory.Length < length)
            {
                blockSegment2 = blockSegment2.Append(blocks[num]);
                num++;
            }
            return new ReadOnlySequence<byte>(blockSegment, 0, blockSegment2, (int)(length - blockSegment2.RunningIndex));
        }

        /// <summary>
        /// Returns an <c>ArraySegment</c> that wraps a single buffer containing the contents of the stream.
        /// </summary>
        /// <param name="buffer">An <c>ArraySegment</c> containing a reference to the underlying bytes.</param>
        /// <returns>Returns <see langword="true" /> if a buffer can be returned; otherwise, <see langword="false" />.</returns>
        public override bool TryGetBuffer(out ArraySegment<byte> buffer)
        {
            CheckDisposed();
            try
            {
                if (length <= 2147483591)
                {
                    buffer = new ArraySegment<byte>(GetBuffer(), 0, (int)Length);
                    return true;
                }
            }
            catch (OutOfMemoryException)
            {
            }
            buffer = default(ArraySegment<byte>);
            return false;
        }

        /// <summary>
        /// Returns a new array with a copy of the buffer's contents. You should almost certainly be using <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> combined with the <see cref="P:Microsoft.IO.RecyclableMemoryStream.Length" /> to
        /// access the bytes in this stream. Calling <c>ToArray</c> will destroy the benefits of pooled buffers, but it is included
        /// for the sake of completeness.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.NotSupportedException">The current <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager" />object disallows <c>ToArray</c> calls.</exception>
        /// <exception cref="T:System.OutOfMemoryException">The length of the stream is too long for a contiguous array.</exception>
        [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided." , true)]
        public override byte[] ToArray()
        {
            CheckDisposed();
            string stack = (memoryManager.GenerateCallStacks ? Environment.StackTrace : null);
            memoryManager.ReportStreamToArray(id, tag, stack, length);
            if (memoryManager.ThrowExceptionOnToArray)
            {
                throw new NotSupportedException("The underlying RecyclableMemoryStreamManager is configured to not allow calls to ToArray.");
            }
            byte[] array = new byte[Length];
            InternalRead(array, 0, (int)length, 0L);
            return array;
        }

        /// <summary>
        /// Reads from the current position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ArgumentNullException">buffer is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">offset subtracted from the buffer length is less than count.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return SafeRead(buffer, offset, count, ref position);
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream position is beyond <c>int.MaxValue</c>.</exception>
        public int SafeRead(byte[] buffer, int offset, int count, ref int streamPosition)
        {
            long streamPosition2 = streamPosition;
            int result = SafeRead(buffer, offset, count, ref streamPosition2);
            if (streamPosition2 > int.MaxValue)
            {
                throw new InvalidOperationException("Stream position is beyond int.MaxValue. Use SafeRead(byte[], int, int, ref long) override.");
            }
            streamPosition = (int)streamPosition2;
            return result;
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int SafeRead(byte[] buffer, int offset, int count, ref long streamPosition)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset cannot be negative.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count cannot be negative.");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("buffer length must be at least offset + count.");
            }
            int num = InternalRead(buffer, offset, count, streamPosition);
            streamPosition += num;
            return num;
        }

        /// <summary>
        /// Reads from the current position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int Read(Span<byte> buffer)
        {
            return SafeRead(buffer, ref position);
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream position is beyond <c>int.MaxValue</c>.</exception>
        public int SafeRead(Span<byte> buffer, ref int streamPosition)
        {
            long streamPosition2 = streamPosition;
            int result = SafeRead(buffer, ref streamPosition2);
            if (streamPosition2 > int.MaxValue)
            {
                throw new InvalidOperationException("Stream position is beyond int.MaxValue. Use SafeRead(Span<byte>, ref long) override.");
            }
            streamPosition = (int)streamPosition2;
            return result;
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int SafeRead(Span<byte> buffer, ref long streamPosition)
        {
            CheckDisposed();
            int num = InternalRead(buffer, streamPosition);
            streamPosition += num;
            return num;
        }

        /// <summary>
        /// Writes the buffer to the stream.
        /// </summary>
        /// <param name="buffer">Source buffer.</param>
        /// <param name="offset">Start position.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException">buffer is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative.</exception>
        /// <exception cref="T:System.ArgumentException">buffer.Length - offset is not less than count.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "offset must be in the range of 0 - buffer.Length-1.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", count, "count must be non-negative.");
            }
            if (count + offset > buffer.Length)
            {
                throw new ArgumentException("count must be greater than buffer.Length - offset.");
            }
            int blockSize = memoryManager.BlockSize;
            long newCapacity = position + count;
            EnsureCapacity(newCapacity);
            if (largeBuffer == null)
            {
                int num = count;
                int num2 = 0;
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(position);
                while (num > 0)
                {
                    byte[] dst = blocks[blockAndRelativeOffset.Block];
                    int num3 = Math.Min(blockSize - blockAndRelativeOffset.Offset, num);
                    Buffer.BlockCopy(buffer, offset + num2, dst, blockAndRelativeOffset.Offset, num3);
                    num -= num3;
                    num2 += num3;
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, largeBuffer, (int)position, count);
            }
            position = newCapacity;
            length = Math.Max(position, length);
        }

        /// <summary>
        /// Writes the buffer to the stream.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <exception cref="T:System.ArgumentNullException">buffer is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void Write(ReadOnlySpan<byte> source)
        {
            CheckDisposed();
            int blockSize = memoryManager.BlockSize;
            long newCapacity = position + source.Length;
            EnsureCapacity(newCapacity);
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(position);
                while (source.Length > 0)
                {
                    byte[] array = blocks[blockAndRelativeOffset.Block];
                    int start = Math.Min(blockSize - blockAndRelativeOffset.Offset, source.Length);
                    source.Slice(0, start).CopyTo(array.AsSpan(blockAndRelativeOffset.Offset));
                    source = source.Slice(start);
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
            }
            else
            {
                source.CopyTo(largeBuffer.AsSpan((int)position));
            }
            position = newCapacity;
            length = Math.Max(position, length);
        }

        /// <summary>
        /// Returns a useful string for debugging. This should not normally be called in actual production code.
        /// </summary>
        public override string ToString()
        {
            if (!disposed)
            {
                return $"Id = {Id}, Tag = {Tag}, Length = {Length:N0} bytes";
            }
            return $"Disposed: Id = {id}, Tag = {tag}, Final Length: {length:N0} bytes";
        }

        /// <summary>
        /// Writes a single byte to the current position in the stream.
        /// </summary>
        /// <param name="value">byte value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void WriteByte(byte value)
        {
            CheckDisposed();
            long newCapacity = position + 1;
            if (largeBuffer == null)
            {
                int blockSize = memoryManager.BlockSize;
                long result;
                int num = (int)Math.DivRem(position, blockSize, out result);
                if (num >= blocks.Count)
                {
                    EnsureCapacity(newCapacity);
                }
                blocks[num][result] = value;
            }
            else
            {
                if (position >= largeBuffer.Length)
                {
                    EnsureCapacity(newCapacity);
                }
                largeBuffer[position] = value;
            }
            position = newCapacity;
            if (position > length)
            {
                length = position;
            }
        }

        /// <summary>
        /// Reads a single byte from the current position in the stream.
        /// </summary>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override int ReadByte()
        {
            return SafeReadByte(ref position);
        }

        /// <summary>
        /// Reads a single byte from the specified position in the stream.
        /// </summary>
        /// <param name="streamPosition">The position in the stream to read from.</param>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream position is beyond <c>int.MaxValue</c>.</exception>
        public int SafeReadByte(ref int streamPosition)
        {
            long streamPosition2 = streamPosition;
            int result = SafeReadByte(ref streamPosition2);
            if (streamPosition2 > int.MaxValue)
            {
                throw new InvalidOperationException("Stream position is beyond int.MaxValue. Use SafeReadByte(ref long) override.");
            }
            streamPosition = (int)streamPosition2;
            return result;
        }

        /// <summary>
        /// Reads a single byte from the specified position in the stream.
        /// </summary>
        /// <param name="streamPosition">The position in the stream to read from.</param>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int SafeReadByte(ref long streamPosition)
        {
            CheckDisposed();
            if (streamPosition == length)
            {
                return -1;
            }
            byte result;
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(streamPosition);
                result = blocks[blockAndRelativeOffset.Block][blockAndRelativeOffset.Offset];
            }
            else
            {
                result = largeBuffer[streamPosition];
            }
            streamPosition++;
            return result;
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">value is negative or larger than <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumStreamCapacity" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void SetLength(long value)
        {
            CheckDisposed();
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "value must be non-negative.");
            }
            EnsureCapacity(value);
            length = value;
            if (position > value)
            {
                position = value;
            }
        }

        /// <summary>
        /// Sets the position to the offset from the seek location.
        /// </summary>
        /// <param name="offset">How many bytes to move.</param>
        /// <param name="loc">From where.</param>
        /// <returns>The new position.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> is larger than <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumStreamCapacity" />.</exception>
        /// <exception cref="T:System.ArgumentException">Invalid seek origin.</exception>
        /// <exception cref="T:System.IO.IOException">Attempt to set negative position.</exception>
        public override long Seek(long offset, SeekOrigin loc)
        {
            CheckDisposed();
            long num = loc switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => offset + position,
                SeekOrigin.End => offset + length,
                _ => throw new ArgumentException("Invalid seek origin.", "loc"),
            };
            if (num < 0)
            {
                throw new IOException("Seek before beginning.");
            }
            position = num;
            return position;
        }

        /// <summary>
        /// Synchronously writes this stream's bytes to the argument stream.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <remarks>Important: This does a synchronous write, which may not be desired in some situations.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void WriteTo(Stream stream)
        {
            WriteTo(stream, 0L, length);
        }

        /// <summary>
        /// Synchronously writes this stream's bytes, starting at offset, for count bytes, to the argument stream.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <param name="offset">Offset in source.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond  this <paramref name="stream" />'s length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(Stream stream, int offset, int count)
        {
            WriteTo(stream, (long)offset, (long)count);
        }

        /// <summary>
        /// Synchronously writes this stream's bytes, starting at offset, for count bytes, to the argument stream.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <param name="offset">Offset in source.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond  this <paramref name="stream" />'s length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(Stream stream, long offset, long count)
        {
            CheckDisposed();
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (offset < 0 || offset + count > length)
            {
                throw new ArgumentOutOfRangeException("offset must not be negative and offset + count must not exceed the length of the stream.", (Exception)null);
            }
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(offset);
                long num = count;
                int num2 = blockAndRelativeOffset.Block;
                int num3 = blockAndRelativeOffset.Offset;
                while (num > 0)
                {
                    byte[] array = blocks[num2];
                    int num4 = (int)Math.Min((long)array.Length - (long)num3, num);
                    stream.Write(array, num3, num4);
                    num -= num4;
                    num2++;
                    num3 = 0;
                }
            }
            else
            {
                stream.Write(largeBuffer, (int)offset, (int)count);
            }
        }

        /// <summary>
        /// Writes bytes from the current stream to a destination <c>byte</c> array.
        /// </summary>
        /// <param name="buffer">Target buffer.</param>
        /// <remarks>The entire stream is written to the target array.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" />&gt; is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(byte[] buffer)
        {
            WriteTo(buffer, 0L, Length);
        }

        /// <summary>
        /// Writes bytes from the current stream to a destination <c>byte</c> array.
        /// </summary>
        /// <param name="buffer">Target buffer.</param>
        /// <param name="offset">Offset in the source stream, from which to start.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" />&gt; is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond this stream's length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(byte[] buffer, long offset, long count)
        {
            WriteTo(buffer, offset, count, 0);
        }

        /// <summary>
        /// Writes bytes from the current stream to a destination <c>byte</c> array.
        /// </summary>
        /// <param name="buffer">Target buffer.</param>
        /// <param name="offset">Offset in the source stream, from which to start.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <param name="targetOffset">Offset in the target byte array to start writing</param>
        /// <exception cref="T:System.ArgumentNullException"><c>buffer</c> is null</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond this stream's length.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="targetOffset" /> is less than 0, or <paramref name="targetOffset" /> + <paramref name="count" /> is beyond the target <paramref name="buffer" />'s length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(byte[] buffer, long offset, long count, int targetOffset)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset + count > length)
            {
                throw new ArgumentOutOfRangeException("offset must not be negative and offset + count must not exceed the length of the stream.", (Exception)null);
            }
            if (targetOffset < 0 || count + targetOffset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("targetOffset must not be negative and targetOffset + count must not exceed the length of the target buffer.", (Exception)null);
            }
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(offset);
                long num = count;
                int num2 = blockAndRelativeOffset.Block;
                int num3 = blockAndRelativeOffset.Offset;
                int num4 = targetOffset;
                while (num > 0)
                {
                    byte[] array = blocks[num2];
                    int num5 = (int)Math.Min((long)array.Length - (long)num3, num);
                    Buffer.BlockCopy(array, num3, buffer, num4, num5);
                    num -= num5;
                    num2++;
                    num3 = 0;
                    num4 += num5;
                }
            }
            else
            {
                AssertLengthIsSmall();
                Buffer.BlockCopy(largeBuffer, (int)offset, buffer, targetOffset, (int)count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (Disposed)
            {
                ThrowDisposedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowDisposedException()
        {
            throw new ObjectDisposedException($"The stream with Id {id} and Tag {tag} is disposed.");
        }

        private int InternalRead(byte[] buffer, int offset, int count, long fromPosition)
        {
            if (length - fromPosition <= 0)
            {
                return 0;
            }
            int num3;
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(fromPosition);
                int num = 0;
                int num2 = (int)Math.Min(count, length - fromPosition);
                while (num2 > 0)
                {
                    byte[] array = blocks[blockAndRelativeOffset.Block];
                    num3 = Math.Min(array.Length - blockAndRelativeOffset.Offset, num2);
                    Buffer.BlockCopy(array, blockAndRelativeOffset.Offset, buffer, num + offset, num3);
                    num += num3;
                    num2 -= num3;
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
                return num;
            }
            num3 = (int)Math.Min(count, length - fromPosition);
            Buffer.BlockCopy(largeBuffer, (int)fromPosition, buffer, offset, num3);
            return num3;
        }

        private int InternalRead(Span<byte> buffer, long fromPosition)
        {
            if (length - fromPosition <= 0)
            {
                return 0;
            }
            int num3;
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(fromPosition);
                int num = 0;
                int num2 = (int)Math.Min(buffer.Length, length - fromPosition);
                while (num2 > 0)
                {
                    byte[] array = blocks[blockAndRelativeOffset.Block];
                    num3 = Math.Min(array.Length - blockAndRelativeOffset.Offset, num2);
                    array.AsSpan(blockAndRelativeOffset.Offset, num3).CopyTo(buffer.Slice(num));
                    num += num3;
                    num2 -= num3;
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
                return num;
            }
            num3 = (int)Math.Min(buffer.Length, length - fromPosition);
            largeBuffer.AsSpan((int)fromPosition, num3).CopyTo(buffer);
            return num3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BlockAndOffset GetBlockAndRelativeOffset(long offset)
        {
            int blockSize = memoryManager.BlockSize;
            long result;
            return new BlockAndOffset((int)Math.DivRem(offset, blockSize, out result), (int)result);
        }

        private void EnsureCapacity(long newCapacity)
        {
            if (newCapacity > memoryManager.MaximumStreamCapacity && memoryManager.MaximumStreamCapacity > 0)
            {
                memoryManager.ReportStreamOverCapacity(id, tag, newCapacity, AllocationStack);
                throw new OutOfMemoryException($"Requested capacity is too large: {newCapacity}. Limit is {memoryManager.MaximumStreamCapacity}.");
            }
            if (largeBuffer != null)
            {
                if (newCapacity > largeBuffer.Length)
                {
                    byte[] buffer = memoryManager.GetLargeBuffer(newCapacity, id, tag);
                    InternalRead(buffer, 0, (int)length, 0L);
                    ReleaseLargeBuffer();
                    largeBuffer = buffer;
                }
            }
            else
            {
                long num = newCapacity / memoryManager.BlockSize + 1;
                if (blocks.Capacity < num)
                {
                    blocks.Capacity = (int)num;
                }
                while (Capacity64 < newCapacity)
                {
                    blocks.Add(memoryManager.GetBlock());
                }
            }
        }

        /// <summary>
        /// Release the large buffer (either stores it for eventual release or returns it immediately).
        /// </summary>
        private void ReleaseLargeBuffer()
        {
            if (memoryManager.AggressiveBufferReturn)
            {
                memoryManager.ReturnLargeBuffer(largeBuffer, id, tag);
            }
            else
            {
                if (dirtyBuffers == null)
                {
                    dirtyBuffers = new List<byte[]>(1);
                }
                dirtyBuffers.Add(largeBuffer);
            }
            largeBuffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssertLengthIsSmall()
        {
        }
    }

    /// <summary>
    /// Manages pools of <see cref="System.IO.RecyclableMemoryStream" /> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There are two pools managed in here. The small pool contains same-sized buffers that are handed to streams
    /// as they write more data.
    ///             </para>
    ///             <para>
    /// For scenarios that need to call <see cref="System.IO.RecyclableMemoryStream.GetBuffer" />, the large pool contains buffers of various sizes, all
    /// multiples/exponentials of <see cref="System.IO.RecyclableMemoryStreamManager.LargeBufferMultiple" /> (1 MB by default). They are split by size to avoid overly-wasteful buffer
    /// usage. There should be far fewer 8 MB buffers than 1 MB buffers, for example.
    /// </para>
    /// </remarks>
    public sealed class RecyclableMemoryStreamManager
    {
        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamCreated" /> event.
        /// </summary>
        public sealed class StreamCreatedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Requested stream size.
            /// </summary>
            public long RequestedSize { get; }

            /// <summary>
            /// Actual stream size.
            /// </summary>
            public long ActualSize { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamCreatedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="requestedSize">The requested stream size.</param>
            /// <param name="actualSize">The actual stream size.</param>
            public StreamCreatedEventArgs(Guid guid, string tag, long requestedSize, long actualSize)
            {
                Id = guid;
                Tag = tag;
                RequestedSize = requestedSize;
                ActualSize = actualSize;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamDisposed" /> event.
        /// </summary>
        public sealed class StreamDisposedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where the stream was allocated.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// Stack where stream was disposed.
            /// </summary>
            public string DisposeStack { get; }

            /// <summary>
            /// Lifetime of the stream.
            /// </summary>
            public TimeSpan Lifetime { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamDisposedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            /// <param name="disposeStack">Dispose stack.</param>
            [Obsolete("Use another constructor override")]
            public StreamDisposedEventArgs(Guid guid, string tag, string allocationStack, string disposeStack)
                : this(guid, tag, TimeSpan.Zero, allocationStack, disposeStack)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamDisposedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="lifetime">Lifetime of the stream</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            /// <param name="disposeStack">Dispose stack.</param>
            public StreamDisposedEventArgs(Guid guid, string tag, TimeSpan lifetime, string allocationStack, string disposeStack)
            {
                Id = guid;
                Tag = tag;
                Lifetime = lifetime;
                AllocationStack = allocationStack;
                DisposeStack = disposeStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamDoubleDisposed" /> event.
        /// </summary>
        public sealed class StreamDoubleDisposedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where the stream was allocated.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// First dispose stack.
            /// </summary>
            public string DisposeStack1 { get; }

            /// <summary>
            /// Second dispose stack.
            /// </summary>
            public string DisposeStack2 { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamDoubleDisposedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            /// <param name="disposeStack1">First dispose stack.</param>
            /// <param name="disposeStack2">Second dispose stack.</param>
            public StreamDoubleDisposedEventArgs(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
            {
                Id = guid;
                Tag = tag;
                AllocationStack = allocationStack;
                DisposeStack1 = disposeStack1;
                DisposeStack2 = disposeStack2;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamFinalized" /> event.
        /// </summary>
        public sealed class StreamFinalizedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where the stream was allocated.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamFinalizedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            public StreamFinalizedEventArgs(Guid guid, string tag, string allocationStack)
            {
                Id = guid;
                Tag = tag;
                AllocationStack = allocationStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamConvertedToArray" /> event.
        /// </summary>
        public sealed class StreamConvertedToArrayEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where ToArray was called.
            /// </summary>
            public string Stack { get; }

            /// <summary>
            /// Length of stack.
            /// </summary>
            public long Length { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamConvertedToArrayEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="stack">Stack of ToArray call.</param>
            /// <param name="length">Length of stream.</param>
            public StreamConvertedToArrayEventArgs(Guid guid, string tag, string stack, long length)
            {
                Id = guid;
                Tag = tag;
                Stack = stack;
                Length = length;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamOverCapacity" /> event.
        /// </summary>
        public sealed class StreamOverCapacityEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Original allocation stack.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// Requested capacity.
            /// </summary>
            public long RequestedCapacity { get; }

            /// <summary>
            /// Maximum capacity.
            /// </summary>
            public long MaximumCapacity { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamOverCapacityEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="requestedCapacity">Requested capacity.</param>
            /// <param name="maximumCapacity">Maximum stream capacity of the manager.</param>
            /// <param name="allocationStack">Original allocation stack.</param>
            internal StreamOverCapacityEventArgs(Guid guid, string tag, long requestedCapacity, long maximumCapacity, string allocationStack)
            {
                Id = guid;
                Tag = tag;
                RequestedCapacity = requestedCapacity;
                MaximumCapacity = maximumCapacity;
                AllocationStack = allocationStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.BlockCreated" /> event.
        /// </summary>
        public sealed class BlockCreatedEventArgs : EventArgs
        {
            /// <summary>
            /// How many bytes are currently in use from the small pool.
            /// </summary>
            public long SmallPoolInUse { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.BlockCreatedEventArgs" /> class.
            /// </summary>
            /// <param name="smallPoolInUse">Number of bytes currently in use from the small pool.</param>
            internal BlockCreatedEventArgs(long smallPoolInUse)
            {
                SmallPoolInUse = smallPoolInUse;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.LargeBufferCreated" /> events.
        /// </summary>
        public sealed class LargeBufferCreatedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Whether the buffer was satisfied from the pool or not.
            /// </summary>
            public bool Pooled { get; }

            /// <summary>
            /// Required buffer size.
            /// </summary>
            public long RequiredSize { get; }

            /// <summary>
            /// How many bytes are in use from the large pool.
            /// </summary>
            public long LargePoolInUse { get; }

            /// <summary>
            /// If the buffer was not satisfied from the pool, and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.GenerateCallStacks" /> is turned on, then.
            /// this will contain the callstack of the allocation request.
            /// </summary>
            public string CallStack { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.LargeBufferCreatedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="requiredSize">Required size of the new buffer.</param>
            /// <param name="largePoolInUse">How many bytes from the large pool are currently in use.</param>
            /// <param name="pooled">Whether the buffer was satisfied from the pool or not.</param>
            /// <param name="callStack">Callstack of the allocation, if it wasn't pooled.</param>
            internal LargeBufferCreatedEventArgs(Guid guid, string tag, long requiredSize, long largePoolInUse, bool pooled, string callStack)
            {
                RequiredSize = requiredSize;
                LargePoolInUse = largePoolInUse;
                Pooled = pooled;
                Id = guid;
                Tag = tag;
                CallStack = callStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.BufferDiscarded" /> event.
        /// </summary>
        public sealed class BufferDiscardedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Type of the buffer.
            /// </summary>
            public Events.MemoryStreamBufferType BufferType { get; }

            /// <summary>
            /// The reason this buffer was discarded.
            /// </summary>
            public Events.MemoryStreamDiscardReason Reason { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.BufferDiscardedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="bufferType">Type of buffer being discarded.</param>
            /// <param name="reason">The reason for the discard.</param>
            internal BufferDiscardedEventArgs(Guid guid, string tag, Events.MemoryStreamBufferType bufferType, Events.MemoryStreamDiscardReason reason)
            {
                Id = guid;
                Tag = tag;
                BufferType = bufferType;
                Reason = reason;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamLength" /> event.
        /// </summary>
        public sealed class StreamLengthEventArgs : EventArgs
        {
            /// <summary>
            /// Length of the stream.
            /// </summary>
            public long Length { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamLengthEventArgs" /> class.
            /// </summary>
            /// <param name="length">Length of the strength.</param>
            public StreamLengthEventArgs(long length)
            {
                Length = length;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.UsageReport" /> event.
        /// </summary>
        public sealed class UsageReportEventArgs : EventArgs
        {
            /// <summary>
            /// Bytes from the small pool currently in use.
            /// </summary>
            public long SmallPoolInUseBytes { get; }

            /// <summary>
            /// Bytes from the small pool currently available.
            /// </summary>
            public long SmallPoolFreeBytes { get; }

            /// <summary>
            /// Bytes from the large pool currently in use.
            /// </summary>
            public long LargePoolInUseBytes { get; }

            /// <summary>
            /// Bytes from the large pool currently available.
            /// </summary>
            public long LargePoolFreeBytes { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.UsageReportEventArgs" /> class.
            /// </summary>
            /// <param name="smallPoolInUseBytes">Bytes from the small pool currently in use.</param>
            /// <param name="smallPoolFreeBytes">Bytes from the small pool currently available.</param>
            /// <param name="largePoolInUseBytes">Bytes from the large pool currently in use.</param>
            /// <param name="largePoolFreeBytes">Bytes from the large pool currently available.</param>
            public UsageReportEventArgs(long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes)
            {
                SmallPoolInUseBytes = smallPoolInUseBytes;
                SmallPoolFreeBytes = smallPoolFreeBytes;
                LargePoolInUseBytes = largePoolInUseBytes;
                LargePoolFreeBytes = largePoolFreeBytes;
            }
        }

        /// <summary>
        /// ETW events for RecyclableMemoryStream.
        /// </summary>
        [EventSource(Name = "Microsoft-IO-RecyclableMemoryStream", Guid = "{B80CD4E4-890E-468D-9CBA-90EB7C82DFC7}")]
        public sealed class Events : EventSource
        {
            /// <summary>
            /// Type of buffer.
            /// </summary>
            public enum MemoryStreamBufferType
            {
                /// <summary>
                /// Small block buffer.
                /// </summary>
                Small,
                /// <summary>
                /// Large pool buffer.
                /// </summary>
                Large
            }

            /// <summary>
            /// The possible reasons for discarding a buffer.
            /// </summary>
            public enum MemoryStreamDiscardReason
            {
                /// <summary>
                /// Buffer was too large to be re-pooled.
                /// </summary>
                TooLarge,
                /// <summary>
                /// There are enough free bytes in the pool.
                /// </summary>
                EnoughFree
            }

            /// <summary>
            /// Static log object, through which all events are written.
            /// </summary>
            public static Events Writer = new Events();

            /// <summary>
            /// Logged when a stream object is created.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="requestedSize">Requested size of the stream.</param>
            /// <param name="actualSize">Actual size given to the stream from the pool.</param>
            [Event(1, Level = EventLevel.Verbose, Version = 2)]
            public void MemoryStreamCreated(Guid guid, string tag, long requestedSize, long actualSize)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(1, guid, tag ?? string.Empty, requestedSize, actualSize);
                }
            }

            /// <summary>
            /// Logged when the stream is disposed.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="lifetimeMs">Lifetime in milliseconds of the stream</param>
            /// <param name="allocationStack">Call stack of initial allocation.</param>
            /// <param name="disposeStack">Call stack of the dispose.</param>
            [Event(2, Level = EventLevel.Verbose, Version = 3)]
            public void MemoryStreamDisposed(Guid guid, string tag, long lifetimeMs, string allocationStack, string disposeStack)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(2, guid, tag ?? string.Empty, lifetimeMs, allocationStack ?? string.Empty, disposeStack ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when the stream is disposed for the second time.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="allocationStack">Call stack of initial allocation.</param>
            /// <param name="disposeStack1">Call stack of the first dispose.</param>
            /// <param name="disposeStack2">Call stack of the second dispose.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(3, Level = EventLevel.Critical)]
            public void MemoryStreamDoubleDispose(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
            {
                if (IsEnabled())
                {
                    WriteEvent(3, guid, tag ?? string.Empty, allocationStack ?? string.Empty, disposeStack1 ?? string.Empty, disposeStack2 ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when a stream is finalized.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="allocationStack">Call stack of initial allocation.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(4, Level = EventLevel.Error)]
            public void MemoryStreamFinalized(Guid guid, string tag, string allocationStack)
            {
                if (IsEnabled())
                {
                    WriteEvent(4, guid, tag ?? string.Empty, allocationStack ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when ToArray is called on a stream.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="stack">Call stack of the ToArray call.</param>
            /// <param name="size">Length of stream.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(5, Level = EventLevel.Verbose, Version = 2)]
            public void MemoryStreamToArray(Guid guid, string tag, string stack, long size)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(5, guid, tag ?? string.Empty, stack ?? string.Empty, size);
                }
            }

            /// <summary>
            /// Logged when the RecyclableMemoryStreamManager is initialized.
            /// </summary>
            /// <param name="blockSize">Size of blocks, in bytes.</param>
            /// <param name="largeBufferMultiple">Size of the large buffer multiple, in bytes.</param>
            /// <param name="maximumBufferSize">Maximum buffer size, in bytes.</param>
            [Event(6, Level = EventLevel.Informational)]
            public void MemoryStreamManagerInitialized(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            {
                if (IsEnabled())
                {
                    WriteEvent(6, blockSize, largeBufferMultiple, maximumBufferSize);
                }
            }

            /// <summary>
            /// Logged when a new block is created.
            /// </summary>
            /// <param name="smallPoolInUseBytes">Number of bytes in the small pool currently in use.</param>
            [Event(7, Level = EventLevel.Warning, Version = 2)]
            public void MemoryStreamNewBlockCreated(long smallPoolInUseBytes)
            {
                if (IsEnabled(EventLevel.Warning, EventKeywords.None))
                {
                    WriteEvent(7, smallPoolInUseBytes);
                }
            }

            /// <summary>
            /// Logged when a new large buffer is created.
            /// </summary>
            /// <param name="requiredSize">Requested size.</param>
            /// <param name="largePoolInUseBytes">Number of bytes in the large pool in use.</param>
            [Event(8, Level = EventLevel.Warning, Version = 3)]
            public void MemoryStreamNewLargeBufferCreated(long requiredSize, long largePoolInUseBytes)
            {
                if (IsEnabled(EventLevel.Warning, EventKeywords.None))
                {
                    WriteEvent(8, requiredSize, largePoolInUseBytes);
                }
            }

            /// <summary>
            /// Logged when a buffer is created that is too large to pool.
            /// </summary>
            /// <param name="guid">Unique stream ID.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="requiredSize">Size requested by the caller.</param>
            /// <param name="allocationStack">Call stack of the requested stream.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(9, Level = EventLevel.Verbose, Version = 3)]
            public void MemoryStreamNonPooledLargeBufferCreated(Guid guid, string tag, long requiredSize, string allocationStack)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(9, guid, tag ?? string.Empty, requiredSize, allocationStack ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when a buffer is discarded (not put back in the pool, but given to GC to clean up).
            /// </summary>
            /// <param name="guid">Unique stream ID.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="bufferType">Type of the buffer being discarded.</param>
            /// <param name="reason">Reason for the discard.</param>
            /// <param name="smallBlocksFree">Number of free small pool blocks.</param>
            /// <param name="smallPoolBytesFree">Bytes free in the small pool.</param>
            /// <param name="smallPoolBytesInUse">Bytes in use from the small pool.</param>
            /// <param name="largeBlocksFree">Number of free large pool blocks.</param>
            /// <param name="largePoolBytesFree">Bytes free in the large pool.</param>
            /// <param name="largePoolBytesInUse">Bytes in use from the large pool.</param>
            [Event(10, Level = EventLevel.Warning, Version = 2)]
            public void MemoryStreamDiscardBuffer(Guid guid, string tag, MemoryStreamBufferType bufferType, MemoryStreamDiscardReason reason, long smallBlocksFree, long smallPoolBytesFree, long smallPoolBytesInUse, long largeBlocksFree, long largePoolBytesFree, long largePoolBytesInUse)
            {
                if (IsEnabled(EventLevel.Warning, EventKeywords.None))
                {
                    WriteEvent(10, guid, tag ?? string.Empty, bufferType, reason, smallBlocksFree, smallPoolBytesFree, smallPoolBytesInUse, largeBlocksFree, largePoolBytesFree, largePoolBytesInUse);
                }
            }

            /// <summary>
            /// Logged when a stream grows beyond the maximum capacity.
            /// </summary>
            /// <param name="guid">Unique stream ID</param>
            /// <param name="requestedCapacity">The requested capacity.</param>
            /// <param name="maxCapacity">Maximum capacity, as configured by RecyclableMemoryStreamManager.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="allocationStack">Call stack for the capacity request.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(11, Level = EventLevel.Error, Version = 3)]
            public void MemoryStreamOverCapacity(Guid guid, string tag, long requestedCapacity, long maxCapacity, string allocationStack)
            {
                if (IsEnabled())
                {
                    WriteEvent(11, guid, tag ?? string.Empty, requestedCapacity, maxCapacity, allocationStack ?? string.Empty);
                }
            }
        }

        /// <summary>
        /// Maximum length of a single array.
        /// </summary>
        /// <remarks>See documentation at https://docs.microsoft.com/dotnet/api/system.array?view=netcore-3.1
        /// </remarks>
        internal const int MaxArrayLength = 2147483591;

        /// <summary>
        /// Default block size, in bytes.
        /// </summary>
        public const int DefaultBlockSize = 131072;

        /// <summary>
        /// Default large buffer multiple, in bytes.
        /// </summary>
        public const int DefaultLargeBufferMultiple = 1048576;

        /// <summary>
        /// Default maximum buffer size, in bytes.
        /// </summary>
        public const int DefaultMaximumBufferSize = 134217728;

        private const long DefaultMaxSmallPoolFreeBytes = 0L;

        private const long DefaultMaxLargePoolFreeBytes = 0L;

        private readonly long[] largeBufferFreeSize;

        private readonly long[] largeBufferInUseSize;

        private readonly ConcurrentStack<byte[]>[] largePools;

        private readonly ConcurrentStack<byte[]> smallPool;

        private long smallPoolFreeSize;

        private long smallPoolInUseSize;

        /// <summary>
        /// The size of each block. It must be set at creation and cannot be changed.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// All buffers are multiples/exponentials of this number. It must be set at creation and cannot be changed.
        /// </summary>
        public int LargeBufferMultiple { get; }

        /// <summary>
        /// Use multiple large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseMultipleLargeBuffer => !UseExponentialLargeBuffer;

        /// <summary>
        /// Use exponential large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseExponentialLargeBuffer { get; }

        /// <summary>
        /// Gets the maximum buffer size.
        /// </summary>
        /// <remarks>Any buffer that is returned to the pool that is larger than this will be
        /// discarded and garbage collected.</remarks>
        public int MaximumBufferSize { get; }

        /// <summary>
        /// Number of bytes in small pool not currently in use.
        /// </summary>
        public long SmallPoolFreeSize => smallPoolFreeSize;

        /// <summary>
        /// Number of bytes currently in use by stream from the small pool.
        /// </summary>
        public long SmallPoolInUseSize => smallPoolInUseSize;

        /// <summary>
        /// Number of bytes in large pool not currently in use.
        /// </summary>
        public long LargePoolFreeSize
        {
            get
            {
                long num = 0L;
                long[] array = largeBufferFreeSize;
                foreach (long num2 in array)
                {
                    num += num2;
                }
                return num;
            }
        }

        /// <summary>
        /// Number of bytes currently in use by streams from the large pool.
        /// </summary>
        public long LargePoolInUseSize
        {
            get
            {
                long num = 0L;
                long[] array = largeBufferInUseSize;
                foreach (long num2 in array)
                {
                    num += num2;
                }
                return num;
            }
        }

        /// <summary>
        /// How many blocks are in the small pool.
        /// </summary>
        public long SmallBlocksFree => smallPool.Count;

        /// <summary>
        /// How many buffers are in the large pool.
        /// </summary>
        public long LargeBuffersFree
        {
            get
            {
                long num = 0L;
                ConcurrentStack<byte[]>[] array = largePools;
                foreach (ConcurrentStack<byte[]> concurrentStack in array)
                {
                    num += concurrentStack.Count;
                }
                return num;
            }
        }

        /// <summary>
        /// How many bytes of small free blocks to allow before we start dropping
        /// those returned to us.
        /// </summary>
        /// <remarks>The default value is 0, meaning the pool is unbounded.</remarks>
        public long MaximumFreeSmallPoolBytes { get; set; }

        /// <summary>
        /// How many bytes of large free buffers to allow before we start dropping
        /// those returned to us.
        /// </summary>
        /// <remarks>The default value is 0, meaning the pool is unbounded.</remarks>
        public long MaximumFreeLargePoolBytes { get; set; }

        /// <summary>
        /// Maximum stream capacity in bytes. Attempts to set a larger capacity will
        /// result in an exception.
        /// </summary>
        /// <remarks>A value of 0 indicates no limit.</remarks>
        public long MaximumStreamCapacity { get; set; }

        /// <summary>
        /// Whether to save callstacks for stream allocations. This can help in debugging.
        /// It should NEVER be turned on generally in production.
        /// </summary>
        public bool GenerateCallStacks { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> is called on a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
        /// to the buffer pool immediately.
        /// </para>
        /// <para>
        /// Note when enabling this setting that the user is responsible for ensuring that any buffer previously
        /// retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
        /// be valid).
        /// </para>
        /// </remarks>
        public bool AggressiveBufferReturn { get; set; }

        /// <summary>
        /// Causes an exception to be thrown if <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" /> is ever called.
        /// </summary>
        /// <remarks>Calling <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" /> defeats the purpose of a pooled buffer. Use this property to discover code that is calling <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" />. If this is
        /// set and <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" /> is called, a <c>NotSupportedException</c> will be thrown.</remarks>
        public bool ThrowExceptionOnToArray { get; set; }

        /// <summary>
        /// Triggered when a new block is created.
        /// </summary>
        public event EventHandler<BlockCreatedEventArgs> BlockCreated;

        /// <summary>
        /// Triggered when a new large buffer is created.
        /// </summary>
        public event EventHandler<LargeBufferCreatedEventArgs> LargeBufferCreated;

        /// <summary>
        /// Triggered when a new stream is created.
        /// </summary>
        public event EventHandler<StreamCreatedEventArgs> StreamCreated;

        /// <summary>
        /// Triggered when a stream is disposed.
        /// </summary>
        public event EventHandler<StreamDisposedEventArgs> StreamDisposed;

        /// <summary>
        /// Triggered when a stream is disposed of twice (an error).
        /// </summary>
        public event EventHandler<StreamDoubleDisposedEventArgs> StreamDoubleDisposed;

        /// <summary>
        /// Triggered when a stream is finalized.
        /// </summary>
        public event EventHandler<StreamFinalizedEventArgs> StreamFinalized;

        /// <summary>
        /// Triggered when a stream is disposed to report the stream's length.
        /// </summary>
        public event EventHandler<StreamLengthEventArgs> StreamLength;

        /// <summary>
        /// Triggered when a user converts a stream to array.
        /// </summary>
        public event EventHandler<StreamConvertedToArrayEventArgs> StreamConvertedToArray;

        /// <summary>
        /// Triggered when a stream is requested to expand beyond the maximum length specified by the responsible RecyclableMemoryStreamManager.
        /// </summary>
        public event EventHandler<StreamOverCapacityEventArgs> StreamOverCapacity;

        /// <summary>
        /// Triggered when a buffer of either type is discarded, along with the reason for the discard.
        /// </summary>
        public event EventHandler<BufferDiscardedEventArgs> BufferDiscarded;

        /// <summary>
        /// Periodically triggered to report usage statistics.
        /// </summary>
        public event EventHandler<UsageReportEventArgs> UsageReport;

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications. This pool may have unbounded growth unless you modify <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeSmallPoolBytes" /> and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeLargePoolBytes" />.
        /// </summary>
        public RecyclableMemoryStreamManager()
            : this(131072, 1048576, 134217728, useExponentialLargeBuffer: false, 0L, 0L)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications and maximum free bytes specifications.
        /// </summary>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maximumSmallPoolFreeBytes" /> is negative, or <paramref name="maximumLargePoolFreeBytes" /> is negative.</exception>
        public RecyclableMemoryStreamManager(long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
            : this(131072, 1048576, 134217728, useExponentialLargeBuffer: false, maximumSmallPoolFreeBytes, maximumLargePoolFreeBytes)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize. This pool may have unbounded growth unless you modify <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeSmallPoolBytes" /> and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeLargePoolBytes" />.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, useExponentialLargeBuffer: false, 0L, 0L)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />,
        /// or <paramref name="maximumSmallPoolFreeBytes" /> is negative,
        /// or <paramref name="maximumLargePoolFreeBytes" /> is negative.
        /// </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, useExponentialLargeBuffer: false, maximumSmallPoolFreeBytes, maximumLargePoolFreeBytes)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize. This pool may have unbounded growth unless you modify <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeSmallPoolBytes" /> and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeLargePoolBytes" />.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple/exponential of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, useExponentialLargeBuffer, 0L, 0L)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled.</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy.</param>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection.</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />,
        /// or <paramref name="maximumSmallPoolFreeBytes" /> is negative,
        /// or <paramref name="maximumLargePoolFreeBytes" /> is negative.
        /// </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple/exponential of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException("blockSize", blockSize, "blockSize must be a positive number");
            }
            if (largeBufferMultiple <= 0)
            {
                throw new ArgumentOutOfRangeException("largeBufferMultiple", "largeBufferMultiple must be a positive number");
            }
            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentOutOfRangeException("maximumBufferSize", "maximumBufferSize must be at least blockSize");
            }
            if (maximumSmallPoolFreeBytes < 0)
            {
                throw new ArgumentOutOfRangeException("maximumSmallPoolFreeBytes", "maximumSmallPoolFreeBytes must be non-negative");
            }
            if (maximumLargePoolFreeBytes < 0)
            {
                throw new ArgumentOutOfRangeException("maximumLargePoolFreeBytes", "maximumLargePoolFreeBytes must be non-negative");
            }
            BlockSize = blockSize;
            LargeBufferMultiple = largeBufferMultiple;
            MaximumBufferSize = maximumBufferSize;
            UseExponentialLargeBuffer = useExponentialLargeBuffer;
            MaximumFreeSmallPoolBytes = maximumSmallPoolFreeBytes;
            MaximumFreeLargePoolBytes = maximumLargePoolFreeBytes;
            if (!IsLargeBufferSize(maximumBufferSize))
            {
                throw new ArgumentException("maximumBufferSize is not " + (UseExponentialLargeBuffer ? "an exponential" : "a multiple") + " of largeBufferMultiple.", "maximumBufferSize");
            }
            smallPool = new ConcurrentStack<byte[]>();
            int num = (useExponentialLargeBuffer ? ((int)Math.Log(maximumBufferSize / largeBufferMultiple, 2.0) + 1) : (maximumBufferSize / largeBufferMultiple));
            largeBufferInUseSize = new long[num + 1];
            largeBufferFreeSize = new long[num];
            largePools = new ConcurrentStack<byte[]>[num];
            for (int i = 0; i < largePools.Length; i++)
            {
                largePools[i] = new ConcurrentStack<byte[]>();
            }
            Events.Writer.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
        }

        /// <summary>
        /// Removes and returns a single block from the pool.
        /// </summary>
        /// <returns>A <c>byte[]</c> array.</returns>
        internal byte[] GetBlock()
        {
            Interlocked.Add(ref smallPoolInUseSize, BlockSize);
            if (!smallPool.TryPop(out var result))
            {
                result = new byte[BlockSize];
                ReportBlockCreated();
            }
            else
            {
                Interlocked.Add(ref smallPoolFreeSize, -BlockSize);
            }
            return result;
        }

        /// <summary>
        /// Returns a buffer of arbitrary size from the large buffer pool. This buffer
        /// will be at least the requiredSize and always be a multiple/exponential of largeBufferMultiple.
        /// </summary>
        /// <param name="requiredSize">The minimum length of the buffer.</param>
        /// <param name="id">Unique ID for the stream.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <returns>A buffer of at least the required size.</returns>
        /// <exception cref="T:System.OutOfMemoryException">Requested array size is larger than the maximum allowed.</exception>
        internal byte[] GetLargeBuffer(long requiredSize, Guid id, string tag)
        {
            if (requiredSize > 2147483591)
            {
                throw new OutOfMemoryException($"Requested size exceeds maximum array length of {2147483591}.");
            }
            requiredSize = RoundToLargeBufferSize(requiredSize);
            int num = GetPoolIndex(requiredSize);
            bool flag = false;
            bool pooled = true;
            string callStack = null;
            byte[] result;
            if (num < largePools.Length)
            {
                if (!largePools[num].TryPop(out result))
                {
                    result = AllocateArray(requiredSize);
                    flag = true;
                }
                else
                {
                    Interlocked.Add(ref largeBufferFreeSize[num], -result.Length);
                }
            }
            else
            {
                num = largeBufferInUseSize.Length - 1;
                result = AllocateArray(requiredSize);
                if (GenerateCallStacks)
                {
                    callStack = Environment.StackTrace;
                }
                flag = true;
                pooled = false;
            }
            Interlocked.Add(ref largeBufferInUseSize[num], result.Length);
            if (flag)
            {
                ReportLargeBufferCreated(id, tag, requiredSize, pooled, callStack);
            }
            return result;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static byte[] AllocateArray(long requiredSize)
            {
                return new byte[requiredSize];
            }
        }

        private long RoundToLargeBufferSize(long requiredSize)
        {
            if (UseExponentialLargeBuffer)
            {
                long num = 1L;
                while (LargeBufferMultiple * num < requiredSize)
                {
                    num <<= 1;
                }
                return LargeBufferMultiple * num;
            }
            return (requiredSize + LargeBufferMultiple - 1) / LargeBufferMultiple * LargeBufferMultiple;
        }

        private bool IsLargeBufferSize(int value)
        {
            if (value != 0)
            {
                if (!UseExponentialLargeBuffer)
                {
                    return value % LargeBufferMultiple == 0;
                }
                return value == RoundToLargeBufferSize(value);
            }
            return false;
        }

        private int GetPoolIndex(long length)
        {
            if (UseExponentialLargeBuffer)
            {
                int i;
                for (i = 0; LargeBufferMultiple << i < length; i++)
                {
                }
                return i;
            }
            return (int)(length / LargeBufferMultiple - 1);
        }

        /// <summary>
        /// Returns the buffer to the large pool.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <param name="id">Unique stream ID.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><c>buffer.Length</c> is not a multiple/exponential of <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.LargeBufferMultiple" /> (it did not originate from this pool).</exception>
        internal void ReturnLargeBuffer(byte[] buffer, Guid id, string tag)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (!IsLargeBufferSize(buffer.Length))
            {
                throw new ArgumentException("buffer did not originate from this memory manager. The size is not " + string.Format("{0} of {1}.", UseExponentialLargeBuffer ? "an exponential" : "a multiple", LargeBufferMultiple));
            }
            int num = GetPoolIndex(buffer.Length);
            if (num < largePools.Length)
            {
                if ((largePools[num].Count + 1) * buffer.Length <= MaximumFreeLargePoolBytes || MaximumFreeLargePoolBytes == 0L)
                {
                    largePools[num].Push(buffer);
                    Interlocked.Add(ref largeBufferFreeSize[num], buffer.Length);
                }
                else
                {
                    ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Large, Events.MemoryStreamDiscardReason.EnoughFree);
                }
            }
            else
            {
                num = largeBufferInUseSize.Length - 1;
                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Large, Events.MemoryStreamDiscardReason.TooLarge);
            }
            Interlocked.Add(ref largeBufferInUseSize[num], -buffer.Length);
        }

        /// <summary>
        /// Returns the blocks to the pool.
        /// </summary>
        /// <param name="blocks">Collection of blocks to return to the pool.</param>
        /// <param name="id">Unique Stream ID.</param>
        /// <param name="tag">The tag of the stream returning these blocks, for logging if necessary.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="blocks" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="blocks" /> contains buffers that are the wrong size (or null) for this memory manager.</exception>
        internal void ReturnBlocks(List<byte[]> blocks, Guid id, string tag)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException("blocks");
            }
            long num = (long)blocks.Count * (long)BlockSize;
            Interlocked.Add(ref smallPoolInUseSize, -num);
            foreach (byte[] block in blocks)
            {
                if (block == null || block.Length != BlockSize)
                {
                    throw new ArgumentException("blocks contains buffers that are not BlockSize in length.", "blocks");
                }
            }
            foreach (byte[] block2 in blocks)
            {
                if (MaximumFreeSmallPoolBytes == 0L || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
                {
                    Interlocked.Add(ref smallPoolFreeSize, BlockSize);
                    smallPool.Push(block2);
                    continue;
                }
                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Small, Events.MemoryStreamDiscardReason.EnoughFree);
                break;
            }
        }

        /// <summary>
        /// Returns a block to the pool.
        /// </summary>
        /// <param name="block">Block to return to the pool.</param>
        /// <param name="id">Unique Stream ID.</param>
        /// <param name="tag">The tag of the stream returning this, for logging if necessary.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="block" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="block" /> is the wrong size for this memory manager.</exception>
        internal void ReturnBlock(byte[] block, Guid id, string tag)
        {
            int blockSize = BlockSize;
            Interlocked.Add(ref smallPoolInUseSize, -blockSize);
            if (block == null)
            {
                throw new ArgumentNullException("block");
            }
            if (block.Length != BlockSize)
            {
                throw new ArgumentException("block is not not BlockSize in length.");
            }
            if (MaximumFreeSmallPoolBytes == 0L || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
            {
                Interlocked.Add(ref smallPoolFreeSize, BlockSize);
                smallPool.Push(block);
            }
            else
            {
                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Small, Events.MemoryStreamDiscardReason.EnoughFree);
            }
        }

        internal void ReportBlockCreated()
        {
            Events.Writer.MemoryStreamNewBlockCreated(smallPoolInUseSize);
            this.BlockCreated?.Invoke(this, new BlockCreatedEventArgs(smallPoolInUseSize));
        }

        internal void ReportLargeBufferCreated(Guid id, string tag, long requiredSize, bool pooled, string callStack)
        {
            if (pooled)
            {
                Events.Writer.MemoryStreamNewLargeBufferCreated(requiredSize, LargePoolInUseSize);
            }
            else
            {
                Events.Writer.MemoryStreamNonPooledLargeBufferCreated(id, tag, requiredSize, callStack);
            }
            this.LargeBufferCreated?.Invoke(this, new LargeBufferCreatedEventArgs(id, tag, requiredSize, LargePoolInUseSize, pooled, callStack));
        }

        internal void ReportBufferDiscarded(Guid id, string tag, Events.MemoryStreamBufferType bufferType, Events.MemoryStreamDiscardReason reason)
        {
            Events.Writer.MemoryStreamDiscardBuffer(id, tag, bufferType, reason, SmallBlocksFree, smallPoolFreeSize, smallPoolInUseSize, LargeBuffersFree, LargePoolFreeSize, LargePoolInUseSize);
            this.BufferDiscarded?.Invoke(this, new BufferDiscardedEventArgs(id, tag, bufferType, reason));
        }

        internal void ReportStreamCreated(Guid id, string tag, long requestedSize, long actualSize)
        {
            Events.Writer.MemoryStreamCreated(id, tag, requestedSize, actualSize);
            this.StreamCreated?.Invoke(this, new StreamCreatedEventArgs(id, tag, requestedSize, actualSize));
        }

        internal void ReportStreamDisposed(Guid id, string tag, TimeSpan lifetime, string allocationStack, string disposeStack)
        {
            Events.Writer.MemoryStreamDisposed(id, tag, (long)lifetime.TotalMilliseconds, allocationStack, disposeStack);
            this.StreamDisposed?.Invoke(this, new StreamDisposedEventArgs(id, tag, lifetime, allocationStack, disposeStack));
        }

        internal void ReportStreamDoubleDisposed(Guid id, string tag, string allocationStack, string disposeStack1, string disposeStack2)
        {
            Events.Writer.MemoryStreamDoubleDispose(id, tag, allocationStack, disposeStack1, disposeStack2);
            this.StreamDoubleDisposed?.Invoke(this, new StreamDoubleDisposedEventArgs(id, tag, allocationStack, disposeStack1, disposeStack2));
        }

        internal void ReportStreamFinalized(Guid id, string tag, string allocationStack)
        {
            Events.Writer.MemoryStreamFinalized(id, tag, allocationStack);
            this.StreamFinalized?.Invoke(this, new StreamFinalizedEventArgs(id, tag, allocationStack));
        }

        internal void ReportStreamLength(long bytes)
        {
            this.StreamLength?.Invoke(this, new StreamLengthEventArgs(bytes));
        }

        internal void ReportStreamToArray(Guid id, string tag, string stack, long length)
        {
            Events.Writer.MemoryStreamToArray(id, tag, stack, length);
            this.StreamConvertedToArray?.Invoke(this, new StreamConvertedToArrayEventArgs(id, tag, stack, length));
        }

        internal void ReportStreamOverCapacity(Guid id, string tag, long requestedCapacity, string allocationStack)
        {
            Events.Writer.MemoryStreamOverCapacity(id, tag, requestedCapacity, MaximumStreamCapacity, allocationStack);
            this.StreamOverCapacity?.Invoke(this, new StreamOverCapacityEventArgs(id, tag, requestedCapacity, MaximumStreamCapacity, allocationStack));
        }

        internal void ReportUsageReport()
        {
            this.UsageReport?.Invoke(this, new UsageReportEventArgs(smallPoolInUseSize, smallPoolFreeSize, LargePoolInUseSize, LargePoolFreeSize));
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with no tag and a default initial capacity.
        /// </summary>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream()
        {
            return new RecyclableMemoryStream(this);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with no tag and a default initial capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id)
        {
            return new RecyclableMemoryStream(this, id);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag)
        {
            return new RecyclableMemoryStream(this, id, tag);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, id, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, long requiredSize)
        {
            return new RecyclableMemoryStream(this, id, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a <c>MemoryStream</c> which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, int requiredSize, bool asContiguousBuffer)
        {
            return GetStream(id, tag, (long)requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a <c>MemoryStream</c> which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, long requiredSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || requiredSize <= BlockSize)
            {
                return GetStream(id, tag, requiredSize);
            }
            return new RecyclableMemoryStream(this, id, tag, requiredSize, GetLargeBuffer(requiredSize, id, tag));
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
        {
            return GetStream(tag, (long)requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, long requiredSize, bool asContiguousBuffer)
        {
            return GetStream(Guid.NewGuid(), tag, requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, byte[] buffer, int offset, int count)
        {
            RecyclableMemoryStream recyclableMemoryStream = null;
            try
            {
                recyclableMemoryStream = new RecyclableMemoryStream(this, id, tag, count);
                recyclableMemoryStream.Write(buffer, offset, count);
                recyclableMemoryStream.Position = 0L;
                return recyclableMemoryStream;
            }
            catch
            {
                recyclableMemoryStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(byte[] buffer)
        {
            return GetStream(null, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
        {
            return GetStream(Guid.NewGuid(), tag, buffer, offset, count);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(Guid id, string tag, Memory<byte> buffer)
        {
            RecyclableMemoryStream recyclableMemoryStream = null;
            try
            {
                recyclableMemoryStream = new RecyclableMemoryStream(this, id, tag, buffer.Length);
                recyclableMemoryStream.Write(buffer.Span);
                recyclableMemoryStream.Position = 0L;
                return recyclableMemoryStream;
            }
            catch
            {
                recyclableMemoryStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, ReadOnlySpan<byte> buffer)
        {
            RecyclableMemoryStream recyclableMemoryStream = null;
            try
            {
                recyclableMemoryStream = new RecyclableMemoryStream(this, id, tag, buffer.Length);
                recyclableMemoryStream.Write(buffer);
                recyclableMemoryStream.Position = 0L;
                return recyclableMemoryStream;
            }
            catch
            {
                recyclableMemoryStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(Memory<byte> buffer)
        {
            return GetStream(null, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(ReadOnlySpan<byte> buffer)
        {
            return GetStream(null, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(string tag, Memory<byte> buffer)
        {
            return GetStream(Guid.NewGuid(), tag, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, ReadOnlySpan<byte> buffer)
        {
            return GetStream(Guid.NewGuid(), tag, buffer);
        }
    }

}

namespace Microsoft.IO
{
    /// <summary>
	/// MemoryStream implementation that deals with pooling and managing memory streams which use potentially large
	/// buffers.
	/// </summary>
	/// <remarks>
	/// This class works in tandem with the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager" /> to supply <c>MemoryStream</c>-derived
	/// objects to callers, while avoiding these specific problems:
	/// <list type="number">
	/// <item>
	/// <term>LOH allocations</term>
	/// <description>Since all large buffers are pooled, they will never incur a Gen2 GC</description>
	/// </item>
	/// <item>
	/// <term>Memory waste</term><description>A standard memory stream doubles its size when it runs out of room. This
	/// leads to continual memory growth as each stream approaches the maximum allowed size.</description>
	/// </item>
	/// <item>
	/// <term>Memory copying</term>
	/// <description>Each time a <c>MemoryStream</c> grows, all the bytes are copied into new buffers.
	/// This implementation only copies the bytes when <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> is called.</description>
	/// </item>
	/// <item>
	/// <term>Memory fragmentation</term>
	/// <description>By using homogeneous buffer sizes, it ensures that blocks of memory
	/// can be easily reused.
	/// </description>
	/// </item>
	/// </list>
	/// <para>
	/// The stream is implemented on top of a series of uniformly-sized blocks. As the stream's length grows,
	/// additional blocks are retrieved from the memory manager. It is these blocks that are pooled, not the stream
	/// object itself.
	/// </para>
	/// <para>
	/// The biggest wrinkle in this implementation is when <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> is called. This requires a single
	/// contiguous buffer. If only a single block is in use, then that block is returned. If multiple blocks
	/// are in use, we retrieve a larger buffer from the memory manager. These large buffers are also pooled,
	/// split by size--they are multiples/exponentials of a chunk size (1 MB by default).
	/// </para>
	/// <para>
	/// Once a large buffer is assigned to the stream the small blocks are NEVER again used for this stream. All operations take place on the
	/// large buffer. The large buffer can be replaced by a larger buffer from the pool as needed. All blocks and large buffers
	/// are maintained in the stream until the stream is disposed (unless AggressiveBufferReturn is enabled in the stream manager).
	/// </para>
	/// <para>
	/// A further wrinkle is what happens when the stream is longer than the maximum allowable array length under .NET. This is allowed
	/// when only blocks are in use, and only the Read/Write APIs are used. Once a stream grows to this size, any attempt to convert it
	/// to a single buffer will result in an exception. Similarly, if a stream is already converted to use a single larger buffer, then
	/// it cannot grow beyond the limits of the maximum allowable array size.
	/// </para>
	/// <para>
	/// Any method that modifies the stream has the potential to throw an <c>OutOfMemoryException</c>, either because
	/// the stream is beyond the limits set in <c>RecyclableStreamManager</c>, or it would result in a buffer larger than
	/// the maximum array size supported by .NET.
	/// </para>
	/// </remarks>
	public sealed class RecyclableMemoryStream : MemoryStream, IBufferWriter<byte>
    {
        private sealed class BlockSegment : ReadOnlySequenceSegment<byte>
        {
            public BlockSegment(Memory<byte> memory)
            {
                base.Memory = memory;
            }

            public BlockSegment Append(Memory<byte> memory)
            {
                return (BlockSegment)(base.Next = new BlockSegment(memory)
                {
                    RunningIndex = base.RunningIndex + base.Memory.Length
                });
            }
        }

        private struct BlockAndOffset
        {
            public int Block;

            public int Offset;

            public BlockAndOffset(int block, int offset)
            {
                Block = block;
                Offset = offset;
            }
        }

        private static readonly byte[] emptyArray = new byte[0];

        /// <summary>
        /// All of these blocks must be the same size.
        /// </summary>
        private readonly List<byte[]> blocks;

        private readonly Guid id;

        private readonly RecyclableMemoryStreamManager memoryManager;

        private readonly string tag;

        private readonly long creationTimestamp;

        /// <summary>
        /// This list is used to store buffers once they're replaced by something larger.
        /// This is for the cases where you have users of this class that may hold onto the buffers longer
        /// than they should and you want to prevent race conditions which could corrupt the data.
        /// </summary>
        private List<byte[]> dirtyBuffers;

        private bool disposed;

        /// <summary>
        /// This is only set by GetBuffer() if the necessary buffer is larger than a single block size, or on
        /// construction if the caller immediately requests a single large buffer.
        /// </summary>
        /// <remarks>If this field is non-null, it contains the concatenation of the bytes found in the individual
        /// blocks. Once it is created, this (or a larger) largeBuffer will be used for the life of the stream.
        /// </remarks>
        private byte[] largeBuffer;

        private long length;

        private long position;

        private byte[] bufferWriterTempBuffer;

        /// <summary>
        /// Unique identifier for this stream across its entire lifetime.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        internal Guid Id
        {
            get
            {
                CheckDisposed();
                return id;
            }
        }

        /// <summary>
        /// A temporary identifier for the current usage of this stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        internal string Tag
        {
            get
            {
                CheckDisposed();
                return tag;
            }
        }

        /// <summary>
        /// Gets the memory manager being used by this stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        internal RecyclableMemoryStreamManager MemoryManager
        {
            get
            {
                CheckDisposed();
                return memoryManager;
            }
        }

        /// <summary>
        /// Callstack of the constructor. It is only set if <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.GenerateCallStacks" /> is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string AllocationStack { get; }

        /// <summary>
        /// Callstack of the <see cref="M:Microsoft.IO.RecyclableMemoryStream.Dispose(System.Boolean)" /> call. It is only set if <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.GenerateCallStacks" /> is true,
        /// which should only be in debugging situations.
        /// </summary>
        internal string DisposeStack { get; private set; }

        /// <summary>
        /// Gets or sets the capacity.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Capacity is always in multiples of the memory manager's block size, unless
        /// the large buffer is in use. Capacity never decreases during a stream's lifetime.
        /// Explicitly setting the capacity to a lower value than the current value will have no effect.
        /// This is because the buffers are all pooled by chunks and there's little reason to
        /// allow stream truncation.
        /// </para>
        /// <para>
        /// Writing past the current capacity will cause <see cref="P:Microsoft.IO.RecyclableMemoryStream.Capacity" /> to automatically increase, until MaximumStreamCapacity is reached.
        /// </para>
        /// <para>
        /// If the capacity is larger than <c>int.MaxValue</c>, then <c>InvalidOperationException</c> will be thrown. If you anticipate using
        /// larger streams, use the <see cref="P:Microsoft.IO.RecyclableMemoryStream.Capacity64" /> property instead.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Capacity is larger than int.MaxValue.</exception>
        public override int Capacity
        {
            get
            {
                CheckDisposed();
                if (largeBuffer != null)
                {
                    return largeBuffer.Length;
                }
                long num = (long)blocks.Count * (long)memoryManager.BlockSize;
                if (num > int.MaxValue)
                {
                    throw new InvalidOperationException("Capacity is larger than int.MaxValue. Use Capacity64 instead.");
                }
                return (int)num;
            }
            set
            {
                Capacity64 = value;
            }
        }

        /// <summary>
        /// Returns a 64-bit version of capacity, for streams larger than <c>int.MaxValue</c> in length.
        /// </summary>
        public long Capacity64
        {
            get
            {
                CheckDisposed();
                if (largeBuffer != null)
                {
                    return largeBuffer.Length;
                }
                return (long)blocks.Count * (long)memoryManager.BlockSize;
            }
            set
            {
                CheckDisposed();
                EnsureCapacity(value);
            }
        }

        /// <summary>
        /// Gets the number of bytes written to this stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <remarks>If the buffer has already been converted to a large buffer, then the maximum length is limited by the maximum allowed array length in .NET.</remarks>
        public override long Length
        {
            get
            {
                CheckDisposed();
                return length;
            }
        }

        /// <summary>
        /// Gets the current position in the stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">A negative value was passed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream is in large-buffer mode, but an attempt was made to set the position past the maximum allowed array length.</exception>
        /// <remarks>If the buffer has already been converted to a large buffer, then the maximum length (and thus position) is limited by the maximum allowed array length in .NET.</remarks>
        public override long Position
        {
            get
            {
                CheckDisposed();
                return position;
            }
            set
            {
                CheckDisposed();
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value must be non-negative.");
                }
                if (largeBuffer != null && value > 2147483591)
                {
                    throw new InvalidOperationException($"Once the stream is converted to a single large buffer, position cannot be set past {2147483591}.");
                }
                position = value;
            }
        }

        /// <summary>
        /// Whether the stream can currently read.
        /// </summary>
        public override bool CanRead => !Disposed;

        /// <summary>
        /// Whether the stream can currently seek.
        /// </summary>
        public override bool CanSeek => !Disposed;

        /// <summary>
        /// Always false.
        /// </summary>
        public override bool CanTimeout => false;

        /// <summary>
        /// Whether the stream can currently write.
        /// </summary>
        public override bool CanWrite => !Disposed;

        private bool Disposed => disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
            : this(memoryManager, Guid.NewGuid(), null, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id)
            : this(memoryManager, id, null, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag)
            : this(memoryManager, Guid.NewGuid(), tag, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag)
            : this(memoryManager, id, tag, 0L, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize)
            : this(memoryManager, Guid.NewGuid(), tag, requestedSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, long requestedSize)
            : this(memoryManager, Guid.NewGuid(), tag, requestedSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, int requestedSize)
            : this(memoryManager, id, tag, (long)requestedSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, long requestedSize)
            : this(memoryManager, id, tag, requestedSize, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStream" /> class.
        /// </summary>
        /// <param name="memoryManager">The memory manager.</param>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A string identifying this stream for logging and debugging purposes.</param>
        /// <param name="requestedSize">The initial requested size to prevent future allocations.</param>
        /// <param name="initialLargeBuffer">An initial buffer to use. This buffer will be owned by the stream and returned to the memory manager upon Dispose.</param>
        internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, Guid id, string tag, long requestedSize, byte[] initialLargeBuffer)
            : base(emptyArray)
        {
            this.memoryManager = memoryManager;
            this.id = id;
            this.tag = tag;
            blocks = new List<byte[]>();
            creationTimestamp = Stopwatch.GetTimestamp();
            long num = Math.Max(requestedSize, this.memoryManager.BlockSize);
            if (initialLargeBuffer == null)
            {
                EnsureCapacity(num);
            }
            else
            {
                largeBuffer = initialLargeBuffer;
            }
            if (this.memoryManager.GenerateCallStacks)
            {
                AllocationStack = Environment.StackTrace;
            }
            this.memoryManager.ReportStreamCreated(this.id, this.tag, requestedSize, num);
            this.memoryManager.ReportUsageReport();
        }

        /// <summary>
        /// The finalizer will be called when a stream is not disposed properly.
        /// </summary>
        /// <remarks>Failing to dispose indicates a bug in the code using streams. Care should be taken to properly account for stream lifetime.</remarks>
        ~RecyclableMemoryStream()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Returns the memory used by this stream back to the pool.
        /// </summary>
        /// <param name="disposing">Whether we're disposing (true), or being called by the finalizer (false).</param>
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                string disposeStack = null;
                if (memoryManager.GenerateCallStacks)
                {
                    disposeStack = Environment.StackTrace;
                }
                memoryManager.ReportStreamDoubleDisposed(id, tag, AllocationStack, DisposeStack, disposeStack);
                return;
            }
            disposed = true;
            TimeSpan lifetime = TimeSpan.FromTicks((Stopwatch.GetTimestamp() - creationTimestamp) * 10000000 / Stopwatch.Frequency);
            if (memoryManager.GenerateCallStacks)
            {
                DisposeStack = Environment.StackTrace;
            }
            memoryManager.ReportStreamDisposed(id, tag, lifetime, AllocationStack, DisposeStack);
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            else
            {
                memoryManager.ReportStreamFinalized(id, tag, AllocationStack);
                if (AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    base.Dispose(disposing);
                    return;
                }
            }
            memoryManager.ReportStreamLength(length);
            if (largeBuffer != null)
            {
                memoryManager.ReturnLargeBuffer(largeBuffer, id, tag);
            }
            if (dirtyBuffers != null)
            {
                foreach (byte[] dirtyBuffer in dirtyBuffers)
                {
                    memoryManager.ReturnLargeBuffer(dirtyBuffer, id, tag);
                }
            }
            memoryManager.ReturnBlocks(blocks, id, tag);
            memoryManager.ReportUsageReport();
            blocks.Clear();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Equivalent to <c>Dispose</c>.
        /// </summary>
        public override void Close()
        {
            Dispose(disposing: true);
        }

        /// <summary>
        /// Returns a single buffer containing the contents of the stream.
        /// The buffer may be longer than the stream length.
        /// </summary>
        /// <returns>A byte[] buffer.</returns>
        /// <remarks>IMPORTANT: Doing a <see cref="M:Microsoft.IO.RecyclableMemoryStream.Write(System.Byte[],System.Int32,System.Int32)" /> after calling <c>GetBuffer</c> invalidates the buffer. The old buffer is held onto
        /// until <see cref="M:Microsoft.IO.RecyclableMemoryStream.Dispose(System.Boolean)" /> is called, but the next time <c>GetBuffer</c> is called, a new buffer from the pool will be required.</remarks>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.OutOfMemoryException">stream is too large for a contiguous buffer.</exception>
        public override byte[] GetBuffer()
        {
            CheckDisposed();
            if (largeBuffer != null)
            {
                return largeBuffer;
            }
            if (blocks.Count == 1)
            {
                return blocks[0];
            }
            byte[] buffer = memoryManager.GetLargeBuffer(Capacity64, id, tag);
            AssertLengthIsSmall();
            InternalRead(buffer, 0, (int)length, 0L);
            largeBuffer = buffer;
            if (blocks.Count > 0 && memoryManager.AggressiveBufferReturn)
            {
                memoryManager.ReturnBlocks(blocks, id, tag);
                blocks.Clear();
            }
            return largeBuffer;
        }

        /// <summary>Asynchronously reads all the bytes from the current position in this stream and writes them to another stream.</summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">This parameter is ignored.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous copy operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="destination" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Either the current stream or the destination stream is disposed.</exception>
        /// <exception cref="T:System.NotSupportedException">The current stream does not support reading, or the destination stream does not support writing.</exception>
        /// <remarks>Similarly to <c>MemoryStream</c>'s behavior, <c>CopyToAsync</c> will adjust the source stream's position by the number of bytes written to the destination stream, as a Read would do.</remarks>
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            CheckDisposed();
            if (length == 0L)
            {
                return Task.CompletedTask;
            }
            long num = position;
            long num2 = length - num;
            position += num2;
            if (destination is MemoryStream stream)
            {
                WriteTo(stream, num, num2);
                return Task.CompletedTask;
            }
            if (largeBuffer == null)
            {
                if (blocks.Count == 1)
                {
                    AssertLengthIsSmall();
                    return destination.WriteAsync(blocks[0], (int)num, (int)num2, cancellationToken);
                }
                return CopyToAsyncImpl(destination, GetBlockAndRelativeOffset(num), num2, blocks, cancellationToken);
            }
            AssertLengthIsSmall();
            return destination.WriteAsync(largeBuffer, (int)num, (int)num2, cancellationToken);
            static async Task CopyToAsyncImpl(Stream destination, BlockAndOffset blockAndOffset, long count, List<byte[]> blocks, CancellationToken cancellationToken)
            {
                long bytesRemaining = count;
                int currentBlock = blockAndOffset.Block;
                int num3 = blockAndOffset.Offset;
                while (bytesRemaining > 0)
                {
                    byte[] array = blocks[currentBlock];
                    int amountToCopy = (int)Math.Min(array.Length - num3, bytesRemaining);
                    await destination.WriteAsync(array, num3, amountToCopy, cancellationToken);
                    bytesRemaining -= amountToCopy;
                    int num4 = currentBlock + 1;
                    currentBlock = num4;
                    num3 = 0;
                }
            }
        }

        /// <summary>
        /// Notifies the stream that <paramref name="count" /> bytes were written to the buffer returned by <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetMemory(System.Int32)" /> or <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetSpan(System.Int32)" />.
        /// Seeks forward by <paramref name="count" /> bytes.
        /// </summary>
        /// <remarks>
        /// You must request a new buffer after calling Advance to continue writing more data and cannot write to a previously acquired buffer.
        /// </remarks>
        /// <param name="count">How many bytes to advance.</param>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.InvalidOperationException"><paramref name="count" /> is larger than the size of the previously requested buffer.</exception>
        public void Advance(int count)
        {
            CheckDisposed();
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count must be non-negative.");
            }
            byte[] array = bufferWriterTempBuffer;
            if (array != null)
            {
                if (count > array.Length)
                {
                    throw new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {array.Length}.");
                }
                Write(array, 0, count);
                ReturnTempBuffer(array);
                bufferWriterTempBuffer = null;
            }
            else
            {
                long num = ((largeBuffer == null) ? (memoryManager.BlockSize - GetBlockAndRelativeOffset(position).Offset) : (largeBuffer.Length - position));
                if (count > num)
                {
                    throw new InvalidOperationException($"Cannot advance past the end of the buffer, which has a size of {num}.");
                }
                position += count;
                length = Math.Max(position, length);
            }
        }

        private void ReturnTempBuffer(byte[] buffer)
        {
            if (buffer.Length == memoryManager.BlockSize)
            {
                memoryManager.ReturnBlock(buffer, id, tag);
            }
            else
            {
                memoryManager.ReturnLargeBuffer(buffer, id, tag);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// IMPORTANT: Calling Write(), GetBuffer(), TryGetBuffer(), Seek(), GetLength(), Advance(),
        /// or setting Position after calling GetMemory() invalidates the memory.
        /// </remarks>
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            return GetWritableBuffer(sizeHint);
        }

        /// <inheritdoc />
        /// <remarks>
        /// IMPORTANT: Calling Write(), GetBuffer(), TryGetBuffer(), Seek(), GetLength(), Advance(),
        /// or setting Position after calling GetSpan() invalidates the span.
        /// </remarks>
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            return GetWritableBuffer(sizeHint);
        }

        /// <summary>
        /// When callers to GetSpan() or GetMemory() request a buffer that is larger than the remaining size of the current block
        /// this method return a temp buffer. When Advance() is called, that temp buffer is then copied into the stream.
        /// </summary>
        private ArraySegment<byte> GetWritableBuffer(int sizeHint)
        {
            CheckDisposed();
            if (sizeHint < 0)
            {
                throw new ArgumentOutOfRangeException("sizeHint", "sizeHint must be non-negative.");
            }
            int num = Math.Max(sizeHint, 1);
            EnsureCapacity(position + num);
            if (bufferWriterTempBuffer != null)
            {
                ReturnTempBuffer(bufferWriterTempBuffer);
                bufferWriterTempBuffer = null;
            }
            if (largeBuffer != null)
            {
                return new ArraySegment<byte>(largeBuffer, (int)position, largeBuffer.Length - (int)position);
            }
            BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(position);
            if (MemoryManager.BlockSize - blockAndRelativeOffset.Offset >= num)
            {
                return new ArraySegment<byte>(blocks[blockAndRelativeOffset.Block], blockAndRelativeOffset.Offset, MemoryManager.BlockSize - blockAndRelativeOffset.Offset);
            }
            bufferWriterTempBuffer = ((num > memoryManager.BlockSize) ? memoryManager.GetLargeBuffer(num, id, tag) : memoryManager.GetBlock());
            return new ArraySegment<byte>(bufferWriterTempBuffer);
        }

        /// <summary>
        /// Returns a sequence containing the contents of the stream.
        /// </summary>
        /// <returns>A ReadOnlySequence of bytes.</returns>
        /// <remarks>IMPORTANT: Calling Write(), GetMemory(), GetSpan(), Dispose(), or Close() after calling GetReadOnlySequence() invalidates the sequence.</remarks>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public ReadOnlySequence<byte> GetReadOnlySequence()
        {
            CheckDisposed();
            if (largeBuffer != null)
            {
                AssertLengthIsSmall();
                return new ReadOnlySequence<byte>(largeBuffer, 0, (int)length);
            }
            if (blocks.Count == 1)
            {
                AssertLengthIsSmall();
                return new ReadOnlySequence<byte>(blocks[0], 0, (int)length);
            }
            BlockSegment blockSegment = new BlockSegment(blocks[0]);
            BlockSegment blockSegment2 = blockSegment;
            int num = 1;
            while (blockSegment2.RunningIndex + blockSegment2.Memory.Length < length)
            {
                blockSegment2 = blockSegment2.Append(blocks[num]);
                num++;
            }
            return new ReadOnlySequence<byte>(blockSegment, 0, blockSegment2, (int)(length - blockSegment2.RunningIndex));
        }

        /// <summary>
        /// Returns an <c>ArraySegment</c> that wraps a single buffer containing the contents of the stream.
        /// </summary>
        /// <param name="buffer">An <c>ArraySegment</c> containing a reference to the underlying bytes.</param>
        /// <returns>Returns <see langword="true" /> if a buffer can be returned; otherwise, <see langword="false" />.</returns>
        public override bool TryGetBuffer(out ArraySegment<byte> buffer)
        {
            CheckDisposed();
            try
            {
                if (length <= 2147483591)
                {
                    buffer = new ArraySegment<byte>(GetBuffer(), 0, (int)Length);
                    return true;
                }
            }
            catch (OutOfMemoryException)
            {
            }
            buffer = default(ArraySegment<byte>);
            return false;
        }

        /// <summary>
        /// Returns a new array with a copy of the buffer's contents. You should almost certainly be using <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> combined with the <see cref="P:Microsoft.IO.RecyclableMemoryStream.Length" /> to
        /// access the bytes in this stream. Calling <c>ToArray</c> will destroy the benefits of pooled buffers, but it is included
        /// for the sake of completeness.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.NotSupportedException">The current <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager" />object disallows <c>ToArray</c> calls.</exception>
        /// <exception cref="T:System.OutOfMemoryException">The length of the stream is too long for a contiguous array.</exception>
        [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided." , true)]
        public override byte[] ToArray()
        {
            CheckDisposed();
            string stack = (memoryManager.GenerateCallStacks ? Environment.StackTrace : null);
            memoryManager.ReportStreamToArray(id, tag, stack, length);
            if (memoryManager.ThrowExceptionOnToArray)
            {
                throw new NotSupportedException("The underlying RecyclableMemoryStreamManager is configured to not allow calls to ToArray.");
            }
            byte[] array = new byte[Length];
            InternalRead(array, 0, (int)length, 0L);
            return array;
        }

        /// <summary>
        /// Reads from the current position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ArgumentNullException">buffer is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException">offset subtracted from the buffer length is less than count.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return SafeRead(buffer, offset, count, ref position);
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream position is beyond <c>int.MaxValue</c>.</exception>
        public int SafeRead(byte[] buffer, int offset, int count, ref int streamPosition)
        {
            long streamPosition2 = streamPosition;
            int result = SafeRead(buffer, offset, count, ref streamPosition2);
            if (streamPosition2 > int.MaxValue)
            {
                throw new InvalidOperationException("Stream position is beyond int.MaxValue. Use SafeRead(byte[], int, int, ref long) override.");
            }
            streamPosition = (int)streamPosition2;
            return result;
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
        /// <param name="count">Number of bytes to read.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> or <paramref name="count" /> is less than 0.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int SafeRead(byte[] buffer, int offset, int count, ref long streamPosition)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset cannot be negative.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count cannot be negative.");
            }
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("buffer length must be at least offset + count.");
            }
            int num = InternalRead(buffer, offset, count, streamPosition);
            streamPosition += num;
            return num;
        }

        /// <summary>
        /// Reads from the current position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int Read(Span<byte> buffer)
        {
            return SafeRead(buffer, ref position);
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream position is beyond <c>int.MaxValue</c>.</exception>
        public int SafeRead(Span<byte> buffer, ref int streamPosition)
        {
            long streamPosition2 = streamPosition;
            int result = SafeRead(buffer, ref streamPosition2);
            if (streamPosition2 > int.MaxValue)
            {
                throw new InvalidOperationException("Stream position is beyond int.MaxValue. Use SafeRead(Span<byte>, ref long) override.");
            }
            streamPosition = (int)streamPosition2;
            return result;
        }

        /// <summary>
        /// Reads from the specified position into the provided buffer.
        /// </summary>
        /// <param name="buffer">Destination buffer.</param>
        /// <param name="streamPosition">Position in the stream to start reading from.</param>
        /// <returns>The number of bytes read.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int SafeRead(Span<byte> buffer, ref long streamPosition)
        {
            CheckDisposed();
            int num = InternalRead(buffer, streamPosition);
            streamPosition += num;
            return num;
        }

        /// <summary>
        /// Writes the buffer to the stream.
        /// </summary>
        /// <param name="buffer">Source buffer.</param>
        /// <param name="offset">Start position.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException">buffer is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative.</exception>
        /// <exception cref="T:System.ArgumentException">buffer.Length - offset is not less than count.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "offset must be in the range of 0 - buffer.Length-1.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", count, "count must be non-negative.");
            }
            if (count + offset > buffer.Length)
            {
                throw new ArgumentException("count must be greater than buffer.Length - offset.");
            }
            int blockSize = memoryManager.BlockSize;
            long newCapacity = position + count;
            EnsureCapacity(newCapacity);
            if (largeBuffer == null)
            {
                int num = count;
                int num2 = 0;
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(position);
                while (num > 0)
                {
                    byte[] dst = blocks[blockAndRelativeOffset.Block];
                    int num3 = Math.Min(blockSize - blockAndRelativeOffset.Offset, num);
                    Buffer.BlockCopy(buffer, offset + num2, dst, blockAndRelativeOffset.Offset, num3);
                    num -= num3;
                    num2 += num3;
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, largeBuffer, (int)position, count);
            }
            position = newCapacity;
            length = Math.Max(position, length);
        }

        /// <summary>
        /// Writes the buffer to the stream.
        /// </summary>
        /// <param name="source">Source buffer.</param>
        /// <exception cref="T:System.ArgumentNullException">buffer is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void Write(ReadOnlySpan<byte> source)
        {
            CheckDisposed();
            int blockSize = memoryManager.BlockSize;
            long newCapacity = position + source.Length;
            EnsureCapacity(newCapacity);
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(position);
                while (source.Length > 0)
                {
                    byte[] array = blocks[blockAndRelativeOffset.Block];
                    int start = Math.Min(blockSize - blockAndRelativeOffset.Offset, source.Length);
                    source.Slice(0, start).CopyTo(array.AsSpan(blockAndRelativeOffset.Offset));
                    source = source.Slice(start);
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
            }
            else
            {
                source.CopyTo(largeBuffer.AsSpan((int)position));
            }
            position = newCapacity;
            length = Math.Max(position, length);
        }

        /// <summary>
        /// Returns a useful string for debugging. This should not normally be called in actual production code.
        /// </summary>
        public override string ToString()
        {
            if (!disposed)
            {
                return $"Id = {Id}, Tag = {Tag}, Length = {Length:N0} bytes";
            }
            return $"Disposed: Id = {id}, Tag = {tag}, Final Length: {length:N0} bytes";
        }

        /// <summary>
        /// Writes a single byte to the current position in the stream.
        /// </summary>
        /// <param name="value">byte value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void WriteByte(byte value)
        {
            CheckDisposed();
            long newCapacity = position + 1;
            if (largeBuffer == null)
            {
                int blockSize = memoryManager.BlockSize;
                long result;
                int num = (int)Math.DivRem(position, blockSize, out result);
                if (num >= blocks.Count)
                {
                    EnsureCapacity(newCapacity);
                }
                blocks[num][result] = value;
            }
            else
            {
                if (position >= largeBuffer.Length)
                {
                    EnsureCapacity(newCapacity);
                }
                largeBuffer[position] = value;
            }
            position = newCapacity;
            if (position > length)
            {
                length = position;
            }
        }

        /// <summary>
        /// Reads a single byte from the current position in the stream.
        /// </summary>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override int ReadByte()
        {
            return SafeReadByte(ref position);
        }

        /// <summary>
        /// Reads a single byte from the specified position in the stream.
        /// </summary>
        /// <param name="streamPosition">The position in the stream to read from.</param>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">Stream position is beyond <c>int.MaxValue</c>.</exception>
        public int SafeReadByte(ref int streamPosition)
        {
            long streamPosition2 = streamPosition;
            int result = SafeReadByte(ref streamPosition2);
            if (streamPosition2 > int.MaxValue)
            {
                throw new InvalidOperationException("Stream position is beyond int.MaxValue. Use SafeReadByte(ref long) override.");
            }
            streamPosition = (int)streamPosition2;
            return result;
        }

        /// <summary>
        /// Reads a single byte from the specified position in the stream.
        /// </summary>
        /// <param name="streamPosition">The position in the stream to read from.</param>
        /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public int SafeReadByte(ref long streamPosition)
        {
            CheckDisposed();
            if (streamPosition == length)
            {
                return -1;
            }
            byte result;
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(streamPosition);
                result = blocks[blockAndRelativeOffset.Block][blockAndRelativeOffset.Offset];
            }
            else
            {
                result = largeBuffer[streamPosition];
            }
            streamPosition++;
            return result;
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">value is negative or larger than <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumStreamCapacity" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void SetLength(long value)
        {
            CheckDisposed();
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", "value must be non-negative.");
            }
            EnsureCapacity(value);
            length = value;
            if (position > value)
            {
                position = value;
            }
        }

        /// <summary>
        /// Sets the position to the offset from the seek location.
        /// </summary>
        /// <param name="offset">How many bytes to move.</param>
        /// <param name="loc">From where.</param>
        /// <returns>The new position.</returns>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset" /> is larger than <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumStreamCapacity" />.</exception>
        /// <exception cref="T:System.ArgumentException">Invalid seek origin.</exception>
        /// <exception cref="T:System.IO.IOException">Attempt to set negative position.</exception>
        public override long Seek(long offset, SeekOrigin loc)
        {
            CheckDisposed();
            long num = loc switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => offset + position,
                SeekOrigin.End => offset + length,
                _ => throw new ArgumentException("Invalid seek origin.", "loc"),
            };
            if (num < 0)
            {
                throw new IOException("Seek before beginning.");
            }
            position = num;
            return position;
        }

        /// <summary>
        /// Synchronously writes this stream's bytes to the argument stream.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <remarks>Important: This does a synchronous write, which may not be desired in some situations.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public override void WriteTo(Stream stream)
        {
            WriteTo(stream, 0L, length);
        }

        /// <summary>
        /// Synchronously writes this stream's bytes, starting at offset, for count bytes, to the argument stream.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <param name="offset">Offset in source.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond  this <paramref name="stream" />'s length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(Stream stream, int offset, int count)
        {
            WriteTo(stream, (long)offset, (long)count);
        }

        /// <summary>
        /// Synchronously writes this stream's bytes, starting at offset, for count bytes, to the argument stream.
        /// </summary>
        /// <param name="stream">Destination stream.</param>
        /// <param name="offset">Offset in source.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="stream" /> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond  this <paramref name="stream" />'s length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(Stream stream, long offset, long count)
        {
            CheckDisposed();
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (offset < 0 || offset + count > length)
            {
                throw new ArgumentOutOfRangeException("offset must not be negative and offset + count must not exceed the length of the stream.", (Exception)null);
            }
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(offset);
                long num = count;
                int num2 = blockAndRelativeOffset.Block;
                int num3 = blockAndRelativeOffset.Offset;
                while (num > 0)
                {
                    byte[] array = blocks[num2];
                    int num4 = (int)Math.Min((long)array.Length - (long)num3, num);
                    stream.Write(array, num3, num4);
                    num -= num4;
                    num2++;
                    num3 = 0;
                }
            }
            else
            {
                stream.Write(largeBuffer, (int)offset, (int)count);
            }
        }

        /// <summary>
        /// Writes bytes from the current stream to a destination <c>byte</c> array.
        /// </summary>
        /// <param name="buffer">Target buffer.</param>
        /// <remarks>The entire stream is written to the target array.</remarks>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" />&gt; is null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(byte[] buffer)
        {
            WriteTo(buffer, 0L, Length);
        }

        /// <summary>
        /// Writes bytes from the current stream to a destination <c>byte</c> array.
        /// </summary>
        /// <param name="buffer">Target buffer.</param>
        /// <param name="offset">Offset in the source stream, from which to start.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" />&gt; is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond this stream's length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(byte[] buffer, long offset, long count)
        {
            WriteTo(buffer, offset, count, 0);
        }

        /// <summary>
        /// Writes bytes from the current stream to a destination <c>byte</c> array.
        /// </summary>
        /// <param name="buffer">Target buffer.</param>
        /// <param name="offset">Offset in the source stream, from which to start.</param>
        /// <param name="count">Number of bytes to write.</param>
        /// <param name="targetOffset">Offset in the target byte array to start writing</param>
        /// <exception cref="T:System.ArgumentNullException"><c>buffer</c> is null</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> is less than 0, or <paramref name="offset" /> + <paramref name="count" /> is beyond this stream's length.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="targetOffset" /> is less than 0, or <paramref name="targetOffset" /> + <paramref name="count" /> is beyond the target <paramref name="buffer" />'s length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">Object has been disposed.</exception>
        public void WriteTo(byte[] buffer, long offset, long count, int targetOffset)
        {
            CheckDisposed();
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset + count > length)
            {
                throw new ArgumentOutOfRangeException("offset must not be negative and offset + count must not exceed the length of the stream.", (Exception)null);
            }
            if (targetOffset < 0 || count + targetOffset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("targetOffset must not be negative and targetOffset + count must not exceed the length of the target buffer.", (Exception)null);
            }
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(offset);
                long num = count;
                int num2 = blockAndRelativeOffset.Block;
                int num3 = blockAndRelativeOffset.Offset;
                int num4 = targetOffset;
                while (num > 0)
                {
                    byte[] array = blocks[num2];
                    int num5 = (int)Math.Min((long)array.Length - (long)num3, num);
                    Buffer.BlockCopy(array, num3, buffer, num4, num5);
                    num -= num5;
                    num2++;
                    num3 = 0;
                    num4 += num5;
                }
            }
            else
            {
                AssertLengthIsSmall();
                Buffer.BlockCopy(largeBuffer, (int)offset, buffer, targetOffset, (int)count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckDisposed()
        {
            if (Disposed)
            {
                ThrowDisposedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowDisposedException()
        {
            throw new ObjectDisposedException($"The stream with Id {id} and Tag {tag} is disposed.");
        }

        private int InternalRead(byte[] buffer, int offset, int count, long fromPosition)
        {
            if (length - fromPosition <= 0)
            {
                return 0;
            }
            int num3;
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(fromPosition);
                int num = 0;
                int num2 = (int)Math.Min(count, length - fromPosition);
                while (num2 > 0)
                {
                    byte[] array = blocks[blockAndRelativeOffset.Block];
                    num3 = Math.Min(array.Length - blockAndRelativeOffset.Offset, num2);
                    Buffer.BlockCopy(array, blockAndRelativeOffset.Offset, buffer, num + offset, num3);
                    num += num3;
                    num2 -= num3;
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
                return num;
            }
            num3 = (int)Math.Min(count, length - fromPosition);
            Buffer.BlockCopy(largeBuffer, (int)fromPosition, buffer, offset, num3);
            return num3;
        }

        private int InternalRead(Span<byte> buffer, long fromPosition)
        {
            if (length - fromPosition <= 0)
            {
                return 0;
            }
            int num3;
            if (largeBuffer == null)
            {
                BlockAndOffset blockAndRelativeOffset = GetBlockAndRelativeOffset(fromPosition);
                int num = 0;
                int num2 = (int)Math.Min(buffer.Length, length - fromPosition);
                while (num2 > 0)
                {
                    byte[] array = blocks[blockAndRelativeOffset.Block];
                    num3 = Math.Min(array.Length - blockAndRelativeOffset.Offset, num2);
                    array.AsSpan(blockAndRelativeOffset.Offset, num3).CopyTo(buffer.Slice(num));
                    num += num3;
                    num2 -= num3;
                    blockAndRelativeOffset.Block++;
                    blockAndRelativeOffset.Offset = 0;
                }
                return num;
            }
            num3 = (int)Math.Min(buffer.Length, length - fromPosition);
            largeBuffer.AsSpan((int)fromPosition, num3).CopyTo(buffer);
            return num3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BlockAndOffset GetBlockAndRelativeOffset(long offset)
        {
            int blockSize = memoryManager.BlockSize;
            long result;
            return new BlockAndOffset((int)Math.DivRem(offset, blockSize, out result), (int)result);
        }

        private void EnsureCapacity(long newCapacity)
        {
            if (newCapacity > memoryManager.MaximumStreamCapacity && memoryManager.MaximumStreamCapacity > 0)
            {
                memoryManager.ReportStreamOverCapacity(id, tag, newCapacity, AllocationStack);
                throw new OutOfMemoryException($"Requested capacity is too large: {newCapacity}. Limit is {memoryManager.MaximumStreamCapacity}.");
            }
            if (largeBuffer != null)
            {
                if (newCapacity > largeBuffer.Length)
                {
                    byte[] buffer = memoryManager.GetLargeBuffer(newCapacity, id, tag);
                    InternalRead(buffer, 0, (int)length, 0L);
                    ReleaseLargeBuffer();
                    largeBuffer = buffer;
                }
            }
            else
            {
                long num = newCapacity / memoryManager.BlockSize + 1;
                if (blocks.Capacity < num)
                {
                    blocks.Capacity = (int)num;
                }
                while (Capacity64 < newCapacity)
                {
                    blocks.Add(memoryManager.GetBlock());
                }
            }
        }

        /// <summary>
        /// Release the large buffer (either stores it for eventual release or returns it immediately).
        /// </summary>
        private void ReleaseLargeBuffer()
        {
            if (memoryManager.AggressiveBufferReturn)
            {
                memoryManager.ReturnLargeBuffer(largeBuffer, id, tag);
            }
            else
            {
                if (dirtyBuffers == null)
                {
                    dirtyBuffers = new List<byte[]>(1);
                }
                dirtyBuffers.Add(largeBuffer);
            }
            largeBuffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AssertLengthIsSmall()
        {
        }
    }

    /// <summary>
    /// Manages pools of <see cref="Microsoft.IO.RecyclableMemoryStream" /> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There are two pools managed in here. The small pool contains same-sized buffers that are handed to streams
    /// as they write more data.
    ///             </para>
    ///             <para>
    /// For scenarios that need to call <see cref="Microsoft.IO.RecyclableMemoryStream.GetBuffer" />, the large pool contains buffers of various sizes, all
    /// multiples/exponentials of <see cref="Microsoft.IO.RecyclableMemoryStreamManager.LargeBufferMultiple" /> (1 MB by default). They are split by size to avoid overly-wasteful buffer
    /// usage. There should be far fewer 8 MB buffers than 1 MB buffers, for example.
    /// </para>
    /// </remarks>
    public sealed class RecyclableMemoryStreamManager
    {
        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamCreated" /> event.
        /// </summary>
        public sealed class StreamCreatedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Requested stream size.
            /// </summary>
            public long RequestedSize { get; }

            /// <summary>
            /// Actual stream size.
            /// </summary>
            public long ActualSize { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamCreatedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="requestedSize">The requested stream size.</param>
            /// <param name="actualSize">The actual stream size.</param>
            public StreamCreatedEventArgs(Guid guid, string tag, long requestedSize, long actualSize)
            {
                Id = guid;
                Tag = tag;
                RequestedSize = requestedSize;
                ActualSize = actualSize;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamDisposed" /> event.
        /// </summary>
        public sealed class StreamDisposedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where the stream was allocated.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// Stack where stream was disposed.
            /// </summary>
            public string DisposeStack { get; }

            /// <summary>
            /// Lifetime of the stream.
            /// </summary>
            public TimeSpan Lifetime { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamDisposedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            /// <param name="disposeStack">Dispose stack.</param>
            [Obsolete("Use another constructor override")]
            public StreamDisposedEventArgs(Guid guid, string tag, string allocationStack, string disposeStack)
                : this(guid, tag, TimeSpan.Zero, allocationStack, disposeStack)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamDisposedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="lifetime">Lifetime of the stream</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            /// <param name="disposeStack">Dispose stack.</param>
            public StreamDisposedEventArgs(Guid guid, string tag, TimeSpan lifetime, string allocationStack, string disposeStack)
            {
                Id = guid;
                Tag = tag;
                Lifetime = lifetime;
                AllocationStack = allocationStack;
                DisposeStack = disposeStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamDoubleDisposed" /> event.
        /// </summary>
        public sealed class StreamDoubleDisposedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where the stream was allocated.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// First dispose stack.
            /// </summary>
            public string DisposeStack1 { get; }

            /// <summary>
            /// Second dispose stack.
            /// </summary>
            public string DisposeStack2 { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamDoubleDisposedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            /// <param name="disposeStack1">First dispose stack.</param>
            /// <param name="disposeStack2">Second dispose stack.</param>
            public StreamDoubleDisposedEventArgs(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
            {
                Id = guid;
                Tag = tag;
                AllocationStack = allocationStack;
                DisposeStack1 = disposeStack1;
                DisposeStack2 = disposeStack2;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamFinalized" /> event.
        /// </summary>
        public sealed class StreamFinalizedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where the stream was allocated.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamFinalizedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="allocationStack">Stack of original allocation.</param>
            public StreamFinalizedEventArgs(Guid guid, string tag, string allocationStack)
            {
                Id = guid;
                Tag = tag;
                AllocationStack = allocationStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamConvertedToArray" /> event.
        /// </summary>
        public sealed class StreamConvertedToArrayEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Stack where ToArray was called.
            /// </summary>
            public string Stack { get; }

            /// <summary>
            /// Length of stack.
            /// </summary>
            public long Length { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamConvertedToArrayEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="stack">Stack of ToArray call.</param>
            /// <param name="length">Length of stream.</param>
            public StreamConvertedToArrayEventArgs(Guid guid, string tag, string stack, long length)
            {
                Id = guid;
                Tag = tag;
                Stack = stack;
                Length = length;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamOverCapacity" /> event.
        /// </summary>
        public sealed class StreamOverCapacityEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Original allocation stack.
            /// </summary>
            public string AllocationStack { get; }

            /// <summary>
            /// Requested capacity.
            /// </summary>
            public long RequestedCapacity { get; }

            /// <summary>
            /// Maximum capacity.
            /// </summary>
            public long MaximumCapacity { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamOverCapacityEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="requestedCapacity">Requested capacity.</param>
            /// <param name="maximumCapacity">Maximum stream capacity of the manager.</param>
            /// <param name="allocationStack">Original allocation stack.</param>
            internal StreamOverCapacityEventArgs(Guid guid, string tag, long requestedCapacity, long maximumCapacity, string allocationStack)
            {
                Id = guid;
                Tag = tag;
                RequestedCapacity = requestedCapacity;
                MaximumCapacity = maximumCapacity;
                AllocationStack = allocationStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.BlockCreated" /> event.
        /// </summary>
        public sealed class BlockCreatedEventArgs : EventArgs
        {
            /// <summary>
            /// How many bytes are currently in use from the small pool.
            /// </summary>
            public long SmallPoolInUse { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.BlockCreatedEventArgs" /> class.
            /// </summary>
            /// <param name="smallPoolInUse">Number of bytes currently in use from the small pool.</param>
            internal BlockCreatedEventArgs(long smallPoolInUse)
            {
                SmallPoolInUse = smallPoolInUse;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.LargeBufferCreated" /> events.
        /// </summary>
        public sealed class LargeBufferCreatedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Whether the buffer was satisfied from the pool or not.
            /// </summary>
            public bool Pooled { get; }

            /// <summary>
            /// Required buffer size.
            /// </summary>
            public long RequiredSize { get; }

            /// <summary>
            /// How many bytes are in use from the large pool.
            /// </summary>
            public long LargePoolInUse { get; }

            /// <summary>
            /// If the buffer was not satisfied from the pool, and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.GenerateCallStacks" /> is turned on, then.
            /// this will contain the callstack of the allocation request.
            /// </summary>
            public string CallStack { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.LargeBufferCreatedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="requiredSize">Required size of the new buffer.</param>
            /// <param name="largePoolInUse">How many bytes from the large pool are currently in use.</param>
            /// <param name="pooled">Whether the buffer was satisfied from the pool or not.</param>
            /// <param name="callStack">Callstack of the allocation, if it wasn't pooled.</param>
            internal LargeBufferCreatedEventArgs(Guid guid, string tag, long requiredSize, long largePoolInUse, bool pooled, string callStack)
            {
                RequiredSize = requiredSize;
                LargePoolInUse = largePoolInUse;
                Pooled = pooled;
                Id = guid;
                Tag = tag;
                CallStack = callStack;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.BufferDiscarded" /> event.
        /// </summary>
        public sealed class BufferDiscardedEventArgs : EventArgs
        {
            /// <summary>
            /// Unique ID for the stream.
            /// </summary>
            public Guid Id { get; }

            /// <summary>
            /// Optional Tag for the event.
            /// </summary>
            public string Tag { get; }

            /// <summary>
            /// Type of the buffer.
            /// </summary>
            public Events.MemoryStreamBufferType BufferType { get; }

            /// <summary>
            /// The reason this buffer was discarded.
            /// </summary>
            public Events.MemoryStreamDiscardReason Reason { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.BufferDiscardedEventArgs" /> class.
            /// </summary>
            /// <param name="guid">Unique ID of the stream.</param>
            /// <param name="tag">Tag of the stream.</param>
            /// <param name="bufferType">Type of buffer being discarded.</param>
            /// <param name="reason">The reason for the discard.</param>
            internal BufferDiscardedEventArgs(Guid guid, string tag, Events.MemoryStreamBufferType bufferType, Events.MemoryStreamDiscardReason reason)
            {
                Id = guid;
                Tag = tag;
                BufferType = bufferType;
                Reason = reason;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.StreamLength" /> event.
        /// </summary>
        public sealed class StreamLengthEventArgs : EventArgs
        {
            /// <summary>
            /// Length of the stream.
            /// </summary>
            public long Length { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.StreamLengthEventArgs" /> class.
            /// </summary>
            /// <param name="length">Length of the strength.</param>
            public StreamLengthEventArgs(long length)
            {
                Length = length;
            }
        }

        /// <summary>
        /// Arguments for the <see cref="E:Microsoft.IO.RecyclableMemoryStreamManager.UsageReport" /> event.
        /// </summary>
        public sealed class UsageReportEventArgs : EventArgs
        {
            /// <summary>
            /// Bytes from the small pool currently in use.
            /// </summary>
            public long SmallPoolInUseBytes { get; }

            /// <summary>
            /// Bytes from the small pool currently available.
            /// </summary>
            public long SmallPoolFreeBytes { get; }

            /// <summary>
            /// Bytes from the large pool currently in use.
            /// </summary>
            public long LargePoolInUseBytes { get; }

            /// <summary>
            /// Bytes from the large pool currently available.
            /// </summary>
            public long LargePoolFreeBytes { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:Microsoft.IO.RecyclableMemoryStreamManager.UsageReportEventArgs" /> class.
            /// </summary>
            /// <param name="smallPoolInUseBytes">Bytes from the small pool currently in use.</param>
            /// <param name="smallPoolFreeBytes">Bytes from the small pool currently available.</param>
            /// <param name="largePoolInUseBytes">Bytes from the large pool currently in use.</param>
            /// <param name="largePoolFreeBytes">Bytes from the large pool currently available.</param>
            public UsageReportEventArgs(long smallPoolInUseBytes, long smallPoolFreeBytes, long largePoolInUseBytes, long largePoolFreeBytes)
            {
                SmallPoolInUseBytes = smallPoolInUseBytes;
                SmallPoolFreeBytes = smallPoolFreeBytes;
                LargePoolInUseBytes = largePoolInUseBytes;
                LargePoolFreeBytes = largePoolFreeBytes;
            }
        }

        /// <summary>
        /// ETW events for RecyclableMemoryStream.
        /// </summary>
        [EventSource(Name = "Microsoft-IO-RecyclableMemoryStream", Guid = "{B80CD4E4-890E-468D-9CBA-90EB7C82DFC7}")]
        public sealed class Events : EventSource
        {
            /// <summary>
            /// Type of buffer.
            /// </summary>
            public enum MemoryStreamBufferType
            {
                /// <summary>
                /// Small block buffer.
                /// </summary>
                Small,
                /// <summary>
                /// Large pool buffer.
                /// </summary>
                Large
            }

            /// <summary>
            /// The possible reasons for discarding a buffer.
            /// </summary>
            public enum MemoryStreamDiscardReason
            {
                /// <summary>
                /// Buffer was too large to be re-pooled.
                /// </summary>
                TooLarge,
                /// <summary>
                /// There are enough free bytes in the pool.
                /// </summary>
                EnoughFree
            }

            /// <summary>
            /// Static log object, through which all events are written.
            /// </summary>
            public static Events Writer = new Events();

            /// <summary>
            /// Logged when a stream object is created.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="requestedSize">Requested size of the stream.</param>
            /// <param name="actualSize">Actual size given to the stream from the pool.</param>
            [Event(1, Level = EventLevel.Verbose, Version = 2)]
            public void MemoryStreamCreated(Guid guid, string tag, long requestedSize, long actualSize)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(1, guid, tag ?? string.Empty, requestedSize, actualSize);
                }
            }

            /// <summary>
            /// Logged when the stream is disposed.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="lifetimeMs">Lifetime in milliseconds of the stream</param>
            /// <param name="allocationStack">Call stack of initial allocation.</param>
            /// <param name="disposeStack">Call stack of the dispose.</param>
            [Event(2, Level = EventLevel.Verbose, Version = 3)]
            public void MemoryStreamDisposed(Guid guid, string tag, long lifetimeMs, string allocationStack, string disposeStack)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(2, guid, tag ?? string.Empty, lifetimeMs, allocationStack ?? string.Empty, disposeStack ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when the stream is disposed for the second time.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="allocationStack">Call stack of initial allocation.</param>
            /// <param name="disposeStack1">Call stack of the first dispose.</param>
            /// <param name="disposeStack2">Call stack of the second dispose.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(3, Level = EventLevel.Critical)]
            public void MemoryStreamDoubleDispose(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
            {
                if (IsEnabled())
                {
                    WriteEvent(3, guid, tag ?? string.Empty, allocationStack ?? string.Empty, disposeStack1 ?? string.Empty, disposeStack2 ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when a stream is finalized.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="allocationStack">Call stack of initial allocation.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(4, Level = EventLevel.Error)]
            public void MemoryStreamFinalized(Guid guid, string tag, string allocationStack)
            {
                if (IsEnabled())
                {
                    WriteEvent(4, guid, tag ?? string.Empty, allocationStack ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when ToArray is called on a stream.
            /// </summary>
            /// <param name="guid">A unique ID for this stream.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="stack">Call stack of the ToArray call.</param>
            /// <param name="size">Length of stream.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(5, Level = EventLevel.Verbose, Version = 2)]
            public void MemoryStreamToArray(Guid guid, string tag, string stack, long size)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(5, guid, tag ?? string.Empty, stack ?? string.Empty, size);
                }
            }

            /// <summary>
            /// Logged when the RecyclableMemoryStreamManager is initialized.
            /// </summary>
            /// <param name="blockSize">Size of blocks, in bytes.</param>
            /// <param name="largeBufferMultiple">Size of the large buffer multiple, in bytes.</param>
            /// <param name="maximumBufferSize">Maximum buffer size, in bytes.</param>
            [Event(6, Level = EventLevel.Informational)]
            public void MemoryStreamManagerInitialized(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            {
                if (IsEnabled())
                {
                    WriteEvent(6, blockSize, largeBufferMultiple, maximumBufferSize);
                }
            }

            /// <summary>
            /// Logged when a new block is created.
            /// </summary>
            /// <param name="smallPoolInUseBytes">Number of bytes in the small pool currently in use.</param>
            [Event(7, Level = EventLevel.Warning, Version = 2)]
            public void MemoryStreamNewBlockCreated(long smallPoolInUseBytes)
            {
                if (IsEnabled(EventLevel.Warning, EventKeywords.None))
                {
                    WriteEvent(7, smallPoolInUseBytes);
                }
            }

            /// <summary>
            /// Logged when a new large buffer is created.
            /// </summary>
            /// <param name="requiredSize">Requested size.</param>
            /// <param name="largePoolInUseBytes">Number of bytes in the large pool in use.</param>
            [Event(8, Level = EventLevel.Warning, Version = 3)]
            public void MemoryStreamNewLargeBufferCreated(long requiredSize, long largePoolInUseBytes)
            {
                if (IsEnabled(EventLevel.Warning, EventKeywords.None))
                {
                    WriteEvent(8, requiredSize, largePoolInUseBytes);
                }
            }

            /// <summary>
            /// Logged when a buffer is created that is too large to pool.
            /// </summary>
            /// <param name="guid">Unique stream ID.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="requiredSize">Size requested by the caller.</param>
            /// <param name="allocationStack">Call stack of the requested stream.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(9, Level = EventLevel.Verbose, Version = 3)]
            public void MemoryStreamNonPooledLargeBufferCreated(Guid guid, string tag, long requiredSize, string allocationStack)
            {
                if (IsEnabled(EventLevel.Verbose, EventKeywords.None))
                {
                    WriteEvent(9, guid, tag ?? string.Empty, requiredSize, allocationStack ?? string.Empty);
                }
            }

            /// <summary>
            /// Logged when a buffer is discarded (not put back in the pool, but given to GC to clean up).
            /// </summary>
            /// <param name="guid">Unique stream ID.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="bufferType">Type of the buffer being discarded.</param>
            /// <param name="reason">Reason for the discard.</param>
            /// <param name="smallBlocksFree">Number of free small pool blocks.</param>
            /// <param name="smallPoolBytesFree">Bytes free in the small pool.</param>
            /// <param name="smallPoolBytesInUse">Bytes in use from the small pool.</param>
            /// <param name="largeBlocksFree">Number of free large pool blocks.</param>
            /// <param name="largePoolBytesFree">Bytes free in the large pool.</param>
            /// <param name="largePoolBytesInUse">Bytes in use from the large pool.</param>
            [Event(10, Level = EventLevel.Warning, Version = 2)]
            public void MemoryStreamDiscardBuffer(Guid guid, string tag, MemoryStreamBufferType bufferType, MemoryStreamDiscardReason reason, long smallBlocksFree, long smallPoolBytesFree, long smallPoolBytesInUse, long largeBlocksFree, long largePoolBytesFree, long largePoolBytesInUse)
            {
                if (IsEnabled(EventLevel.Warning, EventKeywords.None))
                {
                    WriteEvent(10, guid, tag ?? string.Empty, bufferType, reason, smallBlocksFree, smallPoolBytesFree, smallPoolBytesInUse, largeBlocksFree, largePoolBytesFree, largePoolBytesInUse);
                }
            }

            /// <summary>
            /// Logged when a stream grows beyond the maximum capacity.
            /// </summary>
            /// <param name="guid">Unique stream ID</param>
            /// <param name="requestedCapacity">The requested capacity.</param>
            /// <param name="maxCapacity">Maximum capacity, as configured by RecyclableMemoryStreamManager.</param>
            /// <param name="tag">A temporary ID for this stream, usually indicates current usage.</param>
            /// <param name="allocationStack">Call stack for the capacity request.</param>
            /// <remarks>Note: Stacks will only be populated if RecyclableMemoryStreamManager.GenerateCallStacks is true.</remarks>
            [Event(11, Level = EventLevel.Error, Version = 3)]
            public void MemoryStreamOverCapacity(Guid guid, string tag, long requestedCapacity, long maxCapacity, string allocationStack)
            {
                if (IsEnabled())
                {
                    WriteEvent(11, guid, tag ?? string.Empty, requestedCapacity, maxCapacity, allocationStack ?? string.Empty);
                }
            }
        }

        /// <summary>
        /// Maximum length of a single array.
        /// </summary>
        /// <remarks>See documentation at https://docs.microsoft.com/dotnet/api/system.array?view=netcore-3.1
        /// </remarks>
        internal const int MaxArrayLength = 2147483591;

        /// <summary>
        /// Default block size, in bytes.
        /// </summary>
        public const int DefaultBlockSize = 131072;

        /// <summary>
        /// Default large buffer multiple, in bytes.
        /// </summary>
        public const int DefaultLargeBufferMultiple = 1048576;

        /// <summary>
        /// Default maximum buffer size, in bytes.
        /// </summary>
        public const int DefaultMaximumBufferSize = 134217728;

        private const long DefaultMaxSmallPoolFreeBytes = 0L;

        private const long DefaultMaxLargePoolFreeBytes = 0L;

        private readonly long[] largeBufferFreeSize;

        private readonly long[] largeBufferInUseSize;

        private readonly ConcurrentStack<byte[]>[] largePools;

        private readonly ConcurrentStack<byte[]> smallPool;

        private long smallPoolFreeSize;

        private long smallPoolInUseSize;

        /// <summary>
        /// The size of each block. It must be set at creation and cannot be changed.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// All buffers are multiples/exponentials of this number. It must be set at creation and cannot be changed.
        /// </summary>
        public int LargeBufferMultiple { get; }

        /// <summary>
        /// Use multiple large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseMultipleLargeBuffer => !UseExponentialLargeBuffer;

        /// <summary>
        /// Use exponential large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseExponentialLargeBuffer { get; }

        /// <summary>
        /// Gets the maximum buffer size.
        /// </summary>
        /// <remarks>Any buffer that is returned to the pool that is larger than this will be
        /// discarded and garbage collected.</remarks>
        public int MaximumBufferSize { get; }

        /// <summary>
        /// Number of bytes in small pool not currently in use.
        /// </summary>
        public long SmallPoolFreeSize => smallPoolFreeSize;

        /// <summary>
        /// Number of bytes currently in use by stream from the small pool.
        /// </summary>
        public long SmallPoolInUseSize => smallPoolInUseSize;

        /// <summary>
        /// Number of bytes in large pool not currently in use.
        /// </summary>
        public long LargePoolFreeSize
        {
            get
            {
                long num = 0L;
                long[] array = largeBufferFreeSize;
                foreach (long num2 in array)
                {
                    num += num2;
                }
                return num;
            }
        }

        /// <summary>
        /// Number of bytes currently in use by streams from the large pool.
        /// </summary>
        public long LargePoolInUseSize
        {
            get
            {
                long num = 0L;
                long[] array = largeBufferInUseSize;
                foreach (long num2 in array)
                {
                    num += num2;
                }
                return num;
            }
        }

        /// <summary>
        /// How many blocks are in the small pool.
        /// </summary>
        public long SmallBlocksFree => smallPool.Count;

        /// <summary>
        /// How many buffers are in the large pool.
        /// </summary>
        public long LargeBuffersFree
        {
            get
            {
                long num = 0L;
                ConcurrentStack<byte[]>[] array = largePools;
                foreach (ConcurrentStack<byte[]> concurrentStack in array)
                {
                    num += concurrentStack.Count;
                }
                return num;
            }
        }

        /// <summary>
        /// How many bytes of small free blocks to allow before we start dropping
        /// those returned to us.
        /// </summary>
        /// <remarks>The default value is 0, meaning the pool is unbounded.</remarks>
        public long MaximumFreeSmallPoolBytes { get; set; }

        /// <summary>
        /// How many bytes of large free buffers to allow before we start dropping
        /// those returned to us.
        /// </summary>
        /// <remarks>The default value is 0, meaning the pool is unbounded.</remarks>
        public long MaximumFreeLargePoolBytes { get; set; }

        /// <summary>
        /// Maximum stream capacity in bytes. Attempts to set a larger capacity will
        /// result in an exception.
        /// </summary>
        /// <remarks>A value of 0 indicates no limit.</remarks>
        public long MaximumStreamCapacity { get; set; }

        /// <summary>
        /// Whether to save callstacks for stream allocations. This can help in debugging.
        /// It should NEVER be turned on generally in production.
        /// </summary>
        public bool GenerateCallStacks { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" /> is called on a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
        /// to the buffer pool immediately.
        /// </para>
        /// <para>
        /// Note when enabling this setting that the user is responsible for ensuring that any buffer previously
        /// retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
        /// be valid).
        /// </para>
        /// </remarks>
        public bool AggressiveBufferReturn { get; set; }

        /// <summary>
        /// Causes an exception to be thrown if <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" /> is ever called.
        /// </summary>
        /// <remarks>Calling <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" /> defeats the purpose of a pooled buffer. Use this property to discover code that is calling <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" />. If this is
        /// set and <see cref="M:Microsoft.IO.RecyclableMemoryStream.ToArray" /> is called, a <c>NotSupportedException</c> will be thrown.</remarks>
        public bool ThrowExceptionOnToArray { get; set; }

        /// <summary>
        /// Triggered when a new block is created.
        /// </summary>
        public event EventHandler<BlockCreatedEventArgs> BlockCreated;

        /// <summary>
        /// Triggered when a new large buffer is created.
        /// </summary>
        public event EventHandler<LargeBufferCreatedEventArgs> LargeBufferCreated;

        /// <summary>
        /// Triggered when a new stream is created.
        /// </summary>
        public event EventHandler<StreamCreatedEventArgs> StreamCreated;

        /// <summary>
        /// Triggered when a stream is disposed.
        /// </summary>
        public event EventHandler<StreamDisposedEventArgs> StreamDisposed;

        /// <summary>
        /// Triggered when a stream is disposed of twice (an error).
        /// </summary>
        public event EventHandler<StreamDoubleDisposedEventArgs> StreamDoubleDisposed;

        /// <summary>
        /// Triggered when a stream is finalized.
        /// </summary>
        public event EventHandler<StreamFinalizedEventArgs> StreamFinalized;

        /// <summary>
        /// Triggered when a stream is disposed to report the stream's length.
        /// </summary>
        public event EventHandler<StreamLengthEventArgs> StreamLength;

        /// <summary>
        /// Triggered when a user converts a stream to array.
        /// </summary>
        public event EventHandler<StreamConvertedToArrayEventArgs> StreamConvertedToArray;

        /// <summary>
        /// Triggered when a stream is requested to expand beyond the maximum length specified by the responsible RecyclableMemoryStreamManager.
        /// </summary>
        public event EventHandler<StreamOverCapacityEventArgs> StreamOverCapacity;

        /// <summary>
        /// Triggered when a buffer of either type is discarded, along with the reason for the discard.
        /// </summary>
        public event EventHandler<BufferDiscardedEventArgs> BufferDiscarded;

        /// <summary>
        /// Periodically triggered to report usage statistics.
        /// </summary>
        public event EventHandler<UsageReportEventArgs> UsageReport;

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications. This pool may have unbounded growth unless you modify <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeSmallPoolBytes" /> and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeLargePoolBytes" />.
        /// </summary>
        public RecyclableMemoryStreamManager()
            : this(131072, 1048576, 134217728, useExponentialLargeBuffer: false, 0L, 0L)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications and maximum free bytes specifications.
        /// </summary>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maximumSmallPoolFreeBytes" /> is negative, or <paramref name="maximumLargePoolFreeBytes" /> is negative.</exception>
        public RecyclableMemoryStreamManager(long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
            : this(131072, 1048576, 134217728, useExponentialLargeBuffer: false, maximumSmallPoolFreeBytes, maximumLargePoolFreeBytes)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize. This pool may have unbounded growth unless you modify <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeSmallPoolBytes" /> and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeLargePoolBytes" />.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, useExponentialLargeBuffer: false, 0L, 0L)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />,
        /// or <paramref name="maximumSmallPoolFreeBytes" /> is negative,
        /// or <paramref name="maximumLargePoolFreeBytes" /> is negative.
        /// </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, useExponentialLargeBuffer: false, maximumSmallPoolFreeBytes, maximumLargePoolFreeBytes)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize. This pool may have unbounded growth unless you modify <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeSmallPoolBytes" /> and <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.MaximumFreeLargePoolBytes" />.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple/exponential of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, useExponentialLargeBuffer, 0L, 0L)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be &gt; 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled.</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy.</param>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection.</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="blockSize" /> is not a positive number,
        /// or <paramref name="largeBufferMultiple" /> is not a positive number,
        /// or <paramref name="maximumBufferSize" /> is less than <paramref name="blockSize" />,
        /// or <paramref name="maximumSmallPoolFreeBytes" /> is negative,
        /// or <paramref name="maximumLargePoolFreeBytes" /> is negative.
        /// </exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="maximumBufferSize" /> is not a multiple/exponential of <paramref name="largeBufferMultiple" />.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException("blockSize", blockSize, "blockSize must be a positive number");
            }
            if (largeBufferMultiple <= 0)
            {
                throw new ArgumentOutOfRangeException("largeBufferMultiple", "largeBufferMultiple must be a positive number");
            }
            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentOutOfRangeException("maximumBufferSize", "maximumBufferSize must be at least blockSize");
            }
            if (maximumSmallPoolFreeBytes < 0)
            {
                throw new ArgumentOutOfRangeException("maximumSmallPoolFreeBytes", "maximumSmallPoolFreeBytes must be non-negative");
            }
            if (maximumLargePoolFreeBytes < 0)
            {
                throw new ArgumentOutOfRangeException("maximumLargePoolFreeBytes", "maximumLargePoolFreeBytes must be non-negative");
            }
            BlockSize = blockSize;
            LargeBufferMultiple = largeBufferMultiple;
            MaximumBufferSize = maximumBufferSize;
            UseExponentialLargeBuffer = useExponentialLargeBuffer;
            MaximumFreeSmallPoolBytes = maximumSmallPoolFreeBytes;
            MaximumFreeLargePoolBytes = maximumLargePoolFreeBytes;
            if (!IsLargeBufferSize(maximumBufferSize))
            {
                throw new ArgumentException("maximumBufferSize is not " + (UseExponentialLargeBuffer ? "an exponential" : "a multiple") + " of largeBufferMultiple.", "maximumBufferSize");
            }
            smallPool = new ConcurrentStack<byte[]>();
            int num = (useExponentialLargeBuffer ? ((int)Math.Log(maximumBufferSize / largeBufferMultiple, 2.0) + 1) : (maximumBufferSize / largeBufferMultiple));
            largeBufferInUseSize = new long[num + 1];
            largeBufferFreeSize = new long[num];
            largePools = new ConcurrentStack<byte[]>[num];
            for (int i = 0; i < largePools.Length; i++)
            {
                largePools[i] = new ConcurrentStack<byte[]>();
            }
            Events.Writer.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
        }

        /// <summary>
        /// Removes and returns a single block from the pool.
        /// </summary>
        /// <returns>A <c>byte[]</c> array.</returns>
        internal byte[] GetBlock()
        {
            Interlocked.Add(ref smallPoolInUseSize, BlockSize);
            if (!smallPool.TryPop(out var result))
            {
                result = new byte[BlockSize];
                ReportBlockCreated();
            }
            else
            {
                Interlocked.Add(ref smallPoolFreeSize, -BlockSize);
            }
            return result;
        }

        /// <summary>
        /// Returns a buffer of arbitrary size from the large buffer pool. This buffer
        /// will be at least the requiredSize and always be a multiple/exponential of largeBufferMultiple.
        /// </summary>
        /// <param name="requiredSize">The minimum length of the buffer.</param>
        /// <param name="id">Unique ID for the stream.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <returns>A buffer of at least the required size.</returns>
        /// <exception cref="T:System.OutOfMemoryException">Requested array size is larger than the maximum allowed.</exception>
        internal byte[] GetLargeBuffer(long requiredSize, Guid id, string tag)
        {
            if (requiredSize > 2147483591)
            {
                throw new OutOfMemoryException($"Requested size exceeds maximum array length of {2147483591}.");
            }
            requiredSize = RoundToLargeBufferSize(requiredSize);
            int num = GetPoolIndex(requiredSize);
            bool flag = false;
            bool pooled = true;
            string callStack = null;
            byte[] result;
            if (num < largePools.Length)
            {
                if (!largePools[num].TryPop(out result))
                {
                    result = AllocateArray(requiredSize);
                    flag = true;
                }
                else
                {
                    Interlocked.Add(ref largeBufferFreeSize[num], -result.Length);
                }
            }
            else
            {
                num = largeBufferInUseSize.Length - 1;
                result = AllocateArray(requiredSize);
                if (GenerateCallStacks)
                {
                    callStack = Environment.StackTrace;
                }
                flag = true;
                pooled = false;
            }
            Interlocked.Add(ref largeBufferInUseSize[num], result.Length);
            if (flag)
            {
                ReportLargeBufferCreated(id, tag, requiredSize, pooled, callStack);
            }
            return result;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static byte[] AllocateArray(long requiredSize)
            {
                return new byte[requiredSize];
            }
        }

        private long RoundToLargeBufferSize(long requiredSize)
        {
            if (UseExponentialLargeBuffer)
            {
                long num = 1L;
                while (LargeBufferMultiple * num < requiredSize)
                {
                    num <<= 1;
                }
                return LargeBufferMultiple * num;
            }
            return (requiredSize + LargeBufferMultiple - 1) / LargeBufferMultiple * LargeBufferMultiple;
        }

        private bool IsLargeBufferSize(int value)
        {
            if (value != 0)
            {
                if (!UseExponentialLargeBuffer)
                {
                    return value % LargeBufferMultiple == 0;
                }
                return value == RoundToLargeBufferSize(value);
            }
            return false;
        }

        private int GetPoolIndex(long length)
        {
            if (UseExponentialLargeBuffer)
            {
                int i;
                for (i = 0; LargeBufferMultiple << i < length; i++)
                {
                }
                return i;
            }
            return (int)(length / LargeBufferMultiple - 1);
        }

        /// <summary>
        /// Returns the buffer to the large pool.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <param name="id">Unique stream ID.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><c>buffer.Length</c> is not a multiple/exponential of <see cref="P:Microsoft.IO.RecyclableMemoryStreamManager.LargeBufferMultiple" /> (it did not originate from this pool).</exception>
        internal void ReturnLargeBuffer(byte[] buffer, Guid id, string tag)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (!IsLargeBufferSize(buffer.Length))
            {
                throw new ArgumentException("buffer did not originate from this memory manager. The size is not " + string.Format("{0} of {1}.", UseExponentialLargeBuffer ? "an exponential" : "a multiple", LargeBufferMultiple));
            }
            int num = GetPoolIndex(buffer.Length);
            if (num < largePools.Length)
            {
                if ((largePools[num].Count + 1) * buffer.Length <= MaximumFreeLargePoolBytes || MaximumFreeLargePoolBytes == 0L)
                {
                    largePools[num].Push(buffer);
                    Interlocked.Add(ref largeBufferFreeSize[num], buffer.Length);
                }
                else
                {
                    ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Large, Events.MemoryStreamDiscardReason.EnoughFree);
                }
            }
            else
            {
                num = largeBufferInUseSize.Length - 1;
                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Large, Events.MemoryStreamDiscardReason.TooLarge);
            }
            Interlocked.Add(ref largeBufferInUseSize[num], -buffer.Length);
        }

        /// <summary>
        /// Returns the blocks to the pool.
        /// </summary>
        /// <param name="blocks">Collection of blocks to return to the pool.</param>
        /// <param name="id">Unique Stream ID.</param>
        /// <param name="tag">The tag of the stream returning these blocks, for logging if necessary.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="blocks" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="blocks" /> contains buffers that are the wrong size (or null) for this memory manager.</exception>
        internal void ReturnBlocks(List<byte[]> blocks, Guid id, string tag)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException("blocks");
            }
            long num = (long)blocks.Count * (long)BlockSize;
            Interlocked.Add(ref smallPoolInUseSize, -num);
            foreach (byte[] block in blocks)
            {
                if (block == null || block.Length != BlockSize)
                {
                    throw new ArgumentException("blocks contains buffers that are not BlockSize in length.", "blocks");
                }
            }
            foreach (byte[] block2 in blocks)
            {
                if (MaximumFreeSmallPoolBytes == 0L || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
                {
                    Interlocked.Add(ref smallPoolFreeSize, BlockSize);
                    smallPool.Push(block2);
                    continue;
                }
                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Small, Events.MemoryStreamDiscardReason.EnoughFree);
                break;
            }
        }

        /// <summary>
        /// Returns a block to the pool.
        /// </summary>
        /// <param name="block">Block to return to the pool.</param>
        /// <param name="id">Unique Stream ID.</param>
        /// <param name="tag">The tag of the stream returning this, for logging if necessary.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="block" /> is null.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="block" /> is the wrong size for this memory manager.</exception>
        internal void ReturnBlock(byte[] block, Guid id, string tag)
        {
            int blockSize = BlockSize;
            Interlocked.Add(ref smallPoolInUseSize, -blockSize);
            if (block == null)
            {
                throw new ArgumentNullException("block");
            }
            if (block.Length != BlockSize)
            {
                throw new ArgumentException("block is not not BlockSize in length.");
            }
            if (MaximumFreeSmallPoolBytes == 0L || SmallPoolFreeSize < MaximumFreeSmallPoolBytes)
            {
                Interlocked.Add(ref smallPoolFreeSize, BlockSize);
                smallPool.Push(block);
            }
            else
            {
                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Small, Events.MemoryStreamDiscardReason.EnoughFree);
            }
        }

        internal void ReportBlockCreated()
        {
            Events.Writer.MemoryStreamNewBlockCreated(smallPoolInUseSize);
            this.BlockCreated?.Invoke(this, new BlockCreatedEventArgs(smallPoolInUseSize));
        }

        internal void ReportLargeBufferCreated(Guid id, string tag, long requiredSize, bool pooled, string callStack)
        {
            if (pooled)
            {
                Events.Writer.MemoryStreamNewLargeBufferCreated(requiredSize, LargePoolInUseSize);
            }
            else
            {
                Events.Writer.MemoryStreamNonPooledLargeBufferCreated(id, tag, requiredSize, callStack);
            }
            this.LargeBufferCreated?.Invoke(this, new LargeBufferCreatedEventArgs(id, tag, requiredSize, LargePoolInUseSize, pooled, callStack));
        }

        internal void ReportBufferDiscarded(Guid id, string tag, Events.MemoryStreamBufferType bufferType, Events.MemoryStreamDiscardReason reason)
        {
            Events.Writer.MemoryStreamDiscardBuffer(id, tag, bufferType, reason, SmallBlocksFree, smallPoolFreeSize, smallPoolInUseSize, LargeBuffersFree, LargePoolFreeSize, LargePoolInUseSize);
            this.BufferDiscarded?.Invoke(this, new BufferDiscardedEventArgs(id, tag, bufferType, reason));
        }

        internal void ReportStreamCreated(Guid id, string tag, long requestedSize, long actualSize)
        {
            Events.Writer.MemoryStreamCreated(id, tag, requestedSize, actualSize);
            this.StreamCreated?.Invoke(this, new StreamCreatedEventArgs(id, tag, requestedSize, actualSize));
        }

        internal void ReportStreamDisposed(Guid id, string tag, TimeSpan lifetime, string allocationStack, string disposeStack)
        {
            Events.Writer.MemoryStreamDisposed(id, tag, (long)lifetime.TotalMilliseconds, allocationStack, disposeStack);
            this.StreamDisposed?.Invoke(this, new StreamDisposedEventArgs(id, tag, lifetime, allocationStack, disposeStack));
        }

        internal void ReportStreamDoubleDisposed(Guid id, string tag, string allocationStack, string disposeStack1, string disposeStack2)
        {
            Events.Writer.MemoryStreamDoubleDispose(id, tag, allocationStack, disposeStack1, disposeStack2);
            this.StreamDoubleDisposed?.Invoke(this, new StreamDoubleDisposedEventArgs(id, tag, allocationStack, disposeStack1, disposeStack2));
        }

        internal void ReportStreamFinalized(Guid id, string tag, string allocationStack)
        {
            Events.Writer.MemoryStreamFinalized(id, tag, allocationStack);
            this.StreamFinalized?.Invoke(this, new StreamFinalizedEventArgs(id, tag, allocationStack));
        }

        internal void ReportStreamLength(long bytes)
        {
            this.StreamLength?.Invoke(this, new StreamLengthEventArgs(bytes));
        }

        internal void ReportStreamToArray(Guid id, string tag, string stack, long length)
        {
            Events.Writer.MemoryStreamToArray(id, tag, stack, length);
            this.StreamConvertedToArray?.Invoke(this, new StreamConvertedToArrayEventArgs(id, tag, stack, length));
        }

        internal void ReportStreamOverCapacity(Guid id, string tag, long requestedCapacity, string allocationStack)
        {
            Events.Writer.MemoryStreamOverCapacity(id, tag, requestedCapacity, MaximumStreamCapacity, allocationStack);
            this.StreamOverCapacity?.Invoke(this, new StreamOverCapacityEventArgs(id, tag, requestedCapacity, MaximumStreamCapacity, allocationStack));
        }

        internal void ReportUsageReport()
        {
            this.UsageReport?.Invoke(this, new UsageReportEventArgs(smallPoolInUseSize, smallPoolFreeSize, LargePoolInUseSize, LargePoolFreeSize));
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with no tag and a default initial capacity.
        /// </summary>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream()
        {
            return new RecyclableMemoryStream(this);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with no tag and a default initial capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id)
        {
            return new RecyclableMemoryStream(this, id);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag)
        {
            return new RecyclableMemoryStream(this, id, tag);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, id, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, long requiredSize)
        {
            return new RecyclableMemoryStream(this, id, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a <c>MemoryStream</c> which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, int requiredSize, bool asContiguousBuffer)
        {
            return GetStream(id, tag, (long)requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a <c>MemoryStream</c> which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, long requiredSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || requiredSize <= BlockSize)
            {
                return GetStream(id, tag, requiredSize);
            }
            return new RecyclableMemoryStream(this, id, tag, requiredSize, GetLargeBuffer(requiredSize, id, tag));
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
        {
            return GetStream(tag, (long)requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="M:Microsoft.IO.RecyclableMemoryStream.GetBuffer" />
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, long requiredSize, bool asContiguousBuffer)
        {
            return GetStream(Guid.NewGuid(), tag, requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, byte[] buffer, int offset, int count)
        {
            RecyclableMemoryStream recyclableMemoryStream = null;
            try
            {
                recyclableMemoryStream = new RecyclableMemoryStream(this, id, tag, count);
                recyclableMemoryStream.Write(buffer, offset, count);
                recyclableMemoryStream.Position = 0L;
                return recyclableMemoryStream;
            }
            catch
            {
                recyclableMemoryStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(byte[] buffer)
        {
            return GetStream(null, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
        {
            return GetStream(Guid.NewGuid(), tag, buffer, offset, count);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(Guid id, string tag, Memory<byte> buffer)
        {
            RecyclableMemoryStream recyclableMemoryStream = null;
            try
            {
                recyclableMemoryStream = new RecyclableMemoryStream(this, id, tag, buffer.Length);
                recyclableMemoryStream.Write(buffer.Span);
                recyclableMemoryStream.Position = 0L;
                return recyclableMemoryStream;
            }
            catch
            {
                recyclableMemoryStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, ReadOnlySpan<byte> buffer)
        {
            RecyclableMemoryStream recyclableMemoryStream = null;
            try
            {
                recyclableMemoryStream = new RecyclableMemoryStream(this, id, tag, buffer.Length);
                recyclableMemoryStream.Write(buffer);
                recyclableMemoryStream.Position = 0L;
                return recyclableMemoryStream;
            }
            catch
            {
                recyclableMemoryStream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(Memory<byte> buffer)
        {
            return GetStream(null, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(ReadOnlySpan<byte> buffer)
        {
            return GetStream(null, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(string tag, Memory<byte> buffer)
        {
            return GetStream(Guid.NewGuid(), tag, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, ReadOnlySpan<byte> buffer)
        {
            return GetStream(Guid.NewGuid(), tag, buffer);
        }
    }

}