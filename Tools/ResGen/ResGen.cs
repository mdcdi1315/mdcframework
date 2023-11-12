using System;
using System.IO;
using System.Xml;
using System.Text;
using System.CodeDom;
using System.Security;
using Microsoft.Win32;
using System.Resources;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Resources.Tools;
using System.CodeDom.Compiler;
using Microsoft.Runtime.Hosting;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]
[assembly: AssemblyTitle(".NET Command Line Resource Generator")]
[assembly: AssemblyDescription("Clone of the original ResGen.exe !!!")]
[assembly: AssemblyDefaultAlias("ResGen.exe")]
[assembly: AssemblyCompany("Microsoft Corporation Built by © MDCDI1315.")]
[assembly: AssemblyProduct("Microsoft® .NET Resource Generator")]
[assembly: AssemblyCopyright("© Microsoft Corporation.  All rights reserved.")]
[assembly: AssemblyFileVersion("4.8.4084.0")]
[assembly: AssemblyInformationalVersion("4.8.4084.0")]
[assembly: SatelliteContractVersion("4.0.0.0")]
[assembly: NeutralResourcesLanguage("en-US")]
//[assembly: AssemblySignatureKey("002400000c800000140100000602000000240000525341310008000001000100613399aff18ef1a2c2514a273a42d9042b72321" + 
//"f1757102df9ebada69923e2738406c21e5b801552ab8d200a65a235e001ac9adc25f2d811eb09496a4c6a59d4619589c69f5baf0c4179a47311d92555cd006acc8b5959" + 
//"f2bd6e10e360c34537a1d266da8085856583c85d81da7f3ec01ed9564c58d93d713cd0172c8e23a10f0239b80c96b07736f5d8b022542a4e74251a5f432824318b3539a" + 
//"5a087f8e53d2f135f9ca47f3bb2e10aff0af0849504fb7cea3ff192dc8de0edad64c68efde34c56d302ad55fd6e80f302d5efcdeae953658d3452561b5f36c542efdbdd" + 
//"9f888538d374cef106acf7d93a4445c3c73cd911f0571aaf3d54da12b11ddec375b3", "a5a866e1ee186f807668209f3b11236ace5e21f117803a3143abb126dd035d7" + 
//"d2f876b6938aaf2ee3414d5420d753621400db44a49c486ce134300a2106adb6bdb433590fef8ad5c43cba82290dc49530effd86523d9483c00f458af46890036b0e2c6" + 
//"1d077d7fbac467a506eba29e467a87198b053c749aa2a4d2840c784e6d")]
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.AssemblyDirectory)]
[assembly: AssemblyVersion("4.0.0.0")]

internal static class AssemblyRef
{
	internal const string EcmaPublicKey = "b77a5c561934e089";

	internal const string EcmaPublicKeyToken = "b77a5c561934e089";

	internal const string EcmaPublicKeyFull = "00000000000000000400000000000000";

	internal const string SilverlightPublicKey = "31bf3856ad364e35";

	internal const string SilverlightPublicKeyToken = "31bf3856ad364e35";

	internal const string SilverlightPublicKeyFull = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";

	internal const string SilverlightPlatformPublicKey = "7cec85d7bea7798e";

	internal const string SilverlightPlatformPublicKeyToken = "7cec85d7bea7798e";

	internal const string SilverlightPlatformPublicKeyFull = "00240000048000009400000006020000002400005253413100040000010001008D56C76F9E8649383049F383C44BE0EC204181822A6C31CF5EB7EF486944D032188EA1D3920763712CCB12D75FB77E9811149E6148E5D32FBAAB37611C1878DDC19E20EF135D0CB2CFF2BFEC3D115810C3D9069638FE4BE215DBF795861920E5AB6F7DB2E2CEEF136AC23D5DD2BF031700AEC232F6C6B1C785B4305C123B37AB";

	internal const string PlatformPublicKey = "b77a5c561934e089";

	internal const string PlatformPublicKeyToken = "b77a5c561934e089";

	internal const string PlatformPublicKeyFull = "00000000000000000400000000000000";

	internal const string Mscorlib = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemData = "System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemDataOracleClient = "System.Data.OracleClient, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string System = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemCore = "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemNumerics = "System.Numerics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemRuntimeRemoting = "System.Runtime.Remoting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemThreadingTasksDataflow = "System.Threading.Tasks.Dataflow, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemWindowsForms = "System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemXml = "System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string MicrosoftPublicKey = "b03f5f7f11d50a3a";

	internal const string MicrosoftPublicKeyToken = "b03f5f7f11d50a3a";

	internal const string MicrosoftPublicKeyFull = "002400000480000094000000060200000024000052534131000400000100010007D1FA57C4AED9F0A32E84AA0FAEFD0DE9E8FD6AEC8F87FB03766C834C99921EB23BE79AD9D5DCC1DD9AD236132102900B723CF980957FC4E177108FC607774F29E8320E92EA05ECE4E821C0A5EFE8F1645C4C0C93C1AB99285D622CAA652C1DFAD63D745D6F2DE5F17E5EAF0FC4963D261C8A12436518206DC093344D5AD293";

	internal const string SharedLibPublicKey = "31bf3856ad364e35";

	internal const string SharedLibPublicKeyToken = "31bf3856ad364e35";

	internal const string SharedLibPublicKeyFull = "0024000004800000940000000602000000240000525341310004000001000100B5FC90E7027F67871E773A8FDE8938C81DD402BA65B9201D60593E96C492651E889CC13F1415EBB53FAC1131AE0BD333C5EE6021672D9718EA31A8AEBD0DA0072F25D87DBA6FC90FFD598ED4DA35E44C398C454307E8E33B8426143DAEC9F596836F97C8F74750E5975C64E2189F45DEF46B2A2B1247ADC3652BF5C308055DA9";

	internal const string SystemComponentModelDataAnnotations = "System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemConfiguration = "System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemConfigurationInstall = "System.Configuration.Install, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemDeployment = "System.Deployment, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemDesign = "System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemDirectoryServices = "System.DirectoryServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemDrawingDesign = "System.Drawing.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemDrawing = "System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemEnterpriseServices = "System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemManagement = "System.Management, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemMessaging = "System.Messaging, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemNetHttp = "System.Net.Http, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemNetHttpWebRequest = "System.Net.Http.WebRequest, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemRuntimeSerializationFormattersSoap = "System.Runtime.Serialization.Formatters.Soap, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemRuntimeWindowsRuntime = "System.Runtime.WindowsRuntime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemRuntimeWindowsRuntimeUIXaml = "System.Runtime.WindowsRuntimeUIXaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemSecurity = "System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemServiceModelWeb = "System.ServiceModel.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemServiceProcess = "System.ServiceProcess, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemWeb = "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemWebAbstractions = "System.Web.Abstractions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemWebDynamicData = "System.Web.DynamicData, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemWebDynamicDataDesign = "System.Web.DynamicData.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemWebEntityDesign = "System.Web.Entity.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

	internal const string SystemWebExtensions = "System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemWebExtensionsDesign = "System.Web.Extensions.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemWebMobile = "System.Web.Mobile, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemWebRegularExpressions = "System.Web.RegularExpressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string SystemWebRouting = "System.Web.Routing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string SystemWebServices = "System.Web.Services, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string WindowsBase = "WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

	internal const string MicrosoftVisualStudio = "Microsoft.VisualStudio, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string MicrosoftVisualStudioWindowsForms = "Microsoft.VisualStudio.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string VJSharpCodeProvider = "VJSharpCodeProvider, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string ASPBrowserCapsPublicKey = "b7bd7678b977bd8f";

	internal const string ASPBrowserCapsFactory = "ASP.BrowserCapsFactory, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b7bd7678b977bd8f";

	internal const string MicrosoftVSDesigner = "Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string MicrosoftVisualStudioWeb = "Microsoft.VisualStudio.Web, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string MicrosoftWebDesign = "Microsoft.Web.Design.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string MicrosoftVSDesignerMobile = "Microsoft.VSDesigner.Mobile, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

	internal const string MicrosoftJScript = "Microsoft.JScript, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
}

internal static class CommonResStrings
{
	private static ResourceManager resmgr = new ResourceManager("CommonResStrings", Assembly.GetExecutingAssembly());

	internal static string CopyrightForCmdLine => GetString("Microsoft_Copyright_CommandLine_Logo");

	internal static string GetString(string id)
	{
		return resmgr.GetString(id);
	}
}

internal static class FXAssembly
{
	internal const string Version = "4.0.0.0";
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct GUID
{
	public int Data1;
	
	[CLSCompliant(false)]
	public ushort Data2;
	
	[CLSCompliant(false)]
	public ushort Data3;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public byte[] Data4;
}

[ComImport]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("00000101-0000-0000-C000-000000000046")]
public interface IEnumString
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	void RemoteNext([In] int celt, [MarshalAs(UnmanagedType.LPWStr)] out string rgelt, out int pceltFetched);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void Skip([In] int celt);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void Reset();

	[MethodImpl(MethodImplOptions.InternalCall)]
	void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumString ppenum);
}

