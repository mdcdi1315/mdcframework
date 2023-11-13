# About <Code style="color:magenta">Build.cmd</Code>

The <Code style="color:lime">Build.cmd</Code> is a Windows Script file and assists the programmer to batch all the build artifacts into a single ,
structured and unique folder for each build configuration.

This file was added for the need to gather up all the artifacts into one directory , since the directory structure
of the Project was radically changed.

Plus, the Build.cmd script is itself a new project - the script is actually nothing 
more than a 'Build Guider' , as I call it out.

## How the `Build.cmd` works?

The meaning of 'Build Guider' is for me two meanings that describe different purposes:

1. It builds out all the projects it finds as 'valid' ones, and
2. It batches all the outputs instructed accordingly.

To achieve this , the `Build.cmd` script handles this role by adding custom build guide files , that have the
`.script` extension.

That's why and I call it a 'Build Guider' , because it depends on these files so as to do whatever the need is , 
plus to ensure that every build command is run regardlessly of the error code and without a doubt for 
the running commands.

### The true nature and flexibility of `Build.cmd` file

As I mentioned above , the script file reads out and builds given the `.script` files.

However , to make this a little easier , the script searches itself for these files
in each sub-directory of the directory this script file is running on.

If it locates `.script` files , it will execute the commands that those contain from 
the first to the last , until it finishes out which will then move to the next file it finds ,
until no more files are found out.

The commands that the `.script` files contain are called "Actions" , which are 
pre-defined targets that need to be executed , so as to form a build target.

Due to the statement we made above , which is that all commands execute regardlessly 
of error codes , gives great freedom to programmers , because those "Actions" 
execute even if a condition is NOT satisfied. (For example the "Copy" target may 
have been supplied an invalid or non-existent path , but that will NOT stop from 
NOT continuing the build.)

This allows programmers to mostly not spend time in writing build code , but instead
allocate their time to other , useful stuff they think should do.

Actions that are existent until now:

| Action | Exists for Build.script  | Exists for Boss.script | Exists for Global.script | Exists for GlobalAfter.script | Description |
| --------| ---------------------------- | --------------------------- | ----------------------------- | ------------------------------------| -------------- |
| Build   |   Yes  |  No | No | No | Builds the specified project file.
| Restore | Yes   |  No | No | No | Restores the specified project file.
| Wait    | Yes    | Yes | Yes | Yes | Suspends build execution for the specified seconds.
| Copy  | Yes     | Yes  | Yes | Yes | Copies a file or a set of files from one location to another.
| Move  | Yes    | Yes | Yes | Yes | Move a file or a set of files from one location to another.
| Console | Yes   | Yes | Yes | Yes | Shows a message to the console.
| CreateDir | Yes   | Yes | Yes | Yes | Creates a directory , IF it does not exist.
| DeleteDir | Yes    | Yes | Yes | Yes | Deletes a directory , IF it does exist.
| DeleteFile | Yes   | Yes | Yes | Yes | Deletes the specified file.
| Clean | Yes   | No  | No | No | Cleans a project or solution by using the `dotnet clean` command.
| Stamp | Yes | Yes | No | No | Creates a stamp file that contains the current date and time. Tools can use this to determine build dates.
| CreateFile | Yes | Yes | Yes | Yes | Creates a new , empty UTF-8 file. 
| Execute | Yes | Yes | Yes | Yes | Executes the specified command in the build environment.
| SetVariable | Yes | Yes | Yes | Yes | Creates a new environment variable with the specified value.
| DeleteVariable | Yes | Yes | Yes | Yes | Deletes a previously-created environment variable.

Note: All Action identifiers are case-sensitive.

These are the actions that are existing for now. You can also see which action is supported and
when.

These Actions are a declarative component and as metioned above , they are executed 
as they were found from first to last.

An example Build.script file:

~~~SCRIPT
# This is a comment
[Restore:AProject.csproj]
# The above line will restore the specified project.
[Build:AProject.csproj]
# The above line will build the specified project.
[Copy:.\bin\net70\*.*$..\dir\output]
# The above line will copy the contents of .\bin\net70 to a directory ..\dir\output .
~~~

This was a complete version of a script file that restores and 
builds a project , and finally copies the artifacts to a directory
called "..\dir\output".

The actions in script files are components (blocks). Specifically the Actions 
are defined with this way:

~~~
[Action-Identifier:Param{$OptParam}]
~~~

