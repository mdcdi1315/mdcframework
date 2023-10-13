
using MDCFR;
using ROOT;
using System;

namespace MDCFR
{

    /// <summary>
    /// This Class checks whether an class pass the declared custom attribute <see cref="AsInterfaceAttribute"/> requirements. 
    /// </summary>
    internal class AsInterfaceChecker
    {
        private System.Type Interface;
        private System.Type DerivedClass;
        private const System.String IntTypeArg = "InterfaceType";
        private const System.String DerClassTypeArg = "DerivedClassType";

        /// <summary>
        /// Construct a new instance of the <see cref="AsInterfaceChecker"/> with the specified class type that acts 
        /// as the derived interface , 
        /// and the interface that the checker will check if the pseudo-derived class meets the interface requirements.
        /// </summary>
        /// <param name="InterfaceType">The interface that the pseudo-derived class uses.</param>
        /// <param name="DerivedClassType">The pseudo class that defines the interface members.</param>
        public AsInterfaceChecker(System.Type InterfaceType, System.Type DerivedClassType)
        {
            this.Interface = InterfaceType;
            this.DerivedClass = DerivedClassType;
        }

        private void ThrowNotInterface()
        {
            throw new ArgumentException(
            $"The type provided as interface is NOT an interface.\nObject Type: {Interface.FullName}", IntTypeArg);
        }

        private void ThrowDoesNotContainAttr()
        {
            throw new ArgumentException(
            $"The pseudo-derived class does NOT contain the derived interface.\nDerived Class Name: {DerivedClass.FullName}", DerClassTypeArg);
        }

        private void ThrowNotCorrectType(System.Type ErrType)
        {
            throw new ArgumentException($"The Type declared in the attribute , {ErrType.FullName} , " +
            $"does not match the type of the given interface.\n" +
            $"Declared type to check: {Interface}\n" +
            $"Type that resides in the declared attribute in {DerivedClass.FullName}: {ErrType.FullName}", DerClassTypeArg);
        }

        private void ThrowNoMembersFound()
        {
            throw new ArgumentException(
            $"No members were detected inside the derived class: {DerivedClass.FullName}", DerClassTypeArg);
        }

        private void ThrowNoSpecificMemberFound(System.Reflection.MethodInfo MI)
        {
            throw new ArgumentException(
            $"The interface has an implementation for {MI.Name} function , but that does not exist in the {DerivedClass.FullName} .\n" +
            $"Full interface member data: \n" +
            $"Member Name: {MI.Name} \n" +
            $"Member Return Type: {MI.ReturnType} \n" +
            $"Member Has Generic Parameters: {MI.ContainsGenericParameters} \n" +
            $"Member Is Special: {MI.IsSpecialName} \n" +
            $"This member resides in the {Interface.FullName} type.", DerClassTypeArg);
        }

        /// <summary>
        /// Runs the actual check. When no exceptions are thrown , it means that the pseudo-derived class fully implements the given interface.
        /// </summary>
        public void Check()
        {
            if (Interface.IsInterface == false) { ThrowNotInterface(); }
            System.Boolean Found = false;
            System.Type Referer = null;
            foreach (System.Reflection.CustomAttributeData DS in DerivedClass.CustomAttributes)
            {
                if (DS.AttributeType == typeof(AsInterfaceAttribute))
                {
                    foreach (System.Reflection.CustomAttributeTypedArgument DG in DS.ConstructorArguments)
                    {
                        if (DG.ArgumentType == typeof(System.String)) { Referer = System.Type.GetType(DG.Value.ToString()); }
                        if (DG.ArgumentType == typeof(System.Type)) { Referer = (System.Type)DG.Value; }
                    }
                    Found = true; break;
                }
            }
            if (Found == false) { ThrowDoesNotContainAttr(); }
            if (Referer != Interface) { ThrowNotCorrectType(Referer); }
            Found = false;
            Referer = null;
            if (DerivedClass.GetMethods().Length <= 0) { ThrowNoMembersFound(); }
            foreach (System.Reflection.MethodInfo DN in Interface.GetMethods())
            {
                Found = false;
                foreach (System.Reflection.MethodInfo DG in DerivedClass.GetMethods())
                {
                    if (DN.Name == DG.Name)
                    {
                        if (DN.ReturnType != DG.ReturnType) { break; }
                        Found = true; break;
                    }
                }
                if (Found == false) { ThrowNoSpecificMemberFound(DN); }
            }
        }
    }

