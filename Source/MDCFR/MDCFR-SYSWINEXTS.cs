/*
	Most of the code provided here is from .NET Foundation:
	Licensed to the .NET Foundation under one or more agreements.
    The .NET Foundation licenses this file to you under the MIT license.
*/

using System;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32.SafeHandles;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

#nullable enable

namespace ROOT.CryptographicOperations
{
	/// <summary>
    /// Contains values for avalilable X509 Selection Flags.
    /// </summary>
    public enum X509SelectionFlag
    {
        /// <summary>Single Selection is applied.</summary>
        SingleSelection = 0x00,
        /// <summary>Multiple Selection is applied.</summary>
        MultiSelection = 0x01
    }

    /// <summary>
    /// Shows the default Windows Certificate properties for a X509 certificate as a UI.
    /// </summary>
    public sealed class X509Certificate2UI
    {
        internal const int ERROR_SUCCESS = 0;
        internal const int ERROR_CANCELLED = 1223;

        /// <summary>
        /// Displays the specified <see cref="X509Certificate2"/> certificate.
        /// </summary>
        /// <param name="certificate">The <see cref="X509Certificate2"/> certificate to show.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="certificate"/> is <see langword="null"/>.</exception>
        /// <exception cref="CryptographicException">An internal error occured.</exception>
        public static void DisplayCertificate(X509Certificate2 certificate)
        {
            if (certificate == null) { throw new ArgumentNullException(nameof(certificate)); }

            DisplayX509Certificate(certificate, IntPtr.Zero);
        }

        /// <summary>
        /// Displays the specified <see cref="X509Certificate2"/> certificate , and spawns the result using the specified UI handle.
        /// </summary>
        /// <param name="certificate">The <see cref="X509Certificate2"/> certificate to show.</param>
        /// <param name="hwndParent">The UI handle that the certificate UI will use so as to spawn.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="certificate"/> is <see langword="null"/>.</exception>
        /// <exception cref="CryptographicException">An internal error occured.</exception>
        public static void DisplayCertificate(X509Certificate2 certificate, IntPtr hwndParent)
        {
            if (certificate == null) { throw new ArgumentNullException(nameof(certificate)); }

            DisplayX509Certificate(certificate, hwndParent);
        }

        /// <summary>
        /// Selects a certificate or certificates from a specified collection.
        /// </summary>
        /// <param name="certificates">The <see cref="X509Certificate2Collection"/> to select certififcates from.</param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="selectionFlag">The certificate selection mode to use. To return multiple certififcates , use the <see cref="X509SelectionFlag.MultiSelection"/> value.</param>
        /// <returns>A new <see cref="X509Certificate2Collection"/> given the match criteria.</returns>
        public static X509Certificate2Collection SelectFromCollection(X509Certificate2Collection certificates, string? title, string? message, X509SelectionFlag selectionFlag)
        {
            return SelectFromCollectionHelper(certificates, title, message, selectionFlag, IntPtr.Zero);
        }

        /// <summary>
        /// Selects a certificate or certificates from a specified collection.
        /// </summary>
        /// <param name="certificates">The <see cref="X509Certificate2Collection"/> to select certififcates from.</param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="selectionFlag">The certificate selection mode to use. To return multiple certififcates , use the <see cref="X509SelectionFlag.MultiSelection"/> value.</param>
        /// <param name="hwndParent">The UI handle that the certificate UI will use so as to spawn.</param>
        /// <returns>A new <see cref="X509Certificate2Collection"/> given the match criteria.</returns>
        public static X509Certificate2Collection SelectFromCollection(X509Certificate2Collection certificates, string? title, string? message, X509SelectionFlag selectionFlag, IntPtr hwndParent)
        {
            return SelectFromCollectionHelper(certificates, title, message, selectionFlag, hwndParent);
        }

