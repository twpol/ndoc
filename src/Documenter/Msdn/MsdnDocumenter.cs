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
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;

using NDoc.Core;

namespace NDoc.Documenter.Msdn
{
	/// <summary>The MsdnDocumenter class.</summary>
	public class MsdnDocumenter : BaseDocumenter
	{
		enum WhichType
		{
			Class,
			Interface,
			Structure,
			Enumeration,
			Delegate,
			Unknown
		};

		private HtmlHelp htmlHelp;

		private XmlDocument xmlDocumentation;
		private XPathDocument xpathDocument;

		private Hashtable lowerCaseTypeNames;
		private Hashtable mixedCaseTypeNames;
		private StringDictionary fileNames;
		private StringDictionary elemNames;
		private MsdnXsltUtilities utilities;

		private XslTransform xsltNamespace;
		private XslTransform xsltNamespaceHierarchy;
		private XslTransform xsltType;
		private XslTransform xsltAllMembers;
		private XslTransform xsltIndividualMembers;
		private XslTransform xsltEvent;
		private XslTransform xsltMember;
		private XslTransform xsltMemberOverload;
		private XslTransform xsltProperty;
		private XslTransform xsltField;

		private ArrayList documentedNamespaces;

		private Workspace workspace;

		/// <summary>
		/// Initializes a new instance of the <see cref="MsdnDocumenter" />
		/// class.
		/// </summary>
		public MsdnDocumenter() : base("MSDN")
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

			fileNames = new StringDictionary();
			elemNames = new StringDictionary();

			Clear();
		}

		/// <summary>See IDocumenter.</summary>
		public override void Clear()
		{
			Config = new MsdnDocumenterConfig();
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string MainOutputFile 
		{ 
			get 
			{
				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0)
				{
					return Path.Combine(MyConfig.OutputDirectory, 
						MyConfig.HtmlHelpName + ".chm");
				}
				else
				{
					return Path.Combine(MyConfig.OutputDirectory, "index.html");
				}
			} 
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string CanBuild(Project project, bool checkInputOnly)
		{
			string result = base.CanBuild(project, checkInputOnly); 
			if (result != null)
			{
				return result;
			}

			if ( MyConfig.ExtensibilityStylesheet.Length != 0 && !File.Exists( MyConfig.ExtensibilityStylesheet ) )
				return string.Format( "The file {0} could not be found", MyConfig.ExtensibilityStylesheet );

			if (checkInputOnly) 
			{
				return null;
			}

			string path = Path.Combine(MyConfig.OutputDirectory, 
				MyConfig.HtmlHelpName + ".chm");

			string temp = Path.Combine(MyConfig.OutputDirectory, "~chm.tmp");

			try
			{

				if (File.Exists(path))
				{
					//if we can move the file, then it is not open...
					File.Move(path, temp);
					File.Move(temp, path);
				}
			}
			catch (Exception)
			{
				result = "The compiled HTML Help file is probably open.\nPlease close it and try again.";
			}

			return result;
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Build(Project project)
		{
			try
			{
				OnDocBuildingStep(0, "Initializing...");

				// the workspace class is responsible for maintaining the outputdirectory
				// and compile intermediate locations
				workspace = new MsdnWorkspace( Path.GetFullPath( MyConfig.OutputDirectory ) );
				workspace.Clean();
				workspace.Prepare();

				// Define this when you want to edit the stylesheets
				// without having to shutdown the application to rebuild.
#if NO_RESOURCES
				// copy all of the xslt source files into the workspace
				DirectoryInfo xsltSource = new DirectoryInfo( Path.GetFullPath(Path.Combine(
					System.Windows.Forms.Application.StartupPath, @"..\..\..\Documenter\Msdn\xslt") ) );

				foreach ( FileInfo f in xsltSource.GetFiles( "*.xslt" ) )
					f.CopyTo( Path.Combine( workspace.ResourceDirectory, f.Name ), true );
#else
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc.Documenter.Msdn.xslt",
					workspace.ResourceDirectory);
#endif

				// Write the embedded css files to the html output directory
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc.Documenter.Msdn.css",
					workspace.WorkingDirectory);

				// Write the embedded icons to the html output directory
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc.Documenter.Msdn.images",
					workspace.WorkingDirectory);

				// Write the embedded scripts to the html output directory
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc.Documenter.Msdn.scripts",
					workspace.WorkingDirectory);

				// Write the external files (FilesToInclude) to the html output directory

				foreach( string srcFile in MyConfig.FilesToInclude.Split( '|' ) )
				{
					if ((srcFile == null) || (srcFile.Length == 0))
						continue;

					string dstFile = Path.Combine(workspace.WorkingDirectory, Path.GetFileName(srcFile));
					File.Copy(srcFile, dstFile, true);
					File.SetAttributes(dstFile, FileAttributes.Archive);
				}

				OnDocBuildingStep(10, "Merging XML documentation...");

				// Let the Documenter base class do it's thing.
				string tempFileName = MakeXmlFile(project);
				// Load the XML documentation into a DOM.
				xmlDocumentation = new XmlDocument();
				Stream tempFile=null;
				try
				{
					tempFile=File.Open(tempFileName,FileMode.Open,FileAccess.Read);
					xmlDocumentation.Load(tempFile);
					tempFile.Seek(0,SeekOrigin.Begin);
					xpathDocument = new XPathDocument(tempFile);
				}
				finally
				{
					if (tempFile!=null) tempFile.Close();
					if (File.Exists(tempFileName)) File.Delete(tempFileName);
				}