    internal struct CmdLineData
    {
        public System.String FileSearchLoc;
        public System.String AssemblyPath;
        public System.String TypeOfInterface;
        public System.String TypeOfDerivedClass;
        public System.Boolean IsVerbosityEnabled;
        public System.Boolean ExceptionInfoEnabled;

        public System.Boolean IsFileSearchLocDefined { get { if (System.String.IsNullOrEmpty(FileSearchLoc)) { return false; } else { return true; } } }

        public System.Boolean AssemblyExists { get { if (IsFileSearchLocDefined == false) { if (MAIN.FileExists(AssemblyPath)) { return true; } else { return false; } } else { return false; } } }

        public System.Boolean AllArgsAreDefault { get
            {
                if (System.String.IsNullOrEmpty(AssemblyPath) &&
                    System.String.IsNullOrEmpty(TypeOfInterface) &&
                    System.String.IsNullOrEmpty(TypeOfDerivedClass)) { return true; } else { return false; }
            } }

        public CmdLineData() { FileSearchLoc = ""; AssemblyPath = ""; TypeOfDerivedClass = ""; TypeOfInterface = ""; IsVerbosityEnabled = false; ExceptionInfoEnabled = false; }
    }

    internal enum OperationType { Default = 0, AllTypesInAssembly, SpecificType }

    internal class Runner
    {
        public System.Reflection.Assembly LoadedAssembly;
        public System.String OfIntInterface;
        public System.String OfPseudoClass;
        public System.Collections.Generic.List<ClassDataThatHasAttr> ClassData = new();

        public Runner() { ErrorDetected += AsInterfaceValidator.ShowErrorAndExit; }

        public void FindAndLoadAssemblyThroughReflection()
        {
            System.String path = "";
            System.Boolean Found = false;
            if (AsInterfaceValidator.ProgramTempValues.IsFileSearchLocDefined)
            {
                Logger.WriteVerbose($"Finding assembly using relative path loaded from PATH.");
                FileSearchResult FRT = MAIN.FindFileFromPath(AsInterfaceValidator.ProgramTempValues.AssemblyPath);
                if (FRT.MatchFound)
                {
                    Logger.WriteVerbose($"Found the assembly. Relative path used.");
                    path = FRT.Path;
                } else if (
                    MAIN.FileExists($"{AsInterfaceValidator.ProgramTempValues.FileSearchLoc}" +
                    $"\\{AsInterfaceValidator.ProgramTempValues.AssemblyPath}"))
                {
                    Logger.WriteVerbose($"Found the assembly using the fallback path.");
                    path = $"{AsInterfaceValidator.ProgramTempValues.FileSearchLoc}\\{AsInterfaceValidator.ProgramTempValues.AssemblyPath}";
                }
                else { Logger.WriteFatal("Could not locate file " +
                    $"{AsInterfaceValidator.ProgramTempValues.AssemblyPath} , either in the PATH variable or in the fallback path. \n" +
                    "Define the absolute or relative path to the assembly instead."); return; }
            } else { path = AsInterfaceValidator.ProgramTempValues.AssemblyPath; }
            Logger.WriteVerbose($"Found assembly {path}.");
            Logger.WriteVerbose("Loading assembly ...");
            try
            {
                LoadedAssembly = System.Reflection.Assembly.LoadFrom(path);
            } catch (System.IO.FileNotFoundException e) { Logger.WriteException(e); return; }
            catch (System.IO.FileLoadException e) { Logger.WriteException(e); return; }
            catch (System.IO.PathTooLongException e) { Logger.WriteException(e); return; }
            catch (System.BadImageFormatException e) { Logger.WriteException(e); return; }
            catch (System.Security.SecurityException e) { Logger.WriteException(e); return; }
            Logger.WriteVerbose("Assembly loaded. Data:\n" +
                $"Assembly Full Name: {LoadedAssembly.FullName} \n" +
                $"Assembly Trust Information: {LoadedAssembly.PermissionSet.ToXml().Text} \n" +
                $"Assembly loaded with full trust: {LoadedAssembly.IsFullyTrusted} \n" +
                $"Assembly created at .NET version: {LoadedAssembly.ImageRuntimeVersion}");

            if (AsInterfaceValidator.OP != OperationType.AllTypesInAssembly)
            {
                Logger.WriteVerbose("Finding pseudo-class implementation.");
                foreach (System.Type DX in LoadedAssembly.ExportedTypes) { if (DX.FullName == OfPseudoClass) { Found = true; break; } }
                if (Found == false)
                {
                    Logger.WriteException(
                    new System.TypeLoadException($"Cannot open class {OfPseudoClass}: \n" +
                    $"This class does not exist in the assembly \'{LoadedAssembly.FullName}\' .\n" +
                    $"As a result , the code cannot continue execution.")); return;
                }
                else
                { Logger.WriteVerbose("Pseudo-class found sucessfully."); }
                Logger.WriteVerbose("Finding interface implementation.");
                Found = false;
                foreach (System.Type DX in LoadedAssembly.ExportedTypes) { if (DX.FullName == OfPseudoClass) { Found = true; break; } }
                if (Found == false)
                {
                    Logger.WriteException(
                    new System.TypeLoadException($"Cannot open class {OfPseudoClass}: \n" +
                    "This interface does not exist.\nAs a result , the code cannot continue execution.")); return;
                }
                else { Logger.WriteVerbose("Interface found sucessfully."); }
            }
            Logger.WriteVerbose("Assembly loading done.");
        }