        private static unsafe void DisplayX509Certificate(X509Certificate2 certificate, IntPtr hwndParent)
        {
            using (SafeCertContextHandle safeCertContext = X509Utils.DuplicateCertificateContext(certificate))
            {
                if (safeCertContext.IsInvalid)
                    throw new CryptographicException(SR.Format(MDCFR.Properties.Resources.Cryptography_InvalidHandle, nameof(safeCertContext)));

                int dwErrorCode = ERROR_SUCCESS;

                // Initialize view structure.
                Interop.CryptUI.CRYPTUI_VIEWCERTIFICATE_STRUCTW ViewInfo = default;
#if NET7_0_OR_GREATER
                ViewInfo.dwSize = (uint)sizeof(Interop.CryptUI.CRYPTUI_VIEWCERTIFICATE_STRUCTW.Marshaller.Native);
#else
                ViewInfo.dwSize = (uint)Marshal.SizeOf<Interop.CryptUI.CRYPTUI_VIEWCERTIFICATE_STRUCTW>();
#endif
                ViewInfo.hwndParent = hwndParent;
                ViewInfo.dwFlags = 0;
                ViewInfo.szTitle = null;
                ViewInfo.pCertContext = safeCertContext.DangerousGetHandle();
                ViewInfo.rgszPurposes = IntPtr.Zero;
                ViewInfo.cPurposes = 0;
                ViewInfo.pCryptProviderData = IntPtr.Zero;
                ViewInfo.fpCryptProviderDataTrustedUsage = false;
                ViewInfo.idxSigner = 0;
                ViewInfo.idxCert = 0;
                ViewInfo.fCounterSigner = false;
                ViewInfo.idxCounterSigner = 0;
                ViewInfo.cStores = 0;
                ViewInfo.rghStores = IntPtr.Zero;
                ViewInfo.cPropSheetPages = 0;
                ViewInfo.rgPropSheetPages = IntPtr.Zero;
                ViewInfo.nStartPage = 0;

                // View the certificate
                if (!Interop.CryptUI.CryptUIDlgViewCertificateW(ViewInfo, IntPtr.Zero))
                    dwErrorCode = Marshal.GetLastWin32Error();

                // CryptUIDlgViewCertificateW returns ERROR_CANCELLED if the user closes
                // the window through the x button or by pressing CANCEL, so ignore this error code
                if (dwErrorCode != ERROR_SUCCESS && dwErrorCode != ERROR_CANCELLED)
                    throw new CryptographicException(dwErrorCode);
            }
        }

        private static X509Certificate2Collection SelectFromCollectionHelper(X509Certificate2Collection certificates, string? title, string? message, X509SelectionFlag selectionFlag, IntPtr hwndParent)
        {
            if (certificates == null) { throw new ArgumentNullException(nameof(certificates)); }

            if (selectionFlag < X509SelectionFlag.SingleSelection || selectionFlag > X509SelectionFlag.MultiSelection)
                throw new ArgumentException(SR.Format(MDCFR.Properties.Resources.Enum_InvalidValue, nameof(selectionFlag)));

            using (SafeCertStoreHandle safeSourceStoreHandle = X509Utils.ExportToMemoryStore(certificates))
            using (SafeCertStoreHandle safeTargetStoreHandle = SelectFromStore(safeSourceStoreHandle, title, message, selectionFlag, hwndParent))
            {
                return X509Utils.GetCertificates(safeTargetStoreHandle);
            }
        }

