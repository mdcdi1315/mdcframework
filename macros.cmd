@Echo off

Set "Dest=%cd%\%~1"
If "%Dest%" == "%cd%\" (
Echo ERROR: This script must be run by the MSBuild task.
Echo INFO: Press any key to exit...
1>nul pause
exit /b 4
)
If "%~2" == "-publish" Set PUB_M=1
Echo INFO: Copying Unmanaged Libraries: %Dest%
Echo INFO: Copying xxhash.dll...   "%Dest%\xxhash.dll"
If not exist "%Dest%\xxhash.dll" (
(1>nul Copy /y ".\Imports\xxhash.dll" /B "%Dest%") || (
Echo ERROR: Could not copy the file.
exit /b 1
))
Echo INFO: Copying zstd.dll...   "%Dest%\zstd.dll"
If not exist "%Dest%\zstd.dll" (
(1>nul Copy /y ".\Imports\zstd.dll" /B "%Dest%" > nul) || (
Echo ERROR: Could not copy the file.
exit /b 1
))
Echo INFO: Sucessfull Operation.
If defined PUB_M (
Echo INFO: Checking for Publish build...
If not exist "%Dest%\publish\xxhash.dll" (
Echo INFO: Copying xxhash.dll...   "%Dest%\publish\xxhash.dll"
(1>nul Copy /y ".\Imports\xxhash.dll" /B "%Dest%\publish") || (
Echo ERROR: Could not copy the file.
exit /b 1
))


Rem If not exist "%Dest%\publish\Documentation.txt" (
Rem Echo INFO: Copying Documentation File...  "%Dest%\publish\Documentation.txt"
Rem (1>nul Copy /y ".\Imports\Documentation.txt" /A "%Dest%\publish") || (
Rem Echo ERROR: Could not copy the file.
Rem exit /b 1
Rem ))

If not exist "%Dest%\publish\zstd.dll" (
Echo INFO: Copying zstd.dll...   "%Dest%\publish\zstd.dll"
(1>nul Copy /y ".\Imports\zstd.dll" /B "%Dest%\publish") || (
Echo ERROR: Could not copy the file.
exit /b 1
)))
Echo INFO: Done Copying.
exit /b 0