        public void FindAllClassesThatHaveAsInterfaceAttr()
        {
            Logger.WriteVerbose("Getting assembly types to find all classes:");
            foreach (System.Type D in LoadedAssembly.GetTypes())
            {
                if (D.IsClass || D.IsValueType || D.IsAnsiClass || D.IsAbstract)
                {
                    if (D.IsPublic && D.IsVisible) { Logger.WriteVerbose($"Found a public class: {D.FullName}"); }
                    foreach (System.Reflection.CustomAttributeData G in D.CustomAttributes)
                    {
                        if (G.AttributeType == typeof(ROOT.AsInterfaceAttribute))
                        {
                            Logger.WriteVerbose($"Found the attribute for this class. Adding it to the check list. Current items: {ClassData.Count + 1}");
                            ClassData.Add(new ClassDataThatHasAttr() { AttributeType = FindConstructorArg(G), ClassType = D });
                        }
                    }
                } else { Logger.WriteVerbose($"Type {D.FullName} excluded because it is not in the requirements."); }
            }
        }

        private System.Type FindConstructorArg(System.Reflection.CustomAttributeData A)
        {
            foreach (System.Reflection.CustomAttributeTypedArgument E in A.ConstructorArguments)
            {
                if (E.ArgumentType == typeof(System.Type)) { return (System.Type)E.Value; }
                if (E.ArgumentType == typeof(System.String)) { return System.Type.GetType(E.Value.ToString()); }
            }
            return null;
        }

        public void RunChecksAgainstAll()
        {
            System.Int32 Sucessfull = ClassData.Count;
            System.Int32 Unsucessfull = 0;
            if (ClassData.Count <= 0) 
            {
                MAIN.WriteConsoleText("No Classes were found that implement the AsInterface attribute.\n" +
                    "This means that the assembly provided is not an assembly derived on MDCFR , or the\n" +
                    "assembly provided just does not implement anywhere this attribute.");
                Logger.WriteVerbose("No classes were found that have the AsInterface attribute.");
                Logger.WriteVerbose("Program will terminate after all dependency checks are finished.");
                return;
            }
            foreach (ClassDataThatHasAttr DS in ClassData)
            {
                try
                {
                    Logger.WriteVerbose($"Checking for {DS.ClassType.FullName} from {DS.ClassType.AssemblyQualifiedName} ...");
                    new MDCFR.AsInterfaceChecker(DS.AttributeType, DS.ClassType).Check();
                } catch (System.Exception D) { Sucessfull--; Unsucessfull++; Logger.WriteVerbose($"One class check failed for {DS.ClassType.FullName}:\n" +
                    D + $"\nSucessfull checks: {Sucessfull} \nUnsuccessfull checks: {Unsucessfull}"); }
            }
            Logger.WriteVerbose("All available checks completed.");
        }

        public static event System.EventHandler<ErrorDetectedEventArgs> ErrorDetected;

        public static void InvokeErrorDetected(System.Object _send , ErrorDetectedEventArgs ed)
        {
            ErrorDetected.Invoke(_send , ed);
        }
    }

    internal class ErrorDetectedEventArgs : System.EventArgs
    {
        public System.String Error { get; internal set; }

        public System.Exception Cause { get; internal set; }

        public ErrorDetectedEventArgs(string error, Exception cause)
        {
            Error = error;
            Cause = cause;
        }
    }


    internal struct ClassDataThatHasAttr
    {
        public System.Type AttributeType;
        public System.Type ClassType;
    }

    internal class AsInterfaceValidator
	{
        private static System.String ThisExe => $"{System.Environment.GetCommandLineArgs()[0]}";
        internal static CmdLineData ProgramTempValues = new CmdLineData();
        internal static OperationType OP = OperationType.SpecificType;

