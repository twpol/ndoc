@echo off
cd ..
if not exist ndoc-src.zip goto zip
echo ndoc-src.zip exists. rename it or delete it.
goto end
:zip
"C:\Program Files\WinZip\WZZIP.EXE" -P -x@ndoc\x-files.txt ndoc-src.zip @ndoc\files.txt
:end
cd ndoc
