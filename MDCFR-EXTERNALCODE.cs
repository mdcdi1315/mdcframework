// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Runtime.InteropServices;
#if NET6_0_OR_GREATER
	using System.Runtime.Intrinsics;
	using System.Runtime.Intrinsics.X86;
	using static System.Runtime.Intrinsics.X86.Ssse3;
#endif

namespace System
{

    namespace Imported
    {
        /// <summary>
        /// Represents position in non-contiguous set of memory.
        /// Parts of this type should not be interpreted by anything but the type that created it.
        /// </summary>
        #nullable enable
        public readonly struct SequencePosition : IEquatable<SequencePosition>
        {
            private readonly object? _object;
            private readonly int _integer;

            /// <summary>
            /// Creates new <see cref="SequencePosition"/>
            /// </summary>
            public SequencePosition(object? @object, int integer)
            {
                _object = @object;
                _integer = integer;
            }

            /// <summary>
            /// Returns object part of this <see cref="SequencePosition"/>
            /// </summary>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public object? GetObject() => _object;

            /// <summary>
            /// Returns integer part of this <see cref="SequencePosition"/>
            /// </summary>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public int GetInteger() => _integer;

            /// <summary>
            /// Indicates whether the current <see cref="SequencePosition"/> is equal to another <see cref="SequencePosition"/>.
            /// <see cref="SequencePosition"/> equality does not guarantee that they point to the same location in <see cref="System.Buffers.ReadOnlySequence{T}" />
            /// </summary>
            public bool Equals(SequencePosition other) => _integer == other._integer && object.Equals(this._object, other._object);

            /// <summary>
            /// Indicates whether the current <see cref="SequencePosition"/> is equal to another <see cref="object"/>.
            /// <see cref="SequencePosition"/> equality does not guarantee that they point to the same location in <see cref="System.Buffers.ReadOnlySequence{T}" />
            /// </summary>
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override bool Equals([NotNullWhen(true)] object? obj) => obj is SequencePosition other && this.Equals(other);

            /// <inheritdoc />
            [EditorBrowsable(EditorBrowsableState.Never)]
            public override System.Int32 GetHashCode()
            {
                return _object?.GetHashCode() ?? _integer;
            }
        }
        #nullable disable
    }

    namespace Runtime.CompilerServices
    {
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

    namespace Numerics.Hashing
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
}

namespace Internal.Runtime.CompilerServices
{
    //
    // Subsetted clone of System.Runtime.CompilerServices.Unsafe for internal runtime use.
    // Keep in sync with https://github.com/dotnet/corefx/tree/master/src/System.Runtime.CompilerServices.Unsafe.
    //

    /// <summary>
    /// For internal use only. Contains generic, low-level functionality for manipulating pointers.
    /// </summary>
    public static unsafe class Unsafe
    {
        /// <summary>
        /// Returns a pointer to the given by-ref parameter.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AsPointer<T>(ref T value)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // conv.u
            // ret
        }

        /// <summary>
        /// Returns the size of an object of the given type parameter.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            throw new PlatformNotSupportedException();

            // sizeof !!0
            // ret
        }

        #nullable enable
        /// <summary>
        /// Casts the given object to the specified type, performs no dynamic type checking.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static T As<T>(object? value) where T : class?
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }
        #nullable disable

        /// <summary>
        /// Reinterprets the given reference as a reference to a value of type <typeparamref name="TTo"/>.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ret
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
            return ref AddByteOffset(ref source, (IntPtr)(elementOffset * (nint)SizeOf<T>()));
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, IntPtr elementOffset)
        {
            return ref AddByteOffset(ref source, (IntPtr)((nint)elementOffset * (nint)SizeOf<T>()));
        }

        /// <summary>
        /// Adds an element offset to the given pointer.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Add<T>(void* source, int elementOffset)
        {
            return (byte*)source + (elementOffset * (nint)SizeOf<T>());
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T AddByteOffset<T>(ref T source, nuint byteOffset)
        {
            return ref AddByteOffset(ref source, (IntPtr)(void*)byteOffset);
        }

        /// <summary>
        /// Determines whether the specified references point to the same location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AreSame<T>(ref T left, ref T right)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // ceq
            // ret
        }

        /// <summary>
        /// Determines whether the memory address referenced by <paramref name="left"/> is greater than
        /// the memory address referenced by <paramref name="right"/>.
        /// </summary>
        /// <remarks>
        /// This check is conceptually similar to "(void*)(&amp;left) &gt; (void*)(&amp;right)".
        /// </remarks>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressGreaterThan<T>(ref T left, ref T right)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // cgt.un
            // ret
        }

        /// <summary>
        /// Determines whether the memory address referenced by <paramref name="left"/> is less than
        /// the memory address referenced by <paramref name="right"/>.
        /// </summary>
        /// <remarks>
        /// This check is conceptually similar to "(void*)(&amp;left) &lt; (void*)(&amp;right)".
        /// </remarks>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAddressLessThan<T>(ref T left, ref T right)
        {
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // clt.un
            // ret
        }

        /// <summary>
        /// Initializes a block of memory at the given location with a given initial value 
        /// without assuming architecture dependent alignment of the address.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
        {
            for (uint i = 0; i < byteCount; i++) AddByteOffset(ref startAddress, i) = value;
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(void* source) { return Unsafe.As<byte, T>(ref *(byte*)source); }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref byte source)
        {
            return Unsafe.As<byte, T>(ref source);
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(void* destination, T value)
        {
            Unsafe.As<byte, T>(ref *(byte*)destination) = value;
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref byte destination, T value)
        {
            Unsafe.As<byte, T>(ref destination) = value;
        }

        /// <summary>
        /// Adds an element offset to the given reference.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
        {
            // This method is implemented by the toolchain
            throw new PlatformNotSupportedException();

            // ldarg.0
            // ldarg.1
            // add
            // ret
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(void* source)
        {
            return Unsafe.As<byte, T>(ref *(byte*)source);
        }

        /// <summary>
        /// Reads a value of type <typeparamref name="T"/> from the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Read<T>(ref byte source)
        {
            return Unsafe.As<byte, T>(ref source);
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(void* destination, T value)
        {
            Unsafe.As<byte, T>(ref *(byte*)destination) = value;
        }

        /// <summary>
        /// Writes a value of type <typeparamref name="T"/> to the given location.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write<T>(ref byte destination, T value)
        {
            Unsafe.As<byte, T>(ref destination) = value;
        }

        /// <summary>
        /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(void* source)
        {
            return ref Unsafe.As<byte, T>(ref *(byte*)source);
        }

        /// <summary>
        /// Reinterprets the given location as a reference to a value of type <typeparamref name="T"/>.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AsRef<T>(in T source)
        {
            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Determines the byte offset from origin to target from the given references.
        /// </summary>
        [Intrinsic]
        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr ByteOffset<T>(ref T origin, ref T target)
        {
            throw new PlatformNotSupportedException();
        }
    }

}



