# The MDCFR Build Behavior System

After the latest update (version 1.5.5.0 when written) , the Project needed to reference the System.Resources.Extensions assembly so as the `dotnet build`
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

