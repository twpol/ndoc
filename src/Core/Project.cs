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
			_probePath = new ArrayList();
			_referencePaths = new ArrayList();
			_namespaces = new Namespaces();
			_namespaces.ContentsChanged += new EventHandler(ContentsChanged);
		}

		private bool _IsDirty;
		private string _projectFile;

		/// <summary>
		/// Holds the list of directories that will be scanned for documenters.
		/// </summary>
		private ArrayList _probePath;

		/// <summary>
		/// Holds the list of additional directories that will be probed when loading assemblies.
		/// </summary>
		private ArrayList _referencePaths;
		/// <summary>Gets an enumerable list of ReferencePaths.</summary>
		public IEnumerable GetReferencePaths()
		{
			return _referencePaths;
		}


		private void ContentsChanged(object sender, EventArgs e)
		{
			IsDirty = true;
		}

		/// <summary>Gets the IsDirty property.</summary>
		public bool IsDirty
		{
			get { return _IsDirty; }

			set
			{
				if (!_IsDirty && value)
				{
					_IsDirty = true;
					if (Modified != null) Modified(this, EventArgs.Empty);
				}
				else
				{
					_IsDirty = value;
				}
			}
		}

		private ArrayList _AssemblySlashDocs = new ArrayList();

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

			if (path != null && path.Length > 0)
			{
				if (!Path.IsPathRooted(path)) 
				{
				path = Path.GetFullPath(Path.Combine(BaseDirectory, path));
			}
			}

			return path;
		}

		/// <summary>
		/// Gets the relative path of the passed path with respect to the <see cref="BaseDirectory"/> of 
		/// the <see cref="Project" />.
		/// </summary>
		/// <param name="path">The relative or absolute path.</param>
		/// <returns>
		/// A relative path.
		/// </returns>
		public string GetRelativePath(string path) 
		{

			if (path != null && path.Length > 0)
			{
				if (Path.IsPathRooted(path)) 
				{
					path = AbsoluteToRelativePath(BaseDirectory, path);
				}
			}

			return path;
		}

		/// <summary>
		/// Converts an absolute path to one relative to the given base directory path
		/// </summary>
		/// <param name="basePath">The base directory path</param>
		/// <param name="absolutePath">An absolute path</param>
		/// <returns>A path to the given absolute path, relative to the base path</returns>
		public string AbsoluteToRelativePath(string basePath, string absolutePath)
		{
			char[] separators = {
									Path.DirectorySeparatorChar, 
									Path.AltDirectorySeparatorChar, 
									Path.VolumeSeparatorChar 
								};

			//split the paths into their component parts
			string[] basePathParts = basePath.Split(separators);
			string[] absPathParts = absolutePath.Split(separators);
			int indx = 0;

			//work out how much they have in common
			int minLength=Math.Min(basePathParts.Length, absPathParts.Length);
			for(; indx < minLength; ++indx)
			{
				if(!basePathParts[indx].Equals(absPathParts[indx]))
					break;
			}
			
			//if they have nothing in common, just return the absolute path
			if (indx == 0) 
			{
				return absolutePath;
			}
			
			
			//start constructing the relative path
			string relPath = "";
			
			if(indx == basePathParts.Length)
			{
				// the entire base path is in the abs path
				// so the rel path starts with "./"
				relPath += "." + Path.DirectorySeparatorChar;
			} 
			else 
			{
				//step up from the base to the common root 
				for (int i = indx; i < basePathParts.Length; ++i) 
				{
					relPath += ".." + Path.DirectorySeparatorChar;
				}
			}
			//add the path from the common root to the absPath
			relPath += String.Join(Path.DirectorySeparatorChar.ToString(), absPathParts, indx, absPathParts.Length-indx);
			
			return relPath;
		}

		private Namespaces _namespaces;

		/// <summary>
		/// Gets the project namespace summaries collection.
		/// </summary>
		/// <value></value>
		public Namespaces Namespaces 
		{
			get { return _namespaces; }
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
			Clear();

			_projectFile = Path.GetFullPath(filename);

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
							case "assemblies" : 
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
							case "referencePaths" : 
								ReadReferencePaths(reader);
								break;
							case "namespaces" : 
								//GetNamespacesFromAssemblies();
								Namespaces.Read(reader);
								break;
							case "documenters" : 
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
							default : 
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
					+ filename + ".\n" + ex.Message, ex);

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
					assemblySlashDoc.AssemblyFilename = reader["location"];
					assemblySlashDoc.SlashDocFilename = reader["documentation"];
					count++;
					try
					{
						AddAssemblySlashDoc(assemblySlashDoc);
					}
					catch (FileNotFoundException e)
					{
						failedDocs.Add(assemblySlashDoc);
						loadExceptions.Add(e);
					}
				}
				reader.Read();
			}
			if (failedDocs.Count > 0)
			{
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < failedDocs.Count; i++)
				{
					FileNotFoundException LoadException = (FileNotFoundException)loadExceptions[i];
					sb.Append(LoadException.Message + "\n");
					sb.Append(LoadException.FusionLog);
					sb.Append("\n");
				}
				throw new CouldNotLoadAllAssembliesException(sb.ToString());
			}
		}


		/// <summary>
		/// Loads reference paths from an XML document.
		/// </summary>
		/// <param name="reader">
		/// An open XmlReader positioned before the referencePath elements.</param>
		public void ReadReferencePaths(XmlReader reader)
		{
			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "referencePaths"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "referencePath")
				{
					string path = reader["path"];
					if (Directory.Exists(path))
					{
						_referencePaths.Add(path);
					}
				}
				reader.Read();
			}
		}

		private void ReadDocumenters(XmlReader reader)
		{
			string FailureMessages = "";

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
			_projectFile = Path.GetFullPath(filename);

			XmlTextWriter writer = null;

			try
			{
				StreamWriter streamWriter = new StreamWriter(filename);
				writer = new XmlTextWriter(streamWriter);
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 4;

				writer.WriteStartElement("project");
				writer.WriteAttributeString("SchemaVersion", "1.3");

				//do not change the order of those lines
				WriteAssemblySlashDocs(writer);
				WriteReferencePaths(writer);
				Namespaces.Write(writer);
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
					writer.WriteAttributeString("location", GetRelativePath(assemblySlashDoc.AssemblyFilename));
					writer.WriteAttributeString("documentation", GetRelativePath(assemblySlashDoc.SlashDocFilename));
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}
		}

		private void WriteReferencePaths(XmlWriter writer)
		{
			if (_referencePaths.Count > 0)
			{
				writer.WriteStartElement("referencePaths");

				foreach (string refPath in _referencePaths)
				{
					writer.WriteStartElement("referencePath");
					writer.WriteAttributeString("path", refPath);
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
			if (_namespaces != null) _namespaces = new Namespaces();

			foreach (IDocumenter documenter in Documenters)
			{
				documenter.Clear();
			}

			IsDirty = false;
			_projectFile = "";
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
) : base(info, context) { }
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
) : base(info, context) { }
	}
}
