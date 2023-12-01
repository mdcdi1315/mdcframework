/*
 * Most of the code used here is from .NET Foundation. A small license excerpt is here:
 * 
 * 
 * Licensed to the .NET Foundation under one or more agreements.
 * The .NET Foundation licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
*/

using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


#if NET6_0_OR_GREATER == false
// Types exposed in .NET Framework 4.8. :

namespace Microsoft.Win32.SafeHandles
{
    internal sealed class SafeAllocHHandle : SafeBuffer
    {
        internal static SafeAllocHHandle InvalidHandle => new SafeAllocHHandle(System.IntPtr.Zero);

        public SafeAllocHHandle() : base(ownsHandle: true) { }

        internal SafeAllocHHandle(System.IntPtr handle) : base(ownsHandle: true) { SetHandle(handle); }

        protected override bool ReleaseHandle()
        {
            if (handle != System.IntPtr.Zero) { Marshal.FreeHGlobal(handle); }
            return true;
        }
    }

    internal sealed class SafeFindHandle : SafeHandle
    {
        public override bool IsInvalid
        {
            get
            {
                if (!(handle == System.IntPtr.Zero)) { return handle == new System.IntPtr(-1); }
                return true;
            }
        }

        public SafeFindHandle() : base(System.IntPtr.Zero, ownsHandle: true) { }

        protected override bool ReleaseHandle() { return global::Interop.Kernel32.FindClose(handle); }
    }
}

namespace Internal
{
    [StructLayout(LayoutKind.Explicit, Size = 124)]
    internal struct PaddingFor32 { }

    internal static class PaddingHelpers { internal const int CACHE_LINE_SIZE = 128; }

    namespace Reflection.Core.Execution
    {
        internal static class ReflectionCoreExecution
        {
            internal static class ExecutionDomain
            {
                internal static System.Exception CreateMissingMetadataException(System.Type t)
                {
                    return new System.Exception();
                }
            }
        }
    }
}

namespace System
{
    using System.Reflection.TypeLoading;
    #nullable enable

    namespace Collections
    {
        namespace Concurrent
        {
            internal abstract class ConcurrentUnifier<K, V> where K : IEquatable<K> where V : class
            {
                private readonly ConcurrentDictionary<K, V> _dict = new ConcurrentDictionary<K, V>();

                public V GetOrAdd(K key) { return _dict.GetOrAdd(key, Factory); }

                protected abstract V Factory(K key);
            }
        }

        namespace Generic
        {
            using System.Threading;

            internal interface IHashKeyCollection<in TKey> { IEqualityComparer<TKey> KeyComparer { get; } }

            internal interface ISortKeyCollection<in TKey> { IComparer<TKey> KeyComparer { get; } }

#pragma warning disable CS8601
            internal static class EnumerableHelpers
            {
                internal static T[] ToArray<T>(IEnumerable<T> source) { int length; T[] array = ToArray(source, out length); Array.Resize(ref array, length); return array; }

                internal static T[] ToArray<T>(IEnumerable<T> source, out int length)
                {
                    if (source is ICollection<T> collection)
                    {
                        int count = collection.Count;
                        if (count != 0) { T[] array = new T[count]; collection.CopyTo(array, 0); length = count; return array; }
                    }
                    else
                    {
                        using IEnumerator<T> enumerator = source.GetEnumerator();
                        if (enumerator.MoveNext())
                        {
                            T[] array2 = new T[4]
                            {
                        enumerator.Current,
                        default(T),
                        default(T),
                        default(T)
                            };
                            int num = 1;
                            while (enumerator.MoveNext())
                            {
                                if (num == array2.Length)
                                {
                                    int num2 = num << 1;
                                    if ((uint)num2 > 2146435071u) { num2 = ((2146435071 <= num) ? (num + 1) : 2146435071); }
                                    Array.Resize(ref array2, num2);
                                }
                                array2[num++] = enumerator.Current;
                            }
                            length = num;
                            return array2;
                        }
                    }
                    length = 0;
                    return Array.Empty<T>();
                }
            }
#pragma warning restore CS8601

            /// <summary>Exposes an enumerator that provides asynchronous iteration over values of a specified type.</summary>
            /// <typeparam name="T">The type of values to enumerate.</typeparam>
            public interface IAsyncEnumerable<out T>
            {
                /// <summary>Returns an enumerator that iterates asynchronously through the collection.</summary>
                /// <param name="cancellationToken">A <see cref="CancellationToken" /> that may be used to cancel the asynchronous iteration.</param>
                /// <returns>An enumerator that can be used to iterate asynchronously through the collection.</returns>
                IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken));
            }

            /// <summary>Supports a simple asynchronous iteration over a generic collection.</summary>
            /// <typeparam name="T">The type of objects to enumerate.</typeparam>
            public interface IAsyncEnumerator<out T> : IAsyncDisposable
            {
                /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
                T Current { get; }

                /// <summary>Advances the enumerator asynchronously to the next element of the collection.</summary>
                /// <returns>
                /// A <see cref="T:System.Threading.Tasks.ValueTask`1" /> that will complete with a result of <c>true</c> if the enumerator
                /// was successfully advanced to the next element, or <c>false</c> if the enumerator has passed the end
                /// of the collection.
                /// </returns>
                Threading.Tasks.ValueTask<bool> MoveNextAsync();
            }

            /// <summary>
            /// An <see cref="IEqualityComparer{T}" /> that uses reference equality (<see cref="M:System.Object.ReferenceEquals(System.Object,System.Object)" />)
            /// instead of value equality (<see cref="System.Object.Equals(System.Object)" />) when comparing two object instances.
            /// </summary>
            /// <remarks>
            /// The <see cref="ReferenceEqualityComparer" /> type cannot be instantiated. Instead, use the <see cref="P:System.Collections.Generic.ReferenceEqualityComparer.Instance" /> property
            /// to access the singleton instance of this type.
            /// </remarks>
            public sealed class ReferenceEqualityComparer : IEqualityComparer<object>, IEqualityComparer
            {
                /// <summary>
                /// Gets the singleton <see cref="ReferenceEqualityComparer" /> instance.
                /// </summary>
                public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

                private ReferenceEqualityComparer() { }

                /// <summary>
                /// Determines whether two object references refer to the same object instance.
                /// </summary>
                /// <param name="x">The first object to compare.</param>
                /// <param name="y">The second object to compare.</param>
                /// <returns>
                /// <see langword="true" /> if both <paramref name="x" /> and <paramref name="y" /> refer to the same object instance
                /// or if both are <see langword="null" />; otherwise, <see langword="false" />.
                /// </returns>
                /// <remarks>
                /// This API is a wrapper around <see cref="Object.ReferenceEquals(System.Object,System.Object)" />.
                /// It is not necessarily equivalent to calling <see cref="Object.Equals(System.Object,System.Object)" />.
                /// </remarks>
                public new bool Equals(object x, object y) { return x == y; }

                /// <summary>
                /// Returns a hash code for the specified object. The returned hash code is based on the object
                /// identity, not on the contents of the object.
                /// </summary>
                /// <param name="obj">The object for which to retrieve the hash code.</param>
                /// <returns>A hash code for the identity of <paramref name="obj" />.</returns>
                /// <remarks>
                /// This API is a wrapper around <see cref="RuntimeHelpers.GetHashCode(System.Object)" />.
                /// It is not necessarily equivalent to calling <see cref="Object.GetHashCode" />.
                /// </remarks>
                public int GetHashCode(object obj) { return RuntimeHelpers.GetHashCode(obj); }
            }

            /// <summary>Polyfills for <see cref="Stack{T}" />.</summary>
            internal static class StackExtensions
            {
                public static bool TryPeek<T>(this Stack<T> stack, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T result)
                {
                    if (stack.Count > 0) { result = stack.Peek(); return true; }
                    result = default(T);
                    return false;
                }

