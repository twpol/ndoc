// LinearHtmlDocumenter.cs
// Copyright (C) 2003 Ryan Seghers
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

// To-do:
//   - make types in tables (method return types, etc) different color and font
//   - add namespace summaries
//   - add Types list to start of each namespace section
//       - if the ndoc xml was reorganized some this would be a lot easier
//          - want namespaces, Types sorted
//          - different structure to handle namespaces-across-modules issues
//           <ndoc>
//              <namespace1>
//                  <assembly name="whatever" version="whatever">
//                  <assembly name="whatever1" version="whatever1">
//                  <class ...
//                  <interface ...
//                  ...
//              <namespace2>
//       - build index: namespaceName -> Type category -> entries
//       - alphabetize and emit at end
//   - handle method overloads some way or other
//   - add an option to provide full details on members
//     (method arguments, remarks sections, ...)
//   - finish XSLT mode or remove it
//		- remove unused embedded resources
//   - make sorting and grouping simpler? (speed-complexity tradeoff)
//

// this allows switching between XmlDocument and XPathDocument
// (XPathDocument currently doesn't handle <code> nodes well)
#define USE_XML_DOCUMENT

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Schema;

using NDoc.Core;


namespace NDoc.Documenter.LinearHtml
{
	/// <summary>
	/// This creates a linear (serial, more printable) html file from an ndoc xml file. 
	/// This was designed and implemented with the intention that the html will be
	/// inserted into a Word document, but this could be useful for other scenarios.
	/// </summary>
	/// <remarks>
	/// <para><pre>
	/// The document produced is organized as follows:
	///    Namespaces List: a section listing namespaces and which assembly they're from
	///    Namespace: a section for each namespace
	///			Types List: a list of classes, interfaces, etc in the namespace, with their
	///				summaries. (not implemented yet)
	///			Type: Classes
	///			Type: Interfaces
	///			Type: Enumerations
	///			Type: Structs
	///			Type: Delegates
	///	</pre></para>
	///	<para>
	///	This class uses C#'s xml processing capabilities rather than xslt.
	///	This was more or less an experiment, and I'm not sure whether this
	///	is better than an xslt implementation or not.  The complexity might
	///	be similar, but I expect this implementation to be many times faster
	///	than an equivalent xslt implementation.
	///	</para>
	///	<para>
	///	This class writes a single linear html file, but traverses the xml
	///	document pretty much just once.  To do this, multiple XmlTextWriters
	///	are create such that they can be written to in any order.  Then at
	///	the end the memory buffers written by each text writer are copied
	///	to the output file in the appropriate order.
	///	</para> 
	///	<para>This has a Main for easier and faster test runs outside of NDoc.</para>
	/// </remarks>
	public class LinearHtmlDocumenter : BaseDocumenter
	{
		#region Fields

		/// <summary>The main navigator which keeps track of where we
		/// are in the document.</summary>
		XPathNavigator xPathNavigator;

		/// <summary>Writer for the first section, the namespace list.</summary>
		XmlTextWriter namespaceListWriter;

		/// <summary>A hashtable from namespace name to Hashtables which
		/// go from section name (Classes, Interfaces, etc) to XmlTextWriters
		/// for that section.</summary>
		Hashtable namespaceWriters; // namespace name -> Hashtables (of writers for each section)

		/// <summary>Hashtable from xml node name to section name. For example
		/// class to Classes.</summary>
		Hashtable namespaceSections; // xml node name -> section name

		/// <summary>
		/// A list of Type (class, interface) member types, to specify
		/// the order in which they should be rendered.
		/// </summary>
		string[] orderedMemberTypes = { "constructor", "field", "property", "method" };

		/// <summary>Tmp location for embedded resource files.</summary>
		string resourceDirectory;

		// the Xslt is nowhere near good yet
		bool useXslt = false;

		/// <summary>This transform can be used for each type. This is incomplete.</summary>
		XslTransform typeTransform;

		#endregion

		#region Properties