        private static unsafe SafeCertStoreHandle SelectFromStore(SafeCertStoreHandle safeSourceStoreHandle, string? title, string? message, X509SelectionFlag selectionFlags, IntPtr hwndParent)
        {
            int dwErrorCode = ERROR_SUCCESS;

            SafeCertStoreHandle safeCertStoreHandle = Interop.Crypt32.CertOpenStore(
                (IntPtr)Interop.Crypt32.CERT_STORE_PROV_MEMORY,
                Interop.Crypt32.X509_ASN_ENCODING | Interop.Crypt32.PKCS_7_ASN_ENCODING,
                IntPtr.Zero,
                0,
                IntPtr.Zero);

            if (safeCertStoreHandle == null || safeCertStoreHandle.IsInvalid)
            {
                Exception e = new CryptographicException(Marshal.GetLastWin32Error());
                safeCertStoreHandle?.Dispose();
                throw e;
            }

            Interop.CryptUI.CRYPTUI_SELECTCERTIFICATE_STRUCTW csc = default;
            // Older versions of CRYPTUI do not check the size correctly,
            // so always force it to the oldest version of the structure.
#if NET7_0_OR_GREATER
            // Declare a local for Native to enable us to get the managed byte offset
            // without having a null check cause a failure.
            Interop.CryptUI.CRYPTUI_SELECTCERTIFICATE_STRUCTW.Marshaller.Native native;
            Unsafe.SkipInit(out native);
            csc.dwSize = (uint)Unsafe.ByteOffset(ref Unsafe.As<Interop.CryptUI.CRYPTUI_SELECTCERTIFICATE_STRUCTW.Marshaller.Native, byte>(ref native), ref Unsafe.As<IntPtr, byte>(ref native.hSelectedCertStore));
#else
            csc.dwSize = (uint)Marshal.OffsetOf(typeof(Interop.CryptUI.CRYPTUI_SELECTCERTIFICATE_STRUCTW), "hSelectedCertStore");
#endif
            csc.hwndParent = hwndParent;
            csc.dwFlags = (uint)selectionFlags;
            csc.szTitle = title;
            csc.dwDontUseColumn = 0;
            csc.szDisplayString = message;
            csc.pFilterCallback = IntPtr.Zero;
            csc.pDisplayCallback = IntPtr.Zero;
            csc.pvCallbackData = IntPtr.Zero;
            csc.cDisplayStores = 1;
            IntPtr hSourceCertStore = safeSourceStoreHandle.DangerousGetHandle();
            csc.rghDisplayStores = new IntPtr(&hSourceCertStore);
            csc.cStores = 0;
            csc.rghStores = IntPtr.Zero;
            csc.cPropSheetPages = 0;
            csc.rgPropSheetPages = IntPtr.Zero;
            csc.hSelectedCertStore = safeCertStoreHandle.DangerousGetHandle();

            SafeCertContextHandle safeCertContextHandle = Interop.CryptUI.CryptUIDlgSelectCertificateW(ref csc);

            if (safeCertContextHandle != null && !safeCertContextHandle.IsInvalid)
            {
                // Single select, so add it to our hCertStore
                SafeCertContextHandle ppStoreContext = SafeCertContextHandle.InvalidHandle;
                if (!Interop.Crypt32.CertAddCertificateLinkToStore(safeCertStoreHandle,
                                                        safeCertContextHandle,
                                                        Interop.Crypt32.CERT_STORE_ADD_ALWAYS,
                                                        ppStoreContext))
                {
                    dwErrorCode = Marshal.GetLastWin32Error();
                }
            }

            if (dwErrorCode != ERROR_SUCCESS)
            {
                safeCertContextHandle?.Dispose();
                throw new CryptographicException(dwErrorCode);
            }

            return safeCertStoreHandle;
        }
    }
	
	internal static class X509Utils
    {
        internal const uint CERT_STORE_ENUM_ARCHIVED_FLAG = 0x00000200;
        internal const uint CERT_STORE_CREATE_NEW_FLAG = 0x00002000;

        internal static SafeCertContextHandle DuplicateCertificateContext(X509Certificate2 certificate)
        {
            SafeCertContextHandle safeCertContext = Interop.Crypt32.CertDuplicateCertificateContext(certificate.Handle);
            GC.KeepAlive(certificate);
            return safeCertContext;
        }

        internal static SafeCertStoreHandle ExportToMemoryStore(X509Certificate2Collection collection)
        {
            SafeCertStoreHandle safeCertStoreHandle;

            // we always want to use CERT_STORE_ENUM_ARCHIVED_FLAG since we want to preserve the collection in this operation.
            // By default, Archived certificates will not be included.
            safeCertStoreHandle = Interop.Crypt32.CertOpenStore(
                new IntPtr(Interop.Crypt32.CERT_STORE_PROV_MEMORY),
                Interop.Crypt32.X509_ASN_ENCODING | Interop.Crypt32.PKCS_7_ASN_ENCODING,
                IntPtr.Zero,
                CERT_STORE_ENUM_ARCHIVED_FLAG | CERT_STORE_CREATE_NEW_FLAG,
                IntPtr.Zero);

            if (safeCertStoreHandle == null || safeCertStoreHandle.IsInvalid)
            {
                Exception e = new CryptographicException(Marshal.GetLastWin32Error());
                safeCertStoreHandle?.Dispose();
                throw e;
            }

            // We use CertAddCertificateLinkToStore to keep a link to the original store, so any property changes get
            // applied to the original store. This has a limit of 99 links per cert context however.
            foreach (X509Certificate2 x509 in collection)
            {
                using (SafeCertContextHandle handle = DuplicateCertificateContext(x509))
                {
                    if (!Interop.Crypt32.CertAddCertificateLinkToStore(
                        safeCertStoreHandle,
                        handle,
                        Interop.Crypt32.CERT_STORE_ADD_ALWAYS,
                        SafeCertContextHandle.InvalidHandle))
                    {
                        throw new CryptographicException(Marshal.GetLastWin32Error());
                    }
                }
            }

            return safeCertStoreHandle;
        }

