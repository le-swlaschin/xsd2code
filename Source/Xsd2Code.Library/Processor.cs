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

namespace Xsd2Code
{
    public sealed class Processor
    {
        public const string ExtensionNamespace = "http://www.myxaml.fr";

        private static XPathExpression Extensions;

        static Processor()
        {
            XPathNavigator nav = new XmlDocument().CreateNavigator();

            // Select all extension types.
            Extensions = nav.Compile("/xs:schema/xs:annotation/xs:appinfo/axi:Code/axi:Extension/@Type");

            // Create and set namespace resolution context.
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nav.NameTable);
            nsmgr.AddNamespace("xs", XmlSchema.Namespace);
            nsmgr.AddNamespace("axi", ExtensionNamespace);
            Extensions.SetContext(nsmgr);
        }

        private Processor() { }

        internal static CodeNamespace Process(string xsdFile, string targetNamespace, Xsd2Code.Library.GenerateCodeType ACodeType,  Xsd2Code.Library.CollectionType AColType, bool AUseIPropertyNotifyChanged, bool AHidePrivateFieldInIde)
        {
            // Charge le schéma
            XmlSchema xsd;
            GenrationContext.CollectionObjectType = AColType;
            GenrationContext.EnableDataBinding = AUseIPropertyNotifyChanged;
            GenrationContext.HidePrivateFieldInIde = AHidePrivateFieldInIde;
            GenrationContext.Language = ACodeType;
            using (FileStream fs = new FileStream(xsdFile, FileMode.Open))
            {
                xsd = XmlSchema.Read(fs, null);
                xsd.Compile(null);
            }
            
            XmlSchemas schemas = new XmlSchemas();
            schemas.Add(xsd);

            XmlSchemaImporter importer = new XmlSchemaImporter(schemas);

            #region NameSpace
            CodeNamespace ns = new CodeNamespace();
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            ns.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections"));
            ns.Imports.Add(new CodeNamespaceImport("System.Xml.Schema"));
            ns.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            
            switch (GenrationContext.CollectionObjectType)
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
            
            XmlCodeExporter exporter = new XmlCodeExporter(ns);
            foreach (XmlSchemaElement element in xsd.Elements.Values)
            {
                XmlTypeMapping mapping = importer.ImportTypeMapping(element.QualifiedName);
                exporter.ExportTypeMapping(mapping);
            }
            
            #region Execute extensions
            CustomExtension ext = new CustomExtension();
            ext.Process(ns, xsd);
            #endregion Execute extensions

            return ns;
        }
    }
}
