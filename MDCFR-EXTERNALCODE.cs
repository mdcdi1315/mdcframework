// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text;
using System.Buffers;
using System.Diagnostics;
using System.Buffers.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Sources;
using System.Collections.Generic;
#if NET6_0_OR_GREATER
	using System.Runtime.Intrinsics;
	using System.Runtime.Intrinsics.X86;
	using static System.Runtime.Intrinsics.X86.Ssse3;
#endif

namespace Microsoft.Win32.SafeHandles
{
    internal sealed class SafeAllocHHandle : SafeBuffer
    {
        internal static SafeAllocHHandle InvalidHandle => new SafeAllocHHandle(System.IntPtr.Zero);

        public SafeAllocHHandle()
            : base(ownsHandle: true)
        {
        }

        internal SafeAllocHHandle(System.IntPtr handle)
            : base(ownsHandle: true)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            if (handle != System.IntPtr.Zero)
            {
                Marshal.FreeHGlobal(handle);
            }
            return true;
        }
    }
}

namespace Internal
{
    [StructLayout(LayoutKind.Explicit, Size = 124)]
    internal struct PaddingFor32 { }

    internal static class PaddingHelpers { internal const int CACHE_LINE_SIZE = 128; }
}

namespace System
{
    #nullable enable

    namespace Collections.Generic
    {
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

        // Code required when the Snappy Archiving is compiled < .NET 6 .
        #if ! NET6_0_OR_GREATER
            // Licensed to the .NET Foundation under one or more agreements.
            // The .NET Foundation licenses this file to you under the MIT license.

