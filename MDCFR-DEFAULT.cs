// An All-In-One framework abstracting the most important classes that are used in .NET
// that are more easily and more consistently to be used.
// The framework was designed to host many different operations , with the last goal 
// to be everything accessible for everyone.

// Global namespaces
using ROOT;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Compression;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ROOT
{
    // A Collection Namespace which includes Microsoft's Managed code.
    // Many methods here , however , are controlled and built by me at all.

	/// <summary>
	/// Contains a lot and different static methods for different usages.
	/// </summary>
    public static class MAIN 
	{

#if NET472_OR_GREATER

        // Found that the File Dialogs work the same even when 
        // these run under .NET Standard.
        // Weird that they work the same , though.

        /// <summary>
        /// A storage class used by the file/dir dialogs to access the paths given (Full and name only) , the dialog type ran and if there was an error.		
        /// </summary>
        /// <remarks>This class is used only by several functions in the MAIN class. It is not allowed to override this class.</remarks>
        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public class DialogsReturner
		{
			private string ERC;
			private string FNM;
			private string FNMFP;
			private FileDialogType FT;
			private System.String FTD;

			public enum FileDialogType : System.Int32
			{
				CreateFile = 0,
				LoadFile = 2,
				DirSelect = 4
			}

			public FileDialogType DialogType
			{ 
				get { return FT; } 
				set { FT = value; }
			}
			
			public string ErrorCode
			{
				get { return ERC; }
				set { ERC = value; }
			}
			
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static System.String GetFilePathFromInvokedDialog(DialogsReturner DG) { return DG.FileNameFullPath; }
        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static System.Boolean GetLastErrorFromInvokedDialog(DialogsReturner DG) { if (DG.ErrorCode == "Error") { return false; } else { return true; } }

		public static class IntuitiveConsoleText
		{
			public static void InfoText(System.String Text)
			{
				System.ConsoleColor FORE , BACK;
				FORE = System.Console.ForegroundColor;
				BACK = System.Console.BackgroundColor;
				System.Console.ForegroundColor = System.ConsoleColor.Gray;
				System.Console.BackgroundColor = System.ConsoleColor.Black;
				System.Console.WriteLine(@"INFO: " + Text);
				System.Console.ForegroundColor = FORE;
				System.Console.BackgroundColor = BACK;
			}
			
			public static void WarningText(System.String Text)
			{
				System.ConsoleColor FORE , BACK;
				FORE = System.Console.ForegroundColor;
				BACK = System.Console.BackgroundColor;
				System.Console.ForegroundColor = System.ConsoleColor.Yellow;
				System.Console.BackgroundColor = System.ConsoleColor.Black;
				System.Console.WriteLine(@"WARNING: " + Text);
				System.Console.ForegroundColor = FORE;
				System.Console.BackgroundColor = BACK;
			}
			
			public static void ErrorText(System.String Text)
			{
				System.ConsoleColor FORE , BACK;
				FORE = System.Console.ForegroundColor;
				BACK = System.Console.BackgroundColor;
				System.Console.ForegroundColor = System.ConsoleColor.Red;
				System.Console.BackgroundColor = System.ConsoleColor.Black;
				System.Console.WriteLine(@"ERROR: " + Text);
				System.Console.ForegroundColor = FORE;
				System.Console.BackgroundColor = BACK;
			}
			
			public static void FatalText(System.String Text)
			{
				System.ConsoleColor FORE , BACK;
				FORE = System.Console.ForegroundColor;
				BACK = System.Console.BackgroundColor;
				System.Console.ForegroundColor = System.ConsoleColor.Magenta;
				System.Console.BackgroundColor = System.ConsoleColor.Black;
				System.Console.WriteLine(@"FATAL: " + Text);
				System.Console.ForegroundColor = FORE;
				System.Console.BackgroundColor = BACK;
			}
			
		}

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static void EmitBeepSound() { Microsoft.VisualBasic.Interaction.Beep(); }
		
		public static void WriteConsoleText(System.String Text) { System.Console.WriteLine(Text); }

		public static void WriteConsoleText(System.Char[] Text) 
		{
			System.String Result = null;
			for (System.Int32 D = 0; D < Text.Length; D++)
			{
				Result += Text[D];
			}
			System.Console.WriteLine(Result);
		}
		
		public static void WriteCustomColoredText(System.String Message , System.ConsoleColor ForegroundColor , System.ConsoleColor BackgroundColor)
		{
			System.ConsoleColor FORE , BACK;
			FORE = System.Console.ForegroundColor;
			BACK = System.Console.BackgroundColor;
			System.Console.ForegroundColor = ForegroundColor;
			System.Console.BackgroundColor = BackgroundColor;
			System.Console.WriteLine(Message);
			System.Console.ForegroundColor = FORE;
			System.Console.BackgroundColor = BACK;
		}
		
		public static System.String GetVBRuntimeInfo()
		{
			return Microsoft.VisualBasic.Globals.ScriptEngine + " Engine , Version " + 
			Microsoft.VisualBasic.Globals.ScriptEngineMajorVersion.ToString() + "." +
			Microsoft.VisualBasic.Globals.ScriptEngineMinorVersion.ToString() +
			"." + Microsoft.VisualBasic.Globals.ScriptEngineBuildVersion.ToString();
		}

#if NET472_OR_GREATER

        public static System.String GetRuntimeVersion() { return System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion(); }

        public static System.Boolean CheckIfStartedFromSpecifiedOS(System.Runtime.InteropServices.OSPlatform OSP)
        { return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSP); }

        public static System.String OSInformation() { return System.Runtime.InteropServices.RuntimeInformation.OSDescription; }

		public static System.String OSFramework() {return System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription; }

		public static System.String OSProcessorArchitecture()
		{
			System.Runtime.InteropServices.Architecture RRD = System.Runtime.InteropServices.RuntimeInformation.OSArchitecture;
			if (System.Runtime.InteropServices.Architecture.X86 == RRD) { return "x86"; } 
			else if (System.Runtime.InteropServices.Architecture.X64 == RRD) { return "AMD64"; }
			else if (System.Runtime.InteropServices.Architecture.Arm == RRD) { return "ARM"; }
			else if (System.Runtime.InteropServices.Architecture.Arm64 == RRD) { return "ARM64"; } else { return "Error"; }
		}

		public static System.String ProcessArchitecture()
		{
            System.Runtime.InteropServices.Architecture RRD = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
            if (System.Runtime.InteropServices.Architecture.X86 == RRD) { return "x86"; }
            else if (System.Runtime.InteropServices.Architecture.X64 == RRD) { return "AMD64"; }
            else if (System.Runtime.InteropServices.Architecture.Arm == RRD) { return "ARM"; }
            else if (System.Runtime.InteropServices.Architecture.Arm64 == RRD) { return "ARM64"; } else { return "Error"; }
        }

#endif 

		public static System.String RemoveDefinedChars(System.String StringToClear , System.Char[] CharToClear)
		{
			System.Char[] CharString = StringToClear.ToCharArray();
			if (CharString.Length <= 0 ) { return null; }
			if (CharToClear.Length <= 0) { return null; }
            System.Boolean Keeper = false;
            System.String Result = null;
            for (System.Int32 ITER = 1; ITER < CharString.Length; ITER++)
            {
                for (System.Int32 ILGITER = 1; ILGITER < CharToClear.Length; ILGITER++)
                {
                    if (CharString[ITER] == CharToClear[ILGITER])
                    {
                        Keeper = true;
                        break;
                    }
                }
                if (Keeper == false)
                {
                    Result += CharString[ITER];
                }
                else { Keeper = false; }
            }
            CharString = null;
            CharToClear = null;
			return Result;
        }

		[Notice("GetAStringFromTheUser" , "GetAStringFromTheUserNew")]
		public static System.String GetAStringFromTheUser(System.String Prompt, 
		System.String Title, System.String DefaultResponse)
		{
			System.String RETVAL = Microsoft.VisualBasic.Interaction.InputBox(Prompt , Title , DefaultResponse);
			if (RETVAL == "")
			{
				return null;
			} 
			else
			{
				return RETVAL;
			}
		}

		public static System.String GetAStringFromTheUserNew(System.String Prompt,
        System.String Title, System.String DefaultResponse)
		{
			IntuitiveInteraction.GetAStringFromTheUser DZ = new(Prompt ,Title , DefaultResponse);
			switch (DZ.ButtonClicked) 
			{
				case ROOT.IntuitiveInteraction.ButtonReturned.NotAnAnswer: return null;
				default: return DZ.ValueReturned;
			}
		}


        public static System.Boolean FileExists(System.String Path) { if (System.IO.File.Exists(Path)) { return true; } else { return false; } }
		
		public static System.Boolean DirExists(System.String Path)
		{
			if (System.IO.Directory.Exists(@Path))
			{
				return true;
			}
			else 
			{
				return false;
			}
		}
		
		public static System.Boolean CreateADir(System.String Path)
		{
			try
			{
				System.IO.Directory.CreateDirectory(@Path);
			}
			catch (System.Exception)
			{
				return false;
			}
			return true;
		}
		
		public static System.Boolean DeleteADir(System.String Path , System.Boolean DeleteAll)
		{
			try
			{
				System.IO.Directory.Delete(@Path , DeleteAll);
			}
			catch (System.Exception)
			{
				return false;
			}
			return true;
		}
		
		public static System.Boolean MoveFilesOrDirs(System.String SourcePath , System.String DestPath)
		{
			try 
			{
				System.IO.Directory.Move(SourcePath , DestPath);
			}
			catch (System.Exception)
			{
				return false;
			}
			return true;
		}
		
		public static System.Boolean CopyFile(System.String SourceFilePath ,
        System.String DestPath ,System.Boolean OverWriteAllowed = false)
		{
			try
			{
				System.IO.File.Copy(SourceFilePath , DestPath , OverWriteAllowed);
			}
			catch (System.Exception)
			{
				return false;
			}
			return true;
		}
		
		public static System.IO.FileSystemInfo[] GetANewFileSystemInfo(System.String Path)
		{
			if (! DirExists(Path))
			{
				return null;
			}
			try
			{
				System.IO.DirectoryInfo RFD = new System.IO.DirectoryInfo(Path);
				return RFD.GetFileSystemInfos();
			}
			catch (System.Exception)
			{
				return null;
			}
		}
		
		public static System.IO.FileStream CreateANewFile(System.String Path)
		{
			try 
			{
				return System.IO.File.OpenWrite(Path);
			}
			catch (System.Exception)
			{
				return null;
			}
		}
		
		public static System.IO.FileStream ReadAFileUsingFileStream(System.String Path)
		{
			if (! FileExists(Path)) { return null; }
			try 
			{
				return System.IO.File.Open(Path , System.IO.FileMode.Open);
			}
			catch (System.Exception)
			{
				return null;
			}
		}
		
		public static System.IO.FileStream ClearAndWriteAFile(System.String Path)
		{
			if (! FileExists(Path)) {return null;}
			try
			{
				return System.IO.File.Open(Path , System.IO.FileMode.Truncate);
			} catch (System.Exception)
			{
				return null;
			}
		}
		
		public static void PassNewContentsToFile(System.String Contents,System.IO.FileStream FileStreamObject)
		{
			System.Byte[] EMDK = new System.Text.UTF8Encoding(true).GetBytes(Contents + System.Environment.NewLine);
			FileStreamObject.Write(EMDK , 0 ,EMDK.Length);
			EMDK = null;
			return;
		}

		public static void AppendNewContentsToFile(System.String Contents, System.IO.FileStream FileStreamObject)
		{
            System.Byte[] EMDK = new System.Text.UTF8Encoding(true).GetBytes(Contents);
			try
			{
				FileStreamObject.Write(EMDK, System.Convert.ToInt32(FileStreamObject.Length), EMDK.Length);
			} catch (System.Exception) {}
			EMDK = null;
            return;
        }

		public static System.String GetContentsFromFile(System.IO.FileStream FileStreamObject)
		{
			System.Byte[] EMS = new System.Byte[FileStreamObject.Length];
			FileStreamObject.Read(EMS , 0 , System.Convert.ToInt32(FileStreamObject.Length));
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
		/// otherwise , the <see cref="System.String" /> <code>"Error"</code>. </returns>
        public static System.String GetACryptographyHashForAFile(System.String PathOfFile, 
			HashDigestSelection HashToSelect)
		{
			System.Byte[] Contents = null;
			using System.IO.FileStream Initialiser = ReadAFileUsingFileStream(PathOfFile);
			{
                if (Initialiser == null)
                {
                    return "Error";
                }
                Initialiser.Read(Contents , 0 , Contents.Length);
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
            for (System.Int32 ITER = 0; ITER <= RSS.Length - 1; ITER++)
            {
                Result += RSS[ITER].ToString("x2");
            }
            return Result;
        }

        /// <summary>
        /// This method gets a cryptography hash for the selected file with the selected algorithm.
        /// </summary>
        /// <param name="PathOfFile">The path to the file you want it's hash.</param>
        /// <param name="HashToSelect">The hash algorithm to select. Valid Values: <code>"SHA1" , "SHA256" , "SHA384" , "SHA512" , "MD5"</code></param>
        /// <returns>The computed hash as a hexadecimal <see cref="System.String" /> if succeeded; 
		/// otherwise , the <see cref="System.String" /> <code>"Error"</code>. </returns>
        public static System.String GetACryptographyHashForAFile(System.String PathOfFile , System.String HashToSelect)
		{
            System.Byte[] Contents = null;
            using System.IO.FileStream Initialiser = ReadAFileUsingFileStream(PathOfFile);
            {
                if (Initialiser == null)
                {
                    return "Error";
                }
                Initialiser.Read(Contents, 0, Contents.Length);
            }
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
			for (System.Int32 ITER = 0; ITER <= RSS.Length - 1; ITER++)
			{
				Result += RSS[ITER].ToString("x2");
			}
			return Result;
		}

		[System.ObsoleteAttribute("AppendAnExistingFile method is no longer required , because using ReadAFileUsingFileStream will normally open it with R/W " +
            "permissions. Use then the AppendNewContentsToFile function to append data to that opened file." , true)]
		public static System.IO.StreamWriter AppendAnExistingFile(System.String Path)
		{
			try 
			{
				return System.IO.File.AppendText(Path);
			}
			catch (System.Exception)
			{
				return null;
			}
		}
		
		public static System.Boolean DeleteAFile(System.String Path)
		{
			if (! FileExists(Path)) {return false;}
			try { System.IO.File.Delete(Path); } catch (System.Exception) { return false; }
			return true;
		}

		[System.ObsoleteAttribute("ReadAFile method has been replaced with the ReadAFileUsingFileStream function , which performs better at performance level." +
			"You should notice that sometime this function will be removed without prior notice." , false)]
		public static System.IO.StreamReader ReadAFile(System.String Path)
		{
			if (! FileExists(Path))
			{
				return null;
			}
			try
			{
				return new System.IO.StreamReader(new System.IO.FileStream(Path , System.IO.FileMode.Open));
			}
			catch (System.Exception)
			{
				return null;
			}
		}

        [System.ObsoleteAttribute("GetFileContentsFromStreamReader method has been replaced with the GetContentsFromFile function , which performs better at performance level." +
            "You should notice that sometime this function will be removed without prior notice.", false)]
        public static System.String GetFileContentsFromStreamReader(System.IO.StreamReader FileStream)
		{
			System.String ReturnableString = null;
			try
			{
				while (FileStream.Peek() >= 0)
				{
					ReturnableString += FileStream.ReadLine() + System.Environment.NewLine;
				}
				return ReturnableString;
			}
			catch (System.Exception)
			{
				return null;
			}
		}

#if NET472_OR_GREATER

        /// <summary>
        /// This shows the default Windows Message box on the screen.
        /// </summary>
        /// <param name="MessageString">The text for the message to show.</param>
        /// <param name="Title">The window's title.</param>
        /// <param name="MessageButton">The button(s) to show as options in the message box.</param>
        /// <param name="MessageIcon">The icon to show as a prompt in the message box.</param>
        /// <returns>An <see cref="System.Int32"/> which indicates which button the user pressed.</returns>
        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static System.Int32 NewMessageBoxToUser(System.String MessageString , System.String Title , 
        System.Windows.Forms.MessageBoxButtons MessageButton = 0 , 
        System.Windows.Forms.MessageBoxIcon MessageIcon = 0)
		{
			System.Windows.Forms.DialogResult RET = System.Windows.Forms.MessageBox.Show(MessageString , Title , MessageButton , MessageIcon);
			switch (RET)
			{
				case System.Windows.Forms.DialogResult.OK:
                    return 1;
                case System.Windows.Forms.DialogResult.Cancel:
                    return 2;
                case System.Windows.Forms.DialogResult.Abort:
                    return 3;
                case System.Windows.Forms.DialogResult.Retry:
                    return 4;
                case System.Windows.Forms.DialogResult.Ignore:
                    return 5;
                case System.Windows.Forms.DialogResult.Yes:
                    return 6;
                case System.Windows.Forms.DialogResult.No:
                    return 7;
                default: 
                    return 0;
			}
		}

        /// <summary>
        /// This is a modified Windows Message box made by me.
		/// To customize the options and for more information , consult the <see cref="IntuitiveInteraction.IntuitiveMessageBox"/> class.
        /// </summary>
        /// <param name="MessageString">The text for the message to show.</param>
        /// <param name="Title">The window's title.</param>
        /// <param name="MessageButton">The button(s) to show as options in the message box.</param>
        /// <param name="MessageIcon">The icon to show as a prompt in the message box.</param>
        /// <returns>An <see cref="System.Int32"/> which indicates which button the user pressed.</returns>
		[SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static System.Int32 NewMessageBoxToUser(System.String MessageString, System.String Title, 
			ROOT.IntuitiveInteraction.ButtonSelection MessageButton , ROOT.IntuitiveInteraction.IconSelection IconToSelect)
		{
			ROOT.IntuitiveInteraction.IntuitiveMessageBox DX = new(MessageString , Title , MessageButton , IconToSelect);
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner CreateLoadDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle , System.String DirToPresent)
		{
            DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = DialogsReturner.FileDialogType.LoadFile;
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle , System.String DirToPresent)
        {
            DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = DialogsReturner.FileDialogType.CreateFile;
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner CreateLoadDialog(System.String FileFilterOfWin32,System.String FileExtensionToPresent ,
		System.String FileDialogWindowTitle)
		{
			DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = DialogsReturner.FileDialogType.LoadFile;
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32,System.String FileExtensionToPresent ,
		System.String FileDialogWindowTitle)
		{
			DialogsReturner EDOut = new DialogsReturner();
			EDOut.DialogType = DialogsReturner.FileDialogType.CreateFile;
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle , System.Boolean FileMustExist , System.String DirToPresent)
        {
            DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = DialogsReturner.FileDialogType.CreateFile;
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner CreateSaveDialog(System.String FileFilterOfWin32, System.String FileExtensionToPresent,
        System.String FileDialogWindowTitle , System.Boolean FileMustExist)
        {
            DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = DialogsReturner.FileDialogType.CreateFile;
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent , System.String DialogWindowTitle)
		{
            DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = DialogsReturner.FileDialogType.DirSelect;
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent, System.String DialogWindowTitle ,System.String AlternateDir)
        {
            DialogsReturner EDOut = new DialogsReturner();
            EDOut.DialogType = DialogsReturner.FileDialogType.DirSelect;
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
        public static void HaltApplicationThread(System.Int32 TimeoutEpoch) { System.Threading.Thread.Sleep(TimeoutEpoch); }

		[SupportedOS(SupportedOSAttributePlatforms.Windows)]
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

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public static System.Int32 LaunchProcess(System.String PathOfExecToRun ,System.String CommandLineArgs , 
        System.Boolean ImplicitSearch ,System.Boolean WaitToClose ,System.Boolean? RunAtNativeConsole , 
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
			try {FRM.Start();} catch (System.ComponentModel.Win32Exception)
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
		public static System.String[] GetPathEnvironmentVar()
		{
			try
			{
				System.String[] RMF = (System.Environment.GetEnvironmentVariable("Path")).Split(';');
				return RMF;
			}
			catch (System.Exception)
			{
				return null;
			}
		}
		
	}

	/// <summary>
	/// An enumeration of values which help the function to properly select the algorithm requested.
	/// </summary>
	public enum HashDigestSelection
	{
		Default_None = 0,
		RSVD_0 = 1, RSVD_1 = 2,
		RSVD_2 = 3, RSVD_3 = 4,
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

	public enum RegTypes
	{
		ERROR = 0,
		RSVD_0 = 1,
		RSVD_1 = 2,
		RSVD_2 = 3,
		RSVD_3 = 4,
		RSVD_4 = 5,
	    String = 6,
		ExpandString = 7,
		QuadWord = 8,
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
		RSVD_0 = 1,
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
		RSVD_0 = 1,
		RSVD_1 = 2,
		HKLM = 3,
		HKCU = 4,
		HKCC = 5,
        HKPD = 6,
        HKU = 7,
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

    [SupportedOS(SupportedOSAttributePlatforms.Windows)]
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
					case "HKEY_LOCAL_MACHINE":  return RegRootKeyValues.HKLM;
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
		/// <param name="SubKey"></param>
		public RegEditor(RegRootKeyValues KeyValue , System.String SubKey)
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
		public System.Boolean DiagnosticMessages
		{
			set { _DIAG_ = value; }
		}
		
		private System.Boolean _CheckPredefinedProperties()
		{
			if ((System.String.IsNullOrEmpty(_RootKey_) == false) && (System.String.IsNullOrEmpty(_SubKey_) == false))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		/// <summary>
		/// Gets the specified value from the key provided.
		/// </summary>
		/// <param name="VariableRegistryMember">The value name to retrieve the value data.</param>
		/// <returns>If it succeeded , a new <see cref="System.Object"/> instance containing the data; Otherwise , a <see cref="System.String"/> explaining the error.</returns>
		public System.Object GetEntry(System.String VariableRegistryMember)
		{
			if (System.String.IsNullOrEmpty(VariableRegistryMember))
			{
				return "Error";
			}
			if (! _CheckPredefinedProperties())
			{
				if (_DIAG_) {System.Console.WriteLine("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined.");}
				return "UNDEF_ERR";
			}
			System.Object RegEntry = Microsoft.Win32.Registry.GetValue($"{_RootKey_}\\{_SubKey_}" , VariableRegistryMember , "_ER_C_");
			if (System.Convert.ToString(RegEntry) == "_ER_C_")
			{
				return "Error";
			}
			else
			{
				if (RegEntry is System.String[])
				{
					return RegEntry;
				} else if (RegEntry is System.Byte[])
				{
					return RegEntry;
				} else if (RegEntry is System.String)
				{
					return RegEntry;
				}
				else
				{
					if (_DIAG_)
					{
						System.Console.WriteLine("Error - Could not translate the object returned by the procedure.");
                        System.Console.WriteLine("Please check that the entry is not broken , incorrect or in format that is not supported by this editor.");
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
		public RegFunctionResult SetEntry(System.String VariableRegistryMember,RegTypes RegistryType,System.Object RegistryData)
		{
			if (System.String.IsNullOrEmpty(VariableRegistryMember))
			{
				return RegFunctionResult.Error;
			}
			if (! _CheckPredefinedProperties())
			{
				if (_DIAG_) {System.Console.WriteLine("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined.");}
				return RegFunctionResult.Misdefinition_Error;
			}
			if (RegistryData == null)
			{
				if (_DIAG_) {System.Console.WriteLine("ERROR: 'null' value detected in RegistryData object. Maybe invalid definition?"); }
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
					System.Console.WriteLine($"ERROR: Unknown registry value type argument in the object creator was given: {RegistryType}");
				}
				return RegFunctionResult.InvalidRootKey;
			}
			try
			{
				Microsoft.Win32.Registry.SetValue($"{_RootKey_}\\{_SubKey_}" , VariableRegistryMember , RegistryData , RegType_);
			}
			catch (System.Exception EX)
			{
				if (_DIAG_)
				{
					System.Console.WriteLine($"ERROR: Could not create key {VariableRegistryMember} . Invalid name maybe?");
				    System.Console.WriteLine($"Error Raw Data: {EX}");
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
			if (System.String.IsNullOrEmpty(VariableRegistryMember))
			{
				return RegFunctionResult.Error;
			}
			if (! _CheckPredefinedProperties())
			{
				if (_DIAG_) {System.Console.WriteLine("Error - Cannot initiate the Internal editor due to an error: Properties that point the searcher are undefined.");}
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
						System.Console.WriteLine("Error - Registry root key could not be get. Incorrect Root Key Detected.");
                        System.Console.WriteLine("Error while getting the root key: Root Key " + _RootKey_ + "Is invalid.");
					}
					return RegFunctionResult.Misdefinition_Error;
			}
			if (System.Convert.ToString(ValueDelete.GetValue(VariableRegistryMember , "_DNE_")) == "_DNE_")
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
		public void Dispose() {DisposeRes();}

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

        public class KeyGenTable
        {
            private System.String ERC;
            private System.Byte[] _IV_;
            private System.Byte[] _EK_;
            private System.Int32 EMF_;

            public System.String ErrorCode
            {
                get { return ERC; }
                set { ERC = value; }
            }

			public System.Boolean CallerErroredOut
			{
				get 
				{
					if (ERC == null) { return false; } else {
						if (ERC == "Error")
						{ return true; } else { return false; }
					}
				}
			}

            public System.Byte[] Key
            {
                get { return _EK_; }
                set { _EK_ = value; }
            }

            public System.Byte[] IV
            {
                get { return _IV_; }
                set { _IV_ = value; }
            }

            public System.Int32 KeyLengthInBits
            {
                get { return EMF_; }
                set { EMF_ = value; }
            }
        }

        public class AESEncryption : System.IDisposable
		{
			// Cryptographic Operations Class.
			private System.Byte[] _EncryptionKey_;
			private System.Byte[] _InitVec_;
			private System.Security.Cryptography.AesCng CNGBaseObject = new System.Security.Cryptography.AesCng();

			public System.Byte[] EncryptionKey
			{
				set { _EncryptionKey_ = value; }
			}

			public System.Byte[] IV
			{
				set { _InitVec_ = value; }
			}

			public static KeyGenTable MakeNewKeyAndInitVector()
			{
				System.Security.Cryptography.AesCng RETM;
				KeyGenTable RDM = new KeyGenTable();
				try
				{
					RETM = new System.Security.Cryptography.AesCng();
				}
				catch (System.Exception)
				{
					RDM.ErrorCode = "Error";
					return RDM;
				}
				RDM.ErrorCode = "OK";
				RDM.IV = RETM.IV;
				RDM.Key = RETM.Key;
				RDM.KeyLengthInBits = RETM.KeySize;
				return RDM;
			}

			private System.Boolean _CheckPredefinedProperties()
			{
				if ((_EncryptionKey_ is null) || (_InitVec_ is null))
				{
					return true;
				}
				if ((_EncryptionKey_.Length <= 0) || (_InitVec_.Length <= 0))
				{
					return true;
				}
				return false;
			}

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

			public System.Byte[] EncryptSpecifiedDataForFiles(System.IO.FileStream UnderlyingStream)
			{
				if (!(UnderlyingStream is System.IO.FileStream) || (UnderlyingStream.CanRead == false))
				{
					return null;
				}
				if (_CheckPredefinedProperties())
				{
					return null;
				}
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

			public System.String DecryptSpecifiedDataForFiles(System.IO.FileStream EncasingStream)
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
					using (System.IO.StreamReader SDE = new System.IO.StreamReader(DCryptStrEnc, System.Text.Encoding.UTF8))
					{
						FinalString = SDE.ReadToEnd();
					}
				}
				return FinalString;
			}

			public static System.String ConvertTextKeyOrIvToString(System.Byte[] ByteValue)
			{
				if ((ByteValue is null) || (ByteValue.Length <= 0))
				{
					return null;
				}
				try
				{
					return System.Convert.ToBase64String(ByteValue, 0, ByteValue.Length, 0);
				}
				catch (System.Exception)
				{
					return null;
				}
			}

			public static System.Byte[] ConvertTextKeyOrIvFromStringToByteArray(System.String StringValue)
			{
				if (System.String.IsNullOrEmpty(StringValue)) { return null; }
				try
				{
					return System.Convert.FromBase64String(StringValue);
				}
				catch (System.Exception)
				{
					return null;
				}
			}

			public void Dispose()
			{
				DISPMETHOD();
			}

			private protected void DISPMETHOD()
			{
				_EncryptionKey_ = null;
				_InitVec_ = null;
				#pragma warning disable CS0219
				System.Object CNGBaseObject = null;
				#pragma warning restore CS0219
			}
		}

		public class EDAFile
		{

			public class EncryptionContext
			{
				private System.String _ERC_;
				private System.Byte[] _KEY_;
				private System.Byte[] _IV_;

				public System.String ErrorCode
				{
					get { return _ERC_; }
					set { _ERC_ = value; }
				}

				public System.Byte[] KeyUsed
				{
					get { return _KEY_; }
					set { _KEY_ = value; }
				}

				public System.Byte[] InitVectorUsed
				{
					get { return _IV_; }
					set { _IV_ = value; }
				}
			}

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
					System.Console.WriteLine(EX.Message);
					DMF.ErrorCode = "Error";
					return DMF;
				}
				MDA.Dispose();
				DMF.KeyUsed = MAKER.Key;
				DMF.InitVectorUsed = MAKER.IV;
				return DMF;
			}

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
					System.Console.WriteLine(EX.Message);
					return;
				}
				MDA.Dispose();
				return;
			}

			// Executable code examples for encrypting and unencrypting files:
			// Executable code starts here: <--
			/// EDAFile.EncryptionContext RDF = EDAFile.EncryptAFile("E:\winrt\base.h" , "E:\IMAGES\winrtbase_h.Encrypted")
			/// EDAFile.DecryptAFile("E:\IMAGES\winrtbase_h.Encrypted" , RDF.KeyUsed , RDF.InitVectorUsed , "E:\IMAGES\Unencrypted-4664.h")
			// --> Executable code ended.
			// This is the simpliest way to encrypt and decrypt the files , but you can make use of the original AES API and make the encryption/decryption as you like to.
			// To Access that API , use APIFR.CryptographicOperations.AESEncryption .
			// More instructions on how to do such security conversions can be found in our Developing Website.
		}

	}

	namespace Archives
    {
        // A Collection Namespace for making and extracting archives.

        public class GZipArchives
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
				if (System.String.IsNullOrEmpty(Path)) {return false;}
				if (!(System.IO.File.Exists(Path))) {return false;}
				return true;
			}
			
			public static System.Boolean CompressTheSelectedFile(System.String FilePath ,System.String ArchivePath = null)
			{
				if (!(CheckFilePath(FilePath))) {return false;}
				System.String OutputFile;
				if (System.String.IsNullOrEmpty(ArchivePath))
				{
					System.IO.FileInfo FSIData = new System.IO.FileInfo(FilePath);
					OutputFile = FSIData.DirectoryName + @"\" + FSIData.Name + ".gz";
                    FSIData = null;
				}
				else 
				{
					OutputFile = ArchivePath;
				}
				System.IO.FileStream FSI;
				System.IO.FileStream FSO;
				try
				{
					FSI = System.IO.File.OpenRead(FilePath);
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
                    return false;
				}
				try
				{
					FSO = System.IO.File.OpenWrite(ArchivePath);
				}
				catch (System.Exception EX)
				{
					FSI.Close();
                    FSI.Dispose();
					System.Console.WriteLine(EX.Message);
                    return false;
				}
				try 
				{
					using (System.IO.Compression.GZipStream CMP = new System.IO.Compression.GZipStream(FSO , System.IO.Compression.CompressionMode.Compress))
					{
						FSI.CopyTo(CMP);
					}
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
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
			
			public static System.Boolean CompressAsFileStreams(System.IO.FileStream InputFileStream ,System.IO.FileStream OutputFileStream)
			{
				if (InputFileStream.CanRead == false) {return false;}
				if (OutputFileStream.CanWrite == false) {return false;}
				try 
				{
					using (System.IO.Compression.GZipStream CMP = new System.IO.Compression.GZipStream(OutputFileStream , System.IO.Compression.CompressionMode.Compress))
					{
						InputFileStream.CopyTo(CMP);
					}
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
                    return false;
				}
				return true;
			}
			
			public static System.Boolean DecompressTheSelectedFile(System.String ArchiveFile ,System.String OutputPath = null)
			{
				if (!(CheckFilePath(ArchiveFile))) {return false;}
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
					System.Console.WriteLine(EX.Message);
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
					System.Console.WriteLine(EX.Message);
                    return false;
				}
				try
				{
					using (System.IO.Compression.GZipStream DCMP = new System.IO.Compression.GZipStream(FSI , System.IO.Compression.CompressionMode.Decompress))
					{
						DCMP.CopyTo(FSO);
					}
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
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
			
			public static System.Boolean DecompressAsFileStreams(System.IO.FileStream ArchiveFileStream,System.IO.FileStream DecompressedFileStream)
			{
				if (ArchiveFileStream.CanRead == false) {return false;}
				if (DecompressedFileStream.CanWrite == false) {return false;}
				try
				{
					using (System.IO.Compression.GZipStream DCMP = new System.IO.Compression.GZipStream(ArchiveFileStream , System.IO.Compression.CompressionMode.Decompress))
					{
						DCMP.CopyTo(DecompressedFileStream);
					}
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
                    return false;
				}
				return true;
			}
			
		}
		
		public class ZipArchives
		{	
			public static System.Boolean ExtractZipFileToSpecifiedLocation(System.String PathOfZip,System.String PathToExtract)
			{
				if (!(System.IO.File.Exists(PathOfZip))) {return false;}
				if (!(System.IO.Directory.Exists(PathToExtract))) {return false;}
				try
				{
					System.IO.Compression.ZipFile.ExtractToDirectory(PathOfZip , PathToExtract);
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
					return false;
				}
				return true;
			}
			
			public static System.Boolean MakeZipFromDir(System.String PathOfZipToMake ,System.String PathToCollect)
			{
				if (System.String.IsNullOrEmpty(PathOfZipToMake)) {return false;}
				if (!(System.IO.Directory.Exists(PathToCollect))) {return false;}
				try
				{
					System.IO.Compression.ZipFile.CreateFromDirectory(PathToCollect , PathOfZipToMake);
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
					return false;
				}
				return true;
			}
			
			public static System.IO.Compression.ZipArchive InitZipFileStream(System.String PathofZipToCreate,System.IO.Compression.ZipArchiveMode ArchModeSelector)
			{
				if ((!(System.IO.File.Exists(PathofZipToCreate))) && (System.Convert.ToInt32(ArchModeSelector) != 2))
				{
					System.Console.WriteLine("Cannot Call 'InitZipFileStream' method with the argument ArchModeSelector set to " + ArchModeSelector + " and PathOfZipToMake: " + PathofZipToCreate + " resolves to False.");
                    return null;
				}
				try
				{
					return System.IO.Compression.ZipFile.Open(PathofZipToCreate , ArchModeSelector);
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
					return null;
				}
			}
			
			public static System.Boolean AddNewFileEntryToZip(System.String Path ,System.IO.Compression.ZipArchive ArchFileStream ,System.IO.Compression.CompressionLevel CompLevel)
			{
				if (!(System.IO.File.Exists(Path))) {return false;}
				System.IO.FileInfo RDF = new System.IO.FileInfo(Path);
				try
				{
					ArchFileStream.CreateEntryFromFile(RDF.FullName , RDF.Name , CompLevel);
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
					return false;
				}
				return true;
			}
			
			public static System.Boolean CreateZipArchiveViaFileSystemInfo(System.String PathofZipToCreate ,System.IO.FileSystemInfo[] InfoObject ,System.IO.Compression.CompressionLevel ENTCMPL )
			{
				if (!(System.IO.File.Exists(PathofZipToCreate))) {return false;}
				System.IO.FileStream Zipper = null;
				try
				{
					Zipper = new System.IO.FileStream(PathofZipToCreate , System.IO.FileMode.Open);
					using (System.IO.Compression.ZipArchive ArchZip = new System.IO.Compression.ZipArchive(Zipper , System.IO.Compression.ZipArchiveMode.Update))
					{
						foreach (System.IO.FileSystemInfo T in InfoObject)
						{
							if (T is System.IO.FileInfo)
							{
								ArchZip.CreateEntryFromFile(T.FullName , T.Name , ENTCMPL);
							}
						}
					}
				}
				catch (System.Exception EX)
				{
					System.Console.WriteLine(EX.Message);
					return false;
				}
				finally
				{
					Zipper.Close();
					Zipper.Dispose();
				}
				return true;
			}
			
		}

        [SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public class Cabinets
		{
			public static System.Boolean CompressFromDirectory(System.String DirToCapture , System.String OutputArchivePath)
			{ 
				if (! System.IO.Directory.Exists(DirToCapture)) { return false; }
				try
				{
					ExternalArchivingMethods.Cabinets.CabInfo CI = new(OutputArchivePath);
					System.IO.FileSystemInfo[] FileArray = MAIN.GetANewFileSystemInfo(DirToCapture);
					IList<System.String> FLT = new List<System.String>();
					foreach (System.IO.FileSystemInfo FI in FileArray)
					{
						if (FI is System.IO.FileInfo) { FLT.Add(FI.Name); }
					}
					CI.PackFiles(DirToCapture, FLT, FLT);
					CI.Refresh();
				} catch (System.Exception EX) 
				{
					System.Console.WriteLine(EX.Message);
					return false;
				}
				return true;
			}

			public static System.Boolean DecompressFromArchive(System.String DestDir , System.String ArchiveFile)
			{
				if (! System.IO.Directory.Exists(DestDir)) { return false; }
				if (! System.IO.File.Exists(ArchiveFile)) { return false; }
				try
				{
					ExternalArchivingMethods.Cabinets.CabInfo CI = new(ArchiveFile);
					CI.Unpack(DestDir);
				} catch (System.Exception EX) 
				{
					System.Console.WriteLine(EX.Message);
					return false;
				}
				return true;
			}

			public static System.Boolean AddAFileToCabinet(System.String FilePath ,  System.String CabinetFile)
			{
                if (! System.IO.File.Exists(CabinetFile)) { return false; }
                if (! System.IO.File.Exists(FilePath)) { return false; }
				try 
				{
					System.IO.FileInfo FI = new System.IO.FileInfo(FilePath);
					IList<System.String> IL = new List<System.String>();
					IL.Add(FI.Name);
					ExternalArchivingMethods.Cabinets.CabInfo CI = new (CabinetFile);
					CI.PackFiles(FI.DirectoryName ,IL , IL);
				} catch (System.Exception EX) 
				{
					System.Console.WriteLine(EX.Message);
					return false;
				}
				return true;
            }

		}
	}
	
	/// <summary>
	/// This is used to mark an function when it is used to generate a warning that will be deprecated.
	/// </summary>
	internal class NoticeAttribute : System.Attribute
	{
		public NoticeAttribute(System.String FunctionName) 
		{
			MAIN.WriteCustomColoredText($"Notice - the function {FunctionName} is no longer recommended " +
				" for usage and will be obsoleted in the next release. Use instead the other one recommended.",
				ConsoleColor.Red, ConsoleColor.Black);
		}

		public NoticeAttribute(System.String FunctionName , System.String Recommended)
		{
            MAIN.WriteCustomColoredText($"Notice - the function {FunctionName} is no longer recommended " +
                $" for usage and will be obsoleted in the next release. Use instead the {Recommended} function.",
                ConsoleColor.Red, ConsoleColor.Black);
        }

		public override string ToString() 
		{
			return "#NOTICEATTRIBUTE#";
		}
	}

	/// <summary>
	/// The Eumeration Values which describe in which platform the marked method can run.
	/// </summary>
	internal enum SupportedOSAttributePlatforms : System.Int32
	{
		Windows = 2 , OSX = 3, Linux = 4
	}

	/// <summary>
	/// This marks a method or class that can only be run in a specific platform. This does not yet work as expected , so you cannot use this class yet.
	/// </summary>
	/// <remarks>If the platform is not the one specified , then it throws a new <see cref="PlatformNotSupportedException"/>. </remarks>
	/// <seealso cref="SupportedOSAttributePlatforms"/>
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	internal class SupportedOSAttribute : System.Attribute
	{
		private Func<SupportedOSAttributePlatforms,System.String> DI = ((DD) => GETPLATFORM(DD));

		private static System.String GETPLATFORM(SupportedOSAttributePlatforms OSA)
		{
            System.Runtime.InteropServices.OSPlatform FinalPlatformSelection;
            switch (OSA)
            {
                case SupportedOSAttributePlatforms.Windows: FinalPlatformSelection = OSPlatform.Windows; break;
                case SupportedOSAttributePlatforms.Linux: FinalPlatformSelection = OSPlatform.Linux; break;
                case SupportedOSAttributePlatforms.OSX: FinalPlatformSelection = OSPlatform.OSX; break;
            }
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(FinalPlatformSelection) == false)
            {
                throw new System.PlatformNotSupportedException("This method , function , class or namespace cannot be executed" +
                $" on this type of platform: {Environment.OSVersion.Platform}");
            }
			return "Executed";
        }

		/// <summary>
		/// The default class constructor.
		/// </summary>
		/// <param name="OSPlatformRequired">The Platform <see cref="SupportedOSAttributePlatforms"/> by the developer that the code can run to.</param>
		/// <exception cref="System.PlatformNotSupportedException">It is thrown when the current running platform is not the one provided.</exception>
        public SupportedOSAttribute(SupportedOSAttributePlatforms OSPlatformRequired)
		{
			DI(OSPlatformRequired);
        }

        public override System.String ToString() { return "#SUPPORTEDOSATTRIBUTE#"; }
    }

	/// <summary>
	/// Calculates an estimated time required , for example , the time needed to execute a code excerpt.
	/// </summary>
	public class TimeCaculator : System.IDisposable
	{
		private System.DateTime _TimeEl_;
		private System.Boolean _Init_ = false;
		
		/// <summary>
		/// Use this method to clear and start counting.
		/// </summary>
		public void Init()
		{
			if (_Init_ == true) {return;}
			_TimeEl_ = Microsoft.VisualBasic.DateAndTime.Now;
			_Init_ = true;
			return;
		}
		
		/// <summary>
		/// Stop the counting and calculate the elapsed time.
		/// </summary>
		/// <returns>The time counted to milliseconds.</returns>
		public System.Int32 CaculateTime()
		{
			if (_Init_ == false) {return -1;}
			try
			{
				_Init_ = false;
				return System.Convert.ToInt32(Microsoft.VisualBasic.DateAndTime.Now.Subtract(_TimeEl_).TotalMilliseconds);
			}
			catch (System.Exception EX)
			{
				System.Console.WriteLine(EX.Message);
                return -1;
			}
		}
		
		/// <summary>
		/// Use the Dispose method to clear up the values so as to prepare it again to count up.
		/// </summary>
		public void Dispose() {DisposeResources();}
		
		private protected void DisposeResources()
		{
			#pragma warning disable CS0219
			System.Object _TimeEl_ = null;
			System.Object _Init_ = null;
			#pragma warning restore CS0219
		}
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
     * \\ System.Threading.Thread ClassThread = new Thread(new ThreadStart(ClassInitalisator.Invoke));
     * which will register the thread. Be noted that the thread must only be invoked by the Invoke() function.
     * Use then the ClassThread.Start();
     * to show the progress bar to the user.
     * use then each time ClassThread.UpdateProgress(); to update the progress indicator specified by the step given.
     * This will finished when the value presented is equal or more to the stop barrier(Which of this case is the end)
     * But this class has many , many other features ,like changing the progress message at executing time ,
     * breaking the bar before it ends , and setting the min/max values allowed and the step , which is also allowed to be
     * a negative number.
     *
     * I have also here an function example , which defines and controls the message itself.
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
    */

	/// <summary>
	/// A simple and to-the-point console progress bar class.
	/// </summary>
    public class SimpleProgressBar
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
            set
            { if (System.String.IsNullOrEmpty(value)) { throw new System.ArgumentException("Illegal , not allowed to be null."); } else { Progr = value; } }
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
            Progr = progressMessage;
            start = Start;
            stp = Step;
            end = End;
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
            Progr = progressMessage;
            stp = Step;
            end = End;
        }

        private class ProgressChangedArgs : System.EventArgs
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
		/// The Progress char that will be used inside the bar ([...])
		/// </summary>
        public System.Char ProgressChar
        {
            get { return Progc; }
            set
            {
                System.Char[] InvalidChars = new System.Char[] { '\a', '\b', '\\', '\'', '\"', '\r', '\n', '\0' };
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
        public System.Int32 ProgressStep
        {
            get { return stp; }
            set { stp = value; }
        }

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
            System.Console.Write($"{Progr}: {iterator}% [{Progm}]\r");
        }

		/// <summary>
		/// Indicates if the bar was executed. Use it to close the running thread.
		/// </summary>
        public System.Boolean Ended { get { return _Ended; } set { _Ended = value; } }

		/// <summary>
		/// Update the progress by the defined step.
		/// </summary>
        public void UpdateProgress()
        {
            if (_Ended == false)
            {
                ProgressChangedArgs DFV = new ProgressChangedArgs(iterator += stp);
                Progm += Progc;
                this.ChangeProgress?.Invoke(null, DFV);
            }
        }

        private void ChangeBar(System.Object sender, ProgressChangedArgs e)
        {
            System.Console.Write($"{Progr}: {e.ChangedValueTo}% [{Progm}] \r");
        }

		/// <summary>
		/// The function which starts up the Console Bar. This should only be used in a new <see cref="System.Threading.ThreadStart"/> delegate.
		/// </summary>
        public void Invoke()
        {
            this.ChangeProgress += ChangeBar;
            iterator = start;
            System.Console.Write($"{Progr}: {iterator}% [{Progm}]\r");
            do
            {
                if (iterator >= end) { _Ended = true; }
                System.Threading.Thread.Sleep(80);
            } while (_Ended == false);
            this.ChangeProgress -= ChangeBar;
            this.ChangeProgress = null;
            System.Console.WriteLine("\nCompleted.");
            return;
        }
    }
	
	namespace IntuitiveInteraction
	{
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

        /// <summary>
        /// A class that extends the default <see cref="Microsoft.VisualBasic.Interaction.InputBox"/> method.
        /// </summary>
		[SupportedOS(SupportedOSAttributePlatforms.Windows)]
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
		[SupportedOS(SupportedOSAttributePlatforms.Windows)]
        public class IntuitiveMessageBox : System.IDisposable
        {
            private System.String _MSG;
            private System.Windows.Forms.Label Label1 = new();
            private System.Windows.Forms.Label Label2 = new();
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
            public void Dispose()
            {
                if (ButtonHandle != null) { ButtonHandle -= Button_Click; }
                Label1.Dispose();
                Label2.Dispose();
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
            public ButtonSelection ButtonsToShow
            {
                get { return BSL; }
                set { BSL = value; }
            }

            /// <summary>
            /// The Icon that will be shown to the user. Can also be <see cref="IconSelection.None"/>.
            /// </summary>
            public IconSelection IconToShow
            {
                get { return SELI; }
                set { SELI = value; }
            }

            /// <summary>
            /// The message box title to show.
            /// </summary>
            public System.String Title
            {
                get { return _TITLE; }
                set { _TITLE = value; }
            }

			/// <summary>
			/// Returns the button selected by the User. It's value is one of the <see cref="ButtonReturned"/> <see cref="System.Enum"/> values.
			/// </summary>
            public ButtonReturned ButtonSelected
            {
                get { return BTR; }
            }

            private protected void Button_Click(System.Object sender, System.EventArgs e)
            {
                Menu.Close();
                if (BSL == 0)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 1; }
                }
                if (BSL == (ButtonSelection) 1)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 4; }
                    if (sender == Button2) { BTR = (ButtonReturned) 3; }
                }
                if (BSL == (ButtonSelection) 2)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 2; }
                    if (sender == Button2) { BTR = (ButtonReturned) 1; }
                }
                if (BSL == (ButtonSelection) 3)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 5; }
                    if (sender == Button2) { BTR = (ButtonReturned) 6; }
                }
                if (BSL == (ButtonSelection) 4)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 5; }
                    if (sender == Button2) { BTR = (ButtonReturned) 2; }
                }
                if (BSL == (ButtonSelection) 5)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 7; }
                    if (sender == Button2) { BTR = (ButtonReturned) 2; }
                }
                if (BSL == (ButtonSelection) 6)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 2; }
                    if (sender == Button2) { BTR = (ButtonReturned) 4; }
                    if (sender == Button3) { BTR = (ButtonReturned) 3; }
                }
                if (BSL == (ButtonSelection) 7)
                {
                    if (sender == Button1) { BTR = (ButtonReturned) 5; }
                    if (sender == Button2) { BTR = (ButtonReturned) 4; }
                    if (sender == Button3) { BTR = (ButtonReturned) 3; }
                }
                if (BSL == (ButtonSelection) 8)
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
            public void InvokeInstance()
            {
                MakeAndInitDialog(BSL, SELI);
                ButtonHandle -= Button_Click;
                this.Dispose();
            }

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
                else
                {
                    Label1.Location = new System.Drawing.Point(26, 22);
                }
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
                    if (DI == FindNL_S.Length) { if (CWCM > CW) { CW = CWCM; } }
                }
                CWCM = 0;
                FindNL_S = null;
                // -->
                Label1.ResumeLayout();
                Label1.Refresh();
                Label2.SuspendLayout();
                Label2.Text = "";
                // These nested loops draw the label color to the appropriate distances defined by Label2.Width.
                // (that's why it is drawing spaces , for the background like the normal modal Windows Message Box has.)
                for (System.Int32 NB = 0; NB < 3; NB++)
                {
                    for (System.Int32 ITR = 0; ITR < (Label2.Location.X + CW); ITR++)
                    {
                        Label2.Text += " ";
                    }
                    Label2.Text += "\n";
                }
                Label2.BorderStyle = BorderStyle.None;
                Label2.AutoSize = true;
                Label2.BackColor = System.Drawing.Color.Gray;
                Label2.Location = new System.Drawing.Point(0, Label1.Location.Y + CH + 40);
                CH = 0;
                Label2.ResumeLayout();
                Button1.SuspendLayout();
                Button2.SuspendLayout();
                Button3.SuspendLayout();
                Button1.Location = new System.Drawing.Point(Label1.Location.X + CW - 18, Label2.Location.Y + 10);
                Button1.Size = new System.Drawing.Size(65, 20);
                Button2.Location = new System.Drawing.Point(Button1.Left - 75, Button1.Top);
                Button2.Size = Button1.Size;
                Button3.Location = new System.Drawing.Point(Button2.Left - 75, Button1.Top);
                Button3.Size = Button1.Size;
                Label2.Size = new System.Drawing.Size(Label2.Location.X + CW, 60);
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
                if (Butt == (ButtonSelection) 1)
                {
                    Button1.Text = "No";
                    Button2.Text = "Yes";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button2);
                    Menu.Controls.Add(Button1);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection) 2)
                {
                    Button1.Text = "Cancel";
                    Button2.Text = "OK";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection) 3)
                {
                    Button1.Text = "Retry";
                    Button2.Text = "Abort";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection) 4)
                {
                    Button1.Text = "Retry";
                    Button2.Text = "Cancel";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection) 5)
                {
                    Button1.Text = "Ignore";
                    Button2.Text = "Cancel";
                    Button3.Visible = false;
                    Menu.Controls.Add(Button1);
                    Menu.Controls.Add(Button2);
                    Button1.Click += ButtonHandle;
                    Button2.Click += ButtonHandle;
                }
                if (Butt == (ButtonSelection) 6)
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
                if (Butt == (ButtonSelection) 7)
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
                Menu.ShowInTaskbar = false;
                // All the redrawings are only valid for Microsoft Sans Serif font!!!
                Menu.Font = new Font("Microsoft Sans Serif", (System.Single)8.25, FontStyle.Regular, GraphicsUnit.Point);
                Menu.Size = new System.Drawing.Size(Label1.Location.X + CW + 85, Label2.Location.Y + Label2.Height + 20);
                Menu.Controls.Add(Label1);
                Menu.Controls.Add(Label2);
                if (Icon != 0)
                {
                    Menu.Controls.Add(PictureBox1);
                }
                if (Icon == (IconSelection)1) { System.Media.SystemSounds.Hand.Play(); }
                if (Icon == (IconSelection)4) { System.Media.SystemSounds.Asterisk.Play(); }
                if (Icon == (IconSelection)5) { System.Media.SystemSounds.Question.Play(); }
                if (Icon == (IconSelection)6) { System.Media.SystemSounds.Hand.Play(); }
                if (Icon == (IconSelection)7) { System.Media.SystemSounds.Question.Play(); }
                Menu.ShowDialog();
            }

        }
    }
}

