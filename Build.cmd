@Echo off
Set "_BD=%~dp0"
Shift /1

Echo ========================================
Echo Custom build script for MDCFR Library.
Echo ========================================
Echo ^(C^) MDCDI1315. All Rights Reserved.
Echo ========================================
1>nul timeout /t 2 /nobreak 

Echo INFO: Starting operation...
Echo INFO: Checking if the current directory is the location of this script...
if not "%_BD%" == "%cd%\" (
	Echo WARNING: This instance is NOT running on the directory where it is situated.
	Echo INFO: Attempting to change to the script location...
	chdir /d "%_BD%"
)
Echo INFO: Done!
Set _BD=
if defined DEBUG (
Echo: Executing: ".\Tools\BuildTool\MDCFR-Builder.exe" %* "--cwd=%cd%\"
)
@".\Tools\BuildTool\MDCFR-Builder.exe" %* "--cwd=%cd%\"