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

namespace NDoc3.Documenter.Msdn
{
	/// <summary>The MsdnDocumenter class.</summary>
	public class MsdnDocumenter : BaseReflectionDocumenter
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
        private XmlNamespaceManager xmlnsManager;

		private readonly Hashtable lowerCaseTypeNames;
		private readonly Hashtable mixedCaseTypeNames;
		private readonly NameResolver _nameResolver = new NameResolver();

		private StyleSheetCollection stylesheets;

		private Encoding currentFileEncoding;

		private ArrayList documentedNamespaces;

		private Workspace workspace;

		/// <summary>
		/// Initializes a new instance of the <see cref="MsdnDocumenter" />
		/// class.
		/// </summary>
		public MsdnDocumenter( MsdnDocumenterConfig config ) : base( config )
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

			string AdditionalContentResourceDirectory = MyConfig.AdditionalContentResourceDirectory;
			if ( AdditionalContentResourceDirectory.Length != 0 && !Directory.Exists( AdditionalContentResourceDirectory ) )
				return string.Format( "The Additional Content Resource Directory {0} could not be found", AdditionalContentResourceDirectory );

			string ExtensibilityStylesheet = MyConfig.ExtensibilityStylesheet;
			if ( ExtensibilityStylesheet.Length != 0 && !File.Exists( ExtensibilityStylesheet ) )
				return string.Format( "The Extensibility Stylesheet file {0} could not be found", ExtensibilityStylesheet );

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

				//Get an Encoding for the current LangID
				CultureInfo ci = new CultureInfo(MyConfig.LangID);
				currentFileEncoding = Encoding.GetEncoding(ci.TextInfo.ANSICodePage);

				// the workspace class is responsible for maintaining the outputdirectory
				// and compile intermediate locations
				workspace = new MsdnWorkspace( Path.GetFullPath( MyConfig.OutputDirectory ) );
				workspace.Clean();
				workspace.Prepare();

