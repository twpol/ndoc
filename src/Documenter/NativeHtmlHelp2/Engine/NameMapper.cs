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
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace NDoc.Documenter.NativeHtmlHelp2.Engine
{
	/// <summary>
	/// The types of code elemements that topics are generated for
	/// </summary>
	public enum WhichType
	{
		/// <summary>
		/// classes
		/// </summary>
		Class,
		/// <summary>
		/// interfaces
		/// </summary>
		Interface,
		/// <summary>
		/// structs
		/// </summary>
		Structure,
		/// <summary>
		/// enumberations
		/// </summary>
		Enumeration,
		/// <summary>
		/// delegates
		/// </summary>
		Delegate,
		/// <summary>
		/// error case
		/// </summary>
		Unknown
	};

	/// <summary>
	/// Provides methods for mapping type name to file names
	/// </summary>
	public class NameMapper
	{

		private static Hashtable lowerCaseTypeNames;

		static NameMapper()
		{
			lowerCaseTypeNames = new Hashtable();

			lowerCaseTypeNames.Add( WhichType.Class, "class" );
			lowerCaseTypeNames.Add( WhichType.Interface, "interface" );
			lowerCaseTypeNames.Add( WhichType.Structure, "structure" );
			lowerCaseTypeNames.Add( WhichType.Enumeration, "enumeration" );
			lowerCaseTypeNames.Add( WhichType.Delegate, "delegate" );
		}

		/// <summary>
		/// The collection of lower class type names
		/// </summary>
		public static Hashtable LowerCaseTypeNames
		{
			get{ return lowerCaseTypeNames; }
		}		


		/// <summary>
		/// Creates a new isntance of a NameMapper
		/// </summary>
		public NameMapper()
		{
		}

		private StringDictionary elemNames;

		/// <summary>
		/// A collection of element names generated for the documentation xml
		/// </summary>
		public StringDictionary ElemNames
		{
			get{ return elemNames; }
		}

		/// <summary>
		/// Creates the filename to type mapping
		/// </summary>
		/// <param name="documentation">The NDoc XML documentation summary</param>
		public void MakeFilenames( XmlNode documentation )
		{
			elemNames = new StringDictionary();

			XmlNodeList namespaces = documentation.SelectNodes("/ndoc/assembly/module/namespace");
			foreach (XmlElement namespaceNode in namespaces)
			{
				string namespaceName = namespaceNode.Attributes["name"].Value;
				string namespaceId = "N:" + namespaceName;

				elemNames[namespaceId] = namespaceName;

				XmlNodeList types = namespaceNode.SelectNodes("*[@id]");
				foreach (XmlElement typeNode in types)
				{
					string typeId = typeNode.Attributes["id"].Value;
					elemNames[typeId] = typeNode.Attributes["name"].Value;

					XmlNodeList members = typeNode.SelectNodes("*[@id]");
					foreach (XmlElement memberNode in members)
					{
						string id = memberNode.Attributes["id"].Value;
						switch (memberNode.Name)
						{
							case "constructor":
								elemNames[id] = elemNames[typeId];
								break;
							case "field":
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "property":
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "method":
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "operator":
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "event":
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Determines what type of item a node described
		/// </summary>
		/// <param name="typeNode">The documantaion node</param>
		/// <returns>An enumeration for the item type</returns>
		public static WhichType GetWhichType( XmlNode typeNode )
		{
			WhichType whichType;

			switch (typeNode.Name)
			{
				case "class":
					whichType = WhichType.Class;
					break;
				case "interface":
					whichType = WhichType.Interface;
					break;
				case "structure":
					whichType = WhichType.Structure;
					break;
				case "enumeration":
					whichType = WhichType.Enumeration;
					break;
				case "delegate":
					whichType = WhichType.Delegate;
					break;
				default:
					whichType = WhichType.Unknown;
					break;
			}

			return whichType;
		}

		/// <summary>
		/// Determines the filename for the namespace hierarchy topic
		/// </summary>
		/// <param name="namespaceName">The namespace</param>
		/// <returns>Topic Filename</returns>
		public static string GetFileNameForNamespaceHierarchy( string namespaceName )
		{
			return namespaceName + "Hierarchy.html";
		}

		/// <summary>
		/// Determines the filename for a namespace topic
		/// </summary>
		/// <param name="namespaceName">The namespace</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForNamespace( string namespaceName )
		{
			return namespaceName + ".html";
		}

		/// <summary>
		/// Determines the filename for a type overview topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForType( string typeID )
		{
			return typeID.Substring(2) + "Topic.html";
		}
		/// <summary>
		/// Determines the filename for a type overview topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForType( XmlNode typeNode )
		{
			return GetFilenameForType( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Determines the filename for a type member list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeMembers( string typeID )
		{
			return typeID.Substring(2) + "MembersTopic.html";
		}
		/// <summary>
		/// Determines the filename for a type member list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeMembers( XmlNode typeNode )
		{
			return GetFilenameForTypeMembers( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Determines the filename for a constructor list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForConstructors( string typeID )
		{
			return typeID.Substring(2) + "ConstructorTopic.html";
		}
		/// <summary>
		/// Determines the filename for a constructor list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForConstructors( XmlNode typeNode )
		{
			return GetFilenameForConstructors( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Determines the filename for a constructor topic
		/// </summary>
		/// <param name="constructorID">The id of the constructor</param>
		/// <param name="isStatic">Is it a static constructor</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForConstructor( string constructorID, bool isStatic  )
		{
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor

			string fileName = constructorID.Substring(2, dotHash - 2);
			if ( isStatic )
				fileName += "Static";

			fileName += "Constructor";

			return fileName += "Topic.html";
		}
		/// <summary>
		/// Determines the filename for a constructor topic
		/// </summary>
		/// <param name="constructorID">The id of the constructor</param>
		/// <param name="isStatic">Is it a static constructor</param>
		/// <param name="overLoad">The oerload of the constructor</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForConstructor( string constructorID, bool isStatic, string overLoad  )
		{
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor

			string fileName = constructorID.Substring(2, dotHash - 2);
			if ( isStatic )
				fileName += "Static";

			fileName += "Constructor";

			fileName += overLoad;

			return fileName += "Topic.html";
		}
		/// <summary>
		/// Determines the filename for a constructor topic
		/// </summary>
		/// <param name="constructorNode">The XmlNode representing the constructor</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForConstructor( XmlNode constructorNode )
		{
			if ( constructorNode.Attributes["overload"] != null )	
				return GetFilenameForConstructor( constructorNode.Attributes["id"].Value, 
					constructorNode.Attributes["contract"].Value == "Static",
					constructorNode.Attributes["overload"].Value );
			else
				return GetFilenameForConstructor( constructorNode.Attributes["id"].Value, 
					constructorNode.Attributes["contract"].Value == "Static" );
		}

		/// <summary>
		/// Determines the filename for a type field list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeFields( string typeID )
		{
			return typeID.Substring(2) + "FieldsTopic.html";
		}
		/// <summary>
		/// Determines the filename for a type field list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeFields( XmlNode typeNode )
		{
			return GetFilenameForTypeFields( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Gets the filename for a particular field topic
		/// </summary>
		/// <param name="fieldID"></param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForField( string fieldID )
		{
			return fieldID.Substring(2) + "Topic.html";
		}
		/// <summary>
		/// Gets the filename for a particular field topic
		/// </summary>
		/// <param name="fieldNode"></param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForField( XmlNode fieldNode )
		{
			return GetFilenameForField( fieldNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Determines the filename for a type operator list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeOperators( string typeID )
		{
			return typeID.Substring(2) + "OperatorsTopic.html";
		}
		/// <summary>
		/// Determines the filename for a type operator list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeOperators( XmlNode typeNode )
		{
			return GetFilenameForTypeOperators( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Determines the filename for an operator overloads list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="opName">The name of the operator</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForOperatorsOverloads( string typeID, string opName )
		{
			return typeID.Substring(2) + "." + opName + "Topic.html";
		}
		/// <summary>
		/// Determines the filename for an operator overloads list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <param name="opNode">The XmlNode repsenting the operator</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForOperatorsOverloads( XmlNode typeNode, XmlNode opNode )
		{
			return GetFilenameForOperatorsOverloads( typeNode.Attributes["id"].Value, opNode.Attributes["name"].Value );
		}

		/// <summary>
		/// Gets the filename for a particular operator topic
		/// </summary>
		/// <param name="operatorNode">The XmlNode repsenting the operator</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForOperator( XmlNode operatorNode )
		{
			string operatorID = operatorNode.Attributes["id"].Value;
			string fileName = operatorID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
				fileName = fileName.Substring(0, leftParenIndex);
			
			if (operatorNode.Attributes["overload"] != null)
				fileName += operatorNode.Attributes["overload"].Value;

			fileName += "Topic.html";

			return fileName;
		}
		
		/// <summary>
		/// Determines the filename for a type event list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeEvents( string typeID )
		{
			return typeID.Substring(2) + "EventsTopic.html";
		}
		/// <summary>
		/// Determines the filename for a type event list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeEvents( XmlNode typeNode )
		{
			return GetFilenameForTypeEvents( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Gets the filename for a particular event topic
		/// </summary>
		/// <param name="eventID"></param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForEvent( string eventID )
		{
			return eventID.Substring(2) + "Topic.html";
		}
		/// <summary>
		/// Gets the filename for a particular event topic
		/// </summary>
		/// <param name="eventNode"></param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForEvent( XmlNode eventNode )
		{
			return GetFilenameForEvent( eventNode.Attributes["id"].Value );
		}
		
		/// <summary>
		/// Determines the filename for a type property list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeProperties( string typeID )
		{
			return typeID.Substring(2) + "PropertiesTopic.html";
		}
		/// <summary>
		/// Determines the filename for a type property list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeProperties( XmlNode typeNode )
		{
			return GetFilenameForTypeProperties( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Determines the filename for an property overloads list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="propertyName">The property name</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForPropertyOverloads( string typeID, string propertyName )
		{
			return typeID.Substring(2) + "." + propertyName + "Topic.html";
		}
		/// <summary>
		/// Determines the filename for an property overloads list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <param name="propertyNode"></param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForPropertyOverloads( XmlNode typeNode, XmlNode propertyNode )
		{
			 return GetFilenameForPropertyOverloads( typeNode.Attributes["id"].Value, propertyNode.Attributes["name"].Value );
		}

		/// <summary>
		/// Gets the filename for a particular property topic
		/// </summary>
		/// <param name="propertyNode">XmlNode representing the property</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForProperty( XmlNode propertyNode )
		{
			string propertyID = propertyNode.Attributes["id"].Value;
			string fileName = propertyID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
				fileName = fileName.Substring(0, leftParenIndex);

			if (propertyNode.Attributes["overload"] != null)
				fileName += propertyNode.Attributes["overload"].Value;

			fileName += "Topic.html";

			return fileName;
		}

		/// <summary>
		/// Determines the filename for a type method list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeMethods( string typeID )
		{
			return typeID.Substring(2) + "MethodsTopic.html";
		}
		/// <summary>
		/// Determines the filename for a type method list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForTypeMethods( XmlNode typeNode )
		{
			 return GetFilenameForTypeMethods( typeNode.Attributes["id"].Value );
		}

		/// <summary>
		/// Determines the filename for a method voerload list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="methodName">The name of the method</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForMethodOverloads( string typeID, string methodName )
		{
			return typeID.Substring(2) + "." + methodName + "Topic.html";
		}
		/// <summary>
		/// Determines the filename for a method voerload list topic
		/// </summary>
		/// <param name="typeNode">XmlNode representing the type</param>
		/// <param name="methodNode">XmlNode representing the method</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForMethodOverloads( XmlNode typeNode, XmlNode methodNode )
		{
			return GetFilenameForMethodOverloads( typeNode.Attributes["id"].Value, methodNode.Attributes["name"].Value );
		}

		/// <summary>
		/// Gets the filename for a particular method topic
		/// </summary>
		/// <param name="methodNode">XmlNode representing the method</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForMethod( XmlNode methodNode )
		{
			string methodID = methodNode.Attributes["id"].Value;
			string fileName = methodID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
				fileName = fileName.Substring(0, leftParenIndex);

			fileName = RemoveChar( fileName, '#' );

			if ( methodNode.Attributes["overload"] != null )
				fileName += methodNode.Attributes["overload"].Value;

			fileName += "Topic.html";

			return fileName;
		}

		private static string RemoveChar(string s, char c)
		{
			StringBuilder builder = new StringBuilder();

			foreach ( char ch in s.ToCharArray() )
			{
				if ( ch != c )				
					builder.Append(ch);				
			}

			return builder.ToString();
		}
	}
}
