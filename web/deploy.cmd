@echo off
if %1x == x goto usage
scp xmarks.css index.html doc.html rdf.html %1@xmarks.sourceforge.net:/home/groups/x/xm/xmarks/htdocs
goto end
:usage
echo usage: deploy username
:end
