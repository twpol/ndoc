// MsdnDocumenter.cs - a MSDN-like documenter
// Copyright (C) 2003 Don Kackman
// Parts copyright 2001  Kral Ferch, Jason Diamond
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
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace NDoc.Documenter.NativeHtmlHelp2
{
	/// <summary>
	/// Summary description for Workspace.
	/// </summary>
	public class Workspace
	{

		/// <summary>
		/// The location of the workspace and files
		/// </summary>
		public readonly string RootDirectory = string.Empty;

		/// <summary>
		/// Contructs a new instance of the Workspace class
		/// </summary>
		/// <param name="rootDir">The location to create the workspace</param>
		public Workspace( string rootDir )
		{
			if ( !Path.IsPathRooted( rootDir ) )
				throw new ArgumentException( "A relative path cannot be used for a worksapce" );

			RootDirectory = rootDir;
			if ( !Directory.Exists( rootDir ) )
				Directory.CreateDirectory( rootDir );

			if ( !Directory.Exists( ContentDirectory ) )
				Directory.CreateDirectory( ContentDirectory );
		}

		/// <summary>
		/// The location of the help contents source files
		/// </summary>
		public string ContentDirectory
		{
			get 
			{
				return Path.Combine( RootDirectory, "html" );
			}
		}

		/// <summary>
		/// Copies project resources into the workspace
		/// </summary>
		/// <param name="sourceDirectory">The path to the resources</param>
		public void ImportProjectFiles( string sourceDirectory )
		{
			ImportProjectFiles( sourceDirectory, "*.*" );
		}

		/// <summary>
		/// Copies project resources into the workspace
		/// </summary>
		/// <param name="sourceDirectory">The path to the resources</param>
		/// <param name="filer">File filter to use when selecting files to import</param>
		public void ImportProjectFiles( string sourceDirectory, string filter )
		{
			if ( !Directory.Exists( sourceDirectory ) )
				throw new ArgumentException( "The source location does not exist" );

			DirectoryInfo dir = new DirectoryInfo( sourceDirectory );
			foreach( FileInfo file in dir.GetFiles( filter ) )
				file.CopyTo( Path.Combine( RootDirectory, file.Name ), true );
		}

		/// <summary>
		/// Copies content into the workspace ContentDirectory
		/// </summary>
		/// <param name="sourceDirectory">The path to the xontent files</param>
		public void ImportContent( string sourceDirectory )
		{
			if ( !Directory.Exists( sourceDirectory ) )
				throw new ArgumentException( "The source location does not exist" );

			DirectoryInfo dir = new DirectoryInfo( sourceDirectory );
			foreach( FileInfo file in dir.GetFiles( "*.*" ) )
				file.CopyTo( Path.Combine( ContentDirectory, file.Name ), true );
		}

		/// <summary>
		/// Delets all output and intermediate files from the project workspace
		/// This will delte all files and folder in RootDirectory
		/// </summary>
		public void Clean()
		{
			CleanDirectory( new DirectoryInfo( RootDirectory ) );
		}

		private void CleanDirectory( DirectoryInfo dir )
		{
			//clean-up output path
			foreach ( FileInfo file in dir.GetFiles( "*.*" ) )
			{
				try
				{
					file.Delete();
				}
				catch (UnauthorizedAccessException)
				{
					Trace.WriteLine("Could not delete " + file 
						+ " from the output directory.  It might be read-only.");
				}
				catch ( IOException )
				{
					Trace.WriteLine("Could not delete " + file 
						+ " from the output directory because it is in use.");
				}
			}

			foreach ( DirectoryInfo child in dir.GetDirectories() )
				CleanDirectory( child );
		}
	}
}
