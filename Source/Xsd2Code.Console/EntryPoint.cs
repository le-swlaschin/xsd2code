using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Xsd2Code.Library;

namespace Xsd2Code
{
    class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Utility to generate class from XSD.");

            if (args.Length < 5)
            {
                Console.WriteLine("Xsd2Code.exe <schema>.xsd namespace <destination>.cs [CS|VB] [Array|List|ObservableCollection] [enableDataBinding|noDataBinding] [ShowPrivateInIde|HidePrivateInIde]");

                Console.WriteLine("");
                Console.WriteLine("[CS|VB]");
                Console.WriteLine("    The language to use for the generated code. Choose from 'CS', 'VB', 'JS'");

                Console.WriteLine("");
                Console.WriteLine("[Array|List|ObservableCollection]");
                Console.WriteLine("    Type of collection. Choose from 'Array', 'List', 'ObservableCollection'");
                Console.WriteLine("    to enable data binding.");

                Console.WriteLine("");
                Console.WriteLine("enableDataBinding");
                Console.WriteLine("    Implement INotifyPropertyChanged interface on all generated types");
                Console.WriteLine("    to enable data binding.");

                return;
            }

            string XSDFile = args[0];
            string NameSpace = args[1];
            string OutputFile = args[2];
            GenerationLanguage generateCodeType = GeneratorContext.ToGenerateCodeType(args[3]);
            CollectionType collectionType = GeneratorContext.ToCollectionType(args[4]);

            bool enableDataBinding = false;
            if (args.Length > 5)
                enableDataBinding = args[5] == "enableDataBinding";

            bool HidePrivateFieldInIDE = false;
            if (args.Length > 6)
                HidePrivateFieldInIDE = args[6] == "HideInIde";

            GeneratorFacade gen = new GeneratorFacade(XSDFile, NameSpace, generateCodeType, collectionType, enableDataBinding, HidePrivateFieldInIDE, false);
            string ErrorMessage = "";
            if (!gen.Process(out ErrorMessage))
            {
                Console.WriteLine(ErrorMessage);
                return;
            }
            Console.WriteLine("Finished");
        }
    }
}
