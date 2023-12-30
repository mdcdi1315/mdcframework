
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

#if (WPFExists == false) && NET7_0_OR_GREATER
#pragma warning disable CS1574
#endif

namespace ROOT
{
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

		/// <summary>
		/// API's that utilise encoding operations will get the value of this property in several operations , like in 
		/// <see cref="PassNewContentsToFile(string, System.IO.FileStream)"/> API.
		/// </summary>
		public static APIEncodingOptions Encoding { get { return MAINInternal.EncodingOPT; } set { MAINInternal.EncodingOPT = value; MAINInternal.SetNewEncoding(); } }

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
#if DEBUG
			System.Boolean SUCC = Interop.Shell32.ExecuteApp(System.IntPtr.Zero,
				Interop.Shell32.ExecuteVerbs.Open, URL, "", null, 9) >= 32;
			Debugger.DebuggingInfo($"(in ROOT.MAIN.__UNKNOWN(...)) ACTIVATED: {SUCC}");
			return SUCC;
#else
			return Interop.Shell32.ExecuteApp(System.IntPtr.Zero,
				Interop.Shell32.ExecuteVerbs.Open, URL, "", null, 9) >= 32;
#endif
		}

		[SupportedOSPlatform("windows")]
		private static System.Boolean StartUWPApp(System.String URL, IWin32Window window)
		{
#if DEBUG
            System.Boolean SUCC = Interop.Shell32.ExecuteApp(window.Handle,
				Interop.Shell32.ExecuteVerbs.Open, URL, "", null, 9) >= 32;
            Debugger.DebuggingInfo($"(in ROOT.MAIN.__UNKNOWN(...)) ACTIVATED: {SUCC}");
			return SUCC;
#else
			return Interop.Shell32.ExecuteApp(System.IntPtr.Zero,
				Interop.Shell32.ExecuteVerbs.Open, URL, "", null, 9) >= 32;
#endif
		}

#if NET47_OR_GREATER

        /// <summary>
        /// Translates a <see cref="Microsoft.IO.FileInfo"/> object to a <see cref="System.IO.FileInfo"/> object , 
        /// which are doing the same job and are identical to each other.
        /// </summary>
        /// <param name="file">The <see cref="Microsoft.IO.FileInfo"/> object to convert.</param>
        /// <returns>The converted object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is null.</exception>
		/// <exception cref="NullReferenceException"><paramref name="file"/> is null.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
		/// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
		/// <exception cref="UnauthorizedAccessException">Access to <paramref name="file"/> is denied.</exception>
		/// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
		/// <exception cref="NotSupportedException"><paramref name="file"/> contains a colon (:) in the middle of the string.</exception>
        [RequiresPreviewFeatures]
        [SupportedOSPlatform("windows")]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static System.IO.FileInfo ToFileInfo(this Microsoft.IO.FileInfo file) { return new(file.FullName); }

        /// <summary>
        /// Translates a <see cref="Microsoft.IO.DirectoryInfo"/> object to a <see cref="System.IO.DirectoryInfo"/> object , 
        /// which are doing the same job and are identical to each other.
        /// </summary>
        /// <param name="dir">The <see cref="Microsoft.IO.DirectoryInfo"/> object to convert.</param>
        /// <returns>The converted object.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="dir"/> is null.</exception>
		/// <exception cref="NullReferenceException"><paramref name="dir"/> is null.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
		/// <exception cref="ArgumentException"><paramref name="dir"/> contains invalid characters such as ", &lt;, &gt;, or |.</exception>
		/// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        [RequiresPreviewFeatures]
        [SupportedOSPlatform("windows")]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static System.IO.DirectoryInfo ToDirectoryInfo(this Microsoft.IO.DirectoryInfo dir) { return new(dir.FullName); }

        /// <summary>
		///  Translates a <see cref="Microsoft.IO.DirectoryInfo"/> array to a <see cref="System.IO.DirectoryInfo"/> array ,
        /// which are doing the same job and are identical to each other.
		/// </summary>
		/// <param name="dirs">The array to convert.</param>
		/// <returns>The converted array.</returns>
		/// <exception cref="ArgumentNullException">One of the elements in <paramref name="dirs"/> is null.</exception>
		/// <exception cref="NullReferenceException">One of the elements in <paramref name="dirs"/> is null.</exception>
		/// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
		/// <exception cref="ArgumentException">One of the elements in <paramref name="dirs"/> contains invalid characters such as ", &lt;, &gt;, or |.</exception>
		/// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        [RequiresPreviewFeatures]
        [SupportedOSPlatform("windows")]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static System.IO.DirectoryInfo[] ToDirectoryInfo(this Microsoft.IO.DirectoryInfo[] dirs)
		{
			System.IO.DirectoryInfo[] DR = new System.IO.DirectoryInfo[dirs.LongLength];
			for (System.Int64 D = 0; D < dirs.LongLength; D++) { DR[D] = dirs[D].ToDirectoryInfo(); }
			return DR;
		}

        /// <summary>
        /// Translates a <see cref="Microsoft.IO.FileInfo"/> array to a <see cref="System.IO.FileInfo"/> array , 
        /// which are doing the same job and are identical to each other.
        /// </summary>
        /// <param name="files">The array to convert.</param>
        /// <returns>The converted array.</returns>
        /// <exception cref="ArgumentNullException">One of the elements in <paramref name="files"/> is null.</exception>
        /// <exception cref="NullReferenceException">One of the elements in <paramref name="files"/> is null.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="ArgumentException">The file name is empty, contains only white spaces, or contains invalid characters.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to one of the elements in <paramref name="files"/> is denied.</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="NotSupportedException">One of the elements in <paramref name="files"/> contains a colon (:) in the middle of the string.</exception>
        [RequiresPreviewFeatures]
        [SupportedOSPlatform("windows")]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static System.IO.FileInfo[] ToFileInfo(this Microsoft.IO.FileInfo[] files)
		{
            System.IO.FileInfo[] DR = new System.IO.FileInfo[files.LongLength];
            for (System.Int64 D = 0; D < files.LongLength; D++) { DR[D] = files[D].ToFileInfo(); }
            return DR;
        }

