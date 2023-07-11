// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Buffers;
using System.Diagnostics;
using System.Buffers.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Sources;
#if NET6_0_OR_GREATER
	using System.Runtime.Intrinsics;
	using System.Runtime.Intrinsics.X86;
	using static System.Runtime.Intrinsics.X86.Ssse3;
#endif

namespace System
{
    #nullable enable
    namespace Threading.Tasks
    {

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
                        return (System.Int32) t.Status == 5;
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
                    EqualityComparer<TResult>.Default.Equals(_result, other._result);
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
                        return (System.Int32) t.Status == 5;
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

    }

    namespace Runtime.CompilerServices
    {

        /// <summary>Provides an awaiter for a <see cref="ValueTask{TResult}"/>.</summary>
        public readonly struct ValueTaskAwaiter<TResult> : ICriticalNotifyCompletion, INotifyCompletion
        {


            /// <summary>The value being awaited.</summary>
            private readonly ValueTask<TResult> _value;

            /// <summary>Initializes the awaiter.</summary>
            /// <param name="value">The value to be awaited.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueTaskAwaiter(in ValueTask<TResult> value) => _value = value;

            /// <summary>Gets whether the <see cref="ValueTask{TResult}"/> has completed.</summary>
            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _value.IsCompleted;
            }

            /// <summary>Gets the result of the ValueTask.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TResult GetResult() => _value.Result;

            /// <summary>Schedules the continuation action for this ValueTask.</summary>
            public void OnCompleted(Action continuation)
            {
                object? obj = _value._obj;
                Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                if (obj is Task<TResult> t)
                {
                    t.GetAwaiter().OnCompleted(continuation);
                }
                else if (obj != null)
                {
                    Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
                }
                else
                {
                    Task.CompletedTask.GetAwaiter().OnCompleted(continuation);
                }
            }

            /// <summary>Schedules the continuation action for this ValueTask.</summary>
            public void UnsafeOnCompleted(Action continuation)
            {
                object? obj = _value._obj;
                Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                if (obj is Task<TResult> t)
                {
                    t.GetAwaiter().UnsafeOnCompleted(continuation);
                }
                else if (obj != null)
                {
                    Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
                }
                else
                {
                    Task.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
                }
            }


        }

        /// <summary>Provides an awaiter for a <see cref="ValueTask"/>.</summary>
        public readonly struct ValueTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            /// <summary>Shim used to invoke an <see cref="Action"/> passed as the state argument to a <see cref="Action{Object}"/>.</summary>
            internal static readonly Action<object?> s_invokeActionDelegate = static state =>
            {
                if (!(state is Action action))
                {
                    throw new ArgumentOutOfRangeException($"{state}");
                }

                action();
            };

            /// <summary>The value being awaited.</summary>
            private readonly ValueTask _value;

            /// <summary>Initializes the awaiter.</summary>
            /// <param name="value">The value to be awaited.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ValueTaskAwaiter(in ValueTask value) => _value = value;

