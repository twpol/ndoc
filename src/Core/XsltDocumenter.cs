using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms.Design;
using System.Xml;
using System.Xml.Xsl;

namespace NDoc.Core
{
	/// <summary>Summary description for XsltDocumenter.</summary>
	public class XsltDocumenter : BaseDocumenter
	{
		/// <summary>Initializes a new instance of the XsltDocumenter 
		/// class.</summary>
		public XsltDocumenter() : base("XSLT")
		{
			Config = new XsltDocumenterConfig();
		}

		/// <summary>Document me.</summary>
		public override void Clear()
		{
			_TransformExists.Clear();
			_Transforms.Clear();
		}

		/// <summary>Document me.</summary>
		public override void View()
		{
			Process.Start(
				Path.Combine(
					MyConfig.OutputDirectory, 
					GetFilenameForNamespaces()));
		}

		/// <summary>Document me.</summary>
		public override void Build(Project project)
		{
			Clear();

			MakeXml(project.AssemblySlashDocs, project.NamespaceSummaries);

			if (!Directory.Exists(MyConfig.OutputDirectory))
			{
				Directory.CreateDirectory(MyConfig.OutputDirectory);
			}

			TransformNamespaces();
		}

		private XsltDocumenterConfig MyConfig
		{
			get { return (XsltDocumenterConfig)Config; }
		}

		private XmlNode[] SortNodes(XmlNodeList nodeList, string attributeName)
		{
			XmlNode[] nodeArray = new XmlNode[nodeList.Count];
			
			int i = 0;
			
			foreach (XmlNode node in nodeList) 
			{
				nodeArray[i++] = node;
			}

			Array.Sort(nodeArray, new AttributeComparer(attributeName));

			return nodeArray;
		}

		private class AttributeComparer : IComparer
		{
			internal AttributeComparer(string attributeName)
			{
				_AttributeName = attributeName;
			}

			private string _AttributeName;
            
			public int Compare(object x, object y)
			{
				return String.Compare(
					((XmlNode)x).Attributes[_AttributeName].Value, 
					((XmlNode)y).Attributes[_AttributeName].Value);
			}
		}

		private string GetTransformPath(string transformName)
		{
			return Path.Combine(MyConfig.XsltDirectory, transformName + ".xslt");
		}

		private Hashtable _TransformExists = new Hashtable();

		private bool TransformExists(string transformName)
		{
			bool result = false;

			if (_TransformExists.Contains(transformName))
			{
				result = (bool)_TransformExists[transformName];
			}
			else
			{
				result = File.Exists(GetTransformPath(transformName));
				_TransformExists.Add(transformName, result);
			}

			return result;
		}

		private Hashtable _Transforms = new Hashtable();

		private XslTransform GetTransform(string transformName)
		{
			XslTransform transform = _Transforms[transformName] as XslTransform;

			if (transform == null)
			{
				transform = new XslTransform();
				transform.Load(GetTransformPath(transformName));
				_Transforms.Add(transformName, transform);
			}

			return transform;
		}

		private void Transform(
			string transformName, 
			string outputPath, 
			params object[] args)
		{
			if (TransformExists(transformName))
			{
				Trace.Write(transformName + ": ");

				XslTransform transform = GetTransform(transformName);
				XsltArgumentList xsltArgs = new XsltArgumentList();

				for (int i = 0; i < args.Length; i += 2)
				{
					Trace.Write(args[i + 1] + " ");

					xsltArgs.AddParam((string)args[i], String.Empty, args[i + 1]);
				}

				Trace.WriteLine("");

				MemoryStream memoryStream = new MemoryStream();
				transform.Transform(Document, xsltArgs, memoryStream);

                FileStream fileStream = null;
				
				try
				{
					fileStream = new FileStream(
						Path.Combine(MyConfig.OutputDirectory, outputPath), 
						FileMode.Create);
					memoryStream.WriteTo(fileStream);
				}
				finally
				{
					if (fileStream != null)
					{
						fileStream.Close();
					}
				}
			}
		}

		private void TransformNamespaces()
		{
			Transform(
				"namespaces",
				GetFilenameForNamespaces());

			XmlNodeList namespaceNodeList = 
				Document.SelectNodes("/ndoc/assembly/module/namespace");
			
			XmlNode[] namespaceNodes = SortNodes(namespaceNodeList, "name");
			
			string previousNamespaceName = null;

			foreach (XmlNode namespaceNode in namespaceNodes)
			{
				string currentNamespaceName = namespaceNode.Attributes["name"].Value;
				
				if (previousNamespaceName != currentNamespaceName)
				{
					TransformNamespace(currentNamespaceName);
				}

				previousNamespaceName = currentNamespaceName;
			}
		}

