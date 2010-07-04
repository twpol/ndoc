using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using NDoc3.MsdnContentService;

namespace Core.Test {
	public class MsdnContentServiceTests {
		[Fact]
		public void GetLink() {
			ContentServiceHelper helper = new ContentServiceHelper("en-us", "VS.90");
			string link = helper.GetLink("T:System.String");
			Assert.Contains("s1wwdcbf", link);
		}

		[Fact]
		public void GetGenericLink() {
			ContentServiceHelper helper = new ContentServiceHelper("en-us", "VS.90");
			string link = helper.GetLink("M:System.Collections.Generic.IEnumerable`1.GetEnumerator");
			Assert.Contains("s793z9y2", link);
		}
	}
}
