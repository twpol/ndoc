BUILD_DIR = build
DEBUG = /debug+

all: $(BUILD_DIR)\NDocGui.exe $(BUILD_DIR)\NDocConsole.exe $(BUILD_DIR)\NDoc.Core.dll $(BUILD_DIR)\NDoc.Documenter.Xml.dll $(BUILD_DIR)\NDoc.VisualStudio.dll $(BUILD_DIR)\NDoc.Test.dll $(BUILD_DIR)\NDoc.Documenter.Msdn.dll $(BUILD_DIR)\NDoc.Documenter.JavaDoc.dll

$(BUILD_DIR):
	md $(BUILD_DIR)

$(BUILD_DIR)\NDocGui.exe: $(BUILD_DIR) $(BUILD_DIR)\NDoc.Core.dll $(BUILD_DIR)\NDoc.Documenter.Msdn.dll $(BUILD_DIR)\NDoc.Documenter.Xml.dll $(BUILD_DIR)\NDoc.Documenter.JavaDoc.dll $(BUILD_DIR)\NDoc.VisualStudio.dll
	resgen Gui\AboutForm.resx $(BUILD_DIR)\NDoc.Gui.AboutForm.resources
	resgen Gui\AssemblySlashDocForm.resx $(BUILD_DIR)\NDoc.Gui.AssemblySlashDocForm.resources
	resgen Gui\ErrorForm.resx $(BUILD_DIR)\NDoc.Gui.ErrorForm.resources
	resgen Gui\MainForm.resx $(BUILD_DIR)\NDoc.Gui.MainForm.resources
	resgen Gui\NamespaceSummariesForm.resx $(BUILD_DIR)\NDoc.Gui.NamespaceSummariesForm.resources
	resgen Gui\SolutionForm.resx $(BUILD_DIR)\NDoc.Gui.SolutionForm.resources
	csc /nologo /target:winexe /out:$(BUILD_DIR)\NDocGui.exe $(DEBUG) Gui\AboutForm.cs Gui\AssemblyInfo.cs Gui\AssemblySlashDocForm.cs Gui\BuildWorker.cs Gui\ErrorForm.cs Gui\HeaderGroupBox.cs Gui\MainForm.cs Gui\NamespaceSummariesForm.cs Gui\SolutionForm.cs /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Xml.dll /reference:System.Data.dll /reference:$(BUILD_DIR)\NDoc.Core.dll /reference:$(BUILD_DIR)\NDoc.Documenter.Msdn.dll /reference:$(BUILD_DIR)\NDoc.Documenter.Xml.dll /reference:$(BUILD_DIR)\NDoc.Documenter.JavaDoc.dll /reference:$(BUILD_DIR)\NDoc.VisualStudio.dll /resource:Gui\About.rtf,NDoc.Gui.About.rtf /resource:$(BUILD_DIR)\NDoc.Gui.AboutForm.resources /resource:$(BUILD_DIR)\NDoc.Gui.AssemblySlashDocForm.resources /resource:$(BUILD_DIR)\NDoc.Gui.ErrorForm.resources /resource:$(BUILD_DIR)\NDoc.Gui.MainForm.resources /resource:$(BUILD_DIR)\NDoc.Gui.NamespaceSummariesForm.resources /resource:$(BUILD_DIR)\NDoc.Gui.SolutionForm.resources

$(BUILD_DIR)\NDocConsole.exe: $(BUILD_DIR) $(BUILD_DIR)\NDoc.Core.dll $(BUILD_DIR)\NDoc.Documenter.JavaDoc.dll $(BUILD_DIR)\NDoc.Documenter.Msdn.dll $(BUILD_DIR)\NDoc.Documenter.Xml.dll
	csc /nologo /target:exe /out:$(BUILD_DIR)\NDocConsole.exe $(DEBUG) Console\AssemblyInfo.cs Console\Console.cs /reference:System.dll /reference:System.Xml.dll /reference:$(BUILD_DIR)\NDoc.Core.dll /reference:$(BUILD_DIR)\NDoc.Documenter.JavaDoc.dll /reference:$(BUILD_DIR)\NDoc.Documenter.Msdn.dll /reference:$(BUILD_DIR)\NDoc.Documenter.Xml.dll

$(BUILD_DIR)\NDoc.Core.dll: $(BUILD_DIR)
	csc /nologo /target:library /out:$(BUILD_DIR)\NDoc.Core.dll $(DEBUG) Core\AssemblyInfo.cs Core\AssemblySlashDoc.cs Core\BaseDocumenter.cs Core\BaseDocumenterConfig.cs Core\DocumenterException.cs Core\EmbeddedResources.cs Core\HtmlHelp.cs Core\IDocumenter.cs Core\IDocumenterConfig.cs Core\Project.cs Core\XsltDocumenter.cs /reference:System.dll /reference:System.Xml.dll /reference:System.Design.dll /reference:System.Drawing.dll

