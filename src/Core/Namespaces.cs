// Project.cs - project management code 
// Copyright (C) 2004  Kevin Downs
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
using System.Xml;
using System.Reflection;
using System.IO;

namespace NDoc.Core
{
	/// <summary>
	/// Summary description for Namespaces.
	/// </summary>
	public class Namespaces
	{
		private SortedList _namespaces;

		
		/// <summary>
		/// Allows a Namespaces collection to be treated as a SortedList
		/// </summary>
		/// <param name="namespaces">The Namespaces object to convert.</param>
		/// <returns></returns>
		public static implicit operator SortedList(Namespaces namespaces)
		{
			return namespaces._namespaces;
		}

		/// <summary>
		/// Raised when contents of collection change
		/// </summary>
		public event EventHandler ContentsChanged;

		private void OnContentsChanged()
		{
			if (ContentsChanged!=null) ContentsChanged(this,EventArgs.Empty);
		}


		/// <summary>
		/// Creates a new <see cref="Namespaces"/> instance.
		/// </summary>
		public Namespaces()
		{
		}

		/// <summary>
		/// Gets or sets the namespace summary with the specified namespace name.
		/// </summary>
		/// <value></value>
		public string this[string namespaceName]
		{
			get
			{
				if (_namespaces==null)
					return "";
				else
					return (string)_namespaces[namespaceName];
			}

			set
			{
				   //ignore a namespace that is not already in the collecton
				   if (_namespaces.ContainsKey(namespaceName))
				   {
					   if (value.Length == 0)
					   {
						   value = null;
					   }

					   //throw ContentsChanged if the value changed
					   if ((string)_namespaces[namespaceName] != value)
					   {
						   _namespaces[namespaceName] = value;
						   OnContentsChanged();
					   }
				   }
			   }
		}

		/// <summary>Gets an enumerable list of namespace names.</summary>
		public IEnumerable NamespaceNames
		{
			get
			{
				if (Count>0)
					return _namespaces.Keys;
				else
					return new ArrayList();
			}
		}

		/// <summary>The number of namespaces in the collection.</summary>
		public int Count
		{
			get 
			{
				if (_namespaces!=null)
				{
					return _namespaces.Count;
				}
				else
				{
					return 0;
				}
			}
		}
		/// <summary>
		/// Reads namespace summaries from an XML document.
		/// </summary>
		/// <param name="reader">
		/// An open XmlReader positioned before the namespace elements.</param>
		public void Read(XmlReader reader)
		{
			bool IsDirty=false;
			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "namespaces"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "namespace")
				{
					if (_namespaces==null) 
						_namespaces = new SortedList();

					string name = reader["name"];
					string summary = reader.ReadInnerXml();

					//ignore duplicate summaries
					if (!_namespaces.ContainsKey(name))
					{
						_namespaces[name] = summary;
						IsDirty=true;
					}
					// Note:ReadInnerXml moved Reader cursor to next node.
				}
				else
				{
					reader.Read();
				}
			}
			if (IsDirty) OnContentsChanged();
		}

		/// <summary>
		/// Writes namespace summaries to an XML document.
		/// </summary>
		/// <param name="writer">
		/// An open XmlWriter.</param>
		public void Write(XmlWriter writer)
		{
			if (_namespaces!=null && _namespaces.Count > 0)
			{
				//do a quick check to make sure there are some namespace summaries
				//if not, we don't need to write this section out
				bool summariesExist=false;
				foreach (string ns in _namespaces.Keys)
				{
					string summary = (string)_namespaces[ns];
					if (summary!=null && summary.Length>0) summariesExist = true;
				}

				if (summariesExist)
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
		}

		public void LoadNamespacesFromAssemblies(Project project)
		{
			//let's try to create this in a new AppDomain
			AppDomain appDomain=null;
			try
			{
				appDomain = AppDomain.CreateDomain("NDocNamespaces");
				ReflectionEngine re = (ReflectionEngine)
					appDomain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().CodeBase, 
					"NDoc.Core.ReflectionEngine");
				ReflectionEngineParameters rep = new ReflectionEngineParameters(project);
				foreach (AssemblySlashDoc assemblySlashDoc in project.GetAssemblySlashDocs())
				{
					if(assemblySlashDoc.AssemblyFilename!=null && assemblySlashDoc.AssemblyFilename.Length>0)
					{
						string assemblyFullPath = project.GetFullPath(assemblySlashDoc.AssemblyFilename);
						if(File.Exists(assemblyFullPath))
						{
							SortedList namespaces = re.GetNamespacesFromAssembly(rep,assemblyFullPath);
							foreach(string ns in namespaces.GetKeyList())
							{
								if (_namespaces==null)
									_namespaces = new SortedList();
								if ((!_namespaces.ContainsKey(ns)))
								{
									_namespaces.Add(ns, null);
								}
							}
						}
					}
				}
			}
			finally
			{
				if (appDomain!=null) AppDomain.Unload(appDomain);
			}

		}
	}
}
