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
using System.Text;
using System.Diagnostics;

namespace NDoc.Documenter.NativeHtmlHelp2.Compiler
{
	/// <summary>
	/// Wraps the HxReg.exe registry component
	/// </summary>
	public class HxReg : HxObject
	{
		/*
			Microsoft Help Help File Registration Tool Version 2.1.9466
			Copyright (c) 1999-2000 Microsoft Corp.

			Usage: HxReg [switches] | HxReg <Help filename .HxS>
				-n <namespace>
				-i <title ID>
				-c <collection Name .HxC | .HxS>
				-d <namespace description>
				-s <Help filename .HxS>
				-x <Help Index filename .HxI>
				-q <Help Collection Combined FTS filename .HxQ>
				-t <Help Collection Combined Attribute Index filename .HxR>
				-l <language ID>
				-a <alias>
				-f <filename listing HxReg commands>
				-r Remove a namespace, Help title, or alias

			EXAMPLES
			To register a namespace:
				HxReg -n <namespace> -c <collection filename> -d <namespace description>
			To register a Help file:
				HxReg -n <namespace> -i <title id> -s <HxS filename>		  
		*/

		/// <summary>
		/// Create a new instance of a HxReg object
		/// </summary>
		/// <param name="compilerPath"><see cref="HxObject.CompilerPath"/></param>
		public HxReg( string compilerPath ) : base( compilerPath, "HxReg.exe" )
		{
		}

		/// <summary>
		/// Registers the a help namespace
		/// </summary>
		/// <param name="Namespace">The ID of the namespace to register</param>
		/// <param name="CollectionFile">The collection (HxS or HxC) file</param>
		/// <param name="Description">Namespace description</param>
		public void RegisterNamespace( string Namespace, FileInfo CollectionFile, string Description )
		{
			Debug.Assert( CollectionFile != null );
			Debug.Assert( CollectionFile.Exists );

			StringBuilder sb = new StringBuilder();
			sb.AppendFormat( " -n {0} ", Namespace );
			
			sb.Append( " -c " );
			sb.Append( '"' );
			sb.Append( CollectionFile.FullName );
			sb.Append( '"' );

			sb.AppendFormat( " -d " );
			sb.Append( '"' );
			sb.AppendFormat( Description );
			sb.Append( '"' );

			Execute( sb.ToString(), CollectionFile.Directory.FullName );
		}

		/// <summary>
		/// Register an HxS title with a help namespace
		/// </summary>
		/// <param name="Namespace">The namespace to register with</param>
		/// <param name="TitleID">The id of the new title</param>
		/// <param name="HxsFile">The location of the HxS file</param>
		public void RegisterTitle( string Namespace, string TitleID, FileInfo HxsFile )
		{
			Debug.Assert( HxsFile != null );
			Debug.Assert( HxsFile.Exists );
 
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat( " -n {0} ", Namespace );
			sb.AppendFormat( " -i {0} ", TitleID );

			sb.Append( '"' );
			sb.Append( HxsFile.FullName );
			sb.Append( '"' );

			Execute( sb.ToString(), HxsFile.Directory.FullName );
		}
	}
}
