// MsdnDocumenter.cs - a MSDN-like documenter
// Copyright (C) 2003 Don Kackman
// Parts Copyright (C) 2004 Kevin Downs
// Parts copyright (C) 2001 Kral Ferch, Jason Diamond
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
	public class FileNameMapper
	{
		private Hashtable fileNames;
		private StringDictionary DuplicateCheckDictionary;

		private const string FilenamePrefix = "ndoc";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="xmlDocumentation"></param>
		public FileNameMapper(XmlNode xmlDocumentation)
		{
			fileNames = new Hashtable();
			BuildFilenameCache(xmlDocumentation);
		}

		/// <summary>
		/// Determines what type of item a node described
		/// </summary>
		/// <param name="typeNode">The documantaion node</param>
		/// <returns>An enumeration for the item type</returns>
		public static WhichType GetWhichType(XmlNode typeNode)
		{
			switch (typeNode.Name)
			{
				case "class" : return WhichType.Class;
				case "interface" : return WhichType.Interface;
				case "structure" : return WhichType.Structure;
				case "enumeration" : return WhichType.Enumeration;
				case "delegate" : return WhichType.Delegate;
				default : return WhichType.Unknown;
			}
		}

		/// <summary>
		/// Determines the filename for the namespace hierarchy topic
		/// </summary>
		/// <param name="namespaceName">The namespace</param>
		/// <returns>Topic Filename</returns>
		public static string GetFileNameForNamespaceHierarchy(string namespaceName)
		{
			return namespaceName.Replace(".", "") + "Hierarchy.html";
		}

		/// <summary>
		/// Determines the filename for a namespace topic
		/// </summary>
		/// <param name="namespaceName">The namespace</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForNamespace(string namespaceName)
		{
			return namespaceName.Replace(".", "") + ".html";
		}

		/// <summary>
		/// Determines the filename for a type overview topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForType(string typeID)
		{
			return BaseNameFromTypeId(typeID) + "Topic.html";
		}


		/// <summary>
		/// Determines the filename for an overview topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="pageType">The type of overview page to generate</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForOverviewPage(string typeID, string pageType)
		{
			Debug.Assert(((pageType == "Members") || (pageType == "Constructor") || 
				(pageType == "Methods") || (pageType == "Properties") || 
				(pageType == "Fields") || (pageType == "Operators") || (pageType == "Events")), 
				"Unknown Overview Page Type '" + pageType + "' requested");

			return BaseNameFromTypeId(typeID) + pageType + "Topic.html";
		}

		/// <summary>
		/// Determines the filename for a constructor topic
		/// </summary>
		/// <param name="constructorID">The id of the constructor</param>
		/// <param name="isStatic">Is it a static constructor</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForConstructor(string constructorID, bool isStatic)
		{
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor

			StringBuilder sb = new StringBuilder(BaseNameFromTypeId(constructorID.Substring(0, dotHash)));
		
			if (isStatic)
				sb.Append("Static");

			return sb.Append("ctorTopic.html").ToString();
		}

		/// <summary>
		/// Determines the filename for a constructor topic
		/// </summary>
		/// <param name="constructorID">The id of the constructor</param>
		/// <param name="isStatic">Is it a static constructor</param>
		/// <param name="overLoad">The overload of the constructor</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForConstructor(string constructorID, bool isStatic, string overLoad)
		{
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor

			StringBuilder sb = new StringBuilder(BaseNameFromTypeId(constructorID.Substring(0, dotHash)));
		
			if (isStatic)
				sb.Append("Static");

			return sb.AppendFormat("ctorTopic{0}.html", overLoad).ToString();
		}

		/// <summary>
		/// Gets the filename for a particular field topic
		/// </summary>
		/// <param name="fieldID"></param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForField(string fieldID)
		{
			return BaseNameFromMemberId(fieldID) + "Topic.html";
		}

		/// <summary>
		/// Determines the filename for an operator overloads list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="opName">The name of the operator</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForOperatorsOverloads(string typeID, string opName)
		{
			return BaseNameFromTypeId(typeID) + opName + "Topic.html";
		}

		/// <summary>
		/// Gets the filename for a particular event topic
		/// </summary>
		/// <param name="eventID"></param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForEvent(string eventID)
		{
			return BaseNameFromMemberId(eventID) + "Topic.html";
		}
		
		/// <summary>
		/// Determines the filename for an property overloads list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="propertyName">The property name</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForPropertyOverloads(string typeID, string propertyName)
		{
			return BaseNameFromTypeId(typeID) + propertyName + "Topic.html";
		}

		/// <summary>
		/// Determines the filename for a method overload list topic
		/// </summary>
		/// <param name="typeID">The id of the type</param>
		/// <param name="methodName">The name of the method</param>
		/// <returns>Topic Filename</returns>
		public static string GetFilenameForMethodOverloads(string typeID, string methodName)
		{
			return BaseNameFromTypeId(typeID) + methodName + "Topic.html";
		}

		/// <summary>
		/// Gets the filename for a specific member overload
		/// </summary>
		/// <param name="methodID">The NDoc generated member id</param>
		/// <param name="overload">The overload index of the member (can be empty string)</param>
		/// <returns>The filename for the member topic</returns>
		public static string GetFileNameForMemberOverload(string methodID, string overload)
		{
			StringBuilder sb;

			int leftParenIndex = methodID.IndexOf('(');

			if (leftParenIndex != -1)
				sb = new StringBuilder(BaseNameFromMemberId(methodID.Substring(0, leftParenIndex)));
			else
				sb = new StringBuilder(BaseNameFromMemberId(methodID));
			
			sb.Replace("#", "");

			return sb.AppendFormat("Topic{0}.html", overload).ToString();
		}

		/// <summary>
		/// Given a type id (including the T: prefix)
		/// determines the Base name for the topic file
		/// </summary>
		/// <param name="typeID">The ndoc generated id of a type</param>
		/// <returns>Topic's base name</returns>
		private static string BaseNameFromTypeId(string typeID)
		{
			return String.Format("ndoc{0}Class", typeID.Substring(2).Replace(".", ""));
		}

		/// <summary>
		/// Given a fully qualified member id (including the prefix)
		/// determines the Base name for the topic file
		/// </summary>
		/// <param name="memberID">The ndoc generated id of a type member</param>
		/// <returns>Topic's base name</returns>
		private static string BaseNameFromMemberId(string memberID)
		{
			int lastDot = memberID.LastIndexOf('.');
			return BaseNameFromTypeId(memberID.Substring(0, lastDot)) + memberID.Substring(lastDot + 1);
		}

	
		/// <summary>
		/// Gets Filename from cache
		/// </summary>
		public string this[string key]
		{
			get { return (string)fileNames[key]; }
		}


		/// <summary>
		/// Build Cache of filenames
		/// </summary>
		/// <param name="xmlDocumentation">NDoc generated XML Documentation</param>
		/// <remarks>
		/// <para>walk through the XML documentation and determine the filename for each relevant ID.</para>
		/// <para>Ensure these filenames are unique, then save these filenames in a hashtable for later use.</para>
		/// </remarks>
		private void BuildFilenameCache(XmlNode xmlDocumentation)
		{
#if DEBUG
			int start = Environment.TickCount;
#endif
			DuplicateCheckDictionary = new StringDictionary();

			XmlNodeList namespaces = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace");
			foreach (XmlElement namespaceNode in namespaces)
			{
				string namespaceName = namespaceNode.Attributes["name"].Value;
				string namespaceId = "N:" + namespaceName;
				string namespaceFilename = GetFilenameForNamespace(namespaceName);
				AddCacheItem(namespaceId, namespaceFilename);

				XmlNodeList types;
				
				types = namespaceNode.SelectNodes("*[@id and @access='Public']");
				CacheTypes(types);
				
				types = namespaceNode.SelectNodes("*[@id and @access='NestedPublic']");
				CacheTypes(types);

				types = namespaceNode.SelectNodes("*[@id and @access='NestedFamilyOrAssembly']");
				CacheTypes(types);

				types = namespaceNode.SelectNodes("*[@id and @access='NestedFamily']");
				CacheTypes(types);

				types = namespaceNode.SelectNodes("*[@id and @access='NestedAssembly']");
				CacheTypes(types);

				types = namespaceNode.SelectNodes("*[@id and @access='NestedFamilyAndAssembly']");
				CacheTypes(types);

				types = namespaceNode.SelectNodes("*[@id and @access='NestedPrivate']");
				CacheTypes(types);

				types = namespaceNode.SelectNodes("*[@id and @access='NotPublic']");
				CacheTypes(types);

				types = namespaceNode.SelectNodes("*[@id and @access='Unknown']");
				CacheTypes(types);

			}
			DuplicateCheckDictionary = null;

#if DEBUG
			Trace.WriteLine("Building File Map: " + ((Environment.TickCount - start) / 1000.0).ToString() + " sec.");
#endif
		}


		/// <summary>
		/// Caches filesnames for a given list of types and their members
		/// </summary>
		/// <param name="types">list of type nodes to cache</param>
		private void CacheTypes(XmlNodeList types)
		{
			foreach (XmlElement typeNode in types)
			{
				string typeId = typeNode.Attributes["id"].Value;
				string typeFilename = GetFilenameForType(typeId);

				AddCacheItem(typeId, typeFilename);

				XmlNodeList members;
					
				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='Public']");
				CacheMembers(members,typeNode);

				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='Family']");
				CacheMembers(members,typeNode);

				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='FamilyOrAssembly']");
				CacheMembers(members,typeNode);

				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='Assembly']");
				CacheMembers(members,typeNode);

				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='FamilyAndAssembly']");
				CacheMembers(members,typeNode);

				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='Private']");
				CacheMembers(members,typeNode);

				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='PrivateScope']");
				CacheMembers(members,typeNode);

				members	= typeNode.SelectNodes("*[@id and not(@declaringType) and @access='Unknown']");
				CacheMembers(members,typeNode);
			}
		}

		/// <summary>
		/// Cahces Filenames for a given list of members
		/// </summary>
		/// <param name="members">a list of member nodes to cache</param>
		/// <param name="typeNode">the owning type's node</param>
		private void CacheMembers(XmlNodeList members,XmlNode typeNode)
		{
			foreach (XmlElement memberNode in members)
			{
				string id = memberNode.Attributes["id"].Value;
				string filename = "";
				switch (memberNode.Name)
				{
					case "constructor" : 
						filename = GetFilenameForConstructor(memberNode);
						break;
					case "field" : 
						if (typeNode.Name == "enumeration")
							filename = (string)fileNames[typeNode.Attributes["id"].Value];
						else
							filename = GetFilenameForField(id);
						break;
					case "property" : 
						filename = GetFilenameForProperty(memberNode);
						break;
					case "method" : 
						filename = GetFilenameForMethod(memberNode);
						break;
					case "operator" : 
						filename = GetFilenameForOperator(memberNode);
						break;
					case "event" : 
						filename = GetFilenameForEvent(id);
						break;
				}

				if (filename.Length > 0)
				{
					if (typeNode.Name == "enumeration")
						fileNames[id] = filename;
					else
						AddCacheItem(id, filename);
				}
			}
		}


		/// <summary>
		/// Adds a filename to the cache, disambiguating where required
		/// </summary>
		/// <param name="id">The ID of the item to cache</param>
		/// <param name="filename">The filename for the given ID</param>
		private void AddCacheItem(string id, string filename)
		{
			while (DuplicateCheckDictionary.ContainsKey(filename))
			{
				filename = filename.Replace(".html","~.html");
			}
			DuplicateCheckDictionary.Add(filename,id);
			//Debug.WriteLine(id + "\t" + filename);
			fileNames[id] = filename;
		}


		//---------------------------------------------------------------------
		// The following methods are used during the cache building process
		//---------------------------------------------------------------------


		/// <summary>
		/// Determines the filename for a constructor topic
		/// </summary>
		/// <param name="constructorNode">The XmlNode representing the constructor</param>
		/// <returns>Topic Filename</returns>
		private static string GetFilenameForConstructor(XmlNode constructorNode)
		{
			if (constructorNode.Attributes["overload"] != null)	
				return GetFilenameForConstructor(constructorNode.Attributes["id"].Value, 
					constructorNode.Attributes["contract"].Value == "Static", 
					constructorNode.Attributes["overload"].Value);
			else
				return GetFilenameForConstructor(constructorNode.Attributes["id"].Value, 
					constructorNode.Attributes["contract"].Value == "Static");
		}

		/// <summary>
		/// Gets the filename for a particular method topic
		/// </summary>
		/// <param name="methodNode">XmlNode representing the method</param>
		/// <returns>Topic Filename</returns>
		private static string GetFilenameForMethod(XmlNode methodNode)
		{
			string methodID = methodNode.Attributes["id"].Value;

			if (methodNode.Attributes["overload"] != null)
				return GetFileNameForMemberOverload(methodID, methodNode.Attributes["overload"].Value);
			else
				return GetFileNameForMemberOverload(methodID, "");
		}

		/// <summary>
		/// Gets the filename for a particular operator topic
		/// </summary>
		/// <param name="operatorNode">The XmlNode repsenting the operator</param>
		/// <returns>Topic Filename</returns>
		private static string GetFilenameForOperator(XmlNode operatorNode)
		{
			string operatorID = operatorNode.Attributes["id"].Value;

			if (operatorNode.Attributes["overload"] != null)
				return GetFileNameForMemberOverload(operatorID, operatorNode.Attributes["overload"].Value);

			else
				return GetFileNameForMemberOverload(operatorID, "");
		}
		
		/// <summary>
		/// Gets the filename for a particular property topic
		/// </summary>
		/// <param name="propertyNode">XmlNode representing the property</param>
		/// <returns>Topic Filename</returns>
		private static string GetFilenameForProperty(XmlNode propertyNode)
		{
			string propertyID = propertyNode.Attributes["id"].Value;

			if (propertyNode.Attributes["overload"] != null)
				return GetFileNameForMemberOverload(propertyID, propertyNode.Attributes["overload"].Value);

			else
				return GetFileNameForMemberOverload(propertyID, "");
		}

	}
}
