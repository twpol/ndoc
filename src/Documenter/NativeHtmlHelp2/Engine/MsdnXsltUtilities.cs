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
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections.Specialized;

namespace NDoc.Documenter.NativeHtmlHelp2.Engine
{
	/// <summary>
	/// Provides an extension object for the xslt transformations.
	/// </summary>
	public class MsdnXsltUtilities
	{
		private const string sdkDoc10BaseUrl = "ms-help://MS.NETFrameworkSDK";
		private const string sdkDoc11BaseUrl = "ms-help://MS.NETFrameworkSDKv1.1";

		private const string systemPrefix = "System.";

		private string sdkDocBaseUrl; 

		private StringDictionary elemNames;

		private StringCollection descriptions;

		/// <summary>
		/// Initializes a new instance of class MsdnXsltUtilities
		/// </summary>
		/// <param name="elemNames">A StringDictionary holding id to element name mappings</param>
		/// <param name="linkToSdkDocVersion">Specifies the version of the SDK documentation.</param>	
		public MsdnXsltUtilities( StringDictionary elemNames, SdkDocVersion linkToSdkDocVersion )
		{
			descriptions = new StringCollection();

			this.elemNames = elemNames;
			
			switch (linkToSdkDocVersion)
			{
				case SdkDocVersion.SDK_v1_0:
					sdkDocBaseUrl = sdkDoc10BaseUrl;
					break;
				case SdkDocVersion.SDK_v1_1:
					sdkDocBaseUrl = sdkDoc11BaseUrl;
					break;
			}

		}

		/// <summary>
		/// Gets the base Url for system types links.
		/// </summary>
		public string SdkDocBaseUrl
		{
			get { return sdkDocBaseUrl; }
		}


#if MONO
		/// <summary>
		/// Returns an HRef for a CRef.
		/// </summary>
		/// <param name="list">The argument list containing the 
		/// cRef for which the HRef will be looked up.</param>
		/// <remarks>Mono needs this overload, as its XsltTransform can only
		/// call methods with an ArraList parameter.</remarks>
		public string GetHRef(System.Collections.ArrayList list)
		{
			string cref = (string)list[0];
			return GetHRef(cref);
		}
#endif
		/// <summary>
		/// Gets the href for a namespace topic
		/// </summary>
		/// <param name="namespaceName">The namespace name</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetNamespaceHRef( string namespaceName )
		{
			return NameMapper.GetFilenameForNamespace( namespaceName );
		}

		/// <summary>
		/// Gets the Href for the namespace hierarchy topic
		/// </summary>
		/// <param name="namespaceName">The namespace name</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetNamespaceHierarchyHRef( string namespaceName )
		{
			return NameMapper.GetFileNameForNamespaceHierarchy( namespaceName );
		}

		/// <summary>
		/// Gets the href for the all members topic of a type
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetTypeMembersHRef( string typeID )
		{
			return NameMapper.GetFilenameForTypeMembers( typeID );
		}

		/// <summary>
		/// Gets the href for the fields topic for a type
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetTypeFieldsHRef( string typeID )
		{
			return NameMapper.GetFilenameForTypeProperties( typeID );
		}

		/// <summary>
		/// Gets the href for the hethods topic of a type
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetTypeMethodsHRef( string typeID )
		{
			return NameMapper.GetFilenameForTypeMethods( typeID );
		}

		/// <summary>
		/// Gets the href for the operators topic of a type
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetTypeOperatorsHRef( string typeID )
		{
			return NameMapper.GetFilenameForTypeOperators( typeID );
		}

		/// <summary>
		/// Gets the href for the events topic of a type
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetTypeEventsHRef( string typeID )
		{
			return NameMapper.GetFilenameForTypeEvents( typeID );
		}

		/// <summary>
		/// Gets the href for the properties topic of a type
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetTypePropertiesHRef( string typeID )
		{
			return NameMapper.GetFilenameForTypeProperties( typeID );
		}

		/// <summary>
		/// Gets the href for the constructor overloads topic of a type
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetCustructorOverloadHRef( string typeID )
		{
			return NameMapper.GetFilenameForConstructors( typeID );
		}

