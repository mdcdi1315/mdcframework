
// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Compression;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        private static System.Boolean StartUWPApp(System.String URL , IWin32Window window)
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
        /// Opens a Windows UWP App.
        /// </summary>
        /// <param name="link">One of the values of the <see cref="SystemLinks"/> enumeration.</param>
		/// <param name="Window">The function will show messages to the specified window , if needed.</param>
        /// <returns>A <see cref="System.Boolean"/> determining whether the 
        /// specified UWP App was opened or restored.</returns>
        [SupportedOSPlatform("windows")]
        public static System.Boolean OpenSystemApp(SystemLinks link , IWin32Window Window) { return StartUWPApp(ToURL(link) , Window); }

		/// <summary>
		/// Creates a new Windows Internet Shortcut.
		/// </summary>
		/// <param name="URL">The URL that the shortcut will point to.</param>
		/// <param name="Path">The path of the shortcut that will be saved.</param>
		/// <returns>Returns <see langword="true"/> on success; <see langword="false"/> on error.</returns>
		public static System.Boolean CreateInternetShortcut(System.Uri URL , System.String Path)
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
        public static System.Boolean CreateInternetShortcut(System.Uri URL, System.String Path , System.Boolean OverwriteIfExists)
        {
            System.IO.FileStream Out = null;
            HTTPLIB.RequestBuilder req = HTTPLIB.Http.Get(URL.ToString());
            System.Boolean executed = false;
            System.Net.WebException err = null;
            req.OnSuccess(df => { executed = true; });
            req.OnFail(dg => { executed = true; err = dg; });
            req.Go();
            while (executed == false) { HaltApplicationThread(430); }
            req = null;
            if (err != null) { err = null; goto G_ExitErr; }
            err = null;
			if (FileExists(Path)) 
			{
				if (OverwriteIfExists) 
				{
					Out = ClearAndWriteAFile(Path);
					if (Out == null) { goto G_ExitErr; }
				} else { goto G_ExitErr; }
			} else 
			{
				Out = CreateANewFile(Path);
                if (Out == null) { goto G_ExitErr; }
            }
			PassNewContentsToFile(String.Format(MDCFR.Properties.Resources.MDCFR_INTS_CREATE , URL.ToString()) , Out);
			Out.Close();
			Out.Dispose();
			Out = null;
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
        public static System.Boolean OpenSystemApp(System.String link , IWin32Window Window) 
		{
            if (link.EndsWith("://") == false) { return false; }
            return StartUWPApp(link , Window);
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
		public static void EmitBeepSound() { Interop.Kernel32.ConsoleBeep(800 , 200); }

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
		/// Read a text line from console. This uses a custom inplementation so as to get the data.
		/// </summary>
		/// <param name="Opt">The Buffer Size to set. If left undefined , then it is <see cref="ConsoleExtensions.ConsoleReadBufferOptions.Default"/></param>
		/// <returns>The data read from the console. If any error found , then it will return the <c>"Error"</c> <see cref="System.String"/> .</returns>
        [SupportedOSPlatform("windows")]
        public static System.String ReadConsoleText(ROOT.ConsoleExtensions.ConsoleReadBufferOptions Opt = 
			ROOT.ConsoleExtensions.ConsoleReadBufferOptions.Default) { return global::ConsoleInterop.ReadFromConsole(Opt); }

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

		/// <summary>
		/// (For informational purposes only) The Current Visual Basic Runtime Library version that is used by the runtime.
		/// </summary>
		/// <returns>A <see cref="System.String"/> which contains the Visual Basic Runtime Library version.</returns>
		[System.Obsolete("The Visual Basic Runtime Library will be removed in the next major version." , true)]
		public static System.String GetVBRuntimeInfo()
		{
			return Microsoft.VisualBasic.Globals.ScriptEngine + " Engine , Version " +
			Microsoft.VisualBasic.Globals.ScriptEngineMajorVersion.ToString() + "." +
			Microsoft.VisualBasic.Globals.ScriptEngineMinorVersion.ToString() +
			"." + Microsoft.VisualBasic.Globals.ScriptEngineBuildVersion.ToString();
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
			if (System.Runtime.InteropServices.Architecture.X86 == RRD) { return "x86"; }
			else if (System.Runtime.InteropServices.Architecture.X64 == RRD) { return "AMD64"; }
			else if (System.Runtime.InteropServices.Architecture.Arm == RRD) { return "ARM"; }
			else if (System.Runtime.InteropServices.Architecture.Arm64 == RRD) { return "ARM64"; } else { return "Error"; }
		}

#endif

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
		public static System.String ChangeDefinedChar(System.String Stringtoreplacechar , 
			System.Char Chartochange , System.Char Chartobechanged)
		{
			System.Char[] array = Stringtoreplacechar.ToCharArray();
			if (array == null || array.Length < 1) { return "Error"; }
			System.String Result = null;
			for (System.Int32 I = 0; I < array.Length; I++)
			{
				if (array[I] == Chartochange) 
				{
					Result += Chartobechanged;
				} else { Result += array[I]; }
			}
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
			return Result;
		}

        /// <summary>
        /// This method requests from the user to supply a <see cref="System.String"/> and return the result to the caller.
        /// </summary>
        /// <param name="Prompt">How to prompt the user so as to type the correct <see cref="System.String"/> needed.</param>
        /// <param name="Title">The window's title.</param>
        /// <param name="DefaultResponse">The default response or an example on what the user should type.</param>
        /// <returns>If the user wrote something in the box and pressed 'OK' , then the <see cref="System.String"/> that supplied; otherwise , <c>null</c>.</returns>
        [System.Obsolete("The Visual Basic Runtime Library will be removed in the next major version. Use instead the GetAStringFromTheUserNew function.", true)]
        [SupportedOSPlatform("windows")]
		public static System.String GetAStringFromTheUser(System.String Prompt,
		System.String Title, System.String DefaultResponse)
		{
			System.String RETVAL = Microsoft.VisualBasic.Interaction.InputBox(Prompt, Title, DefaultResponse);
			if (RETVAL == "") { return null; } else { return RETVAL; }
		}

		/// <summary>
		/// This method requests from the user to supply a <see cref="System.String"/> and return the result to the caller.
		/// </summary>
		/// <param name="Prompt">How to prompt the user so as to type the correct <see cref="System.String"/> needed.</param>
		/// <param name="Title">The window's title.</param>
		/// <param name="DefaultResponse">The default response or an example on what the user should type.</param>
		/// <returns>If the user wrote something in the box and pressed 'OK' , then the <see cref="System.String"/> that supplied; otherwise , <c>null</c>.</returns>
		/// <remarks>Note: This uses the <see cref="ROOT.IntuitiveInteraction.GetAStringFromTheUser"/> class instead of the 
		/// <see cref="Microsoft.VisualBasic.Interaction.InputBox(string, string, string, int, int)"/> method.</remarks>
		[SupportedOSPlatform("windows")]
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String GetAStringFromTheUserNew(System.String Prompt,
		System.String Title, System.String DefaultResponse)
		{
			IntuitiveInteraction.GetAStringFromTheUser DZ = new(Prompt, Title, DefaultResponse);
			switch (DZ.ButtonClicked)
			{
				case ROOT.IntuitiveInteraction.ButtonReturned.NotAnAnswer: return null;
				default: return DZ.ValueReturned;
			}
		}

		/// <summary>
		/// Checks whether a file exists or not by the <paramref name="Path"/> supplied.
		/// </summary>
		/// <param name="Path">The <see cref="System.String"/> which is a filepath to check if the file exists.</param>
		/// <returns>If the file exists in the <paramref name="Path"/> supplied , then <c>true</c>; otherwise <c>false</c>.</returns>
		public static System.Boolean FileExists(System.String Path) { if (System.IO.File.Exists(Path)) { return true; } else { return false; } }

		/// <summary>
		/// Checks whether a directory exists or not by the <paramref name="Path"/> supplied.
		/// </summary>
		/// <param name="Path">The <see cref="System.String"/> which is a directory path to check if the directory exists.</param>
		/// <returns>If the directory exists in the <paramref name="Path"/> supplied , then <c>true</c>; otherwise <c>false</c>.</returns>
		public static System.Boolean DirExists(System.String Path) { if (System.IO.Directory.Exists(Path)) { return true; } else { return false; } }

		/// <summary>
		/// Creates a directory specified by the <paramref name="Path"/> parameter.
		/// </summary>
		/// <param name="Path">The directory path to create.</param>
		/// <returns><c>true</c> if the directory was created sucessfully; otherwise , <c>false</c>.</returns>
		public static System.Boolean CreateADir(System.String Path)
		{
			try { System.IO.Directory.CreateDirectory(Path); } catch (System.Exception) { return false; }
			return true;
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
			try { System.IO.Directory.Delete(Path, DeleteAll); } catch (System.Exception) { return false; }
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
			try { System.IO.Directory.Move(SourcePath, DestPath); } catch (System.Exception) { return false; }
			return true;
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
			try { System.IO.File.Copy(SourceFilePath, DestPath, OverWriteAllowed); } catch (System.Exception) { return false; }
			return true;
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
			catch (System.Exception) { return null; }
		}

		/// <summary>
		/// Creates a new and fresh file and opens a new handle for it by the <see cref="System.IO.FileStream"/>.
		/// </summary>
		/// <param name="Path">The file path where the file will be created. Example: <![CDATA[C:\Files\Start.txt]]> .</param>
		/// <returns>A new <see cref="System.IO.FileStream"/> object if no error occured; otherwise , <c>null</c>.</returns>
		public static System.IO.FileStream CreateANewFile(System.String Path) { try { return System.IO.File.OpenWrite(Path); } catch (System.Exception) { return null; } }

		/// <summary>
		/// Opens an handle for the existing file as a <see cref="System.IO.FileStream"/>.
		/// The file is opened with both Read and Write permissions.
		/// </summary>
		/// <param name="Path">The file path where the file is located to.</param>
		/// <returns>A new <see cref="System.IO.FileStream"/> if no errors found; otherwise , <c>null</c>.</returns>
		public static System.IO.FileStream ReadAFileUsingFileStream(System.String Path)
		{
			if (FileExists(Path) == false) { return null; }
			try { return System.IO.File.Open(Path, System.IO.FileMode.Open); } catch (System.Exception) { return null; }
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
			try { return System.IO.File.Open(Path, System.IO.FileMode.Truncate); } catch (System.Exception) { return null; }
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
				FileStreamObject.Write(EMDK, 0, EMDK.Length);
			}
			catch (System.Exception) { }
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
				FileStreamObject.Write(EMDK, System.Convert.ToInt32(FileStreamObject.Length), EMDK.Length);
			} catch (System.Exception) { }
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
					WriteConsoleText($"Error - Option {HashToSelect} Is Invalid!!!");
					return "Error";
			}
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
					WriteConsoleText("Error - Option " + HashToSelect + " Is Invalid!!!");
					return "Error";
			}
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
			try { System.IO.File.Delete(Path); } catch (System.Exception) { return false; }
			return true;
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
			System.Windows.Forms.DialogResult RET = System.Windows.Forms.MessageBox.Show(MessageString, Title, MessageButton, MessageIcon);
			switch (RET)
			{
				case System.Windows.Forms.DialogResult.OK: return 1;
				case System.Windows.Forms.DialogResult.Cancel: return 2;
				case System.Windows.Forms.DialogResult.Abort: return 3;
				case System.Windows.Forms.DialogResult.Retry: return 4;
				case System.Windows.Forms.DialogResult.Ignore: return 5;
				case System.Windows.Forms.DialogResult.Yes: return 6;
				case System.Windows.Forms.DialogResult.No: return 7;
				default: return 0;
			}
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
			ROOT.IntuitiveInteraction.IntuitiveMessageBox DX = new(MessageString, Title, MessageButton, IconToSelect);
			switch (DX.ButtonSelected)
			{
				case ROOT.IntuitiveInteraction.ButtonReturned.OK: return 1;
				case ROOT.IntuitiveInteraction.ButtonReturned.Cancel: return 2;
				case ROOT.IntuitiveInteraction.ButtonReturned.Abort: return 3;
				case ROOT.IntuitiveInteraction.ButtonReturned.Retry: return 4;
				case ROOT.IntuitiveInteraction.ButtonReturned.Ignore: return 5;
				case ROOT.IntuitiveInteraction.ButtonReturned.Yes: return 6;
				case ROOT.IntuitiveInteraction.ButtonReturned.No: return 7;
				default: return 0;
			}
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
		public static void HaltApplicationThread(System.Int32 TimeoutEpoch) { System.Threading.Thread.Sleep(TimeoutEpoch); }

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
			} else
			{
				FRM.StartInfo.UseShellExecute = true;
				if (HideExternalConsole == true)
				{
					FRM.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
				}
			}
			FRM.StartInfo.FileName = FinalFilePath;
			FRM.StartInfo.Arguments = CommandLineArgs;
			try { FRM.Start(); } catch (System.ComponentModel.Win32Exception)
			{
				FRM = null;
				return -10337880;
			} catch (System.Exception)
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
				} catch { return; }
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
				if (((System.Int32) HW31Mapper.Mapper[I, 0]) == Value) { return $"{HW31Mapper.Mapper[I, 1]}"; }
			}
			return "Error";
		}

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.Byte CharsToCorrespondingByte(System.String Chars)
		{
			if (Chars.Length > 2 && Chars.Length < 2) { return 0; }
			for (System.Int32 I = 0; I < HW31Mapper.Mapper.Length; I++)
			{
				if ( (System.String) HW31Mapper.Mapper[I, 1] == Chars) { return (System.Byte) HW31Mapper.Mapper[I, 0]; }
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
        private static bool IsDigit(System.Int32 I) { return (System.UInt32) (I - '0') <= ('9' - '0'); }

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
				if (ByteToCorrespondingChars((System.Byte) HW31Mapper.Mapper[Array[I], 0]) != "Error")
				{
					Result += (ByteToCorrespondingChars((System.Byte) HW31Mapper.Mapper[Array[I], 0]) + " ");
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
				if (ByteToCorrespondingChars((System.Byte) HW31Mapper.Mapper[Array[I], 0]) != "Error")
				{
					Result += (ByteToCorrespondingChars((System.Byte) HW31Mapper.Mapper[Array[I], 0]) + " ");
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
			} catch (System.Exception) { return null; }
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
				} catch { continue; }
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
			if (Erro_r == true) { throw new InvalidOperationException("Cannot use this HW31 instance " +
				"because this structure is marked as invalid."); }
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
				(System.String.IsNullOrEmpty(_SubKey_) == false)) { return true; } else { return false; }
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
				} else if (RegEntry is System.Byte[])
				{
					return RegEntry;
				} else if (RegEntry is System.String) { return RegEntry; }
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
			} else if (RegistryType == RegTypes.ExpandString)
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.ExpandString;
			} else if (RegistryType == RegTypes.QuadWord)
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.QWord;
			} else if (RegistryType == RegTypes.DoubleWord)
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

	namespace CryptographicOperations
	{
		// A Collection Namespace of encrypting and decrypting files.
		// For now (At the time of writing this code) , only UTF-8 is supported.

		/// <summary>
		/// A storage class used to take values from the randomizer (Function <see cref="AESEncryption.MakeNewKeyAndInitVector"/>).
		/// </summary>
		public class KeyGenTable
		{
			private System.String ERC;
			private System.Byte[] _IV_;
			private System.Byte[] _EK_;
			private System.Int32 EMF_;

			/// <summary>
			/// Returns the <c>"Error"</c> <see cref="System.String"/> 
			/// in case of error; otherwise , the <see cref="System.String"/> <c>"OK"</c>.
			/// </summary>
			public System.String ErrorCode
			{
				get { return ERC; }
				set { ERC = value; }
			}

			/// <summary>
			/// Create a new <see cref="AESEncryption"/> class , if the required data were got.
			/// </summary>
			public AESEncryption Create 
			{
				get 
				{
					if (CallerErroredOut == false)
					{
						AESEncryption EW = new();
						EW.EncryptionKey = _EK_;
						EW.IV = _IV_;
						return EW;
					} else
					{
						throw new InvalidOperationException("The data were not got , so it is not possible to instantiate a new AES Encryption class.");
					}
				} 
			}
			
			/// <summary>
			/// Returns a <see cref="System.Boolean"/> indicating whether the function or any function that uses this has errored out.
			/// </summary>
			public System.Boolean CallerErroredOut
			{
				get { if (ERC == null) { return false; } else { if (ERC == "Error") { return true; } else { return false; } } }
			}

			/// <summary>
			/// The Encryption key. Recommended to be more than 32 bytes.
			/// </summary>
			public System.Byte[] Key
			{
				get { return _EK_; }
				set { _EK_ = value; }
			}

			/// <summary>
			/// The initialisation vector to use. Recommended to be more than 16 bytes.
			/// </summary>
			public System.Byte[] IV
			{
				get { return _IV_; }
				set { _IV_ = value; }
			}

			/// <summary>
			/// Returns the actual message key length as of <see cref="System.Int32"/> units.
			/// </summary>
			public System.Int32 KeyLengthInBits
			{
				get { return EMF_; }
				set { EMF_ = value; }
			}
		}

		/// <summary>
		/// AES Encryption class. It can also encrypt files.
		/// NOTE: You are NOT allowed to override this class.
		/// </summary>
		/// <remarks>Only files with UTF-8 encoding can be sucessfully encrypted and decrypted for now.</remarks>
		public sealed class AESEncryption : System.IDisposable
		{
			// Cryptographic Operations Class.
			private System.Byte[] _EncryptionKey_;
			private System.Byte[] _InitVec_;
			private System.Security.Cryptography.AesCng CNGBaseObject = new();

			/// <summary>
			/// The encryption key to set for encoding/decoding.
			/// </summary>
			public System.Byte[] EncryptionKey { set { _EncryptionKey_ = value; } }

			/// <summary>
			/// The Initialisation Vector to set for encoding/decoding.
			/// </summary>
			public System.Byte[] IV { set { _InitVec_ = value; } }

            /// <summary>
            /// Create a new random key and Initialisation vector to use.
            /// </summary>
            /// <returns>A new <see cref="KeyGenTable"/> containing the key and the IV.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static KeyGenTable MakeNewKeyAndInitVector()
			{
				System.Security.Cryptography.AesCng RETM;
				KeyGenTable RDM = new();
				try { RETM = new System.Security.Cryptography.AesCng(); }
				catch (System.Exception) { RDM.ErrorCode = "Error"; return RDM; }
				RDM.ErrorCode = "OK";
				RDM.IV = RETM.IV;
				RDM.Key = RETM.Key;
				RDM.KeyLengthInBits = RETM.KeySize;
				return RDM;
			}

            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            private System.Boolean _CheckPredefinedProperties()
			{
				if ((_EncryptionKey_ is null) || (_InitVec_ is null)) { return true; }
				if ((_EncryptionKey_.Length <= 0) || (_InitVec_.Length <= 0)) { return true; }
				return false;
			}

            /// <summary>
            /// Encrypts the specified <see cref="System.String"/> plain text as <see cref="System.Byte"/>[] units.
            /// </summary>
            /// <param name="PlainText">The text to encrypt.</param>
            /// <returns>The encrypted AES CNG message.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.Byte[] EncryptSpecifiedData(System.String PlainText)
			{
				if (System.String.IsNullOrEmpty(PlainText))
				{
					return null;
				}
				if (_CheckPredefinedProperties())
				{
					return null;
				}
				System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
				ENC_1.Key = _EncryptionKey_;
				ENC_1.IV = _InitVec_;
				ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
				System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateEncryptor();
				System.Byte[] EncryptedArray;
				using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream())
				{
					using (System.Security.Cryptography.CryptoStream CryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR, ENC_2, System.Security.Cryptography.CryptoStreamMode.Write))
					{
						using (System.IO.StreamWriter SDM = new System.IO.StreamWriter(CryptStrEnc, System.Text.Encoding.UTF8))
						{
							SDM.Write(PlainText);
						}
						EncryptedArray = MSSSR.ToArray();
					}
				}
				return EncryptedArray;
			}

            /// <summary>
            /// Encrypts the alive <see cref="System.IO.FileStream"/> with all of it's containing data as <see cref="System.Byte"/>[] units.
            /// </summary>
            /// <param name="UnderlyingStream">The <see cref="System.IO.Stream"/> object to get data from.</param>
            /// <returns>The encrypted AES CNG message.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.Byte[] EncryptSpecifiedDataForFiles(System.IO.Stream UnderlyingStream)
			{
				if (!(UnderlyingStream is System.IO.FileStream) || (UnderlyingStream.CanRead == false)) { return null; }
				if (_CheckPredefinedProperties()) { return null; }
				System.Byte[] ByteArray = new System.Byte[UnderlyingStream.Length];
				UnderlyingStream.Read(ByteArray, 0, System.Convert.ToInt32(UnderlyingStream.Length));
				System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
				ENC_1.Key = _EncryptionKey_;
				ENC_1.IV = _InitVec_;
				ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
				System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateEncryptor();
				System.Byte[] EncryptedArray;
				using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream())
				{
					using (System.Security.Cryptography.CryptoStream CryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR, ENC_2, System.Security.Cryptography.CryptoStreamMode.Write))
					{
						using (System.IO.BinaryWriter SDM = new System.IO.BinaryWriter(CryptStrEnc, System.Text.Encoding.UTF8))
						{
							SDM.Write(ByteArray, 0, ByteArray.Length);
						}
						EncryptedArray = MSSSR.ToArray();
					}
				}
				return EncryptedArray;
			}

            /// <summary>
            /// Decrypts the encdoed AES CNG message to <see cref="System.String"/> units.
            /// </summary>
            /// <param name="EncryptedArray">The encrypted AES CNG message.</param>
            /// <returns>The decoded message , as <see cref="System.String"/> code units.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.String DecryptSpecifiedData(System.Byte[] EncryptedArray)
			{
				if ((EncryptedArray is null) || (EncryptedArray.Length <= 0))
				{
					return null;
				}
				if (_CheckPredefinedProperties()) { return null; }
				System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
				ENC_1.Key = _EncryptionKey_;
				ENC_1.IV = _InitVec_;
				ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
				System.String StringToReturn = null;
				System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateDecryptor();
				using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream(EncryptedArray))
				{
					using (System.Security.Cryptography.CryptoStream DCryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR, ENC_2, System.Security.Cryptography.CryptoStreamMode.Read))
					{
						using (System.IO.StreamReader SDE = new System.IO.StreamReader(DCryptStrEnc, System.Text.Encoding.UTF8))
						{
							StringToReturn = SDE.ReadToEnd();
						}
					}
				}
				return StringToReturn;
			}

            /// <summary>
            /// Decrypts the encdoed AES CNG message from an alive <see cref="System.IO.Stream"/> 
            /// object to <see cref="System.String"/> units.
            /// </summary>
            /// <param name="EncasingStream">The <see cref="System.IO.Stream"/> object which contains the encoded AES CNG message.</param>
            /// <returns>The decoded message , as <see cref="System.String"/> code units.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public System.String DecryptSpecifiedDataForFiles(System.IO.Stream EncasingStream)
			{
				if (EncasingStream.CanRead == false) { return null; }
				if (_CheckPredefinedProperties()) { return null; }
				System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
				ENC_1.Key = _EncryptionKey_;
				ENC_1.IV = _InitVec_;
				ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
				System.String FinalString = null;
				System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateDecryptor();
				using (System.Security.Cryptography.CryptoStream DCryptStrEnc = new System.Security.Cryptography.CryptoStream(EncasingStream, ENC_2, System.Security.Cryptography.CryptoStreamMode.Read))
				{
					using (System.IO.StreamReader SDE = new(DCryptStrEnc, System.Text.Encoding.UTF8)) { FinalString = SDE.ReadToEnd(); }
				}
				return FinalString;
			}

            /// <summary>
            /// Converts either the key or Initialisation Vector to a safety-secure Base64 <see cref="System.String"/>.
            /// </summary>
            /// <param name="ByteValue">The Key or Initalisation Vector to convert.</param>
            /// <returns>A new Base64 <see cref="System.String"/> , which contains the encoded key or initialisation vector.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static System.String ConvertTextKeyOrIvToString(System.Byte[] ByteValue)
			{
				if ((ByteValue is null) || (ByteValue.Length <= 0)) { return null; }
				try
				{
					ReadOnlySpan<System.Byte> In = ByteValue;
					Span<System.Byte> Out = new();
					if (System.Buffers.Text.Base64.EncodeToUtf8(In, Out, out System.Int32 BG, 
						out System.Int32 BW, true) == System.Buffers.OperationStatus.Done)
					{
						return System.Text.Encoding.UTF8.GetString(Out.ToArray());
					} else { return null; }
				}
				catch (System.Exception) { return null; }
			}

            /// <summary>
            /// Converts the converted key or Initialisation Vector from a Base64 <see cref="System.String"/> 
            /// back to a <see cref="System.Byte"/>[] array.
            /// </summary>
            /// <param name="StringValue">The Base64 encoded <see cref="System.String"/>.</param>
            /// <returns>The <see cref="System.Byte"/>[] before the conversion.</returns>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static System.Byte[] ConvertTextKeyOrIvFromStringToByteArray(System.String StringValue)
			{
				if (System.String.IsNullOrEmpty(StringValue)) { return null; }
				try
				{
					ReadOnlySpan<System.Byte> In = System.Text.Encoding.UTF8.GetBytes(StringValue);
					Span<System.Byte> Out = new();
					if (System.Buffers.Text.Base64.DecodeFromUtf8(In , Out , out System.Int32 BG,
						out System.Int32 ED , true) == System.Buffers.OperationStatus.Done) 
					{
						return Out.ToArray();
					} else { return null; }
				}
				catch (System.Exception) { return null; }
			}

			/// <summary>
			/// Use the <see cref="Dispose"/> method to clear up the current key and Initialisation Vector so as to prepare
			/// the encryptor/decryptor for a new session or to invalidate it.
			/// </summary>
			public void Dispose() { DISPMETHOD(); }

			private void DISPMETHOD()
			{
				_EncryptionKey_ = null;
				_InitVec_ = null;
#pragma warning disable CS0219
				System.Object CNGBaseObject = null;
#pragma warning restore CS0219
			}
		}

		/// <summary>
		/// An example class demonstrating how you can encrypt and decrypt UTF-8 files.
		/// </summary>
		public class EDAFile
		{
			/// <summary>
			/// An storage class used in the example.
			/// </summary>
			public class EncryptionContext
			{
				private System.String _ERC_;
				private System.Byte[] _KEY_;
				private System.Byte[] _IV_;

				/// <summary>
				/// The error code of the <see cref="EncryptAFile(string, string)"/> function.
				/// </summary>
				public System.String ErrorCode
				{
					get { return _ERC_; }
					set { _ERC_ = value; }
				}

				/// <summary>
				/// The key that the function <see cref="EncryptAFile(string, string)"/> used.
				/// </summary>
				public System.Byte[] KeyUsed
				{
					get { return _KEY_; }
					set { _KEY_ = value; }
				}

				/// <summary>
				/// The Initialisation Vector that the function <see cref="EncryptAFile(string, string)"/> used.
				/// </summary>
				public System.Byte[] InitVectorUsed
				{
					get { return _IV_; }
					set { _IV_ = value; }
				}
			}

			/// <summary>
			/// Encrypts the specified file and puts the encrypted contents to a new file.
			/// </summary>
			/// <param name="FilePath">The file to encrypt.</param>
			/// <param name="FileOutputPath">The file path to put th encrypted file.</param>
			/// <returns>A new <see cref="EncryptionContext"/> class containing the key used and the Initialisation Vector</returns>
			public static EncryptionContext EncryptAFile(System.String FilePath, System.String FileOutputPath = "")
			{
				if (!(System.IO.File.Exists(FilePath))) { return null; }
				if (System.String.IsNullOrEmpty(FileOutputPath))
				{
					FileOutputPath = (FilePath).Remove(FilePath.IndexOf(".")) + "_ENCRYPTED_" + (FilePath).Substring(FilePath.IndexOf("."));
				}
				EncryptionContext DMF = new EncryptionContext();
				AESEncryption MDA = new AESEncryption();
				KeyGenTable MAKER = AESEncryption.MakeNewKeyAndInitVector();
				if (MAKER.ErrorCode == "Error")
				{
					DMF.ErrorCode = "Error";
					return DMF;
				}
				MDA.EncryptionKey = MAKER.Key;
				MDA.IV = MAKER.IV;
				try
				{
					using (System.IO.FileStream MDR = new System.IO.FileStream(FilePath, System.IO.FileMode.Open))
					{
						using (System.IO.FileStream MNH = System.IO.File.OpenWrite(FileOutputPath))
						{
							System.Byte[] FLL = MDA.EncryptSpecifiedDataForFiles(MDR);
							MNH.Write(FLL, 0, System.Convert.ToInt32(FLL.Length));
						}
					}
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					DMF.ErrorCode = "Error";
					return DMF;
				}
				MDA.Dispose();
				DMF.KeyUsed = MAKER.Key;
				DMF.InitVectorUsed = MAKER.IV;
				return DMF;
			}

			/// <summary>
			/// Decrypts the specified file and puts it's decrypted contents to the file path pointed out.
			/// </summary>
			/// <param name="FilePath">The encrypted file.</param>
			/// <param name="Key">The key that this file uses.</param>
			/// <param name="IV">The Initialisation Vector that this file uses.</param>
			/// <param name="FileOutputPath">The output from the decrypted file.</param>
			public static void DecryptAFile(System.String FilePath, System.Byte[] Key,
			System.Byte[] IV, System.String FileOutputPath = "")
			{
				if (!(System.IO.File.Exists(FilePath))) { return; }
				if ((Key is null) || (IV is null)) { return; }
				if ((Key.Length <= 0) || (IV.Length <= 0)) { return; }
				if (System.String.IsNullOrEmpty(FileOutputPath))
				{
					FileOutputPath = (FilePath).Remove(FilePath.IndexOf(".")) + "_UNENCRYPTED_" + (FilePath).Substring(FilePath.IndexOf("."));
				}
				AESEncryption MDA = new AESEncryption();
				MDA.EncryptionKey = Key;
				MDA.IV = IV;
				try
				{
					using (System.IO.FileStream MDR = new System.IO.FileStream(FilePath, System.IO.FileMode.Open))
					{
						using (System.IO.StreamWriter MNH = new System.IO.StreamWriter(System.IO.File.OpenWrite(FileOutputPath)))
						{
							MNH.WriteLine(MDA.DecryptSpecifiedDataForFiles(MDR));
						}
					}
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return;
				}
				MDA.Dispose();
				return;
			}

			// Executable code examples for encrypting and unencrypting files:
			// Executable code starts here: <--
			// EDAFile.EncryptionContext RDF = EDAFile.EncryptAFile("E:\winrt\base.h" , "E:\IMAGES\winrtbase_h.Encrypted")
			// EDAFile.DecryptAFile("E:\IMAGES\winrtbase_h.Encrypted" , RDF.KeyUsed , RDF.InitVectorUsed , "E:\IMAGES\Unencrypted-4664.h")
			// --> Executable code ended.
			// This is the simpliest way to encrypt and decrypt the files , but you can make use of the original AES API and make the encryption/decryption as you like to.
			// To Access that API , use MDCFR.CryptographicOperations.AESEncryption .
			// More instructions on how to do such security conversions can be found in our Developing Website.
		}

	}

	namespace Archives
	{
		// A Collection Namespace for making and extracting archives.

		/// <summary>
		/// Archive files using the GZIP algorithm.
		/// </summary>
		public static class GZipArchives
		{
			/* A Class that abstracts the GZIP archive format.
			   USAGE NOTE: You can only add one file per archive each time.
			   This style is adopted by the UNIX Systems and can be used to transfer data to an UNIX operating system.
			   You can use this class with three ways:
				1 ->  Supply the file paths.
					   You supply the file paths to the function and it will create or extract the archive respectively.
				2 -> Use GZ UNIX filename format.
						You only supply the filepath and the function creates automagically the Gzip archive filename.
						You can also extract from it with about the same way.
				3 -> Use an already initialised FileStream.
						Using this way , you can customize the file creation options , and how the file should be created or read.
						See http://learn.microsoft.com/en-us/dotnet/api/system.io?view=netframework-4.8 for more details on how exactly the 
						the files can be read or written.
			  NOTES: 
			  1: Using GZIP methods do not expose any exception to be thrown. Any found exception will be thrown to the console.
			  2: These methods are only allowed to return System.Boolean values , which means that only True or False values can be displayed only.
			  3: True determines that the operation was completed sucessfully; while False means that something broke up the execution and the file was not created OR
			  it was created the result , but it is empty.
			  4: Be careful when using the File Paths methods , because if one archive with the same name is already existing , it will be overwritten.
			*/

			private static System.Boolean CheckFilePath(System.String Path)
			{
				if (System.String.IsNullOrEmpty(Path)) { return false; }
				if (!(System.IO.File.Exists(Path))) { return false; }
				return true;
			}

			/// <summary>
			/// Compress the specified file to GZIP format.
			/// </summary>
			/// <param name="FilePath">The file to compress.</param>
			/// <param name="ArchivePath">The output file.</param>
			/// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
			public static System.Boolean CompressTheSelectedFile(System.String FilePath, System.String ArchivePath = null)
			{
				if (!(CheckFilePath(FilePath))) { return false; }
				System.String OutputFile;
				if (System.String.IsNullOrEmpty(ArchivePath))
				{
					System.IO.FileInfo FSIData = new System.IO.FileInfo(FilePath);
					OutputFile = $"{FSIData.DirectoryName}\\{FSIData.Name}.gz";
					FSIData = null;
				}
				else { OutputFile = ArchivePath; }
				System.IO.FileStream FSI;
				System.IO.FileStream FSO;
				try { FSI = System.IO.File.OpenRead(FilePath); }
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				try
				{ FSO = System.IO.File.OpenWrite(ArchivePath); }
				catch (System.Exception EX)
				{
					FSI.Close();
					FSI.Dispose();
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				try
				{
					using (System.IO.Compression.GZipStream CMP = new System.IO.Compression.GZipStream(FSO, System.IO.Compression.CompressionMode.Compress))
					{
						FSI.CopyTo(CMP);
					}
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				finally
				{
					if (FSI != null)
					{
						FSI.Close();
						FSI.Dispose();
					}
					if (FSO != null)
					{
						FSO.Close();
						FSO.Dispose();
					}
				}
				return true;
			}

            /// <summary>
            /// Compress an alive <see cref="System.IO.FileStream"/> that contains the data to 
            /// compress to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="InputFileStream">The input file stream that contains the data to compress.</param>
            /// <param name="OutputFileStream">The compressed data.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressAsFileStreams(System.IO.FileStream InputFileStream, System.IO.FileStream OutputFileStream)
			{
				if (InputFileStream.CanRead == false) { return false; }
				if (OutputFileStream.CanWrite == false) { return false; }
				try
				{
					using (System.IO.Compression.GZipStream CMP = new System.IO.Compression.GZipStream(OutputFileStream, System.IO.Compression.CompressionMode.Compress))
					{
						InputFileStream.CopyTo(CMP);
					}
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

			/// <summary>
			/// Decompress a GZIP archive back to the file.
			/// </summary>
			/// <param name="ArchiveFile">The Archive file path.</param>
			/// <param name="OutputPath">Path to put the decompressed data.</param>
			/// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
			public static System.Boolean DecompressTheSelectedFile(System.String ArchiveFile, System.String OutputPath = null)
			{
				if (!(CheckFilePath(ArchiveFile))) { return false; }
				System.String OutputFile;
				if (System.String.IsNullOrEmpty(OutputPath))
				{
					System.IO.FileInfo ArchInfo = new System.IO.FileInfo(ArchiveFile);
					System.String FinalPath = ArchInfo.DirectoryName;
					System.String TruncatePath = ArchiveFile.Substring(FinalPath.Length + 1);
					System.String FPH = TruncatePath.Remove(TruncatePath.Length - 3);
					OutputFile = FinalPath + @"\" + FPH;
					FPH = null;
					ArchInfo = null;
					FinalPath = null;
					TruncatePath = null;
				}
				else
				{
					OutputFile = OutputPath;
				}
				System.IO.FileStream FSI;
				System.IO.FileStream FSO;
				try
				{
					FSI = System.IO.File.OpenRead(ArchiveFile);
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				try
				{
					FSO = System.IO.File.OpenWrite(OutputFile);
				}
				catch (System.Exception EX)
				{
					FSI.Close();
					FSI.Dispose();
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				try
				{
					using (System.IO.Compression.GZipStream DCMP = new System.IO.Compression.GZipStream(FSI, System.IO.Compression.CompressionMode.Decompress))
					{
						DCMP.CopyTo(FSO);
					}
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				finally
				{
					FSI.Close();
					FSI.Dispose();
					FSO.Close();
					FSO.Dispose();
				}
				return true;
			}

			/// <summary>
			/// Decompress an alive <see cref="System.IO.FileStream"/> and send the decompressed data
			/// to another alive <see cref="System.IO.FileStream"/> object.
			/// </summary>
			/// <param name="ArchiveFileStream">The compressed data.</param>
			/// <param name="DecompressedFileStream">The decompressed data to put to.</param>
			/// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
			public static System.Boolean DecompressAsFileStreams(System.IO.FileStream ArchiveFileStream, System.IO.FileStream DecompressedFileStream)
			{
				if (ArchiveFileStream.CanRead == false) { return false; }
				if (DecompressedFileStream.CanWrite == false) { return false; }
				try
				{
					using (System.IO.Compression.GZipStream DCMP = new System.IO.Compression.GZipStream(ArchiveFileStream, System.IO.Compression.CompressionMode.Decompress))
					{
						DCMP.CopyTo(DecompressedFileStream);
					}
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

		}

        /// <summary>
        /// Archive files using the GZIP algorithm from the ZIP managed library. (Experimental class)
        /// </summary>
        public static class Experimental_GZipArchives
		{

            /// <summary>
            /// Compress the specified file to GZIP format.
            /// </summary>
            /// <param name="FilePath">The file to compress.</param>
            /// <param name="ArchivePath">The output file.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressTheSelectedFile(System.String FilePath, System.String ArchivePath = null)
			{
				if (MAIN.FileExists(FilePath) == false) { return false; }

                System.String OutputFile;
                if (System.String.IsNullOrEmpty(ArchivePath))
                {
                    System.IO.FileInfo FSIData = new System.IO.FileInfo(FilePath);
                    OutputFile = $"{FSIData.DirectoryName}\\{FSIData.Name}.gz";
                    FSIData = null;
                } else { OutputFile = ArchivePath; }
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;
                try { FSI = System.IO.File.OpenRead(FilePath); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try
                {
					ExternalArchivingMethods.SharpZipLib.GZip.Compress(FSI, FSO, false, 1024, 5);
                }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    if (FSI != null)
                    {
                        FSI.Close();
                        FSI.Dispose();
                    }
                    if (FSO != null)
                    {
                        FSO.Close();
                        FSO.Dispose();
                    }
                }
                return true;
            }

            /// <summary>
            /// Compress an alive <see cref="System.IO.FileStream"/> that contains the data to 
            /// compress to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="InputFileStream">The input file stream that contains the data to compress.</param>
            /// <param name="OutputFileStream">The compressed data.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressAsFileStreams(System.IO.FileStream InputFileStream, System.IO.FileStream OutputFileStream)
            {
                if (InputFileStream.CanRead == false) { return false; }
                if (OutputFileStream.CanWrite == false) { return false; }
                try
                {
					ExternalArchivingMethods.SharpZipLib.GZip.Compress(InputFileStream, OutputFileStream, false, 1024, 5);
                }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Decompress a GZIP archive back a decompressed file.
            /// </summary>
            /// <param name="ArchiveFile">The Archive file path.</param>
            /// <param name="OutputPath">Path to put the decompressed data.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressTheSelectedFile(System.String ArchiveFile, System.String OutputPath = null)
            {
                if (MAIN.FileExists(ArchiveFile) == false) { return false; }
                System.String OutputFile;

                if (System.String.IsNullOrEmpty(OutputPath))
                {
                    System.IO.FileInfo ArchInfo = new System.IO.FileInfo(ArchiveFile);
                    System.String TruncatePath = ArchInfo.FullName.Substring(ArchInfo.DirectoryName.Length);
                    System.String FPH = TruncatePath.Remove(TruncatePath.Length - 3);
                    OutputFile = $"{ArchInfo.DirectoryName}\\{FPH}";
                    FPH = null;
                    ArchInfo = null;
                    TruncatePath = null;
                } else { OutputFile = OutputPath; }
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;
				try { FSI = System.IO.File.OpenRead(ArchiveFile); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }

                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.GZip.Decompress(FSI, FSO, false); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    FSI.Close();
                    FSI.Dispose();
                    FSO.Close();
                    FSO.Dispose();
                }
                return true;
            }

            /// <summary>
            /// Decompress an alive <see cref="System.IO.FileStream"/> and send the decompressed data
            /// to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="ArchiveFileStream">The compressed data.</param>
            /// <param name="DecompressedFileStream">The decompressed data to put to.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressAsFileStreams(System.IO.FileStream ArchiveFileStream, System.IO.FileStream DecompressedFileStream)
            {
                if (ArchiveFileStream.CanRead == false) { return false; }
                if (DecompressedFileStream.CanWrite == false) { return false; }
                try { ExternalArchivingMethods.SharpZipLib.GZip.Decompress(ArchiveFileStream, DecompressedFileStream , false); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                return true;
            }

        }

		/// <summary>
		/// Archive files using the BZip2 format (Experimental class).
		/// </summary>
		public static class Experimental_BZip2Archives
		{
            /// <summary>
            /// Compress the specified file to GZIP format.
            /// </summary>
            /// <param name="FilePath">The file to compress.</param>
            /// <param name="ArchivePath">The output file.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressTheSelectedFile(System.String FilePath, System.String ArchivePath = null)
            {
                if (MAIN.FileExists(FilePath) == false) { return false; }

                System.String OutputFile;
                if (System.String.IsNullOrEmpty(ArchivePath))
                {
                    System.IO.FileInfo FSIData = new System.IO.FileInfo(FilePath);
                    OutputFile = $"{FSIData.DirectoryName}\\{FSIData.Name}.bz2";
                    FSIData = null;
                }
                else { OutputFile = ArchivePath; }
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;
                try { FSI = System.IO.File.OpenRead(FilePath); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Compress(FSI, FSO, false, 5); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    if (FSI != null)
                    {
                        FSI.Close();
                        FSI.Dispose();
                    }
                    if (FSO != null)
                    {
                        FSO.Close();
                        FSO.Dispose();
                    }
                }
                return true;
            }

            /// <summary>
            /// Compress an alive <see cref="System.IO.FileStream"/> that contains the data to 
            /// compress to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="InputFileStream">The input file stream that contains the data to compress.</param>
            /// <param name="OutputFileStream">The compressed data.</param>
            /// <returns><c>true</c> if the command succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean CompressAsFileStreams(System.IO.FileStream InputFileStream, System.IO.FileStream OutputFileStream)
            {
                if (InputFileStream.CanRead == false) { return false; }
                if (OutputFileStream.CanWrite == false) { return false; }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Compress(InputFileStream, OutputFileStream, false, 5); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Decompress a GZIP archive back to the file.
            /// </summary>
            /// <param name="ArchiveFile">The Archive file path.</param>
            /// <param name="OutputPath">Path to put the decompressed data.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressTheSelectedFile(System.String ArchiveFile, System.String OutputPath = null)
            {
                if (MAIN.FileExists(ArchiveFile) == false) { return false; }
                System.String OutputFile;
                if (System.String.IsNullOrEmpty(OutputPath))
                {
                    System.IO.FileInfo ArchInfo = new System.IO.FileInfo(ArchiveFile);
                    System.String TruncatePath = ArchInfo.FullName.Substring(ArchInfo.DirectoryName.Length);
                    System.String FPH = TruncatePath.Remove(TruncatePath.Length - 4);
                    OutputFile = $"{ArchInfo.DirectoryName}\\{FPH}";
                    FPH = null;
                    ArchInfo = null;
                    TruncatePath = null;
                }
                else { OutputFile = OutputPath; }
                System.IO.FileStream FSI;
                System.IO.FileStream FSO;

                try { FSI = System.IO.File.OpenRead(ArchiveFile); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }

                try { FSO = System.IO.File.OpenWrite(OutputFile); }
                catch (System.Exception EX)
                {
                    FSI.Close();
                    FSI.Dispose();
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Decompress(FSI, FSO, false); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                finally
                {
                    FSI.Close();
                    FSI.Dispose();
                    FSO.Close();
                    FSO.Dispose();
                }
                return true;
            }

            /// <summary>
            /// Decompress an alive <see cref="System.IO.FileStream"/> and send the decompressed data
            /// to another alive <see cref="System.IO.FileStream"/> object.
            /// </summary>
            /// <param name="ArchiveFileStream">The compressed data.</param>
            /// <param name="DecompressedFileStream">The decompressed data to put to.</param>
            /// <returns><c>true</c> if the decompression succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean DecompressAsFileStreams(System.IO.FileStream ArchiveFileStream, System.IO.FileStream DecompressedFileStream)
            {
                if (ArchiveFileStream.CanRead == false) { return false; }
                if (DecompressedFileStream.CanWrite == false) { return false; }
                try { ExternalArchivingMethods.SharpZipLib.BZip2.Decompress(ArchiveFileStream, DecompressedFileStream, false); }
                catch (System.Exception EX)
                {
                    MAIN.WriteConsoleText(EX.Message);
                    return false;
                }
                return true;
            }

        }

        /// <summary>
        /// Archive files using the well-known ZIP format.
        /// </summary>
        public static class ZipArchives
		{
			/// <summary>
			/// Extract all the contents of a ZIP file to the specified directory path.
			/// </summary>
			/// <param name="PathOfZip">The archive file.</param>
			/// <param name="PathToExtract">The directory to put the extracted data.</param>
			/// <returns><c>true</c> if extraction succeeded; otherwise , <c>false</c>.</returns>
			public static System.Boolean ExtractZipFileToSpecifiedLocation(System.String PathOfZip, System.String PathToExtract)
			{
				if (!(System.IO.File.Exists(PathOfZip))) { return false; }
				if (!(System.IO.Directory.Exists(PathToExtract))) { return false; }
				try
				{
					System.IO.Compression.ZipFile.ExtractToDirectory(PathOfZip, PathToExtract);
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

			/// <summary>
			/// Create a new ZIP archive by capturing data from a specified directory.
			/// </summary>
			/// <param name="PathOfZipToMake">The file path that the archive will be created.</param>
			/// <param name="PathToCollect">The directory path to capture data from.</param>
			/// <returns><c>true</c> if the operation succeeded; otherwise , <c>false</c>.</returns>
			public static System.Boolean MakeZipFromDir(System.String PathOfZipToMake, System.String PathToCollect)
			{
				if (System.String.IsNullOrEmpty(PathOfZipToMake)) { return false; }
				if (!(System.IO.Directory.Exists(PathToCollect))) { return false; }
				try
				{
					System.IO.Compression.ZipFile.CreateFromDirectory(PathToCollect, PathOfZipToMake);
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

            /// <summary>
            /// Create a new ZIP archive stream so as to customize it.
            /// </summary>
            /// <param name="PathofZipToCreate">The file path of the archive to be created , or the existing one , if modified.</param>
            /// <param name="ArchModeSelector">One of the <see cref="ZipArchiveMode"/> enumerations which indicate at which mode the archive should be opened.</param>
            /// <returns>A new <see cref="ZipArchive"/> object if the command was sucessfull ; otherwise , <c>null</c>.</returns>
            [System.Obsolete("Start using the Experimental_ZipArchives class functions instead. If you want a custom implementation , " +
                "use the namespace ExternalArchivingMethods.SharpZipLib .", false)]
            public static System.IO.Compression.ZipArchive InitZipFileStream(System.String PathofZipToCreate, System.IO.Compression.ZipArchiveMode ArchModeSelector)
			{
				if ((!(System.IO.File.Exists(PathofZipToCreate))) && (System.Convert.ToInt32(ArchModeSelector) != 2))
				{
					MAIN.WriteConsoleText("Cannot Call 'InitZipFileStream' method with the argument ArchModeSelector set to " + ArchModeSelector + " and PathOfZipToMake: " + PathofZipToCreate + " resolves to False.");
					return null;
				}
				try
				{
					return System.IO.Compression.ZipFile.Open(PathofZipToCreate, ArchModeSelector);
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return null;
				}
			}

			/// <summary>
			/// Add a new file to the root directory of the ZIP archive.
			/// </summary>
			/// <param name="Path">The path of the file where it is located.</param>
			/// <param name="ArchFileStream">The alive <see cref="ZipArchive"/> stream to write data to.</param>
			/// <param name="CompLevel">The compression level that should be applied to the file.</param>
			/// <returns><c>true</c> if the file was added to the stream; otherwise , <c>false</c>.</returns>
			[System.Obsolete("Start using the Experimental_ZipArchives class functions instead. If you want a custom implementation , " +
                "use the namespace ExternalArchivingMethods.SharpZipLib ." , false)]
			public static System.Boolean AddNewFileEntryToZip(System.String Path, System.IO.Compression.ZipArchive ArchFileStream, System.IO.Compression.CompressionLevel CompLevel)
			{
				if (!(System.IO.File.Exists(Path))) { return false; }
				System.IO.FileInfo RDF = new System.IO.FileInfo(Path);
				try
				{
					ArchFileStream.CreateEntryFromFile(RDF.FullName, RDF.Name, CompLevel);
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

			/// <summary>
			/// Add all the files detected in a <see cref="System.IO.FileSystemInfo"/>[] array to the root of the ZIP archive.
			/// </summary>
			/// <param name="PathofZipToCreate">The file path of the existing archive.</param>
			/// <param name="InfoObject">The <see cref="System.IO.FileSystemInfo"/> array to purge and add the files to the archive.</param>
			/// <param name="ENTCMPL">The compression level to apply while processing the files.</param>
			/// <returns><c>true</c> if all the files were added to the archive.; otherwise , <c>false</c>.</returns>
			public static System.Boolean CreateZipArchiveViaFileSystemInfo(System.String PathofZipToCreate, System.IO.FileSystemInfo[] InfoObject, System.IO.Compression.CompressionLevel ENTCMPL)
			{
				if (!(System.IO.File.Exists(PathofZipToCreate))) { return false; }
				System.IO.FileStream Zipper = null;
				try
				{
					Zipper = new(PathofZipToCreate, System.IO.FileMode.Open);
					using (System.IO.Compression.ZipArchive ArchZip = new(Zipper, System.IO.Compression.ZipArchiveMode.Update))
					{
						foreach (System.IO.FileSystemInfo T in InfoObject) { if (T is System.IO.FileInfo) { ArchZip.CreateEntryFromFile(T.FullName, T.Name, ENTCMPL); } }
					}
				}
				catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				finally
				{
					if (Zipper != null)
					{
						Zipper.Close();
						Zipper.Dispose();
					}
				}
				return true;
			}

		}

		/// <summary>
		/// Zip Files compression level.
		/// </summary>
		public enum ZipCompressionLevel : System.Int32
		{
			/// <summary>
			/// The Compression level is set to zero (almost the files are stored.)
			/// </summary>
			Zero = 0,
			/// <summary>
			/// Low Compression Level.
			/// </summary>
			Low = 2,
			/// <summary>
			/// A medium compression level will be applied. It is the most casual case.
			/// </summary>
			Medium = 5,
			/// <summary>
			/// High compression level sacrifices performance for better compression.
			/// </summary>
			High = 8,
			/// <summary>
			/// The Ultra compression level uses as most as possible the available computer 
			/// resources so as to achieve the best compression ratio as possible.
			/// </summary>
			Ultra = 9
		}

		/// <summary>
		/// Experimental class that abstracts the methods of the ZIP managed library.
		/// </summary>
		public static class Experimental_ZipArchives
		{

            /// <summary>
            /// Extract all the contents of a ZIP file to the specified directory path.
            /// </summary>
            /// <param name="InputArchivePath">The archive file.</param>
            /// <param name="OutputPath">The directory to put the extracted data.</param>
            /// <returns><c>true</c> if extraction succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean ExtractZipFileToSpecifiedLocation
				(System.String InputArchivePath, System.String OutputPath) 
			{ 
				if (MAIN.FileExists(InputArchivePath) == false) { return false; }
				if (MAIN.DirExists(OutputPath) == false) { MAIN.CreateADir(OutputPath); }
				System.IO.FileStream FS = MAIN.ReadAFileUsingFileStream(InputArchivePath);
				System.IO.FileStream DI;
				if (FS == null) { return false; }
				ExternalArchivingMethods.SharpZipLib.ZipFile CT = new(FS);
                foreach (ExternalArchivingMethods.SharpZipLib.ZipEntry un in CT)
				{
					if (un.IsFile || un.IsDOSEntry)
					{
						System.String g = MAIN.ChangeDefinedChar(un.Name , '/' , '\\');
                        System.String jk = null;
						if (g.IndexOf('\\') != -1) 
						{
                            jk = g.Remove(g.LastIndexOf('\\'));
							if (MAIN.DirExists($"{OutputPath}\\{jk}") == false)
							{
                                if (MAIN.CreateADir($"{OutputPath}\\{jk}") == false) { return false; }
                            }
							jk = null;
                        }
						DI = MAIN.CreateANewFile($"{OutputPath}\\{g}");
						g = null;
						if (DI == null) { FS.Close(); FS.Dispose(); return false; }
						using (DI) { CT.GetInputStream(un).CopyTo(DI); }
						DI = null;
					}
                }

                FS.Close();
				FS.Dispose();
				return true;
			}

            /// <summary>
            /// Create a new ZIP archive by capturing data from a specified directory.
            /// </summary>
            /// <param name="PathOfZipToMake">The file path that the archive will be created.</param>
            /// <param name="PathToCollect">The directory path to capture data from.</param>
			/// <param name="CmpLevel">The Compression level to apply. For migration reasons , 
			/// it is optional and it's value is <see cref="ZipCompressionLevel.Medium"/> .</param>
            /// <returns><c>true</c> if the operation succeeded; otherwise , <c>false</c>.</returns>
            public static System.Boolean MakeZipFromDir
                (System.String PathOfZipToMake, 
				System.String PathToCollect  ,
				ZipCompressionLevel CmpLevel = ZipCompressionLevel.Medium)
			{
                // Start up the Make ZIP from Directory procedure.

                // Check if the parameters are correct:
                // PathOfZipToMake is not NULL and
				// PathToCollect is an existing directory.
                if (System.String.IsNullOrEmpty(PathOfZipToMake)) { return false; }
                if (System.IO.Directory.Exists(PathToCollect) == false) { return false; }
				// Open a new FileStream to the desired path.
				// Exit with FALSE if it could not be opened.
				System.IO.FileStream EDI = ROOT.MAIN.CreateANewFile(PathOfZipToMake);
				if (EDI == null) { return false; }
				// This FileStream is a temporary FileStream that will open the files to compress
				// in the ZIP.
				System.IO.FileStream GDX = null;
				// The fundamental ZIP class: The ZIP Stream that is being used for compression.
				ExternalArchivingMethods.SharpZipLib.ZipOutputStream DI = new(EDI);
				// Create a new DirectoryInfo class from the PathToCollect argument.
                System.IO.DirectoryInfo E1 = new(PathToCollect);
				// Set the Base Directory. This directory will be used so as to compare 
				// the files to add against it.
                System.String BaseDir = E1.FullName;
                System.String TempPathConstructor = null;
				// Begin execution Phase. Set the compression level to normal.
                DI.SetLevel((System.Int32) CmpLevel);
				// Internal string array that makes the recursion reality.
                IList<System.String> dirs = new List<System.String>();
                try
				{
					System.Int32 Count = 0;
					System.Int32 Index = -1;

					// First , the files on the directory set in PathToCollect will be added.
                    foreach (System.IO.FileInfo DOI in E1.EnumerateFiles())
					{
						// Create a new entry.
                        DI.PutNextEntry(new ExternalArchivingMethods.SharpZipLib.ZipEntry(DOI.Name));
						// Open the file to read it.
                        GDX = MAIN.ReadAFileUsingFileStream(DOI.FullName);
                        // Exit if the file could not be opened.
                        if (GDX == null)
                        {
                            DI.Close();
                            DI.Dispose();
                            EDI.Close();
                            EDI.Dispose();
                            return false;
                        }
                        // Initialise a new Byte array with it's length to be set as the file's one. 
                        System.Byte[] Array = new System.Byte[GDX.Length];
						// Read the data.
                        GDX.Read(Array, 0, Array.Length);
						// Close the stream.
                        GDX.Close();
                        GDX.Dispose();
						// Write back the data to the ZIP file. These will be automatically compressed.
                        DI.Write(Array, 0, Array.Length);
						// Close entry and invalidate any used variables.
						Array = null;
                        DI.CloseEntry();
                        GDX = null;
                    }

					// The Directory recursion starts from this point.
                    for (; ;)
					{
						//Recursion procedure:
						foreach (System.IO.DirectoryInfo DX in E1.EnumerateDirectories())
						{
                            TempPathConstructor = ExternalArchivingMethods.SharpZipLib.ZipEntry.CleanName(DX.FullName.Substring(BaseDir.Length));
                            // Add any files that the dir contains.
							foreach (System.IO.FileInfo DOI in DX.EnumerateFiles())
							{
								if (TempPathConstructor == "")
								{
									DI.PutNextEntry(new ExternalArchivingMethods.SharpZipLib.ZipEntry(DOI.Name));
								}
								else
								{
									DI.PutNextEntry(new ExternalArchivingMethods.SharpZipLib.ZipEntry($"{TempPathConstructor}/{DOI.Name}"));
								}
								GDX = MAIN.ReadAFileUsingFileStream(DOI.FullName);
                                if (GDX == null)
                                {
                                    DI.Close();
                                    DI.Dispose();
                                    EDI.Close();
                                    EDI.Dispose();
                                    return false;
                                }
                                System.Byte[] Array = new System.Byte[GDX.Length];
								GDX.Read(Array, 0, Array.Length);
								GDX.Close();
								GDX.Dispose();
								DI.Write(Array , 0 , Array.Length);
                                Array = null;
                                DI.CloseEntry();
								GDX = null;
							}
							// Add this directory to the list , so it will be recursed in the next iteration.
							dirs.Add(DX.FullName);
							Count++;
						}

						// No more sub-directories; exit the procedure.
						if (Count <= 0) { break; } else
						{
							// Change the value of the class from the argument given to the next index of the recursion list.
							Index++;
							E1 = new(dirs[Index]);
						}
					}

                } catch (System.ArgumentOutOfRangeException) 
				{ 
					// It is expected. 
					// This exception breaks the FOR statement and threrfore , the recursion.
					// Any other execption it is good to be rethrown.
				} catch (System.Exception EX) { throw EX; }
				
				dirs.Clear();
				BaseDir = null;
                GDX = null;
                dirs = null;
				DI.Close();
				DI.Dispose();
				EDI.Close();
				EDI.Dispose();
				return true;
            }

            /// <summary>
            /// Add all the files detected in a <see cref="System.IO.FileSystemInfo"/>[] array to the root of the ZIP archive.
            /// </summary>
            /// <param name="PathofZipToCreate">The file path of the existing archive.</param>
            /// <param name="InfoObject">The <see cref="System.IO.FileSystemInfo"/> array to purge and add the files to the archive.</param>
            /// <param name="ENTCMPL">The compression level to apply while processing the files.</param>
            /// <returns><c>true</c> if all the files were added to the archive.; otherwise , <c>false</c>.</returns>
            public static System.Boolean CreateZipArchiveViaFileSystemInfo(System.String PathofZipToCreate, System.IO.FileSystemInfo[] InfoObject, ZipCompressionLevel ENTCMPL)
			{
				if (System.String.IsNullOrEmpty(PathofZipToCreate)) { return false; }
                if (InfoObject == null || InfoObject.Length <= 0) { return false; }
				System.IO.FileStream EDI = ROOT.MAIN.CreateANewFile(PathofZipToCreate);
				if (EDI == null) { return false; }
                ExternalArchivingMethods.SharpZipLib.ZipOutputStream DI = new(EDI);
                System.IO.FileStream GDX = null;
                foreach (System.IO.FileSystemInfo DF in InfoObject)
				{
					if (DF is System.IO.FileInfo)
					{
                        // Create a new entry.
                        DI.PutNextEntry(new ExternalArchivingMethods.SharpZipLib.ZipEntry(DF.Name));
                        // Open the file to read it.
                        GDX = MAIN.ReadAFileUsingFileStream(DF.FullName);
                        // Exit if the file could not be opened.
                        if (GDX == null) 
						{
                            DI.Close();
                            DI.Dispose();
                            EDI.Close();
                            EDI.Dispose();
							return false; 
						}
                        // Initialise a new Byte array with it's length to be set as the file's one. 
                        System.Byte[] Array = new System.Byte[GDX.Length];
                        // Read the data.
                        GDX.Read(Array, 0, Array.Length);
                        // Close the stream.
                        GDX.Close();
                        GDX.Dispose();
                        // Write back the data to the ZIP file. These will be automatically compressed.
                        DI.Write(Array, 0, Array.Length);
                        // Close entry and invalidate any used variables.
                        Array = null;
                        DI.CloseEntry();
                        GDX = null;
                    }
				}

				GDX = null;
				DI.Finish();
				DI.Close();
				DI.Dispose();
				EDI.Close();
				EDI.Dispose();
				return true;
            }

        }

		/// <summary>
		/// Compress files and directories using the Microsoft's Cabinet format.
		/// </summary>
		[SupportedOSPlatform("windows")]
		public class Cabinets
		{
			/// <summary>
			/// Compress files of the specified directory and add them to a new Cabinet file.
			/// </summary>
			/// <param name="DirToCapture">The directory to purge and add the files to the archive.</param>
			/// <param name="OutputArchivePath">The archive output file path.</param>
			/// <returns><c>true</c> if archiving succeeded; otherwise , <c>false</c>.</returns>
			public static System.Boolean CompressFromDirectory(System.String DirToCapture, System.String OutputArchivePath)
			{
				if (!System.IO.Directory.Exists(DirToCapture)) { return false; }
				try
				{
					ExternalArchivingMethods.Cabinets.CabInfo CI = new(OutputArchivePath);
					System.IO.FileSystemInfo[] FileArray = MAIN.GetANewFileSystemInfo(DirToCapture);
					if (FileArray == null) { return false; }
					IList<System.String> FLT = new List<System.String>();
					foreach (System.IO.FileSystemInfo FI in FileArray) { if (FI is System.IO.FileInfo) { FLT.Add(FI.Name); } }
					CI.PackFiles(DirToCapture, FLT, FLT);
					CI.Refresh();
				} catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

			/// <summary>
			/// Decompresses all the files located in the archive to the specified directory.
			/// </summary>
			/// <param name="DestDir">The destination directory to unpack the files in.</param>
			/// <param name="ArchiveFile">The archive file path from where the files will be extracted from.</param>
			/// <returns><c>true</c> if decompression succeeded; otherwise , <c>false</c>.</returns>
			public static System.Boolean DecompressFromArchive(System.String DestDir, System.String ArchiveFile)
			{
				if (!System.IO.Directory.Exists(DestDir)) { return false; }
				if (!System.IO.File.Exists(ArchiveFile)) { return false; }
				try
				{
					ExternalArchivingMethods.Cabinets.CabInfo CI = new(ArchiveFile);
					CI.Unpack(DestDir);
				} catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

			/// <summary>
			/// Add a file to an existing archive. The archive must be valid and an existing one.
			/// </summary>
			/// <param name="FilePath">The file which you want to add.</param>
			/// <param name="CabinetFile">The archive file to add the file to.</param>
			/// <returns><c>true</c> if the file was added to the archive; otherwise , <c>false</c>.</returns>
			public static System.Boolean AddAFileToCabinet(System.String FilePath, System.String CabinetFile)
			{
				if (!System.IO.File.Exists(CabinetFile)) { return false; }
				if (!System.IO.File.Exists(FilePath)) { return false; }
				try
				{
					System.IO.FileInfo FI = new System.IO.FileInfo(FilePath);
					IList<System.String> IL = new List<System.String>();
					IL.Add(FI.Name);
					ExternalArchivingMethods.Cabinets.CabInfo CI = new(CabinetFile);
					CI.PackFiles(FI.DirectoryName, IL, IL);
				} catch (System.Exception EX)
				{
					MAIN.WriteConsoleText(EX.Message);
					return false;
				}
				return true;
			}

		}

	}

	/// <summary>
	/// Calculates an estimated time required , for example , the time needed to execute a code excerpt.
	/// </summary>
	public sealed class TimeCaculator : System.IDisposable
	{
		private System.DateTime _TimeEl_;
		private System.Boolean _Init_ = false;

		/// <summary>
		/// Use this method to clear and start counting.
		/// </summary>
		public void Init()
		{
			if (_Init_ == true) { return; }
			_TimeEl_ = System.DateTime.Now;
			_Init_ = true;
			return;
		}

		/// <summary>
		/// Stop the counting and calculate the elapsed time.
		/// </summary>
		/// <returns>The time counted to milliseconds.</returns>
		public System.Int32 CaculateTime()
		{
			if (_Init_ == false) { return -1; }
			try
			{
				_Init_ = false;
				return (System.Int32) System.DateTime.Now.Subtract(_TimeEl_).TotalMilliseconds;
			}
			catch (System.Exception EX)
			{
				MAIN.WriteConsoleText(EX.Message);
				return -1;
			}
		}

		/// <summary>
		/// Use the Dispose method to clear up the values so as to prepare it again to count up.
		/// </summary>
		public void Dispose() { DisposeResources(); }

		private void DisposeResources() { _TimeEl_ = default; _Init_ = default; }
	}

	/*
	 *	This is a simple console progress bar. 
	 * Had you ever wanted to represent to console a progress of an action?
	 * Now you can with this easy-to-use and simple class.
	 * Note that you must create a new thread for this class so as to execute the actions required.
	 * Usage
	 * 
	 * Create a new variable that represents this class with one of the available constructors.
	 * then , make a new thread like this:
	 * -> System.Threading.Thread ClassThread = new Thread(new ThreadStart(ClassInitailizator.Invoke));
	 * which will register the thread. Be noted that the thread must only be invoked by the Invoke() function.
	 * Use then the ClassThread.Start();
	 * to show the progress bar to the user.
	 * Use then each time ClassThread.UpdateProgress(); to update the progress indicator specified by the step given.
	 * This will finished when the value presented is equal or more to the stop barrier(Which of this case is the end)
	 * But this class has many , many other features ,like changing the progress message at executing time ,
	 * breaking the bar before it ends , and setting the min/max values allowed and the step , which is also allowed to be
	 * a negative number.
	 *
	 * I have also here an function example , which defines and controls the message itself. 
	 * ->
	 * using ROOT;
	 * 
	 * public static void Test()
	   {
		   SimpleProgressBar CB = new SimpleProgressBar();
		   System.Threading.Thread FG = new System.Threading.Thread(new System.Threading.ThreadStart(CB.Invoke));
		   CB.ProgressChar = '@';
		   CB.ProgressStep = 1;
		   CB.ProgressEndValue = 100;
		   FG.Start();
		   while (CB.Ended == false)
		   {
			   CB.UpdateProgress();
			   System.Threading.Thread.Sleep(88);
		   }
		   System.Threading.Thread.Sleep(480);
		   System.Console.WriteLine("Ended.");
		   FG = null;
		   CB = null;
	   }
		<-
	*/

	internal class ProgressChangedArgs : System.EventArgs
	{
		private System.Int32 changedto;
		private System.Boolean ch = false;

		public System.Boolean Changed
		{
			get { return ch; }
			set { ch = value; }
		}

		public System.Int32 ChangedValueTo
		{
			get { return changedto; }
			set { changedto = value; }
		}

		public ProgressChangedArgs(System.Int32 ChangedValue)
		{
			Changed = true;
			changedto = ChangedValue;
		}
	}


	/// <summary>
	/// A simple and to-the-point console progress bar class.
	/// </summary>
	public sealed class SimpleProgressBar
	{
		private System.String Progr = "Completed";
		private System.String Progm = "";
		private System.Char Progc = '.';
		private System.Boolean _Ended = false;
		private System.Int32 stp = 1;
		private System.Int32 iterator = 0;
		private System.Int32 start = 0;
		private System.Int32 end = 100;

		private event System.EventHandler<ProgressChangedArgs> ChangeProgress;

		/// <summary>
		/// Change the Progress bar message.
		/// </summary>
		public System.String ProgressMessage
		{
			get { return Progr; }
			set { if (System.String.IsNullOrEmpty(value)) { throw new System.ArgumentException("Illegal , not allowed to be null."); } else { Progr = value; } }
		}

		/// <summary>
		/// Constructor option 1: Define the arguments via the properties and run the bar when you need it.
		/// </summary>
		public SimpleProgressBar() { }

		/// <summary>
		/// Constructor option 2: Define the start , stop and end values.
		/// </summary>
		/// <param name="Start">From which number the bar should start counting.</param>
		/// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
		/// <param name="End">At what number the bar will be stopped.</param>
		/// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
		public SimpleProgressBar(System.Int32 Start, System.Int32 Step, System.Int32 End)
		{
			if (End > 300) { throw new System.ArgumentException("It is not allowed the End value to be more than 300."); }
			if (Start >= End) { throw new System.ArgumentException("It is not allowed the Start value to be more than the ending value."); }
			start = Start;
			stp = Step;
			end = End;
		}

		/// <summary>
		/// Constructor option 3: Define the initial progress message , start , stop and end values.
		/// </summary>
		/// <param name="progressMessage">The Initial progress message prompt.</param>
		/// <param name="Start">From which number the bar should start counting.</param>
		/// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
		/// <param name="End">At what number the bar will be stopped.</param>
		/// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
		public SimpleProgressBar(System.String progressMessage, System.Int32 Start, System.Int32 Step, System.Int32 End)
		{
			if (End > 300) { throw new System.ArgumentException("It is not allowed the End value to be more than 300."); }
			if (Start >= End) { throw new System.ArgumentException("It is not allowed the Start value to be more than the ending value."); }
			if (System.String.IsNullOrEmpty(progressMessage)) { throw new System.ArgumentException("The progressMessage is null."); }
			Progr = progressMessage; start = Start; stp = Step; end = End;
		}

		/// <summary>
		/// Constructor option 4: Define only the Step and End values.
		/// </summary>
		/// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
		/// <param name="End">At what number the bar will be stopped.</param>
		/// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
		public SimpleProgressBar(System.Int32 Step, System.Int32 End) { if (End > 300) { throw new System.ArgumentException("It is not allowed the End value to be more than 300."); } else { stp = Step; end = End; } }

		/// <summary>
		/// Constructor option 5: Define only the progress message , Step and End values.
		/// </summary>
		/// <param name="progressMessage">The Initial progress message prompt.</param>
		/// <param name="Step">The step to use for the bar. Can be also a negative <see cref="System.Int32"/>.</param>
		/// <param name="End">At what number the bar will be stopped.</param>
		/// <exception cref="System.ArgumentException">This is thrown in two cases: 1: The End value is bigger than 300 and the Start value is bigger that the End one.</exception>
		public SimpleProgressBar(System.String progressMessage, System.Int32 Step, System.Int32 End)
		{
			if (End > 300) { throw new System.ArgumentException("It is not allowed this value to be more than 300."); }
			if (System.String.IsNullOrEmpty(progressMessage)) { throw new System.ArgumentException("The progressMessage is null."); }
			Progr = progressMessage; stp = Step; end = End;
		}


		/// <summary>
		/// The Progress character that will be used inside the bar ([...])
		/// </summary>
		public System.Char ProgressChar
		{
			get { return Progc; }
			set
			{
				System.Char[] InvalidChars = new System.Char[] { '\a', '\b', '\\', '\'', '\"', '\r', '\n', '\0' , '\f' };
				for (System.Int32 D = 0; D < InvalidChars.Length; D++)
				{
					if (InvalidChars[D] == value) { throw new System.ArgumentException("The character is illegal."); }
				}
				Progc = value;
			}
		}

		/// <summary>
		/// This defines the step to use when the bar number will be changed. Can be also a negative <see cref="System.Int32"/> .
		/// </summary>
		public System.Int32 ProgressStep { get { return stp; } set { stp = value; } }

		/// <summary>
		/// This defines the value that the progress bar will end to.
		/// </summary>
		public System.Int32 ProgressEndValue
		{
			get { return end; }
			set { if (value > 300) { throw new System.ArgumentException("It is not allowed this value to be more than 300."); } else { end = value; } }
		}

		/// <summary>
		/// From which number the bar will start counting.
		/// </summary>
		public System.Int32 ProgressStartValue
		{
			get { return start; }
			set { if (value >= end) { throw new System.ArgumentException("It is not allowed this value to be more than the ending value."); } }
		}

		/// <summary>
		/// This updates the message bar string while being executed.
		/// </summary>
		/// <param name="Message">The message to replace the current one.</param>
		/// <exception cref="System.ArgumentException">When the <paramref name="Message"/> is <code>null</code>.</exception>
		public void UpdateMessageString(System.String Message)
		{
			if (System.String.IsNullOrEmpty(Message)) { throw new System.ArgumentException("The Message is null."); }
			Progr = Message;
            ConsoleInterop.WriteToConsole($"{Progr}: {iterator}/{end} [{Progm}]\r");
		}

		/// <summary>
		/// Indicates if the bar was executed. Use it to close the running thread.
		/// </summary>
		public System.Boolean Ended { get { return _Ended; } set { _Ended = value; } }

		/// <summary>
		/// Update the progress by the defined step.
		/// </summary>
		public void UpdateProgress() { if (_Ended == false) { Progm += Progc; ChangeProgress.Invoke(null, new ProgressChangedArgs(iterator += stp)); } }

		private void ChangeBar(System.Object sender, ProgressChangedArgs e) { ConsoleInterop.WriteToConsole($"{Progr}: {e.ChangedValueTo}/{end} [{Progm}] \r"); }

		/// <summary>
		/// The function which starts up the Console Bar. This should only be used in a new <see cref="System.Threading.ThreadStart"/> delegate.
		/// </summary>
		public void Invoke()
		{
			ChangeProgress += ChangeBar;
			iterator = start;
			ConsoleInterop.WriteToConsole($"{Progr}: {iterator}/{end} [{Progm}]\r");
			do
			{
				if (iterator >= end) { _Ended = true; }
				System.Threading.Thread.Sleep(80);
			} while (_Ended == false);
			ChangeProgress -= ChangeBar;
			ChangeProgress = null;
			ROOT.MAIN.WriteConsoleText("\nCompleted.");
			return;
		}
	}

	namespace IntuitiveInteraction
	{
#pragma warning disable CS1591
		/// <summary>
		/// An enumeration of <see cref="System.Int32" /> that hold valid icon images allowed be shown when the class 
		/// <see cref="IntuitiveMessageBox"/> is invoked.
		/// </summary>
		public enum IconSelection : System.Int32
		{
			None = 0,
			Error = 1,
			Info = 2,
			Info2 = 3,
			Warning = 4,
			Notice = 5,
			InvalidOperation = 6,
			Question = 7
		}

		/// <summary>
		/// An enumeration of <see cref="System.Int32" /> that keeps valid button patterns for returning the button selected.
		/// </summary>
		/// <remarks>This is used only with the class <see cref="IntuitiveMessageBox"/>.</remarks>
		public enum ButtonSelection : System.Int32
		{
			OK = 0,
			YesNo = 1,
			OKCancel = 2,
			AbortRetry = 3,
			RetryCancel = 4,
			IgnoreCancel = 5,
			YesNoCancel = 6,
			YesNoRetry = 7,
			YesCancelAbort = 8
		}

		/// <summary>
		/// An enumeration of <see cref="System.Int32" /> values which indicates which button pressed or presents an error.
		/// </summary>
		public enum ButtonReturned : System.Int32
		{
			Error = 0,
			OK = 1,
			Cancel = 2,
			Yes = 3,
			No = 4,
			Retry = 5,
			Abort = 6,
			Ignore = 7,
			NotAnAnswer = Error | Cancel
		}
#pragma warning restore CS1591

		/// <summary>
		/// A class that extends the default <see cref="Microsoft.VisualBasic.Interaction.InputBox"/> method.
		/// </summary>
		[SupportedOSPlatform("windows")]
		public class GetAStringFromTheUser : System.IDisposable
		{
			private Form Menu = new();
			private Button Button1 = new Button();
			private Button Button2 = new Button();
			private Label Label1 = new Label();
			private TextBox TextBox1 = new TextBox();
			private System.String Prompt_msg;
			private System.String Title_msg;
			private System.String Default_msg;
			private ButtonReturned _RET = 0;
			private System.String Value;

			private event System.EventHandler HANDLE;

			/// <summary>
			/// Constructor Option 1: Define the settings at any time you would like.
			/// </summary>
			/// <remarks>Do not forget to invoke the dialog using the <see cref="Invoke"/> function.</remarks>
			public GetAStringFromTheUser() { }

            /// <summary>
            /// Constructor Option 2: Define the arguments required at once , run the dialog and then dispose it.
            /// </summary>
            /// <param name="Prompt">A message prompting the User what he should type inside the input box.</param>
            /// <param name="Title">The dialog's title.</param>
            /// <param name="DefaultResponse">The default response or an example of the data to be provided by the User.</param>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public GetAStringFromTheUser(System.String Prompt, System.String Title, System.String DefaultResponse)
			{
				Prompt_msg = Prompt;
				Title_msg = Title;
				Default_msg = DefaultResponse;
				HANDLE += Button_click;
				Initiate();
				HANDLE -= Button_click;
				this.Dispose();
			}

			/// <summary>
			/// Disposes all the <see cref="System.Windows.Forms.Form"/> members used to make this dialog.
			/// </summary>
			public void Dispose()
			{
				if (HANDLE != null) { HANDLE -= Button_click; HANDLE = null; }
				TextBox1.Dispose();
				Label1.Dispose();
				Button1.Dispose();
				Button2.Dispose();
				Menu.Dispose();
			}

			/// <summary>
			/// A message prompting the User what he should type inside the input box.
			/// </summary>
			public System.String Prompt
			{
				set { Prompt_msg = value; }
			}

			/// <summary>
			/// The window's title.
			/// </summary>
			public System.String Title
			{
				set { Title_msg = value; }
			}

            /// <summary>
            /// Invokes the User Input Box. Use it when you have used the parameterless constructor.
            /// </summary>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public void Invoke()
			{
				HANDLE += Button_click;
				Initiate();
				HANDLE -= Button_click;
				this.Dispose();
			}

			/// <summary>
			/// The default response or an example of the data to be provided by the User.
			/// </summary>
			public System.String DefaultResponse
			{
				set { Default_msg = value; }
			}

			/// <summary>
			/// Returns the button pressed. 
			///  -> 0 indicates an system error or the user used the 'X' (Close Window) button.
			///  -> 2 indicates that the User supplied an option and then he pressed the 'OK' button.
			///  -> 4 indicates that the User did or not gave an answer , but he canceled the action.
			/// </summary>
			public ButtonReturned ButtonClicked
			{
				get { return _RET; }
			}

			/// <summary>
			/// Returns a <see cref="System.Boolean" /> value indicating that the User has supplied a value and pressed the 'OK' button.
			/// </summary>
			public System.Boolean Success
			{
				get { if (_RET == ButtonReturned.NotAnAnswer) { return false; } else { return true; } }
			}

			/// <summary>
			/// Returns the value given by the User. It's type is a <see cref="System.String"/>.
			/// </summary>
			public System.String ValueReturned
			{
				get { if (_RET != ButtonReturned.Error) { return Value; } else { return null; } }
			}

			private protected void Button_click(System.Object sender, System.EventArgs e)
			{
				Menu.Close();
				if (sender == Button1)
				{
					Value = TextBox1.Text;
					_RET = ButtonReturned.OK;
				}
				if (sender == Button2)
				{
					_RET = ButtonReturned.Cancel;
				}
				return;
			}

            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            private protected void Initiate()
			{
				Label1.SuspendLayout();
				Button1.SuspendLayout();
				Button2.SuspendLayout();
				TextBox1.SuspendLayout();
				Label1.BorderStyle = BorderStyle.None;
				Label1.UseMnemonic = true;
				Label1.AutoSize = true;
				Label1.Text = Prompt_msg;
				Label1.Location = new System.Drawing.Point(14, 25);
				Label1.Size = new System.Drawing.Size(180, 110);
				Button1.Location = new Point(260, 22);
				Button1.Size = new Size(65, 24);
				Button1.Text = "OK";
				Button2.Location = new Point(Button1.Location.X, Button1.Location.Y + Button1.Height + 9);
				Button2.Size = Button1.Size;
				Button2.Text = "Cancel";
				Button1.Click += HANDLE;
				Button2.Click += HANDLE;
				System.Char[] FindNL_S = Label1.Text.ToCharArray();
				// The last value for vertical padding.
				System.Int32 CH = 0;
				for (System.Int32 DI = 0; DI < FindNL_S.Length; DI++)
				{
					// The required padding for Microsoft Sans Serif is now 16?.
					if (FindNL_S[DI] == '\n') { CH += 16; }
				}
				FindNL_S = null;
				TextBox1.Location = new Point(11, (Label1.Height + CH) - 17);
				TextBox1.Size = new Size(330, 14);
				TextBox1.Text = Default_msg;
				TextBox1.BorderStyle = BorderStyle.Fixed3D;
				TextBox1.ReadOnly = false;
				TextBox1.BackColor = Color.LightGray;
				TextBox1.Multiline = false;
				TextBox1.ResumeLayout();
				TextBox1.Invalidate();
				Label1.ResumeLayout();
				Button1.ResumeLayout();
				Button2.ResumeLayout();
				Menu.FormBorderStyle = FormBorderStyle.FixedDialog;
				Menu.StartPosition = FormStartPosition.CenterScreen;
				Menu.Text = Title_msg;
				Menu.MinimizeBox = false;
				Menu.MaximizeBox = false;
				Menu.TopMost = true;
                Menu.ControlBox = false;
                Menu.ShowInTaskbar = false;
				// All the redrawings are only valid for Microsoft Sans Serif font!!!
				Menu.Font = new Font("Microsoft Sans Serif", (System.Single)9.10, FontStyle.Regular, GraphicsUnit.Point);
				Menu.Size = new System.Drawing.Size(TextBox1.Location.X + TextBox1.Size.Width + 28, TextBox1.Location.Y + TextBox1.Size.Height + 42);
				Menu.Controls.Add(TextBox1);
				Menu.Controls.Add(Label1);
				Menu.Controls.Add(Button1);
				Menu.Controls.Add(Button2);
				Menu.ShowDialog();
			}
		}
		/// <summary>
		/// A class that extends the <see cref="System.Windows.Forms.MessageBox"/> class by adding it new features.
		/// </summary>
		/// <remarks>Do not expect that it will be as fast as the <see cref="MessageBox"/>; This is made on managed code. </remarks>
		[SupportedOSPlatform("windows")]
		public class IntuitiveMessageBox : System.IDisposable
		{
			private System.String _MSG;
			private System.Windows.Forms.Label Label1 = new();
			private System.Windows.Forms.Panel Panel1 = new();
			private System.Drawing.Image Image1 = null;
			private System.Windows.Forms.PictureBox PictureBox1 = new();
			private Form Menu = new Form();
			private System.Windows.Forms.Button Button1 = new();
			private System.Windows.Forms.Button Button2 = new();
			private System.Windows.Forms.Button Button3 = new();
			private System.String _TITLE;
			private ButtonReturned BTR;
			private ButtonSelection BSL;
			private IconSelection SELI;

			private event System.EventHandler ButtonHandle;

            /// <summary>
            /// Constructor Option 1:  Define all the arguments at once , run the dialog and dispose the class.
            /// </summary>
            /// <param name="Message">The text of the information to show to the user.</param>
            /// <param name="Title">The message box title to show.</param>
            /// <param name="Buttons">The buttons that will be shown to the User.</param>
            /// <param name="Ic">The Icon that will be shown to the user. Can also be <see cref="IconSelection.None"/>.</param>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public IntuitiveMessageBox(System.String Message, System.String Title, ButtonSelection Buttons, IconSelection Ic)
			{
				ButtonHandle += Button_Click;
				_MSG = Message;
				_TITLE = Title;
				BSL = Buttons;
				SELI = Ic;
				MakeAndInitDialog(Buttons, Ic);
				this.Dispose();
				ButtonHandle -= Button_Click;
			}

			/// <summary>
			/// Constructor Option 2: Define the properties instead and run the dialog whenever you want.
			/// </summary>
			/// <remarks>Do not forget to invoke the window using the <see cref="InvokeInstance"/> function.</remarks>
			public IntuitiveMessageBox() { ButtonHandle += Button_Click; }

			/// <summary>
			/// Constructor Option 3: Define the Message and the Title properties , do any other settings you want 
			/// and run the dialog whenever you want.
			/// </summary>
			/// <param name="Message">The text of the information to show to the user.</param>
			/// <param name="Title">The message box title to show.</param>
			/// <remarks>Do not forget to invoke the window using the <see cref="InvokeInstance"/> function.</remarks>
			public IntuitiveMessageBox(System.String Message, System.String Title)
			{
				ButtonHandle += Button_Click;
				_MSG = Message;
				_TITLE = Title;
			}

            /// <summary>
            /// The Dispose function clears up the resources used by the Message Box and then invalidates the class.
            /// </summary>
            /// <remarks>This function also implements the <see cref="System.IDisposable"/> interface.</remarks>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
			{
				if (ButtonHandle != null) { ButtonHandle -= Button_Click; }
				Label1.Dispose();
				Panel1.Dispose();
				if (Image1 != null) { Image1.Dispose(); }
				PictureBox1.Dispose();
				Button1.Dispose();
				Button2.Dispose();
				Button3.Dispose();
				Menu.Dispose();
				System.GC.Collect(3, System.GCCollectionMode.Forced, true);
			}

			/// <summary>
			/// The text of the information to show to the user.
			/// </summary>
			public System.String Message
			{
				get { return _MSG; }
				set { _MSG = value; }
			}

			/// <summary>
			/// The buttons that will be shown to the User.
			/// </summary>
			public ButtonSelection ButtonsToShow { get { return BSL; } set { BSL = value; } }

			/// <summary>
			/// The Icon that will be shown to the user. Can also be <see cref="IconSelection.None"/>.
			/// </summary>
			public IconSelection IconToShow { get { return SELI; } set { SELI = value; } }

			/// <summary>
			/// The message box title to show.
			/// </summary>
			public System.String Title { get { return _TITLE; } set { _TITLE = value; } }

			/// <summary>
			/// Returns the button selected by the User. It's value is one of the <see cref="ButtonReturned"/> <see cref="System.Enum"/> values.
			/// </summary>
			public ButtonReturned ButtonSelected { get { return BTR; } }

			private protected void Button_Click(System.Object sender, System.EventArgs e)
			{
				Menu.Close();
				if (BSL == 0)
				{
					if (sender == Button1) { BTR = (ButtonReturned)1; }
				}
				if (BSL == (ButtonSelection)1)
				{
					if (sender == Button1) { BTR = (ButtonReturned)4; }
					if (sender == Button2) { BTR = (ButtonReturned)3; }
				}
				if (BSL == (ButtonSelection)2)
				{
					if (sender == Button1) { BTR = (ButtonReturned)2; }
					if (sender == Button2) { BTR = (ButtonReturned)1; }
				}
				if (BSL == (ButtonSelection)3)
				{
					if (sender == Button1) { BTR = (ButtonReturned)5; }
					if (sender == Button2) { BTR = (ButtonReturned)6; }
				}
				if (BSL == (ButtonSelection)4)
				{
					if (sender == Button1) { BTR = (ButtonReturned)5; }
					if (sender == Button2) { BTR = (ButtonReturned)2; }
				}
				if (BSL == (ButtonSelection)5)
				{
					if (sender == Button1) { BTR = (ButtonReturned)7; }
					if (sender == Button2) { BTR = (ButtonReturned)2; }
				}
				if (BSL == (ButtonSelection)6)
				{
					if (sender == Button1) { BTR = (ButtonReturned)2; }
					if (sender == Button2) { BTR = (ButtonReturned)4; }
					if (sender == Button3) { BTR = (ButtonReturned)3; }
				}
				if (BSL == (ButtonSelection)7)
				{
					if (sender == Button1) { BTR = (ButtonReturned)5; }
					if (sender == Button2) { BTR = (ButtonReturned)4; }
					if (sender == Button3) { BTR = (ButtonReturned)3; }
				}
				if (BSL == (ButtonSelection)8)
				{
					if (sender == Button1) { BTR = (ButtonReturned)6; }
					if (sender == Button2) { BTR = (ButtonReturned)2; }
					if (sender == Button3) { BTR = (ButtonReturned)3; }
				}
				return;
			}

            /// <summary>
            /// Invokes the Message Box based on the current settings done by the User.
            /// </summary>
            /// <remarks>Do not forget to dispose the class by using the <see cref="Dispose"/> function.</remarks>
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public void InvokeInstance()
			{
				MakeAndInitDialog(BSL, SELI);
				ButtonHandle -= Button_Click;
				this.Dispose();
			}

            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            private protected void MakeAndInitDialog(ButtonSelection Butt, IconSelection Icon)
			{
				// The below statements select the appropriate image to be shown each time.
				// Although that these are icons , 
				if (Icon == (IconSelection)1) { Image1 = MDCFR.Properties.Resources.Error.ToBitmap(); }
				if (Icon == (IconSelection)2) { Image1 = MDCFR.Properties.Resources.Information.ToBitmap(); }
				if (Icon == (IconSelection)3) { Image1 = MDCFR.Properties.Resources.Information2.ToBitmap(); }
				if (Icon == (IconSelection)4) { Image1 = MDCFR.Properties.Resources.Warning.ToBitmap(); }
				if (Icon == (IconSelection)5) { Image1 = MDCFR.Properties.Resources.Information.ToBitmap(); }
				if (Icon == (IconSelection)6) { Image1 = MDCFR.Properties.Resources.InvalidOperation.ToBitmap(); }
				if (Icon == (IconSelection)7) { Image1 = MDCFR.Properties.Resources.Question.ToBitmap(); }
				if (Icon != 0)
				{
					PictureBox1.SuspendLayout();
					PictureBox1.AutoSize = true;
					PictureBox1.Size = new System.Drawing.Size(38, 38);
					PictureBox1.Image = Image1;
					PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
					PictureBox1.Location = new System.Drawing.Point(26, 22);
					PictureBox1.ResumeLayout();
				}
				PictureBox1.PerformLayout();
				Label1.SuspendLayout();
				Label1.BorderStyle = BorderStyle.None;
				Label1.UseMnemonic = true;
				Label1.AutoSize = true;
				Label1.Text = _MSG;
				Label1.AutoEllipsis = false;
				if (Icon != 0)
				{
					Label1.Location = new System.Drawing.Point((PictureBox1.Width + PictureBox1.Location.X) + 25, PictureBox1.Location.Y);
				}
				else { Label1.Location = new System.Drawing.Point(26, 22); }
				// By default , the Size value is not updated when the Label is even resized , as set by AutoSize.
				// To fix that , we will get the label's text , we will convert it to an array of System.Char , and then
				// will check out if there are any newline characters. If there are , the location of the last label and buttons will be padded up
				// accordingly. Additionally , neither the Width value is updated.
				// To Implement that , it will use a check mechanism while checking for paddings and will enable it to also
				//  resize that value too. <--
				System.Char[] FindNL_S = Label1.Text.ToCharArray();
				// The last value for vertical padding.
				System.Int32 CH = 0;
				// The Last value for horizontal padding.
				System.Int32 CW = 0;
				// Check flag for horizontal padding.
				System.Boolean CWC = false;
				// A temporary value that will compare if the temporary one is larger than the other.
				System.Int32 CWCM = 0;
				for (System.Int32 DI = 0; DI < FindNL_S.Length; DI++)
				{
					// The required padding for Microsoft Sans Serif is 15.
					if (FindNL_S[DI] == '\n') { CH += 15; CWC = true; } else { CWCM += 5; }
					if (CWC) { if (CWCM > CW) { CW = CWCM; CWCM = 0; CWC = false; } }
					// Recheck once more time in the last iteration if the box must be redrawn.
					if (DI == FindNL_S.Length) { if (CWCM > CW) { CW = CWCM; } }
				}
				// Test if we don't have any '\n' s.
				System.Boolean CWH = true;
				for (System.Int32 DI = 0; DI < FindNL_S.Length; DI++) { if (FindNL_S[DI] == '\n') { CWH = false; } }
				// This special case is executed when the above statement does not have detected any \n s.
				// The numbers are really approximate; these were random math until the wanted situation was performed.
				if (CWH) { CH = 14; CW = (FindNL_S.Length + 10) * 4; }
				CWH = false;
				CWCM = 0;
				FindNL_S = null;
				// -->
				Label1.ResumeLayout();
				Label1.Refresh();
				Panel1.SuspendLayout();
				Panel1.BorderStyle = BorderStyle.None;
				Panel1.AutoSize = true;
				Panel1.BackColor = System.Drawing.Color.Gray;
				Panel1.Location = new System.Drawing.Point(0, Label1.Location.Y + CH + 40);
				CH = 0;
				Panel1.ResumeLayout();
				Button1.SuspendLayout();
				Button2.SuspendLayout();
				Button3.SuspendLayout();
				Button1.Location = new System.Drawing.Point(Label1.Location.X + CW - 18, Panel1.Location.Y + 10);
				Button1.Size = new System.Drawing.Size(65, 20);
				Button2.Location = new System.Drawing.Point(Button1.Left - 75, Button1.Top);
				Button2.Size = Button1.Size;
				Button3.Location = new System.Drawing.Point(Button2.Left - 75, Button1.Top);
				Button3.Size = Button1.Size;
				Panel1.Size = new System.Drawing.Size(PictureBox1.Size.Width + Label1.Location.X + CW + 35, 60);
				// Selection workflow; These statements select which buttons are shown , and determine the dialog.
				if (Butt == 0)
				{
					Button1.Text = "OK";
					Button1.Visible = true;
					Button2.Visible = false;
					Button3.Visible = false;
					Menu.Controls.Add(Button1);
					Button1.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)1)
				{
					Button1.Text = "No";
					Button2.Text = "Yes";
					Button3.Visible = false;
					Menu.Controls.Add(Button2);
					Menu.Controls.Add(Button1);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)2)
				{
					Button1.Text = "Cancel";
					Button2.Text = "OK";
					Button3.Visible = false;
					Menu.Controls.Add(Button1);
					Menu.Controls.Add(Button2);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)3)
				{
					Button1.Text = "Retry";
					Button2.Text = "Abort";
					Button3.Visible = false;
					Menu.Controls.Add(Button1);
					Menu.Controls.Add(Button2);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)4)
				{
					Button1.Text = "Retry";
					Button2.Text = "Cancel";
					Button3.Visible = false;
					Menu.Controls.Add(Button1);
					Menu.Controls.Add(Button2);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)5)
				{
					Button1.Text = "Ignore";
					Button2.Text = "Cancel";
					Button3.Visible = false;
					Menu.Controls.Add(Button1);
					Menu.Controls.Add(Button2);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)6)
				{
					Button1.Text = "Cancel";
					Button2.Text = "No";
					Button3.Text = "Yes";
					Menu.Controls.Add(Button3);
					Menu.Controls.Add(Button2);
					Menu.Controls.Add(Button1);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
					Button3.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)7)
				{
					Button1.Text = "Retry";
					Button2.Text = "No";
					Button3.Text = "Yes";
					Menu.Controls.Add(Button3);
					Menu.Controls.Add(Button2);
					Menu.Controls.Add(Button1);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
					Button3.Click += ButtonHandle;
				}
				if (Butt == (ButtonSelection)8)
				{
					Button1.Text = "Abort";
					Button2.Text = "Cancel";
					Button3.Text = "Yes";
					Menu.Controls.Add(Button3);
					Menu.Controls.Add(Button2);
					Menu.Controls.Add(Button1);
					Button1.Click += ButtonHandle;
					Button2.Click += ButtonHandle;
					Button3.Click += ButtonHandle;
				}
				Button1.ResumeLayout();
				Button2.ResumeLayout();
				Button3.ResumeLayout();
				Menu.FormBorderStyle = FormBorderStyle.FixedDialog;
				Menu.StartPosition = FormStartPosition.CenterScreen;
				Menu.Text = _TITLE;
				Menu.MinimizeBox = false;
				Menu.MaximizeBox = false;
				Menu.TopMost = true;
				Menu.ControlBox = false;
				Menu.ShowInTaskbar = false;
				// All the redrawings are only valid for Microsoft Sans Serif font!!!
				Menu.Font = new Font("Microsoft Sans Serif", (System.Single)8.25, FontStyle.Regular, GraphicsUnit.Point);
				Menu.Size = new System.Drawing.Size(Label1.Location.X + CW + 85, Panel1.Location.Y + Panel1.Height + 20);
				Menu.Controls.Add(Label1);
				Menu.Controls.Add(Panel1);
				if (Icon != 0) { Menu.Controls.Add(PictureBox1); }
				if (Icon == (IconSelection)1) { System.Media.SystemSounds.Hand.Play(); }
				if (Icon == (IconSelection)4) { System.Media.SystemSounds.Asterisk.Play(); }
				if (Icon == (IconSelection)5) { System.Media.SystemSounds.Question.Play(); }
				if (Icon == (IconSelection)6) { System.Media.SystemSounds.Hand.Play(); }
				if (Icon == (IconSelection)7) { System.Media.SystemSounds.Question.Play(); }
				Menu.ShowDialog();
			}

		}
	}

	namespace RandomNumbers
	{

       /* 
         * Written in 2018 by David Blackman and Sebastiano Vigna (vigna@acm.org)
         *
		 * To the extent possible under law, the author has dedicated all copyright
		 * and related and neighboring rights to this software to the public domain
		 * worldwide. This software is distributed without any warranty.
		 *
		 *	See <http://creativecommons.org/publicdomain/zero/1.0/>. 
		 */

        /// <summary>
        /// <see cref="IRandomBase"/> is an interface which allows the random number generators
        /// to easily implement the random number generation mechanism.
        /// </summary>
        public interface IRandomBase : System.IDisposable
		{
			/// <summary>
			/// Inherited from the <see cref="System.IDisposable"/> interface.
			/// The random instance must dispose the seed and the state instances.
			/// </summary>
			public new abstract void Dispose();

			/// <summary>
			/// Produces a new random number. Expressed as <see cref="System.UInt64"/> code points.
			/// </summary>
			/// <returns>A new random number.</returns>
			public abstract System.UInt64 Next();

			/// <summary>
			/// Gets or sets the Seed for the instance. Can be private for that class
			/// and just implement a constructor which would allow to set an seed.
			/// </summary>
			public abstract System.Int32 Seed { get; set; }
			
			/// <summary>
			/// Gets a value whether the algorithm is optimized for 32-Bit machines.
			/// If <see langword="false"/> , it indicates that the random 
			/// algorithm is optimized for 64-Bit machines only.
			/// </summary>
			public abstract System.Boolean Is32Bit { get; }
		}

        /// <summary>
        /// <para>
        /// This is xoroshiro128++ 1.0, one of our all-purpose, rock-solid,
        /// small-state generators. It is extremely (sub-ns) fast and it passes all
        /// tests we are aware of, but its state space is large enough only for
        /// mild parallelism.
        /// </para>
        /// <para>
        /// For generating just floating-point numbers, xoroshiro128+ is even
        /// faster(but it has a very mild bias, see notes in the comments).
		/// </para>
        /// </summary>
        public sealed class Xoroshiro128PP : IRandomBase
		{
			private System.UInt64[] _STATE = new System.UInt64[2];
			private static System.UInt64[] JUMP = { 0x2bd7a6a6e99c2ddc, 0x0992ccaf6a6fca05 };
			private static System.UInt64[] LONG_JUMP = { 0x360fd5f2cf8d5d99, 0x9c6e6877736c46e3 };
			private System.UInt64 _SEED = 0;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            private System.UInt64 rotl(System.UInt64 x, int k) { return (x << k) | (x >> (64 - k)); }

			/// <inheritdoc />
			public System.Boolean Is32Bit { get { return true; } }

			/// <summary>
			/// Create a new instance of <see cref="Xoroshiro128PP"/> with a default seed value of 0.
			/// </summary>
			public Xoroshiro128PP() { _STATE[0] = 2; _STATE[1] = 0; _STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro128PP"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro128PP(System.Int32 Seed)
			{
				_SEED = (System.UInt64) Seed;
				if ((Seed / 2) != 0) { _STATE[0] = (System.UInt64) Seed / 2; } else { _STATE[0] = _SEED * 2; }
				_STATE[1] = (_STATE[0] + (System.UInt64) Seed) ^ 2;
			}

			/// <summary>
			/// Sets or gets the SEED number for this instance.
			/// </summary>
			public System.Int32 Seed 
			{
				get { return (System.Int32) _SEED; }
				set
				{
					_SEED = (System.UInt64) value;
                    if ((_SEED / 2) != 0) { _STATE[0] = _SEED / 2; } else { _STATE[0] = _SEED * 2; }
                    _STATE[1] = (_STATE[0] + _SEED) ^ 2;
                }
			}

			/// <summary>
			/// Disposes the <see cref="Xoroshiro128PP"/> instance.
			/// </summary>
			public void Dispose()
			{
				_STATE = null;
				JUMP = null;
				LONG_JUMP = null;
			}

            /// <summary>
            /// Produce a new <see cref="Xoroshiro128PP"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro128PP"/> random number.</returns>
            public System.UInt64 Next()
            {
                System.UInt64 s0 = _STATE[0];
                System.UInt64 s1 = _STATE[1];
                System.UInt64 result = rotl(s0 + s1, 17) + s0;

                s1 ^= s0;
                _STATE[0] = rotl(s0, 49) ^ s1 ^ (s1 << 21); // a, b
                _STATE[1] = rotl(s1, 28); // c

                return result;
            }

            /// <summary>
            /// This is the jump function for the generator. It is equivalent
            /// to 2^64 calls to Next(); it can be used to generate 2^64
			/// non-overlapping subsequences for parallel computations.
			/// </summary>
			public void Jump()
			{
				System.UInt64 s0 = 0;
				System.UInt64 s1 = 0;
				for (int i = 0; i < JUMP.Length; i++)
				{ 
					for (int b = 0; b < 64; b++)
					{
						if ((JUMP[i] & (System.UInt64) (1 << b)) != 0)
						{
							s0 ^= _STATE[0];
							s1 ^= _STATE[1];
						}
						Next();
					}
				}

                _STATE[0] = s0;
                _STATE[1] = s1;
            }

			/// <summary>
			/// This is the long-jump function for the generator. It is equivalent to
			/// 2^96 calls to <see cref="Next"/>; it can be used to generate 2^32 starting points,
			/// from each of which <see cref="Jump"/> will generate 2^32 non-overlapping
			/// subsequences for parallel distributed computations.
			/// </summary>
			public void LongJump()
			{
				System.UInt64 s0 = 0;
                System.UInt64 s1 = 0;
				for (int i = 0; i < LONG_JUMP.Length; i++) 
				{ 
					for (int b = 0; b < 64; b++)
					{
						if ((LONG_JUMP[i] & (System.UInt64) (1 << b)) != 0)
						{
							s0 ^= _STATE[0];
							s1 ^= _STATE[1];
						}
						Next();
					}
				}
                _STATE[0] = s0;
                _STATE[1] = s1;
            }

        }

        /// <summary>
        /// <para>
        /// This is xoshiro128+ 1.0, our best and fastest 32-bit generator for 32-bit
        /// floating-point numbers.We suggest to use its upper bits for
        /// floating-point generation, as it is slightly faster than xoshiro128**.
        /// It passes all tests we are aware of except for
        /// linearity tests, as the lowest four bits have low linear complexity, so
        /// if low linear complexity is not considered an issue (as it is usually
        /// the case) it can be used to generate 32-bit outputs, too.
        /// </para>
        /// <para>
        /// We suggest to use a sign test to extract a random Boolean value, and right shifts to extract subsets of bits.
        /// </para>
        /// </summary>
        public sealed class Xoshiro128P : IRandomBase
		{
			private static System.UInt32[] JUMP = { 0x8764000b, 0xf542d2d3, 0x6fa035c3, 0x77f2db5b };
			private static System.UInt32[] LONG_JUMP = { 0xb523952e, 0x0b6f099f, 0xccf5a0ef, 0x1c580662 };
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private System.UInt32 rotl(System.UInt32 x, int k) { return (x << k) | (x >> (32 - k)); }
			private static System.UInt32[] STATE = new System.UInt32[4];
            private System.UInt32 _SEED = 0;

            /// <summary>
            /// Create a new instance of <see cref="Xoshiro128P"/> with a default seed value of 0.
            /// </summary>
            public Xoshiro128P() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoshiro128P"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoshiro128P(System.Int32 Seed)
            {
                _SEED = (System.UInt32) Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32) Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32) Seed) ^ 2;
            }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt32) value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Produce a new <see cref="Xoshiro128P"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoshiro128P"/> random number.</returns>
            public System.UInt64 Next()
			{
                System.UInt32 result = STATE[0] + STATE[3];
                System.UInt32 t = STATE[1] << 9;
                STATE[2] ^= STATE[0];
                STATE[3] ^= STATE[1];
                STATE[1] ^= STATE[2];
                STATE[0] ^= STATE[3];
                STATE[2] ^= t;
                STATE[3] = rotl(STATE[3], 11);
                return result;
            }

            /// <summary>
            /// This is the jump function for the generator. It is equivalent
            /// to 2^64 calls to Next(); it can be used to generate 2^64
			/// non-overlapping subsequences for parallel computations.
			/// </summary>
			public void Jump()
			{
                System.UInt32 s0 = 0;
                System.UInt32 s1 = 0;
                System.UInt32 s2 = 0;
                System.UInt32 s3 = 0;
				for (int i = 0; i < JUMP.Length; i++)
				{
					for (int b = 0; b < 32; b++)
					{
						if ((JUMP[i] & 1 << b) != 0)
						{
							s0 ^= STATE[0];
							s1 ^= STATE[1];
							s2 ^= STATE[2];
							s3 ^= STATE[3];
						}
						Next();
					}
				}
                STATE[0] = s0;
                STATE[1] = s1;
                STATE[2] = s2;
                STATE[3] = s3;
            }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }

            /// <summary>
            /// This is the long-jump function for the generator. It is equivalent to
            /// 2^96 calls to Next(); it can be used to generate 2^32 starting points,
            /// from each of which Jump() will generate 2^32 non-overlapping
            /// subsequences for parallel distributed computations.
            /// </summary>
            public void Long_Jump() 
			{
                System.UInt32 s0 = 0;
                System.UInt32 s1 = 0;
                System.UInt32 s2 = 0;
                System.UInt32 s3 = 0;
				for (int i = 0; i < LONG_JUMP.Length; i++)
				{
					for (int b = 0; b < 32; b++)
					{
						if ((LONG_JUMP[i] & 1 << b) != 0)
						{
							s0 ^= STATE[0];
							s1 ^= STATE[1];
							s2 ^= STATE[2];
							s3 ^= STATE[3];
						}
						Next();
					}
				}
                STATE[0] = s0;
                STATE[1] = s1;
                STATE[2] = s2;
                STATE[3] = s3;
            }

            /// <summary>
            /// Disposes the <see cref="Xoshiro128P"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; JUMP = null; LONG_JUMP = null; }

        }

        /// <summary>
        /// <para>
        /// This is xoroshiro128** 1.0, one of our all-purpose, rock-solid,
        /// small-state generators.It is extremely (sub-ns) fast and it passes all
        /// tests we are aware of, but its state space is large enough only for
        /// mild parallelism.
        /// </para>
        /// <para>
        /// For generating just floating-point numbers, xoroshiro128+ is even
        /// faster(but it has a very mild bias, see notes in the comments).
		/// </para>
		/// </summary>
		public sealed class Xoroshiro128SS : IRandomBase
		{
            private System.UInt64[] STATE = new System.UInt64[2];
			private static System.UInt64[] JUMP = { 0xdf900294d8f554a5, 0x170865df4b3201fc };
			private static System.UInt64[] LONG_JUMP = { 0xd2a98b26625eee7b, 0xdddf9b1090aa7ac1 };
            private System.UInt64 _SEED = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)] private System.UInt64 rotl(System.UInt64 x, int k) { return (x << k) | (x >> (64 - k)); }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro128SS"/> with a default seed value of 0.
            /// </summary>
            public Xoroshiro128SS() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro128SS"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro128SS(System.Int32 Seed)
            {
                _SEED = (System.UInt32) Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32)Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32)Seed) ^ 2;
            }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32) _SEED; }
                set
                {
                    _SEED = (System.UInt64) value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Produce a new <see cref="Xoroshiro128SS"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro128SS"/> random number.</returns>
            public System.UInt64 Next()
			{
                System.UInt64 s0 = STATE[0];
                System.UInt64 s1 = STATE[1];
                System.UInt64 result = rotl(s0 * 5, 7) * 9;

                s1 ^= s0;
                STATE[0] = rotl(s0, 24) ^ s1 ^ (s1 << 16); // a, b
                STATE[1] = rotl(s1, 37); // c

                return result;
            }

            /// <summary>
			/// This is the jump function for the generator. It is equivalent
			/// to 2^64 calls to Next(); it can be used to generate 2^64
			/// non-overlapping subsequences for parallel computations.
			/// </summary>
            public void Jump()
			{
                System.UInt64 s0 = 0;
                System.UInt64 s1 = 0;
				for (int i = 0; i < JUMP.Length; i++)
				{
					for (int b = 0; b < 64; b++)
					{
						if ((JUMP[i] & (System.UInt64) (1 << b)) != 0)
						{
							s0 ^= STATE[0];
							s1 ^= STATE[1];
						}
						Next();
					}
				}
                STATE[0] = s0;
                STATE[1] = s1;
            }

			/// <summary>
			/// This is the long-jump function for the generator. It is equivalent to
			/// 2^96 calls to Next(); it can be used to generate 2^32 starting points,
			/// from each of which Jump() will generate 2^32 non-overlapping
			/// subsequences for parallel distributed computations
            /// </summary>
            public void Long_Jump()
			{
				System.UInt64 s0 = 0;
                System.UInt64 s1 = 0;
				for (int i = 0; i < LONG_JUMP.Length; i++)
				{
					for (int b = 0; b < 64; b++)
					{
						if ((LONG_JUMP[i] & (System.UInt64)(1 << b)) != 0)
						{
							s0 ^= STATE[0];
							s1 ^= STATE[1];
						}
						Next();
					}
				}
                STATE[0] = s0;
                STATE[1] = s1;
            }

            /// <summary>
            /// Disposes the <see cref="Xoroshiro128SS"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; JUMP = null; LONG_JUMP = null; }
		}

        /// <summary>
        /// <para>
        /// This is xoroshiro64* 1.0, our best and fastest 32-bit small-state
        /// generator for 32-bit floating-point numbers.We suggest to use its
        /// upper bits for floating-point generation, as it is slightly faster than
        /// xoroshiro64**. It passes all tests we are aware of except for linearity
        /// tests, as the lowest six bits have low linear complexity, so if low
        /// linear complexity is not considered an issue (as it is usually the
        /// case) it can be used to generate 32-bit outputs, too.
        /// </para>
        /// <para>
        /// We suggest to use a sign test to extract a random Boolean value, and right shifts to extract subsets of bits.
        /// </para>
        /// </summary>
        public sealed class Xoroshiro64S : IRandomBase
		{
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private System.UInt32 rotl(System.UInt32 x, int k) { return (x << k) | (x >> (32 - k)); }
            private static System.UInt32[] STATE = new System.UInt32[2];
            private System.UInt32 _SEED = 0;

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro64S"/> with a default seed value of 0.
            /// </summary>
            public Xoroshiro64S() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Produce a new <see cref="Xoroshiro64S"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro64S"/> random number.</returns>
            public System.UInt64 Next()
			{
                System.UInt32 s0 = STATE[0];
                System.UInt32 s1 = STATE[1];
                System.UInt32 result = s0 * 0x9E3779BB;

                s1 ^= s0;
                STATE[0] = rotl(s0, 26) ^ s1 ^ (s1 << 9); // a, b
                STATE[1] = rotl(s1, 13); // c

                return result;
            }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt32)value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro64S"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro64S(System.Int32 Seed)
            {
                _SEED = (System.UInt32)Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32)Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32)Seed) ^ 2;
            }

            /// <summary>
            /// Disposes the <see cref="Xoroshiro64S"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; _SEED = 0; }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }

        }

        /// <summary>
        /// <para>
        /// This is xoroshiro64** 1.0, our 32-bit all-purpose, rock-solid,
        /// small-state generator.It is extremely fast and it passes all tests we
        /// are aware of, but its state space is not large enough for any parallel
        /// application.
        /// </para>
        /// <para>
        /// For generating just single-precision (i.e., 32-bit) floating-point numbers, xoroshiro64* is even faster.
        /// </para>
        /// </summary>
        public sealed class Xoroshiro64SS : IRandomBase 
		{
            [MethodImpl(MethodImplOptions.AggressiveInlining)] private System.UInt32 rotl(System.UInt32 x, int k) { return (x << k) | (x >> (32 - k)); }
            private static System.UInt32[] STATE = new System.UInt32[2];
            private System.UInt32 _SEED = 0;

			/// <summary>
			/// Create a new instance of <see cref="Xoroshiro64SS"/> with a default seed value of 0.
			/// </summary>
			public Xoroshiro64SS() { STATE[0] = 2; STATE[1] = 0; STATE[1] -= 2; }

            /// <summary>
            /// Create a new instance of <see cref="Xoroshiro64SS"/> with the seed value specified.
            /// </summary>
            /// <param name="Seed">A Seed value which will be used for consumption in the generator.</param>
            public Xoroshiro64SS(System.Int32 Seed)
            {
                _SEED = (System.UInt32) Seed;
                if ((Seed / 2) != 0) { STATE[0] = (System.UInt32) Seed / 2; } else { STATE[0] = _SEED * 2; }
                STATE[1] = (STATE[0] + (System.UInt32) Seed) ^ 2;
            }

            /// <summary>
            /// Produce a new <see cref="Xoroshiro64SS"/> random number.
            /// </summary>
            /// <returns>A new <see cref="Xoroshiro64SS"/> random number.</returns>
            public System.UInt64 Next()
			{
				System.UInt32 s0 = STATE[0];
                System.UInt32 s1 = STATE[1];
                System.UInt32 result = rotl(s0 * 0x9E3779BB, 5) * 5;

                s1 ^= s0;
                STATE[0] = rotl(s0, 26) ^ s1 ^ (s1 << 9); // a, b
                STATE[1] = rotl(s1, 13); // c

                return result;
            }

            /// <summary>
            /// Sets or gets the SEED number for this instance.
            /// </summary>
            public System.Int32 Seed
            {
                get { return (System.Int32)_SEED; }
                set
                {
                    _SEED = (System.UInt32)value;
                    if ((_SEED / 2) != 0) { STATE[0] = _SEED / 2; } else { STATE[0] = _SEED * 2; }
                    STATE[1] = (STATE[0] + _SEED) ^ 2;
                }
            }

            /// <summary>
            /// Disposes the <see cref="Xoroshiro64SS"/> instance.
            /// </summary>
            public void Dispose() { STATE = null; _SEED = 0; }

            /// <inheritdoc />
            public System.Boolean Is32Bit { get { return true; } }
        }

		/// <summary>
		/// General purpose class to extract random values smaller than an <see cref="System.UInt64"/> number
		/// returned from the classes that implement the <see cref="IRandomBase"/> interface.
		/// </summary>
		public static class RandomHelpers
		{
			/// <summary>
			/// Converts the random value given to a equivalent <see cref="System.Int32"/> instance.
			/// </summary>
			/// <param name="value">The random value to convert.</param>
			/// <returns>A new <see cref="System.Int32"/> instance that represents the <paramref name="value"/> ,
			/// but as <see cref="System.Int32"/> code points.</returns>
			public static System.Int32 ToInt32(System.UInt64 value) { return (System.Int32) (value >> 48); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int64"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Int64"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Int64"/> code points.</returns>
            public static System.Int64 ToInt64(System.UInt64 value) { return (System.Int32) (value >> 32); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.UInt16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public static System.UInt16 ToUInt16(System.UInt64 value) { return (System.UInt16) ((value >> 32) >> 16); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Int16"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.UInt16"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.UInt16"/> code points.</returns>
            public static System.Int16 ToInt16(System.UInt64 value) { return (System.Int16)((value >> 32) >> 6 >> 11);  }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Byte"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Byte"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Byte"/> code points.</returns>
            public static System.Byte ToByte(System.UInt64 value) { return (System.Byte) ((value >> 32) >> 8 >> 16); }

            /// <summary>
            /// Converts the random value given to a equivalent <see cref="System.Char"/> instance.
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>A new <see cref="System.Char"/> instance that represents the <paramref name="value"/> ,
            /// but as <see cref="System.Char"/> code points.</returns>
            public static System.Char ToChar(System.UInt64 value) { return (System.Char) ToUInt16(value); }

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Single"/> value using a special formula. </para>
			/// <para>This formula guarantees that one value returned from 10 random iterations might produce 
			/// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Single"/> number produced by the formula.</returns>
            public static System.Single ToFloat(System.UInt64 value) { return (System.Single) ((value >> 32) >> 16 / 5 / 4); }

            /// <summary>
            /// <para>Converts the random value given to a <see cref="System.Double"/> value using a special formula. </para>
            /// <para>This formula guarantees that one value returned from 10 random iterations might produce 
            /// a number that is fully convertible to an integer. </para>
            /// </summary>
            /// <param name="value">The random value to convert.</param>
            /// <returns>The random <see cref="System.Double"/> number produced by the formula.</returns>
            public static System.Double ToDouble(System.UInt64 value) { return (System.Double) ((value >> 32) >> 16 / 5 / 4); }

		}

    }

    /// <summary>
    /// This <see langword="struct" /> bears a resemblance to the <see cref="System.Collections.ArrayList"/> , 
    /// but this extends it's functionality and it's aim is to work only with <see cref="System.Byte"/> data.
    /// It is useful also for interplaying between an array and a generic <see cref="IList{T}"/>
    /// collection , which it combines the advantages that both provide.
    /// </summary>
    /// <remarks>
    /// <para> This <see langword="struct" /> implements the <see cref="IList{T}"/> interface. </para>
    /// <para> Note: &lt;T&gt; is <see cref="System.Byte"/>. </para>
    /// </remarks>
    public struct ModifidableBuffer : System.Collections.Generic.IList<System.Byte>
	{
		private readonly System.Collections.Specialized.OrderedDictionary _dict = new();
		private System.Int32 Iter = 0;

		/// <summary>
		/// Initialise a new modifidable buffer with no data in it.
		/// </summary>
		public ModifidableBuffer() { }

		/// <summary>
		/// Initialise a new modifidable buffer and populate it with data taken 
		/// from a instantiated <see cref="System.Byte"/>[] array. 
		/// </summary>
		/// <param name="Value">The <see cref="System.Byte"/>[] data.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModifidableBuffer(System.Byte[] Value) { for (System.Int32 O = 0; O < Value.Length; O++) { Add(Value[O]); } }

		/// <summary>
		/// Initialise a new modifidable buffer and populate it with data taken 
		/// from a instantiated <see cref="System.Byte"/>[] array. 
		/// </summary>
		/// <param name="Value">The <see cref="System.Byte"/>[] data.</param>
		/// <param name="Index">The index that this instance will start 
		/// saving data from <paramref name="Value"/> parameter.</param>
		/// <param name="Count">How many elements to 
		/// copy from the <paramref name="Value"/> array.</param>
		/// <exception cref="InvalidOperationException">Index parameter is not allowed to be more than Count parameter.</exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ModifidableBuffer(System.Byte[] Value, System.Int32 Index, System.Int32 Count)
		{
			if (Index >= Count) { throw new InvalidOperationException("Index parameter is not allowed to be more than Count parameter."); }
			for (System.Int32 O = Index; O < Count; O++) { Add(Value[O]); }
		}

		/// <inheritdoc />
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public System.Int32 IndexOf(System.Byte Value)
		{
			foreach (System.Collections.Generic.KeyValuePair<System.Int32, System.Byte> DE in _dict)
			{
				if (DE.Value == Value) { return DE.Key; }
			}
			return -1;
		}

		/// <inheritdoc />
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public void Insert(System.Int32 Index, System.Byte Value)
		{
			try
			{
				System.Boolean Flag = false;
				foreach (System.Collections.Generic.KeyValuePair<System.Int32, System.Byte> DE in _dict)
				{
					if (DE.Key == Index) { Flag = true; }
				}
				if (Flag) { _dict[Index] = Value; } else { Add(Value); }
			}
			catch (System.Exception EX)
			{
				throw new System.AggregateException("Could not add the specified value to the dictionary.", EX);
			}
		}

		/// <inheritdoc />
		/// <remarks>Be careful when removing entries , this will make the whole data
		/// array to shift by one and cover up the blank entry. Using this method
		/// can result to data corruption.</remarks>
		public void RemoveAt(System.Int32 Index) { _dict.Remove(Index); Iter--; }

		/// <summary>
		/// Adds a new entry to this instance.
		/// </summary>
		/// <param name="Value">The <see cref="System.Byte"/> value to add to the newly created entry.</param>
		/// <exception cref="System.AggregateException">Thrown when the adding was failed for a reason.</exception>
		public void Add(System.Byte Value)
		{
			try
			{
				_dict.Add(Iter, Value);
				Iter++;
			}
			catch (System.Exception EX)
			{
				throw new System.AggregateException($"Could not add the specified value to the dictionary.", EX);
			}
		}

		/// <inheritdoc />
		public System.Byte this[System.Int32 Index]
		{
			get { return (System.Byte)_dict[Index]; }
			set { Insert(Index, value); }
		}

		/// <summary>
		/// Adds empty entries specified by the <paramref name="Times"/> <see cref="System.Int32"/> .
		/// </summary>
		/// <param name="Times">The number of empty entries to add.</param>
		[System.Runtime.ConstrainedExecution.ReliabilityContract(
			System.Runtime.ConstrainedExecution.Consistency.MayCorruptAppDomain , 
			System.Runtime.ConstrainedExecution.Cer.MayFail)]
		public void AddEntries(System.Int32 Times) { for (System.Int32 I = 0; I < Times; I++) { _dict.Add(Iter++, 0); } }

		/// <inheritdoc />
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public void CopyTo(System.Byte[] Array, System.Int32 Index)
		{
			System.Int32 tmp = 0;
			for (System.Int32 I = Index; I < Iter; I++) { Array[tmp] = (System.Byte)_dict[I]; tmp++; }
		}

		/// <summary>
		/// The <see cref="ToArray()"/> method gets all the data representing the current buffer , and returns them
		/// as a one-dimensional and fixed <see cref="System.Byte"/>[] array.
		/// </summary>
		/// <returns>The data which this <see langword="struct"/> holds.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public System.Byte[] ToArray()
		{
			System.Byte[] bytes = new System.Byte[Iter];
			CopyTo(bytes, 0);
			return bytes;
		}

		/// <inheritdoc />
		public System.Int32 Count { get { return Iter; } }

		/// <inheritdoc />
		public void Clear() { _dict.Clear(); Iter = 0; }

		/// <inheritdoc />
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public System.Boolean Contains(System.Byte item)
		{
			foreach (System.Collections.Generic.KeyValuePair<System.Int32, System.Byte> DE in _dict)
			{
				if (DE.Value == item) { return true; }
			}
			return false;
		}

		/// <inheritdoc />
		/// <remarks>Note: This property always returns <c>false</c>.</remarks>
		public System.Boolean IsReadOnly { get { return false; } }

		/// <inheritdoc />
		/// <remarks>Be careful when removing entries , this will make the whole data
		/// array to shift by one and cover up the blank entry. Using this method
		/// can result to data corruption.</remarks>
		public System.Boolean Remove(System.Byte item)
		{
			if (IndexOf(item) == -1) { return false; }
			try
			{
				_dict.Remove(IndexOf(item));
				Iter--;
			}
			catch { return false; }
			return true;
		}

		IEnumerator<System.Byte> IEnumerable<System.Byte>.GetEnumerator()
		{
			System.Collections.Generic.IList<System.Byte> result = new System.Byte[_dict.Count];
			for (System.Int32 I = 0; I < _dict.Count; I++) { result[I] = (System.Byte)_dict[I]; }
			return result.GetEnumerator();
		}

		/// <inheritdoc />
		public System.Collections.IEnumerator GetEnumerator() { return _dict.GetEnumerator(); }

		/// <summary>
		/// Returns the byte data , but as an hexadecimal <see cref="System.String"/> , if it fits to one.
		/// Otherwise , the <see cref="System.String"/> representation of this type.
		/// </summary>
		/// <returns>The <see cref="System.Byte"/> data kept by this instance as a 
		/// <see cref="System.String"/> , otherwise 
		/// the <see cref="System.String"/> representation of this type.</returns>
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public override System.String ToString()
		{
			System.String Thestring = null;
			try
			{
				for (System.Int32 I = 0; I < Iter; I++)
				{
					Thestring += this[I].ToString("x2");
				}
				return Thestring;
			} catch { return GetType().ToString(); }
		}

		/// <summary>
		/// Convert immediately from an intitialised <see cref="ModifidableBuffer"/> structure as an new 
		/// <see cref="System.Byte"/>[] array.
		/// </summary>
		/// <param name="instance">The <see cref="ModifidableBuffer"/> instance to take the data from.</param>
		public static explicit operator System.Byte[](ModifidableBuffer instance) { return instance.ToArray(); }

		/// <summary>
		/// Convert the <see cref="System.Byte"/>[] data as an new <see cref="ModifidableBuffer"/>.
		/// </summary>
		/// <param name="Data">The <see cref="System.Byte"/>[] to take the data from.</param>
		public static explicit operator ModifidableBuffer(System.Byte[] Data) { return new ModifidableBuffer(Data); }
	}

	internal struct STDConstants
	{
		// Standard Text Definition constants and helper functions.
		// The below characters define how the STD format will be read and written.

		public const System.Char COMMENT = '#';

		public const System.Char SEPERATOR = '$';

		public const System.Char HDROPEN = '{';

		public const System.Char HDRCLOSE = '}';

		public const System.Char STRING = '\"';

		public const System.Int32 VERSION = 1;

		// The encoding table. 
		// This table will be used so as to return and save the colors as one value 
		// to the dictionary.
		// Example: Array is at index 0.
		// The Index 0 returns two values: 
		// The first one is the foreground color.
		// The second one is the background color.
		// So , { 0 , 0 } means that the foreground and background colors will be black.
		// Note: Consult the STDFrontColor and STDBackColor enums for more information about this
		// generated table.
		public static readonly System.Int32[,] EncodeTable = { { 0 , 0 } , { 0 , 1 } , { 0 , 2 } ,
		{ 0 , 3 } , { 0 , 4 } , { 0 , 5 } , { 0 , 6 } , { 0 , 7 } , { 0 , 8 } , { 0 , 9 } , { 0 , 10 } ,
		{ 0 , 11 } , { 0 , 12 } , { 0 , 13 } , { 0 , 14 } , { 0 , 15 } , { 1 , 0 } , { 1 , 1 } , { 1 , 2 } ,
		{ 1 , 3 } , { 1 , 4 } , { 1 , 5 } , { 1 , 6 } , { 1 , 7 } , { 1 , 8 } , { 1 , 9 } , { 1 , 10 } ,
		{ 1 , 11 } , { 1 , 12 } , { 1 , 13 } , { 1 , 14 } , { 1 , 15 } , { 2 , 0 } , { 2 , 1 } , { 2 , 2 } ,
		{ 2 , 3 } , { 2 , 4 } , { 2 , 5 } , { 2 , 6 } , { 2 , 7 } , { 2 , 8 } , { 2 , 9 } , { 2 , 10 } ,
		{ 2 , 11 } , { 2 , 12 } , { 2 , 13 } , { 2 , 14 } , { 2 , 15 } , { 3 , 0 } , { 3 , 1 } , { 3 , 2 } ,
		{ 3 , 3 } , { 3 , 4 } , { 3 , 5 } , { 3 , 6 } , { 3 , 7 } , { 3 , 8 } , { 3 , 9 } , { 3 , 10 } ,
		{ 3 , 11 } , { 3 , 12 } , { 3 , 13 } , { 3 , 14 } , { 3 , 15 } , { 4 , 0 } , { 4 , 1 } , { 4 , 2 } ,
		{ 4 , 3 } , { 4 , 4 } , { 4 , 5 } , { 4 , 6 } , { 4 , 7 } , { 4 , 8 } , { 4 , 9 } , { 4 , 10 } ,
		{ 4 , 11 } , { 4 , 12 } , { 4 , 13 } , { 4 , 14 } , { 4 , 15 } , { 5 , 0 } , { 5 , 1 } , { 5 , 2 } ,
		{ 5 , 3 } , { 5 , 4 } , { 5 , 5 } , { 5 , 6 } , { 5 , 7 } , { 5 , 8 } , { 5 , 9 } , { 5 , 10 } ,
		{ 5 , 11 } , { 5 , 12 } , { 5 , 13 } , { 5 , 14 } , { 5 , 15 } , { 6 , 0 } , { 6 , 1 } , { 6 , 2 } ,
		{ 6 , 3 } , { 6 , 4 } , { 6 , 5 } , { 6 , 6 } , { 6 , 7 } , { 6 , 8 } , { 6 , 9 } , { 6 , 10 } ,
		{ 6 , 11 } , { 6 , 12 } , { 6 , 13 } , { 6 , 14 } , { 6 , 15 } , { 7 , 0 } , { 7 , 1 } , { 7 , 2 } ,
		{ 7 , 3 } , { 7 , 4 } , { 7 , 5 } , { 7 , 6 } , { 7 , 7 } , { 7 , 8 } , { 7 , 9 } , { 7 , 10 } ,
		{ 7 , 11 } , { 7 , 12 } , { 7 , 13 } , { 7 , 14 } , { 7 , 15 } , { 8 , 0 } , { 8 , 1 } , { 8 , 2 } ,
		{ 8 , 3 } , { 8 , 4 } , { 8 , 5 } , { 8 , 6 } , { 8 , 7 } , { 8 , 8 } , { 8 , 9 } , { 8 , 10 } ,
		{ 8 , 11 } , { 8 , 12 } , { 8 , 13 } , { 8 , 14 } , { 8 , 15 } , { 9 , 0 } , { 9 , 1 } , { 9 , 2 } ,
		{ 9 , 3 } , { 9 , 4 } , { 9 , 5 } , { 9 , 6 } , { 9 , 7 } , { 9 , 8 } , { 9 , 9 } , { 9 , 10 } ,
		{ 9 , 11 } , { 9 , 12 } , { 9 , 13 } , { 9 , 14 } , { 9 , 15 } , { 10 , 0 } , { 10 , 1 } , { 10 , 2 } ,
		{ 10 , 3 } , { 10 , 4 } , { 10 , 5 } , { 10 , 6 } , { 10 , 7 } , { 10 , 8 } , { 10 , 9 } , { 10 , 10 } ,
		{ 10 , 11 } , { 10 , 12 } , { 10 , 13 } , { 10 , 14 } , { 10 , 15 } , { 11 , 0 } , { 11 , 1 } , { 11 , 2 } ,
		{ 11 , 3 } , { 11 , 4 } , { 11 , 5 } , { 11 , 6 } , { 11 , 7 } , { 11 , 8 } , { 11 , 9 } , { 11 , 10 } ,
		{ 11 , 11 } , { 11 , 12 } , { 11 , 13 } , { 11 , 14 } , { 11 , 15 } , { 12 , 0 } , { 12 , 1 } , { 12 , 2 } ,
		{ 12 , 3 } , { 12 , 4 } , { 12 , 5 } , { 12 , 6 } , { 12 , 7 } , { 12 , 8 } , { 12 , 9 } , { 12 , 10 } ,
		{ 12 , 11 } , { 12 , 12 } , { 12 , 13 } , { 12 , 14 } , { 12 , 15 } , { 13 , 0 } , { 13 , 1 } , { 13 , 2 } ,
		{ 13 , 3 } , { 13 , 4 } , { 13 , 5 } , { 13 , 6 } , { 13 , 7 } , { 13 , 8 } , { 13 , 9 } , { 13 , 10 } ,
		{ 13 , 11 } , { 13 , 12 } , { 13 , 13 } , { 13 , 14 } , { 13 , 15 } , { 14 , 0 } , { 14 , 1 } , { 14 , 2 } ,
		{ 14 , 3 } , { 14 , 4 } , { 14 , 5 } , { 14 , 6 } , { 14 , 7 } , { 14 , 8 } , { 14 , 9 } , { 14 , 10 } ,
		{ 14 , 11 } , { 14 , 12 } , { 14 , 13 } , { 14 , 14 } , { 14 , 15 } , { 15 , 0 } , { 15 , 1 } , { 15 , 2 } ,
		{ 15 , 3 } , { 15 , 4 } , { 15 , 5 } , { 15 , 6 } , { 15 , 7 } , { 15 , 8 } , { 15 , 9 } , { 15 , 10 } ,
		{ 15 , 11 } , { 15 , 12 } , { 15 , 13 } , { 15 , 14 } , { 15 , 15 } };

		// This function encodes the two enum values accepted and then converts them as a single value.
		// The value returned from this function is actually the index of the EncodeTable array that is equal
		// to the values supplied against this function.
		// Error handling: -1 suggests that the encoder failed for a reason.
		// 0 is the first index of the EncodeTable array.
		public static System.Int32 Encode(STDFrontColor Front, STDBackColor Back)
		{
			System.Int32 KeepFr = -1;
			System.Int32 KeepBk = -1;

			for (System.Int32 I = 0; I < EncodeTable.Length; I++)
			{
				if (EncodeTable[I, 0] == (System.Int32)Front) { KeepFr = I; }
				if (EncodeTable[I, 1] == (System.Int32)Back) { KeepBk = I; }

				if ((KeepFr != -1) && (KeepBk != -1))
				{
					if (KeepFr == KeepBk) { return KeepFr; } else { continue; }
				}
			}
			return -1;
		}

		// This function decodes the single number given from the Encode function and then 
		// returns the two colors.
		public static STDColors Decode(System.Int32 Encoded)
		{
			return new STDColors(
				(STDFrontColor)EncodeTable[Encoded, 0],
				(STDBackColor)EncodeTable[Encoded, 1]);
		}
	
	}

    /// <summary>
    /// Represents a new STD (Standard Text with color Definition) context , which is a storage type
    /// which holds the STD data parsed , or the STD data to parse.
    /// </summary>
    public struct STDContext : System.IDisposable , IEnumerable<STDLine>
	{
		/// <summary>
		/// Initialise a new instance of the <see cref="STDContext"/> structure.
		/// </summary>
		public STDContext() { }

		// Dictionary that keeps the string data to display.
		private System.Collections.Generic.IDictionary<System.Int32, System.String> _dt1 = 
			new System.Collections.Generic.SortedList<System.Int32 , System.String>();	
		// Dictionary that keeps which colors to display to the user.
		// The value set here is returned by the Encode function.
		private System.Collections.Generic.IDictionary<System.Int32, System.Int32> _dt2 =
            new System.Collections.Generic.SortedList<System.Int32, System.Int32>();
        // Dictionary that keeps data determining the type of data written to it , so :
        // 0 indicates a normal STD string , 
        // 1 indicates the STD version block , 
        // and 2 indicates a comment in the file.
		// These values are also exposed at ROOT.STDType enum.
        private System.Collections.Generic.IDictionary<System.Int32, System.Int32> _dt3 =
            new System.Collections.Generic.SortedList<System.Int32, System.Int32>();
        private System.Boolean addedheader = false;
        private System.Int32 Count = -1;
		private System.Boolean _disposed = false;

        /// <summary>
        /// Adds a new STD (Standard Text with color Definition) Line to store in the dictionary.
        /// </summary>
        /// <param name="Fr">The foreground color.</param>
        /// <param name="Bk">The background color.</param>
        /// <param name="Data">The string data to save.</param>
        /// <exception cref="InvalidOperationException" />
        public void Add(STDFrontColor Fr, STDBackColor Bk, System.String Data)
		{
            CheckIfDisposed();
            Count++;
			System.Int32 ER = STDConstants.Encode(Fr, Bk);
			if (ER == -1)
			{
				throw new InvalidOperationException("The Color values given were out of bounds.");
			}
			_dt1.Add(Count, Data);
			_dt2.Add(Count, ER);
			_dt3.Add(Count, 0);
		}

        /// <summary>
        /// Adds a new STD (Standard Text with color Definition) comment.
        /// </summary>
        /// <param name="Comment">The <see cref="System.String"/> comment data to pass.</param>
        public void AddComment(System.String Comment)
		{
            CheckIfDisposed();
            Count++;
			_dt1.Add(Count, Comment);
			_dt2.Add(Count, 0);
			_dt3.Add(Count, 2);
		}

        /// <summary>
        /// Adds a new STD (Standard Text with color Definition) version block.
        /// </summary>
        public void AddVersionBlock()
		{
            CheckIfDisposed();
            if (addedheader)
            {
                throw new InvalidOperationException("A version block has already been added." +
                "\nNo need to add the block two times.");
            }
            else { addedheader = true; }
            Count++;
			_dt1.Add(Count, $"{{Version${STDConstants.VERSION}}}");
			_dt2.Add(Count, 0);
			_dt3.Add(Count, 1);
		}

        /// <summary>
        /// Clears the saved STD (Standard Text with color Definition) entries , which can make this instance reusable.
        /// </summary>
        public void Clear()
		{
            CheckIfDisposed();
            Count = -1;
			addedheader = false;
			_dt1.Clear();
			_dt2.Clear();
			_dt3.Clear();
		}

		/// <summary>
		/// Disposes the current instance. This method is the same as <see cref="Clear()"/> , 
		/// but invalidates the internal dictionaries too.
		/// </summary>
		/// <remarks>
		/// It is not important to explicitly call <see cref="Dispose()"/> , because you can re-use this instance by calling
		/// the <see cref="Clear()"/> method and you have the instance as it is was created.
		/// </remarks>
		public void Dispose() { CheckIfDisposed(); Clear(); _dt1 = null; _dt2 = null; _dt3 = null; _disposed = true; }

		// Adds an invalid item to the dictionary.
		// This is added so as to detect parser errors or code mistakes.
		internal void AddInvalidItem(System.String Data) 
		{
            CheckIfDisposed();
            Count++;
            _dt1.Add(Count, Data);
            _dt2.Add(Count, 0);
            _dt3.Add(Count, 3);
        }

        /// <summary>
        /// Counts the found STD (Standard Text with color Definition) entries that are existing on this instance.
        /// </summary>
		/// <remarks>This property includes ALL STD entries , including the Version Block ,
		/// Comments and STD entries.</remarks>
        public System.Int32 ItemsCount { get { CheckIfDisposed(); if (Count == -1) { return 0; } else { return Count + 1; } } }

        /// <summary>
        /// Gets the specified STD (Standard Text with color Definition) Line Entry at the specified index.
        /// </summary>
        /// <param name="Index">The index to get the entry from.</param>
        /// <returns>A new <see cref="STDLine"/> <see langword="struct"/> which contains the STD data.</returns>
        public STDLine Get(System.Int32 Index)
		{
            CheckIfDisposed();
            return new STDLine() 
			{ 
				Colors = STDConstants.Decode(_dt2[Index]) , 
				Data = _dt1[Index] , Type = (STDType) _dt3[Index] 
			};
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { CheckIfDisposed(); return new STDENUM(this); }

		/// <inheritdoc />
		public IEnumerator<STDLine> GetEnumerator() { CheckIfDisposed(); return new STDENUM(this); }

		/// <summary>
		/// Return the data that an STD structure holds as a new <see cref="STDLine"/>[] array.
		/// </summary>
		/// <param name="context">The STD structure to get the data from.</param>
		public static explicit operator STDLine[](STDContext context) 
		{
			STDLine[] output = new STDLine[context.ItemsCount];
			for (System.Int32 I = 0; I < output.Length; I++) { output[I] = context.Get(I); }
			return output;
		}
	
		private void CheckIfDisposed() { if (_disposed) { throw new ObjectDisposedException(this.GetType().FullName , 
			"Disposed STD contexts are not allowed to be reused.\n If you want to reuse an STD context, " +
			$"then use the {this.GetType().FullName}.Clear() method."); } }
	}

	// The internal Enumerator implementation.
	// This structure is needed so as the GetEnumerator() function can be exported.
	// However , you might not need to use ever that function; On the other hand , 
	// this kind of implementation is used in an 'foreach' ('For Each' for Visual Basic)
	// context , which that one uses the GetEnumerator() function , which that keyword
	// is really a nice way to iterate through the STD Context without hassle.
    internal struct STDENUM : IEnumerator<STDLine>
    {
        System.Int32 POS = -1;
        STDContext _context;

		public STDENUM() { throw new AggregateException("This constructor cannot build the enumerator correctly.");  }

        public STDENUM(STDContext context) { _context = context; }

        public STDLine Current { get { return _context.Get(POS); } }

        System.Object System.Collections.IEnumerator.Current { get { return _context.Get(POS); } }

        public System.Boolean MoveNext()
        {
            POS++;
            if (POS >= _context.ItemsCount) { return false; }
            return true;
        }

        public void Reset() { POS = -1; }

        public void Dispose() { _context.Dispose(); }
    }

    /// <summary>
    /// Represents an STD (Standard Text with color Definition) Line.
    /// </summary>
    [Serializable]
	public struct STDLine
	{
		/// <summary>
		/// The colors to use.
		/// </summary>
		public STDColors Colors;

		/// <summary>
		/// The <see cref="System.String"/> data to show.
		/// </summary>
		public System.String Data;

		/// <summary>
		/// The STD type that was got.
		/// </summary>
		public STDType Type;
	}

    /// <summary>
    /// This structure keeps a record of STD colors that each line uses.
    /// </summary>
    [Serializable]
    public readonly struct STDColors
    {
        /// <summary>
        /// The foreground color to specify.
        /// </summary>
        public readonly STDFrontColor FrontColor;

        /// <summary>
        /// The background color to specify.
        /// </summary>
        public readonly STDBackColor BackColor;

        /// <summary>
        /// Initialise a new <see cref="STDColors"/> structure with the specified STD colors.
        /// </summary>
        /// <param name="FC">The foreground color to specify.</param>
        /// <param name="BK">The background color to specify.</param>
        public STDColors(STDFrontColor FC, STDBackColor BK) { FrontColor = FC; BackColor = BK; }
    }

#pragma warning disable CS1591
    public enum STDFrontColor : System.Int32
	{
		INV = -2 , 
		Black = 0 , 
		White  = 1 ,
		Blue = 2 , 
		Red = 3 , 
		Magenta = 4 ,
		Cyan = 5 ,
		Gray = 6 , 
		Green = 7 ,
		Yellow = 8 ,
        DarkBlue = 9 ,
        DarkCyan = 10 ,
        DarkGray = 11 ,
        DarkGreen = 12 ,
        DarkMagenta = 13 ,
		DarkRed = 14 , 
		DarkYellow = 15
    }

    /// <summary>
    /// The STD (Standard Text with color Definition) type that the entry you want to compare against is.
    /// </summary>
    public enum STDType : System.Int32
	{
		/// <summary>
		/// The Entry is a normal STD string definition.
		/// </summary>
		STDString = 0 , 
		/// <summary>
		/// The Entry is a valid STD version block.
		/// </summary>
		VersionBlock = 1,
		/// <summary>
		/// The Entry is a comment.
		/// </summary>
		Comment = 2 ,
		/// <summary>
		/// Invalid Entry which should be ignored.
		/// Normally , this occurs due to the parser incorrect readiness , but was not an error due to the following resons:
		/// </summary>
		/// <remarks>
		/// Unexpected new line , corrupt text , or unexpected characters returned.
		/// </remarks>
		Invalid = 3 
	}

    public enum STDBackColor : System.Int32
    {
		INV = -2,
        Black = 0,
        White = 1,
        Blue = 2,
        Red = 3,
        Magenta = 4,
        Cyan = 5,
        Gray = 6,
        Green = 7,
        Yellow = 8,
        DarkBlue = 9,
        DarkCyan = 10,
        DarkGray = 11,
        DarkGreen = 12,
        DarkMagenta = 13,
        DarkRed = 14,
        DarkYellow = 15
    }

#pragma warning restore CS1591

	/// <summary>
	/// <para>
	/// The STD (Standard Text with color Definition) class gets data 
	/// from strings that contain messages colored as specified.
	/// </para>
	/// <para>
	/// This class contains static methods to do serialisation/deserialisation of this format.
	/// </para>
	/// </summary>
	/// <remarks>The STD format is an easy style outlining definition which can be used to a number of applications.
	/// Read more about it in the coding website.</remarks>
    public static class STD
	{
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static STDFrontColor ParseFrontColor(System.String C)
		{
			switch (C.ToUpperInvariant()) 
			{
				case "BLACK": return STDFrontColor.Black;
				case "WHITE": return STDFrontColor.White;
				case "BLUE": return STDFrontColor.Blue;
				case "RED": return STDFrontColor.Red;
				case "GREEN": return STDFrontColor.Green;
				case "MAGENTA": return STDFrontColor.Magenta;
				case "YELLOW": return STDFrontColor.Yellow;
				case "CYAN": return STDFrontColor.Cyan;
				case "DARK BLUE": return STDFrontColor.DarkBlue;
				case "GRAY": return STDFrontColor.Gray;
				case "DARK CYAN": return STDFrontColor.DarkCyan;
				case "DARK GRAY": return STDFrontColor.DarkGray;
				case "DARK GREEN": return STDFrontColor.DarkGreen;
				case "DARK MAGENTA": return STDFrontColor.DarkMagenta;
				case "DARK YELLOW": return STDFrontColor.DarkYellow;
				case "DARK RED": return STDFrontColor.DarkRed;
				default: return (STDFrontColor) (-2);
			}
		}
        
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static STDBackColor ParseBackColor(System.String C)
        {
            switch (C.ToUpperInvariant())
            {
                case "BLACK": return STDBackColor.Black;
                case "WHITE": return STDBackColor.White;
                case "BLUE": return STDBackColor.Blue;
                case "RED": return STDBackColor.Red;
                case "GREEN": return STDBackColor.Green;
                case "MAGENTA": return STDBackColor.Magenta;
                case "YELLOW": return STDBackColor.Yellow;
                case "CYAN": return STDBackColor.Cyan;
                case "DARK BLUE": return STDBackColor.DarkBlue;
                case "GRAY": return STDBackColor.Gray;
                case "DARK CYAN": return STDBackColor.DarkCyan;
                case "DARK GRAY": return STDBackColor.DarkGray;
                case "DARK GREEN": return STDBackColor.DarkGreen;
                case "DARK MAGENTA": return STDBackColor.DarkMagenta;
                case "DARK YELLOW": return STDBackColor.DarkYellow;
                case "DARK RED": return STDBackColor.DarkRed;
                default: return (STDBackColor) (-2);
            }
        }
        
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static System.String ParseFrontColorS(STDFrontColor C)
		{
			return C switch
			{
				STDFrontColor.Black => "Black",
				STDFrontColor.White => "White",
				STDFrontColor.Cyan => "Cyan",
				STDFrontColor.Red => "Red",
				STDFrontColor.Blue => "Blue",
				STDFrontColor.Gray => "Gray",
				STDFrontColor.Magenta => "Magenta",
				STDFrontColor.Green => "Green",
				STDFrontColor.Yellow => "Yellow",
				STDFrontColor.DarkBlue => "Dark Blue",
				STDFrontColor.DarkGreen => "Dark Green" ,
				STDFrontColor.DarkRed => "Dark Red",
				STDFrontColor.DarkYellow => "Dark Yellow" ,
				STDFrontColor.DarkGray => "Dark Gray",
				STDFrontColor.DarkCyan => "Dark Cyan",
				STDFrontColor.DarkMagenta => "Dark Magenta",
				_ => ""
			};
		}
        
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static System.String ParseBackColorS(STDBackColor C)
		{
            return C switch
            {
                STDBackColor.Black => "Black",
                STDBackColor.White => "White",
                STDBackColor.Cyan => "Cyan",
                STDBackColor.Red => "Red",
                STDBackColor.Blue => "Blue",
                STDBackColor.Gray => "Gray",
                STDBackColor.Magenta => "Magenta",
                STDBackColor.Green => "Green",
                STDBackColor.Yellow => "Yellow",
                STDBackColor.DarkBlue => "Dark Blue",
                STDBackColor.DarkGreen => "Dark Green",
                STDBackColor.DarkRed => "Dark Red",
                STDBackColor.DarkYellow => "Dark Yellow",
                STDBackColor.DarkGray => "Dark Gray",
                STDBackColor.DarkCyan => "Dark Cyan",
                STDBackColor.DarkMagenta => "Dark Magenta",
                _ => ""
            };
        }

		/*
#pragma warning disable CS8509
		private static System.ConsoleColor AsConsoleColorFrnt(STDFrontColor C)
		{
			return C switch
			{
				STDFrontColor.Black => System.ConsoleColor.Black,
				STDFrontColor.Magenta => System.ConsoleColor.Magenta,
				STDFrontColor.White => System.ConsoleColor.White,
				STDFrontColor.Blue => System.ConsoleColor.Blue,
				STDFrontColor.Red => System.ConsoleColor.Red,
				STDFrontColor.Cyan => System.ConsoleColor.Cyan,
				STDFrontColor.Gray => System.ConsoleColor.Gray,
				STDFrontColor.Green => System.ConsoleColor.Green,
				STDFrontColor.Yellow => System.ConsoleColor.Yellow,
				STDFrontColor.DarkBlue => System.ConsoleColor.DarkBlue,
				STDFrontColor.DarkCyan => System.ConsoleColor.DarkCyan,
				STDFrontColor.DarkGray => System.ConsoleColor.DarkGray,
				STDFrontColor.DarkMagenta => System.ConsoleColor.DarkMagenta,
				STDFrontColor.DarkGreen => System.ConsoleColor.DarkGreen,
				STDFrontColor.DarkRed => System.ConsoleColor.DarkRed,
				STDFrontColor.DarkYellow => System.ConsoleColor.DarkYellow
			};
		}

		private static System.ConsoleColor AsConsoleColorBack(STDBackColor C)
		{
            return C switch
            {
                STDBackColor.Black => System.ConsoleColor.Black,
                STDBackColor.Magenta => System.ConsoleColor.Magenta,
                STDBackColor.White => System.ConsoleColor.White,
                STDBackColor.Blue => System.ConsoleColor.Blue,
                STDBackColor.Red => System.ConsoleColor.Red,
                STDBackColor.Cyan => System.ConsoleColor.Cyan,
                STDBackColor.Gray => System.ConsoleColor.Gray,
                STDBackColor.Green => System.ConsoleColor.Green,
                STDBackColor.Yellow => System.ConsoleColor.Yellow,
                STDBackColor.DarkBlue => System.ConsoleColor.DarkBlue,
                STDBackColor.DarkCyan => System.ConsoleColor.DarkCyan,
                STDBackColor.DarkGray => System.ConsoleColor.DarkGray,
                STDBackColor.DarkMagenta => System.ConsoleColor.DarkMagenta,
                STDBackColor.DarkGreen => System.ConsoleColor.DarkGreen,
                STDBackColor.DarkRed => System.ConsoleColor.DarkRed,
                STDBackColor.DarkYellow => System.ConsoleColor.DarkYellow
            };
        }
#pragma warning restore CS8509 */

		/// <summary>
		/// Deserialise the STD string data and convert them to a new STD Context.
		/// </summary>
		/// <param name="Data">The <see cref="System.String"/> data to parse.</param>
		/// <returns>A new STD Context.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static STDContext Deserialize(System.String Data)
		{
			System.String[] _tmp = Data.Split('\n');
			System.Boolean terminated = true;
			System.String slicer = null;
			System.Int32 Iterator0 = 0;
            System.Int32 Iterator1 = 0;
            System.String Temp = null;
			System.String Front = null;
			System.String Back = null;
			System.Boolean GivenVersion = false;
			STDContext STD = new();


            foreach (System.String Runner in _tmp)
			{
                try
				{
					if (Runner.IndexOf(STDConstants.COMMENT, 0, 2) != -1)
					{
						// skip execution , it is a comment.
						STD.AddComment(Runner.Substring(Runner.IndexOf(STDConstants.COMMENT) + 1));
						terminated = true;
						slicer = null;
						continue;
					}

					if (Runner.Trim() == "") { break; }

					// In normal practice , it would be expected that the parser should throw an error.
					// However , this is done so as to parse as most as possible uncorrupted data.
					// So , it instead adds an invalid item that can be examined by the programmer and detect the mistake or corruption.
					if (((Runner.IndexOf(STDConstants.STRING) == -1) && (Runner.IndexOf(STDConstants.SEPERATOR) == -1)) &&
						(Runner.IndexOf(STDConstants.COMMENT) == -1) && (Runner.IndexOf(STDConstants.HDRCLOSE) == -1) &&
						(Runner.IndexOf(STDConstants.HDROPEN) == -1))
					{
						// Has none of the specified STD characters. In this case , add a new invalid item.
						STD.AddInvalidItem(Runner);
						continue;
					}

					if (Runner.IndexOf(STDConstants.HDROPEN, 0, 1) != -1)
					{
						// Start parsing the version block.
						Temp = Runner.Substring(Runner.IndexOf(STDConstants.HDROPEN) + 1, Runner.IndexOf(STDConstants.SEPERATOR) - 1);
                        if (Temp.ToUpperInvariant() == "VERSION")
						{
							Temp = Runner.Substring(Runner.IndexOf(STDConstants.SEPERATOR) + 1, Runner.Length - Runner.IndexOf(STDConstants.HDRCLOSE));
                            if (System.Convert.ToInt32(Temp) == STDConstants.VERSION) { GivenVersion = true; STD.AddVersionBlock(); }
						}
						continue;
					}

					if ((Runner.IndexOf(STDConstants.STRING, 0, 1) != -1) && (terminated == true))
					{
						Temp = Runner.Substring(Runner.IndexOf(STDConstants.STRING) + 1);
						slicer = Runner.Substring(Runner.IndexOf(STDConstants.STRING) + 1, Temp.IndexOf(STDConstants.STRING));
						Iterator0 = Temp.IndexOf(STDConstants.STRING) + 1;
						if (Temp.Substring(Iterator0, 1) != $"{STDConstants.SEPERATOR}") { terminated = false; continue; }
						Temp = null;
						Temp = Runner.Substring(Iterator0 + 3);
						Iterator1 = Temp.IndexOf(STDConstants.STRING);
						Front = Temp.Substring(0, Iterator1);
						if (Temp.Substring(Iterator1 + 1, 1) != $"{STDConstants.SEPERATOR}") { terminated = false; continue; }
						Back = MAIN.RemoveDefinedChars(Runner.Substring(Runner.LastIndexOf(STDConstants.SEPERATOR) + 1),
							new[] { STDConstants.SEPERATOR, STDConstants.STRING });
					}

					if (terminated == false)
					{
						Front = null;
						Back = null;
						slicer = null;
						throw new System.InvalidOperationException("Unterminated STD color string detected.");
					}
					else
					{
						STDFrontColor SS = ParseFrontColor(Front.Trim());
						STDBackColor SD = ParseBackColor(Back.Trim());
						if ((SS == STDFrontColor.INV) || (SD == STDBackColor.INV))
						{
							throw new System.InvalidOperationException(
								$"The STD Color was not parsed. Invalid colors detected: [{Front}] [{Back}]");
						}
						STD.Add(SS, SD, slicer);
						Front = null;
						Back = null;
						slicer = null;
					}
				} catch (System.InvalidOperationException EX) 
				{
					// For fatal parser errors , rethrow the particular exception.
					throw new InvalidOperationException("Could not deserialise the given data.\n", EX);
				}
				catch 
				{
                    // Parser error. In this case , add a new invalid item.
                    STD.AddInvalidItem(Runner);
                    continue;
                }
			}

			if (GivenVersion == false)
			{
				throw new InvalidOperationException("The STD Version block was malformed. " +
					"Please detect the error and fix it so as the data can be parsed again.");
			}
			_tmp = null;
			return STD;
		}

		/// <summary>
		/// Serialise from an STD Context the given data.
		/// </summary>
		/// <param name="Context">The STD Context to get data from.</param>
		/// <returns>The STD encoded data as a single <see cref="System.String"/>.</returns>
		/// <exception cref="AggregateException"></exception>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.String Serialize(STDContext Context)
		{
			System.String Result = null;
			System.String FR = null;
			System.String BK = null;
			foreach (STDLine STDL in Context)
			{
				if (STDL.Type == STDType.VersionBlock) 
				{
					Result += $"{STDL.Data}\n";
					continue;
				}
				if (STDL.Type == STDType.Comment)
				{
					Result += $"{STDConstants.COMMENT}{STDL.Data}\n";
                    continue;
                }
				if (STDL.Type == STDType.STDString)
				{
					FR = ParseFrontColorS(STDL.Colors.FrontColor);
					BK = ParseBackColorS(STDL.Colors.BackColor);
					if (FR == "") { throw new AggregateException("The Front Color string could not be parsed."); }
                    if (BK == "") { throw new AggregateException("The Back Color string could not be parsed."); }
                    Result += $"{STDConstants.STRING}{STDL.Data}{STDConstants.STRING}" +
						$"{STDConstants.SEPERATOR}{STDConstants.STRING}" +
						$"{FR}{STDConstants.STRING}{STDConstants.SEPERATOR}{STDConstants.STRING}" +
						$"{BK}{STDConstants.STRING}\n";
					FR = null;
					BK = null;
                    continue;
                }
			}
			return Result;
		}

		/*
		/// <summary>
		/// Displays the current data to the .NET running console.
		/// </summary>
		/// <param name="Context">The STD Context to display the data from.</param>
		public static void DisplayToConsole(STDContext Context)
		{
            STDLine STDL = new();
			for (System.Int32 I = 0; I < Context.ItemsCount - 1; I++)
			{
				if (STDL.Type == STDType.STDString)
				{
					ROOT.MAIN.WriteCustomColoredText(STDL.Data , 
						AsConsoleColorFrnt(STDL.colors.FrontColor) , AsConsoleColorBack(STDL.colors.BackColor));
					continue;
				}
			}
        }
		*/
	}

	/// <summary>
	/// The Write Mode to use when the <see cref="STDWriter"/> class is used.
	/// </summary>
    public enum STDWriteMode : System.Int32
    {
		/// <summary>
		/// Serialise and return the data as an new <see cref="STDContext"/> structure.
		/// </summary>
        AsContext = 2,
		/// <summary>
		/// Serialise and return the data as an new <see cref="System.String"/> class.
		/// </summary>
        AsString = 3,
		/// <summary>
		/// Serialise and emit the produced STD data to a new file.
		/// </summary>
        AsFile = 4,
		/// <summary>
		/// Serialise and return the data as a new <see cref="System.Text.StringBuilder"/> class.
		/// </summary>
        AsStringBuilder = 5
    }

    /// <summary>
    /// The STD Writer is another way to write directly either to a new STD context 
    /// or as a raw string.
    /// </summary>
	/// <remarks>This class cannot be inherited.</remarks>
    public sealed class STDWriter : System.IDisposable
	{
		private STDContext _context;
		private STDWriteMode _mode = STDWriteMode.AsContext;
		private System.IO.FileStream _stream = null;
		private System.Text.StringBuilder _sb = null;
		private STDLine _line = new();
		private System.Boolean _closed = true;

		/// <summary>
		/// Start a new instance of the <see cref="STDWriter"/> class. This constructor implies that the mode used is 
		/// the <see cref="STDWriteMode.AsContext"/> .
		/// </summary>
		public STDWriter() { _context = new STDContext(); _context.AddVersionBlock(); }

        /// <summary>
        /// Start a new instance of the <see cref="STDWriter"/> class. The mode that will be used is determined by the 
		/// <paramref name="mode"/> parameter.
        /// </summary>
        /// <param name="mode">The Write mode to use.</param>
        public STDWriter(STDWriteMode mode) { _mode = mode; InitWriter(); }

		private void InitWriter()
		{
			_context = new();
			if (_mode == STDWriteMode.AsFile) { _stream = default; }
			if (_mode == STDWriteMode.AsStringBuilder) { _sb = new(); }
			_context.AddVersionBlock();
		}

		/// <inheritdoc/>
		public void Dispose() 
		{ 
			_context.Dispose(); 
			if (_mode == STDWriteMode.AsFile) { _stream.Close(); _stream.Dispose(); }
			if (_mode == STDWriteMode.AsStringBuilder) { _sb.Clear(); _sb = null; }
			_line = default; //discard any data.
		}

		/// <summary>
		/// Returns the underlying STD Context that is being used by the writer.
		/// </summary>
		public STDContext Context  { get { return _context; } }

		/// <summary>
		/// Adds a new STD entry.
		/// </summary>
		/// <param name="Data">The STD <see cref="System.String"/> text to add.</param>
		public void AddSTDEntry(System.String Data) 
		{
            if (_closed == false)
            {
				throw new System.InvalidOperationException("Cannot add a new STD entry bacause it is not closed.\n" +
					"To close an STD Entry , use the Colors property so as to close it.");
            }
            _line.Data = Data;
			_line.Type = STDType.STDString;
			_closed = false;
		}

		/// <summary>
		/// The colors to set for a new STD Entry.
		/// </summary>
		public STDColors Colors 
		{ 
			set 
			{ 
				if (_closed)
				{
					throw new System.InvalidOperationException("A new STD Entry was not defined. Define one , then set it's colors.");
				}
				_line.Colors = value;
				_context.Add(_line.Colors.FrontColor, _line.Colors.BackColor, _line.Data);
				_closed = true;
			}
		}

		/// <summary>
		/// Append a new STD Comment.
		/// </summary>
		/// <param name="Comment">The Comment data to include too.</param>
		public void AddComment(System.String Comment) { _context.AddComment(Comment); }

        /// <summary>
        /// Gets a new instance of <see cref="System.Text.StringBuilder"/> with the serialised 
        /// STD data. Be noted , the class must be initialised with the <see cref="STDWriter(STDWriteMode)"/>
        /// constructor with a value of <see cref="STDWriteMode.AsStringBuilder"/> so that this can work.
        /// </summary>
        /// <exception cref="InvalidOperationException">The class was not initialised with the 
        /// <see cref="STDWriteMode.AsStringBuilder"/> value.</exception>
		/// <exception cref="AggregateException">Inherited from <see cref="STD.Serialize(STDContext)"/> . 
		/// See the documentation on that function so as to learn more about this exception. </exception>
        public System.Text.StringBuilder AsStringBuilder
		{
			get 
			{
				if (_mode != STDWriteMode.AsStringBuilder)
				{
					throw new InvalidOperationException("The instance was prepared for a different target. Use that method instead.");
				}
				_sb.AppendLine();
				_sb.Append(STD.Serialize(_context));
				return _sb;
			}
		}

        /// <summary>
        /// Gets the serialised data as a new <see cref="System.String"/>.
        /// Be noted , the class must be initialised with the <see cref="STDWriter(STDWriteMode)"/>
        /// constructor with a value of <see cref="STDWriteMode.AsString"/> so that this can work.
        /// </summary>
		/// <exception cref="InvalidOperationException">The class was not initialised with the 
        /// <see cref="STDWriteMode.AsString"/> value.</exception>
		/// <exception cref="AggregateException">Inherited from <see cref="STD.Serialize(STDContext)"/> . 
		/// See the documentation on that function so as to learn more about this exception. </exception>
        public System.String AsSingleString
		{
			get 
			{
                if (_mode != STDWriteMode.AsString)
                {
                    throw new InvalidOperationException("The instance was prepared for a different target. Use that method instead.");
                }
                System.String DI = null;
				DI += " \n";
				DI += STD.Serialize(_context);
				return DI;
			}
		}

        /// <summary>
		/// <para>
        /// Writes the emitted STD data to a new file , or if the <paramref name="OverwriteIfExists"/> parameter is set to 
        /// <see langword="true"/> , and the file exists , it is overwrriten. </para>
		/// <para>
		/// Be noted , the class must be initialised with the <see cref="STDWriter(STDWriteMode)"/>
        /// constructor with a value of <see cref="STDWriteMode.AsFile"/> so that this can work.
		/// </para>
        /// </summary>
        /// <param name="PathToSave"></param>
        /// <param name="OverwriteIfExists"></param>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.AggregateException">Inherited from <see cref="STD.Serialize(STDContext)"/> . 
        /// See the documentation on that function so as to learn more about this exception , or an file stream could
		/// not be opened to complete this operation.</exception>
		/// <exception cref="InvalidOperationException">The class was not initialised with the 
        /// <see cref="STDWriteMode.AsFile"/> value.</exception>
        public void AsANewFile(System.String PathToSave ,System.Boolean OverwriteIfExists = false)
		{
            if (_mode != STDWriteMode.AsFile)
            {
                throw new InvalidOperationException("The instance was prepared for a different target. Use that method instead.");
            }
            if (MAIN.FileExists(PathToSave) && (OverwriteIfExists == false)) 
			{
				throw new System.IO.IOException($"Could not save the file {PathToSave} because the OverwriteIfExists was \n" +
					$"not set or set to \'false\'. To allow file overwriting , set the OverwriteIfExists parameter to \'true\'.");
			} else if (MAIN.FileExists(PathToSave))
			{
				_stream = MAIN.ClearAndWriteAFile(PathToSave);
			} else { _stream = MAIN.CreateANewFile(PathToSave); }
			if (_stream == null) 
			{
				throw new System.AggregateException("A new file stream could not be opened. An unexpected error occured.");
			}
			MAIN.AppendNewContentsToFile($"\n{STD.Serialize(_context)}" , _stream);
			_stream.Flush();
		}

	}

    /// <summary>
    /// The STD Reader is another way to get the STD serialised data back to a new STD Context.
    /// </summary>
    /// <remarks>This class cannot be inherited.</remarks>
    public sealed class STDReader : System.IDisposable
	{
		private STDContext _context;
		private System.String _serialised = null;

		/// <summary>
		/// Get the data from an simple <see cref="System.String"/> .
		/// </summary>
		/// <param name="STDText">The <see cref="System.String"/> class that contains the STD data.</param>
		public STDReader(System.String STDText) { _serialised = STDText; }

        /// <summary>
        /// Get the data from an instantiated <see cref="System.Text.StringBuilder"/> class.
        /// </summary>
        /// <param name="builder">The <see cref="System.Text.StringBuilder"/> class that contains the STD data.</param>
        public STDReader(System.Text.StringBuilder builder) { _serialised = builder.ToString(); }

        /// <summary>
        /// Get the data from an intialised <see cref="System.IO.Stream"/> .
        /// </summary>
        /// <param name="Stream">The <see cref="System.IO.Stream"/> class that contains the STD data.</param>
        public STDReader(System.IO.Stream Stream) 
		{
			System.IO.StreamReader dk = new(Stream);
			_serialised = dk.ReadToEnd();
			dk.Close();
			dk.Dispose();
		}

		/// <summary>
		/// Calling this constructor always throws an <see cref="System.InvalidOperationException"/> exception.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public STDReader() { throw new InvalidOperationException("The class must be initialised with at least one of the \n" +
            "parameterized constructor variants provided."); }

		/// <inheritdoc/>
		public void Dispose() 
		{
			_context.Dispose();
			_serialised = null;
		}

		/// <summary>
		/// Returns the deserialised data given by the constructors.
		/// </summary>
		public STDContext Context 
		{ 
			get 
			{
				_context = STD.Deserialize(_serialised);
				return _context; 
			}
		}
	}

	/// <summary>
	/// This class contains the internal console implementation extensions , which some of them are exposed publicly.
	/// </summary>
	public static class ConsoleExtensions
	{
        internal enum ConsoleHandleOptions : System.UInt32
        {
            Input = 0xFFFFFFF6,
            Output = 0xFFFFFFF5,
            Error = 0xFFFFFFF4
        }

		// This value indicates whether the console is detached , and therefore 
		// it notifies the console functions to 'not' actually call the console code.
		internal static System.Boolean Detached = false;

		// An internal buffer for the Title commands.
		internal static System.String T = T2;
        internal const System.String T2 = "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0";

		/// <summary>
		/// Define the Buffer Size when using the function <see cref="ROOT.MAIN.ReadConsoleText(ConsoleReadBufferOptions)"/> .
		/// </summary>
        public enum ConsoleReadBufferOptions : System.Int32
        {
			/// <summary>
			/// Buffer size is set to 1024.
			/// </summary>
            Small = 1024,
            /// <summary>
            /// Buffer size is set to 2048.
            /// </summary>
            Default = 2048,
            /// <summary>
            /// Buffer size is set to 3072.
            /// </summary>
            Large = 3072,
            /// <summary>
            /// Buffer size is set to 4096.
            /// </summary>
            VeryLarge = 4096,
            /// <summary>
            /// Buffer size is set to 8192.
            /// </summary>
            ExtravagantlyLarge = 8192
        }

        /// <summary>
        /// Gets the underlying KERNEL32 handle which this implementation uses to write any kind of data to console.
        /// </summary>
        /// <returns>A new <see cref="System.IntPtr"/> handle which is the handle for writing data to the console.</returns>
        [System.Security.SecurityCritical]
        public static System.IntPtr GetOutputHandle()
		{
			if (ConsoleInterop.OutputHandle == System.IntPtr.Zero) 
			{
				ConsoleInterop.OutputHandle = ConsoleInterop.GetConsoleStream(ConsoleHandleOptions.Output);
				return ConsoleInterop.OutputHandle;
			} else { return ConsoleInterop.OutputHandle; }
		}

        /// <summary>
        /// Gets the underlying KERNEL32 handle which this implementation uses to read from the console.
        /// </summary>
        /// <returns>A new <see cref="System.IntPtr"/> handle which is the handle for reading data 
		/// from the console.</returns>
        [System.Security.SecurityCritical]
        public static System.IntPtr GetInputHandle()
        {
            if (ConsoleInterop.InputHandle == System.IntPtr.Zero)
            {
                ConsoleInterop.InputHandle = ConsoleInterop.GetConsoleStream(ConsoleHandleOptions.Output);
                return ConsoleInterop.InputHandle;
            }
            else { return ConsoleInterop.InputHandle; }
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[System.Security.SecurityCritical]
		internal static void InitIfNotInitOut()
		{
			if (ConsoleInterop.OutputHandle == System.IntPtr.Zero)
			{
				ConsoleInterop.OutputHandle = GetOutputHandle();
			}
			return;
		}

		/// <summary>
		/// Globally set or get the console foreground color. This property is used for the exported MDCFR functions , 
		/// and this is equivalent to <see cref="System.Console.ForegroundColor"/> property.
		/// </summary>
		public static System.ConsoleColor ForegroundColor
		{
            [System.Security.SecurityCritical]
            get 
			{
                if (ROOT.ConsoleExtensions.Detached == true) { return System.ConsoleColor.Gray; }
                InitIfNotInitOut();
				ConsoleInterop.GetBufferInfo(ConsoleInterop.OutputHandle, out CONSOLE_SCREEN_BUFFER_INFO CSBI);
				return ColorAttributeToConsoleColor((System.Int16)(CSBI.wAttributes & (System.Int16)ConsoleControlChars.ForegroundMask));
            }
			[System.Security.SecurityCritical]
			set 
			{
				InitIfNotInitOut();
				SetForeColor(value);
			}
		}

        /// <summary>
        /// Globally set or get the console background color. This property is used for the exported MDCFR functions , 
        /// and this is equivalent to <see cref="System.Console.BackgroundColor"/> property.
        /// </summary>
        public static System.ConsoleColor BackgroundColor
		{
			[System.Security.SecurityCritical]
			get 
			{
                if (ROOT.ConsoleExtensions.Detached == true) { return System.ConsoleColor.Black; }
                InitIfNotInitOut();
                ConsoleInterop.GetBufferInfo(ConsoleInterop.OutputHandle, out CONSOLE_SCREEN_BUFFER_INFO CSBI);
                return ColorAttributeToConsoleColor((System.Int16)(CSBI.wAttributes & (System.Int16)ConsoleControlChars.BackgroundMask));
            }
            [System.Security.SecurityCritical]
			set 
			{
				InitIfNotInitOut();
				SetBackColor(value);
			}
        }

		/// <summary>
		/// Get or Set the Console's Output encoding as an <see cref="System.Text.Encoding"/> class.
		/// </summary>
		/// <exception cref="AggregateException">
		/// Occurs when the Code Page defined to the 
		/// console does not exist as an <see cref="System.Text.Encoding"/> class.</exception>
		/// <exception cref="InvalidOperationException">
		/// Occurs when the specified Code Page is invalid for the console.</exception>
		public static System.Text.Encoding OutputEncoding
		{
            [System.Security.SecurityCritical]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
			{
				System.UInt32 CP = ConsoleInterop.GetOutputEnc();
                if (CP == 0) 
				{
					throw new AggregateException("Error occured while getting the current code page!!!");
				}
				System.Text.Encoding TI = null;
				TI = System.Text.CodePagesEncodingProvider.Instance.GetEncoding((System.Int32)CP);
                if (TI == null)
				{
					try
					{
						TI = System.Text.Encoding.GetEncoding((System.Int32) CP);
						return TI;
					} catch (System.Exception EX)
					{
						throw new AggregateException($"Could not get the codepage set to the console: {CP} ." , EX);
					}
				} else { return TI; }
			}

            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
			set 
			{
				System.UInt32 CP = (System.UInt32) value.CodePage;
				if (ConsoleInterop.SetOutputEnc(CP) == 0) 
				{
					throw new InvalidOperationException("Cannot apply the specific code page as a console output encoding. \n" +
						$"Code Page Identifier: {CP} \n" +
						$"Code Page Name: {value.BodyName} \n" +
						$"Code Page Web Name: {value.WebName} \n" +
						$"Code Page Windows Name: {value.WindowsCodePage}");
				}
			}

        }

        /// <summary>
        /// Get or Set the Console's Input encoding as an <see cref="System.Text.Encoding"/> class.
        /// </summary>
		/// <exception cref="AggregateException">
		/// Occurs when the Code Page defined to the 
		/// console does not exist as an <see cref="System.Text.Encoding"/> class.</exception>
		/// <exception cref="InvalidOperationException">
		/// Occurs when the specified Code Page is invalid for the console.</exception>
        public static System.Text.Encoding InputEncoding
        {
            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                System.UInt32 CP = ConsoleInterop.GetInputEnc();
                if (CP == 0)
                {
                    throw new AggregateException("Error occured while getting the current code page!!!");
                }
                System.Text.Encoding TI = null;
                TI = System.Text.CodePagesEncodingProvider.Instance.GetEncoding((System.Int32)CP);
                if (TI == null)
                {
                    try
                    {
                        TI = System.Text.Encoding.GetEncoding((System.Int32)CP);
                        return TI;
                    }
                    catch (System.Exception EX)
                    {
                        throw new AggregateException($"Could not get the codepage set to the console: {CP} .", EX);
                    }
                }
                else { return TI; }
            }

            [System.Security.SecurityCritical]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                System.UInt32 CP = (System.UInt32)value.CodePage;
                if (ConsoleInterop.SetInputEnc(CP) == 0)
                {
                    throw new InvalidOperationException("Cannot apply the specific code page as a console input encoding. \n" +
                        $"Code Page Identifier: {CP} \n" +
                        $"Code Page Name: {value.BodyName} \n" +
                        $"Code Page Web Name: {value.WebName} \n" +
                        $"Code Page Windows Name: {value.WindowsCodePage}");
                }
            }

        }

		/// <summary>
		/// Get or Set the current console title. This property is equivalent to <see cref="System.Console.Title"/> property.
		/// </summary>
		public static System.String Title
		{
			[System.Security.SecurityCritical]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				ConsoleInterop.GetTitle(T, T.Length);
				System.String I = T;
				T = T2;
				return MAIN.RemoveDefinedChars(I, '\0');
			}
			[System.Security.SecurityCritical]
			set { ConsoleInterop.SetTitle(value); }
        }

		/// <summary>
		/// Gets the original title , when the application attached to the console.
		/// </summary>
		public static System.String OriginalTitle
		{
			[System.Security.SecurityCritical]
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get 
			{
				ConsoleInterop.OriginalTitle(T, T.Length);
				System.String I = T;
				T = T2;
				return MAIN.RemoveDefinedChars(I, '\0');
			}
		}

		// Global Windows C structure defining the coordinates of an window.
		// Interpreted as System.Int16 points.
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;

            internal short Y;
        }

        // Global Windows C structure defining the size of an window.
        // Interpreted as System.Int16 points.
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            internal short Left;

            internal short Top;

            internal short Right;

            internal short Bottom;
        }

		// The Console Info for this console session. This table is filled by specific functions only.
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_SCREEN_BUFFER_INFO
        {
            internal COORD dwSize;

            internal COORD dwCursorPosition;

            internal short wAttributes;

            internal SMALL_RECT srWindow;

            internal COORD dwMaximumWindowSize;
        }

		internal static void SetForeColor(System.ConsoleColor Color) 
		{
            new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.SafeTopLevelWindows).Demand();
            CONSOLE_SCREEN_BUFFER_INFO INF = GetCBufferInfo(true, out System.Boolean SU);
			System.Int16 attrs = INF.wAttributes;
			attrs = (short)(attrs & -16);
            attrs = (short)((ushort)attrs | (ushort) ConsoleColorToColorAttribute(Color , false));
			ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, attrs);
        }

		internal static void SetBackColor(System.ConsoleColor Color)
		{
            new System.Security.Permissions.UIPermission(System.Security.Permissions.UIPermissionWindow.SafeTopLevelWindows).Demand();
            CONSOLE_SCREEN_BUFFER_INFO INF = GetCBufferInfo(true, out System.Boolean SU);
            System.Int16 attrs = INF.wAttributes;
            attrs = (short)(attrs & -241);
            attrs = (short)((ushort)attrs | (ushort)ConsoleColorToColorAttribute(Color, true));
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, attrs);
        }

        [System.Security.SecuritySafeCritical]
        private static CONSOLE_SCREEN_BUFFER_INFO GetCBufferInfo(bool throwOnNoConsole, out bool succeeded)
        {
            succeeded = false;
            IntPtr consoleOutputHandle = ConsoleInterop.OutputHandle;
            if (consoleOutputHandle == System.IntPtr.Zero)
            {
                if (!throwOnNoConsole)
                {
                    return default(CONSOLE_SCREEN_BUFFER_INFO);
                }
                throw new System.IO.IOException("There is not any Console Spawned! ");
            }
            if (!ConsoleInterop.GetBufferInfo(consoleOutputHandle, out var lpConsoleScreenBufferInfo))
            {
                System.Boolean consoleScreenBufferInfo = ConsoleInterop.GetBufferInfo(consoleOutputHandle, out lpConsoleScreenBufferInfo);
                if (!consoleScreenBufferInfo)
                {
                    consoleScreenBufferInfo = ConsoleInterop.GetBufferInfo(consoleOutputHandle, out lpConsoleScreenBufferInfo);
                }
                if (!consoleScreenBufferInfo)
                {
                    System.Int32 lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == 6 && !throwOnNoConsole)
                    {
                        return default(CONSOLE_SCREEN_BUFFER_INFO);
                    }
                    throw new System.AggregateException($"Win32 Exception detected. HRESULT is {lastWin32Error} .");
                }
            }
            return lpConsoleScreenBufferInfo;
        }

        internal enum ConsoleControlChars : System.Int32
        {
            FOREGROUND_BLUE = 0x0001, // text color contains blue.
            FOREGROUND_GREEN = 0x0002, // text color contains green.
            FOREGROUND_RED = 0x0004, // text color contains red.
            FOREGROUND_INTENSITY = 0x0008, // text color is intensified.
            BACKGROUND_BLUE = 0x0010, // background color contains blue.
            BACKGROUND_GREEN = 0x0020, // background color contains green.
            BACKGROUND_RED = 0x0040, // background color contains red.
            BACKGROUND_INTENSITY = 0x0080, // background color is intensified.
            COMMON_LVB_LEADING_BYTE = 0x0100, // Leading Byte of DBCS
            COMMON_LVB_TRAILING_BYTE = 0x0200, // Trailing Byte of DBCS
            COMMON_LVB_GRID_HORIZONTAL = 0x0400, // DBCS: Grid attribute: top horizontal.
            COMMON_LVB_GRID_LVERTICAL = 0x0800, // DBCS: Grid attribute: left vertical.
            COMMON_LVB_GRID_RVERTICAL = 0x1000, // DBCS: Grid attribute: right vertical.
            COMMON_LVB_REVERSE_VIDEO = 0x4000, // DBCS: Reverse fore/back ground attribute.
            COMMON_LVB_UNDERSCORE = 0x8000, // DBCS: Underscore.
            ForegroundMask = 0xF,
            BackgroundMask = 0xF0,
            ColorMask = 0xFF
        }

        [System.Security.SecurityCritical]
        internal static ConsoleControlChars ConsoleColorToColorAttribute(ConsoleColor color, bool isBackground)
        {
            if (((uint)color & 0xFFFFFFF0u) != 0)
            {
                throw new ArgumentException($"The Console Color specified , {color} , is invalid.");
            }
            ConsoleControlChars color2 = (ConsoleControlChars)color;
            if (isBackground)
            {
                color2 = (ConsoleControlChars)((System.Int32)color2 << 4);
            }
            return color2;
        }

        [System.Security.SecurityCritical]
        internal static ConsoleColor ColorAttributeToConsoleColor(System.Int16 c)
        {
            if ((c & (System.Int16) ConsoleControlChars.BackgroundMask) != 0)
            {
                c = (System.Int16) ((System.Int32)c >> 4);
            }
            return (ConsoleColor)c;
        }
    
		/// <summary>
		/// Revert the current implementation's back to default console colors , when it is initiated.
		/// </summary>
		public static void ToDefaultColors()
		{
			InitIfNotInitOut();
            ConsoleControlChars F = ConsoleColorToColorAttribute(System.ConsoleColor.Black, true);
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, (System.Int16) F);
            F = ConsoleColorToColorAttribute(System.ConsoleColor.Gray, false);
            ConsoleInterop.DefineNewAttributes(ConsoleInterop.OutputHandle, (System.Int16) F);
        }
	}

}

