using System;
using System.IO;
using System.Text;
using System.Xml;
using NDoc3.Core.Reflection;
using NUnit.Framework;

namespace NDoc3.Core
{
	[TestFixture]
	public class ReflectionEngineTests
	{
		[Test, Explicit]
		public void GenerateTestNDocXml()
		{
			Uri uri = new Uri(this.GetType().Assembly.CodeBase);
			FileInfo assemblyFilename = new FileInfo(uri.AbsolutePath);
//			string slashdocFilename = assemblyFilename.Substring(0, assemblyFilename.Length - 4) + ".xml";

			ReflectionEngine re = new ReflectionEngine(null);

			XmlTextWriter xmlWriter = new XmlTextWriter("test.ndoc.xml", Encoding.UTF8);
			xmlWriter.Indentation = 2;
			xmlWriter.IndentChar = ' ';
			xmlWriter.Formatting = Formatting.Indented;

			using (xmlWriter) {
				NDocXmlGeneratorParameters args = new NDocXmlGeneratorParameters();
				args.AddAssemblyToDocument(assemblyFilename);

				string xml = ReflectionEngine.MakeXml(args);
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);
				doc.Save(xmlWriter);
			}
		}

		[Test]
		public void GeneratesValidNDocXml()
		{

		}
	}
}
