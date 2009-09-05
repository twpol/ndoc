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
using System.Xml;
using System.Collections;


namespace NDoc3.Core {
	/// <summary>
	/// Event arguments class for events related to an AssemblySlashDoc
	/// </summary>
	public class AssemblySlashDocEventArgs : EventArgs {
		/// <summary>
		/// The AssemblySlashDoc
		/// </summary>
		public readonly AssemblySlashDoc AssemblySlashDoc;

		internal AssemblySlashDocEventArgs(AssemblySlashDoc assemblySlashDoc) {
			AssemblySlashDoc = assemblySlashDoc;
		}
	}

	/// <summary>
	/// Event handler delegate for AssemblySlashDoc related events
	/// </summary>
	public delegate void AssemblySlashDocEventHandler(object sender, AssemblySlashDocEventArgs args);

	/// <summary>
	/// Represents a collection of assemblies and their associated documentation comment XML files. 
	/// </summary>
	[Serializable]
	public class AssemblySlashDocCollection : CollectionBase {
		#region collection methods

		/// <summary>
		/// Adds the specified <see cref="AssemblySlashDoc"/> object to the collection.
		/// </summary>
		/// <param name="assySlashDoc">The <see cref="AssemblySlashDoc"/> to add to the collection.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assySlashDoc"/> is a <see langword="null"/>.</exception>
		/// <remarks>
		/// If the path of the <see cref="AssemblySlashDoc.Assembly"/> 
		/// in <paramref name="assySlashDoc"/> matches one already existing in the collection, the
		/// operation is silently ignored.
		/// </remarks>
		public void Add(AssemblySlashDoc assySlashDoc) {
			if (assySlashDoc == null)
				throw new ArgumentNullException("assySlashDoc");

			if (!Contains(assySlashDoc.Assembly.Path))
				List.Add(assySlashDoc);
		}

		/// <summary>
		/// Event rasied when the collection is cleared
		/// </summary>
		public event EventHandler Cleared;

		/// <summary>
		/// Raises the <see cref="Cleared"/> event
		/// </summary>
		protected override void OnClear() {
			if (Cleared != null)
				Cleared(this, EventArgs.Empty);

			base.OnClear();
		}


		/// <summary>
		/// Event rasied when an item is added to the collection
		/// </summary>
		public event AssemblySlashDocEventHandler ItemAdded;

		/// <summary>
		/// Raises the <see cref="ItemAdded"/> event
		/// </summary>
		protected override void OnInsertComplete(int index, object value) {
			if (ItemAdded != null)
				ItemAdded(this, new AssemblySlashDocEventArgs(value as AssemblySlashDoc));

			base.OnInsertComplete(index, value);
		}

		/// <summary>
		/// Removes the first occurence of a specific <see cref="AssemblySlashDoc"/> from the collection.
		/// </summary>
		/// <param name="assySlashDoc">The <see cref="AssemblySlashDoc"/> to remove from the collection.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assySlashDoc"/> is a <see langword="null"/>.</exception>
		/// <remarks>
		/// Elements that follow the removed element move up to occupy the vacated spot and the indexes of the elements that are moved are also updated.
		/// </remarks>
		public void Remove(AssemblySlashDoc assySlashDoc) {
			if (assySlashDoc == null)
				throw new ArgumentNullException("assySlashDoc");

			List.Remove(assySlashDoc);
		}

		/// <summary>
		/// Event rasied when an item is removed from the collection
		/// </summary>
		public event AssemblySlashDocEventHandler ItemRemoved;

		/// <summary>
		/// Raises the <see cref="ItemRemoved"/> event
		/// </summary>
		protected override void OnRemoveComplete(int index, object value) {
			if (ItemRemoved != null)
				ItemRemoved(this, new AssemblySlashDocEventArgs(value as AssemblySlashDoc));

			base.OnRemoveComplete(index, value);
		}

		/// <summary>
		/// Gets or sets the <see cref="AssemblySlashDoc"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the <see cref="AssemblySlashDoc"/> to get or set.</param>
		/// <value>The <see cref="AssemblySlashDoc"/> at the specified index</value>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index 
		/// in the collection.</exception>
		/// <exception cref="ArgumentNullException">set <i>value</i> is a <see langword="null"/>.</exception>
		public AssemblySlashDoc this[int index] {
			get {
				return List[index] as AssemblySlashDoc;
			}
			set {
				if (value == null)
					throw new ArgumentNullException("index");

				List[index] = value;
			}
		}

