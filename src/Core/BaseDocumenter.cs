// Documenter.cs - base XML documenter code
// Copyright (C) 2001  Kral Ferch, Jason Diamond
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
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace NDoc.Core
{
	/// <summary>Provides the base class for documenters.</summary>
	abstract public class BaseDocumenter : IDocumenter, IComparable
	{
		IDocumenterConfig config;

		Project _Project;

		Assembly currentAssembly;
		XmlDocument currentSlashDoc;

		XmlDocument xmlDocument;

		private class ImplementsInfo
		{
			public Type TargetType;
			public MemberInfo TargetMethod;
			public Type InterfaceType;
			public MemberInfo InterfaceMethod;
		}
		private class ImplementsCollection : NameObjectCollectionBase
		{
			public ImplementsInfo this [int index]
			{
				get { return (ImplementsInfo)BaseGet(index); }
				set { BaseSet(index, value); }
			}
			public ImplementsInfo this [string name]
			{
				get { return (ImplementsInfo)BaseGet(name); }
				set { BaseSet(name, value); }
			}
		}
		ImplementsCollection implementations;

		/// <summary>Initialized a new BaseDocumenter instance.</summary>
		protected BaseDocumenter(string name)
		{
			_Name = name;
			xmlDocument = new XmlDocument();
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

		/// <summary>Gets the XmlDocument containing the combined relected metadata and /doc comments.</summary>
		protected XmlDocument Document
		{
			get { return xmlDocument; }
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

		/// <summary>Builds an XmlDocument combining the reflected metadata with the /doc comments.</summary>
		protected void MakeXml(Project project)
		{
			_Project = project;

			AssemblyResolver assemblyResolver = null;

			string referencesPath = MyConfig.ReferencesPath;

			if (string.Empty != referencesPath)
			{
				assemblyResolver = new AssemblyResolver(referencesPath);
				assemblyResolver.Install();
			}

			// Sucks that there's no XmlNodeWriter. Instead we
			// have to write out to a file then load back in.
			// For performance, we'll write to a memory stream instead of a file.
			MemoryStream memoryStream = new MemoryStream();
			XmlWriter writer = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8);

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

				int step = 100 / project.AssemblySlashDocCount;
				int i = 0;

				foreach (AssemblySlashDoc assemblySlashDoc in project.GetAssemblySlashDocs())
				{
					OnDocBuildingProgress(i * step);

					currentAssemblyFilename = assemblySlashDoc.AssemblyFilename;
					string path = Path.GetFullPath(currentAssemblyFilename);
					currentAssembly = Assembly.LoadFrom(path);

					currentSlashDoc = new XmlDocument();
					currentSlashDoc.Load(assemblySlashDoc.SlashDocFilename);

					Write(writer);

					i++;
				}

				OnDocBuildingProgress(100);

				writer.WriteEndElement();

				writer.WriteEndDocument();

				//writer.Close();
				writer.Flush();

				// write our intermediate xml to a file for debugging
#if DEBUG
				FileStream fs = new FileStream(@"C:\test.xml", FileMode.Create);
				fs.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
				fs.Close();
#endif

				// xmlDocument.Load(new MemoryStream(memoryStream.GetBuffer()));
				memoryStream.Position = 0;
				xmlDocument.Load(memoryStream);
				writer.Close();
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
				writer.Close();

				if (null != assemblyResolver)
				{
					assemblyResolver.Deinstall();
				}
			}
		}

		private bool MustDocumentType(Type type)
		{
			Type declaringType = type.DeclaringType;

			return !type.FullName.StartsWith("<PrivateImplementationDetails>") &&
				(declaringType == null || MustDocumentType(declaringType)) &&
				(type.IsPublic ||
				(type.IsNotPublic && MyConfig.DocumentInternals) ||
				type.IsNestedPublic ||
				(type.IsNestedFamily && MyConfig.DocumentProtected) ||
				(type.IsNestedFamORAssem && MyConfig.DocumentProtected) ||
				(type.IsNestedAssembly && MyConfig.DocumentInternals) ||
				(type.IsNestedFamANDAssem && MyConfig.DocumentInternals) ||
				(type.IsNestedPrivate && MyConfig.DocumentPrivates));
		}

		private bool MustDocumentMethod(MethodBase method)
		{
			// Methods containing '.' in their name that aren't constructors are probably
			// explicit interface implementations, we check whether we document those or not.
			if((method.Name.IndexOf('.') != -1 && method.Name != ".ctor"))
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
						return true;
					}
				}
			}

			// All other methods
			return (method.IsPublic ||
				(method.IsFamily && MyConfig.DocumentProtected) ||
				(method.IsFamilyOrAssembly && MyConfig.DocumentProtected) ||
				(method.IsAssembly && MyConfig.DocumentInternals) ||
				(method.IsFamilyAndAssembly && MyConfig.DocumentInternals) ||
				(method.IsPrivate && MyConfig.DocumentPrivates));
		}

		private bool MustDocumentField(FieldInfo field)
		{
			return (field.IsPublic ||
				(field.IsFamily && MyConfig.DocumentProtected) ||
				(field.IsFamilyOrAssembly && MyConfig.DocumentProtected) ||
				(field.IsAssembly && MyConfig.DocumentInternals) ||
				(field.IsFamilyAndAssembly && MyConfig.DocumentInternals) ||
				(field.IsPrivate && MyConfig.DocumentPrivates));
		}

		private void Write(XmlWriter writer)
		{
			AssemblyName assemblyName = currentAssembly.GetName();

			if (MyConfig.CopyrightText != string.Empty)
			{
				writer.WriteStartElement("copyright");
				writer.WriteAttributeString("text", MyConfig.CopyrightText);

				if (MyConfig.CopyrightHref != string.Empty)
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

			writer.WriteStartElement("assembly");
			writer.WriteAttributeString("name", assemblyName.Name);

			if (MyConfig.IncludeAssemblyVersion)
			{
				writer.WriteAttributeString("version", assemblyName.Version.ToString());
			}

			foreach(Module module in currentAssembly.GetModules())
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

				string namespaceSummary = _Project.GetNamespaceSummary(ourNamespaceName);

				if (MyConfig.SkipNamespacesWithoutSummaries &&
					(namespaceSummary == null || namespaceSummary.Length == 0))
				{
					Trace.WriteLine(string.Format("Skipping namespace {0}...", namespaceName));
				}
				else
				{
					Trace.WriteLine(string.Format("Writing namespace {0}...", namespaceName));

					MemoryStream xmlMemoryStream = null;
					XmlWriter tempWriter = writer;

					// If we don't want empty namespaces, we need to write the XML to a temporary
					// writer, because we'll only know if its empty once the WriteXxx methods
					// have been called.

					if (!MyConfig.DocumentEmptyNamespaces)
					{
						xmlMemoryStream = new MemoryStream();
						tempWriter = new XmlTextWriter(xmlMemoryStream, System.Text.Encoding.UTF8);
					}

					tempWriter.WriteStartElement("namespace");
					tempWriter.WriteAttributeString("name", ourNamespaceName);

					if (namespaceSummary != null && namespaceSummary.Length > 0)
					{
						WriteStartDocumentation(tempWriter);
						tempWriter.WriteStartElement("summary");
						tempWriter.WriteRaw(namespaceSummary);
						tempWriter.WriteEndElement();
						WriteEndDocumentation(tempWriter);
					}
					else if (MyConfig.ShowMissingSummaries)
					{
						WriteStartDocumentation(tempWriter);
						WriteMissingDocumentation(tempWriter, "summary", null, "Missing <summary> Documentation for " + namespaceName);
						WriteEndDocumentation(tempWriter);
					}

					int nbClasses = WriteClasses(tempWriter, types, namespaceName);
					Trace.WriteLine(string.Format("Wrote {0} classes.", nbClasses));

					int nbInterfaces = WriteInterfaces(tempWriter, types, namespaceName);
					Trace.WriteLine(string.Format("Wrote {0} interfaces.", nbInterfaces));

					int nbStructures = WriteStructures(tempWriter, types, namespaceName);
					Trace.WriteLine(string.Format("Wrote {0} structures.", nbStructures));

					int nbDelegates = WriteDelegates(tempWriter, types, namespaceName);
					Trace.WriteLine(string.Format("Wrote {0} delegates.", nbDelegates));

					int nbEnums = WriteEnumerations(tempWriter, types, namespaceName);
					Trace.WriteLine(string.Format("Wrote {0} enumerations.", nbEnums));

					tempWriter.WriteEndElement();

					if (!MyConfig.DocumentEmptyNamespaces)
					{
						tempWriter.Close();

						if (nbClasses == 0 && nbInterfaces == 0 && nbStructures == 0 &&
							nbDelegates == 0 && nbEnums == 0)
						{
							Trace.WriteLine(string.Format("Discarding namespace {0} because it does not contain any documented types.", namespaceName));
						}
						else
						{
							string rawXml = System.Text.Encoding.UTF8.GetString( xmlMemoryStream.ToArray() );
							writer.WriteRaw( rawXml.Substring(1) );
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
					WriteClass(writer, type);
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
					WriteClass(writer, type);
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
			return type.BaseType.FullName == "System.Delegate" ||
				type.BaseType.FullName == "System.MulticastDelegate";
		}

		private int GetMethodOverload(MethodInfo method, MethodInfo[] methods)
		{
			int count = 0;
			int overload = 0;

			foreach (MethodInfo m in methods)
			{
				if (m.Name == method.Name)
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
				if (p.Name == property.Name)
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
		private void WriteClass(XmlWriter writer, Type type)
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
			WriteBaseType(writer, type.BaseType);

			Debug.Assert(implementations == null);
			implementations = new ImplementsCollection();

			foreach(Type interfaceType in type.GetInterfaces())
			{
				if(MustDocumentType(interfaceType))
				{
					writer.WriteElementString("implements", interfaceType.Name);
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
			WriteFields(writer, type);
			WriteProperties(writer, type);
			WriteMethods(writer, type);
			WriteOperators(writer, type);
			WriteEvents(writer, type);

			implementations = null;

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
			//TODO: more special attributes here?
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
				WriteCustomAttribute(writer, attribute);
			}
		}

		private void WriteCustomAttribute(XmlWriter writer, Attribute attribute)
		{
			writer.WriteStartElement("attribute");
			writer.WriteAttributeString("name", attribute.GetType().FullName);

			BindingFlags bindingFlags =
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

			BindingFlags bindingFlags =
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

		private void WriteFields(XmlWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			foreach (FieldInfo field in type.GetFields(bindingFlags))
			{
				if (MustDocumentField(field) && !IsAlsoAnEvent(field))
				{
					WriteField(writer, field, type);
				}
			}
		}

		private void WriteProperties(XmlWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			#warning explicitly implemented properties are not returned here.
			PropertyInfo[] properties = type.GetProperties(bindingFlags);

			foreach (PropertyInfo property in properties)
			{
				MethodInfo getMethod = property.GetGetMethod(true);
				MethodInfo setMethod = property.GetSetMethod(true);

				bool hasGetter = (getMethod != null) && MustDocumentMethod(getMethod);
				bool hasSetter = (setMethod != null) && MustDocumentMethod(setMethod);

				if ((hasGetter || hasSetter) && !IsAlsoAnEvent(property))
				{
					WriteProperty(
						writer,
						property,
						property.DeclaringType.FullName != type.FullName,
						GetPropertyOverload(property, properties));
				}
			}
		}

		private void WriteMethods(XmlWriter writer, Type type)
		{
			BindingFlags bindingFlags =
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
					MustDocumentMethod(method))
				{
					WriteMethod(
						writer,
						method,
						method.DeclaringType.FullName != type.FullName,
						GetMethodOverload(method, methods));
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

			MethodInfo[] methods = type.GetMethods(bindingFlags);

			foreach (MethodInfo method in methods)
			{
				if (method.Name.StartsWith("op_") &&
					MustDocumentMethod(method))
				{
					WriteOperator(
						writer,
						method,
						GetMethodOverload(method, methods));
				}
			}
		}

		private void WriteEvents(XmlWriter writer, Type type)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			foreach (EventInfo eventInfo in type.GetEvents(bindingFlags))
			{
				MethodInfo addMethod = eventInfo.GetAddMethod(true);

				if (addMethod != null &&
					MustDocumentMethod(addMethod))
				{
					WriteEvent(writer, eventInfo);
				}
			}
		}

		private bool IsAlsoAnEvent(Type type, string fullName)
		{
			bool isEvent = false;

			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			foreach (EventInfo eventInfo in type.GetEvents(bindingFlags))
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

			Debug.Assert(implementations == null);

			WriteFields(writer, type);
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
			writer.WriteAttributeString("name", type.Name.Replace('+', '.'));
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic;

			MethodInfo[] methods = type.GetMethods(bindingFlags);

			foreach (MethodInfo method in methods)
			{
				if (method.Name == "Invoke")
				{
					writer.WriteAttributeString("returnType", method.ReturnType.FullName);

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

		private string GetEnumerationName(Type type)
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
			writer.WriteAttributeString("name", GetEnumerationName(type));
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetTypeAccessValue(type));

			WriteEnumerationDocumentation(writer, memberName);
			WriteCustomAttributes(writer, type);

			BindingFlags bindingFlags =
				BindingFlags.Instance |
				BindingFlags.Static |
				BindingFlags.Public |
				BindingFlags.NonPublic |
				BindingFlags.DeclaredOnly;

			foreach (FieldInfo field in type.GetFields(bindingFlags))
			{
				// *** Not sure what this field is but don't want to document it for now.
				if (field.Name != "value__")
				{
					WriteField(writer, field, type);
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
		private void WriteField(XmlWriter writer, FieldInfo field,
			Type type)
		{
			string memberName = GetMemberName(field);

			writer.WriteStartElement("field");
			writer.WriteAttributeString("name", field.Name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetFieldAccessValue(field));
			writer.WriteAttributeString("type", field.FieldType.FullName.Replace('+', '.'));

			bool inherited = field.DeclaringType != field.ReflectedType;

			if (inherited)
			{
				writer.WriteAttributeString("declaringType", field.DeclaringType.FullName);
			}

			if (field.IsStatic)
			{
				writer.WriteAttributeString("contract", "Static");
			}

			if (field.IsLiteral)
			{
				writer.WriteAttributeString("literal", "true");
			}

			if (field.IsInitOnly)
			{
				writer.WriteAttributeString("initOnly", "true");
			}

			WriteFieldDocumentation(writer, memberName, !inherited, type);
			WriteCustomAttributes(writer, field);

			writer.WriteEndElement();
		}

		/// <summary>Writes XML documenting an event.</summary>
		/// <param name="writer">XmlWriter to write on.</param>
		/// <param name="eventInfo">Event to document.</param>
		private void WriteEvent(XmlWriter writer, EventInfo eventInfo)
		{
			string memberName = GetMemberName(eventInfo);

			writer.WriteStartElement("event");
			writer.WriteAttributeString("name", eventInfo.Name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetMethodAccessValue(eventInfo.GetAddMethod(true)));
			writer.WriteAttributeString("contract", GetMethodContractValue(eventInfo.GetAddMethod(true)));
			writer.WriteAttributeString("type", eventInfo.EventHandlerType.FullName.Replace('+', '.'));

			bool inherited = eventInfo.DeclaringType != eventInfo.ReflectedType;

			if (inherited)
			{
				writer.WriteAttributeString("declaringType", eventInfo.DeclaringType.FullName);
			}

			if (eventInfo.IsMulticast)
			{
				writer.WriteAttributeString("multicast", "true");
			}

			if (implementations != null)
			{
				ImplementsInfo implements = null;
				MethodInfo adder = eventInfo.GetAddMethod();
				if (adder != null)
				{
					implements = implementations[adder.ToString()];
				}
				if (implements == null)
				{
					MethodInfo remover = eventInfo.GetRemoveMethod();
					if (remover != null)
					{
						implements = implementations[remover.ToString()];
					}
				}
				if (implements != null)
				{
					writer.WriteStartElement("implements");
					writer.WriteAttributeString("name", eventInfo.Name);
					writer.WriteAttributeString("interface", implements.InterfaceType.Name);
					writer.WriteAttributeString("interfaceId", GetMemberName(implements.InterfaceType));
					writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName);
					writer.WriteEndElement();
				}
			}

			WriteEventDocumentation(writer, memberName, !inherited);
			WriteCustomAttributes(writer, eventInfo);

			writer.WriteEndElement();
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

			if (overload > 0)
			{
				writer.WriteAttributeString("overload", overload.ToString());
			}

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
		private void WriteProperty(XmlWriter writer, PropertyInfo property, bool inherited, int overload)
		{
			string memberName = GetMemberName(property);

			writer.WriteStartElement("property");
			writer.WriteAttributeString("name", property.Name);
			writer.WriteAttributeString("id", memberName);
			writer.WriteAttributeString("access", GetPropertyAccessValue(property));

			if (inherited)
			{
				writer.WriteAttributeString("declaringType", property.DeclaringType.FullName);
			}

			writer.WriteAttributeString("type", property.PropertyType.FullName.Replace('+', '.'));
			writer.WriteAttributeString("contract", GetPropertyContractValue(property));
			writer.WriteAttributeString("get", property.GetGetMethod(true) != null ? "true" : "false");
			writer.WriteAttributeString("set", property.GetSetMethod(true) != null ? "true" : "false");

			if (overload > 0)
			{
				writer.WriteAttributeString("overload", overload.ToString());
			}

			if (implementations != null)
			{
				ImplementsInfo implements = null;
				MethodInfo getter = property.GetGetMethod();
				if (getter != null)
				{
					implements = implementations[getter.ToString()];
				}
				if (implements == null)
				{
					MethodInfo setter = property.GetSetMethod();
					if (setter != null)
					{
						implements = implementations[setter.ToString()];
					}
				}
				if (implements != null)
				{
					writer.WriteStartElement("implements");
					writer.WriteAttributeString("name", property.Name);
					writer.WriteAttributeString("interface", implements.InterfaceType.Name);
					writer.WriteAttributeString("interfaceId", GetMemberName(implements.InterfaceType));
					writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName);
					writer.WriteEndElement();
				}
			}

			WritePropertyDocumentation(writer, memberName, property, !inherited);
			WriteCustomAttributes(writer, property);

			foreach (ParameterInfo parameter in GetIndexParameters(property))
			{
				WriteParameter(writer, memberName, parameter);
			}

			writer.WriteEndElement();
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
		private void WriteMethod(
			XmlWriter writer,
			MethodInfo method,
			bool inherited,
			int overload)
		{
			if (method != null)
			{
				string memberName = GetMemberName(method);

				string name = method.Name;
				string interfaceName = null;

				int lastIndexOfDot = name.LastIndexOf('.');
				if (lastIndexOfDot != -1)
				{
					interfaceName = name.Substring(0, lastIndexOfDot);
					lastIndexOfDot = interfaceName.LastIndexOf('.');
					if (lastIndexOfDot != -1)
						name = name.Substring(lastIndexOfDot + 1);
				}

				writer.WriteStartElement("method");
				writer.WriteAttributeString("name", name);
				writer.WriteAttributeString("id", memberName);
				writer.WriteAttributeString("access", GetMethodAccessValue(method));

				if (interfaceName != null)
				{
					writer.WriteAttributeString("interface", interfaceName);
				}

				if (inherited)
				{
					writer.WriteAttributeString("declaringType", method.DeclaringType.FullName);
				}

				writer.WriteAttributeString("contract", GetMethodContractValue(method));

				if (overload > 0)
				{
					writer.WriteAttributeString("overload", overload.ToString());
				}

				writer.WriteAttributeString("returnType", GetTypeName(method.ReturnType));

				if (implementations != null)
				{
					ImplementsInfo implements = implementations[method.ToString()];
					if (implements != null)
					{
						writer.WriteStartElement("implements");
						writer.WriteAttributeString("name", implements.InterfaceMethod.Name);
						writer.WriteAttributeString("interface", implements.InterfaceType.Name);
						writer.WriteAttributeString("interfaceId", GetMemberName(implements.InterfaceType));
						writer.WriteAttributeString("declaringType", implements.InterfaceType.FullName);
						writer.WriteEndElement();
					}
				}

				WriteMethodDocumentation(writer, memberName, method, !inherited);
				WriteCustomAttributes(writer, method);

				foreach (ParameterInfo parameter in method.GetParameters())
				{
					WriteParameter(writer, GetMemberName(method), parameter);
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
			writer.WriteAttributeString("type", GetTypeName(parameter.ParameterType));

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

				bool inherited = method.DeclaringType != method.ReflectedType;

				if (inherited)
				{
					writer.WriteAttributeString("declaringType", method.DeclaringType.FullName);
				}

				writer.WriteAttributeString("contract", GetMethodContractValue(method));

				if (overload > 0)
				{
					writer.WriteAttributeString("overload", overload.ToString());
				}

				writer.WriteAttributeString("returnType", method.ReturnType.FullName);

				WriteMethodDocumentation(writer, memberName, method, !inherited);

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
			return type.FullName.Replace('+', '.').Replace("&", null);
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
			return "E:" + GetFullNamespaceName(eventInfo) + "." + eventInfo.Name;
		}

		/// <summary>Derives the member name ID for a property.  Used to match nodes in the /doc XML.</summary>
		/// <param name="property">The property to derive the member name ID from.</param>
		private string GetMemberName(PropertyInfo property)
		{
			string memberName;

			memberName = "P:" + GetFullNamespaceName(property) + "." + property.Name;

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
				method.Name.Replace('.', '#');

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

				parameterName = Regex.Replace(parameterName, ",", ",0:");
				parameterName = Regex.Replace(parameterName, @"\[,", "[0:,");

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
			string xPathExpr = "/doc/members/member[@name=\"" + memberName + "\"]";
			XmlNode xmlNode = currentSlashDoc.SelectSingleNode(xPathExpr);

			if (xmlNode != null && xmlNode.HasChildNodes)
			{
				WriteStartDocumentation(writer);
				writer.WriteRaw(xmlNode.InnerXml);
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
					result = "NestedFamilyOrAssembly";
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
					result = "FamilyOrAssembly";
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
			string result = "Unknown";

			switch (method.Attributes & MethodAttributes.MemberAccessMask)
			{
				case MethodAttributes.Public:
					result = "Public";
					break;
				case MethodAttributes.Family:
					result = "Family";
					break;
				case MethodAttributes.FamORAssem:
					result = "FamilyOrAssembly";
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
					/*
					  if (false)
					  {
					  // This is where we need to check if the class we
					  // derive from has a method with our same sig. If
					  // so then this would be the 'new' keyword.
					  result = "new";
					  }
					  else
					  {
					*/
					result = "Virtual";
					//   }
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
			if (MyConfig.ShowMissingSummaries || MyConfig.ShowMissingRemarks)
			{
				string xPathExpr = "/doc/members/member[@name=\"" + memberName + "\"]";
				XmlNode xmlNode = currentSlashDoc.SelectSingleNode(xPathExpr);

				if (MyConfig.ShowMissingSummaries)
				{
					XmlNode summary;
					if (xmlNode == null
						|| (summary = xmlNode.SelectSingleNode("summary")) == null
						|| summary.InnerText.Length == 0
						|| summary.InnerText.Trim().StartsWith("Summary description for"))
					{
						WriteMissingDocumentation(writer, "summary", null,
							"Missing <summary> documentation for " + memberName);
					}
				}

				if (MyConfig.ShowMissingRemarks)
				{
					XmlNode remarks;
					if (xmlNode == null
						|| (remarks = xmlNode.SelectSingleNode("remarks")) == null
						|| remarks.InnerText.Length == 0)
					{
						WriteMissingDocumentation(writer, "remarks", null,
							"Missing <remarks> documentation for " + memberName);
					}
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
				foreach (ParameterInfo parameter in parameters)
				{
					string xpath = String.Format(
						"/doc/members/member[@name='{0}']/param[@name='{1}']",
						memberName,
						parameter.Name);

					XmlNode param;
					if ((param = currentSlashDoc.SelectSingleNode(xpath)) == null
						|| param.InnerText.Length == 0)
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
				string xpath = String.Format(
					"/doc/members/member[@name='{0}']/returns",
					memberName);

				XmlNode returns;
				if ((returns = currentSlashDoc.SelectSingleNode(xpath)) == null
					|| returns.InnerText.Length == 0)
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
				string xpath = String.Format(
					"/doc/members/member[@name='{0}']/value",
					memberName);

				XmlNode valuenode;
				if ((valuenode = currentSlashDoc.SelectSingleNode(xpath)) == null
					|| valuenode.InnerText.Length == 0)
				{
					WriteMissingDocumentation(writer, "value", null,
						"Missing <value> documentation for " + memberName);
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
					string xPathExpr = "/doc/members/member[@name=\"" + memberName + "\"]";
					XmlNode xmlNode = currentSlashDoc.SelectSingleNode(xPathExpr);

					XmlNode summary;
					if (xmlNode == null
						|| (summary = xmlNode.SelectSingleNode("summary")) == null
						|| summary.InnerText.Length == 0)
					{
						WriteStartDocumentation(writer);
						writer.WriteElementString("summary", "Initializes a new instance of the " 
							+ constructor.DeclaringType.Name + " class.");
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
			bool writeMissing,
			Type type)
		{
			if (writeMissing)
			{
				CheckForMissingSummaryAndRemarks(writer, memberName);
			}
			CheckForPropertyBacker(writer, memberName, type);
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
		/// This takes advantage of the fact that most people
		/// have a simple convention for the names of the fields
		/// and the properties that they back.
		/// If the field doesn't have a summary already, and it
		/// looks like it backs a property, and the BaseDocumenterConfig
		/// property is set appropriately, then this adds a
		/// summary indicating that.
		/// </summary>
		/// <remarks>
		/// Note that this design will call multiple fields the backer
		/// for a single property.
		/// <para/>This also will call a public field a backer for a
		/// property, when typically that wouldn't be the case.
		/// </remarks>
		/// <param name="writer">The XmlWriter to write to.</param>
		/// <param name="memberName">The full name of the field.</param>
		/// <param name="type">The Type which contains the field
		/// and potentially the property.</param>
		private void CheckForPropertyBacker(
			XmlWriter writer,
			string memberName,
			Type type)
		{
			if (!MyConfig.AutoPropertyBackerSummaries) return;

			string xPathExpr = "/doc/members/member[@name=\"" + memberName + "\"]";
			XmlNode xmlNode = currentSlashDoc.SelectSingleNode(xPathExpr);

			// determine if field is non-public
			// (because public fields are probably not backers for properties)
			bool isNonPublic = true;  // stubbed out for now

			// only do this if there is no summary already
			XmlNode summary;
			if ((xmlNode == null
					|| (summary = xmlNode.SelectSingleNode("summary")) == null
					|| summary.InnerText.Length == 0
					|| summary.InnerText.Trim().StartsWith("Summary description for"))
				&& isNonPublic)
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
				}
			}
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
			BindingFlags bindingFlags =
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
			string memberName = GetMemberName(property);

			WriteStartDocumentation(writer);
			writer.WriteStartElement(element);
			writer.WriteRaw("Backer for property <see cref=\"" 
				+ propertyId + "\">" + memberName + "</see>");
			writer.WriteEndElement();
		}


		/// <summary>Loads an assembly.</summary>
		/// <param name="filename">The assembly filename.</param>
		/// <returns>The assembly object.</returns>
		/// <remarks>This method loads an assembly into memory. If you
		/// use Assembly.Load or Assembly.LoadFrom the assembly file locks.
		/// This method doesn't lock the assembly file.</remarks>
		public static Assembly LoadAssembly(string filename)
		{
			if (!File.Exists(filename))
			{
				throw new ApplicationException("can't find assembly " + filename);
			}

			FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read);
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, (int)fs.Length);
			fs.Close();

			return Assembly.Load(buffer);
		}
	}
}