		public static System.Int32 Main(System.String[] Args)
		{
            System.String DF;
            System.Int32 I = 0;
            AppDomain.CurrentDomain.UnhandledException += UnhandledToErrorTranslator;
            ROOT.ConsoleExtensions.OutputEncoding = System.Text.Encoding.UTF8;
            ROOT.ConsoleExtensions.InputEncoding = System.Text.Encoding.UTF8;
            foreach (System.String _DF in Args)
            {
                DF = _DF.ToLowerInvariant();
                if (DF == "--help" || DF == "-h" || DF == "-?" || DF == "/?") { PrintHelp(); return 1; }
                if (DF == "--ver" || DF == "-v") { MAIN.WriteConsoleText(Internals.VersionAndInfo); return 2; }
                if (DF.StartsWith("--pfilelocation=")) { ProgramTempValues.FileSearchLoc = _DF.Substring(_DF.IndexOf('=') + 1); }
                if (DF == "--show-exception-info" || DF == "-sei") { ProgramTempValues.ExceptionInfoEnabled = true; }
                if (DF == "--verbosity" || DF == "--analytical" || DF == "-verb") { ProgramTempValues.IsVerbosityEnabled = true; }
                if (DF == "--all" || DF == "-a") { OP = OperationType.AllTypesInAssembly; }
            }

            DF = null;
            foreach (System.String DG in Args) 
            {
                if (DG.Substring(0 , 1) == "-" || DG.Substring(0, 1) == "/") { /* do nothing.*/ } else 
                {
                    if (I == 0) { ProgramTempValues.AssemblyPath = DG; }
                    if (I == 1) { ProgramTempValues.TypeOfInterface = DG; }
                    if (I == 2) { ProgramTempValues.TypeOfDerivedClass = DG; }
                    I++;
                }
            }

            if (ProgramTempValues.AllArgsAreDefault) { ShowError(); return 1; }

            MAIN.WriteConsoleText("Starting operation.");
            Runner GI = new() { OfIntInterface = ProgramTempValues.TypeOfInterface, OfPseudoClass = ProgramTempValues.TypeOfDerivedClass };
            MAIN.WriteConsoleText($"Loading assembly: {ProgramTempValues.AssemblyPath} ");
            GI.FindAndLoadAssemblyThroughReflection();
            MAIN.WriteConsoleText($"Getting operation and starting up...");
            if (OP == OperationType.AllTypesInAssembly) 
            {
                MAIN.WriteConsoleText($"Getting all types in assembly... (Assembly {GI.LoadedAssembly.FullName})");
                GI.FindAllClassesThatHaveAsInterfaceAttr();
                MAIN.WriteConsoleText($"Checking up... (Found {GI.ClassData.Count} items.)");
                GI.RunChecksAgainstAll();
            } else 
            {
                System.Type T1 = null;
                System.Type T2 = null;
                foreach (System.Type GD in GI.LoadedAssembly.GetTypes())
                {
                    if (GD.FullName == ProgramTempValues.TypeOfInterface) { T1 = GD; }
                    if (GD.FullName == ProgramTempValues.TypeOfDerivedClass) { T2 = GD; }
                }
                if (T1 == null || T2 == null) { 
                    Logger.WriteFatal("Cannot continue execution because the defined types do not exist in the defined assembly:\n" +
                    $"Loaded assembly: {GI.LoadedAssembly.FullName}\n" +
                    $"Defined Interface Type: {ProgramTempValues.TypeOfInterface}\n" +
                    $"Defined Class Implementation Type: {ProgramTempValues.TypeOfDerivedClass}");
                    return 2;
                }
                AsInterfaceChecker AIC = new(T1, T2);
                MAIN.WriteConsoleText("Checking implementation...");
                AIC.Check();
            }
            MAIN.WriteConsoleText("Done!");
            return 0;
		}

        private static void ShowError() 
        {
            MAIN.WriteConsoleText("Error. Insufficient arguments supplied. Expected at least 3.\n" +
                "Use the --help option for more information.");
        }

