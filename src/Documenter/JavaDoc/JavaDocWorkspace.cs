using NDoc3.Core;

namespace NDoc3.Documenter.JavaDoc {
	/// <summary>
	/// Summary description for LatexWorkspace.
	/// </summary>
	public class JavaDocWorkspace : Workspace {
		/// <summary>
		/// Manages the location of the documentation build process
		/// </summary>
		/// <param name="rootDir">The location to create the workspace</param>
		public JavaDocWorkspace(string rootDir)
			: base(rootDir, "javadoc", ".", "") {

		}
	}
}
