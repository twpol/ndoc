﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Xml;
using NDoc3.Core;
using NDoc3.Core.Reflection;
using NDoc3.Xml;

namespace NDoc3.Documenter.Msdn
{
	public class NameResolver
	{
		public const string EXT = ".html";

		// <param name="fileNames">A StringDictionary holding id to file name mappings.</param>
		// <param name="elemNames">A StringDictionary holding id to display name mappings</param>

		private readonly StringDictionary fileNames = new StringDictionary();
		private readonly StringDictionary elemNames = new StringDictionary();

		private readonly ReferenceTypeDictionary<string, string[]> assemblyReferences = new ReferenceTypeDictionary<string, string[]>();

		public NameResolver(XmlDocument documentation)
		{
			BuildNameTables(documentation);
		}

		#region Used for Html file generation

		public string GetFilenameForFieldList(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Fields");
		}

		public string GetFilenameForOperatorList(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Operators");
		}

		public string GetFilenameForMethodList(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Methods");
		}

		public string GetFilenameForPropertyList(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Properties");
		}

		public string GetFilenameForEventList(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Events");
		}
		#endregion

		// exposed to XSLT
		public string GetDisplayNameForId(string currentAssemblyName, string memberId)
		{
			string name = elemNames[currentAssemblyName + memberId];
			if (name == null) {
				// search for identifier in referenced assemblies
				string[] assemblyReference = this.assemblyReferences[currentAssemblyName];
				if (assemblyReference != null) {
					foreach (string assemblyName in assemblyReference) {
						name = elemNames[assemblyName + memberId];
						if (name != null)
							break;
					}
				}
			}

			if (name == null) {
				name = elemNames[memberId];
			}
			//			Debug.WriteLine(string.Format("GetDisplayNameForId('{0}','{1}') => {2}", currentAssemblyName, memberId, name));
			return name;
		}

		// exposed to XSLT
		public string GetFilenameForId(string currentAssemblyName, string memberId)
		{
			// lookup current assembly
			string filename = GetFilenameForIdInternal(currentAssemblyName, memberId);
			return filename;
		}

		public string GetFilenameForAssembly(string assemblyName)
		{
			return GetFilenameForId(assemblyName, null);
		}

		// exposed to XSLT
		public string GetFilenameForNamespaceHierarchy(string assemblyName, string namespaceName)
		{
			if (string.IsNullOrEmpty(namespaceName))
				namespaceName = "(global)";
			return GetFilenameForIdSpecial(assemblyName, "N:" + namespaceName, "~Hierarchy");
		}

		// exposed to XSLT
		public string GetFilenameForNamespace(string assemblyName, string namespaceName)
		{
			if (string.IsNullOrEmpty(namespaceName))
				namespaceName = "(global)";
			return GetFilenameForId(assemblyName, "N:" + namespaceName);
		}

		// exposed to XSLT
		public string GetFilenameForTypeHierarchy(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Hierarchy");
		}

		// exposed to XSLT
		public string GetFilenameForTypeMemberList(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Members");
		}

