// Console.cs - a console application for NDoc3
// Copyright (C) 2001  Jason Diamond
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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using NDoc3.Core;
using System.Diagnostics.CodeAnalysis;
using System.Resources;

namespace NDoc3.ConsoleApplication {
	class EntryPoint {
		private static ResourceManager resourceManager = new ResourceManager("NDoc3.ConsoleApplication.Console", Assembly.GetExecutingAssembly());
		private static CultureInfo currentCulture = CultureInfo.CurrentUICulture;

		private static Project project;
		private static IDocumenterConfig documenterConfig;
		private static DateTime startDateTime;

		public static int Main(string[] args) {
			try {
				WriteLogoBanner();

				project = new Project();
				IDocumenterInfo info = InstalledDocumenters.GetDocumenter("MSDN");
				if (info == null) {
					//MSDN documenterConfig not found, pick the first one available.
					if (InstalledDocumenters.Documenters.Count > 0) {
						info = (IDocumenterInfo)InstalledDocumenters.Documenters[0];
					} else {
						throw new InvalidOperationException(resourceManager.GetString("NoDocumenterAssemblies", currentCulture));
					}
				}
				project.ActiveDocumenter = info;
				documenterConfig = project.ActiveConfig;

				int maxDepth = 20; //to limit recursion depth
				bool propertiesSet = false;
				bool projectSet = false;

				if (args.Length == 0) {
					WriteUsage();
					return 1;
				}

				if (args[0].StartsWith("-help", StringComparison.OrdinalIgnoreCase)) {
					WriteHelp(args);
					return 1;
				}

				foreach (string arg in args) {
					if (arg.StartsWith("-", StringComparison.OrdinalIgnoreCase)) {
						if (string.Compare(arg, "-verbose", StringComparison.OrdinalIgnoreCase) == 0) {
							Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
						} else {
							string[] pair = { arg.Substring(0, arg.IndexOf('=')), arg.Substring(arg.IndexOf('=') + 1) };

							if (pair.Length == 2) {
								string name = pair[0].Substring(1);
								string val = pair[1];

								switch (name.ToUpperInvariant()) {
									case "DOCUMENTER":
										if (propertiesSet) {
											throw new ArgumentException("documenter", resourceManager.GetString("DocumenterBeforeOptions", currentCulture));
										}
										if (projectSet) {
											throw new ArgumentException("documenter", resourceManager.GetString("DocumenterBeforeProjectFile", currentCulture));
										}
										info = InstalledDocumenters.GetDocumenter(val.Replace("_", " "));

										if (info == null) {
											throw new ArgumentException("documenter", resourceManager.GetString("InvalidDocumenter", currentCulture));
										}
										project.ActiveDocumenter = info;
										documenterConfig = project.ActiveConfig;
										break;
									case "PROJECT":
										if (propertiesSet) {
											throw new ArgumentException("project", resourceManager.GetString("ProjectFileBeforeOptions", currentCulture));
										}
										Console.WriteLine(resourceManager.GetString("UsingProjectFile", currentCulture) + val);
										project.Read(val);
										project.ActiveDocumenter = info;
										documenterConfig = project.ActiveConfig;
										projectSet = true;
										string directoryName = Path.GetDirectoryName(val);
										if (!string.IsNullOrEmpty(directoryName)) {
											Directory.SetCurrentDirectory(directoryName);
										}
										Debug.WriteLine(Directory.GetCurrentDirectory());
										break;
									case "RECURSE":
										string[] recPair = val.Split(',');
										if (2 == recPair.Length) {
											maxDepth = Convert.ToInt32(recPair[1], CultureInfo.CurrentCulture);
										}
										RecurseDir(recPair[0], maxDepth);
										break;
									case "NAMESPACESUMMARIES":
										using (StreamReader streamReader = new StreamReader(val)) {
											XmlTextReader reader = new XmlTextReader(streamReader);
											reader.MoveToContent();
											project.Namespaces.Read(reader);
										}
										break;
									case "REFERENCEPATH":
										project.ReferencePaths.Add(new ReferencePath(val));
										break;
									default:
										documenterConfig.SetValue(name, val);
										propertiesSet = true;
										break;
								}
							}
						}
					} else if (arg.IndexOf(',') != -1) {
						string[] pair = arg.Split(',');

						if (pair.Length == 2) {
							project.AssemblySlashDocs.Add(
								new AssemblySlashDoc(pair[0], pair[1]));
						}
					} else {
						string doc = Path.ChangeExtension(arg, ".xml");
						if (File.Exists(doc)) {
							project.AssemblySlashDocs.Add(
								new AssemblySlashDoc(arg, doc));
						} else {
							project.AssemblySlashDocs.Add(
								new AssemblySlashDoc(arg, ""));
						}
					}
				}

				if (project.AssemblySlashDocs.Count == 0) {
					Console.WriteLine(resourceManager.GetString("NoAssembliesSpecified", currentCulture));
					//WriteUsage();
					return 1;
				}
				startDateTime = DateTime.UtcNow;
				IDocumenter documenter = documenterConfig.CreateDocumenter();
				documenter.DocBuildingStep += DocBuildingStepHandler;
				documenter.Build(project);
				TimeSpan ts = DateTime.UtcNow - startDateTime;
				Console.WriteLine(String.Format(CultureInfo.CurrentCulture, resourceManager.GetString("TotalBuildTime", currentCulture), ts.TotalSeconds));
				return 0;
			} catch (Exception except) {
				string errorText = BuildExceptionText(except);
				Console.WriteLine(errorText);
				Trace.WriteLine(errorText);
				return 2;
			}
		}

