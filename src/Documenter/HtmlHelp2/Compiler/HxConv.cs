using System;
using System.IO;
using System.Text;


namespace NDoc.Documenter.HtmlHelp2.Compiler
{
	/*
		HXCONV HTML Help 1.x to Microsoft Help 2.0 Converter v1.0
		Copyright (c) 2000 Microsoft Corp.

		Usage:
			HXCONV [options] <input file>

		Options:
			-o <output dir>    Specify an output directory
			-l <logfile>       Specify a log file
			-m <mapping file>  Specify a CHM-to-namespace mapping file
			-w                 Output Hx files as Unicode
			-u                 Output Hx files as UTF-8
			-y                 Automatically overwrite existing files
			-v                 Verbose mode: output all errors
			-q                 Quiet mode: output nothing
			-s                 Suppress running display of progress

			-?                 Display this help message
	*/

	/// <summary>
	/// Converts compiled Html Help version 1 CHM files into
	/// Compiled Html Help version 2 HxS files
	/// This class wraps the HxConv.exe converter supplied with the HTML v2 SDK
	/// </summary>
	public class HxConv : HxObject
	{

		/// <summary>
		/// Create new instance of a Chm2HxsConverter
		/// </summary>
		/// <param name="compilerPath"><see cref="HxObject.CompilerPath"/></param>
		public HxConv( string compilerPath ) : base( compilerPath, "HxConv.exe" )
		{
		}

		/// <summary>
		/// Converts the specified CHM files
		/// </summary>
		/// <param name="CHMFile">The CHM Help file to convert</param>
		public void Convert( FileInfo CHMFile )
		{
			Execute( GetArguments( CHMFile ), CHMFile.Directory.FullName );
		}

		private string GetArguments( FileInfo CHMFile )
		{
			StringBuilder ret = new StringBuilder();

			ret.Append( " -q " ); //quiet mode
			ret.Append( " -s " ); //suppress progress
			ret.Append( " -y " ); //overwrite files

			ret.Append( " -l HxConv.log " );	//make a log file

			if ( _CharacterSet == CharacterSet.UTF8 )
				ret.Append( " -u " );
			else if ( _CharacterSet == CharacterSet.Unicode )
				ret.Append( " -w " );

			//set the output directory where the converted files get placed
			//this is relarive to the location of the input CHM file
			ret.Append( " -o . " );//, HxObject.WorkingDirectoryName );

			ret.Append( '"' );
			ret.Append( CHMFile.FullName );
			ret.Append( '"' );

			return ret.ToString();
		}

		CharacterSet _CharacterSet = CharacterSet.Ascii;
		/// <summary>
		/// Gets or sets the character set that will be used when converting the CHM file.
		/// Defaults to Ascii.
		/// </summary>
		public CharacterSet CharacterSet
		{
			get{ return _CharacterSet; }
			set	{ _CharacterSet = value; }
		}
	}
}
