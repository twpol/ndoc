// Copyright (C) 2004  Kevin Downs
// Parts Copyright (c) 2002 Jean-Claude Manoli
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
using System.IO;
using System.Collections;
using System.Globalization;
using System.Xml;
using System.Reflection;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NDoc.Core
{
	/// <summary>
	/// Caches XML summaries.
	/// </summary>
	public class ExternalXmlSummaryCache
	{
		private Hashtable cachedDocs;
		private Hashtable summaries;
		private string localizationLanguage;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExternalXmlSummaryCache" />
		/// class.
		/// </summary>
		public ExternalXmlSummaryCache(string localizationLanguage)
		{
			Flush();
			this.localizationLanguage = localizationLanguage;
		}

		/// <summary>
		/// Flushes the <see cref="ExternalXmlSummaryCache" />.
		/// </summary>
		public void Flush()
		{
			cachedDocs = new Hashtable();
			summaries = new Hashtable();
		}

		/// <summary>
		/// Adds given XML document to the summary cache.
		/// </summary>
		/// <param name="xmlFileName">The filename of XML document to cache.</param>
		/// <param name="assemblyName">The fullname of the assembly to which to XML document refers.</param>
		public void AddXmlDoc(string xmlFileName, string assemblyName)
		{
			int start = Environment.TickCount;

			XmlTextReader reader = null;
			try
			{
				reader = new XmlTextReader(xmlFileName);
				CacheSummaries(reader);
			}
			finally
			{
				if (reader != null) reader.Close();
			}
			cachedDocs.Add(assemblyName, xmlFileName);
			Trace.WriteLine("Cached doc : " + ((Environment.TickCount - start) / 1000.0).ToString() + " sec.");
		}

		/// <summary>
		/// Adds given XML document to the summary cache.
		/// </summary>
		/// <param name="xmlFileName">The filename of XML document to cache.</param>
		public void AddXmlDoc(string xmlFileName)
		{
			AddXmlDoc(xmlFileName, "");
		}

		/// <summary>
		/// Gets the xml documentation for the assembly of the specified type.
		/// </summary>
		public void GetXmlFor(Type type)
		{
			string searchedDoc = (string)cachedDocs[type.Assembly.FullName];

			if (searchedDoc == null)
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

#if (!MONO)
						//TODO: search in the mono lib folder, if they ever give us the xml documentation
						// If still not found, try locating the assembly in the Framework folder
						if (docPath == null)
						{
#if (NET_1_0)
							FileVersionInfo version = FileVersionInfo.GetVersionInfo(assemblyPath);
							string stringVersion = string.Format(
								"v{0}.{1}.{2}", 
								version.FileMajorPart, 
								version.FileMinorPart, 
								version.FileBuildPart);
							string frameworkPath = this.GetDotnetFrameworkPath(stringVersion);
#else
							string frameworkPath = this.GetDotnetFrameworkPath(a.ImageRuntimeVersion);
#endif
							if (frameworkPath != null)
							{
								string localizedFrameworkPath = Path.Combine(frameworkPath, localizationLanguage);
								if (Directory.Exists(localizedFrameworkPath))
								{
									docPath = Path.Combine(localizedFrameworkPath, a.GetName().Name + ".xml");
								}
								if ((docPath == null) || (!File.Exists(docPath)))
								{
									docPath = Path.Combine(frameworkPath, a.GetName().Name + ".xml");
								}
							}
						}
#endif

						if ((docPath != null) && (File.Exists(docPath)))
						{
							Debug.WriteLine("Loading XML Doc for " + type.Assembly.FullName);
							Debug.WriteLine("at " + docPath);
							AddXmlDoc(docPath, type.Assembly.FullName);
							searchedDoc = docPath;
						}
					}
				}


				//if the doc was still not found, create an empty document filename
				if (searchedDoc == null)
				{
					Trace.WriteLine("XML Doc not found for " + type.Name);
					searchedDoc = "";
					//cache the document path
					cachedDocs.Add(type.Assembly.FullName, searchedDoc);
				}

			}
		}

		/// <summary>
		/// Caches summaries for all members in XML documentation file.
		/// </summary>
		/// <param name="reader">XmlTextReader for XML Documentation</param>
		/// <remarks>If a member does not have a summary, a zero-length string is stored instead.</remarks>
		private void CacheSummaries(XmlTextReader reader)
		{
			object oMember = reader.NameTable.Add("member");
			object oSummary = reader.NameTable.Add("summary");

			reader.MoveToContent();

			string MemberID = "";
			string Summary = "";
			while (reader.Read()) 
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element : 

						if (reader.Name.Equals(oMember)) 
						{
							MemberID = reader.GetAttribute("name");
							Summary = "";
						}      
						if (reader.Name.Equals(oSummary)) 
						{
							Summary = reader.ReadInnerXml();
							Summary = Summary.Replace("\t", " ").Replace("\n", " ").Replace("\r", " ").Trim().Replace("        ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ").Trim();
						}
						break;

					case XmlNodeType.EndElement : 
 
						if (reader.Name.Equals(oMember)) 
						{
							if (!summaries.ContainsKey(MemberID))
							{
								summaries.Add(MemberID, Summary);
							}
						}
						break;

					default : 
						break;
				}
			}
		}

		/// <summary>
		/// Returns the original summary for a member inherited from a specified type. 
		/// </summary>
		/// <param name="memberID">The member ID to lookup.</param>
		/// <param name="declaringType">The type that declares that member.</param>
		/// <returns>The summary xml.  If not found, returns an zero length string.</returns>
		public string GetSummary(string memberID, Type declaringType)
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
			string key = memberType + declaringType.FullName.Replace("+", ".") + "." + memberName;

			//check the summaries cache first
			string summary = (string)summaries[key];

			if (summary == null)
			{
				//lookup the xml document
				GetXmlFor(declaringType);

				//the summary should now be cached (if it exists!)
				//so lets have another go at getting it...
				summary = (string)summaries[key];

				//if no summary was not found, create an blank one
				if (summary == null)
				{
					//Debug.WriteLine("#NoSummary#\t" + key); 
					summary = "";
					//cache the blank so we don't search for it again
					summaries.Add(key, summary);
				}

			}
			return "<summary>" + summary + "</summary>";
		}

		private string GetDotnetFrameworkPath(string version)
		{
			using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework"))
			{
				if (regKey == null)
					return null;

				string installRoot = regKey.GetValue("InstallRoot") as string;

				if (installRoot == null)
					return null;

				return Path.Combine(installRoot, version);
			}
		}
	}
}
