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
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace NDoc.Documenter.NativeHtmlHelp2.HxProject
{
	/// <summary>
	/// Summary description for HxFile.
	/// </summary>
	public abstract class HxFile
	{
		/// <summary>
		/// 
		/// </summary>
		protected XmlNode dataNode;

		/// <summary>
		/// 
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="node"></param>
		protected HxFile( string name, XmlNode node )
		{
			if ( node == null )
				throw new NullReferenceException();

			Name = name;
			dataNode = node;
		}

		public void Save( string location )
		{
			Debug.Assert( dataNode.OwnerDocument != null );
			dataNode.OwnerDocument.Save( Path.Combine( location, FileName ) );
		}

		public abstract string FileName{ get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		protected string GetProperty( string path )
		{
			XmlNode node = dataNode.SelectSingleNode( path );
			Debug.Assert( node != null );
			return node.InnerText;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="value"></param>
		protected void SetProperty( string path, object value )
		{
			if ( object.ReferenceEquals( value, null ) )
				throw new NullReferenceException();

			XmlNode node = dataNode.SelectSingleNode( path );
			Debug.Assert( node != null );
			node.InnerText = value.ToString();
		}
	}
}