        internal static X509Certificate2Collection GetCertificates(SafeCertStoreHandle safeCertStoreHandle)
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            IntPtr pEnumContext = Interop.Crypt32.CertEnumCertificatesInStore(safeCertStoreHandle, IntPtr.Zero);
            while (pEnumContext != IntPtr.Zero)
            {
                X509Certificate2 certificate = new X509Certificate2(pEnumContext);
                collection.Add(certificate);
                pEnumContext = Interop.Crypt32.CertEnumCertificatesInStore(safeCertStoreHandle, pEnumContext);
            }

            return collection;
        }
    }
}

namespace Microsoft.Win32.SafeHandles
{
	
	/// <summary>SafeHandle for the HCERTSTORE handle defined by crypt32.</summary>
    internal sealed class SafeCertStoreHandle : SafeCrypt32Handle<SafeCertStoreHandle>
    {
        protected sealed override bool ReleaseHandle()
        {
            bool success = Interop.Crypt32.CertCloseStore(handle, 0);
            return success;
        }
    }
	
	/// <summary>SafeHandle for the CERT_CONTEXT structure defined by crypt32.</summary>
    internal class SafeCertContextHandle : SafeCrypt32Handle<SafeCertContextHandle>
    {
        private SafeCertContextHandle? _parent;

        public SafeCertContextHandle() { }

        public SafeCertContextHandle(SafeCertContextHandle parent)
        {
            if (parent == null) { throw new ArgumentNullException("parent"); }

            Debug.Assert(!parent.IsInvalid);
            Debug.Assert(!parent.IsClosed);

            bool ignored = false;
            parent.DangerousAddRef(ref ignored);
            _parent = parent;

            SetHandle(_parent.handle);
        }

        protected override bool ReleaseHandle()
        {
            if (_parent != null)
            {
                _parent.DangerousRelease();
                _parent = null;
            }
            else
            {
                Interop.Crypt32.CertFreeCertificateContext(handle);
            }

            SetHandle(IntPtr.Zero);
            return true;
        }

        public unsafe Interop.Crypt32.CERT_CONTEXT* CertContext
        {
            get { return (Interop.Crypt32.CERT_CONTEXT*)handle; }
        }

        // Extract the raw CERT_CONTEXT* pointer and reset the SafeHandle to the invalid state so it no longer auto-destroys the CERT_CONTEXT.
        public unsafe Interop.Crypt32.CERT_CONTEXT* Disconnect()
        {
            Interop.Crypt32.CERT_CONTEXT* pCertContext = (Interop.Crypt32.CERT_CONTEXT*)handle;
            SetHandle(IntPtr.Zero);
            return pCertContext;
        }

        public bool HasPersistedPrivateKey
        {
            get { return CertHasProperty(Interop.Crypt32.CertContextPropId.CERT_KEY_PROV_INFO_PROP_ID); }
        }

        public bool HasEphemeralPrivateKey
        {
            get { return CertHasProperty(Interop.Crypt32.CertContextPropId.CERT_KEY_CONTEXT_PROP_ID); }
        }

        public bool ContainsPrivateKey
        {
            get { return HasPersistedPrivateKey || HasEphemeralPrivateKey; }
        }

        public SafeCertContextHandle Duplicate()
        {
            return Interop.Crypt32.CertDuplicateCertificateContext(handle);
        }

        private bool CertHasProperty(Interop.Crypt32.CertContextPropId propertyId)
        {
            int cb = 0;
            bool hasProperty = Interop.Crypt32.CertGetCertificateContextProperty(
                this,
                propertyId,
                null,
                ref cb);

            return hasProperty;
        }
    }
	
	/// <summary>Base class for safe handles representing NULL-based pointers.</summary>
    internal abstract class SafeCrypt32Handle<T> : SafeHandle where T : SafeHandle, new()
    {
        protected SafeCrypt32Handle()
            : base(IntPtr.Zero, true)
        {
        }

        public sealed override bool IsInvalid
        {
            get { return handle == IntPtr.Zero; }
        }

        public static T InvalidHandle
        {
            get { return SafeHandleCache<T>.GetInvalidHandle(() => new T()); }
        }

        protected override void Dispose(bool disposing)
        {
            if (!SafeHandleCache<T>.IsCachedInvalidHandle(this))
            {
                base.Dispose(disposing);
            }
        }
    }
	
    /// <summary>Provides a cache for special instances of SafeHandles.</summary>
    /// <typeparam name="T">Specifies the type of SafeHandle.</typeparam>
    internal static class SafeHandleCache<T> where T : SafeHandle
    {
        private static T? s_invalidHandle;

