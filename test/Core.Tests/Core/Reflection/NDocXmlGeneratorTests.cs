using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;

namespace NDoc3.Core.Reflection
{
	[TestFixture]
	public class NDocXmlGeneratorTests
	{
		[Test, Explicit]
		public void ProducesNDocXml()
		{
			NDocXmlGeneratorParameters args = new NDocXmlGeneratorParameters();
			AddAssemblyToDocument(typeof(GlobalAssembly1Class), args);
			AddAssemblyToDocument(typeof(GlobalAssembly2Class), args);
			args.DocumentInternals = true;
			args.UseNamespaceDocSummaries = true;

			NDocXmlGenerator ngen = new NDocXmlGenerator(new AssemblyLoader(), args);

			ngen.MakeXmlFile(new FileInfo("ndoc.test.xml"), Formatting.Indented, 2, ' ');

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(ngen.MakeXml());
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("ndoc", "urn:ndoc-schema");

			// copes w/ duplicate typenames in different assemblies
			Assert.AreEqual(2, doc.SelectNodes("//ndoc:namespace[@name='NDoc3.ReflectionTests.DuplicateNamespace']/ndoc:class[@name='DuplicateClass']", nsmgr).Count);
		}

		private void AddAssemblyToDocument(Type assemblyType, NDocXmlGeneratorParameters args)
		{
			string testAssembly1AssemblyFileName = new Uri(assemblyType.Assembly.CodeBase).AbsolutePath;
			string testAssembly1SlashDocFileName = testAssembly1AssemblyFileName.Substring(0, testAssembly1AssemblyFileName.Length - 4) + ".xml";
			args.AssemblyFileNames.Add(testAssembly1AssemblyFileName);
			args.XmlDocFileNames.Add(testAssembly1SlashDocFileName);
		}

		private class TestStringReader : StringReader
		{
			public bool CloseCalled;

			public TestStringReader(string s)
				: base(s)
			{ }

			public override void Close()
			{
				CloseCalled = true;
				base.Close();
			}
		}

		[Test]
		public void ValidateClosesReader()
		{
			TestStringReader sr = new TestStringReader("<ndoc xmlns='urn:ndoc-schema' SchemaVersion='1.3'><namespaceHierarchies /><assembly name='dummy'/></ndoc>");
			NDocXmlGenerator.ValidateNDocXml(sr);
			Assert.IsTrue(sr.CloseCalled);
		}

		[Test]
		public void ThrowsValidationExceptionOnInvalidNDocXml()
		{
			MockRepository mocks = new MockRepository();
			StringReader sr = new StringReader("<ndoc xmlns='urn:ndoc-schema' SchemaVersion='1.3' />");
			Assert.Throws<ValidationException>(
				() => NDocXmlGenerator.ValidateNDocXml(sr)
			);
		}
	}
}