            /// <summary>Gets whether the <see cref="ValueTask"/> has completed.</summary>
            public bool IsCompleted
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _value.IsCompleted;
            }

            /// <summary>Gets the result of the ValueTask.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void GetResult() => _value.ThrowIfCompletedUnsuccessfully();

            /// <summary>Schedules the continuation action for this ValueTask.</summary>
            public void OnCompleted(Action continuation)
            {
                object? obj = _value._obj;
                Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                if (obj is Task t)
                {
                    t.GetAwaiter().OnCompleted(continuation);
                }
                else if (obj != null)
                {
                    Unsafe.As<IValueTaskSource>(obj).OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext | ValueTaskSourceOnCompletedFlags.FlowExecutionContext);
                }
                else
                {
                    Task.CompletedTask.GetAwaiter().OnCompleted(continuation);
                }
            }

            /// <summary>Schedules the continuation action for this ValueTask.</summary>
            public void UnsafeOnCompleted(Action continuation)
            {
                object? obj = _value._obj;
                Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                if (obj is Task t)
                {
                    t.GetAwaiter().UnsafeOnCompleted(continuation);
                }
                else if (obj != null)
                {
                    Unsafe.As<IValueTaskSource>(obj).OnCompleted(s_invokeActionDelegate, continuation, _value._token, ValueTaskSourceOnCompletedFlags.UseSchedulingContext);
                }
                else
                {
                    Task.CompletedTask.GetAwaiter().UnsafeOnCompleted(continuation);
                }
            }
        }
        
        #nullable disable

        /// <summary>
        /// Indicates the type of the async method builder that should be used by a language compiler to
        /// build the attributed async method or to build the attributed type when used as the return type
        /// of an async method.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        public sealed class AsyncMethodBuilderAttribute : Attribute
        {
            /// <summary>Initializes the <see cref="AsyncMethodBuilderAttribute"/>.</summary>
            /// <param name="builderType">The <see cref="Type"/> of the associated builder.</param>
            public AsyncMethodBuilderAttribute(Type builderType) => BuilderType = builderType;

            /// <summary>Gets the <see cref="Type"/> of the associated builder.</summary>
            public Type BuilderType { get; }
        }

        /// <summary>
        /// Calls to methods or references to fields marked with this attribute may be replaced at
        /// some call sites with jit intrinsic expansions.
        /// Types marked with this attribute may be specially treated by the runtime/compiler.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
        public sealed class IntrinsicAttribute : Attribute { }

        //Code required when the Snappy Archiving is compiled < .NET 6 .
        #if ! NET6_0_OR_GREATER
            // Licensed to the .NET Foundation under one or more agreements.
            // The .NET Foundation licenses this file to you under the MIT license.

            [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
            internal sealed class CallerArgumentExpressionAttribute : Attribute
            {
                public CallerArgumentExpressionAttribute(string parameterName)
                {
                    ParameterName = parameterName;
                }

                public string ParameterName { get; }
            }
#endif

        // Special internal struct that we use to signify that we are not interested in
        // a Task<VoidTaskResult>'s result.
        internal struct VoidTaskResult { }


        /// <summary>Represents a builder for asynchronous methods that return a <see cref="ValueTask"/>.</summary>
        [StructLayout(LayoutKind.Auto)]
        public struct AsyncValueTaskMethodBuilder
        {
            private System.Boolean _haveResult;
            private System.Runtime.CompilerServices.AsyncTaskMethodBuilder _methodBuilder;
            private System.Boolean _useBuilder;

            /// <summary>Creates an instance of the <see cref="AsyncValueTaskMethodBuilder"/> struct.</summary>
            /// <returns>The initialized instance.</returns>
            public static AsyncValueTaskMethodBuilder Create() => default;

            /// <summary>Begins running the builder with the associated state machine.</summary>
            /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
            /// <param name="stateMachine">The state machine instance, passed by reference.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
                _methodBuilder.Start(ref stateMachine);

            /// <summary>Associates the builder with the specified state machine.</summary>
            /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
            public void SetStateMachine(IAsyncStateMachine stateMachine) =>
                _methodBuilder.SetStateMachine(stateMachine);

            /// <summary>Marks the task as successfully completed.</summary>
            public void SetResult()
            {
                if (_useBuilder)
                {
                    _methodBuilder.SetResult();
                    return;
                }
                else
                {
                    _haveResult = true;
                }
            }

            /// <summary>Marks the task as failed and binds the specified exception to the task.</summary>
            /// <param name="exception">The exception to bind to the task.</param>
            public void SetException(Exception exception) => _methodBuilder.SetException(exception);

            /// <summary>Gets the task for this builder.</summary>
            public ValueTask Task
            {
                get
                {
                    if (_haveResult)
                    {
                        return default;
                    }
                    else 
                    {
                        _useBuilder = true;
                        Task task = _methodBuilder.Task;
                        return new ValueTask(task);
                    }

                    // With normal access paterns, m_task should always be non-null here: the async method should have
                    // either completed synchronously, in which case SetResult would have set m_task to a non-null object,
                    // or it should be completing asynchronously, in which case AwaitUnsafeOnCompleted would have similarly
                    // initialized m_task to a state machine object.  However, if the type is used manually (not via
                    // compiler-generated code) and accesses Task directly, we force it to be initialized.  Things will then
                    // "work" but in a degraded mode, as we don't know the TStateMachine type here, and thus we use a normal
                    // task object instead.
                }
            }

            /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
            /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
            /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
            /// <param name="awaiter">The awaiter.</param>
            /// <param name="stateMachine">The state machine.</param>
            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                _useBuilder = true;
                _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
            }

            /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
            /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
            /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
            /// <param name="awaiter">The awaiter.</param>
            /// <param name="stateMachine">The state machine.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                _useBuilder = true;
                _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
            }
        }

        /// <summary>Represents a builder for asynchronous methods that return a <see cref="ValueTask"/>.</summary>
        [StructLayout(LayoutKind.Auto)]
        public struct AsyncValueTaskMethodBuilder<TResult>
        {
            private System.Boolean _haveResult;
            private System.Runtime.CompilerServices.AsyncTaskMethodBuilder<TResult> _methodBuilder;
            private System.Boolean _useBuilder;
            private TResult _result;

            /// <summary>Creates an instance of the <see cref="AsyncValueTaskMethodBuilder"/> struct.</summary>
            /// <returns>The initialized instance.</returns>
            public static AsyncValueTaskMethodBuilder<TResult> Create() => default;

            /// <summary>Begins running the builder with the associated state machine.</summary>
            /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
            /// <param name="stateMachine">The state machine instance, passed by reference.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine =>
                _methodBuilder.Start(ref stateMachine);

            /// <summary>Associates the builder with the specified state machine.</summary>
            /// <param name="stateMachine">The state machine instance to associate with the builder.</param>
            public void SetStateMachine(IAsyncStateMachine stateMachine) =>
                _methodBuilder.SetStateMachine(stateMachine);

            /// <summary>Marks the task as successfully completed.</summary>
            public void SetResult(TResult result)
            {
                if (_useBuilder) { _methodBuilder.SetResult(result);  return; } else { _result = result; _haveResult = true; }
            }

            /// <summary>Marks the task as failed and binds the specified exception to the task.</summary>
            /// <param name="exception">The exception to bind to the task.</param>
            public void SetException(System.Exception exception) => _methodBuilder.SetException(exception);

            /// <summary>Gets the task for this builder.</summary>
            public ValueTask<TResult> Task
            {
                get
                {
                    if (_haveResult)
                    { return default; } else { _useBuilder = true; return new ValueTask<TResult>(_methodBuilder.Task); }

                    // With normal access paterns, task should always be non-null here: the async method should have
                    // either completed synchronously, in which case SetResult would have set m_task to a non-null object,
                    // or it should be completing asynchronously, in which case AwaitUnsafeOnCompleted would have similarly
                    // initialized m_task to a state machine object.  However, if the type is used manually (not via
                    // compiler-generated code) and accesses Task directly, we force it to be initialized.  Things will then
                    // "work" but in a degraded mode, as we don't know the TStateMachine type here, and thus we use a normal
                    // task object instead.
                }
            }

            /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
            /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
            /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
            /// <param name="awaiter">The awaiter.</param>
            /// <param name="stateMachine">The state machine.</param>
            public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : INotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                _useBuilder = true;
                _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
            }

            /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
            /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
            /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
            /// <param name="awaiter">The awaiter.</param>
            /// <param name="stateMachine">The state machine.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
                where TAwaiter : ICriticalNotifyCompletion
                where TStateMachine : IAsyncStateMachine
            {
                _useBuilder = true;
                _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
            }
        }

        #nullable enable
        /// <summary>Provides an awaitable type that enables configured awaits on a <see cref="ValueTask"/>.</summary>
        [StructLayout(LayoutKind.Auto)]
        public readonly struct ConfiguredValueTaskAwaitable
        {
            /// <summary>The wrapped <see cref="Task"/>.</summary>
            private readonly ValueTask _value;

            /// <summary>Initializes the awaitable.</summary>
            /// <param name="value">The wrapped <see cref="ValueTask"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ConfiguredValueTaskAwaitable(in ValueTask value) => _value = value;

            /// <summary>Returns an awaiter for this <see cref="ConfiguredValueTaskAwaitable"/> instance.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfiguredValueTaskAwaiter GetAwaiter() => new ConfiguredValueTaskAwaiter(in _value);

            /// <summary>Provides an awaiter for a <see cref="ConfiguredValueTaskAwaitable"/>.</summary>
            [StructLayout(LayoutKind.Auto)]
            public readonly struct ConfiguredValueTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
            {
                /// <summary>The value being awaited.</summary>
                private readonly ValueTask _value;

                /// <summary>Initializes the awaiter.</summary>
                /// <param name="value">The value to be awaited.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal ConfiguredValueTaskAwaiter(in ValueTask value) => _value = value;

                /// <summary>Gets whether the <see cref="ConfiguredValueTaskAwaitable"/> has completed.</summary>
                public bool IsCompleted
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _value.IsCompleted;
                }

                /// <summary>Gets the result of the ValueTask.</summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void GetResult() => _value.ThrowIfCompletedUnsuccessfully();

                /// <summary>Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable"/>.</summary>
                public void OnCompleted(Action continuation)
                {
                    object? obj = _value._obj;
                    Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                    if (obj is Task t)
                    {
                        t.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
                    }
                    else if (obj != null)
                    {
                        Unsafe.As<IValueTaskSource>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token,
                            ValueTaskSourceOnCompletedFlags.FlowExecutionContext |
                                (_value._continueOnCapturedContext ? ValueTaskSourceOnCompletedFlags.UseSchedulingContext : ValueTaskSourceOnCompletedFlags.None));
                    }
                    else
                    {
                        Task.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
                    }
                }

                /// <summary>Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable"/>.</summary>
                public void UnsafeOnCompleted(Action continuation)
                {
                    object? obj = _value._obj;
                    Debug.Assert(obj == null || obj is Task || obj is IValueTaskSource);

                    if (obj is Task t)
                    {
                        t.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
                    }
                    else if (obj != null)
                    {
                        Unsafe.As<IValueTaskSource>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token,
                            _value._continueOnCapturedContext ? ValueTaskSourceOnCompletedFlags.UseSchedulingContext : ValueTaskSourceOnCompletedFlags.None);
                    }
                    else
                    {
                        Task.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
                    }
                }


            }

        }

        /// <summary>Provides an awaitable type that enables configured awaits on a <see cref="ValueTask{TResult}"/>.</summary>
        /// <typeparam name="TResult">The type of the result produced.</typeparam>
        [StructLayout(LayoutKind.Auto)]
        public readonly struct ConfiguredValueTaskAwaitable<TResult>
        {
            /// <summary>The wrapped <see cref="ValueTask{TResult}"/>.</summary>
            private readonly ValueTask<TResult> _value;

            /// <summary>Initializes the awaitable.</summary>
            /// <param name="value">The wrapped <see cref="ValueTask{TResult}"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal ConfiguredValueTaskAwaitable(in ValueTask<TResult> value) => _value = value;

            /// <summary>Returns an awaiter for this <see cref="ConfiguredValueTaskAwaitable{TResult}"/> instance.</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ConfiguredValueTaskAwaiter GetAwaiter() => new ConfiguredValueTaskAwaiter(in _value);

            /// <summary>Provides an awaiter for a <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.</summary>
            [StructLayout(LayoutKind.Auto)]
            public readonly struct ConfiguredValueTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
            {
                /// <summary>The value being awaited.</summary>
                private readonly ValueTask<TResult> _value;

                /// <summary>Initializes the awaiter.</summary>
                /// <param name="value">The value to be awaited.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                internal ConfiguredValueTaskAwaiter(in ValueTask<TResult> value) => _value = value;

                /// <summary>Gets whether the <see cref="ConfiguredValueTaskAwaitable{TResult}"/> has completed.</summary>
                public bool IsCompleted
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _value.IsCompleted;
                }

                /// <summary>Gets the result of the ValueTask.</summary>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public TResult GetResult() => _value.Result;

                /// <summary>Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.</summary>
                public void OnCompleted(Action continuation)
                {
                    object? obj = _value._obj;
                    Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                    if (obj is Task<TResult> t)
                    {
                        t.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
                    }
                    else if (obj != null)
                    {
                        Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token,
                            ValueTaskSourceOnCompletedFlags.FlowExecutionContext |
                                (_value._continueOnCapturedContext ? ValueTaskSourceOnCompletedFlags.UseSchedulingContext : ValueTaskSourceOnCompletedFlags.None));
                    }
                    else
                    {
                        Task.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().OnCompleted(continuation);
                    }
                }

                /// <summary>Schedules the continuation action for the <see cref="ConfiguredValueTaskAwaitable{TResult}"/>.</summary>
                public void UnsafeOnCompleted(Action continuation)
                {
                    object? obj = _value._obj;
                    Debug.Assert(obj == null || obj is Task<TResult> || obj is IValueTaskSource<TResult>);

                    if (obj is Task<TResult> t)
                    {
                        t.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
                    }
                    else if (obj != null)
                    {
                        Unsafe.As<IValueTaskSource<TResult>>(obj).OnCompleted(ValueTaskAwaiter.s_invokeActionDelegate, continuation, _value._token,
                            _value._continueOnCapturedContext ? ValueTaskSourceOnCompletedFlags.UseSchedulingContext : ValueTaskSourceOnCompletedFlags.None);
                    }
                    else
                    {
                        Task.CompletedTask.ConfigureAwait(_value._continueOnCapturedContext).GetAwaiter().UnsafeOnCompleted(continuation);
                    }
                }
            }
        }
        #nullable disable



    }

    namespace Runtime.InteropServices.Marshalling
    {

        /// <summary>
        /// Represents the different marshalling modes.
        /// </summary>
        public enum MarshalMode
        {
            /// <summary>
            /// All modes. A marshaller specified with this mode will be used if there's no specific
            /// marshaller for a given usage mode.
            /// </summary>
            Default,
            /// <summary>
            /// By-value and <c>in</c> parameters in managed-to-unmanaged scenarios, like P/Invoke.
            /// </summary>
            ManagedToUnmanagedIn,
            /// <summary>
            /// <c>ref</c> parameters in managed-to-unmanaged scenarios, like P/Invoke.
            /// </summary>
            ManagedToUnmanagedRef,
            /// <summary>
            /// <c>out</c> parameters in managed-to-unmanaged scenarios, like P/Invoke.
            /// </summary>
            ManagedToUnmanagedOut,
            /// <summary>
            /// By-value and <c>in</c> parameters in unmanaged-to-managed scenarios, like Reverse P/Invoke.
            /// </summary>
            UnmanagedToManagedIn,
            /// <summary>
            /// <c>ref</c> parameters in unmanaged-to-managed scenarios, like Reverse P/Invoke.
            /// </summary>
            UnmanagedToManagedRef,
            /// <summary>
            /// <c>out</c> parameters in unmanaged-to-managed scenarios, like Reverse P/Invoke.
            /// </summary>
            UnmanagedToManagedOut,
            /// <summary>
            /// Elements of arrays passed with <c>in</c> or by-value in interop scenarios.
            /// </summary>
            ElementIn,
            /// <summary>
            /// Elements of arrays passed with <c>ref</c> or passed by-value with both <see cref="InAttribute"/> and <see cref="OutAttribute" /> in interop scenarios.
            /// </summary>
            ElementRef,
            /// <summary>
            /// Elements of arrays passed with <c>out</c> or passed by-value with only <see cref="OutAttribute" /> in interop scenarios.
            /// </summary>
            ElementOut
        }


        /// <summary>
        /// Specifies that this marshaller entry-point type is a contiguous collection marshaller.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
        public sealed class ContiguousCollectionMarshallerAttribute : Attribute
        {
        }

        /// <summary>
        /// Indicates an entry point type for defining a marshaller.
        /// </summary>
        [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true)]
        public sealed class CustomMarshallerAttribute : Attribute
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CustomMarshallerAttribute"/> class.
            /// </summary>
            /// <param name="managedType">The managed type to marshal.</param>
            /// <param name="marshalMode">The marshalling mode this attribute applies to.</param>
            /// <param name="marshallerType">The type used for marshalling.</param>
            public CustomMarshallerAttribute(Type managedType, MarshalMode marshalMode, Type marshallerType)
            {
                ManagedType = managedType;
                MarshalMode = marshalMode;
                MarshallerType = marshallerType;
            }

            /// <summary>
            /// Gets the managed type to marshal.
            /// </summary>
            public Type ManagedType { get; }

            /// <summary>
            /// Gets the marshalling mode this attribute applies to.
            /// </summary>
            public MarshalMode MarshalMode { get; }

            /// <summary>
            /// Gets the type used for marshalling.
            /// </summary>
            public Type MarshallerType { get; }

            /// <summary>
            /// Placeholder type for a generic parameter.
            /// </summary>
            public struct GenericPlaceholder
            {
            }
        }

        /// <summary>
        /// Provides a default custom marshaller type for a given managed type.
        /// </summary>
        /// <remarks>
        /// This attribute is recognized by the runtime-provided source generators for source-generated interop scenarios.
        /// It's not used by the interop marshalling system at run time.
        /// </remarks>
        /// <seealso cref="LibraryImportAttribute" />
        /// <seealso cref="CustomMarshallerAttribute" />
        [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Delegate)]
        public sealed class NativeMarshallingAttribute : Attribute
        {
            /// <summary>
            /// Initializes a new instance of the  <see cref="NativeMarshallingAttribute" /> class that provides a native marshalling type.
            /// </summary>
            /// <param name="nativeType">The marshaller type used to convert the attributed type from managed to native code. This type must be attributed with <see cref="CustomMarshallerAttribute" />.</param>
            public NativeMarshallingAttribute(Type nativeType)
            {
                NativeType = nativeType;
            }

            /// <summary>
            /// Gets the marshaller type used to convert the attributed type from managed to native code. This type must be attributed with <see cref="CustomMarshallerAttribute" />.
            /// </summary>
            public Type NativeType { get; }
        }

    }

    namespace Runtime.InteropServices
    {
        public static class MemoryMarshal
        {
            public static bool TryGetArray<T>(ReadOnlyMemory<T> memory, out ArraySegment<T> segment)
            {
                int start;
                int length;
                object objectStartLength = memory.GetObjectStartLength(out start, out length);
                if (start < 0)
                {
                    if (((MemoryManager<T>)objectStartLength).TryGetArray(out var segment2))
                    {
                        segment = new ArraySegment<T>(segment2.Array, segment2.Offset + (start & 0x7FFFFFFF), length);
                        return true;
                    }
                }
                else if (objectStartLength is T[] array)
                {
                    segment = new ArraySegment<T>(array, start, length & 0x7FFFFFFF);
                    return true;
                }
                if ((length & 0x7FFFFFFF) == 0)
                {
                    segment = new ArraySegment<T>(SpanHelpers.PerTypeValues<T>.EmptyArray);
                    return true;
                }
                segment = default(ArraySegment<T>);
                return false;
            }

            public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, out TManager manager) where TManager : MemoryManager<T>
            {
                int start;
                int length;
                TManager val = (manager = memory.GetObjectStartLength(out start, out length) as TManager);
                return manager != null;
            }

            public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, out TManager manager, out int start, out int length) where TManager : MemoryManager<T>
            {
                TManager val = (manager = memory.GetObjectStartLength(out start, out length) as TManager);
                start &= int.MaxValue;
                if (manager == null)
                {
                    start = 0;
                    length = 0;
                    return false;
                }
                return true;
            }

            public static IEnumerable<T> ToEnumerable<T>(ReadOnlyMemory<T> memory)
            {
                for (int i = 0; i < memory.Length; i++)
                {
                    yield return memory.Span[i];
                }
            }

            public static bool TryGetString(ReadOnlyMemory<char> memory, out string text, out int start, out int length)
            {
                if (memory.GetObjectStartLength(out var start2, out var length2) is string text2)
                {
                    text = text2;
                    start = start2;
                    length = length2;
                    return true;
                }
                text = null;
                start = 0;
                length = 0;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T Read<T>(ReadOnlySpan<byte> source) where T : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
                }
                if (Unsafe.SizeOf<T>() > source.Length)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
                }
                return Unsafe.ReadUnaligned<T>(ref GetReference(source));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryRead<T>(ReadOnlySpan<byte> source, out T value) where T : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
                }
                if (Unsafe.SizeOf<T>() > (uint)source.Length)
                {
                    value = default(T);
                    return false;
                }
                value = Unsafe.ReadUnaligned<T>(ref GetReference(source));
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Write<T>(Span<byte> destination, ref T value) where T : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
                }
                if ((uint)Unsafe.SizeOf<T>() > (uint)destination.Length)
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException(System.ExceptionArgument.length);
                }
                Unsafe.WriteUnaligned(ref GetReference(destination), value);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool TryWrite<T>(Span<byte> destination, ref T value) where T : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
                }
                if (Unsafe.SizeOf<T>() > (uint)destination.Length)
                {
                    return false;
                }
                Unsafe.WriteUnaligned(ref GetReference(destination), value);
                return true;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Memory<T> CreateFromPinnedArray<T>(T[] array, int start, int length)
            {
                if (array == null)
                {
                    if (start != 0 || length != 0)
                    {
                        System.ThrowHelper.ThrowArgumentOutOfRangeException();
                    }
                    return default(Memory<T>);
                }
                if (default(T) == null && array.GetType() != typeof(T[]))
                {
                    System.ThrowHelper.ThrowArrayTypeMismatchException();
                }
                if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                {
                    System.ThrowHelper.ThrowArgumentOutOfRangeException();
                }
                return new Memory<T>((System.Object)array, start, length | int.MinValue);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Span<byte> AsBytes<T>(Span<T> span) where T : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
                }
                int length = checked(span.Length * Unsafe.SizeOf<T>());
                return new Span<byte>(Unsafe.As<Pinnable<byte>>(span.Pinnable), span.ByteOffset, length);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ReadOnlySpan<byte> AsBytes<T>(ReadOnlySpan<T> span) where T : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<T>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(T));
                }
                int length = checked(span.Length * Unsafe.SizeOf<T>());
                return new ReadOnlySpan<byte>(Unsafe.As<Pinnable<byte>>(span.Pinnable), span.ByteOffset, length);
            }

            public static Memory<T> AsMemory<T>(ReadOnlyMemory<T> memory)
            {
                return Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref memory);
            }

            public unsafe static ref T GetReference<T>(Span<T> span)
            {
                if (span.Pinnable == null)
                {
                    return ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer());
                }
                return ref Unsafe.AddByteOffset(ref span.Pinnable.Data, span.ByteOffset);
            }

            public unsafe static ref T GetReference<T>(ReadOnlySpan<T> span)
            {
                if (span.Pinnable == null)
                {
                    return ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer());
                }
                return ref Unsafe.AddByteOffset(ref span.Pinnable.Data, span.ByteOffset);
            }

            public static Span<TTo> Cast<TFrom, TTo>(Span<TFrom> span) where TFrom : struct where TTo : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<TFrom>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TFrom));
                }
                if (SpanHelpers.IsReferenceOrContainsReferences<TTo>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TTo));
                }
                checked
                {
                    int length = (int)unchecked(checked(unchecked((long)span.Length) * unchecked((long)Unsafe.SizeOf<TFrom>())) / Unsafe.SizeOf<TTo>());
                    return new Span<TTo>(Unsafe.As<Pinnable<TTo>>(span.Pinnable), span.ByteOffset, length);
                }
            }

            public static ReadOnlySpan<TTo> Cast<TFrom, TTo>(ReadOnlySpan<TFrom> span) where TFrom : struct where TTo : struct
            {
                if (SpanHelpers.IsReferenceOrContainsReferences<TFrom>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TFrom));
                }
                if (SpanHelpers.IsReferenceOrContainsReferences<TTo>())
                {
                    System.ThrowHelper.ThrowArgumentException_InvalidTypeWithPointersNotSupported(typeof(TTo));
                }
                checked
                {
                    int length = (int)unchecked(checked(unchecked((long)span.Length) * unchecked((long)Unsafe.SizeOf<TFrom>())) / Unsafe.SizeOf<TTo>());
                    return new ReadOnlySpan<TTo>(Unsafe.As<Pinnable<TTo>>(span.Pinnable), span.ByteOffset, length);
                }
            }
        }

        public static class SequenceMarshal
        {
            public static bool TryGetReadOnlySequenceSegment<T>(ReadOnlySequence<T> sequence, out ReadOnlySequenceSegment<T> startSegment, out int startIndex, out ReadOnlySequenceSegment<T> endSegment, out int endIndex)
            {
                return sequence.TryGetReadOnlySequenceSegment(out startSegment, out startIndex, out endSegment, out endIndex);
            }

            public static bool TryGetArray<T>(ReadOnlySequence<T> sequence, out ArraySegment<T> segment)
            {
                return sequence.TryGetArray(out segment);
            }

            public static bool TryGetReadOnlyMemory<T>(ReadOnlySequence<T> sequence, out ReadOnlyMemory<T> memory)
            {
                if (!sequence.IsSingleSegment)
                {
                    memory = default(ReadOnlyMemory<T>);
                    return false;
                }
                memory = sequence.First;
                return true;
            }

            internal static bool TryGetString(ReadOnlySequence<char> sequence, out string text, out int start, out int length)
            {
                return sequence.TryGetString(out text, out start, out length);
            }
        }
    
    }

    namespace Runtime.Versioning
    {
        /*============================================================
        **
        **
        **
        ** The [NonVersionable] attribute is applied to indicate that the implementation 
        ** of a particular member or layout of a struct cannot be changed for given platform in incompatible way.
        ** This allows cross-module inlining of methods and data structures whose implementation 
        ** is never changed in ReadyToRun native images. Any changes to such members or types would be 
        ** breaking changes for ReadyToRun.
        **
        ** Applying this type also has the side effect that the inlining tables in R2R images will not
        ** report that inlining of NonVersionable attributed methods occured. These inlining tables are used
        ** by profilers to figure out the set of methods that need to be rejited when one method is instrumented,
        ** so in effect NonVersionable methods are also non-instrumentable. Generally this is OK for
        ** extremely trivial low level methods where NonVersionable gets used, but if there is any plan to 
        ** significantly extend its usage or allow 3rd parties to use it please discuss with the diagnostics team.
        ===========================================================*/

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor,
                        AllowMultiple = false, Inherited = false)]
        internal sealed class NonVersionableAttribute : Attribute
        {
            public NonVersionableAttribute()
            {
            }
        }
        #if !NET7_0_OR_GREATER
            #nullable enable
            /// <summary>
            /// [INHERITEDFROMDOTNET7] An attribute that specifies the platform that the assembly ,
            /// method , class , structure or token can run to or not. This class is abstract; which means 
            /// that you must create another class that inherit from this one.
            /// </summary>
            public abstract partial class OSPlatformAttribute : System.Attribute
            {
                private protected OSPlatformAttribute(string platformName) { }

                /// <summary>
                /// The Platform name that the attributed function can run to.
                /// Do not use this property directly. Otherwise , this one method will throw up an exception.
                /// </summary>
                public string PlatformName { get { throw new System.Exception(); } }

            }

            /// <summary>
            /// [INHERITEDFROMDOTNET7] An attribute that specifies the platform that the assembly ,
            /// method , class , structure or token can run to.
            /// </summary>
            [System.AttributeUsageAttribute(System.AttributeTargets.Assembly | System.AttributeTargets.Class
                | System.AttributeTargets.Constructor | System.AttributeTargets.Enum
                | System.AttributeTargets.Event | System.AttributeTargets.Field | System.AttributeTargets.Interface
                | System.AttributeTargets.Method | System.AttributeTargets.Module | System.AttributeTargets.Property
                | System.AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
            public sealed partial class SupportedOSPlatformAttribute : System.Runtime.Versioning.OSPlatformAttribute
            {
                /// <summary>
                /// Create a new instance of the <see cref="SupportedOSPlatformAttribute"/> class with the specified platform name.
                /// </summary>
                /// <param name="platformName">The platform name that the attributed signature is allowed to run.</param>
                public SupportedOSPlatformAttribute(string platformName) : base(platformName) { }
            }

            /// <summary>
            /// [INHERITEDFROMDOTNET7] An attribute that specifies the platform that the assembly ,
            /// method , class , structure or token can NOT run to.
            /// </summary>
            [System.AttributeUsageAttribute(System.AttributeTargets.Assembly | System.AttributeTargets.Class
                | System.AttributeTargets.Constructor | System.AttributeTargets.Enum | System.AttributeTargets.Event
                | System.AttributeTargets.Field | System.AttributeTargets.Interface | System.AttributeTargets.Method
                | System.AttributeTargets.Module | System.AttributeTargets.Property | System.AttributeTargets.Struct,
                AllowMultiple = true, Inherited = false)]
            public sealed partial class UnsupportedOSPlatformAttribute : System.Runtime.Versioning.OSPlatformAttribute
            {
                /// <summary>
                /// Create a new instance of the <see cref="UnsupportedOSPlatformAttribute"/> class with the specified platform name.
                /// </summary>
                public UnsupportedOSPlatformAttribute(string platformName) : base(platformName) { }
                /// <summary>
                /// Create a new instance of the <see cref="UnsupportedOSPlatformAttribute"/> class with the specified platform name
                /// with the specified message.
                /// </summary>
                public UnsupportedOSPlatformAttribute(string platformName, string? message) : base(platformName) { Message = message; }
                /// <summary>
                /// Read-only <see cref="System.String"/> that when it is attempted to be retrieved , throws an exception.
                /// </summary>
                public string? Message { get { throw new System.Exception(); } set { } }
            }
            #nullable disable   
        #endif
    }

    namespace Numerics
    {

        namespace Hashing
        {
            internal static class HashHelpers
            {
                private readonly static System.Func<System.Int32> RS1 = () =>
                {
                    System.Random RD = null;
                    try
                    {
                        RD = new System.Random();
                        return RD.Next(System.Int32.MinValue, System.Int32.MaxValue);
                    }
                    catch (System.Exception EX)
                    {
                        // Rethrow the exception , but as an invalidoperation one , because actually calling unintialised RD is illegal.
                        throw new InvalidOperationException("Could not call Rand.Next. More than one errors occured.", EX);
                    }
                    finally { if (RD != null) { RD = null; } }
                };

                public static readonly int RandomSeed = RS1();

                public static int Combine(int h1, int h2)
                {
                    // RyuJIT optimizes this to use the ROL instruction
                    // Related GitHub pull request: dotnet/coreclr#1830
                    uint rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
                    return ((int)rol5 + h1) ^ h2;
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Register
        {
            [FieldOffset(0)]
            internal byte byte_0;

            [FieldOffset(1)]
            internal byte byte_1;

            [FieldOffset(2)]
            internal byte byte_2;

            [FieldOffset(3)]
            internal byte byte_3;

            [FieldOffset(4)]
            internal byte byte_4;

            [FieldOffset(5)]
            internal byte byte_5;

            [FieldOffset(6)]
            internal byte byte_6;

            [FieldOffset(7)]
            internal byte byte_7;

            [FieldOffset(8)]
            internal byte byte_8;

            [FieldOffset(9)]
            internal byte byte_9;

            [FieldOffset(10)]
            internal byte byte_10;

            [FieldOffset(11)]
            internal byte byte_11;

            [FieldOffset(12)]
            internal byte byte_12;

            [FieldOffset(13)]
            internal byte byte_13;

            [FieldOffset(14)]
            internal byte byte_14;

            [FieldOffset(15)]
            internal byte byte_15;

            [FieldOffset(0)]
            internal sbyte sbyte_0;

            [FieldOffset(1)]
            internal sbyte sbyte_1;

            [FieldOffset(2)]
            internal sbyte sbyte_2;

            [FieldOffset(3)]
            internal sbyte sbyte_3;

            [FieldOffset(4)]
            internal sbyte sbyte_4;

            [FieldOffset(5)]
            internal sbyte sbyte_5;

            [FieldOffset(6)]
            internal sbyte sbyte_6;

            [FieldOffset(7)]
            internal sbyte sbyte_7;

            [FieldOffset(8)]
            internal sbyte sbyte_8;

            [FieldOffset(9)]
            internal sbyte sbyte_9;

            [FieldOffset(10)]
            internal sbyte sbyte_10;

            [FieldOffset(11)]
            internal sbyte sbyte_11;

            [FieldOffset(12)]
            internal sbyte sbyte_12;

            [FieldOffset(13)]
            internal sbyte sbyte_13;

            [FieldOffset(14)]
            internal sbyte sbyte_14;

            [FieldOffset(15)]
            internal sbyte sbyte_15;

            [FieldOffset(0)]
            internal ushort uint16_0;

            [FieldOffset(2)]
            internal ushort uint16_1;

            [FieldOffset(4)]
            internal ushort uint16_2;

            [FieldOffset(6)]
            internal ushort uint16_3;

            [FieldOffset(8)]
            internal ushort uint16_4;

            [FieldOffset(10)]
            internal ushort uint16_5;

            [FieldOffset(12)]
            internal ushort uint16_6;

            [FieldOffset(14)]
            internal ushort uint16_7;

            [FieldOffset(0)]
            internal short int16_0;

            [FieldOffset(2)]
            internal short int16_1;

            [FieldOffset(4)]
            internal short int16_2;

            [FieldOffset(6)]
            internal short int16_3;

            [FieldOffset(8)]
            internal short int16_4;

            [FieldOffset(10)]
            internal short int16_5;

            [FieldOffset(12)]
            internal short int16_6;

            [FieldOffset(14)]
            internal short int16_7;

            [FieldOffset(0)]
            internal uint uint32_0;

            [FieldOffset(4)]
            internal uint uint32_1;

            [FieldOffset(8)]
            internal uint uint32_2;

            [FieldOffset(12)]
            internal uint uint32_3;

            [FieldOffset(0)]
            internal int int32_0;

            [FieldOffset(4)]
            internal int int32_1;

            [FieldOffset(8)]
            internal int int32_2;

            [FieldOffset(12)]
            internal int int32_3;

            [FieldOffset(0)]
            internal ulong uint64_0;

            [FieldOffset(8)]
            internal ulong uint64_1;

            [FieldOffset(0)]
            internal long int64_0;

            [FieldOffset(8)]
            internal long int64_1;

            [FieldOffset(0)]
            internal float single_0;

            [FieldOffset(4)]
            internal float single_1;

            [FieldOffset(8)]
            internal float single_2;

            [FieldOffset(12)]
            internal float single_3;

            [FieldOffset(0)]
            internal double double_0;

            [FieldOffset(8)]
            internal double double_1;
        }

        internal class ConstantHelper
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static byte GetByteWithAllBitsSet()
            {
                byte result = 0;
                result = byte.MaxValue;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static sbyte GetSByteWithAllBitsSet()
            {
                sbyte result = 0;
                result = -1;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ushort GetUInt16WithAllBitsSet()
            {
                ushort result = 0;
                result = ushort.MaxValue;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static short GetInt16WithAllBitsSet()
            {
                short result = 0;
                result = -1;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetUInt32WithAllBitsSet()
            {
                uint result = 0u;
                result = uint.MaxValue;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetInt32WithAllBitsSet()
            {
                int result = 0;
                result = -1;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong GetUInt64WithAllBitsSet()
            {
                ulong result = 0uL;
                result = ulong.MaxValue;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long GetInt64WithAllBitsSet()
            {
                long result = 0L;
                result = -1L;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static float GetSingleWithAllBitsSet()
            {
                float result = 0f;
                *(int*)(&result) = -1;
                return result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static double GetDoubleWithAllBitsSet()
            {
                double result = 0.0;
                *(long*)(&result) = -1L;
                return result;
            }
        }

    }

    namespace Diagnostics.CodeAnalysis
    {
        #nullable enable
        /// <summary>Specifies that the method or property will ensure that the listed field and property members have not-null values.</summary>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
        public sealed class MemberNotNullAttribute : Attribute
        {
            /// <summary>Initializes the attribute with a field or property member.</summary>
            /// <param name="member">
            /// The field or property member that is promised to be not-null.
            /// </param>
            public MemberNotNullAttribute(string member) => Members = new[] { member };

            /// <summary>Initializes the attribute with the list of field and property members.</summary>
            /// <param name="members">
            /// The list of field and property members that are promised to be not-null.
            /// </param>
            public MemberNotNullAttribute(params string[] members) => Members = members;

            /// <summary>Gets field or property member names.</summary>
            public string[] Members { get; }
        }

        /// <summary>
        /// Suppresses reporting of a specific rule violation, allowing multiple suppressions on a
        /// single code artifact.
        /// </summary>
        /// <remarks>
        /// <see cref="UnconditionalSuppressMessageAttribute"/> is different than
        /// <see cref="SuppressMessageAttribute"/> in that it doesn't have a
        /// <see cref="ConditionalAttribute"/>. So it is always preserved in the compiled assembly.
        /// </remarks>
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        public sealed class UnconditionalSuppressMessageAttribute : Attribute
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UnconditionalSuppressMessageAttribute"/>
            /// class, specifying the category of the tool and the identifier for an analysis rule.
            /// </summary>
            /// <param name="category">The category for the attribute.</param>
            /// <param name="checkId">The identifier of the analysis rule the attribute applies to.</param>
            public UnconditionalSuppressMessageAttribute(string category, string checkId)
            {
                Category = category;
                CheckId = checkId;
            }

            /// <summary>
            /// Gets the category identifying the classification of the attribute.
            /// </summary>
            /// <remarks>
            /// The <see cref="Category"/> property describes the tool or tool analysis category
            /// for which a message suppression attribute applies.
            /// </remarks>
            public string Category { get; }

            /// <summary>
            /// Gets the identifier of the analysis tool rule to be suppressed.
            /// </summary>
            /// <remarks>
            /// Concatenated together, the <see cref="Category"/> and <see cref="CheckId"/>
            /// properties form a unique check identifier.
            /// </remarks>
            public string CheckId { get; }

            /// <summary>
            /// Gets or sets the scope of the code that is relevant for the attribute.
            /// </summary>
            /// <remarks>
            /// The Scope property is an optional argument that specifies the metadata scope for which
            /// the attribute is relevant.
            /// </remarks>
            public string? Scope { get; set; }

            /// <summary>
            /// Gets or sets a fully qualified path that represents the target of the attribute.
            /// </summary>
            /// <remarks>
            /// The <see cref="Target"/> property is an optional argument identifying the analysis target
            /// of the attribute. An example value is "System.IO.Stream.ctor():System.Void".
            /// Because it is fully qualified, it can be long, particularly for targets such as parameters.
            /// The analysis tool user interface should be capable of automatically formatting the parameter.
            /// </remarks>
            public string? Target { get; set; }

            /// <summary>
            /// Gets or sets an optional argument expanding on exclusion criteria.
            /// </summary>
            /// <remarks>
            /// The <see cref="MessageId "/> property is an optional argument that specifies additional
            /// exclusion where the literal metadata target is not sufficiently precise. For example,
            /// the <see cref="UnconditionalSuppressMessageAttribute"/> cannot be applied within a method,
            /// and it may be desirable to suppress a violation against a statement in the method that will
            /// give a rule violation, but not against all statements in the method.
            /// </remarks>
            public string? MessageId { get; set; }

            /// <summary>
            /// Gets or sets the justification for suppressing the code analysis message.
            /// </summary>
            public string? Justification { get; set; }
        }

        /// <summary>
        /// Indicates that the specified method requires dynamic access to code that is not referenced
        /// statically, for example through <see cref="System.Reflection"/>.
        /// </summary>
        /// <remarks>
        /// This allows tools to understand which methods are unsafe to call when removing unreferenced
        /// code from an application.
        /// </remarks>
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class, Inherited = false)]
        public sealed class RequiresUnreferencedCodeAttribute : Attribute
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RequiresUnreferencedCodeAttribute"/> class
            /// with the specified message.
            /// </summary>
            /// <param name="message">
            /// A message that contains information about the usage of unreferenced code.
            /// </param>
            public RequiresUnreferencedCodeAttribute(string message)
            {
                Message = message;
            }

            /// <summary>
            /// Gets a message that contains information about the usage of unreferenced code.
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// Gets or sets an optional URL that contains more information about the method,
            /// why it requires unreferenced code, and what options a consumer has to deal with it.
            /// </summary>
            public string? Url { get; set; }
        }
        #nullable disable
    }

    namespace IO
    {

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
                        throw new FormatException("Bad Encoded 7-Bit Integer encountered.");
                    }

                    // ReadByte handles end of stream cases for us.
                    b = reader.ReadByte();
                    count |= (b & 0x7F) << shift;
                    shift += 7;
                } while ((b & 0x80) != 0);
                return count;
            }
        }
    }

    #if NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET6 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

        // https://github.com/dotnet/corefx/blob/48363ac826ccf66fbe31a5dcb1dc2aab9a7dd768/src/Common/src/CoreLib/System/Diagnostics/CodeAnalysis/NullableAttributes.cs

        // Licensed to the .NET Foundation under one or more agreements.
        // The .NET Foundation licenses this file to you under the MIT license.
        // See the LICENSE file in the project root for more information.

    namespace Diagnostics.CodeAnalysis
    {
        /// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
            sealed class AllowNullAttribute : Attribute
        { }

        /// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class DisallowNullAttribute : Attribute
        { }

        /// <summary>Specifies that an output may be null even if the corresponding type disallows it.</summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class MaybeNullAttribute : Attribute
        { }

        /// <summary>Specifies that an output will not be null even if the corresponding type allows it.</summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class NotNullAttribute : Attribute
        { }

        /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.</summary>
        [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class MaybeNullWhenAttribute : Attribute
        {
            /// <summary>Initializes the attribute with the specified return value condition.</summary>
            /// <param name="returnValue">
            /// The return value condition. If the method returns this value, the associated parameter may be null.
            /// </param>
            public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

            /// <summary>Gets the return value condition.</summary>
            public bool ReturnValue { get; }
        }

        /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
        [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class NotNullWhenAttribute : Attribute
        {
            /// <summary>Initializes the attribute with the specified return value condition.</summary>
            /// <param name="returnValue">
            /// The return value condition. If the method returns this value, the associated parameter will not be null.
            /// </param>
            public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

            /// <summary>Gets the return value condition.</summary>
            public bool ReturnValue { get; }
        }

        /// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
        [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class NotNullIfNotNullAttribute : Attribute
        {
            /// <summary>Initializes the attribute with the associated parameter name.</summary>
            /// <param name="parameterName">
            /// The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.
            /// </param>
            public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

            /// <summary>Gets the associated parameter name.</summary>
            public string ParameterName { get; }
        }

        /// <summary>Applied to a method that will never return under any circumstance.</summary>
        [AttributeUsage(AttributeTargets.Method, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class DoesNotReturnAttribute : Attribute
        { }

        /// <summary>Specifies that the method will not return if the associated Boolean parameter is passed the specified value.</summary>
        [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
        #if INTERNAL_NULLABLE_ATTRIBUTES
			internal
        #else
            public
        #endif
        sealed class DoesNotReturnIfAttribute : Attribute
        {
            /// <summary>Initializes the attribute with the specified parameter value.</summary>
            /// <param name="parameterValue">
            /// The condition parameter value. Code after the method will be considered unreachable by diagnostics if the argument to
            /// the associated parameter matches this value.
            /// </param>
            public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;

            /// <summary>Gets the condition parameter value.</summary>
            public bool ParameterValue { get; }
        }
    }


    #endif

    #nullable enable
    #pragma warning disable CS1591
    /// <summary>
    /// The Microsoft's base class for the Internal Runtime Resource Handler.
    /// This class , however , does only contain some formatting methods that you might need when you migrate code.
    /// Be noted , this class does not conflict with the original <see cref="System.SR"/> class ,
    /// because that class is internally used in mscorlib for .NET Framework and System.Private.CoreLib for .NET .
    /// </summary>
    public static class SR
    {

        private static bool UsingResourceKeys() => AppContext.TryGetSwitch("System.Resources.UseSystemResourceKeys", out bool usingResourceKeys) ? usingResourceKeys : false;

        public static System.String Format(string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(resourceFormat, p1);
        }

        public static System.String Format(string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(resourceFormat, p1, p2);
        }

        public static System.String Format(string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(resourceFormat, p1, p2, p3);
        }

        public static System.String Format(string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(resourceFormat, args);
            }

            return resourceFormat;
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, object? p1)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1);
            }

            return string.Format(provider, resourceFormat, p1);
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2);
            }

            return string.Format(provider, resourceFormat, p1, p2);
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys())
            {
                return string.Join(", ", resourceFormat, p1, p2, p3);
            }

            return string.Format(provider, resourceFormat, p1, p2, p3);
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys())
                {
                    return resourceFormat + ", " + string.Join(", ", args);
                }

                return string.Format(provider, resourceFormat, args);
            }

            return resourceFormat;
        }

    }


    #pragma warning restore CS1591
    #nullable disable

    internal static class DecimalDecCalc
    {
        private static uint D32DivMod1E9(uint hi32, ref uint lo32)
        {
            ulong num = ((ulong)hi32 << 32) | lo32;
            lo32 = (uint)(num / 1000000000uL);
            return (uint)(num % 1000000000uL);
        }

        internal static uint DecDivMod1E9(ref MutableDecimal value)
        {
            return D32DivMod1E9(D32DivMod1E9(D32DivMod1E9(0u, ref value.High), ref value.Mid), ref value.Low);
        }

        internal static void DecAddInt32(ref MutableDecimal value, uint i)
        {
            if (D32AddCarry(ref value.Low, i) && D32AddCarry(ref value.Mid, 1u))
            {
                D32AddCarry(ref value.High, 1u);
            }
        }

        private static bool D32AddCarry(ref uint value, uint i)
        {
            uint num = value;
            uint num2 = (value = num + i);
            if (num2 >= num)
            {
                return num2 < i;
            }
            return true;
        }

        internal static void DecMul10(ref MutableDecimal value)
        {
            MutableDecimal d = value;
            DecShiftLeft(ref value);
            DecShiftLeft(ref value);
            DecAdd(ref value, d);
            DecShiftLeft(ref value);
        }

        private static void DecShiftLeft(ref MutableDecimal value)
        {
            uint num = (((value.Low & 0x80000000u) != 0) ? 1u : 0u);
            uint num2 = (((value.Mid & 0x80000000u) != 0) ? 1u : 0u);
            value.Low <<= 1;
            value.Mid = (value.Mid << 1) | num;
            value.High = (value.High << 1) | num2;
        }

        private static void DecAdd(ref MutableDecimal value, MutableDecimal d)
        {
            if (D32AddCarry(ref value.Low, d.Low) && D32AddCarry(ref value.Mid, 1u))
            {
                D32AddCarry(ref value.High, 1u);
            }
            if (D32AddCarry(ref value.Mid, d.Mid))
            {
                D32AddCarry(ref value.High, 1u);
            }
            D32AddCarry(ref value.High, d.High);
        }
    }

    internal struct MutableDecimal
    {
        public uint Flags;

        public uint High;

        public uint Low;

        public uint Mid;

        private const uint SignMask = 2147483648u;

        private const uint ScaleMask = 16711680u;

        private const int ScaleShift = 16;

        public bool IsNegative
        {
            get { return (Flags & 0x80000000u) != 0; }
            set { Flags = (Flags & 0x7FFFFFFFu) | (value ? 2147483648u : 0u); }
        }

        public int Scale
        {
            get { return (byte)(Flags >> 16); }
            set { Flags = (Flags & 0xFF00FFFFu) | (uint)(value << 16); }
        }
    }

    internal struct NUInt
    {
        private unsafe readonly void* _value;

        private unsafe NUInt(uint value)
        {
            _value = (void*)value;
        }

        private unsafe NUInt(ulong value)
        {
            _value = (void*)value;
        }

        public static implicit operator NUInt(uint value)
        {
            return new NUInt(value);
        }

        public unsafe static implicit operator IntPtr(NUInt value)
        {
            return (IntPtr)value._value;
        }

        public static explicit operator NUInt(int value)
        {
            return new NUInt((uint)value);
        }

        public unsafe static explicit operator void*(NUInt value)
        {
            return value._value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static NUInt operator *(NUInt left, NUInt right)
        {
            if (sizeof(IntPtr) != 4)
            {
                return new NUInt((ulong)left._value * (ulong)right._value);
            }
            return new NUInt((uint)((int)left._value * (int)right._value));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class Pinnable<T> { public T Data; }

    internal enum ExceptionArgument
    {
        length,
        start,
        minimumBufferSize,
        elementIndex,
        comparable,
        comparer,
        destination,
        offset,
        startSegment,
        endSegment,
        startIndex,
        endIndex,
        array,
        culture,
        manager
    }

    internal static class Number
    {
        private static class DoubleHelper
        {
            public unsafe static uint Exponent(double d)
            {
                return (*(uint*)((byte*)(&d) + 4) >> 20) & 0x7FFu;
            }

            public unsafe static ulong Mantissa(double d)
            {
                return *(uint*)(&d) | ((ulong)(uint)(*(int*)((byte*)(&d) + 4) & 0xFFFFF) << 32);
            }

            public unsafe static bool Sign(double d)
            {
                return *(uint*)((byte*)(&d) + 4) >> 31 != 0;
            }
        }

        internal const int DECIMAL_PRECISION = 29;

        private static readonly ulong[] s_rgval64Power10 = new ulong[30]
        {
            11529215046068469760uL, 14411518807585587200uL, 18014398509481984000uL, 11258999068426240000uL, 14073748835532800000uL, 17592186044416000000uL, 10995116277760000000uL, 13743895347200000000uL, 17179869184000000000uL, 10737418240000000000uL,
            13421772800000000000uL, 16777216000000000000uL, 10485760000000000000uL, 13107200000000000000uL, 16384000000000000000uL, 14757395258967641293uL, 11805916207174113035uL, 9444732965739290428uL, 15111572745182864686uL, 12089258196146291749uL,
            9671406556917033399uL, 15474250491067253438uL, 12379400392853802751uL, 9903520314283042201uL, 15845632502852867522uL, 12676506002282294018uL, 10141204801825835215uL, 16225927682921336344uL, 12980742146337069075uL, 10384593717069655260uL
        };

        private static readonly sbyte[] s_rgexp64Power10 = new sbyte[15]
        {
        4, 7, 10, 14, 17, 20, 24, 27, 30, 34,
        37, 40, 44, 47, 50
        };

        private static readonly ulong[] s_rgval64Power10By16 = new ulong[42]
        {
        10240000000000000000uL, 11368683772161602974uL, 12621774483536188886uL, 14012984643248170708uL, 15557538194652854266uL, 17272337110188889248uL, 9588073174409622172uL, 10644899600020376798uL, 11818212630765741798uL, 13120851772591970216uL,
        14567071740625403792uL, 16172698447808779622uL, 17955302187076837696uL, 9967194951097567532uL, 11065809325636130658uL, 12285516299433008778uL, 13639663065038175358uL, 15143067982934716296uL, 16812182738118149112uL, 9332636185032188787uL,
        10361307573072618722uL, 16615349947311448416uL, 14965776766268445891uL, 13479973333575319909uL, 12141680576410806707uL, 10936253623915059637uL, 9850501549098619819uL, 17745086042373215136uL, 15983352577617880260uL, 14396524142538228461uL,
        12967236152753103031uL, 11679847981112819795uL, 10520271803096747049uL, 9475818434452569218uL, 17070116948172427008uL, 15375394465392026135uL, 13848924157002783096uL, 12474001934591998882uL, 11235582092889474480uL, 10120112665365530972uL,
        18230774251475056952uL, 16420821625123739930uL
        };

        private static readonly short[] s_rgexp64Power10By16 = new short[21]
        {
        54, 107, 160, 213, 266, 319, 373, 426, 479, 532,
        585, 638, 691, 745, 798, 851, 904, 957, 1010, 1064,
        1117
        };

        public static void RoundNumber(ref NumberBuffer number, int pos)
        {
            Span<byte> digits = number.Digits;
            int i;
            for (i = 0; i < pos && digits[i] != 0; i++)
            {
            }
            if (i == pos && digits[i] >= 53)
            {
                while (i > 0 && digits[i - 1] == 57)
                {
                    i--;
                }
                if (i > 0)
                {
                    digits[i - 1]++;
                }
                else
                {
                    number.Scale++;
                    digits[0] = 49;
                    i = 1;
                }
            }
            else
            {
                while (i > 0 && digits[i - 1] == 48)
                {
                    i--;
                }
            }
            if (i == 0)
            {
                number.Scale = 0;
                number.IsNegative = false;
            }
            digits[i] = 0;
        }

        internal static bool NumberBufferToDouble(ref NumberBuffer number, out double value)
        {
            double num = NumberToDouble(ref number);
            uint num2 = DoubleHelper.Exponent(num);
            ulong num3 = DoubleHelper.Mantissa(num);
            switch (num2)
            {
                case 2047u:
                    value = 0.0;
                    return false;
                case 0u:
                    if (num3 == 0L)
                    {
                        num = 0.0;
                    }
                    break;
            }
            value = num;
            return true;
        }

        public unsafe static bool NumberBufferToDecimal(ref NumberBuffer number, ref decimal value)
        {
            MutableDecimal source = default(MutableDecimal);
            byte* ptr = number.UnsafeDigits;
            int num = number.Scale;
            if (*ptr == 0)
            {
                if (num > 0)
                {
                    num = 0;
                }
            }
            else
            {
                if (num > 29)
                {
                    return false;
                }
                while ((num > 0 || (*ptr != 0 && num > -28)) && (source.High < 429496729 || (source.High == 429496729 && (source.Mid < 2576980377u || (source.Mid == 2576980377u && (source.Low < 2576980377u || (source.Low == 2576980377u && *ptr <= 53)))))))
                {
                    DecimalDecCalc.DecMul10(ref source);
                    if (*ptr != 0)
                    {
                        DecimalDecCalc.DecAddInt32(ref source, (uint)(*(ptr++) - 48));
                    }
                    num--;
                }
                if (*(ptr++) >= 53)
                {
                    bool flag = true;
                    if (*(ptr - 1) == 53 && (int)(*(ptr - 2)) % 2 == 0)
                    {
                        int num2 = 20;
                        while (*ptr == 48 && num2 != 0)
                        {
                            ptr++;
                            num2--;
                        }
                        if (*ptr == 0 || num2 == 0)
                        {
                            flag = false;
                        }
                    }
                    if (flag)
                    {
                        DecimalDecCalc.DecAddInt32(ref source, 1u);
                        if ((source.High | source.Mid | source.Low) == 0)
                        {
                            source.High = 429496729u;
                            source.Mid = 2576980377u;
                            source.Low = 2576980378u;
                            num++;
                        }
                    }
                }
            }
            if (num > 0)
            {
                return false;
            }
            if (num <= -29)
            {
                source.High = 0u;
                source.Low = 0u;
                source.Mid = 0u;
                source.Scale = 28;
            }
            else
            {
                source.Scale = -num;
            }
            source.IsNegative = number.IsNegative;
            value = Unsafe.As<MutableDecimal, decimal>(ref source);
            return true;
        }

        public static void DecimalToNumber(decimal value, ref NumberBuffer number)
        {
            ref MutableDecimal reference = ref Unsafe.As<decimal, MutableDecimal>(ref value);
            Span<byte> digits = number.Digits;
            number.IsNegative = reference.IsNegative;
            int num = 29;
            while ((reference.Mid != 0) | (reference.High != 0))
            {
                uint num2 = DecimalDecCalc.DecDivMod1E9(ref reference);
                for (int i = 0; i < 9; i++)
                {
                    digits[--num] = (byte)(num2 % 10u + 48);
                    num2 /= 10u;
                }
            }
            for (uint num3 = reference.Low; num3 != 0; num3 /= 10u)
            {
                digits[--num] = (byte)(num3 % 10u + 48);
            }
            int num4 = 29 - num;
            number.Scale = num4 - reference.Scale;
            Span<byte> digits2 = number.Digits;
            int index = 0;
            while (--num4 >= 0)
            {
                digits2[index++] = digits[num++];
            }
            digits2[index] = 0;
        }

        private static uint DigitsToInt(ReadOnlySpan<byte> digits, int count)
        {
            uint value;
            int bytesConsumed;
            bool flag = Utf8Parser.TryParse(digits.Slice(0, count), out value, out bytesConsumed, 'D');
            return value;
        }

        private static ulong Mul32x32To64(uint a, uint b)
        {
            return (ulong)a * (ulong)b;
        }

        private static ulong Mul64Lossy(ulong a, ulong b, ref int pexp)
        {
            ulong num = Mul32x32To64((uint)(a >> 32), (uint)(b >> 32)) + (Mul32x32To64((uint)(a >> 32), (uint)b) >> 32) + (Mul32x32To64((uint)a, (uint)(b >> 32)) >> 32);
            if ((num & 0x8000000000000000uL) == 0L)
            {
                num <<= 1;
                pexp--;
            }
            return num;
        }

        private static int abs(int value)
        {
            if (value < 0)
            {
                return -value;
            }
            return value;
        }

        private unsafe static double NumberToDouble(ref NumberBuffer number)
        {
            ReadOnlySpan<byte> digits = number.Digits;
            int i = 0;
            int numDigits = number.NumDigits;
            int num = numDigits;
            for (; digits[i] == 48; i++)
            {
                num--;
            }
            if (num == 0)
            {
                return 0.0;
            }
            int num2 = Math.Min(num, 9);
            num -= num2;
            ulong num3 = DigitsToInt(digits, num2);
            if (num > 0)
            {
                num2 = Math.Min(num, 9);
                num -= num2;
                uint b = (uint)(s_rgval64Power10[num2 - 1] >> 64 - s_rgexp64Power10[num2 - 1]);
                num3 = Mul32x32To64((uint)num3, b) + DigitsToInt(digits.Slice(9), num2);
            }
            int num4 = number.Scale - (numDigits - num);
            int num5 = abs(num4);
            if (num5 >= 352)
            {
                ulong num6 = ((num4 > 0) ? 9218868437227405312uL : 0);
                if (number.IsNegative)
                {
                    num6 |= 0x8000000000000000uL;
                }
                return *(double*)(&num6);
            }
            int pexp = 64;
            if ((num3 & 0xFFFFFFFF00000000uL) == 0L)
            {
                num3 <<= 32;
                pexp -= 32;
            }
            if ((num3 & 0xFFFF000000000000uL) == 0L)
            {
                num3 <<= 16;
                pexp -= 16;
            }
            if ((num3 & 0xFF00000000000000uL) == 0L)
            {
                num3 <<= 8;
                pexp -= 8;
            }
            if ((num3 & 0xF000000000000000uL) == 0L)
            {
                num3 <<= 4;
                pexp -= 4;
            }
            if ((num3 & 0xC000000000000000uL) == 0L)
            {
                num3 <<= 2;
                pexp -= 2;
            }
            if ((num3 & 0x8000000000000000uL) == 0L)
            {
                num3 <<= 1;
                pexp--;
            }
            int num7 = num5 & 0xF;
            if (num7 != 0)
            {
                int num8 = s_rgexp64Power10[num7 - 1];
                pexp += ((num4 < 0) ? (-num8 + 1) : num8);
                ulong b2 = s_rgval64Power10[num7 + ((num4 < 0) ? 15 : 0) - 1];
                num3 = Mul64Lossy(num3, b2, ref pexp);
            }
            num7 = num5 >> 4;
            if (num7 != 0)
            {
                int num9 = s_rgexp64Power10By16[num7 - 1];
                pexp += ((num4 < 0) ? (-num9 + 1) : num9);
                ulong b3 = s_rgval64Power10By16[num7 + ((num4 < 0) ? 21 : 0) - 1];
                num3 = Mul64Lossy(num3, b3, ref pexp);
            }
            if (((uint)(int)num3 & 0x400u) != 0)
            {
                ulong num10 = num3 + 1023 + (ulong)(((int)num3 >> 11) & 1);
                if (num10 < num3)
                {
                    num10 = (num10 >> 1) | 0x8000000000000000uL;
                    pexp++;
                }
                num3 = num10;
            }
            pexp += 1022;
            num3 = ((pexp <= 0) ? ((pexp == -52 && num3 >= 9223372036854775896uL) ? 1 : ((pexp > -52) ? (num3 >> -pexp + 11 + 1) : 0)) : ((pexp < 2047) ? ((ulong)((long)pexp << 52) + ((num3 >> 11) & 0xFFFFFFFFFFFFFL)) : 9218868437227405312uL));
            if (number.IsNegative)
            {
                num3 |= 0x8000000000000000uL;
            }
            return *(double*)(&num3);
        }
    }

    internal ref struct NumberBuffer
    {
        public int Scale;

        public bool IsNegative;

        public const int BufferSize = 51;

        private byte _b0;

        private byte _b1;

        private byte _b2;

        private byte _b3;

        private byte _b4;

        private byte _b5;

        private byte _b6;

        private byte _b7;

        private byte _b8;

        private byte _b9;

        private byte _b10;

        private byte _b11;

        private byte _b12;

        private byte _b13;

        private byte _b14;

        private byte _b15;

        private byte _b16;

        private byte _b17;

        private byte _b18;

        private byte _b19;

        private byte _b20;

        private byte _b21;

        private byte _b22;

        private byte _b23;

        private byte _b24;

        private byte _b25;

        private byte _b26;

        private byte _b27;

        private byte _b28;

        private byte _b29;

        private byte _b30;

        private byte _b31;

        private byte _b32;

        private byte _b33;

        private byte _b34;

        private byte _b35;

        private byte _b36;

        private byte _b37;

        private byte _b38;

        private byte _b39;

        private byte _b40;

        private byte _b41;

        private byte _b42;

        private byte _b43;

        private byte _b44;

        private byte _b45;

        private byte _b46;

        private byte _b47;

        private byte _b48;

        private byte _b49;

        private byte _b50;

        public unsafe Span<byte> Digits => new Span<byte>(Unsafe.AsPointer(ref _b0), 51);

        public unsafe byte* UnsafeDigits => (byte*)Unsafe.AsPointer(ref _b0);

        public int NumDigits => Digits.IndexOf<byte>(0);

        [Conditional("DEBUG")]
        public void CheckConsistency()
        {
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append('[');
            stringBuilder.Append('"');
            Span<byte> digits = Digits;
            for (int i = 0; i < 51; i++)
            {
                byte b = digits[i];
                if (b == 0)
                {
                    break;
                }
                stringBuilder.Append((char)b);
            }
            stringBuilder.Append('"');
            stringBuilder.Append(", Scale = " + Scale);
            stringBuilder.Append(", IsNegative   = " + IsNegative);
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }
    }

    internal static class ThrowHelper
    {
        internal static void ThrowArgumentNullException(System.ExceptionArgument argument)
        {
            throw CreateArgumentNullException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentNullException(System.ExceptionArgument argument)
        {
            return new ArgumentNullException(argument.ToString());
        }

        internal static void ThrowArrayTypeMismatchException()
        {
            throw CreateArrayTypeMismatchException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArrayTypeMismatchException()
        {
            return new ArrayTypeMismatchException();
        }

        internal static void ThrowArgumentException_InvalidTypeWithPointersNotSupported(Type type)
        {
            throw CreateArgumentException_InvalidTypeWithPointersNotSupported(type);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_InvalidTypeWithPointersNotSupported(Type type)
        {
            return new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.Argument_InvalidTypeWithPointersNotSupported, type));
        }

        internal static void ThrowArgumentException_DestinationTooShort()
        {
            throw CreateArgumentException_DestinationTooShort();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_DestinationTooShort()
        {
            return new ArgumentException(MDCFR.Properties.Resources.Argument_DestinationTooShort);
        }

        internal static void ThrowIndexOutOfRangeException()
        {
            throw CreateIndexOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateIndexOutOfRangeException()
        {
            return new IndexOutOfRangeException();
        }

        internal static void ThrowArgumentOutOfRangeException()
        {
            throw CreateArgumentOutOfRangeException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException()
        {
            return new ArgumentOutOfRangeException();
        }

        internal static void ThrowArgumentOutOfRangeException(System.ExceptionArgument argument)
        {
            throw CreateArgumentOutOfRangeException(argument);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException(System.ExceptionArgument argument)
        {
            return new ArgumentOutOfRangeException(argument.ToString());
        }

        internal static void ThrowArgumentOutOfRangeException_PrecisionTooLarge()
        {
            throw CreateArgumentOutOfRangeException_PrecisionTooLarge();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_PrecisionTooLarge()
        {
            return new ArgumentOutOfRangeException("precision", System.SR.Format(MDCFR.Properties.Resources.Argument_PrecisionTooLarge, (byte)99));
        }

        internal static void ThrowArgumentOutOfRangeException_SymbolDoesNotFit()
        {
            throw CreateArgumentOutOfRangeException_SymbolDoesNotFit();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_SymbolDoesNotFit()
        {
            return new ArgumentOutOfRangeException("symbol", MDCFR.Properties.Resources.Argument_BadFormatSpecifier);
        }

        internal static void ThrowInvalidOperationException()
        {
            throw CreateInvalidOperationException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException()
        {
            return new InvalidOperationException();
        }

        internal static void ThrowInvalidOperationException_OutstandingReferences()
        {
            throw CreateInvalidOperationException_OutstandingReferences();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_OutstandingReferences()
        {
            return new InvalidOperationException(MDCFR.Properties.Resources.OutstandingReferences);
        }

        internal static void ThrowInvalidOperationException_UnexpectedSegmentType()
        {
            throw CreateInvalidOperationException_UnexpectedSegmentType();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_UnexpectedSegmentType()
        {
            return new InvalidOperationException(MDCFR.Properties.Resources.UnexpectedSegmentType);
        }

        internal static void ThrowInvalidOperationException_EndPositionNotReached()
        {
            throw CreateInvalidOperationException_EndPositionNotReached();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_EndPositionNotReached()
        {
            return new InvalidOperationException(MDCFR.Properties.Resources.EndPositionNotReached);
        }

        internal static void ThrowArgumentOutOfRangeException_PositionOutOfRange()
        {
            throw CreateArgumentOutOfRangeException_PositionOutOfRange();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_PositionOutOfRange()
        {
            return new ArgumentOutOfRangeException("position");
        }

        internal static void ThrowArgumentOutOfRangeException_OffsetOutOfRange()
        {
            throw CreateArgumentOutOfRangeException_OffsetOutOfRange();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_OffsetOutOfRange()
        {
            return new ArgumentOutOfRangeException("offset");
        }

        internal static void ThrowObjectDisposedException_ArrayMemoryPoolBuffer()
        {
            throw CreateObjectDisposedException_ArrayMemoryPoolBuffer();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateObjectDisposedException_ArrayMemoryPoolBuffer()
        {
            return new ObjectDisposedException("ArrayMemoryPoolBuffer");
        }

        internal static void ThrowFormatException_BadFormatSpecifier()
        {
            throw CreateFormatException_BadFormatSpecifier();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateFormatException_BadFormatSpecifier()
        {
            return new FormatException(MDCFR.Properties.Resources.Argument_BadFormatSpecifier);
        }

        internal static void ThrowArgumentException_OverlapAlignmentMismatch()
        {
            throw CreateArgumentException_OverlapAlignmentMismatch();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_OverlapAlignmentMismatch()
        {
            return new ArgumentException(MDCFR.Properties.Resources.Argument_OverlapAlignmentMismatch);
        }

        internal static void ThrowNotSupportedException()
        {
            throw CreateThrowNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateThrowNotSupportedException()
        {
            return new NotSupportedException();
        }

        public static bool TryFormatThrowFormatException(out int bytesWritten)
        {
            bytesWritten = 0;
            ThrowFormatException_BadFormatSpecifier();
            return false;
        }

        public static bool TryParseThrowFormatException<T>(out T value, out int bytesConsumed)
        {
            value = default(T);
            bytesConsumed = 0;
            ThrowFormatException_BadFormatSpecifier();
            return false;
        }

        public static void ThrowArgumentValidationException<T>(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment)
        {
            throw CreateArgumentValidationException(startSegment, startIndex, endSegment);
        }

        private static Exception CreateArgumentValidationException<T>(ReadOnlySequenceSegment<T> startSegment, int startIndex, ReadOnlySequenceSegment<T> endSegment)
        {
            if (startSegment == null)
            {
                return CreateArgumentNullException(System.ExceptionArgument.startSegment);
            }
            if (endSegment == null)
            {
                return CreateArgumentNullException(System.ExceptionArgument.endSegment);
            }
            if (startSegment != endSegment && startSegment.RunningIndex > endSegment.RunningIndex)
            {
                return CreateArgumentOutOfRangeException(System.ExceptionArgument.endSegment);
            }
            if ((uint)startSegment.Memory.Length < (uint)startIndex)
            {
                return CreateArgumentOutOfRangeException(System.ExceptionArgument.startIndex);
            }
            return CreateArgumentOutOfRangeException(System.ExceptionArgument.endIndex);
        }

        public static void ThrowArgumentValidationException(Array array, int start)
        {
            throw CreateArgumentValidationException(array, start);
        }

        private static Exception CreateArgumentValidationException(Array array, int start)
        {
            if (array == null)
            {
                return CreateArgumentNullException(System.ExceptionArgument.array);
            }
            if ((uint)start > (uint)array.Length)
            {
                return CreateArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return CreateArgumentOutOfRangeException(System.ExceptionArgument.length);
        }

        public static void ThrowStartOrEndArgumentValidationException(long start)
        {
            throw CreateStartOrEndArgumentValidationException(start);
        }

        private static Exception CreateStartOrEndArgumentValidationException(long start)
        {
            if (start < 0)
            {
                return CreateArgumentOutOfRangeException(System.ExceptionArgument.start);
            }
            return CreateArgumentOutOfRangeException(System.ExceptionArgument.length);
        }
    }

    public readonly struct SequencePosition : IEquatable<SequencePosition>
    {
        private readonly object _object;

        private readonly int _integer;

        public SequencePosition(object @object, int integer)
        {
            _object = @object;
            _integer = integer;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public object GetObject()
        {
            return _object;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public int GetInteger()
        {
            return _integer;
        }

        public bool Equals(SequencePosition other)
        {
            if (_integer == other._integer)
            {
                return object.Equals(_object, other._object);
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj is SequencePosition other)
            {
                return Equals(other);
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return System.Numerics.Hashing.HashHelpers.Combine(_object?.GetHashCode() ?? 0, _integer);
        }
    }

    internal static class MathF
    {
        public const float PI = 3.1415927f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float x)
        {
            return Math.Abs(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Acos(float x)
        {
            return (float)Math.Acos(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float x)
        {
            return (float)Math.Cos(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float IEEERemainder(float x, float y)
        {
            return (float)Math.IEEERemainder(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow(float x, float y)
        {
            return (float)Math.Pow(x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float x)
        {
            return (float)Math.Sin(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float x)
        {
            return (float)Math.Sqrt(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan(float x)
        {
            return (float)Math.Tan(x);
        }
    }

}