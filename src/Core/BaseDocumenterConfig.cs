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
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Xml;

namespace NDoc.Core
{

	/// <summary>The base documenter config class.</summary>
	abstract public class BaseDocumenterConfig : IDocumenterConfig
	{
		private string _Name;

		/// <summary>Initializes a new instance of the DocumenterConfig class.</summary>
		protected BaseDocumenterConfig(string name)
		{
			_Name = name;

			_ShowMissingSummaries = false;
			_ShowMissingRemarks = false;
			_ShowMissingParams = false;
			_ShowMissingReturns = false;
			_ShowMissingValues = false;

			_DocumentInternals = false;
			_DocumentProtected = true;
			_DocumentSealedProtected = false;
			_DocumentPrivates = false;
			_DocumentProtectedInternalAsProtected = false;
			_DocumentEmptyNamespaces = false;
			_EditorBrowsableFilter = EditorBrowsableFilterLevel.Off;

			_IncludeAssemblyVersion = false;
			_CopyrightText = string.Empty;
			_CopyrightHref = string.Empty;

			_SkipNamespacesWithoutSummaries = false;
			_UseNamespaceDocSummaries = false;
			_AutoPropertyBackerSummaries = false;
			_AutoDocumentConstructors = true;

			_GetExternalSummaries = true;

			_DocumentAttributes = false;
			_ShowTypeIdInAttributes = false;
			_DocumentedAttributes = "";

			_ReferencesPath = string.Empty;
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
					// fix for bug 839384 
					object value2 = null;
					if(property.PropertyType.IsEnum) 
					{
						value2 = Enum.Parse(property.PropertyType, value);
					}
					else 
					{
						value2 = System.Convert.ChangeType(value, property.PropertyType);
					}

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
			string FailureMessages="";

			while(!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "documenter"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
				{
					string name = reader["name"];
					bool ValueParsedOK = false;

					PropertyInfo property = GetType().GetProperty(name);

					if (property != null)
					{
						string value = reader["value"];
						object value2 = null;
						
						// if the string in the project file is not a valid member
						// of the enum, or cannot be parsed into the property type
						// for som reason,we don't want to throw an exception and
						// ditch all the settings stored later in the file!
						// save the exception details, and  we will throw a 
						// single exception at the end..
						try
						{
						if (property.PropertyType.BaseType == typeof(Enum))
						{
							value2 = Enum.Parse(property.PropertyType, value);
								ValueParsedOK = true;
						}
						else
						{
							value2 = Convert.ChangeType(value, property.PropertyType);
								ValueParsedOK = true;
							}
						}
						catch(System.ArgumentException)
						{
							FailureMessages += String.Format("     Property '{0}' has an invalid value ('{1}') \n", name, value);
						}
						catch(System.FormatException)
						{
							FailureMessages += String.Format("     Property '{0}' has an invalid value for type {1} ('{2}') \n", name, property.PropertyType.ToString() ,value);
						}
						// any other exception will be thrown immediately

						if (property.CanWrite && ValueParsedOK)
						{
							property.SetValue(this, value2, null);
						}
					}
				}
				reader.Read(); // Advance.
			}

			// Restore the saved project.
			_Project = project;
			if (FailureMessages.Length > 0)
				throw new DocumenterPropertyFormatException(FailureMessages);
		}

		bool _ShowMissingSummaries;

		/// <summary>Gets or sets the ShowMissingSummaries property.</summary>
		/// <remarks>If this is true, all members without /doc summary
		/// comments will contain the phrase "Missing Documentation" in the
		/// generated documentation.</remarks>
		[Category("Show Missing Documentation")]
		[Description("Turning this flag on will show you where you are missing summaries.")]
		[DefaultValue(false)]
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
		[Category("Show Missing Documentation")]
		[Description("Turning this flag on will show you where you are missing Remarks.")]
		[DefaultValue(false)]
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
		[Category("Show Missing Documentation")]
		[Description("Turning this flag on will show you where you are missing Params.")]
		[DefaultValue(false)]
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
		[Category("Show Missing Documentation")]
		[Description("Turning this flag on will show you where you are missing Returns.")]
		[DefaultValue(false)]
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
		[Category("Show Missing Documentation")]
		[Description("Turning this flag on will show you where you are missing Values.")]
		[DefaultValue(false)]
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
		[Category("Visibility")]
		[Description("Turn this flag on to document internal code.")]
		[DefaultValue(false)]
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
		[Category("Visibility")]
		[Description("Turn this flag on to document protected code.")]
		[DefaultValue(true)]
		public bool DocumentProtected
		{
			get { return _DocumentProtected; }

			set
			{
				_DocumentProtected = value;

				// If DocumentProtected is turned off, then we automatically turn off
				// DocumentSealedProtected, too.
				if (!value)
				{
					_DocumentSealedProtected = false;
				}
				SetDirty();
			}
		}

