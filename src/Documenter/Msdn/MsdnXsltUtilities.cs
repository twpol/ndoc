using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Xml.XPath;
using System.Xml.Xsl;
using NDoc3.Support;

namespace NDoc3.Documenter.Msdn {
	/// <summary>
	/// Provides an extension object for the xslt transformations.
	/// </summary>
	public class MsdnXsltUtilities {
		/// <summary>
		/// an Xslt function extension that dynamically evaluates xpath expressions
		/// </summary>
		private class FuncEvaluate : IXsltContextFunction {
			public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext) {
				XPathNavigator nav = (XPathNavigator)args[0];
				string expr = nav.Value;
				XPathExpression xpath = docContext.Compile(expr);
				xpath.SetContext(xsltContext);
				object result = docContext.Evaluate(xpath);
				return result;
			}

			public int Minargs {
				get { return 1; }
			}

			public int Maxargs {
				get { return 1; }
			}

			public XPathResultType ReturnType {
				get { return XPathResultType.NodeSet; }
			}

			public XPathResultType[] ArgTypes {
				get { return new[] { XPathResultType.String }; }
			}
		}

		static MsdnXsltUtilities() {
			// install evaluate() function 

			FieldInfo fiCompiledStylesheet = typeof(XslTransform).GetField("_CompiledStylesheet", BindingFlags.Instance | BindingFlags.NonPublic);

			Type TXsltCompileContext = fiCompiledStylesheet.FieldType.Assembly.GetType("System.Xml.Xsl.XsltOld.XsltCompileContext");
			FieldInfo fiFunctionTable = TXsltCompileContext.GetField("s_FunctionTable", BindingFlags.Static | BindingFlags.NonPublic);
			Hashtable functionTable = (Hashtable)fiFunctionTable.GetValue(null);

			functionTable["evaluate"] = new FuncEvaluate();
		}

