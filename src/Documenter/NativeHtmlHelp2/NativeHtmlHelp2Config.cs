// MsdnDocumenter.cs - a MSDN-like documenter
// Copyright (C) 2003 Don Kackman
// Parts copyright 2001  Kral Ferch, Jason Diamond
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
using System.IO;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;

using NDoc.Core;

namespace NDoc.Documenter.NativeHtmlHelp2
{
	/// <summary>
	/// Represents the character set  that will be used when compiling the Hxs file
	/// </summary>
	public enum CharacterSet
	{
		/// <summary>
		/// Ascii characters set
		/// </summary>
		Ascii,
		/// <summary>
		/// UTF 8 character set
		/// </summary>
		UTF8,
		/// <summary>
		/// Unicode chacracters
		/// </summary>
		Unicode
	}

	/// <summary>
	/// Specifies a version of the .NET Framework documentation.
	/// </summary>
	public enum SdkDocVersion
	{
		/// <summary>The SDK version 1.0.</summary>
		SDK_v1_0,

		/// <summary>The SDK version 1.1.</summary>
		SDK_v1_1,
	}

	/// <summary>
	/// Specifies how the collection will be integrated with the help browser
	/// </summary>
	public enum TOCStyle
	{
		/// <summary>
		/// Each root topic in the TOC is appears at the plug in point
		/// </summary>
		Flat,

		/// <summary>
		/// Creates a root node in the browser at the plug in point
		/// </summary>
		Hierarchical
	}

	/// <summary>
	/// Config settings for the native Html Help 2 Documenter
	/// </summary>
	public class NativeHtmlHelp2Config : BaseDocumenterConfig
	{
		private const string HTMLHELP2_CONFIG_CATEGORY = "Html Help 2 Settings";
		private const string DEPLOYMENT_CATEGORY = "Html Help 2 Deployment";
		private const string ADDITIONAL_CONTENT_CATEGORY = "Html Help 2 Additional Content";

		/// <summary>Initializes a new instance of the NativeHtmlHelp2Config class.</summary>
		public NativeHtmlHelp2Config() : base( "VS.NET 2003" )
		{
		}

		#region Main Settings properties
		string _outputDirectory = @".\doc\";
		
		/// <summary>Gets or sets the OutputDirectory property.</summary>
		[Category("Documentation Main Settings")]
		[Description("The directory in which .html files and the .Hx* files will be generated.")]
#if !MONO //System.Windows.Forms.Design.FolderNameEditor is not implemented in mono 0.28
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
#endif
		public string OutputDirectory
		{
			get { return _outputDirectory; }

			set
			{
				_outputDirectory = value;

				if (!_outputDirectory.EndsWith("\\"))
				{
					_outputDirectory += "\\";
				}

				SetDirty();
			}
		}

		string _htmlHelpName = "Documentation";

		/// <summary>Gets or sets the HtmlHelpName property.</summary>
		/// <remarks>The HTML Help project file and the compiled HTML Help file
		/// use this property plus the appropriate extension as names.</remarks>
		[Category("Documentation Main Settings")]
		[Description("The name of the HTML Help project and the Compiled HTML Help file.")]
		public string HtmlHelpName
		{
			get { return _htmlHelpName; }

			set 
			{ 
				if (Path.GetExtension(value).ToLower() == ".hxs") 
				{
					HtmlHelpName = Path.GetFileNameWithoutExtension(value);
				}
				else
				{
					_htmlHelpName = value; 
				}

				SetDirty();
			}
		}

		private string _Title = "An NDoc documented library";

		/// <summary>Gets or sets the Title property.</summary>
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

		private bool _IncludeHierarchy = false;

		/// <summary>Gets or sets the IncludeHierarchy property.</summary>
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
		#endregion

		#region Deployment properties
		bool _RegisterTitleWithNamespace = false;

		/// <summary>
		/// Should the compiled Html 2 title be registered after it is compiled. (If true CollectionNamespace is required)
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("Should the compiled Html 2 title be registered on this machine after it is compiled. Good for testing. (If true CollectionNamespace is required)")]
		[DefaultValue(false)]
		public bool RegisterTitleWithNamespace
		{
			get { return _RegisterTitleWithNamespace; }

			set
			{
				_RegisterTitleWithNamespace = value;
				SetDirty();
			}
		}

		string _CollectionNamespace = String.Empty;