		bool _DocumentSealedProtected;

		/// <summary>Gets or sets the DocumentSealedProtected property.</summary>
		[Category("Visibility")]
		[Description("Turn this flag on to document protected members of sealed classes. DocumentProtected must be turned on, too.")]
		[DefaultValue(false)]
		public bool DocumentSealedProtected
		{
			get { return _DocumentSealedProtected; }

			set
			{
				_DocumentSealedProtected = value;

				// If DocumentSealedProtected is turned on, then we automatically turn on
				// DocumentProtected, too.
				if (value)
				{
					_DocumentProtected = true;
				}
				SetDirty();
			}
		}

		bool _DocumentPrivates;

		/// <summary>Gets or sets the DocumentPrivates property.</summary>
		[Category("Visibility")]
		[Description("Turn this flag on to document private code.")]
		[DefaultValue(false)]
		public bool DocumentPrivates
		{
			get { return _DocumentPrivates; }

			set
			{
				_DocumentPrivates = value;
				SetDirty();
			}
		}

		private bool _DocumentProtectedInternalAsProtected;

		/// <summary>Gets or sets the DocumentProtectedInternalAsProtected property.</summary>
		[Category("Visibility")]
		[Description("If true, NDoc will treat \"protected internal\" members as \"protected\" only.")]
		[DefaultValue(false)]
		public bool DocumentProtectedInternalAsProtected
		{
			get { return _DocumentProtectedInternalAsProtected; }

			set
			{
				_DocumentProtectedInternalAsProtected = value;
				SetDirty();
			}
		}

		bool _DocumentEmptyNamespaces;

