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

		/// <summary>See IDocumenter.</summary>
		public override void View()
		{
			if (File.Exists(((XmlDocumenterConfig)Config).OutputFile))
			{
				Process.Start(((XmlDocumenterConfig)Config).OutputFile);
			}
			else
			{
				// maybe throw a custom exception here so documenter DLLs don't have to reference WinForms.
				//        MessageBox("Filename {0} doesn't exist.  Maybe try selecting the Build button first then try the View button again.", xmlFilename);
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

			MakeXml(project.AssemblySlashDocs, project.NamespaceSummaries);

			OnDocBuildingStep(50, "Saving XML documentation...");

			string directoryName = Path.GetDirectoryName(config.OutputFile);

			if (directoryName != null && directoryName.Length > 0)
			{
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
			}

			Document.Save(config.OutputFile);

			OnDocBuildingStep(100, "Done.");
		}
	}
}
