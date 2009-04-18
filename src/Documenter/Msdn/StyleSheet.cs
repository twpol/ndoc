#region License

// Copyright (C) 2003-2009  the NDoc and NDoc3 team
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

#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace NDoc3.Documenter.Msdn
{
	internal class StyleSheet
	{
		private readonly string _name;
		private readonly XslTransform _transform;
		private readonly NameTable _nameTable;

		public StyleSheet(string name, XmlResolver xmlResolver)
		{
			_name = name;
			_nameTable= new NameTable();
			_transform = MakeTransform(name, xmlResolver);
		}

		public string Name
		{
			get { return _name; }
		}

		public NameTable NameTable
		{
			get { return _nameTable; }
		}

		public void Transform(IXPathNavigable xpathNavigable, XsltArgumentList arguments, TextWriter writer)
		{
			_transform.Transform(xpathNavigable, arguments, writer);
		}

		private XslTransform MakeTransform( string name,  XmlResolver resolver)
		{
			try
			{
				Trace.WriteLine( string.Format("Compiling {0}.xslt", name) );

				XmlReader xmlDoc=(XmlReader)resolver.GetEntity(new Uri("res:" + name + ".xslt"),null,typeof(XmlReader));

				XslTransform transform = new XslTransform();
				transform.Load(xmlDoc, resolver);
				return transform;
			}
			catch ( XsltException e )
			{
				throw new Exception(string.Format("Error compiling the stylesheet '{0}': {1} at {2}:{3}", name, e.Message, e.LineNumber, e.LinePosition), e);
			}
		}

		// CompiledTransform causes lots of troubles!

//		private XslCompiledTransform MakeCompiledTransform( string name,  XmlResolver resolver)
//		{
//			try
//			{
//				Trace.WriteLine( string.Format("Compiling {0}.xslt", name) );
//				XmlReader reader=(XmlReader)resolver.GetEntity(new Uri("res:" + name + ".xslt"),null,typeof(XmlReader));
//
////				transform.Load(reader, resolver, Assembly.GetExecutingAssembly().Evidence);
//				XsltSettings settings = new XsltSettings(false, true);
//				XslCompiledTransform transform = new XslCompiledTransform(false);
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