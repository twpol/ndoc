@echo off
cd ..
if not exist ndoc.zip goto zip
echo ndoc.zip exists. rename it or delete it.
goto end
:zip
"C:\Program Files\WinZip\WZZIP.EXE" ndoc.zip ndoc\src\Gui\bin\Release\* ndoc\src\Gui\NDocGui.exe.manifest ndoc\src\Console\bin\Release\NDocConsole.*
:end
cd ndoc
