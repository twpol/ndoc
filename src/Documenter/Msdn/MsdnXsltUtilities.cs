using System;
using System.Collections.Specialized;
using System.Globalization;

namespace NDoc3.Documenter.Msdn
{
	/// <summary>
	/// Provides an extension object for the xslt transformations.
	/// </summary>
	public class MsdnXsltUtilities
	{
		private const string helpURL = "ms-help://";
		private const string sdkRoot = "/cpref/html/frlrf";
		private const string sdkDocPageExt = ".htm";
		//private const string msdnOnlineSdkBaseUrl = "http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/frlrf";
        private const string msdnOnlineSdkBaseUrl = "http://msdn.microsoft.com/{0}/library/{1}({2}).aspx";
		private const string msdnOnlineSdkPageExt = ".aspx";
		private const string systemPrefix = "System.";
		private string sdkDocBaseUrl; 
        private string sdkVersion;
		private string sdkDocExt;
        private string sdkDocLanguage;
//		private readonly StringDictionary fileNames;
//		private readonly StringDictionary elemNames;
		private string assemblyName;
		private NameResolver nameResolver;
		private StringCollection descriptions;
		private string encodingString;

		/// <summary>
		/// Initializes a new instance of class MsdnXsltUtilities
		/// </summary>
		/// <param name="nameResolver">TODO</param>
		/// <param name="assemblyName">TODO</param>
		/// <param name="linkToSdkDocVersion">Specifies the version of the SDK documentation.</param>
		/// <param name="linkToSdkDocLangauge">Specifies the version of the SDK documentation.</param>
		/// <param name="SdkLinksOnWeb">Specifies if links should be to ms online documentation.</param>
		/// <param name="fileEncoding">Specifies if links should be to ms online documentation.</param>
		public MsdnXsltUtilities(
			NameResolver nameResolver,
			string assemblyName,
			string linkToSdkDocVersion,
			string linkToSdkDocLangauge,
			bool SdkLinksOnWeb,
			System.Text.Encoding fileEncoding)
		{
			Reset();

			this.nameResolver = nameResolver;
			this.assemblyName = assemblyName;
//			this.fileNames = nameResolver.fileNames;
//			this.elemNames = nameResolver.elemNames;
            this.sdkDocLanguage = linkToSdkDocLangauge;
            this.sdkVersion = linkToSdkDocVersion;

            if (SdkLinksOnWeb)
            {
                sdkDocBaseUrl = msdnOnlineSdkBaseUrl;
                sdkDocExt = msdnOnlineSdkPageExt;
            }
            /*else
            {
                switch (linkToSdkDocVersion)
                {
                    case SdkVersion.SDK_v1_0:
                        sdkDocBaseUrl = GetLocalizedFrameworkURL(sdkDoc10BaseNamespace, linkToSdkDocLangauge);
                        sdkDocExt = sdkDocPageExt;
                        break;
                    case SdkVersion.SDK_v1_1:
                        sdkDocBaseUrl = GetLocalizedFrameworkURL(sdkDoc11BaseNamespace, linkToSdkDocLangauge);
                        sdkDocExt = sdkDocPageExt;
                        break;
                }
            }*/
			encodingString = "text/html; charset=" + fileEncoding.WebName; 
		}

		/// <summary>
		/// Reset Overload method checking state.
		/// </summary>
		public void Reset()
		{
			descriptions = new StringCollection();
		}

		/// <summary>
		/// Gets the base Url for system types links.
		/// </summary>
		public string SdkDocBaseUrl
		{
			get { return sdkDocBaseUrl; }
		}

        /// <summary>
        /// Gets the lamguage for system type links
        /// </summary>
        public string SdkDocLanguage
        {
            get { return sdkDocLanguage; }
        }

		/// <summary>
		/// Gets the page file extension for system types links.
		/// </summary>
		public string SdkDocExt
		{
			get { return sdkDocExt; }
		}

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
				string fileName = nameResolver.GetFilenameForId(assemblyName, cref);
				if ((fileName == null) && cref.StartsWith("F:"))
					fileName = nameResolver.GetFilenameForId(assemblyName,"E:" + cref.Substring(2));

