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


		public static WhichType GetWhichType(XmlNode typeNode)
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

		public static string GetFilenameForNamespace(string namespaceName)
		{
			string fileName = namespaceName + ".html";
			return fileName;
		}

		public static string GetFilenameForType(XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "Topic.html";
			return fileName;
		}

		public static string GetFilenameForTypeMembers(XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "MembersTopic.html";
			return fileName;
		}

		public static string GetFilenameForConstructors(XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "ConstructorTopic.html";
			return fileName;
		}

		public static string GetFilenameForConstructor(XmlNode constructorNode)
		{
			string constructorID = (string)constructorNode.Attributes["id"].Value;
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor

			string fileName = constructorID.Substring(2, dotHash - 2);
			if (constructorNode.Attributes["contract"].Value == "Static")
				fileName += "Static";

			fileName += "Constructor";

			if (constructorNode.Attributes["overload"] != null)
			{
				fileName += (string)constructorNode.Attributes["overload"].Value;
			}

			fileName += "Topic.html";

			return fileName;
		}

		public static string GetFilenameForFields(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "FieldsTopic.html";
			return fileName;
		}

		public static string GetFilenameForField(XmlNode fieldNode)
		{
			string fieldID = (string)fieldNode.Attributes["id"].Value;
			string fileName = fieldID.Substring(2) + "Topic.html";
			return fileName;
		}

		public static string GetFilenameForOperators(WhichType whichType, XmlNode typeNode)
		{
			string typeID = typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "OperatorsTopic.html";
			return fileName;
		}

		public static string GetFilenameForOperatorsOverloads(XmlNode typeNode, XmlNode opNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string opName = (string)opNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + opName + "Topic.html";
			return fileName;
		}

		public static string GetFilenameForOperator(XmlNode operatorNode)
		{
			string operatorID = operatorNode.Attributes["id"].Value;
			string fileName = operatorID.Substring(2);

			//			int opIndex = fileName.IndexOf("op_");
			//
			//			if (opIndex != -1)
			//			{
			//				fileName = fileName.Remove(opIndex, 3);
			//			}

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
			{
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (operatorNode.Attributes["overload"] != null)
			{
				//fileName += "_overload_" + operatorNode.Attributes["overload"].Value;
				fileName += operatorNode.Attributes["overload"].Value;
			}

			fileName += "Topic.html";

			return fileName;
		}

		public static string GetFilenameForEvents(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "EventsTopic.html";
			return fileName;
		}

		public static string GetFilenameForEvent(XmlNode eventNode)
		{
			string eventID = (string)eventNode.Attributes["id"].Value;
			string fileName = eventID.Substring(2) + "Topic.html";
			return fileName;
		}

		public static string GetFilenameForProperties(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "PropertiesTopic.html";
			return fileName;
		}

		public static string GetFilenameForPropertyOverloads(XmlNode typeNode, XmlNode propertyNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string propertyName = (string)propertyNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + propertyName + "Topic.html";
			return fileName;
		}

		public static string GetFilenameForProperty(XmlNode propertyNode)
		{
			string propertyID = (string)propertyNode.Attributes["id"].Value;
			string fileName = propertyID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
			{
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (propertyNode.Attributes["overload"] != null)
			{
				fileName += (string)propertyNode.Attributes["overload"].Value;
			}

			fileName += "Topic.html";

			return fileName;
		}

		public static string GetFilenameForMethods(WhichType whichType, XmlNode typeNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string fileName = typeID.Substring(2) + "MethodsTopic.html";
			return fileName;
		}

		public static string GetFilenameForMethodOverloads(XmlNode typeNode, XmlNode methodNode)
		{
			string typeID = (string)typeNode.Attributes["id"].Value;
			string methodName = (string)methodNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + methodName + "Topic.html";
			return fileName;
		}

		public static string GetFilenameForMethod(XmlNode methodNode)
		{
			string methodID = (string)methodNode.Attributes["id"].Value;
			string fileName = methodID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1)
			{
				fileName = fileName.Substring(0, leftParenIndex);
			}

			fileName = RemoveChar(fileName, '#');

			if (methodNode.Attributes["overload"] != null)
			{
				fileName += "_overload_" + (string)methodNode.Attributes["overload"].Value;
			}

			fileName += "Topic.html";

			return fileName;
		}

		private static string RemoveChar(string s, char c)
		{
			StringBuilder builder = new StringBuilder();

			foreach (char ch in s.ToCharArray())
			{
				if (ch != c)
				{
					builder.Append(ch);
				}
			}

			return builder.ToString();
		}

	}
}
