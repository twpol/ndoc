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
using System.Diagnostics;

using NDoc.Core;

using NDoc.Documenter.NativeHtmlHelp2.Compiler;
using NDoc.Documenter.NativeHtmlHelp2.HxProject;
using NDoc.Documenter.NativeHtmlHelp2.Engine;

namespace NDoc.Documenter.NativeHtmlHelp2
{
	/// <summary>Native Html Help 2 MSDN.Net documenter</summary>
	public class NativeHtmlHelp2Documenter : BaseDocumenter
	{

		/// <summary>Initializes a new instance of the NativeHtmlHelp2Documenter class.</summary>
		public NativeHtmlHelp2Documenter() : base( "Native HtmlHelp2" )
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

				TOCFile toc = TOCFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\project.HxT" ), MyConfig.HtmlHelpName );
				
				HxProject.TOCFile = toc.FileName;
				HxProject.Save( w.RootDirectory );

				// create and intialize a HtmlFactory
				ExternalHtmlProvider htmlProvider = new ExternalHtmlProvider( MyConfig.HeaderHtml, MyConfig.FooterHtml );
				HtmlFactory factory = new HtmlFactory( Path.Combine( this.WorkingPath, "html" ), htmlProvider );

				using( new TOCBuilder( toc, factory ) )
					MakeHtml( xmlDocumentation, factory );

				toc.Save( w.RootDirectory );

				//then compile the HxC into and HxS
				OnDocBuildingStep( 75, "Compiling Html Help 2 Files..." );
				CompileHxCFile();

				// do clean up and final registration steps
				OnDocBuildingStep( 99, "Finishing up..." );
				if ( MyConfig.RegisterTitleWithNamespace )
					RegisterTitle();
				else if ( MyConfig.RegisterTitleAsCollection )
					RegisterCollection();
			}
			catch ( Exception e )
			{
				throw new DocumenterException( "An error occured while creating the documentation", e );
			}
		}

		private ProjectFile CreateProjectFile()
		{
			// create a project file from the resource template
			ProjectFile project = ProjectFile.CreateFrom( Path.Combine( ResourceDirectory, @"HxProject\project.HxC" ), MyConfig.HtmlHelpName );
			
			// set it up
			project.BuildSeperateIndexFile = MyConfig.BuildSeperateIndexFile;
			project.Copyright = MyConfig.CopyrightText;
			project.CreateFullTextIndex = MyConfig.CreateFullTextIndex;
			project.FileVersion = MyConfig.Version;
			project.LangId = MyConfig.LangID;
			project.Title = MyConfig.Title;

			if ( MyConfig.IncludeDefaultStopWordList )
				project.StopWordFile = "FTSstop_" + MyConfig.CharacterSet.ToString() + ".stp";
			else
				project.StopWordFile = "";

			return project;
		}

		private void MakeHtml( XmlNode xmlDocumentation, HtmlFactory factory )
		{
			// load the stylesheets
			OnDocBuildingStep( 15, "Loading StyleSheets..." );
			factory.LoadStylesheets( ResourceDirectory );

			OnDocBuildingStep( 20, "Generating HTML..." );

			// add properties to the factory
			// these get passed to the stylesheets
			factory.Properties.Add( "ndoc-title", MyConfig.Title );
			factory.Properties.Add( "ndoc-vb-syntax", true );
			factory.Properties.Add( "ndoc-omit-object-tags", true );
			factory.Properties.Add( "ndoc-document-attributes", MyConfig.DocumentAttributes );
			factory.Properties.Add( "ndoc-documented-attributes", MyConfig.DocumentedAttributes );

			// make the html
			factory.MakeHtml( xmlDocumentation, MyConfig.LinkToSdkDocVersion, MyConfig.IncludeHierarchy );;
		}

		private XmlDocument MergeXml( Project project )
		{
			// Let the Documenter base class do it's thing.
			// Load the XML documentation into a DOM.
			XmlDocument xmlDocumentation = new XmlDocument();
			xmlDocumentation.LoadXml( MakeXml( project ) );

			XmlNodeList typeNodes = xmlDocumentation.SelectNodes("/ndoc/assembly/module/namespace/*[name()!='documentation']");
			if ( typeNodes.Count == 0 )			
				throw new DocumenterException("There are no documentable types in this project.");

			return xmlDocumentation;
		}

		private void PrepareWorkspace( Workspace w )
		{
			w.Clean();
			w.ImportContent( Path.Combine( ResourceDirectory, "graphics" ) );
			w.ImportProjectFiles( Path.Combine( ResourceDirectory, "includes" ) );
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
				"NDoc.Documenter.NativeHtmlHelp2.HxProject",
				Path.Combine( ResourceDirectory, "HxProject") );
		}

		private void RegisterCollection()
		{
			string ns = MyConfig.HtmlHelpName;

			if ( ns.Length > 0 )
			{
				HxReg reg = new HxReg();
				reg.RegisterNamespace( ns, new FileInfo( Path.Combine( WorkingPath, MyConfig.HtmlHelpName + ".Hxs" ) ), MyConfig.Title );
				reg.RegisterTitle( ns, ns, new FileInfo( Path.Combine( WorkingPath, MyConfig.HtmlHelpName + ".Hxs" ) ) );
			}
		}

		private void RegisterTitle()
		{
			string ns = MyConfig.ParentCollectionNamespace;

			if ( ns.Length > 0 )
			{
				HxReg reg = new HxReg();
				reg.RegisterTitle( ns, MyConfig.HtmlHelpName, new FileInfo( Path.Combine( WorkingPath, MyConfig.HtmlHelpName + ".Hxs" ) ) );
			}
		}

		private void CompileHxCFile()
		{
			try
			{
				HxCompiler compiler = new HxCompiler();
				compiler.Compile( new DirectoryInfo( WorkingPath ), MyConfig.HtmlHelpName, MyConfig.LangID );
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

