// Easy API For Any Executable Building need.

// Global namespaces
using System;
using System.IO;
using System.Text;
using Windows.Win32;
using Microsoft.Win32;
using System.Security;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO.Compression;
using System.Runtime.InteropServices;


namespace ROOT 
{
	// A Collection Namespace which includes Microsoft's Managed code.
    // Many methods here , however , are controlled and built by me at all.
	
	public class MAIN 
	{


#if NETFRAMEWORK

		// Since that the functions that use this class can run only on .NET framework , 
		// this is also exisiting only on .NET framework.
		public class FileDialogsReturner
		{
			private string ERC;
			private string FNM;
			private string FNMFP;
			
			public string ErrorCode
			{
				get { return ERC; }
				set { ERC = value; }
			}
			
			public string FileNameOnly
			{
				get { return FNM; }
				set { FNM = value; }
			}
			
			public string FileNameFullPath
			{
				get { return FNMFP; }
				set { FNMFP = value; }
			}
		}

#endif

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
		
		public static void EmitBeepSound()
		{
			Microsoft.VisualBasic.Interaction.Beep();
		}
		
		public static void WriteConsoleText(System.String Text)
		{
			System.Console.WriteLine(@Text);
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

        public static System.String GetRuntimeVersion()
        { return System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion(); }

        public static System.Boolean CheckIfStartedFromSpecifiedOS(System.Runtime.InteropServices.OSPlatform OSP)
        { return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSP); }

        public static System.String OSInformation()
		{ return System.Runtime.InteropServices.RuntimeInformation.OSDescription; }

		public static System.String OSFramework()
		{return System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription; }

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
		
		public static System.Boolean FileExists(System.String Path)
		{
			if (System.IO.File.Exists(@Path))
			{
				return true;
			}
			else 
			{
				return false;
			}
		}
		
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
            System.Byte[] EMDK = new System.Text.UTF8Encoding(true).GetBytes(Contents + System.Environment.NewLine);
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
		
