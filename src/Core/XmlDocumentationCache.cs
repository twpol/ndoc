#region Copyright © 2002 Jean-Claude Manoli
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
 * 
 * mailto:jc@manoli.net
 */ 
#endregion

using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;

namespace NDoc.Core
{
	/// <summary>
	/// Caches XML Documentation files and summaries.
	/// </summary>
	public class XmlDocumentationCache
	{
		private Hashtable docs;
		private Hashtable summaries;

		/// <summary>
		/// Initializes a new instance of the XmlDocumentationCache class.
		/// </summary>
		public XmlDocumentationCache()
		{
			docs = new Hashtable();
			summaries = new Hashtable();
		}

		/// <summary>
		/// Gets the xml documentation for the assembly of the specified type.
		/// </summary>
		/// <value>The xml document.  If the xml file was not found, returns an empty document.</value>
		public XmlDocument this[Type type]
		{
			get
			{
				XmlDocument doc = (XmlDocument)docs[type.Assembly];

				if (doc == null)
				{
					Type t = Type.GetType(type.AssemblyQualifiedName);
					if (t != null)
					{
						Assembly a = t.Assembly;
						string assemblyPath = a.Location;
						
						if (assemblyPath.Length > 0)
						{
							string docPath = Path.ChangeExtension(assemblyPath, ".xml");

							//if not found, try loading __AssemblyInfo__.ini
							if (!File.Exists(docPath))
							{
								string infoPath = Path.Combine(
									Path.GetDirectoryName(docPath), "__AssemblyInfo__.ini");
								docPath = null;

								if (File.Exists(infoPath))
								{
									Debug.WriteLine("Loading __AssemblyInfo__.ini.");
									TextReader reader = new StreamReader(infoPath);
									string line;
									try
									{
										while ((line = reader.ReadLine()) != null)
										{
											if (line.StartsWith("URL=file:///"))
											{
												docPath = Path.ChangeExtension(line.Substring(12), ".xml");
												break;
											}
										}
									}
									finally
									{
										reader.Close();
									}
								}
							}

#if !MONO //TODO: search in the mono lib folder, if they ever give us the xml documentation
							// If still not found, try locating the assembly in the Framework folder
							if (docPath == null )
							{
								string frameworkPath = this.GetDotnetFrameworkPath(FileVersionInfo.GetVersionInfo(assemblyPath));

								if (frameworkPath != null)
								{
									docPath = Path.Combine(frameworkPath, a.GetName().Name + ".xml");
								}
							}
#endif

							if ((docPath != null) && (File.Exists(docPath)))
							{
								Debug.WriteLine("Loading XML Doc for " + type.Assembly.FullName);
								doc = new XmlDocument();
								doc.Load(docPath);
							}
						}
					}

					//if the doc was still not found, create an empty document
					if (doc == null)
					{
						Debug.WriteLine("XML Doc not found for " + type.Name);
						doc = new XmlDocument();
					}

					//cache the document
					docs.Add(type.Assembly, doc);
				}

				return doc;
			}
		}

		/// <summary>
		/// Returns the original summary for a member inherited from a specified type. 
		/// </summary>
		/// <param name="memberID">The member ID to lookup.</param>
		/// <param name="declaringType">The type that declares that member.</param>
		/// <returns>The summary xml node.  If not found, returns an empty node.</returns>
		public XmlNode GetSummary(string memberID, Type declaringType)
		{
			//extract member type (T:, P:, etc.)
			string memberType = memberID.Substring(0, 2);

			//extract member name
			int i = memberID.IndexOf('(');
			string memberName;
			if (i > -1)
			{
				memberName = memberID.Substring(memberID.LastIndexOf('.', i) + 1);
			}
			else
			{
				memberName = memberID.Substring(memberID.LastIndexOf('.') + 1);
			}

			//the member id in the declaring assembly
			string key = memberType + declaringType.FullName + "." + memberName;

			//check the summaries cache first
			XmlNode summary = (XmlNode)summaries[key];

			if (summary == null)
			{
				//lookup the xml document
				XmlDocument doc = this[declaringType];
				if (doc.HasChildNodes)
				{
					string xPathExpr = "/doc/members/member[@name=\"" + key + "\"]/summary";
					summary = doc.SelectSingleNode(xPathExpr);
				}

				//if the node was not found, create an empty one
				if (summary == null)
				{
					//Debug.WriteLine("Summary node not found for " + key);
					summary = new XmlDocument();
				}

				//cache the node
				summaries.Add(key, summary);
			}
			return summary;
		}

		private string GetDotnetFrameworkPath(FileVersionInfo version)
		{
			using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework"))
			{
				if (regKey == null)
					return null;

				string installRoot = regKey.GetValue("InstallRoot") as string;

				if (installRoot == null)
					return null;

				string stringVersion = string.Format(
					"v{0}.{1}.{2}", 
					version.FileMajorPart, 
					version.FileMinorPart, 
					version.FileBuildPart);

				return Path.Combine(installRoot, stringVersion);
			}
		}
	}
}
