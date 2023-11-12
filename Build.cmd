@Echo off
Set "_BD=%~dp0"
Shift /1

Echo ========================================
Echo Custom build script for MDCFR Library.
Echo ========================================
Echo ^(C^) MDCDI1315. All Rights Reserved.
Echo ========================================
1>nul timeout /t 2 /nobreak 
for /f "tokens=1,2,3,4,5,6,7 delims= " %%d in ("%*") do (
	if "%%d" == "/?" (
		goto :Help
	) else if "%%d" == "-?" (
		goto :Help
	) else if /I "%%d" == "-help" (
		goto :Help
	) else if /I "%%d" == "--help" (
		goto :Help
	) else if /I "%%d" == "/help" (
		goto :Help
	) else if /I "%%d" == "-resetenv" (
		Echo INFO: Environment was resetted sucessfully.
		Set CUSTOM_DOTNT_BUILD_ARGS=
		Set CUSTOM_DOTNT_RESTORE_ARGS=
		Set DOTNT_PATH=
		Exit /b 0
	) else if /I "%%d" == "-clear" (
		Echo INFO: Clearing build artifacts.
		Echo NOTICE: Tool will NOT execute ^(build^) anything after this.
		Echo NOTICE: Explicitly call the script without arguments to run the build again.
		1>nul rd /s /q "%cd%\bin"
		Echo INFO: Done!
		Exit /b 0
	)
	
	if not defined CUSTOM_DOTNT_BUILD_ARGS (
		if /I "%%d" == "-config" (
			Set "CUSTOM_DOTNT_BUILD_ARGS=%CUSTOM_DOTNT_BUILD_ARGS% --configuration "%%e" "
		) else if /I "%%d" == "-verbosity" (
			Set "CUSTOM_DOTNT_BUILD_ARGS=%CUSTOM_DOTNT_BUILD_ARGS% --verbosity "%%e" "
			Set "CUSTOM_DOTNT_RESTORE_ARGS=%CUSTOM_DOTNT_RESTORE_ARGS% --verbosity "%%e" "
		) else if /I "%%d" == "-dbs" (
			Set "CUSTOM_DOTNT_BUILD_ARGS=%CUSTOM_DOTNT_BUILD_ARGS% --disable-build-servers "
		)
	) else (
		Echo WARNING: Custom arguments variable is already set. No options will be accepted from the command-line.
		Echo INFO: If you want the command-line arguments take again effect , delete them first by calling -resetenv
		Echo INFO: variable , then call again this build script with your needed arguments.
	)
)
Echo Starting out...
if not defined DOTNT_PATH (
for /f "tokens=*" %%1 in ('where dotnet.exe') do Set "DOTNT_PATH=%%~dp1"
if not defined DOTNT_PATH (
Echo ERROR: DOTNT_PATH environment variable not set.
Echo INFO: Supply this variable with a valid .NET SDK ^> 7 path and retry.
call :Exit 3
)) else (
if not exist "%DOTNT_PATH%\dotnet.exe" (
Echo ERROR: The path supplied in DOTNT_PATH variable is invalid.
Echo INFO: Supply this variable with a valid .NET SDK ^> 7 path and retry.
call :Exit 3
))

if defined CUSTOM_DOTNT_BUILD_ARGS (
Echo INFO: Custom dotnet arguments for build operations found. These
Echo INFO: arguments will be passed to dotnet.exe
Echo INFO: as restore options.
) else Set "CUSTOM_DOTNT_BUILD_ARGS= --verbosity normal"

if defined CUSTOM_DOTNT_RESTORE_ARGS (
Echo INFO: Custom dotnet arguments for restore operations found. These
Echo INFO: arguments will be passed to dotnet.exe
Echo INFO: as restore options.
) else Set "CUSTOM_DOTNT_RESTORE_ARGS= --verbosity normal"

Echo INFO: Starting operation...
Echo INFO: Checking if the current directory is the location of this script...
if not "%_BD%" == "%cd%\" (
Echo WARNING: This instance is NOT running on the directory where it is situated.
Echo INFO: Attempting to change to the script location...
chdir /d "%_BD%"
Echo INFO: Done!
) else (Echo INFO: Done!)
Set _BD=