            [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
            internal sealed class CallerArgumentExpressionAttribute : Attribute
            {
                public CallerArgumentExpressionAttribute(string parameterName) { ParameterName = parameterName; }

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
        /// Indicates that the use of <see cref="T:System.ValueTuple" /> on a member is meant to be treated as a tuple with element names.
        /// </summary>
        [CLSCompliant(false)]
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
        public sealed class TupleElementNamesAttribute : Attribute
        {
            private readonly string[] _transformNames;

            /// <summary>
            /// Specifies, in a pre-order depth-first traversal of a type's
            /// construction, which <see cref="T:System.ValueTuple" /> elements are
            /// meant to carry element names.
            /// </summary>
            public IList<string> TransformNames => _transformNames;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.TupleElementNamesAttribute" /> class.
            /// </summary>
            /// <param name="transformNames">
            /// Specifies, in a pre-order depth-first traversal of a type's
            /// construction, which <see cref="T:System.ValueType" /> occurrences are
            /// meant to carry element names.
            /// </param>
            /// <remarks>
            /// This constructor is meant to be used on types that contain an
            /// instantiation of <see cref="T:System.ValueType" /> that contains
            /// element names.  For instance, if <c>C</c> is a generic type with
            /// two type parameters, then a use of the constructed type <c>C{<see cref="T:System.ValueTuple`2" />, <see cref="T:System.ValueTuple`3" /></c> might be intended to
            /// treat the first type argument as a tuple with element names and the
            /// second as a tuple without element names. In which case, the
            /// appropriate attribute specification should use a
            /// <c>transformNames</c> value of <c>{ "name1", "name2", null, null,
            /// null }</c>.
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
            /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result of invoking the transform function on each element of source.</returns>
            public static IEnumerable<TResult> Select<T, TResult>(this ImmutableArray<T> immutableArray, Func<T, TResult> selector)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                return immutableArray.array.Select(selector);
            }

            /// <summary>Projects each element of a sequence to an <see cref="T:System.Collections.Generic.IEnumerable`1" />,             flattens the resulting sequences into one sequence, and invokes a result             selector function on each element therein.</summary>
            /// <param name="immutableArray">The immutable array.</param>
            /// <param name="collectionSelector">A transform function to apply to each element of the input sequence.</param>
            /// <param name="resultSelector">A transform function to apply to each element of the intermediate sequence.</param>
            /// <typeparam name="TSource">The type of the elements of <paramref name="immutableArray" />.</typeparam>
            /// <typeparam name="TCollection">The type of the intermediate elements collected by <paramref name="collectionSelector" />.</typeparam>
            /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
            /// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> whose elements are the result             of invoking the one-to-many transform function <paramref name="collectionSelector" /> on each             element of <paramref name="immutableArray" /> and then mapping each of those sequence elements and their             corresponding source element to a result element.</returns>
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
            /// <returns>Returns <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains elements that meet the condition.</returns>
            public static IEnumerable<T> Where<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                return immutableArray.array.Where(predicate);
            }

            /// <summary>Gets a value indicating whether the array contains any elements.</summary>
            /// <param name="immutableArray">The array to check for elements.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>
            ///   <see langword="true" /> if the array contains an elements; otherwise, <see langword="false" />.</returns>
            public static bool Any<T>(this ImmutableArray<T> immutableArray)
            {
                return immutableArray.Length > 0;
            }

            /// <summary>Gets a value indicating whether the array contains any elements that match a specified condition.</summary>
            /// <param name="immutableArray">The array to check for elements.</param>
            /// <param name="predicate">The delegate that defines the condition to match to an element.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>
            ///   <see langword="true" /> if an element matches the specified condition; otherwise, <see langword="false" />.</returns>
            public static bool Any<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                immutableArray.ThrowNullRefIfNotInitialized();
                Requires.NotNull(predicate, "predicate");
                T[] array = immutableArray.array;
                foreach (T arg in array)
                {
                    if (predicate(arg))
                    {
                        return true;
                    }
                }
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
                foreach (T arg in array)
                {
                    if (!predicate(arg))
                    {
                        return false;
                    }
                }
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
                if (comparer == null)
                {
                    comparer = EqualityComparer<TBase>.Default;
                }
                int num = 0;
                int length = immutableArray.Length;
                foreach (TDerived item in items)
                {
                    if (num == length)
                    {
                        return false;
                    }
                    if (!comparer.Equals(immutableArray[num], (TBase)(object)item))
                    {
                        return false;
                    }
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
                if (immutableArray.Length == 0)
                {
                    return default(T);
                }
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
                foreach (T arg in array)
                {
                    val = func(val, arg);
                }
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
            public static T ElementAt<T>(this ImmutableArray<T> immutableArray, int index)
            {
                return immutableArray[index];
            }

            /// <summary>Returns the element at a specified index in a sequence or a default value if the index is out of range.</summary>
            /// <param name="immutableArray">The array to find an element in.</param>
            /// <param name="index">The index for the element to retrieve.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The item at the specified index, or the default value if the index is not found.</returns>
            public static T? ElementAtOrDefault<T>(this ImmutableArray<T> immutableArray, int index)
            {
                if (index < 0 || index >= immutableArray.Length)
                {
                    return default(T);
                }
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
                foreach (T val in array)
                {
                    if (predicate(val))
                    {
                        return val;
                    }
                }
                return Enumerable.Empty<T>().First();
            }

            /// <summary>Returns the first element in an array.</summary>
            /// <param name="immutableArray">The array to get an item from.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">If the array is empty.</exception>
            /// <returns>The first item in the array.</returns>
            public static T First<T>(this ImmutableArray<T> immutableArray)
            {
                if (immutableArray.Length <= 0)
                {
                    return immutableArray.array.First();
                }
                return immutableArray[0];
            }

            /// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
            /// <param name="immutableArray">The array to retrieve items from.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <returns>The first item in the list, if found; otherwise the default value for the item type.</returns>
            public static T? FirstOrDefault<T>(this ImmutableArray<T> immutableArray)
            {
                if (immutableArray.array.Length == 0)
                {
                    return default(T);
                }
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
                foreach (T val in array)
                {
                    if (predicate(val))
                    {
                        return val;
                    }
                }
                return default(T);
            }

            /// <summary>Returns the last element of the array.</summary>
            /// <param name="immutableArray">The array to retrieve items from.</param>
            /// <typeparam name="T">The type of element contained by the array.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">The collection is empty.</exception>
            /// <returns>The last element in the array.</returns>
            public static T Last<T>(this ImmutableArray<T> immutableArray)
            {
                if (immutableArray.Length <= 0)
                {
                    return immutableArray.array.Last();
                }
                return immutableArray[immutableArray.Length - 1];
            }

            /// <summary>Returns the last element of a sequence that satisfies a specified condition.</summary>
            /// <param name="immutableArray">The array to retrieve elements from.</param>
            /// <param name="predicate">The delegate that defines the conditions of the element to retrieve.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">The collection is empty.</exception>
            /// <returns>The last element of the array that satisfies the <paramref name="predicate" /> condition.</returns>
            public static T Last<T>(this ImmutableArray<T> immutableArray, Func<T, bool> predicate)
            {
                Requires.NotNull(predicate, "predicate");
                for (int num = immutableArray.Length - 1; num >= 0; num--)
                {
                    if (predicate(immutableArray[num]))
                    {
                        return immutableArray[num];
                    }
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
                    if (predicate(immutableArray[num]))
                    {
                        return immutableArray[num];
                    }
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
            /// <returns>Returns <see cref="T:System.Boolean" />.</returns>
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
                        if (!flag)
                        {
                            ImmutableArray.TwoElementArray.Single();
                        }
                        flag = false;
                        result = val;
                    }
                }
                if (flag)
                {
                    Enumerable.Empty<T>().Single();
                }
                return result;
            }

            /// <summary>Returns the only element of the array, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.</summary>
            /// <param name="immutableArray">The array.</param>
            /// <typeparam name="T">The type of element contained by the collection.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">
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
                        if (!flag)
                        {
                            ImmutableArray.TwoElementArray.Single();
                        }
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
                if (immutableArray.array.Length == 0)
                {
                    return ImmutableArray<T>.Empty.array;
                }
                return (T[])immutableArray.array.Clone();
            }

