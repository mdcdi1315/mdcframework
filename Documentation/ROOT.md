# The ROOT Namespace
It is the central namespace of this Framework and includes functions which are implemented by me.

 It includes: Generic methods , Archiving and cryptography operations.
## The MAIN Class
CSharp declaration:
```C#
public static class ROOT.MAIN
```
Description:
The _MAIN_ Class contains static methods that assist a programmer develop quickly an application.

### Classes:
The Classes nested inside this one are:
  
 1. The IntuitiveConsoleText class , which shows a message to a console formatted and colored properly.

### Methods:
The Methods nested inside this one are (Sorted alphabetically):

#### 1. The AppendAnExistingFile:
```C# 
[System.ObsoleteAttribute("AppendAnExistingFile method is no longer required , because using ReadAFileUsingFileStream will normally open it with R/W permissions." + 
" Use then the AppendNewContentsToFile function to append data to that opened file." , true)]
public static System.IO.StreamWriter AppendAnExistingFile(System.String Path)
```
This will open the __existing__ file at _Path_ parameter to be opened for writing.

 __Returns__: `null` if it is unsucessfull , a new `System.IO.StreamWriter` instance if sucessfull.

 __NOTICE__: This function has been replaced with AppendNewContentsToFile , which works better and is not required.
#### 2. The AppendNewContentsToFile:
```C#
public static void AppendNewContentsToFile(System.String Contents , System.IO.FileStream FileStreamObject)
```
This function appends the specified string to a opened System.IO.FileStream with write permissions.

 __Parameters__:
 1. `System.String Contents`: The contents to write to the target in the form of a string.
 2. `System.IO.FileStream FileStreamObject`: The FileStream to write the target data.

 __Returns__: The function is declared as `void` , so it does not return something.
 
#### 3. The CheckIfStartedFromSpecifiedOS:
```C#
public static System.Boolean CheckIfStartedFromSpecifiedOS(System.Runtime.InteropServices.OSPlatform OSP)
```
This function returns a Boolean value if the current OS running the aplication is matching the one supplied.

 __Parameters__:
 1. `System.Runtime.InteropServices.OSPlatform OSP`: One of the System.Runtime.InteropServices.OSPlatform enumeration values , which are:
      1. Linux
      2. OSX
      3. Windows
 
 __Returns__: If the specified OS is matching the one supplied to the `OSP` , then returns `true`; otherwise , `false`.
 
 #### 4. The ClearAndWriteAFile:
 ```C#
 public static System.IO.FileStream ClearAndWriteAFile(System.String Path)
 ```
 This function clears up the contents of an existing file and it initialises the newly created FileStream for writing.
 
   __Parameters__:
   1. `System.String Path`: The fully qualified or relative path to a _existing_ file.
  
   __Returns__: If the file is existing and the object created sucessfully , a new `System.IO.FileStream`; otherwise , `null`.
   
#### 5. The CopyFile:
```C#
public static System.Boolean CopyFile(System.String SourceFilePath , System.String DestPath, System.Boolean OverWriteAllowed = false)
```
This function copies a specified file to the specified target.

   __Parameters__:
   1. `System.String SourceFilePath`: The file to copy to. Must be an _existing_ one.
   2. `System.String DestPath`: The file destination. Must be with it's filename , for example: `"D:\\files\\aFile.txt"`. If it is an _existing_ one , you must set the `OverWriteAllowed` to `true`.
   3. `System.Boolean OverWriteAllowed = false`: _Optional_ value indicating to overwrite the file if in the target location this file exists. __Default Value__:`false`.
   
   __Returns__: `true` if the specified file was copied to the destination; otherwise , `false`.
#### 6. The CreateADir:
```C#
public static System.Boolean CreateADir(System.String Path)
```
This function will create the specified directory to the path requested.

  __Parameters__:
  1. `System.String Path`: The Directory Path that the directory must be created in.
  
  __Returns__: `true` if the directory requested created sucessfully; otherwise , `false`.
  
#### 7. The CreateANewFile:
```C#
public static System.IO.FileStream CreateANewFile(System.String Path)
```
This function will create the specified file in the specified path. Then , it will open it with Write permissions.

  __Parameters__:
  1. `System.String Path`: The file Path to save the file in.

  __Returns__: A new `System.IO.FileStream` if it was sucessfull; otherwise , `null`.
  
#### 8. The CreateLoadDialog:
~~~C#
public static MAIN.DialogsReturner CreateLoadDialog(System.String FileFilterOfWin32 , System.String FileExtensionToPresent ,System.String FileDialogWindowTitle)
public static MAIN.DialogsReturner CreateLoadDialog(System.String FileFilterOfWin32 , System.String FileExtensionToPresent ,System.String FileDialogWindowTitle , System.String DirToPresent)
~~~
___NOTICE___!!! This function is available only for _.NET Framework_ and __Windows Desktop__ builds!!!

 This function prompts the User to select and load a file.

  __Parameters__: 
  1. `System.String FileFilterOfWin32`: The file filter to use for limiting the search (It is an array of `';'`-seperated strings).
  2. `System.String FileExtensionToPresent`: The default file extension to present when the `FileFilterOfWin32` has more than two entries.
  3. `System.String FileDialogWindowTitle`: The title of the window that it is presented.

  __Overloads__:
  There is also an overloaded function which takes the below extra argument:
  
  `System.String DirToPresent`: The directory which this dialog must be opened to.
  
  __Returns__: a new `ROOT.MAIN.DialogsReturner` instance.

  __Example__: make a file filter , invoke the function and get the values.
