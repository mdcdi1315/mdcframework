using System;

namespace ROOT
{
    using static ROOT.MAIN;
    /// <summary>
    /// The missing members from the ROOT.MAIN class that are dependent of the deprecated in .NET 7 HTTPLIB
    /// are located here.
    /// </summary>
    public static class HTTPLIB_MAIN
    {
        /// <summary>
        /// Creates a new Windows Internet Shortcut.
        /// </summary>
        /// <param name="URL">The URL that the shortcut will point to.</param>
        /// <param name="Path">The path of the shortcut that will be saved.</param>
        /// <returns>Returns <see langword="true"/> on success; <see langword="false"/> on error.</returns>
        public static System.Boolean CreateInternetShortcut(System.Uri URL, System.String Path)
        {
            return CreateInternetShortcut(URL, Path, false);
        }

        /// <summary>
        /// Creates a new Windows Internet Shortcut.
        /// </summary>
        /// <param name="URL">The URL that the shortcut will point to.</param>
        /// <param name="Path">The path of the shortcut that will be saved.</param>
        /// <param name="OverwriteIfExists">If <see langword="true"/> , then it will delete the contents of the existing file , if exists.</param>
        /// <returns>Returns <see langword="true"/> on success; <see langword="false"/> on error.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code",
            Justification = "The code described here is NOT dead code.")]
        public static System.Boolean CreateInternetShortcut(System.Uri URL, System.String Path, System.Boolean OverwriteIfExists)
        {
            System.IO.FileStream Out = null;
            HTTPLIB.RequestBuilder req = HTTPLIB.Http.Get(URL.ToString());
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) URLFIND: {URL}");
#endif
            System.Boolean executed = false;
            System.Net.WebException err = null;
            req.OnSuccess(df => { executed = true; });
            req.OnFail(dg => { executed = true; err = dg; });
            req.Go();
            while (executed == false)
            {
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) WAITINGREP: {URL}: 130 ms");
#endif
                HaltApplicationThread(130);
            }
            req = null;
            if (err != null) { err = null; goto G_ExitErr; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) URLFIND: true");
#endif
            err = null;
            if (FileExists(Path))
            {
                if (OverwriteIfExists)
                {
                    Out = ClearAndWriteAFile(Path);
#if DEBUG
                    Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) CALL: ClearAndWriteAFile()");
#endif
                    if (Out == null) { goto G_ExitErr; }
                }
                else { goto G_ExitErr; }
            }
            else
            {
                Out = CreateANewFile(Path);
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) CALL: CreateANewFile()");
#endif
                if (Out == null) { goto G_ExitErr; }
            }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) PROCESS_ACTION");
#endif
            PassNewContentsToFile(String.Format(MDCFR.Properties.Resources.MDCFR_INTS_CREATE, URL.ToString()), Out);
            Out.Close();
            Out.Dispose();
            Out = null;
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) RESULT: Success");
#endif
            goto G_Succ;
            G_ExitErr: { if (Out != null) { Out.Close(); Out.Dispose(); } return false; }
            G_Succ: { if (Out != null) { Out.Close(); Out.Dispose(); } return true; }
        }

        /// <summary>
        /// Creates a new Windows Internet Shortcut.
        /// </summary>
        /// <param name="URL">The URL that the shortcut will point to.</param>
        /// <param name="Path">The path of the shortcut that will be saved.</param>
        /// <param name="CustomIconFile">The custom icon file or Windows executable to use for the new shortcut.</param>
        /// <param name="IconFileNumToUse">The icon index inside the icon file that contains the images to show. 
        /// If you are not sure whether the icon file only has one image , then set this parameter to zero.</param>
        /// <param name="OverwriteIfExists">If <see langword="true"/> , then it will delete the contents of the existing file , if exists.</param>
        /// <returns>Returns <see langword="true"/> on success; <see langword="false"/> on error.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability",
            "CA1508:Avoid dead conditional code", Justification = "Functions used in this class have still the possiblity to return null.")]
        public static System.Boolean CreateInternetShortcut(
            System.Uri URL, System.String Path,
            System.String CustomIconFile, System.Int16 IconFileNumToUse,
            System.Boolean OverwriteIfExists = false)
        {
            System.IO.FileStream Out = null;
            if (FileExists(CustomIconFile) == false) { goto G_ExitErr; }
#if DEBUG
            Debugger.DebuggingInfo("(in ROOT.MAIN.CreateInternetShortcut(... , ...)) ACTIVATION: Success");
#endif
            HTTPLIB.RequestBuilder req = HTTPLIB.Http.Get(URL.ToString());
            System.Boolean executed = false;
            System.Net.WebException err = null;
            req.OnSuccess(df => { executed = true; });
            req.OnFail(dg => { executed = true; err = dg; });
            req.Go();
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) URLFIND: {URL}");
#endif
            while (executed == false)
            {
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) WAITINGREP: {URL}: 130 ms");
#endif
                HaltApplicationThread(130);
            }
            req = null;
            if (err != null) { err = null; goto G_ExitErr; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) URLFIND: true");
#endif
            err = null;
            if (FileExists(Path))
            {
                if (OverwriteIfExists)
                {
                    Out = ClearAndWriteAFile(Path);
#if DEBUG
                    Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) CALL: ClearAndWriteAFile()");
#endif
                    if (Out == null) { goto G_ExitErr; }
                }
                else { goto G_ExitErr; }
            }
            else
            {
                Out = CreateANewFile(Path);
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) CALL: CreateANewFile()");
#endif
                if (Out == null) { goto G_ExitErr; }
            }
            if (IconFileNumToUse < 0) { goto G_ExitErr; }
            if (IconFileNumToUse > System.Byte.MaxValue) { goto G_ExitErr; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) PROCESS_ACTION");
#endif
            PassNewContentsToFile(System.String.Format(MDCFR.Properties.Resources.MDCFR_INTS_CREATE2,
                new System.String[] { URL.ToString(), $"{IconFileNumToUse}" ,
                    new System.IO.FileInfo(CustomIconFile).FullName}), Out);
            Out.Close();
            Out.Dispose();
            Out = null;
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) RESULT: Success");
#endif
            goto G_Succ;
        G_ExitErr: { if (Out != null) { Out.Close(); Out.Dispose(); } return false; }
        G_Succ: { if (Out != null) { Out.Close(); Out.Dispose(); } return true; }
        }
    }
}