Where `Action-Identifier` is the Action name , such as "Move" ,
the `Param` the specified parameter to be passed to the action , 
and the `OptParam` is an optional Action parameter required by
some Actions only , such as "Move".

Note that for the `OptParam` to work , it requires the dollar `$` seperator
after the last character of the first parameter and before the first 
character of the optional Action parameter.

Now that you know about the Actions , the flexibility of `Build.cmd`
file and building a `.script` file , it is time to learn the types of 
`.script` files and use them properly in a build scenario.

### The Types of `.script` files

The types of `.script` files are four:

- The normal `BUILD.script` file
- The always-running-only-once `Boss.script` file
- The `Global.script` that is executed just before the `BUILD.script` file
- The `GlobalAfter.script` that is executed just after the `BUILD.script` file

By default , all the Actions can only run in the `BUILD.script` file.

About which Actions are allowed to run and where , see the Actions table.

The Build Guider Execution Hierarchy (Flow Chart):

~~~
(Start)
    |
    |
    |
  |---------------------------------|
  |  Execute  Boss.script           |
  |---------------------------------|
                  |
                  |
                  |<|----------------------------------------------------|
                  |                                                      |
            /--------------\                                             |
           /                \                                            |
   False  /  Sub-Directories \ True                                      |
  |-------\  Exist?          /--------------|                            |
  |        \                /               |                            |
  |         \--------------/                |                            |
  |                                         |                            |
  |                     | ------------------------------------- |        |    
  |                     |  Execute Global.script if exists      |        |
  |                     |  Execute BUILD.script if exists       |--------|
  |                     |  Execute GlobalAfter.script if exists |
  |                     | ------------------------------------- |
  |
  |
  |
  |
(Exit)
~~~

This is a very plain and theoritical view of how `Build.cmd` actually works
and acts.

While sub-directories exist , the script will continue searching for these files
and if finds them , it executes them with the order mentioned in the chart.

Be noted though , that `Global.script` and `GlobalAfter.script` will be executed only
when `BUILD.script` exists in the same directory with them.

In the file-level , we encounter the same results:

~~~SCRIPT
1. # This is a comment 
2. [Restore:AProject.csproj] 
3. # The above line will restore the specified project.
4. [Build:AProject.csproj]
5. # The above line will build the specified project.
6. [Copy:.\bin\net70\*.*$..\dir\output]
7. # The above line will copy the contents of .\bin\net70 to a directory ..\dir\output .
~~~

The `Build.cmd` will execute the commands in the following way.

~~~
1. Comment ignored
2. Restore the project named as "AProject.csproj".
3. Comment ignored
4. Build the project named as "AProject.csproj".
5. Comment ignored
6. Copy all files from ".\bin\net70\*.*" to "..\dir\output"
7. Comment ignored
~~~

Generally , if for example the command 2 fails , then execution does immediately continue
to the next command without even getting the error code.

So the `.script` files are generally executing the one after the another.

However , the one that will always execute first before sub-directory
and once in every `Build.cmd` instance is `Boss.script`. You can use this
file to create and/or prepare the build environment.

Additionally , `Build.cmd` sessions are persistent across instances because it
uses environment variables to save argument data , except in some rare cases.

Another aspect you need to know is that `Global.script` and `GlobalAfter.script`
can be located in the same directory as every `BUILD.script` because are the `Global.script`
executes always just before the `BUILD.script` and `GlobalAfter.script` executes
just right after `BUILD.script`.

Using these files allow you to prepare project environments locally and then destroying them.

## The <Code style="color:green">Build.cmd</Code> Command-Line Interface

Although that you can use for only one argument the script command-line interface ,
the most work can be done using environment variables.

The Environment variables that are recognized by <Code style="color:green">Build.cmd</Code>
are: 

- <Code style="color:red">DOTNT_PATH</Code> is an environment variable that must point to a valid
.NET 7.0 SDK directory where the <Code style="color:darkred">dotnet.exe</Code> executable is located.
If you do not supply this environment variable with a valid .NET 7.0 SDK path , then the __script__ will search
out for the .NET SDK path.

- <Code style="color:red">CUSTOM_DOTNT_BUILD_ARGS</Code> is an environment variable that adds to the .NET Build
    Action positional arguments that might be needed during building.

- <Code style="color:red">CUSTOM_DOTNT_RESTORE_ARGS</Code>  is an environment variable that adds to the .NET Restore
    Action positional arguments that might be needed during restoration actions.

