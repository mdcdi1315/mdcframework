
// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using System;
using System.Windows.Forms;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace ROOT
{
	// A Collection Namespace which includes Microsoft's Managed code.
	// Many methods here , however , are controlled and built by me at all.

	/// <summary>
	/// Contains a lot and different static methods for different usages.
	/// </summary>
	public static class MAIN
	{

        /// <summary>
        /// This property is filled out when any of the <see cref="MAIN"/> class functions called failed for a reason.
        /// This is done so as to be given more information about 'invisible exceptions'.
        /// </summary>
        public static System.Exception ExceptionData { get; set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static System.String ToURL(SystemLinks link)
		{
			return link switch
			{
				SystemLinks.Settings => MDCFR.Properties.Resources.MDCFR_LINK_WINSETTINGS,
				SystemLinks.Store => MDCFR.Properties.Resources.MDCFR_LINK_STORE,
				SystemLinks.ActionBar => MDCFR.Properties.Resources.MDCFR_LINK_ACTIONBAR,
				SystemLinks.FastSettings => MDCFR.Properties.Resources.MDCFR_LINK_ACTIONBAR2,
				SystemLinks.WindowsSecurity => MDCFR.Properties.Resources.MDCFR_LINK_WINDEFENDER,
				SystemLinks.MailApp => MDCFR.Properties.Resources.MDCFR_LINK_MAIL,
				SystemLinks.Calendar => MDCFR.Properties.Resources.MDCFR_LINK_CALENDAR,
				SystemLinks.GetStarted => MDCFR.Properties.Resources.MDCFR_LINK_GETSTARTED,
				SystemLinks.MapsApp => MDCFR.Properties.Resources.MDCFR_LINK_MAPS,
				SystemLinks.PhoneLink => MDCFR.Properties.Resources.MDCFR_LINK_PHONELINK,
				SystemLinks.ScreenSnippingTool => MDCFR.Properties.Resources.MDCFR_LINK_SCREENCLIP,
				SystemLinks.Camera => MDCFR.Properties.Resources.MDCFR_LINK_CAMERA,
				SystemLinks.MusicApp => MDCFR.Properties.Resources.MDCFR_LINK_MUSIC,
				SystemLinks.Calculator => MDCFR.Properties.Resources.MDCFR_LINK_CALC,
				SystemLinks.PeopleApp => MDCFR.Properties.Resources.MDCFR_LINK_PEOPLE,
				SystemLinks.ClockApp => MDCFR.Properties.Resources.MDCFR_LINK_CLOCK,
				_ => null,
			};
		}

		[SupportedOSPlatform("windows")]
		private static System.Boolean StartUWPApp(System.String URL)
		{
			return Interop.Shell32.ExecuteApp(System.IntPtr.Zero,
				Interop.Shell32.ExecuteVerbs.Open, URL, "", null, 9) >= 32;
		}

		[SupportedOSPlatform("windows")]
		private static System.Boolean StartUWPApp(System.String URL, IWin32Window window)
		{
			return Interop.Shell32.ExecuteApp(window.Handle,
				Interop.Shell32.ExecuteVerbs.Open, URL, "", null, 9) >= 32;
		}

		/// <summary>
		/// Opens a Windows UWP App.
		/// </summary>
		/// <param name="link">One of the values of the <see cref="SystemLinks"/> enumeration.</param>
		/// <returns>A <see cref="System.Boolean"/> determining whether the 
		/// specified UWP App was opened or restored.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean OpenSystemApp(SystemLinks link) { return StartUWPApp(ToURL(link)); }

		/// <summary>
		/// Calculates the power of an interger.
		/// </summary>
		/// <param name="Base">The base value to be raised.</param>
		/// <param name="exponent">The value that specifies the power.</param>
		/// <returns>The number <paramref name="Base"/> raised to the <paramref name="exponent"/> power.</returns>
        public static System.Int64 Power(System.Int64 Base, System.Int64 exponent)
        {
            System.Int64 Result = 1;
            for (System.Int64 I = 1; I < exponent; I++) { Result *= Base; }
            return Result;
        }

		/// <summary>
		/// Calculates the absolute value of an number.
		/// </summary>
		/// <param name="Number">The number to find the absolute value for.</param>
		/// <returns>The absolute value of <paramref name="Number"/> .</returns>
        public static System.Int64 Absolute(System.Int64 Number)
        {
            System.Int64 Result;
            if (Number < 0) { Result = Number * (-1); } else { Result = Number; }
			if (Number == System.Int64.MinValue) { Result = System.Int64.MaxValue; }
            return Result;
        }

		/// <summary>
		/// Finds the larger number from two numbers.
		/// </summary>
		/// <param name="one">The first one number.</param>
		/// <param name="two">The second one number.</param>
		/// <returns>The larger number of the two numbers.</returns>
        public static System.Int64 Max(System.Int64 one, System.Int64 two)
        {
            System.Int64 Result;
            if (one > two) { Result = one; } else if (two > one) { Result = two; } else { Result = one; }
            return Result;
        }

        /// <summary>
        /// Finds the smaller number from two numbers.
        /// </summary>
        /// <param name="One">The first one number.</param>
        /// <param name="Two">The second one number.</param>
        /// <returns>The smaller number of the two numbers.</returns>
        public static System.Int64 Min(System.Int64 One, System.Int64 Two)
        {
            System.Int64 Result;
            if (Two < One) { Result = Two; } else if (One < Two) { Result = One; } else { Result = One; }
            return Result;
        }

        /// <summary>
        /// Finds the remainder from a division operation.
        /// </summary>
        /// <param name="One">The first number to divide.</param>
        /// <param name="Two">The second number to divide.</param>
        /// <returns>The remainder of the two divided numbers.</returns>
        public static System.Int64 Mod(System.Double One, System.Double Two) { return System.Convert.ToInt64(One % Two); }

        /// <summary>
        /// Finds the quotient from a division operation.
        /// </summary>
        /// <param name="One">The first number to divide.</param>
        /// <param name="Two">The second number to divide.</param>
        /// <returns>The quotient of the two divided numbers.</returns>
        public static System.Int64 Div(System.Double One, System.Double Two) { return System.Convert.ToInt64(One / Two); }

        /// <summary>
        /// Opens a Windows UWP App.
        /// </summary>
        /// <param name="link">One of the values of the <see cref="SystemLinks"/> enumeration.</param>
        /// <param name="Window">The function will show messages to the specified window , if needed.</param>
        /// <returns>A <see cref="System.Boolean"/> determining whether the 
        /// specified UWP App was opened or restored.</returns>
        [SupportedOSPlatform("windows")]
		public static System.Boolean OpenSystemApp(SystemLinks link, IWin32Window Window) { return StartUWPApp(ToURL(link), Window); }

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
					new System.IO.FileInfo(CustomIconFile).FullName}) , Out);
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
		/// Opens a Windows UWP App from a custom link.
		/// </summary>
		/// <param name="link">The custom link that points to an UWP app that does 
		/// not exist in the <see cref="SystemLinks"/> eumeration.</param>
		/// <returns>A <see cref="System.Boolean"/> determining whether the 
		/// specified UWP App was opened or restored.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean OpenSystemApp(System.String link)
		{
			if (link.EndsWith("://") == false) { return false; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.OpenSystemApp(... , ...) ACTIVATION: Success)");
#endif
            return StartUWPApp(link);
		}

		/// <summary>
		/// Opens a Windows UWP App from a custom link.
		/// </summary>
		/// <param name="link">The custom link that points to an UWP app that does 
		/// not exist in the <see cref="SystemLinks"/> eumeration.</param>
		/// <param name="Window">The function will show messages to the 
		/// specified window , if needed.</param>
		/// <returns>A <see cref="System.Boolean"/> determining whether the 
		/// specified UWP App was opened or restored.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean OpenSystemApp(System.String link, IWin32Window Window)
		{
			if (link.EndsWith("://") == false) { return false; }
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.OpenSystemApp(... , ...) ACTIVATION: Success)");
#endif
			return StartUWPApp(link, Window);
		}

		/// <summary>
		/// Gets from an instance of the <see cref="DialogsReturner"/> class the file path given by the dialog.
		/// </summary>
		/// <param name="DG">The <see cref="DialogsReturner"/> class instance to get data from.</param>
		/// <returns>The full file path returned by the dialog.</returns>
		public static System.String GetFilePathFromInvokedDialog(DialogsReturner DG) { return DG.FileNameFullPath; }

		/// <summary>
		/// Gets from an instance of the <see cref="DialogsReturner"/> class if the dialog was executed sucessfully and a path was got.
		/// </summary>
		/// <param name="DG">The <see cref="DialogsReturner"/> class instance to get data from.</param>
		/// <returns>A <see cref="System.Boolean"/> value indicating whether the dialog execution 
		/// was sucessfull; <c>false</c> in the case of error or the user did not supplied a file path.</returns>
		public static System.Boolean GetLastErrorFromInvokedDialog(DialogsReturner DG) { if (DG.ErrorCode == "Error") { return false; } else { return true; } }

		/// <summary>
		/// A static class which creates console colored messages to differentiate the types of errors or information given.
		/// </summary>
		public static class IntuitiveConsoleText
		{
			/// <summary>
			/// This writes the data specified on the console. These data are informational. The background is black and the foreground is gray.
			/// </summary>
			/// <param name="Text">The <see cref="System.String"/> to write to the console.</param>
			public static void InfoText(System.String Text)
			{
				if (ROOT.ConsoleExtensions.Detached == true) { return; }
				System.ConsoleColor FORE, BACK;
				FORE = ConsoleExtensions.ForegroundColor;
				BACK = ConsoleExtensions.BackgroundColor;
				ConsoleExtensions.InitIfNotInitOut();
				ConsoleExtensions.ForegroundColor = System.ConsoleColor.Gray;
				ConsoleExtensions.BackgroundColor = System.ConsoleColor.Black;
				WriteConsoleText(@"INFO: " + Text);
				ConsoleExtensions.ForegroundColor = FORE;
				ConsoleExtensions.BackgroundColor = BACK;
			}

			/// <summary>
			/// This writes the data specified on the console. These data are warnings. The background is black and the foreground is yellow.
			/// </summary>
			/// <param name="Text">The <see cref="System.String"/> to write to the console.</param>
			public static void WarningText(System.String Text)
			{
				if (ROOT.ConsoleExtensions.Detached == true) { return; }
				System.ConsoleColor FORE, BACK;
				FORE = ConsoleExtensions.ForegroundColor;
				BACK = ConsoleExtensions.BackgroundColor;
				ConsoleExtensions.InitIfNotInitOut();
				ConsoleExtensions.ForegroundColor = System.ConsoleColor.Yellow;
				ConsoleExtensions.BackgroundColor = System.ConsoleColor.Black;
				WriteConsoleText(@"WARNING: " + Text);
				ConsoleExtensions.ForegroundColor = FORE;
				ConsoleExtensions.BackgroundColor = BACK;
			}

			/// <summary>
			/// This writes the data specified on the console. These data are errors. The background is black and the foreground is red.
			/// </summary>
			/// <param name="Text">The <see cref="System.String"/> to write to the console.</param>
			public static void ErrorText(System.String Text)
			{
				if (ROOT.ConsoleExtensions.Detached == true) { return; }
				System.ConsoleColor FORE, BACK;
				FORE = ConsoleExtensions.ForegroundColor;
				BACK = ConsoleExtensions.BackgroundColor;
				ConsoleExtensions.InitIfNotInitOut();
				ConsoleExtensions.ForegroundColor = System.ConsoleColor.Red;
				ConsoleExtensions.BackgroundColor = System.ConsoleColor.Black;
				WriteConsoleText(@"ERROR: " + Text);
				ConsoleExtensions.ForegroundColor = FORE;
				ConsoleExtensions.BackgroundColor = BACK;
			}

			/// <summary>
			/// This writes the data specified on the console. These data are fatal errors. The background is black and the foreground is magenta.
			/// </summary>
			/// <param name="Text">The <see cref="System.String"/> to write to the console.</param>
			public static void FatalText(System.String Text)
			{
				if (ROOT.ConsoleExtensions.Detached == true) { return; }
				System.ConsoleColor FORE, BACK;
				FORE = ConsoleExtensions.ForegroundColor;
				BACK = ConsoleExtensions.BackgroundColor;
				ConsoleExtensions.InitIfNotInitOut();
				ConsoleExtensions.ForegroundColor = System.ConsoleColor.Magenta;
				ConsoleExtensions.BackgroundColor = System.ConsoleColor.Black;
				WriteConsoleText(@"FATAL: " + Text);
				ConsoleExtensions.ForegroundColor = FORE;
				ConsoleExtensions.BackgroundColor = BACK;
			}

		}

		/// <summary>
		/// This method plays a sound to the computer.
		/// </summary>
		[SupportedOSPlatform("windows")]
		public static void EmitBeepSound() { Interop.Kernel32.ConsoleBeep(800, 200); }

		/// <summary>
		/// This is a custom implementation of <see cref="System.Console.WriteLine()"/> which writes data to the console.
		/// </summary>
		/// <param name="Text">The <see cref="System.String"/> data to write to the console.</param>
		/// <returns>A <see cref="System.Boolean"/> value whether the native function suceeded.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean WriteConsoleText(System.String Text)
		{ return global::ConsoleInterop.WriteToConsole($"{Text}\r\n"); }

		/// <summary>
		/// Detaches the running console.
		/// </summary>
		/// <returns>A <see cref="System.Boolean"/> value indicating if this method was executed sucessfully.</returns>
		public static System.Boolean DetachConsole()
		{
			if (ConsoleInterop.DetachConsole() != 0) { ConsoleExtensions.Detached = true; return false; } else { return true; }
		}

		/// <summary>
		/// Attach the current application to the specified console PID.
		/// </summary>
		/// <param name="ConsolePID">The Console's PID to attach to. If not defined , it will try to attach to the parent process console ,
		/// if it exists and has spawned a console.</param>
		/// <returns>A <see cref="System.Boolean"/> value indicating if this method was executed sucessfully.</returns>
		public static System.Boolean AttachToConsole(System.Int32 ConsolePID = -1)
		{
			if (ConsoleInterop.AttachToConsole(ConsolePID) != 0) { ConsoleExtensions.Detached = false; return true; } else { return false; }
		}

        /// <summary>
        /// Create (allocate) a new console for the current process.
        /// </summary>
        /// <returns>A <see cref="System.Boolean"/> value indicating if this method was executed sucessfully.</returns>
        public static System.Boolean CreateConsole()
		{
			if (ConsoleInterop.CreateConsole() != 0) { ConsoleExtensions.Detached = false; return true; } else { return false; }
		}

		/// <summary>
		/// Read a text line from console. This uses a custom inplementation so as to get the data.
		/// </summary>
		/// <param name="Opt">The Buffer Size to set. If left undefined , then it is <see cref="ConsoleExtensions.ConsoleReadBufferOptions.Default"/></param>
		/// <returns>The data read from the console. If any error found , then it will return the <c>"Error"</c> <see cref="System.String"/> .</returns>
		[SupportedOSPlatform("windows")]
		public static System.String ReadConsoleText(ROOT.ConsoleExtensions.ConsoleReadBufferOptions Opt =
			ROOT.ConsoleExtensions.ConsoleReadBufferOptions.Default)
		{ return global::ConsoleInterop.ReadFromConsole(Opt); }

		/// <summary>
		/// This writes a <see cref="System.Char"/>[] to the console. <see cref="System.Console.WriteLine()"/> also 
		/// contains such a method , but this one is different and has no any relationship with that one.
		/// </summary>
		/// <param name="Text">The <see cref="System.Char"/>[] data to write.</param>
		/// <returns>A <see cref="System.Boolean"/> value whether the native function suceeded.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean WriteConsoleText(System.Char[] Text)
		{
			List<System.Char> LST = new List<System.Char>(Text) { '\r', '\n' };
			System.Boolean DS = ConsoleInterop.WriteToConsole(LST.ToArray());
			LST.Clear();
			LST = null;
			return DS;
		}

		/// <summary>
		/// This writes a custom colored text to console and returns to the current console color settings after written.
		/// </summary>
		/// <param name="Message">The <see cref="System.String"/> data to write to.</param>
		/// <param name="ForegroundColor">The <see cref="System.ConsoleColor"/> enumeration value that represents the foreground color.</param>
		/// <param name="BackgroundColor">The <see cref="System.ConsoleColor"/> enumeration value that represents the background color.</param>
		[SupportedOSPlatform("windows")]
		public static void WriteCustomColoredText(System.String Message, System.ConsoleColor ForegroundColor, System.ConsoleColor BackgroundColor)
		{
			System.ConsoleColor FORE, BACK;
			FORE = ConsoleExtensions.ForegroundColor;
			BACK = ConsoleExtensions.BackgroundColor;
			ConsoleExtensions.InitIfNotInitOut();
			ConsoleExtensions.SetForeColor(ForegroundColor);
			ConsoleExtensions.SetBackColor(BackgroundColor);
			ConsoleInterop.WriteToConsole($"{Message}\r\n");
			ConsoleExtensions.SetForeColor(FORE);
			ConsoleExtensions.SetBackColor(BACK);
		}

