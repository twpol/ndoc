// StyleSheetCollection
// Copyright (C) 2004 Kevin Downs
// Parts Copyright (c) 2003 Don Kackman
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
using NDoc3.Core;
using NDoc3.Documenter.Msdn.xslt;

namespace NDoc3.Documenter.Msdn
{
	/// <summary>
	/// The collection of xslt stylesheets used to generate the Html
	/// </summary>
	internal class StyleSheetCollection : DictionaryBase
	{
		/// <summary>
		/// Load the predefined set of xslt stylesheets into a dictionary
		/// </summary>
		/// <param name="extensibilityStylesheet"></param>
		/// <returns>The populated collection</returns>
		public static StyleSheetCollection LoadStyleSheets( string extensibilityStylesheet )
		{
			StyleSheetCollection stylesheets = new StyleSheetCollection();

//			string resourceBase = "file://" + Path.GetFullPath(Path.Combine(System.Windows.Forms.Application.StartupPath, String.Format("..{0}..{0}..{0}Documenter{0}Msdn{0}xslt", Path.DirectorySeparatorChar)) );
//			string resourceBase = "NDoc3.Documenter.Msdn.xslt";
			string path = "..{0}..{0}..{0}Documenter{0}Msdn{0}xslt";

			string[] resourceDirs = {
											MakeAbsolutePath("Documenter{0}Msdn{0}xslt")
											,MakeAbsolutePath("..{0}..{0}..{0}Documenter{0}Msdn{0}xslt")
											,MakeAbsolutePath("..{0}..{0}..{0}src{0}Documenter{0}Msdn{0}xslt")
			                        };
            XsltResourceResolver resolver = new XsltResourceResolver(typeof(StyleSheetLocation), resourceDirs);
			resolver.ExtensibilityStylesheet=extensibilityStylesheet;
			Trace.Indent();

			stylesheets.AddFrom( "assembly", resolver );
			stylesheets.AddFrom( "namespace", resolver );
			stylesheets.AddFrom( "namespacehierarchy", resolver );
			stylesheets.AddFrom( "type", resolver );
			stylesheets.AddFrom( "typehierarchy", resolver );
			stylesheets.AddFrom( "allmembers", resolver );
			stylesheets.AddFrom( "individualmembers", resolver );
			stylesheets.AddFrom( "event", resolver );
			stylesheets.AddFrom( "member", resolver );
			stylesheets.AddFrom( "memberoverload", resolver );
			stylesheets.AddFrom( "property", resolver );
			stylesheets.AddFrom( "field", resolver );
			stylesheets.AddFrom( "htmlcontents", resolver );


			Trace.Unindent();

			return stylesheets;
		}

		private static string MakeAbsolutePath(string path)
		{
			return "file://" + Path.GetFullPath(Path.Combine(System.Windows.Forms.Application.StartupPath, String.Format(path, Path.DirectorySeparatorChar)) );
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
//		/// <summary>
//		/// Return a named stylesheet from the collection
//		/// </summary>
//		public XslCompiledTransform this[ string name ]
//		{
//			get
//			{
//				Debug.Assert( base.InnerHashtable.Contains( name ) );
//				return (XslCompiledTransform)base.InnerHashtable[name];
//			}
//		}

		private void AddFrom( string name, XsltResourceResolver resolver )
		{
			base.InnerHashtable.Add( name, MakeTransform( name, resolver ) );
		}

		private XslTransform MakeTransform( string name,  XmlResolver resolver)
		{
			try
			{
				Trace.WriteLine( string.Format("Compiling {0}.xslt", name) );
				XmlReader reader=(XmlReader)resolver.GetEntity(new Uri("res:" + name + ".xslt"),null,typeof(XmlReader));

				XslTransform transform = new XslTransform();
				transform.Load(reader, resolver);
				return transform;
			}
			catch ( XsltException e )
			{
				throw new Exception(string.Format("Error compiling the stylesheet '{0}': {1} at {2}:{3}", name, e.Message, e.LineNumber, e.LinePosition), e);
			}
		}

//		private XslCompiledTransform MakeCompiledTransform( string name,  XmlResolver resolver)
//		{
//			try
//			{
//				Trace.WriteLine( string.Format("Compiling {0}.xslt", name) );
//				XmlReader reader=(XmlReader)resolver.GetEntity(new Uri("res:" + name + ".xslt"),null,typeof(XmlReader));
//
////				transform.Load(reader, resolver, Assembly.GetExecutingAssembly().Evidence);
//				XsltSettings settings = new XsltSettings(false, true);
//				XslCompiledTransform transform = new XslCompiledTransform();
//				transform.Load(reader, settings, resolver);
//				return transform;
//			}
//			catch ( XsltException e )
//			{
//				throw new Exception(string.Format("Error compiling the stylesheet '{0}': {1} at {2}:{3}", name, e.Message, e.LineNumber, e.LinePosition), e);
//			}
//		}

	}
}
