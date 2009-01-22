using System;
using System.Collections.Generic;
using System.Text;
using Xsd2Code.Helpers;

namespace Xsd2Code.Library
{
    public class GeneratorParams
    {
        /// <summary>
        /// Gets or sets the name space.
        /// </summary>
        /// <value>The name space.</value>
        public string NameSpace
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the output file path.
        /// </summary>
        /// <value>The output file path.</value>
        public string OutputFilePath
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the input file path.
        /// </summary>
        /// <value>The input file path.</value>
        public string InputFilePath
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets collection type to use for code generation
        /// </summary>
        public CollectionType CollectionObjectType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if implement INotifyPropertyChanged
        /// </summary>
        public bool EnableDataBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether serialize/deserialize method support
        /// </summary>
        public bool IncludeSerializeMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the name of Serialize method.
        /// </summary>
        public string SerializeMethodName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the name of Deserialize method.
        /// </summary>
        public string DeserializeMethodName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if generate EditorBrowsableState.Never attribute
        /// </summary>
        public bool HidePrivateFieldInIde
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the name of Serialize method.
        /// </summary>
        public string SaveToFileMethodName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [disable debug].
        /// </summary>
        /// <value><c>true</c> if [disable debug]; otherwise, <c>false</c>.</value>
        public bool DisableDebug
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating the name of SaveToFile method.
        /// </summary>
        public string LoadFromFileMethodName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if generate summary documentation
        /// </summary>
        public bool EnableSummaryComment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets generation language
        /// </summary>
        public GenerationLanguage Language
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection base
        /// </summary>
        public string CollectionBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets custom usings
        /// </summary>
        public string CustomUsings
        {
            get;
            set;
        }

        public static GeneratorParams GetParams(string parameters)
        {
            GeneratorParams newparams = new GeneratorParams();
            
            return newparams;
        }
            
            
        public string ToXmlTag()
        {
            string optionsLine;
            optionsLine = XmlHelper.InsertXMLFromStr(GeneratorContext.NameSpaceTag, this.NameSpace);
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CollectionTag, this.CollectionObjectType.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CodeTypeTag, this.Language.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.EnableDataBindingTag, this.EnableDataBinding.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.HidePrivateFieldTag, this.HidePrivateFieldInIde.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.EnableSummaryCommentTag, this.EnableSummaryComment.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CustomUsingsTag, this.CustomUsings.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.CollectionBaseTag, this.CollectionBase.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.IncludeSerializeMethodTag, this.IncludeSerializeMethod.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.SerializeMethodNameTag, this.SerializeMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.DeserializeMethodNameTag, this.DeserializeMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.SaveToFileMethodNameTag, this.SaveToFileMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.LoadFromFileMethodNameTag, this.LoadFromFileMethodName.ToString());
            optionsLine += XmlHelper.InsertXMLFromStr(GeneratorContext.DisableDebugTag, this.DisableDebug.ToString());
            return optionsLine;
        }
    }
}
