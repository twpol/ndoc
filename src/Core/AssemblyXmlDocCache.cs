// Copyright (C) 2004  Kevin Downs
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
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.IO;

namespace NDoc.Core
{
	/// <summary>
	/// Summary description for AssemblyXmlDocCache.
	/// </summary>
	public class AssemblyXmlDocCache
	{
		private Hashtable docs;
		private Hashtable excludeTags;

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
			excludeTags = new Hashtable();
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
						if (docs.ContainsKey(ID))
						{
							Trace.WriteLine("Warning: Multiple <member> tags found with id=\"" + ID + "\"");
							docs[ID] += doc;
						}
						else
						{
							docs.Add(ID, doc);
						}
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
			xmldoc.Load(new XmlTextReader(new StringReader("<root>" + doc + "</root>")));
			FixupNodes(id, xmldoc.ChildNodes);
			return xmldoc.DocumentElement.InnerXml;
		}

		/// <summary>
		/// strip out redundant newlines and spaces from documentation
		/// </summary>
		/// <param name="id">member</param>
		/// <param name="nodes">list of nodes</param>
		private void FixupNodes(string id, XmlNodeList nodes)
		{
			foreach (XmlNode node in nodes)
			{
				if (node.NodeType == XmlNodeType.Element) 
				{
					if (node.Name == "exclude") excludeTags.Add(id, null);
					
					if (node.Name == "code")
						FixupCodeTag(node);
					else
						FixupNodes(id, node.ChildNodes);

					// Trim attribute values...
					foreach(XmlNode attr in node.Attributes)
					{
						attr.Value=attr.Value.Trim();
					}
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
			string codeText = (string)node.InnerText;
			if (codeText.TrimStart(new Char[] {' '}).StartsWith("\r\n"))
			{
				codeText = codeText.TrimStart(new Char[] {' '}).Substring(2);
			}
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
						codeLines[index] = codeLines[index].TrimStart();
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
		/// Returns whether a member has an exclude tag
		/// </summary>
		/// <param name="memberID">ID to check</param>
		/// <returns>true if the member has an exclude tag, otherwise false</returns>
		public bool HasExcludeTag(string memberID)
		{
			return excludeTags.ContainsKey(memberID);
		}
	}
}