#if NET472_OR_GREATER

		/// <summary>
		/// (For informational purposes only) The CLR version that the application called this method runs.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that describes the current CLR version.</returns>
		public static System.String GetRuntimeVersion() { return System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion(); }

		/// <summary>
		/// Check if the application is run on a specific platform.
		/// </summary>
		/// <param name="OSP">One of the <see cref="System.Runtime.InteropServices.OSPlatform"/> enumeration values to check against the current. </param>
		/// <returns><c>true</c> if the <paramref name="OSP"/> value is matching the current platform , otherwise , <c>false</c>.</returns>
		public static System.Boolean CheckIfStartedFromSpecifiedOS(System.Runtime.InteropServices.OSPlatform OSP)
		{ return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSP); }

		/// <summary>
		/// (For informational purposes only) The OS information that the .NET runtime is run to.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that describes this OS infromation.</returns>
		public static System.String OSInformation() { return System.Runtime.InteropServices.RuntimeInformation.OSDescription; }

		/// <summary>
		/// (For informational purposes only) The OS .NET Runtime Specific information.
		/// </summary>
		/// <returns>A <see cref="System.String"/> describing the OS .NET Runtime information.</returns>
		public static System.String OSFramework() { return System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription; }

		/// <summary>
		/// This method returns the computer's processor architecture.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that can return the values: <c>"x86"</c> for Unicode 32-bit machines , 
		/// <c>"AMD64"</c> for Unicode 64-Bit machines , <c>"ARM"</c> for ARM 32-Bit machines , and <c>"ARM64"</c> for ARM 64-Bit
		/// machines. Otherwise , <c>"Error"</c>.</returns>
		public static System.String OSProcessorArchitecture()
		{
			System.Runtime.InteropServices.Architecture RRD = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
#if DEBUG
            Debugger.DebuggingInfo($"(in OSProcessorArchitecture()) VALUE: {RRD}");
#endif
            if (System.Runtime.InteropServices.Architecture.X86 == RRD) { return "x86"; }
			else if (System.Runtime.InteropServices.Architecture.X64 == RRD) { return "AMD64"; }
			else if (System.Runtime.InteropServices.Architecture.Arm == RRD) { return "ARM"; }
			else if (System.Runtime.InteropServices.Architecture.Arm64 == RRD) { return "ARM64"; } else { return "Error"; }
		}
		
		/// <summary>
		/// This method returns the application's compiled platform.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that can return the values: <c>"x86"</c> for Unicode 32-bit processes , 
		/// <c>"AMD64"</c> for Unicode 64-Bit processes , <c>"ARM"</c> for ARM 32-Bit processes , and <c>"ARM64"</c> for ARM 64-Bit
		/// processes. Otherwise , <c>"Error"</c>.</returns>
		public static System.String ProcessArchitecture()
		{
			System.Runtime.InteropServices.Architecture RRD = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
#if DEBUG
			Debugger.DebuggingInfo($"(in ProcessArchitecture()) VALUE: {RRD}");
#endif
			if (System.Runtime.InteropServices.Architecture.X86 == RRD) { return "x86"; }
			else if (System.Runtime.InteropServices.Architecture.X64 == RRD) { return "AMD64"; }
			else if (System.Runtime.InteropServices.Architecture.Arm == RRD) { return "ARM"; }
			else if (System.Runtime.InteropServices.Architecture.Arm64 == RRD) { return "ARM64"; } else { return "Error"; }
		}

#endif

		/// <summary>
		/// Creates a new Link to any file.
		/// </summary>
		/// <param name="PathToSave">Path of the link to be saved.</param>
		/// <param name="PathToPoint">Path that will point to the link saved.</param>
		/// <param name="WorkingDirectory">The working directory that the link will open to.</param>
		/// <returns><see langword="true"/> if the command succeeded; otherwise , <see langword="false"/>. </returns>
		public static System.Boolean CreateLink(System.String PathToSave , System.String PathToPoint , 
			System.String WorkingDirectory = "") 
		{
			try
			{
				return FileInterop.CreateNewLink(PathToSave, PathToPoint, WorkingDirectory);
			} catch (System.Exception e) { ExceptionData = e; return false; }
		}

		/// <summary>
		/// Determines whether the caller has administrative permissions 
		/// (That is , the caller was launched with the 'Run As Administrator' option).
		/// </summary>
		/// <returns>A <see cref="System.Boolean"/> value determining whether the caller has administrative priviledges.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean RunsAsAdmin()
		{
			System.Security.Principal.WindowsPrincipal DI = new(System.Security.Principal.WindowsIdentity.GetCurrent());
			return DI.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
		}

		/// <summary>
		/// Change in a <see cref="System.String"/> the defined character with another one.
		/// </summary>
		/// <param name="Stringtoreplacechar">The <see cref="System.String"/> data to take from.</param>
		/// <param name="Chartochange">The character that will be changed.</param>
		/// <param name="Chartobechanged">The character that will replace the <paramref name="Chartochange"/> character.</param>
		/// <returns>The <see cref="System.String"/> given to <paramref name="Stringtoreplacechar"/> ,
		/// but with the defined character changed as specified by the <paramref name="Chartobechanged"/> parameter.</returns>
		public static System.String ChangeDefinedChar(System.String Stringtoreplacechar,
			System.Char Chartochange, System.Char Chartobechanged)
		{
			System.Char[] array = Stringtoreplacechar.ToCharArray();
			if (array == null || array.Length < 1) { return "Error"; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.ChangeDefinedChar(... , {Chartochange} , {Chartobechanged})) VALIDARRAY: true");
#endif
            System.String Result = null;
			for (System.Int32 I = 0; I < array.Length; I++)
			{
				if (array[I] == Chartochange)
				{
					Result += Chartobechanged;
				}
				else { Result += array[I]; }
			}
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.ChangeDefinedChar(... , {Chartochange} , {Chartobechanged})) RESULT: Success");
#endif
            return Result;
		}

		/// <summary>
		/// The method returns a new <see cref="System.String"/> defined from the <paramref name="StringToClear"/> parameter , 
		/// but it's characters removed defined by the <paramref name="CharToClear"/> parameter.
		/// </summary>
		/// <param name="StringToClear">The <see cref="System.String"/> which the characters will be removed from.</param>
		/// <param name="CharToClear">The <see cref="System.Char"/>[] array which defines which characters will be removed.
		/// The array can also have only one entry (character) to remove from the <see cref="System.String"/>.</param>
		/// <returns>A new <see cref="System.String"/> which is the 
		/// <paramref name="StringToClear"/> but with the defined characters removed.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.String RemoveDefinedChars(System.String StringToClear, params System.Char[] CharToClear)
		{
			System.Char[] CharString = StringToClear.ToCharArray();
			if (CharString.Length <= 0) { return null; }
			if (CharToClear.Length <= 0) { return null; }
#if DEBUG
			Debugger.DebuggingInfo("(in ROOT.MAIN.RemoveDefinedChars(... , ...)) VALIDARRAY: true");
#endif
			System.Boolean Keeper = false;
			System.String Result = null;
			for (System.Int32 ITER = 0; ITER < CharString.Length; ITER++)
			{
				for (System.Int32 ILGITER = 0; ILGITER < CharToClear.Length; ILGITER++)
				{
					if (CharString[ITER] == CharToClear[ILGITER]) { Keeper = true; break; }
				}
				if (Keeper == false) { Result += CharString[ITER]; } else { Keeper = false; }
			}
			CharString = null;
			CharToClear = null;
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.RemoveDefinedChars(... , ...)) RESULT: Success");
#endif
            return Result;
		}


		/// <summary>
		/// This method requests from the user to supply a <see cref="System.String"/> and return the result to the caller.
		/// </summary>
		/// <param name="Prompt">How to prompt the user so as to type the correct <see cref="System.String"/> needed.</param>
		/// <param name="Title">The window's title.</param>
		/// <param name="DefaultResponse">The default response or an example on what the user should type.</param>
		/// <returns>If the user wrote something in the box and pressed 'OK' , then the <see cref="System.String"/> that supplied; otherwise , <c>null</c>.</returns>
		/// <remarks>Note: This uses the <see cref="ROOT.IntuitiveInteraction.GetAStringFromTheUser"/> class instead of the 
		/// Microsoft.VisualBasic.Interaction.InputBox() method.</remarks>
		[SupportedOSPlatform("windows")]
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.String GetAStringFromTheUserNew(System.String Prompt,
		System.String Title, System.String DefaultResponse)
		{
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.GetAStringFromTheUserNew({Prompt} , {Title} , {DefaultResponse})) CREATE: Dialog");
#endif
			IntuitiveInteraction.GetAStringFromTheUser DZ = new(Prompt, Title, DefaultResponse);
			switch (DZ.ButtonClicked)
			{
				case ROOT.IntuitiveInteraction.ButtonReturned.NotAnAnswer:
					return null;
				default:
#if DEBUG
                    Debugger.DebuggingInfo($"(in ROOT.MAIN.GetAStringFromTheUserNew({Prompt} , {Title} , {DefaultResponse})) RESULT: {DZ.ValueReturned}");
#endif
                    return DZ.ValueReturned;
			}
		}

		/// <summary>
		/// Checks whether a file exists or not by the <paramref name="Path"/> supplied.
		/// </summary>
		/// <param name="Path">The <see cref="System.String"/> which is a filepath to check if the file exists.</param>
		/// <returns>If the file exists in the <paramref name="Path"/> supplied , then <c>true</c>; otherwise <c>false</c>.</returns>
		public static System.Boolean FileExists(System.String Path) 
		{
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.FileExists({Path})) EXISTS: {FileInterop.FileExists(Path)}");
#endif
			return FileInterop.FileExists(Path); 
		}

		/// <summary>
		/// Checks whether a directory exists or not by the <paramref name="Path"/> supplied.
		/// </summary>
		/// <param name="Path">The <see cref="System.String"/> which is a directory path to check if the directory exists.</param>
		/// <returns>If the directory exists in the <paramref name="Path"/> supplied , then <c>true</c>; otherwise <c>false</c>.</returns>
		public static System.Boolean DirExists(System.String Path) 
		{
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.DirExists({Path}) EXISTS: {System.IO.Directory.Exists(Path)})");
#endif
			if (System.IO.Directory.Exists(Path)) { return true; } else { return false; } 
		}

		/// <summary>
		/// Creates a directory specified by the <paramref name="Path"/> parameter.
		/// </summary>
		/// <param name="Path">The directory path to create.</param>
		/// <returns><c>true</c> if the directory was created sucessfully; otherwise , <c>false</c>.</returns>
		public static System.Boolean CreateADir(System.String Path) 
		{
#if DEBUG
			Debugger.DebuggingInfo("(in ROOT.MAIN.CreateADir(...)) CREATING");
#endif
			return FileInterop.CreateDir(Path) != 0; 
		}

		/// <summary>
		/// Deletes a directory specified by the <paramref name="Path"/> parameter.
		/// If it is NOT empty , you can also delete all of it's contents by passing <c>true</c>
		/// to the <paramref name="DeleteAll"/> parameter.
		/// </summary>
		/// <param name="Path">The directory to delete. It is a path.</param>
		/// <param name="DeleteAll">Specifies whether to delete everyting inside the directory or not.</param>
		/// <returns><c>true</c> if the directory was deleted sucessfully; otherwise , <c>false</c>.</returns>
		public static System.Boolean DeleteADir(System.String Path, System.Boolean DeleteAll)
		{
			if (DeleteAll == false) { return FileInterop.RemoveDir(Path) != 0; } 
			if ((FileInterop.RemoveDir(Path) == 0) && (DeleteAll))
			{
                // Find all directories first to delete.
                List<System.String> info = new List<System.String>() { Path };
                for (System.Int32 I = 0; I < info.Count; I++)
				{
                    foreach (System.IO.DirectoryInfo DS in new System.IO.DirectoryInfo(info[I]).GetDirectories()) { info.Add(DS.FullName); }
                }
				//Enumerate through them , delete all the files , then the directories , and exit.
				//In case that any of the native functions fail during deletion  , function ends with a false value.
				System.IO.DirectoryInfo DI;
				foreach (System.String A in info)
				{
					DI = new(A);
					foreach (System.IO.FileInfo I in DI.GetFiles())
					{
                        // Call native function: Deletes the file.
                        // This function fails if returned from it 0. Exit with false in this case.
#if DEBUG
                        Debugger.DebuggingInfo($"(in ROOT.MAIN.DeleteADir({Path} , {DeleteAll})) DELETE: {I.FullName}");
#endif
                        if (FileInterop.DeleteFile(I.FullName) == 0) { return false; }
					}
				}
				// Now that the directories are empty , we are safe to delete them.
				// The RemoveDirectoryW function has this requirement.
				// Note: recursion here must happen in reverse order as they were found.
				for (System.Int32 I = info.Count - 1; I > 0; I--)
				{
                    // Call native function: Deletes the directory.
                    // This function fails if returned from it 0. Exit with false in this case.
#if DEBUG
                    Debugger.DebuggingInfo($"(in ROOT.MAIN.DeleteADir({Path} , {DeleteAll})) DELETE: {info[I]}");
#endif
                    if (FileInterop.RemoveDir(info[I]) == 0) { return false; }
				}
                return FileInterop.RemoveDir(Path) != 0;
            }
			return true;
		}

		/// <summary>
		/// Move files or directories specified by the parameters. The <paramref name="SourcePath"/> must exist.
		/// </summary>
		/// <param name="SourcePath">The path to get the directory or file to move it.</param>
		/// <param name="DestPath">The destination path that the file or directory should go to.</param>
		/// <returns><c>true</c> if the file or directory was moved; otherwise , <c>false</c>.</returns>
		public static System.Boolean MoveFilesOrDirs(System.String SourcePath, System.String DestPath)
		{
#if DEBUG
            Debugger.DebuggingInfo("(in ROOT.MAIN.MoveFileOrDir(... , ...)) MOVING");
#endif
            return FileInterop.MoveFileOrDir(SourcePath, DestPath) != 0;
		}

		/// <summary>
		/// Copy a file from a directory to another one.
		/// </summary>
		/// <param name="SourceFilePath">The path of the file that will be copied.</param>
		/// <param name="DestPath">The destination path of the file. Must also have the filename. Example: <![CDATA[C:\Files\fg.txt]]> .</param>
		/// <param name="OverWriteAllowed">[opt] Allows to overwrite the target if it exists.</param>
		/// <returns><c>true</c> if the file was copied to <paramref name="DestPath"/>; otherwise , <c>false</c>.</returns>
		public static System.Boolean CopyFile(System.String SourceFilePath,
		System.String DestPath, System.Boolean OverWriteAllowed = false)
		{
#if DEBUG
            Debugger.DebuggingInfo("(in ROOT.MAIN.CopyFile(... , ...)) COPYING");
#endif
            return FileInterop.CopyFile(SourceFilePath, DestPath, OverWriteAllowed == false) != 0;
		}

		/// <summary>
		/// Gets a new <see cref="System.IO.FileSystemInfo"/> captured from the specified directory.
		/// </summary>
		/// <param name="Path">The directory to get the data from.</param>
		/// <returns>A new <see cref="System.IO.FileSystemInfo"/> object containing the data; otherwise , <c>null</c> if an error occured.</returns>
		public static System.IO.FileSystemInfo[] GetANewFileSystemInfo(System.String Path)
		{
			if (DirExists(Path) == false) { return null; }
			try
			{
				System.IO.DirectoryInfo RFD = new System.IO.DirectoryInfo(Path);
				return RFD.GetFileSystemInfos();
			}
			catch (System.Exception e) { ExceptionData = e; return null; }
		}

		/// <summary>
		/// Creates a new and fresh file and opens a new handle for it by the <see cref="System.IO.FileStream"/>.
		/// </summary>
		/// <param name="Path">The file path where the file will be created. Example: <![CDATA[C:\Files\Start.txt]]> .</param>
		/// <returns>A new <see cref="System.IO.FileStream"/> object if no error occured; otherwise , <c>null</c>.</returns>
		public static System.IO.FileStream CreateANewFile(System.String Path) 
		{ 
			try 
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.CreateANewFile(...)) INFO: Attempting to create it.");
#endif
                return System.IO.File.OpenWrite(Path); 
			} catch (System.Exception e) 
			{
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateANewFile(...)) INFO: Error detected!\n{e}");
#endif
                ExceptionData = e; 
				return null; 
			}
		}

		/// <summary>
		/// Opens an handle for the existing file as a <see cref="System.IO.FileStream"/>.<br />
		/// The file is opened with both Read and Write permissions.
		/// </summary>
		/// <param name="Path">The file path where the file is located to.</param>
		/// <param name="AllowAccessToOthers">This parameter specifies whether to give other processes the
		/// permission to read or write the file too. By default , this is set to <see langword="false"/>.</param>
		/// <returns>A new <see cref="System.IO.FileStream"/> if no errors found; otherwise , <c>null</c>.</returns>
		public static System.IO.FileStream ReadAFileUsingFileStream(System.String Path , System.Boolean AllowAccessToOthers = false)
		{
			if (FileExists(Path) == false) { return null; }
			try 
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.ReadAFileUsingFileStream(...)) INFO: Attempting to open it with R/W permissions.");
#endif
				if (AllowAccessToOthers)
				{
					return System.IO.File.Open(Path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.ReadWrite);
				} else
				{
                    return System.IO.File.Open(Path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                }
			} catch (System.Exception e) 
			{
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.ReadAFileUsingFileStream(...)) INFO: Error detected!\n{e}");
#endif
                ExceptionData = e; return null; 
			}
		}

		/// <summary>
		/// Clears the pontential data that the file specified has and opens that file with Write permissions.
		/// The file specified at the <paramref name="Path"/> parameter must exist.
		/// </summary>
		/// <param name="Path">The file path where the file is located to.</param>
		/// <returns>A new <see cref="System.IO.FileStream"/> if no errors found; otherwise , <c>null</c>.</returns>
		public static System.IO.FileStream ClearAndWriteAFile(System.String Path)
		{
			if (FileExists(Path) == false) { return null; }
			try
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.ClearAndWriteAFile(...)) INFO: Attempting to open it with R/W permissions.");
#endif
                return System.IO.File.Open(Path, System.IO.FileMode.Truncate);
			}
			catch (System.Exception e)
			{
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.ClearAndWriteAFile(...)) INFO: Error detected!\n{e}");
#endif
                ExceptionData = e; return null;
			}
		}

		/// <summary>
		/// This method writes the <see cref="System.String"/> data specified to an alive <see cref="System.IO.FileStream"/> object with Write permissions.
		/// </summary>
		/// <param name="Contents">The <see cref="System.String"/> contents to write to the file.</param>
		/// <param name="FileStreamObject">The </param>
		public static void PassNewContentsToFile(System.String Contents, System.IO.FileStream FileStreamObject)
		{
			System.Byte[] EMDK = new System.Text.UTF8Encoding(true).GetBytes(Contents + System.Environment.NewLine);
			try
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.PassNewContentsToFile(...)) INFO: Attempting to write data to the target.");
#endif
                FileStreamObject.Write(EMDK, 0, EMDK.Length);
			}
			catch (System.Exception E) 
			{
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.PassNewContentsToFile(...)) INFO: Error detected!\n{E}");
#endif
				ExceptionData = E; 
				return;
            }
