// BaseDocumenterConfig.cs - base XML documenter config class
// Copyright (C) 2001 Kral Ferch, Jason Diamond
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

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

			_ShowMissingSummaries = false;
			_ShowMissingRemarks = false;
			_ShowMissingParams = false;
			_ShowMissingReturns = false;
			_ShowMissingValues = false;

			_DocumentInternals = false;
			_DocumentProtected = true;
			_DocumentPrivates = false;
			_DocumentEmptyNamespaces = false;

			_IncludeAssemblyVersion = false;
			_CopyrightText = string.Empty;
			_CopyrightHref = string.Empty;

			_SkipNamespacesWithoutSummaries = false;
			_AutoPropertyBackerSummaries = false;
			_AutoDocumentConstructors = true;
		}

		private Project _Project;

		/// <summary>Associates this documenter with a project;</summary>
		public void SetProject(Project project)
		{
			_Project = project;
		}

		/// <summary>Sets the IsDirty property on the project if any is set.</summary>
		protected void SetDirty()
		{
			if (_Project != null)
			{
				_Project.IsDirty = true;
			}
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
			// If there's an associated project, we don't want to set it as
			// dirty during the read. Temporarily set it to null so that
			// calls to SetDirty get ignored.
			Project project = _Project;
			_Project = null;

			while(!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "documenter"))
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
				reader.Read(); // Advance.
			}

			// Restore the saved project.
			_Project = project;
		}

		bool _ShowMissingSummaries;

		/// <summary>Gets or sets the ShowMissingSummaries property.</summary>
		/// <remarks>If this is true, all members without /doc summary
		/// comments will contain the phrase "Missing Documentation" in the
		/// generated documentation.</remarks>
		[
		Category("Show Missing Documentation"),
		Description("Turning this flag on will show you where you are missing summaries.")
		]
		public bool ShowMissingSummaries
		{
			get { return _ShowMissingSummaries; }

			set
			{
				_ShowMissingSummaries = value;
				SetDirty();
			}
		}

		bool _ShowMissingRemarks;

		/// <summary>Gets or sets the ShowMissingRemarks property.</summary>
		/// <remarks>If this is true, all members without /doc summary
		/// comments will contain the phrase "Missing Documentation" in the
		/// generated documentation.</remarks>
		[
		Category("Show Missing Documentation"),
		Description("Turning this flag on will show you where you are missing Remarks.")
		]
		public bool ShowMissingRemarks
		{
			get { return _ShowMissingRemarks; }

			set
			{
				_ShowMissingRemarks = value;
				SetDirty();
			}
		}

		bool _ShowMissingParams;

		/// <summary>Gets or sets the ShowMissingParams property.</summary>
		/// <remarks>If this is true, all members without /doc summary
		/// comments will contain the phrase "Missing Documentation" in the
		/// generated documentation.</remarks>
		[
		Category("Show Missing Documentation"),
		Description("Turning this flag on will show you where you are missing Params.")
		]
		public bool ShowMissingParams
		{
			get { return _ShowMissingParams; }

			set
			{
				_ShowMissingParams = value;
				SetDirty();
			}
		}

		bool _ShowMissingReturns;

		/// <summary>Gets or sets the ShowMissingReturns property.</summary>
		/// <remarks>If this is true, all members without /doc summary
		/// comments will contain the phrase "Missing Documentation" in the
		/// generated documentation.</remarks>
		[
		Category("Show Missing Documentation"),
		Description("Turning this flag on will show you where you are missing Returns.")
		]
		public bool ShowMissingReturns
		{
			get { return _ShowMissingReturns; }

			set
			{
				_ShowMissingReturns = value;
				SetDirty();
			}
		}

		bool _ShowMissingValues;

		/// <summary>Gets or sets the ShowMissingValues property.</summary>
		/// <remarks>If this is true, all members without /doc summary
		/// comments will contain the phrase "Missing Documentation" in the
		/// generated documentation.</remarks>
		[
		Category("Show Missing Documentation"),
		Description("Turning this flag on will show you where you are missing Values.")
		]
		public bool ShowMissingValues
		{
			get { return _ShowMissingValues; }

			set
			{
				_ShowMissingValues = value;
				SetDirty();
			}
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

			set
			{
				_DocumentInternals = value;
				SetDirty();
			}
		}

		bool _DocumentProtected;

		/// <summary>Gets or sets the DocumentProtected property.</summary>
		[
		Category("Visibility"),
		Description("Turn this flag on to document protected code.")
		]
		public bool DocumentProtected
		{
			get { return _DocumentProtected; }

			set
			{
				_DocumentProtected = value;
				SetDirty();
			}
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

			set
			{
				_DocumentPrivates = value;
				SetDirty();
			}
		}

		bool _DocumentEmptyNamespaces;

		/// <summary>Gets or sets the DocumentPrivates property.</summary>
		[
		Category("Visibility"),
		Description("Turn this flag on to document empty namespaces.")
		]
		public bool DocumentEmptyNamespaces
		{
			get { return _DocumentEmptyNamespaces; }

			set
			{
				_DocumentEmptyNamespaces = value;
				SetDirty();
			}
		}

		bool _IncludeAssemblyVersion;

		/// <summary>Gets or sets the IncludeAssemblyVersion property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("Turn this flag on to include the assembly version number in the documentation.")
		]
		public bool IncludeAssemblyVersion
		{
			get { return _IncludeAssemblyVersion; }

			set
			{
				_IncludeAssemblyVersion = value;
				SetDirty();
			}
		}

		string _CopyrightText;

		/// <summary>Gets or sets the CopyrightText property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("A copyright notice text that will be included in the generated docs.")
		]
		public string CopyrightText
		{
			get { return _CopyrightText; }

			set
			{
				_CopyrightText = value;
				SetDirty();
			}
		}

		string _CopyrightHref;

		/// <summary>Gets or sets the CopyrightHref property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("An URL referenced by the copyright notice.")
		]
		public string CopyrightHref
		{
			get { return _CopyrightHref; }

			set
			{
				_CopyrightHref = value;
				SetDirty();
			}
		}


		string _ReferencesPath;

		/// <summary>Gets or sets the base directory used to resolve directory and assembly references.</summary>
		[
		Category("Documentation Main Settings"),
		Description("The directory used to resolve path specifications and assembly references. The search for assemblies includes this directory and all subdirectories."),
		Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor)),
		]
		public string ReferencesPath
		{
			get { return _ReferencesPath; }

			set
			{
				_ReferencesPath = value;
				SetDirty();
			}
		}
		bool _SkipNamespacesWithoutSummaries;

		/// <summary>Gets or sets the SkipNamespacesWithoutSummaries property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("Setting this property to true will not document namespaces that don't have an associated namespace summary."),
		]
		public bool SkipNamespacesWithoutSummaries
		{
			get { return _SkipNamespacesWithoutSummaries; }

			set
			{
				_SkipNamespacesWithoutSummaries = value;
				SetDirty();
			}
		}

		bool _AutoPropertyBackerSummaries;

		/// <summary>Gets or sets the AutoPropertyBackerSummaries property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("If true, the documenter will automatically add a summary "
			+ "for fields which look like they back (hold the value for) a "
			+ "property. The summary is only added if there is no existing summary, "
			+ "which gives you a way to opt out of this behavior in particular cases. "
			+ "Currently the naming conventions supported are such that "
			+ "fields '_Length' and 'length' will be inferred to back property 'Length'."),
		]
		public bool AutoPropertyBackerSummaries
		{
			get { return _AutoPropertyBackerSummaries; }

			set
			{
				_AutoPropertyBackerSummaries = value;
				SetDirty();
			}
		}

		bool _AutoDocumentConstructors;

		/// <summary>Gets or sets the AutoDocumentConstructors property.</summary>
		/// <remarks>If this is true, default constructors without /doc summary
		/// comments will be automatically documented.</remarks>
		[
		Category("Documentation Main Settings"),
		Description("Turning this flag on will enable automatic summary documentation for default constructors.")
		]
		public bool AutoDocumentConstructors
		{
			get { return _AutoDocumentConstructors; }

			set
			{
				_AutoDocumentConstructors = value;
				SetDirty();
			}
		}

	}
}
