
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;


namespace System.Reflection
{
	using System.Reflection.Metadata;
	using System.Reflection.PortableExecutable;
	using System.Reflection.TypeLoading;
	using System.Reflection.TypeLoading.Ecma;
	
	internal static class AssemblyNameHelpers
	{
		internal static AssemblyContentType ExtractAssemblyContentType(this AssemblyNameFlags flags)
		{
			return (AssemblyContentType)(((int)flags >> 9) & 7);
		}

		public static AssemblyNameFlags ExtractAssemblyNameFlags(this AssemblyNameFlags combinedFlags)
		{
			return combinedFlags & (AssemblyNameFlags)(-3825);
		}
	}

	/// <summary>
	/// The base class for binding algorithms used by <see cref="T:System.Reflection.MetadataLoadContext" />.
	/// </summary>
	public abstract class MetadataAssemblyResolver
	{
        /// <summary>
        /// The binding algorithm. This method is called when an Assembly is to be returned from a given AssemblyName.
        /// This occurs when MetadataLoadContext.LoadAssemblyByName() is called or when a Type from one assembly has a
        /// dependency on another assembly.
        ///
        /// It should use MetadataLoadContext.LoadFromStream(), LoadFromAssemblyPath()
        /// or LoadFromByteArray() to load the requested assembly and return it.
        /// </summary>
        ///             <remarks>
        /// To indicate the failure to find an assembly, the handler should return null rather than throwing an exception. Returning null commits
        /// the failure so that future attempts to load that name will fail without re-invoking the handler.
        ///
        /// If the handler throws an exception, the exception will be passed through to the application that invoked the operation that triggered
        /// the binding. The MetadataLoadContext will not catch it and no binding will occur.
        ///
        /// The handler will generally not be called more than once for the same name, unless two threads race to load the same assembly.
        /// Even in that case, one result will win and be atomically bound to the name.
        ///
        /// The MetadataLoadContext intentionally performs no ref-def matching on the returned assembly as what constitutes a ref-def match is a policy.
        /// It is also the kind of arbitrary restriction that MetadataLoadContext strives to avoid.
        ///
        /// The MetadataLoadContext cannot consume assemblies from other MetadataLoadContexts or other type providers (such as the underlying runtime's own Reflection system.)
        /// If a handler returns such an assembly, the MetadataLoadContext throws a FileLoadException.
        /// </remarks>
        #nullable enable
        public abstract Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName);
        #nullable disable
    }

    /// <summary>
    /// A MetadataLoadContext represents a closed universe of Type objects loaded for inspection-only purposes.
    /// Each MetadataLoadContext can have its own binding rules and is isolated from all other MetadataLoadContexts.
    ///
    /// A MetadataLoadContext serves as a dictionary that binds assembly names to Assembly instances that were previously
    /// loaded into the context or need to be loaded.
    ///
    /// Assemblies are treated strictly as metadata. There are no restrictions on loading assemblies based
    /// on target platform, CPU architecture or pointer size. There are no restrictions on the assembly designated
    /// as the core assembly ("mscorlib").
    /// </summary>
    /// <remarks>
    /// Also, as long as the metadata is "syntactically correct", the MetadataLoadContext strives to report it "as is" (as long it
    /// can do so in a way that can be distinguished from valid data) and refrains from judging whether it's "executable."
    /// This is both for performance reasons (checks cost time) and its intended role as metadata inspection tool.
    /// Examples of things that MetadataLoadContexts let go unchecked include creating generic instances that violate generic
    /// parameter constraints, and loading type hierarchies that would be unloadable in an actual runtime (deriving from sealed classes,
    /// overriding members that don't exist in the ancestor classes, failing to implement all abstract methods, etc.)
    ///
    /// You cannot invoke methods, set or get field or property values or instantiate objects using
    /// the Type objects from a MetadataLoadContext. You can however, use FieldInfo.GetRawConstantValue(),
    /// ParameterInfo.RawDefaultValue and PropertyInfo.GetRawConstantValue(). You can retrieve custom attributes
    /// in CustomAttributeData format but not as instantiated custom attributes. The CustomAttributeExtensions
    /// extension api will not work with these Types nor will the IsDefined() family of api.
    ///
    /// There is no default binding policy. You must use a MetadataAssemblyResolver-derived class to load dependencies as needed.
    /// The MetadataLoadContext strives to avoid loading dependencies unless needed.
    /// Therefore, it is possible to do useful analysis of an assembly even
    /// in the absence of dependencies. For example, retrieving an assembly's name and the names of its (direct)
    /// dependencies can be done without having any of those dependencies on hand.
    ///
    /// To bind assemblies, the MetadataLoadContext calls the Resolve method on the correspding MetadataAssemblyResolver.
    /// That method should load the requested assembly and return it.
    /// To do this, it can use LoadFromAssemblyPath() or one of its variants (LoadFromStream(), LoadFromByteArray()).
    ///
    /// Once an assembly has been bound, no assembly with the same assembly name identity
    /// can be bound again from a different location unless the Mvids are identical.
    ///
    /// Once loaded, the underlying file may be locked for the duration of the MetadataLoadContext's lifetime. You can
    /// release the locks by disposing the MetadataLoadContext object. The behavior of any Type, Assembly or other reflection
    /// objects handed out by the MetadataLoadContext is undefined after disposal. Though objects provided by the MetadataLoadContext
    /// strive to throw an ObjectDisposedException, this is not guaranteed. Some apis may return fixed or previously
    /// cached data. Accessing objects *during* a Dispose may result in a unmanaged access violation and failfast.
    ///
    /// Comparing Type, Member and Assembly objects:
    ///   The right way to compare two Reflection objects dispensed by the MetadataLoadContext are:
    ///       m1 == m2
    ///       m1.Equals(m2)
    ///   but not
    ///       object.ReferenceEquals(m1, m2)   /// WRONG
    ///       (object)m1 == (object)m2         /// WRONG
    ///
    ///   Note that the following descriptions are not literal descriptions of how Equals() is implemented. The MetadataLoadContext
    ///   reserves the right to implement Equals() as "object.ReferenceEquals()" and intern the associated objects in such
    ///   a way that Equals() works "as if" it were comparing those things.
    ///
    /// - Each MetadataLoadContext permits only one Assembly instance per assembly identity so equality of assemblies is the same as the
    ///   equality of their assembly identity.
    ///
    /// - Modules are compared by comparing their containing assemblies and their row indices in the assembly's manifest file table.
    ///
    /// - Defined types are compared by comparing their containing modules and their row indices in the module's TypeDefinition table.
    ///
    /// - Constructed types (arrays, byrefs, pointers, generic instances) are compared by comparing all of their component types.
    ///
    /// - Generic parameter types are compared by comparing their containing Modules and their row indices in the module's GenericParameter table.
    ///
    /// - Constructors, methods, fields, events and properties are compared by comparing their declaring types, their row indices in their respective
    ///   token tables and their ReflectedType property.
    ///
    /// - Parameters are compared by comparing their declaring member and their position index.
    ///
    /// Multithreading:
    ///   The MetadataLoadContext and the reflection objects it hands out are all multithread-safe and logically immutable,
    ///   except that no Loads or inspections of reflection objects can be done during or after disposing the owning MetadataLoadContext.
    ///
    /// Support for NetCore Reflection apis:
    ///   .NETCore added a number of apis (IsSZArray, IsVariableBoundArray, IsTypeDefinition, IsGenericTypeParameter, IsGenericMethodParameter,
    ///      HasSameMetadataDefinitionAs, to name a few.) to the Reflection surface area.
    ///
    ///   The Reflection objects dispensed by MetadataLoadContexts support all the new apis *provided* that you are using the netcore build of System.Reflection.MetadataLoadContext.dll.
    ///
    ///   If you are using the netstandard build of System.Reflection.MetadataLoadContext.dll, the NetCore-specific apis are not supported. Attempting to invoke
    ///   them will generate a NotImplementedException or NullReferenceException (unfortunately, we can't improve the exceptions thrown because
    ///   they are being thrown by code this library doesn't control.)
    /// </remarks>
    public sealed class MetadataLoadContext : IDisposable
	{
		private readonly MetadataAssemblyResolver resolver;

		private static readonly string[] s_CoreNames = new string[3] { "mscorlib", "System.Runtime", "netstandard" };

		private RoAssembly _coreAssembly;

		private readonly CoreTypes _coreTypes;

		private volatile Binder _lazyDefaultBinder;

		private ConcurrentBag<IDisposable> _disposables = new ConcurrentBag<IDisposable>();

		private volatile ConstructorInfo _lazyFieldOffset;

		private volatile ConstructorInfo _lazyIn;

		private volatile ConstructorInfo _lazyOut;

		private volatile ConstructorInfo _lazyOptional;

		private volatile ConstructorInfo _lazyPreserveSig;

		private volatile ConstructorInfo _lazyComImport;

		private volatile ConstructorInfo _lazyDllImport;

		private volatile ConstructorInfo _lazyMarshalAs;

		private readonly ConcurrentDictionary<RoAssemblyName, RoAssembly> _loadedAssemblies = new ConcurrentDictionary<RoAssemblyName, RoAssembly>();

		private readonly ConcurrentDictionary<RoAssemblyName, RoAssembly> _binds = new ConcurrentDictionary<RoAssemblyName, RoAssembly>();

        /// <summary>
        /// Returns the assembly that denotes the "system assembly" that houses the well-known types such as System.Int32.
        /// The core assembly is treated differently than other assemblies because references to these well-known types do
        /// not include the assembly reference, unlike normal types.
        ///
        /// Typically, this assembly is named "mscorlib", or "netstandard". If the core assembly cannot be found, the value will be
        /// null and many other reflection methods, including those that parse method signatures, will throw.
        ///
        /// The CoreAssembly is determined by passing the coreAssemblyName parameter passed to the MetadataAssemblyResolver constructor
        /// to the MetadataAssemblyResolver's Resolve method.
        /// If no coreAssemblyName argument was specified in the constructor of MetadataLoadContext, then default values are used
        /// including "mscorlib", "System.Runtime" and "netstandard".
        ///
        /// The designated core assembly does not need to contain the core types directly. It can type forward them to other assemblies.
        /// Thus, it is perfectly permissible to use the mscorlib facade as the designated core assembly.
        ///
        /// Note that "System.Runtime" is not an ideal core assembly as it excludes some of the interop-related pseudo-custom attribute types
        /// such as DllImportAttribute. However, it can serve if you have no interest in those attributes. The CustomAttributes api
        /// will skip those attributes if the core assembly does not include the necessary types.
        ///
        /// The CoreAssembly is not loaded until necessary. These APIs do not trigger the search for the core assembly:
        ///    MetadataLoadContext.LoadFromStream(), LoadFromAssemblyPath(), LoadFromByteArray()
        ///    Assembly.GetName(), Assembly.FullName, Assembly.GetReferencedAssemblies()
        ///    Assembly.GetTypes(), Assembly.DefinedTypes, Assembly.GetExportedTypes(), Assembly.GetForwardedTypes()
        ///    Assembly.GetType(string, bool, bool)
        ///    Type.Name, Type.FullName, Type.AssemblyQualifiedName
        ///
        /// If a core assembly cannot be found or if the core assembly is missing types, this will affect the behavior of the MetadataLoadContext as follows:
        ///
        /// - Apis that need to parse signatures or typespecs and return the results as Types will throw. For example,
        ///   MethodBase.ReturnType, MethodBase.GetParameters(), Type.BaseType, Type.GetInterfaces().
        ///
        /// - Apis that need to compare types to well known core types will not throw and the comparison will evaluate to "false."
        ///   For example, if you do not specify a core assembly, Type.IsPrimitive will return false for everything,
        ///   even types named "System.Int32". Similarly, Type.GetTypeCode() will return TypeCode.Object for everything.
        ///
        /// - If a metadata entity sets flags that surface as a pseudo-custom attribute, and the core assembly does not contain the pseudo-custom attribute
        ///   type, the necessary constructor or any of the parameter types of the constructor, the MetadataLoadContext will not throw. It will omit the pseudo-custom
        ///   attribute from the list of returned attributes.
        /// </summary>
        #nullable enable
        public Assembly? CoreAssembly
		{
			get
			{
				if (IsDisposed)
				{
					throw new ObjectDisposedException("MetadataLoadContext");
				}
				return _coreAssembly;
			}
		}
        #nullable disable

        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Create a new MetadataLoadContext object.
        /// </summary>
        /// <param name="resolver">A <see cref="T:System.Reflection.MetadataAssemblyResolver" /> instance.</param>
        /// <param name="coreAssemblyName">
        /// The name of the assembly that contains the core types such as System.Object. Typically, this would be "mscorlib".
        /// </param>
        #nullable enable
        public MetadataLoadContext(MetadataAssemblyResolver resolver, string? coreAssemblyName = null)
		{
			if (resolver == null)
			{
				throw new ArgumentNullException("resolver");
			}
			this.resolver = resolver;
			if (coreAssemblyName != null)
			{
				new AssemblyName(coreAssemblyName);
			}
			_coreTypes = new CoreTypes(this, coreAssemblyName);
		}
        #nullable disable

        /// <summary>
        /// Loads an assembly from a specific path on the disk and binds its assembly name to it in the MetadataLoadContext. If a prior
        /// assembly with the same name was already loaded into the MetadataLoadContext, the prior assembly will be returned. If the
        /// two assemblies do not have the same Mvid, this method throws a FileLoadException.
        /// </summary>
        public Assembly LoadFromAssemblyPath(string assemblyPath)
		{
			if (assemblyPath == null)
			{
				throw new ArgumentNullException("assemblyPath");
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("MetadataLoadContext");
			}
			return LoadFromStreamCore(File.OpenRead(assemblyPath));
		}

		/// <summary>
		/// Loads an assembly from a byte array and binds its assembly name to it in the MetadataLoadContext. If a prior
		/// assembly with the same name was already loaded into the MetadataLoadContext, the prior assembly will be returned. If the
		/// two assemblies do not have the same Mvid, this method throws a FileLoadException.
		/// </summary>
		public Assembly LoadFromByteArray(byte[] assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("MetadataLoadContext");
			}
			return LoadFromStreamCore(new MemoryStream(assembly));
		}

		/// <summary>
		/// Loads an assembly from a stream and binds its assembly name to it in the MetadataLoadContext. If a prior
		/// assembly with the same name was already loaded into the MetadataLoadContext, the prior assembly will be returned. If the
		/// two assemblies do not have the same Mvid, this method throws a FileLoadException.
		///
		/// The MetadataLoadContext takes ownership of the Stream passed into this method. The original owner must not mutate its position, dispose the Stream or
		/// assume that its position will stay unchanged.
		/// </summary>
		public Assembly LoadFromStream(Stream assembly)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException("assembly");
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("MetadataLoadContext");
			}
			assembly.Position = 0L;
			return LoadFromStreamCore(assembly);
		}

		/// <summary>
		/// Resolves the supplied assembly name to an assembly. If an assembly was previously bound by to this name, that assembly is returned.
		/// Otherwise, the MetadataLoadContext calls the specified MetadataAssemblyResolver. If the resolver returns null, this method throws a FileNotFoundException.
		///
		/// Note that this behavior matches the behavior of AssemblyLoadContext.LoadFromAssemblyName() but does not match the behavior of
		/// Assembly.ReflectionOnlyLoad(). (the latter gives up without raising its resolve event.)
		/// </summary>
		public Assembly LoadFromAssemblyName(string assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("MetadataLoadContext");
			}
			AssemblyName assemblyName2 = new AssemblyName(assemblyName);
			RoAssemblyName refName = assemblyName2.ToRoAssemblyName();
			return ResolveAssembly(refName);
		}

		/// <summary>
		/// Resolves the supplied assembly name to an assembly. If an assembly was previously bound by to this name, that assembly is returned.
		/// Otherwise, the MetadataLoadContext calls the specified MetadataAssemblyResolver. If the resolver returns null, this method throws a FileNotFoundException.
		///
		/// Note that this behavior matches the behavior of AssemblyLoadContext.LoadFromAssemblyName() resolve event but does not match the behavior of
		/// Assembly.ReflectionOnlyLoad(). (the latter gives up without raising its resolve event.)
		/// </summary>
		public Assembly LoadFromAssemblyName(AssemblyName assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			if (IsDisposed)
			{
				throw new ObjectDisposedException("MetadataLoadContext");
			}
			RoAssemblyName refName = assemblyName.ToRoAssemblyName();
			return ResolveAssembly(refName);
		}

		/// <summary>
		/// Return an atomic snapshot of the assemblies that have been loaded into the MetadataLoadContext.
		/// </summary>
		public IEnumerable<Assembly> GetAssemblies()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("MetadataLoadContext");
			}
			return _loadedAssemblies.Values;
		}

		/// <summary>
		/// Releases any native resources (such as file locks on assembly files.) After disposal, it is not safe to use
		/// any Assembly objects dispensed by the MetadataLoadContext, nor any Reflection objects dispensed by those Assembly objects.
		/// Though objects provided by the MetadataLoadContext strive to throw an ObjectDisposedException, this is not guaranteed.
		/// Some apis may return fixed or previously cached data. Accessing objects *during* a Dispose may result in an
		/// unmanaged access violation and failfast.
		/// </summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		internal RoAssembly TryGetCoreAssembly(string coreAssemblyName, out Exception e)
		{
			if (coreAssemblyName == null)
			{
				_coreAssembly = TryGetDefaultCoreAssembly(out e);
			}
			else
			{
				RoAssemblyName refName = new AssemblyName(coreAssemblyName).ToRoAssemblyName();
				_coreAssembly = TryResolveAssembly(refName, out e);
			}
			return _coreAssembly;
		}

		private RoAssembly TryGetDefaultCoreAssembly(out Exception e)
		{
			string[] array = s_CoreNames;
			foreach (string assemblyName in array)
			{
				RoAssemblyName refName = new AssemblyName(assemblyName).ToRoAssemblyName();
				RoAssembly roAssembly = TryResolveAssembly(refName, out e);
				if (roAssembly != null)
				{
					e = null;
					return roAssembly;
				}
			}
			e = new FileNotFoundException(MDCFR.Properties.Resources.UnableToDetermineCoreAssembly);
			return null;
		}

		/// <summary>
		/// Returns a lazily created and cached Type instance corresponding to the indicated core type. This method throws
		/// if the core assembly name wasn't supplied, the core assembly could not be loaded for some reason or if the specified
		/// type does not exist in the core assembly.
		/// </summary>
		internal RoType GetCoreType(CoreType coreType)
		{
			CoreTypes allFoundCoreTypes = GetAllFoundCoreTypes();
			RoType roType = TryGetCoreType(coreType);
			return roType ?? throw allFoundCoreTypes.GetException(coreType);
		}

		/// <summary>
		/// Returns a lazily created and cached Type instance corresponding to the indicated core type. This method returns null
		/// if the core assembly name wasn't supplied, the core assembly could not be loaded for some reason or if the specified
		/// type does not exist in the core assembly.
		/// </summary>
		internal RoType TryGetCoreType(CoreType coreType)
		{
			CoreTypes allFoundCoreTypes = GetAllFoundCoreTypes();
			return allFoundCoreTypes[coreType];
		}

		/// <summary>
		/// Returns a cached array containing the resolved CoreTypes, indexed by the CoreType enum cast to an int.
		/// If the core assembly was not specified, not locatable or if one or more core types aren't present in the core assembly,
		/// the corresponding elements will be null.
		/// </summary>
		internal CoreTypes GetAllFoundCoreTypes()
		{
			return _coreTypes;
		}

		internal Binder GetDefaultBinder()
		{
			return _lazyDefaultBinder ?? (_lazyDefaultBinder = new System.DefaultBinder(this));
		}

		internal void DisposeCheck()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException(MDCFR.Properties.Resources.MetadataLoadContextDisposed, (Exception)null);
			}
		}

		/// <summary>
		/// Adds an object to an internal list of objects to be disposed when the MetadataLoadContext is disposed.
		/// </summary>
		internal void RegisterForDisposal(IDisposable disposable)
		{
			_disposables.Add(disposable);
		}

		private void Dispose(bool disposing)
		{
			IsDisposed = true;
			if (!disposing)
			{
				return;
			}
			ConcurrentBag<IDisposable> disposables = _disposables;
			if (disposables == null)
			{
				return;
			}
			_disposables = null;
			foreach (IDisposable item in disposables)
			{
				item.Dispose();
			}
		}

		internal ConstructorInfo TryGetFieldOffsetCtor()
		{
			return _lazyFieldOffset ?? (_lazyFieldOffset = TryGetConstructor(CoreType.FieldOffsetAttribute, CoreType.Int32));
		}

		internal ConstructorInfo TryGetInCtor()
		{
			return _lazyIn ?? (_lazyIn = TryGetConstructor(CoreType.InAttribute));
		}

		internal ConstructorInfo TryGetOutCtor()
		{
			return _lazyOut ?? (_lazyOut = TryGetConstructor(CoreType.OutAttribute));
		}

		internal ConstructorInfo TryGetOptionalCtor()
		{
			return _lazyOptional ?? (_lazyOptional = TryGetConstructor(CoreType.OptionalAttribute));
		}

		internal ConstructorInfo TryGetPreserveSigCtor()
		{
			return _lazyPreserveSig ?? (_lazyPreserveSig = TryGetConstructor(CoreType.PreserveSigAttribute));
		}

		internal ConstructorInfo TryGetComImportCtor()
		{
			return _lazyComImport ?? (_lazyComImport = TryGetConstructor(CoreType.ComImportAttribute));
		}

		internal ConstructorInfo TryGetDllImportCtor()
		{
			return _lazyDllImport ?? (_lazyDllImport = TryGetConstructor(CoreType.DllImportAttribute, CoreType.String));
		}

		internal ConstructorInfo TryGetMarshalAsCtor()
		{
			return _lazyMarshalAs ?? (_lazyMarshalAs = TryGetConstructor(CoreType.MarshalAsAttribute, CoreType.UnmanagedType));
		}

		private ConstructorInfo TryGetConstructor(CoreType attributeCoreType, params CoreType[] parameterCoreTypes)
		{
			int num = parameterCoreTypes.Length;
			Type type = TryGetCoreType(attributeCoreType);
			if (type == null)
			{
				return null;
			}
			Type[] array = new Type[num];
			for (int i = 0; i < num; i++)
			{
				if ((array[i] = TryGetCoreType(parameterCoreTypes[i])) == null)
				{
					return null;
				}
			}
			return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding, null, array, null);
		}

		private RoAssembly LoadFromStreamCore(Stream peStream)
		{
			PEReader pEReader = new PEReader(peStream);
			PEReader pEReader2 = pEReader;
			try
			{
				if (!pEReader.HasMetadata)
				{
					throw new BadImageFormatException(MDCFR.Properties.Resources.NoMetadataInPeImage);
				}
				string location = ((peStream is FileStream fileStream) ? (fileStream.Name ?? string.Empty) : string.Empty);
				MetadataReader metadataReader = pEReader.GetMetadataReader();
				RoAssembly roAssembly = new EcmaAssembly(this, pEReader, metadataReader, location);
				AssemblyNameData assemblyNameDataNoCopy = roAssembly.GetAssemblyNameDataNoCopy();
				byte[] array = assemblyNameDataNoCopy.PublicKeyToken ?? Array.Empty<byte>();
				if (array.Length == 0 && assemblyNameDataNoCopy.PublicKey != null && assemblyNameDataNoCopy.PublicKey.Length != 0)
				{
					array = assemblyNameDataNoCopy.PublicKey.ComputePublicKeyToken();
				}
				RoAssemblyName roAssemblyName = new RoAssemblyName(assemblyNameDataNoCopy.Name, assemblyNameDataNoCopy.Version, assemblyNameDataNoCopy.CultureName, array, assemblyNameDataNoCopy.Flags);
				RoAssembly orAdd = _loadedAssemblies.GetOrAdd(roAssemblyName, roAssembly);
				if (orAdd == roAssembly)
				{
					RegisterForDisposal(pEReader);
					pEReader2 = null;
					return orAdd;
				}
				if (roAssembly.ManifestModule.ModuleVersionId != orAdd.ManifestModule.ModuleVersionId)
				{
					throw new FileLoadException(System.SR.Format(MDCFR.Properties.Resources.FileLoadDuplicateAssemblies, roAssemblyName));
				}
				return orAdd;
			}
			finally
			{
				pEReader2?.Dispose();
			}
		}

		internal RoAssembly ResolveAssembly(RoAssemblyName refName)
		{
			Exception e;
			RoAssembly roAssembly = TryResolveAssembly(refName, out e);
			return roAssembly ?? throw e;
		}

		internal RoAssembly TryResolveAssembly(RoAssemblyName refName, out Exception e)
		{
			e = null;
			RoAssembly roAssembly = ResolveToAssemblyOrExceptionAssembly(refName);
			if (roAssembly is RoExceptionAssembly roExceptionAssembly)
			{
				e = roExceptionAssembly.Exception;
				return null;
			}
			return roAssembly;
		}

		internal RoAssembly ResolveToAssemblyOrExceptionAssembly(RoAssemblyName refName)
		{
			if (_binds.TryGetValue(refName, out var value))
			{
				return value;
			}
			RoAssembly value2 = TryFindAssemblyByCallingResolveHandler(refName);
			return _binds.GetOrAdd(refName, value2);
		}

		private RoAssembly TryFindAssemblyByCallingResolveHandler(RoAssemblyName refName)
		{
			Assembly assembly = resolver?.Resolve(this, refName.ToAssemblyName());
			if (assembly == null)
			{
				return new RoExceptionAssembly(new FileNotFoundException(System.SR.Format(MDCFR.Properties.Resources.FileNotFoundAssembly, refName.FullName)));
			}
			if (!(assembly is RoAssembly roAssembly) || roAssembly.Loader != this)
			{
				throw new FileLoadException(MDCFR.Properties.Resources.ExternalAssemblyReturnedByMetadataAssemblyResolver);
			}
			return roAssembly;
		}
	}

	internal abstract class NameFilter
	{
		protected string ExpectedName { get; }

		protected NameFilter(string expectedName)
		{
			ExpectedName = expectedName;
		}

		public abstract bool Matches(string name);

		public abstract bool Matches(StringHandle stringHandle, MetadataReader reader);
	}

	internal sealed class NameFilterCaseInsensitive : NameFilter
	{
		public NameFilterCaseInsensitive(string expectedName)
			: base(expectedName)
		{
		}

		public sealed override bool Matches(string name)
		{
			return name.Equals(base.ExpectedName, StringComparison.OrdinalIgnoreCase);
		}

		public sealed override bool Matches(StringHandle stringHandle, MetadataReader reader)
		{
			return reader.StringComparer.Equals(stringHandle, base.ExpectedName, ignoreCase: true);
		}
	}

	internal sealed class NameFilterCaseSensitive : NameFilter
	{
		private readonly byte[] _expectedNameUtf8;

		public NameFilterCaseSensitive(string expectedName)
			: base(expectedName)
		{
			_expectedNameUtf8 = expectedName.ToUtf8();
		}

		public sealed override bool Matches(string name)
		{
			return name.Equals(base.ExpectedName, StringComparison.Ordinal);
		}

		public sealed override bool Matches(StringHandle stringHandle, MetadataReader reader)
		{
			return MetadataExtensions.Equals(stringHandle, _expectedNameUtf8, reader);
		}
	}

	/// <summary>
	/// An assembly resolver that uses paths to every assembly that may be loaded.
	/// The file name is expected to be the same as the assembly's simple name.
	/// Multiple assemblies can exist on disk with the same name but in different directories.
	/// A single instance of PathAssemblyResolver can be used with multiple MetadataAssemblyResolver instances.
	/// </summary>
	/// <remarks>
	/// In order for an AssemblyName to match to a loaded assembly, AssemblyName.Name must be equal (casing ignored).
	/// - If AssemblyName.PublicKeyToken is specified, it must be equal.
	/// - If AssemblyName.PublicKeyToken is not specified, assemblies with no PublicKeyToken are selected over those with a PublicKeyToken.
	/// - If more than one assembly matches, the assembly with the highest Version is returned.
	/// - CultureName is ignored.
	/// </remarks>
	public class PathAssemblyResolver : MetadataAssemblyResolver
	{
		private readonly Dictionary<string, List<string>> _fileToPaths = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Reflection.PathAssemblyResolver" /> class.
		/// </summary>
		/// <exception cref="T:System.ArgumentNullException">Thrown when assemblyPaths is null.</exception>
		/// <exception cref="T:System.ArgumentException">Thrown when a path is invalid.</exception>
		public PathAssemblyResolver(IEnumerable<string> assemblyPaths)
		{
			if (assemblyPaths == null)
			{
				throw new ArgumentNullException("assemblyPaths");
			}
			foreach (string assemblyPath in assemblyPaths)
			{
				if (string.IsNullOrEmpty(assemblyPath))
				{
					throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.Arg_InvalidPath, assemblyPath), "assemblyPaths");
				}
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyPath);
				if (fileNameWithoutExtension.Length == 0)
				{
					throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.Arg_InvalidPath, assemblyPath), "assemblyPaths");
				}
				if (!_fileToPaths.TryGetValue(fileNameWithoutExtension, out var value))
				{
					_fileToPaths.Add(fileNameWithoutExtension, value = new List<string>());
				}
				value.Add(assemblyPath);
			}
		}
        #nullable enable
        public override Assembly? Resolve(MetadataLoadContext context, AssemblyName assemblyName)
		{
			Assembly assembly = null;
			Assembly assembly2 = null;
			if (_fileToPaths.TryGetValue(assemblyName.Name, out var value))
			{
				ReadOnlySpan<byte> span = assemblyName.GetPublicKeyToken();
				foreach (string item in value)
				{
					Assembly assembly3 = context.LoadFromAssemblyPath(item);
					AssemblyName name = assembly3.GetName();
					if (!assemblyName.Name.Equals(name.Name, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					ReadOnlySpan<byte> other = name.GetPublicKeyToken();
					if (span.SequenceEqual(other))
					{
						if (assembly == null || name.Version > assembly.GetName().Version)
						{
							assembly = assembly3;
						}
					}
					else if (((assembly == null && span.IsEmpty) || (assemblyName.Flags & AssemblyNameFlags.Retargetable) != 0) && (assembly2 == null || name.Version > assembly2.GetName().Version))
					{
						assembly2 = assembly3;
					}
				}
			}
			return assembly ?? assembly2;
		}
		#nullable disable
    }

    internal static class SignatureTypeExtensions
	{
		/// <summary>
		/// This is semantically identical to
		///
		///      parameter.ParameterType == pattern.TryResolveAgainstGenericMethod(parameter.Member)
		///
		/// but without the allocation overhead of TryResolve.
		/// </summary>
		public static bool MatchesParameterTypeExactly(this Type pattern, ParameterInfo parameter)
		{
			if (pattern.IsSignatureType())
			{
				return pattern.MatchesExactly(parameter.ParameterType);
			}
			return (object)pattern == parameter.ParameterType;
		}

		/// <summary>
		/// This is semantically identical to
		///
		///      actual == pattern.TryResolveAgainstGenericMethod(parameterMember)
		///
		/// but without the allocation overhead of TryResolve.
		/// </summary>
		internal static bool MatchesExactly(this Type pattern, Type actual)
		{
			if (pattern.IsSZArray())
			{
				if (actual.IsSZArray())
				{
					return pattern.GetElementType().MatchesExactly(actual.GetElementType());
				}
				return false;
			}
			if (pattern.IsVariableBoundArray())
			{
				if (actual.IsVariableBoundArray() && pattern.GetArrayRank() == actual.GetArrayRank())
				{
					return pattern.GetElementType().MatchesExactly(actual.GetElementType());
				}
				return false;
			}
			if (pattern.IsByRef)
			{
				if (actual.IsByRef)
				{
					return pattern.GetElementType().MatchesExactly(actual.GetElementType());
				}
				return false;
			}
			if (pattern.IsPointer)
			{
				if (actual.IsPointer)
				{
					return pattern.GetElementType().MatchesExactly(actual.GetElementType());
				}
				return false;
			}
			if (pattern.IsConstructedGenericType)
			{
				if (!actual.IsConstructedGenericType)
				{
					return false;
				}
				if (!(pattern.GetGenericTypeDefinition() == actual.GetGenericTypeDefinition()))
				{
					return false;
				}
				Type[] genericTypeArguments = pattern.GenericTypeArguments;
				Type[] genericTypeArguments2 = actual.GenericTypeArguments;
				int num = genericTypeArguments.Length;
				if (num != genericTypeArguments2.Length)
				{
					return false;
				}
				for (int i = 0; i < num; i++)
				{
					Type type = genericTypeArguments[i];
					if (type.IsSignatureType())
					{
						if (!type.MatchesExactly(genericTypeArguments2[i]))
						{
							return false;
						}
					}
					else if (type != genericTypeArguments2[i])
					{
						return false;
					}
				}
				return true;
			}
			if (pattern.IsGenericMethodParameter())
			{
				if (!actual.IsGenericMethodParameter())
				{
					return false;
				}
				if (pattern.GenericParameterPosition != actual.GenericParameterPosition)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Translates a SignatureType into its equivalent resolved Type by recursively substituting all generic parameter references
		/// with its corresponding generic parameter definition. This is slow so MatchesExactly or MatchesParameterTypeExactly should be
		/// substituted instead whenever possible. This is only used by the DefaultBinder when its fast-path checks have been exhausted and
		/// it needs to call non-trivial methods like IsAssignableFrom which SignatureTypes will never support.
		///
		/// Because this method is used to eliminate method candidates in a GetMethod() lookup, it is entirely possible that the Type
		/// might not be creatable due to conflicting generic constraints. Since this merely implies that this candidate is not
		/// the method we're looking for, we return null rather than let the TypeLoadException bubble up. The DefaultBinder will catch
		/// the null and continue its search for a better candidate.
		/// </summary>
		internal static Type TryResolveAgainstGenericMethod(this Type signatureType, MethodInfo genericMethod)
		{
			return signatureType.TryResolve(genericMethod.GetGenericArguments());
		}

		private static Type TryResolve(this Type signatureType, Type[] genericMethodParameters)
		{
			if (signatureType.IsSZArray())
			{
				Type type = signatureType.GetElementType().TryResolve(genericMethodParameters);
				if ((object)type == null)
				{
					return null;
				}
				return type.TryMakeArrayType();
			}
			if (signatureType.IsVariableBoundArray())
			{
				Type type2 = signatureType.GetElementType().TryResolve(genericMethodParameters);
				if ((object)type2 == null)
				{
					return null;
				}
				return type2.TryMakeArrayType(signatureType.GetArrayRank());
			}
			if (signatureType.IsByRef)
			{
				Type type3 = signatureType.GetElementType().TryResolve(genericMethodParameters);
				if ((object)type3 == null)
				{
					return null;
				}
				return type3.TryMakeByRefType();
			}
			if (signatureType.IsPointer)
			{
				Type type4 = signatureType.GetElementType().TryResolve(genericMethodParameters);
				if ((object)type4 == null)
				{
					return null;
				}
				return type4.TryMakePointerType();
			}
			if (signatureType.IsConstructedGenericType)
			{
				Type[] genericTypeArguments = signatureType.GenericTypeArguments;
				int num = genericTypeArguments.Length;
				Type[] array = new Type[num];
				for (int i = 0; i < num; i++)
				{
					Type type5 = genericTypeArguments[i];
					if (type5.IsSignatureType())
					{
						array[i] = type5.TryResolve(genericMethodParameters);
						if (array[i] == null)
						{
							return null;
						}
					}
					else
					{
						array[i] = type5;
					}
				}
				return signatureType.GetGenericTypeDefinition().TryMakeGenericType(array);
			}
			if (signatureType.IsGenericMethodParameter())
			{
				int genericParameterPosition = signatureType.GenericParameterPosition;
				if (genericParameterPosition >= genericMethodParameters.Length)
				{
					return null;
				}
				return genericMethodParameters[genericParameterPosition];
			}
			return null;
		}

		private static Type TryMakeArrayType(this Type type)
		{
			try
			{
				return type.MakeArrayType();
			}
			catch
			{
				return null;
			}
		}

		private static Type TryMakeArrayType(this Type type, int rank)
		{
			try
			{
				return type.MakeArrayType(rank);
			}
			catch
			{
				return null;
			}
		}

		private static Type TryMakeByRefType(this Type type)
		{
			try
			{
				return type.MakeByRefType();
			}
			catch
			{
				return null;
			}
		}

		private static Type TryMakePointerType(this Type type)
		{
			try
			{
				return type.MakePointerType();
			}
			catch
			{
				return null;
			}
		}

		private static Type TryMakeGenericType(this Type type, Type[] instantiation)
		{
			try
			{
				return type.MakeGenericType(instantiation);
			}
			catch
			{
				return null;
			}
		}
	}
	
	namespace Runtime
	{
		namespace BindingFlagSupport
		{
			using System.Reflection.Runtime.TypeInfos;
			/// <summary>
			/// Policies for constructors
			/// </summary>
			internal sealed class ConstructorPolicies : MemberPolicies<ConstructorInfo>
			{
			public sealed override bool AlwaysTreatAsDeclaredOnly => true;

			public sealed override IEnumerable<ConstructorInfo> GetDeclaredMembers(TypeInfo typeInfo)
			{
				return typeInfo.DeclaredConstructors;
			}

			public sealed override IEnumerable<ConstructorInfo> CoreGetDeclaredMembers(RoType type, NameFilter filter, RoType reflectedType)
			{
				return type.GetConstructorsCore(filter);
			}

			public sealed override BindingFlags ModifyBindingFlags(BindingFlags bindingFlags)
			{
				return bindingFlags | BindingFlags.DeclaredOnly;
			}

			public sealed override void GetMemberAttributes(ConstructorInfo member, out MethodAttributes visibility, out bool isStatic, out bool isVirtual, out bool isNewSlot)
			{
				MethodAttributes attributes = member.Attributes;
				visibility = attributes & MethodAttributes.MemberAccessMask;
				isStatic = (attributes & MethodAttributes.Static) != 0;
				isVirtual = false;
				isNewSlot = false;
			}

			public sealed override bool ImplicitlyOverrides(ConstructorInfo baseMember, ConstructorInfo derivedMember)
			{
				return false;
			}

			public sealed override bool IsSuppressedByMoreDerivedMember(ConstructorInfo member, ConstructorInfo[] priorMembers, int startIndex, int endIndex)
			{
				return false;
			}

			public sealed override bool OkToIgnoreAmbiguity(ConstructorInfo m1, ConstructorInfo m2)
			{
				throw new NotSupportedException();
			}
		}

			/// <summary>
			/// Policies for events.
			/// </summary>
			internal sealed class EventPolicies : MemberPolicies<EventInfo>
			{
				public sealed override bool AlwaysTreatAsDeclaredOnly => false;

				public sealed override IEnumerable<EventInfo> GetDeclaredMembers(TypeInfo typeInfo)
				{
					return typeInfo.DeclaredEvents;
				}

				public sealed override IEnumerable<EventInfo> CoreGetDeclaredMembers(RoType type, NameFilter filter, RoType reflectedType)
				{
					return type.GetEventsCore(filter, reflectedType);
				}

				public sealed override void GetMemberAttributes(EventInfo member, out MethodAttributes visibility, out bool isStatic, out bool isVirtual, out bool isNewSlot)
				{
					MethodInfo accessorMethod = GetAccessorMethod(member);
					if (accessorMethod == null)
					{
						visibility = MethodAttributes.Private;
						isStatic = false;
						isVirtual = false;
						isNewSlot = true;
					}
					else
					{
						MethodAttributes attributes = accessorMethod.Attributes;
						visibility = attributes & MethodAttributes.MemberAccessMask;
						isStatic = (attributes & MethodAttributes.Static) != 0;
						isVirtual = (attributes & MethodAttributes.Virtual) != 0;
						isNewSlot = (attributes & MethodAttributes.VtableLayoutMask) != 0;
					}
				}

				public sealed override bool IsSuppressedByMoreDerivedMember(EventInfo member, EventInfo[] priorMembers, int startIndex, int endIndex)
				{
					for (int i = startIndex; i < endIndex; i++)
					{
						if (priorMembers[i].Name == member.Name)
						{
							return true;
						}
					}
					return false;
				}

				public sealed override bool ImplicitlyOverrides(EventInfo baseMember, EventInfo derivedMember)
				{
					MethodInfo accessorMethod = GetAccessorMethod(baseMember);
					MethodInfo accessorMethod2 = GetAccessorMethod(derivedMember);
					return MemberPolicies<MethodInfo>.Default.ImplicitlyOverrides(accessorMethod, accessorMethod2);
				}

				public sealed override bool OkToIgnoreAmbiguity(EventInfo m1, EventInfo m2)
				{
					return false;
				}

				private static MethodInfo GetAccessorMethod(EventInfo e)
				{
					return e.AddMethod;
				}
			}

			/// <summary>
			/// Policies for fields.
			/// </summary>
			internal sealed class FieldPolicies : MemberPolicies<FieldInfo>
			{
				public sealed override bool AlwaysTreatAsDeclaredOnly => false;

				public sealed override IEnumerable<FieldInfo> GetDeclaredMembers(TypeInfo typeInfo)
				{
					return typeInfo.DeclaredFields;
				}

				public sealed override IEnumerable<FieldInfo> CoreGetDeclaredMembers(RoType type, NameFilter filter, RoType reflectedType)
				{
					return type.GetFieldsCore(filter, reflectedType);
				}

				public sealed override void GetMemberAttributes(FieldInfo member, out MethodAttributes visibility, out bool isStatic, out bool isVirtual, out bool isNewSlot)
				{
					FieldAttributes attributes = member.Attributes;
					visibility = (MethodAttributes)(attributes & FieldAttributes.FieldAccessMask);
					isStatic = (attributes & FieldAttributes.Static) != 0;
					isVirtual = false;
					isNewSlot = false;
				}

				public sealed override bool ImplicitlyOverrides(FieldInfo baseMember, FieldInfo derivedMember)
				{
					return false;
				}

				public sealed override bool IsSuppressedByMoreDerivedMember(FieldInfo member, FieldInfo[] priorMembers, int startIndex, int endIndex)
				{
					return false;
				}

				public sealed override bool OkToIgnoreAmbiguity(FieldInfo m1, FieldInfo m2)
				{
					return true;
				}
			}

			/// <summary>
			/// This class encapsulates the minimum set of arcane .NET Framework CLR policies needed to implement the Get*(BindingFlags) apis.
			/// In particular, it encapsulates behaviors such as what exactly determines the "visibility" of a property and event, and
			/// what determines whether and how they are overridden.
			/// </summary>
			internal abstract class MemberPolicies<M> where M : MemberInfo
			{
				public static readonly MemberPolicies<M> Default;

				public static readonly int MemberTypeIndex;

				public abstract bool AlwaysTreatAsDeclaredOnly { get; }

				public abstract IEnumerable<M> GetDeclaredMembers(TypeInfo typeInfo);

				public abstract IEnumerable<M> CoreGetDeclaredMembers(RoType type, NameFilter filter, RoType reflectedType);

				public abstract void GetMemberAttributes(M member, out MethodAttributes visibility, out bool isStatic, out bool isVirtual, out bool isNewSlot);

				public abstract bool ImplicitlyOverrides(M baseMember, M derivedMember);

				public virtual BindingFlags ModifyBindingFlags(BindingFlags bindingFlags)
				{
					return bindingFlags;
				}

				public abstract bool IsSuppressedByMoreDerivedMember(M member, M[] priorMembers, int startIndex, int endIndex);

				public abstract bool OkToIgnoreAmbiguity(M m1, M m2);

				protected static bool AreNamesAndSignaturesEqual(MethodInfo method1, MethodInfo method2)
				{
					if (method1.Name != method2.Name)
					{
						return false;
					}
					ParameterInfo[] parametersNoCopy = method1.GetParametersNoCopy();
					ParameterInfo[] parametersNoCopy2 = method2.GetParametersNoCopy();
					if (parametersNoCopy.Length != parametersNoCopy2.Length)
					{
						return false;
					}
					bool isGenericMethodDefinition = method1.IsGenericMethodDefinition;
					bool isGenericMethodDefinition2 = method2.IsGenericMethodDefinition;
					if (isGenericMethodDefinition != isGenericMethodDefinition2)
					{
						return false;
					}
					if (!isGenericMethodDefinition)
					{
						for (int i = 0; i < parametersNoCopy.Length; i++)
						{
							Type parameterType = parametersNoCopy[i].ParameterType;
							Type parameterType2 = parametersNoCopy2[i].ParameterType;
							if (!parameterType.Equals(parameterType2))
							{
								return false;
							}
						}
					}
					else
					{
						if (method1.GetGenericArguments().Length != method2.GetGenericArguments().Length)
						{
							return false;
						}
						for (int j = 0; j < parametersNoCopy.Length; j++)
						{
							Type parameterType3 = parametersNoCopy[j].ParameterType;
							Type parameterType4 = parametersNoCopy2[j].ParameterType;
							if (!GenericMethodAwareAreParameterTypesEqual(parameterType3, parameterType4))
							{
								return false;
							}
						}
					}
					return true;
				}

				private static bool GenericMethodAwareAreParameterTypesEqual(Type t1, Type t2)
				{
					if (t1.Equals(t2))
					{
						return true;
					}
					if (!t1.ContainsGenericParameters || !t2.ContainsGenericParameters)
					{
						return false;
					}
					if ((t1.IsArray && t2.IsArray) || (t1.IsByRef && t2.IsByRef) || (t1.IsPointer && t2.IsPointer))
					{
						if (t1.IsSZArray() != t2.IsSZArray())
						{
							return false;
						}
						if (t1.IsArray && t1.GetArrayRank() != t2.GetArrayRank())
						{
							return false;
						}
						return GenericMethodAwareAreParameterTypesEqual(t1.GetElementType(), t2.GetElementType());
					}
					if (t1.IsConstructedGenericType)
					{
						if (!t1.GetGenericTypeDefinition().Equals(t2.GetGenericTypeDefinition()))
						{
							return false;
						}
						Type[] genericTypeArguments = t1.GenericTypeArguments;
						Type[] genericTypeArguments2 = t2.GenericTypeArguments;
						if (genericTypeArguments.Length != genericTypeArguments2.Length)
						{
							return false;
						}
						for (int i = 0; i < genericTypeArguments.Length; i++)
						{
							if (!GenericMethodAwareAreParameterTypesEqual(genericTypeArguments[i], genericTypeArguments2[i]))
							{
								return false;
							}
						}
						return true;
					}
					if (t1.IsGenericMethodParameter() && t2.IsGenericMethodParameter())
					{
						return t1.GenericParameterPosition == t2.GenericParameterPosition;
					}
					return false;
				}

				static MemberPolicies()
				{
					Type typeFromHandle = typeof(M);
					if (typeFromHandle.Equals(typeof(FieldInfo)))
					{
						MemberTypeIndex = 2;
						Default = (MemberPolicies<M>)(object)new FieldPolicies();
					}
					else if (typeFromHandle.Equals(typeof(MethodInfo)))
					{
						MemberTypeIndex = 3;
						Default = (MemberPolicies<M>)(object)new MethodPolicies();
					}
					else if (typeFromHandle.Equals(typeof(ConstructorInfo)))
					{
						MemberTypeIndex = 0;
						Default = (MemberPolicies<M>)(object)new ConstructorPolicies();
					}
					else if (typeFromHandle.Equals(typeof(PropertyInfo)))
					{
						MemberTypeIndex = 5;
						Default = (MemberPolicies<M>)(object)new PropertyPolicies();
					}
					else if (typeFromHandle.Equals(typeof(EventInfo)))
					{
						MemberTypeIndex = 1;
						Default = (MemberPolicies<M>)(object)new EventPolicies();
					}
					else if (typeFromHandle.Equals(typeof(Type)))
					{
						MemberTypeIndex = 4;
						Default = (MemberPolicies<M>)(object)new NestedTypePolicies();
					}
				}
			}

			internal static class MemberTypeIndex
			{
				public const int Constructor = 0;

				public const int Event = 1;

				public const int Field = 2;

				public const int Method = 3;

				public const int NestedType = 4;

				public const int Property = 5;

				public const int Count = 6;
			}

			/// <summary>
			/// Policies for methods.
			/// </summary>
			internal sealed class MethodPolicies : MemberPolicies<MethodInfo>
			{
				public sealed override bool AlwaysTreatAsDeclaredOnly => false;

				public sealed override IEnumerable<MethodInfo> GetDeclaredMembers(TypeInfo typeInfo)
				{
					return typeInfo.DeclaredMethods;
				}

				public sealed override IEnumerable<MethodInfo> CoreGetDeclaredMembers(RoType type, NameFilter filter, RoType reflectedType)
				{
					return type.GetMethodsCore(filter, reflectedType);
				}

				public sealed override void GetMemberAttributes(MethodInfo member, out MethodAttributes visibility, out bool isStatic, out bool isVirtual, out bool isNewSlot)
				{
					MethodAttributes attributes = member.Attributes;
					visibility = attributes & MethodAttributes.MemberAccessMask;
					isStatic = (attributes & MethodAttributes.Static) != 0;
					isVirtual = (attributes & MethodAttributes.Virtual) != 0;
					isNewSlot = (attributes & MethodAttributes.VtableLayoutMask) != 0;
				}

				public sealed override bool ImplicitlyOverrides(MethodInfo baseMember, MethodInfo derivedMember)
				{
					return MemberPolicies<MethodInfo>.AreNamesAndSignaturesEqual(baseMember, derivedMember);
				}

				public sealed override bool IsSuppressedByMoreDerivedMember(MethodInfo member, MethodInfo[] priorMembers, int startIndex, int endIndex)
				{
					if (!member.IsVirtual)
					{
						return false;
					}
					for (int i = startIndex; i < endIndex; i++)
					{
						MethodInfo methodInfo = priorMembers[i];
						MethodAttributes methodAttributes = methodInfo.Attributes & (MethodAttributes.Virtual | MethodAttributes.VtableLayoutMask);
						if (methodAttributes == MethodAttributes.Virtual && ImplicitlyOverrides(member, methodInfo))
						{
							return true;
						}
					}
					return false;
				}

				public sealed override bool OkToIgnoreAmbiguity(MethodInfo m1, MethodInfo m2)
				{
					return System.DefaultBinder.CompareMethodSig(m1, m2);
				}
			}

			/// <summary>
			/// Policies for nested types.
			///
			/// Nested types enumerate a little differently than other members:
			///    Base classes are never searched, regardless of BindingFlags.DeclaredOnly value.
			///    Public|NonPublic|IgnoreCase are the only relevant BindingFlags. The apis ignore any other bits.
			///    There is no such thing as a "static" or "instanced" nested type. For enumeration purposes,
			///    we'll arbitrarily denote all nested types as "static."
			/// </summary>
			internal sealed class NestedTypePolicies : MemberPolicies<Type>
			{
				public sealed override bool AlwaysTreatAsDeclaredOnly => true;

				public sealed override IEnumerable<Type> GetDeclaredMembers(TypeInfo typeInfo)
				{
					return typeInfo.DeclaredNestedTypes;
				}

				public sealed override IEnumerable<Type> CoreGetDeclaredMembers(RoType type, NameFilter filter, RoType reflectedType)
				{
					return type.GetNestedTypesCore(filter);
				}

				public sealed override void GetMemberAttributes(Type member, out MethodAttributes visibility, out bool isStatic, out bool isVirtual, out bool isNewSlot)
				{
					isStatic = true;
					isVirtual = false;
					isNewSlot = false;
					visibility = ((!member.IsNestedPublic) ? MethodAttributes.Private : MethodAttributes.Public);
				}

				public sealed override bool ImplicitlyOverrides(Type baseMember, Type derivedMember)
				{
					return false;
				}

				public sealed override bool IsSuppressedByMoreDerivedMember(Type member, Type[] priorMembers, int startIndex, int endIndex)
				{
					return false;
				}

				public sealed override BindingFlags ModifyBindingFlags(BindingFlags bindingFlags)
				{
					bindingFlags &= BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic;
					bindingFlags |= BindingFlags.DeclaredOnly | BindingFlags.Static;
					return bindingFlags;
				}

				public sealed override bool OkToIgnoreAmbiguity(Type m1, Type m2)
				{
					return false;
				}
			}

			/// <summary>
			/// Policies for properties.
			/// </summary>
			internal sealed class PropertyPolicies : MemberPolicies<PropertyInfo>
			{
				public sealed override bool AlwaysTreatAsDeclaredOnly => false;

				public sealed override IEnumerable<PropertyInfo> GetDeclaredMembers(TypeInfo typeInfo)
				{
					return typeInfo.DeclaredProperties;
				}

				public sealed override IEnumerable<PropertyInfo> CoreGetDeclaredMembers(RoType type, NameFilter filter, RoType reflectedType)
				{
					return type.GetPropertiesCore(filter, reflectedType);
				}

				public sealed override void GetMemberAttributes(PropertyInfo member, out MethodAttributes visibility, out bool isStatic, out bool isVirtual, out bool isNewSlot)
				{
					MethodInfo accessorMethod = GetAccessorMethod(member);
					if (accessorMethod == null)
					{
						visibility = MethodAttributes.Private;
						isStatic = false;
						isVirtual = false;
						isNewSlot = true;
					}
					else
					{
						MethodAttributes attributes = accessorMethod.Attributes;
						visibility = attributes & MethodAttributes.MemberAccessMask;
						isStatic = (attributes & MethodAttributes.Static) != 0;
						isVirtual = (attributes & MethodAttributes.Virtual) != 0;
						isNewSlot = (attributes & MethodAttributes.VtableLayoutMask) != 0;
					}
				}

				public sealed override bool ImplicitlyOverrides(PropertyInfo baseMember, PropertyInfo derivedMember)
				{
					MethodInfo accessorMethod = GetAccessorMethod(baseMember);
					MethodInfo accessorMethod2 = GetAccessorMethod(derivedMember);
					return MemberPolicies<MethodInfo>.Default.ImplicitlyOverrides(accessorMethod, accessorMethod2);
				}

				public sealed override bool IsSuppressedByMoreDerivedMember(PropertyInfo member, PropertyInfo[] priorMembers, int startIndex, int endIndex)
				{
					MethodInfo accessorMethod = GetAccessorMethod(member);
					for (int i = startIndex; i < endIndex; i++)
					{
						PropertyInfo propertyInfo = priorMembers[i];
						MethodInfo accessorMethod2 = GetAccessorMethod(propertyInfo);
						if (MemberPolicies<PropertyInfo>.AreNamesAndSignaturesEqual(accessorMethod, accessorMethod2) && accessorMethod2.IsStatic == accessorMethod.IsStatic && propertyInfo.PropertyType.Equals(member.PropertyType))
						{
							return true;
						}
					}
					return false;
				}

				public sealed override bool OkToIgnoreAmbiguity(PropertyInfo m1, PropertyInfo m2)
				{
					return false;
				}

				private static MethodInfo GetAccessorMethod(PropertyInfo property)
				{
					return property.GetMethod ?? property.SetMethod;
				}
			}

			internal sealed class QueriedMemberList<M> where M : MemberInfo
			{
				private int _totalCount;

				private int _declaredOnlyCount;

				private M[] _members;

				private BindingFlags[] _allFlagsThatMustMatch;

				private RoType _typeThatBlockedBrowsing;

				private const int Grow = 64;

				/// <summary>
				/// Returns the # of candidates for a non-DeclaredOnly search. Caution: Can throw MissingMetadataException. Use DeclaredOnlyCount if you don't want to search base classes.
				/// </summary>
				public int TotalCount
				{
					get
					{
						if (_typeThatBlockedBrowsing != null)
						{
							throw global::Internal.Reflection.Core.Execution.ReflectionCoreExecution.
								ExecutionDomain.CreateMissingMetadataException(_typeThatBlockedBrowsing);
						}
						return _totalCount;
					}
				}

				/// <summary>
				/// Returns the # of candidates for a DeclaredOnly search
				/// </summary>
				public int DeclaredOnlyCount => _declaredOnlyCount;

				public bool ImmediateTypeOnly { get; }

				public M this[int index]
				{
					[MethodImpl(MethodImplOptions.AggressiveInlining)]
					get
					{
						return _members[index];
					}
				}

				private QueriedMemberList(bool immediateTypeOnly)
				{
					_members = new M[64];
					_allFlagsThatMustMatch = new BindingFlags[64];
					ImmediateTypeOnly = immediateTypeOnly;
				}

				private QueriedMemberList(int totalCount, int declaredOnlyCount, M[] members, BindingFlags[] allFlagsThatMustMatch, RoType typeThatBlockedBrowsing)
				{
					_totalCount = totalCount;
					_declaredOnlyCount = declaredOnlyCount;
					_members = members;
					_allFlagsThatMustMatch = allFlagsThatMustMatch;
					_typeThatBlockedBrowsing = typeThatBlockedBrowsing;
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				public bool Matches(int index, BindingFlags bindingAttr)
				{
					BindingFlags bindingFlags = _allFlagsThatMustMatch[index];
					return (bindingAttr & bindingFlags) == bindingFlags;
				}

				public QueriedMemberList<M> Filter(Func<M, bool> predicate)
				{
					BindingFlags[] array = new BindingFlags[_totalCount];
					M[] array2 = new M[_totalCount];
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < _totalCount; i++)
					{
						M val = _members[i];
						if (predicate(val))
						{
							array2[num2] = val;
							array[num2] = _allFlagsThatMustMatch[i];
							num2++;
							if (i < _declaredOnlyCount)
							{
								num++;
							}
						}
					}
					return new QueriedMemberList<M>(num2, num, array2, array, _typeThatBlockedBrowsing);
				}

				public static QueriedMemberList<M> Create(RoType type, string filter, bool ignoreCase, bool immediateTypeOnly)
				{
					RoType reflectedType = type;
					MemberPolicies<M> @default = MemberPolicies<M>.Default;
					NameFilter filter2 = ((filter == null) ? null : ((!ignoreCase) ? ((NameFilter)new NameFilterCaseSensitive(filter)) : ((NameFilter)new NameFilterCaseInsensitive(filter))));
					bool flag = false;
					QueriedMemberList<M> queriedMemberList = new QueriedMemberList<M>(immediateTypeOnly);
					while (type != null)
					{
						int totalCount = queriedMemberList._totalCount;
						foreach (M item in @default.CoreGetDeclaredMembers(type, filter2, reflectedType))
						{
							@default.GetMemberAttributes(item, out var visibility, out var isStatic, out var _, out var _);
							if ((!flag || visibility != MethodAttributes.Private) && (totalCount == 0 || !@default.IsSuppressedByMoreDerivedMember(item, queriedMemberList._members, 0, totalCount)))
							{
								BindingFlags bindingFlags = BindingFlags.Default;
								bindingFlags |= (isStatic ? BindingFlags.Static : BindingFlags.Instance);
								if (isStatic && flag)
								{
									bindingFlags |= BindingFlags.FlattenHierarchy;
								}
								bindingFlags |= ((visibility == MethodAttributes.Public) ? BindingFlags.Public : BindingFlags.NonPublic);
								queriedMemberList.Add(item, bindingFlags);
							}
						}
						if (!flag)
						{
							queriedMemberList._declaredOnlyCount = queriedMemberList._totalCount;
							if (immediateTypeOnly || @default.AlwaysTreatAsDeclaredOnly)
							{
								break;
							}
							flag = true;
						}
						type = type.BaseType.CastToRuntimeTypeInfo();
						if (type != null && !type.CanBrowseWithoutMissingMetadataExceptions())
						{
							queriedMemberList._typeThatBlockedBrowsing = type;
							queriedMemberList._totalCount = queriedMemberList._declaredOnlyCount;
							break;
						}
					}
					return queriedMemberList;
				}

				public void Compact()
				{
					Array.Resize(ref _members, _totalCount);
					Array.Resize(ref _allFlagsThatMustMatch, _totalCount);
				}

				private void Add(M member, BindingFlags allFlagsThatMustMatch)
				{
					int totalCount = _totalCount;
					if (totalCount == _members.Length)
					{
						Array.Resize(ref _members, totalCount + 64);
						Array.Resize(ref _allFlagsThatMustMatch, totalCount + 64);
					}
					_members[totalCount] = member;
					_allFlagsThatMustMatch[totalCount] = allFlagsThatMustMatch;
					_totalCount++;
				}
			}

			internal struct QueryResult<M> where M : MemberInfo
			{
				internal struct QueryResultEnumerator
				{
					private int _index;

					private readonly int _unfilteredCount;

					private readonly BindingFlags _bindingAttr;

					private readonly QueriedMemberList<M> _queriedMembers;

					public M Current
					{
						[MethodImpl(MethodImplOptions.AggressiveInlining)]
						get
						{
							return _queriedMembers[_index];
						}
					}

					public QueryResultEnumerator(QueryResult<M> queryResult)
					{
						_bindingAttr = queryResult._bindingAttr;
						_unfilteredCount = queryResult.UnfilteredCount;
						_queriedMembers = queryResult._queriedMembers;
						_index = -1;
					}

					public bool MoveNext()
					{
						while (++_index < _unfilteredCount && !_queriedMembers.Matches(_index, _bindingAttr))
						{
						}
						if (_index < _unfilteredCount)
						{
							return true;
						}
						_index = _unfilteredCount;
						return false;
					}
				}

				private readonly BindingFlags _bindingAttr;

				private int _lazyCount;

				private QueriedMemberList<M> _queriedMembers;

				/// <summary>
				/// Returns the number of matching results.
				/// </summary>
				public int Count
				{
					get
					{
						int num = _lazyCount;
						if (num == 0)
						{
							if (_queriedMembers == null)
							{
								return 0;
							}
							int unfilteredCount = UnfilteredCount;
							for (int i = 0; i < unfilteredCount; i++)
							{
								if (_queriedMembers.Matches(i, _bindingAttr))
								{
									num++;
								}
							}
							if (num == 0)
							{
								_queriedMembers = null;
							}
							_lazyCount = num;
						}
						return num;
					}
				}

				private int UnfilteredCount
				{
					get
					{
						if ((_bindingAttr & BindingFlags.DeclaredOnly) == 0)
						{
							return _queriedMembers.TotalCount;
						}
						return _queriedMembers.DeclaredOnlyCount;
					}
				}

				public QueryResult(BindingFlags bindingAttr, QueriedMemberList<M> queriedMembers)
				{
					_lazyCount = 0;
					_bindingAttr = bindingAttr;
					_queriedMembers = queriedMembers;
				}

				public QueryResultEnumerator GetEnumerator()
				{
					return new QueryResultEnumerator(this);
				}

				/// <summary>
				/// Copies the results to a freshly allocated array. Use this at api boundary points.
				/// </summary>
				public M[] ToArray()
				{
					int count = Count;
					if (count == 0)
					{
						return Array.Empty<M>();
					}
					M[] array = new M[count];
					MemberInfo[] array2 = array;
					CopyTo(array2, 0);
					return array;
				}

				/// <summary>
				/// Copies the results into an existing array.
				/// </summary>
				public void CopyTo(MemberInfo[] array, int startIndex)
				{
					if (_queriedMembers == null)
					{
						return;
					}
					int unfilteredCount = UnfilteredCount;
					for (int i = 0; i < unfilteredCount; i++)
					{
						if (_queriedMembers.Matches(i, _bindingAttr))
						{
							array[startIndex++] = _queriedMembers[i];
						}
					}
				}

				/// <summary>
				/// Returns a single member, null or throws AmbiguousMatchException, for the Type.Get*(string name,...) family of apis.
				/// </summary>
				public M Disambiguate()
				{
					if (_queriedMembers == null)
					{
						return null;
					}
					int unfilteredCount = UnfilteredCount;
					M val = null;
					for (int i = 0; i < unfilteredCount; i++)
					{
						if (!_queriedMembers.Matches(i, _bindingAttr))
						{
							continue;
						}
						if ((MemberInfo)val != (MemberInfo)null)
						{
							M val2 = _queriedMembers[i];
							if (val.DeclaringType.Equals(val2.DeclaringType))
							{
								throw new AmbiguousMatchException();
							}
							MemberPolicies<M> @default = MemberPolicies<M>.Default;
							if (!@default.OkToIgnoreAmbiguity(val, val2))
							{
								throw new AmbiguousMatchException();
							}
						}
						else
						{
							val = _queriedMembers[i];
						}
					}
					return val;
				}
			}

			internal static class Shared
			{
				public static bool QualifiesBasedOnParameterCount(this MethodBase methodBase, BindingFlags bindingFlags, CallingConventions callConv, Type[] argumentTypes)
				{
					if ((callConv & CallingConventions.Any) == 0)
					{
						if ((callConv & CallingConventions.VarArgs) != 0 && (methodBase.CallingConvention & CallingConventions.VarArgs) == 0)
						{
							return false;
						}
						if ((callConv & CallingConventions.Standard) != 0 && (methodBase.CallingConvention & CallingConventions.Standard) == 0)
						{
							return false;
						}
					}
					ParameterInfo[] parametersNoCopy = methodBase.GetParametersNoCopy();
					if (argumentTypes.Length != parametersNoCopy.Length)
					{
						if ((bindingFlags & (BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetProperty | BindingFlags.SetProperty)) == 0)
						{
							return false;
						}
						throw new InvalidOperationException(MDCFR.Properties.Resources.NoInvokeMember);
					}
					if ((bindingFlags & BindingFlags.ExactBinding) != 0 && (bindingFlags & BindingFlags.InvokeMethod) == 0)
					{
						for (int i = 0; i < parametersNoCopy.Length; i++)
						{
							if ((object)argumentTypes[i] != null && !argumentTypes[i].MatchesParameterTypeExactly(parametersNoCopy[i]))
							{
								return false;
							}
						}
					}
					return true;
				}
			}
		}

		namespace General
		{
			internal struct ListBuilder<T> where T : class
			{
				private T[] _items;

				private T _item;

				private int _count;

				private int _capacity;

				public T this[int index]
				{
					get
					{
						if (_items == null)
						{
							return _item;
						}
						return _items[index];
					}
				}

				public int Count => _count;

				public ListBuilder(int capacity)
				{
					_items = null;
					_item = null;
					_count = 0;
					_capacity = capacity;
				}

				public T[] ToArray()
				{
					if (_count == 0)
					{
						return Array.Empty<T>();
					}
					if (_count == 1)
					{
						return new T[1] { _item };
					}
					Array.Resize(ref _items, _count);
					return _items;
				}

				public void CopyTo(object[] array, int index)
				{
					if (_count != 0)
					{
						if (_count == 1)
						{
							array[index] = _item;
						}
						else
						{
							Array.Copy(_items, 0, array, index, _count);
						}
					}
				}

				public void Add(T item)
				{
					if (_count == 0)
					{
						_item = item;
					}
					else
					{
						if (_count == 1)
						{
							if (_capacity < 2)
							{
								_capacity = 4;
							}
							_items = new T[_capacity];
							_items[0] = _item;
						}
						else if (_capacity == _count)
						{
							int num = 2 * _capacity;
							Array.Resize(ref _items, num);
							_capacity = num;
						}
						_items[_count] = item;
					}
					_count++;
				}
			}
		}
		
		namespace TypeInfos
		{
			using System.Configuration.Assemblies;

			internal static class RoShims
			{
				internal static RoType CastToRuntimeTypeInfo(this Type t)
				{
					return (RoType)t;
				}

				internal static bool CanBrowseWithoutMissingMetadataExceptions(this Type t)
				{
					return true;
				}

				internal static Type[] GetGenericTypeParameters(this Type t)
				{
					return t.GetGenericArguments();
				}
			}

			internal readonly struct AssemblyFileInfo
			{
				public string Name { get; }

				public int RowIndex { get; }

				public bool ContainsMetadata { get; }

				public AssemblyFileInfo(string name, bool containsMetadata, int rowIndex)
				{
					Name = name;
					ContainsMetadata = containsMetadata;
					RowIndex = rowIndex;
				}
			}
		}
	}

	namespace TypeLoading
	{
		using System.Threading;
		using System.Diagnostics;
		using System.Reflection.Metadata;
		using System.Runtime.Serialization;
		using System.Diagnostics.CodeAnalysis;
		using System.Reflection.Runtime.General;
		using System.Reflection.Runtime.TypeInfos;
		using System.Reflection.PortableExecutable;
		using System.Reflection.Runtime.BindingFlagSupport;
		
		internal sealed class AssemblyNameData
		{
			public AssemblyNameFlags Flags;

			public string Name;

			public Version Version;

			public string CultureName;

			public byte[] PublicKey;

			public byte[] PublicKeyToken;

			public AssemblyContentType ContentType;

			public System.Configuration.Assemblies.AssemblyHashAlgorithm HashAlgorithm;

			public ProcessorArchitecture ProcessorArchitecture;

			public AssemblyName CreateAssemblyName()
			{
				AssemblyName assemblyName = new AssemblyName
				{
					Flags = Flags,
					Name = Name,
					Version = Version,
					CultureName = CultureName,
					ContentType = ContentType,
					HashAlgorithm = HashAlgorithm,
					ProcessorArchitecture = ProcessorArchitecture
				};
				assemblyName.SetPublicKey(PublicKey.CloneArray());
				assemblyName.SetPublicKeyToken(PublicKeyToken.CloneArray());
				return assemblyName;
			}
		}

		internal static class Assignability
		{
			public static bool IsAssignableFrom(Type toTypeInfo, Type fromTypeInfo, CoreTypes coreTypes)
			{
				if (toTypeInfo == null)
				{
					throw new NullReferenceException();
				}
				if (fromTypeInfo == null)
				{
					return false;
				}
				if (fromTypeInfo.Equals(toTypeInfo))
				{
					return true;
				}
				if (toTypeInfo.IsGenericTypeDefinition)
				{
					return false;
				}
				if (fromTypeInfo.IsGenericTypeDefinition)
				{
					fromTypeInfo = fromTypeInfo.GetGenericTypeDefinition().MakeGenericType(fromTypeInfo.GetGenericTypeParameters());
				}
				if (fromTypeInfo.CanCastTo(toTypeInfo, coreTypes))
				{
					return true;
				}
				if (!fromTypeInfo.IsGenericParameter && toTypeInfo.IsConstructedGenericType && toTypeInfo.GetGenericTypeDefinition() == coreTypes[CoreType.NullableT])
				{
					Type type = toTypeInfo.GenericTypeArguments[0];
					if (type.Equals(fromTypeInfo))
					{
						return true;
					}
				}
				return false;
			}

			private static bool CanCastTo(this Type fromTypeInfo, Type toTypeInfo, CoreTypes coreTypes)
			{
				if (fromTypeInfo.Equals(toTypeInfo))
				{
					return true;
				}
				if (fromTypeInfo.IsArray)
				{
					if (toTypeInfo.IsInterface)
					{
						return fromTypeInfo.CanCastArrayToInterface(toTypeInfo, coreTypes);
					}
					if (fromTypeInfo.IsSubclassOf(toTypeInfo))
					{
						return true;
					}
					if (!toTypeInfo.IsArray)
					{
						return false;
					}
					int arrayRank = fromTypeInfo.GetArrayRank();
					if (arrayRank != toTypeInfo.GetArrayRank())
					{
						return false;
					}
					bool flag = fromTypeInfo.IsSZArray();
					bool flag2 = toTypeInfo.IsSZArray();
					if (flag != flag2 && (arrayRank != 1 || flag2))
					{
						return false;
					}
					Type elementType = toTypeInfo.GetElementType();
					Type elementType2 = fromTypeInfo.GetElementType();
					return elementType2.IsElementTypeCompatibleWith(elementType, coreTypes);
				}
				if (fromTypeInfo.IsByRef)
				{
					if (!toTypeInfo.IsByRef)
					{
						return false;
					}
					Type elementType3 = toTypeInfo.GetElementType();
					Type elementType4 = fromTypeInfo.GetElementType();
					return elementType4.IsElementTypeCompatibleWith(elementType3, coreTypes);
				}
				if (fromTypeInfo.IsPointer)
				{
					if (toTypeInfo.Equals(coreTypes[CoreType.Object]))
					{
						return true;
					}
					if (toTypeInfo.Equals(coreTypes[CoreType.UIntPtr]))
					{
						return true;
					}
					if (!toTypeInfo.IsPointer)
					{
						return false;
					}
					Type elementType5 = toTypeInfo.GetElementType();
					Type elementType6 = fromTypeInfo.GetElementType();
					return elementType6.IsElementTypeCompatibleWith(elementType5, coreTypes);
				}
				if (fromTypeInfo.IsGenericParameter)
				{
					if (toTypeInfo.Equals(coreTypes[CoreType.Object]))
					{
						return true;
					}
					if (toTypeInfo.Equals(coreTypes[CoreType.ValueType]))
					{
						GenericParameterAttributes genericParameterAttributes = fromTypeInfo.GenericParameterAttributes;
						if ((genericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
						{
							return true;
						}
					}
					Type[] genericParameterConstraints = fromTypeInfo.GetGenericParameterConstraints();
					foreach (Type fromTypeInfo2 in genericParameterConstraints)
					{
						if (fromTypeInfo2.CanCastTo(toTypeInfo, coreTypes))
						{
							return true;
						}
					}
					return false;
				}
				if (toTypeInfo.IsArray || toTypeInfo.IsByRef || toTypeInfo.IsPointer || toTypeInfo.IsGenericParameter)
				{
					return false;
				}
				if (fromTypeInfo.MatchesWithVariance(toTypeInfo, coreTypes))
				{
					return true;
				}
				if (toTypeInfo.IsInterface)
				{
					Type[] interfaces = fromTypeInfo.GetInterfaces();
					foreach (Type fromTypeInfo3 in interfaces)
					{
						if (fromTypeInfo3.MatchesWithVariance(toTypeInfo, coreTypes))
						{
							return true;
						}
					}
					return false;
				}
				if (toTypeInfo.Equals(coreTypes[CoreType.Object]) && fromTypeInfo.IsInterface)
				{
					return true;
				}
				Type type = fromTypeInfo;
				do
				{
					Type baseType = type.BaseType;
					if (baseType == null)
					{
						return false;
					}
					type = baseType;
				}
				while (!type.MatchesWithVariance(toTypeInfo, coreTypes));
				return true;
			}

			private static bool MatchesWithVariance(this Type fromTypeInfo, Type toTypeInfo, CoreTypes coreTypes)
			{
				if (fromTypeInfo.Equals(toTypeInfo))
				{
					return true;
				}
				if (!fromTypeInfo.IsConstructedGenericType || !toTypeInfo.IsConstructedGenericType)
				{
					return false;
				}
				Type genericTypeDefinition = fromTypeInfo.GetGenericTypeDefinition();
				if (!genericTypeDefinition.Equals(toTypeInfo.GetGenericTypeDefinition()))
				{
					return false;
				}
				Type[] genericTypeArguments = fromTypeInfo.GenericTypeArguments;
				Type[] genericTypeArguments2 = toTypeInfo.GenericTypeArguments;
				Type[] genericTypeParameters = genericTypeDefinition.GetGenericTypeParameters();
				for (int i = 0; i < genericTypeParameters.Length; i++)
				{
					Type type = genericTypeArguments[i];
					Type type2 = genericTypeArguments2[i];
					GenericParameterAttributes genericParameterAttributes = genericTypeParameters[i].GenericParameterAttributes;
					switch (genericParameterAttributes & GenericParameterAttributes.VarianceMask)
					{
					case GenericParameterAttributes.Covariant:
						if (!type.IsGcReferenceTypeAndCastableTo(type2, coreTypes))
						{
							return false;
						}
						break;
					case GenericParameterAttributes.Contravariant:
						if (!type2.IsGcReferenceTypeAndCastableTo(type, coreTypes))
						{
							return false;
						}
						break;
					case GenericParameterAttributes.None:
						if (!type.Equals(type2))
						{
							return false;
						}
						break;
					default:
						throw new BadImageFormatException();
					}
				}
				return true;
			}

			private static bool IsElementTypeCompatibleWith(this Type fromTypeInfo, Type toTypeInfo, CoreTypes coreTypes)
			{
				if (fromTypeInfo.IsGcReferenceTypeAndCastableTo(toTypeInfo, coreTypes))
				{
					return true;
				}
				Type type = fromTypeInfo.ReducedType(coreTypes);
				Type o = toTypeInfo.ReducedType(coreTypes);
				if (type.Equals(o))
				{
					return true;
				}
				return false;
			}

			private static Type ReducedType(this Type t, CoreTypes coreTypes)
			{
				if (t.IsEnum)
				{
					t = t.GetEnumUnderlyingType();
				}
				if (t.Equals(coreTypes[CoreType.Byte]))
				{
					return coreTypes[CoreType.SByte] ?? throw new TypeLoadException(System.SR.Format(MDCFR.Properties.Resources.CoreTypeNotFound, "System.SByte"));
				}
				if (t.Equals(coreTypes[CoreType.UInt16]))
				{
					return coreTypes[CoreType.Int16] ?? throw new TypeLoadException(System.SR.Format(MDCFR.Properties.Resources.CoreTypeNotFound, "System.Int16"));
				}
				if (t.Equals(coreTypes[CoreType.UInt32]))
				{
					return coreTypes[CoreType.Int32] ?? throw new TypeLoadException(System.SR.Format(MDCFR.Properties.Resources.CoreTypeNotFound, "System.Int32"));
				}
				if (t.Equals(coreTypes[CoreType.UInt64]))
				{
					return coreTypes[CoreType.Int64] ?? throw new TypeLoadException(System.SR.Format(MDCFR.Properties.Resources.CoreTypeNotFound, "System.Int64"));
				}
				return t;
			}

			private static bool IsGcReferenceTypeAndCastableTo(this Type fromTypeInfo, Type toTypeInfo, CoreTypes coreTypes)
			{
				if (fromTypeInfo.Equals(toTypeInfo))
				{
					return true;
				}
				if (fromTypeInfo.ProvablyAGcReferenceType(coreTypes))
				{
					return fromTypeInfo.CanCastTo(toTypeInfo, coreTypes);
				}
				return false;
			}

			private static bool ProvablyAGcReferenceType(this Type t, CoreTypes coreTypes)
			{
				if (t.IsGenericParameter)
				{
					GenericParameterAttributes genericParameterAttributes = t.GenericParameterAttributes;
					if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
					{
						return true;
					}
				}
				return t.ProvablyAGcReferenceTypeHelper(coreTypes);
			}

			private static bool ProvablyAGcReferenceTypeHelper(this Type t, CoreTypes coreTypes)
			{
				if (t.IsArray)
				{
					return true;
				}
				if (t.IsByRef || t.IsPointer)
				{
					return false;
				}
				if (t.IsGenericParameter)
				{
					Type[] genericParameterConstraints = t.GetGenericParameterConstraints();
					foreach (Type t2 in genericParameterConstraints)
					{
						if (t2.ProvablyAGcReferenceTypeHelper(coreTypes))
						{
							return true;
						}
					}
					return false;
				}
				if (t.IsClass && !t.Equals(coreTypes[CoreType.Object]) && !t.Equals(coreTypes[CoreType.ValueType]))
				{
					return !t.Equals(coreTypes[CoreType.Enum]);
				}
				return false;
			}

			private static bool CanCastArrayToInterface(this Type fromTypeInfo, Type toTypeInfo, CoreTypes coreTypes)
			{
				if (toTypeInfo.IsConstructedGenericType)
				{
					Type[] genericTypeArguments = toTypeInfo.GenericTypeArguments;
					if (genericTypeArguments.Length != 1)
					{
						return false;
					}
					Type toTypeInfo2 = genericTypeArguments[0];
					Type genericTypeDefinition = toTypeInfo.GetGenericTypeDefinition();
					Type elementType = fromTypeInfo.GetElementType();
					Type[] interfaces = fromTypeInfo.GetInterfaces();
					foreach (Type type in interfaces)
					{
						if (type.IsConstructedGenericType)
						{
							Type genericTypeDefinition2 = type.GetGenericTypeDefinition();
							if (genericTypeDefinition2.Equals(genericTypeDefinition) && elementType.IsElementTypeCompatibleWith(toTypeInfo2, coreTypes))
							{
								return true;
							}
						}
					}
					return false;
				}
				Type[] interfaces2 = fromTypeInfo.GetInterfaces();
				foreach (Type type2 in interfaces2)
				{
					if (type2.Equals(toTypeInfo))
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Enumerates all the system types that MetadataLoadContexts may need to fish out of the core assembly.
		/// Note that the enum values are often cast to "int" and used as indices into a table so the
		/// enum values should be left contiguous.
		///
		/// If you add a member to this enum, you must also add a switch case for it in CoreTypeHelpers.GetFullName();
		/// </summary>
		internal enum CoreType
		{
			Array,
			Boolean,
			Byte,
			Char,
			Double,
			Enum,
			Int16,
			Int32,
			Int64,
			IntPtr,
			Object,
			NullableT,
			SByte,
			Single,
			String,
			TypedReference,
			UInt16,
			UInt32,
			UInt64,
			UIntPtr,
			ValueType,
			Void,
			MulticastDelegate,
			IEnumerableT,
			ICollectionT,
			IListT,
			IReadOnlyListT,
			DBNull,
			Decimal,
			DateTime,
			Type,
			ComImportAttribute,
			DllImportAttribute,
			CallingConvention,
			CharSet,
			MarshalAsAttribute,
			UnmanagedType,
			VarEnum,
			InAttribute,
			OutAttribute,
			OptionalAttribute,
			PreserveSigAttribute,
			FieldOffsetAttribute,
			NumCoreTypes
		}

		internal static class CoreTypeHelpers
		{
			public static void GetFullName(this CoreType coreType, out ReadOnlySpan<byte> ns, out ReadOnlySpan<byte> name)
			{
				switch (coreType)
				{
				case CoreType.Array:
					ns = Utf8Constants.System;
					name = Utf8Constants.Array;
					break;
				case CoreType.Boolean:
					ns = Utf8Constants.System;
					name = Utf8Constants.Boolean;
					break;
				case CoreType.Byte:
					ns = Utf8Constants.System;
					name = Utf8Constants.Byte;
					break;
				case CoreType.Char:
					ns = Utf8Constants.System;
					name = Utf8Constants.Char;
					break;
				case CoreType.Double:
					ns = Utf8Constants.System;
					name = Utf8Constants.Double;
					break;
				case CoreType.Enum:
					ns = Utf8Constants.System;
					name = Utf8Constants.Enum;
					break;
				case CoreType.Int16:
					ns = Utf8Constants.System;
					name = Utf8Constants.Int16;
					break;
				case CoreType.Int32:
					ns = Utf8Constants.System;
					name = Utf8Constants.Int32;
					break;
				case CoreType.Int64:
					ns = Utf8Constants.System;
					name = Utf8Constants.Int64;
					break;
				case CoreType.IntPtr:
					ns = Utf8Constants.System;
					name = Utf8Constants.IntPtr;
					break;
				case CoreType.NullableT:
					ns = Utf8Constants.System;
					name = Utf8Constants.NullableT;
					break;
				case CoreType.Object:
					ns = Utf8Constants.System;
					name = Utf8Constants.Object;
					break;
				case CoreType.SByte:
					ns = Utf8Constants.System;
					name = Utf8Constants.SByte;
					break;
				case CoreType.Single:
					ns = Utf8Constants.System;
					name = Utf8Constants.Single;
					break;
				case CoreType.String:
					ns = Utf8Constants.System;
					name = Utf8Constants.String;
					break;
				case CoreType.TypedReference:
					ns = Utf8Constants.System;
					name = Utf8Constants.TypedReference;
					break;
				case CoreType.UInt16:
					ns = Utf8Constants.System;
					name = Utf8Constants.UInt16;
					break;
				case CoreType.UInt32:
					ns = Utf8Constants.System;
					name = Utf8Constants.UInt32;
					break;
				case CoreType.UInt64:
					ns = Utf8Constants.System;
					name = Utf8Constants.UInt64;
					break;
				case CoreType.UIntPtr:
					ns = Utf8Constants.System;
					name = Utf8Constants.UIntPtr;
					break;
				case CoreType.ValueType:
					ns = Utf8Constants.System;
					name = Utf8Constants.ValueType;
					break;
				case CoreType.Void:
					ns = Utf8Constants.System;
					name = Utf8Constants.Void;
					break;
				case CoreType.MulticastDelegate:
					ns = Utf8Constants.System;
					name = Utf8Constants.MulticastDelegate;
					break;
				case CoreType.IEnumerableT:
					ns = Utf8Constants.SystemCollectionsGeneric;
					name = Utf8Constants.IEnumerableT;
					break;
				case CoreType.ICollectionT:
					ns = Utf8Constants.SystemCollectionsGeneric;
					name = Utf8Constants.ICollectionT;
					break;
				case CoreType.IListT:
					ns = Utf8Constants.SystemCollectionsGeneric;
					name = Utf8Constants.IListT;
					break;
				case CoreType.IReadOnlyListT:
					ns = Utf8Constants.SystemCollectionsGeneric;
					name = Utf8Constants.IReadOnlyListT;
					break;
				case CoreType.Type:
					ns = Utf8Constants.System;
					name = Utf8Constants.Type;
					break;
				case CoreType.DBNull:
					ns = Utf8Constants.System;
					name = Utf8Constants.DBNull;
					break;
				case CoreType.Decimal:
					ns = Utf8Constants.System;
					name = Utf8Constants.Decimal;
					break;
				case CoreType.DateTime:
					ns = Utf8Constants.System;
					name = Utf8Constants.DateTime;
					break;
				case CoreType.ComImportAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.ComImportAttribute;
					break;
				case CoreType.DllImportAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.DllImportAttribute;
					break;
				case CoreType.CallingConvention:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.CallingConvention;
					break;
				case CoreType.CharSet:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.CharSet;
					break;
				case CoreType.MarshalAsAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.MarshalAsAttribute;
					break;
				case CoreType.UnmanagedType:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.UnmanagedType;
					break;
				case CoreType.VarEnum:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.VarEnum;
					break;
				case CoreType.InAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.InAttribute;
					break;
				case CoreType.OutAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.OutAttriubute;
					break;
				case CoreType.OptionalAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.OptionalAttribute;
					break;
				case CoreType.PreserveSigAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.PreserveSigAttribute;
					break;
				case CoreType.FieldOffsetAttribute:
					ns = Utf8Constants.SystemRuntimeInteropServices;
					name = Utf8Constants.FieldOffsetAttribute;
					break;
				default:
					ns = (name = default(ReadOnlySpan<byte>));
					break;
				}
			}
		}

		/// <summary>
		/// A convenience class that holds the palette of core types that were successfully loaded (or the reason they were not.)
		/// </summary>
		internal sealed class CoreTypes
		{
			private readonly RoType[] _coreTypes;

			private readonly Exception[] _exceptions;

			/// <summary>
			/// Returns null if the specific core type did not exist or could not be loaded. Call GetException(coreType) to get detailed info.
			/// </summary>
			public RoType this[CoreType coreType] => _coreTypes[(int)coreType];

			internal CoreTypes(MetadataLoadContext loader, string coreAssemblyName)
			{
				int num = 43;
				RoType[] array = new RoType[num];
				Exception[] array2 = new Exception[num];
				Exception e;
				RoAssembly roAssembly = loader.TryGetCoreAssembly(coreAssemblyName, out e);
				if (roAssembly == null)
				{
					throw e;
				}
				for (int i = 0; i < num; i++)
				{
					((CoreType)i).GetFullName(out var ns, out var name);
					if ((array[i] = roAssembly.GetTypeCore(ns, name, ignoreCase: false, out e)) == null)
					{
						array2[i] = e;
					}
				}
				_coreTypes = array;
				_exceptions = array2;
			}

			public Exception GetException(CoreType coreType)
			{
				return _exceptions[(int)coreType];
			}
		}

		internal readonly struct CustomAttributeArguments
		{
			public IList<CustomAttributeTypedArgument> FixedArguments { get; }

			public IList<CustomAttributeNamedArgument> NamedArguments { get; }

			public CustomAttributeArguments(IList<CustomAttributeTypedArgument> fixedArguments, IList<CustomAttributeNamedArgument> namedArguments)
			{
				FixedArguments = fixedArguments;
				NamedArguments = namedArguments;
			}
		}

		internal static class CustomAttributeHelpers
		{
			/// <summary>
			/// Helper for creating a CustomAttributeNamedArgument.
			/// </summary>
			public static CustomAttributeNamedArgument ToCustomAttributeNamedArgument(this Type attributeType, string name, Type argumentType, object value)
			{
				MemberInfo[] member = attributeType.GetMember(name, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public);
				if (member.Length == 0)
				{
					throw new MissingMemberException(attributeType.FullName, name);
				}
				if (member.Length > 1)
				{
					throw new AmbiguousMatchException();
				}
				return new CustomAttributeNamedArgument(member[0], new CustomAttributeTypedArgument(argumentType, value));
			}

			/// <summary>
			/// Clones a cached CustomAttributeTypedArgument list into a freshly allocated one suitable for direct return through an api.
			/// </summary>
			public static ReadOnlyCollection<CustomAttributeTypedArgument> CloneForApiReturn(this IList<CustomAttributeTypedArgument> cats)
			{
				int count = cats.Count;
				CustomAttributeTypedArgument[] array = new CustomAttributeTypedArgument[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = cats[i].CloneForApiReturn();
				}
				return array.ToReadOnlyCollection();
			}

			/// <summary>
			/// Clones a cached CustomAttributeNamedArgument list into a freshly allocated one suitable for direct return through an api.
			/// </summary>
			public static ReadOnlyCollection<CustomAttributeNamedArgument> CloneForApiReturn(this IList<CustomAttributeNamedArgument> cans)
			{
				int count = cans.Count;
				CustomAttributeNamedArgument[] array = new CustomAttributeNamedArgument[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = cans[i].CloneForApiReturn();
				}
				return array.ToReadOnlyCollection();
			}

			/// <summary>
			/// Clones a cached CustomAttributeTypedArgument into a freshly allocated one suitable for direct return through an api.
			/// </summary>
			private static CustomAttributeTypedArgument CloneForApiReturn(this CustomAttributeTypedArgument cat)
			{
				Type argumentType = cat.ArgumentType;
				object value = cat.Value;
				if (!(value is IList<CustomAttributeTypedArgument> list))
				{
					return cat;
				}
				int count = list.Count;
				CustomAttributeTypedArgument[] array = new CustomAttributeTypedArgument[count];
				for (int i = 0; i < count; i++)
				{
					array[i] = list[i].CloneForApiReturn();
				}
				return new CustomAttributeTypedArgument(argumentType, array.ToReadOnlyCollection());
			}

			/// <summary>
			/// Clones a cached CustomAttributeNamedArgument into a freshly allocated one suitable for direct return through an api.
			/// </summary>
			private static CustomAttributeNamedArgument CloneForApiReturn(this CustomAttributeNamedArgument can)
			{
				return new CustomAttributeNamedArgument(can.MemberInfo, can.TypedValue.CloneForApiReturn());
			}

			/// <summary>
			/// Convert MarshalAsAttribute data into CustomAttributeData form. Returns null if the core assembly cannot be loaded or if the necessary
			/// types aren't in the core assembly.
			/// </summary>
			public static CustomAttributeData TryComputeMarshalAsCustomAttributeData(Func<MarshalAsAttribute> marshalAsAttributeComputer, MetadataLoadContext loader)
			{
				CoreTypes ct = loader.GetAllFoundCoreTypes();
				if (ct[CoreType.String] == null || ct[CoreType.Boolean] == null || ct[CoreType.UnmanagedType] == null || ct[CoreType.VarEnum] == null || ct[CoreType.Type] == null || ct[CoreType.Int16] == null || ct[CoreType.Int32] == null)
				{
					return null;
				}
				ConstructorInfo ci = loader.TryGetMarshalAsCtor();
				if (ci == null)
				{
					return null;
				}
				Func<CustomAttributeArguments> argumentsPromise = delegate
				{
					MarshalAsAttribute marshalAsAttribute = marshalAsAttributeComputer();
					Type declaringType = ci.DeclaringType;
					CustomAttributeTypedArgument[] fixedArguments = new CustomAttributeTypedArgument[1]
					{
						new CustomAttributeTypedArgument(ct[CoreType.UnmanagedType], (int)marshalAsAttribute.Value)
					};
					List<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>();
					list.AddRange(new CustomAttributeNamedArgument[5]
					{
						declaringType.ToCustomAttributeNamedArgument("ArraySubType", ct[CoreType.UnmanagedType], (int)marshalAsAttribute.ArraySubType),
						declaringType.ToCustomAttributeNamedArgument("IidParameterIndex", ct[CoreType.Int32], marshalAsAttribute.IidParameterIndex),
						declaringType.ToCustomAttributeNamedArgument("SafeArraySubType", ct[CoreType.VarEnum], (int)marshalAsAttribute.SafeArraySubType),
						declaringType.ToCustomAttributeNamedArgument("SizeConst", ct[CoreType.Int32], marshalAsAttribute.SizeConst),
						declaringType.ToCustomAttributeNamedArgument("SizeParamIndex", ct[CoreType.Int16], marshalAsAttribute.SizeParamIndex)
					});
					if (marshalAsAttribute.SafeArrayUserDefinedSubType != null)
					{
						list.Add(declaringType.ToCustomAttributeNamedArgument("SafeArrayUserDefinedSubType", ct[CoreType.Type], marshalAsAttribute.SafeArrayUserDefinedSubType));
					}
					if (marshalAsAttribute.MarshalType != null)
					{
						list.Add(declaringType.ToCustomAttributeNamedArgument("MarshalType", ct[CoreType.String], marshalAsAttribute.MarshalType));
					}
					if (marshalAsAttribute.MarshalTypeRef != null)
					{
						list.Add(declaringType.ToCustomAttributeNamedArgument("MarshalTypeRef", ct[CoreType.Type], marshalAsAttribute.MarshalTypeRef));
					}
					if (marshalAsAttribute.MarshalCookie != null)
					{
						list.Add(declaringType.ToCustomAttributeNamedArgument("MarshalCookie", ct[CoreType.String], marshalAsAttribute.MarshalCookie));
					}
					return new CustomAttributeArguments(fixedArguments, list);
				};
				return new RoPseudoCustomAttributeData(ci, argumentsPromise);
			}
		}

		internal static class DefaultBinderThunks
		{
			internal static ParameterInfo[] GetParametersNoCopy(this MethodBase m)
			{
				if (m is RoMethod roMethod)
				{
					return roMethod.GetParametersNoCopy();
				}
				if (m is RoConstructor roConstructor)
				{
					return roConstructor.GetParametersNoCopy();
				}
				return m.GetParameters();
			}

			internal static int GetGenericParameterCount(this MethodInfo m)
			{
				if (m is RoMethod roMethod)
				{
					return roMethod.GetGenericArgumentsOrParametersNoCopy().Length;
				}
				return m.GetGenericArguments().Length;
			}
		}

		internal sealed class GetTypeCoreCache
		{
			private sealed class Container
			{
				private readonly int[] _buckets;

				private readonly Entry[] _entries;

				private int _nextFreeEntry;

				private readonly GetTypeCoreCache _owner;

				private const int _initialCapacity = 5;

				public bool HasCapacity => _nextFreeEntry != _entries.Length;

				public Container(GetTypeCoreCache owner)
				{
					_buckets = new int[5];
					for (int i = 0; i < 5; i++)
					{
						_buckets[i] = -1;
					}
					_entries = new Entry[5];
					_nextFreeEntry = 0;
					_owner = owner;
				}

				private Container(GetTypeCoreCache owner, int[] buckets, Entry[] entries, int nextFreeEntry)
				{
					_buckets = buckets;
					_entries = entries;
					_nextFreeEntry = nextFreeEntry;
					_owner = owner;
				}

				public bool TryGetValue(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, int hashCode, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out RoDefinitionType value)
				{
					int num = ComputeBucket(hashCode, _buckets.Length);
					for (int num2 = Volatile.Read(ref _buckets[num]); num2 != -1; num2 = _entries[num2]._next)
					{
						if (hashCode == _entries[num2]._hashCode)
						{
							RoDefinitionType value2 = _entries[num2]._value;
							if (value2.IsTypeNameEqual(ns, name))
							{
								value = value2;
								return true;
							}
						}
					}
					value = null;
					return false;
				}

				public void Add(int hashCode, RoDefinitionType value)
				{
					int num = ComputeBucket(hashCode, _buckets.Length);
					int nextFreeEntry = _nextFreeEntry;
					_entries[nextFreeEntry]._value = value;
					_entries[nextFreeEntry]._hashCode = hashCode;
					_entries[nextFreeEntry]._next = _buckets[num];
					_nextFreeEntry++;
					Volatile.Write(ref _buckets[num], nextFreeEntry);
				}

				public void Resize()
				{
					int prime = HashHelpers.GetPrime(_buckets.Length * 2);
					if (prime <= _nextFreeEntry)
					{
						throw new OutOfMemoryException();
					}
					Entry[] array = new Entry[prime];
					int[] array2 = new int[prime];
					for (int i = 0; i < prime; i++)
					{
						array2[i] = -1;
					}
					int num = 0;
					for (int j = 0; j < _buckets.Length; j++)
					{
						for (int num2 = _buckets[j]; num2 != -1; num2 = _entries[num2]._next)
						{
							array[num]._value = _entries[num2]._value;
							array[num]._hashCode = _entries[num2]._hashCode;
							int num3 = ComputeBucket(array[num]._hashCode, prime);
							array[num]._next = array2[num3];
							array2[num3] = num;
							num++;
						}
					}
					_owner._container = new Container(_owner, array2, array, num);
				}

				private static int ComputeBucket(int hashCode, int numBuckets)
				{
					return (hashCode & 0x7FFFFFFF) % numBuckets;
				}

				[Conditional("DEBUG")]
				public void VerifyUnifierConsistency()
				{
					if (_nextFreeEntry >= 5000 && _nextFreeEntry % 100 != 0)
					{
						return;
					}
					int num = 0;
					for (int i = 0; i < _buckets.Length; i++)
					{
						int num2 = _buckets[i];
						int num3 = _buckets[i];
						while (num2 != -1)
						{
							num++;
							int num4 = ComputeBucket(_entries[num2]._hashCode, _buckets.Length);
							num2 = _entries[num2]._next;
							if (num3 != -1)
							{
								num3 = _entries[num3]._next;
							}
							if (num3 != -1)
							{
								num3 = _entries[num3]._next;
							}
							if (num2 == num3)
							{
								_ = -1;
							}
						}
					}
				}
			}

			private struct Entry
			{
				public RoDefinitionType _value;

				public int _hashCode;

				public int _next;
			}

			private volatile Container _container;

			private readonly object _lock;

			public GetTypeCoreCache()
			{
				_lock = new object();
				_container = new Container(this);
			}

			public bool TryGet(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, int hashCode, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out RoDefinitionType type)
			{
				return _container.TryGetValue(ns, name, hashCode, out type);
			}

			public RoDefinitionType GetOrAdd(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, int hashCode, RoDefinitionType type)
			{
				if (_container.TryGetValue(ns, name, hashCode, out var value))
				{
					return value;
				}
				Monitor.Enter(_lock);
				try
				{
					if (_container.TryGetValue(ns, name, hashCode, out var value2))
					{
						return value2;
					}
					if (!_container.HasCapacity)
					{
						_container.Resize();
					}
					_container.Add(hashCode, type);
					return type;
				}
				finally
				{
					Monitor.Exit(_lock);
				}
			}

			public static int ComputeHashCode(ReadOnlySpan<byte> name)
			{
				int num = 947009409;
				for (int i = 0; i < name.Length; i++)
				{
					num = (num << 8) ^ name[i];
				}
				return num;
			}
		}

		internal static class HashHelpers
		{
			public const int HashPrime = 101;

			public static readonly int[] primes = new int[72]
			{
				3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 
				163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 
				5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 
				90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
				968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 
			};

			public static bool IsPrime(int candidate)
			{
				if (((uint)candidate & (true ? 1u : 0u)) != 0)
				{
					int num = (int)Math.Sqrt(candidate);
					for (int i = 3; i <= num; i += 2)
					{
						if (candidate % i == 0)
						{
							return false;
						}
					}
					return true;
				}
				return candidate == 2;
			}

			public static int GetPrime(int min)
			{
				if (min < 0)
				{
					throw new ArgumentException(MDCFR.Properties.Resources.Arg_HTCapacityOverflow);
				}
				for (int i = 0; i < primes.Length; i++)
				{
					int num = primes[i];
					if (num >= min)
					{
						return num;
					}
				}
				for (int j = min | 1; j < int.MaxValue; j += 2)
				{
					if (IsPrime(j) && (j - 1) % 101 != 0)
					{
						return j;
					}
				}
				return min;
			}
		}

		internal static class Helpers
		{
			private static readonly char[] s_charsToEscape = new char[7] { '\\', '[', ']', '+', '*', '&', ',' };

			[return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull("original")]
			public static T[] CloneArray<T>(this T[] original)
			{
				if (original == null)
				{
					return null;
				}
				if (original.Length == 0)
				{
					return Array.Empty<T>();
				}
				T[] array = new T[original.Length];
				Array.Copy(original, 0, array, 0, original.Length);
				return array;
			}

			public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumeration)
			{
				List<T> list = new List<T>(enumeration);
				return new ReadOnlyCollection<T>(list.ToArray());
			}

			public static int GetTokenRowNumber(this int token)
			{
				return token & 0xFFFFFF;
			}

			public static RoMethod FilterInheritedAccessor(this RoMethod accessor)
			{
				if (accessor.ReflectedType == accessor.DeclaringType)
				{
					return accessor;
				}
				if (accessor.IsPrivate)
				{
					return null;
				}
				return accessor;
			}

			public static MethodInfo FilterAccessor(this MethodInfo accessor, bool nonPublic)
			{
				if (nonPublic)
				{
					return accessor;
				}
				if (accessor.IsPublic)
				{
					return accessor;
				}
				return null;
			}

			public static string ComputeArraySuffix(int rank, bool multiDim)
			{
				if (!multiDim)
				{
					return "[]";
				}
				if (rank == 1)
				{
					return "[*]";
				}
				return "[" + new string(',', rank - 1) + "]";
			}

			public static string EscapeTypeNameIdentifier(this string identifier)
			{
				if (identifier.IndexOfAny(s_charsToEscape) != -1)
				{
					StringBuilder stringBuilder = new StringBuilder(identifier.Length);
					string text = identifier;
					foreach (char c in text)
					{
						if (c.NeedsEscapingInTypeName())
						{
							stringBuilder.Append('\\');
						}
						stringBuilder.Append(c);
					}
					identifier = stringBuilder.ToString();
				}
				return identifier;
			}

			public static bool TypeNameContainsTypeParserMetacharacters(this string identifier)
			{
				return identifier.IndexOfAny(s_charsToEscape) != -1;
			}

			public static bool NeedsEscapingInTypeName(this char c)
			{
				return Array.IndexOf(s_charsToEscape, c) >= 0;
			}

			public static string UnescapeTypeNameIdentifier(this string identifier)
			{
				if (identifier.IndexOf('\\') != -1)
				{
					StringBuilder stringBuilder = new StringBuilder(identifier.Length);
					for (int i = 0; i < identifier.Length; i++)
					{
						if (identifier[i] == '\\')
						{
							i++;
						}
						stringBuilder.Append(identifier[i]);
					}
					identifier = stringBuilder.ToString();
				}
				return identifier;
			}

			/// <summary>
			/// For AssemblyReferences, convert "unspecified" components from the ECMA format (0xffff) to the in-memory System.Version format (0xffffffff).
			/// </summary>
			public static Version AdjustForUnspecifiedVersionComponents(this Version v)
			{
				return (int)(((v.Revision == 65535) ? 1u : 0u) | (uint)((v.Build == 65535) ? 2 : 0) | (uint)((v.Minor == 65535) ? 4 : 0) | (uint)((v.Major == 65535) ? 8 : 0)) switch
				{
					0 => v, 
					1 => new Version(v.Major, v.Minor, v.Build), 
					2 => new Version(v.Major, v.Minor), 
					3 => new Version(v.Major, v.Minor), 
					_ => null, 
				};
			}

			public static byte[] ComputePublicKeyToken(this byte[] pkt)
			{
				AssemblyName assemblyName = new AssemblyName();
				assemblyName.SetPublicKey(pkt);
				return assemblyName.GetPublicKeyToken();
			}

			public static AssemblyNameFlags ConvertAssemblyFlagsToAssemblyNameFlags(AssemblyFlags assemblyFlags)
			{
				AssemblyNameFlags assemblyNameFlags = AssemblyNameFlags.None;
				if ((assemblyFlags & AssemblyFlags.Retargetable) != 0)
				{
					assemblyNameFlags |= AssemblyNameFlags.Retargetable;
				}
				return assemblyNameFlags;
			}

			public static void SplitTypeName(this string fullName, out string ns, out string name)
			{
				int num = fullName.LastIndexOf('.');
				if (num == -1)
				{
					ns = string.Empty;
					name = fullName;
				}
				else
				{
					ns = fullName.Substring(0, num);
					name = fullName.Substring(num + 1);
				}
			}

			public static string AppendTypeName(this string ns, string name)
			{
				if (ns.Length != 0)
				{
					return ns + "." + name;
				}
				return name;
			}

			public static string ToString(this IRoMethodBase roMethodBase, MethodSig<string> methodSigStrings)
			{
				TypeContext typeContext = roMethodBase.TypeContext;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(methodSigStrings[-1]);
				stringBuilder.Append(' ');
				stringBuilder.Append(roMethodBase.MethodBase.Name);
				Type[] genericMethodArguments = typeContext.GenericMethodArguments;
				Type[] array = genericMethodArguments;
				int num = ((array != null) ? array.Length : 0);
				if (num != 0)
				{
					stringBuilder.Append('[');
					for (int i = 0; i < num; i++)
					{
						if (i != 0)
						{
							stringBuilder.Append(',');
						}
						stringBuilder.Append(array[i].ToString());
					}
					stringBuilder.Append(']');
				}
				stringBuilder.Append('(');
				for (int j = 0; j < methodSigStrings.Parameters.Length; j++)
				{
					if (j != 0)
					{
						stringBuilder.Append(", ");
					}
					stringBuilder.Append(methodSigStrings[j]);
				}
				stringBuilder.Append(')');
				return stringBuilder.ToString();
			}

			public static bool HasSameMetadataDefinitionAsCore<M>(this M thisMember, MemberInfo other) where M : MemberInfo
			{
				if ((object)other == null)
				{
					throw new ArgumentNullException("other");
				}
				if (!(other is M))
				{
					return false;
				}
				if (thisMember.MetadataToken != other.MetadataToken)
				{
					return false;
				}
				if (!thisMember.Module.Equals(other.Module))
				{
					return false;
				}
				return true;
			}

			public static RoType LoadTypeFromAssemblyQualifiedName(string name, RoAssembly defaultAssembly, bool ignoreCase, bool throwOnError)
			{
				if (!name.TypeNameContainsTypeParserMetacharacters())
				{
					name.SplitTypeName(out var ns, out var name2);
					Exception e;
					RoType typeCore = defaultAssembly.GetTypeCore(ns, name2, ignoreCase, out e);
					if (typeCore != null)
					{
						return typeCore;
					}
					if (throwOnError)
					{
						throw e;
					}
				}
				MetadataLoadContext loader = defaultAssembly.Loader;
				Func<AssemblyName, Assembly> assemblyResolver = loader.LoadFromAssemblyName;
				Func<Assembly, string, bool, Type> typeResolver = delegate(Assembly assembly, string fullName, bool ignoreCase2)
				{
					if ((object)assembly == null)
					{
						assembly = defaultAssembly;
					}
					RoAssembly roAssembly = (RoAssembly)assembly;
					fullName = fullName.UnescapeTypeNameIdentifier();
					fullName.SplitTypeName(out var ns2, out var name3);
					Exception e2;
					Type typeCore2 = roAssembly.GetTypeCore(ns2, name3, ignoreCase2, out e2);
					if (typeCore2 != null)
					{
						return typeCore2;
					}
					if (throwOnError)
					{
						throw e2;
					}
					return null;
				};
				return (RoType)Type.GetType(name, assemblyResolver, typeResolver, throwOnError, ignoreCase);
			}

			public static Type[] ExtractCustomModifiers(this RoType type, bool isRequired)
			{
				int num = 0;
				RoType roType;
				for (roType = type; roType is RoModifiedType roModifiedType; roType = roModifiedType.UnmodifiedType)
				{
					if (roModifiedType.IsRequired == isRequired)
					{
						num++;
					}
				}
				Type[] array = new Type[num];
				roType = type;
				int num2 = num;
				while (roType is RoModifiedType roModifiedType2)
				{
					if (roModifiedType2.IsRequired == isRequired)
					{
						array[--num2] = roModifiedType2.Modifier;
					}
					roType = roModifiedType2.UnmodifiedType;
				}
				return array;
			}

			public static RoType SkipTypeWrappers(this RoType type)
			{
				while (type is RoWrappedType roWrappedType)
				{
					type = roWrappedType.UnmodifiedType;
				}
				return type;
			}

			public static bool IsVisibleOutsideAssembly(this Type type)
			{
				return (type.Attributes & TypeAttributes.VisibilityMask) switch
				{
					TypeAttributes.Public => true, 
					TypeAttributes.NestedPublic => type.DeclaringType.IsVisibleOutsideAssembly(), 
					_ => false, 
				};
			}

			public static RoAssemblyName ToRoAssemblyName(this AssemblyName assemblyName)
			{
				if (assemblyName.Name == null)
				{
					throw new ArgumentException();
				}
				byte[] publicKeyToken = assemblyName.GetPublicKeyToken().CloneArray();
				return new RoAssemblyName(assemblyName.Name, assemblyName.Version, assemblyName.CultureName, publicKeyToken, assemblyName.Flags);
			}

			public static byte[] ToUtf8(this string s)
			{
				return Encoding.UTF8.GetBytes(s);
			}

			public unsafe static string ToUtf16(this ReadOnlySpan<byte> utf8)
			{
				if (utf8.IsEmpty)
				{
					return string.Empty;
				}
				fixed (byte* bytes = utf8)
				{
					return Encoding.UTF8.GetString(bytes, utf8.Length);
				}
			}

			public static string GetDisposedString(this MetadataLoadContext loader)
			{
				if (!loader.IsDisposed)
				{
					return null;
				}
				return MDCFR.Properties.Resources.MetadataLoadContextDisposed;
			}

			public static TypeContext ToTypeContext(this RoType[] instantiation)
			{
				return new TypeContext(instantiation, null);
			}
		}

		internal interface IMethodDecoder
		{
			int MetadataToken { get; }

			RoModule GetRoModule();

			string ComputeName();

			MethodAttributes ComputeAttributes();

			CallingConventions ComputeCallingConvention();

			MethodImplAttributes ComputeMethodImplementationFlags();

			int ComputeGenericParameterCount();

			RoType[] ComputeGenericArgumentsOrParameters();

			IEnumerable<CustomAttributeData> ComputeTrueCustomAttributes();

			DllImportAttribute ComputeDllImportAttribute();

			MethodSig<RoParameter> SpecializeMethodSig(IRoMethodBase member);

			MethodSig<RoType> SpecializeCustomModifiers(in TypeContext typeContext);

			MethodBody SpecializeMethodBody(IRoMethodBase owner);

			MethodSig<string> SpecializeMethodSigStrings(in TypeContext typeContext);
		}

		internal interface IRoMethodBase
		{
			MethodBase MethodBase { get; }

			MetadataLoadContext Loader { get; }

			TypeContext TypeContext { get; }

			Type[] GetCustomModifiers(int position, bool isRequired);

			string GetMethodSigString(int position);
		}

		internal abstract class LeveledAssembly : Assembly
		{
			public abstract Type[] GetForwardedTypes();
		}

		internal abstract class LeveledConstructorInfo : ConstructorInfo
		{
			public abstract bool IsConstructedGenericMethod { get; }

			public abstract bool HasSameMetadataDefinitionAs(MemberInfo other);
		}

		internal abstract class LeveledCustomAttributeData : CustomAttributeData
		{
			public new abstract Type AttributeType { get; }
		}

		internal abstract class LeveledEventInfo : EventInfo
		{
			public abstract bool HasSameMetadataDefinitionAs(MemberInfo other);
		}

		internal abstract class LeveledFieldInfo : FieldInfo
		{
			public abstract bool HasSameMetadataDefinitionAs(MemberInfo other);
		}

		internal abstract class LeveledMethodInfo : MethodInfo
		{
			public abstract bool IsConstructedGenericMethod { get; }

			public abstract bool HasSameMetadataDefinitionAs(MemberInfo other);
		}

		internal abstract class LeveledPropertyInfo : PropertyInfo
		{
			public abstract bool HasSameMetadataDefinitionAs(MemberInfo other);
		}

		/// <summary>
		/// Another layer of base types. For NetCore, these base types are all but empty. For NetStandard, these base types add the NetCore apis to NetStandard
		/// so code interacting with "RoTypes" and friends can happily code to the full NetCore surface area.
		///
		/// On NetStandard (and pre-2.2 NetCore), the TypeInfo constructor is not exposed so we cannot derive directly from TypeInfo.
		/// But we *can* derive from TypeDelegator which derives from TypeInfo. Since we're overriding (almost) every method,
		/// none of TypeDelegator's own methods get called (and the instance field it has for holding the "underlying Type" goes
		/// to waste.)
		///
		/// For future platforms, RoTypeBase's base type should be changed back to TypeInfo. Deriving from TypeDelegator is a hack and
		/// causes us to waste an extra pointer-sized field per Type instance. It is also fragile as TypeDelegator could break us in the future
		/// by overriding more methods.
		/// </summary>
		internal abstract class LeveledTypeInfo : TypeDelegator
		{
			public abstract bool IsGenericTypeParameter { get; }

			public abstract bool IsGenericMethodParameter { get; }

			public abstract bool IsSZArray { get; }

			public abstract bool IsVariableBoundArray { get; }

			public abstract bool IsTypeDefinition { get; }

			public abstract bool IsByRefLike { get; }

			public virtual bool IsSignatureType => false;

			public override EventInfo[] GetEvents()
			{
				return GetEvents(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
			}

			protected abstract MethodInfo GetMethodImpl(string name, int genericParameterCount, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

			public abstract bool HasSameMetadataDefinitionAs(MemberInfo other);
		}

		internal sealed class MethodSig<T>
		{
			public T Return { get; private set; }

			public T[] Parameters { get; }

			public T this[int position]
			{
				get
				{
					if (position != -1)
					{
						return Parameters[position];
					}
					return Return;
				}
				set
				{
					if (position == -1)
					{
						Return = value;
					}
					else
					{
						Parameters[position] = value;
					}
				}
			}

			public MethodSig(int parameterCount)
			{
				Parameters = new T[parameterCount];
			}
		}

		internal static class NetCoreApiEmulators
		{
			public static bool IsSignatureType(this Type type)
			{
				return false;
			}

			public static bool IsSZArray(this Type type)
			{
				if (type.IsArray && type.GetArrayRank() == 1)
				{
					return type.Name.EndsWith("[]", StringComparison.Ordinal);
				}
				return false;
			}

			public static bool IsVariableBoundArray(this Type type)
			{
				if (type.IsArray)
				{
					return !type.IsSZArray();
				}
				return false;
			}

			public static bool IsGenericMethodParameter(this Type type)
			{
				if (type.IsGenericParameter)
				{
					return type.DeclaringMethod != null;
				}
				return false;
			}

			public static Type MakeSignatureGenericType(this Type genericTypeDefinition, Type[] typeArguments)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_MakeGenericType_SignatureTypes);
			}
		}

		/// <summary>
		/// All RoTypes that return true for IsArray. This includes both SZArrays and multi-dim arrays.
		/// </summary>
		internal sealed class RoArrayType : RoHasElementType
		{
			public readonly struct Key : IEquatable<Key>
			{
				public RoType ElementType { get; }

				public int Rank { get; }

				public Key(RoType elementType, int rank)
				{
					ElementType = elementType;
					Rank = rank;
				}

				public bool Equals(Key other)
				{
					if (ElementType != other.ElementType)
					{
						return false;
					}
					if (Rank != other.Rank)
					{
						return false;
					}
					return true;
				}

				public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
				{
					if (obj is Key other)
					{
						return Equals(other);
					}
					return false;
				}

				public override int GetHashCode()
				{
					return ElementType.GetHashCode() ^ Rank.GetHashCode();
				}
			}

			private readonly bool _multiDim;

			private readonly int _rank;

			private static readonly CoreType[] s_typesImplementedByArray = new CoreType[4]
			{
				CoreType.IEnumerableT,
				CoreType.ICollectionT,
				CoreType.IListT,
				CoreType.IReadOnlyListT
			};

			public sealed override bool IsSZArray => !_multiDim;

			public sealed override bool IsVariableBoundArray => _multiDim;

			protected sealed override string Suffix => Helpers.ComputeArraySuffix(_rank, _multiDim);

			internal RoArrayType(RoType elementType, bool multiDim, int rank)
				: base(elementType)
			{
				_multiDim = multiDim;
				_rank = rank;
			}

			protected sealed override bool IsArrayImpl()
			{
				return true;
			}

			protected sealed override bool IsByRefImpl()
			{
				return false;
			}

			protected sealed override bool IsPointerImpl()
			{
				return false;
			}

			public sealed override int GetArrayRank()
			{
				return _rank;
			}

			protected sealed override RoType ComputeBaseTypeWithoutDesktopQuirk()
			{
				return base.Loader.GetCoreType(CoreType.Array);
			}

			protected sealed override IEnumerable<RoType> ComputeDirectlyImplementedInterfaces()
			{
				if (_multiDim)
				{
					yield break;
				}
				RoType[] typeArguments = new RoType[1] { GetRoElementType() };
				CoreType[] array = s_typesImplementedByArray;
				foreach (CoreType coreType in array)
				{
					RoType roType = base.Loader.TryGetCoreType(coreType);
					if (roType != null && roType is RoDefinitionType roDefinitionType && roDefinitionType.GetGenericParameterCount() == 1)
					{
						yield return roDefinitionType.GetUniqueConstructedGenericType(typeArguments);
					}
				}
			}

			protected sealed override TypeAttributes ComputeAttributeFlags()
			{
				return TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Serializable;
			}

			internal sealed override IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter)
			{
				if (filter != null && !filter.Matches(ConstructorInfo.ConstructorName))
				{
					yield break;
				}
				int rank = _rank;
				bool multiDim = _multiDim;
				RoType systemInt32 = base.Loader.GetCoreType(CoreType.Int32);
				int uniquifier = 0;
				RoType[] array = new RoType[rank];
				for (int i = 0; i < rank; i++)
				{
					array[i] = systemInt32;
				}
				yield return new RoSyntheticConstructor(this, uniquifier++, array);
				if (!multiDim)
				{
					int parameterCount = 2;
					RoType elementType = GetRoElementType();
					while (elementType.IsSZArray)
					{
						RoType[] array2 = new RoType[parameterCount];
						for (int j = 0; j < parameterCount; j++)
						{
							array2[j] = systemInt32;
						}
						yield return new RoSyntheticConstructor(this, uniquifier++, array2);
						parameterCount++;
						elementType = elementType.GetRoElementType();
					}
				}
				if (multiDim)
				{
					RoType[] array3 = new RoType[rank * 2];
					for (int k = 0; k < rank * 2; k++)
					{
						array3[k] = systemInt32;
					}
					yield return new RoSyntheticConstructor(this, uniquifier, array3);
				}
			}

			internal sealed override IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType)
			{
				int rank = _rank;
				RoType systemInt32 = base.Loader.GetCoreType(CoreType.Int32);
				RoType elementType = GetRoElementType();
				RoType systemVoid = base.Loader.GetCoreType(CoreType.Void);
				if (filter == null || filter.Matches("Get"))
				{
					RoType[] array = new RoType[rank];
					for (int i = 0; i < rank; i++)
					{
						array[i] = systemInt32;
					}
					yield return new RoSyntheticMethod(this, 0, "Get", elementType, array);
				}
				if (filter == null || filter.Matches("Set"))
				{
					RoType[] array2 = new RoType[rank + 1];
					for (int j = 0; j < rank; j++)
					{
						array2[j] = systemInt32;
					}
					array2[rank] = elementType;
					yield return new RoSyntheticMethod(this, 1, "Set", systemVoid, array2);
				}
				if (filter == null || filter.Matches("Address"))
				{
					RoType[] array3 = new RoType[rank];
					for (int k = 0; k < rank; k++)
					{
						array3[k] = systemInt32;
					}
					yield return new RoSyntheticMethod(this, 2, "Address", elementType.GetUniqueByRefType(), array3);
				}
			}
		}

		/// <summary>
		/// Base class for all Assembly objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoAssembly : LeveledAssembly
		{
			/// <summary>
			/// Intentionally excludes forwards to nested types.
			/// </summary>
			protected delegate void TypeForwardHandler(RoAssembly redirectedAssembly, ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name);

			private readonly RoModule[] _loadedModules;

			private volatile AssemblyNameData _lazyAssemblyNameData;

			private volatile string _lazyFullName;

			internal const string ThrowingMessageInRAF = "This member throws an exception for assemblies embedded in a single-file app";

			private volatile AssemblyNameData[] _lazyAssemblyReferences;

			public sealed override Module ManifestModule => GetRoManifestModule();

			protected bool IsSingleModule { get; }

			public sealed override string FullName => _lazyFullName ?? (_lazyFullName = GetName().FullName);

			public abstract override string Location { get; }

			public sealed override string CodeBase
			{
				get
				{
					throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_AssemblyCodeBase);
				}
			}

			public sealed override string EscapedCodeBase
			{
				get
				{
					throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_AssemblyCodeBase);
				}
			}

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			public sealed override IEnumerable<TypeInfo> DefinedTypes => GetDefinedRoTypes();

			public sealed override IEnumerable<Type> ExportedTypes
			{
				get
				{
					foreach (RoType definedRoType in GetDefinedRoTypes())
					{
						if (definedRoType.IsVisibleOutsideAssembly())
						{
							yield return definedRoType;
						}
					}
				}
			}

			public sealed override bool ReflectionOnly => true;

			public sealed override bool GlobalAssemblyCache => false;

			public sealed override long HostContext => 0L;

			public abstract override string ImageRuntimeVersion { get; }

			public abstract override bool IsDynamic { get; }

			public abstract override MethodInfo EntryPoint { get; }

			internal MetadataLoadContext Loader { get; }

			public abstract override event ModuleResolveEventHandler ModuleResolve;

			protected RoAssembly(MetadataLoadContext loader, int assemblyFileCount)
			{
				Loader = loader;
				IsSingleModule = assemblyFileCount == 0;
				_loadedModules = ((assemblyFileCount == 0) ? Array.Empty<RoModule>() : new RoModule[assemblyFileCount]);
			}

			internal abstract RoModule GetRoManifestModule();

			public sealed override string ToString()
			{
				return Loader.GetDisposedString() ?? base.ToString();
			}

			public sealed override AssemblyName GetName(bool copiedName)
			{
				return GetAssemblyNameDataNoCopy().CreateAssemblyName();
			}

			internal AssemblyNameData GetAssemblyNameDataNoCopy()
			{
				return _lazyAssemblyNameData ?? (_lazyAssemblyNameData = ComputeNameData());
			}

			protected abstract AssemblyNameData ComputeNameData();

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			public sealed override Type[] GetTypes()
			{
				if (!IsSingleModule)
				{
					return base.GetTypes();
				}
				return ManifestModule.GetTypes();
			}

			private IEnumerable<RoType> GetDefinedRoTypes()
			{
				if (!IsSingleModule)
				{
					return MultiModuleGetDefinedRoTypes();
				}
				return GetRoManifestModule().GetDefinedRoTypes();
			}

			private IEnumerable<RoType> MultiModuleGetDefinedRoTypes()
			{
				RoModule[] array = ComputeRoModules(getResourceModules: false);
				foreach (RoModule roModule in array)
				{
					foreach (RoType definedRoType in roModule.GetDefinedRoTypes())
					{
						yield return definedRoType;
					}
				}
			}

			public sealed override Type[] GetExportedTypes()
			{
				List<Type> list = new List<Type>(ExportedTypes);
				return list.ToArray();
			}

			public sealed override Type GetType(string name, bool throwOnError, bool ignoreCase)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				return Helpers.LoadTypeFromAssemblyQualifiedName(name, this, ignoreCase, throwOnError);
			}

			/// <summary>
			/// Helper routine for the more general Assembly.GetType() family of apis. Also used in typeRef resolution.
			///
			/// Resolves top-level named types only. No nested types. No constructed types. The input name must not be escaped.
			///
			/// If a type is not contained or forwarded from the assembly, this method returns null (does not throw.)
			/// This supports the "throwOnError: false" behavior of Assembly.GetType(string, bool).
			/// </summary>
			internal RoDefinitionType GetTypeCore(string ns, string name, bool ignoreCase, out Exception e)
			{
				return GetTypeCore(ns.ToUtf8(), name.ToUtf8(), ignoreCase, out e);
			}

			internal RoDefinitionType GetTypeCore(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, bool ignoreCase, out Exception e)
			{
				RoDefinitionType typeCore = GetRoManifestModule().GetTypeCore(ns, name, ignoreCase, out e);
				if (IsSingleModule || typeCore != null)
				{
					return typeCore;
				}
				RoModule[] array = ComputeRoModules(getResourceModules: false);
				foreach (RoModule roModule in array)
				{
					if (!(roModule == ManifestModule))
					{
						typeCore = roModule.GetTypeCore(ns, name, ignoreCase, out e);
						if (typeCore != null)
						{
							return typeCore;
						}
					}
				}
				return null;
			}

			public sealed override AssemblyName[] GetReferencedAssemblies()
			{
				AssemblyNameData[] referencedAssembliesNoCopy = GetReferencedAssembliesNoCopy();
				AssemblyName[] array = new AssemblyName[referencedAssembliesNoCopy.Length];
				for (int i = 0; i < referencedAssembliesNoCopy.Length; i++)
				{
					array[i] = referencedAssembliesNoCopy[i].CreateAssemblyName();
				}
				return array;
			}

			private AssemblyNameData[] GetReferencedAssembliesNoCopy()
			{
				return _lazyAssemblyReferences ?? (_lazyAssemblyReferences = ComputeAssemblyReferences());
			}

			protected abstract AssemblyNameData[] ComputeAssemblyReferences();

			public abstract override ManifestResourceInfo GetManifestResourceInfo(string resourceName);

			public abstract override string[] GetManifestResourceNames();

			public abstract override Stream GetManifestResourceStream(string name);

			public sealed override Stream GetManifestResourceStream(Type type, string name)
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (type == null)
				{
					if (name == null)
					{
						throw new ArgumentNullException("type");
					}
				}
				else
				{
					string @namespace = type.Namespace;
					if (@namespace != null)
					{
						stringBuilder.Append(@namespace);
						if (name != null)
						{
							stringBuilder.Append(Type.Delimiter);
						}
					}
				}
				if (name != null)
				{
					stringBuilder.Append(name);
				}
				return GetManifestResourceStream(stringBuilder.ToString());
			}

			public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				throw new NotSupportedException();
			}

			public sealed override Assembly GetSatelliteAssembly(CultureInfo culture)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_SatelliteAssembly);
			}

			public sealed override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_SatelliteAssembly);
			}

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Arg_ReflectionOnlyInvoke);
			}

			public sealed override Type[] GetForwardedTypes()
			{
				List<Type> types = new List<Type>();
				List<Exception> exceptions = null;
				IterateTypeForwards(delegate(RoAssembly redirectedAssembly, ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
				{
					Type type = null;
					Exception item = null;
					if (redirectedAssembly is RoExceptionAssembly roExceptionAssembly)
					{
						item = roExceptionAssembly.Exception;
					}
					else
					{
						type = redirectedAssembly.GetTypeCore(ns, name, ignoreCase: false, out var e);
						if (type == null)
						{
							item = e;
						}
					}
					if (type != null)
					{
						types.Add(type);
						AddPublicNestedTypes(type, types);
					}
					else
					{
						if (exceptions == null)
						{
							exceptions = new List<Exception>();
						}
						exceptions.Add(item);
					}
				});
				if (exceptions != null)
				{
					int count = types.Count;
					int count2 = exceptions.Count;
					types.AddRange(new Type[count2]);
					exceptions.InsertRange(0, new Exception[count]);
					throw new ReflectionTypeLoadException(types.ToArray(), exceptions.ToArray());
				}
				return types.ToArray();
			}

			private static void AddPublicNestedTypes(Type type, List<Type> types)
			{
				Type[] nestedTypes = type.GetNestedTypes(BindingFlags.Public);
				foreach (Type type2 in nestedTypes)
				{
					types.Add(type2);
					AddPublicNestedTypes(type2, types);
				}
			}

			protected abstract void IterateTypeForwards(TypeForwardHandler handler);

			public sealed override Module GetModule(string name)
			{
				return GetRoModule(name);
			}

			public sealed override Module[] GetModules(bool getResourceModules)
			{
				Module[] original = ComputeRoModules(getResourceModules);
				return original.CloneArray();
			}

			public sealed override FileStream GetFile(string name)
			{
				Module module = GetModule(name);
				if (module == null)
				{
					return null;
				}
				return new FileStream(module.FullyQualifiedName, FileMode.Open, FileAccess.Read, FileShare.Read);
			}

			public sealed override FileStream[] GetFiles(bool getResourceModules)
			{
				Module[] modules = GetModules(getResourceModules);
				FileStream[] array = new FileStream[modules.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = new FileStream(modules[i].FullyQualifiedName, FileMode.Open, FileAccess.Read, FileShare.Read);
				}
				return array;
			}

			public sealed override Module[] GetLoadedModules(bool getResourceModules)
			{
				List<Module> list = new List<Module>(_loadedModules.Length + 1) { GetRoManifestModule() };
				for (int i = 0; i < _loadedModules.Length; i++)
				{
					RoModule roModule = Volatile.Read(ref _loadedModules[i]);
					if (roModule != null && (getResourceModules || !roModule.IsResource()))
					{
						list.Add(roModule);
					}
				}
				return list.ToArray();
			}

			internal RoModule GetRoModule(string name)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				if (!TryGetAssemblyFileInfo(name, includeManifestModule: true, out var afi))
				{
					return null;
				}
				return GetRoModule(in afi);
			}

			private RoModule GetRoModule(in AssemblyFileInfo afi)
			{
				if (afi.RowIndex == 0)
				{
					return GetRoManifestModule();
				}
				int num = afi.RowIndex - 1;
				string name = afi.Name;
				RoModule roModule = Volatile.Read(ref _loadedModules[num]);
				if (roModule != null)
				{
					return roModule;
				}
				RoModule roModule2 = LoadModule(name, afi.ContainsMetadata);
				return Interlocked.CompareExchange(ref _loadedModules[num], roModule2, null) ?? roModule2;
			}

			internal RoModule[] ComputeRoModules(bool getResourceModules)
			{
				List<RoModule> list = new List<RoModule>(_loadedModules.Length + 1);
				foreach (AssemblyFileInfo item in GetAssemblyFileInfosFromManifest(includeManifestModule: true, getResourceModules))
				{
					AssemblyFileInfo afi = item;
					RoModule roModule = GetRoModule(in afi);
					list.Add(roModule);
				}
				return list.ToArray();
			}

			public sealed override Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore)
			{
				if (moduleName == null)
				{
					throw new ArgumentNullException("moduleName");
				}
				if (rawModule == null)
				{
					throw new ArgumentNullException("rawModule");
				}
				if (!TryGetAssemblyFileInfo(moduleName, includeManifestModule: false, out var afi))
				{
					throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.SpecifiedFileNameInvalid, moduleName));
				}
				int num = afi.RowIndex - 1;
				RoModule roModule = CreateModule(new MemoryStream(rawModule), afi.ContainsMetadata);
				Interlocked.CompareExchange(ref _loadedModules[num], roModule, null);
				return roModule;
			}

			private bool TryGetAssemblyFileInfo(string name, bool includeManifestModule, out AssemblyFileInfo afi)
			{
				foreach (AssemblyFileInfo item in GetAssemblyFileInfosFromManifest(includeManifestModule, includeResourceModules: true))
				{
					if (name.Equals(item.Name, StringComparison.OrdinalIgnoreCase))
					{
						afi = item;
						return true;
					}
				}
				afi = default(AssemblyFileInfo);
				return false;
			}

			protected abstract RoModule LoadModule(string moduleName, bool containsMetadata);

			protected abstract RoModule CreateModule(Stream peStream, bool containsMetadata);

			protected abstract IEnumerable<AssemblyFileInfo> GetAssemblyFileInfosFromManifest(bool includeManifestModule, bool includeResourceModules);
		}

		internal sealed class RoAssemblyName : IEquatable<RoAssemblyName>
		{
			public byte[] PublicKeyToken;

			private static readonly Version s_Version0000 = new Version(0, 0, 0, 0);

			public string Name { get; }

			public Version Version { get; }

			public string CultureName { get; }

			public AssemblyNameFlags Flags { get; }

			public string FullName => ToAssemblyName().FullName;

			public RoAssemblyName(string name, Version version, string cultureName, byte[] publicKeyToken, AssemblyNameFlags flags)
			{
				Name = name;
				Version = version ?? s_Version0000;
				CultureName = cultureName ?? string.Empty;
				PublicKeyToken = publicKeyToken ?? Array.Empty<byte>();
				Flags = flags;
			}

			public bool Equals(RoAssemblyName other)
			{
				if (Name != other.Name)
				{
					return false;
				}
				if (Version != other.Version)
				{
					return false;
				}
				if (CultureName != other.CultureName)
				{
					return false;
				}
				if (!((ReadOnlySpan<byte>)PublicKeyToken).SequenceEqual((ReadOnlySpan<byte>)other.PublicKeyToken))
				{
					return false;
				}
				return true;
			}

			public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
			{
				if (obj is RoAssemblyName other)
				{
					return Equals(other);
				}
				return false;
			}

			public sealed override int GetHashCode()
			{
				return Name.GetHashCode();
			}

			public sealed override string ToString()
			{
				return FullName;
			}

			public AssemblyName ToAssemblyName()
			{
				AssemblyName assemblyName = new AssemblyName
				{
					Name = Name,
					Version = Version,
					CultureName = CultureName,
					Flags = Flags
				};
				assemblyName.SetPublicKeyToken(PublicKeyToken.CloneArray());
				return assemblyName;
			}
		}

		/// <summary>
		/// All RoTypes that return true for IsByRef.
		/// </summary>
		internal sealed class RoByRefType : RoHasElementType
		{
			public sealed override bool IsSZArray => false;

			public sealed override bool IsVariableBoundArray => false;

			protected sealed override string Suffix => "&";

			internal RoByRefType(RoType elementType)
				: base(elementType)
			{
			}

			protected sealed override bool IsArrayImpl()
			{
				return false;
			}

			protected sealed override bool IsByRefImpl()
			{
				return true;
			}

			protected sealed override bool IsPointerImpl()
			{
				return false;
			}

			public sealed override int GetArrayRank()
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Argument_HasToBeArrayClass);
			}

			protected sealed override TypeAttributes ComputeAttributeFlags()
			{
				return TypeAttributes.Public;
			}

			protected sealed override RoType ComputeBaseTypeWithoutDesktopQuirk()
			{
				return null;
			}

			protected sealed override IEnumerable<RoType> ComputeDirectlyImplementedInterfaces()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter)
			{
				return Array.Empty<ConstructorInfo>();
			}

			internal sealed override IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<MethodInfo>();
			}
		}

		/// <summary>
		/// Class for all RoMethod objects created by a MetadataLoadContext for which IsConstructedGenericMethod returns true.
		/// </summary>
		internal sealed class RoConstructedGenericMethod : RoMethod
		{
			private readonly RoDefinitionMethod _genericMethodDefinition;

			private readonly RoType[] _genericMethodArguments;

			public sealed override int MetadataToken => _genericMethodDefinition.MetadataToken;

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => _genericMethodDefinition.CustomAttributes;

			public sealed override bool IsConstructedGenericMethod => true;

			public sealed override bool IsGenericMethodDefinition => false;

			public sealed override TypeContext TypeContext => new TypeContext(_genericMethodDefinition.TypeContext.GenericTypeArguments, _genericMethodArguments);

			internal RoConstructedGenericMethod(RoDefinitionMethod genericMethodDefinition, RoType[] genericMethodArguments)
				: base(genericMethodDefinition.ReflectedType)
			{
				_genericMethodDefinition = genericMethodDefinition;
				_genericMethodArguments = genericMethodArguments;
			}

			internal sealed override RoType GetRoDeclaringType()
			{
				return _genericMethodDefinition.GetRoDeclaringType();
			}

			internal sealed override RoModule GetRoModule()
			{
				return _genericMethodDefinition.GetRoModule();
			}

			protected sealed override string ComputeName()
			{
				return _genericMethodDefinition.Name;
			}

			protected sealed override MethodAttributes ComputeAttributes()
			{
				return _genericMethodDefinition.Attributes;
			}

			protected sealed override CallingConventions ComputeCallingConvention()
			{
				return _genericMethodDefinition.CallingConvention;
			}

			protected sealed override MethodImplAttributes ComputeMethodImplementationFlags()
			{
				return _genericMethodDefinition.MethodImplementationFlags;
			}

			protected sealed override MethodSig<RoParameter> ComputeMethodSig()
			{
				return _genericMethodDefinition.SpecializeMethodSig(this);
			}

			protected sealed override MethodSig<RoType> ComputeCustomModifiers()
			{
				RoDefinitionMethod genericMethodDefinition = _genericMethodDefinition;
				TypeContext typeContext = TypeContext;
				return genericMethodDefinition.SpecializeCustomModifiers(in typeContext);
			}

			public sealed override MethodBody GetMethodBody()
			{
				return _genericMethodDefinition.SpecializeMethodBody(this);
			}

			protected sealed override RoType[] ComputeGenericArgumentsOrParameters()
			{
				return _genericMethodArguments;
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				return _genericMethodArguments;
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				return Array.Empty<RoType>();
			}

			public sealed override MethodInfo GetGenericMethodDefinition()
			{
				return _genericMethodDefinition;
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override MethodInfo MakeGenericMethod(params Type[] typeArguments)
			{
				throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.Arg_NotGenericMethodDefinition, this));
			}

			public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
			{
				if (!(obj is RoConstructedGenericMethod roConstructedGenericMethod))
				{
					return false;
				}
				if (!(_genericMethodDefinition == roConstructedGenericMethod._genericMethodDefinition))
				{
					return false;
				}
				if (_genericMethodArguments.Length != roConstructedGenericMethod._genericMethodArguments.Length)
				{
					return false;
				}
				for (int i = 0; i < _genericMethodArguments.Length; i++)
				{
					if (_genericMethodArguments[i] != roConstructedGenericMethod._genericMethodArguments[i])
					{
						return false;
					}
				}
				return true;
			}

			public sealed override int GetHashCode()
			{
				int num = _genericMethodDefinition.GetHashCode();
				RoType[] genericMethodArguments = _genericMethodArguments;
				foreach (Type type in genericMethodArguments)
				{
					num ^= type.GetHashCode();
				}
				return num;
			}

			protected sealed override MethodSig<string> ComputeMethodSigStrings()
			{
				RoDefinitionMethod genericMethodDefinition = _genericMethodDefinition;
				TypeContext typeContext = TypeContext;
				return genericMethodDefinition.SpecializeMethodSigStrings(in typeContext);
			}
		}

		/// <summary>
		/// All RoTypes that return true for IsConstructedGenericType.
		/// </summary>
		internal sealed class RoConstructedGenericType : RoInstantiationProviderType
		{
			public readonly struct Key : IEquatable<Key>
			{
				public RoDefinitionType GenericTypeDefinition { get; }

				public RoType[] GenericTypeArguments { get; }

				public Key(RoDefinitionType genericTypeDefinition, RoType[] genericTypeArguments)
				{
					GenericTypeDefinition = genericTypeDefinition;
					GenericTypeArguments = genericTypeArguments;
				}

				public bool Equals(Key other)
				{
					if (GenericTypeDefinition != other.GenericTypeDefinition)
					{
						return false;
					}
					if (GenericTypeArguments.Length != other.GenericTypeArguments.Length)
					{
						return false;
					}
					for (int i = 0; i < GenericTypeArguments.Length; i++)
					{
						if (GenericTypeArguments[i] != other.GenericTypeArguments[i])
						{
							return false;
						}
					}
					return true;
				}

				public override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
				{
					if (obj is Key other)
					{
						return Equals(other);
					}
					return false;
				}

				public override int GetHashCode()
				{
					int num = GenericTypeDefinition.GetHashCode();
					for (int i = 0; i < GenericTypeArguments.Length; i++)
					{
						num ^= GenericTypeArguments[i].GetHashCode();
					}
					return num;
				}
			}

			private readonly RoDefinitionType _genericTypeDefinition;

			private readonly RoType[] _genericTypeArguments;

			public sealed override bool IsTypeDefinition => false;

			public sealed override bool IsGenericTypeDefinition => false;

			public sealed override bool IsSZArray => false;

			public sealed override bool IsVariableBoundArray => false;

			public sealed override bool IsConstructedGenericType => true;

			public sealed override bool IsGenericParameter => false;

			public sealed override bool IsGenericTypeParameter => false;

			public sealed override bool IsGenericMethodParameter => false;

			public sealed override bool ContainsGenericParameters
			{
				get
				{
					RoType[] genericTypeArguments = _genericTypeArguments;
					foreach (RoType roType in genericTypeArguments)
					{
						if (roType.ContainsGenericParameters)
						{
							return true;
						}
					}
					return false;
				}
			}

			public sealed override MethodBase DeclaringMethod
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => _genericTypeDefinition.CustomAttributes;

			public sealed override int MetadataToken => _genericTypeDefinition.MetadataToken;

			public sealed override Guid GUID => _genericTypeDefinition.GUID;

			public sealed override StructLayoutAttribute StructLayoutAttribute => _genericTypeDefinition.StructLayoutAttribute;

			public sealed override GenericParameterAttributes GenericParameterAttributes
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			public sealed override int GenericParameterPosition
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			internal sealed override RoType[] Instantiation => _genericTypeArguments;

			internal RoConstructedGenericType(RoDefinitionType genericTypeDefinition, RoType[] genericTypeArguments)
			{
				_genericTypeDefinition = genericTypeDefinition;
				_genericTypeArguments = genericTypeArguments;
			}

			protected sealed override bool HasElementTypeImpl()
			{
				return false;
			}

			protected sealed override bool IsArrayImpl()
			{
				return false;
			}

			protected sealed override bool IsByRefImpl()
			{
				return false;
			}

			protected sealed override bool IsPointerImpl()
			{
				return false;
			}

			internal sealed override RoModule GetRoModule()
			{
				return _genericTypeDefinition.GetRoModule();
			}

			protected sealed override string ComputeName()
			{
				return _genericTypeDefinition.Name;
			}

			protected sealed override string ComputeNamespace()
			{
				return _genericTypeDefinition.Namespace;
			}

			protected sealed override string ComputeFullName()
			{
				if (ContainsGenericParameters)
				{
					return null;
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(_genericTypeDefinition.FullName);
				stringBuilder.Append('[');
				for (int i = 0; i < _genericTypeArguments.Length; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(',');
					}
					stringBuilder.Append('[');
					stringBuilder.Append(_genericTypeArguments[i].AssemblyQualifiedName);
					stringBuilder.Append(']');
				}
				stringBuilder.Append(']');
				return stringBuilder.ToString();
			}

			public sealed override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(_genericTypeDefinition.ToString());
				stringBuilder.Append('[');
				for (int i = 0; i < _genericTypeArguments.Length; i++)
				{
					if (i != 0)
					{
						stringBuilder.Append(',');
					}
					stringBuilder.Append(_genericTypeArguments[i].ToString());
				}
				stringBuilder.Append(']');
				return stringBuilder.ToString();
			}

			protected sealed override RoType ComputeDeclaringType()
			{
				return _genericTypeDefinition.GetRoDeclaringType();
			}

			protected sealed override RoType ComputeBaseTypeWithoutDesktopQuirk()
			{
				return _genericTypeDefinition.SpecializeBaseType(Instantiation);
			}

			protected sealed override IEnumerable<RoType> ComputeDirectlyImplementedInterfaces()
			{
				return _genericTypeDefinition.SpecializeInterfaces(Instantiation);
			}

			internal sealed override bool IsCustomAttributeDefined(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				return _genericTypeDefinition.IsCustomAttributeDefined(ns, name);
			}

			internal sealed override CustomAttributeData TryFindCustomAttribute(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				return _genericTypeDefinition.TryFindCustomAttribute(ns, name);
			}

			protected sealed override TypeAttributes ComputeAttributeFlags()
			{
				return _genericTypeDefinition.Attributes;
			}

			protected sealed override TypeCode GetTypeCodeImpl()
			{
				return Type.GetTypeCode(_genericTypeDefinition);
			}

			internal sealed override RoType GetRoElementType()
			{
				return null;
			}

			public sealed override Type GetGenericTypeDefinition()
			{
				return _genericTypeDefinition;
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				return _genericTypeArguments;
			}

			protected internal sealed override RoType[] GetGenericArgumentsNoCopy()
			{
				return _genericTypeArguments;
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override Type MakeGenericType(params Type[] typeArguments)
			{
				throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.Arg_NotGenericTypeDefinition, this));
			}

			protected internal sealed override RoType ComputeEnumUnderlyingType()
			{
				return _genericTypeDefinition.ComputeEnumUnderlyingType();
			}

			public sealed override int GetArrayRank()
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Argument_HasToBeArrayClass);
			}

			public sealed override Type[] GetGenericParameterConstraints()
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
			}

			internal sealed override IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter)
			{
				return _genericTypeDefinition.SpecializeConstructors(filter, this);
			}

			internal sealed override IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType)
			{
				return _genericTypeDefinition.SpecializeMethods(filter, reflectedType, this);
			}

			internal sealed override IEnumerable<EventInfo> GetEventsCore(NameFilter filter, Type reflectedType)
			{
				return _genericTypeDefinition.SpecializeEvents(filter, reflectedType, this);
			}

			internal sealed override IEnumerable<FieldInfo> GetFieldsCore(NameFilter filter, Type reflectedType)
			{
				return _genericTypeDefinition.SpecializeFields(filter, reflectedType, this);
			}

			internal sealed override IEnumerable<PropertyInfo> GetPropertiesCore(NameFilter filter, Type reflectedType)
			{
				return _genericTypeDefinition.SpecializeProperties(filter, reflectedType, this);
			}

			internal sealed override IEnumerable<RoType> GetNestedTypesCore(NameFilter filter)
			{
				return _genericTypeDefinition.GetNestedTypesCore(filter);
			}
		}

		/// <summary>
		/// Base class for all ConstructorInfo objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoConstructor : LeveledConstructorInfo, IRoMethodBase
		{
			private volatile string _lazyName;

			private const MethodAttributes MethodAttributesSentinel = (MethodAttributes)(-1);

			private volatile MethodAttributes _lazyMethodAttributes = (MethodAttributes)(-1);

			private const CallingConventions CallingConventionsSentinel = (CallingConventions)(-1);

			private volatile CallingConventions _lazyCallingConventions = (CallingConventions)(-1);

			private const MethodImplAttributes MethodImplAttributesSentinel = (MethodImplAttributes)(-1);

			private volatile MethodImplAttributes _lazyMethodImplAttributes = (MethodImplAttributes)(-1);

			private volatile MethodSig<RoParameter> _lazyMethodSig;

			private volatile MethodSig<RoType> _lazyCustomModifiers;

			public sealed override Type DeclaringType => GetRoDeclaringType();

			public sealed override Type ReflectedType => DeclaringType;

			public sealed override string Name => _lazyName ?? (_lazyName = ComputeName());

			public sealed override Module Module => GetRoModule();

			public abstract override int MetadataToken { get; }

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			public sealed override bool IsConstructedGenericMethod => false;

			public sealed override bool IsGenericMethodDefinition => false;

			public sealed override bool IsGenericMethod => false;

			public sealed override MethodAttributes Attributes
			{
				get
				{
					if (_lazyMethodAttributes != (MethodAttributes)(-1))
					{
						return _lazyMethodAttributes;
					}
					return _lazyMethodAttributes = ComputeAttributes();
				}
			}

			public sealed override CallingConventions CallingConvention
			{
				get
				{
					if (_lazyCallingConventions != (CallingConventions)(-1))
					{
						return _lazyCallingConventions;
					}
					return _lazyCallingConventions = ComputeCallingConvention();
				}
			}

			public sealed override MethodImplAttributes MethodImplementationFlags
			{
				get
				{
					if (_lazyMethodImplAttributes != (MethodImplAttributes)(-1))
					{
						return _lazyMethodImplAttributes;
					}
					return _lazyMethodImplAttributes = ComputeMethodImplementationFlags();
				}
			}

			public sealed override bool ContainsGenericParameters => GetRoDeclaringType().ContainsGenericParameters;

			private MethodSig<RoParameter> MethodSig => _lazyMethodSig ?? (_lazyMethodSig = ComputeMethodSig());

			private MethodSig<RoType> CustomModifiers => _lazyCustomModifiers ?? (_lazyCustomModifiers = ComputeCustomModifiers());

			public sealed override bool IsSecurityCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecuritySafeCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecurityTransparent
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override RuntimeMethodHandle MethodHandle
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
				}
			}

			MethodBase IRoMethodBase.MethodBase => this;

			public MetadataLoadContext Loader => GetRoModule().Loader;

			public abstract TypeContext TypeContext { get; }

			public abstract override bool Equals(object obj);

			public abstract override int GetHashCode();

			internal abstract RoType GetRoDeclaringType();

			protected abstract string ComputeName();

			internal abstract RoModule GetRoModule();

			public sealed override bool HasSameMetadataDefinitionAs(MemberInfo other)
			{
				return this.HasSameMetadataDefinitionAsCore(other);
			}

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			protected abstract MethodAttributes ComputeAttributes();

			protected abstract CallingConventions ComputeCallingConvention();

			protected abstract MethodImplAttributes ComputeMethodImplementationFlags();

			public sealed override MethodImplAttributes GetMethodImplementationFlags()
			{
				return MethodImplementationFlags;
			}

			public abstract override MethodBody GetMethodBody();

			public sealed override ParameterInfo[] GetParameters()
			{
				ParameterInfo[] parametersNoCopy = GetParametersNoCopy();
				return parametersNoCopy.CloneArray();
			}

			internal RoParameter[] GetParametersNoCopy()
			{
				return MethodSig.Parameters;
			}

			protected abstract MethodSig<RoParameter> ComputeMethodSig();

			protected abstract MethodSig<RoType> ComputeCustomModifiers();

			public sealed override string ToString()
			{
				return Loader.GetDisposedString() ?? this.ToString(ComputeMethodSigStrings());
			}

			protected abstract MethodSig<string> ComputeMethodSigStrings();

			public sealed override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyInvoke);
			}

			public sealed override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyInvoke);
			}

			Type[] IRoMethodBase.GetCustomModifiers(int position, bool isRequired)
			{
				return CustomModifiers[position].ExtractCustomModifiers(isRequired);
			}

			string IRoMethodBase.GetMethodSigString(int position)
			{
				return ComputeMethodSigStrings()[position];
			}
		}

		/// <summary>
		/// Base class for all CustomAttributeData objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoCustomAttributeData : LeveledCustomAttributeData
		{
			private volatile Type _lazyAttributeType;

			private volatile ConstructorInfo _lazyConstructorInfo;

			public sealed override Type AttributeType => _lazyAttributeType ?? (_lazyAttributeType = ComputeAttributeType());

			public sealed override ConstructorInfo Constructor => _lazyConstructorInfo ?? (_lazyConstructorInfo = ComputeConstructor());

			public abstract override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }

			public abstract override IList<CustomAttributeNamedArgument> NamedArguments { get; }

			protected abstract Type ComputeAttributeType();

			protected abstract ConstructorInfo ComputeConstructor();

			public sealed override string ToString()
			{
				return GetType().ToString();
			}
		}

		/// <summary>
		/// Class for all RoConstructor objects created by a MetadataLoadContext that has a MethodDef token associated with it.
		/// </summary>
		internal sealed class RoDefinitionConstructor<TMethodDecoder> : RoConstructor where TMethodDecoder : IMethodDecoder
		{
			private readonly RoInstantiationProviderType _declaringType;

			private readonly TMethodDecoder _decoder;

			public sealed override int MetadataToken
			{
				get
				{
					TMethodDecoder decoder = _decoder;
					return decoder.MetadataToken;
				}
			}

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes
			{
				get
				{
					TMethodDecoder decoder = _decoder;
					return decoder.ComputeTrueCustomAttributes();
				}
			}

			public sealed override TypeContext TypeContext => _declaringType.Instantiation.ToTypeContext();

			internal RoDefinitionConstructor(RoInstantiationProviderType declaringType, TMethodDecoder decoder)
			{
				_declaringType = declaringType;
				_decoder = decoder;
			}

			internal sealed override RoType GetRoDeclaringType()
			{
				return _declaringType;
			}

			internal sealed override RoModule GetRoModule()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.GetRoModule();
			}

			protected sealed override string ComputeName()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeName();
			}

			protected sealed override MethodAttributes ComputeAttributes()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeAttributes();
			}

			protected sealed override CallingConventions ComputeCallingConvention()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeCallingConvention();
			}

			protected sealed override MethodImplAttributes ComputeMethodImplementationFlags()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeMethodImplementationFlags();
			}

			protected sealed override MethodSig<RoParameter> ComputeMethodSig()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeMethodSig(this);
			}

			public sealed override MethodBody GetMethodBody()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeMethodBody(this);
			}

			protected sealed override MethodSig<string> ComputeMethodSigStrings()
			{
				TMethodDecoder decoder = _decoder;
				TypeContext typeContext = TypeContext;
				return decoder.SpecializeMethodSigStrings(in typeContext);
			}

			protected sealed override MethodSig<RoType> ComputeCustomModifiers()
			{
				TMethodDecoder decoder = _decoder;
				TypeContext typeContext = TypeContext;
				return decoder.SpecializeCustomModifiers(in typeContext);
			}

			public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
			{
				if (!(obj is RoDefinitionConstructor<TMethodDecoder> roDefinitionConstructor))
				{
					return false;
				}
				if (MetadataToken != roDefinitionConstructor.MetadataToken)
				{
					return false;
				}
				if (DeclaringType != roDefinitionConstructor.DeclaringType)
				{
					return false;
				}
				return true;
			}

			public sealed override int GetHashCode()
			{
				return MetadataToken.GetHashCode() ^ DeclaringType.GetHashCode();
			}
		}

		/// <summary>
		/// Base class for all RoMethod objects created by a MetadataLoadContext that has a MethodDef token associated with it
		/// and for which IsConstructedGenericMethod returns false.
		/// </summary>
		internal abstract class RoDefinitionMethod : RoMethod
		{
			protected RoDefinitionMethod(Type reflectedType)
				: base(reflectedType)
			{
			}

			internal abstract MethodSig<RoParameter> SpecializeMethodSig(IRoMethodBase member);

			internal abstract MethodSig<RoType> SpecializeCustomModifiers(in TypeContext typeContext);

			internal abstract MethodSig<string> SpecializeMethodSigStrings(in TypeContext typeContext);

			internal abstract MethodBody SpecializeMethodBody(IRoMethodBase owner);
		}
		
		/// <summary>
		/// Class for all RoMethod objects created by a MetadataLoadContext that has a MethodDef token associated with it
		/// and for which IsConstructedGenericMethod returns false.
		/// </summary>
		internal sealed class RoDefinitionMethod<TMethodDecoder> : RoDefinitionMethod where TMethodDecoder : IMethodDecoder
		{
			private readonly RoInstantiationProviderType _declaringType;

			private readonly TMethodDecoder _decoder;

			public sealed override int MetadataToken
			{
				get
				{
					TMethodDecoder decoder = _decoder;
					return decoder.MetadataToken;
				}
			}

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes
			{
				get
				{
					TMethodDecoder decoder = _decoder;
					foreach (CustomAttributeData item in decoder.ComputeTrueCustomAttributes())
					{
						yield return item;
					}
					if ((MethodImplementationFlags & MethodImplAttributes.PreserveSig) != 0)
					{
						ConstructorInfo constructorInfo = base.Loader.TryGetPreserveSigCtor();
						if (constructorInfo != null)
						{
							yield return new RoPseudoCustomAttributeData(constructorInfo);
						}
					}
					CustomAttributeData customAttributeData = ComputeDllImportCustomAttributeDataIfAny();
					if (customAttributeData != null)
					{
						yield return customAttributeData;
					}
				}
			}

			public sealed override bool IsConstructedGenericMethod => false;

			public sealed override bool IsGenericMethodDefinition => GetGenericTypeParametersNoCopy().Length != 0;

			public sealed override TypeContext TypeContext => new TypeContext(_declaringType.Instantiation, GetGenericTypeParametersNoCopy());

			internal RoDefinitionMethod(RoInstantiationProviderType declaringType, Type reflectedType, TMethodDecoder decoder)
				: base(reflectedType)
			{
				_declaringType = declaringType;
				_decoder = decoder;
			}

			internal sealed override RoType GetRoDeclaringType()
			{
				return _declaringType;
			}

			internal sealed override RoModule GetRoModule()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.GetRoModule();
			}

			protected sealed override string ComputeName()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeName();
			}

			protected sealed override MethodAttributes ComputeAttributes()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeAttributes();
			}

			protected sealed override CallingConventions ComputeCallingConvention()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeCallingConvention();
			}

			protected sealed override MethodImplAttributes ComputeMethodImplementationFlags()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeMethodImplementationFlags();
			}

			protected sealed override MethodSig<RoParameter> ComputeMethodSig()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeMethodSig(this);
			}

			public sealed override MethodBody GetMethodBody()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeMethodBody(this);
			}

			protected sealed override MethodSig<string> ComputeMethodSigStrings()
			{
				TMethodDecoder decoder = _decoder;
				TypeContext typeContext = TypeContext;
				return decoder.SpecializeMethodSigStrings(in typeContext);
			}

			protected sealed override MethodSig<RoType> ComputeCustomModifiers()
			{
				TMethodDecoder decoder = _decoder;
				TypeContext typeContext = TypeContext;
				return decoder.SpecializeCustomModifiers(in typeContext);
			}

			public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
			{
				if (!(obj is RoDefinitionMethod<TMethodDecoder> roDefinitionMethod))
				{
					return false;
				}
				if (MetadataToken != roDefinitionMethod.MetadataToken)
				{
					return false;
				}
				if (DeclaringType != roDefinitionMethod.DeclaringType)
				{
					return false;
				}
				if (ReflectedType != roDefinitionMethod.ReflectedType)
				{
					return false;
				}
				return true;
			}

			public sealed override int GetHashCode()
			{
				return MetadataToken.GetHashCode() ^ DeclaringType.GetHashCode();
			}

			public sealed override MethodInfo GetGenericMethodDefinition()
			{
				if (!IsGenericMethodDefinition)
				{
					throw new InvalidOperationException();
				}
				return this;
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override MethodInfo MakeGenericMethod(params Type[] typeArguments)
			{
				if (typeArguments == null)
				{
					throw new ArgumentNullException("typeArguments");
				}
				if (!IsGenericMethodDefinition)
				{
					throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.Arg_NotGenericMethodDefinition, this));
				}
				int num = typeArguments.Length;
				RoType[] array = new RoType[num];
				for (int i = 0; i < num; i++)
				{
					Type type = typeArguments[i];
					if (type == null)
					{
						throw new ArgumentNullException();
					}
					if (!(type is RoType roType) || roType.Loader != base.Loader)
					{
						throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.MakeGenericType_NotLoadedByMetadataLoadContext, type));
					}
					array[i] = roType;
				}
				if (num != GetGenericTypeParametersNoCopy().Length)
				{
					throw new ArgumentException(MDCFR.Properties.Resources.Argument_GenericArgsCount, "typeArguments");
				}
				return new RoConstructedGenericMethod(this, array);
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				return GetGenericArgumentsOrParametersNoCopy();
			}

			protected sealed override RoType[] ComputeGenericArgumentsOrParameters()
			{
				TMethodDecoder decoder = _decoder;
				return decoder.ComputeGenericArgumentsOrParameters();
			}

			internal sealed override MethodSig<RoParameter> SpecializeMethodSig(IRoMethodBase member)
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeMethodSig(member);
			}

			internal sealed override MethodSig<RoType> SpecializeCustomModifiers(in TypeContext typeContext)
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeCustomModifiers(in typeContext);
			}

			internal sealed override MethodSig<string> SpecializeMethodSigStrings(in TypeContext typeContext)
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeMethodSigStrings(in typeContext);
			}

			internal sealed override MethodBody SpecializeMethodBody(IRoMethodBase owner)
			{
				TMethodDecoder decoder = _decoder;
				return decoder.SpecializeMethodBody(owner);
			}

			private CustomAttributeData ComputeDllImportCustomAttributeDataIfAny()
			{
				if ((Attributes & MethodAttributes.PinvokeImpl) == 0)
				{
					return null;
				}
				CoreTypes ct = base.Loader.GetAllFoundCoreTypes();
				if (ct[CoreType.String] == null || ct[CoreType.Boolean] == null || ct[CoreType.DllImportAttribute] == null || ct[CoreType.CharSet] == null || ct[CoreType.CallingConvention] == null)
				{
					return null;
				}
				ConstructorInfo ctor = base.Loader.TryGetDllImportCtor();
				if (ctor == null)
				{
					return null;
				}
				Func<CustomAttributeArguments> argumentsPromise = delegate
				{
					Type declaringType = ctor.DeclaringType;
					TMethodDecoder decoder = _decoder;
					DllImportAttribute dllImportAttribute = decoder.ComputeDllImportAttribute();
					CustomAttributeTypedArgument[] fixedArguments = new CustomAttributeTypedArgument[1]
					{
						new CustomAttributeTypedArgument(ct[CoreType.String], dllImportAttribute.Value)
					};
					CustomAttributeNamedArgument[] namedArguments = new CustomAttributeNamedArgument[8]
					{
						declaringType.ToCustomAttributeNamedArgument("EntryPoint", ct[CoreType.String], dllImportAttribute.EntryPoint),
						declaringType.ToCustomAttributeNamedArgument("CharSet", ct[CoreType.CharSet], (int)dllImportAttribute.CharSet),
						declaringType.ToCustomAttributeNamedArgument("CallingConvention", ct[CoreType.CallingConvention], (int)dllImportAttribute.CallingConvention),
						declaringType.ToCustomAttributeNamedArgument("ExactSpelling", ct[CoreType.Boolean], dllImportAttribute.ExactSpelling),
						declaringType.ToCustomAttributeNamedArgument("PreserveSig", ct[CoreType.Boolean], dllImportAttribute.PreserveSig),
						declaringType.ToCustomAttributeNamedArgument("SetLastError", ct[CoreType.Boolean], dllImportAttribute.SetLastError),
						declaringType.ToCustomAttributeNamedArgument("BestFitMapping", ct[CoreType.Boolean], dllImportAttribute.BestFitMapping),
						declaringType.ToCustomAttributeNamedArgument("ThrowOnUnmappableChar", ct[CoreType.Boolean], dllImportAttribute.ThrowOnUnmappableChar)
					};
					return new CustomAttributeArguments(fixedArguments, namedArguments);
				};
				return new RoPseudoCustomAttributeData(ctor, argumentsPromise);
			}
		}

		/// <summary>
		/// Base type for all RoTypes that return true for IsTypeDefinition.
		/// </summary>
		internal abstract class RoDefinitionType : RoInstantiationProviderType
		{
			public sealed override bool IsTypeDefinition => true;

			public sealed override bool IsSZArray => false;

			public sealed override bool IsVariableBoundArray => false;

			public sealed override bool IsConstructedGenericType => false;

			public sealed override bool IsGenericParameter => false;

			public sealed override bool IsGenericTypeParameter => false;

			public sealed override bool IsGenericMethodParameter => false;

			public sealed override bool ContainsGenericParameters => IsGenericTypeDefinition;

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes
			{
				get
				{
					foreach (CustomAttributeData trueCustomAttribute in GetTrueCustomAttributes())
					{
						yield return trueCustomAttribute;
					}
					if ((base.Attributes & TypeAttributes.Import) != 0)
					{
						ConstructorInfo constructorInfo = base.Loader.TryGetComImportCtor();
						if (constructorInfo != null)
						{
							yield return new RoPseudoCustomAttributeData(constructorInfo);
						}
					}
				}
			}

			public sealed override Guid GUID
			{
				get
				{
					CustomAttributeData customAttributeData = TryFindCustomAttribute(Utf8Constants.SystemRuntimeInteropServices, Utf8Constants.GuidAttribute);
					if (customAttributeData == null)
					{
						return default(Guid);
					}
					IList<CustomAttributeTypedArgument> constructorArguments = customAttributeData.ConstructorArguments;
					if (constructorArguments.Count != 1)
					{
						return default(Guid);
					}
					CustomAttributeTypedArgument customAttributeTypedArgument = constructorArguments[0];
					if (customAttributeTypedArgument.ArgumentType != base.Loader.TryGetCoreType(CoreType.String))
					{
						return default(Guid);
					}
					if (!(customAttributeTypedArgument.Value is string g))
					{
						return default(Guid);
					}
					return new Guid(g);
				}
			}

			public sealed override StructLayoutAttribute StructLayoutAttribute
			{
				get
				{
					if (base.IsInterface)
					{
						return null;
					}
					TypeAttributes attributes = base.Attributes;
					LayoutKind layoutKind = (attributes & TypeAttributes.LayoutMask) switch
					{
						TypeAttributes.ExplicitLayout => LayoutKind.Explicit, 
						TypeAttributes.NotPublic => LayoutKind.Auto, 
						TypeAttributes.SequentialLayout => LayoutKind.Sequential, 
						_ => LayoutKind.Auto, 
					};
					CharSet charSet = (attributes & TypeAttributes.StringFormatMask) switch
					{
						TypeAttributes.NotPublic => CharSet.Ansi, 
						TypeAttributes.AutoClass => CharSet.Auto, 
						TypeAttributes.UnicodeClass => CharSet.Unicode, 
						_ => CharSet.None, 
					};
					GetPackSizeAndSize(out var packSize, out var size);
					return new StructLayoutAttribute(layoutKind)
					{
						CharSet = charSet,
						Pack = packSize,
						Size = size
					};
				}
			}

			public sealed override GenericParameterAttributes GenericParameterAttributes
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			public sealed override int GenericParameterPosition
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			public sealed override MethodBase DeclaringMethod
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			internal sealed override RoType[] Instantiation => GetGenericTypeParametersNoCopy();

			protected sealed override bool HasElementTypeImpl()
			{
				return false;
			}

			protected sealed override bool IsArrayImpl()
			{
				return false;
			}

			protected sealed override bool IsByRefImpl()
			{
				return false;
			}

			protected sealed override bool IsPointerImpl()
			{
				return false;
			}

			protected sealed override string ComputeFullName()
			{
				string name = Name;
				Type declaringType = DeclaringType;
				if (declaringType != null)
				{
					string fullName = declaringType.FullName;
					return fullName + "+" + name;
				}
				string @namespace = Namespace;
				if (@namespace == null)
				{
					return name;
				}
				return @namespace + "." + name;
			}

			public sealed override string ToString()
			{
				return base.Loader.GetDisposedString() ?? FullName;
			}

			internal abstract int GetGenericParameterCount();

			internal abstract override RoType[] GetGenericTypeParametersNoCopy();

			protected abstract IEnumerable<CustomAttributeData> GetTrueCustomAttributes();

			public sealed override Type GetGenericTypeDefinition()
			{
				if (!IsGenericTypeDefinition)
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_NotGenericType);
				}
				return this;
			}

			protected sealed override RoType ComputeBaseTypeWithoutDesktopQuirk()
			{
				return SpecializeBaseType(Instantiation);
			}

			internal abstract RoType SpecializeBaseType(RoType[] instantiation);

			protected sealed override IEnumerable<RoType> ComputeDirectlyImplementedInterfaces()
			{
				return SpecializeInterfaces(Instantiation);
			}

			internal abstract IEnumerable<RoType> SpecializeInterfaces(RoType[] instantiation);

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override Type MakeGenericType(params Type[] typeArguments)
			{
				if (typeArguments == null)
				{
					throw new ArgumentNullException("typeArguments");
				}
				if (!IsGenericTypeDefinition)
				{
					throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.Arg_NotGenericTypeDefinition, this));
				}
				int num = typeArguments.Length;
				if (num != GetGenericParameterCount())
				{
					throw new ArgumentException(MDCFR.Properties.Resources.Argument_GenericArgsCount, "typeArguments");
				}
				bool flag = false;
				RoType[] array = new RoType[num];
				for (int i = 0; i < num; i++)
				{
					Type type = typeArguments[i];
					if (type == null)
					{
						throw new ArgumentNullException();
					}
					if (type.IsSignatureType())
					{
						flag = true;
						continue;
					}
					if (!(type is RoType roType) || roType.Loader != base.Loader)
					{
						throw new ArgumentException(System.SR.Format(MDCFR.Properties.Resources.MakeGenericType_NotLoadedByMetadataLoadContext, type));
					}
					array[i] = roType;
				}
				if (flag)
				{
					return this.MakeSignatureGenericType(typeArguments);
				}
				return this.GetUniqueConstructedGenericType(array);
			}

			protected abstract void GetPackSizeAndSize(out int packSize, out int size);

			protected sealed override TypeCode GetTypeCodeImpl()
			{
				Type type = (IsEnum ? GetEnumUnderlyingType() : this);
				CoreTypes allFoundCoreTypes = base.Loader.GetAllFoundCoreTypes();
				if (type == allFoundCoreTypes[CoreType.Boolean])
				{
					return TypeCode.Boolean;
				}
				if (type == allFoundCoreTypes[CoreType.Char])
				{
					return TypeCode.Char;
				}
				if (type == allFoundCoreTypes[CoreType.SByte])
				{
					return TypeCode.SByte;
				}
				if (type == allFoundCoreTypes[CoreType.Byte])
				{
					return TypeCode.Byte;
				}
				if (type == allFoundCoreTypes[CoreType.Int16])
				{
					return TypeCode.Int16;
				}
				if (type == allFoundCoreTypes[CoreType.UInt16])
				{
					return TypeCode.UInt16;
				}
				if (type == allFoundCoreTypes[CoreType.Int32])
				{
					return TypeCode.Int32;
				}
				if (type == allFoundCoreTypes[CoreType.UInt32])
				{
					return TypeCode.UInt32;
				}
				if (type == allFoundCoreTypes[CoreType.Int64])
				{
					return TypeCode.Int64;
				}
				if (type == allFoundCoreTypes[CoreType.UInt64])
				{
					return TypeCode.UInt64;
				}
				if (type == allFoundCoreTypes[CoreType.Single])
				{
					return TypeCode.Single;
				}
				if (type == allFoundCoreTypes[CoreType.Double])
				{
					return TypeCode.Double;
				}
				if (type == allFoundCoreTypes[CoreType.String])
				{
					return TypeCode.String;
				}
				if (type == allFoundCoreTypes[CoreType.DateTime])
				{
					return TypeCode.DateTime;
				}
				if (type == allFoundCoreTypes[CoreType.Decimal])
				{
					return TypeCode.Decimal;
				}
				if (type == allFoundCoreTypes[CoreType.DBNull])
				{
					return TypeCode.DBNull;
				}
				return TypeCode.Object;
			}

			internal sealed override RoType GetRoElementType()
			{
				return null;
			}

			public sealed override int GetArrayRank()
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Argument_HasToBeArrayClass);
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				return Array.Empty<RoType>();
			}

			protected internal sealed override RoType[] GetGenericArgumentsNoCopy()
			{
				return GetGenericTypeParametersNoCopy();
			}

			public sealed override Type[] GetGenericParameterConstraints()
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
			}

			internal sealed override IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter)
			{
				return SpecializeConstructors(filter, this);
			}

			internal sealed override IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType)
			{
				return SpecializeMethods(filter, reflectedType, this);
			}

			internal sealed override IEnumerable<EventInfo> GetEventsCore(NameFilter filter, Type reflectedType)
			{
				return SpecializeEvents(filter, reflectedType, this);
			}

			internal sealed override IEnumerable<FieldInfo> GetFieldsCore(NameFilter filter, Type reflectedType)
			{
				return SpecializeFields(filter, reflectedType, this);
			}

			internal sealed override IEnumerable<PropertyInfo> GetPropertiesCore(NameFilter filter, Type reflectedType)
			{
				return SpecializeProperties(filter, reflectedType, this);
			}

			internal abstract IEnumerable<ConstructorInfo> SpecializeConstructors(NameFilter filter, RoInstantiationProviderType declaringType);

			internal abstract IEnumerable<MethodInfo> SpecializeMethods(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType);

			internal abstract IEnumerable<EventInfo> SpecializeEvents(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType);

			internal abstract IEnumerable<FieldInfo> SpecializeFields(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType);

			internal abstract IEnumerable<PropertyInfo> SpecializeProperties(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType);

			internal abstract bool IsTypeNameEqual(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name);

			internal abstract RoDefinitionType GetNestedTypeCore(ReadOnlySpan<byte> utf8Name);
		}

		/// <summary>
		/// Base class for all EventInfo objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoEvent : LeveledEventInfo
		{
			private readonly RoInstantiationProviderType _declaringType;

			private readonly Type _reflectedType;

			private volatile string _lazyName;

			private const EventAttributes EventAttributesSentinel = (EventAttributes)(-1);

			private volatile EventAttributes _lazyEventAttributes = (EventAttributes)(-1);

			private volatile Type _lazyEventType;

			private volatile RoMethod _lazyAdder = Sentinels.RoMethod;

			private volatile RoMethod _lazyRemover = Sentinels.RoMethod;

			private volatile RoMethod _lazyRaiser = Sentinels.RoMethod;

			public sealed override Type DeclaringType => GetRoDeclaringType();

			public sealed override Type ReflectedType => _reflectedType;

			public sealed override string Name => _lazyName ?? (_lazyName = ComputeName());

			public sealed override Module Module => GetRoModule();

			public abstract override int MetadataToken { get; }

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			public sealed override EventAttributes Attributes
			{
				get
				{
					if (_lazyEventAttributes != (EventAttributes)(-1))
					{
						return _lazyEventAttributes;
					}
					return _lazyEventAttributes = ComputeAttributes();
				}
			}

			public sealed override Type EventHandlerType => _lazyEventType ?? (_lazyEventType = ComputeEventHandlerType());

			public sealed override bool IsMulticast => Loader.GetCoreType(CoreType.MulticastDelegate).IsAssignableFrom(EventHandlerType);

			private MetadataLoadContext Loader => GetRoModule().Loader;

			internal TypeContext TypeContext => _declaringType.Instantiation.ToTypeContext();

			protected RoEvent(RoInstantiationProviderType declaringType, Type reflectedType)
			{
				_declaringType = declaringType;
				_reflectedType = reflectedType;
			}

			public abstract override bool Equals(object obj);

			public abstract override int GetHashCode();

			public abstract override string ToString();

			internal RoInstantiationProviderType GetRoDeclaringType()
			{
				return _declaringType;
			}

			protected abstract string ComputeName();

			internal abstract RoModule GetRoModule();

			public sealed override bool HasSameMetadataDefinitionAs(MemberInfo other)
			{
				return this.HasSameMetadataDefinitionAsCore(other);
			}

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			protected abstract EventAttributes ComputeAttributes();

			protected abstract Type ComputeEventHandlerType();

			private MethodInfo GetRoAddMethod()
			{
				if (!(_lazyAdder == Sentinels.RoMethod))
				{
					return _lazyAdder;
				}
				return _lazyAdder = ComputeEventAddMethod()?.FilterInheritedAccessor();
			}

			private MethodInfo GetRoRemoveMethod()
			{
				if (!(_lazyRemover == Sentinels.RoMethod))
				{
					return _lazyRemover;
				}
				return _lazyRemover = ComputeEventRemoveMethod()?.FilterInheritedAccessor();
			}

			private MethodInfo GetRoRaiseMethod()
			{
				if (!(_lazyRaiser == Sentinels.RoMethod))
				{
					return _lazyRaiser;
				}
				return _lazyRaiser = ComputeEventRaiseMethod()?.FilterInheritedAccessor();
			}

			public sealed override MethodInfo GetAddMethod(bool nonPublic)
			{
				return GetRoAddMethod()?.FilterAccessor(nonPublic);
			}

			public sealed override MethodInfo GetRemoveMethod(bool nonPublic)
			{
				return GetRoRemoveMethod()?.FilterAccessor(nonPublic);
			}

			public sealed override MethodInfo GetRaiseMethod(bool nonPublic)
			{
				return GetRoRaiseMethod()?.FilterAccessor(nonPublic);
			}

			protected abstract RoMethod ComputeEventAddMethod();

			protected abstract RoMethod ComputeEventRemoveMethod();

			protected abstract RoMethod ComputeEventRaiseMethod();

			public abstract override MethodInfo[] GetOtherMethods(bool nonPublic);

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override void AddEventHandler(object target, Delegate handler)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override void RemoveEventHandler(object target, Delegate handler)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}
		}

		/// <summary>
		/// This "assembly" holds an exception resulting from a failure to bind an assembly name. It can be stored in bind caches and assembly ref
		/// memoization tables.
		/// </summary>
		internal sealed class RoExceptionAssembly : RoStubAssembly
		{
			internal Exception Exception { get; }

			internal RoExceptionAssembly(Exception exception)
			{
				Exception = exception;
			}
		}

		internal sealed class RoExceptionHandlingClause : ExceptionHandlingClause
		{
			private readonly Type _catchType;

			private readonly ExceptionHandlingClauseOptions _flags;

			private readonly int _filterOffset;

			private readonly int _tryOffset;

			private readonly int _tryLength;

			private readonly int _handlerOffset;

			private readonly int _handlerLength;

			public sealed override Type CatchType
			{
				get
				{
					if (_flags != 0)
					{
						throw new InvalidOperationException(MDCFR.Properties.Resources.NotAClause);
					}
					return _catchType;
				}
			}

			public sealed override ExceptionHandlingClauseOptions Flags => _flags;

			public sealed override int FilterOffset
			{
				get
				{
					if (_flags != ExceptionHandlingClauseOptions.Filter)
					{
						throw new InvalidOperationException(MDCFR.Properties.Resources.NotAFilter);
					}
					return _filterOffset;
				}
			}

			public sealed override int HandlerOffset => _handlerOffset;

			public sealed override int HandlerLength => _handlerLength;

			public sealed override int TryOffset => _tryOffset;

			public sealed override int TryLength => _tryLength;

			internal RoExceptionHandlingClause(Type catchType, ExceptionHandlingClauseOptions flags, int filterOffset, int tryOffset, int tryLength, int handlerOffset, int handlerLength)
			{
				_catchType = catchType;
				_flags = flags;
				_filterOffset = filterOffset;
				_tryOffset = tryOffset;
				_tryLength = tryLength;
				_handlerOffset = handlerOffset;
				_handlerLength = handlerLength;
			}
		}

		/// <summary>
		/// This class exists only to stash Exceptions inside GetTypeCore caches.
		/// </summary>
		internal sealed class RoExceptionType : RoDefinitionType
		{
			private readonly byte[] _ns;

			private readonly byte[] _name;

			internal Exception Exception { get; }

			public sealed override bool IsGenericTypeDefinition
			{
				get
				{
					throw null;
				}
			}

			public sealed override int MetadataToken
			{
				get
				{
					throw null;
				}
			}

			internal RoExceptionType(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, Exception exception)
			{
				_ns = ns.ToArray();
				_name = name.ToArray();
				Exception = exception;
			}

			internal sealed override bool IsTypeNameEqual(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				if (name.SequenceEqual(_name))
				{
					return ns.SequenceEqual(_ns);
				}
				return false;
			}

			internal sealed override RoModule GetRoModule()
			{
				throw null;
			}

			protected sealed override string ComputeName()
			{
				throw null;
			}

			protected sealed override string ComputeNamespace()
			{
				throw null;
			}

			protected sealed override TypeAttributes ComputeAttributeFlags()
			{
				throw null;
			}

			protected sealed override RoType ComputeDeclaringType()
			{
				throw null;
			}

			internal sealed override int GetGenericParameterCount()
			{
				throw null;
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				throw null;
			}

			internal sealed override bool IsCustomAttributeDefined(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				throw null;
			}

			internal sealed override CustomAttributeData TryFindCustomAttribute(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				throw null;
			}

			protected sealed override IEnumerable<CustomAttributeData> GetTrueCustomAttributes()
			{
				throw null;
			}

			protected sealed override void GetPackSizeAndSize(out int packSize, out int size)
			{
				throw null;
			}

			protected internal sealed override RoType ComputeEnumUnderlyingType()
			{
				throw null;
			}

			internal sealed override RoType SpecializeBaseType(RoType[] instantiation)
			{
				throw null;
			}

			internal sealed override IEnumerable<RoType> SpecializeInterfaces(RoType[] instantiation)
			{
				throw null;
			}

			internal sealed override IEnumerable<RoType> GetNestedTypesCore(NameFilter filter)
			{
				throw null;
			}

			internal sealed override RoDefinitionType GetNestedTypeCore(ReadOnlySpan<byte> utf8Name)
			{
				throw null;
			}

			internal sealed override IEnumerable<ConstructorInfo> SpecializeConstructors(NameFilter filter, RoInstantiationProviderType declaringType)
			{
				throw null;
			}

			internal sealed override IEnumerable<MethodInfo> SpecializeMethods(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
			{
				throw null;
			}

			internal sealed override IEnumerable<EventInfo> SpecializeEvents(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
			{
				throw null;
			}

			internal sealed override IEnumerable<FieldInfo> SpecializeFields(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
			{
				throw null;
			}

			internal sealed override IEnumerable<PropertyInfo> SpecializeProperties(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
			{
				throw null;
			}
		}

		/// <summary>
		/// Base class for all RoParameter's returned by MethodBase.GetParameters() that have an entry in the Param table.
		/// </summary>
		internal abstract class RoFatMethodParameter : RoMethodParameter
		{
			private volatile string _lazyName;

			private const ParameterAttributes ParameterAttributesSentinel = (ParameterAttributes)(-1);

			private volatile ParameterAttributes _lazyParameterAttributes = (ParameterAttributes)(-1);

			public sealed override string Name => _lazyName ?? (_lazyName = ComputeName());

			public sealed override ParameterAttributes Attributes
			{
				get
				{
					if (_lazyParameterAttributes != (ParameterAttributes)(-1))
					{
						return _lazyParameterAttributes;
					}
					return _lazyParameterAttributes = ComputeAttributes();
				}
			}

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes
			{
				get
				{
					foreach (CustomAttributeData trueCustomAttribute in GetTrueCustomAttributes())
					{
						yield return trueCustomAttribute;
					}
					ParameterAttributes attributes = Attributes;
					if ((attributes & ParameterAttributes.In) != 0)
					{
						ConstructorInfo constructorInfo = Loader.TryGetInCtor();
						if (constructorInfo != null)
						{
							yield return new RoPseudoCustomAttributeData(constructorInfo);
						}
					}
					if ((attributes & ParameterAttributes.Out) != 0)
					{
						ConstructorInfo constructorInfo2 = Loader.TryGetOutCtor();
						if (constructorInfo2 != null)
						{
							yield return new RoPseudoCustomAttributeData(constructorInfo2);
						}
					}
					if ((attributes & ParameterAttributes.Optional) != 0)
					{
						ConstructorInfo constructorInfo3 = Loader.TryGetOptionalCtor();
						if (constructorInfo3 != null)
						{
							yield return new RoPseudoCustomAttributeData(constructorInfo3);
						}
					}
					if ((attributes & ParameterAttributes.HasFieldMarshal) != 0)
					{
						CustomAttributeData customAttributeData = CustomAttributeHelpers.TryComputeMarshalAsCustomAttributeData(ComputeMarshalAsAttribute, Loader);
						if (customAttributeData != null)
						{
							yield return customAttributeData;
						}
					}
				}
			}

			public abstract override bool HasDefaultValue { get; }

			public abstract override object RawDefaultValue { get; }

			private MetadataLoadContext Loader => GetRoMethodBase().Loader;

			protected RoFatMethodParameter(IRoMethodBase roMethodBase, int position, Type parameterType)
				: base(roMethodBase, position, parameterType)
			{
			}

			protected abstract string ComputeName();

			protected abstract ParameterAttributes ComputeAttributes();

			protected abstract MarshalAsAttribute ComputeMarshalAsAttribute();

			protected abstract IEnumerable<CustomAttributeData> GetTrueCustomAttributes();
		}

		/// <summary>
		/// Base class for all FieldInfo objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoField : LeveledFieldInfo
		{
			private readonly RoInstantiationProviderType _declaringType;

			private readonly Type _reflectedType;

			private volatile string _lazyName;

			private const FieldAttributes FieldAttributesSentinel = (FieldAttributes)(-1);

			private volatile FieldAttributes _lazyFieldAttributes = (FieldAttributes)(-1);

			private volatile Type _lazyFieldType;

			public sealed override Type DeclaringType => GetRoDeclaringType();

			public sealed override Type ReflectedType => _reflectedType;

			public sealed override string Name => _lazyName ?? (_lazyName = ComputeName());

			public sealed override Module Module => GetRoModule();

			public abstract override int MetadataToken { get; }

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes
			{
				get
				{
					foreach (CustomAttributeData trueCustomAttribute in GetTrueCustomAttributes())
					{
						yield return trueCustomAttribute;
					}
					if (_declaringType.IsExplicitLayout)
					{
						ConstructorInfo constructorInfo = Loader.TryGetFieldOffsetCtor();
						if (constructorInfo != null)
						{
							int explicitFieldOffset = GetExplicitFieldOffset();
							Type coreType = Loader.GetCoreType(CoreType.Int32);
							CustomAttributeTypedArgument[] fixedArguments = new CustomAttributeTypedArgument[1]
							{
								new CustomAttributeTypedArgument(coreType, explicitFieldOffset)
							};
							yield return new RoPseudoCustomAttributeData(constructorInfo, fixedArguments);
						}
					}
					if ((Attributes & FieldAttributes.HasFieldMarshal) != 0)
					{
						CustomAttributeData customAttributeData = CustomAttributeHelpers.TryComputeMarshalAsCustomAttributeData(ComputeMarshalAsAttribute, Loader);
						if (customAttributeData != null)
						{
							yield return customAttributeData;
						}
					}
				}
			}

			public sealed override FieldAttributes Attributes
			{
				get
				{
					if (_lazyFieldAttributes != (FieldAttributes)(-1))
					{
						return _lazyFieldAttributes;
					}
					return _lazyFieldAttributes = ComputeAttributes();
				}
			}

			public sealed override Type FieldType => _lazyFieldType ?? (_lazyFieldType = ComputeFieldType());

			public sealed override bool IsSecurityCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecuritySafeCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecurityTransparent
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override RuntimeFieldHandle FieldHandle
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
				}
			}

			private MetadataLoadContext Loader => GetRoModule().Loader;

			internal TypeContext TypeContext => _declaringType.Instantiation.ToTypeContext();

			protected RoField(RoInstantiationProviderType declaringType, Type reflectedType)
			{
				_declaringType = declaringType;
				_reflectedType = reflectedType;
			}

			public abstract override bool Equals(object obj);

			public abstract override int GetHashCode();

			public abstract override string ToString();

			internal RoInstantiationProviderType GetRoDeclaringType()
			{
				return _declaringType;
			}

			protected abstract string ComputeName();

			internal abstract RoModule GetRoModule();

			public sealed override bool HasSameMetadataDefinitionAs(MemberInfo other)
			{
				return this.HasSameMetadataDefinitionAsCore(other);
			}

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			protected abstract IEnumerable<CustomAttributeData> GetTrueCustomAttributes();

			protected abstract int GetExplicitFieldOffset();

			protected abstract MarshalAsAttribute ComputeMarshalAsAttribute();

			protected abstract FieldAttributes ComputeAttributes();

			protected abstract Type ComputeFieldType();

			public sealed override object GetRawConstantValue()
			{
				if (!base.IsLiteral)
				{
					throw new InvalidOperationException();
				}
				return ComputeRawConstantValue();
			}

			protected abstract object ComputeRawConstantValue();

			public abstract override Type[] GetOptionalCustomModifiers();

			public abstract override Type[] GetRequiredCustomModifiers();

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object GetValue(object obj)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object GetValueDirect(TypedReference obj)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override void SetValueDirect(TypedReference obj, object value)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}
		}

		/// <summary>
		/// Base type for all RoTypes that return true for IsGenericParameter. This can a generic parameter defined on a type or a method.
		/// </summary>
		internal abstract class RoGenericParameterType : RoType
		{
			private volatile int _lazyPosition = -1;

			private volatile RoType[] _lazyConstraints;

			public sealed override bool IsTypeDefinition => false;

			public sealed override bool IsGenericTypeDefinition => false;

			public sealed override bool IsSZArray => false;

			public sealed override bool IsVariableBoundArray => false;

			public sealed override bool IsConstructedGenericType => false;

			public sealed override bool IsGenericParameter => true;

			public sealed override bool ContainsGenericParameters => true;

			public sealed override int GenericParameterPosition
			{
				get
				{
					if (_lazyPosition != -1)
					{
						return _lazyPosition;
					}
					return _lazyPosition = ComputePosition();
				}
			}

			public sealed override Guid GUID => Guid.Empty;

			public sealed override StructLayoutAttribute StructLayoutAttribute => null;

			protected sealed override bool HasElementTypeImpl()
			{
				return false;
			}

			protected sealed override bool IsArrayImpl()
			{
				return false;
			}

			protected sealed override bool IsByRefImpl()
			{
				return false;
			}

			protected sealed override bool IsPointerImpl()
			{
				return false;
			}

			protected sealed override string ComputeNamespace()
			{
				return DeclaringType.Namespace;
			}

			protected sealed override string ComputeFullName()
			{
				return null;
			}

			public sealed override string ToString()
			{
				return base.Loader.GetDisposedString() ?? Name;
			}

			protected sealed override TypeAttributes ComputeAttributeFlags()
			{
				return TypeAttributes.Public;
			}

			protected sealed override TypeCode GetTypeCodeImpl()
			{
				return TypeCode.Object;
			}

			internal sealed override RoType GetRoElementType()
			{
				return null;
			}

			public sealed override int GetArrayRank()
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Argument_HasToBeArrayClass);
			}

			public sealed override Type GetGenericTypeDefinition()
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_NotGenericType);
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				return Array.Empty<RoType>();
			}

			protected internal sealed override RoType[] GetGenericArgumentsNoCopy()
			{
				return Array.Empty<RoType>();
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override Type MakeGenericType(params Type[] typeArguments)
			{
				throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.Arg_NotGenericTypeDefinition, this));
			}

			protected abstract int ComputePosition();

			public sealed override Type[] GetGenericParameterConstraints()
			{
				Type[] genericParameterConstraintsNoCopy = GetGenericParameterConstraintsNoCopy();
				return genericParameterConstraintsNoCopy.CloneArray();
			}

			private RoType[] GetGenericParameterConstraintsNoCopy()
			{
				return _lazyConstraints ?? (_lazyConstraints = ComputeGenericParameterConstraints());
			}

			protected abstract RoType[] ComputeGenericParameterConstraints();

			protected internal sealed override RoType ComputeEnumUnderlyingType()
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeEnum);
			}

			protected sealed override RoType ComputeBaseTypeWithoutDesktopQuirk()
			{
				RoType[] genericParameterConstraintsNoCopy = GetGenericParameterConstraintsNoCopy();
				RoType[] array = genericParameterConstraintsNoCopy;
				foreach (RoType roType in array)
				{
					if (!roType.IsInterface)
					{
						return roType;
					}
				}
				return base.Loader.GetCoreType(CoreType.Object);
			}

			protected sealed override IEnumerable<RoType> ComputeDirectlyImplementedInterfaces()
			{
				RoType[] genericParameterConstraintsNoCopy = GetGenericParameterConstraintsNoCopy();
				RoType[] array = genericParameterConstraintsNoCopy;
				foreach (RoType roType in array)
				{
					if (roType.IsInterface)
					{
						yield return roType;
					}
				}
			}

			internal sealed override IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter)
			{
				return Array.Empty<ConstructorInfo>();
			}

			internal sealed override IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<MethodInfo>();
			}

			internal sealed override IEnumerable<EventInfo> GetEventsCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<EventInfo>();
			}

			internal sealed override IEnumerable<FieldInfo> GetFieldsCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<FieldInfo>();
			}

			internal sealed override IEnumerable<PropertyInfo> GetPropertiesCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<PropertyInfo>();
			}

			internal sealed override IEnumerable<RoType> GetNestedTypesCore(NameFilter filter)
			{
				return Array.Empty<RoType>();
			}
		}

		/// <summary>
		/// Base type for all RoTypes that return true for HasElementType.
		/// </summary>
		internal abstract class RoHasElementType : RoType
		{
			private readonly RoType _elementType;

			public sealed override bool IsTypeDefinition => false;

			public sealed override bool IsGenericTypeDefinition => false;

			public sealed override bool IsConstructedGenericType => false;

			public sealed override bool IsGenericParameter => false;

			public sealed override bool IsGenericTypeParameter => false;

			public sealed override bool IsGenericMethodParameter => false;

			public sealed override bool ContainsGenericParameters => _elementType.ContainsGenericParameters;

			public sealed override MethodBase DeclaringMethod
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => Array.Empty<CustomAttributeData>();

			public sealed override int MetadataToken => 33554432;

			public sealed override GenericParameterAttributes GenericParameterAttributes
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			public sealed override int GenericParameterPosition
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
				}
			}

			public sealed override Guid GUID => Guid.Empty;

			public sealed override StructLayoutAttribute StructLayoutAttribute => null;

			protected abstract string Suffix { get; }

			protected RoHasElementType(RoType elementType)
			{
				_elementType = elementType;
			}

			protected sealed override bool HasElementTypeImpl()
			{
				return true;
			}

			internal sealed override RoModule GetRoModule()
			{
				return _elementType.GetRoModule();
			}

			protected sealed override string ComputeName()
			{
				return _elementType.Name + Suffix;
			}

			protected sealed override string ComputeNamespace()
			{
				return _elementType.Namespace;
			}

			protected sealed override string ComputeFullName()
			{
				string fullName = _elementType.FullName;
				if (fullName != null)
				{
					return fullName + Suffix;
				}
				return null;
			}

			protected sealed override TypeCode GetTypeCodeImpl()
			{
				return TypeCode.Object;
			}

			public sealed override string ToString()
			{
				return _elementType.ToString() + Suffix;
			}

			protected sealed override RoType ComputeDeclaringType()
			{
				return null;
			}

			internal sealed override bool IsCustomAttributeDefined(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				return false;
			}

			internal sealed override CustomAttributeData TryFindCustomAttribute(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				return null;
			}

			internal sealed override RoType GetRoElementType()
			{
				return _elementType;
			}

			public sealed override Type GetGenericTypeDefinition()
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_NotGenericType);
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				return Array.Empty<RoType>();
			}

			protected internal sealed override RoType[] GetGenericArgumentsNoCopy()
			{
				return _elementType.GetGenericArgumentsNoCopy();
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override Type MakeGenericType(params Type[] typeArguments)
			{
				throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.Arg_NotGenericTypeDefinition, this));
			}

			public sealed override Type[] GetGenericParameterConstraints()
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_NotGenericParameter);
			}

			protected internal sealed override RoType ComputeEnumUnderlyingType()
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeEnum);
			}

			internal sealed override IEnumerable<EventInfo> GetEventsCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<EventInfo>();
			}

			internal sealed override IEnumerable<FieldInfo> GetFieldsCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<FieldInfo>();
			}

			internal sealed override IEnumerable<PropertyInfo> GetPropertiesCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<PropertyInfo>();
			}

			internal sealed override IEnumerable<RoType> GetNestedTypesCore(NameFilter filter)
			{
				return Array.Empty<RoType>();
			}
		}

		/// <summary>
		/// Base type for RoDefinitionType and RoConstructedGenericType. These are the two types that can declare members backed by metadata.
		/// (Though Array types "declare" members too, those are not backed by actual metadata so there will never be a typespec that has to be resolved
		/// which is what an instantiation is for in the first place.)
		/// </summary>
		internal abstract class RoInstantiationProviderType : RoType
		{
			internal abstract RoType[] Instantiation { get; }
		}

		internal sealed class RoLocalVariableInfo : LocalVariableInfo
		{
			private readonly int _localIndex;

			private readonly bool _isPinned;

			private readonly Type _localType;

			public sealed override int LocalIndex => _localIndex;

			public sealed override bool IsPinned => _isPinned;

			public sealed override Type LocalType => _localType;

			internal RoLocalVariableInfo(int localIndex, bool isPinned, Type localType)
			{
				_localIndex = localIndex;
				_isPinned = isPinned;
				_localType = localType;
			}
		}

		/// <summary>
		/// Base class for all MethodInfo objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoMethod : LeveledMethodInfo, IRoMethodBase
		{
			private readonly Type _reflectedType;

			private volatile string _lazyName;

			private const MethodAttributes MethodAttributesSentinel = (MethodAttributes)(-1);

			private volatile MethodAttributes _lazyMethodAttributes = (MethodAttributes)(-1);

			private const CallingConventions CallingConventionsSentinel = (CallingConventions)(-1);

			private volatile CallingConventions _lazyCallingConventions = (CallingConventions)(-1);

			private const MethodImplAttributes MethodImplAttributesSentinel = (MethodImplAttributes)(-1);

			private volatile MethodImplAttributes _lazyMethodImplAttributes = (MethodImplAttributes)(-1);

			private volatile MethodSig<RoParameter> _lazyMethodSig;

			private volatile MethodSig<RoType> _lazyCustomModifiers;

			private volatile RoType[] _lazyGenericArgumentsOrParameters;

			public sealed override Type DeclaringType => GetRoDeclaringType();

			public sealed override Type ReflectedType => _reflectedType;

			public sealed override string Name => _lazyName ?? (_lazyName = ComputeName());

			public sealed override Module Module => GetRoModule();

			public abstract override int MetadataToken { get; }

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			public abstract override bool IsConstructedGenericMethod { get; }

			public abstract override bool IsGenericMethodDefinition { get; }

			public sealed override bool IsGenericMethod
			{
				get
				{
					if (!IsGenericMethodDefinition)
					{
						return IsConstructedGenericMethod;
					}
					return true;
				}
			}

			public sealed override MethodAttributes Attributes
			{
				get
				{
					if (_lazyMethodAttributes != (MethodAttributes)(-1))
					{
						return _lazyMethodAttributes;
					}
					return _lazyMethodAttributes = ComputeAttributes();
				}
			}

			public sealed override CallingConventions CallingConvention
			{
				get
				{
					if (_lazyCallingConventions != (CallingConventions)(-1))
					{
						return _lazyCallingConventions;
					}
					return _lazyCallingConventions = ComputeCallingConvention();
				}
			}

			public sealed override MethodImplAttributes MethodImplementationFlags
			{
				get
				{
					if (_lazyMethodImplAttributes != (MethodImplAttributes)(-1))
					{
						return _lazyMethodImplAttributes;
					}
					return _lazyMethodImplAttributes = ComputeMethodImplementationFlags();
				}
			}

			public sealed override bool ContainsGenericParameters
			{
				get
				{
					if (GetRoDeclaringType().ContainsGenericParameters)
					{
						return true;
					}
					Type[] genericArgumentsOrParametersNoCopy = GetGenericArgumentsOrParametersNoCopy();
					Type[] array = genericArgumentsOrParametersNoCopy;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].ContainsGenericParameters)
						{
							return true;
						}
					}
					return false;
				}
			}

			public sealed override ParameterInfo ReturnParameter => MethodSig.Return;

			private MethodSig<RoParameter> MethodSig => _lazyMethodSig ?? (_lazyMethodSig = ComputeMethodSig());

			private MethodSig<RoType> CustomModifiers => _lazyCustomModifiers ?? (_lazyCustomModifiers = ComputeCustomModifiers());

			public sealed override ICustomAttributeProvider ReturnTypeCustomAttributes => ReturnParameter;

			public sealed override Type ReturnType => ReturnParameter.ParameterType;

			public sealed override bool IsSecurityCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecuritySafeCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecurityTransparent
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override RuntimeMethodHandle MethodHandle
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
				}
			}

			MethodBase IRoMethodBase.MethodBase => this;

			public MetadataLoadContext Loader => GetRoModule().Loader;

			public abstract TypeContext TypeContext { get; }

			protected RoMethod(Type reflectedType)
			{
				_reflectedType = reflectedType;
			}

			public abstract override bool Equals(object obj);

			public abstract override int GetHashCode();

			internal abstract RoType GetRoDeclaringType();

			protected abstract string ComputeName();

			internal abstract RoModule GetRoModule();

			public sealed override bool HasSameMetadataDefinitionAs(MemberInfo other)
			{
				return this.HasSameMetadataDefinitionAsCore(other);
			}

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			protected abstract MethodAttributes ComputeAttributes();

			protected abstract CallingConventions ComputeCallingConvention();

			protected abstract MethodImplAttributes ComputeMethodImplementationFlags();

			public sealed override MethodImplAttributes GetMethodImplementationFlags()
			{
				return MethodImplementationFlags;
			}

			public abstract override MethodBody GetMethodBody();

			public sealed override ParameterInfo[] GetParameters()
			{
				ParameterInfo[] parametersNoCopy = GetParametersNoCopy();
				return parametersNoCopy.CloneArray();
			}

			internal RoParameter[] GetParametersNoCopy()
			{
				return MethodSig.Parameters;
			}

			protected abstract MethodSig<RoParameter> ComputeMethodSig();

			protected abstract MethodSig<RoType> ComputeCustomModifiers();

			public abstract override MethodInfo GetGenericMethodDefinition();

			public sealed override Type[] GetGenericArguments()
			{
				Type[] genericArgumentsOrParametersNoCopy = GetGenericArgumentsOrParametersNoCopy();
				return genericArgumentsOrParametersNoCopy.CloneArray();
			}

			internal RoType[] GetGenericArgumentsOrParametersNoCopy()
			{
				return _lazyGenericArgumentsOrParameters ?? (_lazyGenericArgumentsOrParameters = ComputeGenericArgumentsOrParameters());
			}

			protected abstract RoType[] ComputeGenericArgumentsOrParameters();

			internal abstract RoType[] GetGenericTypeParametersNoCopy();

			internal abstract RoType[] GetGenericTypeArgumentsNoCopy();

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public abstract override MethodInfo MakeGenericMethod(params Type[] typeArguments);

			public sealed override string ToString()
			{
				return Loader.GetDisposedString() ?? this.ToString(ComputeMethodSigStrings());
			}

			protected abstract MethodSig<string> ComputeMethodSigStrings();

			public sealed override MethodInfo GetBaseDefinition()
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_GetBaseDefinition);
			}

			public sealed override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyInvoke);
			}

			public sealed override Delegate CreateDelegate(Type delegateType)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override Delegate CreateDelegate(Type delegateType, object target)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			Type[] IRoMethodBase.GetCustomModifiers(int position, bool isRequired)
			{
				return CustomModifiers[position].ExtractCustomModifiers(isRequired);
			}

			string IRoMethodBase.GetMethodSigString(int position)
			{
				return ComputeMethodSigStrings()[position];
			}
		}

		internal abstract class RoMethodBody : MethodBody
		{
			private volatile byte[] _lazyIL;

			public abstract override bool InitLocals { get; }

			public abstract override int MaxStackSize { get; }

			public abstract override int LocalSignatureMetadataToken { get; }

			public abstract override IList<LocalVariableInfo> LocalVariables { get; }

			public abstract override IList<ExceptionHandlingClause> ExceptionHandlingClauses { get; }

			public sealed override byte[] GetILAsByteArray()
			{
				return _lazyIL ?? (_lazyIL = ComputeIL());
			}

			protected abstract byte[] ComputeIL();
		}

		/// <summary>
		/// Base class for all RoParameter's returned by MethodBase.GetParameters().
		/// </summary>
		internal abstract class RoMethodParameter : RoParameter
		{
			private readonly Type _parameterType;

			public sealed override Type ParameterType => _parameterType;

			private MetadataLoadContext Loader => GetRoMethodBase().Loader;

			protected RoMethodParameter(IRoMethodBase roMethodBase, int position, Type parameterType)
				: base(roMethodBase.MethodBase, position)
			{
				_parameterType = parameterType;
			}

			public sealed override Type[] GetOptionalCustomModifiers()
			{
				return GetRoMethodBase().GetCustomModifiers(Position, isRequired: false).CloneArray();
			}

			public sealed override Type[] GetRequiredCustomModifiers()
			{
				return GetRoMethodBase().GetCustomModifiers(Position, isRequired: true).CloneArray();
			}

			public sealed override string ToString()
			{
				return Loader.GetDisposedString() ?? (GetRoMethodBase().GetMethodSigString(Position) + " " + Name);
			}

			internal IRoMethodBase GetRoMethodBase()
			{
				return (IRoMethodBase)Member;
			}
		}

		/// <summary>
		/// This is used to represent a ModifiedType. It is quite ill-behaved so the only time it is created is by the EcmaModifiedTypeProvider.
		/// It is only used to implement the GetCustomModifiers apis.
		/// </summary>
		internal sealed class RoModifiedType : RoWrappedType
		{
			internal RoType Modifier { get; }

			internal bool IsRequired { get; }

			internal RoModifiedType(RoType modifier, RoType unmodifiedType, bool isRequired)
				: base(unmodifiedType)
			{
				Modifier = modifier;
				IsRequired = isRequired;
			}
		}

		/// <summary>
		/// Base class for all Module objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoModule : Module
		{
			private readonly string _fullyQualifiedName;

			internal const string FullyQualifiedNameForModulesLoadedFromByteArrays = "<Unknown>";

			internal const string UnknownStringMessageInRAF = "Returns <Unknown> for modules with no file path";

			internal readonly GetTypeCoreCache _getTypeCoreCache = new GetTypeCoreCache();

			private static readonly Func<RoType, RoArrayType> s_szArrayTypeFactory = (RoType e) => new RoArrayType(e, multiDim: false, 1);

			private readonly ConcurrentDictionary<RoType, RoArrayType> _szArrayDict = new ConcurrentDictionary<RoType, RoArrayType>();

			private static readonly Func<RoArrayType.Key, RoArrayType> s_mdArrayTypeFactory = (RoArrayType.Key k) => new RoArrayType(k.ElementType, multiDim: true, k.Rank);

			private readonly ConcurrentDictionary<RoArrayType.Key, RoArrayType> _mdArrayDict = new ConcurrentDictionary<RoArrayType.Key, RoArrayType>();

			private static readonly Func<RoType, RoByRefType> s_byrefTypeFactory = (RoType e) => new RoByRefType(e);

			private readonly ConcurrentDictionary<RoType, RoByRefType> _byRefDict = new ConcurrentDictionary<RoType, RoByRefType>();

			private readonly ConcurrentDictionary<RoType, RoPointerType> _pointerDict = new ConcurrentDictionary<RoType, RoPointerType>();

			private static readonly Func<RoConstructedGenericType.Key, RoConstructedGenericType> s_constructedGenericTypeFactory = (RoConstructedGenericType.Key k) => new RoConstructedGenericType(k.GenericTypeDefinition, k.GenericTypeArguments);

			private readonly ConcurrentDictionary<RoConstructedGenericType.Key, RoConstructedGenericType> _constructedGenericTypeDict = new ConcurrentDictionary<RoConstructedGenericType.Key, RoConstructedGenericType>();

			public sealed override Assembly Assembly => GetRoAssembly();

			public sealed override string FullyQualifiedName => _fullyQualifiedName;

			public abstract override int MDStreamVersion { get; }

			public abstract override int MetadataToken { get; }

			public abstract override Guid ModuleVersionId { get; }

			public sealed override string Name
			{
				get
				{
					string fullyQualifiedName = FullyQualifiedName;
					int num = fullyQualifiedName.LastIndexOf(Path.DirectorySeparatorChar);
					if (num == -1)
					{
						return fullyQualifiedName;
					}
					return fullyQualifiedName.Substring(num + 1);
				}
			}

			public abstract override string ScopeName { get; }

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			internal MetadataLoadContext Loader => GetRoAssembly().Loader;

			internal RoModule(string fullyQualifiedName)
			{
				_fullyQualifiedName = fullyQualifiedName;
			}

			public sealed override string ToString()
			{
				return Loader.GetDisposedString() ?? base.ToString();
			}

			internal abstract RoAssembly GetRoAssembly();

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public abstract override FieldInfo GetField(string name, BindingFlags bindingAttr);

			public abstract override FieldInfo[] GetFields(BindingFlags bindingFlags);

			public abstract override MethodInfo[] GetMethods(BindingFlags bindingFlags);

			protected abstract override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);

			public sealed override void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				throw new NotSupportedException();
			}

			public abstract override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine);

			public abstract override Type[] GetTypes();

			internal abstract IEnumerable<RoType> GetDefinedRoTypes();

			public abstract override bool IsResource();

			public sealed override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_ResolvingTokens);
			}

			public sealed override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_ResolvingTokens);
			}

			public sealed override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_ResolvingTokens);
			}

			public sealed override byte[] ResolveSignature(int metadataToken)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_ResolvingTokens);
			}

			public sealed override string ResolveString(int metadataToken)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_ResolvingTokens);
			}

			public sealed override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_ResolvingTokens);
			}

			public sealed override Type GetType(string className, bool throwOnError, bool ignoreCase)
			{
				Type type = Assembly.GetType(className, throwOnError, ignoreCase);
				if (type.Module != this)
				{
					return null;
				}
				return type;
			}

			/// <summary>
			/// Helper routine for the more general Module.GetType() family of apis. Also used in typeRef resolution.
			///
			/// Resolves top-level named types only. No nested types. No constructed types. The input name must not be escaped.
			///
			/// If a type is not contained or forwarded from the module, this method returns null (does not throw.)
			/// This supports the "throwOnError: false" behavior of Module.GetType(string, bool).
			/// </summary>
			internal RoDefinitionType GetTypeCore(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, bool ignoreCase, out Exception e)
			{
				if (ignoreCase)
				{
					throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_CaseInsensitive);
				}
				int hashCode = GetTypeCoreCache.ComputeHashCode(name);
				if (!_getTypeCoreCache.TryGet(ns, name, hashCode, out var type))
				{
					type = GetTypeCoreNoCache(ns, name, out e) ?? new RoExceptionType(ns, name, e);
					_getTypeCoreCache.GetOrAdd(ns, name, hashCode, type);
				}
				if (type is RoExceptionType roExceptionType)
				{
					e = roExceptionType.Exception;
					return null;
				}
				e = null;
				return type;
			}

			protected abstract RoDefinitionType GetTypeCoreNoCache(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, out Exception e);

			internal RoArrayType GetUniqueArrayType(RoType elementType)
			{
				return _szArrayDict.GetOrAdd(elementType, s_szArrayTypeFactory);
			}

			internal RoArrayType GetUniqueArrayType(RoType elementType, int rank)
			{
				return _mdArrayDict.GetOrAdd(new RoArrayType.Key(elementType, rank), s_mdArrayTypeFactory);
			}

			internal RoByRefType GetUniqueByRefType(RoType elementType)
			{
				return _byRefDict.GetOrAdd(elementType, s_byrefTypeFactory);
			}

			internal RoPointerType GetUniquePointerType(RoType elementType)
			{
				return _pointerDict.GetOrAdd(elementType, (RoType e) => new RoPointerType(e));
			}

			internal RoConstructedGenericType GetUniqueConstructedGenericType(RoDefinitionType genericTypeDefinition, RoType[] genericTypeArguments)
			{
				return _constructedGenericTypeDict.GetOrAdd(new RoConstructedGenericType.Key(genericTypeDefinition, genericTypeArguments), s_constructedGenericTypeFactory);
			}
		}

		/// <summary>
		/// Base class for all ParameterInfo objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoParameter : ParameterInfo
		{
			private readonly MemberInfo _member;

			private readonly int _position;

			public sealed override MemberInfo Member => _member;

			public sealed override int Position => _position;

			public abstract override int MetadataToken { get; }

			public abstract override string Name { get; }

			public abstract override Type ParameterType { get; }

			public abstract override ParameterAttributes Attributes { get; }

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			public abstract override bool HasDefaultValue { get; }

			public sealed override object DefaultValue
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyParameterDefaultValue);
				}
			}

			public abstract override object RawDefaultValue { get; }

			protected RoParameter(MemberInfo member, int position)
			{
				_member = member;
				_position = position;
			}

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			public abstract override Type[] GetOptionalCustomModifiers();

			public abstract override Type[] GetRequiredCustomModifiers();

			public abstract override string ToString();

			public sealed override bool Equals(object obj)
			{
				if (!(obj is RoParameter roParameter))
				{
					return false;
				}
				if (_member != roParameter._member)
				{
					return false;
				}
				if (_position != roParameter._position)
				{
					return false;
				}
				return true;
			}

			public sealed override int GetHashCode()
			{
				return _member.GetHashCode() ^ _position.GetHashCode();
			}

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}
		}

		/// <summary>
		/// This is used to represent a PinnedType. It is quite ill-behaved so the only time it is created is by the EcmaPinnedTypeProvider.
		/// It is only used to implement the MethodBody.LocalVariables property.
		/// </summary>
		internal sealed class RoPinnedType : RoWrappedType { internal RoPinnedType(RoType unmodifiedType) : base(unmodifiedType) { } }

		/// <summary>
		/// All RoTypes that return true for IsPointer.
		/// </summary>
		internal sealed class RoPointerType : RoHasElementType
		{
			public sealed override bool IsSZArray => false;

			public sealed override bool IsVariableBoundArray => false;

			protected sealed override string Suffix => "*";

			internal RoPointerType(RoType elementType)
				: base(elementType)
			{
			}

			protected sealed override bool IsArrayImpl()
			{
				return false;
			}

			protected sealed override bool IsByRefImpl()
			{
				return false;
			}

			protected sealed override bool IsPointerImpl()
			{
				return true;
			}

			public sealed override int GetArrayRank()
			{
				throw new ArgumentException(MDCFR.Properties.Resources.Argument_HasToBeArrayClass);
			}

			protected sealed override TypeAttributes ComputeAttributeFlags()
			{
				return TypeAttributes.Public;
			}

			protected sealed override RoType ComputeBaseTypeWithoutDesktopQuirk()
			{
				return null;
			}

			protected sealed override IEnumerable<RoType> ComputeDirectlyImplementedInterfaces()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter)
			{
				return Array.Empty<ConstructorInfo>();
			}

			internal sealed override IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType)
			{
				return Array.Empty<MethodInfo>();
			}
		}

		/// <summary>
		/// Base class for all PropertyInfo objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoProperty : LeveledPropertyInfo
		{
			private readonly RoInstantiationProviderType _declaringType;

			private readonly Type _reflectedType;

			private volatile string _lazyName;

			private const PropertyAttributes PropertyAttributesSentinel = (PropertyAttributes)(-1);

			private volatile PropertyAttributes _lazyPropertyAttributes = (PropertyAttributes)(-1);

			private volatile Type _lazyPropertyType;

			private volatile RoMethod _lazyGetter = Sentinels.RoMethod;

			private volatile RoMethod _lazySetter = Sentinels.RoMethod;

			private volatile RoPropertyIndexParameter[] _lazyIndexedParameters;

			public sealed override Type DeclaringType => GetRoDeclaringType();

			public sealed override Type ReflectedType => _reflectedType;

			public sealed override string Name => _lazyName ?? (_lazyName = ComputeName());

			public sealed override Module Module => GetRoModule();

			public abstract override int MetadataToken { get; }

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			public sealed override PropertyAttributes Attributes
			{
				get
				{
					if (_lazyPropertyAttributes != (PropertyAttributes)(-1))
					{
						return _lazyPropertyAttributes;
					}
					return _lazyPropertyAttributes = ComputeAttributes();
				}
			}

			public sealed override Type PropertyType => _lazyPropertyType ?? (_lazyPropertyType = ComputePropertyType());

			public sealed override bool CanRead => GetMethod != null;

			public sealed override bool CanWrite => SetMethod != null;

			internal TypeContext TypeContext => _declaringType.Instantiation.ToTypeContext();

			protected RoProperty(RoInstantiationProviderType declaringType, Type reflectedType)
			{
				_declaringType = declaringType;
				_reflectedType = reflectedType;
			}

			public abstract override bool Equals(object obj);

			public abstract override int GetHashCode();

			public abstract override string ToString();

			internal RoInstantiationProviderType GetRoDeclaringType()
			{
				return _declaringType;
			}

			protected abstract string ComputeName();

			internal abstract RoModule GetRoModule();

			public sealed override bool HasSameMetadataDefinitionAs(MemberInfo other)
			{
				return this.HasSameMetadataDefinitionAsCore(other);
			}

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			protected abstract PropertyAttributes ComputeAttributes();

			protected abstract Type ComputePropertyType();

			public sealed override MethodInfo GetGetMethod(bool nonPublic)
			{
				return GetRoGetMethod()?.FilterAccessor(nonPublic);
			}

			public sealed override MethodInfo GetSetMethod(bool nonPublic)
			{
				return GetRoSetMethod()?.FilterAccessor(nonPublic);
			}

			private RoMethod GetRoGetMethod()
			{
				if ((object)_lazyGetter != Sentinels.RoMethod)
				{
					return _lazyGetter;
				}
				return _lazyGetter = ComputeGetterMethod()?.FilterInheritedAccessor();
			}

			private RoMethod GetRoSetMethod()
			{
				if ((object)_lazySetter != Sentinels.RoMethod)
				{
					return _lazySetter;
				}
				return _lazySetter = ComputeSetterMethod()?.FilterInheritedAccessor();
			}

			protected abstract RoMethod ComputeGetterMethod();

			protected abstract RoMethod ComputeSetterMethod();

			public sealed override MethodInfo[] GetAccessors(bool nonPublic)
			{
				MethodInfo getMethod = GetGetMethod(nonPublic);
				MethodInfo setMethod = GetSetMethod(nonPublic);
				int num = 0;
				if (getMethod != null)
				{
					num++;
				}
				if (setMethod != null)
				{
					num++;
				}
				MethodInfo[] array = new MethodInfo[num];
				int num2 = 0;
				if (getMethod != null)
				{
					array[num2++] = getMethod;
				}
				if (setMethod != null)
				{
					array[num2++] = setMethod;
				}
				return array;
			}

			public sealed override ParameterInfo[] GetIndexParameters()
			{
				ParameterInfo[] original = _lazyIndexedParameters ?? (_lazyIndexedParameters = ComputeIndexParameters());
				return original.CloneArray();
			}

			private RoPropertyIndexParameter[] ComputeIndexParameters()
			{
				bool canRead = CanRead;
				RoMethod roMethod = (canRead ? GetRoGetMethod() : GetRoSetMethod());
				if (roMethod == null)
				{
					throw new BadImageFormatException();
				}
				RoParameter[] parametersNoCopy = roMethod.GetParametersNoCopy();
				int num = parametersNoCopy.Length;
				if (!canRead)
				{
					num--;
				}
				if (num == 0)
				{
					return Array.Empty<RoPropertyIndexParameter>();
				}
				RoPropertyIndexParameter[] array = new RoPropertyIndexParameter[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = new RoPropertyIndexParameter(this, parametersNoCopy[i]);
				}
				return array;
			}

			public sealed override object GetRawConstantValue()
			{
				if ((Attributes & PropertyAttributes.HasDefault) == 0)
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_EnumLitValueNotFound);
				}
				return ComputeRawConstantValue();
			}

			protected abstract object ComputeRawConstantValue();

			public abstract override Type[] GetOptionalCustomModifiers();

			public abstract override Type[] GetRequiredCustomModifiers();

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object GetConstantValue()
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}
		}

		/// <summary>
		/// Base class for all RoParameter's returned by PropertyInfo.GetParameters(). These are identical to the associated
		/// getter's ParameterInfo's except for the Member property returning a property.
		/// </summary>
		internal sealed class RoPropertyIndexParameter : RoParameter
		{
			private readonly RoParameter _backingParameter;

			public sealed override int MetadataToken => _backingParameter.MetadataToken;

			public sealed override string Name => _backingParameter.Name;

			public sealed override Type ParameterType => _backingParameter.ParameterType;

			public sealed override ParameterAttributes Attributes => _backingParameter.Attributes;

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => _backingParameter.CustomAttributes;

			public sealed override bool HasDefaultValue => _backingParameter.HasDefaultValue;

			public sealed override object RawDefaultValue => _backingParameter.RawDefaultValue;

			internal RoPropertyIndexParameter(RoProperty member, RoParameter backingParameter)
				: base(member, backingParameter.Position)
			{
				_backingParameter = backingParameter;
			}

			public sealed override Type[] GetOptionalCustomModifiers()
			{
				return _backingParameter.GetOptionalCustomModifiers();
			}

			public sealed override Type[] GetRequiredCustomModifiers()
			{
				return _backingParameter.GetRequiredCustomModifiers();
			}

			public sealed override string ToString()
			{
				return _backingParameter.ToString();
			}
		}

		internal sealed class RoPseudoCustomAttributeData : RoCustomAttributeData
		{
			private readonly ConstructorInfo _constructor;

			private readonly Func<CustomAttributeArguments> _argumentsPromise;

			private volatile IList<CustomAttributeTypedArgument> _lazyFixedArguments;

			private volatile IList<CustomAttributeNamedArgument> _lazyNamedArguments;

			public sealed override IList<CustomAttributeTypedArgument> ConstructorArguments => GetLatchedFixedArguments().CloneForApiReturn();

			public sealed override IList<CustomAttributeNamedArgument> NamedArguments => GetLatchedNamedArguments().CloneForApiReturn();

			internal RoPseudoCustomAttributeData(ConstructorInfo constructor, Func<CustomAttributeArguments> argumentsPromise)
			{
				_constructor = constructor;
				_argumentsPromise = argumentsPromise;
			}

			internal RoPseudoCustomAttributeData(ConstructorInfo constructor, IList<CustomAttributeTypedArgument> fixedArguments = null, IList<CustomAttributeNamedArgument> namedArguments = null)
			{
				_constructor = constructor;
				_lazyFixedArguments = fixedArguments ?? Array.Empty<CustomAttributeTypedArgument>();
				_lazyNamedArguments = namedArguments ?? Array.Empty<CustomAttributeNamedArgument>();
			}

			private IList<CustomAttributeTypedArgument> GetLatchedFixedArguments()
			{
				return _lazyFixedArguments ?? LazilyComputeArguments().FixedArguments;
			}

			private IList<CustomAttributeNamedArgument> GetLatchedNamedArguments()
			{
				return _lazyNamedArguments ?? LazilyComputeArguments().NamedArguments;
			}

			protected sealed override Type ComputeAttributeType()
			{
				return _constructor.DeclaringType;
			}

			protected sealed override ConstructorInfo ComputeConstructor()
			{
				return _constructor;
			}

			private CustomAttributeArguments LazilyComputeArguments()
			{
				CustomAttributeArguments result = _argumentsPromise();
				_lazyFixedArguments = result.FixedArguments;
				_lazyNamedArguments = result.NamedArguments;
				return result;
			}
		}

		/// <summary>
		/// Resource-only modules created by a MetadataLoadContext.
		/// </summary>
		internal sealed class RoResourceModule : RoModule
		{
			private readonly RoAssembly _assembly;

			public sealed override int MDStreamVersion
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.ResourceOnlyModule);
				}
			}

			public sealed override int MetadataToken => 0;

			public sealed override Guid ModuleVersionId
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.ResourceOnlyModule);
				}
			}

			[UnconditionalSuppressMessage("SingleFile", "IL3002:RequiresAssemblyFiles on Name", Justification = "https://github.com/dotnet/runtime/issues/56519")]
			public sealed override string ScopeName => Name;

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => Array.Empty<CustomAttributeData>();

			internal RoResourceModule(RoAssembly assembly, string fullyQualifiedName)
				: base(fullyQualifiedName)
			{
				_assembly = assembly;
			}

			internal sealed override RoAssembly GetRoAssembly()
			{
				return _assembly;
			}

			public sealed override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
			{
				peKind = PortableExecutableKinds.NotAPortableExecutableImage;
				machine = (ImageFileMachine)0;
			}

			public sealed override FieldInfo GetField(string name, BindingFlags bindingAttr)
			{
				return null;
			}

			public sealed override FieldInfo[] GetFields(BindingFlags bindingFlags)
			{
				return Array.Empty<FieldInfo>();
			}

			public sealed override MethodInfo[] GetMethods(BindingFlags bindingFlags)
			{
				return Array.Empty<MethodInfo>();
			}

			protected sealed override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
			{
				return null;
			}

			public sealed override bool IsResource()
			{
				return true;
			}

			public sealed override Type[] GetTypes()
			{
				return Type.EmptyTypes;
			}

			protected sealed override RoDefinitionType GetTypeCoreNoCache(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, out Exception e)
			{
				e = new TypeLoadException(System.SR.Format(MDCFR.Properties.Resources.TypeNotFound, ns.ToUtf16().AppendTypeName(name.ToUtf16()), Assembly));
				return null;
			}

			internal sealed override IEnumerable<RoType> GetDefinedRoTypes()
			{
				return null;
			}
		}

		internal abstract class RoStubAssembly : RoAssembly
		{
			public sealed override string Location
			{
				get
				{
					throw null;
				}
			}

			public sealed override MethodInfo EntryPoint
			{
				get
				{
					throw null;
				}
			}

			public sealed override string ImageRuntimeVersion
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsDynamic
			{
				get
				{
					throw null;
				}
			}

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes
			{
				get
				{
					throw null;
				}
			}

			public sealed override event ModuleResolveEventHandler ModuleResolve
			{
				add
				{
					throw null;
				}
				remove
				{
					throw null;
				}
			}

			internal RoStubAssembly()
				: base(null, 0)
			{
			}

			public sealed override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
			{
				throw null;
			}

			public sealed override string[] GetManifestResourceNames()
			{
				throw null;
			}

			public sealed override Stream GetManifestResourceStream(string name)
			{
				throw null;
			}

			protected sealed override AssemblyNameData[] ComputeAssemblyReferences()
			{
				throw null;
			}

			protected sealed override AssemblyNameData ComputeNameData()
			{
				throw null;
			}

			internal sealed override RoModule GetRoManifestModule()
			{
				throw null;
			}

			protected sealed override void IterateTypeForwards(TypeForwardHandler handler)
			{
				throw null;
			}

			protected sealed override RoModule LoadModule(string moduleName, bool containsMetadata)
			{
				throw null;
			}

			protected sealed override IEnumerable<AssemblyFileInfo> GetAssemblyFileInfosFromManifest(bool includeManifestModule, bool includeResourceModules)
			{
				throw null;
			}

			protected sealed override RoModule CreateModule(Stream peStream, bool containsMetadata)
			{
				throw null;
			}
		}

		internal abstract class RoStubType : RoType
		{
			public sealed override bool IsTypeDefinition
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsGenericTypeDefinition
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsSZArray
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsVariableBoundArray
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsConstructedGenericType
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsGenericParameter
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsGenericTypeParameter
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool IsGenericMethodParameter
			{
				get
				{
					throw null;
				}
			}

			public sealed override bool ContainsGenericParameters
			{
				get
				{
					throw null;
				}
			}

			public sealed override MethodBase DeclaringMethod
			{
				get
				{
					throw null;
				}
			}

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes
			{
				get
				{
					throw null;
				}
			}

			public sealed override int MetadataToken
			{
				get
				{
					throw null;
				}
			}

			public sealed override GenericParameterAttributes GenericParameterAttributes
			{
				get
				{
					throw null;
				}
			}

			public sealed override int GenericParameterPosition
			{
				get
				{
					throw null;
				}
			}

			public sealed override Guid GUID
			{
				get
				{
					throw null;
				}
			}

			public sealed override StructLayoutAttribute StructLayoutAttribute
			{
				get
				{
					throw null;
				}
			}

			protected sealed override bool HasElementTypeImpl()
			{
				throw null;
			}

			protected sealed override bool IsArrayImpl()
			{
				throw null;
			}

			protected sealed override bool IsByRefImpl()
			{
				throw null;
			}

			protected sealed override bool IsPointerImpl()
			{
				throw null;
			}

			internal sealed override RoModule GetRoModule()
			{
				throw null;
			}

			public sealed override int GetArrayRank()
			{
				throw null;
			}

			protected sealed override string ComputeName()
			{
				throw null;
			}

			protected sealed override string ComputeNamespace()
			{
				throw null;
			}

			protected sealed override string ComputeFullName()
			{
				throw null;
			}

			protected sealed override TypeAttributes ComputeAttributeFlags()
			{
				throw null;
			}

			protected sealed override TypeCode GetTypeCodeImpl()
			{
				throw null;
			}

			public sealed override string ToString()
			{
				return GetType().ToString();
			}

			protected sealed override RoType ComputeDeclaringType()
			{
				throw null;
			}

			internal sealed override bool IsCustomAttributeDefined(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				throw null;
			}

			internal sealed override CustomAttributeData TryFindCustomAttribute(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
			{
				throw null;
			}

			internal sealed override RoType GetRoElementType()
			{
				throw null;
			}

			public sealed override Type GetGenericTypeDefinition()
			{
				throw null;
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				throw null;
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				throw null;
			}

			protected internal sealed override RoType[] GetGenericArgumentsNoCopy()
			{
				throw null;
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override Type MakeGenericType(params Type[] typeArguments)
			{
				throw null;
			}

			public sealed override Type[] GetGenericParameterConstraints()
			{
				throw null;
			}

			protected internal sealed override RoType ComputeEnumUnderlyingType()
			{
				throw null;
			}

			protected sealed override RoType ComputeBaseTypeWithoutDesktopQuirk()
			{
				throw null;
			}

			protected sealed override IEnumerable<RoType> ComputeDirectlyImplementedInterfaces()
			{
				throw null;
			}

			internal sealed override IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter)
			{
				throw null;
			}

			internal sealed override IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType)
			{
				throw null;
			}

			internal sealed override IEnumerable<EventInfo> GetEventsCore(NameFilter filter, Type reflectedType)
			{
				throw null;
			}

			internal sealed override IEnumerable<FieldInfo> GetFieldsCore(NameFilter filter, Type reflectedType)
			{
				throw null;
			}

			internal sealed override IEnumerable<PropertyInfo> GetPropertiesCore(NameFilter filter, Type reflectedType)
			{
				throw null;
			}

			internal sealed override IEnumerable<RoType> GetNestedTypesCore(NameFilter filter)
			{
				throw null;
			}
		}

		/// <summary>
		/// Base class for all RoConstructors objects created by a MetadataLoadContext that appear on arrays.
		/// </summary>
		internal sealed class RoSyntheticConstructor : RoConstructor
		{
			private readonly RoType _declaringType;

			private readonly int _uniquifier;

			private readonly RoType[] _parameterTypes;

			public sealed override int MetadataToken => 100663296;

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => Array.Empty<CustomAttributeData>();

			public sealed override TypeContext TypeContext => default(TypeContext);

			internal RoSyntheticConstructor(RoType declaringType, int uniquifier, params RoType[] parameterTypes)
			{
				_declaringType = declaringType;
				_uniquifier = uniquifier;
				_parameterTypes = parameterTypes;
			}

			internal sealed override RoType GetRoDeclaringType()
			{
				return _declaringType;
			}

			internal sealed override RoModule GetRoModule()
			{
				return GetRoDeclaringType().GetRoModule();
			}

			protected sealed override string ComputeName()
			{
				return ConstructorInfo.ConstructorName;
			}

			protected sealed override MethodAttributes ComputeAttributes()
			{
				return MethodAttributes.Public | MethodAttributes.RTSpecialName;
			}

			protected sealed override CallingConventions ComputeCallingConvention()
			{
				return CallingConventions.Standard | CallingConventions.HasThis;
			}

			protected sealed override MethodImplAttributes ComputeMethodImplementationFlags()
			{
				return MethodImplAttributes.IL;
			}

			protected sealed override MethodSig<RoParameter> ComputeMethodSig()
			{
				int num = _parameterTypes.Length;
				MethodSig<RoParameter> methodSig = new MethodSig<RoParameter>(num);
				RoType coreType = GetRoModule().Loader.GetCoreType(CoreType.Void);
				methodSig[-1] = new RoThinMethodParameter(this, -1, coreType);
				for (int i = 0; i < num; i++)
				{
					methodSig[i] = new RoThinMethodParameter(this, i, _parameterTypes[i]);
				}
				return methodSig;
			}

			public sealed override MethodBody GetMethodBody()
			{
				return null;
			}

			protected sealed override MethodSig<string> ComputeMethodSigStrings()
			{
				int num = _parameterTypes.Length;
				MethodSig<string> methodSig = new MethodSig<string>(num);
				MethodSig<RoParameter> methodSig2 = ComputeMethodSig();
				for (int i = -1; i < num; i++)
				{
					methodSig[i] = methodSig2[i].ParameterType.ToString();
				}
				return methodSig;
			}

			protected sealed override MethodSig<RoType> ComputeCustomModifiers()
			{
				return new MethodSig<RoType>(_parameterTypes.Length);
			}

			public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
			{
				if (!(obj is RoSyntheticConstructor roSyntheticConstructor))
				{
					return false;
				}
				if (DeclaringType != roSyntheticConstructor.DeclaringType)
				{
					return false;
				}
				if (_uniquifier != roSyntheticConstructor._uniquifier)
				{
					return false;
				}
				return true;
			}

			public sealed override int GetHashCode()
			{
				return DeclaringType.GetHashCode() ^ _uniquifier.GetHashCode();
			}
		}

		/// <summary>
		/// Base class for all RoMethod objects created by a MetadataLoadContext that appear on arrays.
		/// </summary>
		internal sealed class RoSyntheticMethod : RoMethod
		{
			private readonly RoType _declaringType;

			private readonly int _uniquifier;

			private readonly string _name;

			private readonly RoType _returnType;

			private readonly RoType[] _parameterTypes;

			public sealed override int MetadataToken => 100663296;

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => Array.Empty<CustomAttributeData>();

			public sealed override bool IsGenericMethodDefinition => false;

			public sealed override bool IsConstructedGenericMethod => false;

			public sealed override TypeContext TypeContext => default(TypeContext);

			internal RoSyntheticMethod(RoType declaringType, int uniquifier, string name, RoType returnType, params RoType[] parameterTypes)
				: base(declaringType)
			{
				_declaringType = declaringType;
				_uniquifier = uniquifier;
				_name = name;
				_returnType = returnType;
				_parameterTypes = parameterTypes;
			}

			internal sealed override RoType GetRoDeclaringType()
			{
				return _declaringType;
			}

			internal sealed override RoModule GetRoModule()
			{
				return GetRoDeclaringType().GetRoModule();
			}

			protected sealed override string ComputeName()
			{
				return _name;
			}

			protected sealed override MethodAttributes ComputeAttributes()
			{
				return MethodAttributes.Public;
			}

			protected sealed override CallingConventions ComputeCallingConvention()
			{
				return CallingConventions.Standard | CallingConventions.HasThis;
			}

			protected sealed override MethodImplAttributes ComputeMethodImplementationFlags()
			{
				return MethodImplAttributes.IL;
			}

			protected sealed override MethodSig<RoParameter> ComputeMethodSig()
			{
				int num = _parameterTypes.Length;
				MethodSig<RoParameter> methodSig = new MethodSig<RoParameter>(num);
				methodSig[-1] = new RoThinMethodParameter(this, -1, _returnType);
				for (int i = 0; i < num; i++)
				{
					methodSig[i] = new RoThinMethodParameter(this, i, _parameterTypes[i]);
				}
				return methodSig;
			}

			public sealed override MethodBody GetMethodBody()
			{
				return null;
			}

			protected sealed override MethodSig<string> ComputeMethodSigStrings()
			{
				int num = _parameterTypes.Length;
				MethodSig<string> methodSig = new MethodSig<string>(num);
				MethodSig<RoParameter> methodSig2 = ComputeMethodSig();
				for (int i = -1; i < num; i++)
				{
					methodSig[i] = methodSig2[i].ParameterType.ToString();
				}
				return methodSig;
			}

			protected sealed override MethodSig<RoType> ComputeCustomModifiers()
			{
				return new MethodSig<RoType>(_parameterTypes.Length);
			}

			public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
			{
				if (!(obj is RoSyntheticMethod roSyntheticMethod))
				{
					return false;
				}
				if (DeclaringType != roSyntheticMethod.DeclaringType)
				{
					return false;
				}
				if (_uniquifier != roSyntheticMethod._uniquifier)
				{
					return false;
				}
				return true;
			}

			public sealed override int GetHashCode()
			{
				return DeclaringType.GetHashCode() ^ _uniquifier.GetHashCode();
			}

			public sealed override MethodInfo GetGenericMethodDefinition()
			{
				throw new InvalidOperationException();
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public sealed override MethodInfo MakeGenericMethod(params Type[] typeArguments)
			{
				throw new InvalidOperationException(System.SR.Format(MDCFR.Properties.Resources.Arg_NotGenericMethodDefinition, this));
			}

			protected sealed override RoType[] ComputeGenericArgumentsOrParameters()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
			{
				return Array.Empty<RoType>();
			}

			internal sealed override RoType[] GetGenericTypeParametersNoCopy()
			{
				return Array.Empty<RoType>();
			}
		}

		/// <summary>
		/// Base class for all RoParameter's returned by MethodBase.GetParameters() that don't have an entry in the Param table.
		/// (in practice, these are return value "parameters.") These parameters have no name, custom attributes or default values.
		/// </summary>
		internal sealed class RoThinMethodParameter : RoMethodParameter
		{
			public sealed override string Name => null;

			public sealed override ParameterAttributes Attributes => ParameterAttributes.None;

			public sealed override IEnumerable<CustomAttributeData> CustomAttributes => Array.Empty<CustomAttributeData>();

			public sealed override int MetadataToken => 134217728;

			public sealed override bool HasDefaultValue => true;

			public sealed override object RawDefaultValue => null;

			internal RoThinMethodParameter(IRoMethodBase roMethodBase, int position, Type parameterType)
				: base(roMethodBase, position, parameterType)
			{
			}
		}

		/// <summary>
		/// Base class for all Type and TypeInfo objects created by a MetadataLoadContext.
		/// </summary>
		/// <summary>
		/// Base class for all Type and TypeInfo objects created by a MetadataLoadContext.
		/// </summary>
		internal abstract class RoType : LeveledTypeInfo
		{
			/// <summary>
			/// TypeComponentsCache objects are allocated on-demand on a per-Type basis to cache hot data for key scenarios.
			/// To maximize throughput once the cache is created, the object creates all of its internal caches up front
			/// and holds entries strongly (and relying on the fact that Types themselves are held weakly to avoid immortality.)
			///
			/// Note that it is possible that two threads racing to query the same TypeInfo may allocate and query two different
			/// cache objects. Thus, this object must not be relied upon to preserve object identity.
			/// </summary>
			private sealed class TypeComponentsCache
			{
				private sealed class PerNameQueryCache<M> : ConcurrentUnifier<string, QueriedMemberList<M>> where M : MemberInfo
				{
					private readonly RoType _type;

					private readonly bool _ignoreCase;

					private readonly bool _immediateTypeOnly;

					public PerNameQueryCache(RoType type, bool ignoreCase, bool immediateTypeOnly)
					{
						_type = type;
						_ignoreCase = ignoreCase;
						_immediateTypeOnly = immediateTypeOnly;
					}

					protected sealed override QueriedMemberList<M> Factory(string key)
					{
						QueriedMemberList<M> queriedMemberList = QueriedMemberList<M>.Create(_type, key, _ignoreCase, _immediateTypeOnly);
						queriedMemberList.Compact();
						return queriedMemberList;
					}
				}

				private readonly object[] _perNameQueryCaches_CaseSensitive;

				private readonly object[] _perNameQueryCaches_CaseInsensitive;

				private readonly object[] _perNameQueryCaches_CaseSensitive_ImmediateTypeOnly;

				private readonly object[] _perNameQueryCaches_CaseInsensitive_ImmediateTypeOnly;

				private readonly object[] _nameAgnosticQueryCaches;

				private readonly RoType _type;

				public TypeComponentsCache(RoType type)
				{
					_type = type;
					_perNameQueryCaches_CaseSensitive = CreatePerNameQueryCaches(type, ignoreCase: false, immediateTypeOnly: false);
					_perNameQueryCaches_CaseInsensitive = CreatePerNameQueryCaches(type, ignoreCase: true, immediateTypeOnly: false);
					_perNameQueryCaches_CaseSensitive_ImmediateTypeOnly = CreatePerNameQueryCaches(type, ignoreCase: false, immediateTypeOnly: true);
					_perNameQueryCaches_CaseInsensitive_ImmediateTypeOnly = CreatePerNameQueryCaches(type, ignoreCase: true, immediateTypeOnly: true);
					_nameAgnosticQueryCaches = new object[6];
				}

				public QueriedMemberList<M> GetQueriedMembers<M>(string name, bool ignoreCase, bool immediateTypeOnly) where M : MemberInfo
				{
					int memberTypeIndex = MemberPolicies<M>.MemberTypeIndex;
					object[] array = ((!ignoreCase) ? (immediateTypeOnly ? _perNameQueryCaches_CaseSensitive_ImmediateTypeOnly : _perNameQueryCaches_CaseSensitive) : (immediateTypeOnly ? _perNameQueryCaches_CaseInsensitive_ImmediateTypeOnly : _perNameQueryCaches_CaseInsensitive));
					object obj = array[memberTypeIndex];
					PerNameQueryCache<M> perNameQueryCache = (PerNameQueryCache<M>)obj;
					return perNameQueryCache.GetOrAdd(name);
				}

				public QueriedMemberList<M> GetQueriedMembers<M>(bool immediateTypeOnly) where M : MemberInfo
				{
					int memberTypeIndex = MemberPolicies<M>.MemberTypeIndex;
					object obj = Volatile.Read(ref _nameAgnosticQueryCaches[memberTypeIndex]);
					if (obj == null)
					{
						QueriedMemberList<M> queriedMemberList = QueriedMemberList<M>.Create(_type, null, ignoreCase: false, immediateTypeOnly);
						queriedMemberList.Compact();
						Volatile.Write(ref _nameAgnosticQueryCaches[memberTypeIndex], queriedMemberList);
						return queriedMemberList;
					}
					QueriedMemberList<M> queriedMemberList2 = (QueriedMemberList<M>)obj;
					if (queriedMemberList2.ImmediateTypeOnly && !immediateTypeOnly)
					{
						QueriedMemberList<M> queriedMemberList3 = QueriedMemberList<M>.Create(_type, null, ignoreCase: false, immediateTypeOnly: false);
						queriedMemberList3.Compact();
						Volatile.Write(ref _nameAgnosticQueryCaches[memberTypeIndex], queriedMemberList3);
						return queriedMemberList3;
					}
					return queriedMemberList2;
				}

				private static object[] CreatePerNameQueryCaches(RoType type, bool ignoreCase, bool immediateTypeOnly)
				{
					object[] array = new object[6];
					array[0] = new PerNameQueryCache<ConstructorInfo>(type, ignoreCase, immediateTypeOnly);
					array[1] = new PerNameQueryCache<EventInfo>(type, ignoreCase, immediateTypeOnly);
					array[2] = new PerNameQueryCache<FieldInfo>(type, ignoreCase, immediateTypeOnly);
					array[3] = new PerNameQueryCache<MethodInfo>(type, ignoreCase, immediateTypeOnly);
					array[5] = new PerNameQueryCache<PropertyInfo>(type, ignoreCase, immediateTypeOnly);
					array[4] = new PerNameQueryCache<Type>(type, ignoreCase, immediateTypeOnly);
					return array;
				}
			}

			[Flags]
			private enum TypeClassification
			{
				Computed = 1,
				IsByRefLike = 4
			}

			[Flags]
			private enum BaseTypeClassification
			{
				Computed = 1,
				IsValueType = 2,
				IsEnum = 4
			}

			private volatile TypeComponentsCache _lazyCache;

			private const int GenericParameterCountAny = -1;

			private const TypeAttributes TypeAttributesSentinel = (TypeAttributes)(-1);

			private volatile string _lazyName;

			private volatile string _lazyNamespace;

			private volatile string _lazyFullName;

			private volatile string _lazyAssemblyQualifiedFullName;

			private volatile RoType _lazyDeclaringType;

			private volatile RoType _lazyBaseType = Sentinels.RoType;

			private volatile RoType[] _lazyInterfaces;

			private volatile TypeAttributes _lazyTypeAttributes = (TypeAttributes)(-1);

			private volatile RoType _lazyUnderlyingEnumType;

			private volatile TypeClassification _lazyClassification;

			private volatile BaseTypeClassification _lazyBaseTypeClassification;

			private static readonly CoreType[] s_primitiveTypes = new CoreType[14]
			{
				CoreType.Boolean,
				CoreType.Char,
				CoreType.SByte,
				CoreType.Byte,
				CoreType.Int16,
				CoreType.UInt16,
				CoreType.Int32,
				CoreType.UInt32,
				CoreType.Int64,
				CoreType.UInt64,
				CoreType.Single,
				CoreType.Double,
				CoreType.IntPtr,
				CoreType.UIntPtr
			};

			private TypeComponentsCache Cache => _lazyCache ?? (_lazyCache = new TypeComponentsCache(this));

			public sealed override Type UnderlyingSystemType => this;

			public abstract override bool IsTypeDefinition { get; }

			public abstract override bool IsGenericTypeDefinition { get; }

			public abstract override bool IsSZArray { get; }

			public abstract override bool IsVariableBoundArray { get; }

			public abstract override bool IsConstructedGenericType { get; }

			public abstract override bool IsGenericParameter { get; }

			public abstract override bool IsGenericTypeParameter { get; }

			public abstract override bool IsGenericMethodParameter { get; }

			public sealed override bool IsByRefLike => (GetClassification() & TypeClassification.IsByRefLike) != 0;

			public abstract override bool ContainsGenericParameters { get; }

			public sealed override Type[] GenericTypeParameters
			{
				get
				{
					Type[] genericTypeParametersNoCopy = GetGenericTypeParametersNoCopy();
					return genericTypeParametersNoCopy.CloneArray();
				}
			}

			public sealed override Type[] GenericTypeArguments
			{
				get
				{
					Type[] genericTypeArgumentsNoCopy = GetGenericTypeArgumentsNoCopy();
					return genericTypeArgumentsNoCopy.CloneArray();
				}
			}

			public abstract override GenericParameterAttributes GenericParameterAttributes { get; }

			public abstract override int GenericParameterPosition { get; }

			public sealed override bool IsGenericType
			{
				get
				{
					if (!IsConstructedGenericType)
					{
						return IsGenericTypeDefinition;
					}
					return true;
				}
			}

			public sealed override string Name => _lazyName ?? (_lazyName = ComputeName());

			public sealed override string Namespace => _lazyNamespace ?? (_lazyNamespace = ComputeNamespace());

			public sealed override string FullName => _lazyFullName ?? (_lazyFullName = ComputeFullName());

			public sealed override string AssemblyQualifiedName => _lazyAssemblyQualifiedFullName ?? (_lazyAssemblyQualifiedFullName = ComputeAssemblyQualifiedName());

			public sealed override Assembly Assembly => Module.Assembly;

			public sealed override Module Module => GetRoModule();

			public sealed override Type DeclaringType => GetRoDeclaringType();

			public abstract override MethodBase DeclaringMethod { get; }

			public sealed override Type ReflectedType => DeclaringType;

			public abstract override IEnumerable<CustomAttributeData> CustomAttributes { get; }

			public sealed override Type BaseType => GetRoBaseType();

			public sealed override IEnumerable<Type> ImplementedInterfaces
			{
				get
				{
					RoType[] interfacesNoCopy = GetInterfacesNoCopy();
					for (int i = 0; i < interfacesNoCopy.Length; i++)
					{
						yield return interfacesNoCopy[i];
					}
				}
			}

			public sealed override bool IsEnum => (GetBaseTypeClassification() & BaseTypeClassification.IsEnum) != 0;

			public abstract override int MetadataToken { get; }

			public sealed override MemberTypes MemberType
			{
				get
				{
					if (!base.IsPublic && !base.IsNotPublic)
					{
						return MemberTypes.NestedType;
					}
					return MemberTypes.TypeInfo;
				}
			}

			public abstract override Guid GUID { get; }

			public abstract override StructLayoutAttribute StructLayoutAttribute { get; }

			public sealed override bool IsSecurityCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecuritySafeCritical
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override bool IsSecurityTransparent
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.InvalidOperation_IsSecurity);
				}
			}

			public sealed override RuntimeTypeHandle TypeHandle
			{
				get
				{
					throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
				}
			}

			internal MetadataLoadContext Loader => GetRoModule().Loader;

			public sealed override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
			{
				return Query<ConstructorInfo>(bindingAttr).ToArray();
			}

			protected sealed override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
			{
				QueryResult<ConstructorInfo> queryResult = Query<ConstructorInfo>(bindingAttr);
				ListBuilder<ConstructorInfo> listBuilder = default(ListBuilder<ConstructorInfo>);
				QueryResult<ConstructorInfo>.QueryResultEnumerator enumerator = queryResult.GetEnumerator();
				while (enumerator.MoveNext())
				{
					ConstructorInfo current = enumerator.Current;
					if (current.QualifiesBasedOnParameterCount(bindingAttr, callConvention, types))
					{
						listBuilder.Add(current);
					}
				}
				if (listBuilder.Count == 0)
				{
					return null;
				}
				if (types.Length == 0 && listBuilder.Count == 1)
				{
					ConstructorInfo constructorInfo = listBuilder[0];
					ParameterInfo[] parametersNoCopy = constructorInfo.GetParametersNoCopy();
					if (parametersNoCopy.Length == 0)
					{
						return constructorInfo;
					}
				}
				MethodBase[] match;
				if ((bindingAttr & BindingFlags.ExactBinding) != 0)
				{
					match = listBuilder.ToArray();
					return System.DefaultBinder.ExactBinding(match, types, modifiers) as ConstructorInfo;
				}
				if (binder == null)
				{
					binder = Loader.GetDefaultBinder();
				}
				Binder binder2 = binder;
				match = listBuilder.ToArray();
				return binder2.SelectMethod(bindingAttr, match, types, modifiers) as ConstructorInfo;
			}

			public sealed override EventInfo[] GetEvents(BindingFlags bindingAttr)
			{
				return Query<EventInfo>(bindingAttr).ToArray();
			}

			public sealed override EventInfo GetEvent(string name, BindingFlags bindingAttr)
			{
				return Query<EventInfo>(name, bindingAttr).Disambiguate();
			}

			public sealed override FieldInfo[] GetFields(BindingFlags bindingAttr)
			{
				return Query<FieldInfo>(bindingAttr).ToArray();
			}

			public sealed override FieldInfo GetField(string name, BindingFlags bindingAttr)
			{
				return Query<FieldInfo>(name, bindingAttr).Disambiguate();
			}

			public sealed override MethodInfo[] GetMethods(BindingFlags bindingAttr)
			{
				return Query<MethodInfo>(bindingAttr).ToArray();
			}

			protected sealed override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
			{
				return GetMethodImplCommon(name, -1, bindingAttr, binder, callConvention, types, modifiers);
			}

			protected sealed override MethodInfo GetMethodImpl(string name, int genericParameterCount, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
			{
				return GetMethodImplCommon(name, genericParameterCount, bindingAttr, binder, callConvention, types, modifiers);
			}

			private MethodInfo GetMethodImplCommon(string name, int genericParameterCount, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
			{
				if (types == null)
				{
					return Query<MethodInfo>(name, bindingAttr).Disambiguate();
				}
				QueryResult<MethodInfo> queryResult = Query<MethodInfo>(name, bindingAttr);
				ListBuilder<MethodInfo> listBuilder = default(ListBuilder<MethodInfo>);
				QueryResult<MethodInfo>.QueryResultEnumerator enumerator = queryResult.GetEnumerator();
				while (enumerator.MoveNext())
				{
					MethodInfo current = enumerator.Current;
					if ((genericParameterCount == -1 || genericParameterCount == current.GetGenericParameterCount()) && current.QualifiesBasedOnParameterCount(bindingAttr, callConvention, types))
					{
						listBuilder.Add(current);
					}
				}
				if (listBuilder.Count == 0)
				{
					return null;
				}
				if (types.Length == 0 && listBuilder.Count == 1)
				{
					return listBuilder[0];
				}
				if (binder == null)
				{
					binder = Loader.GetDefaultBinder();
				}
				Binder binder2 = binder;
				MethodBase[] match = listBuilder.ToArray();
				return binder2.SelectMethod(bindingAttr, match, types, modifiers) as MethodInfo;
			}

			public sealed override Type[] GetNestedTypes(BindingFlags bindingAttr)
			{
				return Query<Type>(bindingAttr).ToArray();
			}

			public sealed override Type GetNestedType(string name, BindingFlags bindingAttr)
			{
				return Query<Type>(name, bindingAttr).Disambiguate();
			}

			public sealed override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
			{
				return Query<PropertyInfo>(bindingAttr).ToArray();
			}

			protected sealed override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
			{
				if (types == null && returnType == null)
				{
					return Query<PropertyInfo>(name, bindingAttr).Disambiguate();
				}
				QueryResult<PropertyInfo> queryResult = Query<PropertyInfo>(name, bindingAttr);
				ListBuilder<PropertyInfo> listBuilder = default(ListBuilder<PropertyInfo>);
				QueryResult<PropertyInfo>.QueryResultEnumerator enumerator = queryResult.GetEnumerator();
				while (enumerator.MoveNext())
				{
					PropertyInfo current = enumerator.Current;
					if (types == null || current.GetIndexParameters().Length == types.Length)
					{
						listBuilder.Add(current);
					}
				}
				if (listBuilder.Count == 0)
				{
					return null;
				}
				if (types == null || types.Length == 0)
				{
					if (listBuilder.Count == 1)
					{
						PropertyInfo propertyInfo = listBuilder[0];
						if ((object)returnType != null && !returnType.IsEquivalentTo(propertyInfo.PropertyType))
						{
							return null;
						}
						return propertyInfo;
					}
					if ((object)returnType == null)
					{
						throw new AmbiguousMatchException();
					}
				}
				if ((bindingAttr & BindingFlags.ExactBinding) != 0)
				{
					return System.DefaultBinder.ExactPropertyBinding(listBuilder.ToArray(), returnType, types, modifiers);
				}
				if (binder == null)
				{
					binder = Loader.GetDefaultBinder();
				}
				return binder.SelectProperty(bindingAttr, listBuilder.ToArray(), returnType, types, modifiers);
			}

			private QueryResult<M> Query<M>(BindingFlags bindingAttr) where M : MemberInfo
			{
				return Query<M>(null, bindingAttr, null);
			}

			private QueryResult<M> Query<M>(string name, BindingFlags bindingAttr) where M : MemberInfo
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				return Query<M>(name, bindingAttr, null);
			}

			private QueryResult<M> Query<M>(string optionalName, BindingFlags bindingAttr, Func<M, bool> optionalPredicate) where M : MemberInfo
			{
				MemberPolicies<M> @default = MemberPolicies<M>.Default;
				bindingAttr = @default.ModifyBindingFlags(bindingAttr);
				bool immediateTypeOnly = NeedToSearchImmediateTypeOnly(bindingAttr);
				bool ignoreCase = (bindingAttr & BindingFlags.IgnoreCase) != 0;
				TypeComponentsCache cache = Cache;
				QueriedMemberList<M> queriedMemberList = ((optionalName != null) ? cache.GetQueriedMembers<M>(optionalName, ignoreCase, immediateTypeOnly) : cache.GetQueriedMembers<M>(immediateTypeOnly));
				if (optionalPredicate != null)
				{
					queriedMemberList = queriedMemberList.Filter(optionalPredicate);
				}
				return new QueryResult<M>(bindingAttr, queriedMemberList);
			}

			private static bool NeedToSearchImmediateTypeOnly(BindingFlags bf)
			{
				if ((bf & (BindingFlags.Static | BindingFlags.FlattenHierarchy)) == (BindingFlags.Static | BindingFlags.FlattenHierarchy))
				{
					return false;
				}
				if ((bf & (BindingFlags.DeclaredOnly | BindingFlags.Instance)) == BindingFlags.Instance)
				{
					return false;
				}
				return true;
			}

			public sealed override MemberInfo[] GetMembers(BindingFlags bindingAttr)
			{
				return GetMemberImpl(null, MemberTypes.All, bindingAttr);
			}

			public sealed override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				return GetMemberImpl(name, MemberTypes.All, bindingAttr);
			}

			public sealed override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				return GetMemberImpl(name, type, bindingAttr);
			}

			private MemberInfo[] GetMemberImpl(string optionalNameOrPrefix, MemberTypes type, BindingFlags bindingAttr)
			{
				bool flag = optionalNameOrPrefix?.EndsWith("*", StringComparison.Ordinal) ?? false;
				string optionalName = (flag ? null : optionalNameOrPrefix);
				Func<MemberInfo, bool> optionalPredicate = null;
				if (flag)
				{
					bool flag2 = (bindingAttr & BindingFlags.IgnoreCase) != 0;
					StringComparison comparisonType = (flag2 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
					string prefix = optionalNameOrPrefix.Substring(0, optionalNameOrPrefix.Length - 1);
					optionalPredicate = (MemberInfo member) => member.Name.StartsWith(prefix, comparisonType);
				}
				MemberInfo[] array = QuerySpecificMemberTypeIfRequested(type, optionalName, bindingAttr, optionalPredicate, MemberTypes.Method, out QueryResult<MethodInfo> queryResult);
				MemberInfo[] result;
				if ((result = array) != null)
				{
					return result;
				}
				array = QuerySpecificMemberTypeIfRequested(type, optionalName, bindingAttr, optionalPredicate, MemberTypes.Constructor, out QueryResult<ConstructorInfo> queryResult2);
				if ((result = array) != null)
				{
					return result;
				}
				array = QuerySpecificMemberTypeIfRequested(type, optionalName, bindingAttr, optionalPredicate, MemberTypes.Property, out QueryResult<PropertyInfo> queryResult3);
				if ((result = array) != null)
				{
					return result;
				}
				array = QuerySpecificMemberTypeIfRequested(type, optionalName, bindingAttr, optionalPredicate, MemberTypes.Event, out QueryResult<EventInfo> queryResult4);
				if ((result = array) != null)
				{
					return result;
				}
				array = QuerySpecificMemberTypeIfRequested(type, optionalName, bindingAttr, optionalPredicate, MemberTypes.Field, out QueryResult<FieldInfo> queryResult5);
				if ((result = array) != null)
				{
					return result;
				}
				array = QuerySpecificMemberTypeIfRequested(type, optionalName, bindingAttr, optionalPredicate, MemberTypes.NestedType, out QueryResult<Type> queryResult6);
				if ((result = array) != null)
				{
					return result;
				}
				if ((type & (MemberTypes.TypeInfo | MemberTypes.NestedType)) == MemberTypes.TypeInfo)
				{
					array = QuerySpecificMemberTypeIfRequested(type, optionalName, bindingAttr, optionalPredicate, MemberTypes.TypeInfo, out queryResult6);
					if ((result = array) != null)
					{
						return result;
					}
				}
				int num = queryResult.Count + queryResult2.Count + queryResult3.Count + queryResult4.Count + queryResult5.Count + queryResult6.Count;
				MemberInfo[] array2;
				if (type != (MemberTypes.Constructor | MemberTypes.Method))
				{
					array2 = new MemberInfo[num];
				}
				else
				{
					array = new MethodBase[num];
					array2 = array;
				}
				result = array2;
				int num2 = 0;
				queryResult.CopyTo(result, num2);
				num2 += queryResult.Count;
				queryResult2.CopyTo(result, num2);
				num2 += queryResult2.Count;
				queryResult3.CopyTo(result, num2);
				num2 += queryResult3.Count;
				queryResult4.CopyTo(result, num2);
				num2 += queryResult4.Count;
				queryResult5.CopyTo(result, num2);
				num2 += queryResult5.Count;
				queryResult6.CopyTo(result, num2);
				num2 += queryResult6.Count;
				return result;
			}

			private M[] QuerySpecificMemberTypeIfRequested<M>(MemberTypes memberType, string optionalName, BindingFlags bindingAttr, Func<MemberInfo, bool> optionalPredicate, MemberTypes targetMemberType, out QueryResult<M> queryResult) where M : MemberInfo
			{
				if ((memberType & targetMemberType) == 0)
				{
					queryResult = default(QueryResult<M>);
					return null;
				}
				queryResult = Query(optionalName, bindingAttr, (Func<M, bool>)optionalPredicate);
				if ((memberType & ~targetMemberType) == 0)
				{
					return queryResult.ToArray();
				}
				return null;
			}

			private protected RoType()
			{
			}

			public sealed override Type AsType()
			{
				return this;
			}

			protected abstract override bool HasElementTypeImpl();

			protected abstract override bool IsArrayImpl();

			protected abstract override bool IsByRefImpl();

			protected abstract override bool IsPointerImpl();

			internal abstract RoType[] GetGenericTypeParametersNoCopy();

			public sealed override Type GetElementType()
			{
				return GetRoElementType();
			}

			internal abstract RoType GetRoElementType();

			public abstract override int GetArrayRank();

			public abstract override Type GetGenericTypeDefinition();

			internal abstract RoType[] GetGenericTypeArgumentsNoCopy();

			public abstract override Type[] GetGenericParameterConstraints();

			public sealed override Type[] GetGenericArguments()
			{
				Type[] genericArgumentsNoCopy = GetGenericArgumentsNoCopy();
				return genericArgumentsNoCopy.CloneArray();
			}

			protected internal abstract RoType[] GetGenericArgumentsNoCopy();

			protected abstract string ComputeName();

			protected abstract string ComputeNamespace();

			protected abstract string ComputeFullName();

			private string ComputeAssemblyQualifiedName()
			{
				string fullName = FullName;
				if (fullName == null)
				{
					return null;
				}
				string fullName2 = Assembly.FullName;
				return fullName + ", " + fullName2;
			}

			internal abstract RoModule GetRoModule();

			protected abstract RoType ComputeDeclaringType();

			internal RoType GetRoDeclaringType()
			{
				return _lazyDeclaringType ?? (_lazyDeclaringType = ComputeDeclaringType());
			}

			public sealed override IList<CustomAttributeData> GetCustomAttributesData()
			{
				return CustomAttributes.ToReadOnlyCollection();
			}

			internal abstract bool IsCustomAttributeDefined(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name);

			internal abstract CustomAttributeData TryFindCustomAttribute(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name);

			internal RoType GetRoBaseType()
			{
				if ((object)_lazyBaseType != Sentinels.RoType)
				{
					return _lazyBaseType;
				}
				return _lazyBaseType = ComputeBaseType();
			}

			private RoType ComputeBaseType()
			{
				RoType roType = ComputeBaseTypeWithoutDesktopQuirk();
				if (roType != null && roType.IsGenericParameter)
				{
					GenericParameterAttributes genericParameterAttributes = roType.GenericParameterAttributes;
					if ((genericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == 0)
					{
						roType = Loader.GetCoreType(CoreType.Object);
					}
				}
				return roType;
			}

			protected abstract RoType ComputeBaseTypeWithoutDesktopQuirk();

			public sealed override Type[] GetInterfaces()
			{
				Type[] interfacesNoCopy = GetInterfacesNoCopy();
				return interfacesNoCopy.CloneArray();
			}

			protected abstract IEnumerable<RoType> ComputeDirectlyImplementedInterfaces();

			internal RoType[] GetInterfacesNoCopy()
			{
				return _lazyInterfaces ?? (_lazyInterfaces = ComputeInterfaceClosure());
			}

			private RoType[] ComputeInterfaceClosure()
			{
				HashSet<RoType> hashSet = new HashSet<RoType>();
				RoType roType = ComputeBaseTypeWithoutDesktopQuirk();
				if (roType != null)
				{
					RoType[] interfacesNoCopy = roType.GetInterfacesNoCopy();
					foreach (RoType item in interfacesNoCopy)
					{
						hashSet.Add(item);
					}
				}
				foreach (RoType item3 in ComputeDirectlyImplementedInterfaces())
				{
					if (!hashSet.Add(item3))
					{
						RoType[] interfacesNoCopy2 = item3.GetInterfacesNoCopy();
						foreach (RoType item2 in interfacesNoCopy2)
						{
							hashSet.Add(item2);
						}
					}
				}
				if (hashSet.Count == 0)
				{
					return Array.Empty<RoType>();
				}
				RoType[] array = new RoType[hashSet.Count];
				hashSet.CopyTo(array);
				return array;
			}

			public sealed override InterfaceMapping GetInterfaceMap(Type interfaceType)
			{
				throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_InterfaceMapping);
			}

			public sealed override bool IsAssignableFrom(TypeInfo typeInfo)
			{
				return IsAssignableFrom((Type)typeInfo);
			}

			public sealed override bool IsAssignableFrom(Type c)
			{
				if (c == null)
				{
					return false;
				}
				if ((object)c == this)
				{
					return true;
				}
				c = c.UnderlyingSystemType;
				if (!(c is RoType roType) || roType.Loader != Loader)
				{
					return false;
				}
				return Assignability.IsAssignableFrom(this, c, Loader.GetAllFoundCoreTypes());
			}

			protected sealed override bool IsCOMObjectImpl()
			{
				return false;
			}

			protected sealed override bool IsValueTypeImpl()
			{
				return (GetBaseTypeClassification() & BaseTypeClassification.IsValueType) != 0;
			}

			public sealed override bool HasSameMetadataDefinitionAs(MemberInfo other)
			{
				return this.HasSameMetadataDefinitionAsCore(other);
			}

			protected sealed override TypeAttributes GetAttributeFlagsImpl()
			{
				if (_lazyTypeAttributes != (TypeAttributes)(-1))
				{
					return _lazyTypeAttributes;
				}
				return _lazyTypeAttributes = ComputeAttributeFlags();
			}

			protected abstract TypeAttributes ComputeAttributeFlags();

			protected abstract override TypeCode GetTypeCodeImpl();

			public abstract override string ToString();

			public sealed override MemberInfo[] GetDefaultMembers()
			{
				string defaultMemberName = GetDefaultMemberName();
				if (defaultMemberName == null)
				{
					return Array.Empty<MemberInfo>();
				}
				return GetMember(defaultMemberName);
			}

			private string GetDefaultMemberName()
			{
				RoType roType = this;
				while (roType != null)
				{
					CustomAttributeData customAttributeData = roType.TryFindCustomAttribute(Utf8Constants.SystemReflection, Utf8Constants.DefaultMemberAttribute);
					if (customAttributeData != null)
					{
						IList<CustomAttributeTypedArgument> constructorArguments = customAttributeData.ConstructorArguments;
						if (constructorArguments.Count == 1 && constructorArguments[0].Value is string result)
						{
							return result;
						}
					}
					roType = roType.GetRoBaseType();
				}
				return null;
			}

			public sealed override Type MakeArrayType()
			{
				return this.GetUniqueArrayType();
			}

			public sealed override Type MakeArrayType(int rank)
			{
				if (rank <= 0)
				{
					throw new IndexOutOfRangeException();
				}
				return this.GetUniqueArrayType(rank);
			}

			public sealed override Type MakeByRefType()
			{
				return this.GetUniqueByRefType();
			}

			public sealed override Type MakePointerType()
			{
				return this.GetUniquePointerType();
			}

			[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
			public abstract override Type MakeGenericType(params Type[] typeArguments);

			public sealed override Type GetEnumUnderlyingType()
			{
				return _lazyUnderlyingEnumType ?? (_lazyUnderlyingEnumType = ComputeEnumUnderlyingType());
			}

			protected internal abstract RoType ComputeEnumUnderlyingType();

			public sealed override Array GetEnumValues()
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_InvalidOperation_Reflection);
			}

			public sealed override object[] GetCustomAttributes(bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyCA);
			}

			public sealed override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
			{
				throw new InvalidOperationException(MDCFR.Properties.Resources.Arg_ReflectionOnlyInvoke);
			}

			internal abstract IEnumerable<ConstructorInfo> GetConstructorsCore(NameFilter filter);

			internal abstract IEnumerable<MethodInfo> GetMethodsCore(NameFilter filter, Type reflectedType);

			internal abstract IEnumerable<EventInfo> GetEventsCore(NameFilter filter, Type reflectedType);

			internal abstract IEnumerable<FieldInfo> GetFieldsCore(NameFilter filter, Type reflectedType);

			internal abstract IEnumerable<PropertyInfo> GetPropertiesCore(NameFilter filter, Type reflectedType);

			internal abstract IEnumerable<RoType> GetNestedTypesCore(NameFilter filter);

			internal MethodInfo InternalGetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
			{
				return GetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
			}

			public sealed override Type GetInterface(string name, bool ignoreCase)
			{
				if (name == null)
				{
					throw new ArgumentNullException("name");
				}
				name.SplitTypeName(out var ns, out var name2);
				Type type = null;
				foreach (Type implementedInterface in ImplementedInterfaces)
				{
					string name3 = implementedInterface.Name;
					if ((ignoreCase ? name2.Equals(name3, StringComparison.OrdinalIgnoreCase) : name2.Equals(name3)) && (ns.Length == 0 || ns.Equals(implementedInterface.Namespace)))
					{
						if (type != null)
						{
							throw new AmbiguousMatchException();
						}
						type = implementedInterface;
					}
				}
				return type;
			}

			private TypeClassification GetClassification()
			{
				if (_lazyClassification == (TypeClassification)0)
				{
					return _lazyClassification = ComputeClassification();
				}
				return _lazyClassification;
			}

			private TypeClassification ComputeClassification()
			{
				TypeClassification typeClassification = TypeClassification.Computed;
				if (IsCustomAttributeDefined(Utf8Constants.SystemRuntimeCompilerServices, Utf8Constants.IsByRefLikeAttribute))
				{
					typeClassification |= TypeClassification.IsByRefLike;
				}
				return typeClassification;
			}

			private BaseTypeClassification GetBaseTypeClassification()
			{
				if (_lazyBaseTypeClassification == (BaseTypeClassification)0)
				{
					return _lazyBaseTypeClassification = ComputeBaseTypeClassification();
				}
				return _lazyBaseTypeClassification;
			}

			private BaseTypeClassification ComputeBaseTypeClassification()
			{
				BaseTypeClassification baseTypeClassification = BaseTypeClassification.Computed;
				Type baseType = BaseType;
				if (baseType != null)
				{
					CoreTypes allFoundCoreTypes = Loader.GetAllFoundCoreTypes();
					Type type = allFoundCoreTypes[CoreType.Enum];
					Type type2 = allFoundCoreTypes[CoreType.ValueType];
					if (baseType == type)
					{
						baseTypeClassification |= BaseTypeClassification.IsValueType | BaseTypeClassification.IsEnum;
					}
					if (baseType == type2 && this != type)
					{
						baseTypeClassification |= BaseTypeClassification.IsValueType;
					}
				}
				return baseTypeClassification;
			}

			protected sealed override bool IsPrimitiveImpl()
			{
				CoreTypes allFoundCoreTypes = Loader.GetAllFoundCoreTypes();
				CoreType[] array = s_primitiveTypes;
				foreach (CoreType coreType in array)
				{
					if (this == allFoundCoreTypes[coreType])
					{
						return true;
					}
				}
				return false;
			}
		}

		/// <summary>
		/// Base type for RoModifiedType and RoPinnedType. These types are very ill-behaved so they are only produced in very specific circumstances
		/// and quickly peeled away once their usefulness has ended.
		/// </summary>
		internal abstract class RoWrappedType : RoStubType
		{
			internal RoType UnmodifiedType { get; }

			internal RoWrappedType(RoType unmodifiedType)
			{
				UnmodifiedType = unmodifiedType;
			}
		}

		internal static class Sentinels
		{
			private sealed class SentinelType : RoStubType { internal SentinelType() { } }

			private sealed class SentinelAssembly : RoStubAssembly { internal SentinelAssembly() { } }

			private sealed class SentinelMethod : RoMethod
			{
				public sealed override int MetadataToken
				{
					get
					{
						throw null;
					}
				}

				public sealed override IEnumerable<CustomAttributeData> CustomAttributes
				{
					get
					{
						throw null;
					}
				}

				public sealed override bool IsConstructedGenericMethod
				{
					get
					{
						throw null;
					}
				}

				public sealed override bool IsGenericMethodDefinition
				{
					get
					{
						throw null;
					}
				}

				public sealed override TypeContext TypeContext
				{
					get
					{
						throw null;
					}
				}

				internal SentinelMethod()
					: base(RoType)
				{
				}

				internal sealed override RoType GetRoDeclaringType()
				{
					throw null;
				}

				internal sealed override RoModule GetRoModule()
				{
					throw null;
				}

				public sealed override bool Equals(object obj)
				{
					throw null;
				}

				public sealed override MethodInfo GetGenericMethodDefinition()
				{
					throw null;
				}

				public sealed override int GetHashCode()
				{
					throw null;
				}

				public sealed override MethodBody GetMethodBody()
				{
					throw null;
				}

				[RequiresUnreferencedCode("If some of the generic arguments are annotated (either with DynamicallyAccessedMembersAttribute, or generic constraints), trimming can't validate that the requirements of those annotations are met.")]
				public sealed override MethodInfo MakeGenericMethod(params Type[] typeArguments)
				{
					throw null;
				}

				protected sealed override MethodAttributes ComputeAttributes()
				{
					throw null;
				}

				protected sealed override CallingConventions ComputeCallingConvention()
				{
					throw null;
				}

				protected sealed override RoType[] ComputeGenericArgumentsOrParameters()
				{
					throw null;
				}

				protected sealed override MethodImplAttributes ComputeMethodImplementationFlags()
				{
					throw null;
				}

				protected sealed override MethodSig<RoParameter> ComputeMethodSig()
				{
					throw null;
				}

				protected sealed override MethodSig<RoType> ComputeCustomModifiers()
				{
					throw null;
				}

				protected sealed override MethodSig<string> ComputeMethodSigStrings()
				{
					throw null;
				}

				protected sealed override string ComputeName()
				{
					throw null;
				}

				internal sealed override RoType[] GetGenericTypeArgumentsNoCopy()
				{
					throw null;
				}

				internal sealed override RoType[] GetGenericTypeParametersNoCopy()
				{
					throw null;
				}
			}

			public static readonly RoType RoType = new SentinelType();

			public static readonly RoMethod RoMethod = new SentinelMethod();
		}

		internal readonly struct TypeContext
		{
			public RoType[] GenericTypeArguments { get; }

			public RoType[] GenericMethodArguments { get; }

			internal TypeContext(RoType[] genericTypeArguments, RoType[] genericMethodArguments)
			{
				GenericTypeArguments = genericTypeArguments;
				GenericMethodArguments = genericMethodArguments;
			}

			public RoType GetGenericTypeArgumentOrNull(int index)
			{
				if (GenericTypeArguments == null || (uint)index >= GenericTypeArguments.Length)
				{
					return null;
				}
				return GenericTypeArguments[index];
			}

			public RoType GetGenericMethodArgumentOrNull(int index)
			{
				if (GenericMethodArguments == null || (uint)index >= GenericMethodArguments.Length)
				{
					return null;
				}
				return GenericMethodArguments[index];
			}
		}

		internal static class TypeFactories
		{
			public static RoArrayType GetUniqueArrayType(this RoType elementType)
			{
				return elementType.GetRoModule().GetUniqueArrayType(elementType);
			}

			public static RoArrayType GetUniqueArrayType(this RoType elementType, int rank)
			{
				return elementType.GetRoModule().GetUniqueArrayType(elementType, rank);
			}

			public static RoByRefType GetUniqueByRefType(this RoType elementType)
			{
				return elementType.GetRoModule().GetUniqueByRefType(elementType);
			}

			public static RoPointerType GetUniquePointerType(this RoType elementType)
			{
				return elementType.GetRoModule().GetUniquePointerType(elementType);
			}

			public static RoConstructedGenericType GetUniqueConstructedGenericType(this RoDefinitionType genericTypeDefinition, RoType[] genericTypeArguments)
			{
				return genericTypeDefinition.GetRoModule().GetUniqueConstructedGenericType(genericTypeDefinition, genericTypeArguments);
			}
		}

		internal static class Utf8Constants
		{
			public static ReadOnlySpan<byte> System => new byte[6] { 83, 121, 115, 116, 101, 109 };

			public static ReadOnlySpan<byte> SystemReflection => new byte[17] { 83, 121, 115, 116, 101, 109, 46, 82, 101, 102, 108, 101, 99, 116, 105, 111, 110 };

			public static ReadOnlySpan<byte> SystemCollectionsGeneric => new byte[26]
			{
				83, 121, 115, 116, 101, 109, 46, 67, 111, 108,
				108, 101, 99, 116, 105, 111, 110, 115, 46, 71,
				101, 110, 101, 114, 105, 99
			};

			public static ReadOnlySpan<byte> SystemRuntimeInteropServices => new byte[30]
			{
				83, 121, 115, 116, 101, 109, 46, 82, 117, 110,
				116, 105, 109, 101, 46, 73, 110, 116, 101, 114,
				111, 112, 83, 101, 114, 118, 105, 99, 101, 115
			};

			public static ReadOnlySpan<byte> SystemRuntimeCompilerServices => new byte[31]
			{
				83, 121, 115, 116, 101, 109, 46, 82, 117, 110,
				116, 105, 109, 101, 46, 67, 111, 109, 112, 105,
				108, 101, 114, 83, 101, 114, 118, 105, 99, 101,
				115
			};

			public static ReadOnlySpan<byte> Array => new byte[5] { 65, 114, 114, 97, 121 };

			public static ReadOnlySpan<byte> Boolean => new byte[7] { 66, 111, 111, 108, 101, 97, 110 };

			public static ReadOnlySpan<byte> Byte => new byte[4] { 66, 121, 116, 101 };

			public static ReadOnlySpan<byte> Char => new byte[4] { 67, 104, 97, 114 };

			public static ReadOnlySpan<byte> Double => new byte[6] { 68, 111, 117, 98, 108, 101 };

			public static ReadOnlySpan<byte> Enum => new byte[4] { 69, 110, 117, 109 };

			public static ReadOnlySpan<byte> Int16 => new byte[5] { 73, 110, 116, 49, 54 };

			public static ReadOnlySpan<byte> Int32 => new byte[5] { 73, 110, 116, 51, 50 };

			public static ReadOnlySpan<byte> Int64 => new byte[5] { 73, 110, 116, 54, 52 };

			public static ReadOnlySpan<byte> IntPtr => new byte[6] { 73, 110, 116, 80, 116, 114 };

			public static ReadOnlySpan<byte> Object => new byte[6] { 79, 98, 106, 101, 99, 116 };

			public static ReadOnlySpan<byte> NullableT => new byte[10] { 78, 117, 108, 108, 97, 98, 108, 101, 96, 49 };

			public static ReadOnlySpan<byte> SByte => new byte[5] { 83, 66, 121, 116, 101 };

			public static ReadOnlySpan<byte> Single => new byte[6] { 83, 105, 110, 103, 108, 101 };

			public static ReadOnlySpan<byte> String => new byte[6] { 83, 116, 114, 105, 110, 103 };

			public static ReadOnlySpan<byte> TypedReference => new byte[14]
			{
				84, 121, 112, 101, 100, 82, 101, 102, 101, 114,
				101, 110, 99, 101
			};

			public static ReadOnlySpan<byte> UInt16 => new byte[6] { 85, 73, 110, 116, 49, 54 };

			public static ReadOnlySpan<byte> UInt32 => new byte[6] { 85, 73, 110, 116, 51, 50 };

			public static ReadOnlySpan<byte> UInt64 => new byte[6] { 85, 73, 110, 116, 54, 52 };

			public static ReadOnlySpan<byte> UIntPtr => new byte[7] { 85, 73, 110, 116, 80, 116, 114 };

			public static ReadOnlySpan<byte> ValueType => new byte[9] { 86, 97, 108, 117, 101, 84, 121, 112, 101 };

			public static ReadOnlySpan<byte> Void => new byte[4] { 86, 111, 105, 100 };

			public static ReadOnlySpan<byte> MulticastDelegate => new byte[17]
			{
				77, 117, 108, 116, 105, 99, 97, 115, 116, 68,
				101, 108, 101, 103, 97, 116, 101
			};

			public static ReadOnlySpan<byte> IEnumerableT => new byte[13]
			{
				73, 69, 110, 117, 109, 101, 114, 97, 98, 108,
				101, 96, 49
			};

			public static ReadOnlySpan<byte> ICollectionT => new byte[13]
			{
				73, 67, 111, 108, 108, 101, 99, 116, 105, 111,
				110, 96, 49
			};

			public static ReadOnlySpan<byte> IListT => new byte[7] { 73, 76, 105, 115, 116, 96, 49 };

			public static ReadOnlySpan<byte> IReadOnlyListT => new byte[15]
			{
				73, 82, 101, 97, 100, 79, 110, 108, 121, 76,
				105, 115, 116, 96, 49
			};

			public static ReadOnlySpan<byte> Type => new byte[4] { 84, 121, 112, 101 };

			public static ReadOnlySpan<byte> DBNull => new byte[6] { 68, 66, 78, 117, 108, 108 };

			public static ReadOnlySpan<byte> Decimal => new byte[7] { 68, 101, 99, 105, 109, 97, 108 };

			public static ReadOnlySpan<byte> DateTime => new byte[8] { 68, 97, 116, 101, 84, 105, 109, 101 };

			public static ReadOnlySpan<byte> ComImportAttribute => new byte[18]
			{
				67, 111, 109, 73, 109, 112, 111, 114, 116, 65,
				116, 116, 114, 105, 98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> DllImportAttribute => new byte[18]
			{
				68, 108, 108, 73, 109, 112, 111, 114, 116, 65,
				116, 116, 114, 105, 98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> CallingConvention => new byte[17]
			{
				67, 97, 108, 108, 105, 110, 103, 67, 111, 110,
				118, 101, 110, 116, 105, 111, 110
			};

			public static ReadOnlySpan<byte> CharSet => new byte[7] { 67, 104, 97, 114, 83, 101, 116 };

			public static ReadOnlySpan<byte> MarshalAsAttribute => new byte[18]
			{
				77, 97, 114, 115, 104, 97, 108, 65, 115, 65,
				116, 116, 114, 105, 98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> UnmanagedType => new byte[13]
			{
				85, 110, 109, 97, 110, 97, 103, 101, 100, 84,
				121, 112, 101
			};

			public static ReadOnlySpan<byte> VarEnum => new byte[7] { 86, 97, 114, 69, 110, 117, 109 };

			public static ReadOnlySpan<byte> InAttribute => new byte[11]
			{
				73, 110, 65, 116, 116, 114, 105, 98, 117, 116,
				101
			};

			public static ReadOnlySpan<byte> OutAttriubute => new byte[12]
			{
				79, 117, 116, 65, 116, 116, 114, 105, 98, 117,
				116, 101
			};

			public static ReadOnlySpan<byte> OptionalAttribute => new byte[17]
			{
				79, 112, 116, 105, 111, 110, 97, 108, 65, 116,
				116, 114, 105, 98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> PreserveSigAttribute => new byte[20]
			{
				80, 114, 101, 115, 101, 114, 118, 101, 83, 105,
				103, 65, 116, 116, 114, 105, 98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> FieldOffsetAttribute => new byte[20]
			{
				70, 105, 101, 108, 100, 79, 102, 102, 115, 101,
				116, 65, 116, 116, 114, 105, 98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> IsByRefLikeAttribute => new byte[20]
			{
				73, 115, 66, 121, 82, 101, 102, 76, 105, 107,
				101, 65, 116, 116, 114, 105, 98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> DecimalConstantAttribute => new byte[24]
			{
				68, 101, 99, 105, 109, 97, 108, 67, 111, 110,
				115, 116, 97, 110, 116, 65, 116, 116, 114, 105,
				98, 117, 116, 101
			};

			public static ReadOnlySpan<byte> CustomConstantAttribute => new byte[23]
			{
				67, 117, 115, 116, 111, 109, 67, 111, 110, 115,
				116, 97, 110, 116, 65, 116, 116, 114, 105, 98,
				117, 116, 101
			};

			public static ReadOnlySpan<byte> GuidAttribute => new byte[13]
			{
				71, 117, 105, 100, 65, 116, 116, 114, 105, 98,
				117, 116, 101
			};

			public static ReadOnlySpan<byte> DefaultMemberAttribute => new byte[22]
			{
				68, 101, 102, 97, 117, 108, 116, 77, 101, 109,
				98, 101, 114, 65, 116, 116, 114, 105, 98, 117,
				116, 101
			};

			public static ReadOnlySpan<byte> DateTimeConstantAttribute => new byte[25]
			{
				68, 97, 116, 101, 84, 105, 109, 101, 67, 111,
				110, 115, 116, 97, 110, 116, 65, 116, 116, 114,
				105, 98, 117, 116, 101
			};
		}

		namespace Ecma
		{
			using System.Linq;
			using System.Reflection.Metadata;
			using System.Collections.Immutable;
			using System.Configuration.Assemblies;
			using System.Reflection.Metadata.Ecma335;
			using System.Reflection.PortableExecutable;
			
			/// <summary>
			/// Base class for all Assembly objects created by a MetadataLoadContext and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaAssembly : RoAssembly
			{
				private readonly string _location;

				private readonly EcmaModule _manifestModule;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly AssemblyDefinition _neverAccessThisExceptThroughAssemblyDefinitionProperty;

				public sealed override MethodInfo EntryPoint => GetEcmaManifestModule().ComputeEntryPoint(fileRefEntryPointAllowed: true);

				public sealed override string ImageRuntimeVersion => Reader.MetadataVersion;

				public sealed override bool IsDynamic => false;

				public sealed override string Location => _location;

				public sealed override IEnumerable<CustomAttributeData> CustomAttributes => AssemblyDefinition.GetCustomAttributes().ToTrueCustomAttributes(GetEcmaManifestModule());

				internal MetadataReader Reader => _manifestModule.Reader;

				private ref readonly AssemblyDefinition AssemblyDefinition
				{
					get
					{
						base.Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughAssemblyDefinitionProperty;
					}
				}

				public sealed override event ModuleResolveEventHandler ModuleResolve;

				internal EcmaAssembly(MetadataLoadContext loader, PEReader peReader, MetadataReader reader, string location)
					: base(loader, reader.AssemblyFiles.Count)
				{
					_location = location;
					_neverAccessThisExceptThroughAssemblyDefinitionProperty = reader.GetAssemblyDefinition();
					_manifestModule = new EcmaModule(this, location, peReader, reader);
				}

				internal sealed override RoModule GetRoManifestModule()
				{
					return _manifestModule;
				}

				internal EcmaModule GetEcmaManifestModule()
				{
					return _manifestModule;
				}

				protected sealed override AssemblyNameData[] ComputeAssemblyReferences()
				{
					MetadataReader reader = Reader;
					AssemblyNameData[] array = new AssemblyNameData[reader.AssemblyReferences.Count];
					int num = 0;
					foreach (AssemblyReferenceHandle assemblyReference2 in reader.AssemblyReferences)
					{
						AssemblyReference assemblyReference = assemblyReference2.GetAssemblyReference(reader);
						AssemblyNameData assemblyNameData = new AssemblyNameData();
						AssemblyNameFlags assemblyNameFlags = (assemblyNameData.Flags = assemblyReference.Flags.ToAssemblyNameFlags());
						assemblyNameData.Name = assemblyReference.Name.GetString(reader);
						assemblyNameData.Version = assemblyReference.Version.AdjustForUnspecifiedVersionComponents();
						assemblyNameData.CultureName = assemblyReference.Culture.GetStringOrNull(reader) ?? string.Empty;
						if ((assemblyNameFlags & AssemblyNameFlags.PublicKey) != 0)
						{
							byte[] array2 = (assemblyNameData.PublicKey = assemblyReference.PublicKeyOrToken.GetBlobBytes(reader));
							if (array2.Length != 0)
							{
								assemblyNameData.PublicKeyToken = array2.ComputePublicKeyToken();
							}
						}
						else
						{
							assemblyNameData.PublicKeyToken = assemblyReference.PublicKeyOrToken.GetBlobBytes(reader);
						}
						array[num++] = assemblyNameData;
					}
					return array;
				}

				protected sealed override void IterateTypeForwards(TypeForwardHandler handler)
				{
					MetadataReader reader = Reader;
					foreach (ExportedTypeHandle exportedType2 in reader.ExportedTypes)
					{
						ExportedType exportedType = reader.GetExportedType(exportedType2);
						if (exportedType.IsForwarder)
						{
							EntityHandle implementation = exportedType.Implementation;
							if (implementation.Kind == HandleKind.AssemblyReference)
							{
								RoAssembly redirectedAssembly = ((AssemblyReferenceHandle)implementation).ResolveToAssemblyOrExceptionAssembly(GetEcmaManifestModule());
								ReadOnlySpan<byte> ns = exportedType.Namespace.AsReadOnlySpan(reader);
								ReadOnlySpan<byte> name = exportedType.Name.AsReadOnlySpan(reader);
								handler(redirectedAssembly, ns, name);
							}
						}
					}
				}

				protected sealed override AssemblyNameData ComputeNameData()
				{
					MetadataReader reader = Reader;
					AssemblyDefinition assemblyDefinition = AssemblyDefinition;
					AssemblyNameData assemblyNameData = new AssemblyNameData
					{
						Name = assemblyDefinition.Name.GetString(reader),
						Version = assemblyDefinition.Version,
						CultureName = (assemblyDefinition.Culture.GetStringOrNull(reader) ?? string.Empty)
					};
					byte[] array = (assemblyNameData.PublicKey = assemblyDefinition.PublicKey.GetBlobBytes(reader));
					if (array.Length != 0)
					{
						assemblyNameData.PublicKeyToken = array.ComputePublicKeyToken();
					}
					AssemblyNameFlags assemblyNameFlags = assemblyDefinition.Flags.ToAssemblyNameFlags() | AssemblyNameFlags.PublicKey;
					assemblyNameData.Flags = assemblyNameFlags.ExtractAssemblyNameFlags();
					assemblyNameData.HashAlgorithm = assemblyDefinition.HashAlgorithm.ToConfigurationAssemblyHashAlgorithm();
					assemblyNameData.ContentType = assemblyNameFlags.ExtractAssemblyContentType();
					ManifestModule.GetPEKind(out var peKind, out var machine);
					switch (machine)
					{
					case ImageFileMachine.AMD64:
						assemblyNameData.ProcessorArchitecture = ProcessorArchitecture.Amd64;
						break;
					case ImageFileMachine.ARM:
						assemblyNameData.ProcessorArchitecture = ProcessorArchitecture.Arm;
						break;
					case ImageFileMachine.IA64:
						assemblyNameData.ProcessorArchitecture = ProcessorArchitecture.IA64;
						break;
					case ImageFileMachine.I386:
						if ((peKind & PortableExecutableKinds.Required32Bit) != 0)
						{
							assemblyNameData.ProcessorArchitecture = ProcessorArchitecture.X86;
						}
						else
						{
							assemblyNameData.ProcessorArchitecture = ProcessorArchitecture.MSIL;
						}
						break;
					default:
						assemblyNameData.ProcessorArchitecture = ProcessorArchitecture.None;
						break;
					}
					return assemblyNameData;
				}

				public sealed override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
				{
					if (resourceName == null)
					{
						throw new ArgumentNullException("resourceName");
					}
					if (resourceName.Length == 0)
					{
						throw new ArgumentException(null, "resourceName");
					}
					InternalManifestResourceInfo internalManifestResourceInfo = GetEcmaManifestModule().GetInternalManifestResourceInfo(resourceName);
					if (!internalManifestResourceInfo.Found)
					{
						return null;
					}
					if (internalManifestResourceInfo.ResourceLocation == ResourceLocation.ContainedInAnotherAssembly)
					{
						ManifestResourceInfo manifestResourceInfo = internalManifestResourceInfo.ReferencedAssembly.GetManifestResourceInfo(resourceName);
						internalManifestResourceInfo.FileName = manifestResourceInfo.FileName ?? string.Empty;
						internalManifestResourceInfo.ResourceLocation = manifestResourceInfo.ResourceLocation | ResourceLocation.ContainedInAnotherAssembly;
						if (manifestResourceInfo.ReferencedAssembly != null)
						{
							internalManifestResourceInfo.ReferencedAssembly = manifestResourceInfo.ReferencedAssembly;
						}
					}
					return new ManifestResourceInfo(internalManifestResourceInfo.ReferencedAssembly, internalManifestResourceInfo.FileName, internalManifestResourceInfo.ResourceLocation);
				}

				public sealed override string[] GetManifestResourceNames()
				{
					MetadataReader reader = Reader;
					ManifestResourceHandleCollection manifestResources = reader.ManifestResources;
					string[] array = new string[manifestResources.Count];
					int num = 0;
					foreach (ManifestResourceHandle item in manifestResources)
					{
						array[num] = item.GetManifestResource(reader).Name.GetString(reader);
						num++;
					}
					return array;
				}

				[UnconditionalSuppressMessage("SingleFile", "IL3002:RequiresAssemblyFiles on Module.GetFile", Justification = "ResourceLocation should never be ContainedInAnotherAssembly if embedded in a single-file")]
				public unsafe sealed override Stream GetManifestResourceStream(string name)
				{
					if (name == null)
					{
						throw new ArgumentNullException("name");
					}
					if (name.Length == 0)
					{
						throw new ArgumentException(null, "name");
					}
					InternalManifestResourceInfo internalManifestResourceInfo = GetEcmaManifestModule().GetInternalManifestResourceInfo(name);
					if (!internalManifestResourceInfo.Found)
					{
						return null;
					}
					if ((internalManifestResourceInfo.ResourceLocation & ResourceLocation.Embedded) != 0)
					{
						return new UnmanagedMemoryStream(internalManifestResourceInfo.PointerToResource, internalManifestResourceInfo.SizeOfResource);
					}
					if (internalManifestResourceInfo.ResourceLocation == ResourceLocation.ContainedInAnotherAssembly)
					{
						return internalManifestResourceInfo.ReferencedAssembly.GetManifestResourceStream(name);
					}
					return GetFile(internalManifestResourceInfo.FileName);
				}

				protected sealed override RoModule LoadModule(string moduleName, bool containsMetadata)
				{
					FileStream fileStream = FindModuleNextToAssembly(moduleName);
					if (fileStream != null)
					{
						return CreateModule(fileStream, containsMetadata);
					}
					Module module = ModuleResolve?.Invoke(this, new ResolveEventArgs(moduleName));
					if (module != null)
					{
						if (!(module is RoModule roModule) || roModule.Loader != base.Loader)
						{
							throw new FileLoadException(MDCFR.Properties.Resources.ModuleResolveEventReturnedExternalModule);
						}
						return roModule;
					}
					throw new FileNotFoundException(System.SR.Format(MDCFR.Properties.Resources.FileNotFoundModule, moduleName));
				}

				[UnconditionalSuppressMessage("SingleFile", "IL3000: Avoid accessing Assembly file path when publishing as a single file", Justification = "The code has a fallback using a ModuleResolveEventHandler")]
				private FileStream FindModuleNextToAssembly(string moduleName)
				{
					string location = Location;
					if (location == null || location.Length == 0)
					{
						return null;
					}
					string directoryName = Path.GetDirectoryName(location);
					string path = Path.Combine(directoryName, moduleName);
					if (File.Exists(path))
					{
						return File.OpenRead(path);
					}
					return null;
				}

				protected sealed override RoModule CreateModule(Stream peStream, bool containsMetadata)
				{
					string fullyQualifiedName = "<Unknown>";
					if (peStream is FileStream fileStream)
					{
						fullyQualifiedName = fileStream.Name;
					}
					if (!containsMetadata)
					{
						peStream.Close();
						return new RoResourceModule(this, fullyQualifiedName);
					}
					PEReader pEReader = new PEReader(peStream);
					base.Loader.RegisterForDisposal(pEReader);
					return new EcmaModule(this, fullyQualifiedName, pEReader, pEReader.GetMetadataReader());
				}

				protected sealed override IEnumerable<AssemblyFileInfo> GetAssemblyFileInfosFromManifest(bool includeManifestModule, bool includeResourceModules)
				{
					MetadataReader reader = Reader;
					if (includeManifestModule)
					{
						yield return new AssemblyFileInfo(reader.GetModuleDefinition().Name.GetString(reader), containsMetadata: true, 0);
					}
					foreach (AssemblyFileHandle assemblyFile2 in reader.AssemblyFiles)
					{
						AssemblyFile assemblyFile = assemblyFile2.GetAssemblyFile(reader);
						if (includeResourceModules || assemblyFile.ContainsMetadata)
						{
							yield return new AssemblyFileInfo(assemblyFile.Name.GetString(reader), assemblyFile.ContainsMetadata, assemblyFile2.GetToken().GetTokenRowNumber());
						}
					}
				}
			}

			internal sealed class EcmaCustomAttributeData : RoCustomAttributeData
			{
				private readonly CustomAttributeHandle _handle;

				private readonly EcmaModule _module;

				private volatile IList<CustomAttributeTypedArgument<RoType>> _lazyFixedArguments;

				private volatile IList<CustomAttributeNamedArgument<RoType>> _lazyNamedArguments;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly CustomAttribute _neverAccessThisExceptThroughCustomAttributeProperty;

				public sealed override IList<CustomAttributeTypedArgument> ConstructorArguments
				{
					get
					{
						if (_lazyFixedArguments == null)
						{
							LoadArguments();
						}
						return _lazyFixedArguments.ToApiForm();
					}
				}

				public sealed override IList<CustomAttributeNamedArgument> NamedArguments
				{
					get
					{
						if (_lazyNamedArguments == null)
						{
							LoadArguments();
						}
						return _lazyNamedArguments.ToApiForm(AttributeType);
					}
				}

				private MetadataReader Reader => _module.Reader;

				private MetadataLoadContext Loader => _module.Loader;

				private ref readonly CustomAttribute CustomAttribute
				{
					get
					{
						Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughCustomAttributeProperty;
					}
				}

				internal EcmaCustomAttributeData(CustomAttributeHandle handle, EcmaModule module)
				{
					_handle = handle;
					_module = module;
					_neverAccessThisExceptThroughCustomAttributeProperty = handle.GetCustomAttribute(Reader);
				}

				protected sealed override Type ComputeAttributeType()
				{
					EntityHandle entityHandle = CustomAttribute.TryGetDeclaringTypeHandle(Reader);
					if (entityHandle.IsNil)
					{
						throw new BadImageFormatException();
					}
					EntityHandle handle = entityHandle;
					EcmaModule module = _module;
					TypeContext typeContext = default(TypeContext);
					return handle.ResolveTypeDefRefOrSpec(module, in typeContext);
				}

				protected sealed override ConstructorInfo ComputeConstructor()
				{
					EntityHandle constructor = CustomAttribute.Constructor;
					switch (constructor.Kind)
					{
					case HandleKind.MethodDefinition:
					{
						MethodDefinitionHandle handle = (MethodDefinitionHandle)constructor;
						EcmaDefinitionType declaringType = handle.GetMethodDefinition(Reader).GetDeclaringType().ResolveTypeDef(_module);
						return new RoDefinitionConstructor<EcmaMethodDecoder>(declaringType, new EcmaMethodDecoder(handle, _module));
					}
					case HandleKind.MemberReference:
					{
						TypeContext typeContext = default(TypeContext);
						MemberReference memberReference = ((MemberReferenceHandle)constructor).GetMemberReference(Reader);
						Type[] array = ImmutableArrayExtensions.ToArray<RoType>(memberReference.DecodeMethodSignature(_module, typeContext).ParameterTypes);
						Type[] types = array;
						Type type = memberReference.Parent.ResolveTypeDefRefOrSpec(_module, in typeContext);
						ConstructorInfo constructor2 = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.ExactBinding, null, types, null);
						if (constructor2 == null)
						{
							throw new MissingMethodException(System.SR.Format(MDCFR.Properties.Resources.MissingCustomAttributeConstructor, type));
						}
						return constructor2;
					}
					case HandleKind.MethodSpecification:
						throw new BadImageFormatException();
					default:
						throw new BadImageFormatException();
					}
				}

				private void LoadArguments()
				{
					CustomAttributeValue<RoType> customAttributeValue = CustomAttribute.DecodeValue(_module);
					_lazyFixedArguments = (IList<CustomAttributeTypedArgument<RoType>>)(object)customAttributeValue.FixedArguments;
					_lazyNamedArguments = (IList<CustomAttributeNamedArgument<RoType>>)(object)customAttributeValue.NamedArguments;
				}
			}

			internal static class EcmaCustomAttributeHelpers
			{
				/// <summary>
				/// Converts ECMA-encoded custom attributes into a freshly allocated CustomAttributeData object suitable for direct return
				/// from the CustomAttributes api.
				/// </summary>
				public static IEnumerable<CustomAttributeData> ToTrueCustomAttributes(this CustomAttributeHandleCollection handles, EcmaModule module)
				{
					foreach (CustomAttributeHandle item in handles)
					{
						yield return item.ToCustomAttributeData(module);
					}
				}

				public static CustomAttributeData ToCustomAttributeData(this CustomAttributeHandle handle, EcmaModule module)
				{
					return new EcmaCustomAttributeData(handle, module);
				}

				public static bool IsCustomAttributeDefined(this CustomAttributeHandleCollection handles, ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, EcmaModule module)
				{
					return !handles.FindCustomAttributeByName(ns, name, module).IsNil;
				}

				public static CustomAttributeData TryFindCustomAttribute(this CustomAttributeHandleCollection handles, ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, EcmaModule module)
				{
					CustomAttributeHandle handle = handles.FindCustomAttributeByName(ns, name, module);
					if (handle.IsNil)
					{
						return null;
					}
					return handle.ToCustomAttributeData(module);
				}

				private static CustomAttributeHandle FindCustomAttributeByName(this CustomAttributeHandleCollection handles, ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, EcmaModule module)
				{
					MetadataReader reader = module.Reader;
					foreach (CustomAttributeHandle item in handles)
					{
						CustomAttribute ca = item.GetCustomAttribute(reader);
						EntityHandle handle = ca.TryGetDeclaringTypeHandle(reader);
						if (!handle.IsNil && handle.TypeMatchesNameAndNamespace(ns, name, reader))
						{
							return item;
						}
					}
					return default(CustomAttributeHandle);
				}

				public static bool TypeMatchesNameAndNamespace(this EntityHandle handle, ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, MetadataReader reader)
				{
					switch (handle.Kind)
					{
					case HandleKind.TypeDefinition:
					{
						TypeDefinition typeDefinition = ((TypeDefinitionHandle)handle).GetTypeDefinition(reader);
						if (MetadataExtensions.Equals(typeDefinition.Name, name, reader))
						{
							return MetadataExtensions.Equals(typeDefinition.Namespace, ns, reader);
						}
						return false;
					}
					case HandleKind.TypeReference:
					{
						TypeReference typeReference = ((TypeReferenceHandle)handle).GetTypeReference(reader);
						if (typeReference.ResolutionScope.Kind != HandleKind.TypeReference && MetadataExtensions.Equals(typeReference.Name, name, reader))
						{
							return MetadataExtensions.Equals(typeReference.Namespace, ns, reader);
						}
						return false;
					}
					default:
						return false;
					}
				}

				public static EntityHandle TryGetDeclaringTypeHandle(this in CustomAttribute ca, MetadataReader reader)
				{
					EntityHandle constructor = ca.Constructor;
					switch (constructor.Kind)
					{
					case HandleKind.MethodDefinition:
					{
						MethodDefinitionHandle handle2 = (MethodDefinitionHandle)constructor;
						return handle2.GetMethodDefinition(reader).GetDeclaringType();
					}
					case HandleKind.MemberReference:
					{
						MemberReferenceHandle handle = (MemberReferenceHandle)constructor;
						return handle.GetMemberReference(reader).Parent;
					}
					default:
						return default(EntityHandle);
					}
				}

				/// <summary>
				/// Converts a list of System.Reflection.Metadata CustomAttributeTypedArgument&lt;&gt; into a freshly allocated CustomAttributeTypedArgument
				/// list suitable for direct return from the CustomAttributes api.
				/// </summary>
				public static IList<CustomAttributeTypedArgument> ToApiForm(this IList<CustomAttributeTypedArgument<RoType>> catgs)
				{
					int count = catgs.Count;
					CustomAttributeTypedArgument[] array = new CustomAttributeTypedArgument[count];
					for (int i = 0; i < count; i++)
					{
						array[i] = catgs[i].ToApiForm();
					}
					return array.ToReadOnlyCollection();
				}

				/// <summary>
				/// Converts a System.Reflection.Metadata CustomAttributeTypedArgument&lt;&gt; into a freshly allocated CustomAttributeTypedArgument
				/// object suitable for direct return from the CustomAttributes api.
				/// </summary>
				public static CustomAttributeTypedArgument ToApiForm(this CustomAttributeTypedArgument<RoType> catg)
				{
					return ToApiForm(catg.Type, catg.Value);
				}

				private static CustomAttributeTypedArgument ToApiForm(Type type, object value)
				{
					if (!(value is IList<CustomAttributeTypedArgument<RoType>> catgs))
					{
						return new CustomAttributeTypedArgument(type, value);
					}
					return new CustomAttributeTypedArgument(type, catgs.ToApiForm());
				}

				/// <summary>
				/// Converts a list of System.Reflection.Metadata CustomAttributeNamedArgument&lt;&gt; into a freshly allocated CustomAttributeNamedArgument
				/// list suitable for direct return from the CustomAttributes api.
				/// </summary>
				public static IList<CustomAttributeNamedArgument> ToApiForm(this IList<CustomAttributeNamedArgument<RoType>> cangs, Type attributeType)
				{
					int count = cangs.Count;
					CustomAttributeNamedArgument[] array = new CustomAttributeNamedArgument[count];
					for (int i = 0; i < count; i++)
					{
						array[i] = cangs[i].ToApiForm(attributeType);
					}
					return array.ToReadOnlyCollection();
				}

				/// <summary>
				/// Converts a System.Reflection.Metadata CustomAttributeNamedArgument&lt;&gt; into a freshly allocated CustomAttributeNamedArgument
				/// object suitable for direct return from the CustomAttributes api.
				/// </summary>
				public static CustomAttributeNamedArgument ToApiForm(this CustomAttributeNamedArgument<RoType> cang, Type attributeType)
				{
					return new CustomAttributeNamedArgument(cang.Kind switch
					{
						CustomAttributeNamedArgumentKind.Field => attributeType.GetField(cang.Name, BindingFlags.Instance | BindingFlags.Public), 
						CustomAttributeNamedArgumentKind.Property => attributeType.GetProperty(cang.Name, BindingFlags.Instance | BindingFlags.Public), 
						_ => throw new BadImageFormatException(), 
					}, ToApiForm(cang.Type, cang.Value));
				}

				public static MarshalAsAttribute ToMarshalAsAttribute(this BlobHandle blobHandle, EcmaModule module)
				{
					MetadataReader reader = module.Reader;
					BlobReader blobReader = blobHandle.GetBlobReader(reader);
					UnmanagedType unmanagedType = (UnmanagedType)blobReader.ReadByte();
					MarshalAsAttribute marshalAsAttribute = new MarshalAsAttribute(unmanagedType);
					switch (unmanagedType)
					{
					case UnmanagedType.IUnknown:
					case UnmanagedType.IDispatch:
					case UnmanagedType.Interface:
						if (blobReader.RemainingBytes != 0)
						{
							marshalAsAttribute.IidParameterIndex = blobReader.ReadCompressedInteger();
						}
						break;
					case UnmanagedType.ByValArray:
						if (blobReader.RemainingBytes != 0)
						{
							marshalAsAttribute.SizeConst = blobReader.ReadCompressedInteger();
							if (blobReader.RemainingBytes != 0)
							{
								marshalAsAttribute.ArraySubType = (UnmanagedType)blobReader.ReadCompressedInteger();
							}
						}
						break;
					case UnmanagedType.ByValTStr:
						if (blobReader.RemainingBytes != 0)
						{
							marshalAsAttribute.SizeConst = blobReader.ReadCompressedInteger();
						}
						break;
					case UnmanagedType.SafeArray:
						if (blobReader.RemainingBytes != 0)
						{
							marshalAsAttribute.SafeArraySubType = (VarEnum)blobReader.ReadCompressedInteger();
							if (blobReader.RemainingBytes != 0)
							{
								string name = blobReader.ReadSerializedString();
								marshalAsAttribute.SafeArrayUserDefinedSubType = Helpers.LoadTypeFromAssemblyQualifiedName(name, module.GetRoAssembly(), ignoreCase: false, throwOnError: false);
							}
						}
						break;
					case UnmanagedType.LPArray:
						if (blobReader.RemainingBytes == 0)
						{
							break;
						}
						marshalAsAttribute.ArraySubType = (UnmanagedType)blobReader.ReadCompressedInteger();
						if (blobReader.RemainingBytes != 0)
						{
							marshalAsAttribute.SizeParamIndex = (short)blobReader.ReadCompressedInteger();
							if (blobReader.RemainingBytes != 0)
							{
								marshalAsAttribute.SizeConst = blobReader.ReadCompressedInteger();
							}
						}
						break;
					case UnmanagedType.CustomMarshaler:
						if (blobReader.RemainingBytes == 0)
						{
							break;
						}
						blobReader.ReadSerializedString();
						if (blobReader.RemainingBytes == 0)
						{
							break;
						}
						blobReader.ReadSerializedString();
						if (blobReader.RemainingBytes != 0)
						{
							marshalAsAttribute.MarshalType = blobReader.ReadSerializedString();
							marshalAsAttribute.MarshalTypeRef = Helpers.LoadTypeFromAssemblyQualifiedName(marshalAsAttribute.MarshalType, module.GetRoAssembly(), ignoreCase: false, throwOnError: false);
							if (blobReader.RemainingBytes != 0)
							{
								marshalAsAttribute.MarshalCookie = blobReader.ReadSerializedString();
							}
						}
						break;
					}
					return marshalAsAttribute;
				}
			}

			internal static class EcmaDefaultValueProcessing
			{
				public static object ToRawObject(this ConstantHandle constantHandle, MetadataReader metadataReader)
				{
					if (constantHandle.IsNil)
					{
						throw new BadImageFormatException();
					}
					Constant constant = metadataReader.GetConstant(constantHandle);
					if (constant.Value.IsNil && constant.TypeCode != ConstantTypeCode.String)
					{
						throw new BadImageFormatException();
					}
					BlobReader blobReader = metadataReader.GetBlobReader(constant.Value);
					switch (constant.TypeCode)
					{
					case ConstantTypeCode.Boolean:
						return blobReader.ReadBoolean();
					case ConstantTypeCode.Char:
						return blobReader.ReadChar();
					case ConstantTypeCode.SByte:
						return blobReader.ReadSByte();
					case ConstantTypeCode.Int16:
						return blobReader.ReadInt16();
					case ConstantTypeCode.Int32:
						return blobReader.ReadInt32();
					case ConstantTypeCode.Int64:
						return blobReader.ReadInt64();
					case ConstantTypeCode.Byte:
						return blobReader.ReadByte();
					case ConstantTypeCode.UInt16:
						return blobReader.ReadUInt16();
					case ConstantTypeCode.UInt32:
						return blobReader.ReadUInt32();
					case ConstantTypeCode.UInt64:
						return blobReader.ReadUInt64();
					case ConstantTypeCode.Single:
						return blobReader.ReadSingle();
					case ConstantTypeCode.Double:
						return blobReader.ReadDouble();
					case ConstantTypeCode.String:
						return blobReader.ReadUTF16(blobReader.Length);
					case ConstantTypeCode.NullReference:
						if (blobReader.ReadUInt32() == 0)
						{
							return null;
						}
						break;
					}
					throw new BadImageFormatException();
				}

				public static bool TryFindRawDefaultValueFromCustomAttributes(this CustomAttributeHandleCollection handles, EcmaModule module, out object rawDefaultValue)
				{
					rawDefaultValue = null;
					MetadataReader reader = module.Reader;
					foreach (CustomAttributeHandle item in handles)
					{
						CustomAttribute ca = item.GetCustomAttribute(reader);
						EntityHandle handle = ca.TryGetDeclaringTypeHandle(reader);
						if (handle.IsNil)
						{
							continue;
						}
						if (handle.TypeMatchesNameAndNamespace(Utf8Constants.SystemRuntimeCompilerServices, Utf8Constants.DateTimeConstantAttribute, reader))
						{
							CustomAttributeData customAttributeData = item.ToCustomAttributeData(module);
							IList<CustomAttributeTypedArgument> constructorArguments = customAttributeData.ConstructorArguments;
							if (constructorArguments.Count != 1)
							{
								return false;
							}
							CoreTypes allFoundCoreTypes = module.Loader.GetAllFoundCoreTypes();
							if (constructorArguments[0].ArgumentType != allFoundCoreTypes[CoreType.Int64])
							{
								return false;
							}
							long ticks = (long)constructorArguments[0].Value;
							rawDefaultValue = new DateTimeConstantAttribute(ticks).Value;
							return true;
						}
						if (handle.TypeMatchesNameAndNamespace(Utf8Constants.SystemRuntimeCompilerServices, Utf8Constants.DecimalConstantAttribute, reader))
						{
							CustomAttributeData customAttributeData2 = item.ToCustomAttributeData(module);
							IList<CustomAttributeTypedArgument> constructorArguments2 = customAttributeData2.ConstructorArguments;
							if (constructorArguments2.Count != 5)
							{
								return false;
							}
							CoreTypes allFoundCoreTypes2 = module.Loader.GetAllFoundCoreTypes();
							if (constructorArguments2[0].ArgumentType != allFoundCoreTypes2[CoreType.Byte] || constructorArguments2[1].ArgumentType != allFoundCoreTypes2[CoreType.Byte])
							{
								return false;
							}
							byte scale = (byte)constructorArguments2[0].Value;
							byte sign = (byte)constructorArguments2[1].Value;
							if (constructorArguments2[2].ArgumentType == allFoundCoreTypes2[CoreType.Int32] && constructorArguments2[3].ArgumentType == allFoundCoreTypes2[CoreType.Int32] && constructorArguments2[4].ArgumentType == allFoundCoreTypes2[CoreType.Int32])
							{
								int hi = (int)constructorArguments2[2].Value;
								int mid = (int)constructorArguments2[3].Value;
								int low = (int)constructorArguments2[4].Value;
								rawDefaultValue = new DecimalConstantAttribute(scale, sign, hi, mid, low).Value;
								return true;
							}
							if (constructorArguments2[2].ArgumentType == allFoundCoreTypes2[CoreType.UInt32] && constructorArguments2[3].ArgumentType == allFoundCoreTypes2[CoreType.UInt32] && constructorArguments2[4].ArgumentType == allFoundCoreTypes2[CoreType.UInt32])
							{
								uint hi2 = (uint)constructorArguments2[2].Value;
								uint mid2 = (uint)constructorArguments2[3].Value;
								uint low2 = (uint)constructorArguments2[4].Value;
								rawDefaultValue = new DecimalConstantAttribute(scale, sign, hi2, mid2, low2).Value;
								return true;
							}
							return false;
						}
					}
					return false;
				}
			}

			/// <summary>
			/// RoTypes that return true for IsTypeDefinition and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaDefinitionType : RoDefinitionType
			{
				private readonly EcmaModule _module;

				private readonly TypeDefinitionHandle _handle;

				private volatile RoType[] _lazyGenericParameters;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly TypeDefinition _neverAccessThisExceptThroughTypeDefinitionProperty;

				public sealed override int MetadataToken => _handle.GetToken();

				public sealed override bool IsGenericTypeDefinition => GetGenericParameterCount() != 0;

				private new MetadataLoadContext Loader => _module.Loader;

				private MetadataReader Reader => GetEcmaModule().Reader;

				private ref readonly TypeDefinition TypeDefinition
				{
					get
					{
						Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughTypeDefinitionProperty;
					}
				}

				internal sealed override IEnumerable<ConstructorInfo> SpecializeConstructors(NameFilter filter, RoInstantiationProviderType declaringType)
				{
					MetadataReader reader = Reader;
					foreach (MethodDefinitionHandle method2 in TypeDefinition.GetMethods())
					{
						MethodDefinition method = method2.GetMethodDefinition(reader);
						if ((filter == null || filter.Matches(method.Name, reader)) && method.IsConstructor(reader))
						{
							yield return new RoDefinitionConstructor<EcmaMethodDecoder>(declaringType, new EcmaMethodDecoder(method2, GetEcmaModule()));
						}
					}
				}

				internal sealed override IEnumerable<MethodInfo> SpecializeMethods(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
				{
					MetadataReader reader = Reader;
					foreach (MethodDefinitionHandle method2 in TypeDefinition.GetMethods())
					{
						MethodDefinition method = method2.GetMethodDefinition(reader);
						if ((filter == null || filter.Matches(method.Name, reader)) && !method.IsConstructor(reader))
						{
							yield return new RoDefinitionMethod<EcmaMethodDecoder>(declaringType, reflectedType, new EcmaMethodDecoder(method2, GetEcmaModule()));
						}
					}
				}

				internal sealed override IEnumerable<EventInfo> SpecializeEvents(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
				{
					MetadataReader reader = Reader;
					foreach (EventDefinitionHandle @event in TypeDefinition.GetEvents())
					{
						if (filter == null || filter.Matches(@event.GetEventDefinition(reader).Name, reader))
						{
							yield return new EcmaEvent(declaringType, @event, reflectedType);
						}
					}
				}

				internal sealed override IEnumerable<FieldInfo> SpecializeFields(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
				{
					MetadataReader reader = Reader;
					foreach (FieldDefinitionHandle field in TypeDefinition.GetFields())
					{
						if (filter == null || filter.Matches(field.GetFieldDefinition(reader).Name, reader))
						{
							yield return new EcmaField(declaringType, field, reflectedType);
						}
					}
				}

				internal sealed override IEnumerable<PropertyInfo> SpecializeProperties(NameFilter filter, Type reflectedType, RoInstantiationProviderType declaringType)
				{
					MetadataReader reader = Reader;
					foreach (PropertyDefinitionHandle property in TypeDefinition.GetProperties())
					{
						if (filter == null || filter.Matches(property.GetPropertyDefinition(reader).Name, reader))
						{
							yield return new EcmaProperty(declaringType, property, reflectedType);
						}
					}
				}

				internal sealed override IEnumerable<RoType> GetNestedTypesCore(NameFilter filter)
				{
					MetadataReader reader = Reader;
					ImmutableArray<TypeDefinitionHandle>.Enumerator enumerator = TypeDefinition.GetNestedTypes().GetEnumerator();
					while (enumerator.MoveNext())
					{
						TypeDefinitionHandle current = enumerator.Current;
						TypeDefinition typeDefinition = current.GetTypeDefinition(reader);
						if (filter == null || filter.Matches(typeDefinition.Name, reader))
						{
							yield return current.ResolveTypeDef(GetEcmaModule());
						}
					}
				}

				internal sealed override RoDefinitionType GetNestedTypeCore(ReadOnlySpan<byte> utf8Name)
				{
					//IL_0017: Unknown result type (might be due to invalid IL or missing references)
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					RoDefinitionType roDefinitionType = null;
					MetadataReader reader = Reader;
					ImmutableArray<TypeDefinitionHandle>.Enumerator enumerator = TypeDefinition.GetNestedTypes().GetEnumerator();
					while (enumerator.MoveNext())
					{
						TypeDefinitionHandle current = enumerator.Current;
						if (MetadataExtensions.Equals(current.GetTypeDefinition(reader).Name, utf8Name, reader))
						{
							if (roDefinitionType != null)
							{
								throw new AmbiguousMatchException();
							}
							roDefinitionType = current.ResolveTypeDef(GetEcmaModule());
						}
					}
					return roDefinitionType;
				}

				internal EcmaDefinitionType(TypeDefinitionHandle handle, EcmaModule module)
				{
					_module = module;
					_handle = handle;
					_neverAccessThisExceptThroughTypeDefinitionProperty = handle.GetTypeDefinition(Reader);
				}

				internal sealed override RoModule GetRoModule()
				{
					return _module;
				}

				internal EcmaModule GetEcmaModule()
				{
					return _module;
				}

				protected sealed override RoType ComputeDeclaringType()
				{
					if (!TypeDefinition.IsNested)
					{
						return null;
					}
					return TypeDefinition.GetDeclaringType().ResolveTypeDef(GetEcmaModule());
				}

				protected sealed override string ComputeName()
				{
					return TypeDefinition.Name.GetString(Reader).EscapeTypeNameIdentifier();
				}

				protected sealed override string ComputeNamespace()
				{
					Type declaringType = DeclaringType;
					if (declaringType != null)
					{
						return declaringType.Namespace;
					}
					return TypeDefinition.Namespace.GetStringOrNull(Reader)?.EscapeTypeNameIdentifier();
				}

				protected sealed override TypeAttributes ComputeAttributeFlags()
				{
					return TypeDefinition.Attributes;
				}

				internal sealed override RoType SpecializeBaseType(RoType[] instantiation)
				{
					EntityHandle baseType = TypeDefinition.BaseType;
					if (baseType.IsNil)
					{
						return null;
					}
					EntityHandle handle = baseType;
					EcmaModule ecmaModule = GetEcmaModule();
					TypeContext typeContext = instantiation.ToTypeContext();
					return handle.ResolveTypeDefRefOrSpec(ecmaModule, in typeContext);
				}

				internal sealed override IEnumerable<RoType> SpecializeInterfaces(RoType[] instantiation)
				{
					MetadataReader reader = Reader;
					EcmaModule module = GetEcmaModule();
					TypeContext typeContext = instantiation.ToTypeContext();
					foreach (InterfaceImplementationHandle interfaceImplementation in TypeDefinition.GetInterfaceImplementations())
					{
						yield return interfaceImplementation.GetInterfaceImplementation(reader).Interface.ResolveTypeDefRefOrSpec(module, in typeContext);
					}
				}

				protected sealed override IEnumerable<CustomAttributeData> GetTrueCustomAttributes()
				{
					return TypeDefinition.GetCustomAttributes().ToTrueCustomAttributes(GetEcmaModule());
				}

				internal sealed override bool IsCustomAttributeDefined(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
				{
					return TypeDefinition.GetCustomAttributes().IsCustomAttributeDefined(ns, name, GetEcmaModule());
				}

				internal sealed override CustomAttributeData TryFindCustomAttribute(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
				{
					return TypeDefinition.GetCustomAttributes().TryFindCustomAttribute(ns, name, GetEcmaModule());
				}

				internal sealed override int GetGenericParameterCount()
				{
					return GetGenericTypeParametersNoCopy().Length;
				}

				internal sealed override RoType[] GetGenericTypeParametersNoCopy()
				{
					return _lazyGenericParameters ?? (_lazyGenericParameters = ComputeGenericTypeParameters());
				}

				private RoType[] ComputeGenericTypeParameters()
				{
					EcmaModule ecmaModule = GetEcmaModule();
					GenericParameterHandleCollection genericParameters = TypeDefinition.GetGenericParameters();
					if (genericParameters.Count == 0)
					{
						return Array.Empty<RoType>();
					}
					RoType[] array = new RoType[genericParameters.Count];
					foreach (GenericParameterHandle item in genericParameters)
					{
						RoType roType = item.ResolveGenericParameter(ecmaModule);
						array[roType.GenericParameterPosition] = roType;
					}
					return array;
				}

				protected internal sealed override RoType ComputeEnumUnderlyingType()
				{
					if (!IsEnum)
					{
						throw new ArgumentException(MDCFR.Properties.Resources.Arg_MustBeEnum);
					}
					MetadataReader reader = Reader;
					TypeContext genericContext = Instantiation.ToTypeContext();
					RoType roType = null;
					foreach (FieldDefinitionHandle field in TypeDefinition.GetFields())
					{
						FieldDefinition fieldDefinition = field.GetFieldDefinition(reader);
						if ((fieldDefinition.Attributes & FieldAttributes.Static) == 0)
						{
							if (roType != null)
							{
								throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidEnum);
							}
							roType = fieldDefinition.DecodeSignature(GetEcmaModule(), genericContext);
						}
					}
					if (roType == null)
					{
						throw new ArgumentException(MDCFR.Properties.Resources.Argument_InvalidEnum);
					}
					return roType;
				}

				protected sealed override void GetPackSizeAndSize(out int packSize, out int size)
				{
					TypeLayout layout = TypeDefinition.GetLayout();
					packSize = layout.PackingSize;
					size = layout.Size;
				}

				internal sealed override bool IsTypeNameEqual(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
				{
					MetadataReader reader = Reader;
					TypeDefinition typeDefinition = TypeDefinition;
					if (MetadataExtensions.Equals(typeDefinition.Name, name, reader))
					{
						return MetadataExtensions.Equals(typeDefinition.Namespace, ns, reader);
					}
					return false;
				}
			}

			/// <summary>
			/// Base class for all EventInfo objects created by a MetadataLoadContext and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaEvent : RoEvent
			{
				private readonly EcmaModule _module;

				private readonly EventDefinitionHandle _handle;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly EventDefinition _neverAccessThisExceptThroughEventDefinitionProperty;

				public sealed override IEnumerable<CustomAttributeData> CustomAttributes => EventDefinition.GetCustomAttributes().ToTrueCustomAttributes(_module);

				public sealed override int MetadataToken => _handle.GetToken();

				private MetadataReader Reader => _module.Reader;

				private MetadataLoadContext Loader => GetRoModule().Loader;

				private ref readonly EventDefinition EventDefinition
				{
					get
					{
						Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughEventDefinitionProperty;
					}
				}

				internal EcmaEvent(RoInstantiationProviderType declaringType, EventDefinitionHandle handle, Type reflectedType)
					: base(declaringType, reflectedType)
				{
					_handle = handle;
					_module = (EcmaModule)declaringType.Module;
					_neverAccessThisExceptThroughEventDefinitionProperty = handle.GetEventDefinition(Reader);
				}

				internal sealed override RoModule GetRoModule()
				{
					return _module;
				}

				public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
				{
					if (!(obj is EcmaEvent ecmaEvent))
					{
						return false;
					}
					if (_handle != ecmaEvent._handle)
					{
						return false;
					}
					if (DeclaringType != ecmaEvent.DeclaringType)
					{
						return false;
					}
					if (ReflectedType != ecmaEvent.ReflectedType)
					{
						return false;
					}
					return true;
				}

				public sealed override int GetHashCode()
				{
					return _handle.GetHashCode() ^ DeclaringType.GetHashCode();
				}

				protected sealed override string ComputeName()
				{
					return EventDefinition.Name.GetString(Reader);
				}

				protected sealed override EventAttributes ComputeAttributes()
				{
					return EventDefinition.Attributes;
				}

				protected sealed override Type ComputeEventHandlerType()
				{
					EntityHandle type = EventDefinition.Type;
					EcmaModule module = _module;
					TypeContext typeContext = base.TypeContext;
					return type.ResolveTypeDefRefOrSpec(module, in typeContext);
				}

				public sealed override MethodInfo[] GetOtherMethods(bool nonPublic)
				{
					MetadataReader reader = Reader;
					System.Collections.Immutable.ImmutableArray<MethodDefinitionHandle> others = EventDefinition.GetAccessors().Others;
					int length = others.Length;
					List<MethodInfo> list = new List<MethodInfo>(length);
					for (int i = 0; i < length; i++)
					{
						MethodDefinition methodDefinition = others[i].GetMethodDefinition(reader);
						if (nonPublic || (methodDefinition.Attributes & MethodAttributes.MemberAccessMask) == MethodAttributes.Public)
						{
							MethodInfo item = others[i].ToMethod(GetRoDeclaringType(), GetRoDeclaringType());
							list.Add(item);
						}
					}
					return list.ToArray();
				}

				public sealed override string ToString()
				{
					string disposedString = Loader.GetDisposedString();
					if (disposedString != null)
					{
						return disposedString;
					}
					EntityHandle type = EventDefinition.Type;
					TypeContext typeContext = base.TypeContext;
					return type.ToTypeString(in typeContext, Reader) + " " + Name;
				}

				protected sealed override RoMethod ComputeEventAddMethod()
				{
					return EventDefinition.GetAccessors().Adder.ToMethodOrNull(GetRoDeclaringType(), ReflectedType);
				}

				protected sealed override RoMethod ComputeEventRemoveMethod()
				{
					return EventDefinition.GetAccessors().Remover.ToMethodOrNull(GetRoDeclaringType(), ReflectedType);
				}

				protected sealed override RoMethod ComputeEventRaiseMethod()
				{
					return EventDefinition.GetAccessors().Raiser.ToMethodOrNull(GetRoDeclaringType(), ReflectedType);
				}
			}

			/// <summary>
			/// Base class for all RoParameter's returned by MethodBase.GetParameters() that have an entry in the Param table
			/// and get their metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaFatMethodParameter : RoFatMethodParameter
			{
				private readonly EcmaModule _module;

				private readonly ParameterHandle _handle;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly Parameter _neverAccessThisExceptThroughParameterProperty;

				public sealed override int MetadataToken => _handle.GetToken();

				public sealed override bool HasDefaultValue
				{
					get
					{
						object rawDefaultValue;
						return TryGetRawDefaultValue(out rawDefaultValue);
					}
				}

				public sealed override object RawDefaultValue
				{
					get
					{
						if (TryGetRawDefaultValue(out var rawDefaultValue))
						{
							return rawDefaultValue;
						}
						if (!base.IsOptional)
						{
							return DBNull.Value;
						}
						return Missing.Value;
					}
				}

				private MetadataReader Reader => GetEcmaModule().Reader;

				private MetadataLoadContext Loader => GetEcmaModule().Loader;

				private ref readonly Parameter Parameter
				{
					get
					{
						Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughParameterProperty;
					}
				}

				internal EcmaFatMethodParameter(IRoMethodBase roMethodBase, int position, Type parameterType, ParameterHandle handle)
					: base(roMethodBase, position, parameterType)
				{
					_handle = handle;
					_module = (EcmaModule)roMethodBase.MethodBase.Module;
					_neverAccessThisExceptThroughParameterProperty = handle.GetParameter(Reader);
				}

				protected sealed override string ComputeName()
				{
					return Parameter.Name.GetStringOrNull(Reader);
				}

				protected sealed override ParameterAttributes ComputeAttributes()
				{
					return Parameter.Attributes;
				}

				protected sealed override IEnumerable<CustomAttributeData> GetTrueCustomAttributes()
				{
					return Parameter.GetCustomAttributes().ToTrueCustomAttributes(GetEcmaModule());
				}

				private bool TryGetRawDefaultValue(out object rawDefaultValue)
				{
					MetadataReader reader = Reader;
					ConstantHandle defaultValue = Parameter.GetDefaultValue();
					if (!defaultValue.IsNil)
					{
						rawDefaultValue = defaultValue.ToRawObject(reader);
						return true;
					}
					return Parameter.GetCustomAttributes().TryFindRawDefaultValueFromCustomAttributes(GetEcmaModule(), out rawDefaultValue);
				}

				protected sealed override MarshalAsAttribute ComputeMarshalAsAttribute()
				{
					return Parameter.GetMarshallingDescriptor().ToMarshalAsAttribute(GetEcmaModule());
				}

				private EcmaModule GetEcmaModule()
				{
					return _module;
				}
			}

			/// <summary>
			/// Base class for all FieldInfo objects created by a MetadataLoadContext and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaField : RoField
			{
				private readonly EcmaModule _module;

				private readonly FieldDefinitionHandle _handle;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly FieldDefinition _neverAccessThisExceptThroughFieldDefinitionProperty;

				public sealed override int MetadataToken => _handle.GetToken();

				private MetadataReader Reader => _module.Reader;

				private MetadataLoadContext Loader => GetRoModule().Loader;

				private ref readonly FieldDefinition FieldDefinition
				{
					get
					{
						Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughFieldDefinitionProperty;
					}
				}

				internal EcmaField(RoInstantiationProviderType declaringType, FieldDefinitionHandle handle, Type reflectedType)
					: base(declaringType, reflectedType)
				{
					_handle = handle;
					_module = (EcmaModule)declaringType.Module;
					_neverAccessThisExceptThroughFieldDefinitionProperty = handle.GetFieldDefinition(Reader);
				}

				internal sealed override RoModule GetRoModule()
				{
					return _module;
				}

				protected sealed override IEnumerable<CustomAttributeData> GetTrueCustomAttributes()
				{
					return FieldDefinition.GetCustomAttributes().ToTrueCustomAttributes(_module);
				}

				protected sealed override int GetExplicitFieldOffset()
				{
					return FieldDefinition.GetOffset();
				}

				protected sealed override MarshalAsAttribute ComputeMarshalAsAttribute()
				{
					return FieldDefinition.GetMarshallingDescriptor().ToMarshalAsAttribute(_module);
				}

				public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
				{
					if (!(obj is EcmaField ecmaField))
					{
						return false;
					}
					if (_handle != ecmaField._handle)
					{
						return false;
					}
					if (DeclaringType != ecmaField.DeclaringType)
					{
						return false;
					}
					if (ReflectedType != ecmaField.ReflectedType)
					{
						return false;
					}
					return true;
				}

				public sealed override int GetHashCode()
				{
					return _handle.GetHashCode() ^ DeclaringType.GetHashCode();
				}

				protected sealed override string ComputeName()
				{
					return FieldDefinition.Name.GetString(Reader);
				}

				protected sealed override FieldAttributes ComputeAttributes()
				{
					return FieldDefinition.Attributes;
				}

				protected sealed override Type ComputeFieldType()
				{
					return FieldDefinition.DecodeSignature(_module, base.TypeContext);
				}

				public sealed override Type[] GetOptionalCustomModifiers()
				{
					return GetCustomModifiers(isRequired: false);
				}

				public sealed override Type[] GetRequiredCustomModifiers()
				{
					return GetCustomModifiers(isRequired: true);
				}

				private Type[] GetCustomModifiers(bool isRequired)
				{
					RoType type = FieldDefinition.DecodeSignature(new EcmaModifiedTypeProvider(_module), base.TypeContext);
					return type.ExtractCustomModifiers(isRequired);
				}

				protected sealed override object ComputeRawConstantValue()
				{
					return FieldDefinition.GetDefaultValue().ToRawObject(Reader);
				}

				public sealed override string ToString()
				{
					string disposedString = Loader.GetDisposedString();
					if (disposedString != null)
					{
						return disposedString;
					}
					return FieldDefinition.DecodeSignature(EcmaSignatureTypeProviderForToString.Instance, base.TypeContext) + " " + Name;
				}
			}

			/// <summary>
			/// RoTypes that return true for IsGenericMethodParameter and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaGenericMethodParameterType : EcmaGenericParameterType
			{
				private volatile RoMethod _lazyDeclaringMethod;

				public sealed override bool IsGenericTypeParameter => false;

				public sealed override bool IsGenericMethodParameter => true;

				public sealed override MethodBase DeclaringMethod => GetRoDeclaringMethod();

				protected sealed override TypeContext TypeContext => GetRoDeclaringMethod().TypeContext;

				internal EcmaGenericMethodParameterType(GenericParameterHandle handle, EcmaModule module)
					: base(handle, module)
				{
				}

				protected sealed override RoType ComputeDeclaringType()
				{
					return GetRoDeclaringMethod().GetRoDeclaringType();
				}

				private RoMethod GetRoDeclaringMethod()
				{
					return _lazyDeclaringMethod ?? (_lazyDeclaringMethod = ComputeDeclaringMethod());
				}

				private RoMethod ComputeDeclaringMethod()
				{
					MethodDefinitionHandle handle = (MethodDefinitionHandle)base.GenericParameter.Parent;
					EcmaModule ecmaModule = GetEcmaModule();
					TypeContext typeContext = default(TypeContext);
					return handle.ResolveMethod<RoMethod>(ecmaModule, in typeContext);
				}
			}

			/// <summary>
			/// RoTypes that return true for IsGenericParameter and get its metadata from a PEReader.
			/// </summary>
			internal abstract class EcmaGenericParameterType : RoGenericParameterType
			{
				private readonly EcmaModule _ecmaModule;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly GenericParameter _neverAccessThisExceptThroughGenericParameterProperty;

				public sealed override GenericParameterAttributes GenericParameterAttributes => GenericParameter.Attributes;

				public sealed override IEnumerable<CustomAttributeData> CustomAttributes => GenericParameter.GetCustomAttributes().ToTrueCustomAttributes(GetEcmaModule());

				public sealed override int MetadataToken => Handle.GetToken();

				public abstract override MethodBase DeclaringMethod { get; }

				internal GenericParameterHandle Handle { get; }

				internal MetadataReader Reader => GetEcmaModule().Reader;

				protected abstract TypeContext TypeContext { get; }

				protected ref readonly GenericParameter GenericParameter
				{
					get
					{
						base.Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughGenericParameterProperty;
					}
				}

				internal EcmaGenericParameterType(GenericParameterHandle handle, EcmaModule module)
				{
					Handle = handle;
					_ecmaModule = module;
					_neverAccessThisExceptThroughGenericParameterProperty = handle.GetGenericParameter(Reader);
				}

				internal sealed override RoModule GetRoModule()
				{
					return _ecmaModule;
				}

				protected sealed override int ComputePosition()
				{
					return GenericParameter.Index;
				}

				protected sealed override string ComputeName()
				{
					return GenericParameter.Name.GetString(Reader);
				}

				internal sealed override bool IsCustomAttributeDefined(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
				{
					return GenericParameter.GetCustomAttributes().IsCustomAttributeDefined(ns, name, GetEcmaModule());
				}

				internal sealed override CustomAttributeData TryFindCustomAttribute(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name)
				{
					return GenericParameter.GetCustomAttributes().TryFindCustomAttribute(ns, name, GetEcmaModule());
				}

				protected sealed override RoType[] ComputeGenericParameterConstraints()
				{
					MetadataReader reader = Reader;
					GenericParameterConstraintHandleCollection constraints = GenericParameter.GetConstraints();
					int count = constraints.Count;
					if (count == 0)
					{
						return Array.Empty<RoType>();
					}
					TypeContext typeContext = TypeContext;
					RoType[] array = new RoType[count];
					int num = 0;
					foreach (GenericParameterConstraintHandle item in constraints)
					{
						RoType roType = item.GetGenericParameterConstraint(reader).Type.ResolveTypeDefRefOrSpec(GetEcmaModule(), in typeContext);
						array[num++] = roType;
					}
					return array;
				}

				protected abstract override RoType ComputeDeclaringType();

				internal EcmaModule GetEcmaModule()
				{
					return _ecmaModule;
				}
			}

			/// <summary>
			/// RoTypes that return true for IsGenericTypeParameter and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaGenericTypeParameterType : EcmaGenericParameterType
			{
				public sealed override bool IsGenericTypeParameter => true;

				public sealed override bool IsGenericMethodParameter => false;

				public sealed override MethodBase DeclaringMethod => null;

				protected sealed override TypeContext TypeContext => ((RoInstantiationProviderType)GetRoDeclaringType()).Instantiation.ToTypeContext();

				internal EcmaGenericTypeParameterType(GenericParameterHandle handle, EcmaModule module)
					: base(handle, module)
				{
				}

				protected sealed override RoType ComputeDeclaringType()
				{
					TypeDefinitionHandle handle = (TypeDefinitionHandle)base.GenericParameter.Parent;
					return handle.ResolveTypeDef(GetEcmaModule());
				}
			}

			internal static class EcmaHelpers
			{
				/// <summary>
				/// Returns a RoAssemblyName corresponding to the assembly reference.
				/// </summary>
				public static RoAssemblyName ToRoAssemblyName(this AssemblyReferenceHandle h, MetadataReader reader)
				{
					AssemblyReference assemblyReference = h.GetAssemblyReference(reader);
					string @string = assemblyReference.Name.GetString(reader);
					Version version = assemblyReference.Version.AdjustForUnspecifiedVersionComponents();
					string stringOrNull = assemblyReference.Culture.GetStringOrNull(reader);
					byte[] array = assemblyReference.PublicKeyOrToken.GetBlobBytes(reader);
					AssemblyFlags flags = assemblyReference.Flags;
					AssemblyNameFlags flags2 = Helpers.ConvertAssemblyFlagsToAssemblyNameFlags(flags);
					if ((flags & AssemblyFlags.PublicKey) != 0)
					{
						array = array.ComputePublicKeyToken();
					}
					return new RoAssemblyName(@string, version, stringOrNull, array, flags2);
				}

				public static CoreType ToCoreType(this PrimitiveTypeCode typeCode)
				{
					return typeCode switch
					{
						PrimitiveTypeCode.Boolean => CoreType.Boolean, 
						PrimitiveTypeCode.Byte => CoreType.Byte, 
						PrimitiveTypeCode.Char => CoreType.Char, 
						PrimitiveTypeCode.Double => CoreType.Double, 
						PrimitiveTypeCode.Int16 => CoreType.Int16, 
						PrimitiveTypeCode.Int32 => CoreType.Int32, 
						PrimitiveTypeCode.Int64 => CoreType.Int64, 
						PrimitiveTypeCode.IntPtr => CoreType.IntPtr, 
						PrimitiveTypeCode.Object => CoreType.Object, 
						PrimitiveTypeCode.SByte => CoreType.SByte, 
						PrimitiveTypeCode.Single => CoreType.Single, 
						PrimitiveTypeCode.String => CoreType.String, 
						PrimitiveTypeCode.TypedReference => CoreType.TypedReference, 
						PrimitiveTypeCode.UInt16 => CoreType.UInt16, 
						PrimitiveTypeCode.UInt32 => CoreType.UInt32, 
						PrimitiveTypeCode.UInt64 => CoreType.UInt64, 
						PrimitiveTypeCode.UIntPtr => CoreType.UIntPtr, 
						PrimitiveTypeCode.Void => CoreType.Void, 
						_ => CoreType.Void, 
					};
				}

				public static PrimitiveTypeCode GetEnumUnderlyingPrimitiveTypeCode(this Type enumType, MetadataLoadContext loader)
				{
					Type enumUnderlyingType = enumType.GetEnumUnderlyingType();
					CoreTypes allFoundCoreTypes = loader.GetAllFoundCoreTypes();
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.Boolean])
					{
						return PrimitiveTypeCode.Boolean;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.Char])
					{
						return PrimitiveTypeCode.Char;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.Byte])
					{
						return PrimitiveTypeCode.Byte;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.Int16])
					{
						return PrimitiveTypeCode.Int16;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.Int32])
					{
						return PrimitiveTypeCode.Int32;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.Int64])
					{
						return PrimitiveTypeCode.Int64;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.IntPtr])
					{
						return PrimitiveTypeCode.IntPtr;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.SByte])
					{
						return PrimitiveTypeCode.SByte;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.UInt16])
					{
						return PrimitiveTypeCode.UInt16;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.UInt32])
					{
						return PrimitiveTypeCode.UInt32;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.UInt64])
					{
						return PrimitiveTypeCode.UInt64;
					}
					if (enumUnderlyingType == allFoundCoreTypes[CoreType.UIntPtr])
					{
						return PrimitiveTypeCode.UIntPtr;
					}
					throw new BadImageFormatException(System.SR.Format(MDCFR.Properties.Resources.UnexpectedUnderlyingEnumType, enumType, enumUnderlyingType));
				}

				public static System.Configuration.Assemblies.AssemblyHashAlgorithm ToConfigurationAssemblyHashAlgorithm(this AssemblyHashAlgorithm srmHash)
				{
					return (System.Configuration.Assemblies.AssemblyHashAlgorithm)srmHash;
				}

				public static ExceptionHandlingClauseOptions ToExceptionHandlingClauseOptions(this ExceptionRegionKind kind)
				{
					return (ExceptionHandlingClauseOptions)kind;
				}

				public static AssemblyNameFlags ToAssemblyNameFlags(this AssemblyFlags flags)
				{
					return (AssemblyNameFlags)flags;
				}

				public static bool IsConstructor(this in MethodDefinition method, MetadataReader reader)
				{
					if ((method.Attributes & (MethodAttributes.SpecialName | MethodAttributes.RTSpecialName)) != (MethodAttributes.SpecialName | MethodAttributes.RTSpecialName))
					{
						return false;
					}
					MetadataStringComparer stringComparer = reader.StringComparer;
					StringHandle name = method.Name;
					if (!stringComparer.Equals(name, ConstructorInfo.ConstructorName))
					{
						return stringComparer.Equals(name, ConstructorInfo.TypeConstructorName);
					}
					return true;
				}

				public unsafe static ReadOnlySpan<byte> AsReadOnlySpan(this StringHandle handle, MetadataReader reader)
				{
					BlobReader blobReader = handle.GetBlobReader(reader);
					return new ReadOnlySpan<byte>(blobReader.CurrentPointer, blobReader.Length);
				}

				public static RoMethod ToMethodOrNull(this MethodDefinitionHandle handle, RoInstantiationProviderType declaringType, Type reflectedType)
				{
					if (handle.IsNil)
					{
						return null;
					}
					return handle.ToMethod(declaringType, reflectedType);
				}

				public static RoMethod ToMethod(this MethodDefinitionHandle handle, RoInstantiationProviderType declaringType, Type reflectedType)
				{
					return new RoDefinitionMethod<EcmaMethodDecoder>(declaringType, reflectedType, new EcmaMethodDecoder(handle, (EcmaModule)declaringType.Module));
				}
			}

			internal sealed class EcmaMethodBody : RoMethodBody
			{
				private readonly IRoMethodBase _roMethodBase;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly MethodBodyBlock _neverAccessThisExceptThroughBlockProperty;

				public sealed override bool InitLocals => Block.LocalVariablesInitialized;

				public sealed override int MaxStackSize => Block.MaxStack;

				public sealed override int LocalSignatureMetadataToken => Block.LocalSignature.GetToken();

				public sealed override IList<LocalVariableInfo> LocalVariables
				{
					get
					{
						MetadataReader reader = Reader;
						EcmaPinnedTypeProvider provider = new EcmaPinnedTypeProvider(GetEcmaModule());
						StandaloneSignatureHandle localSignature = Block.LocalSignature;
						if (localSignature.IsNil)
						{
							return Array.Empty<LocalVariableInfo>();
						}
						System.Collections.Immutable.ImmutableArray<RoType> immutableArray = localSignature.GetStandaloneSignature(reader).DecodeLocalSignature(provider, TypeContext);
						int length = immutableArray.Length;
						LocalVariableInfo[] array = new LocalVariableInfo[length];
						for (int i = 0; i < length; i++)
						{
							bool isPinned = false;
							RoType roType = immutableArray[i];
							if (roType is RoPinnedType)
							{
								isPinned = true;
								roType = roType.SkipTypeWrappers();
							}
							array[i] = new RoLocalVariableInfo(i, isPinned, roType);
						}
						return array.ToReadOnlyCollection();
					}
				}

				public sealed override IList<ExceptionHandlingClause> ExceptionHandlingClauses
				{
					get
					{
						System.Collections.Immutable.ImmutableArray<ExceptionRegion> exceptionRegions = Block.ExceptionRegions;
						int length = exceptionRegions.Length;
						ExceptionHandlingClause[] array = new ExceptionHandlingClause[length];
						for (int i = 0; i < length; i++)
						{
							EntityHandle catchType = exceptionRegions[i].CatchType;
							object obj;
							if (!catchType.IsNil)
							{
								EntityHandle handle = catchType;
								EcmaModule ecmaModule = GetEcmaModule();
								TypeContext typeContext = TypeContext;
								obj = handle.ResolveTypeDefRefOrSpec(ecmaModule, in typeContext);
							}
							else
							{
								obj = null;
							}
							RoType catchType2 = (RoType)obj;
							array[i] = new RoExceptionHandlingClause(catchType2, exceptionRegions[i].Kind.ToExceptionHandlingClauseOptions(), exceptionRegions[i].FilterOffset, exceptionRegions[i].TryOffset, exceptionRegions[i].TryLength, exceptionRegions[i].HandlerOffset, exceptionRegions[i].HandlerLength);
						}
						return array.ToReadOnlyCollection();
					}
				}

				private TypeContext TypeContext => _roMethodBase.TypeContext;

				private MetadataReader Reader => GetEcmaModule().Reader;

				private MetadataLoadContext Loader => GetEcmaModule().Loader;

				private ref readonly MethodBodyBlock Block
				{
					get
					{
						Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughBlockProperty;
					}
				}

				internal EcmaMethodBody(IRoMethodBase roMethodBase, MethodBodyBlock methodBodyBlock)
				{
					_roMethodBase = roMethodBase;
					_neverAccessThisExceptThroughBlockProperty = methodBodyBlock;
				}

				protected sealed override byte[] ComputeIL()
				{
					return Block.GetILBytes();
				}

				private EcmaModule GetEcmaModule()
				{
					return (EcmaModule)_roMethodBase.MethodBase.Module;
				}
			}

			internal readonly struct EcmaMethodDecoder : IMethodDecoder
			{
				private readonly MethodDefinitionHandle _handle;

				private readonly EcmaModule _module;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly MethodDefinition _neverAccessThisExceptThroughMethodDefinitionProperty;

				public int MetadataToken => _handle.GetToken();

				private MetadataReader Reader => _module.Reader;

				private MetadataLoadContext Loader => GetRoModule().Loader;

				private MethodDefinition MethodDefinition
				{
					get
					{
						Loader.DisposeCheck();
						return _neverAccessThisExceptThroughMethodDefinitionProperty;
					}
				}

				internal EcmaMethodDecoder(MethodDefinitionHandle handle, EcmaModule module)
				{
					this = default(EcmaMethodDecoder);
					_handle = handle;
					_module = module;
					_neverAccessThisExceptThroughMethodDefinitionProperty = handle.GetMethodDefinition(Reader);
				}

				public RoModule GetRoModule()
				{
					return _module;
				}

				public string ComputeName()
				{
					return MethodDefinition.Name.GetString(Reader);
				}

				public IEnumerable<CustomAttributeData> ComputeTrueCustomAttributes()
				{
					return MethodDefinition.GetCustomAttributes().ToTrueCustomAttributes(_module);
				}

				public int ComputeGenericParameterCount()
				{
					return MethodDefinition.GetGenericParameters().Count;
				}

				public RoType[] ComputeGenericArgumentsOrParameters()
				{
					GenericParameterHandleCollection genericParameters = MethodDefinition.GetGenericParameters();
					int count = genericParameters.Count;
					if (count == 0)
					{
						return Array.Empty<RoType>();
					}
					RoType[] array = new RoType[count];
					foreach (GenericParameterHandle item in genericParameters)
					{
						RoType roType = item.ResolveGenericParameter(_module);
						array[roType.GenericParameterPosition] = roType;
					}
					return array;
				}

				public MethodAttributes ComputeAttributes()
				{
					return MethodDefinition.Attributes;
				}

				public CallingConventions ComputeCallingConvention()
				{
					SignatureHeader signatureHeader = MethodDefinition.Signature.GetBlobReader(Reader).ReadSignatureHeader();
					CallingConventions callingConventions = ((signatureHeader.CallingConvention != SignatureCallingConvention.VarArgs) ? CallingConventions.Standard : CallingConventions.VarArgs);
					if (signatureHeader.IsInstance)
					{
						callingConventions |= CallingConventions.HasThis;
					}
					if (signatureHeader.HasExplicitThis)
					{
						callingConventions |= CallingConventions.ExplicitThis;
					}
					return callingConventions;
				}

				public MethodImplAttributes ComputeMethodImplementationFlags()
				{
					return MethodDefinition.ImplAttributes;
				}

				public MethodSig<RoParameter> SpecializeMethodSig(IRoMethodBase roMethodBase)
				{
					MetadataReader reader = Reader;
					MethodDefinition methodDefinition = MethodDefinition;
					MethodSignature<RoType> methodSignature = methodDefinition.DecodeSignature(_module, roMethodBase.TypeContext);
					int requiredParameterCount = methodSignature.RequiredParameterCount;
					MethodSig<RoParameter> methodSig = new MethodSig<RoParameter>(requiredParameterCount);
					foreach (ParameterHandle parameter in methodDefinition.GetParameters())
					{
						int num = parameter.GetParameter(reader).SequenceNumber - 1;
						Type parameterType = ((num == -1) ? methodSignature.ReturnType : methodSignature.ParameterTypes[num]);
						methodSig[num] = new EcmaFatMethodParameter(roMethodBase, num, parameterType, parameter);
					}
					for (int i = -1; i < requiredParameterCount; i++)
					{
						Type parameterType2 = ((i == -1) ? methodSignature.ReturnType : methodSignature.ParameterTypes[i]);
						MethodSig<RoParameter> methodSig2 = methodSig;
						int position = i;
						if (methodSig2[position] == null)
						{
							RoParameter roParameter2 = (methodSig2[position] = new RoThinMethodParameter(roMethodBase, i, parameterType2));
						}
					}
					return methodSig;
				}

				public MethodSig<RoType> SpecializeCustomModifiers(in TypeContext typeContext)
				{
					MethodSignature<RoType> methodSignature = MethodDefinition.DecodeSignature(new EcmaModifiedTypeProvider(_module), typeContext);
					int requiredParameterCount = methodSignature.RequiredParameterCount;
					MethodSig<RoType> methodSig = new MethodSig<RoType>(requiredParameterCount);
					for (int i = -1; i < requiredParameterCount; i++)
					{
						RoType value = ((i == -1) ? methodSignature.ReturnType : methodSignature.ParameterTypes[i]);
						methodSig[i] = value;
					}
					return methodSig;
				}

				public MethodSig<string> SpecializeMethodSigStrings(in TypeContext typeContext)
				{
					ISignatureTypeProvider<string, TypeContext> instance = EcmaSignatureTypeProviderForToString.Instance;
					MethodSignature<string> methodSignature = MethodDefinition.DecodeSignature(instance, typeContext);
					int length = methodSignature.ParameterTypes.Length;
					MethodSig<string> methodSig = new MethodSig<string>(length);
					methodSig[-1] = methodSignature.ReturnType;
					for (int i = 0; i < length; i++)
					{
						methodSig[i] = methodSignature.ParameterTypes[i];
					}
					return methodSig;
				}

				public MethodBody SpecializeMethodBody(IRoMethodBase owner)
				{
					int relativeVirtualAddress = MethodDefinition.RelativeVirtualAddress;
					if (relativeVirtualAddress == 0)
					{
						return null;
					}
					return new EcmaMethodBody(owner, ((EcmaModule)owner.MethodBase.Module).PEReader.GetMethodBody(relativeVirtualAddress));
				}

				public DllImportAttribute ComputeDllImportAttribute()
				{
					MetadataReader reader = Reader;
					MethodImport import = MethodDefinition.GetImport();
					string @string = import.Module.GetModuleReference(reader).Name.GetString(reader);
					string string2 = import.Name.GetString(reader);
					MethodImportAttributes attributes = import.Attributes;
					CharSet charSet = (attributes & MethodImportAttributes.CharSetAuto) switch
					{
						MethodImportAttributes.CharSetAnsi => CharSet.Ansi, 
						MethodImportAttributes.CharSetAuto => CharSet.Auto, 
						MethodImportAttributes.CharSetUnicode => CharSet.Unicode, 
						_ => CharSet.None, 
					};
					CallingConvention callingConvention = (attributes & MethodImportAttributes.CallingConventionMask) switch
					{
						MethodImportAttributes.CallingConventionCDecl => CallingConvention.Cdecl, 
						MethodImportAttributes.CallingConventionFastCall => CallingConvention.FastCall, 
						MethodImportAttributes.CallingConventionStdCall => CallingConvention.StdCall, 
						MethodImportAttributes.CallingConventionThisCall => CallingConvention.ThisCall, 
						MethodImportAttributes.CallingConventionWinApi => CallingConvention.Winapi, 
						_ => throw new BadImageFormatException(), 
					};
					return new DllImportAttribute(@string)
					{
						EntryPoint = string2,
						ExactSpelling = ((attributes & MethodImportAttributes.ExactSpelling) != 0),
						CharSet = charSet,
						CallingConvention = callingConvention,
						PreserveSig = ((ComputeMethodImplementationFlags() & MethodImplAttributes.PreserveSig) != 0),
						SetLastError = ((attributes & MethodImportAttributes.SetLastError) != 0),
						BestFitMapping = ((attributes & MethodImportAttributes.BestFitMappingMask) == MethodImportAttributes.BestFitMappingEnable),
						ThrowOnUnmappableChar = ((attributes & MethodImportAttributes.ThrowOnUnmappableCharMask) == MethodImportAttributes.ThrowOnUnmappableCharEnable)
					};
				}

				MethodSig<RoType> IMethodDecoder.SpecializeCustomModifiers(in TypeContext typeContext)
				{
					return SpecializeCustomModifiers(in typeContext);
				}

				MethodSig<string> IMethodDecoder.SpecializeMethodSigStrings(in TypeContext typeContext)
				{
					return SpecializeMethodSigStrings(in typeContext);
				}
			}

			internal sealed class EcmaModifiedTypeProvider : EcmaWrappedTypeProvider
			{
				internal EcmaModifiedTypeProvider(EcmaModule module) : base(module) { }

				public sealed override RoType GetModifiedType(RoType modifier, RoType unmodifiedType, bool isRequired)
				{
					return new RoModifiedType(modifier.SkipTypeWrappers(), unmodifiedType, isRequired);
				}

				public sealed override RoType GetPinnedType(RoType elementType) { return elementType; }
			}

			/// <summary>
			/// Base class for all Module objects created by a MetadataLoadContext and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaModule : RoModule, ISignatureTypeProvider<RoType, TypeContext>, IConstructedTypeProvider<RoType>, ISZArrayTypeProvider<RoType>, ISimpleTypeProvider<RoType>, ICustomAttributeTypeProvider<RoType>
			{
				private const int ModuleTypeToken = 33554433;

				private readonly EcmaAssembly _assembly;

				private readonly GuardedPEReader _guardedPEReader;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly ModuleDefinition _neverAccessThisExceptThroughModuleDefinitionProperty;

				private volatile MetadataTable<EcmaDefinitionType, EcmaModule> _lazyTypeDefTable;

				private bool _typeDefTableFullyFilled;

				private volatile MetadataTable<RoDefinitionType, EcmaModule> _lazyTypeRefTable;

				private volatile MetadataTable<EcmaGenericParameterType, EcmaModule> _lazyGenericParamTable;

				private volatile MetadataTable<RoAssembly, EcmaModule> _lazyAssemblyRefTable;

				public sealed override int MDStreamVersion
				{
					get
					{
						throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_MDStreamVersion);
					}
				}

				public sealed override int MetadataToken => 1;

				public sealed override Guid ModuleVersionId => ModuleDefinition.Mvid.GetGuid(Reader);

				public sealed override string ScopeName => ModuleDefinition.Name.GetString(Reader);

				public sealed override IEnumerable<CustomAttributeData> CustomAttributes => ModuleDefinition.GetCustomAttributes().ToTrueCustomAttributes(this);

				internal PEReader PEReader => _guardedPEReader.PEReader;

				internal MetadataReader Reader => _guardedPEReader.Reader;

				private ref readonly ModuleDefinition ModuleDefinition
				{
					get
					{
						base.Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughModuleDefinitionProperty;
					}
				}

				internal MetadataTable<EcmaDefinitionType, EcmaModule> TypeDefTable => _lazyTypeDefTable ?? Interlocked.CompareExchange(ref _lazyTypeDefTable, CreateTable<EcmaDefinitionType>(TableIndex.TypeDef), null) ?? _lazyTypeDefTable;

				internal MetadataTable<RoDefinitionType, EcmaModule> TypeRefTable => _lazyTypeRefTable ?? Interlocked.CompareExchange(ref _lazyTypeRefTable, CreateTable<RoDefinitionType>(TableIndex.TypeRef), null) ?? _lazyTypeRefTable;

				internal MetadataTable<EcmaGenericParameterType, EcmaModule> GenericParamTable => _lazyGenericParamTable ?? Interlocked.CompareExchange(ref _lazyGenericParamTable, CreateTable<EcmaGenericParameterType>(TableIndex.GenericParam), null) ?? _lazyGenericParamTable;

				internal MetadataTable<RoAssembly, EcmaModule> AssemblyRefTable => _lazyAssemblyRefTable ?? Interlocked.CompareExchange(ref _lazyAssemblyRefTable, CreateTable<RoAssembly>(TableIndex.AssemblyRef), null) ?? _lazyAssemblyRefTable;

				internal EcmaModule(EcmaAssembly assembly, string fullyQualifiedName, PEReader peReader, MetadataReader reader)
					: base(fullyQualifiedName)
				{
					_assembly = assembly;
					_guardedPEReader = new GuardedPEReader(assembly.Loader, peReader, reader);
					_neverAccessThisExceptThroughModuleDefinitionProperty = reader.GetModuleDefinition();
				}

				internal sealed override RoAssembly GetRoAssembly()
				{
					return _assembly;
				}

				internal EcmaAssembly GetEcmaAssembly()
				{
					return _assembly;
				}

				public sealed override bool IsResource()
				{
					return false;
				}

				internal MethodInfo ComputeEntryPoint(bool fileRefEntryPointAllowed)
				{
					PEHeaders pEHeaders = PEReader.PEHeaders;
					CorHeader corHeader = pEHeaders.CorHeader;
					if ((corHeader.Flags & CorFlags.NativeEntryPoint) != 0)
					{
						return null;
					}
					int entryPointTokenOrRelativeVirtualAddress = corHeader.EntryPointTokenOrRelativeVirtualAddress;
					Handle handle = entryPointTokenOrRelativeVirtualAddress.ToHandle();
					if (handle.IsNil)
					{
						return null;
					}
					switch (handle.Kind)
					{
					case HandleKind.MethodDefinition:
					{
						MethodDefinitionHandle handle2 = (MethodDefinitionHandle)handle;
						TypeContext typeContext = default(TypeContext);
						return handle2.ResolveMethod<MethodInfo>(this, in typeContext);
					}
					case HandleKind.AssemblyFile:
					{
						if (!fileRefEntryPointAllowed)
						{
							throw new BadImageFormatException();
						}
						MetadataReader reader = Reader;
						string @string = ((AssemblyFileHandle)handle).GetAssemblyFile(reader).Name.GetString(reader);
						EcmaModule ecmaModule = (EcmaModule)Assembly.GetModule(@string);
						return ecmaModule.ComputeEntryPoint(fileRefEntryPointAllowed: false);
					}
					default:
						throw new BadImageFormatException();
					}
				}

				public sealed override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
				{
					PEHeaders pEHeaders = PEReader.PEHeaders;
					PEMagic magic = pEHeaders.PEHeader.Magic;
					Machine machine2 = pEHeaders.CoffHeader.Machine;
					CorFlags flags = pEHeaders.CorHeader.Flags;
					peKind = PortableExecutableKinds.NotAPortableExecutableImage;
					if ((flags & CorFlags.ILOnly) != 0)
					{
						peKind |= PortableExecutableKinds.ILOnly;
					}
					if ((flags & CorFlags.Prefers32Bit) != 0)
					{
						peKind |= PortableExecutableKinds.Preferred32Bit;
					}
					else if ((flags & CorFlags.Requires32Bit) != 0)
					{
						peKind |= PortableExecutableKinds.Required32Bit;
					}
					if (magic == PEMagic.PE32Plus)
					{
						peKind |= PortableExecutableKinds.PE32Plus;
					}
					machine = (ImageFileMachine)machine2;
				}

				public sealed override FieldInfo GetField(string name, BindingFlags bindingAttr)
				{
					return GetModuleType().GetField(name, bindingAttr);
				}

				public sealed override FieldInfo[] GetFields(BindingFlags bindingFlags)
				{
					return GetModuleType().GetFields(bindingFlags);
				}

				public sealed override MethodInfo[] GetMethods(BindingFlags bindingFlags)
				{
					return GetModuleType().GetMethods(bindingFlags);
				}

				protected sealed override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
				{
					return GetModuleType().InternalGetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
				}

				private RoType GetModuleType()
				{
					return 33554433.ToTypeDefinitionHandle().ResolveTypeDef(this);
				}

				public sealed override Type[] GetTypes()
				{
					EnsureTypeDefTableFullyFilled();
					return TypeDefTable.ToArray<Type>(1);
				}

				internal sealed override IEnumerable<RoType> GetDefinedRoTypes()
				{
					EnsureTypeDefTableFullyFilled();
					return TypeDefTable.EnumerateValues(1);
				}

				/// <summary>
				/// Helper routine for the more general Module.GetType() family of apis. Also used in typeRef resolution.
				///
				/// Resolves top-level named types only. No nested types. No constructed types. The input name must not be escaped.
				///
				/// If a type is not contained or forwarded from the assembly, this method returns null (does not throw.)
				/// This supports the "throwOnError: false" behavior of Module.GetType(string, bool).
				/// </summary>
				[UnconditionalSuppressMessage("SingleFile", "IL3002:RequiresAssemblyFiles on FullyQualifiedName", Justification = "FullyQualifiedName is only used for exception message")]
				protected sealed override RoDefinitionType GetTypeCoreNoCache(ReadOnlySpan<byte> ns, ReadOnlySpan<byte> name, out Exception e)
				{
					MetadataReader reader = Reader;
					foreach (TypeDefinitionHandle typeDefinition2 in reader.TypeDefinitions)
					{
						TypeDefinition typeDefinition = typeDefinition2.GetTypeDefinition(reader);
						if (!typeDefinition.IsNested && MetadataExtensions.Equals(typeDefinition.Name, name, reader) && MetadataExtensions.Equals(typeDefinition.Namespace, ns, reader))
						{
							e = null;
							return typeDefinition2.ResolveTypeDef(this);
						}
					}
					foreach (ExportedTypeHandle exportedType2 in reader.ExportedTypes)
					{
						ExportedType exportedType = exportedType2.GetExportedType(reader);
						if (exportedType.IsForwarder)
						{
							EntityHandle implementation = exportedType.Implementation;
							if (implementation.Kind == HandleKind.AssemblyReference && MetadataExtensions.Equals(exportedType.Name, name, reader) && MetadataExtensions.Equals(exportedType.Namespace, ns, reader))
							{
								return ((AssemblyReferenceHandle)implementation).TryResolveAssembly(this, out e)?.GetTypeCore(ns, name, ignoreCase: false, out e);
							}
						}
					}
					e = new TypeLoadException(System.SR.Format(MDCFR.Properties.Resources.TypeNotFound, ns.ToUtf16().AppendTypeName(name.ToUtf16()), FullyQualifiedName));
					return null;
				}

				internal unsafe InternalManifestResourceInfo GetInternalManifestResourceInfo(string resourceName)
				{
					MetadataReader reader = Reader;
					InternalManifestResourceInfo result = default(InternalManifestResourceInfo);
					checked
					{
						foreach (ManifestResourceHandle manifestResource2 in reader.ManifestResources)
						{
							ManifestResource manifestResource = manifestResource2.GetManifestResource(reader);
							if (!MetadataExtensions.Equals(manifestResource.Name, resourceName, reader))
							{
								continue;
							}
							result.Found = true;
							if (manifestResource.Implementation.IsNil)
							{
								result.ResourceLocation = ResourceLocation.Embedded | ResourceLocation.ContainedInManifestFile;
								PEReader pEReader = _guardedPEReader.PEReader;
								PEMemoryBlock sectionData = pEReader.GetSectionData(pEReader.PEHeaders.CorHeader.ResourcesDirectory.RelativeVirtualAddress);
								BlobReader reader2 = sectionData.GetReader((int)manifestResource.Offset, sectionData.Length - (int)manifestResource.Offset);
								uint num = reader2.ReadUInt32();
								result.PointerToResource = reader2.CurrentPointer;
								if (num + 4u > reader2.Length)
								{
									throw new BadImageFormatException();
								}
								result.SizeOfResource = num;
							}
							else if (manifestResource.Implementation.Kind == HandleKind.AssemblyFile)
							{
								result.ResourceLocation = (ResourceLocation)0;
								AssemblyFile assemblyFile = ((AssemblyFileHandle)manifestResource.Implementation).GetAssemblyFile(reader);
								result.FileName = assemblyFile.Name.GetString(reader);
								if (assemblyFile.ContainsMetadata)
								{
									EcmaModule ecmaModule = (EcmaModule)Assembly.GetModule(result.FileName);
									if (ecmaModule == null)
									{
										throw new BadImageFormatException(System.SR.Format(MDCFR.Properties.Resources.ManifestResourceInfoReferencedBadModule, result.FileName));
									}
									result = ecmaModule.GetInternalManifestResourceInfo(resourceName);
								}
							}
							else if (manifestResource.Implementation.Kind == HandleKind.AssemblyReference)
							{
								result.ResourceLocation = ResourceLocation.ContainedInAnotherAssembly;
								RoAssemblyName refName = ((AssemblyReferenceHandle)manifestResource.Implementation).ToRoAssemblyName(reader);
								result.ReferencedAssembly = base.Loader.ResolveAssembly(refName);
							}
						}
						return result;
					}
				}

				private void EnsureTypeDefTableFullyFilled()
				{
					if (_typeDefTableFullyFilled)
					{
						return;
					}
					foreach (TypeDefinitionHandle typeDefinition in Reader.TypeDefinitions)
					{
						typeDefinition.ResolveTypeDef(this);
					}
					_typeDefTableFullyFilled = true;
				}

				private MetadataTable<T, EcmaModule> CreateTable<T>(TableIndex tableIndex) where T : class
				{
					return new MetadataTable<T, EcmaModule>(Reader.GetTableRowCount(tableIndex));
				}

				public RoType GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
				{
					return handle.ResolveTypeDef(this);
				}

				public RoType GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
				{
					return handle.ResolveTypeRef(this);
				}

				public RoType GetTypeFromSpecification(MetadataReader reader, TypeContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
				{
					return handle.ResolveTypeSpec(this, in genericContext);
				}

				public RoType GetSZArrayType(RoType elementType)
				{
					return elementType.GetUniqueArrayType();
				}

				public RoType GetArrayType(RoType elementType, ArrayShape shape)
				{
					return elementType.GetUniqueArrayType(shape.Rank);
				}

				public RoType GetByReferenceType(RoType elementType)
				{
					return elementType.GetUniqueByRefType();
				}

				public RoType GetPointerType(RoType elementType)
				{
					return elementType.GetUniquePointerType();
				}

				public RoType GetGenericInstantiation(RoType genericType, System.Collections.Immutable.ImmutableArray<RoType> typeArguments)
				{
					if (!(genericType is RoDefinitionType genericTypeDefinition))
					{
						throw new BadImageFormatException();
					}
					return genericTypeDefinition.GetUniqueConstructedGenericType(ImmutableArrayExtensions.ToArray<RoType>(typeArguments));
				}

				public RoType GetGenericTypeParameter(TypeContext genericContext, int index)
				{
					return genericContext.GetGenericTypeArgumentOrNull(index) ?? throw new BadImageFormatException(System.SR.Format(MDCFR.Properties.Resources.GenericTypeParamIndexOutOfRange, index));
				}

				public RoType GetGenericMethodParameter(TypeContext genericContext, int index)
				{
					return genericContext.GetGenericMethodArgumentOrNull(index) ?? throw new BadImageFormatException(System.SR.Format(MDCFR.Properties.Resources.GenericMethodParamIndexOutOfRange, index));
				}

				public RoType GetFunctionPointerType(MethodSignature<RoType> signature)
				{
					throw new NotSupportedException(MDCFR.Properties.Resources.NotSupported_FunctionPointers);
				}

				public RoType GetModifiedType(RoType modifier, RoType unmodifiedType, bool isRequired)
				{
					return unmodifiedType;
				}

				public RoType GetPinnedType(RoType elementType)
				{
					return elementType;
				}

				public RoType GetPrimitiveType(PrimitiveTypeCode typeCode)
				{
					return base.Loader.GetCoreType(typeCode.ToCoreType());
				}

				public RoType GetSystemType()
				{
					return base.Loader.GetCoreType(CoreType.Type);
				}

				public bool IsSystemType(RoType type)
				{
					return type == base.Loader.TryGetCoreType(CoreType.Type);
				}

				public PrimitiveTypeCode GetUnderlyingEnumType(RoType type)
				{
					return type.GetEnumUnderlyingPrimitiveTypeCode(base.Loader);
				}

				public RoType GetTypeFromSerializedName(string name)
				{
					if (name == null)
					{
						return null;
					}
					return Helpers.LoadTypeFromAssemblyQualifiedName(name, GetRoAssembly(), ignoreCase: false, throwOnError: true);
				}
			}

			internal sealed class EcmaPinnedTypeProvider : EcmaWrappedTypeProvider
			{
				internal EcmaPinnedTypeProvider(EcmaModule module) : base(module) { }

				public sealed override RoType GetModifiedType(RoType modifier, RoType unmodifiedType, bool isRequired)
				{
					return unmodifiedType;
				}

				public sealed override RoType GetPinnedType(RoType elementType) { return new RoPinnedType(elementType); }
			}

			/// <summary>
			/// Base class for all PropertyInfo objects created by a MetadataLoadContext and get its metadata from a PEReader.
			/// </summary>
			internal sealed class EcmaProperty : RoProperty
			{
				private readonly EcmaModule _module;

				private readonly PropertyDefinitionHandle _handle;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly PropertyDefinition _neverAccessThisExceptThroughPropertyDefinitionProperty;

				public sealed override IEnumerable<CustomAttributeData> CustomAttributes => PropertyDefinition.GetCustomAttributes().ToTrueCustomAttributes(_module);

				public sealed override int MetadataToken => _handle.GetToken();

				private MetadataReader Reader => _module.Reader;

				private MetadataLoadContext Loader => GetRoModule().Loader;

				private ref readonly PropertyDefinition PropertyDefinition
				{
					get
					{
						Loader.DisposeCheck();
						return ref _neverAccessThisExceptThroughPropertyDefinitionProperty;
					}
				}

				internal EcmaProperty(RoInstantiationProviderType declaringType, PropertyDefinitionHandle handle, Type reflectedType)
					: base(declaringType, reflectedType)
				{
					_handle = handle;
					_module = (EcmaModule)declaringType.Module;
					_neverAccessThisExceptThroughPropertyDefinitionProperty = handle.GetPropertyDefinition(Reader);
				}

				internal sealed override RoModule GetRoModule()
				{
					return _module;
				}

				public sealed override bool Equals([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] object obj)
				{
					if (!(obj is EcmaProperty ecmaProperty))
					{
						return false;
					}
					if (_handle != ecmaProperty._handle)
					{
						return false;
					}
					if (DeclaringType != ecmaProperty.DeclaringType)
					{
						return false;
					}
					if (ReflectedType != ecmaProperty.ReflectedType)
					{
						return false;
					}
					return true;
				}

				public sealed override int GetHashCode()
				{
					return _handle.GetHashCode() ^ DeclaringType.GetHashCode();
				}

				protected sealed override string ComputeName()
				{
					return PropertyDefinition.Name.GetString(Reader);
				}

				protected sealed override PropertyAttributes ComputeAttributes()
				{
					return PropertyDefinition.Attributes;
				}

				protected sealed override Type ComputePropertyType()
				{
					return PropertyDefinition.DecodeSignature(_module, base.TypeContext).ReturnType;
				}

				protected sealed override object ComputeRawConstantValue()
				{
					return PropertyDefinition.GetDefaultValue().ToRawObject(Reader);
				}

				public sealed override Type[] GetOptionalCustomModifiers()
				{
					return GetCustomModifiers(isRequired: false);
				}

				public sealed override Type[] GetRequiredCustomModifiers()
				{
					return GetCustomModifiers(isRequired: true);
				}

				private Type[] GetCustomModifiers(bool isRequired)
				{
					RoType returnType = PropertyDefinition.DecodeSignature(new EcmaModifiedTypeProvider(_module), base.TypeContext).ReturnType;
					return returnType.ExtractCustomModifiers(isRequired);
				}

				public sealed override string ToString()
				{
					string disposedString = Loader.GetDisposedString();
					if (disposedString != null)
					{
						return disposedString;
					}
					StringBuilder stringBuilder = new StringBuilder();
					ISignatureTypeProvider<string, TypeContext> instance = EcmaSignatureTypeProviderForToString.Instance;
					MethodSignature<string> methodSignature = PropertyDefinition.DecodeSignature(instance, base.TypeContext);
					stringBuilder.Append(methodSignature.ReturnType);
					stringBuilder.Append(' ');
					stringBuilder.Append(Name);
					if (methodSignature.ParameterTypes.Length != 0)
					{
						stringBuilder.Append('[');
						for (int i = 0; i < methodSignature.ParameterTypes.Length; i++)
						{
							if (i != 0)
							{
								stringBuilder.Append(',');
							}
							stringBuilder.Append(methodSignature.ParameterTypes[i]);
						}
						stringBuilder.Append(']');
					}
					return stringBuilder.ToString();
				}

				protected sealed override RoMethod ComputeGetterMethod()
				{
					return PropertyDefinition.GetAccessors().Getter.ToMethodOrNull(GetRoDeclaringType(), ReflectedType);
				}

				protected sealed override RoMethod ComputeSetterMethod()
				{
					return PropertyDefinition.GetAccessors().Setter.ToMethodOrNull(GetRoDeclaringType(), ReflectedType);
				}
			}

			/// <summary>
			/// These are the official entrypoints that code should use to resolve metadata tokens.
			/// </summary>
			internal static class EcmaResolver
			{
				private static readonly Func<EntityHandle, EcmaModule, EcmaDefinitionType> s_resolveTypeDef = (EntityHandle h, EcmaModule m) => new EcmaDefinitionType((TypeDefinitionHandle)h, m);

				private static readonly Func<EntityHandle, EcmaModule, RoDefinitionType> s_resolveTypeRef = (EntityHandle h, EcmaModule m) => ComputeTypeRefResolution((TypeReferenceHandle)h, m);

				private static readonly Func<EntityHandle, EcmaModule, EcmaGenericParameterType> s_resolveGenericParam = delegate(EntityHandle h, EcmaModule module)
				{
					MetadataReader reader = module.Reader;
					GenericParameterHandle handle = (GenericParameterHandle)h;
					return handle.GetGenericParameter(reader).Parent.Kind switch
					{
						HandleKind.TypeDefinition => new EcmaGenericTypeParameterType(handle, module), 
						HandleKind.MethodDefinition => new EcmaGenericMethodParameterType(handle, module), 
						_ => throw new BadImageFormatException(), 
					};
				};

				private static readonly Func<EntityHandle, EcmaModule, RoAssembly> s_resolveAssembly = delegate(EntityHandle h, EcmaModule m)
				{
					RoAssemblyName refName = ((AssemblyReferenceHandle)h).ToRoAssemblyName(m.Reader);
					return m.Loader.ResolveToAssemblyOrExceptionAssembly(refName);
				};

				public static RoType ResolveTypeDefRefOrSpec(this EntityHandle handle, EcmaModule module, in TypeContext typeContext)
				{
					return handle.Kind switch
					{
						HandleKind.TypeDefinition => ((TypeDefinitionHandle)handle).ResolveTypeDef(module), 
						HandleKind.TypeReference => ((TypeReferenceHandle)handle).ResolveTypeRef(module), 
						HandleKind.TypeSpecification => ((TypeSpecificationHandle)handle).ResolveTypeSpec(module, in typeContext), 
						_ => throw new BadImageFormatException(), 
					};
				}

				public static EcmaDefinitionType ResolveTypeDef(this TypeDefinitionHandle handle, EcmaModule module)
				{
					return module.TypeDefTable.GetOrAdd(handle, module, s_resolveTypeDef);
				}

				public static RoDefinitionType ResolveTypeRef(this TypeReferenceHandle handle, EcmaModule module)
				{
					return module.TypeRefTable.GetOrAdd(handle, module, s_resolveTypeRef);
				}

				private static RoDefinitionType ComputeTypeRefResolution(TypeReferenceHandle handle, EcmaModule module)
				{
					MetadataReader reader = module.Reader;
					TypeReference typeReference = handle.GetTypeReference(reader);
					ReadOnlySpan<byte> ns = typeReference.Namespace.AsReadOnlySpan(reader);
					ReadOnlySpan<byte> readOnlySpan = typeReference.Name.AsReadOnlySpan(reader);
					EntityHandle resolutionScope = typeReference.ResolutionScope;
					if (resolutionScope.IsNil)
					{
						Exception e;
						RoDefinitionType typeCore = module.GetEcmaAssembly().GetTypeCore(ns, readOnlySpan, ignoreCase: false, out e);
						if (typeCore == null)
						{
							throw e;
						}
						return typeCore;
					}
					HandleKind kind = resolutionScope.Kind;
					switch (kind)
					{
					case HandleKind.TypeReference:
					{
						if (kind != HandleKind.TypeReference)
						{
							break;
						}
						RoDefinitionType roDefinitionType = ((TypeReferenceHandle)resolutionScope).ResolveTypeRef(module);
						RoDefinitionType nestedTypeCore = roDefinitionType.GetNestedTypeCore(readOnlySpan);
						return nestedTypeCore ?? throw new TypeLoadException(System.SR.Format(System.SR.Format(MDCFR.Properties.Resources.TypeNotFound, roDefinitionType.ToString() + "[]", roDefinitionType.Assembly.FullName)));
					}
					case HandleKind.AssemblyReference:
					{
						AssemblyReferenceHandle handle2 = (AssemblyReferenceHandle)resolutionScope;
						RoAssembly roAssembly = handle2.ResolveAssembly(module);
						Exception e4;
						RoDefinitionType typeCore4 = roAssembly.GetTypeCore(ns, readOnlySpan, ignoreCase: false, out e4);
						if (typeCore4 == null)
						{
							throw e4;
						}
						return typeCore4;
					}
					case HandleKind.ModuleDefinition:
					{
						Exception e3;
						RoDefinitionType typeCore3 = module.GetTypeCore(ns, readOnlySpan, ignoreCase: false, out e3);
						if (typeCore3 == null)
						{
							throw e3;
						}
						return typeCore3;
					}
					case HandleKind.ModuleReference:
					{
						string @string = ((ModuleReferenceHandle)resolutionScope).GetModuleReference(module.Reader).Name.GetString(module.Reader);
						RoModule roModule = module.GetRoAssembly().GetRoModule(@string);
						if (roModule == null)
						{
							throw new BadImageFormatException(System.SR.Format(MDCFR.Properties.Resources.BadImageFormat_TypeRefModuleNotInManifest, module.Assembly.FullName, $"0x{handle.GetToken():x8}"));
						}
						Exception e2;
						RoDefinitionType typeCore2 = roModule.GetTypeCore(ns, readOnlySpan, ignoreCase: false, out e2);
						if (typeCore2 == null)
						{
							throw e2;
						}
						return typeCore2;
					}
					}
					throw new BadImageFormatException(System.SR.Format(MDCFR.Properties.Resources.BadImageFormat_TypeRefBadScopeType, module.Assembly.FullName, $"0x{handle.GetToken():x8}"));
				}

				public static RoType ResolveTypeSpec(this TypeSpecificationHandle handle, EcmaModule module, in TypeContext typeContext)
				{
					return handle.GetTypeSpecification(module.Reader).DecodeSignature(module, typeContext);
				}

				public static EcmaGenericParameterType ResolveGenericParameter(this GenericParameterHandle handle, EcmaModule module)
				{
					return module.GenericParamTable.GetOrAdd(handle, module, s_resolveGenericParam);
				}

				public static RoAssembly ResolveAssembly(this AssemblyReferenceHandle handle, EcmaModule module)
				{
					Exception e;
					RoAssembly roAssembly = handle.TryResolveAssembly(module, out e);
					if (roAssembly == null)
					{
						throw e;
					}
					return roAssembly;
				}

				public static RoAssembly TryResolveAssembly(this AssemblyReferenceHandle handle, EcmaModule module, out Exception e)
				{
					e = null;
					RoAssembly roAssembly = handle.ResolveToAssemblyOrExceptionAssembly(module);
					if (roAssembly is RoExceptionAssembly roExceptionAssembly)
					{
						e = roExceptionAssembly.Exception;
						return null;
					}
					return roAssembly;
				}

				public static RoAssembly ResolveToAssemblyOrExceptionAssembly(this AssemblyReferenceHandle handle, EcmaModule module)
				{
					return module.AssemblyRefTable.GetOrAdd(handle, module, s_resolveAssembly);
				}

				public static T ResolveMethod<T>(this MethodDefinitionHandle handle, EcmaModule module, in TypeContext typeContext) where T : MethodBase
				{
					MetadataReader reader = module.Reader;
					MethodDefinition method = handle.GetMethodDefinition(reader);
					RoInstantiationProviderType roInstantiationProviderType = method.GetDeclaringType().ResolveAndSpecializeType(module, in typeContext);
					EcmaMethodDecoder decoder = new EcmaMethodDecoder(handle, module);
					if (method.IsConstructor(reader))
					{
						return (T)(MethodBase)new RoDefinitionConstructor<EcmaMethodDecoder>(roInstantiationProviderType, decoder);
					}
					return (T)(MethodBase)new RoDefinitionMethod<EcmaMethodDecoder>(roInstantiationProviderType, roInstantiationProviderType, decoder);
				}

				private static RoInstantiationProviderType ResolveAndSpecializeType(this TypeDefinitionHandle handle, EcmaModule module, in TypeContext typeContext)
				{
					RoDefinitionType roDefinitionType = handle.ResolveTypeDef(module);
					if (typeContext.GenericTypeArguments != null && roDefinitionType.IsGenericTypeDefinition)
					{
						return roDefinitionType.GetUniqueConstructedGenericType(typeContext.GenericTypeArguments);
					}
					return roDefinitionType;
				}
			}

			internal sealed class EcmaSignatureTypeProviderForToString : ISignatureTypeProvider<string, TypeContext>, IConstructedTypeProvider<string>, ISZArrayTypeProvider<string>, ISimpleTypeProvider<string>
			{
				public static readonly EcmaSignatureTypeProviderForToString Instance = new EcmaSignatureTypeProviderForToString();

				private EcmaSignatureTypeProviderForToString()
				{
				}

				public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
				{
					return handle.ToTypeString(reader);
				}

				public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
				{
					return handle.ToTypeString(reader);
				}

				public string GetTypeFromSpecification(MetadataReader reader, TypeContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
				{
					return handle.ToTypeString(reader, in genericContext);
				}

				public string GetSZArrayType(string elementType)
				{
					return elementType + "[]";
				}

				public string GetArrayType(string elementType, ArrayShape shape)
				{
					return elementType + Helpers.ComputeArraySuffix(shape.Rank, multiDim: true);
				}

				public string GetByReferenceType(string elementType)
				{
					return elementType + "&";
				}

				public string GetPointerType(string elementType)
				{
					return elementType + "*";
				}

				public string GetGenericInstantiation(string genericType, System.Collections.Immutable.ImmutableArray<string> typeArguments)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(genericType);
					stringBuilder.Append('[');
					for (int i = 0; i < typeArguments.Length; i++)
					{
						if (i != 0)
						{
							stringBuilder.Append(',');
						}
						stringBuilder.Append(typeArguments[i]);
					}
					stringBuilder.Append(']');
					return stringBuilder.ToString();
				}

				public string GetGenericTypeParameter(TypeContext genericContext, int index)
				{
					return genericContext.GetGenericTypeArgumentOrNull(index)?.ToString() ?? ("!" + index);
				}

				public string GetGenericMethodParameter(TypeContext genericContext, int index)
				{
					return genericContext.GetGenericMethodArgumentOrNull(index)?.ToString() ?? ("!!" + index);
				}

				public string GetFunctionPointerType(MethodSignature<string> signature)
				{
					return "?";
				}

				public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired)
				{
					return unmodifiedType;
				}

				public string GetPinnedType(string elementType)
				{
					return elementType;
				}

				public string GetPrimitiveType(PrimitiveTypeCode typeCode)
				{
					typeCode.ToCoreType().GetFullName(out var ns, out var name);
					return ns.ToUtf16() + "." + name.ToUtf16();
				}
			}

			/// <summary>
			/// Helpers to generate ToString() output for Type objects that occur as part of MemberInfo objects. Not used to generate ToString() for
			/// System.Type itself.
			///
			/// Though this may seem like something that belongs at the format-agnostic layer, it is not acceptable for ToString() to
			/// trigger resolving. Thus, ToString() must be built up using only the raw data in the metadata and without creating or
			/// resolving Type objects.
			/// </summary>
			internal static class EcmaToStringHelpers
			{
				public static string ToTypeString(this EntityHandle handle, in TypeContext typeContext, MetadataReader reader)
				{
					return handle.Kind switch
					{
						HandleKind.TypeDefinition => ((TypeDefinitionHandle)handle).ToTypeString(reader), 
						HandleKind.TypeReference => ((TypeReferenceHandle)handle).ToTypeString(reader), 
						HandleKind.TypeSpecification => ((TypeSpecificationHandle)handle).ToTypeString(reader, in typeContext), 
						_ => "?", 
					};
				}

				public static string ToTypeString(this TypeDefinitionHandle handle, MetadataReader reader)
				{
					TypeDefinition typeDefinition = handle.GetTypeDefinition(reader);
					string ns = typeDefinition.Namespace.GetStringOrNull(reader) ?? string.Empty;
					string text = typeDefinition.Name.GetString(reader);
					if (typeDefinition.IsNested)
					{
						string text2 = typeDefinition.GetDeclaringType().ToTypeString(reader);
						text = text2 + "+" + text;
					}
					return ns.AppendTypeName(text);
				}

				public static string ToTypeString(this TypeReferenceHandle handle, MetadataReader reader)
				{
					TypeReference typeReference = handle.GetTypeReference(reader);
					string ns = typeReference.Namespace.GetStringOrNull(reader) ?? string.Empty;
					string text = typeReference.Name.GetString(reader);
					if (typeReference.ResolutionScope.Kind == HandleKind.TypeDefinition || typeReference.ResolutionScope.Kind == HandleKind.TypeReference)
					{
						EntityHandle resolutionScope = typeReference.ResolutionScope;
						TypeContext typeContext = default(TypeContext);
						string text2 = resolutionScope.ToTypeString(in typeContext, reader);
						text = text2 + "+" + text;
					}
					return ns.AppendTypeName(text);
				}

				public static string ToTypeString(this TypeSpecificationHandle handle, MetadataReader reader, in TypeContext typeContext)
				{
					return handle.GetTypeSpecification(reader).DecodeSignature(EcmaSignatureTypeProviderForToString.Instance, typeContext);
				}
			}

			internal abstract class EcmaWrappedTypeProvider : ISignatureTypeProvider<RoType, TypeContext>, IConstructedTypeProvider<RoType>, ISZArrayTypeProvider<RoType>, ISimpleTypeProvider<RoType>
			{
				private readonly EcmaModule _module;

				private readonly ISignatureTypeProvider<RoType, TypeContext> _typeProvider;

				protected EcmaWrappedTypeProvider(EcmaModule module)
				{
					_module = module;
					_typeProvider = module;
				}

				public RoType GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
				{
					return _typeProvider.GetTypeFromDefinition(reader, handle, rawTypeKind);
				}

				public RoType GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
				{
					return _typeProvider.GetTypeFromReference(reader, handle, rawTypeKind);
				}

				public RoType GetTypeFromSpecification(MetadataReader reader, TypeContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
				{
					return _typeProvider.GetTypeFromSpecification(reader, genericContext, handle, rawTypeKind);
				}

				public RoType GetSZArrayType(RoType elementType)
				{
					return _typeProvider.GetSZArrayType(elementType.SkipTypeWrappers());
				}

				public RoType GetArrayType(RoType elementType, ArrayShape shape)
				{
					return _typeProvider.GetArrayType(elementType.SkipTypeWrappers(), shape);
				}

				public RoType GetByReferenceType(RoType elementType)
				{
					return _typeProvider.GetByReferenceType(elementType.SkipTypeWrappers());
				}

				public RoType GetPointerType(RoType elementType)
				{
					return _typeProvider.GetPointerType(elementType.SkipTypeWrappers());
				}

				public RoType GetGenericInstantiation(RoType genericType, System.Collections.Immutable.ImmutableArray<RoType> typeArguments)
				{
					genericType = genericType.SkipTypeWrappers();
					System.Collections.Immutable.ImmutableArray<RoType> typeArguments2 = System.Collections.Immutable.ImmutableArray<RoType>.Empty;
					for (int i = 0; i < typeArguments.Length; i++)
					{
						typeArguments2 = typeArguments2.Add(typeArguments[i].SkipTypeWrappers());
					}
					return _typeProvider.GetGenericInstantiation(genericType, typeArguments2);
				}

				public RoType GetGenericTypeParameter(TypeContext genericContext, int index)
				{
					return _typeProvider.GetGenericTypeParameter(genericContext, index);
				}

				public RoType GetGenericMethodParameter(TypeContext genericContext, int index)
				{
					return _typeProvider.GetGenericMethodParameter(genericContext, index);
				}

				public RoType GetFunctionPointerType(MethodSignature<RoType> signature)
				{
					return _typeProvider.GetFunctionPointerType(signature);
				}

				public abstract RoType GetModifiedType(RoType modifier, RoType unmodifiedType, bool isRequired);

				public abstract RoType GetPinnedType(RoType elementType);

				public RoType GetPrimitiveType(PrimitiveTypeCode typeCode)
				{
					return _typeProvider.GetPrimitiveType(typeCode);
				}
			}

			internal readonly struct GuardedPEReader
			{
				private readonly MetadataLoadContext _loader;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly PEReader _peReader;

				[DebuggerBrowsable(DebuggerBrowsableState.Never)]
				private readonly MetadataReader _reader;

				public PEReader PEReader
				{
					get
					{
						_loader.DisposeCheck();
						return _peReader;
					}
				}

				public MetadataReader Reader
				{
					get
					{
						_loader.DisposeCheck();
						return _reader;
					}
				}

				public GuardedPEReader(MetadataLoadContext loader, PEReader peReader, MetadataReader reader)
				{
					_loader = loader;
					_peReader = peReader;
					_reader = reader;
				}
			}

			internal struct InternalManifestResourceInfo
			{
				public bool Found;

				public string FileName;

				public Assembly ReferencedAssembly;

				public unsafe byte* PointerToResource;

				public uint SizeOfResource;

				public ResourceLocation ResourceLocation;
			}

			internal static class MetadataExtensions
			{
				public static AssemblyFile GetAssemblyFile(this AssemblyFileHandle handle, MetadataReader reader)
				{
					return reader.GetAssemblyFile(handle);
				}

				public static AssemblyReference GetAssemblyReference(this AssemblyReferenceHandle handle, MetadataReader reader)
				{
					return reader.GetAssemblyReference(handle);
				}

				public static byte[] GetBlobBytes(this BlobHandle handle, MetadataReader reader)
				{
					return reader.GetBlobBytes(handle);
				}

				public static System.Collections.Immutable.ImmutableArray<byte> GetBlobContent(this BlobHandle handle, MetadataReader reader)
				{
					return reader.GetBlobContent(handle);
				}

				public static BlobReader GetBlobReader(this BlobHandle handle, MetadataReader reader)
				{
					return reader.GetBlobReader(handle);
				}

				public static BlobReader GetBlobReader(this StringHandle handle, MetadataReader reader)
				{
					return reader.GetBlobReader(handle);
				}

				public static Constant GetConstant(this ConstantHandle handle, MetadataReader reader)
				{
					return reader.GetConstant(handle);
				}

				public static CustomAttribute GetCustomAttribute(this CustomAttributeHandle handle, MetadataReader reader)
				{
					return reader.GetCustomAttribute(handle);
				}

				public static CustomAttributeHandleCollection GetCustomAttributes(this EntityHandle handle, MetadataReader reader)
				{
					return reader.GetCustomAttributes(handle);
				}

				public static CustomDebugInformation GetCustomDebugInformation(this CustomDebugInformationHandle handle, MetadataReader reader)
				{
					return reader.GetCustomDebugInformation(handle);
				}

				public static CustomDebugInformationHandleCollection GetCustomDebugInformation(this EntityHandle handle, MetadataReader reader)
				{
					return reader.GetCustomDebugInformation(handle);
				}

				public static DeclarativeSecurityAttribute GetDeclarativeSecurityAttribute(this DeclarativeSecurityAttributeHandle handle, MetadataReader reader)
				{
					return reader.GetDeclarativeSecurityAttribute(handle);
				}

				public static Document GetDocument(this DocumentHandle handle, MetadataReader reader)
				{
					return reader.GetDocument(handle);
				}

				public static EventDefinition GetEventDefinition(this EventDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetEventDefinition(handle);
				}

				public static ExportedType GetExportedType(this ExportedTypeHandle handle, MetadataReader reader)
				{
					return reader.GetExportedType(handle);
				}

				public static FieldDefinition GetFieldDefinition(this FieldDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetFieldDefinition(handle);
				}

				public static GenericParameter GetGenericParameter(this GenericParameterHandle handle, MetadataReader reader)
				{
					return reader.GetGenericParameter(handle);
				}

				public static GenericParameterConstraint GetGenericParameterConstraint(this GenericParameterConstraintHandle handle, MetadataReader reader)
				{
					return reader.GetGenericParameterConstraint(handle);
				}

				public static Guid GetGuid(this GuidHandle handle, MetadataReader reader)
				{
					return reader.GetGuid(handle);
				}

				public static ImportScope GetImportScope(this ImportScopeHandle handle, MetadataReader reader)
				{
					return reader.GetImportScope(handle);
				}

				public static InterfaceImplementation GetInterfaceImplementation(this InterfaceImplementationHandle handle, MetadataReader reader)
				{
					return reader.GetInterfaceImplementation(handle);
				}

				public static LocalConstant GetLocalConstant(this LocalConstantHandle handle, MetadataReader reader)
				{
					return reader.GetLocalConstant(handle);
				}

				public static LocalScope GetLocalScope(this LocalScopeHandle handle, MetadataReader reader)
				{
					return reader.GetLocalScope(handle);
				}

				public static LocalScopeHandleCollection GetLocalScopes(this MethodDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetLocalScopes(handle);
				}

				public static LocalScopeHandleCollection GetLocalScopes(this MethodDebugInformationHandle handle, MetadataReader reader)
				{
					return reader.GetLocalScopes(handle);
				}

				public static LocalVariable GetLocalVariable(this LocalVariableHandle handle, MetadataReader reader)
				{
					return reader.GetLocalVariable(handle);
				}

				public static ManifestResource GetManifestResource(this ManifestResourceHandle handle, MetadataReader reader)
				{
					return reader.GetManifestResource(handle);
				}

				public static MemberReference GetMemberReference(this MemberReferenceHandle handle, MetadataReader reader)
				{
					return reader.GetMemberReference(handle);
				}

				public static MethodDebugInformation GetMethodDebugInformation(this MethodDebugInformationHandle handle, MetadataReader reader)
				{
					return reader.GetMethodDebugInformation(handle);
				}

				public static MethodDebugInformation GetMethodDebugInformation(this MethodDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetMethodDebugInformation(handle);
				}

				public static MethodDefinition GetMethodDefinition(this MethodDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetMethodDefinition(handle);
				}

				public static MethodImplementation GetMethodImplementation(this MethodImplementationHandle handle, MetadataReader reader)
				{
					return reader.GetMethodImplementation(handle);
				}

				public static MethodSpecification GetMethodSpecification(this MethodSpecificationHandle handle, MetadataReader reader)
				{
					return reader.GetMethodSpecification(handle);
				}

				public static ModuleReference GetModuleReference(this ModuleReferenceHandle handle, MetadataReader reader)
				{
					return reader.GetModuleReference(handle);
				}

				public static NamespaceDefinition GetNamespaceDefinition(this NamespaceDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetNamespaceDefinition(handle);
				}

				public static Parameter GetParameter(this ParameterHandle handle, MetadataReader reader)
				{
					return reader.GetParameter(handle);
				}

				public static PropertyDefinition GetPropertyDefinition(this PropertyDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetPropertyDefinition(handle);
				}

				public static StandaloneSignature GetStandaloneSignature(this StandaloneSignatureHandle handle, MetadataReader reader)
				{
					return reader.GetStandaloneSignature(handle);
				}

				public static string GetString(this StringHandle handle, MetadataReader reader)
				{
					return reader.GetString(handle);
				}

				public static string GetString(this NamespaceDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetString(handle);
				}

				public static string GetString(this DocumentNameBlobHandle handle, MetadataReader reader)
				{
					return reader.GetString(handle);
				}

				public static TypeDefinition GetTypeDefinition(this TypeDefinitionHandle handle, MetadataReader reader)
				{
					return reader.GetTypeDefinition(handle);
				}

				public static TypeReference GetTypeReference(this TypeReferenceHandle handle, MetadataReader reader)
				{
					return reader.GetTypeReference(handle);
				}

				public static TypeSpecification GetTypeSpecification(this TypeSpecificationHandle handle, MetadataReader reader)
				{
					return reader.GetTypeSpecification(handle);
				}

				public static string GetUserString(this UserStringHandle handle, MetadataReader reader)
				{
					return reader.GetUserString(handle);
				}

				public static int GetToken(this Handle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this EntityHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this TypeDefinitionHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this TypeReferenceHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this TypeSpecificationHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this GenericParameterHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this GenericParameterConstraintHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this FieldDefinitionHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this EventDefinitionHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this MethodDefinitionHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this PropertyDefinitionHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this ParameterHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this StandaloneSignatureHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static int GetToken(this AssemblyFileHandle handle)
				{
					return MetadataTokens.GetToken(handle);
				}

				public static string GetStringOrNull(this StringHandle handle, MetadataReader reader)
				{
					if (!handle.IsNil)
					{
						return reader.GetString(handle);
					}
					return null;
				}

				public static bool Equals(this StringHandle handle, string value, MetadataReader reader)
				{
					return reader.StringComparer.Equals(handle, value, ignoreCase: false);
				}

				public unsafe static bool Equals(this StringHandle handle, ReadOnlySpan<byte> utf8, MetadataReader reader)
				{
					BlobReader blobReader = handle.GetBlobReader(reader);
					ReadOnlySpan<byte> other = new ReadOnlySpan<byte>(blobReader.CurrentPointer, blobReader.Length);
					return utf8.SequenceEqual(other);
				}

				public static Handle ToHandle(this int token)
				{
					return MetadataTokens.Handle(token);
				}

				public static TypeDefinitionHandle ToTypeDefinitionHandle(this int token)
				{
					return MetadataTokens.TypeDefinitionHandle(token);
				}

				public static TypeReferenceHandle ToTypeReferenceHandle(this int token)
				{
					return MetadataTokens.TypeReferenceHandle(token);
				}

				public static TypeSpecificationHandle ToTypeSpecificationHandle(this int token)
				{
					return MetadataTokens.TypeSpecificationHandle(token);
				}
			}

			/// <summary>
			/// Thread-safe interning table for objects that map 1-1 with ECMA tokens.
			///
			/// The key type is hard-coded to EntityHandle.
			/// The "T" type is the value type (e.g. RoTypeDefinition objects)
			/// The "C" type is an optional context value passed through the factory methods (so we don't to allocate a closure each time.)
			/// </summary>
			internal sealed class MetadataTable<T, C> where T : class
			{
				private readonly T[] _table;

				public int Count { get; }

				public MetadataTable(int count)
				{
					Count = count;
					_table = new T[count];
				}

				public T GetOrAdd(EntityHandle handle, C context, Func<EntityHandle, C, T> factory)
				{
					int num = handle.GetToken().GetTokenRowNumber() - 1;
					T[] table = _table;
					T val = Volatile.Read(ref table[num]);
					if (val != null)
					{
						return val;
					}
					T val2 = factory(handle, context);
					return Interlocked.CompareExchange(ref table[num], val2, null) ?? val2;
				}

				/// <summary>
				/// Return a read-only enumeration of the table (safe to hand back to app code.)
				/// </summary>
				public IEnumerable<T> EnumerateValues(int skip = 0)
				{
					for (int i = skip; i < _table.Length; i++)
					{
						yield return _table[i];
					}
				}

				/// <summary>
				/// Return a newly allocated array containing the contents (safe to hand back to app code.)
				/// </summary>
				public TOut[] ToArray<TOut>(int skip = 0)
				{
					TOut[] array = new TOut[Count - skip];
					Array.Copy(_table, skip, array, 0, array.Length);
					return array;
				}
			}
		}
	}
}