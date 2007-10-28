using System.CodeDom;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using Xsd2Code.Library.Extensions;

namespace Xsd2Code.Library
{
    public sealed class Processor
    {
        public const string ExtensionNamespace = "http://www.axilog.fr";
        private static readonly XPathExpression Extensions;

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

        /// <summary>
        /// Processes the schema.
        /// </summary>
        /// <param name="xsdFile">The full path to the WXS file to process.</param>
        /// <param name="targetNamespace">The namespace to put generated classes in.</param>
        /// <returns>The CodeDom tree generated from the schema.</returns>
        public static CodeNamespace Process(string xsdFile, string targetNamespace)
        {
            // Charge le schéma
            XmlSchema xsd;
            using (FileStream fs = new FileStream(xsdFile, FileMode.Open))
            {
                xsd = XmlSchema.Read(fs, null);
#pragma warning disable 612,618
                xsd.Compile(null);
#pragma warning restore 612,618
            }

            XmlSchemas schemas = new XmlSchemas();
            schemas.Add(xsd);

            // Création de l'importer pour ce Create the importer for these schemas.
            XmlSchemaImporter importer = new XmlSchemaImporter(schemas);

            // System.CodeDom namespace for the XmlCodeExporter to put classes in.
            CodeNamespace ns = new CodeNamespace();

            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Diagnostics"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
            ns.Imports.Add(new CodeNamespaceImport("System.Xml.Serialization"));
            ns.Imports.Add(new CodeNamespaceImport("System.Collections"));
            ns.Imports.Add(new CodeNamespaceImport("System.Xml.Schema"));
            ns.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            ns.Name = targetNamespace;

            XmlCodeExporter exporter = new XmlCodeExporter(ns);
            foreach (XmlSchemaElement element in xsd.Elements.Values)
            {
                // Export du mapping.
                XmlTypeMapping mapping = importer.ImportTypeMapping(element.QualifiedName);

                // Export du code
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