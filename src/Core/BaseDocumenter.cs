// Documenter.cs - base XML documenter code
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.ComponentModel;

namespace NDoc.Core
{
	/// <summary>Provides the base class for documenters.</summary>
	abstract public class BaseDocumenter : IDocumenter, IComparable
	{
		IDocumenterConfig		config;
		Project					_Project;

		AssemblyXmlDocCache		assemblyDocCache;
		ExternalXmlSummaryCache	externalSummaryCache;

		private class ImplementsInfo
		{
			public Type TargetType;
			public MemberInfo TargetMethod;
			public Type InterfaceType;
			public MemberInfo InterfaceMethod;
		}
		private class ImplementsCollection
		{
			private Hashtable data;
			public ImplementsCollection()
			{
				data = new Hashtable(15);  // give it an initial capacity...
			}
			public ImplementsInfo this [int index]
			{
				get { return (ImplementsInfo)data[index]; }
				set { data[index]=value; }
			}
			public ImplementsInfo this [string name]
			{
				get { return (ImplementsInfo)data[name]; }
				set { data[name]= value; }
			}
		}
		ImplementsCollection implementations;

		/// <summary>Initialized a new BaseDocumenter instance.</summary>
		protected BaseDocumenter(string name)
		{
			_Name = name;
		}

		/// <summary>
		/// The development status (alpha, beta, stable) of this documenter.
		/// Documenters should override this if they aren't stable.
		/// </summary>
		public virtual DocumenterDevelopmentStatus DevelopmentStatus
		{
			get { return(DocumenterDevelopmentStatus.Stable); }
		}

