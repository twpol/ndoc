using NDoc3.Core;

namespace NDoc3.Documenter.Latex
{
	/// <summary>
	/// Information about the Xml Documenter
	/// </summary>
	public class LatexDocumenterInfo : BaseDocumenterInfo
	{
		/// <summary>
		/// Creates a new instance of the class
		/// </summary>
		public LatexDocumenterInfo() : base( "LaTeX", DocumenterDevelopmentStatus.Alpha )
		{
		}

		/// <summary>
		/// See <see cref="IDocumenterInfo.CreateConfig()"/>
		/// </summary>
		/// <returns>A config instance</returns>
		public override IDocumenterConfig CreateConfig()
		{
			return new LatexDocumenterConfig( this );
		}
	}
}