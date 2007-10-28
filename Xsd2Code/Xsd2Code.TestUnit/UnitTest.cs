using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xsd2Code.Library;

namespace Xsd2Code.TestUnit
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest
    {
        private readonly string NameSpace = "XSDCodeGen";
        private readonly string DirOutput = "";

        [TestMethod]
        public void XSDCodeGen()
        {
            // Get the code namespace for the schema.

            using (StreamWriter sw = new StreamWriter(DirOutput+"dvd.xsd", false))
            {
                sw.Write(Xsd2Code.TestUnit.Properties.Resources.dvd);
            }

            CodeNamespace ns = Processor.Process(DirOutput + "dvd.xsd", NameSpace);
            CodeDomProvider provider;

            provider = new Microsoft.CSharp.CSharpCodeProvider();

            using (StreamWriter sw = new StreamWriter(DirOutput+"dvd.cs", false))
            {
                provider.GenerateCodeFromNamespace(ns, sw, new CodeGeneratorOptions());
            }
        }
    }
}