if exist "%cd%\boss.script" (
	Echo INFO: Found boss script file. Executing the file "%cd%\Boss.script" first.
	Call :BossLoadFile "%cd%"
)

for /f "tokens=* delims=" %%$ in ('dir /a:D /b /d /s "%cd%\*"') do (
	if exist "%%$\BUILD.script" (
		pushd "%%$"
		if exist "%%$\Global.script" (
			Echo INFO: Found global script file. Executing it...
			Call :GlobalLoadFile "%%$"
			Echo INFO: Script file executed sucessfully.
		)
		Echo INFO: Found valid script file. Executing it...
		Call :LoadFile "%%$"
		Echo INFO: Script file executed sucessfully.
		if exist "%%$\GlobalAfter.script" (
			Echo INFO: Found global script file that must be executed after the normal build file. Executing it...
			Call :GlobalAfterLoadFile "%%$"
			Echo INFO: Script file executed sucessfully.
		)
		popd
	)
)

Echo INFO: Done building.
Echo INFO: Exiting sucessfully...

Exit /b 0


:Help
Echo =======================================
Echo  Custom script file for building MDCFR
Echo =======================================
Echo Why this file exists?
Echo  This file exists for two reasons - 
Echo  the one is that you can build ALL 
Echo  the projects at once and for the need
Echo  to collect all the build artifacts at
Echo  one folder.
Echo.  
Echo  This file does not just invoke the builds ,
Echo  it guides them so as to produce the needed
Echo  result.
Echo  This is achieved by using a custom engine
Echo  included with this script. Then with the 
Echo  proper files that this engine reads out ,
Echo  it produces the expected result.
Echo.
Echo  How does the engine works?
Echo   Well , the operation principle is far
Echo   enough simple.
Echo   This file searches out for possible
Echo   script files that may exist in each 
Echo   sub-directory of the directory which
Echo   this script resides.
Echo   Then , it executes them accordingly.
Echo   For some favor and to add flexibility , 
Echo   there were added some other support 
Echo   and optional of course files that can help
Echo   in guiding the project build behavior.
Echo   Here is provided an example tree of how
Echo   it works:
Echo   =========================================
Echo   Tree(Root folder)
Echo   ^|
Echo   ^|-----------^|
Echo   ^|           ^|
Echo   ^|        ^|-----------------------------^|
Echo   ^|        ^|  An_Project_Directory       ^|
Echo   ^|        ^|-----------------------------^|
Echo   ^|               ^|
Echo   ^|               ^|---^> [Global.script] (This file will be executed just before BUILD.script. Useful for environment preparation.)
Echo   ^|               ^|---^> BUILD.script (The file that this engine uses to execute project-related commands.)
Echo   ^|               ^|---^> [GlobalAfter.script] (This file will be executed just after BUILD.script. Useful for copy preparation.)
Echo   ^|
Echo   ^|---^> [Boss.script] (This file is executed always the first , even before enumeration starts. Useful for preparing output directories.)
Echo   ^|---^> Build.cmd (This file!)
Echo   =========================================
Echo   All the files that are inside brackets are optional.
Echo   The files that are inside brackets do not have the full functionality
Echo   provided instead to BUILD.script file. This is absolutely logical since
Echo   someone would not use a Build action inside a script file after building has finished.
Echo.
Echo  Advantages/limitations:
Echo   -^> An advantage of using simple syntax files is to create fast build trees 
Echo   and then taking their output and just copy/move the artifacts wherever you want.
Echo   -^> Writing such files are easy and understandable by everyone.
Echo   -^> This engine provides default action sets which include file copy/move actions,
Echo   directory creation/deletion and other actions as well!
Echo   -^> Because it is actually a build guider , it should have support for 
Echo   passing to build actions custom command-line.
Echo   Although that someone can add custom command-line arguments , these will be applied in 
Echo   all subsequent build actions specified , which is a large problem.
Echo   -^> It is not possible to have conditional statements , plus build variables
Echo   which this in turn , makes it hard-to-use.
Echo   -^> This tool has been designed only for .NET projects.
Echo.
Echo  Usage of this tool:
Echo   This build script is working out using and command-line arguments , but mostly 
Echo   depends heavily on environment variables.
Echo   Therefore , to use it correctly:
Echo     1. DOTNT_PATH is an environment variable that points to the .NET SDK directory.
Echo     Although that if you do not supply it does actually search for the
Echo     .NET SDK installation , it is a good practice to set this one to an installation
Echo     that can compile .NET code for .NET 7 and above.
Echo     2. CUSTOM_DOTNT_BUILD_ARGS is an environment variable that adds to the .NET build
Echo     action positional arguments that might be needed during building.
Echo     3. CUSTOM_DOTNT_RESTORE_ARGS is an environment variable that adds to the .NET restore
Echo     action positional arguments that might be needed during restoring.
Echo     The last two variables can accept any valid .NET SDK arguments of the build and restore
Echo     commands , respectively.
Echo   For the command-line , these are the options that you can use:
Echo   -config "configuration" : This argument builds the project with the specified configuration.
Echo   -verbosity "q[uiet]^|m[inimal]^|n[ormal]^|d[etailed]^|diag[nostic]": Specifies the dotnet verbosity to use.
Echo   -dbs : For all build commands , do not use any active build servers. 
Echo   -clear: Clears the bin directory from any previous artifacts.
Echo   Instead , use the tool to compile the result.
Echo.
Echo  Writing the build files:
Echo   Writing a new build file means that you add that file to the build queue.
Echo   Be noted , though , that this script does not load the build files by order , 
Echo   but in the way those are enumerated through the directories.
Echo.
Echo  Writing a build file Walkthrough
Echo   Writing build files is easy enough because the goal was actually to just execute
Echo   simple commands , and not making complex decisions.
Echo   To build and get the resulting assemblies , you need the project file with all the code/resources
Echo   of the project , (the project itself with a few words) and then with the proper actions , to move
Echo   the files in the location you want to.
Echo   You must always place the build file in the project directory that contains the project file (csproj , vbproj , fsproj)
Echo   and you must make sure that is named build.script. Otherwise , it will NOT be recognized by the engine.
Echo   Then , create a new text file , give it the name stated , and open it in any text editor.
Echo   Because restoring is explicitly disabled when building , you must add two lines in the file:
Echo   ======================================
Echo   [Restore:Your-project-name.csproj]
Echo   [Build:Your-project-name.csproj]
Echo   ======================================
Echo   Of course , where Your-project-name you will put the project filename to build.
Echo   These two lines specify that: 
Echo    1. Will locate a project file named Your-project-name.csproj inside 
Echo    the directory you placed the newly created file.
Echo    2. Will first restore the project dependencies.
Echo    3. Will attempt to build the project.
Echo   Note: If you have multiple build files(or multiple projects), even if
Echo   one failed , it will continue executing until all build files were processed.
Echo   This one example was really , really simple , huh?
Echo   Let's now imagine a case where we want to move those artifacts to the parent directory...
Echo   How that will be done?
Echo   Just add the below line to your build file you saved just before:
Echo   =======================================
Echo   [Move:.\bin\Debug\*.*$..\]
Echo   =======================================
Echo   This line will move all the files from the bin\Debug directory 
Echo   to the project's parent directory.
Echo.
Echo   About Actions:
Echo   Just moments ago , you had written out three lines that specified 
Echo   the restoration , building the project and moving the artifacts to 
Echo   the project's parent directory.
Echo   Those lines starting with brackets are called "Actions"
Echo   and are executing a specific job. Their exact syntax is:
Echo   [Action-Identifier:Parameter{$OtherParameter}]
Echo   Where the Action-Identifier is the name of the action to execute,
Echo   Parameter is the first parameter to pass to the action , 
Echo   and the OtherParameter is the second and optional parameter to pass
Echo   to the action. Some actions , however , 
Echo   require the OtherParameter parameter frequently.
Echo   There are many actions available for usage , such as Wait , CreateDir , Move , Copy , etc.
Echo   These actions are easy to learn and use and you will notice that when writing such files.
Echo.  
Exit 1



