using System;
using System.Collections.Specialized;

namespace NDoc.Documenter.Msdn
{
	/// <summary>
	/// Summary description for MsdnXsltUtilities.
	/// </summary>
	public class MsdnXsltUtilities
	{
		const string baseURL = "ms-help://MS.NETFrameworkSDK/cpref/html/frlrf";
		const string systemPrefix = "System.";

		/// <summary>
		/// Initializes a new instance of class MsdnXsltUtilities
		/// </summary>
		/// <param name="fileNames">A StringDictionary holding id to file name mappings.</param>
		/// <param name="elemNames">A StringDictionary holding id to element name mappings</param>
		public MsdnXsltUtilities(StringDictionary fileNames, StringDictionary elemNames)
		{
			_fileNames = fileNames;
			_elemNames = elemNames;
		}

		/// <summary>
		/// Returns an HRef for a CRef
		/// </summary>
		/// <param name="cref">CRef for which the HRef will be looked up.</param>
		/// <returns></returns>
		public string GetHRef(string cref)
		{
			if (cref.Substring(2, 7) != systemPrefix)
			{
				string fileName = _fileNames[cref];
				if ((fileName == null) && cref.StartsWith("F:"))
					fileName = _fileNames["E:" + cref.Substring(2)];

				if (fileName == null)
					return "";
				else
					return fileName;
			}
			else
			{
				switch (cref.Substring(0, 2))
				{
					case "N:":	// Namespace
						return baseURL + cref.Substring(2).Replace(".", "") + ".htm";
					case "T:":	// Type: class, interface, struct, enum, delegate
						return baseURL + cref.Substring(2).Replace(".", "") + "ClassTopic.htm";
					case "F:":	// Field
					case "P:":	// Property
					case "M:":	// Method
					case "E:":	// Event
						return GetFilenameForSystemMember(cref);
					default:
						return "";
				}
			}
		}
		/// <summary>
		/// Returns a name for a CRef
		/// </summary>
		/// <param name="cref">CRef for which the name will be looked up.</param>
		/// <returns></returns>
		public string GetName(string cref)
		{
			if (cref.Substring(2, 7) != systemPrefix)
			{
				string name = _elemNames[cref];
				if (name != null)
					return name;
			}

			int index;
			if ((index = cref.IndexOf(".#c")) >= 0)
				cref = cref.Substring(2, index - 2);
			else if ((index = cref.IndexOf("(")) >= 0)
				cref = cref.Substring(2, index - 2);
			else
				cref = cref.Substring(2);

			return cref.Substring(cref.LastIndexOf(".") + 1);
		}

		private string GetFilenameForSystemMember(string id)
		{
			string crefName;
			int index;
			if ((index = id.IndexOf(".#c")) >= 0)
				crefName = id.Substring(2, index - 2) + ".ctor";
			else if ((index = id.IndexOf("(")) >= 0)
				crefName = id.Substring(2, index - 2);
			else
				crefName = id.Substring(2);
			index = crefName.LastIndexOf(".");
			string crefType = crefName.Substring(0, index);
			string crefMember = crefName.Substring(index + 1);
			return baseURL + crefType.Replace(".", "") + "Class" + crefMember + "Topic.htm";
		}

		private StringDictionary _fileNames;
		private StringDictionary _elemNames;

		public bool HasSimilarOverloads(string description)
		{
			if (_descriptions.Contains(description))
				return true;
			_descriptions.Add(description);
			return false;
		}

		private StringCollection _descriptions = new StringCollection();
	}
}