		/// <summary>Gets or sets the DocumentPrivates property.</summary>
		[Category("Visibility")]
		[Description("Turn this flag on to document empty namespaces.")]
		[DefaultValue(false)]
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
		[Category("Documentation Main Settings")]
		[Description("Turn this flag on to include the assembly version number in the documentation.")]
		[DefaultValue(false)]
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
		[Category("Documentation Main Settings")]
		[Description("A copyright notice text that will be included in the generated docs.")]
		[Editor(typeof(TextEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
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
		[Category("Documentation Main Settings")]
		[Description("An URL referenced by the copyright notice.")]
		[DefaultValue("")]
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
		[Category("Documentation Main Settings")]
		[Description("The directory used to resolve path specifications and assembly references. The search for assemblies includes this directory and all subdirectories.")]
#if !MONO //System.Windows.Forms.Design.FolderNameEditor is not implemented in mono 0.28
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
#endif
		[DefaultValue("")]
		public string ReferencesPath
		{
			get { return _ReferencesPath; }

			set
			{
				_ReferencesPath = value;
				SetDirty();
			}
		}

		string _FeedbackEmailAddress  = string.Empty;

		/// <summary>Gets or sets the FeedbackEmailAddress property.</summary>
		[Category("Documentation Main Settings")]
		[Description("If an email address is supplied, a mailto link will be placed at the bottom of each page using this address")]
		[DefaultValue("")]
		public string FeedbackEmailAddress
		{
			get { return _FeedbackEmailAddress ; }
			set
			{
				_FeedbackEmailAddress  = value;
				SetDirty();
			}
		}			
		
		bool _SkipNamespacesWithoutSummaries;

		/// <summary>Gets or sets the SkipNamespacesWithoutSummaries property.</summary>
		[Category("Visibility")]
		[Description("Setting this property to true will not document namespaces that don't have an associated namespace summary.")]
		[DefaultValue(false)]
		public bool SkipNamespacesWithoutSummaries
		{
			get { return _SkipNamespacesWithoutSummaries; }

			set
			{
				_SkipNamespacesWithoutSummaries = value;
				SetDirty();
			}
		}

		bool _UseNamespaceDocSummaries;

		/// <summary>Gets or sets the UseNamespaceDocSummaries property.</summary>
		[Category("Documentation Main Settings")]
		[Description("If true, the documenter will look for a class with the name "
			+ "\"NamespaceDoc\" in each namespace. The summary from that class "
			+ "will then be used as the namespace summary.  The class itself will not "
			+ "show up in the resulting documentation output. You may want to use "
			+ "#if ... #endif together with conditional compilation constants to "
			+ "exclude the NamespaceDoc classes from release build assemblies.")]
		[DefaultValue(false)]
		public bool UseNamespaceDocSummaries
		{
			get { return _UseNamespaceDocSummaries; }

			set
			{
				_UseNamespaceDocSummaries = value;
				SetDirty();
			}
		}

		bool _AutoPropertyBackerSummaries;

		/// <summary>Gets or sets the AutoPropertyBackerSummaries property.</summary>
		[Category("Documentation Main Settings")]
		[Description("If true, the documenter will automatically add a summary "
			+ "for fields which look like they back (hold the value for) a "
			+ "property. The summary is only added if there is no existing summary, "
			+ "which gives you a way to opt out of this behavior in particular cases. "
			+ "Currently the naming conventions supported are such that "
			+ "fields '_Length' and 'length' will be inferred to back property 'Length'.")]
		[DefaultValue(false)]
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
		[Category("Documentation Main Settings")]
		[Description("Turning this flag on will enable automatic summary documentation for default constructors.")]
		[DefaultValue(true)]
		public bool AutoDocumentConstructors
		{
			get { return _AutoDocumentConstructors; }

			set
			{
				_AutoDocumentConstructors = value;
				SetDirty();
			}
		}

		private bool _DocumentAttributes;

		/// <summary>Gets or sets whether or not to document the attributes.</summary>
		[Category("Show Attributes")]
		[Description("Set this to true to output the attributes of the types/members in the syntax portion.")]
		[DefaultValue(false)]
		public bool DocumentAttributes
		{
			get { return _DocumentAttributes; }

			set 
			{ 
				_DocumentAttributes = value; 
				SetDirty();
			}
		}

		private bool _ShowTypeIdInAttributes;

		/// <summary>Gets or sets whether or not to show the TypeId property in attributes.</summary>
		[Category("Show Attributes")]
		[Description("Set this to true to output the TypeId property in the attributes.")]
		[DefaultValue(false)]
		public bool ShowTypeIdInAttributes
		{
			get { return _ShowTypeIdInAttributes; }

			set 
			{ 
				_ShowTypeIdInAttributes = value; 
				SetDirty();
			}
		}

		private string _DocumentedAttributes;

		/// <summary>Gets or sets which attributes should be documented.</summary>
		[Category("Show Attributes")]
		[Description("When DocumentAttributes is set to true, this specifies which attributes/property are visisble.  Empty to show all.  Format: '<attribute-name-starts-with>,<property-to-show>,<property-to-show>|<attribute-name-starts-with>,<property-to-show>,<property-to-show>|(etc...)'.")]
		[Editor(typeof(AttributesEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string DocumentedAttributes
		{
			get { return _DocumentedAttributes; }

			set
			{
				_DocumentedAttributes = value;
				SetDirty();
			}
		}

		private bool _GetExternalSummaries;

		/// <summary>Load external xml files?</summary>
		[Category("Documentation Main Settings")]
		[Description("If true, NDoc will try loading external xml files to retreive the summaries of inherited members.")]
		[DefaultValue(true)]
		public bool GetExternalSummaries
		{
			get { return _GetExternalSummaries; }

			set
			{
				_GetExternalSummaries = value;
				SetDirty();
			}
		}

		private EditorBrowsableFilterLevel _EditorBrowsableFilter;

		/// <summary>Specifies the level of filtering on the EditorBrowsable attribute.</summary>
		[Category("Visibility")]
		[Description("Sets the level of filtering to apply on types/members marked with the EditorBrowsable attribute.  Warning: enabling this filter might result in invalid links in the documentation.")]
		[DefaultValue(EditorBrowsableFilterLevel.Off)]
		public EditorBrowsableFilterLevel EditorBrowsableFilter
		{
			get { return _EditorBrowsableFilter; }

			set
			{
				_EditorBrowsableFilter = value;
				SetDirty();
			}
		}

		string _UseNDocXmlFile = string.Empty;

		/// <summary>Gets or sets the UseNDocXmlFile property.</summary>
		[Category("Documentation Main Settings")]
		[Description("When set, NDoc will use the specified XML file as input instead of reflecting the list of assemblies specified on the project.  Very useful for debugging documenters.  Leave empty for normal usage.")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		[DefaultValue("")]
		public string UseNDocXmlFile
		{
			get { return _UseNDocXmlFile; }
			set
			{
				_UseNDocXmlFile = value;
				SetDirty();
			}
		}

		bool _InheritPlatformSupport  = true;

		/// <summary>Gets or sets the InheritFrameworkSupport property.</summary>
		[Category("Supported Platforms")]
		[Description("When true, types and members that don't have specific framework support specified will display the default operating system and framework support values")]
		[DefaultValue(true)]
		public bool InheritPlatformSupport 
		{
			get { return _InheritPlatformSupport; }
			set
			{
				_InheritPlatformSupport  = value;
				SetDirty();
			}
		}

		OSSupport _DefaultOSSupport  = OSSupport.NoRestrictions;

		/// <summary>Gets or sets the DefaultOSSupport property.</summary>
		[Category("Supported Platforms")]
		[Description("Defines the default set of operating systems supported by classes that don't have OS support specified in their comments (ignored if InheritPlatformSupport is false)")]
		[DefaultValue(OSSupport.NoRestrictions)]
		public OSSupport DefaultOSSupport 
		{
			get { return _DefaultOSSupport ; }
			set
			{
				_DefaultOSSupport  = value;
				SetDirty();
			}
		}

		bool _SupportCompactFrameworkByDefault  = false;

		/// <summary>Gets or sets the DefaultSupportCompactFramework property.</summary>
		[Category("Supported Platforms")]
		[Description("If true, the .NET compact framework will be included in the default set of platforms (ignored if InheritPlatformSupport is false)")]
		[DefaultValue(false)]
		public bool SupportCompactFrameworkByDefault 
		{
			get { return _SupportCompactFrameworkByDefault ; }
			set
			{
				_SupportCompactFrameworkByDefault  = value;
				SetDirty();
			}
		}

		bool _SupportMONOFrameworkByDefault  = false;

		/// <summary>Gets or sets the DefaultSupportMONOFramework property.</summary>
		[Category("Supported Platforms")]
		[Description("If true, the MONO open source framework will be included in the default set of platforms (ignored if InheritPlatformSupport is false)")]
		[DefaultValue(false)]
		public bool SupportMONOFrameworkByDefault
		{
			get { return _SupportMONOFrameworkByDefault ; }
			set
			{
				_SupportMONOFrameworkByDefault  = value;
				SetDirty();
			}
		}	

		string _AdditionalFrameworkList  = string.Empty;

		/// <summary>Gets or sets the AdditionalFrameworkList property.</summary>
		[Category("Supported Platforms")]
		[Description("User defined list of additional framework implementations to be displayed for default platform support (ignored if InheritPlatformSupport is false)")]
		[DefaultValue("")]
		public string AdditionalFrameworkList
		{
			get { return _AdditionalFrameworkList ; }
			set
			{
				_AdditionalFrameworkList  = value;
				SetDirty();
			}
		}		

		string _AdditionalOSList  = string.Empty;

		/// <summary>Gets or sets the AdditionalOSList property.</summary>
		[Category("Supported Platforms")]
		[Description("User defined list of additional operating systems to be displayed for default platform support (ignored if InheritPlatformSupport is false)")]
		[DefaultValue("")]
		public string AdditionalOSList
		{
			get { return _AdditionalOSList ; }
			set
			{
				_AdditionalOSList  = value;
				SetDirty();
			}
		}	

	}

	/// <summary>
	/// Define the levels of filtering on the EditorBrowsable attribute.
	/// </summary>
	public enum EditorBrowsableFilterLevel
	{
		/// <summary>No filtering.</summary>
		Off, 

		/// <summary>Hide members flagged with EditorBrowsableState.Never.</summary>
		HideNever, 

		/// <summary>Hide members flagged with EditorBrowsableState.Never or EditorBrowsableState.Advanced.</summary>
		HideAdvanced
	}

	/// <summary>
	/// Defines the default set of operating systems to support
	/// if not explicitly specified in documentation comments
	/// </summary>
	public enum OSSupport
	{
		/// <summary>
		/// All operating systems that support .NET
		/// </summary>
		NoRestrictions,

		/// <summary>
		/// A Windows OS from the NT 5 family of operating systems
		/// </summary>
		NT5Plus,

		/// <summary>
		/// A server operating system
		/// </summary>
		Server,

		/// <summary>
		/// Do not show a default list of operating systems
		/// </summary>
		None
	}
}
