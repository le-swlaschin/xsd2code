using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Xsd2Code
{
    class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: Xsd2Code xsdfile namespace outputfile [cs|vb]");
                return;
            }

            // Récupère le namespace du schéma
            CodeNamespace ns = Processor.Process(args[0], args[1]);

            // Création du provider de génération de code
            CodeDomProvider provider;
            if (args[3] == "cs")
                provider = new Microsoft.CSharp.CSharpCodeProvider();
            else if (args[3] == "vb")
                provider = new Microsoft.VisualBasic.VBCodeProvider();
            else
                throw new ArgumentException("language invalide", args[3]);

            // Ecriture dans le fichier de sortie
            using (StreamWriter sw = new StreamWriter(args[2], false))
            {
                provider.GenerateCodeFromNamespace(ns, sw, new CodeGeneratorOptions());
            }
            Console.WriteLine("Finished");
            Console.Read();
        }
    }
}
