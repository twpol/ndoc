#region Copyright � 2002 Jean-Claude Manoli [jc@manoli.net]
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
using System.DirectoryServices; // to get IIS virtual directory fiel path.

namespace NDoc3.VisualStudio {
	/// <summary>
	/// Represents a Visual Studio solution file.
	/// </summary>
	/// <remarks>
	/// This class is used to read a Visual Studio solution file
	/// </remarks>
	public class Solution {
		/// <summary>
		/// Initializes a new instance of the Solution class.
		/// </summary>
		/// <param name="slnPath">The Visual Studio solution file to parse.</param>
		public Solution(string slnPath) {
			Read(slnPath);
		}

		private string _directory;

		/// <summary>Gets the SolutionDirectory property.</summary>
		/// <remarks>This is the directory that contains the VS.NET
		/// solution file.</remarks>
		public string Directory {
			get { return _directory; }
		}

		private string _name;

		/// <summary>Gets the SolutionName property.</summary>
		/// <remarks>This is the name of the VS.NET solution file
		/// without the .sln extension.</remarks>
		public string Name {
			get { return _name; }
		}


		/// <summary>Reads a .sln file.</summary>
		/// <param name="path">The path to the .sln file.</param>
		private void Read(string path) {
			path = Path.GetFullPath(path);
			_directory = Path.GetDirectoryName(path);
			_name = Path.GetFileNameWithoutExtension(path);

			StreamReader reader;
			using (reader = new StreamReader(path)) {
				string line = reader.ReadLine();
				
				// Read blank lines
				while (line != null && line.Length == 0) {
					line = reader.ReadLine();
				}

				// Check if the first non-blank line indicates that this is  a Microsoft Visual Studio Solution file
				if (line == null || !line.StartsWith("Microsoft Visual Studio Solution File")) {
					throw new ApplicationException("This is not a Microsoft Visual Studio Solution file.");
				}

				// Read the file to find projects, project configurations and solution configurations
				while ((line = reader.ReadLine()) != null) {
					if (line.StartsWith("Project")) {
						// Add the project
						AddProject(line);
					} else if (line.StartsWith("\tGlobalSection(SolutionConfiguration)")
								|| line.StartsWith("\tGlobalSection(SolutionConfigurationPlatforms)")) {
						ReadSolutionConfig(reader);
					} else if (line.StartsWith("\tGlobalSection(ProjectConfiguration)")
								  || line.StartsWith("\tGlobalSection(ProjectConfigurationPlatforms)")) {
						ReadProjectConfig(reader);
					}
				}
			}
		}

		private readonly Hashtable _configurations = new Hashtable();

		/// <summary>
		/// Returns the specified project's configuration name based for 
		/// a specific solution configuration.
		/// </summary>
		/// <param name="solutionConfig">A valid configuration name for the solution.</param>
		/// <param name="projectId">A valid project guid.</param>
		/// <returns>The project configuration name or null.</returns>
		/// <remarks>The null value is returned when the parameters are invalid,
		/// or if the project is not marked to be built under the specified
		/// solution configuration.</remarks>
		public string GetProjectConfigName(string solutionConfig, string projectId) {
			Hashtable pcfg = (Hashtable)_configurations[solutionConfig];
			if (pcfg == null)
				return null;
			return (string)pcfg[projectId];
		}

		/// <summary>
		/// Get the solution's configurations.
		/// </summary>
		/// <returns>A collection of configuration names.</returns>
		public ICollection GetConfigurations() {
			return _configurations.Keys;
		}

		private void ReadSolutionConfig(TextReader reader) {
			string line;
			while ((line = reader.ReadLine()) != null) {
				if (line.StartsWith("\tEndGlobalSection"))
					return;

				int eqpos = line.IndexOf('=');
				string config = line.Substring(eqpos + 2);

				_configurations.Add(config, new Hashtable());
			}
		}

		private void ReadProjectConfig(TextReader reader) {
			const string pattern = @"^\t\t(?<projid>\S+)\.(?<solcfg>.+)\.Build\.\d+ = (?<projcfg>\S+)\|.+";
			Regex regex = new Regex(pattern);
			string line;

			while ((line = reader.ReadLine()) != null) {
				if (line.StartsWith("\tEndGlobalSection"))
					return;

				Match match = regex.Match(line);
				if (match.Success) {
					string projid = match.Groups["projid"].Value;
					string solcfg = match.Groups["solcfg"].Value;
					string projcfg = match.Groups["projcfg"].Value;
					projid = (new Guid(projid)).ToString();

					((Hashtable)_configurations[solcfg]).Add(projid, projcfg);
				}
			}
		}

