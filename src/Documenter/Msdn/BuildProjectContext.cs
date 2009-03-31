using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using NDoc3.Core;
using NDoc3.Support;

namespace NDoc3.Documenter.Msdn
{
	internal class BuildProjectContext : IDisposable
	{
		private XmlDocument _xmlDocumentation;

		public XPathNavigator GetXPathNavigator()
		{
			XPathNavigator xpathdoc = new XPathDocument(new XmlNodeReader(_xmlDocumentation), XmlSpace.Preserve).CreateNavigator();
			return xpathdoc;
		}

		public void SetProjectXml(XmlDocument projectXml)
		{
			XmlNamespaceManager nsmgr;
			nsmgr = new XmlNamespaceManager(projectXml.NameTable);
			nsmgr.AddNamespace("ndoc", "urn:ndoc-schema");
			nsmgr.AddNamespace("ns", "urn:ndoc-schema");

			XmlNodeList typeNodes = projectXml.SelectNodes("/ndoc:ndoc/ndoc:assembly/ndoc:module/ndoc:namespace/*[name()!='documentation']", nsmgr);
			if (typeNodes.Count == 0) {
				throw new DocumenterException("There are no documentable types in this project.\n\nAny types that exist in the assemblies you are documenting have been excluded by the current visibility settings.\nFor example, you are attempting to document an internal class, but the 'DocumentInternals' visibility setting is set to False.\n\nNote: C# defaults to 'internal' if no accessibilty is specified, which is often the case for Console apps created in VS.NET...");
			}

			_nameResolver = new NameResolver(projectXml);
			_xmlnsManager = nsmgr;
			_xmlDocumentation = projectXml;
		}

		private XmlNamespaceManager _xmlnsManager;
		public StyleSheetCollection stylesheets;
		//			public ArrayList documentedNamespaces = new ArrayList();
		private readonly Workspace workspace;
		public NameResolver _nameResolver;
		private readonly Encoding _currentFileEncoding;
		public HtmlHelp htmlHelp;
		private readonly DirectoryInfo _workingDirectory;

		public BuildProjectContext(CultureInfo ci, DirectoryInfo targetDirectory, bool cleanIntermediates)
		{
			ArgUtils.AssertNotNull(ci, "ci");
			ArgUtils.AssertNotNull(targetDirectory, "targetDirectory");

			_currentFileEncoding = Encoding.GetEncoding(ci.TextInfo.ANSICodePage);
			workspace = new MsdnWorkspace(targetDirectory.FullName, cleanIntermediates);
			_workingDirectory = new DirectoryInfo(workspace.WorkingDirectory);
		}

		public BuildProjectContext(BuildProjectContext other)
		{
			this._xmlDocumentation = other._xmlDocumentation;
			this._xmlnsManager = other._xmlnsManager;
			this.stylesheets = other.stylesheets;
			this.workspace = other.workspace;
			this._nameResolver = other._nameResolver;
			this._currentFileEncoding = other._currentFileEncoding;
			this.htmlHelp = other.htmlHelp;
			this._workingDirectory = other._workingDirectory;
		}

		public DirectoryInfo WorkingDirectory
		{
			get { return _workingDirectory; }
		}

		public Encoding CurrentFileEncoding
		{
			get { return _currentFileEncoding; }
		}

		public FileInfo HtmlHelpContentFilePath
		{
			get { return new FileInfo(htmlHelp.GetPathToContentsFile()); }
		}

		/// <summary>
		/// Selects nodes using the default <see cref="_xmlnsManager"/>
		/// </summary>
		/// <param name="xpath">an xpath expression to select nodes</param>
		/// <returns>the result of <see cref="XmlNode.SelectNodes(string,System.Xml.XmlNamespaceManager)"/></returns>
		public XmlNodeList SelectNodes(string xpath)
		{
			return SelectNodes(_xmlDocumentation, xpath);
		}

		/// <summary>
		/// Selects nodes using the default <see cref="_xmlnsManager"/>
		/// </summary>
		/// <param name="contextNode">the node to evaluate <paramref name="xpath" />against</param>
		/// <param name="xpath">an xpath expression to select nodes</param>
		/// <returns>the result of <see cref="XmlNode.SelectNodes(string,System.Xml.XmlNamespaceManager)"/></returns>
		public XmlNodeList SelectNodes(XmlNode contextNode, string xpath)
		{
			return contextNode.SelectNodes(xpath, _xmlnsManager);
		}

		/// <summary>
		/// Selects single node using the default <see cref="_xmlnsManager"/>
		/// </summary>
		/// <param name="xpath">an xpath expression to select nodes</param>
		/// <returns>the result of <see cref="XmlNode.SelectSingleNode(string,System.Xml.XmlNamespaceManager)"/></returns>
		public XmlNode SelectSingleNode(string xpath)
		{
			return SelectSingleNode(_xmlDocumentation, xpath);
		}

		/// <summary>
		/// Selects single node using the default <see cref="_xmlnsManager"/>
		/// </summary>
		/// <param name="contextNode">the node to evaluate <paramref name="xpath" />against</param>
		/// <param name="xpath">an xpath expression to select nodes</param>
		/// <returns>the result of <see cref="XmlNode.SelectNodes(string,System.Xml.XmlNamespaceManager)"/></returns>
		public XmlNode SelectSingleNode(XmlNode contextNode, string xpath)
		{
			return contextNode.SelectSingleNode(xpath, _xmlnsManager);
		}

		/// <summary>
		/// Saves files mathing the specified filter from the temporary directory to the target directory
		/// </summary>
		/// <param name="pattern">File filter to search for</param>
		public void SaveOutputs(string pattern)
		{
			this.workspace.SaveOutputs(pattern);
		}

		/// <summary>
		/// Disposes the context, cleaning up the temporary build directory.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
			workspace.Dispose();
		}

		/// <summary>
		/// Initializes the context, creating the temporary build directory etc.
		/// </summary>
		public void Initialize()
		{
			workspace.Clean();
			workspace.Prepare();
		}

		/// <summary>
		/// Copy files from an arbitrary directory to the temporary build directory
		/// </summary>
		/// <param name="resourceDirectory"></param>
		public void CopyToWorkingDirectory(DirectoryInfo resourceDirectory)
		{
			workspace.ImportContentDirectory(resourceDirectory);
		}
	}
}