#if DEBUG
            Debugger.DebuggingInfo("(in ROOT.MAIN.PassNewContentsToFile(...)) INFO: Clearing up. Sucessfull data flush.");
#endif
            EMDK = null;
			return;
		}

		/// <summary>
		/// This method appends the specified <see cref="System.String"/> contents at an alive <see cref="System.IO.FileStream"/> object.
		/// </summary>
		/// <param name="Contents">The <see cref="System.String"/> data to write to the file.</param>
		/// <param name="FileStreamObject">The alive <see cref="System.IO.FileStream"/> to write the data to the opened file.</param>
		public static void AppendNewContentsToFile(System.String Contents, System.IO.FileStream FileStreamObject)
		{
			System.Byte[] EMDK = new System.Text.UTF8Encoding(true).GetBytes(Contents);
			try
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.AppendNewContentsToFile(...)) INFO: Attempting to write data to the target.");
#endif
                FileStreamObject.Write(EMDK, System.Convert.ToInt32(FileStreamObject.Length), EMDK.Length);
			}
			catch (System.Exception E) 
			{
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.AppendNewContentsToFile(...)) INFO: Error detected!\n{E}");
#endif
                ExceptionData = E;
                return;
            }
#if DEBUG
            Debugger.DebuggingInfo("(in ROOT.MAIN.AppendNewContentsToFile(...)) INFO: Clearing up. Sucessfull data flush.");
#endif
            EMDK = null;
			return;
		}

		/// <summary>
		/// This method gets all the file contents from the alive <see cref="System.IO.FileStream"/> object.
		/// Be noted that the object must have at least Read permissions.
		/// </summary>
		/// <param name="FileStreamObject">The alive <see cref="System.IO.FileStream"/> object to get the file data from.</param>
		/// <returns>The file contents to a <see cref="System.String"/> .</returns>
		public static System.String GetContentsFromFile(System.IO.FileStream FileStreamObject)
		{
			System.Byte[] EMS = new System.Byte[FileStreamObject.Length];
			FileStreamObject.Read(EMS, 0, System.Convert.ToInt32(FileStreamObject.Length));
			return new System.Text.UTF8Encoding(true).GetString(EMS);
		}

		/// <summary>
		/// This method gets a cryptography hash for the selected file with the selected algorithm.
		/// </summary>
		/// <param name="PathOfFile">The path to the file you want it's hash.</param>
		/// <param name="HashToSelect">The hash algorithm to select. Consult the <see cref="HashDigestSelection"/> <see cref="System.Enum"/>
		/// for more information.
		/// </param>
		/// <returns>The computed hash as a hexadecimal <see cref="System.String" /> if succeeded; 
		/// otherwise , the <see cref="System.String" /> <c>"Error"</c>. </returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.String GetACryptographyHashForAFile(System.String PathOfFile, HashDigestSelection HashToSelect)
		{
			System.Byte[] Contents = null;
			using System.IO.FileStream Initialiser = ReadAFileUsingFileStream(PathOfFile);
			{
				if (Initialiser == null) { return "Error"; } else { Contents = new System.Byte[Initialiser.Length]; }
				if (Initialiser != null) { Initialiser.Read(Contents, 0, Contents.Length); }
			}
			System.Security.Cryptography.HashAlgorithm EDI;
			switch (HashToSelect)
			{
				case HashDigestSelection.SHA1: EDI = new System.Security.Cryptography.SHA1Managed(); break;
				case HashDigestSelection.SHA256: EDI = new System.Security.Cryptography.SHA256Managed(); break;
				case HashDigestSelection.SHA384: EDI = new System.Security.Cryptography.SHA384Managed(); break;
				case HashDigestSelection.SHA512: EDI = new System.Security.Cryptography.SHA512Managed(); break;
				case HashDigestSelection.MD5: EDI = System.Security.Cryptography.MD5.Create(); break;
				default:
					ExceptionData = new InvalidOperationException($"Error - Option {HashToSelect} Is Invalid!!!");
					return "Error";
			}
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.GetACryptographyHashForAFile(...)) INFO: Computing hash of target.");
#endif
            System.Byte[] RSS = EDI.ComputeHash(Contents);
			EDI.Dispose();
			System.String Result = null;
			for (System.Int32 ITER = 0; ITER <= RSS.Length - 1; ITER++) { Result += RSS[ITER].ToString("x2"); }
			return Result;
		}

		/// <summary>
		/// This method gets a cryptography hash for the selected file with the selected algorithm.
		/// </summary>
		/// <param name="PathOfFile">The path to the file you want it's hash.</param>
		/// <param name="HashToSelect">The hash algorithm to select. Valid Values: <code>"SHA1" , "SHA256" , "SHA384" , "SHA512" , "MD5"</code></param>
		/// <returns>The computed hash as a hexadecimal <see cref="System.String" /> if succeeded; 
		/// otherwise , the <see cref="System.String" /> <c>"Error"</c>. </returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.String GetACryptographyHashForAFile(System.String PathOfFile, System.String HashToSelect)
		{
			System.Byte[] Contents = null;
			System.IO.FileStream Initialiser = ReadAFileUsingFileStream(PathOfFile);
			if (Initialiser == null) { return "Error"; } else { Contents = new System.Byte[Initialiser.Length]; }
			if (Initialiser != null) { Initialiser.Read(Contents, 0, Contents.Length); }
			if (Initialiser != null) { Initialiser.Close(); Initialiser.Dispose(); }
			System.Security.Cryptography.HashAlgorithm EDI;
			switch (HashToSelect)
			{
				case "SHA1":
					EDI = new System.Security.Cryptography.SHA1Managed();
					break;
				case "SHA256":
					EDI = new System.Security.Cryptography.SHA256Managed();
					break;
				case "SHA384":
					EDI = new System.Security.Cryptography.SHA384Managed();
					break;
				case "SHA512":
					EDI = new System.Security.Cryptography.SHA512Managed();
					break;
				case "MD5":
					EDI = System.Security.Cryptography.MD5.Create();
					break;
				default:
                    ExceptionData = new InvalidOperationException($"Error - Option {HashToSelect} Is Invalid!!!");
                    return "Error";
			}
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.GetACryptographyHashForAFile(...)) INFO: Computing hash of target.");
#endif
            System.Byte[] RSS = EDI.ComputeHash(Contents);
			EDI.Dispose();
			System.String Result = null;
			for (System.Int32 ITER = 0; ITER <= RSS.Length - 1; ITER++) { Result += RSS[ITER].ToString("x2"); }
			return Result;
		}

#pragma warning disable CS1591
		[System.ObsoleteAttribute("AppendAnExistingFile method is no longer required , because using ReadAFileUsingFileStream will normally open it with R/W " +
			"permissions. Use then the AppendNewContentsToFile function to append data to that opened file.", true)]
		public static System.IO.StreamWriter AppendAnExistingFile(System.String Path) { try { return System.IO.File.AppendText(Path); } catch (System.Exception) { return null; } }
#pragma warning restore CS1591
		/// <summary>
		/// This method deletes the specified file from the <paramref name="Path"/> specified.
		/// </summary>
		/// <param name="Path">The file path wich points out the file to delete.</param>
		/// <returns><c>true</c> if the file was deleted; otherwise , <c>false</c>.</returns>
		public static System.Boolean DeleteAFile(System.String Path)
		{
			if (FileExists(Path) == false) { return false; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.DeleteAFile(...)) INFO: Attempting to delete the target.");
#endif
            return FileInterop.DeleteFile(Path) != 0;
		}

