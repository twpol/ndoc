// MsdnDocumenterConfig.cs - the MsdnHelp documenter config class
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;

using NDoc.Core;

namespace NDoc.Documenter.Msdn
{
	/// <summary>The MsdnDocumenterConfig class.</summary>
	/// <remarks>
	/// <para>The MSDN documenter creates a compiled HTML help version 1 help file (CHM).</para>
	/// </remarks>
	public class MsdnDocumenterConfig : BaseDocumenterConfig
	{
		string outputDirectory;
		string htmlHelpName;
		bool includeFavorites;

		/// <summary>Initializes a new instance of the MsdnHelpConfig class.</summary>
		public MsdnDocumenterConfig() : this("MSDN")
		{
		}

		/// <summary>
		/// Constructor used by derived classes
		/// </summary>
		/// <param name="name">The name of the derived class config</param>
		protected MsdnDocumenterConfig( string name ) : base( name )
		{
			// fix for bug 884121 - OutputDirectory on Linux
			outputDirectory = string.Format(".{0}doc{0}",Path.DirectorySeparatorChar );

			htmlHelpName = "Documentation";

			_Title = "An NDoc Documented Class Library";

			_SortTOCByNamespace = true;
			_SplitTOCs = false;
			_BinaryTOC = true;
			_DefaultTOC = string.Empty;

			_IncludeHierarchy = false;
			_ShowVisualBasic = false;
			_OutputTarget = OutputType.HtmlHelpAndWeb;

			_RootPageContainsNamespaces = false;

			_HeaderHtml = string.Empty;
			_FooterHtml = string.Empty;
			_FilesToInclude = string.Empty;

		}

		#region Main Settings
		/// <summary>Gets or sets the OutputDirectory property.</summary>
		/// <remarks>The directory in which .html files and the .chm file will be generated.</remarks>
		[Category("Documentation Main Settings")]
		[Description("The directory in which .html files and the .chm file will be generated.")]
#if !MONO //System.Windows.Forms.Design.FolderNameEditor is not implemented in mono 0.28
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
#endif
		public string OutputDirectory
		{
			get { return outputDirectory; }

			set
			{
				if ( value.IndexOfAny(new char[]{'#','?', ';'}) != -1) 
				{
					throw new FormatException("Output Directory '" + value + 
						"' is not valid because it contains '#','?', or ';' which" +
						" are reserved characters in HTML URLs."); 
				}

				outputDirectory = value;

				if (!outputDirectory.EndsWith( Path.DirectorySeparatorChar.ToString() ))
				{
					outputDirectory += Path.DirectorySeparatorChar;
				}

				SetDirty();
			}
		}

		/// <summary>Gets or sets the HtmlHelpName property.</summary>
		/// <remarks>The HTML Help project file and the compiled HTML Help file
		/// use this property plus the appropriate extension as names.</remarks>
		[Category("Documentation Main Settings")]
		[Description("The name of the HTML Help project and the Compiled HTML Help file.")]
		public string HtmlHelpName
		{
			get { return htmlHelpName; }

			set 
			{ 
				if (Path.GetExtension(value).ToLower() == ".chm") 
				{
					HtmlHelpName = Path.GetFileNameWithoutExtension(value);
				}
				else
				{
					htmlHelpName = value; 
				}

				SetDirty();
			}
		}

		private string _Title;

		/// <summary>Gets or sets the Title property.</summary>
		/// <remarks>This is the title displayed at the top of every page.</remarks>
		[Category("Documentation Main Settings")]
		[Description("This is the title displayed at the top of every page.")]
		public string Title
		{
			get { return _Title; }

			set 
			{ 
				_Title = value; 
				SetDirty();
			}
		}

		private bool _IncludeHierarchy;

		/// <summary>Gets or sets the IncludeHierarchy property.</summary>
		/// <remarks>To include a class hiararchy page for each 
		/// namespace. Don't turn it on if your project has a namespace 
		/// with many types, as NDoc will become very slow and might crash.</remarks>
		[Category("Documentation Main Settings")]
		[Description("To include a class hiararchy page for each namespace. Don't turn it on if your project has a namespace with many types, as NDoc will become very slow and might crash.")]
		[DefaultValue(false)]
		public bool IncludeHierarchy
		{
			get { return _IncludeHierarchy; }

			set 
			{ 
				_IncludeHierarchy = value; 
				SetDirty();
			}
		}

