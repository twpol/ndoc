using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;
using Microsoft.Win32;
using NDoc.Core;

using NDoc.Documenter.Msdn;

namespace NDoc.Documenter.HtmlHelp2
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
	/// Config setting for the CHM to HxS converter/compiler
	/// </summary>
	public class HtmlHelp2Config : MsdnDocumenterConfig
	{

		/// <summary>Initializes a new instance of the MsdnHelpConfig class.</summary>
		public HtmlHelp2Config() : base( "HtmlHelp2" )
		{
		}

		CharacterSet _CharacterSet = CharacterSet.Ascii;
		/// <summary>
		/// Gets or sets the character set that will be used when compiling the help file.
		/// Defaults to Ascii.
		/// </summary>
		[
		Category("Html Help v2.0 Settings"),
		Description("Gets or sets the character set that will be used when compiling the help file")
		]
		public CharacterSet CharacterSet
		{
			get{ return _CharacterSet; }
			set
			{
				_CharacterSet = value;
				SetDirty();
			}
		}


		private string _HtmlHelp2CompilerPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
			"Microsoft Help 2.0 SDK");

		private bool FindHxComp()
		{
			return File.Exists(Path.Combine(_HtmlHelp2CompilerPath, "hxcomp.exe"));
		}

		internal string HtmlHelp2CompilerPath
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

		
		short _LangID = 1033;

		/// <summary>The language ID of the locale used by the compiled helpfile</summary>
		[
		Category("Html Help v2.0 Settings"),
		Description("The ID of the language the help file is in.")
		]
		public short LangID
		{
			get { return _LangID; }

			set
			{
				_LangID = value;
				SetDirty();
			}
		}	

		bool _DeleteCHM = false;

		/// <summary>Flag that indicates whether to keep the CHM file after successful conversion</summary>
		[
		Category("Html Help v2.0 Settings"),
		Description("If true the CHM file will be deleted after the HxS file is created")
		]
		public bool DeleteCHM
		{
			get { return _DeleteCHM; }

			set
			{
				_DeleteCHM = value;
				SetDirty();
			}
		}

		#region Not yet implemented features
		
//		bool _BuildSeperateIndexFile = true;
//
//		/// <summary>Gets or sets the property that causes a seperate index file to be generated.</summary>
//		[
//		Category("Html Help v2.0 Settings"),
//		Description("If true, create a seperate index file (HxI), otherwise the index is compiled into the HxS file.")
//		]
//		public bool BuildSeperateIndexFile
//		{
//			get { return _BuildSeperateIndexFile; }
//
//			set
//			{
//				_BuildSeperateIndexFile = value;
//				SetDirty();
//			}
//		}
//
//
//		string _Version = "1.0.0.0";
//
//		/// <summary>Gets or sets the base directory used to resolve directory and assembly references.</summary>
//		[
//		Category("Html Help v2.0 Settings"),
//		Description("The version number for the help file (#.#.#.#)")
//		]
//		public string Version
//		{
//			get { return _Version; }
//
//			set
//			{
//				_Version = value;
//				SetDirty();
//			}
//		}
//		bool _RegisterTitle = false;
//
//		[
//		Category("Html Help v2.0 Settings"),
//		Description("Should the compiled Html 2 title be registered after it is compiled. (If true both a TitleID and Namespace are required)")
//		]
//		public bool RegisterTitle
//		{
//			get { return _RegisterTitle; }
//
//			set
//			{
//				_RegisterTitle = value;
//				SetDirty();
//			}
//		}
//
//		string _TitleID = String.Empty;
//
//		[
//		Category("Html Help v2.0 Settings"),
//		Description("The Html Help 2 registry title ID for this help file (avoid spaces). Only used if RegisterTutle is True.")
//		]
//		public string TitleID
//		{
//			get { return _TitleID; }
//
//			set
//			{
//				_TitleID = value;
//				SetDirty();
//			}
//		}	
//
//		string _HtmlHelpNamespace = String.Empty;
//
//		[
//		Category("Html Help v2.0 Settings"),
//		Description("The Html Help 2 registry namespace (avoid spaces). Only used if RegisterTitle is True.")
//		]
//		public string HtmlHelpNamespace
//		{
//			get { return _HtmlHelpNamespace; }
//
//			set
//			{
//				_HtmlHelpNamespace = value;
//				SetDirty();
//			}
//		}		
//
//
//		string _CollectionHxCPath = String.Empty;
//
//		[
//		Category("Html Help v2.0 Settings"),
//		Description("The path to an HxC collection to which the help file will be added"),
//		Editor(typeof(FileNameEditor), typeof(UITypeEditor))
//		]
//		public string CollectionHxCPath
//		{
//			get { return _CollectionHxCPath; }
//
//			set
//			{
//				_CollectionHxCPath = value;
//				SetDirty();
//			}
//		}		
//
//		string _PluginNamespace = String.Empty;
//
//		[
//		Category("Html Help v2.0 Settings"),
//		Description("The namespace of a help collection that this collection will be referenced from (MS.VSCC is Msdn v7)")
//		]
//		public string PluginNamespace
//		{
//			get { return _PluginNamespace; }
//
//			set
//			{
//				_PluginNamespace = value;
//				SetDirty();
//			}
//		}			
		#endregion
	
	}
}
