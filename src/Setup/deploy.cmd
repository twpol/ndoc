@echo off
cd Release
if %1x == x goto usage
if %2x == -allx goto deployall

scp NDocSetup.msi %1@ndoc.sourceforge.net:/home/groups/n/nd/ndoc/htdocs/setup
goto end

:deployall
scp InstMsiA.Exe InstMsiW.Exe NDocSetup.msi %1@ndoc.sourceforge.net:/home/groups/n/nd/ndoc/htdocs/setup
goto end

:usage
echo usage: deploy username [-all]

:end
cd ..

