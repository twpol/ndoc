using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;

namespace NDoc3.MsdnContentService {
	public class ContentServiceHelper {
		private const string MSDN_URL_TEMPLATE = "http://msdn.microsoft.com/{0}/library/{1}({2}).aspx";
		private static Dictionary<string, string> cache = new Dictionary<string, string>();
		private string locale;
		private string version;

		public ContentServiceHelper(string locale, string version) {
			this.locale = locale;
			this.version = version;
		}

		public string GetLink(string alias) {
			string link = GetLinkFromCache(alias);
			if (!String.IsNullOrEmpty(link)) return link;

			getContentRequest request = new getContentRequest();
			request.contentIdentifier = "AssetId:" + alias;
			request.locale = locale;
			request.version = version;

			ContentServicePortTypeClient proxy = new ContentServicePortTypeClient();
			getContentResponse response = null;
			appId id = new appId();
			id.value = "NDoc3";
			try {
				response = proxy.GetContent(id, request);
			} catch (SoapException e) {
				throw new Exception("Exception occurred while contacting MSDN Content Service", e);
			}
			string content = response.contentId;
			return String.Format(MSDN_URL_TEMPLATE, locale, response.contentId, version);
		}

		private string GetLinkFromCache(string alias) {
			return cache.ContainsKey(alias) ? cache[alias] : null;
		}
	}
}
