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
	/// HxFile is the base class for vaiors Html Help 2 project files 
	/// that use Xml to store their data
	/// </summary>
	public abstract class HxFile
	{
		/// <summary>
		/// The XmlNode that represents the project file's data
		/// </summary>
		protected XmlNode dataNode;

		/// <summary>
		/// The name of the file
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Creates a new instance of the HxFile class
		/// </summary>
		/// <param name="name">The file's name</param>
		/// <param name="node">The file's data</param>
		protected HxFile( string name, XmlNode node )
		{
			if ( node == null )
				throw new NullReferenceException();

			Name = name;
			dataNode = node;
		}

		/// <summary>
		/// Save's the Xml to the specified location, using the FileName propery or the filename
		/// </summary>
		/// <param name="location">Folder name to save the file into</param>
		public void Save( string location )
		{
			if ( !Directory.Exists( location ) )
				throw new ArgumentException( string.Format( "The specified directory {0}, does not exist", location ) );

			Debug.Assert( dataNode.OwnerDocument != null );
			dataNode.OwnerDocument.Save( Path.Combine( location, FileName ) );
		}

		/// <summary>
		/// The File name to use when saving
		/// </summary>
		public abstract string FileName{ get; }

		/// <summary>
		/// Retrieves the value of the specified node
		/// </summary>
		/// <param name="xpath">The node to retrieve (relative to dataNode)</param>
		/// <returns>The InnerText property of the node</returns>
		protected string GetProperty( string xpath )
		{
			XmlNode node = dataNode.SelectSingleNode( xpath );
			Debug.Assert( node != null );
			return node.InnerText;
		}

		/// <summary>
		/// Sets the value of the specified node
		/// </summary>
		/// <param name="xpath">The node to retrieve (relative to dataNode)</param>
		/// <param name="value">The value to set (uses the ToString() method as the value)</param>
		protected void SetProperty( string xpath, object value )
		{
			if ( object.ReferenceEquals( value, null ) )
				throw new NullReferenceException();

			XmlNode node = dataNode.SelectSingleNode( xpath );
			Debug.Assert( node != null );
			node.InnerText = value.ToString();
		}

	}
}
