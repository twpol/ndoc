using System;
using System.IO;
using System.Xml;
using System.Diagnostics;

using NDoc.Core;

namespace NDoc.Documenter.HtmlHelp2.Compiler
{
	/// <summary>
	/// Augments the Xml dat islands that HxConv creates by default
	/// in the html help files with additional index and MSHelp tags
	/// to increase integration with VS.NET
	/// </summary>
	public class HxAugmenter
	{
		/// <summary>
		/// Creates a new instance of an HxAugmenter class
		/// </summary>
		public HxAugmenter()
		{
		}

		/// <summary>
		/// Augments all of the html files in the specified directory
		/// with additonal MSHelp tags
		/// </summary>
		/// <param name="workingDir"></param>
		public void Augment( DirectoryInfo workingDir, string helpName )
		{
			InsertFIndex( workingDir, helpName );
		}

		private void InsertFIndex( DirectoryInfo workingDir, string helpName )
		{
			string FIndexName = string.Format( "{0}_F.HxK", helpName );

			// Write the embedded context index to the html output directory
			EmbeddedResources.WriteEmbeddedResource(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.HtmlHelp2.xml._F.HxK",
				workingDir.FullName,
				FIndexName );

			string HxCName = Path.Combine( workingDir.FullName, helpName + ".HxC" );

			Trace.WriteLine( string.Format( "Adding F Index to {0}", helpName + ".HxC" ) );

			XmlValidatingReader reader = new XmlValidatingReader( new XmlTextReader( HxCName ) );
			reader.ValidationType = ValidationType.None;
			reader.XmlResolver = null;		// it doesn't like the ms-help in the DTD URI

			XmlDocument HxCDoc = new XmlDocument();
			
			HxCDoc.Load( reader );

			// all of the KeywordIndexDef nodes need to be grouped together
			XmlNode sibling = HxCDoc.DocumentElement.SelectSingleNode( "KeywordIndexDef" );
			Debug.Assert( sibling != null );

			XmlElement KeywordIndexDef = HxCDoc.CreateElement( "KeywordIndexDef" );
			KeywordIndexDef.SetAttribute( "File", FIndexName );

			HxCDoc.DocumentElement.InsertAfter( KeywordIndexDef, sibling );

			reader.Close();			//make sure we close the reader before saving
			HxCDoc.Save( HxCName );
		}
	}
}
