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
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;
using Microsoft.Win32;
using NDoc.Core;

namespace NDoc.Documenter.Msdn
{
	/// <summary>The MsdnDocumenterConfig class.</summary>
	public class MsdnDocumenterConfig : BaseDocumenterConfig
	{
		string outputDirectory;
		string htmlHelpName;
		string htmlHelpCompilerFilename;
		bool includeFavorites;

		/// <summary>Initializes a new instance of the MsdnHelpConfig class.</summary>
		public MsdnDocumenterConfig() : base("MSDN")
		{
			outputDirectory = @".\doc\";

			htmlHelpName = "Documentation";

			_Title = "An NDoc Documented Class Library";

			_SortTOCByNamespace = true;
			_SplitTOCs = false;
			_DefaultTOC = string.Empty;

			_IncludeHierarchy = false;
			_ShowVisualBasic = false;
			_OutputTarget = OutputType.HtmlHelpAndWeb;

			_RootPageContainsNamespaces = false;

			_HeaderHtml = string.Empty;
			_FooterHtml = string.Empty;
			_FilesToInclude = string.Empty;
		}


		/// <summary>Gets or sets the OutputDirectory property.</summary>
		[
		Category("Documentation Main Settings"),
		Editor(typeof(FolderNameEditor), typeof(UITypeEditor)),
		Description("The directory in which .html files and the .chm file will be generated."),
		]
		public string OutputDirectory
		{
			get { return outputDirectory; }

			set
			{
				outputDirectory = value;

				if (!outputDirectory.EndsWith("\\"))
				{
					outputDirectory += "\\";
				}

				SetDirty();
			}
		}

		/// <summary>Gets or sets the HtmlHelpName property.</summary>
		/// <remarks>The HTML Help project file and the compiled HTML Help file
		/// use this property plus the appropriate extension as names.</remarks>
		[
		Category("Documentation Main Settings"),
		Description("The name of the HTML Help project and the Compiled HTML Help file."),
		]
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

		/// <summary>The path to the Html Help Compiler.</summary>
		internal string HtmlHelpCompilerFilename
		{
			get
			{
				if ((htmlHelpCompilerFilename != null) 
					&&(File.Exists(htmlHelpCompilerFilename)))
				{
					return htmlHelpCompilerFilename;
				}

				//try the default Html Help Workshop installation directory
				htmlHelpCompilerFilename = 	Path.Combine(
					Environment.GetFolderPath(
						Environment.SpecialFolder.ProgramFiles),
					@"HTML Help Workshop\hhc.exe");
				if (File.Exists(htmlHelpCompilerFilename))
				{
					return htmlHelpCompilerFilename;
				}

				//not in default dir, try to locate it from the registry
				RegistryKey key = Registry.ClassesRoot.OpenSubKey("hhc.file");
				if (key != null)
				{
					key = key.OpenSubKey("DefaultIcon");
					if (key != null)
					{
						object val = key.GetValue(null);
						if (val != null)				
						{
							string hhw = (string)val;
							if (hhw.Length > 0)
							{
								hhw = hhw.Split(new Char[] {','})[0];
								hhw = Path.GetDirectoryName(hhw);
								htmlHelpCompilerFilename = Path.Combine(hhw, "hhc.exe");
							}
						}
					}
				}
				if (File.Exists(htmlHelpCompilerFilename))
				{
					return htmlHelpCompilerFilename;
				}

				//still not finding the compiler, give up
				throw new DocumenterException(
					"Unable to find the HTML Help Compiler. Please verify that the HTML Help Workshop has been installed.");
			}
		}

		/// <summary>Gets or sets the IncludeFavorites property.</summary>
		[
		Category("HTML Help Options"),
		Description("Turning this flag on will include a Favorites tab in the HTML Help file.")
		]
		public bool IncludeFavorites
		{
			get { return includeFavorites; }

			set 
			{ 
				includeFavorites = value; 
				SetDirty();
			}
		}

		private string _Title;

		/// <summary>Gets or sets the Title property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("This is the title displayed at the top of every page.")
		]
		public string Title
		{
			get { return _Title; }

			set 
			{ 
				_Title = value; 
				SetDirty();
			}
		}

		private bool _SplitTOCs;

