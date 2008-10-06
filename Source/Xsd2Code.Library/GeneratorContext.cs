//-----------------------------------------------------------------------
// <copyright file="GeneratorContext.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

//-----------------------------------------------------------------------
// <copyright file="GeneratorContext.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

namespace Xsd2Code.Library
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Static generation context 
    /// </summary>
    public static class GeneratorContext
    {
        /// <summary>
        /// namespace tag
        /// </summary>
        public const string NameSpaceTag = "NameSpace";

        /// <summary>
        /// Collection tag
        /// </summary>
        public const string CollectionTag = "Collection";

        /// <summary>
        /// Language tag
        /// </summary>
        public const string CodeTypeTag = "codeType";

        /// <summary>
        /// Enable data binding tag
        /// </summary>
        public const string EnableDataBindingTag = "EnableDataBinding";

        /// <summary>
        /// Hide private fields in IDE
        /// </summary>
        public const string HidePrivateFieldTag = "HidePrivateFieldInIDE";

        /// <summary>
        /// Generate summary documentation
        /// </summary>
        public const string EnableSummaryCommentTag = "EnableSummaryComment";

        /// <summary>
        /// List of custom using/import types (, separated)
        /// </summary>
        public const string CustomUsingsTag = "CustomUsings";

        /// <summary>
        /// Base generic type for collections when CollectionType is DefinedType
        /// </summary>
        public const string CollectionBaseTag = "CollectionBase";

        #region property
        /// <summary>
        /// Gets or sets collection type to use for code generation
        /// </summary>
        public static CollectionType CollectionObjectType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if implement INotifyPropertyChanged
        /// </summary>
        public static bool EnableDataBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether if generate EditorBrowsableState.Never attribute
        /// </summary>
        public static bool HidePrivateFieldInIde
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether if generate summary documentation
        /// </summary>
        public static bool EnableSummaryComment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets generation language
        /// </summary>
        public static GenerationLanguage Language
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets collection base
        /// </summary>
        public static string CollectionBase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets custom usings
        /// </summary>
        public static string CustomUsings
        {
            get;
            set;
        }
        #endregion

        #region Method
        /// <summary>
        /// Convert string to CollectionType
        /// </summary>
        /// <param name="p">string to transform</param>
        /// <returns>return CollectionType object</returns>
        public static CollectionType ToCollectionType(string p)
        {
            CollectionType returnValue = CollectionType.List;
            if (p == CollectionType.Array.ToString())
            {
                returnValue = CollectionType.Array;
            }

            if (p == CollectionType.List.ToString())
            {
                returnValue = CollectionType.List;
            }

            if (p == CollectionType.ObservableCollection.ToString())
            {
                returnValue = CollectionType.ObservableCollection;
            }

            if (p == CollectionType.DefinedType.ToString())
            {
                returnValue = CollectionType.DefinedType;
            }

            return returnValue;
        }

        /// <summary>
        /// Transform string to GenerationLanguage
        /// </summary>
        /// <param name="p">language string</param>
        /// <returns>GenerationLanguage object</returns>
        public static GenerationLanguage ToGenerateCodeType(string p)
        {
            GenerationLanguage returnValue = GenerationLanguage.CSharp;
            if (p == GenerationLanguage.CSharp.ToString())
            {
                returnValue = GenerationLanguage.CSharp;
            }

            if (p == GenerationLanguage.VisualBasic.ToString())
            {
                returnValue = GenerationLanguage.VisualBasic;
            }

            return returnValue;
        }

        /// <summary>
        /// String to boolean static method 
        /// </summary>
        /// <param name="p">string to transform</param>
        /// <returns>booean result</returns>
        public static bool ToBoolean(string p)
        {
            bool res = true;
            if (p == bool.FalseString)
            {
                res = false;
            }

            return res;
        }
        #endregion
    }
}
