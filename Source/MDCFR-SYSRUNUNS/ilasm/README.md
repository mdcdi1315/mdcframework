### Notice

This directory contains the .NET Framework ILAsm tool found from the Windows .NET Framework directory.

This tool runs so as to build the custom `System.Runtime.CompilerServices.Unsafe` DLL , and therefore , 
not any other tool must be used so as to generate the required result , because if someone uses the 
.NET `ilasm` flavor , the `System.Runtime.CompilerServices.Unsafe` DLL will not be generated.