				if (fileName == null)
					return "";
				else
					return fileName;
			}
			else
			{
                return String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2), sdkVersion);
				/*switch (cref.Substring(0, 2))
				{
					case "N:":	// Namespace
						//return sdkDocBaseUrl + cref.Substring(2).Replace(".", "") + sdkDocExt;
                        return String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2));
					case "T:":	// Type: class, interface, struct, enum, delegate
						// pointer types link to the type being pointed to
                        return String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2));
						//return sdkDocBaseUrl + cref.Substring(2).Replace(".", "").Replace( "*", "" ) + "ClassTopic" + sdkDocExt;
					case "F:":	// Field
                        return String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2));
					case "P:":	// Property
                        return String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2));
					case "M:":	// Method
                        return String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2));
					case "E:":	// Event
                        //return GetFilenameForSystemMember(cref);
                        return String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2));
					default:
						return string.Empty;
				}*/
			}
		}

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
					string name = nameResolver.GetDisplayNameForId(assemblyName, cref);
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
            return String.Format(sdkDocBaseUrl, sdkDocLanguage, id);
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

		/// <summary>
		/// Exposes <see cref="String.Replace(string, string)"/> to XSLT
		/// </summary>
		/// <param name="str">The string to search</param>
		/// <param name="oldValue">The string to search for</param>
		/// <param name="newValue">The string to replace</param>
		/// <returns>A new string</returns>
		public string Replace( string str, string oldValue, string newValue )
		{
			return str.Replace( oldValue, newValue );
		}

        /// <summary>
        /// Exposes easy formatting of SDK links to XSLT
        /// </summary>
        /// <param name="typename">Name of the type to refer to</param>
        /// <returns>URL to the type</returns>
        public string FormatOnlineSDKLink(string typename)
        {
        	//Remove array brackets and unsafe pointers from the link
			typename = typename.Replace("[", "").Replace("]", "").Replace("*", "");
            return String.Format(sdkDocBaseUrl, sdkDocLanguage, typename, sdkVersion);
        }

		/// <summary>
		/// returns a localized sdk url if one exists for the <see cref="CultureInfo.CurrentCulture"/>.
		/// </summary>
		/// <param name="searchNamespace">base namespace to search for</param>
		/// <param name="langCode">the localization language code</param>
		/// <returns>ms-help url for sdk</returns>
		private string GetLocalizedFrameworkURL(string searchNamespace, string langCode)
		{
			if (langCode!="en")
			{
				return helpURL + searchNamespace + "." + langCode.ToUpper() + sdkRoot;
			}
			else
			{
				//default to non-localized namespace
				return helpURL + searchNamespace + sdkRoot;
			}
		}

		/// <summary>
		/// Gets HTML ContentType for the system's current ANSI code page. 
		/// </summary>
		/// <returns>ContentType attribute string</returns>
		public string GetContentType()
		{
			return encodingString;
		}

		public string GetFilenameForId(string id)
		{
			return nameResolver.GetFilenameForId(assemblyName, id);
		}
		public string GetFilenameForIdHierarchy(string id)
		{
			return nameResolver.GetFilenameForIdHierarchy(assemblyName, id);
		}
		public string GetFilenameForNamespace(string namespaceName)
		{
			return nameResolver.GetFilenameForNamespace(assemblyName, namespaceName);
		}
		public string GetFilenameForNamespaceHierarchy(string namespaceName)
		{
			return nameResolver.GetFilenameForNamespaceHierarchy(assemblyName, namespaceName);
		}
		public string GetFilenameForConstructors(string typeId)
		{
			return nameResolver.GetFilenameForConstructors(assemblyName, typeId);
		}
		public string GetFilenameForTypeMembers(string typeId)
		{
			return nameResolver.GetFilenameForTypeMembers(assemblyName, typeId);
		}
		public string GetFilenameForOperatorOverloads(string typeID, string operatorName)
		{
			return nameResolver.GetFilenameForOperatorOverloads(assemblyName, typeID, operatorName);
		}
		public string GetFilenameForPropertyOverloads(string typeID, string propertyName)
		{
			return nameResolver.GetFilenameForPropertyOverloads(assemblyName, typeID, propertyName);
		}
		public string GetFilenameForMethodOverloads(string typeID, string methodName)
		{
			return nameResolver.GetFilenameForMethodOverloads(assemblyName, typeID, methodName);
		}
		public string GetFilenameForTypename(string typeName)
		{
			return nameResolver.GetFilenameForTypename(assemblyName, typeName);
		}
		public string GetFilenameForCRefOverload(string cref, string overload)
		{
			return nameResolver.GetFilenameForCRefOverload(assemblyName, cref, overload);
		}
	}
}
