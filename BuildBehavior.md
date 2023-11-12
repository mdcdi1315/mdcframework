# The MDCFR Build Behavior System

## Version 1.5.5.0 Changes

After the latest update (version 1.5.5.0 when written) , the Project needed to reference the `System.Resources.Extensions` assembly so as the `dotnet build`
command would sucessfully build the Project.

In the beginning , even when referencing it , it was throwing conflict errors.

Now , however , it can be built by `dotnet build` again because some refactoring was happened which allowed it to 
use the required assembly only when the `csc` builds the project. 

So , proper code had to be added so as work correctly.

That meant that two different trees had to be made so as to support both Visual Studio and the `dotnet build` command.

You will notice that none change was happened to the csproj itself , but were only added two new targets , called 
`DotnetCore0` and `DotnetCore1` , which the `DotnetCore0` builds the required dependencies for `dotnet build` command , 
while `DotnetCore1` clears up the used and not required System.Resources.Extensions assembly and all it's dependencies.

You can use these two new targets if you need to make your own project and want this one to be referenced as an Project 
reference. 

Use these two targets to control this Project's build flow and (or) modify the target behavior when using it with 
other projects and building with the `dotnet build` command.

## Version 1.5.5.6 Changes

A notable regression found out when someone attempted to use the MDCFR Library API , mostly about the `ROOT.MAIN.NewMessageBoxToUser()`
member. 

The class implementing this stated member in the `MAIN` class is using some resources that are icons (images).

Any non-string resources for .NET >= 6 must be built out using as a reference the 
`System.Resources.Extensions` assembly.

However , what was finally found out it was that the improper ResGen tool was used.

Although that through Visual Studio the MSBuild was mocked out producing the desired result , 
and it was okay , the `dotnet build` command has the problem that the required cached data to correctly
execute the code from MDCFR directly were not available , the MSBuild was not mocked out as a result
to still request the `System.Resources.Extensions` package. Due to that , the embedded code of the MDCFR
that contains the package remained unused , and could cause again confilcting errors in between these
two assemblies.

The result was to throw an invalid exception on resource retrieval even when the authentic package
was existing along however failed again due to those confilcting errors. 
Be noted , this situation was only happened for .NET Framework 4.8. build target.

As it seems out , the responsible for this was the improper version of the ResGen tool used.
These projects are for .NET Framework 4.8. , but this is not satisfied , because MSBuild thinks 
that these projects are .NET 6/7 , until it encounters the TargetFramework property , which is relatively 
late because already it has set a path to the ResGen tool.

To tackle this problem , I fully refactored the `DotnetCore0` and `DotnetCore1` targets so as to 
use and build a custom ResGen tool (Actually it is nothing more than
a decompiled source code of the original ResGen)
and use this instead to build the resources , instead of the provided one.
The tool is only being built during `dotnet build` command execution for MDCFR , or if you explicitly invoke
it's build either with `dotnet build` or within Visual Studio.

However , this error is not resolved but you will never again encounter such an exception , at least for
the internal API. If are still peristent errors that the `System.Resources.Extensions` package carries on
due to MSBuild detection on your project that you have referenced it with MDCFR , 
then go ahead report it as issue because it will removed if something similar is 
detected in the future.