#pragma warning disable CS1591
		[System.ObsoleteAttribute("ReadAFile method has been replaced with the ReadAFileUsingFileStream function , which performs better at performance level." +
			"You should notice that sometime this function will be removed without prior notice.", false)]
		public static System.IO.StreamReader ReadAFile(System.String Path)
		{
			if (FileExists(Path) == false) { return null; }
			try
			{
				return new System.IO.StreamReader(new System.IO.FileStream(Path, System.IO.FileMode.Open));
			}
			catch (System.Exception) { return null; }
		}

		[System.ObsoleteAttribute("GetFileContentsFromStreamReader method has been replaced with the GetContentsFromFile function , which performs better at performance level." +
			"You should notice that sometime this function will be removed without prior notice.", false)]
		public static System.String GetFileContentsFromStreamReader(System.IO.StreamReader FileStream)
		{
			System.String ReturnableString = null;
			try
			{
				while (FileStream.Peek() >= 0) { ReturnableString += FileStream.ReadLine() + System.Environment.NewLine; }
				return ReturnableString;
			}
			catch (System.Exception) { return null; }
		}

#pragma warning restore CS1591

#if NET472_OR_GREATER

		/// <summary>
		/// This shows the default Windows Message box on the screen.
		/// </summary>
		/// <param name="MessageString">The text for the message to show.</param>
		/// <param name="Title">The window's title.</param>
		/// <param name="MessageButton">The button(s) to show as options in the message box.</param>
		/// <param name="MessageIcon">The icon to show as a prompt in the message box.</param>
		/// <returns>An <see cref="System.Int32"/> which indicates which button the user pressed.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Int32 NewMessageBoxToUser(System.String MessageString, System.String Title,
		System.Windows.Forms.MessageBoxButtons MessageButton = 0,
		System.Windows.Forms.MessageBoxIcon MessageIcon = 0)
		{
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.NewMessageBoxToUser({MessageString} , {Title} , {MessageButton} , {MessageIcon}))" +
				" INFO: Calling native method instead. Deferral will NOT pass from the custom message handler.");
#endif
			System.Windows.Forms.DialogResult RET = System.Windows.Forms.MessageBox.Show(MessageString, Title, MessageButton, MessageIcon);
            return RET switch
            {
                DialogResult.OK => 1,
                DialogResult.Cancel => 2,
                DialogResult.Abort => 3,
                DialogResult.Retry => 4,
                DialogResult.Ignore => 5,
                DialogResult.Yes => 6,
                DialogResult.No => 7,
                _ => 0,
            };
        }

		/// <summary>
		/// This is a modified Windows Message box made by me.
		/// To customize the options and for more information , consult the <see cref="IntuitiveInteraction.IntuitiveMessageBox"/> class.
		/// </summary>
		/// <param name="MessageString">The text for the message to show.</param>
		/// <param name="Title">The window's title.</param>
		/// <param name="MessageButton">The button(s) to show as options in the message box.</param>
		/// <param name="IconToSelect">The icon to show as a prompt in the message box.</param>
		/// <returns>An <see cref="System.Int32"/> which indicates which button the user pressed.</returns>
		[SupportedOSPlatform("windows")]
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.Int32 NewMessageBoxToUser(System.String MessageString, System.String Title,
			ROOT.IntuitiveInteraction.ButtonSelection MessageButton, ROOT.IntuitiveInteraction.IconSelection IconToSelect)
		{
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.NewMessageBoxToUser({MessageString} , {Title} , {MessageButton} , {IconToSelect}))" +
                " INFO: Calling Internal class instead , because of the programmer's intention.");
