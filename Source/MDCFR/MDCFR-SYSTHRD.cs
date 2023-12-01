// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace System.Threading
{

    #nullable enable
    namespace Tasks
    {
        using Internal;
        using Sources;
        using System.Collections;
        using System.Collections.Generic;
        using System.Collections.Concurrent;
        using System.Runtime.ExceptionServices;

        internal interface IProducerConsumerQueue<T> : IEnumerable<T>, System.Collections.IEnumerable
        {
            bool IsEmpty { get; }
            int Count { get; }
            void Enqueue(T item);
            bool TryDequeue([MaybeNullWhen(false)] out T result);

            int GetCountSafe(object syncObj);
        }

        [DebuggerDisplay("Count = {Count}")]
        internal sealed class MultiProducerMultiConsumerQueue<T> : ConcurrentQueue<T>, System.Threading.Tasks.IProducerConsumerQueue<T>, IEnumerable<T>, IEnumerable
        {
            bool System.Threading.Tasks.IProducerConsumerQueue<T>.IsEmpty => base.IsEmpty;

            int System.Threading.Tasks.IProducerConsumerQueue<T>.Count => base.Count;

            void System.Threading.Tasks.IProducerConsumerQueue<T>.Enqueue(T item)
            {
                Enqueue(item);
            }

            bool System.Threading.Tasks.IProducerConsumerQueue<T>.TryDequeue([MaybeNullWhen(false)] out T result)
            {
                return TryDequeue(out result);
            }

            int System.Threading.Tasks.IProducerConsumerQueue<T>.GetCountSafe(object syncObj)
            {
                return base.Count;
            }
        }

#pragma warning disable CS8600, CS8618, CS8601
        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(SingleProducerSingleConsumerQueue<>.SingleProducerSingleConsumerQueue_DebugView))]
        internal sealed class SingleProducerSingleConsumerQueue<T> : System.Threading.Tasks.IProducerConsumerQueue<T>, IEnumerable<T>, IEnumerable
        {
            [StructLayout(LayoutKind.Sequential)]
            private sealed class Segment
            {
                internal Segment _next;

                internal readonly T[] _array;

                internal SegmentState _state;

                internal Segment(int size)
                {
                    _array = new T[size];
                }
            }

            private struct SegmentState
            {
                internal PaddingFor32 _pad0;

                internal volatile int _first;

                internal int _lastCopy;

                internal PaddingFor32 _pad1;

                internal int _firstCopy;

                internal volatile int _last;

                internal PaddingFor32 _pad2;
            }

            private sealed class SingleProducerSingleConsumerQueue_DebugView
            {
                private readonly System.Threading.Tasks.SingleProducerSingleConsumerQueue<T> _queue;

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public T[] Items
                {
                    get
                    {
                        List<T> list = new List<T>();
                        foreach (T item in _queue)
                        {
                            list.Add(item);
                        }
                        return list.ToArray();
                    }
                }

                public SingleProducerSingleConsumerQueue_DebugView(System.Threading.Tasks.SingleProducerSingleConsumerQueue<T> queue)
                {
                    _queue = queue;
                }
            }

            private const int INIT_SEGMENT_SIZE = 32;

            private const int MAX_SEGMENT_SIZE = 16777216;

            private volatile Segment _head;

            private volatile Segment _tail;

            public bool IsEmpty
            {
                get
                {
                    Segment head = _head;
                    if (head._state._first != head._state._lastCopy)
                    {
                        return false;
                    }
                    if (head._state._first != head._state._last)
                    {
                        return false;
                    }
                    return head._next == null;
                }
            }

            public int Count
            {
                get
                {
                    int num = 0;
                    for (Segment segment = _head; segment != null; segment = segment._next)
                    {
                        int num2 = segment._array.Length;
                        int first;
                        int last;
                        do
                        {
                            first = segment._state._first;
                            last = segment._state._last;
                        }
                        while (first != segment._state._first);
                        num += (last - first) & (num2 - 1);
                    }
                    return num;
                }
            }

            internal SingleProducerSingleConsumerQueue()
            {
                _head = (_tail = new Segment(32));
            }

            public void Enqueue(T item)
            {
                Segment segment = _tail;
                T[] array = segment._array;
                int last = segment._state._last;
                int num = (last + 1) & (array.Length - 1);
                if (num != segment._state._firstCopy)
                {
                    array[last] = item;
                    segment._state._last = num;
                }
                else
                {
                    EnqueueSlow(item, ref segment);
                }
            }

            private void EnqueueSlow(T item, ref Segment segment)
            {
                if (segment._state._firstCopy != segment._state._first)
                {
                    segment._state._firstCopy = segment._state._first;
                    Enqueue(item);
                    return;
                }
                int num = _tail._array.Length << 1;
                if (num > 16777216)
                {
                    num = 16777216;
                }
                Segment segment2 = new Segment(num);
                segment2._array[0] = item;
                segment2._state._last = 1;
                segment2._state._lastCopy = 1;
                try
                {
                }
                finally
                {
                    Volatile.Write(ref _tail._next, segment2);
                    _tail = segment2;
                }
            }

            public bool TryDequeue([MaybeNullWhen(false)] out T result)
            {
                Segment segment = _head;
                T[] array = segment._array;
                int first = segment._state._first;
                if (first != segment._state._lastCopy)
                {
                    result = array[first];
                    array[first] = default(T);
                    segment._state._first = (first + 1) & (array.Length - 1);
                    return true;
                }
                return TryDequeueSlow(ref segment, ref array, out result);
            }

            private bool TryDequeueSlow(ref Segment segment, ref T[] array, [MaybeNullWhen(false)] out T result)
            {
                if (segment._state._last != segment._state._lastCopy)
                {
                    segment._state._lastCopy = segment._state._last;
                    return TryDequeue(out result);
                }
                if (segment._next != null && segment._state._first == segment._state._last)
                {
                    segment = segment._next;
                    array = segment._array;
                    _head = segment;
                }
                int first = segment._state._first;
                if (first == segment._state._last)
                {
                    result = default(T);
                    return false;
                }
                result = array[first];
                array[first] = default(T);
                segment._state._first = (first + 1) & (segment._array.Length - 1);
                segment._state._lastCopy = segment._state._last;
                return true;
            }

            public bool TryPeek([MaybeNullWhen(false)] out T result)
            {
                Segment segment = _head;
                T[] array = segment._array;
                int first = segment._state._first;
                if (first != segment._state._lastCopy)
                {
                    result = array[first];
                    return true;
                }
                return TryPeekSlow(ref segment, ref array, out result);
            }

            private bool TryPeekSlow(ref Segment segment, ref T[] array, [MaybeNullWhen(false)] out T result)
            {
                if (segment._state._last != segment._state._lastCopy)
                {
                    segment._state._lastCopy = segment._state._last;
                    return TryPeek(out result);
                }
                if (segment._next != null && segment._state._first == segment._state._last)
                {
                    segment = segment._next;
                    array = segment._array;
                    _head = segment;
                }
                int first = segment._state._first;
                if (first == segment._state._last)
                {
                    result = default(T);
                    return false;
                }
                result = array[first];
                return true;
            }

            public bool TryDequeueIf(Predicate<T> predicate, [MaybeNullWhen(false)] out T result)
            {
                Segment segment = _head;
                T[] array = segment._array;
                int first = segment._state._first;
                if (first != segment._state._lastCopy)
                {
                    result = array[first];
                    if (predicate == null || predicate(result))
                    {
                        array[first] = default(T);
                        segment._state._first = (first + 1) & (array.Length - 1);
                        return true;
                    }
                    result = default(T);
                    return false;
                }
                return TryDequeueIfSlow(predicate, ref segment, ref array, out result);
            }

            private bool TryDequeueIfSlow(Predicate<T> predicate, ref Segment segment, ref T[] array, [MaybeNullWhen(false)] out T result)
            {
                if (segment._state._last != segment._state._lastCopy)
                {
                    segment._state._lastCopy = segment._state._last;
                    return TryDequeueIf(predicate, out result);
                }
                if (segment._next != null && segment._state._first == segment._state._last)
                {
                    segment = segment._next;
                    array = segment._array;
                    _head = segment;
                }
                int first = segment._state._first;
                if (first == segment._state._last)
                {
                    result = default(T);
                    return false;
                }
                result = array[first];
                if (predicate == null || predicate(result))
                {
                    array[first] = default(T);
                    segment._state._first = (first + 1) & (segment._array.Length - 1);
                    segment._state._lastCopy = segment._state._last;
                    return true;
                }
                result = default(T);
                return false;
            }

            public void Clear()
            {
                T result;
                while (TryDequeue(out result)) { }
            }

            public IEnumerator<T> GetEnumerator()
            {
                for (Segment segment = _head; segment != null; segment = segment._next)
                {
                    for (int pt = segment._state._first; pt != segment._state._last; pt = (pt + 1) & (segment._array.Length - 1))
                    {
                        yield return segment._array[pt];
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            int System.Threading.Tasks.IProducerConsumerQueue<T>.GetCountSafe(object syncObj)
            {
                lock (syncObj)
                {
                    return Count;
                }
            }
        }
#pragma warning restore CS8600, CS8618, CS8601

        namespace Sources
        {
            /// <summary>
            /// Flags passed from <see cref="ValueTask"/> and <see cref="ValueTask{TResult}"/> to
            /// <see cref="IValueTaskSource.OnCompleted"/> and <see cref="IValueTaskSource{TResult}.OnCompleted"/>
            /// to control behavior.
            /// </summary>
            [Flags]
            public enum ValueTaskSourceOnCompletedFlags
            {
                /// <summary>
                /// No requirements are placed on how the continuation is invoked.
                /// </summary>
                None,
                /// <summary>
                /// Set if OnCompleted should capture the current scheduling context (e.g. SynchronizationContext)
                /// and use it when queueing the continuation for execution.  If this is not set, the implementation
                /// may choose to execute the continuation in an arbitrary location.
                /// </summary>
                UseSchedulingContext = 0x1,
                /// <summary>
                /// Set if OnCompleted should capture the current ExecutionContext and use it to run the continuation.
                /// </summary>
                FlowExecutionContext = 0x2,
            }

            /// <summary>Indicates the status of an <see cref="IValueTaskSource"/> or <see cref="IValueTaskSource{TResult}"/>.</summary>
            public enum ValueTaskSourceStatus
            {
                /// <summary>The operation has not yet completed.</summary>
                Pending = 0,
                /// <summary>The operation completed successfully.</summary>
                Succeeded = 1,
                /// <summary>The operation completed with an error.</summary>
                Faulted = 2,
                /// <summary>The operation completed due to cancellation.</summary>
                Canceled = 3
            }

            /// <summary>Represents an object that can be wrapped by a <see cref="ValueTask"/>.</summary>
            public interface IValueTaskSource
            {
                /// <summary>Gets the status of the current operation.</summary>
                /// <param name="token">Opaque value that was provided to the <see cref="ValueTask"/>'s constructor.</param>
                ValueTaskSourceStatus GetStatus(short token);

                /// <summary>Schedules the continuation action for this <see cref="IValueTaskSource"/>.</summary>
                /// <param name="continuation">The continuation to invoke when the operation has completed.</param>
                /// <param name="state">The state object to pass to <paramref name="continuation"/> when it's invoked.</param>
                /// <param name="token">Opaque value that was provided to the <see cref="ValueTask"/>'s constructor.</param>
                /// <param name="flags">The flags describing the behavior of the continuation.</param>
                void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags);

                /// <summary>Gets the result of the <see cref="IValueTaskSource"/>.</summary>
                /// <param name="token">Opaque value that was provided to the <see cref="ValueTask"/>'s constructor.</param>
                void GetResult(short token);
            }

            /// <summary>Represents an object that can be wrapped by a <see cref="ValueTask{TResult}"/>.</summary>
            /// <typeparam name="TResult">Specifies the type of data returned from the object.</typeparam>
            public interface IValueTaskSource<out TResult>
            {
                /// <summary>Gets the status of the current operation.</summary>
                /// <param name="token">Opaque value that was provided to the <see cref="ValueTask"/>'s constructor.</param>
                ValueTaskSourceStatus GetStatus(short token);

                /// <summary>Schedules the continuation action for this <see cref="IValueTaskSource{TResult}"/>.</summary>
                /// <param name="continuation">The continuation to invoke when the operation has completed.</param>
                /// <param name="state">The state object to pass to <paramref name="continuation"/> when it's invoked.</param>
                /// <param name="token">Opaque value that was provided to the <see cref="ValueTask"/>'s constructor.</param>
                /// <param name="flags">The flags describing the behavior of the continuation.</param>
                void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags);

                /// <summary>Gets the result of the <see cref="IValueTaskSource{TResult}"/>.</summary>
                /// <param name="token">Opaque value that was provided to the <see cref="ValueTask"/>'s constructor.</param>
                TResult GetResult(short token);
            }

            #nullable disable
            /// <summary>Provides the core logic for implementing a manual-reset <see cref="IValueTaskSource" /> or <see cref="IValueTaskSource{T}" />.</summary>
            /// <typeparam name="TResult"></typeparam>
            [StructLayout(LayoutKind.Auto)]
            public struct ManualResetValueTaskSourceCore<TResult>
            {
                /// <summary>
                /// The callback to invoke when the operation completes if <see cref="ManualResetValueTaskSourceCore{T}.OnCompleted(System.Action{System.Object},System.Object,System.Int16,System.Threading.Tasks.Sources.ValueTaskSourceOnCompletedFlags)" /> was called before the operation completed,
                /// or <see cref="ManualResetValueTaskSourceCoreShared.s_sentinel" /> if the operation completed before a callback was supplied,
                /// or null if a callback hasn't yet been provided and the operation hasn't yet completed.
                /// </summary>
                private Action<object> _continuation;

                /// <summary>State to pass to <see cref="ManualResetValueTaskSourceCore{T}._continuation" />.</summary>
                private object _continuationState;

                /// <summary><see cref="System.Threading.ExecutionContext" /> to flow to the callback, or null if no flowing is required.</summary>
                private ExecutionContext _executionContext;

                /// <summary>
                /// A "captured" <see cref="System.Threading.SynchronizationContext" /> or <see cref="System.Threading.Tasks.TaskScheduler" /> with which to invoke the callback,
                /// or null if no special context is required.
                /// </summary>
                private object _capturedContext;

                /// <summary>Whether the current operation has completed.</summary>
                private bool _completed;

                /// <summary>The result with which the operation succeeded, or the default value if it hasn't yet completed or failed.</summary>
                private TResult _result;

                /// <summary>The exception with which the operation failed, or null if it hasn't yet completed or completed successfully.</summary>
                private ExceptionDispatchInfo _error;

                /// <summary>The current version of this value, used to help prevent misuse.</summary>
                private short _version;

                /// <summary>Gets or sets whether to force continuations to run asynchronously.</summary>
                /// <remarks>Continuations may run asynchronously if this is false, but they'll never run synchronously if this is true.</remarks>
                public bool RunContinuationsAsynchronously { get; set; }

                /// <summary>Gets the operation version.</summary>
                public short Version => _version;

                /// <summary>Resets to prepare for the next operation.</summary>
                public void Reset()
                {
                    _version++;
                    _completed = false;
                    _result = default(TResult);
                    _error = null;
                    _executionContext = null;
                    _capturedContext = null;
                    _continuation = null;
                    _continuationState = null;
                }

                /// <summary>Completes with a successful result.</summary>
                /// <param name="result">The result.</param>
                public void SetResult(TResult result)
                {
                    _result = result;
                    SignalCompletion();
                }

                /// <summary>Complets with an error.</summary>
                /// <param name="error"></param>
                public void SetException(Exception error)
                {
                    _error = ExceptionDispatchInfo.Capture(error);
                    SignalCompletion();
                }

                /// <summary>Gets the status of the operation.</summary>
                /// <param name="token">Opaque value that was provided to the <see cref="T:System.Threading.Tasks.ValueTask" />'s constructor.</param>
                public ValueTaskSourceStatus GetStatus(short token)
                {
                    ValidateToken(token);
                    if (_continuation != null && _completed)
                    {
                        if (_error != null)
                        {
                            if (!(_error.SourceException is OperationCanceledException))
                            {
                                return ValueTaskSourceStatus.Faulted;
                            }
                            return ValueTaskSourceStatus.Canceled;
                        }
                        return ValueTaskSourceStatus.Succeeded;
                    }
                    return ValueTaskSourceStatus.Pending;
                }

                /// <summary>Gets the result of the operation.</summary>
                /// <param name="token">Opaque value that was provided to the <see cref="T:System.Threading.Tasks.ValueTask" />'s constructor.</param>
                public TResult GetResult(short token)
                {
                    ValidateToken(token);
                    if (!_completed)
                    {
                        throw new InvalidOperationException();
                    }
                    _error?.Throw();
                    return _result;
                }

                /// <summary>Schedules the continuation action for this operation.</summary>
                /// <param name="continuation">The continuation to invoke when the operation has completed.</param>
                /// <param name="state">The state object to pass to <paramref name="continuation" /> when it's invoked.</param>
                /// <param name="token">Opaque value that was provided to the <see cref="ValueTask" />'s constructor.</param>
                /// <param name="flags">The flags describing the behavior of the continuation.</param>
                #nullable enable
                public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
                {
                #nullable disable
                    if (continuation == null)
                    {
                        throw new ArgumentNullException("continuation");
                    }
                    ValidateToken(token);
                    if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
                    {
                        _executionContext = ExecutionContext.Capture();
                    }
                    if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
                    {
                        SynchronizationContext current = SynchronizationContext.Current;
                        if (current != null && current.GetType() != typeof(SynchronizationContext))
                        {
                            _capturedContext = current;
                        }
                        else
                        {
                            TaskScheduler current2 = TaskScheduler.Current;
                            if (current2 != TaskScheduler.Default)
                            {
                                _capturedContext = current2;
                            }
                        }
                    }
                    object obj = _continuation;
                    if (obj == null)
                    {
                        _continuationState = state;
                        obj = Interlocked.CompareExchange(ref _continuation, continuation, null);
                    }
                    if (obj == null)
                    {
                        return;
                    }
                    if (obj != ManualResetValueTaskSourceCoreShared.s_sentinel)
                    {
                        throw new InvalidOperationException();
                    }
                    object capturedContext = _capturedContext;
                    if (capturedContext != null)
                    {
                        if (!(capturedContext is SynchronizationContext synchronizationContext))
                        {
                            if (capturedContext is TaskScheduler scheduler)
                            {
                                Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);
                            }
                        }
                        else
                        {
                            synchronizationContext.Post(delegate (object s)
                            {
                                Tuple<Action<object>, object> tuple = (Tuple<Action<object>, object>)s;
                                tuple.Item1(tuple.Item2);
                            }, Tuple.Create(continuation, state));
                        }
                    }
                    else
                    {
                        Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
                    }
                }

                /// <summary>Ensures that the specified token matches the current version.</summary>
                /// <param name="token">The token supplied by <see cref="T:System.Threading.Tasks.ValueTask" />.</param>
                private void ValidateToken(short token)
                {
                    if (token != _version)
                    {
                        throw new InvalidOperationException();
                    }
                }

                /// <summary>Signals that the operation has completed.  Invoked after the result or error has been set.</summary>
                private void SignalCompletion()
                {
                    if (_completed)
                    {
                        throw new InvalidOperationException();
                    }
                    _completed = true;
                    if (_continuation == null && Interlocked.CompareExchange(ref _continuation, ManualResetValueTaskSourceCoreShared.s_sentinel, null) == null)
                    {
                        return;
                    }
                    if (_executionContext != null)
                    {
                        ExecutionContext.Run(_executionContext, delegate (object s)
                        {
                            ((ManualResetValueTaskSourceCore<TResult>)s).InvokeContinuation();
                        }, this);
                    }
                    else
                    {
                        InvokeContinuation();
                    }
                }

                /// <summary>
                /// Invokes the continuation with the appropriate captured context / scheduler.
                /// This assumes that if <see cref="F:System.Threading.Tasks.Sources.ManualResetValueTaskSourceCore`1._executionContext" /> is not null we're already
                /// running within that <see cref="T:System.Threading.ExecutionContext" />.
                /// </summary>
                private void InvokeContinuation()
                {
                    object capturedContext = _capturedContext;
                    if (capturedContext != null)
                    {
                        if (!(capturedContext is SynchronizationContext synchronizationContext))
                        {
                            if (capturedContext is TaskScheduler scheduler)
                            {
                                Task.Factory.StartNew(_continuation, _continuationState, CancellationToken.None, TaskCreationOptions.DenyChildAttach, scheduler);
                            }
                        }
                        else
                        {
                            synchronizationContext.Post(delegate (object s)
                            {
                                Tuple<Action<object>, object> tuple = (Tuple<Action<object>, object>)s;
                                tuple.Item1(tuple.Item2);
                            }, Tuple.Create(_continuation, _continuationState));
                        }
                    }
                    else if (RunContinuationsAsynchronously)
                    {
                        Task.Factory.StartNew(_continuation, _continuationState, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
                    }
                    else
                    {
                        _continuation(_continuationState);
                    }
                }
            }

            internal static class ManualResetValueTaskSourceCoreShared
            {
                internal static readonly Action<object> s_sentinel = CompletionSentinel;

                private static void CompletionSentinel(object _)
                {
                    throw new InvalidOperationException();
                }
            }
            #nullable enable
        }


        // TYPE SAFETY WARNING:
        // This code uses Unsafe.As to cast _obj.  This is done in order to minimize the costs associated with
        // casting _obj to a variety of different types that can be stored in a ValueTask, e.g. Task<TResult>
        // vs IValueTaskSource<TResult>.  Previous attempts at this were faulty due to using a separate field
        // to store information about the type of the object in _obj; this is faulty because if the ValueTask
        // is stored into a field, concurrent read/writes can result in tearing the _obj from the type information
        // stored in a separate field.  This means we can rely only on the _obj field to determine how to handle
        // it.  As such, the pattern employed is to copy _obj into a local obj, and then check it for null and
        // type test against Task/Task<TResult>.  Since the ValueTask can only be constructed with null, Task,
        // or IValueTaskSource, we can then be confident in knowing that if it doesn't match one of those values,
        // it must be an IValueTaskSource, and we can use Unsafe.As.  This could be defeated by other unsafe means,
        // like private reflection or using Unsafe.As manually, but at that point you're already doing things
        // that can violate type safety; we only care about getting correct behaviors when using "safe" code.
        // There are still other race conditions in user's code that can result in errors, but such errors don't
        // cause ValueTask to violate type safety.

        /// <summary>Provides an awaitable result of an asynchronous operation.</summary>
        /// <remarks>
        /// <see cref="ValueTask"/> instances are meant to be directly awaited.  To do more complicated operations with them, a <see cref="Task"/>
        /// should be extracted using <see cref="AsTask"/>.  Such operations might include caching a task instance to be awaited later,
        /// registering multiple continuations with a single task, awaiting the same task multiple times, and using combinators over
        /// multiple operations:
        /// <list type="bullet">
        /// <item>
        /// Once the result of a <see cref="ValueTask"/> instance has been retrieved, do not attempt to retrieve it again.
        /// <see cref="ValueTask"/> instances may be backed by <see cref="IValueTaskSource"/> instances that are reusable, and such
        /// instances may use the act of retrieving the instances result as a notification that the instance may now be reused for
        /// a different operation.  Attempting to then reuse that same <see cref="ValueTask"/> results in undefined behavior.
        /// </item>
        /// <item>
        /// Do not attempt to add multiple continuations to the same <see cref="ValueTask"/>.  While this might work if the
        /// <see cref="ValueTask"/> wraps a <c>T</c> or a <see cref="Task"/>, it may not work if the <see cref="ValueTask"/>
        /// was constructed from an <see cref="IValueTaskSource"/>.
        /// </item>
        /// <item>
        /// Some operations that return a <see cref="ValueTask"/> may invalidate it based on some subsequent operation being performed.
        /// Unless otherwise documented, assume that a <see cref="ValueTask"/> should be awaited prior to performing any additional operations
        /// on the instance from which it was retrieved.
        /// </item>
        /// </list>
        /// </remarks>
        [AsyncMethodBuilder(typeof(AsyncValueTaskMethodBuilder))]
        [StructLayout(LayoutKind.Auto)]
        public readonly struct ValueTask : IEquatable<ValueTask>
        {
            /// <summary>A task canceled using `new CancellationToken(true)`. Lazily created only when first needed.</summary>
            private static volatile Task? s_canceledTask;

            /// <summary>null if representing a successful synchronous completion, otherwise a <see cref="Task"/> or a <see cref="IValueTaskSource"/>.</summary>
            internal readonly object? _obj;
            /// <summary>Opaque value passed through to the <see cref="IValueTaskSource"/>.</summary>
            internal readonly short _token;
            /// <summary>true to continue on the captured context; otherwise, false.</summary>
            /// <remarks>Stored in the <see cref="ValueTask"/> rather than in the configured awaiter to utilize otherwise padding space.</remarks>
            internal readonly bool _continueOnCapturedContext;

            // An instance created with the default ctor (a zero init'd struct) represents a synchronously, successfully completed operation.

            /// <summary>Initialize the <see cref="ValueTask"/> with a <see cref="Task"/> that represents the operation.</summary>
            /// <param name="task">The task.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask(Task task)
            {
                if (task == null)
                {
                    throw new System.ArgumentNullException($"{task}");
                }

                _obj = task;

                _continueOnCapturedContext = true;
                _token = 0;
            }

            /// <summary>Initialize the <see cref="ValueTask"/> with a <see cref="IValueTaskSource"/> object that represents the operation.</summary>
            /// <param name="source">The source.</param>
            /// <param name="token">Opaque value passed through to the <see cref="IValueTaskSource"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask(IValueTaskSource source, short token)
            {
                if (source == null)
                {
                    throw new System.ArgumentNullException($"{source}");
                }

                _obj = source;
                _token = token;

                _continueOnCapturedContext = true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ValueTask(object? obj, short token, bool continueOnCapturedContext)
            {
                _obj = obj;
                _token = token;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            /// <summary>Gets a task that has already completed successfully.</summary>
            public static ValueTask CompletedTask => default;

            /// <summary>Creates a <see cref="ValueTask{TResult}"/> that's completed successfully with the specified result.</summary>
            /// <typeparam name="TResult">The type of the result returned by the task.</typeparam>
            /// <param name="result">The result to store into the completed task.</param>
            /// <returns>The successfully completed task.</returns>
            public static ValueTask<TResult> FromResult<TResult>(TResult result) => new ValueTask<TResult>(result);

            /// <summary>Creates a <see cref="ValueTask"/> that has completed due to cancellation with the specified cancellation token.</summary>
            /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
            /// <returns>The canceled task.</returns>
            public static ValueTask FromCanceled(CancellationToken cancellationToken) => new ValueTask(Task.FromCanceled(cancellationToken));

            /// <summary>Creates a <see cref="ValueTask{TResult}"/> that has completed due to cancellation with the specified cancellation token.</summary>
            /// <param name="cancellationToken">The cancellation token with which to complete the task.</param>
            /// <returns>The canceled task.</returns>
            public static ValueTask<TResult> FromCanceled<TResult>(CancellationToken cancellationToken) =>
                new ValueTask<TResult>(Task.FromCanceled<TResult>(cancellationToken));

            /// <summary>Creates a <see cref="ValueTask"/> that has completed with the specified exception.</summary>
            /// <param name="exception">The exception with which to complete the task.</param>
            /// <returns>The faulted task.</returns>
            public static ValueTask FromException(Exception exception) => new ValueTask(Task.FromException(exception));

            /// <summary>Creates a <see cref="ValueTask{TResult}"/> that has completed with the specified exception.</summary>
            /// <param name="exception">The exception with which to complete the task.</param>
            /// <returns>The faulted task.</returns>
            public static ValueTask<TResult> FromException<TResult>(Exception exception) => new ValueTask<TResult>(Task.FromException<TResult>(exception));

            /// <summary>Returns the hash code for this instance.</summary>
            public override int GetHashCode() => _obj?.GetHashCode() ?? 0;

            /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
            public override bool Equals([NotNullWhen(true)] object? obj) => obj is ValueTask && Equals((ValueTask)obj);

            /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="ValueTask"/> value.</summary>
            public bool Equals(ValueTask other) => _obj == other._obj && _token == other._token;

            /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are equal.</summary>
            public static bool operator ==(ValueTask left, ValueTask right) => left.Equals(right);

            /// <summary>Returns a value indicating whether two <see cref="ValueTask"/> values are not equal.</summary>
            public static bool operator !=(ValueTask left, ValueTask right) => !left.Equals(right);

            /// <summary>
            /// Gets a <see cref="Task"/> object to represent this ValueTask.
            /// </summary>
            /// <remarks>
            /// It will either return the wrapped task object if one exists, or it'll
            /// manufacture a new task object to represent the result.
            /// </remarks>
            public Task AsTask()
            {
                object? obj = _obj;
                Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);
                return obj == null ? Task.CompletedTask : obj as Task ?? GetTaskForValueTaskSource(Unsafe.As<IValueTaskSource>(obj));
            }

            /// <summary>Gets a <see cref="ValueTask"/> that may be used at any point in the future.</summary>
            public ValueTask Preserve() => _obj == null ? this : new ValueTask(AsTask());

            /// <summary>Creates a <see cref="Task"/> to represent the <see cref="IValueTaskSource"/>.</summary>
            /// <remarks>
            /// The <see cref="IValueTaskSource"/> is passed in rather than reading and casting <see cref="_obj"/>
            /// so that the caller can pass in an object it's already validated.
            /// </remarks>
            private Task GetTaskForValueTaskSource(IValueTaskSource t)
            {
                ValueTaskSourceStatus status = t.GetStatus(_token);
                if (status != ValueTaskSourceStatus.Pending)
                {
                    try
                    {
                        // Propagate any exceptions that may have occurred, then return
                        // an already successfully completed task.
                        t.GetResult(_token);
                        return Task.CompletedTask;

                        // If status is Faulted or Canceled, GetResult should throw.  But
                        // we can't guarantee every implementation will do the "right thing".
                        // If it doesn't throw, we just treat that as success and ignore
                        // the status.
                    }
                    catch (System.Exception exc)
                    {
                        if (status == ValueTaskSourceStatus.Canceled)
                        {
                            if (exc is System.OperationCanceledException oce)
                            {
                                var task = new TaskCompletionSource<System.Boolean>();
                                task.TrySetException(oce);
                                return task.Task;
                            }

                            // Benign race condition to initialize cached task, as identity doesn't matter.
                            return s_canceledTask ??= Task.FromCanceled(new CancellationToken(canceled: true));
                        }
                        else
                        {
                            return Task.FromException(exc);
                        }
                    }
                }

                return new ValueTaskSourceAsTask(t, _token).Task;
            }

            /// <summary>Type used to create a <see cref="Task"/> to represent a <see cref="IValueTaskSource"/>.</summary>
            private sealed class ValueTaskSourceAsTask : Threading.Tasks.TaskCompletionSource<System.Boolean>
            {

                private static readonly Action<object?> s_completionAction = static state =>
                {
                    if (!(state is ValueTaskSourceAsTask vtst) ||
                        !(vtst._source is IValueTaskSource source))
                    {
                        // This could only happen if the IValueTaskSource passed the wrong state
                        // or if this callback were invoked multiple times such that the state
                        // was previously nulled out.
                        throw new ArgumentOutOfRangeException($"{state}");
                    }

                    vtst._source = null;
                    ValueTaskSourceStatus status = source.GetStatus(vtst._token);
                    try
                    {
                        source.GetResult(vtst._token);
                        vtst.TrySetResult(false);
                    }
                    catch (Exception exc)
                    {
                        if (status == ValueTaskSourceStatus.Canceled)
                        {
                            if (exc is OperationCanceledException oce)
                            {
                                vtst.TrySetCanceled(oce.CancellationToken);
                            }
                            else
                            {
                                vtst.TrySetCanceled(new CancellationToken(true));
                            }
                        }
                        else
                        {
                            vtst.TrySetException(exc);
                        }
                    }
                };

                /// <summary>The associated <see cref="IValueTaskSource"/>.</summary>
                private IValueTaskSource? _source;
                /// <summary>The token to pass through to operations on <see cref="_source"/></summary>
                private readonly short _token;

                internal ValueTaskSourceAsTask(IValueTaskSource source, short token)
                {
                    _token = token;
                    _source = source;
                    source.OnCompleted(s_completionAction, this, token, ValueTaskSourceOnCompletedFlags.None);
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask"/> represents a completed operation.</summary>
            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                    if (obj == null)
                    {
                        return true;
                    }

                    if (obj is Task t)
                    {
                        return t.IsCompleted;
                    }

                    return Unsafe.As<IValueTaskSource>(obj).GetStatus(_token) != ValueTaskSourceStatus.Pending;
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask"/> represents a successfully completed operation.</summary>
            public bool IsCompletedSuccessfully
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                    if (obj == null)
                    {
                        return true;
                    }

                    if (obj is Task t)
                    {
                        return (System.Int32)t.Status == 5;
                    }

                    return Unsafe.As<IValueTaskSource>(obj).GetStatus(_token) == ValueTaskSourceStatus.Succeeded;
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask"/> represents a failed operation.</summary>
            public bool IsFaulted
            {
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                    if (obj == null)
                    {
                        return false;
                    }

                    if (obj is Task t)
                    {
                        return t.IsFaulted;
                    }

                    return Unsafe.As<IValueTaskSource>(obj).GetStatus(_token) == ValueTaskSourceStatus.Faulted;
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask"/> represents a canceled operation.</summary>
            /// <remarks>
            /// If the <see cref="ValueTask"/> is backed by a result or by a <see cref="IValueTaskSource"/>,
            /// this will always return false.  If it's backed by a <see cref="Task"/>, it'll return the
            /// value of the task's <see cref="Task.IsCanceled"/> property.
            /// </remarks>
            public bool IsCanceled
            {
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                    if (obj == null)
                    {
                        return false;
                    }

                    if (obj is Task t)
                    {
                        return t.IsCanceled;
                    }

                    return Unsafe.As<IValueTaskSource>(obj).GetStatus(_token) == ValueTaskSourceStatus.Canceled;
                }
            }

            /// <summary>Throws the exception that caused the <see cref="ValueTask"/> to fail.  If it completed successfully, nothing is thrown.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void ThrowIfCompletedUnsuccessfully()
            {
                object? obj = _obj;
                Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                if (obj != null)
                {
                    if (obj is Task t)
                    {
                        t.GetAwaiter().GetResult();
                    }
                    else
                    {
                        Unsafe.As<IValueTaskSource>(obj).GetResult(_token);
                    }
                }
            }

            /// <summary>
            /// Transfers the <see cref="ValueTask{TResult}"/> to a <see cref="ValueTask"/> instance.
            ///
            /// The <see cref="ValueTask{TResult}"/> should not be used after calling this method.
            /// </summary>
            internal static ValueTask DangerousCreateFromTypedValueTask<TResult>(ValueTask<TResult> valueTask)
            {
                Debug.Assert(valueTask._obj is null or Task or IValueTaskSource, "If the ValueTask<>'s backing object is an IValueTaskSource<TResult>, it must also be IValueTaskSource.");

                return new ValueTask(valueTask._obj, valueTask._token, valueTask._continueOnCapturedContext);
            }

            /// <summary>Gets an awaiter for this <see cref="ValueTask"/>.</summary>
            public ValueTaskAwaiter GetAwaiter() => new ValueTaskAwaiter(this);

            /// <summary>Configures an awaiter for this <see cref="ValueTask"/>.</summary>
            /// <param name="continueOnCapturedContext">
            /// true to attempt to marshal the continuation back to the captured context; otherwise, false.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfiguredValueTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) =>
                new ConfiguredValueTaskAwaitable(new ValueTask(_obj, _token, continueOnCapturedContext));
        }

        /// <summary>Provides a value type that can represent a synchronously available value or a task object.</summary>
        /// <typeparam name="TResult">Specifies the type of the result.</typeparam>
        /// <remarks>
        /// <see cref="ValueTask{TResult}"/> instances are meant to be directly awaited.  To do more complicated operations with them, a <see cref="Task{TResult}"/>
        /// should be extracted using <see cref="AsTask"/>.  Such operations might include caching a task instance to be awaited later,
        /// registering multiple continuations with a single task, awaiting the same task multiple times, and using combinators over
        /// multiple operations:
        /// <list type="bullet">
        /// <item>
        /// Once the result of a <see cref="ValueTask{TResult}"/> instance has been retrieved, do not attempt to retrieve it again.
        /// <see cref="ValueTask{TResult}"/> instances may be backed by <see cref="IValueTaskSource{TResult}"/> instances that are reusable, and such
        /// instances may use the act of retrieving the instances result as a notification that the instance may now be reused for
        /// a different operation.  Attempting to then reuse that same <see cref="ValueTask{TResult}"/> results in undefined behavior.
        /// </item>
        /// <item>
        /// Do not attempt to add multiple continuations to the same <see cref="ValueTask{TResult}"/>.  While this might work if the
        /// <see cref="ValueTask{TResult}"/> wraps a <c>T</c> or a <see cref="Task{TResult}"/>, it may not work if the <see cref="Task{TResult}"/>
        /// was constructed from an <see cref="IValueTaskSource{TResult}"/>.
        /// </item>
        /// <item>
        /// Some operations that return a <see cref="ValueTask{TResult}"/> may invalidate it based on some subsequent operation being performed.
        /// Unless otherwise documented, assume that a <see cref="ValueTask{TResult}"/> should be awaited prior to performing any additional operations
        /// on the instance from which it was retrieved.
        /// </item>
        /// </list>
        /// </remarks>
        [AsyncMethodBuilder(typeof(AsyncValueTaskMethodBuilder<>))]
        [StructLayout(LayoutKind.Auto)]
        public readonly struct ValueTask<TResult> : IEquatable<ValueTask<TResult>>
        {
            /// <summary>A task canceled using `new CancellationToken(true)`. Lazily created only when first needed.</summary>
            private static volatile Task<TResult>? s_canceledTask;
            /// <summary>null if <see cref="_result"/> has the result, otherwise a <see cref="Task{TResult}"/> or a <see cref="IValueTaskSource{TResult}"/>.</summary>
            internal readonly object? _obj;
            /// <summary>The result to be used if the operation completed successfully synchronously.</summary>
            internal readonly TResult? _result;
            /// <summary>Opaque value passed through to the <see cref="IValueTaskSource{TResult}"/>.</summary>
            internal readonly short _token;
            /// <summary>true to continue on the captured context; otherwise, false.</summary>
            /// <remarks>Stored in the <see cref="ValueTask{TResult}"/> rather than in the configured awaiter to utilize otherwise padding space.</remarks>
            internal readonly bool _continueOnCapturedContext;

            // An instance created with the default ctor (a zero init'd struct) represents a synchronously, successfully completed operation
            // with a result of default(TResult).

            /// <summary>Initialize the <see cref="ValueTask{TResult}"/> with a <typeparamref name="TResult"/> result value.</summary>
            /// <param name="result">The result.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask(TResult result)
            {
                _result = result;

                _obj = null;
                _continueOnCapturedContext = true;
                _token = 0;
            }

            /// <summary>Initialize the <see cref="ValueTask{TResult}"/> with a <see cref="Task{TResult}"/> that represents the operation.</summary>
            /// <param name="task">The task.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask(Task<TResult> task)
            {
                if (task == null)
                {
                    throw new ArgumentNullException($"{task}");
                }

                _obj = task;

                _result = default;
                _continueOnCapturedContext = true;
                _token = 0;
            }

            /// <summary>Initialize the <see cref="ValueTask{TResult}"/> with a <see cref="IValueTaskSource{TResult}"/> object that represents the operation.</summary>
            /// <param name="source">The source.</param>
            /// <param name="token">Opaque value passed through to the <see cref="IValueTaskSource"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTask(IValueTaskSource<TResult> source, short token)
            {
                if (source == null)
                {
                    throw new ArgumentNullException($"{source}");
                }

                _obj = source;
                _token = token;

                _result = default;
                _continueOnCapturedContext = true;
            }

            /// <summary>Non-verified initialization of the struct to the specified values.</summary>
            /// <param name="obj">The object.</param>
            /// <param name="result">The result.</param>
            /// <param name="token">The token.</param>
            /// <param name="continueOnCapturedContext">true to continue on captured context; otherwise, false.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private ValueTask(object? obj, TResult? result, short token, bool continueOnCapturedContext)
            {
                _obj = obj;
                _result = result;
                _token = token;
                _continueOnCapturedContext = continueOnCapturedContext;
            }

            /// <summary>Returns the hash code for this instance.</summary>
            public override int GetHashCode() => _obj != null ? _obj.GetHashCode() : _result != null ? _result.GetHashCode() : 0;

            /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="object"/>.</summary>
            public override bool Equals([NotNullWhen(true)] object? obj) => obj is ValueTask<TResult> && Equals((ValueTask<TResult>)obj);

#pragma warning disable CS8604
            /// <summary>Returns a value indicating whether this value is equal to a specified <see cref="ValueTask{TResult}"/> value.</summary>
            public bool Equals(ValueTask<TResult> other) => _obj != null || other._obj != null ? _obj == other._obj && _token == other._token :
                    System.Collections.Generic.EqualityComparer<TResult>.Default.Equals(_result, other._result);
#pragma warning restore CS8604

            /// <summary>Returns a value indicating whether two <see cref="ValueTask{TResult}"/> values are equal.</summary>
            public static bool operator ==(ValueTask<TResult> left, ValueTask<TResult> right) => left.Equals(right);

            /// <summary>Returns a value indicating whether two <see cref="ValueTask{TResult}"/> values are not equal.</summary>
            public static bool operator !=(ValueTask<TResult> left, ValueTask<TResult> right) => !left.Equals(right);

            /// <summary>
            /// Gets a <see cref="Task{TResult}"/> object to represent this ValueTask.
            /// </summary>
            /// <remarks>
            /// It will either return the wrapped task object if one exists, or it'll
            /// manufacture a new task object to represent the result.
            /// </remarks>
            public Task<TResult> AsTask()
            {
                object? obj = _obj;
                Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                if (obj == null)
                {
                    return Task.FromResult(_result!);
                }

                if (obj is Task<TResult> t)
                {
                    return t;
                }

                return GetTaskForValueTaskSource(Unsafe.As<IValueTaskSource<TResult>>(obj));
            }

            /// <summary>Gets a <see cref="ValueTask{TResult}"/> that may be used at any point in the future.</summary>
            public ValueTask<TResult> Preserve() => _obj == null ? this : new ValueTask<TResult>(AsTask());

            /// <summary>Creates a <see cref="Task{TResult}"/> to represent the <see cref="IValueTaskSource{TResult}"/>.</summary>
            /// <remarks>
            /// The <see cref="IValueTaskSource{TResult}"/> is passed in rather than reading and casting <see cref="_obj"/>
            /// so that the caller can pass in an object it's already validated.
            /// </remarks>
            private Task<TResult> GetTaskForValueTaskSource(IValueTaskSource<TResult> t)
            {
                ValueTaskSourceStatus status = t.GetStatus(_token);
                if (status != ValueTaskSourceStatus.Pending)
                {
                    try
                    {
                        // Get the result of the operation and return a task for it.
                        // If any exception occurred, propagate it
                        return Task.FromResult(t.GetResult(_token));

                        // If status is Faulted or Canceled, GetResult should throw.  But
                        // we can't guarantee every implementation will do the "right thing".
                        // If it doesn't throw, we just treat that as success and ignore
                        // the status.
                    }
                    catch (Exception exc)
                    {
                        if (status == ValueTaskSourceStatus.Canceled)
                        {
                            if (exc is OperationCanceledException oce)
                            {
                                var task = new TaskCompletionSource<TResult>();
                                task.TrySetCanceled(oce.CancellationToken);
                                return task.Task;
                            }

                            // Benign race condition to initialize cached task, as identity doesn't matter.
                            return s_canceledTask ??= Task.FromCanceled<TResult>(new CancellationToken(true));
                        }
                        else
                        {
                            return Task.FromException<TResult>(exc);
                        }
                    }
                }

                return new ValueTaskSourceAsTask(t, _token).Task;
            }

            /// <summary>Type used to create a <see cref="Task{TResult}"/> to represent a <see cref="IValueTaskSource{TResult}"/>.</summary>
            private sealed class ValueTaskSourceAsTask : TaskCompletionSource<TResult>
            {
                private static readonly Action<object?> s_completionAction = static state =>
                {
                    if (!(state is ValueTaskSourceAsTask vtst) ||
                        !(vtst._source is IValueTaskSource<TResult> source))
                    {
                        // This could only happen if the IValueTaskSource<TResult> passed the wrong state
                        // or if this callback were invoked multiple times such that the state
                        // was previously nulled out.
                        throw new ArgumentOutOfRangeException($"{state}");
                    }

                    vtst._source = null;
                    ValueTaskSourceStatus status = source.GetStatus(vtst._token);
                    try
                    {
                        vtst.TrySetResult(source.GetResult(vtst._token));
                    }
                    catch (Exception exc)
                    {
                        if (status == ValueTaskSourceStatus.Canceled)
                        {
                            if (exc is OperationCanceledException oce)
                            {
                                vtst.TrySetCanceled(oce.CancellationToken);
                            }
                            else
                            {
                                vtst.TrySetCanceled(new CancellationToken(true));
                            }
                        }
                        else
                        {
                            vtst.TrySetException(exc);
                        }
                    }
                };

                /// <summary>The associated <see cref="IValueTaskSource"/>.</summary>
                private IValueTaskSource<TResult>? _source;
                /// <summary>The token to pass through to operations on <see cref="_source"/></summary>
                private readonly short _token;

                public ValueTaskSourceAsTask(IValueTaskSource<TResult> source, short token)
                {
                    _source = source;
                    _token = token;
                    source.OnCompleted(s_completionAction, this, token, ValueTaskSourceOnCompletedFlags.None);
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a completed operation.</summary>
            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                    if (obj == null)
                    {
                        return true;
                    }

                    if (obj is Task<TResult> t)
                    {
                        return t.IsCompleted;
                    }

                    return Unsafe.As<IValueTaskSource<TResult>>(obj).GetStatus(_token) != ValueTaskSourceStatus.Pending;
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a successfully completed operation.</summary>
            public bool IsCompletedSuccessfully
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                    if (obj == null)
                    {
                        return true;
                    }

                    if (obj is Task<TResult> t)
                    {
                        return (System.Int32)t.Status == 5;
                    }

                    return Unsafe.As<IValueTaskSource<TResult>>(obj).GetStatus(_token) == ValueTaskSourceStatus.Succeeded;
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a failed operation.</summary>
            public bool IsFaulted
            {
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                    if (obj == null)
                    {
                        return false;
                    }

                    if (obj is Task<TResult> t)
                    {
                        return t.IsFaulted;
                    }

                    return Unsafe.As<IValueTaskSource<TResult>>(obj).GetStatus(_token) == ValueTaskSourceStatus.Faulted;
                }
            }

            /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> represents a canceled operation.</summary>
            /// <remarks>
            /// If the <see cref="ValueTask{TResult}"/> is backed by a result or by a <see cref="IValueTaskSource{TResult}"/>,
            /// this will always return false.  If it's backed by a <see cref="Task"/>, it'll return the
            /// value of the task's <see cref="Task.IsCanceled"/> property.
            /// </remarks>
            public bool IsCanceled
            {
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                    if (obj == null)
                    {
                        return false;
                    }

                    if (obj is Task<TResult> t)
                    {
                        return t.IsCanceled;
                    }

                    return Unsafe.As<IValueTaskSource<TResult>>(obj).GetStatus(_token) == ValueTaskSourceStatus.Canceled;
                }
            }

            /// <summary>Gets the result.</summary>
            [DebuggerBrowsable(DebuggerBrowsableState.Never)] // prevent debugger evaluation from invalidating an underling IValueTaskSource<T>
            public TResult Result
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    object? obj = _obj;
                    Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                    if (obj == null)
                    {
                        return _result!;
                    }

                    if (obj is Task<TResult> t)
                    {
                        TaskAwaiter<TResult> TA = t.GetAwaiter();
                        return TA.GetResult();
                    }

                    return Unsafe.As<IValueTaskSource<TResult>>(obj).GetResult(_token);
                }
            }

            /// <summary>Gets an awaiter for this <see cref="ValueTask{TResult}"/>.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueTaskAwaiter<TResult> GetAwaiter() => new ValueTaskAwaiter<TResult>(this);

            /// <summary>Configures an awaiter for this <see cref="ValueTask{TResult}"/>.</summary>
            /// <param name="continueOnCapturedContext">
            /// true to attempt to marshal the continuation back to the captured context; otherwise, false.
            /// </param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfiguredValueTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) =>
                new ConfiguredValueTaskAwaitable<TResult>(new ValueTask<TResult>(_obj, _result, _token, continueOnCapturedContext));

            /// <summary>Gets a string-representation of this <see cref="ValueTask{TResult}"/>.</summary>
            public override string? ToString()
            {
                if (IsCompletedSuccessfully)
                {
                    Debugger.NotifyOfCrossThreadDependency(); // prevent debugger evaluation from invalidating an underling IValueTaskSource<T> unless forced

                    TResult result = Result;
                    if (result != null)
                    {
                        return result.ToString();
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>Provides a set of static methods for configuring <see cref="T:System.Threading.Tasks.Task" />-related behaviors on asynchronous enumerables and disposables.</summary>
        public static class TaskAsyncEnumerableExtensions
        {
            /// <summary>Configures how awaits on the tasks returned from an async disposable will be performed.</summary>
            /// <param name="source">The source async disposable.</param>
            /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
            /// <returns>The configured async disposable.</returns>
            public static ConfiguredAsyncDisposable ConfigureAwait(this IAsyncDisposable source, bool continueOnCapturedContext)
            {
                return new ConfiguredAsyncDisposable(source, continueOnCapturedContext);
            }

            /// <summary>Configures how awaits on the tasks returned from an async iteration will be performed.</summary>
            /// <typeparam name="T">The type of the objects being iterated.</typeparam>
            /// <param name="source">The source enumerable being iterated.</param>
            /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
            /// <returns>The configured enumerable.</returns>
            public static ConfiguredCancelableAsyncEnumerable<T> ConfigureAwait<T>(this IAsyncEnumerable<T> source, bool continueOnCapturedContext)
            {
                return new ConfiguredCancelableAsyncEnumerable<T>(source, continueOnCapturedContext, default(CancellationToken));
            }

            /// <summary>Sets the <see cref="T:System.Threading.CancellationToken" /> to be passed to <see cref="M:System.Collections.Generic.IAsyncEnumerable`1.GetAsyncEnumerator(System.Threading.CancellationToken)" /> when iterating.</summary>
            /// <typeparam name="T">The type of the objects being iterated.</typeparam>
            /// <param name="source">The source enumerable being iterated.</param>
            /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to use.</param>
            /// <returns>The configured enumerable.</returns>
            public static ConfiguredCancelableAsyncEnumerable<T> WithCancellation<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
            {
                return new ConfiguredCancelableAsyncEnumerable<T>(source, continueOnCapturedContext: true, cancellationToken);
            }
        }

        /// <summary>
        /// Provides support for efficiently using Tasks to implement the APM (Begin/End) pattern.
        /// </summary>
        internal static class TaskToApm
        {
            /// <summary>Provides a simple IAsyncResult that wraps a Task.</summary>
            /// <remarks>
            /// We could use the Task as the IAsyncResult if the Task's AsyncState is the same as the object state,
            /// but that's very rare, in particular in a situation where someone cares about allocation, and always
            /// using TaskAsyncResult simplifies things and enables additional optimizations.
            /// </remarks>
            internal sealed class TaskAsyncResult : IAsyncResult
            {
                /// <summary>The wrapped Task.</summary>
                internal readonly Task _task;

                /// <summary>Callback to invoke when the wrapped task completes.</summary>
                private readonly AsyncCallback _callback;

                /// <summary>Gets a user-defined object that qualifies or contains information about an asynchronous operation.</summary>
                public object AsyncState { get; }

                /// <summary>Gets a value that indicates whether the asynchronous operation completed synchronously.</summary>
                /// <remarks>This is set lazily based on whether the <see cref="F:System.Threading.Tasks.TaskToApm.TaskAsyncResult._task" /> has completed by the time this object is created.</remarks>
                public bool CompletedSynchronously { get; }

                /// <summary>Gets a value that indicates whether the asynchronous operation has completed.</summary>
                public bool IsCompleted => _task.IsCompleted;

                /// <summary>Gets a <see cref="T:System.Threading.WaitHandle" /> that is used to wait for an asynchronous operation to complete.</summary>
                public WaitHandle AsyncWaitHandle => ((IAsyncResult)_task).AsyncWaitHandle;

                /// <summary>Initializes the IAsyncResult with the Task to wrap and the associated object state.</summary>
                /// <param name="task">The Task to wrap.</param>
                /// <param name="state">The new AsyncState value.</param>
                /// <param name="callback">Callback to invoke when the wrapped task completes.</param>
                internal TaskAsyncResult(Task task, object state, AsyncCallback callback)
                {
                    _task = task;
                    AsyncState = state;
                    if (task.IsCompleted)
                    {
                        CompletedSynchronously = true;
                        callback?.Invoke(this);
                    }
                    else if (callback != null)
                    {
                        _callback = callback;
                        _task.ConfigureAwait(continueOnCapturedContext: false).GetAwaiter().OnCompleted(InvokeCallback);
                    }
                }

                /// <summary>Invokes the callback.</summary>
                private void InvokeCallback()
                {
                    _callback(this);
                }
            }

            /// <summary>
            /// Marshals the Task as an IAsyncResult, using the supplied callback and state
            /// to implement the APM pattern.
            /// </summary>
            /// <param name="task">The Task to be marshaled.</param>
            /// <param name="callback">The callback to be invoked upon completion.</param>
            /// <param name="state">The state to be stored in the IAsyncResult.</param>
            /// <returns>An IAsyncResult to represent the task's asynchronous operation.</returns>
            public static IAsyncResult Begin(Task task, AsyncCallback callback, object state)
            {
                return new TaskAsyncResult(task, state, callback);
            }

            /// <summary>Processes an IAsyncResult returned by Begin.</summary>
            /// <param name="asyncResult">The IAsyncResult to unwrap.</param>
            public static void End(IAsyncResult asyncResult)
            {
                Task task = GetTask(asyncResult);
                if (task != null)
                {
                    task.GetAwaiter().GetResult();
                }
                else
                {
                    ThrowArgumentException(asyncResult);
                }
            }

            /// <summary>Processes an IAsyncResult returned by Begin.</summary>
            /// <param name="asyncResult">The IAsyncResult to unwrap.</param>
            public static TResult End<TResult>(IAsyncResult asyncResult)
            {
                if (GetTask(asyncResult) is Task<TResult> task)
                {
                    return task.GetAwaiter().GetResult();
                }
                ThrowArgumentException(asyncResult);
                return default(TResult);
            }

            /// <summary>Gets the task represented by the IAsyncResult.</summary>
            public static Task GetTask(IAsyncResult asyncResult)
            {
                return (asyncResult as TaskAsyncResult)?._task;
            }

            /// <summary>Throws an argument exception for the invalid <paramref name="asyncResult" />.</summary>
            [System.Diagnostics.CodeAnalysis.DoesNotReturn]
            private static void ThrowArgumentException(IAsyncResult asyncResult)
            {
                throw (asyncResult == null) ? new ArgumentNullException("asyncResult") : new ArgumentException(null, "asyncResult");
            }
        }

    }
    #nullable disable

    /// <summary>The exception that is thrown when the post-phase action of a <see cref="System.Threading.Barrier" /> fails</summary>
    public class BarrierPostPhaseException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="System.Threading.BarrierPostPhaseException" /> class with a system-supplied message that describes the error.</summary>
        public BarrierPostPhaseException()
            : this((string)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="System.Threading.BarrierPostPhaseException" /> class with the specified inner exception.</summary>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public BarrierPostPhaseException(Exception innerException)
            : this(null, innerException)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="System.Threading.BarrierPostPhaseException" /> class with a specified message that describes the error.</summary>
        /// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
        public BarrierPostPhaseException(string message)
            : this(message, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="System.Threading.BarrierPostPhaseException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
        /// <param name="message">The message that describes the exception. The caller of this constructor is required to ensure that this string has been localized for the current system culture. </param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
        public BarrierPostPhaseException(string message, Exception innerException)
            : base((message == null) ? MDCFR.Properties.Resources.BarrierPostPhaseException : message, innerException)
        {
        }
    }

    [System.Diagnostics.Tracing.EventSource(Name = "System.Threading.SynchronizationEventSource", Guid = "EC631D38-466B-4290-9306-834971BA0217")]
    internal sealed class CdsSyncEtwBCLProvider : System.Diagnostics.Tracing.EventSource
    {
        public static System.Threading.CdsSyncEtwBCLProvider Log = new System.Threading.CdsSyncEtwBCLProvider();

        private const System.Diagnostics.Tracing.EventKeywords ALL_KEYWORDS = System.Diagnostics.Tracing.EventKeywords.All;

        private const int SPINLOCK_FASTPATHFAILED_ID = 1;

        private const int SPINWAIT_NEXTSPINWILLYIELD_ID = 2;

        private const int BARRIER_PHASEFINISHED_ID = 3;

        private CdsSyncEtwBCLProvider()
        {
        }

        [System.Diagnostics.Tracing.Event(1, Level = System.Diagnostics.Tracing.EventLevel.Warning)]
        public void SpinLock_FastPathFailed(int ownerID)
        {
            if (IsEnabled(System.Diagnostics.Tracing.EventLevel.Warning, System.Diagnostics.Tracing.EventKeywords.All))
            {
                WriteEvent(1, ownerID);
            }
        }

        [System.Diagnostics.Tracing.Event(2, Level = System.Diagnostics.Tracing.EventLevel.Informational)]
        public void SpinWait_NextSpinWillYield()
        {
            if (IsEnabled(System.Diagnostics.Tracing.EventLevel.Informational, System.Diagnostics.Tracing.EventKeywords.All))
            {
                WriteEvent(2);
            }
        }

        [System.Security.SecuritySafeCritical]
        [System.Diagnostics.Tracing.Event(3, Level = System.Diagnostics.Tracing.EventLevel.Verbose, Version = 1)]
        public unsafe void Barrier_PhaseFinished(bool currentSense, long phaseNum)
        {
            if (IsEnabled(System.Diagnostics.Tracing.EventLevel.Verbose, System.Diagnostics.Tracing.EventKeywords.All))
            {
                EventData* ptr = stackalloc EventData[2];
                int num = (currentSense ? 1 : 0);
                ptr->Size = 4;
                ptr->DataPointer = (IntPtr)(&num);
                ptr[1].Size = 8;
                ptr[1].DataPointer = (IntPtr)(&phaseNum);
                WriteEventCore(3, 2, ptr);
            }
        }
    }

    internal static class Helpers
    {
        internal static void Sleep(int milliseconds) { Thread.Sleep(milliseconds); }

        internal static void Spin(int iterations) { Thread.SpinWait(iterations); }
    }

    /// <summary>Specifies whether a lock can be entered multiple times by the same thread.</summary>
	public enum LockRecursionPolicy
    {
        /// <summary>If a thread tries to enter a lock recursively, an exception is thrown. Some classes may allow certain recursions when this setting is in effect. </summary>
        NoRecursion,
        /// <summary>A thread can enter a lock recursively. Some classes may restrict this capability. </summary>
        SupportsRecursion
    }

    internal class ReaderWriterCount
    {
        public long lockID;

        public int readercount;

        public int writercount;

        public int upgradecount;

        public ReaderWriterCount next;
    }

    /// <summary>Represents a lock that is used to manage access to a resource, allowing multiple threads for reading or exclusive access for writing.</summary>
	public class ReaderWriterLockSlim : IDisposable
    {
        private struct TimeoutTracker
        {
            private int _total;

            private int _start;

            public int RemainingMilliseconds
            {
                get
                {
                    if (_total == -1 || _total == 0)
                    {
                        return _total;
                    }
                    int num = Environment.TickCount - _start;
                    if (num < 0 || num >= _total)
                    {
                        return 0;
                    }
                    return _total - num;
                }
            }

            public bool IsExpired => RemainingMilliseconds == 0;

            public TimeoutTracker(TimeSpan timeout)
            {
                long num = (long)timeout.TotalMilliseconds;
                if (num < -1 || num > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("timeout");
                }
                _total = (int)num;
                if (_total != -1 && _total != 0)
                {
                    _start = Environment.TickCount;
                }
                else
                {
                    _start = 0;
                }
            }

            public TimeoutTracker(int millisecondsTimeout)
            {
                if (millisecondsTimeout < -1)
                {
                    throw new ArgumentOutOfRangeException("millisecondsTimeout");
                }
                _total = millisecondsTimeout;
                if (_total != -1 && _total != 0)
                {
                    _start = Environment.TickCount;
                }
                else
                {
                    _start = 0;
                }
            }
        }

        private bool _fIsReentrant;

        private int _myLock;

        private const int LockSpinCycles = 20;

        private const int LockSpinCount = 10;

        private const int LockSleep0Count = 5;

        private uint _numWriteWaiters;

        private uint _numReadWaiters;

        private uint _numWriteUpgradeWaiters;

        private uint _numUpgradeWaiters;

        private bool _fNoWaiters;

        private int _upgradeLockOwnerId;

        private int _writeLockOwnerId;

        private EventWaitHandle _writeEvent;

        private EventWaitHandle _readEvent;

        private EventWaitHandle _upgradeEvent;

        private EventWaitHandle _waitUpgradeEvent;

        private static long s_nextLockID;

        private long _lockID;

        [ThreadStatic]
        private static ReaderWriterCount t_rwc;

        private bool _fUpgradeThreadHoldingRead;

        private const int MaxSpinCount = 20;

        private uint _owners;

        private const uint WRITER_HELD = 2147483648u;

        private const uint WAITING_WRITERS = 1073741824u;

        private const uint WAITING_UPGRADER = 536870912u;

        private const uint MAX_READER = 268435454u;

        private const uint READER_MASK = 268435455u;

        private bool _fDisposed;

        /// <summary>Gets a value that indicates whether the current thread has entered the lock in read mode.</summary>
        /// <returns>true if the current thread has entered read mode; otherwise, false.</returns>
        /// <filterpriority>2</filterpriority>
        public bool IsReadLockHeld
        {
            get
            {
                if (RecursiveReadCount > 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>Gets a value that indicates whether the current thread has entered the lock in upgradeable mode. </summary>
        /// <returns>true if the current thread has entered upgradeable mode; otherwise, false.</returns>
        /// <filterpriority>2</filterpriority>
        public bool IsUpgradeableReadLockHeld
        {
            get
            {
                if (RecursiveUpgradeCount > 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>Gets a value that indicates whether the current thread has entered the lock in write mode.</summary>
        /// <returns>true if the current thread has entered write mode; otherwise, false.</returns>
        /// <filterpriority>2</filterpriority>
        public bool IsWriteLockHeld
        {
            get
            {
                if (RecursiveWriteCount > 0)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>Gets a value that indicates the recursion policy for the current <see cref="System.Threading.ReaderWriterLockSlim" /> object.</summary>
        /// <returns>One of the enumeration values that specifies the lock recursion policy.</returns>
        public LockRecursionPolicy RecursionPolicy
        {
            get
            {
                if (_fIsReentrant)
                {
                    return LockRecursionPolicy.SupportsRecursion;
                }
                return LockRecursionPolicy.NoRecursion;
            }
        }

        /// <summary>Gets the total number of unique threads that have entered the lock in read mode.</summary>
        /// <returns>The number of unique threads that have entered the lock in read mode.</returns>
        public int CurrentReadCount
        {
            get
            {
                int numReaders = (int)GetNumReaders();
                if (_upgradeLockOwnerId != -1)
                {
                    return numReaders - 1;
                }
                return numReaders;
            }
        }

        /// <summary>Gets the number of times the current thread has entered the lock in read mode, as an indication of recursion.</summary>
        /// <returns>0 (zero) if the current thread has not entered read mode, 1 if the thread has entered read mode but has not entered it recursively, or n if the thread has entered the lock recursively n - 1 times.</returns>
        /// <filterpriority>2</filterpriority>
        public int RecursiveReadCount
        {
            get
            {
                int result = 0;
                ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
                if (threadRWCount != null)
                {
                    result = threadRWCount.readercount;
                }
                return result;
            }
        }

        /// <summary>Gets the number of times the current thread has entered the lock in upgradeable mode, as an indication of recursion.</summary>
        /// <returns>0 if the current thread has not entered upgradeable mode, 1 if the thread has entered upgradeable mode but has not entered it recursively, or n if the thread has entered upgradeable mode recursively n - 1 times.</returns>
        /// <filterpriority>2</filterpriority>
        public int RecursiveUpgradeCount
        {
            get
            {
                if (_fIsReentrant)
                {
                    int result = 0;
                    ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
                    if (threadRWCount != null)
                    {
                        result = threadRWCount.upgradecount;
                    }
                    return result;
                }
                if (Environment.CurrentManagedThreadId == _upgradeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>Gets the number of times the current thread has entered the lock in write mode, as an indication of recursion.</summary>
        /// <returns>0 if the current thread has not entered write mode, 1 if the thread has entered write mode but has not entered it recursively, or n if the thread has entered write mode recursively n - 1 times.</returns>
        /// <filterpriority>2</filterpriority>
        public int RecursiveWriteCount
        {
            get
            {
                if (_fIsReentrant)
                {
                    int result = 0;
                    ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
                    if (threadRWCount != null)
                    {
                        result = threadRWCount.writercount;
                    }
                    return result;
                }
                if (Environment.CurrentManagedThreadId == _writeLockOwnerId)
                {
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>Gets the total number of threads that are waiting to enter the lock in read mode.</summary>
        /// <returns>The total number of threads that are waiting to enter read mode.</returns>
        /// <filterpriority>2</filterpriority>
        public int WaitingReadCount => (int)_numReadWaiters;

        /// <summary>Gets the total number of threads that are waiting to enter the lock in upgradeable mode.</summary>
        /// <returns>The total number of threads that are waiting to enter upgradeable mode.</returns>
        /// <filterpriority>2</filterpriority>
        public int WaitingUpgradeCount => (int)_numUpgradeWaiters;

        /// <summary>Gets the total number of threads that are waiting to enter the lock in write mode.</summary>
        /// <returns>The total number of threads that are waiting to enter write mode.</returns>
        /// <filterpriority>2</filterpriority>
        public int WaitingWriteCount => (int)_numWriteWaiters;

        private void InitializeThreadCounts()
        {
            _upgradeLockOwnerId = -1;
            _writeLockOwnerId = -1;
        }

        /// <summary>Initializes a new instance of the <see cref="System.Threading.ReaderWriterLockSlim" /> class with default property values.</summary>
        public ReaderWriterLockSlim() : this(LockRecursionPolicy.NoRecursion) { }

        /// <summary>Initializes a new instance of the <see cref="System.Threading.ReaderWriterLockSlim" /> class, specifying the lock recursion policy.</summary>
        /// <param name="recursionPolicy">One of the enumeration values that specifies the lock recursion policy. </param>
        public ReaderWriterLockSlim(LockRecursionPolicy recursionPolicy)
        {
            if (recursionPolicy == LockRecursionPolicy.SupportsRecursion)
            {
                _fIsReentrant = true;
            }
            InitializeThreadCounts();
            _fNoWaiters = true;
            _lockID = Interlocked.Increment(ref s_nextLockID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRWEntryEmpty(ReaderWriterCount rwc)
        {
            if (rwc.lockID == 0L)
            {
                return true;
            }
            if (rwc.readercount == 0 && rwc.writercount == 0 && rwc.upgradecount == 0)
            {
                return true;
            }
            return false;
        }

        private bool IsRwHashEntryChanged(ReaderWriterCount lrwc) { return lrwc.lockID != _lockID; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReaderWriterCount GetThreadRWCount(bool dontAllocate)
        {
            ReaderWriterCount next = t_rwc;
            ReaderWriterCount readerWriterCount = null;
            while (next != null)
            {
                if (next.lockID == _lockID)
                {
                    return next;
                }
                if (!dontAllocate && readerWriterCount == null && IsRWEntryEmpty(next))
                {
                    readerWriterCount = next;
                }
                next = next.next;
            }
            if (dontAllocate)
            {
                return null;
            }
            if (readerWriterCount == null)
            {
                readerWriterCount = new ReaderWriterCount();
                readerWriterCount.next = t_rwc;
                t_rwc = readerWriterCount;
            }
            readerWriterCount.lockID = _lockID;
            return readerWriterCount;
        }

        /// <summary>Tries to enter the lock in read mode.</summary>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is 
        /// <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread has 
        /// already entered read mode. -or-The current thread may not acquire the read lock when it already 
        /// holds the write lock. -or-The recursion number would exceed the capacity of the counter. This limit is
        /// so large that applications should never encounter it. </exception>
        /// <exception cref="System.ObjectDisposedException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public void EnterReadLock() { TryEnterReadLock(-1); }

        /// <summary>Tries to enter the lock in read mode, with an optional time-out.</summary>
        /// <returns>true if the calling thread entered read mode, otherwise, false.</returns>
        /// <param name="timeout">The interval to wait, or -1 milliseconds to wait indefinitely. </param>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is 
        /// <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> and the current thread 
        /// has already entered the lock. -or-The recursion number would exceed the capacity of the 
        /// counter. The limit is so large that applications should never encounter it.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> 
        /// is negative, but it is not equal to -1 milliseconds, which is the only negative value allowed.-or-The value of 
        /// <paramref name="timeout" /> is greater than <see cref="System.Int32.MaxValue" /> milliseconds. </exception>
        /// <exception cref="System.ObjectDisposedException">The <see cref="System.Threading.ReaderWriterLockSlim" /> 
        /// object has been disposed. </exception>
        public bool TryEnterReadLock(TimeSpan timeout) { return TryEnterReadLock(new TimeoutTracker(timeout)); }

        /// <summary>Tries to enter the lock in read mode, with an optional integer time-out.</summary>
        /// <returns>true if the calling thread entered read mode, otherwise, false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 
        /// (<see cref="System.Threading.Timeout.Infinite" />) to wait indefinitely.</param>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is 
        /// <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> and the current 
        /// thread has already entered the lock. -or-The recursion number would exceed the 
        /// capacity of the counter. The limit is so large that applications should never encounter it.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The value of 
        /// <paramref name="millisecondsTimeout" /> is negative, but it is not equal to 
        /// <see cref="System.Threading.Timeout.Infinite" /> (-1), which is the only 
        /// negative value allowed. </exception>
        /// <exception cref="System.ObjectDisposedException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public bool TryEnterReadLock(int millisecondsTimeout) { return TryEnterReadLock(new TimeoutTracker(millisecondsTimeout)); }

        private bool TryEnterReadLock(TimeoutTracker timeout) { return TryEnterReadLockCore(timeout); }

        private bool TryEnterReadLockCore(TimeoutTracker timeout)
        {
            if (_fDisposed)
            {
                throw new ObjectDisposedException(null);
            }
            ReaderWriterCount readerWriterCount = null;
            int currentManagedThreadId = Environment.CurrentManagedThreadId;
            if (!_fIsReentrant)
            {
                if (currentManagedThreadId == _writeLockOwnerId)
                {
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_ReadAfterWriteNotAllowed);
                }
                EnterMyLock();
                readerWriterCount = GetThreadRWCount(dontAllocate: false);
                if (readerWriterCount.readercount > 0)
                {
                    ExitMyLock();
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_RecursiveReadNotAllowed);
                }
                if (currentManagedThreadId == _upgradeLockOwnerId)
                {
                    readerWriterCount.readercount++;
                    _owners++;
                    ExitMyLock();
                    return true;
                }
            }
            else
            {
                EnterMyLock();
                readerWriterCount = GetThreadRWCount(dontAllocate: false);
                if (readerWriterCount.readercount > 0)
                {
                    readerWriterCount.readercount++;
                    ExitMyLock();
                    return true;
                }
                if (currentManagedThreadId == _upgradeLockOwnerId)
                {
                    readerWriterCount.readercount++;
                    _owners++;
                    ExitMyLock();
                    _fUpgradeThreadHoldingRead = true;
                    return true;
                }
                if (currentManagedThreadId == _writeLockOwnerId)
                {
                    readerWriterCount.readercount++;
                    _owners++;
                    ExitMyLock();
                    return true;
                }
            }
            bool flag = true;
            int num = 0;
            while (true)
            {
                if (_owners < 268435454)
                {
                    _owners++;
                    readerWriterCount.readercount++;
                    ExitMyLock();
                    return flag;
                }
                if (num < 20)
                {
                    ExitMyLock();
                    if (timeout.IsExpired)
                    {
                        return false;
                    }
                    num++;
                    SpinWait(num);
                    EnterMyLock();
                    if (IsRwHashEntryChanged(readerWriterCount))
                    {
                        readerWriterCount = GetThreadRWCount(dontAllocate: false);
                    }
                }
                else if (_readEvent == null)
                {
                    LazyCreateEvent(ref _readEvent, makeAutoResetEvent: false);
                    if (IsRwHashEntryChanged(readerWriterCount))
                    {
                        readerWriterCount = GetThreadRWCount(dontAllocate: false);
                    }
                }
                else
                {
                    flag = WaitOnEvent(_readEvent, ref _numReadWaiters, timeout, isWriteWaiter: false);
                    if (!flag)
                    {
                        break;
                    }
                    if (IsRwHashEntryChanged(readerWriterCount))
                    {
                        readerWriterCount = GetThreadRWCount(dontAllocate: false);
                    }
                }
            }
            return false;
        }

        /// <summary>Tries to enter the lock in write mode.</summary>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is 
        /// <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> and the current 
        /// thread has already entered the lock in any mode. -or-The current thread has entered 
        /// read mode, so trying to enter the lock in write mode would create the possibility of a deadlock. 
        /// -or-The recursion number would exceed the capacity of the counter. The limit is so large that
        /// applications should never encounter it.</exception>
        /// <exception cref="System.ObjectDisposedException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public void EnterWriteLock() { TryEnterWriteLock(-1); }

        /// <summary>Tries to enter the lock in write mode, with an optional time-out.</summary>
        /// <returns>true if the calling thread entered write mode, otherwise, false.</returns>
        /// <param name="timeout">The interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is 
        /// <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> and the current 
        /// thread has already entered the lock. -or-The current thread initially entered the lock 
        /// in read mode, and therefore trying to enter write mode would create the possibility of a
        /// deadlock. -or-The recursion number would exceed the capacity of the counter. The limit is
        /// so large that applications should never encounter it.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The value of 
        /// <paramref name="timeout" /> is negative, but it is not equal to -1 milliseconds, which is the 
        /// only negative value allowed. -or-The value of <paramref name="timeout" /> is greater than 
        /// <see cref="System.Int32.MaxValue" /> milliseconds. </exception>
        /// <exception cref="System.ObjectDisposedException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public bool TryEnterWriteLock(TimeSpan timeout) { return TryEnterWriteLock(new TimeoutTracker(timeout)); }

        /// <summary>Tries to enter the lock in write mode, with an optional time-out.</summary>
        /// <returns>true if the calling thread entered write mode, otherwise, false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 
        /// (<see cref="System.Threading.Timeout.Infinite" />) to wait indefinitely.</param>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> 
        /// property is <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> 
        /// and the current thread has already entered the lock. -or-The current thread 
        /// initially entered the lock in read mode, and therefore trying to enter write mode 
        /// would create the possibility of a deadlock. -or-The recursion number would exceed 
        /// the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The value of 
        /// <paramref name="millisecondsTimeout" /> is negative, but it is not equal to 
        /// <see cref="System.Threading.Timeout.Infinite" /> (-1), which is the only negative value allowed. </exception>
        /// <exception cref="System.ObjectDisposedException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public bool TryEnterWriteLock(int millisecondsTimeout) { return TryEnterWriteLock(new TimeoutTracker(millisecondsTimeout)); }

        private bool TryEnterWriteLock(TimeoutTracker timeout) { return TryEnterWriteLockCore(timeout); }

        private bool TryEnterWriteLockCore(TimeoutTracker timeout)
        {
            if (_fDisposed) { throw new ObjectDisposedException(null); }
            int currentManagedThreadId = Environment.CurrentManagedThreadId;
            bool flag = false;
            ReaderWriterCount threadRWCount;
            if (!_fIsReentrant)
            {
                if (currentManagedThreadId == _writeLockOwnerId)
                {
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_RecursiveWriteNotAllowed);
                }
                if (currentManagedThreadId == _upgradeLockOwnerId)
                {
                    flag = true;
                }
                EnterMyLock();
                threadRWCount = GetThreadRWCount(dontAllocate: true);
                if (threadRWCount != null && threadRWCount.readercount > 0)
                {
                    ExitMyLock();
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_WriteAfterReadNotAllowed);
                }
            }
            else
            {
                EnterMyLock();
                threadRWCount = GetThreadRWCount(dontAllocate: false);
                if (currentManagedThreadId == _writeLockOwnerId)
                {
                    threadRWCount.writercount++;
                    ExitMyLock();
                    return true;
                }
                if (currentManagedThreadId == _upgradeLockOwnerId)
                {
                    flag = true;
                }
                else if (threadRWCount.readercount > 0)
                {
                    ExitMyLock();
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_WriteAfterReadNotAllowed);
                }
            }
            int num = 0;
            bool flag2 = true;
            while (true)
            {
                if (IsWriterAcquired())
                {
                    SetWriterAcquired();
                    break;
                }
                if (flag)
                {
                    uint numReaders = GetNumReaders();
                    if (numReaders == 1)
                    {
                        SetWriterAcquired();
                        break;
                    }
                    if (numReaders == 2 && threadRWCount != null)
                    {
                        if (IsRwHashEntryChanged(threadRWCount))
                        {
                            threadRWCount = GetThreadRWCount(dontAllocate: false);
                        }
                        if (threadRWCount.readercount > 0)
                        {
                            SetWriterAcquired();
                            break;
                        }
                    }
                }
                if (num < 20)
                {
                    ExitMyLock();
                    if (timeout.IsExpired)
                    {
                        return false;
                    }
                    num++;
                    SpinWait(num);
                    EnterMyLock();
                }
                else if (flag)
                {
                    if (_waitUpgradeEvent == null)
                    {
                        LazyCreateEvent(ref _waitUpgradeEvent, makeAutoResetEvent: true);
                    }
                    else if (!WaitOnEvent(_waitUpgradeEvent, ref _numWriteUpgradeWaiters, timeout, isWriteWaiter: true))
                    {
                        return false;
                    }
                }
                else if (_writeEvent == null)
                {
                    LazyCreateEvent(ref _writeEvent, makeAutoResetEvent: true);
                }
                else if (!WaitOnEvent(_writeEvent, ref _numWriteWaiters, timeout, isWriteWaiter: true))
                {
                    return false;
                }
            }
            if (_fIsReentrant)
            {
                if (IsRwHashEntryChanged(threadRWCount))
                {
                    threadRWCount = GetThreadRWCount(dontAllocate: false);
                }
                threadRWCount.writercount++;
            }
            ExitMyLock();
            _writeLockOwnerId = currentManagedThreadId;
            return true;
        }

        /// <summary>Tries to enter the lock in upgradeable mode.</summary>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> 
        /// property is <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> 
        /// and the current thread has already entered the lock in any mode. -or-The 
        /// current thread has entered read mode, so trying to enter upgradeable mode 
        /// would create the possibility of a deadlock. -or-The recursion number would 
        /// exceed the capacity of the counter. The limit is so large that applications should never encounter it.</exception>
        /// <exception cref="System.ObjectDisposedException">The
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public void EnterUpgradeableReadLock() { TryEnterUpgradeableReadLock(-1); }

        /// <summary>Tries to enter the lock in upgradeable mode, with an optional time-out.</summary>
        /// <returns>true if the calling thread entered upgradeable mode, otherwise, false.</returns>
        /// <param name="timeout">The interval to wait, or -1 milliseconds to wait indefinitely.</param>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> property is 
        /// <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> and the current 
        /// thread has already entered the lock. -or-The current thread initially entered the 
        /// lock in read mode, and therefore trying to enter upgradeable mode would create the 
        /// possibility of a deadlock. -or-The recursion number would exceed the capacity of 
        /// the counter. The limit is so large that applications should never encounter it.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The value of 
        /// <paramref name="timeout" /> is negative, but it is not equal to -1 milliseconds,
        /// which is the only negative value allowed.-or-The value of <paramref name="timeout" /> 
        /// is greater than <see cref="System.Int32.MaxValue" /> milliseconds. </exception>
        /// <exception cref="System.ObjectDisposedException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public bool TryEnterUpgradeableReadLock(TimeSpan timeout) { return TryEnterUpgradeableReadLock(new TimeoutTracker(timeout)); }

        /// <summary>Tries to enter the lock in upgradeable mode, with an optional time-out.</summary>
        /// <returns>true if the calling thread entered upgradeable mode, otherwise, false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or -1 
        /// (<see cref="System.Threading.Timeout.Infinite" />) to wait indefinitely.</param>
        /// <exception cref="System.Threading.LockRecursionException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim.RecursionPolicy" /> 
        /// property is <see cref="System.Threading.LockRecursionPolicy.NoRecursion" /> 
        /// and the current thread has already entered the lock. -or-The current thread 
        /// initially entered the lock in read mode, and therefore trying to enter upgradeable 
        /// mode would create the possibility of a deadlock. -or-The recursion number would 
        /// exceed the capacity of the counter. The limit is so large that applications should 
        /// never encounter it.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The value of 
        /// <paramref name="millisecondsTimeout" /> is negative, but it is not equal to 
        /// <see cref="System.Threading.Timeout.Infinite" /> (-1), which is the only 
        /// negative value allowed. </exception>
        /// <exception cref="System.ObjectDisposedException">The 
        /// <see cref="System.Threading.ReaderWriterLockSlim" /> object has been disposed. </exception>
        public bool TryEnterUpgradeableReadLock(int millisecondsTimeout) { return TryEnterUpgradeableReadLock(new TimeoutTracker(millisecondsTimeout)); }

        private bool TryEnterUpgradeableReadLock(TimeoutTracker timeout) { return TryEnterUpgradeableReadLockCore(timeout); }

        private bool TryEnterUpgradeableReadLockCore(TimeoutTracker timeout)
        {
            if (_fDisposed) { throw new ObjectDisposedException(null); }
            int currentManagedThreadId = Environment.CurrentManagedThreadId;
            ReaderWriterCount threadRWCount;
            if (!_fIsReentrant)
            {
                if (currentManagedThreadId == _upgradeLockOwnerId)
                {
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_RecursiveUpgradeNotAllowed);
                }
                if (currentManagedThreadId == _writeLockOwnerId)
                {
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_UpgradeAfterWriteNotAllowed);
                }
                EnterMyLock();
                threadRWCount = GetThreadRWCount(dontAllocate: true);
                if (threadRWCount != null && threadRWCount.readercount > 0)
                {
                    ExitMyLock();
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_UpgradeAfterReadNotAllowed);
                }
            }
            else
            {
                EnterMyLock();
                threadRWCount = GetThreadRWCount(dontAllocate: false);
                if (currentManagedThreadId == _upgradeLockOwnerId)
                {
                    threadRWCount.upgradecount++;
                    ExitMyLock();
                    return true;
                }
                if (currentManagedThreadId == _writeLockOwnerId)
                {
                    _owners++;
                    _upgradeLockOwnerId = currentManagedThreadId;
                    threadRWCount.upgradecount++;
                    if (threadRWCount.readercount > 0)
                    {
                        _fUpgradeThreadHoldingRead = true;
                    }
                    ExitMyLock();
                    return true;
                }
                if (threadRWCount.readercount > 0)
                {
                    ExitMyLock();
                    throw new LockRecursionException(MDCFR.Properties.Resources.LockRecursionException_UpgradeAfterReadNotAllowed);
                }
            }
            bool flag = true;
            int num = 0;
            while (true)
            {
                if (_upgradeLockOwnerId == -1 && _owners < 268435454)
                {
                    _owners++;
                    _upgradeLockOwnerId = currentManagedThreadId;
                    if (_fIsReentrant)
                    {
                        if (IsRwHashEntryChanged(threadRWCount))
                        {
                            threadRWCount = GetThreadRWCount(dontAllocate: false);
                        }
                        threadRWCount.upgradecount++;
                    }
                    break;
                }
                if (num < 20)
                {
                    ExitMyLock();
                    if (timeout.IsExpired)
                    {
                        return false;
                    }
                    num++;
                    SpinWait(num);
                    EnterMyLock();
                }
                else if (_upgradeEvent == null)
                {
                    LazyCreateEvent(ref _upgradeEvent, makeAutoResetEvent: true);
                }
                else if (!WaitOnEvent(_upgradeEvent, ref _numUpgradeWaiters, timeout, isWriteWaiter: false))
                {
                    return false;
                }
            }
            ExitMyLock();
            return true;
        }

        /// <summary>Reduces the recursion count for read mode, and exits read mode if the resulting count is 0 (zero).</summary>
        /// <exception cref="System.Threading.SynchronizationLockException">The current thread has not entered the 
        /// lock in read mode. </exception>
        public void ExitReadLock()
        {
            ReaderWriterCount readerWriterCount = null;
            EnterMyLock();
            readerWriterCount = GetThreadRWCount(dontAllocate: true);
            if (readerWriterCount == null || readerWriterCount.readercount < 1)
            {
                ExitMyLock();
                throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_MisMatchedRead);
            }
            if (_fIsReentrant)
            {
                if (readerWriterCount.readercount > 1)
                {
                    readerWriterCount.readercount--;
                    ExitMyLock();
                    return;
                }
                if (Environment.CurrentManagedThreadId == _upgradeLockOwnerId)
                {
                    _fUpgradeThreadHoldingRead = false;
                }
            }
            _owners--;
            readerWriterCount.readercount--;
            ExitAndWakeUpAppropriateWaiters();
        }

        /// <summary>Reduces the recursion count for write mode, and exits write mode if the resulting count is 0 (zero).</summary>
        /// <exception cref="System.Threading.SynchronizationLockException">The current thread has not entered the 
        /// lock in write mode.</exception>
        public void ExitWriteLock()
        {
            if (!_fIsReentrant)
            {
                if (Environment.CurrentManagedThreadId != _writeLockOwnerId)
                {
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_MisMatchedWrite);
                }
                EnterMyLock();
            }
            else
            {
                EnterMyLock();
                ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: false);
                if (threadRWCount == null)
                {
                    ExitMyLock();
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_MisMatchedWrite);
                }
                if (threadRWCount.writercount < 1)
                {
                    ExitMyLock();
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_MisMatchedWrite);
                }
                threadRWCount.writercount--;
                if (threadRWCount.writercount > 0)
                {
                    ExitMyLock();
                    return;
                }
            }
            ClearWriterAcquired();
            _writeLockOwnerId = -1;
            ExitAndWakeUpAppropriateWaiters();
        }

        /// <summary>Reduces the recursion count for upgradeable mode, and exits upgradeable mode if the resulting count is 0 (zero).</summary>
        /// <exception cref="System.Threading.SynchronizationLockException">The current thread has not entered the lock in 
        /// upgradeable mode.</exception>
        public void ExitUpgradeableReadLock()
        {
            if (!_fIsReentrant)
            {
                if (Environment.CurrentManagedThreadId != _upgradeLockOwnerId)
                {
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_MisMatchedUpgrade);
                }
                EnterMyLock();
            }
            else
            {
                EnterMyLock();
                ReaderWriterCount threadRWCount = GetThreadRWCount(dontAllocate: true);
                if (threadRWCount == null)
                {
                    ExitMyLock();
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_MisMatchedUpgrade);
                }
                if (threadRWCount.upgradecount < 1)
                {
                    ExitMyLock();
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_MisMatchedUpgrade);
                }
                threadRWCount.upgradecount--;
                if (threadRWCount.upgradecount > 0)
                {
                    ExitMyLock();
                    return;
                }
                _fUpgradeThreadHoldingRead = false;
            }
            _owners--;
            _upgradeLockOwnerId = -1;
            ExitAndWakeUpAppropriateWaiters();
        }

        private void LazyCreateEvent(ref EventWaitHandle waitEvent, bool makeAutoResetEvent)
        {
            ExitMyLock();
            EventWaitHandle eventWaitHandle = ((!makeAutoResetEvent) ? ((EventWaitHandle)new ManualResetEvent(initialState: false)) : ((EventWaitHandle)new AutoResetEvent(initialState: false)));
            EnterMyLock();
            if (waitEvent == null) { waitEvent = eventWaitHandle; } else { eventWaitHandle.Dispose(); }
        }

        private bool WaitOnEvent(EventWaitHandle waitEvent, ref uint numWaiters, TimeoutTracker timeout, bool isWriteWaiter)
        {
            waitEvent.Reset();
            numWaiters++;
            _fNoWaiters = false;
            if (_numWriteWaiters == 1) { SetWritersWaiting(); }
            if (_numWriteUpgradeWaiters == 1) { SetUpgraderWaiting(); }
            bool flag = false;
            ExitMyLock();
            try
            {
                flag = waitEvent.WaitOne(timeout.RemainingMilliseconds);
            }
            finally
            {
                EnterMyLock();
                numWaiters--;
                if (_numWriteWaiters == 0 && _numWriteUpgradeWaiters == 0 && _numUpgradeWaiters == 0 && _numReadWaiters == 0)
                {
                    _fNoWaiters = true;
                }
                if (_numWriteWaiters == 0) { ClearWritersWaiting(); }
                if (_numWriteUpgradeWaiters == 0) { ClearUpgraderWaiting(); }
                if (!flag)
                {
                    if (isWriteWaiter)
                    {
                        ExitAndWakeUpAppropriateReadWaiters();
                    } else { ExitMyLock(); }
                }
            }
            return flag;
        }

        private void ExitAndWakeUpAppropriateWaiters()
        {
            if (_fNoWaiters) { ExitMyLock(); }
            else
            {
                ExitAndWakeUpAppropriateWaitersPreferringWriters();
            }
        }

        private void ExitAndWakeUpAppropriateWaitersPreferringWriters()
        {
            uint numReaders = GetNumReaders();
            if (_fIsReentrant && _numWriteUpgradeWaiters != 0 && _fUpgradeThreadHoldingRead && numReaders == 2)
            {
                ExitMyLock();
                _waitUpgradeEvent.Set();
            }
            else if (numReaders == 1 && _numWriteUpgradeWaiters != 0)
            {
                ExitMyLock();
                _waitUpgradeEvent.Set();
            }
            else if (numReaders == 0 && _numWriteWaiters != 0)
            {
                ExitMyLock();
                _writeEvent.Set();
            } else { ExitAndWakeUpAppropriateReadWaiters(); }
        }

        private void ExitAndWakeUpAppropriateReadWaiters()
        {
            if (_numWriteWaiters != 0 || _numWriteUpgradeWaiters != 0 || _fNoWaiters)
            {
                ExitMyLock();
                return;
            }
            bool flag = _numReadWaiters != 0;
            bool flag2 = _numUpgradeWaiters != 0 && _upgradeLockOwnerId == -1;
            ExitMyLock();
            if (flag) { _readEvent.Set(); }
            if (flag2) { _upgradeEvent.Set(); }
        }

        private bool IsWriterAcquired() { return (_owners & 0xBFFFFFFFu) == 0; }

        private void SetWriterAcquired() { _owners |= 2147483648u; }

        private void ClearWriterAcquired() { _owners &= 2147483647u; }

        private void SetWritersWaiting() { _owners |= 1073741824u; }

        private void ClearWritersWaiting() { _owners &= 3221225471u; }

        private void SetUpgraderWaiting() { _owners |= 536870912u; }

        private void ClearUpgraderWaiting() { _owners &= 3758096383u; }

        private uint GetNumReaders() { return _owners & 0xFFFFFFFu; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnterMyLock() { if (Interlocked.CompareExchange(ref _myLock, 1, 0) != 0) { EnterMyLockSpin(); } }

        private void EnterMyLockSpin()
        {
            int processorCount = Environment.ProcessorCount;
            int num = 0;
            while (true)
            {
                if (num < 10 && processorCount > 1) { Helpers.Spin(20 * (num + 1)); } else if (num < 15) { Helpers.Sleep(0); } else { Helpers.Sleep(1); }
                if (_myLock == 0 && Interlocked.CompareExchange(ref _myLock, 1, 0) == 0) { break; }
                num++;
            }
        }

        private void ExitMyLock() { Volatile.Write(ref _myLock, 0); }

        private static void SpinWait(int SpinCount)
        {
            if (SpinCount < 5 && Environment.ProcessorCount > 1) { Helpers.Spin(20 * SpinCount); } else if (SpinCount < 17) { Helpers.Sleep(0); } else { Helpers.Sleep(1); }
        }

        /// <summary>Releases all resources used by the current instance of the <see cref="System.Threading.ReaderWriterLockSlim" /> class.</summary>
        /// <exception cref="System.Threading.SynchronizationLockException">
        ///   <see cref="System.Threading.ReaderWriterLockSlim.WaitingReadCount" /> is greater than zero. -or-
        ///   <see cref="System.Threading.ReaderWriterLockSlim.WaitingUpgradeCount" /> is greater than zero. -or-
        ///   <see cref="System.Threading.ReaderWriterLockSlim.WaitingWriteCount" /> is greater than zero. </exception>
        /// <filterpriority>2</filterpriority>
        public void Dispose() { Dispose(disposing: true); }

        private void Dispose(bool disposing)
        {
            if (disposing && !_fDisposed)
            {
                if (WaitingReadCount > 0 || WaitingUpgradeCount > 0 || WaitingWriteCount > 0)
                {
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_IncorrectDispose);
                }
                if (IsReadLockHeld || IsUpgradeableReadLockHeld || IsWriteLockHeld)
                {
                    throw new SynchronizationLockException(MDCFR.Properties.Resources.SynchronizationLockException_IncorrectDispose);
                }
                if (_writeEvent != null)
                {
                    _writeEvent.Dispose();
                    _writeEvent = null;
                }
                if (_readEvent != null)
                {
                    _readEvent.Dispose();
                    _readEvent = null;
                }
                if (_upgradeEvent != null)
                {
                    _upgradeEvent.Dispose();
                    _upgradeEvent = null;
                }
                if (_waitUpgradeEvent != null)
                {
                    _waitUpgradeEvent.Dispose();
                    _waitUpgradeEvent = null;
                }
                _fDisposed = true;
            }
        }
    }

    /// <summary>Enables multiple tasks to cooperatively work on an algorithm in parallel through multiple phases.</summary>
	[DebuggerDisplay("Participant Count={ParticipantCount},Participants Remaining={ParticipantsRemaining}")]
    public class Barrier : IDisposable
    {
        private volatile int _currentTotalCount;

        private const int CURRENT_MASK = 2147418112;

        private const int TOTAL_MASK = 32767;

        private const int SENSE_MASK = int.MinValue;

        private const int MAX_PARTICIPANTS = 32767;

        private long _currentPhase;

        private bool _disposed;

        private ManualResetEventSlim _oddEvent;

        private ManualResetEventSlim _evenEvent;

        private ExecutionContext _ownerThreadContext;

        [System.Security.SecurityCritical]
        private static ContextCallback s_invokePostPhaseAction;

        private Action<Barrier> _postPhaseAction;

        private Exception _exception;

        private int _actionCallerID;

        /// <summary>Gets the number of participants in the barrier that haven’t yet signaled in the current phase.</summary>
        /// <returns>Returns the number of participants in the barrier that haven’t yet signaled in the current phase.</returns>
        public int ParticipantsRemaining
        {
            get
            {
                int currentTotalCount = _currentTotalCount;
                int num = currentTotalCount & 0x7FFF;
                int num2 = (currentTotalCount & 0x7FFF0000) >> 16;
                return num - num2;
            }
        }

        /// <summary>Gets the total number of participants in the barrier.</summary>
        /// <returns>Returns the total number of participants in the barrier.</returns>
        public int ParticipantCount => _currentTotalCount & 0x7FFF;

        /// <summary>Gets the number of the barrier's current phase.</summary>
        /// <returns>Returns the number of the barrier's current phase.</returns>
        public long CurrentPhaseNumber
        {
            get { return Volatile.Read(ref _currentPhase); }
            internal set { Volatile.Write(ref _currentPhase, value); }
        }

        /// <summary>Initializes a new instance of the <see cref="System.Threading.Barrier" /> class.</summary>
        /// <param name="participantCount">The number of participating threads.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="participantCount" /> is less than 0 or greater than 32,767.</exception>
        public Barrier(int participantCount) : this(participantCount, null) { }

        /// <summary>Initializes a new instance of the <see cref="System.Threading.Barrier" /> class.</summary>
        /// <param name="participantCount">The number of participating threads.</param>
        /// <param name="postPhaseAction">The <see cref="System.Action{T}" /> to be executed after 
        /// each phase. null (Nothing in Visual Basic) may be passed to indicate no action is taken.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="participantCount" /> is less than 0 or greater than 32,767.</exception>
        public Barrier(int participantCount, Action<Barrier> postPhaseAction)
        {
            if (participantCount < 0 || participantCount > 32767)
            {
                throw new ArgumentOutOfRangeException("participantCount", participantCount, MDCFR.Properties.Resources.Barrier_ctor_ArgumentOutOfRange);
            }
            _currentTotalCount = participantCount;
            _postPhaseAction = postPhaseAction;
            _oddEvent = new ManualResetEventSlim(initialState: true);
            _evenEvent = new ManualResetEventSlim(initialState: false);
            if (postPhaseAction != null)
            {
                _ownerThreadContext = ExecutionContext.Capture();
            }
            _actionCallerID = 0;
        }

        private void GetCurrentTotal(int currentTotal, out int current, out int total, out bool sense)
        {
            total = currentTotal & 0x7FFF;
            current = (currentTotal & 0x7FFF0000) >> 16;
            sense = (((currentTotal & int.MinValue) == 0) ? true : false);
        }

        private bool SetCurrentTotal(int currentTotal, int current, int total, bool sense)
        {
            int num = (current << 16) | total;
            if (!sense) { num |= int.MinValue; }
            return Interlocked.CompareExchange(ref _currentTotalCount, num, currentTotal) == currentTotal;
        }

        /// <summary>Notifies the <see cref="System.Threading.Barrier" /> that there will be an additional participant.</summary>
        /// <returns>The phase number of the barrier in which the new participants will first participate.</returns>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">Adding a participant would cause the barrier's 
        /// participant count to exceed 32,767.-or-The method was invoked from within a post-phase action.</exception>
        public long AddParticipant()
        {
            try { return AddParticipants(1); }
            catch (ArgumentOutOfRangeException)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_AddParticipants_Overflow_ArgumentOutOfRange);
            }
        }

        /// <summary>Notifies the <see cref="System.Threading.Barrier" /> that there will be additional participants.</summary>
        /// <returns>The phase number of the barrier in which the new participants will first participate.</returns>
        /// <param name="participantCount">The number of additional participants to add to the barrier.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="participantCount" /> is less than 0.-or-Adding <paramref name="participantCount" /> 
        ///  participants would cause the barrier's participant count to exceed 32,767.</exception>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase action.</exception>
        public long AddParticipants(int participantCount)
        {
            ThrowIfDisposed();
            if (participantCount < 1)
            {
                throw new ArgumentOutOfRangeException("participantCount", participantCount, MDCFR.Properties.Resources.Barrier_AddParticipants_NonPositive_ArgumentOutOfRange);
            }
            if (participantCount > 32767)
            {
                throw new ArgumentOutOfRangeException("participantCount", MDCFR.Properties.Resources.Barrier_AddParticipants_Overflow_ArgumentOutOfRange);
            }
            if (_actionCallerID != 0 && Environment.CurrentManagedThreadId == _actionCallerID)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_InvalidOperation_CalledFromPHA);
            }
            SpinWait spinWait = default(SpinWait);
            long num = 0L;
            bool sense;
            while (true)
            {
                int currentTotalCount = _currentTotalCount;
                GetCurrentTotal(currentTotalCount, out var current, out var total, out sense);
                if (participantCount + total > 32767)
                {
                    throw new ArgumentOutOfRangeException("participantCount", MDCFR.Properties.Resources.Barrier_AddParticipants_Overflow_ArgumentOutOfRange);
                }
                if (SetCurrentTotal(currentTotalCount, current, total + participantCount, sense))
                {
                    break;
                }
                spinWait.SpinOnce();
            }
            long currentPhaseNumber = CurrentPhaseNumber;
            num = ((sense != (currentPhaseNumber % 2 == 0)) ? (currentPhaseNumber + 1) : currentPhaseNumber);
            if (num != currentPhaseNumber)
            {
                if (sense)
                {
                    _oddEvent.Wait();
                }
                else
                {
                    _evenEvent.Wait();
                }
            }
            else if (sense && _evenEvent.IsSet)
            {
                _evenEvent.Reset();
            }
            else if (!sense && _oddEvent.IsSet)
            {
                _oddEvent.Reset();
            }
            return num;
        }

        /// <summary>Notifies the <see cref="System.Threading.Barrier" /> that there will be one less participant.</summary>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">The barrier already has 0 participants.-or-The method was invoked from within a post-phase action.</exception>
        public void RemoveParticipant() { RemoveParticipants(1); }

        /// <summary>Notifies the <see cref="System.Threading.Barrier" /> that there will be fewer participants.</summary>
        /// <param name="participantCount">The number of additional participants to remove from the barrier.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="participantCount" /> is less than 0.</exception>
        /// <exception cref="System.InvalidOperationException">The barrier already has 0 participants.-or-The method was 
        /// invoked from within a post-phase action. -or-current participant count is less than the specified participantCount.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">The total participant count is less than the specified 
        /// <paramref name=" participantCount" /></exception>
        public void RemoveParticipants(int participantCount)
        {
            ThrowIfDisposed();
            if (participantCount < 1)
            {
                throw new ArgumentOutOfRangeException("participantCount", participantCount, MDCFR.Properties.Resources.Barrier_RemoveParticipants_NonPositive_ArgumentOutOfRange);
            }
            if (_actionCallerID != 0 && Environment.CurrentManagedThreadId == _actionCallerID)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_InvalidOperation_CalledFromPHA);
            }
            SpinWait spinWait = default(SpinWait);
            while (true)
            {
                int currentTotalCount = _currentTotalCount;
                GetCurrentTotal(currentTotalCount, out var current, out var total, out var sense);
                if (total < participantCount)
                {
                    throw new ArgumentOutOfRangeException("participantCount", MDCFR.Properties.Resources.Barrier_RemoveParticipants_ArgumentOutOfRange);
                }
                if (total - participantCount < current)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_RemoveParticipants_InvalidOperation);
                }
                int num = total - participantCount;
                if (num > 0 && current == num)
                {
                    if (SetCurrentTotal(currentTotalCount, 0, total - participantCount, !sense))
                    {
                        FinishPhase(sense);
                        break;
                    }
                }
                else if (SetCurrentTotal(currentTotalCount, current, total - participantCount, sense))
                {
                    break;
                }
                spinWait.SpinOnce();
            }
        }

        /// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier as well.</summary>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier 
        /// currently has 0 participants, or the barrier is signaled by more threads than are registered as participants.</exception>
        /// <exception cref="System.Threading.BarrierPostPhaseException">If an exception is thrown from the post phase action of a 
        /// Barrier after all participating threads have called SignalAndWait, the exception will be wrapped in a BarrierPostPhaseException 
        /// and be thrown on all participating threads.</exception>
        public void SignalAndWait() { SignalAndWait(default(CancellationToken)); }

        /// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier, while observing a cancellation token.</summary>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="System.OperationCanceledException">
        ///   <paramref name="cancellationToken" /> has been canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase action, the barrier currently has 0 participants, or the 
        /// barrier is signaled by more threads than are registered as participants.</exception>
        public void SignalAndWait(CancellationToken cancellationToken) { SignalAndWait(-1, cancellationToken); }

        /// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier 
        /// as well, using a <see cref="System.TimeSpan" /> object to measure the time interval.</summary>
        /// <returns>true if all other participants reached the barrier; otherwise, false.</returns>
        /// <param name="timeout">A <see cref="T:System.TimeSpan" /> that represents the number of milliseconds to wait, or a 
        /// <see cref="System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="timeout" />is a negative number other than -1 milliseconds, which represents an infinite time-out, 
        ///   or it is greater than 32,767.</exception>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase action, 
        /// the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered 
        /// as participants.</exception>
        public bool SignalAndWait(TimeSpan timeout) { return SignalAndWait(timeout, default(CancellationToken)); }

        /// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach 
        /// the barrier as well, using a <see cref="System.TimeSpan" /> object to measure the time interval, while 
        /// observing a cancellation token.</summary>
        /// <returns>true if all other participants reached the barrier; otherwise, false.</returns>
        /// <param name="timeout">A <see cref="System.TimeSpan" /> that represents the number of milliseconds to wait,
        /// or a <see cref="System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="System.OperationCanceledException">
        /// <paramref name="cancellationToken" /> has been canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="timeout" />is a negative number other than -1 milliseconds, which represents an infinite time-out.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase action, the
        /// barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as 
        /// participants.</exception>
        public bool SignalAndWait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long num = (long)timeout.TotalMilliseconds;
            if (num < -1 || num > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, MDCFR.Properties.Resources.Barrier_SignalAndWait_ArgumentOutOfRange);
            }
            return SignalAndWait((int)timeout.TotalMilliseconds, cancellationToken);
        }

        /// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier
        /// as well, using a 32-bit signed integer to measure the timeout.</summary>
        /// <returns>if all participants reached the barrier within the specified time; otherwise false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="System.Threading.Timeout.Infinite" />
        /// (-1) to wait indefinitely.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite
        /// time-out.</exception>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase 
        /// action, the barrier currently has 0 participants, or the barrier is signaled by more threads than are registered 
        /// as participants.</exception>
        /// <exception cref="System.Threading.BarrierPostPhaseException">If an exception is thrown from the post phase 
        /// action of a Barrier after all participating threads have called SignalAndWait, the exception will be wrapped in a 
        /// BarrierPostPhaseException and be thrown on all participating threads.</exception>
        public bool SignalAndWait(int millisecondsTimeout) { return SignalAndWait(millisecondsTimeout, default(CancellationToken)); }

        /// <summary>Signals that a participant has reached the barrier and waits for all other participants to reach the barrier as
        /// well, using a 32-bit signed integer to measure the timeout, while observing a cancellation token.</summary>
        /// <returns>if all participants reached the barrier within the specified time; otherwise false</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or <see cref="System.Threading.Timeout.Infinite" />
        /// (-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="System.OperationCanceledException">
        /// <paramref name="cancellationToken" /> has been canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase action, the 
        /// barrier currently has 0 participants, or the barrier is signaled by more threads than are registered as
        /// participants.</exception>
        public bool SignalAndWait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, MDCFR.Properties.Resources.Barrier_SignalAndWait_ArgumentOutOfRange);
            }
            if (_actionCallerID != 0 && Environment.CurrentManagedThreadId == _actionCallerID)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_InvalidOperation_CalledFromPHA);
            }
            SpinWait spinWait = default(SpinWait);
            int current;
            int total;
            bool sense;
            long currentPhaseNumber;
            while (true)
            {
                int currentTotalCount = _currentTotalCount;
                GetCurrentTotal(currentTotalCount, out current, out total, out sense);
                currentPhaseNumber = CurrentPhaseNumber;
                if (total == 0)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_SignalAndWait_InvalidOperation_ZeroTotal);
                }
                if (current == 0 && sense != (CurrentPhaseNumber % 2 == 0))
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_SignalAndWait_InvalidOperation_ThreadsExceeded);
                }
                if (current + 1 == total)
                {
                    if (SetCurrentTotal(currentTotalCount, 0, total, !sense))
                    {
                        if (System.Threading.CdsSyncEtwBCLProvider.Log.IsEnabled())
                        {
                            System.Threading.CdsSyncEtwBCLProvider.Log.Barrier_PhaseFinished(sense, CurrentPhaseNumber);
                        }
                        FinishPhase(sense);
                        return true;
                    }
                }
                else if (SetCurrentTotal(currentTotalCount, current + 1, total, sense))
                {
                    break;
                }
                spinWait.SpinOnce();
            }
            ManualResetEventSlim currentPhaseEvent = (sense ? _evenEvent : _oddEvent);
            bool flag = false;
            bool flag2 = false;
            try
            {
                flag2 = DiscontinuousWait(currentPhaseEvent, millisecondsTimeout, cancellationToken, currentPhaseNumber);
            }
            catch (OperationCanceledException)
            {
                flag = true;
            }
            catch (ObjectDisposedException)
            {
                if (currentPhaseNumber >= CurrentPhaseNumber)
                {
                    throw;
                }
                flag2 = true;
            }
            if (!flag2)
            {
                spinWait.Reset();
                while (true)
                {
                    int currentTotalCount = _currentTotalCount;
                    GetCurrentTotal(currentTotalCount, out current, out total, out var sense2);
                    if (currentPhaseNumber < CurrentPhaseNumber || sense != sense2)
                    {
                        break;
                    }
                    if (SetCurrentTotal(currentTotalCount, current - 1, total, sense))
                    {
                        if (flag)
                        {
                            throw new OperationCanceledException(MDCFR.Properties.Resources.Common_OperationCanceled, cancellationToken);
                        }
                        return false;
                    }
                    spinWait.SpinOnce();
                }
                WaitCurrentPhase(currentPhaseEvent, currentPhaseNumber);
            }
            if (_exception != null)
            {
                throw new BarrierPostPhaseException(_exception);
            }
            return true;
        }

        [System.Security.SecuritySafeCritical]
        private void FinishPhase(bool observedSense)
        {
            if (_postPhaseAction != null)
            {
                try
                {
                    _actionCallerID = Environment.CurrentManagedThreadId;
                    if (_ownerThreadContext != null)
                    {
                        ExecutionContext ownerThreadContext = _ownerThreadContext;
                        ContextCallback callback = InvokePostPhaseAction;
                        ExecutionContext.Run(_ownerThreadContext, callback, this);
                    }
                    else
                    {
                        _postPhaseAction(this);
                    }
                    _exception = null;
                    return;
                }
                catch (Exception exception)
                {
                    _exception = exception;
                    return;
                }
                finally
                {
                    _actionCallerID = 0;
                    SetResetEvents(observedSense);
                    if (_exception != null)
                    {
                        throw new BarrierPostPhaseException(_exception);
                    }
                }
            }
            SetResetEvents(observedSense);
        }

        [System.Security.SecurityCritical]
        private static void InvokePostPhaseAction(object obj)
        {
            Barrier barrier = (Barrier)obj;
            barrier._postPhaseAction(barrier);
        }

        private void SetResetEvents(bool observedSense)
        {
            CurrentPhaseNumber++;
            if (observedSense) { _oddEvent.Reset(); _evenEvent.Set(); } else { _evenEvent.Reset(); _oddEvent.Set(); }
        }

        private void WaitCurrentPhase(ManualResetEventSlim currentPhaseEvent, long observedPhase)
        {
            SpinWait spinWait = default(SpinWait);
            while (!currentPhaseEvent.IsSet && CurrentPhaseNumber - observedPhase <= 1) { spinWait.SpinOnce(); }
        }

        private bool DiscontinuousWait(ManualResetEventSlim currentPhaseEvent, int totalTimeout, CancellationToken token, long observedPhase)
        {
            int num = 100;
            int num2 = 10000;
            while (observedPhase == CurrentPhaseNumber)
            {
                int num3 = ((totalTimeout == -1) ? num : Math.Min(num, totalTimeout));
                if (currentPhaseEvent.Wait(num3, token))
                {
                    return true;
                }
                if (totalTimeout != -1)
                {
                    totalTimeout -= num3;
                    if (totalTimeout <= 0)
                    {
                        return false;
                    }
                }
                num = ((num >= num2) ? num2 : Math.Min(num << 1, num2));
            }
            WaitCurrentPhase(currentPhaseEvent, observedPhase);
            return true;
        }

        /// <summary>Releases all resources used by the current instance of the <see cref="System.Threading.Barrier" /> class.</summary>
        /// <exception cref="System.InvalidOperationException">The method was invoked from within a post-phase action.</exception>
        public void Dispose()
        {
            if (_actionCallerID != 0 && Environment.CurrentManagedThreadId == _actionCallerID)
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.Barrier_InvalidOperation_CalledFromPHA);
            }
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="System.Threading.Barrier" />, and optionally releases 
        /// the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged 
        /// resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing) { _oddEvent.Dispose(); _evenEvent.Dispose(); }
                _disposed = true;
            }
        }

        private void ThrowIfDisposed() { if (_disposed) { throw new ObjectDisposedException("Barrier", MDCFR.Properties.Resources.Barrier_Dispose); } }
    }

    /// <summary>Represents a synchronization primitive that is signaled when its count reaches zero.</summary>
	[DebuggerDisplay("Initial Count={InitialCount}, Current Count={CurrentCount}")]
    public class CountdownEvent : IDisposable
    {
        private int _initialCount;

        private volatile int _currentCount;

        private ManualResetEventSlim _event;

        private volatile bool _disposed;

        /// <summary>Gets the number of remaining signals required to set the event.</summary>
        /// <returns> The number of remaining signals required to set the event.</returns>
        public int CurrentCount
        {
            get
            {
                int currentCount = _currentCount;
                if (currentCount >= 0) { return currentCount; }
                return 0;
            }
        }

        /// <summary>Gets the numbers of signals initially required to set the event.</summary>
        /// <returns> The number of signals initially required to set the event.</returns>
        public int InitialCount => _initialCount;

        /// <summary>Determines whether the event is set.</summary>
        /// <returns>true if the event is set; otherwise, false.</returns>
        public bool IsSet => _currentCount <= 0;

        /// <summary>Gets a <see cref="System.Threading.WaitHandle" /> that is used to wait for the event to be set.</summary>
        /// <returns>A <see cref="System.Threading.WaitHandle" /> that is used to wait for the event to be set.</returns>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        public WaitHandle WaitHandle { get { ThrowIfDisposed(); return _event.WaitHandle; } }

        /// <summary>Initializes a new instance of <see cref="System.Threading.CountdownEvent" /> class with the specified count.</summary>
        /// <param name="initialCount">The number of signals initially required to set the <see cref="System.Threading.CountdownEvent" />.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="initialCount" /> is less than 0.</exception>
        public CountdownEvent(int initialCount)
        {
            if (initialCount < 0) { throw new ArgumentOutOfRangeException("initialCount"); }
            _initialCount = initialCount;
            _currentCount = initialCount;
            _event = new ManualResetEventSlim();
            if (initialCount == 0) { _event.Set(); }
        }

        /// <summary>Releases all resources used by the current instance of the <see cref="System.Threading.CountdownEvent" /> class.</summary>
        public void Dispose() { Dispose(disposing: true); GC.SuppressFinalize(this); }

        /// <summary>Releases the unmanaged resources used by the <see cref="System.Threading.CountdownEvent" />, and optionally releases 
        /// the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) { if (disposing) { _event.Dispose(); _disposed = true; } }

        /// <summary>Registers a signal with the <see cref="System.Threading.CountdownEvent" />, decrementing the 
        /// value of <see cref="System.Threading.CountdownEvent.CurrentCount" />.</summary>
        /// <returns>true if the signal caused the count to reach zero and the event was set; otherwise, false.</returns>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">The current instance is already set.</exception>
        public bool Signal()
        {
            ThrowIfDisposed();
            if (_currentCount <= 0) { throw new InvalidOperationException(MDCFR.Properties.Resources.CountdownEvent_Decrement_BelowZero); }
            int num = Interlocked.Decrement(ref _currentCount);
            if (num == 0) { _event.Set(); return true; }
            if (num < 0) { throw new InvalidOperationException(MDCFR.Properties.Resources.CountdownEvent_Decrement_BelowZero); }
            return false;
        }

        /// <summary>Registers multiple signals with the <see cref="System.Threading.CountdownEvent" />, decrementing 
        /// the value of <see cref="System.Threading.CountdownEvent.CurrentCount" /> by the specified amount.</summary>
        /// <returns>true if the signals caused the count to reach zero and the event was set; otherwise, false.</returns>
        /// <param name="signalCount">The number of signals to register.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="signalCount" /> is less than 1.</exception>
        /// <exception cref="System.InvalidOperationException">The current instance is already set. -or- 
        /// Or <paramref name="signalCount" /> is greater than <see cref="System.Threading.CountdownEvent.CurrentCount" />.
        /// </exception>
        public bool Signal(int signalCount)
        {
            if (signalCount <= 0) { throw new ArgumentOutOfRangeException("signalCount"); }
            ThrowIfDisposed();
            SpinWait spinWait = default(SpinWait);
            int currentCount;
            while (true)
            {
                currentCount = _currentCount;
                if (currentCount < signalCount)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CountdownEvent_Decrement_BelowZero);
                }
                if (Interlocked.CompareExchange(ref _currentCount, currentCount - signalCount, currentCount) == currentCount)
                {
                    break;
                }
                spinWait.SpinOnce();
            }
            if (currentCount == signalCount) { _event.Set(); return true; }
            return false;
        }

        /// <summary>Increments the <see cref="System.Threading.CountdownEvent" />'s current count by one.</summary>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">The current instance is already set.-or-
        /// <see cref="System.Threading.CountdownEvent.CurrentCount" /> is equal to or greater than 
        /// <see cref="System.Int32.MaxValue" />.</exception>
        public void AddCount() { AddCount(1); }

        /// <summary>Attempts to increment <see cref="System.Threading.CountdownEvent.CurrentCount" /> by one.</summary>
        /// <returns>true if the increment succeeded; otherwise, false. If 
        /// <see cref="System.Threading.CountdownEvent.CurrentCount" /> is already at zero, this method will return false.</returns>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.InvalidOperationException">
        /// <see cref="System.Threading.CountdownEvent.CurrentCount" /> is equal to <see cref="System.Int32.MaxValue" />.</exception>
        public bool TryAddCount() { return TryAddCount(1); }

        /// <summary>Increments the <see cref="System.Threading.CountdownEvent" />'s current count by a specified value.</summary>
        /// <param name="signalCount">The value by which to increase <see cref="System.Threading.CountdownEvent.CurrentCount" />.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="signalCount" /> is less than or equal to 0.</exception>
        /// <exception cref="System.InvalidOperationException">The current instance is already set.-or-
        /// <see cref="System.Threading.CountdownEvent.CurrentCount" /> is equal to or greater than 
        /// <see cref="System.Int32.MaxValue" /> after count is incremented by <paramref name="signalCount." /></exception>
        public void AddCount(int signalCount)
        {
            if (!TryAddCount(signalCount)) { throw new InvalidOperationException(MDCFR.Properties.Resources.CountdownEvent_Increment_AlreadyZero); }
        }

        /// <summary>Attempts to increment <see cref="System.Threading.CountdownEvent.CurrentCount" /> by a specified value.</summary>
        /// <returns>true if the increment succeeded; otherwise, false. If 
        /// <see cref="System.Threading.CountdownEvent.CurrentCount" /> is already at zero this will return false.</returns>
        /// <param name="signalCount">The value by which to increase <see cref="System.Threading.CountdownEvent.CurrentCount" />.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="signalCount" /> is less than or equal to 0.</exception>
        /// <exception cref="System.InvalidOperationException">The current instance is already set.-or-
        /// <see cref="System.Threading.CountdownEvent.CurrentCount" /> + <paramref name="signalCount" /> is equal to or greater 
        /// than <see cref="System.Int32.MaxValue" />.</exception>
        public bool TryAddCount(int signalCount)
        {
            if (signalCount <= 0)
            {
                throw new ArgumentOutOfRangeException("signalCount");
            }
            ThrowIfDisposed();
            SpinWait spinWait = default(SpinWait);
            while (true)
            {
                int currentCount = _currentCount;
                if (currentCount <= 0)
                {
                    return false;
                }
                if (currentCount > int.MaxValue - signalCount)
                {
                    throw new InvalidOperationException(MDCFR.Properties.Resources.CountdownEvent_Increment_AlreadyMax);
                }
                if (Interlocked.CompareExchange(ref _currentCount, currentCount + signalCount, currentCount) == currentCount)
                {
                    break;
                }
                spinWait.SpinOnce();
            }
            return true;
        }

        /// <summary>Resets the <see cref="System.Threading.CountdownEvent.CurrentCount" /> 
        /// to the value of <see cref="System.Threading.CountdownEvent.InitialCount" />.</summary>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed..</exception>
        public void Reset() { Reset(_initialCount); }

        /// <summary>Resets the <see cref="System.Threading.CountdownEvent.InitialCount" /> property to a specified value.</summary>
        /// <param name="count">The number of signals required to set the <see cref="System.Threading.CountdownEvent" />.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has alread been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="count" /> is less than 0.</exception>
        public void Reset(int count)
        {
            ThrowIfDisposed();
            if (count < 0) { throw new ArgumentOutOfRangeException("count"); }
            _currentCount = count;
            _initialCount = count;
            if (count == 0) { _event.Set(); } else { _event.Reset(); }
        }

        /// <summary>Blocks the current thread until the <see cref="System.Threading.CountdownEvent" /> is set.</summary>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        public void Wait() { Wait(-1, default(CancellationToken)); }

        /// <summary>Blocks the current thread until the <see cref="System.Threading.CountdownEvent" /> is set,
        /// while observing a <see cref="System.Threading.CancellationToken" />.</summary>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="System.OperationCanceledException">
        ///   <paramref name="cancellationToken" /> has been canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed. -or- 
        /// The <see cref="System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> 
        /// has already been disposed.</exception>
        public void Wait(CancellationToken cancellationToken) { Wait(-1, cancellationToken); }

        /// <summary>Blocks the current thread until the <see cref="System.Threading.CountdownEvent" /> is set, using a 
        /// <see cref="System.TimeSpan" /> to measure the timeout.</summary>
        /// <returns>true if the <see cref="System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
        /// <param name="timeout">A <see cref="System.TimeSpan" /> that represents the number of milliseconds to wait, or a 
        /// <see cref="System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out -or- 
        ///   timeout is greater than <see cref="System.Int32.MaxValue" />.</exception>
        public bool Wait(TimeSpan timeout)
        {
            long num = (long)timeout.TotalMilliseconds;
            if (num < -1 || num > int.MaxValue) { throw new ArgumentOutOfRangeException("timeout"); }
            return Wait((int)num, default(CancellationToken));
        }

        /// <summary>Blocks the current thread until the <see cref="System.Threading.CountdownEvent" /> is set, using a 
        /// <see cref="System.TimeSpan" /> to measure the timeout, while observing a 
        /// <see cref="System.Threading.CancellationToken" />.</summary>
        /// <returns>true if the <see cref="System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
        /// <param name="timeout">A <see cref="System.TimeSpan" /> that represents the number of milliseconds to wait, 
        /// or a <see cref="System.TimeSpan" /> that represents -1 milliseconds to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="System.OperationCanceledException">
        ///   <paramref name="cancellationToken" /> has been canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed. -or- The 
        /// <see cref="System.Threading.CancellationTokenSource" /> that created <paramref name="cancellationToken" /> 
        /// has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="timeout" /> is a negative number other than -1 milliseconds, which represents an infinite time-out
        ///   -or- timeout is greater than <see cref="System.Int32.MaxValue" />.</exception>
        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long num = (long)timeout.TotalMilliseconds;
            if (num < -1 || num > int.MaxValue) { throw new ArgumentOutOfRangeException("timeout"); }
            return Wait((int)num, cancellationToken);
        }

        /// <summary>Blocks the current thread until the <see cref="System.Threading.CountdownEvent" /> is set, using a 32-bit 
        /// signed integer to measure the timeout.</summary>
        /// <returns>true if the <see cref="System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or 
        /// <see cref="System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///   <paramref name="millisecondsTimeout" /> is a negative number other than -1, which represents an infinite time-out.
        ///   </exception>
        public bool Wait(int millisecondsTimeout)
        {
            return Wait(millisecondsTimeout, default(CancellationToken));
        }

        /// <summary>Blocks the current thread until the <see cref="System.Threading.CountdownEvent" /> is set, using a 
        /// 32-bit signed integer to measure the timeout, while observing a 
        /// <see cref="System.Threading.CancellationToken" />.</summary>
        /// <returns>true if the <see cref="System.Threading.CountdownEvent" /> was set; otherwise, false.</returns>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait, or 
        /// <see cref="System.Threading.Timeout.Infinite" />(-1) to wait indefinitely.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> to observe.</param>
        /// <exception cref="System.OperationCanceledException">
        /// <paramref name="cancellationToken" /> has been canceled.</exception>
        /// <exception cref="System.ObjectDisposedException">The current instance has already been disposed. -or- 
        /// The <see cref="System.Threading.CancellationTokenSource" /> that created 
        /// <paramref name="cancellationToken" /> has already been disposed.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout" /> is a negative number other than -1,
        /// which represents an infinite time-out.</exception>
        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            if (millisecondsTimeout < -1) { throw new ArgumentOutOfRangeException("millisecondsTimeout"); }
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            bool flag = IsSet;
            if (!flag) { flag = _event.Wait(millisecondsTimeout, cancellationToken); }
            return flag;
        }

        private void ThrowIfDisposed() { if (_disposed) { throw new ObjectDisposedException("CountdownEvent"); } }
    }

}