build: NDoc.mak
	nmake /nologo /f NDoc.mak

PROJECTS = \
	Addins\NDocBuild\NDocBuild.csproj \
	Addins\NDocBuild\NDocBuildSetup\NDocBuildSetup.vdproj \
	Console\Console.csproj \
	Core\Core.csproj \
	Documenter\JavaDoc\JavaDoc.csproj \
	Documenter\Msdn\Msdn.csproj \
	Documenter\Xml\Xml.csproj \
	Gui\Gui.csproj \
	Test\Test.csproj \
	VisualStudio\VisualStudio.csproj \
	Setup\Setup.vdproj

NDoc.mak: NDoc.sln $(PROJECTS)
	.\SLiNgshoT.exe nmake NDoc.sln > NDoc.mak

clean:
	if exist ..\examples\docs rd /s /q ..\examples\docs
	if exist Addins\NDocBuild\bin rd /s /q Addins\NDocBuild\bin
	if exist Addins\NDocBuild\obj rd /s /q Addins\NDocBuild\obj
	if exist Addins\NDocBuild\NDocBuildSetup\Debug rd /s /q Addins\NDocBuild\NDocBuildSetup\Debug
	if exist Addins\NDocBuild\NDocBuildSetup\Release rd /s /q Addins\NDocBuild\NDocBuildSetup\Release
	if exist Console\bin rd /s /q Console\bin
	if exist Console\obj rd /s /q Console\obj
	if exist Core\bin rd /s /q Core\bin
	if exist Core\obj rd /s /q Core\obj
	if exist Documenter\JavaDoc\bin rd /s /q Documenter\JavaDoc\bin
	if exist Documenter\JavaDoc\obj rd /s /q Documenter\JavaDoc\obj
	if exist Documenter\Msdn\bin rd /s /q Documenter\Msdn\bin
	if exist Documenter\Msdn\obj rd /s /q Documenter\Msdn\obj
	if exist Documenter\Xml\bin rd /s /q Documenter\Xml\bin
	if exist Documenter\Xml\obj rd /s /q Documenter\Xml\obj
	if exist Gui\bin rd /s /q Gui\bin
	if exist Gui\obj rd /s /q Gui\obj
	if exist Test\bin rd /s /q Test\bin
	if exist Test\obj rd /s /q Test\obj
	if exist VisualStudio\bin rd /s /q VisualStudio\bin
	if exist VisualStudio\obj rd /s /q VisualStudio\obj
	if exist Setup\Debug rd /s /q Setup\Debug
	if exist Setup\Release rd /s /q Setup\Release

tags.html: Documenter\Msdn\xslt\tags.xslt Documenter\Msdn\xslt\doc.xslt
	saxon Documenter\Msdn\xslt\tags.xslt Documenter\Msdn\xslt\doc.xslt > tags.html
