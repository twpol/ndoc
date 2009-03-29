// Copyright (C) 2004  Kevin Downs
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace NDoc3.Core.Reflection
{
	/// <summary>
	/// Handles the resolution and loading of assemblies.
	/// </summary>
	internal class AssemblyLoader : IAssemblyLoader
	{
		/// <summary>primary search directories.</summary>
		private readonly ReferencePathCollection searchDirectories = new ReferencePathCollection();

		/// <summary>List of subdirectory lists already scanned.</summary>
		private readonly Hashtable directoryLists = new Hashtable();

		/// <summary>List of directories already scanned.</summary>
		private readonly Hashtable searchedDirectories = new Hashtable();

		/// <summary>List of Assemblies that could not be resolved.</summary>
		private readonly Hashtable unresolvedAssemblies = new Hashtable();

		/// <summary>assemblies already scanned, but not loaded.</summary>
		/// <remarks>Maps Assembly FullName to Filename for assemblies scanned, 
		/// but not loaded because they were not a match to the required FullName.
		/// <p>This list is scanned twice,</p>
		/// <list type="unordered">
		/// <term>If the requested assembly has not been loaded, but is in this list, then the file is loaded.</term>
		/// <term>Once all search paths have been exhausted in an exact name match, this list is checked for a 'partial' match.</term>
		/// </list></remarks>
		private readonly ReferenceTypeDictionary<string, FileInfo> AssemblyNameFileNameMap = new ReferenceTypeDictionary<string, FileInfo>();

		/// <summary>Loaded assembly cache keyed by Assembly FileName</summary>
		private readonly Hashtable assemblysLoadedFileName = new Hashtable();

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
		/// </summary>
		/// <param name="referenceDirectories">Reference directories.</param>
		public AssemblyLoader(params ReferencePath[] referenceDirectories)
			: this(new ReferencePathCollection())
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="AssemblyLoader"/> class.
		/// </summary>
		/// <param name="referenceDirectories">Reference directories.</param>
		public AssemblyLoader(ReferencePathCollection referenceDirectories)
		{
			if (referenceDirectories != null) {
				searchDirectories.AddRange(referenceDirectories);
			}
		}

		/// <summary>
		/// Add the path to the list of directories for dependency resolution
		/// </summary>
		public void AddSearchDirectory(ReferencePath path)
		{
			searchDirectories.Add(path);
		}

		/// <summary>
		/// Directories Searched for assemblies.
		/// </summary>
		public ICollection SearchedDirectories
		{
			// TODO (EE): return typed collection
			get { return searchedDirectories.Keys; }
		}

		/// <summary>
		/// Assemblies that could not be resolved.
		/// </summary>
		public ICollection UnresolvedAssemblies
		{
			// TODO (EE): return typed collection
			get { return unresolvedAssemblies.Keys; }
		}

		/// <summary> 
		/// Installs the assembly resolver by hooking up to the AppDomain's AssemblyResolve event.
		/// </summary>
		public void Install()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
		}

		//		/// <summary> 
		//		/// Deinstalls the assembly resolver.
		//		/// </summary>
		//		public void Deinstall() 
		//		{
		//			AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;
		//		}

		/// <summary>Loads an assembly.</summary>
		/// <param name="assemblyFile">The assembly filename.</param>
		/// <returns>The assembly object.</returns>
		/// <remarks>This method loads an assembly into memory. If you
		/// use Assembly.Load or Assembly.LoadFrom the assembly file locks.
		/// This method doesn't lock the assembly file.</remarks>
		public IAssemblyInfo GetAssemblyInfo(FileInfo assemblyFile)
		{
			return new ReflectionAssemblyInfo(LoadAssembly(assemblyFile));
		}

		/// <summary>Loads an assembly.</summary>
		/// <param name="assemblyFile">The assembly filename.</param>
		/// <returns>The assembly object.</returns>
		/// <remarks>This method loads an assembly into memory. If you
		/// use Assembly.Load or Assembly.LoadFrom the assembly file locks.
		/// This method doesn't lock the assembly file.</remarks>
		protected Assembly LoadAssembly(FileInfo assemblyFile)
		{
			string fileName = assemblyFile.FullName;

			// have we already loaded this assembly?
			Assembly assy = assemblysLoadedFileName[fileName] as Assembly;

			//double check assy not already loaded
			if (assy == null) {
				AssemblyName assyName = AssemblyName.GetAssemblyName(fileName);
				foreach (Assembly loadedAssy in AppDomain.CurrentDomain.GetAssemblies()) {
					if (assyName.FullName == loadedAssy.FullName) {
						assy = loadedAssy;
						break;
					}
				}
			}

			// Assembly not loaded, so we must go a get it
			if (assy == null) {
				Trace.WriteLine(String.Format("LoadAssembly: {0}", fileName));

				// we will load the assembly image into a byte array, then get the CLR to load it
				// This allows us to side-step the host permissions which would otherwise prevent
				// loading from a network share...also we don't have the overhead over shadow-copying 
				// to avoid assembly locking
				FileStream assyFile = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384);
				byte[] bin = new byte[16384];
				long total = assyFile.Length;
				MemoryStream memStream = new MemoryStream((int)total);
				long rdlen = 0;
				while (rdlen < total) {
					int len = assyFile.Read(bin, 0, 16384);
					memStream.Write(bin, 0, len);
					rdlen = rdlen + len;
				}
				// done with input file
				assyFile.Close();


				// Now we have the assembly image, try to load it into the CLR
				try {
					// ensure the assembly's path is added to the search list for resolving dependencies
					string assyDir = System.IO.Path.GetDirectoryName(fileName);
					searchDirectories.Add(new ReferencePath(assyDir));

					Evidence evidence = CreateAssemblyEvidence(fileName);
					assy = Assembly.Load(memStream.ToArray(), null, evidence);
					// If the assembly loaded OK, cache the Assembly ref using the fileName as key.
					assemblysLoadedFileName.Add(fileName, assy);
				} catch (SecurityException e) {
					if (e.Message.IndexOf("0x8013141A") != -1) {
						throw new SecurityException(String.Format("Strong name validation failed for assembly '{0}'.", fileName));
					}
					throw;
				} catch (FileLoadException e) {
					// HACK: replace the text comparison with non-localized test when further details are available
					if ((e.Message.IndexOf("0x80131019") != -1) ||
						(e.Message.IndexOf("contains extra relocations") != -1)) {
						try {
							// LoadFile is really preferable, 
							// but since .Net 1.0 doesn't have it,
							// we have to use LoadFrom on that framework...
							assy = Assembly.LoadFile(fileName);
						} catch (Exception e2) {
							throw new DocumenterException(string.Format(CultureInfo.InvariantCulture, "Unable to load assembly '{0}'", fileName), e2);
						}
					} else
						throw new DocumenterException(string.Format(CultureInfo.InvariantCulture, "Unable to load assembly '{0}'", fileName), e);
				} catch (Exception e) {
					throw new DocumenterException(string.Format(CultureInfo.InvariantCulture, "Unable to load assembly '{0}'", fileName), e);
				}

			}

			return assy;
		}

		/// <summary>
		/// Creates assembly evidence
		/// </summary>
		/// <param name="fileName">The assembly filename</param>
		/// <returns>The new assembly evidence</returns>
		static Evidence CreateAssemblyEvidence(string fileName)
		{
			//HACK: I am unsure whether 'Hash' evidence is required - since this will be difficult to obtain, we will not supply it...

			Evidence newEvidence = new Evidence();

			//We must have zone evidence, or we will get a policy exception
			Zone zone = new Zone(SecurityZone.MyComputer);
			newEvidence.AddHost(zone);

			//If the assembly is strong-named, we must supply this evidence
			//for StrongNameIdentityPermission demands
			AssemblyName assemblyName = AssemblyName.GetAssemblyName(fileName);
			byte[] pk = assemblyName.GetPublicKey();
			if (pk != null && pk.Length != 0) {
				StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob(pk);
				StrongName strongName = new StrongName(blob, assemblyName.Name, assemblyName.Version);
				newEvidence.AddHost(strongName);
			}

			return newEvidence;
		}

		/// <summary> 
		/// Resolves the location and loads an assembly not found by the system.
		/// </summary>
		/// <remarks>The CLR will take care of loading Framework and GAC assemblies.
		/// <p>The resolution process uses the following heuristic</p>
		/// </remarks>
		/// <param name="sender">the sender of the event</param>
		/// <param name="args">event arguments</param>
		/// <returns>the loaded assembly, null, if not found</returns>
		protected Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{

			// first, have we already loaded the required assembly?
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly a in assemblies) {
				if (IsAssemblyNameEquivalent(a.FullName, args.Name)) {
					return a;
				}
			}

			Debug.WriteLine(
				"Attempting to resolve assembly " + args.Name + ".",
				"AssemblyResolver");

			// we may have already located the assembly but not loaded it...
			FileInfo file = AssemblyNameFileNameMap[args.Name];
			if (file != null && file.Exists) {
				return LoadAssembly(file);
			}

			string[] assemblyInfo = args.Name.Split(new[] { ',' });

			string fullName = args.Name;

			// first we will try filenames derived from the assembly name.

			// Project Path DLLs
			string fileName = assemblyInfo[0] + ".dll";
			Assembly assy = LoadAssemblyFrom(fullName, fileName);

			// Project Path Exes
			if (assy == null) {
				fileName = assemblyInfo[0] + ".exe";
				assy = LoadAssemblyFrom(fullName, fileName);
			}

			// Reference Path DLLs
			if (assy == null) {
				fileName = assemblyInfo[0] + ".dll";
				assy = LoadAssemblyFrom(fullName, fileName);
			}

			// Reference Path Exes
			if (assy == null) {
				fileName = assemblyInfo[0] + ".exe";
				assy = LoadAssemblyFrom(fullName, fileName);
			}

			//if the requested assembly did not have a strong name, we can
			//get even more desperate and start looking for partial name matches
			if (assemblyInfo.Length < 4 || assemblyInfo[3].Trim() == "PublicKeyToken=null") {
				if (assy == null) {
					//start looking for partial name matches in
					//the assemblies we have already loaded...
					assemblies = AppDomain.CurrentDomain.GetAssemblies();
					foreach (Assembly a in assemblies) {
						if (a.FullName != null) {
							string[] assemblyNameParts = a.FullName.Split(new[] { ',' });
							if (assemblyNameParts[0] == assemblyInfo[0]) {
								assy = a;
								break;
							}
						} else
							throw new Exception("Assembly fullname are null");
					}
				}

				if (assy == null) {
					//get even more desperate and start looking for partial name matches
					//the assemblies we have already scanned...
					foreach (string assemblyName in AssemblyNameFileNameMap.Keys) {

						string[] assemblyNameParts = assemblyName.Split(new[] { ',' });
						if (assemblyNameParts[0] == assemblyInfo[0]) {
							FileInfo assemblyFile = AssemblyNameFileNameMap[assemblyName];
							if (assemblyFile != null && assemblyFile.Exists) {
								assy = LoadAssembly(assemblyFile);
								break;
							}
						}
					}
				}
			}

			if (assy == null) {
				if (!unresolvedAssemblies.ContainsKey(args.Name))
					unresolvedAssemblies.Add(args.Name, null);
			}

			return assy;
		}

		/// <summary> 
		/// Search for and load the specified assembly in a set of directories.
		/// This will optionally search recursively.
		/// </summary>
		/// <param name="fullName">
		/// Fully qualified assembly name. If not empty, the full name of each assembly found is
		/// compared to this name and the assembly is accepted only, if the names match.
		/// </param>
		/// <param name="fileName">The name of the assembly.</param>
		/// <returns>The assembly, or null if not found.</returns>
		private Assembly LoadAssemblyFrom(string fullName, string fileName)
		{

			if ((searchDirectories == null) || (searchDirectories.Count == 0))
				return (null);

			foreach (ReferencePath rp in searchDirectories) {
				if (Directory.Exists(rp.Path)) {
					Assembly assy = LoadAssemblyFrom(fullName, fileName, rp.Path, rp.IncludeSubDirectories);
					if (assy != null)
						return assy;
				}
			}
			return null;
		}

		/// <summary> 
		/// Search for and load the specified assembly in a given directory.
		/// This will optionally search recursively into sub-directories if requested.
		/// </summary>
		/// <param name="path">The directory to look in.</param>
		/// <param name="fullName">
		/// Fully qualified assembly name. If not empty, the full name of each assembly found is
		/// compared to this name and the assembly is accepted only, if the names match.
		/// </param>
		/// <param name="fileName">The name of the assembly.</param>
		/// <param name="includeSubDirs">true, search subdirectories.</param>
		/// <returns>The assembly, or null if not found.</returns>
		private Assembly LoadAssemblyFrom(string fullName, string fileName, string path, bool includeSubDirs)
		{
			if (!searchedDirectories.ContainsKey(path)) {
				searchedDirectories.Add(path, null);
			}
			FileInfo assemblyFileInfo = new FileInfo(Path.Combine(path, fileName));
			if (assemblyFileInfo.Exists) {
				// file exists, check it's the right assembly
				try {
					AssemblyName assyName = AssemblyName.GetAssemblyName(assemblyFileInfo.FullName);
					if (IsAssemblyNameEquivalent(assyName.FullName, fullName)) {
						//This looks like the right assembly, try loading it
						try {
							Assembly assembly = LoadAssembly(assemblyFileInfo);
							return (assembly);
						} catch (Exception e) {
							Debug.WriteLine("Assembly Load Error: " + e.Message, "AssemblyResolver");
						}
					} else {
						//nope, names don't match; save the FileName and AssemblyName map
						//in case we need this assembly later...
						//only first found occurence of fully-qualifed assembly name is cached
						if (!AssemblyNameFileNameMap.ContainsKey(assyName.FullName)) {
							AssemblyNameFileNameMap.Add(assyName.FullName, assemblyFileInfo);
						}
					}
				} catch (Exception e) {
					//oops this wasn't a valid assembly
					Debug.WriteLine("AssemblyResolver: File " + assemblyFileInfo + " not a valid assembly");
					Debug.WriteLine(e.Message);
				}
			} else {
				Debug.WriteLine("AssemblyResolver: File " + fileName + " not in " + path);
			}

			// not in this dir (or load failed), scan subdirectories
			if (includeSubDirs) {
				string[] subdirs = GetSubDirectories(path);
				foreach (string subdir in subdirs) {
					Assembly assy = LoadAssemblyFrom(fullName, fileName, subdir, true);
					if (assy != null)
						return assy;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets all subdirectories of a directory
		/// </summary>
		/// <param name="parentDir">The parent directory</param>
		/// <returns>Array containing all subdirectories</returns>
		private string[] GetSubDirectories(string parentDir)
		{
			string[] subdirs = (string[])directoryLists[parentDir];
			if (null == subdirs) {
				subdirs = Directory.GetDirectories(parentDir);
				directoryLists.Add(parentDir, subdirs);
			}
			return subdirs;
		}

		private bool IsAssemblyNameEquivalent(string AssyFullName, string RequiredAssyName)
		{
			if (RequiredAssyName.Length < AssyFullName.Length)
				return (AssyFullName.Substring(0, RequiredAssyName.Length) == RequiredAssyName);
			return (AssyFullName == RequiredAssyName.Substring(0, AssyFullName.Length));
		}
	}
}