		/// <summary>Compares the currrent document to another documenter.</summary>
		public int CompareTo(object obj)
		{
			return String.Compare(Name, ((IDocumenter)obj).Name);
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public IDocumenterConfig Config
		{
			get { return config; }
			set { config = value; }
		}

		private string _Name;

		/// <summary>Gets the display name for the documenter.</summary>
		public string Name
		{
			get { return _Name; }
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public abstract string MainOutputFile { get; }

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public virtual void View()
		{
			if (File.Exists(this.MainOutputFile))
			{
				Process.Start(this.MainOutputFile);
			}
			else
			{
				throw new FileNotFoundException("Documentation not built.",
					this.MainOutputFile);
			}
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public event DocBuildingEventHandler DocBuildingStep;
		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public event DocBuildingEventHandler DocBuildingProgress;

		/// <summary>Raises the DocBuildingStep event.</summary>
		/// <param name="step">The overall percent complete value.</param>
		/// <param name="label">A description of the work currently beeing done.</param>
		protected void OnDocBuildingStep(int step, string label)
		{
			if (DocBuildingStep != null)
				DocBuildingStep(this, new ProgressArgs(step, label));
		}

		/// <summary>Raises the DocBuildingProgress event.</summary>
		/// <param name="progress">Percentage progress value</param>
		protected void OnDocBuildingProgress(int progress)
		{
			if (DocBuildingProgress != null)
				DocBuildingProgress(this, new ProgressArgs(progress, ""));
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		abstract public void Clear();

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public virtual string CanBuild(Project project)
		{
			return this.CanBuild(project, false);
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public virtual string CanBuild(Project project, bool checkInputOnly)
		{
			StringBuilder xfiles = new StringBuilder();
			foreach (AssemblySlashDoc asd in project.GetAssemblySlashDocs())
			{
				if (!File.Exists(asd.AssemblyFilename))
				{
					xfiles.Append("\n" + asd.AssemblyFilename);
				}
				if (!File.Exists(asd.SlashDocFilename))
				{
					xfiles.Append("\n" + asd.SlashDocFilename);
				}
			}

			if (xfiles.Length > 0)
			{
				return "One of more source files not found:\n" + xfiles.ToString();
			}
			else
			{
				return null;
			}
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		abstract public void Build(Project project);

		/// <summary>
		/// Setup AssemblyResolver for case where system doesn't resolve
		/// an assembly automatically.
		/// This puts in the directories in ReferencesPath, and the directories
		/// to each assembly referenced in the project.
		/// </summary>
		/// <remarks>
		/// <para>The case which forced this to be so thorough is when an assembly 
		/// references an unmanaged (native) dll.  When the assembly is loaded,
		/// the system must also find the unmanaged dll.  The rules for
		/// finding the unmanaged dll are apparently just like any other application:
		/// current working directory, the path environment variable, etc. </para>
		/// <para>So in order to handle that case, we have to install an
		/// AssemblyResolver that catches the resolution failure, and uses
		/// an assembly load function that cd's to the directory which hopefully
		/// contains the unmanaged dll (see LoadAssembly()).  So in this
		/// case I'm assuming that the directory containing the referencing
		/// assembly also contains the unmanaged dll.</para>
		/// </remarks>
		/// <param name="project"></param>
		protected AssemblyResolver SetupAssemblyResolver(Project project)
		{
			ArrayList assemblyResolveDirs = new ArrayList();

			// add references path
			if ((MyConfig.ReferencesPath != null) && (MyConfig.ReferencesPath.Length > 0))
			{
				string[] dirs = MyConfig.ReferencesPath.Split(';');
				foreach(string dir in dirs)
				{
					if (!assemblyResolveDirs.Contains(dir))
						assemblyResolveDirs.Add(dir);
				}
			}

			// put dirs containing assemblies in also
			foreach(AssemblySlashDoc assemblySlashDoc in project.GetAssemblySlashDocs())
			{
				string dir = Path.GetDirectoryName(assemblySlashDoc.AssemblyFilename);
				if (!assemblyResolveDirs.Contains(dir)) assemblyResolveDirs.Add(dir);
			}
			AssemblyResolver assemblyResolver = new AssemblyResolver(assemblyResolveDirs);

			// For performance, don't have resolver search all subdirs.  Also, it's not
			// clear that's a reasonable behavior.
			assemblyResolver.IncludeSubdirs = false; 
			assemblyResolver.Install();

			return(assemblyResolver);
		}

		/// <summary>Builds an Xml file combining the reflected metadata with the /doc comments.</summary>
		/// <returns>full pathname of XML file</returns>
		/// <remarks>The caller is responsible for deleting the xml file after use...</remarks>
		protected string MakeXmlFile(Project project)
		{
			string tempfilename = Path.GetTempFileName();

			//if MyConfig.UseNDocXmlFile is set, 
			//copy it to the temp file and return.
			string xmlFile = MyConfig.UseNDocXmlFile;
			if (xmlFile.Length > 0)
			{
				Trace.WriteLine("Loading pre-compiled XML information from:\n" + xmlFile);
				File.Copy(xmlFile,tempfilename,true);
				return tempfilename;
			}

			XmlWriter writer=null;
			try
			{
				writer = new XmlTextWriter(tempfilename,Encoding.Default);
			
				BuildXml(project, writer);
			
				if (writer != null)  writer.Close();
			
				return tempfilename;
			}
			finally
			{
				if (writer != null)  writer.Close();
			}
			
		}


		/// <summary>Builds an Xml string combining the reflected metadata with the /doc comments.</summary>
		/// <remarks>This now evidently writes the string in utf-16 format (and 
		/// says so, correctly I suppose, in the xml text) so if you write this string to a file with 
		/// utf-8 encoding it will be unparseable because the file will claim to be utf-16
		/// but will actually be utf-8.</remarks>
		/// <returns>XML string</returns>
		protected string MakeXml(Project project)
		{
			//if MyConfig.UseNDocXmlFile is set, 
			//load the XmlBuffer from the file and return.
			string xmlFile = MyConfig.UseNDocXmlFile;
			if (xmlFile.Length > 0)
			{
				Trace.WriteLine("Loading pre-compiled XML information from:\n" + xmlFile);
				using (TextReader reader = new StreamReader(xmlFile))
				{
					return reader.ReadToEnd();
				}
			}

			StringWriter swriter = new StringWriter();
			XmlWriter writer = new XmlTextWriter(swriter);

			try
			{
				BuildXml(project, writer);
				return swriter.ToString();
			}
			finally
			{
				if (writer != null)  writer.Close();
				if (swriter != null) swriter.Close();
			}

		}

		/// <summary>
		/// Allows documenter implementations to add their own content to the xml file
		/// </summary>
		/// <param name="writer">XmlWriter to write to</param>
		/// <remarks>
		/// <para>This method should be overriden if a documenter wishes to add xml elements. 
		/// It is called after the root (&gt;ndoc&lt;) element is created. </para>
		/// <para><note>Individual documenters are responsible for ensuring that the added xml is well-formed...</note></para>
		/// </remarks>
		protected virtual void AddDocumenterSpecificXmlData(XmlWriter writer)
		{
		}

		/// <summary>Builds an Xml file combining the reflected metadata with the /doc comments.</summary>
		protected void BuildXml(Project project, XmlWriter writer)
		{
		int start = Environment.TickCount;

			Debug.WriteLine("Memory making xml: " + GC.GetTotalMemory(false).ToString());


			_Project = project;
			AssemblyResolver assemblyResolver = SetupAssemblyResolver(project);


			if (MyConfig.GetExternalSummaries)
			{
				string DocLangCode = Enum.GetName(typeof(SdkLanguage),MyConfig.SdkDocLanguage).Replace("_","-");
				externalSummaryCache = new ExternalXmlSummaryCache(DocLangCode);
				foreach (AssemblySlashDoc assemblySlashDoc in project.GetAssemblySlashDocs())
				{
					string assemblypath = Path.GetFullPath(assemblySlashDoc.AssemblyFilename);
					string slashdocpath = Path.GetFullPath(assemblySlashDoc.SlashDocFilename);
					Assembly assembly = LoadAssembly(assemblypath);
					externalSummaryCache.AddXmlDoc(assembly.FullName, slashdocpath);
				}
			}

			string currentAssemblyFilename = "";

			try
			{
				// Use indenting for readability
				//writer.Formatting = Formatting.Indented;
				//writer.Indentation=4;

				// Start the document with the XML declaration tag
				writer.WriteStartDocument();

				// Start the root element
				writer.WriteStartElement("ndoc");
				writer.WriteAttributeString("SchemaVersion","1.0");

				//add any documenter specific elements.
				AddDocumenterSpecificXmlData(writer);

				if (MyConfig.FeedbackEmailAddress.Length > 0)
					WriteFeedBackEmailAddress( writer );

				if (MyConfig.CopyrightText.Length > 0)
					WriteCopyright( writer );

				if ( MyConfig.InheritPlatformSupport )
					WriteDefaultPlatform( writer );

				if ( MyConfig.Preliminary )
					writer.WriteElementString( "preliminary", "" );

				int step = 100 / project.AssemblySlashDocCount;
				int i = 0;

				foreach (AssemblySlashDoc assemblySlashDoc in project.GetAssemblySlashDocs())
				{
					OnDocBuildingProgress(i * step);

					currentAssemblyFilename = assemblySlashDoc.AssemblyFilename;
					string path = Path.GetFullPath(currentAssemblyFilename);
					Assembly assembly = LoadAssembly(path);

					assemblyDocCache = new AssemblyXmlDocCache(assemblySlashDoc.SlashDocFilename);

					int starta = Environment.TickCount;

					WriteAssembly(writer, assembly);

					Trace.WriteLine("Completed " + assembly.FullName); 
					Trace.WriteLine(((Environment.TickCount - starta)/1000.0).ToString() + " sec.");

					assemblyDocCache.Flush();

					i++;
				}

				OnDocBuildingProgress(100);

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();

				Trace.WriteLine("MakeXML : " + ((Environment.TickCount - start)/1000.0).ToString() + " sec.");

				// if you want to see NDoc's intermediate XML file, use the XML documenter.
			}
			catch (Exception e)
			{
				throw new DocumenterException(
					"Error reflecting against the " +
					Path.GetFileName(currentAssemblyFilename) +
					" assembly: \n" + e.Message, e);
			}
			finally
			{
				Debug.WriteLine("Memory before cleanup: " + GC.GetTotalMemory(false).ToString());

				_Project = null;

				if (assemblyResolver != null)
				{
					assemblyResolver.Deinstall();
				}

				if (externalSummaryCache != null) externalSummaryCache=null;

				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				Debug.WriteLine("Memory after cleanup: " + GC.GetTotalMemory(false).ToString());
			}
		}

		// writes out the list of default supported operating systems and frameworks 
		private void WriteDefaultPlatform( XmlWriter writer )
		{			
			writer.WriteStartElement( "platform" );

			//write out the supported operating systems
			if ( MyConfig.DefaultOSSupport != OSSupport.none )
			{
				writer.WriteStartElement( "os" );

				switch ( MyConfig.DefaultOSSupport )
				{
					case OSSupport.all:
						writer.WriteAttributeString( "predefined", "all" );
						break;
					case OSSupport.nt5plus:
						writer.WriteAttributeString( "predefined", "nt5plus" );
						break;
					case OSSupport.enterprise:
						writer.WriteAttributeString( "predefined", "enterprise" );
						break;
					default:
						Debug.Assert( false );	//remind ourselves to update this switch if new items are added
						break;
				}	
			
				if ( MyConfig.AdditionalOSList.Length > 0 )
					writer.WriteString( MyConfig.AdditionalOSList );

				writer.WriteEndElement();
			}
			
			//write out the supported frameworks
			if ( MyConfig.AdditionalFrameworkList.Length > 0 || MyConfig.SupportCompactFrameworkByDefault || MyConfig.SupportMONOFrameworkByDefault )
			{
				writer.WriteStartElement( "frameworks" );

				if ( MyConfig.AdditionalFrameworkList.Length > 0 )
					writer.WriteElementString( "custom", MyConfig.AdditionalFrameworkList );

				if ( MyConfig.SupportCompactFrameworkByDefault )			
					writer.WriteElementString( "compact", "true" );
			

				if ( MyConfig.SupportMONOFrameworkByDefault )
					writer.WriteElementString( "mono", "true" );			

				writer.WriteEndElement();
			}

			writer.WriteEndElement();			
		}

		
		private void WriteFeedBackEmailAddress( XmlWriter writer )
		{
			writer.WriteElementString( "feedbackEmail", MyConfig.FeedbackEmailAddress );
		}
		
		// writes the copyright node to the documentation
		private void WriteCopyright( XmlWriter writer )
		{
			writer.WriteStartElement("copyright");
			writer.WriteAttributeString("text", MyConfig.CopyrightText);

			if (MyConfig.CopyrightHref.Length > 0)
			{
				if (!MyConfig.CopyrightHref.StartsWith("http:"))
				{
					writer.WriteAttributeString("href", Path.GetFileName(MyConfig.CopyrightHref));
				}
				else
				{
					writer.WriteAttributeString("href", MyConfig.CopyrightHref);
				}
			}

			writer.WriteEndElement();
		}

		//checks if the member has been flagged with the 
		//EditorBrowsableState.Never value
		private bool IsEditorBrowsable(MemberInfo minfo)
		{
			if (MyConfig.EditorBrowsableFilter == EditorBrowsableFilterLevel.Off)
			{
				return true;
			}

			EditorBrowsableAttribute[] browsables = 
				Attribute.GetCustomAttributes(minfo, typeof(EditorBrowsableAttribute), false)
				as EditorBrowsableAttribute[];
			
			if (browsables.Length == 0)
			{
				return true;
			}
			else
			{
				EditorBrowsableAttribute browsable = browsables[0];
				return (browsable.State == EditorBrowsableState.Always) || 
					((browsable.State == EditorBrowsableState.Advanced) && 
					(MyConfig.EditorBrowsableFilter != EditorBrowsableFilterLevel.HideAdvanced));
			}
		}

		private bool MustDocumentType(Type type)
		{
			Type declaringType = type.DeclaringType;

			//If type name starts with a digit it is not a valid identifier
			//in any of the MS .Net languages.
			//It's probably a J# anonomous inner class...
			//Whatever, do not document it.
			if (Char.IsDigit(type.Name,0))
				return false;

			//exclude types that are internal to the .Net framework.
			if (type.FullName.StartsWith("System") || type.FullName.StartsWith("Microsoft"))
			{
				if(type.IsNotPublic) return false;
				if(type.DeclaringType !=null &&
					!MustDocumentType(type.DeclaringType))
					return false;
			}

			return 
				!type.FullName.StartsWith("<PrivateImplementationDetails>") &&
				(declaringType == null || MustDocumentType(declaringType)) &&
				(
				(type.IsPublic) ||
				(type.IsNotPublic && MyConfig.DocumentInternals) ||
				(type.IsNestedPublic) ||
				(type.IsNestedFamily && MyConfig.DocumentProtected) ||
				(type.IsNestedFamORAssem && MyConfig.DocumentProtected) ||
				(type.IsNestedAssembly && MyConfig.DocumentInternals) ||
				(type.IsNestedFamANDAssem && MyConfig.DocumentInternals) ||
				(type.IsNestedPrivate && MyConfig.DocumentPrivates)
				) &&
				IsEditorBrowsable(type) &&
				(!MyConfig.UseNamespaceDocSummaries || (type.Name != "NamespaceDoc")) &&
				!assemblyDocCache.HasExcludeTag(GetMemberName(type));
		}

		private bool MustDocumentMethod(MethodBase method)
		{
			// Methods containing '.' in their name that aren't constructors are probably
			// explicit interface implementations, we check whether we document those or not.
			if((method.Name.IndexOf('.') != -1) && (method.Name != ".ctor") && (method.Name != ".cctor"))
			{
				string interfaceName = null;
				int lastIndexOfDot = method.Name.LastIndexOf('.');
				if (lastIndexOfDot != -1)
				{
					interfaceName = method.Name.Substring(0, lastIndexOfDot);

					Type interfaceType = method.ReflectedType.GetInterface(interfaceName);

					// Document method if interface is (public) or (isInternal and documentInternal).
					if(  interfaceType != null && (interfaceType.IsPublic || 
						(interfaceType.IsNotPublic && MyConfig.DocumentInternals)))
					{
						return IsEditorBrowsable(method);
					}
				}
			}

			// All other methods
			return 
				(
				(method.IsPublic) ||
				(method.IsFamily && MyConfig.DocumentProtected &&
				(MyConfig.DocumentSealedProtected || !method.ReflectedType.IsSealed)) ||
				(method.IsFamilyOrAssembly && MyConfig.DocumentProtected) ||
				(method.IsAssembly && MyConfig.DocumentInternals) ||
				(method.IsFamilyAndAssembly && MyConfig.DocumentInternals) ||
				(method.IsPrivate && MyConfig.DocumentPrivates)
				) &&
				IsEditorBrowsable(method) &&
				!assemblyDocCache.HasExcludeTag(GetMemberName(method));
		}


		private bool IsHidden(MemberInfo member, Type type)
		{
			if (member.DeclaringType == member.ReflectedType)
				return false;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MemberInfo [] members = type.GetMember(member.Name, bindingFlags);
			foreach (MemberInfo m in members)
			{
				if ((m != member)
					&& m.DeclaringType.IsSubclassOf(member.DeclaringType))
				{
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

			MemberInfo [] members = type.GetMember(method.Name, bindingFlags);
			foreach (MemberInfo m in members)
			{
				if ((m != method)
					&& (m.DeclaringType.IsSubclassOf(method.DeclaringType))
					&& ((m.MemberType != MemberTypes.Method)
					||	HaveSameSig(m as MethodInfo, method)))
				{
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

			MemberInfo [] members = baseType.GetMember(member.Name, bindingFlags);
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

			MemberInfo [] members = baseType.GetMember(method.Name, bindingFlags);
			foreach (MemberInfo m in members)
			{
				if (m == method)
					continue;

				if (m.MemberType != MemberTypes.Method)
					return true;

				MethodInfo meth = m as MethodInfo;
				if (HaveSameSig(meth, method)
					&& (((method.Attributes & MethodAttributes.Virtual) == 0)
					||  ((method.Attributes & MethodAttributes.NewSlot) != 0)))
				{
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
			foreach (MethodInfo accessor in property.GetAccessors(true))
			{
				if (	((accessor.Attributes & MethodAttributes.Virtual) != 0)
					&&  ((accessor.Attributes & MethodAttributes.NewSlot) == 0))
					return false;

				// indexers only hide indexers with the same signature
				if (isIndexer && !IsHiding(accessor, type))
					return false;
			}

			return true;
		}

		private bool HaveSameSig(MethodInfo m1, MethodInfo m2)
		{
			ParameterInfo [] ps1 = m1.GetParameters();
			ParameterInfo [] ps2 = m2.GetParameters();

			if (ps1.Length != ps2.Length)
				return false;

			for (int i = 0; i < ps1.Length; i++)
			{
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

		private bool MustDocumentField(FieldInfo field)
		{
			return (field.IsPublic ||
				(field.IsFamily && MyConfig.DocumentProtected &&
				(MyConfig.DocumentSealedProtected || !field.ReflectedType.IsSealed)) ||
				(field.IsFamilyOrAssembly && MyConfig.DocumentProtected) ||
				(field.IsAssembly && MyConfig.DocumentInternals) ||
				(field.IsFamilyAndAssembly && MyConfig.DocumentInternals) ||
				(field.IsPrivate && MyConfig.DocumentPrivates)) &&
				IsEditorBrowsable(field) &&
				!assemblyDocCache.HasExcludeTag(GetMemberName(field));
		}

		private void WriteAssembly(XmlWriter writer, Assembly assembly)
		{
			AssemblyName assemblyName = assembly.GetName();

			writer.WriteStartElement("assembly");
			writer.WriteAttributeString("name", assemblyName.Name);

			if (MyConfig.IncludeAssemblyVersion)
			{
				writer.WriteAttributeString("version", assemblyName.Version.ToString());
			}

            WriteCustomAttributes(writer, assembly);

			foreach (Module module in assembly.GetModules())
			{
				WriteModule(writer, module);
			}

			writer.WriteEndElement(); // assembly
		}

		/// <summary>Writes documentation about a module out as XML.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="module">Module to document.</param>
		private void WriteModule(XmlWriter writer, Module module)
		{
			writer.WriteStartElement("module");
			writer.WriteAttributeString("name", module.ScopeName);
            WriteCustomAttributes(writer, module);
			WriteNamespaces(writer, module);
			writer.WriteEndElement();
		}

		private void WriteNamespaces(XmlWriter writer, Module module)
		{
			Type[] types = module.GetTypes();

			StringCollection namespaceNames = GetNamespaceNames(types);

			foreach (string namespaceName in namespaceNames)
			{
				string ourNamespaceName;

				if (namespaceName == null)
				{
					ourNamespaceName = "(global)";
				}
				else
				{
					ourNamespaceName = namespaceName;
				}

				string namespaceSummary = null;
				if (MyConfig.UseNamespaceDocSummaries)
				{
					if (namespaceName == null)
						namespaceSummary = assemblyDocCache.GetDoc("T:NamespaceDoc");
					else
						namespaceSummary = assemblyDocCache.GetDoc("T:" + namespaceName + ".NamespaceDoc");
				}

				bool isNamespaceDoc = false;

				if ((namespaceSummary == null) || (namespaceSummary.Length == 0))
					namespaceSummary = _Project.GetNamespaceSummary(ourNamespaceName);
				else
					isNamespaceDoc = true;

				if (MyConfig.SkipNamespacesWithoutSummaries &&
					(namespaceSummary == null || namespaceSummary.Length == 0))
				{
					Trace.WriteLine(string.Format("Skipping namespace {0}...", namespaceName));
				}
				else
				{
					Trace.WriteLine(string.Format("Writing namespace {0}...", namespaceName));

					StringWriter swriter = null;
					XmlWriter tempWriter = null;
					try
					{
						// If we don't want empty namespaces, we need to write the XML to a temporary
						// writer, because we'll only know if its empty once the WriteXxx methods
						// have been called.

						XmlWriter myWriter;
						if (!MyConfig.DocumentEmptyNamespaces)
						{
							swriter = new StringWriter();
							tempWriter = new XmlTextWriter(swriter);
							myWriter = tempWriter;
						}
						else
						{
							myWriter = writer;
						}

						myWriter.WriteStartElement("namespace");
						myWriter.WriteAttributeString("name", ourNamespaceName);

						if (namespaceSummary != null && namespaceSummary.Length > 0)
						{
							WriteStartDocumentation(myWriter);

							if ( isNamespaceDoc )
							{
								myWriter.WriteRaw( namespaceSummary );
							}
							else
							{
								myWriter.WriteElementString("summary",namespaceSummary);
							}
							WriteEndDocumentation(myWriter);
						}
						else if (MyConfig.ShowMissingSummaries)
						{
							WriteStartDocumentation(myWriter);
							WriteMissingDocumentation(myWriter, "summary", null, "Missing <summary> Documentation for " + namespaceName);
							WriteEndDocumentation(myWriter);
						}

						int classCount = WriteClasses(myWriter, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} classes.", classCount));

						int interfaceCount = WriteInterfaces(myWriter, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} interfaces.", interfaceCount));

						int structureCount = WriteStructures(myWriter, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} structures.", structureCount));

						int delegateCount = WriteDelegates(myWriter, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} delegates.", delegateCount));

						int enumCount = WriteEnumerations(myWriter, types, namespaceName);
						Trace.WriteLine(string.Format("Wrote {0} enumerations.", enumCount));

						myWriter.WriteEndElement();

						if (!MyConfig.DocumentEmptyNamespaces)
						{
							tempWriter.Flush();
							if (classCount == 0 && interfaceCount == 0 && structureCount == 0 &&
								delegateCount == 0 && enumCount == 0)
							{
								Trace.WriteLine(string.Format("Discarding namespace {0} because it does not contain any documented types.", namespaceName));
							}
							else
							{
								writer.WriteRaw(swriter.ToString());
							}
						}
					}
					finally
					{
						if (tempWriter != null)
						{
							tempWriter.Close();
							tempWriter = null;
						}
						if (swriter != null)
						{
							swriter.Close();
							swriter = null;
						}
					}
				}
			}
		}

		private int WriteClasses(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types)
			{
				if (type.IsClass &&
					!IsDelegate(type) &&
					type.Namespace == namespaceName &&
					MustDocumentType(type))
				{
					bool hiding = ((type.MemberType & MemberTypes.NestedType) != 0)
						&& IsHiding(type, type.DeclaringType);
					WriteClass(writer, type, hiding);
					nbWritten++;
				}
			}

			return nbWritten;
		}

		private int WriteInterfaces(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types)
			{
				if (type.IsInterface &&
					type.Namespace == namespaceName &&
					MustDocumentType(type))
				{
					WriteInterface(writer, type);
					nbWritten++;
				}
			}

			return nbWritten;
		}

		private int WriteStructures(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types)
			{
				if (type.IsValueType &&
					!type.IsEnum &&
					type.Namespace == namespaceName &&
					MustDocumentType(type))
				{
					bool hiding = ((type.MemberType & MemberTypes.NestedType) != 0)
						&& IsHiding(type, type.DeclaringType);
					WriteClass(writer, type, hiding);
					nbWritten++;
				}
			}

			return nbWritten;
		}

		private int WriteDelegates(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types)
			{
				if (type.IsClass &&
					IsDelegate(type) &&
					type.Namespace == namespaceName &&
					MustDocumentType(type))
				{
					WriteDelegate(writer, type);
					nbWritten++;
				}
			}

			return nbWritten;
		}

		private int WriteEnumerations(XmlWriter writer, Type[] types, string namespaceName)
		{
			int nbWritten = 0;

			foreach (Type type in types)
			{
				if (type.IsEnum &&
					type.Namespace == namespaceName &&
					MustDocumentType(type))
				{
					WriteEnumeration(writer, type);
					nbWritten++;
				}
			}

			return nbWritten;
		}

		private bool IsDelegate(Type type)
		{
			if (type.BaseType==null) return false;
			return type.BaseType.FullName == "System.Delegate" ||
				type.BaseType.FullName == "System.MulticastDelegate";
		}

		private int GetMethodOverload(MethodInfo method, Type type)
		{
			int count = 0;
			int overload = 0;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MemberInfo[] methods = type.GetMember(method.Name, MemberTypes.Method, bindingFlags);
			foreach (MethodInfo m in methods)
			{
				if (!IsHidden(m, type) && MustDocumentMethod(m))
				{
					++count;
				}

				if (m == method)
				{
					overload = count;
				}
			}

			return (count > 1) ? overload : 0;
		}

		private int GetPropertyOverload(PropertyInfo property, PropertyInfo[] properties)
		{
			int count = 0;
			int overload = 0;

			foreach (PropertyInfo p in properties)
			{
				if ((p.Name == property.Name)
					/*&& !IsHidden(p, properties)*/)
				{
					++count;
				}

				if (p == property)
				{
					overload = count;
				}
			}

			return (count > 1) ? overload : 0;
		}

		private BaseDocumenterConfig MyConfig
		{
			get
			{
				return (BaseDocumenterConfig)Config;
			}
		}

		/// <summary>Writes XML documenting a class.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Class to document.</param>
		/// <param name="hiding">true if hiding base members</param>
		private void WriteClass(XmlWriter writer, Type type, bool hiding)
		{
			bool isStruct = type.IsValueType;

			string memberName = GetMemberName(type);

			string fullNameWithoutNamespace = type.FullName.Replace('+', '.');

			if (type.Namespace != null)
			{
				fullNameWithoutNamespace = fullNameWithoutNamespace.Substring(type.Namespace.Length + 1);
			}

			writer.WriteStartElement(isStruct ? "structure" : "class");
			writer.WriteAttributeString("name", fullNameWithoutNamespace);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			if (hiding)
			{
				writer.WriteAttributeString("hiding", "true");
			}

			// structs can't be abstract and always derive from System.ValueType
			// so don't bother including those attributes.
			if (!isStruct)
			{
				if (type.IsAbstract)
				{
					writer.WriteAttributeString("abstract", "true");
				}

				if (type.IsSealed)
				{
					writer.WriteAttributeString("sealed", "true");
				}

				if (type.BaseType != null && type.BaseType.FullName != "System.Object")
				{
					writer.WriteAttributeString("baseType", type.BaseType.Name);
				}
			}

			WriteTypeDocumentation(writer, memberName, type);
			WriteCustomAttributes(writer, type);
			if (type.BaseType!=null)
				WriteBaseType(writer, type.BaseType);

			//Debug.Assert(implementations == null);
			implementations = new ImplementsCollection();

			//build a collection of the base type's interfaces
			//to determine which have been inherited
			StringCollection baseInterfaces = new StringCollection();
			if (type.BaseType!=null)
			{
				foreach(Type baseInterfaceType in type.BaseType.GetInterfaces())
				{
					baseInterfaces.Add(baseInterfaceType.FullName);
				}
			}
 
			foreach(Type interfaceType in type.GetInterfaces())
			{
				if(MustDocumentType(interfaceType))
				{
					writer.WriteStartElement("implements");
					if (baseInterfaces.Contains(interfaceType.FullName))
					{
						writer.WriteAttributeString("inherited", "true");
					}
					writer.WriteString(interfaceType.FullName);
					writer.WriteEndElement();
 
					InterfaceMapping interfaceMap = type.GetInterfaceMap(interfaceType);
					int numberOfMethods = interfaceMap.InterfaceMethods.Length;
					for (int i = 0; i < numberOfMethods; i++)
					{
						string implementation			= interfaceMap.TargetMethods[i].ToString();
						ImplementsInfo implements		= new ImplementsInfo();
						implements.InterfaceMethod		= interfaceMap.InterfaceMethods[i];
						implements.InterfaceType		= interfaceMap.InterfaceType;
						implements.TargetMethod			= interfaceMap.TargetMethods[i];
						implements.TargetType			= interfaceMap.TargetType;
						implementations[implementation]	= implements;
					}
				}
			}

			WriteConstructors(writer, type);
			WriteStaticConstructor(writer, type);
			WriteFields(writer, type);
			WriteProperties(writer, type);
			WriteMethods(writer, type);
			WriteOperators(writer, type);
			WriteEvents(writer, type);

			implementations = null;

			writer.WriteEndElement();
		}

        private void WriteStructLayoutAttribute(XmlWriter writer, Type type) {
            string charSet = null;
            string layoutKind = null;

            // determine if CharSet property should be documented
            if ((type.Attributes & TypeAttributes.AutoClass) == TypeAttributes.AutoClass)
            {
                charSet = CharSet.Auto.ToString(CultureInfo.InvariantCulture);
            } 
            if ((type.Attributes & TypeAttributes.AnsiClass) == TypeAttributes.AnsiClass)
            {
                charSet = CharSet.Ansi.ToString(CultureInfo.InvariantCulture);
            } 
            if ((type.Attributes & TypeAttributes.UnicodeClass) == TypeAttributes.UnicodeClass)
            {
                charSet = CharSet.Unicode.ToString(CultureInfo.InvariantCulture);
            }

            // determine if Value property should be documented
            if ((type.Attributes & TypeAttributes.AutoLayout) == TypeAttributes.AutoLayout)
            {
                layoutKind = LayoutKind.Auto.ToString(CultureInfo.InvariantCulture);
            } 
            if ((type.Attributes & TypeAttributes.ExplicitLayout) == TypeAttributes.ExplicitLayout)
            {
                layoutKind = LayoutKind.Explicit.ToString(CultureInfo.InvariantCulture);
            } 
            if ((type.Attributes & TypeAttributes.SequentialLayout) == TypeAttributes.SequentialLayout)
            {
                layoutKind = LayoutKind.Sequential.ToString(CultureInfo.InvariantCulture);
            }

            if (charSet == null && layoutKind == null)
            {
                return;
            }

            // create attribute element
            writer.WriteStartElement("attribute");
            writer.WriteAttributeString("name", "System.Runtime.InteropServices.StructLayoutAttribute");

            if (charSet != null)
            {
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
			if ((type.Attributes & TypeAttributes.Serializable) == TypeAttributes.Serializable)
			{
				writer.WriteStartElement("attribute");
				writer.WriteAttributeString("name", "System.SerializableAttribute");
				writer.WriteEndElement(); // attribute
			}

            WriteStructLayoutAttribute(writer, type);
		}

		private void WriteSpecialAttributes(XmlWriter writer, FieldInfo field)
		{
			if ((field.Attributes & FieldAttributes.NotSerialized) == FieldAttributes.NotSerialized)
			{
				writer.WriteStartElement("attribute");
				writer.WriteAttributeString("name", "System.NonSerializedAttribute");
				writer.WriteEndElement(); // attribute
			}

			//TODO: more special attributes here?
		}

        private void WriteCustomAttributes(XmlWriter writer, Assembly assembly) {
            WriteCustomAttributes(writer, assembly.GetCustomAttributes(true));
        }

        private void WriteCustomAttributes(XmlWriter writer, Module module) {
            WriteCustomAttributes(writer, module.GetCustomAttributes(true));
        }

		private void WriteCustomAttributes(XmlWriter writer, Type type)
		{
			WriteSpecialAttributes(writer, type);
			WriteCustomAttributes(writer, type.GetCustomAttributes(true));
		}

		private void WriteCustomAttributes(XmlWriter writer, FieldInfo field)
		{
			WriteSpecialAttributes(writer, field);
			WriteCustomAttributes(writer, field.GetCustomAttributes(true));
		}

		private void WriteCustomAttributes(XmlWriter writer, MemberInfo memberInfo)
		{
			WriteCustomAttributes(writer, memberInfo.GetCustomAttributes(true));
		}

		private void WriteCustomAttributes(XmlWriter writer, ParameterInfo parameterInfo)
		{
			WriteCustomAttributes(writer, parameterInfo.GetCustomAttributes(true));
		}

		private void WriteCustomAttributes(XmlWriter writer, object[] attributes)
		{
			foreach (Attribute attribute in attributes)
			{
				if (this.MyConfig.DocumentAttributes)
				{
					WriteCustomAttribute(writer, attribute);
				}

				if (attribute.GetType().FullName == "System.ObsoleteAttribute") 
				{
					writer.WriteElementString("obsolete",((ObsoleteAttribute)attribute).Message);
				}
			}
		}

		private void WriteCustomAttribute(XmlWriter writer, Attribute attribute)
		{
			writer.WriteStartElement("attribute");
			writer.WriteAttributeString("name", attribute.GetType().FullName);

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Public;

			foreach (FieldInfo field in attribute.GetType().GetFields(bindingFlags))
			{
				writer.WriteStartElement("field");
				writer.WriteAttributeString("name", field.Name);
				writer.WriteAttributeString("type", field.FieldType.FullName);
				object value = field.GetValue(attribute);
				writer.WriteAttributeString("value", value != null ? value.ToString() : "");
				writer.WriteEndElement(); // field
			}

			foreach (PropertyInfo property in attribute.GetType().GetProperties(bindingFlags))
			{
				//skip the TypeId property
				if ((!MyConfig.ShowTypeIdInAttributes) && (property.Name == "TypeId"))
				{
					continue;
				}

				writer.WriteStartElement("property");
				writer.WriteAttributeString("name", property.Name);
				writer.WriteAttributeString("type", property.PropertyType.FullName);

				if (property.CanRead)
				{
					object value = null;
					/* WV030802: if an exception occurs while trying to read the value of the Attribute,
					 * write out the Exception as "value" */
					try
					{
						value = property.GetValue(attribute, null);
					}
					catch (Exception e)
					{
						value = e; 
					}
					writer.WriteAttributeString("value", value != null ? value.ToString() : "");
				}

				writer.WriteEndElement(); // property
			}

			writer.WriteEndElement(); // attribute
		}

		private void WriteConstructors(XmlWriter writer, Type type)
		{
			int overload = 0;

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			ConstructorInfo[] constructors = type.GetConstructors(bindingFlags);

			if (constructors.Length > 1)
			{
				overload = 1;
			}

			foreach (ConstructorInfo constructor in constructors)
			{
				if (MustDocumentMethod(constructor))
				{
					WriteConstructor(writer, constructor, overload++);
				}
			}
		}

		private void WriteStaticConstructor(XmlWriter writer, Type type)
		{
			const BindingFlags bindingFlags =
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			ConstructorInfo[] constructors = type.GetConstructors(bindingFlags);

			foreach (ConstructorInfo constructor in constructors)
			{
				if (MustDocumentMethod(constructor))
				{
					WriteConstructor(writer, constructor, 0);
				}
			}
		}

		private void WriteFields(XmlWriter writer, Type type)
		{
			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			FieldInfo [] fields = type.GetFields(bindingFlags);
			foreach (FieldInfo field in fields)
			{
				if (MustDocumentField(field)
					&& !IsAlsoAnEvent(field)
					&& !IsHidden(field, type))
				{
					WriteField(
						writer,
						field,
						type,
						IsHiding(field, type));
				}
			}
		}

		private void WriteProperties(XmlWriter writer, Type type)
		{
			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			PropertyInfo[] properties = type.GetProperties(bindingFlags);

			foreach (PropertyInfo property in properties)
			{
				if (IsEditorBrowsable(property))
				{
					MethodInfo getMethod = null;
					MethodInfo setMethod = null;
					if (property.CanRead)
					{
						try{getMethod = property.GetGetMethod(true);}
						catch(System.Security.SecurityException){}
					}
					if (property.CanWrite)
					{
						try{setMethod = property.GetSetMethod(true);}
						catch(System.Security.SecurityException){}
					}

					bool hasGetter = (getMethod != null) && MustDocumentMethod(getMethod);
					bool hasSetter = (setMethod != null) && MustDocumentMethod(setMethod);

					if ((hasGetter || hasSetter)
						&& !IsAlsoAnEvent(property)
						&& !IsHidden(property, type)
						&& !assemblyDocCache.HasExcludeTag(GetMemberName(property)))
					{
						WriteProperty(
							writer,
							property,
							property.DeclaringType.FullName != type.FullName,
							GetPropertyOverload(property, properties),
							IsHiding(property, type));
					}
				}
			}
		}

		private void WriteMethods(XmlWriter writer, Type type)
		{
			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MethodInfo[] methods = type.GetMethods(bindingFlags);

			foreach (MethodInfo method in methods)
			{
				string name = method.Name;

				int lastIndexOfDot = name.LastIndexOf('.');

				if (lastIndexOfDot != -1)
				{
					name = method.Name.Substring(lastIndexOfDot + 1);
				}

				if (!name.StartsWith("get_") &&
					!name.StartsWith("set_") &&
					!name.StartsWith("add_") &&
					!name.StartsWith("remove_") &&
					!name.StartsWith("op_") &&
					MustDocumentMethod(method) &&
					!IsHidden(method, type))
				{
					WriteMethod(
						writer,
						method,
						method.DeclaringType.FullName != type.FullName,
						GetMethodOverload(method, type),
						IsHiding(method, type));
				}
			}
		}

		private void WriteOperators(XmlWriter writer, Type type)
		{
			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MethodInfo[] methods = type.GetMethods(bindingFlags);
			foreach (MethodInfo method in methods)
			{
				if (method.Name.StartsWith("op_") &&
					MustDocumentMethod(method))
				{
					WriteOperator(
						writer,
						method,
						GetMethodOverload(method, type));
				}
			}
		}

		private void WriteEvents(XmlWriter writer, Type type)
		{
			foreach (EventInfo eventInfo in type.GetEvents(
				BindingFlags.Instance 
				| BindingFlags.Static
				| BindingFlags.Public
				| BindingFlags.NonPublic))
			{
				MethodInfo addMethod = eventInfo.GetAddMethod(true);

				if (addMethod != null &&
					MustDocumentMethod(addMethod) &&
					IsEditorBrowsable(eventInfo))
				{
					WriteEvent(writer, eventInfo);
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

			EventInfo [] events = type.GetEvents(bindingFlags);
			foreach (EventInfo eventInfo in events)
			{
				if (eventInfo.EventHandlerType.FullName == fullName)
				{
					isEvent = true;
					break;
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

		/// <summary>Writes XML documenting an interface.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Interface to document.</param>
		private void WriteInterface(XmlWriter writer, Type type)
		{
			string memberName = GetMemberName(type);

			string fullNameWithoutNamespace = type.FullName.Replace('+', '.');

			if (type.Namespace != null)
			{
				fullNameWithoutNamespace = fullNameWithoutNamespace.Substring(type.Namespace.Length + 1);
			}

			writer.WriteStartElement("interface");
			writer.WriteAttributeString("name", fullNameWithoutNamespace);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			WriteTypeDocumentation(writer, memberName, type);
			WriteCustomAttributes(writer, type);

			foreach(Type interfaceType in type.GetInterfaces())
			{
				if(MustDocumentType(interfaceType))
				{
					writer.WriteElementString("implements", interfaceType.Name);
				}
			}

			WriteProperties(writer, type);
			WriteMethods(writer, type);
			WriteEvents(writer, type);

			writer.WriteEndElement();
		}

		/// <summary>Writes XML documenting a delegate.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Delegate to document.</param>
		private void WriteDelegate(XmlWriter writer, Type type)
		{
			string memberName = GetMemberName(type);

			writer.WriteStartElement("delegate");
			writer.WriteAttributeString("name", GetNestedTypeName(type));
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic;

			MethodInfo[] methods = type.GetMethods(bindingFlags);
			foreach (MethodInfo method in methods)
			{
				if (method.Name == "Invoke")
				{
					Type t = method.ReturnType;
					writer.WriteAttributeString("returnType", GetTypeName(t));
					writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

					WriteDelegateDocumentation(writer, memberName, type, method);
					WriteCustomAttributes(writer, type);

					foreach (ParameterInfo parameter in method.GetParameters())
					{
						WriteParameter(writer, GetMemberName(method), parameter);
					}
				}
			}

			writer.WriteEndElement();
		}

		private string GetNestedTypeName(Type type)
		{
			int indexOfPlus = type.FullName.IndexOf('+');
			if (indexOfPlus != -1)
			{
				int lastIndexOfDot = type.FullName.LastIndexOf('.');
				return type.FullName.Substring(lastIndexOfDot + 1).Replace('+', '.');
			}
			else
			{
				return type.Name;
			}
		}

		/// <summary>Writes XML documenting an enumeration.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="type">Enumeration to document.</param>
		private void WriteEnumeration(XmlWriter writer, Type type)
		{
			string memberName = GetMemberName(type);

			writer.WriteStartElement("enumeration");
			writer.WriteAttributeString("name", GetNestedTypeName(type));
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			const BindingFlags bindingFlags =
					  BindingFlags.Instance |
					  BindingFlags.Static |
					  BindingFlags.Public |
					  BindingFlags.NonPublic |
					  BindingFlags.DeclaredOnly;

			foreach (FieldInfo field in type.GetFields(bindingFlags))
			{
				// Enums are normally based on Int32, but this is not a CLR requirement.
				// In fact, they may be based on any integer type. The value__ field
				// defines the enum's base type, so we will treat this seperately...
				if (field.Name == "value__")
				{
					if (field.FieldType.FullName != "System.Int32")
					{
						writer.WriteAttributeString("baseType",field.FieldType.FullName);
					}
					break;
				}
			}

			WriteEnumerationDocumentation(writer, memberName);
			WriteCustomAttributes(writer, type);

			foreach (FieldInfo field in type.GetFields(bindingFlags))
			{
				// value__ field handled above...
				if (field.Name != "value__")
				{
					WriteField(
						writer,
						field,
						type,
						IsHiding(field, type));
				}
			}

			writer.WriteEndElement();
		}

		private void WriteBaseType(XmlWriter writer, Type type)
		{
			if (!"System.Object".Equals(type.FullName))
			{
				writer.WriteStartElement("base");
				writer.WriteAttributeString("name", type.Name);
				writer.WriteAttributeString("id", GetMemberName(type));
				writer.WriteAttributeString("type", type.FullName.Replace('+', '.'));

				WriteBaseType(writer, type.BaseType);

				writer.WriteEndElement();
			}
		}

		/// <summary>Writes XML documenting a field.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="field">Field to document.</param>
		/// <param name="type">Type containing the field.</param>
		/// <param name="hiding">true if hiding base members</param>
		private void WriteField(
			XmlWriter writer,
			FieldInfo field,
			Type type,
			bool hiding)
		{
			string memberName = GetMemberName(field);

			writer.WriteStartElement("field");
			writer.WriteAttributeString("name", field.Name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetFieldAccessValue(field));
			
			if (field.IsStatic)
			{
				writer.WriteAttributeString("contract", "Static");
			}
			else
			{
				writer.WriteAttributeString("contract", "Normal");
			}

			Type t = field.FieldType;
			writer.WriteAttributeString("type", GetTypeName(t));
			writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

			bool inherited = (field.DeclaringType != field.ReflectedType);
			if (inherited)
			{
				writer.WriteAttributeString("declaringType", field.DeclaringType.FullName.Replace('+', '.'));
			}

			if ( !IsMemberSafe( field ) )
				writer.WriteAttributeString( "unsafe", "true" );

			if (hiding)
			{
				writer.WriteAttributeString("hiding", "true");
			}

			if (field.IsInitOnly)
			{
				writer.WriteAttributeString("initOnly", "true");
			}

			if (field.IsLiteral)
			{
				writer.WriteAttributeString("literal", "true");
			}

			if (inherited)
			{
				WriteInheritedDocumentation(writer, memberName, field.DeclaringType);
			}
			else
			{
				WriteFieldDocumentation(writer, memberName, type);
			}
			WriteCustomAttributes(writer, field);

			writer.WriteEndElement();
		}

		/// <summary>Writes XML documenting an event.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="eventInfo">Event to document.</param>
		private void WriteEvent(XmlWriter writer, EventInfo eventInfo)
		{
			string memberName = GetMemberName(eventInfo);

			string name = eventInfo.Name;
			string interfaceName = null;

			int lastIndexOfDot = name.LastIndexOf('.');
			if (lastIndexOfDot != -1)
			{
				//this is an explicit interface implementation. if we don't want
				//to document them, get out of here quick...
				if (!MyConfig.DocumentExplicitInterfaceImplementations) return;

				interfaceName = name.Substring(0, lastIndexOfDot);
				lastIndexOfDot = interfaceName.LastIndexOf('.');
				if (lastIndexOfDot != -1)
					name = name.Substring(lastIndexOfDot + 1);

				//check if we want to document this interface.
				ImplementsInfo implements = null;
				MethodInfo adder = eventInfo.GetAddMethod(true);
				if (adder != null)
				{
					implements = implementations[adder.ToString()];
				}
				if (implements == null)
				{
					MethodInfo remover = eventInfo.GetRemoveMethod(true);
					if (remover != null)
					{
						implements = implementations[remover.ToString()];
					}
				}
				if (implements != null) return;
			}

			writer.WriteStartElement("event");
			writer.WriteAttributeString("name", name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetMethodAccessValue(eventInfo.GetAddMethod(true)));
			writer.WriteAttributeString("contract", GetMethodContractValue(eventInfo.GetAddMethod(true)));
			Type t = eventInfo.EventHandlerType;
			writer.WriteAttributeString("type", GetTypeName(t));
			writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

			bool inherited = eventInfo.DeclaringType != eventInfo.ReflectedType;

			if (inherited)
			{
				writer.WriteAttributeString("declaringType", eventInfo.DeclaringType.FullName.Replace('+', '.'));
			}

			if (interfaceName != null)
			{
				writer.WriteAttributeString("interface", interfaceName);
			}

			if (eventInfo.IsMulticast)
			{
				writer.WriteAttributeString("multicast", "true");
			}

			if (inherited)
			{
				WriteInheritedDocumentation(writer, memberName, eventInfo.DeclaringType);
			}
			else
			{
				WriteEventDocumentation(writer, memberName, true);
			}
			WriteCustomAttributes(writer, eventInfo);

			if (implementations != null)
			{
				ImplementsInfo implements = null;
				MethodInfo adder = eventInfo.GetAddMethod(true);
				if (adder != null)
				{
					implements = implementations[adder.ToString()];
				}
				if (implements == null)
				{
					MethodInfo remover = eventInfo.GetRemoveMethod(true);
					if (remover != null)
					{
						implements = implementations[remover.ToString()];
					}
				}
				if (implements != null)
				{
					writer.WriteStartElement("implements");
					MemberInfo InterfaceMethod = (MemberInfo)implements.InterfaceMethod;
					EventInfo InterfaceEvent = 
						InterfaceMethod.DeclaringType.GetEvent(InterfaceMethod.Name.Substring(4));
					writer.WriteAttributeString("name", InterfaceEvent.Name);
					writer.WriteAttributeString("id",GetMemberName(InterfaceEvent));
					writer.WriteAttributeString("interface", implements.InterfaceType.Name);
					writer.WriteAttributeString("interfaceId", GetMemberName(implements.InterfaceType));
					writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName.Replace('+', '.'));
					writer.WriteEndElement();
				}
			}

			writer.WriteEndElement();
		}


		private bool IsMemberSafe( FieldInfo field )
		{
			return !field.FieldType.IsPointer;
		}

		private bool IsMemberSafe( PropertyInfo property )
		{
			return !property.PropertyType.IsPointer;
		}

		private bool IsMemberSafe( MethodBase method )
		{
			foreach (ParameterInfo parameter in method.GetParameters())
			{
				if ( parameter.GetType().IsPointer )
					return false;
			}
			return true;
		}

		private bool IsMemberSafe( MethodInfo method )
		{
			if ( method.ReturnType.IsPointer )
				return false;

			return IsMemberSafe( (MethodBase)method );
		}

		/// <summary>Writes XML documenting a constructor.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="constructor">Constructor to document.</param>
		/// <param name="overload">If &gt; 0, indicates this is the nth overloaded constructor.</param>
		private void WriteConstructor(XmlWriter writer, ConstructorInfo constructor, int overload)
		{
			string memberName = GetMemberName(constructor);

			writer.WriteStartElement("constructor");
			writer.WriteAttributeString("name", constructor.Name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetMethodAccessValue(constructor));
			writer.WriteAttributeString("contract", GetMethodContractValue(constructor));
			
			if (overload > 0)
			{
				writer.WriteAttributeString("overload", overload.ToString());
			}

			if ( !IsMemberSafe( constructor ) )
				writer.WriteAttributeString( "unsafe", "true" );

			WriteConstructorDocumentation(writer, memberName, constructor);
			WriteCustomAttributes(writer, constructor);

			foreach (ParameterInfo parameter in constructor.GetParameters())
			{
				WriteParameter(writer, GetMemberName(constructor), parameter);
			}

			writer.WriteEndElement();
		}

		/// <summary>Writes XML documenting a property.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="property">Property to document.</param>
		/// <param name="inherited">true if a declaringType attribute should be included.</param>
		/// <param name="overload">If &gt; 0, indicates this it the nth overloaded method with the same name.</param>
		/// <param name="hiding">true if this property is hiding base class members with the same name.</param>
		private void WriteProperty(
			XmlWriter writer,
			PropertyInfo property,
			bool inherited,
			int overload,
			bool hiding)
		{
			if (property != null)
			{
				string memberName = GetMemberName(property);

				string name = property.Name;
				string interfaceName = null;

				int lastIndexOfDot = name.LastIndexOf('.');
				if (lastIndexOfDot != -1)
				{
					//this is an explicit interface implementation. if we don't want
					//to document them, get out of here quick...
					if (!MyConfig.DocumentExplicitInterfaceImplementations) return;

					interfaceName = name.Substring(0, lastIndexOfDot);
					lastIndexOfDot = interfaceName.LastIndexOf('.');
					if (lastIndexOfDot != -1)
						name = name.Substring(lastIndexOfDot + 1);

					//check if we want to document this interface.
					ImplementsInfo implements = null;
					MethodInfo getter = property.GetGetMethod(true);
					if (getter != null)
					{
						implements = implementations[getter.ToString()];
					}
					if (implements == null)
					{
						MethodInfo setter = property.GetSetMethod(true);
						if (setter != null)
						{
							implements = implementations[setter.ToString()];
						}
					}
					if (implements==null) return;
				}

				writer.WriteStartElement("property");
				writer.WriteAttributeString("name", name);
				writer.WriteAttributeString("id", memberName);
				writer.WriteAttributeString("access", GetPropertyAccessValue(property));
				writer.WriteAttributeString("contract", GetPropertyContractValue(property));
				Type t = property.PropertyType;
				writer.WriteAttributeString("type", GetTypeName(t));
				writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

				if (inherited)
				{
					writer.WriteAttributeString("declaringType", property.DeclaringType.FullName.Replace('+', '.'));
				}

				if (overload > 0)
				{
					writer.WriteAttributeString("overload", overload.ToString());
				}

				if ( !IsMemberSafe( property ) )
					writer.WriteAttributeString( "unsafe", "true" );

				if (hiding)
				{
					writer.WriteAttributeString("hiding", "true");
				}

				if (interfaceName != null)
				{
					writer.WriteAttributeString("interface", interfaceName);
				}

				writer.WriteAttributeString("get", property.GetGetMethod(true) != null ? "true" : "false");
				writer.WriteAttributeString("set", property.GetSetMethod(true) != null ? "true" : "false");

				if (inherited)
				{
					WriteInheritedDocumentation(writer, memberName, property.DeclaringType);
				}
				else
				{
					WritePropertyDocumentation(writer, memberName, property, true);
				}
				WriteCustomAttributes(writer, property);

				foreach (ParameterInfo parameter in GetIndexParameters(property))
				{
					WriteParameter(writer, memberName, parameter);
				}

				if (implementations != null)
				{
					ImplementsInfo implements = null;
					MethodInfo getter = property.GetGetMethod(true);
					if (getter != null)
					{
						implements = implementations[getter.ToString()];
					}
					if (implements == null)
					{
						MethodInfo setter = property.GetSetMethod(true);
						if (setter != null)
						{
							implements = implementations[setter.ToString()];
						}
					}
					if (implements != null)
					{
						writer.WriteStartElement("implements");
						MethodInfo InterfaceMethod = (MethodInfo)implements.InterfaceMethod;
						PropertyInfo InterfaceProperty = DerivePropertyFromAccessorMethod(InterfaceMethod);
						string InterfacePropertyID=GetMemberName(InterfaceProperty);
						writer.WriteAttributeString("name", InterfaceProperty.Name);
						writer.WriteAttributeString("id",InterfacePropertyID);
						writer.WriteAttributeString("interface", implements.InterfaceType.Name);
						writer.WriteAttributeString("interfaceId", GetMemberName(implements.InterfaceType));
						writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName.Replace('+', '.'));
						writer.WriteEndElement();
					}
				}

				writer.WriteEndElement();
			}
		}

		private PropertyInfo DerivePropertyFromAccessorMethod(MemberInfo accessor)
		{
			MethodInfo accessorMethod = (MethodInfo)accessor;
			string accessortype = accessorMethod.Name.Substring(0,3);
			string propertyName = accessorMethod.Name.Substring(4);

			ParameterInfo[] parameters;
			parameters = accessorMethod.GetParameters();
			int parmCount = parameters.GetLength(0);
			
			Type   returnType = null;
			Type[] types      = null;

			if (accessortype=="get")
			{
				returnType = accessorMethod.ReturnType;
				types = new Type[parmCount];
				for(int i=0; i<parmCount;i++)
				{
					types[i]=((ParameterInfo)parameters.GetValue(i)).ParameterType;
				}
			}
			else
			{
				returnType = ((ParameterInfo)parameters.GetValue(parmCount-1)).ParameterType;
				parmCount--;
				types = new Type[parmCount];
				for(int i=0; i<parmCount;i++)
				{
					types[i]=((ParameterInfo)parameters.GetValue(i+1)).ParameterType;
				}
			}

			PropertyInfo derivedProperty= accessorMethod.DeclaringType.GetProperty(propertyName,returnType,types);
			return derivedProperty;
		}

		private string GetPropertyContractValue(PropertyInfo property)
		{
			return GetMethodContractValue(property.GetAccessors(true)[0]);
		}

		private ParameterInfo[] GetIndexParameters(PropertyInfo property)
		{
			// The ParameterInfo[] returned by GetIndexParameters()
			// contains ParameterInfo objects with empty names so
			// we have to get the parameters from the getter or
			// setter instead.

			ParameterInfo[] parameters;
			int length = 0;

			if (property.GetGetMethod(true) != null)
			{
				parameters = property.GetGetMethod(true).GetParameters();

				if (parameters != null)
				{
					length = parameters.Length;
				}
			}
			else
			{
				parameters = property.GetSetMethod(true).GetParameters();

				if (parameters != null)
				{
					// If the indexer only has a setter, we neet
					// to subtract 1 so that the value parameter
					// isn't displayed.

					length = parameters.Length - 1;
				}
			}

			ParameterInfo[] result = new ParameterInfo[length];

			if (length > 0)
			{
				for (int i = 0; i < length; ++i)
				{
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
		private void WriteMethod(
			XmlWriter writer,
			MethodInfo method,
			bool inherited,
			int overload,
			bool hiding)
		{
			if (method != null)
			{
				string memberName = GetMemberName(method);

				string name = method.Name;
				string interfaceName = null;

				name=name.Replace('+','.');
				int lastIndexOfDot = name.LastIndexOf('.');
				if (lastIndexOfDot != -1)
				{
					//this is an explicit interface implementation. if we don't want
					//to document them, get out of here quick...
					if (!MyConfig.DocumentExplicitInterfaceImplementations) return;

					interfaceName = name.Substring(0, lastIndexOfDot);
					lastIndexOfDot = interfaceName.LastIndexOf('.');
					if (lastIndexOfDot != -1)
						name = name.Substring(lastIndexOfDot + 1);

					//check if we want to document this interface.
					ImplementsInfo implements = implementations[method.ToString()];
					if (implements==null) return;
				}

				writer.WriteStartElement("method");
				writer.WriteAttributeString("name", name);
				writer.WriteAttributeString("id", memberName);
				writer.WriteAttributeString("access", GetMethodAccessValue(method));
				writer.WriteAttributeString("contract", GetMethodContractValue(method));
				Type t = method.ReturnType;
				writer.WriteAttributeString("returnType", GetTypeName(t));
				writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

				if (inherited)
				{
					writer.WriteAttributeString("declaringType", method.DeclaringType.FullName.Replace('+', '.'));
				}

				if (overload > 0)
				{
					writer.WriteAttributeString("overload", overload.ToString());
				}

				if ( !IsMemberSafe( method ) )
					writer.WriteAttributeString( "unsafe", "true" );

				if (hiding)
				{
					writer.WriteAttributeString("hiding", "true");
				}

				if (interfaceName != null)
				{
					writer.WriteAttributeString("interface", interfaceName);
				}

				if (inherited)
				{
					WriteInheritedDocumentation(writer, memberName, method.DeclaringType);
				}
				else
				{
					WriteMethodDocumentation(writer, memberName, method, true);
				}

				WriteCustomAttributes(writer, method);

				foreach (ParameterInfo parameter in method.GetParameters())
				{
					WriteParameter(writer, GetMemberName(method), parameter);
				}

				if (implementations != null)
				{
					ImplementsInfo implements = implementations[method.ToString()];
					if (implements != null)
					{
						writer.WriteStartElement("implements");
						writer.WriteAttributeString("name", implements.InterfaceMethod.Name);
						writer.WriteAttributeString("id",GetMemberName((MethodBase)implements.InterfaceMethod));
						writer.WriteAttributeString("interface", implements.InterfaceType.Name);
						writer.WriteAttributeString("interfaceId", GetMemberName(implements.InterfaceType));
						writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName.Replace('+', '.'));
						writer.WriteEndElement();
					}
				}

				writer.WriteEndElement();
			}
		}

		private void WriteParameter(XmlWriter writer, string memberName, ParameterInfo parameter)
		{
			string direction = "in";
			bool isParamArray = false;

			if (parameter.ParameterType.IsByRef)
			{
				direction = parameter.IsOut ? "out" : "ref";
			}

			if (parameter.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
			{
				isParamArray = true;
			}

			writer.WriteStartElement("parameter");
			writer.WriteAttributeString("name", parameter.Name);
			
			Type t = parameter.ParameterType;
			writer.WriteAttributeString("type", GetTypeName(t));
			writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

			if ( t.IsPointer )
				writer.WriteAttributeString( "unsafe", "true" );

			if (parameter.IsOptional)
			{
				writer.WriteAttributeString("optional", "true");
				if (parameter.DefaultValue != null)
				{
					writer.WriteAttributeString("defaultValue", parameter.DefaultValue.ToString());
				}
				else
				{
					//HACK: assuming this is only for VB syntax
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

			WriteCustomAttributes(writer, parameter);

			writer.WriteEndElement();
		}

		private void WriteOperator(
			XmlWriter writer,
			MethodInfo method,
			int overload)
		{
			if (method != null)
			{
				string memberName = GetMemberName(method);

				writer.WriteStartElement("operator");
				writer.WriteAttributeString("name", method.Name);
				writer.WriteAttributeString("id", memberName);
				writer.WriteAttributeString("access", GetMethodAccessValue(method));
				writer.WriteAttributeString("contract", GetMethodContractValue(method));
				Type t = method.ReturnType;
				writer.WriteAttributeString("returnType", GetTypeName(t));
				writer.WriteAttributeString("valueType", t.IsValueType.ToString().ToLower());

				bool inherited = method.DeclaringType != method.ReflectedType;

				if (inherited)
				{
					writer.WriteAttributeString("declaringType", method.DeclaringType.FullName.Replace('+', '.'));
				}

				if (overload > 0)
				{
					writer.WriteAttributeString("overload", overload.ToString());
				}

				if ( !IsMemberSafe( method ) )
					writer.WriteAttributeString( "unsafe", "true" );

				if (inherited)
				{
					WriteInheritedDocumentation(writer, memberName, method.DeclaringType);
				}
				else
				{
					WriteMethodDocumentation(writer, memberName, method, true);
				}

				WriteCustomAttributes(writer, method);

				foreach (ParameterInfo parameter in method.GetParameters())
				{
					WriteParameter(writer, GetMemberName(method), parameter);
				}

				writer.WriteEndElement();
			}
		}

		/// <summary>Used by GetMemberName(Type type) and by
		/// GetFullNamespaceName(MemberInfo member) functions to build
		/// up most of the /doc member name.</summary>
		/// <param name="type"></param>
		private string GetTypeNamespaceName(Type type)
		{
			return type.FullName.Replace('+', '.');
		}

		/// <summary>Used by all the GetMemberName() functions except the
		/// Type one. It returns most of the /doc member name.</summary>
		/// <param name="member"></param>
		private string GetFullNamespaceName(MemberInfo member)
		{
			return GetTypeNamespaceName(member.ReflectedType);
		}

		/// <summary>Derives the ID for a type. Used to match nodes in the /doc XML.</summary>
		/// <param name="type">The type to derive the member name ID from.</param>
		private string GetTypeName(Type type)
		{
			return type.FullName.Replace('+', '.').Replace("&", null).Replace('+', '#');
		}

		/// <summary>Derives the member name ID for a type. Used to match nodes in the /doc XML.</summary>
		/// <param name="type">The type to derive the member name ID from.</param>
		private string GetMemberName(Type type)
		{
			return "T:" + type.FullName.Replace('+', '.');
		}

		/// <summary>Derives the member name ID for a field. Used to match nodes in the /doc XML.</summary>
		/// <param name="field">The field to derive the member name ID from.</param>
		private string GetMemberName(FieldInfo field)
		{
			return "F:" + GetFullNamespaceName(field) + "." + field.Name;
		}

		/// <summary>Derives the member name ID for an event. Used to match nodes in the /doc XML.</summary>
		/// <param name="eventInfo">The event to derive the member name ID from.</param>
		private string GetMemberName(EventInfo eventInfo)
		{
			return "E:" + GetFullNamespaceName(eventInfo) + 
				"." + eventInfo.Name.Replace('.', '#').Replace('+', '#');
		}

		/// <summary>Derives the member name ID for a property.  Used to match nodes in the /doc XML.</summary>
		/// <param name="property">The property to derive the member name ID from.</param>
		private string GetMemberName(PropertyInfo property)
		{
			string memberName;

			memberName = "P:" + GetFullNamespaceName(property) + 
				"." + property.Name.Replace('.', '#').Replace('+', '#');

			try
			{
				if (property.GetIndexParameters().Length > 0)
				{
					memberName += "(";

					int i = 0;

					foreach (ParameterInfo parameter in property.GetIndexParameters())
					{
						if (i > 0)
						{
							memberName += ",";
						}

						memberName += parameter.ParameterType.FullName;

						++i;
					}

					memberName += ")";
				}
			}
			catch(System.Security.SecurityException){}

			return memberName;
		}

		/// <summary>Derives the member name ID for a member function. Used to match nodes in the /doc XML.</summary>
		/// <param name="method">The method to derive the member name ID from.</param>
		private string GetMemberName(MethodBase method)
		{
			string memberName;

			memberName =
				"M:" +
				GetFullNamespaceName(method) +
				"." +
				method.Name.Replace('.', '#').Replace('+', '#');

			int i = 0;

			foreach (ParameterInfo parameter in method.GetParameters())
			{
				if (i == 0)
				{
					memberName += "(";
				}
				else
				{
					memberName += ",";
				}

				string parameterName = parameter.ParameterType.FullName;

				parameterName = parameterName.Replace(",", ",0:");
				parameterName = parameterName.Replace(@"\[,", "[0:,");

				// XML Documentation file appends a "@" to reference and out types, not a "&"
				memberName += parameterName.Replace('&', '@').Replace('+', '.');

				++i;
			}

			if (i > 0)
			{
				memberName += ")";
			}

			if (method is MethodInfo)
			{
				MethodInfo mi = (MethodInfo)method;
				if (mi.Name == "op_Implicit" || mi.Name == "op_Explicit")
				{
					memberName += "~" + mi.ReturnType;
				}
			}
			
			return memberName;
		}

		private void WriteSlashDocElements(XmlWriter writer, string memberName)
		{
			string temp=assemblyDocCache.GetDoc(memberName);
			if (temp!=null)	
			{
				WriteStartDocumentation(writer);
				writer.WriteRaw(temp);
			}
		}

		private string GetTypeAccessValue(Type type)
		{
			string result = "Unknown";

			switch (type.Attributes & TypeAttributes.VisibilityMask)
			{
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
					if (MyConfig.DocumentProtectedInternalAsProtected)
					{
						result = "NestedFamily";
					}
					else
					{
						result = "NestedFamilyOrAssembly";
					}
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

			switch (field.Attributes & FieldAttributes.FieldAccessMask)
			{
				case FieldAttributes.Public:
					result = "Public";
					break;
				case FieldAttributes.Family:
					result = "Family";
					break;
				case FieldAttributes.FamORAssem:
					if (MyConfig.DocumentProtectedInternalAsProtected)
					{
						result = "Family";
					}
					else
					{
						result = "FamilyOrAssembly";
					}
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
			MethodInfo method;

			if (property.GetGetMethod(true) != null)
			{
				method = property.GetGetMethod(true);
			}
			else
			{
				method = property.GetSetMethod(true);
			}

			return GetMethodAccessValue(method);
		}

		private string GetMethodAccessValue(MethodBase method)
		{
			string result;

			switch (method.Attributes & MethodAttributes.MemberAccessMask)
			{
				case MethodAttributes.Public:
					result = "Public";
					break;
				case MethodAttributes.Family:
					result = "Family";
					break;
				case MethodAttributes.FamORAssem:
					if (MyConfig.DocumentProtectedInternalAsProtected)
					{
						result = "Family";
					}
					else
					{
						result = "FamilyOrAssembly";
					}
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

		private string GetMethodContractValue(MethodBase method)
		{
			string  result;
			MethodAttributes methodAttributes = method.Attributes;

			if ((methodAttributes & MethodAttributes.Static) > 0)
			{
				result = "Static";
			}
			else if ((methodAttributes & MethodAttributes.Abstract) > 0)
			{
				result = "Abstract";
			}
			else if ((methodAttributes & MethodAttributes.Final) > 0)
			{
				result = "Final";
			}
			else if ((methodAttributes & MethodAttributes.Virtual) > 0)
			{
				if ((methodAttributes & MethodAttributes.NewSlot) > 0)
				{
					result = "Virtual";
				}
				else
				{
					result = "Override";
				}
			}
			else
			{
				result = "Normal";
			}

			return result;
		}

		private StringCollection GetNamespaceNames(Type[] types)
		{
			StringCollection namespaceNames = new StringCollection();

			foreach (Type type in types)
			{
				if (namespaceNames.Contains(type.Namespace) == false)
				{
					namespaceNames.Add(type.Namespace);
				}
			}

			return namespaceNames;
		}

		private void CheckForMissingSummaryAndRemarks(
			XmlWriter writer,
			string memberName)
		{
			if (MyConfig.ShowMissingSummaries)
			{
				bool bMissingSummary=true;
				string xmldoc=assemblyDocCache.GetDoc(memberName);

				if (xmldoc!=null)
				{
					XmlTextReader reader= new XmlTextReader(xmldoc,XmlNodeType.Element,null);
					while (reader.Read()) 
					{
						if (reader.NodeType == XmlNodeType.Element) 
						{
							if (reader.Name=="summary") 
							{
								string summarydetails =reader.ReadInnerXml();
								if (summarydetails.Length>0 && !summarydetails.Trim().StartsWith("Summary description for"))
								{
									bMissingSummary=false;
									break;
								}
							}
						}
					}
				}

				if (bMissingSummary)
				{
					WriteMissingDocumentation(writer, "summary", null,
						"Missing <summary> documentation for " + memberName);
					Debug.WriteLine("@@missing@@\t" + memberName);
				}
			}

			if (MyConfig.ShowMissingRemarks)
			{
				bool bMissingRemarks=true;
				string xmldoc=assemblyDocCache.GetDoc(memberName);

				if (xmldoc!=null)
				{
					XmlTextReader reader= new XmlTextReader(xmldoc,XmlNodeType.Element,null);
					while (reader.Read()) 
					{
						if (reader.NodeType == XmlNodeType.Element) 
						{
							if (reader.Name=="remarks")
							{
								string remarksdetails =reader.ReadInnerXml();
								if (remarksdetails.Length>0)
								{
									bMissingRemarks=false;
									break;
								}
							}
						}
					}
				}

				if (bMissingRemarks)
				{
					WriteMissingDocumentation(writer, "remarks", null,
						"Missing <remarks> documentation for " + memberName);
				}
			}
		}

		private void CheckForMissingParams(
			XmlWriter writer,
			string memberName,
			ParameterInfo[] parameters)
		{
			if (MyConfig.ShowMissingParams)
			{
				string xmldoc=assemblyDocCache.GetDoc(memberName);
				foreach (ParameterInfo parameter in parameters)
				{
					bool bMissingParams=true;

					if (xmldoc!=null)
					{
						XmlTextReader reader= new XmlTextReader(xmldoc,XmlNodeType.Element,null);
						while (reader.Read()) 
						{
							if (reader.NodeType == XmlNodeType.Element) 
							{
								if (reader.Name=="param") 
								{
									string name= reader.GetAttribute("name");
									if (name==parameter.Name)
									{
										string paramsdetails = reader.ReadInnerXml();
										if(paramsdetails.Length>0)
										{ 
											bMissingParams=false;
											break; // we can stop if we locate what we are looking for
										}
									}
								}
							}
						}
					}

					if (bMissingParams)
					{
						WriteMissingDocumentation(writer, "param", parameter.Name,
							"Missing <param> documentation for " + parameter.Name);
					}
				}
			}
		}

		private void CheckForMissingReturns(
			XmlWriter writer,
			string memberName,
			MethodInfo method)
		{
			if (MyConfig.ShowMissingReturns &&
				!"System.Void".Equals(method.ReturnType.FullName))
			{
				string xmldoc=assemblyDocCache.GetDoc(memberName);
				bool bMissingReturns=true;

				if (xmldoc!=null)
				{
					XmlTextReader reader= new XmlTextReader(xmldoc,XmlNodeType.Element,null);
					while (reader.Read()) 
					{
						if (reader.NodeType == XmlNodeType.Element) 
						{
							if (reader.Name=="returns") 
							{
								string returnsdetails =reader.ReadInnerXml();
								if (returnsdetails.Length>0)
								{ 
									bMissingReturns=false;
									break; // we can stop if we locate what we are looking for
								}
							}
						}
					}
				}

				if (bMissingReturns)
				{
					WriteMissingDocumentation(writer, "returns", null,
						"Missing <returns> documentation for " + memberName);
				}
			}
		}

		private void CheckForMissingValue(
			XmlWriter writer,
			string memberName)
		{
			if (MyConfig.ShowMissingValues)
			{
				string xmldoc=assemblyDocCache.GetDoc(memberName);
				bool bMissingValues=true;

				if (xmldoc!=null)
				{
					XmlTextReader reader= new XmlTextReader(xmldoc,XmlNodeType.Element,null);
					while (reader.Read()) 
					{
						if (reader.NodeType == XmlNodeType.Element) 
						{
							if (reader.Name=="value") 
							{
								string valuesdetails =reader.ReadInnerXml();
								if (valuesdetails.Length>0)
								{
									bMissingValues=false;
									break; // we can stop if we locate what we are looking for
								}
							}
						}
					}
				}

				if (bMissingValues)
				{
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

			if (name != null)
			{
				writer.WriteAttributeString("name", name);
			}

			writer.WriteStartElement("span");
			writer.WriteAttributeString("class", "missing");
			writer.WriteString(message);
			writer.WriteEndElement();

			writer.WriteEndElement();
		}

		private bool didWriteStartDocumentation = false;

		private void WriteStartDocumentation(XmlWriter writer)
		{
			if (!didWriteStartDocumentation)
			{
				writer.WriteStartElement("documentation");
				didWriteStartDocumentation = true;
			}
		}

		private void WriteEndDocumentation(XmlWriter writer)
		{
			if (didWriteStartDocumentation)
			{
				writer.WriteEndElement();
				didWriteStartDocumentation = false;
			}
		}

		private void WriteInheritedDocumentation(
			XmlWriter writer, 
			string memberName, 
			Type declaringType)
		{
			if (MyConfig.GetExternalSummaries)
			{
				string summary = externalSummaryCache.GetSummary(memberName, declaringType);
				if (summary.Length > 0)
				{
					WriteStartDocumentation(writer);
					writer.WriteRaw(summary);
					WriteEndDocumentation(writer);
				}
			}
		}

		private void WriteTypeDocumentation(
			XmlWriter writer,
			string memberName,
			Type type)
		{
			CheckForMissingSummaryAndRemarks(writer, memberName);
			WriteSlashDocElements(writer, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteDelegateDocumentation(
			XmlWriter writer,
			string memberName,
			Type type,
			MethodInfo method)
		{
			CheckForMissingParams(writer, memberName, method.GetParameters());
			CheckForMissingReturns(writer, memberName, method);
			WriteTypeDocumentation(writer, memberName, type);
			WriteEndDocumentation(writer);
		}

		private void WriteEnumerationDocumentation(XmlWriter writer, string memberName)
		{
			CheckForMissingSummaryAndRemarks(writer, memberName);
			WriteSlashDocElements(writer, memberName);
			WriteEndDocumentation(writer);
		}

		//if the constructor has no parameters and no summary,
		//add a default summary text.
		private bool DoAutoDocumentConstructor(
			XmlWriter writer,
			string memberName,
			ConstructorInfo constructor)
		{
			BaseDocumenterConfig conf = (BaseDocumenterConfig)config;
			if (conf.AutoDocumentConstructors)
			{		
				if (constructor.GetParameters().Length == 0)
				{
					string xmldoc=assemblyDocCache.GetDoc(memberName);
					bool bMissingSummary=true;

					if (xmldoc!=null)
					{
						XmlTextReader reader= new XmlTextReader(xmldoc,XmlNodeType.Element,null);
						while (reader.Read()) 
						{
							if (reader.NodeType == XmlNodeType.Element) 
							{
								if (reader.Name=="summary") 
								{
									string summarydetails =reader.ReadInnerXml();
									if (summarydetails.Length>0 && !summarydetails.Trim().StartsWith("Summary description for"))
									{ 
										bMissingSummary=false;
									}
								}
							}
						}
					}

					if (bMissingSummary)
					{
						WriteStartDocumentation(writer);
						if (constructor.IsStatic)
						{
							writer.WriteElementString("summary", "Initializes the static fields of the " 
								+ constructor.DeclaringType.Name + " class.");
						}
						else
						{
							writer.WriteElementString("summary", "Initializes a new instance of the " 
								+ constructor.DeclaringType.Name + " class.");
						}
						return true;
					}
				}
			}
			return false;
		}

		private void WriteConstructorDocumentation(
			XmlWriter writer,
			string memberName,
			ConstructorInfo constructor)
		{
			if (!DoAutoDocumentConstructor(writer, memberName, constructor))
			{
				CheckForMissingSummaryAndRemarks(writer, memberName);
				CheckForMissingParams(writer, memberName, constructor.GetParameters());
			}
			WriteSlashDocElements(writer, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteFieldDocumentation(
			XmlWriter writer,
			string memberName,
			Type type)
		{
			if (!CheckForPropertyBacker(writer, memberName, type))
			{
				CheckForMissingSummaryAndRemarks(writer, memberName);
			}
			WriteSlashDocElements(writer, memberName);
			WriteEndDocumentation(writer);
		}

		private void WritePropertyDocumentation(
			XmlWriter writer,
			string memberName,
			PropertyInfo property,
			bool writeMissing)
		{
			if (writeMissing)
			{
				CheckForMissingSummaryAndRemarks(writer, memberName);
				CheckForMissingParams(writer, memberName, GetIndexParameters(property));
				CheckForMissingValue(writer, memberName);
			}
			WriteSlashDocElements(writer, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteMethodDocumentation(
			XmlWriter writer,
			string memberName,
			MethodInfo method,
			bool writeMissing)
		{
			if (writeMissing)
			{
				CheckForMissingSummaryAndRemarks(writer, memberName);
				CheckForMissingParams(writer, memberName, method.GetParameters());
				CheckForMissingReturns(writer, memberName, method);
			}
			WriteSlashDocElements(writer, memberName);
			WriteEndDocumentation(writer);
		}

		private void WriteEventDocumentation(
			XmlWriter writer,
			string memberName,
			bool writeMissing)
		{
			if (writeMissing)
			{
				CheckForMissingSummaryAndRemarks(writer, memberName);
			}
			WriteSlashDocElements(writer, memberName);
			WriteEndDocumentation(writer);
		}

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
		/// <param name="memberName">The full name of the field.</param>
		/// <param name="type">The Type which contains the field
		/// and potentially the property.</param>
		/// <returns>True only if a property backer is auto-documented.</returns>
		private bool CheckForPropertyBacker(
			XmlWriter writer,
			string memberName,
			Type type)
		{
			if (!MyConfig.AutoPropertyBackerSummaries) return false;

			// determine if field is non-public
			// (because public fields are probably not backers for properties)
			bool isNonPublic = true;  // stubbed out for now

			//check whether or not we have a valid summary
			bool isMissingSummary=true;
			string xmldoc=assemblyDocCache.GetDoc(memberName);
			if (xmldoc!=null)
			{
				XmlTextReader reader= new XmlTextReader(xmldoc,XmlNodeType.Element,null);
				while (reader.Read()) 
				{
					if (reader.NodeType == XmlNodeType.Element) 
					{
						if (reader.Name=="summary") 
						{
							string summarydetails =reader.ReadInnerXml();
							if (summarydetails.Length>0 && !summarydetails.Trim().StartsWith("Summary description for"))
							{
								isMissingSummary=false;
							}
						}
					}
				}
			}

			// only do this if there is no summary already
			if (isMissingSummary && isNonPublic)
			{
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
					type)) != null))
				{
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
			foreach (PropertyInfo property in properties)
			{
				if (property.Name.Equals(expectedPropertyName))
				{
					MethodInfo getMethod = property.GetGetMethod(true);
					MethodInfo setMethod = property.GetSetMethod(true);

					bool hasGetter = (getMethod != null) && MustDocumentMethod(getMethod);
					bool hasSetter = (setMethod != null) && MustDocumentMethod(setMethod);

					if ((hasGetter || hasSetter) && !IsAlsoAnEvent(property))
					{
						return(property);
					}
				}
			}

			return(null);
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


		/// <summary>Loads an assembly.</summary>
		/// <param name="fileName">The assembly filename.</param>
		/// <returns>The assembly object.</returns>
		/// <remarks>This method loads an assembly into memory. If you
		/// use Assembly.Load or Assembly.LoadFrom the assembly file locks.
		/// This method doesn't lock the assembly file.</remarks>
		public static Assembly LoadAssembly(string fileName)
		{
			// apparently need to cd there if it references native dlls
			// (and those dlls are in that dir)
			string oldDir = Directory.GetCurrentDirectory();
			string dirName = Path.GetDirectoryName(fileName);
			Directory.SetCurrentDirectory(dirName);
			string baseName = Path.GetFileName(fileName);

			Trace.WriteLine(String.Format("LoadAssembly: Trying to load {0} from dir {1}", 
				baseName, dirName));
			Assembly assy = null;
			try
			{
				assy = Assembly.LoadFrom(baseName);
			}
			catch(Exception e)
			{
				Directory.SetCurrentDirectory(oldDir);
				Console.WriteLine("Error: LoadAssembly: Unable to load assembly {0} in dir {1}",
					baseName, dirName);
				Console.WriteLine("Error: LoadAssembly: Exception is {0}", e.ToString());
				throw e;
			}

			Directory.SetCurrentDirectory(oldDir);
			return(assy);
		}
	}
}
