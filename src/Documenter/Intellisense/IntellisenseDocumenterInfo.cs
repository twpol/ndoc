using System;

using NDoc3.Core;

namespace NDoc3.Documenter.Intellisense
{
	/// <summary>
	/// Information about the Documenter
	/// </summary>
	public class IntellisenseDocumenterInfo : BaseDocumenterInfo
	{
		/// <summary>
		/// Creates a new instance of the class
		/// </summary>
		public IntellisenseDocumenterInfo() : base( "Intellisense" )
		{
		}

		/// <summary>
		/// See <see cref="IDocumenterInfo.CreateConfig()"/>
		/// </summary>
		/// <returns>A config instance</returns>
		public override IDocumenterConfig CreateConfig()
		{
			return new IntellisenseDocumenterConfig( this );
		}
	}
}