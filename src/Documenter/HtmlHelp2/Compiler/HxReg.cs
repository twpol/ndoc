using System;

namespace NDoc.Documenter.HtmlHelp2.Compiler
{
	/// <summary>
	/// Wraps the HxReg.exe registry component
	/// not sure whether to use the command line interface 
	/// or if the HxHelpServices API can be used to accomplish this
	/// </summary>
	public class HxReg : HxObject
	{
		/*
			Microsoft Help Help File Registration Tool Version 2.1.9466
			Copyright (c) 1999-2000 Microsoft Corp.

			Usage: HxReg [switches] | HxReg <Help filename .HxS>
				-n <namespace>
				-i <title ID>
				-c <collection Name .HxC | .HxS>
				-d <namespace description>
				-s <Help filename .HxS>
				-x <Help Index filename .HxI>
				-q <Help Collection Combined FTS filename .HxQ>
				-t <Help Collection Combined Attribute Index filename .HxR>
				-l <language ID>
				-a <alias>
				-f <filename listing HxReg commands>
				-r Remove a namespace, Help title, or alias

			EXAMPLES
			To register a namespace:
				HxReg -n <namespace> -c <collection filename> -d <namespace description>
			To register a Help file:
				HxReg -n <namespace> -i <title id> -s <HxS filename>		  
		*/

		/// <summary>
		/// Create a new instance of a HxReg object
		/// </summary>
		/// <param name="compilerPath"><see cref="HxObject.CompilerPath"/></param>
		public HxReg( string compilerPath ) : base( compilerPath, "HxReg.exe" )
		{
		}
	}
}
