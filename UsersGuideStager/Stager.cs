using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace NDoc.UsersGuideStager
{
	/// <summary>
	/// Summary description for Translator.
	/// </summary>
	public class Stager
	{
		private string _hhcPath;

		public Stager( string hhc )
		{
			if ( !File.Exists( hhc ) )
				throw new FileNotFoundException( "File not found", hhc );

			_hhcPath = hhc;
		}

		public void Stage()
		{
			string stagingDir = Path.Combine( Path.GetDirectoryName( _hhcPath ), "online_staging" );

			if ( Directory.Exists( stagingDir ) )
				Directory.Delete( stagingDir, true );

			Directory.CreateDirectory( stagingDir );

			StageSupportFiles( stagingDir );
			StageTocFile( stagingDir );
			StageContent( stagingDir );
		}

		private void StageContent( string stagingDir )
		{
			DirectoryInfo sourceContentDir = new DirectoryInfo( Path.Combine( Path.GetDirectoryName( _hhcPath ), "content" ) );
			Debug.Assert( sourceContentDir.Exists );

			StageDirectory( sourceContentDir, new DirectoryInfo( stagingDir ) );
		}

		private void StageFile( FileInfo file, DirectoryInfo targetDir )
		{
			if ( file.Name.ToLower() != ".cvsignore" )
			{
				if ( file.Extension == ".htm" || file.Extension == ".html" )
					HtmlTransform.Transform( file, targetDir );
				else
					file.CopyTo( Path.Combine( targetDir.FullName.ToLower(), file.Name.ToLower() ) );
			}
		}

		private void StageDirectory( DirectoryInfo sourceDir, DirectoryInfo targetRoot )
		{
			// strip out cvs and frontpage directories
			if ( sourceDir.Name.ToLower() != "cvs" && sourceDir.Name.ToLower().IndexOf( "_vti_" ) == -1 && sourceDir.Name.ToLower().IndexOf( "_private" ) == -1 )
			{
				DirectoryInfo targetDir = new DirectoryInfo( Path.Combine( targetRoot.FullName, sourceDir.Name.ToLower() ) );
				targetDir.Create();

				foreach ( FileInfo file in sourceDir.GetFiles() )
					StageFile( file, targetDir );

				foreach( DirectoryInfo subDir in sourceDir.GetDirectories() )
					StageDirectory( subDir, targetDir );						
			}
		}

		private void StageSupportFiles( string stagingDir )
		{
			EmbeddedResources.WriteEmbeddedResources( Assembly.GetExecutingAssembly(), "NDoc.UsersGuideStager.html", stagingDir );
		}

		private void StageTocFile( string stagingDir )
		{
			HHCTranslator hhc = new HHCTranslator( _hhcPath );
			hhc.Translate( Path.Combine( stagingDir, "toc.html" ) );
		}
	}
}
