using System;
using System.IO;
using System.Collections;

using NDoc.Core;

namespace NDoc.Gui
{
	/// <summary>
	/// Handles drap and drop operations
	/// </summary>
	public class DragDropHandler
	{
		/// <summary>
		/// Determines if the list of files is a list of assemblies
		/// </summary>
		/// <param name="files">File list</param>
		/// <returns>True if all the files are dll's or exe's</returns>
		public static bool CanDrop( string[] files )
		{
			foreach (string s in files)
			{
				string ext = Path.GetExtension( s ).ToLower();
				if ( ext != ".dll" && ext != ".exe" )
					return false;
			}
		
			return true;
		}

		/// <summary>
		/// Create a collection of <see cref="AssemblySlashDoc"/> objects
		/// </summary>
		/// <param name="files">An arrray of assembly files names</param>
		/// <returns>Populated collection</returns>
		public static ICollection HandleDrop( string[] files )
		{
			ArrayList assemblySlashDocs = new ArrayList();

			foreach (string s in files)
			{
				string ext = Path.GetExtension( s ).ToLower();
				if ( ext == ".dll" || ext == ".exe" )
				{
					string slashDocFile = FindDocFile( s );
					if ( slashDocFile.Length > 0 )
						assemblySlashDocs.Add( new AssemblySlashDoc( s, slashDocFile ) );
				}
			}
					
			return assemblySlashDocs;
		}

		private static string FindDocFile( string assemblyFile )
		{
			string slashDocFilename = assemblyFile.Substring(0, assemblyFile.Length-4) + ".xml";

			if (File.Exists(slashDocFilename))
				return slashDocFilename;

			return "";
		}
	}
}
