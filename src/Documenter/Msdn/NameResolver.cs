using System;
using System.Collections.Specialized;
using NDoc3.Core.Reflection;

namespace NDoc3.Documenter.Msdn
{
	public class NameResolver
	{
		// <param name="fileNames">A StringDictionary holding id to file name mappings.</param>
		// <param name="elemNames">A StringDictionary holding id to element name mappings</param>

		private readonly StringDictionary fileNames = new StringDictionary();
		private readonly StringDictionary elemNames = new StringDictionary();

		public void RegisterNamespace(string assemblyName, string namespaceName)
		{
			string namespaceId = "N:" + namespaceName;
			fileNames[namespaceId] = GenerateFilenameForNamespace(assemblyName, namespaceName);
			elemNames[namespaceId] = namespaceName;
		}

		public void RegisterType(string assemblyName, string typeId, string displayName)
		{
			fileNames[typeId] = GenerateFilenameForType(assemblyName, typeId);
			elemNames[typeId] = displayName;
		}

		public void RegisterConstructor(string assemblyName, string typeId, string id, MethodContract contract, string overload)
		{
			fileNames[id] = GenerateFilenameForConstructor(assemblyName, id, contract, overload);
			elemNames[id] = elemNames[typeId];
		}

		public void RegisterOperator(string assemblyName, string memberId, string memberName, string overload)
		{
			fileNames[memberId] = GenerateFilenameForOperator(assemblyName, memberId, overload);
			elemNames[memberId] = memberName;
		}

		public void RegisterMethod(string assemblyName, string memberId, string memberName, string overload)
		{
			fileNames[memberId] = GenerateFilenameForMethod(assemblyName, memberId, overload);
			elemNames[memberId] = memberName;
		}

		public void RegisterProperty(string assemblyName, string memberId, string memberName, string overload)
		{
			fileNames[memberId] = GenerateFilenameForProperty(assemblyName, memberId, overload);
			elemNames[memberId] = memberName;
		}

		public void RegisterField(string assemblyName, string typeId, string memberId, bool isEnum, string memberName)
		{
			if (isEnum)
				fileNames[memberId] = fileNames[typeId];
			else
				fileNames[memberId] = GenerateFilenameForField(assemblyName, memberId);
			elemNames[memberId] = memberName;
		}

		public void RegisterEvent(string assemblyName, string memberId, string memberName)
		{
			fileNames[memberId] = GenerateFilenameForEvent(assemblyName, memberId);
			elemNames[memberId] = memberName;
		}

		private string GenerateFilenameForNamespace(string assemblyName, string namespaceName)
		{
			string fileName = namespaceName + ".html";
			return fileName;
		}

		private string GenerateFilenameForType(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + ".html";
			return fileName;
		}

		private string GenerateFilenameForConstructor(string assemblyName, string constructorID, MethodContract contract, string overload)
		{
			int dotHash = constructorID.IndexOf(".#"); // constructors could be #ctor or #cctor
			string fileName = constructorID.Substring(2, dotHash - 2);

			if (contract == MethodContract.Static)
				fileName += "Static";

			fileName += "Constructor";

			if (overload != null) {
				fileName += overload;
			}

			fileName += ".html";

			return fileName;
		}

		private string GenerateFilenameForEvent(string assemblyName, string eventID)
		{
			string fileName = eventID.Substring(2) + ".html";
			fileName = fileName.Replace("#",".");
			return fileName;
		}

		private string GenerateFilenameForProperty(string assemblyName, string propertyID, string overload)
		{
			string fileName = propertyID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1) {
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (overload != null) {
				fileName += overload;
			}

			fileName += ".html";
			fileName = fileName.Replace("#", ".");

			return fileName;
		}

		private string GenerateFilenameForField(string assemblyName, string fieldID)
		{
			string fileName = fieldID.Substring(2) + ".html";
			fileName = fileName.Replace("#", ".");
			return fileName;
		}

		private string GenerateFilenameForOperator(string assemblyName, string operatorID, string overload)
		{
			string fileName = operatorID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1) {
				fileName = fileName.Substring(0, leftParenIndex);
			}

			if (overload != null) {
				fileName += "_overload_" + overload;
			}

			fileName += ".html";
			fileName = fileName.Replace("#", ".");

			return fileName;
		}

		private string GenerateFilenameForMethod(string assemblyName, string methodID, string overload)
		{
			string fileName = methodID.Substring(2);

			int leftParenIndex = fileName.IndexOf('(');

			if (leftParenIndex != -1) {
				fileName = fileName.Substring(0, leftParenIndex);
			}

			fileName = fileName.Replace("#", ".");

			if (overload != null) {
				fileName += "_overload_" + overload;
			}

			fileName += ".html";

			return fileName;
		}

		public string GetFilenameForNamespaceHierarchy(string assemblyName, string namespaceName)
		{
			return namespaceName + "Hierarchy.html";
		}

		public string GetFilenameForNamespace(string assemblyName, string namespaceName)
		{
			return namespaceName + ".html";
		}

		public string GetFilenameForId(string assemblyName, string memberId)
		{
			return fileNames[memberId];
		}

		public string GetFilenameForIdHierarchy(string assemblyName, string memberId)
		{
			string fn = fileNames[memberId];
			if (fn == null || fn.Length < 5) return fn;

			fn = fn.Insert(fn.Length - ".html".Length, "Hierarchy");
			return fn;
		}

		public string GetDisplayNameForId(string assemblyName, string memberId)
		{
			return elemNames[memberId];
		}

		public string GetFilenameForFields(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Fields.html";
			return fileName;
		}

		public string GetFilenameForOperators(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Operators.html";
			return fileName;
		}

		public string GetFilenameForMethods(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Methods.html";
			return fileName;
		}

		public string GetFilenameForProperties(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Properties.html";
			return fileName;
		}

		public string GetFilenameForEvents(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Events.html";
			return fileName;
		}

		public string GetFilenameForTypeHierarchy(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Hierarchy.html";
			return fileName;
		}

		public string GetFilenameForTypeMembers(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Members.html";
			return fileName;
		}

		public string GetFilenameForConstructors(string assemblyName, string typeID)
		{
			string fileName = typeID.Substring(2) + "Constructors.html";
			return fileName;
		}

		public string GetFilenameForOperatorOverloads(string assemblyName, string typeID, string operatorName)
		{
			//			string typeID = (string)typeNode.Attributes["id"].Value;
			//			string operatorName = (string)opNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + operatorName + "_overloads.html";
			return fileName;
		}

		public string GetFilenameForPropertyOverloads(string assemblyName, string typeID, string propertyName)
		{
			//			string typeID = (string)typeNode.Attributes["id"].Value;
			//			string propertyName = (string)propertyNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + propertyName + "_overloads.html";
			fileName = fileName.Replace("#", ".");
			return fileName;
		}

		public string GetFilenameForMethodOverloads(string assemblyName, string typeID, string methodName)
		{
			//			string typeID = (string)typeNode.Attributes["id"].Value;
			//			string methodName = (string)methodNode.Attributes["name"].Value;
			string fileName = typeID.Substring(2) + "." + methodName + "_overloads.html";
			return fileName;
		}

		public string GetFilenameForTypename(string assemblyName, string typeName)
		{
			return typeName + ".html";
		}

		public string GetFilenameForCRefOverload(string assemblyName, string cref, string overload)
		{
			// TODO!
			throw new NotImplementedException();
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
		}
	}
}
