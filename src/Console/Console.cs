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

using NDoc.Core;
using NDoc.Documenter.Msdn;
using NDoc.Documenter.Xml;

namespace NDoc.Console
{
	class EntryPoint
	{
		static void Main(string[] args)
		{
			try
			{
				MsdnDocumenter documenter = new MsdnDocumenter();

				if (args.Length < 1)
				{
					WriteUsage(documenter);
				}
				else
				{
					Project project = new Project();
					int maxDepth = 20; //to limit recursion depth

					foreach (string arg in args)
					{
						if (arg.StartsWith("-"))
						{
							if (string.Compare(arg, "-verbose", true) == 0)
							{
								Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
							}
							else
							{
								string[] pair = arg.Split('=');

								if (pair.Length == 2)
								{
									string name = pair[0].Substring(1);
									string value = pair[1];

									switch (name.ToLower())
									{
										case "project":
											project = new Project();
											project.Read(value);
											break;
										case "recurse":
											string[] recPair = value.Split(',');
											if (2 == recPair.Length)
											{
												maxDepth = Convert.ToInt32(recPair[1]);
											}
											RecurseDir(ref project, recPair[0], maxDepth);
											break;
										default:
											documenter.Config.SetValue(name, value);
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
						WriteUsage(documenter);
					}
					else
					{
						documenter.DocBuildingStep += new DocBuildingEventHandler(DocBuildingStepHandler);
						documenter.Build(project);
					}
				}
			}
			catch( Exception except )
			{
				System.Console.WriteLine( "Error: " + except.Message );
				System.Diagnostics.Trace.WriteLine( "Exception: " + Environment.NewLine + except.ToString() );
			}
		}

		private static void WriteUsage(IDocumenter documenter)
		{
			System.Console.WriteLine("usage: NDoc.Console [-verbose] [-project=file] [-recurse=dir[,maxDepth]] [-property=value...] assembly,xml [assembly,xml...]");
			System.Console.WriteLine("  available properties:");

			foreach (string property in documenter.Config.GetProperties())
			{
				System.Console.WriteLine("    " + property);
			}
		}

		private static void DocBuildingStepHandler(object sender, ProgressArgs e)
		{
			System.Console.WriteLine( e.Status );
		}

		private static void RecurseDir(ref Project project, string dirName, int maxDepth)
		{
			if (0 == maxDepth) return;
			string docFile;
			string[] extensions = {"*.dll", "*.exe"};
			foreach (string extension in extensions)
			{
				foreach (string file in System.IO.Directory.GetFiles(dirName, extension))
				{
					docFile = System.IO.Path.GetDirectoryName(file) + "\\" + System.IO.Path.GetFileNameWithoutExtension(file) + ".xml";
					if (System.IO.File.Exists(docFile))
					{
						project.AddAssemblySlashDoc(new AssemblySlashDoc(file, docFile));
					}
				}
			}
			foreach (string subDir in System.IO.Directory.GetDirectories(dirName))
			{
				RecurseDir(ref project, subDir, maxDepth - 1);
			}
		}
	}
}