#endif
            IntuitiveInteraction.IntuitiveMessageBox DX = new(MessageString, Title, MessageButton, IconToSelect);
            return DX.ButtonSelected switch
            {
                IntuitiveInteraction.ButtonReturned.OK => 1,
                IntuitiveInteraction.ButtonReturned.Cancel => 2,
                IntuitiveInteraction.ButtonReturned.Abort => 3,
                IntuitiveInteraction.ButtonReturned.Retry => 4,
                IntuitiveInteraction.ButtonReturned.Ignore => 5,
                IntuitiveInteraction.ButtonReturned.Yes => 6,
                IntuitiveInteraction.ButtonReturned.No => 7,
                _ => 0,
            };
        }

		/// <summary>
		/// This spawns a new and classical Windows Load File dialog to the user.
		/// </summary>
		/// <param name="FileFilterOfWin32">The file filter that the User will select from the options given. For example , "Zip Archives|*.zip|Log Files|*.log;*.txt"</param>
		/// <param name="FileExtensionToPresent">The file extension from the <paramref name="FileFilterOfWin32"/> to select when the dialog will be invoked. Be noted , for this
		/// to take effect , the <paramref name="FileFilterOfWin32"/> list must have more than two different file extensions.</param>
		/// <param name="FileDialogWindowTitle">The title that will be shown when this dialog is invoked.</param>
		/// <param name="DirToPresent">The initial directory that the dialog will start to.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner CreateLoadDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
		System.String FileDialogWindowTitle, System.String DirToPresent)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = FileDialogType.LoadFile;
			if (System.String.IsNullOrEmpty(DirToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (DirExists(DirToPresent) == false)
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileExtensionToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileDialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileFilterOfWin32))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new Microsoft.Win32.OpenFileDialog();

			FLD.Title = FileDialogWindowTitle;
			FLD.DefaultExt = FileExtensionToPresent;
			FLD.Filter = FileFilterOfWin32;
			// FileDialog Settings: <--
			// If any link is given as path , the path given by the link must be only returned.
			FLD.DereferenceLinks = true;
			// Only one filepath is required.
			FLD.Multiselect = false;
			FLD.InitialDirectory = DirToPresent;
			FLD.AddExtension = false;
			// Those two below check if the file path supplied is existing.
			// If not , throw a warning.
			FLD.CheckFileExists = true;
			FLD.CheckPathExists = true;
			// -->
			// Now , spawn the dialog after all these settings.
			System.Boolean? REST = FLD.ShowDialog();

			if (REST == true)
			{
				EDOut.FileNameFullPath = FLD.FileName;
				EDOut.FileNameOnly = FLD.SafeFileName;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

		/// <summary>
		/// This spawns a new and classical Windows Save File dialog to the user.
		/// </summary>
		/// <param name="FileFilterOfWin32">The file filter that the User will select from the options given. For example , "Zip Archives|*.zip|Log Files|*.log;*.txt"</param>
		/// <param name="FileExtensionToPresent">The file extension from the <paramref name="FileFilterOfWin32"/> to select when the dialog will be invoked. Be noted , for this
		/// to take effect , the <paramref name="FileFilterOfWin32"/> list must have more than two different file extensions.</param>
		/// <param name="FileDialogWindowTitle">The title that will be shown when this dialog is invoked.</param>
		/// <param name="DirToPresent">The initial directory that the dialog will start to.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
		System.String FileDialogWindowTitle, System.String DirToPresent)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = FileDialogType.CreateFile;
			if (System.String.IsNullOrEmpty(DirToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (DirExists(DirToPresent) == false)
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileExtensionToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileDialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileFilterOfWin32))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new Microsoft.Win32.SaveFileDialog();

			FLD.Title = FileDialogWindowTitle;
			FLD.DefaultExt = FileExtensionToPresent;
			FLD.Filter = FileFilterOfWin32;
			// FileDialog Settings: <--
			// If any link is given as path , the path given by the link must be only returned.
			FLD.DereferenceLinks = true;
			// Only one filepath is required.
			FLD.AddExtension = false;
			FLD.InitialDirectory = DirToPresent;
			// Those two below check if the file path supplied is existing.
			// If not , throw a warning.
			FLD.CheckFileExists = true;
			FLD.CheckPathExists = true;
			FLD.OverwritePrompt = true;
			// -->
			// Now , spawn the dialog after all these settings.
			System.Boolean? REST = FLD.ShowDialog();

			if (REST == true)
			{
				EDOut.FileNameFullPath = FLD.FileName;
				EDOut.FileNameOnly = FLD.SafeFileName;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

		/// <summary>
		/// This spawns a new and classical Windows Load File dialog to the user.
		/// </summary>
		/// <param name="FileFilterOfWin32">The file filter that the User will select from the options given. For example , "Zip Archives|*.zip|Log Files|*.log;*.txt"</param>
		/// <param name="FileExtensionToPresent">The file extension from the <paramref name="FileFilterOfWin32"/> to select when the dialog will be invoked. Be noted , for this
		/// to take effect , the <paramref name="FileFilterOfWin32"/> list must have more than two different file extensions.</param>
		/// <param name="FileDialogWindowTitle">The title that will be shown when this dialog is invoked.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner CreateLoadDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
		System.String FileDialogWindowTitle)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = FileDialogType.LoadFile;
			if (System.String.IsNullOrEmpty(FileExtensionToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileDialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileFilterOfWin32))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new Microsoft.Win32.OpenFileDialog();

			FLD.Title = FileDialogWindowTitle;
			FLD.DefaultExt = FileExtensionToPresent;
			FLD.Filter = FileFilterOfWin32;
			// FileDialog Settings: <--
			// If any link is given as path , the path given by the link must be only returned.
			FLD.DereferenceLinks = true;
			// Only one filepath is required.
			FLD.Multiselect = false;
			FLD.AddExtension = false;
			// Those two below check if the file path supplied is existing.
			// If not , throw a warning.
			FLD.CheckFileExists = true;
			FLD.CheckPathExists = true;
			// -->
			// Now , spawn the dialog after all these settings.
			System.Boolean? REST = FLD.ShowDialog();

			if (REST == true)
			{
				EDOut.FileNameFullPath = FLD.FileName;
				EDOut.FileNameOnly = FLD.SafeFileName;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

		/// <summary>
		/// This spawns a new and classical Windows Save File dialog to the user.
		/// </summary>
		/// <param name="FileFilterOfWin32">The file filter that the User will select from the options given. For example , "Zip Archives|*.zip|Log Files|*.log;*.txt"</param>
		/// <param name="FileExtensionToPresent">The file extension from the <paramref name="FileFilterOfWin32"/> to select when the dialog will be invoked. Be noted , for this
		/// to take effect , the <paramref name="FileFilterOfWin32"/> list must have more than two different file extensions.</param>
		/// <param name="FileDialogWindowTitle">The title that will be shown when this dialog is invoked.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
		System.String FileDialogWindowTitle)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = ROOT.FileDialogType.CreateFile;
			if (System.String.IsNullOrEmpty(FileExtensionToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileDialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileFilterOfWin32))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new Microsoft.Win32.SaveFileDialog();

			FLD.Title = FileDialogWindowTitle;
			FLD.DefaultExt = FileExtensionToPresent;
			FLD.Filter = FileFilterOfWin32;
			// FileDialog Settings: <--
			// If any link is given as path , the path given by the link must be only returned.
			FLD.DereferenceLinks = true;
			// Only one filepath is required.
			FLD.AddExtension = false;
			// Those two below check if the file path supplied is existing.
			// If not , throw a warning.
			FLD.CheckFileExists = true;
			FLD.CheckPathExists = true;
			FLD.OverwritePrompt = true;
			// -->
			// Now , spawn the dialog after all these settings.
			System.Boolean? REST = FLD.ShowDialog();

			if (REST == true)
			{
				EDOut.FileNameFullPath = FLD.FileName;
				EDOut.FileNameOnly = FLD.SafeFileName;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

		/// <summary>
		/// This spawns a new and classical Windows Save File dialog to the user.
		/// </summary>
		/// <param name="FileFilterOfWin32">The file filter that the User will select from the options given. For example , "Zip Archives|*.zip|Log Files|*.log;*.txt"</param>
		/// <param name="FileExtensionToPresent">The file extension from the <paramref name="FileFilterOfWin32"/> to select when the dialog will be invoked. Be noted , for this
		/// to take effect , the <paramref name="FileFilterOfWin32"/> list must have more than two different file extensions.</param>
		/// <param name="FileDialogWindowTitle">The title that will be shown when this dialog is invoked.</param>
		/// <param name="FileMustExist">A <see cref="System.Boolean"/> value that the file selected must be existing.</param>
		/// <param name="DirToPresent">The initial directory that the dialog will start to.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
		System.String FileDialogWindowTitle, System.Boolean FileMustExist, System.String DirToPresent)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = ROOT.FileDialogType.CreateFile;
			if (System.String.IsNullOrEmpty(FileExtensionToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileDialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileFilterOfWin32))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new Microsoft.Win32.SaveFileDialog();

			FLD.Title = FileDialogWindowTitle;
			FLD.DefaultExt = FileExtensionToPresent;
			FLD.Filter = FileFilterOfWin32;
			// FileDialog Settings: <--
			// If any link is given as path , the path given by the link must be only returned.
			FLD.DereferenceLinks = true;
			// Only one filepath is required.
			FLD.AddExtension = false;
			// Those two below check if the file path supplied is existing.
			// If not , throw a warning.
			FLD.CheckFileExists = FileMustExist;
			FLD.InitialDirectory = DirToPresent;
			FLD.CheckPathExists = true;
			FLD.OverwritePrompt = true;
			// -->
			// Now , spawn the dialog after all these settings.
			System.Boolean? REST = FLD.ShowDialog();

			if (REST == true)
			{
				EDOut.FileNameFullPath = FLD.FileName;
				EDOut.FileNameOnly = FLD.SafeFileName;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

		/// <summary>
		/// This spawns a new and classical Windows Save File dialog to the user.
		/// </summary>
		/// <param name="FileFilterOfWin32">The file filter that the User will select from the options given. For example , "Zip Archives|*.zip|Log Files|*.log;*.txt"</param>
		/// <param name="FileExtensionToPresent">The file extension from the <paramref name="FileFilterOfWin32"/> to select when the dialog will be invoked. Be noted , for this
		/// to take effect , the <paramref name="FileFilterOfWin32"/> list must have more than two different file extensions.</param>
		/// <param name="FileDialogWindowTitle">The title that will be shown when this dialog is invoked.</param>
		/// <param name="FileMustExist">A <see cref="System.Boolean"/> value that the file selected must be existing.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
		System.String FileDialogWindowTitle, System.Boolean FileMustExist)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = ROOT.FileDialogType.CreateFile;
			if (System.String.IsNullOrEmpty(FileExtensionToPresent))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileDialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
			if (System.String.IsNullOrEmpty(FileFilterOfWin32))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new Microsoft.Win32.SaveFileDialog();

			FLD.Title = FileDialogWindowTitle;
			FLD.DefaultExt = FileExtensionToPresent;
			FLD.Filter = FileFilterOfWin32;
			// FileDialog Settings: <--
			// If any link is given as path , the path given by the link must be only returned.
			FLD.DereferenceLinks = true;
			// Only one filepath is required.
			FLD.AddExtension = false;
			// Those two below check if the file path supplied is existing.
			// If not , throw a warning.
			FLD.CheckFileExists = FileMustExist;
			FLD.CheckPathExists = true;
			FLD.OverwritePrompt = true;
			// -->
			// Now , spawn the dialog after all these settings.
			System.Boolean? REST = FLD.ShowDialog();

			if (REST == true)
			{
				EDOut.FileNameFullPath = FLD.FileName;
				EDOut.FileNameOnly = FLD.SafeFileName;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

		/// <summary>
		/// This spawns a Directory Selection dialog to the user.
		/// </summary>
		/// <param name="DirToPresent">The Directory that this dialog should show first.</param>
		/// <param name="DialogWindowTitle">The title that will be shown when this dialog is invoked , with an additional description if needed.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent, System.String DialogWindowTitle)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = ROOT.FileDialogType.DirSelect;
			if (System.String.IsNullOrEmpty(DialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new System.Windows.Forms.FolderBrowserDialog();
			// Settings for the FolderBrowserDialog.
			FLD.ShowNewFolderButton = true;
			FLD.Description = DialogWindowTitle;
			FLD.RootFolder = DirToPresent;

			DialogResult REST = FLD.ShowDialog();

			if (REST == DialogResult.OK)
			{
				EDOut.DirPath = FLD.SelectedPath;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

		/// <summary>
		/// This spawns a Directory Selection dialog to the user.
		/// </summary>
		/// <param name="DirToPresent">The Directory that this dialog should show first.</param>
		/// <param name="DialogWindowTitle">The title that will be shown when this dialog is invoked , with an additional description if needed.</param>
		/// <param name="AlternateDir">When the <paramref name="DirToPresent"/> parameter is MyComputer , you can set whatever initial directory you like to.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent, System.String DialogWindowTitle, System.String AlternateDir)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = ROOT.FileDialogType.DirSelect;
			if (System.String.IsNullOrEmpty(DialogWindowTitle))
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}

			var FLD = new System.Windows.Forms.FolderBrowserDialog();
			// Settings for the FolderBrowserDialog.
			FLD.ShowNewFolderButton = true;
			FLD.Description = DialogWindowTitle;
			FLD.RootFolder = DirToPresent;
			if (DirToPresent == Environment.SpecialFolder.MyComputer)
			{
				//Because the above returns the MyComputer directory , which is a virtual one , we can make use of it and pass a custom directory instead.
				EDOut.DirPath = AlternateDir;
			}

			DialogResult REST = FLD.ShowDialog();

			if (REST == DialogResult.OK)
			{
				EDOut.DirPath = FLD.SelectedPath;
				EDOut.ErrorCode = "None";
				return EDOut;
			}
			else
			{
				EDOut.ErrorCode = "Error";
				return EDOut;
			}
		}

#endif
        /// <summary>
        /// Stops the application execution for the specified time (Counted in milliseconds).
        /// Think it like that the application gets to a "HALT" state for that time.
        /// </summary>
        /// <param name="TimeoutEpoch">The time to stop the execution.</param>
        public static void HaltApplicationThread(System.Int32 TimeoutEpoch) 
		{
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.HaltApplicationThread({TimeoutEpoch})) INFO: Getting to \'HALT\' state for {TimeoutEpoch} ms...");
#endif
			System.Threading.Thread.Sleep(TimeoutEpoch);
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.HaltApplicationThread({TimeoutEpoch})) INFO: Exited HALT state...");
#endif
        }

        /// <summary>
        /// Launches a new process defined by the parameters.
        /// </summary>
        /// <param name="PathOfExecToRun">The path of executable or document to open.</param>
        /// <param name="CommandLineArgs">The arguments to pass to the executable.</param>
        /// <param name="ImplicitSearch">The %PATH% variable should be searched for this executable.</param>
        /// <param name="WaitToClose">Wait for the app to close.</param>
        /// <returns>0 when the process launched sucessfully and <paramref name="WaitToClose"/> was <c>false</c>;
        /// otherwise , the process exit code. 
        /// <c>-10337880</c> for the Windows Launcher Error , like architecture mismatch error.
        /// <c>-10337881</c> for any other generic error.</returns>
        [SupportedOSPlatform("windows")]
		public static System.Int32 LaunchProcess(System.String PathOfExecToRun, System.String CommandLineArgs,
		System.Boolean ImplicitSearch, System.Boolean WaitToClose)
		{
			System.Int32 ExitCode = 0;
			System.String FinalFilePath = "Error";
			/*
			 Here the ImplcitSearch variable is being evaluated.
			 ImplicitSearch Argument Usage: 
			 ImplicitSearch instructs the function to treat the filename as a valid one.
			 How this happens? When you explicitly do not provide a full path , the System asumes that the file 
			 is located to the current working directory. and if not existing , a Win32Handle exception will be thrown.
			 However , when setting the value to True(It is a Bool) , the function will behave exactly as the Cmd.exe does
			 (That is , supplying the path to the known paths.) and will auto-search the file from the Path environment 
			 variable. 
			 Notice: When Using the Implicit Searcher , make sure that the filepath is not a full one , otherwise it will fail.
			*/
			if (ImplicitSearch == false)
			{
				if (!FileExists(PathOfExecToRun))
				{
					return -8;
				}
				FinalFilePath = PathOfExecToRun;
			}
			else
			{
				foreach (System.String T in GetPathEnvironmentVar())
				{
					if (FileExists(T + "\\" + PathOfExecToRun))
					{
						FinalFilePath = T + "\\" + PathOfExecToRun;
						break;
					}
				}
				if (FinalFilePath == "Error")
				{
					return -8;
				}
			}
			System.Diagnostics.Process FRM = new System.Diagnostics.Process();
			FRM.StartInfo.UseShellExecute = true;
			FRM.StartInfo.FileName = FinalFilePath;
			FRM.StartInfo.Arguments = CommandLineArgs;
			try { FRM.Start(); }
			catch (System.ComponentModel.Win32Exception)
			{
				FRM = null;
				return -10337880;
			}
			catch (System.Exception)
			{
				FRM = null;
				return -10337881;
			}
			if (WaitToClose)
			{
				FRM.WaitForExit();
				if (FRM.HasExited)
				{
					ExitCode = FRM.ExitCode;
				}
			}
			FRM = null;
			return ExitCode;
		}

		/// <summary>
		/// Launches a new process defined by the parameters.
		/// </summary>
		/// <param name="PathOfExecToRun">The path of executable or document to open.</param>
		/// <param name="CommandLineArgs">The arguments to pass to the executable.</param>
		/// <param name="ImplicitSearch">The %PATH% variable should be searched for this executable.</param>
		/// <param name="WaitToClose">Wait for the app to close.</param>
		/// <param name="RunAtNativeConsole">Whether the app should use the aplication shell to run.</param>
		/// <param name="HideExternalConsole">Whether the process console window must be hidden.</param>
		/// <returns>0 when the process launched sucessfully and <paramref name="WaitToClose"/> was <c>false</c>;
		/// otherwise , the process exit code. 
		/// <c>-10337880</c> for the Windows Launcher Error , like architecture mismatch error.
		/// <c>-10337881</c> for any other generic error.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Int32 LaunchProcess(System.String PathOfExecToRun, System.String CommandLineArgs,
		System.Boolean ImplicitSearch, System.Boolean WaitToClose, System.Boolean? RunAtNativeConsole,
		System.Boolean? HideExternalConsole)
		{
			if (RunAtNativeConsole == null) { RunAtNativeConsole = false; }
			if (HideExternalConsole == null) { HideExternalConsole = true; }
			if (CommandLineArgs == null) { CommandLineArgs = " "; }
			System.Int32 ExitCode = 0;
			System.String FinalFilePath = "Error";
			/*
			 Here the ImplcitSearch variable is being evaluated.
			 ImplicitSearch Argument Usage: 
			 ImplicitSearch instructs the function to treat the filename as a valid one.
			 How this happens? When you explicitly do not provide a full path , the System asumes that the file 
			 is located to the current working directory. and if not existing , a Win32Handle exception will be thrown.
			 However , when setting the value to True(It is a Bool) , the function will behave exactly as the Cmd.exe does
			 (That is , supplying the path to the known paths.) and will auto-search the file from the Path environment 
			 variable. 
			 Notice: When Using the Implicit Searcher , make sure that the filepath is not a full one , otherwise it will fail.
			*/
			if (ImplicitSearch == false)
			{
				if (!FileExists(PathOfExecToRun))
				{
					return -8;
				}
				FinalFilePath = PathOfExecToRun;
			}
			else
			{
				foreach (System.String T in GetPathEnvironmentVar())
				{
					if (FileExists(T + "\\" + PathOfExecToRun))
					{
						FinalFilePath = T + "\\" + PathOfExecToRun;
						break;
					}
				}
				if (FinalFilePath == "Error")
				{
					return -8;
				}
			}
			System.Diagnostics.Process FRM = new System.Diagnostics.Process();
			if (RunAtNativeConsole == true)
			{
				FRM.StartInfo.UseShellExecute = false;
			}
			else
			{
				FRM.StartInfo.UseShellExecute = true;
				if (HideExternalConsole == true)
				{
					FRM.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				}
			}
			FRM.StartInfo.FileName = FinalFilePath;
			FRM.StartInfo.Arguments = CommandLineArgs;
			try { FRM.Start(); }
			catch (System.ComponentModel.Win32Exception)
			{
				FRM = null;
				return -10337880;
			}
			catch (System.Exception)
			{
				FRM = null;
				return -10337881;
			}
			if (WaitToClose)
			{
				FRM.WaitForExit();
				if (FRM.HasExited)
				{
					ExitCode = FRM.ExitCode;
				}
			}
			FRM = null;
			return ExitCode;
		}

		/// <summary>
		/// Gets the %PATH% environment variable of this instance.
		/// </summary>
		/// <returns>The <see cref="System.String"/>[] of the directory paths found in the variable.</returns>
		[SupportedOSPlatform("windows")]
		// Spliting can be a very consuming process , process the data as fast as possible.
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.String[] GetPathEnvironmentVar()
		{
			try
			{
				System.String[] RMF = (System.Environment.GetEnvironmentVariable("Path")).Split(';');
				return RMF;
			}
			catch (System.Exception) { return null; }
		}

		/// <summary>
		/// Finds a specified file from the %PATH% variable.
		/// </summary>
		/// <param name="FileName">The File name to look up. Must have and it's extension too.</param>
		/// <returns>A new <see cref="FileSearchResult"/> <see langword="struct"/> 
		/// which contains the file path that was found.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static FileSearchResult FindFileFromPath(System.String FileName)
		{
			FileSearchResult FSR = new FileSearchResult();
			if (FileName == null) { FSR.success = false; return FSR; }
			if (FileName.IndexOf('\\') != -1) { FSR.success = false; return FSR; }
			System.String[] strings = GetPathEnvironmentVar();
			for (System.Int32 I = 0; I < strings.Length; I++)
			{
				if (FileExists($"{strings[I]}\\{FileName}") == true)
				{
					FSR.Filepath = $"{strings[I]}\\{FileName}";
					FSR.GetExtension();
					FSR.success = true;
					return FSR;
				}
			}
			FSR.success = false;
			return FSR;
		}

		/// <summary>
		/// Finds a specified file from the %PATH% variable.
		/// </summary>
		/// <param name="FileName">The File name to look up , without it's extension.</param>
		/// <param name="Extensions">The Extensions to also look up. 
		/// The array must have the file extensions without the dot. Example:
		/// <c>{ "exe" , "dll" , "txt" , "log" , "evtx" }</c></param>
		/// <returns>A new <see cref="FileSearchResult"/> <see langword="struct"/> 
		/// which contains the file path that was found.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static FileSearchResult FindFileFromPath(System.String FileName, System.String[] Extensions)
		{
			FileSearchResult FSR = new FileSearchResult();
			if (FileName == null) { FSR.success = false; return FSR; }
			if (FileName.IndexOf('\\') != -1) { FSR.success = false; return FSR; }
			if (Extensions.Length == 0) { FSR.success = false; return FSR; }
			if (Extensions == null) { FSR.success = false; return FSR; }
			System.String[] strings = GetPathEnvironmentVar();
			for (System.Int32 A = 0; A < Extensions.Length; A++)
			{
				for (System.Int32 B = 0; B < strings.Length; B++)
				{
					if (FileExists($"{strings[B]}\\{FileName}.{Extensions[A]}") == true)
					{
						FSR.Filepath = $"{strings[B]}\\{FileName}.{Extensions[A]}";
						FSR.extension = Extensions[A];
						FSR.success = true;
						return FSR;
					}
				}
			}
			FSR.success = false;
			return FSR;
		}

	}

	/// <summary>
	/// This enumeration has valid System links that exist across all Windows computers.
	/// </summary>
	public enum SystemLinks
	{
		/// <summary>
		/// Open the Settings Menu.
		/// </summary>
		Settings = 2,
		/// <summary>
		/// Open the Microsoft Store.
		/// </summary>
		Store = 3,
		/// <summary>
		/// Open the Action Bar (Control Center).
		/// </summary>
		ActionBar = 4,
		/// <summary>
		/// Open the Quick Settings Action bar (That is , WI-FI settings , BlueTooth settings and sound settings.)
		/// </summary>
		FastSettings = 5,
		/// <summary>
		/// Start the Print-Screen Snipping Tool.
		/// </summary>
		ScreenSnippingTool = 6,
		/// <summary>
		/// Open the Phone Link tool.
		/// </summary>
		PhoneLink = 7,
		/// <summary>
		/// Open The Get Started tool.
		/// </summary>
		GetStarted = 8,
		/// <summary>
		/// Opens the default Windows Music App.
		/// </summary>
		MusicApp = 9,
		/// <summary>
		/// Opens the Windows Security App.
		/// </summary>
		WindowsSecurity = 10,
		/// <summary>
		/// Opens the Mail App.
		/// </summary>
		MailApp = 11,
		/// <summary>
		/// Opens the Calendar App.
		/// </summary>
		Calendar = 12,
		/// <summary>
		/// Opens the People App.
		/// </summary>
		PeopleApp = 13,
		/// <summary>
		/// Opens the Camera App.
		/// </summary>
		Camera = 14,
		/// <summary>
		/// Opens the Maps App.
		/// </summary>
		MapsApp = 15,
		/// <summary>
		/// Opens the Calculator App.
		/// </summary>
		Calculator = 16,
		/// <summary>
		/// Opens the Clock App.
		/// </summary>
		ClockApp = 17
	}

	/// <summary>
	/// The <see cref="FileSearchResult"/> <see langword="struct"/> is the return type for the file
	/// searcher functions defined in the <see cref="MAIN"/> class.
	/// </summary>
	public struct FileSearchResult
	{
		internal System.String Filepath;
		internal System.Boolean success;
		internal System.String extension;

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		internal FileSearchResult(System.String Path, System.Boolean Successfull)
		{
			Filepath = Path;
			success = Successfull;
		}

		internal void GetExtension()
		{
			if (Filepath != null)
			{
				// The try clause is added because the below method can throw exceptions.
				try
				{
					extension = Filepath.Substring(Filepath.LastIndexOf('.') + 1);
				}
				catch { return; }
			}
		}

		/// <summary>
		/// The file extension , without the dot.
		/// </summary>
		public System.String Extension { get { return extension; } }

		/// <summary>
		/// The Path where the file is located.
		/// </summary>
		public System.String Path { get { return Filepath; } }

		/// <summary>
		/// Detect whether the functions found a match or not.
		/// </summary>
		public System.Boolean MatchFound { get { return success; } }
	}

	/// <summary>
	/// An enumeration of values which indicate which dialog was invoked.
	/// </summary>
	public enum FileDialogType : System.Int32
	{
		/// <summary>
		/// The Dialog invoked was one of the <see cref="MAIN.CreateSaveDialog(string, string, string)"/> overloads.
		/// </summary>
		CreateFile = 0,
		/// <summary>
		/// The Dialog invoked was one of the <see cref="MAIN.CreateLoadDialog(string, string, string)"/> overloads.
		/// </summary>
		LoadFile = 2,
		/// <summary>
		/// The Dialog invoked was one of the <see cref="MAIN.GetADirDialog(Environment.SpecialFolder, string)"/> overloads.
		/// </summary>
		DirSelect = 4
	}


