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
		}

		private bool _IsDirty;

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

		/// <summary>Gets or sets the AssemblySlashDocs property.</summary>
		private ArrayList AssemblySlashDocs
		{
			get { return _AssemblySlashDocs; }
		}

		/// <summary>Adds an assembly/doc pair to the project.</summary>
		public void AddAssemblySlashDoc(AssemblySlashDoc assemblySlashDoc)
		{
			_AssemblySlashDocs.Add(assemblySlashDoc);
			IsDirty = true;
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

		private Hashtable _NamespaceSummaries = new Hashtable();

		/// <summary>Gets or sets the NamespaceSummaries property.</summary>
		private Hashtable NamespaceSummaries
		{
			get { return _NamespaceSummaries; }
			set { _NamespaceSummaries = value; }
		}

		/// <summary>Adds a namespace summary to the project.</summary>
		public void SetNamespaceSummary(string namespaceName, string summary)
		{
			if (namespaceName != null)
			{
				if (summary != null && summary.Length > 0)
				{
					_NamespaceSummaries[namespaceName] = summary;
				}
				else
				{
					_NamespaceSummaries.Remove(namespaceName);
				}

				IsDirty = true;
			}
		}

		/// <summary>Gets the summary for a namespace.</summary>
		public string GetNamespaceSummary(string namespaceName)
		{
			string summary = null;

			if (namespaceName != null)
			{
				summary = _NamespaceSummaries[namespaceName] as string;
			}

			return summary;
		}

		/// <summary>Gets an enumerable list of namespace names with summaries.</summary>
		public IEnumerable GetNamespaceSummaries()
		{
			return _NamespaceSummaries.Keys;
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

			string mainModuleDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

			foreach (string fileName in Directory.GetFiles(mainModuleDirectory, "*.dll"))
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
				}

				if (assembly != null)
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (type.IsClass && !type.IsAbstract && (type.GetInterface("NDoc.Core.IDocumenter") != null))
						{
							documenters.Add(Activator.CreateInstance(type));
						}
					}
				}
			}
			
			documenters.Sort();
			
			return documenters;
		}

		/// <summary>Reads an XDP file.</summary>
		public void Read(string filename)
		{
			Clear();

			XmlTextReader reader = null;

			try
			{
				StreamReader streamReader = new StreamReader(filename);
				reader = new XmlTextReader(streamReader);

				reader.MoveToContent();
				reader.ReadStartElement("project");

				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.Name)
						{
							case "assemblies":
								ReadAssemblySlashDocs(reader);
								break;
							case "namespaces":
								ReadNamespaceSummaries(reader);
								break;
							case "documenters":
								ReadDocumenters(reader);
								break;
						}
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

			IsDirty = false;
		}

		private void ReadAssemblySlashDocs(XmlReader reader)
		{
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "assemblies"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "assembly")
				{
					AssemblySlashDoc assemblySlashDoc = new AssemblySlashDoc();
					assemblySlashDoc.AssemblyFilename = reader["location"];
					assemblySlashDoc.SlashDocFilename = reader["documentation"];
					AssemblySlashDocs.Add(assemblySlashDoc);
				}
			}
		}

		private void ReadNamespaceSummaries(XmlReader reader)
		{
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "namespaces"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "namespace")
				{
					string name = reader["name"];
					string summary = reader.ReadInnerXml();
					NamespaceSummaries[name] = summary;
				}
			}
		}

		private void ReadDocumenters(XmlReader reader)
		{
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "documenters"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "documenter")
				{
					string name = reader["name"];
					IDocumenter documenter = GetDocumenter(name);
					documenter.Config.Read(reader);
				}
			}
		}

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

		/// <summary>Writes an XDP file.</summary>
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

				WriteAssemblySlashDocs(writer);
				WriteNamespaceSummaries(writer);
				WriteDocumenters(writer);

				writer.WriteEndElement();
			}
			catch (Exception ex)
			{
				throw new DocumenterException("Error saving project file " 
					+ "xxxx.wdp" + ".\n" + ex.Message, ex);
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
			if (AssemblySlashDocs.Count > 0)
			{
				writer.WriteStartElement("assemblies");

				foreach (AssemblySlashDoc assemblySlashDoc in AssemblySlashDocs)
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
			if (NamespaceSummaries.Count > 0)
			{
				writer.WriteStartElement("namespaces");

				foreach (string ns in NamespaceSummaries.Keys)
				{
					writer.WriteStartElement("namespace");
					writer.WriteAttributeString("name", ns);
					writer.WriteRaw((string)NamespaceSummaries[ns]);
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
			AssemblySlashDocs.Clear();
			NamespaceSummaries.Clear();

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
}
