build: NDoc.mak
	nmake /nologo /f NDoc.mak

clean: NDoc.mak
	nmake /nologo /f NDoc.mak clean

PROJECTS = \
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

tags.html: Documenter\Msdn\xslt\tags.xslt Documenter\Msdn\xslt\doc.xslt
	saxon Documenter\Msdn\xslt\tags.xslt Documenter\Msdn\xslt\doc.xslt > tags.html