		/// <summary>
		/// If RegisterTitleWithNamespace is true this is the namesapce to which it will be added.
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("The Html Help 2 registry namespace (avoid spaces). Used in conjunction with GenerateCollectionFiles and RegisterTitleWithNamespace")]
		[DefaultValue("")]
		public string CollectionNamespace
		{
			get { return _CollectionNamespace; }

			set
			{
				_CollectionNamespace = value;
				SetDirty();
			}
		}		

		bool _RegisterTitleAsCollection = false;

		/// <summary>
		/// If true the HxS title will be registered as a collection (ignored if RegisterTitleWithNamespace is ture)
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("If true the HxS title will be registered as a collection on this machine. Good for testing. (ignored if RegisterTitleWithNamespace is true)")]
		[DefaultValue(false)]
		public bool RegisterTitleAsCollection
		{
			get { return _RegisterTitleAsCollection; }

			set
			{
				_RegisterTitleAsCollection = value;
				SetDirty();
			}
		}	

		bool _GenerateCollectionFiles = false;

		/// <summary>
		/// If true creates collection files to contain the help title.
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("If true creates collection files to contain the help title. These all the title to be plugged into the Visual Studio help namespace during deployment.")]
		[DefaultValue(false)]
		public bool GenerateCollectionFiles
		{
			get { return _GenerateCollectionFiles; }

			set
			{
				_GenerateCollectionFiles = value;
				SetDirty();
			}
		}	

		string _PlugInNamespace = "ms.vscc";

		/// <summary>
		/// If GenerateCollectionFiles is true, the resulting collection will be plugged into this namespace during deployment
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("If GenerateCollectionFiles is true, the resulting collection will be plugged into this namespace during deployment. ('ms.vscc' is the VS.NET help namespace)")]
		[DefaultValue("ms.vscc")]
		public string PlugInNamespace
		{
			get { return _PlugInNamespace; }

			set
			{
				_PlugInNamespace = value;
				SetDirty();
			}
		}

		
		TOCStyle _CollectionTOCStyle = TOCStyle.Hierarchical;

		/// <summary>
		/// Determines how the collection table of contents will appear in the help browser
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("Determines how the collection table of contents will appear in the help browser")]
		[DefaultValue(TOCStyle.Hierarchical)]
		public TOCStyle CollectionTOCStyle
		{
			get { return _CollectionTOCStyle; }

			set
			{
				_CollectionTOCStyle = value;
				SetDirty();
			}
		}
		#endregion

		#region HTML Help 2 properties

		SdkDocVersion _LinkToSdkDocVersion = SdkDocVersion.SDK_v1_1;

		/// <summary>Gets or sets the LinkToSdkDocVersion property.</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("Specifies to which version of the .NET Framework SDK documentation the links to system types will be pointing.")]
		[DefaultValue(SdkDocVersion.SDK_v1_1)]
		public SdkDocVersion LinkToSdkDocVersion
		{
			get { return _LinkToSdkDocVersion; }
			set
			{
				_LinkToSdkDocVersion = value;
				SetDirty();
			}
		}

		CharacterSet _CharacterSet = CharacterSet.UTF8;
		/// <summary>
		/// Gets or sets the character set that will be used when compiling the help file.
		/// Defaults to UTF8.
		/// </summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("Gets or sets the character set that will be used when compiling the help file")]
		[DefaultValue(CharacterSet.UTF8)]
		public CharacterSet CharacterSet
		{
			get{ return _CharacterSet; }
			set
			{
				_CharacterSet = value;
				SetDirty();
			}
		}
		
		short _LangID = 1033;

		/// <summary>The language ID of the locale used by the compiled helpfile</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("The ID of the language the help file is in.")]
		[DefaultValue(1033)]
		public short LangID
		{
			get { return _LangID; }

			set
			{
				_LangID = value;
				SetDirty();
			}
		}	

		bool _BuildSeparateIndexFile = false;

		/// <summary>If true a seperate index file is generated, otherwise it is compiled into the HxS (recommended)</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("If true, create a separate index file (HxI), otherwise the index is compiled into the HxS file.")]
		[DefaultValue(false)]
		public bool BuildSeparateIndexFile
		{
			get { return _BuildSeparateIndexFile; }

			set
			{
				_BuildSeparateIndexFile = value;
				SetDirty();
			}
		}


		string _Version = "1.0.0.0";

		/// <summary>Get's or sets the version number for the help file</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("The version number for the help file (#.#.#.#)")]
		[DefaultValue("1.0.0.0")]
		public string Version
		{
			get { return _Version; }

			set
			{
				_Version = value;
				SetDirty();
			}
		}
	

		bool _CreateFullTextIndex = true;

		/// <summary>If true creates a full text index for the help file</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("If true creates a full text index for the help file")]
		[DefaultValue(true)]
		public bool CreateFullTextIndex
		{
			get { return _CreateFullTextIndex; }

			set
			{
				_CreateFullTextIndex = value;
				SetDirty();
			}
		}

		bool _IncludeDefaultStopWordList = true;

		/// <summary>If true the default stop word list is compiled into the help file. 
		/// (A stop word list is a list of words that will be ignored during a full text search)</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("If true the default stop word list is compiled into the help file. (A stop word list is a " +
			"list of words that will be ignored during a full text search)")]
		[DefaultValue(true)]
		public bool IncludeDefaultStopWordList
		{
			get { return _IncludeDefaultStopWordList; }

			set
			{
				_IncludeDefaultStopWordList = value;
				SetDirty();
			}
		}

		string _UseHelpNamespaceMappingFile = string.Empty;

		/// <summary>If the documentation includes references to types registered in a seperate html help 2
		/// namespace, supplying a mapping file allows XLinks to be created to topics within that namespace.
		/// </summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("If the documentation includes references to types registered in a seperate html help 2 " +
			 "namespace, supplying a mapping file allows XLinks to be created to topics within that namespace. " +
			 "The schema for the mapping file can be found in the location you installed NDoc in a file named " +
			 "'NamespaceMap.xsd'")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string UseHelpNamespaceMappingFile
		{
			get { return _UseHelpNamespaceMappingFile; }

			set
			{
				_UseHelpNamespaceMappingFile = value;
				SetDirty();
			}
		}

		
		string _HeaderHtml;

		/// <summary>Gets or sets the HeaderHtml property.</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("Raw HTML that is used as a page header instead of the default blue banner. " +
			 "\"%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. " +
			 "\"%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.")]
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
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("Raw HTML that is used as a page footer instead of the default footer." +
			 "\"%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. " +
			 "\"%ASSEMBLY_NAME%\" is dynamically replaced by the name of the assembly for the current page. " +
			 "\"%ASSEMBLY_VERSION%\" is dynamically replaced by the version of the assembly for the current page. " +
			 "\"%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.")]
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
		#endregion

		#region Additonal content properties
		
		string _IntroductionPage = string.Empty;

		/// <summary>An HTML page that will be dispayed when the root TOC node is selected</summary>
		[Category(ADDITIONAL_CONTENT_CATEGORY)]
		[Description("An HTML page that will be dispayed when the root TOC node is selected")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string IntroductionPage
		{
			get { return _IntroductionPage; }

			set
			{
				_IntroductionPage = value;
				SetDirty();
			}
		}
		
		string _AboutPageInfo = string.Empty;

		/// <summary>Displays product information in Help About.</summary>
		[Category(ADDITIONAL_CONTENT_CATEGORY)]
		[Description("Displays product information in Help About.")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string AboutPageInfo
		{
			get { return _AboutPageInfo; }

			set
			{
				_AboutPageInfo = value;
				SetDirty();
			}
		}

		string _EmptyIndexTermPage = string.Empty;

		/// <summary>Displays when a user chooses a keyword index term that has subkeywords but is not directly associated with a topic itself.</summary>
		[Category(ADDITIONAL_CONTENT_CATEGORY)]
		[Description("Displays when a user chooses a keyword index term that has subkeywords but is not directly associated with a topic itself.")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string EmptyIndexTermPage
		{
			get { return _EmptyIndexTermPage; }

			set
			{
				_EmptyIndexTermPage = value;
				SetDirty();
			}
		}		

		string _NavFailPage = string.Empty;

		/// <summary>Opens if a link to a topic or URL is broken.</summary>
		[Category(ADDITIONAL_CONTENT_CATEGORY)]
		[Description("Opens if a link to a topic or URL is broken.")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string NavFailPage
		{
			get { return _NavFailPage; }

			set
			{
				_NavFailPage = value;
				SetDirty();
			}
		}	
	
		string _AboutPageIconPage = string.Empty;

		/// <summary>HTML file that displays the Help About image.</summary>
		[Category(ADDITIONAL_CONTENT_CATEGORY)]
		[Description("HTML file that displays the Help About image.")]
		[DefaultValue("")]
		[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
		public string AboutPageIconPage
		{
			get { return _AboutPageIconPage; }

			set
			{
				_AboutPageIconPage = value;
				SetDirty();
			}
		}		

		string _AdditionalContentResourceDirectory = string.Empty;

		/// <summary>Directory that contains resources (images etc.) used by the additional content pages. This directory will be recursively compiled into the help file.</summary>
		[Category(ADDITIONAL_CONTENT_CATEGORY)]
		[Description("Directory that contains resources (images etc.) used by the additional content pages. This directory will be recursively compiled into the help file.")]
		[DefaultValue("")]
#if !MONO //System.Windows.Forms.Design.FolderNameEditor is not implemented in mono 0.28
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
#endif
		public string AdditionalContentResourceDirectory
		{
			get { return _AdditionalContentResourceDirectory; }

			set
			{
				_AdditionalContentResourceDirectory = value;
				SetDirty();
			}
		}	
		#endregion
	}
}