        /// <summary>
        /// Gets a cached, invalid handle.  As the instance is cached, it should either never be Disposed
        /// or it should override <see cref="SafeHandle.Dispose(bool)"/> to prevent disposal when the
        /// instance represents an invalid handle: <see cref="System.Runtime.InteropServices.SafeHandle.IsInvalid"/> returns <see language="true"/>.
        /// </summary>
        internal static T GetInvalidHandle(Func<T> invalidHandleFactory)
        {
            T? currentHandle = Volatile.Read(ref s_invalidHandle);
            if (currentHandle == null)
            {
                T newHandle = invalidHandleFactory();
                currentHandle = Interlocked.CompareExchange(ref s_invalidHandle, newHandle, null);
                if (currentHandle == null)
                {
                    GC.SuppressFinalize(newHandle);
                    currentHandle = newHandle;
                }
                else
                {
                    newHandle.Dispose();
                }
            }
            Debug.Assert(currentHandle.IsInvalid);
            return currentHandle;
        }

        /// <summary>Gets whether the specified handle is invalid handle.</summary>
        /// <param name="handle">The handle to compare.</param>
        /// <returns>true if <paramref name="handle"/> is invalid handle; otherwise, false.</returns>
        internal static bool IsCachedInvalidHandle(SafeHandle handle)
        {
            Debug.Assert(handle != null);
            bool isCachedInvalidHandle = ReferenceEquals(handle, Volatile.Read(ref s_invalidHandle));
            Debug.Assert(!isCachedInvalidHandle || handle.IsInvalid, "The cached invalid handle must still be invalid.");
            return isCachedInvalidHandle;
        }
    }
}

internal static partial class Interop
{
	
	internal static partial class CryptUI
    {
#if NET7_0_OR_GREATER
        [System.Runtime.InteropServices.Marshalling.NativeMarshalling(typeof(Marshaller))]
#else
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
#endif
        internal struct CRYPTUI_VIEWCERTIFICATE_STRUCTW
        {
            internal uint dwSize;
            internal IntPtr hwndParent;
            internal uint dwFlags;
            internal string? szTitle;
            internal IntPtr pCertContext;
            internal IntPtr rgszPurposes;
            internal uint cPurposes;
            internal IntPtr pCryptProviderData;
            internal bool fpCryptProviderDataTrustedUsage;
            internal uint idxSigner;
            internal uint idxCert;
            internal bool fCounterSigner;
            internal uint idxCounterSigner;
            internal uint cStores;
            internal IntPtr rghStores;
            internal uint cPropSheetPages;
            internal IntPtr rgPropSheetPages;
            internal uint nStartPage;

#if NET7_0_OR_GREATER
            [System.Runtime.InteropServices.Marshalling.CustomMarshaller(typeof(CRYPTUI_VIEWCERTIFICATE_STRUCTW), System.Runtime.InteropServices.Marshalling.MarshalMode.Default, typeof(Marshaller))]
            public static class Marshaller
            {
                public static Native ConvertToUnmanaged(CRYPTUI_VIEWCERTIFICATE_STRUCTW managed) => new(managed);

                public static CRYPTUI_VIEWCERTIFICATE_STRUCTW ConvertToManaged(Native n) => n.ToManaged();

                public static void Free(Native native) => native.FreeNative();

                internal unsafe struct Native
                {
                    private uint dwSize;
                    private IntPtr hwndParent;
                    private uint dwFlags;
                    private IntPtr szTitle;
                    private IntPtr pCertContext;
                    private IntPtr rgszPurposes;
                    private uint cPurposes;
                    private IntPtr pCryptProviderData;
                    private bool fpCryptProviderDataTrustedUsage;
                    private uint idxSigner;
                    private uint idxCert;
                    private bool fCounterSigner;
                    private uint idxCounterSigner;
                    private uint cStores;
                    private IntPtr rghStores;
                    private uint cPropSheetPages;
                    private IntPtr rgPropSheetPages;
                    private uint nStartPage;

                    public Native(CRYPTUI_VIEWCERTIFICATE_STRUCTW managed)
                    {
                        dwSize = managed.dwSize;
                        hwndParent = managed.hwndParent;
                        dwFlags = managed.dwFlags;
                        szTitle = Marshal.StringToCoTaskMemUni(managed.szTitle);
                        pCertContext = managed.pCertContext;
                        rgszPurposes = managed.rgszPurposes;
                        cPurposes = managed.cPurposes;
                        pCryptProviderData = managed.pCryptProviderData;
                        fpCryptProviderDataTrustedUsage = managed.fpCryptProviderDataTrustedUsage;
                        idxSigner = managed.idxSigner;
                        idxCert = managed.idxCert;
                        fCounterSigner = managed.fCounterSigner;
                        idxCounterSigner = managed.idxCounterSigner;
                        cStores = managed.cStores;
                        rghStores = managed.rghStores;
                        cPropSheetPages = managed.cPropSheetPages;
                        rgPropSheetPages = managed.rgPropSheetPages;
                        nStartPage = managed.nStartPage;

                    }