		private static void WriteUsage() {
			Console.WriteLine();
			Console.WriteLine(resourceManager.GetString("NDoc3Usage1", currentCulture));
			Console.WriteLine();
			Console.WriteLine(resourceManager.GetString("NDoc3Usage2", currentCulture));
			Console.WriteLine();
			Console.WriteLine(resourceManager.GetString("NDoc3Usage3", currentCulture));
			Console.WriteLine();
			Console.WriteLine(resourceManager.GetString("NDoc3Usage4", currentCulture));
			Console.WriteLine();
			Console.WriteLine();

			WriteHelpAvailableDocumenters();

			Console.WriteLine();
			Console.WriteLine(resourceManager.GetString("NamespaceSummarySyntax", currentCulture));

		}

		private static void WriteHelp(string[] args) {
			if (args.Length == 1) {
				WriteUsage();
				return;
			}

			if (args.Length > 1) {
				IDocumenterInfo info = InstalledDocumenters.GetDocumenter(args[1].Replace("_", " "));
				if (info == null) {
					WriteHelpAvailableDocumenters();
					return;
				}

				IDocumenterConfig documenterConfig = info.CreateConfig(project);
				if (args.Length == 2) {
					WriteHelpAvailableDocParameters(documenterConfig);
				} else {
					WriteHelpDocParameter(documenterConfig, args[2]);
				}
			}

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.Write(System.String)")]
		private static void WriteHelpAvailableDocumenters() {
			Console.WriteLine(resourceManager.GetString("AvailableDocumenters", currentCulture));
			ArrayList docs = InstalledDocumenters.Documenters;
			for (int i = 0; i < docs.Count; i++) {
				Console.WriteLine(((IDocumenterInfo)docs[i]).Name.Replace(" ", "_"));
			}
			Console.WriteLine();
		}

		private static void WriteHelpAvailableDocParameters(IDocumenterConfig documenterConfig) {
			Console.WriteLine(resourceManager.GetString("AvailableProperties", currentCulture), documenterConfig.DocumenterInfo.Name);
			foreach (PropertyInfo property in documenterConfig.GetProperties()) {
				if (!property.IsDefined(typeof(NonPersistedAttribute), true)) {
					Console.WriteLine("\t" + property.Name);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "documenterConfig")]
		private static void WriteHelpDocParameter(IDocumenterConfig documenterConfig, string propertyName) {
			PropertyInfo foundProperty = null;

			foreach (PropertyInfo property in documenterConfig.GetProperties()) {
				if (String.Compare(property.Name, propertyName, StringComparison.OrdinalIgnoreCase) == 0) {
					foundProperty = property;
					break;
				}
			}

			if (foundProperty == null) {
				Console.WriteLine(resourceManager.GetString("NotPropertyOfConfig"), propertyName, documenterConfig.DocumenterInfo.Name);
				Console.WriteLine();
				WriteHelpAvailableDocParameters(documenterConfig);
			} else {
				WriteHelpPropertyDetails(foundProperty);
			}
		}

		private static void WriteHelpPropertyDetails(PropertyInfo property) {
			Console.WriteLine(property.Name);
			Console.WriteLine();

			object[] descAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), true);
			if (descAttr.Length > 0) {
				Console.WriteLine(resourceManager.GetString("Description", currentCulture));
				Console.WriteLine(((System.ComponentModel.DescriptionAttribute)descAttr[0]).Description);
				Console.WriteLine();
			}

			if (property.PropertyType.IsSubclassOf(typeof(Enum))) {
				Console.WriteLine(resourceManager.GetString("PossibleValues", currentCulture));
				string[] enumValues = Enum.GetNames(property.PropertyType);
				foreach (string enumValue in enumValues) {
					Console.WriteLine(enumValue);
				}
				Console.WriteLine();
			}

			object[] defaultAttr = property.GetCustomAttributes(typeof(System.ComponentModel.DefaultValueAttribute), true);
			if (defaultAttr.Length > 0) {
				Console.WriteLine(resourceManager.GetString("DefaultValue", currentCulture));
				Console.WriteLine(((System.ComponentModel.DefaultValueAttribute)defaultAttr[0]).Value);
				Console.WriteLine();
			}
		}

