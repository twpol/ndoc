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
using System.Reflection;
using System.Xml;
using System.IO;
using System.Xml.XPath;

namespace NDoc3.Core.Reflection {
	/// <summary>
	/// AssemblyXmlDocCache.
	/// </summary>
	internal class AssemblyXmlDocCache {
		private class XmlDocKey {
			public readonly string AssemblyName;
			public readonly string MemberId;

			public XmlDocKey(string assemblyName, string memberId) {
				AssemblyName = assemblyName;
				MemberId = memberId;
			}

			public bool Equals(XmlDocKey other) {
				if (ReferenceEquals(null, other))
					return false;
				if (ReferenceEquals(this, other))
					return true;
				return Equals(other.AssemblyName, AssemblyName) && Equals(other.MemberId, MemberId);
			}

			public override bool Equals(object obj) {
				if (ReferenceEquals(null, obj))
					return false;
				if (ReferenceEquals(this, obj))
					return true;
				if (obj.GetType() != typeof(XmlDocKey))
					return false;
				return Equals((XmlDocKey)obj);
			}

			public override int GetHashCode() {
				unchecked {
					int result = (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
					result = (result * 397) ^ (MemberId != null ? MemberId.GetHashCode() : 0);
					return result;
				}
			}

			public override string ToString() {
				return string.Format("[{0}]{1}", AssemblyName, MemberId);
			}
		}

		private Hashtable docs;
		private Hashtable excludeTags;

		/// <summary>
		/// Creates a new instance of the <see cref="AssemblyXmlDocCache"/> class.
		/// </summary>
		public AssemblyXmlDocCache() {
			Flush();
		}

		/// <summary>
		/// Flushes the Cache.
		/// </summary>
		public void Flush() {
			docs = new Hashtable();
			excludeTags = new Hashtable();
		}


		/// <summary>
		/// Populates cache from the given file.
		/// </summary>
		/// <param name="fileName">Fully-qualified filename of xml file with which to populate the cache.</param>
		public void CacheDocFile(string fileName) {
			XmlTextReader reader = new XmlTextReader(fileName);
			reader.WhitespaceHandling = WhitespaceHandling.All;
			CacheDocs(reader);
		}


		/// <summary>
		/// Cache the xmld docs into a hashtable for faster access.
		/// </summary>
		/// <param name="reader">An XmlReader containg the docs the cache</param>
		private void CacheDocs(XmlReader reader) {
			XPathDocument xpathDoc = new XPathDocument(reader);
			XPathNavigator doc = xpathDoc.CreateNavigator();

			string assemblyName = doc.SelectSingleNode("/doc/assembly/name").Value;

			foreach (XPathNavigator node in doc.Select("/doc/members/member")) {
				string ID = node.GetAttribute("name", string.Empty);
				//Handles multidimensional arrays by replacing 0: from XML doc to the reflection type
				// TODO(EE): find out, if this still applies 
				//				ID = ID.Replace("0:", "");
				XmlDocKey key = new XmlDocKey(assemblyName, ID);

				string slashdoc = node.InnerXml.Trim();
				slashdoc = PreprocessDoc(key, slashdoc);

				if (docs.ContainsKey(key)) {
					Trace.WriteLine("Warning: Multiple <member> tags found with id=\"" + ID + "\"");
				} else {
					docs.Add(key, slashdoc);
				}
			}

			//            object oMember = reader.NameTable.Add("member");
			//			reader.MoveToContent();
			//
			//			while (!reader.EOF) {
			//				if ((reader.NodeType == XmlNodeType.Element) && (reader.Name.Equals("assembly"))) {
			//					
			//				}
			//
			//				if ((reader.NodeType == XmlNodeType.Element) && (reader.Name.Equals(oMember))) {
			//					//Handles multidimensional arrays by replacing 0: from XML doc to the reflection type
			//					string ID = reader.GetAttribute("name").Replace("0:", "");
			//
			//					string doc = reader.ReadInnerXml().Trim();
			//					doc = PreprocessDoc(ID, doc);
			//					if (docs.ContainsKey(ID)) {
			//						Trace.WriteLine("Warning: Multiple <member> tags found with id=\"" + ID + "\"");
			//					} else {
			//						docs.Add(ID, doc);
			//					}
			//				} else {
			//					reader.Read();
			//				}
			//			}
		}

