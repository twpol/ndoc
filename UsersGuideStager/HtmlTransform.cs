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

			// make all filenames lower case. In the transform call all links are made lower case as well
			// this accounts for mixed case situation on Windows authoring boxes and the sf.net Unix machine
			StreamWriter writer = File.CreateText( Path.Combine( targetDir.FullName, htmlFile.Name.ToLower() ) );
			
			writer.Write( Transform( htmlDoc ).documentElement.outerHTML );

			writer.Close();
		}

		private static IHTMLDocument3 Transform( IHTMLDocument2 htmlDoc )
		{
			AddEmbeddedCSS( htmlDoc );
		
			// remove the onload InitTitle script call
			IHTMLBodyElement body = (IHTMLBodyElement)htmlDoc.body;
			body.onload = "";
			
			// because the scripts that insert the header and footer get
			// run when we load the source html, we remove them here
			// This also removes the dependency on having scripting enabled 
			// on client browsers for the online version
			foreach ( IHTMLDOMNode script in htmlDoc.scripts )
				script.parentNode.removeChild( script );

			IHTMLDocument3 doc3 = htmlDoc as IHTMLDocument3;
			Debug.Assert( doc3 != null );

			// fix up all of the hyper-links
			foreach ( IHTMLAnchorElement anchor in doc3.getElementsByTagName( "a" ) )
				Transform( anchor );

			// fix up all of the img tags
			foreach ( IHTMLImgElement img in doc3.getElementsByTagName( "img" ) )
				Transform( img );

			return doc3;
		}

		/// <summary>
		/// This embeds an inline css stylesheet that allows tags
		/// in the users guide html that are in the class "hideonline"
		/// to be hidden in the version poseted to the website
		/// </summary>
		/// <param name="htmlDoc"></param>
		private static void AddEmbeddedCSS( IHTMLDocument2 htmlDoc )
		{
			IHTMLStyleElement style = htmlDoc.createElement( "style" ) as IHTMLStyleElement;
			style.type = "text/css";
			style.styleSheet.cssText = ".hideonline{ display:none; }";

			IHTMLElementCollection collection = htmlDoc.all.tags( "head" ) as IHTMLElementCollection;
			Debug.Assert( collection != null );
			Debug.Assert( collection.length == 1 );

			mshtml.IHTMLDOMNode head = collection.item( 0, 0 ) as IHTMLDOMNode;
			Debug.Assert( head != null );
			head.appendChild( style as IHTMLDOMNode );
		}

		private static void Transform( IHTMLImgElement img )
		{
			if ( img.src != null )
			{
				// replace the local link to the sf logo to the online version tied to our project counter
				if ( img.src.IndexOf( "sf.gif" ) > -1 )
					img.src = "http://sourceforge.net/sflogo.php?group_id=36057&amp;type=5";
				else
					img.src = TransformLocalLink( img.src );
			}
		}

		private static void Transform( IHTMLAnchorElement anchor )
		{
			// have CHM links which would open a new window
			// replace the user's guide page
			if ( anchor.target == "_blank" )
				anchor.target = "_parent";

			if ( anchor.href != null )
			{
				// make sure all hrefs are lower case (Unix compatible)
				if ( anchor.href.StartsWith( "file:///" ) )
				{
					anchor.href = TransformLocalLink( anchor.href );
				}				
				else
				{
					anchor.href = anchor.href.ToLower();

					// replace all ms-help links with online equivalents
					if ( anchor.href.IndexOf( "ms-help" ) != -1 )
					{
						anchor.href = TransformMSHelpLink( anchor.href );
						anchor.target = "_parent";
					}
				}
			}

			// make all anchor names lower case as well
			if ( anchor.name != null )
				anchor.name = anchor.name.ToLower();
		}

		private static string TransformLocalLink( string href )
		{
			const string content = "content";
			Debug.Assert( href.IndexOf( content ) > -1 );

			// strip off the absolute part of the path
			// and replace back slashes with forward slashes
			return href.Substring( href.IndexOf( content ) + content.Length + 1 ).ToLower().Replace( @"\", "/" );
		}

		private static string TransformMSHelpLink( string href )
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