[ComImport]
[Guid("79EAC9EE-BAF9-11CE-8C82-00AA004BA90B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[ComConversionLoss]
public interface IInternetSecurityManager
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	void SetSecuritySite([In][MarshalAs(UnmanagedType.Interface)] IInternetSecurityMgrSite pSite);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void GetSecuritySite([MarshalAs(UnmanagedType.Interface)] out IInternetSecurityMgrSite ppSite);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void MapUrlToZone([In][MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, out int pdwZone, [In] int dwFlags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void GetSecurityId([In][MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, out byte pbSecurityId, [In][Out] ref int pcbSecurityId, [In][ComAliasName("UrlMonTypeLib.ULONG_PTR")] int dwReserved);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void ProcessUrlAction([In][MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, [In] int dwAction, out byte pPolicy, [In] int cbPolicy, [In] ref byte pContext, [In] int cbContext, [In] int dwFlags, [In] int dwReserved);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void QueryCustomPolicy([In][MarshalAs(UnmanagedType.LPWStr)] string pwszUrl, [In][ComAliasName("UrlMonTypeLib.GUID")] ref GUID guidKey, [Out] IntPtr ppPolicy, out int pcbPolicy, [In] ref byte pContext, [In] int cbContext, [In] int dwReserved);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void SetZoneMapping([In] int dwZone, [In][MarshalAs(UnmanagedType.LPWStr)] string lpszPattern, [In] int dwFlags);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void GetZoneMappings([In] int dwZone, [MarshalAs(UnmanagedType.Interface)] out IEnumString ppenumString, [In] int dwFlags);
}


[ComImport]
[ComConversionLoss]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("79EAC9ED-BAF9-11CE-8C82-00AA004BA90B")]
public interface IInternetSecurityMgrSite
{
	[MethodImpl(MethodImplOptions.InternalCall)]
	void GetWindow([Out][ComAliasName("UrlMonTypeLib.wireHWND")] IntPtr phwnd);

	[MethodImpl(MethodImplOptions.InternalCall)]
	void EnableModeless([In] int fEnable);
}


namespace Microsoft.Runtime.Hosting
{
	[ComImport]
	[SecurityCritical]
	[ComConversionLoss]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D")]
	internal interface IClrStrongName
	{
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromAssemblyFile([In][MarshalAs(UnmanagedType.LPStr)] string pszFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromAssemblyFileW([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromBlob([In] IntPtr pbBlob, [In][MarshalAs(UnmanagedType.U4)] int cchBlob, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromFile([In][MarshalAs(UnmanagedType.LPStr)] string pszFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromFileW([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromHandle([In] IntPtr hFile, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameCompareAssemblies([In][MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly1, [In][MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly2, [MarshalAs(UnmanagedType.U4)] out int dwResult);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameFreeBuffer([In] IntPtr pbMemory);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameGetBlob([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbBlob, [In][Out][MarshalAs(UnmanagedType.U4)] ref int pcbBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameGetBlobFromImage([In] IntPtr pbBase, [In][MarshalAs(UnmanagedType.U4)] int dwLength, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbBlob, [In][Out][MarshalAs(UnmanagedType.U4)] ref int pcbBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameGetPublicKey([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameHashSize([In][MarshalAs(UnmanagedType.U4)] int ulHashAlg, [MarshalAs(UnmanagedType.U4)] out int cbSize);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyDelete([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyGen([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In][MarshalAs(UnmanagedType.U4)] int dwFlags, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyGenEx([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In][MarshalAs(UnmanagedType.U4)] int dwFlags, [In][MarshalAs(UnmanagedType.U4)] int dwKeySize, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyInstall([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameSignatureGeneration([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob, [In][Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameSignatureGenerationEx([In][MarshalAs(UnmanagedType.LPWStr)] string wszFilePath, [In][MarshalAs(UnmanagedType.LPWStr)] string wszKeyContainer, [In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob, [In][Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob, [In][MarshalAs(UnmanagedType.U4)] int dwFlags);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameSignatureSize([In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbPublicKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSize);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameSignatureVerification([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][MarshalAs(UnmanagedType.U4)] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameSignatureVerificationEx([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][MarshalAs(UnmanagedType.I1)] bool fForceVerification, [MarshalAs(UnmanagedType.I1)] out bool fWasVerified);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameSignatureVerificationFromImage([In] IntPtr pbBase, [In][MarshalAs(UnmanagedType.U4)] int dwLength, [In][MarshalAs(UnmanagedType.U4)] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameTokenFromAssembly([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameTokenFromAssemblyEx([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameTokenFromPublicKey([In][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] pbPublicKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
	}

	[ComImport]
	[SecurityCritical]
	[ComConversionLoss]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D")]
	internal interface IClrStrongNameUsingIntPtr
	{
		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromAssemblyFile([In][MarshalAs(UnmanagedType.LPStr)] string pszFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromAssemblyFileW([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromBlob([In] IntPtr pbBlob, [In][MarshalAs(UnmanagedType.U4)] int cchBlob, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromFile([In][MarshalAs(UnmanagedType.LPStr)] string pszFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromFileW([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int GetHashFromHandle([In] IntPtr hFile, [In][Out][MarshalAs(UnmanagedType.U4)] ref int piHashAlg, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbHash, [In][MarshalAs(UnmanagedType.U4)] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameCompareAssemblies([In][MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly1, [In][MarshalAs(UnmanagedType.LPWStr)] string pwzAssembly2, [MarshalAs(UnmanagedType.U4)] out int dwResult);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameFreeBuffer([In] IntPtr pbMemory);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameGetBlob([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pbBlob, [In][Out][MarshalAs(UnmanagedType.U4)] ref int pcbBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameGetBlobFromImage([In] IntPtr pbBase, [In][MarshalAs(UnmanagedType.U4)] int dwLength, [Out][MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] pbBlob, [In][Out][MarshalAs(UnmanagedType.U4)] ref int pcbBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameGetPublicKey([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In] IntPtr pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameHashSize([In][MarshalAs(UnmanagedType.U4)] int ulHashAlg, [MarshalAs(UnmanagedType.U4)] out int cbSize);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyDelete([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyGen([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In][MarshalAs(UnmanagedType.U4)] int dwFlags, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyGenEx([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In][MarshalAs(UnmanagedType.U4)] int dwFlags, [In][MarshalAs(UnmanagedType.U4)] int dwKeySize, out IntPtr ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameKeyInstall([In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In] IntPtr pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameSignatureGeneration([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][MarshalAs(UnmanagedType.LPWStr)] string pwzKeyContainer, [In] IntPtr pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob, [In][Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameSignatureGenerationEx([In][MarshalAs(UnmanagedType.LPWStr)] string wszFilePath, [In][MarshalAs(UnmanagedType.LPWStr)] string wszKeyContainer, [In] IntPtr pbKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbKeyBlob, [In][Out] IntPtr ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob, [In][MarshalAs(UnmanagedType.U4)] int dwFlags);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameSignatureSize([In] IntPtr pbPublicKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSize);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameSignatureVerification([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][MarshalAs(UnmanagedType.U4)] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameSignatureVerificationEx([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, [In][MarshalAs(UnmanagedType.I1)] bool fForceVerification, [MarshalAs(UnmanagedType.I1)] out bool fWasVerified);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.U4)]
		int StrongNameSignatureVerificationFromImage([In] IntPtr pbBase, [In][MarshalAs(UnmanagedType.U4)] int dwLength, [In][MarshalAs(UnmanagedType.U4)] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameTokenFromAssembly([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameTokenFromAssemblyEx([In][MarshalAs(UnmanagedType.LPWStr)] string pwzFilePath, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken, out IntPtr ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

		[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int StrongNameTokenFromPublicKey([In] IntPtr pbPublicKeyBlob, [In][MarshalAs(UnmanagedType.U4)] int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
	}

	internal static class StrongNameHelpers
	{
		[ThreadStatic]
		private static int ts_LastStrongNameHR;

		[SecurityCritical]
		[ThreadStatic]
		private static Microsoft.Runtime.Hosting.IClrStrongName s_StrongName;

		private static Microsoft.Runtime.Hosting.IClrStrongName StrongName
		{
			[SecurityCritical]
			get
			{
				if (s_StrongName == null)
				{
					s_StrongName = (Microsoft.Runtime.Hosting.IClrStrongName)RuntimeEnvironment.GetRuntimeInterfaceAsObject(new Guid("B79B0ACD-F5CD-409b-B5A5-A16244610B92"), new Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D"));
				}
				return s_StrongName;
			}
		}

		private static Microsoft.Runtime.Hosting.IClrStrongNameUsingIntPtr StrongNameUsingIntPtr
		{
			[SecurityCritical]
			get
			{
				return (Microsoft.Runtime.Hosting.IClrStrongNameUsingIntPtr)StrongName;
			}
		}

		[SecurityCritical]
		public static int StrongNameErrorInfo()
		{
			return ts_LastStrongNameHR;
		}

		[SecurityCritical]
		public static void StrongNameFreeBuffer(IntPtr pbMemory)
		{
			StrongNameUsingIntPtr.StrongNameFreeBuffer(pbMemory);
		}

		[SecurityCritical]
		public static bool StrongNameGetPublicKey(string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob, out IntPtr ppbPublicKeyBlob, out int pcbPublicKeyBlob)
		{
			int num = StrongNameUsingIntPtr.StrongNameGetPublicKey(pwzKeyContainer, pbKeyBlob, cbKeyBlob, out ppbPublicKeyBlob, out pcbPublicKeyBlob);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				ppbPublicKeyBlob = IntPtr.Zero;
				pcbPublicKeyBlob = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameKeyDelete(string pwzKeyContainer)
		{
			int num = StrongName.StrongNameKeyDelete(pwzKeyContainer);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameKeyGen(string pwzKeyContainer, int dwFlags, out IntPtr ppbKeyBlob, out int pcbKeyBlob)
		{
			int num = StrongName.StrongNameKeyGen(pwzKeyContainer, dwFlags, out ppbKeyBlob, out pcbKeyBlob);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				ppbKeyBlob = IntPtr.Zero;
				pcbKeyBlob = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameKeyInstall(string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob)
		{
			int num = StrongNameUsingIntPtr.StrongNameKeyInstall(pwzKeyContainer, pbKeyBlob, cbKeyBlob);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob)
		{
			IntPtr ppbSignatureBlob = IntPtr.Zero;
			int pcbSignatureBlob = 0;
			return StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, pbKeyBlob, cbKeyBlob, ref ppbSignatureBlob, out pcbSignatureBlob);
		}

		[SecurityCritical]
		public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, IntPtr pbKeyBlob, int cbKeyBlob, ref IntPtr ppbSignatureBlob, out int pcbSignatureBlob)
		{
			int num = StrongNameUsingIntPtr.StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, pbKeyBlob, cbKeyBlob, ppbSignatureBlob, out pcbSignatureBlob);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				pcbSignatureBlob = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameSignatureSize(IntPtr pbPublicKeyBlob, int cbPublicKeyBlob, out int pcbSize)
		{
			int num = StrongNameUsingIntPtr.StrongNameSignatureSize(pbPublicKeyBlob, cbPublicKeyBlob, out pcbSize);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				pcbSize = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameSignatureVerification(string pwzFilePath, int dwInFlags, out int pdwOutFlags)
		{
			int num = StrongName.StrongNameSignatureVerification(pwzFilePath, dwInFlags, out pdwOutFlags);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				pdwOutFlags = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameSignatureVerificationEx(string pwzFilePath, bool fForceVerification, out bool pfWasVerified)
		{
			int num = StrongName.StrongNameSignatureVerificationEx(pwzFilePath, fForceVerification, out pfWasVerified);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				pfWasVerified = false;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameTokenFromPublicKey(IntPtr pbPublicKeyBlob, int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, out int pcbStrongNameToken)
		{
			int num = StrongNameUsingIntPtr.StrongNameTokenFromPublicKey(pbPublicKeyBlob, cbPublicKeyBlob, out ppbStrongNameToken, out pcbStrongNameToken);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				ppbStrongNameToken = IntPtr.Zero;
				pcbStrongNameToken = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameSignatureSize(byte[] bPublicKeyBlob, int cbPublicKeyBlob, out int pcbSize)
		{
			int num = StrongName.StrongNameSignatureSize(bPublicKeyBlob, cbPublicKeyBlob, out pcbSize);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				pcbSize = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameTokenFromPublicKey(byte[] bPublicKeyBlob, int cbPublicKeyBlob, out IntPtr ppbStrongNameToken, out int pcbStrongNameToken)
		{
			int num = StrongName.StrongNameTokenFromPublicKey(bPublicKeyBlob, cbPublicKeyBlob, out ppbStrongNameToken, out pcbStrongNameToken);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				ppbStrongNameToken = IntPtr.Zero;
				pcbStrongNameToken = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameGetPublicKey(string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob, out IntPtr ppbPublicKeyBlob, out int pcbPublicKeyBlob)
		{
			int num = StrongName.StrongNameGetPublicKey(pwzKeyContainer, bKeyBlob, cbKeyBlob, out ppbPublicKeyBlob, out pcbPublicKeyBlob);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				ppbPublicKeyBlob = IntPtr.Zero;
				pcbPublicKeyBlob = 0;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameKeyInstall(string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob)
		{
			int num = StrongName.StrongNameKeyInstall(pwzKeyContainer, bKeyBlob, cbKeyBlob);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				return false;
			}
			return true;
		}

		[SecurityCritical]
		public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob)
		{
			IntPtr ppbSignatureBlob = IntPtr.Zero;
			int pcbSignatureBlob = 0;
			return StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, bKeyBlob, cbKeyBlob, ref ppbSignatureBlob, out pcbSignatureBlob);
		}

		[SecurityCritical]
		public static bool StrongNameSignatureGeneration(string pwzFilePath, string pwzKeyContainer, byte[] bKeyBlob, int cbKeyBlob, ref IntPtr ppbSignatureBlob, out int pcbSignatureBlob)
		{
			int num = StrongName.StrongNameSignatureGeneration(pwzFilePath, pwzKeyContainer, bKeyBlob, cbKeyBlob, ppbSignatureBlob, out pcbSignatureBlob);
			if (num < 0)
			{
				ts_LastStrongNameHR = num;
				pcbSignatureBlob = 0;
				return false;
			}
			return true;
		}
	}
}

namespace System
{
	internal static class ExternDll
	{
		public const string Activeds = "activeds.dll";

		public const string Advapi32 = "advapi32.dll";

		public const string Comctl32 = "comctl32.dll";

		public const string Comdlg32 = "comdlg32.dll";

		public const string Gdi32 = "gdi32.dll";

		public const string Gdiplus = "gdiplus.dll";

		public const string Hhctrl = "hhctrl.ocx";

		public const string Imm32 = "imm32.dll";

		public const string Kernel32 = "kernel32.dll";

		public const string Loadperf = "Loadperf.dll";

		public const string Mscoree = "mscoree.dll";

		public const string Clr = "clr.dll";

		public const string Msi = "msi.dll";

		public const string Mqrt = "mqrt.dll";

		public const string Ntdll = "ntdll.dll";

		public const string Ole32 = "ole32.dll";

		public const string Oleacc = "oleacc.dll";

		public const string Oleaut32 = "oleaut32.dll";

		public const string Olepro32 = "olepro32.dll";

		public const string PerfCounter = "perfcounter.dll";

		public const string Powrprof = "Powrprof.dll";

		public const string Psapi = "psapi.dll";

		public const string Shell32 = "shell32.dll";

		public const string User32 = "user32.dll";

		public const string Uxtheme = "uxtheme.dll";

		public const string WinMM = "winmm.dll";

		public const string Winspool = "winspool.drv";

		public const string Wtsapi32 = "wtsapi32.dll";

		public const string Version = "version.dll";

		public const string Vsassert = "vsassert.dll";

		public const string Fxassert = "Fxassert.dll";

		public const string Shlwapi = "shlwapi.dll";

		public const string Crypt32 = "crypt32.dll";

		public const string ShCore = "SHCore.dll";

		public const string Wldp = "wldp.dll";

		internal const string Odbc32 = "odbc32.dll";

		internal const string SNI = "System.Data.dll";

		internal const string OciDll = "oci.dll";

		internal const string OraMtsDll = "oramts.dll";

		internal const string UiaCore = "UIAutomationCore.dll";
	}

	public static class ResGen
	{
		internal sealed class ResourceClassOptions
		{
			private string _language;

			private string _nameSpace;

			private string _className;

			private string _outputFileName;

			private bool _internalClass;

			private bool _simulateVS;

			internal string Language => _language;

			internal string NameSpace => _nameSpace;

			internal string ClassName => _className;

			internal string OutputFileName => _outputFileName;

			internal bool InternalClass
			{
				get
				{
					return _internalClass;
				}
				set
				{
					_internalClass = value;
				}
			}

			internal bool SimulateVS
			{
				get
				{
					return _simulateVS;
				}
				set
				{
					_simulateVS = value;
				}
			}

			internal ResourceClassOptions(string language, string nameSpace, string className, string outputFileName, bool isClassInternal, bool simulateVS)
			{
				_language = language;
				_nameSpace = nameSpace;
				_className = className;
				_outputFileName = outputFileName;
				_internalClass = isClassInternal;
				_simulateVS = simulateVS;
			}
		}

		internal sealed class LineNumberStreamReader : StreamReader
		{
			private int _lineNumber;

			private int _col;

			internal int LineNumber => _lineNumber;

			internal int LinePosition => _col;

			internal LineNumberStreamReader(string fileName, Encoding encoding, bool detectEncoding)
				: base(fileName, encoding, detectEncoding)
			{
				_lineNumber = 1;
				_col = 0;
			}

			internal LineNumberStreamReader(Stream stream)
				: base(stream)
			{
				_lineNumber = 1;
				_col = 0;
			}

			public override int Read()
			{
				int num = base.Read();
				if (num != -1)
				{
					_col++;
					if (num == 10)
					{
						_lineNumber++;
						_col = 0;
					}
				}
				return num;
			}

			public override int Read([In][Out] char[] chars, int index, int count)
			{
				int num = base.Read(chars, index, count);
				for (int i = 0; i < num; i++)
				{
					if (chars[i + index] == '\n')
					{
						_lineNumber++;
						_col = 0;
					}
					else
					{
						_col++;
					}
				}
				return num;
			}

			public override string ReadLine()
			{
				string text = base.ReadLine();
				if (text != null)
				{
					_lineNumber++;
					_col = 0;
				}
				return text;
			}

			public override string ReadToEnd()
			{
				throw new NotImplementedException("NYI");
			}
		}

		internal sealed class TextFileException : Exception
		{
			private string _fileName;

			private int _lineNumber;

			private int _column;

			internal string FileName => _fileName;

			internal int LineNumber => _lineNumber;

			internal int LinePosition => _column;

			internal TextFileException(string message, string fileName, int lineNumber, int linePosition)
				: base(message)
			{
				_fileName = fileName;
				_lineNumber = lineNumber;
				_column = linePosition;
			}
		}

		private class ResGenRunner
		{
			internal sealed class ReaderInfo
			{
				public string outputFileName;

				public string cultureName;

				public ArrayList resources;

				public Hashtable resourcesHashTable;

				public ReaderInfo()
				{
					resources = new ArrayList();
					resourcesHashTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
				}
			}

			internal static class StrongNameHelper
			{
				private enum StrongNameFlags
				{
					ForceVerification = 1,
					AllAccess = 0x10
				}

				public static bool AssemblyIsFullySigned(Assembly assembly)
				{
					string location = assembly.Location;
					if (location == null)
					{
						throw new ArgumentException("MissingFileLocation");
					}
					int pdwOutFlags;
					return Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameSignatureVerification(location, 17, out pdwOutFlags);
				}
			}

			private List<Action> bufferedOutput = new List<Action>(2);

			private List<ReaderInfo> readers = new List<ReaderInfo>();

			private bool hadErrors;

			private const string CLSID_InternetSecurityManager = "7b8a2d94-0ac9-11d1-896c-00c04fb6bfc4";

			public const uint ZoneLocalMachine = 0u;

			public const uint ZoneIntranet = 1u;

			public const uint ZoneTrusted = 2u;

			public const uint ZoneInternet = 3u;

			public const uint ZoneUntrusted = 4u;

			private IInternetSecurityManager internetSecurityManager;

			private void AddResource(ReaderInfo reader, string name, object value, string inputFileName, int lineNumber, int linePosition)
			{
				Entry value2 = new Entry(name, value);
				if (reader.resourcesHashTable.ContainsKey(name))
				{
					Warning(SR.GetString("DuplicateResourceKey", name), inputFileName, lineNumber, linePosition);
				}
				else
				{
					reader.resources.Add(value2);
					reader.resourcesHashTable.Add(name, value);
				}
			}

			private void AddResource(ReaderInfo reader, string name, object value, string inputFileName)
			{
				Entry value2 = new Entry(name, value);
				if (reader.resourcesHashTable.ContainsKey(name))
				{
					Warning(SR.GetString("DuplicateResourceKey", name), inputFileName);
				}
				else
				{
					reader.resources.Add(value2);
					reader.resourcesHashTable.Add(name, value);
				}
			}

			private void Error(string message)
			{
				Error(message, 0);
			}

			private void Error(string message, int errorNumber)
			{
				string formatString = "ResGen : error RG{1:0000}: {0}";
				BufferErrorLine(formatString, message, errorNumber);
				Interlocked.Increment(ref errors);
				hadErrors = true;
			}

			private void Error(string message, string fileName)
			{
				Error(message, fileName, 0);
			}

			private void Error(string message, string fileName, int errorNumber)
			{
				string formatString = "{0} : error RG{1:0000}: {2}";
				BufferErrorLine(formatString, fileName, errorNumber, message);
				Interlocked.Increment(ref errors);
				hadErrors = true;
			}

			private void Error(string message, string fileName, int line, int column)
			{
				Error(message, fileName, line, column, 0);
			}

			private void Error(string message, string fileName, int line, int column, int errorNumber)
			{
				string formatString = "{0}({1},{2}): error RG{3:0000}: {4}";
				BufferErrorLine(formatString, fileName, line, column, errorNumber, message);
				Interlocked.Increment(ref errors);
				hadErrors = true;
			}

			private void Warning(string message)
			{
				string formatString = "ResGen : warning RG0000 : {0}";
				BufferErrorLine(formatString, message);
				Interlocked.Increment(ref warnings);
			}

			private void Warning(string message, string fileName)
			{
				Warning(message, fileName, 0);
			}

			private void Warning(string message, string fileName, int warningNumber)
			{
				string formatString = "{0} : warning RG{1:0000}: {2}";
				BufferErrorLine(formatString, fileName, warningNumber, message);
				Interlocked.Increment(ref warnings);
			}

			private void Warning(string message, string fileName, int line, int column)
			{
				Warning(message, fileName, line, column, 0);
			}

			private void Warning(string message, string fileName, int line, int column, int warningNumber)
			{
				string formatString = "{0}({1},{2}): warning RG{3:0000}: {4}";
				BufferErrorLine(formatString, fileName, line, column, warningNumber, message);
				Interlocked.Increment(ref warnings);
			}

			private void BufferErrorLine(string formatString, params object[] args)
			{
				bufferedOutput.Add(delegate
				{
					Console.Error.WriteLine(formatString, args);
				});
			}

			private void BufferWriteLine()
			{
				BufferWriteLine("");
			}

			private void BufferWriteLine(string formatString, params object[] args)
			{
				bufferedOutput.Add(delegate
				{
					Console.WriteLine(formatString, args);
				});
			}

			private void BufferWrite(string formatString, params object[] args)
			{
				bufferedOutput.Add(delegate
				{
					Console.Write(formatString, args);
				});
			}

			public void ProcessFile(string inFile, string outFileOrDir, ResourceClassOptions resourceClassOptions, bool useSourcePath)
			{
				ProcessFileWorker(inFile, outFileOrDir, resourceClassOptions, useSourcePath);
				lock (consoleOutputLock)
				{
					foreach (Action item in bufferedOutput)
					{
						item();
					}
				}
				if (hadErrors && outFileOrDir != null && File.Exists(outFileOrDir) && GetFormat(inFile) != Format.Assembly && GetFormat(outFileOrDir) != Format.Assembly)
				{
					GC.Collect(2);
					GC.WaitForPendingFinalizers();
					try
					{
						File.Delete(outFileOrDir);
					}
					catch
					{
					}
				}
			}

			public void ProcessFileWorker(string inFile, string outFileOrDir, ResourceClassOptions resourceClassOptions, bool useSourcePath)
			{
				try
				{
					if (!File.Exists(inFile))
					{
						Error(SR.GetString("FileNotFound", inFile));
						return;
					}
					if (GetFormat(inFile) != Format.Assembly && GetFormat(outFileOrDir) == Format.Assembly)
					{
						Error(SR.GetString("CannotWriteAssembly", outFileOrDir));
						return;
					}
					if (!ReadResources(inFile, useSourcePath))
					{
						return;
					}
				}
				catch (ArgumentException ex)
				{
					if (ex.InnerException is XmlException)
					{
						XmlException ex2 = (XmlException)ex.InnerException;
						Error(ex2.Message, inFile, ex2.LineNumber, ex2.LinePosition);
					}
					else
					{
						Error(ex.Message, inFile);
					}
					return;
				}
				catch (TextFileException ex3)
				{
					Error(ex3.Message, ex3.FileName, ex3.LineNumber, ex3.LinePosition);
					return;
				}
				catch (XmlException ex4)
				{
					Error(ex4.Message, inFile, ex4.LineNumber, ex4.LinePosition);
					return;
				}
				catch (Exception ex5)
				{
					Error(ex5.Message, inFile);
					if (ex5.InnerException != null)
					{
						Exception innerException = ex5.InnerException;
						StringBuilder stringBuilder = new StringBuilder(200);
						stringBuilder.Append(ex5.Message);
						while (innerException != null)
						{
							stringBuilder.Append(" ---> ");
							stringBuilder.Append(innerException.GetType().Name);
							stringBuilder.Append(": ");
							stringBuilder.Append(innerException.Message);
							innerException = innerException.InnerException;
						}
						Error(SR.GetString("SpecificError", ex5.InnerException.GetType().Name, stringBuilder.ToString()), inFile);
					}
					return;
				}
				string text = null;
				string text2 = null;
				string sourceFile = null;
				bool flag = true;
				try
				{
					if (GetFormat(inFile) == Format.Assembly)
					{
						foreach (ReaderInfo reader in readers)
						{
							string text3 = reader.outputFileName + ".resw";
							text = null;
							flag = true;
							text2 = Path.Combine(outFileOrDir ?? string.Empty, reader.cultureName ?? string.Empty);
							if (text2.Length == 0)
							{
								text = text3;
							}
							else
							{
								if (!Directory.Exists(text2))
								{
									flag = false;
									Directory.CreateDirectory(text2);
								}
								text = Path.Combine(text2, text3);
							}
							WriteResources(reader, text);
						}
						return;
					}
					text = outFileOrDir;
					WriteResources(readers[0], outFileOrDir);
					if (resourceClassOptions != null)
					{
						CreateStronglyTypedResources(readers[0], outFileOrDir, resourceClassOptions, inFile, out sourceFile);
					}
				}
				catch (IOException ex6)
				{
					if (text != null)
					{
						Error(SR.GetString("WriteError", text), text);
						if (ex6.Message != null)
						{
							Error(SR.GetString("SpecificError", ex6.GetType().Name, ex6.Message), text);
						}
						if (File.Exists(text) && GetFormat(text) != Format.Assembly)
						{
							RemoveCorruptedFile(text);
							if (sourceFile != null)
							{
								RemoveCorruptedFile(sourceFile);
							}
						}
					}
					if (text2 != null && !flag)
					{
						try
						{
							Directory.Delete(text2);
							return;
						}
						catch (Exception)
						{
							return;
						}
					}
				}
				catch (Exception ex8)
				{
					if (text != null)
					{
						Error(SR.GetString("GenericWriteError", text));
					}
					if (ex8.Message != null)
					{
						Error(SR.GetString("SpecificError", ex8.GetType().Name, ex8.Message));
					}
				}
			}

			private void CreateStronglyTypedResources(ReaderInfo reader, string outFile, ResourceClassOptions options, string inputFileName, out string sourceFile)
			{
				CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider(options.Language);
				string text = outFile.Substring(0, outFile.LastIndexOf('.'));
				int num = text.LastIndexOfAny(new char[3]
				{
					Path.VolumeSeparatorChar,
					Path.DirectorySeparatorChar,
					Path.AltDirectorySeparatorChar
				});
				if (num != -1)
				{
					text = text.Substring(num + 1);
				}
				string nameSpace = options.NameSpace;
				string text2 = options.ClassName;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = text;
				}
				sourceFile = options.OutputFileName;
				if (string.IsNullOrEmpty(sourceFile))
				{
					string text3 = outFile.Substring(0, outFile.LastIndexOf('.'));
					sourceFile = text3 + "." + codeDomProvider.FileExtension;
				}
				string[] unmatchable = null;
				string text4 = StronglyTypedResourceBuilder.VerifyResourceName(text2, codeDomProvider);
				if (text4 != null)
				{
					text2 = text4;
				}
				string text5;
				if (string.IsNullOrEmpty(nameSpace))
				{
					BufferWrite(SR.GetString("BeginSTRClass"), text2);
					text5 = text2;
				}
				else
				{
					BufferWrite(SR.GetString("BeginSTRClassNamespace"), nameSpace, text2);
					text5 = nameSpace + "." + text2;
				}
				if (!text.Equals(text5, StringComparison.OrdinalIgnoreCase) && outFile.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
				{
					BufferWriteLine();
					Warning(SR.GetString("ClassnameMustMatchBasename", text, text5), inputFileName);
				}
				IDictionary resourcesHashTable = reader.resourcesHashTable;
				CodeCompileUnit codeCompileUnit = StronglyTypedResourceBuilder.Create(resourcesHashTable, text2, nameSpace, codeDomProvider, options.InternalClass, out unmatchable);
				codeCompileUnit.ReferencedAssemblies.Add("System.dll");
				CodeGeneratorOptions options2 = new CodeGeneratorOptions();
				UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);
				using (TextWriter writer = new StreamWriter(sourceFile, append: false, encoding))
				{
					codeDomProvider.GenerateCodeFromCompileUnit(codeCompileUnit, writer, options2);
				}
				if (unmatchable.Length != 0)
				{
					BufferWriteLine();
					string[] array = unmatchable;
					foreach (string text6 in array)
					{
						Error(SR.GetString("UnmappableResource", text6), inputFileName);
					}
				}
				else
				{
					BufferWriteLine(SR.GetString("DoneDot"));
				}
			}

			private bool IsDangerous(string filename)
			{
				if (allowMOTW)
				{
					return false;
				}
				if (internetSecurityManager == null)
				{
					Type typeFromCLSID = Type.GetTypeFromCLSID(new Guid("7b8a2d94-0ac9-11d1-896c-00c04fb6bfc4"));
					internetSecurityManager = (IInternetSecurityManager)Activator.CreateInstance(typeFromCLSID);
				}
				int pdwZone = 0;
				internetSecurityManager.MapUrlToZone(Path.GetFullPath(filename), out pdwZone, 0);
				if ((long)pdwZone < 3L)
				{
					return false;
				}
				bool result = true;
				if (GetFormat(filename) == Format.XML)
				{
					result = false;
					FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
					XmlTextReader xmlTextReader = new XmlTextReader(fileStream);
					xmlTextReader.DtdProcessing = DtdProcessing.Ignore;
					xmlTextReader.XmlResolver = null;
					try
					{
						while (xmlTextReader.Read())
						{
							if (xmlTextReader.NodeType != XmlNodeType.Element)
							{
								continue;
							}
							string localName = xmlTextReader.LocalName;
							if (xmlTextReader.LocalName.Equals("data"))
							{
								if (xmlTextReader["mimetype"] != null)
								{
									result = true;
								}
							}
							else if (xmlTextReader.LocalName.Equals("metadata") && xmlTextReader["mimetype"] != null)
							{
								result = true;
							}
						}
					}
					catch
					{
						result = true;
					}
					fileStream.Close();
					xmlTextReader.Close();
				}
				return result;
			}

			private bool ReadResources(string filename, bool useSourcePath)
			{
				Format format = GetFormat(filename);
				if (format == Format.Assembly)
				{
					if (IsDangerous(filename))
					{
						Error(SR.GetString("MOTW", filename));
						return false;
					}
					ReadAssemblyResources(filename);
				}
				else
				{
					ReaderInfo readerInfo = new ReaderInfo();
					readers.Add(readerInfo);
					switch (format)
					{
					case Format.Text:
						ReadTextResources(readerInfo, filename);
						break;
					case Format.XML:
					{
						if (IsDangerous(filename))
						{
							Error(SR.GetString("MOTW", filename));
							return false;
						}
						ResXResourceReader resXResourceReader = null;
						resXResourceReader = ((assemblyList == null) ? new ResXResourceReader(filename) : new ResXResourceReader(filename, assemblyList.ToArray()));
						if (useSourcePath)
						{
							string fullPath = Path.GetFullPath(filename);
							resXResourceReader.BasePath = Path.GetDirectoryName(fullPath);
						}
						ReadResources(readerInfo, resXResourceReader, filename);
						break;
					}
					case Format.Binary:
						if (IsDangerous(filename))
						{
							Error(SR.GetString("MOTW", filename));
							return false;
						}
						ReadResources(readerInfo, new ResourceReader(filename), filename);
						break;
					}
					BufferWriteLine(SR.GetString("ReadIn", readerInfo.resources.Count, filename));
				}
				return true;
			}

			private void ReadResources(ReaderInfo readerInfo, IResourceReader reader, string fileName)
			{
				using (reader)
				{
					IDictionaryEnumerator enumerator = reader.GetEnumerator();
					while (enumerator.MoveNext())
					{
						string name = (string)enumerator.Key;
						object value = enumerator.Value;
						AddResource(readerInfo, name, value, fileName);
					}
				}
			}

			private void ReadTextResources(ReaderInfo reader, string fileName)
			{
				Stack<string> stack = new Stack<string>();
				bool flag = false;
				using LineNumberStreamReader lineNumberStreamReader = new LineNumberStreamReader(fileName, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), detectEncoding: true);
				StringBuilder stringBuilder = new StringBuilder(40);
				StringBuilder stringBuilder2 = new StringBuilder(120);
				int num = lineNumberStreamReader.Read();
				while (true)
				{
					switch (num)
					{
					case 10:
					case 13:
						num = lineNumberStreamReader.Read();
						continue;
					case 35:
					{
						string text = lineNumberStreamReader.ReadLine();
						if (string.IsNullOrEmpty(text))
						{
							num = lineNumberStreamReader.Read();
							continue;
						}
						if (text.StartsWith("ifdef ", StringComparison.InvariantCulture) || text.StartsWith("ifndef ", StringComparison.InvariantCulture) || text.StartsWith("if ", StringComparison.InvariantCulture) || text.StartsWith("If ", StringComparison.InvariantCulture))
						{
							string text2 = text.Substring(text.IndexOf(' ') + 1).Trim();
							for (int i = 0; i < text2.Length; i++)
							{
								if (text2[i] == '#' || text2[i] == ';')
								{
									text2 = text2.Substring(0, i).Trim();
									break;
								}
							}
							if (text[0] == 'I' && text2.EndsWith(" Then", StringComparison.InvariantCulture))
							{
								text2 = text2.Substring(0, text2.Length - 5);
							}
							if (text2.Length == 0 || text2.Contains("&") || text2.Contains("|") || text2.Contains("("))
							{
								throw new TextFileException(SR.GetString("InvalidIfdef", text2), fileName, lineNumberStreamReader.LineNumber - 1, 7);
							}
							if (text.StartsWith("ifndef", StringComparison.InvariantCulture))
							{
								text2 = "!" + text2;
							}
							stack.Push(text2);
							flag = !IfdefsAreActive(stack, definesList);
						}
						else if (text.StartsWith("endif", StringComparison.InvariantCulture) || text.StartsWith("End If", StringComparison.InvariantCulture))
						{
							if (stack.Count == 0)
							{
								throw new TextFileException(SR.GetString("UnbalancedEndifs"), fileName, lineNumberStreamReader.LineNumber - 1, 1);
							}
							stack.Pop();
							flag = !IfdefsAreActive(stack, definesList);
						}
						num = lineNumberStreamReader.Read();
						continue;
					}
					case -1:
						if (stack.Count > 0)
						{
							throw new TextFileException(SR.GetString("UnbalancedIfdefs", stack.Pop()), fileName, lineNumberStreamReader.LineNumber - 1, 1);
						}
						return;
					}
					if (!flag)
					{
						switch (num)
						{
						case 9:
						case 32:
						case 59:
							break;
						case 91:
						{
							string text3 = lineNumberStreamReader.ReadLine();
							if (text3.Equals("strings]", StringComparison.OrdinalIgnoreCase))
							{
								Warning(SR.GetString("StringsTagObsolete"), fileName, lineNumberStreamReader.LineNumber - 1, 1);
								num = lineNumberStreamReader.Read();
								continue;
							}
							throw new TextFileException(SR.GetString("INFFileBracket", text3), fileName, lineNumberStreamReader.LineNumber - 1, 1);
						}
						default:
							stringBuilder.Length = 0;
							do
							{
								switch (num)
								{
								case 10:
								case 13:
									throw new TextFileException(SR.GetString("NoEqualsWithNewLine", stringBuilder.Length, stringBuilder), fileName, lineNumberStreamReader.LineNumber, lineNumberStreamReader.LinePosition);
								default:
									goto IL_030c;
								case 61:
									break;
								}
								break;
								IL_030c:
								stringBuilder.Append((char)num);
								num = lineNumberStreamReader.Read();
							}
							while (num != -1);
							if (stringBuilder.Length == 0)
							{
								throw new TextFileException(SR.GetString("NoEquals"), fileName, lineNumberStreamReader.LineNumber, lineNumberStreamReader.LinePosition);
							}
							if (stringBuilder[stringBuilder.Length - 1] == ' ')
							{
								stringBuilder.Length--;
							}
							num = lineNumberStreamReader.Read();
							if (num == 32)
							{
								num = lineNumberStreamReader.Read();
							}
							stringBuilder2.Length = 0;
							for (; num != -1; stringBuilder2.Append((char)num), num = lineNumberStreamReader.Read())
							{
								bool flag2 = false;
								if (num == 92)
								{
									num = lineNumberStreamReader.Read();
									switch (num)
									{
									case 110:
										num = 10;
										flag2 = true;
										break;
									case 114:
										num = 13;
										flag2 = true;
										break;
									case 116:
										num = 9;
										break;
									case 34:
										num = 34;
										break;
									case 117:
									{
										char[] array = new char[4];
										int num2 = 4;
										int num3 = 0;
										while (num2 > 0)
										{
											int num4 = lineNumberStreamReader.Read(array, num3, num2);
											if (num4 == 0)
											{
												throw new TextFileException(SR.GetString("BadEscape", (char)num, stringBuilder.ToString()), fileName, lineNumberStreamReader.LineNumber, lineNumberStreamReader.LinePosition);
											}
											num3 += num4;
											num2 -= num4;
										}
										num = ushort.Parse(new string(array), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
										flag2 = num == 10 || num == 13;
										break;
									}
									default:
										throw new TextFileException(SR.GetString("BadEscape", (char)num, stringBuilder.ToString()), fileName, lineNumberStreamReader.LineNumber, lineNumberStreamReader.LinePosition);
									case 92:
										break;
									}
								}
								if (flag2)
								{
									continue;
								}
								switch (num)
								{
								case 13:
									num = lineNumberStreamReader.Read();
									switch (num)
									{
									case 10:
										num = lineNumberStreamReader.Read();
										break;
									default:
										continue;
									case -1:
										break;
									}
									break;
								case 10:
									num = lineNumberStreamReader.Read();
									break;
								default:
									continue;
								}
								break;
							}
							AddResource(reader, stringBuilder.ToString(), stringBuilder2.ToString(), fileName, lineNumberStreamReader.LineNumber, lineNumberStreamReader.LinePosition);
							continue;
						}
					}
					lineNumberStreamReader.ReadLine();
					num = lineNumberStreamReader.Read();
				}
			}

			private void WriteResources(ReaderInfo reader, string filename)
			{
				switch (GetFormat(filename))
				{
				case Format.Text:
					WriteTextResources(reader, filename);
					break;
				case Format.XML:
					WriteResources(reader, new ResXResourceWriter(filename));
					break;
				case Format.Assembly:
					Error(SR.GetString("CannotWriteAssembly", filename));
					break;
				case Format.Binary:
					WriteResources(reader, new ResourceWriter(filename));
					break;
				}
			}

			private void WriteResources(ReaderInfo reader, IResourceWriter writer)
			{
				Exception ex = null;
				try
				{
					foreach (Entry resource in reader.resources)
					{
						string name = resource.name;
						object value = resource.value;
						writer.AddResource(name, value);
					}
					BufferWrite(SR.GetString("BeginWriting"));
				}
				catch (Exception ex2)
				{
					ex = ex2;
				}
				finally
				{
					if (ex != null)
					{
						try
						{
							writer.Close();
						}
						catch (Exception)
						{
						}
						try
						{
							writer.Close();
						}
						catch (Exception)
						{
						}
						throw ex;
					}
					writer.Close();
				}
				BufferWriteLine(SR.GetString("DoneDot"));
			}

			private void WriteTextResources(ReaderInfo reader, string fileName)
			{
				using StreamWriter streamWriter = new StreamWriter(fileName, append: false, Encoding.UTF8);
				foreach (Entry resource in reader.resources)
				{
					string name = resource.name;
					object value = resource.value;
					string text = value as string;
					if (text == null)
					{
						Error(SR.GetString("OnlyString", name, value.GetType().FullName), fileName);
					}
					text = text.Replace("\\", "\\\\");
					text = text.Replace("\n", "\\n");
					text = text.Replace("\r", "\\r");
					text = text.Replace("\t", "\\t");
					streamWriter.WriteLine("{0}={1}", name, text);
				}
			}

			internal void ReadAssemblyResources(string name)
			{
				Assembly assembly = null;
				bool flag = false;
				bool flag2 = false;
				NeutralResourcesLanguageAttribute neutralResourcesLanguageAttribute = null;
				AssemblyName assemblyName = null;
				try
				{
					assembly = Assembly.UnsafeLoadFrom(name);
					assemblyName = assembly.GetName();
					CultureInfo cultureInfo = null;
					try
					{
						cultureInfo = assemblyName.CultureInfo;
					}
					catch (ArgumentException ex)
					{
						Warning(SR.GetString("CreatingCultureInfoFailed", ex.GetType().Name, ex.Message, assemblyName.ToString()));
						flag2 = true;
					}
					if (!flag2)
					{
						flag = cultureInfo.Equals(CultureInfo.InvariantCulture);
						neutralResourcesLanguageAttribute = CheckAssemblyCultureInfo(name, assemblyName, cultureInfo, assembly, flag);
					}
				}
				catch (BadImageFormatException)
				{
					Error(SR.GetString("BadImageFormat", name));
				}
				catch (Exception ex3)
				{
					Error(SR.GetString("CannotLoadAssemblyLoadFromFailed", name, ex3));
				}
				if (!(assembly != null))
				{
					return;
				}
				string[] manifestResourceNames = assembly.GetManifestResourceNames();
				CultureInfo cultureInfo2 = null;
				string text = null;
				if (!flag2)
				{
					cultureInfo2 = assemblyName.CultureInfo;
					if (!cultureInfo2.Equals(CultureInfo.InvariantCulture))
					{
						text = "." + cultureInfo2.Name + ".resources";
					}
				}
				string[] array = manifestResourceNames;
				foreach (string text2 in array)
				{
					if (!text2.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}
					if (flag)
					{
						if (CultureInfo.InvariantCulture.CompareInfo.IsSuffix(text2, ".en-US.resources"))
						{
							Error(SR.GetString("ImproperlyBuiltMainAssembly", text2, name));
							continue;
						}
					}
					else if (!flag2 && !CultureInfo.InvariantCulture.CompareInfo.IsSuffix(text2, text))
					{
						Error(SR.GetString("ImproperlyBuiltSatelliteAssembly", text2, text, name));
						continue;
					}
					try
					{
						Stream manifestResourceStream = assembly.GetManifestResourceStream(text2);
						using IResourceReader resourceReader = new ResourceReader(manifestResourceStream);
						ReaderInfo readerInfo = new ReaderInfo();
						readerInfo.outputFileName = text2.Remove(text2.Length - 10);
						if (cultureInfo2 != null && !string.IsNullOrEmpty(cultureInfo2.Name))
						{
							readerInfo.cultureName = cultureInfo2.Name;
						}
						else if (neutralResourcesLanguageAttribute != null && !string.IsNullOrEmpty(neutralResourcesLanguageAttribute.CultureName))
						{
							readerInfo.cultureName = neutralResourcesLanguageAttribute.CultureName;
							Warning(SR.GetString("NeutralityOfCultureNotPreserved", readerInfo.cultureName));
						}
						if (readerInfo.cultureName != null && readerInfo.outputFileName.EndsWith("." + readerInfo.cultureName, StringComparison.OrdinalIgnoreCase))
						{
							readerInfo.outputFileName = readerInfo.outputFileName.Remove(readerInfo.outputFileName.Length - (readerInfo.cultureName.Length + 1));
						}
						readers.Add(readerInfo);
						foreach (DictionaryEntry item in resourceReader)
						{
							AddResource(readerInfo, (string)item.Key, item.Value, text2);
						}
						BufferWriteLine(SR.GetString("ReadIn", readerInfo.resources.Count, text2));
					}
					catch (FileNotFoundException)
					{
						Error(SR.GetString("NoResourcesFileInAssembly", text2));
					}
				}
			}

			private NeutralResourcesLanguageAttribute CheckAssemblyCultureInfo(string name, AssemblyName assemblyName, CultureInfo culture, Assembly a, bool mainAssembly)
			{
				NeutralResourcesLanguageAttribute neutralResourcesLanguageAttribute = null;
				if (mainAssembly)
				{
					object[] customAttributes = a.GetCustomAttributes(typeof(NeutralResourcesLanguageAttribute), inherit: false);
					if (customAttributes.Length != 0)
					{
						neutralResourcesLanguageAttribute = (NeutralResourcesLanguageAttribute)customAttributes[0];
						if (neutralResourcesLanguageAttribute.Location != UltimateResourceFallbackLocation.Satellite && neutralResourcesLanguageAttribute.Location != 0)
						{
							Warning(SR.GetString("UnrecognizedUltimateResourceFallbackLocation", neutralResourcesLanguageAttribute.Location, name));
						}
						if (!ContainsProperlyNamedResourcesFiles(a, mainAssembly: true))
						{
							Error(SR.GetString("NoResourcesFilesInAssembly"));
						}
					}
				}
				else
				{
					if (!assemblyName.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase))
					{
						Error(SR.GetString("SatelliteOrMalformedAssembly", name, culture.Name, assemblyName.Name));
						return null;
					}
					Type[] types = a.GetTypes();
					if (types.Length != 0)
					{
						Warning(SR.GetString("SatelliteAssemblyContainsCode", name));
					}
					if (!ContainsProperlyNamedResourcesFiles(a, mainAssembly: false))
					{
						Warning(SR.GetString("SatelliteAssemblyContainsNoResourcesFile", assemblyName.CultureInfo.Name));
					}
				}
				byte[] publicKey = assemblyName.GetPublicKey();
				if (publicKey != null && publicKey.Length != 0 && !StrongNameHelper.AssemblyIsFullySigned(a))
				{
					Warning(SR.GetString("AssemblyNotFullySigned", name));
				}
				return neutralResourcesLanguageAttribute;
			}

			private static bool ContainsProperlyNamedResourcesFiles(Assembly a, bool mainAssembly)
			{
				string value = (mainAssembly ? ".resources" : (a.GetName().CultureInfo.Name + ".resources"));
				string[] manifestResourceNames = a.GetManifestResourceNames();
				foreach (string text in manifestResourceNames)
				{
					if (text.EndsWith(value, StringComparison.InvariantCultureIgnoreCase))
					{
						return true;
					}
				}
				return false;
			}
		}

		private enum Format
		{
			Text,
			XML,
			Assembly,
			Binary
		}

		private class Entry
		{
			public string name;

			public object value;

			public Entry(string name, object value)
			{
				this.name = name;
				this.value = value;
			}
		}

		private const int errorCode = -1;

		private static int errors = 0;

		private static int warnings = 0;

		private static bool allowMOTW = false;

		private static List<AssemblyName> assemblyList;

		private static List<string> definesList = new List<string>();

		private static readonly object consoleOutputLock = new object();

		private static string BadFileExtensionResourceString;

		private static void Error(string message) { Error(message, 0); }

		private static void Error(string message, int errorNumber)
		{
			string format = "ResGen : error RG{1:0000}: {0}";
			Console.Error.WriteLine(format, message, errorNumber);
			errors++;
		}

		private static void Error(string message, string fileName)
		{
			Error(message, fileName, 0);
		}

		private static void Error(string message, string fileName, int errorNumber)
		{
			string format = "{0} : error RG{1:0000}: {2}";
			Console.Error.WriteLine(format, fileName, errorNumber, message);
			errors++;
		}

		private static void Error(string message, string fileName, int line, int column)
		{
			Error(message, fileName, line, column, 0);
		}

		private static void Error(string message, string fileName, int line, int column, int errorNumber)
		{
			string format = "{0}({1},{2}): error RG{3:0000}: {4}";
			Console.Error.WriteLine(format, fileName, line, column, errorNumber, message);
			errors++;
		}

		private static void Warning(string message)
		{
			string format = "ResGen : warning RG0000 : {0}";
			Console.Error.WriteLine(format, message);
			warnings++;
		}

		private static void Warning(string message, string fileName)
		{
			Warning(message, fileName, 0);
		}

		private static void Warning(string message, string fileName, int warningNumber)
		{
			string format = "{0} : warning RG{1:0000}: {2}";
			Console.Error.WriteLine(format, fileName, warningNumber, message);
			warnings++;
		}

		private static void Warning(string message, string fileName, int line, int column)
		{
			Warning(message, fileName, line, column, 0);
		}

		private static void Warning(string message, string fileName, int line, int column, int warningNumber)
		{
			string format = "{0}({1},{2}): warning RG{3:0000}: {4}";
			Console.Error.WriteLine(format, fileName, line, column, warningNumber, message);
			warnings++;
		}

		private static Format GetFormat(string filename)
		{
			string extension = Path.GetExtension(filename);
			if (string.Compare(extension, ".txt", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(extension, ".restext", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Format.Text;
			}
			if (string.Compare(extension, ".resx", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(extension, ".resw", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Format.XML;
			}
			if (string.Compare(extension, ".resources.dll", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(extension, ".dll", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(extension, ".exe", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Format.Assembly;
			}
			if (string.Compare(extension, ".resources", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return Format.Binary;
			}
			Error(SR.GetString("UnknownFileExtension", extension, filename));
			Environment.Exit(-1);
			return Format.Text;
		}

		private static void RemoveCorruptedFile(string filename)
		{
			Error(SR.GetString("CorruptOutput", filename));
			try
			{
				File.Delete(filename);
			}
			catch (Exception)
			{
				Error(SR.GetString("DeleteOutputFileFailed", filename));
			}
		}

		private static void SetConsoleUICulture()
		{
			Thread currentThread = Thread.CurrentThread;
			currentThread.CurrentUICulture = CultureInfo.CurrentUICulture.GetConsoleFallbackUICulture();
			if (Console.OutputEncoding.CodePage != Encoding.UTF8.CodePage && Console.OutputEncoding.CodePage != currentThread.CurrentUICulture.TextInfo.OEMCodePage && Console.OutputEncoding.CodePage != currentThread.CurrentUICulture.TextInfo.ANSICodePage)
			{
				currentThread.CurrentUICulture = new CultureInfo("en-US");
			}
		}

		public static void Main(string[] args)
		{
			Environment.ExitCode = -1;
			SetConsoleUICulture();
			BadFileExtensionResourceString = "BadFileExtensionOnWindows";
			if (args.Length < 1 || args[0].Equals("-h", StringComparison.OrdinalIgnoreCase) || args[0].Equals("-?", StringComparison.OrdinalIgnoreCase) || args[0].Equals("/h", StringComparison.OrdinalIgnoreCase) || args[0].Equals("/?", StringComparison.OrdinalIgnoreCase))
			{
				Usage();
				return;
			}
			bool flag = false;
			List<string> list = new List<string>();
			foreach (string text in args)
			{
				if (text.StartsWith("@", StringComparison.OrdinalIgnoreCase))
				{
					if (flag)
					{
						Error(SR.GetString("MultipleResponseFiles"));
						break;
					}
					if (text.Length == 1)
					{
						Error(SR.GetString("MalformedResponseFileName", text));
						break;
					}
					string text2 = text.Substring(1);
					if (!ValidResponseFileName(text2))
					{
						Error(SR.GetString(BadFileExtensionResourceString, text2));
						break;
					}
					if (!File.Exists(text2))
					{
						Error(SR.GetString("ResponseFileDoesntExist", text2));
						break;
					}
					flag = true;
					try
					{
						string[] array = File.ReadAllLines(text2);
						string[] array2 = array;
						foreach (string text3 in array2)
						{
							string text4 = text3.Trim();
							if (text4.Length != 0 && !text4.StartsWith("#", StringComparison.OrdinalIgnoreCase))
							{
								if (text4.StartsWith("/compile", StringComparison.OrdinalIgnoreCase) && text4.Length > 8)
								{
									Error(SR.GetString("MalformedResponseFileEntry", text2, text4));
									break;
								}
								list.Add(text4);
							}
						}
					}
					catch (Exception ex)
					{
						Error(ex.Message, text2);
					}
				}
				else
				{
					list.Add(text);
				}
			}
			string[] inFiles = null;
			string[] outFilesOrDirs = null;
			ResourceClassOptions resourceClassOptions = null;
			int l = 0;
			bool flag2 = false;
			bool flag3 = false;
			bool useSourcePath = false;
			bool flag4 = true;
			bool simulateVS = false;
			for (; l < list.Count; l++)
			{
				if (errors != 0)
				{
					break;
				}
				if (list[l].Equals("/compile", StringComparison.OrdinalIgnoreCase))
				{
					SortedSet<string> sortedSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
					inFiles = new string[list.Count - l - 1];
					outFilesOrDirs = new string[list.Count - l - 1];
					for (int m = 0; m < inFiles.Length; m++)
					{
						inFiles[m] = list[l + 1];
						int num = inFiles[m].IndexOf(',');
						if (num != -1)
						{
							string text5 = inFiles[m];
							inFiles[m] = text5.Substring(0, num);
							if (!ValidResourceFileName(inFiles[m]))
							{
								Error(SR.GetString(BadFileExtensionResourceString, inFiles[m]));
								break;
							}
							if (num == text5.Length - 1)
							{
								Error(SR.GetString("MalformedCompileString", text5));
								inFiles = new string[0];
								break;
							}
							outFilesOrDirs[m] = text5.Substring(num + 1);
							if (GetFormat(inFiles[m]) == Format.Assembly)
							{
								Error(SR.GetString("CompileSwitchNotSupportedForAssemblies"));
								break;
							}
							if (!ValidResourceFileName(outFilesOrDirs[m]))
							{
								Error(SR.GetString(BadFileExtensionResourceString, outFilesOrDirs[m]));
								break;
							}
						}
						else
						{
							if (!ValidResourceFileName(inFiles[m]))
							{
								if (inFiles[m][0] == '/' || inFiles[m][0] == '-')
								{
									Error(SR.GetString("InvalidCommandLineSyntax", "/compile", inFiles[m]));
								}
								else
								{
									Error(SR.GetString(BadFileExtensionResourceString, inFiles[m]));
								}
								break;
							}
							string resourceFileName = GetResourceFileName(inFiles[m]);
							outFilesOrDirs[m] = resourceFileName;
						}
						string fullPath = Path.GetFullPath(outFilesOrDirs[m]);
						if (sortedSet.Contains(fullPath))
						{
							Error(SR.GetString("DuplicateOutputFilenames", fullPath));
							break;
						}
						sortedSet.Add(fullPath);
						l++;
					}
					continue;
				}
				if (list[l].StartsWith("/str:", StringComparison.OrdinalIgnoreCase))
				{
					string text6 = list[l];
					int num2 = text6.IndexOf(',', 5);
					if (num2 == -1)
					{
						num2 = text6.Length;
					}
					string language = text6.Substring(5, num2 - 5);
					string nameSpace = null;
					string className = null;
					string outputFileName = null;
					int num3 = num2 + 1;
					if (num2 < text6.Length)
					{
						num2 = text6.IndexOf(',', num3);
						if (num2 == -1)
						{
							num2 = text6.Length;
						}
					}
					if (num3 <= num2)
					{
						nameSpace = text6.Substring(num3, num2 - num3);
						if (num2 < text6.Length)
						{
							num3 = num2 + 1;
							num2 = text6.IndexOf(',', num3);
							if (num2 == -1)
							{
								num2 = text6.Length;
							}
							className = text6.Substring(num3, num2 - num3);
						}
						num3 = num2 + 1;
						if (num3 < text6.Length)
						{
							outputFileName = text6.Substring(num3, text6.Length - num3);
						}
					}
					resourceClassOptions = new ResourceClassOptions(language, nameSpace, className, outputFileName, flag4, simulateVS);
					continue;
				}
				if (list[l].StartsWith("/define:", StringComparison.OrdinalIgnoreCase) || list[l].StartsWith("-define:", StringComparison.OrdinalIgnoreCase) || list[l].StartsWith("/D:", StringComparison.OrdinalIgnoreCase) || list[l].StartsWith("-D:", StringComparison.OrdinalIgnoreCase) || list[l].StartsWith("/d:", StringComparison.OrdinalIgnoreCase) || list[l].StartsWith("-d:", StringComparison.OrdinalIgnoreCase))
				{
					string text7 = ((!list[l].StartsWith("/D:", StringComparison.OrdinalIgnoreCase) && !list[l].StartsWith("-D:", StringComparison.OrdinalIgnoreCase) && !list[l].StartsWith("/d:", StringComparison.OrdinalIgnoreCase) && !list[l].StartsWith("-d:", StringComparison.OrdinalIgnoreCase)) ? list[l].Substring(8) : list[l].Substring(3));
					string[] array3 = text7.Split(',');
					foreach (string text8 in array3)
					{
						if (text8.Length == 0 || text8.Contains("&") || text8.Contains("|") || text8.Contains("("))
						{
							Error(SR.GetString("InvalidIfdef", text8));
						}
						definesList.Add(text8);
					}
					continue;
				}
				if (list[l].StartsWith("/r:", StringComparison.OrdinalIgnoreCase) || list[l].StartsWith("-r:", StringComparison.OrdinalIgnoreCase))
				{
					string text9 = list[l];
					text9 = text9.Substring(3);
					if (assemblyList == null)
					{
						assemblyList = new List<AssemblyName>();
					}
					try
					{
						assemblyList.Add(AssemblyName.GetAssemblyName(text9));
					}
					catch (Exception ex2)
					{
						Error(SR.GetString("CantLoadAssembly", text9, ex2.GetType().Name, ex2.Message));
					}
					continue;
				}
				if (list[l].Equals("/usesourcepath", StringComparison.OrdinalIgnoreCase) || list[l].Equals("-usesourcepath", StringComparison.OrdinalIgnoreCase))
				{
					useSourcePath = true;
					continue;
				}
				if (list[l].Equals("/publicclass", StringComparison.OrdinalIgnoreCase) || list[l].Equals("-publicclass", StringComparison.OrdinalIgnoreCase))
				{
					flag4 = false;
					continue;
				}
				if (list[l].Equals("/AllowUntrustedFiles", StringComparison.OrdinalIgnoreCase) || list[l].Equals("-AllowUntrustedFiles", StringComparison.OrdinalIgnoreCase))
				{
					allowMOTW = true;
					continue;
				}
				if (ValidResourceFileName(list[l]))
				{
					if (!flag2)
					{
						inFiles = new string[1];
						inFiles[0] = list[l];
						outFilesOrDirs = new string[1];
						if (GetFormat(inFiles[0]) == Format.Assembly)
						{
							outFilesOrDirs[0] = null;
						}
						else
						{
							outFilesOrDirs[0] = GetResourceFileName(inFiles[0]);
						}
						flag2 = true;
						continue;
					}
					if (!flag3)
					{
						outFilesOrDirs[0] = list[l];
						if (GetFormat(inFiles[0]) == Format.Assembly)
						{
							if (ValidResourceFileName(outFilesOrDirs[0]))
							{
								Warning(SR.GetString("MustProvideOutputDirectoryNotFilename", outFilesOrDirs[0]));
							}
							if (!Directory.Exists(outFilesOrDirs[0]))
							{
								Error(SR.GetString("OutputDirectoryMustExist", outFilesOrDirs[0]));
							}
						}
						flag3 = true;
						continue;
					}
					Error(SR.GetString("InvalidCommandLineSyntax", "<none>", list[l]));
					break;
				}
				if (flag2 && !flag3 && GetFormat(inFiles[0]) == Format.Assembly)
				{
					outFilesOrDirs[0] = list[l];
					if (!Directory.Exists(outFilesOrDirs[0]))
					{
						Error(SR.GetString("OutputDirectoryMustExist", outFilesOrDirs[0]));
					}
					flag3 = true;
					continue;
				}
				if (list[l][0] == '/' || list[l][0] == '-')
				{
					Error(SR.GetString("BadCommandLineOption", list[l]));
				}
				else
				{
					Error(SR.GetString(BadFileExtensionResourceString, list[l]));
				}
				return;
			}
			if ((inFiles == null || inFiles.Length == 0) && errors == 0)
			{
				Usage();
				return;
			}
			if (resourceClassOptions != null)
			{
				resourceClassOptions.InternalClass = flag4;
				if (inFiles.Length > 1 && (resourceClassOptions.ClassName != null || resourceClassOptions.OutputFileName != null))
				{
					Error(SR.GetString("CompileAndSTRDontMix"));
				}
				if (GetFormat(inFiles[0]) == Format.Assembly)
				{
					Error(SR.GetString("STRSwitchNotSupportedForAssemblies"));
				}
			}
			try
			{
				object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\.NETFramework\\SDK", "AllowProcessOfUntrustedResourceFiles", null);
				if (value is string)
				{
					allowMOTW = ((string)value).Equals("true", StringComparison.OrdinalIgnoreCase);
				}
			}
			catch
			{
			}
			if (errors == 0)
			{
				Parallel.For(0, inFiles.Length, delegate(int i)
				{
					ResGenRunner resGenRunner = new ResGenRunner();
					resGenRunner.ProcessFile(inFiles[i], outFilesOrDirs[i], resourceClassOptions, useSourcePath);
				});
			}
			if (warnings != 0)
			{
				Console.Error.WriteLine(SR.GetString("WarningCount", warnings));
			}
			if (errors != 0)
			{
				Console.Error.WriteLine(SR.GetString("ErrorCount", errors));
			}
			else
			{
				Environment.ExitCode = 0;
			}
		}

		private static string GetResourceFileName(string inFile)
		{
			if (inFile == null)
			{
				return null;
			}
			int num = inFile.LastIndexOf('.');
			if (num == -1)
			{
				return null;
			}
			return inFile.Substring(0, num) + ".resources";
		}

		private static bool ValidResourceFileName(string inFile)
		{
			if (inFile == null)
			{
				return false;
			}
			if (inFile.EndsWith(".resx", StringComparison.OrdinalIgnoreCase) || inFile.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || inFile.EndsWith(".restext", StringComparison.OrdinalIgnoreCase) || inFile.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase) || inFile.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || inFile.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) || inFile.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return false;
		}

		private static bool ValidResponseFileName(string inFile)
		{
			if (inFile == null)
			{
				return false;
			}
			if (inFile.EndsWith(".rsp", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return false;
		}

		private static bool IfdefsAreActive(IEnumerable<string> searchForAll, IList<string> defines)
		{
			foreach (string item in searchForAll)
			{
				if (item[0] == '!')
				{
					if (defines.Contains(item.Substring(1)))
					{
						return false;
					}
				}
				else if (!defines.Contains(item))
				{
					return false;
				}
			}
			return true;
		}

		private static void Usage()
		{
			Console.WriteLine(SR.GetString("UsageOnWindows", "4.8.4084.0", CommonResStrings.CopyrightForCmdLine));
			Console.WriteLine(SR.GetString("ValidLanguages"));
			CompilerInfo[] allCompilerInfo = CodeDomProvider.GetAllCompilerInfo();
			for (int i = 0; i < allCompilerInfo.Length; i++)
			{
				string[] languages = allCompilerInfo[i].GetLanguages();
				if (i != 0)
				{
					Console.Write(", ");
				}
				for (int j = 0; j < languages.Length; j++)
				{
					if (j != 0)
					{
						Console.Write(", ");
					}
					Console.Write(languages[j]);
				}
			}
			Console.WriteLine();
		}
	}

	internal sealed class SR
	{
		internal const string DuplicateResourceKey = "DuplicateResourceKey";

		internal const string UnknownFileExtension = "UnknownFileExtension";

		internal const string FileNotFound = "FileNotFound";

		internal const string InvalidResX = "InvalidResX";

		internal const string WriteError = "WriteError";

		internal const string CorruptOutput = "CorruptOutput";

		internal const string DeleteOutputFileFailed = "DeleteOutputFileFailed";

		internal const string SpecificError = "SpecificError";

		internal const string GenericWriteError = "GenericWriteError";

		internal const string ErrorCount = "ErrorCount";

		internal const string WarningCount = "WarningCount";

		internal const string INFFileBracket = "INFFileBracket";

		internal const string NoEqualsWithNewLine = "NoEqualsWithNewLine";

		internal const string NoEquals = "NoEquals";

		internal const string BadFileExtensionOnWindows = "BadFileExtensionOnWindows";

		internal const string BadFileExtensionNotOnWindows = "BadFileExtensionNotOnWindows";

		internal const string MustProvideOutputDirectoryNotFilename = "MustProvideOutputDirectoryNotFilename";

		internal const string OutputDirectoryMustExist = "OutputDirectoryMustExist";

		internal const string BadCommandLineOption = "BadCommandLineOption";

		internal const string BadEscape = "BadEscape";

		internal const string NoName = "NoName";

		internal const string ReadIn = "ReadIn";

		internal const string BeginWriting = "BeginWriting";

		internal const string DoneDot = "DoneDot";

		internal const string BeginSTRClass = "BeginSTRClass";

		internal const string BeginSTRClassNamespace = "BeginSTRClassNamespace";

		internal const string MalformedCompileString = "MalformedCompileString";

		internal const string MalformedResponseFileName = "MalformedResponseFileName";

		internal const string MalformedResponseFileEntry = "MalformedResponseFileEntry";

		internal const string DuplicateOutputFilenames = "DuplicateOutputFilenames";

		internal const string MultipleResponseFiles = "MultipleResponseFiles";

		internal const string ResponseFileDoesntExist = "ResponseFileDoesntExist";

		internal const string StringsTagObsolete = "StringsTagObsolete";

		internal const string OnlyString = "OnlyString";

		internal const string UnmappableResource = "UnmappableResource";

		internal const string UsageOnWindows = "UsageOnWindows";

		internal const string UsageNotOnWindows = "UsageNotOnWindows";

		internal const string InvalidCommandLineSyntax = "InvalidCommandLineSyntax";

		internal const string CompileSwitchNotSupportedForAssemblies = "CompileSwitchNotSupportedForAssemblies";

		internal const string CompileAndSTRDontMix = "CompileAndSTRDontMix";

		internal const string STRSwitchNotSupportedForAssemblies = "STRSwitchNotSupportedForAssemblies";

		internal const string CantLoadAssembly = "CantLoadAssembly";

		internal const string ClassnameMustMatchBasename = "ClassnameMustMatchBasename";

		internal const string ValidLanguages = "ValidLanguages";

		internal const string InvalidIfdef = "InvalidIfdef";

		internal const string UnbalancedEndifs = "UnbalancedEndifs";

		internal const string UnbalancedIfdefs = "UnbalancedIfdefs";

		internal const string CannotWriteAssembly = "CannotWriteAssembly";

		internal const string CreatingCultureInfoFailed = "CreatingCultureInfoFailed";

		internal const string UnrecognizedUltimateResourceFallbackLocation = "UnrecognizedUltimateResourceFallbackLocation";

		internal const string NoResourcesFilesInAssembly = "NoResourcesFilesInAssembly";

		internal const string SatelliteOrMalformedAssembly = "SatelliteOrMalformedAssembly";

		internal const string SatelliteAssemblyContainsCode = "SatelliteAssemblyContainsCode";

		internal const string SatelliteAssemblyContainsNoResourcesFile = "SatelliteAssemblyContainsNoResourcesFile";

		internal const string AssemblyNotFullySigned = "AssemblyNotFullySigned";

		internal const string BadImageFormat = "BadImageFormat";

		internal const string ImproperlyBuiltMainAssembly = "ImproperlyBuiltMainAssembly";

		internal const string ImproperlyBuiltSatelliteAssembly = "ImproperlyBuiltSatelliteAssembly";

		internal const string NeutralityOfCultureNotPreserved = "NeutralityOfCultureNotPreserved";

		internal const string NoResourcesFileInAssembly = "NoResourcesFileInAssembly";

		internal const string MissingFileLocation = "MissingFileLocation";

		internal const string CannotLoadAssemblyLoadFromFailed = "CannotLoadAssemblyLoadFromFailed";

		internal const string MOTW = "MOTW";

		private static SR loader;

		private ResourceManager resources;

		private static CultureInfo Culture => null;

		public static ResourceManager Resources => GetLoader().resources;

		internal SR()
		{
			resources = new ResourceManager("SR", GetType().Assembly);
		}

		private static SR GetLoader()
		{
			if (loader == null)
			{
				SR value = new SR();
				Interlocked.CompareExchange(ref loader, value, null);
			}
			return loader;
		}

		public static string GetString(string name, params object[] args)
		{
			SR sR = GetLoader();
			if (sR == null)
			{
				return null;
			}
			string @string = sR.resources.GetString(name, Culture);
			if (args != null && args.Length != 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i] is string text && text.Length > 1024)
					{
						args[i] = text.Substring(0, 1021) + "...";
					}
				}
				return string.Format(CultureInfo.CurrentCulture, @string, args);
			}
			return @string;
		}

		public static string GetString(string name)
		{
			return GetLoader()?.resources.GetString(name, Culture);
		}

		public static string GetString(string name, out bool usedFallback)
		{
			usedFallback = false;
			return GetString(name);
		}

		public static object GetObject(string name)
		{
			return GetLoader()?.resources.GetObject(name, Culture);
		}
	}

	[AttributeUsage(AttributeTargets.All)]
	internal sealed class SRCategoryAttribute : CategoryAttribute
	{
		public SRCategoryAttribute(string category)
			: base(category)
		{
		}

		protected override string GetLocalizedString(string value)
		{
			return SR.GetString(value);
		}
	}

	[AttributeUsage(AttributeTargets.All)]
	internal sealed class SRDescriptionAttribute : DescriptionAttribute
	{
		private bool replaced;

		public override string Description
		{
			get
			{
				if (!replaced)
				{
					replaced = true;
					base.DescriptionValue = SR.GetString(base.Description);
				}
				return base.Description;
			}
		}

		public SRDescriptionAttribute(string description)
			: base(description)
		{
		}
	}
}