~~~C#
    using static ROOT.MAIN;
    //...
    string File_Filter = "Text Documents|*.txt;Zip Archives|*.zip;Settings Text Document|settings.txt";
    string FileExt = ".txt";
    string title = "Open the File...";
    DialogsReturner DialogResult = CreateLoadDialog(File_Filter , FileExt , title);
    WriteConsoleText("The File Name is " + DialogResult.FileName);
    WriteConsoleText("The File Full path is " + DialogResult.FileNameFullPath);
    WriteConsoleText("Error Detected: " + DialogResult.ErrorCode);
    //...
~~~
#### 9. The CreateSaveDialog:
_Notice_: This function is not explained because it is the same with CreateLoadDialog.

Their difference is that the former has a "Save" button , while the latter has a "Load" button.
#### 10. The DeleteADir:
~~~C#
public static System.Boolean DeleteADir(System.String Path , System.Boolean DeleteAll)
~~~
This function deletes the specified directory.

  __Parameters__:
  1. `System.String Path`: The path of the directory to be deleted.
  2. `System.Boolean DeleteAll`: if `true` , then all the directory's contents will be deleted and then the directory itself. Otherwise , the directory must be empty so as to succeed.

  __Returns__: `true` if the directory deleted; otherwise , `false`.
  
#### 11. The DeleteAFile:
~~~C#
public static System.Boolean DeleteAFile(System.String Path)
~~~
This function deletes a specified file.

  __Parameters__:
  1. `System.String Path`: The path to the file you want to delete.
  
  __Returns__: `true` if the file deleted; otherwise , `false`.
#### 12. The DirExists:
~~~C#
public static System.Boolean DirExists(System.String Path)
~~~
This function checks if the specified path is existing as a directory.

  __Parameters__:
  1. `System.String Path`: The path to the directory you want to test.
  
  __Returns__: `true` if the path supplied exists; otherwise , `false`.
#### 13. The EmitBeepSound:
~~~C#
public static void EmitBeepSound()
~~~
This function plays a tone through the app to the computer.

 __Returns__: The function is declared as `void` , so it does not return something.
#### 14. The FileExists:
~~~C#
public static System.Boolean FileExists(System.String Path)
~~~
This function checks if the specified path is existing as a file.

  __Parameters__:
  1. `System.String Path`: The path to the file you want to test.
  
  __Returns__: `true` if the path supplied exists; otherwise , `false`.
#### 15. The GetACryptographyHashForAFile:
~~~C#
public static System.String GetACryptographyHashForAFile(System.String PathOfFile , System.String HashToSelect = "SHA1")
~~~
This function reads the specified file and gets the specified hash algorithm digest.

 __Parameters__:
 1. `System.String PathOfFile`: The path to the file you want to get the hash from.
 2. `System.String HashToSelect = "SHA1"`: _optional_ string that selects the digest algorithm to use. Valid Values:
      1. `"SHA1"`
      2. `"SHA256"`
      3. `"SHA384"`
      4. `"SHA512"`
      5. `"MD5"`

 __Returns__: the specified hash of that file; otherwise , the "Error" string.

