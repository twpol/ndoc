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
using System.Collections;
using System.Xml;
using System.Xml.Xsl;
using System.Diagnostics;

namespace NDoc.Documenter.NativeHtmlHelp2.Engine
{
	/// <summary>
	/// The collection of xslt stylesheets used to generate the Html
	/// </summary>
	public class StyleSheetCollection : DictionaryBase
	{
		/// <summary>
		/// Load the predefined set of xslt stylesheets into a dictionary
		/// </summary>
		/// <param name="extensibilityStylesheet">Path to an xslt stylesheet used for displaying custom tags</param>
		/// <param name="resourceDirectory">The location of the xsl files</param>
		/// <returns>The populated collection</returns>
		public static StyleSheetCollection LoadStyleSheets( string extensibilityStylesheet, string resourceDirectory )
		{
			XmlDocument common = new XmlDocument();
			common.Load( Path.Combine( Path.Combine( resourceDirectory, "xslt" ), "tags.xslt" ) );
			
			XmlElement include = common.CreateElement( "xsl", "include", "http://www.w3.org/1999/XSL/Transform" );

			if ( !Path.IsPathRooted( extensibilityStylesheet ) )
				extensibilityStylesheet = Path.GetFullPath( extensibilityStylesheet );

			include.SetAttribute( "href", extensibilityStylesheet );

			common.DocumentElement.PrependChild( include );

			common.Save( Path.Combine( Path.Combine( resourceDirectory, "xslt" ), "tags.xslt" ) );

			return LoadStyleSheets( resourceDirectory );
		}

		/// <summary>
		/// Load the predefined set of xslt stylesheets into a dictionary
		/// </summary>
		/// <param name="resourceDirectory">The location of the xsl files</param>
		/// <returns>The populated collection</returns>
		public static StyleSheetCollection LoadStyleSheets( string resourceDirectory )
		{
			StyleSheetCollection stylesheets = new StyleSheetCollection();

			Trace.Indent();

			stylesheets.AddFrom( "namespace", resourceDirectory );
			stylesheets.AddFrom( "namespacehierarchy", resourceDirectory );
			stylesheets.AddFrom( "type", resourceDirectory );
			stylesheets.AddFrom( "allmembers", resourceDirectory );
			stylesheets.AddFrom( "individualmembers", resourceDirectory );
			stylesheets.AddFrom( "event", resourceDirectory );
			stylesheets.AddFrom( "member", resourceDirectory );
			stylesheets.AddFrom( "memberoverload", resourceDirectory );
			stylesheets.AddFrom( "property", resourceDirectory );
			stylesheets.AddFrom( "field", resourceDirectory );

			Trace.Unindent();

			return stylesheets;
		}


		private StyleSheetCollection( )
		{

		}

		/// <summary>
		/// Return a named stylesheet from the collection
		/// </summary>
		public XslTransform this[ string name ]
		{
			get
			{
				Debug.Assert( base.InnerHashtable.Contains( name ) );
				return (XslTransform)base.InnerHashtable[name];
			}
		}

		private void AddFrom( string name, string resourceDirectory )
		{
			base.InnerHashtable.Add( name, MakeTransform( resourceDirectory, name ) );
		}

		private static XslTransform MakeTransform( string resourceDirectory, string name )
		{
			try
			{
				Trace.WriteLine( name + ".xslt" );
				XslTransform transform = new XslTransform();
				transform.Load( Path.Combine( Path.Combine( resourceDirectory, "xslt" ), name + ".xslt" ) );
				return transform;
			}
			catch ( Exception e )
			{
				throw new Exception( string.Format(	"Error compiling the {0} stylesheet", name ), e );
			}
		}
	}
}