		/// <summary>
		/// Gets the href for a constructor
		/// </summary>
		/// <param name="xPathNode">The node selection for the contsructor</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetCustructorHRef( XPathNodeIterator xPathNode )
		{
			xPathNode.MoveNext();
			if ( xPathNode.Current != null && xPathNode.Current is IHasXmlNode )
				return NameMapper.GetFilenameForConstructor( ((IHasXmlNode)xPathNode.Current).GetNode() );
			return "";
		}

		/// <summary>
		/// Get the href for a member overload topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="methodName">The name of the method</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetMemberOverloadHRef( string typeID, string methodName )
		{
			return NameMapper.GetFilenameForMethodOverloads( typeID, methodName );
		}

		/// <summary>
		/// Get the href for a type member topic
		/// </summary>
		/// <param name="xPathNode">The member selection</param>
		/// <returns>Relative HRef to the Topic</returns>			
		public string GetMemberHRef( XPathNodeIterator xPathNode )
		{
			xPathNode.MoveNext();
			if ( xPathNode.Current != null && xPathNode.Current is IHasXmlNode )
			{
				XmlNode node = ((IHasXmlNode)xPathNode.Current).GetNode();

				switch ( node.Name )
				{
					case "field":		return NameMapper.GetFilenameForField( node );
					case "event":		return NameMapper.GetFilenameForEvent( node );
					case "method":		return NameMapper.GetFilenameForMethod( node );
					case "property":	return NameMapper.GetFilenameForProperty( node );
					case "operator":	return NameMapper.GetFilenameForOperator( node );
					case "constructor":	return NameMapper.GetFilenameForConstructor( node );
					default:			return "";
				}
			}

			return "";
		}

		/// <summary>
		/// Get the HRef for a local method topic
		/// </summary>
		/// <param name="typeID">The id of the containing type</param>
		/// <param name="memberName"></param>
		/// <returns>Relative HRef to the Topic</returns>			
		public string GetMethodHRef( string typeID, string memberName )
		{
			return NameMapper.GetFilenameForMethodOverloads( typeID, memberName );
		}

		/// <summary>
		/// Get the HRef for a local property topic
		/// </summary>
		/// <param name="typeID">The id of the containing type</param>
		/// <param name="propertyName">The property name</param>
		/// <returns>Relative HRef to the Topic</returns>			
		public string GetPropertyHRef( string typeID, string propertyName )
		{
			return NameMapper.GetFilenameForPropertyOverloads( typeID, propertyName );
		}

		/// <summary>
		/// Get the HRef for a local field topic
		/// </summary>
		/// <param name="fieldID">The ID of the field</param>
		/// <returns>Relative HRef to the Topic</returns>			
		public string GetFieldHRef( string fieldID  )
		{
			return NameMapper.GetFilenameForField( fieldID );
		}

		/// <summary>
		/// Get the HRef for a local event topic
		/// </summary>
		/// <param name="eventID">The ID of the event</param>
		/// <returns>Relative HRef to the Topic</returns>			
		public string GetEventHRef( string eventID  )
		{
			return NameMapper.GetFilenameForEvent( eventID );
		}

		/// <summary>
		/// Gets the href for a local type topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Relative HRef to the Topic</returns>
		public string GetTypeHRef( string typeID )
		{
			return NameMapper.GetFilenameForType( typeID );
		}