		/// <summary>Gets or sets the SplitTOCs property.</summary>
		[
		Category("HTML Help Options"),
		Description("Turning this flag on will generate a separate TOC for each assembly. "
			+ "It cannot be set if SortTOCByNamespace is set or RootPageFileName is specified.")
		]
		public bool SplitTOCs
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
		[
		Category("HTML Help Options"),
		Description("When SplitTOCs is true, this represents the default TOC to use.")
		]
		public string DefaulTOC
		{
			get { return _DefaultTOC; }

			set 
			{ 
				_DefaultTOC = value; 
				SetDirty();
			}
		}

		private bool _IncludeHierarchy;

		/// <summary>Gets or sets the IncludeHierarchy property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("To include a class hiararchy page for each namespace. Don't turn it on if your project has a namespace with many types, as NDoc will become very slow and might crash.")
		]
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
		[
		Category("Documentation Main Settings"),
		Description("Show Visual Basic syntax for types and members.")
		]
		public bool ShowVisualBasic
		{
			get { return _ShowVisualBasic; }

			set 
			{ 
				_ShowVisualBasic = value; 
				SetDirty();
			}
		}

		string _RootPageTOCName;

		/// <summary>Gets or sets the RootPageTOCName property.</summary>
		[
		Category("HTML Help Options"),
		Description("The name for the Table of Contents entry corresponding "
			+ " to the root page."
			+ " If this is not specified and RootPageFileName is, then"
			+ " the TOC entry will be 'Overview'.")
		]
		public string RootPageTOCName
		{
			get { return _RootPageTOCName; }

			set
			{
				_RootPageTOCName = value;
				SetDirty();
			}
		}

		string _RootPageFileName;

		/// <summary>Gets or sets the RootPageFileName property.</summary>
		[
		Category("HTML Help Options"),
		Description("The name of an html file to be included as the root home page. "
			+ "SplitTOCs is disabled when this property is set."),
		Editor(typeof(FileNameEditor), typeof(UITypeEditor))
		]
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
		[
		Category("HTML Help Options"),
		Description("If true, the Root Page will be made the container"
			+ " of the namespaces in the TOC."
			+ " If false, the Root Page will be made a peer of"
			+ " the namespaces in the TOC.")
		]
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
		[
		Category("HTML Help Options"),
		Description("Sorts the TOC by namespace name. "
			+ "SplitTOCs is disabled when this option is selected.")
		]
		public bool SortTOCByNamespace
		{
			get { return _SortTOCByNamespace; }

			set
			{
				_SortTOCByNamespace = value;
				_SplitTOCs = _SplitTOCs && !value;
				SetDirty();
			}
		}

		private OutputType _OutputTarget;

		/// <summary>Gets or sets the OutputTarget property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("Sets this property to .")
		]
		public OutputType OutputTarget
		{
			get { return _OutputTarget; }

			set 
			{ 
				_OutputTarget = value; 
				SetDirty();
			}
		}

		string _HeaderHtml;

		/// <summary>Gets or sets the HeaderHtml property.</summary>
		[
		Category("HTML Help Options"),
		Description("Raw HTML that is used as a page header instead of the default blue banner. " +
			"\"%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.")
		]
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
		[
		Category("HTML Help Options"),
		Description("Raw HTML that is used as a page footer instead of the default footer." +
			"\"%ASSEMBLY_NAME%\" is dynamically replaced by the name of the assembly for the current page. " +
			"\"%ASSEMBLY_VERSION%\" is dynamically replaced by the version of the assembly for the current page. " +
			"\"%TOPIC_TITLE%\" is dynamically replaced by the title of the current page.")
		]
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
		[
		Category("HTML Help Options"),
		Description("Specifies external files that must be included in the compiled CHM file. Multiple files must be separated by a pipe ('|').")
		]
		public string FilesToInclude
		{
			get { return _FilesToInclude; }

			set
			{
				_FilesToInclude = value;
				SetDirty();
			}
 		}

	}

	/// <summary>
	/// Defines the output types for this documenter.
	/// </summary>
	[Flags]
	public enum OutputType
	{
		/// <summary>Output only an HTML Help file (.chm).</summary>
		HtmlHelp = 1,

		/// <summary>Output only Web pages.</summary>
		Web = 2,

		/// <summary>Output both HTML Help and Web.</summary>
		HtmlHelpAndWeb = HtmlHelp | Web
	}
}
