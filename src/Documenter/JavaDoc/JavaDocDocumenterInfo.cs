using System;

using NDoc3.Core;

namespace NDoc3.Documenter.JavaDoc
{
	/// <summary>
	/// Information about the Xml Documenter
	/// </summary>
	public class JavaDocDocumenterInfo : BaseDocumenterInfo
	{
		/// <summary>
		/// Creates a new instance of the class
		/// </summary>
		public JavaDocDocumenterInfo() : base( "JavaDoc" )
		{
		}

		/// <summary>
		/// See <see cref="IDocumenterInfo.CreateConfig()"/>
		/// </summary>
		/// <returns>A config instance</returns>
		public override IDocumenterConfig CreateConfig()
		{
			return new JavaDocDocumenterConfig( this );
		}
	}
}