            /// <summary>Returns the first element in the collection.</summary>
            /// <param name="builder">The builder to retrieve an item from.</param>
            /// <typeparam name="T">The type of items in the array.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">If the array is empty.</exception>
            /// <returns>The first item in the list.</returns>
            public static T First<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any())
                {
                    throw new InvalidOperationException();
                }
                return builder[0];
            }

            /// <summary>Returns the first element in the collection, or the default value if the collection is empty.</summary>
            /// <param name="builder">The builder to retrieve an element from.</param>
            /// <typeparam name="T">The type of item in the builder.</typeparam>
            /// <returns>The first item in the list, if found; otherwise the default value for the item type.</returns>
            public static T? FirstOrDefault<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any())
                {
                    return default(T);
                }
                return builder[0];
            }

            /// <summary>Returns the last element in the collection.</summary>
            /// <param name="builder">The builder to retrieve elements from.</param>
            /// <typeparam name="T">The type of item in the builder.</typeparam>
            /// <exception cref="T:System.InvalidOperationException">The collection is empty.</exception>
            /// <returns>The last element in the builder.</returns>
            public static T Last<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any())
                {
                    throw new InvalidOperationException();
                }
                return builder[builder.Count - 1];
            }

            /// <summary>Returns the last element in the collection, or the default value if the collection is empty.</summary>
            /// <param name="builder">The builder to retrieve an element from.</param>
            /// <typeparam name="T">The type of item in the builder.</typeparam>
            /// <returns>The last element of a sequence, or a default value if the sequence contains no elements.</returns>
            public static T? LastOrDefault<T>(this ImmutableArray<T>.Builder builder)
            {
                Requires.NotNull(builder, "builder");
                if (!builder.Any())
                {
                    return default(T);
                }
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
        #pragma warning restore CS8600 , CS8602 , CS8603 , CS8604
        #nullable disable
    }

    namespace Runtime.InteropServices
    {
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

        #nullable enable
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
        internal enum StringMarshalling
        {
            Custom,
            Utf8,
            Utf16
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
        internal sealed class NonVersionableAttribute : Attribute { public NonVersionableAttribute() { } }

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
            }
            else
            {
                while (i > 0 && digits[i - 1] == 48) { i--; }
            }
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
            bool flag = Utf8Parser.TryParse(digits.Slice(0, count), out value, out bytesConsumed, 'D');
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

        private byte _b0; private byte _b1; private byte _b2;

        private byte _b3; private byte _b4; private byte _b5; 
        
        private byte _b6; private byte _b7; private byte _b8;

        private byte _b9; private byte _b10; private byte _b11; 
        
        private byte _b12; private byte _b13; private byte _b14;

        private byte _b15; private byte _b16; private byte _b17;

        private byte _b18; private byte _b19; private byte _b20;

        private byte _b21; private byte _b22; private byte _b23;

        private byte _b24; private byte _b25; private byte _b26;

        private byte _b27; private byte _b28; private byte _b29;

        private byte _b30; private byte _b31; private byte _b32;

        private byte _b33; private byte _b34; private byte _b35;

        private byte _b36; private byte _b37; private byte _b38;

        private byte _b39; private byte _b40; private byte _b41;

        private byte _b42; private byte _b43; private byte _b44;

        private byte _b45; private byte _b46; private byte _b47; 
        
        private byte _b48; private byte _b49; private byte _b50;

        public unsafe Span<byte> Digits => new Span<byte>(Unsafe.AsPointer(ref _b0), 51);

        public unsafe byte* UnsafeDigits => (byte*)Unsafe.AsPointer(ref _b0);

        public int NumDigits => Digits.IndexOf<byte>(0);

        [Conditional("DEBUG")]
        public void CheckConsistency() { }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
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

}