		private static void WriteLogoBanner() {
			string productName;
			string informationalVersion;
			string configurationInformation = null;
			string copyrightInformation = null;
			string companyInformation = null;

			Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

			// get product name
			object[] productAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
			if (productAttributes.Length > 0) {
				AssemblyProductAttribute productAttribute = (AssemblyProductAttribute)productAttributes[0];
				productName = productAttribute.Product;
			} else {
				productName = assembly.GetName().Name;
			}

			// get informational version
			object[] informationalVersionAttributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
			if (informationalVersionAttributes.Length > 0) {
				AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)informationalVersionAttributes[0];
				informationalVersion = informationalVersionAttribute.InformationalVersion;
			} else {
				if (assembly.Location == null)
					throw new InvalidOperationException(resourceManager.GetString("FatalErrorAssemblyVersion", currentCulture));
				FileVersionInfo info = FileVersionInfo.GetVersionInfo(assembly.Location);
				informationalVersion = info.FileMajorPart + "." + info.FileMinorPart;
			}

			// get assembly version 
			Version assemblyVersion = assembly.GetName().Version;

			// determine build date using build number of assembly 
			// version (specified as number of days passed since 1/1/2000)
			DateTime buildDate = new DateTime(2000, 1, 1).AddDays(assemblyVersion.Build);

			// get configuration information
			object[] configurationAttributes = assembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
			if (configurationAttributes.Length > 0) {
				AssemblyConfigurationAttribute configurationAttribute = (AssemblyConfigurationAttribute)configurationAttributes[0];
				configurationInformation = configurationAttribute.Configuration;
			}

			// get copyright information
			object[] copyrightAttributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			if (copyrightAttributes.Length > 0) {
				AssemblyCopyrightAttribute copyrightAttribute = (AssemblyCopyrightAttribute)copyrightAttributes[0];
				copyrightInformation = copyrightAttribute.Copyright;
			}

			// get company information
			object[] companyAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
			if (companyAttributes.Length > 0) {
				AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute)companyAttributes[0];
				companyInformation = companyAttribute.Company;
			}

