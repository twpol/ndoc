using System;
using System.Collections;
using System.Diagnostics;
using System.Xml;

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
						string doc = reader.ReadInnerXml().Replace("\n","").Replace("\r","").Trim(); 
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
