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
			get { return DocumenterDevelopmentStatus.Beta; }
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

			// validate the namespace map
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

			// validate that all of the additional content resources are present
			if ( MyConfig.IntroductionPage.Length != 0 && !File.Exists( MyConfig.IntroductionPage ) )
				return string.Format( "The file {0} could not be found", MyConfig.IntroductionPage );

			if ( MyConfig.AboutPageIconPage.Length != 0 && !File.Exists( MyConfig.AboutPageIconPage ) )
				return string.Format( "The file {0} could not be found", MyConfig.AboutPageIconPage );

			if ( MyConfig.AboutPageInfo.Length != 0 && !File.Exists( MyConfig.AboutPageInfo ) )
				return string.Format( "The file {0} could not be found", MyConfig.AboutPageInfo );

			if ( MyConfig.NavFailPage.Length != 0 && !File.Exists( MyConfig.NavFailPage ) )
				return string.Format( "The file {0} could not be found", MyConfig.NavFailPage );

			if ( MyConfig.EmptyIndexTermPage.Length != 0 && !File.Exists( MyConfig.EmptyIndexTermPage ) )
				return string.Format( "The file {0} could not be found", MyConfig.EmptyIndexTermPage );

			if ( MyConfig.ExtensibilityStylesheet.Length != 0 && !File.Exists( MyConfig.ExtensibilityStylesheet ) )
				return string.Format( "The file {0} could not be found", MyConfig.ExtensibilityStylesheet );

			if ( MyConfig.AdditionalContentResourceDirectory.Length != 0 && !Directory.Exists( MyConfig.AdditionalContentResourceDirectory ) )
				return string.Format( "The directory {0} could not be found", MyConfig.AdditionalContentResourceDirectory );

			// make sure we have a collection namespace
			if ( ( MyConfig.GenerateCollectionFiles || MyConfig.RegisterTitleWithNamespace ) && MyConfig.CollectionNamespace.Length == 0 )
				return "If GenerateCollectionFiles or RegisterTitleWithNamespace is true, a valid CollectionNamespace is required";

			// test if we can write to the output file
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
#if DEBUG
				int start = Environment.TickCount;
#endif
				OnDocBuildingStep( 0, "Initializing..." );
				UnPackResources();

				Workspace w = new NativeHtmlHelp2Workspace( WorkingPath );
				PrepareWorkspace( w );

				// set up the includes file
				IncludeFile includes = IncludeFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\HelpTitle\includes.hxf" ), "includes" );
				// attach to this event so resource directories get included in the include file
				w.ContentDirectoryAdded += new ContentEventHandler( includes.AddDirectory ); 

				// create and save the named url index
				CreateNamedUrlIndex( w );

				// save the includes file
				includes.Save( w.WorkingDirectory );

				// set up the table of contents
				TOCFile toc = TOCFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\HelpTitle\project.HxT" ), MyConfig.HtmlHelpName );
				toc.LangId = MyConfig.LangID;

				// set up the project file
				ProjectFile HxProject = CreateProjectFile();
				HxProject.TOCFile = toc.FileName;
				HxProject.Save( w.WorkingDirectory );

				// get the ndoc xml
				OnDocBuildingStep( 10, "Merging XML documentation..." );
				XmlDocument xmlDocumentation = MergeXml( project );

				// create and intialize a HtmlFactory
				ExternalHtmlProvider htmlProvider = new ExternalHtmlProvider( MyConfig.HeaderHtml, MyConfig.FooterHtml );
				HtmlFactory factory = new HtmlFactory( xmlDocumentation, w.ContentDirectory, htmlProvider, MyConfig.LinkToSdkDocVersion );

				// generate all the html content - builds the toc along the way
				using( new TOCBuilder( toc, factory ) )
					MakeHtml( factory );

				toc.Save( w.WorkingDirectory );

				//then compile the HxC into an HxS
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

				// create collection level files
				if( MyConfig.GenerateCollectionFiles )
					CreateCollectionFiles( w );

				if ( MyConfig.CleanIntermediates )
					w.CleanIntermediates();

#if DEBUG
				Trace.WriteLine( string.Format( "It took a total of {0} seconds", ( Environment.TickCount - start ) / 1000 ) );