				XmlNodeList typeNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/*[name()!='documentation']");
				if (typeNodes.Count == 0)
				{
					throw new DocumenterException("There are no documentable types in this project.");
				}

				XmlNodeList namespaceNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace");
				int[] indexes = SortNodesByAttribute(namespaceNodes, "name");

				XmlNode defaultNamespace = namespaceNodes[indexes[0]];;

				string defaultNamespaceName = (string)defaultNamespace.Attributes["name"].Value;
				string defaultTopic = defaultNamespaceName + ".html";

				// setup for root page
				string rootPageFileName = null;
				string rootPageTOCName = "Overview";

				if ((MyConfig.RootPageFileName != null) && (MyConfig.RootPageFileName != string.Empty))
				{
					rootPageFileName = MyConfig.RootPageFileName;
					defaultTopic = "default.html";

					// what to call the top page in the table of contents?
					if ((MyConfig.RootPageTOCName != null) && (MyConfig.RootPageTOCName != string.Empty))
					{
						rootPageTOCName = MyConfig.RootPageTOCName;
					}
				}

				htmlHelp = new HtmlHelp(
					workspace.WorkingDirectory,
					MyConfig.HtmlHelpName,
					defaultTopic,
					((MyConfig.OutputTarget & OutputType.HtmlHelp) == 0));

				htmlHelp.IncludeFavorites = MyConfig.IncludeFavorites;
				htmlHelp.BinaryTOC = MyConfig.BinaryTOC;

				OnDocBuildingStep(25, "Building file mapping...");

				MakeFilenames();

				utilities = new MsdnXsltUtilities(fileNames, elemNames, MyConfig.LinkToSdkDocVersion);

				OnDocBuildingStep(30, "Loading XSLT files...");

				MakeTransforms();

				OnDocBuildingStep(40, "Generating HTML pages...");

				htmlHelp.OpenProjectFile();

//				if (!MyConfig.SplitTOCs)
//				{
					htmlHelp.OpenContentsFile(string.Empty, true);
//				}

				try
				{
					if (MyConfig.CopyrightHref != null && MyConfig.CopyrightHref != String.Empty)
					{
						if (!MyConfig.CopyrightHref.StartsWith("http:"))
						{
							string copyrightFile = Path.Combine(workspace.WorkingDirectoryName, Path.GetFileName(MyConfig.CopyrightHref));
							File.Copy(MyConfig.CopyrightHref, copyrightFile, true);
							File.SetAttributes(copyrightFile, FileAttributes.Archive);
							htmlHelp.AddFileToProject(Path.GetFileName(MyConfig.CopyrightHref));
						}
					}

					// add root page if requested
					if (rootPageFileName != null)
					{
						if (!File.Exists(rootPageFileName))
						{
							throw new DocumenterException("Cannot find the documentation's root page file:\n" 
								+ rootPageFileName);
						}

						// add the file
						string rootPageOutputName = Path.Combine(workspace.WorkingDirectory, "default.html");
						if (Path.GetFullPath(rootPageFileName) != Path.GetFullPath(rootPageOutputName))
						{
							File.Copy(rootPageFileName, rootPageOutputName, true);
							File.SetAttributes(rootPageOutputName, FileAttributes.Archive);
						}
						htmlHelp.AddFileToProject(Path.GetFileName(rootPageOutputName));
						htmlHelp.AddFileToContents(rootPageTOCName, 
							Path.GetFileName(rootPageOutputName));

						// depending on peer setting, make root page the container
						if (MyConfig.RootPageContainsNamespaces) htmlHelp.OpenBookInContents();
					}

					documentedNamespaces = new ArrayList();
					MakeHtmlForAssemblies();

					// close root book if applicable
					if (rootPageFileName != null)
					{
						if (MyConfig.RootPageContainsNamespaces) htmlHelp.CloseBookInContents();
					}
				}
				finally
				{
//					if (!MyConfig.SplitTOCs)
//					{
						htmlHelp.CloseContentsFile();
//					}

					htmlHelp.CloseProjectFile();
				}

				htmlHelp.WriteEmptyIndexFile();

				if ((MyConfig.OutputTarget & OutputType.Web) > 0)
				{
					OnDocBuildingStep(75, "Generating HTML content file...");

					// Write the embedded online templates to the html output directory
					EmbeddedResources.WriteEmbeddedResources(
						this.GetType().Module.Assembly,
						"NDoc.Documenter.Msdn.onlinefiles",
						workspace.WorkingDirectory);

					using (TemplateWriter indexWriter = new TemplateWriter(
							   Path.Combine(workspace.WorkingDirectory, "index.html"),
							   new StreamReader(this.GetType().Module.Assembly.GetManifestResourceStream(
							   "NDoc.Documenter.Msdn.onlinetemplates.index.html"))))
					{
						indexWriter.CopyToLine("\t\t<title><%TITLE%></title>");
						indexWriter.WriteLine("\t\t<title>" + MyConfig.HtmlHelpName + "</title>");
						indexWriter.CopyToLine("\t\t<frame name=\"main\" src=\"<%HOME_PAGE%>\" frameborder=\"1\">");
						indexWriter.WriteLine("\t\t<frame name=\"main\" src=\"" + defaultTopic + "\" frameborder=\"1\">");
						indexWriter.CopyToEnd();
						indexWriter.Close();
					}

					//transform the HHC contents file into html
					XslTransform xsltContents = new XslTransform();
					MakeTransform(xsltContents, "htmlcontents.xslt");
					xsltContents.Transform(htmlHelp.GetPathToContentsFile(), 
						Path.Combine(workspace.WorkingDirectory, "contents.html"));
				}

				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0)
				{
					OnDocBuildingStep(85, "Compiling HTML Help file...");
					htmlHelp.CompileProject();
				}
				else
				{
					//remove .hhc file
					File.Delete(htmlHelp.GetPathToContentsFile());
				}

				// if we're only building a CHM, copy that to the Outpur dir
				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0 && (MyConfig.OutputTarget & OutputType.Web) == 0) {
					workspace.SaveOutputs( "*.chm" );
				} else {
					// otherwise copy everything to the output dir (cause the help file is all the html, not just one chm)
					workspace.SaveOutputs( "*.*" );
				}
				
				if ( MyConfig.CleanIntermediates )
					workspace.CleanIntermediates();
				
				OnDocBuildingStep(100, "Done.");
			}
			catch(Exception ex)
			{
				throw new DocumenterException(ex.Message, ex);
			}
			finally
			{
				xmlDocumentation = null;
				workspace.RemoveResourceDirectory();
			}
		}

		private MsdnDocumenterConfig MyConfig
		{
			get
			{
				return (MsdnDocumenterConfig)Config;
			}
		}

		private void MakeTransform(
			XslTransform transform,
			string filename)
		{
			try
			{
				transform.Load(Path.Combine( this.workspace.ResourceDirectory, filename));
			}
			catch (Exception e)
			{
				throw new DocumenterException(
					"Error compiling the " +
					filename +
					" stylesheet: \n" + e.Message,
					e);
			}
		}

		private void MakeFilenames()
		{
			XmlNodeList namespaces = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace");
			foreach (XmlElement namespaceNode in namespaces)
			{
				string namespaceName = namespaceNode.Attributes["name"].Value;
				string namespaceId = "N:" + namespaceName;
				fileNames[namespaceId] = GetFilenameForNamespace(namespaceName);
				elemNames[namespaceId] = namespaceName;

				XmlNodeList types = namespaceNode.SelectNodes("*[@id]");
				foreach (XmlElement typeNode in types)
				{
					string typeId = typeNode.Attributes["id"].Value;
					fileNames[typeId] = GetFilenameForType(typeNode);
					elemNames[typeId] = typeNode.Attributes["name"].Value;

					XmlNodeList members = typeNode.SelectNodes("*[@id]");
					foreach (XmlElement memberNode in members)
					{
						string id = memberNode.Attributes["id"].Value;
						switch (memberNode.Name)
						{
							case "constructor":
								fileNames[id] = GetFilenameForConstructor(memberNode);
								elemNames[id] = elemNames[typeId];
								break;
							case "field":
								if (typeNode.Name == "enumeration")
									fileNames[id] = GetFilenameForType(typeNode);
								else
									fileNames[id] = GetFilenameForField(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "property":
								fileNames[id] = GetFilenameForProperty(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "method":
								fileNames[id] = GetFilenameForMethod(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "operator":
								fileNames[id] = GetFilenameForOperator(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "event":
								fileNames[id] = GetFilenameForEvent(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Addes the ExtensibilityStylesheet to the tags.xslt stylesheet
		/// so that custom templates will get called during processing
		/// </summary>
		private void AddExtensibilityStylesheet()
		{
			XmlDocument tags = new XmlDocument();
			tags.Load( Path.Combine( this.workspace.ResourceDirectory, "tags.xslt" ) );
			
			XmlElement include = tags.CreateElement( "xsl", "include", "http://www.w3.org/1999/XSL/Transform" );

			string extensibilityStylesheet = MyConfig.ExtensibilityStylesheet;

			// make relative paths absolute
			if ( !Path.IsPathRooted( extensibilityStylesheet ) )
				extensibilityStylesheet = Path.GetFullPath( extensibilityStylesheet );

			include.SetAttribute( "href", extensibilityStylesheet );

			tags.DocumentElement.PrependChild( include );

			tags.Save( Path.Combine( this.workspace.ResourceDirectory, "tags.xslt" ) );
		}

		private void MakeTransforms()
		{
			OnDocBuildingProgress(0);
			Trace.Indent();

			xsltNamespace = new XslTransform();
			xsltNamespaceHierarchy = new XslTransform();
			xsltType = new XslTransform();
			xsltAllMembers = new XslTransform();
			xsltIndividualMembers = new XslTransform();
			xsltEvent = new XslTransform();
			xsltMember = new XslTransform();
			xsltMemberOverload = new XslTransform();
			xsltProperty = new XslTransform();
			xsltField = new XslTransform();

			// if we have an extensibility stylesheet, add it before compiling the transforms
			if ( MyConfig.ExtensibilityStylesheet.Length > 0 )
				AddExtensibilityStylesheet();

			Trace.WriteLine("namespace.xslt");
			OnDocBuildingProgress(10);
			MakeTransform(
				xsltNamespace,
				"namespace.xslt");

			Trace.WriteLine("namespacehierarchy.xslt");
			OnDocBuildingProgress(20);
			MakeTransform(
				xsltNamespaceHierarchy,
				"namespacehierarchy.xslt");

			Trace.WriteLine("type.xslt");
			OnDocBuildingProgress(30);
			MakeTransform(
				xsltType,
				"type.xslt");

			Trace.WriteLine("allmembers.xslt");
			OnDocBuildingProgress(40);
			MakeTransform(
				xsltAllMembers,
				"allmembers.xslt");

			Trace.WriteLine("individualmembers.xslt");
			OnDocBuildingProgress(50);
			MakeTransform(
				xsltIndividualMembers,
				"individualmembers.xslt");

			Trace.WriteLine("event.xslt");
			OnDocBuildingProgress(60);
			MakeTransform(
				xsltEvent,
				"event.xslt");

			Trace.WriteLine("member.xslt");
			OnDocBuildingProgress(70);
			MakeTransform(
				xsltMember,
				"member.xslt");

			Trace.WriteLine("memberoverload.xslt");
			OnDocBuildingProgress(80);
			MakeTransform(
				xsltMemberOverload,
				"memberoverload.xslt");

			Trace.WriteLine("property.xslt");
			OnDocBuildingProgress(90);
			MakeTransform(
				xsltProperty,
				"property.xslt");

			Trace.WriteLine("field.xslt");
			OnDocBuildingProgress(100);
			MakeTransform(
				xsltField,
				"field.xslt");

			Trace.Unindent();
		}

		private WhichType GetWhichType(XmlNode typeNode)
		{
			WhichType whichType;

			switch (typeNode.Name)
			{
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

		private void MakeHtmlForAssemblies()
		{
#if DEBUG
			int start = Environment.TickCount;
#endif

//			if (MyConfig.SortTOCByNamespace)
				MakeHtmlForAssembliesSorted();
//			else
//				MakeHtmlForAssembliesUnsorted();

#if DEBUG
			Trace.WriteLine("Making Html: " + ((Environment.TickCount - start)/1000.0).ToString() + " sec.");
#endif
		}

//		private void MakeHtmlForAssembliesUnsorted()
//		{
//			XmlNodeList assemblyNodes = xmlDocumentation.SelectNodes("/ndoc/assembly");
//			int[] indexes = SortNodesByAttribute(assemblyNodes, "name");
//
//			int nNodes = assemblyNodes.Count;
//
//			for (int i = 0; i < nNodes; i++)
//			{
//				XmlNode assemblyNode = assemblyNodes[indexes[i]];
//
//				if (assemblyNode.ChildNodes.Count > 0)
//				{
//					string assemblyName = (string)assemblyNode.Attributes["name"].Value;
//
//					if (MyConfig.SplitTOCs)
//					{
//						bool isDefault = (assemblyName == MyConfig.DefaultTOC);
//
//						if (isDefault)
//						{
//							XmlNode defaultNamespace =
//								xmlDocumentation.SelectSingleNode("/ndoc/assembly[@name='" 
//								+ assemblyName + "']/module/namespace");
//
//							if (defaultNamespace != null)
//							{
//								string defaultNamespaceName = (string)defaultNamespace.Attributes["name"].Value;
//								htmlHelp.DefaultTopic = defaultNamespaceName + ".html";
//							}
//						}
//
//						htmlHelp.OpenContentsFile(assemblyName, isDefault);
//					}
//
//					try
//					{
//						MakeHtmlForNamespaces(assemblyName);
//					}
//					finally
//					{
//						if (MyConfig.SplitTOCs)
//						{
//							htmlHelp.CloseContentsFile();
//						}
//					}
//				}
//			}
//		}
//
		private void MakeHtmlForNamespaces(string assemblyName)
		{
			XmlNodeList namespaceNodes = xmlDocumentation.SelectNodes("/ndoc/assembly[@name=\"" + assemblyName + "\"]/module/namespace");
			int[] indexes = SortNodesByAttribute(namespaceNodes, "name");

			int nNodes = namespaceNodes.Count;

			for (int i = 0; i < nNodes; i++)
			{
				OnDocBuildingProgress(i*100/nNodes);

				XmlNode namespaceNode = namespaceNodes[indexes[i]];

				if (namespaceNode.ChildNodes.Count > 0)
				{
					string namespaceName = (string)namespaceNode.Attributes["name"].Value;

					MakeHtmlForNamespace(assemblyName, namespaceName);
				}
			}

			OnDocBuildingProgress(100);
		}

		private void MakeHtmlForAssembliesSorted()
		{
			XmlNodeList assemblyNodes = xmlDocumentation.SelectNodes("/ndoc/assembly");
			int[] indexes = SortNodesByAttribute(assemblyNodes, "name");

			System.Collections.Specialized.NameValueCollection namespaceAssemblies
				= new System.Collections.Specialized.NameValueCollection();

			int nNodes = assemblyNodes.Count;
			for (int i = 0; i < nNodes; i++)
			{
				XmlNode assemblyNode = assemblyNodes[indexes[i]];
				if (assemblyNode.ChildNodes.Count > 0)
				{
					string assemblyName = (string)assemblyNode.Attributes["name"].Value;
					GetNamespacesFromAssembly(assemblyName, namespaceAssemblies);
				}
			}

			string [] namespaces = namespaceAssemblies.AllKeys;
			Array.Sort(namespaces);
			nNodes = namespaces.Length;
			for (int i = 0; i < nNodes; i++)
			{
				OnDocBuildingProgress(i*100/nNodes);
				string namespaceName = namespaces[i];
				foreach (string assemblyName in namespaceAssemblies.GetValues(namespaceName))
					MakeHtmlForNamespace(assemblyName, namespaceName);
			}

			OnDocBuildingProgress(100);
		}

		private void GetNamespacesFromAssembly(string assemblyName, System.Collections.Specialized.NameValueCollection namespaceAssemblies)
		{
			XmlNodeList namespaceNodes = xmlDocumentation.SelectNodes("/ndoc/assembly[@name=\"" + assemblyName + "\"]/module/namespace");
			foreach (XmlNode namespaceNode in namespaceNodes)
			{
				string namespaceName = (string)namespaceNode.Attributes["name"].Value;
				namespaceAssemblies.Add(namespaceName, assemblyName);
			}
		}

		private void MakeHtmlForNamespace(string assemblyName, string namespaceName)
		{
			if (documentedNamespaces.Contains(namespaceName)) 
				return;

			documentedNamespaces.Add(namespaceName);

			string fileName = GetFilenameForNamespace(namespaceName);
			htmlHelp.AddFileToContents(namespaceName, fileName);

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("namespace", String.Empty, namespaceName);
			arguments.AddParam("includeHierarchy", String.Empty, MyConfig.IncludeHierarchy);

			TransformAndWriteResult(xsltNamespace, arguments, fileName);

			arguments = new XsltArgumentList();
			arguments.AddParam("namespace", String.Empty, namespaceName);

			if (MyConfig.IncludeHierarchy)
			{
				TransformAndWriteResult(
					xsltNamespaceHierarchy,
					arguments,
					fileName.Insert(fileName.Length - 5, "Hierarchy"));
			}

			MakeHtmlForTypes(namespaceName);
		}

		private void MakeHtmlForTypes(string namespaceName)
		{
			XmlNodeList typeNodes =
				xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace[@name=\"" + namespaceName + "\"]/*[local-name()!='documentation']");

			int[] indexes = SortNodesByAttribute(typeNodes, "id");
			int nNodes = typeNodes.Count;

			htmlHelp.OpenBookInContents();

			for (int i = 0; i < nNodes; i++)
			{
				XmlNode typeNode = typeNodes[indexes[i]];

				WhichType whichType = GetWhichType(typeNode);

				switch(whichType)
				{
					case WhichType.Class:
						MakeHtmlForInterfaceOrClassOrStructure(whichType, typeNode);
						break;
					case WhichType.Interface:
						MakeHtmlForInterfaceOrClassOrStructure(whichType, typeNode);
						break;
					case WhichType.Structure:
						MakeHtmlForInterfaceOrClassOrStructure(whichType, typeNode);
						break;
					case WhichType.Enumeration:
						MakeHtmlForEnumerationOrDelegate(whichType, typeNode);
						break;
					case WhichType.Delegate:
						MakeHtmlForEnumerationOrDelegate(whichType, typeNode);
						break;
					default:
						break;
				}
			}

			htmlHelp.CloseBookInContents();
		}

		private void MakeHtmlForEnumerationOrDelegate(WhichType whichType, XmlNode typeNode)
		{
			string typeName = typeNode.Attributes["name"].Value;
			string typeID = typeNode.Attributes["id"].Value;
			string fileName = GetFilenameForType(typeNode);

			htmlHelp.AddFileToContents(typeName + " " + mixedCaseTypeNames[whichType], fileName, HtmlHelpIcon.Page );

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("type-id", String.Empty, typeID);
			TransformAndWriteResult(xsltType, arguments, fileName);
		}

		private void MakeHtmlForInterfaceOrClassOrStructure(
			WhichType whichType,
			XmlNode typeNode)
		{
			string typeName = typeNode.Attributes["name"].Value;
			string typeID = typeNode.Attributes["id"].Value;
			string fileName = GetFilenameForType(typeNode);

			htmlHelp.AddFileToContents(typeName + " " + mixedCaseTypeNames[whichType], fileName);

			bool hasMembers = typeNode.SelectNodes("constructor|field|property|method|operator|event").Count > 0;

			if (hasMembers)
			{
				htmlHelp.OpenBookInContents();
			}

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("type-id", String.Empty, typeID);
			TransformAndWriteResult(xsltType, arguments, fileName);

			if (hasMembers)
			{
				fileName = GetFilenameForTypeMembers(typeNode);
				htmlHelp.AddFileToContents(typeName + " Members", 
					fileName, 
					HtmlHelpIcon.Page);

				arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				TransformAndWriteResult(xsltAllMembers, arguments, fileName);

				MakeHtmlForConstructors(whichType, typeNode);
				MakeHtmlForFields(whichType, typeNode);
				MakeHtmlForProperties(whichType, typeNode);
				MakeHtmlForMethods(whichType, typeNode);
				MakeHtmlForOperators(whichType, typeNode);
				MakeHtmlForEvents(whichType, typeNode);

				htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForConstructors(WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList   constructorNodes;
			string        constructorID;
			string        typeName;
			string        typeID;
			string        fileName;

			typeName = typeNode.Attributes["name"].Value;
			typeID = typeNode.Attributes["id"].Value;
			constructorNodes = typeNode.SelectNodes("constructor[@contract!='Static']");

			// If the constructor is overloaded then make an overload page.
			if (constructorNodes.Count > 1)
			{
				fileName = GetFilenameForConstructors(typeNode);
				htmlHelp.AddFileToContents(typeName + " Constructor", fileName);

				htmlHelp.OpenBookInContents();

				constructorID = constructorNodes[0].Attributes["id"].Value;

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(xsltMemberOverload, arguments, fileName);
			}

			foreach (XmlNode constructorNode in constructorNodes)
			{
				constructorID = constructorNode.Attributes["id"].Value;
				fileName = GetFilenameForConstructor(constructorNode);

				if (constructorNodes.Count > 1)
				{
					XmlNodeList   parameterNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/constructor[@id=\"" + constructorID + "\"]/parameter");
					htmlHelp.AddFileToContents(typeName + " Constructor " + GetParamList(parameterNodes), fileName,
						HtmlHelpIcon.Page );
				}
				else
				{
					htmlHelp.AddFileToContents(typeName + " Constructor", fileName, HtmlHelpIcon.Page );
				}

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(xsltMember, arguments, fileName);
			}

			if (constructorNodes.Count > 1)
			{
				htmlHelp.CloseBookInContents();
			}

			XmlNode staticConstructorNode = typeNode.SelectSingleNode("constructor[@contract='Static']");
			if (staticConstructorNode != null)
			{
				constructorID = staticConstructorNode.Attributes["id"].Value;
				fileName = GetFilenameForConstructor(staticConstructorNode);

				htmlHelp.AddFileToContents(typeName + " Static Constructor", fileName, HtmlHelpIcon.Page);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(xsltMember, arguments, fileName);
			}
		}

		private void MakeHtmlForFields(WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList fields = typeNode.SelectNodes("field[not(@declaringType)]");

			if (fields.Count > 0)
			{
				string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;
				string fileName = GetFilenameForFields(whichType, typeNode);

				htmlHelp.AddFileToContents("Fields", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "field");
				TransformAndWriteResult(xsltIndividualMembers, arguments, fileName);

				htmlHelp.OpenBookInContents();

				int[] indexes = SortNodesByAttribute(fields, "id");

				foreach (int index in indexes)
				{
					XmlNode field = fields[index];

					string fieldName = field.Attributes["name"].Value;
					string fieldID = field.Attributes["id"].Value;
					fileName = GetFilenameForField(field);
					htmlHelp.AddFileToContents(fieldName + " Field", fileName, HtmlHelpIcon.Page );

					arguments = new XsltArgumentList();
					arguments.AddParam("field-id", String.Empty, fieldID);
					TransformAndWriteResult(xsltField, arguments, fileName);
				}

				htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForProperties(WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList declaredPropertyNodes = typeNode.SelectNodes("property[not(@declaringType)]");

			if (declaredPropertyNodes.Count > 0)
			{
				XmlNodeList   propertyNodes;
				XmlNode     propertyNode;
				string        propertyName;
				string        propertyID;
				string        previousPropertyName;
				string        nextPropertyName;
				bool        bOverloaded = false;
				string        typeName;
				string        typeID;
				string        fileName;
				int         nNodes;
				int[]       indexes;
				int         i;

				typeName = typeNode.Attributes["name"].Value;
				typeID = typeNode.Attributes["id"].Value;
				propertyNodes = typeNode.SelectNodes("property[not(@declaringType)]");
				nNodes = propertyNodes.Count;

				indexes = SortNodesByAttribute(propertyNodes, "id");

				fileName = GetFilenameForProperties(whichType, typeNode);
				htmlHelp.AddFileToContents("Properties", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "property");
				TransformAndWriteResult(xsltIndividualMembers, arguments, fileName);

				htmlHelp.OpenBookInContents();

				for (i = 0; i < nNodes; i++)
				{
					propertyNode = propertyNodes[indexes[i]];

					propertyName = (string)propertyNode.Attributes["name"].Value;
					propertyID = (string)propertyNode.Attributes["id"].Value;

					// If the method is overloaded then make an overload page.
					previousPropertyName = ((i - 1 < 0) || (propertyNodes[indexes[i - 1]].Attributes.Count == 0))
						? "" : propertyNodes[indexes[i - 1]].Attributes[0].Value;
					nextPropertyName = ((i + 1 == nNodes) || (propertyNodes[indexes[i + 1]].Attributes.Count == 0))
						? "" : propertyNodes[indexes[i + 1]].Attributes[0].Value;

					if ((previousPropertyName != propertyName) && (nextPropertyName == propertyName))
					{
						fileName = GetFilenameForPropertyOverloads(typeNode, propertyNode);
						htmlHelp.AddFileToContents(propertyName + " Property", fileName);

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, propertyID);
						TransformAndWriteResult(xsltMemberOverload, arguments, fileName);

						htmlHelp.OpenBookInContents();

						bOverloaded = true;
					}

					fileName = GetFilenameForProperty(propertyNode);

					if (bOverloaded)
					{
						XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/property[@id=\"" + propertyID + "\"]/parameter");
						htmlHelp.AddFileToContents(propertyName + " Property " + GetParamList(parameterNodes), fileName,
							HtmlHelpIcon.Page );
					}
					else
					{
						htmlHelp.AddFileToContents(propertyName + " Property", fileName, 
							HtmlHelpIcon.Page );
					}

					XsltArgumentList arguments2 = new XsltArgumentList();
					arguments2.AddParam("property-id", String.Empty, propertyID);
					TransformAndWriteResult(xsltProperty, arguments2, fileName);

					if ((previousPropertyName == propertyName) && (nextPropertyName != propertyName))
					{
						htmlHelp.CloseBookInContents();
						bOverloaded = false;
					}
				}

				htmlHelp.CloseBookInContents();
			}
		}

		private string GetPreviousMethodName(XmlNodeList methodNodes, int[] indexes, int index)
		{
			while (--index >= 0)
			{
				if (methodNodes[indexes[index]].Attributes["declaringType"] == null)
					return methodNodes[indexes[index]].Attributes["name"].Value;
			}
			return null;
		}

		private string GetNextMethodName(XmlNodeList methodNodes, int[] indexes, int index)
		{
			while (++index < methodNodes.Count)
			{
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

			string name			= methodNodes[indexes[index]].Attributes["name"].Value;
			string previousName	= GetPreviousMethodName(methodNodes, indexes, index);
			return previousName != name;
		}

		private bool IsMethodLastOverload(XmlNodeList methodNodes, int[] indexes, int index)
		{
			if ((methodNodes[indexes[index]].Attributes["declaringType"] != null)
				|| IsMethodAlone(methodNodes, indexes, index))
				return false;

			string name		= methodNodes[indexes[index]].Attributes["name"].Value;
			string nextName	= GetNextMethodName(methodNodes, indexes, index);
			return nextName != name;
		}

		private void MakeHtmlForMethods(WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList declaredMethodNodes = typeNode.SelectNodes("method[not(@declaringType)]");

			if (declaredMethodNodes.Count > 0)
			{
				bool bOverloaded = false;
				string fileName;

				string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;
				XmlNodeList methodNodes = typeNode.SelectNodes("method");
				int nNodes = methodNodes.Count;

				int[] indexes = SortNodesByAttribute(methodNodes, "id");

				fileName = GetFilenameForMethods(whichType, typeNode);
				htmlHelp.AddFileToContents("Methods", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "method");
				TransformAndWriteResult(xsltIndividualMembers, arguments, fileName);

				htmlHelp.OpenBookInContents();

				for (int i = 0; i < nNodes; i++)
				{
					XmlNode methodNode = methodNodes[indexes[i]];
					string methodName = (string)methodNode.Attributes["name"].Value;
					string methodID = (string)methodNode.Attributes["id"].Value;

					if (IsMethodFirstOverload(methodNodes, indexes, i))
					{
						bOverloaded = true;

						fileName = GetFilenameForMethodOverloads(typeNode, methodNode);
						htmlHelp.AddFileToContents(methodName + " Method", fileName);

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, methodID);
						TransformAndWriteResult(xsltMemberOverload, arguments, fileName);

						htmlHelp.OpenBookInContents();
					}

					if (methodNode.Attributes["declaringType"] == null)
					{
						fileName = GetFilenameForMethod(methodNode);

						if (bOverloaded)
						{
							XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/method[@id=\"" + methodID + "\"]/parameter");
							htmlHelp.AddFileToContents(methodName + " Method " + GetParamList(parameterNodes), fileName,
								HtmlHelpIcon.Page );
						}
						else
						{
							htmlHelp.AddFileToContents(methodName + " Method", fileName,
								HtmlHelpIcon.Page );
						}

						XsltArgumentList arguments2 = new XsltArgumentList();
						arguments2.AddParam("member-id", String.Empty, methodID);
						TransformAndWriteResult(xsltMember, arguments2, fileName);
					}

					if (bOverloaded && IsMethodLastOverload(methodNodes, indexes, i))
					{
						bOverloaded = false;
						htmlHelp.CloseBookInContents();
					}
				}

				htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForOperators(WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList operators = typeNode.SelectNodes("operator");

			if (operators.Count > 0)
			{
				string typeName = (string)typeNode.Attributes["name"].Value;
				string typeID = (string)typeNode.Attributes["id"].Value;
				XmlNodeList opNodes = typeNode.SelectNodes("operator");
				string fileName = GetFilenameForOperators(whichType, typeNode);
				bool bOverloaded = false;

				bool bHasOperators =  (typeNode.SelectSingleNode("operator[@name != 'op_Explicit' and @name != 'op_Implicit']") != null);;
				bool bHasConverters = (typeNode.SelectSingleNode("operator[@name  = 'op_Explicit' or  @name  = 'op_Implicit']") != null);
				string title="";

				if (bHasOperators)
				{
					if (bHasConverters)
					{
						title = "Operators and Type Conversions";
					}
					else
					{
						title = "Operators";
					}
				}
				else
				{
					if (bHasConverters)
					{
						title = "Type Conversions";
					}
				}

				htmlHelp.AddFileToContents(title, fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "operator");
				TransformAndWriteResult(xsltIndividualMembers, arguments, fileName);

				htmlHelp.OpenBookInContents();

				int[] indexes = SortNodesByAttribute(operators, "id");
				int nNodes = opNodes.Count;

				//operators first
				for (int i = 0; i < nNodes; i++)
				{
					XmlNode operatorNode = operators[indexes[i]];
					string operatorID = operatorNode.Attributes["id"].Value;

					string opName = (string)operatorNode.Attributes["name"].Value;
					if ((opName != "op_Implicit") && (opName != "op_Explicit"))
					{
						if (IsMethodFirstOverload(opNodes, indexes, i))
						{
							bOverloaded = true;

							fileName = GetFilenameForOperatorsOverloads(typeNode, operatorNode);
							htmlHelp.AddFileToContents(GetOperatorName(operatorNode), fileName);

							arguments = new XsltArgumentList();
							arguments.AddParam("member-id", String.Empty, operatorID);
							TransformAndWriteResult(xsltMemberOverload, arguments, fileName);

							htmlHelp.OpenBookInContents();
						}


						fileName = GetFilenameForOperator(operatorNode);
						if (bOverloaded)
						{
							XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/operator[@id=\"" + operatorID + "\"]/parameter");
							htmlHelp.AddFileToContents(GetOperatorName(operatorNode) + GetParamList(parameterNodes), fileName, 
								HtmlHelpIcon.Page);
						}
						else
						{
							htmlHelp.AddFileToContents(GetOperatorName(operatorNode), fileName, 
								HtmlHelpIcon.Page );
						}

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, operatorID);
						TransformAndWriteResult(xsltMember, arguments, fileName);

						if (bOverloaded && IsMethodLastOverload(opNodes, indexes, i))
						{
							bOverloaded = false;
							htmlHelp.CloseBookInContents();
						}
					}
				}

				//type converters
				for (int i = 0; i < nNodes; i++)
				{
					XmlNode operatorNode = operators[indexes[i]];
					string operatorID = operatorNode.Attributes["id"].Value;

					string opName = (string)operatorNode.Attributes["name"].Value;
					if ((opName == "op_Implicit") || (opName == "op_Explicit"))
					{
						fileName = GetFilenameForOperator(operatorNode);
						htmlHelp.AddFileToContents(GetOperatorName(operatorNode), fileName, 
							HtmlHelpIcon.Page );

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, operatorID);
						TransformAndWriteResult(xsltMember, arguments, fileName);

					}
				}

				htmlHelp.CloseBookInContents();
			}
		}

		private string GetOperatorName(XmlNode operatorNode)
		{
			string name = operatorNode.Attributes["name"].Value;

			switch (name)
			{
				case "op_UnaryPlus":
					return "Unary Plus Operator";
				case "op_UnaryNegation":
					return "Unary Negation Operator";
				case "op_LogicalNot":
					return "Logical Not Operator";
				case "op_OnesComplement":
					return "Ones Complement Operator";
				case "op_Increment":
					return "Increment Operator";
				case "op_Decrement":
					return "Decrement Operator";
				case "op_True":
					return "True Operator";
				case "op_False":
					return "False Operator";
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
				case "op_BitwiseAnd":
					return "Bitwise And Operator";
				case "op_BitwiseOr":
					return "Bitwise Or Operator";
				case "op_ExclusiveOr":
					return "Exclusive Or Operator";
				case "op_LeftShift":
					return "Left Shift Operator";
				case "op_RightShift":
					return "Right Shift Operator";
				case "op_Equality":
					return "Equality Operator";
				case "op_Inequality":
					return "Inequality Operator";
				case "op_LessThan":
					return "Less Than Operator";
				case "op_GreaterThan":
					return "Greater Than Operator";
				case "op_LessThanOrEqual":
					return "Less Than Or Equal Operator";
				case "op_GreaterThanOrEqual":
					return "Greater Than Or Equal Operator";
				case "op_Explicit":
					XmlNode parameterNode = operatorNode.SelectSingleNode("parameter");
					string from = parameterNode.Attributes["type"].Value;
					string to = operatorNode.Attributes["returnType"].Value;
					return "Explicit " + StripNamespace(from) + " to " + StripNamespace(to) + " Conversion";
				case "op_Implicit":
					XmlNode parameterNode2 = operatorNode.SelectSingleNode("parameter");
					string from2 = parameterNode2.Attributes["type"].Value;
					string to2 = operatorNode.Attributes["returnType"].Value;
					return "Implicit " + StripNamespace(from2) + " to " + StripNamespace(to2) + " Conversion";
				default:
					return "ERROR";
			}
		}

		private string StripNamespace(string name)
		{
			string result = name;

			int lastDot = name.LastIndexOf('.');

			if (lastDot != -1)
			{
				result = name.Substring(lastDot + 1);
			}

			return result;
		}

		private void MakeHtmlForEvents(WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList declaredEventNodes = typeNode.SelectNodes("event[not(@declaringType)]");

			if (declaredEventNodes.Count > 0)
			{
				XmlNodeList events = typeNode.SelectNodes("event");

				if (events.Count > 0)
				{
					string typeName = (string)typeNode.Attributes["name"].Value;
					string typeID = (string)typeNode.Attributes["id"].Value;
					string fileName = GetFilenameForEvents(whichType, typeNode);

					htmlHelp.AddFileToContents("Events", fileName);

					XsltArgumentList arguments = new XsltArgumentList();
					arguments.AddParam("id", String.Empty, typeID);
					arguments.AddParam("member-type", String.Empty, "event");
					TransformAndWriteResult(xsltIndividualMembers, arguments, fileName);

					htmlHelp.OpenBookInContents();

					int[] indexes = SortNodesByAttribute(events, "id");

					foreach (int index in indexes)
					{
						XmlNode eventElement = events[index];

						if (eventElement.Attributes["declaringType"] == null)
						{
							string eventName = (string)eventElement.Attributes["name"].Value;
							string eventID = (string)eventElement.Attributes["id"].Value;

							fileName = GetFilenameForEvent(eventElement);
							htmlHelp.AddFileToContents(eventName + " Event", 
								fileName, 
								HtmlHelpIcon.Page);

							arguments = new XsltArgumentList();
							arguments.AddParam("event-id", String.Empty, eventID);
							TransformAndWriteResult(xsltEvent, arguments, fileName);
						}
					}

					htmlHelp.CloseBookInContents();
				}
			}
		}

		private string GetParamList(XmlNodeList parameterNodes)
		{
			int numberOfNodes = parameterNodes.Count;
			int nodeIndex = 1;
			string paramList = "(";

			foreach (XmlNode parameterNode in parameterNodes)
			{
				paramList += StripNamespace(parameterNode.Attributes["type"].Value);

				if (nodeIndex < numberOfNodes)
				{
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

			foreach (XmlNode node in nodes)
			{
				names[i] = (string)node.Attributes[attributeName].Value;
				indexes[i] = i++;
			}

			Array.Sort(names, indexes);

			return indexes;
		}

		private void TransformAndWriteResult(
			XslTransform transform,
			XsltArgumentList arguments,
			string filename)
		{
			Trace.WriteLine(filename);
#if DEBUG
			int start = Environment.TickCount;
#endif

			ExternalHtmlProvider htmlProvider = new ExternalHtmlProvider(MyConfig, filename);
			StreamWriter streamWriter = null;

			using (streamWriter =  new StreamWriter(
					File.Open(Path.Combine(workspace.WorkingDirectory, filename), FileMode.Create),
					new UTF8Encoding(true)))
			{
				arguments.AddParam("ndoc-title", String.Empty, MyConfig.Title);
				arguments.AddParam("ndoc-vb-syntax", String.Empty, MyConfig.ShowVisualBasic);
				arguments.AddParam("ndoc-omit-object-tags", String.Empty, ((MyConfig.OutputTarget & OutputType.HtmlHelp) == 0));
				arguments.AddParam("ndoc-document-attributes", String.Empty, MyConfig.DocumentAttributes);
				arguments.AddParam("ndoc-documented-attributes", String.Empty, MyConfig.DocumentedAttributes);

				arguments.AddParam("ndoc-sdk-doc-base-url", String.Empty, utilities.SdkDocBaseUrl);
				arguments.AddParam("ndoc-sdk-doc-file-ext", String.Empty, utilities.SdkDocExt);

				arguments.AddExtensionObject("urn:NDocUtil", utilities);
				arguments.AddExtensionObject("urn:NDocExternalHtml", htmlProvider);

				//reset overloads testing
				utilities.Reset();

				transform.Transform(xpathDocument, arguments, streamWriter);
			}

#if DEBUG
			Trace.WriteLine((Environment.TickCount - start).ToString() + " msec.");
#endif
			htmlHelp.AddFileToProject(filename);
		}

		private string RemoveChar(string s, char c)
		{
			StringBuilder builder = new StringBuilder();

			foreach (char ch in s.ToCharArray())
			{
				if (ch != c)
				{
					builder.Append(ch);
				}
			}

			return builder.ToString();
		}

		private string GetFilenameForNamespace(string namespaceName)
		{
			string fileName = namespaceName + ".html";
			return fileName;
		}

		private string GetFilenameForType(XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + ".html";
			return fileName;
		}

		private string GetFilenameForTypeMembers(XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Members.html";
			return fileName;
		}

		private string GetFilenameForConstructors(XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Constructor.html";
			return fileName;
		}

		private string GetFilenameForConstructor(XmlNode constructorNode)
		{
			string constructorID = (string)constructorNode.Attributes["id"].Value;
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor

			string fileName = constructorID.Substring(2, dotHash - 2);
			if (constructorNode.Attributes["contract"].Value == "Static")
				fileName += "Static";

			fileName += "Constructor";

			if (constructorNode.Attributes["overload"] != null)
			{
				fileName += (string)constructorNode.Attributes["overload"].Value;
			}

			fileName += ".html";

			return fileName;
		}

		private string GetFilenameForFields(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Fields.html";
			return fileName;
		}

		private string GetFilenameForField(XmlNode fieldNode)
		{
			string fieldID = (string)fieldNode.Attributes["id"].Value;
			string fileName = fieldID.Substring(2) + ".html";
			return fileName;
		}

		private string GetFilenameForOperators(WhichType whichType, XmlNode typeNode)
		{
			string typeID = typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Operators.html";
			return fileName;
		}

		private string GetFilenameForOperatorsOverloads(XmlNode typeNode, XmlNode opNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string opName = (string)opNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + opName + "_overloads.html";
			return fileName;
		}

		private string GetFilenameForOperator(XmlNode operatorNode)
		{
			string operatorID = operatorNode.Attributes["id"].Value;
			string fileName = operatorID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
			{
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (operatorNode.Attributes["overload"] != null)
			{
				fileName += "_overload_" + operatorNode.Attributes["overload"].Value;
			}

			fileName += ".html";

			return fileName;
		}

		private string GetFilenameForEvents(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Events.html";
			return fileName;
		}

		private string GetFilenameForEvent(XmlNode eventNode)
		{
			string eventID = (string)eventNode.Attributes["id"].Value;
			string fileName = eventID.Substring(2) + ".html";
			return fileName;
		}

		private string GetFilenameForProperties(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Properties.html";
			return fileName;
		}

		private string GetFilenameForPropertyOverloads(XmlNode typeNode, XmlNode propertyNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string propertyName = (string)propertyNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + propertyName + ".html";
			return fileName;
		}

		private string GetFilenameForProperty(XmlNode propertyNode)
		{
			string propertyID = (string)propertyNode.Attributes["id"].Value;
			string fileName = propertyID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
			{
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (propertyNode.Attributes["overload"] != null)
			{
				fileName += (string)propertyNode.Attributes["overload"].Value;
			}

			fileName += ".html";

			return fileName;
		}

		private string GetFilenameForMethods(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Methods.html";
			return fileName;
		}

		private string GetFilenameForMethodOverloads(XmlNode typeNode, XmlNode methodNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string methodName = (string)methodNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + methodName + "_overloads.html";
			return fileName;
		}

		private string GetFilenameForMethod(XmlNode methodNode)
		{
			string methodID = (string)methodNode.Attributes["id"].Value;
			string fileName = methodID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
			{
				fileName = fileName.Substring(0, leftParenIndex);
			}

			fileName = RemoveChar(fileName, '#');

			if (methodNode.Attributes["overload"] != null)
			{
				fileName += "_overload_" + (string)methodNode.Attributes["overload"].Value;
			}

			fileName += ".html";

			return fileName;
		}
	}
}
