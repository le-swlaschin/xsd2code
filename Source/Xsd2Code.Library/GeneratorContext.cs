namespace Xsd2Code.Library
{
    /// <summary>
    /// Static generation context 
    /// </summary>
    /// <remarks>
    /// Revision history:
    /// 
    ///     Modified 2009-02-20 by Ruslan Urban
    ///     Added CodeBaseTag and GenerateCloneMethodTag constants
    /// 
    /// </remarks>
    public static class GeneratorContext
    {
        /// <summary>
        /// Enable serialize/deserialize generation method
        /// </summary>
        public const string CodeBaseTag = "CodeBaseTag";

        /// <summary>
        /// Language tag
        /// </summary>
        public const string CodeTypeTag = "codeType";

        /// <summary>
        /// Base generic type for collections when CollectionType is DefinedType
        /// </summary>
        public const string CollectionBaseTag = "CollectionBase";

        /// <summary>
        /// Collection tag
        /// </summary>
        public const string CollectionTag = "Collection";

        /// <summary>
        /// List of custom using/import types (, separated)
        /// </summary>
        public const string CustomUsingsTag = "CustomUsings";

        /// <summary>
        /// Name of Deserialize method
        /// </summary>
        public const string DeserializeMethodNameTag = "DeserializeMethodName";

        /// <summary>
        /// Disable debug in genetated code.
        /// </summary>
        public const string DisableDebugTag = "DisableDebug";

        /// <summary>
        /// Enable data binding tag
        /// </summary>
        public const string EnableDataBindingTag = "EnableDataBinding";

        /// <summary>
        /// Generate summary documentation
        /// </summary>
        public const string EnableSummaryCommentTag = "EnableSummaryComment";

        /// <summary>
        /// Hide private fields in IDE
        /// </summary>
        public const string HidePrivateFieldTag = "HidePrivateFieldInIDE";

        /// <summary>
        /// Enable generation of the Clone method
        /// </summary>
        public const string GenerateCloneMethodTag = "GenerateCloneMethod";

        /// <summary>
        /// Enable serialize/deserialize generation method
        /// </summary>
        public const string IncludeSerializeMethodTag = "IncludeSerializeMethod";

        /// <summary>
        /// Name of Deserialize method
        /// </summary>
        public const string LoadFromFileMethodNameTag = "LoadFromFileMethodName";

        /// <summary>
        /// namespace tag
        /// </summary>
        public const string NameSpaceTag = "NameSpace";

        /// <summary>
        /// Name of Serialize method
        /// </summary>
        public const string SaveToFileMethodNameTag = "SaveToFileMethodName";

        /// <summary>
        /// Name of Serialize method
        /// </summary>
        public const string SerializeMethodNameTag = "SerializeMethodName";


        /// <summary>
        /// Generate data contracts tag
        /// </summary>
        public const string GenerateDataContractsTag = "GenerateDataContracts";

        #region Fields

        static private GeneratorParams generatorParams = new GeneratorParams();

        #endregion

        #region property

        /// <summary>
        /// Static instance of generator parameters, accessible from within any class in the application domain
        /// </summary>
        public static GeneratorParams GeneratorParams
        {
            get { return generatorParams; }
            set { generatorParams = value; }
        }

        #endregion
    }
}