#endif

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		public static System.Int64 Mod(System.Double One, System.Double Two) { return Round(One % Two); }

        /// <summary>
        /// Rounds the specified double value to the nearest integer value.
        /// </summary>
        /// <param name="Num">The number to round.</param>
        /// <returns>The rounded number as <see cref="System.Int64"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.Int64 Round(System.Double Num)
		{
			System.Int64 Result;
			System.Int64 Part = IntegerPart(Num);
            if (Num - Part > 0.5) { Result = Part + 1; } else { Result = Part; }
			return Result;
		}

		/// <summary>
		/// Returns the integer part of a <see cref="System.Double"/> value.
		/// </summary>
		/// <param name="Num">The number to return it's integer part.</param>
		/// <returns>The integer part of <paramref name="Num"/> as <see cref="System.Int64"/>.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static System.Int64 IntegerPart(System.Double Num) 
		{
            // Uses a more optimized result for calculating integer parts
            // Should only compile for .NET Core since doing the same for
            // .NET Framework this code would make it slow.
			// However , both practices give accurately the same results.
#if NET7_0_OR_GREATER
            System.Int64 Result = (System.Int64)Math.Floor(Math.Abs(Num));
			if (Num < 0) { Result *= -1; }
			return Result;
#else
			return (System.Int64)Num;
#endif
        }

		/// <summary>
		/// Tests whether a <see cref="System.String"/> has a character at the same position of the other <see cref="System.String"/>
		/// before the one tested.
		/// </summary>
		/// <param name="one">The <see cref="System.String"/> to check.</param>
		/// <param name="two">The second <see cref="System.String"/> to check.</param>
		/// <returns>If the above statement occurs , then it returns <see langword="true"/>; otherwise , <see langword="false"/>.</returns>
		/// <exception cref="OverflowException">
		/// If a <see cref="System.String"/> provided has more or less characters than the other one , and reaches the limit
		/// either the first or the last , and no value has been returned yet , then the result cannot be defined in this case. 
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// The parameter provided was <see langword="null"/>.
		/// </exception>
		public static System.Boolean StringIsLessThan(this System.String one, System.String two)
		{
			if (System.String.IsNullOrEmpty(one)) { throw new ArgumentNullException(nameof(one)); }
			if (System.String.IsNullOrEmpty(two)) { throw new ArgumentNullException(nameof(two)); }
			System.Char[] Array1 = one.ToUpperInvariant().ToCharArray();
			System.Char[] Array2 = two.ToUpperInvariant().ToCharArray();
            System.Int64 Len = Max(Array1.LongLength, Array2.LongLength);
            try {
                for (System.Int64 I = 0; I < Len; I++) { if (Array1[I] < Array2[I]) { return true; } }
            } catch (IndexOutOfRangeException e) { throw new OverflowException("Result was undefined until this point of search." , e); }
			return false;
		}

        /// <summary>
        /// Tests whether a <see cref="System.String"/> has a character at the same position of the other <see cref="System.String"/>
        /// before or is equal to the one tested.
        /// </summary>
        /// <param name="one">The <see cref="System.String"/> to check.</param>
        /// <param name="two">The second <see cref="System.String"/> to check.</param>
        /// <returns>If the above statement occurs , then it returns <see langword="true"/>; otherwise , <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">
        /// If a <see cref="System.String"/> provided has more or less characters than the other one , and reaches the limit
        /// either the first or the last , and no value has been returned yet , then the result cannot be defined in this case. 
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter provided was <see langword="null"/>.
        /// </exception>
        public static System.Boolean StringIsLessOrEqualThan(this System.String one, System.String two)
		{
			if (System.String.IsNullOrEmpty(one)) { throw new ArgumentNullException(nameof(one)); }
			if (System.String.IsNullOrEmpty(two)) { throw new ArgumentNullException(nameof(two)); }
			System.Char[] Array1 = one.ToUpperInvariant().ToCharArray();
			System.Char[] Array2 = two.ToUpperInvariant().ToCharArray();
			System.Int64 Len = Max(Array1.LongLength, Array2.LongLength);
            try {
                for (System.Int64 I = 0; I < Len; I++) { if (Array1[I] <= Array2[I]) { return true; } }
            } catch (IndexOutOfRangeException e) { throw new OverflowException("Result was undefined until this point of search.", e); }
            return false;
		}

        /// <summary>
        /// Tests whether a <see cref="System.String"/> has a character at the same position of the other <see cref="System.String"/>
        /// later the one tested.
        /// </summary>
        /// <param name="one">The <see cref="System.String"/> to check.</param>
        /// <param name="two">The second <see cref="System.String"/> to check.</param>
        /// <returns>If the above statement occurs , then it returns <see langword="true"/>; otherwise , <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">
        /// If a <see cref="System.String"/> provided has more or less characters than the other one , and reaches the limit
        /// either the first or the last , and no value has been returned yet , then the result cannot be defined in this case. 
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter provided was <see langword="null"/>.
        /// </exception>
        public static System.Boolean StringIsGreaterThan(this System.String one, System.String two)
		{
			if (System.String.IsNullOrEmpty(one)) { throw new ArgumentNullException(nameof(one)); }
			if (System.String.IsNullOrEmpty(two)) { throw new ArgumentNullException(nameof(two)); }
			System.Char[] Array1 = one.ToUpperInvariant().ToCharArray();
			System.Char[] Array2 = two.ToUpperInvariant().ToCharArray();
			System.Int64 Len = Max(Array1.LongLength, Array2.LongLength);
            try {
                for (System.Int64 I = 0; I < Len; I++) { if (Array1[I] > Array2[I]) { return true; } }
            } catch (IndexOutOfRangeException e) { throw new OverflowException("Result was undefined until this point of search.", e); }
            return false;
		}

        /// <summary>
        /// Tests whether a <see cref="System.String"/> has a character at the same position of the other <see cref="System.String"/>
        /// later or equal the one tested.
        /// </summary>
        /// <param name="one">The <see cref="System.String"/> to check.</param>
        /// <param name="two">The second <see cref="System.String"/> to check.</param>
        /// <returns>If the above statement occurs , then it returns <see langword="true"/>; otherwise , <see langword="false"/>.</returns>
        /// <exception cref="OverflowException">
        /// If a <see cref="System.String"/> provided has more or less characters than the other one , and reaches the limit
        /// either the first or the last , and no value has been returned yet , then the result cannot be defined in this case. 
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter provided was <see langword="null"/>.
        /// </exception>
        public static System.Boolean StringIsGreaterOrEqualThan(this System.String one, System.String two)
		{
			if (System.String.IsNullOrEmpty(one)) { throw new ArgumentNullException(nameof(one)); }
			if (System.String.IsNullOrEmpty(two)) { throw new ArgumentNullException(nameof(two)); }
			System.Char[] Array1 = one.ToUpperInvariant().ToCharArray();
			System.Char[] Array2 = two.ToUpperInvariant().ToCharArray();
			System.Int64 Len = Max(Array1.LongLength, Array2.LongLength);
            try {
                for (System.Int64 I = 0; I < Len; I++) { if (Array1[I] >= Array2[I]) { return true; } }
            } catch (IndexOutOfRangeException e) { throw new OverflowException("Result was undefined until this point of search.", e); }
            return false;
		}

		/// <summary>
		/// Finds the quotient from a division operation.
		/// </summary>
		/// <param name="One">The first number to divide.</param>
		/// <param name="Two">The second number to divide.</param>
		/// <returns>The quotient of the two divided numbers.</returns>
		public static System.Int64 Div(System.Double One, System.Double Two) { return Round(One / Two); }

		/// <summary>
		/// Opens a Windows UWP App.
		/// </summary>
		/// <param name="link">One of the values of the <see cref="SystemLinks"/> enumeration.</param>
		/// <param name="Window">The function will show messages to the specified window , if needed.</param>
		/// <returns>A <see cref="System.Boolean"/> determining whether the 
		/// specified UWP App was opened or restored.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean OpenSystemApp(SystemLinks link, IWin32Window Window) { return StartUWPApp(ToURL(link), Window); }

