using System;

namespace NDoc.Test.Extensibility
{
	/// <summary>
	/// This namespace is used to test the extensibility feature
	/// see <a href="extend-ndoc.xslt"/>
	/// </summary>
	public class NamespaceDoc {}

	/// <summary>
	/// When processed by the VS.NET or MSDN documenters, using the stylesheet "extend-ndoc.xslt"
	/// as the ExtensibilityStylesheet property will result in end-user defined tags
	/// being displayed in the final help output topics
	/// </summary>
	/// <custom>This is a custom tag</custom>
	/// <mySeeAlso>This should appear in the "See Also" section</mySeeAlso>
	public class ABunchOfCustomTags
	{

	}
}