                public static bool TryPop<T>(this Stack<T> stack, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out T result)
                {
                    if (stack.Count > 0) { result = stack.Pop(); return true; }
                    result = default(T);
                    return false;
                }
            }
        }
    }

    namespace Runtime
    {
        namespace CompilerServices
        {
            using System.Threading;
            using System.Threading.Tasks;
            using System.Threading.Tasks.Sources;
            using System.Collections.Generic;

            // Code required when the Snappy Archiving is compiled < .NET 6 .
            // <--
            // Licensed to the .NET Foundation under one or more agreements.
            // The .NET Foundation licenses this file to you under the MIT license.

            [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
            internal sealed class CallerArgumentExpressionAttribute : Attribute
            {
                public CallerArgumentExpressionAttribute(string parameterName) { ParameterName = parameterName; }

                public string ParameterName { get; }
            }
            // -->

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

            /// <summary></summary>
            [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
            public sealed class EnumeratorCancellationAttribute : Attribute { }

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
                    if (_useBuilder) { _methodBuilder.SetResult(result); return; } else { _result = result; _haveResult = true; }
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
                        { return default; }
                        else { _useBuilder = true; return new ValueTask<TResult>(_methodBuilder.Task); }

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

            [CompilerGenerated]
            [AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
            internal sealed class NullablePublicOnlyAttribute : Attribute
            {
                public readonly bool IncludesInternals;

                public NullablePublicOnlyAttribute(bool includes_internals)
                {
                    IncludesInternals = includes_internals;
                }
            }

            /// <summary>
            /// Indicates that the use of <see cref="ValueTuple" /> on a member is meant to be treated as a tuple with element names.
            /// </summary>
            [CLSCompliant(false)]
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
            public sealed class TupleElementNamesAttribute : Attribute
            {
                private readonly string[] _transformNames;

                /// <summary>
                /// Specifies, in a pre-order depth-first traversal of a type's
                /// construction, which <see cref="ValueTuple" /> elements are
                /// meant to carry element names.
                /// </summary>
                public IList<string> TransformNames => _transformNames;

                /// <summary>
                /// Initializes a new instance of the <see cref="TupleElementNamesAttribute" /> class.
                /// </summary>
                /// <param name="transformNames">
                /// Specifies, in a pre-order depth-first traversal of a type's
                /// construction, which <see cref="ValueType" /> occurrences are
                /// meant to carry element names.
                /// </param>
                /// <remarks>
                /// This constructor is meant to be used on types that contain an
                /// instantiation of <see cref="ValueType" /> that contains
                /// element names.  For instance, if <c>C</c> is a generic type with
                /// two type parameters, then a use of the constructed type 
                /// <c>C{<see cref="ValueTuple{T1, T}" />, <see cref="ValueTuple{T1 , T2 , T3}" /></c> might be intended to
                /// treat the first type argument as a tuple with element names and the
                /// second as a tuple without element names. In which case, the
                /// appropriate attribute specification should use a
                /// <paramref name="transformNames"/>
                /// value of <c>{ "name1", "name2", null, null, null }</c>.
                /// </remarks>
                public TupleElementNamesAttribute(string[] transformNames)
                {
                    if (transformNames == null)
                    {
                        throw new ArgumentNullException("transformNames");
                    }
                    _transformNames = transformNames;
                }
            }

            /// <summary>Indicates whether a method is an asynchronous iterator.</summary>
            [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
            public sealed class AsyncIteratorStateMachineAttribute : StateMachineAttribute
            {
                /// <summary>Initializes a new instance of the <see cref="AsyncIteratorStateMachineAttribute" /> class.</summary>
                /// <param name="stateMachineType">The type object for the underlying state machine type that's used to implement a state machine method.</param>
                public AsyncIteratorStateMachineAttribute(Type stateMachineType)
                    : base(stateMachineType) { }
            }

            /// <summary>Provides an awaitable async enumerable that enables cancelable iteration and configured awaits.</summary>
            [StructLayout(LayoutKind.Auto)]
            public readonly struct ConfiguredCancelableAsyncEnumerable<T>
            {
                /// <summary>Provides an awaitable async enumerator that enables cancelable iteration and configured awaits.</summary>
                [StructLayout(LayoutKind.Auto)]
                public readonly struct Enumerator
                {
                    private readonly IAsyncEnumerator<T> _enumerator;

                    private readonly bool _continueOnCapturedContext;

                    /// <summary>Gets the element in the collection at the current position of the enumerator.</summary>
                    public T Current => _enumerator.Current;

                    internal Enumerator(IAsyncEnumerator<T> enumerator, bool continueOnCapturedContext)
                    {
                        _enumerator = enumerator;
                        _continueOnCapturedContext = continueOnCapturedContext;
                    }

                    /// <summary>Advances the enumerator asynchronously to the next element of the collection.</summary>
                    /// <returns>
                    /// A <see cref="T:System.Runtime.CompilerServices.ConfiguredValueTaskAwaitable`1" /> that will complete with a result of <c>true</c>
                    /// if the enumerator was successfully advanced to the next element, or <c>false</c> if the enumerator has
                    /// passed the end of the collection.
                    /// </returns>
                    public ConfiguredValueTaskAwaitable<bool> MoveNextAsync()
                    {
                        return _enumerator.MoveNextAsync().ConfigureAwait(_continueOnCapturedContext);
                    }

                    /// <summary>
                    /// Performs application-defined tasks associated with freeing, releasing, or
                    /// resetting unmanaged resources asynchronously.
                    /// </summary>
                    public ConfiguredValueTaskAwaitable DisposeAsync()
                    {
                        return _enumerator.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
                    }
                }

                private readonly IAsyncEnumerable<T> _enumerable;

                private readonly CancellationToken _cancellationToken;

                private readonly bool _continueOnCapturedContext;

                internal ConfiguredCancelableAsyncEnumerable(IAsyncEnumerable<T> enumerable, bool continueOnCapturedContext, CancellationToken cancellationToken)
                {
                    _enumerable = enumerable;
                    _continueOnCapturedContext = continueOnCapturedContext;
                    _cancellationToken = cancellationToken;
                }

                /// <summary>Configures how awaits on the tasks returned from an async iteration will be performed.</summary>
                /// <param name="continueOnCapturedContext">Whether to capture and marshal back to the current context.</param>
                /// <returns>The configured enumerable.</returns>
                /// <remarks>This will replace any previous value set by <see cref="M:System.Runtime.CompilerServices.ConfiguredCancelableAsyncEnumerable`1.ConfigureAwait(System.Boolean)" /> for this iteration.</remarks>
                public ConfiguredCancelableAsyncEnumerable<T> ConfigureAwait(bool continueOnCapturedContext)
                {
                    return new ConfiguredCancelableAsyncEnumerable<T>(_enumerable, continueOnCapturedContext, _cancellationToken);
                }

                /// <summary>Sets the <see cref="T:System.Threading.CancellationToken" /> to be passed to <see cref="M:System.Collections.Generic.IAsyncEnumerable`1.GetAsyncEnumerator(System.Threading.CancellationToken)" /> when iterating.</summary>
                /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> to use.</param>
                /// <returns>The configured enumerable.</returns>
                /// <remarks>This will replace any previous <see cref="T:System.Threading.CancellationToken" /> set by <see cref="M:System.Runtime.CompilerServices.ConfiguredCancelableAsyncEnumerable`1.WithCancellation(System.Threading.CancellationToken)" /> for this iteration.</remarks>
                public ConfiguredCancelableAsyncEnumerable<T> WithCancellation(CancellationToken cancellationToken)
                {
                    return new ConfiguredCancelableAsyncEnumerable<T>(_enumerable, _continueOnCapturedContext, cancellationToken);
                }

                public Enumerator GetAsyncEnumerator()
                {
                    return new Enumerator(_enumerable.GetAsyncEnumerator(_cancellationToken), _continueOnCapturedContext);
                }
            }

            /// <summary>Provides a type that can be used to configure how awaits on an <see cref="System.IAsyncDisposable" /> are performed.</summary>
            [StructLayout(LayoutKind.Auto)]
            public readonly struct ConfiguredAsyncDisposable
            {
                private readonly IAsyncDisposable _source;

                private readonly bool _continueOnCapturedContext;

                internal ConfiguredAsyncDisposable(IAsyncDisposable source, bool continueOnCapturedContext)
                {
                    _source = source;
                    _continueOnCapturedContext = continueOnCapturedContext;
                }

                /// <summary>
                /// Performs application-defined tasks associated with freeing, releasing, or
                /// resetting unmanaged resources asynchronously.
                /// </summary>
                public ConfiguredValueTaskAwaitable DisposeAsync()
                {
                    return _source.DisposeAsync().ConfigureAwait(_continueOnCapturedContext);
                }
            }

            /// <summary>Represents a builder for asynchronous iterators.</summary>
            [StructLayout(LayoutKind.Auto)]
            public struct AsyncIteratorMethodBuilder
            {
                private AsyncTaskMethodBuilder _methodBuilder;

                private object _id;

                /// <summary>Gets an object that may be used to uniquely identify this builder to the debugger.</summary>
                internal object ObjectIdForDebugger => _id ?? Interlocked.CompareExchange(ref _id, new object(), null) ?? _id;

                /// <summary>Creates an instance of the <see cref="T:System.Runtime.CompilerServices.AsyncIteratorMethodBuilder" /> struct.</summary>
                /// <returns>The initialized instance.</returns>
                public static AsyncIteratorMethodBuilder Create()
                {
                    AsyncIteratorMethodBuilder result = default(AsyncIteratorMethodBuilder);
                    result._methodBuilder = AsyncTaskMethodBuilder.Create();
                    return result;
                }

                /// <summary>Invokes <see cref="M:System.Runtime.CompilerServices.IAsyncStateMachine.MoveNext" /> on the state machine while guarding the <see cref="T:System.Threading.ExecutionContext" />.</summary>
                /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
                /// <param name="stateMachine">The state machine instance, passed by reference.</param>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void MoveNext<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
                {
                    _methodBuilder.Start(ref stateMachine);
                }

                /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
                /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
                /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
                /// <param name="awaiter">The awaiter.</param>
                /// <param name="stateMachine">The state machine.</param>
                public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
                {
                    _methodBuilder.AwaitOnCompleted(ref awaiter, ref stateMachine);
                }

                /// <summary>Schedules the state machine to proceed to the next action when the specified awaiter completes.</summary>
                /// <typeparam name="TAwaiter">The type of the awaiter.</typeparam>
                /// <typeparam name="TStateMachine">The type of the state machine.</typeparam>
                /// <param name="awaiter">The awaiter.</param>
                /// <param name="stateMachine">The state machine.</param>
                public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
                {
                    _methodBuilder.AwaitUnsafeOnCompleted(ref awaiter, ref stateMachine);
                }

                /// <summary>Marks iteration as being completed, whether successfully or otherwise.</summary>
                public void Complete()
                {
                    _methodBuilder.SetResult();
                }
            }

            /// <summary>
            /// Reserved to be used by the compiler for tracking metadata.
            /// This class should not be used by developers in source code.
            /// </summary>
            [ComponentModel.EditorBrowsable(ComponentModel.EditorBrowsableState.Never)]
            public static class IsExternalInit { }

            [CompilerGenerated]
            [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
            internal sealed class ScopedRefAttribute : Attribute { }
        }
    
        namespace InteropServices
        {

            using System.Buffers;

            /// <summary>
            /// Provides methods to interoperate with <see cref="System.Memory{T}"/>, 
            /// <see cref="System.ReadOnlySpan{T}"/>, <see cref="System.Span{T}"/>, and 
            /// <see cref="System.ReadOnlyMemory{T}"/>.
            /// </summary>
            public static class MemoryMarshal
            {

                /// <summary>
                /// Tries to get an array segment from the underlying memory buffer. 
                /// The return value indicates the success of the operation.
                /// </summary>
                /// <typeparam name="T">The type of items in the read-only memory buffer.</typeparam>
                /// <param name="memory">A read-only memory buffer.</param>
                /// <param name="segment">When this method returns, contains the array segment retrieved from the underlying read-only memory buffer. 
                /// If the method fails, the method returns a default array segment.</param>
                /// <returns><c>true</c> if the method call succeeds; <c>false</c> otherwise.</returns>
                /// <remarks>
                /// CAUTION!!!! <see cref="System.ReadOnlyMemory{T}"/> is used to represent immutable data. 
                /// <see cref="System.ArraySegment{T}"/> instances returned by this method should not be written to, 
                /// and the wrapped array instance should only be passed to methods which treat the array contents as read-only.
                /// </remarks>
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

                /// <summary>
                /// Tries to retrieve a <see cref="System.Buffers.MemoryManager{T}" /> 
                /// from the underlying read-only memory buffer.
                /// </summary>
                /// <typeparam name="T">The type of the items in the read-only memory buffer.</typeparam>
                /// <typeparam name="TManager">The type of the <see cref="System.Buffers.MemoryManager{T}"/> 
                /// to retrieve.</typeparam>
                /// <param name="memory">The read-only memory buffer for which to get the memory manager.</param>
                /// <param name="manager">When the method returns, the manager of <paramref name="memory"/>.</param>
                /// <returns><c>true</c> if the method retrieved the memory manager; otherwise, <c>false</c>.</returns>
                public static bool TryGetMemoryManager<T, TManager>(ReadOnlyMemory<T> memory, out TManager manager) where TManager : MemoryManager<T>
                {
                    int start;
                    int length;
                    TManager val = (manager = memory.GetObjectStartLength(out start, out length) as TManager);
                    return manager != null;
                }

                /// <summary>
                /// Tries to retrieve a <see cref="System.Buffers.MemoryManager{T}" /> , 
                /// start index, and length from the underlying read-only memory buffer.
                /// </summary>
                /// <typeparam name="T">The type of the items in the read-only memory buffer.</typeparam>
                /// <typeparam name="TManager">The type of the <see cref="System.Buffers.MemoryManager{T}"/> </typeparam>
                /// <param name="memory">The read-only memory buffer for which to get the memory manager.</param>
                /// <param name="manager">When the method returns, the manager of <paramref name="memory"/>.</param>
                /// <param name="start">When the method returns, the offset from the start of the <paramref name="manager"/> that the 
                /// <paramref name="memory"/> represents.</param>
                /// <param name="length">When the method returns, the length of the 
                /// <paramref name="manager"/> that the <paramref name="memory"/> represents.</param>
                /// <returns><c>true</c> if the method retrieved the memory manager; otherwise, <c>false</c>.</returns>
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

                /// <summary>
                /// Creates an <see cref="System.Collections.Generic.IEnumerable{T}"/> view of the given read-only memory buffer.
                /// </summary>
                /// <typeparam name="T">The type of the items in the read-only memory buffer.</typeparam>
                /// <param name="memory">A read-only memory buffer.</param>
                /// <returns>An enumerable view of <paramref name="memory"/>.</returns>
                /// <remarks>
                /// This method allows a read-only memory buffer to be used in 
                /// existing APIs that require a parameter of type 
                /// <see cref="System.Collections.Generic.IEnumerable{T}"/>.</remarks>
                public static System.Collections.Generic.IEnumerable<T> ToEnumerable<T>(ReadOnlyMemory<T> memory)
                {
                    for (int i = 0; i < memory.Length; i++) { yield return memory.Span[i]; }
                }

                /// <summary>
                /// Tries to get the underlying string from a <see cref="System.ReadOnlyMemory{T}"/>.
                /// T is <see cref="System.Char"/> .
                /// </summary>
                /// <param name="memory">Read-only memory containing a block of characters.</param>
                /// <param name="text">When the method returns, the string contained in the memory buffer.</param>
                /// <param name="start">The starting location in <paramref name="text"/>.</param>
                /// <param name="length">The number of characters in <paramref name="text"/>.</param>
                /// <returns><c>true</c> if the method successfully retrieves the underlying string; otherwise, <c>false</c>.</returns>
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

                /// <summary>
                /// Reads a structure of type <typeparamref name="T"/> out of a read-only span of bytes.
                /// </summary>
                /// <typeparam name="T">The type of the item to retrieve from the read-only span.</typeparam>
                /// <param name="source">A read-only span.</param>
                /// <returns>The structure retrieved from the read-only span.</returns>
                /// <remarks>
                /// <para>
                /// <typeparamref name="T"/> cannot contain managed object references. The <see cref="Read{T}(ReadOnlySpan{byte})"/> 
                /// method performs this check at runtime and throws <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// CAUTION: This method initializes an instance of <typeparamref name="T"/>, including private instance fields and other 
                /// implementation details, from the raw binary contents of the source span. Callers must ensure that the contents of the 
                /// source span are well-formed with regard to <typeparamref name="T"/>'s internal invariants.
                /// </para>
                /// </remarks>
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

                /// <summary>
                /// Tries to read a structure of type <typeparamref name="T"/> from a read-only span of bytes.
                /// </summary>
                /// <typeparam name="T">The type of the structure to retrieve.</typeparam>
                /// <param name="source">A read-only span of bytes.</param>
                /// <param name="value">When the method returns, an instance of <typeparamref name="T"/>.</param>
                /// <returns><c>true</c> if the method succeeds in retrieving an instance of the structure; otherwise, <c>false</c>.</returns>
                /// <remarks>
                /// <para>
                /// <typeparamref name="T"/> cannot contain managed object references. The <see cref="TryRead{T}(ReadOnlySpan{byte}, out T)"/> 
                /// method performs this check at runtime and throws <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// CAUTION: This method initializes an instance of <typeparamref name="T"/>, including private instance fields and other 
                /// implementation details, from the raw binary contents of the source span. Callers must ensure that the contents of the 
                /// source span are well-formed with regard to <typeparamref name="T"/>'s internal invariants.
                /// </para>
                /// </remarks>
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

                /// <summary>
                /// Writes a structure of type <typeparamref name="T"/> into a span of bytes.
                /// </summary>
                /// <typeparam name="T">The type of the structure.</typeparam>
                /// <param name="destination">The span of bytes to contain the structure.</param>
                /// <param name="value">The structure to be written to the span.</param>
                /// <remarks>
                /// <para>
                /// <typeparamref name="T"/> cannot contain managed object references. The <see cref="Write{T}(Span{byte}, ref T)"/> 
                /// method performs this check at runtime and throws <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// CAUTION: This method initializes an instance of <typeparamref name="T"/>, including private instance fields and other 
                /// implementation details, from the raw binary contents of the source span. Callers must ensure that the contents of the 
                /// source span are well-formed with regard to <typeparamref name="T"/>'s internal invariants.
                /// </para>
                /// </remarks>
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

                /// <summary>
                /// Tries to write a structure of type <typeparamref name="T"/> into a span of bytes.
                /// </summary>
                /// <typeparam name="T">The type of the structure.</typeparam>
                /// <param name="destination">The span of bytes to contain the structure.</param>
                /// <param name="value">The structure to be written to the span.</param>
                /// <returns><c>true</c> if the write operation succeeded; otherwise, <c>false</c>. 
                /// The method returns <c>false</c> if the span is too small to contain <typeparamref name="T"/>
                /// .</returns>
                /// <remarks>
                /// <para>
                /// <typeparamref name="T"/> cannot contain managed object references. The <see cref="Write{T}(Span{byte}, ref T)"/> 
                /// method performs this check at runtime and throws <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// CAUTION: This method initializes an instance of <typeparamref name="T"/>, including private instance fields and other 
                /// implementation details, from the raw binary contents of the source span. Callers must ensure that the contents of the 
                /// source span are well-formed with regard to <typeparamref name="T"/>'s internal invariants.
                /// </para>
                /// </remarks>
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

                /// <summary>
                /// Creates a new memory buffer over the portion of the pre-pinned target array 
                /// beginning at the <paramref name="start"/> index and consisting of <paramref name="length"/> items.
                /// </summary>
                /// <typeparam name="T">The type of the array.</typeparam>
                /// <param name="array">The pre-pinned source array.</param>
                /// <param name="start">The index of <paramref name="array"/> 
                /// at which to begin the memory block.</param>
                /// <param name="length">The number of items to include in the
                /// memory block.</param>
                /// <returns>A block of memory over the specified elements of <paramref name="array"/>. 
                /// If <paramref name="array"/> is <c>null</c>, or if <paramref name="start"/> and 
                /// <paramref name="length"/> are 0, the method returns a <see cref="Memory{T}"/> 
                /// instance of Length zero.</returns>
                /// <remarks>
                /// The array must already be pinned before this method is called, and that array must not 
                /// be unpinned while the <see cref="Memory{T}"/> buffer that it returns is still in use. 
                /// Calling this method on an unpinned array could result in memory corruption.
                /// </remarks>
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static Memory<T> CreateFromPinnedArray<T>(T[] array, int start, int length)
                {
                    if (array == null)
                    {
                        if (start != 0 || length != 0)
                        {
                            System.ThrowHelper.ThrowArgumentOutOfRangeException();
                        }
                        return default;
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

                /// <summary>
                /// Casts a <see cref="Span{T}"/> of one primitive type, <typeparamref name="T"/>, to a Span&lt;Byte&gt; .
                /// </summary>
                /// <typeparam name="T">The type of items in the span.</typeparam>
                /// <param name="span">The source slice to convert.</param>
                /// <returns>A span of type <see cref="System.Byte"/>.</returns>
                /// <remarks>
                /// <para>
                /// <typeparamref name="T"/> cannot contain managed object references. The <see cref="AsBytes{T}(Span{T})"/> 
                /// method performs this check at runtime and throws <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// CAUTION: This method provides a raw binary projection over the original span, including private instance fields and other 
                /// implementation details of type <typeparamref name="T"/>. Callers should ensure that their code is resilient 
                /// to changes in the internal layout of <typeparamref name="T"/>.
                /// </para>
                /// </remarks>
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

                /// <summary>
                /// Casts a <see cref="ReadOnlySpan{T}"/> of one primitive type, <typeparamref name="T"/>, to a ReadOnlySpan&lt;Byte&gt; .
                /// </summary>
                /// <typeparam name="T">The type of items in the read-only span.</typeparam>
                /// <param name="span">The source slice to convert.</param>
                /// <returns>A read-only span of type <see cref="System.Byte"/>.</returns>
                /// <remarks>
                /// <para>
                /// <typeparamref name="T"/> cannot contain managed object references. The <see cref="AsBytes{T}(Span{T})"/> 
                /// method performs this check at runtime and throws <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// CAUTION: This method provides a raw binary projection over the original span, including private instance fields and other 
                /// implementation details of type <typeparamref name="T"/>. Callers should ensure that their code is resilient 
                /// to changes in the internal layout of <typeparamref name="T"/>.
                /// </para>
                /// </remarks>
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

                /// <summary>
                /// Creates a <see cref="Memory{T}"/> instance from a <see cref="ReadOnlyMemory{T}"/>.
                /// </summary>
                /// <typeparam name="T">The type of items in the read-only memory buffer.</typeparam>
                /// <param name="memory">The read-only memory buffer.</param>
                /// <returns>A memory block that represents the same memory as the <see cref="ReadOnlyMemory{T}"/>.</returns>
                /// <remarks>
                /// CAUTION!!! This method must be used with extreme caution. <see cref="ReadOnlyMemory{T}"/> is used to represent 
                /// immutable data and other memory that is not meant to be written to. <see cref="Memory{T}"/> instances created by 
                /// this method should not be written to. The purpose of this method is to allow variables typed as <see cref="Memory{T}"/> 
                /// but only used for reading to store a <see cref="ReadOnlyMemory{T}"/>. 
                /// </remarks>
                public static Memory<T> AsMemory<T>(ReadOnlyMemory<T> memory)
                {
                    return Unsafe.As<ReadOnlyMemory<T>, Memory<T>>(ref memory);
                }

                /// <summary>Returns a reference to the element of the span at index 0. </summary>
                /// <typeparam name="T">The type of items in the span.</typeparam>
                /// <param name="span">The span from which the reference is retrieved.</param>
                /// <returns>A reference to the element at index 0.</returns>
                /// <remarks>
                /// If the span is empty, this method returns a reference to the location where the element at index 0 would have been stored. 
                /// Such a reference may or may not be <c>null</c>. 
                /// The returned reference can be used for pinning, but it must never be dereferenced.
                /// </remarks>
                public unsafe static ref T GetReference<T>(Span<T> span)
                {
                    if (span.Pinnable == null)
                    {
                        return ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer());
                    }
                    return ref Unsafe.AddByteOffset(ref span.Pinnable.Data, span.ByteOffset);
                }

                /// <summary>Returns a reference to the element of the read-only span at index 0. </summary>
                /// <typeparam name="T">The type of items in the read-only span.</typeparam>
                /// <param name="span">The read-only span from which the reference is retrieved.</param>
                /// <returns>A reference to the element at index 0.</returns>
                /// <remarks>
                /// If the read-only span is empty, this method returns a reference to the location where the element at index 0 
                /// would have been stored.  Such a reference may or may not be <c>null</c>. 
                /// The returned reference can be used for pinning, but it must never be dereferenced.
                /// </remarks>
                public unsafe static ref T GetReference<T>(ReadOnlySpan<T> span)
                {
                    if (span.Pinnable == null)
                    {
                        return ref Unsafe.AsRef<T>(span.ByteOffset.ToPointer());
                    }
                    return ref Unsafe.AddByteOffset(ref span.Pinnable.Data, span.ByteOffset);
                }

                /// <summary>Casts a span of one primitive type to a span of another primitive type. </summary>
                /// <typeparam name="TFrom">The type of the source span.</typeparam>
                /// <typeparam name="TTo">The type of the target span.</typeparam>
                /// <param name="span">The source slice to convert.</param>
                /// <returns>The converted span.</returns>
                /// <remarks>
                /// <para>
                /// Neither <typeparamref name="TFrom"/> nor <typeparamref name="TTo"/> can contain managed object references. 
                /// The <see cref="Cast{TFrom, TTo}(Span{TFrom})"/> method performs this check at runtime and throws 
                /// <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// If the sizes of the two types are different, the cast combines or splits values, which leads to a change in length.
                /// </para>
                /// <para>
                /// For example, if <typeparamref name="TFrom"/> is <see cref="System.Int64"/>, the ReadOnlySpan&lt;Int64&gt; contains 
                /// a single value, 0x0100001111110F0F, and <typeparamref name="TTo"/> is <see cref="System.Int32"/>, the resulting 
                /// ReadOnlySpan&lt;Int32&gt; contains two values. The values are 0x11110F0F and 0x01000011 on a little-endian 
                /// architecture, such as x86. On a big-endian architecture, the order of the two values is reversed, i.e. 0x01000011, 
                /// followed by 0x11110F0F.
                /// </para>
                /// <para>
                /// As another example, if <typeparamref name="TFrom"/> is <see cref="System.Int32"/>, the ReadOnlySpan&lt;Int32&gt; 
                /// contains the values of 1, 2 and 3, and <typeparamref name="TTo"/> is <see cref="System.Int64"/>, the resulting 
                /// ReadOnlySpan&lt;Int64&gt; contains a single value: 0x0000000200000001 on a little-endian architecture 
                /// and 0x0000000100000002 on a big-endian architecture.
                /// </para>
                /// <para>
                /// This method is supported only on platforms that support misaligned memory 
                /// access or when the memory block is aligned by other means.
                /// </para>
                /// </remarks>
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

                /// <summary>Casts a read-only span of one primitive type to a read-only span of another primitive type. </summary>
                /// <typeparam name="TFrom">The type of the source span.</typeparam>
                /// <typeparam name="TTo">The type of the target span.</typeparam>
                /// <param name="span">The source slice to convert.</param>
                /// <returns>The converted read-only span.</returns>
                /// <remarks>
                /// <para>
                /// Neither <typeparamref name="TFrom"/> nor <typeparamref name="TTo"/> can contain managed object references. 
                /// The <see cref="Cast{TFrom, TTo}(Span{TFrom})"/> method performs this check at runtime and throws 
                /// <see cref="ArgumentException"/> if the check fails.
                /// </para>
                /// <para>
                /// This method is supported only on platforms that support misaligned memory 
                /// access or when the memory block is aligned by other means.
                /// </para>
                /// </remarks>
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

            /// <summary>
            /// Provides a collection of methods for interoperating with <see cref="ReadOnlySequence{T}"/>.
            /// </summary>
            public static class SequenceMarshal
            {

                /// <summary>
                /// Attempts to retrieve a read-only sequence segment from the specified read-only sequence.
                /// </summary>
                /// <typeparam name="T">The type of the read-only sequence.</typeparam>
                /// <param name="sequence">The read-only sequence from which the read-only sequence segment will be retrieved.</param>
                /// <param name="startSegment">The beginning read-only sequence segment.</param>
                /// <param name="startIndex">The initial position.</param>
                /// <param name="endSegment">The ending read-only sequence segment.</param>
                /// <param name="endIndex">The final position.</param>
                /// <returns><c>true</c> if the read-only sequence segment can be retrieved; otherwise, <c>false</c>.</returns>
                public static bool TryGetReadOnlySequenceSegment<T>(ReadOnlySequence<T> sequence,
                    out ReadOnlySequenceSegment<T> startSegment, out int startIndex,
                    out ReadOnlySequenceSegment<T> endSegment, out int endIndex)
                {
                    return sequence.TryGetReadOnlySequenceSegment(out startSegment, out startIndex, out endSegment, out endIndex);
                }

                /// <summary>
                /// Gets an array segment from the underlying read-only sequence.
                /// </summary>
                /// <typeparam name="T">The type of the read-only sequence.</typeparam>
                /// <param name="sequence">The read-only sequence from which the array segment will be retrieved.</param>
                /// <param name="segment">The returned array segment.</param>
                /// <returns><c>true</c> if it's possible to retrieve the array segment; 
                /// otherwise, <c>false</c> and a default array segment is returned.</returns>
                public static bool TryGetArray<T>(ReadOnlySequence<T> sequence, out ArraySegment<T> segment)
                {
                    return sequence.TryGetArray(out segment);
                }

                /// <summary>
                /// Attempts to retrieve a read-only memory from the specified read-only sequence.
                /// </summary>
                /// <typeparam name="T">The type of the read-only sequence.</typeparam>
                /// <param name="sequence">The read-only sequence from which the memory will be retrieved.</param>
                /// <param name="memory">The returned read-only memory of type <typeparamref name="T"/> .</param>
                /// <returns><c>true</c> if the read-only memory can be retrieved; otherwise, <c>false</c>.</returns>
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

            /// <summary>
            /// An attribute used to indicate a GC transition should be skipped when making an unmanaged function call.
            /// </summary>
            /// <example>
            /// Example of a valid use case. The Win32 `GetTickCount()` function is a small performance related function
            /// that reads some global memory and returns the value. In this case, the GC transition overhead is significantly
            /// more than the memory read.
            /// <code>
            /// using System;
            /// using System.Runtime.InteropServices;
            /// class Program
            /// {
            ///     [DllImport("Kernel32")]
            ///     [SuppressGCTransition]
            ///     static extern int GetTickCount();
            ///     static void Main()
            ///     {
            ///         Console.WriteLine($"{GetTickCount()}");
            ///     }
            /// }
            /// </code>
            /// </example>
            /// <remarks>
            /// This attribute is ignored if applied to a method without the <see cref="DllImportAttribute" />.
            ///
            /// Forgoing this transition can yield benefits when the cost of the transition is more than the execution time
            /// of the unmanaged function. However, avoiding this transition removes some of the guarantees the runtime
            /// provides through a normal P/Invoke. When exiting the managed runtime to enter an unmanaged function the
            /// GC must transition from Cooperative mode into Preemptive mode. Full details on these modes can be found at
            /// https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/clr-code-guide.md#2.1.8.
            /// Suppressing the GC transition is an advanced scenario and should not be done without fully understanding
            /// potential consequences.
            ///
            /// One of these consequences is an impact to Mixed-mode debugging (https://docs.microsoft.com/visualstudio/debugger/how-to-debug-in-mixed-mode).
            /// During Mixed-mode debugging, it is not possible to step into or set breakpoints in a P/Invoke that
            /// has been marked with this attribute. A workaround is to switch to native debugging and set a breakpoint in the native function.
            /// In general, usage of this attribute is not recommended if debugging the P/Invoke is important, for example
            /// stepping through the native code or diagnosing an exception thrown from the native code.
            ///
            /// The runtime may load the native library for method marked with this attribute in advance before the method is called for the first time.
            /// Usage of this attribute is not recommended for platform neutral libraries with conditional platform specific code.
            ///
            /// The P/Invoke method that this attribute is applied to must have all of the following properties:
            ///   * Native function always executes for a trivial amount of time (less than 1 microsecond).
            ///   * Native function does not perform a blocking syscall (e.g. any type of I/O).
            ///   * Native function does not call back into the runtime (e.g. Reverse P/Invoke).
            ///   * Native function does not throw exceptions.
            ///   * Native function does not manipulate locks or other concurrency primitives.
            ///
            /// Consequences of invalid uses of this attribute:
            ///   * GC starvation.
            ///   * Immediate runtime termination.
            ///   * Data corruption.
            /// </remarks>
            [AttributeUsage(AttributeTargets.Method, Inherited = false)]
            public sealed class SuppressGCTransitionAttribute : Attribute { }

            namespace Marshalling
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
                public sealed class ContiguousCollectionMarshallerAttribute : Attribute { }

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
                    public CustomMarshallerAttribute(Type managedType, MarshalMode marshalMode, Type marshallerType) { ManagedType = managedType; MarshalMode = marshalMode; MarshallerType = marshallerType; }

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
                    public struct GenericPlaceholder { }
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
                    public NativeMarshallingAttribute(Type nativeType) { NativeType = nativeType; }

                    /// <summary>
                    /// Gets the marshaller type used to convert the attributed type from managed to native code. This type must be attributed with <see cref="CustomMarshallerAttribute" />.
                    /// </summary>
                    public Type NativeType { get; }
                }

            }
        }
    
        namespace Versioning
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
            internal sealed class NonVersionableAttribute : Attribute { public NonVersionableAttribute() { } }

            /// <summary>
            /// Indicates that an API is in preview.  <br />
            /// This attribute allows call sites to be flagged with a diagnostic that indicates that a preview feature is used. <br />
            /// Authors can use this attribute to ship preview features in their assemblies.
            /// </summary>
            [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
            public sealed class RequiresPreviewFeaturesAttribute : Attribute
            {
                /// <summary>
                /// Returns the optional message associated with this attribute instance.
                /// </summary>
                public string Message { get; }

                /// <summary>
                /// Returns the optional URL associated with this attribute instance.
                /// </summary>
                public string Url { get; set; }

                /// <summary>
                /// Initializes a new instance of the <see cref="RequiresPreviewFeaturesAttribute" /> class.
                /// </summary>
                public RequiresPreviewFeaturesAttribute() { }

                /// <summary>
                /// Initializes a new instance of the <see cref="RequiresPreviewFeaturesAttribute" /> class with the specified message.
                /// </summary>
                /// <param name="message">An optional message associated with this attribute instance.</param>
                public RequiresPreviewFeaturesAttribute(string message)
                {
                    Message = message;
                }
            }

            #nullable enable
            /// <summary>
            /// [INHERITEDFROMDOTNET7] An attribute that specifies the platform that the assembly ,
            /// method , class , structure or token can run to or not. This class is abstract; which means 
            /// that you must create another class that inherit from this one.
            /// </summary>
            public abstract class OSPlatformAttribute : System.Attribute
            {
                private protected OSPlatformAttribute(string platformName) { PlatformName = platformName; }

                /// <summary>
                /// The Platform name that the attributed member can run to.
                /// Do not use this property directly. Otherwise , this one method will throw up an exception.
                /// </summary>
                public string? PlatformName { get; private set; }

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
            public sealed class SupportedOSPlatformAttribute : System.Runtime.Versioning.OSPlatformAttribute
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
            public sealed class UnsupportedOSPlatformAttribute : System.Runtime.Versioning.OSPlatformAttribute
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
                /// Read-only <see cref="System.String"/> that represents the exception message. Cannot be set by the user.
                /// </summary>
                public string? Message { get; private set; }
            }
#nullable disable
        }
    }

    namespace Linq
    {
        using System.Collections.Generic;
        using System.Collections.Immutable;

        /// <summary>
        /// LINQ extension method overrides that offer greater efficiency for <see cref="System.Collections.Immutable.ImmutableArray{T}" /> than the standard LINQ methods.
        /// </summary>
#nullable enable
#pragma warning disable CS8600, CS8602, CS8603, CS8604
        public static class ImmutableArrayExtensions
        {
            /// <summary>Projects each element of a sequence into a new form.</summary>
            /// <param name="immutableArray">The immutable array to select items from.</param>
            /// <param name="selector">A transform function to apply to each element.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <typeparam name="TResult">The type of the result element.</typeparam>
            /// <returns>An <see cref="IEnumerable{T}" /> whose elements are the result of invoking the transform function on each element of source.</returns>
            public static IEnumerable<TResult> Select<T, TResult>(this ImmutableArray<T> immutableArray, Func<T, TResult> selector)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                return immutableArray.array.Select(selector);
            }

            /// <summary>Projects each element of a sequence to an <see cref="IEnumerable{T}" />, 
            /// flattens the resulting sequences into one sequence, and invokes a result selector function on each element therein.
            /// </summary>
            /// <param name="immutableArray">The immutable array.</param>
            /// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
            /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
            /// <typeparam name="TSource">The type of the elements of <paramref name="immutableArray" />.</typeparam>
            /// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector" />.</typeparam>
            /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
            /// <returns>An <see cref="IEnumerable{T}" /> whose elements are the result
            /// of invoking the one-to-many transform function <paramref name="collectionSelector" /> on each
            /// element of <paramref name="immutableArray" /> and then mapping each of those sequence elements and their 
            /// corresponding source element to a result element.</returns>
            public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this ImmutableArray<TSource> immutableArray, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                if (collectionSelector == null || resultSelector == null)
                {
                    return Enumerable.SelectMany(immutableArray, collectionSelector, resultSelector);
                }
                if (immutableArray.Length != 0)
                {
                    return immutableArray.SelectManyIterator(collectionSelector, resultSelector);
                }
                return Enumerable.Empty<TResult>();
            }

            /// <summary>Filters a sequence of values based on a predicate.</summary>
            /// <param name="immutableArray">The array to filter.</param>
            /// <param name="predicate">The condition to use for filtering the array content.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>Returns <see cref="IEnumerable{T}" /> that contains elements that meet the condition.</returns>
            public static IEnumerable<T> Where<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                return immutableArray.array.Where(predicate);
            }

            /// <summary>Gets a value indicating whether the array contains any elements.</summary>
            /// <param name="immutableArray">The array to check for elements.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns><see langword="true" /> if the array contains elements; otherwise, <see langword="false" />.</returns>
            public static bool Any<T>(this ImmutableArray<T> immutableArray) { return immutableArray.Length > 0; }

            /// <summary>Gets a value indicating whether the array contains any elements that match a specified condition.</summary>
            /// <param name="immutableArray">The array to check for elements.</param>
            /// <param name="predicate">The delegate that defines the condition to match to an element.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns><see langword="true" /> if an element matches the specified condition; otherwise, <see langword="false" />.</returns>
            public static bool Any<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                Requires.NotNull(predicate, "predicate");
                T[] array = immutableArray.array;
                foreach (T arg in array) { if (predicate(arg)) { return true; } }
                return false;
            }

            /// <summary>Gets a value indicating whether all elements in this array match a given condition.</summary>
            /// <param name="immutableArray">The array to check for matches.</param>
            /// <param name="predicate">The predicate.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>
            ///   <see langword="true" /> if every element of the source sequence passes the test in the specified predicate; otherwise, <see langword="false" />.</returns>
            public static bool All<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                Requires.NotNull(predicate, "predicate");
                T[] array = immutableArray.array;
                foreach (T arg in array) { if (!predicate(arg)) { return false; } }
                return true;
            }

            /// <summary>Determines whether two sequences are equal according to an equality comparer.</summary>
            /// <param name="immutableArray">The array to use for comparison.</param>
            /// <param name="items">The items to use for comparison.</param>
            /// <param name="comparer">The comparer to use to check for equality.</param>
            /// <typeparam name="TDerived">The type of element in the compared array.</typeparam>
            /// <typeparam name="TBase">The type of element contained by the collection.</typeparam>
            /// <returns>
            ///   <see langword="true" /> to indicate the sequences are equal; otherwise, <see langword="false" />.</returns>
            public static bool SequenceEqual<TDerived, TBase>(this ImmutableArray<TBase> immutableArray, IEnumerable<TDerived> items, IEqualityComparer<TBase>? comparer = null) where TDerived : TBase
            {
                Requires.NotNull(items, "items");
                if (comparer == null) { comparer = EqualityComparer<TBase>.Default; }
                int num = 0;
                int length = immutableArray.Length;
                foreach (TDerived item in items)
                {
                    if (num == length) { return false; }
                    if (!comparer.Equals(immutableArray[num], (TBase)(object)item)) { return false; }
                    num++;
                }
                return num == length;
            }

            /// <summary>Applies a function to a sequence of elements in a cumulative way.</summary>
            /// <param name="immutableArray">The collection to apply the function to.</param>
            /// <param name="func">A function to be invoked on each element, in a cumulative way.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The final value after the cumulative function has been applied to all elements.</returns>
            public static T? Aggregate<T>(this ImmutableArray<T> immutableArray, Func<T, T, T> func)
            {
                Requires.NotNull(func, "func");
                if (immutableArray.Length == 0) { return default(T); }
                T val = immutableArray[0];
                int i = 1;
                for (int length = immutableArray.Length; i < length; i++)
                {
                    val = func(val, immutableArray[i]);
                }
                return val;
            }

            /// <summary>Applies a function to a sequence of elements in a cumulative way.</summary>
            /// <param name="immutableArray">The collection to apply the function to.</param>
            /// <param name="seed">The initial accumulator value.</param>
            /// <param name="func">A function to be invoked on each element, in a cumulative way.</param>
            /// <typeparam name="TAccumulate">The type of the accumulated value.</typeparam>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The final accumulator value.</returns>
            public static TAccumulate Aggregate<TAccumulate, T>(this ImmutableArray<T> immutableArray, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            {
                Requires.NotNull(func, "func");
                TAccumulate val = seed;
                T[] array = immutableArray.array;
                foreach (T arg in array) { val = func(val, arg); }
                return val;
            }

            /// <summary>Applies a function to a sequence of elements in a cumulative way.</summary>
            /// <param name="immutableArray">The collection to apply the function to.</param>
            /// <param name="seed">The initial accumulator value.</param>
            /// <param name="func">A function to be invoked on each element, in a cumulative way.</param>
            /// <param name="resultSelector">A function to transform the final accumulator value into the result type.</param>
            /// <typeparam name="TAccumulate">The type of the accumulated value.</typeparam>
            /// <typeparam name="TResult">The type of result returned by the result selector.</typeparam>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The final accumulator value.</returns>
            public static TResult Aggregate<TAccumulate, TResult, T>(this ImmutableArray<T> immutableArray, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            {
                Requires.NotNull(resultSelector, "resultSelector");
                return resultSelector(immutableArray.Aggregate(seed, func));
            }

            /// <summary>Returns the element at a specified index in the array.</summary>
            /// <param name="immutableArray">The array to find an element in.</param>
            /// <param name="index">The index for the element to retrieve.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The item at the specified index.</returns>
            public static T ElementAt<T>(this ImmutableArray<T> immutableArray, int index) { return immutableArray[index]; }

            /// <summary>Returns the element at a specified index in a sequence or a default value if the index is out of range.</summary>
            /// <param name="immutableArray">The array to find an element in.</param>
            /// <param name="index">The index for the element to retrieve.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The item at the specified index, or the default value if the index is not found.</returns>
            public static T? ElementAtOrDefault<T>(this ImmutableArray<T> immutableArray, int index)
            {
                if (index < 0 || index >= immutableArray.Length) { return default(T); }
                return immutableArray[index];
            }

            /// <summary>Returns the first element in a sequence that satisfies a specified condition.</summary>
            /// <param name="immutableArray">The array to get an item from.</param>
            /// <param name="predicate">The delegate that defines the conditions of the element to search for.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">If the array is empty.</exception>
            /// <returns>The first item in the list if it meets the condition specified by <paramref name="predicate" />.</returns>
            public static T First<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                Requires.NotNull(predicate, "predicate");
                T[] array = immutableArray.array;
                foreach (T val in array) { if (predicate(val)) { return val; } }
                return Enumerable.Empty<T>().First();
            }

            /// <summary>Returns the first element in an array.</summary>
            /// <param name="immutableArray">The array to get an item from.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">If the array is empty.</exception>
            /// <returns>The first item in the array.</returns>
            public static T First<T>(this ImmutableArray<T> immutableArray)
            {
                if (immutableArray.Length <= 0) { return immutableArray.array.First(); }
                return immutableArray[0];
            }

            /// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
            /// <param name="immutableArray">The array to retrieve items from.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The first item in the list, if found; otherwise the default value for the item type.</returns>
            public static T? FirstOrDefault<T>(this ImmutableArray<T> immutableArray)
            {
                if (immutableArray.array.Length == 0) { return default(T); }
                return immutableArray.array[0];
            }

            /// <summary>Returns the first element of the sequence that satisfies a condition or a default value if no such element is found.</summary>
            /// <param name="immutableArray">The array to retrieve elements from.</param>
            /// <param name="predicate">The delegate that defines the conditions of the element to search for.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The first item in the list, if found; otherwise the default value for the item type.</returns>
            public static T? FirstOrDefault<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                Requires.NotNull(predicate, "predicate");
                T[] array = immutableArray.array;
                foreach (T val in array) { if (predicate(val)) { return val; } }
                return default(T);
            }

            /// <summary>Returns the last element of the array.</summary>
            /// <param name="immutableArray">The array to retrieve items from.</param>
            /// <typeparam name="T">The type of element contained by the array.</typeparam>
            /// <exception cref="InvalidOperationException">The collection is empty.</exception>
            /// <returns>The last element in the array.</returns>
            public static T Last<T>(this ImmutableArray<T> immutableArray)
            {
                if (immutableArray.Length <= 0) { return immutableArray.array.Last(); }
                return immutableArray[immutableArray.Length - 1];
            }

            /// <summary>Returns the last element of a sequence that satisfies a specified condition.</summary>
            /// <param name="immutableArray">The array to retrieve elements from.</param>
            /// <param name="predicate">The delegate that defines the conditions of the element to retrieve.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="InvalidOperationException">The collection is empty.</exception>
            /// <returns>The last element of the array that satisfies the <paramref name="predicate" /> condition.</returns>
            public static T Last<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                Requires.NotNull(predicate, "predicate");
                for (int num = immutableArray.Length - 1; num >= 0; num--)
                {
                    if (predicate(immutableArray[num])) { return immutableArray[num]; }
                }
                return Enumerable.Empty<T>().Last();
            }

            /// <summary>Returns the last element of a sequence, or a default value if the sequence contains no elements.</summary>
            /// <param name="immutableArray">The array to retrieve items from.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The last element of a sequence, or a default value if the sequence contains no elements.</returns>
            public static T? LastOrDefault<T>(this ImmutableArray<T> immutableArray)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                return immutableArray.array.LastOrDefault();
            }

            /// <summary>Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.</summary>
            /// <param name="immutableArray">The array to retrieve an element from.</param>
            /// <param name="predicate">The delegate that defines the conditions of the element to search for.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The last element of a sequence, or a default value if the sequence contains no elements.</returns>
            public static T? LastOrDefault<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                Requires.NotNull(predicate, "predicate");
                for (int num = immutableArray.Length - 1; num >= 0; num--)
                {
                    if (predicate(immutableArray[num])) { return immutableArray[num]; }
                }
                return default(T);
            }

            /// <summary>Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.</summary>
            /// <param name="immutableArray">The array to retrieve the element from.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The element in the sequence.</returns>
            public static T Single<T>(this ImmutableArray<T> immutableArray)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                return immutableArray.array.Single();
            }

            /// <summary>Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.</summary>
            /// <param name="immutableArray">The immutable array to return a single element from.</param>
            /// <param name="predicate">The function to test whether an element should be returned.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>Returns <see cref="Boolean" />.</returns>
            public static T Single<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                Requires.NotNull(predicate, "predicate");
                bool flag = true;
                T result = default(T);
                T[] array = immutableArray.array;
                foreach (T val in array)
                {
                    if (predicate(val))
                    {
                        if (!flag) { ImmutableArray.TwoElementArray.Single(); }
                        flag = false;
                        result = val;
                    }
                }
                if (flag) { Enumerable.Empty<T>().Single(); }
                return result;
            }

            /// <summary>Returns the only element of the array, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.</summary>
            /// <param name="immutableArray">The array.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="InvalidOperationException">
            ///   <paramref name="immutableArray" /> contains more than one element.</exception>
            /// <returns>The element in the array, or the default value if the array is empty.</returns>
            public static T? SingleOrDefault<T>(this ImmutableArray<T> immutableArray)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                return immutableArray.array.SingleOrDefault();
            }

            /// <summary>Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.</summary>
            /// <param name="immutableArray">The array to get the element from.</param>
            /// <param name="predicate">The condition the element must satisfy.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">More than one element satisfies the condition in <paramref name="predicate" />.</exception>
            /// <returns>The element if it satisfies the specified condition; otherwise the default element.</returns>
            public static T? SingleOrDefault<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                Requires.NotNull(predicate, "predicate");
                bool flag = true;
                T result = default(T);
                T[] array = immutableArray.array;
                foreach (T val in array)
                {
                    if (predicate(val))
                    {
                        if (!flag) { ImmutableArray.TwoElementArray.Single(); }
                        flag = false;
                        result = val;
                    }
                }
                return result;
            }

            /// <summary>Creates a dictionary based on the contents of this array.</summary>
            /// <param name="immutableArray">The array to create a dictionary from.</param>
            /// <param name="keySelector">The key selector.</param>
            /// <typeparam name="TKey">The type of the key.</typeparam>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The newly initialized dictionary.</returns>
            public static Dictionary<TKey, T> ToDictionary<TKey, T>(this ImmutableArray<T> immutableArray, Func<T, TKey> keySelector) where TKey : notnull
            {
                return immutableArray.ToDictionary(keySelector, EqualityComparer<TKey>.Default);
            }

            /// <summary>Creates a dictionary based on the contents of this array.</summary>
            /// <param name="immutableArray">The array to create a dictionary from.</param>
            /// <param name="keySelector">The key selector.</param>
            /// <param name="elementSelector">The element selector.</param>
            /// <typeparam name="TKey">The type of the key.</typeparam>
            /// <typeparam name="TElement">The type of the element.</typeparam>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The newly initialized dictionary.</returns>
            public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement, T>(this ImmutableArray<T> immutableArray, Func<T, TKey> keySelector, Func<T, TElement> elementSelector) where TKey : notnull
            {
                return immutableArray.ToDictionary(keySelector, elementSelector, EqualityComparer<TKey>.Default);
            }

            /// <summary>Creates a dictionary based on the contents of this array.</summary>
            /// <param name="immutableArray">The array to create a dictionary from.</param>
            /// <param name="keySelector">The key selector.</param>
            /// <param name="comparer">The comparer to initialize the dictionary with.</param>
            /// <typeparam name="TKey">The type of the key.</typeparam>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The newly initialized dictionary.</returns>
            public static Dictionary<TKey, T> ToDictionary<TKey, T>(this ImmutableArray<T> immutableArray, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer) where TKey : notnull
            {
                Requires.NotNull(keySelector, "keySelector");
                Dictionary<TKey, T> dictionary = new Dictionary<TKey, T>(immutableArray.Length, comparer);
                ImmutableArray<T>.Enumerator enumerator = immutableArray.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    dictionary.Add(keySelector(current), current);
                }
                return dictionary;
            }

            /// <summary>Creates a dictionary based on the contents of this array.</summary>
            /// <param name="immutableArray">The array to create a dictionary from.</param>
            /// <param name="keySelector">The key selector.</param>
            /// <param name="elementSelector">The element selector.</param>
            /// <param name="comparer">The comparer to initialize the dictionary with.</param>
            /// <typeparam name="TKey">The type of the key.</typeparam>
            /// <typeparam name="TElement">The type of the element.</typeparam>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The newly initialized dictionary.</returns>
            public static Dictionary<TKey, TElement> ToDictionary<TKey, TElement, T>(this ImmutableArray<T> immutableArray, Func<T, TKey> keySelector, Func<T, TElement> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull
            {
                Requires.NotNull(keySelector, "keySelector");
                Requires.NotNull(elementSelector, "elementSelector");
                Dictionary<TKey, TElement> dictionary = new Dictionary<TKey, TElement>(immutableArray.Length, comparer);
                T[] array = immutableArray.array;
                foreach (T arg in array)
                {
                    dictionary.Add(keySelector(arg), elementSelector(arg));
                }
                return dictionary;
            }

            /// <summary>Copies the contents of this array to a mutable array.</summary>
            /// <param name="immutableArray">The immutable array to copy into a mutable one.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The newly instantiated array.</returns>
            public static T[] ToArray<T>(this ImmutableArray<T> immutableArray)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                if (immutableArray.array.Length == 0) { return ImmutableArray<T>.Empty.array; }
                return (T[])immutableArray.array.Clone();
            }

            /// <summary>Returns the first element in the collection.</summary>
            /// <param name="builder">The builder to retrieve an item from.</param>
            /// <typeparam name="T">The type of items in the array.</typeparam>
            /// <exception cref="InvalidOperationException">If the array is empty.</exception>
            /// <returns>The first item in the list.</returns>
            public static T First<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any()) { throw new InvalidOperationException(); }
                return builder[0];
            }

            /// <summary>Returns the first element in the collection, or the default value if the collection is empty.</summary>
            /// <param name="builder">The builder to retrieve an element from.</param>
            /// <typeparam name="T">The type of item in the builder.</typeparam>
            /// <returns>The first item in the list, if found; otherwise the default value for the item type.</returns>
            public static T? FirstOrDefault<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any()) { return default(T); }
                return builder[0];
            }

            /// <summary>Returns the last element in the collection.</summary>
            /// <param name="builder">The builder to retrieve elements from.</param>
            /// <typeparam name="T">The type of item in the builder.</typeparam>
            /// <exception cref="InvalidOperationException">The collection is empty.</exception>
            /// <returns>The last element in the builder.</returns>
            public static T Last<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any()) { throw new InvalidOperationException(); }
                return builder[builder.Count - 1];
            }

            /// <summary>Returns the last element in the collection, or the default value if the collection is empty.</summary>
            /// <param name="builder">The builder to retrieve an element from.</param>
            /// <typeparam name="T">The type of item in the builder.</typeparam>
            /// <returns>The last element of a sequence, or a default value if the sequence contains no elements.</returns>
            public static T? LastOrDefault<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any()) { return default(T); }
                return builder[builder.Count - 1];
            }

            /// <summary>Returns a value indicating whether this collection contains any elements.</summary>
            /// <param name="builder">The builder to check for matches.</param>
            /// <typeparam name="T">The type of elements in the array.</typeparam>
            /// <returns>
            ///   <see langword="true" /> if the array builder contains any elements; otherwise, <see langword="false" />.</returns>
            public static bool Any<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                return builder.Count > 0;
            }

            private static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(this ImmutableArray<TSource> immutableArray, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            {
                TSource[] array = immutableArray.array;
                foreach (TSource item in array)
                {
                    foreach (TCollection item2 in collectionSelector(item))
                    {
                        yield return resultSelector(item, item2);
                    }
                }
            }
        }
