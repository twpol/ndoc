// MsdnDocumenter.cs - a MSDN-like documenter
// Copyright (C) 2003 Don Kackman
// Parts copyright 2001  Kral Ferch, Jason Diamond
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace NDoc.Documenter.NativeHtmlHelp2.Engine
{
	/// <summary>
	/// Deleagate for handling file events
	/// </summary>
	public delegate void TopicEventHandler( object sender, FileEventArgs args );

	/// <summary>
	/// The Html factory orchestrates the transformation of the NDoc Xml into the
	/// set of Html files that will be compiled into the help project
	/// </summary>
	public class HtmlFactory
	{
		private ArrayList documentedNamespaces;
		private string _outputDirectory;
		private ExternalHtmlProvider _htmlProvider;
		private StyleSheetCollection _stylesheets;
		private SdkDocVersion linkToSdkDocVersion = SdkDocVersion.SDK_v1_1;

		private FileNameMapper mapper;

		/// <summary>
		/// The collection of properties that are passed to each stylesheet
		/// </summary>
		public readonly Hashtable Properties = new Hashtable();

		/// <summary>
		/// Constructs a new instance of HtmlFactory
		/// </summary>
		/// <param name="outputDirectory">The directory to write the Html files to</param>
		/// <param name="htmlProvider">Object the provides additional Html content</param>
		public HtmlFactory( string outputDirectory, ExternalHtmlProvider htmlProvider )
		{			
			if ( !Directory.Exists( outputDirectory ) )
				throw new Exception( string.Format( "The output directory {0}, does not exist", outputDirectory ) );

			documentedNamespaces = new ArrayList();
			_outputDirectory = outputDirectory;

			_htmlProvider = htmlProvider;
		}

		#region events
		/// <summary>
		/// Event raised when a topic is started
		/// </summary>
		public event TopicEventHandler TopicStart;

		/// <summary>
		/// Raises the <see cref="TopicStart"/> event
		/// </summary>
		/// <param name="fileName">File name of the topic being started</param>
		protected virtual void OnTopicStart( string fileName )
		{
			if ( TopicStart != null )
				TopicStart( this, new FileEventArgs( fileName ) );
		}
	
		/// <summary>
		/// Event raises when a topic is closed
		/// </summary>
		public event EventHandler TopicEnd;
		/// <summary>
		/// Raises the <see cref="TopicEnd"/> event
		/// </summary>
		protected virtual void OnTopicEnd()
		{
			if ( TopicEnd != null )
				TopicEnd( this, EventArgs.Empty );
		}
	

		/// <summary>
		/// Event raised when a file is being added to a topic
		/// </summary>
		public event TopicEventHandler AddFileToTopic;
		/// <summary>
		/// Raises the <see cref="AddFileToTopic"/> event
		/// </summary>
		/// <param name="fileName">The file being added</param>
		protected virtual void OnAddFileToTopic( string fileName )
		{
			if ( AddFileToTopic != null )
				AddFileToTopic( this, new FileEventArgs( fileName ) );
		}
		#endregion

		/// <summary>
		/// loads and compiles all the stylesheets
		/// </summary>
		/// <param name="resourceDirectory">The location of the xslt files</param>
		public void LoadStylesheets( string resourceDirectory )
		{
			_stylesheets = StyleSheetCollection.LoadStyleSheets( resourceDirectory );
		}

		/// <summary>
		/// Generates HTML for the NDoc XML
		/// </summary>
		/// <param name="documentation">NDoc generated xml</param>
		/// <param name="sdkVersion">The SDK version to use</param>
		public void MakeHtml( XmlNode documentation, SdkDocVersion sdkVersion )
		{
			linkToSdkDocVersion = sdkVersion;
			mapper = new FileNameMapper();
			mapper.MakeElementNames( documentation );
			MakeHtmlForAssemblies( documentation );
		}


		private void MakeHtmlForAssemblies( XmlNode xmlDocumentation )
		{
			XmlNodeList assemblyNodes = xmlDocumentation.SelectNodes( "/ndoc/assembly" );
			int[] indexes = SortNodesByAttribute( assemblyNodes, "name" );

			NameValueCollection namespaceAssemblies	= new NameValueCollection();

			for ( int i = 0; i < assemblyNodes.Count; i++ )
			{
				XmlNode assemblyNode = assemblyNodes[indexes[i]];
				if ( assemblyNode.ChildNodes.Count > 0 )
				{
					string assemblyName = assemblyNode.Attributes["name"].Value;
					GetNamespacesFromAssembly( xmlDocumentation, assemblyName, namespaceAssemblies );
				}
			}

			string [] namespaces = namespaceAssemblies.AllKeys;
			Array.Sort( namespaces );

			for ( int i = 0; i < namespaces.Length; i++ )
			{
				string namespaceName = namespaces[i];
				foreach ( string assemblyName in namespaceAssemblies.GetValues( namespaceName ) )
					MakeHtmlForNamespace( xmlDocumentation, assemblyName, namespaceName );
			}
		}

		private static void GetNamespacesFromAssembly( XmlNode xmlDocumentation, string assemblyName, System.Collections.Specialized.NameValueCollection namespaceAssemblies)
		{
			XmlNodeList namespaceNodes = xmlDocumentation.SelectNodes("/ndoc/assembly[@name=\"" + assemblyName + "\"]/module/namespace");
			foreach ( XmlNode namespaceNode in namespaceNodes )
			{
				string namespaceName = namespaceNode.Attributes["name"].Value;
				namespaceAssemblies.Add( namespaceName, assemblyName );
			}
		}


		private void TransformAndWriteResult( XmlNode xmlDocumentation, string transformName, XsltArgumentList arguments, string fileName )
		{
			Trace.WriteLine( fileName );
#if DEBUG
			int start = Environment.TickCount;
#endif
			XslTransform transform = _stylesheets[transformName];

			_htmlProvider.SetFilename( fileName );

			using ( StreamWriter streamWriter = new StreamWriter(
				File.Open( Path.Combine( _outputDirectory, fileName ), FileMode.Create ), new UTF8Encoding( true ) ) )
			{
				foreach ( DictionaryEntry entry in Properties )				
					arguments.AddParam( entry.Key.ToString(), "", entry.Value );
				
				MsdnXsltUtilities utilities = new MsdnXsltUtilities( mapper.ElemNames, linkToSdkDocVersion );

				arguments.AddParam( "ndoc-sdk-doc-base-url", String.Empty, utilities.SdkDocBaseUrl );
		
				arguments.AddExtensionObject( "urn:ndoc-sourceforge-net:documenters.NativeHtmlHelp2.xsltUtilities", utilities );
				arguments.AddExtensionObject( "urn:NDocExternalHtml", _htmlProvider );

				transform.Transform( xmlDocumentation, arguments, streamWriter, null );
			}
#if DEBUG
			Trace.WriteLine( ( Environment.TickCount - start ).ToString() + " msec.");
#endif
		}


		private void MakeHtmlForNamespace( XmlNode xmlDocumentation, string assemblyName, string namespaceName )
		{
			if ( !documentedNamespaces.Contains( namespaceName ) ) 
			{
				documentedNamespaces.Add( namespaceName );

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam( "namespace", String.Empty, namespaceName );

				string fileName = FileNameMapper.GetFilenameForNamespace( namespaceName );
				TransformAndWriteResult( xmlDocumentation, "namespace", arguments, fileName );
				OnTopicStart( fileName );

				arguments = new XsltArgumentList();
				arguments.AddParam( "namespace", String.Empty, namespaceName );

				if ( Properties.Contains("includeHierarchy") && (bool)Properties["includeHierarchy"] )
					TransformAndWriteResult( xmlDocumentation, "namespacehierarchy", arguments, FileNameMapper.GetFileNameForNamespaceHierarchy( namespaceName ) );

				MakeHtmlForTypes( xmlDocumentation, namespaceName );

				OnTopicEnd();
			}
		}

		private void MakeHtmlForTypes( XmlNode xmlDocumentation, string namespaceName )
		{
			XmlNodeList typeNodes =
				xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace[@name=\"" + namespaceName + "\"]/*[local-name()!='documentation']");

			int[] indexes = SortNodesByAttribute( typeNodes, "id" );

			for ( int i = 0; i < typeNodes.Count; i++ )
			{
				XmlNode typeNode = typeNodes[ indexes[i] ];

				switch( FileNameMapper.GetWhichType( typeNode ) )
				{
					case WhichType.Class:
						MakeHtmlForInterfaceOrClassOrStructure( xmlDocumentation, typeNode );
						break;
					case WhichType.Interface:
						MakeHtmlForInterfaceOrClassOrStructure( xmlDocumentation, typeNode );
						break;
					case WhichType.Structure:
						MakeHtmlForInterfaceOrClassOrStructure( xmlDocumentation, typeNode );
						break;
					case WhichType.Enumeration:
						MakeHtmlForEnumerationOrDelegate( xmlDocumentation, typeNode );
						break;
					case WhichType.Delegate:
						MakeHtmlForEnumerationOrDelegate( xmlDocumentation, typeNode );
						break;
					default:
						break;
				}
			}
		}

		private void MakeHtmlForEnumerationOrDelegate( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			string typeName = typeNode.Attributes["name"].Value;
			string typeID = typeNode.Attributes["id"].Value;

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam( "type-id", String.Empty, typeID );

			string fileName = FileNameMapper.GetFilenameForType( typeNode );
			TransformAndWriteResult( xmlDocumentation, "type", arguments, fileName );
			OnAddFileToTopic( fileName );
		}

		private void MakeHtmlForInterfaceOrClassOrStructure( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			string typeName = typeNode.Attributes["name"].Value;
			string typeID = typeNode.Attributes["id"].Value;

			XsltArgumentList arguments = new XsltArgumentList();
			arguments.AddParam( "type-id", String.Empty, typeID );

			string fileName = FileNameMapper.GetFilenameForType( typeNode );
			TransformAndWriteResult( xmlDocumentation, "type", arguments, fileName );
			OnTopicStart( fileName );

			if ( typeNode.SelectNodes( "constructor|field|property|method|operator|event" ).Count > 0 )
			{
				arguments = new XsltArgumentList();
				arguments.AddParam("id", String.Empty, typeID);

				fileName = FileNameMapper.GetFilenameForTypeMembers(typeNode);
				TransformAndWriteResult( xmlDocumentation, "allmembers", arguments, fileName );
				OnAddFileToTopic( fileName );

				MakeHtmlForConstructors( xmlDocumentation, typeNode );
				MakeHtmlForFields( xmlDocumentation, typeNode );
				MakeHtmlForProperties( xmlDocumentation, typeNode );
				MakeHtmlForMethods( xmlDocumentation, typeNode );
				MakeHtmlForOperators( xmlDocumentation, typeNode );
				MakeHtmlForEvents( xmlDocumentation, typeNode );
			}

			OnTopicEnd();
		}

		private void MakeHtmlForConstructors( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			string typeName = typeNode.Attributes["name"].Value;
			string typeID = typeNode.Attributes["id"].Value;
			XmlNodeList constructorNodes = typeNode.SelectNodes( "constructor[@contract!='Static']" );

			// If the constructor is overloaded then make an overload page.
			if ( constructorNodes.Count > 1 )
			{
				string constructorID = constructorNodes[0].Attributes["id"].Value;

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam( "member-id", String.Empty, constructorID );

				string fileName = FileNameMapper.GetFilenameForConstructors(typeNode);
				TransformAndWriteResult( xmlDocumentation, "memberoverload", arguments, fileName );
				OnTopicStart( fileName );
			}

			foreach ( XmlNode constructorNode in constructorNodes )
			{
				string constructorID = constructorNode.Attributes["id"].Value;

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam( "member-id", String.Empty, constructorID );

				string fileName = FileNameMapper.GetFilenameForConstructor(constructorNode);
				TransformAndWriteResult( xmlDocumentation, "member", arguments, fileName );
				OnAddFileToTopic( fileName );
			}

			if ( constructorNodes.Count > 1 )
				OnTopicEnd();
			
			XmlNode staticConstructorNode = typeNode.SelectSingleNode("constructor[@contract='Static']");
			if ( staticConstructorNode != null )
			{
				string constructorID = staticConstructorNode.Attributes["id"].Value;

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam("member-id", String.Empty, constructorID);

				string fileName = FileNameMapper.GetFilenameForConstructor(staticConstructorNode);
				TransformAndWriteResult( xmlDocumentation, "member", arguments, fileName );
				OnAddFileToTopic( fileName );
			}
		}

		private void MakeHtmlForFields( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			XmlNodeList fields = typeNode.SelectNodes("field[not(@declaringType)]");

			if ( fields.Count > 0 )
			{
				string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam( "id", String.Empty, typeID );
				arguments.AddParam( "member-type", String.Empty, "field" );

				string fileName = FileNameMapper.GetFilenameForTypeFields( typeNode );
				TransformAndWriteResult( xmlDocumentation, "individualmembers", arguments, fileName );
				OnTopicStart( fileName );

				int[] indexes = SortNodesByAttribute(fields, "id");

				foreach ( int index in indexes )
				{
					XmlNode field = fields[index];

					string fieldName = field.Attributes["name"].Value;
					string fieldID = field.Attributes["id"].Value;

					arguments = new XsltArgumentList();
					arguments.AddParam( "field-id", String.Empty, fieldID );

					fileName = FileNameMapper.GetFilenameForField(field);
					TransformAndWriteResult( xmlDocumentation, "field", arguments, fileName );
					OnAddFileToTopic( fileName );
				}

				OnTopicEnd();
			}
		}

		private void MakeHtmlForProperties( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			XmlNodeList declaredPropertyNodes = typeNode.SelectNodes("property[not(@declaringType)]");

			if ( declaredPropertyNodes.Count > 0 )
			{
				string previousPropertyName;
				string nextPropertyName;

				string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;
				XmlNodeList propertyNodes = typeNode.SelectNodes( "property[not(@declaringType)]" );

				int[] indexes = SortNodesByAttribute( propertyNodes, "id" );

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam( "id", String.Empty, typeID );
				arguments.AddParam( "member-type", String.Empty, "property" );

				string fileName = FileNameMapper.GetFilenameForTypeProperties( typeNode );
				TransformAndWriteResult( xmlDocumentation, "individualmembers", arguments, fileName );
				OnTopicStart( fileName );

				for ( int i = 0; i < propertyNodes.Count; i++ )
				{
					XmlNode propertyNode = propertyNodes[indexes[i]];

					string propertyName = propertyNode.Attributes["name"].Value;
					string propertyID = propertyNode.Attributes["id"].Value;

					previousPropertyName = ( (i - 1 < 0 ) || ( propertyNodes[indexes[i - 1]].Attributes.Count == 0 ) )
						? "" : propertyNodes[indexes[i - 1]].Attributes[0].Value;
					nextPropertyName = ( ( i + 1 == propertyNodes.Count ) || ( propertyNodes[indexes[i + 1]].Attributes.Count == 0 ) )
						? "" : propertyNodes[indexes[i + 1]].Attributes[0].Value;

					if ( ( previousPropertyName != propertyName ) && ( nextPropertyName == propertyName ) )
					{
						arguments = new XsltArgumentList();
						arguments.AddParam( "member-id", String.Empty, propertyID );

						fileName = FileNameMapper.GetFilenameForPropertyOverloads( typeNode, propertyNode );
						TransformAndWriteResult( xmlDocumentation, "memberoverload", arguments, fileName );
						OnTopicStart( fileName );
					}

					XsltArgumentList arguments2 = new XsltArgumentList();
					arguments2.AddParam( "property-id", String.Empty, propertyID );

					fileName = FileNameMapper.GetFilenameForProperty( propertyNode );
					TransformAndWriteResult( xmlDocumentation, "property", arguments2, fileName );
					OnAddFileToTopic( fileName );

					if ( ( previousPropertyName == propertyName ) && ( nextPropertyName != propertyName ) )
						OnTopicEnd();
				}

				OnTopicEnd();
			}
		}
		private void MakeHtmlForMethods( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			XmlNodeList declaredMethodNodes = typeNode.SelectNodes( "method[not(@declaringType)]" );

			if ( declaredMethodNodes.Count > 0 )
			{
				bool bOverloaded = false;

				string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;
				XmlNodeList methodNodes = typeNode.SelectNodes( "method" );

				int[] indexes = SortNodesByAttribute( methodNodes, "id" );

				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam( "id", String.Empty, typeID );
				arguments.AddParam( "member-type", String.Empty, "method" );

				string fileName = FileNameMapper.GetFilenameForTypeMethods( typeNode );
				TransformAndWriteResult( xmlDocumentation, "individualmembers", arguments, fileName );
				OnTopicStart( fileName );

				for (int i = 0; i < methodNodes.Count; i++)
				{
					XmlNode methodNode = methodNodes[indexes[i]];
					string methodName = methodNode.Attributes["name"].Value;
					string methodID = methodNode.Attributes["id"].Value;

					if ( MethodHelper.IsMethodFirstOverload( methodNodes, indexes, i ) )
					{
						bOverloaded = true;

						arguments = new XsltArgumentList();
						arguments.AddParam( "member-id", String.Empty, methodID );

						fileName = FileNameMapper.GetFilenameForMethodOverloads( typeNode, methodNode );
						TransformAndWriteResult( xmlDocumentation, "memberoverload", arguments, fileName );
						OnTopicStart( fileName );
					}

					if ( methodNode.Attributes["declaringType"] == null )
					{
						XsltArgumentList arguments2 = new XsltArgumentList();
						arguments2.AddParam( "member-id", String.Empty, methodID );

						fileName = FileNameMapper.GetFilenameForMethod( methodNode );
						TransformAndWriteResult( xmlDocumentation, "member", arguments2, fileName );
						OnAddFileToTopic( fileName );
					}

					if ( bOverloaded && MethodHelper.IsMethodLastOverload( methodNodes, indexes, i ) )
					{
						bOverloaded = false;
						OnTopicEnd();
					}
				}

				OnTopicEnd();
			}
		}

		private void MakeHtmlForEvents( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			XmlNodeList declaredEventNodes = typeNode.SelectNodes( "event[not(@declaringType)]" );

			if ( declaredEventNodes.Count > 0 )
			{
				XmlNodeList events = typeNode.SelectNodes( "event" );

				if ( events.Count > 0 )
				{
					string typeName = typeNode.Attributes["name"].Value;
					string typeID = typeNode.Attributes["id"].Value;

					XsltArgumentList arguments = new XsltArgumentList();
					arguments.AddParam( "id", String.Empty, typeID );
					arguments.AddParam( "member-type", String.Empty, "event" );

					string fileName = FileNameMapper.GetFilenameForTypeEvents( typeNode );
					TransformAndWriteResult( xmlDocumentation, "individualmembers", arguments, fileName );
					OnTopicStart( fileName );

					int[] indexes = SortNodesByAttribute( events, "id" );

					foreach ( int index in indexes )
					{
						XmlNode eventElement = events[index];

						if ( eventElement.Attributes["declaringType"] == null )
						{
							string eventName = eventElement.Attributes["name"].Value;
							string eventID = eventElement.Attributes["id"].Value;

							arguments = new XsltArgumentList();
							arguments.AddParam( "event-id", String.Empty, eventID );

							fileName = FileNameMapper.GetFilenameForEvent( eventElement );
							TransformAndWriteResult( xmlDocumentation, "event", arguments, fileName );
							OnAddFileToTopic( fileName );
						}
					}

					OnTopicEnd();
				}
			}
		}

		private void MakeHtmlForOperators( XmlNode xmlDocumentation, XmlNode typeNode )
		{
			XmlNodeList operators = typeNode.SelectNodes( "operator" );

			if ( operators.Count > 0 )
			{
				string typeName = typeNode.Attributes["name"].Value;
				string typeID = typeNode.Attributes["id"].Value;
				XmlNodeList opNodes = typeNode.SelectNodes( "operator" );
				bool bOverloaded = false;

				string title = "Operators";

				if ( typeNode.SelectSingleNode( "operator[@name = 'op_Explicit' or @name = 'op_Implicit']" ) != null )
					title += " and Type Conversions";
				
				XsltArgumentList arguments = new XsltArgumentList();
				arguments.AddParam( "id", String.Empty, typeID );
				arguments.AddParam( "member-type", String.Empty, "operator" );

				string fileName = FileNameMapper.GetFilenameForTypeOperators( typeNode );
				TransformAndWriteResult( xmlDocumentation, "individualmembers", arguments, fileName );
				OnTopicStart( fileName );

				int[] indexes = SortNodesByAttribute( operators, "id" );

				for ( int i = 0; i < opNodes.Count; i++ )
				{
					XmlNode operatorNode = operators[indexes[i]];
					string operatorID = operatorNode.Attributes["id"].Value;

					if ( MethodHelper.IsMethodFirstOverload( opNodes, indexes, i ) )
					{
						string opName = operatorNode.Attributes["name"].Value;
						if ( ( opName != "op_Implicit" ) && ( opName != "op_Implicit" ) )
						{
							bOverloaded = true;

							arguments = new XsltArgumentList();
							arguments.AddParam( "member-id", String.Empty, operatorID );

							fileName = FileNameMapper.GetFilenameForOperatorsOverloads( typeNode, operatorNode );
							TransformAndWriteResult( xmlDocumentation, "memberoverload", arguments, fileName );
							OnTopicStart( fileName );
						}
					}

					arguments = new XsltArgumentList();
					arguments.AddParam("member-id", String.Empty, operatorID);

					fileName = FileNameMapper.GetFilenameForOperator(operatorNode);
					TransformAndWriteResult( xmlDocumentation, "member", arguments, fileName);
					OnAddFileToTopic( fileName );

					if ( bOverloaded && MethodHelper.IsMethodLastOverload( opNodes, indexes, i ) )
					{
						bOverloaded = false;
						OnTopicEnd();
					}
				}

				OnTopicEnd();
			}
		}

		private static int[] SortNodesByAttribute( XmlNodeList nodes, string attributeName )
		{
			string[] names = new string[nodes.Count];
			int[] indexes = new int[nodes.Count];
			int i = 0;

			foreach ( XmlNode node in nodes )
			{
				names[i] = node.Attributes[attributeName].Value;
				indexes[i] = i++;
			}

			Array.Sort( names, indexes );

			return indexes;
		}
	}
}