		/// <overloads>Determines whether the collection contains a specified element.</overloads>
		/// <summary>
		/// Determines whether the collection contains the specified <see cref="AssemblySlashDoc"/>.
		/// </summary>
		/// <param name="assySlashDoc">The <see cref="AssemblySlashDoc"/> to locate in the collection.</param>
		/// <returns><see langword="true"/> if the collection contains the specified <see cref="AssemblySlashDoc"/>, 
		/// otherwise <see langword="false"/>.</returns>
		public bool Contains(AssemblySlashDoc assySlashDoc) {
			return InnerList.Contains(assySlashDoc);
		}

		/// <summary>
		/// Determines whether the collection contains a specified assembly path.
		/// </summary>
		/// <param name="path">The assembly path to locate in the collection.</param>
		/// <returns><see langword="true"/> if the collection contains the specified path, 
		/// otherwise <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is a <see langword="null"/>.</exception>
		/// <remarks>Path comparison is case-insensitive.</remarks>
		public bool Contains(string path) {
			// Check if the path are specified
			if (path == null)
				throw new ArgumentNullException("path");

			// Go through the list and look of the assembly with the specified path
			bool result = false;
			foreach (object obj in InnerList) {
				AssemblySlashDoc asd = obj as AssemblySlashDoc;
				if (asd != null) {
					if (String.Compare(asd.Assembly.Path, path, true) == 0) {
						result = true;
						break;
					}
				} else
					throw new Exception("AssemblySlashDoc object are null");
			}
			return result;
		}
		#endregion

		/// <summary>
		/// Loads <see cref="AssemblySlashDoc"/> details from an <see cref="XmlReader"/>.
		/// </summary>
		/// <param name="reader">
		/// <exception cref="DocumenterException">The <i>location</i> attribute is missing or is an empty string</exception>
		/// An open <see cref="XmlReader"/> positioned before, or on, the <b>&lt;assemblies&gt;</b> element.</param>
		/// <remarks>
		/// The expected format is is follows
		/// <code escaped="true">
		/// <assemblies>
		///		<assembly location="relative or fixed path" documentation="relative or fixed path"/>
		///		...
		/// </assemblies>
		/// </code>
		/// <para>If the <i>location</i> attribute is missing or an empty string an exception will be thrown.</para>
		/// <para>If the <i>documentation</i> attribute is missing or an empty string it will be silently ignored.</para>
		/// </remarks>
		public void ReadXml(XmlReader reader) {
			// Read the assemblies section of the XML file
			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "assemblies")) {
				// Read an assembly section
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "assembly") {
					// Check if the location attribute are present or an empty string
					if (reader.GetAttribute("location") == null) {
						throw new DocumenterException("\"location\" attribute is" + " required for <assembly> element in project file.");
					}
					string location = reader.GetAttribute("location").Trim();
					if (location.Length == 0) {
						throw new DocumenterException("\"location\" attribute of" + " <assembly> element cannot be empty in project file.");
					}
					// Read the assembly documentation
					string documentation = reader.GetAttribute("documentation") ?? String.Empty;
					// Add the assembly to the collection
					AssemblySlashDoc assemblySlashDoc = new AssemblySlashDoc(location, documentation);
					Add(assemblySlashDoc);
				}
				reader.Read();
			}
		}


		/// <summary>
		/// Saves <see cref="AssemblySlashDoc"/> details to an <see cref="XmlWriter"/>.
		/// </summary>
		/// <param name="writer">An open <see cref="XmlWriter"/>.</param>
		/// <remarks>
		/// The persisted format is is follows
		/// <code escaped="true">
		/// <assemblies>
		///		<assembly location="relative or fixed path" documentation="relative or fixed path"/>
		///		...
		/// </assemblies>
		/// </code>
		/// </remarks>
		public void WriteXml(XmlWriter writer) {
			// If there are any assemblies in the collection write them into the XML file
			if (Count > 0) {
				writer.WriteStartElement("assemblies");

				// Write the location and documentation of the assembly
				foreach (AssemblySlashDoc asd in InnerList) {
					writer.WriteStartElement("assembly");
					writer.WriteAttributeString("location", asd.Assembly.ToString());
					writer.WriteAttributeString("documentation", asd.SlashDoc.ToString());
					writer.WriteEndElement();
				}

				writer.WriteEndElement();
			}
		}
	}
}
