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
	/// Handler for content directory events
	/// </summary>
	public delegate void ContentDirectoryEventHandler( string path );

	/// <summary>
	/// Summary description for Workspace.
	/// </summary>
	public class Workspace
	{
		/// <summary>
		/// Event raised when a content directory is added
		/// </summary>
		public event ContentDirectoryEventHandler ContentDirectoryAdded;

		/// <summary>
		/// The name of the directory where the html file are created
		/// </summary>
		public readonly static string ContentDirectoryName = "html";

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
			if ( !Directory.Exists( RootDirectory ) )
				Directory.CreateDirectory( RootDirectory );

			if ( !Directory.Exists( WorkingDirectory ) )
				Directory.CreateDirectory( WorkingDirectory );

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
				return Path.Combine( WorkingDirectory, ContentDirectoryName );
			}
		}

		/// <summary>
		/// The directory where the compilation will run
		/// </summary>
		public string WorkingDirectory
		{
			get
			{
				return Path.Combine( RootDirectory, "temp" );
			}
		}

		/// <summary>
		/// Saves files mathing the specified filter from the working directory to the root directory
		/// </summary>
		/// <param name="filter">File filter to search for</param>
		public void SaveOutputs( string filter )
		{
			DirectoryInfo dir = new DirectoryInfo( WorkingDirectory );
			foreach ( FileInfo f in dir.GetFiles( filter ) )
				f.MoveTo( Path.Combine( this.RootDirectory, f.Name ) );
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
		/// <param name="filter">File filter to use when selecting files to import</param>
		public void ImportProjectFiles( string sourceDirectory, string filter )
		{
			if ( !Directory.Exists( sourceDirectory ) )
				throw new ArgumentException( string.Format( "The source location {0} does not exist", sourceDirectory ) );

			DirectoryInfo dir = new DirectoryInfo( sourceDirectory );
			foreach( FileInfo file in dir.GetFiles( filter ) )
				file.CopyTo( Path.Combine( WorkingDirectory, file.Name ), true );
		}

		/// <summary>
		/// Recursively copies the contents of sourceDirectory into the workspace,
		/// maintainng the same directory structure
		/// </summary>
		/// <param name="sourceDirectory">The directory to import</param>
		public void ImportContentDirectory( string sourceDirectory )
		{
			if ( !Directory.Exists( sourceDirectory ) )
				throw new ArgumentException( string.Format( "The source location {0} does not exist", sourceDirectory ) );

			ImportDirectory( new DirectoryInfo( sourceDirectory ), new DirectoryInfo( this.ContentDirectory ) );
		}

		/// <summary>
		/// Raises the <see cref="Workspace.ContentDirectoryAdded"/> event
		/// </summary>
		/// <param name="relativePath">Path relative to the workspace root</param>
		protected void OnContentDirectoryAdded( string relativePath )
		{
			if ( ContentDirectoryAdded != null )
				ContentDirectoryAdded( relativePath );
		}

		private void ImportDirectory( DirectoryInfo sourceDir, DirectoryInfo targetParent )
		{
			DirectoryInfo targetDir = new DirectoryInfo( Path.Combine( targetParent.FullName, sourceDir.Name ) );;

			if ( !targetDir.Exists )
				targetDir.Create();

			OnContentDirectoryAdded( GetRelativePath( new DirectoryInfo( this.WorkingDirectory ), targetDir ) );

			foreach( FileInfo f in sourceDir.GetFiles() )
				f.CopyTo( Path.Combine( targetDir.FullName, f.Name ) );

			foreach( DirectoryInfo childDir in sourceDir.GetDirectories() )
				ImportDirectory( childDir, targetDir );
		}

		private static string GetRelativePath( DirectoryInfo ancestor, DirectoryInfo child )
		{	
			if ( ancestor.Root.FullName != child.Root.FullName )
				return "";

			string tmp = child.FullName.Replace( ancestor.FullName, "" );

			// strip off the leading backslash if present
			if ( tmp.IndexOf( @"\", 0 ) == 0 )
				tmp = tmp.Substring( 1 );

			return tmp;
		}

		/// <summary>
		/// Copies content into the workspace ContentDirectory
		/// </summary>
		/// <param name="sourceDirectory">The path to the content files</param>
		public void ImportContent( string sourceDirectory )
		{
			ImportContent( sourceDirectory, "*.*" );
		}

		/// <summary>
		/// Copies content into the workspace ContentDirectory
		/// </summary>
		/// <param name="sourceDirectory">The path to the xontent files</param>
		/// <param name="filter">File filter to use when selecting files to import</param>
		public void ImportContent( string sourceDirectory, string filter )
		{
			if ( !Directory.Exists( sourceDirectory ) )
				throw new ArgumentException( string.Format( "The source location {0} does not exist", sourceDirectory ) );

			DirectoryInfo dir = new DirectoryInfo( sourceDirectory );
			foreach( FileInfo file in dir.GetFiles( filter ) )
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
				catch ( UnauthorizedAccessException )
				{
					Trace.WriteLine("Could not delete " + file + " from the output directory.  It might be read-only.");
				}
				catch ( IOException )
				{
					Trace.WriteLine("Could not delete " + file + " from the output directory because it is in use.");
				}
			}

			foreach ( DirectoryInfo child in dir.GetDirectories() )
				CleanDirectory( child );
		}
	}
}
