

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
        #region Private
        GeneratorParams generatorParamsField;
        #endregion

        #region Property
        public string InputFile
        {
            get;
            set;
        }

        public string OutputFile
        {
            get;
            set;
        }
        public GeneratorParams GeneratorParamsField
        {
            get { return generatorParamsField; }
            set { generatorParamsField = value; }
        }

        #endregion

        #region cTor
        /// <summary>
        /// Constructor
        /// </summary>
        public FormOption()
        {
            InitializeComponent();
        }
        #endregion

        #region Method
        /// <summary>
        /// Analyse file to find generation option.
        /// </summary>
        /// <param name="xsdFilePath">The XSD file path.</param>
        public void Init(string xsdFilePath)
        {
            string outputFile;
            GeneratorParams paramFieldFromFile = GeneratorParams.LoadFromFile(xsdFilePath, out outputFile);
            if (paramFieldFromFile == null)
            {
                generatorParamsField = new GeneratorParams();
            }
            else
                generatorParamsField = paramFieldFromFile;

            propertyGrid.SelectedObject = generatorParamsField;
            OutputFile = outputFile;
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
            if (string.IsNullOrEmpty(generatorParamsField.NameSpace))
            {
                MessageBox.Show("you must specify the NameSpace");
                return;
            }

            if (generatorParamsField.CollectionObjectType.ToString() == CollectionType.DefinedType.ToString())
            {
                if (string.IsNullOrEmpty(generatorParamsField.CollectionBase))
                {
                    MessageBox.Show("you must specify the custom collection base type");
                    return;
                }
            }

            if (generatorParamsField.IncludeSerializeMethod)
            {
                if (string.IsNullOrEmpty(generatorParamsField.SerializeMethodName))
                {
                    MessageBox.Show("you must specify the Serialize method name.");
                    return;
                }
                else
                {
                    if(!IsValidateMethodName(generatorParamsField.SerializeMethodName))
                    {
                        MessageBox.Show(string.Format("Serialize method name {0} is invalid.", generatorParamsField.SerializeMethodName));
                        return;
                    }
                }
                if (string.IsNullOrEmpty(generatorParamsField.DeserializeMethodName))
                {
                    MessageBox.Show("you must specify the Deserialize method name.");
                    return;
                }
                else
                {
                    if (!IsValidateMethodName(generatorParamsField.DeserializeMethodName))
                    {
                        MessageBox.Show(string.Format("Deserialize method name {0} is invalid.", generatorParamsField.DeserializeMethodName));
                        return;
                    }
                }
                if (string.IsNullOrEmpty(generatorParamsField.SaveToFileMethodName))
                {
                    MessageBox.Show("you must specify the save to xml file method name.");
                    return;
                }
                else
                {
                    if (!IsValidateMethodName(generatorParamsField.SaveToFileMethodName))
                    {
                        MessageBox.Show(string.Format("Save to file method name {0} is invalid.", generatorParamsField.SaveToFileMethodName));
                        return;
                    }
                }
                if (string.IsNullOrEmpty(generatorParamsField.LoadFromFileMethodName))
                {
                    MessageBox.Show("you must specify the load from xml file method name.");
                    return;
                }
                else
                {
                    if (!IsValidateMethodName(generatorParamsField.LoadFromFileMethodName))
                    {
                        MessageBox.Show(string.Format("Load from file method name {0} is invalid.", generatorParamsField.LoadFromFileMethodName));
                        return;
                    }
                }
            }
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
            int ascii = Convert.ToInt16(e.KeyChar);
            if (ascii == 27)
            {
                Close();
            }
        }


        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        private bool IsValidateMethodName(string value)
        {
            foreach (char item in value.ToCharArray())
            {
                int ascii = Convert.ToInt16(item);
                if ((ascii < 65 || ascii > 90) && (ascii < 97 || ascii > 122) && (ascii != 8))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