		/// <summary>
		/// Preprocess documentation before placing it in the cache.
		/// </summary>
		/// <param name="key">Member name 'id' to which the docs belong</param>
		/// <param name="doc">A string containing the members documentation</param>
		/// <returns>processed doc string</returns>
		private string PreprocessDoc(XmlDocKey key, string doc) {
			//create an XmlDocument containg the memeber's documentation
			XmlTextReader reader = new XmlTextReader(new StringReader("<root>" + doc + "</root>"));
			reader.WhitespaceHandling = WhitespaceHandling.All;

			XmlDocument xmldoc = new XmlDocument();
			xmldoc.PreserveWhitespace = true;
			xmldoc.Load(reader);

			if (xmldoc.DocumentElement != null) {
				Trace.WriteLine(string.Format("TRACE: processing comment '{0}':\n{1}", key, doc));
				XmlNodeList textNodes = xmldoc.DocumentElement.SelectNodes("comment() | text() | processing-instruction()");

				if (textNodes != null) {
					bool isTextNodeOnly = xmldoc.DocumentElement.ChildNodes.Count == textNodes.Count;
					if (!isTextNodeOnly) {
						CleanupNodes(key, xmldoc.DocumentElement.ChildNodes);
						ProcessSeeLinks(key, xmldoc.DocumentElement.ChildNodes);
						return xmldoc.DocumentElement.InnerXml;
					}
				}
				Trace.WriteLine(string.Format("WARN: comment '{0}' is not well formed", key));
				return "<summary>" + xmldoc.DocumentElement.InnerText + "</summary>";
			}
			throw new Exception("DocumentElement is null");
		}

		/// <summary>
		/// strip out redundant newlines and spaces from documentation.
		/// </summary>
		/// <param name="key">member</param>
		/// <param name="nodes">list of nodes</param>
		private void CleanupNodes(XmlDocKey key, XmlNodeList nodes) {
			foreach (XmlNode node in nodes) {
				if (node.NodeType == XmlNodeType.Element) {
					if (node.Name == "exclude") {
						excludeTags.Add(key, null);
					}

					if (node.Name == "code") {
						FixupCodeTag(node);
					} else {
						CleanupNodes(key, node.ChildNodes);
					}

					// Trim attribute values...
					foreach (XmlNode attr in node.Attributes) {
						attr.Value = attr.Value.Trim();
					}
				}
				if (node.NodeType == XmlNodeType.Text) {
					node.Value = node.Value.Replace("\t", "    ").Replace("\n", " ").Replace("\r", " ").Replace("        ", " ").Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");
				}
			}
		}