                    public void FreeNative()
                    {
                        Marshal.FreeCoTaskMem(szTitle);
                    }

                    public CRYPTUI_VIEWCERTIFICATE_STRUCTW ToManaged()
                    {
                        return new()
                        {
                            dwSize = dwSize,
                            hwndParent = hwndParent,
                            dwFlags = dwFlags,
                            szTitle = Marshal.PtrToStringUni(szTitle),
                            pCertContext = pCertContext,
                            rgszPurposes = rgszPurposes,
                            cPurposes = cPurposes,
                            pCryptProviderData = pCryptProviderData,
                            fpCryptProviderDataTrustedUsage = fpCryptProviderDataTrustedUsage,
                            idxSigner = idxSigner,
                            idxCert = idxCert,
                            fCounterSigner = fCounterSigner,
                            idxCounterSigner = idxCounterSigner,
                            cStores = cStores,
                            rghStores = rghStores,
                            cPropSheetPages = cPropSheetPages,
                            rgPropSheetPages = rgPropSheetPages,
                            nStartPage = nStartPage
                        };
                    }
                }
            }
#endif
        }

#if NET7_0_OR_GREATER
        [System.Runtime.InteropServices.Marshalling.NativeMarshalling(typeof(Marshaller))]
#else
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
#endif
        internal struct CRYPTUI_SELECTCERTIFICATE_STRUCTW
        {
            internal uint dwSize;
            internal IntPtr hwndParent;
            internal uint dwFlags;
            internal string? szTitle;
            internal uint dwDontUseColumn;
            internal string? szDisplayString;
            internal IntPtr pFilterCallback;
            internal IntPtr pDisplayCallback;
            internal IntPtr pvCallbackData;
            internal uint cDisplayStores;
            internal IntPtr rghDisplayStores;
            internal uint cStores;
            internal IntPtr rghStores;
            internal uint cPropSheetPages;
            internal IntPtr rgPropSheetPages;
            internal IntPtr hSelectedCertStore;

#if NET7_0_OR_GREATER
            [System.Runtime.InteropServices.Marshalling.CustomMarshaller(typeof(CRYPTUI_SELECTCERTIFICATE_STRUCTW), System.Runtime.InteropServices.Marshalling.MarshalMode.Default, typeof(Marshaller))]
            public static class Marshaller
            {
                public static Native ConvertToUnmanaged(CRYPTUI_SELECTCERTIFICATE_STRUCTW managed) => new(managed);

                public static CRYPTUI_SELECTCERTIFICATE_STRUCTW ConvertToManaged(Native n) => n.ToManaged();

                public static void Free(Native native) => native.FreeNative();

                internal unsafe struct Native
                {
                    private uint dwSize;
                    private IntPtr hwndParent;
                    private uint dwFlags;
                    private IntPtr szTitle;
                    private uint dwDontUseColumn;
                    private IntPtr szDisplayString;
                    private IntPtr pFilterCallback;
                    private IntPtr pDisplayCallback;
                    private IntPtr pvCallbackData;
                    private uint cDisplayStores;
                    private IntPtr rghDisplayStores;
                    private uint cStores;
                    private IntPtr rghStores;
                    private uint cPropSheetPages;
                    private IntPtr rgPropSheetPages;
                    internal IntPtr hSelectedCertStore;

                    public Native(CRYPTUI_SELECTCERTIFICATE_STRUCTW managed)
                    {
                        dwSize = managed.dwSize;
                        hwndParent = managed.hwndParent;
                        dwFlags = managed.dwFlags;
                        szTitle = Marshal.StringToCoTaskMemUni(managed.szTitle);
                        dwDontUseColumn = managed.dwDontUseColumn;
                        szDisplayString = Marshal.StringToCoTaskMemUni(managed.szDisplayString);
                        pFilterCallback = managed.pFilterCallback;
                        pDisplayCallback = managed.pDisplayCallback;
                        pvCallbackData = managed.pvCallbackData;
                        cDisplayStores = managed.cDisplayStores;
                        rghDisplayStores = managed.rghDisplayStores;
                        cStores = managed.cStores;
                        rghStores = managed.rghStores;
                        cPropSheetPages = managed.cPropSheetPages;
                        rgPropSheetPages = managed.rgPropSheetPages;
                        hSelectedCertStore = managed.hSelectedCertStore;
                    }

