@echo off
if %1x == x goto usage
scp style.css index.html screenshots.html download.html involved.html cvs.html users.html %1@ndoc.sourceforge.net:/home/groups/n/nd/ndoc/htdocs
goto end
:usage
echo usage: deploy username
:end
