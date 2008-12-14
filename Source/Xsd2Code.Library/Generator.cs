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
        /// Processes the specified XSD file.
        /// </summary>
        /// <param name="xsdFile">The XSD file.</param>
        /// <param name="targetNamespace">The target namespace.</param>
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
        /// <param name="errorMessage">The error message.</param>
        /// <returns>result CodeNamespace</returns>
        internal static CodeNamespace Process(string xsdFile, string targetNamespace, GenerationLanguage language, CollectionType collectionType, bool enableDataBinding, bool hidePrivate, bool enableSummaryComment, string customUsings, string collectionBase, bool includeSerializeMethod, string serializeMethodName, string deserializeMethodName, string saveToFileMethodName, string loadFromFileMethodName, out string errorMessage)
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
                GeneratorContext.CustomUsings = customUsings;
                GeneratorContext.CollectionBase = collectionBase;
                GeneratorContext.IncludeSerializeMethod = includeSerializeMethod;
                GeneratorContext.SerializeMethodName = serializeMethodName;
                GeneratorContext.DeserializeMethodName = deserializeMethodName;
                GeneratorContext.SaveToFileMethodName = saveToFileMethodName;
                GeneratorContext.LoadFromFileMethodName = loadFromFileMethodName;

                using (FileStream fs = new FileStream(xsdFile, FileMode.Open, FileAccess.Read))
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

                if (!string.IsNullOrEmpty(customUsings))
                {
                    foreach (string s in customUsings.Split(';'))
                    {
                        ns.Imports.Add(new CodeNamespaceImport(s));
                    }
                }

                if (GeneratorContext.IncludeSerializeMethod)
                {
                    ns.Imports.Add(new CodeNamespaceImport("System.IO"));
                }

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
