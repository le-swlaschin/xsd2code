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
using Xsd2Code.Library;

namespace Xsd2Code.TestUnit
{
    /// <summary>
    /// Summary description for UnitTest1 (איû)
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
        public void Circular()
        {
            // Get the code namespace for the schema.
            using (StreamWriter sw = new StreamWriter(DirOutput + "Circular.xsd", false))
            {
                sw.Write(Xsd2Code.TestUnit.Properties.Resources.Circular);
            }

            string FileName = DirOutput + "Circular.xsd";
            string ErrorMessage = "";
            string OutputFileName = "";
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty);
            
            if (!xsdGen.Process(out OutputFileName, out ErrorMessage))
            {
                Assert.Fail(ErrorMessage);
            }
        }

        [TestMethod]
        public void Dvd()
        {
            // Get the code namespace for the schema.
            using (StreamWriter sw = new StreamWriter(DirOutput + "Dvd.xsd", false))
            {
                sw.Write(Xsd2Code.TestUnit.Properties.Resources.dvd);
            }

            // Get the code namespace for the schema.

            using (StreamWriter sw = new StreamWriter(DirOutput + "Actor.xsd", false))
            {
                sw.Write(Xsd2Code.TestUnit.Properties.Resources.Actor);
            }

            string FileName = DirOutput + "Dvd.xsd";
            string ErrorMessage = "";
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty);
            string OutputFileName;
            if (!xsdGen.Process(out OutputFileName, out ErrorMessage))
            {
                Assert.Fail(ErrorMessage);
            }
        }
        [TestMethod]
        public void Hierarchical()
        {
            // Get the code namespace for the schema.
            using (StreamWriter sw = new StreamWriter(DirOutput + "Hierarchical.xsd", false))
            {
                sw.Write(Xsd2Code.TestUnit.Properties.Resources.Hierarchical);
            }

            string FileName = DirOutput + "Hierarchical.xsd";
            string ErrorMessage = "";
            string OutputFileName = "";
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty);
            if (!xsdGen.Process(out OutputFileName, out ErrorMessage))
            {
                Assert.Fail(ErrorMessage);
            }
        }
    }
}
