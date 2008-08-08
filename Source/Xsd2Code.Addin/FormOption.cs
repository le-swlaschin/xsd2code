using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Xsd2Code.Library;
using System.IO;

namespace Xsd2Code.Addin
{
    public partial class FormOption : Form
    {
        #region Private Field
        private string _InputFile;
        private string _NameSpace;
        private GenerateCodeType _GenerateCodeType;
        private CollectionType _CollectionType;
        private string _OutputFile;
        private bool _UseIPropertyNotifyChanged;
        private bool _HidePrivateFieldInIDE;
        #endregion

        #region Property
        public string InputFile
        {
            get { return _InputFile; }
            set { _InputFile = value; }
        }

        public string OutputFile
        {
            get { return _OutputFile; }
            set { _OutputFile = value; }
        }

        public string NameSpace
        {
            get { return _NameSpace; }
            set
            {
                _NameSpace = value;
                txtNameSpace.Text = value;
            }

        }
        public GenerateCodeType GenerateCodeType
        {
            get { return _GenerateCodeType; }
            set
            {
                _GenerateCodeType = value;
                cbxCodeType.Text = _GenerateCodeType.ToString();
            }
        }

        public CollectionType CollectionType
        {
            get { return _CollectionType; }
            set
            {
                _CollectionType = value;
                cbxCollection.Text = _CollectionType.ToString();
            }
        }

        public bool UseIPropertyNotifyChanged
        {
            get { return _UseIPropertyNotifyChanged; }
            set
            {
                _UseIPropertyNotifyChanged = value;
                chkIPropertyNotifyChanged.Checked = _UseIPropertyNotifyChanged;
            }
        }

        public bool HidePrivateFieldInIDE
        {
            get { return _HidePrivateFieldInIDE; }
            set
            {
                _HidePrivateFieldInIDE = value;
                chkHideInIDE.Checked = _HidePrivateFieldInIDE;
            }
        }

        #endregion

        #region cTor
        /// <summary>
        /// Constructor
        /// </summary>
        public FormOption()
        {
            InitializeComponent();
            cbxCollection.Items.Add(CollectionType.List.ToString());
            cbxCollection.Items.Add(CollectionType.ObservableCollection.ToString());
            cbxCollection.Items.Add(CollectionType.Array.ToString());
            cbxCodeType.Items.Add(GenerateCodeType.CSharp.ToString());
            cbxCodeType.Items.Add(GenerateCodeType.VisualBasic.ToString());
        }
        #endregion

        #region Method
        /// <summary>
        /// Analyse file to find generation option.
        /// </summary>
        public void Init(string XSDFilePath)
        {
            #region Search generationFile
            _OutputFile = string.Empty;
            string csFileName = XSDFilePath.Replace(".xsd", ".cs");
            FileInfo CsFile = new FileInfo(csFileName);
            if (CsFile.Exists)
            {
                OutputFile = csFileName;
            }
            else
            {
                string vbFileName = XSDFilePath.Replace(".xsd", ".vb");
                FileInfo vbFile = new FileInfo(csFileName);
                if (CsFile.Exists)
                {
                    OutputFile = csFileName;
                }
            }
            if (OutputFile.Length == 0)
                return;

            #endregion

            #region Try to get Last options
            using (TextReader streamReader = new StreamReader(OutputFile))
            {
                streamReader.ReadLine();
                streamReader.ReadLine();
                streamReader.ReadLine();
                string optionLine = streamReader.ReadLine();
                if (optionLine != null)
                {
                    NameSpace = XmlHerper.ExtractStrFromXML(optionLine, OptionsContext.NameSpaceTag);
                    CollectionType = OptionsContext.ToCollectionType(XmlHerper.ExtractStrFromXML(optionLine, OptionsContext.CollectionTag));
                    GenerateCodeType = OptionsContext.ToGenerateCodeType(XmlHerper.ExtractStrFromXML(optionLine, OptionsContext.codeTypeTag));
                    UseIPropertyNotifyChanged = OptionsContext.ToBoolean(XmlHerper.ExtractStrFromXML(optionLine, OptionsContext.EnableDataBindingTag));
                    HidePrivateFieldInIDE = OptionsContext.ToBoolean(XmlHerper.ExtractStrFromXML(optionLine, OptionsContext.EnableDataBindingTag));
                }
            }
            #endregion

        }
        /// <summary>
        /// Cancel the validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Validate the generation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            #region Validate input
            if (txtNameSpace.Text.Length == 0)
            {
                MessageBox.Show("you must specify the NameSpace");
                return;
            }

            if (cbxCollection.Text.Length == 0)
            {
                MessageBox.Show("you must specify the collection type");
                return;
            }

            if (cbxCodeType.Text.Length == 0)
            {
                MessageBox.Show("you must specify the code type");
                return;
            }

            #endregion

            #region SetProperty
            _NameSpace = txtNameSpace.Text;

            if (cbxCollection.Text == CollectionType.Array.ToString())
                _CollectionType = CollectionType.Array;

            if (cbxCollection.Text == CollectionType.List.ToString())
                _CollectionType = CollectionType.List;

            if (cbxCollection.Text == CollectionType.ObservableCollection.ToString())
                _CollectionType = CollectionType.ObservableCollection;

            if (cbxCodeType.Text == GenerateCodeType.CSharp.ToString())
                _GenerateCodeType = GenerateCodeType.CSharp;

            if (cbxCodeType.Text == GenerateCodeType.VisualBasic.ToString())
                _GenerateCodeType = GenerateCodeType.VisualBasic;

            _UseIPropertyNotifyChanged = chkIPropertyNotifyChanged.Checked;
            _HidePrivateFieldInIDE = chkHideInIDE.Checked;
            #endregion

            this.DialogResult = DialogResult.OK;
            Close();
        }
        #endregion
    }
}