		private bool _ShowVisualBasic;

		/// <summary>Gets or sets the ShowVisualBasic property.</summary>
		/// <remarks>This is a temporary property until we get a working
		/// language filter in the output like MSDN.</remarks>
		[Category("Documentation Main Settings")]
		[Description("Show Visual Basic syntax for types and members.")]
		[DefaultValue(false)]
		public bool ShowVisualBasic
		{
			get { return _ShowVisualBasic; }

			set 
			{ 
				_ShowVisualBasic = value; 
				SetDirty();
			}
		}

		private OutputType _OutputTarget;

		/// <summary>Gets or sets the OutputTarget property.</summary>
		/// <remarks>Sets the output type to HTML Help (.chm) or Web or both.</remarks>
		[Category("Documentation Main Settings")]
		[Description("Sets the output type to HTML Help (.chm) or Web or both.")]
		[DefaultValue(OutputType.HtmlHelpAndWeb)]
		[System.ComponentModel.TypeConverter(typeof(NDoc.Core.EnumDescriptionConverter))]
		public OutputType OutputTarget
		{
			get { return _OutputTarget; }

			set 
			{ 
				_OutputTarget = value; 
				SetDirty();
			}
		}

		bool _SdkLinksOnWeb = false;

		/// <summary>Gets or sets the SdkLinksOnWeb property.</summary>
		/// <remarks>
		/// </remarks>
		[Category("Documentation Main Settings")]
		[Description("Turning this flag on will point all SDK links to the online MSDN library")]
		[DefaultValue(false)]
		public bool SdkLinksOnWeb
		{
			get { return _SdkLinksOnWeb; }

			set
			{
				_SdkLinksOnWeb = value;
				SetDirty();
			}
		}

		#endregion

		/// <summary>Gets or sets the IncludeFavorites property.</summary>
		/// <remarks>Turning this flag on will include a Favorites tab in the HTML Help file.</remarks>
		[Category("HTML Help Options")]
		[Description("Turning this flag on will include a Favorites tab in the HTML Help file.")]
		[DefaultValue(false)]
		public bool IncludeFavorites
		{
			get { return includeFavorites; }

			set 
			{ 
				includeFavorites = value; 
				SetDirty();
			}
		}

		private bool _SplitTOCs ;

		/// <summary>Gets or sets the SplitTOCs property.</summary>
		/// <remarks>Turning this flag on will generate a separate table-of-contents for each assembly. 
		/// It cannot be set if SortTOCByNamespace is set or RootPageFileName is specified.</remarks>
		[Category("HTML Help Options")]
		[Description("Turning this flag on will generate a separate table-of-contents for each assembly. "
			 + "It cannot be set if SortTOCByNamespace is set or RootPageFileName is specified.")]
		[DefaultValue(false)]
		private bool SplitTOCs
		{
			get { return _SplitTOCs; }

			set 
			{
				if ((!_SortTOCByNamespace) && (_RootPageFileName.Length == 0))
				{
					_SplitTOCs = value; 
					SetDirty();
				}
			}
		}

		private string _DefaultTOC;

		/// <summary>Gets or sets the DefaultTOC property.</summary>
		/// <remarks>When SplitTOCs is true, this represents the default table-of-contents to use.</remarks>
		[Category("HTML Help Options")]
		[Description("When SplitTOCs is true, this represents the default table-of-contents to use.")]
		[DefaultValue("")]
		private string DefaultTOC
		{
			get { return _DefaultTOC; }

			set 
			{ 
				_DefaultTOC = value; 
				SetDirty();
			}
		}

		string _RootPageTOCName;