#endif
				Trace.WriteLine( "Build complete" );
			}
			catch ( Exception e )
			{
				throw new DocumenterException( "An error occured while creating the documentation", e );
			}
		}

		private void CreateNamedUrlIndex( Workspace w )
		{
			NamedUrlFile namedUrlIndex = NamedUrlFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\HelpTitle\NamedURL.HxK" ), "NamedURL" );
			namedUrlIndex.LangId = MyConfig.LangID;

			if ( MyConfig.IntroductionPage.Length > 0 )			
				namedUrlIndex.IntroductionPage = ImportContentFileToWorkspace( MyConfig.IntroductionPage, w );

			if ( MyConfig.AboutPageIconPage.Length > 0 )			
				namedUrlIndex.AboutPageIcon = ImportContentFileToWorkspace( MyConfig.AboutPageIconPage, w );

			if ( MyConfig.AboutPageInfo.Length > 0 )			
				namedUrlIndex.AboutPageInfo = ImportContentFileToWorkspace( MyConfig.AboutPageInfo, w );

			if ( MyConfig.EmptyIndexTermPage.Length > 0 )			
				namedUrlIndex.EmptyIndexTerm = ImportContentFileToWorkspace( MyConfig.EmptyIndexTermPage, w );

			if ( MyConfig.NavFailPage.Length > 0 )			
				namedUrlIndex.NavFailPage = ImportContentFileToWorkspace( MyConfig.NavFailPage, w );
			
			if ( MyConfig.AdditionalContentResourceDirectory.Length > 0 )			
				w.ImportContentDirectory( MyConfig.AdditionalContentResourceDirectory );

			namedUrlIndex.Save( w.WorkingDirectory );
		}

		private static string ImportContentFileToWorkspace( string path, Workspace w )
		{
			string fileName = Path.GetFileName( path );
			w.ImportContent( Path.GetDirectoryName( path ), fileName );
			return Path.Combine( w.ContentDirectoryName, fileName );
		}


		private ProjectFile CreateProjectFile()
		{
			// create a project file from the resource template
			ProjectFile project = ProjectFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\HelpTitle\project.HxC" ), MyConfig.HtmlHelpName );
			
			// set it up
			project.BuildSeparateIndexFile = MyConfig.BuildSeparateIndexFile;
			project.Copyright = MyConfig.CopyrightText;
			project.CreateFullTextIndex = MyConfig.CreateFullTextIndex;
			project.FileVersion = MyConfig.Version;
			project.LangId = MyConfig.LangID;
			project.Title = MyConfig.Title;

			if ( MyConfig.IncludeDefaultStopWordList )
				project.StopWordFile = string.Format( "FTSstop_{0}.stp", MyConfig.CharacterSet.ToString() );

			return project;
		}

		private void MakeHtml( HtmlFactory factory )
		{
			// load the stylesheets
			OnDocBuildingStep( 20, "Loading StyleSheets..." );

			if ( MyConfig.ExtensibilityStylesheet.Length > 0 )
				factory.LoadStylesheets( MyConfig.ExtensibilityStylesheet, ResourceDirectory );

			else
				factory.LoadStylesheets( ResourceDirectory );

			OnDocBuildingStep( 30, "Generating HTML..." );

			if ( MyConfig.UseHelpNamespaceMappingFile.Length != 0 )
				factory.SetNamespaceMap( MyConfig.UseHelpNamespaceMappingFile );

			// add properties to the factory
			// these get passed to the stylesheets
			factory.Arguments.AddParam( "ndoc-title", "", MyConfig.Title );
			factory.Arguments.AddParam( "ndoc-document-attributes", "", MyConfig.DocumentAttributes );
			factory.Arguments.AddParam( "ndoc-documented-attributes", "", MyConfig.DocumentedAttributes );
			factory.Arguments.AddParam( "ndoc-net-framework-version", "", GetNETVersionString() );
			factory.Arguments.AddParam( "ndoc-version", "", MyConfig.Version );
			factory.Arguments.AddParam( "ndoc-includeHierarchy", "", MyConfig.IncludeHierarchy );
			factory.Arguments.AddParam( "ndoc-omit-syntax", "", MyConfig.OmitSyntaxSection );
			

#if DEBUG
			int start = Environment.TickCount;
#endif
			// make the html
			factory.MakeHtml();
#if DEBUG
			Trace.WriteLine( string.Format( "It took {0} seconds to make the html", ( Environment.TickCount - start ) / 1000 ) );
#endif
		}

		private string GetNETVersionString()
		{
			switch ( MyConfig.LinkToSdkDocVersion )
			{
				case SdkDocVersion.SDK_v1_1:	return "1.1";
				case SdkDocVersion.SDK_v1_0:	return "1.0";
				default:						return "";
			}
		}

		private XmlDocument MergeXml( Project project )
		{
			// Let the Documenter base class do it's thing.
			// Load the XML documentation into a DOM.
			XmlDocument xmlDocumentation = new XmlDocument();
			xmlDocumentation.LoadXml( MakeXml( project ) );
//xmlDocumentation.Save( @"C:\Tests.xml" );
			XmlNodeList typeNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/*[name()!='documentation']");
			
			if ( typeNodes.Count == 0 )			
				throw new DocumenterException("There are no documentable types in this project.");

			return xmlDocumentation;
		}

		private void PrepareWorkspace( Workspace w )
		{
			// delete any existing intermediates
			w.Clean();
			// preapre workspace
			w.Prepare();

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

			AddIndexToCollection( w, collection, "Collection_A.HxK", collectionName + "_A" );
			AddIndexToCollection( w, collection, "Collection_F.HxK", collectionName + "_F" );
			AddIndexToCollection( w, collection, "Collection_K.HxK", collectionName + "_K" );
			
			collection.Save( w.RootDirectory );
			
			// create and save the H2reg ini file
			H2RegFile h2reg = new H2RegFile( collectionName );
			h2reg.LangId = MyConfig.LangID;
			h2reg.CollectionFileName = collection.FileName;
			h2reg.Description = MyConfig.Title;
			h2reg.PluginNamespace = MyConfig.PlugInNamespace;
			
			if ( MyConfig.BuildSeparateIndexFile )
				h2reg.AddTitle( MyConfig.HtmlHelpName, MyConfig.LangID, MyConfig.HtmlHelpName + ".HxS", MyConfig.HtmlHelpName + ".HxI" );
			else
				h2reg.AddTitle( MyConfig.HtmlHelpName, MyConfig.LangID, MyConfig.HtmlHelpName + ".HxS" );

			h2reg.Save( w.RootDirectory );
		}

		private void AddIndexToCollection( Workspace w, CollectionFile collection, string templateName, string fileName )
		{
			IndexFile index = IndexFile.CreateFrom( 
				Path.Combine( ResourceDirectory, @"HxProject\HelpCollection\" + templateName ), fileName );
			index.LangId = collection.LangId;
			collection.AddKeywordIndex( index.FileName );
			index.Save( w.RootDirectory );
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
				Trace.WriteLine( "Compiling Html Help 2 file" );
#if DEBUG
				int start = Environment.TickCount;
#endif				
				HxCompiler compiler = new HxCompiler();
				compiler.Compile( new DirectoryInfo( w.WorkingDirectory ), MyConfig.HtmlHelpName, MyConfig.LangID );
#if DEBUG
				Trace.WriteLine( string.Format( "It took {0} seconds to compile the html", ( Environment.TickCount - start ) / 1000 ) );
#endif
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

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void View()
		{
			if (File.Exists(this.MainOutputFile))
			{
				try
				{
					// let's first try to start the Hxs using the shell.
					// If the user has FAR (or a tool like it) installed this will
					// open the title in their default HXs viewer even if it's not registered
					Process.Start(this.MainOutputFile);
				}
				catch ( System.ComponentModel.Win32Exception )
				{
					// well that didn't work, meaning the user doesn't have a default hxs viewer
					// let's try and open it in dexexplore

					// if the title is registers as a collection then use HtmlHelpName as the namesapce
					if ( MyConfig.RegisterTitleAsCollection )
					{
						StartDexplore( MyConfig.HtmlHelpName  );
					}
					// otherwise if we're registered in an external namespace open that one
					else if ( MyConfig.RegisterTitleWithNamespace )
					{
						StartDexplore( MyConfig.CollectionNamespace  );
					}
					else
					{
						string msg = "In order to view an Html Help 2 file it must " +
							"be registered. Set RegisterTitleAsCollection to true, rebuild " +
							"the project, and try again.";
						throw new DocumenterException( msg );
					}
				}
			}
			else
			{
				throw new FileNotFoundException("Documentation not built.",	this.MainOutputFile);
			}
		}

		private void StartDexplore( string ns )
		{
			// dexplore requires a namespace in order to view a help file
			string dexplore = Environment.GetFolderPath( Environment.SpecialFolder.CommonProgramFiles );
			dexplore = Path.Combine( dexplore, @"Microsoft Shared\Help\dexplore.exe" );
			string s = string.Format( "/helpcol \"ms-help://{0}\"", ns );

			Process.Start( dexplore, s );
		}
	}
}

