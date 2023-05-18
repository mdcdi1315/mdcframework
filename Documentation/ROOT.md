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
 2. _(.NET Framework only)_ The FileDialogsReturner class , which is a storage class used when one of the 
 file dialogs is invoked.

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
public static MAIN.FileDialogsReturner CreateLoadDialog(System.String FileFilterOfWin32 , System.String FileExtensionToPresent ,System.String FileDialogWindowTitle)
~~~
___NOTICE___!!! This function is available only for _.NET Framework_ builds!!!
 This function displays to the User to select and load a file.

  __Parameters__: 
  1. `System.String FileFilterOfWin32`: The file filter to use for limiting the search (It is an array of `';'`-seperated strings).
  2. `System.String FileExtensionToPresent`: The default file extension to present when the `FileFilterOfWin32` has more than two entries.
  3. `System.String FileDialogWindowTitle`: The title of the window that it is presented.
  
  __Returns__: a new `ROOT.MAIN.FileDialogsReturner` instance.

  __Example__: make a file filter , invoke the function and get the values.
  ~~~C#
    using static ROOT.MAIN;
    //...
    string File_Filter = "Text Documents|*.txt;Zip Archives|*.zip;Settings Text Document|settings.txt";
    string FileExt = ".txt";
    string title = "Open the File...";
    FileDialogsReturner DialogResult = CreateLoadDialog(File_Filter , FileExt , title);
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
