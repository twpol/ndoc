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
			docs      = new Hashtable();
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
						string ID  = reader.GetAttribute("name");
						string doc = reader.ReadInnerXml().Trim();
						doc = TidyDoc(doc);
						docs.Add(ID,doc);
						if (doc.IndexOf("<nodoc")>-1)
						{
							nodocTags.Add(ID,null);
						}
					}      
				}
			}
		}

		/// <summary>
		/// tidy documentation.
		/// </summary>
		/// <param name="doc"></param>
		/// <returns></returns>
		private string TidyDoc(string doc)
		{
			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml(String.Format("<root>{0}</root>", doc));
			FixupNodes(xmldoc.ChildNodes);
			return xmldoc.DocumentElement.InnerXml;
		}

		/// <summary>
		/// strip out redundant newlines and spaces from documatation
		/// </summary>
		/// <param name="nodes">list of nodes</param>
		private void FixupNodes(XmlNodeList nodes)
		{
			foreach(XmlNode node in nodes)
			{
				if (node.NodeType==XmlNodeType.Element) 
				{
					if (node.Name=="code")
						FixupCodeTag(node);
					else
						FixupNodes(node.ChildNodes);
				}
				if (node.NodeType==XmlNodeType.Text)
				{
					node.Value=((string)node.Value).Replace("\t","    ").Replace("\n"," ").Replace("\r"," ").Replace("        "," ").Replace("    "," ").Replace("   "," ").Replace("  "," ");
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
			if (codeText.StartsWith("\r\n")) codeText=codeText.Substring(2);
			codeText=codeText.Replace("\r\n","\n");
			codeText=codeText.Replace("\t","    ");
			string[] codeLines = codeText.Split(new Char[]{'\r','\n'});
			if (codeLines.Length>0)
			{
				string firstLine = codeLines[0];
				int i=0; //number of chars at start of firstline
				while (i<firstLine.Length && firstLine.Substring(i,1)==" ") i++;
				for(int index=0;index<codeLines.Length;index++)
				{
					codeLines[index]=codeLines[index].Substring(i);
				}
				string newtext = String.Join(System.Environment.NewLine, codeLines);
				node.InnerText=newtext;
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
