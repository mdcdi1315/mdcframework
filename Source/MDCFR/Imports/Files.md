# Reference Assemblies Used During Build
These are the required Reference Assemblies so as the Project can be sucessfully built.

The Reference Assemblies are now split-up for .NET Framework 4.8 and .NET 7.0
project targets , repectively.

For .NET Framework 4.8. , these are included in the `NETFR48` folder.

For .NET 7.0. , these are included in the `NET70` folder.

 There are also included the `xxhash.dll` and `zstd.dll` that are extensions of the ___MDCFR___
library and do only work when these files are located to the app's current working directory.
If you do not use the relevant API's , there is no need to include them in your project
and can be omitted.

NOTE (1): The `xxhash.dll` and `zstd.dll` work only for Windows 64-Bit machines.
NOTE (2): For the Zstd wrapper to work , it requires a DLL version equal or greater than 1.5.2.0.