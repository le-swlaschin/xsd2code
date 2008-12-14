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
        #region Fields
        /// <summary>
        /// Full XSD file path
        /// </summary>
        private string inputFileField;

        /// <summary>
        /// Full cs or vb file path
        /// </summary>
        private string outputFileField;

        /// <summary>
        /// namespace to use for generation
        /// </summary>
        private string nameSpaceField;

        /// <summary>
        /// Language generation
        /// </summary>
        private GenerationLanguage languageField;

        /// <summary>
        /// Type of collection to use in generation
        /// </summary>
        private CollectionType collectionField;

        /// <summary>
        /// Indicate if use databinding
        /// </summary>
        private bool enableDataBindingField;

        /// <summary>
        /// Indicates if generate EditorBrowsableState.Never attribute
        /// </summary>
        private bool hidePrivateField;

        /// <summary>
        /// Indicates if generate summary comment
        /// </summary>
        private bool enableSummaryCommentField;

        /// <summary>
        /// List of custom usings to include in the file
        /// </summary>
        private string customUsingsField;

        /// <summary>
        /// Name of the generic type to use for collections
        /// </summary>
        private string collectionBaseField;

        /// <summary>
        /// Enable generation for serialize/deserialise method
        /// </summary>
        private bool includeSerializeMethodField;

        /// <summary>
        /// serialize method name
        /// </summary>
        private string serializeMethodNameField;

        /// <summary>
        /// serialize method name
        /// </summary>
        private string deserializeMethodNameField;

        /// <summary>
        /// serialize method name
        /// </summary>
        private string saveToFileMethodNameField;

        /// <summary>
        /// serialize method name
        /// </summary>
        private string loadFromFileMethodNameField;

        #endregion

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
        public GeneratorFacade(string inputFile, string nameSpace, GenerationLanguage language, CollectionType collectionType, bool enableDataBinding, bool hidePrivate, bool enableSummaryComment, string customUsings, string collectionBase, bool includeSerializeMethod, string serializeMethodName, string deserializeMethodName, string saveToFileMethodName, string loadFromFileMethodName)
        {
            this.inputFileField = inputFile;
            this.nameSpaceField = nameSpace;
            this.languageField = language;
            this.collectionField = collectionType;
            this.enableDataBindingField = enableDataBinding;
            this.hidePrivateField = hidePrivate;
            this.enableSummaryCommentField = enableSummaryComment;
            this.customUsingsField = customUsings;
            this.collectionBaseField = collectionBase;
            this.includeSerializeMethodField = includeSerializeMethod;
            this.serializeMethodNameField = serializeMethodName;
            this.deserializeMethodNameField = deserializeMethodName;
            this.saveToFileMethodNameField = saveToFileMethodName;
            this.loadFromFileMethodNameField = loadFromFileMethodName;

            switch (language)
            {
                case GenerationLanguage.CSharp:
                    this.outputFileField = inputFile.Replace(".xsd", ".cs");
                    break;
                case GenerationLanguage.VisualBasic:
                    this.outputFileField = inputFile.Replace(".xsd", ".vb");
                    break;
                case GenerationLanguage.VisualCpp:
                    this.outputFileField = inputFile.Replace(".xsd", ".cpp");
                    break;
            }
        }
        #endregion

        #region Property

        /// <summary>
        /// Gets full xsd file path
        /// </summary>
        public string InputFile
        {
            get { return this.inputFileField; }
        }

        /// <summary>
        /// Gets namespace to use for generation
        /// </summary>
        public string NameSpace
        {
            get { return this.nameSpaceField; }
        }

        /// <summary>
        /// Gets language type for generation
        /// </summary>
        public GenerationLanguage Language
        {
            get { return this.languageField; }
        }

        /// <summary>
        /// Gets a value indicating whether if implement INotifyPropertyChanged
        /// </summary>
        public bool EnableDataBinding
        {
            get { return this.enableDataBindingField; }
        }

        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        public string OutputFile
        {
            get { return outputFileField; }
            set { outputFileField = value; }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Processes the code generation.
        /// </summary>
        /// <param name="outputFileName">Name of the output file.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>true if sucess or false.</returns>
        public bool ProcessCodeGeneration(out string outputFileName, out string errorMessage)
        {
            CodeDomProvider provider;
            errorMessage = "";
            outputFileName = "";

            #region Change CurrentDir for include schema resolution.
            string savedCurrentDir = Directory.GetCurrentDirectory();
            FileInfo fi = new FileInfo(this.inputFileField);
            if (!fi.Exists)
            {
                errorMessage = "Faild to generate code\n";
                errorMessage += "Exception :\n";
                errorMessage += string.Format("Input file {0} not exist", this.inputFileField);
                return false;
            }

            Directory.SetCurrentDirectory(fi.Directory.FullName);
            #endregion

            try
            {
                try
                {
                    switch (this.languageField)
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

                    CodeGeneratorOptions codeOption = new CodeGeneratorOptions();
                    CodeNamespace ns = Generator.Process(this.inputFileField, this.nameSpaceField, this.languageField, this.collectionField, this.enableDataBindingField, this.hidePrivateField, this.enableSummaryCommentField, this.customUsingsField, this.collectionBaseField, this.includeSerializeMethodField, this.serializeMethodNameField, this.deserializeMethodNameField, this.saveToFileMethodNameField, this.loadFromFileMethodNameField, out errorMessage);
                    if (ns == null)
                    {
                        return false;
                    }

                    using (StreamWriter sw = new StreamWriter(this.outputFileField + ".tmp", false))
                    {
                        provider.GenerateCodeFromNamespace(ns, sw, new CodeGeneratorOptions());
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

                FileInfo outPutInfo = new FileInfo(this.outputFileField);
                if (outPutInfo.Exists)
                {
                    if ((outPutInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        errorMessage = "Faild to generate code\n";
                        errorMessage += this.outputFileField + " is write protect";
                        return false;
                    }
                }

                outputFileName = this.outputFileField;

                #region Insert tag for future generation
                StreamWriter swend = new StreamWriter(this.outputFileField, false);
                string commentStr;
                commentStr = this.languageField == GenerationLanguage.CSharp ? "// " : "'' ";

                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                AssemblyName currentAssemblyName = currentAssembly.GetName();

                string optionsLine = commentStr;
                swend.WriteLine(string.Format("{0}------------------------------------------------------------------------------", commentStr));
                swend.WriteLine(string.Format("{0} <auto-generated>", commentStr));
                swend.WriteLine(string.Format("{0}   Generated by Xsd2Code. Version {1}", commentStr, currentAssemblyName.Version.ToString()));
                optionsLine += "  ";
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.NameSpaceTag, this.nameSpaceField);
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CollectionTag, this.collectionField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CodeTypeTag, this.languageField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.EnableDataBindingTag, this.enableDataBindingField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.HidePrivateFieldTag, this.hidePrivateField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.EnableSummaryCommentTag, this.enableSummaryCommentField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CustomUsingsTag, this.customUsingsField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CollectionBaseTag, this.collectionBaseField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.IncludeSerializeMethodTag, this.includeSerializeMethodField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.SerializeMethodNameTag, this.serializeMethodNameField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.DeserializeMethodNameTag, this.deserializeMethodNameField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.SaveToFileMethodNameTag, this.saveToFileMethodNameField.ToString());
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.LoadFromFileMethodNameTag, this.loadFromFileMethodNameField.ToString());

                swend.WriteLine(optionsLine);
                swend.WriteLine(string.Format("{0} <auto-generated>", commentStr));

                swend.WriteLine(string.Format("{0}------------------------------------------------------------------------------", commentStr));
                #endregion

                string line = "";
                using (TextReader streamReader = new StreamReader(this.outputFileField + ".tmp"))
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Trim() != "[System.SerializableAttribute()]" &&
                            line.Trim() != "[System.ComponentModel.DesignerCategoryAttribute(\"code\")]")
                        {
                            swend.WriteLine(line);
                        }
                    }
                }

                swend.Close();
                FileInfo tmp = new FileInfo(this.outputFileField + ".tmp");
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