/// <summary>
/// The subsetted clone of the global::Interop class used to 
/// make the proper interopability between native methods and
/// .NET code.
/// </summary>
// Do not modify it's contents since the Interop class is programmatically created and embedded in the
// assembly. The only case now that this class should be modified is the new package or API
// additions which use the Interop class.
internal static class Interop
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

    internal enum BOOL
    {
        FALSE,
        TRUE
    }

    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static class Kernel32
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
        [LibraryImport("kernel32.dll", EntryPoint = "GetCPInfoExW", StringMarshalling = StringMarshalling.Utf16)]
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
        [LibraryImport("kernel32.dll")]
        internal unsafe static extern int WideCharToMultiByte(uint CodePage, uint dwFlags, char* lpWideCharStr, int cchWideChar, byte* lpMultiByteStr, int cbMultiByte, byte* lpDefaultChar, BOOL* lpUsedDefaultChar);

        [DllImport("kernel32.dll", EntryPoint = "Beep")]
        [System.Security.Permissions.HostProtection(
            Action = System.Security.Permissions.SecurityAction.Assert,
            Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
            UI = true)]
        internal static extern System.Int32 ConsoleBeep(System.Int16 Frequency, System.Int16 Timeout);
    }

    [System.Security.SecurityCritical]
    [System.Security.SuppressUnmanagedCodeSecurity]
    internal static class Shell32
    {
        [DllImport(Libraries.Shell32 , EntryPoint = "ShellAboutW" , CallingConvention = CallingConvention.Winapi)]
        [System.Security.Permissions.HostProtection(
            Action = System.Security.Permissions.SecurityAction.Assert, 
            Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt, 
            UI = true)]
        private static extern System.Int32 Shownetframeworkinfo(System.IntPtr Handle, 
            [MarshalAs(UnmanagedType.LPWStr)] System.String Title, 
            [MarshalAs(UnmanagedType.LPWStr)] System.String desc, System.IntPtr IHandle);

        public static System.Boolean ShowDotNetFrameworkInfo()
        {
            if (Shownetframeworkinfo(
                System.IntPtr.Zero , 
                "Microsoft ® .NET Framework" , 
                ".NET Framework is a product of Microsoft Corporation.\n" +
                $"Common Language Runtime Version: {ROOT.MAIN.GetRuntimeVersion()} \n" +
                $"Current Machine Architecture: {ROOT.MAIN.OSProcessorArchitecture()}" ,
                System.IntPtr.Zero) != 0) { return true; } else { return false; }
        }

        public static System.Boolean ShowDotNetFrameworkInfo(System.Windows.Forms.IWin32Window hwnd)
        {
            if (Shownetframeworkinfo(
                hwnd.Handle,
                "Microsoft ® .NET Framework",
                ".NET Framework is a product of Microsoft Corporation.\n" +
                $"Common Language Runtime Version: {ROOT.MAIN.GetRuntimeVersion()} \n" +
                $"Current Machine Architecture: {ROOT.MAIN.OSProcessorArchitecture()}",
                System.IntPtr.Zero) != 0) { return true; } else { return false; }
        }

        [DllImport(Libraries.Shell32, CallingConvention = CallingConvention.Winapi, EntryPoint = "ShellExecuteW")]
        [System.Security.Permissions.HostProtection(
            Action = System.Security.Permissions.SecurityAction.Assert,
            Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
            UI = true)]
        internal static extern System.Int16 ExecuteApp(System.IntPtr winHandle,
            [MarshalAs(UnmanagedType.LPWStr)] System.String Verb,
            [MarshalAs(UnmanagedType.LPWStr)] System.String Path,
            [MarshalAs(UnmanagedType.LPWStr)] System.String Parameters,
            [MarshalAs(UnmanagedType.LPWStr)] System.String WorkDir,
            System.Int32 WinShowArgs);

        internal struct ExecuteVerbs 
        {
            public const System.String RunAs = "runas";

            public const System.String Print = "print";

            public const System.String Explore = "explore";

            public const System.String Find = "find";

            public const System.String Edit = "edit";

            public const System.String Open = "open";
        }


    }

}
