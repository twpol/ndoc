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
			_Documenters = FindDocumenters();
			_IsDirty = false;
			_namespaces = new SortedList();
		}

		private bool _IsDirty;
		private SortedList _namespaces;

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
		/// <returns>bool - true for doc added, false or exception otherwise</returns>
		public bool AddAssemblySlashDoc(AssemblySlashDoc assemblySlashDoc)
		{
			bool ret = true; // assume success, set otherwise
			if (FindAssemblySlashDoc(assemblySlashDoc))
			{
				throw new AssemblyAlreadyExistsException("Assembly already exists.");
			}

			try
			{
				AddNamespacesFromAssembly(assemblySlashDoc.AssemblyFilename);
				_AssemblySlashDocs.Add(assemblySlashDoc);
			}
			catch (FileNotFoundException) 
			{
				ret = false;
			}
			finally
			{
				IsDirty = true;
			}
			return(ret);
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

		private ArrayList _Documenters = new ArrayList();

		/// <summary>Gets or sets the Documenters property.</summary>
		public ArrayList Documenters
		{
			get { return _Documenters; }
		}

		/// <summary>Searches the module directory for assemblies containing classes the implement IDocumenter.</summary>
		/// <returns>An ArrayList containing new instances of all the found documenters.</returns>
		public static ArrayList FindDocumenters()
		{
			ArrayList documenters = new ArrayList();

			string mainModuleDirectory = System.Windows.Forms.Application.StartupPath;

			foreach (string fileName in Directory.GetFiles(mainModuleDirectory, "NDoc.Documenter.*.dll"))
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

			documenters.Sort();

			return documenters;
		}

		/// <summary>Reads an NDoc project file.</summary>
		public void Read(string filename)
		{
			Clear();

			XmlTextReader reader = null;

			// keep track of whether or not any assemblies fail to load
			CouldNotLoadAllAssembliesException assemblyLoadException = null;

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
								ReadDocumenters(reader);
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

			IsDirty = false;
		}

		private void ReadAssemblySlashDocs(XmlReader reader)
		{
			int count = 0;

			// keep a list of slash-docs which we fail to load
			ArrayList failedDocs = new ArrayList();

			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "assemblies"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "assembly")
				{
					AssemblySlashDoc assemblySlashDoc = new AssemblySlashDoc();
					assemblySlashDoc.AssemblyFilename = reader["location"];
					assemblySlashDoc.SlashDocFilename = reader["documentation"];
					count++;
					if (!AddAssemblySlashDoc(assemblySlashDoc))
					{
						failedDocs.Add(assemblySlashDoc);
					}
				}
				reader.Read();
			}
			if (count > AssemblySlashDocCount)
			{
				StringBuilder sb = new StringBuilder("One or more assemblies could not be loaded:\n");
				foreach(AssemblySlashDoc slashDoc in failedDocs)
					sb.Append(slashDoc.AssemblyFilename + "\n");
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
			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "documenters"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "documenter")
				{
					string name = reader["name"];
					IDocumenter documenter = GetDocumenter(name);

					if (documenter != null)
					{
						reader.Read(); // Advance to next node.
						documenter.Config.Read(reader);
					}
				}
				reader.Read();
			}
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
}