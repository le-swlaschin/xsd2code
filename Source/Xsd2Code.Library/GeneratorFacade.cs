//-----------------------------------------------------------------------
// <copyright file="GeneratorFacade.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

namespace Xsd2Code.Library
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.CodeDom.Compiler;
    using System.CodeDom;
    using System.IO;
    using System.Diagnostics;
    using Xsd2Code.Helpers;
    using System.Reflection;

    #region enum
    /// <summary>
    /// Language for generation
    /// </summary>
    public enum GenerationLanguage
    {
        /// <summary>
        /// CShap language
        /// </summary>
        CSharp,

        /// <summary>
        /// Visual Basic language
        /// </summary>
        VisualBasic,

        /// <summary>
        /// Visual c++
        /// </summary>
        VisualCpp
    }

    /// <summary>
    /// Collection for generation
    /// </summary>
    public enum CollectionType
    {
        /// <summary>
        /// Array collection
        /// </summary>
        Array,

        /// <summary>
        /// Generic List
        /// </summary>
        List,

        /// <summary>
        /// Generic list for notifications when items get added, removed, or when the whole list is refreshed
        /// </summary>
        ObservableCollection,

        /// <summary>
        /// Defined type for each specialized collection with designer and base files
        /// </summary>
        DefinedType
    }
    #endregion

    /// <summary>
    /// Encapsulation of all generation process.
    /// </summary>
    public class GeneratorFacade
    {
        private GeneratorParams generatorParamsField = new GeneratorParams();
        private CodeDomProvider providerField;

        #region Class constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorFacade"/> class.
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="language">The language.</param>
        /// <param name="collectionType">Type of the collection.</param>
        /// <param name="enableDataBinding">if set to <c>true</c> [enable data binding].</param>
        /// <param name="hidePrivate">if set to <c>true</c> [hide private].</param>
        /// <param name="enableSummaryComment">if set to <c>true</c> [enable summary comment].</param>
        /// <param name="customUsings">The custom usings.</param>
        /// <param name="collectionBase">The collection base.</param>
        /// <param name="includeSerializeMethod">if set to <c>true</c> [include serialize method].</param>
        /// <param name="serializeMethodName">Name of the serialize method.</param>
        /// <param name="deserializeMethodName">Name of the deserialize method.</param>
        /// <param name="saveToFileMethodName">Name of the save to file method.</param>
        /// <param name="loadFromFileMethodName">Name of the load from file method.</param>
        /// <param name="disableDebug">if set to <c>true</c> [disable debug].</param>
        public GeneratorFacade(string inputFile, string nameSpace, GenerationLanguage language, CollectionType collectionType, bool enableDataBinding, bool hidePrivate, bool enableSummaryComment, string customUsings, string collectionBase, bool includeSerializeMethod, string serializeMethodName, string deserializeMethodName, string saveToFileMethodName, string loadFromFileMethodName, bool disableDebug)
        {
            CodeDomProvider provider;
            switch (this.generatorParamsField.Language)
            {
                case GenerationLanguage.CSharp:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
                case GenerationLanguage.VisualBasic:
                    provider = new Microsoft.VisualBasic.VBCodeProvider();
                    break;
                case GenerationLanguage.VisualCpp:
                    provider = new Microsoft.VisualC.CppCodeProvider();
                    break;
                default:
                    provider = new Microsoft.CSharp.CSharpCodeProvider();
                    break;
            }
            Init(inputFile, nameSpace, provider, collectionType, enableDataBinding, hidePrivate, enableSummaryComment, customUsings, collectionBase, includeSerializeMethod, serializeMethodName, deserializeMethodName, saveToFileMethodName, loadFromFileMethodName, disableDebug);
        }

        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorFacade"/> class.
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="collectionType">Type of the collection.</param>
        /// <param name="enableDataBinding">if set to <c>true</c> [enable data binding].</param>
        /// <param name="hidePrivate">if set to <c>true</c> [hide private].</param>
        /// <param name="enableSummaryComment">if set to <c>true</c> [enable summary comment].</param>
        /// <param name="customUsings">The custom usings.</param>
        /// <param name="collectionBase">The collection base.</param>
        /// <param name="includeSerializeMethod">if set to <c>true</c> [include serialize method].</param>
        /// <param name="serializeMethodName">Name of the serialize method.</param>
        /// <param name="deserializeMethodName">Name of the deserialize method.</param>
        /// <param name="saveToFileMethodName">Name of the save to file method.</param>
        /// <param name="loadFromFileMethodName">Name of the load from file method.</param>
        /// <param name="disableDebug">if set to <c>true</c> [disable debug].</param>
        public GeneratorFacade(string inputFile, string nameSpace, CodeDomProvider provider, CollectionType collectionType, bool enableDataBinding, bool hidePrivate, bool enableSummaryComment, string customUsings, string collectionBase, bool includeSerializeMethod, string serializeMethodName, string deserializeMethodName, string saveToFileMethodName, string loadFromFileMethodName, bool disableDebug)
        {
            Init(inputFile, nameSpace, provider, collectionType, enableDataBinding, hidePrivate, enableSummaryComment, customUsings, collectionBase, includeSerializeMethod, serializeMethodName, deserializeMethodName, saveToFileMethodName, loadFromFileMethodName, disableDebug);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratorFacade"/> class.
        /// </summary>
        /// <param name="inputFile">The input file.</param>
        /// <param name="nameSpace">The name space.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="collectionType">Type of the collection.</param>
        /// <param name="enableDataBinding">if set to <c>true</c> [enable data binding].</param>
        /// <param name="hidePrivate">if set to <c>true</c> [hide private].</param>
        /// <param name="enableSummaryComment">if set to <c>true</c> [enable summary comment].</param>
        /// <param name="customUsings">The custom usings.</param>
        /// <param name="collectionBase">The collection base.</param>
        /// <param name="includeSerializeMethod">if set to <c>true</c> [include serialize method].</param>
        /// <param name="serializeMethodName">Name of the serialize method.</param>
        /// <param name="deserializeMethodName">Name of the deserialize method.</param>
        /// <param name="saveToFileMethodName">Name of the save to file method.</param>
        /// <param name="loadFromFileMethodName">Name of the load from file method.</param>
        /// <param name="disableDebug">if set to <c>true</c> [disable debug].</param>
        public void Init(string inputFile, string nameSpace, CodeDomProvider provider, CollectionType collectionType, bool enableDataBinding, bool hidePrivate, bool enableSummaryComment, string customUsings, string collectionBase, bool includeSerializeMethod, string serializeMethodName, string deserializeMethodName, string saveToFileMethodName, string loadFromFileMethodName, bool disableDebug)
        {
            this.generatorParamsField.InputFilePath = inputFile;
            this.generatorParamsField.NameSpace = nameSpace;
            this.generatorParamsField.CollectionObjectType = collectionType;
            this.generatorParamsField.EnableDataBinding = enableDataBinding;
            this.generatorParamsField.HidePrivateFieldInIde = hidePrivate;
            this.generatorParamsField.EnableSummaryComment = enableSummaryComment;
            this.generatorParamsField.CustomUsings = customUsings;
            this.generatorParamsField.CollectionBase = collectionBase;
            this.generatorParamsField.IncludeSerializeMethod = includeSerializeMethod;
            this.generatorParamsField.SerializeMethodName = serializeMethodName;
            this.generatorParamsField.DeserializeMethodName = deserializeMethodName;
            this.generatorParamsField.SaveToFileMethodName = saveToFileMethodName;
            this.generatorParamsField.LoadFromFileMethodName = loadFromFileMethodName;
            this.generatorParamsField.DisableDebug = disableDebug;
            this.providerField = provider;

            string pbstrDefaultExtension = providerField.FileExtension;
            if (pbstrDefaultExtension != null && pbstrDefaultExtension.Length > 0)
            {
                pbstrDefaultExtension = ".Designer." + pbstrDefaultExtension.TrimStart(".".ToCharArray());

                if (provider.FileExtension.ToUpper() == ".CS")
                    this.generatorParamsField.Language = GenerationLanguage.CSharp;

                if (provider.FileExtension.ToUpper() == ".VB")
                    this.generatorParamsField.Language = GenerationLanguage.VisualBasic;

                if (provider.FileExtension.ToUpper() == ".CPP")
                    this.generatorParamsField.Language = GenerationLanguage.VisualCpp;
            }
            this.generatorParamsField.OutputFilePath = inputFile.Replace(".xsd", pbstrDefaultExtension);
        }
        #endregion

        #region Property
        public GeneratorParams GeneratorParams
        {
            get { return generatorParamsField; }
            set { generatorParamsField = value; }
        }
        #endregion

        #region Methods

        private byte[] FileToByte(string path)
        {
            System.IO.FileInfo MonFichier = new System.IO.FileInfo(path);
            System.IO.FileStream MonFileStream = MonFichier.OpenRead();
            byte[] TableauDeBytes = new byte[MonFileStream.Length];
            MonFileStream.Read(TableauDeBytes, 0, (int)MonFileStream.Length);
            MonFileStream.Close();
            return TableauDeBytes;
        }

        /// <summary>
        /// Generates the specified buffer size.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public byte[] Generate(out string errorMessage)
        {
            byte[] result = null;
            string outputFilePath = Path.GetTempFileName();
            if (!Process(outputFilePath, out errorMessage))
            {
                return null;
            }
            else
            {
                result = FileToByte(outputFilePath);
                try
                {
                    File.Delete(outputFilePath);
                }
                catch { }
                return result;
            }
        }
        /// <summary>
        /// Processes the code generation.
        /// </summary>
        /// <param name="outputFileName">Name of the output file.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>true if sucess or false.</returns>
        public bool Generate(out string outputFileName, out string errorMessage)
        {
            outputFileName = this.generatorParamsField.OutputFilePath;
            return Process(outputFileName, out errorMessage);
        }

        /// <summary>
        /// Processes the specified file name.
        /// </summary>
        /// <param name="OutputFilePath">The output file path.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private bool Process(string OutputFilePath, out string errorMessage)
        {
            errorMessage = "";

            #region Change CurrentDir for include schema resolution.
            string savedCurrentDir = Directory.GetCurrentDirectory();
            FileInfo fi = new FileInfo(this.generatorParamsField.InputFilePath);
            if (!fi.Exists)
            {
                errorMessage = "Faild to generate code\n";
                errorMessage += "Exception :\n";
                errorMessage += string.Format("Input file {0} not exist", this.generatorParamsField.InputFilePath);
                return false;
            }

            Directory.SetCurrentDirectory(fi.Directory.FullName);
            #endregion

            try
            {
                try
                {
                    CodeGeneratorOptions codeOption = new CodeGeneratorOptions();
                    CodeNamespace ns = Generator.Process(this.generatorParamsField.InputFilePath, this.generatorParamsField.NameSpace, this.generatorParamsField.Language, this.generatorParamsField.CollectionObjectType, this.generatorParamsField.EnableDataBinding, this.generatorParamsField.HidePrivateFieldInIde, this.generatorParamsField.EnableSummaryComment, this.generatorParamsField.CustomUsings, this.generatorParamsField.CollectionBase, this.generatorParamsField.IncludeSerializeMethod, this.generatorParamsField.SerializeMethodName, this.generatorParamsField.DeserializeMethodName, this.generatorParamsField.SaveToFileMethodName, this.generatorParamsField.LoadFromFileMethodName, out errorMessage);
                    if (ns == null)
                    {
                        return false;
                    }

                    using (StreamWriter sw = new StreamWriter(OutputFilePath + ".tmp", false))
                    {
                        providerField.GenerateCodeFromNamespace(ns, sw, new CodeGeneratorOptions());
                    }
                }
                catch (Exception e)
                {
                    errorMessage = "Faild to generate code\n";
                    errorMessage += "Exception :\n";
                    errorMessage += e.Message;

                    Debug.WriteLine("");
                    Debug.WriteLine("XSD2Code - ----------------------------------------------------------");
                    Debug.WriteLine("XSD2Code - " + e.Message);
                    Debug.WriteLine("XSD2Code - ----------------------------------------------------------");
                    Debug.WriteLine("");
                    return false;
                }

                FileInfo outPutInfo = new FileInfo(OutputFilePath);
                if (outPutInfo.Exists)
                {
                    if ((outPutInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        errorMessage = "Faild to generate code\n";
                        errorMessage += OutputFilePath + " is write protect";
                        return false;
                    }
                }

                #region Insert tag for future generation
                StreamWriter swend = new StreamWriter(OutputFilePath, false);
                string commentStr;
                commentStr = this.generatorParamsField.Language == GenerationLanguage.CSharp ? "// " : "'' ";

                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                AssemblyName currentAssemblyName = currentAssembly.GetName();

                string optionsLine = commentStr;
                swend.WriteLine(string.Format("{0}------------------------------------------------------------------------------", commentStr));
                swend.WriteLine(string.Format("{0} <auto-generated>", commentStr));
                swend.WriteLine(string.Format("{0}   Generated by Xsd2Code. Version {1}", commentStr, currentAssemblyName.Version.ToString()));
                optionsLine += "  ";
                optionsLine = string.Format("{0}   {1}", commentStr, this.GeneratorParams.ToXmlTag());
                swend.WriteLine(optionsLine);
                swend.WriteLine(string.Format("{0} <auto-generated>", commentStr));
                swend.WriteLine(string.Format("{0}------------------------------------------------------------------------------", commentStr));
                #endregion

                string line = "";
                using (TextReader streamReader = new StreamReader(OutputFilePath + ".tmp"))
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Trim() != "[System.SerializableAttribute()]" &&
                            line.Trim() != "[System.ComponentModel.DesignerCategoryAttribute(\"code\")]")
                        {
                            if (!this.generatorParamsField.DisableDebug)
                            {
                                if (line.Trim() != "[System.Diagnostics.DebuggerStepThroughAttribute()]")
                                    swend.WriteLine(line);
                            }
                            else
                            {
                                swend.WriteLine(line);
                            }
                        }
                    }
                }

                swend.Close();
                FileInfo tmp = new FileInfo(OutputFilePath + ".tmp");
                tmp.Delete();
            }
            finally
            {
                Directory.SetCurrentDirectory(savedCurrentDir);
            }

            return true;
        }
        #endregion
    }
}
