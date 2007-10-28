using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Xsd2Code.Library;

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
            try
            {
                CodeNamespace ns = Processor.Process(args[0], args[1]);

            // Création du provider de génération de code
            CodeDomProvider provider;
            if (args[3] == "cs")
                provider = new Microsoft.CSharp.CSharpCodeProvider();
            else if (args[3] == "vb")
                provider = new Microsoft.VisualBasic.VBCodeProvider();
            else
                throw new ArgumentException("invalid language", args[3]);

            // Ecriture dans le fichier de sortie
            using (StreamWriter sw = new StreamWriter(args[2], false))
            {
                provider.GenerateCodeFromNamespace(ns, sw, new CodeGeneratorOptions());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        }
    }
}