#if NEEDS_HTTPLIB == false

		/// <summary>
		/// Creates a new Windows Internet Shortcut.
		/// </summary>
		/// <param name="URL">The URL that the shortcut will point to.</param>
		/// <param name="Path">The path of the shortcut that will be saved.</param>
		/// <returns>Returns <see langword="true"/> on success; <see langword="false"/> on error.</returns>
		[Obsolete("The method " + nameof(CreateInternetShortcut) + " and it's overloads has been moved to ROOT.HTTPLIB_MAIN." , DiagnosticId = "MDCFR002")]
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
        [Obsolete("The method " + nameof(CreateInternetShortcut) + " and it's overloads has been moved to ROOT.HTTPLIB_MAIN.", DiagnosticId = "MDCFR002")]
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
			"CA1508:Avoid dead conditional code", Justification = "Methods used in this class have still the possiblity to return null.")]
        [Obsolete("The method " + nameof(CreateInternetShortcut) + " and it's overloads has been moved to ROOT.HTTPLIB_MAIN.", DiagnosticId = "MDCFR002")]
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
#endif

        /// <summary>
        /// The <see cref="ToArray"/> method gets all the data representing the current buffer , and returns them
        /// as a one-dimensional and fixed <see cref="System.Byte"/>[] array.
        /// </summary>
        /// <returns>The data which the <see langword="struct"/> given holds.</returns>
        public static System.Byte[] ToArray(this ModifidableBuffer str) { return str.ToArray(); }

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
			if (link == null) { return false; }
			if (link.EndsWith("://", StringComparison.OrdinalIgnoreCase) == false) { return false; }
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
			if (link == null) { return false; }
			if (link.EndsWith("://", StringComparison.OrdinalIgnoreCase) == false) { return false; }
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
		[SupportedOSPlatform("windows")]
		public static System.String GetFilePathFromInvokedDialog(this DialogsReturner DG) { return DG.FileNameFullPath; }

		/// <summary>
		/// Gets from an instance of the <see cref="DialogsReturner"/> class if the dialog was executed sucessfully and a path was got.
		/// </summary>
		/// <param name="DG">The <see cref="DialogsReturner"/> class instance to get data from.</param>
		/// <returns>A <see cref="System.Boolean"/> value indicating whether the dialog execution 
		/// was sucessfull; <c>false</c> in the case of error or the user did not supplied a file path.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean GetLastErrorFromInvokedDialog(this DialogsReturner DG) { if (DG.ErrorCode == "Error") { return false; } else { return true; } }

		/// <summary>
		/// A static class which creates console colored messages to differentiate the types of errors or information given.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
			"CA1034:Nested types should not be visible",
			Justification = "Cannot be unnested due to API Compatibility Surface")]
		public static class IntuitiveConsoleText
		{
			/// <summary>
			/// This writes the data specified on the console. These data are informational. The background is black and the foreground is gray.
			/// </summary>
			/// <param name="Text">The <see cref="System.String"/> to write to the console.</param>
			[SupportedOSPlatform("windows")]
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
			[SupportedOSPlatform("windows")]
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
			[SupportedOSPlatform("windows")]
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
			[SupportedOSPlatform("windows")]
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
		/// <exception cref="PlatformNotSupportedException">
		/// This exception will be thrown when MDCFR was built and used for other platforms , such as Unix.
		/// </exception>
		[SupportedOSPlatform("windows")]
		public static void EmitBeepSound()
		{
#if IsWindows
			_ = Interop.Kernel32.ConsoleBeep(800, 210);
#else
			ExceptionData = new PlatformNotSupportedException("This API is not supported for this OS.");
#endif
		}

		/// <summary>
		/// This is a custom implementation of <see cref="System.Console.WriteLine()"/> which writes data to the console.
		/// </summary>
		/// <param name="Text">The <see cref="System.String"/> data to write to the console.</param>
		/// <returns>A <see cref="System.Boolean"/> value whether the native function suceeded.</returns>
		public static System.Boolean WriteConsoleText(System.String Text)
		{
#if IsWindows
			return global::ConsoleInterop.WriteToConsole($"{Text}\r\n");
#else
			try
			{
				System.Console.WriteLine(Text);
				return true;
			} catch (System.IO.IOException E) { ExceptionData = E; return false; }
#endif
		}

		/// <summary>
		/// This is a custom implementation of <see cref="System.Console.Write(string)"/> which writes data to the console.
		/// </summary>
		/// <param name="Text">The <see cref="System.String"/> data to write to the console.</param>
		/// <returns>A <see cref="System.Boolean"/> value whether the native function suceeded.</returns>
		[RequiresPreviewFeatures]
		public static System.Boolean WriteConsole(System.String Text)
		{
#if IsWindows
			return global::ConsoleInterop.WriteToConsole(Text);
#else
			try
			{
				System.Console.Write(Text);
				return true;
			} catch (System.IO.IOException E) { ExceptionData = E; return false; }
#endif
		}

		/// <summary>
		/// Detaches the running console.
		/// </summary>
		/// <returns>A <see cref="System.Boolean"/> value indicating if this method was executed sucessfully.</returns>
		/// <exception cref="PlatformNotSupportedException">
		/// This exception will be thrown when MDCFR was built and used for other platforms , such as Unix.
		/// </exception>
		public static System.Boolean DetachConsole()
		{
#if IsWindows
			if (ConsoleInterop.DetachConsole() != 0) { ConsoleExtensions.Detached = true; return false; } else { return true; }
#else
			ExceptionData = new PlatformNotSupportedException("This API is not supported for this OS.");
			return false;
#endif
		}

		/// <summary>
		/// Attach the current application to the specified console PID.
		/// </summary>
		/// <param name="ConsolePID">The Console's PID to attach to. If not defined , it will try to attach to the parent process console ,
		/// if it exists and has spawned a console.</param>
		/// <returns>A <see cref="System.Boolean"/> value indicating if this method was executed sucessfully.</returns>
		/// <exception cref="PlatformNotSupportedException">
		/// This exception will be thrown when MDCFR was built and used for other platforms , such as Unix.
		/// </exception>
		public static System.Boolean AttachToConsole(System.Int32 ConsolePID = -1)
		{
#if IsWindows
			if (ConsoleInterop.AttachToConsole(ConsolePID) != 0) { ConsoleExtensions.Detached = false; return true; } else { return false; }
#else
			ExceptionData = new PlatformNotSupportedException("This API is not supported for this OS.");
			return false;
#endif
		}

		/// <summary>
		/// Create (allocate) a new console for the current process.
		/// </summary>
		/// <returns>A <see cref="System.Boolean"/> value indicating if this method was executed sucessfully.</returns>
		/// <exception cref="PlatformNotSupportedException">
		/// This exception will be thrown when MDCFR was built and used for other platforms , such as Unix.
		/// </exception>
		public static System.Boolean CreateConsole()
		{
#if IsWindows
			if (ConsoleInterop.CreateConsole() != 0) { ConsoleExtensions.Detached = false; return true; } else { return false; }
#else
			ExceptionData = new PlatformNotSupportedException("This API is not supported for this OS.");
			return false;
#endif
		}

		/// <summary>
		/// Read a text line from console. This uses a custom inplementation so as to get the data.
		/// </summary>
		/// <param name="Opt">The Buffer Size to set. If left undefined , then it is <see cref="ConsoleExtensions.ConsoleReadBufferOptions.Default"/></param>
		/// <returns>The data read from the console. If any error found , then it will return the <c>"Error"</c> <see cref="System.String"/> .</returns>
		[SupportedOSPlatform("windows")]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static System.String ReadConsoleText(ROOT.ConsoleExtensions.ConsoleReadBufferOptions Opt =
			ROOT.ConsoleExtensions.ConsoleReadBufferOptions.Default)
		{
#if IsWindows
			return ConsoleInterop.ReadFromConsole(Opt);
#else
			return System.Console.ReadLine();	
#endif
		}

		/// <summary>
		/// This writes a <see cref="System.Char"/>[] to the console. <see cref="System.Console.WriteLine()"/> also 
		/// contains such a method , but this one is different and has no any relationship with that one.
		/// </summary>
		/// <param name="Text">The <see cref="System.Char"/>[] data to write.</param>
		/// <returns>A <see cref="System.Boolean"/> value whether the native function suceeded.</returns>
		[SupportedOSPlatform("windows")]
		public static System.Boolean WriteConsoleText(System.Char[] Text)
		{
#if IsWindows
			List<System.Char> LST = new List<System.Char>(Text) { '\r', '\n' };
			System.Boolean DS = ConsoleInterop.WriteToConsole(LST.ToArray());
			LST.Clear();
			LST = null;
			return DS;
#else
			try
			{
				 System.Console.WriteLine(Text);
				 return true;
			} catch (System.IO.IOException E) { ExceptionData = E; return false; }
#endif
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

#if NET472_OR_GREATER || NET7_0_OR_GREATER

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
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
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
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
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
		/// <returns>This method will always throw <see cref="PlatformNotSupportedException"/>.</returns>
		[Obsolete("This method has been deprecated due to the disability to create the target properly.", DiagnosticId = "MDCFR_DD1")]
		[SupportedOSPlatform("windows")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
			"CA1031:Do not catch general exception types",
			Justification = "The Internal Debugger depends on this general exception type.")]
		public static System.Boolean CreateLink(System.String PathToSave, System.String PathToPoint,
			System.String WorkingDirectory = "")
		{
			PlatformNotSupportedException F = new PlatformNotSupportedException("This method has been deprecated.");
			ExceptionData = F;
			throw F;
		}

		/// <summary>
		/// Determines whether the caller has administrative permissions 
		/// (That is , the caller was launched with the 'Run As Administrator' option). <br />
		/// On other platforms , such as Unix , this method will always return <see langword="true"/>.
		/// </summary>
		/// <returns>A <see cref="System.Boolean"/> value determining whether the caller has administrative priviledges.</returns>
		public static System.Boolean RunsAsAdmin()
		{
#if IsWindows
			System.Security.Principal.WindowsPrincipal DI = new(System.Security.Principal.WindowsIdentity.GetCurrent());
			return DI.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
#else
			return true;
#endif
		}

		/// <summary>
		/// Change in a <see cref="System.String"/> the defined character with another one.
		/// </summary>
		/// <param name="Stringtoreplacechar">The <see cref="System.String"/> data to take from.</param>
		/// <param name="Chartochange">The character that will be changed.</param>
		/// <param name="Chartobechanged">The character that will replace the <paramref name="Chartochange"/> character.</param>
		/// <returns>The <see cref="System.String"/> given to <paramref name="Stringtoreplacechar"/> ,
		/// but with the defined character changed as specified by the <paramref name="Chartobechanged"/> parameter.</returns>
		public static System.String ChangeDefinedChar(this System.String Stringtoreplacechar,
			System.Char Chartochange, System.Char Chartobechanged)
		{
			if (System.String.IsNullOrEmpty(Stringtoreplacechar)) { return "Error"; }
			System.Char[] array = Stringtoreplacechar.ToCharArray();
			if (array == null || array.Length < 1) { return "Error"; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.ChangeDefinedChar(... , {Chartochange} , {Chartobechanged})) VALIDARRAY: true");
#endif
			System.String Result = null;
			for (System.Int32 I = 0; I < array.Length; I++)
			{
				if (array[I] == Chartochange) { Result += Chartobechanged; } else { Result += array[I]; }
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
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.String RemoveDefinedChars(this System.String StringToClear, params System.Char[] CharToClear)
		{
			if (System.String.IsNullOrEmpty(StringToClear)) { return null; }
			System.Char[] CharString = StringToClear.ToCharArray();
			if (CharString.Length <= 0) { return null; }
			if (CharToClear == null || CharToClear.Length <= 0) { return null; }
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
		/// Checks whether a file exists or not by the <paramref name="Path"/> supplied.
		/// </summary>
		/// <param name="Path">The <see cref="System.String"/> which is a filepath to check if the file exists.</param>
		/// <returns>If the file exists in the <paramref name="Path"/> supplied , then <c>true</c>; otherwise <c>false</c>.</returns>
		public static System.Boolean FileExists(System.String Path)
		{
			System.Boolean Ex = false;
#if IsWindows
#if NET47_OR_GREATER
			Ex = Microsoft.IO.File.Exists(Path);
#else
			Ex = FileInterop.FileExists(Path);
#endif
#else
			Ex = System.IO.File.Exists(Path);
#endif
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.FileExists({Path})) EXISTS: {Ex}");
#endif
			return Ex;
		}

		/// <summary>
		/// Checks whether a directory exists or not by the <paramref name="Path"/> supplied.
		/// </summary>
		/// <param name="Path">The <see cref="System.String"/> which is a directory path to check if the directory exists.</param>
		/// <returns>If the directory exists in the <paramref name="Path"/> supplied , then <c>true</c>; otherwise <c>false</c>.</returns>
		public static System.Boolean DirExists(System.String Path)
		{
			System.Boolean Ex = false;
#if IsWindows
#if NET47_OR_GREATER
			Ex = Microsoft.IO.Directory.Exists(Path);
#else
			Ex = System.IO.Directory.Exists(Path);
#endif
#else
			Ex = System.IO.Directory.Exists(Path);
#endif
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.DirExists({Path}) EXISTS: {Ex})");
#endif
			return Ex;
		}

		/// <summary>
		/// Creates a directory specified by the <paramref name="Path"/> parameter.
		/// </summary>
		/// <param name="Path">The directory path to create.</param>
		/// <returns><c>true</c> if the directory was created sucessfully; otherwise , <c>false</c>.</returns>
		public static System.Boolean CreateADir(System.String Path)
		{
			System.Boolean Ex = false;
#if IsWindows
#if NET47_OR_GREATER
			Ex = Microsoft.IO.Directory.CreateDirectory(Path).Exists;
#else
			Ex = FileInterop.CreateDir(Path) != 0;
#endif
#else
			Ex = System.IO.Directory.CreateDirectory(Path).Exists;
#endif
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateADir(...)) CREATED: {Ex}");
#endif
			return Ex;
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
#if IsWindows
#if NET47_OR_GREATER
			try
			{
				Microsoft.IO.Directory.Delete(Path, DeleteAll);
				return true;
			} catch (System.Exception E) { ExceptionData = E; return false; }
#else
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
#endif
#else
			try
			{
				System.IO.Directory.Delete(Path, DeleteAll);
				return true;
			} catch (System.Exception E) { ExceptionData = E; return false; }
#endif
		}

		/// <summary>
		/// Move files or directories specified by the parameters. The <paramref name="SourcePath"/> must exist.
		/// </summary>
		/// <param name="SourcePath">The path to get the directory or file to move it.</param>
		/// <param name="DestPath">The destination path that the file or directory should go to.</param>
		/// <returns><c>true</c> if the file or directory was moved; otherwise , <c>false</c>.</returns>
		public static System.Boolean MoveFilesOrDirs(System.String SourcePath, System.String DestPath)
		{
			System.Boolean Ex = false;
#if IsWindows
#if NET47_OR_GREATER
			try
			{
				if (FileExists(SourcePath))
				{
					Microsoft.IO.File.Move(SourcePath, DestPath);
					Ex = true;
				}
				else if (DirExists(SourcePath))
				{
					Microsoft.IO.Directory.Move(SourcePath, DestPath);
					Ex = true;
				}
			}
			catch (System.Exception E) { ExceptionData = E; Ex = false; }
#else
			Ex = FileInterop.MoveFileOrDir(SourcePath, DestPath) != 0;
#endif
#else
			try
            {
                if (FileExists(SourcePath))
				{
					System.IO.File.Move(SourcePath, DestPath);
					Ex = true;
				}
				else if (DirExists(SourcePath))
				{
					System.IO.Directory.Move(SourcePath, DestPath);
					Ex = true;
				}
            }
            catch (System.Exception E) { ExceptionData = E; Ex = false; }
#endif
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.MoveFileOrDir(... , ...)) MOVED: {Ex}");
#endif
			return Ex;
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
			System.Boolean Ex = false;
#if IsWindows
#if NET47_OR_GREATER
			try
			{
				Microsoft.IO.File.Copy(SourceFilePath, DestPath, OverWriteAllowed);
				Ex = true;
			} catch (System.Exception E) { ExceptionData = E; Ex = false;  }
#else
			Ex = FileInterop.CopyFile(SourceFilePath, DestPath, OverWriteAllowed == false) != 0;
#endif
#else
			try
			{
				System.IO.File.Copy(SourceFilePath, DestPath, OverWriteAllowed);
				Ex = true;
			} catch (System.Exception E) { ExceptionData = E; Ex = false;  }
#endif
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CopyFile(... , ...)) COPIED: {Ex}");
#endif
			return Ex;
		}

		/// <summary>
		/// Gets a new <see cref="System.IO.FileSystemInfo"/> captured from the specified directory.
		/// </summary>
		/// <param name="Path">The directory to get the data from.</param>
		/// <returns>A new <see cref="System.IO.FileSystemInfo"/> object containing the data; otherwise , <c>null</c> if an error occured.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
			"CA1031:Do not catch general exception types",
			Justification = "The Internal Debugger depends on this general exception type.")]
		[return: System.Diagnostics.CodeAnalysis.MaybeNull]
		public static System.IO.FileSystemInfo[] GetANewFileSystemInfo(System.String Path)
		{
			if (DirExists(Path) == false) { return null; }
			try
			{
				System.IO.DirectoryInfo RFD = new System.IO.DirectoryInfo(Path);
				return RFD.GetFileSystemInfos("*", System.IO.SearchOption.AllDirectories);
			} catch (System.Exception e) { ExceptionData = e; return null; }
		}

		/// <summary>
		/// Creates a new and fresh file and opens a new handle for it by the <see cref="System.IO.FileStream"/>.
		/// </summary>
		/// <param name="Path">The file path where the file will be created. Example: <![CDATA[C:\Files\Start.txt]]> .</param>
		/// <returns>A new <see cref="System.IO.FileStream"/> object if no error occured; otherwise , <c>null</c>.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
			"CA1031:Do not catch general exception types",
			Justification = "The Internal Debugger depends on this general exception type.")]
		[return: System.Diagnostics.CodeAnalysis.MaybeNull]
		public static System.IO.FileStream CreateANewFile(System.String Path)
		{
			try
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.CreateANewFile(...)) INFO: Attempting to create it.");
#endif
#if IsWindows
#if NET47_OR_GREATER
				return Microsoft.IO.File.Create(Path);
