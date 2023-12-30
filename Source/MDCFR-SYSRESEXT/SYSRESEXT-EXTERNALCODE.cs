/*
 * Portions of this code too comes from .NET Foundation:
 * 
 * Licensed to the .NET Foundation under one or more agreements.
 * The .NET Foundation licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
 */


using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{

    namespace IO
    {
        internal static class BinaryReaderExtensions
        {
            public static int Read7BitEncodedInt(this BinaryReader reader)
            {
                // Read out an Int32 7 bits at a time.  The high bit
                // of the byte when on means to continue reading more bytes.
                int count = 0;
                int shift = 0;
                byte b;
                do
                {
                    // Check for a corrupted stream.  Read a max of 5 bytes.
                    // In a future version, add a DataFormatException.
                    if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                    {
                        throw new FormatException(MDCFR.SysResExt.Properties.Resources.Format_Bad7BitInt32);
                    }

                    // ReadByte handles end of stream cases for us.
                    b = reader.ReadByte();
                    count |= (b & 0x7F) << shift;
                    shift += 7;
                } while ((b & 0x80) != 0);
                return count;
            }
        }

        internal static class BinaryWriterExtensions
        {
            public static void Write7BitEncodedInt(this BinaryWriter writer, int value)
            {
                // Write out an int 7 bits at a time.  The high bit of the byte,
                // when on, tells reader to continue reading more bytes.
                uint v = (uint)value;   // support negative numbers
                while (v >= 0x80)
                {
                    writer.Write((byte)(v | 0x80));
                    v >>= 7;
                }
                writer.Write((byte)v);
            }
        }

        /// <summary>
        /// Pins a <see langword="byte[]"/>, exposing it as an unmanaged memory stream.  Used in <see cref="System.Resources.ResourceReader"/> for corner cases.
        /// </summary>
        internal sealed unsafe class PinnedBufferMemoryStream : UnmanagedMemoryStream
        {
            private readonly byte[] _array;
            private GCHandle _pinningHandle;

            internal PinnedBufferMemoryStream(byte[] array)
            {
                Debug.Assert(array != null, "Array can't be null");

                _array = array;
                _pinningHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
                // Now the byte[] is pinned for the lifetime of this instance.
                // But I also need to get a pointer to that block of memory...
                int len = array.Length;
                fixed (byte* ptr = &MemoryMarshal.GetReference((Span<byte>)array))
                    Initialize(ptr, len, len, FileAccess.Read);
            }

            ~PinnedBufferMemoryStream()
            {
                Dispose(false);
            }

            protected override void Dispose(bool disposing)
            {
                if (_pinningHandle.IsAllocated)
                {
                    _pinningHandle.Free();
                }

                base.Dispose(disposing);
            }
        }
    }

    namespace Numerics.Hashing
    {
        internal static class HashHelpers
        {
            private readonly static System.Func<System.Int32> RS1 = () =>
            {
                System.Random RD = null;
                try
                {
                    RD = new();
                    return RD.Next(System.Int32.MinValue, System.Int32.MaxValue);
                } catch (System.Exception EX)
                {
                    // Rethrow the exception , but as an invalidoperation one , because actually calling unintialised RD is illegal.
                    throw new InvalidOperationException("Could not call Rand.Next. More than one errors occured.", EX);
                } finally { if (RD != null) { RD = null; } }
            };

            public static readonly int RandomSeed = RS1();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Combine(int h1, int h2)
            {
                // RyuJIT optimizes this to use the ROL instruction
                // Related GitHub pull request: dotnet/coreclr#1830
                uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
                return ((int)rol5 + h1) ^ h2;
            }
        }
    }

}