                    public void FreeNative()
                    {
                        Marshal.FreeCoTaskMem(szTitle);
                        Marshal.FreeCoTaskMem(szDisplayString);
                    }

                    public CRYPTUI_SELECTCERTIFICATE_STRUCTW ToManaged()
                    {
                        return new()
                        {
                            dwSize = dwSize,
                            hwndParent = hwndParent,
                            dwFlags = dwFlags,
                            szTitle = Marshal.PtrToStringUni(szTitle),
                            dwDontUseColumn = dwDontUseColumn,
                            szDisplayString = Marshal.PtrToStringUni(szDisplayString),
                            pFilterCallback = pFilterCallback,
                            pDisplayCallback = pDisplayCallback,
                            pvCallbackData = pvCallbackData,
                            cDisplayStores = cDisplayStores,
                            rghDisplayStores = rghDisplayStores,
                            cStores = cStores,
                            rghStores = rghStores,
                            cPropSheetPages = cPropSheetPages,
                            rgPropSheetPages = rgPropSheetPages,
                            hSelectedCertStore = hSelectedCertStore
                        };
                    }
                }
            }
#endif
        }

#if NET7_0_OR_GREATER
        [LibraryImport(Interop.Libraries.CryptUI, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool CryptUIDlgViewCertificateW(
            in CRYPTUI_VIEWCERTIFICATE_STRUCTW ViewInfo, IntPtr pfPropertiesChanged);

        [LibraryImport(Interop.Libraries.CryptUI, SetLastError = true)]
        internal static partial SafeCertContextHandle CryptUIDlgSelectCertificateW(ref CRYPTUI_SELECTCERTIFICATE_STRUCTW csc);
#else
        [DllImport(Interop.Libraries.CryptUI, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CryptUIDlgViewCertificateW(
          in CRYPTUI_VIEWCERTIFICATE_STRUCTW ViewInfo, IntPtr pfPropertiesChanged);

        [DllImport(Interop.Libraries.CryptUI, SetLastError = true)]
        internal static extern SafeCertContextHandle CryptUIDlgSelectCertificateW(ref CRYPTUI_SELECTCERTIFICATE_STRUCTW csc);
#endif
    }

    internal static partial class Crypt32
    {
		internal const uint PKCS_7_ASN_ENCODING = 0x00010000;
        internal const uint X509_ASN_ENCODING = 0x00000001;
        internal const uint CERT_STORE_PROV_MEMORY = 2;
		internal const uint CERT_STORE_ADD_ALWAYS = 4;

#if NET7_0_OR_GREATER
        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertAddCertificateLinkToStore(SafeCertStoreHandle hCertStore, SafeCertContextHandle pCertContext, uint dwAddDisposition, SafeCertContextHandle ppStoreContext);

        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertCloseStore(IntPtr hCertStore, uint dwFlags);

        [DllImport(Libraries.Crypt32, SetLastError = true)]
        internal static extern SafeCertContextHandle CertDuplicateCertificateContext(IntPtr pCertContext);

        // Note: This api always return TRUE, regardless of success.
        [DllImport(Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertFreeCertificateContext(IntPtr pCertContext);

        [DllImport(Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertGetCertificateContextProperty(
            SafeCertContextHandle pCertContext,
            CertContextPropId dwPropId,
            byte[]? pvData,
            ref int pcbData);

        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        internal static extern IntPtr CertEnumCertificatesInStore(SafeCertStoreHandle hCertStore, IntPtr pPrevCertContext);

        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        internal static extern SafeCertStoreHandle CertOpenStore(IntPtr lpszStoreProvider, uint dwMsgAndCertEncodingType, IntPtr hCryptProv, uint dwFlags, IntPtr pvPara);
#else
        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertAddCertificateLinkToStore(SafeCertStoreHandle hCertStore, SafeCertContextHandle pCertContext, uint dwAddDisposition, SafeCertContextHandle ppStoreContext);

        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertCloseStore(IntPtr hCertStore, uint dwFlags);

        [DllImport(Libraries.Crypt32, SetLastError = true)]
        internal static extern SafeCertContextHandle CertDuplicateCertificateContext(IntPtr pCertContext);
		
		// Note: This api always return TRUE, regardless of success.
        [DllImport(Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertFreeCertificateContext(IntPtr pCertContext);
		
		[DllImport(Libraries.Crypt32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CertGetCertificateContextProperty(
            SafeCertContextHandle pCertContext,
            CertContextPropId dwPropId,
            byte[]? pvData,
            ref int pcbData);

        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        internal static extern IntPtr CertEnumCertificatesInStore(SafeCertStoreHandle hCertStore, IntPtr pPrevCertContext);

        [DllImport(Interop.Libraries.Crypt32, SetLastError = true)]
        internal static extern SafeCertStoreHandle CertOpenStore(IntPtr lpszStoreProvider, uint dwMsgAndCertEncodingType, IntPtr hCryptProv, uint dwFlags, IntPtr pvPara);
#endif

        internal enum CertContextPropId : int
        {
            CERT_KEY_PROV_INFO_PROP_ID = 2,
            CERT_SHA1_HASH_PROP_ID = 3,
            CERT_KEY_CONTEXT_PROP_ID = 5,
            CERT_FRIENDLY_NAME_PROP_ID = 11,
            CERT_ARCHIVED_PROP_ID = 19,
            CERT_KEY_IDENTIFIER_PROP_ID = 20,
            CERT_PUBKEY_ALG_PARA_PROP_ID = 22,
            CERT_OCSP_RESPONSE_PROP_ID = 70,
            CERT_NCRYPT_KEY_HANDLE_PROP_ID = 78,
            CERT_DELETE_KEYSET_PROP_ID = 101,
            CERT_CLR_DELETE_KEY_PROP_ID = 125,
        }
        
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct CERT_CONTEXT
        {
            internal MsgEncodingType dwCertEncodingType;
            internal byte* pbCertEncoded;
            internal int cbCertEncoded;
            internal CERT_INFO* pCertInfo;
            internal IntPtr hCertStore;
        }

        [Flags]
        internal enum MsgEncodingType : int
        {
            PKCS_7_ASN_ENCODING = 0x10000,
            X509_ASN_ENCODING = 0x00001,

            All = PKCS_7_ASN_ENCODING | X509_ASN_ENCODING,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CERT_INFO
        {
            internal int dwVersion;
            internal DATA_BLOB SerialNumber;
            internal CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
            internal DATA_BLOB Issuer;
            internal FILETIME NotBefore;
            internal FILETIME NotAfter;
            internal DATA_BLOB Subject;
            internal CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;
            internal CRYPT_BIT_BLOB IssuerUniqueId;
            internal CRYPT_BIT_BLOB SubjectUniqueId;
            internal int cExtension;
            internal IntPtr rgExtension;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILETIME
        {
            internal uint ftTimeLow;
            internal uint ftTimeHigh;

            internal DateTime ToDateTime()
            {
                long fileTime = (((long)ftTimeHigh) << 32) + ftTimeLow;
                return DateTime.FromFileTime(fileTime);
            }

            internal static FILETIME FromDateTime(DateTime dt)
            {
                long fileTime = dt.ToFileTime();

                unchecked
                {
                    return new FILETIME()
                    {
                        ftTimeLow = (uint)fileTime,
                        ftTimeHigh = (uint)(fileTime >> 32),
                    };
                }
            }
        }
		
		[StructLayout(LayoutKind.Sequential)]
        internal struct CERT_PUBLIC_KEY_INFO
        {
            internal CRYPT_ALGORITHM_IDENTIFIER Algorithm;
            internal CRYPT_BIT_BLOB PublicKey;
        }
		
		[StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_ALGORITHM_IDENTIFIER
        {
            internal IntPtr pszObjId;
            internal DATA_BLOB Parameters;
        }
		
		[StructLayout(LayoutKind.Sequential)]
        internal struct CRYPT_BIT_BLOB
        {
            internal int cbData;
            internal IntPtr pbData;
            internal int cUnusedBits;

            internal byte[] ToByteArray()
            {
                int numBytes = cbData;
                byte[] data = new byte[numBytes];
                Marshal.Copy(pbData, data, 0, numBytes);
                return data;
            }
        }
		
		[StructLayout(LayoutKind.Sequential)]
        internal struct DATA_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;

            internal DATA_BLOB(IntPtr handle, uint size)
            {
                cbData = size;
                pbData = handle;
            }

            internal byte[] ToByteArray()
            {
                if (cbData == 0)
                {
                    return Array.Empty<byte>();
                }

                byte[] array = new byte[cbData];
                Marshal.Copy(pbData, array, 0, (int)cbData);
                return array;
            }

            internal unsafe ReadOnlySpan<byte> DangerousAsSpan() => new ReadOnlySpan<byte>((void*)pbData, (int)cbData);
        }
    }
}