namespace ExternalHashCaculators
{
    //A Collection Namespace for computing hash values from external generated libraries.

    /// <summary>
    /// xxHash is a fast non-cryptographic hash digest. This is a wrapper for the unmanaged library.
    /// Note that you can run this only on AMD64 machines and you must have the library where the 
    /// application's current directory is.
    /// </summary>
    [SupportedOS(SupportedOSAttributePlatforms.Windows)]
    public class XXHash
	{
		// xxHash Hash caculator system.
        // It is a fast , non-cryptographic algorithm , as described from Cyan4973.
        // It is also used by the zstd archiving protocol , so as to check and the file integrity.
        // The version imported here is 0.8.1.
		
		private static System.Boolean _CheckDLLVer()
		{
			if (!(System.IO.File.Exists(@".\xxhash.dll"))) {return false;}
            if (ROOT.MAIN.OSProcessorArchitecture() != "AMD64") { return false; }
            if (XXHASHMETHODS.XXH_versionNumber() < 00801) 
			{
				return false;
			}
			else
			{return true;}
		}
		
		private sealed class XXHASHMETHODS
		{
			[System.Runtime.InteropServices.DllImport(@".\xxhash.dll")]
			public static extern System.Int32 XXH32(System.Byte[] buffer ,
            System.Int32 size ,System.Int32 seed = 0);
			
