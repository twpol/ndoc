// MsdnDocumenter.cs - a MSDN-like documenter
// Copyright (C) 2001  Kral Ferch, Jason Diamond
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Globalization;

using NDoc3.Core;
using NDoc3.Core.Reflection;
using NDoc3.Documenter.Msdn.onlinefiles;
using NDoc3.Documenter.Msdn.onlinetemplates;
using NDoc3.Xml;

namespace NDoc3.Documenter.Msdn
{
	/// <summary>The MsdnDocumenter class.</summary>
	public class MsdnDocumenter : BaseReflectionDocumenter
	{
		private enum WhichType
		{
			Class,
			Interface,
			Structure,
			Enumeration,
			Delegate,
			Unknown
		};

		private readonly Hashtable lowerCaseTypeNames;
		private readonly Hashtable mixedCaseTypeNames;

		/// <summary>
		/// Initializes a new instance of the <see cref="MsdnDocumenter" />
		/// class.
		/// </summary>
		public MsdnDocumenter(MsdnDocumenterConfig config)
			: base(config)
		{
			lowerCaseTypeNames = new Hashtable();
			lowerCaseTypeNames.Add(WhichType.Class, "class");
			lowerCaseTypeNames.Add(WhichType.Interface, "interface");
			lowerCaseTypeNames.Add(WhichType.Structure, "structure");
			lowerCaseTypeNames.Add(WhichType.Enumeration, "enumeration");
			lowerCaseTypeNames.Add(WhichType.Delegate, "delegate");

			mixedCaseTypeNames = new Hashtable();
			mixedCaseTypeNames.Add(WhichType.Class, "Class");
			mixedCaseTypeNames.Add(WhichType.Interface, "Interface");
			mixedCaseTypeNames.Add(WhichType.Structure, "Structure");
			mixedCaseTypeNames.Add(WhichType.Enumeration, "Enumeration");
			mixedCaseTypeNames.Add(WhichType.Delegate, "Delegate");
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string MainOutputFile
		{
			get
			{
				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0) {
					return Path.Combine(MyConfig.OutputDirectory,
						MyConfig.HtmlHelpName + ".chm");
				}
				return Path.Combine(MyConfig.OutputDirectory, "index.html");
			}
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string CanBuild(Project project, bool checkInputOnly)
		{
			string result = base.CanBuild(project, checkInputOnly);
			if (result != null) {
				return result;
			}

			string AdditionalContentResourceDirectory = MyConfig.AdditionalContentResourceDirectory;
			if (AdditionalContentResourceDirectory.Length != 0 && !Directory.Exists(AdditionalContentResourceDirectory))
				return string.Format("The Additional Content Resource Directory {0} could not be found", AdditionalContentResourceDirectory);

			string ExtensibilityStylesheet = MyConfig.ExtensibilityStylesheet;
			if (ExtensibilityStylesheet.Length != 0 && !File.Exists(ExtensibilityStylesheet))
				return string.Format("The Extensibility Stylesheet file {0} could not be found", ExtensibilityStylesheet);

			if (checkInputOnly) {
				return null;
			}

			string path = Path.Combine(MyConfig.OutputDirectory,
				MyConfig.HtmlHelpName + ".chm");

			string temp = Path.Combine(MyConfig.OutputDirectory, "~chm.tmp");

			try {

				if (File.Exists(path)) {
					//if we can move the file, then it is not open...
					File.Move(path, temp);
					File.Move(temp, path);
				}
			} catch (Exception) {
				result = "The compiled HTML Help file is probably open.\nPlease close it and try again.";
			}

			return result;
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Build(Project project)
		{
			BuildProjectContext buildContext = new BuildProjectContext(new CultureInfo(MyConfig.LangID),
				new DirectoryInfo(MyConfig.OutputDirectory), MyConfig.CleanIntermediates);

			try {
				OnDocBuildingStep(0, "Initializing...");

				buildContext.Initialize();

				OnDocBuildingStep(10, "Merging XML documentation...");

				// Will hold the name of the file name containing the XML doc
				XmlDocument projectXml = CreateNDocXml(project);
				buildContext.SetProjectXml(projectXml);

				OnDocBuildingStep(30, "Loading XSLT files...");

				buildContext.stylesheets = StyleSheetCollection.LoadStyleSheets(MyConfig.ExtensibilityStylesheet);

				OnDocBuildingStep(40, "Generating HTML pages...");

				// setup for root page
				string defaultTopic;
				string rootPageFileName = null;
				string rootPageTOCName = null;

				if ((MyConfig.RootPageFileName != null) && (MyConfig.RootPageFileName != string.Empty)) {
					rootPageFileName = MyConfig.RootPageFileName;
					defaultTopic = "default.html";

					rootPageTOCName = "Overview";
					// what to call the top page in the table of contents?
					if ((MyConfig.RootPageTOCName != null) && (MyConfig.RootPageTOCName != string.Empty)) {
						rootPageTOCName = MyConfig.RootPageTOCName;
					}
				} else {
					XmlNodeList namespaceNodes = buildContext.SelectNodes("/ndoc:ndoc/ndoc:assembly/ndoc:module/ndoc:namespace");
					int[] indexes = SortNodesByAttribute(namespaceNodes, "name");

					XmlNode defaultNamespace = namespaceNodes[indexes[0]];

					string defaultNamespaceName = GetNodeName(defaultNamespace);
					defaultTopic = defaultNamespaceName + ".html";
				}
				buildContext.htmlHelp = SetupHtmlHelpBuilder(buildContext.WorkingDirectory, defaultTopic);

				using (buildContext.htmlHelp.OpenProjectFile())
				using (buildContext.htmlHelp.OpenContentsFile(string.Empty, true)) {
					// Write the embedded css files to the html output directory
					WriteHtmlContentResources(buildContext);

					GenerateHtmlContentFiles(buildContext, rootPageFileName, rootPageTOCName);
				}

				HtmlHelp htmlHelp = buildContext.htmlHelp;
				htmlHelp.WriteEmptyIndexFile();

				if ((MyConfig.OutputTarget & OutputType.Web) > 0) {
					OnDocBuildingStep(75, "Generating HTML index file...");

					// Write the embedded online templates to the html output directory
					GenerateHtmlIndexFile(buildContext, defaultTopic);
				}

				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0) {
					OnDocBuildingStep(85, "Compiling HTML Help file...");
					htmlHelp.CompileProject();
				}
#if !DEBUG
				else
				{
					//remove .hhc file
					File.Delete(htmlHelp.GetPathToContentsFile());
				}
#endif

				// if we're only building a CHM, copy that to the Outpur dir
				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0 && (MyConfig.OutputTarget & OutputType.Web) == 0) {
					buildContext.SaveOutputs("*.chm");
				} else {
					// otherwise copy everything to the output dir (cause the help file is all the html, not just one chm)
					buildContext.SaveOutputs("*.*");
				}

				OnDocBuildingStep(100, "Done.");
			} catch (Exception ex) {
				throw new DocumenterException(ex.Message, ex);
			} finally {
				buildContext.Dispose();
			}
		}

