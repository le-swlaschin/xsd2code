//-----------------------------------------------------------------------
// <copyright file="GeneratorParams.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

namespace Xsd2Code.Library
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xsd2Code.Helpers;
    using System.IO;
    using System.ComponentModel;

    /// <summary>
    /// Represent all parameters.
    /// </summary>
    public class GeneratorParams
    {
        #region Private
        /// <summary>
        /// Type of collection
        /// </summary>
        private CollectionType collectionObjectTypeField = CollectionType.List;

        /// <summary>
        /// Name of serialize method.
        /// </summary>
        private string serializeMethodNameField = "Serialize";

        /// <summary>
        /// Name of deserialize method
        /// </summary>
        private string deserializeMethodNameField = "Deserialize";

        /// <summary>
        /// Name of save to file method.
        /// </summary>
        private string saveToFileMethodNameField = "SaveToFile";

        /// <summary>
        /// Name of load from file method.
        /// </summary>
        private string loadFromFileMethodNameField = "LoadFromFile";

        /// <summary>
        /// List of custom usings
        /// </summary>
        private List<NamespaceParam> customUsingsField = new List<NamespaceParam>();

        /// <summary>
        /// Indicate if allow debug into generated code.
        /// </summary>
        private bool disableDebugField = true;

        /// <summary>
        /// Indicate if hide private field in ide.
        /// </summary>
        private bool hidePrivateFieldInIdeField = true;
        #endregion

        /// <summary>
        /// Gets or sets the name space.
        /// </summary>
        /// <value>The name space.</value>
        [CategoryAttribute("Code"), DescriptionAttribute("namespace of generated file")]
        public string NameSpace
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets generation language
        /// </summary>
        [CategoryAttribute("Code"), DescriptionAttribute("Language")]
        public GenerationLanguage Language
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the output file path.
        /// </summary>
        /// <value>The output file path.</value>
        [Browsable(false)]
        public string OutputFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the input file path.
        /// </summary>
        /// <value>The input file path.</value>
        [Browsable(false)]
        public string InputFilePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection type to use for code generation
        /// </summary>
        [CategoryAttribute("Collection"), DescriptionAttribute("Set type of collection for unbounded elements")]
        public CollectionType CollectionObjectType
        {
            get { return this.collectionObjectTypeField; }
            set { this.collectionObjectTypeField = value; }
        }

        /// <summary>
        /// Gets or sets collection base
        /// </summary>
        [CategoryAttribute("Collection"), DescriptionAttribute("Set the collection base if using CustomCollection")]
        public string CollectionBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets custom usings
        /// </summary>
        [CategoryAttribute("Collection"), DescriptionAttribute("list of custom using for CustomCollection")]
        public List<NamespaceParam> CustomUsings
        {
            get { return this.customUsingsField; }
            set { this.customUsingsField = value; }
        }

        /// <summary>
        /// Gets or sets the custom usings string.
        /// </summary>
        /// <value>The custom usings string.</value>
        [Browsable(false)]
        public string CustomUsingsString
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if implement INotifyPropertyChanged
        /// </summary>
        [CategoryAttribute("Behavior"), DefaultValueAttribute(false), DescriptionAttribute("Indicating whether if implement INotifyPropertyChanged.")]
        public bool EnableDataBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether serialize/deserialize method support
        /// </summary>
        [CategoryAttribute("Behavior"), DefaultValueAttribute(false), DescriptionAttribute("Indicating whether serialize/deserialize method nust be generate.")]
        public bool IncludeSerializeMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if generate EditorBrowsableState.Never attribute
        /// </summary>
        [CategoryAttribute("Behavior"), DefaultValueAttribute(true), DescriptionAttribute("Indicating whether if generate EditorBrowsableState.Never attribute.")]
        public bool HidePrivateFieldInIde
        {
            get { return this.hidePrivateFieldInIdeField; }
            set { this.hidePrivateFieldInIdeField = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [disable debug].
        /// </summary>
        /// <value><c>true</c> if [disable debug]; otherwise, <c>false</c>.</value>
        [CategoryAttribute("Behavior"), DefaultValueAttribute(true), DescriptionAttribute("Indicating whether if generate attribute for debug into generated code.")]
        public bool DisableDebug
        {
            get { return this.disableDebugField; }
            set { this.disableDebugField = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether if generate summary documentation
        /// </summary>
        [CategoryAttribute("Behavior"), DefaultValueAttribute(false), DescriptionAttribute("Indicating whether if generate summary documentation from xsd annotation.")]
        public bool EnableSummaryComment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the name of Serialize method.
        /// </summary>
        [CategoryAttribute("Serialize"), DefaultValue("Serialize"), DescriptionAttribute("The name of Serialize method.")]
        public string SerializeMethodName
        {
            get { return this.serializeMethodNameField; }
            set { this.serializeMethodNameField = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the name of Deserialize method.
        /// </summary>
        [CategoryAttribute("Serialize"), DefaultValue("Deserialize"), DescriptionAttribute("The name of deserialize method.")]
        public string DeserializeMethodName
        {
            get { return this.deserializeMethodNameField; }
            set { this.deserializeMethodNameField = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the name of Serialize method.
        /// </summary>
        [CategoryAttribute("Serialize"), DefaultValue("SaveToFile"), DescriptionAttribute("The name of save to xml file method.")]
        public string SaveToFileMethodName
        {
            get { return this.saveToFileMethodNameField; }
            set { this.saveToFileMethodNameField = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the name of SaveToFile method.
        /// </summary>
        [CategoryAttribute("Serialize"), DefaultValue("LoadFromFile"), DescriptionAttribute("The name of load from xml file method.")]
        public string LoadFromFileMethodName
        {
            get { return this.loadFromFileMethodNameField; }
            set { this.loadFromFileMethodNameField = value; }
        }

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="xsdFilePath">The XSD file path.</param>
        /// <returns>GeneratorParams instance</returns>
        public static GeneratorParams LoadFromFile(string xsdFilePath)
        {
            string outFile;
            return LoadFromFile(xsdFilePath, out outFile);
        }

        /// <summary>
        /// Loads from file.
        /// </summary>
        /// <param name="xsdFilePath">The XSD file path.</param>
        /// <param name="outputFile">The output file.</param>
        /// <returns>GeneratorParams instance</returns>
        public static GeneratorParams LoadFromFile(string xsdFilePath, out string outputFile)
        {
            GeneratorParams parameters = new GeneratorParams();

            // TODO:Change this to default project code langage.
            #region Search generationFile
            outputFile = string.Empty;
            
            string csharpFileName = xsdFilePath.Replace(".xsd", ".Designer.cs");
            FileInfo csharpFile = new FileInfo(csharpFileName);
            if (csharpFile.Exists)
            {
                outputFile = csharpFileName;
            }
            else
            {
                string visualBasicFileName = xsdFilePath.Replace(".xsd", ".Designer.vb");
                FileInfo visualBasicFile = new FileInfo(visualBasicFileName);
                if (visualBasicFile.Exists)
                {
                    outputFile = visualBasicFileName;
                }
                else
                {
                    string cppFileName = xsdFilePath.Replace(".xsd", ".Designer.cpp");
                    FileInfo cppFile = new FileInfo(cppFileName);
                    if (cppFile.Exists)
                    {
                        outputFile = cppFileName;
                    }
                }
            }

            if (outputFile.Length == 0)
            {
                return null;
            }

            #endregion

            #region Try to get Last options
            using (TextReader streamReader = new StreamReader(outputFile))
            {
                // TODO:Change this to search method
                streamReader.ReadLine();
                streamReader.ReadLine();
                streamReader.ReadLine();
                string optionLine = streamReader.ReadLine();
                if (optionLine != null)
                {
                    parameters.NameSpace = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.NameSpaceTag);
                    parameters.CollectionObjectType = GeneratorContext.ToCollectionType(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.CollectionTag));
                    parameters.Language = GeneratorContext.ToGenerateCodeType(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.CodeTypeTag));
                    parameters.EnableDataBinding = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.EnableDataBindingTag));
                    parameters.HidePrivateFieldInIde = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.HidePrivateFieldTag));
                    parameters.EnableSummaryComment = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.EnableSummaryCommentTag));
                    parameters.IncludeSerializeMethod = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.IncludeSerializeMethodTag));
                    parameters.DisableDebug = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.DisableDebugTag));

                    string str = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.SerializeMethodNameTag);
                    if (str.Length > 0)
                    {
                        parameters.SerializeMethodName = str;
                    }
                    else
                    {
                        parameters.SerializeMethodName = "Serialize";
                    }

                    str = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.DeserializeMethodNameTag);
                    if (str.Length > 0)
                    {
                        parameters.DeserializeMethodName = str;
                    }
                    else
                    {
                        parameters.DeserializeMethodName = "Deserialize";
                    }

                    str = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.SaveToFileMethodNameTag);
                    if (str.Length > 0)
                    {
                        parameters.SaveToFileMethodName = str;
                    }
                    else
                    {
                        parameters.SaveToFileMethodName = "SaveToFile";
                    }

                    str = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.LoadFromFileMethodNameTag);
                    if (str.Length > 0)
                    {
                        parameters.LoadFromFileMethodName = str;
                    }
                    else
                    {
                        parameters.LoadFromFileMethodName = "LoadFromFile";
                    }

                    // TODO:get custom using
                    string customUsingString = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.CustomUsingsTag);
                    if (!string.IsNullOrEmpty(customUsingString))
                    {
                        string[] usings = customUsingString.Split(';');
                        foreach (var item in usings)
                        {
                            parameters.CustomUsings.Add(new NamespaceParam() { NameSpace = item });
                        }
                    }

                    parameters.CollectionBase = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.CollectionBaseTag);
                }
            }

            return parameters;
            #endregion
        }

        /// <summary>
        /// Gets the params.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>GeneratorParams instance</returns>
        public static GeneratorParams GetParams(string parameters)
        {
            GeneratorParams newparams = new GeneratorParams();

            return newparams;
        }

        /// <summary>
        /// Save values into xml string
        /// </summary>
        /// <returns>xml string value</returns>
        public string ToXmlTag()
        {
            string optionsLine;
            optionsLine = XmlHelper.InsertXMLFromStr(GeneratorContext.NameSpaceTag, this.NameSpace);
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CollectionTag, this.CollectionObjectType.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CodeTypeTag, this.Language.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.EnableDataBindingTag, this.EnableDataBinding.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.HidePrivateFieldTag, this.HidePrivateFieldInIde.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.EnableSummaryCommentTag, this.EnableSummaryComment.ToString());

            if (!string.IsNullOrEmpty(this.CollectionBase))
            {
                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CollectionBaseTag, this.CollectionBase.ToString());
            }

            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.IncludeSerializeMethodTag, this.IncludeSerializeMethod.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.SerializeMethodNameTag, this.SerializeMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.DeserializeMethodNameTag, this.DeserializeMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.SaveToFileMethodNameTag, this.SaveToFileMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.LoadFromFileMethodNameTag, this.LoadFromFileMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.DisableDebugTag, this.DisableDebug.ToString());

            string customUsingsStr = string.Empty;
            if (this.CustomUsings != null)
            {
                foreach (var usingStr in this.CustomUsings)
                {
                    if (usingStr.NameSpace.Length > 0)
                    {
                        customUsingsStr += usingStr.NameSpace + ";";
                    }
                }

                // remove last ";"
                if (customUsingsStr.Length > 0)
                {
                    if (customUsingsStr[customUsingsStr.Length - 1] == ';')
                    {
                        customUsingsStr = customUsingsStr.Substring(0, customUsingsStr.Length - 1);
                    }
                }

                optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CustomUsingsTag, customUsingsStr);
            }

            return optionsLine;
        }
    }
}