			[System.Runtime.InteropServices.DllImport(@".\xxhash.dll")]
			public static extern System.Int32 XXH64(System.Byte[] buffer ,
            System.Int32 size ,System.Int32 seed = 0);
			
			[System.Runtime.InteropServices.DllImport(@".\xxhash.dll")]
			public static extern System.Int32 XXH_versionNumber();
		}
		
		/// <summary>
		/// Computes a file hash by using the XXH32 function.
		/// </summary>
		/// <param name="FileStream">The alive <see cref="System.IO.FileStream"/> object from which the data will be collected.</param>
		/// <returns>A caculated xxHash32 value written as an hexadecimal <see cref="System.String"/>.</returns>
		public static System.String xxHash_32(System.IO.FileStream FileStream)
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
        /// Computes a file hash by using the XXH64 function.
        /// </summary>
        /// <param name="FileStream">The alive <see cref="System.IO.FileStream"/> object from which the data will be collected.</param>
        /// <returns>A caculated xxHash64 value written as an hexadecimal <see cref="System.String"/>.</returns>
		/// <remarks>This function performs well only on AMD64 machines; it's performance is degraded when working on IA32.</remarks>
        public static System.String xxHash_64(System.IO.FileStream FileStream)
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


namespace ExternalArchivingMethods
{
    // A Collection Namespace for making archives outside Microsoft's managed code.

