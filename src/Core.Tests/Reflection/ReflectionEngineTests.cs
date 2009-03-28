using System;
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
            string assemblyFilename = uri.AbsolutePath;
            string slashdocFilename = assemblyFilename.Substring(0, assemblyFilename.Length - 4) + ".xml";

            ReflectionEngine re = new ReflectionEngine(null);

            XmlTextWriter xmlWriter = new XmlTextWriter("test.ndoc.xml", Encoding.UTF8);
#if DEBUG
            xmlWriter.Indentation = 2;
            xmlWriter.IndentChar = ' ';
            xmlWriter.Formatting = Formatting.Indented;
#endif
            using (xmlWriter)
            {
                NDocXmlGeneratorParameters args = new NDocXmlGeneratorParameters();
                args.AssemblyFileNames.Add(assemblyFilename);
                args.XmlDocFileNames.Add(slashdocFilename);

                string xml = re.MakeXml(args);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                doc.Save(xmlWriter);
        }
    }
}
