using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace NDoc.UsersGuideStager
{
	/// <summary>
	/// Summary description for HHCTranslator.
	/// </summary>
	public class HHCTranslator
	{
		private string _hhcPath;

		public HHCTranslator( string hhc )
		{
			_hhcPath = hhc;
		}

		public void Translate( string destinationFile )
		{
			XslTransform transform = new XslTransform();
			transform.Load( new XmlTextReader( Assembly.GetExecutingAssembly().GetManifestResourceStream( "NDoc.UsersGuideStager.xslt.htmlcontents.xslt" ) ), null, null );

			XmlDocument doc = new XmlDocument();
			doc.Load( TransformHtmlToXml() );

			transform.Transform( doc, null, File.CreateText( destinationFile ), null );
		}

		private StreamReader TransformHtmlToXml()
		{
			XmlTextWriter writer = null; 
			TextReader reader = null;

			try
			{
				reader = new StreamReader( File.OpenRead( _hhcPath ) );

				Stream xml = new MemoryStream();
				writer = new XmlTextWriter( xml, Encoding.UTF8 );

				writer.WriteStartDocument();

				string line = reader.ReadLine();
				while ( line != null )
				{
					string newLine = TransformLine( line );

					if ( newLine.Length > 0 )
						writer.WriteRaw( newLine );

					line = reader.ReadLine();
				}

				writer.Flush();

				xml.Seek( 0, SeekOrigin.Begin );
			
				return new StreamReader( xml );
			}
			finally
			{
				if ( reader != null )
					reader.Close();
			}
		}

		private static string TransformLine( string line )
		{
			string ret = string.Empty;
			
			if ( line.IndexOf( "<!DOCTYPE " ) > -1 )
			{
				// strip out hte doctype
			}
			else if ( line.IndexOf( "<meta " ) > -1 )
			{
				// strip out any meta tags
			}
			else if ( line.IndexOf( "<LI>" ) > -1 )
			{
				// remove any of the unclosde LI tags
				ret = line.Replace( "<LI>", "" );
			}
			else if ( line.IndexOf( "<param " ) > -1 )
			{
				// make sure all hrefs are lower case (for the case sensitive Unix server at sf.net)
				if ( line.IndexOf( "name=\"Local" ) > -1 )
					ret = line.ToLower();
				else
					ret = line;

				// properly close param tags
				ret += "</param>\n";
			}
			else
			{
				ret = line;
			}

			return ret;
		}
	}
}
