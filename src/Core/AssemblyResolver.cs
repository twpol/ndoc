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
	/// Class AssemblyResolver resolves assemblies not found by the system.
	/// An instance of this class is configured with a base directory and hooks
	/// up to the AppDomain.AssemblyResolve event. Whenever called, the instance
	/// checks the associated directory with all subdirectories for the assembly
	/// requested.
	/// </summary>
	/// <remarks>
	/// The class implements two features to speed up the search:
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
	public class AssemblyResolver {

		#region Initialization & Termination

		/// <summary> 
		/// Constructs an instance of this type.
		/// </summary>
		/// <param name="baseDirectory">The base directory for assembly searching.</param>
		public AssemblyResolver(string baseDirectory) {
			this.baseDirectory = baseDirectory;
			this.directoryLists = new Hashtable();
		}

		#endregion

		#region Public Operations

		/// <summary> 
		/// Installs the assembly resolver by hooking up to the AppDomain's AssemblyResolve event.
		/// </summary>
		public void Install() {
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
			return LoadAssemblyFrom(this.baseDirectory, fullName, fileName, true);
		}

		/// <summary> 
		/// Loads the assembly with the specified filename from the specified directory.
		/// If the assembly is not found in the directory, all subdirectories of the
		/// directory are searched for the assembly.
		/// </summary>
		/// <param name="path">The base directory to look in.</param>
		/// <param name="fullName">
		/// Fully qualified assembly name. If not empty, the full name of each assembly found is
		/// compared to this name and the assembly is accepted only, if the names match.
		/// </param>
		/// <param name="fileName">The name of the assembly.</param>
		/// <param name="includeSubDirs">true, to include subdirectories.</param>
		/// <returns>The assembly, null, if not found.</returns>
		private Assembly LoadAssemblyFrom(string path, string fullName, string fileName, bool includeSubDirs) {

			Assembly assembly = null;
			string fn = Path.Combine(path, fileName);
			try {
				if (true == File.Exists(fn)) {
					assembly = BaseDocumenter.LoadAssembly(fn);
					if ("" != fullName && fullName != assembly.FullName) {
//						assembly = null;
						Debug.WriteLine(
							"Assembly: " + fullName + ", Wrong Assembly Version. Loaded anyway!",
							"AssemblyResolver");
					} else {
						Debug.WriteLine(
							"Assembly Loaded: " + fn,
							"AssemblyResolver");
					}
				}
			} catch (Exception e) {
				Debug.WriteLine(
					"Error: " + e.Message,
					"AssemblyResolver");
			}

			// scan subdirectories
			if (null == assembly && true == includeSubDirs) {
				string[] subdirs = GetSubDirectories(path);
				foreach (string subdir in subdirs) {
					string p = Path.Combine(path, subdir);
					assembly = LoadAssemblyFrom(p, fullName, fileName, true);
					if (null != assembly) {
						break;
					}
				}
			}
			return assembly;
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
		private string baseDirectory;

		/// <summary>List of subdirectory lists already scanned.</summary>
		private Hashtable directoryLists;

		#endregion
	}
}