#pragma warning restore CS8600, CS8602, CS8603, CS8604
#nullable disable
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

        [StructLayout(LayoutKind.Explicit)]
        internal struct Register
        {
            [FieldOffset(0)] internal byte byte_0; [FieldOffset(1)] internal byte byte_1;
            
            [FieldOffset(2)] internal byte byte_2; [FieldOffset(3)] internal byte byte_3;

            [FieldOffset(4)] internal byte byte_4; [FieldOffset(5)] internal byte byte_5;

            [FieldOffset(6)] internal byte byte_6; [FieldOffset(7)] internal byte byte_7;

            [FieldOffset(8)] internal byte byte_8; [FieldOffset(9)] internal byte byte_9;

            [FieldOffset(10)] internal byte byte_10; [FieldOffset(11)] internal byte byte_11;

            [FieldOffset(12)] internal byte byte_12; [FieldOffset(13)] internal byte byte_13;

            [FieldOffset(14)] internal byte byte_14; [FieldOffset(15)] internal byte byte_15;

            [FieldOffset(0)] internal sbyte sbyte_0; [FieldOffset(1)] internal sbyte sbyte_1;

            [FieldOffset(2)] internal sbyte sbyte_2; [FieldOffset(3)] internal sbyte sbyte_3;

            [FieldOffset(4)] internal sbyte sbyte_4; [FieldOffset(5)] internal sbyte sbyte_5;

            [FieldOffset(6)] internal sbyte sbyte_6; [FieldOffset(7)] internal sbyte sbyte_7;

            [FieldOffset(8)] internal sbyte sbyte_8; [FieldOffset(9)] internal sbyte sbyte_9;

            [FieldOffset(10)] internal sbyte sbyte_10; [FieldOffset(11)] internal sbyte sbyte_11;

            [FieldOffset(12)] internal sbyte sbyte_12; [FieldOffset(13)] internal sbyte sbyte_13;

            [FieldOffset(14)] internal sbyte sbyte_14; [FieldOffset(15)] internal sbyte sbyte_15;

            [FieldOffset(0)] internal ushort uint16_0; [FieldOffset(2)] internal ushort uint16_1;

            [FieldOffset(4)] internal ushort uint16_2; [FieldOffset(6)] internal ushort uint16_3;

            [FieldOffset(8)] internal ushort uint16_4; [FieldOffset(10)] internal ushort uint16_5;

            [FieldOffset(12)] internal ushort uint16_6; [FieldOffset(14)] internal ushort uint16_7;

            [FieldOffset(0)] internal short int16_0; [FieldOffset(2)] internal short int16_1;

            [FieldOffset(4)] internal short int16_2; [FieldOffset(6)] internal short int16_3;

            [FieldOffset(8)] internal short int16_4; [FieldOffset(10)] internal short int16_5;

            [FieldOffset(12)] internal short int16_6; [FieldOffset(14)] internal short int16_7;

            [FieldOffset(0)] internal uint uint32_0; [FieldOffset(4)] internal uint uint32_1;

            [FieldOffset(8)] internal uint uint32_2; [FieldOffset(12)] internal uint uint32_3;

            [FieldOffset(0)] internal int int32_0; [FieldOffset(4)] internal int int32_1;

            [FieldOffset(8)] internal int int32_2; [FieldOffset(12)] internal int int32_3;

            [FieldOffset(0)] internal ulong uint64_0; [FieldOffset(8)] internal ulong uint64_1;

            [FieldOffset(0)] internal long int64_0; [FieldOffset(8)] internal long int64_1;

            [FieldOffset(0)] internal float single_0; [FieldOffset(4)] internal float single_1;

            [FieldOffset(8)] internal float single_2; [FieldOffset(12)] internal float single_3;

            [FieldOffset(0)] internal double double_0; [FieldOffset(8)] internal double double_1;
        }

        internal class ConstantHelper
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static byte GetByteWithAllBitsSet() { byte result = 0; result = byte.MaxValue; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static sbyte GetSByteWithAllBitsSet() { sbyte result = 0; result = -1; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ushort GetUInt16WithAllBitsSet() { ushort result = 0; result = ushort.MaxValue; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static short GetInt16WithAllBitsSet() { short result = 0; result = -1; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetUInt32WithAllBitsSet() { uint result = 0u; result = uint.MaxValue; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetInt32WithAllBitsSet() { int result = 0; result = -1; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static ulong GetUInt64WithAllBitsSet() { ulong result = 0uL; result = ulong.MaxValue; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long GetInt64WithAllBitsSet() { long result = 0L; result = -1L; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static float GetSingleWithAllBitsSet() { float result = 0f; *(int*)(&result) = -1; return result; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public unsafe static double GetDoubleWithAllBitsSet() { double result = 0.0; *(long*)(&result) = -1L; return result; }
        }

        internal static class BitOperations
        {
            private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]  { 0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30, 8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31 };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int Log2(uint value) { return Log2SoftwareFallback(value | 1u); }

            private static int Log2SoftwareFallback(uint value)
            {
                value |= value >> 1;
                value |= value >> 2;
                value |= value >> 4;
                value |= value >> 8;
                value |= value >> 16;
                return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(Log2DeBruijn), (IntPtr)(value * 130329821 >> 27));
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

        /// <summary>
        /// Indicates that certain members on a specified <see cref="T:System.Type" /> are accessed dynamically,
        /// for example through <see cref="N:System.Reflection" />.
        /// </summary>
        /// <remarks>
        /// This allows tools to understand which members are being accessed during the execution
        /// of a program.
        ///
        /// This attribute is valid on members whose type is <see cref="T:System.Type" /> or <see cref="T:System.String" />.
        ///
        /// When this attribute is applied to a location of type <see cref="T:System.String" />, the assumption is
        /// that the string represents a fully qualified type name.
        ///
        /// When this attribute is applied to a class, interface, or struct, the members specified
        /// can be accessed dynamically on <see cref="T:System.Type" /> instances returned from calling
        /// <see cref="M:System.Object.GetType" /> on instances of that class, interface, or struct.
        ///
        /// If the attribute is applied to a method it's treated as a special case and it implies
        /// the attribute should be applied to the "this" parameter of the method. As such the attribute
        /// should only be used on instance methods of types assignable to System.Type (or string, but no methods
        /// will use it there).
        /// </remarks>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, Inherited = false)]
        public sealed class DynamicallyAccessedMembersAttribute : Attribute
        {
            /// <summary>
            /// Gets the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes" /> which specifies the type
            /// of members dynamically accessed.
            /// </summary>
            public DynamicallyAccessedMemberTypes MemberTypes { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembersAttribute" /> class
            /// with the specified member types.
            /// </summary>
            /// <param name="memberTypes">The types of members dynamically accessed.</param>
            public DynamicallyAccessedMembersAttribute(DynamicallyAccessedMemberTypes memberTypes)
            {
                MemberTypes = memberTypes;
            }
        }

        /// <summary>
        /// Specifies the types of members that are dynamically accessed.
        /// <br />
        /// This enumeration has a <see cref="T:System.FlagsAttribute" /> attribute that allows a
        /// bitwise combination of its member values.
        /// </summary>
        [Flags]
        public enum DynamicallyAccessedMemberTypes
        {
            /// <summary>
            /// Specifies no members.
            /// </summary>
            None = 0,
            /// <summary>
            /// Specifies the default, parameterless public constructor.
            /// </summary>
            PublicParameterlessConstructor = 1,
            /// <summary>
            /// Specifies all public constructors.
            /// </summary>
            PublicConstructors = 3,
            /// <summary>
            /// Specifies all non-public constructors.
            /// </summary>
            NonPublicConstructors = 4,
            /// <summary>
            /// Specifies all public methods.
            /// </summary>
            PublicMethods = 8,
            /// <summary>
            /// Specifies all non-public methods.
            /// </summary>
            NonPublicMethods = 0x10,
            /// <summary>
            /// Specifies all public fields.
            /// </summary>
            PublicFields = 0x20,
            /// <summary>
            /// Specifies all non-public fields.
            /// </summary>
            NonPublicFields = 0x40,
            /// <summary>
            /// Specifies all public nested types.
            /// </summary>
            PublicNestedTypes = 0x80,
            /// <summary>
            /// Specifies all non-public nested types.
            /// </summary>
            NonPublicNestedTypes = 0x100,
            /// <summary>
            /// Specifies all public properties.
            /// </summary>
            PublicProperties = 0x200,
            /// <summary>
            /// Specifies all non-public properties.
            /// </summary>
            NonPublicProperties = 0x400,
            /// <summary>
            /// Specifies all public events.
            /// </summary>
            PublicEvents = 0x800,
            /// <summary>
            /// Specifies all non-public events.
            /// </summary>
            NonPublicEvents = 0x1000,
            /// <summary>
            /// Specifies all interfaces implemented by the type.
            /// </summary>
            Interfaces = 0x2000,
            /// <summary>
            /// Specifies all members.
            /// </summary>
            All = -1
        }

        /// <summary>
        /// States a dependency that one member has on another.
        /// </summary>
        /// <remarks>
        /// This can be used to inform tooling of a dependency that is otherwise not evident purely from
        /// metadata and IL, for example a member relied on via reflection.
        /// </remarks>
        [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
        public sealed class DynamicDependencyAttribute : Attribute
        {
            /// <summary>
            /// Gets the signature of the member depended on.
            /// </summary>
            /// <remarks>
            /// Either <see cref="MemberSignature" /> must be a valid string or <see cref="MemberTypes" />
            /// must not equal <see cref="DynamicallyAccessedMemberTypes.None" />, but not both.
            /// </remarks>
            public string MemberSignature { get; }

            /// <summary>
            /// Gets the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes" /> which specifies the type
            /// of members depended on.
            /// </summary>
            /// <remarks>
            /// Either <see cref="MemberSignature" /> must be a valid string or <see cref="MemberTypes" />
            /// must not equal <see cref="DynamicallyAccessedMemberTypes.None" />, but not both.
            /// </remarks>
            public DynamicallyAccessedMemberTypes MemberTypes { get; }

            /// <summary>
            /// Gets the <see cref="System.Type" /> containing the specified member.
            /// </summary>
            /// <remarks>
            /// If neither <see cref="Type" /> nor <see cref="TypeName" /> are specified,
            /// the type of the consumer is assumed.
            /// </remarks>
            public Type Type { get; }

            /// <summary>
            /// Gets the full name of the type containing the specified member.
            /// </summary>
            /// <remarks>
            /// If neither <see cref="Type" /> nor <see cref="TypeName" /> are specified,
            /// the type of the consumer is assumed.
            /// </remarks>
            public string TypeName { get; }

            /// <summary>
            /// Gets the assembly name of the specified type.
            /// </summary>
            /// <remarks>
            /// <see cref="AssemblyName" /> is only valid when <see cref="TypeName" /> is specified.
            /// </remarks>
            public string AssemblyName { get; }

            /// <summary>
            /// Gets or sets the condition in which the dependency is applicable, e.g. "DEBUG".
            /// </summary>
            public string Condition { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute" /> class
            /// with the specified signature of a member on the same type as the consumer.
            /// </summary>
            /// <param name="memberSignature">The signature of the member depended on.</param>
            public DynamicDependencyAttribute(string memberSignature)
            {
                MemberSignature = memberSignature;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute" /> class
            /// with the specified signature of a member on a <see cref="T:System.Type" />.
            /// </summary>
            /// <param name="memberSignature">The signature of the member depended on.</param>
            /// <param name="type">The <see cref="T:System.Type" /> containing <paramref name="memberSignature" />.</param>
            public DynamicDependencyAttribute(string memberSignature, Type type)
            {
                MemberSignature = memberSignature;
                Type = type;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute" /> class
            /// with the specified signature of a member on a type in an assembly.
            /// </summary>
            /// <param name="memberSignature">The signature of the member depended on.</param>
            /// <param name="typeName">The full name of the type containing the specified member.</param>
            /// <param name="assemblyName">The assembly name of the type containing the specified member.</param>
            public DynamicDependencyAttribute(string memberSignature, string typeName, string assemblyName)
            {
                MemberSignature = memberSignature;
                TypeName = typeName;
                AssemblyName = assemblyName;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute" /> class
            /// with the specified types of members on a <see cref="T:System.Type" />.
            /// </summary>
            /// <param name="memberTypes">The types of members depended on.</param>
            /// <param name="type">The <see cref="T:System.Type" /> containing the specified members.</param>
            public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, Type type)
            {
                MemberTypes = memberTypes;
                Type = type;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.DynamicDependencyAttribute" /> class
            /// with the specified types of members on a type in an assembly.
            /// </summary>
            /// <param name="memberTypes">The types of members depended on.</param>
            /// <param name="typeName">The full name of the type containing the specified members.</param>
            /// <param name="assemblyName">The assembly name of the type containing the specified members.</param>
            public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, string typeName, string assemblyName)
            {
                MemberTypes = memberTypes;
                TypeName = typeName;
                AssemblyName = assemblyName;
            }
        }

        /// <summary>
        /// Indicates that the specified method requires the ability to generate new code at runtime,
        /// for example through <see cref="Reflection" />.
        /// </summary>
        /// <remarks>
        /// This allows tools to understand which methods are unsafe to call when compiling ahead of time.
        /// </remarks>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method, Inherited = false)]
        public sealed class RequiresDynamicCodeAttribute : Attribute
        {
            /// <summary>
            /// Gets a message that contains information about the usage of dynamic code.
            /// </summary>
            public string Message { get; }

            /// <summary>
            /// Gets or sets an optional URL that contains more information about the method,
            /// why it requires dynamic code, and what options a consumer has to deal with it.
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.RequiresDynamicCodeAttribute" /> class
            /// with the specified message.
            /// </summary>
            /// <param name="message">
            /// A message that contains information about the usage of dynamic code.
            /// </param>
            public RequiresDynamicCodeAttribute(string message)
            {
                Message = message;
            }
        }

        /// <summary>Specifies the syntax used in a string.</summary>
        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
        public sealed class StringSyntaxAttribute : Attribute
        {
            /// <summary>The syntax identifier for strings containing composite formats for string formatting.</summary>
            public const string CompositeFormat = "CompositeFormat";

            /// <summary>The syntax identifier for strings containing date format specifiers.</summary>
            public const string DateOnlyFormat = "DateOnlyFormat";

            /// <summary>The syntax identifier for strings containing date and time format specifiers.</summary>
            public const string DateTimeFormat = "DateTimeFormat";

            /// <summary>The syntax identifier for strings containing <see cref="T:System.Enum" /> format specifiers.</summary>
            public const string EnumFormat = "EnumFormat";

            /// <summary>The syntax identifier for strings containing <see cref="T:System.Guid" /> format specifiers.</summary>
            public const string GuidFormat = "GuidFormat";

            /// <summary>The syntax identifier for strings containing JavaScript Object Notation (JSON).</summary>
            public const string Json = "Json";

            /// <summary>The syntax identifier for strings containing numeric format specifiers.</summary>
            public const string NumericFormat = "NumericFormat";

            /// <summary>The syntax identifier for strings containing regular expressions.</summary>
            public const string Regex = "Regex";

            /// <summary>The syntax identifier for strings containing time format specifiers.</summary>
            public const string TimeOnlyFormat = "TimeOnlyFormat";

            /// <summary>The syntax identifier for strings containing <see cref="T:System.TimeSpan" /> format specifiers.</summary>
            public const string TimeSpanFormat = "TimeSpanFormat";

            /// <summary>The syntax identifier for strings containing URIs.</summary>
            public const string Uri = "Uri";

            /// <summary>The syntax identifier for strings containing XML.</summary>
            public const string Xml = "Xml";

            /// <summary>Gets the identifier of the syntax used.</summary>
            public string Syntax { get; }

            /// <summary>Optional arguments associated with the specific syntax employed.</summary>
            public object[] Arguments { get; }

            /// <summary>Initializes the <see cref="T:System.Diagnostics.CodeAnalysis.StringSyntaxAttribute" /> with the identifier of the syntax used.</summary>
            /// <param name="syntax">The syntax identifier.</param>
            public StringSyntaxAttribute(string syntax)
            {
                Syntax = syntax;
                Arguments = Array.Empty<object>();
            }

            /// <summary>Initializes the <see cref="T:System.Diagnostics.CodeAnalysis.StringSyntaxAttribute" /> with the identifier of the syntax used.</summary>
            /// <param name="syntax">The syntax identifier.</param>
            /// <param name="arguments">Optional arguments associated with the specific syntax employed.</param>
            public StringSyntaxAttribute(string syntax, params object[] arguments)
            {
                Syntax = syntax;
                Arguments = arguments;
            }
        }

    }

    namespace IO
    {
        using System.Text;
        using System.Collections;
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;

        /// <summary>Contains internal volume helpers that are shared between many projects.</summary>
        internal static class DriveInfoInternal
        {
            public static string[] GetLogicalDrives()
            {
                int logicalDrives = global::Interop.Kernel32.GetLogicalDrives();
                if (logicalDrives == 0)
                {
                    throw Win32Marshal.GetExceptionForLastWin32Error();
                }
                uint num = (uint)logicalDrives;
                int num2 = 0;
                while (num != 0)
                {
                    if ((num & (true ? 1u : 0u)) != 0) { num2++; }
                    num >>= 1;
                }
                string[] array = new string[num2];
                System.Span<char> span = stackalloc char[3] { 'A', ':', '\\' };
                num = (uint)logicalDrives;
                num2 = 0;
                while (num != 0)
                {
                    if ((num & (true ? 1u : 0u)) != 0) { array[num2++] = span.ToString(); }
                    num >>= 1;
                    span[0] += '\u0001';
                }
                return array;
            }

            public static string NormalizeDriveName(string driveName)
            {
                string text;
                if (driveName.Length == 1)
                {
                    text = driveName + ":\\";
                }
                else
                {
                    text = Path.GetPathRoot(driveName);
                    if (string.IsNullOrEmpty(text) || text.StartsWith("\\\\", StringComparison.Ordinal))
                    {
                        throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeDriveLetterOrRootDir, "driveName");
                    }
                }
                if (text.Length == 2 && text[1] == ':')
                {
                    text += "\\";
                }
                char c = driveName[0];
                if ((c < 'A' || c > 'Z') && (c < 'a' || c > 'z'))
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeDriveLetterOrRootDir, "driveName");
                }
                return text;
            }
        }

        internal abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
        {
            private readonly int _threadId;

            internal int state;

            internal TSource current;

            public TSource Current => current;

            object IEnumerator.Current => Current;

            public Iterator()
            {
                _threadId = Environment.CurrentManagedThreadId;
            }

            protected abstract System.IO.Iterator<TSource> Clone();

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                current = default(TSource);
                state = -1;
            }

            public IEnumerator<TSource> GetEnumerator()
            {
                if (state == 0 && _threadId == Environment.CurrentManagedThreadId)
                {
                    state = 1;
                    return this;
                }
                System.IO.Iterator<TSource> iterator = Clone();
                iterator.state = 1;
                return iterator;
            }

            public abstract bool MoveNext();

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Wrapper to help with path normalization.
        /// </summary>
        internal static class PathHelper
        {
            /// <summary>
            /// Normalize the given path.
            /// </summary>
            /// <remarks>
            /// Normalizes via Win32 GetFullPathName().
            /// </remarks>
            /// <param name="path">Path to normalize</param>
            /// <exception cref="T:System.IO.PathTooLongException">Thrown if we have a string that is too large to fit into a UNICODE_STRING.</exception>
            /// <exception cref="T:System.IO.IOException">Thrown if the path is empty.</exception>
            /// <returns>Normalized path</returns>
            internal static string Normalize(string path)
            {
                System.Span<char> initialBuffer = stackalloc char[260];
                ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
                GetFullPathName(MemoryExtensions.AsSpan(path), ref builder);
                string result = ((MemoryExtensions.IndexOf<char>(builder.AsSpan(), '~') >= 0) ? TryExpandShortFileName(ref builder, path) : (MemoryExtensions.Equals(builder.AsSpan(), MemoryExtensions.AsSpan(path), StringComparison.Ordinal) ? path : builder.ToString()));
                builder.Dispose();
                return result;
            }

            /// <summary>
            /// Normalize the given path.
            /// </summary>
            /// <remarks>
            /// Exceptions are the same as the string overload.
            /// </remarks>
            internal static string Normalize(ref ValueStringBuilder path)
            {
                System.Span<char> initialBuffer = stackalloc char[260];
                ValueStringBuilder builder = new ValueStringBuilder(initialBuffer);
                GetFullPathName(path.AsSpan(terminate: true), ref builder);
                string result = ((MemoryExtensions.IndexOf<char>(builder.AsSpan(), '~') >= 0) ? TryExpandShortFileName(ref builder, null) : builder.ToString());
                builder.Dispose();
                return result;
            }

            /// <summary>
            /// Calls GetFullPathName on the given path.
            /// </summary>
            /// <param name="path">The path name. MUST be null terminated after the span.</param>
            /// <param name="builder">Builder that will store the result.</param>
            private static void GetFullPathName(System.ReadOnlySpan<char> path, ref ValueStringBuilder builder)
            {
                uint fullPathNameW;
                while ((fullPathNameW = global::Interop.Kernel32.GetFullPathNameW(ref MemoryMarshal.GetReference<char>(path), (uint)builder.Capacity, ref builder.GetPinnableReference(), IntPtr.Zero)) > builder.Capacity)
                {
                    builder.EnsureCapacity(checked((int)fullPathNameW));
                }
                if (fullPathNameW == 0)
                {
                    int num = Marshal.GetLastWin32Error();
                    if (num == 0)
                    {
                        num = 161;
                    }
                    throw Win32Marshal.GetExceptionForWin32Error(num, path.ToString());
                }
                builder.Length = (int)fullPathNameW;
            }

            internal static int PrependDevicePathChars(ref ValueStringBuilder content, bool isDosUnc, ref ValueStringBuilder buffer)
            {
                int length = content.Length;
                length += (isDosUnc ? 6 : 4);
                buffer.EnsureCapacity(length + 1);
                buffer.Length = 0;
                if (isDosUnc)
                {
                    buffer.Append("\\\\?\\UNC\\");
                    buffer.Append(content.AsSpan(2));
                    return 6;
                }
                buffer.Append("\\\\?\\");
                buffer.Append(content.AsSpan());
                return 4;
            }

            internal static string TryExpandShortFileName(ref ValueStringBuilder outputBuilder, string originalPath)
            {
                int rootLength = System.IO.PathInternal.GetRootLength(outputBuilder.AsSpan());
                bool flag = System.IO.PathInternal.IsDevice(outputBuilder.AsSpan());
                ValueStringBuilder buffer = default(ValueStringBuilder);
                bool flag2 = false;
                int num = 0;
                bool flag3 = false;
                if (flag)
                {
                    buffer.Append(outputBuilder.AsSpan());
                    if (outputBuilder[2] == '.')
                    {
                        flag3 = true;
                        buffer[2] = '?';
                    }
                }
                else
                {
                    flag2 = !System.IO.PathInternal.IsDevice(outputBuilder.AsSpan()) && outputBuilder.Length > 1 && outputBuilder[0] == '\\' && outputBuilder[1] == '\\';
                    num = PrependDevicePathChars(ref outputBuilder, flag2, ref buffer);
                }
                rootLength += num;
                int length = buffer.Length;
                bool flag4 = false;
                int num2 = buffer.Length - 1;
                while (!flag4)
                {
                    uint longPathNameW = global::Interop.Kernel32.GetLongPathNameW(ref buffer.GetPinnableReference(terminate: true), ref outputBuilder.GetPinnableReference(), (uint)outputBuilder.Capacity);
                    if (buffer[num2] == '\0')
                    {
                        buffer[num2] = '\\';
                    }
                    if (longPathNameW == 0)
                    {
                        int lastWin32Error = Marshal.GetLastWin32Error();
                        if (lastWin32Error != 2 && lastWin32Error != 3)
                        {
                            break;
                        }
                        num2--;
                        while (num2 > rootLength && buffer[num2] != '\\')
                        {
                            num2--;
                        }
                        if (num2 == rootLength)
                        {
                            break;
                        }
                        buffer[num2] = '\0';
                    }
                    else if (longPathNameW > outputBuilder.Capacity)
                    {
                        outputBuilder.EnsureCapacity(checked((int)longPathNameW));
                    }
                    else
                    {
                        flag4 = true;
                        outputBuilder.Length = checked((int)longPathNameW);
                        if (num2 < length - 1)
                        {
                            outputBuilder.Append(buffer.AsSpan(num2, buffer.Length - num2));
                        }
                    }
                }
                ref ValueStringBuilder reference = ref flag4 ? ref outputBuilder : ref buffer;
                if (flag3)
                {
                    reference[2] = '.';
                }
                if (flag2)
                {
                    reference[6] = '\\';
                }
                System.ReadOnlySpan<char> readOnlySpan = reference.AsSpan(num);
                string result = ((originalPath != null && MemoryExtensions.Equals(readOnlySpan, MemoryExtensions.AsSpan(originalPath), StringComparison.Ordinal)) ? originalPath : readOnlySpan.ToString());
                buffer.Dispose();
                return result;
            }
        }

        /// <summary>Contains internal path helpers that are shared between many projects.</summary>
        internal static class PathInternal
        {
            internal const char DirectorySeparatorChar = '\\';

            internal const char AltDirectorySeparatorChar = '/';

            internal const char VolumeSeparatorChar = ':';

            internal const char PathSeparator = ';';

            internal const string DirectorySeparatorCharAsString = "\\";

            internal const string NTPathPrefix = "\\??\\";

            internal const string ExtendedPathPrefix = "\\\\?\\";

            internal const string UncPathPrefix = "\\\\";

            internal const string UncExtendedPrefixToInsert = "?\\UNC\\";

            internal const string UncExtendedPathPrefix = "\\\\?\\UNC\\";

            internal const string UncNTPathPrefix = "\\??\\UNC\\";

            internal const string DevicePathPrefix = "\\\\.\\";

            internal const string ParentDirectoryPrefix = "..\\";

            internal const int MaxShortPath = 260;

            internal const int MaxShortDirectoryPath = 248;

            internal const int DevicePrefixLength = 4;

            internal const int UncPrefixLength = 2;

            internal const int UncExtendedPrefixLength = 8;

            /// <summary>Returns a comparison that can be used to compare file and directory names for equality.</summary>
            internal static StringComparison StringComparison
            {
                get
                {
                    if (!IsCaseSensitive)
                    {
                        return StringComparison.OrdinalIgnoreCase;
                    }
                    return StringComparison.Ordinal;
                }
            }

            /// <summary>Gets whether the system is case-sensitive.</summary>
            internal static bool IsCaseSensitive => false;

            /// <summary>
            /// Returns true if the path starts in a directory separator.
            /// </summary>
            internal unsafe static bool StartsWithDirectorySeparator(System.ReadOnlySpan<char> path)
            {
                if (path.Length > 0)
                {
                    return IsDirectorySeparator(path[0]);
                }
                return false;
            }

            internal static string EnsureTrailingSeparator(string path)
            {
                if (!EndsInDirectorySeparator(path)) { return path + "\\"; }
                return path;
            }

            internal static bool IsRoot(System.ReadOnlySpan<char> path) { return path.Length == GetRootLength(path); }

            /// <summary>
            /// Get the common path length from the start of the string.
            /// </summary>
            internal static int GetCommonPathLength(string first, string second, bool ignoreCase)
            {
                int num = EqualStartingCharacterCount(first, second, ignoreCase);
                if (num == 0)
                {
                    return num;
                }
                if (num == first.Length && (num == second.Length || IsDirectorySeparator(second[num])))
                {
                    return num;
                }
                if (num == second.Length && IsDirectorySeparator(first[num]))
                {
                    return num;
                }
                while (num > 0 && !IsDirectorySeparator(first[num - 1]))
                {
                    num--;
                }
                return num;
            }

            /// <summary>
            /// Gets the count of common characters from the left optionally ignoring case
            /// </summary>
            internal unsafe static int EqualStartingCharacterCount(string first, string second, bool ignoreCase)
            {
                if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second)) { return 0; }
                int num = 0;
                fixed (char* ptr = first)
                {
                    fixed (char* ptr3 = second)
                    {
                        char* ptr2 = ptr;
                        char* ptr4 = ptr3;
                        char* ptr5 = ptr2 + first.Length;
                        char* ptr6 = ptr4 + second.Length;
                        while (ptr2 != ptr5 && ptr4 != ptr6 && (*ptr2 == *ptr4 || (ignoreCase && char.ToUpperInvariant(*ptr2) == char.ToUpperInvariant(*ptr4))))
                        {
                            num++;
                            ptr2++;
                            ptr4++;
                        }
                    }
                }
                return num;
            }

            /// <summary>
            /// Returns true if the two paths have the same root
            /// </summary>
            internal static bool AreRootsEqual(string first, string second, StringComparison comparisonType)
            {
                int rootLength = GetRootLength(MemoryExtensions.AsSpan(first));
                int rootLength2 = GetRootLength(MemoryExtensions.AsSpan(second));
                if (rootLength == rootLength2)
                {
                    return string.Compare(first, 0, second, 0, rootLength, comparisonType) == 0;
                }
                return false;
            }

            /// <summary>
            /// Try to remove relative segments from the given path (without combining with a root).
            /// </summary>
            /// <param name="path">Input path</param>
            /// <param name="rootLength">The length of the root of the given path</param>
            internal static string RemoveRelativeSegments(string path, int rootLength)
            {
                System.Span<char> initialBuffer = stackalloc char[260];
                ValueStringBuilder sb = new ValueStringBuilder(initialBuffer);
                if (RemoveRelativeSegments(MemoryExtensions.AsSpan(path), rootLength, ref sb))
                {
                    path = sb.ToString();
                }
                sb.Dispose();
                return path;
            }

            /// <summary>
            /// Try to remove relative segments from the given path (without combining with a root).
            /// </summary>
            /// <param name="path">Input path</param>
            /// <param name="rootLength">The length of the root of the given path</param>
            /// <param name="sb">String builder that will store the result</param>
            /// <returns>"true" if the path was modified</returns>
            internal static bool RemoveRelativeSegments(System.ReadOnlySpan<char> path, int rootLength, ref ValueStringBuilder sb)
            {
                bool flag = false;
                int num = rootLength;
                if (IsDirectorySeparator(path[num - 1]))
                {
                    num--;
                }
                if (num > 0)
                {
                    sb.Append(path.Slice(0, num));
                }
                for (int i = num; i < path.Length; i++)
                {
                    char c = path[i];
                    if (IsDirectorySeparator(c) && i + 1 < path.Length)
                    {
                        if (IsDirectorySeparator(path[i + 1])) { continue; }
                        if ((i + 2 == path.Length || IsDirectorySeparator(path[i + 2])) && path[i + 1] == 46)
                        {
                            i++;
                            continue;
                        }
                        if (i + 2 < path.Length && (i + 3 == path.Length || IsDirectorySeparator(path[i + 3])) && path[i + 1] == 46 && path[i + 2] == 46)
                        {
                            int num2;
                            for (num2 = sb.Length - 1; num2 >= num; num2--)
                            {
                                if (IsDirectorySeparator(sb[num2]))
                                {
                                    sb.Length = ((i + 3 >= path.Length && num2 == num) ? (num2 + 1) : num2);
                                    break;
                                }
                            }
                            if (num2 < num)
                            {
                                sb.Length = num;
                            }
                            i += 2;
                            continue;
                        }
                    }
                    if (c != '\\' && c == '/')
                    {
                        c = '\\';
                        flag = true;
                    }
                    sb.Append(c);
                }
                if (!flag && sb.Length == path.Length)
                {
                    return false;
                }
                if (num != rootLength && sb.Length < rootLength)
                {
                    sb.Append(path[rootLength - 1]);
                }
                return true;
            }

            /// <summary>
            /// Trims one trailing directory separator beyond the root of the path.
            /// </summary>
            [return: NotNullIfNotNull(nameof(path))]
            internal static string TrimEndingDirectorySeparator(string path)
            {
                if (!EndsInDirectorySeparator(path) || IsRoot(MemoryExtensions.AsSpan(path)))
                {
                    return path;
                }
                return path.Substring(0, path.Length - 1);
            }

            /// <summary>
            /// Returns true if the path ends in a directory separator.
            /// </summary>
            internal static bool EndsInDirectorySeparator(string path)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    return IsDirectorySeparator(path[path.Length - 1]);
                }
                return false;
            }

            /// <summary>
            /// Trims one trailing directory separator beyond the root of the path.
            /// </summary>
            internal static System.ReadOnlySpan<char> TrimEndingDirectorySeparator(System.ReadOnlySpan<char> path)
            {
                if (!EndsInDirectorySeparator(path) || IsRoot(path)) { return path; }
                return path.Slice(0, path.Length - 1);
            }

            /// <summary>
            /// Returns true if the path ends in a directory separator.
            /// </summary>
            internal static bool EndsInDirectorySeparator(System.ReadOnlySpan<char> path)
            {
                if (path.Length > 0) { return IsDirectorySeparator(path[path.Length - 1]); }
                return false;
            }

            internal static string GetLinkTargetFullPath(string path, string pathToTarget)
            {
                if (!IsPartiallyQualified(MemoryExtensions.AsSpan(pathToTarget)))
                {
                    return pathToTarget;
                }
                return Path.Combine(Path.GetDirectoryName(path), pathToTarget);
            }

            /// <summary>
            /// Returns true if the given character is a valid drive letter
            /// </summary>
            internal static bool IsValidDriveChar(char value)
            {
                if (value < 'A' || value > 'Z')
                {
                    if (value >= 'a')
                    {
                        return value <= 'z';
                    }
                    return false;
                }
                return true;
            }

            internal static bool EndsWithPeriodOrSpace(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }
                char c = path[path.Length - 1];
                if (c != ' ')
                {
                    return c == '.';
                }
                return true;
            }

            /// <summary>
            /// Adds the extended path prefix (\\?\) if not already a device path, IF the path is not relative,
            /// AND the path is more than 259 characters. (&gt; MAX_PATH + null). This will also insert the extended
            /// prefix if the path ends with a period or a space. Trailing periods and spaces are normally eaten
            /// away from paths during normalization, but if we see such a path at this point it should be
            /// normalized and has retained the final characters. (Typically from one of the *Info classes)
            /// </summary>
            [return: NotNullIfNotNull(nameof(path))]
            internal static string EnsureExtendedPrefixIfNeeded(string path)
            {
                if (path != null && (path.Length >= 260 || EndsWithPeriodOrSpace(path)))
                {
                    return EnsureExtendedPrefix(path);
                }
                return path;
            }

            /// <summary>
            /// Adds the extended path prefix (\\?\) if not relative or already a device path.
            /// </summary>
            internal static string EnsureExtendedPrefix(string path)
            {
                if (IsPartiallyQualified(MemoryExtensions.AsSpan(path)) || IsDevice(MemoryExtensions.AsSpan(path)))
                {
                    return path;
                }
                if (path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
                {
                    return path.Insert(2, "?\\UNC\\");
                }
                return "\\\\?\\" + path;
            }

            /// <summary>
            /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
            /// </summary>
            internal static bool IsDevice(System.ReadOnlySpan<char> path)
            {
                if (!IsExtended(path))
                {
                    if (path.Length >= 4 && IsDirectorySeparator(path[0]) && IsDirectorySeparator(path[1]) && (path[2] == 46 || path[2] == 63))
                    {
                        return IsDirectorySeparator(path[3]);
                    }
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
            /// </summary>
            internal static bool IsDeviceUNC(System.ReadOnlySpan<char> path)
            {
                if (path.Length >= 8 && IsDevice(path) && IsDirectorySeparator(path[7]) && path[4] == 85 && path[5] == 78) { return path[6] == 67; }
                return false;
            }

            /// <summary>
            /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
            /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
            /// and path length checks.
            /// </summary>
            internal static bool IsExtended(System.ReadOnlySpan<char> path)
            {
                if (path.Length >= 4 && path[0] == 92 && (path[1] == 92 || path[1] == 63) && path[2] == 63) { return path[3] == 92; }
                return false;
            }

            /// <summary>
            /// Gets the length of the root of the path (drive, share, etc.).
            /// </summary>
            internal static int GetRootLength(System.ReadOnlySpan<char> path)
            {
                int length = path.Length;
                int i = 0;
                bool flag = IsDevice(path);
                bool flag2 = flag && IsDeviceUNC(path);
                if ((!flag || flag2) && length > 0 && IsDirectorySeparator(path[0]))
                {
                    if (flag2 || (length > 1 && IsDirectorySeparator(path[1])))
                    {
                        i = (flag2 ? 8 : 2);
                        int num = 2;
                        for (; i < length; i++) { if (IsDirectorySeparator(path[i]) && --num <= 0) { break; } }
                    }
                    else
                    {
                        i = 1;
                    }
                }
                else if (flag)
                {
                    for (i = 4; i < length && !IsDirectorySeparator(path[i]); i++)
                    {
                    }
                    if (i < length && i > 4 && IsDirectorySeparator(path[i]))
                    {
                        i++;
                    }
                }
                else if (length >= 2 && path[1] == 58 && IsValidDriveChar(path[0]))
                {
                    i = 2;
                    if (length > 2 && IsDirectorySeparator(path[2]))
                    {
                        i++;
                    }
                }
                return i;
            }

            /// <summary>
            /// Returns true if the path specified is relative to the current drive or working directory.
            /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
            /// validation of the path (URIs will be returned as relative as a result).
            /// </summary>
            /// <remarks>
            /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
            /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
            /// "C:a" is drive relative- meaning that it will be resolved against the current directory
            /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
            /// will not be used to modify the path).
            /// </remarks>
            internal unsafe static bool IsPartiallyQualified(System.ReadOnlySpan<char> path)
            {
                if (path.Length < 2)
                {
                    return true;
                }
                if (IsDirectorySeparator(path[0]))
                {
                    if (path[1] != 63)
                    {
                        return !IsDirectorySeparator(path[1]);
                    }
                    return false;
                }
                if (path.Length >= 3 && path[1] == 58 && IsDirectorySeparator(path[2]))
                {
                    return !IsValidDriveChar(path[0]);
                }
                return true;
            }

            /// <summary>
            /// True if the given character is a directory separator.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool IsDirectorySeparator(char c)
            {
                if (c != '\\') { return c == '/'; }
                return true;
            }

            /// <summary>
            /// Normalize separators in the given path. Converts forward slashes into back slashes and compresses slash runs, keeping initial 2 if present.
            /// Also trims initial whitespace in front of "rooted" paths (see PathStartSkip).
            ///
            /// This effectively replicates the behavior of the legacy NormalizePath when it was called with fullCheck=false and expandShortpaths=false.
            /// The current NormalizePath gets directory separator normalization from Win32's GetFullPathName(), which will resolve relative paths and as
            /// such can't be used here (and is overkill for our uses).
            ///
            /// Like the current NormalizePath this will not try and analyze periods/spaces within directory segments.
            /// </summary>
            /// <remarks>
            /// The only callers that used to use Path.Normalize(fullCheck=false) were Path.GetDirectoryName() and Path.GetPathRoot(). Both usages do
            /// not need trimming of trailing whitespace here.
            ///
            /// GetPathRoot() could technically skip normalizing separators after the second segment- consider as a future optimization.
            ///
            /// For legacy .NET Framework behavior with ExpandShortPaths:
            ///  - It has no impact on GetPathRoot() so doesn't need consideration.
            ///  - It could impact GetDirectoryName(), but only if the path isn't relative (C:\ or \\Server\Share).
            ///
            /// In the case of GetDirectoryName() the ExpandShortPaths behavior was undocumented and provided inconsistent results if the path was
            /// fixed/relative. For example: "C:\PROGRA~1\A.TXT" would return "C:\Program Files" while ".\PROGRA~1\A.TXT" would return ".\PROGRA~1". If you
            /// ultimately call GetFullPath() this doesn't matter, but if you don't or have any intermediate string handling could easily be tripped up by
            /// this undocumented behavior.
            ///
            /// We won't match this old behavior because:
            ///
            ///   1. It was undocumented
            ///   2. It was costly (extremely so if it actually contained '~')
            ///   3. Doesn't play nice with string logic
            ///   4. Isn't a cross-plat friendly concept/behavior
            /// </remarks>
            [return: NotNullIfNotNull(nameof(path))]
            internal static string NormalizeDirectorySeparators(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return path;
                }
                bool flag = true;
                for (int i = 0; i < path.Length; i++)
                {
                    char c = path[i];
                    if (IsDirectorySeparator(c) && (c != '\\' || (i > 0 && i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return path;
                }
                System.Span<char> initialBuffer = stackalloc char[260];
                ValueStringBuilder valueStringBuilder = new ValueStringBuilder(initialBuffer);
                int num = 0;
                if (IsDirectorySeparator(path[num]))
                {
                    num++;
                    valueStringBuilder.Append('\\');
                }
                for (int j = num; j < path.Length; j++)
                {
                    char c = path[j];
                    if (IsDirectorySeparator(c))
                    {
                        if (j + 1 < path.Length && IsDirectorySeparator(path[j + 1]))
                        {
                            continue;
                        }
                        c = '\\';
                    }
                    valueStringBuilder.Append(c);
                }
                return valueStringBuilder.ToString();
            }

            /// <summary>
            /// Returns true if the path is effectively empty for the current OS.
            /// For unix, this is empty or null. For Windows, this is empty, null, or
            /// just spaces ((char)32).
            /// </summary>
            internal static bool IsEffectivelyEmpty(System.ReadOnlySpan<char> path)
            {
                if (path.IsEmpty) { return true; }
                ReadOnlySpan<char> readOnlySpan = path;
                for (int i = 0; i < readOnlySpan.Length; i++)
                {
                    char c = readOnlySpan[i];
                    if (c != ' ')
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        internal sealed class ReadLinesIterator : System.IO.Iterator<string>
        {
            private readonly string _path;

            private readonly Encoding _encoding;

            private StreamReader _reader;

            private ReadLinesIterator(string path, Encoding encoding, StreamReader reader)
            {
                _path = path;
                _encoding = encoding;
                _reader = reader;
            }

            public override bool MoveNext()
            {
                if (_reader != null)
                {
                    current = _reader.ReadLine();
                    if (current != null)
                    {
                        return true;
                    }
                    Dispose();
                }
                return false;
            }

            protected override System.IO.Iterator<string> Clone()
            {
                return CreateIterator(_path, _encoding, _reader);
            }

            protected override void Dispose(bool disposing)
            {
                try
                {
                    if (disposing && _reader != null)
                    {
                        _reader.Dispose();
                    }
                }
                finally
                {
                    _reader = null;
                    base.Dispose(disposing);
                }
            }

            internal static System.IO.ReadLinesIterator CreateIterator(string path, Encoding encoding)
            {
                return CreateIterator(path, encoding, null);
            }

            private static System.IO.ReadLinesIterator CreateIterator(string path, Encoding encoding, StreamReader reader)
            {
                return new System.IO.ReadLinesIterator(path, encoding, reader ?? new StreamReader(path, encoding));
            }
        }

        /// <summary>
        /// Provides static methods for converting from Win32 errors codes to exceptions, HRESULTS and error messages.
        /// </summary>
        internal static class Win32Marshal
        {
            /// <summary>
            /// Converts, resetting it, the last Win32 error into a corresponding <see cref="T:System.Exception" /> object, optionally
            /// including the specified path in the error message.
            /// </summary>
            internal static Exception GetExceptionForLastWin32Error(string path = "")
            {
                return GetExceptionForWin32Error(Marshal.GetLastWin32Error(), path);
            }

            /// <summary>
            /// Converts the specified Win32 error into a corresponding <see cref="T:System.Exception" /> object, optionally
            /// including the specified path in the error message.
            /// </summary>
            internal static Exception GetExceptionForWin32Error(int errorCode, string path = "")
            {
                switch (errorCode)
                {
                    case 2:
                        return new FileNotFoundException(string.IsNullOrEmpty(path) ? MDCFR.Properties.Resources.IO_FileNotFound : System.SR.Format(MDCFR.Properties.Resources.IO_FileNotFound_FileName, path), path);
                    case 3:
                        return new DirectoryNotFoundException(string.IsNullOrEmpty(path) ? MDCFR.Properties.Resources.IO_PathNotFound_NoPathName : System.SR.Format(MDCFR.Properties.Resources.IO_PathNotFound_Path, path));
                    case 5:
                        return new UnauthorizedAccessException(string.IsNullOrEmpty(path) ? MDCFR.Properties.Resources.UnauthorizedAccess_IODenied_NoPathName : System.SR.Format(MDCFR.Properties.Resources.UnauthorizedAccess_IODenied_Path, path));
                    case 183:
                        if (!string.IsNullOrEmpty(path))
                        {
                            return new IOException(System.SR.Format(MDCFR.Properties.Resources.IO_AlreadyExists_Name, path), MakeHRFromErrorCode(errorCode));
                        }
                        break;
                    case 206:
                        return new PathTooLongException(string.IsNullOrEmpty(path) ? MDCFR.Properties.Resources.IO_PathTooLong : System.SR.Format(MDCFR.Properties.Resources.IO_PathTooLong_Path, path));
                    case 32:
                        return new IOException(string.IsNullOrEmpty(path) ? MDCFR.Properties.Resources.IO_SharingViolation_NoFileName : System.SR.Format(MDCFR.Properties.Resources.IO_SharingViolation_File, path), MakeHRFromErrorCode(errorCode));
                    case 80:
                        if (!string.IsNullOrEmpty(path))
                        {
                            return new IOException(System.SR.Format(MDCFR.Properties.Resources.IO_FileExists_Name, path), MakeHRFromErrorCode(errorCode));
                        }
                        break;
                    case 995:
                        return new OperationCanceledException();
                }
                return new IOException(string.IsNullOrEmpty(path) ? GetMessage(errorCode) : (GetMessage(errorCode) + " : '" + path + "'"), MakeHRFromErrorCode(errorCode));
            }

            /// <summary>
            /// If not already an HRESULT, returns an HRESULT for the specified Win32 error code.
            /// </summary>
            internal static int MakeHRFromErrorCode(int errorCode)
            {
                if ((0xFFFF0000u & errorCode) != 0L)
                {
                    return errorCode;
                }
                return -2147024896 | errorCode;
            }

            /// <summary>
            /// Returns a Win32 error code for the specified HRESULT if it came from FACILITY_WIN32
            /// If not, returns the HRESULT unchanged
            /// </summary>
            internal static int TryMakeWin32ErrorCodeFromHR(int hr)
            {
                if ((0xFFFF0000u & hr) == 2147942400u)
                {
                    hr &= 0xFFFF;
                }
                return hr;
            }

            /// <summary>
            /// Returns a string message for the specified Win32 error code.
            /// </summary>
            internal static string GetMessage(int errorCode)
            {
                return global::Interop.Kernel32.GetMessage(errorCode);
            }
        }

        internal static class TextWriterExtensions
        {
            public static void WritePartialString(this TextWriter writer, string value, int offset, int count)
            {
                if (offset == 0 && count == value.Length)
                {
                    writer.Write(value);
                    return;
                }
                ReadOnlySpan<char> readOnlySpan = value.AsSpan(offset, count);
                char[] array = System.Buffers.ArrayPool<char>.Shared.Rent(readOnlySpan.Length);
                readOnlySpan.CopyTo(array);
                writer.Write(array, 0, readOnlySpan.Length);
                System.Buffers.ArrayPool<char>.Shared.Return(array);
            }
        }

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

        /// <summary>Provides a <see cref="T:System.IO.Stream" /> for the contents of a <see cref="T:System.ReadOnlyMemory`1" />.</summary>
        internal sealed class ReadOnlyMemoryStream : Stream
        {
            private ReadOnlyMemory<byte> _content;

            private int _position;

            private bool _isOpen;

            public override bool CanRead => _isOpen;

            public override bool CanSeek => _isOpen;

            public override bool CanWrite => false;

            public override long Length
            {
                get
                {
                    EnsureNotClosed();
                    return _content.Length;
                }
            }

            public override long Position
            {
                get
                {
                    EnsureNotClosed();
                    return _position;
                }
                set
                {
                    EnsureNotClosed();
                    if (value < 0 || value > int.MaxValue)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    _position = (int)value;
                }
            }

            public ReadOnlyMemoryStream(ReadOnlyMemory<byte> content)
            {
                _content = content;
                _isOpen = true;
            }

            private void EnsureNotClosed()
            {
                if (!_isOpen)
                {
                    throw new ObjectDisposedException(null, MDCFR.Properties.Resources.ObjectDisposed_StreamClosed);
                }
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                EnsureNotClosed();
                long num = origin switch
                {
                    SeekOrigin.End => _content.Length + offset,
                    SeekOrigin.Current => _position + offset,
                    SeekOrigin.Begin => offset,
                    _ => throw new ArgumentOutOfRangeException("origin"),
                };
                if (num > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (num < 0)
                {
                    throw new IOException(MDCFR.Properties.Resources.IO_SeekBeforeBegin);
                }
                _position = (int)num;
                return _position;
            }

            public unsafe override int ReadByte()
            {
                EnsureNotClosed();
                System.ReadOnlySpan<byte> span = _content.Span;
                if (_position >= span.Length)
                {
                    return -1;
                }
                return span[_position++];
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                ValidateBufferArguments(buffer, offset, count);
                return ReadBuffer(new System.Span<byte>(buffer, offset, count));
            }

            private int ReadBuffer(System.Span<byte> buffer)
            {
                EnsureNotClosed();
                int num = _content.Length - _position;
                if (num <= 0 || buffer.Length == 0)
                {
                    return 0;
                }
                if (num <= buffer.Length)
                {
                    _content.Span.Slice(_position).CopyTo(buffer);
                    _position = _content.Length;
                    return num;
                }
                _content.Span.Slice(_position, buffer.Length).CopyTo(buffer);
                _position += buffer.Length;
                return buffer.Length;
            }

            public override Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            {
                ValidateBufferArguments(buffer, offset, count);
                EnsureNotClosed();
                if (!cancellationToken.IsCancellationRequested)
                {
                    return Threading.Tasks.Task.FromResult(ReadBuffer(new System.Span<byte>(buffer, offset, count)));
                }
                return Threading.Tasks.Task.FromCanceled<int>(cancellationToken);
            }

            public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            {
                return System.Threading.Tasks.TaskToApm.Begin(ReadAsync(buffer, offset, count), callback, state);
            }

            public override int EndRead(IAsyncResult asyncResult)
            {
                EnsureNotClosed();
                return Threading.Tasks.TaskToApm.End<int>(asyncResult);
            }

            public override void Flush()
            {
            }

            public override Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken)
            {
                return Threading.Tasks.Task.CompletedTask;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                //IL_000d: Unknown result type (might be due to invalid IL or missing references)
                _isOpen = false;
                _content = default(ReadOnlyMemory<byte>);
                base.Dispose(disposing);
            }

            private static void ValidateBufferArguments(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException("offset", MDCFR.Properties.Resources.ArgumentOutOfRange_NeedNonNegNum);
                }
                if ((uint)count > buffer.Length - offset)
                {
                    throw new ArgumentOutOfRangeException("count", MDCFR.Properties.Resources.Argument_InvalidOffLen);
                }
            }
        }
    }


    // Reference: https://github.com/dotnet/corefx/blob/48363ac826ccf66fbe31a5dcb1dc2aab9a7dd768/src/Common/src/CoreLib/System/Diagnostics/CodeAnalysis/NullableAttributes.cs

    // Licensed to the .NET Foundation under one or more agreements.
    // The .NET Foundation licenses this file to you under the MIT license.
    // See the LICENSE file in the project root for more information.

    namespace Diagnostics.CodeAnalysis
    {
            /// <summary>Specifies that null is allowed as an input even if the corresponding type disallows it.</summary>
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
            public sealed class AllowNullAttribute : Attribute { }

            /// <summary>Specifies that null is disallowed as an input even if the corresponding type allows it.</summary>
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
            public sealed class DisallowNullAttribute : Attribute { }

            /// <summary>Specifies that an output may be null even if the corresponding type disallows it.</summary>
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
            public sealed class MaybeNullAttribute : Attribute { }

            /// <summary>Specifies that an output will not be null even if the corresponding type allows it.</summary>
            [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
            public sealed class NotNullAttribute : Attribute { }

            /// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter may be null even if the corresponding type disallows it.</summary>
            [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
            public sealed class MaybeNullWhenAttribute : Attribute
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
            public sealed class NotNullWhenAttribute : Attribute
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
            public sealed class NotNullIfNotNullAttribute : Attribute
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
            public sealed class DoesNotReturnAttribute : Attribute { }

            /// <summary>Specifies that the method will not return if the associated Boolean parameter is passed the specified value.</summary>
            [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
            public sealed class DoesNotReturnIfAttribute : Attribute
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

    internal static class DecimalDecCalc
    {
        private static uint D32DivMod1E9(uint hi32, ref uint lo32) { ulong num = ((ulong)hi32 << 32) | lo32; lo32 = (uint)(num / 1000000000uL); return (uint)(num % 1000000000uL); }

        internal static uint DecDivMod1E9(ref MutableDecimal value) { return D32DivMod1E9(D32DivMod1E9(D32DivMod1E9(0u, ref value.High), ref value.Mid), ref value.Low); }

        internal static void DecAddInt32(ref MutableDecimal value, uint i) { if (D32AddCarry(ref value.Low, i) && D32AddCarry(ref value.Mid, 1u)) { D32AddCarry(ref value.High, 1u); } }

        private static bool D32AddCarry(ref uint value, uint i) { uint num = value; uint num2 = (value = num + i); if (num2 >= num) { return num2 < i; } return true; }

        internal static void DecMul10(ref MutableDecimal value) { MutableDecimal d = value; DecShiftLeft(ref value); DecShiftLeft(ref value); DecAdd(ref value, d); DecShiftLeft(ref value); }

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
            if (D32AddCarry(ref value.Low, d.Low) && D32AddCarry(ref value.Mid, 1u)) { D32AddCarry(ref value.High, 1u); }
            if (D32AddCarry(ref value.Mid, d.Mid)) { D32AddCarry(ref value.High, 1u); }
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

        private unsafe NUInt(uint value) { _value = (void*)value; }

        private unsafe NUInt(ulong value) { _value = (void*)value; }

        public static implicit operator NUInt(uint value) { return new NUInt(value); }

        public unsafe static implicit operator IntPtr(NUInt value) { return (IntPtr)value._value; }

        public static explicit operator NUInt(int value) { return new NUInt((uint)value); }

        public unsafe static explicit operator void*(NUInt value) { return value._value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static NUInt operator *(NUInt left, NUInt right)
        {
            if (sizeof(IntPtr) != 4) { return new NUInt((ulong)left._value * (ulong)right._value); }
            return new NUInt((uint)((int)left._value * (int)right._value));
        }
    }

    internal enum ExceptionArgument
    {
        length,
        start, minimumBufferSize,
        elementIndex, comparable,
        comparer, destination,
        offset, startSegment,
        endSegment, startIndex,
        endIndex, array,
        culture, manager
    }

    internal static class Number
    {
        private static class DoubleHelper
        {
            public unsafe static uint Exponent(double d) { return (*(uint*)((byte*)(&d) + 4) >> 20) & 0x7FFu; }

            public unsafe static ulong Mantissa(double d) { return *(uint*)(&d) | ((ulong)(uint)(*(int*)((byte*)(&d) + 4) & 0xFFFFF) << 32); }

            public unsafe static bool Sign(double d) { return *(uint*)((byte*)(&d) + 4) >> 31 != 0; }
        }

        internal const int DECIMAL_PRECISION = 29;

        private static readonly ulong[] s_rgval64Power10 = new ulong[30]
        {
            11529215046068469760uL, 14411518807585587200uL, 18014398509481984000uL, 11258999068426240000uL, 14073748835532800000uL, 17592186044416000000uL, 10995116277760000000uL, 13743895347200000000uL, 17179869184000000000uL, 10737418240000000000uL,
            13421772800000000000uL, 16777216000000000000uL, 10485760000000000000uL, 13107200000000000000uL, 16384000000000000000uL, 14757395258967641293uL, 11805916207174113035uL, 9444732965739290428uL, 15111572745182864686uL, 12089258196146291749uL,
            9671406556917033399uL, 15474250491067253438uL, 12379400392853802751uL, 9903520314283042201uL, 15845632502852867522uL, 12676506002282294018uL, 10141204801825835215uL, 16225927682921336344uL, 12980742146337069075uL, 10384593717069655260uL
        };

        private static readonly sbyte[] s_rgexp64Power10 = new sbyte[15] { 4, 7, 10, 14, 17, 20, 24, 27, 30, 34, 37, 40, 44, 47, 50 };

        private static readonly ulong[] s_rgval64Power10By16 = new ulong[42]
        {
        10240000000000000000uL, 11368683772161602974uL, 12621774483536188886uL, 14012984643248170708uL, 15557538194652854266uL, 17272337110188889248uL, 9588073174409622172uL, 10644899600020376798uL, 11818212630765741798uL, 13120851772591970216uL,
        14567071740625403792uL, 16172698447808779622uL, 17955302187076837696uL, 9967194951097567532uL, 11065809325636130658uL, 12285516299433008778uL, 13639663065038175358uL, 15143067982934716296uL, 16812182738118149112uL, 9332636185032188787uL,
        10361307573072618722uL, 16615349947311448416uL, 14965776766268445891uL, 13479973333575319909uL, 12141680576410806707uL, 10936253623915059637uL, 9850501549098619819uL, 17745086042373215136uL, 15983352577617880260uL, 14396524142538228461uL,
        12967236152753103031uL, 11679847981112819795uL, 10520271803096747049uL, 9475818434452569218uL, 17070116948172427008uL, 15375394465392026135uL, 13848924157002783096uL, 12474001934591998882uL, 11235582092889474480uL, 10120112665365530972uL,
        18230774251475056952uL, 16420821625123739930uL
        };

        private static readonly short[] s_rgexp64Power10By16 = new short[21] { 54, 107, 160, 213, 266, 319, 373, 426, 479, 532, 585, 638, 691, 745, 798, 851, 904, 957, 1010, 1064, 1117 };

        public static void RoundNumber(ref NumberBuffer number, int pos)
        {
            Span<byte> digits = number.Digits;
            int i;
            for (i = 0; i < pos && digits[i] != 0; i++) { }
            if (i == pos && digits[i] >= 53)
            {
                while (i > 0 && digits[i - 1] == 57) { i--; }
                if (i > 0) { digits[i - 1]++; }
                else { number.Scale++; digits[0] = 49; i = 1; }
            } else { while (i > 0 && digits[i - 1] == 48) { i--; } }
            if (i == 0) { number.Scale = 0; number.IsNegative = false; }
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
                    if (num3 == 0L) { num = 0.0; }
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
                if (num > 0) { num = 0; }
            }
            else
            {
                if (num > 29) { return false; }
                while ((num > 0 || (*ptr != 0 && num > -28)) && (source.High < 429496729 || (source.High == 429496729 && (source.Mid < 2576980377u || (source.Mid == 2576980377u && (source.Low < 2576980377u || (source.Low == 2576980377u && *ptr <= 53)))))))
                {
                    DecimalDecCalc.DecMul10(ref source);
                    if (*ptr != 0) { DecimalDecCalc.DecAddInt32(ref source, (uint)(*(ptr++) - 48)); }
                    num--;
                }
                if (*(ptr++) >= 53)
                {
                    bool flag = true;
                    if (*(ptr - 1) == 53 && (int)(*(ptr - 2)) % 2 == 0)
                    {
                        int num2 = 20;
                        while (*ptr == 48 && num2 != 0) { ptr++; num2--; }
                        if (*ptr == 0 || num2 == 0) { flag = false; }
                    }
                    if (flag)
                    {
                        DecimalDecCalc.DecAddInt32(ref source, 1u);
                        if ((source.High | source.Mid | source.Low) == 0) { source.High = 429496729u; source.Mid = 2576980377u; source.Low = 2576980378u; num++; }
                    }
                }
            }
            if (num > 0) { return false; }
            if (num <= -29) { source.High = 0u; source.Low = 0u; source.Mid = 0u; source.Scale = 28; } else { source.Scale = -num; }
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
                for (int i = 0; i < 9; i++) { digits[--num] = (byte)(num2 % 10u + 48); num2 /= 10u; }
            }
            for (uint num3 = reference.Low; num3 != 0; num3 /= 10u) { digits[--num] = (byte)(num3 % 10u + 48); }
            int num4 = 29 - num;
            number.Scale = num4 - reference.Scale;
            Span<byte> digits2 = number.Digits;
            int index = 0;
            while (--num4 >= 0) { digits2[index++] = digits[num++]; }
            digits2[index] = 0;
        }

        private static uint DigitsToInt(ReadOnlySpan<byte> digits, int count)
        {
            uint value; int bytesConsumed;
            bool flag = System.Buffers.Text.Utf8Parser.TryParse(digits.Slice(0, count), out value, out bytesConsumed, 'D');
            return value;
        }

        private static ulong Mul32x32To64(uint a, uint b) { return (ulong)a * (ulong)b; }

        private static ulong Mul64Lossy(ulong a, ulong b, ref int pexp)
        {
            ulong num = Mul32x32To64((uint)(a >> 32), (uint)(b >> 32)) + (Mul32x32To64((uint)(a >> 32), (uint)b) >> 32) + (Mul32x32To64((uint)a, (uint)(b >> 32)) >> 32);
            if ((num & 0x8000000000000000uL) == 0L) { num <<= 1; pexp--; }
            return num;
        }

        private static int abs(int value)
        {
            if (value < 0) { return -value; }
            return value;
        }

        private unsafe static double NumberToDouble(ref NumberBuffer number)
        {
            ReadOnlySpan<byte> digits = number.Digits; int i = 0; int numDigits = number.NumDigits; int num = numDigits;
            for (; digits[i] == 48; i++) { num--; }
            if (num == 0) { return 0.0; }
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
                if (number.IsNegative) { num6 |= 0x8000000000000000uL; }
                return *(double*)(&num6);
            }
            int pexp = 64;
            if ((num3 & 0xFFFFFFFF00000000uL) == 0L) { num3 <<= 32; pexp -= 32; }
            if ((num3 & 0xFFFF000000000000uL) == 0L) { num3 <<= 16; pexp -= 16; }
            if ((num3 & 0xFF00000000000000uL) == 0L) { num3 <<= 8; pexp -= 8; }
            if ((num3 & 0xF000000000000000uL) == 0L) { num3 <<= 4; pexp -= 4; }
            if ((num3 & 0xC000000000000000uL) == 0L) { num3 <<= 2; pexp -= 2; }
            if ((num3 & 0x8000000000000000uL) == 0L) { num3 <<= 1; pexp--; }
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
                if (num10 < num3) { num10 = (num10 >> 1) | 0x8000000000000000uL; pexp++; }
                num3 = num10;
            }
            pexp += 1022;
            num3 = ((pexp <= 0) ? ((pexp == -52 && num3 >= 9223372036854775896uL) ? 1 : ((pexp > -52) ? (num3 >> -pexp + 11 + 1) : 0)) : ((pexp < 2047) ? ((ulong)((long)pexp << 52) + ((num3 >> 11) & 0xFFFFFFFFFFFFFL)) : 9218868437227405312uL));
            if (number.IsNegative) { num3 |= 0x8000000000000000uL; }
            return *(double*)(&num3);
        }
    }

    internal ref struct NumberBuffer
    {
        public int Scale;

        public bool IsNegative;

        public const int BufferSize = 51;

        private byte _b0; private byte _b1; private byte _b2; private byte _b3; private byte _b4; private byte _b5; 
        
        private byte _b6; private byte _b7; private byte _b8; private byte _b9; private byte _b10; private byte _b11; 
        
        private byte _b12; private byte _b13; private byte _b14; private byte _b15; private byte _b16; private byte _b17;

        private byte _b18; private byte _b19; private byte _b20; private byte _b21; private byte _b22; private byte _b23;

        private byte _b24; private byte _b25; private byte _b26; private byte _b27; private byte _b28; private byte _b29;

        private byte _b30; private byte _b31; private byte _b32; private byte _b33; private byte _b34; private byte _b35; 
        
        private byte _b36; private byte _b37; private byte _b38; private byte _b39; private byte _b40; private byte _b41;

        private byte _b42; private byte _b43; private byte _b44; private byte _b45; private byte _b46; private byte _b47; 
        
        private byte _b48; private byte _b49; private byte _b50;

        public unsafe Span<byte> Digits => new Span<byte>(Unsafe.AsPointer(ref _b0), 51);

        public unsafe byte* UnsafeDigits => (byte*)Unsafe.AsPointer(ref _b0);

        public int NumDigits => Digits.IndexOf<byte>(0);

        [Conditional("DEBUG")]
        public void CheckConsistency() { }

        public override string ToString()
        {
            System.Text.StringBuilder stringBuilder = new();
            stringBuilder.Append('[');
            stringBuilder.Append('"');
            Span<byte> digits = Digits;
            for (int i = 0; i < 51; i++) { byte b = digits[i]; if (b == 0) { break; } stringBuilder.Append((char)b); }
            stringBuilder.Append('"');
            stringBuilder.Append(", Scale = " + Scale);
            stringBuilder.Append(", IsNegative   = " + IsNegative);
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }
    }

    internal static class ThrowHelper
    {
        internal static void ThrowArgumentNullException(System.ExceptionArgument argument) { throw CreateArgumentNullException(argument); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentNullException(System.ExceptionArgument argument) { return new ArgumentNullException(argument.ToString()); }

        internal static void ThrowArrayTypeMismatchException() { throw CreateArrayTypeMismatchException(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArrayTypeMismatchException() { return new ArrayTypeMismatchException(); }

        internal static void ThrowArgumentException_InvalidTypeWithPointersNotSupported(Type type) { throw CreateArgumentException_InvalidTypeWithPointersNotSupported(type); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_InvalidTypeWithPointersNotSupported(Type type) { return new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.Argument_InvalidTypeWithPointersNotSupported, type)); }

        internal static void ThrowArgumentException_DestinationTooShort() { throw CreateArgumentException_DestinationTooShort(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_DestinationTooShort() { return new ArgumentException(MDCFR.Properties.Resources.Argument_DestinationTooShort); }

        internal static void ThrowIndexOutOfRangeException() { throw CreateIndexOutOfRangeException(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateIndexOutOfRangeException() { return new IndexOutOfRangeException(); }

        internal static void ThrowArgumentOutOfRangeException() { throw CreateArgumentOutOfRangeException(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException() { return new ArgumentOutOfRangeException(); }

        internal static void ThrowArgumentOutOfRangeException(System.ExceptionArgument argument) { throw CreateArgumentOutOfRangeException(argument); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException(System.ExceptionArgument argument) { return new ArgumentOutOfRangeException(argument.ToString()); }

        internal static void ThrowArgumentOutOfRangeException_PrecisionTooLarge() { throw CreateArgumentOutOfRangeException_PrecisionTooLarge(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_PrecisionTooLarge()
        {
            return new ArgumentOutOfRangeException("precision", System.SR.Format(MDCFR.Properties.Resources.Argument_PrecisionTooLarge, (byte)99));
        }

        internal static void ThrowArgumentOutOfRangeException_SymbolDoesNotFit() { throw CreateArgumentOutOfRangeException_SymbolDoesNotFit(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_SymbolDoesNotFit()
        {
            return new ArgumentOutOfRangeException("symbol", MDCFR.Properties.Resources.Argument_BadFormatSpecifier);
        }

        internal static void ThrowInvalidOperationException() { throw CreateInvalidOperationException(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException() { return new InvalidOperationException(); }

        internal static void ThrowInvalidOperationException_OutstandingReferences() { throw CreateInvalidOperationException_OutstandingReferences(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_OutstandingReferences() { return new InvalidOperationException(MDCFR.Properties.Resources.OutstandingReferences); }

        internal static void ThrowInvalidOperationException_UnexpectedSegmentType() { throw CreateInvalidOperationException_UnexpectedSegmentType(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_UnexpectedSegmentType()
        {
            return new InvalidOperationException(MDCFR.Properties.Resources.UnexpectedSegmentType);
        }

        internal static void ThrowInvalidOperationException_EndPositionNotReached() { throw CreateInvalidOperationException_EndPositionNotReached(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateInvalidOperationException_EndPositionNotReached()
        {
            return new InvalidOperationException(MDCFR.Properties.Resources.EndPositionNotReached);
        }

        internal static void ThrowArgumentOutOfRangeException_PositionOutOfRange() { throw CreateArgumentOutOfRangeException_PositionOutOfRange(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_PositionOutOfRange() { return new ArgumentOutOfRangeException("position"); }

        internal static void ThrowArgumentOutOfRangeException_OffsetOutOfRange() { throw CreateArgumentOutOfRangeException_OffsetOutOfRange(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentOutOfRangeException_OffsetOutOfRange() { return new ArgumentOutOfRangeException("offset"); }

        internal static void ThrowObjectDisposedException_ArrayMemoryPoolBuffer() { throw CreateObjectDisposedException_ArrayMemoryPoolBuffer(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateObjectDisposedException_ArrayMemoryPoolBuffer() { return new ObjectDisposedException("ArrayMemoryPoolBuffer"); }

        internal static void ThrowFormatException_BadFormatSpecifier() { throw CreateFormatException_BadFormatSpecifier(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateFormatException_BadFormatSpecifier()
        {
            return new FormatException(MDCFR.Properties.Resources.Argument_BadFormatSpecifier);
        }

        internal static void ThrowArgumentException_OverlapAlignmentMismatch() { throw CreateArgumentException_OverlapAlignmentMismatch(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateArgumentException_OverlapAlignmentMismatch()
        { return new ArgumentException(MDCFR.Properties.Resources.Argument_OverlapAlignmentMismatch); }

        internal static void ThrowNotSupportedException() { throw CreateThrowNotSupportedException(); }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Exception CreateThrowNotSupportedException() { return new NotSupportedException(); }

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

        public static void ThrowArgumentValidationException<T>(Buffers.ReadOnlySequenceSegment<T> startSegment, int startIndex, Buffers.ReadOnlySequenceSegment<T> endSegment)
        {
            throw CreateArgumentValidationException(startSegment, startIndex, endSegment);
        }

        private static Exception CreateArgumentValidationException<T>(Buffers.ReadOnlySequenceSegment<T> startSegment, int startIndex, Buffers.ReadOnlySequenceSegment<T> endSegment)
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

        public static void ThrowArgumentValidationException(Array array, int start) { throw CreateArgumentValidationException(array, start); }

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

        public static void ThrowStartOrEndArgumentValidationException(long start) { throw CreateStartOrEndArgumentValidationException(start); }

        private static Exception CreateStartOrEndArgumentValidationException(long start)
        {
            if (start < 0) { return CreateArgumentOutOfRangeException(System.ExceptionArgument.start); }
            return CreateArgumentOutOfRangeException(System.ExceptionArgument.length);
        }
    }

    internal static class MathF
    {
        public const float PI = 3.1415927f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float x) { return Math.Abs(x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Acos(float x) { return (float)Math.Acos(x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float x) { return (float)Math.Cos(x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float IEEERemainder(float x, float y) { return (float)Math.IEEERemainder(x, y); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow(float x, float y) { return (float)Math.Pow(x, y); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float x) { return (float)Math.Sin(x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float x) { return (float)Math.Sqrt(x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan(float x) { return (float)Math.Tan(x); }
    }

    /// <summary>Provides a mechanism for releasing unmanaged resources asynchronously.</summary>
	public interface IAsyncDisposable
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources asynchronously.
        /// </summary>
        Threading.Tasks.ValueTask DisposeAsync();
    }

    internal static class HexConverter
    {
        public enum Casing : uint { Upper = 0u, Lower = 8224u }

        public static ReadOnlySpan<byte> CharToHexLookup => new byte[256]
        {
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 0, 1,
            2, 3, 4, 5, 6, 7, 8, 9, 255, 255,
            255, 255, 255, 255, 255, 10, 11, 12, 13, 14,
            15, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 10, 11, 12,
            13, 14, 15, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToBytesBuffer(byte value, Span<byte> buffer, int startingIndex = 0, Casing casing = Casing.Upper)
        {
            uint num = (uint)(((value & 0xF0) << 4) + (value & 0xF) - 35209);
            uint num2 = ((((0 - num) & 0x7070) >> 4) + num + 47545) | (uint)casing;
            buffer[startingIndex + 1] = (byte)num2;
            buffer[startingIndex] = (byte)(num2 >> 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ToCharsBuffer(byte value, Span<char> buffer, int startingIndex = 0, Casing casing = Casing.Upper)
        {
            uint num = (uint)(((value & 0xF0) << 4) + (value & 0xF) - 35209);
            uint num2 = ((((0 - num) & 0x7070) >> 4) + num + 47545) | (uint)casing;
            buffer[startingIndex + 1] = (char)(num2 & 0xFFu);
            buffer[startingIndex] = (char)(num2 >> 8);
        }

        public static void EncodeToUtf16(ReadOnlySpan<byte> bytes, Span<char> chars, Casing casing = Casing.Upper)
        {
            for (int i = 0; i < bytes.Length; i++) { ToCharsBuffer(bytes[i], chars, i * 2, casing); }
        }

        public static string ToString(ReadOnlySpan<byte> bytes, Casing casing = Casing.Upper)
        {
            Span<char> span = ((bytes.Length <= 16) ? stackalloc char[bytes.Length * 2] : new char[bytes.Length * 2].AsSpan());
            Span<char> buffer = span;
            int num = 0;
            ReadOnlySpan<byte> readOnlySpan = bytes;
            for (int i = 0; i < readOnlySpan.Length; i++)
            {
                byte value = readOnlySpan[i];
                ToCharsBuffer(value, buffer, num, casing);
                num += 2;
            }
            return buffer.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToCharUpper(int value)
        {
            value &= 0xF;
            value += 48;
            if (value > 57) { value += 7; }
            return (char)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToCharLower(int value)
        {
            value &= 0xF;
            value += 48;
            if (value > 57)
            {
                value += 39;
            }
            return (char)value;
        }

        public static bool TryDecodeFromUtf16(ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            return TryDecodeFromUtf16(chars, bytes, out int charsProcessed);
        }

        public static bool TryDecodeFromUtf16(ReadOnlySpan<char> chars, Span<byte> bytes, out int charsProcessed)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            while (num2 < bytes.Length)
            {
                num3 = FromChar(chars[num + 1]);
                num4 = FromChar(chars[num]);
                if ((num3 | num4) == 255)
                {
                    break;
                }
                bytes[num2++] = (byte)((num4 << 4) | num3);
                num += 2;
            }
            if (num3 == 255)
            {
                num++;
            }
            charsProcessed = num;
            return (num3 | num4) != 255;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromChar(int c)
        {
            if (c < CharToHexLookup.Length) { return CharToHexLookup[c]; }
            return 255;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromUpperChar(int c)
        {
            if (c <= 71) { return CharToHexLookup[c]; }
            return 255;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FromLowerChar(int c)
        {
            switch (c)
            {
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                    return c - 48;
                case 97:
                case 98:
                case 99:
                case 100:
                case 101:
                case 102:
                    return c - 97 + 10;
                default:
                    return 255;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHexChar(int c)
        {
            if (IntPtr.Size == 8)
            {
                ulong num = (uint)(c - 48);
                ulong num2 = (ulong)(-17875860044349952L << (int)num);
                ulong num3 = num - 64;
                if ((long)(num2 & num3) >= 0L)
                {
                    return false;
                }
                return true;
            }
            return FromChar(c) != 255;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHexUpperChar(int c)
        {
            if ((uint)(c - 48) > 9u)
            {
                return (uint)(c - 65) <= 5u;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHexLowerChar(int c)
        {
            if ((uint)(c - 48) > 9u)
            {
                return (uint)(c - 97) <= 5u;
            }
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, Inherited = false)]
    internal sealed class ObsoleteAttribute : Attribute
    {
        public string Message { get; }

        public bool IsError { get; }

        public string DiagnosticId { get; set; }

        public string UrlFormat { get; set; }

        public ObsoleteAttribute() { }

        public ObsoleteAttribute(string message) { Message = message; }

        public ObsoleteAttribute(string message, bool error) { Message = message; IsError = error; }
    }

    internal sealed class DefaultBinder : Binder
    {
        [Flags]
        private enum Primitives
        {
            Boolean = 8, Char = 0x10, SByte = 0x20, Byte = 0x40,
            Int16 = 0x80, UInt16 = 0x100, Int32 = 0x200, UInt32 = 0x400,
            Int64 = 0x800, UInt64 = 0x1000, Single = 0x2000, Double = 0x4000,
            Decimal = 0x8000, DateTime = 0x10000, String = 0x40000
        }

        private readonly MetadataLoadContext _loader;

        private readonly Type _objectType;

        private static readonly Primitives[] s_primitiveConversions = new Primitives[19]
        {
                0, 0, 0, Primitives.Boolean,
                Primitives.Char | Primitives.UInt16 | Primitives.Int32 | Primitives.UInt32 | Primitives.Int64 | Primitives.UInt64 | Primitives.Single | Primitives.Double,
                Primitives.SByte | Primitives.Int16 | Primitives.Int32 | Primitives.Int64 | Primitives.Single | Primitives.Double,
                Primitives.Char | Primitives.Byte | Primitives.Int16 | Primitives.UInt16 | Primitives.Int32 | Primitives.UInt32 | Primitives.Int64 | Primitives.UInt64 | Primitives.Single | Primitives.Double,
                Primitives.Int16 | Primitives.Int32 | Primitives.Int64 | Primitives.Single | Primitives.Double,
                Primitives.UInt16 | Primitives.Int32 | Primitives.UInt32 | Primitives.Int64 | Primitives.UInt64 | Primitives.Single | Primitives.Double,
                Primitives.Int32 | Primitives.Int64 | Primitives.Single | Primitives.Double,
                Primitives.UInt32 | Primitives.Int64 | Primitives.UInt64 | Primitives.Single | Primitives.Double,
                Primitives.Int64 | Primitives.Single | Primitives.Double,
                Primitives.UInt64 | Primitives.Single | Primitives.Double,
                Primitives.Single | Primitives.Double,
                Primitives.Double, Primitives.Decimal, Primitives.DateTime, 0, Primitives.String
        };

        internal DefaultBinder(MetadataLoadContext loader)
        {
            _loader = loader;
            _objectType = loader.TryGetCoreType(CoreType.Object);
        }

        private bool IsImplementedByMetadataLoadContext(Type type)
        {
            if (type is RoType roType) { return roType.Loader == _loader; }
            return false;
        }

        public sealed override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo cultureInfo, string[] names, out object state)
        {
            throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
        }

        public sealed override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, System.Globalization.CultureInfo cultureInfo)
        {
            throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
        }

        public sealed override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            Type[] array = new Type[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                array[i] = types[i].UnderlyingSystemType;
                if (!IsImplementedByMetadataLoadContext(array[i]) && !array[i].IsSignatureType())
                {
                    throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeType, "types");
                }
            }
            types = array;
            if (match == null || match.Length == 0)
            {
                throw new ArgumentException(MDCFR.Properties.Resources.Arg_EmptyArray, "match");
            }
            MethodBase[] array2 = (MethodBase[])match.Clone();
            int num = 0;
            for (int i = 0; i < array2.Length; i++)
            {
                ParameterInfo[] parametersNoCopy = array2[i].GetParametersNoCopy();
                if (parametersNoCopy.Length != types.Length) { continue; }
                int j;
                for (j = 0; j < types.Length; j++)
                {
                    Type parameterType = parametersNoCopy[j].ParameterType;
                    if (types[j].MatchesParameterTypeExactly(parametersNoCopy[j]) || parameterType == _objectType)
                    {
                        continue;
                    }
                    Type type = types[j];
                    if (type.IsSignatureType())
                    {
                        if (!(array2[i] is MethodInfo genericMethod)) { break; }
                        type = type.TryResolveAgainstGenericMethod(genericMethod);
                        if (type == null) { break; }
                    }
                    if (parameterType.IsPrimitive)
                    {
                        if (!IsImplementedByMetadataLoadContext(type.UnderlyingSystemType) || !CanChangePrimitive(type.UnderlyingSystemType, parameterType.UnderlyingSystemType))
                        {
                            break;
                        }
                    }
                    else if (!parameterType.IsAssignableFrom(type)) { break; }
                }
                if (j == types.Length) { array2[num++] = array2[i]; }
            }
            switch (num)
            {
                case 0:
                    return null;
                case 1:
                    return array2[0];
                default:
                    {
                        int num2 = 0;
                        bool flag = false;
                        int[] array3 = new int[types.Length];
                        for (int i = 0; i < types.Length; i++)
                        {
                            array3[i] = i;
                        }
                        for (int i = 1; i < num; i++)
                        {
                            switch (FindMostSpecificMethod(array2[num2], array3, null, array2[i], array3, null, types, null))
                            {
                                case 0:
                                    flag = true;
                                    break;
                                case 2:
                                    num2 = i;
                                    flag = false;
                                    break;
                            }
                        }
                        if (flag)
                        {
                            throw new AmbiguousMatchException();
                        }
                        return array2[num2];
                    }
            }
        }

        public sealed override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
        {
            if (indexes != null)
            {
                foreach (Type type in indexes) { if (type == null) { throw new ArgumentNullException("indexes"); } }
            }
            if (match == null || match.Length == 0) { throw new ArgumentException(MDCFR.Properties.Resources.Arg_EmptyArray, "match"); }
            PropertyInfo[] array = (PropertyInfo[])match.Clone();
            int j = 0; int num = 0; int num2 = ((indexes != null) ? indexes.Length : 0);
            for (int k = 0; k < array.Length; k++)
            {
                if (indexes != null)
                {
                    ParameterInfo[] indexParameters = array[k].GetIndexParameters();
                    if (indexParameters.Length != num2) { continue; }
                    for (j = 0; j < num2; j++)
                    {
                        Type parameterType = indexParameters[j].ParameterType;
                        if (parameterType == indexes[j] || parameterType == _objectType) { continue; }
                        if (parameterType.IsPrimitive)
                        {
                            if (!IsImplementedByMetadataLoadContext(indexes[j].UnderlyingSystemType) || !CanChangePrimitive(indexes[j].UnderlyingSystemType, parameterType.UnderlyingSystemType))
                            {
                                break;
                            }
                        }
                        else if (!parameterType.IsAssignableFrom(indexes[j])) { break; }
                    }
                }
                if (j != num2) { continue; }
                if (returnType != null)
                {
                    if (array[k].PropertyType.IsPrimitive)
                    {
                        if (!IsImplementedByMetadataLoadContext(returnType.UnderlyingSystemType) || !CanChangePrimitive(returnType.UnderlyingSystemType, array[k].PropertyType.UnderlyingSystemType))
                        {
                            continue;
                        }
                    }
                    else if (!array[k].PropertyType.IsAssignableFrom(returnType)) { continue; }
                }
                array[num++] = array[k];
            }
            switch (num)
            {
                case 0:
                    return null;
                case 1:
                    return array[0];
                default:
                    {
                        int num3 = 0;
                        bool flag = false;
                        int[] array2 = new int[num2];
                        for (int k = 0; k < num2; k++) { array2[k] = k; }
                        for (int k = 1; k < num; k++)
                        {
                            int num4 = FindMostSpecificType(array[num3].PropertyType, array[k].PropertyType, returnType);
                            if (num4 == 0 && indexes != null)
                            {
                                num4 = FindMostSpecific(array[num3].GetIndexParameters(), array2, null, array[k].GetIndexParameters(), array2, null, indexes, null);
                            }
                            if (num4 == 0)
                            {
                                num4 = FindMostSpecificProperty(array[num3], array[k]);
                                if (num4 == 0) { flag = true; }
                            }
                            if (num4 == 2) { flag = false; num3 = k; }
                        }
                        if (flag) { throw new AmbiguousMatchException(); }
                        return array[num3];
                    }
            }
        }

        public override object ChangeType(object value, Type type, System.Globalization.CultureInfo cultureInfo)
        {
            throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
        }

        public sealed override void ReorderArgumentArray(ref object[] args, object state)
        {
            throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
        }

        public static MethodBase ExactBinding(MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            if (match == null) { throw new ArgumentNullException("match"); }
            MethodBase[] array = new MethodBase[match.Length];
            int num = 0;
            for (int i = 0; i < match.Length; i++)
            {
                ParameterInfo[] parametersNoCopy = match[i].GetParametersNoCopy();
                if (parametersNoCopy.Length == 0) { continue; }
                int j;
                for (j = 0; j < types.Length; j++)
                {
                    Type parameterType = parametersNoCopy[j].ParameterType;
                    if (!parameterType.Equals(types[j])) { break; }
                }
                if (j >= types.Length) { array[num] = match[i]; num++; }
            }
            return num switch
            {
                0 => null,
                1 => array[0],
                _ => FindMostDerivedNewSlotMeth(array, num),
            };
        }

        public static PropertyInfo ExactPropertyBinding(PropertyInfo[] match, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (match == null) { throw new ArgumentNullException("match"); }
            PropertyInfo propertyInfo = null;
            int num = ((types != null) ? types.Length : 0);
            for (int i = 0; i < match.Length; i++)
            {
                ParameterInfo[] indexParameters = match[i].GetIndexParameters();
                int j;
                for (j = 0; j < num; j++)
                {
                    Type parameterType = indexParameters[j].ParameterType;
                    if (parameterType != types[j]) { break; }
                }
                if (j >= num && (!(returnType != null) || !(returnType != match[i].PropertyType)))
                {
                    if (propertyInfo != null) { throw new AmbiguousMatchException(); }
                    propertyInfo = match[i];
                }
            }
            return propertyInfo;
        }

        private static int FindMostSpecific(ParameterInfo[] p1, int[] paramOrder1, Type paramArrayType1, ParameterInfo[] p2, int[] paramOrder2, Type paramArrayType2, Type[] types, object[] args)
        {
            if (paramArrayType1 != null && paramArrayType2 == null) { return 2; }
            if (paramArrayType2 != null && paramArrayType1 == null) { return 1; }
            bool flag = false; bool flag2 = false;
            for (int i = 0; i < types.Length; i++)
            {
                if (args != null && args[i] == Type.Missing) { continue; }
                Type type = ((!(paramArrayType1 != null) || paramOrder1[i] < p1.Length - 1) ? p1[paramOrder1[i]].ParameterType : paramArrayType1);
                Type type2 = ((!(paramArrayType2 != null) || paramOrder2[i] < p2.Length - 1) ? p2[paramOrder2[i]].ParameterType : paramArrayType2);
                if (!(type == type2))
                {
                    switch (FindMostSpecificType(type, type2, types[i]))
                    {
                        case 0:
                            return 0;
                        case 1:
                            flag = true;
                            break;
                        case 2:
                            flag2 = true;
                            break;
                    }
                }
            }
            if (flag == flag2)
            {
                if (!flag && args != null)
                {
                    if (p1.Length > p2.Length) { return 1; }
                    if (p2.Length > p1.Length) { return 2; }
                }
                return 0;
            }
            if (!flag) { return 2; }
            return 1;
        }

        private static int FindMostSpecificType(Type c1, Type c2, Type t)
        {
            if (c1 == c2) { return 0; }
            if (t.IsSignatureType())
            {
                if (t.MatchesExactly(c1)) { return 1; }
                if (t.MatchesExactly(c2)) { return 2; }
            }
            else { if (c1 == t) { return 1; } if (c2 == t) { return 2; } }

            if (c1.IsByRef || c2.IsByRef)
            {
                if (c1.IsByRef && c2.IsByRef) { c1 = c1.GetElementType(); c2 = c2.GetElementType(); }
                else if (c1.IsByRef)
                {
                    if (c1.GetElementType() == c2) { return 2; }
                    c1 = c1.GetElementType();
                }
                else
                {
                    if (c2.GetElementType() == c1) { return 1; }
                    c2 = c2.GetElementType();
                }
            }
            bool flag; bool flag2;
            if (c1.IsPrimitive && c2.IsPrimitive) { flag = CanChangePrimitive(c2, c1); flag2 = CanChangePrimitive(c1, c2); }
            else { flag = c1.IsAssignableFrom(c2); flag2 = c2.IsAssignableFrom(c1); }

            if (flag == flag2) { return 0; }
            if (flag) { return 2; }
            return 1;
        }

        private static int FindMostSpecificMethod(MethodBase m1, int[] paramOrder1, Type paramArrayType1, MethodBase m2, int[] paramOrder2, Type paramArrayType2, Type[] types, object[] args)
        {
            int num = FindMostSpecific(m1.GetParametersNoCopy(), paramOrder1, paramArrayType1, m2.GetParametersNoCopy(), paramOrder2, paramArrayType2, types, args);
            if (num != 0) { return num; }
            if (CompareMethodSig(m1, m2))
            {
                int hierarchyDepth = GetHierarchyDepth(m1.DeclaringType);
                int hierarchyDepth2 = GetHierarchyDepth(m2.DeclaringType);
                if (hierarchyDepth == hierarchyDepth2) { return 0; }
                if (hierarchyDepth < hierarchyDepth2) { return 2; }
                return 1;
            }
            return 0;
        }

        private static int FindMostSpecificProperty(PropertyInfo cur1, PropertyInfo cur2)
        {
            if (cur1.Name == cur2.Name)
            {
                int hierarchyDepth = GetHierarchyDepth(cur1.DeclaringType);
                int hierarchyDepth2 = GetHierarchyDepth(cur2.DeclaringType);
                if (hierarchyDepth == hierarchyDepth2) { return 0; }
                if (hierarchyDepth < hierarchyDepth2) { return 2; }
                return 1;
            }
            return 0;
        }

        public static bool CompareMethodSig(MethodBase m1, MethodBase m2)
        {
            ParameterInfo[] parametersNoCopy = m1.GetParametersNoCopy();
            ParameterInfo[] parametersNoCopy2 = m2.GetParametersNoCopy();
            if (parametersNoCopy.Length != parametersNoCopy2.Length) { return false; }
            for (int i = 0; i < parametersNoCopy.Length; i++)
            {
                if (parametersNoCopy[i].ParameterType != parametersNoCopy2[i].ParameterType) { return false; }
            }
            return true;
        }

        private static int GetHierarchyDepth(Type t)
        {
            int num = 0;
            Type type = t;
            do { num++; type = type.BaseType; } while (type != null);
            return num;
        }

        internal static MethodBase FindMostDerivedNewSlotMeth(MethodBase[] match, int cMatches)
        {
            int num = 0;
            MethodBase result = null;
            for (int i = 0; i < cMatches; i++)
            {
                int hierarchyDepth = GetHierarchyDepth(match[i].DeclaringType);
                if (hierarchyDepth == num) { throw new AmbiguousMatchException(); }
                if (hierarchyDepth > num) { num = hierarchyDepth; result = match[i]; }
            }
            return result;
        }

        private static bool CanChangePrimitive(Type source, Type target)
        {
            return CanPrimitiveWiden(source, target);
        }

        private static bool CanPrimitiveWiden(Type source, Type target)
        {
            Primitives primitives = s_primitiveConversions[(int)Type.GetTypeCode(source)];
            Primitives primitives2 = (Primitives)(1 << (int)Type.GetTypeCode(target));
            return (primitives & primitives2) != 0;
        }
    }

}
#endif

// Types exposed in any .NET flavor or version.

namespace System
{
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
            if (UsingResourceKeys()) { return string.Join(", ", resourceFormat, p1); }

            return string.Format(resourceFormat, p1);
        }

        public static System.String Format(string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys()) { return string.Join(", ", resourceFormat, p1, p2); }

            return string.Format(resourceFormat, p1, p2);
        }

        public static System.String Format(string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys()) { return string.Join(", ", resourceFormat, p1, p2, p3); }

            return string.Format(resourceFormat, p1, p2, p3);
        }

        public static System.String Format(string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys()) { return resourceFormat + ", " + string.Join(", ", args); }

                return string.Format(resourceFormat, args);
            }

            return resourceFormat;
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, object? p1)
        {
            if (UsingResourceKeys()) { return string.Join(", ", resourceFormat, p1); }

            return string.Format(provider, resourceFormat, p1);
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2)
        {
            if (UsingResourceKeys()) { return string.Join(", ", resourceFormat, p1, p2); }

            return string.Format(provider, resourceFormat, p1, p2);
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, object? p1, object? p2, object? p3)
        {
            if (UsingResourceKeys()) { return string.Join(", ", resourceFormat, p1, p2, p3); }

            return string.Format(provider, resourceFormat, p1, p2, p3);
        }

        public static System.String Format(IFormatProvider? provider, string resourceFormat, params object?[]? args)
        {
            if (args != null)
            {
                if (UsingResourceKeys()) { return resourceFormat + ", " + string.Join(", ", args); }

                return string.Format(provider, resourceFormat, args);
            }

            return resourceFormat;
        }

    }

    #pragma warning restore CS1591

    namespace Runtime.InteropServices
    {
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
        internal sealed class LibraryImportAttribute : Attribute
        {
            public string LibraryName { get; }

            public string? EntryPoint { get; set; }

            public StringMarshalling StringMarshalling { get; set; }

            public Type? StringMarshallingCustomType { get; set; }

            public bool SetLastError { get; set; }

            public LibraryImportAttribute(string libraryName) { LibraryName = libraryName; }
        }

        #nullable disable
        internal enum StringMarshalling { Custom, Utf8, Utf16 }
    }
}

/// <summary>
/// The subsetted clone of the global::Interop class used to 
/// make the proper interopability between native methods and
/// .NET code.
/// </summary>
// Do not modify it's contents since the Interop class is programmatically created and embedded in the
// assembly. The only case now that this class should be modified is the new package or API
// additions which use the Interop class.
internal static partial class Interop
{
    internal static class Libraries
    {
        internal const string Activeds = "activeds.dll";

        internal const string Advapi32 = "advapi32.dll";

        internal const string Authz = "authz.dll";

        internal const string BCrypt = "BCrypt.dll";

        internal const string Credui = "credui.dll";

        internal const string Crypt32 = "crypt32.dll";

        internal const string CryptUI = "cryptui.dll";

        internal const string Dnsapi = "dnsapi.dll";

        internal const string Dsrole = "dsrole.dll";

        internal const string Gdi32 = "gdi32.dll";

        internal const string HttpApi = "httpapi.dll";

        internal const string IpHlpApi = "iphlpapi.dll";

        internal const string Kernel32 = "kernel32.dll";

        internal const string Logoncli = "logoncli.dll";

        internal const string Mswsock = "mswsock.dll";

        internal const string NCrypt = "ncrypt.dll";

        internal const string Netapi32 = "netapi32.dll";

        internal const string Netutils = "netutils.dll";

        internal const string NtDll = "ntdll.dll";

        internal const string Odbc32 = "odbc32.dll";

        internal const string Ole32 = "ole32.dll";

        internal const string OleAut32 = "oleaut32.dll";

        internal const string Pdh = "pdh.dll";

        internal const string Secur32 = "secur32.dll";

        internal const string Shell32 = "shell32.dll";

        internal const string SspiCli = "sspicli.dll";

        internal const string User32 = "user32.dll";

        internal const string Version = "version.dll";

        internal const string WebSocket = "websocket.dll";

        internal const string Wevtapi = "wevtapi.dll";

        internal const string WinHttp = "winhttp.dll";

        internal const string WinMM = "winmm.dll";

        internal const string Wkscli = "wkscli.dll";

        internal const string Wldap32 = "wldap32.dll";

        internal const string Ws2_32 = "ws2_32.dll";

        internal const string Wtsapi32 = "wtsapi32.dll";

        internal const string CompressionNative = "System.IO.Compression.Native";

        internal const string GlobalizationNative = "System.Globalization.Native";

        internal const string MsQuic = "msquic.dll";

        internal const string HostPolicy = "hostpolicy.dll";

        internal const string Ucrtbase = "ucrtbase.dll";

        internal const string Xolehlp = "xolehlp.dll";

        internal const System.String XXHash = "xxhash.dll";

        internal const System.String Zstd = "zstd.dll";
    }

    /// <summary>
	/// Blittable version of Windows BOOL type. It is convenient in situations where
	/// manual marshalling is required, or to avoid overhead of regular bool marshalling.
	/// </summary>
	/// <remarks>
	/// Some Windows APIs return arbitrary integer values although the return type is defined
	/// as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
	/// or bResult == BOOL.FALSE .
	/// </remarks>
	internal enum BOOL { FALSE, TRUE }

    /// <summary>
    /// Blittable version of Windows BOOLEAN type. It is convenient in situations where
    /// manual marshalling is required, or to avoid overhead of regular bool marshalling.
    /// </summary>
    /// <remarks>
    /// Some Windows APIs return arbitrary integer values although the return type is defined
    /// as BOOLEAN. It is best to never compare BOOLEAN to TRUE. Always use bResult != BOOLEAN.FALSE
    /// or bResult == BOOLEAN.FALSE .
    /// </remarks>
    internal enum BOOLEAN : byte { FALSE, TRUE }

    [System.Security.SecurityCritical]
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static partial class Kernel32
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CPINFOEXW
        {
            internal uint MaxCharSize;

            internal unsafe fixed byte DefaultChar[2];

            internal unsafe fixed byte LeadByte[12];

            internal char UnicodeDefaultChar;

            internal uint CodePage;

            internal unsafe fixed char CodePageName[260];
        }

        internal const int MAX_PATH = 260;

        internal const uint CP_ACP = 0u;

        internal const uint WC_NO_BEST_FIT_CHARS = 1024u;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private unsafe static extern BOOL GetCPInfoExW(uint CodePage, uint dwFlags, CPINFOEXW* lpCPInfoEx);

        internal unsafe static int GetLeadByteRanges(int codePage, byte[] leadByteRanges)
        {
            int num = 0;
            CPINFOEXW cPINFOEXW = default;
            if (GetCPInfoExW((uint)codePage, 0u, &cPINFOEXW) != 0)
            {
                for (int i = 0; i < 10 && leadByteRanges[i] != 0; i += 2)
                {
                    leadByteRanges[i] = cPINFOEXW.LeadByte[i];
                    leadByteRanges[i + 1] = cPINFOEXW.LeadByte[i + 1];
                    num++;
                }
            }
            return num;
        }

        internal unsafe static bool TryGetACPCodePage(out int codePage)
        {
            CPINFOEXW cPINFOEXW = default;
            if (GetCPInfoExW(0u, 0u, &cPINFOEXW) != 0)
            {
                codePage = (int)cPINFOEXW.CodePage;
                return true;
            }
            codePage = 0;
            return false;
        }

        [DllImport("kernel32.dll", ExactSpelling = true)]
        internal unsafe static extern int WideCharToMultiByte(uint CodePage, uint dwFlags, char* lpWideCharStr, int cchWideChar, byte* lpMultiByteStr, int cbMultiByte, byte* lpDefaultChar, BOOL* lpUsedDefaultChar);

        [DllImport("kernel32.dll", EntryPoint = "Beep")]
#if NET7_0_OR_GREATER == false
        [System.Security.Permissions.HostProtection(
            Action = System.Security.Permissions.SecurityAction.Assert,
            Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
            UI = true)]
#endif
        internal static extern System.Int32 ConsoleBeep(System.Int16 Frequency, System.Int16 Timeout);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        internal unsafe static extern System.Int32 ReadFile(SafeHandle handle, 
            byte* bytes, int numBytesToRead, out int numBytesRead, System.IntPtr mustBeZero);
    }

}
