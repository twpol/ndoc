using System;
using System.Runtime.InteropServices;
using System.Xml;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections;
using System.Text;
using System.IO;


namespace NDoc.Core
{
	/// <summary>
	/// Type safe collection class for <see cref="AssemblySlashDoc"/> objects. 
	/// </summary>
	/// <remarks>Extends the base class CollectionBase to inherit base collection functionality.
	/// </remarks>
	[Serializable]
	public class AssemblySlashDocCollection : CollectionBase
	{
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
		public void Add(AssemblySlashDoc assySlashDoc)
		{
			if (assySlashDoc == null)
				throw new ArgumentNullException("assySlashDoc");

			if (!Contains(assySlashDoc.Assembly.Path))
				this.List.Add(assySlashDoc);
		}
		
		/// <summary>
		/// Adds the elements of an <see cref="ICollection"/> to the end of the collection.
		/// </summary>
		/// <param name="c">The <see cref="ICollection"/> whose elements should be added to the end of the collection. 
		/// The collection itself cannot be a <see langword="null"/>.</param>
		/// <exception cref="ArgumentNullException"><paramref name="c"/> is a <see langword="null"/>.</exception>
		/// <remarks>
		/// </remarks>
		public virtual void AddRange(ICollection c)
		{
			base.InnerList.AddRange(c);

		}

		/// <summary>
		/// Removes the first occurence of a specific <see cref="AssemblySlashDoc"/> from the collection.
		/// </summary>
		/// <param name="assySlashDoc">The <see cref="AssemblySlashDoc"/> to remove from the collection.</param>
		/// <exception cref="ArgumentNullException"><paramref name="assySlashDoc"/> is a <see langword="null"/>.</exception>
		/// <remarks>
		/// Elements that follow the removed element move up to occupy the vacated spot and the indexes of the elements that are moved are also updated.
		/// </remarks>
		public void Remove(AssemblySlashDoc assySlashDoc)
		{
			if (assySlashDoc == null)
				throw new ArgumentNullException("assySlashDoc");

			this.List.Remove(assySlashDoc);
		}
		
		/// <summary>
		/// Gets or sets the <see cref="AssemblySlashDoc"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the <see cref="AssemblySlashDoc"/> to get or set.</param>
		/// <value>The <see cref="AssemblySlashDoc"/> at the specified index</value>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index 
		/// in the collection.</exception>
		/// <exception cref="ArgumentNullException">set <i>value</i> is a <see langword="null"/>.</exception>
		public AssemblySlashDoc this[int index] 
		{
			get
			{
				return this.List[index] as AssemblySlashDoc;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException("set value");
				this.List[index] = value;
			}
		}

		/// <overrides>Determines whether the collection contains a specified element.</overrides>
		/// <summary>
		/// Determines whether the collection contains the specified <see cref="AssemblySlashDoc"/>.
		/// </summary>
		/// <param name="assySlashDoc">The <see cref="AssemblySlashDoc"/> to locate in the collection.</param>
		/// <returns><see langword="true"/> if the collection contains the specified <see cref="AssemblySlashDoc"/>, 
		/// otherwise <see langword="false"/>.</returns>
		public bool Contains(AssemblySlashDoc assySlashDoc)
		{
			return base.InnerList.Contains(assySlashDoc);
		}

		/// <summary>
		/// Determines whether the collection contains a specified assembly path.
		/// </summary>
		/// <param name="path">The assembly path to locate in the collection.</param>
		/// <returns><see langword="true"/> if the collection contains the specified path, 
		/// otherwise <see langword="false"/>.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="path"/> is a <see langword="null"/>.</exception>
		/// <remarks>Path comparison is case-insensitive.</remarks>
		public bool Contains(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			bool result = false;
			foreach (object obj in base.InnerList)
			{
				AssemblySlashDoc asd = obj as AssemblySlashDoc;
				if (String.Compare(asd.Assembly.Path, path, true) == 0)
				{
					result = true;
					break;
				}
			}
			return result;
		}
		#endregion

		/// <summary>
		/// Loads AssemblySlashDoc details from an XMLReader.
		/// </summary>
		/// <param name="reader">
		/// An open XmlReader positioned before or on the &lt;assemblies&gt; element.</param>
		public void ReadXml(XmlReader reader)
		{
			while (!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "assemblies"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "assembly")
				{
					if (reader.GetAttribute("location") == null) 
					{
						throw new DocumenterException("\"location\" attribute is"
							+ " required for <assembly> element in project file.");
					}
					if (reader.GetAttribute("location").Trim().Length == 0) 
					{
						throw new DocumenterException("\"location\" attribute of"
							+ " <assembly> element cannot be empty in project file.");
					}
					AssemblySlashDoc assemblySlashDoc = new AssemblySlashDoc(reader["location"], reader["documentation"]);
					Add(assemblySlashDoc);
				}
				reader.Read();
			}
		}


		/// <summary>
		/// Saves AssemblySlashDoc details to an XmlWriter.
		/// </summary>
		/// <param name="writer">An open XmlWriter.</param>
		public void WriteXml(XmlWriter writer)
		{
			if (Count > 0)
			{
				writer.WriteStartElement("assemblies");

				foreach (AssemblySlashDoc asd in this.InnerList)
				{
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