		/// <summary>
		/// Remove leading spaces from code tag contents.
		/// </summary>
		/// <param name="node">a code tag node</param>
		private static void FixupCodeTag(XmlNode node) {
			string codeText = node.InnerXml;
			if (codeText.TrimStart(new[] { ' ' }).StartsWith("\r\n")) {
				codeText = codeText.TrimStart(new[] { ' ' }).Substring(2);
			}
			codeText = codeText.Replace("\r\n", "\n");
			codeText = codeText.Replace("\t", "    ");
			string[] codeLines = codeText.Split(new[] { '\r', '\n' });
			if (codeLines.Length > 0) {
				int numberOfCharsToRemove = int.MaxValue;
				for (int index = 0; index < codeLines.Length; index++) {
					string testLine = codeLines[index];
					int leadingWhitespaceChars = 0; //number of chars at start of line
					while (leadingWhitespaceChars < testLine.Length && testLine.Substring(leadingWhitespaceChars, 1) == " ") {
						leadingWhitespaceChars++;
					}
					if (numberOfCharsToRemove > leadingWhitespaceChars) {
						numberOfCharsToRemove = leadingWhitespaceChars;
					}
				}

				if (numberOfCharsToRemove < int.MaxValue && numberOfCharsToRemove > 0) {

					for (int index = 0; index < codeLines.Length; index++) {
						codeLines[index] = numberOfCharsToRemove < codeLines[index].Length ? codeLines[index].Substring(numberOfCharsToRemove)
							: codeLines[index].TrimStart();
					}
				}

				string newtext = String.Join(Environment.NewLine, codeLines);

				XmlAttribute escaped = node.Attributes["escaped"];
				if (escaped != null && escaped.Value == "true") {
					node.InnerText = newtext;
				} else {
					node.InnerXml = newtext;
				}
			}

		}

		/// <summary>
		/// Add 'nolink' attributes to self referencing or duplicate see tags.
		/// </summary>
		/// <param name="key">current member name 'id'</param>
		/// <param name="nodes">list of top-level nodes</param>
		/// <remarks>
		/// </remarks>
		private static void ProcessSeeLinks(XmlDocKey key, XmlNodeList nodes) {
			foreach (XmlNode node in nodes) {
				if (node.NodeType == XmlNodeType.Element) {
					Hashtable linkTable = null;
					MarkupSeeLinks(ref linkTable, key, node);
				}
			}
		}

		/// <summary>
		/// Search tags for duplicate or self-referencing see links.
		/// </summary>
		/// <param name="linkTable">A table of previous links.</param>
		/// <param name="key">current member name 'id'</param>
		/// <param name="node">an Xml Node containing a doc tag</param>
		private static void MarkupSeeLinks(ref Hashtable linkTable, XmlDocKey key, XmlNode node) {
			if (node.LocalName == "see") {
				//we will only do this for crefs
				XmlAttribute cref = node.Attributes["cref"];
				if (cref != null) {
					if (cref.Value == key.MemberId) //self referencing tag
					{
						XmlAttribute dup = node.OwnerDocument.CreateAttribute("nolink");
						dup.Value = "true";
						node.Attributes.Append(dup);
					} else {
						if (linkTable == null) {
							//assume an resonable initial table size,
							//so we don't have to resize often.
							linkTable = new Hashtable(16);
						}
						if (linkTable.ContainsKey(cref.Value)) {
							XmlAttribute dup = node.OwnerDocument.CreateAttribute("nolink");
							dup.Value = "true";
							node.Attributes.Append(dup);
						} else {
							linkTable.Add(cref.Value, null);
						}
					}
				}
			}

			//search this tags' children
			foreach (XmlNode childnode in node.ChildNodes) {
				if (childnode.NodeType == XmlNodeType.Element) {
					MarkupSeeLinks(ref linkTable, key, childnode);
				}
			}
		}


		/// <summary>
		/// Gets Xml documentation for the given ID
		/// </summary>
		/// <param name="assemblyName">the name of the assembly to lookup</param>
		/// <param name="memberId">The ID of the item for which documentation is required</param>
		/// <returns>a string containg the Xml documentation</returns>
		public string GetDoc(AssemblyName assemblyName, string memberId) {
			return (string)docs[new XmlDocKey(assemblyName.Name, memberId)];
		}

		/// <summary>
		/// Returns whether a member has an exclude tag
		/// </summary>
		/// <param name="type"></param>
		/// <param name="memberId">ID to check</param>
		/// <returns>true if the member has an exclude tag, otherwise false</returns>
		public bool HasExcludeTag(Type type, string memberId) {
			return excludeTags.ContainsKey(
				new XmlDocKey(type.Assembly.GetName().Name, memberId));
		}
	}
}