		/// <summary>Gets or sets the RootPageTOCName property.</summary>
		/// <remarks>The name for the table-of-contents entry corresponding 
		/// to the root page.
		/// If this is not specified and RootPageFileName is, then
		/// the TOC entry will be 'Overview'.</remarks>
		[Category("HTML Help Options")]
		[Description("The name for the table-of-contents entry corresponding "
			 + " to the root page."
			 + " If this is not specified and RootPageFileName is, then"
			 + " the TOC entry will be 'Overview'.")]
		[DefaultValue("")]
		public string RootPageTOCName
		{
			get { return _RootPageTOCName; }

			set
			{
				_RootPageTOCName = value;
				SetDirty();
			}
		}

		string _RootPageFileName = string.Empty;

		/// <summary>Gets or sets the RootPageFileName property.</summary>
		/// <remarks>The name of an html file to be included as the root home page. "
		/// SplitTOCs is disabled when this property is set.</remarks>
		[Category("HTML Help Options")]
		[Description("The name of an html file to be included as the root home page. "
			 + "SplitTOCs is disabled when this property is set.")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string RootPageFileName
		{
			get { return _RootPageFileName; }

			set
			{
				_RootPageFileName = value;
				_SplitTOCs = _SplitTOCs && (value.Length == 0);
				SetDirty();
			}
		}

		bool _RootPageContainsNamespaces;

		/// <summary>Gets or sets the RootPageContainsNamespaces property.</summary>
		/// <remarks>If true, the Root Page will be made the container
		/// of the namespaces in the table-of-contents.
		/// If false, the Root Page will be made a peer of
		/// the namespaces in the table-of-contents.</remarks>
		[Category("HTML Help Options")]
		[Description("If true, the Root Page will be made the container"
			 + " of the namespaces in the table-of-contents."
			 + " If false, the Root Page will be made a peer of"
			 + " the namespaces in the table-of-contents.")]
		[DefaultValue(false)]
		public bool RootPageContainsNamespaces
		{
			get { return _RootPageContainsNamespaces; }

			set
			{
				_RootPageContainsNamespaces = value;
				SetDirty();
			}
		}

		bool _SortTOCByNamespace;

		/// <summary>Gets or sets the SortTOCByNamespace property.</summary>
		/// <remarks>Sorts the table-of-contents by namespace name. 
		/// SplitTOCs is disabled when this option is selected.</remarks>
		[Category("HTML Help Options")]
		[Description("Sorts the table-of-contents by namespace name. "
			 + "SplitTOCs is disabled when this option is selected.")]
		private bool SortTOCByNamespace
		{
			get { return _SortTOCByNamespace; }

			set
			{
				_SortTOCByNamespace = value;
				_SplitTOCs = _SplitTOCs && !value;
				SetDirty();
			}
		}

		bool _BinaryTOC;

		/// <summary>Gets or sets the BinaryToc property.</summary>
		/// <remarks>Create a binary table-of-contents file. 
		/// This can significantly reduce the amount of time 
		/// required to load a very large help document.</remarks>
		[Category("HTML Help Options")]
		[Description("Create a binary table-of-contents file. \r"
			 + "This can significantly reduce the amount of time required to load a very large help document.")]
		[DefaultValue(true)]
		public bool BinaryTOC
		{
			get { return _BinaryTOC; }

			set
			{
				_BinaryTOC = value;
				SetDirty();
			}
		}

		string _HeaderHtml;

		/// <summary>Gets or sets the HeaderHtml property.</summary>
		/// <remarks>Raw HTML that is used as a page header instead of the default blue banner. 
		/// "%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. 
		/// "%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.</remarks>
		[Category("HTML Help Options")]
		[Description("Raw HTML that is used as a page header instead of the default blue banner. " +
			 "\"%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. " +
			 "\"%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.")]
		[DefaultValue("")]
		[Editor(typeof(TextEditor), typeof(UITypeEditor))]
		public string HeaderHtml
		{
			get { return _HeaderHtml; }

			set
			{
				_HeaderHtml = value;
				SetDirty();
			}
		}

		string _FooterHtml;

		/// <summary>Gets or sets the FooterHtml property.</summary>
		/// <remarks>Raw HTML that is used as a page footer instead of the default footer.
		/// "%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. 
		/// "%ASSEMBLY_NAME%\" is dynamically replaced by the name of the assembly for the current page. 
		/// "%ASSEMBLY_VERSION%\" is dynamically replaced by the version of the assembly for the current page. 
		/// "%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.</remarks>
		[Category("HTML Help Options")]
		[Description("Raw HTML that is used as a page footer instead of the default footer." +
			 "\"%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. " +
			 "\"%ASSEMBLY_NAME%\" is dynamically replaced by the name of the assembly for the current page. " +
			 "\"%ASSEMBLY_VERSION%\" is dynamically replaced by the version of the assembly for the current page. " +
			 "\"%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.")]
		[DefaultValue("")]
		[Editor(typeof(TextEditor), typeof(UITypeEditor))]
		public string FooterHtml
		{
			get { return _FooterHtml; }

			set
			{
				_FooterHtml = value;
				SetDirty();
			}
		}

		string _FilesToInclude;

		/// <summary>Gets or sets the FilesToInclude property.</summary>
		/// <remarks>Specifies external files that must be included 
		/// in the compiled CHM file. Multiple files must be separated by a pipe ('|').</remarks>
		[Category("HTML Help Options")]
		[Description("Specifies external files that must be included in the compiled CHM file. Multiple files must be separated by a pipe ('|').")]
		[DefaultValue("")]
		public string FilesToInclude
		{
			get { return _FilesToInclude; }

			set
			{
				_FilesToInclude = value;
				SetDirty();
			}
		}

		string _ExtensibilityStylesheet = string.Empty;

		/// <summary>Path to an xslt stylesheet that contains templates for documenting extensibility tags</summary>
		/// <remarks>Path to an xslt stylesheet that contains templates for documenting extensibility tags. 
		/// </remarks>
		[Category("Extensibility")]
		[Description("Path to an xslt stylesheet that contains templates for documenting extensibility tags. Refer to the NDoc user's guide for more details on extending NDoc.")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string ExtensibilityStylesheet
		{
			get { return _ExtensibilityStylesheet; }

			set
			{
				_ExtensibilityStylesheet = value;
				SetDirty();
			}
		}
	
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected override string HandleUnknownPropertyType(string name, string value)
		{
			string FailureMessages="";
			//SdkDocVersion has been split into two separate options
			// - value "MsdnOnline" replaced by "SDK_v1_1" and setting option SdkLinksOnWeb to true
			//note: case insensitive comparison
			if (String.Compare(name,"LinkToSdkDocVersion",true) == 0) 
			{
				if (String.Compare(value,"MsdnOnline",true) == 0)
				{
					Trace.WriteLine("WARNING: " + base.Name + " Configuration - value 'MsdnOnline' of property 'LinkSdkDocVersion' is OBSOLETE. Please use new option 'SdkLinksOnWeb'\n");
					FailureMessages += base.ReadProperty("SdkDocVersion", "SDK_v1_1");
					FailureMessages += base.ReadProperty("SdkLinksOnWeb", "True");
				}
				else
				{
					Trace.WriteLine("WARNING: " + base.Name + " Configuration - property 'LinkToSdkDocVersion' is OBSOLETE. Please use new property 'SdkDocVersion'\n");
					FailureMessages += base.ReadProperty("SdkDocVersion", value);
				}
			}
			else
			{
				// if we don't know how to handle this, let the base class have a go
				FailureMessages = base.HandleUnknownPropertyType (name, value);
			}
			return FailureMessages;
		}
	}

	/// <summary>
	/// Defines the output types for this documenter.
	/// </summary>
	[Flags]
	public enum OutputType
	{
		/// <summary>Output only an HTML Help file (.chm).</summary>
		[Description("HTML Help")]
		HtmlHelp = 1,

		/// <summary>Output only Web pages.</summary>
		[Description("Web")]
		Web = 2,

		/// <summary>Output both HTML Help and Web.</summary>
		[Description("HTML Help and Web")]
		HtmlHelpAndWeb = HtmlHelp | Web
	}
}