    [SupportedOS(SupportedOSAttributePlatforms.Windows)]
    public class ZstandardArchives
	{
		// Yes , this patch adds (Patch Version 1.5.4.0) the Zstandard Archive format.
		// The runtime algorithm is built from the GitHub tree , version 1.5.2.0.
    	// This has the following advantages: 
		//    1. The Dynamic-Link Library for the archive format is native C++.
		//    2. This format is very efficient. It can compress and decompress data very fast.
		//    3. The C algorithm that is comprised from is one of the most fast programming languages.
		//    4. The project has only ported the original DLL. If you want to use this class , make sure
		//    that you have ported that DLL to .\ . Note that you cannot run earlier versions than 1.5.2.0.
        // NOTICE: the zstd.dll bundled with my API is being built by me.
        // Because actually this API calls the library via unmanaged way (Not very safe)
        // and requires the DLL path , use only updates which are either came from GitHub or other source that
        // is reliable. However , it is still very safe and stable , of course.
		
		private static System.Boolean _CheckDLLVer()
		{
			if (!(System.IO.File.Exists(@".\zstd.dll")))
			{
				return false;
			}
            if (ROOT.MAIN.OSProcessorArchitecture() != "AMD64") { return false; }
            if (ZSTD.ZSTD_versionNumber() < 10502)
			{
				return false;
			}
			else 
			{
				return true;
			}
		}
		
		public enum ZSTDCMPLevel : System.Int32
		{
			Fast = 1,
			Fast2 = 2,
			Efficient = 3,
			Lazy = 4,
			Lazy2 = 5,
			LazyOptimized = 6,
			Optimal = 7,
			Ultra = 8,
			FullCompressPower = 9,
		}
		
		private sealed class ZSTD
		{
			// Proper API Calls defined in this class. DO NOT Modify.
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_compress(System.Byte[] dst ,System.Int32 dstCapacity , 
			System.Byte[] src ,System.Int32 srcCapacity ,ZSTDCMPLevel compressionLevel);
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_decompress(System.Byte[] dst ,System.Int32 dstCapacity , 
			System.Byte[] src ,System.Int32 srcSize);
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int64 ZSTD_getFrameContentSize(System.Byte[] src ,System.Int32 srcSize);
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_isError(System.Int32 code);
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_findFrameCompressedSize(System.Byte[] src ,System.Int32 srcSize);
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_defaultCLevel();
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_minCLevel();
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_maxCLevel();
			
			[System.Runtime.InteropServices.DllImport(@".\zstd.dll")]
			public static extern System.Int32 ZSTD_versionNumber();
			
		}
		
		
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
