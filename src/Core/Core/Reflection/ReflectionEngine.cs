using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace NDoc3.Core.Reflection
{
	///<summary>
	/// Used to reflect assemblies of a given project
	///</summary>
	public class ReflectionEngine : IDisposable
	{
		private readonly AppDomain _remoteDomain;
		private readonly ReflectionEngineServer _remoteServer;

		/// <summary>
		/// Sets up a remote domain for reflecting assemblies.
		/// </summary>
		/// <param name="referencePaths">paths for resolving assembly dependencies</param>
		public ReflectionEngine(ReferencePathCollection referencePaths)
		{
			_remoteDomain = CreateRemoteDomain();
			_remoteServer = CreateRemoteInstance(_remoteDomain, referencePaths);
		}

		///<summary>
		/// Unloads the remote <see cref="AppDomain"/>
		///</summary>
		public void Dispose()
		{
			AppDomain.Unload(_remoteDomain);
		}

		///<summary>
		/// Reflects assemblies and generates an NDoc xml file
		///</summary>
		///<param name="args">configuration controlling the reflection process.</param>
		///<param name="xmlFile">the file to write the NDoc xml content to</param>
		internal void MakeXmlFile(NDocXmlGeneratorParameters args, FileInfo xmlFile)
		{
			using (NDocXmlGenerator xmlGen = _remoteServer.CreateInstance(args)) {
				xmlGen.MakeXmlFile(xmlFile);
			}
		}

		///<summary>
		/// Reflects assemblies and generates an NDoc xml content
		///</summary>
		///<param name="args">configuration controlling the reflection process.</param>
		///<returns>A string containing the generated XML content</returns>
		internal string MakeXml(NDocXmlGeneratorParameters args)
		{
			using (NDocXmlGenerator xmlGen = _remoteServer.CreateInstance(args)) {
				return xmlGen.MakeXml();
			}
		}

		///<summary>
		/// Scans the given assemblyFile for a list of all namespaces.
		///</summary>
		///<param name="assemblyFile">a local file path to the assembly</param>
		/// <returns>an array of namespaces. Is never <c>null</c></returns>
		public string[] GetNamespacesFromAssembly(FileInfo assemblyFile)
		{
			return _remoteServer.GetNamespacesFromAssembly(assemblyFile);
		}

		private static AppDomain CreateRemoteDomain()
		{
			AppDomain remoteDomain = AppDomain.CreateDomain("NDoc3Reflection",
												   AppDomain.CurrentDomain.Evidence,
												   AppDomain.CurrentDomain.SetupInformation);
			remoteDomain.SetupInformation.ShadowCopyFiles = "true"; //only required for managed c++ assemblies
			return remoteDomain;
		}

		private static ReflectionEngineServer CreateRemoteInstance(AppDomain remoteDomain, ReferencePathCollection referencePaths)
		{
			ReflectionEngineServer server = CreateInstanceAndUnwrap<ReflectionEngineServer>(remoteDomain);
			server.Initialize(referencePaths);
			return server;
		}

		private static T CreateInstanceAndUnwrap<T>(AppDomain appDomain)
		{
			return (T)appDomain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
			//                    appDomain.CreateInstanceAndUnwrap(typeof(ReflectionEngineServer).Assembly.FullName,
			//                    typeof(ReflectionEngineServer).FullName, false,
			//                    BindingFlags.Public | BindingFlags.Instance,
			//                    null, new object[0], CultureInfo.InvariantCulture, new object[0],
			//                    AppDomain.CurrentDomain.Evidence);
		}

		#region ReflectionEngineServer

		///<summary>
		/// Used for instantiating the server part of the reflection engine
		///</summary>
		public class ReflectionEngineServer : MarshalByRefObject
		{
			private static AssemblyLoader s_assemblyLoader;

			///<summary>
			/// Creates a new instance and installs the global <see cref="AssemblyLoader"/>.
			///</summary>
			///<exception cref="NotSupportedException"></exception>
			public ReflectionEngineServer()
			{
				if (s_assemblyLoader != null) {
					throw new NotSupportedException("Only 1 instance of " + this.GetType() + " is allowed per AppDomain");
				}
			}

			internal void Initialize(ReferencePathCollection referencePaths)
			{
				s_assemblyLoader = new AssemblyLoader(referencePaths);
				s_assemblyLoader.Install();
			}

			internal NDocXmlGenerator CreateInstance(NDocXmlGeneratorParameters args)
			{
				return new NDocXmlGenerator(s_assemblyLoader, args);
			}

			/// <summary>
			/// Gets the namespaces from assembly.
			/// </summary>
			/// <param name="assemblyFile">Assembly file name.</param>
			/// <returns></returns>
			internal string[] GetNamespacesFromAssembly(FileInfo assemblyFile)
			{
				try {
					IAssemblyInfo a = s_assemblyLoader.GetAssemblyInfo(assemblyFile);
					List<string> namespaces = new List<string>();

					foreach (Type t in a.GetTypes()) {
						string ns = t.Namespace;
						{
							if (ns == null) {
								if ((!namespaces.Contains("(global)")))
									namespaces.Add("(global)");
							} else {
								if ((!namespaces.Contains(ns)))
									namespaces.Add(ns);
							}
						}
					}

					return namespaces.ToArray();
				} catch (ReflectionTypeLoadException rtle) {
					StringBuilder sb = new StringBuilder();
					if (s_assemblyLoader.UnresolvedAssemblies.Count > 0) {
						sb.Append("One or more required assemblies could not be located : \n");
						foreach (string ass in s_assemblyLoader.UnresolvedAssemblies) {
							sb.AppendFormat("   {0}\n", ass);
						}
						sb.Append("\nThe following directories were searched, \n");
						foreach (string dir in s_assemblyLoader.SearchedDirectories) {
							sb.AppendFormat("   {0}\n", dir);
						}
					} else {
						Hashtable fileLoadExceptions = new Hashtable();
						foreach (Exception loaderEx in rtle.LoaderExceptions) {
							FileLoadException fileLoadEx = loaderEx as FileLoadException;
							if (fileLoadEx != null) {
								if (!fileLoadExceptions.ContainsKey(fileLoadEx.FileName)) {
									fileLoadExceptions.Add(fileLoadEx.FileName, null);
									sb.Append("Unable to load: " + fileLoadEx.FileName + "\r\n");
								}
							}
							sb.Append(loaderEx.Message + Environment.NewLine);
							sb.Append(loaderEx.StackTrace + Environment.NewLine);
							sb.Append("--------------------" + Environment.NewLine + Environment.NewLine);
						}
					}
					throw new DocumenterException(sb.ToString());
				}
			}
		}

		#endregion
	}

}
