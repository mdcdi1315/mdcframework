# MDC (.NET Application Development Library)
MDC (Called __MDCFR__) is a .NET Application Development Library which aims to include an all-in-one solution for your .NET projects
(It works on any app , even if it is written in C# , Visual Basic or F#).

## Detailed Description
This .NET Library (__MDCFR__ called) contains anything needed to start and develop easily .NET applications without
learning for how to develop something specific (like getting a file hash). Just Code!!
Usage Example:
```C#
	
	using System;
	using APIROOT;
	using static APIROOT.MAINAPI;

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
This Project was developed for a need to develop easily .NET Applications , and started as of my own idea.
The intention at the first place was not to post it on GitHub , but it is important to have some code 
foundation to start your new .NET project. So , this repository contains all of the source code used
in the initial built project.

## Avaliable Platforms
The Project is compiled as a Dynamic Link Library (DLL) and works on any platform (Not sure , but surely is compiled as MSIL).
You can compile it either using the command `dotnet` or using the bundled Visual Studio Solution.
- __NOTE!!!__ The xxhash.dll and zstd.dll can only work in 64-Bit computers and cannot be used in any other computer architecture.


## What it includes?
The project includes operations related to:
- File Operations (Abstracting System.IO and System.IO.FileStream).
- User Interaction (Showing a message to Windows , Getting or saving a file , etc.).
- Windows Registry Editor (Not fully inplemented yet , causes problems).
- Cryptography operations (Digests , AES , DES , etc.).
- Archiving operations (Tar Archives , Zip Archives , GZip Archives and [Zstandard](https://github.com/facebook/zstd).)
NOTE: Zstandard archiving is only avaliable as a wrapper.
- HTTP operations (The [JumpKick.HttpLib](https://github.com/j6mes/httplib) from James Thorne.).

#### New Features that might be added:
- Cabinet Archives (Possibly by using Windows.Win32).
- Other Archiving formats (Not still confirmed).
- Extension methods on Save/Load file dialogs.


#### Contribution
Anyone can contribute to this project.
However , other contributor not approved by me is not allowed to add new features to the Project , but only to fix a bug.

#### Contact
For any considerations , ideas or a question relating to the Project can be subitted to my E-mail. Note though that I do not check my E-mail every day , so it is difficult to get an answer for what you need.
Any questions for the project can be also posted as Issues and marking them as 'Question'.

##### Source code links:
- Http Library: https://github.com/j6mes/httplib .
- Zstandard archive format repo at https://github.com/facebook/zstd .
- xxhash non-cryptographic hash algorithm at https://github.com/Cyan4973/xxhash .
