# The ExternalArchivingMethods Namespace:

~~~C#
namespace ExternalArchivingMethods
~~~

This namespace contains external projects that are evolved with file or data compression-decompression.

This file will only describe which methods are included until now , and a summary of what they are.

## The Brotlis Namespace:
Description: Brotli is a compression algorithm by Google intended for reliable , lossless and fast-as-possible
applications which involve Internet Package transactions.

It contains two classes: Brotli and BrotliStream.

The Brotli class contains static methods on compressing and decompressing data , 
while BrotliStream was implemented as of the `System.IO.Stream` logic.

## The Snappys Namespace:
Description: Snappy is a compression algorithm by Google intended for reliable and on-the-fly
psuedo-compression algorithm. It is also used for retrieving Internet Packages , with the 
difference that Snappy usage is to compress data quickly , while in Brotli the speed does not 
have so much meaning (as I think that from it's documentation is implied.).

This namespace also contains in the Snappy class static and thread-safe methods (Span , ReadOnlySpan).
and the SnappyStream class is implemented like an `System.IO.Stream` .

## The Cabinets Namespace:
Description: This is an .NET Wrapper around the Windows Cabinet Archive Format (CAB).
It is provided by WiX Toolset because Windows Installer files are actually 
such kind of files.
You can find there classes and methods to work with , but I have implemented an
easy flavor of it in the `ROOT.Archives` namespace.

## The SharpZipLib Namespace:
Description: This is a managed flavor of ZLib which contains methods for ZIP , BZip2 and GZip 
algorithms.

For these compression types I have also implemented easy and static methods.
For now , though , these are experimental until the next major release.
You can use them using the following class names:

- `ROOT.Archives.Experimental_ZipArchives`
- `ROOT.Archives.Experimental_BZip2Archives`
- `ROOT.Archives.Experimental_GZipArchives`

## The Tars Namespace:
This namespace contains a simple implementation for US Tape Archives.
However , the default classes it provides do only save files as they were are.
However , it is extensible and you can add a compression algorithm to compress the data
as individual files represented in the archive too.

## The ZstandardArchives Class:
This is a simple Zstd Algorithm wrapper implemented by me.

However , this only works in x64 Windows Unicode machines.


### End of the `ExternalArchivingMethods` Documentation for now. Last Update Time: 21/8/2023 , 19:37 EST (UTC+02:00).



