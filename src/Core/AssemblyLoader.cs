using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace NDoc.Core
{
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
	public class AssemblyLoader
	{
		/// <summary>primary search directories.</summary>
		private ArrayList projectDirectories;

		/// <summary>Additional search directories.</summary>
		private ArrayList referenceDirectories;

		/// <summary>List of subdirectory lists already scanned.</summary>
		private Hashtable directoryLists;

		/// <summary>assemblies already scanned, but not loaded.</summary>
		/// <remarks>Maps Assembly FullName to Filename for assemblies scanned, 
		/// but not loaded because they were not a match to the required FullName.
		/// <p>This list is scanned twice,</p>
		/// <list type="unordered">
		/// <term>If the requested assembly has not been loaded, but is in this list, then the file is loaded.</term>
		/// <term>Once all search paths have been exhausted in an exact name match, this list is checked for a 'partial' match.</term>
		/// </list></remarks>
		private NameValueCollection AssemblyNameFileNameMap;

		/// <summary>Whether or not to include subdirectories in searches.</summary>
		private bool includeSubdirs = false;

		// loaded assembly cache keyed by Assembly FullName
		private Hashtable assemblysLoadedAssyName;

		// loaded assembly cache keyed by Assembly FileName
		private Hashtable assemblysLoadedFileName;

		/// <summary>
		/// Creates a new <see cref="AssemblyLoader"/> instance.
		/// </summary>
		/// <param name="projectDirectories">Project directories.</param>
		/// <param name="referenceDirectories">Reference directories.</param>
		public AssemblyLoader(ArrayList projectDirectories,ArrayList referenceDirectories)
		{
			this.assemblysLoadedAssyName = new Hashtable();
			this.assemblysLoadedFileName = new Hashtable();
			this.AssemblyNameFileNameMap = new NameValueCollection();
			this.projectDirectories = projectDirectories;
			this.referenceDirectories = referenceDirectories;
			this.directoryLists = new Hashtable();
		}

		/// <summary>
		/// Whether or not to include sub-directories in the searches which
		/// are in response to the AssemblyResolve event.
		/// </summary>
		public bool IncludeSubdirs
		{
			get { return (includeSubdirs); }
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
		public void Deinstall() 
		{
			AppDomain.CurrentDomain.AssemblyResolve -= 
				new ResolveEventHandler(this.ResolveAssembly);
		}

		/// <summary>Loads an assembly.</summary>
		/// <param name="fileName">The assembly filename.</param>
		/// <returns>The assembly object.</returns>
		/// <remarks>This method loads an assembly into memory. If you
		/// use Assembly.Load or Assembly.LoadFrom the assembly file locks.
		/// This method doesn't lock the assembly file.</remarks>
		public Assembly LoadAssembly(string fileName)
		{
			// have we already loaded this assembly?
			Assembly assy = assemblysLoadedFileName[fileName] as Assembly;
			
			// Assembly not loaded, so we must go a get it
			if (assy == null)
			{
				Trace.WriteLine(String.Format("LoadAssembly: {0}", fileName));

				// we will load the assembly image into a byte array, then get the CLR to load it
				// This allows us to side-step the host permissions which would otherwise prevent
				// loading from a network share...also we don't have the overhead over shadow-copying 
				// to avoid assembly locking
				FileStream assyFile = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 16384);
				byte[] bin = new byte[16384];
				long rdlen = 0;
				long total = assyFile.Length;
				int len;
				MemoryStream memStream = new MemoryStream((int)total);
				rdlen = 0;
				while (rdlen < total)
				{
					len = assyFile.Read(bin, 0, 16384);
					memStream.Write(bin, 0, len);
					rdlen = rdlen + len;
				}
				// done with input file
				assyFile.Close();

				
				// Now we have the assembly image, try to load it into the CLR
				try
				{
					assy = Assembly.Load(memStream.ToArray());
					// If the assembly loaded OK, cache the Assembly ref using the fileName as key.
					assemblysLoadedFileName.Add(fileName, assy);
				}
				catch(System.IO.FileLoadException e)
				{
					if (e.Message == "Exception from HRESULT: 0x80131019.")
					{
						try
						{
							// LoadFile is really preferable, 
							// but since .Net 1.0 doesn't have it,
							// we have to use LoadFrom on that framework...
#if(NET_1_0)
							assy = Assembly.LoadFrom(fileName);
#else
							assy = Assembly.LoadFile(fileName);
#endif
						}
						catch (Exception e2)
						{
							throw new DocumenterException(string.Format(CultureInfo.InvariantCulture, "Unable to load assembly '{0}'", fileName), e2);
						}
					}
					else
						throw new DocumenterException(string.Format(CultureInfo.InvariantCulture, "Unable to load assembly '{0}'", fileName), e);
				}
				catch (Exception e)
				{
					throw new DocumenterException(string.Format(CultureInfo.InvariantCulture, "Unable to load assembly '{0}'", fileName), e);
				}
 
			}

			return assy;
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
			foreach (Assembly a in assemblies) 
			{
				if (a.FullName == args.Name) 
				{
					return a;
				}
			}

			Debug.WriteLine(
				"Attempting to resolve assembly " + args.Name + ".", 
				"AssemblyResolver");

			string fileName;

			// we may have already located the assembly but not loaded it...
			fileName = AssemblyNameFileNameMap[args.Name];
			if (fileName!=null && fileName.Length>0)
			{
				return LoadAssembly(AssemblyNameFileNameMap[args.Name]);
			}

			string[] assemblyInfo = args.Name.Split(new char[]{','});

			string fullName = args.Name;
			
			Assembly assy=null;

			// first we will try filenames derived from the assembly name.
	
			// Project Path DLLs
			if (assy==null)
			{
				fileName = assemblyInfo[0] + ".dll";
				assy = LoadAssemblyFrom(this.projectDirectories, fullName, fileName, this.includeSubdirs);
			}

			// Project Path Exes
			if (assy==null)
			{
				fileName = assemblyInfo[0] + ".exe";
				assy = LoadAssemblyFrom(this.projectDirectories, fullName, fileName, this.includeSubdirs);
			}

			// Reference Path DLLs
			if (assy==null)
			{
				fileName = assemblyInfo[0] + ".dll";
				assy = LoadAssemblyFrom(this.referenceDirectories, fullName, fileName, this.includeSubdirs);
			}

			// Reference Path Exes
			if (assy==null)
			{
				fileName = assemblyInfo[0] + ".exe";
				assy = LoadAssemblyFrom(this.referenceDirectories, fullName, fileName, this.includeSubdirs);
			}

			if (assy==null)
			{
				//nothing found so far, get desperate and start looking for partial name matches in
				//the assemblies we have already loaded...
				assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly a in assemblies) 
				{
					string[] assemblyNameParts = a.FullName.Split(new char[]{','});
					if (assemblyNameParts[0] == assemblyInfo[0])
					{
						assy =  a;
						break;
					}
				}
			}

			if (assy==null)
			{
				//get even more desperate and start looking for partial name matches in
				//the assemblies we have already scanned...
				foreach (string assemblyName in AssemblyNameFileNameMap.Keys)
				{

					string[] assemblyNameParts = assemblyName.Split(new char[]{','});
					if (assemblyNameParts[0] == assemblyInfo[0])
					{
						assy =  LoadAssembly(AssemblyNameFileNameMap[assemblyName]);
						break;
					}
				}
			}

			return assy;
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
		private Assembly LoadAssemblyFrom(ArrayList dirs, string fullName, string fileName, bool includeSubDirs) 
		{
			Assembly assembly = null;
			if ((dirs == null) || (dirs.Count == 0)) return (null);

			foreach (string path in dirs)
			{
				if (Directory.Exists(path))
				{
					string fn = Path.Combine(path, fileName);
					if (File.Exists(fn)) 
					{
						// file exists, check it's the right assembly
						try
						{
							AssemblyName assyName = AssemblyName.GetAssemblyName(fn);
							if (assyName.FullName==fullName)
							{
								//This looks like the right assembly, try loading it
								try
								{
									assembly = LoadAssembly(fn);
									return (assembly);
								}
								catch (Exception e)
								{
									Debug.WriteLine("Assembly Load Error: " + e.Message, "AssemblyResolver");
								}
							}
							else
							{
								//nope, names don't match; save the FileName and AssemblyName map
								//in case we need this assembly later...
								AssemblyNameFileNameMap.Add(assyName.FullName,fn);
							}
						}
						catch (Exception e)
						{
							//oops this wasn't a valid assembly
							Debug.WriteLine("AssemblyResolver: File " + fn + " not a valid assembly");
							Debug.WriteLine(e.Message);
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
						return (LoadAssemblyFrom(subDirList, fullName, fileName, true));
					}
				}
			}
			return (null);
		}

		private string[] GetSubDirectories(string parentDir) 
		{
			string[] subdirs = (string[])this.directoryLists[parentDir];
			if (null == subdirs) 
			{
				subdirs = Directory.GetDirectories(parentDir);
				this.directoryLists.Add(parentDir, subdirs);
			}
			return subdirs;
		}

	}
}

