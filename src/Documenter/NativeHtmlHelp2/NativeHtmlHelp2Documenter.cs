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

				Workspace workspace = new NativeHtmlHelp2Workspace( WorkingPath );
				workspace.Clean();
				workspace.Prepare();

				UnPackResources( workspace );

				// set up the includes file
				IncludeFile includes = IncludeFile.CreateFrom( Path.Combine( workspace.ResourceDirectory, "includes.hxf" ), "includes" );
				// attach to this event so resource directories get included in the include file
				workspace.ContentDirectoryAdded += new ContentEventHandler( includes.AddDirectory ); 

				// create and save the named url index
				CreateNamedUrlIndex( workspace );

				// save the includes file
				includes.Save( workspace.WorkingDirectory );

				// set up the table of contents
				TOCFile toc = TOCFile.CreateFrom( Path.Combine( workspace.ResourceDirectory, "project.HxT" ), MyConfig.HtmlHelpName );
				toc.LangId = MyConfig.LangID;

				// set up the project file
				ProjectFile HxProject = CreateProjectFile( workspace );
				HxProject.TOCFile = toc.FileName;
				HxProject.Save( workspace.WorkingDirectory );

				// get the ndoc xml
				OnDocBuildingStep( 10, "Merging XML documentation..." );
				string tempFileName = MakeXmlFile( project );
				//Note: Temp file will be deleted by factory after loading...

				// create and intialize a HtmlFactory
				ExternalHtmlProvider htmlProvider = new ExternalHtmlProvider( MyConfig.HeaderHtml, MyConfig.FooterHtml );
				string DocLangCode = Enum.GetName(typeof(SdkLanguage),MyConfig.SdkDocLanguage).Replace("_","-");
				HtmlFactory factory = new HtmlFactory( tempFileName, workspace.ContentDirectory, htmlProvider, MyConfig.SdkDocVersion, DocLangCode );

				// generate all the html content - builds the toc along the way
				using( new TOCBuilder( toc, factory ) )
					MakeHtml( workspace, factory );

				toc.Save( workspace.WorkingDirectory );

				//then compile the HxC into an HxS
				OnDocBuildingStep( 65, "Compiling Html Help 2 Files..." );
				CompileHxCFile( workspace );

				// copy outputs to the final build location
				workspace.SaveOutputs( "*.Hxs" );
				workspace.SaveOutputs( "*.HxI" );

				// do clean up and final registration steps
				OnDocBuildingStep( 95, "Finishing up..." );

				if ( MyConfig.RegisterTitleWithNamespace )
					RegisterTitleWithCollection( workspace );
				else if ( MyConfig.RegisterTitleAsCollection )
					RegisterTitleAsCollection( workspace );

				// create collection level files
				if( MyConfig.GenerateCollectionFiles )
					CreateCollectionFiles( workspace );

				workspace.RemoveResourceDirectory();
				if ( MyConfig.CleanIntermediates )
					workspace.CleanIntermediates();

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

		private void CreateNamedUrlIndex( Workspace workspace )
		{
			NamedUrlFile namedUrlIndex = NamedUrlFile.CreateFrom( Path.Combine( workspace.ResourceDirectory, "NamedURL.HxK" ), "NamedURL" );
			namedUrlIndex.LangId = MyConfig.LangID;

			if ( MyConfig.IntroductionPage.Length > 0 )			
				namedUrlIndex.IntroductionPage = ImportContentFileToWorkspace( MyConfig.IntroductionPage, workspace );

			if ( MyConfig.AboutPageIconPage.Length > 0 )			
				namedUrlIndex.AboutPageIcon = ImportContentFileToWorkspace( MyConfig.AboutPageIconPage, workspace );

			if ( MyConfig.AboutPageInfo.Length > 0 )			
				namedUrlIndex.AboutPageInfo = ImportContentFileToWorkspace( MyConfig.AboutPageInfo, workspace );

			if ( MyConfig.EmptyIndexTermPage.Length > 0 )			
				namedUrlIndex.EmptyIndexTerm = ImportContentFileToWorkspace( MyConfig.EmptyIndexTermPage, workspace );

			if ( MyConfig.NavFailPage.Length > 0 )			
				namedUrlIndex.NavFailPage = ImportContentFileToWorkspace( MyConfig.NavFailPage, workspace );
			
			if ( MyConfig.AdditionalContentResourceDirectory.Length > 0 )			
				workspace.ImportContentDirectory( MyConfig.AdditionalContentResourceDirectory );

			namedUrlIndex.Save( workspace.WorkingDirectory );
		}

		private static string ImportContentFileToWorkspace( string path, Workspace workspace )
		{
			string fileName = Path.GetFileName( path );
			workspace.ImportContent( Path.GetDirectoryName( path ), fileName );
			return Path.Combine( workspace.ContentDirectoryName, fileName );
		}


		private ProjectFile CreateProjectFile( Workspace workspace )
		{
			// create a project file from the resource template
			ProjectFile project = ProjectFile.CreateFrom( Path.Combine( workspace.ResourceDirectory, "project.HxC" ), MyConfig.HtmlHelpName );
			
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

		private void MakeHtml( Workspace workspace, HtmlFactory factory )
		{
			// load the stylesheets
			OnDocBuildingStep( 20, "Loading StyleSheets..." );

			if ( MyConfig.ExtensibilityStylesheet.Length > 0 )
				factory.LoadStylesheets( MyConfig.ExtensibilityStylesheet, workspace.ResourceDirectory );

			else
				factory.LoadStylesheets( workspace.ResourceDirectory );

			OnDocBuildingStep( 30, "Generating HTML..." );

			if ( MyConfig.UseHelpNamespaceMappingFile.Length != 0 )
				factory.SetNamespaceMap( MyConfig.UseHelpNamespaceMappingFile );

			// add properties to the factory - these get passed to the stylesheets
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
			switch ( MyConfig.SdkDocVersion )
			{
				case SdkVersion.SDK_v1_1:	return "1.1";
				case SdkVersion.SDK_v1_0:	return "1.0";
				default:						return "";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		protected override void AddDocumenterSpecificXmlData(XmlWriter writer)
		{
			// add the default docset for this help title
			AddDocSet( writer, MyConfig.HtmlHelpName );

			// also add the items from the custom docset list
			foreach ( string s in MyConfig.DocSetList.Split( new char [] { ',' } ) )
				AddDocSet( writer, s );
		}

		private void AddDocSet(XmlWriter writer, string id )
		{
			if ( id.Length > 0 )
			{
				writer.WriteStartElement( "docSet" );
				writer.WriteString(id.Trim());
				writer.WriteEndElement();
			}
		}

		private void UnPackResources( Workspace workspace )
		{
#if NO_RESOURCES
			// copy all of the xslt source files into the workspace
			DirectoryInfo xsltSource = new DirectoryInfo( Path.GetFullPath(Path.Combine(
				System.Windows.Forms.Application.StartupPath, @"..\..\..\Documenter\NativeHtmlHelp2\xslt") ) );
                				
			foreach ( FileInfo f in xsltSource.GetFiles( "*.xslt" ) )
			{
				string fname = Path.Combine( workspace.ResourceDirectory, f.Name );
				f.CopyTo( fname, true );
				File.SetAttributes( fname, FileAttributes.Normal );
			}
#else
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.xslt",
				workspace.ResourceDirectory );
#endif

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.includes",
				workspace.WorkingDirectory );

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.graphics",
				workspace.ContentDirectory );

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.HxProject.HelpTitle",
				workspace.ResourceDirectory );

			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.HxProject.HelpCollection",
				workspace.ResourceDirectory );
			
			EmbeddedResources.WriteEmbeddedResources(
				this.GetType().Module.Assembly,
				"NDoc.Documenter.NativeHtmlHelp2.Engine.NamespaceMapping",
				workspace.WorkingDirectory );
		}

		private void CreateCollectionFiles( Workspace workspace )
		{
			string collectionName = MyConfig.HtmlHelpName + "Collection";

			// add the collection table of contents
			CollectionTOCFile toc = CollectionTOCFile.CreateFrom( 
				Path.Combine( workspace.ResourceDirectory, "Collection.HxT" ), collectionName );

			toc.LangId = MyConfig.LangID;
			toc.Flat = MyConfig.CollectionTOCStyle == TOCStyle.Flat;
			toc.Title = MyConfig.Title;
			toc.BaseUrl = MyConfig.HtmlHelpName;
			toc.Save( workspace.RootDirectory );

			// add the collection file
			CollectionFile collection = CollectionFile.CreateFrom( 
				Path.Combine( workspace.ResourceDirectory, "Collection.HxC" ), collectionName );
			
			collection.LangId = MyConfig.LangID;
			collection.Copyright = MyConfig.CopyrightText;
			collection.FileVersion = MyConfig.Version;
			collection.Title = MyConfig.Title;
			collection.TOCFile = toc.FileName;

			AddIndexToCollection( workspace, collection, "Collection_A.HxK", collectionName + "_A" );
			AddIndexToCollection( workspace, collection, "Collection_F.HxK", collectionName + "_F" );
			AddIndexToCollection( workspace, collection, "Collection_K.HxK", collectionName + "_K" );
			
			collection.Save( workspace.RootDirectory );
			
			// create and save the H2reg ini file
			H2RegFile h2reg = new H2RegFile( collectionName );
			h2reg.LangId = MyConfig.LangID;
			h2reg.CollectionFileName = collection.FileName;
			h2reg.Description = MyConfig.Title;
			h2reg.PluginNamespace = MyConfig.PlugInNamespace;
			h2reg.SetDocSetFilter( MyConfig.Title, MyConfig.HtmlHelpName );

			if ( MyConfig.BuildSeparateIndexFile )
				h2reg.AddTitle( MyConfig.HtmlHelpName, MyConfig.LangID, MyConfig.HtmlHelpName + ".HxS", MyConfig.HtmlHelpName + ".HxI" );
			else
				h2reg.AddTitle( MyConfig.HtmlHelpName, MyConfig.LangID, MyConfig.HtmlHelpName + ".HxS" );

			h2reg.Save( workspace.RootDirectory );
		}

		private void AddIndexToCollection( Workspace workspace, CollectionFile collection, string templateName, string fileName )
		{
			IndexFile index = IndexFile.CreateFrom( 
				Path.Combine( workspace.ResourceDirectory, templateName ), fileName );
			index.LangId = collection.LangId;
			collection.AddKeywordIndex( index.FileName );
			index.Save( workspace.RootDirectory );
		}
	
		private void RegisterTitleAsCollection( Workspace workspace )
		{
			string ns = MyConfig.HtmlHelpName;

			if ( ns.Length > 0 )
			{
				HxReg reg = new HxReg();

				FileInfo f = new FileInfo( Path.Combine( workspace.RootDirectory, MyConfig.HtmlHelpName + ".Hxs" ) );
				reg.RegisterNamespace( ns, f, MyConfig.Title );
				reg.RegisterTitle( ns, ns, f );
			}
		}

		private void RegisterTitleWithCollection( Workspace workspace )
		{
			string ns = MyConfig.CollectionNamespace;

			if ( ns.Length > 0 )
			{
				HxReg reg = new HxReg();

				reg.RegisterTitle( ns, MyConfig.HtmlHelpName, new FileInfo( Path.Combine( workspace.RootDirectory, MyConfig.HtmlHelpName + ".Hxs" ) ) );
			}
		}

		private void CompileHxCFile( Workspace workspace )
		{
			try
			{
				Trace.WriteLine( "Compiling Html Help 2 file" );
#if DEBUG
				int start = Environment.TickCount;
#endif				
				HxCompiler compiler = new HxCompiler();
				compiler.Compile( new DirectoryInfo( workspace.WorkingDirectory ), MyConfig.HtmlHelpName, MyConfig.LangID );
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

					// if if we're registered in an external namespace open that one
					if ( MyConfig.RegisterTitleWithNamespace )
					{
						StartDexplore( MyConfig.CollectionNamespace  );
					}
					// otherwise the title is registered as a collection then use HtmlHelpName as the namesapce
					else if ( MyConfig.RegisterTitleAsCollection )
					{
						StartDexplore( MyConfig.HtmlHelpName  );
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