		/// <summary>
		/// Returns an HRef for a CRef. This may be local or system
		/// </summary>
		/// <param name="cref">The local html filename for local topics or the assocaitave index for system topics</param>
		public string GetHRef( string cref )
		{
			// if it's not a type string return nothing
			if ( ( cref.Length <= 2 ) || ( cref[1] != ':' ) )
				return string.Empty;

			// look up the local filename for non system types
			if ( IsLocalCRef( cref ) )
			{
				int lastDot = cref.LastIndexOf( '.' );
				string memberName = cref.Substring( lastDot + 1 );
				string typeID = cref.Substring( 0, lastDot );

				switch (cref.Substring(0, 2))
				{
					case "N:":	return GetNamespaceHRef( cref.Substring( 2 ) );
					case "T:":	return GetTypeHRef( cref );
					case "F:":	return GetFieldHRef( cref );
					case "E:":	return GetEventHRef( cref );
					case "P:":	return GetPropertyHRef( typeID, memberName );
					case "M:":	return GetMethodHRef( typeID, memberName ) ;
					default:	return string.Empty;
				}
			}
			else
			{
				switch (cref.Substring(0, 2))
				{
					case "N:":	// Namespace
						return "frlrf" + cref.Substring(2).Replace(".", "");
					case "T:":	// Type: class, interface, struct, enum, delegate
						return "frlrf" + cref.Substring(2).Replace(".", "") + "ClassTopic";
					case "F:":	// Field
					case "P:":	// Property
					case "M:":	// Method
					case "E:":	// Event
						return GetFilenameForSystemMember(cref);
					default:
						return string.Empty;
				}
			}
		}


#if MONO
		/// <summary>
		/// Returns a name for a CRef.
		/// </summary>
		/// <param name="list">The argument list containing the 
		/// cRef for which the HRef will be looked up.</param>
		/// <remarks>Mono needs this overload, as its XsltTransform can only
		/// call methods with an ArraList parameter.</remarks>
		public string GetName(System.Collections.ArrayList list)
		{
			string cref = (string)list[0];
			return GetName(cref);
		}
#endif
		/// <summary>
		/// Determines if a cref is for a local or a system type
		/// </summary>
		/// <param name="cref">The cref to evaluate</param>
		/// <returns>True if the cref namespace starts with 'System.'</returns>
		private static bool IsLocalCRef( string cref )
		{
			return ( cref.Length < 9 ) || ( cref.Substring( 2, 7 ) != systemPrefix );
		}

		/// <summary>
		/// Returns a name for a CRef.
		/// </summary>
		/// <param name="cref">CRef for which the name will be looked up.</param>
		public string GetName( string cref )
		{
			if (cref.Length < 2)
				return cref;

			if (cref[1] == ':')
			{
				if ( IsLocalCRef( cref ) )
				{
					string name = elemNames[cref];
					if (name != null)
						return name;
				}

				int index;
				if ((index = cref.IndexOf(".#c")) >= 0)
					cref = cref.Substring(2, index - 2);
				else if ((index = cref.IndexOf("(")) >= 0)
					cref = cref.Substring(2, index - 2);
				else
					cref = cref.Substring(2);
			}

			return cref.Substring(cref.LastIndexOf(".") + 1);
		}

		private string GetFilenameForSystemMember(string id)
		{
			string crefName;
			int index;
			if ((index = id.IndexOf(".#c")) >= 0)
				crefName = id.Substring(2, index - 2) + ".ctor";
			else if ((index = id.IndexOf("(")) >= 0)
				crefName = id.Substring(2, index - 2);
			else
				crefName = id.Substring(2);
			index = crefName.LastIndexOf(".");
			string crefType = crefName.Substring(0, index);
			string crefMember = crefName.Substring(index + 1);
			return "frlrf" + crefType.Replace(".", "") + "Class" + crefMember + "Topic";
		}

		/// <summary>
		/// Looks up, whether a member has similar overloads, that have already been documented.
		/// </summary>
		/// <param name="description">A string describing this overload.</param>
		/// <returns>true, if there has been a member with the same description.</returns>
		/// <remarks>
		/// <para>On the members pages overloads are cumulated. Instead of adding all overloads
		/// to the members page, a link is added to the members page, that points
		/// to an overloads page.</para>
		/// <para>If for example one overload is public, while another one is protected,
		/// we want both to appear on the members page. This is to make the search
		/// for suitable members easier.</para>
		/// <para>This leads us to the similarity of overloads. Two overloads are considered
		/// similar, if they have the same name, declaring type, access (public, protected, ...)
		/// and contract (static, non static). The description contains these four attributes
		/// of the member. This means, that two members are similar, when they have the same
		/// description.</para>
		/// <para>Asking for the first time, if a member has similar overloads, will return false.
		/// After that, if asking with the same description again, it will return true, so
		/// the overload does not need to be added to the members page.</para>
		/// </remarks>
		public bool HasSimilarOverloads(string description)
		{
			if (descriptions.Contains(description))
				return true;
			descriptions.Add(description);
			return false;
		}
	}
}