// XmlDocumenter.cs - an XML documenter
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using NDoc.Core;

namespace NDoc.Documenter.Xml
{
	/// <summary>The XmlDocumenter class.</summary>
	public class XmlDocumenter : BaseDocumenter
	{
		/// <summary>Initializes a new instance of the XmlDocumenter class.</summary>
		public XmlDocumenter() : base("XML")
		{
			Clear();
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string MainOutputFile 
		{ 
			get 
			{
				return ((XmlDocumenterConfig)Config).OutputFile;
			} 
		}

		/// <summary>See IDocumenter.</summary>
		public override void Clear()
		{
			Config = new XmlDocumenterConfig();
		}

		/// <summary>See IDocumenter.</summary>
		public override void Build(Project project)
		{
			OnDocBuildingStep(0, "Building XML documentation...");

			XmlDocumenterConfig config = (XmlDocumenterConfig)Config;

			MakeXml(project);

			OnDocBuildingStep(50, "Saving XML documentation...");

			string directoryName = Path.GetDirectoryName(config.OutputFile);

			if (directoryName != null && directoryName.Length > 0)
			{
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
			}

			string buffer = XmlBuffer;
			using (StreamWriter sr = File.CreateText(config.OutputFile))
			{
				sr.Write(buffer);
			}
			//Document.Save(config.OutputFile);

			OnDocBuildingStep(100, "Done.");
		}
	}
}
