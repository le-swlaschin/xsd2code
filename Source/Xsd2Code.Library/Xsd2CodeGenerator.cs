using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Diagnostics;

namespace Xsd2Code.Library
{
    public enum GenerateCodeType { CSharp, VisualBasic };
    public enum CollectionType { Array, List, ObservableCollection };

    public class Xsd2CodeGenerator
    {
        #region Private Field
        private string _InputFile;
        private string _OutputFile;
        private string _NameSpace;
        private GenerateCodeType _CodeType;
        private CollectionType _ColType;
        private bool _UseIPropertyNotifyChanged;
        private bool _HidePrivateField;

        #endregion

        #region Property
        public string FileName
        {
            get { return _InputFile; }
        }
        public string NameSpace
        {
            get { return _NameSpace; }
        }
        public GenerateCodeType CodeType
        {
            get { return _CodeType; }
        }
        public bool UseIPropertyNotifyChanged
        {
            get { return _UseIPropertyNotifyChanged; }
        }
        #endregion

        public Xsd2CodeGenerator(string AInputFile, string ANameSpace, GenerateCodeType AcodeType, CollectionType AColType, bool AUseIPropertyNotifyChanged, bool AHidePrivateField)
        {
            _InputFile = AInputFile;
            _NameSpace = ANameSpace;
            _CodeType = AcodeType;
            _ColType = AColType;
            _UseIPropertyNotifyChanged = AUseIPropertyNotifyChanged;
            _HidePrivateField = AHidePrivateField;

            switch (AcodeType)
            {
                case GenerateCodeType.CSharp:
                    _OutputFile = AInputFile.Replace(".xsd", ".cs");
                    break;
                case GenerateCodeType.VisualBasic:
                    _OutputFile = AInputFile.Replace(".xsd", ".vb");
                    break;
            }
        }

        public bool Process(out string ErrorMessage)
        {
            CodeDomProvider provider;
            ErrorMessage = "";

            #region Change CurrentDir for include schema resolution.
            string savedCurrentDir = Directory.GetCurrentDirectory();
            FileInfo fi = new FileInfo(_InputFile);
            if (!fi.Exists)
            {
                ErrorMessage = "Faild to generate code\n";
                ErrorMessage += "Exception :\n";
                ErrorMessage += string.Format("Input file {0} not exist", _InputFile); ;
                return false;
            }
            Directory.SetCurrentDirectory(fi.Directory.FullName);
            #endregion

            try
            {
                try
                {
                    switch (_CodeType)
                    {
                        case GenerateCodeType.CSharp:
                            provider = new Microsoft.CSharp.CSharpCodeProvider();
                            break;
                        case GenerateCodeType.VisualBasic:
                            provider = new Microsoft.VisualBasic.VBCodeProvider();
                            break;
                        default:
                            provider = new Microsoft.CSharp.CSharpCodeProvider();
                            break;
                    }
                    CodeGeneratorOptions CodeOption = new CodeGeneratorOptions();

                    CodeNamespace ns = Processor.Process(_InputFile, _NameSpace, _CodeType, _ColType, _UseIPropertyNotifyChanged, _HidePrivateField);

                    using (StreamWriter sw = new StreamWriter(_OutputFile + ".tmp", false))
                    {
                        provider.GenerateCodeFromNamespace(ns, sw, new CodeGeneratorOptions());
                    }
                }

                catch (Exception e)
                {
                    ErrorMessage = "Faild to generate code\n";
                    ErrorMessage += "Exception :\n";
                    ErrorMessage += e.Message;

                    Debug.WriteLine("");
                    Debug.WriteLine("XSD2Code - ----------------------------------------------------------");
                    Debug.WriteLine("XSD2Code - " + e.Message);
                    Debug.WriteLine("XSD2Code - ----------------------------------------------------------");
                    Debug.WriteLine("");
                    return false;
                }

                FileInfo OutPutInfo = new FileInfo(_OutputFile);
                if (OutPutInfo.Exists)
                {
                    if ((OutPutInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        ErrorMessage = "Faild to generate code\n";
                        ErrorMessage += _OutputFile + " is write protect";
                        return false;
                    }
                }


                StreamWriter swend = new StreamWriter(_OutputFile, false);

                #region Insert tag for futur generation

                string CommentStr;
                CommentStr = _CodeType == GenerateCodeType.CSharp ? "// " : "'' ";

                string OptionsLine = CommentStr;
                swend.WriteLine(string.Format("{0} <auto-generated />", CommentStr));
                swend.WriteLine(string.Format("{0} ------------------------------------------------------------------------------", CommentStr));
                swend.WriteLine(string.Format("{0} XSD2Code generation options. ** Must be at the begining of the file **", CommentStr));
                OptionsLine += XmlHerper.InsertXMLFromStr(OptionsContext.NameSpaceTag, _NameSpace);
                OptionsLine += XmlHerper.InsertXMLFromStr(OptionsContext.CollectionTag, _ColType.ToString());
                OptionsLine += XmlHerper.InsertXMLFromStr(OptionsContext.codeTypeTag, _CodeType.ToString());
                OptionsLine += XmlHerper.InsertXMLFromStr(OptionsContext.EnableDataBindingTag, _UseIPropertyNotifyChanged.ToString());
                OptionsLine += XmlHerper.InsertXMLFromStr(OptionsContext.HidePrivateFieldTag, _HidePrivateField.ToString());

                swend.WriteLine(OptionsLine);
                swend.WriteLine(string.Format("{0} ------------------------------------------------------------------------------", CommentStr));
                #endregion

                string line = "";
                using (TextReader streamReader = new StreamReader(_OutputFile + ".tmp"))
                {
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        if (line.Trim() != "[System.SerializableAttribute()]" &&
                            line.Trim() != "[System.ComponentModel.DesignerCategoryAttribute(\"code\")]")
                        {
                            swend.WriteLine(line);
                        }
                    }
                }
                swend.Close();

                FileInfo tmp = new FileInfo(_OutputFile + ".tmp");
                tmp.Delete();
            }
            finally
            {
                Directory.SetCurrentDirectory(savedCurrentDir);
            }
            return true;
        }
    }
}
