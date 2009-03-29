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
using System.Collections.Generic;
using System.Xml;
using System.IO;
using NDoc3.Core.Reflection;

namespace NDoc3.Core
{
    /// <summary>
    /// Summary description for Namespaces.
    /// </summary>
    public class Namespaces
    {
        private readonly IDictionary<string,string> _namespaces;

        /// <summary>
        /// Copy the list of namespaces to the given list.
        /// </summary>
        /// <param name="list"></param>
        public void CopyTo(IDictionary<string, string> list)
        {
            foreach (KeyValuePair<string, string> ns in _namespaces)
            {
                list.Add(ns.Key, ns.Value);
            }
        }

        //		/// <summary>
        //		/// Allows a Namespaces collection to be treated as a SortedList
        //		/// </summary>
        //		/// <param name="namespaces">The Namespaces object to convert.</param>
        //		/// <returns></returns>
        //		public static implicit operator SortedList(Namespaces namespaces)
        //		{
        //			return namespaces._namespaces;
        //		}

        /// <summary>
        /// Raised when contents of collection change
        /// </summary>
        public event EventHandler ContentsChanged;

        private void OnContentsChanged()
        {
            if (ContentsChanged != null) ContentsChanged(this, EventArgs.Empty);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Namespaces"/> class.
        /// </summary>
        public Namespaces()
        {
            _namespaces = new SortedStringDictionary();
        }

        /// <summary>
        /// Gets or sets the namespace summary with the specified namespace name.
        /// </summary>
        /// <value></value>
        public string this[string namespaceName]
        {
            get
            {
                return _namespaces[namespaceName];
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
                    if (_namespaces[namespaceName] != value)
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
                if (Count > 0)
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
                return _namespaces.Count;
            }
        }
        /// <summary>
        /// Reads namespace summaries from an XML document.
        /// </summary>
        /// <param name="reader">
        /// An open XmlReader positioned before the namespace elements.</param>
        public void Read(XmlReader reader)
        {
            bool IsDirty = false;
            while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "namespaces"))
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "namespace")
                {
                    string name = reader["name"];
                    string summary = reader.ReadInnerXml();

                    //ignore duplicate summaries
                    if (!_namespaces.ContainsKey(name))
                    {
                        _namespaces[name] = summary;
                        IsDirty = true;
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
            if (_namespaces != null && _namespaces.Count > 0)
            {
                //do a quick check to make sure there are some namespace summaries
                //if not, we don't need to write this section out
                bool summariesExist = false;
                foreach (KeyValuePair<string, string> ns in _namespaces)
                {
                    if (ns.Value != null && ns.Value.Length > 0) summariesExist = true;
                }

                if (summariesExist)
                {
                    writer.WriteStartElement("namespaces");

                    foreach (KeyValuePair<string, string> ns in _namespaces)
                    {
                        if (ns.Value != null && ns.Value.Length > 0)
                        {
                            writer.WriteStartElement("namespace");
                            writer.WriteAttributeString("name", ns.Key);
                            writer.WriteRaw((string)ns.Value);
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Loads the namespaces from assemblies.
        /// </summary>
        /// <param name="project">Project.</param>
        public void LoadNamespacesFromAssemblies(Project project)
        {
            //let's try to create this in a new AppDomain
            using (ReflectionEngine re = new ReflectionEngine(project.ReferencePaths))
            {
                foreach (AssemblySlashDoc assemblySlashDoc in project.AssemblySlashDocs)
                {
                    if (assemblySlashDoc.Assembly.Path.Length > 0)
                    {
                        FileInfo assemblyFullPath = new FileInfo(assemblySlashDoc.Assembly.Path);
                        if (assemblyFullPath.Exists)
                        {
                            string[] namespaces = re.GetNamespacesFromAssembly(assemblyFullPath);
                            foreach (string ns in namespaces)
                            {
                                if ((!_namespaces.ContainsKey(ns)))
                                {
                                    _namespaces.Add(ns, null);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
