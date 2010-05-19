using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Rhino.Mocks;
using System.Reflection;

namespace NDoc3.Core.Reflection {
	[TestFixture]
	public class NDocXmlGeneratorTests {
		/// <summary>
		/// The reason for introducing ComparableFileInfo
		/// </summary>
		[Test, Explicit]
		public void FileInfoComparison() {
			FileInfo fi1 = new FileInfo("C:\\Test.xml");
			FileInfo fi2 = new FileInfo("c:\\tesT.xml");
			Assert.AreEqual(fi1, fi2, "FileInfo type's Equals() implementation doesn't fulfill expected behavior");
		}

		[Test]
		public void ProducesNDocXml() {
			NDocXmlGeneratorParameters args = new NDocXmlGeneratorParameters();
			args.AddAssemblyToDocument(typeof(GlobalAssembly1Class));
			args.AddAssemblyToDocument(typeof(GlobalAssembly2Class));
			args.DocumentInternals = true;
			args.UseNamespaceDocSummaries = true;

			NDocXmlGenerator ngen = new NDocXmlGenerator(new AssemblyLoader(), args);

			ngen.MakeXmlFile(new FileInfo("ndoc.test.xml"), Formatting.Indented, 2, ' ');

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(ngen.MakeXml());
			XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("ndoc", "urn:ndoc-schema");

			// copes w/ duplicate typenames in different assemblies
			XmlNodeList duplicateClassNodes = doc.SelectNodes("//ndoc:namespace[@name='NDoc3.ReflectionTests.DuplicateNamespace']/ndoc:class[@name='DuplicateClass']", nsmgr);
			Assert.AreEqual(2, duplicateClassNodes.Count);

			// TestAssembly2 references TestAssembly1
			XmlNodeList testAssembly1References = doc.SelectNodes("//ndoc:assembly[@name='NDoc3.Core.Tests.TestAssembly2']/ndoc:assemblyReference[@name='NDoc3.Core.Tests.TestAssembly1']", nsmgr);
			Assert.AreEqual(1, testAssembly1References.Count);
		}

		private class TestStringReader : StringReader {
			public bool CloseCalled;

			public TestStringReader(string s)
				: base(s) { }

			public override void Close() {
				CloseCalled = true;
				base.Close();
			}
		}

		[Test]
		public void ValidateClosesReader() {
			TestStringReader sr = new TestStringReader("<ndoc xmlns='urn:ndoc-schema' SchemaVersion='1.3'><namespaceHierarchies /><assembly name='dummy'/></ndoc>");
			NDocXmlGenerator.ValidateNDocXml(sr);
			Assert.IsTrue(sr.CloseCalled);
		}

		[Test]
		public void ThrowsValidationExceptionOnInvalidNDocXml() {
			MockRepository mocks = new MockRepository();
			StringReader sr = new StringReader("<ndoc xmlns='urn:ndoc-schema' SchemaVersion='1.3' />");
			Assert.Throws<ValidationException>(
				() => NDocXmlGenerator.ValidateNDocXml(sr)
			);
		}

		/// <summary>
		/// Test for bugticket #3003325, Constant string values aren't qouted correctly in documentation.
		/// Patch provided by Piotr Fusik (pfusik).
		/// </summary>
		[Test]
		public void TestConstantExpressionValue() {
			MethodInfo mi = typeof(NDocXmlGenerator).GetMethod("GetDisplayValue", BindingFlags.NonPublic | BindingFlags.Static);
			string value = mi.Invoke(null, new object[] { null, "\tHello\n\"World\"\\\033" }) as string;
			Assert.AreEqual("\"\\tHello\\n\\\"World\\\"\\\\\\033\"", value);
			value = mi.Invoke(null, new object[] { null, 33 }) as string;
			Assert.AreEqual("33", value);
		}
	}
}