		private const string helpURL = "ms-help://";
		private const string sdkRoot = "/cpref/html/frlrf";
		private const string msdnOnlineSdkBaseUrl = "http://msdn.microsoft.com/{0}/library/{1}({2}).aspx";
		private const string msdnOnlineSdkPageExt = ".aspx";
		private const string systemPrefix = "System.";
		private readonly string sdkDocBaseUrl;
		private readonly string sdkVersion;
		private readonly string sdkDocExt;
		private readonly string sdkDocLanguage;
		private readonly string assemblyName;
		private readonly NameResolver nameResolver;
		private readonly StringCollection descriptions = new StringCollection();
		private readonly string encodingString;

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
			System.Text.Encoding fileEncoding) {
			this.nameResolver = ArgUtils.AssertNotNull(nameResolver, "nameResolver");
			this.assemblyName = assemblyName;
			sdkDocLanguage = ArgUtils.AssertNotNull(linkToSdkDocLangauge, "linkToSdkDocLangauge");
			sdkVersion = ArgUtils.AssertNotNull(linkToSdkDocVersion, "linkToSdkDocVersion");
			ArgUtils.AssertNotNull(fileEncoding, "fileEncoding");
			encodingString = "text/html; charset=" + fileEncoding.WebName;

			if (SdkLinksOnWeb) {
				sdkDocBaseUrl = msdnOnlineSdkBaseUrl;
				sdkDocExt = msdnOnlineSdkPageExt;
			}
		}

		/// <summary>
		/// Gets the base Url for system types links.
		/// </summary>
		public string SdkDocBaseUrl {
			get { return sdkDocBaseUrl; }
		}

		/// <summary>
		/// Gets the lamguage for system type links
		/// </summary>
		public string SdkDocLanguage {
			get { return sdkDocLanguage; }
		}

		/// <summary>
		/// Gets the page file extension for system types links.
		/// </summary>
		public string SdkDocExt {
			get { return sdkDocExt; }
		}

		/// <summary>
		/// Returns an HRef for a CRef.
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <param name="cref">CRef for which the HRef will be looked up.</param>
		public string GetHRef(string assemblyName, string cref) {
			string message = string.Format("GetHRef('{0}')", cref);

			if ((cref.Length < 2) || (cref[1] != ':')) {
				Debug.WriteLine(string.Format("{0} returns ''", message));
				return string.Empty;
			}

			if ((cref.Length < 9)
				|| (cref.Substring(2, 7) != systemPrefix)) {
				string fileName = nameResolver.GetFilenameForId(assemblyName, cref);
				if ((fileName == null) && cref.StartsWith("F:"))
					fileName = nameResolver.GetFilenameForId(assemblyName, "E:" + cref.Substring(2));

				if (fileName == null) {
					return "";
				}
				return fileName;
			}
			string result = String.Format(sdkDocBaseUrl, sdkDocLanguage, cref.Substring(2), sdkVersion);
			return result;
		}

		/// <summary>
		/// Returns a name for a CRef.
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <param name="cref">CRef for which the name will be looked up.</param>
		public string GetName(string assemblyName, string cref) {
			string name = null;

			if (cref.Length < 2) {
				name = cref;
			} else if (cref[1] == ':') {
				if ((cref.Length < 9)
					|| (cref.Substring(2, 7) != systemPrefix)) {
					name = nameResolver.GetDisplayNameForId(assemblyName, cref);
				}

				if (name == null) {
					int index;
					if ((index = cref.IndexOf(".#c")) >= 0)
						cref = cref.Substring(2, index - 2);
					else cref = (index = cref.IndexOf("(")) >= 0 ? cref.Substring(2, index - 2) : cref.Substring(2);
					name = cref.Substring(cref.LastIndexOf(".") + 1);
				}
			}

			//			Debug.WriteLine(string.Format("{0} returns {1}", message, name));
			return name;
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
		public bool HasSimilarOverloads(string description) {
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
		public string Replace(string str, string oldValue, string newValue) {
			return str.Replace(oldValue, newValue);
		}

		public string ToGeneralGenericFormat(string str) {
			return str.Replace('{', '(').Replace('}', ')');
		}

		/// <summary>
		/// Exposes easy formatting of SDK links to XSLT
		/// </summary>
		/// <param name="typename">Name of the type to refer to</param>
		/// <returns>URL to the type</returns>
		public string FormatOnlineSDKLink(string typename) {
			//Remove array brackets and unsafe pointers from the link
			typename = typename.Replace("[", "").Replace("]", "").Replace("*", "");
			return String.Format(sdkDocBaseUrl, sdkDocLanguage, typename, sdkVersion);
		}

		/// <summary>
		/// Gets HTML ContentType for the system's current ANSI code page. 
		/// </summary>
		/// <returns>ContentType attribute string</returns>
		public string GetContentType() {
			return encodingString;
		}

		public string GetFilenameForId(string assembly, string id) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForId(assembly, id);
		}
		
		public string GetFilenameForAssembly(string assembly) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForAssembly(assembly);
		}
		
		public string GetFilenameForNamespace(string assembly, string namespaceName) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForNamespace(assembly, namespaceName);
		}
		
		public string GetFilenameForNamespaceHierarchy(string assembly, string namespaceName) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForNamespaceHierarchy(assembly, namespaceName);
		}
		
		public string GetFilenameForTypeHierarchy(string assembly, string namespaceName) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForTypeHierarchy(assembly, namespaceName);
		}
		
		public string GetFilenameForConstructors(string assembly, string typeId) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForConstructorList(assembly, typeId);
		}
		
		public string GetFilenameForTypeMembers(string assembly, string typeId) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForTypeMemberList(assembly, typeId);
		}

		public string GetFilenameForPropertyOverloads(string assembly, string typeID, string propertyName) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForPropertyOverloads(assembly, typeID, propertyName);
		}
		
		public string GetFilenameForMethodOverloads(string assembly, string typeID, string methodName) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForMethodOverloads(assembly, typeID, methodName);
		}
		
		public string GetFilenameForTypename(string assembly, string typeName) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			return nameResolver.GetFilenameForTypename(assembly, typeName);
		}
		
		public string GetFilenameForCRefOverload(string assembly, string cref, string overload) {
			if (string.IsNullOrEmpty(assembly)) {
				assembly = assemblyName;
			}
			string result = nameResolver.GetFilenameForCRefOverload(assembly, cref, overload);
			return result;
		}
	}
}
