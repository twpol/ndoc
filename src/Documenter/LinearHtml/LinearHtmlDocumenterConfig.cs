// LinearHtmlDocumenterConfig.cs - the MsdnHelp documenter config class
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

using NDoc.Documenter.Msdn;


namespace NDoc.Documenter.LinearHtml
{
	/// <summary>The LinearHtmlDocumenterConfig class.</summary>
	public class LinearHtmlDocumenterConfig : BaseDocumenterConfig
	{
		/// <summary>Initializes a new instance of the MsdnHelpConfig class.</summary>
		public LinearHtmlDocumenterConfig() : base("LinearHtml")
		{
			_OutputDirectory = @".\doc\";
			_Title = "An NDoc Documented Class Library";
			_HeaderHtml = string.Empty;
			_FooterHtml = string.Empty;
			_FilesToInclude = string.Empty;
			_IncludeHierarchy = false;
			_SortTOCByNamespace = true;

			_NamespaceExcludeRegexp = string.Empty;
			_TypeIncludeRegexp = string.Empty;
			_MethodParametersInTable = false;
			_IncludeTypeMemberDetails = false;
		}

		bool _IncludeTypeMemberDetails;

		/// <summary>Gets or sets the IncludeTypeMemberDetails property.</summary>
		[
		Category("LinearHtml Style Settings"),
		Description("Whether or not to put type member (methods, fields, properties, ...) details into the "
			+ "document.  For fields and properties this means whether or not to include "
			+ "remarks in the table.  For methods this means whether or not to break out "
			+ "method details (such as parameters) into separate sub-sections."),
		]
		public bool IncludeTypeMemberDetails
		{
			get { return _IncludeTypeMemberDetails; }

			set
			{
				_IncludeTypeMemberDetails = value;
				SetDirty();
			}
		}


		string _OutputDirectory;

		/// <summary>Gets or sets the OutputDirectory property.</summary>
		[
		Category("Documentation Main Settings"),
#if !MONO //System.Windows.Forms.Design.FolderNameEditor is not implemented in mono 0.25
		Editor(typeof(FolderNameEditor), typeof(UITypeEditor)),
#endif
		Description("The directory in which .html file will be generated."),
		]
		public string OutputDirectory
		{
			get { return _OutputDirectory; }

			set
			{
				_OutputDirectory = value;

				if (!_OutputDirectory.EndsWith("\\"))
				{
					_OutputDirectory += "\\";
				}

				SetDirty();
			}
		}

		private bool _MethodParametersInTable;

		/// <summary>Gets or sets the MethodParametersInTable property.</summary>
		[
		Category("LinearHtml Style Settings"),
		Description("Whether or not to put method parameter lists into the "
			+ "same table with the method name."),
		]
		public bool MethodParametersInTable
		{
			get { return _MethodParametersInTable; }

			set
			{
				_MethodParametersInTable = value;
				SetDirty();
			}
		}


		private string _TypeIncludeRegexp;

		/// <summary>Gets or sets the TypeIncludeRegexp property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("A C# regular expression to include types.  If this is specified,"
			+ " only types which match will be included in the output."),
		]
		public string TypeIncludeRegexp
		{
			get { return _TypeIncludeRegexp; }

			set
			{
				_TypeIncludeRegexp = value;
				SetDirty();
			}
		}

		private string _NamespaceExcludeRegexp;

		/// <summary>Gets or sets the NamespaceExcludeRegexp property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("A C# regular expression to exclude namespaces."),
		]
		public string NamespaceExcludeRegexp
		{
			get { return _NamespaceExcludeRegexp; }

			set
			{
				_NamespaceExcludeRegexp = value;
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

		bool _SortTOCByNamespace;

		/// <summary>Gets or sets the SortTOCByNamespace property.</summary>
		[
		Category("Documentation Main Settings"),
		Description("Sorts the TOC by namespace name. "
			+ "SplitTOCs is disabled when this option is selected.")
		]
		public bool SortTOCByNamespace
		{
			get { return _SortTOCByNamespace; }

			set
			{
				_SortTOCByNamespace = value;
				SetDirty();
			}
		}

		string _HeaderHtml;

		/// <summary>Gets or sets the HeaderHtml property.</summary>
		[
		Category("Documentation Main Settings"),
		Editor(typeof(TextEditor), typeof(UITypeEditor)),
		Description("Raw HTML that is used as a page header instead of the default blue banner. " +
			"\"%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. " +
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
		Category("Documentation Main Settings"),
		Editor(typeof(TextEditor), typeof(UITypeEditor)),
		Description("Raw HTML that is used as a page footer instead of the default footer." +
			"\"%FILE_NAME%\" is dynamically replaced by the name of the file for the current html page. " +
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
		Category("Documentation Main Settings"),
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
}
