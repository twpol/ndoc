using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Diagnostics;
using System.Reflection;

namespace NDoc.Documenter.NativeHtmlHelp2.Engine.NamespaceMapping
{
	/// <summary>
	/// NamespaceMapper allows managed namespaces to be asscoitated with html help namespaces
	/// when creatin XLinks in the compiled help
	/// </summary>
	public class NamespaceMapper
	{
		private static string mapXmlNamespace = "urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.schemas.namespaceMap";

		private static XmlSchema namespaceMapSchema = null;
		private static XmlNamespaceManager nsmgr;
		private static bool schemaIsValid = true;

		static NamespaceMapper()
		{
			Stream schemaStream = GetSchemaResource();
			try
			{
				XmlSchema s = XmlSchema.Read( schemaStream, new ValidationEventHandler( validateSchema ) );

				NameTable table = new NameTable();
				nsmgr = new XmlNamespaceManager( table );
				nsmgr.AddNamespace( "map", mapXmlNamespace );
				
				namespaceMapSchema = s;
			}
			catch ( Exception )
			{
				namespaceMapSchema = null;
				nsmgr = null;
				schemaIsValid = false;
			}
			finally
			{
				schemaStream.Close();
			}
		}

		static Stream GetSchemaResource()
		{
			string name = "NDoc.Documenter.NativeHtmlHelp2.Engine.NamespaceMapping.NamespaceMap.xsd";

			return Assembly.GetExecutingAssembly().GetManifestResourceStream( name );
		}

		private static void validateSchema(object sender, ValidationEventArgs e)
		{
			Trace.WriteLine( e.Message );	
			schemaIsValid = false;
		}

		/// <summary>
		/// Creates a new instance of the NamespaceMapper class based on the specified map file
		/// </summary>
		/// <param name="path">Path to the map file</param>
		public NamespaceMapper( string path )
		{
			if ( !schemaIsValid )
				throw new Exception( "The namespaceMap schema is not valid or could not be found" );

			XmlValidatingReader reader = new XmlValidatingReader( new XmlTextReader( path ) );
			reader.Schemas.Add( namespaceMapSchema );
		
			XmlDocument doc = new XmlDocument();
			doc.Load( reader );
			map = doc.DocumentElement;
		}

		private XmlNode map;

		/// <summary>
		/// Sets the help namespace to use for system types
		/// </summary>
		/// <param name="systemHelpNamespace">The help namespace associates with system types</param>
		public void SetSystemNamespace( string systemHelpNamespace )
		{
			XmlNode systemNode = map.SelectSingleNode( "//map:managedNamespace[ @ns = 'System' ]", nsmgr );
			XmlNode helpNSNode = systemNode.SelectSingleNode( "parent::node()/@ns", nsmgr );
			helpNSNode.Value = systemHelpNamespace;
		}

		/// <summary>
		/// Looks up the html help 2 namespace associated with the specified manage namesapce
		/// </summary>
		/// <param name="managedName">The managed name to query for (case sensitive)</param>
		/// <returns>The best match for the managed namespace or an empty string if none is found</returns>
		public string LookupHelpNamespace( string managedName )
		{
			string helpNamespace = String.Empty;

			ManagedName name = new ManagedName( managedName );

			// since in most cases all managed names in a hierarchy will be in the
			// same help collection, let's fisrt try to short circuit the search by seeing
			// if there is a single managedNamespace entry for the root of the name we are looking for
			XmlNodeList firstTry = SelectManagedNamespaces( name.RootNamespace );
			if ( firstTry.Count == 1 )
			{
				XmlNode node = firstTry.Item( 0 );
				XmlNode helpNSNode = node.SelectSingleNode( "parent::node()/@ns", nsmgr );
				helpNamespace = helpNSNode.Value;
			}

			return helpNamespace;
		}

		private XmlNodeList SelectManagedNamespaces( string match )
		{
			string xpath = string.Format( "//map:managedNamespace[ @ns='{0}' ]", match );

			return map.SelectNodes( xpath, nsmgr );
		}

		#region ManagedName nested class
		private class ManagedName
		{
			string _ns;

			public ManagedName( string ns )
			{
				// strip off any NDoc type prefix
				int colonPos = ns.IndexOf( ':' );
				if ( colonPos > -1 )
					_ns = ns.Substring( colonPos + 1 );
				else
					_ns = ns;
			}

			public string RootNamespace
			{
				get
				{
					int firstDot = _ns.IndexOf( '.' );
					if ( firstDot > -1 )
						return _ns.Substring( 0, firstDot );

					return _ns;
				}
			}

			public override string ToString()
			{
				return _ns;
			}
		}
		#endregion
	}
}
