@echo off 

set cosmoteerInstallDir=D:\SteamLibrary\steamapps\common\Cosmoteer\Bin
set targetDir=%1
 
copy "%targetDir%CosmoteerModLoader.dll" "%cosmoteerInstallDir%\CosmoteerModLoader.dll" /y
copy "%targetDir%CosmoteerModLoader.exe" "%cosmoteerInstallDir%\Cosmoteer.exe" /y