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
using System.Text;
using System.Diagnostics;
using System.Reflection;

using NDoc.Core;

using NDoc.Documenter.NativeHtmlHelp2.Compiler;
using NDoc.Documenter.NativeHtmlHelp2.HxProject;
using NDoc.Documenter.NativeHtmlHelp2.Engine;
using NDoc.Documenter.NativeHtmlHelp2.Engine.NamespaceMapping;

namespace NDoc.Documenter.NativeHtmlHelp2
{
	/// <summary>Native Html Help 2 MSDN.Net documenter</summary>
	public class NativeHtmlHelp2Documenter : BaseDocumenter
	{

		/// <summary>Initializes a new instance of the NativeHtmlHelp2Documenter class.</summary>
		public NativeHtmlHelp2Documenter() : base( "VS.NET 2003" )
		{
			Clear();
		}

		/// <summary>
		/// The development status (alpha, beta, stable) of this documenter.
		/// See <see cref="BaseDocumenter"/>
		/// </summary>
		public override DocumenterDevelopmentStatus DevelopmentStatus
		{
			get { return(DocumenterDevelopmentStatus.Alpha); }
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Clear()
		{
			//create a new instance of our config settings
			Config = new NativeHtmlHelp2Config();
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string MainOutputFile 
		{ 
			get 
			{
				return Path.Combine( MyConfig.OutputDirectory, MyConfig.HtmlHelpName + ".HxS" );
			} 			
		} 


		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string CanBuild( Project project, bool checkInputOnly )
		{
			string result = base.CanBuild(project, checkInputOnly); 
			if (result != null)
				return result;

			if ( !HxObject.HxCompIsInstalled )
				return "Could not find Html Help 2 compiler. Please make sure VSHIK 2003 is properly installed";

			if ( MyConfig.OutputDirectory == null )
				return "The output directory must be set";

			if ( MyConfig.UseHelpNamespaceMappingFile.Length != 0 )
			{
				if ( !File.Exists( MyConfig.UseHelpNamespaceMappingFile ) )
					return string.Format( "Could not find the namespace mapping file: {0}", Path.GetFullPath( MyConfig.UseHelpNamespaceMappingFile ) );

				try
				{
					NamespaceMapper mapper = new NamespaceMapper( Path.GetFullPath( MyConfig.UseHelpNamespaceMappingFile ) );
				}
				catch ( Exception e )
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendFormat( "The namespace mapping file {0} failed to validate.\n", Path.GetFullPath( MyConfig.UseHelpNamespaceMappingFile ) );
					sb.Append( "Make sure that it conforms to the NamespaceMap.xsd schema that can be found in the NDoc installation directory.\n" );
					sb.AppendFormat( "Parse error={0}", e.Message );
					return sb.ToString();
				}
			}

			if ( ( MyConfig.GenerateCollectionFiles || MyConfig.RegisterTitleWithNamespace ) && MyConfig.CollectionNamespace.Length == 0 )
				return "If GenerateCollectionFiles or RegisterTitleWithNamespace is true, a valid CollectionNamespace is required";

			if ( !checkInputOnly ) 
			{
				string temp = Path.Combine( MyConfig.OutputDirectory, "~HxS.tmp" );

				try
				{

					if ( File.Exists( MainOutputFile ) )
					{
						//if we can move the file, then it is not open...
						File.Move( MainOutputFile, temp );
						File.Move( temp, MainOutputFile );
					}
				}
				catch ( Exception )
				{
					result = "The compiled HTML Help file is probably open.\nPlease close it and try again.";
				}
			}

			return result;
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Build(Project project)
		{
			if ( !HxObject.HxCompIsInstalled )
				throw new DocumenterException( "Could not find Html Help 2 compiler. Please make sure VSHIK 2003 is properly installed" );

			try
			{
				OnDocBuildingStep( 0, "Initializing..." );
				UnPackResources();

				Workspace w = new Workspace( WorkingPath );
				PrepareWorkspace( w );

				ProjectFile HxProject = CreateProjectFile();

				OnDocBuildingStep( 10, "Merging XML documentation..." );
				XmlDocument xmlDocumentation = MergeXml( project );

				TOCFile toc = TOCFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\HelpTitle\project.HxT" ), MyConfig.HtmlHelpName );
				toc.LangId = MyConfig.LangID;

				HxProject.TOCFile = toc.FileName;
				HxProject.Save( w.WorkingDirectory );

				// create and intialize a HtmlFactory
				ExternalHtmlProvider htmlProvider = new ExternalHtmlProvider( MyConfig.HeaderHtml, MyConfig.FooterHtml );
				HtmlFactory factory = new HtmlFactory( w.ContentDirectory, htmlProvider, MyConfig.LinkToSdkDocVersion );

				using( new TOCBuilder( toc, factory ) )
					MakeHtml( xmlDocumentation, factory );

				toc.Save( w.WorkingDirectory );

				//then compile the HxC into and HxS
				OnDocBuildingStep( 65, "Compiling Html Help 2 Files..." );
				CompileHxCFile( w );

				// copy outputs to the final build location
				w.SaveOutputs( "*.Hxs" );
				w.SaveOutputs( "*.HxI" );

				// do clean up and final registration steps
				OnDocBuildingStep( 95, "Finishing up..." );
				if ( MyConfig.RegisterTitleWithNamespace )
					RegisterTitle( w );
				else if ( MyConfig.RegisterTitleAsCollection )
					RegisterCollection( w );

				if( MyConfig.GenerateCollectionFiles )
					CreateCollectionFiles( w );
			}
			catch ( Exception e )
			{
				throw new DocumenterException( "An error occured while creating the documentation", e );
			}
		}

		private ProjectFile CreateProjectFile()
		{
			// create a project file from the resource template
			ProjectFile project = ProjectFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\HelpTitle\project.HxC" ), MyConfig.HtmlHelpName );
			
			// set it up
			project.BuildSeperateIndexFile = MyConfig.BuildSeperateIndexFile;
			project.Copyright = MyConfig.CopyrightText;
			project.CreateFullTextIndex = MyConfig.CreateFullTextIndex;
			project.FileVersion = MyConfig.Version;
			project.LangId = MyConfig.LangID;
			project.Title = MyConfig.Title;

			if ( MyConfig.IncludeDefaultStopWordList )
				project.StopWordFile = string.Format( "FTSstop_{0}.stp", MyConfig.CharacterSet.ToString() );

			return project;
		}

		private void MakeHtml( XmlNode xmlDocumentation, HtmlFactory factory )
		{
			// load the stylesheets
			OnDocBuildingStep( 20, "Loading StyleSheets..." );
			factory.LoadStylesheets( ResourceDirectory );

			OnDocBuildingStep( 30, "Generating HTML..." );

			if ( MyConfig.UseHelpNamespaceMappingFile.Length != 0 )
				factory.SetNamespaceMap( MyConfig.UseHelpNamespaceMappingFile );

			// add properties to the factory
			// these get passed to the stylesheets
			factory.Properties.Add( "ndoc-title", MyConfig.Title );
			factory.Properties.Add( "ndoc-document-attributes", MyConfig.DocumentAttributes );
			factory.Properties.Add( "ndoc-documented-attributes", MyConfig.DocumentedAttributes );
			factory.Properties.Add( "ndoc-platforms", GetPlatformString() );
			factory.Properties.Add( "ndoc-version", MyConfig.Version );
			factory.Properties.Add( "ndoc-includeHierarchy", MyConfig.IncludeHierarchy );

			// make the html
			factory.MakeHtml( xmlDocumentation );
		}

		private string GetPlatformString()
		{
			StringBuilder sb = new StringBuilder();

			if ( MyConfig.PCPlatform == PCPlatforms.DesktopAndServer )
				sb.Append( "Windows 98, Windows NT 4.0, Windows Millennium Edition, Windows 2000, Windows XP Home Edition, Windows XP Professional, Windows Server 2003 family" );
			else if ( MyConfig.PCPlatform == PCPlatforms.ServerOnly )
				sb.Append( "Windows 2000, Windows XP Professional, Windows Server 2003 family" );

			if ( MyConfig.MONO )
			{
				if ( sb.Length > 0 )
					sb.Append( ", " );

				sb.Append( "MONO" );
			}

			if ( MyConfig.CompactFramework )
			{
				if ( sb.Length > 0 )
					sb.Append( ", " );

				sb.Append( ".NET Compact Framework - Windows CE .NET" );
			}

			return sb.ToString();
		}

		private XmlDocument MergeXml( Project project )
		{
			// Let the Documenter base class do it's thing.
			// Load the XML documentation into a DOM.
			XmlDocument xmlDocumentation = new XmlDocument();
			xmlDocumentation.LoadXml( MakeXml( project ) );
//xmlDocumentation.Save( @"C:\NRefDocTests.xml" );
			XmlNodeList typeNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/*[name()!='documentation']");
			if ( typeNodes.Count == 0 )			
				throw new DocumenterException("There are no documentable types in this project.");

			return xmlDocumentation;
		}

		private void PrepareWorkspace( Workspace w )
		{
			w.Clean();

			// import the base content files
			w.ImportContent( Path.Combine( ResourceDirectory, "graphics" ) );

			// import the project template
			w.ImportProjectFiles( Path.Combine( ResourceDirectory, "includes" ) );
			w.ImportProjectFiles( Path.Combine( ResourceDirectory, "NamespaceMapping" ) );
		}


		private static string ResourceDirectory = Path.Combine(
														Path.Combine(
															Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
															"NDoc"), 
														"NativeHtmlHelp2" );
		private void UnPackResources()
		{
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.xslt",
				Path.Combine( ResourceDirectory, "xslt") );

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.includes",
				Path.Combine( ResourceDirectory, "includes") );

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.graphics",
				Path.Combine( ResourceDirectory, "graphics") );

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.HxProject.HelpTitle",
				Path.Combine( ResourceDirectory, @"HxProject\HelpTitle") );

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.HxProject.HelpCollection",
				Path.Combine( ResourceDirectory, @"HxProject\HelpCollection") );
			
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.Engine.NamespaceMapping",
				Path.Combine( ResourceDirectory, "NamespaceMapping") );		

			// also unpack a copy of the namespace map schema into the runtime directory
			// so that users can find it there to use as a reference
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.Engine.NamespaceMapping",
				Path.GetDirectoryName( new Uri( Assembly.GetExecutingAssembly().CodeBase ).AbsolutePath ) );
		}

		private void CreateCollectionFiles( Workspace w )
		{
			string collectionName = MyConfig.HtmlHelpName + "Collection";

			// add the collection table of contents
			CollectionTOCFile toc = CollectionTOCFile.CreateFrom( 
				Path.Combine( ResourceDirectory, @"HxProject\HelpCollection\Collection.HxT" ), collectionName );

			toc.LangId = MyConfig.LangID;
			toc.Flat = MyConfig.CollectionTOCStyle == TOCStyle.Flat;
			toc.Title = MyConfig.Title;
			toc.BaseUrl = MyConfig.HtmlHelpName;
			toc.Save( w.RootDirectory );

			// add the collection file
			CollectionFile collection = CollectionFile.CreateFrom( 
				Path.Combine( ResourceDirectory, @"HxProject\HelpCollection\Collection.HxC" ), collectionName );
			
			collection.LangId = MyConfig.LangID;
			collection.Copyright = MyConfig.CopyrightText;
			collection.FileVersion = MyConfig.Version;
			collection.Title = MyConfig.Title;
			collection.TOCFile = toc.FileName;
			
			collection.Save( w.RootDirectory );

			// add the various index files
			IndexFile index = IndexFile.CreateFrom( 
				Path.Combine( ResourceDirectory, @"HxProject\HelpCollection\Collection_A.HxK" ), collectionName + "_A" );
			index.LangId = MyConfig.LangID;
			index.Save( w.RootDirectory );

			index = IndexFile.CreateFrom( 
				Path.Combine( ResourceDirectory, @"HxProject\HelpCollection\Collection_F.HxK" ), collectionName + "_F" );
			index.LangId = MyConfig.LangID;
			index.Save( w.RootDirectory );

			index = IndexFile.CreateFrom( 
				Path.Combine( ResourceDirectory, @"HxProject\HelpCollection\Collection_K.HxK" ), collectionName + "_K" );
			index.LangId = MyConfig.LangID;
			index.Save( w.RootDirectory );

			//TODO set up an H2Reg ini file and save it the workspace
		}
	
		private void RegisterCollection( Workspace w )
		{
			string ns = MyConfig.HtmlHelpName;

			if ( ns.Length > 0 )
			{
				HxReg reg = new HxReg();
				FileInfo f = new FileInfo( Path.Combine( w.RootDirectory, MyConfig.HtmlHelpName + ".Hxs" ) );
				reg.RegisterNamespace( ns, f, MyConfig.Title );
				reg.RegisterTitle( ns, ns, f );
			}
		}

		private void RegisterTitle( Workspace w )
		{
			string ns = MyConfig.CollectionNamespace;

			if ( ns.Length > 0 )
			{
				HxReg reg = new HxReg();
				reg.RegisterTitle( ns, MyConfig.HtmlHelpName, new FileInfo( Path.Combine( w.RootDirectory, MyConfig.HtmlHelpName + ".Hxs" ) ) );
			}
		}

		private void CompileHxCFile( Workspace w )
		{
			try
			{
				HxCompiler compiler = new HxCompiler();
				compiler.Compile( new DirectoryInfo( w.WorkingDirectory ), MyConfig.HtmlHelpName, MyConfig.LangID );
			}
			catch ( Exception e )
			{
				throw new DocumenterException( "HtmlHelp2 compilation error", e );
			}
		}

		private string WorkingPath
		{ 
			get
			{ 
				if ( Path.IsPathRooted( MyConfig.OutputDirectory ) )
					return MyConfig.OutputDirectory; 

				return Path.GetFullPath( MyConfig.OutputDirectory );
			} 
		}

		private NativeHtmlHelp2Config MyConfig{ get{ return (NativeHtmlHelp2Config)Config; } }
	}
}

