using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;
using System.Text;

namespace NDoc.Core
{
	/// <summary>
	/// Summary description for AssemblyXmlDocCache.
	/// </summary>
	public class AssemblyXmlDocCache
	{
		private Hashtable docs;
		private Hashtable nodocTags;

		/// <summary>
		/// Initializes a new instance of the AssemblyXmlDocCache class.
		/// </summary>
		public AssemblyXmlDocCache(string fileName)
		{
			Flush();
			XmlTextReader reader = new XmlTextReader(fileName);
			CacheDocs(reader);
		}

		/// <summary>
		/// Flushes the Cache.
		/// </summary>
		public void Flush()
		{
			docs = new Hashtable();
			nodocTags = new Hashtable();
		}

		/// <summary>
		/// Cache the xmld docs into a hashtable for fater access.
		/// </summary>
		/// <param name="reader">An XMLTextReader containg the docs the cache</param>
		private void CacheDocs(XmlTextReader reader)
		{
			object oMember = reader.NameTable.Add("member");
			reader.MoveToContent();

			while (reader.Read()) 
			{
				if (reader.NodeType == XmlNodeType.Element) 
				{
					if (reader.Name.Equals(oMember)) 
					{
						string ID = reader.GetAttribute("name");
						string doc = reader.ReadInnerXml().Trim();
						doc = TidyDoc(ID, doc);
						docs.Add(ID, doc);
					}      
				}
			}
		}

		/// <summary>
		/// tidy documentation.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="doc"></param>
		/// <returns></returns>
		private string TidyDoc(string id, string doc)
		{
			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml(String.Format("<root>{0}</root>", doc));
			FixupNodes(id, xmldoc.ChildNodes);
			return xmldoc.DocumentElement.InnerXml;
		}

		/// <summary>
		/// strip out redundant newlines and spaces from documatation
		/// </summary>
		/// <param name="id">member</param>
		/// <param name="nodes">list of nodes</param>
		private void FixupNodes(string id, XmlNodeList nodes)
		{
			foreach (XmlNode node in nodes)
			{
				if (node.NodeType == XmlNodeType.Element) 
				{
					if (node.Name == "ndoc") nodocTags.Add(id, null);
					
					if (node.Name == "code")
						FixupCodeTag(node);
					else
						FixupNodes(id, node.ChildNodes);
				}
				if (node.NodeType == XmlNodeType.Text)
				{
					node.Value = ((string)node.Value).Replace("\t", "    ").Replace("\n", " ").Replace("\r", " ").Replace("        ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");
				}
			}
		}

		/// <summary>
		/// Remove leading spaces from code tag contents.
		/// </summary>
		/// <param name="node">a code tag node</param>
		private void FixupCodeTag(XmlNode node)
		{
			String codeText = (string)node.InnerText;
			if (codeText.StartsWith("\r\n")) codeText = codeText.Substring(2);
			codeText = codeText.Replace("\r\n", "\n");
			codeText = codeText.Replace("\t", "    ");
			string[] codeLines = codeText.Split(new Char[] {'\r', '\n'});
			if (codeLines.Length > 0)
			{
				string firstLine = codeLines[0];
				int leadingChars = 0; //number of chars at start of firstline

				while (leadingChars < firstLine.Length && firstLine.Substring(leadingChars, 1) == " ")
					leadingChars++;

				for (int index = 0; index < codeLines.Length; index++)
				{
					if (leadingChars < codeLines[index].Length)
						codeLines[index] = codeLines[index].Substring(leadingChars);
					else
						codeLines[index] = String.Empty;
				}

				string newtext = String.Join(System.Environment.NewLine, codeLines);
				node.InnerText = newtext;
 			}

		}

		/// <summary>
		/// Gets Xml documentation for the given ID
		/// </summary>
		/// <param name="memberID">The ID of the item for which documentation is required</param>
		/// <returns>a string containg the Xml documentation</returns>
		public string GetDoc(string memberID)
		{
			return (string)docs[memberID];
		}

		/// <summary>
		/// Returns whether a member has a nodoc tag
		/// </summary>
		/// <param name="memberID">ID to check</param>
		/// <returns>true if the member has a nodoc tag, otherwise false</returns>
		public bool HasNodocTag(string memberID)
		{
			return nodocTags.ContainsKey(memberID);
		}
	}
}