		private string GetFilenameForNamespaces()
		{
			return "index.html";
		}

		private string GetFilenameForNamespace(string namespaceName)
		{
			return namespaceName + ".html";
		}

		private void TransformNamespace(string namespaceName)
		{
			Transform(
				"namespace", 
				GetFilenameForNamespace(namespaceName), 
				"namespace-name", namespaceName);
			
			TransformClasses(namespaceName);
		}

		private void TransformClasses(string namespaceName)
		{
			XmlNodeList nodeList = Document.SelectNodes(
				String.Format(
					"/ndoc/assembly/module/namespace[@name='{0}']/class", 
					namespaceName));

			XmlNode[] nodes = SortNodes(nodeList, "name");

			foreach (XmlNode node in nodes)
			{
				TransformClass(node.Attributes["id"].Value);
			}
		}

		private string GetFileNameForClass(string classID)
		{
			return classID.Substring(2) + ".html";
		}

		private string GetFileNameForClassMembers(string classID)
		{
			return classID.Substring(2) + "Members.html";
		}

		private string GetFileNameForConstructors(string classID)
		{
			return classID.Substring(2) + "Constructors.html";
		}

		private string GetFileNameForConstructor(string classID, XmlNode node)
		{
			string overload = "";

			if (node.Attributes["overload"] != null)
			{
				overload = node.Attributes["overload"].Value;
			}

			return classID.Substring(2) + "Constructor" + overload + ".html";
		}

		private void TransformClass(string classID)
		{
			Transform(
				"class", 
				GetFileNameForClass(classID), 
				"class-id", classID);

			TransformClassMembers(classID);
		}

		private void TransformClassMembers(string classID)
		{
			Transform(
				"class-members", 
				GetFileNameForClassMembers(classID), 
				"class-id", classID);

			TransformClassConstructors(classID);
			TransformClassFields(classID);
			TransformClassProperties(classID);
			TransformClassMethods(classID);
			TransformClassOperators(classID);
			TransformClassEvents(classID);
		}

		private void TransformClassConstructors(string classID)
		{
			//Trace.WriteLine("class-constructors: " + classID);

			XmlNodeList nodeList = Document.SelectNodes(
				String.Format(
					"/ndoc/assembly/module/namespace/class[@id='{0}']/constructor", 
					classID));

			XmlNode[] nodes = SortNodes(nodeList, "name");

			if (nodes.Length > 1)
			{
				Transform(
					"constructors", 
					GetFileNameForConstructors(classID), 
					"class-id", classID);
			}

			foreach (XmlNode node in nodes)
			{
				Transform(
					"constructor",
					GetFileNameForConstructor(classID, node),
					"class-id", classID,
					"constructor-id", node.Attributes["id"].Value);
			}
		}

		private void TransformClassFields(string classID)
		{
			//Trace.WriteLine("class-fields: " + classID);
		}
		
		private void TransformClassProperties(string classID)
		{
			//Trace.WriteLine("class-properties: " + classID);
		}
		
		private void TransformClassMethods(string classID)
		{
			//Trace.WriteLine("class-methods: " + classID);
		}
		
		private void TransformClassOperators(string classID)
		{
			//Trace.WriteLine("class-operators: " + classID);
		}
		
		private void TransformClassEvents(string classID)
		{
			//Trace.WriteLine("class-events: " + classID);
		}
	}

	/// <summary>Document me.</summary>
	public class XsltDocumenterConfig : BaseDocumenterConfig
	{
		/// <summary>Document me.</summary>
		public XsltDocumenterConfig() : base("XSLT")
		{
		}

		private string _XsltDirectory;

		/// <summary>Document me.</summary>
		[
			Category("Input"),
			Description("The directory in which XSLT transform files are located."),
			Editor(typeof(FolderNameEditor), typeof(UITypeEditor))
		]
		public string XsltDirectory
		{
			get { return _XsltDirectory; }
			set { _XsltDirectory = value; }
		}

		private string _OutputDirectory;

		/// <summary>Document me.</summary>
		[
			Category("Output"),
			Description("The directory in which result files will be generated."),
			Editor(typeof(FolderNameEditor), typeof(UITypeEditor))
		]
		public string OutputDirectory
		{
			get { return _OutputDirectory; }
			set { _OutputDirectory = value; }
		}
	}
}
