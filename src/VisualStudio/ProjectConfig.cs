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
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NDoc3.VisualStudio {
	/// <summary>
	/// ProjectConfig settings for Visual Studio C# projects.
	/// </summary>
	public class ProjectConfig {
		private readonly XPathNavigator _Navigator;
		private readonly ProjectVersion _projectVersion;
		private readonly XNamespace _namespace;
		private readonly XElement _configuration;

		internal ProjectConfig(XElement configuration, ProjectVersion version) {
			_configuration = configuration;
			_projectVersion = version;
			if (_projectVersion == ProjectVersion.VS2005AndAbove)
				_namespace = "http://schemas.microsoft.com/developer/msbuild/2003";
		}

		/// <summary>Gets the name of the configuration.</summary>
		/// <remarks>This is usually "Debug" or "Release".</remarks>
		public string Name {
			get {
				switch (_projectVersion) {
					case ProjectVersion.VS2003:
						return _configuration.Attribute("Name").Value;
					case ProjectVersion.VS2005AndAbove:
						return
							_configuration.Attribute("Condition").Value.Split(new[] {'\''}, StringSplitOptions.RemoveEmptyEntries)[0].Trim(
								new[] {'\'', ' '});
					default:
						throw new ApplicationException("Could not get project configuration name, because of unknown project version");
				}
			}
		}

		/// <summary>Gets the location of the output files (relative to the 
		/// project directory) for this project's configuration.</summary>
		public string OutputPath {
			get {
				switch (_projectVersion) {
					case ProjectVersion.VS2003:
						return _configuration.Attribute("OutputPath").Value;
					case ProjectVersion.VS2005AndAbove:
						return _configuration.Element(_namespace + "OutputPath").Value;
					default:
						throw new ApplicationException("Couldn't find output path, because of unknown project version");
				}
			}
		}

		/// <summary>Gets the name of the file (relative to the project 
		/// directory) into which documentation comments will be 
		/// processed.</summary>
		public string DocumentationFile {
			get {
				switch (_projectVersion) {
					case ProjectVersion.VS2003:
						if (_configuration.Attribute("DocumentationFile") == null) return null;
						return _configuration.Attribute("DocumentationFile").Value;
					case ProjectVersion.VS2005AndAbove:
						if (_configuration.Element(_namespace + "DocumentationFile") == null) return null;
						return _configuration.Element(_namespace + "DocumentationFile").Value;
					default:
						throw new ApplicationException("Couldn't find documentation file tag, because of unknown project version");
				}
			}
		}
	}
}
