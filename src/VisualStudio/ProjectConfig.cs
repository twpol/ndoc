using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace NDoc.VisualStudio
{
	/// <summary>
	/// ProjectConfig settings for Visual Studio C# projects.
	/// </summary>
	public class ProjectConfig
	{
		private XPathNavigator _Navigator;

		internal ProjectConfig(XPathNavigator navigator)
		{
			_Navigator = navigator.Clone();
		}

		/// <summary>Gets the name of the configuration.</summary>
		/// <remarks>This is usually "Debug" or "Release".</remarks>
		public string Name
		{
			get
			{
				return (string)_Navigator.Evaluate("string(@Name)");
			}
		}

		/// <summary>Gets the location of the output files (relative to the 
		/// project directory) for this project's configuration.</summary>
		public string OutputPath
		{
			get
			{
				return (string)_Navigator.Evaluate("string(@OutputPath)");
			}
		}

		/// <summary>Gets the name of the file (relative to the project 
		/// directory) into which documentation comments will be 
		/// processed.</summary>
		public string DocumentationFile
		{
			get
			{
				return (string)_Navigator.Evaluate("string(@DocumentationFile)");
			}
		}
	}
}
