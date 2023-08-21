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
[System.Obsolete("The Visual Basic Runtime Library will be removed in the next major version." , true)]
public static System.String GetVBRuntimeInfo()
~~~
This function gets the version of the currently loaded assembly `Microsoft.VisualBasic`. For informational purposes only.

 __Parameters__: This method does not accept any parameters.
 
 __Returns__: The `System.String` describing the `Microsoft.VisualBasic` assembly version.
 
 __NOTICE__: The Visual Basic Runtime Library will be removed in the next major version , so calling this function always
throws a compiler error.

 #### 23. The HaltApplicationThread:
 ~~~C#
 public static void HaltApplicationThread(System.Int32 TimeoutEpoch)
 ~~~
 This Function stops the aplication thread (Or any that this command is run under) for the 
 miliseconds specified.
 
  __Parameters__:
  1. `System.Int32 TimeoutEpoch`: The Timeout time specified. Maximum value that it accepts is [here](http://learn.microsoft.com/dotnet/api/System.Int32.MaxValue?view=netframework-4.7.2).

  __Returns__:
  This function is declared as `void` , so it does not return nothing. What it does is to stop only the application thread for the time specified.
  
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
public static System.Int32 NewMessageBoxToUser(System.String MessageString , System.String Title , 
System.Windows.Forms.MessageBoxButtons MessageButton = MessageBoxButtons.OK , 
System.Windows.Forms.MessageBoxIcon MessageIcon = MessageBoxIcon.None)
~~~
This function shows a new Message Box window to user and prompts him to do an action , based on the buttons selected.

 __Parameters__:
 1. `System.String MessageString`: The message to show. Can have multiple lines and feed characters.
 2. `System.String Title`: The window's title of this message.
 3. `System.Windows.Forms.MessageBoxButtons MessageButton = MessageBoxButtons.OK`: This selects the buttons that will exist as options to do in that window.
 4. `System.Windows.Forms.MessageBoxIcon MessageIcon = MessageBoxIcon.None`: This selects and a icon too to show among with the message.
 
 The valid values for the button and icon selection are [here](http://learn.microsoft.com/dotnet/api/system.windows.forms.messageboxbuttons?view=netframework-4.7.2) and 
 [here](http://learn.microsoft.com/dotnet/api/system.windows.forms.messageboxicon?view=netframework-4.7.2) , respectively.
 
 __Returns__:
 An `System.Int32` value , which indicates that:
 1. `1` when 'OK' button pressed.
 2. `2` when 'Cancel' button pressed.
 3. `3` when 'Abort' button pressed.
 4. `4` when 'Retry' button pressed.
 5. `5` when 'Ignore' button pressed.
 6. `6` when 'Yes' button pressed.
 7. `7` when 'No' button pressed.
 8. `0` indicates an error or the [`System.Windows.Forms`](http://learn.microsoft.com/dotnet/api/system.windows.forms?view=netframework-4.7.2) DLL version deprecation.
 
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

## The HashDigesetSeletion Enumeration:
CSharp Declaration:
~~~C#
public enum HashDigestSelection
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed ROOT.HashDigestSelection
       extends [mscorlib]System.Enum
~~~
Description:
This enumeration provides values so as these can be used in the `ROOT.MAIN.GetACryptographyHashForAFile(System.String , 
ROOT.HashDigestSelection)` overload.

It provides the available cryptography algorithms.

### Fields:

   #### 1. The SHA1: 
   This field selects the SHA1 algorithm. Using this field will produce a SHA1 hash algorithm.
   __NOTICE__: Microsoft has found out that this hash algorithm has the possibility to provide
   the same hash values if the files are almost the same. 

   If your case is the integrity , then you must use the `SHA256` field or a better algorithm 
   provided herein.

   #### 2. The SHA256:
   This field selects the SHA256 algorithm. Using this field will produce a SHA256 hash algorithm.

   #### 3. The SHA384:
   This field selects the SHA384 algorithm. Using this field will produce a SHA384 hash algorithm.

   #### 4. The SHA512:
   This field selects the SHA512 algorithm. Using this field will produce a SHA512 hash algorithm.

   #### 5. The MD5:
   This field selects the MD5 algorithm. Using this field will produce a MD5 hash algorithm.

 -- End of `ROOT.HashDigestSelection` enumeration --

## The ModifidableBuffer Structure: 
CSharp Declaration:
~~~C#
public struct ModifidableBuffer : System.Collections.Generic.IList<System.Byte>
~~~
IL Declaration:
~~~IL
.class public sequential ansi sealed beforefieldinit ROOT.ModifidableBuffer
       extends [mscorlib]System.ValueType
       implements class [mscorlib]System.Collections.Generic.IList`1<uint8>,
                  class [mscorlib]System.Collections.Generic.ICollection`1<uint8>,
                  class [mscorlib]System.Collections.Generic.IEnumerable`1<uint8>,
                  [mscorlib]System.Collections.IEnumerable
{
  .custom instance void [mscorlib]System.Reflection.DefaultMemberAttribute::.ctor(string) = ( 01 00 04 49 74 65 6D 00 00 ) 
}
~~~
Description: The `ModifidableBuffer` structure was designed to provide a modifidable byte array , 
which is used for transferring byte data to streams. 

This structure implements both the `System.Collections.Generic.IList` and the functionality that byte arrays
offer.

 __Implementations__: 

   1. `System.Collections.Generic.IList<System.Byte>`: This structure implements the 
    [IList](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=netframework-4.8)
    interface and it is the main abstraction type for this structure.
    
   2. `System.Collections.Generic.ICollection<System.Byte>`: This structure implements the
   [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
   interface since [IList](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=netframework-4.8)
   is implemented from it.
   
   3. `System.Collections.Generic.IEnumerable<System.Byte>`: This structure implements the
   [IEnumerable](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=netframework-4.8)
   interface since [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
   implements this interface.

   4. `System.Collections.IEnumerable`: This structure implements the 
   [IEnumerable](http://learn.microsoft.com/en-us/dotnet/api/system.collections.ienumerable?view=netframework-4.8)
   interface since the [IEnumerable<System.Byte>](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=netframework-4.8)
   interface is implemented from this interface.

### Constructors:

#### Parameterless Constructor:

CSharp Declaration:
~~~C#
public ModifidableBuffer()
~~~
Initiates a new `ModifidableBuffer` structure with no data in it.

#### Constructor 2:

CSharp Declaration:
~~~C#
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public ModifidableBuffer(System.Byte[] Value)
~~~
Initiates a new `ModifidableBuffer` structure and flushes it with data taken from a byte array.

  __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

  This constructor is aggressively inlined so as to decrease the execution time.

 __Parameters__:
   1. `System.Byte[] Value`: The byte array to flush data to the newly created structure.

#### Constructor 3:

CSharp Declaration:
~~~C#
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public ModifidableBuffer(System.Byte[] Value, System.Int32 Index, System.Int32 Count)
~~~
Initiates a new `ModifidableBuffer` structure and flushes it with data taken from a byte array , 

starting from an specified index in the array and copies the specified items.

  __Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

  This constructor is aggressively inlined so as to decrease the execution time.

 __Parameters__:
   1. `System.Byte[] Value`: The byte array to flush data to the newly created structure.
   2. `System.Int32 Index`: The index inside the array to start copying data to the structure.
   3. `System.Int32 Count`: The items from the array to copy to the structure.
   
### Properties:

#### The `this[System.Int32 Index]` property:

CSharp Declaration:
~~~C#
public System.Byte this[System.Int32 Index]
~~~

This property is an implemented member of the  [IList](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=netframework-4.8)
interface. Please see the documentation of the 
[Indexer Property](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1.item?view=netframework-4.8)
for more information about it.

#### The Count Property:

CSharp Declaration:
~~~C#
public System.Int32 Count
~~~
This property is an implemented member of the [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
interface. Please see the documentation of the 
[Count Property](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.count?view=netframework-4.8)
for more information about it.

#### The IsReadOnly Property:

CSharp Declaration:
~~~C#
public System.Boolean IsReadOnly
~~~
This property is an implemented member of the [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
interface. Please see the documentation of the 
[IsReadOnly Property](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.isreadonly?view=netframework-4.8)
for more information about it.

__NOTICE__: This property will always return `false`.

### Methods:

#### 1. The Add method:
CSharp Declaration:
~~~C#
public void Add(System.Byte Value)
~~~

This method is an implemented member of the [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
interface. Please see the documentation of the 
[Add Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.add?view=netframework-4.8)
for more information about it.

#### 2. The AddEntries method:
CSharp Declaration:
~~~C#
public void AddEntries(System.Int32 Times)
~~~
Adds new , blank entries in the instance , the number of them is specified by the Times parameter.

 __Parameters__:
 1. `System.Int32 Times`: The number of blank entries to add.

__Returns__: This function is declared as `void` , so it does not return nothing.
   What it does is to add the specified blank entries to the instance.

#### 3. The Clear method:
CSharp Declaration:
~~~C#
public void Clear()
~~~

This method is an implemented member of the [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
interface. Please see the documentation of the 
[Clear Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.clear?view=netframework-4.8)
for more information about it.

#### 4. The Contains method:
CSharp Declaration:
~~~C#
public System.Boolean Contains(System.Byte item)
~~~

This method is an implemented member of the [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
interface. Please see the documentation of the 
[Contains Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.contains?view=netframework-4.8)
for more information about it.

#### 5. The CopyTo method:
CSharp Declaration:
~~~C#
public void CopyTo(System.Byte[] Array, System.Int32 Index)
~~~

This method is an implemented member of the [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
interface. Please see the documentation of the 
[CopyTo Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.copyto?view=netframework-4.8)
for more information about it.

#### 6. The GetEnumerator method:
CSharp Declaration:
~~~C#
public System.Collections.IEnumerator GetEnumerator()
~~~

This method is an implemented member of the [IEnumerable](http://learn.microsoft.com/en-us/dotnet/api/system.collections.ienumerable?view=netframework-4.8)
interface. Please see the documentation of the 
[GetEnumerator Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.ienumerable.getenumerator?view=netframework-4.8)
for more information about it.


#### 7. The IndexOf method:
CSharp Declaration:
~~~C#
public System.Int32 IndexOf(System.Byte Value)
~~~

This method is an implemented member of the  [IList](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=netframework-4.8)
interface. Please see the documentation of the 
[IndexOf Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1.indexof?view=netframework-4.8)
for more information about it.

#### 8. The Insert method:
CSharp Declaration:
~~~C#
public void Insert(System.Int32 Index, System.Byte Value)
~~~

This method is an implemented member of the  [IList](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=netframework-4.8)
interface. Please see the documentation of the 
[Insert Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1.insert?view=netframework-4.8)
for more information about it.

#### 9. The Remove method:
CSharp Declaration:
~~~C#
public System.Boolean Remove(System.Byte item)
~~~

This method is an implemented member of the [ICollection](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1?view=netframework-4.8)
interface. Please see the documentation of the 
[Remove Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icollection-1.remove?view=netframework-4.8)
for more information about it.

#### 10. The RemoveAt method:
CSharp Declaration:
~~~C#
public void RemoveAt(System.Int32 Index)
~~~

This method is an implemented member of the  [IList](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1?view=netframework-4.8)
interface. Please see the documentation of the 
[RemoveAt Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ilist-1.removeat?view=netframework-4.8)
for more information about it.

#### 11. The ToArray method:
CSharp Declaration:
~~~C#
public void System.Byte[] ToArray()
~~~
This method creates a new byte array that contains the data that the instance contains.

 __Returns__: A new `System.Byte[]` containing the instance data.

#### 12. The ToString method:
CSharp Declaration:
~~~C#
[MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
public override System.String ToString()
~~~
Attempts to return the byte array data as an hexadecimal string.

If the data do not fit in an string , then it returns the type of this instance.

__Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

  This method is aggressively inlined so as to decrease the execution time.

 __Returns__: If the instance data can fit to an string , then it returns the data as an hexadcimal string.
Otherwise , the type of this instance.

### Explicit Operators:

#### Operator 1:
CSharp Declaration:
~~~C#
public static explicit operator System.Byte[](ModifidableBuffer instance)
~~~
Converts an `ROOT.ModifidableBuffer` instance to an byte array.
This operator is equal to calling the `ROOT.ModifidableBuffer.ToArray()` method.

 __Parameters__: 
 1. `ROOT.ModifidableBuffer instance`: The `ModifidableBuffer` instance to take the array from.
 
 __Returns__: The converted byte array data.

#### Operator 2:
CSharp Declaration:
~~~C#
public static explicit operator ModifidableBuffer(System.Byte[] Data)
~~~
Converts the specified byte array data to a new `ROOT.ModifidableBuffer` structure.

This operator is equal to calling the `ROOT.ModifidableBuffer..ctor(System.Byte[] Data)` constructor.

 __Parameters__:
 1. `System.Byte[] Data`: The byte array data that will be passed to a new `ROOT.ModifidableBuffer` structure.
 
 __Returns__: A new `ROOT.ModifidableBuffer` structure containing the given byte array data.

 -- End of `ROOT.ModifidableBuffer` structure --

## The RegEditor class:
CSharp Declaration:
~~~C#
[SupportedOSPlatform("windows")]
public class RegEditor : System.IDisposable
~~~
IL Declaration:
~~~IL
.class public auto ansi beforefieldinit ROOT.RegEditor
       extends [mscorlib]System.Object
       implements [mscorlib]System.IDisposable
{
  .custom instance void System.Runtime.Versioning.SupportedOSPlatformAttribute::.ctor(string) = ( 01 00 07 77 69 6E 64 6F 77 73 00 00 )             // ...windows..
}
~~~
Description: This is an easy-to-use Windows Registry Editor , based on the top of 
[`Microsoft.Win32.Registry`](http://learn.microsoft.com/en-us/dotnet/api/microsoft.win32.registry?view=netframework-4.8) class.

### Attributes:
 1. `System.Runtime.Versioning.SupportedOSPlatformAttribute("windows")`: This class is only allowed to be used in the Windows platform.

### Constructors:

#### Parameterless constructor:
CSharp Declaration:
~~~C#
public RegEditor() { }
~~~
Creates a new instance of `ROOT.RegEditor` , but without any Key path data to get or set.
The Key paths must be set by the RootKey and SubKey properties.

#### Constructor 1:
CSharp Declaration:
~~~C#
public RegEditor(RegRootKeyValues KeyValue, System.String SubKey)
~~~
Creates a new instance of `ROOT.RegEditor` with the specified Key paths.

### Properties:

#### 1. The RootKey property:
CSharp Declaration:
~~~C#
public RegRootKeyValues RootKey { get; set; }
~~~

This sets or gets the Root Registry Key. 
For a list of possible values , see the `ROOT.RegRootKeyValues` enumeration.

- __Get__: Gets the current set root registry key.
- __Set__: Sets a new root registry key.

#### 2. The SubKey property:
CSharp Declaration:
~~~C#
public System.String SubKey
~~~

This sets or gets the registry sub-key , the next key from the root one.

- __Get__: Gets the current set sub-key.
- __Set__: Sets a new registry sub-key key.

#### 3. The DiagnosticMessages property:
CSharp Declaration:
~~~C#
public System.Boolean DiagnosticMessages
~~~
Sets a `System.Boolean` value which determines if exception messages should be thrown on console.

__Set__: Setting to `true` enables the exception messages to be thrown; `false` prohibits them to be shown.

### Methods:

#### 1. The DeleteEntry method:
CSharp Declaration:
~~~C#
public RegFunctionResult DeleteEntry(System.String VariableRegistryMember)
~~~
Deletes a registry value defined by the `VariableRegistryMember` parameter.

__Parameters__: 
 1. `System.String VariableRegistryMember`: The name of the registry value to delete.

 __Returns__: One of the `ROOT.RegFunctionResult` enum values.
Consult the `ROOT.RegFunctionResult` enumeration for more information.

#### 2. The GetEntry method:
CSharp Declaration:
~~~C#
public System.Object GetEntry(System.String VariableRegistryMember)
~~~
Gets the registry value defined by the `VariableRegistryMember` parameter.

__Parameters__: 
 1. `System.String VariableRegistryMember`: The name of the registry value to get.
 
__Returns__: The registry value data. Must be converted to an `System.String` if the value is set as
of type REG_SZ .

#### 3. The SetEntry method:
CSharp Declaration:
~~~C#
public RegFunctionResult SetEntry(System.String VariableRegistryMember, RegTypes RegistryType, System.Object RegistryData)
~~~
Sets a new value in the registry value. It's name is defined by the `VariableRegistryMember` , it's type
by the `ROOT.RegTypes` enumeration , and it's data defined by the  `RegistryData` parameter.

If the registry value does not exist , it is created.

__Parameters__: 

 1. `System.String VariableRegistryMember`: The name of the registry value to get.
 2. `RegTypes RegistryType`: The registry value type to set. This value will determine which type the value will be.
 3. `System.Object RegistryData`: The data to set to the registry value.
 
  __Returns__: One of the `ROOT.RegFunctionResult` enum values.
Consult the `ROOT.RegFunctionResult` enumeration for more information.

#### 4. The Dispose method:
CSharp Declaration:
~~~C#
public void Dispose()
~~~
The Dispose method clears the key paths set and makes this class reusuable.

 __Returns__:
  This function is declared as `void` , so it does return nothing.
  What it does is to make the instance reusuable.

 -- End of `ROOT.RegEditor` Class --

## The RegFunctionResult Enumeration:
CSharp Declaration:
~~~C#
public enum RegFunctionResult
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed ROOT.RegFunctionResult
       extends [mscorlib]System.Enum
~~~
Description: This enumeration is used in some of the methods of the `ROOT.RegEditor` class
, which is the return type.

### Fields:

  #### 1. The Error Field:
   When one of the functions return this field , it suggests that the operation failed for unknown reason.
  
  #### 2. The Misdefinition_Error Field:
   When one of the functions return this field , it suggests that the operation failed because 
   the root and sub-key paths were incorrectly formed or invalid.
  
  #### 3. The InvalidRootKey Field:
   When one of the functions return this field , it suggests that the operation failed because
   the root key was invalid.

  #### 4. The Success Field:
   When one of the functions return this field , it means that the function 
   was executed sucessfully.

 -- End of the `ROOT.RegFunctionResult` enumeration --

## The RegRootKeyValues Enumeration:
CSharp Declaration:
~~~C#
public enum RegRootKeyValues
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed ROOT.RegRootKeyValues
       extends [mscorlib]System.Enum
~~~
Description: This enumeration has default Registry Root key values to be used for the
Root Key value in the `ROOT.RegEditor` class.

### Fields: 
   
#### 1. The CurrentClassesRoot Field:
   This field is the `"HKEY_CLASSES_ROOT"` root key represented in the registry.

   In that root key there are usually the executable-file associations (A 'D' file is opened by an 'G' executable).

#### 2. The LocalMachine Field:
   This field is the `"HKEY_LOCAL_MACHINE"` root key represented in the registry.

   You cannot modify values or sub-keys of this root key and in normal cases , 
   it should NOT be accessed at all ,

   because this root key contains the Windows settings and any change can cause system instability.
   
#### 3. The CurrentUser Field:
  This field is the `"HKEY_CURRENT_USER"` root key represented in the registry.

  This field is a dynamic one , because applications usually save their data there 
  
  and it changes for every user (The name also implies that is a sortcut for the
  settings data of the current user.)

#### 4. The CurrentConfig Field:
  This field is the `"HKEY_CURRENT_CONFIG"` root key represented in the registry.

  This root key is almost deprecated , just only defines some default settings for
  Windows and nothing else.

#### 5. The PerfData Field:
  This field is the `"HKEY_PERFORMANCE_DATA"` root key represented in the registry.

  Be noted , this root key is deprecated; provided for legacy compatibility.

#### 6. The UsersStore Field: 
   This field is the `"HKEY_USERS"` root key represented in the registry.

   This root key contains all the registered Windows users defined in this computer.

  -- End of `ROOT.RegRootKeyValues` Enumeration --

## The RegTypes Enumeration:
CSharp Declaration:
~~~C#
public enum RegTypes
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed ROOT.RegTypes
       extends [mscorlib]System.Enum
~~~
Description: The `RegTypes` enumeration defines common registry data types
for the value of a registry value. You will only need this enumeration in the 
`SetValue()` method in the `ROOT.RegEditor` class.

### Fields:

#### 1. The String Field: 
This field specifies that the data type will be a string value.

#### 2. The ExpandString Field: 
This field specifies that the data type will be a string value with environment variables.

#### 3. The QuadWord Field:
This field specifies that the data type will be a quad-word length byte array.

#### 4. The DoubleWord Field:
This field specifies that the data type will be a double-word length byte array.

 -- End of `ROOT.RegTypes` Enumeration --

## The STD Class:
CSharp Declaration:
~~~C#
public static class STD
~~~
IL Declaration:
~~~IL
.class public abstract auto ansi sealed beforefieldinit ROOT.STD
       extends [mscorlib]System.Object
~~~
Description: The STD Format is an message-color pair definition pseudo-progamming language 
intended to save and later show messages in an application. Where the data would be applied 
to does not matter , but it's goal is to save messages that either will be loaded later or
will be used in a resource-style format , by accessing the index of each message.

The files that are saved under this format have the extension ".std" .

Example:
~~~
# This is a comment.
# The below line is a version block.
{Version$01}
# The below line is a new STD Message with the foregorund color
# set to White , and the background color set to Black.
"A new message"$"White"$"Black"
~~~

Reserved Characters: 

`{` , `}` : The Opening and Closing Brakets specify a special data block.

`#` : Signifies a new STD Comment.

`$` : Data Seperator , like in the STD message you have seen above.

`"` : Double quote , seperates the STD data , which are literal values.

__STD Messages , and their syntax__.

An STD Message is comprised of the message data themselves , 
the foreground and the background colors.

Syntax:
~~~
"Your-Message"$"foreground-color"$"background-color"
~~~

Where `foreground-color` and `background-color` are literal colors , 
like "Cyan" or "Dark Blue" .

Both colors are case-insensitive , like "DARK Blue" or "CyAN" .

These above will be again translated as "Dark Blue" and "Cyan" .

__STD Special Data Blocks__.

The Open Braket and the Closed Braket specify a new , and special data block.

For the current STD version , 1 , only the Version block exists , which has the format:
~~~
{Version$'Version-Number'}
~~~
The Version block is a requirement for all STD serialised data or files .

Without it , an parser exception will be thrown.

The Version block specifies the STD Format version , which the parser can correctly 
translate the given input into STD Entries.

__STD Comments__

STD Comments are starting with an `#` character , and finish until just before the next line.

STD Comments are only single-line , and are not allowed to be in or after STD Messages or Data blocks.

Examples:
~~~
# This comment will be parsed sucessfully.
"# This is not a comment , because the character is in an STD message."$"Dark Cyan"$"Black"
"A Man tries to climb up a mountain."$"Dark Magenta"$"Dark Red" # This will be completely invalidated.
~~~

__STD Parser behavior__:
The behavior of the STD parser is simple and to the point:

"If the first or second character in the line is a `#` , then it is a comment."

"If the first or second character in the line is a `{` and closes with a `}` , then it is a data block."

"If the first or second character in the line is a `"` , then it might probably be an STD Message."

"If I cannot determine something , I will not throw an exception. Instead , it will be saved as an invalid sequence."

"If I cannot find an Version Block , then I will throw an exception , since I do not know which STD version to parse."

"Any reserved character found inside of an STD message , except for the colors , which are used in this format , 
it will be passed literally instead. I care to parse sucessfully the message , and not how to 
determine why it shouldn't be there actually."

These simple six clauses define the STD parser behavior. 

STD does ignore the reserved characters inside the strings , because someone might need them 
for a message.

__Parsed STD Data Handling__:

When serializing-deserialising data using the STD format , an intermediate structure called STD Context
is the temporary "buffer" for saving there the STD data before they are used or serialized again.

You can add or get STD entries from it. 

The rest of the stuff will be now explained while describing the classes that are
involved in this format.

### Methods:

#### 1. The Deserialize method:
CSharp Declaration:
~~~C#
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static STDContext Deserialize(System.String Data)
~~~
This method deserialises the data from an string and returns them
as a new STD Context.

__Attributes__: `System.Runtime.CompilerServices.MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)`:

  This method is aggressively inlined so as to decrease the execution time.

__Parameters__:
    1.  `System.String Data`: The STD data to deserialise. Can be locally created in the application
session or from an external read file.

__Returns__: A new `ROOT.STDContext` structure containing the deserialised data.

Remarks: This method will throw an `System.InvalidOperationException` in one of these cases:

1. The method has not found an valid Version block.
2. The method could not deserialise a color specified in the STD Message.
3. The STD message was unterminated (invalid , corrupted).

#### 2. The Serialize method:
CSharp Declaration:
~~~C#
[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static System.String Serialize(STDContext Context)
~~~

This method serialises the given data from an STD Context and converts them
back to the STD Format.

__Parameters__:
   1. `ROOT.STDContext Context`: The STD Context to get the data from.
   
__Returns__: The serialised STD Format data.

 -- End of `ROOT.STD` Class --

## The STDColors Structure:
CSharp Declaration:
~~~C#
[Serializable]
public readonly struct STDColors
~~~
IL Declaration:
~~~IL
.class public sequential ansi serializable sealed beforefieldinit ROOT.STDColors
       extends [mscorlib]System.ValueType
{
  .custom instance void [mscorlib]System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor() = ( 01 00 00 00 ) 
}
~~~
Description: This structure is used to store the STD Message Colors as a single value type.

__Attributes__:

 1. `System.Serializable()`: The runtime is allowed to serialise this structure , if needed.

### Constructors:

#### Constructor 1:
CSharp Declaration:
~~~C#
public STDColors(STDFrontColor FC, STDBackColor BK)
~~~
Initialises a new instance of this structure with the specified colors.

__Parameters__:

1. `ROOT.STDFrontColor FC`: The foreground color to set for this instance.
2. `ROOT.STDBackColor BK`: The background color to set for this instance.

### Fields:

#### 1. The FrontColor Field:
CSharp Declaration:
~~~C#
public readonly STDFrontColor FrontColor;
~~~
Gets or sets the foreground color for this instance.

#### 2. The BackColor Field:
CSharp Declaration:
~~~C#
public readonly STDBackColor BackColor;
~~~
Gets or sets the background color for this instance.

 -- End of `ROOT.STDColors` Structure -- 

## The STDContext Structure:
CSharp Declaration:
~~~C#
public struct STDContext : System.IDisposable , IEnumerable<STDLine>
~~~
IL Declaration:
~~~IL
.class public sequential ansi sealed beforefieldinit ROOT.STDContext
       extends [mscorlib]System.ValueType
       implements [mscorlib]System.IDisposable,
                  class [mscorlib]System.Collections.Generic.IEnumerable`1<valuetype ROOT.STDLine>,
                  [mscorlib]System.Collections.IEnumerable
~~~
Description: The STD Context is the intermediate storage type which holds temporarily the parsed
STD data which are ready to be read by the application or the STD data created to parse and
format as of STD.

It's internal implementation uses dictionaries to store and retrieve the STD data, while it has proper
support for adding and retrieving STD parsed data in the proper way. 
This helps the STD format to work efficiently , and give to the user 
the ability to even detect parsing errors in the data that has written.

Remarks: This could be possibly implemented as an collection , but some of the methods
the collections provide make it no suitable to implement it as an collection at all.

__Implementations__:
 1. `System.IDisposable`: This interface allows disposing and clearing the data that the instance contains.
   Additionally , it prepars the instance after used , for garbage collection.
 2. `System.Collections.Generic.IEnumerable<STDLine>`: This interface allows the user to use the foreach 
('For Each' in Visual Basic) keyword. This is useful for a couple of reasons and it's usage is not only
that , because the structure can be used to a number of applications.


### Constructors:

#### Parameterless constructor:
CSharp Declaration:
~~~C#
public STDContext() { }
~~~
Initialises a new instance of STDContext.


### Properties:

#### The ItemsCount Property:
CSharp Declaration:
~~~C#
public System.Int32 ItemsCount { get; }
~~~
Returns the number of entries contained in this structure.

This property could be considered equal of the 
[Count Property](http://learn.microsoft.com/en-us/dotnet/api/system.collections.icollection.count?view=netframework-4.8)
of the ICollection interface.

__Get__: The number of STD entries contained in this instance.

### Methods:

#### 1. The Add method:
CSharp Declaration:
~~~C#
public void Add(STDFrontColor Fr, STDBackColor Bk, System.String Data)
~~~
Adds a new STD Message Entry in the structure , with the specified message data , 
and front/back colors.

__Parameters__:

1. `ROOT.STDFrontColor Fr`: The Foreground Color of the message to specify.
2. `ROOT.STDBackColor Bk`: The Background Color of the message to specify.
3. `System.String Data`: The STD Message body itself.

__Returns__:
   This function is declared as `void` , so it does return nothing.
   What it does is to add a new STD Message Entry to the structure.

#### 2. The AddComment method:
CSharp Declaration:
~~~C#
public void AddComment(System.String Comment)
~~~
Adds a new and valid STD comment with the specified comment message.

__Parameters__: 1. `System.String Comment`: The comment message to apply too.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What it does is to add the specified STD comment.

#### 3. The AddVersionBlock method:
CSharp Declaration:
~~~C#
public void AddVersionBlock()
~~~
Adds a new and valid STD Version Block.
The Version number written when the structure data
are serialised is the latest one version available.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What it does is to add a new STD Version Block.

#### 4. The Clear method:
CSharp Declaration:
~~~C#
public void Clear()
~~~
The Clear() method clears all the STD entries until just before this function was called.
Using this method can make this instance reusuable , which it means that it is not needed
to create a new instance of this structure so as to use it somewhere two times.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What it does is to remove all the STD Entries added before.


#### 5. The Dispose method:
CSharp Declaration:
~~~C#
public void Dispose()
~~~
The Dispose() method disposes all of the resources that the current instance uses.

After calling this method , in all instance members a 
[`System.ObjectDisposedException`](http://learn.microsoft.com/en-us/dotnet/api/system.objectdisposedexception?view=netframework-4.8) will be thrown.

If you want to reuse the instance for completely other 
reason , then use the `Clear()` method instead.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What it does is to dispose this instance of the structure.

#### 6. The Get method:
CSharp Declaration:
~~~C#
public STDLine Get(System.Int32 Index)
~~~
Gets the specified STD Entry at the specified index.

For usual development , however , it is recommended to use the GetEnumerator() function or 
the `foreach` keyword for iterating through the context.

__Returns__: A new `ROOT.STDLine` structure containing the entry data located at the specified index location.

#### 6. The GetEnumerator method:
CSharp Declaration:
~~~C#
public System.Collections.Generic.IEnumerator<ROOT.STDLine> GetEnumerator()
~~~

This method is an implemented member of the [IEnumerable](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=netframework-4.8) 
interface. Please the the documentation of the 
[GetEnumerator Method](http://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1.getenumerator?view=netframework-4.8) 
for more information about this method.

### Operators:

#### Explicit Operator 1:
CSharp Declaration:
~~~C#
public static explicit operator STDLine[](STDContext context) 
~~~
Returns the data that an STD Context structure holds as a new 
array of STD entries , which in fact the STD Context is comprised
of.

__Parameter__: `ROOT.STDContext context`: An initialised STD context to get the data from.

__Returns__: A new `ROOT.STDLine[]` array that contains the context data.

 -- End of the `ROOT.STDContext` Structure -- 

## The STDLine Structure:
CSharp Declaration:
~~~C#
[System.Serializable]
public struct STDLine
~~~
IL Declaration:
~~~IL
.class public sequential ansi serializable sealed beforefieldinit ROOT.STDLine
       extends [mscorlib]System.ValueType
~~~
Description: This structure defines itself the most fundamental 
component in STD , the STD Line Entry.
The STD Line Entry defines the data that has each STD Entry , which type of data is
and which colors a STD Line Entry has.

The `ROOT.STDContext` structure described above can be considered a collection
of such structures.

This structure is kept so simple that has only three fields for a couple of reasons:

 - Less memory occupation of each structure.
 - Fast construction or deconstruction.
 - Intended to be simple , so simple that it's layout is even familiar to C/C++ developers.
 
### Attributes:

1. `System.Serializable()`: The runtime is allowed to serialize this structure , if required.

### Fields:

#### 1. The Colors Field:
CSharp Declaration:
~~~C#
public STDColors Colors;
~~~
The Colors of the STD Entry to get or set.

See the `ROOT.STDColors` structure for more information on how 
to set or get the colors.

#### 2. The Data Field:
CSharp Declaration:
~~~C#
public System.String Data;
~~~
Gets or sets the literal STD Entry data.
In a new STD Message Entry , it is the message itself.
In a new STD Comment , it is the comment description.
In a new STD Version Block , this contains the literal version block.

#### 3. The Type Field:
CSharp Declaration:
~~~C#
public STDType Type;
~~~
Gets or Sets the STD Entry that this structure holds.
This value can be anything of the values that the `ROOT.STDType`
enumeration has , so please consult it so as to determine
it's value.

 -- End of the `ROOT.STDLine` Structure --

## The STDType Enumeration:
CSharp Declaration:
~~~C#
public enum STDType : System.Int32
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed ROOT.STDType
       extends [mscorlib]System.Enum
~~~
Description: Defines the possible STD Entry types.

### Fields:

#### 1. The STDString Field:
This field indicates that the STD Entry type is
a new STD Message.

#### 2. The VersionBlock Field:
This field indicates that the STD Entry type is
a STD Version Block.

#### 3. The Comment Field:
This field indicates that the STD Entry type is
a valid STD Comment.

#### 4. The Invalid Field:
This field indicates that an entry was found in the file
which was nothing of the three fields above.
This one field exists only and only to assist the
programmer in the try to determine which STD Line 
cannot be correctly parsed.

 -- End of the `ROOT.STDType` Enumeration --

## The SimpleProgressBar Class:
CSharp Declaration:
~~~C#
public sealed class SimpleProgressBar
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed beforefieldinit ROOT.SimpleProgressBar
       extends [mscorlib]System.Object
~~~
Description: This class intends to present a simple progress bar for console applications, which do not
have such functionality and support for something similar.

This class also aims to be simple and has only a defined set of functions and properties someone will need.

Finally , it is allowed to change any setting during the display of the bar , or even to terminate it prematurely , if required.

To make such an instance to work correctly , you must initiate a new thread for it and pass to the `ThreadStart` constructor
the function `Invoke()`.

### Constructors:

#### Parameterless Constructor:
CSharp Declaration:
~~~C#
public SimpleProgressBar() { }
~~~
Initailises a new instance of the class. All required settings must be set by the provided properties.

#### Constructor 1:
CSharp Declaration:
~~~C#
public SimpleProgressBar(System.Int32 Start, System.Int32 Step, System.Int32 End)
~~~
Initailises a new instance of the class with the specified start point , the step defined for each progress change and
when the Progress Bar will end it's execution (End value).

__Parameters__:
1. `System.Int32 Start`: The start point value.
2. `System.Int32 Step`: The Step value for each progress change.
3. `System.Int32 End`: The value when the Progress Bar will end it's execution.

#### Constructor 2:
CSharp Declaration:
~~~C#
public SimpleProgressBar(System.String progressMessage, System.Int32 Start, System.Int32 Step, System.Int32 End)
~~~
Initailises a new instance of the class with the specified progress message , start point , 
the step defined for each progress change and
when the Progress Bar will end it's execution (End value).

__Parameters__:
1. `System.Int32 progressMessage`: The initial , and specified Progress Message.
2. `System.Int32 Start`: The start point value.
3. `System.Int32 Step`: The Step value for each progress change.
4. `System.Int32 End`: The value when the Progress Bar will end it's execution.

#### Constructor 3:
CSharp Declaration:
~~~C#
public SimpleProgressBar(System.Int32 Step, System.Int32 End) 
~~~
Initailises a new instance of the class with the step defined for each progress change and
when the Progress Bar will end it's execution (End value).

__Parameters__:
1. `System.Int32 Step`: The Step value for each progress change.
2. `System.Int32 End`: The value when the Progress Bar will end it's execution.

#### Constructor 4:
CSharp Declaration:
~~~C#
public SimpleProgressBar(System.String progressMessage, System.Int32 Step, System.Int32 End)
~~~
Initailises a new instance of the class with the specified progress message ,
the step defined for each progress change and
when the Progress Bar will end it's execution (End value).

__Parameters__:
1. `System.String progressMessage`: The initial , and specified Progress Message.
2. `System.Int32 Step`: The Step value for each progress change.
3. `System.Int32 End`: The value when the Progress Bar will end it's execution.

### Properties:

#### 1. The ProgressChar Property:
CSharp Declaration:
~~~C#
public System.Char ProgressChar
~~~
Gets or sets the progress character to show in the literal progress bar.

By default , that character is a dot `.` .

__Get__: The current set character for the progress bar.

__Set__: A different character to set for the progress bar. 

__NOTE!!__ If you set an character in the bar that is illegal (special character , slashes , etc.) will throw an
`System.ArgumentException`.

#### 2. The ProgressStep Property:
CSharp Declaration:
~~~C#
public System.Int32 ProgressStep
~~~
Gets or sets the current progress step which is used when the `UpdateProgress()` method is called.

__Get__: The current progress step.

__Set__: Define a new progress step to be used when the `UpdateProgress()` method is called.


#### 3. The ProgressEndValue Property:
CSharp Declaration:
~~~C#
public System.Int32 ProgressEndValue
~~~
Gets or sets the value when the progress bar will finish execution.

__Get__: The current value.

__Set__: The new value that the progress bar will finish execution.

#### 3. The ProgressStartValue Property:
CSharp Declaration:
~~~C#
public System.Int32 ProgressStartValue
~~~
Gets or sets the value when the progress bar will start execution.

__Get__: The current value.

__Set__: The new value that the progress bar will start execution.

__NOTE!!__ If the value set is greater than the value of the `ProgressEndValue` property , 
then it will throw a `System.ArgumentException` .


#### 4. The Ended Property:
CSharp Declaration:
~~~C#
public System.Boolean Ended
~~~
Gets a value whether the instance has finished execution.

If you set this value to `true` , then the instance will stop execution immediately
and you can then dispose the thread.

__Get__: `false` if the progress bar is still running actively; `true` if the execution has stopped.

__Set__: If set to `true` , it will stop execution immediately.

### Methods:

#### 1. The Invoke method:
CSharp Declaration:
~~~C#
public void Invoke()
~~~
Initiates actively the console progress bar. Should be passed to either a new thread or anything else
that has access and control to a thread.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What this function does is to actively initiate the progress bar.

#### 2. The UpdateMessageString method:
CSharp Declaration:
~~~C#
public void UpdateMessageString(System.String Message)
~~~
Updates the message that describes the current execution action in your application.

__Parameters__:
   1. `System.String Message`: The message data to replace with the bar's current one.
   
__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What it does is to update the bar's message.


#### 3. The UpdateProgress method:
CSharp Declaration:
~~~C#
public void UpdateProgress()
~~~
Increments the current bar progress by the value defined by the `ProgressStep` property.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What is does is to increment the current bar progress.

 -- End of `ROOT.SimpleProgressBar` Class --

## The SystemLinks Enumeration:
CSharp Declaration:
~~~C#
public enum SystemLinks
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed ROOT.SystemLinks
       extends [mscorlib]System.Enum
~~~
Description: This Enumeration provides some default values so as to 
open some UWP system apps or UWP apps that ship with every 
Windows 10 / 11 OS.

This eunmeration is only used by the OpenSystemApp overloads defined
in the _MAIN_ class.

### Fields:

#### 1. The Settings Field:
Opens the Windows Settings application.

#### 2. The Store Field:
Opens the Microsoft Store application.

#### 3. The ActionBar Field:
Opens the Windows Shell Action Bar. It has Windows Notifications and a small calendar.

#### 4. The FastSettings Field:
Opens the Quick Settings menu. It contains settings for BlueTooth , Wi-FI , Sound and Mobile Hotspot settings.

#### 5. The ScreenSnippingTool Field:
Opens the Print-Screen Snipping tool. It assists in printing the computer's screen or selecting only a portion of it.

#### 6. The PhoneLink Field:
Opens the Phone Link tool for externally connected Android devices.

#### 7. The GetStarted Field:
Opens the Get Started App.

#### 8. The MusicApp Field:
Opens the default Music App that you have associated to Windows.

#### 9. The WindowsSecurity Field:
Opens the Windows Defended App.

#### 10. The MailApp Field:
Opens the Mail App.

#### 11. The Calendar Field:
Opens the Calendar App. 

#### 12. The PeopleApp Field:
Opens the People App. 

#### 13. The Camera Field:
Opens the Camera App. 

#### 14. The MapsApp Field:
Opens the Maps App. 

#### 15. The Calculator Field:
Opens the Calculator App. 

#### 16. The ClockApp Field:
Opens the Clock App. 

 -- End of the `ROOT.SystemLinks` Enumeration --

## The TimeCalculator Class:
CSharp Declaration:
~~~C#
public sealed class TimeCaculator : System.IDisposable
~~~
IL Declaration:
~~~IL
.class public auto ansi sealed beforefieldinit ROOT.TimeCaculator
       extends [mscorlib]System.Object
       implements [mscorlib]System.IDisposable
~~~
Description: The TimeCalculator class can be used to 
calculate the time needed to execute code excerpts.

It's mecahnism does not use any CPU resources , but only when it is
initiated or closed.

### Methods:

#### 1. The Init method:
CSharp Declaration:
~~~C#
public void Init()
~~~
Initialises the Time Calculator. Remember that the clock will calculate the time after this method was called.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What this function does is to initiate the time calculator.

#### 2. The CalculateTime method:
CSharp Declaration:
~~~C#
public System.Int32 CaculateTime()
~~~
Closes the calculation session and calculates the time needed to reach until before this function was called.

__Returns__: A value which is the needed time to reach this function.

#### 2. The Dispose method:
CSharp Declaration:
~~~C#
public void Dispose()
~~~
Resets the current saved values so as to begin a new calculation session.

__Returns__: 
   This function is declared as `void` , so it does return nothing.
   What it does is to reset the calculated values so as to begin a new session.

-- End of `ROOT.TimeCalculator` Class -- 

### End of the `ROOT` Documentation for now. Last Update Time: 21/8/2023 , 18:44 EST (UTC+02:00).
