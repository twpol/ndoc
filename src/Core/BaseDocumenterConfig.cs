// BaseDocumenterConfig.cs - base XML documenter config class
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
using System.ComponentModel;
using System.Reflection;
using System.Xml;

namespace NDoc.Core
{
	/// <summary>The base documenter config class.</summary>
	abstract public class BaseDocumenterConfig : IDocumenterConfig
	{
		private string _Name;

		/// <summary>Initializes a new instance of the DocumenterConfig class.</summary>
		public BaseDocumenterConfig(string name)
		{
			_Name = name;
			
			ShowMissingSummaries = false;
			ShowMissingRemarks = false;
			ShowMissingParams = false;
			ShowMissingReturns = false;
			ShowMissingValues = false;

			DocumentInternals = false;
			DocumentPrivates = false;
		}

		/// <summary>Gets a list of property names.</summary>
		public IEnumerable GetProperties()
		{
			ArrayList properties = new ArrayList();

			foreach (PropertyInfo property in GetType().GetProperties())
			{
				properties.Add(property.Name);
			}

			return properties;
		}

		/// <summary>Sets the value of a property.</summary>
		public void SetValue(string name, string value)
		{
			name = name.ToLower();

			foreach (PropertyInfo property in GetType().GetProperties())
			{
				if (name == property.Name.ToLower())
				{
					object value2 = System.Convert.ChangeType(value, property.PropertyType);
					property.SetValue(this, value2, null);
					break;
				}
			}
		}

		/// <summary>Writes the current state of the documenter to the specified XmlWrtier.</summary>
		/// <param name="writer">An XmlWriter.</param>
		/// <remarks>This method uses reflection to serialize all of the public properties in the documenter.</remarks>
		public void Write(XmlWriter writer)
		{
			writer.WriteStartElement("documenter");
			writer.WriteAttributeString("name", _Name);

			PropertyInfo[] properties = GetType().GetProperties();

			foreach (PropertyInfo property in properties)
			{
				object value = property.GetValue(this, null);

				if (value != null)
				{
					string value2 = Convert.ToString(value);

					if (value2 != null)
					{
						writer.WriteStartElement("property");
						writer.WriteAttributeString("name", property.Name);
						writer.WriteAttributeString("value", value2);
						writer.WriteEndElement();
					}
				}
			}

			writer.WriteEndElement();
		}

		/// <summary>Reads the previously serialized state of the documenter into memory.</summary>
		/// <param name="reader">An XmlReader positioned on a documenter element.</param>
		/// <remarks>This method uses reflection to set all of the public properties in the documenter.</remarks>
		public void Read(XmlReader reader)
		{
			while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "documenter"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
				{
					string name = reader["name"];

					PropertyInfo property = GetType().GetProperty(name);

					if (property != null)
					{
						string value = reader["value"];
						object value2 = Convert.ChangeType(value, property.PropertyType);

						property.SetValue(this, value2, null);
					}
				}
			}
		}

		bool _ShowMissingSummaries;

		/// <summary>Gets or sets the ShowMissingSummaries property.</summary>
		/// <remarks>If this is true, all members without /doc summary 
		/// comments will contain the phrase "Missing Documentation" in the 
		/// generated documentation.</remarks>
		[
		Category("Missing"),
		Description("Turning this flag on will show you where you are missing summaries.")
		]
		public bool ShowMissingSummaries
		{
			get { return _ShowMissingSummaries; }
			set { _ShowMissingSummaries = value; }
		}

		bool _ShowMissingRemarks;

		/// <summary>Gets or sets the ShowMissingRemarks property.</summary>
		/// <remarks>If this is true, all members without /doc summary 
		/// comments will contain the phrase "Missing Documentation" in the 
		/// generated documentation.</remarks>
		[
		Category("Missing"),
		Description("Turning this flag on will show you where you are missing Remarks.")
		]
		public bool ShowMissingRemarks
		{
			get { return _ShowMissingRemarks; }
			set { _ShowMissingRemarks = value; }
		}

		bool _ShowMissingParams;

		/// <summary>Gets or sets the ShowMissingParams property.</summary>
		/// <remarks>If this is true, all members without /doc summary 
		/// comments will contain the phrase "Missing Documentation" in the 
		/// generated documentation.</remarks>
		[
		Category("Missing"),
		Description("Turning this flag on will show you where you are missing Params.")
		]
		public bool ShowMissingParams
		{
			get { return _ShowMissingParams; }
			set { _ShowMissingParams = value; }
		}

		bool _ShowMissingReturns;

		/// <summary>Gets or sets the ShowMissingReturns property.</summary>
		/// <remarks>If this is true, all members without /doc summary 
		/// comments will contain the phrase "Missing Documentation" in the 
		/// generated documentation.</remarks>
		[
		Category("Missing"),
		Description("Turning this flag on will show you where you are missing Returns.")
		]
		public bool ShowMissingReturns
		{
			get { return _ShowMissingReturns; }
			set { _ShowMissingReturns = value; }
		}

		bool _ShowMissingValues;

		/// <summary>Gets or sets the ShowMissingValues property.</summary>
		/// <remarks>If this is true, all members without /doc summary 
		/// comments will contain the phrase "Missing Documentation" in the 
		/// generated documentation.</remarks>
		[
		Category("Missing"),
		Description("Turning this flag on will show you where you are missing Values.")
		]
		public bool ShowMissingValues
		{
			get { return _ShowMissingValues; }
			set { _ShowMissingValues = value; }
		}

		bool _DocumentInternals;

		/// <summary>Gets or sets the DocumentInternals property.</summary>
		[
		Category("Visibility"),
		Description("Turn this flag on to document internal code.")
		]
		public bool DocumentInternals
		{
			get { return _DocumentInternals; }
			set { _DocumentInternals = value; }
		}

		bool _DocumentPrivates;

		/// <summary>Gets or sets the DocumentPrivates property.</summary>
		[
		Category("Visibility"),
		Description("Turn this flag on to document private code.")
		]
		public bool DocumentPrivates
		{
			get { return _DocumentPrivates; }
			set { _DocumentPrivates = value; }
		}
	}
}
