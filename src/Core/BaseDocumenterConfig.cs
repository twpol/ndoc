// BaseDocumenterConfig.cs - base XML documenter config class
// Copyright (C) 2001 Kral Ferch, Jason Diamond
// Parts Copyright (C) 2004  Kevin Downs
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
	/// This is a base class for NDoc Documenter Configs.  
	/// It implements all the methods required by the <see cref="IDocumenterConfig"/> interface. 
	/// It also provides some basic properties which are shared by all documenters. 
	/// </remarks>
	abstract public class BaseDocumenterConfig : IDocumenterConfig
	{
		private string _Name;

		/// <summary>Initializes a new instance of the DocumenterConfig class.</summary>
		protected BaseDocumenterConfig(string name)
		{
			_Name = name;
		}

		private Project _Project;
		/// <summary>
		/// 
		/// </summary>
		protected Project Project
		{
			get{return _Project;}
		}

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
		[Browsable(false)]
		public string Name
		{
			get { return _Name;}
		}

		/// <summary>Gets a list of properties.</summary>
		public IEnumerable GetProperties()
		{
			ArrayList properties = new ArrayList();

			foreach (PropertyInfo property in GetType().GetProperties())
			{
				object[] attr = property.GetCustomAttributes(typeof(BrowsableAttribute),true);
				if (attr.Length>0)
				{
					if( ((BrowsableAttribute)attr[0]).Browsable )
					{
						properties.Add(property);
					}
				}
				else
				{
					properties.Add(property);
				}
			}

			return properties;
		}

		/// <summary>
		/// Sets the value of a config property.
		/// </summary>
		/// <param name="name">The name of the property to set.</param>
		/// <param name="value">A string representation of the desired property value.</param>
		/// <remarks>Property name matching is case-insensitive.</remarks>
		public void SetValue(string name, string value)
		{
			name = name.ToLower();

			foreach (PropertyInfo property in GetType().GetProperties())
			{
				if (name == property.Name.ToLower())
				{
					string result = ReadProperty(property.Name, value);
					if (result.Length>0)
					{
						System.Diagnostics.Trace.WriteLine(result);
					}
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
				if (!property.IsDefined(typeof(NonPersistedAttribute),true))
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

						//being lazy and assuming only one BrowsableAttribute...
						BrowsableAttribute[] browsableAttributes=(BrowsableAttribute[])property.GetCustomAttributes(typeof(BrowsableAttribute),true);
						if (browsableAttributes.Length>0 && !browsableAttributes[0].Browsable)
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
			}

			writer.WriteEndElement();
		}

		/// <summary>Reads the previously serialized state of the documenter into memory.</summary>
		/// <param name="reader">An XmlReader positioned on a documenter element.</param>
		/// <remarks>This method uses reflection to set all of the public properties in the documenter.</remarks>
		public void Read(XmlReader reader)
		{
			// we don't want to set the project isdirty flag during the read...
			_Project.SuspendDirtyCheck=true;

			string FailureMessages="";

			while(!reader.EOF && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "documenter"))
			{
				if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
				{
					FailureMessages += ReadProperty(reader["name"], reader["value"]);
				}
				reader.Read(); // Advance.
			}

			// Restore the project IsDirty checking.
			_Project.SuspendDirtyCheck=false;
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
			// if value is an empty string, do not bother with anything else
			if (value==null) return String.Empty;
			if (value.Length==0) return String.Empty;

			string FailureMessages=String.Empty;
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
						TypeConverter tc = TypeDescriptor.GetConverter(property.PropertyType);
						value2 = tc.ConvertFromString(value);
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


		#region Documentation Main Settings 

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
		

		#endregion
		
	}

	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class NonPersistedAttribute : Attribute
	{
	}
}