#### 16. The GetADirDialog:
~~~C#
public static MAIN.DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent , System.String DialogWindowTitle)
public static MAIN.DialogsReturner GetADirDialog(System.Environment.SpecialFolder DirToPresent , System.String DialogWindowTitle , System.String AlternateDir)
~~~
___NOTICE___!!! This function is available only for __.NET Framework__ and __Windows Desktop__ builds!!!

 This function prompts the User to select a specified directory.
 
 __Parameters__:
 1. `System.Environment.SpecialFolder DirToPresent`: The Directory to start the prompt instance.
  
  The values are located [here](http://learn.microsoft.com/dotnet/api/system.environment.specialfolder?view=netframework-4.7.2).
  
 2. `System.String DialogWindowTitle`: The title to show to the dialog.

 __Overloads__:
 1. `System.String AlternateDir`: You can specify any directory path to this argument , provided that the `DirToPresent`
 argument is specified to `System.Environment.SpecialFolder.MyComputer`
 
 __Returns__:
 A new `MAIN.DialogsReturner` instance.
 
 __Overloads__:
 There is also an overloaded function which takes the below extra argument:
 
 `System.String AlternateDir`: The path to a start directory , if the `DirToPresent` is set to 'MyComputer'.
 
 #### 17.The GetANewFileSystemInfo:
 ~~~C#
 public static System.IO.FileSystemInfo[] GetANewFileSystemInfo(System.String Path)
 ~~~
 This function gets a new `System.IO.FileSystemInfo` array which can be used to enumerate files and directories.
 
  __Parameters__:
  1. `System.String Path`: An existing directory path.

  __Returns__:
  A new `System.IO.FileSystemInfo[]` if it succeeded; otherwise , `null`.
  
  __Example__:

~~~C#
    using System;
    using System.IO;
    
   FileSystemInfo[] Array = GetANewFileSystemInfo("C:\\files");
   foreach(System.IO.FileSystemInfo FSI in Array)
   {
      //The below statement tests if the object took from the array is a file.
      if (FSI.GetType() == typeof(System.IO.FileInfo)) {Console.WriteLine("File: " + FSI.FullName);}
      //The below statement tests if the object took from the array is a directory.
      if (FSI.GetType() == typeof(System.IO.DirectoryInfo)) {Console.WriteLine("Directory: " + FSI.FullName);}
   }
    /* 
     * This will display output like the following:
     * Directory: C:\files\mdcframework
     * File: C:\files\mdcframework\README.md
     * File: C:\files\mdcframework\LICENSE
     * File: C:\files\mdcframework\MDCFR.csproj
     * File: C:\files\mdcframework\MDCFR.sln
    */
  ~~~
 
 #### 18. The GetAStringFromTheUser:
 ~~~C#
[System.Obsolete("The Visual Basic Runtime Library will be removed in the next major version. Use instead the GetAStringFromTheUserNew function.", true)]
 public static System.String GetAStringFromTheUser(System.String Prompt, System.String Title, System.String DefaultResponse)
 ~~~
 This function is an exact implementation of the [`Microsoft.VisualBasic.Interaction.InputBox`](https://learn.microsoft.com/dotnet/api/microsoft.visualbasic.interaction.inputbox?view=netframework-4.7.2),
 which is imported here if you want to use it , but it is not needed to reference the `Microsoft.VisualBasic` DLL.

  __Attributes__: `System.Obsolete(message , true)`:
  Someone who attempts to use this method will always throw a compiler error because in the next major version the library 
  the Visual Basic Runtime Library will be removed.
 
  __Parameters__:
  1. `System.String Prompt`: This is the prompt message shown to the user.
  2. `System.String Title`: The title of this window.
  3. `System.String DefaultResponse`: The default answer to the prompt. Can be `null` , suggesting that there is not a default answer.
  
  __Returns__:
  The string as an answer taken from the user; otherwise , `null` if it cancelled or returned an empty string.
  
 #### 19. The GetContentsFromFile:
 ~~~C#
 public static System.String GetContentsFromFile(System.IO.FileStream FileStreamObject)
 ~~~
 This function gets all the contents of a `System.IO.FileStream` object with Read permissions at least , and 
 gets it's contents as a `System.String`.
 
  __Parameters__:
  1. `System.IO.FileStreamObject`: The file object to get the data from. __Remember__ that the object must be active and it must have at least Read permissions.
  
  __Returns__:
  A `System.String` containing all the read data; if any of the above situations are not met or an error detected , then returns `null`.
  
#### 19. The GetContentsFromStreamReader:
~~~C#
[System.ObsoleteAttribute("GetFileContentsFromStreamReader method has been replaced with the GetContentsFromFile function , which performs better at performance level." +
"You should notice that sometime this function will be removed without prior notice.", false)]
public static System.String GetFileContentsFromStreamReader(System.IO.StreamReader FileStream)
~~~
 This function gets all the contents of a `System.IO.StreamReader` object with Read permissions at least , and 
 gets it's contents as a `System.String`.
 ___NOTICE___: This function is deprecated and have limitations on usage; Use instead the `MAIN.GetContentsFromFile` function instead.
 
 __Parameters__:
 1. `System.IO.StreamReader FileStream`: The file object to get the data from. __Remember__ that the object must be active and it must have at least Read permissions.
 
 __Returns__:
  A `System.String` containing all the read data; if any of the above situations are not met or an error detected , then returns `null`.
  
#### 20. The GetPathEnvironmentVar:
~~~C#
public static System.String[] GetPathEnvironmentVar()
~~~
This function returns an array of folder paths , which are conventions for easily accessing executables and scripts.

 __Parameters__: This method does not accept any parameters.
 
 __Returns__: A new `System.String[]` containing the folder paths; otherwise , `null`.
 
#### 21. The GetRuntimeVersion:
~~~C#
public static System.String GetRuntimeVersion()
~~~
This function returns the specified runtime version. For informational purposes only.

 __Parameters__: This method does not accept any parameters.
 
 __Returns__: The `System.String` describing the runtime version.
 
#### 22. The GetVBRuntimeInfo: 
~~~C#
public static System.String GetVBRuntimeInfo()
~~~
This function gets the version of the currently loaded assembly `Microsoft.VisualBasic`. For informational purposes only.

 __Parameters__: This method does not accept any parameters.
 
 __Returns__: The `System.String` describing the `Microsoft.VisualBasic` assembly version.
 
 #### 23. The HaltApplicationThread:
 ~~~C#
 public static void HaltApplicationThread(System.Int32 TimeoutEpoch)
 ~~~
 This Function stops the aplication thread (Or any that this command is run under) for the 
 miliseconds specified.
 
  __Parameters__:
  1. `System.Int32 TimeoutEpoch`: The Timeout time specified. Maximum value that it accepts is [here](http://learn.microsoft.com/dotnet/api/System.Int32.MaxValue?view=netframework-4.7.2).

  __Returns__:
  This function is declared as `void` , so it does not return nothing. That it does is to stop only the application thread for the time specified.
  
#### 24. The LaunchProcess:
~~~C#
public static System.Int32 LaunchProcess(System.String PathOfExecToRun , System.String CommandLineArgs , System.Boolean ImplicitSearch , System.Boolean WaitToClose)
public static System.Int32 LaunchProcess(System.String PathOfExecToRun , System.String CommandLineArgs = " " , System.Boolean ImplicitSearch = false , System.Boolean WaitToClose = false, System.Boolean RunAtNativeConsole = false , System.Boolean HideExternalConsole = true)
~~~
This function opens and runs a process with the defined arguments and settings.
The function uses the Machine's Shell Context , and __NOT__ the .NET Context. 

 __Parameters__:
 1. `System.String PathOfExecToRun`: The Path of the executable to open. 
 2. `System.String CommandLineArgs`: The Command-Line arguments to specify. If you do not need , just pass `null` or an empty string with a space `" "`.
 3. `System.Boolean ImplicitSearch`: This determines whether the executable must be found using guessing; that means that it acts like a cmd.exe window when passing a command.
 4. `System.Boolean WaitToClose`: This determines if the function should wait the child application to exit so as to complete. Also returns the exit code.
 
 __Overloads__:
 1. `System.Boolean RunAtNativeConsole = false`: This runs the executable either at the executable that this function called or to a new Process context.
 2. `System.Boolean HideExternalConsole = true`: This will hide the external console if the called app uses one. However , this is used only when `RunAtNAtiveConsole` is `false`.
 
 __Returns__:
 An `System.Int32` number , which returns:
 
 1. `-8` if the path or the app supplied is a non-existent one.
 2. `-10337880` if the process for this environment is illegal (i.e. wrong executable architecture).
 3. `-10337881` for a generic error.
 4. Or , if sucessfull , `0` , and if the `WaitToClose` parameter is set to `true` , then the exit code of the child process.
 
 __Example__:
~~~C#
 // Call a process using the guessing system.
 using ROOT;
 using static ROOT.MAIN;
 
 // ..
     MAIN.LaunchProcess("cmd.exe" , "/c Echo." , true , false);
     /* The above statement will launch cmd.exe with the argument /c Echo. , and the guessing system will be used. 
      * The below one will do the same , but without the guessing system.
     */
     MAIN.LaunchProcess("C:\\Windows\\System32\\cmd.exe" , "/c Echo." , false , false);
 // ..
 ~~~
 
#### 25. The MoveFilesOrDirs:
~~~C#
public static System.Boolean MoveFilesOrDirs(System.String SourceFilePath , System.String DestPath)
~~~
This function moves a file or directory , depending by the `SourceFilePath` and `DestPath` parameters.
__NOTE__: When moving files , the filename with the path is required at both arguments , like: `C:\\Files\\Start.cmd`.

 __Parameters__:
 1. `System.String SourceFilePath`: The source file or directory that will be moved.
 2. `System.String DestPath`: The new file or directory that will be moved to.

 __Returns__:
 if moving was sucessfull , then `true`; otherwise , `false`.

#### 26. The NewMessageBoxToUser:
~~~C#
public static System.Int32 NewMessageBoxToUser(System.String MessageString , System.String Title , System.Windows.Forms.MessageBoxButtons MessageButton = MessageBoxButtons.OK , System.Windows.Forms.MessageBoxIcon MessageIcon = MessageBoxIcon.None)
~~~
This function shows a new Message Box window to user and prompts him to do an action , based on the buttons selected.

 __Parameters__:
 1. `System.String MessageString`: The message to show. Can have multiple lines and feed characters.
 2. `System.String Title`: The window's title of this message.
 3. `System.Windows.Forms.MessageBoxButtons MessageButton = MessageBoxButtons.OK`: This selects the buttons that will exist as options to do in that window.
 4. `System.Windows.Forms.MessageBoxIcon MessageIcon = MessageBoxIcon.None`: This selects and a icon too to show among with the message.
 
 The valid values for the button and icon selection are [here](http://learn.microsoft.com/dotnet/api/system.windows.forms.messageboxbuttons?view=netframework-4.7.2) and [here](http://learn.microsoft.com/dotnet/api/system.windows.forms.messageboxicon?view=netframework-4.7.2) , respectively.
 
 __Returns__:
 An `System.Int32` value , which indicates that:
 1. `1` when 'OK' button pressed.
 2. `2` when 'Cancel' button pressed.
 3. `3` when 'Abort' button pressed.
 4. `4` when 'Retry' button pressed.
 5. `5` when 'Ignore' button pressed.
 6. `6` when 'Yes' button pressed.
 7. `7` when 'No' button pressed.
 8. `0` indicates an error or the [System.Windows.Forms](http://learn.microsoft.com/dotnet/api/system.windows.forms?view=netframework-4.7.2) DLL version deprecation.
 
#### 27. The OSFramework:
~~~C#
public static System.String OSFramework()
~~~
This function displays information about the OS Current Framework that the application called this function runs on.
(For informational purposes only.)

__Returns__: An `System.String` value which contains the current OS Framework information.

#### 28. The OSInformation:
~~~C#
public static System.String OSInformation()
~~~
This function displays information about the OS that the application called this function runs on.
(For informational purposes only.)

__Returns__: An `System.String` value which contains the current OS information.

#### 29. The OSProcessorArchitecture:
~~~C#
public static System.String OSProcessorArchitecture()
~~~
This function gets information about the OS Processor Architecture that the application called this function runs on.

 __Returns__: This function can return the below strings:
1. `"AMD64"` for 64-bit Unicode machines.
2. `"x86"` for 32-bit Unicode machines.
3. `"ARM"` for ARM machines.
4. `"ARM64"` for ARM 64-bit machines.
5. `"Error"` if an error was found or the processor architecture could not be determined by the OS.

#### 30: The PassNewcontentsToFile:
~~~C#
public static void PassNewContentsToFile(System.String Contents , System.IO.FileStream FileStreamObject)
~~~
This function writes the string data to an alive System.IO.FileStream object , which 
represents a file that is opened by the system.

 __Parameters__:

1. `System.String Contents`: The string data to pass to the file. Be noted that any existing data will be deleted.
2. `System.IO.FileStream FileStreamObject`: The alive file object to write the data to.

__Returns__: The function is declared as `void` , so it does not return something.

#### 31. The ProcessArchitecture:
~~~C#
public static System.String ProcessArchitecture()
~~~
This function returns the application's compiled architecture string.

 __Returns__: This function can return the below strings:
1. `"AMD64"` for 64-bit Unicode machines.
2. `"x86"` for 32-bit Unicode machines.
3. `"ARM"` for ARM machines.
4. `"ARM64"` for ARM 64-bit machines.
5. `"Error"` if an error was found or the application's compiled architecture could not be determined by the OS.

#### 32. The ReadAFile:
~~~C#
[System.ObsoleteAttribute("ReadAFile method has been replaced with the ReadAFileUsingFileStream function , which performs better at performance level." +
"You should notice that sometime this function will be removed without prior notice." , false)]
public static System.IO.StreamReader ReadAFile(System.String Path)
~~~
This function will create a new `System.IO.StreamReader` instance for the file specified.

__NOTICE__: This function is deprecated and in the next release , it will have the same functionality
as the `ReadAFileUsingFileStream` one.

 __Parameters__:
1. `System.String Path`: The file path that this function will open. Can be a relative or a full path.

 __Returns__: If the file is existing and all went good , will return a new `System.IO.StreamReader` instance; 
 otherwise , `null`.

#### 33. The ReadAFileUsingFileStream:
~~~C#
public static System.IO.FileStream ReadAFileUsingFileStream(System.String Path)
~~~
This function will create a new `System.IO.FileStream` instance for the file specified.

 __Parameters__:
1. `System.String Path`: The file path that this function will open. Can be a relative or a full path.

 __Returns__: If the file is existing and all went good , will return a new `System.IO.FileStream` instance; 
 otherwise , `null`.

#### 34. The RemoveDefinedChars:
~~~C#
public static System.String RemoveDefinedChars(System.String StringToClear , System.Char[] CharToClear)
~~~
This function will remove all the characters from the specified string and will return
the result.

 __Parameters__:
1. `System.String StringToClear`: The String whose the charaters defined in the `CharToClear` will be removed.
2. `System.Char[] CharToClear`: An array of characters to clear from the string. Can also be one chararcter in the array.

 __Returns__: The string after the defined chararcters were removed; if an error was found , `null`.

#### 35. The FindFileFromPath:
~~~C#
public static ROOT.FileSearchResult FindFileFromPath(System.String FileName)
~~~
This function searches up a file from the %PATH% environment variable and returns a 
new `ROOT.FileSearchResult` structure which contains whether the file was found , and
if yes , the fully qualified path to it.

Consult the `ROOT.FileSearchResult` structure for more information on how to get the
data exposed by this function.

 __Parameters__:
1. `System.String FileName`: The File name (only) to search for. (Example: doskey.exe)

 __Overloads__:
 This function has an overload too:
~~~C#
public static ROOT.FileSearchResult FindFileFromPath(System.String FileName, System.String[] Extensions)
~~~
 __Parameters__:
 1. `System.String FileName`: The File name (only) without it's extension to search for. (Example: doskey)
 2. `System.String[] Extensions`: Possible file extensions to find. (Example: { "exe" , "dll" , "rs" , "uce" , "vbs" , "csproj" })

__Returns__: A new `ROOT.FileSearchResult` structure.

#### 36. The WriteConsoleText:
~~~C#
public static void WriteConsoleText(System.String Text)
public static void WriteConsoleText(System.Char[] Text)
~~~
This function writes either as an `System.String` or an `System.Char[]` array the specified text to the application's console.

 __Parameters__:
 1. `System.String Text`: The text to write to the console.
 
 __Overloads__:
 1. `System.Char[] Text`: The text to write to the console.

 __Returns__: The function is declared as `void` , so it does not return something.

#### 37. The WriteCustomColoredText:
~~~C#
public static void WriteCustomColoredText(System.String Message, System.ConsoleColor ForegroundColor, System.ConsoleColor BackgroundColor)
~~~
This function also writes the specified text to the application's console , but colored as defined by the user.

 __Parameters__:
 1. `System.String Message`: The colored text to write to the console.
 2. `System.ConsoleColor ForegroundColor`: The foreground color (text) color to use.
 3. `System.ConsoleColor BackgroundColor`: The background color to use.
 
 __Returns__: The function is declared as `void` , so it does not return something.

 ### The IntuitiveConsoleText Class:
CSharp Declaration:
~~~C#
public static class ROOT.MAIN.IntuitiveConsoleText
~~~
IL Declaration:
~~~IL
.class abstract auto ansi sealed nested public beforefieldinit IntuitiveConsoleText
       extends [mscorlib]System.Object
~~~

#### Description:
A static class which creates console colored messages to differentiate the types of errors or information given.

### Methods:

#### 1. The InfoText:
~~~C#
public static void InfoText(System.String Text)
~~~
This function writes the data specified on the console. 
These data are informational. The background color is black and the foreground color is gray.

 __Parameters__:
 1. `System.String Text`: The text to write to the console.
 
__Returns__: The function is declared as `void` , so it does not return something.
 
#### 2. The WarningText:
~~~C#
public static void WarningText(System.String Text)
~~~
This function writes the data specified on the console. 
These data are warnings. The background color is black and the foreground color is yellow.

 __Parameters__:
 1. `System.String Text`: The text to write to the console.
 
__Returns__: The function is declared as `void` , so it does not return something.


#### 3. The ErrorText:
~~~C#
public static void ErrorText(System.String Text)
~~~
This function writes the data specified on the console. 
These data are errors. The background color is black and the foreground color is red.

 __Parameters__:
 1. `System.String Text`: The text to write to the console.
 
__Returns__: The function is declared as `void` , so it does not return something.

#### 4. The FatalText:
~~~C#
public static void FatalText(System.String Text)
~~~
This function writes the data specified on the console. 
These data are fatal errors. The background color is black and the foreground color is magenta.

 __Parameters__:
 1. `System.String Text`: The text to write to the console.
 
__Returns__: The function is declared as `void` , so it does not return something.
  
  -- End of the `ROOT.MAIN.IntuitiveConsoleText` Class --

  -- End of the `ROOT.MAIN` Class --

## The DialogsReturner Structure:
CSharp Declaration:
~~~C#
[SupportedOSPlatform("windows")]
public struct DialogsReturner
~~~
IL Declaration:
~~~IL
.class public sequential ansi sealed beforefieldinit ROOT.DialogsReturner
       extends [mscorlib]System.ValueType
{
  .custom instance void System.Runtime.Versioning.SupportedOSPlatformAttribute::.ctor(string) = ( 01 00 07 77 69 6E 64 6F 77 73 00 00 )             // ...windows..
}
~~~
Description:
A storage class used by the file/dir dialogs to access the paths given (Full and name only) , the dialog type ran and if there was an error.

Remarks:
This class is used only by several functions in the MAIN class. It is not allowed to override this class.

 ### Attributes:
 1. `System.Runtime.Versioning.SupportedOSPlatformAttribute("windows")`: This structure is only allowed to be used in the Windows platform.


 ### Constructors:
 You cannot use any constructor for this structure because all constructors are declared as `internal`.

 ### Properties:

 #### 1. DialogType:
 The `DialogType` property gets the dialog function ran.
 
 __Get__: Returns a value of the `ROOT.FileDialogType` enumeration. Consult it to find out the dialog types.

 #### 2. DirPath: 
 The `DirPath` property gets the directory selected by the user.
 
 __Note__: This property can only be used when the `DialogType` property is `ROOT.FileDialogType.DirSelect` , 
 otherwise it will throw an exception.

 __Get__: Returns a `System.String` value containing the directory path selected.

  #### 3. FileNameFullPath:
  The `FileNameFullPath` property gets the fully qualified path of the selected file.

  __Note__: This property can only be used when the `DialogType` property is _NOT_ `ROOT.FileDialogType.DirSelect` , 
 otherwise it will throw an exception.
  
  __Get__: Returns a `System.String` value containing the file path.

  #### 4. FileNameOnly:
  The `FileNameOnly` property gets the file name with it's extension only.
  
  __Note__: This property can only be used when the `DialogType` property is _NOT_ `ROOT.FileDialogType.DirSelect` , 
 otherwise it will throw an exception.
  
  __Get__: Returns a `System.String` value containing the file name only.
  
  #### 5. ErrorCode:
  The `ErrorCode` property gets the error code (if any) when the function was invoked.

  A generic error is suggested when this property returns the `"Error"` string.

  __Get__: Returns a `System.String` indicating if there was an execution error.

  -- End of `ROOT.FileDialogsReturner` structure --

## The DialogType Enumeration:
CSharp Declaration:
~~~C#
public enum FileDialogType : System.Int32
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed ROOT.FileDialogType
       extends [mscorlib]System.Enum
~~~
Description:
`FileDialogType` is an enumeration of values which indicate which dialog was invoked.

 ### Fields:
 
 #### 1. CreateFile:
 The `CreateFile` field suggests that the dialog is a Save File Dialog.
 
 #### 2. LoadFile:
 The `LoadFile` field suggests that the dialog is a Load File Dialog.

 #### 3. DirSelect:
 The `DirSelect` field suggests that the dialog is a Directory Selection Dialog.

 -- End of `ROOT.FileDialogType` Enumeration --

## The FileSearchResult Structure:
CSharp Declaration:
~~~C#
public struct FileSearchResult
~~~
IL Declaration:
~~~IL
.class public sequential ansi sealed beforefieldinit ROOT.FileSearchResult
       extends [mscorlib]System.ValueType
~~~
Description: The FileSearchResult struct is the return type for the file
	searcher functions defined in the `ROOT.MAIN` class.

 ### Constructors:
 You cannot use any constructor for this structure because all constructors are declared as `internal`.

### Properties:

  #### 1. Path:
  The `Path` property gets the full file path , if the file requested was found.

  __Get__: Returns a `System.String` containing the fully qualified path for the found file.

  #### 2. Extension:
   The `Extension` property gets the file's extension , if there is one.

   __Get__: Returns a `System.String` containing the extension of the file without the first dot.


  #### 3. MatchFound:
  The `MatchFound` property gets whether a match found by the function.

  __Get__: Returns a `System.Boolean` value containing whether a match was found.

  -- End of the `ROOT.FileSearchResult` Structure --

## The HW31 Structure:
CSharp Declaration: 
~~~C#
[System.Serializable]
public struct HW31 : System.IEquatable<HW31?>
~~~
IL Declaration:
~~~IL
.class public sequential ansi serializable sealed beforefieldinit ROOT.HW31
       extends [mscorlib]System.ValueType
       implements class [mscorlib]System.IEquatable`1<valuetype [mscorlib]System.Nullable`1<valuetype ROOT.HW31>>
{
  .custom instance void System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = ( 01 00 01 00 00 ) 
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
  .interfaceimpl type class [mscorlib]System.IEquatable`1<valuetype [mscorlib]System.Nullable`1<valuetype ROOT.HW31>>
  .custom instance void System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = ( 01 00 00 00 00 ) 
}
~~~
Description: `HW31` is a simple and easy to use binary data converter which converts byte arrays to strings 
and the opposite. This structure declares this format and expands it's usage.

  ### Implementations:
  This class implements the [`System.IEquatable`](http://learn.microsoft.com/en-us/dotnet/api/system.iequatable-1?view=netframework-4.8)
  interface. The implemented is the HW31 structure , but it allows `null` values when used in a nullable context.

  ### Attributes:
  1. `System.Runtime.CompilerServices.NullableAttribute()`: This attribute is used in conjuction with the 
  `System.IEquatable` interface stated above so as to implement the nullable structure.
  
  2. `System.Serializable()`: The runtime is allowed to serialize this structure , if required.
  
  ### Constructors:
  1. `HW31.HW31()`: The default constructor. The structure is initialised without data in it.
  However , you cannot add data to this structure. It should be used only in serialization
  contexts.
  
  2. `HW31.HW31(System.String HW31)`: Initiate the structure with the specified `HW31` string.
  
  Be noted , the string given must be an `HW31` one , otherwise the [`System.InvalidOperationException`](http://learn.microsoft.com/en-us/dotnet/api/system.invalidoperationexception?view=netframework-4.8)
  will be thrown.
  
  ### Methods:
  
  #### 1. The ClearLength:
~~~C#
public System.Int32 ClearLength()
~~~
  This function returns the number of characters (without the spaces) that are contained in this instance.

  __Returns__: A `System.Int32` value , indicating the number of characters.

  #### 2. The Equals:
~~~C#
public System.Boolean Equals(System.Object obj)
public System.Boolean Equals(ROOT.HW31? Struct)
~~~
  This function determines whether this instance is equal to a bare `HW31` string or to another `ROOT.HW31` structure.

  __Parameters__:
  1. `System.Object obj`: The bare `HW31` string to compare against.

  __Overloads__:
  1. `ROOT.HW31? Struct`: The another initialised `ROOT.HW31` structure to compare against.
  
  __Returns__: `true` if the object and this instance are the same ; otherwise . `false`.

  #### 3. The GetHashCode:
~~~C#
public System.Int32 GetHashCode()
~~~
   This function implements the basic usage of object value types.

   See the [`System.Object`](http://learn.microsoft.com/en-us/dotnet/api/system.object?view=netframework-4.8)
   documentation for more information.

   #### 4. The IsHW31:
~~~C#
public static System.Boolean IsHW31(System.String HW31)
~~~
Test whether a bare string can be an `HW31` string.

 Note: This function is static , which means that you can use it without instantiating a new structure.  

 __Returns__: `true` if the bare string given can be an `HW31` one , otherwise , `false`.

  #### 5. The IsInvalid:
~~~C#
public System.Boolean IsInvalid()
~~~
 Gets a `System.Boolean` value indicating that the structure is invalidated and should be disposed.

 __Returns__: `true` if the structure is invalid and should be disposed; otherwise , `false`.

  #### 6. The Length:
  This method is identical as the `System.String.Length` property , because this function returns the length
  of the kept HW31 string. 

  Please see the [`System.String.Length`](http://learn.microsoft.com/en-us/dotnet/api/system.string.length?view=netframework-4.8) 
  property documentation for more information.

  #### 7. The ToString:
~~~C#
public System.String ToString()
~~~
  ___NOTE___: This method is to support the product infrastracture and not meant to be used directly in your code.

  Returns the `HW31` string representation of this instance. 

  __Exceptions__: `System.InvalidOperationException`: An attempt was made to access the invalidated structure.

  __Returns__: The `HW31` string representation of this instance.

  ### Operators:

  ### Equality operators:
~~~C#
[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static System.Boolean operator ==(HW31 left, System.String right)

[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static System.Boolean operator ==(HW31 lhs, HW31 rhs)
~~~

  #### Operator 1: 
   Test the equality of an HW31 construct with a bare HW31 string.
    
   __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

   This method is aggressively inlined so as to offer speed in multiple `if` constructs.

   __Parameters__: 
    1. `ROOT.HW31 left`: The HW31 construct to compare against.
    2. `System.String right`: The HW31 string to compare against.
   
  #### Operator 2:
   Test the equality of two HW31 constructs.
    
   __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

   This method is aggressively inlined so as to offer speed in multiple `if` constructs.

   __Parameters__: 
    1. `ROOT.HW31 lhs`: The one HW31 construct to compare against.
    2. `ROOT.HW31 rhs`: The other HW31 construct to compare against.

   ### Inequality operators:
~~~C#
[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static System.Boolean operator !=(HW31 left, System.String right)

[System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
public static System.Boolean operator !=(HW31 lhs, HW31 rhs)
~~~

#### Operator 1: 
   Test the inequality of an HW31 construct with a bare HW31 string.
    
   __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

   This method is aggressively inlined so as to offer speed in multiple `if` constructs.

   __Parameters__: 
    1. `ROOT.HW31 left`: The HW31 construct to compare against.
    2. `System.String right`: The HW31 string to compare against.
   
  #### Operator 2:
   Test the inequality of two HW31 constructs.
    
   __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

   This method is aggressively inlined so as to offer speed in multiple `if` constructs.

   __Parameters__: 
    1. `ROOT.HW31 lhs`: The one HW31 construct to compare against.
    2. `ROOT.HW31 rhs`: The other HW31 construct to compare against.


  --- End of `ROOT.HW31` Structure ---

## The HW31Strings Class:

~~~C#
public static class HW31Strings
~~~
IL Declaration:
~~~IL
.class public abstract auto ansi sealed beforefieldinit ROOT.HW31Strings
       extends [mscorlib]System.Object
~~~

Description: The HW31Strings is the static class which has the methods to convert a byte array
to strings and the opposite.

### Methods:

#### 1. The ByteArrayToHW31String:
CSharp Declaration:
~~~C#
[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
public static HW31 ByteArrayToHW31String(System.Byte[] Array)
[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
public static HW31 ByteArrayToHW31String(System.Byte[] Array, System.Int32 Start, System.Int32 Count)
~~~
Converts the given byte array to a new `ROOT.HW31` structure.

 __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

   This method is aggressively inlined so as to decrease the execution time.

 __Parameters__:
 1. `System.Byte[] Array`: The byte array data to convert.

 __Overloads__:
 
 2. `System.Int32 Start`: The index of the `Array` parameter which the function will start converting data to HW31 symbols.
 
 3. `System.Int32 Count`: The items to copy from the `Array` parameter.

 __Returns__:

 If the `Start` parameter was `Start < Count - 1` , the Count parameter was less than the length of the array and execution suceeeded , 
 a new uninvalidated `HW31` structure; otherwise , the structure returned is invalidated and should be disposed immediately.

#### 2. The EstimateHW31StringLength:
CSharp Declaration:
~~~C#
[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
public static System.Int64 EstimateHW31StringLength(System.Byte[] Array)
~~~
Estimate the HW31 string length before it is even produced.

 __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

   This method is aggressively inlined so as to decrease the execution time.

__Parameters__:
 1. `System.Byte[] Array`: The byte array data to get the length from.

__Returns__:
 A `System.Int64` value that is the estimated HW31 length.

#### 3. The HW31StringToByteArray:
CSharp Declaration:
~~~C#
[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
public static System.Byte[] HW31StringToByteArray(HW31 HW31String)
~~~
Return the byte array data back from an encoded HW31 structure.

 __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

This method is aggressively inlined so as to decrease the execution time.

 __Parameters__:
 1. `ROOT.HW31 HW31String`: The `HW31` structure to get the data from.

 __Returns__: if the structure was an HW31 string and execution was finished sucessfully , then
 it returns the de-coded array; otherwise , `null`.

  -- End of class `ROOT.HW31Strings` --


