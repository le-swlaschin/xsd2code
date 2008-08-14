

namespace Xsd2Code.Addin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Text;
    using System.Windows.Forms;
    using Xsd2Code.Library;
    using System.IO;
    using Xsd2Code.Helpers;

    public partial class FormOption : Form
    {
        #region Private Field
        private string _InputFile;
        private string _NameSpace;
        private GenerationLanguage _GenerateCodeType;
        private CollectionType _CollectionType;
        private string _OutputFile;
        private bool _UseIPropertyNotifyChanged;
        private bool _HidePrivateFieldInIDE;
        private bool _EnableSummaryComment;
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
        public GenerationLanguage GenerateCodeType
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

        public bool EnableSummaryComment
        {
            get { return _EnableSummaryComment; }
            set
            {
                _EnableSummaryComment = value;
                chkEnableSummaryComment.Checked = _EnableSummaryComment;
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
            cbxCodeType.Items.Add(GenerationLanguage.CSharp.ToString());
            cbxCodeType.Items.Add(GenerationLanguage.VisualBasic.ToString());
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
                    NameSpace = XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.NameSpaceTag);
                    CollectionType = GeneratorContext.ToCollectionType(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.CollectionTag));
                    GenerateCodeType = GeneratorContext.ToGenerateCodeType(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.CodeTypeTag));
                    UseIPropertyNotifyChanged = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.EnableDataBindingTag));
                    HidePrivateFieldInIDE = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.HidePrivateFieldTag));
                    EnableSummaryComment = GeneratorContext.ToBoolean(XmlHelper.ExtractStrFromXML(optionLine, GeneratorContext.EnableSummaryCommentTag));
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

            if (cbxCodeType.Text == GenerationLanguage.CSharp.ToString())
                _GenerateCodeType = GenerationLanguage.CSharp;

            if (cbxCodeType.Text == GenerationLanguage.VisualBasic.ToString())
                _GenerateCodeType = GenerationLanguage.VisualBasic;

            _UseIPropertyNotifyChanged = chkIPropertyNotifyChanged.Checked;
            _HidePrivateFieldInIDE = chkHideInIDE.Checked;
            _EnableSummaryComment = chkEnableSummaryComment.Checked;
            #endregion

            this.DialogResult = DialogResult.OK;
            Close();
        }
        #endregion

        /// <summary>
        /// Close form if press esc.
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">EventArgs param</param>
        private void FormOption_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                Close();
            }
        }
    }
}
