// Project.cs - project management code 
// Copyright (C) 2001  Jason Diamond
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
using System.Reflection;
using System.Xml;
using System.Text;

namespace NDoc.Core
{
	/// <summary>Represents an NDoc project.</summary>
	public class Project
	{
		/// <summary>Initializes a new instance of the Project class.</summary>
		public Project()
		{
			_IsDirty = false;
			_namespaces = new SortedList();
			_probePath = new ArrayList();
		}

		private bool _IsDirty;
		private SortedList _namespaces;
		private string _projectFile;

		/// <summary>
		/// Holds the list of directories that will be scanned for documenters.
		/// </summary>
		private ArrayList _probePath;

		/// <summary>Gets the IsDirty property.</summary>
		public bool IsDirty
		{
			get { return _IsDirty; }

			set
			{
				if (!_IsDirty && value && Modified != null)
				{
					_IsDirty = true;
					Modified(this, new EventArgs());
				}
				else
				{
					_IsDirty = value;
				}
			}
		}

		private ArrayList _AssemblySlashDocs = new ArrayList();

		//		/// <summary>Gets or sets the AssemblySlashDocs property.</summary>
		//		private ArrayList AssemblySlashDocs
		//		{
		//			get { return _AssemblySlashDocs; }
		//		}

		/// <summary>
		/// A custom exception to detect if a duplicate assembly is beeing added.
		/// </summary>
		public class AssemblyAlreadyExistsException : ApplicationException
		{
			/// <summary>Initializes a new instance of the AssemblyAlreadyExistsException 
			/// class with a specified error message.</summary>
			public AssemblyAlreadyExistsException(string message) : base(message)
			{}
		}

		/// <summary>Adds an assembly/doc pair to the project.</summary>
		public void AddAssemblySlashDoc(AssemblySlashDoc assemblySlashDoc)
		{
			if (FindAssemblySlashDoc(assemblySlashDoc))
			{
				throw new AssemblyAlreadyExistsException("Assembly already exists.");
			}

			IsDirty = true;

			AddNamespacesFromAssembly(assemblySlashDoc.AssemblyFilename);
			_AssemblySlashDocs.Add(assemblySlashDoc);
		}

