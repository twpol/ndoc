using NDoc3.Core;

namespace NDoc3.Documenter.Msdn {
	/// <summary>
	/// Summary description for MsdnWorkspace.
	/// </summary>
	public class MsdnWorkspace : Workspace {
		private readonly bool _cleanIntermediates;

		/// <summary>
		/// Contructs a new instance of the MsdnWorkspace class
		/// </summary>
		/// <param name="rootDir">The location to create the workspace</param>
		/// <param name="cleanIntermediates">whether to clean intermediate files on dispose</param>
		public MsdnWorkspace(string rootDir, bool cleanIntermediates)
			: base(rootDir, "msdn", ".", "*.chm") {
			_cleanIntermediates = cleanIntermediates;
		}

		public override void Dispose() {
			if (_cleanIntermediates) {
				CleanIntermediates();
			}
			base.Dispose();
		}
	}
}
