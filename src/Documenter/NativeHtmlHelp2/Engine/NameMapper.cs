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
	/// 
	/// </summary>
	public enum WhichType
	{
		Class,
		Interface,
		Structure,
		Enumeration,
		Delegate,
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


		private StringDictionary fileNames;
		private StringDictionary elemNames;

		public NameMapper()
		{
		}

		private void Reset()
		{
			fileNames = new StringDictionary();
			elemNames = new StringDictionary();
		}

		public StringDictionary FileNames
		{
			get{ return fileNames; }
		}

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
			Reset();

			XmlNodeList namespaces = documentation.SelectNodes("/ndoc/assembly/module/namespace");
			foreach (XmlElement namespaceNode in namespaces)
			{
				string namespaceName = namespaceNode.Attributes["name"].Value;
				string namespaceId = "N:" + namespaceName;
				fileNames[namespaceId] = NameMapper.GetFilenameForNamespace( namespaceName );
				elemNames[namespaceId] = namespaceName;

				XmlNodeList types = namespaceNode.SelectNodes("*[@id]");
				foreach (XmlElement typeNode in types)
				{
					string typeId = typeNode.Attributes["id"].Value;
					fileNames[typeId] = NameMapper.GetFilenameForType(typeNode);
					elemNames[typeId] = typeNode.Attributes["name"].Value;

					XmlNodeList members = typeNode.SelectNodes("*[@id]");
					foreach (XmlElement memberNode in members)
					{
						string id = memberNode.Attributes["id"].Value;
						switch (memberNode.Name)
						{
							case "constructor":
								fileNames[id] = NameMapper.GetFilenameForConstructor(memberNode);
								elemNames[id] = elemNames[typeId];
								break;
							case "field":
								if (typeNode.Name == "enumeration")
									fileNames[id] = NameMapper.GetFilenameForType(typeNode);
								else
									fileNames[id] = NameMapper.GetFilenameForField(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "property":
								fileNames[id] = NameMapper.GetFilenameForProperty(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "method":
								fileNames[id] = NameMapper.GetFilenameForMethod(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "operator":
								fileNames[id] = NameMapper.GetFilenameForOperator(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
							case "event":
								fileNames[id] = NameMapper.GetFilenameForEvent(memberNode);
								elemNames[id] = memberNode.Attributes["name"].Value;
								break;
						}
					}
				}
			}
		}


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

		public static string GetFileNameForNamespaceHierarchy( string namespaceName )
		{
			return namespaceName + "Hierarchy.html";
		}

		public static string GetFilenameForNamespace( string namespaceName )
		{
			return namespaceName + ".html";
		}

		public static string GetFilenameForType( string typeID )
		{
			return typeID.Substring(2) + "Topic.html";
		}

		public static string GetFilenameForType( XmlNode typeNode )
		{
			return GetFilenameForType( typeNode.Attributes["id"].Value );
		}

		public static string GetFilenameForTypeMembers( string typeID )
		{
			return typeID.Substring(2) + "MembersTopic.html";
		}
		public static string GetFilenameForTypeMembers( XmlNode typeNode )
		{
			return GetFilenameForTypeMembers( typeNode.Attributes["id"].Value );
		}

		public static string GetFilenameForConstructors( string typeID )
		{
			return typeID.Substring(2) + "ConstructorTopic.html";
		}
		public static string GetFilenameForConstructors( XmlNode typeNode )
		{
			return GetFilenameForConstructors( typeNode.Attributes["id"].Value );
		}

		public static string GetFilenameForConstructor( string constructorID, bool isStatic  )
		{
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor

			string fileName = constructorID.Substring(2, dotHash - 2);
			if ( isStatic )
				fileName += "Static";

			fileName += "Constructor";

			return fileName += "Topic.html";
		}
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

		public static string GetFilenameForTypeFields( string typeID )
		{
			return typeID.Substring(2) + "FieldsTopic.html";
		}
		public static string GetFilenameForTypeFields( XmlNode typeNode )
		{
			return GetFilenameForTypeFields( typeNode.Attributes["id"].Value );
		}


		public static string GetFilenameForField( string fieldID )
		{
			return fieldID.Substring(2) + "Topic.html";
		}
		public static string GetFilenameForField( XmlNode fieldNode )
		{
			return GetFilenameForField( fieldNode.Attributes["id"].Value );
		}


		public static string GetFilenameForTypeOperators( string typeID )
		{
			return typeID.Substring(2) + "OperatorsTopic.html";
		}
		public static string GetFilenameForTypeOperators( XmlNode typeNode )
		{
			return GetFilenameForTypeOperators( typeNode.Attributes["id"].Value );
		}


		public static string GetFilenameForOperatorsOverloads( string typeID, string opName )
		{
			return typeID.Substring(2) + "." + opName + "Topic.html";
		}
		public static string GetFilenameForOperatorsOverloads( XmlNode typeNode, XmlNode opNode )
		{
			return GetFilenameForOperatorsOverloads( typeNode.Attributes["id"].Value, opNode.Attributes["name"].Value );
		}


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

		public static string GetFilenameForTypeEvents( string typeID )
		{
			return typeID.Substring(2) + "EventsTopic.html";
		}
		public static string GetFilenameForTypeEvents( XmlNode typeNode )
		{
			return GetFilenameForTypeEvents( typeNode.Attributes["id"].Value );
		}

		public static string GetFilenameForEvent( string eventID )
		{
			return eventID.Substring(2) + "Topic.html";
		}
		public static string GetFilenameForEvent( XmlNode eventNode )
		{
			return GetFilenameForEvent( eventNode.Attributes["id"].Value );
		}

		public static string GetFilenameForTypeProperties( string typeID )
		{
			return typeID.Substring(2) + "PropertiesTopic.html";
		}
		public static string GetFilenameForTypeProperties( XmlNode typeNode )
		{
			return GetFilenameForTypeProperties( typeNode.Attributes["id"].Value );
		}

		public static string GetFilenameForPropertyOverloads( string typeID, string propertyName )
		{
			return typeID.Substring(2) + propertyName + "Topic.html";
		}
		public static string GetFilenameForPropertyOverloads( XmlNode typeNode, XmlNode propertyNode )
		{
			 return GetFilenameForPropertyOverloads( typeNode.Attributes["id"].Value, propertyNode.Attributes["name"].Value );
		}

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

		public static string GetFilenameForTypeMethods( string typeID )
		{
			return typeID.Substring(2) + "MethodsTopic.html";
		}
		public static string GetFilenameForTypeMethods( XmlNode typeNode )
		{
			 return GetFilenameForTypeMethods( typeNode.Attributes["id"].Value );
		}

		public static string GetFilenameForMethodOverloads( string typeID, string methodName )
		{
			return typeID.Substring(2) + "." + methodName + "Topic.html";
		}
		public static string GetFilenameForMethodOverloads( XmlNode typeNode, XmlNode methodNode )
		{
			return GetFilenameForMethodOverloads( typeNode.Attributes["id"].Value, methodNode.Attributes["name"].Value );
		}

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
