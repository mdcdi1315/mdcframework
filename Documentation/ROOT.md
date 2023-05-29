# The ROOT Namespace
It is the central namespace of this Framework and includes functions which are implemented by me.

 It includes: Generic methods , Archiving and cryptography operations.
## The MAIN Class
```C#
public static class ROOT.MAIN
```
The _MAIN_ Class contains static methods that assist a programmer develop quickly an application.

### Classes:
The Classes nested inside this one are:
  
 1. The IntuitiveConsoleText class , which shows a message to a console formatted and colored properly.
 2. _(.NET Framework and Windows Desktop builds only)_ The DialogsReturner class , which is a storage class used when one of the 
 file/directory dialogs is invoked.

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
       if (FSI is System.IO.FileInfo) {Console.WriteLine("File: " + FSI.FullName);}
       //The below statement tests if the object took from the array is a directory.
       if (FSI is System.IO.DirectoryInfo) {Console.WriteLine("Directory: " + FSI.FullName);}
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
 public static System.String GetAStringFromTheUser(System.String Prompt, System.String Title, System.String DefaultResponse)
 ~~~
 This function is an exact implementation of the [`Microsoft.VisualBasic.Interaction.InputBox`](https://learn.microsoft.com/dotnet/api/microsoft.visualbasic.interaction.inputbox?view=netframework-4.7.2),
 which is imported here if you want to use it , but it is not needed to reference the `Microsoft.VisualBasic` DLL.
 
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
public static System.String 
~~~
