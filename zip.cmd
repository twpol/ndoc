@echo off
setlocal
set zipname=NDoc-devel

if not exist %zipname%.zip goto zip
del %zipname%.zip

:zip
"C:\Program Files\WinZip\WZZIP.EXE" -P -ex -x@zip-x-files.txt %zipname%.zip @zip-files.txt

:end
pause
