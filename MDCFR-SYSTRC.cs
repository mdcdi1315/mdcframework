// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace System
{
    internal static class Obsoletions
    {
        internal const string SharedUrlFormat = "https://aka.ms/dotnet-warnings/{0}";

        internal const string SystemTextEncodingUTF7Message = "The UTF-7 encoding is insecure and should not be used. Consider using UTF-8 instead.";

        internal const string SystemTextEncodingUTF7DiagId = "SYSLIB0001";

        internal const string PrincipalPermissionAttributeMessage = "PrincipalPermissionAttribute is not honored by the runtime and must not be used.";

        internal const string PrincipalPermissionAttributeDiagId = "SYSLIB0002";

        internal const string CodeAccessSecurityMessage = "Code Access Security is not supported or honored by the runtime.";

        internal const string CodeAccessSecurityDiagId = "SYSLIB0003";

        internal const string ConstrainedExecutionRegionMessage = "The Constrained Execution Region (CER) feature is not supported.";

        internal const string ConstrainedExecutionRegionDiagId = "SYSLIB0004";

        internal const string GlobalAssemblyCacheMessage = "The Global Assembly Cache is not supported.";

        internal const string GlobalAssemblyCacheDiagId = "SYSLIB0005";

        internal const string ThreadAbortMessage = "Thread.Abort is not supported and throws PlatformNotSupportedException.";

        internal const string ThreadResetAbortMessage = "Thread.ResetAbort is not supported and throws PlatformNotSupportedException.";

        internal const string ThreadAbortDiagId = "SYSLIB0006";

        internal const string DefaultCryptoAlgorithmsMessage = "The default implementation of this cryptography algorithm is not supported.";

        internal const string DefaultCryptoAlgorithmsDiagId = "SYSLIB0007";

        internal const string CreatePdbGeneratorMessage = "The CreatePdbGenerator API is not supported and throws PlatformNotSupportedException.";

        internal const string CreatePdbGeneratorDiagId = "SYSLIB0008";

        internal const string AuthenticationManagerMessage = "The AuthenticationManager Authenticate and PreAuthenticate methods are not supported and throw PlatformNotSupportedException.";

        internal const string AuthenticationManagerDiagId = "SYSLIB0009";

        internal const string RemotingApisMessage = "This Remoting API is not supported and throws PlatformNotSupportedException.";

        internal const string RemotingApisDiagId = "SYSLIB0010";

        internal const string BinaryFormatterMessage = "BinaryFormatter serialization is obsolete and should not be used. See https://aka.ms/binaryformatter for more information.";

        internal const string BinaryFormatterDiagId = "SYSLIB0011";

        internal const string CodeBaseMessage = "Assembly.CodeBase and Assembly.EscapedCodeBase are only included for .NET Framework compatibility. Use Assembly.Location instead.";

        internal const string CodeBaseDiagId = "SYSLIB0012";

        internal const string EscapeUriStringMessage = "Uri.EscapeUriString can corrupt the Uri string in some cases. Consider using Uri.EscapeDataString for query string components instead.";

        internal const string EscapeUriStringDiagId = "SYSLIB0013";

        internal const string WebRequestMessage = "WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.";

        internal const string WebRequestDiagId = "SYSLIB0014";

        internal const string DisablePrivateReflectionAttributeMessage = "DisablePrivateReflectionAttribute has no effect in .NET 6.0+.";

        internal const string DisablePrivateReflectionAttributeDiagId = "SYSLIB0015";

        internal const string GetContextInfoMessage = "Use the Graphics.GetContextInfo overloads that accept arguments for better performance and fewer allocations.";

        internal const string GetContextInfoDiagId = "SYSLIB0016";

        internal const string StrongNameKeyPairMessage = "Strong name signing is not supported and throws PlatformNotSupportedException.";

        internal const string StrongNameKeyPairDiagId = "SYSLIB0017";

        internal const string ReflectionOnlyLoadingMessage = "ReflectionOnly loading is not supported and throws PlatformNotSupportedException.";

        internal const string ReflectionOnlyLoadingDiagId = "SYSLIB0018";

        internal const string RuntimeEnvironmentMessage = "RuntimeEnvironment members SystemConfigurationFile, GetRuntimeInterfaceAsIntPtr, and GetRuntimeInterfaceAsObject are not supported and throw PlatformNotSupportedException.";

        internal const string RuntimeEnvironmentDiagId = "SYSLIB0019";

        internal const string JsonSerializerOptionsIgnoreNullValuesMessage = "JsonSerializerOptions.IgnoreNullValues is obsolete. To ignore null values when serializing, set DefaultIgnoreCondition to JsonIgnoreCondition.WhenWritingNull.";

        internal const string JsonSerializerOptionsIgnoreNullValuesDiagId = "SYSLIB0020";

        internal const string DerivedCryptographicTypesMessage = "Derived cryptographic types are obsolete. Use the Create method on the base type instead.";

        internal const string DerivedCryptographicTypesDiagId = "SYSLIB0021";

        internal const string RijndaelMessage = "The Rijndael and RijndaelManaged types are obsolete. Use Aes instead.";

        internal const string RijndaelDiagId = "SYSLIB0022";

        internal const string RNGCryptoServiceProviderMessage = "RNGCryptoServiceProvider is obsolete. To generate a random number, use one of the RandomNumberGenerator static methods instead.";

        internal const string RNGCryptoServiceProviderDiagId = "SYSLIB0023";

        internal const string AppDomainCreateUnloadMessage = "Creating and unloading AppDomains is not supported and throws an exception.";

        internal const string AppDomainCreateUnloadDiagId = "SYSLIB0024";

        internal const string SuppressIldasmAttributeMessage = "SuppressIldasmAttribute has no effect in .NET 6.0+.";

        internal const string SuppressIldasmAttributeDiagId = "SYSLIB0025";

        internal const string X509CertificateImmutableMessage = "X509Certificate and X509Certificate2 are immutable. Use the appropriate constructor to create a new certificate.";

        internal const string X509CertificateImmutableDiagId = "SYSLIB0026";

        internal const string PublicKeyPropertyMessage = "PublicKey.Key is obsolete. Use the appropriate method to get the public key, such as GetRSAPublicKey.";

        internal const string PublicKeyPropertyDiagId = "SYSLIB0027";

        internal const string X509CertificatePrivateKeyMessage = "X509Certificate2.PrivateKey is obsolete. Use the appropriate method to get the private key, such as GetRSAPrivateKey, or use the CopyWithPrivateKey method to create a new instance with a private key.";

        internal const string X509CertificatePrivateKeyDiagId = "SYSLIB0028";

        internal const string ProduceLegacyHmacValuesMessage = "ProduceLegacyHmacValues is obsolete. Producing legacy HMAC values is not supported.";

        internal const string ProduceLegacyHmacValuesDiagId = "SYSLIB0029";

        internal const string UseManagedSha1Message = "HMACSHA1 always uses the algorithm implementation provided by the platform. Use a constructor without the useManagedSha1 parameter.";

        internal const string UseManagedSha1DiagId = "SYSLIB0030";

        internal const string CryptoConfigEncodeOIDMessage = "EncodeOID is obsolete. Use the ASN.1 functionality provided in System.Formats.Asn1.";

        internal const string CryptoConfigEncodeOIDDiagId = "SYSLIB0031";

        internal const string CorruptedStateRecoveryMessage = "Recovery from corrupted process state exceptions is not supported; HandleProcessCorruptedStateExceptionsAttribute is ignored.";

        internal const string CorruptedStateRecoveryDiagId = "SYSLIB0032";

        internal const string Rfc2898CryptDeriveKeyMessage = "Rfc2898DeriveBytes.CryptDeriveKey is obsolete and is not supported. Use PasswordDeriveBytes.CryptDeriveKey instead.";

        internal const string Rfc2898CryptDeriveKeyDiagId = "SYSLIB0033";

        internal const string CmsSignerCspParamsCtorMessage = "CmsSigner(CspParameters) is obsolete and is not supported. Use an alternative constructor instead.";

        internal const string CmsSignerCspParamsCtorDiagId = "SYSLIB0034";

        internal const string SignerInfoCounterSigMessage = "ComputeCounterSignature without specifying a CmsSigner is obsolete and is not supported. Use the overload that accepts a CmsSigner.";

        internal const string SignerInfoCounterSigDiagId = "SYSLIB0035";

        internal const string RegexCompileToAssemblyMessage = "Regex.CompileToAssembly is obsolete and not supported. Use the GeneratedRegexAttribute with the regular expression source generator instead.";

        internal const string RegexCompileToAssemblyDiagId = "SYSLIB0036";

        internal const string AssemblyNameMembersMessage = "AssemblyName members HashAlgorithm, ProcessorArchitecture, and VersionCompatibility are obsolete and not supported.";

        internal const string AssemblyNameMembersDiagId = "SYSLIB0037";

        internal const string SystemDataSerializationFormatBinaryMessage = "SerializationFormat.Binary is obsolete and should not be used. See https://aka.ms/serializationformat-binary-obsolete for more information.";

        internal const string SystemDataSerializationFormatBinaryDiagId = "SYSLIB0038";

        internal const string TlsVersion10and11Message = "TLS versions 1.0 and 1.1 have known vulnerabilities and are not recommended. Use a newer TLS version instead, or use SslProtocols.None to defer to OS defaults.";

        internal const string TlsVersion10and11DiagId = "SYSLIB0039";

        internal const string EncryptionPolicyMessage = "EncryptionPolicy.NoEncryption and AllowEncryption significantly reduce security and should not be used in production code.";

        internal const string EncryptionPolicyDiagId = "SYSLIB0040";

        internal const string Rfc2898OutdatedCtorMessage = "The default hash algorithm and iteration counts in Rfc2898DeriveBytes constructors are outdated and insecure. Use a constructor that accepts the hash algorithm and the number of iterations.";

        internal const string Rfc2898OutdatedCtorDiagId = "SYSLIB0041";

        internal const string EccXmlExportImportMessage = "ToXmlString and FromXmlString have no implementation for ECC types, and are obsolete. Use a standard import and export format such as ExportSubjectPublicKeyInfo or ImportSubjectPublicKeyInfo for public keys and ExportPkcs8PrivateKey or ImportPkcs8PrivateKey for private keys.";

        internal const string EccXmlExportImportDiagId = "SYSLIB0042";

        internal const string EcDhPublicKeyBlobMessage = "ECDiffieHellmanPublicKey.ToByteArray() and the associated constructor do not have a consistent and interoperable implementation on all platforms. Use ECDiffieHellmanPublicKey.ExportSubjectPublicKeyInfo() instead.";

        internal const string EcDhPublicKeyBlobDiagId = "SYSLIB0043";

        internal const string AssemblyNameCodeBaseMessage = "AssemblyName.CodeBase and AssemblyName.EscapedCodeBase are obsolete. Using them for loading an assembly is not supported.";

        internal const string AssemblyNameCodeBaseDiagId = "SYSLIB0044";

        internal const string CryptoStringFactoryMessage = "Cryptographic factory methods accepting an algorithm name are obsolete. Use the parameterless Create factory method on the algorithm type instead.";

        internal const string CryptoStringFactoryDiagId = "SYSLIB0045";

        internal const string ControlledExecutionRunMessage = "ControlledExecution.Run method may corrupt the process and should not be used in production code.";

        internal const string ControlledExecutionRunDiagId = "SYSLIB0046";

        internal const string XmlSecureResolverMessage = "XmlSecureResolver is obsolete. Use XmlResolver.ThrowingResolver instead when attempting to forbid XML external entity resolution.";

        internal const string XmlSecureResolverDiagId = "SYSLIB0047";

        internal const string RsaEncryptDecryptValueMessage = "RSA.EncryptValue and DecryptValue are not supported and throw NotSupportedException. Use RSA.Encrypt and RSA.Decrypt instead.";

        internal const string RsaEncryptDecryptDiagId = "SYSLIB0048";

        internal const string JsonSerializerOptionsAddContextMessage = "JsonSerializerOptions.AddContext is obsolete. To register a JsonSerializerContext, use either the TypeInfoResolver or TypeInfoResolverChain properties.";

        internal const string JsonSerializerOptionsAddContextDiagId = "SYSLIB0049";

        internal const string LegacyFormatterMessage = "Formatter-based serialization is obsolete and should not be used.";

        internal const string LegacyFormatterDiagId = "SYSLIB0050";

        internal const string LegacyFormatterImplMessage = "This API supports obsolete formatter-based serialization. It should not be called or extended by application code.";

        internal const string LegacyFormatterImplDiagId = "SYSLIB0051";

        internal const string RegexExtensibilityImplMessage = "This API supports obsolete mechanisms for Regex extensibility. It is not supported.";

        internal const string RegexExtensibilityDiagId = "SYSLIB0052";

        internal const string AesGcmTagConstructorMessage = "AesGcm should indicate the required tag size for encryption and decryption. Use a constructor that accepts the tag size.";

        internal const string AesGcmTagConstructorDiagId = "SYSLIB0053";
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal readonly struct VoidResult { }

    namespace Collections.Generic
    {
        [DebuggerDisplay("Count = {_size}")]
        internal sealed class Deque<T>
        {
            private T[] _array = Array.Empty<T>();

            private int _head;

            private int _tail;

            private int _size;

            public int Count => _size;

            public bool IsEmpty => _size == 0;

            public void EnqueueTail(T item)
            {
                if (_size == _array.Length)
                {
                    Grow();
                }
                _array[_tail] = item;
                if (++_tail == _array.Length)
                {
                    _tail = 0;
                }
                _size++;
            }

            public T DequeueHead()
            {
                T result = _array[_head];
                _array[_head] = default(T);
                if (++_head == _array.Length)
                {
                    _head = 0;
                }
                _size--;
                return result;
            }

            public T PeekHead()
            {
                return _array[_head];
            }

            public T PeekTail()
            {
                int num = _tail - 1;
                if (num == -1)
                {
                    num = _array.Length - 1;
                }
                return _array[num];
            }

            public T DequeueTail()
            {
                if (--_tail == -1)
                {
                    _tail = _array.Length - 1;
                }
                T result = _array[_tail];
                _array[_tail] = default(T);
                _size--;
                return result;
            }

            public IEnumerator<T> GetEnumerator()
            {
                int pos = _head;
                int count = _size;
                while (count-- > 0)
                {
                    yield return _array[pos];
                    pos = (pos + 1) % _array.Length;
                }
            }

            private void Grow()
            {
                int num = (int)((long)_array.Length * 2L);
                if (num < _array.Length + 4)
                {
                    num = _array.Length + 4;
                }
                T[] array = new T[num];
                if (_head == 0)
                {
                    Array.Copy(_array, array, _size);
                }
                else
                {
                    Array.Copy(_array, _head, array, 0, _array.Length - _head);
                    Array.Copy(_array, 0, array, _array.Length - _head, _tail);
                }
                _array = array;
                _head = 0;
                _tail = _size;
            }
        }
    }

    namespace Threading.Channels
    {
        using System.Threading.Tasks;
        using System.Collections.Generic;
        using System.Collections.Concurrent;
        using System.Threading.Tasks.Sources;
        using System.Runtime.ExceptionServices;

        internal abstract class AsyncOperation
        {
            protected static readonly Action<object> s_availableSentinel = AvailableSentinel;

            protected static readonly Action<object> s_completedSentinel = CompletedSentinel;

            private static void AvailableSentinel(object s) { }

            private static void CompletedSentinel(object s) { }

            protected static void ThrowIncompleteOperationException()
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IncompleteAsyncOperation);
            }

            protected static void ThrowMultipleContinuations()
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_MultipleContinuations);
            }

            protected static void ThrowIncorrectCurrentIdException()
            {
                throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IncorrectToken);
            }
        }
        
        internal class AsyncOperation<TResult> : AsyncOperation, IValueTaskSource, IValueTaskSource<TResult>
        {
            private readonly CancellationTokenRegistration _registration;

            private readonly bool _pooled;

            private readonly bool _runContinuationsAsynchronously;

            private volatile int _completionReserved;

            private TResult _result;

            private ExceptionDispatchInfo _error;

            private Action<object> _continuation;

            private object _continuationState;

            private object _schedulingContext;

            private ExecutionContext _executionContext;

            private short _currentId;

            public AsyncOperation<TResult> Next { get; set; }

            public CancellationToken CancellationToken { get; }

            public ValueTask ValueTask => new ValueTask(this, _currentId);

            public ValueTask<TResult> ValueTaskOfT => new ValueTask<TResult>(this, _currentId);

            internal bool IsCompleted => (object)_continuation == AsyncOperation.s_completedSentinel;

            public AsyncOperation(bool runContinuationsAsynchronously, CancellationToken cancellationToken = default(CancellationToken), bool pooled = false)
            {
                _continuation = (pooled ? AsyncOperation.s_availableSentinel : null);
                _pooled = pooled;
                _runContinuationsAsynchronously = runContinuationsAsynchronously;
                if (cancellationToken.CanBeCanceled)
                {
                    CancellationToken = cancellationToken;
                    _registration = UnsafeRegister(cancellationToken, delegate (object s)
                    {
                        AsyncOperation<TResult> asyncOperation = (AsyncOperation<TResult>)s;
                        asyncOperation.TrySetCanceled(asyncOperation.CancellationToken);
                    }, this);
                }
            }

            public ValueTaskSourceStatus GetStatus(short token)
            {
                if (_currentId != token)
                {
                    AsyncOperation.ThrowIncorrectCurrentIdException();
                }
                if (IsCompleted)
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

            public TResult GetResult(short token)
            {
                if (_currentId != token)
                {
                    AsyncOperation.ThrowIncorrectCurrentIdException();
                }
                if (!IsCompleted)
                {
                    AsyncOperation.ThrowIncompleteOperationException();
                }
                ExceptionDispatchInfo error = _error;
                TResult result = _result;
                _currentId++;
                if (_pooled)
                {
                    Volatile.Write(ref _continuation, AsyncOperation.s_availableSentinel);
                }
                error?.Throw();
                return result;
            }

            void IValueTaskSource.GetResult(short token)
            {
                if (_currentId != token)
                {
                    AsyncOperation.ThrowIncorrectCurrentIdException();
                }
                if (!IsCompleted)
                {
                    AsyncOperation.ThrowIncompleteOperationException();
                }
                ExceptionDispatchInfo error = _error;
                _currentId++;
                if (_pooled)
                {
                    Volatile.Write(ref _continuation, AsyncOperation.s_availableSentinel);
                }
                error?.Throw();
            }

            public bool TryOwnAndReset()
            {
                if ((object)Interlocked.CompareExchange(ref _continuation, null, AsyncOperation.s_availableSentinel) == AsyncOperation.s_availableSentinel)
                {
                    _continuationState = null;
                    _result = default(TResult);
                    _error = null;
                    _schedulingContext = null;
                    _executionContext = null;
                    return true;
                }
                return false;
            }

            public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
            {
                if (_currentId != token)
                {
                    AsyncOperation.ThrowIncorrectCurrentIdException();
                }
                if (_continuationState != null)
                {
                    AsyncOperation.ThrowMultipleContinuations();
                }
                _continuationState = state;
                if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
                {
                    _executionContext = ExecutionContext.Capture();
                }
                SynchronizationContext synchronizationContext = null;
                TaskScheduler taskScheduler = null;
                if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
                {
                    synchronizationContext = SynchronizationContext.Current;
                    if (synchronizationContext != null && synchronizationContext.GetType() != typeof(SynchronizationContext))
                    {
                        _schedulingContext = synchronizationContext;
                    }
                    else
                    {
                        synchronizationContext = null;
                        taskScheduler = TaskScheduler.Current;
                        if (taskScheduler != TaskScheduler.Default)
                        {
                            _schedulingContext = taskScheduler;
                        }
                    }
                }
                Action<object> action = Interlocked.CompareExchange(ref _continuation, continuation, null);
                if (action == null)
                {
                    return;
                }
                if ((object)action != AsyncOperation.s_completedSentinel)
                {
                    AsyncOperation.ThrowMultipleContinuations();
                }
                if (_schedulingContext == null)
                {
                    if (_executionContext == null)
                    {
                        UnsafeQueueUserWorkItem(continuation, state);
                    }
                    else
                    {
                        QueueUserWorkItem(continuation, state);
                    }
                }
                else if (synchronizationContext != null)
                {
                    synchronizationContext.Post(delegate (object s)
                    {
                        KeyValuePair<Action<object>, object> keyValuePair = (KeyValuePair<Action<object>, object>)s;
                        keyValuePair.Key(keyValuePair.Value);
                    }, new KeyValuePair<Action<object>, object>(continuation, state));
                }
                else
                {
                    Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, taskScheduler);
                }
            }

            public bool UnregisterCancellation()
            {
                if (CancellationToken.CanBeCanceled)
                {
                    _registration.Dispose();
                    return _completionReserved == 0;
                }
                return true;
            }

            public bool TrySetResult(TResult item)
            {
                UnregisterCancellation();
                if (TryReserveCompletionIfCancelable())
                {
                    _result = item;
                    SignalCompletion();
                    return true;
                }
                return false;
            }

            public bool TrySetException(Exception exception)
            {
                UnregisterCancellation();
                if (TryReserveCompletionIfCancelable())
                {
                    _error = ExceptionDispatchInfo.Capture(exception);
                    SignalCompletion();
                    return true;
                }
                return false;
            }

            public bool TrySetCanceled(CancellationToken cancellationToken = default(CancellationToken))
            {
                if (TryReserveCompletionIfCancelable())
                {
                    _error = ExceptionDispatchInfo.Capture(new OperationCanceledException(cancellationToken));
                    SignalCompletion();
                    return true;
                }
                return false;
            }

            private bool TryReserveCompletionIfCancelable()
            {
                if (CancellationToken.CanBeCanceled)
                {
                    return Interlocked.CompareExchange(ref _completionReserved, 1, 0) == 0;
                }
                return true;
            }

            private void SignalCompletion()
            {
                if (_continuation == null && Interlocked.CompareExchange(ref _continuation, AsyncOperation.s_completedSentinel, null) == null)
                {
                    return;
                }
                if (_schedulingContext == null)
                {
                    if (_runContinuationsAsynchronously)
                    {
                        UnsafeQueueSetCompletionAndInvokeContinuation();
                        return;
                    }
                }
                else if (_schedulingContext is SynchronizationContext synchronizationContext)
                {
                    if (_runContinuationsAsynchronously || synchronizationContext != SynchronizationContext.Current)
                    {
                        synchronizationContext.Post(delegate (object s)
                        {
                            ((AsyncOperation<TResult>)s).SetCompletionAndInvokeContinuation();
                        }, this);
                        return;
                    }
                }
                else
                {
                    TaskScheduler taskScheduler = (TaskScheduler)_schedulingContext;
                    if (_runContinuationsAsynchronously || taskScheduler != TaskScheduler.Current)
                    {
                        Task.Factory.StartNew(delegate (object s)
                        {
                            ((AsyncOperation<TResult>)s).SetCompletionAndInvokeContinuation();
                        }, this, CancellationToken.None, TaskCreationOptions.DenyChildAttach, taskScheduler);
                        return;
                    }
                }
                SetCompletionAndInvokeContinuation();
            }

            private void SetCompletionAndInvokeContinuation()
            {
                if (_executionContext == null)
                {
                    Action<object> continuation = _continuation;
                    _continuation = AsyncOperation.s_completedSentinel;
                    continuation(_continuationState);
                    return;
                }
                ExecutionContext.Run(_executionContext, delegate (object s)
                {
                    AsyncOperation<TResult> asyncOperation = (AsyncOperation<TResult>)s;
                    Action<object> continuation2 = asyncOperation._continuation;
                    asyncOperation._continuation = AsyncOperation.s_completedSentinel;
                    continuation2(asyncOperation._continuationState);
                }, this);
            }

            private void UnsafeQueueSetCompletionAndInvokeContinuation()
            {
                ThreadPool.UnsafeQueueUserWorkItem(delegate (object s)
                {
                    ((AsyncOperation<TResult>)s).SetCompletionAndInvokeContinuation();
                }, this);
            }

            private static void UnsafeQueueUserWorkItem(Action<object> action, object state)
            {
                QueueUserWorkItem(action, state);
            }

            private static void QueueUserWorkItem(Action<object> action, object state)
            {
                Task.Factory.StartNew(action, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }

            private static CancellationTokenRegistration UnsafeRegister(CancellationToken cancellationToken, Action<object> action, object state)
            {
                return cancellationToken.Register(action, state);
            }
        }

        [DebuggerDisplay("Items = {ItemsCountForDebugger}, Capacity = {_bufferedCapacity}, Mode = {_mode}, Closed = {ChannelIsClosedForDebugger}")]
        [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
        internal sealed class BoundedChannel<T> : Channel<T>, IDebugEnumerable<T>
        {
            [DebuggerDisplay("Items = {ItemsCountForDebugger}")]
            [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
            private sealed class BoundedChannelReader : ChannelReader<T>, IDebugEnumerable<T>
            {
                internal readonly BoundedChannel<T> _parent;

                private readonly AsyncOperation<T> _readerSingleton;

                private readonly AsyncOperation<bool> _waiterSingleton;

                public override Task Completion => _parent._completion.Task;

                public override bool CanCount => true;

                public override bool CanPeek => true;

                public override int Count
                {
                    get
                    {
                        BoundedChannel<T> parent = _parent;
                        lock (parent.SyncObj)
                        {
                            return parent._items.Count;
                        }
                    }
                }

                private int ItemsCountForDebugger => _parent._items.Count;

                internal BoundedChannelReader(BoundedChannel<T> parent)
                {
                    _parent = parent;
                    _readerSingleton = new AsyncOperation<T>(parent._runContinuationsAsynchronously, default(CancellationToken), pooled: true);
                    _waiterSingleton = new AsyncOperation<bool>(parent._runContinuationsAsynchronously, default(CancellationToken), pooled: true);
                }

                public override bool TryRead([MaybeNullWhen(false)] out T item)
                {
                    BoundedChannel<T> parent = _parent;
                    lock (parent.SyncObj)
                    {
                        if (!parent._items.IsEmpty)
                        {
                            item = DequeueItemAndPostProcess();
                            return true;
                        }
                    }
                    item = default(T);
                    return false;
                }

                public override bool TryPeek([MaybeNullWhen(false)] out T item)
                {
                    BoundedChannel<T> parent = _parent;
                    lock (parent.SyncObj)
                    {
                        if (!parent._items.IsEmpty)
                        {
                            item = parent._items.PeekHead();
                            return true;
                        }
                    }
                    item = default(T);
                    return false;
                }

                public override ValueTask<T> ReadAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask<T>(Task.FromCanceled<T>(cancellationToken));
                    }
                    BoundedChannel<T> parent = _parent;
                    lock (parent.SyncObj)
                    {
                        if (!parent._items.IsEmpty)
                        {
                            return new ValueTask<T>(DequeueItemAndPostProcess());
                        }
                        if (parent._doneWriting != null)
                        {
                            return ChannelUtilities.GetInvalidCompletionValueTask<T>(parent._doneWriting);
                        }
                        if (!cancellationToken.CanBeCanceled)
                        {
                            AsyncOperation<T> readerSingleton = _readerSingleton;
                            if (readerSingleton.TryOwnAndReset())
                            {
                                parent._blockedReaders.EnqueueTail(readerSingleton);
                                return readerSingleton.ValueTaskOfT;
                            }
                        }
                        AsyncOperation<T> asyncOperation = new AsyncOperation<T>(parent._runContinuationsAsynchronously | cancellationToken.CanBeCanceled, cancellationToken);
                        parent._blockedReaders.EnqueueTail(asyncOperation);
                        return asyncOperation.ValueTaskOfT;
                    }
                }

                public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken));
                    }
                    BoundedChannel<T> parent = _parent;
                    lock (parent.SyncObj)
                    {
                        if (!parent._items.IsEmpty)
                        {
                            return new ValueTask<bool>(result: true);
                        }
                        if (parent._doneWriting != null)
                        {
                            return (parent._doneWriting != ChannelUtilities.s_doneWritingSentinel) ? new ValueTask<bool>(Task.FromException<bool>(parent._doneWriting)) : default(ValueTask<bool>);
                        }
                        if (!cancellationToken.CanBeCanceled)
                        {
                            AsyncOperation<bool> waiterSingleton = _waiterSingleton;
                            if (waiterSingleton.TryOwnAndReset())
                            {
                                ChannelUtilities.QueueWaiter(ref parent._waitingReadersTail, waiterSingleton);
                                return waiterSingleton.ValueTaskOfT;
                            }
                        }
                        AsyncOperation<bool> asyncOperation = new AsyncOperation<bool>(parent._runContinuationsAsynchronously | cancellationToken.CanBeCanceled, cancellationToken);
                        ChannelUtilities.QueueWaiter(ref _parent._waitingReadersTail, asyncOperation);
                        return asyncOperation.ValueTaskOfT;
                    }
                }

                private T DequeueItemAndPostProcess()
                {
                    BoundedChannel<T> parent = _parent;
                    T result = parent._items.DequeueHead();
                    if (parent._doneWriting != null)
                    {
                        if (parent._items.IsEmpty)
                        {
                            ChannelUtilities.Complete(parent._completion, parent._doneWriting);
                        }
                    }
                    else
                    {
                        while (!parent._blockedWriters.IsEmpty)
                        {
                            VoidAsyncOperationWithData<T> voidAsyncOperationWithData = parent._blockedWriters.DequeueHead();
                            if (voidAsyncOperationWithData.TrySetResult(default(VoidResult)))
                            {
                                parent._items.EnqueueTail(voidAsyncOperationWithData.Item);
                                return result;
                            }
                        }
                        ChannelUtilities.WakeUpWaiters(ref parent._waitingWritersTail, result: true);
                    }
                    return result;
                }

                IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
                {
                    return _parent._items.GetEnumerator();
                }
            }

            [DebuggerDisplay("Items = {ItemsCountForDebugger}, Capacity = {CapacityForDebugger}")]
            [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
            private sealed class BoundedChannelWriter : ChannelWriter<T>, IDebugEnumerable<T>
            {
                internal readonly BoundedChannel<T> _parent;

                private readonly VoidAsyncOperationWithData<T> _writerSingleton;

                private readonly AsyncOperation<bool> _waiterSingleton;

                private int ItemsCountForDebugger => _parent._items.Count;

                private int CapacityForDebugger => _parent._bufferedCapacity;

                internal BoundedChannelWriter(BoundedChannel<T> parent)
                {
                    _parent = parent;
                    _writerSingleton = new VoidAsyncOperationWithData<T>(runContinuationsAsynchronously: true, default(CancellationToken), pooled: true);
                    _waiterSingleton = new AsyncOperation<bool>(runContinuationsAsynchronously: true, default(CancellationToken), pooled: true);
                }

                public override bool TryComplete(Exception error)
                {
                    BoundedChannel<T> parent = _parent;
                    bool isEmpty;
                    lock (parent.SyncObj)
                    {
                        if (parent._doneWriting != null)
                        {
                            return false;
                        }
                        parent._doneWriting = error ?? ChannelUtilities.s_doneWritingSentinel;
                        isEmpty = parent._items.IsEmpty;
                    }
                    if (isEmpty)
                    {
                        ChannelUtilities.Complete(parent._completion, error);
                    }
                    ChannelUtilities.FailOperations<AsyncOperation<T>, T>(parent._blockedReaders, ChannelUtilities.CreateInvalidCompletionException(error));
                    ChannelUtilities.FailOperations<VoidAsyncOperationWithData<T>, VoidResult>(parent._blockedWriters, ChannelUtilities.CreateInvalidCompletionException(error));
                    ChannelUtilities.WakeUpWaiters(ref parent._waitingReadersTail, result: false, error);
                    ChannelUtilities.WakeUpWaiters(ref parent._waitingWritersTail, result: false, error);
                    return true;
                }

                public override bool TryWrite(T item)
                {
                    AsyncOperation<T> asyncOperation = null;
                    AsyncOperation<bool> listTail = null;
                    BoundedChannel<T> parent = _parent;
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter(parent.SyncObj, ref lockTaken);
                        if (parent._doneWriting != null)
                        {
                            return false;
                        }
                        int count = parent._items.Count;
                        if (count != 0)
                        {
                            if (count < parent._bufferedCapacity)
                            {
                                parent._items.EnqueueTail(item);
                                return true;
                            }
                            if (parent._mode == BoundedChannelFullMode.Wait)
                            {
                                return false;
                            }
                            if (parent._mode == BoundedChannelFullMode.DropWrite)
                            {
                                Monitor.Exit(parent.SyncObj);
                                lockTaken = false;
                                parent._itemDropped?.Invoke(item);
                                return true;
                            }
                            T obj = ((parent._mode == BoundedChannelFullMode.DropNewest) ? parent._items.DequeueTail() : parent._items.DequeueHead());
                            parent._items.EnqueueTail(item);
                            Monitor.Exit(parent.SyncObj);
                            lockTaken = false;
                            parent._itemDropped?.Invoke(obj);
                            return true;
                        }
                        while (!parent._blockedReaders.IsEmpty)
                        {
                            AsyncOperation<T> asyncOperation2 = parent._blockedReaders.DequeueHead();
                            if (asyncOperation2.UnregisterCancellation())
                            {
                                asyncOperation = asyncOperation2;
                                break;
                            }
                        }
                        if (asyncOperation == null)
                        {
                            parent._items.EnqueueTail(item);
                            listTail = parent._waitingReadersTail;
                            if (listTail == null)
                            {
                                return true;
                            }
                            parent._waitingReadersTail = null;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(parent.SyncObj);
                        }
                    }
                    if (asyncOperation != null)
                    {
                        bool flag = asyncOperation.TrySetResult(item);
                    }
                    else
                    {
                        ChannelUtilities.WakeUpWaiters(ref listTail, result: true);
                    }
                    return true;
                }

                public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken));
                    }
                    BoundedChannel<T> parent = _parent;
                    lock (parent.SyncObj)
                    {
                        if (parent._doneWriting != null)
                        {
                            return (parent._doneWriting != ChannelUtilities.s_doneWritingSentinel) ? new ValueTask<bool>(Task.FromException<bool>(parent._doneWriting)) : default(ValueTask<bool>);
                        }
                        if (parent._items.Count < parent._bufferedCapacity || parent._mode != 0)
                        {
                            return new ValueTask<bool>(result: true);
                        }
                        if (!cancellationToken.CanBeCanceled)
                        {
                            AsyncOperation<bool> waiterSingleton = _waiterSingleton;
                            if (waiterSingleton.TryOwnAndReset())
                            {
                                ChannelUtilities.QueueWaiter(ref parent._waitingWritersTail, waiterSingleton);
                                return waiterSingleton.ValueTaskOfT;
                            }
                        }
                        AsyncOperation<bool> asyncOperation = new AsyncOperation<bool>(runContinuationsAsynchronously: true, cancellationToken);
                        ChannelUtilities.QueueWaiter(ref parent._waitingWritersTail, asyncOperation);
                        return asyncOperation.ValueTaskOfT;
                    }
                }

                public override ValueTask WriteAsync(T item, CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask(Task.FromCanceled(cancellationToken));
                    }
                    AsyncOperation<T> asyncOperation = null;
                    AsyncOperation<bool> listTail = null;
                    BoundedChannel<T> parent = _parent;
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter(parent.SyncObj, ref lockTaken);
                        if (parent._doneWriting != null)
                        {
                            return new ValueTask(Task.FromException(ChannelUtilities.CreateInvalidCompletionException(parent._doneWriting)));
                        }
                        int count = parent._items.Count;
                        if (count != 0)
                        {
                            if (count < parent._bufferedCapacity)
                            {
                                parent._items.EnqueueTail(item);
                                return default(ValueTask);
                            }
                            if (parent._mode == BoundedChannelFullMode.Wait)
                            {
                                if (!cancellationToken.CanBeCanceled)
                                {
                                    VoidAsyncOperationWithData<T> writerSingleton = _writerSingleton;
                                    if (writerSingleton.TryOwnAndReset())
                                    {
                                        writerSingleton.Item = item;
                                        parent._blockedWriters.EnqueueTail(writerSingleton);
                                        return writerSingleton.ValueTask;
                                    }
                                }
                                VoidAsyncOperationWithData<T> voidAsyncOperationWithData = new VoidAsyncOperationWithData<T>(runContinuationsAsynchronously: true, cancellationToken);
                                voidAsyncOperationWithData.Item = item;
                                parent._blockedWriters.EnqueueTail(voidAsyncOperationWithData);
                                return voidAsyncOperationWithData.ValueTask;
                            }
                            if (parent._mode == BoundedChannelFullMode.DropWrite)
                            {
                                Monitor.Exit(parent.SyncObj);
                                lockTaken = false;
                                parent._itemDropped?.Invoke(item);
                                return default(ValueTask);
                            }
                            T obj = ((parent._mode == BoundedChannelFullMode.DropNewest) ? parent._items.DequeueTail() : parent._items.DequeueHead());
                            parent._items.EnqueueTail(item);
                            Monitor.Exit(parent.SyncObj);
                            lockTaken = false;
                            parent._itemDropped?.Invoke(obj);
                            return default(ValueTask);
                        }
                        while (!parent._blockedReaders.IsEmpty)
                        {
                            AsyncOperation<T> asyncOperation2 = parent._blockedReaders.DequeueHead();
                            if (asyncOperation2.UnregisterCancellation())
                            {
                                asyncOperation = asyncOperation2;
                                break;
                            }
                        }
                        if (asyncOperation == null)
                        {
                            parent._items.EnqueueTail(item);
                            listTail = parent._waitingReadersTail;
                            if (listTail == null)
                            {
                                return default(ValueTask);
                            }
                            parent._waitingReadersTail = null;
                        }
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(parent.SyncObj);
                        }
                    }
                    if (asyncOperation != null)
                    {
                        bool flag = asyncOperation.TrySetResult(item);
                    }
                    else
                    {
                        ChannelUtilities.WakeUpWaiters(ref listTail, result: true);
                    }
                    return default(ValueTask);
                }

                IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
                {
                    return _parent._items.GetEnumerator();
                }
            }

            private readonly BoundedChannelFullMode _mode;

            private readonly Action<T> _itemDropped;

            private readonly TaskCompletionSource _completion;

            private readonly int _bufferedCapacity;

            private readonly Deque<T> _items = new Deque<T>();

            private readonly Deque<AsyncOperation<T>> _blockedReaders = new Deque<AsyncOperation<T>>();

            private readonly Deque<VoidAsyncOperationWithData<T>> _blockedWriters = new Deque<VoidAsyncOperationWithData<T>>();

            private AsyncOperation<bool> _waitingReadersTail;

            private AsyncOperation<bool> _waitingWritersTail;

            private readonly bool _runContinuationsAsynchronously;

            private Exception _doneWriting;

            private object SyncObj => _items;

            private int ItemsCountForDebugger => _items.Count;

            private bool ChannelIsClosedForDebugger => _doneWriting != null;

            internal BoundedChannel(int bufferedCapacity, BoundedChannelFullMode mode, bool runContinuationsAsynchronously, Action<T> itemDropped)
            {
                _bufferedCapacity = bufferedCapacity;
                _mode = mode;
                _runContinuationsAsynchronously = runContinuationsAsynchronously;
                _itemDropped = itemDropped;
                _completion = new TaskCompletionSource(runContinuationsAsynchronously ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None);
                base.Reader = new BoundedChannelReader(this);
                base.Writer = new BoundedChannelWriter(this);
            }

            [Conditional("DEBUG")]
            private void AssertInvariants()
            {
                _ = _items.IsEmpty;
                _ = _items.Count;
                _ = _bufferedCapacity;
                _ = _blockedReaders.IsEmpty;
                _ = _blockedWriters.IsEmpty;
                _ = _completion.Task.IsCompleted;
            }

            IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }

        /// <summary>Specifies the behavior to use when writing to a bounded channel that is already full.</summary>
        public enum BoundedChannelFullMode
        {
            /// <summary>Waits for space to be available in order to complete the write operation.</summary>
            Wait,
            /// <summary>Removes and ignores the newest item in the channel in order to make room for the item being written.</summary>
            DropNewest,
            /// <summary>Removes and ignores the oldest item in the channel in order to make room for the item being written.</summary>
            DropOldest,
            /// <summary>Drops the item being written.</summary>
            DropWrite
        }

        /// <summary>Provides options that control the behavior of bounded <see cref="System.Threading.Channels.Channel{T}" /> instances.</summary>
        public sealed class BoundedChannelOptions : ChannelOptions
        {
            private int _capacity;

            private BoundedChannelFullMode _mode;

            /// <summary>Gets or sets the maximum number of items the bounded channel may store.</summary>
            public int Capacity
            {
                get
                {
                    return _capacity;
                }
                set
                {
                    if (value < 1)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    _capacity = value;
                }
            }

            /// <summary>Gets or sets the behavior incurred by write operations when the channel is full.</summary>
            public BoundedChannelFullMode FullMode
            {
                get
                {
                    return _mode;
                }
                set
                {
                    if ((uint)value <= 3u)
                    {
                        _mode = value;
                        return;
                    }
                    throw new ArgumentOutOfRangeException("value");
                }
            }

            /// <summary>Initializes the options.</summary>
            /// <param name="capacity">The maximum number of items the bounded channel may store.</param>
            public BoundedChannelOptions(int capacity)
            {
                if (capacity < 1)
                {
                    throw new ArgumentOutOfRangeException("capacity");
                }
                _capacity = capacity;
            }
        }

        /// <summary>Provides static methods for creating channels.</summary>
        public static class Channel
        {
            /// <summary>Creates an unbounded channel usable by any number of readers and writers concurrently.</summary>
            /// <typeparam name="T">The type of data in the channel.</typeparam>
            /// <returns>The created channel.</returns>
            public static Channel<T> CreateUnbounded<T>()
            {
                return new UnboundedChannel<T>(runContinuationsAsynchronously: true);
            }

            /// <summary>Creates an unbounded channel subject to the provided options.</summary>
            /// <param name="options">Options that guide the behavior of the channel.</param>
            /// <typeparam name="T">Specifies the type of data in the channel.</typeparam>
            /// <returns>The created channel.</returns>
            public static Channel<T> CreateUnbounded<T>(UnboundedChannelOptions options)
            {
                if (options == null)
                {
                    throw new ArgumentNullException("options");
                }
                if (options.SingleReader)
                {
                    return new SingleConsumerUnboundedChannel<T>(!options.AllowSynchronousContinuations);
                }
                return new UnboundedChannel<T>(!options.AllowSynchronousContinuations);
            }

            /// <summary>Creates a channel with the specified maximum capacity.</summary>
            /// <param name="capacity">The maximum number of items the channel may store.</param>
            /// <typeparam name="T">Specifies the type of data in the channel.</typeparam>
            /// <returns>The created channel.</returns>
            public static Channel<T> CreateBounded<T>(int capacity)
            {
                if (capacity < 1)
                {
                    throw new ArgumentOutOfRangeException("capacity");
                }
                return new BoundedChannel<T>(capacity, BoundedChannelFullMode.Wait, runContinuationsAsynchronously: true, null);
            }

            /// <summary>Creates a channel with the specified maximum capacity.</summary>
            /// <param name="options">Options that guide the behavior of the channel.</param>
            /// <typeparam name="T">Specifies the type of data in the channel.</typeparam>
            /// <returns>The created channel.</returns>
            public static Channel<T> CreateBounded<T>(BoundedChannelOptions options)
            {
                return CreateBounded<T>(options, null);
            }

            #nullable enable
            /// <summary>Creates a channel subject to the provided options.</summary>
            /// <param name="options">Options that guide the behavior of the channel.</param>
            /// <param name="itemDropped">Delegate that will be called when item is being dropped from channel. See <see cref="T:System.Threading.Channels.BoundedChannelFullMode" />.</param>
            /// <typeparam name="T">Specifies the type of data in the channel.</typeparam>
            /// <returns>The created channel.</returns>
            public static Channel<T> CreateBounded<T>(BoundedChannelOptions options, Action<T>? itemDropped)
            {
                if (options == null)
                {
                    throw new ArgumentNullException("options");
                }
                return new BoundedChannel<T>(options.Capacity, options.FullMode, !options.AllowSynchronousContinuations, itemDropped);
            }
            #nullable disable
        }

        /// <summary>Provides a base class for channels that support reading and writing elements of type <typeparamref name="T" />.</summary>
        /// <typeparam name="T">Specifies the type of data readable and writable in the channel.</typeparam>
        public abstract class Channel<T> : Channel<T, T>
        {
            /// <summary>Initializes an instance of the <see cref="System.Threading.Channels.Channel{T}" /> class.</summary>
            protected Channel() { }
        }

        /// <summary>Provides a base class for channels that support reading elements of type <typeparamref name="TRead" /> and writing elements of type <typeparamref name="TWrite" />.</summary>
        /// <typeparam name="TWrite">Specifies the type of data that may be written to the channel.</typeparam>
        /// <typeparam name="TRead">Specifies the type of data that may be read from the channel.</typeparam>
        public abstract class Channel<TWrite, TRead>
        {
            /// <summary>Gets the readable half of this channel.</summary>
            public ChannelReader<TRead> Reader { get; protected set; }

            /// <summary>Gets the writable half of this channel.</summary>
            public ChannelWriter<TWrite> Writer { get; protected set; }

            /// <summary>Implicit cast from a <see cref="T:System.Threading.Channels.Channel`2" /> to its readable half.</summary>
            /// <param name="channel">The <see cref="T:System.Threading.Channels.Channel`2" /> being cast.</param>
            /// <returns>The readable half.</returns>
            public static implicit operator ChannelReader<TRead>(Channel<TWrite, TRead> channel)
            {
                return channel.Reader;
            }

            /// <summary>Implicit cast from a <see cref="T:System.Threading.Channels.Channel`2" /> to its writable half.</summary>
            /// <param name="channel">The <see cref="T:System.Threading.Channels.Channel`2" /> being cast.</param>
            /// <returns>The writable half.</returns>
            public static implicit operator ChannelWriter<TWrite>(Channel<TWrite, TRead> channel)
            {
                return channel.Writer;
            }

            /// <summary>Initializes an instance of the <see cref="T:System.Threading.Channels.Channel`2" /> class.</summary>
            protected Channel()
            {
            }
        }

        #nullable enable
        /// <summary>Exception thrown when a channel is used after it's been closed.</summary>
        public class ChannelClosedException : InvalidOperationException
        {
            /// <summary>Initializes a new instance of the <see cref="T:System.Threading.Channels.ChannelClosedException" /> class.</summary>
            public ChannelClosedException()
                : base(MDCFR.Properties.Resources.ChannelClosedException_DefaultMessage) { }

            /// <summary>Initializes a new instance of the <see cref="T:System.Threading.Channels.ChannelClosedException" /> class.</summary>
            /// <param name="message">The message that describes the error.</param>
            public ChannelClosedException(string? message)
                : base(message) { }

            /// <summary>Initializes a new instance of the <see cref="T:System.Threading.Channels.ChannelClosedException" /> class.</summary>
            /// <param name="innerException">The exception that is the cause of this exception.</param>
            public ChannelClosedException(Exception? innerException)
                : base(MDCFR.Properties.Resources.ChannelClosedException_DefaultMessage, innerException) { }

            /// <summary>Initializes a new instance of the <see cref="T:System.Threading.Channels.ChannelClosedException" /> class.</summary>
            /// <param name="message">The message that describes the error.</param>
            /// <param name="innerException">The exception that is the cause of this exception.</param>
            public ChannelClosedException(string? message, Exception? innerException)
                : base(message, innerException) { }
        }
        #nullable disable

        /// <summary>Provides options that control the behavior of channel instances.</summary>
        public abstract class ChannelOptions
        {
            /// <summary>
            ///   <see langword="true" /> if writers to the channel guarantee that there will only ever be at most one write operation
            ///       at a time; <see langword="false" /> if no such constraint is guaranteed.</summary>
            public bool SingleWriter { get; set; }

            /// <summary>
            ///   <see langword="true" /> readers from the channel guarantee that there will only ever be at most one read operation at a time;
            ///       <see langword="false" /> if no such constraint is guaranteed.</summary>
            public bool SingleReader { get; set; }

            /// <summary>
            ///   <see langword="true" /> if operations performed on a channel may synchronously invoke continuations subscribed to
            ///       notifications of pending async operations; <see langword="false" /> if all continuations should be invoked asynchronously.</summary>
            public bool AllowSynchronousContinuations { get; set; }

            /// <summary>Initializes an instance of the <see cref="T:System.Threading.Channels.ChannelOptions" /> class.</summary>
            protected ChannelOptions() { }
        }

        /// <summary>Provides a base class for reading from a channel.</summary>
        /// <typeparam name="T">Specifies the type of data that may be read from the channel.</typeparam>
        public abstract class ChannelReader<T>
        {
            /// <summary>Gets a <see cref="T:System.Threading.Tasks.Task" /> that completes when no more data will ever
            ///       be available to be read from this channel.</summary>
            public virtual Task Completion => ChannelUtilities.s_neverCompletingTask;

            /// <summary>Gets a value that indicates whether <see cref="P:System.Threading.Channels.ChannelReader`1.Count" /> is available for use on this <see cref="T:System.Threading.Channels.ChannelReader`1" /> instance.</summary>
            public virtual bool CanCount => false;

            /// <summary>Gets a value that indicates whether <see cref="M:System.Threading.Channels.ChannelReader`1.TryPeek(`0@)" /> is available for use on this <see cref="T:System.Threading.Channels.ChannelReader`1" /> instance.</summary>
            /// <returns>
            ///   <see langword="true" /> if peeking is supported by this channel instance; <see langword="false" /> otherwise.</returns>
            public virtual bool CanPeek => false;

            /// <summary>Gets the current number of items available from this channel reader.</summary>
            /// <exception cref="T:System.NotSupportedException">Counting is not supported on this instance.</exception>
            public virtual int Count
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            /// <summary>Attempts to read an item from the channel.</summary>
            /// <param name="item">The read item, or a default value if no item could be read.</param>
            /// <returns>
            ///   <see langword="true" /> if an item was read; otherwise, <see langword="false" />.</returns>
            public abstract bool TryRead([MaybeNullWhen(false)] out T item);

            /// <summary>Attempts to peek at an item from the channel.</summary>
            /// <param name="item">The peeked item, or a default value if no item could be peeked.</param>
            /// <returns>
            ///   <see langword="true" /> if an item was read; otherwise, <see langword="false" />.</returns>
            public virtual bool TryPeek([MaybeNullWhen(false)] out T item)
            {
                item = default(T);
                return false;
            }

            /// <summary>Returns a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that will complete when data is available to read.</summary>
            /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the wait operation.</param>
            /// <returns>
            ///   <para>A <see cref="T:System.Threading.Tasks.ValueTask`1" /> that will complete with a <see langword="true" /> result when data is available to read
            ///       or with a <see langword="false" /> result when no further data will ever be available to be read due to the channel completing successfully.</para>
            ///   <para>If the channel completes with an exception, the task will also complete with an exception.</para>
            /// </returns>
            public abstract ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken = default(CancellationToken));

            /// <summary>Asynchronously reads an item from the channel.</summary>
            /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the read operation.</param>
            /// <returns>A <see cref="T:System.Threading.Tasks.ValueTask`1" /> that represents the asynchronous read operation.</returns>
            public virtual ValueTask<T> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new ValueTask<T>(Task.FromCanceled<T>(cancellationToken));
                }
                try
                {
                    if (TryRead(out var item))
                    {
                        return new ValueTask<T>(item);
                    }
                }
                catch (Exception ex) when (!(ex is ChannelClosedException) && !(ex is OperationCanceledException))
                {
                    return new ValueTask<T>(Task.FromException<T>(ex));
                }
                return ReadAsyncCore(cancellationToken);
                async ValueTask<T> ReadAsyncCore(CancellationToken ct)
                {
                    T item2;
                    do
                    {
                        if (!(await WaitToReadAsync(ct).ConfigureAwait(continueOnCapturedContext: false)))
                        {
                            throw new ChannelClosedException();
                        }
                    }
                    while (!TryRead(out item2));
                    return item2;
                }
            }

            /// <summary>Initializes an instance of the <see cref="T:System.Threading.Channels.ChannelReader`1" /> class.</summary>
            protected ChannelReader()
            {
            }
        }

        internal static class ChannelUtilities
        {
            internal static readonly Exception s_doneWritingSentinel = new Exception("s_doneWritingSentinel");

            internal static readonly Task<bool> s_trueTask = Task.FromResult(result: true);

            internal static readonly Task<bool> s_falseTask = Task.FromResult(result: false);

            internal static readonly Task s_neverCompletingTask = new TaskCompletionSource<bool>().Task;

            internal static void Complete(TaskCompletionSource tcs, Exception error = null)
            {
                if (error is OperationCanceledException ex)
                {
                    tcs.TrySetCanceled(ex.CancellationToken);
                }
                else if (error != null && error != s_doneWritingSentinel)
                {
                    if (tcs.TrySetException(error))
                    {
                        _ = tcs.Task.Exception;
                    }
                }
                else
                {
                    tcs.TrySetResult();
                }
            }

            internal static ValueTask<T> GetInvalidCompletionValueTask<T>(Exception error)
            {
                Task<T> task = ((error == s_doneWritingSentinel) ? Task.FromException<T>(CreateInvalidCompletionException()) : ((error is OperationCanceledException ex) ? Task.FromCanceled<T>(ex.CancellationToken.IsCancellationRequested ? ex.CancellationToken : new CancellationToken(canceled: true)) : Task.FromException<T>(CreateInvalidCompletionException(error))));
                return new ValueTask<T>(task);
            }

            internal static void QueueWaiter(ref AsyncOperation<bool> tail, AsyncOperation<bool> waiter)
            {
                AsyncOperation<bool> asyncOperation = tail;
                if (asyncOperation == null)
                {
                    waiter.Next = waiter;
                }
                else
                {
                    waiter.Next = asyncOperation.Next;
                    asyncOperation.Next = waiter;
                }
                tail = waiter;
            }

            internal static void WakeUpWaiters(ref AsyncOperation<bool> listTail, bool result, Exception error = null)
            {
                AsyncOperation<bool> asyncOperation = listTail;
                if (asyncOperation != null)
                {
                    listTail = null;
                    AsyncOperation<bool> next = asyncOperation.Next;
                    AsyncOperation<bool> asyncOperation2 = next;
                    do
                    {
                        AsyncOperation<bool> next2 = asyncOperation2.Next;
                        asyncOperation2.Next = null;
                        bool flag = ((error != null) ? asyncOperation2.TrySetException(error) : asyncOperation2.TrySetResult(result));
                        asyncOperation2 = next2;
                    }
                    while (asyncOperation2 != next);
                }
            }

            internal static void FailOperations<T, TInner>(Deque<T> operations, Exception error) where T : AsyncOperation<TInner>
            {
                while (!operations.IsEmpty)
                {
                    operations.DequeueHead().TrySetException(error);
                }
            }

            internal static Exception CreateInvalidCompletionException(Exception inner = null)
            {
                if (!(inner is OperationCanceledException))
                {
                    if (inner == null || inner == s_doneWritingSentinel)
                    {
                        return new ChannelClosedException();
                    }
                    return new ChannelClosedException(inner);
                }
                return inner;
            }
        }

        /// <summary>Provides a base class for writing to a channel.</summary>
        /// <typeparam name="T">Specifies the type of data that may be written to the channel.</typeparam>
        public abstract class ChannelWriter<T>
        {
            /// <summary>Attempts to mark the channel as being completed, meaning no more data will be written to it.</summary>
            /// <param name="error">An <see cref="T:System.Exception" /> indicating the failure causing no more data to be written, or null for success.</param>
            /// <returns>
            ///   <see langword="true" /> if this operation successfully completes the channel; otherwise, <see langword="false" /> if the channel could not be marked for completion,
            ///       for example due to having already been marked as such, or due to not supporting completion.
            ///     .</returns>
            #nullable enable
            public virtual bool TryComplete(Exception? error = null) { return false; }
            #nullable disable

            /// <summary>Attempts to write the specified item to the channel.</summary>
            /// <param name="item">The item to write.</param>
            /// <returns>
            ///   <see langword="true" /> if the item was written; otherwise, <see langword="false" />.</returns>
            public abstract bool TryWrite(T item);

            /// <summary>Returns a <see cref="T:System.Threading.Tasks.ValueTask`1" /> that will complete when space is available to write an item.</summary>
            /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the wait operation.</param>
            /// <returns>A <see cref="T:System.Threading.Tasks.ValueTask`1" /> that will complete with a <see langword="true" /> result when space is available to write an item
            ///       or with a <see langword="false" /> result when no further writing will be permitted.</returns>
            public abstract ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken = default(CancellationToken));

            /// <summary>Asynchronously writes an item to the channel.</summary>
            /// <param name="item">The value to write to the channel.</param>
            /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> used to cancel the write operation.</param>
            /// <returns>A <see cref="T:System.Threading.Tasks.ValueTask" /> that represents the asynchronous write operation.</returns>
            public virtual ValueTask WriteAsync(T item, CancellationToken cancellationToken = default(CancellationToken))
            {
                try
                {
                    return cancellationToken.IsCancellationRequested ? new ValueTask(Task.FromCanceled<T>(cancellationToken)) : (TryWrite(item) ? default(ValueTask) : WriteAsyncCore(item, cancellationToken));
                }
                catch (Exception exception)
                {
                    return new ValueTask(Task.FromException(exception));
                }
            }

            private async ValueTask WriteAsyncCore(T innerItem, CancellationToken ct)
            {
                while (await WaitToWriteAsync(ct).ConfigureAwait(continueOnCapturedContext: false))
                {
                    if (TryWrite(innerItem))
                    {
                        return;
                    }
                }
                throw ChannelUtilities.CreateInvalidCompletionException();
            }

            /// <summary>Mark the channel as being complete, meaning no more items will be written to it.</summary>
            /// <param name="error">Optional Exception indicating a failure that's causing the channel to complete.</param>
            /// <exception cref="System.InvalidOperationException">The channel has already been marked as complete.</exception>
            #nullable enable
            public void Complete(Exception? error = null) { if (TryComplete(error) == false) { throw ChannelUtilities.CreateInvalidCompletionException(); } }
            #nullable disable

            /// <summary>Initializes an instance of the <see cref="System.Threading.Channels.ChannelWriter{T}" /> class.</summary>
            protected ChannelWriter() { }
        }

        internal sealed class DebugEnumeratorDebugView<T>
        {
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items { get; }

            public DebugEnumeratorDebugView(IDebugEnumerable<T> enumerable)
            {
                List<T> list = new List<T>();
                foreach (T item in enumerable)
                {
                    list.Add(item);
                }
                Items = list.ToArray();
            }
        }

        internal interface IDebugEnumerable<T> { IEnumerator<T> GetEnumerator(); }

        [DebuggerDisplay("Items = {ItemsCountForDebugger}, Closed = {ChannelIsClosedForDebugger}")]
        [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
        internal sealed class SingleConsumerUnboundedChannel<T> : Channel<T>, IDebugEnumerable<T>
        {
            [DebuggerDisplay("Items = {ItemsCountForDebugger}")]
            [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
            private sealed class UnboundedChannelReader : ChannelReader<T>, IDebugEnumerable<T>
            {
                internal readonly SingleConsumerUnboundedChannel<T> _parent;

                private readonly AsyncOperation<T> _readerSingleton;

                private readonly AsyncOperation<bool> _waiterSingleton;

                public override Task Completion => _parent._completion.Task;

                public override bool CanPeek => true;

                private int ItemsCountForDebugger => _parent._items.Count;

                internal UnboundedChannelReader(SingleConsumerUnboundedChannel<T> parent)
                {
                    _parent = parent;
                    _readerSingleton = new AsyncOperation<T>(parent._runContinuationsAsynchronously, default(CancellationToken), pooled: true);
                    _waiterSingleton = new AsyncOperation<bool>(parent._runContinuationsAsynchronously, default(CancellationToken), pooled: true);
                }

                public override ValueTask<T> ReadAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask<T>(Task.FromCanceled<T>(cancellationToken));
                    }
                    if (TryRead(out var item))
                    {
                        return new ValueTask<T>(item);
                    }
                    SingleConsumerUnboundedChannel<T> parent = _parent;
                    AsyncOperation<T> asyncOperation;
                    AsyncOperation<T> asyncOperation2;
                    lock (parent.SyncObj)
                    {
                        if (TryRead(out item))
                        {
                            return new ValueTask<T>(item);
                        }
                        if (parent._doneWriting != null)
                        {
                            return ChannelUtilities.GetInvalidCompletionValueTask<T>(parent._doneWriting);
                        }
                        asyncOperation = parent._blockedReader;
                        if (!cancellationToken.CanBeCanceled && _readerSingleton.TryOwnAndReset())
                        {
                            asyncOperation2 = _readerSingleton;
                            if (asyncOperation2 == asyncOperation)
                            {
                                asyncOperation = null;
                            }
                        }
                        else
                        {
                            asyncOperation2 = new AsyncOperation<T>(_parent._runContinuationsAsynchronously, cancellationToken);
                        }
                        parent._blockedReader = asyncOperation2;
                    }
                    asyncOperation?.TrySetCanceled();
                    return asyncOperation2.ValueTaskOfT;
                }

                public override bool TryRead([MaybeNullWhen(false)] out T item)
                {
                    SingleConsumerUnboundedChannel<T> parent = _parent;
                    if (parent._items.TryDequeue(out item))
                    {
                        if (parent._doneWriting != null && parent._items.IsEmpty)
                        {
                            ChannelUtilities.Complete(parent._completion, parent._doneWriting);
                        }
                        return true;
                    }
                    return false;
                }

                public override bool TryPeek([MaybeNullWhen(false)] out T item)
                {
                    return _parent._items.TryPeek(out item);
                }

                public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken));
                    }
                    if (!_parent._items.IsEmpty)
                    {
                        return new ValueTask<bool>(result: true);
                    }
                    SingleConsumerUnboundedChannel<T> parent = _parent;
                    AsyncOperation<bool> asyncOperation = null;
                    AsyncOperation<bool> asyncOperation2;
                    lock (parent.SyncObj)
                    {
                        if (!parent._items.IsEmpty)
                        {
                            return new ValueTask<bool>(result: true);
                        }
                        if (parent._doneWriting != null)
                        {
                            return (parent._doneWriting != ChannelUtilities.s_doneWritingSentinel) ? new ValueTask<bool>(Task.FromException<bool>(parent._doneWriting)) : default(ValueTask<bool>);
                        }
                        asyncOperation = parent._waitingReader;
                        if (!cancellationToken.CanBeCanceled && _waiterSingleton.TryOwnAndReset())
                        {
                            asyncOperation2 = _waiterSingleton;
                            if (asyncOperation2 == asyncOperation)
                            {
                                asyncOperation = null;
                            }
                        }
                        else
                        {
                            asyncOperation2 = new AsyncOperation<bool>(_parent._runContinuationsAsynchronously, cancellationToken);
                        }
                        parent._waitingReader = asyncOperation2;
                    }
                    asyncOperation?.TrySetCanceled();
                    return asyncOperation2.ValueTaskOfT;
                }

                IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
                {
                    return _parent._items.GetEnumerator();
                }
            }

            [DebuggerDisplay("Items = {ItemsCountForDebugger}")]
            [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
            private sealed class UnboundedChannelWriter : ChannelWriter<T>, IDebugEnumerable<T>
            {
                internal readonly SingleConsumerUnboundedChannel<T> _parent;

                private int ItemsCountForDebugger => _parent._items.Count;

                internal UnboundedChannelWriter(SingleConsumerUnboundedChannel<T> parent)
                {
                    _parent = parent;
                }

                public override bool TryComplete(Exception error)
                {
                    AsyncOperation<T> asyncOperation = null;
                    AsyncOperation<bool> asyncOperation2 = null;
                    bool flag = false;
                    SingleConsumerUnboundedChannel<T> parent = _parent;
                    lock (parent.SyncObj)
                    {
                        if (parent._doneWriting != null)
                        {
                            return false;
                        }
                        parent._doneWriting = error ?? ChannelUtilities.s_doneWritingSentinel;
                        if (parent._items.IsEmpty)
                        {
                            flag = true;
                            if (parent._blockedReader != null)
                            {
                                asyncOperation = parent._blockedReader;
                                parent._blockedReader = null;
                            }
                            if (parent._waitingReader != null)
                            {
                                asyncOperation2 = parent._waitingReader;
                                parent._waitingReader = null;
                            }
                        }
                    }
                    if (flag)
                    {
                        ChannelUtilities.Complete(parent._completion, error);
                    }
                    if (asyncOperation != null)
                    {
                        error = ChannelUtilities.CreateInvalidCompletionException(error);
                        asyncOperation.TrySetException(error);
                    }
                    if (asyncOperation2 != null)
                    {
                        if (error != null)
                        {
                            asyncOperation2.TrySetException(error);
                        }
                        else
                        {
                            asyncOperation2.TrySetResult(item: false);
                        }
                    }
                    return true;
                }

                public override bool TryWrite(T item)
                {
                    SingleConsumerUnboundedChannel<T> parent = _parent;
                    AsyncOperation<T> asyncOperation;
                    do
                    {
                        asyncOperation = null;
                        AsyncOperation<bool> asyncOperation2 = null;
                        lock (parent.SyncObj)
                        {
                            if (parent._doneWriting != null)
                            {
                                return false;
                            }
                            asyncOperation = parent._blockedReader;
                            if (asyncOperation != null)
                            {
                                parent._blockedReader = null;
                            }
                            else
                            {
                                parent._items.Enqueue(item);
                                asyncOperation2 = parent._waitingReader;
                                if (asyncOperation2 == null)
                                {
                                    return true;
                                }
                                parent._waitingReader = null;
                            }
                        }
                        if (asyncOperation2 != null)
                        {
                            asyncOperation2.TrySetResult(item: true);
                            return true;
                        }
                    }
                    while (!asyncOperation.TrySetResult(item));
                    return true;
                }

                public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken)
                {
                    Exception doneWriting = _parent._doneWriting;
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (doneWriting != null)
                        {
                            if (doneWriting == ChannelUtilities.s_doneWritingSentinel)
                            {
                                return default(ValueTask<bool>);
                            }
                            return new ValueTask<bool>(Task.FromException<bool>(doneWriting));
                        }
                        return new ValueTask<bool>(result: true);
                    }
                    return new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken));
                }

                public override ValueTask WriteAsync(T item, CancellationToken cancellationToken)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (!TryWrite(item))
                        {
                            return new ValueTask(Task.FromException(ChannelUtilities.CreateInvalidCompletionException(_parent._doneWriting)));
                        }
                        return default(ValueTask);
                    }
                    return new ValueTask(Task.FromCanceled(cancellationToken));
                }

                IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
                {
                    return _parent._items.GetEnumerator();
                }
            }

            private readonly TaskCompletionSource _completion;

            private readonly SingleProducerSingleConsumerQueue<T> _items = new SingleProducerSingleConsumerQueue<T>();

            private readonly bool _runContinuationsAsynchronously;

            private volatile Exception _doneWriting;

            private AsyncOperation<T> _blockedReader;

            private AsyncOperation<bool> _waitingReader;

            private object SyncObj => _items;

            private int ItemsCountForDebugger => _items.Count;

            private bool ChannelIsClosedForDebugger => _doneWriting != null;

            internal SingleConsumerUnboundedChannel(bool runContinuationsAsynchronously)
            {
                _runContinuationsAsynchronously = runContinuationsAsynchronously;
                _completion = new TaskCompletionSource(runContinuationsAsynchronously ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None);
                base.Reader = new UnboundedChannelReader(this);
                base.Writer = new UnboundedChannelWriter(this);
            }

            IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }

        internal sealed class TaskCompletionSource : TaskCompletionSource<VoidResult>
        {
            public TaskCompletionSource(TaskCreationOptions creationOptions)
                : base(creationOptions) { }

            public bool TrySetResult() { return TrySetResult(default(VoidResult)); }
        }

        [DebuggerDisplay("Items = {ItemsCountForDebugger}, Closed = {ChannelIsClosedForDebugger}")]
	    [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
        internal sealed class UnboundedChannel<T> : Channel<T>, IDebugEnumerable<T>
        {
            [DebuggerDisplay("Items = {Count}")]
            [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
            private sealed class UnboundedChannelReader : ChannelReader<T>, IDebugEnumerable<T>
            {
                internal readonly UnboundedChannel<T> _parent;

                private readonly AsyncOperation<T> _readerSingleton;

                private readonly AsyncOperation<bool> _waiterSingleton;

                public override Task Completion => _parent._completion.Task;

                public override bool CanCount => true;

                public override bool CanPeek => true;

                public override int Count => _parent._items.Count;

                internal UnboundedChannelReader(UnboundedChannel<T> parent)
                {
                    _parent = parent;
                    _readerSingleton = new AsyncOperation<T>(parent._runContinuationsAsynchronously, default(CancellationToken), pooled: true);
                    _waiterSingleton = new AsyncOperation<bool>(parent._runContinuationsAsynchronously, default(CancellationToken), pooled: true);
                }

                public override ValueTask<T> ReadAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask<T>(Task.FromCanceled<T>(cancellationToken));
                    }
                    UnboundedChannel<T> parent = _parent;
                    if (parent._items.TryDequeue(out var result))
                    {
                        CompleteIfDone(parent);
                        return new ValueTask<T>(result);
                    }
                    lock (parent.SyncObj)
                    {
                        if (parent._items.TryDequeue(out result))
                        {
                            CompleteIfDone(parent);
                            return new ValueTask<T>(result);
                        }
                        if (parent._doneWriting != null)
                        {
                            return ChannelUtilities.GetInvalidCompletionValueTask<T>(parent._doneWriting);
                        }
                        if (!cancellationToken.CanBeCanceled)
                        {
                            AsyncOperation<T> readerSingleton = _readerSingleton;
                            if (readerSingleton.TryOwnAndReset())
                            {
                                parent._blockedReaders.EnqueueTail(readerSingleton);
                                return readerSingleton.ValueTaskOfT;
                            }
                        }
                        AsyncOperation<T> asyncOperation = new AsyncOperation<T>(parent._runContinuationsAsynchronously, cancellationToken);
                        parent._blockedReaders.EnqueueTail(asyncOperation);
                        return asyncOperation.ValueTaskOfT;
                    }
                }

                public override bool TryRead([MaybeNullWhen(false)] out T item)
                {
                    UnboundedChannel<T> parent = _parent;
                    if (parent._items.TryDequeue(out item))
                    {
                        CompleteIfDone(parent);
                        return true;
                    }
                    item = default(T);
                    return false;
                }

                public override bool TryPeek([MaybeNullWhen(false)] out T item)
                {
                    return _parent._items.TryPeek(out item);
                }

                private static void CompleteIfDone(UnboundedChannel<T> parent)
                {
                    if (parent._doneWriting != null && parent._items.IsEmpty)
                    {
                        ChannelUtilities.Complete(parent._completion, parent._doneWriting);
                    }
                }

                public override ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken));
                    }
                    if (!_parent._items.IsEmpty)
                    {
                        return new ValueTask<bool>(result: true);
                    }
                    UnboundedChannel<T> parent = _parent;
                    lock (parent.SyncObj)
                    {
                        if (!parent._items.IsEmpty)
                        {
                            return new ValueTask<bool>(result: true);
                        }
                        if (parent._doneWriting != null)
                        {
                            return (parent._doneWriting != ChannelUtilities.s_doneWritingSentinel) ? new ValueTask<bool>(Task.FromException<bool>(parent._doneWriting)) : default(ValueTask<bool>);
                        }
                        if (!cancellationToken.CanBeCanceled)
                        {
                            AsyncOperation<bool> waiterSingleton = _waiterSingleton;
                            if (waiterSingleton.TryOwnAndReset())
                            {
                                ChannelUtilities.QueueWaiter(ref parent._waitingReadersTail, waiterSingleton);
                                return waiterSingleton.ValueTaskOfT;
                            }
                        }
                        AsyncOperation<bool> asyncOperation = new AsyncOperation<bool>(parent._runContinuationsAsynchronously, cancellationToken);
                        ChannelUtilities.QueueWaiter(ref parent._waitingReadersTail, asyncOperation);
                        return asyncOperation.ValueTaskOfT;
                    }
                }

                IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
                {
                    return _parent._items.GetEnumerator();
                }
            }

            [DebuggerDisplay("Items = {ItemsCountForDebugger}")]
            [DebuggerTypeProxy(typeof(DebugEnumeratorDebugView<>))]
            private sealed class UnboundedChannelWriter : ChannelWriter<T>, IDebugEnumerable<T>
            {
                internal readonly UnboundedChannel<T> _parent;

                private int ItemsCountForDebugger => _parent._items.Count;

                internal UnboundedChannelWriter(UnboundedChannel<T> parent)
                {
                    _parent = parent;
                }

                public override bool TryComplete(Exception error)
                {
                    UnboundedChannel<T> parent = _parent;
                    bool isEmpty;
                    lock (parent.SyncObj)
                    {
                        if (parent._doneWriting != null)
                        {
                            return false;
                        }
                        parent._doneWriting = error ?? ChannelUtilities.s_doneWritingSentinel;
                        isEmpty = parent._items.IsEmpty;
                    }
                    if (isEmpty)
                    {
                        ChannelUtilities.Complete(parent._completion, error);
                    }
                    ChannelUtilities.FailOperations<AsyncOperation<T>, T>(parent._blockedReaders, ChannelUtilities.CreateInvalidCompletionException(error));
                    ChannelUtilities.WakeUpWaiters(ref parent._waitingReadersTail, result: false, error);
                    return true;
                }

                public override bool TryWrite(T item)
                {
                    UnboundedChannel<T> parent = _parent;
                    AsyncOperation<bool> listTail;
                    while (true)
                    {
                        AsyncOperation<T> asyncOperation = null;
                        listTail = null;
                        lock (parent.SyncObj)
                        {
                            if (parent._doneWriting != null)
                            {
                                return false;
                            }
                            if (parent._blockedReaders.IsEmpty)
                            {
                                parent._items.Enqueue(item);
                                listTail = parent._waitingReadersTail;
                                if (listTail == null)
                                {
                                    return true;
                                }
                                parent._waitingReadersTail = null;
                            }
                            else
                            {
                                asyncOperation = parent._blockedReaders.DequeueHead();
                            }
                        }
                        if (asyncOperation == null)
                        {
                            break;
                        }
                        if (asyncOperation.TrySetResult(item))
                        {
                            return true;
                        }
                    }
                    ChannelUtilities.WakeUpWaiters(ref listTail, result: true);
                    return true;
                }

                public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken)
                {
                    Exception doneWriting = _parent._doneWriting;
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (doneWriting != null)
                        {
                            if (doneWriting == ChannelUtilities.s_doneWritingSentinel)
                            {
                                return default(ValueTask<bool>);
                            }
                            return new ValueTask<bool>(Task.FromException<bool>(doneWriting));
                        }
                        return new ValueTask<bool>(result: true);
                    }
                    return new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken));
                }

                public override ValueTask WriteAsync(T item, CancellationToken cancellationToken)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (!TryWrite(item))
                        {
                            return new ValueTask(Task.FromException(ChannelUtilities.CreateInvalidCompletionException(_parent._doneWriting)));
                        }
                        return default(ValueTask);
                    }
                    return new ValueTask(Task.FromCanceled(cancellationToken));
                }

                IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
                {
                    return _parent._items.GetEnumerator();
                }
            }

            private readonly TaskCompletionSource _completion;

            private readonly ConcurrentQueue<T> _items = new ConcurrentQueue<T>();

            private readonly Deque<AsyncOperation<T>> _blockedReaders = new Deque<AsyncOperation<T>>();

            private readonly bool _runContinuationsAsynchronously;

            private AsyncOperation<bool> _waitingReadersTail;

            private Exception _doneWriting;

            private object SyncObj => _items;

            private int ItemsCountForDebugger => _items.Count;

            private bool ChannelIsClosedForDebugger => _doneWriting != null;

            internal UnboundedChannel(bool runContinuationsAsynchronously)
            {
                _runContinuationsAsynchronously = runContinuationsAsynchronously;
                _completion = new TaskCompletionSource(runContinuationsAsynchronously ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None);
                base.Reader = new UnboundedChannelReader(this);
                base.Writer = new UnboundedChannelWriter(this);
            }

            [Conditional("DEBUG")]
            private void AssertInvariants()
            {
                if (!_items.IsEmpty)
                {
                    _ = _runContinuationsAsynchronously;
                }
                if (!_blockedReaders.IsEmpty || _waitingReadersTail != null)
                {
                    _ = _runContinuationsAsynchronously;
                }
                _ = _completion.Task.IsCompleted;
            }

            IEnumerator<T> IDebugEnumerable<T>.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
        }

        /// <summary>Provides options that control the behavior of unbounded <see cref="T:System.Threading.Channels.Channel`1" /> instances.</summary>
        public sealed class UnboundedChannelOptions : ChannelOptions
        {
            /// <summary>Initializes a new instance of the <see cref="System.Threading.Channels.UnboundedChannelOptions" /> class.</summary>
            public UnboundedChannelOptions() { }
        }

        internal sealed class VoidAsyncOperationWithData<TData> : AsyncOperation<VoidResult>
        {
            public TData Item { get; set; }

            public VoidAsyncOperationWithData(bool runContinuationsAsynchronously, CancellationToken cancellationToken = default(CancellationToken), bool pooled = false)
                : base(runContinuationsAsynchronously, cancellationToken, pooled) { }
        }

    }

}