using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

using mshtml;

namespace NDoc.UsersGuideStager
{
	/// <summary>
	/// Summary description for HtmlTransform.
	/// </summary>
	public class HtmlTransform
	{

		public static void Transform( FileInfo htmlFile, DirectoryInfo targetDir )
		{
			Console.WriteLine( string.Format( "Staging {0}", htmlFile.Name ) );

			IHTMLDocument2 htmlDoc = OpenHtml( "file:///" + htmlFile.FullName );

			StreamWriter writer = File.CreateText( Path.Combine( targetDir.FullName, htmlFile.Name.ToLower() ) );
			
			writer.Write( Transform( htmlDoc ).documentElement.outerHTML );

			writer.Close();
		}

		private static IHTMLDocument3 Transform( IHTMLDocument2 htmlDoc )
		{
			IHTMLDocument3 doc3 = htmlDoc as IHTMLDocument3;
			Debug.Assert( doc3 != null );
			
			// remove the onload InitTitle script call
			IHTMLBodyElement body = (IHTMLBodyElement)htmlDoc.body;
			body.onload = "";
			
			// because the scripts that insert the header and footer get
			// run when we load the source html, we remove them here
			// This allows removes the dependency on having scripting enabled 
			// on client browsers for the online version
			foreach ( IHTMLDOMNode script in htmlDoc.scripts )
				script.parentNode.removeChild( script );

			// fix up all of the hyper-links
			foreach ( IHTMLAnchorElement anchor in doc3.getElementsByTagName( "a" ) )
				Transform( anchor );

			return doc3;
		}

		private static void Transform( IHTMLAnchorElement anchor )
		{
			// have CHM links which would open a new window
			// replace the user's guide page
			if ( anchor.target == "_blank" )
				anchor.target = "_parent";

			// make sure all hrefs are lower case (Unix compatible)
			if ( anchor.href != null )
			{
				if ( !anchor.href.StartsWith( "file:///" ) )
				{
					anchor.href = anchor.href.ToLower();

					// replace all ms-help links with online equivalents
					if ( anchor.href.IndexOf( "ms-help" ) != -1 )
						anchor.href = Transform( anchor.href );
				}
			}
		}

		private static string Transform( string href )
		{
			string ret = "";
			
			if ( href.IndexOf( "ms.vshik.2003" ) > -1 )
			{
				// there is no online version of VSHIK - so all these links will point to the download page
				ret = "http://msdn.microsoft.com/library/default.asp?url=/library/en-us/htmlhelp/html/hwmscextendingnethelp.asp";
			}
			else
			{
				ret = href.Replace( "ms-help://ms.netframeworksdkv1.1/", "http://msdn.microsoft.com/library/default.asp?url=/library/en-us/" );
				ret = ret.Replace( ".htm", ".asp" );
			}

			return ret;
		}


		private static IHTMLDocument2 OpenHtml( string uri )
		{
			// this is a dummy document used to open the real document we're after
			HTMLDocumentClass doc = new HTMLDocumentClass();
			// we need to do these QI's because the disp interface methods
			// seem to fail
			IHTMLDocument2 iDoc2a = doc;
			IHTMLDocument4 iDoc4 = doc;

			// need to put some html into the dummy document
			iDoc2a.writeln("<html></html>");
			iDoc2a.close();

			IHTMLDocument2 htmlDoc = doc.createDocumentFromUrl( uri, "null" );

			for(uint i = 0; i < 300 && htmlDoc.readyState != "complete"; i++)
				Thread.Sleep(100);

			return htmlDoc;
		}
	}
}
