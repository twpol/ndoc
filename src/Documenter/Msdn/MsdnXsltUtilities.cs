using System;
using System.Collections.Specialized;

namespace NDoc.Documenter.Msdn
{
	/// <summary>
	/// Provides an extension object for the xslt transformations.
	/// </summary>
	public class MsdnXsltUtilities
	{
		private const string sdkDoc10BaseUrl = "ms-help://MS.NETFrameworkSDK/cpref/html/frlrf";
		private const string sdkDoc11BaseUrl = "ms-help://MS.NETFrameworkSDKv1.1/cpref/html/frlrf";
		private const string sdkDocPageExt = ".htm";
		private const string msdnOnlineSdkBaseUrl = "http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrf";
		private const string msdnOnlineSdkPageExt = ".asp";
		private const string systemPrefix = "System.";
		private string sdkDocBaseUrl; 
		private string sdkDocExt; 
		private StringDictionary fileNames;
		private StringDictionary elemNames;
		private StringCollection descriptions;

		/// <summary>
		/// Initializes a new instance of class MsdnXsltUtilities
		/// </summary>
		/// <param name="fileNames">A StringDictionary holding id to file name mappings.</param>
		/// <param name="elemNames">A StringDictionary holding id to element name mappings</param>
		/// <param name="linkToSdkDocVersion">Specifies the version of the SDK documentation.</param>
		public MsdnXsltUtilities(
			StringDictionary fileNames, 
			StringDictionary elemNames, 
			SdkDocVersion linkToSdkDocVersion)
		{
			descriptions = new StringCollection();

			this.fileNames = fileNames;
			this.elemNames = elemNames;
			
			switch (linkToSdkDocVersion)
			{
				case SdkDocVersion.SDK_v1_0:
					sdkDocBaseUrl = sdkDoc10BaseUrl;
					sdkDocExt = sdkDocPageExt;
					break;
				case SdkDocVersion.SDK_v1_1:
					sdkDocBaseUrl = sdkDoc11BaseUrl;
					sdkDocExt = sdkDocPageExt;
					break;
				case SdkDocVersion.MsdnOnline:
					sdkDocBaseUrl = msdnOnlineSdkBaseUrl;
					sdkDocExt = msdnOnlineSdkPageExt;
					break;
			}

		}

		/// <summary>
		/// Gets the base Url for system types links.
		/// </summary>
		public string SdkDocBaseUrl
		{
			get { return sdkDocBaseUrl; }
		}

		/// <summary>
		/// Gets the page file extension for system types links.
		/// </summary>
		public string SdkDocExt
		{
			get { return sdkDocExt; }
		}

#if MONO
		/// <summary>
		/// Returns an HRef for a CRef.
		/// </summary>
		/// <param name="list">The argument list containing the 
		/// cRef for which the HRef will be looked up.</param>
		/// <remarks>Mono needs this overload, as its XsltTransform can only
		/// call methods with an ArraList parameter.</remarks>
		public string GetHRef(System.Collections.ArrayList list)
		{
			string cref = (string)list[0];
			return GetHRef(cref);
		}
#endif

		/// <summary>
		/// Returns an HRef for a CRef.
		/// </summary>
		/// <param name="cref">CRef for which the HRef will be looked up.</param>
		public string GetHRef(string cref)
		{
			if ((cref.Length < 2) || (cref[1] != ':'))
				return string.Empty;

			if ((cref.Length < 9)
				|| (cref.Substring(2, 7) != systemPrefix))
			{
				string fileName = fileNames[cref];
				if ((fileName == null) && cref.StartsWith("F:"))
					fileName = fileNames["E:" + cref.Substring(2)];

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
						return sdkDocBaseUrl + cref.Substring(2).Replace(".", "") + sdkDocExt;
					case "T:":	// Type: class, interface, struct, enum, delegate
						return sdkDocBaseUrl + cref.Substring(2).Replace(".", "") + "ClassTopic" + sdkDocExt;
					case "F:":	// Field
					case "P:":	// Property
					case "M:":	// Method
					case "E:":	// Event
						return GetFilenameForSystemMember(cref);
					default:
						return string.Empty;
				}
			}
		}


#if MONO
		/// <summary>
		/// Returns a name for a CRef.
		/// </summary>
		/// <param name="list">The argument list containing the 
		/// cRef for which the HRef will be looked up.</param>
		/// <remarks>Mono needs this overload, as its XsltTransform can only
		/// call methods with an ArraList parameter.</remarks>
		public string GetName(System.Collections.ArrayList list)
		{
			string cref = (string)list[0];
			return GetName(cref);
		}
#endif

		/// <summary>
		/// Returns a name for a CRef.
		/// </summary>
		/// <param name="cref">CRef for which the name will be looked up.</param>
		public string GetName(string cref)
		{
			if (cref.Length < 2)
				return cref;

			if (cref[1] == ':')
			{
				if ((cref.Length < 9)
					|| (cref.Substring(2, 7) != systemPrefix))
				{
					string name = elemNames[cref];
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
			}

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
			return sdkDocBaseUrl + crefType.Replace(".", "") + "Class" + crefMember + "Topic" + sdkDocExt;
		}

		/// <summary>
		/// Looks up, whether a member has similar overloads, that have already been documented.
		/// </summary>
		/// <param name="description">A string describing this overload.</param>
		/// <returns>true, if there has been a member with the same description.</returns>
		/// <remarks>
		/// <para>On the members pages overloads are cumulated. Instead of adding all overloads
		/// to the members page, a link is added to the members page, that points
		/// to an overloads page.</para>
		/// <para>If for example one overload is public, while another one is protected,
		/// we want both to appear on the members page. This is to make the search
		/// for suitable members easier.</para>
		/// <para>This leads us to the similarity of overloads. Two overloads are considered
		/// similar, if they have the same name, declaring type, access (public, protected, ...)
		/// and contract (static, non static). The description contains these four attributes
		/// of the member. This means, that two members are similar, when they have the same
		/// description.</para>
		/// <para>Asking for the first time, if a member has similar overloads, will return false.
		/// After that, if asking with the same description again, it will return true, so
		/// the overload does not need to be added to the members page.</para>
		/// </remarks>
		public bool HasSimilarOverloads(string description)
		{
			if (descriptions.Contains(description))
				return true;
			descriptions.Add(description);
			return false;
		}
	}
}
