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
using System.Text;
using System.Diagnostics;

namespace NDoc.Documenter.NativeHtmlHelp2.HxProject
{

	/// <summary>
	/// Summary description for HxTOC.
	/// </summary>
	public class TOCFile : HxFile
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="templateFile"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static TOCFile CreateFrom( string templateFile, string name )
		{
			if ( !File.Exists( templateFile ) )
				throw new ArgumentException( "The source file does not exist" );

			XmlDocument doc = new XmlDocument();
			
			XmlReader reader = new XmlTextReader( templateFile );
			XmlValidatingReader validator = new XmlValidatingReader( reader );
			validator.ValidationType = ValidationType.None;
			validator.XmlResolver = null;

			doc.Load( validator );
			
			return new TOCFile( name, doc.DocumentElement );
		}

		private XmlTextWriter xmlWriter = null;
		private StringWriter stringWriter = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="node"></param>
		private TOCFile( string name, XmlNode node ) : base( name, node )
		{

		}

		public override string FileName{ get{ return Name + ".HxT"; } }

		public void Open()
		{
			Debug.Assert( xmlWriter == null );
			stringWriter = new StringWriter();
			xmlWriter = new XmlTextWriter( stringWriter ) ;
			xmlWriter.QuoteChar = '\'';

			xmlWriter.WriteStartElement( "tmp" );
		}

		public void OpenNode( string url )
		{
			xmlWriter.WriteStartElement( "HelpTOCNode" );
			xmlWriter.WriteAttributeString( "", "Url", "", url );
		}

		public void InsertNode( string url )
		{
			OpenNode( url );
			CloseNode();
		}

		public void CloseNode()
		{
			xmlWriter.WriteFullEndElement();
		}

		public void Close()
		{
			Debug.Assert( xmlWriter != null );

			xmlWriter.WriteEndElement();
			xmlWriter.Close();

			XmlDocument doc = new XmlDocument();
			doc.LoadXml( stringWriter.ToString() );

			foreach( XmlNode node in doc.DocumentElement.ChildNodes )			
				dataNode.AppendChild( dataNode.OwnerDocument.ImportNode( node, true ) );
			
			xmlWriter = null;
			stringWriter = null;
		}
	}
}
