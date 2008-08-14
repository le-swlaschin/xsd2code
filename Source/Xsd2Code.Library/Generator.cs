//-----------------------------------------------------------------------
// <copyright file="Generator.cs" company="Xsd2Code">
//     copyright Pascal Cabanel.
// </copyright>
//-----------------------------------------------------------------------

namespace Xsd2Code
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using Xsd2Code.Library.Extensions;
    using Xsd2Code.Library;

    /// <summary>
    /// Generator process
    /// </summary>
    public sealed class Generator
    {
        /// <summary>
        /// Extension Namespace const
        /// </summary>
        public const string ExtensionNamespace = "http://www.myxaml.fr";

        /// <summary>
        /// Process code generation
        /// </summary>
        /// <param name="xsdFile">Full xsd file path</param>
        /// <param name="targetNamespace">taget namespace</param>
        /// <param name="language">generation language</param>
        /// <param name="collectionType">collection type</param>
        /// <param name="enableDataBinding">Indicates whether the generation implement the change notification</param>
        /// <param name="hidePrivate">Indicates if generate EditorBrowsableState.Never attribute</param>
        /// <param name="enableSummaryComment">Generate summary comment from schema annotation</param>
        /// <param name="errorMessage">Output error messsage</param>
        /// <returns>if sucess, represents a namespace declaration else the return value is null</returns>
        internal static CodeNamespace Process(string xsdFile, string targetNamespace, GenerationLanguage language, CollectionType collectionType, bool enableDataBinding, bool hidePrivate, bool enableSummaryComment, out string errorMessage)
        {
            errorMessage = "";
            try
            {
                #region Set generation context
                XmlSchema xsd;
                GeneratorContext.CollectionObjectType = collectionType;
                GeneratorContext.EnableDataBinding = enableDataBinding;
                GeneratorContext.HidePrivateFieldInIde = hidePrivate;
                GeneratorContext.Language = language;
                GeneratorContext.EnableSummaryComment = enableSummaryComment;

                using (FileStream fs = new FileStream(xsdFile, FileMode.Open))
                {
                    xsd = XmlSchema.Read(fs, null);
                    XmlSchemaSet schemaSet = new XmlSchemaSet();
                    schemaSet.Add(xsd);
                    schemaSet.Compile();
                }

                XmlSchemas schemas = new XmlSchemas();
                schemas.Add(xsd);
                #endregion

                #region namespace
                CodeNamespace ns = new CodeNamespace();
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
                ns.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));
                ns.Imports.Add(new CodeNamespaceImport("System.Collections"));
                ns.Imports.Add(new CodeNamespaceImport("System.Xml.Schema"));
                ns.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));

                switch (GeneratorContext.CollectionObjectType)
                {
                    case CollectionType.List:
                        ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                        break;
                    case CollectionType.ObservableCollection:
                        ns.Imports.Add(new CodeNamespaceImport("System.Collections.ObjectModel"));
                        break;
                    default:
                        break;
                }

                ns.Name = targetNamespace;
                #endregion

                #region Get XmlTypeMapping
                XmlCodeExporter exporter = new XmlCodeExporter(ns);
                
                XmlSchemaImporter importer = new XmlSchemaImporter(schemas);

                foreach (XmlSchemaElement element in xsd.Elements.Values)
                {
                    XmlTypeMapping mapping = importer.ImportTypeMapping(element.QualifiedName);
                    exporter.ExportTypeMapping(mapping);
                }

                #endregion

                #region Execute extensions
                GeneratorExtension ext = new GeneratorExtension();
                ext.Process(ns, xsd);
                #endregion Execute extensions
                return ns;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                return null;
            }
        }
    }
}
