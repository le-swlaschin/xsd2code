using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Xsd2Code.TestUnit.Properties;

namespace Xsd2Code.TestUnit
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest
    {
        private string NameSpace = "XSDCodeGen";
        private string DirOutput = @"c:\temp\";

        public UnitTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

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