#else
				return System.IO.File.OpenWrite(Path);
#endif
#else
				return System.IO.File.OpenWrite(Path);
#endif
			}
			catch (System.Exception e)
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
		[return: System.Diagnostics.CodeAnalysis.MaybeNull]
		public static System.IO.FileStream ReadAFileUsingFileStream(System.String Path, System.Boolean AllowAccessToOthers = false)
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
			"CA1031:Do not catch general exception types",
			Justification = "The Internal Debugger depends on this general exception type.")]
		[return: System.Diagnostics.CodeAnalysis.MaybeNull]
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
			"CA1031:Do not catch general exception types",
			Justification = "The Internal Debugger depends of this general exception type.")]
		public static void PassNewContentsToFile(System.String Contents, System.IO.FileStream FileStreamObject)
		{
            if (FileStreamObject == null)
            {
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.AppendNewContentsToFile(...)) ERROR: The FileStreamObject given was invalid.");
#endif
                return;
            }
            System.Byte[] EMDK = MAINInternal.CurrentEncoding.GetBytes(Contents + System.Environment.NewLine);
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
			"CA1031:Do not catch general exception types",
			Justification = "The Internal Debugger depends of this general exception type.")]
		public static void AppendNewContentsToFile(System.String Contents, System.IO.FileStream FileStreamObject)
		{
            if (FileStreamObject == null) 
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.AppendNewContentsToFile(...)) ERROR: The FileStreamObject given was invalid.");
#endif
                return; 
			}
            System.Byte[] EMDK = MAINInternal.CurrentEncoding.GetBytes(Contents);
			try
			{
#if DEBUG
                Debugger.DebuggingInfo("(in ROOT.MAIN.AppendNewContentsToFile(...)) INFO: Attempting to write data to the target.");
#endif
				FileStreamObject.Position = FileStreamObject.Length;
                FileStreamObject.Write(EMDK, 0, EMDK.Length);
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
        /// <param name="Length">If you specify this parameter , then instead of getting the whole file , it gets only until the limit specified.</param>
        /// <returns>The file contents to a <see cref="System.String"/> .</returns>
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static System.String GetContentsFromFile(System.IO.FileStream FileStreamObject , System.Int64 Length = -3)
		{
			if (Length == -3) { Length = FileStreamObject.Length; }
			FileStreamObject.Position = 0;
			System.Byte[] EMS = MAINInternal.D_ReadBufferedInternalTarget(FileStreamObject , Length);
			return MAINInternal.CurrentEncoding.GetString(EMS);
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
            System.IO.FileStream Initialiser = ReadAFileUsingFileStream(PathOfFile);
            if (Initialiser == null) { return "Error"; }
            else
            {
                Contents = MAINInternal.D_ReadBufferedInternalTarget(Initialiser, Initialiser.Length);
                Initialiser.Close();
                Initialiser.Dispose();
            }
            if (Contents == null) { return "Error"; }
            System.Security.Cryptography.HashAlgorithm EDI = MAINInternal.D_GetACryptographyHashForAFile_1(HashToSelect);
            if (EDI == null)
            {
                ExceptionData = new InvalidOperationException($"Error - Option {HashToSelect} is invalid.");
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
			if (Initialiser == null) { return "Error"; }
			else {
                Contents = MAINInternal.D_ReadBufferedInternalTarget(Initialiser, Initialiser.Length);
				Initialiser.Close();
                Initialiser.Dispose();
            }
			if (Contents == null) { return "Error"; }
            System.Security.Cryptography.HashAlgorithm EDI = MAINInternal.D_GetACryptographyHashForAFile_2(HashToSelect);
            if (EDI == null)
            {
                ExceptionData = new InvalidOperationException($"Error - Option {HashToSelect} is invalid.");
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
#if IsWindows
#if NET47_OR_GREATER
			try
			{
				Microsoft.IO.File.Delete(Path);
				return true;
			} catch (System.Exception E) { ExceptionData = E; return false; }
#else
			return FileInterop.DeleteFile(Path) != 0;
#endif
#else
			try
			{
				System.IO.File.Delete(Path);
				return true;
			} catch (System.Exception E) { ExceptionData = E; return false; }
#endif
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
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.String GetAStringFromTheUserNew(System.String Prompt,
        System.String Title, System.String DefaultResponse) 
		{ return MAINInternal.GetAStringFromTheUserNewInternal(Prompt, Title, DefaultResponse); }

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
		MessageBoxButtons MessageButton = 0,
		MessageBoxIcon MessageIcon = 0)
		{ return MAINInternal.NewMessageBoxToUserInternal(MessageString , Title , MessageButton , MessageIcon); }

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
		{ return MAINInternal.NewMessageBoxToUserInternal(MessageString, Title, MessageButton, IconToSelect); }

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
			return MAINInternal.CreateLoadDialogInternal(FileFilterOfWin32, FileExtensionToPresent, 
				FileDialogWindowTitle, DirToPresent);
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
			return MAINInternal.CreateSaveDialogInternal(FileFilterOfWin32 , FileExtensionToPresent , 
				FileDialogWindowTitle , DirToPresent);
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
            return MAINInternal.CreateLoadDialogInternal(FileFilterOfWin32, FileExtensionToPresent,
                FileDialogWindowTitle);
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
            return MAINInternal.CreateSaveDialogInternal(FileFilterOfWin32, FileExtensionToPresent,
                FileDialogWindowTitle);
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
			return MAINInternal.CreateSaveDialogInternal(FileFilterOfWin32, FileExtensionToPresent,
                FileDialogWindowTitle, FileMustExist , DirToPresent);
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
            return MAINInternal.CreateSaveDialogInternal(FileFilterOfWin32, FileExtensionToPresent,
                FileDialogWindowTitle, FileMustExist);
        }

		/// <summary>
		/// This spawns a Directory Selection dialog to the user.
		/// </summary>
		/// <param name="DirToPresent">The Directory that this dialog should show first.</param>
		/// <param name="DialogWindowTitle">The title that will be shown when this dialog is invoked , with an additional description if needed.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent, System.String DialogWindowTitle)
		{ return MAINInternal.GetADirDialogInternal(DirToPresent, DialogWindowTitle); }

		/// <summary>
		/// This spawns a Directory Selection dialog to the user.
		/// </summary>
		/// <param name="DirToPresent">The Directory that this dialog should show first.</param>
		/// <param name="DialogWindowTitle">The title that will be shown when this dialog is invoked , with an additional description if needed.</param>
		/// <param name="AlternateDir">When the <paramref name="DirToPresent"/> parameter is MyComputer , you can set whatever initial directory you like to.</param>
		/// <returns>A newly constructed <see cref="DialogsReturner"/> class which contains the data returned by this function.</returns>
		[SupportedOSPlatform("windows")]
		public static DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent, System.String DialogWindowTitle, System.String AlternateDir)
        { return MAINInternal.GetADirDialogInternal(DirToPresent, DialogWindowTitle , AlternateDir); }

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
		// Spliting can be a very consuming process , process the data as fast as possible.
		[return: System.Diagnostics.CodeAnalysis.MaybeNull]
		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		public static System.String[] GetPathEnvironmentVar()
		{
			try
			{
				System.String[] RMF = (System.Environment.GetEnvironmentVariable("Path")).Split(';');
				return RMF;
			} catch (System.Exception) { return null; }
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
				if (FileExists($"{strings[I]}\\{FileName}"))
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
		public static FileSearchResult FindFileFromPath(System.String FileName, params System.String[] Extensions)
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
					if (FileExists($"{strings[B]}\\{FileName}.{Extensions[A]}"))
					{
						FSR.Filepath = $"{strings[B]}\\{FileName}.{Extensions[A]}";
						FSR.ext = Extensions[A];
						FSR.success = true;
						return FSR;
					}
				}
			}
			FSR.success = false;
			return FSR;
		}

	}

