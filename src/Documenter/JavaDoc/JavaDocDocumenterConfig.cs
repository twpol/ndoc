// JavaDocDocumenterConfig.cs - the JavaDoc documenter config class
// Copyright (C) 2001  Jason Diamond
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
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

using NDoc.Core;

namespace NDoc.Documenter.JavaDoc
{
	/// <summary>The JavaDoc documenter config class.</summary>
	public class JavaDocDocumenterConfig : BaseDocumenterConfig
	{
		/// <summary>Initializes a new instance of the JavaDocDocumenterConfig class.</summary>
		public JavaDocDocumenterConfig() : base("JavaDoc")
		{
			// fix for bug 884121 - OutputDirectory on Linux
			OutputDirectory = string.Format(".{0}doc{0}",Path.DirectorySeparatorChar );
		}

		private string _Title;

		/// <summary>Gets or sets the Title property.</summary>
		[Category("Documentation Main Settings")]
		[Description("The name of the JavaDoc project.")]
		public string Title
		{
			get
			{
				return _Title;
			}

			set
			{
				_Title = value;
				SetDirty();
			}
		}

		private string _OutputDirectory;

		/// <summary>Gets or sets the OutputDirectory property.</summary>
		[Category("Documentation Main Settings")]
		[Description("The output folder.")]
#if !MONO //System.Windows.Forms.Design.FolderNameEditor is not implemented in mono 0.28
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
#endif
		public string OutputDirectory
		{
			get 
			{ 
				return _OutputDirectory; 
			}

			set
			{
				if ( value.IndexOfAny(new char[]{'#','?', ';', ':'}) != -1) 
				{
					throw new FormatException("Output Directory '" + value + 
						"' is not valid because it contains '#','?', ':' or ';' which" +
						" are reserved characters in HTML URLs."); 
				}

				_OutputDirectory = value;

				if (!_OutputDirectory.EndsWith( Path.DirectorySeparatorChar.ToString() ))
				{
					_OutputDirectory += Path.DirectorySeparatorChar;
				}

				SetDirty();
			}
		}
	}
}
