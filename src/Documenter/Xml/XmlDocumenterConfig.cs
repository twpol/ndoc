// XmlDocumenterConfig.cs - XML documenter config class
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
using System.Windows.Forms.Design;

using NDoc.Core;

namespace NDoc.Documenter.Xml
{
	/// <summary>The XmlDocumenter config class.</summary>
	public class XmlDocumenterConfig : BaseDocumenterConfig
	{
		/// <summary>Initializes a new instance of the XmlDocumenterConfig class.</summary>
		public XmlDocumenterConfig() : base("XML")
		{
			OutputFile = @".\docs\doc.xml";
		}

		string _OutputFile;

		/// <summary>Gets or sets the OutputFile property.</summary>
		[
			Category("Output"),
			Description("The path to the XML file to create which will be the combined /doc output and reflection information."),
			Editor(typeof(FileNameEditor), typeof(UITypeEditor))
		]
		public string OutputFile
		{
			get { return _OutputFile; }
			set { _OutputFile = value; }
		}
	}
}