				// Write the embedded css files to the html output directory
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc3.Documenter.Msdn.css",
					workspace.WorkingDirectory);

				// Write the embedded icons to the html output directory
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc3.Documenter.Msdn.images",
					workspace.WorkingDirectory);

				// Write the embedded scripts to the html output directory
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc3.Documenter.Msdn.scripts",
					workspace.WorkingDirectory);

				if ( ((string)MyConfig.AdditionalContentResourceDirectory).Length > 0 )			
					workspace.ImportContentDirectory( MyConfig.AdditionalContentResourceDirectory );

				// Write the external files (FilesToInclude) to the html output directory

				foreach( string srcFilePattern in MyConfig.FilesToInclude.Split( '|' ) )
				{
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
 
					foreach(string srcFile in Directory.GetFiles(path, pattern))
					{
						string dstFile = Path.Combine(workspace.WorkingDirectory, Path.GetFileName(srcFile));
						File.Copy(srcFile, dstFile, true);
						File.SetAttributes(dstFile, FileAttributes.Archive);
					}
				}
				OnDocBuildingStep(10, "Merging XML documentation...");

				// Will hold the name of the file name containing the XML doc
				string tempFileName = null;

				try 
				{
					// determine temp file name
					tempFileName = Path.GetTempFileName();
					// Let the Documenter base class do it's thing.
					MakeXmlFile(project, new FileInfo(tempFileName));

					// Load the XML documentation into DOM and XPATH doc.
					using (FileStream tempFile = File.Open(tempFileName, FileMode.Open, FileAccess.Read)) 
					{
						FilteringXmlTextReader fxtr = new FilteringXmlTextReader(tempFile);
						xmlDocumentation = new XmlDocument();
						xmlDocumentation.Load(fxtr);
                        xmlnsManager = new XmlNamespaceManager(xmlDocumentation.NameTable);
                        xmlnsManager.AddNamespace("ns", "urn:ndoc-schema");

						tempFile.Seek(0,SeekOrigin.Begin);
						XmlTextReader reader = new XmlTextReader(tempFile);
						xpathDocument = new XPathDocument(reader, XmlSpace.Preserve);
					}
				}
				finally
				{
					if (tempFileName != null && File.Exists(tempFileName)) 
					{
						File.Delete(tempFileName);
					}
				}

				XmlNodeList typeNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly/ns:module/ns:namespace/*[name()!='documentation']", xmlnsManager);
				if (typeNodes.Count == 0)
				{
					throw new DocumenterException("There are no documentable types in this project.\n\nAny types that exist in the assemblies you are documenting have been excluded by the current visibility settings.\nFor example, you are attempting to document an internal class, but the 'DocumentInternals' visibility setting is set to False.\n\nNote: C# defaults to 'internal' if no accessibilty is specified, which is often the case for Console apps created in VS.NET...");
				}

                XmlNodeList namespaceNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly/ns:module/ns:namespace", xmlnsManager);
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
				htmlHelp.LangID=MyConfig.LangID;

				OnDocBuildingStep(25, "Building file mapping...");

				MakeFilenames();

				OnDocBuildingStep(30, "Loading XSLT files...");

				stylesheets = StyleSheetCollection.LoadStyleSheets(MyConfig.ExtensibilityStylesheet);

				OnDocBuildingStep(40, "Generating HTML pages...");

				htmlHelp.OpenProjectFile();

				htmlHelp.OpenContentsFile(string.Empty, true);

				try
				{
					if (MyConfig.CopyrightHref != null && MyConfig.CopyrightHref != String.Empty)
					{
						if (!MyConfig.CopyrightHref.StartsWith("http:"))
						{
							string copyrightFile = Path.Combine(workspace.WorkingDirectory, Path.GetFileName(MyConfig.CopyrightHref));
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
					htmlHelp.CloseContentsFile();
					htmlHelp.CloseProjectFile();
				}

				htmlHelp.WriteEmptyIndexFile();

				if ((MyConfig.OutputTarget & OutputType.Web) > 0)
				{
					OnDocBuildingStep(75, "Generating HTML content file...");

					// Write the embedded online templates to the html output directory
					EmbeddedResources.WriteEmbeddedResources(
						this.GetType().Module.Assembly,
						"NDoc3.Documenter.Msdn.onlinefiles",
						workspace.WorkingDirectory);

					using (TemplateWriter indexWriter = new TemplateWriter(
							   Path.Combine(workspace.WorkingDirectory, "index.html"),
							   new StreamReader(this.GetType().Module.Assembly.GetManifestResourceStream(
							   "NDoc3.Documenter.Msdn.onlinetemplates.index.html"))))
					{
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
					using(StreamReader contentsFile = new StreamReader(htmlHelp.GetPathToContentsFile(),Encoding.Default))
					{
						xpathDocument=new XPathDocument(contentsFile);
					}
					using ( StreamWriter streamWriter = new StreamWriter(
								File.Open(Path.Combine(workspace.WorkingDirectory, "contents.html"), FileMode.CreateNew, FileAccess.Write, FileShare.None ), Encoding.Default ) )
					{
						XslTransform("htmlcontents", xpathDocument, null, streamWriter);
					}
#if DEBUG
					Trace.WriteLine((Environment.TickCount - start).ToString() + " msec.");
#endif
				}

				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0)
				{
					OnDocBuildingStep(85, "Compiling HTML Help file...");
					htmlHelp.CompileProject();
				}
				else
				{
#if !DEBUG
					//remove .hhc file
					File.Delete(htmlHelp.GetPathToContentsFile());
#endif
				}

				// if we're only building a CHM, copy that to the Outpur dir
				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0 && (MyConfig.OutputTarget & OutputType.Web) == 0) 
				{
					workspace.SaveOutputs( "*.chm" );
				} 
				else 
				{
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
 				xpathDocument = null;
 				stylesheets = null;
				workspace.RemoveResourceDirectory();
			}
		}

		private void XslTransform(string stylesheetName, XPathDocument xpathDocument, XsltArgumentList arguments, TextWriter writer)
		{
			//Use new overload so we don't get obsolete warnings - clean compile :)
			XslCompiledTransform stylesheet = stylesheets[stylesheetName];
			stylesheet.Transform(xpathDocument, arguments, writer);
		}

		private MsdnDocumenterConfig MyConfig
		{
			get
			{
				return (MsdnDocumenterConfig)Config;
			}
		}

		private void MakeFilenames()
		{
            XmlNodeList assemblies = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly", xmlnsManager);
			foreach(XmlElement assemblyNode in assemblies)
			{
				string assemblyName = GetAttributeString(assemblyNode, "name", true);

				XmlNodeList namespaces = assemblyNode.SelectNodes("./ns:module/ns:namespace", xmlnsManager);
				foreach (XmlElement namespaceNode in namespaces)
				{
					string namespaceName = GetAttributeString(namespaceNode, "name", true);
					_nameResolver.RegisterNamespace(assemblyName, namespaceName);

					XmlNodeList types = namespaceNode.SelectNodes("*[@id]", xmlnsManager);
					foreach (XmlElement typeNode in types)
					{
						string typeId = GetAttributeString(typeNode, "id", true);
						string typeDisplayName = GetAttributeString(typeNode, "displayName", true);
						_nameResolver.RegisterType(assemblyName, typeId, typeDisplayName);
						//TODO The rest should also use displayName
						// TODO (EE): clarify what above line means (shall we remove 'name' attribute then?)
						XmlNodeList members = typeNode.SelectNodes("*[@id]");
						foreach (XmlElement memberNode in members)
						{
							string memberId = GetAttributeString(memberNode, "id", true);
							switch (memberNode.Name)
							{
								case "constructor":
									{
										MethodContract contract = GetAttributeEnum<MethodContract>(memberNode, "contract");
										string overload = GetAttributeString(memberNode, "overload", false);
										_nameResolver.RegisterConstructor(assemblyName, typeId, memberId, contract, overload);
									}
									break;
								case "field":
									{
										bool isEnum = (typeNode.Name == "enumeration");
										string memberName = GetAttributeString(memberNode, "name", true);
										_nameResolver.RegisterField(assemblyName, typeId, memberId, isEnum, memberName);
									}
									break;
								case "property":
									{
										string overload = GetAttributeString(memberNode, "overload", false);
										string memberName = GetAttributeString(memberNode, "name", true);
										_nameResolver.RegisterProperty(assemblyName, memberId, memberName, overload);
									}
									break;
								case "method":
									{
										string overload = GetAttributeString(memberNode, "overload", false);
										string memberName = GetAttributeString(memberNode, "name", true);
										_nameResolver.RegisterMethod(assemblyName, memberId, memberName, overload);
									}
									break;
								case "operator":
									{
										string overload = GetAttributeString(memberNode, "overload", false);
										string memberName = GetAttributeString(memberNode, "name", true);
										_nameResolver.RegisterOperator(assemblyName, memberId, memberName, overload);
									}
									break;
								case "event":
									{
										string memberName = GetAttributeString(memberNode, "name", true);
										_nameResolver.RegisterEvent(assemblyName, memberId, memberName);
									}
									break;
							}
						}
					}
				}
			}
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

			MakeHtmlForAssembliesSorted();

#if DEBUG
			Trace.WriteLine("Making Html: " + ((Environment.TickCount - start)/1000.0).ToString() + " sec.");
#endif
		}

		private class HtmlGenerationContext
		{
			// TODO (EE): set assemblyname during generating html
#pragma warning disable 649
			public string AssemblyName;
#pragma warning restore 649
		}

		private void MakeHtmlForAssembliesSorted()
		{
            XmlNodeList assemblyNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly", xmlnsManager);
			bool        heirTOC = (this.MyConfig.NamespaceTOCStyle == TOCStyle.Hierarchical);
			int         level = 0;
			int[] indexes = SortNodesByAttribute(assemblyNodes, "name");

			NameValueCollection namespaceAssemblies = new NameValueCollection();

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

			HtmlGenerationContext generatorContext = new HtmlGenerationContext();

			string[] last = new string[0];

			for (int i = 0; i < nNodes; i++)
			{
				OnDocBuildingProgress(i*100/nNodes);

				string currentNamespace = namespaces[i];

				if (heirTOC) 
				{
					string[] split = currentNamespace.Split('.');

					for (level = last.Length; level >= 0 &&
						ArrayEquals(split, 0, last, 0, level) == false; level--)
					{
						if (level > last.Length) 
							continue;

						MakeHtmlForTypes(generatorContext, string.Join(".", last, 0, level));
						htmlHelp.CloseBookInContents();
					}
	        
					if (level < 0) level = 0;

					for (; level < split.Length; level++)
					{
						string namespaceName = string.Join(".", split, 0, level + 1);
						
						if (Array.BinarySearch(namespaces, namespaceName) < 0)
							MakeHtmlForNamespace(generatorContext, split[level], namespaceName, false);
						else
							MakeHtmlForNamespace(generatorContext, split[level], namespaceName, true);

						htmlHelp.OpenBookInContents();
					}

					last = split;
				}
				else
				{
					MakeHtmlForNamespace(generatorContext, currentNamespace, currentNamespace, true);
					htmlHelp.OpenBookInContents();
					MakeHtmlForTypes(generatorContext, currentNamespace);
					htmlHelp.CloseBookInContents();
				}
			}

			
			if (heirTOC && last.Length > 0)
			{
				for (; level >= 1; level--)
				{
					MakeHtmlForTypes(generatorContext, string.Join(".", last, 0, level));
					htmlHelp.CloseBookInContents();
				}
			}

			OnDocBuildingProgress(100);
		}

		private bool ArrayEquals(string[] array1, int from1, string[] array2, int from2, int count)
		{
			for (int i = 0; i < count; i++)
			{
				if (array1[from1 + i] != array2[from2 + i])
					return false;
			}

			return true;
		}

		private void GetNamespacesFromAssembly(string assemblyName, System.Collections.Specialized.NameValueCollection namespaceAssemblies)
		{
            XmlNodeList namespaceNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly[@name=\"" + assemblyName + "\"]/ns:module/ns:namespace", xmlnsManager);
			foreach (XmlNode namespaceNode in namespaceNodes)
			{
				string namespaceName = (string)namespaceNode.Attributes["name"].Value;
				namespaceAssemblies.Add(namespaceName, assemblyName);
			}
		}

		/// <summary>
		/// Add the namespace elements to the output
		/// </summary>
		/// <remarks>
		/// The namespace 
		/// </remarks>
		/// <param name="generatorContext"></param>
		/// <param name="namespacePart">If nested, the namespace part will be the current
		/// namespace element being documented</param>
		/// <param name="namespaceName">The full namespace name being documented</param>
		/// <param name="addDocumentation">If true, the namespace will be documented, if false
		/// the node in the TOC will not link to a page</param>
		private void MakeHtmlForNamespace(HtmlGenerationContext generatorContext, string namespacePart, string namespaceName, 
			bool addDocumentation)
		{
			if (documentedNamespaces.Contains(namespaceName)) 
				return;

			documentedNamespaces.Add(namespaceName);

			if (addDocumentation)
			{
			string fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, "N:"+namespaceName);
				
				htmlHelp.AddFileToContents(namespacePart, fileName);

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("namespace", String.Empty, namespaceName);

			TransformAndWriteResult(generatorContext, "namespace", arguments, fileName);

			arguments = new XsltArgumentList();
			arguments.AddParam("namespace", String.Empty, namespaceName);

			TransformAndWriteResult(generatorContext,
				"namespacehierarchy",
				arguments,
				fileName.Insert(fileName.Length - 5, "Hierarchy"));
			}
			else
				htmlHelp.AddFileToContents(namespacePart);
		}

		private void MakeHtmlForTypes(HtmlGenerationContext generatorContext, string namespaceName)
		{
			XmlNodeList typeNodes =
                xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly/ns:module/ns:namespace[@name=\"" + namespaceName + "\"]/*[local-name()!='documentation' and local-name()!='typeHierarchy']", xmlnsManager);

			int[] indexes = SortNodesByAttribute(typeNodes, "id");
			int nNodes = typeNodes.Count;

			for (int i = 0; i < nNodes; i++)
			{
				XmlNode typeNode = typeNodes[indexes[i]];

				WhichType whichType = GetWhichType(typeNode);

				switch(whichType)
				{
					case WhichType.Class:
						MakeHtmlForInterfaceOrClassOrStructure(generatorContext, whichType, typeNode);
						break;
					case WhichType.Interface:
						MakeHtmlForInterfaceOrClassOrStructure(generatorContext, whichType, typeNode);
						break;
					case WhichType.Structure:
						MakeHtmlForInterfaceOrClassOrStructure(generatorContext, whichType, typeNode);
						break;
					case WhichType.Enumeration:
						MakeHtmlForEnumerationOrDelegate(generatorContext, whichType, typeNode);
						break;
					case WhichType.Delegate:
						MakeHtmlForEnumerationOrDelegate(generatorContext, whichType, typeNode);
						break;
					default:
						break;
				}
			}
		}

		private void MakeHtmlForEnumerationOrDelegate(HtmlGenerationContext generatorContext, WhichType whichType, XmlNode typeNode)
		{
			string typeName;
            if (whichType == WhichType.Delegate)
                typeName = typeNode.Attributes["displayName"].Value;
            else
                typeName = typeNode.Attributes["name"].Value;
			string typeID = typeNode.Attributes["id"].Value;
			string fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, typeID);

			htmlHelp.AddFileToContents(typeName + " " + mixedCaseTypeNames[whichType], fileName, HtmlHelpIcon.Page );

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("type-id", String.Empty, typeID);
			TransformAndWriteResult(generatorContext, "type", arguments, fileName);
		}

		private void MakeHtmlForInterfaceOrClassOrStructure(HtmlGenerationContext generatorContext, 
			WhichType whichType,
			XmlNode typeNode)
		{
			string typeName = typeNode.Attributes["displayName"].Value;
			string typeID = typeNode.Attributes["id"].Value;
			string fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, typeID);

			htmlHelp.AddFileToContents(typeName + " " + mixedCaseTypeNames[whichType], fileName);

            bool hasMembers = typeNode.SelectNodes("ns:constructor|ns:field|ns:property|ns:method|ns:operator|ns:event", xmlnsManager).Count > 0;

			if (hasMembers)
			{
				htmlHelp.OpenBookInContents();
			}

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("type-id", String.Empty, typeID);
			TransformAndWriteResult(generatorContext, "type", arguments, fileName);

            if (typeNode.SelectNodes("ns:derivedBy", xmlnsManager).Count > 5)
			{
				fileName = _nameResolver.GetFilenameForTypeHierarchy(generatorContext.AssemblyName, typeID);
				arguments = new XsltArgumentList();
				arguments.AddParam("type-id", String.Empty, typeID);
				TransformAndWriteResult(generatorContext, "typehierarchy", arguments, fileName);
			}

			if (hasMembers)
			{
				fileName = _nameResolver.GetFilenameForTypeMembers(generatorContext.AssemblyName, typeID);
				htmlHelp.AddFileToContents(typeName + " Members", 
					fileName, 
					HtmlHelpIcon.Page);

				arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				TransformAndWriteResult(generatorContext, "allmembers", arguments, fileName);

				MakeHtmlForConstructors(generatorContext, whichType, typeNode);
				MakeHtmlForFields(generatorContext, whichType, typeNode);
				MakeHtmlForProperties(generatorContext, whichType, typeNode);
				MakeHtmlForMethods(generatorContext, whichType, typeNode);
				MakeHtmlForOperators(generatorContext, whichType, typeNode);
				MakeHtmlForEvents(generatorContext, whichType, typeNode);

				htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForConstructors(HtmlGenerationContext generatorContext, WhichType whichType, XmlNode typeNode)
		{
			XmlNodeList   constructorNodes;
			string        constructorID;
			string        typeName;
			string        typeID;
			string        fileName;

			typeName = typeNode.Attributes["displayName"].Value;
			typeID = typeNode.Attributes["id"].Value;
            constructorNodes = typeNode.SelectNodes("ns:constructor[@contract!='Static']", xmlnsManager);

			// If the constructor is overloaded then make an overload page.
			if (constructorNodes.Count > 1)
			{
				fileName = _nameResolver.GetFilenameForConstructors(generatorContext.AssemblyName, typeID);
				htmlHelp.AddFileToContents(typeName + " Constructor", fileName);

				htmlHelp.OpenBookInContents();

				constructorID = constructorNodes[0].Attributes["id"].Value;

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(generatorContext, "memberoverload", arguments, fileName);
			}

			foreach (XmlNode constructorNode in constructorNodes)
			{
				constructorID = constructorNode.Attributes["id"].Value;
				fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, constructorID);

				if (constructorNodes.Count > 1)
				{
                    XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly/ns:module/ns:namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/ns:constructor[@id=\"" + constructorID + "\"]/ns:parameter", xmlnsManager);
					htmlHelp.AddFileToContents(typeName + " Constructor " + GetParamList(parameterNodes), fileName,
						HtmlHelpIcon.Page );
				}
				else
				{
					htmlHelp.AddFileToContents(typeName + " Constructor", fileName, HtmlHelpIcon.Page );
				}

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(generatorContext, "member", arguments, fileName);
			}

			if (constructorNodes.Count > 1)
			{
				htmlHelp.CloseBookInContents();
			}

            XmlNode staticConstructorNode = typeNode.SelectSingleNode("ns:constructor[@contract='Static']", xmlnsManager);
			if (staticConstructorNode != null)
			{
				constructorID = staticConstructorNode.Attributes["id"].Value;
				fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, constructorID);

				htmlHelp.AddFileToContents(typeName + " Static Constructor", fileName, HtmlHelpIcon.Page);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(generatorContext, "member", arguments, fileName);
			}
		}

		private void MakeHtmlForFields(HtmlGenerationContext generatorContext, WhichType whichType, XmlNode typeNode)
		{
            XmlNodeList fields = typeNode.SelectNodes("ns:field[not(@declaringType)]", xmlnsManager);

			if (fields.Count > 0)
			{
				//string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;
				string fileName = _nameResolver.GetFilenameForFields(generatorContext.AssemblyName, typeID);

				htmlHelp.AddFileToContents("Fields", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "field");
				TransformAndWriteResult(generatorContext, "individualmembers", arguments, fileName);

				htmlHelp.OpenBookInContents();

				int[] indexes = SortNodesByAttribute(fields, "id");

				foreach (int index in indexes)
				{
					XmlNode field = fields[index];

					string fieldName = field.Attributes["name"].Value;
					string fieldID = field.Attributes["id"].Value;
					fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, fieldID);
					htmlHelp.AddFileToContents(fieldName + " Field", fileName, HtmlHelpIcon.Page );

					arguments = new XsltArgumentList();
					arguments.AddParam("field-id", String.Empty, fieldID);
					TransformAndWriteResult(generatorContext, "field", arguments, fileName);
				}

				htmlHelp.CloseBookInContents();
			}
		}

		private void MakeHtmlForProperties(HtmlGenerationContext generatorContext, WhichType whichType, XmlNode typeNode)
		{
            XmlNodeList declaredPropertyNodes = typeNode.SelectNodes("ns:property[not(@declaringType)]", xmlnsManager);

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
                propertyNodes = typeNode.SelectNodes("ns:property[not(@declaringType)]", xmlnsManager);
				nNodes = propertyNodes.Count;

				indexes = SortNodesByAttribute(propertyNodes, "id");

				fileName = _nameResolver.GetFilenameForProperties(generatorContext.AssemblyName, typeID);
				htmlHelp.AddFileToContents("Properties", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "property");
				TransformAndWriteResult(generatorContext, "individualmembers", arguments, fileName);

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
						TransformAndWriteResult(generatorContext, "memberoverload", arguments, fileName);

						htmlHelp.OpenBookInContents();

						bOverloaded = true;
					}

					fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, propertyID);

					if (bOverloaded)
					{
                        XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly/ns:module/ns:namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/ns:property[@id=\"" + propertyID + "\"]/ns:parameter", xmlnsManager);
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
					TransformAndWriteResult(generatorContext, "property", arguments2, fileName);

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

		private void MakeHtmlForMethods(HtmlGenerationContext generatorContext, WhichType whichType, XmlNode typeNode)
		{
            XmlNodeList declaredMethodNodes = typeNode.SelectNodes("ns:method[not(@declaringType)]", xmlnsManager);

			if (declaredMethodNodes.Count > 0)
			{
				bool bOverloaded = false;
				string fileName;

				string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;
                XmlNodeList methodNodes = typeNode.SelectNodes("ns:method", xmlnsManager);
				int nNodes = methodNodes.Count;

				int[] indexes = SortNodesByAttribute(methodNodes, "id");

				fileName = _nameResolver.GetFilenameForMethods(generatorContext.AssemblyName, typeID);
				htmlHelp.AddFileToContents("Methods", fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "method");
				TransformAndWriteResult(generatorContext, "individualmembers", arguments, fileName);

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
						TransformAndWriteResult(generatorContext, "memberoverload", arguments, fileName);

						htmlHelp.OpenBookInContents();
					}

					if (methodNode.Attributes["declaringType"] == null)
					{
						fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, methodID);

						if (bOverloaded)
						{
                            XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly/ns:module/ns:namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/ns:method[@id=\"" + methodID + "\"]/ns:parameter", xmlnsManager);
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
						TransformAndWriteResult(generatorContext, "member", arguments2, fileName);
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

		private void MakeHtmlForOperators(HtmlGenerationContext generatorContext, WhichType whichType, XmlNode typeNode)
		{
            XmlNodeList operators = typeNode.SelectNodes("ns:operator", xmlnsManager);

			if (operators.Count > 0)
			{
				string typeName = (string)typeNode.Attributes["name"].Value;
				string typeID = (string)typeNode.Attributes["id"].Value;
                XmlNodeList opNodes = typeNode.SelectNodes("ns:operator", xmlnsManager);
				string fileName = _nameResolver.GetFilenameForOperators(generatorContext.AssemblyName, typeID);
				bool bOverloaded = false;

                bool bHasOperators = (typeNode.SelectSingleNode("ns:operator[@name != 'op_Explicit' and @name != 'op_Implicit']", xmlnsManager) != null);
                bool bHasConverters = (typeNode.SelectSingleNode("ns:operator[@name  = 'op_Explicit' or  @name  = 'op_Implicit']", xmlnsManager) != null);
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
				TransformAndWriteResult(generatorContext, "individualmembers", arguments, fileName);

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
							TransformAndWriteResult(generatorContext, "memberoverload", arguments, fileName);

							htmlHelp.OpenBookInContents();
						}


						fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, operatorID);
						if (bOverloaded)
						{
                            XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly/ns:module/ns:namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/ns:operator[@id=\"" + operatorID + "\"]/ns:parameter", xmlnsManager);
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
						TransformAndWriteResult(generatorContext, "member", arguments, fileName);

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
						fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, operatorID);
						htmlHelp.AddFileToContents(GetOperatorName(operatorNode), fileName, 
							HtmlHelpIcon.Page );

						arguments = new XsltArgumentList();
						arguments.AddParam("member-id", String.Empty, operatorID);
						TransformAndWriteResult(generatorContext, "member", arguments, fileName);

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
				case "op_Decrement": return "Decrement Operator";
				case "op_Increment": return "Increment Operator";
				case "op_UnaryNegation": return "Unary Negation Operator";
				case "op_UnaryPlus": return "Unary Plus Operator";
				case "op_LogicalNot": return "Logical Not Operator";
				case "op_True": return "True Operator";
				case "op_False": return "False Operator";
				case "op_AddressOf": return "Address Of Operator";
				case "op_OnesComplement": return "Ones Complement Operator";
				case "op_PointerDereference": return "Pointer Dereference Operator";
				case "op_Addition": return "Addition Operator";
				case "op_Subtraction": return "Subtraction Operator";
				case "op_Multiply": return "Multiplication Operator";
				case "op_Division": return "Division Operator";
				case "op_Modulus": return "Modulus Operator";
				case "op_ExclusiveOr": return "Exclusive Or Operator";
				case "op_BitwiseAnd": return "Bitwise And Operator";
				case "op_BitwiseOr": return "Bitwise Or Operator";
				case "op_LogicalAnd": return "LogicalAnd Operator";
				case "op_LogicalOr": return "Logical Or Operator";
				case "op_Assign": return "Assignment Operator";
				case "op_LeftShift": return "Left Shift Operator";
				case "op_RightShift": return "Right Shift Operator";
				case "op_SignedRightShift": return "Signed Right Shift Operator";
				case "op_UnsignedRightShift": return "Unsigned Right Shift Operator";
				case "op_Equality": return "Equality Operator";
				case "op_GreaterThan": return "Greater Than Operator";
				case "op_LessThan": return "Less Than Operator";
				case "op_Inequality": return "Inequality Operator";
				case "op_GreaterThanOrEqual": return "Greater Than Or Equal Operator";
				case "op_LessThanOrEqual": return "Less Than Or Equal Operator";
				case "op_UnsignedRightShiftAssignment": return "Unsigned Right Shift Assignment Operator";
				case "op_MemberSelection": return "Member Selection Operator";
				case "op_RightShiftAssignment": return "Right Shift Assignment Operator";
				case "op_MultiplicationAssignment": return "Multiplication Assignment Operator";
				case "op_PointerToMemberSelection": return "Pointer To Member Selection Operator";
				case "op_SubtractionAssignment": return "Subtraction Assignment Operator";
				case "op_ExclusiveOrAssignment": return "Exclusive Or Assignment Operator";
				case "op_LeftShiftAssignment": return "Left Shift Assignment Operator";
				case "op_ModulusAssignment": return "Modulus Assignment Operator";
				case "op_AdditionAssignment": return "Addition Assignment Operator";
				case "op_BitwiseAndAssignment": return "Bitwise And Assignment Operator";
				case "op_BitwiseOrAssignment": return "Bitwise Or Assignment Operator";
				case "op_Comma": return "Comma Operator";
				case "op_DivisionAssignment": return "Division Assignment Operator";
				case "op_Explicit":
                    XmlNode parameterNode = operatorNode.SelectSingleNode("ns:parameter", xmlnsManager);
					string from = parameterNode.Attributes["typeId"].Value;
                    string to = operatorNode.SelectSingleNode("ns:returnType", xmlnsManager).Attributes["type"].Value;
					return "Explicit " + StripNamespace(from) + " to " + StripNamespace(to) + " Conversion";
				case "op_Implicit":
                    XmlNode parameterNode2 = operatorNode.SelectSingleNode("ns:parameter", xmlnsManager);
					string from2 = parameterNode2.Attributes["typeId"].Value;
					string to2 = operatorNode.SelectSingleNode("ns:returnType", xmlnsManager).Attributes["type"].Value;
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

		private void MakeHtmlForEvents(HtmlGenerationContext generatorContext, WhichType whichType, XmlNode typeNode)
		{
            XmlNodeList declaredEventNodes = typeNode.SelectNodes("ns:event[not(@declaringType)]", xmlnsManager);

			if (declaredEventNodes.Count > 0)
			{
                XmlNodeList events = typeNode.SelectNodes("ns:event", xmlnsManager);

				if (events.Count > 0)
				{
					//string typeName = (string)typeNode.Attributes["name"].Value;
					string typeID = (string)typeNode.Attributes["id"].Value;
					string fileName = _nameResolver.GetFilenameForEvents(generatorContext.AssemblyName, typeID);

					htmlHelp.AddFileToContents("Events", fileName);

					XsltArgumentList arguments = new XsltArgumentList();
					arguments.AddParam("id", String.Empty, typeID);
					arguments.AddParam("member-type", String.Empty, "event");
					TransformAndWriteResult(generatorContext, "individualmembers", arguments, fileName);

					htmlHelp.OpenBookInContents();

					int[] indexes = SortNodesByAttribute(events, "id");

					foreach (int index in indexes)
					{
						XmlNode eventElement = events[index];

						if (eventElement.Attributes["declaringType"] == null)
						{
							string eventName = (string)eventElement.Attributes["name"].Value;
							string eventID = (string)eventElement.Attributes["id"].Value;

							fileName = _nameResolver.GetFilenameForMember(generatorContext.AssemblyName, eventID);
							htmlHelp.AddFileToContents(eventName + " Event", 
								fileName, 
								HtmlHelpIcon.Page);

							arguments = new XsltArgumentList();
							arguments.AddParam("event-id", String.Empty, eventID);
							TransformAndWriteResult(generatorContext, "event", arguments, fileName);
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

		private void TransformAndWriteResult(HtmlGenerationContext generatorContext,
			string transformName,
			XsltArgumentList arguments,
			string filename)
		{
			Trace.WriteLine(filename);
#if DEBUG
			int start = Environment.TickCount;
#endif

			ExternalHtmlProvider htmlProvider = new ExternalHtmlProvider(MyConfig, filename);

			try
			{
				StreamWriter streamWriter;
				using (streamWriter =  new StreamWriter(
					File.Open(Path.Combine(workspace.WorkingDirectory, filename), FileMode.Create),
					currentFileEncoding))
				{
					string DocLangCode = Enum.GetName(typeof(SdkLanguage), MyConfig.SdkDocLanguage).Replace("_", "-");
					MsdnXsltUtilities utilities = new MsdnXsltUtilities(_nameResolver, generatorContext.AssemblyName, MyConfig.SdkDocVersionString, DocLangCode, MyConfig.SdkLinksOnWeb, currentFileEncoding);

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

					//reset overloads testing
					utilities.Reset();

					XslCompiledTransform transform = stylesheets[transformName];
					
					//Use new overload so we don't get obsolete warnings - clean compile :)
					XslTransform(transformName, xpathDocument, arguments, streamWriter);
				}
			}
			catch(PathTooLongException e)
			{
				throw new PathTooLongException(e.Message + "\nThe file that NDoc3 was trying to create had the following name:\n" + Path.Combine(workspace.WorkingDirectory, filename));
			}

#if DEBUG
			Debug.WriteLine((Environment.TickCount - start) + " msec.");
#endif
			htmlHelp.AddFileToProject(filename);
		}

		private TVal GetAttributeEnum<TVal>(XmlNode node, string attributeName)
		{
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null)
			{
				throw new ArgumentException(string.Format("Required attribute {0} not found on node {1}: {2}", attributeName, node.Name, node.OuterXml));
			}
			return (TVal) Enum.Parse(typeof(TVal), attribute.Value);
		}

		private TVal GetAttributeEnum<TVal>(XmlNode node, string attributeName, TVal defaultValue)
		{
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null)
			{
				return defaultValue;
			}
			return (TVal) Enum.Parse(typeof(TVal), attribute.Value);
		}

		private string GetAttributeString(XmlNode node, string attributeName, bool required)
		{
			string attributeString = null;
			XmlAttribute attribute = node.Attributes[attributeName];
			if (attribute == null && required)
			{
				throw new ArgumentException(string.Format("Required attribute {0} not found on node {1}: {2}", attributeName, node.Name, node.OuterXml));
			}
			if (attribute != null)
			{
				attributeString = attribute.Value;
			}
			return attributeString;
		}

		private string GetFilenameForOperatorsOverloads(XmlNode typeNode, XmlNode opNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string opName = (string)opNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + opName + "_overloads.html";
			return fileName;
		}

		private string GetFilenameForPropertyOverloads(XmlNode typeNode, XmlNode propertyNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string propertyName = (string)propertyNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + propertyName + ".html";
			fileName = fileName.Replace("#",".");
			return fileName;
		}

		private string GetFilenameForMethodOverloads(XmlNode typeNode, XmlNode methodNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string methodName = (string)methodNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + methodName + "_overloads.html";
			return fileName;
		}

		/// <summary>
		/// This custom reader is used to load the XmlDocument. It removes elements that are not required *before* 
		/// they are loaded into memory, and hence lowers memory requirements significantly.
		/// </summary>
		private class FilteringXmlTextReader:XmlTextReader
		{
			readonly object oNamespaceHierarchy;
			readonly object oDocumentation;
			readonly object oImplements;
			readonly object oAttribute;

			public FilteringXmlTextReader(Stream file):base(file)
			{
				WhitespaceHandling=WhitespaceHandling.None;
				oNamespaceHierarchy = base.NameTable.Add("namespaceHierarchy");
				oDocumentation = base.NameTable.Add("documentation");
				oImplements = base.NameTable.Add("implements");
				oAttribute = base.NameTable.Add("attribute");
			}
		
			private bool ShouldSkipElement()
			{
				return
					(
					base.Name.Equals(oNamespaceHierarchy)||
					base.Name.Equals(oDocumentation)||
					base.Name.Equals(oImplements)||
					base.Name.Equals(oAttribute)
					);
			}

			public override bool Read()
			{
				bool notEndOfDoc=base.Read();
				if (!notEndOfDoc) return false;
				while (notEndOfDoc && (base.NodeType == XmlNodeType.Element) && ShouldSkipElement() )
				{
					notEndOfDoc=SkipElement(this.Depth);
				}
				return notEndOfDoc;
			}

			private bool SkipElement(int startDepth)
			{
				if (base.IsEmptyElement) return base.Read();
				bool notEndOfDoc=true;
				while (notEndOfDoc)
				{
					notEndOfDoc=base.Read();
					if ((base.NodeType == XmlNodeType.EndElement) && (this.Depth==startDepth) ) 
						break;
				}
				if (notEndOfDoc) notEndOfDoc=base.Read();
				return notEndOfDoc;
			}

		}
	}
}
