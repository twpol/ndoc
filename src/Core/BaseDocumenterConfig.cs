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
	/// <remarks>
	/// <para>Generating the documentation consists of two high level steps:
	/// <list type="number">
	/// <item><description>Merging the /doc XML summary with reflected meta-data from the assemblies.</description></item>
	/// <item><description>Transforming that merged XML into the documentation (HTML for the MSDN and VS.NET documenters).</description></item>
	/// </list></para>
	/// <para>The settings below govern how exactly the XML summary data is merged
	/// with the reflected meta-data and therefore govern what items will and will not 
	/// appear in the final documentation.
	/// </para>
	/// <para>These settings are shared by all of the NDoc documenters.</para>
	/// </remarks>
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

			_DocumentInheritedMembers = DocumentedInheritedMembers.Instance;
			_DocumentExplicitInterfaceImplementations = true;

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
			_DocumentedAttributes = string.Empty;

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

		/// <summary>
		/// The display name of the documenter.
		/// </summary>
		public string Name
		{
			get { return _Name;}
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
					bool writeProperty = true;
					string value2 = Convert.ToString(value);

					if (value2 != null)
					{
						//see if the property has a default value
						object[] defaultValues=property.GetCustomAttributes(typeof(DefaultValueAttribute),true);
						if (defaultValues.Length > 0)
						{
							if(Convert.ToString(((DefaultValueAttribute)defaultValues[0]).Value)==value2)
								writeProperty=false;
						}
						else
						{
							if(value2=="")
								writeProperty=false;
						}
					}
					else
					{
						writeProperty=false;
					}

					if (writeProperty)
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
					FailureMessages += ReadProperty(reader["name"], reader["value"]);
				}
				reader.Read(); // Advance.
			}

			// Restore the saved project.
			_Project = project;
			if (FailureMessages.Length > 0)
				throw new DocumenterPropertyFormatException(FailureMessages);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected string ReadProperty(string name, string value)
		{
			string FailureMessages="";
			PropertyInfo property = GetType().GetProperty(name);

			if (property == null)
			{
				FailureMessages += HandleUnknownPropertyType(name, value);
			}
			else
			{
				bool ValueParsedOK = false;
				object value2 = null;
						
				// if the string in the project file is not a valid member
				// of the enum, or cannot be parsed into the property type
				// for some reason,we don't want to throw an exception and
				// ditch all the settings stored later in the file!
				// save the exception details, and  we will throw a 
				// single exception at the end..
				try
				{
					if (property.PropertyType.IsEnum)
					{
						//parse is now case-insensitive...
						value2 = Enum.Parse(property.PropertyType, value, true);
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
					FailureMessages += HandleUnknownPropertyValue(property, value);
				}
				catch(System.FormatException)
				{
					FailureMessages += HandleUnknownPropertyValue(property, value);
				}
				// any other exception will be thrown immediately

				if (property.CanWrite && ValueParsedOK)
				{
					property.SetValue(this, value2, null);
				}
			}
			return FailureMessages;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected virtual string HandleUnknownPropertyType(string name, string value)
		{
			// As a default, we will ignore unknown property types
			return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected virtual string HandleUnknownPropertyValue(PropertyInfo property, string value)
		{
			// we cannot handle this, so return an error message
			return String.Format("     Property '{0}' has an invalid value for type {1} ('{2}') \n", property.Name, property.PropertyType.ToString() ,value);
		}

		#region Show Missing Documentation Options

		private bool _ShowMissingSummaries;

		/// <summary>Gets or sets the ShowMissingSummaries property.</summary>
		/// <remarks>If this is true, all members without /doc <b>&lt;summary&gt;</b>
		/// comments will contain the phrase <font color="red">Missing Documentation</font> in the
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

		private bool _ShowMissingRemarks;

		/// <summary>Gets or sets the ShowMissingRemarks property.</summary>
		/// <remarks>If this is true, all members without /doc <b>&lt;remarks&gt;</b>
		/// comments will contain the phrase <font color="red">Missing Documentation</font> in the
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

		private bool _ShowMissingParams;

		/// <summary>Gets or sets the ShowMissingParams property.</summary>
		/// <remarks>If this is true, all parameters without /doc <b>&lt;param&gt;</b>
		/// comments will contain the phrase <font color="red">Missing Documentation</font> in the
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

		private bool _ShowMissingReturns;

		/// <summary>Gets or sets the ShowMissingReturns property.</summary>
		/// <remarks>If this is true, all members without /doc <b>&lt;returns&gt;</b>
		/// comments will contain the phrase <font color="red">Missing Documentation</font> in the
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

		private bool _ShowMissingValues;

		/// <summary>Gets or sets the ShowMissingValues property.</summary>
		/// <remarks>If this is true, all properties without /doc <b>&lt;value&gt;</b>
		/// comments will contain the phrase <font color="red">Missing Documentation</font> in the
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

		#endregion

		#region Visibility Options
		
		private DocumentedInheritedMembers _DocumentInheritedMembers;

		/// <summary>Gets or sets the DocumentInheritedMembers property.</summary>
		/// <remarks>Determines what types of inherited members are documented. 
		/// Normally, only inherited instance members are documented.
		/// </remarks>
		[Category("Visibility")]
		[Description("Which inherited members to document.")]
		[DefaultValue(DocumentedInheritedMembers.Instance)]
		[System.ComponentModel.TypeConverter(typeof(NDoc.Core.EnumDescriptionConverter))]
		public DocumentedInheritedMembers DocumentInheritedMembers
		{
			get { return _DocumentInheritedMembers; }

			set
			{
				_DocumentInheritedMembers = value;
				SetDirty();
			}
		}

		private bool _DocumentInheritedFrameworkMembers;

		/// <summary>Gets or sets the DocumentInheritedFrameworkMembers property.</summary>
		/// <remarks>If true, members inherited from .Net framework classes will be documented. 
		/// </remarks>
		[Category("Visibility")]
		[Description("Which inherited members to document.")]
		[DefaultValue(true)]
		public bool DocumentInheritedFrameworkMembers
		{
			get { return _DocumentInheritedFrameworkMembers; }

			set
			{
				_DocumentInheritedFrameworkMembers = value;
				SetDirty();
			}
		}

		private bool _DocumentExplicitInterfaceImplementations;

		/// <summary>Gets or sets the DocumentInternals property.</summary>
		/// <remarks>If this is true, members which explicitly implement interfaces will
		/// be included in the documentation. Normally, these members are not documented.</remarks>
		[Category("Visibility")]
		[Description("Turn this flag on to document explicit interface implementations.")]
		[DefaultValue(false)]
		public bool DocumentExplicitInterfaceImplementations
		{
			get { return _DocumentExplicitInterfaceImplementations; }

			set
			{
				_DocumentExplicitInterfaceImplementations = value;
				SetDirty();
			}
		}

		private bool _DocumentInternals;

		/// <summary>Gets or sets the DocumentInternals property.</summary>
		/// <remarks>If this is true, types and members marked as internal will
		/// be included in the documentation. Normally, internal items are not documented.</remarks>
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

		private bool _DocumentProtected;

		/// <summary>Gets or sets the DocumentProtected property.</summary>
		/// <remarks>If this is true, protected members will be included in the
		/// documentation. Since protected members of non-internal types can be
		/// accessed outside of an assembly, this is true by default.</remarks>
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

		private bool _DocumentSealedProtected;

		/// <summary>Gets or sets the DocumentSealedProtected property.</summary>
		/// <remarks>Turn this flag on to document protected members of sealed classes. 
		/// <b>DocumentProtected</b> must be turned on, too.</remarks>
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

		private bool _DocumentPrivates;

		/// <summary>Gets or sets the DocumentPrivates property.</summary>
		/// <remarks>
		/// <para>If this is true, types and members marked as private will
		/// be included in the documentation.</para>
		/// <para>Normally private items are not documented. This is useful
		/// when use NDoc to create documentation intended for internal use.</para></remarks>
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
		/// <remarks>If this is true, NDoc will treat "protected internal" members as "protected" only.</remarks>
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

		private bool _DocumentEmptyNamespaces;

		/// <summary>Gets or sets the DocumentPrivates property.</summary>
		/// <remarks>If this is true, empty namespaces will be included in the documentation.
		/// Normally, empty namespaces are not documented.</remarks>
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

		private bool _SkipNamespacesWithoutSummaries;

		/// <summary>Gets or sets the SkipNamespacesWithoutSummaries property.</summary>
		/// <remarks>Setting this property to true , NDoc will not document namespaces 
		/// that don't have an associated namespace summary.</remarks>
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

		
		private EditorBrowsableFilterLevel _EditorBrowsableFilter;

		/// <summary>Specifies the level of filtering on the EditorBrowsable attribute.</summary>
		/// <remarks><para>Sets the level of filtering to apply on types/members marked with the <b>EditorBrowsable</b> attribute.  
		/// <b>Warning: enabling this filter might result in invalid links in the documentation.</b></para>
		/// <para>As of version 1.3 of NDoc, the <b>&lt;exclude/&gt;</b> tag is the preferred mechanism for
		/// suppressing the documentation of types or members.</para></remarks>
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

		#endregion

		#region Documentation Main Settings 
		
		private bool _IncludeAssemblyVersion;

		/// <summary>Gets or sets the IncludeAssemblyVersion property.</summary>
		/// <remarks>If this is true, the assembly version will appear at the bottom
		/// of each topic.</remarks>
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

		private string _CopyrightText;

		/// <summary>Gets or sets the CopyrightText property.</summary>
		/// <remarks>A textual copyright notice that will be included with each topic.</remarks>
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

		private string _CopyrightHref;

		/// <summary>Gets or sets the CopyrightHref property.</summary>
		/// <remarks>The URI of a copyright notice. A link to this URI will be included
		/// with each topic.</remarks>
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


		private string _ReferencesPath;

		/// <summary>Gets or sets the base directory used to resolve directory and assembly references.</summary>
		/// <remarks>The directory used to resolve path specifications and assembly references. 
		/// The search for assemblies includes this directory and all subdirectories.</remarks>
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

		private string _FeedbackEmailAddress  = string.Empty;

		/// <summary>Gets or sets the FeedbackEmailAddress property.</summary>
		/// <remarks>If an email address is supplied, a <b>mailto</b> link 
		/// will be placed at the bottom of each page, pointing to this address.</remarks>
		[Category("Documentation Main Settings")]
		[Description("If an email address is supplied, a mailto link will be placed at the bottom of each page using this address.")]
		[DefaultValue("")]
		public string FeedbackEmailAddress
		{
			get { return _FeedbackEmailAddress ; }
			set
			{
				_FeedbackEmailAddress = value;
				SetDirty();
			}
		}
		
		private bool _UseNamespaceDocSummaries;

		/// <summary>Gets or sets the UseNamespaceDocSummaries property.</summary>
		/// <remarks>If true, the documenter will look for a class with the name 
		/// <b>NamespaceDoc</b> in each namespace. The summary from that class 
		/// will then be used as the namespace summary.  The class itself will not 
		/// show up in the resulting documentation output. 
		/// <para>You may want to use <b>#if</b> ... <b>#endif</b>
		/// together with conditional compilation constants to 
		/// exclude the <b>NamespaceDoc</b> classes from release build assemblies.</para></remarks>
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

		private bool _AutoPropertyBackerSummaries;

		/// <summary>Gets or sets the AutoPropertyBackerSummaries property.</summary>
		/// <remarks>If true, the documenter will automatically add a summary 
		/// for fields which look like they back (hold the value for) a 
		/// property. The summary is only added if there is no existing summary, 
		/// which gives you a way to opt out of this behavior in particular cases. 
		/// Currently the naming conventions supported are such that 
		/// fields <b>_Length</b> and <b>length</b> will be inferred to back property <b>Length</b>.</remarks>
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

		private bool _AutoDocumentConstructors;

		/// <summary>Gets or sets the AutoDocumentConstructors property.</summary>
		/// <remarks>Turning this flag on will enable automatic summary 
		/// documentation for default constructors. If no summary for a parameter-less
		/// constructor is present, the default constructor summary of
		/// <b>Initializes a new instance of the CLASSNAME class</b> is inserted.</remarks>
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

		private bool _GetExternalSummaries;

		/// <summary>Load external xml files?</summary>
		/// <remarks>If true, NDoc will try loading external xml files to 
		/// retreive the summaries of inherited members. Setting this to true results 
		/// in more complete documentation as NDoc will attempt to retrieve summaries
		/// for any inherited members from types external to the project, including CLR types.</remarks>
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

		private bool _Preliminary = false;

		/// <summary>Get/set the Preliminary preoperty</summary>
		/// <remarks>
		/// <para>If true, NDoc will mark every topic as being preliminary documentation.
		/// Each topic will include a notice that the documentation is preliminary</para>
		/// <para>The default notice is <font color="red">[This is preliminary documentation 
		/// and subject to change.]</font></para></remarks>
		[Category("Documentation Main Settings")]
		[Description("If true, NDoc will mark every topic as being preliminary documentation.")]
		[DefaultValue(false)]
		public bool Preliminary
		{
			get { return _Preliminary; }

			set
			{
				_Preliminary = value;
				SetDirty();
			}
		}

		private string _UseNDocXmlFile = string.Empty;

		/// <summary>Gets or sets the UseNDocXmlFile property.</summary>
		/// <remarks><para>When set, NDoc will use the specified XML file as 
		/// input instead of reflecting the list of assemblies specified 
		/// on the project.</para>
		/// <para>Very useful for debugging documenters. <i>Leave empty for normal usage.</i></para>
		/// </remarks>
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

		private bool _CleanIntermediates = false;

		/// <summary>Gets or sets the CleanIntermediates property.</summary>
		/// <remarks>
		/// <para>When true, intermediate files will be deleted after a successful build.</para>
		/// <para>For documenters that result in a compiled output, like the MSDN and VS.NET
		/// documenters, intermediate files include all of the HTML Help project files, as well as the generated
		/// HTML files.</para></remarks>
		[Category("Documentation Main Settings")]
		[Description("When true, intermediate files will be deleted after a successful build.")]
		[DefaultValue(false)]
		public bool CleanIntermediates
		{
			get { return _CleanIntermediates; }
			set
			{
				_CleanIntermediates = value;
				SetDirty();
			}
		}
		

		SdkVersion _SdkDocVersion = SdkVersion.SDK_v1_1;

		/// <summary>Gets or sets the LinkToSdkDocVersion property.</summary>
		/// <remarks>Specifies to which version of the .NET Framework SDK documentation the links to system types will be pointing.</remarks>
		[Category("Documentation Main Settings")]
		[Description("Specifies to which version of the .NET Framework SDK documentation the links to system types will be pointing.")]
		[DefaultValue(SdkVersion.SDK_v1_1)]
		[System.ComponentModel.TypeConverter(typeof(NDoc.Core.EnumDescriptionConverter))]
		public SdkVersion SdkDocVersion
		{
			get { return _SdkDocVersion; }
			set
			{
				_SdkDocVersion = value;
				SetDirty();
			}
		}

		SdkLanguage _SdkDocLanguage = SdkLanguage.en;

		/// <summary>Gets or sets the SdkDocLanguage property.</summary>
		/// <remarks>Specifies to which Language of the .NET Framework SDK documentation the links to system types will be pointing.</remarks>
		[Category("Documentation Main Settings")]
		[Description("Specifies to which Language version of the .NET Framework SDK documentation the links to system types will be pointing.")]
		[DefaultValue(SdkLanguage.en)]
		[System.ComponentModel.TypeConverter(typeof(NDoc.Core.EnumDescriptionConverter))]
		public SdkLanguage SdkDocLanguage
		{
			get { return _SdkDocLanguage; }
			set
			{
				_SdkDocLanguage = value;
				SetDirty();
			}
		}

		#endregion
		
		#region Show Attributes Options

		private bool _DocumentAttributes;

		/// <summary>Gets or sets whether or not to document the attributes.</summary>
		/// <remarks>Set this to true to output the attributes of the types/members 
		/// in the syntax portion of topics.</remarks>
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
		/// <remarks>Set this to true to output the <b>TypeId</b> property in the attributes.</remarks>
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
		/// <remarks><para>When <b>DocumentAttributes</b> is set to true, this specifies 
		/// which attributes/property are visible.  Empty to show all.  </para>
		/// <para>Format: '&lt;attribute-name-starts-with&gt;,&lt;property-to-show&gt;,&lt;property-to-show&gt;|
		/// &lt;attribute-name-starts-with&gt;,&lt;property-to-show&gt;,&lt;property-to-show&gt;|(etc...)'.</para></remarks>
		[Category("Show Attributes")]
		[Description("When DocumentAttributes is set to true, this specifies which attributes/property are visible.  Empty to show all.  Format: '<attribute-name-starts-with>,<property-to-show>,<property-to-show>|<attribute-name-starts-with>,<property-to-show>,<property-to-show>|(etc...)'.")]
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

		#endregion

		#region Thread Safety Options

		private bool _IncludeDefaultThreadSafety  = true;

		/// <summary>Gets or sets the IncludeDefaultThreadSafety property.</summary>
		/// <remarks>When true, typs that do not have an explicit &lt;threadsafety&gt;
		/// tag will include thread safety documentation corresponding to StaticMembersDefaultToSafe 
		/// and InstanceMembersDefaultToSafe.
		/// </remarks>
		[Category("Thread Safety")]
		[Description("When true, typs that do not have an explicit <threadsafety> tag will include thread safety documentation corresponding to StaticMembersDefaultToSafe and InstanceMembersDefaultToSafe.")]
		[DefaultValue(true)]
		public bool IncludeDefaultThreadSafety 
		{
			get { return _IncludeDefaultThreadSafety; }
			set
			{
				_IncludeDefaultThreadSafety  = value;
				SetDirty();
			}
		}

		private bool _StaticMembersDefaultToSafe  = true;

		/// <summary>Gets or sets the StaticMembersDefaultToSafe property.</summary>
		/// <remarks>When true, types that do not have an explicit &lt;threadsafety&gt;
		/// tag will default to being safe for accessing static members across threads. 
		/// (ignored if IncludeDefaultThreadSafety is false)</remarks>
		[Category("Thread Safety")]
		[Description("When true, types that do not have an explicit <threadsafety> tag will default to being safe for accessing static members across threads. (ignored if IncludeDefaultThreadSafety is false)")]
		[DefaultValue(true)]
		public bool StaticMembersDefaultToSafe 
		{
			get { return _StaticMembersDefaultToSafe; }
			set
			{
				_StaticMembersDefaultToSafe  = value;
				SetDirty();
			}
		}

		private bool _InstanceMembersDefaultToSafe  = false;

		/// <summary>Gets or sets the InstanceMembersDefaultToSafe property.</summary>
		/// <remarks>When true, types that do not have an explicit &lt;threadsafety&gt;
		///  tag will default to being safe for accessing instance members across threads. 
		///  (ignored if IncludeDefaultThreadSafety is false)</remarks>
		[Category("Thread Safety")]
		[Description("When true, types that do not have an explicit <threadsafety> tag will default to being safe for accessing instance members across threads. (ignored if IncludeDefaultThreadSafety is false)")]
		[DefaultValue(false)]
		public bool InstanceMembersDefaultToSafe 
		{
			get { return _InstanceMembersDefaultToSafe; }
			set
			{
				_InstanceMembersDefaultToSafe  = value;
				SetDirty();
			}
		}
		#endregion

		#region Supported Platforms Options
		
		private bool _InheritPlatformSupport  = true;

		/// <summary>Gets or sets the InheritFrameworkSupport property.</summary>
		/// <remarks>When true, types and members that don't have specific 
		/// framework support specified, will display default 
		/// operating system and framework support values.</remarks>
		[Category("Supported Platforms")]
		[Description("When true, types and members that don't have specific framework support specified will display the default operating system and framework support values.")]
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

		private OSSupport _DefaultOSSupport  = OSSupport.all;

		/// <summary>Gets or sets the DefaultOSSupport property.</summary>
		/// <remarks>Defines the default set of operating systems 
		/// supported by classes that don't have OS support specified 
		/// in their comments (ignored if <b>InheritPlatformSupport</b> is false).
		/// The valid values are:
		/// <list type="bullet">
		/// <item>
		/// <description><b>all</b> - The default is any operating system that supports the .NET framework.
		/// (Windows 98/98SE, Windows ME, Windows NT4, Windows XP Pro and Home, Windows Server 2003)</description>
		/// </item>
		/// <item>
		/// <description><b>nt5plus</b> - The default operating system includes any NT5 derivative 
		/// (Windows 2000, Windows XP Pro and Home, Windows Server 2003)</description>
		/// </item>
		/// <item>
		/// <description><b>enterprise</b> - The default operating system is a enterprise system 
		/// (Window 2000, Windows XP Pro, Windows Server 2003)</description>
		/// </item>
		/// <item>
		/// <description><b>none</b> - no default operating system list is defined.</description>
		/// </item>
		/// </list>
		/// </remarks>
		[Category("Supported Platforms")]
		[Description("Defines the default set of operating systems supported by classes that don't have OS support specified in their comments (ignored if InheritPlatformSupport is false).")]
		[DefaultValue(OSSupport.all)]
		public OSSupport DefaultOSSupport 
		{
			get { return _DefaultOSSupport ; }
			set
			{
				_DefaultOSSupport  = value;
				SetDirty();
			}
		}

		private bool _SupportCompactFrameworkByDefault  = false;

		/// <summary>Gets or sets the DefaultSupportCompactFramework property.</summary>
		/// <remarks>If true, the .NET compact framework will 
		/// be included in the default set of platforms (ignored if <b>InheritPlatformSupport</b> is false).</remarks>
		[Category("Supported Platforms")]
		[Description("If true, the .NET compact framework will be included in the default set of platforms (ignored if InheritPlatformSupport is false).")]
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

		private bool _SupportMONOFrameworkByDefault  = false;

		/// <summary>Gets or sets the DefaultSupportMONOFramework property.</summary>
		/// <remarks>If true, the MONO open source framework will be included 
		/// in the default set of platforms (ignored if <b>InheritPlatformSupport</b> is false).</remarks>
		[Category("Supported Platforms")]
		[Description("If true, the MONO open source framework will be included in the default set of platforms (ignored if InheritPlatformSupport is false).")]
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

		private string _AdditionalFrameworkList  = string.Empty;

		/// <summary>Gets or sets the AdditionalFrameworkList property.</summary>
		/// <remarks>User defined list of additional framework implementations to 
		/// be displayed for default platform support (ignored if <b>InheritPlatformSupport</b> is false).</remarks>
		[Category("Supported Platforms")]
		[Description("User defined list of additional framework implementations to be displayed for default platform support (ignored if InheritPlatformSupport is false).")]
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

		private string _AdditionalOSList  = string.Empty;

		/// <summary>Gets or sets the AdditionalOSList property.</summary>
		/// <remarks>User defined list of additional operating systems to 
		/// be displayed for default platform support (ignored if <b>InheritPlatformSupport</b> is false).</remarks>
		[Category("Supported Platforms")]
		[Description("User defined list of additional operating systems to be displayed for default platform support (ignored if InheritPlatformSupport is false).")]
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
	
		#endregion
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
		all,

		/// <summary>
		/// A Windows OS from the NT 5 family of operating systems
		/// </summary>
		nt5plus,

		/// <summary>
		/// A enterprise operating system
		/// </summary>
		enterprise,

		/// <summary>
		/// Do not show a default list of operating systems
		/// </summary>
		none
	}

	/// <summary>
	/// Specifies a version of the .NET Framework documentation.
	/// </summary>
	public enum SdkVersion
	{
		/// <summary>The SDK version 1.0.</summary>
		[Description(".Net Version 1.0")]
		SDK_v1_0,

		/// <summary>The SDK version 1.1.</summary>
		[Description(".Net Version 1.1")]
		SDK_v1_1,
	}

	/// <summary>
	/// Specifies a language version of the .NET Framework documentation.
	/// </summary>
	public enum SdkLanguage
	{
		/// <summary>
		/// English
		/// </summary>
		[Description("English")] en,
		/// <summary>
		/// French
		/// </summary>
		[Description("French")] fr,
		/// <summary>
		/// German
		/// </summary>
		[Description("German")] de,
		/// <summary>
		/// Italian
		/// </summary>
		[Description("Italian")] it,
		/// <summary>
		/// Japanese
		/// </summary>
		[Description("Japanese")] ja,
		/// <summary>
		/// Korean
		/// </summary>
		[Description("Korean")] ko,
		/// <summary>
		/// Spanish
		/// </summary>
		[Description("Spanish")] es 
	}

	/// <summary>
	/// Specifies type of inherited members to document.
	/// </summary>
	public enum DocumentedInheritedMembers
	{
		/// <summary>
		/// None
		/// </summary>
		[Description("None")] None,
		/// <summary>
		/// French
		/// </summary>
		[Description("Instance Members")] Instance,
		/// <summary>
		/// German
		/// </summary>
		[Description("Instance and Static Members")] InstanceAndStatic
	}
}
