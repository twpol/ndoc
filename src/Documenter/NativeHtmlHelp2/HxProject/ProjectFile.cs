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

namespace NDoc.Documenter.NativeHtmlHelp2.HxProject
{
	/// <summary>
	/// Summary description for HxProject.
	/// </summary>
	public class ProjectFile : HxFile
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="templateFile"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static ProjectFile CreateFrom( string templateFile, string name )
		{
			if ( !File.Exists( templateFile ) )
				throw new ArgumentException( "The source file does not exist" );
			
			XmlDocument doc = new XmlDocument();
			
			XmlReader reader = new XmlTextReader( templateFile );
			XmlValidatingReader validator = new XmlValidatingReader( reader );
			validator.ValidationType = ValidationType.None;
			validator.XmlResolver = null;

			doc.Load( validator );

			return new ProjectFile( name, doc.DocumentElement );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="node"></param>
		private ProjectFile( string name, XmlNode node ) : base( name, node )
		{

		}

		public override  string FileName{ get{ return Name + ".HxC"; } }


		public string Title
		{
			get{ return GetProperty( "@Title" ); }
			set{ SetProperty( "@Title", value ); }
		}
		public int LangId
		{
			get{ return int.Parse( GetProperty( "@LangId" ) ); }
			set{ SetProperty( "@LangId", value ); }
		}
		public string Copyright
		{
			get{ return GetProperty( "@Copyright" ); }
			set{ SetProperty( "@Copyright", value ); }
		}
		public string FileVersion
		{
			get{ return GetProperty( "@FileVersion" ); }
			set{ SetProperty( "@FileVersion", value ); }
		}
		public bool BuildSeperateIndexFile
		{
			get{ return GetProperty( "CompilerOptions/@CompileResult" ) == "HxiHxs"; }
			set{ SetProperty( "CompilerOptions/@CompileResult", value ? "HxiHxs" : "Hxs" ); }
		}

		public bool CreateFullTextIndex
		{
			get{ return GetProperty( "CompilerOptions/@CreateFullTextIndex" ) == "Yes"; }
			set{ SetProperty( "CompilerOptions/@CreateFullTextIndex", value ? "Yes" : "No" ); }
		}
		public string StopWordFile
		{
			get{ return GetProperty( "CompilerOptions/@StopWordFile" ); }
			set{ SetProperty( "CompilerOptions/@StopWordFile", value ); }
		}
		public string TOCFile
		{
			get{ return GetProperty( "TOCDef/@File" ); }
			set{ SetProperty( "TOCDef/@File", value ); }
		}
	}
}
