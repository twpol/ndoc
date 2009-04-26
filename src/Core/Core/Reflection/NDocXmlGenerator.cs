#region License

// Copyright (C) 2001  Kral Ferch, Jason Diamond
// Parts Copyright (C) 2004  Kevin Downs
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace NDoc3.Core.Reflection
{
	/// <summary>
	/// Summary description for ReflectionEngine.
	/// </summary>
	internal class NDocXmlGenerator : MarshalByRefObject, IDisposable
	{
		public const string NDOCXML_NAMESPACEURI = "urn:ndoc-schema";
		public const string NDOCXML_VERSION = "2.0";

		private readonly NDocXmlGeneratorParameters _rep;
		private readonly IAssemblyLoader _assemblyLoader;
		private readonly AssemblyXmlDocCache _assemblyDocCache;
		private readonly ExternalXmlSummaryCache _externalSummaryCache;
		private readonly Hashtable _notEmptyNamespaces;
		//		private readonly Dictionary<Type, object>_documentedTypes;
		private readonly TypeHierarchy _derivedTypes;
		private readonly TypeHierarchy _interfaceImplementingTypes;
		private readonly NamespaceHierarchyCollection _namespaceHierarchies;
		private readonly TypeHierarchy _baseInterfaces;
		private readonly AttributeUsageDisplayFilter _attributeFilter;

		public NDocXmlGenerator(IAssemblyLoader assemblyLoader, NDocXmlGeneratorParameters rep)
		{
			_rep = rep ?? new NDocXmlGeneratorParameters();
			_assemblyLoader = assemblyLoader ?? new AssemblyLoader();

			foreach (FileInfo assemblyFile in _rep.AssemblyFileNames) {
				// ensure the assembly's path is added to the search list for resolving dependencies
				assemblyLoader.AddSearchDirectory(new ReferencePath(assemblyFile.DirectoryName));
			}

			string DocLangCode = Enum.GetName(typeof(SdkLanguage), _rep.SdkDocLanguage).Replace("_", "-");
			_externalSummaryCache = new ExternalXmlSummaryCache(DocLangCode);

			_assemblyDocCache = new AssemblyXmlDocCache();
			_notEmptyNamespaces = new Hashtable();

			_namespaceHierarchies = new NamespaceHierarchyCollection();
			_baseInterfaces = new TypeHierarchy();
			_derivedTypes = new TypeHierarchy();
			_interfaceImplementingTypes = new TypeHierarchy();
			_attributeFilter = new AttributeUsageDisplayFilter(rep.DocumentedAttributes);

			//			_documentedTypes = new Dictionary<Type, object>();

			PreLoadXmlDocumentation();
		}

		public void Dispose()
		{ }

		private void PreLoadXmlDocumentation()
		{
			//preload all xml documentation
			foreach (FileInfo xmlDocFilename in _rep.SlashDocFileNames) {
				_externalSummaryCache.AddXmlDoc(xmlDocFilename.FullName);
				_assemblyDocCache.CacheDocFile(xmlDocFilename.FullName);
			}
		}

		/// <summary>
		/// Checks if we are using the Mono runtime
		/// </summary>
		/// <returns>
		/// Wether or not we are running with Mono
		/// </returns>
		private static bool IsRunningMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}

		/// <summary>
		/// Validates the given ndoc xml against the schema.
		/// </summary>
		public static void ValidateNDocXml(Uri ndocXmlSource)
		{
			if (!IsRunningMono()) {
				WebRequest req = WebRequest.Create(ndocXmlSource);
				using (WebResponse resp = req.GetResponse()) {
					StreamReader reader = new StreamReader(resp.GetResponseStream());
					using (reader) {
						ValidateNDocXml(reader, true);
					}
				}
			}
		}

		/// <summary>
		/// Validates the given ndoc xml against the schema.
		/// </summary>
		/// <remarks>This method automatically closes the reader instance passed into it.</remarks>
		public static void ValidateNDocXml(FileInfo ndocXmlSource)
		{
			if (!IsRunningMono()) {
				ValidateNDocXml(new StreamReader(ndocXmlSource.OpenRead(), true), true);
			}
		}

		/// <summary>
		/// Validates the given ndoc xml against the schema.
		/// </summary>
		/// <remarks>This method automatically closes the reader instance passed into it.</remarks>
		public static void ValidateNDocXml(TextReader ndocXmlSource)
		{
			if (!IsRunningMono()) {
				ValidateNDocXml(ndocXmlSource, true);
			}
		}

		/// <summary>
		/// Validates the given ndoc xml against the schema.
		/// </summary>
		public static void ValidateNDocXml(TextReader ndocXmlSource, bool autoClose)
		{
			try {
				if (!IsRunningMono())
				{
					XmlTextReader reader = new XmlTextReader(ndocXmlSource);
					reader.WhitespaceHandling = WhitespaceHandling.All;
					ValidateNDocXmlInternal(reader);
				}
			} finally {
				if (autoClose) {
					ndocXmlSource.Close();
				}
			}
		}

		/// <summary>
		/// Validates the generated XML file against the XML schema.
		/// </summary>
		/// <param name="reader">An instance of a XmlReader, containing the XML to be validated.</param>
		/// <exception cref="ValidationException">Occures if validation fails.</exception>
		private static void ValidateNDocXmlInternal(XmlReader reader)
		{
			try
			{
				XmlReader validatingReader = XmlReader.Create(reader, MakeXmlReaderSettings());
				while (validatingReader.Read()) { }
			}
			catch (ValidationException)
			{
//				if (reader is XmlTextReader)
//				{
//					XmlTextReader tr = (XmlTextReader) reader;
//					string offendingText = tr.Value;
//					throw new ValidationException("Offending text:" + offendingText, exception);
//				}
				throw;
			}
		}

		/// <summary>
		/// Generates the XmlReaderSettings for validating against the XML Schema.
		/// </summary>
		/// <returns>The XmlReaderSettings.</returns>
		private static XmlReaderSettings MakeXmlReaderSettings()
		{
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ValidationType = ValidationType.Schema;

			XmlSchemaSet sc = new XmlSchemaSet();
			sc.Add(NDOCXML_NAMESPACEURI, new XmlTextReader(EmbeddedResources.GetEmbeddedResourceStream(MethodBase.GetCurrentMethod().DeclaringType, "reflection.xsd")));
			settings.Schemas = sc;

			settings.ValidationEventHandler += ((sender, args) =>
			{
				if (args.Severity == XmlSeverityType.Error)
				{
					throw new ValidationException(
						string.Format("{0} at ({1},{2})", args.Exception.Message, args.Exception.LineNumber,
									  args.Exception.LinePosition), args.Exception);
				}
			});
			settings.CheckCharacters = false;
			return settings;
		}

		/// <summary>Builds an Xml file combining the reflected metadata with the /doc comments.</summary>
		/// <returns>full pathname of XML file</returns>
		/// <remarks>The caller is responsible for deleting the xml file after use...</remarks>
		public void MakeXmlFile(FileInfo xmlFile)
		{
#if DEBUG
			MakeXmlFile(xmlFile, Formatting.Indented, 2, ' ');
#else
			MakeXmlFile(xmlFile, Formatting.None, 0, ' ');
#endif
		}

		/// <summary>Builds an Xml file combining the reflected metadata with the /doc comments.</summary>
		/// <returns>full pathname of XML file</returns>
		/// <remarks>The caller is responsible for deleting the xml file after use...</remarks>
		public void MakeXmlFile(FileInfo xmlFile, Formatting formatting, int indentation, char indentChar)
		{
			using (XmlTextWriter writer = new XmlTextWriter(new StreamWriter(xmlFile.FullName, false, Encoding.UTF8))) {
				writer.Formatting = formatting;
				writer.IndentChar = indentChar;
				writer.Indentation = indentation;
				BuildXml(writer);
			}

			//HACK Temporary fix until Mono have been released with a fix for the XML validation
			ValidateNDocXml(xmlFile);
		}

		/// <summary>Builds an Xml string combining the reflected metadata with the /doc comments.</summary>
		/// <remarks>This now evidently writes the string in utf-16 format (and
		/// says so, correctly I suppose, in the xml text) so if you write this string to a file with
		/// utf-8 encoding it will be unparseable because the file will claim to be utf-16
		/// but will actually be utf-8.</remarks>
		/// <returns>XML string</returns>
		public string MakeXml()
		{
			StringWriter swriter = new StringWriter();
			using (XmlTextWriter writer = new XmlTextWriter(swriter)) {
				BuildXml(writer);
			}

			string result = swriter.ToString();
			//HACK Temporary fix until Mono have been released with a fix for the XML validation
			if (!IsRunningMono()) {
				ValidateNDocXmlInternal(XmlReader.Create(new StringReader(result), MakeXmlReaderSettings()));
			}

			return result;
		}

		/// <summary>
		/// Builds an Xml file combining the reflected metadata with the /doc comments.
		/// </summary>
		/// <param name="writer">An instance of a XmlWriter</param>
		private void BuildXml(XmlWriter writer)
		{
			int start = Environment.TickCount;
			Debug.WriteLine("Memory before making xml: " + GC.GetTotalMemory(false));

			try {
				PreReflectionProcess();

				// Start the document with the XML declaration tag
				writer.WriteStartDocument();

				// Start the root element
				writer.WriteStartElement("ndoc");
				writer.WriteAttributeString("SchemaVersion", NDOCXML_VERSION);
				writer.WriteAttributeString("xmlns", NDOCXML_NAMESPACEURI);

				if (_rep.FeedbackEmailAddress.Length > 0)
					WriteFeedBackEmailAddress(writer);

				if (_rep.CopyrightText.Length > 0)
					WriteCopyright(writer);

				if (_rep.IncludeDefaultThreadSafety)
					WriteDefaultThreadSafety(writer);

				if (_rep.Preliminary)
					writer.WriteElementString("preliminary", "");

				WriteNamespaceHierarchies(writer);

				foreach (FileInfo assemblyFile in _rep.AssemblyFileNames) {
					IAssemblyInfo assembly = _assemblyLoader.GetAssemblyInfo(assemblyFile);

					int starta = Environment.TickCount;

					WriteAssembly(writer, assembly);

					Trace.WriteLine("Completed " + assembly.FullName);
					Trace.WriteLine(((Environment.TickCount - starta) / 1000.0) + " sec.");
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();

				Trace.WriteLine("MakeXML : " + ((Environment.TickCount - start) / 1000.0) + " sec.");
				// if you want to see NDoc3's intermediate XML file, use the XML documenter.
			}
				//			catch (Exception ex) {
				//				// TODO (EE): reevaluate exception handling
				//				throw;
				//			}
			catch (ReflectionTypeLoadException rtle) {
				StringBuilder sb = new StringBuilder();
				//				if (_assemblyLoader != null && _assemblyLoader.UnresolvedAssemblies.Count > 0) {
				//					sb.Append("One or more required assemblies could not be located : \n");
				//					foreach (string ass in _assemblyLoader.UnresolvedAssemblies) {
				//						sb.AppendFormat("   {0}\n", ass);
				//					}
				//					sb.Append("\nThe following directories were searched, \n");
				//					foreach (string dir in _assemblyLoader.SearchedDirectories) {
				//						sb.AppendFormat("   {0}\n", dir);
				//					}
				//				} else {
				Hashtable fileLoadExceptions = new Hashtable();
				foreach (Exception loaderEx in rtle.LoaderExceptions) {
					System.IO.FileLoadException fileLoadEx = loaderEx as FileLoadException;
					if (fileLoadEx != null) {
						if (!fileLoadExceptions.ContainsKey(fileLoadEx.FileName)) {
							fileLoadExceptions.Add(fileLoadEx.FileName, null);
							sb.Append("Unable to load: " + fileLoadEx.FileName + "\r\n");
						}
					}
					sb.Append(loaderEx.Message + Environment.NewLine);
					sb.Append(loaderEx.StackTrace + Environment.NewLine);
					sb.Append("--------------------" + Environment.NewLine + Environment.NewLine);
					//					}
				}
				throw new DocumenterException(sb.ToString());
			} finally {
				Trace.WriteLine("MakeXML : " + ((Environment.TickCount - start) / 1000.0) + " sec.");
				Debug.WriteLine("Memory after making xml: " + GC.GetTotalMemory(false));
			}
		}


		#region Global Xml Elements

		// writes out the default thead safety settings for the project
		private void WriteDefaultThreadSafety(XmlWriter writer)
		{
			writer.WriteStartElement("threadsafety");
			writer.WriteAttributeString("static", XmlConvert.ToString(_rep.StaticMembersDefaultToSafe));
			writer.WriteAttributeString("instance", XmlConvert.ToString(_rep.InstanceMembersDefaultToSafe));
			writer.WriteEndElement();
		}

		private void WriteFeedBackEmailAddress(XmlWriter writer)
		{
			writer.WriteElementString("feedbackEmail", _rep.FeedbackEmailAddress);
		}

		// writes the copyright node to the documentation
		private void WriteCopyright(XmlWriter writer)
		{
			writer.WriteStartElement("copyright");
			writer.WriteAttributeString("text", _rep.CopyrightText);

			if (_rep.CopyrightHref.Length > 0) {
				if (!_rep.CopyrightHref.StartsWith("http:")) {
					writer.WriteAttributeString("href", Path.GetFileName(_rep.CopyrightHref));
				} else {
					writer.WriteAttributeString("href", _rep.CopyrightHref);
				}
			}

			writer.WriteEndElement();
		}

		#endregion

		#region EditorBrowsable filter

		//checks if the member has been flagged with the
		//EditorBrowsableState.Never value
		private bool IsEditorBrowsable(MemberInfo minfo)
		{
			if (_rep.EditorBrowsableFilter == EditorBrowsableFilterLevel.Off) {
				return true;
			}

			EditorBrowsableAttribute[] browsables =
				Attribute.GetCustomAttributes(minfo, typeof(EditorBrowsableAttribute), false)
				as EditorBrowsableAttribute[];

			if (browsables != null && browsables.Length == 0) {
				return true;
			}
			if (browsables != null) {
				EditorBrowsableAttribute browsable = browsables[0];
				return (browsable.State == EditorBrowsableState.Always) ||
						 ((browsable.State == EditorBrowsableState.Advanced) &&
						  (_rep.EditorBrowsableFilter != EditorBrowsableFilterLevel.HideAdvanced));
			}
			throw new Exception("Unknown exception occured");
		}

		#endregion

		#region MustDocument * filters

		private bool MustDocumentType(Type type)
		{
			Type declaringType = type.DeclaringType;

			//If type name starts with a digit it is not a valid identifier
			//in any of the MS .Net languages.
			//It's probably a J# anonomous inner class...
			//Whatever, do not document it.
			if (Char.IsDigit(type.Name, 0))
				return false;

			if (type.IsGenericType) {
				type = type.GetGenericTypeDefinition();
			}

			//If the type has a CompilerGenerated attribute then we don't want to document it
			//as it is an internal artifact of the compiler
			if (type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false)) {
				return false;
			}

			//exclude types that are internal to the .Net framework.
			if (type.FullName.StartsWith("System.") || type.FullName.StartsWith("Microsoft.")) {
				if (type.IsNotPublic)
					return false;
				if (type.DeclaringType != null &&
					!MustDocumentType(type.DeclaringType))
					return false;
				// There are a group of *public* interfaces in System.Runtime.InteropServices
				// that are not documented by MS and should be considered internal to the framework...
				if (type.IsInterface && type.Namespace == "System.Runtime.InteropServices" && type.Name.StartsWith("_"))
					return false;
			}

			return
				!type.FullName.StartsWith("<PrivateImplementationDetails>") &&
				(declaringType == null || MustDocumentType(declaringType)) &&
				(
				(type.IsPublic) ||
				(type.IsNotPublic && _rep.DocumentInternals) ||
				(type.IsNestedPublic) ||
				(type.IsNestedFamily && _rep.DocumentProtected) ||
				(type.IsNestedFamORAssem && _rep.DocumentProtected) ||
				(type.IsNestedAssembly && _rep.DocumentInternals) ||
				(type.IsNestedFamANDAssem && _rep.DocumentInternals) ||
				(type.IsNestedPrivate && _rep.DocumentPrivates)
				) &&
				IsEditorBrowsable(type) &&
				(!_rep.UseNamespaceDocSummaries || (type.Name != "NamespaceDoc")) &&
				!_assemblyDocCache.HasExcludeTag(type, MemberID.GetMemberID(type));
		}

		private bool MustDocumentMethod(MethodBase method)
		{
			//Ignore MC++ destructor.
			//The __dtor function is just a wrapper that just calls the
			//Finalize method; all code you write in the destructor is
			//actually written to the finalize method. So, we will filter
			//it out of the documentation by default...
			if (method.Name == "__dtor")
				return false;

			//check the basic visibility options
			if (!
				(
				(method.IsPublic) ||
				(method.IsFamily && _rep.DocumentProtected &&
				(_rep.DocumentSealedProtected || !method.ReflectedType.IsSealed)) ||
				(method.IsFamilyOrAssembly && _rep.DocumentProtected) ||
					 (method.ReflectedType.Assembly == method.DeclaringType.Assembly &&
						  ((method.IsAssembly && _rep.DocumentInternals) ||
					 (method.IsFamilyAndAssembly && _rep.DocumentInternals))) ||
				(method.IsPrivate)
				)
				) {
				return false;
			}

			//Exclude Net 2.0 Anonymous Methods
			//These have name starting with "<"
			if (method.Name.StartsWith("<")) {
				return false;
			}

			//Inherited Framework Members
			if ((!_rep.DocumentInheritedFrameworkMembers) &&
				(method.ReflectedType != method.DeclaringType) &&
				(method.DeclaringType.FullName.StartsWith("System.") ||
				method.DeclaringType.FullName.StartsWith("Microsoft."))) {
				return false;
			}


			// Methods containing '.' in their name that aren't constructors are probably
			// explicit interface implementations, we check whether we document those or not.
			if ((method.Name.IndexOf('.') != -1) &&
				(method.Name != ".ctor") &&
				(method.Name != ".cctor") &&
				_rep.DocumentExplicitInterfaceImplementations) {
				int lastIndexOfDot = method.Name.LastIndexOf('.');
				if (lastIndexOfDot != -1) {
					string interfaceName = method.Name.Substring(0, lastIndexOfDot);

					Type interfaceType = method.ReflectedType.GetInterface(interfaceName);

					// Document method if interface is (public) or (isInternal and documentInternal).
					if (interfaceType != null && (interfaceType.IsPublic ||
						(interfaceType.IsNotPublic && _rep.DocumentInternals))) {
						return IsEditorBrowsable(method);
					}
				}
			} else {
				if (method.IsPrivate && !_rep.DocumentPrivates)
					return false;
			}


			//check if the member has an exclude tag
			if (method.DeclaringType != method.ReflectedType) // inherited
            {
				if (_assemblyDocCache.HasExcludeTag(method.DeclaringType, MemberID.GetMemberID(method, true)))
					return false;
			} else {
				if (_assemblyDocCache.HasExcludeTag(method.DeclaringType, MemberID.GetMemberID(method, false)))
					return false;
			}

			return IsEditorBrowsable(method);
		}

		private bool MustDocumentProperty(PropertyInfo property)
		{
			// here we decide if the property is to be documented
			// note that we cannot directly test 'visibility' - it has to
			// be done for both the accessors individualy...
			if (IsEditorBrowsable(property)) {
				MethodInfo getMethod = null;
				MethodInfo setMethod = null;
				if (property.CanRead) {
					try { getMethod = property.GetGetMethod(true); } catch (System.Security.SecurityException) { }
				}
				if (property.CanWrite) {
					try { setMethod = property.GetSetMethod(true); } catch (System.Security.SecurityException) { }
				}

				bool hasGetter = (getMethod != null) && MustDocumentMethod(getMethod);
				bool hasSetter = (setMethod != null) && MustDocumentMethod(setMethod);

				bool IsExcluded;
				//check if the member has an exclude tag
				if (property.DeclaringType != property.ReflectedType) // inherited
                {
					IsExcluded = _assemblyDocCache.HasExcludeTag(property.DeclaringType, MemberID.GetMemberID(property, true));
				} else {
					IsExcluded = _assemblyDocCache.HasExcludeTag(property.DeclaringType, MemberID.GetMemberID(property, false));
				}

				if ((hasGetter || hasSetter)
					&& !IsExcluded)
					return true;
			}
			return false;
		}


		private bool MustDocumentField(FieldInfo field)
		{
			if (!
				(
				(field.IsPublic) ||
				(field.IsFamily && _rep.DocumentProtected &&
				(_rep.DocumentSealedProtected || !field.ReflectedType.IsSealed)) ||
				(field.IsFamilyOrAssembly && _rep.DocumentProtected) ||
					 (field.ReflectedType.Assembly == field.DeclaringType.Assembly &&
					 ((field.IsAssembly && _rep.DocumentInternals) ||
					 (field.IsFamilyAndAssembly && _rep.DocumentInternals))) ||
				(field.IsPrivate && _rep.DocumentPrivates))
				) {
				return false;
			}

			//exclude Net 2.0 Anonymous Method Delegates
			//These have name starting with "<"
			if (field.Name.StartsWith("<")) {
				return false;
			}

			if ((!this._rep.DocumentInheritedFrameworkMembers) &&
				(field.ReflectedType != field.DeclaringType) &&
				(field.DeclaringType.FullName.StartsWith("System.") ||
				field.DeclaringType.FullName.StartsWith("Microsoft."))) {
				return false;
			}

			//check if the member has an exclude tag
			if (field.DeclaringType != field.ReflectedType) // inherited
            {
				if (_assemblyDocCache.HasExcludeTag(field.DeclaringType, MemberID.GetMemberID(field, true)))
					return false;
			} else {
				if (_assemblyDocCache.HasExcludeTag(field.DeclaringType, MemberID.GetMemberID(field, false)))
					return false;
			}

			return IsEditorBrowsable(field);
		}

		#endregion

		#region IsHidden\IsHiding

		private bool IsHidden(MemberInfo member, Type type)
		{
			if (member.DeclaringType == member.ReflectedType)
				return false;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MemberInfo[] members = type.GetMember(member.Name, bindingFlags);
			foreach (MemberInfo m in members) {
				if ((m != member)
					&& m.DeclaringType.IsSubclassOf(member.DeclaringType)) {
					return true;
				}
			}

			return false;
		}

		private bool IsHidden(MethodInfo method, Type type)
		{
			if (method.DeclaringType == method.ReflectedType)
				return false;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MemberInfo[] members = type.GetMember(method.Name, bindingFlags);
			foreach (MemberInfo m in members) {
				if ((m != method)
					&& (m.DeclaringType.IsSubclassOf(method.DeclaringType))
					&& ((m.MemberType != MemberTypes.Method)
					|| HaveSameSig(m as MethodInfo, method))) {
					return true;
				}
			}

			return false;
		}

		private bool IsHiding(MemberInfo member, Type type)
		{
			if (member.DeclaringType != member.ReflectedType)
				return false;

			Type baseType = type.BaseType;
			if (baseType == null)
				return false;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MemberInfo[] members = baseType.GetMember(member.Name, bindingFlags);
			if (members.Length > 0)
				return true;

			return false;
		}

		private bool IsHiding(MethodInfo method, Type type)
		{
			if (method.DeclaringType != method.ReflectedType)
				return false;

			Type baseType = type.BaseType;
			if (baseType == null)
				return false;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MemberInfo[] members = baseType.GetMember(method.Name, bindingFlags);
			foreach (MemberInfo m in members) {
				if (m == method)
					continue;

				if (m.MemberType != MemberTypes.Method)
					return true;

				MethodInfo meth = m as MethodInfo;
				if (HaveSameSig(meth, method)
					&& (((method.Attributes & MethodAttributes.Virtual) == 0)
					|| ((method.Attributes & MethodAttributes.NewSlot) != 0))) {
					return true;
				}
			}

			return false;
		}

		private bool IsHiding(PropertyInfo property, Type type)
		{
			if (!IsHiding((MemberInfo)property, type))
				return false;

			bool isIndexer = (property.Name == "Item");
			if (property == null)
				throw new Exception("Property are null");
			foreach (MethodInfo accessor in property.GetAccessors(true)) {
				if (((accessor.Attributes & MethodAttributes.Virtual) != 0)
					&& ((accessor.Attributes & MethodAttributes.NewSlot) == 0))
					return false;

				// indexers only hide indexers with the same signature
				if (isIndexer && !IsHiding(accessor, type))
					return false;
			}

			return true;
		}

		private bool HaveSameSig(MethodInfo m1, MethodInfo m2)
		{
			ParameterInfo[] ps1 = m1.GetParameters();
			ParameterInfo[] ps2 = m2.GetParameters();

			if (ps1.Length != ps2.Length)
				return false;

			for (int i = 0; i < ps1.Length; i++) {
				ParameterInfo p1 = ps1[i];
				ParameterInfo p2 = ps2[i];
				if (p1.ParameterType != p2.ParameterType)
					return false;
				if (p1.IsIn != p2.IsIn)
					return false;
				if (p1.IsOut != p2.IsOut)
					return false;
				if (p1.IsRetval != p2.IsRetval)
					return false;
			}

			return true;
		}

		#endregion

		#region Assembly

		/// <summary>
		///  WriteAssembly
		///		WriteCustomAttributes
		///		WriteModules
		///
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="assembly"></param>
		private void WriteAssembly(XmlWriter writer, IAssemblyInfo assembly)
		{
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("assembly");
			writer.WriteAttributeString("name", assemblyName.Name);

			if (_rep.AssemblyVersionInfo == AssemblyVersionInformationType.AssemblyVersion) {
				writer.WriteAttributeString("version", assemblyName.Version.ToString());
			}
			if (_rep.AssemblyVersionInfo == AssemblyVersionInformationType.AssemblyFileVersion) {
				AssemblyFileVersionAttribute[] attrs = assembly.GetCustomAttributes<AssemblyFileVersionAttribute>(false);
				if (attrs.Length > 0) {
					string version = attrs[0].Version;
					writer.WriteAttributeString("version", version);
				}
			}

			WriteAssemblyDocumentation(writer, assemblyName);
			WriteAssemblyReferences(writer, assembly);

			WriteCustomAttributes(writer, assembly);

			foreach (IModuleInfo module in assembly.GetModules()) {
				WriteModule(writer, module);
			}

			writer.WriteEndElement(); // assembly
		}

		private void WriteAssemblyReferences(XmlWriter writer, IAssemblyInfo assembly)
		{
			foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies()) {
				writer.WriteStartElement("assemblyReference");
				writer.WriteAttributeString("name", assemblyName.Name);

				writer.WriteEndElement();
			}
		}

		#endregion

		#region Module
		/// <summary>Writes documentation about a module out as XML.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="module">Module to document.</param>
		private void WriteModule(XmlWriter writer, IModuleInfo module)
		{
			writer.WriteStartElement("module");
			writer.WriteAttributeString("name", module.ScopeName);
			WriteCustomAttributes(writer, module);
			WriteNamespaces(writer, module);
			writer.WriteEndElement();
		}
		#endregion

		#region Namespace
		private void WriteNamespaces(XmlWriter writer, IModuleInfo module)
		{
			Type[] types = module.GetTypes();

			StringCollection namespaceNames = GetNamespaceNames(types);

			foreach (string namespaceName in namespaceNames) {
				string ourNamespaceName = string.IsNullOrEmpty(namespaceName) ? "(global)" : namespaceName;

				if (_notEmptyNamespaces.ContainsKey(ourNamespaceName) || _rep.DocumentEmptyNamespaces) {

					string namespaceSummary = null;
					if (_rep.UseNamespaceDocSummaries) {
                        if (ourNamespaceName == "(global)")
							namespaceSummary = _assemblyDocCache.GetDoc(module.AssemblyName, "T:NamespaceDoc");
						else
							namespaceSummary = _assemblyDocCache.GetDoc(module.AssemblyName, "T:" + namespaceName + ".NamespaceDoc");
					}

					bool isNamespaceDoc = false;

					if (string.IsNullOrEmpty(namespaceSummary))
						namespaceSummary = _rep.NamespaceSummaries[ourNamespaceName] as string;
					else
						isNamespaceDoc = true;

					if (_rep.SkipNamespacesWithoutSummaries && string.IsNullOrEmpty(namespaceSummary)) {
						Trace.WriteLine(string.Format("Skipping namespace {0} because it has no summary...", namespaceName));
					} else {
						Trace.WriteLine(string.Format("Writing namespace {0}...", namespaceName));

						writer.WriteStartElement("namespace");
						writer.WriteAttributeString("name", ourNamespaceName);

						if (!string.IsNullOrEmpty(namespaceSummary)) {
							WriteStartDocumentation(writer);

							if (isNamespaceDoc) {
								writer.WriteRaw(namespaceSummary);
							} else {
								writer.WriteStartElement("summary");
								writer.WriteRaw(namespaceSummary);
								writer.WriteEndElement();
							}
							WriteEndDocumentation(writer);
						} else if (_rep.ShowMissingSummaries) {
							WriteStartDocumentation(writer);
							WriteMissingDocumentation(writer, "summary", null, "Missing <summary> Documentation for " + namespaceName);
							WriteEndDocumentation(writer);
						}

						int classCount = WriteClasses(writer, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} classes.", classCount));

						int interfaceCount = WriteInterfaces(writer, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} interfaces.", interfaceCount));

						int structureCount = WriteStructures(writer, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} structures.", structureCount));

						int delegateCount = WriteDelegates(writer, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} delegates.", delegateCount));

						int enumCount = WriteEnumerations(writer, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} enumerations.", enumCount));

						writer.WriteEndElement();
					}
				} else {
					Trace.WriteLine(string.Format("Discarding namespace {0} because it does not contain any documented types.", ourNamespaceName));
				}
			}
		}
		#endregion

		#region TypeCollections
		private int WriteClasses(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types) {
				if (type.IsClass &&
					!IsDelegate(type) &&
					type.Namespace == namespaceName) {
					string typeID = MemberID.GetMemberID(type);
					//					if (!_documentedTypes.ContainsKey(type)) {
					//						_documentedTypes.Add(type, null);
					if (MustDocumentType(type) 
						// && !CheckForMissingTypeDocumentation(type)
						) {
						bool hiding = ((type.MemberType & MemberTypes.NestedType) != 0)
							&& IsHiding(type, type.DeclaringType);
						WriteClass(writer, type, hiding);
						nbWritten++;
					}
					//					} else {
					//						Trace.WriteLine(typeID + " already documented - skipped...");
					//					}
				}
			}

			return nbWritten;
		}

		/// <summary>
		/// Write all interfaces in a module
		/// </summary>
		/// <param name="writer">The XML writer</param>
		/// <param name="types">An array of types in the module</param>
		/// <param name="namespaceName">The name of the namespace currently being documented</param>
		/// <returns>The number of interfaces written</returns>
		private int WriteInterfaces(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types) {
				if (type.IsInterface &&
					type.Namespace == namespaceName &&
					MustDocumentType(type)) {
					string typeID = MemberID.GetMemberID(type);
					//					if (!_documentedTypes.ContainsKey(type)) {
					//						_documentedTypes.Add(type, null);
					WriteInterface(writer, type);
					nbWritten++;
					//					} else {
					//						Trace.WriteLine(typeID + " already documented - skipped...");
					//					}
				}
			}

			return nbWritten;
		}

		private int WriteStructures(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types) {
				if (type.IsValueType &&
					!type.IsEnum &&
					type.Namespace == namespaceName &&
					MustDocumentType(type)) {
					string typeID = MemberID.GetMemberID(type);
					//					if (!_documentedTypes.ContainsKey(type)) {
					//						_documentedTypes.Add(type, null);
					bool hiding = ((type.MemberType & MemberTypes.NestedType) != 0)
						&& IsHiding(type, type.DeclaringType);
					WriteClass(writer, type, hiding);
					nbWritten++;
					//					} else {
					//						Trace.WriteLine(typeID + " already documented - skipped...");
					//					}
				}
			}

			return nbWritten;
		}

		private int WriteDelegates(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types) {
				if (type.IsClass &&
					IsDelegate(type) &&
					type.Namespace == namespaceName &&
					MustDocumentType(type)) {
					string typeID = MemberID.GetMemberID(type);
					//					if (!_documentedTypes.ContainsKey(type)) {
					//						_documentedTypes.Add(type, null);
					WriteDelegate(writer, type);
					nbWritten++;
					//					} else {
					//						Trace.WriteLine(typeID + " already documented - skipped...");
					//					}
				}
			}

			return nbWritten;
		}

		private int WriteEnumerations(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types) {
				if (type.IsEnum &&
					type.Namespace == namespaceName &&
					MustDocumentType(type)) {
					string typeID = MemberID.GetMemberID(type);
					//					if (!_documentedTypes.ContainsKey(type)) {
					//						_documentedTypes.Add(type, null);
					WriteEnumeration(writer, type);
					nbWritten++;
					//					} else {
					//						Trace.WriteLine(typeID + " already documented - skipped...");
					//					}
				}
			}

			return nbWritten;
		}

		private bool IsDelegate(Type type)
		{
			if (type.BaseType == null)
				return false;
			return type.BaseType.FullName == "System.Delegate" ||
				type.BaseType.FullName == "System.MulticastDelegate";
		}

		#endregion

		/// <summary>
		/// Returns the method's index in the list of overloads
		/// </summary>
		private int GetMethodOverloadIndex(MethodInfo method, Type type)
		{
			int currentIndex = 0;
			int overloadIndex = 0;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			int genericArgCount = method.IsGenericMethod ? method.GetGenericArguments().Length : 0;

			MemberInfo[] methods = type.GetMember(method.Name, MemberTypes.Method, bindingFlags);
			foreach (MethodInfo m in methods) {

				int currGenericArgCount = m.IsGenericMethod ? m.GetGenericArguments().Length : 0;
				if (genericArgCount == currGenericArgCount
					&& !IsHidden(m, type)
					&& MustDocumentMethod(m)
					)
				{
					++currentIndex;
				}

				if (m == method) {
					overloadIndex = currentIndex;
				}
			}

			return (currentIndex > 1) ? overloadIndex : 0;
		}

		private int GetPropertyOverload(PropertyInfo property, PropertyInfo[] properties)
		{
			int count = 0;
			int overload = 0;

			foreach (PropertyInfo p in properties) {
				if (p.Name == property.Name) {
					++count;
				}

				if (p == property) {
					overload = count;
				}
			}

			return (count > 1) ? overload : 0;
		}


		#region Types
		/// <summary>Writes XML documenting a class.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Class to document.</param>
		/// <param name="hiding">true if hiding base members</param>
		private void WriteClass(XmlWriter writer, Type type, bool hiding)
		{
			bool isStruct = type.IsValueType;

			string typeId = MemberID.GetMemberID(type);

			string fullNameWithoutNamespace = GetTypeFullName(type).Replace('+', '.');

			if (type.Namespace != null) {
				fullNameWithoutNamespace = fullNameWithoutNamespace.Substring(type.Namespace.Length + 1);
			}

			writer.WriteStartElement(isStruct ? "structure" : "class");
			writer.WriteAttributeString("name", fullNameWithoutNamespace);
			writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(type));
			writer.WriteAttributeString("namespace", type.Namespace);
			writer.WriteAttributeString("id", typeId);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));
			if (hiding) {
				writer.WriteAttributeString("hiding", "true");
			}

			// structs can't be abstract and always derive from System.ValueType
			// so don't bother including those attributes.
			if (!isStruct) {
				if (type.IsAbstract && type.IsSealed) {
					writer.WriteAttributeString("static", "true");
				} else {
					if (type.IsAbstract) {
						writer.WriteAttributeString("abstract", "true");
					} else if (type.IsSealed) {
						writer.WriteAttributeString("sealed", "true");
					}
				}
			}

			WriteTypeDocumentation(writer, type.Assembly.GetName(), typeId);
			WriteCustomAttributes(writer, type);
			//Write base type if there is one and the current "class" isn't a struct
			if (type.BaseType != null && !isStruct)
				WriteBaseType(writer, type.BaseType);
			WriteDerivedTypes(writer, type);

			ImplementsCollection implementations = new ImplementsCollection();

			//build a collection of the base type's interfaces
			//to determine which have been inherited
			StringCollection baseInterfaces = new StringCollection();
			if (type.BaseType != null) {
				foreach (Type baseInterfaceType in type.BaseType.GetInterfaces()) {
					if (baseInterfaceType.IsGenericType)
						baseInterfaces.Add(baseInterfaceType.GetGenericTypeDefinition().FullName);
					else
						baseInterfaces.Add(baseInterfaceType.FullName);
				}
			}

			//Write all implemented interfaces and write
			foreach (Type interfaceType in type.GetInterfaces()) {
				if (MustDocumentType(interfaceType)) {
					Type interType;
					interType = interfaceType.IsGenericType ? interfaceType.GetGenericTypeDefinition() : interfaceType;
					writer.WriteStartElement("implementsClass");
					writer.WriteAttributeString("type", interType.FullName.Replace('+', '.'));
					writer.WriteAttributeString("id", MemberID.GetMemberID(interType));
					writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(interType));
					writer.WriteAttributeString("namespace", interType.Namespace);
					//Check if the interfaces is inherited by basetype
					if (baseInterfaces.Contains(interType.FullName)) {
						writer.WriteAttributeString("inherited", "true");
					}
					WriteGenericArgumentsAndParameters(interType, writer);
					writer.WriteEndElement();

					InterfaceMapping interfaceMap = type.GetInterfaceMap(interfaceType);
					int numberOfMethods = interfaceMap.InterfaceMethods.Length;
					for (int i = 0; i < numberOfMethods; i++) {
						if (interfaceMap.TargetMethods[i] != null) {
							string implementation = interfaceMap.TargetMethods[i].ToString();
							ImplementsInfo implements = new ImplementsInfo();
							implements.InterfaceMethod = interfaceMap.InterfaceMethods[i];
							implements.InterfaceType = interfaceMap.InterfaceType;
							implements.TargetMethod = interfaceMap.TargetMethods[i];
							implements.TargetType = interfaceMap.TargetType;
							implementations[implementation] = implements;
						}
					}
				}
			}


			if (type.IsGenericType) {
				WriteGenericArgumentsAndParameters(type, writer);
				WriteGenericTypeConstraints(type, writer);
			}


			WriteConstructors(writer, type);
			WriteStaticConstructor(writer, type);
			WriteFields(writer, type);
			WriteProperties(writer, type, implementations);
			WriteMethods(writer, type, implementations);
			WriteOperators(writer, type);
			WriteEvents(writer, type, implementations);

			implementations = null;

			writer.WriteEndElement();
		}

		/// <summary>Writes XML documenting an interface.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Interface to document.</param>
		private void WriteInterface(XmlWriter writer, Type type)
		{
			string memberName = MemberID.GetMemberID(type);

			string fullNameWithoutNamespace = type.FullName.Replace('+', '.');

			if (type.Namespace != null) {
				fullNameWithoutNamespace = fullNameWithoutNamespace.Substring(type.Namespace.Length + 1);
			}

			writer.WriteStartElement("interface");
			writer.WriteAttributeString("name", fullNameWithoutNamespace);
			writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(type));
			writer.WriteAttributeString("namespace", type.Namespace);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			WriteTypeDocumentation(writer, type.Assembly.GetName(), memberName);
			WriteCustomAttributes(writer, type);

			WriteDerivedTypes(writer, type);
			WriteImplementsInterfaces(writer, type);

			if (type.IsGenericType)
			{
				WriteGenericArgumentsAndParameters(type, writer);
				WriteGenericTypeConstraints(type, writer);
			}

			WriteInterfaceImplementingTypes(writer, type);

			WriteProperties(writer, type, null);
			WriteMethods(writer, type, null);
			WriteEvents(writer, type, null);

			writer.WriteEndElement();
		}

		private void WriteImplementsInterfaces(XmlWriter writer, Type type)
		{
			foreach (Type interfaceType in type.GetInterfaces()) {
				if (MustDocumentType(interfaceType)) {
					string interName = interfaceType.IsGenericType ? interfaceType.GetGenericTypeDefinition().FullName : interfaceType.FullName;
					writer.WriteStartElement("implements");
					//if(interfaceType.IsGenericType)
					//    writer.WriteAttributeString("type", interfaceType.GetGenericTypeDefinition().FullName.Replace('+', '.'));
					//else
					//    writer.WriteAttributeString("type", interfaceType.FullName.Replace('+', '.'));
					writer.WriteAttributeString("type", interName);
					writer.WriteAttributeString("assembly", interfaceType.Assembly.GetName().Name);
					writer.WriteAttributeString("id", MemberID.GetMemberID(interfaceType));
					WriteGenericArgumentsAndParameters(interfaceType, writer);
					writer.WriteEndElement();
				}
			}
		}

		/// <summary>Writes XML documenting a delegate.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Delegate to document.</param>
		private void WriteDelegate(XmlWriter writer, Type type)
		{
			string memberName = MemberID.GetMemberID(type);

			writer.WriteStartElement("delegate");
			writer.WriteAttributeString("name", GetNestedTypeName(type));
			writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(type));
			writer.WriteAttributeString("namespace", type.Namespace);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));
			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MethodInfo[] methods = type.GetMethods(bindingFlags);
			foreach (MethodInfo method in methods) {
				if (method.Name == "Invoke") {
					Type t = method.ReturnType;
					writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

					WriteReturnType(writer, t);

					WriteDelegateDocumentation(writer, memberName, method);
					WriteCustomAttributes(writer, type);

					foreach (ParameterInfo parameter in method.GetParameters()) {
						WriteParameter(writer, parameter);
					}
				}
			}

			if (type.IsGenericType) {
				WriteGenericArgumentsAndParameters(type, writer);
				WriteGenericTypeConstraints(type, writer);
			}

			writer.WriteEndElement();
		}

		private void WriteReturnType(XmlWriter writer, Type t)
		{
			writer.WriteStartElement("returnType");
			WriteMethodSignatureTypeMetadata(writer, t);
			writer.WriteEndElement();
		}

		private string GetNestedTypeName(Type type)
		{
			int indexOfPlus = type.FullName.IndexOf('+');
			if (indexOfPlus != -1) {
				int lastIndexOfDot = type.FullName.LastIndexOf('.');
				return type.FullName.Substring(lastIndexOfDot + 1).Replace('+', '.');
			} else {
				return type.Name;
			}
		}

		/// <summary>Writes XML documenting an enumeration.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Enumeration to document.</param>
		private void WriteEnumeration(XmlWriter writer, Type type)
		{
			string memberName = MemberID.GetMemberID(type);

			writer.WriteStartElement("enumeration");
			writer.WriteAttributeString("name", GetNestedTypeName(type));
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(type));
			writer.WriteAttributeString("namespace", type.Namespace);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic |
					  BindingFlags.DeclaredOnly;

			if (type.IsDefined(typeof(FlagsAttribute), false)) {
				writer.WriteAttributeString("flags", "true");
			}

			WriteEnumerationDocumentation(writer, type.Assembly.GetName(), memberName);
			WriteCustomAttributes(writer, type);

			foreach (FieldInfo field in type.GetFields(bindingFlags)) {
				// Enums are normally based on Int32, but this is not a CLR requirement.
				// In fact, they may be based on any integer type. The value__ field
				// defines the enum's base type, so we will treat this seperately...
				if (field.Name == "value__") {
					if (field.FieldType.FullName != "System.Int32") {
						WriteBaseType(writer, field.FieldType);
					}
					break;
				}
			}

			foreach (FieldInfo field in type.GetFields(bindingFlags)) {
				// value__ field handled above...
				if (field.Name != "value__") {
					WriteField(
						writer,
						field,
						type,
						IsHiding(field, type));
				}
			}

			writer.WriteEndElement();
		}

		#endregion

		#region Attributes
		private void WriteStructLayoutAttribute(XmlWriter writer, Type type)
		{
			string charSet = null;
			string layoutKind = null;

			if (!_attributeFilter.Show("System.Runtime.InteropServices.StructLayoutAttribute", "CharSet")) {
				// determine if CharSet property should be documented
				if ((type.Attributes & TypeAttributes.AutoClass) == TypeAttributes.AutoClass) {
					charSet = CharSet.Auto.ToString();
				}
				//			//Do not document if default value....
				//			if ((type.Attributes & TypeAttributes.AnsiClass) == TypeAttributes.AnsiClass)
				//			{
				//				charSet = CharSet.Ansi.ToString(CultureInfo.InvariantCulture);
				//			}
				if ((type.Attributes & TypeAttributes.UnicodeClass) == TypeAttributes.UnicodeClass) {
					charSet = CharSet.Unicode.ToString();
				}
			}

			if (!_attributeFilter.Show("System.Runtime.InteropServices.StructLayoutAttribute", "Value")) {
				// determine if Value property should be documented
				//			//Do not document if default value....
				//			if ((type.Attributes & TypeAttributes.AutoLayout) == TypeAttributes.AutoLayout)
				//			{
				//				layoutKind = LayoutKind.Auto.ToString(CultureInfo.InvariantCulture);
				//			}
				if ((type.Attributes & TypeAttributes.ExplicitLayout) == TypeAttributes.ExplicitLayout) {
					layoutKind = LayoutKind.Explicit.ToString();
				}
				if ((type.Attributes & TypeAttributes.SequentialLayout) == TypeAttributes.SequentialLayout) {
					layoutKind = LayoutKind.Sequential.ToString();
				}
			}

			if (charSet == null && layoutKind == null) {
				return;
			}

			// create attribute element
			writer.WriteStartElement("attribute");
			writer.WriteAttributeString("name", "System.Runtime.InteropServices.StructLayoutAttribute");

			if (charSet != null) {
				// create CharSet property element
				writer.WriteStartElement("property");
				writer.WriteAttributeString("name", "CharSet");
				writer.WriteAttributeString("type", "System.Runtime.InteropServices.CharSet");
				writer.WriteAttributeString("value", charSet);
				writer.WriteEndElement();
			}

			if (layoutKind != null) {
				// create Value property element
				writer.WriteStartElement("property");
				writer.WriteAttributeString("name", "Value");
				writer.WriteAttributeString("type", "System.Runtime.InteropServices.LayoutKind");
				writer.WriteAttributeString("value", layoutKind);
				writer.WriteEndElement();
			}

			// end attribute element
			writer.WriteEndElement();

		}

		private void WriteSpecialAttributes(XmlWriter writer, Type type)
		{
			if ((type.Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable) {
				if (_attributeFilter.Show("System.SerializableAttribute")) {
					writer.WriteStartElement("attribute");
					writer.WriteAttributeString("name", "System.SerializableAttribute");
					writer.WriteEndElement(); // attribute
				}
			}

			WriteStructLayoutAttribute(writer, type);
		}

		private void WriteSpecialAttributes(XmlWriter writer, FieldInfo field)
		{
			if ((field.Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized) {
				if (_attributeFilter.Show("System.NonSerializedAttribute")) {
					writer.WriteStartElement("attribute");
					writer.WriteAttributeString("name", "System.NonSerializedAttribute");
					writer.WriteEndElement();
				}
			}
		}

		private void WriteCustomAttributes(XmlWriter writer, IAssemblyInfo assembly)
		{
			WriteCustomAttributes(writer, assembly.GetCustomAttributes<Attribute>(_rep.DocumentInheritedAttributes), "assembly");
		}

		private void WriteCustomAttributes(XmlWriter writer, IModuleInfo module)
		{
			WriteCustomAttributes(writer, module.GetCustomAttributes<Attribute>(_rep.DocumentInheritedAttributes), "module");
		}

		private void WriteCustomAttributes(XmlWriter writer, Type type)
		{
			try {
				WriteSpecialAttributes(writer, type);
				WriteCustomAttributes(writer, type.GetCustomAttributes(_rep.DocumentInheritedAttributes), "");
			} catch (Exception e) {
				TraceErrorOutput("Error retrieving custom attributes for " + MemberID.GetMemberID(type), e);
			}
		}

		private void WriteCustomAttributes(XmlWriter writer, FieldInfo fieldInfo)
		{
			try {
				WriteSpecialAttributes(writer, fieldInfo);
				WriteCustomAttributes(writer, fieldInfo.GetCustomAttributes(_rep.DocumentInheritedAttributes), "");
			} catch (Exception e) {
				TraceErrorOutput("Error retrieving custom attributes for " + MemberID.GetMemberID(fieldInfo, false), e);
			}
		}

		private void WriteCustomAttributes(XmlWriter writer, ConstructorInfo constructorInfo)
		{
			try {
				WriteCustomAttributes(writer, constructorInfo.GetCustomAttributes(_rep.DocumentInheritedAttributes), "");
			} catch (Exception e) {
				TraceErrorOutput("Error retrieving custom attributes for " + MemberID.GetMemberID(constructorInfo, false), e);
			}
		}

		private bool WriteCustomAttributes(XmlWriter writer, MethodInfo methodInfo)
		{
			bool extensionMethod = false;

			try {
				extensionMethod = WriteCustomAttributes(writer, methodInfo.GetCustomAttributes(_rep.DocumentInheritedAttributes), "");
				WriteCustomAttributes(writer, methodInfo.ReturnTypeCustomAttributes.GetCustomAttributes(_rep.DocumentInheritedAttributes), "return");
			} catch (Exception e) {
				TraceErrorOutput("Error retrieving custom attributes for " + MemberID.GetMemberID(methodInfo, false), e);
			}

			return extensionMethod;
		}

		private void WriteCustomAttributes(XmlWriter writer, PropertyInfo propertyInfo)
		{
			try {
				WriteCustomAttributes(writer, propertyInfo.GetCustomAttributes(_rep.DocumentInheritedAttributes), "");
			} catch (Exception e) {
				TraceErrorOutput("Error retrieving custom attributes for " + MemberID.GetMemberID(propertyInfo, false), e);
			}
		}

		private void WriteCustomAttributes(XmlWriter writer, ParameterInfo parameterInfo)
		{
			try
			{
				object[] customAttributes = parameterInfo.GetCustomAttributes(_rep.DocumentInheritedAttributes);
				WriteCustomAttributes(writer, customAttributes, "");
			}
			catch (Exception e) {
				TraceErrorOutput("Error retrieving custom attributes for " + parameterInfo.Member.ReflectedType.FullName + "." + parameterInfo.Member.Name + " param " + parameterInfo.Name, e);
			}
		}

		private void WriteCustomAttributes(XmlWriter writer, EventInfo eventInfo)
		{
			try {
				WriteCustomAttributes(writer, eventInfo.GetCustomAttributes(_rep.DocumentInheritedAttributes), "");
			} catch (Exception e) {
				TraceErrorOutput("Error retrieving custom attributes for " + MemberID.GetMemberID(eventInfo, false), e);
			}
		}

		private bool WriteCustomAttributes(XmlWriter writer, object[] attributes, string target)
		{
			bool extensionMethod = false;

			foreach (Attribute attribute in attributes) {
				//If the serializable attribute have been shown already
				//don't show it again
				if (_attributeFilter.Show("System.SerializableAttribute") && attribute.GetType().FullName == "System.SerializableAttribute")
					continue;

				//Handle extension methods
				if (attribute.GetType().FullName == "System.Runtime.CompilerServices.ExtensionAttribute") {
					extensionMethod = true;
				}

				if (_rep.DocumentAttributes) {
					if (MustDocumentType(attribute.GetType()) && _attributeFilter.Show(attribute.GetType().FullName)) {
						WriteCustomAttribute(writer, attribute, target);
					}
				}
				if (attribute.GetType().FullName == "System.ObsoleteAttribute") {
					writer.WriteElementString("obsolete", ((ObsoleteAttribute)attribute).Message);
				}
			}

			return extensionMethod;
		}

		private void WriteCustomAttribute(XmlWriter writer, Attribute attribute, string target)
		{
			writer.WriteStartElement("attribute");
			string fullName = attribute.GetType().FullName;
			writer.WriteAttributeString("name", fullName);
			if (target.Length > 0) {
				writer.WriteAttributeString("target", target);
			}

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Public;

			foreach (FieldInfo field in attribute.GetType().GetFields(bindingFlags)) {
				if (MustDocumentField(field) && _attributeFilter.Show(fullName, field.Name)) {
					string fieldValue;
					try {
						fieldValue = GetDisplayValue(field.DeclaringType, field.GetValue(attribute));
					} catch (Exception e) {
						TraceErrorOutput("Value for attribute field " + MemberID.GetMemberID(field, false).Substring(2) + " cannot be determined", e);
						fieldValue = "***UNKNOWN***";
					}
					if (fieldValue.Length > 0) {
						writer.WriteStartElement("field");
						writer.WriteAttributeString("name", field.Name);
						writer.WriteAttributeString("type", field.FieldType.FullName);
						writer.WriteAttributeString("value", fieldValue);
						writer.WriteEndElement(); // field
					}
				}
			}

			foreach (PropertyInfo property in attribute.GetType().GetProperties(bindingFlags)) {
				//skip the TypeId property
				if ((!_rep.ShowTypeIdInAttributes) && (property.Name == "TypeId")) {
					continue;
				}

				if (MustDocumentProperty(property) && _attributeFilter.Show(fullName, property.Name)) {
					if (property.CanRead) {
						string propertyValue;
						try {
							propertyValue = GetDisplayValue(property.DeclaringType, property.GetValue(attribute, null));
						} catch (Exception e) {
							TraceErrorOutput("Value for attribute property " + MemberID.GetMemberID(property, false).Substring(2) + " cannot be determined", e);
							propertyValue = "***UNKNOWN***";
						}
						if (propertyValue.Length > 0) {
							writer.WriteStartElement("property");
							writer.WriteAttributeString("name", property.Name);
							writer.WriteAttributeString("type", property.PropertyType.FullName);
							writer.WriteAttributeString("value", propertyValue);
							writer.WriteEndElement(); // property
						}
					}
				}
			}

			writer.WriteEndElement(); // attribute
		}

		#endregion

		#region MemberCollections

		private void WriteConstructors(XmlWriter writer, Type type)
		{
			int overload = 0;

			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			if (!_rep.DocumentInheritedMembers) {
				bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
			}

			ConstructorInfo[] constructors = type.GetConstructors(bindingFlags);

			if (constructors.Length > 1) {
				overload = 1;
			}

			foreach (ConstructorInfo constructor in constructors) {
				if (MustDocumentMethod(constructor)) {
					WriteConstructor(writer, constructor, overload++);
				}
			}
		}

		private void WriteStaticConstructor(XmlWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			if (!_rep.DocumentInheritedMembers) {
				bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
			}

			ConstructorInfo[] constructors = type.GetConstructors(bindingFlags);

			foreach (ConstructorInfo constructor in constructors) {
				if (MustDocumentMethod(constructor)) {
					WriteConstructor(writer, constructor, 0);
				}
			}
		}

		private void WriteFields(XmlWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			if (!_rep.DocumentInheritedMembers) {
				bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
			}

			FieldInfo[] fields = type.GetFields(bindingFlags);
			foreach (FieldInfo field in fields) {
				if (MustDocumentField(field)
					&& !IsAlsoAnEvent(field)
					&& !IsHidden(field, type)) {
					WriteField(
						writer,
						field,
						type,
						IsHiding(field, type));
				}
			}
		}

		private void WriteProperties(XmlWriter writer, Type type, ImplementsCollection implementations)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			if (!_rep.DocumentInheritedMembers) {
				bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
			}

			PropertyInfo[] properties = type.GetProperties(bindingFlags);

			foreach (PropertyInfo property in properties) {
				if (MustDocumentProperty(property)
					&& !IsAlsoAnEvent(property)
					&& !IsHidden(property, type)
					) {
					WriteProperty(
						writer,
						property,
						property.DeclaringType.FullName != type.FullName,
						GetPropertyOverload(property, properties),
						IsHiding(property, type), implementations);
				}

			}
		}

		private void WriteMethods(XmlWriter writer, Type type, ImplementsCollection implementations)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			if (!_rep.DocumentInheritedMembers) {
				bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
			}

			MethodInfo[] methods = type.GetMethods(bindingFlags);

			foreach (MethodInfo method in methods) {
				string name = method.Name;

				int lastIndexOfDot = name.LastIndexOf('.');

				if (lastIndexOfDot != -1) {
					name = method.Name.Substring(lastIndexOfDot + 1);
				}

				if (
					!(
						method.IsSpecialName &&
											(
											name.StartsWith("get_") ||
											name.StartsWith("set_") ||
											name.StartsWith("add_") ||
											name.StartsWith("remove_") ||
											name.StartsWith("raise_") ||
											name.StartsWith("op_")
											)
						) 
						&& MustDocumentMethod(method) 
						&& !IsHidden(method, type) 
						//&& !CheckForMissingMethodDocumentation(method)
					) {
					WriteMethod(
						writer,
						method,
						method.DeclaringType.FullName != type.FullName,
						GetMethodOverloadIndex(method, type),
						IsHiding(method, type), implementations);
				}
			}
		}

		private void WriteOperators(XmlWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			if (!_rep.DocumentInheritedMembers) {
				bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
			}

			MethodInfo[] methods = type.GetMethods(bindingFlags);
			foreach (MethodInfo method in methods) {
				if (method.Name.StartsWith("op_") &&
					MustDocumentMethod(method)) {
					WriteOperator(
						writer,
						method,
						GetMethodOverloadIndex(method, type));
				}
			}
		}

		private void WriteEvents(XmlWriter writer, Type type, ImplementsCollection implementations)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			if (!this._rep.DocumentInheritedMembers) {
				bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
			}

			EventInfo[] events = type.GetEvents(bindingFlags);
			foreach (EventInfo eventInfo in events) {
				bool IsExcluded;
				//check if the event has an exclude tag
				if (eventInfo.DeclaringType != eventInfo.ReflectedType) // inherited
                {
					IsExcluded = _assemblyDocCache.HasExcludeTag(eventInfo.DeclaringType, MemberID.GetMemberID(eventInfo, true));
				} else {
					IsExcluded = _assemblyDocCache.HasExcludeTag(eventInfo.DeclaringType, MemberID.GetMemberID(eventInfo, false));
				}

				if (!IsExcluded) {
					MethodInfo addMethod = eventInfo.GetAddMethod(true);

					if (addMethod != null &&
						MustDocumentMethod(addMethod) &&
						IsEditorBrowsable(eventInfo)) {
						WriteEvent(writer, eventInfo, implementations);
					}
				}
			}
		}


		private bool IsAlsoAnEvent(Type type, string fullName)
		{
			bool isEvent = false;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic |
					  BindingFlags.DeclaredOnly;

			EventInfo[] events = type.GetEvents(bindingFlags);
			foreach (EventInfo eventInfo in events) {
				if (eventInfo.EventHandlerType != null) {
					if (eventInfo.EventHandlerType.FullName == fullName) {
						isEvent = true;
						break;
					}
				} else {
					throw new Exception("EventInfo are null");
				}
			}

			return isEvent;
		}

		private bool IsAlsoAnEvent(FieldInfo field)
		{
			return IsAlsoAnEvent(field.DeclaringType, field.FieldType.FullName);
		}

		private bool IsAlsoAnEvent(PropertyInfo property)
		{
			return IsAlsoAnEvent(property.DeclaringType, property.PropertyType.FullName);
		}

		/// <summary>
		/// Writes the XML structure for the base type of a class
		/// </summary>
		/// <param name="writer">The XML writer</param>
		/// <param name="type">The type of the basetype</param>
		private void WriteBaseType(XmlWriter writer, Type type)
		{
			if (!"System.Object".Equals(type.FullName)) {
				writer.WriteStartElement("baseType");
				writer.WriteAttributeString("name", type.Name);
				writer.WriteAttributeString("id", MemberID.GetMemberID(type));
				writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(type));
				writer.WriteAttributeString("namespace", type.Namespace);
				writer.WriteAttributeString("assembly", type.Assembly.GetName().Name);

				WriteGenericArgumentsAndParameters(type, writer);

				WriteBaseType(writer, type.BaseType);

				writer.WriteEndElement();
			}
		}

		#endregion

		#region Members

		/// <summary>Writes XML documenting a field.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="field">Field to document.</param>
		/// <param name="type">Type containing the field.</param>
		/// <param name="hiding">true if hiding base members</param>
		private void WriteField(XmlWriter writer, FieldInfo field, Type type, bool hiding)
		{
			string memberName = MemberID.GetMemberID(field, false);

			writer.WriteStartElement("field");
			writer.WriteAttributeString("name", field.Name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetFieldAccessValue(field));

			if (field.IsStatic) {
				writer.WriteAttributeString("contract", "Static");
			} else {
				writer.WriteAttributeString("contract", "Normal");
			}

			Type t = field.FieldType;

//			writer.WriteAttributeString("typeId", MemberID.GetMemberID(t));

//			if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
//				writer.WriteAttributeString("nullable", "true");
//
//			writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

			bool inherited = (field.DeclaringType != field.ReflectedType);

			if (!IsMemberSafe(field))
				writer.WriteAttributeString("unsafe", "true");

			if (hiding) {
				writer.WriteAttributeString("hiding", "true");
			}

			if (field.IsInitOnly) {
				writer.WriteAttributeString("initOnly", "true");
			}

			if (field.IsLiteral) {
				writer.WriteAttributeString("literal", "true");
				string fieldValue = null;
				try {
					fieldValue = GetDisplayValue(field.DeclaringType, field.GetValue(null));
				} catch (Exception e) {
					TraceErrorOutput("Literal value for " + memberName.Substring(2) + " cannot be determined", e);
				}
				if (fieldValue != null) {
					writer.WriteAttributeString("value", fieldValue);
				}
			}
			if (inherited) {
				WriteDeclaringType( field, writer);
			}
//			if (t.IsGenericType && t.GetGenericTypeDefinition() != typeof(Nullable<>)) {
//				WriteGenericArgumentsAndParameters(t, writer);
//			}

			WriteMethodSignatureTypeMetadata(writer, t);

			if (inherited) {
				WriteInheritedDocumentation(writer, field.DeclaringType.Assembly.GetName(), memberName, field.DeclaringType);
			} else {
				WriteFieldDocumentation(writer, type.Assembly.GetName(), memberName, type);
			}
			WriteCustomAttributes(writer, field);

			writer.WriteEndElement();
		}

		/// <summary>
		/// Write the declaring type of a member to the XML file
		/// </summary>
		/// <param name="member">The name of the declaring type</param>
		/// <param name="writer">The XML writer</param>
		private void WriteDeclaringType(MemberInfo member, XmlWriter writer)
		{
			writer.WriteAttributeString("declaringType", MemberID.GetDeclaringTypeName(member));
			writer.WriteAttributeString("declaringAssembly", MemberID.GetDeclaringAssemblyName(member));
			writer.WriteAttributeString("declaringId", MemberID.GetDeclaringMemberID(member));
//
//			writer.WriteAttributeString("declaringType", type);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		protected string GetDisplayValue(Type parent, object value)
		{
			if (value == null)
				return "null";

			if (value is string) {
				return (value.ToString());
			}

			if (value is Enum) {
				if (parent.IsEnum) {
					return Enum.Format(value.GetType(), value, "d");
				}
				string enumTypeName = value.GetType().Name;
				string enumValue = value.ToString();
				string[] enumValues = enumValue.Split(new[] { ',' });
				if (enumValues.Length > 1) {
					for (int i = 0; i < enumValues.Length; i++) {
						enumValues[i] = enumTypeName + "." + enumValues[i].Trim();
					}
					return "(" + String.Join("|", enumValues) + ")";
				}
				return enumTypeName + "." + enumValue;
			}

			return value.ToString();

		}

		/// <summary>Writes XML documenting an event.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="eventInfo">Event to document.</param>
		/// <param name="implementations">Holds a mapping of interface members to concrete implementations</param>
		private void WriteEvent(XmlWriter writer, EventInfo eventInfo, ImplementsCollection implementations)
		{
			string memberName = MemberID.GetMemberID(eventInfo, false);

			string name = eventInfo.Name;
			string interfaceName = null;

			int lastIndexOfDot = name.LastIndexOf('.');
			if (lastIndexOfDot != -1) {
				//this is an explicit interface implementation. if we don't want
				//to document them, get out of here quick...
				if (!_rep.DocumentExplicitInterfaceImplementations)
					return;

				interfaceName = name.Substring(0, lastIndexOfDot);
				lastIndexOfDot = interfaceName.LastIndexOf('.');
				if (lastIndexOfDot != -1)
					name = name.Substring(lastIndexOfDot + 1);

				//check if we want to document this interface.
				ImplementsInfo implements = null;
				MethodInfo adder = eventInfo.GetAddMethod(true);
				if (adder != null) {
					implements = implementations[adder.ToString()];
				}
				if (implements == null) {
					MethodInfo remover = eventInfo.GetRemoveMethod(true);
					if (remover != null) {
						implements = implementations[remover.ToString()];
					}
				}
				if (implements != null)
					return;
			}

			writer.WriteStartElement("event");
			writer.WriteAttributeString("name", name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetMethodAccessValue(eventInfo.GetAddMethod(true)));
			writer.WriteAttributeString("contract", GetEnumString(GetMethodContractValue(eventInfo.GetAddMethod(true))));
			Type t = eventInfo.EventHandlerType;
			writer.WriteAttributeString("typeId", MemberID.GetMemberID(t));
			if (t != null)
				writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());
			else
				throw new Exception("Eventhandler type are null");

			bool inherited = eventInfo.DeclaringType != eventInfo.ReflectedType;

			if (inherited) {
				WriteDeclaringType( eventInfo, writer );
//				writer.WriteAttributeString("declaringType", MemberID.GetDeclaringTypeName(eventInfo));
//				writer.WriteAttributeString("declaringAssembly", MemberID.GetDeclaringAssemblyName(eventInfo));
			}

			if (interfaceName != null) {
				writer.WriteAttributeString("interface", interfaceName);
			}

			if (eventInfo.IsMulticast) {
				writer.WriteAttributeString("multicast", "true");
			}

			if (inherited) {
				WriteInheritedDocumentation(writer, eventInfo.DeclaringType.Assembly.GetName(), memberName, eventInfo.DeclaringType);
			} else {
				WriteEventDocumentation(writer, eventInfo.DeclaringType.Assembly.GetName(), memberName, true);
			}
			WriteCustomAttributes(writer, eventInfo);

			if (implementations != null) {
				ImplementsInfo implements = null;
				MethodInfo adder = eventInfo.GetAddMethod(true);
				if (adder != null) {
					implements = implementations[adder.ToString()];
				}
				if (implements == null) {
					MethodInfo remover = eventInfo.GetRemoveMethod(true);
					if (remover != null) {
						implements = implementations[remover.ToString()];
					}
				}
				if (implements != null) {
					writer.WriteStartElement("implements");
					MemberInfo InterfaceMethod = implements.InterfaceMethod;
					EventInfo InterfaceEvent =
						InterfaceMethod.DeclaringType.GetEvent(InterfaceMethod.Name.Substring(4));
					writer.WriteAttributeString("name", InterfaceEvent.Name);
					writer.WriteAttributeString("id", MemberID.GetMemberID(InterfaceEvent, false));
					writer.WriteAttributeString("interface", implements.InterfaceType.Name);
					writer.WriteAttributeString("interfaceId", MemberID.GetMemberID(implements.InterfaceType));
					writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName.Replace('+', '.'));
					writer.WriteAttributeString("assembly", implements.InterfaceType.Assembly.GetName().Name);
					writer.WriteEndElement();
				}
			}

			writer.WriteEndElement();
		}

		/// <summary>Writes XML documenting a constructor.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="constructor">Constructor to document.</param>
		/// <param name="overload">If &gt; 0, indicates this is the nth overloaded constructor.</param>
		private void WriteConstructor(XmlWriter writer, ConstructorInfo constructor, int overload)
		{
			string memberName = MemberID.GetMemberID(constructor, false);

			writer.WriteStartElement("constructor");
			writer.WriteAttributeString("name", constructor.Name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetMethodAccessValue(constructor));
			writer.WriteAttributeString("contract", GetEnumString(GetMethodContractValue(constructor)));

			if (overload > 0) {
				writer.WriteAttributeString("overload", overload.ToString());
			}

			if (!IsMemberSafe(constructor))
				writer.WriteAttributeString("unsafe", "true");

			WriteConstructorDocumentation(writer, constructor.DeclaringType.Assembly.GetName(), memberName, constructor);
			WriteCustomAttributes(writer, constructor);

			foreach (ParameterInfo parameter in constructor.GetParameters()) {
				WriteParameter(writer, parameter);
			}

			writer.WriteEndElement();
		}

		/// <summary>Writes XML documenting a property.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="property">Property to document.</param>
		/// <param name="inherited">true if a declaringType attribute should be included.</param>
		/// <param name="overload">If &gt; 0, indicates this it the nth overloaded method with the same name.</param>
		/// <param name="hiding">true if this property is hiding base class members with the same name.</param>
		/// <param name="implementations">Holds a mapping of interface members to concrete implementations</param>
		private void WriteProperty(XmlWriter writer, PropertyInfo property, bool inherited, int overload, bool hiding, ImplementsCollection implementations)
		{
			if (property != null) {
				string memberName = MemberID.GetMemberID(property, false);

				string name = property.Name;
				string interfaceName = null;

				MethodInfo getter = property.GetGetMethod(true);
				MethodInfo setter = property.GetSetMethod(true);

				int lastIndexOfDot = name.LastIndexOf('.');
				if (lastIndexOfDot != -1) {
					//this is an explicit interface implementation. if we don't want
					//to document them, get out of here quick...
					if (!_rep.DocumentExplicitInterfaceImplementations)
						return;

					interfaceName = name.Substring(0, lastIndexOfDot);
					lastIndexOfDot = interfaceName.LastIndexOf('.');
					if (lastIndexOfDot != -1)
						name = name.Substring(lastIndexOfDot + 1);

					//check if we want to document this interface.
					ImplementsInfo implements = null;
					if (getter != null) {
						implements = implementations[getter.ToString()];
					}
					if (implements == null) {
						if (setter != null) {
							implements = implementations[setter.ToString()];
						}
					}
					if (implements == null)
						return;
				}

				writer.WriteStartElement("property");
				writer.WriteAttributeString("name", name);
				writer.WriteAttributeString("id", memberName);
				writer.WriteAttributeString("access", GetPropertyAccessValue(property));
				writer.WriteAttributeString("contract", GetPropertyContractValue(property));
				Type t = property.PropertyType;
//				writer.WriteAttributeString("typeId", MemberID.GetMemberID(t));
//				if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
//					writer.WriteAttributeString("nullable", "true");
//				writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

				if (inherited) {
					WriteDeclaringType(property, writer);
//					writer.WriteAttributeString("declaringType", MemberID.GetDeclaringTypeName(property));
				}

				if (overload > 0) {
					writer.WriteAttributeString("overload", overload.ToString());
				}

				if (!IsMemberSafe(property))
					writer.WriteAttributeString("unsafe", "true");

				if (hiding) {
					writer.WriteAttributeString("hiding", "true");
				}

				if (interfaceName != null) {
					writer.WriteAttributeString("interface", interfaceName);
				}

				writer.WriteAttributeString("get", getter != null ? GetMethodAccessValue(getter) : "false");
				writer.WriteAttributeString("set", setter != null ? GetMethodAccessValue(setter) : "false");

				WriteMethodSignatureTypeMetadata(writer, t);

				WriteCustomAttributes(writer, property);
				if (getter != null)
				{
					WriteCustomAttributes(writer, getter.ReturnTypeCustomAttributes.GetCustomAttributes(true), "return");
				}

				foreach (ParameterInfo parameter in GetIndexParameters(property))
				{
					WriteParameter(writer, parameter);
				}

				if (inherited)
				{
					WriteInheritedDocumentation(writer, property.DeclaringType.Assembly.GetName(), memberName, property.DeclaringType);
				} else {
					WritePropertyDocumentation(writer, property.DeclaringType.Assembly.GetName(), memberName, property, true);
				}

				if (implementations != null) {
					ImplementsInfo implements = null;
					if (getter != null) {
						implements = implementations[getter.ToString()];
					}
					if (implements == null) {
						if (setter != null) {
							implements = implementations[setter.ToString()];
						}
					}
					if (implements != null) {
						MethodInfo InterfaceMethod = (MethodInfo)implements.InterfaceMethod;
						PropertyInfo InterfaceProperty = DerivePropertyFromAccessorMethod(InterfaceMethod);
						if (InterfaceProperty != null) {
							string InterfacePropertyID = MemberID.GetMemberID(InterfaceProperty, false);
							writer.WriteStartElement("implements");
							writer.WriteAttributeString("name", InterfaceProperty.Name);
							writer.WriteAttributeString("id", InterfacePropertyID);
							writer.WriteAttributeString("interface", implements.InterfaceType.Name);
							writer.WriteAttributeString("assembly", implements.InterfaceType.Assembly.GetName().Name);
							writer.WriteAttributeString("interfaceId", MemberID.GetMemberID(implements.InterfaceType));
							if (implements.InterfaceType.IsGenericType)
								writer.WriteAttributeString("declaringType", implements.InterfaceType.GetGenericTypeDefinition().FullName.Replace('+', '.'));
							else
								writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName.Replace('+', '.'));
							writer.WriteEndElement();
						} else if (InterfaceMethod != null) {
							string InterfaceMethodID = MemberID.GetMemberID(InterfaceMethod, false);
							writer.WriteStartElement("implements");
							writer.WriteAttributeString("name", InterfaceMethod.Name);
							writer.WriteAttributeString("id", InterfaceMethodID);
							writer.WriteAttributeString("interface", implements.InterfaceType.Name);
							writer.WriteAttributeString("assembly", implements.InterfaceType.Assembly.GetName().Name);
							writer.WriteAttributeString("interfaceId", MemberID.GetMemberID(implements.InterfaceType));
							string declaringType;
							if (implements.InterfaceType.GetGenericArguments().Length > 0)
								declaringType = implements.InterfaceType.GetGenericTypeDefinition().FullName.Replace('+', '.');
							else
								declaringType = implements.InterfaceType.FullName.Replace('+', '.');
							writer.WriteAttributeString("declaringType", declaringType);
							writer.WriteEndElement();
						}
					}
				}

//				if (t.IsGenericType && t.GetGenericTypeDefinition() != typeof(Nullable<>)) {
//					WriteGenericArgumentsAndParameters(t, writer);
//				}

				writer.WriteEndElement();
			}
		}

		private PropertyInfo DerivePropertyFromAccessorMethod(MemberInfo accessor)
		{
			MethodInfo accessorMethod = (MethodInfo)accessor;
			string accessortype = accessorMethod.Name.Substring(0, 3);
			string propertyName = accessorMethod.Name.Substring(4);

			ParameterInfo[] parameters;
			parameters = accessorMethod.GetParameters();
			int parmCount = parameters.GetLength(0);

			Type returnType = null;
			Type[] types = null;

			if (accessortype == "get") {
				returnType = accessorMethod.ReturnType;
				types = new Type[parmCount];
				for (int i = 0; i < parmCount; i++) {
					types[i] = ((ParameterInfo)parameters.GetValue(i)).ParameterType;
				}
			} else {
				returnType = ((ParameterInfo)parameters.GetValue(parmCount - 1)).ParameterType;
				parmCount--;
				types = new Type[parmCount];
				for (int i = 0; i < parmCount; i++) {
					types[i] = ((ParameterInfo)parameters.GetValue(i + 1)).ParameterType;
				}
			}

			PropertyInfo derivedProperty = accessorMethod.DeclaringType.GetProperty(propertyName, returnType, types);
			return derivedProperty;
		}

		private string GetPropertyContractValue(PropertyInfo property)
		{
			if (property != null)
				return GetEnumString(GetMethodContractValue(property.GetAccessors(true)[0]));
			else
				throw new Exception("PropertyInfo are null");
		}

		private ParameterInfo[] GetIndexParameters(PropertyInfo property)
		{
			// The ParameterInfo[] returned by GetIndexParameters()
			// contains ParameterInfo objects with empty names so
			// we have to get the parameters from the getter or
			// setter instead.

			ParameterInfo[] parameters;
			int length = 0;

			if (property.GetGetMethod(true) != null) {
				parameters = property.GetGetMethod(true).GetParameters();

				if (parameters != null) {
					length = parameters.Length;
				}
			} else {
				parameters = property.GetSetMethod(true).GetParameters();

				if (parameters != null) {
					// If the indexer only has a setter, we neet
					// to subtract 1 so that the value parameter
					// isn't displayed.

					length = parameters.Length - 1;
				}
			}

			ParameterInfo[] result = new ParameterInfo[length];

			if (length > 0) {
				for (int i = 0; i < length; ++i) {
					result[i] = parameters[i];
				}
			}

			return result;
		}

		/// <summary>Writes XML documenting a method.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="method">Method to document.</param>
		/// <param name="inherited">true if a declaringType attribute should be included.</param>
		/// <param name="overload">If &gt; 0, indicates this it the nth overloaded method with the same name.</param>
		/// <param name="hiding">true if this method hides methods of the base class with the same signature.</param>
		/// <param name="implementations">Holds a mapping of interface members to concrete implementations</param>
		private void WriteMethod(XmlWriter writer, MethodInfo method, bool inherited, int overload, bool hiding, ImplementsCollection implementations)
		{
			if (method != null) {
				string methodId = MemberID.GetMemberID(method, false);

				string name = method.Name;
				string interfaceName = null;

				name = name.Replace('+', '.');
				int lastIndexOfDot = name.LastIndexOf('.');
				if (lastIndexOfDot != -1) {
					//this is an explicit interface implementation. if we don't want
					//to document them, get out of here quick...
					if (!this._rep.DocumentExplicitInterfaceImplementations)
						return;

					interfaceName = name.Substring(0, lastIndexOfDot);
					lastIndexOfDot = interfaceName.LastIndexOf('.');
					if (lastIndexOfDot != -1)
						name = name.Substring(lastIndexOfDot + 1);

					//check if we want to document this interface.
					ImplementsInfo implements = implementations[method.ToString()];
					if (implements == null)
						return;
				}

				string displayName = name;
				if (method.IsGenericMethod)
				{
					Type[] genericTypeArgs = method.GetGenericArguments();
					if (genericTypeArgs.Length > 0)
					{
						// language-agnostic display format uses () for generic args
						displayName += "(" + genericTypeArgs[0].Name;
						for(int ix=1;ix<genericTypeArgs.Length;ix++)
						{
							name += "," + genericTypeArgs[ix].Name;
						}
						displayName += ")";
//						name = name + "``" + genericTypeArgs.Length;
					}
				}

				writer.WriteStartElement("method");
				writer.WriteAttributeString("name", displayName);
				writer.WriteAttributeString("displayName", displayName);
				writer.WriteAttributeString("id", methodId);
				writer.WriteAttributeString("access", GetMethodAccessValue(method));
				writer.WriteAttributeString("contract", GetEnumString(GetMethodContractValue(method)));
				Type t = method.ReturnType;
				writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());
				if (inherited) {
					WriteDeclaringType(method, writer);
				}

				if (overload > 0) {
					writer.WriteAttributeString("overload", overload.ToString());
				}

				if (!IsMemberSafe(method))
					writer.WriteAttributeString("unsafe", "true");

				if (hiding) {
					writer.WriteAttributeString("hiding", "true");
				}

				if (interfaceName != null) {
					writer.WriteAttributeString("interface", interfaceName);
				}

				WriteReturnType(writer, t);

				if (inherited) {
					WriteInheritedDocumentation(writer, method.DeclaringType.Assembly.GetName(), methodId, method.DeclaringType);
				} else {
					WriteMethodDocumentation(writer, method.DeclaringType.Assembly.GetName(), methodId, method, true);
				}

				bool extensionMethod = WriteCustomAttributes(writer, method);

				foreach (ParameterInfo parameter in method.GetParameters()) {
					WriteParameter(writer, parameter, extensionMethod);
				}

				if (implementations != null) {
					ImplementsInfo implements = implementations[method.ToString()];
					if (implements != null) {
						writer.WriteStartElement("implements");
						writer.WriteAttributeString("name", implements.InterfaceMethod.Name);
						writer.WriteAttributeString("id", MemberID.GetMemberID((MethodInfo)implements.InterfaceMethod, false));
						writer.WriteAttributeString("interface", MemberDisplayName.GetMemberDisplayName(implements.InterfaceType));
						writer.WriteAttributeString("interfaceId", MemberID.GetMemberID(implements.InterfaceType));
						writer.WriteAttributeString("assembly", implements.InterfaceType.Assembly.GetName().Name);
						writer.WriteAttributeString("declaringType", GetTypeFullName(implements.InterfaceType).Replace('+', '.'));
						writer.WriteEndElement();
					}
				}

				if (method.IsGenericMethod) {
					WriteGenericArgumentsAndParametersMethod(method, writer);
					WriteGenericMethodConstraints(method, writer);
				}

				writer.WriteEndElement();
			}
		}

		private void WriteParameter(XmlWriter writer, ParameterInfo parameter)
		{
			WriteParameter(writer, parameter, false);
		}

		private void WriteParameter(XmlWriter writer, ParameterInfo parameter, bool extensionMethod)
		{
			string direction = "in";
			bool isParamArray = false;

			if (parameter.ParameterType.IsByRef) {
				direction = parameter.IsOut ? "out" : "ref";
			}

			if (parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0) {
				isParamArray = true;
			}

			writer.WriteStartElement("parameter");
			writer.WriteAttributeString("name", parameter.Name);

			Type parameterType = parameter.ParameterType;
			if (parameterType.IsByRef)
			{
				parameterType = parameterType.GetElementType();
			}

			if (extensionMethod)
				writer.WriteAttributeString("extension", "true");

			if (parameterType.IsPointer)
				writer.WriteAttributeString("unsafe", "true");

			if (parameter.IsOptional)
			{
				writer.WriteAttributeString("optional", "true");
				if (parameter.DefaultValue != null)
				{
					writer.WriteAttributeString("defaultValue", parameter.DefaultValue.ToString());
				}
				else
				{
					//assuming this is only for VB syntax
					writer.WriteAttributeString("defaultValue", "Nothing");
				}
			}

			if (direction != "in")
			{
				writer.WriteAttributeString("direction", direction);
			}

			if (isParamArray)
			{
				writer.WriteAttributeString("isParamArray", "true");
			}

			WriteMethodSignatureTypeMetadata(writer, parameterType);

			WriteCustomAttributes(writer, parameter);

			writer.WriteEndElement();
		}

		private void WriteMethodSignatureTypeMetadata(XmlWriter writer, Type methodSignatureType)
		{
			Type elementType = MemberID.DereferenceType(methodSignatureType);
			writer.WriteAttributeString("typeId", MemberID.GetMemberID(elementType));
			writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(elementType));
			writer.WriteAttributeString("namespace", MemberID.GetTypeNamespace(elementType));
			writer.WriteAttributeString("assembly", elementType.Assembly.GetName().Name);

			bool nullable = elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Nullable<>);
			writer.WriteAttributeString("nullable", nullable.ToString().ToLower());

			writer.WriteAttributeString("valueType", methodSignatureType.IsValueType.ToString().ToLower());

			if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() != typeof(Nullable<>))
			{
				WriteGenericArgumentsAndParameters(elementType, writer);
			}
			WriteArrayRank(writer, methodSignatureType);
		}

		private void WriteArrayRank(XmlWriter writer, Type type)
		{
			if (type.IsByRef)
			{
				type = type.GetElementType();
			}
			if (type.IsArray)
			{
				writer.WriteStartElement("array");
				writer.WriteAttributeString("rank", type.GetArrayRank().ToString());
				WriteArrayRank(writer, type.GetElementType());
				writer.WriteEndElement();
			}
		}

		private void WriteOperator(XmlWriter writer, MethodInfo method, int overload)
		{
			if (method != null) {
				string memberName = MemberID.GetMemberID(method, false);

				writer.WriteStartElement("operator");
				writer.WriteAttributeString("name", method.Name);
				writer.WriteAttributeString("id", memberName);
				writer.WriteAttributeString("access", GetMethodAccessValue(method));
				writer.WriteAttributeString("contract", GetEnumString(GetMethodContractValue(method)));
				Type t = method.ReturnType;
				writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

				bool inherited = method.DeclaringType != method.ReflectedType;

				if (inherited) {
					WriteDeclaringType(method, writer);
//					writer.WriteAttributeString("declaringType", MemberID.GetDeclaringTypeName(method));
				}

				if (overload > 0) {
					writer.WriteAttributeString("overload", overload.ToString());
				}

				if (!IsMemberSafe(method))
					writer.WriteAttributeString("unsafe", "true");

				WriteReturnType(writer, t);
//				writer.WriteStartElement("returnType");
//				writer.WriteAttributeString("type", MemberID.GetTypeName(t));
//				writer.WriteAttributeString("id", MemberID.GetMemberID(t));
//				if (t.IsGenericType) {
//					WriteGenericArgumentsAndParameters(t, writer);
//				}
//				writer.WriteEndElement();

				if (inherited) {
					WriteInheritedDocumentation(writer, method.DeclaringType.Assembly.GetName(), memberName, method.DeclaringType);
				} else {
					WriteMethodDocumentation(writer, method.DeclaringType.Assembly.GetName(), memberName, method, true);
				}

				WriteCustomAttributes(writer, method);

				foreach (ParameterInfo parameter in method.GetParameters()) {
					WriteParameter(writer, parameter);
				}

				writer.WriteEndElement();
			}
		}


		#endregion

		#region IsMemberSafe
		private bool IsMemberSafe(FieldInfo field)
		{
			return !field.FieldType.IsPointer;
		}

		private bool IsMemberSafe(PropertyInfo property)
		{
			return !property.PropertyType.IsPointer;
		}

		private bool IsMemberSafe(MethodBase method)
		{
			foreach (ParameterInfo parameter in method.GetParameters()) {
				if (parameter.GetType().IsPointer)
					return false;
			}
			return true;
		}

		private bool IsMemberSafe(MethodInfo method)
		{
			if (method.ReturnType.IsPointer)
				return false;

			return IsMemberSafe((MethodBase)method);
		}
		#endregion

		#region Get MemberID of base of inherited member

		private string GetTypeFullName(Type type)
		{
			if (type.IsByRef)
				type = type.GetElementType();

			while(type.IsArray)
				type = type.GetElementType();

			if (type.FullName != null)
			{
				return type.FullName;
			}

			return type.Namespace + "." + type.Name;
		}

		/// <summary>Used by GetFullNamespaceName(MemberInfo member) functions to build
		/// up most of the /doc member name.</summary>
		/// <param name="type"></param>
		private string GetTypeNamespaceName(Type type)
		{
			if (type.IsGenericType)
				type = type.GetGenericTypeDefinition();
			return GetTypeFullName(type).Replace('+', '.');
		}

