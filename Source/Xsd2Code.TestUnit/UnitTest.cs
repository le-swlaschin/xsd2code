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
        private string NameSpace = "Xsd2Code.TestUnit";
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
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty, false, string.Empty, string.Empty, string.Empty, string.Empty, false);

            if (!xsdGen.ProcessCodeGeneration(out OutputFileName, out ErrorMessage))
            {
                Assert.Fail(ErrorMessage);
            }
        }

        [TestMethod]
        public void StackOverFlow()
        {
            // Get the code namespace for the schema.
            using (StreamWriter sw = new StreamWriter(DirOutput + "StackOverFlow.xsd", false))
            {
                sw.Write(Xsd2Code.TestUnit.Properties.Resources.StackOverFlow);
            }

            string FileName = DirOutput + "StackOverFlow.xsd";
            string ErrorMessage = "";
            string OutputFileName = "";
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty, false, string.Empty, string.Empty, string.Empty, string.Empty, false);

            if (!xsdGen.ProcessCodeGeneration(out OutputFileName, out ErrorMessage))
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
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty, true, "Serialize", "Deserialize", "SaveToFile", "LoadFromFile", false);
            string OutputFileName;
            if (!xsdGen.ProcessCodeGeneration(out OutputFileName, out ErrorMessage))
            {
                Assert.Fail(ErrorMessage);
            }
        }

        [TestMethod]
        public void AlowDebug()
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
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty, true, "Serialize", "Deserialize", "SaveToFile", "LoadFromFile", true);
            string OutputFileName;
            if (!xsdGen.ProcessCodeGeneration(out OutputFileName, out ErrorMessage))
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
            GeneratorFacade xsdGen = new GeneratorFacade(FileName, NameSpace, GenerationLanguage.CSharp, CollectionType.List, true, true, true, string.Empty, string.Empty, false, string.Empty, string.Empty, string.Empty, string.Empty, false);
            if (!xsdGen.ProcessCodeGeneration(out OutputFileName, out ErrorMessage))
            {
                Assert.Fail(ErrorMessage);
            }
        }
        
        [TestMethod]
        
        public void Serialize()
        {
            DvdCollection dvdCol = GetDvd();
            string dvdColStr1 = dvdCol.Serialize();

            DvdCollection dvdColFromXml;
            Exception exception;
            bool sucess = DvdCollection.Deserialize(dvdColStr1, out dvdColFromXml, out exception);
            if (sucess)
            {
                string dvdColStr2 = dvdColFromXml.Serialize();
                if (!dvdColStr1.Equals(dvdColStr2))
                {
                    Assert.Fail("dvdColFromXml is not equal after Deserialize");
                }
            }
            else
            {
                Assert.Fail(exception.Message);
            }
        }

        [TestMethod]
        public void Persistent()
        {
            DvdCollection dvdCol = GetDvd();
            Exception exception;
            if (!dvdCol.SaveToFile(@"c:\temp\savedvd.xml", out exception))
            {
                Assert.Fail(string.Format("Failed to save file. {0}", exception.Message));
            }

            DvdCollection loadedDvdCollection;
            Exception e;
            if (!DvdCollection.LoadFromFile(@"c:\temp\savedvd.xml", out loadedDvdCollection, out e))
            {
                Assert.Fail(string.Format("Failed to load file. {0}", e.Message));
            }

            string xmlBegin = dvdCol.Serialize();
            string xmlEnd = loadedDvdCollection.Serialize();

            if (!xmlBegin.Equals(xmlEnd))
            {
                Assert.Fail(string.Format("xmlBegin and xmlEnd are not equal after LoadFromFile"));
            }
        }
        
        [TestMethod]
        public void InvalidLoadFromFile()
        {
            DvdCollection loadedDvdCollection;
            Exception e;
            DvdCollection.LoadFromFile(@"c:\tempo\savedvd.xml", out loadedDvdCollection, out e);
        }
        
        private static DvdCollection GetDvd()
        {
            DvdCollection dvdCol = new DvdCollection();
            dvd newdvd = new dvd();
            newdvd.Title = "Matrix";
            newdvd.Style = Styles.Action;
            newdvd.Actor.Add(new Actor { firstname = "Thomas", lastname = "Anderson" });
            dvdCol.Dvds.Add(newdvd);
            return dvdCol;
        }
    }
}
