#region Copyright © 2002 Jean-Claude Manoli [jc@manoli.net]
/*
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the author(s) be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 *   1. The origin of this software must not be misrepresented; you must not
 *      claim that you wrote the original software. If you use this software
 *      in a product, an acknowledgment in the product documentation would be
 *      appreciated but is not required.
 * 
 *   2. Altered source versions must be plainly marked as such, and must not
 *      be misrepresented as being the original software.
 * 
 *   3. This notice may not be removed or altered from any source distribution.
 */ 
#endregion

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
        private ProjectVersion _ProjectVersion;
        private XmlNamespaceManager _ProjectNamespaceManager;

		internal ProjectConfig(XPathNavigator navigator, ProjectVersion version)
		{
			_Navigator = navigator.Clone();
            _ProjectVersion = version;
            if (_ProjectVersion == ProjectVersion.VS2005AndAbove)
            {
                _ProjectNamespaceManager = new XmlNamespaceManager(_Navigator.NameTable);
                _ProjectNamespaceManager.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
            }
		}

		/// <summary>Gets the name of the configuration.</summary>
		/// <remarks>This is usually "Debug" or "Release".</remarks>
		public string Name
		{
			get
			{
                //TODO Return the right name
				return (string)_Navigator.Evaluate("string(@Name)");
			}
		}

		/// <summary>Gets the location of the output files (relative to the 
		/// project directory) for this project's configuration.</summary>
		public string OutputPath
		{
			get
			{
                if (_ProjectVersion == ProjectVersion.VS2003)
                    return (string)_Navigator.Evaluate("string(@OutputPath)");
                else if (_ProjectVersion == ProjectVersion.VS2005AndAbove)
                    return (string)_Navigator.Evaluate("string(//ns:OutputPath)", _ProjectNamespaceManager);
                else
                    throw new ApplicationException("Couldn't find output path");
			}
		}

		/// <summary>Gets the name of the file (relative to the project 
		/// directory) into which documentation comments will be 
		/// processed.</summary>
		public string DocumentationFile
		{
			get
			{
                if (_ProjectVersion == ProjectVersion.VS2003)
                    return (string)_Navigator.Evaluate("string(@DocumentationFile)");
                else if (_ProjectVersion == ProjectVersion.VS2005AndAbove)
                    return (string)_Navigator.Evaluate("string(//ns:DocumentationFile)", _ProjectNamespaceManager);
                else
                    throw new ApplicationException("Couldn't documentation file tag");
			}
		}
	}
}
