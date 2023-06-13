# Upcoming Releases
Everything that will be added to new releases will be here for anyone to see.

## The upcoming version: 1.5.5.0
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