#if NEEDS_HTTPLIB == false
	/// <summary>
	/// Because HTTPLIB is deprecated after .NET 7 , any references from the <see cref="MAIN"/> class that bother HTTPLIB are located here. <br />
	/// The <see cref="MAIN"/> signatures that use HTTPLIB are deprecated.
	/// </summary>
    public static class HTTPLIB_MAIN
	{
		/// <summary>
		/// Creates a new Windows Internet Shortcut.
		/// </summary>
		/// <param name="URL">The URL that the shortcut will point to.</param>
		/// <param name="Path">The path of the shortcut that will be saved.</param>
		/// <returns>Returns <see langword="true"/> on success; <see langword="false"/> on error.</returns>
		[return: System.Diagnostics.CodeAnalysis.MaybeNull]
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
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
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
                MAIN.HaltApplicationThread(130);
            }
            req = null;
            if (err != null) { err = null; goto G_ExitErr; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) URLFIND: true");
#endif
            err = null;
            if (MAIN.FileExists(Path))
            {
                if (OverwriteIfExists)
                {
                    Out = MAIN.ClearAndWriteAFile(Path);
#if DEBUG
                    Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) CALL: ClearAndWriteAFile()");
#endif
                    if (Out == null) { goto G_ExitErr; }
                }
                else { goto G_ExitErr; }
            }
            else
            {
                Out = MAIN.CreateANewFile(Path);
#if DEBUG
                Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) CALL: CreateANewFile()");
