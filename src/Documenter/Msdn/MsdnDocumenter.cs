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
using System.Diagnostics;
using System.IO;
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

		HtmlHelp htmlHelp;

		string resourceDirectory;

		XmlDocument xmlDocumentation;

		Hashtable lowerCaseTypeNames;
		Hashtable mixedCaseTypeNames;

		XslTransform xsltNamespace;
		XslTransform xsltNamespaceHierarchy;
		XslTransform xsltType;
		XslTransform xsltAllMembers;
		XslTransform xsltIndividualMembers;
		XslTransform xsltEvent;
		XslTransform xsltMember;
		XslTransform xsltMemberOverload;
		XslTransform xsltProperty;
		XslTransform xsltField;

		ArrayList documentedNamespaces;

		/// <summary>Initializes a new instance of the MsdnHelp class.</summary>
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
				return Path.Combine(MyConfig.OutputDirectory, 
					MyConfig.HtmlHelpName) + ".chm";
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

			if (checkInputOnly) 
			{
				return null;
			}

			string path = Path.Combine(MyConfig.OutputDirectory, 
				MyConfig.HtmlHelpName) + ".chm";

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

				// Define this when you want to edit the stylesheets
				// without having to shutdown the application to rebuild.
				#if NO_RESOURCES
					string mainModuleDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
					resourceDirectory = Path.GetFullPath(Path.Combine(mainModuleDirectory, @"..\..\..\Documenter\Msdn\"));
				#else
					resourceDirectory = Path.Combine(
						Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
						@"\NDoc\MSDN\");

					EmbeddedResources.WriteEmbeddedResources(
						this.GetType().Module.Assembly,
						"NDoc.Documenter.Msdn.css",
						Path.Combine(resourceDirectory, @"css\"));

					EmbeddedResources.WriteEmbeddedResources(
						this.GetType().Module.Assembly,
						"NDoc.Documenter.Msdn.xslt",
						Path.Combine(resourceDirectory, @"xslt\"));
				#endif

				// Create the html output directory if it doesn't exist.
				if (!Directory.Exists(MyConfig.OutputDirectory))
				{
					Directory.CreateDirectory(MyConfig.OutputDirectory);
				}

				// Copy our cascading style sheet to the html output directory
				string cssfile = Path.Combine(MyConfig.OutputDirectory, "MSDN.css");
				File.Copy(Path.Combine(resourceDirectory, @"css\MSDN.css"), cssfile, true);
				File.SetAttributes(cssfile, FileAttributes.Normal); //ensure it's not read only

				// Write the embedded icons to the html output directory
				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc.Documenter.Msdn.icons",
					MyConfig.OutputDirectory);

				OnDocBuildingStep(10, "Merging XML documentation...");
				// Let the Documenter base class do it's thing.
				MakeXml(project);

				// Load the XML documentation into a DOM.
				xmlDocumentation = Document;
				//xmlDocumentation.LoadXml(Document.OuterXml);				

				XmlNodeList typeNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/*[name()!='documentation']");

				if (typeNodes.Count == 0)
				{
					throw new DocumenterException("There are no documentable types in this project.");
				}

				XmlNode defaultNamespace =
					xmlDocumentation.SelectSingleNode("/ndoc/assembly/module/namespace");

				string defaultNamespaceName = (string)defaultNamespace.Attributes["name"].Value;
				string defaultTopic = defaultNamespaceName + ".html";

				// setup for root page
				string rootPageFileName = null;
				string rootPageTOCName = "Overview";

				if ((MyConfig.RootPageFileName != null) && (MyConfig.RootPageFileName != string.Empty))
				{
					rootPageFileName = MyConfig.RootPageFileName;
					defaultTopic = "index.html";

					// what to call the top page in the table of contents?
					if ((MyConfig.RootPageTOCName != null) && (MyConfig.RootPageTOCName != string.Empty))
					{
						rootPageTOCName = MyConfig.RootPageTOCName;
					}
				}

				string compiler = Environment.ExpandEnvironmentVariables(
						MyConfig.HtmlHelpCompilerFilename);

				htmlHelp = new HtmlHelp(
					MyConfig.OutputDirectory,
					MyConfig.HtmlHelpName,
					defaultTopic,
					compiler);

				htmlHelp.IncludeFavorites = MyConfig.IncludeFavorites;

				OnDocBuildingStep(40, "Loading XSLT files...");

				MakeTransforms();

				OnDocBuildingStep(60, "Generating HTML pages...");

				htmlHelp.OpenProjectFile();

				if (!MyConfig.SplitTOCs)
				{
					htmlHelp.OpenContentsFile(string.Empty, true);
				}

				try
				{
					if (MyConfig.CopyrightHref != null && MyConfig.CopyrightHref != String.Empty)
					{
						if (!MyConfig.CopyrightHref.StartsWith("http:"))
						{
							File.Copy(MyConfig.CopyrightHref, Path.Combine(MyConfig.OutputDirectory, Path.GetFileName(MyConfig.CopyrightHref)), true);
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
						string rootPageOutputName = Path.Combine(MyConfig.OutputDirectory, "index.html");
						if (Path.GetFullPath(rootPageFileName) != Path.GetFullPath(rootPageOutputName))
						{
							if (File.Exists(rootPageOutputName))
							{
								File.SetAttributes(rootPageOutputName, FileAttributes.Normal);
							}
							File.Copy(rootPageFileName, rootPageOutputName, true);
						}
						htmlHelp.AddFileToProject(Path.GetFileName(rootPageOutputName));
						htmlHelp.AddFileToContents(rootPageTOCName, 
							Path.GetFileName(rootPageOutputName));
						htmlHelp.OpenBookInContents();
					}

					documentedNamespaces = new ArrayList();
					MakeHtmlForAssemblies();

					// close root book if applicable
					if (rootPageFileName != null)
					{
						htmlHelp.CloseBookInContents();
					}

				}
				finally
				{
					if (!MyConfig.SplitTOCs)
					{
						htmlHelp.CloseContentsFile();
					}

					htmlHelp.CloseProjectFile();
				}

				htmlHelp.WriteEmptyIndexFile();

				OnDocBuildingStep(85, "Compiling HTML Help file...");

				htmlHelp.CompileProject();

				OnDocBuildingStep(100, "Done.");
			}
			catch(Exception ex)
			{
				throw new DocumenterException(ex.Message, ex);
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
				transform.Load(resourceDirectory + "xslt/" + filename);
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

		private void MakeTransforms()
		{
			OnDocBuildingProgress(0);

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

			OnDocBuildingProgress(10);
			MakeTransform(
				xsltNamespace,
				"namespace.xslt");

			OnDocBuildingProgress(20);
			MakeTransform(
				xsltNamespaceHierarchy,
				"namespacehierarchy.xslt");

			OnDocBuildingProgress(30);
			MakeTransform(
				xsltType,
				"type.xslt");

			OnDocBuildingProgress(40);
			MakeTransform(
				xsltAllMembers,
				"allmembers.xslt");

			OnDocBuildingProgress(50);
			MakeTransform(
				xsltIndividualMembers,
				"individualmembers.xslt");

			OnDocBuildingProgress(60);
			MakeTransform(
				xsltEvent,
				"event.xslt");

			OnDocBuildingProgress(70);
			MakeTransform(
				xsltMember,
				"member.xslt");

			OnDocBuildingProgress(80);
			MakeTransform(
				xsltMemberOverload,
				"memberoverload.xslt");

			OnDocBuildingProgress(90);
			MakeTransform(
				xsltProperty,
				"property.xslt");

			OnDocBuildingProgress(100);
			MakeTransform(
				xsltField,
				"field.xslt");
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
			if (MyConfig.SortTOCByNamespace)
				MakeHtmlForAssembliesSorted();
			else
				MakeHtmlForAssembliesUnsorted();
		}

		private void MakeHtmlForAssembliesUnsorted()
		{
			XmlNodeList assemblyNodes = xmlDocumentation.SelectNodes("/ndoc/assembly");
			int[] indexes = SortNodesByAttribute(assemblyNodes, "name");

			int nNodes = assemblyNodes.Count;

			for (int i = 0; i < nNodes; i++)
			{
				XmlNode assemblyNode = assemblyNodes[indexes[i]];

				if (assemblyNode.ChildNodes.Count > 0)
				{
					string assemblyName = (string)assemblyNode.Attributes["name"].Value;

					if (MyConfig.SplitTOCs)
					{
						bool isDefault = (assemblyName == MyConfig.DefaulTOC);

						if (isDefault)
						{
							XmlNode defaultNamespace =
								xmlDocumentation.SelectSingleNode("/ndoc/assembly[@name='" 
								+ assemblyName + "']/module/namespace");

							if (defaultNamespace != null)
							{
								string defaultNamespaceName = (string)defaultNamespace.Attributes["name"].Value;
								htmlHelp.DefaultTopic = defaultNamespaceName + ".html";
							}
						}

						htmlHelp.OpenContentsFile(assemblyName, isDefault);
					}

					try
					{
						MakeHtmlForNamespaces(assemblyName);
					}
					finally
					{
						if (MyConfig.SplitTOCs)
						{
							htmlHelp.CloseContentsFile();
						}
					}
				}
			}
		}

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

					// Skip duplicate namespaces.
					if (documentedNamespaces.Contains(namespaceName))
					{
						continue;
					}

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

			nNodes = namespaceAssemblies.Count;
			string [] namespaces = namespaceAssemblies.AllKeys;
			Array.Sort(namespaces);
			for (int i = 0; i < nNodes; i++)
			{
				OnDocBuildingProgress(i*100/nNodes);
				string namespaceName = namespaces[i];
				foreach (string assemblyName in namespaceAssemblies.GetValues(i))
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

			htmlHelp.AddFileToContents(typeName + " " + mixedCaseTypeNames[whichType], fileName);

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
				htmlHelp.AddFileToContents(typeName + " Members", fileName);

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
			constructorNodes = typeNode.SelectNodes("constructor");

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
					htmlHelp.AddFileToContents(typeName + " Constructor " + GetParamList(parameterNodes), fileName);
				}
				else
				{
					htmlHelp.AddFileToContents(typeName + " Constructor", fileName);
				}

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);
				TransformAndWriteResult(xsltMember, arguments, fileName);
			}

			if (constructorNodes.Count > 1)
			{
				htmlHelp.CloseBookInContents();
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
					htmlHelp.AddFileToContents(fieldName + " Field", fileName);

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
						htmlHelp.AddFileToContents(propertyName + " Property " + GetParamList(parameterNodes), fileName);
					}
					else
					{
						htmlHelp.AddFileToContents(propertyName + " Property", fileName);
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

		private string GetPreviousMethodName(
			XmlNodeList methodNodes,
			int[] indexes,
			int index)
		{
			while (--index >= 0)
			{
				if (methodNodes[indexes[index]].Attributes["declaringType"] == null)
				{
					return methodNodes[indexes[index]].Attributes["name"].Value;
				}
			}

			return null;
		}

		private string GetNextMethodName(
			XmlNodeList methodNodes,
			int[] indexes,
			int index)
		{
			while (++index < methodNodes.Count)
			{
				if (methodNodes[indexes[index]].Attributes["declaringType"] == null)
				{
					return methodNodes[indexes[index]].Attributes["name"].Value;
				}
			}

			return null;
		}

		private bool IsMethodFirstOverload(
			XmlNodeList methodNodes,
			int[] indexes,
			int index)
		{
			if (methodNodes[indexes[index]].Attributes["declaringType"] != null)
			{
				return false;
			}

			string name = methodNodes[indexes[index]].Attributes["name"].Value;
			int count = methodNodes.Count;

			string previousName = GetPreviousMethodName(methodNodes, indexes, index);
			string nextName = GetNextMethodName(methodNodes, indexes, index);

			return previousName != name && name == nextName;
		}

		private bool IsMethodLastOverload(XmlNodeList methodNodes, int[] indexes, int index)
		{
			if (methodNodes[indexes[index]].Attributes["declaringType"] != null)
			{
				return false;
			}

			string name = (string)methodNodes[indexes[index]].Attributes["name"].Value;
			int count = methodNodes.Count;

			string previousName = GetPreviousMethodName(methodNodes, indexes, index);
			string nextName = GetNextMethodName(methodNodes, indexes, index);

			return previousName == name && name != nextName;
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
							htmlHelp.AddFileToContents(methodName + " Method " + GetParamList(parameterNodes), fileName);
						}
						else
						{
							htmlHelp.AddFileToContents(methodName + " Method", fileName);
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

				string title = "Operators";

				if (typeNode.SelectSingleNode("operator[@name = 'op_Explicit' or @name = 'op_Implicit']") != null)
				{
					title += " and Type Conversions";
				}

				htmlHelp.AddFileToContents(title, fileName);

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);
				arguments.AddParam("member-type", String.Empty, "operator");
				TransformAndWriteResult(xsltIndividualMembers, arguments, fileName);

				htmlHelp.OpenBookInContents();

				int[] indexes = SortNodesByAttribute(operators, "id");
				int nNodes = opNodes.Count;

				for (int i = 0; i < nNodes; i++)
				{
					XmlNode operatorNode = operators[indexes[i]];
					string operatorID = operatorNode.Attributes["id"].Value;

					if (IsMethodFirstOverload(opNodes, indexes, i))
					{
						string opName = (string)operatorNode.Attributes["name"].Value;
						if ((opName != "op_Implicit") 
							&& (opName != "op_Implicit"))
						{
							bOverloaded = true;

							fileName = GetFilenameForOperatorsOverloads(typeNode, operatorNode);
							htmlHelp.AddFileToContents(GetOperatorName(operatorNode), fileName);

							arguments = new XsltArgumentList();
							arguments.AddParam("member-id", String.Empty, operatorID);
							TransformAndWriteResult(xsltMemberOverload, arguments, fileName);

							htmlHelp.OpenBookInContents();
						}
					}


					fileName = GetFilenameForOperator(operatorNode);
					if (bOverloaded)
					{
						XmlNodeList parameterNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/" + lowerCaseTypeNames[whichType] + "[@name=\"" + typeName + "\"]/operator[@id=\"" + operatorID + "\"]/parameter");
						htmlHelp.AddFileToContents(GetOperatorName(operatorNode) + GetParamList(parameterNodes), fileName);
					}
					else
					{
						htmlHelp.AddFileToContents(GetOperatorName(operatorNode), fileName);
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

					string eventName = (string)eventElement.Attributes["name"].Value;
					string eventID = (string)eventElement.Attributes["id"].Value;
					fileName = GetFilenameForEvent(eventElement);
					htmlHelp.AddFileToContents(eventName + " Event", fileName);

					arguments = new XsltArgumentList();
					arguments.AddParam("event-id", String.Empty, eventID);
					TransformAndWriteResult(xsltEvent, arguments, fileName);
				}

				htmlHelp.CloseBookInContents();
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

			StreamWriter streamWriter = null;

			try
			{
				streamWriter =  new StreamWriter(
					File.Open(Path.Combine(MyConfig.OutputDirectory, filename), FileMode.Create),
					new UTF8Encoding(true));

				arguments.AddParam("ndoc-title", String.Empty, MyConfig.Title);
				arguments.AddParam("ndoc-vb-syntax", String.Empty, MyConfig.ShowVisualBasic);
				arguments.AddParam("ndoc-omit-object-tags", String.Empty, MyConfig.OmitObjectTags);
				arguments.AddParam("ndoc-document-attributes", String.Empty, MyConfig.DocumentAttributes);
				arguments.AddParam("ndoc-documented-attributes", String.Empty, MyConfig.DocumentedAttributes);

				transform.Transform(xmlDocumentation, arguments, streamWriter);
			}
			finally
			{
				if (streamWriter != null)
				{
					streamWriter.Close();
				}
			}

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
			string fileName = typeID.Substring(2) + "." + opName + ".html";
			return fileName;
		}

		private string GetFilenameForOperator(XmlNode operatorNode)
		{
			string operatorID = operatorNode.Attributes["id"].Value;
			string fileName = operatorID.Substring(2);

			int opIndex = fileName.IndexOf("op_");

			if (opIndex != -1)
			{
				fileName = fileName.Remove(opIndex, 3);
			}

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
			{
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (operatorNode.Attributes["overload"] != null)
			{
				fileName += operatorNode.Attributes["overload"].Value;
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
			string fileName = typeID.Substring(2) + "." + methodName + ".html";
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
				fileName += (string)methodNode.Attributes["overload"].Value;
			}

			fileName += ".html";

			return fileName;
		}
	}
}