#if NET472_OR_GREATER

	// Found that the File Dialogs work the same even when 
	// these run under .NET Standard.
	// Weird that they work the same , though.



	/// <summary>
	/// A storage class used by the file/dir dialogs to access the paths given (Full and name only) , the dialog type ran and if there was an error.		
	/// </summary>
	/// <remarks>This class is used only by several functions in the MAIN class. It is not allowed to override this class.</remarks>
	[SupportedOSPlatform("windows")]
	public struct DialogsReturner
	{
		private string ERC;
		private string FNM;
		private string FNMFP;
		private FileDialogType FT;
		private System.String FTD;

		/// <summary>Initializes a new instance of the <see cref="DialogsReturner"/> structure. </summary>
		public DialogsReturner() { }

		/// <summary>
		/// Returns the dialog used when this class was constructed.
		/// </summary>
		public FileDialogType DialogType
		{
			get { return FT; }
			set { FT = value; }
		}

		/// <summary>
		/// The Error code string that is returned. A generic error is indicated by the <c>"Error"</c> <see cref="System.String"/>.
		/// </summary>
		public string ErrorCode
		{
			get { return ERC; }
			set { ERC = value; }
		}

		/// <summary>
		/// The file name returned by the dialog. (Example: That.txt)
		/// </summary>
		public string FileNameOnly
		{
			get { return FNM; }
			set
			{
				if (FT == FileDialogType.DirSelect)
				{
					throw new InvalidOperationException("Not allowed to set the File Path when the dialog is initialised for directories!!");
				}
				FNM = value;
			}
		}

		/// <summary>
		/// The full path of the file returned by the dialog (Example: C:\That.txt)
		/// </summary>
		public string FileNameFullPath
		{
			get { return FNMFP; }
			set
			{
				if (FT == FileDialogType.DirSelect)
				{
					throw new InvalidOperationException("Not allowed to set the File Path when the dialog is initialised for directories!!");
				}
				FNMFP = value;
			}
		}
		/// <summary>
		/// The directory that was returned from the dialog. It returns null when the dialog invoked is NOT a Directory Select dialog.
		/// </summary>
		public System.String DirPath
		{
			get { return FTD; }
			set
			{
				if (FT != FileDialogType.DirSelect)
				{
					throw new InvalidOperationException("Not allowed to set the Directory Path when the dialog is initialised for files!!");
				}
				FTD = value;
			}
		}
	}