        private static void PrintHelp()
        {
            MAIN.WriteConsoleText(
                $"{Internals.VersionAndInfo}" +
                "Tool for detecting whether an assembly has the custom AsInterface attribute\n" +
                "and checks whether the attribute is correctly used.\n" +
                "\nCommand-Line Usage:\n\n" +
                $"{ThisExe} [Options] {Internals.AssemblyPathArgName} {Internals.InterfaceTypeArgName} {Internals.ClassThatUsesTheAttrArgName}\n\n" +
                "Where [Options] are: \n" +
                "1. --help , -h , /? , -?: Displays this help text. Even if you supply all the arguments , and this option exists,\n" +
                "then this help prompt will be shown up.\n" +
                "2. --ver , -v: Shows the version of this tool and exits.\n" +
                "3. \"--PFileLocation=path\": Instructs the tool to probe for the Managed .NET DLL from PATH variable, or to attempt \n" +
                "to load it using the fallback path defined in the \'path\' value.\n" +
                "4. --Show-Exception-Info , -sei: Shows the actual exception information instead of just simply showing a message.\n" +
                "If no exceptions were found , then no information will be displayed.\n" +
                "5. --Verbosity , --Analytical , -verb: Shows the full activity that the tool does.\n" +
                "6. --All , -a: Runs the check in ALL classes in the specified assembly. DO NOT add the\n" +
                $"{Internals.ClassThatUsesTheAttrArgName} and {Internals.InterfaceTypeArgName} arguments " +
                "since these are NOT needed.\n" +
                "Notes:\n" +
                $"1. The Option 4 requires the directory to search the file requested in the {Internals.AssemblyPathArgName} argument.\n" +
                $"Additionally , the {Internals.AssemblyPathArgName} must contain the file name only.\n\n" +
                $"The {Internals.AssemblyPathArgName} argument:\n" +
                "This argument specifies the module to load , which from it the type will be loaded to check the AsInterface attribute.\n" +
                $"The {Internals.InterfaceTypeArgName} argument:\n" +
                $"This defines the fully qualified type of the interface that the {Internals.ClassThatUsesTheAttrArgName} \n" +
                "pseudo-implements the interface defined in the attribute.\n" +
                $"The {Internals.ClassThatUsesTheAttrArgName} argument:\n" +
                "The class that uses the AsInterface attribute , which pseudo-implements the type of interface that the attribute holds.\n");
        }
		
        internal static void ShowErrorAndExit(System.Object sender , ErrorDetectedEventArgs e)
        {
            MAIN.WriteConsoleText($"Error in execution: {e.Error} \n" +
                $"Exception data: {e.Cause}\n" +
                $"Sended by: {sender}");
            System.Environment.Exit(4);
        }

        internal static void UnhandledToErrorTranslator(System.Object sender , UnhandledExceptionEventArgs e) 
        {
            Runner.InvokeErrorDetected(sender, new ErrorDetectedEventArgs("Unhandled Exception", (System.Exception)e.ExceptionObject));
        }
    }
	
}

internal struct Internals
{
    public const System.String VersionAndInfo = 
        "MDCFR AsInterface attribute implementer checker , Version 1.0.0.1\n" +
        "[MDCFR Tool]\n" +
        "© MDCDI1315.\n";

    public const System.String AssemblyPathArgName = "{AssemblyPath}";

    public const System.String InterfaceTypeArgName = "{InterfaceTypeToCheck}";

    public const System.String ClassThatUsesTheAttrArgName = "{ClassThatUsesTheAttribute}";
}

internal static class Logger
{

    public static void WriteVerbose(System.String Text)
    { 
        System.DateTime str = System.DateTime.Now;
        if (MDCFR.AsInterfaceValidator.ProgramTempValues.IsVerbosityEnabled) 
        {
            MAIN.WriteConsoleText($"VERBOSE: [{str.Year}/{str.Month}/{str.Day} {str.Hour}:{str.Minute}.{str.Millisecond}] {Text}");
        } 
    }

    public static void WriteFatal(System.String Text) { MAIN.WriteConsoleText($"ERROR: Could not complete operation: {Text}"); }

    public static void WriteException(System.Exception Object) 
    {
        System.String FormatObject;
        if (MDCFR.AsInterfaceValidator.ProgramTempValues.ExceptionInfoEnabled)
        {
            FormatObject = $"Exception detected. Data: {Object}\n";
            if (MDCFR.AsInterfaceValidator.ProgramTempValues.IsVerbosityEnabled) { FormatObject += $"\nStack Trace: {System.Environment.StackTrace}"; }
            MAIN.WriteConsoleText(FormatObject); WriteFatal("See the above line for more information.");
            Runner.InvokeErrorDetected("ExceptionHandler",  new ErrorDetectedEventArgs("ExceptionHandler" , Object));
        } else { MAIN.WriteConsoleText($"An error was detected. The error caused by: {Object.Message}"); 
            WriteFatal("See the above line for more information."); System.Environment.Exit(4); }
    }
}