using System;
using System.IO;
using System.Text;
using System.Diagnostics;

using MSHelpCompiler;

using NDoc.Core;
using NDoc.Documenter.Msdn;

using NDoc.Documenter.HtmlHelp2.Compiler;

namespace NDoc.Documenter.HtmlHelp2
{
	/// <summary>CHM to HxS converter/compiler</summary>
	public class HtmlHelp2Documenter : BaseDocumenter
	{

		private MsdnDocumenter _HtmlHelp1Documenter = null;

		/// <summary>Initializes a new instance of the HtmlHelp2 class.</summary>
		public HtmlHelp2Documenter() : base( "HtmlHelp2" )
		{
			Clear();
		}


		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Clear()
		{
			//create a new instance of our config settings
			Config = new HtmlHelp2Config();

			//create new instance of the CHM documenter
			_HtmlHelp1Documenter = new MsdnDocumenter();

			//our settings inherit from the Msdn document config
			//so pass our settings to the Msdh documenter
			_HtmlHelp1Documenter.Config = Config;
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string MainOutputFile 
		{ 
			get 
			{
				if ((MyConfig.OutputTarget & OutputType.HtmlHelp) > 0)
				{
					return Path.Combine( MyConfig.OutputDirectory, MyConfig.HtmlHelpName + ".Hxs");
				}
				else
				{
					return Path.Combine( MyConfig.OutputDirectory, "index.html" );
				}
			} 			
		} 


		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override string CanBuild(Project project, bool checkInputOnly)
		{
			string result = base.CanBuild(project, checkInputOnly); 
			if (result != null)
			{
				return result;
			}
			
			result = _HtmlHelp1Documenter.CanBuild( project, checkInputOnly );
			if (result != null)
			{
				return result;
			}	
	
			return null;
		}
		/// <summary>
		/// The development status (alpha, beta, stable) of this documenter.
		/// </summary>
		public override DocumenterDevelopmentStatus DevelopmentStatus
		{
			get { return(DocumenterDevelopmentStatus.Alpha); }
		}

		/// <summary>See <see cref="IDocumenter"/>.</summary>
		public override void Build(Project project)
		{
			//set the config such that the MSDN compiler only creates the CHM
			//if the user wants the web output they certainly don't need this component
			MyConfig.OutputTarget = OutputType.HtmlHelp;

			//first build the CHM file
			OnDocBuildingStep( 0, "Building Html Help 1..." );
			_HtmlHelp1Documenter.DocBuildingStep += new DocBuildingEventHandler( _HtmlHelp1Documenter_DocBuildingStep );
			_HtmlHelp1Documenter.Build( project );

			string outputDir = MyConfig.OutputDirectory;

			//then get rid of all help 1 inputs
			OnDocBuildingStep( 50, "Cleaning Html 1 intermediate files..." );
			CleanCHMIntermediates();

			//then convert the CHM to an HxC project
			OnDocBuildingStep( 55, "Converting to Html Help 2..." );
			ConvertCHMFile();

			//then compile the HxC into and HxS
			OnDocBuildingStep( 60, "Compiling Html Help 2 Files..." );
			CompileHxCFile();

			if ( MyConfig.DeleteCHM )
				File.Delete( InputCHMPath );
		}

		private void _HtmlHelp1Documenter_DocBuildingStep(object sender, ProgressArgs e)
		{
			// since the CHM compilation takes awhile
			// let's proxy its progress back to the user

			// (since it takes 50% of our progress divide its progress by 2)
			OnDocBuildingStep( e.Progress / 2, e.Status );
		}

		private void CleanCHMIntermediates()
		{
			DirectoryInfo dir = new DirectoryInfo( WorkingPath );

			foreach ( FileInfo file in dir.GetFiles() )
			{
				switch ( file.Extension.ToLower() )
				{
					case ".chm":	//leave teh CHM
						break;
					case ".log":	//leave the log files
						break;
					default:				
						file.Delete();
						break;
				}
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

		private void ConvertCHMFile()
		{
			try
			{
				//convert the CHM into an Hx* project file
				HxConv converter = new HxConv( MyConfig.HtmlHelp2CompilerPath );
				converter.Convert( new FileInfo( InputCHMPath ) );
			}
			catch ( Exception e )
			{
				throw new DocumenterException( "CHM conversion error", e );
			}
		}

		private string WorkingPath{ get{ return MyConfig.OutputDirectory; } }
		
		private string InputCHMPath{ get{ return Path.Combine( MyConfig.OutputDirectory, MyConfig.HtmlHelpName ) + ".chm";} }

		private HtmlHelp2Config MyConfig{ get{ return (HtmlHelp2Config)Config; } }
	}
}