#endif
                if (Out == null) { goto G_ExitErr; }
            }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) PROCESS_ACTION");
#endif
            MAIN.PassNewContentsToFile(String.Format(MDCFR.Properties.Resources.MDCFR_INTS_CREATE, URL.ToString()), Out);
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
			"CA1508:Avoid dead conditional code", Justification = "Methods used in this class have still the possiblity to return null.")]
        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        public static System.Boolean CreateInternetShortcut(
			System.Uri URL, System.String Path,
			System.String CustomIconFile, System.Int16 IconFileNumToUse,
			System.Boolean OverwriteIfExists = false)
		{
			System.IO.FileStream Out = null;
			if (MAIN.FileExists(CustomIconFile) == false) { goto G_ExitErr; }
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
				MAIN.HaltApplicationThread(130);
			}
			req = null;
			if (err != null) { err = null; goto G_ExitErr; }
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) URLFIND: true");
#endif
			err = null;
			if (MAIN.FileExists(Path))
			{
				if (OverwriteIfExists)
				{
					Out = MAIN.ClearAndWriteAFile(Path);
#if DEBUG
                    Debugger.DebuggingInfo($"(in ROOT.MAIN.CreateInternetShortcut(... , ...)) CALL: ClearAndWriteAFile()");
#endif
					if (Out == null) { goto G_ExitErr; }
				}
				else { goto G_ExitErr; }
			}
			else
			{
				Out = MAIN.CreateANewFile(Path);
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
			MAIN.PassNewContentsToFile(System.String.Format(MDCFR.Properties.Resources.MDCFR_INTS_CREATE2,
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
#endif

    [SpecialName]
	internal static class MAINInternal
	{
        // Internal implementation class provided to the MAIN class , mostly for assistive scenarios.
		/// <summary>Internal constant which assists with setting and working around a buffer limit.</summary>
        private const System.Int32 BUFSIZE = 4096;

        // Internal session encoding keeper. Used in the API's in the MAIN class.
        private static System.Text.Encoding encodingnow;

		public static APIEncodingOptions EncodingOPT;

		/// <summary>Gets the globally current set encoding.</summary>
		public static System.Text.Encoding CurrentEncoding { get { return encodingnow; } }

		public static void SetNewEncoding()
		{
			switch (EncodingOPT)
			{
				case APIEncodingOptions.UTF8:
                    encodingnow = new System.Text.UTF8Encoding(false); break;
                case APIEncodingOptions.UTF8BOM:
					encodingnow = new System.Text.UTF8Encoding(true); break;
				case APIEncodingOptions.UTF16:
					encodingnow = new System.Text.UnicodeEncoding(bigEndian: true, byteOrderMark: false); break;
				case APIEncodingOptions.UTF16LE:
                    encodingnow = new System.Text.UnicodeEncoding(bigEndian: false, byteOrderMark: false); break;
				case APIEncodingOptions.UTF16BOM:
                    encodingnow = new System.Text.UnicodeEncoding(bigEndian: true, byteOrderMark: true); break;
				case APIEncodingOptions.UTF16LEBOM:
                    encodingnow = new System.Text.UnicodeEncoding(bigEndian: false, byteOrderMark: true); break;
                case APIEncodingOptions.UTF32:
					encodingnow = new System.Text.UTF32Encoding(bigEndian: true, byteOrderMark: false); break;
				case APIEncodingOptions.UTF32LE:
                    encodingnow = new System.Text.UTF32Encoding(bigEndian: false, byteOrderMark: false); break;
				case APIEncodingOptions.UTF32BOM:
                    encodingnow = new System.Text.UTF32Encoding(bigEndian: true, byteOrderMark: true); break;
				case APIEncodingOptions.UTF32LEBOM:
                    encodingnow = new System.Text.UTF32Encoding(bigEndian: false, byteOrderMark: true); break;
                case APIEncodingOptions.ASCII:
					encodingnow = new System.Text.ASCIIEncoding(); break;
			};
		}

		static MAINInternal()
		{
			EncodingOPT = APIEncodingOptions.UTF8;
			encodingnow = new System.Text.UTF8Encoding(false);
		}

		/// <summary>
		/// Copies data from a <see cref="System.IO.Stream"/>-derived instance to another 
		/// <see cref="System.IO.Stream"/>-derived instance.
		/// </summary>
		/// <param name="Input">The input stream.</param>
		/// <param name="Output">The output stream.</param>
		/// <param name="RequestedBytes">The bytes to read. This can be less than the length of <paramref name="Input"/>.</param>
		public static void BufferedCopyStream(System.IO.Stream Input , System.IO.Stream Output , System.Int64 RequestedBytes)
		{
            if (RequestedBytes > Input.Length || RequestedBytes < 0)
            {
				throw new ArgumentException("RequestedBytes parameter cannot be larger than Input.Length and less than 0." , nameof(RequestedBytes));
            } else if (Input == null || Input.CanRead == false)
			{
				throw new ArgumentException("The input stream must be an instance of a readable stream." , nameof(Input));
			} else if (Output == null || Output.CanWrite == false)
			{ throw new ArgumentException("The output stream must be an instance of a writeable stream.", nameof(Output)); }
            System.Int64 I = 0;
            System.Byte[] TempBuf;
            for (; (I < RequestedBytes) && ((RequestedBytes - I) > BUFSIZE); I += BUFSIZE)
            {
                TempBuf = new System.Byte[BUFSIZE];
                Input.Read(TempBuf, 0, BUFSIZE);
                Output.Write(TempBuf, 0, TempBuf.Length);
            }
            if ((RequestedBytes - I) <= BUFSIZE) { for (; RequestedBytes > I; I++) { Output.WriteByte((System.Byte)Input.ReadByte()); } }
        }

		private static NotSupportedException ThrowNotSupportedPlatform_WPFParts()
		{
			return new NotSupportedException(MDCFR.Properties.Resources.MDCFR_PlatformNotSupportedMsg);
		}

        /// <summary>
        /// Reads bytes from a <see cref="System.IO.Stream"/>-derived class , specifying the bytes to read 
		/// by the <paramref name="RequestedBytes"/> parameter.
        /// </summary>
        /// <param name="Stream">The <see cref="System.IO.Stream"/>-derived class to read bytes from.</param>
        /// <param name="RequestedBytes">The number of bytes to process. 
		/// The bytes number given here can be less than the length of the stream.</param>
        /// <returns>A new <see cref="System.Byte"/>[] if the method succeeds; otherwise , null.</returns>
        public static System.Byte[] D_ReadBufferedInternalTarget(System.IO.Stream Stream , System.Int64 RequestedBytes)
		{
			// Check for null conditions or whether we can read from this stream
			if (Stream == null) { return null; }
			if (Stream.CanRead == false) { return null; }
			// Create a new byte array with the requested size.
			System.Byte[] Contents = new System.Byte[RequestedBytes];
            if (RequestedBytes <= BUFSIZE)
            {
                // Read all bytes directly , if the requested bytes are less than the buffer limit.
				// Otherwise we don't care here; we do not read thousands or millions of bytes.
                Stream.Read(Contents, 0, Contents.Length);
            } else
            {
                System.Int32 Count;
                System.Int32 Offset = 0;
                // Read all bytes with buffered mode.
                do
                {
                    Count = Stream.Read(Contents, Offset, BUFSIZE);
                    Offset += BUFSIZE;
                    // Condition specifies that the loop will continue to run when the read bytes are
                    // more or equal than the buffer limit , plus make sure that the next read will not
                    // surpass the bytes that the final array can hold.
                } while ((Count >= BUFSIZE) && (Offset + BUFSIZE <= Contents.Length));
                // In case that the bytes were surpassed in the above condition , pass all the rest bytes again normally.
                if (Contents.Length - Offset > 0) { Stream.Read(Contents, Offset, Contents.Length - Offset); }
            }
			return Contents;
        }

		/// <summary>
		/// Internal adapter method that picks out the proper hash algorithm provided from a <see cref="System.String"/>. <br />
		/// Used for <see cref="MAIN.GetACryptographyHashForAFile(string, string)"/> method.
		/// </summary>
		/// <param name="Alg">The alogrithm string that will be picked out. It is a case-insensitive <see cref="System.String"/>.</param>
		/// <returns>A <see cref="System.Security.Cryptography.HashAlgorithm"/> that represents the requested algorithm.</returns>
		public static System.Security.Cryptography.HashAlgorithm D_GetACryptographyHashForAFile_2(System.String Alg)
		{
			Alg = Alg.ToUpperInvariant();
            System.Int32 I = 0;
			System.Security.Cryptography.HashAlgorithm EDI = null;
            System.String[] D = System.Enum.GetNames(typeof(HashDigestSelection));
			System.Array D_T = System.Enum.GetValues(typeof(HashDigestSelection));
			System.Int32[] D_1 = new System.Int32[D_T.Length];
			for (; I < D_T.Length; I++) { D_1[I] = (System.Int32)D_T.GetValue(I); }
			D_T = null;
			I = 0;
			System.Boolean Flag = true;
			while (Flag && I < D.Length)
			{
				if (D[I] == Alg)
				{
					Flag = false;
					EDI = D_GetACryptographyHashForAFile_1((HashDigestSelection)D_1[I]);
				}
				I++;
			}
			if (Flag) { return null; } else { return EDI; }
        }

        /// <summary>
        /// Internal adapter method that picks out the proper hash algorithm provided from a <see cref="HashDigestSelection"/> field. <br />
        /// Used for <see cref="MAIN.GetACryptographyHashForAFile(string, HashDigestSelection)"/> method.
        /// </summary>
        /// <param name="HDS">The algorithm to use , provided by the <see cref="HashDigestSelection"/> enumeration.</param>
        /// <returns>A <see cref="System.Security.Cryptography.HashAlgorithm"/> that represents the requested algorithm.</returns>
        public static System.Security.Cryptography.HashAlgorithm D_GetACryptographyHashForAFile_1(HashDigestSelection HDS) 
		{
            System.Security.Cryptography.HashAlgorithm EDI = null;
			try
			{
				switch (HDS)
				{
					case HashDigestSelection.SHA1: EDI = System.Security.Cryptography.SHA1.Create(); break;
					case HashDigestSelection.SHA256: EDI = System.Security.Cryptography.SHA256.Create(); break;
					case HashDigestSelection.SHA384: EDI = System.Security.Cryptography.SHA384.Create(); break;
					case HashDigestSelection.SHA512: EDI = System.Security.Cryptography.SHA512.Create(); break;
					case HashDigestSelection.MD5: EDI = System.Security.Cryptography.MD5.Create(); break;
					case HashDigestSelection.SHA224: EDI = CryptographicOperations.SHA224.Create(); break;
					case HashDigestSelection.MD2: EDI = CryptographicOperations.MD2.Create(); break;
					case HashDigestSelection.MD4: EDI = CryptographicOperations.MD4.Create(); break;
					default:
						return null;
				}
			} catch (System.Reflection.TargetInvocationException) { return null; }
            return EDI;
        }

#if (NET472_OR_GREATER || NET7_0_OR_GREATER) && WPFExists

        public static System.String GetAStringFromTheUserNewInternal(System.String Prompt, System.String Title , System.String DefaultResponse)
		{
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.GetAStringFromTheUserNew({Prompt} , {Title} , {DefaultResponse})) CREATE: Dialog");
#endif
            IntuitiveInteraction.GetAStringFromTheUser DZ = new(Prompt, Title, DefaultResponse);
            try
            {
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
            finally { DZ.Dispose(); }
        }

        public static System.Int32 NewMessageBoxToUserInternal(System.String MessageString, System.String Title,
        MessageBoxButtons MessageButton = 0,
        MessageBoxIcon MessageIcon = 0)
        {
#if DEBUG
			Debugger.DebuggingInfo($"(in ROOT.MAIN.NewMessageBoxToUser({MessageString} , {Title} , {MessageButton} , {MessageIcon}))" +
				" INFO: Calling native method instead. Deferral will NOT pass from the custom message handler.");
#endif
            return MessageBox.Show(MessageString, Title, MessageButton, MessageIcon) switch
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

        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Int32 NewMessageBoxToUserInternal(System.String MessageString, System.String Title,
            ROOT.IntuitiveInteraction.ButtonSelection MessageButton, ROOT.IntuitiveInteraction.IconSelection IconToSelect)
        {
#if DEBUG
            Debugger.DebuggingInfo($"(in ROOT.MAIN.NewMessageBoxToUser({MessageString} , {Title} , {MessageButton} , {IconToSelect}))" +
                " INFO: Calling Internal class instead , because of the programmer's intention.");
#endif
            return new IntuitiveInteraction.IntuitiveMessageBox(MessageString,
                Title, MessageButton, IconToSelect).ButtonSelected switch
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

        public static DialogsReturner CreateLoadDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle, System.String DirToPresent)
        {
            DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = FileDialogType.LoadFile;
            if (System.String.IsNullOrEmpty(DirToPresent))
            {
                EDOut.ErrorCode = "Error";
                return EDOut;
            }
            if (MAIN.DirExists(DirToPresent) == false)
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

        public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle, System.String DirToPresent)
        {
            DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = FileDialogType.CreateFile;
            if (System.String.IsNullOrEmpty(DirToPresent))
            {
                EDOut.ErrorCode = "Error";
                return EDOut;
            }
            if (MAIN.DirExists(DirToPresent) == false)
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

        public static DialogsReturner CreateLoadDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
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

        public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
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

        public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
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

        public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
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

        public static DialogsReturner GetADirDialogInternal(System.Environment.SpecialFolder DirToPresent, System.String DialogWindowTitle)
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

        public static DialogsReturner GetADirDialogInternal(System.Environment.SpecialFolder DirToPresent, System.String DialogWindowTitle, System.String AlternateDir)
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

#else

		public static System.String GetAStringFromTheUserNewInternal(System.String Prompt, System.String Title , System.String DefaultResponse)
		{ throw ThrowNotSupportedPlatform_WPFParts(); }

		public static System.Int32 NewMessageBoxToUserInternal(System.String MessageString, System.String Title,
        MessageBoxButtons MessageButton = 0,
        MessageBoxIcon MessageIcon = 0)
		{ throw ThrowNotSupportedPlatform_WPFParts(); }

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static System.Int32 NewMessageBoxToUserInternal(System.String MessageString, System.String Title,
            ROOT.IntuitiveInteraction.ButtonSelection MessageButton, ROOT.IntuitiveInteraction.IconSelection IconToSelect)
			{ throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner CreateLoadDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle, System.String DirToPresent) { throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle, System.String DirToPresent) { throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner CreateLoadDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle) {  throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle) { throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle, System.Boolean FileMustExist, System.String DirToPresent)
		{ throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner CreateSaveDialogInternal(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle, System.Boolean FileMustExist) { throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner GetADirDialogInternal(System.Environment.SpecialFolder DirToPresent, 
		System.String DialogWindowTitle) { throw ThrowNotSupportedPlatform_WPFParts(); }

		public static DialogsReturner GetADirDialogInternal(System.Environment.SpecialFolder DirToPresent, 
		System.String DialogWindowTitle, System.String AlternateDir) { throw ThrowNotSupportedPlatform_WPFParts(); }

#endif
    }

	/// <summary>
	/// This enumeration has valid System links that exist across all Windows computers.
	/// </summary>
	[Serializable]
	[SupportedOSPlatform("windows")]
	public enum SystemLinks
	{
		/// <summary>
		/// Reserved for enumeration performance.
		/// </summary>
		[NonSerialized] None = 0,
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
	public struct FileSearchResult : System.IEquatable<FileSearchResult>
	{
		internal System.String Filepath;
		internal System.Boolean success;
		internal System.String ext;

		[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
		internal FileSearchResult(System.String Path, System.Boolean Successfull)
		{
			Filepath = Path;
			success = Successfull;
		}

		/// <inheritdoc />
        public override bool Equals(object obj) { return base.Equals(obj); }

        /// <inheritdoc />
        public bool Equals(FileSearchResult other) { return base.Equals(other); }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", 
			"CA1065:Do not raise exceptions in unexpected locations", 
			Justification = "Users must be prohibited from using this method.")]
        public override int GetHashCode() { throw new NotSupportedException("Users of this structure should" +
			" not rely on hash codes for equality."); }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", 
			"CA1031:Do not catch general exception types", 
			Justification = "This method must remain exceptionless")]
        internal void GetExtension()
		{
			if (Filepath != null)
			{
				// The try clause is added because the below method can throw exceptions.
				try
				{
					ext = Filepath.Substring(Filepath.LastIndexOf('.') + 1);
				}
				catch { return; }
			}
		}

		/// <summary>
		/// Determines whether two <see cref="FileSearchResult"/> instances are considered equal.
		/// </summary>
		/// <param name="o">The first instance to compare.</param>
		/// <param name="a">The second instance to compare.</param>
		/// <returns>A <see cref="System.Boolean"/> value determining the equality of those two instances.</returns>
		public static bool operator ==(FileSearchResult o , FileSearchResult a) { return Equals(o, a); }

        /// <summary>
        /// Determines whether two <see cref="FileSearchResult"/> instances are considered inequal.
        /// </summary>
        /// <param name="o">The first instance to compare.</param>
        /// <param name="a">The second instance to compare.</param>
        /// <returns>A <see cref="System.Boolean"/> value determining the equality of those two instances.</returns>
        public static bool operator !=(FileSearchResult o , FileSearchResult a) { return Equals(o, a) == false; }

		/// <summary>
		/// The file extension , without the dot.
		/// </summary>
		public System.String Extension { get { return ext; } }

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", 
		"CA1027:Mark enums with FlagsAttribute", 
		Justification = "File Dialog functions require an absolute value so as to work correctly.")]
	[SupportedOSPlatform("windows")]
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


#if NET472_OR_GREATER || NET7_0_OR_GREATER

	// Found that the File Dialogs work the same even when 
	// these run under .NET Standard.
	// Weird that they work the same , though.

	/// <summary>
	/// A storage class used by the file/dir dialogs to access the paths given (Full and name only) , the dialog type ran and if there was an error.		
	/// </summary>
	/// <remarks>This class is used only by several methods in the <see cref="MAIN"/> class. It is not allowed to override this class.</remarks>
	[SupportedOSPlatform("windows")]
	public struct DialogsReturner : System.IEquatable<DialogsReturner>
	{
		private System.String ERC;
		private System.String FNM;
		private System.String FNMFP;
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

		/// <inheritdoc />
        public override bool Equals(object obj) { return base.Equals(obj); }

        /// <inheritdoc />
        public bool Equals(DialogsReturner other) { return Equals((System.Object)other); }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", 
			"CA1065:Do not raise exceptions in unexpected locations", 
			Justification = "This method is not supported in DialogsReturner.")]
        public override int GetHashCode()
        {
            throw new NotSupportedException("Hash codes for equality should be prohibited for this type.");
        }

        /// <inheritdoc />
        public static bool operator ==(DialogsReturner left, DialogsReturner right) { return left.Equals(right); }

        /// <inheritdoc />
        public static bool operator !=(DialogsReturner left, DialogsReturner right) { return Equals(left ,right) == false; }
    }

#endif

	/// <summary>
	/// An enumeration of values which help the function <see cref="MAIN.GetACryptographyHashForAFile(string, HashDigestSelection)"/> to properly select the algorithm requested.
	/// </summary>
	[Serializable]
	public enum HashDigestSelection
	{
		/// <summary> RSVD </summary>
		[NonSerialized] None = 0,
        /// <summary> Reserved for future use. </summary>
        [NonSerialized] RSVD0 = 1,
        /// <summary> Reserved for future use. </summary>
        [NonSerialized] RSVD1 = 2,
        /// <summary> Reserved for future use. </summary>
        [NonSerialized] RSVD2 = 3,
        /// <summary> Reserved for future use. </summary>
        [NonSerialized] RSVD3 = 4,
		/// <summary>The SHA1 Digest will be used.</summary>
		/// <remarks>Microsoft has detected that the algorithm produces the same result in slightly different files.
		/// If your case is the integrity , you should use then the <see cref="HashDigestSelection.SHA256"/> or a better algorithm.</remarks>
		SHA1 = 5,
		/// <summary>The SHA256 Digest will be used.</summary>
		SHA256 = 6,
		/// <summary>The SHA384 Digest will be used.</summary>
		SHA384 = 7,
		/// <summary>The SHA512 Digest will be used.</summary>
		SHA512 = 8,
		/// <summary>The MD5 Digest will be used.</summary>
		MD5 = 9,
        /// <summary>The internal SHA224 Digest will be used.</summary>
        [RequiresPreviewFeatures] SHA224 = 10,
        /// <summary>The internal MD2 Digest will be used.</summary>
        [RequiresPreviewFeatures] MD2 = 11,
        /// <summary>The internal MD4 Digest will be used.</summary>
        [RequiresPreviewFeatures] MD4 = 12
	}

	/// <summary>
	/// This enumeration defines constants to use for the <see cref="MAIN"/> API's that 
	/// utilise the <see cref="System.Text.Encoding"/> class for encoding operations.
	/// </summary>
	[Serializable]
	[RequiresPreviewFeatures]
	public enum APIEncodingOptions
	{
		/// <summary>UTF-8 Encoding without byte-order mark will be used.</summary>
		UTF8 = 1,
        /// <summary>UTF-8 Encoding with byte-order mark will be used.</summary>
        UTF8BOM,
        /// <summary>UTF-16 Big-Endian Encoding without byte-order mark will be used.</summary>
        UTF16,
        /// <summary>UTF-32 Big-Endian Encoding without byte-order mark will be used.</summary>
        UTF32,
        /// <summary>ASCII Encoding will be used.</summary>
        ASCII,
        /// <summary>UTF-16 Little-Endian Encoding without byte-order mark will be used.</summary>
        UTF16LE,
        /// <summary>UTF-16 Little-Endian Encoding with byte-order mark will be used.</summary>
        UTF16LEBOM,
        /// <summary>UTF-16 Big-Endian Encoding with byte-order mark will be used.</summary>
        UTF16BOM,
        /// <summary>UTF-32 Little-Endian Encoding without byte-order mark will be used.</summary>
        UTF32LE,
        /// <summary>UTF-32 Little-Endian Encoding with byte-order mark will be used.</summary>
        UTF32LEBOM,
        /// <summary>UTF-32 Big-Endian Encoding with byte-order mark will be used.</summary>
        UTF32BOM
    }

}