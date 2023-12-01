# Project MDCFR: Application Development Framework
The directory structure was radically changed so as to not render large previews
when using the GitHub Preview.

This allows a gathered and simple view of the MDCFR directory structure , 
while the directories nicely contain the projects and the tools too.

This directory contains the MDCFR Project along with the HTTPLIB deprecation
assembly that only targets .NET 7.0  (Since .NET 7.0 does have deprecated the used API) .

You might have noticed out that here a PublicKey.snk file is existing.

This file is the Project's Strong Name key and it is the unique identifier of MDCFR.

Notes for the MDCFR-SYSRUNUNS Project:

The MDCFR-SYSRUNUNS is a decompiled flavor of the `System.Runtime.CompilerServices.Unsafe` DLL
found on NuGet Repository. 

However , it is needed to build a custom DLL of it that would reference MDCFR
without errors, plus resulting in a faster code execution. 

It is known that the DLL contains IL code not available to C# , however in the future
maybe the DLL is also embedded in MDCFR so as to finally remove that reference 
and embed it. 

For now , there has been a runtime error while loading the `System.Runtime.CompilerServices.Unsafe`
DLL without or with the .NET Framework assembly key , so the .NET Core key was used , and therefore ,
you must use the `System.Runtime.CompilerServices.Unsafe` shipped with this Library.