		private bool FindAssemblySlashDoc(AssemblySlashDoc assemblySlashDoc)
		{
			foreach (AssemblySlashDoc a in this._AssemblySlashDocs)
			{
				if (Path.GetFullPath(a.AssemblyFilename) == Path.GetFullPath(assemblySlashDoc.AssemblyFilename))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>Returns the index of the assembly/doc based on an
		/// assembly name.</summary>
		/// <param name="assemblyName">The assembly to search for.</param>
		/// <returns></returns>
		public int FindAssemblySlashDocByName(string assemblyName)
		{
			int count = 0;
			foreach (AssemblySlashDoc a in this._AssemblySlashDocs)
			{
				if (a.AssemblyFilename == assemblyName)
				{
					return count;
				}
				count++;
			}
			return -1;
		}

		/// <summary>Gets an assembly/doc pair.</summary>
		public AssemblySlashDoc GetAssemblySlashDoc(int index)
		{
			return _AssemblySlashDocs[index] as AssemblySlashDoc;
		}

		/// <summary>Gets an enumerable list of assembly/doc pairs.</summary>
		public IEnumerable GetAssemblySlashDocs()
		{
			return _AssemblySlashDocs;
		}

		/// <summary>Gets the number of assembly/doc pairs in the project.</summary>
		public int AssemblySlashDocCount
		{
			get { return _AssemblySlashDocs.Count; }
		}

		/// <summary>Removes an assembly/doc pair from the project.</summary>
		public void RemoveAssemblySlashDoc(int index)
		{
			_AssemblySlashDocs.RemoveAt(index);
			IsDirty = true;
		}

		/// <summary>
		/// Gets the base directory used for relative references.
		/// </summary>
		/// <value>
		/// The directory of the project file, or the current working directory 
		/// if the project was not loaded from a project file.
		/// </value>
		public string BaseDirectory {
			get 
			{ 
				if (_projectFile == null) {
					_projectFile = Directory.GetCurrentDirectory();
				}
				return Path.GetDirectoryName(_projectFile);
			}
		}

		/// <summary>
		/// Combines the specified path with the <see cref="BaseDirectory"/> of 
		/// the <see cref="Project" /> to form a full path to file or directory.
		/// </summary>
		/// <param name="path">The relative or absolute path.</param>
		/// <returns>
		/// A rooted path.
		/// </returns>
		public string GetFullPath(string path) {
			if (!Path.IsPathRooted(path)) {
				path = Path.GetFullPath(Path.Combine(BaseDirectory, path));
			}

			return path;
		}

		/// <summary>Sets a namespace summary.</summary>
		public void SetNamespaceSummary(string namespaceName, string summary)
		{
			//ignore a namespace that is not already in the collecton
			if (_namespaces.ContainsKey(namespaceName))
			{
				//set dirty only if the value changed
				if (summary.Length == 0)
				{
					summary = null;
				}
				if ((string)_namespaces[namespaceName] != summary)
				{
					_namespaces[namespaceName] = summary;
					IsDirty = true;
				}
			}
		}

		/// <summary>Gets the summary for a namespace.</summary>
		public string GetNamespaceSummary(string namespaceName)
		{
			return (string)_namespaces[namespaceName];
		}

		/// <summary>Gets an enumerable list of namespace names.</summary>
		public IEnumerable GetNamespaces()
		{
			return _namespaces.Keys;
		}

		/// <summary>The number of namespaces in the project.</summary>
		public int NamespaceCount
		{
			get { return _namespaces.Count; }
		}

		// enumerates the namespaces from an assembly 
		// and add them to the project if new
		private void AddNamespacesFromAssembly(string assemblyFile)
		{
			Assembly a = BaseDocumenter.LoadAssembly(assemblyFile);
			foreach (Type t in a.GetTypes())
			{
				string ns = t.Namespace;
				if ((ns != null) && (!_namespaces.ContainsKey(ns)))
				{
					_namespaces.Add(ns, null);
				}
			}
		}

		private ArrayList _Documenters;

		/// <summary>
		/// Gets the list of available documenters.
		/// </summary>
		public ArrayList Documenters
		{
			get
			{
				if (_Documenters == null)
				{
					_Documenters = FindDocumenters();
				}
				return _Documenters;
			}
		}

		/// <summary>
		/// Appends the specified directory to the probe path.
		/// </summary>
		/// <param name="path">The directory to add to the probe path.</param>
		/// <exception cref="ArgumentNullException"><paramref name="path" /> is <see langword="null" />.</exception>
		/// <exception cref="ArgumentException"><paramref name="path" /> is a zero-length <see cref="string" />.</exception>
		/// <remarks>
		/// <para>
		/// The probe path is the list of directories that will be scanned for
		/// assemblies that have classes implementing <see cref="IDocumenter" />.
		/// </para>
		/// </remarks>
		public void AppendProbePath(string path) 
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}

			if (path.Length == 0)
			{
				throw new ArgumentException("A zero-length string is not a valid value.", "path");
			}

			// resolve relative path to full path
			string fullPath = GetFullPath(path);

			if (!_probePath.Contains(fullPath)) 
			{
				_probePath.Add(fullPath);
			}
		}

		/// <summary>
		/// Searches the module directory and all directories in the probe path
		/// for assemblies containing classes that implement <see cref="IDocumenter" />.
		/// </summary>
		/// <returns>
		/// An <see cref="ArrayList" /> containing new instances of all the 
		/// found documenters.
		/// </returns>
		private ArrayList FindDocumenters()
		{
			ArrayList documenters = new ArrayList();

#if MONO //System.Windows.Forms.Application.StartupPath is not implemented in mono v0.31
			string mainModuleDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
#else
			string mainModuleDirectory = System.Windows.Forms.Application.StartupPath;
#endif
			// make sure module directory is probed
			AppendProbePath(mainModuleDirectory);

			// scan all assemblies in probe path for documenters
			foreach (string path in _probePath) 
			{
				// find documenters in given path
				FindDocumentersInPath(documenters, path);
			}

			// sort documenters
			documenters.Sort();

			return documenters;
		}

		/// <summary>
		/// Searches the specified directory for assemblies containing classes 
		/// that implement <see cref="IDocumenter" />.
		/// </summary>
		/// <param name="documenters">The collection of <see cref="IDocumenter" /> instances to fill.</param>
		/// <param name="path">The directory to scan for assemblies containing documenters.</param>
		private static void FindDocumentersInPath(ArrayList documenters, string path) 
		{
			foreach (string fileName in Directory.GetFiles(path, "NDoc.Documenter.*.dll")) 
			{
				Assembly assembly = null;

				try
				{
					assembly = Assembly.LoadFrom(fileName);
				}
				catch (BadImageFormatException) 
				{
					// The DLL must not be a .NET assembly.
					// Don't need to do anything since the
					// assembly reference should still be null.
					Debug.WriteLine("BadImageFormatException loading " + fileName);
				}

				if (assembly != null) 
				{
					try 
					{
						foreach (Type type in assembly.GetTypes()) 
						{
							if (type.IsClass && !type.IsAbstract && (type.GetInterface("NDoc.Core.IDocumenter") != null))
							{
								documenters.Add(Activator.CreateInstance(type));
							}
						}
					}
					catch (ReflectionTypeLoadException) 
					{
						// eat this exception and just ignore this assembly
						Debug.WriteLine("ReflectionTypeLoadException reflecting " + fileName);
					}
				}
			}
		}