#endif

	/// <summary>
	/// An enumeration of values which help the function <see cref="MAIN.GetACryptographyHashForAFile(string, HashDigestSelection)"/> to properly select the algorithm requested.
	/// </summary>
	public enum HashDigestSelection
	{
		/// <summary> RSVD </summary>
		Default_None = 0,
		/// <summary> RSVD </summary>
		RSVD_0 = 1,
		/// <summary> RSVD </summary>
		RSVD_1 = 2,
		/// <summary> RSVD </summary>
		RSVD_2 = 3,
		/// <summary> RSVD </summary>
		RSVD_3 = 4,
		/// <summary>
		/// The SHA1 Digest will be used.
		/// </summary>
		/// <remarks>Microsoft has detected that the algorithm produces the same result in slightly different files.
		/// If your case is the integrity , you should use then the <see cref="HashDigestSelection.SHA256"/> or a better algorithm.</remarks>
		SHA1 = 5,
		/// <summary>
		/// The SHA256 Digest will be used.
		/// </summary>
		SHA256 = 6,
		/// <summary>
		/// The SHA384 Digest will be used.
		/// </summary>
		SHA384 = 7,
		/// <summary>
		/// The SHA512 Digest will be used.
		/// </summary>
		SHA512 = 8,
		/// <summary>
		/// The MD5 Digest will be used.
		/// </summary>
		MD5 = 9
	}

	internal readonly struct HW31Mapper
	{
		public static readonly System.Object[,] Mapper = { { 0 , "AA" } , { 1 , "AB" } , { 2 , "AC" } , { 3 , "AD" } , { 4 , "AE" } , { 5 , "AF" } ,
		{ 6 , "AG" } , { 7 , "AH" } , { 8 , "AI" } , { 9 , "AJ" } , {10 , "AK" } , {11 , "AL" } , {12 , "AM" } , { 13 , "AN" } , { 14 , "AO" } ,
		{ 15 , "AP" } , { 16 , "AQ" } , { 17 , "AR" } , { 18 , "AS" } , { 19 , "AT" } , { 20 , "AU" } , { 21 , "AV" } , { 22 , "AW" } , { 23 , "AX" },
		{ 24 , "AY" } , { 25 , "AZ" } , { 26 , "Aa" } , { 27 , "Ab" } , { 28 , "Ac" } , { 29 , "Ad" } , { 30 , "Ae" } , { 31 , "Af" } , { 32 , "Ag" },
		{ 33 , "Ah" } , { 34 , "Ai" } , { 35 , "Aj" } , { 36 , "Ak" } , { 37  , "Al" } , { 38 , "Am" } , { 39 , "An" } , { 40 , "Ao" } , { 41 , "Ap" },
		{ 42 , "Aq" } , { 43 , "Ar" } , { 44 , "As" } , { 45 , "At" } , { 46 , "Au" } , { 47 , "Av" } , { 48 , "Aw" } , { 49 , "Ax" } , { 50 , "Ay" },
		{ 51 , "Az" } , { 52 , "aA" } , { 53 , "aB" } , { 54 , "aC" } , { 55 , "aD" } , { 56 , "aE" } , { 57 , "aF" } , { 58 , "aG" } , { 59 , "aH" },
		{ 60 , "aI" } , { 61 , "aJ" } , { 62 , "aK" } , { 63 , "aL" } , { 64 , "aM" } , { 65 , "aN" } , { 66 , "aO" } , { 67 , "aP" } , { 68 , "aQ" },
		{ 69 , "aR" } , { 70 , "aS" } , { 71 , "aT" } , { 72 , "aU" } , { 73 , "aV" } , { 74 , "aW" } , { 75 , "aX" } , { 76 , "aY" } , { 77 , "aZ" },
		{ 78 , "aa" } , { 79 , "ab" } , { 80 , "ac" } , { 81 , "ad" } , { 82 , "ae" } , { 83 , "af" } , { 84 , "ag" } , { 85 , "ah" } , { 86 , "ai" },
		{ 87 , "aj" } , { 88 , "ak" } , { 89 , "al" } , { 90 , "am" } , { 91 , "an" } , { 92 , "ao" } , { 93 , "ap" } , { 94 , "aq" } , { 95 , "ar" },
		{ 96 , "as" } , { 97 , "at" } , { 98 , "au" } , { 99 , "av" } , { 100 , "aw" } , { 101 , "ax" } , { 102 , "ay" } , { 103 , "az" } ,
		{ 104 , "BA" } , { 105 , "BB" } , { 106 , "BC" } , { 107 , "BD" } , { 108 , "BE" } , { 109 , "BF" } , { 110 , "BG" } , { 111 , "BH" },
		{ 112 , "BI" } , { 113 , "BJ" } , { 114 , "BK" } , { 115 , "BL" } , { 116 , "BM" } , { 117 , "BO" } , { 118 , "BP" },
		{ 119 , "BQ" } , { 120 , "BR" } , { 121 , "BS" } , { 122 , "BT" } , { 123 , "BU" } , { 124 , "BV" } , { 125 , "BW" } , { 126 , "BX" },
		{ 127 , "BY" } , { 128 , "BZ" } , { 129 , "Ba" } , { 130 , "Bb" } , { 131 , "Bc" } , { 132 , "Bd" } , { 133 , "Be" } , { 134 , "Bf" },
		{ 135 , "Bg" } , { 136 , "Bh" } , { 137 , "Bi" } , { 138 , "Bj" } , { 139 , "Bk" } , { 140 , "Bl" } , { 141 , "Bm" } , { 142 , "Bn" } ,
		{ 143 , "Bo" } , { 144 , "Bp" } , { 145 , "Bq" } , { 146 , "Br" } , { 147 , "Bs" } , { 148 , "Bt" } , { 149 , "Bu" } , { 150 , "Bv" },
		{ 151 , "Bw" } , { 152 , "Bx" } , { 153 , "By" } , { 154 , "Bz" } , { 155 , "bA" } , { 156 , "bB" } , { 157 , "bC" } , { 158 , "bD" },
		{ 159 , "bE" } , { 160 , "bF" } , { 161 , "bG" } , { 162 , "bH" } , { 163 , "bI" } , { 164 , "bJ" } , { 165 , "bK" } , { 166 , "bL" },
		{ 167 , "bM" } , { 168 , "bN" } , { 169 , "bP" } , { 170 , "bQ" } , { 171 , "bR" } , { 172 , "bS" } , { 173 , "bT" },
		{ 174 , "bU" } , { 175 , "bV" } , { 176 , "bW" } , { 177 , "bX" } , { 178 , "bY" } , { 179 , "bZ" } , { 180 , "ba" } , { 181 , "bb" },
		{ 182 , "bc" } , { 183 , "bd" } , { 184 , "be" } , { 185 , "bf" } , { 186 , "bg" } , { 187 , "bh" } , { 188 , "bi" } , { 189 , "bj" },
		{ 190 , "bk" } , { 191 , "bl" } , { 192 , "bm" } , { 193 , "bn" } , { 194 , "bo" } , { 195 , "bp" } , { 196 , "bq" } , { 197 , "br" },
		{ 198 , "bs" } , { 199 , "bt" } , { 200 , "bu" } , { 201 , "bv" } , { 202 , "bw" } , { 203 , "bx" } , { 204 , "by" } , { 205 , "bz" },
		{ 206 , "CA" } , { 207 , "CB" } , { 208 , "CC" } , { 209 , "CD" } , { 210 , "CE" } , { 211 , "CF" } , { 212 , "CG" } , { 213 , "CH" },
		{ 214 , "CI" } , { 215 , "CJ" } , { 216 , "CK" } , { 217 , "CL" } , { 218 , "CM" } , { 219 , "CN" } , { 220 , "CO" } , { 221 , "CP" },
		{ 222 , "CQ" } , { 223 , "CR" } , { 224 , "CS" } , { 225 , "CT" } , { 226 , "CU" } , { 227 , "CV" } , { 228 , "CW" } , { 229 , "CX" },
		{ 230 , "CY" } , { 231 , "CZ" } ,  { 232 , "Ca" } , { 233 , "Cb" } , { 234 , "Cc" } , { 235 , "Cd" } , { 236 , "Cf" } , { 237 , "Cg" },
		{ 238 , "Ch" } , { 239 , "Ci" } , { 240 , "Cj" } , { 241 , "Ck" } , { 242 , "Cl" } , { 243 , "Cm" } , { 244 , "Cn" } , { 245 , "Co" },
		{ 246 , "Cp" } , { 247 , "Cq" } , { 248 , "Cr" } , { 249 , "Cs" } , { 250 , "Ct" } , { 251 , "Cu" } , { 252 , "Cv" } , { 253 , "Cw" },
		{ 254 , "Cx" } , { 255 , "Cy" } };
	}

	/// <summary>
	/// A static class which constructs HW31 strings from <see cref="System.Byte"/>[] arrays.
	/// </summary>
	public static class HW31Strings
	{
		/*
		 * Where this class is useful? 
		 * > The class , like Base64 , creates a string representation of the byte array given.
		 *    However , Base64 and HW31 have differences:
		 *    HW31 allocates two unique characters representing each byte; Base64 creates the next character based on the last one and the next byte value.
		 *    Base64 leaves null characters at the end of the string(Interpreted as '=') , while HW31 leaves always only a space in the end of the string.
		 * > HW31 will also always produce the same result , no matter how, except in the case of corrupt string , which even then will return zero's.
		 * > HW31 has a dictionary which allows it to pick the appropriate byte each time and translate it into a HW31.
		 * Is it reliable to save byte data on an HW31 string?
		 * It depends on what you will use it. For data encryption keys , better is the Base64;
		 * For small binary data , HW31 will do the work fine.
		 * HW31 could be also used in small data dictionaries , where data precision is required.
		 */

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		private static System.String ByteToCorrespondingChars(System.Byte Value)
		{
			for (System.Int32 I = 0; I < HW31Mapper.Mapper.Length; I++)
			{
				if (((System.Int32)HW31Mapper.Mapper[I, 0]) == Value) { return $"{HW31Mapper.Mapper[I, 1]}"; }
			}
			return "Error";
		}

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		private static System.Byte CharsToCorrespondingByte(System.String Chars)
		{
			if (Chars.Length > 2 && Chars.Length < 2) { return 0; }
			for (System.Int32 I = 0; I < HW31Mapper.Mapper.Length; I++)
			{
				if ((System.String)HW31Mapper.Mapper[I, 1] == Chars) { return (System.Byte)HW31Mapper.Mapper[I, 0]; }
			}
			return 0;
		}

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		private static System.Boolean TestIfItIsAnHW31String(System.String HW31)
		{
			if (HW31 == null) { return false; }
			if (HW31.Length < 3) { return false; }
			System.Char[] HW31Arr = HW31.ToCharArray();
			if (HW31Arr[2] != ' ') { return false; }
			if (HW31Arr[HW31Arr.Length - 1] != ' ') { return false; }
			for (System.Int32 I = 0; I < HW31Arr.Length; I++)
			{
				try
				{
					if (IsDigit(System.Convert.ToInt32(HW31Arr[I])) == true) { return false; }
				}
				catch { continue; }
			}
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsDigit(System.Int32 I) { return (System.UInt32)(I - '0') <= ('9' - '0'); }

		/// <summary>
		/// Converts a <see cref="System.Byte"/>[] array to a new HW31 <see cref="System.String"/>. 
		/// </summary>
		/// <param name="Array">The Byte array to get the data from.</param>
		/// <returns>A new HW31 <see cref="System.String"/> . </returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static HW31 ByteArrayToHW31String(System.Byte[] Array)
		{
			HW31 DC = new HW31();
			if (Array.Length < 1) { DC.SetOrGetError = true; return DC; }
			System.String Result = null;
			for (System.Int32 I = 0; I < Array.Length; I++)
			{
				if (ByteToCorrespondingChars((System.Byte)HW31Mapper.Mapper[Array[I], 0]) != "Error")
				{
					Result += (ByteToCorrespondingChars((System.Byte)HW31Mapper.Mapper[Array[I], 0]) + " ");
				}
				else { DC.SetOrGetError = true; return DC; }
			}
			DC = new HW31(Result);
			return DC;
		}

		/// <summary>
		/// Converts a <see cref="System.Byte"/>[] array to a new HW31 <see langword="struct"/>. 
		/// </summary>
		/// <param name="Array">The Byte array to get the data from.</param>
		/// <param name="Count">How many iterations will happen to the array.</param>
		/// <param name="Start">From which point the iterator will start calculating.</param>
		/// <returns>A new HW31 <see langword="struct"/> .</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static HW31 ByteArrayToHW31String(System.Byte[] Array, System.Int32 Start, System.Int32 Count)
		{
			HW31 DC = new HW31();
			if (Array.Length < 1) { DC.SetOrGetError = true; return DC; }
			if (Start < 0) { DC.SetOrGetError = true; return DC; }
			if (Start > Count) { DC.SetOrGetError = true; return DC; }
			if (Count < 1) { DC.SetOrGetError = true; return DC; }
			System.String Result = null;
			for (System.Int32 I = Start; I < Count; I++)
			{
				if (ByteToCorrespondingChars((System.Byte)HW31Mapper.Mapper[Array[I], 0]) != "Error")
				{
					Result += (ByteToCorrespondingChars((System.Byte)HW31Mapper.Mapper[Array[I], 0]) + " ");
				}
				else { DC.SetOrGetError = true; return DC; }
			}
			DC = new HW31(Result);
			return DC;
		}

		/// <summary>
		/// Calculates the length of the HW31 string before it is created.
		/// </summary>
		/// <param name="Array">The <see cref="System.Byte"/>[] to calculate the data from.</param>
		/// <returns>The estimated HW31 <see langword="struct"/> containing length.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.Int64 EstimateHW31StringLength(System.Byte[] Array) { return Array.Length * 3; }

		/// <summary>
		/// Converts a created HW31 <see cref="System.String"/> back to a <see cref="System.Byte"/>[] array.
		/// </summary>
		/// <param name="HW31String">The already created HW31 <see cref="System.String"/>. </param>
		/// <returns>A new <see cref="System.Byte"/>[] containing the byte data kept by the HW31 string.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.Byte[] HW31StringToByteArray(HW31 HW31String)
		{
			if (TestIfItIsAnHW31String(HW31String.ReturnHW31) == false) { return null; }
			System.Char[] HW31Arr = HW31String.ReturnHW31.ToCharArray();
			if (HW31Arr[HW31Arr.Length - 1] != ' ') { return null; }
			System.Int32 BlanksToRemove = 0;
			for (System.Int32 I = 0; I < HW31Arr.Length; I++) { if (HW31Arr[I] == ' ') { BlanksToRemove += 2; } }
			System.Byte[] Result = new System.Byte[HW31Arr.Length - BlanksToRemove];
			System.String _Tmp = null;
			System.Int32 Count = 0;
			System.Int32 ArrCount = 0;
			try
			{
				for (System.Int32 I = 0; I < HW31Arr.Length; I++)
				{
					if (HW31Arr[I] != ' ') { _Tmp += HW31Arr[I]; Count++; }
					if (Count >= 2) { Result[ArrCount] = CharsToCorrespondingByte(_Tmp); Count = 0; ArrCount++; _Tmp = null; }
				}
			}
			catch (System.Exception) { return null; }
			return Result;
		}

	}

	/// <summary>
	/// The HW31 structure. HW31 is an intermediate storage to store binary data to <see cref="System.String"/>'s and the opposite.
	/// </summary>
#nullable enable
	[Serializable]
	public struct HW31 : IEquatable<HW31?>
	{
		private System.String BackField;
		private System.Boolean Erro_r = false;

		internal System.String ReturnHW31 { get { return BackField; } }
		internal System.Boolean SetOrGetError { get { return Erro_r; } set { Erro_r = value; } }

		/// <summary>
		/// Returns a <see cref="System.Boolean"/> value , indicating that this HW31 is invalid and should be destroyed.
		/// </summary>
		/// <returns><c>true</c> if this structure is invalid; otherwise , <c>false</c> if it is usuable.</returns>
		public System.Boolean IsInvalid() { return Erro_r; }
		/// <summary>
		/// Create a new HW31 structure.
		/// </summary>
		/// <param name="HW31">The HW31 <see cref="System.String"/> to create from.</param>
		/// <exception cref="System.InvalidOperationException">
		/// The <see cref="System.String"/> attempted to set was not in the HW31 format.</exception>
		public HW31(System.String HW31)
		{
			if (TestIfItIsAnHW31String(HW31) == false)
			{
				throw new System.InvalidOperationException("Invalid attempt to set a string which is not an HW31 one.");
			}

			BackField = HW31;
		}

		/// <summary>Initialises a new instance of the <see cref="HW31"/> structure.</summary>
		public HW31() { BackField = ""; }

		private static System.Boolean TestIfItIsAnHW31String(System.String HW31)
		{
			if (HW31 == null) { return false; }
			if (HW31.Length < 3) { return false; }
			System.Char[] HW31Arr = HW31.ToCharArray();
			if (HW31Arr[2] != ' ') { return false; }
			if (HW31Arr[HW31Arr.Length - 1] != ' ') { return false; }
			for (System.Int32 I = 0; I < HW31Arr.Length; I++)
			{
				try
				{
					if (IsDigit(System.Convert.ToInt32(HW31Arr[I])) == true) { return false; }
				}
				catch { continue; }
			}
			return true;
		}

		/// <summary>
		/// Detects if the specified <see cref="System.String"/> is an HW31 <see cref="System.String"/>.
		/// </summary>
		/// <param name="HW31">The HW31 <see cref="System.String"/> to test.</param>
		/// <returns><c>true</c> if the <paramref name="HW31"/> 
		/// can be an HW31 <see langword="struct"/>; otherwise , <c>false</c>.</returns>
		public static System.Boolean IsHW31(System.String HW31) { return TestIfItIsAnHW31String(HW31); }

		/// <summary>
		/// Test if an <see cref="HW31"/> structure that holds the HW31 <see cref="System.String"/> 
		/// is equal to a non-structured HW31 <see cref="System.String"/>.
		/// </summary>
		/// <param name="left">The <see cref="HW31"/> structure to take the <see cref="System.String"/> from.</param>
		/// <param name="right">The non-structured <see cref="System.String"/> to compare against.</param>
		/// <returns><c>true</c> if these two objects specified are equal; otherwise ,  <c>false</c>.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.Boolean operator ==(HW31 left, System.String right) { return left.ReturnHW31.Equals(right); }

		/// <summary>
		/// Test if an <see cref="HW31"/> structure that holds the HW31 <see cref="System.String"/> 
		/// is NOT equal to a non-structured HW31 <see cref="System.String"/>.
		/// </summary>
		/// <param name="left">The <see cref="HW31"/> structure to take the <see cref="System.String"/> from.</param>
		/// <param name="right">The non-structured <see cref="System.String"/> to compare against.</param>
		/// <returns><c>true</c> if these two objects specified are NOT equal; otherwise ,  <c>false</c>.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.Boolean operator !=(HW31 left, System.String right) { return left.ReturnHW31.Equals(right) == false; }

		/// <summary>
		/// Test if an <see cref="HW31"/> structure is equal to another.
		/// </summary>
		/// <param name="lhs">The first structure.</param>
		/// <param name="rhs">The second structure.</param>
		/// <returns><c>true</c> if the structures are equal; otherwise , <c>false</c>.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.Boolean operator ==(HW31 lhs, HW31 rhs) { return lhs.Equals(rhs); }

		/// <summary>
		/// Test if an <see cref="HW31"/> structure is NOT equal to another.
		/// </summary>
		/// <param name="lhs">The first structure.</param>
		/// <param name="rhs">The second structure.</param>
		/// <returns><c>true</c> if the structures are NOT equal; otherwise , false.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.Boolean operator !=(HW31 lhs, HW31 rhs) { return lhs.Equals(rhs) == false; }

		/// <summary>
		/// Test if an generic object is equal to this HW31 instance structure.
		/// </summary>
		/// <param name="obj">The generic object to compare.</param>
		/// <returns><c>true</c> if this structure is equal to the <paramref name="obj"/>;
		/// otherwise , <c>false</c>.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public override System.Boolean Equals(object obj) { return System.String.Equals(BackField, obj); }

		/// <summary>
		/// Test if another nullable HW31 construct is equal to this HW31 instance structure.
		/// </summary>
		/// <param name="Struct">The nullable HW31 construct to compare.</param>
		/// <returns><c>true</c> if this structure is equal to the <paramref name="Struct"/>;
		/// otherwise , <c>false</c>.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public System.Boolean Equals(HW31? Struct) { if (Struct?.Equals(this) == true) { return true; } else { return false; } }

		/// <inheritdoc />
		public override System.Int32 GetHashCode() { return BackField.GetHashCode(); }

		/// <summary>
		/// Gets the length of the HW31 <see cref="System.String"/>.
		/// </summary>
		/// <returns>The computed length.</returns>
		public System.Int32 Length() { return BackField.Length; }

		/// <summary>
		/// Gets the length of the HW31 <see cref="System.String"/> , but only the real interpreted characters.
		/// </summary>
		/// <returns>The computed length.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public System.Int32 ClearLength()
		{
			System.Char[] Chars = BackField.ToCharArray();
			System.Int32 Remove = 0;
			for (System.Int32 I = 0; I < Chars.Length; I++) { if (Chars[I] == ' ') { Remove++; } }
			return Chars.Length - Remove;
		}

		// The below code is residing in the System.Buffers.Text namespace , which is a method for the internal mechanisms.
		// Here it is used for checking if the HW31 has digits , which is illegal.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsDigit(System.Int32 I) { return (System.UInt32)(I - '0') <= ('9' - '0'); }

		/// <summary>
		/// Returns the real HW31 <see cref="System.String"/> created.
		/// </summary>
		/// <returns>The HW31 <see cref="System.String"/> .</returns>
		public override System.String ToString()
		{
			if (Erro_r == true)
			{
				throw new InvalidOperationException("Cannot use this HW31 instance " +
				"because this structure is marked as invalid.");
			}
			return BackField.ToString();
		}

	}
#nullable disable

	/// <summary>
	/// The Registry Types that the user can use to set the data to a value.
	/// </summary>
	public enum RegTypes
	{
		/// <summary>
		/// Reserved property. Should not be used directly by your source code.
		/// </summary>
		ERROR = 0,
		/// <summary> RSVD </summary>
		RSVD_0 = 1,
		/// <summary> RSVD </summary>
		RSVD_1 = 2,
		/// <summary> RSVD </summary>
		RSVD_2 = 3,
		/// <summary> RSVD </summary>
		RSVD_3 = 4,
		/// <summary> RSVD </summary>
		RSVD_4 = 5,
		/// <summary>
		/// The Registry type will be a string value.
		/// </summary>
		String = 6,
		/// <summary>
		/// The Registry type will be an environment variable string value.
		/// </summary>
		ExpandString = 7,
		/// <summary>
		/// The Registry type will be an quad-word byte array value.
		/// </summary>
		QuadWord = 8,
		/// <summary>
		/// The Registry type will be an double-word byte array value.
		/// </summary>
		DoubleWord = 9
	}

	/// <summary>
	/// The <see cref="RegEditor"/> instance class functions result after executing.
	/// </summary>
	public enum RegFunctionResult
	{
		/// <summary>
		/// Generic error.
		/// </summary>
		Error = 0,
		/// <summary> RSVD </summary>
		RSVD_0 = 1,
		/// <summary> RSVD </summary>
		RSVD_1 = 2,
		/// <summary>
		/// Incorrect Registry Path.
		/// </summary>
		/// <remarks>This is mostly returned when the path is incorrect or it is an registry error.</remarks>
		Misdefinition_Error = 3,
		/// <summary>
		/// The Root Key provided is invalid.
		/// </summary>
		InvalidRootKey = 4,
		/// <summary>
		/// Sucessfull execution.
		/// </summary>
		Success = 5
	}

	/// <summary>
	/// Valid root paths for the <see cref="RegEditor"/> to modify or create new values.
	/// </summary>
	public enum RegRootKeyValues
	{
		/// <summary>
		/// Reserved property for indicating a custom or unusuable root key.
		/// </summary>
		Inabsolute = 0,
		/// <summary> RSVD </summary>
		RSVD_0 = 1,
		/// <summary> RSVD </summary>
		RSVD_1 = 2,
		/// <summary> HKLM Path. </summary>
		HKLM = 3,
		/// <summary> HKCU Path. </summary>
		HKCU = 4,
		/// <summary> HKCC Path. </summary>
		HKCC = 5,
		/// <summary> HKPD Path. </summary>
		HKPD = 6,
		/// <summary> HKU Path. </summary>
		HKU = 7,
		/// <summary> HKCR Path. </summary>
		HKCR = 8,
		/// <summary>
		/// This provides the path to the Local Machine.
		/// </summary>
		LocalMachine = HKLM,
		/// <summary>
		/// This is the root path of the current user.
		/// </summary>
		CurrentUser = HKCU,
		/// <summary>
		/// This is the root path of the Current Config key.
		/// </summary>
		/// <remarks>This property is deprecated on the registry; use it with caution.</remarks>
		CurrentConfig = HKCC,
		/// <summary>
		/// This is the root path of the Performance Data key.
		/// </summary>
		/// <remarks>This property is deprecated on the registry; use it with caution.</remarks>
		PerfData = HKPD,
		/// <summary>
		/// This is the root path of the Users Data key.
		/// </summary>
		UsersStore = HKU,
		/// <summary>
		/// This is the root path of the Classes Data Root key.
		/// </summary>
		CurrentClassesRoot = HKCR
	}

	/// <summary>
	/// An easy to use Windows Registry Editor.
	/// </summary>
	[SupportedOSPlatform("windows")]
	public class RegEditor : System.IDisposable
	{
		private System.String _RootKey_;
		private System.String _SubKey_;
		private System.Boolean _DIAG_;

		/// <summary>
		/// The Registry Root Key. It accepts only specific values.
		/// </summary>
		public RegRootKeyValues RootKey
		{
			get
			{
				switch (_RootKey_)
				{
					case "HKEY_LOCAL_MACHINE": return RegRootKeyValues.HKLM;
					case "HKEY_CURRENT_USER": return RegRootKeyValues.HKCU;
					case "HKEY_CURRENT_CONFIG": return RegRootKeyValues.HKCC;
					case "HKEY_PERFORMANCE_DATA": return RegRootKeyValues.HKPD;
					case "HKEY_USERS": return RegRootKeyValues.HKU;
					case "HKEY_CLASSES_ROOT": return RegRootKeyValues.HKCR;
					default: return RegRootKeyValues.Inabsolute;
				}
			}
			set
			{
				switch (value)
				{
					case RegRootKeyValues.HKLM: _RootKey_ = "HKEY_LOCAL_MACHINE"; break;
					case RegRootKeyValues.HKCU: _RootKey_ = "HKEY_CURRENT_USER"; break;
					case RegRootKeyValues.HKCC: _RootKey_ = "HKEY_CURRENT_CONFIG"; break;
					case RegRootKeyValues.HKPD: _RootKey_ = "HKEY_PERFORMANCE_DATA"; break;
					case RegRootKeyValues.HKU: _RootKey_ = "HKEY_USERS"; break;
					case RegRootKeyValues.HKCR: _RootKey_ = "HKEY_CLASSES_ROOT"; break;
				}
			}
		}

		/// <summary>
		/// The Registry sub-root key. Can be nested the one on the another.
		/// </summary>
		public System.String SubKey
		{
			get { return _SubKey_; }
			set { if (System.String.IsNullOrEmpty(value) == false) { _SubKey_ = value; } }
		}

		/// <summary>
		/// The default , classical and parameterless constructor.
		/// </summary>
		/// <remarks>You must set the required Registry Paths by the respective properties.</remarks>
		public RegEditor() { }

		/// <summary>
		/// Constructor which can be used to set the required Registry Paths on initialisation.
		/// </summary>
		/// <param name="KeyValue">One of the valid Root Keys. See the <see cref="RegRootKeyValues"/> <see cref="System.Enum"/> for more information. </param>
		/// <param name="SubKey">The Registry sub-root key. Can be nested the one on the another.</param>
		public RegEditor(RegRootKeyValues KeyValue, System.String SubKey)
		{
			switch (KeyValue)
			{
				case RegRootKeyValues.HKLM: _RootKey_ = "HKEY_LOCAL_MACHINE"; break;
				case RegRootKeyValues.HKCU: _RootKey_ = "HKEY_CURRENT_USER"; break;
				case RegRootKeyValues.HKCC: _RootKey_ = "HKEY_CURRENT_CONFIG"; break;
				case RegRootKeyValues.HKPD: _RootKey_ = "HKEY_PERFORMANCE_DATA"; break;
				case RegRootKeyValues.HKU: _RootKey_ = "HKEY_USERS"; break;
				case RegRootKeyValues.HKCR: _RootKey_ = "HKEY_CLASSES_ROOT"; break;
			}
			if (System.String.IsNullOrEmpty(SubKey) == false) { _SubKey_ = SubKey; }
		}

		/// <summary>
		/// Enable the Console Diagnostic debugging.
		/// </summary>
		public System.Boolean DiagnosticMessages { set { _DIAG_ = value; } }

		private System.Boolean _CheckPredefinedProperties()
		{
			if ((System.String.IsNullOrEmpty(_RootKey_) == false) &&
				(System.String.IsNullOrEmpty(_SubKey_) == false)) { return true; }
			else { return false; }
		}

		/// <summary>
		/// Gets the specified value from the key provided.
		/// </summary>
		/// <param name="VariableRegistryMember">The value name to retrieve the value data.</param>
		/// <returns>If it succeeded , a new <see cref="System.Object"/> instance containing the data; Otherwise , a <see cref="System.String"/> explaining the error.</returns>
		public System.Object GetEntry(System.String VariableRegistryMember)
		{
			if (System.String.IsNullOrEmpty(VariableRegistryMember)) { return "Error"; }
			if (!_CheckPredefinedProperties())
			{
				if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
				return "UNDEF_ERR";
			}
			System.Object RegEntry = Microsoft.Win32.Registry.GetValue($"{_RootKey_}\\{_SubKey_}", VariableRegistryMember, "_ER_C_");
			if (System.Convert.ToString(RegEntry) == "_ER_C_") { return "Error"; }
			else
			{
				if (RegEntry is System.String[])
				{
					return RegEntry;
				}
				else if (RegEntry is System.Byte[])
				{
					return RegEntry;
				}
				else if (RegEntry is System.String) { return RegEntry; }
				else
				{
					if (_DIAG_)
					{
						MAIN.WriteConsoleText("Error - Could not translate the object returned by the procedure.");
						MAIN.WriteConsoleText("Please check that the entry is not broken , incorrect or in format that is not supported by this editor.");
					}
					return "Error";
				}
			}
		}

		/// <summary>
		/// Sets or creates the specified value.
		/// </summary>
		/// <param name="VariableRegistryMember">The value name whose data will be modified.</param>
		/// <param name="RegistryType">The value type that this value will have. Consult the <see cref="RegTypes"/> <see cref="System.Enum"/> for more information.</param>
		/// <param name="RegistryData">The new data that will be saved on the value; The type is depending upon the <paramref name="RegistryType"/> parameter.</param>
		/// <returns>A new <see cref="RegFunctionResult"/> <see cref="System.Enum"/> explaining if it succeeded.</returns>
		public RegFunctionResult SetEntry(System.String VariableRegistryMember, RegTypes RegistryType, System.Object RegistryData)
		{
			if (System.String.IsNullOrEmpty(VariableRegistryMember))
			{
				return RegFunctionResult.Error;
			}
			if (!_CheckPredefinedProperties())
			{
				if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
				return RegFunctionResult.Misdefinition_Error;
			}
			if (RegistryData == null)
			{
				if (_DIAG_) { MAIN.WriteConsoleText("ERROR: 'null' value detected in RegistryData object. Maybe invalid definition?"); }
				return RegFunctionResult.Misdefinition_Error;
			}
			Microsoft.Win32.RegistryValueKind RegType_;
			if (RegistryType == RegTypes.String)
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.String;
			}
			else if (RegistryType == RegTypes.ExpandString)
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.ExpandString;
			}
			else if (RegistryType == RegTypes.QuadWord)
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.QWord;
			}
			else if (RegistryType == RegTypes.DoubleWord)
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.DWord;
			}
			else
			{
				if (_DIAG_)
				{
					MAIN.WriteConsoleText($"ERROR: Unknown registry value type argument in the object creator was given: {RegistryType}");
				}
				return RegFunctionResult.InvalidRootKey;
			}
			try
			{
				Microsoft.Win32.Registry.SetValue($"{_RootKey_}\\{_SubKey_}", VariableRegistryMember, RegistryData, RegType_);
			}
			catch (System.Exception EX)
			{
				if (_DIAG_)
				{
					MAIN.WriteConsoleText($"ERROR: Could not create key {VariableRegistryMember} . Invalid name maybe?");
					MAIN.WriteConsoleText($"Error Raw Data: {EX}");
				}
				return RegFunctionResult.Error;
			}
			return RegFunctionResult.Success;
		}

		/// <summary>
		/// Deletes the specified value from the registry.
		/// </summary>
		/// <param name="VariableRegistryMember">The value which will be deleted.</param>
		/// <returns>A new <see cref="RegFunctionResult"/> <see cref="System.Enum"/> explaining if it succeeded.</returns>
		public RegFunctionResult DeleteEntry(System.String VariableRegistryMember)
		{
			if (System.String.IsNullOrEmpty(VariableRegistryMember)) { return RegFunctionResult.Error; }
			if (!_CheckPredefinedProperties())
			{
				if (_DIAG_) { MAIN.WriteConsoleText("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined."); }
				return RegFunctionResult.Misdefinition_Error;
			}
			Microsoft.Win32.RegistryKey ValueDelete;
			switch (_RootKey_)
			{
				case "HKEY_LOCAL_MACHINE":
					ValueDelete = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(_SubKey_);
					break;
				case "HKEY_CURRENT_USER":
					ValueDelete = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_SubKey_);
					break;
				case "HKEY_CURRENT_CONFIG":
					ValueDelete = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(_SubKey_);
					break;
				case "HKEY_PERFORMANCE_DATA":
					ValueDelete = Microsoft.Win32.Registry.PerformanceData.OpenSubKey(_SubKey_);
					break;
				case "HKEY_USERS":
					ValueDelete = Microsoft.Win32.Registry.Users.OpenSubKey(_SubKey_);
					break;
				case "HKEY_CLASSES_ROOT":
					ValueDelete = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(_SubKey_);
					break;
				default:
					if (_DIAG_)
					{
						MAIN.WriteConsoleText("Error - Registry root key could not be get. Incorrect Root Key Detected.");
						MAIN.WriteConsoleText("Error while getting the root key: Root Key " + _RootKey_ + "Is invalid.");
					}
					return RegFunctionResult.Misdefinition_Error;
			}
			if (System.Convert.ToString(ValueDelete.GetValue(VariableRegistryMember, "_DNE_")) == "_DNE_")
			{
				ValueDelete.Close();
				return RegFunctionResult.Error;
			}
			ValueDelete.DeleteValue(VariableRegistryMember);
			ValueDelete.Close();
			return RegFunctionResult.Success;
		}

		/// <summary>
		/// Creates a new key inside a sub-key or the root key.
		/// </summary>
		/// <param name="KeyName">The sub-key to create. If this parameter is not defined , 
		/// then it will create the sub-key name defined in the <see cref="SubKey"/> property.</param>
		/// <returns>A new <see cref="RegFunctionResult"/> enumeration , which indicates success or not.</returns>
		public RegFunctionResult CreateNewKey(System.String KeyName = "")
		{
			if (KeyName == "") { KeyName = _SubKey_; }
            Microsoft.Win32.RegistryKey ValueCreate;
			try
			{
				switch (_RootKey_)
				{
					case "HKEY_LOCAL_MACHINE":
						ValueCreate = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(KeyName);
						break;
					case "HKEY_CURRENT_USER":
						ValueCreate = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(KeyName);
						break;
					case "HKEY_CURRENT_CONFIG":
						ValueCreate = Microsoft.Win32.Registry.CurrentConfig.CreateSubKey(KeyName);
						break;
					case "HKEY_PERFORMANCE_DATA":
						ValueCreate = Microsoft.Win32.Registry.PerformanceData.CreateSubKey(KeyName);
						break;
					case "HKEY_USERS":
						ValueCreate = Microsoft.Win32.Registry.Users.CreateSubKey(KeyName);
						break;
					case "HKEY_CLASSES_ROOT":
						ValueCreate = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey(KeyName);
						break;
					default:
						if (_DIAG_)
						{
							MAIN.WriteConsoleText("Error - Registry root key could not be get. Incorrect Root Key Detected.");
							MAIN.WriteConsoleText("Error while getting the root key: Root Key " + _RootKey_ + "Is invalid.");
						}
						return RegFunctionResult.Misdefinition_Error;
				}
                ValueCreate.Flush();
                ValueCreate.Close();
            } catch (Exception) { return RegFunctionResult.Error; }
			return RegFunctionResult.Success;
        }

        /// <summary>
        /// Determines whether a specified registry key exists , it's path depends by the <see cref="RootKey"/>
        /// property and the <paramref name="KeyName"/> parameter.
        /// </summary>
        /// <param name="KeyName">The sub-key to find. If this parameter is not defined , 
        /// then it will find the sub-key name defined in the <see cref="SubKey"/> property.</param>
        /// <returns><see langword="true"/> if the key exists , otherwise <see langword="false"/>. </returns>
        public System.Boolean KeyExists(System.String KeyName = "")
		{
            if (KeyName == "") { KeyName = _SubKey_; }
            Microsoft.Win32.RegistryKey ValueFind;
            try
            {
                switch (_RootKey_)
                {
                    case "HKEY_LOCAL_MACHINE":
                        ValueFind = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_USER":
                        ValueFind = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CURRENT_CONFIG":
                        ValueFind = Microsoft.Win32.Registry.CurrentConfig.OpenSubKey(KeyName);
                        break;
                    case "HKEY_PERFORMANCE_DATA":
                        ValueFind = Microsoft.Win32.Registry.PerformanceData.OpenSubKey(KeyName);
                        break;
                    case "HKEY_USERS":
                        ValueFind = Microsoft.Win32.Registry.Users.OpenSubKey(KeyName);
                        break;
                    case "HKEY_CLASSES_ROOT":
                        ValueFind = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(KeyName);
                        break;
                    default:
						return false;
                }
                ValueFind.Close();
            }
            catch (Exception) { return false; }
			return true;
        }

		/// <summary>
		/// Use this Dispose method to clear up the current key that the class is working on and make it possible to set a new path to work on.
		/// </summary>
		public void Dispose() { DisposeRes(); }

		private protected void DisposeRes()
		{
			// Delete any unused values.
			_RootKey_ = null;
			_SubKey_ = null;
			_DIAG_ = false;
		}
	}

}