$(BUILD_DIR)\NDoc.Documenter.Xml.dll: $(BUILD_DIR) $(BUILD_DIR)\NDoc.Core.dll
	csc /nologo /target:library /out:$(BUILD_DIR)\NDoc.Documenter.Xml.dll $(DEBUG) Documenter\Xml\AssemblyInfo.cs Documenter\Xml\XmlDocumenter.cs Documenter\Xml\XmlDocumenterConfig.cs /reference:System.dll /reference:System.XML.dll /reference:System.Design.dll /reference:System.Drawing.Design.dll /reference:System.Drawing.dll /reference:$(BUILD_DIR)\NDoc.Core.dll

$(BUILD_DIR)\NDoc.VisualStudio.dll: $(BUILD_DIR)
	csc /nologo /target:library /out:$(BUILD_DIR)\NDoc.VisualStudio.dll $(DEBUG) VisualStudio\AssemblyInfo.cs VisualStudio\Project.cs VisualStudio\ProjectConfig.cs VisualStudio\Solution.cs /reference:System.dll /reference:System.Data.dll /reference:System.Xml.dll

$(BUILD_DIR)\NDoc.Test.dll: $(BUILD_DIR)
	csc /nologo /target:library /out:$(BUILD_DIR)\NDoc.Test.dll $(DEBUG) Test\AssemblyInfo.cs Test\Test.cs /reference:System.dll /reference:System.Xml.dll

$(BUILD_DIR)\NDoc.Documenter.Msdn.dll: $(BUILD_DIR) $(BUILD_DIR)\NDoc.Core.dll
	csc /nologo /target:library /out:$(BUILD_DIR)\NDoc.Documenter.Msdn.dll $(DEBUG) Documenter\Msdn\AssemblyInfo.cs Documenter\Msdn\MsdnDocumenter.cs Documenter\Msdn\MsdnDocumenterConfig.cs /reference:System.dll /reference:System.Xml.dll /reference:System.Drawing.Design.dll /reference:System.Design.dll /reference:System.Drawing.dll /reference:$(BUILD_DIR)\NDoc.Core.dll /resource:Documenter\Msdn\css\MSDN.css,NDoc.Documenter.Msdn.css.MSDN.css /resource:Documenter\Msdn\xslt\allmembers.xslt,NDoc.Documenter.Msdn.xslt.allmembers.xslt /resource:Documenter\Msdn\xslt\common.xslt,NDoc.Documenter.Msdn.xslt.common.xslt /resource:Documenter\Msdn\xslt\event.xslt,NDoc.Documenter.Msdn.xslt.event.xslt /resource:Documenter\Msdn\xslt\field.xslt,NDoc.Documenter.Msdn.xslt.field.xslt /resource:Documenter\Msdn\xslt\filenames.xslt,NDoc.Documenter.Msdn.xslt.filenames.xslt /resource:Documenter\Msdn\xslt\individualmembers.xslt,NDoc.Documenter.Msdn.xslt.individualmembers.xslt /resource:Documenter\Msdn\xslt\member.xslt,NDoc.Documenter.Msdn.xslt.member.xslt /resource:Documenter\Msdn\xslt\memberoverload.xslt,NDoc.Documenter.Msdn.xslt.memberoverload.xslt /resource:Documenter\Msdn\xslt\memberscommon.xslt,NDoc.Documenter.Msdn.xslt.memberscommon.xslt /resource:Documenter\Msdn\xslt\namespace.xslt,NDoc.Documenter.Msdn.xslt.namespace.xslt /resource:Documenter\Msdn\xslt\namespacehierarchy.xslt,NDoc.Documenter.Msdn.xslt.namespacehierarchy.xslt /resource:Documenter\Msdn\xslt\property.xslt,NDoc.Documenter.Msdn.xslt.property.xslt /resource:Documenter\Msdn\xslt\syntax.xslt,NDoc.Documenter.Msdn.xslt.syntax.xslt /resource:Documenter\Msdn\xslt\type.xslt,NDoc.Documenter.Msdn.xslt.type.xslt

$(BUILD_DIR)\NDoc.Documenter.JavaDoc.dll: $(BUILD_DIR) $(BUILD_DIR)\NDoc.Core.dll
	csc /nologo /target:library /out:$(BUILD_DIR)\NDoc.Documenter.JavaDoc.dll $(DEBUG) Documenter\JavaDoc\AssemblyInfo.cs Documenter\JavaDoc\JavaDocDocumenter.cs Documenter\JavaDoc\JavaDocDocumenterConfig.cs /reference:System.dll /reference:System.Xml.dll /reference:System.Design.dll /reference:System.Drawing.Design.dll /reference:System.Drawing.dll /reference:$(BUILD_DIR)\NDoc.Core.dll /resource:Documenter\JavaDoc\css\JavaDoc.css,NDoc.Documenter.JavaDoc.css.JavaDoc.css /resource:Documenter\JavaDoc\xslt\javadoc.xslt,NDoc.Documenter.JavaDoc.xslt.javadoc.xslt /resource:Documenter\JavaDoc\xslt\namespace-summary.xslt,NDoc.Documenter.JavaDoc.xslt.namespace-summary.xslt /resource:Documenter\JavaDoc\xslt\overview-summary.xslt,NDoc.Documenter.JavaDoc.xslt.overview-summary.xslt /resource:Documenter\JavaDoc\xslt\type.xslt,NDoc.Documenter.JavaDoc.xslt.type.xslt