		private LinearHtmlDocumenterConfig MyConfig
		{
			get { return (LinearHtmlDocumenterConfig)Config; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor.
		/// </summary>
		public LinearHtmlDocumenter() : base("LinearHtml")
		{
			namespaceWriters = new Hashtable();

			namespaceSections = new Hashtable();
			namespaceSections.Add("class", "Classes");
			namespaceSections.Add("interface", "Interfaces");
			namespaceSections.Add("enumeration", "Enumerations");
			namespaceSections.Add("structure", "Structs");
			namespaceSections.Add("delegate", "Delegates");

			Clear();
		}

		#endregion

		#region Main Public API

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Clear()
		{
			Config = new LinearHtmlDocumenterConfig();
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string MainOutputFile 
		{ 
			get { return Path.Combine(MyConfig.OutputDirectory, "linear.html"); } 
		}

		/// <summary>
		/// Load the specified NDoc Xml file into this object's memory.
		/// This is useful when this class is used outside of the context of NDoc.
		/// </summary>
		/// <param name="fileName">The NDoc Xml file to load.</param>
		/// <returns>bool - always true</returns>
		public bool Load(string fileName)
		{
			#if USE_XML_DOCUMENT
				XmlDocument doc = new XmlDocument();
				doc.Load(fileName);
			#else
				XPathDocument doc = new XPathDocument(fileName);
			#endif

			xPathNavigator = doc.CreateNavigator();
			return(true);
		}

		/// <summary>
		/// Load the specified NDoc Xml into this object's memory.
		/// This is useful when this class is used outside of the context of NDoc.
		/// </summary>
		/// <param name="s">The stream to load.</param>
		/// <returns>bool - always true</returns>
		public bool Load(Stream s)
		{
			#if USE_XML_DOCUMENT
				XmlDocument doc = new XmlDocument();
				doc.Load(s);
			#else
				XPathDocument doc = new XPathDocument(s);
			#endif

			xPathNavigator = doc.CreateNavigator();
			return(true);
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string CanBuild(Project project, bool checkInputOnly)
		{
			string result = base.CanBuild(project, checkInputOnly); 
			if (result != null) { return result; }
			if (checkInputOnly) { return null; }

			// test if output file is open
			string path = this.MainOutputFile;
			string temp = Path.Combine(MyConfig.OutputDirectory, "~lhtml.tmp");

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
				result = "The output file is probably open.\nPlease close it and try again.";
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
					string mainModuleDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					resourceDirectory = Path.GetFullPath(Path.Combine(mainModuleDirectory, @"..\..\..\Documenter\Msdn\"));
				#else

				resourceDirectory = Path.Combine(Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
					"NDoc"), "LinearHtml");

				EmbeddedResources.WriteEmbeddedResources(this.GetType().Module.Assembly,
					"NDoc.Documenter.LinearHtml.xslt",
					Path.Combine(resourceDirectory, "xslt"));
				#endif

				// Create the html output directory if it doesn't exist.
				if (!Directory.Exists(MyConfig.OutputDirectory))
				{
					Directory.CreateDirectory(MyConfig.OutputDirectory);
				}
				else
				{
					//clean-up output path
					foreach (string file in Directory.GetFiles(MyConfig.OutputDirectory, "*.*"))
					{
						try
						{
							File.Delete(file);
						}
						catch (IOException)
						{
							Trace.WriteLine("Could not delete " + file 
								+ " from the output directory because it is in use.");
							// IOException means the file is in use. Swallow the exception and continue.
						}
					}
				}

				// Write the embedded css files to the html output directory
				EmbeddedResources.WriteEmbeddedResources(this.GetType().Module.Assembly,
					"NDoc.Documenter.LinearHtml.css", MyConfig.OutputDirectory);

				// Write the external files (FilesToInclude) to the html output directory
				foreach( string srcFile in MyConfig.FilesToInclude.Split( '|' ) )
				{
					if ((srcFile == null) || (srcFile.Length == 0))
						continue;

					string dstFile = Path.Combine(MyConfig.OutputDirectory, Path.GetFileName(srcFile));
					File.Copy(srcFile, dstFile, true);
				}

				OnDocBuildingStep(10, "Merging XML documentation...");
				// Let the Documenter base class do it's thing.
				MakeXml(project);

				// Load the XML documentation
#if USE_XML_DOCUMENT
				xPathNavigator = Document.CreateNavigator();
#else
				XmlTextWriter tmpWriter = new XmlTextWriter(new MemoryStream(), Encoding.UTF8);
				Document.WriteTo(tmpWriter);
				tmpWriter.Flush();
				tmpWriter.BaseStream.Position = 0;
				this.Load(tmpWriter.BaseStream);
#endif

				// check for documentable types
				XmlNodeList typeNodes = Document.SelectNodes("/ndoc/assembly/module/namespace/*[name()!='documentation']");

				if (typeNodes.Count == 0)
				{
					throw new DocumenterException("There are no documentable types in this project.");
				}

				// create and write the html
				OnDocBuildingStep(50, "Generating HTML page...");
				MakeHtml(this.MainOutputFile);
				OnDocBuildingStep(100, "Done.");
			}
			catch(Exception ex)
			{
				throw new DocumenterException(ex.Message, ex);
			}
		}

		#endregion

		#region Html Utility Methods

		/// <summary>
		/// Setup any text writers.
		/// </summary>
		/// <returns></returns>
		private bool StartWriters()
		{
			// namespace list
			namespaceListWriter = new XmlTextWriter(new MemoryStream(), Encoding.UTF8);
			//namespaceListWriter.Formatting = Formatting.Indented;
			namespaceListWriter.Indentation = 4;

			namespaceListWriter.WriteElementString("h1", "Namespace List");
			namespaceListWriter.WriteElementString("p", "The namespaces specified in this document are:");

			// table
			string[] columnNames = { "Namespace", "Assembly" };
			StartTable(namespaceListWriter, "NamespaceListTable", 600, columnNames);

			return(true);
		}

		/// <summary>
		/// Write the starting tags for a table to the specified writer.
		/// </summary>
		/// <param name="xtw"></param>
		/// <param name="id">The table id to put in the html.</param>
		/// <param name="width">The width (pixels) of the table.</param>
		/// <param name="columnNames">An array of column names to use
		/// in the header row. This also sets the number of columns.</param>
		/// <returns>bool - true</returns>
		private bool StartTable(XmlTextWriter xtw, string id, int width, 
			string[] columnNames)
		{
			xtw.WriteStartElement("TABLE");
			xtw.WriteAttributeString("class", "dtTable");
			xtw.WriteAttributeString("id", id);
			xtw.WriteAttributeString("cellSpacing", "1");
			xtw.WriteAttributeString("cellPadding", "1");
			xtw.WriteAttributeString("width", String.Format("{0}", width));
			xtw.WriteAttributeString("border", "1");
			xtw.WriteStartElement("TR");
			foreach(string colName in columnNames)
			{
				xtw.WriteStartElement("TH");
				xtw.WriteAttributeString("style", "background:#FFFFCA");
				xtw.WriteString(colName);
				xtw.WriteEndElement();
			}
			xtw.WriteEndElement(); // TR
			return(true);
		}

		/// <summary>
		/// End a table. This is provided for symmetry, and in case there's
		/// something else I have to write in tables in the future.
		/// </summary>
		/// <param name="xtw"></param>
		private void EndTable(XmlTextWriter xtw)
		{
			xtw.WriteEndElement(); // TABLE
		}

		/// <summary>
		/// Do whatever is neccesary to any writers before emitting html.
		/// </summary>
		/// <returns></returns>
		private bool EndWriters()
		{
			namespaceListWriter.WriteEndElement(); // table
			namespaceListWriter.Flush();
			return(true);
		}

		/// <summary>
		/// Create a namespace section writer if one doesn't already exist
		/// for the specified namespace and section.
		/// </summary>
		/// <param name="namespaceName">C# namespace name, not xml namespace.</param>
		/// <param name="sectionName">The section name, such as Classes.</param>
		void StartNamespaceSectionWriter(string namespaceName, string sectionName)
		{
			if (!namespaceWriters.ContainsKey(namespaceName)) 
				namespaceWriters.Add(namespaceName, new Hashtable());

			Hashtable nsSectionWriters = (Hashtable)namespaceWriters[namespaceName];
			if (!nsSectionWriters.ContainsKey(sectionName))
			{
				nsSectionWriters.Add(sectionName, new XmlTextWriter(new MemoryStream(),
					Encoding.UTF8));

				XmlTextWriter xtw = (XmlTextWriter)nsSectionWriters[sectionName];
				//xtw.Formatting = Formatting.Indented;
				xtw.Indentation = 4;

				xtw.WriteElementString("h2", String.Format("{0} {1}", namespaceName, sectionName));
			}
		}

		#endregion

		#region Make Html

		/// <summary>
		/// Build and emit the html document from the loaded NDoc Xml document.
		/// </summary>
		/// <returns></returns>
		private bool MakeHtml(string outputFileName)
		{
			StartWriters();
			xPathNavigator.MoveToRoot();
			xPathNavigator.MoveToFirstChild(); // moves to doc
			xPathNavigator.MoveToFirstChild(); // moves to assemblies

			// for each assembly...
			do 
			{
				MakeHtmlForAssembly(xPathNavigator);
			} while(xPathNavigator.MoveToNext());

			EndWriters();
			EmitHtml(outputFileName);

			return(true);
		}

		/// <summary>
		/// Do the build operations given that the specified XPathNavigator is pointing to
		/// an assembly node.
		/// </summary>
		/// <param name="nav">The XPathNavigator pointing to a node of type
		/// appropriate for this method.</param>
		void MakeHtmlForAssembly(XPathNavigator nav)
		{
			string assemblyName = nav.GetAttribute("name", "");
			string assemblyVersion = nav.GetAttribute("version", "");
			Console.WriteLine("Assembly: {0}, {1}", assemblyName, assemblyVersion);
			nav.MoveToFirstChild();

			// foreach module...
			do 
			{
				MakeHtmlForModule(nav, assemblyName, assemblyVersion);
			} while(nav.MoveToNext());

			nav.MoveToParent();
		}

		/// <summary>
		/// Do the build operations given that the specified XPathNavigator is pointing to
		/// an module node.
		/// </summary>
		/// <param name="nav">The XPathNavigator pointing to a node of type
		/// appropriate for this method.</param>
		void MakeHtmlForModule(XPathNavigator nav, string assemblyName, string assemblyVersion)
		{
			string moduleName = nav.GetAttribute("name", "");
			Console.WriteLine("Module: {0}", nav.GetAttribute("name", ""));
			nav.MoveToFirstChild();

			// foreach namespace
			do 
			{
				MakeHtmlForNamespace(nav, assemblyName, assemblyVersion);
			} while(nav.MoveToNext());

			nav.MoveToParent();
		}

		/// <summary>
		/// Do the build operations given that the specified XPathNavigator is pointing to
		/// an namespace node.
		/// </summary>
		/// <param name="nav">The XPathNavigator pointing to a node of type
		/// appropriate for this method.</param>
		/// <param name="assemblyName">The name of the assembly containing this namespace.</param>
		/// <param name="assemblyVersion">The version of the assembly containing this namespace.</param>
		void MakeHtmlForNamespace(XPathNavigator nav, string assemblyName, string assemblyVersion)
		{
			string namespaceName = nav.GetAttribute("name", "");

			// skip this namespace based on regex
			if ((MyConfig.NamespaceExcludeRegexp != null) 
				&& (MyConfig.NamespaceExcludeRegexp.Length > 0))
			{
				Regex nsReject = new Regex(MyConfig.NamespaceExcludeRegexp);

				if (nsReject.IsMatch(namespaceName))
				{
					Console.WriteLine("Rejecting namespace {0} by regexp", namespaceName);
					return;
				}
			}

			Console.WriteLine("Namespace: {0}", namespaceName);

			//
			// namespace list
			//
			namespaceListWriter.WriteStartElement("TR");
			namespaceListWriter.WriteElementString("TD", namespaceName);

			string assemblyString = assemblyName;
			if ((assemblyVersion != null) && (assemblyVersion.Length > 0))
			{
				Version v = new Version(assemblyVersion);
				string vString = String.Format("{0}.{1}.{2}.*", v.Major, v.Minor, v.Build);
				assemblyString = assemblyName + " Version " + vString;
			}

			namespaceListWriter.WriteElementString("TD", assemblyString);
			namespaceListWriter.WriteEndElement(); // TR

			//
			// Types in namespace
			//
			nav.MoveToFirstChild(); // move into namespace children

			// foreach Type (class, delegate, interface)...
			do 
			{
				MakeHtmlForType(nav, namespaceName);
			} while(nav.MoveToNext());

			nav.MoveToParent();
		}

		/// <summary>
		/// Builds html for a Type.  An Type here is a class, struct, interface, etc.
		/// </summary>
		/// <param name="nav">The XPathNavigator pointing to the type node.</param>
		/// <param name="namespaceName">The namespace containing this type.</param>
		void MakeHtmlForType(XPathNavigator nav, string namespaceName)
		{
			string nodeName = nav.GetAttribute("name", "");
			string nodeType = nav.LocalName;
			Trace.WriteLine(String.Format("MakeHtmlForType: Visiting Type Node: {0}: {1}", nodeType, nodeName));

			if (namespaceSections.ContainsKey(nodeType))
			{
				// write to appropriate writer
				string sectionName = (string)namespaceSections[nodeType];
				StartNamespaceSectionWriter(namespaceName, sectionName);
					
				// now write members
				Hashtable nsSectionWriters = (Hashtable)namespaceWriters[namespaceName];
				XmlTextWriter xtw = (XmlTextWriter)nsSectionWriters[sectionName];

				if (useXslt)
				{
					MakeHtmlForTypeUsingXslt(nav, xtw, namespaceName);
				}
				else
				{
					MakeHtmlForTypeUsingCs(nav, xtw, namespaceName);
				}
			}
			else Console.WriteLine("Warn: MakeHtmlForType: Unknown section for node name {0}", 
					 nav.LocalName);
		}

		/// <summary>
		/// Use Xslt transform to document this type.
		/// </summary>
		/// <param name="nav"></param>
		/// <param name="xtw"></param>
		/// <param name="namespaceName"></param>
		void MakeHtmlForTypeUsingXslt(XPathNavigator nav, XmlTextWriter xtw, string namespaceName)
		{
			string nodeName = nav.GetAttribute("name", "");
			string nodeType = nav.LocalName;
			string typeId = nav.GetAttribute("id", "");

			// create transform if it hasn't already been created
			if (typeTransform == null)
			{
				string fileName = "linearHtml.xslt";
				//string transformPath = Path.Combine(Path.Combine(resourceDirectory, "xslt"), filename);
				string transformPath = Path.Combine(@"C:\VSProjects\Util\ndoc\src\Documenter\LinearHtml\xslt", 
					fileName);

				typeTransform = new XslTransform();
				typeTransform.Load(transformPath);
			}

			// execute the transform
			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam("namespace", String.Empty, namespaceName);
			arguments.AddParam("type-id", String.Empty, typeId);
			arguments.AddParam("includeHierarchy", String.Empty, MyConfig.IncludeHierarchy);
			typeTransform.Transform(nav, arguments, xtw);
		}

		/// <summary>
		/// Document the current node's type using C#.
		/// </summary>
		/// <param name="nav"></param>
		/// <param name="xtw"></param>
		/// <param name="namespaceName"></param>
		void MakeHtmlForTypeUsingCs(XPathNavigator nav, XmlTextWriter xtw, string namespaceName)
		{
			string nodeName = nav.GetAttribute("name", "");
			string nodeType = nav.LocalName;

			string capsItemType = char.ToUpper(nodeType[0]) + nodeType.Substring(1);
			xtw.WriteElementString("h3", String.Format("{0} {1}", nodeName, capsItemType));

			//
			// collect navigators to various members by category
			//
			Hashtable memberTypeHt = new Hashtable(); // memberType -> Hashtable (id -> navigator)
			nav.MoveToFirstChild();
			Hashtable navTable;

			do 
			{
				// each member of type
				string memberType = nav.LocalName;
				string memberName = nav.GetAttribute("name", "");
				string memberId = nav.GetAttribute("id", "");

				if (!memberTypeHt.ContainsKey(memberType)) 
				{
					//Console.WriteLine("Add member type {0}", memberType);
					memberTypeHt.Add(memberType, new Hashtable());
				}
				navTable = (Hashtable)memberTypeHt[memberType];
				if (!navTable.ContainsKey(memberId)) navTable.Add(memberId, nav.Clone());
			} while(nav.MoveToNext());

			nav.MoveToParent();

			//
			// now render each type of member
			//

			// documentation/summary
			XPathNavigator remarksNav = null;
			if (memberTypeHt.ContainsKey("documentation"))
			{
				xtw.WriteElementString("h4", "Summary");

				navTable = (Hashtable)memberTypeHt["documentation"];
				XPathNavigator nav2 = (XPathNavigator)navTable[String.Empty];
				XPathNavigator summaryNav = GetChildNodeOfType(nav2, "summary");
				remarksNav = GetChildNodeOfType(nav2, "remarks");

				if (summaryNav != null)
				{
					// want the XmlNode if possible, can't seem to get raw xml from the navigator
					// don't know another way to test the cast, and it must be slow on exception
					XmlNode n = null;
					try
					{
						n = ((IHasXmlNode)summaryNav).GetNode();
					}
					catch(Exception) {}

					// write it
					xtw.WriteStartElement("p");
					if (n != null) 
					{
						FixCodeNodes(n); // change <code> to <pre class="code">
						xtw.WriteRaw(n.OuterXml);
					}
					else 
					{
						xtw.WriteRaw(summaryNav.Value);
					}
					xtw.WriteEndElement();
				}
			}

			// attributes
			if (memberTypeHt.ContainsKey("attribute"))
			{
				navTable = (Hashtable)memberTypeHt["attribute"];
				StringBuilder sb = new StringBuilder("This type has the following attributes: ");

				bool first = true;
				foreach(string memberId in navTable.Keys)
				{
					if (!first) { sb.Append(", "); first = false; }
					XPathNavigator nav2 = (XPathNavigator)navTable[memberId];
					//this.DumpNavTree(nav2, "    ");
					string tmps = ((XPathNavigator)navTable[memberId]).GetAttribute("name", "");
					sb.Append(tmps);
				}
				xtw.WriteElementString("p", sb.ToString());
			}

			// documentation/remarks
			if (remarksNav != null)
			{
				xtw.WriteElementString("h4", "Remarks");

				// want the XmlNode if possible (depends on whether nav came from
				// XmlDocument or XPathDocument).
				// Can't seem to get raw xml from the navigator in the XPathDocument case.
				// Don't know another way to test the cast, and it must be slow on exception
				XmlNode n = null;
				try
				{
					n = ((IHasXmlNode)remarksNav).GetNode();
				}
				catch(Exception) {}

				// write it
				xtw.WriteStartElement("p");
				if (n != null) 
				{
					FixCodeNodes(n); // change <code> to <pre class="code">
					xtw.WriteRaw(n.InnerXml);
				}
				else 
				{
					xtw.WriteRaw(remarksNav.Value);
				}
				xtw.WriteEndElement();
			}

			// Types which use name/access/summary table
			foreach(string memberType in orderedMemberTypes)
			{
				if (memberTypeHt.ContainsKey(memberType))
				{
					string capsMemberType = char.ToUpper(memberType[0]) + memberType.Substring(1);
					xtw.WriteElementString("h4", String.Format("{0} Members", capsMemberType));

					string[] columnNames = { "Name", "Access", "Summary" };
					StartTable(xtw, memberType + "_TableId_" + nodeName, 600, columnNames);

					//
					// create a table entry for each member of this Type
					//
					navTable = (Hashtable)memberTypeHt[memberType]; // memberId -> navigator

					// sort by member id (approximately the member name?)
					SortedList sortedMemberIds = new SortedList(navTable);

					foreach(string memberId in sortedMemberIds.Keys) // navTable.Keys
					{
						XPathNavigator nav2 = (XPathNavigator)navTable[memberId];
						string access = nav2.GetAttribute("access", "");
						string memberName = nav2.GetAttribute("name", "");
						string typeName = nav2.GetAttribute("type", "");
						string typeBaseName = TypeBaseName(typeName);
						string declaringType = nav2.GetAttribute("declaringType", "");
						XPathNavigator summaryNav = GetChildNodeOfType(nav2, "summary");
						//DumpNavTree(summaryNav, "    ");

						//
						// create a string for the name column
						//
						string nameString = memberName;
						switch(memberType)
						{
							case "field":
								nameString = memberName  + " : " + typeBaseName;
								break;
							case "property":
								nameString = memberName  + " : " + typeBaseName;
								break;
							case "method":
								typeBaseName = TypeBaseName(nav2.GetAttribute("returnType", ""));
								nameString = memberName  + "()" + " : " + typeBaseName;
								break;
							case "constructor":
								nameString = nodeName + "()";
								break;
						}

						//
						// write the member if it isn't from System.Object
						//
						if (!declaringType.Equals("System.Object"))
						{
							xtw.WriteStartElement("TR");
							xtw.WriteElementString("TD", nameString);
							xtw.WriteElementString("TD", access);

							if (declaringType.Length > 0)
							{
								//xtw.WriteElementString("TD", "<em>(from " + declaringType + ")</em> " + summaryNav.Value);

								// declared by an ancestor
								xtw.WriteStartElement("TD");
								xtw.WriteElementString("em", "(from " + declaringType + ")");
								xtw.WriteString(" ");
								if (summaryNav != null) xtw.WriteString(summaryNav.Value);
								xtw.WriteEndElement();
							}
							else
							{
								if (summaryNav != null)
									xtw.WriteElementString("TD", summaryNav.Value);
								else xtw.WriteElementString("TD", " ");
							}

							xtw.WriteEndElement();
						}
					}

					EndTable(xtw);
				}
			}
		}

		#endregion

		#region EmitHtml

		/// <summary>
		/// This writes the html corresponding to the xml we've already
		/// internalized.
		/// </summary>
		/// <param name="fileName">The name of the file to write to.</param>
		/// <returns></returns>
		private bool EmitHtml(string fileName)
		{
			StreamWriter sw = File.CreateText(fileName);
			Stream fs = sw.BaseStream;

			// doc head
			XmlTextWriter topWriter = new XmlTextWriter(fs, Encoding.UTF8);
			//topWriter.Formatting = Formatting.Indented;
			topWriter.Indentation = 4;
			//topWriter.WriteRaw("<html dir=\"LTR\">\n");
			topWriter.WriteStartElement("html");
			topWriter.WriteAttributeString("dir", "LTR");

			topWriter.WriteStartElement("head");

			topWriter.WriteElementString("title", "Example");
			topWriter.WriteRaw("	<META http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\">\n");
			topWriter.WriteRaw("		<meta name=\"vs_targetSchema\" content=\"http://schemas.microsoft.com/intellisense/ie5\">\n	");
			//		<xml></xml><link rel="stylesheet" type="text/css" href="MSDN.css">
			topWriter.WriteRaw("		<LINK rel=\"stylesheet\" href=\"LinearHtml.css\" type=\"text/css\">\n");
			topWriter.WriteEndElement(); // head
			//topWriter.WriteRaw("	<body>\n");
			topWriter.WriteStartElement("body");
			topWriter.WriteString(" "); // to close previous start, because of interleaved writers to same stream
			topWriter.Flush();

			// namespace list
			MemoryStream ms = (MemoryStream)namespaceListWriter.BaseStream;
			ms.Position = 0;
			fs.Write(ms.GetBuffer(), 0, (int)ms.Length);
			fs.Flush();

			// namespace section header
			topWriter.WriteElementString("h1", "Namespace Specifications");
			topWriter.Flush();

			// namespaces
			XmlTextWriter xtw;
			foreach(string namespaceName in namespaceWriters.Keys)
			{
				Hashtable nsSectionWriters = (Hashtable)namespaceWriters[namespaceName];

				foreach(string sectionName in namespaceSections.Values)
				{
					xtw = null;
					if (nsSectionWriters.ContainsKey(sectionName))
					{
						// so something was written to this section
						xtw = (XmlTextWriter)nsSectionWriters[sectionName];
						xtw.Flush();

						// copy to output stream
						ms = (MemoryStream)xtw.BaseStream;
						ms.Position = 0;
						fs.Write(ms.GetBuffer(), 0, (int)ms.Length);
					}
					else
					{
						// nothing in this section
					}
				}
			}

			// doc close
			topWriter.WriteEndElement(); // body
			topWriter.WriteEndElement(); // html
			topWriter.Flush();

			fs.Close();
			return(true);
		}

		#endregion

		#region Utility Methods

		/// <summary>
		/// Return the base of the input type name.  For example the bsae of 
		/// System.String is String.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		private string TypeBaseName(string typeName)
		{
			if (typeName.IndexOf(".") >= 0)
				return(typeName.Substring(typeName.LastIndexOf(".") + 1));
			else return(typeName);
		}

		/// <summary>
		/// Fix code node such that it will be rendered correctly (using pre).
		/// </summary>
		/// <param name="topNode"></param>
		private void FixCodeNodes(XmlNode topNode)
		{
			foreach(XmlNode codeNode in topNode.SelectNodes("descendant::code"))
			{
				codeNode.InnerXml = "<pre class=\"code\">" + codeNode.InnerXml + "</pre>";
			}
		}

/* doesn't work because can't modify node via XPathNavigator
		/// <summary>
		/// Fix code nodes such that it will be rendered correctly (using pre).
		/// </summary>
		/// <param name="topNode"></param>
		private void FixCodeNodes(XPathNavigator nav)
		{
			XPathNodeIterator iter = nav.SelectDescendants("code", string.Empty, true);
			while(iter.MoveNext())
			{
				XPathNavigator n = iter.Current;
				n.Value = "<pre class=\"code\">" + n.Value + "</pre>";
			}
		}
*/

		/// <summary>
		/// Return a new XPathNavigator pointing to the specified node. This just
		/// finds the first node of matching name.
		/// </summary>
		/// <param name="nodeName">The node name string.</param>
		/// <param name="startNavigator">Initial node to start search from.</param>
		/// <returns>An XPathNavigator pointing to the specified child, or null
		/// for not found.</returns>
		XPathNavigator GetChildNodeOfType(XPathNavigator startNavigator, string nodeName)
		{
			XPathNodeIterator xni = startNavigator.SelectDescendants(nodeName, "", false);
			xni.MoveNext();
			if (xni.Current.ComparePosition(startNavigator) == XmlNodeOrder.Same)
				return(null);
			else return(xni.Current);
		}

		/// <summary>
		/// For debugging, display the node local names starting from
		/// a particular node.
		/// </summary>
		/// <param name="nav">The start point.</param>
		/// <param name="prefix">An indentation prefix for the display.</param>
		void DumpNavTree(XPathNavigator nav, string prefix)
		{
			XPathNavigator n = nav.Clone();
			Console.WriteLine("{0} {1}", prefix, n.LocalName);

			// display children of specified node, recursively
			n.MoveToFirstChild();
			do
			{
				if (n.HasChildren) DumpNavTree(n, prefix + "    ");
			} while(n.MoveToNext());
		}

		#endregion

		#region Main

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		public static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: <input file> <output file>");
				return;
			}

			string fileName = args[0];
			string outFileName = args[1];
			Console.WriteLine("Starting for file {0}, output {1}", fileName, outFileName);
			LinearHtmlDocumenter ld = new LinearHtmlDocumenter();
			ld.Load(fileName);
			ld.MakeHtml(outFileName);
		}

		#endregion
	}
}
