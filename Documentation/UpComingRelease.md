# Upcoming Releases
Everything that will be added to new releases will be here for anyone to see.

## Major Version 1.5.5.0:
This version will make general changes to almost everything on the library.
- The `ROOT.MAIN.GetACryptographyHashForAFile` function has changed it's behavior and will have 1 more overload:
```C#
  using ROOT;
  using static ROOT.MAIN;
  
  //...
       GetACryptogrphyHashForAFile("ThePathFile" , HashDigestSelection.SHA256);
  //...
```
This excerpt will return the SHA256 hash of the file. It will produce the same result as the `GetACryptogrphyHashForAFile("ThePathFile" , "SHA256")`

__NOTE__: The function behavior has changed radically and as a result , the hashes produced in older versions are different for the same file
in the older versions, so if you use it , you must delete any values that you produced using the older versions.

Additionally , these functions do now produce the same result as `CertUtil.exe` , so it means that you may now use the library for normal 
cryptographic operations whenever needed.

- Snappy , Cabinet Support on Windows and Brotil will be added too.

Snappy and Brotli are archive implementations of [Google](http://github.com/google) 
which are written in C. I found ports of these two implementations and will be included
in the library (Which means that you can use them at any CPU architecture).

However , their actual usage is for networking data transfer, and in normal occasions , for that reason
these formats should be used.

Snappy methods will be exposed at `ExternalArchivingMethods.Snappys.Snappy` (_Note_: These methods do accept a Safe
`ReadOnlySpan<Byte>` as input data.) and Brotli offers two options: 
either using the `ExternalArchivingMethods.Brotlis.BrotliStream` (The syntax bears resemblance to 
the [StreamWriter](http://learn.microsoft.com/en-us/dotnet/api/system.io.streamwriter?view=netframework-4.8)
class constructor , when it used inside a `using` statement.) , or accessing the static compression and
decompression methods , which are located to `ExternalArchivingMethods.Brotlis.Brotli`.

The Snappy port is still maintained , while the Brotli port found has stopped maintenance about 5 years ago (Version 0.6.0 of the official implementation) .

__NOTE__: The Snappy port is a bit modified so as to support the .NET Framework 4.8 .

Cabinet Support namespace will be accessed at `ExternalArchivingMethods.Cabinets` and a special class is created under
`ROOT.Archives.Cabinets` , which includes some functions there which wrap up the Cabinet functionality.

- A new namespace will be added: The `ROOT.IntuitiveInteraction`.

This namespace will contain two classes that extend the Microsoft's ones on the point of User interaction.
The first one is `IntuitiveMessageBox` , which is like the ordinary Windows message box , 
but with changes.

The second one is `GetAStringFromTheUser` which is a very modified version of the
[`Microsoft.VisualBasic.Interaction.InputBox`](
http://learn.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.interaction.inputbox?view=netframework-4.8
) function , and extends the usability of that method.

- The Registry Editor has taken it's final form

The Registry Editor was refactored and most of the problems were solved.
However , it needs to be tested so as to be used in production.

## Minor Version 1.5.5.2 & 1.5.5.3:

 In this Version , more features are now added! As going ahead to 1.5.6.0 , 
 some features were added , while some others were removed (or are in process of it).
 
 - Added a native ZIP C# Library that supports ZIP , GZIP and BZIP2 formats
   
   (For now it will remain experimental , so expect changes or even an removal 
   due to incorrect inefficiencies - code errors.)

 - Console Support for Windows is translated directly to the KERNEL32 library.
   
   (This means that it does not use the System.Console class.)
   
 - Added two more NuGet packages : 
   
   [`System.Text.Encoding.CodePages`](http://learn.microsoft.com/en-us/dotnet/api/system.text.codepagesencodingprovider?view=net-7.0) :
    
   This is a package that contains more encoding pages for your applications that do not exist in .NET 4.8.
   
    [`System.Collections.Immutable`](http://learn.microsoft.com/en-us/dotnet/api/system.collections.immutable?view=net-7.0) : 
    
    This package allows the programmer to create immutable collections , like arrays or lists.

  - The Visual Basic Runtime Library will be REMOVED.
   
    -- Any functions that used the library are also deprecated , except for some that can be migrated
    by using some functions of mscorlib.
 
## Minor Version 1.5.5.4:
In this minor release , some more NuGet packages are now embedded , allowing more operations to be done in
.NET Framework 4.8.

 - Final decision about Visual Basic Runtime Library:
     
    The dependencies have all been sucessfully removed from MDCFR that involved the library.
    
    The library dependency will be removed in the last minor release of 1.5.5. .
  
  - Added the NAudio package 
     
    The NAudio package is maybe one of the most famous packages for playing audio through .NET .
    
    This package was included in the Project because someone would need to play an audio resource
    for his application.
 
  - All documentation will be migrated to Markdown using xmldocmd . This will not be used yet in this minor release
   
    because I am testing if that would work sucessfully.
  
  - The Project might be also compiled in .NET >= 6 . 
  
    This is also in test phase yet , however for those who need to migrate apps 
    from .NET Framework 4.8 is extremely useful to have the Project bindings to .NET >= 6.
  
    Of course , that library will not contain any of the packages embedded , due to the fact that
    .NET 6 has these packages already pre-installed.

  - Minor changes to source files , added missing documentation for System.Memory and System.Buffers packages , 
    and some bug fixes.
  
  - From 1.5.5.4. and ahead , a debug release of the library will also exist , along with a full PDB file.
   
   \-> This means that you have the ability to debug even the MDCFR itself if it is needed , 
   or to debug your application that uses the MDCFR code.

   \-> This flavor of the MDCFR library is called "Debug Flavor" and the PDB along with it it
   is used for debugging reasons only.

   \-> New Manifest properties defined globally in the assembly can help you detect if the 
   assembly you are using is in debug or release flavor.

## Minor Version 1.5.5.5:
In this minor release , even more NuGet packages will be embedded , allowing even more possiblities.

This , however , does not stop here. Some new of this framework-specific features are finalised and added too.

  - The Visual Basic Runtime Library , Plus the `System.IO.Compression.FileSystem` DLL's will be removed in 
    the 1.5.5.6 Minor Version.
  - Inside the `ROOT.Archives` namespace , there have been even more changes:
        
      1. Added a new `CurrentProgress` class.
                
            This class is the base implementation for getting the archiving progress in the classes defined
            in this namespace. 
            
            \->  There is full support for the ZIP and Cabinet Archives , and partial support for the other formats.
             Full support for it is expected to be done in the next minor release.
          
            \-> To access the actual progress instance , you must use the field `Progress` of the class 
            `ROOT.Archives.GlobalArchiveProgress`. All the archiving classes defined herein depend on this field.
      2. Added more support for Cabinet Archives , plus the compression level is now adjustable.
            
            \-> The Cabinet Compression Level enumeration can be found as `CabinetCompressionLevel`.
      3. The Experimental classes are now moving out of Experimental Mode.
            
            \-> In the next minor release , these classes will be finalised and the `Experimental_` prefix 
            will be removed.
   - Added the `AsInterface` Attribute and a tool that checks the attribute correctness
            
      \-> The primary usage of the attribute is to describe that a class uses the 
      exactly same members of an existing interface without the need of referencing it.
        
       \-> To verify that your implementation makes correct usage of the attribute , you can
       use the `ROOT.AsInterfaceChecker` class or the Checker Tool shipped with this project.
  - Fixed some issues about the Console Interop used , and some of the file functions are now
  also invoked through interop.
  - Added the ability to create an Windows Internet Shortcut
      
    \-> Creating Internet Shortcuts in Windows was the most difficult part for many developers ,
    especially in .NET , which does not even have such support of it. 
           
    The problem was that only Windows Explorer can actually create such shortcuts and use 
    the Windows Shell to run them.
       
    Now , just plainly calling `ROOT.MAIN.CreateInternetShortcut` with the required arguments
    can create a new Windows Internet Shortcut directly.
          
    Be noted , the shortcut creator is fully written in .NET and DOES NOT use any P/Invoke 
    signatures to do the actual work.
      
  - Added the [`Microsoft.IO.RecyclableMemoryStream`](http://github.com/microsoft/Microsoft.IO.RecyclableMemoryStream) package.
      
     \-> For API accessibility reasons , you can also find the package 
     as `System.IO.RecyclableMemoryStream` instead.

  - Added ALL the [`NAudio`](http://github.com/naudio/NAudio) packages.
         
      \-> `NAudio` is a series of packages that can play audio files to your computer
      using mainly P/Invoke signatures. More documentation on it is provided in the 
      GitHub repository , which is [here](http://github.com/naudio/NAudio).
        
      \-> All the `NAudio` packages are included as they are , in the `NAudio` namespace.
  - Added 2 NuGet packages , these are `System.Threading.Channels` and
   `System.Numerics.Tensors`.
   
More features are coming out in the next minor release of MDCFR , with even more interesting stuff
to explore and find out.