		private void GenerateHtmlIndexFile(BuildProjectContext ctx, string defaultTopic)
		{
			EmbeddedResources.WriteEmbeddedResources(typeof(OnlineFilesLocationHint), ctx.WorkingDirectory);

			using (TemplateWriter indexWriter = new TemplateWriter(
				Path.Combine(ctx.WorkingDirectory.FullName, "index.html"),
				EmbeddedResources.GetEmbeddedResourceReader(typeof(OnlineTemplatesLocationHint), "index.html", null))) {
				indexWriter.CopyToLine("\t\t<title><%TITLE%></title>");
				indexWriter.WriteLine("\t\t<title>" + MyConfig.HtmlHelpName + "</title>");
				indexWriter.CopyToLine("\t\t<frame name=\"main\" src=\"<%HOME_PAGE%>\" frameborder=\"1\">");
				indexWriter.WriteLine("\t\t<frame name=\"main\" src=\"" + defaultTopic + "\" frameborder=\"1\">");
				indexWriter.CopyToEnd();
				indexWriter.Close();
			}

			Trace.WriteLine("transform the HHC contents file into html");
#if DEBUG
			int start = Environment.TickCount;
#endif
			//transform the HHC contents file into html
			using (StreamReader contentsFile = new StreamReader(ctx.HtmlHelpContentFilePath.FullName, Encoding.Default)) {
				XPathDocument xpathDocument = new XPathDocument(contentsFile);
				using (StreamWriter streamWriter = new StreamWriter(
					File.Open(Path.Combine(ctx.WorkingDirectory.FullName, "contents.html"), FileMode.CreateNew, FileAccess.Write, FileShare.None), Encoding.Default)) {
					XslTransform(ctx, "htmlcontents", xpathDocument, null, streamWriter);
				}
			}
#if DEBUG
			Trace.WriteLine(string.Format("{0} msec.", (Environment.TickCount - start)));
#endif
		}

		private HtmlHelp SetupHtmlHelpBuilder(DirectoryInfo workingDirectory, string defaultTopic)
		{
			HtmlHelp htmlHelp = new HtmlHelp(
				workingDirectory,
				MyConfig.HtmlHelpName,
				defaultTopic,
				((MyConfig.OutputTarget & OutputType.HtmlHelp) == 0));
			htmlHelp.IncludeFavorites = MyConfig.IncludeFavorites;
			htmlHelp.BinaryTOC = MyConfig.BinaryTOC;
			htmlHelp.LangID = MyConfig.LangID;
			return htmlHelp;
		}

		private void GenerateHtmlContentFiles(BuildProjectContext buildContext, string rootPageFileName, string rootPageTOCName)
		{
			if (MyConfig.CopyrightHref != null && MyConfig.CopyrightHref != String.Empty) {
				if (!MyConfig.CopyrightHref.StartsWith("http:")) {
					string copyrightFile = Path.Combine(buildContext.WorkingDirectory.FullName, Path.GetFileName(MyConfig.CopyrightHref));
					File.Copy(MyConfig.CopyrightHref, copyrightFile, true);
					File.SetAttributes(copyrightFile, FileAttributes.Archive);
					buildContext.htmlHelp.AddFileToProject(Path.GetFileName(MyConfig.CopyrightHref));
				}
			}

			// add root page if requested
			if (rootPageFileName != null) {
				if (!File.Exists(rootPageFileName)) {
					throw new DocumenterException("Cannot find the documentation's root page file:\n"
												  + rootPageFileName);
				}

				// add the file
				string rootPageOutputName = Path.Combine(buildContext.WorkingDirectory.FullName, "default.html");
				if (Path.GetFullPath(rootPageFileName) != Path.GetFullPath(rootPageOutputName)) {
					File.Copy(rootPageFileName, rootPageOutputName, true);
					File.SetAttributes(rootPageOutputName, FileAttributes.Archive);
				}
				buildContext.htmlHelp.AddFileToProject(Path.GetFileName(rootPageOutputName));
				buildContext.htmlHelp.AddFileToContents(rootPageTOCName,
										   Path.GetFileName(rootPageOutputName));

				// depending on peer setting, make root page the container
				if (MyConfig.RootPageContainsNamespaces)
					buildContext.htmlHelp.OpenBookInContents();
			}

			MakeHtmlForAssemblies(buildContext);

			// close root book if applicable
			if (rootPageFileName != null) {
				if (MyConfig.RootPageContainsNamespaces)
					buildContext.htmlHelp.CloseBookInContents();
			}
		}

		private XmlDocument CreateNDocXml(Project project)
		{
			string tempFileName = null;
			try {
				// determine temp file name
				tempFileName = Path.GetTempFileName();
				// Let the Documenter base class do it's thing.
				MakeXmlFile(project, new FileInfo(tempFileName));

				// Load the XML documentation into DOM and XPATH doc.
				using (FileStream tempFile = File.Open(tempFileName, FileMode.Open, FileAccess.Read)) {
					//					FilteringXmlTextReader fxtr = new FilteringXmlTextReader(tempFile);

					//					XmlTextReader reader = new XmlTextReader(tempFile);
					//					XPathDocument doc = new XPathDocument(reader, XmlSpace.Preserve);

					XmlDocument xml;
					xml = new XmlDocument();
					xml.Load(tempFile);
					return xml;
					//					tempFile.Seek(0, SeekOrigin.Begin);
					//						XmlTextReader reader = new XmlTextReader(tempFile);
					//						xpathDocument = new XPathDocument(reader, XmlSpace.Preserve);
				}
			} finally {
				if (tempFileName != null && File.Exists(tempFileName)) {
#if DEBUG
					File.Copy(tempFileName, MyConfig.OutputDirectory.TrimEnd('\\', '/') + "\\ndoc.xml", true);
#endif
					File.Delete(tempFileName);
				}
			}
		}

		private void WriteHtmlContentResources(BuildProjectContext buildContext)
		{
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				this.GetType().Namespace + ".css", // TODO
				buildContext.WorkingDirectory);

			// Write the embedded icons to the html output directory
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				this.GetType().Namespace + ".images",
				buildContext.WorkingDirectory);