		/// <summary>Reads an NDoc project file.</summary>
		public void Read(string filename)
		{
			_projectFile = Path.GetFullPath(filename);

			Clear();

			XmlTextReader reader = null;

			// keep track of whether or not any assemblies fail to load
			CouldNotLoadAllAssembliesException assemblyLoadException = null;

			// keep track of whether or not any errors in documenter property values
			DocumenterPropertyFormatException documenterPropertyFormatExceptions = null;

			try
			{
				StreamReader streamReader = new StreamReader(filename);
				reader = new XmlTextReader(streamReader);

				reader.MoveToContent();
				reader.ReadStartElement("project");

				while (!reader.EOF)
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.Name)
						{
							case "assemblies":
								// continue even if we don't load all assemblies
								try
								{
									ReadAssemblySlashDocs(reader);
								}
								catch (CouldNotLoadAllAssembliesException e)
								{
									assemblyLoadException = e;
								}
								break;
							case "namespaces":
								ReadNamespaceSummaries(reader);
								break;
							case "documenters":
								// continue even if we have errors in documenter properties
								try
								{
								ReadDocumenters(reader);
								}
								catch (DocumenterPropertyFormatException e)
								{
									documenterPropertyFormatExceptions = e;
								}
								break;
							default:
								reader.Read();
								break;
						}
					}
					else
					{
						reader.Read();
					}
				}
			}
			catch (Exception ex)
			{
				throw new DocumenterException("Error reading in project file " 
					+ filename  + ".\n" + ex.Message, ex);

				// Set all the documenters to a default state since unable to load them.
			}
			finally
			{
				if (reader != null)
				{
					reader.Close(); // Closes the underlying stream.
				}
			}

			if (assemblyLoadException != null)
			{
				throw assemblyLoadException;
			}

			if (documenterPropertyFormatExceptions != null)
			{
				throw documenterPropertyFormatExceptions;
			}

			IsDirty = false;
		}

		private void ReadAssemblySlashDocs(XmlReader reader)
		{
			int count = 0;

			// keep a list of slash-docs which we fail to load
			ArrayList failedDocs = new ArrayList();
			// keep a list of load exceptions.
			ArrayList loadExceptions = new ArrayList();

			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "assemblies"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "assembly")
				{
					AssemblySlashDoc assemblySlashDoc = new AssemblySlashDoc();

					if (reader.GetAttribute("location") == null) 
					{
						throw new DocumenterException("\"location\" attribute is"
							+ " required for <assembly> element in project file.");
					}
					if (reader.GetAttribute("location").Trim().Length == 0) {
						throw new DocumenterException("\"location\" attribute of"
							+ " <assembly> element cannot be empty in project file.");
					}
					assemblySlashDoc.AssemblyFilename = GetFullPath(reader["location"]);

					if (reader.GetAttribute("documentation") == null) {
						throw new DocumenterException("\"documentation\" attribute is"
							+ " required for <assembly> element in project file.");
					}
					if (reader.GetAttribute("documentation").Trim().Length == 0) {
						throw new DocumenterException("\"documentation\" attribute of"
							+ " <assembly> element cannot be empty in project file.");
					}
					assemblySlashDoc.SlashDocFilename = GetFullPath(reader["documentation"]);
					count++;
					try
					{
						AddAssemblySlashDoc(assemblySlashDoc);
					}
					catch(FileNotFoundException e)
					{
						failedDocs.Add(assemblySlashDoc);
						loadExceptions.Add(e);
					}
				}
				reader.Read();
			}
			if (failedDocs.Count>0)
			{
				StringBuilder sb = new StringBuilder();
				for(int i=0;i<failedDocs.Count;i++)
				{
					FileNotFoundException LoadException=(FileNotFoundException)loadExceptions[i];
					sb.Append( LoadException.Message  + "\n");
					sb.Append( LoadException.FusionLog );
					sb.Append("\n");
				}
				throw new CouldNotLoadAllAssembliesException(sb.ToString());
			}
		}

		/// <summary>
		/// Loads namespace summaries from an XML document.
		/// </summary>
		/// <param name="reader">
		/// An open XmlReader positioned before the namespace elements.</param>
		public void ReadNamespaceSummaries(XmlReader reader)
		{
			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "namespaces"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "namespace")
				{
					string name = reader["name"];
					string summary = reader.ReadInnerXml();

					//assume that the assemblies are read first
					//and ignore summaries from unknown namespaces
					if (_namespaces.ContainsKey(name))
					{
						_namespaces[name] = summary;
					}

					// Reader cursor already moved to next node.
				}
				else
				{
					reader.Read();
				}
			}
		}

		private void ReadDocumenters(XmlReader reader)
		{
			string FailureMessages="";

			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "documenters"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "documenter")
				{
					string name = reader["name"];
					IDocumenter documenter = GetDocumenter(name);

					if (documenter != null)
					{
						reader.Read(); // Advance to next node.
						try
						{
						documenter.Config.Read(reader);
					}
						catch (DocumenterPropertyFormatException e)
						{
							FailureMessages += name + " Documenter\n" + e.Message + "\n";
						}
					}
				}
				reader.Read();
			}

			if (FailureMessages.Length > 0)
				throw new DocumenterPropertyFormatException(FailureMessages);

		}

		/// <summary>Retrieves a documenter by name.</summary>
		public IDocumenter GetDocumenter(string name)
		{
			foreach (IDocumenter documenter in Documenters)
			{
				if (documenter.Name == name)
				{
					return documenter;
				}
			}

			return null;
		}

		/// <summary>Writes an NDoc project file.</summary>
		public void Write(string filename)
		{
			XmlTextWriter writer = null;

			try
			{
				StreamWriter streamWriter = new StreamWriter(filename);
				writer = new XmlTextWriter(streamWriter);
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 4;

				writer.WriteStartElement("project");
				writer.WriteAttributeString("SchemaVersion","1.3");

				//do not change the order of those lines
				WriteAssemblySlashDocs(writer);
				WriteNamespaceSummaries(writer);
				WriteDocumenters(writer);

				writer.WriteEndElement();
			}
			catch (Exception ex)
			{
				throw new DocumenterException("Error saving project file "
					+ ".\n" + ex.Message, ex);
			}
			finally
			{
				if (writer != null)
				{
					writer.Close(); // Closes the underlying stream.
				}
			}

			IsDirty = false;
		}

		private void WriteAssemblySlashDocs(XmlWriter writer)
		{
			if (_AssemblySlashDocs.Count > 0)
			{
				writer.WriteStartElement("assemblies");

				foreach (AssemblySlashDoc assemblySlashDoc in _AssemblySlashDocs)
				{
					writer.WriteStartElement("assembly");
					writer.WriteAttributeString("location", assemblySlashDoc.AssemblyFilename);
					writer.WriteAttributeString("documentation", assemblySlashDoc.SlashDocFilename);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}
		}

		private void WriteNamespaceSummaries(XmlWriter writer)
		{
			if (_namespaces.Count > 0)
			{
				writer.WriteStartElement("namespaces");

				foreach (string ns in _namespaces.Keys)
				{
					writer.WriteStartElement("namespace");
					writer.WriteAttributeString("name", ns);
					writer.WriteRaw((string)_namespaces[ns]);
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}
		}

		private void WriteDocumenters(XmlWriter writer)
		{
			if (Documenters.Count > 0)
			{
				writer.WriteStartElement("documenters");

				foreach (IDocumenter documenter in Documenters)
				{
					documenter.Config.Write(writer);
				}

				writer.WriteEndElement();
			}
		}

		/// <summary>Clears the project.</summary>
		public void Clear()
		{
			_AssemblySlashDocs.Clear();
			_namespaces.Clear();

			foreach (IDocumenter documenter in Documenters)
			{
				documenter.Clear();
			}

			IsDirty = false;
		}

		/// <summary>Raised by projects when they're dirty state changes from false to true.</summary>
		public event ProjectModifiedEventHandler Modified;
	}

	/// <summary>Handles ProjectModified events.</summary>
	public delegate void ProjectModifiedEventHandler(object sender, EventArgs e);

	/// <summary>
	/// This exception is thrown when one or more assemblies can not be loaded.
	/// </summary>
	[Serializable]
	public class CouldNotLoadAllAssembliesException : ApplicationException
	{ 
		/// <summary/>
		public CouldNotLoadAllAssembliesException() { }

		/// <summary/>
		public CouldNotLoadAllAssembliesException(string message)
			: base(message) { }

		/// <summary/>
		public CouldNotLoadAllAssembliesException(string message, Exception inner)
			: base(message, inner) { }

		/// <summary/>
		protected CouldNotLoadAllAssembliesException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context
			) : base (info, context) { }
	}
	/// <summary>
	/// This exception is thrown when there were invalid values in the documenter properties.
	/// </summary>
	[Serializable]
	public class DocumenterPropertyFormatException : ApplicationException
	{ 
		/// <summary/>
		public DocumenterPropertyFormatException() { }

		/// <summary/>
		public DocumenterPropertyFormatException(string message)
			: base(message) { }

		/// <summary/>
		public DocumenterPropertyFormatException(string message, Exception inner)
			: base(message, inner) { }

		/// <summary/>
		protected DocumenterPropertyFormatException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context
			) : base (info, context) { }
	}
}