namespace ExternalHashCaculators
{
    //A Collection Namespace for computing hash values from external generated libraries.

    /// <summary>
    /// <para> xxHash is a really fast non-cryptographic hash digest. </para> 
	/// <para>
	/// This is a wrapper for the unmanaged library.
    /// Note that you can run this only on AMD64 machines only. </para>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class XXHash
	{
        // xxHash Hash caculator system.
        // It is a fast , non-cryptographic algorithm , as described from Cyan4973.
        // It is also used by the zstd archiving protocol , so as to check and the file integrity.
        // The version imported here is 0.8.1.

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.Boolean _CheckDLLVer()
		{
			if (ROOT.MAIN.OSProcessorArchitecture() != "AMD64") { return false; }
			if (XXHASHMETHODS.XXH_versionNumber() < 00801) { return false; } else { return true; }
		}

        [System.Security.SuppressUnmanagedCodeSecurity]
        private sealed class XXHASHMETHODS
		{
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.XXHash)]
			[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
			public static extern System.Int32 XXH32(System.Byte[] buffer ,
			System.Int32 size ,System.Int32 seed = 0);
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.XXHash)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 XXH64(System.Byte[] buffer ,
			System.Int32 size ,System.Int32 seed = 0);
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.XXHash)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 XXH_versionNumber();
		}

        /// <summary>
        /// Computes a file hash by using the XXH32 function.
        /// </summary>
        /// <param name="FileStream">The alive <see cref="System.IO.Stream"/> object from which the data will be collected.</param>
        /// <returns>A caculated xxHash32 value written as an hexadecimal <see cref="System.String"/>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_32(System.IO.Stream FileStream)
		{
			if (!(_CheckDLLVer())) {return "Error";}
			if (FileStream.Length < 20) {return "Error";}
			System.Byte[] FBM = new System.Byte[FileStream.Length];
			try
			{
				FileStream.Read(FBM , 0 , FBM.Length);
			}
			catch (System.Exception)
			{
				FBM = null;
				return "Error";
			}
			System.Int32 EFR = XXHASHMETHODS.XXH32(FBM , FBM.Length ,0);
			FBM = null;
			return EFR.ToString("x2");
		}

        /// <summary>
        /// Computes a <see cref="System.Byte"/>[] array by using the XXH32 function.
        /// </summary>
        /// <param name="Data">The <see cref="System.Byte"/>[] array to get the hash from.</param>
        /// <param name="Length">The length of the <paramref name="Data"/> 
        /// <see cref="System.Byte"/>[] .</param>
        /// <param name="Seed">The Seed to use for calculating the hash. Can be 0.</param>
        /// <returns>A caculated xxHash32 value written as an hexadecimal <see cref="System.String"/>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_32(System.Byte[] Data , System.Int32 Length , System.Int32 Seed)
		{
			if (!(_CheckDLLVer())) { return "Error"; }
            System.Int32 EFR = XXHASHMETHODS.XXH32(Data , Length, Seed);
            return EFR.ToString("x2");
        }

        /// <summary>
        /// Computes a <see cref="System.Byte"/>[] array by using the XXH64 function.
        /// </summary>
        /// <param name="Data">The <see cref="System.Byte"/>[] array to get the hash from.</param>
        /// <param name="Length">The length of the <paramref name="Data"/> 
        /// <see cref="System.Byte"/>[] .</param>
        /// <param name="Seed">The Seed to use for calculating the hash. Can be 0.</param>
        /// <returns>A caculated xxHash64 value written as an hexadecimal <see cref="System.String"/>.</returns>
		/// <remarks>This function performs well only on AMD64 machines; it's performance is degraded when working on IA32.</remarks>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_64(System.Byte[] Data, System.Int32 Length, System.Int32 Seed)
        {
            if (!(_CheckDLLVer())) { return "Error"; }
            System.Int32 EFR = XXHASHMETHODS.XXH64(Data, Length, Seed);
            return EFR.ToString("x2");
        }

        /// <summary>
        /// Computes a file hash by using the XXH64 function.
        /// </summary>
        /// <param name="FileStream">The alive <see cref="System.IO.Stream"/> object from which the data will be collected.</param>
        /// <returns>A caculated xxHash64 value written as an hexadecimal <see cref="System.String"/>.</returns>
        /// <remarks>This function performs well only on AMD64 machines; it's performance is degraded when working on IA32.</remarks>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String xxHash_64(System.IO.Stream FileStream)
		{
			if (!(_CheckDLLVer())) {return "Error";}
			if (FileStream.Length < 20) {return "Error";}
			System.Byte[] FBM = new System.Byte[FileStream.Length];
			try
			{
				FileStream.Read(FBM , 0 , FBM.Length);
			}
			catch (System.Exception)
			{
				FBM = null;
				return "Error";
			}
			System.Int32 EFR = XXHASHMETHODS.XXH64(FBM , FBM.Length ,0);
			FBM = null;
			return EFR.ToString("x2");
		}
	}

}