			// Write the embedded scripts to the html output directory
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				this.GetType().Namespace + ".scripts",
				buildContext.WorkingDirectory);

			if (((string)MyConfig.AdditionalContentResourceDirectory).Length > 0)
				buildContext.CopyToWorkingDirectory(new DirectoryInfo(MyConfig.AdditionalContentResourceDirectory));

			// Write the external files (FilesToInclude) to the html output directory

			foreach (string srcFilePattern in MyConfig.FilesToInclude.Split('|')) {
				if ((srcFilePattern == null) || (srcFilePattern.Length == 0))
					continue;

				string path = Path.GetDirectoryName(srcFilePattern);
				string pattern = Path.GetFileName(srcFilePattern);

				// Path.GetDirectoryName can return null in some cases.
				// Treat this as an empty string.
				if (path == null)
					path = string.Empty;

				// Make sure we have a fully-qualified path name
				if (!Path.IsPathRooted(path))
					path = Path.Combine(Environment.CurrentDirectory, path);

				// Directory.GetFiles does not accept null or empty string
				// for the searchPattern parameter. When no pattern was
				// specified, assume all files (*) are wanted.
				if ((pattern == null) || (pattern.Length == 0))
					pattern = "*";

				foreach (string srcFile in Directory.GetFiles(path, pattern)) {
					string dstFile = Path.Combine(buildContext.WorkingDirectory.FullName, Path.GetFileName(srcFile));
					File.Copy(srcFile, dstFile, true);
					File.SetAttributes(dstFile, FileAttributes.Archive);
				}
			}
		}

		private void XslTransform(BuildProjectContext buildContext, string stylesheetName, IXPathNavigable xpathNavigable, XsltArgumentList arguments, TextWriter writer)
		{
			//Use new overload so we don't get obsolete warnings - clean compile :)
			//			XslCompiledTransform stylesheet = stylesheets[stylesheetName];
			//			stylesheet.Transform(xpathDocument, arguments, writer);
			XslTransform stylesheet = buildContext.stylesheets[stylesheetName];
			try {
				stylesheet.Transform(xpathNavigable, arguments, writer);
			} catch (Exception ex) {
				throw new DocumenterException(string.Format("Error transforming document using stylesheet '{0}': {1}", stylesheetName, ex.Message), ex);
			}
		}

		private MsdnDocumenterConfig MyConfig
		{
			get
			{
				return (MsdnDocumenterConfig)Config;
			}
		}

		private WhichType GetWhichType(XmlNode typeNode)
		{
			WhichType whichType;

			switch (typeNode.Name) {
				case "class":
					whichType = WhichType.Class;
					break;
				case "interface":
					whichType = WhichType.Interface;
					break;
				case "structure":
					whichType = WhichType.Structure;
					break;
				case "enumeration":
					whichType = WhichType.Enumeration;
					break;
				case "delegate":
					whichType = WhichType.Delegate;
					break;
				default:
					whichType = WhichType.Unknown;
					break;
			}

			return whichType;
		}

		private void MakeHtmlForAssemblies(BuildProjectContext ctx)
		{
#if DEBUG
			int start = Environment.TickCount;
#endif

			MakeHtmlForAssembliesSorted(ctx);

#if DEBUG
			Trace.WriteLine("Making Html: " + ((Environment.TickCount - start) / 1000.0).ToString() + " sec.");
#endif
		}

		private void MakeHtmlForAssembliesSorted(BuildProjectContext ctx)
		{
			XmlNodeList assemblyNodes = ctx.SelectNodes("/ndoc:ndoc/ndoc:assembly");
			bool heirTOC = (this.MyConfig.NamespaceTOCStyle == TOCStyle.Hierarchical);
			int level = 0;
			int[] indexes = SortNodesByAttribute(assemblyNodes, "name");

			NameValueCollection namespaceAssemblies = new NameValueCollection();

			int nNodes = assemblyNodes.Count;
			for (int i = 0; i < nNodes; i++) {
				XmlNode assemblyNode = assemblyNodes[indexes[i]];
				if (assemblyNode.ChildNodes.Count > 0) {
					string assemblyName = GetNodeName(assemblyNode);
					GetNamespacesFromAssembly(ctx, assemblyName, namespaceAssemblies);
				}
			}

			string[] namespaces = namespaceAssemblies.AllKeys;
			Array.Sort(namespaces);
			nNodes = namespaces.Length;

			string[] last = new string[0];

			BuildAssemblyContext generatorContext = null; // TODO (EE): initialize w/ assembly name
			for (int i = 0; i < nNodes; i++) {
				OnDocBuildingProgress(i * 100 / nNodes);

				string currentNamespace = namespaces[i];
				// determine assembly containing this namespace
				XmlNodeList namespaceNodes = ctx.SelectNodes(string.Format("/ndoc:ndoc/ndoc:assembly/ndoc:module/ndoc:namespace[@name='{0}']", currentNamespace));
				string assemblyName = GetNodeName(ctx.SelectSingleNode(namespaceNodes[0], "ancestor::ndoc:assembly"));
				generatorContext = new BuildAssemblyContext(ctx, assemblyName);

				if (heirTOC) {
					string[] split = currentNamespace.Split('.');

					for (level = last.Length; level >= 0 &&
						ArrayEquals(split, 0, last, 0, level) == false; level--) {
						if (level > last.Length)
							continue;

						MakeHtmlForTypes(generatorContext, string.Join(".", last, 0, level));
						ctx.htmlHelp.CloseBookInContents();
					}

					if (level < 0)
						level = 0;

					for (; level < split.Length; level++) {
						string namespaceName = string.Join(".", split, 0, level + 1);

						if (Array.BinarySearch(namespaces, namespaceName) < 0)
							MakeHtmlForNamespace(generatorContext, split[level], namespaceName, false);
						else
							MakeHtmlForNamespace(generatorContext, split[level], namespaceName, true);

						ctx.htmlHelp.OpenBookInContents();
					}

					last = split;
				} else {
					MakeHtmlForNamespace(generatorContext, currentNamespace, currentNamespace, true);
					using (ctx.htmlHelp.OpenBookInContents()) {
						MakeHtmlForTypes(generatorContext, currentNamespace);
					}
				}
			}


			if (heirTOC && last.Length > 0) {
				for (; level >= 1; level--) {
					MakeHtmlForTypes(generatorContext, string.Join(".", last, 0, level));
					ctx.htmlHelp.CloseBookInContents();
				}
			}

			OnDocBuildingProgress(100);
		}

		private bool ArrayEquals(string[] array1, int from1, string[] array2, int from2, int count)
		{
			for (int i = 0; i < count; i++) {
				if (array1[from1 + i] != array2[from2 + i])
					return false;
			}

			return true;
		}

		private void GetNamespacesFromAssembly(BuildProjectContext buildContext, string assemblyName, NameValueCollection namespaceAssemblies)
		{
			XmlNodeList namespaceNodes = buildContext.SelectNodes(string.Format("/ndoc:ndoc/ndoc:assembly[@name='{0}']/ndoc:module/ndoc:namespace", assemblyName));
			foreach (XmlNode namespaceNode in namespaceNodes) {
				string namespaceName = GetNodeName(namespaceNode);
				namespaceAssemblies.Add(namespaceName, assemblyName);
			}
		}

		/// <summary>
		/// Add the namespace elements to the output
		/// </summary>
		/// <remarks>
		/// The namespace 
		/// </remarks>
		/// <param name="ctx"></param>
		/// <param name="namespacePart">If nested, the namespace part will be the current
		/// namespace element being documented</param>
		/// <param name="namespaceName">The full namespace name being documented</param>
		/// <param name="addDocumentation">If true, the namespace will be documented, if false
		/// the node in the TOC will not link to a page</param>
		private void MakeHtmlForNamespace(BuildAssemblyContext ctx, string namespacePart, string namespaceName,
			bool addDocumentation)
		{
			//			// handle duplicate namespace documentation
			//			if (ctx.documentedNamespaces.Contains(namespaceName)) 
			//				return;
			//			ctx.documentedNamespaces.Add(namespaceName);

			if (addDocumentation) {
				string fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, "N:" + namespaceName);

				ctx.htmlHelp.AddFileToContents(namespacePart, fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("namespace", String.Empty, namespaceName);

				TransformAndWriteResult(ctx, "namespace", arguments, fileName);

				arguments = new XsltArgumentList();
				arguments.AddParam("namespace", String.Empty, namespaceName);

				TransformAndWriteResult(ctx, "namespacehierarchy", arguments,
										ctx._nameResolver.GetFilenameForNamespaceHierarchy(ctx.CurrentAssemblyName, namespaceName));
			} else {
				ctx.htmlHelp.AddFileToContents(namespacePart);
			}
		}

		private void MakeHtmlForTypes(BuildProjectContext projectCtx, string namespaceName)
		{
			XmlNodeList typeNodes = projectCtx.SelectNodes(string.Format("/ns:ndoc/ns:assembly/ns:module/ns:namespace[@name='{0}']/*[local-name()!='documentation' and local-name()!='typeHierarchy']", namespaceName));

			int[] indexes = SortNodesByAttribute(typeNodes, "id");
			int nNodes = typeNodes.Count;

			for (int i = 0; i < nNodes; i++) {
				XmlNode typeNode = typeNodes[indexes[i]];
				WhichType whichType = GetWhichType(typeNode);

				string assemblyName = XmlUtils.GetNodeName(projectCtx.SelectSingleNode(typeNode, "ancestor::ndoc:assembly"));
				BuildAssemblyContext ctx = new BuildAssemblyContext(projectCtx, assemblyName); // TODO (EE): initialize w/ assembly name

				switch (whichType) {
					case WhichType.Class:
						MakeHtmlForInterfaceOrClassOrStructure(ctx, whichType, typeNode);
						break;
					case WhichType.Interface:
						MakeHtmlForInterfaceOrClassOrStructure(ctx, whichType, typeNode);
						break;
					case WhichType.Structure:
						MakeHtmlForInterfaceOrClassOrStructure(ctx, whichType, typeNode);
						break;
					case WhichType.Enumeration:
						MakeHtmlForEnumerationOrDelegate(ctx, whichType, typeNode);
						break;
					case WhichType.Delegate:
						MakeHtmlForEnumerationOrDelegate(ctx, whichType, typeNode);
						break;
					default:
						break;
				}
			}
		}

		private void MakeHtmlForEnumerationOrDelegate(BuildAssemblyContext ctx, WhichType whichType, XmlNode typeNode)
		{
			string typeName;
			if (whichType == WhichType.Delegate)
				typeName = GetNodeDisplayName(typeNode);
			else
				typeName = GetNodeName(typeNode);
			string typeID = GetNodeId(typeNode);
			string fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, typeID);

			ctx.htmlHelp.AddFileToContents(typeName + " " + mixedCaseTypeNames[whichType], fileName, HtmlHelpIcon.Page);

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("type-id", String.Empty, typeID);
			TransformAndWriteResult(ctx, "type", arguments, fileName);
		}

		private void MakeHtmlForInterfaceOrClassOrStructure(BuildAssemblyContext ctx,
			WhichType whichType,
			XmlNode typeNode)
		{
			string typeName = GetNodeDisplayName(typeNode);
			string typeID = GetNodeId(typeNode);
			string fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, typeID);

			ctx.htmlHelp.AddFileToContents(typeName + " " + mixedCaseTypeNames[whichType], fileName);

			bool hasMembers = ctx.SelectNodes(typeNode, "ndoc:constructor|ndoc:field|ndoc:property|ndoc:method|ndoc:operator|ndoc:event").Count > 0;

			if (hasMembers) {
				ctx.htmlHelp.OpenBookInContents();
			}

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("type-id", String.Empty, typeID);
			TransformAndWriteResult(ctx, "type", arguments, fileName);

			if (ctx.SelectNodes(typeNode, "ndoc:derivedBy").Count > 5) {
				fileName = ctx._nameResolver.GetFilenameForTypeHierarchy(ctx.CurrentAssemblyName, typeID);
				arguments = new XsltArgumentList();
				arguments.AddParam("type-id", String.Empty, typeID);
				TransformAndWriteResult(ctx, "typehierarchy", arguments, fileName);
			}

			if (hasMembers) {
				fileName = ctx._nameResolver.GetFilenameForTypeMemberList(ctx.CurrentAssemblyName, typeID);
				ctx.htmlHelp.AddFileToContents(typeName + " Members",
					fileName,
					HtmlHelpIcon.Page);

				arguments = new XsltArgumentList();
				arguments.AddParam("type-id", String.Empty, typeID);
				TransformAndWriteResult(ctx, "allmembers", arguments, fileName);

				MakeHtmlForConstructors(ctx, whichType, typeNode);
				MakeHtmlForFields(ctx, whichType, typeNode);
				MakeHtmlForProperties(ctx, whichType, typeNode);
				MakeHtmlForMethods(ctx, whichType, typeNode);
				MakeHtmlForOperators(ctx, whichType, typeNode);
				MakeHtmlForEvents(ctx, whichType, typeNode);

				ctx.htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForConstructors(BuildAssemblyContext ctx, WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList constructorNodes;
			string constructorID;
			string typeName;
			string typeID;
			string fileName;

			typeName = GetNodeDisplayName(typeNode);
			typeID = GetNodeId(typeNode);

			constructorNodes = ctx.SelectNodes(typeNode, "ndoc:constructor[@contract!='Static']");
			// If the constructor is overloaded then make an overload page.
			if (constructorNodes.Count > 1) {
				fileName = ctx._nameResolver.GetFilenameForConstructorList(ctx.CurrentAssemblyName, typeID);
				ctx.htmlHelp.AddFileToContents(typeName + " Constructor", fileName);

				ctx.htmlHelp.OpenBookInContents();

				constructorID = constructorNodes[0].Attributes["id"].Value;

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(ctx, "memberoverload", arguments, fileName);
			}

			foreach (XmlNode constructorNode in constructorNodes) {
				constructorID = constructorNode.Attributes["id"].Value;
				fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, constructorID);

				if (constructorNodes.Count > 1) {
					XmlNodeList parameterNodes = ctx.SelectNodes(constructorNode, "ndoc:parameter");
					ctx.htmlHelp.AddFileToContents(typeName + " Constructor " + GetParamList(parameterNodes), fileName,
						HtmlHelpIcon.Page);
				} else {
					ctx.htmlHelp.AddFileToContents(typeName + " Constructor", fileName, HtmlHelpIcon.Page);
				}

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(ctx, "member", arguments, fileName);
			}

			if (constructorNodes.Count > 1) {
				ctx.htmlHelp.CloseBookInContents();
			}

			XmlNode staticConstructorNode = ctx.SelectSingleNode(typeNode, "ndoc:constructor[@contract='Static']");
			if (staticConstructorNode != null) {
				constructorID = staticConstructorNode.Attributes["id"].Value;
				fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, constructorID);

				ctx.htmlHelp.AddFileToContents(typeName + " Static Constructor", fileName, HtmlHelpIcon.Page);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(ctx, "member", arguments, fileName);
			}
		}

		private void MakeHtmlForFields(BuildAssemblyContext ctx, WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList fields = ctx.SelectNodes(typeNode, "ndoc:field[not(@declaringType)]");

			if (fields.Count > 0) {
				//string typeName = typeNode.Attributes["name"].Value;
				string typeID = GetNodeId(typeNode);
				string fileName = ctx._nameResolver.GetFilenameForFieldList(ctx.CurrentAssemblyName, typeID);

				ctx.htmlHelp.AddFileToContents("Fields", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("type-id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "field");
				TransformAndWriteResult(ctx, "individualmembers", arguments, fileName);

				ctx.htmlHelp.OpenBookInContents();

				int[] indexes = SortNodesByAttribute(fields, "id");

				foreach (int index in indexes) {
					XmlNode field = fields[index];

					string fieldName = GetNodeName(field);
					string fieldID = GetNodeId(field);
					fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, fieldID);
					ctx.htmlHelp.AddFileToContents(fieldName + " Field", fileName, HtmlHelpIcon.Page);

					arguments = new XsltArgumentList();
					arguments.AddParam("field-id", String.Empty, fieldID);
					TransformAndWriteResult(ctx, "field", arguments, fileName);
				}

				ctx.htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForProperties(BuildAssemblyContext ctx, WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList declaredPropertyNodes = ctx.SelectNodes(typeNode, "ndoc:property[not(@declaringType)]");

			if (declaredPropertyNodes.Count > 0) {
				XmlNodeList propertyNodes;
				XmlNode propertyNode;
				bool bOverloaded = false;
				string typeID;
				string fileName;
				int nNodes;
				int[] indexes;
				int i;

				typeID = GetNodeId(typeNode);
				propertyNodes = ctx.SelectNodes(typeNode, "ndoc:property[not(@declaringType)]");
				nNodes = propertyNodes.Count;

				indexes = SortNodesByAttribute(propertyNodes, "id");

				fileName = ctx._nameResolver.GetFilenameForPropertyList(ctx.CurrentAssemblyName, typeID);
				ctx.htmlHelp.AddFileToContents("Properties", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("type-id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "property");
				TransformAndWriteResult(ctx, "individualmembers", arguments, fileName);

				ctx.htmlHelp.OpenBookInContents();

				for (i = 0; i < nNodes; i++) {
					propertyNode = propertyNodes[indexes[i]];

					string propertyName = (string)propertyNode.Attributes["name"].Value;
					string propertyID = (string)propertyNode.Attributes["id"].Value;

					// If the method is overloaded then make an overload page.
					string previousPropertyName = ((i - 1 < 0) || (propertyNodes[indexes[i - 1]].Attributes.Count == 0))
													? "" : propertyNodes[indexes[i - 1]].Attributes[0].Value;
					string nextPropertyName = ((i + 1 == nNodes) || (propertyNodes[indexes[i + 1]].Attributes.Count == 0))
												? "" : propertyNodes[indexes[i + 1]].Attributes[0].Value;

					if ((previousPropertyName != propertyName) && (nextPropertyName == propertyName)) {
						fileName = ctx._nameResolver.GetFilenameForPropertyOverloads(ctx.CurrentAssemblyName, typeID, propertyName);
						ctx.htmlHelp.AddFileToContents(propertyName + " Property", fileName);

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, propertyID);
						TransformAndWriteResult(ctx, "memberoverload", arguments, fileName);

						ctx.htmlHelp.OpenBookInContents();

						bOverloaded = true;
					}

					fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, propertyID);

					string pageTitle;
					if (!bOverloaded) {
						pageTitle = string.Format("{0} Property", propertyName);
					} else {
						XmlNodeList parameterNodes = ctx.SelectNodes(propertyNode, "ns:parameter");
						pageTitle = string.Format("{0} Property {1}", propertyName, GetParamList(parameterNodes));
					}
					ctx.htmlHelp.AddFileToContents(pageTitle, fileName, HtmlHelpIcon.Page);

					XsltArgumentList arguments2 = new XsltArgumentList();
					arguments2.AddParam("property-id", String.Empty, propertyID);
					TransformAndWriteResult(ctx, "property", arguments2, fileName);

					if ((previousPropertyName == propertyName) && (nextPropertyName != propertyName)) {
						ctx.htmlHelp.CloseBookInContents();
						bOverloaded = false;
					}
				}

				ctx.htmlHelp.CloseBookInContents();
			}
		}

		private string GetPreviousMethodName(XmlNodeList methodNodes, int[] indexes, int index)
		{
			while (--index >= 0) {
				if (methodNodes[indexes[index]].Attributes["declaringType"] == null)
					return methodNodes[indexes[index]].Attributes["name"].Value;
			}
			return null;
		}

		private string GetNextMethodName(XmlNodeList methodNodes, int[] indexes, int index)
		{
			while (++index < methodNodes.Count) {
				if (methodNodes[indexes[index]].Attributes["declaringType"] == null)
					return methodNodes[indexes[index]].Attributes["name"].Value;
			}
			return null;
		}

		// returns true, if method is neither overload of a method in the same class,
		// nor overload of a method in the base class.
		private bool IsMethodAlone(XmlNodeList methodNodes, int[] indexes, int index)
		{
			string name = methodNodes[indexes[index]].Attributes["name"].Value;
			int lastIndex = methodNodes.Count - 1;
			if (lastIndex <= 0)
				return true;
			bool previousNameDifferent = (index == 0)
				|| (methodNodes[indexes[index - 1]].Attributes["name"].Value != name);
			bool nextNameDifferent = (index == lastIndex)
				|| (methodNodes[indexes[index + 1]].Attributes["name"].Value != name);
			return (previousNameDifferent && nextNameDifferent);
		}

		private bool IsMethodFirstOverload(XmlNodeList methodNodes, int[] indexes, int index)
		{
			if ((methodNodes[indexes[index]].Attributes["declaringType"] != null)
				|| IsMethodAlone(methodNodes, indexes, index))
				return false;

			string name = methodNodes[indexes[index]].Attributes["name"].Value;
			string previousName = GetPreviousMethodName(methodNodes, indexes, index);
			return previousName != name;
		}

		private bool IsMethodLastOverload(XmlNodeList methodNodes, int[] indexes, int index)
		{
			if ((methodNodes[indexes[index]].Attributes["declaringType"] != null)
				|| IsMethodAlone(methodNodes, indexes, index))
				return false;

			string name = methodNodes[indexes[index]].Attributes["name"].Value;
			string nextName = GetNextMethodName(methodNodes, indexes, index);
			return nextName != name;
		}

		private void MakeHtmlForMethods(BuildAssemblyContext ctx, WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList declaredMethodNodes = ctx.SelectNodes(typeNode, "ndoc:method[not(@declaringType)]");

			if (declaredMethodNodes.Count > 0) {
				bool bOverloaded = false;
				string fileName;

				string typeID = GetNodeId(typeNode);
				XmlNodeList methodNodes = ctx.SelectNodes(typeNode, "ndoc:method");
				int nNodes = methodNodes.Count;

				int[] indexes = SortNodesByAttribute(methodNodes, "id");

				fileName = ctx._nameResolver.GetFilenameForMethodList(ctx.CurrentAssemblyName, typeID);
				ctx.htmlHelp.AddFileToContents("Methods", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("type-id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "method");
				TransformAndWriteResult(ctx, "individualmembers", arguments, fileName);

				ctx.htmlHelp.OpenBookInContents();

				for (int i = 0; i < nNodes; i++) {
					XmlNode methodNode = methodNodes[indexes[i]];
					string methodName = GetNodeName(methodNode);
					string methodID = GetNodeId(methodNode);

					if (IsMethodFirstOverload(methodNodes, indexes, i)) {
						bOverloaded = true;

						fileName = ctx._nameResolver.GetFilenameForMethodOverloads(ctx.CurrentAssemblyName, typeID, methodName);
						ctx.htmlHelp.AddFileToContents(methodName + " Method", fileName);

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, methodID);
						TransformAndWriteResult(ctx, "memberoverload", arguments, fileName);

						ctx.htmlHelp.OpenBookInContents();
					}

					if (XmlUtils.GetAttributeString(methodNode, "declaringType", false) == null) {
						fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, methodID);

						string pageTitle;
						if (bOverloaded) {
							XmlNodeList parameterNodes = ctx.SelectNodes(methodNode, "ndoc:parameter");
							pageTitle = methodName + GetParamList(parameterNodes) + " Method ";
						} else {
							pageTitle = methodName + " Method";
						}
						ctx.htmlHelp.AddFileToContents(pageTitle, fileName,
							HtmlHelpIcon.Page);

						XsltArgumentList arguments2 = new XsltArgumentList();
						arguments2.AddParam("member-id", String.Empty, methodID);
						TransformAndWriteResult(ctx, "member", arguments2, fileName);
					}

					if (bOverloaded && IsMethodLastOverload(methodNodes, indexes, i)) {
						bOverloaded = false;
						ctx.htmlHelp.CloseBookInContents();
					}
				}

				ctx.htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForOperators(BuildAssemblyContext ctx, WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList opNodes = ctx.SelectNodes(typeNode, "ndoc:operator");

			if (opNodes.Count == 0)
				return;

			string typeID = GetNodeId(typeNode);
			string fileName = ctx._nameResolver.GetFilenameForOperatorList(ctx.CurrentAssemblyName, typeID);
			bool bOverloaded = false;

			bool bHasOperators =
				(ctx.SelectSingleNode(typeNode, "ndoc:operator[@name != 'op_Explicit' and @name != 'op_Implicit']") != null);
			bool bHasConverters =
				(ctx.SelectSingleNode(typeNode, "ndoc:operator[@name  = 'op_Explicit' or  @name  = 'op_Implicit']") != null);
			string pageTitle = "";

			if (bHasOperators) {
				if (bHasConverters) {
					pageTitle = "Operators and Type Conversions";
				} else {
					pageTitle = "Operators";
				}
			} else {
				if (bHasConverters) {
					pageTitle = "Type Conversions";
				}
			}

			ctx.htmlHelp.AddFileToContents(pageTitle, fileName);

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("type-id", String.Empty, typeID);
			arguments.AddParam("member-type", String.Empty, "operator");
			TransformAndWriteResult(ctx, "individualmembers", arguments, fileName);

			ctx.htmlHelp.OpenBookInContents();

			int[] indexes = SortNodesByAttribute(opNodes, "id");
			int nNodes = opNodes.Count;

			//operators first
			for (int i = 0; i < nNodes; i++) {
				XmlNode operatorNode = opNodes[indexes[i]];

				string operatorID = GetNodeId(operatorNode);
				string opName = GetNodeName(operatorNode);
				if ((opName != "op_Implicit") && (opName != "op_Explicit")) {
					if (IsMethodFirstOverload(opNodes, indexes, i)) {
						bOverloaded = true;

						fileName = ctx._nameResolver.GetFilenameForOperatorOverloads(ctx.CurrentAssemblyName, typeID, opName);
						ctx.htmlHelp.AddFileToContents(GetOperatorDisplayName(ctx, operatorNode), fileName);

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, operatorID);
						TransformAndWriteResult(ctx, "memberoverload", arguments, fileName);

						ctx.htmlHelp.OpenBookInContents();
					}


					fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, operatorID);
					string opPageTitle;
					if (bOverloaded) {
						XmlNodeList parameterNodes = ctx.SelectNodes(operatorNode, "ns:parameter");
						opPageTitle = GetOperatorDisplayName(ctx, operatorNode) + GetParamList(parameterNodes);
					} else {
						opPageTitle = GetOperatorDisplayName(ctx, operatorNode);
					}
					ctx.htmlHelp.AddFileToContents(opPageTitle, fileName,
												   HtmlHelpIcon.Page);

					arguments = new XsltArgumentList();
					arguments.AddParam("member-id", String.Empty, operatorID);
					TransformAndWriteResult(ctx, "member", arguments, fileName);

					if (bOverloaded && IsMethodLastOverload(opNodes, indexes, i)) {
						bOverloaded = false;
						ctx.htmlHelp.CloseBookInContents();
					}
				}
			}

			//type converters
			for (int i = 0; i < nNodes; i++) {
				XmlNode operatorNode = opNodes[indexes[i]];
				string operatorID = GetNodeId(operatorNode);
				string opName = GetNodeName(operatorNode);

				if ((opName == "op_Implicit") || (opName == "op_Explicit")) {
					fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, operatorID);
					ctx.htmlHelp.AddFileToContents(GetOperatorDisplayName(ctx, operatorNode), fileName,
												   HtmlHelpIcon.Page);

					arguments = new XsltArgumentList();
					arguments.AddParam("member-id", String.Empty, operatorID);
					TransformAndWriteResult(ctx, "member", arguments, fileName);
				}
			}

			ctx.htmlHelp.CloseBookInContents();
		}

		private string GetOperatorDisplayName(BuildProjectContext ctx, XmlNode operatorNode)
		{
			string name = GetNodeName(operatorNode);

			switch (name) {
				case "op_Decrement":
					return "Decrement Operator";
				case "op_Increment":
					return "Increment Operator";
				case "op_UnaryNegation":
					return "Unary Negation Operator";
				case "op_UnaryPlus":
					return "Unary Plus Operator";
				case "op_LogicalNot":
					return "Logical Not Operator";
				case "op_True":
					return "True Operator";
				case "op_False":
					return "False Operator";
				case "op_AddressOf":
					return "Address Of Operator";
				case "op_OnesComplement":
					return "Ones Complement Operator";
				case "op_PointerDereference":
					return "Pointer Dereference Operator";
				case "op_Addition":
					return "Addition Operator";
				case "op_Subtraction":
					return "Subtraction Operator";
				case "op_Multiply":
					return "Multiplication Operator";
				case "op_Division":
					return "Division Operator";
				case "op_Modulus":
					return "Modulus Operator";
				case "op_ExclusiveOr":
					return "Exclusive Or Operator";
				case "op_BitwiseAnd":
					return "Bitwise And Operator";
				case "op_BitwiseOr":
					return "Bitwise Or Operator";
				case "op_LogicalAnd":
					return "LogicalAnd Operator";
				case "op_LogicalOr":
					return "Logical Or Operator";
				case "op_Assign":
					return "Assignment Operator";
				case "op_LeftShift":
					return "Left Shift Operator";
				case "op_RightShift":
					return "Right Shift Operator";
				case "op_SignedRightShift":
					return "Signed Right Shift Operator";
				case "op_UnsignedRightShift":
					return "Unsigned Right Shift Operator";
				case "op_Equality":
					return "Equality Operator";
				case "op_GreaterThan":
					return "Greater Than Operator";
				case "op_LessThan":
					return "Less Than Operator";
				case "op_Inequality":
					return "Inequality Operator";
				case "op_GreaterThanOrEqual":
					return "Greater Than Or Equal Operator";
				case "op_LessThanOrEqual":
					return "Less Than Or Equal Operator";
				case "op_UnsignedRightShiftAssignment":
					return "Unsigned Right Shift Assignment Operator";
				case "op_MemberSelection":
					return "Member Selection Operator";
				case "op_RightShiftAssignment":
					return "Right Shift Assignment Operator";
				case "op_MultiplicationAssignment":
					return "Multiplication Assignment Operator";
				case "op_PointerToMemberSelection":
					return "Pointer To Member Selection Operator";
				case "op_SubtractionAssignment":
					return "Subtraction Assignment Operator";
				case "op_ExclusiveOrAssignment":
					return "Exclusive Or Assignment Operator";
				case "op_LeftShiftAssignment":
					return "Left Shift Assignment Operator";
				case "op_ModulusAssignment":
					return "Modulus Assignment Operator";
				case "op_AdditionAssignment":
					return "Addition Assignment Operator";
				case "op_BitwiseAndAssignment":
					return "Bitwise And Assignment Operator";
				case "op_BitwiseOrAssignment":
					return "Bitwise Or Assignment Operator";
				case "op_Comma":
					return "Comma Operator";
				case "op_DivisionAssignment":
					return "Division Assignment Operator";
				case "op_Explicit": {
						XmlNode parameterNode = ctx.SelectSingleNode(operatorNode, "ndoc:parameter");
						string from = GetNodeTypeId(parameterNode);
						string to = GetNodeType(ctx.SelectSingleNode(operatorNode, "ndoc:returnType"));
						return "Explicit " + StripNamespace(from) + " to " + StripNamespace(to) + " Conversion";
					}
				case "op_Implicit": {
						XmlNode parameterNode = ctx.SelectSingleNode(operatorNode, "ndoc:parameter");
						string from = GetNodeTypeId(parameterNode);
						string to = GetNodeType(ctx.SelectSingleNode(operatorNode, "ndoc:returnType"));
						return "Implicit " + StripNamespace(from) + " to " + StripNamespace(to) + " Conversion";
					}
				default:
					return "ERROR";
			}
		}

		private string GetNodeId(XmlNode node)
		{
			return XmlUtils.GetNodeId(node);
		}

		private string GetNodeType(XmlNode node)
		{
			return XmlUtils.GetNodeType(node);
		}

		private string GetNodeTypeId(XmlNode node)
		{
			return XmlUtils.GetNodeTypeId(node);
		}

		private string GetNodeName(XmlNode node)
		{
			return XmlUtils.GetNodeName(node);
		}

		private string GetNodeDisplayName(XmlNode node)
		{
			return XmlUtils.GetNodeDisplayName(node);
		}

		private string StripNamespace(string name)
		{
			string result = name;

			int lastDot = name.LastIndexOf('.');

			if (lastDot != -1) {
				result = name.Substring(lastDot + 1);
			}

			return result;
		}

		private void MakeHtmlForEvents(BuildAssemblyContext ctx, WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList declaredEventNodes = ctx.SelectNodes(typeNode, "ndoc:event[not(@declaringType)]");

			if (declaredEventNodes.Count > 0) {
				XmlNodeList events = ctx.SelectNodes(typeNode, "ns:event");

				if (events.Count > 0) {
					//string typeName = (string)typeNode.Attributes["name"].Value;
					string typeID = GetNodeId(typeNode);
					string fileName = ctx._nameResolver.GetFilenameForEventList(ctx.CurrentAssemblyName, typeID);

					ctx.htmlHelp.AddFileToContents("Events", fileName);

					XsltArgumentList arguments = new XsltArgumentList();
					arguments.AddParam("type-id", String.Empty, typeID);
					arguments.AddParam("member-type", String.Empty, "event");
					TransformAndWriteResult(ctx, "individualmembers", arguments, fileName);

					ctx.htmlHelp.OpenBookInContents();

					int[] indexes = SortNodesByAttribute(events, "id");

					foreach (int index in indexes) {
						XmlNode eventElement = events[index];

						if (XmlUtils.GetAttributeString(eventElement, "declaringType", false) == null) {
							string eventName = GetNodeName(eventElement);
							string eventID = GetNodeId(eventElement);

							fileName = ctx._nameResolver.GetFilenameForId(ctx.CurrentAssemblyName, eventID);
							ctx.htmlHelp.AddFileToContents(eventName + " Event",
								fileName,
								HtmlHelpIcon.Page);

							arguments = new XsltArgumentList();
							arguments.AddParam("event-id", String.Empty, eventID);
							TransformAndWriteResult(ctx, "event", arguments, fileName);
						}
					}

					ctx.htmlHelp.CloseBookInContents();
				}
			}
		}

		private string GetParamList(XmlNodeList parameterNodes)
		{
			int numberOfNodes = parameterNodes.Count;
			int nodeIndex = 1;
			string paramList = "(";

			foreach (XmlNode parameterNode in parameterNodes) {
				XmlAttribute typeAtt = parameterNode.Attributes["typeId"];

				paramList += StripNamespace(typeAtt.Value.Substring(2));

				if (nodeIndex < numberOfNodes) {
					paramList += ", ";
				}

				nodeIndex++;
			}

			paramList += ")";

			return paramList;
		}

		private int[] SortNodesByAttribute(XmlNodeList nodes, string attributeName)
		{
			int length = nodes.Count;
			string[] names = new string[length];
			int[] indexes = new int[length];
			int i = 0;

			foreach (XmlNode node in nodes) {
				names[i] = (string)node.Attributes[attributeName].Value;
				indexes[i] = i++;
			}

			Array.Sort(names, indexes);

			return indexes;
		}

		private void TransformAndWriteResult(BuildAssemblyContext ctx,
			string transformName,
			XsltArgumentList arguments,
			string filename)
		{
			Trace.WriteLine(filename);
#if DEBUG
			int start = Environment.TickCount;
#endif

			ExternalHtmlProvider htmlProvider = new ExternalHtmlProvider(MyConfig, filename);

			try {
				StreamWriter streamWriter;
				using (streamWriter = new StreamWriter(
					File.Open(Path.Combine(ctx.WorkingDirectory.FullName, filename), FileMode.Create),
					ctx.CurrentFileEncoding)) {
					string DocLangCode = Enum.GetName(typeof(SdkLanguage), MyConfig.SdkDocLanguage).Replace("_", "-");
					MsdnXsltUtilities utilities = new MsdnXsltUtilities(ctx._nameResolver, ctx.CurrentAssemblyName, MyConfig.SdkDocVersionString, DocLangCode, MyConfig.SdkLinksOnWeb, ctx.CurrentFileEncoding);

					if (arguments.GetParam("assembly-name", string.Empty) == null) {
						arguments.AddParam("assembly-name", String.Empty, ctx.CurrentAssemblyName);
					}
					arguments.AddParam("ndoc-title", String.Empty, MyConfig.Title);
					arguments.AddParam("ndoc-vb-syntax", String.Empty, MyConfig.ShowVisualBasic);
					arguments.AddParam("ndoc-omit-object-tags", String.Empty, ((MyConfig.OutputTarget & OutputType.HtmlHelp) == 0));
					arguments.AddParam("ndoc-document-attributes", String.Empty, MyConfig.DocumentAttributes);
					arguments.AddParam("ndoc-documented-attributes", String.Empty, MyConfig.DocumentedAttributes);

					arguments.AddParam("ndoc-sdk-doc-base-url", String.Empty, utilities.SdkDocBaseUrl);
					arguments.AddParam("ndoc-sdk-doc-file-ext", String.Empty, utilities.SdkDocExt);
					arguments.AddParam("ndoc-sdk-doc-language", String.Empty, utilities.SdkDocLanguage);

					arguments.AddExtensionObject("urn:NDocUtil", utilities);
					arguments.AddExtensionObject("urn:NDocExternalHtml", htmlProvider);

					//Use new overload so we don't get obsolete warnings - clean compile :)

					XslTransform(ctx, transformName, ctx.GetXPathNavigator(), arguments, streamWriter);
				}
			} catch (PathTooLongException e) {
				throw new PathTooLongException(e.Message + "\nThe file that NDoc3 was trying to create had the following name:\n" + Path.Combine(ctx.WorkingDirectory.FullName, filename));
			}

#if DEBUG
			Debug.WriteLine((Environment.TickCount - start) + " msec.");
#endif
			ctx.htmlHelp.AddFileToProject(filename);
		}

		#region FilteringXmlTextReader (not used atm)

		//		/// <summary>
		//		/// This custom reader is used to load the XmlDocument. It removes elements that are not required *before* 
		//		/// they are loaded into memory, and hence lowers memory requirements significantly.
		//		/// </summary>
		//		private class FilteringXmlTextReader : XmlTextReader
		//		{
		//			readonly object oNamespaceHierarchy;
		//			readonly object oDocumentation;
		//			readonly object oImplements;
		//			readonly object oAttribute;
		//
		//			public FilteringXmlTextReader(Stream file)
		//				: base(file)
		//			{
		//				WhitespaceHandling = WhitespaceHandling.None;
		//				oNamespaceHierarchy = base.NameTable.Add("namespaceHierarchy");
		//				oDocumentation = base.NameTable.Add("documentation");
		//				oImplements = base.NameTable.Add("implements");
		//				oAttribute = base.NameTable.Add("attribute");
		//			}
		//
		//			private bool ShouldSkipElement()
		//			{
		//				return
		//					(
		//						base.Name.Equals(oNamespaceHierarchy) ||
		//						base.Name.Equals(oDocumentation) ||
		//						base.Name.Equals(oImplements) ||
		//						base.Name.Equals(oAttribute)
		//					);
		//			}
		//
		//			public override bool Read()
		//			{
		//				bool notEndOfDoc = base.Read();
		//				if (!notEndOfDoc)
		//					return false;
		//				while (notEndOfDoc && (base.NodeType == XmlNodeType.Element) && ShouldSkipElement()) {
		//					notEndOfDoc = SkipElement(this.Depth);
		//				}
		//				return notEndOfDoc;
		//			}
		//
		//			private bool SkipElement(int startDepth)
		//			{
		//				if (base.IsEmptyElement)
		//					return base.Read();
		//				bool notEndOfDoc = true;
		//				while (notEndOfDoc) {
		//					notEndOfDoc = base.Read();
		//					if ((base.NodeType == XmlNodeType.EndElement) && (this.Depth == startDepth))
		//						break;
		//				}
		//				if (notEndOfDoc)
		//					notEndOfDoc = base.Read();
		//				return notEndOfDoc;
		//			}
		//
		//		}

		#endregion
	}
}
