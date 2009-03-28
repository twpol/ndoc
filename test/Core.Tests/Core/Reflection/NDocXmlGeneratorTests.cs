using NUnit.Framework;

namespace NDoc3.Core.Reflection
{
	[TestFixture]
	public class NDocXmlGeneratorTests
	{
		[Test, Ignore]
		public void ValidateClosesReader()
		{
			NDocXmlGeneratorParameters args = new NDocXmlGeneratorParameters();
			NDocXmlGenerator ngen = new NDocXmlGenerator(new AssemblyLoader(), null);
		}
	}
}
