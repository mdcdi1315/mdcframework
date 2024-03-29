# MDC (.NET Application Development Library)
MDC (Called __MDCFR__) is a .NET Framework Application Development Library which aims to include an 
all-in-one solution for your .NET Framework projects
(It works on any app , even if it is written in C# , Visual Basic or F#).

## Detailed Description
This .NET Framework Library contains anything needed to start 
and develop easily .NET Framework applications without
learning , finding or implementing something very specific in .NET Framework . 
It is a single library which allows someone to easily
include it in the project he works on. Additionally , it provides methods that everyone can use. Just Code ! 

Usage Example:
```C#
	using System;
	using ROOT; // This is the default namespace for the basic functions.
	using static ROOT.MAIN; // This accesses the static class MAIN , which is the class that contain most of the functions.

	namespace ANamespace
	{
	  internal class Program
	  {
	      public void Main(System.String[] args)
	      {
		IntuitiveConsoleText.InfoText("Welcome!!!"); // This is like: "INFO: Welcome!!!" with a gray foreground and a black background.
		WriteConsoleText("Start your next .NET Application development using MDCFR!!!"); // This just writes to console any kind of message.
		HaltApplicationThread(320); // This stops the application's thread (Halts the application) for the time given. Counted in milliseconds.
	      }
	   }
	}
```

## The Project History
This Project was developed for a need to develop easily .NET Framework Applications , and started as of my own idea.
The intention at the first place was not to post it on GitHub , but it is important to have some code 
foundation to start your new .NET Framework project. 

Additionally , .NET Framework support is starting now to wipe out of the map because .NET 6 , 7 and 8
have changed and added a lot of new sets of API's and libraries too. 
This project aims , as most as possible , to bridge this created gap between .NET Framework 4.8 and
.NET >= 6 in a single library.

However , for those that in the later time decide to move their project to .NET 7 , no problem at all!

This feature is experimental for now , but most of the current functionality is provided to the Library
to work for .NET 7.

Additionally ,type forwards that were related to the embedded packages , which are not needed to be 
embedded for .NET 7 , were added so as to notify the programmer to move to the real and existing assembly.

This repository contains all of the source code developed
or included until now. The Library will be also updated with new features and bug fixes as the months come.

## Avaliable Architectures
 The Project is compiled as a Dynamic Link Library (DLL) and works on any architecture (Not sure , but surely is compiled as MSIL (Platform agnostic)).
 
 You can compile it with the `dotnet build` command or using the bundled Visual Studio solution.

 __NOTE!!!__ The xxhash.dll and zstd.dll can only work in 64-Bit Windows Unicode machines and cannot be used in any other machine architectures.

## What it includes?
The project includes operations related to:
 - File Operations (Abstracting System.IO and System.IO.FileStream).
 - User Interaction (Showing a message to Windows , Getting or saving a file , etc.).
 - Registry editor (Now it is more accurate and stable , now is to be tested.) {Microsoft Windows Only!!!} .
 - Cryptography operations (Digests , AES(Implemented on my own using System.Security.Cryptography) , some from [`Mono.Security.Cryptography`](https://github.com/mono/mono/tree/main/mcs/class/Mono.Security/Mono.Security.Cryptography) etc.).
 - Archiving operations (Tar Archives , [Zip/GZip Archives](http://github.com/icsharpcode/SharpZipLib)  , [Zstandard](https://github.com/facebook/zstd) , 
	[Snappy](http://github.com/brantburnett/Snappier) and [Brotli](http://github.com/master131/BrotliSharpLib).)

   __NOTE__: Zstandard archiving is only avaliable as a wrapper with limited functionality.
 - HTTP operations (The [JumpKick.HttpLib](https://github.com/j6mes/httplib) from James Thorne.).
 - JSON (Stands for _JavaScript Object Notation_) Serialisation operations using the default `System.Text.Json` package.
 - Playing audio to Windows using various methods from the [NAudio](http://github.com/naudio/NAudio) package.

  Additionally , it includes sixteen NuGet packages:
  - `System.Buffers`
  - `System.Memory`
  - `System.Resources.Extensions`
  - `System.Threading.Tasks.Extensions`
  - `System.Text.Encoding.CodePages`
  - `System.Collections.Immutable`
  - `System.Threading.Tasks.Dataflow`
  - `System.Threading.Channels`
  - `System.Numerics.Vectors`
  - `System.Numerics.Tensors`
  - `System.Text.Encodings.Web`
  - `System.Text.Json`
  - `System.Reflection.Metadata`
  - `System.Reflection.MetadataLoadContext`
  - `Microsoft.IO.Redist` (Only for .NET Framework 4.8. targets will be available , and built under Windows)
  - `Microsoft.IO.RecyclableMemoryStream` (Compiled for both .NET 7.0. and .NET Framework 4.8.)
  - `Microsoft.Bcl.AsyncInterfaces`

  These packages are embedded in the Project and can be used like the normal NuGet packages.
  The documentation for these is kept by Microsoft , as well as how to use them.

  Note: For .NET 7 , however , the respective type forwards are embedded so as to notify the compiler to use
  the normal libraries again.

#### .NET 7 Support (Works as compatiblity layer)
Support for .NET 7 was added sucessfully in 1.5.5.7 !  
However , until the next major release , 1.5.6.0 , this feature will remain experimental 
mostly for testing reasons , however it is confirmed that support is okay to compile
as of .NET 7.

However , this was needed a lot of code refactoration , endless devoted hours for it
and multiple testing times.

#### Other Notes:
The Project Directory Structure has been completely changed so as to make it more easy and accessible
for the new developers to start using it.
Read more about it in the Readme file located inside the Source folder.

#### Documentation
The Documentation explaining how to use this framework is [here](http://github.com/mdcdi1315/mdcframework/blob/dev/Documentation/Main.md).

#### Contribution
Anyone can contribute to this Project.
However , other contributor not approved by me is not allowed to add new features to the Project , but only to fix a bug.

#### Contact
For any considerations , ideas or a question relating to the Project can be submitted to the Project's tab "Discussions".
Any questions relating bugs can be posted as a new Issue and marking them as 'Question'.

#### Adding to this Project your own , and unique project
I really want to add your own project to this Project , beacuse it may help more people like you!

Otherwise , this is and the goal of the MDCFR: To add a collection of projects into a single library.

However , of course , adding a Project has restrictions too. For a set of requirements , see [here](http://github.com/mdcdi1315/mdcframework/blob/dev/Documentation/AddingAProject_Requirements.md) 
for more information.

#### Source code used in the Project (Licenses for all the used code , among with the Project's one , are located to LICENSE file):
- Http Library by j6mes at http://github.com/j6mes/httplib 
  
	NOTE (1): The underlying API used is deprecated for .NET 7 and is shipped as a seperate library.

	NOTE (2): This project will be removed from MDCFR after 1.5.7.0 because it is written crudely and not updated since 2012.
- Zstandard archive format repository (Implemented on my own as a wrapper for the library) at http://github.com/facebook/zstd .
- xxhash non-cryptographic hash algorithm (Implemented on my own as a wrapper for the library) at http://github.com/Cyan4973/xxhash .
- CRC Implementation by nullfx at http://github.com/nullfx/NullFX.CRC .
- Blake2s Checksum by sparkdustjoe at http://github.com/SparkDustJoe/Blake2 .
- Cabinet Support by .NET Foundation (Taken from the WiX Toolset) at http://github.com/wixtoolset/wix .
- Snappy Compression and Decompression .NET library by brantburnett at http://github.com/brantburnett/Snappier .
- A Full and exact implementation of Brotli archiving on C# (version is 0.6.0.) by master131 at http://github.com/master131/BrotliSharpLib .
- The Packages used (NuGet) are creations of .NET Foundation located at http://github.com/dotnet .
- Zip .NET Managed Library (SharpZipLib) by icsharpcode at http://github.com/icsharpcode/SharpZipLib .
- Support for .NET Audio support by Mark Heath's NAudio package: http://github.com/naudio/NAudio .