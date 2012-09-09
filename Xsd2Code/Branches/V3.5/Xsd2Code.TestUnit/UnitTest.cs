using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xsd2Code.Library;
using Xsd2Code.Library.Helpers;
using Xsd2Code.TestUnit.Helpers;
using Xsd2Code.TestUnit.Properties;

namespace Xsd2Code.TestUnit
{
    /// <summary>
    /// Xsd2Code unit tests
    /// </summary>
    /// <remarks>
    /// Revision history:
    /// 
    ///     Modified 2009-02-25 by Ruslan Urban 
    ///     Performed code review
    ///     Changed output folder to the TestResults folder to preserve files in the testing history
    ///     TODO: Add tests that compile generated code
    /// 
    /// </remarks>
    [TestClass]
    public class UnitTest
    {
        private readonly object testLock = new object();
        static readonly object fileLock = new object();


        /// <summary>
        /// Output folder: TestResults folder relative to the solution root folder
        /// </summary>
        private static string OutputFolder
        {
            get { return @"c:\temp\"; } // Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\"; }
        }

        /// <summary>
        /// Code generation namespace  
        /// </summary>
        private const string CodeGenerationNamespace = "Xsd2Code.TestUnit";

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
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        /// <summary>
        /// Circulars this instance.
        /// </summary>
        [TestMethod]
        public void Circular()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("Circular.xsd", Resources.Circular);

                var xsdGen = new GeneratorFacade(GetGeneratorParams(inputFilePath));
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());
            }
        }

        /// <summary>
        /// Circulars this instance.
        /// </summary>
        [TestMethod]
        public void CircularClassReference()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("CircularClassReference.xsd", Resources.CircularClassReference);
                var generatorParams = new GeneratorParams
                                          {
                                              InputFilePath = inputFilePath,
                                              TargetFramework = TargetFramework.Net20,
                                              OutputFilePath = GetOutputFilePath(inputFilePath)

                                          };
                generatorParams.PropertyParams.AutomaticProperties = true;
                generatorParams.Serialization.Enabled = false;
                generatorParams.GenericBaseClass.Enabled = false;

                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());

                try
                {
                    var cs = new Circular();

#pragma warning disable 168
                    int count = cs.circular.count;
#pragma warning restore 168

                    Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
                    Build(generatorParams.OutputFilePath, TargetFramework.Net35);
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }
        }

        /// <summary>
        /// Arrays the of array.
        /// </summary>
        [TestMethod]
        public void ArrayOfArray()
        {
            lock (testLock)
            {

                // Copy resource file to the run-time directory
                var inputFilePath = GetInputFilePath("ArrayOfArray.xsd", Resources.ArrayOfArray);

                var generatorParams = new GeneratorParams
                                          {
                                              GenerateCloneMethod = true,
                                              InputFilePath = inputFilePath,
                                              NameSpace = "MyNameSpace",
                                              CollectionObjectType = CollectionType.Array,
                                              EnableDataBinding = true,
                                              Language = GenerationLanguage.CSharp,
                                              OutputFilePath = Path.ChangeExtension(inputFilePath, ".TestGenerated.cs")
                                          };
                generatorParams.PropertyParams.AutomaticProperties = true;
                generatorParams.Serialization.Enabled = true;
                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());
                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Stacks the over flow.
        /// </summary>
        [TestMethod]
        public void StackOverFlow()
        {
            lock (testLock)
            {

                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("StackOverFlow.xsd", Resources.StackOverFlow);

                var generatorParams = GetGeneratorParams(inputFilePath);
                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        [TestMethod]
        public void Deserialize_ArrayOfMyElement()
        {
            lock (testLock)
            {

                var e = new ArrayOfMyElement();
                var myE = new MyElement { Name = "Name" };
                myE.AttributeLists.Add(new NameValuePair { Name = "Name", Value = "Value" });
                e.MyElement.Add(myE);
                Exception ex;

                var serialized = e.Serialize();
                e.SaveToFile(Path.Combine(OutputFolder, "ReproSampleFile.xml"), out ex);
                if (ex != null) throw ex;

                //try to deserialize

                //generate doc conformant to schema

                ArrayOfMyElement toDeserialize;
                if (!ArrayOfMyElement.LoadFromFile("ReproSampleFile.xml", out toDeserialize, out ex))
                {
                    Console.WriteLine("Unable to deserialize, will exit");
                    return;
                }

                var serialized2 = toDeserialize.Serialize();

                Console.WriteLine("Still missing the <NameValuePairElement>");
                Console.WriteLine(serialized);

                Console.WriteLine("Name value pairs elements missing");
                Console.WriteLine(serialized2);
            }
        }

        /// <summary>
        /// DVDs this instance.
        /// </summary>
        [TestMethod]
        public void EncodingTest()
        {
            lock (testLock)
            {

                // Copy resource file to the run-time directory
                GetInputFilePath("Actor.xsd", Resources.Actor);

                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("Dvd.xsd", Resources.dvd);
                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.CollectionObjectType = CollectionType.List;
                generatorParams.TargetFramework = TargetFramework.Net35;
                generatorParams.EnableDataBinding = true;
                generatorParams.Miscellaneous.EnableSummaryComment = true;
                generatorParams.GenerateDataContracts = false;
                generatorParams.GenericBaseClass.Enabled = false;
                generatorParams.Serialization.GenerateXmlAttributes = true;
                generatorParams.TrackingChanges.Enabled = false;
                generatorParams.TrackingChanges.GenerateTrackingClasses = false;
                generatorParams.Serialization.EnableEncoding = true;
                generatorParams.Serialization.DefaultEncoder = DefaultEncoder.UTF8;
                generatorParams.Language = GenerationLanguage.CSharp;

                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());

                // Create new dvd collection and save it to file
                var dvd = new DvdCollection();
                dvd.Dvds.Add(new dvd { Title = "Matrix יא?" });
                var newitem = new dvd();
                newitem.Actor.Add(new Actor { firstname = "Jamיs א&", nationality = "Us" });
                dvd.Dvds.Add(newitem);
                var originalXml = dvd.Serialize();
                dvd.SaveToFile(@"c:\temp\dvd.xml");

                // Load data fom file and serialize it again.                                                                                                                                                               

                var loadedDvdCollection = DvdCollection.LoadFromFile(@"c:\temp\dvd.xml");
                var finalXml = loadedDvdCollection.Serialize();

                // Then comprate two xml string
                if (!originalXml.Equals(finalXml))
                {
                    Assert.Fail("Xml value are not equals");
                }
                Exception exp;
                DvdCollection deserialiseDvd;
                dvd.SaveToFile(@"c:\temp\dvdASCII.xml", Encoding.ASCII);
                if (!DvdCollection.LoadFromFile(@"c:\temp\dvdASCII.xml", Encoding.ASCII, out deserialiseDvd, out exp))
                {
                    Assert.Fail("LoadFromFile failed on ASCII encoding ");
                }
                else
                {
                    if (!deserialiseDvd.Dvds[0].Title.Equals("Matrix יא?"))
                    {
                        Assert.Fail("LoadFromFile failed on ASCII encoding ");
                    }
                }

                dvd.SaveToFile(@"c:\temp\dvdUTF8.xml", Encoding.UTF8);
                if (!DvdCollection.LoadFromFile(@"c:\temp\dvdUTF8.xml", Encoding.UTF8, out deserialiseDvd, out exp))
                {
                    Assert.Fail("LoadFromFile failed on UTF8 encoding ");
                }
                else
                {
                    if (!deserialiseDvd.Dvds[0].Title.Equals("Matrix יא?"))
                    {
                        Assert.Fail("LoadFromFile failed on UTF8 encoding ");
                    }
                }

                dvd.SaveToFile(@"c:\temp\dvdUnicode.xml", Encoding.Unicode);
                if (!DvdCollection.LoadFromFile(@"c:\temp\dvdUnicode.xml", Encoding.Unicode, out deserialiseDvd, out exp))
                {
                    Assert.Fail("LoadFromFile failed on Unicode encoding ");
                }
                else
                {
                    if (!deserialiseDvd.Dvds[0].Title.Equals("Matrix יא?"))
                    {
                        Assert.Fail("LoadFromFile failed on Unicode encoding ");
                    }
                }

                dvd.SaveToFile(@"c:\temp\dvdUTF32.xml", Encoding.UTF32);
                if (!DvdCollection.LoadFromFile(@"c:\temp\dvdUTF32.xml", Encoding.UTF32, out deserialiseDvd, out exp))
                {
                    Assert.Fail("LoadFromFile failed on UTF32 encoding ");
                }
                else
                {
                    if (!deserialiseDvd.Dvds[0].Title.Equals("Matrix יא?"))
                    {
                        Assert.Fail("LoadFromFile failed on UTF32 encoding ");
                    }
                }

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// DVDs this instance.
        /// </summary>
        [TestMethod]
        public void SerializeTest()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                GetInputFilePath("Actor.xsd", Resources.Actor);

                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("Dvd.xsd", Resources.dvd);
                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.CollectionObjectType = CollectionType.List;
                generatorParams.TargetFramework = TargetFramework.Net20;
                generatorParams.Serialization.GenerateXmlAttributes = true;
                generatorParams.PropertyParams.PascalCaseProperty = true;

                var xsdGen = new GeneratorFacade(generatorParams);
                xsdGen.Generate();

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Test LazyLoadind pattern
        /// </summary>
        [TestMethod]
        public void LazyLoading()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("LazyLoading.xsd", Resources.LazyLoading);
                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.CollectionObjectType = CollectionType.List;
                generatorParams.TargetFramework = TargetFramework.Net20;
                generatorParams.PropertyParams.EnableLazyLoading = true;
                generatorParams.InitializeFields = InitializeFieldsType.All;

                var xsdGen = new GeneratorFacade(generatorParams);
                xsdGen.Generate();

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Test LazyLoadind pattern
        /// </summary>
        [TestMethod]
        public void HexBinary()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("hexBinary.xsd", Resources.hexBinary);
                MultiConfigurationTest(inputFilePath);
            }
        }

        /// <summary>
        /// Test LazyLoadind pattern
        /// </summary>
        [TestMethod]
        public void MusicXMLTest()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("musicxml.xsd", Resources.musicxml);
                MultiConfigurationTest(inputFilePath);
            }
        }

        /// <summary>
        /// Test LazyLoadind pattern
        /// </summary>
        [TestMethod]
        public void MailXMLTest()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                GetInputFilePath("mailxml_base_120108.xsd", Resources.mailxml_base_120108);
                string inputFilePath = GetInputFilePath("mailxml_base.xsd", Resources.mailxml_base);
                MultiConfigurationTest(inputFilePath);
            }
        }

        /// <summary>
        /// Multis the configuration test.
        /// </summary>
        /// <param name="inputFilePath">The input file path.</param>
        private void MultiConfigurationTest(string inputFilePath)
        {
            GeneratorParams generatorParams;
            GeneratorFacade xsdGen;

            // Basic configuration
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.CollectionObjectType = CollectionType.Array;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net20);
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);
            Build(generatorParams.OutputFilePath, TargetFramework.Net35);
            Build(generatorParams.OutputFilePath, TargetFramework.Net40);

            // Automatic property on all framework
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net20;
            generatorParams.PropertyParams.AutomaticProperties = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net20);
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);
            Build(generatorParams.OutputFilePath, TargetFramework.Net35);
            Build(generatorParams.OutputFilePath, TargetFramework.Net40);

            // LazyLoading on all framework
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net20;
            generatorParams.PropertyParams.EnableLazyLoading = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net20);

            // PropertyNameSpecified option on Net20
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net20;
            generatorParams.PropertyParams.GeneratePropertyNameSpecified = PropertyNameSpecifiedType.All;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net20);

            // Virtual property option on Net20
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.CollectionObjectType = CollectionType.List;
            generatorParams.TargetFramework = TargetFramework.Net20;
            generatorParams.PropertyParams.EnableVirtualProperties = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net20);

            // PascalCase option on Net20
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net20;
            generatorParams.PropertyParams.PascalCaseProperty = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net40);

            // WCF
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.GenerateDataContracts = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());

            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // No Fields Initialize in ctor
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.InitializeFields = InitializeFieldsType.None;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // Initialize Fields only for collection types
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.InitializeFields = InitializeFieldsType.Collections;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // Initialize all Fields (instance of classes and collection)
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.InitializeFields = InitializeFieldsType.All;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // Tracking change classes
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.TrackingChanges.Enabled = true;
            generatorParams.TrackingChanges.GenerateTrackingClasses = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // Encoding
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.Serialization.Enabled = true;
            generatorParams.Serialization.EnableEncoding = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // DataBinding
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.CollectionObjectType = CollectionType.ObservableCollection;
            generatorParams.EnableDataBinding = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // Miscellaneous
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.Miscellaneous.DisableDebug = true;
            generatorParams.Miscellaneous.EnableSummaryComment = true;
            generatorParams.Miscellaneous.HidePrivateFieldInIde = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);

            // Use EntityBase as base classe
            generatorParams = new GeneratorParams() { InputFilePath = inputFilePath, NameSpace = CodeGenerationNamespace, OutputFilePath = GetOutputFilePath(inputFilePath) };
            generatorParams.TargetFramework = TargetFramework.Net30;
            generatorParams.GenericBaseClass.BaseClassName = "MyEntityBase";
            generatorParams.GenericBaseClass.Enabled = true;
            generatorParams.GenericBaseClass.GenerateBaseClass = true;
            xsdGen = new GeneratorFacade(generatorParams);
            CheckResult(xsdGen.Generate());
            Build(generatorParams.OutputFilePath, TargetFramework.Net30);
        }

        /// <summary>
        /// Checks the result.
        /// </summary>
        /// <param name="result">The result.</param>
        private void CheckResult(Result<string> result)
        {
            if (!result.Success)
            {
                Assert.Fail(result.Messages.Count > 0 ? result.Messages[0].Text : "Failed to generate class");
            }
        }

        /// <summary>
        /// Test LazyLoadind pattern
        /// </summary>
        [TestMethod]
        public void SerilizeBitmap()
        {
            lock (testLock)
            {
                var igm = new ImageElement();
                var myBmp = Image.FromFile(@"D:\MyDeveloppement\Xsd2Code\Branches\V3.5\Xsd2Code.TestUnit\img15.jpg");
                var ms = new MemoryStream();
                myBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                igm.Value = ms.ToArray();

                igm.SaveToFile(@"c:\temp\bitmap.xml");

                var igmLoaded = ImageElement.LoadFromFile(@"c:\temp\bitmap.xml");
                Stream s = new MemoryStream(igmLoaded.Value);
                var img = Image.FromStream(s);
                img.Save(@"c:\temp\bitmap.jpg");

            }
        }

        /// <summary>
        /// Test LazyLoadind pattern
        /// </summary>
        [TestMethod]
        public void Hierarchy()
        {
            lock (testLock)
            {
                var hierarchy = new Hierarchy();
                hierarchy.inheritedElmt = new MyOverrideType() { somethingelse = "test" };
                hierarchy.SaveToFile(@"c:\temp\test.xml");
                TestUnit.Hierarchy.LoadFromFile(@"c:\temp\test.xml");
            }
        }
        /// <summary>
        /// Test PropertyNameSpecified
        /// </summary>
        [TestMethod]
        public void PropertyNameSpecified()
        {
            lock (testLock)
            {
                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("PropertyNameSpecified.xsd", Resources.PropertyNameSpecified);
                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.TargetFramework = TargetFramework.Net20;
                generatorParams.Serialization.Enabled = false;
                generatorParams.Miscellaneous.HidePrivateFieldInIde = false;

                // All
                generatorParams.PropertyParams.GeneratePropertyNameSpecified = PropertyNameSpecifiedType.All;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath, ".all.cs");
                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);

                // none
                generatorParams.PropertyParams.GeneratePropertyNameSpecified = PropertyNameSpecifiedType.None;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath, ".none.cs");
                xsdGen = new GeneratorFacade(generatorParams);
                result = xsdGen.Generate();

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);

                // Default
                generatorParams.PropertyParams.GeneratePropertyNameSpecified = PropertyNameSpecifiedType.Default;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath, ".default.cs");
                xsdGen = new GeneratorFacade(generatorParams);
                result = xsdGen.Generate();

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Genders this instance.
        /// </summary>
        [TestMethod]
        public void Gender()
        {
            lock (testLock)
            {

                // Get the code namespace for the schema.
                string inputFilePath = GetInputFilePath("Gender.xsd", Resources.Gender);

                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.TargetFramework = TargetFramework.Net30;
                generatorParams.PropertyParams.AutomaticProperties = true;
                generatorParams.GenerateDataContracts = true;
                generatorParams.Serialization.GenerateXmlAttributes = true;
                generatorParams.OutputFilePath = GetOutputFilePath(inputFilePath);

                var xsdGen = new GeneratorFacade(generatorParams);

                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());

                var genderRoot = new Root
                                     {
                                         GenderAttribute = ksgender.female,
                                         GenderAttributeSpecified = true,
                                         GenderElement = ksgender.female,
                                         GenderIntAttribute = "toto"
                                     };
                Exception ex;
                genderRoot.SaveToFile(Path.Combine(OutputFolder, "gender.xml"), out ex);
                if (ex != null) throw ex;

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Genarates the VBCS.
        /// </summary>
        [TestMethod]
        public void GenarateVBCS()
        {
            lock (testLock)
            {
                // Get the code namespace for the schema.
                string inputFilePath = GetInputFilePath("Actor.xsd", Resources.Actor);

                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.TargetFramework = TargetFramework.Net30;
                generatorParams.PropertyParams.AutomaticProperties = true;
                generatorParams.GenerateDataContracts = true;
                generatorParams.Serialization.GenerateXmlAttributes = true;
                generatorParams.OutputFilePath = GetOutputFilePath(inputFilePath);
                generatorParams.EnableDataBinding = true;
                generatorParams.Miscellaneous.EnableSummaryComment = true;
                generatorParams.Language = GenerationLanguage.VisualBasic;
                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();
                Assert.IsTrue(result.Success, result.Messages.ToString());

                generatorParams.Language = GenerationLanguage.CSharp;
                xsdGen = new GeneratorFacade(generatorParams);
                xsdGen.Generate();

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }
       
        /// <summary>
        /// Hierarchicals this instance.
        /// </summary>
        [TestMethod]
        public void Hierarchical()
        {
            lock (testLock)
            {

                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("Hierarchical.xsd", Resources.Hierarchical);

                var generatorParams = GetGeneratorParams(inputFilePath);
                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();
                Assert.IsTrue(result.Success, result.Messages.ToString());

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Inheritances this instance.
        /// </summary>
        [TestMethod]
        public void Inheritance()
        {
            lock (testLock)
            {

                // Copy resource file to the run-time directory
                string inputFilePath = GetInputFilePath("Inheritance.xsd", Resources.Inheritance);

                var generatorParams = GetGeneratorParams(inputFilePath);
                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Silverlights this instance.
        /// </summary>
        [TestMethod]
        public void Silverlight()
        {
            lock (testLock)
            {

                // Get the code namespace for the schema.
                GetInputFilePath("Actor.xsd", Resources.Actor);
                string inputFilePath = GetInputFilePath("dvd.xsd", Resources.dvd);

                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.TargetFramework = TargetFramework.Silverlight;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath, ".Silverlight20_01.cs");

                generatorParams.Serialization.Enabled = true;
                generatorParams.Serialization.EnableEncoding = true;
                var xsdGen = new GeneratorFacade(generatorParams);

                //Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }


        /// <summary>
        /// XMLs the attributes.
        /// </summary>
        [TestMethod]
        public void XMLAttributes()
        {
            lock (testLock)
            {

                // Get the code namespace for the schema.
                GetInputFilePath("Actor.xsd", Resources.Actor);
                string inputFilePath = GetInputFilePath("mailxml_base.xsd", Resources.mailxml_base);
                GetInputFilePath("mailxml_base_120108.xsd", Resources.mailxml_base_120108);

                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.TargetFramework = TargetFramework.Net30;
                generatorParams.PropertyParams.AutomaticProperties = true;
                generatorParams.Serialization.GenerateXmlAttributes = true;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath, ".xml.cs");

                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());

                Build(generatorParams.OutputFilePath, TargetFramework.Net30);
                Build(generatorParams.OutputFilePath, TargetFramework.Net40);
            }
        }


        /// <summary>
        /// Automatics the properties.
        /// </summary>
        [TestMethod]
        public void AutomaticProperties()
        {
            lock (testLock)
            {

                // Get the code namespace for the schema.
                GetInputFilePath("Actor.xsd", Resources.Actor);
                string inputFilePath = GetInputFilePath("dvd.xsd", Resources.dvd);

                var generatorParams = new GeneratorParams { InputFilePath = inputFilePath };
                GetGeneratorParams(inputFilePath);
                generatorParams.TargetFramework = TargetFramework.Net30;
                generatorParams.Miscellaneous.EnableSummaryComment = false;
                generatorParams.GenerateDataContracts = false;
                generatorParams.PropertyParams.AutomaticProperties = true;
                generatorParams.Serialization.GenerateXmlAttributes = false;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath, ".autoProp.cs");

                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());
                Build(generatorParams.OutputFilePath, TargetFramework.Net30);
                Build(generatorParams.OutputFilePath, TargetFramework.Net40);
            }
        }

        /// <summary>
        /// Uses the base class.
        /// </summary>
        [TestMethod]
        public void UseBaseClass()
        {
            lock (testLock)
            {

                // Get the code namespace for the schema.
                GetInputFilePath("Actor.xsd", Resources.Actor);
                string inputFilePath = GetInputFilePath("dvd.xsd", Resources.dvd);

                string outputFilePath = Path.ChangeExtension(inputFilePath, ".baseClass.cs");
                var generatorParams = new GeneratorParams
                                          {
                                              InputFilePath = inputFilePath,
                                              TargetFramework = TargetFramework.Net30,
                                              GenerateDataContracts = true,
                                              EnableDataBinding = true,
                                              OutputFilePath = outputFilePath
                                          };

                generatorParams.PropertyParams.AutomaticProperties = false;
                generatorParams.Miscellaneous.EnableSummaryComment = true;
                generatorParams.GenericBaseClass.Enabled = true;
                generatorParams.GenericBaseClass.BaseClassName = "EntityObject";
                generatorParams.GenericBaseClass.GenerateBaseClass = true;

                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();

                Assert.IsTrue(result.Success, result.Messages.ToString());

                Build(generatorParams.OutputFilePath, TargetFramework.Net30);
                Build(generatorParams.OutputFilePath, TargetFramework.Net40);
            }
        }

        /// <summary>
        /// Tests the choice.
        /// </summary>
        [TestMethod]
        public void TestChoice()
        {
            lock (testLock)
            {
                Choice choice = new Choice();
                choice.Items.Add(new Car() { Speed = 150 });
                choice.Items.Add(new Train() { Speed = 250 });
                choice.Items.Add(new Plane() { Speed = 350, EngineNumber = 4});
                choice.SaveToFile(@"c:\temp\choice.xml");

                Choice deserilizeChoice;
                Choice.LoadFromFile(@"c:\temp\choice.xml", out deserilizeChoice);
                deserilizeChoice.SaveToFile(@"c:\temp\choiceD.xml");
            }
        }

        /// <summary>
        /// Builds the specified source file path.
        /// </summary>
        /// <param name="sourceFilePath">The source file path.</param>
        public void Build(string sourceFilePath, TargetFramework targetFramework)
        {
            lock (testLock)
            {
                var buildDirectory = Path.Combine(OutputFolder, "Build");
                try
                {
                    if (Directory.Exists(buildDirectory))
                        Directory.Delete(buildDirectory, true);

                }
                catch (Exception)
                {
                }

                DirectoryHelper.CopyAll(@"D:\MyDeveloppement\Xsd2Code\Branches\V3.5\Xsd2Code.Test.Console", buildDirectory, false, true);

                var csProjFile = Path.Combine(buildDirectory, "Xsd2Code.Test.Console.csproj");
                var strFile = File.ReadAllText(csProjFile);

                switch (targetFramework)
                {
                    case TargetFramework.Net20:
                        {

                            strFile = strFile.Replace("<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>", "<TargetFrameworkVersion>v2.0</TargetFrameworkVersion>");
                        }
                        break;
                    case TargetFramework.Net30:
                        {
                            strFile = strFile.Replace("<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>", "<TargetFrameworkVersion>v3.0</TargetFrameworkVersion>");
                            strFile = strFile.Replace("<Reference Include=\"System.Data.DataSetExtensions\" />", "<Reference Include=\"WindowsBase\" />");
                        }
                        break;
                    case TargetFramework.Net35:
                        {
                            strFile = strFile.Replace("<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>", "<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>");
                        }
                        break;
                    case TargetFramework.Silverlight:
                        break;
                }


                File.WriteAllText(csProjFile, strFile);
                File.Copy(sourceFilePath, Path.Combine(buildDirectory, "SourceFile.cs"), true);

                string errorMessage;
                if (!SolutionBuilder.CompileSolution(Path.Combine(buildDirectory, "Xsd2Code.Test.Console.csproj"), out errorMessage))
                {
                    File.WriteAllText(Path.Combine(buildDirectory, "ErrorList.txt"), errorMessage);
                    Assert.Fail(errorMessage);
                }
            }
        }


        /// <summary>
        /// Tests the annotations.
        /// </summary>
        [TestMethod]
        public void TestAnnotations()
        {
            lock (testLock)
            {

                // Get the code namespace for the schema.
                string inputFilePath = GetInputFilePath("TestAnnotations.xsd", Resources.TestAnnotations);

                var generatorParams = new GeneratorParams { InputFilePath = inputFilePath };
                GetGeneratorParams(inputFilePath);

                generatorParams.Miscellaneous.EnableSummaryComment = true;
                generatorParams.TargetFramework = TargetFramework.Net35;
                generatorParams.PropertyParams.AutomaticProperties = true;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath,
                                                                      ".TestAnnotations.cs");

                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();
                Assert.IsTrue(result.Success, result.Messages.ToString());

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// WCFs the attributes.
        /// </summary>
        [TestMethod]
        public void WcfAttributes()
        {
            lock (testLock)
            {

                // Get the code namespace for the schema.
                GetInputFilePath("Actor.xsd", Resources.Actor);
                string inputFilePath = GetInputFilePath("dvd.xsd", Resources.dvd);

                var generatorParams = GetGeneratorParams(inputFilePath);
                generatorParams.GenerateDataContracts = true;
                generatorParams.TargetFramework = TargetFramework.Net30;
                generatorParams.OutputFilePath = Path.ChangeExtension(generatorParams.InputFilePath, ".wcf.cs");

                var xsdGen = new GeneratorFacade(generatorParams);
                var result = xsdGen.Generate();
                Assert.IsTrue(result.Success, result.Messages.ToString());

                Build(generatorParams.OutputFilePath, generatorParams.TargetFramework);
            }
        }

        /// <summary>
        /// Gets the input file path.
        /// </summary>
        /// <param name="resourceFileName">Name of the resource file.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns></returns>
        private static string GetInputFilePath(string resourceFileName, string fileContent)
        {
            lock (fileLock)
            {
                using (var sw = new StreamWriter(OutputFolder + resourceFileName, false))
                {
                    sw.Write(fileContent);
                }

                return OutputFolder + resourceFileName;
            }
        }

        private static GeneratorParams GetGeneratorParams(string inputFilePath)
        {
            var generatorParams = new GeneratorParams
                       {
                           InputFilePath = inputFilePath,
                           NameSpace = CodeGenerationNamespace,
                           TargetFramework = TargetFramework.Net20,
                           CollectionObjectType = CollectionType.List,
                           EnableDataBinding = true,
                           GenerateDataContracts = true,
                           GenerateCloneMethod = true,
                           OutputFilePath = GetOutputFilePath(inputFilePath)
                       };
            generatorParams.Miscellaneous.HidePrivateFieldInIde = true;
            generatorParams.Miscellaneous.DisableDebug = true;
            generatorParams.Serialization.Enabled = true;
            return generatorParams;
        }

        /// <summary>
        /// Get output file path
        /// </summary>
        /// <param name="inputFilePath">input file path</param>
        /// <returns></returns>
        static private string GetOutputFilePath(string inputFilePath)
        {
            return Path.ChangeExtension(inputFilePath, ".TestGenerated.cs");
        }



        /// <summary>
        /// Compile file
        /// </summary>
        /// <param name="filePath">CS file path</param>
        /// <returns></returns>
        static private Result<string> CompileCSFile(string filePath)
        {
            var result = new Result<string>(null, true);
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                result.Success = false;
                result.Messages.Add(MessageType.Error, "Input file \"{0}\" does not exist", filePath);
            }
            if (result.Success)
            {
                try
                {
                    var outputPath = Path.ChangeExtension(file.FullName, ".dll");
                    result.Entity = outputPath;

                    var args = new StringBuilder();
                    args.Append(" /target:module /nologo /debug");
                    args.Append(" /out:\"" + outputPath + "\"");
                    args.Append(" \"" + filePath + "\"");

                    var compilerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                                                    @"..\Microsoft.NET\Framework\v2.0.50727\csc.exe");

                    var compilerFile = new FileInfo(compilerPath);

                    Debug.WriteLine(string.Format("Executing:\r\n{0} {1}\r\n", compilerFile.FullName, args));

                    var info = new ProcessStartInfo
                                   {
                                       ErrorDialog = false,
                                       FileName = compilerFile.FullName,
                                       Arguments = args.ToString(),
                                       CreateNoWindow = true,
                                       WindowStyle = ProcessWindowStyle.Minimized
                                   };

                    using (var process = new Process { StartInfo = info })
                    {
                        process.ErrorDataReceived += (s, e) =>
                                                         {
                                                             result.Success = false;
                                                             result.Messages.Add(MessageType.Error, "Error data received", e.Data);
                                                         };

                        process.Exited += (s, e) => { result.Success = process.ExitCode == 1 && File.Exists(outputPath); };

                        process.OutputDataReceived += (s, e) => result.Messages.Add(MessageType.Debug, "Output data received", e.Data);

                        if (!process.Start())
                            throw new ApplicationException("Unablle to start process");

                        var exited = process.WaitForExit((int)TimeSpan.FromSeconds(15).TotalMilliseconds);
                        if (!exited)
                        {
                            result.Success = false;
                            result.Messages.Add(MessageType.Error, "Timeout", "Compile timeout occurred {0}", DateTime.Now - process.StartTime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Messages.Add(MessageType.Error, "Exception", ex.ToString());
                }
            }

            if (result.Messages.Count > 0)
                Debug.WriteLine(result.Messages.ToString());

            return result;
        }

    }

}