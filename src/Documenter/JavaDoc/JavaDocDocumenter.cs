// JavaDocDocumenter.cs - a JavaDoc-like documenter
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
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Reflection;
using System.Collections;

using NDoc.Core;

namespace NDoc.Documenter.JavaDoc
{
	/// <summary>The JavaDoc documenter.</summary>
	public class JavaDocDocumenter : BaseDocumenter
	{
		/// <summary>Initializes a new instance of the JavaDocDocumenter class.</summary>
		public JavaDocDocumenter() : base("JavaDoc")
		{
			Config = new JavaDocDocumenterConfig();
		}

		string _ResourceDirectory;

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string MainOutputFile 
		{ 
			get 
			{
				return Path.Combine(MyConfig.OutputDirectory, 
					"overview-summary.html");
			} 
		}

		string tempFileName;
		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Build(Project project)
		{
			if (!Directory.Exists(MyConfig.OutputDirectory))
			{
				Directory.CreateDirectory(MyConfig.OutputDirectory);
			}

// Define this when you want to edit the stylesheets
// without having to shutdown the application to rebuild.
#if NO_RESOURCES
			_ResourceDirectory = Path.GetFullPath(Path.Combine(
				System.Windows.Forms.Application.StartupPath, @"..\..\..\Documenter\JavaDoc\"));
#else
				_ResourceDirectory = Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData) +
					"\\NDoc\\JavaDoc\\";

				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc.Documenter.JavaDoc.css",
					_ResourceDirectory + "css\\");

				EmbeddedResources.WriteEmbeddedResources(
					this.GetType().Module.Assembly,
					"NDoc.Documenter.JavaDoc.xslt",
					_ResourceDirectory + "xslt\\");
#endif

			string outcss = Path.Combine(MyConfig.OutputDirectory, "JavaDoc.css");
			File.Copy(Path.Combine(_ResourceDirectory, @"css\JavaDoc.css"), outcss, true);
			File.SetAttributes(outcss, FileAttributes.Archive);

			try
			{
				tempFileName = MakeXmlFile(project);

				WriteOverviewSummary();
				WriteNamespaceSummaries();
			}
			finally
			{
				if (File.Exists(tempFileName)) File.Delete(tempFileName);
			}
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Clear()
		{
		}

		private JavaDocDocumenterConfig MyConfig
		{
			get { return (JavaDocDocumenterConfig)Config; }
		}

		private Hashtable cachedTransforms = new Hashtable();
		/// <summary>
		/// Gets the cached transform file, based upon the specified name.  If the file does not 
		/// exist, it is created and then cached.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		private XslTransform GetTransformFile(string fileName)
		{
			XslTransform transform = (XslTransform)cachedTransforms[fileName];
			if(transform == null)
			{
				transform = new XslTransform();
				transform.Load(Path.Combine(_ResourceDirectory, @"xslt\" + fileName));
				cachedTransforms.Add(fileName, transform);
			}
			return transform;
		}

		private void TransformAndWriteResult(
			string transformFilename,
			XsltArgumentList args,
			string resultDirectory,
			string resultFilename)
		{
#if DEBUG
			int start = Environment.TickCount;
#endif
			XslTransform transform = new XslTransform();
			transform.Load(Path.Combine(_ResourceDirectory, @"xslt\" + transformFilename));

			if (args == null)
			{
				args = new XsltArgumentList();
			}

			string pathToRoot = "";

			if (resultDirectory != null)
			{
				string[] directories = resultDirectory.Split('\\');
				int count = directories.Length;

				while (count-- > 0)
				{
					pathToRoot += "../";
				}
			}

			args.AddParam("global-path-to-root", String.Empty, pathToRoot);

			if (resultDirectory != null)
			{
				resultFilename = Path.Combine(resultDirectory, resultFilename);
			}

			string resultPath = Path.Combine(MyConfig.OutputDirectory, resultFilename);
			string resultPathDirectory = Path.GetDirectoryName(resultPath);

			if (!Directory.Exists(resultPathDirectory))
			{
				Directory.CreateDirectory(resultPathDirectory);
			}

			TextWriter writer = new StreamWriter(resultPath);
			XPathDocument doc = GetCachedXPathDocument();
			transform.Transform(doc, args, writer);

			writer.Close();

#if DEBUG
			Trace.WriteLine("Making " + transformFilename + " Html: " + ((Environment.TickCount - start)).ToString() + " ms.");
#endif
		}

		private XPathDocument cachedXPathDocument = null;

		/// <summary>
		/// Gets the XPathDocument, but caches it and returns the cached value.
		/// </summary>
		/// <remarks>
		/// <c>GetXPathDocument</c> can be very slow for large Assemblies, so this is 
		/// designed to speed things up.  As long as the XPathDocument does not ever get 
		/// changed internally by other transforms, this should just speed up the performance 
		/// of the application.
		/// </remarks>
		/// <returns></returns>
		protected XPathDocument GetCachedXPathDocument()
		{
			if(cachedXPathDocument == null)
			{
				Stream tempFile=null;
				try
				{
					tempFile=File.Open(tempFileName,FileMode.Open,FileAccess.Read);
					cachedXPathDocument = new XPathDocument(tempFile);
				}
				finally
				{
					if (tempFile!=null) tempFile.Close();
				}
			}
			return cachedXPathDocument;
		}

		private void WriteOverviewSummary()
		{
			XsltArgumentList args = new XsltArgumentList();
			string title = MyConfig.Title;
			if (title == null) title = string.Empty;
			args.AddParam("global-title", String.Empty, title);

			TransformAndWriteResult(
				"overview-summary.xslt",
				args,
				null,
				"overview-summary.html");
		}

		private void WriteNamespaceSummaries()
		{
			XmlDocument doc = new XmlDocument();
			Stream tempFile=null;
			try
			{
				tempFile=File.Open(tempFileName,FileMode.Open,FileAccess.Read);
				doc.Load(tempFile);
			}
			finally
			{
				if (tempFile!=null) tempFile.Close();
			}

			XmlNodeList namespaceNodes = doc.SelectNodes("/ndoc/assembly/module/namespace");

			foreach (XmlElement namespaceElement in namespaceNodes)
			{
				if (namespaceElement.ChildNodes.Count > 0)
				{
					string name = namespaceElement.GetAttribute("name");
					
					WriteNamespaceSummary(name);
					WriteTypes(namespaceElement);
				}
			}
		}

		private void WriteNamespaceSummary(string name)
		{
			XsltArgumentList args = new XsltArgumentList();
			args.AddParam("global-namespace-name", String.Empty, name);

			TransformAndWriteResult(
				"namespace-summary.xslt",
				args,
				name.Replace('.', '\\'),
				"namespace-summary.html");
		}

		private void WriteTypes(XmlElement namespaceElement)
		{
			XmlNodeList typeNodes = namespaceElement.SelectNodes("interface|class|structure");

			foreach (XmlElement typeElement in typeNodes)
			{
				WriteType(namespaceElement, typeElement);
			}
		}

		private void WriteType(XmlElement namespaceElement, XmlElement typeElement)
		{
			string id = typeElement.GetAttribute("id");

			XsltArgumentList args = new XsltArgumentList();
			args.AddParam("global-type-id", String.Empty, id);

			string namespaceName = namespaceElement.GetAttribute("name");
			string name = typeElement.GetAttribute("name");

			TransformAndWriteResult(
				"type.xslt",
				args,
				namespaceName.Replace('.', '\\'),
				name + ".html");
		}
	}
}
