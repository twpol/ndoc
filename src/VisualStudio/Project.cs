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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NDoc3.VisualStudio {
	internal enum ProjectVersion {
		VS2003,
		VS2005AndAbove
	}

	/// <summary>
	/// Represents a Visual Studio c# project file.
	/// </summary>
	public class Project {
		internal Project(Solution solution, Guid id, string name) {
			_Solution = solution;
			_ID = id;
			_Name = name;
		}

		private readonly Solution _Solution;

		/// <summary>Gets the solution that contains this project.</summary>
		public Solution Solution {
			get { return _Solution; }
		}

		private string _RelativePath;

		/// <summary>Gets or sets the relative path (from the solution 
		/// directory) to the project directory.</summary>
		public string RelativePath {
			get { return _RelativePath; }
			set { _RelativePath = value; }
		}

		private readonly Guid _ID;

		/// <summary>Gets the GUID that identifies the project.</summary>
		public Guid ID {
			get { return _ID; }
		}

		private readonly string _Name;

		/// <summary>Gets the name of the project.</summary>
		public string Name {
			get { return _Name; }
		}

		private XDocument _projectDocument;
		private XPathNavigator _ProjectNavigator;
		private XNamespace _namespace;
		private ProjectVersion _projectVersion;

		/// <summary>Reads the project file from the specified path.</summary>
		/// <param name="path">The path to the project file.</param>
		public void Read(string path) {
			_projectDocument = XDocument.Load(path);
			_namespace = "http://schemas.microsoft.com/developer/msbuild/2003";
		}

		/// <summary>Gets a string that represents the type of project.</summary>
		/// <value>"Visual C++" or "C# Local"</value>
		public string ProjectType {
			get {
            string projectType = "";
				
				// Check if it is a Visual Studio 2003 project file
				XElement vsProject = _projectDocument.Element("VisualStudioProject");
				if(vsProject != null) {
					_projectVersion = ProjectVersion.VS2003;
					if (vsProject.Attribute("ProjectType") != null && vsProject.Attribute("ProjectType").Value == "Visual C++")
						projectType = "Visual C++";
					XElement csharp = vsProject.Element("CSHARP");
					if (csharp != null && csharp.Attribute("ProjectType") != null) {
						if (csharp.Attribute("ProjectType").Value == "Local")
							projectType = "C# Local";
						else if (csharp.Attribute("ProjectType").Value == "Web")
							projectType = "C# Web";
					}
				}
				if (projectType != "") return projectType;
				
				// Check if it is a Visual Studio 2005 or above project file
				bool propertyGroupExists = _projectDocument.Descendants(_namespace + "PropertyGroup").Any();
				if(!propertyGroupExists) throw new ApplicationException("Unknown project file");
				if (_projectDocument.Element(_namespace + "Project").Element(_namespace + "PropertyGroup").Descendants(_namespace + "ProjectType").Any()) {
					_projectVersion = ProjectVersion.VS2005AndAbove;
					string type =
						_projectDocument.Element(_namespace + "Project").Element(_namespace + "PropertyGroup").Element(_namespace +
						                                                                                               "ProjectType").
							Value;
					if (type == "Local") projectType = "C# Local";
					if (type == "Web") projectType = "C# Web";
				} else {
					_projectVersion = ProjectVersion.VS2005AndAbove;
					projectType = "C# Local";
				}

				if(projectType == "")
					throw new ApplicationException("Unknown project file");

				return projectType;
			}
		}

		/// <summary>Gets the name of the assembly this project generates.</summary>
		public string AssemblyName {
			get {
				switch (_projectVersion) {
					case ProjectVersion.VS2003:
						return
							_projectDocument.Element("VisualStudioProject").Element("CSHARP").Element("Build").Element("Settings").Attribute(
								"AssemblyName").Value;
					case ProjectVersion.VS2005AndAbove:
						return
							_projectDocument.Element(_namespace + "Project").Element(
							_namespace + "PropertyGroup").Element(_namespace + "AssemblyName").Value;
					default:
						throw new ApplicationException("Couldn't find assembly name");
				}
			}
		}

		/// <summary>Gets the output type of the project.</summary>
		/// <value>"Library", "Exe", or "WinExe"</value>
		public string OutputType {
			get {
				switch (_projectVersion) {
					case ProjectVersion.VS2003:
						return
							_projectDocument.Element("VisualStudioProject").Element("CSHARP").Element("Build").Element("Settings").Attribute(
								"OutputType").Value;
					case ProjectVersion.VS2005AndAbove:
						return
							_projectDocument.Element(_namespace + "Project").Element(
							_namespace + "PropertyGroup").Element(_namespace + "OutputType").Value;
					default:
						throw new ApplicationException("Could not find outputtype");
				}
			}
		}

		/// <summary>Gets the filename of the generated assembly.</summary>
		public string OutputFile {
			get {
				string extension = "";

				switch (OutputType) {
					case "Library":
						extension = ".dll";
						break;
					case "Exe":
						extension = ".exe";
						break;
					case "WinExe":
						extension = ".exe";
						break;
				}

				return AssemblyName + extension;
			}
		}

		/// <summary>Gets the default namespace for the project.</summary>
		public string RootNamespace {
			get {
				switch (_projectVersion) {
					case ProjectVersion.VS2003:
						return
							_projectDocument.Element("VisualStudioProject").Element("CSHARP").Element("Build").Element("Settings").Attribute(
								"RootNamespace").Value;
					case ProjectVersion.VS2005AndAbove:
						return
							_projectDocument.Element(_namespace + "Project").Element(
							_namespace + "PropertyGroup").Element(_namespace + "RootNamespace").Value;
					default:
						throw new ApplicationException("Couldn't find rootnamespace tag");
				}
			}
		}

		/// <summary>Gets the configuration with the specified name.</summary>
		/// <param name="configName">A valid configuration name, usually "Debug" or "Release".</param>
		/// <returns>A ProjectConfig object.</returns>
		public ProjectConfig GetConfiguration(string configName) {
			XElement configuration = null;
			
			// Find configuration with specified name
			try {
				if (_projectVersion == ProjectVersion.VS2003) {
					configuration =
						_projectDocument.Element("VisualStudioProject").Element("CSHARP").Element("Build").Element("Settings").Elements(
							"Config").Where(el => el.Attribute("Name") != null && el.Attribute("Name").Value == configName).Single();
				} else if (_projectVersion == ProjectVersion.VS2005AndAbove) {
					configuration =
						_projectDocument.Element(_namespace + "Project").Elements(_namespace + "PropertyGroup").Single(
							el => el.Attribute("Condition") != null && el.Attribute("Condition").Value.Contains(configName));
				}
			} catch (Exception e) {
				throw new ApplicationException("Error occured while parsing Visual Studio project file.", e);
			}

			if (configuration == null) throw new ApplicationException("Error occured while parsing Visual Studio project file.");

			return new ProjectConfig(configuration, _projectVersion);
		}

		/// <summary>Gets the relative path (from the solution directory) to the
		/// assembly this project generates.</summary>
		/// <param name="configName">A valid configuration name, usually "Debug" or "Release".</param>
		public string GetRelativeOutputPathForConfiguration(string configName) {
			return Path.Combine(
				Path.Combine(RelativePath, GetConfiguration(configName).OutputPath),
				OutputFile);
		}

		/// <summary>Gets the relative path (from the solution directory) to the
		/// XML documentation this project generates.</summary>
		/// <param name="configName">A valid configuration name, usually "Debug" or "Release".</param>
		public string GetRelativePathToDocumentationFile(string configName) {
			string path = null;

			string documentationFile = GetConfiguration(configName).DocumentationFile;

			if (!String.IsNullOrEmpty(documentationFile)) {
				path = Path.Combine(RelativePath, documentationFile);
			}

			return path;
		}

	}
}
