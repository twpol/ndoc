// Console.cs - a console application for NDoc
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
using System.Diagnostics;
using System.Collections;
using System.Xml;
using System.IO;
using NDoc.Core;
using NDoc.Documenter.Msdn;
using NDoc.Documenter.Xml;

namespace NDoc.ConsoleApplication
{
	class EntryPoint
	{
		private static Project project;
		private static IDocumenter documenter;

		public static void Main(string[] args)
		{
			try
			{
				project = new Project();
				documenter = project.GetDocumenter("MSDN");
				int maxDepth = 20; //to limit recursion depth
				bool propertiesSet = false;
				bool projectSet = false;

				foreach (string arg in args)
				{
					if (arg.StartsWith("-"))
					{
						if (string.Compare(arg, "-verbose", true) == 0)
						{
							Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
						}
						else
						{
							string[] pair = arg.Split('=');

							if (pair.Length == 2)
							{
								string name = pair[0].Substring(1);
								string val = pair[1];

								switch (name.ToLower())
								{
									case "documenter":
										if (propertiesSet)
										{
											throw new ApplicationException("The documenter name must be specified before any documenter specific options.");
										}
										if (projectSet)
										{
											throw new ApplicationException("The documenter name must be specified before the project file.");
										}
										documenter = project.GetDocumenter(val);
										if (documenter == null)
										{
											throw new ApplicationException("The specified documenter name is invalid.");
										}
										break;
									case "project":
										if (propertiesSet)
										{
											throw new ApplicationException("The project file must be specified before any documenter specific options.");
										}
										project = new Project();
										project.Read(val);
										documenter = project.GetDocumenter(documenter.Name);
										projectSet = true;
										break;
									case "recurse":
										string[] recPair = val.Split(',');
										if (2 == recPair.Length)
										{
											maxDepth = Convert.ToInt32(recPair[1]);
										}
										RecurseDir(recPair[0], maxDepth);
										break;
									case "namespacesummaries":
										using(StreamReader streamReader = new StreamReader(val))
										{
											XmlTextReader reader = new XmlTextReader(streamReader);
											reader.MoveToContent();
											project.ReadNamespaceSummaries(reader);
											reader.Close();
											streamReader.Close();
										}
										break;
									default:
										documenter.Config.SetValue(name, val);
										propertiesSet = true;
										break;
								}
							}
						}
					}
					else if (arg.IndexOf(',') != -1)
					{
						string[] pair = arg.Split(',');

						if (pair.Length == 2)
						{
							project.AddAssemblySlashDoc(
								new AssemblySlashDoc(pair[0], pair[1]));
						}
					}
				}

				if (project.AssemblySlashDocCount == 0)
				{
					WriteUsage();
				}
				else
				{
					documenter.DocBuildingStep += new DocBuildingEventHandler(DocBuildingStepHandler);
					documenter.Build(project);
				}
			}
			catch( Exception except )
			{
				Console.WriteLine( "Error: " + except.Message );
				System.Diagnostics.Trace.WriteLine( "Exception: " + Environment.NewLine + except.ToString() );
			}
		}

		private static void WriteUsage()
		{
			Console.WriteLine();
			Console.WriteLine("usage: NDocConsole  [-verbose] [-documenter=docname]");
			Console.WriteLine("                    [-recurse=dir[,maxDepth]] [-property=val...]");
			Console.WriteLine("                    assembly,xml [assembly,xml...]");
			Console.WriteLine("                    [-namespacesummaries=filename]");
			Console.WriteLine();
			Console.WriteLine("or     NDocConsole  [-verbose] [-documenter=docname] [-project=file]");
			Console.WriteLine("                    [-recurse=dir[,maxDepth]] [-property=val...]");
			Console.WriteLine();

			Console.Write("available documenters: ");
			ArrayList docs = project.Documenters;
			for (int i = 0; i < docs.Count; i++)
			{
				if (i > 0) Console.Write(", ");
				Console.Write(((IDocumenter)docs[i]).Name);
			}
			Console.WriteLine();
			Console.WriteLine();

			Console.WriteLine("available properties with the {0} documenter:", documenter.Name);
			foreach (string property in documenter.Config.GetProperties())
			{
				Console.WriteLine("    " + property);
			}
			Console.WriteLine();

			Console.WriteLine(@"namespace summaries file syntax:
    <namespaces>
        <namespace name=""My.NameSpace"">My summary.</namespace>
        ...
    </namespaces>");

		}

		private static void DocBuildingStepHandler(object sender, ProgressArgs e)
		{
			Console.WriteLine( e.Status );
		}

		private static void RecurseDir(string dirName, int maxDepth)
		{
			if (0 == maxDepth) return;
			string docFile;
			string[] extensions = {"*.dll", "*.exe"};
			foreach (string extension in extensions)
			{
				foreach (string file in System.IO.Directory.GetFiles(dirName, extension))
				{
					docFile = Path.ChangeExtension(file, ".xml");
					if (System.IO.File.Exists(docFile))
					{
						project.AddAssemblySlashDoc(new AssemblySlashDoc(file, docFile));
					}
				}
			}
			foreach (string subDir in System.IO.Directory.GetDirectories(dirName))
			{
				RecurseDir(subDir, maxDepth - 1);
			}
		}
	}
}