		public static System.String GetACryptographyHashForAFile(System.String PathOfFile , System.String HashToSelect = "SHA1")
		{
			System.IO.FileStream Initialiser = ReadAFileUsingFileStream(PathOfFile);
			if (Initialiser == null)
			{
				return "Error";
			}
			System.String File = GetContentsFromFile(Initialiser);
			Initialiser.Close();
			Initialiser.Dispose();
			Initialiser = null;
			var RDI = new System.Text.ASCIIEncoding();
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
			System.Byte[] RSS = EDI.ComputeHash(RDI.GetBytes(File));
			EDI.Dispose();
            RDI = null;
            File = null;
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
			if (! FileExists(Path))
			{
				return false;
			}
			try
			{
				System.IO.File.Delete(Path);
			}
			catch (System.Exception)
			{
				return false;
			}
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

#if NETFRAMEWORK

		// The below code excerpts can run only on .NET framework.

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
		
		
		public static FileDialogsReturner CreateLoadDialog(System.String FileFilterOfWin32,System.String FileExtensionToPresent ,
		System.String FileDialogWindowTitle)
		{
			FileDialogsReturner EDOut = new FileDialogsReturner();
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
		
		public static FileDialogsReturner CreateSaveDialog(System.String FileFilterOfWin32,System.String FileExtensionToPresent ,
		System.String FileDialogWindowTitle)
		{
			FileDialogsReturner EDOut = new FileDialogsReturner();
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

#endif
		public static void HaltApplicationThread(System.Int32 TimeoutEpoch)
		{
			System.Threading.Thread.Sleep(TimeoutEpoch);
		}
		
		/*
		private static void WriteProcessDataToConsole(System.Object sender , System.Diagnostics.DataReceivedEventArgs DataObject)
		{
			System.ConsoleColor FORE , BACK;
			FORE = System.Console.ForegroundColor;
			BACK = System.Console.BackgroundColor;
			System.Console.ForegroundColor = System.ConsoleColor.Gray;
			System.Console.BackgroundColor = System.ConsoleColor.Black;
			if (!System.String.IsNullOrEmpty(DataObject.Data)) 
			{
				if (DataObject.Data.IndexOf('\n') != -1) 
				{
					System.Console.Write(@"INFO: " + DataObject.Data);
				} else if (DataObject.Data.IndexOf('\n') == -1)
				{
					System.Console.WriteLine(@"INFO: " + DataObject.Data);
				}
			}
			System.Console.ForegroundColor = FORE;
			System.Console.BackgroundColor = BACK;
		}
		
		private static void WriteErrorProcessDataToConsole(System.Object sender , System.Diagnostics.DataReceivedEventArgs DataObject)
		{
			System.ConsoleColor FORE , BACK;
			FORE = System.Console.ForegroundColor;
			BACK = System.Console.BackgroundColor;
			System.Console.ForegroundColor = System.ConsoleColor.Red;
			System.Console.BackgroundColor = System.ConsoleColor.Black;
			if (!System.String.IsNullOrEmpty(DataObject.Data)) 
			{
				if (DataObject.Data.IndexOf('\n') != -1) 
				{
					System.Console.Write(@"ERROR: " + DataObject.Data);
				} else if (DataObject.Data.IndexOf('\n') == -1)
				{
					System.Console.WriteLine(@"ERROR: " + DataObject.Data);
				}
			}
			System.Console.ForegroundColor = FORE;
			System.Console.BackgroundColor = BACK;
		}
		*/

		public static System.Int32 LaunchProcess(System.String PathOfExecToRun ,System.String CommandLineArgs = " " , 
        System.Boolean ImplicitSearch = false ,System.Boolean WaitToClose = false ,System.Boolean RunAtNativeConsole = false , 
		System.Boolean HideExternalConsole = true)
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
	
	public class RegEditor : System.IDisposable
	{
		private System.String _RootKey_;
		private System.String _SubKey_;
		private System.Boolean _DIAG_;
		
		public System.String RootKey
		{
			get { return _RootKey_; }
			set { _RootKey_ = value; }
		}
		
		public System.String SubKey
		{
			get { return _SubKey_; }
			set { _SubKey_ = value; }
		}
		
		public System.Boolean DiagnosticMessages
		{
			set { _DIAG_ = value; }
		}
		
		private System.Boolean _CheckPredefinedProperties()
		{
			if ((! System.String.IsNullOrEmpty(_RootKey_)) && (! System.String.IsNullOrEmpty(_SubKey_)))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
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
			System.Object RegEntry = Microsoft.Win32.Registry.GetValue(_RootKey_ + @"\" + _SubKey_ , VariableRegistryMember , "_ER_C_");
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
		
		public System.String SetEntry(System.String VariableRegistryMember,System.String RegistryType,System.Object RegistryData)
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
			if (RegistryData == null)
			{
				if (_DIAG_) {System.Console.WriteLine("ERROR: 'System.Null' value detected in RegistryData object. Maybe invalid definition?"); }
				return "Error";
			}
			Microsoft.Win32.RegistryValueKind RegType_;
			if (System.String.IsNullOrEmpty(RegistryType))
			{
				if (_DIAG_)
				{
					System.Console.WriteLine("WARNING: Undefined registry item type. Assuming Value as it is a 'String' .");
				    System.Console.WriteLine("INFO: Allowed Values: String , ExpandString , QWord , DWord .");
				}
				RegType_ = Microsoft.Win32.RegistryValueKind.String;
			} else if (RegistryType == "String")
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.String;
			} else if (RegistryType == "ExpandString")
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.ExpandString;
			} else if (RegistryType == "QWord")
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.QWord;
			} else if (RegistryType == "DWord")
			{
				RegType_ = Microsoft.Win32.RegistryValueKind.DWord;
			}
			else
			{
				if (_DIAG_)
				{
					System.Console.WriteLine("ERROR: Unknown registry value type argument in the object creator was given: " + RegistryType);
				}
				return "Error";
			}
			try
			{
				Microsoft.Win32.Registry.SetValue(_RootKey_ + @"\" + _SubKey_ , VariableRegistryMember , RegistryData , RegType_);
			}
			catch (System.Exception EX)
			{
				if (_DIAG_)
				{
					System.Console.WriteLine("ERROR: Could not create key " + VariableRegistryMember + " . Invalid name maybe?");
				    System.Console.WriteLine("Error Raw Data: " + EX.ToString());
				}
				return "Error";
			}
			return "Sucessfull";
		}
		
		public System.String DeleteEntry(System.String VariableRegistryMember)
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
					return "UNDEF_ERR";
			}
			if (System.Convert.ToString(ValueDelete.GetValue(VariableRegistryMember , "_DNE_")) == "_DNE_")
			{
				ValueDelete.Close();
				return "Error";
			}
			ValueDelete.DeleteValue(VariableRegistryMember);
			ValueDelete.Close();
			return "Sucessfull";
		}
	
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
					using (System.Security.Cryptography.CryptoStream CryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR , ENC_2 , System.Security.Cryptography.CryptoStreamMode.Write))
					{
						using (System.IO.StreamWriter SDM = new System.IO.StreamWriter(CryptStrEnc , System.Text.Encoding.UTF8))
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
				UnderlyingStream.Read(ByteArray , 0 , System.Convert.ToInt32(UnderlyingStream.Length));
				System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
				ENC_1.Key = _EncryptionKey_;
				ENC_1.IV = _InitVec_;
				ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
				System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateEncryptor();
				System.Byte[] EncryptedArray;
				using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream())
				{
					using (System.Security.Cryptography.CryptoStream CryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR , ENC_2 , System.Security.Cryptography.CryptoStreamMode.Write))
					{
						using (System.IO.BinaryWriter SDM = new System.IO.BinaryWriter(CryptStrEnc , System.Text.Encoding.UTF8))
						{
							SDM.Write(ByteArray , 0 , ByteArray.Length);
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
				if (_CheckPredefinedProperties()) {return null;}
				System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
				ENC_1.Key = _EncryptionKey_;
				ENC_1.IV = _InitVec_;
				ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
				System.String StringToReturn = null;
				System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateDecryptor();
				using (System.IO.MemoryStream MSSSR = new System.IO.MemoryStream(EncryptedArray))
				{
					using (System.Security.Cryptography.CryptoStream DCryptStrEnc = new System.Security.Cryptography.CryptoStream(MSSSR , ENC_2 , System.Security.Cryptography.CryptoStreamMode.Read))
					{
						using (System.IO.StreamReader SDE = new System.IO.StreamReader(DCryptStrEnc , System.Text.Encoding.UTF8))
						{
							StringToReturn = SDE.ReadToEnd();
						}
					}
				}
				return StringToReturn;
			}
			
			public System.String DecryptSpecifiedDataForFiles(System.IO.FileStream EncasingStream)
			{
				if (EncasingStream.CanRead == false) {return null;}
				if (_CheckPredefinedProperties()) {return null;}
				System.Security.Cryptography.AesCng ENC_1 = CNGBaseObject;
				ENC_1.Key = _EncryptionKey_;
				ENC_1.IV = _InitVec_;
				ENC_1.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
				ENC_1.Mode = System.Security.Cryptography.CipherMode.CBC;
				System.String FinalString = null;
				System.Security.Cryptography.ICryptoTransform ENC_2 = ENC_1.CreateDecryptor();
				using (System.Security.Cryptography.CryptoStream DCryptStrEnc = new System.Security.Cryptography.CryptoStream(EncasingStream , ENC_2 , System.Security.Cryptography.CryptoStreamMode.Read))
				{
					using (System.IO.StreamReader SDE = new System.IO.StreamReader(DCryptStrEnc , System.Text.Encoding.UTF8))
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
					return System.Convert.ToBase64String(ByteValue , 0 , ByteValue.Length , 0);
				}
				catch (System.Exception)
				{
					return null;
				}
			}
			
			public static System.Byte[] ConvertTextKeyOrIvFromStringToByteArray(System.String StringValue) 
			{
				if (System.String.IsNullOrEmpty(StringValue)) {return null;}
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
					get {return _ERC_;}
					set {_ERC_ = value;}
				}
				
				public System.Byte[] KeyUsed
				{
					get {return _KEY_;}
					set {_KEY_ = value;}
				}
				
				public System.Byte[] InitVectorUsed
				{
					get {return _IV_;}
					set { _IV_ = value;}
				}
			}
			