:GlobalAfterLoadFile
Rem The LoadFile procedure will track down , open and find the situation to do.
Rem The commands will be located in seperate script files and the commands 
Rem will be executed in the order they appear.
Rem All the commands are written inside brackets ([]).
Rem Single-line only comments can be specified using the hash (#) character at start.
Echo Getting into %cd% ...
for /f "tokens=*" %%@ in ('more "%1\GlobalAfter.script"') do (
	for /f "eol=# tokens=1,2,3,4* delims=[]:$" %%a in ("%%@") do (
		if "%%a" == "Copy" (
			Echo Copying %%b to %%c ...
			1>nul 2>nul Copy /Y /V "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done copying.
		) else if "%%a" == "Move" (
			Echo Moving %%b to %%c ...
			1>nul 2>nul Move /Y "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done moving.
		) else if "%%a" == "CreateDir" (
			Echo Creating directory %%b...
			if not exist "%%b" (mkdir "%%b")
			Echo Directory %%b created.
		) else if "%%a" == "DeleteDir" (
			Echo Deleting Directory %%b...
			if exist "%%b" (rd /s /q "%%b")
			Echo Directory %%b was deleted sucessfully...
		) else if "%%a" == "Wait" (
			if "%%b" == ""  (
				1>nul timeout.exe /t 1 /nobreak
			) else (
				1>nul timeout.exe /t %%b /nobreak
			)
		) else if "%%a" == "Console" (
			if "%%b" == "" (Echo.) else (Echo %%b)
		) else (
			Echo WARNING: Action "%%b" is unrecognized.
			Echo INFO: Check that the name "%%b" is a valid Action Identifier.
		)
	)
)
Echo Done!
Echo Exiting current directory (Global After script) , %1.
exit /b 






:BossLoadFile
Rem The LoadFile procedure will track down , open and find the situation to do.
Rem The commands will be located in seperate script files and the commands 
Rem will be executed in the order they appear.
Rem All the commands are written inside brackets ([]).
Rem Single-line only comments can be specified using the hash (#) character at start.
Echo Getting into %cd% ...
for /f "tokens=*" %%@ in ('more "%1\BOSS.script"') do (
	for /f "eol=# tokens=1,2,3,4* delims=[]:$" %%a in ("%%@") do (
		if "%%a" == "Copy" (
			Echo Copying %%b to %%c ...
			1>nul 2>nul Copy /Y /V "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done copying.
		) else if "%%a" == "Move" (
			Echo Moving %%b to %%c ...
			1>nul 2>nul Move /Y "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done moving.
		) else if "%%a" == "CreateDir" (
			Echo Creating directory %%b...
			if not exist "%%b" (mkdir "%%b")
			Echo Directory %%b created.
		) else if "%%a" == "DeleteDir" (
			Echo Deleting Directory %%b...
			if exist "%%b" (rd /s /q "%%b")
			Echo Directory %%b was deleted sucessfully...
		) else if "%%a" == "Wait" (
			if "%%b" == ""  (
				1>nul timeout.exe /t 1 /nobreak
			) else (
				1>nul timeout.exe /t %%b /nobreak
			)
		) else if "%%a" == "Console" (
			if "%%b" == "" (Echo.) else (Echo %%b)
		) else (
			Echo WARNING: Action "%%b" is unrecognized.
			Echo INFO: Check that the name "%%b" is a valid Action Identifier.
		)
	)
)
Echo Done!
Echo Exiting current directory (boss script) , %1.
exit /b 


:GlobalLoadFile
Rem The LoadFile procedure will track down , open and find the situation to do.
Rem The commands will be located in seperate script files and the commands 
Rem will be executed in the order they appear.
Rem All the commands are written inside brackets ([]).
Rem Single-line only comments can be specified using the hash (#) character at start.
Echo Getting into %cd% ...
for /f "tokens=*" %%@ in ('more "%1\Global.script"') do (
	for /f "eol=# tokens=1,2,3,4* delims=[]:$" %%a in ("%%@") do (
		if "%%a" == "Copy" (
			Echo Copying %%b to %%c ...
			1>nul 2>nul Copy /Y /V "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done copying.
		) else if "%%a" == "Move" (
			Echo Moving %%b to %%c ...
			1>nul 2>nul Move /Y "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done moving.
		) else if "%%a" == "CreateDir" (
			Echo Creating directory %%b...
			if not exist "%%b" (mkdir "%%b")
			Echo Directory %%b created.
		) else if "%%a" == "DeleteDir" (
			Echo Deleting Directory %%b...
			if exist "%%b" (rd /s /q "%%b")
			Echo Directory %%b was deleted sucessfully...
		) else if "%%a" == "Wait" (
			if "%%b" == ""  (
				1>nul timeout.exe /t 1 /nobreak
			) else (
				1>nul timeout.exe /t %%b /nobreak
			)
		) else if "%%a" == "Console" (
			if "%%b" == "" (Echo.) else (Echo %%b)
		) else (
			Echo WARNING: Action "%%b" is unrecognized.
			Echo INFO: Check that the name "%%b" is a valid Action Identifier.
		)
	)
)
Echo Done!
Echo Exiting current directory (Global script) , %1.
exit /b 


:LoadFile
Rem The LoadFile procedure will track down , open and find the situation to do.
Rem The commands will be located in seperate script files and the commands 
Rem will be executed in the order they appear.
Rem All the commands are written inside brackets ([]).
Rem Single-line only comments can be specified using the hash (#) character at start.
Echo Getting into %cd% ...
for /f "tokens=*" %%@ in ('more "%1\BUILD.script"') do (
	for /f "eol=# tokens=1,2,3,4* delims=[]:$" %%a in ("%%@") do (
		if "%%a" == "Restore" (
			Rem Target Restore: Restore the dependencies of the specified project.
			Echo Restoring %%b ...
			"%DOTNT_PATH%\dotnet.exe" restore "%1\%%b" --force %CUSTOM_DOTNT_RESTORE_ARGS% 
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done restoring.
		) else if "%%a" == "Build" (
			Rem Target Build: Build the specified project.
			Echo Building %%b ...
			"%DOTNT_PATH%\dotnet.exe" build "%1\%%b" --no-restore %CUSTOM_DOTNT_BUILD_ARGS% 
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done building.
		) else if "%%a" == "Copy" (
			Echo Copying %%b to %%c ...
			1>nul 2>nul Copy /Y /V "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done copying.
		) else if "%%a" == "Move" (
			Echo Moving %%b to %%c ...
			1>nul 2>nul Move /Y "%%b" "%%c"
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done moving.
		) else if "%%a" == "Clean" (
			Echo Cleaning %%b...
			"%DOTNT_PATH%\dotnet.exe" clean "%1\%%b" %CUSTOM_DOTNT_BUILD_ARGS%
			if not errorlevel 0 Call :Exit %Errorlevel%
			Echo Done cleaning.
		) else if "%%a" == "CreateDir" (
			Echo Creating directory %%b...
			if not exist "%%b" (mkdir "%%b")
			Echo Directory %%b created.
		) else if "%%a" == "DeleteDir" (
			Echo Deleting Directory %%b...
			if exist "%%b" (rd /s /q "%%b")
			Echo Directory %%b was deleted sucessfully...
		) else if "%%a" == "Wait" (
			if "%%b" == ""  (
				1>nul timeout.exe /t 1 /nobreak
			) else (
				1>nul timeout.exe /t %%b /nobreak
			)
		) else if "%%a" == "Console" (
			if "%%b" == "" (Echo.) else (Echo %%b)
		) else (
			Echo WARNING: Action "%%b" is unrecognized.
			Echo INFO: Check that the name "%%b" is a valid Action Identifier.
		)
	)
)
Echo Done!
Echo Exiting current directory , %1.
exit /b 



:Exit
if not "%1" == "0" (Echo Error detected while executing: Returned code %1.)
Echo Exiting...
Exit %1
exit /b %1