		private readonly Hashtable _projects = new Hashtable();

		/// <summary>
		/// Adds a project to process
		/// </summary>
		/// <param name="projectLine">The line representing the project definition</param>
		private void AddProject(string projectLine) {
			const string pattern = @"^Project\(""(?<projecttype>.*?)""\) = ""(?<name>.*?)"", ""(?<path>.*?)"", ""(?<id>.*?)""";
			Regex regex = new Regex(pattern);
			Match match = regex.Match(projectLine);

			// Check if the line are in a valid form
			if (match.Success) {
				string projectTypeGUID = match.Groups["projecttype"].Value;
				string name = match.Groups["name"].Value;
				string path = match.Groups["path"].Value;
				string id = match.Groups["id"].Value;

				//we check the GUID as this tells us what type of VS project to process
				//this ensures that it a standard project type, and not an third-party one,
				//which might have a completely differant structure that we could not handle!
				if (projectTypeGUID == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") //C# project
				{
					Project project = new Project(this, new Guid(id), name);
					string absoluteProjectPath = GetAbsoluteProjectPath(path);

					if (!String.IsNullOrEmpty(absoluteProjectPath)) {
						project.Read(absoluteProjectPath);
						project.RelativePath = Path.GetDirectoryName(absoluteProjectPath);

						// If this is a known project type add it to the project list
						if (project.ProjectType == ProjectType.Local_CSHARP || project.ProjectType == ProjectType.Web_CSHARP) {
							_projects.Add(project.ID, project);
						}
					}
				}
				else if (projectTypeGUID == "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}") // VB.NET project
				{
					Project project = new Project(this, new Guid(id), name);
					string absolutePath = GetAbsoluteProjectPath(path);

					if (!String.IsNullOrEmpty(absolutePath)) {
						project.Read(absolutePath);
						project.RelativePath = Path.GetDirectoryName(absolutePath);

						// If this is a known project type add it to the project list
						if (project.ProjectType == ProjectType.VBNET) {
							_projects.Add(project.ID, project);
						}
					}
				}
				else if (projectTypeGUID == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}") // C++ project
				{
					Project project = new Project(this, new Guid(id), name);
					string absolutePath = GetAbsoluteProjectPath(path);

					if (!String.IsNullOrEmpty(absolutePath)) {
						project.Read(absolutePath);
						project.RelativePath = Path.GetDirectoryName(absolutePath);

						// If this is a known project type add it to the project list
						if (project.ProjectType == ProjectType.CPP) {
							_projects.Add(project.ID, project);
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets the abosulte path of a project specified by the supplied path.
		/// </summary>
		/// <param name="path">Th project path.</param>
		/// <returns>The aboslute path.</returns>
		private string GetAbsoluteProjectPath(string path) {
			string absoluteProjectPath = String.Empty;
			if (path.StartsWith("http:")) {
				Uri projectURL = new Uri(path);
				if (projectURL.Authority == "localhost") {
					//we will assume thet the virtual directory is on site 1 of localhost
					DirectoryEntry root = new DirectoryEntry("IIS://localhost/w3svc/1/root");
					string rootPath = root.Properties["Path"].Value as String;
					//we will also assume that the user has been clever and changed to virtual directory local path...
					absoluteProjectPath = rootPath + projectURL.AbsolutePath;
				}
			} else {
				absoluteProjectPath = Path.Combine(_directory, path);
			}
			return absoluteProjectPath;
		}

		/// <summary>Gets the project with the specified name.</summary>
		/// <param name="name">The project name.</param>
		/// <returns>The project.</returns>
		public Project GetProject(string name) {
			foreach (Project project in _projects.Values) {
				if (project.Name == name) {
					return project;
				}
			}

			return null;
		}

		/// <summary>Allows you to enumerate (using foreach) over the 
		/// solution's projects.</summary>
		/// <returns>An enumerable list of projects.</returns>
		public IEnumerable GetProjects() {
			return _projects.Values;
		}

		/// <summary>Gets a count of the number of projects in the solution</summary>
		public int ProjectCount {
			get {
				return _projects.Count;
			}
		}

	}
}