			public static EncryptionContext EncryptAFile(System.String FilePath ,System.String FileOutputPath = "") 
			{
				if (!(System.IO.File.Exists(FilePath))) {return null;}
				if (System.String.IsNullOrEmpty(FileOutputPath))
				{
					FileOutputPath = (FilePath).Remove(FilePath.IndexOf(".")) + "_ENCRYPTED_" + (FilePath).Substring(FilePath.IndexOf("."));
				}
				EncryptionContext DMF = new EncryptionContext();
				AESEncryption MDA = new AESEncryption();
				AESEncryption.KeyGenTable MAKER = AESEncryption.MakeNewKeyAndInitVector();
				if (MAKER.ErrorCode == "Error")
				{
					DMF.ErrorCode = "Error";
					return DMF;
				}
				MDA.EncryptionKey = MAKER.Key;
                MDA.IV = MAKER.IV;
				try
				{
					using (System.IO.FileStream MDR = new System.IO.FileStream(FilePath , System.IO.FileMode.Open))
					{
						using (System.IO.FileStream MNH = System.IO.File.OpenWrite(FileOutputPath))
						{
							System.Byte[] FLL = MDA.EncryptSpecifiedDataForFiles(MDR);
							MNH.Write(FLL , 0 , System.Convert.ToInt32(FLL.Length));
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
			
			public static void DecryptAFile(System.String FilePath ,System.Byte[] Key , 
            System.Byte[] IV ,System.String FileOutputPath = "")
			{
				if (!(System.IO.File.Exists(FilePath))) {return;}
				if ((Key is null) || (IV is null)) {return;}
				if ((Key.Length <= 0) || (IV.Length <= 0)) {return;}
				if (System.String.IsNullOrEmpty(FileOutputPath))
				{
					FileOutputPath = (FilePath).Remove(FilePath.IndexOf(".")) + "_UNENCRYPTED_" + (FilePath).Substring(FilePath.IndexOf("."));
				}
				AESEncryption MDA = new AESEncryption();
				MDA.EncryptionKey = Key;
                MDA.IV = IV;
				try
				{
					using (System.IO.FileStream MDR = new System.IO.FileStream(FilePath , System.IO.FileMode.Open))
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
		
	}
	
	public class TimeCaculator : System.IDisposable
	{
		private System.DateTime _TimeEl_;
		private System.Boolean _Init_ = false;
		
		public void Init()
		{
			if (_Init_ == true) {return;}
			_TimeEl_ = Microsoft.VisualBasic.DateAndTime.Now;
			_Init_ = true;
			return;
		}
		
		public System.Int32 CaculateTime()
		{
			if (!(_Init_)) {return -1;}
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
		
		public void Dispose() {DisposeResources();}
		
		private protected void DisposeResources()
		{
			#pragma warning disable CS0219
			System.Object _TimeEl_ = null;
			System.Object _Init_ = null;
			#pragma warning restore CS0219
		}
	}
	
}

namespace ExternalHashCaculators
{
	// A Collection Namespace for computing hash values from external generated libraries.
	
	public class XXHash
	{
		// xxHash Hash caculator system.
        // It is a fast , non-cryptographic algorithm , as described from Cyan4973.
        // It is also used by the zstd archiving protocol , so as to check and the file integrity.
        // The version imported here is 0.8.1.
		
		private static System.Boolean _CheckDLLVer()
		{
			if (!(System.IO.File.Exists(@".\xxhash.dll"))) {return false;}
			if (System.Environment.ExpandEnvironmentVariables("%PROCESSOR_ARCHITECTURE%") != "AMD64") 
			{
				if (System.Environment.ExpandEnvironmentVariables("%PROCESSOR_ARCHITEW6432%") != "AMD64") {return false;}
			}
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
			if (System.Environment.ExpandEnvironmentVariables("%PROCESSOR_ARCHITECTURE%") != "AMD64")
			{
				if (System.Environment.ExpandEnvironmentVariables("%PROCESSOR_ARCHITEW6432%") != "AMD64") {return false;}
			}
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
	
	
	
	//public class Cabinets
	//{
	//	public static System.Boolean CompressAsFileStreams(System.IO.FileStream Inputfile, System.IO.FileStream ArchiveFile)
	//	{
	//		if (Inputfile.CanRead == false) {return false;}
	//		if (Inputfile.Length < 10) {return false;}
	//		if (ArchiveFile.CanWrite == false) {return false;}
	//		if (ArchiveFile.Length > 0) {return false;}
	//		nint? FH;
	//		if (Windows.Win32.PInvoke.CreateCompressor(Windows.Win32.Storage.Compression.COMPRESS_ALGORITHM.COMPRESS_ALGORITHM_MSZIP 
	//		, null ,out FH) == false) 
	//		{
	//			System.Console.WriteLine("Error");
	//			FH = null;
	//			return false;
	//		}
	//		System.Runtime.InteropServices.SafeHandle SH = new System.Runtime.InteropServices.SafeHandle(FH , true);
	//		FH = null;
	//		System.Byte[] FSI = new System.Byte[Inputfile.Length];
	//		try
	//		{
	//			Inputfile.Read(FSI , 0 , System.Convert.ToInt32(Inputfile.Length));
	//		}
	//		catch (System.Exception)
	//		{
	//			SH = null;
	//			FSI = null;
	//			return false;
	//		}
	//		System.Byte[] FSO = new System.Byte[Inputfile.Length + 38];
	//		System.Int32 CmpData = 0;
	//		if (Windows.Win32.PInvoke.Compress(SH , FSI , FSI.Length , FSO , FSO.Length , CmpData) == false)
	//		{
	//			SH = null;
	//			FSI = null;
	//			FSO = null;
	//			return false;
	//		}
	//		else
	//		{
	//			SH = null;
	//			FSI = null;
	//		}
	//		try
	//		{
	//			ArchiveFile.Write(FSO , 0 , CmpData);
	//		}
	//		catch (System.Exception)
	//		{
	//			return false;
	//		}
	//		finally 
	//		{
	//			FSO = null;
	//		}
	//		return true;
	//	}
	//}
}