//		/// <summary>Derives the member name ID for the base of an inherited field.</summary>
//		/// <param name="field">The field to derive the member name ID from.</param>
//		/// <param name="declaringType">The declaring type.</param>
//		private string GetMemberName(FieldInfo field, Type declaringType)
//		{
//			return "F:" + GetTypeFullName(declaringType).Replace("+", ".") + "." + field.Name;
//		}
//
//		/// <summary>Derives the member name ID for an event. Used to match nodes in the /doc XML.</summary>
//		/// <param name="eventInfo">The event to derive the member name ID from.</param>
//		/// <param name="declaringType">The declaring type.</param>
//		private string GetMemberName(EventInfo eventInfo, Type declaringType)
//		{
//			return "E:" + GetTypeFullName(declaringType).Replace("+", ".") +
//				"." + eventInfo.Name.Replace('.', '#').Replace('+', '#');
//		}
//
//		/// <summary>Derives the member name ID for the base of an inherited property.</summary>
//		/// <param name="property">The property to derive the member name ID from.</param>
//		/// <param name="declaringType">The declaring type.</param>
//		private string GetMemberName(PropertyInfo property, Type declaringType)
//		{
//			string memberID = MemberID.GetMemberID(property);
//
//			//extract member type (T:, P:, etc.)
//			string memberType = memberID.Substring(0, 2);
//
//			//extract member name
//			int i = memberID.IndexOf('(');
//			string memberName;
//			if (i > -1) {
//				memberName = memberID.Substring(memberID.LastIndexOf('.', i) + 1);
//			} else {
//				memberName = memberID.Substring(memberID.LastIndexOf('.') + 1);
//			}
//
//			//the member id in the declaring type
//			string key = memberType + GetTypeNamespaceName(declaringType) + "." + memberName;
//			return key;
//		}
//
//		/// <summary>Derives the member name ID for the basse of an inherited member function.</summary>
//		/// <param name="method">The method to derive the member name ID from.</param>
//		/// <param name="declaringType">The declaring type.</param>
//		private string GetMemberName(MethodBase method, Type declaringType)
//		{
//			string memberID = MemberID.GetMemberID(method);
//
//			//extract member type (T:, P:, etc.)
//			string memberType = memberID.Substring(0, 2);
//
//			//extract member name
//			int i = memberID.IndexOf('(');
//			string memberName;
//			if (i > -1) {
//				memberName = memberID.Substring(memberID.LastIndexOf('.', i) + 1);
//			} else {
//				memberName = memberID.Substring(memberID.LastIndexOf('.') + 1);
//			}
//
//			//the member id in the declaring type
//			string key = memberType + GetTypeNamespaceName(declaringType) + "." + memberName;
//			return key;
//		}

		#endregion

		#region Generics

		/// <summary>
		/// Writes generic arguments and parameters for a method
		/// </summary>
		/// <param name="m">Generic method</param>
		/// <param name="writer">XMLWriter to write the XML</param>
		private void WriteGenericArgumentsAndParametersMethod(MethodInfo m, XmlWriter writer)
		{
			foreach (Type t in m.GetGenericArguments()) {
				WriteGenericArgument(writer, t);
//				writer.WriteStartElement("genericargument");
//				string typeId = MemberID.GetMemberID(t);
//				writer.WriteAttributeString("name", t.Name);
//				writer.WriteAttributeString("typeId", typeId);
//				writer.WriteAttributeString("assembly", t.Assembly.GetName().Name);
//				if (t.IsGenericType)
//					WriteGenericArgumentsAndParameters(t, writer);
//				writer.WriteEndElement();
			}
		}

		/// <summary>
		/// Writes generic arguments and parameters for a type
		/// </summary>
		/// <param name="type">Generic type</param>
		/// <param name="writer">XMLWriter to write the XML</param>
		private void WriteGenericArgumentsAndParameters(Type type, XmlWriter writer)
		{
			foreach (Type t in type.GetGenericArguments()) {
				WriteGenericArgument(writer, t);
			}
		}

		private void WriteGenericArgument(XmlWriter writer, Type t)
		{
			writer.WriteStartElement("genericargument");
			writer.WriteAttributeString("name", t.Name);
			// is it a concrete type? if, write additional type information
			if (!t.IsGenericParameter)
			{
				writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(t));
				writer.WriteAttributeString("namespace", MemberID.GetTypeNamespace(t));
				writer.WriteAttributeString("typeId", MemberID.GetMemberID(t));
				writer.WriteAttributeString("assembly", t.Assembly.GetName().Name);
			}
			else
			{
				writer.WriteAttributeString("displayName", t.Name);
			}

			if (t.IsGenericType)
			{
				if (t.GetGenericTypeDefinition() == typeof(Nullable<>))
					writer.WriteAttributeString("nullable", "true");
				else
					WriteGenericArgumentsAndParameters(t, writer);
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Contains information about constraints on a generic parameter template
		/// </summary>
		private struct Constraint
		{
			/// <summary>
			/// Name of the parameter
			/// </summary>
			public string typeparam;
			/// <summary>
			/// List of constraints on the parameter
			/// </summary>
			public List<string> constraint;
		}

		/// <summary>
		/// Write constraints for a generic type
		/// </summary>
		/// <param name="type">The generic type</param>
		/// <param name="writer">The current XMLWriter</param>
		private void WriteGenericTypeConstraints(Type type, XmlWriter writer)
		{
			WriteGenericConstraints(type.GetGenericTypeDefinition().GetGenericArguments(), writer);
		}

		/// <summary>
		/// Write constrains for a generic method
		/// </summary>
		/// <param name="m">The generic method</param>
		/// <param name="writer">The current XMLWriter</param>
		private void WriteGenericMethodConstraints(MethodInfo m, XmlWriter writer)
		{
			WriteGenericConstraints(m.GetGenericMethodDefinition().GetGenericArguments(), writer);
		}

		/// <summary>
		/// Writes generic constraints
		/// </summary>
		/// <param name="args">An array of generic arguments of a type or method</param>
		/// <param name="writer">The current XMLWriter</param>
		private void WriteGenericConstraints(Type[] args, XmlWriter writer)
		{
			List<Constraint> constraintList = new List<Constraint>();
			foreach (Type t in args) {
				GenericParameterAttributes constraints = t.GenericParameterAttributes
					 & GenericParameterAttributes.SpecialConstraintMask;
				Type[] specificType = t.GetGenericParameterConstraints();
				Constraint c = new Constraint();
				List<string> cons = new List<string>();
				//If any constraints are present
				if (constraints != GenericParameterAttributes.None || specificType.Length > 0)
					c.typeparam = t.Name;
				//struct constraint
				if ((constraints & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
					cons.Add("struct");
				//class constraint
				if ((constraints & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
					cons.Add("class");
				//Specific classes, interfaces or other template constraint
				for (int i = 0; i < specificType.Length; i++) {
					string name;
					//If name is ValueType ignore this, as this is present when struct is a constraint
					if ((name = specificType[i].FullName) != "ValueType") {
						// FullName of a type can be null, if there is a constraint on a type of other generic parameter
						if (name != null) {
							cons.Add(name);
						} else {
							cons.Add(specificType[i].Name);
						}
					}
				}
				//new() constraint always comes last
				if ((constraints & GenericParameterAttributes.NotNullableValueTypeConstraint) == 0
					 && (constraints & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
					cons.Add("new");
				if (c.typeparam != String.Empty) {
					c.constraint = cons;
					constraintList.Add(c);
				}
			}

			foreach (Constraint c in constraintList) {
				if (c.constraint.Count > 0) {
					writer.WriteStartElement("genericconstraints");
					writer.WriteAttributeString("param", c.typeparam);
					foreach (string s in c.constraint) {
						writer.WriteElementString("constraint", s);
					}
					writer.WriteEndElement();
				}
			}
		}

		#endregion

		#region Enumeration Values

		private string GetTypeAccessValue(Type type)
		{
			string result = "Unknown";

			switch (type.Attributes & TypeAttributes.VisibilityMask) {
				case TypeAttributes.Public:
					result = "Public";
					break;
				case TypeAttributes.NotPublic:
					result = "NotPublic";
					break;
				case TypeAttributes.NestedPublic:
					result = "NestedPublic";
					break;
				case TypeAttributes.NestedFamily:
					result = "NestedFamily";
					break;
				case TypeAttributes.NestedFamORAssem:
					result = _rep.DocumentProtectedInternalAsProtected ? "NestedFamily" : "NestedFamilyOrAssembly";
					break;
				case TypeAttributes.NestedAssembly:
					result = "NestedAssembly";
					break;
				case TypeAttributes.NestedFamANDAssem:
					result = "NestedFamilyAndAssembly";
					break;
				case TypeAttributes.NestedPrivate:
					result = "NestedPrivate";
					break;
			}

			return result;
		}

		private string GetFieldAccessValue(FieldInfo field)
		{
			string result = "Unknown";

			switch (field.Attributes & FieldAttributes.FieldAccessMask) {
				case FieldAttributes.Public:
					result = "Public";
					break;
				case FieldAttributes.Family:
					result = "Family";
					break;
				case FieldAttributes.FamORAssem:
					result = _rep.DocumentProtectedInternalAsProtected ? "Family" : "FamilyOrAssembly";
					break;
				case FieldAttributes.Assembly:
					result = "Assembly";
					break;
				case FieldAttributes.FamANDAssem:
					result = "FamilyAndAssembly";
					break;
				case FieldAttributes.Private:
					result = "Private";
					break;
				case FieldAttributes.PrivateScope:
					result = "PrivateScope";
					break;
			}

			return result;
		}

		private string GetPropertyAccessValue(PropertyInfo property)
		{
			MethodInfo method = property.GetGetMethod(true) ?? property.GetSetMethod(true);

			return GetMethodAccessValue(method);
		}

		private string GetMethodAccessValue(MethodBase method)
		{
			string result;

			switch (method.Attributes & MethodAttributes.MemberAccessMask) {
				case MethodAttributes.Public:
					result = "Public";
					break;
				case MethodAttributes.Family:
					result = "Family";
					break;
				case MethodAttributes.FamORAssem:
					result = _rep.DocumentProtectedInternalAsProtected ? "Family" : "FamilyOrAssembly";
					break;
				case MethodAttributes.Assembly:
					result = "Assembly";
					break;
				case MethodAttributes.FamANDAssem:
					result = "FamilyAndAssembly";
					break;
				case MethodAttributes.Private:
					result = "Private";
					break;
				case MethodAttributes.PrivateScope:
					result = "PrivateScope";
					break;
				default:
					result = "Unknown";
					break;
			}

			return result;
		}

		private MethodContract GetMethodContractValue(MethodBase method)
		{
			MethodContract result;
			MethodAttributes methodAttributes = method.Attributes;

			if ((methodAttributes & MethodAttributes.Static) > 0) {
				result = MethodContract.Static;
			} else if ((methodAttributes & MethodAttributes.Abstract) > 0) {
				result = MethodContract.Abstract;
			} else if ((methodAttributes & MethodAttributes.Final) > 0) {
				result = MethodContract.Final;
			} else if ((methodAttributes & MethodAttributes.Virtual) > 0) {
				if ((methodAttributes & MethodAttributes.NewSlot) > 0) {
					result = MethodContract.Virtual;
				} else {
					result = MethodContract.Override;
				}
			} else {
				result = MethodContract.Normal;
			}

			return result;
		}

		#endregion

		private StringCollection GetNamespaceNames(Type[] types)
		{
			StringCollection namespaceNames = new StringCollection();

			foreach (Type type in types) {
				if (namespaceNames.Contains(type.Namespace) == false) {
					namespaceNames.Add(type.Namespace);
				}
			}

			return namespaceNames;
		}


		#region Missing Documentation

//		/// <summary>
//		/// Checks for missing type documentation
//		/// </summary>
//		/// <param name="type">The type to check</param>
//		/// <returns>True if documentation is missing</returns>
//		private bool CheckForMissingTypeDocumentation(Type type)
//		{
//			if (_assemblyDocCache.GetDoc(type.Assembly.GetName(), MemberID.GetMemberID(type)) == null) {
//				Trace.WriteLine(String.Format("The type {0} isn't documented and therefore skipped", MemberID.GetMemberID(type)));
//				return true;
//			}
//			return false;
//		}
//
//		private bool CheckForMissingMethodDocumentation(MethodInfo methodInfo)
//		{
//			string memberId = MemberID.GetMemberID(methodInfo);
//			if (_assemblyDocCache.GetDoc(methodInfo.DeclaringType.Assembly.GetName(), memberId) == null) {
//				Trace.WriteLine(String.Format("The method {0} isn't documented and therefore skipped", memberId));
//				return true;
//			}
//			return false;
//		}

		private void CheckForMissingSummaryAndRemarks(
			XmlWriter writer,
			AssemblyName assemblyName,
			string memberId)
		{
			if (this._rep.ShowMissingSummaries) {
				bool bMissingSummary = true;
				string xmldoc = _assemblyDocCache.GetDoc(assemblyName, memberId);

				if (xmldoc != null) {
					XmlTextReader reader = new XmlTextReader(xmldoc, XmlNodeType.Element, null);
					while (reader.Read()) {
						if (reader.NodeType == XmlNodeType.Element) {
							if (reader.Name == "summary") {
								string summarydetails = reader.ReadInnerXml();
								if (summarydetails.Length > 0 && !summarydetails.Trim().StartsWith("Summary description for")) {
									bMissingSummary = false;
									break;
								}
							}
						}
					}
				}

				if (bMissingSummary) {
					WriteMissingDocumentation(writer, "summary", null,
						memberId == null ? "Missing <summary> documentation" : "Missing <summary> documentation for " + memberId);
				}
			}

			if (_rep.ShowMissingRemarks) {
				bool bMissingRemarks = true;
				string xmldoc = _assemblyDocCache.GetDoc(assemblyName, memberId);

				if (xmldoc != null) {
					XmlTextReader reader = new XmlTextReader(xmldoc, XmlNodeType.Element, null);
					while (reader.Read()) {
						if (reader.NodeType == XmlNodeType.Element) {
							if (reader.Name == "remarks") {
								string remarksdetails = reader.ReadInnerXml();
								if (remarksdetails.Length > 0) {
									bMissingRemarks = false;
									break;
								}
							}
						}
					}
				}

				if (bMissingRemarks) {
					WriteMissingDocumentation(writer, "remarks", null,
						"Missing <remarks> documentation for " + memberId);
				}
			}
		}

		private void CheckForMissingParams(
			XmlWriter writer,
			AssemblyName assemblyName,
			string memberName,
			ParameterInfo[] parameters)
		{
			if (_rep.ShowMissingParams) {
				string xmldoc = _assemblyDocCache.GetDoc(assemblyName, memberName);
				foreach (ParameterInfo parameter in parameters) {
					bool bMissingParams = true;

					if (xmldoc != null) {
						XmlTextReader reader = new XmlTextReader(xmldoc, XmlNodeType.Element, null);
						while (reader.Read()) {
							if (reader.NodeType == XmlNodeType.Element) {
								if (reader.Name == "param") {
									string name = reader.GetAttribute("name");
									if (name == parameter.Name) {
										string paramsdetails = reader.ReadInnerXml();
										if (paramsdetails.Length > 0) {
											bMissingParams = false;
											break; // we can stop if we locate what we are looking for
										}
									}
								}
							}
						}
					}

					if (bMissingParams) {
						WriteMissingDocumentation(writer, "param", parameter.Name,
							"Missing <param> documentation for " + parameter.Name);
					}
				}
			}
		}

		private void CheckForMissingReturns(
			XmlWriter writer,
			AssemblyName assemblyName,
			string memberName,
			MethodInfo method)
		{
			if (_rep.ShowMissingReturns &&
				!"System.Void".Equals(method.ReturnType.FullName)) {
				string xmldoc = _assemblyDocCache.GetDoc(assemblyName, memberName);
				bool bMissingReturns = true;

				if (xmldoc != null) {
					XmlTextReader reader = new XmlTextReader(xmldoc, XmlNodeType.Element, null);
					while (reader.Read()) {
						if (reader.NodeType == XmlNodeType.Element) {
							if (reader.Name == "returns") {
								string returnsdetails = reader.ReadInnerXml();
								if (returnsdetails.Length > 0) {
									bMissingReturns = false;
									break; // we can stop if we locate what we are looking for
								}
							}
						}
					}
				}

				if (bMissingReturns) {
					WriteMissingDocumentation(writer, "returns", null,
						"Missing <returns> documentation for " + memberName);
				}
			}
		}

		private void CheckForMissingValue(
			XmlWriter writer,
			AssemblyName assemblyName,
			string memberName)
		{
			if (_rep.ShowMissingValues) {
				string xmldoc = _assemblyDocCache.GetDoc(assemblyName, memberName);
				bool bMissingValues = true;

				if (xmldoc != null) {
					XmlTextReader reader = new XmlTextReader(xmldoc, XmlNodeType.Element, null);
					while (reader.Read()) {
						if (reader.NodeType == XmlNodeType.Element) {
							if (reader.Name == "value") {
								string valuesdetails = reader.ReadInnerXml();
								if (valuesdetails.Length > 0) {
									bMissingValues = false;
									break; // we can stop if we locate what we are looking for
								}
							}
						}
					}
				}

				if (bMissingValues) {
					WriteMissingDocumentation(writer, "values", null,
						"Missing <values> documentation for " + memberName);
				}
			}
		}

		private void WriteMissingDocumentation(
			XmlWriter writer,
			string element,
			string name,
			string message)
		{
			WriteStartDocumentation(writer);

			writer.WriteStartElement(element);

			if (name != null) {
				writer.WriteAttributeString("name", name);
			}

			writer.WriteStartElement("span");
			writer.WriteAttributeString("class", "missing");
			writer.WriteString(message);
			writer.WriteEndElement();

			writer.WriteEndElement();
		}

		#endregion


		#region Write Documentation

		private bool didWriteStartDocumentation;

		private void WriteStartDocumentation(XmlWriter writer)
		{
			if (!didWriteStartDocumentation) {
				writer.WriteStartElement("documentation");
				didWriteStartDocumentation = true;
			}
		}

		private void WriteEndDocumentation(XmlWriter writer)
		{
			if (didWriteStartDocumentation) {
				writer.WriteEndElement();
				didWriteStartDocumentation = false;
			}
		}

		private void WriteSlashDocElements(XmlWriter writer, AssemblyName assemblyName, string memberName)
		{
			string temp = _assemblyDocCache.GetDoc(assemblyName, memberName);
			if (temp != null) {
				WriteStartDocumentation(writer);
				writer.WriteRaw(temp);
			}
		}

		private void WriteInheritedDocumentation(
			XmlWriter writer,
			AssemblyName assemblyName,
			string memberName,
			Type declaringType)
		{
			if (declaringType.GetGenericArguments().Length > 0)
				declaringType = declaringType.GetGenericTypeDefinition();
			string summary = _externalSummaryCache.GetSummary(memberName, declaringType);
			if (summary.Length > 0) {
				WriteStartDocumentation(writer);
				writer.WriteRaw(summary);
				WriteEndDocumentation(writer);
			}
		}

		private void WriteAssemblyDocumentation(XmlWriter writer, AssemblyName assemblyName)
		{
			if (_rep.UseNamespaceDocSummaries) {
				string assemblySummary = _assemblyDocCache.GetDoc(assemblyName, "T:AssemblyDoc");
				if (!string.IsNullOrEmpty(assemblySummary)) {
					WriteStartDocumentation(writer);
					writer.WriteRaw(assemblySummary);
					WriteEndDocumentation(writer);
				}
			}
//
//			CheckForMissingSummaryAndRemarks(writer, assemblyName, null);
//			WriteSlashDocElements(writer, assemblyName, null);
//			WriteEndDocumentation(writer);
		}

		private void WriteTypeDocumentation(XmlWriter writer, AssemblyName assemblyName, string memberName)
		{
			CheckForMissingSummaryAndRemarks(writer, assemblyName, memberName);
			WriteSlashDocElements(writer, assemblyName, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteDelegateDocumentation(XmlWriter writer, string memberName, MethodInfo method)
		{
			CheckForMissingParams(writer, method.DeclaringType.Assembly.GetName(), memberName, method.GetParameters());
			CheckForMissingReturns(writer, method.DeclaringType.Assembly.GetName(), memberName, method);
			WriteTypeDocumentation(writer, method.DeclaringType.Assembly.GetName(), memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteEnumerationDocumentation(XmlWriter writer, AssemblyName assemblyName, string memberName)
		{
			CheckForMissingSummaryAndRemarks(writer, assemblyName, memberName);
			WriteSlashDocElements(writer, assemblyName, memberName);
			WriteEndDocumentation(writer);
		}

		//if the constructor has no parameters and no summary,
		//add a default summary text.
		private bool DoAutoDocumentConstructor(
			XmlWriter writer,
			AssemblyName assemblyName,
			string memberName,
			ConstructorInfo constructor)
		{
			if (_rep.AutoDocumentConstructors) {
				if (constructor.GetParameters().Length == 0) {
					string xmldoc = _assemblyDocCache.GetDoc(assemblyName, memberName);
					bool bMissingSummary = true;

					if (xmldoc != null) {
						XmlTextReader reader = new XmlTextReader(xmldoc, XmlNodeType.Element, null);
						while (reader.Read()) {
							if (reader.NodeType == XmlNodeType.Element) {
								if (reader.Name == "summary") {
									string summarydetails = reader.ReadInnerXml();
									if (summarydetails.Length > 0 && !summarydetails.Trim().StartsWith("Summary description for")) {
										bMissingSummary = false;
									}
								}
							}
						}
					}

					if (bMissingSummary) {
						WriteStartDocumentation(writer);
						writer.WriteStartElement("summary");
						if (constructor.IsStatic) {
							writer.WriteString("Initializes the static fields of the ");
						} else {
							writer.WriteString("Initializes a new instance of the ");
						}
						writer.WriteStartElement("see");
						writer.WriteAttributeString("cref", MemberID.GetMemberID(constructor.DeclaringType));
						writer.WriteEndElement();
						writer.WriteString(" class.");
						writer.WriteEndElement();
						return true;
					}
				}
			}
			return false;
		}

		private void WriteConstructorDocumentation(
			XmlWriter writer, AssemblyName assemblyName,
			string memberName,
			ConstructorInfo constructor)
		{
			if (!DoAutoDocumentConstructor(writer, assemblyName, memberName, constructor)) {
				CheckForMissingSummaryAndRemarks(writer, assemblyName, memberName);
				CheckForMissingParams(writer, assemblyName, memberName, constructor.GetParameters());
			}
			WriteSlashDocElements(writer, assemblyName, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteFieldDocumentation(
			XmlWriter writer, AssemblyName assemblyName,
			string memberName,
			Type type)
		{
			if (!CheckForPropertyBacker(writer, assemblyName, memberName, type)) {
				CheckForMissingSummaryAndRemarks(writer, assemblyName, memberName);
			}
			WriteSlashDocElements(writer, assemblyName, memberName);
			WriteEndDocumentation(writer);
		}

		private void WritePropertyDocumentation(
			XmlWriter writer, AssemblyName assemblyName,
			string memberName,
			PropertyInfo property,
			bool writeMissing)
		{
			if (writeMissing) {
				CheckForMissingSummaryAndRemarks(writer, assemblyName, memberName);
				CheckForMissingParams(writer, assemblyName, memberName, GetIndexParameters(property));
				CheckForMissingValue(writer, assemblyName, memberName);
			}
			WriteSlashDocElements(writer, assemblyName, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteMethodDocumentation(
			XmlWriter writer, AssemblyName assemblyName,
			string memberName,
			MethodInfo method,
			bool writeMissing)
		{
			if (writeMissing) {
				CheckForMissingSummaryAndRemarks(writer, assemblyName, memberName);
				CheckForMissingParams(writer, assemblyName, memberName, method.GetParameters());
				CheckForMissingReturns(writer, assemblyName, memberName, method);
			}
			WriteSlashDocElements(writer, assemblyName, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteEventDocumentation(
			XmlWriter writer, AssemblyName assemblyName,
			string memberName,
			bool writeMissing)
		{
			if (writeMissing) {
				CheckForMissingSummaryAndRemarks(writer, assemblyName, memberName);
			}
			WriteSlashDocElements(writer, assemblyName, memberName);
			WriteEndDocumentation(writer);
		}

		#endregion


		#region Property Backers

		/// <summary>
		/// This checks whether a field is a property backer, meaning
		/// it stores the information for the property.
		/// </summary>
		/// <remarks>
		/// <para>This takes advantage of the fact that most people
		/// have a simple convention for the names of the fields
		/// and the properties that they back.
		/// If the field doesn't have a summary already, and it
		/// looks like it backs a property, and the BaseDocumenterConfig
		/// property is set appropriately, then this adds a
		/// summary indicating that.</para>
		/// <para>Note that this design will call multiple fields the
		/// backer for a single property.</para>
		/// <para/>This also will call a public field a backer for a
		/// property, when typically that wouldn't be the case.
		/// </remarks>
		/// <param name="writer">The XmlWriter to write to.</param>
		/// <param name="assemblyName">the name of the currently processed assembly</param>
		/// <param name="memberName">The full name of the field.</param>
		/// <param name="type">The Type which contains the field
		/// and potentially the property.</param>
		/// <returns>True only if a property backer is auto-documented.</returns>
		private bool CheckForPropertyBacker(
			XmlWriter writer, AssemblyName assemblyName,
			string memberName,
			Type type)
		{
			if (!_rep.AutoPropertyBackerSummaries)
				return false;

			//check whether or not we have a valid summary
			bool isMissingSummary = true;
			string xmldoc = _assemblyDocCache.GetDoc(assemblyName, memberName);
			if (xmldoc != null) {
				XmlTextReader reader = new XmlTextReader(xmldoc, XmlNodeType.Element, null);
				while (reader.Read()) {
					if (reader.NodeType == XmlNodeType.Element) {
						if (reader.Name == "summary") {
							string summarydetails = reader.ReadInnerXml();
							if (summarydetails.Length > 0 && !summarydetails.Trim().StartsWith("Summary description for")) {
								isMissingSummary = false;
							}
						}
					}
				}
			}

			// only do this if there is no summary already
			if (isMissingSummary) {
				// find the property (if any) that this field backs

				// generate the possible property names that this could back
				// so far have: property Name could be backed by _Name or name
				// but could be other conventions
				string[] words = memberName.Split('.');
				string fieldBaseName = words[words.Length - 1];
				string firstLetter = fieldBaseName.Substring(0, 1);
				string camelCasePropertyName = firstLetter.ToUpper()
					+ fieldBaseName.Remove(0, 1);
				string usPropertyName = fieldBaseName.Replace("_", "");

				// find it
				PropertyInfo propertyInfo;

				if (((propertyInfo = FindProperty(camelCasePropertyName,
					type)) != null)
					|| ((propertyInfo = FindProperty(usPropertyName,
					type)) != null)) {
					WritePropertyBackerDocumentation(writer, "summary",
						propertyInfo);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Find a particular property of the specified type, by name.
		/// Return the PropertyInfo for it.
		/// </summary>
		/// <param name="expectedPropertyName">The name of the property to
		/// find.</param>
		/// <param name="type">The type in which to search for
		/// the property.</param>
		/// <returns>PropertyInfo - The property info, or null for
		/// not found.</returns>
		private PropertyInfo FindProperty(string expectedPropertyName,
			Type type)
		{
			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			PropertyInfo[] properties = type.GetProperties(bindingFlags);
			foreach (PropertyInfo property in properties) {
				if (property.Name.Equals(expectedPropertyName)) {
					MethodInfo getMethod = property.GetGetMethod(true);
					MethodInfo setMethod = property.GetSetMethod(true);

					bool hasGetter = (getMethod != null) && MustDocumentMethod(getMethod);
					bool hasSetter = (setMethod != null) && MustDocumentMethod(setMethod);

					if ((hasGetter || hasSetter) && !IsAlsoAnEvent(property)) {
						return (property);
					}
				}
			}

			return (null);
		}

		/// <summary>
		/// Write xml info for a property's backer field to the specified writer.
		/// This writes a string with a link to the property.
		/// </summary>
		/// <param name="writer">The XmlWriter to write to.</param>
		/// <param name="element">The field which backs the property.</param>
		/// <param name="property">The property backed by the field.</param>
		private void WritePropertyBackerDocumentation(
			XmlWriter writer,
			string element,
			PropertyInfo property)
		{
			string propertyName = property.Name;
			string propertyId = "P:" + property.DeclaringType.FullName + "."
				+ propertyName;

			WriteStartDocumentation(writer);
			writer.WriteStartElement(element);
			writer.WriteRaw("Backer for property <see cref=\""
				+ propertyId + "\">" + property.Name + "</see>");
			writer.WriteEndElement();
		}

		#endregion


		#region PreReflectionProcess

		private void PreReflectionProcess()
		{
			//			PreLoadXmlDocumentation();
			BuildXrefs();
		}

		private void BuildXrefs()
		{
			//build derived members and implementing types xrefs.
			foreach (FileInfo assemblyFileName in _rep.AssemblyFileNames) {
				//attempt to load the assembly
				IAssemblyInfo assembly = _assemblyLoader.GetAssemblyInfo(assemblyFileName);

				// loop through all types in assembly
				foreach (Type type in assembly.GetTypes()) {
					if (MustDocumentType(type)) {
						BuildDerivedMemberXref(type);
						BuildDerivedInterfaceXref(type);
						string friendlyNamespaceName = type.Namespace ?? "(global)";
						BuildNamespaceHierarchy(friendlyNamespaceName, type);
						_notEmptyNamespaces[friendlyNamespaceName] = null;
					}
				}
			}
		}

		private void BuildDerivedMemberXref(Type type)
		{
			if (type.BaseType != null &&
				MustDocumentType(type.BaseType)) // we don't care about undocumented types
            {
				_derivedTypes.Add(type.BaseType, type);
			}
		}

		private void BuildNamespaceHierarchy(string namespaceName, Type type)
		{
			if ((type.BaseType != null) &&
				MustDocumentType(type.BaseType)) // we don't care about undocumented types
            {
				_namespaceHierarchies.Add(namespaceName, type.BaseType, type);
				BuildNamespaceHierarchy(namespaceName, type.BaseType);
			}
			if (type.IsInterface) {
				_namespaceHierarchies.Add(namespaceName, typeof(Object), type);
			}
			//build a collection of the base type's interfaces
			//to determine which have been inherited
			StringCollection interfacesOnBase = new StringCollection();
			if (type.BaseType != null) {
				foreach (Type baseInterfaceType in type.BaseType.GetInterfaces()) {
					interfacesOnBase.Add(MemberID.GetMemberID(baseInterfaceType));
				}
			}
			foreach (Type interfaceType in type.GetInterfaces()) {
				if (MustDocumentType(interfaceType)) {
					if (!interfacesOnBase.Contains(MemberID.GetMemberID(interfaceType))) {
						_baseInterfaces.Add(type, interfaceType);
					}
				}
			}
		}

		private void BuildDerivedInterfaceXref(Type type)
		{
			foreach (Type interfaceType in type.GetInterfaces()) {
				if (MustDocumentType(interfaceType)) // we don't care about undocumented types
                {
					if (type.IsInterface) {
						_derivedTypes.Add(interfaceType, type);
					} else {
						_interfaceImplementingTypes.Add(interfaceType, type);
					}
				}
			}
		}

		#endregion


		#region Write Hierarchies

		/// <summary>
		/// Writes the XML documenting which classes derives from the current class
		/// </summary>
		/// <param name="writer">XML writer</param>
		/// <param name="type">Current class type</param>
		private void WriteDerivedTypes(XmlWriter writer, Type type)
		{
			foreach (Type derived in _derivedTypes.GetDerivedTypes(type)) {
				writer.WriteStartElement("derivedBy");
				writer.WriteAttributeString("id", MemberID.GetMemberID(derived));
				writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(derived));
				writer.WriteAttributeString("namespace", derived.Namespace);
				writer.WriteAttributeString("assembly", derived.Assembly.GetName().Name);
				WriteGenericArgumentsAndParameters(derived, writer);
				writer.WriteEndElement();
			}
		}

		private void WriteInterfaceImplementingTypes(XmlWriter writer, Type type)
		{
			foreach (Type implementingType in _interfaceImplementingTypes.GetDerivedTypes(type)) {
				writer.WriteStartElement("implementedBy");
				writer.WriteAttributeString("id", MemberID.GetMemberID(implementingType));
				writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(implementingType));
				writer.WriteAttributeString("namespace", implementingType.Namespace);
				writer.WriteAttributeString("assembly", implementingType.Assembly.GetName().Name);
				writer.WriteEndElement();
			}
		}


		private void WriteNamespaceHierarchies(XmlWriter writer)
		{
			writer.WriteStartElement("namespaceHierarchies");
			foreach (string namespaceName in _namespaceHierarchies.DefinedNamespaces) {
				WriteNamespaceTypeHierarchy(writer, namespaceName);
			}
			writer.WriteEndElement();
		}

		private void WriteNamespaceTypeHierarchy(XmlWriter writer, string namespaceName)
		{
			//get all base types from which members of this namespace are derived
			TypeHierarchy derivedTypesCollection = _namespaceHierarchies.GetDerivedTypesCollection(namespaceName);
			if (derivedTypesCollection != null) {
				//we will always start the hierarchy with System.Object (hopefully for obvious reasons)
				writer.WriteStartElement("namespaceHierarchy");
				writer.WriteAttributeString("name", namespaceName);
				WriteTypeHierarchy(writer, derivedTypesCollection, typeof(Object));
				writer.WriteEndElement();
			}
		}

		private void WriteTypeHierarchy(XmlWriter writer, TypeHierarchy derivedTypes, Type type)
		{
			List<string> alreadyDocumented = new List<string>();

			string typeId = string.Format("{0}{1}", type.Assembly.GetName().Name, MemberID.GetMemberID(type));
			alreadyDocumented.Add( typeId );

			writer.WriteStartElement("hierarchyType");
			writer.WriteAttributeString("id", MemberID.GetMemberID(type));
			writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(type));
			writer.WriteAttributeString("namespace", type.Namespace);
			writer.WriteAttributeString("assembly", type.Assembly.GetName().Name);

			ArrayList interfaces = _baseInterfaces.GetDerivedTypes(type);
			if (interfaces.Count > 0) {
				writer.WriteStartElement("hierarchyInterfaces");
				foreach (Type baseInterfaceType in interfaces) {

					string interfaceTypeId = string.Format("{0}{1}", baseInterfaceType.Assembly.GetName().Name, MemberID.GetMemberID(baseInterfaceType));

					if (alreadyDocumented.Contains(interfaceTypeId))
						continue;
					alreadyDocumented.Add(interfaceTypeId);

					writer.WriteStartElement("hierarchyInterface");
					writer.WriteAttributeString("id", MemberID.GetMemberID(baseInterfaceType));
					writer.WriteAttributeString("displayName", MemberDisplayName.GetMemberDisplayName(baseInterfaceType));
					writer.WriteAttributeString("namespace", baseInterfaceType.Namespace);
					writer.WriteAttributeString("fullName", baseInterfaceType.FullName);
					writer.WriteAttributeString("assembly", baseInterfaceType.Assembly.GetName().Name);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			ArrayList childTypesList = derivedTypes.GetDerivedTypes(type);
			foreach (Type childType in childTypesList) {
				string childTypeId = string.Format("{0}{1}", childType.Assembly.GetName().Name, MemberID.GetMemberID(childType));
				if (alreadyDocumented.Contains(childTypeId))
					continue;
				alreadyDocumented.Add(childTypeId);
				WriteTypeHierarchy(writer, derivedTypes, childType);
			}
			writer.WriteEndElement();
		}

		#endregion

		private string GetEnumString<T>(T enumValue)
			where T:struct
		{
			return Enum.GetName(typeof (T), enumValue);
		}

		private void TraceErrorOutput(string message)
		{
			TraceErrorOutput(message, null);
		}

		private void TraceErrorOutput(string message, Exception ex)
		{
			Trace.WriteLine("[WARNING] " + message);
			if (ex != null) {
				Exception tempEx = ex;
				do {
					Trace.WriteLine("-> " + tempEx.GetType().ToString() + ":" + ex.Message);
					tempEx = tempEx.InnerException;
				} while (tempEx != null);
				Trace.WriteLine(ex.StackTrace);
			}
		}

		//		private AssemblyLoader SetupAssemblyLoader() {
		//			AssemblyLoader _assemblyLoader = new AssemblyLoader(_rep.ReferencePaths);
		//
		//			_assemblyLoader.Install();
		//
		//			return (_assemblyLoader);
		//		}


		#region ImplementsInfo

		private class ImplementsInfo
		{
			public Type TargetType;
			public MemberInfo TargetMethod;
			public Type InterfaceType;
			public MemberInfo InterfaceMethod;
		}
		private class ImplementsCollection
		{
			private readonly Hashtable data;
			public ImplementsCollection()
			{
				data = new Hashtable(15); // give it an initial capacity...
			}
			public ImplementsInfo this[string name]
			{
				get { return (ImplementsInfo)data[name]; }
				set { data[name] = value; }
			}
		}

		#endregion


	}
}