		// exposed to XSLT
		public string GetFilenameForConstructorList(string assemblyName, string typeID)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "~Constructors");
		}

		// exposed to XSLT
		public string GetFilenameForOperatorOverloads(string assemblyName, string typeID, string operatorName)
		{
			return GetFilenameForIdSpecial(assemblyName, typeID, "." + operatorName + "~Overloads");
		}

		// exposed to XSLT
		public string GetFilenameForPropertyOverloads(string assemblyName, string typeID, string propertyName)
		{
			string fileName = GetFilenameForIdSpecial(assemblyName, typeID, "." + propertyName + "~Overloads");
			return fileName;
		}

		// exposed to XSLT
		public string GetFilenameForMethodOverloads(string assemblyName, string typeID, string methodName)
		{
			string fileName = GetFilenameForIdSpecial(assemblyName, typeID, "." + methodName + "~Overloads");
			return fileName;
		}

		// exposed to XSLT
		public string GetFilenameForTypename(string currentAssemblyName, string typeName)
		{
			//			return GetFilenameForId(assemblyName, "T:"+typeName);

			// strip leading type identifier
			if (typeName.Length > 1 && typeName[1] != ':')
				typeName = "T:" + typeName;
			string filename = GetFilenameForId(currentAssemblyName, typeName);
//			Debug.WriteLine(string.Format("GetFilenameForTypename('{0}', '{1}') => {2}", currentAssemblyName, typeName, filename));
			return filename;
		}

		// exposed
		public string GetFilenameForCRefOverload(string currentAssemblyName, string cref, string overload)
		{
			// lookup current assembly
			string filename = GetFilenameForId(currentAssemblyName, cref);
//			Debug.WriteLine(string.Format("GetFilenameForCRefOverload('{0}', '{1}') => {2}", currentAssemblyName, cref, filename));
			return filename;

			#region Original XSLT Logic
			/*
					<!--<xsl:choose>
						<xsl:when test="starts-with($cref, 'T:')">
							<xsl:call-template name="get-filename-for-type-name">
								<xsl:with-param name="type-name" select="substring-after($cref, 'T:')" />
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="starts-with($cref, 'M:')">
							<xsl:choose>
								<xsl:when test="contains($cref, '.#c')">
									<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '.#c'), 'M:'), '[,]', ''), 'Constructor', $overload, '.html')" />
								</xsl:when>
								<xsl:when test="contains($cref, '(')">
									<xsl:choose>
										<xsl:when test="string-length($overload) &gt; 0">
											<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'M:'), '[,]', ''), '_overload_', $overload, '.html')" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'M:'), '[,]', ''), '.html')" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="string-length($overload) &gt; 0">
											<xsl:value-of select="concat(translate(substring-after($cref, 'M:'), '[,]', ''), '_overload_', $overload, '.html')" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="concat(translate(substring-after($cref, 'M:'), '[,]', ''), '.html')" />
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:when test="starts-with($cref, 'E:')">
							<xsl:value-of select="concat(translate(substring-after($cref, 'E:'), '[,]', ''), $overload, '.html')" />
						</xsl:when>
						<xsl:when test="starts-with($cref, 'F:')">
							<xsl:variable name="enum" select="/ndoc/assembly/module/namespace//enumeration[field/@id = $cref]" />
							<xsl:choose>
								<xsl:when test="$enum">
									<xsl:call-template name="get-filename-for-type-name">
										<xsl:with-param name="type-name" select="substring-after($enum/@id, 'T:')" />
									</xsl:call-template>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="concat(translate(substring-after($cref, 'F:'), '[,]', ''), $overload, '.html')" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:when test="starts-with($cref, 'P:')">
							<xsl:choose>
								<xsl:when test="contains($cref, '(')">
									<xsl:value-of select="concat(translate(substring-after(substring-before($cref, '('), 'P:'), '[,]', ''), $overload, '.html')" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="concat(translate(substring-after($cref, 'P:'), '[,]', ''), $overload, '.html')" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$cref" />
						</xsl:otherwise>
					</xsl:choose>-->
			*/
			#endregion
		}

		#region BuildNameTables

		private void BuildNameTables(XmlDocument xmlDocumentation)
		{
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocumentation.NameTable);
			nsmgr.AddNamespace("ns", "urn:ndoc-schema");
			XmlNodeList assemblies = xmlDocumentation.SelectNodes("/ns:ndoc/ns:assembly", nsmgr);
			// foreach assembly
			foreach (XmlElement assemblyNode in assemblies) {
				string assemblyName = GetNodeName(assemblyNode);
				RegisterAssembly(assemblyName);

				// build list of assemblyReferences
				XmlNodeList assemblyReferenceNodes = assemblyNode.SelectNodes("ns:assemblyReference", nsmgr);
				List<string> assemblyReferenceNames = new List<string>();
				//				assemblyReferenceNames.Add(assemblyName);
				foreach (XmlNode assemblyReferenceNode in assemblyReferenceNodes) {
					assemblyReferenceNames.Add(GetNodeName(assemblyReferenceNode));
				}
				assemblyReferences.Add(assemblyName, assemblyReferenceNames.ToArray());

				// foreach namespace
				XmlNodeList namespaces = assemblyNode.SelectNodes("ns:module/ns:namespace", nsmgr);
				foreach (XmlElement namespaceNode in namespaces) {
					string namespaceName = GetNodeName(namespaceNode);
					// register namespace
					this.RegisterNamespace(assemblyName, namespaceName);

					XmlNodeList types = namespaceNode.SelectNodes("*[@id]", nsmgr);
					// foreach type
					foreach (XmlElement typeNode in types) {
						string typeId = GetNodeId(typeNode);
						//TODO The rest should also use displayName ((EE): clarify what above line means - shall we remove 'name' attribute then?)
						string typeDisplayName = GetNodeDisplayName(typeNode);
						this.RegisterType(assemblyName, typeId, typeDisplayName);

						// foreach member
						XmlNodeList members = typeNode.SelectNodes("*[@id]");
						foreach (XmlElement memberNode in members) {
							string memberId = GetNodeId(memberNode);
							switch (memberNode.Name) {
								case "constructor": {
										MethodContract contract = XmlUtils.GetAttributeEnum<MethodContract>(memberNode, "contract");
										string overload = XmlUtils.GetAttributeString(memberNode, "overload", false);
										this.RegisterConstructor(assemblyName, typeId, memberId, contract, overload);
									}
									break;
								case "field": {
										bool isEnum = (typeNode.Name == "enumeration");
										string memberName = GetNodeName(memberNode);
										this.RegisterField(assemblyName, typeId, memberId, isEnum, memberName);
									}
									break;
								case "property": {
										string overload = GetNodeOverload(memberNode);
										string memberName = GetNodeName(memberNode);
										this.RegisterProperty(assemblyName, memberId, memberName, overload);
									}
									break;
								case "method": {
										string overload = GetNodeOverload(memberNode);
										string memberName = GetNodeName(memberNode);
										this.RegisterMethod(assemblyName, memberId, memberName, overload);
									}
									break;
								case "operator": {
										string overload = GetNodeOverload(memberNode);
										string memberName = GetNodeName(memberNode);
										this.RegisterOperator(assemblyName, memberId, memberName, overload);
									}
									break;
								case "event": {
										string memberName = GetNodeName(memberNode);
										this.RegisterEvent(assemblyName, memberId, memberName);
									}
									break;
							}
						}
					}
				}
			}
		}

		private void RegisterAssembly(string assemblyName)
		{
			Register(assemblyName, null, assemblyName, CalculateFilenameForId(assemblyName, null, null));
		}

		private void RegisterNamespace(string assemblyName, string namespaceName)
		{
			string namespaceId = "N:" + namespaceName;
			Register(assemblyName, namespaceId, namespaceName, CalculateFilenameForId(assemblyName, "N:" + namespaceName, null));
		}

		private void RegisterType(string assemblyName, string typeId, string displayName)
		{
			Register(assemblyName, typeId, displayName, CalculateFilenameForId(assemblyName, typeId, null));
		}

		private void RegisterConstructor(string assemblyName, string typeId, string id, MethodContract contract, string overload)
		{
			Register(assemblyName, id, GetDisplayNameForId(assemblyName, typeId), CalculateFilenameForId(assemblyName, id, overload));
		}

		private void RegisterOperator(string assemblyName, string memberId, string memberName, string overload)
		{
			Register(assemblyName, memberId, memberName, CalculateFilenameForId(assemblyName, memberId, overload));
		}

		private void RegisterMethod(string assemblyName, string memberId, string memberName, string overload)
		{
			Register(assemblyName, memberId, memberName, CalculateFilenameForId(assemblyName, memberId, overload));
		}

		private void RegisterProperty(string assemblyName, string memberId, string memberName, string overload)
		{
			Register(assemblyName, memberId, memberName, CalculateFilenameForId(assemblyName, memberId, overload));
		}

		private void RegisterField(string assemblyName, string typeId, string memberId, bool isEnum, string memberName)
		{
			if (isEnum) {
				Register(assemblyName, memberId, memberName, GetFilenameForId(assemblyName, typeId));
			} else {
				Register(assemblyName, memberId, memberName, CalculateFilenameForId(assemblyName, memberId, null));
			}
		}

		private void RegisterEvent(string assemblyName, string memberId, string memberName)
		{
			Register(assemblyName, memberId, memberName, CalculateFilenameForId(assemblyName, memberId, null));
		}


		#endregion

		#region Registration & Lookup Logic

		private string GetFilenameForIdInternal(string currentAssemblyName, string memberId)
		{
			string filename = fileNames[currentAssemblyName + memberId];
			if (filename == null) {
				// search for identifier in referenced assemblies
				string[] assemblyReference = this.assemblyReferences[currentAssemblyName];
				if (assemblyReference != null) {
					foreach (string assemblyName in assemblyReference) {
						filename = fileNames[assemblyName + memberId];
						if (filename != null)
							break;
					}
				}
			}

			if (filename == null && memberId != null) {
				filename = fileNames[memberId];
			}

			//			Debug.WriteLine(string.Format("GetFilenameForIdInternal('{0}','{1}') => {2}", currentAssemblyName, memberId, filename));
			return filename;
		}

		private string GetFilenameForIdSpecial(string assemblyName, string memberId, string postfix)
		{
			string fn = GetFilenameForIdInternal(assemblyName, memberId);
			if (fn != null && fn.Length > EXT.Length) {
				fn = fn.Insert(fn.Length - EXT.Length, postfix);
			}
//			Debug.WriteLine(string.Format("GetFilenameForIdSpecial('{0}','{1}') => {2}", assemblyName, memberId, fn));
			return fn;
		}

		private void Register(string assemblyName, string id, string displayName, string fileName)
		{
//			Debug.WriteLine(string.Format("Registering [{0},{1}]=[{2},{3}]", assemblyName, id, displayName, fileName));
			fileNames[assemblyName + id] = fileName;
			elemNames[assemblyName + id] = displayName;
		}

		/// <summary>
		/// of the form "T:XXX", "F:XXX" etc
		/// </summary>
		private string CalculateFilenameForId(string assemblyName, string id, string overload)
		{
			if (id == null)
			{
				return assemblyName + EXT;
			}

//			char idType = '\0';
			int ix = id.IndexOf(':');
//			if (ix > -1) {
//				idType = id[0];
//			}
			id = id.Substring(ix + 1);

			// constructors could be #ctor or #cctor
			//			int ixDotHash = id.IndexOf(".#c"); 
			//			if (ixDotHash > -1)
			//				id = id.Substring(0, ixDotHash);

			// methods could have "("
			int ixLBrace = id.IndexOf("(");
			if (ixLBrace > -1)
				id = id.Substring(0, ixLBrace);

			if (overload != null) {
				id += overload;
			}

			id = id.Replace('#', '~');

			return assemblyName + "~" + id + EXT;
		}

		#endregion

		#region Xml Utility Methods

		private string GetNodeOverload(XmlNode memberNode)
		{
			return XmlUtils.GetAttributeString(memberNode, "overload", false);
		}

		private string GetNodeId(XmlNode node)
		{
			return XmlUtils.GetNodeId(node);
		}

		private string GetNodeType(XmlNode node)
		{
			return XmlUtils.GetNodeType(node);
		}

		private string GetNodeTypeId(XmlNode node)
		{
			return XmlUtils.GetNodeTypeId(node);
		}

		private string GetNodeName(XmlNode node)
		{
			return XmlUtils.GetNodeName(node);
		}

		private string GetNodeDisplayName(XmlNode node)
		{
			return XmlUtils.GetNodeDisplayName(node);
		}

		#endregion
	}
}