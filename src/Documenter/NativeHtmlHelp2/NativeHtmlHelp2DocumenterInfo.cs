using System;

using NDoc3.Core;

namespace NDoc3.Documenter.NativeHtmlHelp2
{
	/// <summary>
	/// Information about the Xml Documenter
	/// </summary>
	public class NativeHtmlHelp2DocumenterInfo : BaseDocumenterInfo
	{
		/// <summary>
		/// Creates a new instance of the class
		/// </summary>
		public NativeHtmlHelp2DocumenterInfo() : base( "VS.NET 2003" )
		{
		}

		/// <summary>
		/// See <see cref="IDocumenterInfo.CreateConfig(NDoc3.Core.Project)"/>
		/// </summary>
		/// <returns>A config instance</returns>
		public override IDocumenterConfig CreateConfig()
		{
			return new NativeHtmlHelp2Config( this );
		}
	}
}