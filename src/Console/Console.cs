using System;

using NDoc.Core;
using NDoc.Documenter.Msdn;

namespace NDoc.Console
{
	class EntryPoint
	{
		static void Main(string[] args)
		{
			MsdnDocumenter documenter = new MsdnDocumenter();

			if (args.Length < 1)
			{
				WriteUsage(documenter);
			}
			else
			{
				Project project = new Project();

				foreach (string arg in args)
				{
					if (arg.StartsWith("-"))
					{
						string[] pair = arg.Split('=');

						if (pair.Length == 2)
						{
							string name = pair[0].Substring(1);
							string value = pair[1];

							documenter.Config.SetValue(name, value);
						}
					}
					else if (arg.IndexOf(',') != -1)
					{
						string[] pair = arg.Split(',');

						if (pair.Length == 2)
						{
							project.AssemblySlashDocs.Add(
								new AssemblySlashDoc(pair[0], pair[1]));
						}
					}
				}

				if (project.AssemblySlashDocs.Count == 0)
				{
					WriteUsage(documenter);
				}
				else
				{
					documenter.DocBuildingProgress += new DocBuildingEventHandler(Handler);
					documenter.Build(project);
				}
			}
		}

		private static void WriteUsage(IDocumenter documenter)
		{
			System.Console.WriteLine("usage: NDoc.Console [-property=value...] assembly,xml [assembly,xml...]");
			System.Console.WriteLine("  available properties:");

			foreach (string property in documenter.Config.GetProperties())
			{
				System.Console.WriteLine("    " + property);
			}
		}

		private static void Handler(object sender, ProgressArgs e)
		{
			System.Console.Write(".");
		}
	}
}
