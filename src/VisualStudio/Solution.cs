using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

namespace NDoc.VisualStudio
{
	/// <summary>
	/// Represents a Visual Studio solution file.
	/// </summary>
	/// <remarks>
	/// This class is used to read a Visual Studio solution file
	/// </remarks>
	public class Solution
	{
		/// <summary>
		/// Initializes a new instance of the Solution class.
		/// </summary>
		/// <param name="slnPath">The Visual Studio solution file to parse.</param>
		public Solution(string slnPath)
		{
			Read(slnPath);
		}

		private string _directory;

		/// <summary>Gets the SolutionDirectory property.</summary>
		/// <remarks>This is the directory that contains the VS.NET
		/// solution file.</remarks>
		public string Directory
		{
			get { return _directory; }
		}

		private string _name;

		/// <summary>Gets the SolutionName property.</summary>
		/// <remarks>This is the name of the VS.NET solution file
		/// without the .sln extension.</remarks>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>Reads a .sln file.</summary>
		/// <param name="path">The path to the .sln file.</param>
		private void Read(string path)
		{
			path = Path.GetFullPath(path);
			_directory = Path.GetDirectoryName(path);
			_name = Path.GetFileNameWithoutExtension(path);

			StreamReader reader = null;

			try
			{
				reader = new StreamReader(path);

				string line = reader.ReadLine();
				if (line != "Microsoft Visual Studio Solution File, Format Version 7.00")
				{
					throw new ApplicationException("This is not a 'Microsoft Visual Studio Solution File, Format Version 7.00' file.");
				}

				while ((line = reader.ReadLine()) != null)
				{
					if (line.StartsWith("Project"))
					{
						AddProject(line);
					}
					else if (line.StartsWith("\tGlobalSection(SolutionConfiguration)"))
					{
						ReadSolutionConfig(reader);
					}
					else if (line.StartsWith("\tGlobalSection(ProjectConfiguration)"))
					{
						ReadProjectConfig(reader);
					}
				}
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}
		}

		private Hashtable _configurations = new Hashtable();

		/// <summary>
		/// Returns the specified project's configuration name based for 
		/// a specific solution configuration.
		/// </summary>
		/// <param name="solutionConfig">The solution configuration name.</param>
		/// <param name="projectId">The project guid.</param>
		/// <returns>The project configuration name.</returns>
		public string GetProjectConfigName(string solutionConfig, string projectId)
		{
			return (string)((Hashtable)_configurations[solutionConfig])[projectId];
		}

		/// <summary>
		/// Get the solution's configurations.
		/// </summary>
		/// <returns>A collection of configuration names.</returns>
		public ICollection GetConfigurations()
		{
			return _configurations.Keys;
		}

		private void ReadSolutionConfig(TextReader reader)
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith("\tEndGlobalSection"))
					return;

				int eqpos = line.IndexOf('=');
				string config = line.Substring(eqpos + 2);

				_configurations.Add(config, new Hashtable());
			}
		}

		private void ReadProjectConfig(TextReader reader)
		{
			const string pattern = @"^\t\t(?<projid>\S+)\.(?<solcfg>\S+)\.Build\.\d+ = (?<projcfg>\S+)\|.+";
			Regex regex = new Regex(pattern);
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith("\tEndGlobalSection"))
					return;

				Match match = regex.Match(line);
				if (match.Success)
				{
					string projid = match.Groups["projid"].Value;
					string solcfg = match.Groups["solcfg"].Value;
					string projcfg = match.Groups["projcfg"].Value;
					projid = (new Guid(projid)).ToString();

					((Hashtable)_configurations[solcfg]).Add(projid, projcfg);
				}
			}
		}

		private Hashtable _projects = new Hashtable();

		private void AddProject(string projectLine)
		{
			string pattern = @"^Project\(""(?<unknown>\S+)""\) = ""(?<name>\S+)"", ""(?<path>\S+)"", ""(?<id>\S+)""";
			Regex regex = new Regex(pattern);
			Match match = regex.Match(projectLine);
		
			if (match.Success)
			{
				string unknown = match.Groups["unknown"].Value;
				string name = match.Groups["name"].Value;
				string path = match.Groups["path"].Value;
				string id = match.Groups["id"].Value;

				if (!path.StartsWith("http://"))
				{
					Project project = new Project(this, new Guid(id), name);
					string absoluteProjectPath = Path.Combine(_directory, path);
					project.Read(absoluteProjectPath);

					string relativeProjectPath = Path.GetDirectoryName(path);
					project.RelativePath = relativeProjectPath;

					if (project.ProjectType == "C# Local")
					{
						_projects.Add(project.ID, project);
					}
				}
			}
		}


		/// <summary>Gets the project with the specified GUID.</summary>
		/// <param name="id">The GUID used to identify the project in the .sln file.</param>
		/// <returns>The project.</returns>
		public Project GetProject(Guid id)
		{
			return (Project)_projects[id];
		}

		/// <summary>Gets the project with the specified name.</summary>
		/// <param name="name">The project name.</param>
		/// <returns>The project.</returns>
		public Project GetProject(string name)
		{
			foreach (Project project in _projects.Values)
			{
				if (project.Name == name)
				{
					return project;
				}
			}

			return null;
		}

		/// <summary>Allows you to enumerate (using foreach) over the 
		/// solution's projects.</summary>
		/// <returns>An enumerable list of projects.</returns>
		public IEnumerable GetProjects()
		{
			return _projects.Values;
		}

	}
}
