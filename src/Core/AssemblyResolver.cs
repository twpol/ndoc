// <file> 
// <copyright>(c) Siemens AG 2001, All rights reserved.</copyright>
// 
// <author>Wolfgang Bauer</author>
// <datecreated>12/05/01</datecreated>
// 
// <summary>
// This module contains class AssemblyResolver. AssemblyResolver can be
// used to extend .NET's assembly resolving mechanism based on an arbitrarily
// directory and all subdirectories of this base directory.
// </summary>
// </file>

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Diagnostics;

namespace NDoc.Core {

	/// <summary> 
	/// Resolves assemblies located in a specified directory and its sub-directories.
	/// </summary>
	/// <remarks>
	/// <para>Class AssemblyResolver resolves assemblies not found by the system.
	/// An instance of this class is configured with a base directory and hooks
	/// up to the AppDomain.AssemblyResolve event. Whenever called, the instance
	/// checks the associated directory with all subdirectories for the assembly
	/// requested.</para>
	/// <para>The class implements two features to speed up the search:</para>
	/// <list type="bullet">
	/// <item><description>
	/// AssemblyList: Before searching the file system, the assembly list is searched.
	/// Once an assembly has been found, it is added to the AssemblyList. 
	/// </description></item>
	/// <item><description>
	/// SubDirectoryCache: The class caches the subdirectories of each directory once
	/// they have been determined. This avoids repeated filesystem queries for subdirectories.
	/// </description></item>
	/// </list>
	/// </remarks>
	public class AssemblyResolver 
	{

		#region Initialization & Termination

		/// <summary> 
		/// Constructs an instance of this type.
		/// </summary>
		/// <param name="directories">A list of directories to search for assemblies in.</param>
		public AssemblyResolver(ArrayList directories) 
		{
			this.directories = directories;
			this.directoryLists = new Hashtable();
		}

		#endregion

		#region Public Methods and Properties

		/// <summary>
		/// Whether or not to include sub-directories in the searches which
		/// are in response to the AssemblyResolve event.
		/// </summary>
		public bool IncludeSubdirs
		{
			get { return(includeSubdirs); }
			set { includeSubdirs = value; }
		}

		/// <summary> 
		/// Installs the assembly resolver by hooking up to the AppDomain's AssemblyResolve event.
		/// </summary>
		public void Install() 
		{
			AppDomain.CurrentDomain.AssemblyResolve +=
				new ResolveEventHandler(this.ResolveAssembly);
		}

		/// <summary> 
		/// Deinstalls the assembly resolver.
		/// </summary>
		public void Deinstall() {
			AppDomain.CurrentDomain.AssemblyResolve -=
				new ResolveEventHandler(this.ResolveAssembly);
		}

		#endregion

		#region Event Handlers

		/// <summary> 
		/// Resolves the location and loads an assembly not found by the system.
		/// </summary>
		/// <param name="sender">the sender of the event</param>
		/// <param name="args">event arguments</param>
		/// <returns>the loaded assembly, null, if not found</returns>
		protected Assembly ResolveAssembly(object sender, ResolveEventArgs args) {

			Debug.WriteLine(
				"WARNING: The system cannot resolve assembly " + args.Name + ".",
				"AssemblyResolver");

			string assemblyInfo = "";
			string fileName = Path.GetFileName(args.Name);
			int pos = fileName.IndexOf(',');
			if (-1 < pos) {
				assemblyInfo = fileName.Substring(pos + 1).Trim();
				fileName = fileName.Substring(0, pos);
			}

			pos = fileName.IndexOf(".resources");
			if (-1 < pos) {
//				fileName = fileName.Substring(0, pos);
				// resource loading uses another mechanism not covered here
				return null;
			}

			string fullName = fileName + ", " + assemblyInfo;
			if (0 > fullName.IndexOf("PublicKeyToken")) {
				fullName += ", PublicKeyToken=null";
			}

			fileName += ".dll";

			// first try to find an already loaded assembly
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly a in assemblies) {
				if (a.FullName == fullName) {
					Debug.WriteLine("Assembly found: " + fullName, "AppLoader.EventResolver");
					return a;
				}
			}

			// base directory
			return LoadAssemblyFrom(this.directories, fullName, fileName, this.includeSubdirs);
		}

		/// <summary> 
		/// Search for and load the specified assembly in a set of directories.
		/// This will optionally search recursively.
		/// </summary>
		/// <param name="dirs">The list of directories to look in.</param>
		/// <param name="fullName">
		/// Fully qualified assembly name. If not empty, the full name of each assembly found is
		/// compared to this name and the assembly is accepted only, if the names match.
		/// </param>
		/// <param name="fileName">The name of the assembly.</param>
		/// <param name="includeSubDirs">true, to include subdirectories.</param>
		/// <returns>The assembly, or null if not found.</returns>
		private Assembly LoadAssemblyFrom(ArrayList dirs, string fullName, 
			string fileName, bool includeSubDirs) 
		{
			Assembly assembly = null;
			if ((dirs == null) || (dirs.Count == 0)) return(null);

			foreach(string path in dirs)
			{
				if (Directory.Exists(path))
				{
					string fn = Path.Combine(path, fileName);
					if (File.Exists(fn)) 
					{
						// got it, try load
						try
						{
							assembly = BaseDocumenter.LoadAssembly(fn);
							if ("" != fullName && fullName != assembly.FullName) 
							{
								//						assembly = null;
								Debug.WriteLine("Assembly: " + fullName 
									+ ", Wrong Assembly Version. Loaded anyway!",
									"AssemblyResolver");
							} 
							else 
							{
								Debug.WriteLine("Assembly Loaded: " + fn, "AssemblyResolver");
							}
							return(assembly);
						}
						catch(Exception e)
						{
							Debug.WriteLine("Error: " + e.Message, "AssemblyResolver");
						}
					}
					else
					{
						Debug.WriteLine("AssemblyResolver: File " + fileName + " not in " + path);
					}

					// not in this dir (or load failed), scan subdirectories
					if (includeSubDirs) 
					{
						string[] subdirs = GetSubDirectories(path);
						ArrayList subDirList = new ArrayList();
						foreach (string subdir in subdirs) subDirList.Add(subdir);
						return(LoadAssemblyFrom(subDirList, fullName, fileName, true));
					}
				}
			}
			return(null);
		}

		#endregion

		#region Implementation

		private string[] GetSubDirectories(string parentDir) {
			string[] subdirs = (string[])this.directoryLists[parentDir];
			if (null == subdirs) {
				subdirs = Directory.GetDirectories(parentDir);
				this.directoryLists.Add(parentDir, subdirs);
			}
			return subdirs;
		}

		#endregion

		#region Data

		/// <summary>The base directory used to search for assemblies.</summary>
		private ArrayList directories;

		/// <summary>List of subdirectory lists already scanned.</summary>
		private Hashtable directoryLists;

		/// <summary>Whether or not to include subdirectories in searches.</summary>
		private bool includeSubdirs = false;

		#endregion
	}
}