			StringBuilder logoBanner = new StringBuilder();

			logoBanner.AppendFormat(CultureInfo.InvariantCulture,
				"{0} {1} (Build {2}; {3}; {4})", productName,
				informationalVersion, assemblyVersion.ToString(4),
				configurationInformation, buildDate.ToShortDateString());
			logoBanner.Append(Environment.NewLine);

			// output copyright information
			if (!String.IsNullOrEmpty(copyrightInformation)) {
				logoBanner.Append(copyrightInformation);
				logoBanner.Append(Environment.NewLine);
			}

			// output company information
			if (!String.IsNullOrEmpty(companyInformation)) {
				logoBanner.Append(companyInformation);
				logoBanner.Append(Environment.NewLine);
			}

			Console.WriteLine(logoBanner.ToString());
		}

		private static DateTime lastStepDateTime;

		private static void DocBuildingStepHandler(object sender, ProgressArgs e) {
			// timing
			if (lastStepDateTime.Ticks > 0) {
				TimeSpan ts = DateTime.UtcNow - lastStepDateTime;
				Console.WriteLine(String.Format(CultureInfo.CurrentUICulture, resourceManager.GetString("LastStepTook", currentCulture), ts.TotalSeconds));
			}
			lastStepDateTime = DateTime.UtcNow;

			Console.WriteLine(e.Status);
		}

		private static void RecurseDir(string dirName, int maxDepth) {
			// If the max depth are zero, we don't recurse any directories
			if (0 == maxDepth) return;
			// File extensions to look for
			string[] extensions = { "*.dll", "*.exe" };
			// For each extension recurse through the directories
			foreach (string extension in extensions) {
				foreach (string file in Directory.GetFiles(dirName, extension)) {
					string docFile = Path.ChangeExtension(file, ".xml");
					// If a XML doc file exists for the found assembly add it to the assembly doc files collection
					if (File.Exists(docFile)) {
						project.AssemblySlashDocs.Add(new AssemblySlashDoc(file, docFile));
					} else {
						Console.WriteLine(resourceManager.GetString("NoXMLDocFound", currentCulture), file);
						AssemblySlashDoc assemblySlashDoc = new AssemblySlashDoc();
						assemblySlashDoc.Assembly.Path = file;
						project.AssemblySlashDocs.Add(assemblySlashDoc);
					}
				}
			}
			// Recurse through sub directories in the current directory
			foreach (string subDir in Directory.GetDirectories(dirName)) {
				RecurseDir(subDir, maxDepth - 1);
			}
		}

		private static string BuildExceptionText(Exception ex) {
			StringBuilder strBld = new StringBuilder();

			Exception tmpEx = ex;
			while (tmpEx != null) {
				strBld.AppendFormat("Error: {0}", tmpEx.GetType());
				strBld.Append(Environment.NewLine);
				strBld.Append(tmpEx.Message);
				strBld.Append(Environment.NewLine);
				tmpEx = tmpEx.InnerException;
			}
			strBld.Append(Environment.NewLine);

			ReflectionTypeLoadException rtle = ex as ReflectionTypeLoadException;
			if (rtle != null) {
				Hashtable fileLoadExceptions = new Hashtable();
				foreach (Exception loaderEx in rtle.LoaderExceptions) {
					FileLoadException fileLoadEx = loaderEx as FileLoadException;
					if (fileLoadEx != null) {
						if (!fileLoadExceptions.ContainsKey(fileLoadEx.FileName)) {
							fileLoadExceptions.Add(fileLoadEx.FileName, null);
							strBld.Append("Unable to load: " + fileLoadEx.FileName + Environment.NewLine);
						}
					}
					strBld.Append(loaderEx.Message + Environment.NewLine);
					strBld.Append(loaderEx.StackTrace + Environment.NewLine);
				}
			}

			strBld.Append(ex.ToString());

			return strBld.ToString().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
		}
	}
}
