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

using Microsoft.Win32;

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
	/// Config settings for the native Html Help 2 Documenter
	/// </summary>
	public class NativeHtmlHelp2Config : BaseDocumenterConfig
	{
		private const string HTMLHELP2_CONFIG_CATEGORY = "Html Help v2.0 Settings";
		private const string DEPLOYMENT_CATEGORY = "Html Help 2 Deployment";

		/// <summary>Initializes a new instance of the NativeHtmlHelp2Config class.</summary>
		public NativeHtmlHelp2Config() : base( "Native HtmlHelp2" )
		{
		}

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


		bool _RegisterTitleWithNamespace = false;

		/// <summary>
		/// Should the compiled Html 2 title be registered after it is compiled. (If true ParentCollectionNamespace is required)
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("Should the compiled Html 2 title be registered on this machine after it is compiled. (If true ParentCollectionNamespace is required)")]
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

		string _ParentCollectionNamespace = String.Empty;

		/// <summary>
		/// If RegisterTitleWithNamespace is true this is the namesapce to which it will be added.
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("The Html Help 2 registry namespace (avoid spaces). Only used if RegisterTitleWithNamespace is True.")]
		public string ParentCollectionNamespace
		{
			get { return _ParentCollectionNamespace; }

			set
			{
				_ParentCollectionNamespace = value;
				SetDirty();
			}
		}		

		bool _RegisterTitleAsCollection = false;

		/// <summary>
		/// If true the HxS title will be registered as a collection (ignored if RegisterTitleWithNamespace is ture)
		/// </summary>
		[Category(DEPLOYMENT_CATEGORY)]
		[Description("If true the HxS title will be registered as a collection (ignored if RegisterTitleWithNamespace is ture)")]
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

		
		bool _BuildSeperateIndexFile = false;

		/// <summary>If true a seperate index file is generated, otherwise it is compiled into the HxS (recommended)</summary>
		[Category(HTMLHELP2_CONFIG_CATEGORY)]
		[Description("If true, create a seperate index file (HxI), otherwise the index is compiled into the HxS file.")]
		[DefaultValue(false)]
		public bool BuildSeperateIndexFile
		{
			get { return _BuildSeperateIndexFile; }

			set
			{
				_BuildSeperateIndexFile = value;
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
		[Description("If true the default stop word list is compiled into the help file. (A stop word list is a list of words that will be ignored during a full text search)")]
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

		string _HeaderHtml;

		/// <summary>Gets or sets the HeaderHtml property.</summary>
		[Category("HTML Help Options")]
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
		[Category("HTML Help Options")]
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

		#region HxComp location stuff
		private static string _HtmlHelp2CompilerPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
			"Microsoft Help 2.0 SDK");

		private static bool FindHxComp()
		{
			return File.Exists(Path.Combine(_HtmlHelp2CompilerPath, "hxcomp.exe"));
		}

		internal static string HtmlHelp2CompilerPath
		{
			get
			{
				if (FindHxComp())
				{
					return _HtmlHelp2CompilerPath;
				}

				//not in default dir, try to locate it from the registry
				RegistryKey key = Registry.ClassesRoot.OpenSubKey("Hxcomp.HxComp");
				if (key != null)
				{
					key = key.OpenSubKey("CLSID");
					if (key != null)
					{
						object val = key.GetValue(null);
						if (val != null)				
						{
							string clsid = (string)val;
							key = Registry.ClassesRoot.OpenSubKey("CLSID");
							if (key != null)
							{
								key = key.OpenSubKey(clsid);
								if (key != null)
								{
									key = key.OpenSubKey("LocalServer32");
									if (key != null)
									{
										val = key.GetValue(null);
										if (val != null)
										{
											string path = (string)val;
											_HtmlHelp2CompilerPath = Path.GetDirectoryName(path);
											if (FindHxComp())
											{
												return _HtmlHelp2CompilerPath;
											}
										}
									}
								}
							}
						}
					}
				}

				//still not finding the compiler, give up
				throw new DocumenterException(
					"Unable to find the HTML Help 2 Compiler. Please verify that the Microsoft Visual Studio .NET Help Integration Kit has been installed.");
			}
		}
		#endregion

	}
}
