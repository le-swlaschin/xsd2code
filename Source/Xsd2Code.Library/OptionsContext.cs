using System;
using System.Collections.Generic;
using System.Text;

namespace Xsd2Code.Library
{
    public static class OptionsContext
    {
        public const string NameSpaceTag = "NameSpace";
        public const string CollectionTag = "Collection";
        public const string codeTypeTag = "codeType";
        public const string EnableDataBindingTag = "EnableDataBinding";
        public const string HidePrivateFieldTag = "HidePrivateFieldInIDE";
        
        /// <summary>
        /// Convert string to CollectionType
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static CollectionType ToCollectionType(string p)
        {
            CollectionType returnValue = CollectionType.List;
            if (p == CollectionType.Array.ToString())
                returnValue = CollectionType.Array;

            if (p == CollectionType.List.ToString())
                returnValue = CollectionType.List;

            if (p == CollectionType.ObservableCollection.ToString())
                returnValue = CollectionType.ObservableCollection;
            
            return returnValue;
        }

        public static GenerateCodeType ToGenerateCodeType(string p)
        {
            GenerateCodeType returnValue = GenerateCodeType.CSharp;
            if (p == GenerateCodeType.CSharp.ToString())
                returnValue = GenerateCodeType.CSharp;

            if (p == GenerateCodeType.VisualBasic.ToString())
                returnValue = GenerateCodeType.VisualBasic;

            return returnValue;            
        }

        public static bool ToBoolean(string p)
        {
            bool Res = true;
            if (p == bool.FalseString)
                Res = false;
            return Res;
        }
    }
}
