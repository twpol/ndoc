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
			OutputDirectory = @".\docs\Msdn\";

			HtmlHelpName = "Documentation";

			Title = "An NDoc Documented Class Library";

			SplitTOCs = false;
			DefaulTOC = string.Empty;

			ShowVisualBasic = true;
			OmitObjectTags = false;
		}


		/// <summary>Gets or sets the OutputDirectory property.</summary>
		[
		Category("Documentation Main Settings"),
		Editor(typeof(FolderNameEditor), typeof(UITypeEditor)),
		Description("The directory in which .html files and the .chm file will be generated.")
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
		Description("The name of the HTML Help project and the Compiled HTML Help file.")
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
		Description("Turning this flag on will generate a separate TOC for each assembly.")
		]
		public bool SplitTOCs
		{
			get { return _SplitTOCs; }

			set 
			{ 
				_SplitTOCs = value; 
				SetDirty();
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

		private bool _OmitObjectTags;

		/// <summary>Gets or sets the OmitObjectTags property.</summary>
		[
		Category("HTML Help Options"),
		Description("Set this to true to not output the <object> tags used by the HTML Help compiler.")
		]
		public bool OmitObjectTags
		{
			get { return _OmitObjectTags; }

			set 
			{ 
				_OmitObjectTags = value; 
				SetDirty();
			}
		}


		string _RootPageTOCName;

		/// <summary>Gets or sets the RootPageTOCName property.</summary>
		[
		Category("Documentation Main Settings"),
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
		Category("Documentation Main Settings"),
		Description("The name of an html file to be included as the root page."
			+ "This root page also becomes the default page.")
		]
		public string RootPageFileName
		{
			get { return _RootPageFileName; }

			set
			{
				_RootPageFileName = value;
				SetDirty();
			}
		}
	}
}
