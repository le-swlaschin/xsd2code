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
        VisualBasic
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
        ObservableCollection
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
        #endregion

        #region Class constructor
        /// <summary>
        /// Constructor of genrator class facade
        /// </summary>
        /// <param name="inputFile">Full path of XSD file</param>
        /// <param name="nameSpace">the namespace to use for generation</param>
        /// <param name="language">generation language</param>
        /// <param name="collectionType">type of collection</param>
        /// <param name="enableDataBinding">Indicates whether the generation implement the change notification</param>
        /// <param name="hidePrivate">Indicates if generate EditorBrowsableState.Never attribute</param>
        /// <param name="enableSummaryComment">Enable summary comment from XmlSchema annotation</param>
        public GeneratorFacade(string inputFile, string nameSpace, GenerationLanguage language, CollectionType collectionType, bool enableDataBinding, bool hidePrivate, bool enableSummaryComment)
        {
            this.inputFileField = inputFile;
            this.nameSpaceField = nameSpace;
            this.languageField = language;
            this.collectionField = collectionType;
            this.enableDataBindingField = enableDataBinding;
            this.hidePrivateField = hidePrivate;
            this.enableSummaryCommentField = enableSummaryComment;

            switch (language)
            {
                case GenerationLanguage.CSharp:
                    this.outputFileField = inputFile.Replace(".xsd", ".cs");
                    break;
                case GenerationLanguage.VisualBasic:
                    this.outputFileField = inputFile.Replace(".xsd", ".vb");
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
        #endregion

        #region Methods
        /// <summary>
        /// Process generation
        /// </summary>
        /// <param name="errorMessage">Output error message if return false else empty string.</param>
        /// <returns>true if sucess or false.</returns>
        public bool Process(out string errorMessage)
        {
            CodeDomProvider provider;
            errorMessage = "";

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
                        default:
                            provider = new Microsoft.CSharp.CSharpCodeProvider();
                            break;
                    }

                    CodeGeneratorOptions codeOption = new CodeGeneratorOptions();
                    CodeNamespace ns = Generator.Process(this.inputFileField, this.nameSpaceField, this.languageField, this.collectionField, this.enableDataBindingField, this.hidePrivateField, this.enableSummaryCommentField, out errorMessage);
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

                #region Insert tag for futur generation
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
