using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Text;
using System.Reflection;
using System.Collections;

namespace NDoc.Core
{
	/// <summary>
	/// This class manages read write access to application settings
	/// </summary>
	public class Settings : IDisposable
	{

		/// <summary>
		/// The full path the the default settings file
		/// </summary>
		public static string ApplicationSettingsFile
		{
			get
			{
				return Path.Combine( SettingsLocation, "settings.xml" );
			}
		}

		/// <summary>
		/// The path to the folder where the settings file is stored
		/// </summary>
		public static string SettingsLocation
		{
			get
			{
				// create a path for this major.minor version of the app
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				string folder = string.Format( "NDoc.{0}.{1}", version.Major, version.Minor );
				return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), folder );
			}
		}

		private XmlNode data;
		private string path;

		/// <summary>
		/// Creates a new instance of the Settings class
		/// </summary>
		/// <param name="filePath">Path to serialized settings</param>
		public Settings( string filePath )
		{
			if ( File.Exists( filePath ) )
			{
				XmlTextReader reader = null;
				try
				{
					XmlDocument doc = new XmlDocument();
					reader = new XmlTextReader( filePath );
				
					doc.Load( reader );
					if ( doc.DocumentElement != null )
						data = doc.DocumentElement;				
				}
				catch ( Exception )
				{
					data = null;
				}
				finally
				{
					if ( reader != null )
						reader.Close();
				}
			}

			if ( data == null )			
				data = CreateNew( filePath );
			
			path = filePath;
		}

		/// <summary>
		/// <see cref="System.IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{			
			data.OwnerDocument.Save( path );
		}
		/// <summary>
		/// Retrieves the value of a setting
		/// </summary>
		/// <param name="section">The section name to store the list under</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="defaultValue">The value to use if no setting is found</param>
		/// <returns>The stored setting or the default value if no stroed setting is found</returns>
		public bool GetSetting( string section, string name, bool defaultValue )
		{
			try 
			{
				XmlNode setting = data.SelectSingleNode( string.Format( "{0}/{1}", section, name ) );
				if ( setting != null )
					return XmlConvert.ToBoolean( setting.InnerText );
			}
			catch ( Exception e )
			{
				Trace.WriteLine( e.Message );
			}

			return defaultValue;

		}
		/// <summary>
		/// Retrieves the value of a setting
		/// </summary>
		/// <param name="section">The section name to store the list under</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="defaultValue">The value to use if no setting is found</param>
		/// <returns>The stored setting or the default value if no stroed setting is found</returns>
		public int GetSetting( string section, string name, int defaultValue )
		{
			try 
			{
				XmlNode setting = data.SelectSingleNode( string.Format( "{0}/{1}", section, name ) );
				if ( setting != null )
					return XmlConvert.ToInt32( setting.InnerText );
			}
			catch ( Exception e )
			{
				Trace.WriteLine( e.Message );
			}

			return defaultValue;

		}
		/// <summary>
		/// Retrieves the value of a setting
		/// </summary>
		/// <param name="section">The section name to store the list under</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="defaultValue">The value to use if no setting is found</param>
		/// <returns>The stored setting or the default value if no stroed setting is found</returns>
		public string GetSetting( string section, string name, string defaultValue )
		{
			try 
			{
				XmlNode setting = data.SelectSingleNode( string.Format( "{0}/{1}", section, name ) );
				if ( setting != null )
					return setting.InnerText;
			}
			catch ( Exception e )
			{
				Trace.WriteLine( e.Message );
			}

			return defaultValue;

		}

		/// <summary>
		/// Retrieves the value of a setting
		/// </summary>
		/// <param name="section">The section name to store the list under</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="defaultValue">The value to use if no setting is found</param>
		/// <returns>The stored setting or the default value if no stroed setting is found</returns>
		public object GetSetting( string section, string name, object defaultValue )
		{
			if ( defaultValue == null )
				throw new NullReferenceException( "Null objects cannot be stored" );

			try 
			{
				XmlNode setting = data.SelectSingleNode( string.Format( "{0}/{1}", section, name ) );

				if ( setting != null )
				{
					XmlSerializer serializer = new XmlSerializer( defaultValue.GetType() );
				
					XmlNodeReader reader = new XmlNodeReader( setting.FirstChild );
					return serializer.Deserialize( reader );
				}
			}
			catch ( Exception e )
			{
				Trace.WriteLine( e.Message );
			}

			return defaultValue;
		}

		/// <summary>
		/// Retrieves a list of settings. If the list cannot be found
		/// then no items are added
		/// </summary>
		/// <param name="section">The section name to store the list under</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="itemType">The type of each setting in the list</param>
		/// <param name="list">A <see cref="IList"/> into which to put each item</param>
		public void GetSettingList( string section, string name, Type itemType, ref IList list )
		{
			if ( list == null )
				throw new NullReferenceException();

			try 
			{
				XmlNode setting = data.SelectSingleNode( string.Format( "{0}/{1}", section, name ) );

				if ( setting != null )
				{
					foreach( XmlNode node in setting.ChildNodes )
					{
						XmlSerializer serializer = new XmlSerializer( itemType );
				
						XmlNodeReader reader = new XmlNodeReader( node.FirstChild );
						list.Add( serializer.Deserialize( reader ) );
					}
				}
			}
			catch ( Exception e )
			{
				Trace.WriteLine( e.Message );
			}
		}

		/// <summary>
		/// Stores a list of settings
		/// </summary>
		/// <param name="section">The section name to store the list under</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="itemName">The name of each item in the list</param>
		/// <param name="list">The list</param>
		public void SetSettingList( string section, string name, string itemName, IList list )
		{
			if ( list == null )
				throw new NullReferenceException();

			XmlNode setting = GetOrCreateSettingNode( section, name );
			if ( setting.ChildNodes.Count > 0 )
				setting.RemoveAll();

			foreach( object o in list )
			{
				XmlNode item = setting.AppendChild( data.OwnerDocument.CreateElement( itemName ) );
				item.InnerXml = SerializeObject( o );
			}
		}
		/// <summary>
		/// Stores a setting
		/// </summary>
		/// <param name="section">The section name to store the setting in</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="val">The setting's value</param>
		public void SetSetting( string section, string name, bool val )
		{
			SetSetting( section, name, XmlConvert.ToString( val ) );
		}
		/// <summary>
		/// Stores a setting
		/// </summary>
		/// <param name="section">The section name to store the setting in</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="val">The setting's value</param>
		public void SetSetting( string section, string name, int val )
		{
			SetSetting( section, name, XmlConvert.ToString( val ) );
		}
		/// <summary>
		/// Stores a setting
		/// </summary>
		/// <param name="section">The section name to store the setting in</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="val">The setting's value</param>
		public void SetSetting( string section, string name, string val )
		{
			XmlNode setting = GetOrCreateSettingNode( section, name );
			setting.InnerText = val;
		}

		/// <summary>
		/// Stores a setting
		/// </summary>
		/// <param name="section">The section name to store the setting in</param>
		/// <param name="name">The name of the setting</param>
		/// <param name="val">The setting's value</param>
		public void SetSetting( string section, string name, object val )
		{
			XmlNode setting = GetOrCreateSettingNode( section, name );
			
			setting.InnerXml = SerializeObject( val );
		}

		private string SerializeObject( object o )
		{
			if ( o == null )
				throw new NullReferenceException( "Null objects cannot be stored" );

			XmlSerializer serializer = new XmlSerializer( o.GetType() );

			StringBuilder sb = new StringBuilder();
			NoPrologXmlWriter writer = new NoPrologXmlWriter( new StringWriter( sb ) );

			serializer.Serialize(writer, o);
			writer.Close();

			return sb.ToString();
		}

		private XmlNode GetOrCreateSettingNode( string section, string name )
		{
			XmlNode sectionNode = data.SelectSingleNode( section );
			if ( sectionNode == null )			
				sectionNode = data.AppendChild( data.OwnerDocument.CreateElement( section ) );

			XmlNode setting = sectionNode.SelectSingleNode( name );
			if ( setting == null )			
				setting = sectionNode.AppendChild( data.OwnerDocument.CreateElement( name ) );

			return setting;
		}

		private static XmlNode CreateNew( string path )
		{
			if ( File.Exists( path ) )
				File.Delete( path );

			string folder = Path.GetDirectoryName( path );

			if ( !Directory.Exists( folder ) )
				Directory.CreateDirectory( folder );

			XmlDocument doc = new XmlDocument();
			doc.LoadXml( "<?xml version='1.0'?><setttings/>" );
			doc.Save( path );
			return doc.DocumentElement;
		}

		/// <summary>
		/// This class is used to serialize objects without inserting
		/// xml prolog or doctype declarations
		/// </summary>
		private class NoPrologXmlWriter : XmlTextWriter
		{
			public NoPrologXmlWriter( TextWriter writer ) : base( writer )
			{
			}
			public NoPrologXmlWriter(Stream stream, Encoding encoding) : base( stream, encoding )
			{
			}
			public NoPrologXmlWriter(String s, Encoding encoding) : base( s, encoding )
			{
			}

			public override void WriteDocType(string name,string pubid,string sysid,string subset)
			{

			}
			
			public override void WriteStartDocument(bool standalone)
			{

			}
			
			public override void WriteStartDocument()
			{

			}
		}
	}
}