[System.Security.SecurityCritical]
[System.Security.SuppressUnmanagedCodeSecurity]
internal static class ConsoleInterop
{
    [DllImport(Interop.Libraries.Kernel32 , EntryPoint = "FreeConsole", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 DetachConsole();

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "GetConsoleScreenBufferInfo", CallingConvention = CallingConvention.Winapi , SetLastError = true)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Boolean GetBufferInfo(System.IntPtr ConsoleOutputHandle, 
		out ROOT.ConsoleExtensions.CONSOLE_SCREEN_BUFFER_INFO ConsoleScreenBufferInfo);

    internal static volatile System.IntPtr InputHandle = System.IntPtr.Zero;
	internal static volatile System.IntPtr OutputHandle = System.IntPtr.Zero;

    [DllImport(Interop.Libraries.Kernel32 , EntryPoint = "AttachConsole", CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 AttachToConsole(System.Int32 PID);

	[DllImport(Interop.Libraries.Kernel32 , EntryPoint = "GetStdHandle" , CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.IntPtr GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions Stream);

	[DllImport(Interop.Libraries.Kernel32, EntryPoint = "WriteConsoleW" , CallingConvention = CallingConvention.Winapi)]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 WriteToConsoleUnmanaged(System.IntPtr Handle, 
		[MarshalAs(UnmanagedType.LPWStr)] System.String Data,
		System.Int32 NChars, [OptionalAttribute] out System.Int32 CharsWritten, System.IntPtr MustBeNull);

	[DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi , EntryPoint = "GetConsoleOriginalTitleW")]
	[System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
		Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
		UI = true)]
	internal static extern System.Int32 OriginalTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title , System.Int32 Titlesize = 27500);

    [DllImport(Interop.Libraries.Kernel32, BestFitMapping = true , CharSet = CharSet.Auto , CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleTitleW")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.Int32 SetTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleTitleW")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
	internal static extern System.Int32 GetTitle([MarshalAs(UnmanagedType.LPTStr)] System.String Title , System.Int32 Titlesize = 27500);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "SetConsoleOutputCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
	internal static extern System.Int32 SetOutputEnc(System.UInt32 OutputEnc);

	[DllImport(Interop.Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi , EntryPoint = "SetConsoleCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
	internal static extern System.Int32 SetInputEnc(System.UInt32 InputEnc);

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.UInt32 GetInputEnc();

    [DllImport(Interop.Libraries.Kernel32, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetConsoleOutputCP")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern System.UInt32 GetOutputEnc();

    [DllImport(Interop.Libraries.Kernel32, EntryPoint = "ReadConsoleW", CallingConvention = CallingConvention.Winapi)]
	[System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
		Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
		UI = true)]
	internal static extern System.Int32 ReadFromConsoleUnmanaged(System.IntPtr Handle ,
		System.Byte[] Buffer , System.Int32 NumberOfCharsToRead , 
		out System.UInt32 NumberOfCharsRead , System.IntPtr MustBeNull);

	[System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert ,
		Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt ,
		UI = true)]
	private static void SetOutputHandle() { OutputHandle = GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions.Output); }

    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    private static void SetInputHandle() { InputHandle = GetConsoleStream(ROOT.ConsoleExtensions.ConsoleHandleOptions.Input); }

	[DllImport(Interop.Libraries.Kernel32 , CallingConvention = CallingConvention.Winapi , EntryPoint = "SetConsoleTextAttribute")]
    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    internal static extern void DefineNewAttributes(System.IntPtr Handle, System.Int16 Attributes);

	[System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt , 
		UI = true)]
    public static System.Boolean WriteToConsole(System.Char[] data)
	{
		System.String DI = null;
		for (System.Int32 I = 0; I < data.Length; I++) { DI += data[I]; }
        return WriteToConsole(DI);
    }

	public static System.Boolean WriteToConsole(System.String data) 
	{
        if (ROOT.ConsoleExtensions.Detached == true) { return true; }
        if (OutputHandle == System.IntPtr.Zero) { SetOutputHandle(); }
        if (WriteToConsoleUnmanaged(OutputHandle, data,
            data.Length, out System.Int32 CHARS, System.IntPtr.Zero) != 0)
        { return true; } else { return false; }
    }

    [System.Security.Permissions.HostProtection(Action = System.Security.Permissions.SecurityAction.Assert,
        Resources = System.Security.Permissions.HostProtectionResource.SelfAffectingProcessMgmt,
        UI = true)]
    public static System.String ReadFromConsole(ROOT.ConsoleExtensions.ConsoleReadBufferOptions BufSize)
	{
        if (ROOT.ConsoleExtensions.Detached == true) { return ""; }
        if (InputHandle == System.IntPtr.Zero) { SetInputHandle(); }
		System.Byte[] RF = new System.Byte[(System.Int32) BufSize];
		if (ReadFromConsoleUnmanaged(InputHandle, RF , (System.Int32) BufSize, 
			out System.UInt32 ED, System.IntPtr.Zero) == 0) { return "Error"; }
		System.String Result = null;
		for (System.Int32 I = 0; I < ED; I++) { if (
				(RF[I] != (System.Byte) '\0') && (RF[I] != (System.Byte) '\r') 
				&& (RF[I] != (System.Byte) '\n')) { Result += (System.Char) RF[I]; } }
		return Result;
	}
}

namespace ExternalArchivingMethods
{
    // A Collection Namespace for making archives outside Microsoft's managed code.

    /// <summary>
    /// Zstandard archiving compression level.
    /// </summary>
    public enum ZSTDCMPLevel : System.Int32
    {
        /// <summary>
        /// Fast compression.
        /// </summary>
        Fast = 1,
        /// <summary>
        /// Fast compression , but compresses slightly better than <see cref="ZSTDCMPLevel.Fast"/>.
        /// </summary>
        Fast2 = 2,
        /// <summary>
        /// Good compression.
        /// </summary>
        Efficient = 3,
        /// <summary>
        /// A bit better from the <see cref="Efficient"/> compression level.
        /// </summary>
        Lazy = 4,
        /// <summary>
        /// An balanced compression level. This one is the most popular option.
        /// </summary>
        Lazy2 = 5,
        /// <summary>
        /// The files are compressed about an estimated 62-71% ratio.
        /// </summary>
        LazyOptimized = 6,
        /// <summary>
        /// The files are compressed about an estimated 71-80% ratio.
        /// </summary>
        Optimal = 7,
        /// <summary>
        /// The files are compressed about an estimated 80-85% ratio.
        /// </summary>
        Ultra = 8,
        /// <summary>
        /// The Zstandard algorithm will consume as much as possible resources to compress the target as much as possible.
        /// </summary>
        FullCompressPower = 9,
    }

    /// <summary>
    /// <para> zstd is a fast compression algorithm maintained by Facebook. </para> 
	/// <para>
	/// This is a wrapper for the unmanaged library.
    /// Note that you can run this only on AMD64 machines only. </para>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class ZstandardArchives
	{
        // Yes , this patch adds (Patch Version 1.5.4.0) the Zstandard Archive format.
        // The runtime algorithm is built from the GitHub tree , version 1.5.2.0.
        // This has the following advantages: 
        //    1. The Dynamic-Link Library for the archive format is native C++.
        //    2. This format is very efficient. It can compress and decompress data very fast.
        //    3. The C algorithm that is comprised from is one of the most fast programming languages.
        //    4. Note that you cannot run earlier versions than 1.5.2.0.
        // NOTICE: the zstd.dll bundled with my library is being built by me.
        // Because actually this API calls the library via unmanaged way (Not very safe)
        // and requires the DLL path , use only updates which are either came from GitHub or other source that
        // is reliable. However , it is still very safe and stable , of course.

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        private static System.Boolean _CheckDLLVer()
		{
			if (ROOT.MAIN.OSProcessorArchitecture() != "AMD64") { return false; }
			if (ZSTD.ZSTD_versionNumber() < 10502) { return false; } else  { return true; }
		}

        [System.Security.SuppressUnmanagedCodeSecurity]
        private sealed class ZSTD
		{
			// Proper API Calls defined in this class. DO NOT Modify.
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_compress(System.Byte[] dst ,System.Int32 dstCapacity , 
			System.Byte[] src ,System.Int32 srcCapacity ,ZSTDCMPLevel compressionLevel);
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_decompress(System.Byte[] dst ,System.Int32 dstCapacity , 
			System.Byte[] src ,System.Int32 srcSize);
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int64 ZSTD_getFrameContentSize(System.Byte[] src ,System.Int32 srcSize);
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_isError(System.Int32 code);
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_findFrameCompressedSize(System.Byte[] src ,System.Int32 srcSize);
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_defaultCLevel();
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_minCLevel();
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_maxCLevel();
			
			[System.Runtime.InteropServices.DllImport(Interop.Libraries.Zstd)]
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            public static extern System.Int32 ZSTD_versionNumber();
			
		}

        /// <summary>
        /// Compress an alive <see cref="System.IO.FileStream"/> object that contains decompressed data and store the 
        /// compressed data to another and empty <see cref="System.IO.FileStream"/> object.
        /// </summary>
        /// <param name="InputFile">The alive stream object containing the data to compress.</param>
        /// <param name="ArchiveStream">The alive output stream which will contain the compressed data.</param>
        /// <param name="CmpLevel">The compression level to apply.</param>
        /// <returns><c>true</c> if the compression was succeeded; otherwise, <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean CompressAsFileStreams (System.IO.FileStream InputFile , 
		System.IO.FileStream ArchiveStream, ZSTDCMPLevel CmpLevel)
		{
			// Start first by checking the DLL version.
			if (!(_CheckDLLVer())) {return false;}
			// Check the filestreams.
			// The Input file must be more than 32 bytes (Just an average measure) , 
			// and the Output file (Result Archive) must be empty.
			if (InputFile.Length < 32) {return false;}
			if (ArchiveStream.Length > 0) {return false;}
			// Next step: Read all the file contents.
			// Initialise a new System.Byte[] object.
			System.Byte[] FSI = new System.Byte[InputFile.Length];
			try
			{
				InputFile.Read(FSI , 0 , System.Convert.ToInt32(InputFile.Length));
			}
			catch (System.Exception)
			{
				// Any written contents to the buffer must be disposed.
				FSI = null;
				// Exit because the flushing was not happened sucessfully.
				return false;
			}
			// After the file was read , make the buffer Byte[] that 
			// will host the archive contents (compressed contents).
			// Becuase we cannot know from now which the compressed stream size is ,
			// a good strategy is to initialise a Byte[] same as the file one's.
			System.Byte[] FSO = new System.Byte[InputFile.Length];
			// Now , everything are ready to invoke the compression algorithm.
			// In parallel , set the compressed size as a variable , becuase it gonna be
			// needed to write the archived contents back to the other stream.
			System.Int32 BUFF = ZSTD.ZSTD_compress(FSO , FSO.Length , FSI , FSI.Length , CmpLevel);
			// Check in the meantime if the compression happened sucessfully. 
			// Test that by checking if the final buffer is less than of FSI object,
			if (BUFF >= FSI.Length)
			{
				FSI = null;
				FSO = null;
				return false;
			}
			else 
			{
				FSI = null;
			}
			// Now write the data back to the archived file.
			// Here the compressed value is required; otherwise , we have an incorrect archive.
			try
			{
				ArchiveStream.Write(FSO , 0 , BUFF);
			}
			catch (System.Exception)
			{
				// In any case that the data could not be written , give up again.
				return false;
			}
			finally
			{
				// Clear now the Archive buffer too , because we have done here sucessfully. 
				FSO = null;
			}
			// Sucessfull Operation.
			return true;
		}

        /// <summary>
        /// Decompress from an alive <see cref="System.IO.FileStream"/> object that contains the compressed data
        /// to another <see cref="System.IO.FileStream"/> object which the decompressed data will be stored.
        /// </summary>
        /// <param name="ArchiveFileStream">The stream object that contains the compressed data.</param>
        /// <param name="DecompressedFileStream">The output file stream to save the data to.</param>
        /// <returns><c>true</c> if the decompression was succeeded; otherwise, <c>false</c>.</returns>
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Boolean DecompressFileStreams (System.IO.FileStream ArchiveFileStream , System.IO.FileStream DecompressedFileStream)
		{
			// Start first by checking the DLL version.
			if (!(_CheckDLLVer())) {return false;}
			// Check whether if the Archive is empty or not and check additionally whether the Output File Is empty.
			// A valid ZSTD archive header is around 62 bytes , adding more to be insured that it is okay.
			if (ArchiveFileStream.Length < 68) {return false;}
			if (DecompressedFileStream.Length > 0) {return false;}
			// Next step: Read all the file contents.
			// Initialise a new System.Byte[] object.
			System.Byte[] FSI = new System.Byte[ArchiveFileStream.Length];
			// Now pass the archive to the buffer.
			// It is controlled whether it was done sucessfully at any case.
			try
			{
				ArchiveFileStream.Read(FSI , 0 , System.Convert.ToInt32(ArchiveFileStream.Length));
			}
			catch (System.Exception)
			{
				// If any error was found , close this instance and clear the buffer.
				FSI = null;
				return false;
			}
			// Here we cannot predict the Byte[] length of the output File.
			// We must predict it.
			// The ZSTD_getframeContentSize will help to predict that.
			System.Int32 BUFFOut = System.Convert.ToInt32(ZSTD.ZSTD_getFrameContentSize(FSI , FSI.Length));
			// The below procedure will get the actual file buffer. This will be used so as to initiate the output file buffer.
			System.Int32 BUFF = ZSTD.ZSTD_findFrameCompressedSize(FSI , FSI.Length);
			// Check for any errors / code inefficiencies. 
			if ((ZSTD.ZSTD_isError(BUFF) == 1) || (ZSTD.ZSTD_isError(BUFFOut) == 1))
			{
				FSI = null;
				return false;
			}
			// After this prediction , we can initialise the output file buffer.
			// Add some more bytes , will help it to the decompression process (*.*).
			System.Byte[] FSO = new System.Byte[BUFFOut + 12];
			// Decompression is ready. Call the procedure.
			System.Int32 BUFF1 = ZSTD.ZSTD_decompress(FSO , BUFFOut , FSI , BUFF);
			// Write the compressed data to the output file.
			try
			{
				DecompressedFileStream.Write(FSO , 0 , BUFF1);
			}
			catch (System.Exception)
			{
				// If any error found , exit with an error.
				return false;
			}
			finally
			{
				// This is called in any case , even if it fails (Of Course before executing the Catch block.)
				FSO = null;
			}
			// Done.  All were sucessfully executed.
			return true;
		}
	
	}

    // System.IO.FileStream DM = System.IO.File.OpenRead(@".\ZSEX.zst");
    // System.IO.FileStream VA = System.IO.File.OpenWrite(@".\Out.txt");
    // System.Console.WriteLine(ZstandardArchives.DecompressFileStreams(DM , VA));
    // VA.Close();
    // VA.Dispose();
    // DM.Close();
    // DM.Dispose();
}