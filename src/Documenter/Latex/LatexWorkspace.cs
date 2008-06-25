using System;

using NDoc3.Core;

namespace NDoc3.Documenter.Latex
{
	/// <summary>
	/// Summary description for LatexWorkspace.
	/// </summary>
	public class LatexWorkspace : Workspace
	{
		/// <summary>
		/// Manages the location of the documentation build process
		/// </summary>
		/// <param name="rootDir">The location to create the workspace</param>
		public LatexWorkspace( string rootDir ) : base( rootDir, "latex", ".", "" )
		{

		}
	}
}
