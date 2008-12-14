namespace Xsd2Code.Addin
{
    partial class FormOption
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtLoadFromFileMethodName = new System.Windows.Forms.TextBox();
            this.txtSaveToFileMethodName = new System.Windows.Forms.TextBox();
            this.lblLoadFromFile = new System.Windows.Forms.Label();
            this.lblSaveToFile = new System.Windows.Forms.Label();
            this.txtDeserializeMethodName = new System.Windows.Forms.TextBox();
            this.txtSerializeMethodName = new System.Windows.Forms.TextBox();
            this.lblDeserialize = new System.Windows.Forms.Label();
            this.lblSerialize = new System.Windows.Forms.Label();
            this.chkSerializeMethod = new System.Windows.Forms.CheckBox();
            this.chkEnableSummaryComment = new System.Windows.Forms.CheckBox();
            this.chkHideInIDE = new System.Windows.Forms.CheckBox();
            this.chkIPropertyNotifyChanged = new System.Windows.Forms.CheckBox();
            this.lblNameSpace = new System.Windows.Forms.Label();
            this.txtNameSpace = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxCodeType = new System.Windows.Forms.ComboBox();
            this.cbxCollection = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Collection = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCollectionBase = new System.Windows.Forms.TextBox();
            this.txtUsings = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lslUsing = new System.Windows.Forms.ListBox();
            this.popUsing = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddUsing = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.Collection.SuspendLayout();
            this.popUsing.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnGenerate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 487);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(456, 32);
            this.panel1.TabIndex = 2;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(369, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(288, 4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.txtLoadFromFileMethodName);
            this.groupBox1.Controls.Add(this.txtSaveToFileMethodName);
            this.groupBox1.Controls.Add(this.lblLoadFromFile);
            this.groupBox1.Controls.Add(this.lblSaveToFile);
            this.groupBox1.Controls.Add(this.txtDeserializeMethodName);
            this.groupBox1.Controls.Add(this.txtSerializeMethodName);
            this.groupBox1.Controls.Add(this.lblDeserialize);
            this.groupBox1.Controls.Add(this.lblSerialize);
            this.groupBox1.Controls.Add(this.chkSerializeMethod);
            this.groupBox1.Controls.Add(this.chkEnableSummaryComment);
            this.groupBox1.Controls.Add(this.chkHideInIDE);
            this.groupBox1.Controls.Add(this.chkIPropertyNotifyChanged);
            this.groupBox1.Location = new System.Drawing.Point(15, 259);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(429, 217);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Location = new System.Drawing.Point(21, 107);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(2, 100);
            this.panel2.TabIndex = 25;
            // 
            // txtLoadFromFileMethodName
            // 
            this.txtLoadFromFileMethodName.Enabled = false;
            this.txtLoadFromFileMethodName.Location = new System.Drawing.Point(188, 187);
            this.txtLoadFromFileMethodName.MaxLength = 50;
            this.txtLoadFromFileMethodName.Name = "txtLoadFromFileMethodName";
            this.txtLoadFromFileMethodName.Size = new System.Drawing.Size(236, 20);
            this.txtLoadFromFileMethodName.TabIndex = 24;
            this.txtLoadFromFileMethodName.Text = "LoadFromFile";
            this.txtLoadFromFileMethodName.WordWrap = false;
            this.txtLoadFromFileMethodName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateInput_KeyPress);
            // 
            // txtSaveToFileMethodName
            // 
            this.txtSaveToFileMethodName.Enabled = false;
            this.txtSaveToFileMethodName.Location = new System.Drawing.Point(188, 161);
            this.txtSaveToFileMethodName.MaxLength = 50;
            this.txtSaveToFileMethodName.Name = "txtSaveToFileMethodName";
            this.txtSaveToFileMethodName.Size = new System.Drawing.Size(236, 20);
            this.txtSaveToFileMethodName.TabIndex = 23;
            this.txtSaveToFileMethodName.Text = "SaveToFile";
            this.txtSaveToFileMethodName.WordWrap = false;
            this.txtSaveToFileMethodName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateInput_KeyPress);
            // 
            // lblLoadFromFile
            // 
            this.lblLoadFromFile.AutoSize = true;
            this.lblLoadFromFile.Location = new System.Drawing.Point(35, 190);
            this.lblLoadFromFile.Name = "lblLoadFromFile";
            this.lblLoadFromFile.Size = new System.Drawing.Size(143, 13);
            this.lblLoadFromFile.TabIndex = 22;
            this.lblLoadFromFile.Text = "Load from file method name :";
            // 
            // lblSaveToFile
            // 
            this.lblSaveToFile.AutoSize = true;
            this.lblSaveToFile.Location = new System.Drawing.Point(35, 164);
            this.lblSaveToFile.Name = "lblSaveToFile";
            this.lblSaveToFile.Size = new System.Drawing.Size(133, 13);
            this.lblSaveToFile.TabIndex = 21;
            this.lblSaveToFile.Text = "Save to file method name :";
            // 
            // txtDeserializeMethodName
            // 
            this.txtDeserializeMethodName.Enabled = false;
            this.txtDeserializeMethodName.Location = new System.Drawing.Point(188, 136);
            this.txtDeserializeMethodName.MaxLength = 50;
            this.txtDeserializeMethodName.Name = "txtDeserializeMethodName";
            this.txtDeserializeMethodName.Size = new System.Drawing.Size(235, 20);
            this.txtDeserializeMethodName.TabIndex = 19;
            this.txtDeserializeMethodName.Text = "Deserialize";
            this.txtDeserializeMethodName.WordWrap = false;
            this.txtDeserializeMethodName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateInput_KeyPress);
            // 
            // txtSerializeMethodName
            // 
            this.txtSerializeMethodName.Enabled = false;
            this.txtSerializeMethodName.Location = new System.Drawing.Point(188, 110);
            this.txtSerializeMethodName.MaxLength = 50;
            this.txtSerializeMethodName.Name = "txtSerializeMethodName";
            this.txtSerializeMethodName.Size = new System.Drawing.Size(235, 20);
            this.txtSerializeMethodName.TabIndex = 18;
            this.txtSerializeMethodName.Text = "Serialize";
            this.txtSerializeMethodName.WordWrap = false;
            this.txtSerializeMethodName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ValidateInput_KeyPress);
            // 
            // lblDeserialize
            // 
            this.lblDeserialize.AutoSize = true;
            this.lblDeserialize.Location = new System.Drawing.Point(35, 139);
            this.lblDeserialize.Name = "lblDeserialize";
            this.lblDeserialize.Size = new System.Drawing.Size(131, 13);
            this.lblDeserialize.TabIndex = 17;
            this.lblDeserialize.Text = "Deserialize method name :";
            // 
            // lblSerialize
            // 
            this.lblSerialize.AutoSize = true;
            this.lblSerialize.Location = new System.Drawing.Point(35, 113);
            this.lblSerialize.Name = "lblSerialize";
            this.lblSerialize.Size = new System.Drawing.Size(119, 13);
            this.lblSerialize.TabIndex = 16;
            this.lblSerialize.Text = "Serialize method name :";
            // 
            // chkSerializeMethod
            // 
            this.chkSerializeMethod.AutoSize = true;
            this.chkSerializeMethod.Location = new System.Drawing.Point(17, 88);
            this.chkSerializeMethod.Name = "chkSerializeMethod";
            this.chkSerializeMethod.Size = new System.Drawing.Size(202, 17);
            this.chkSerializeMethod.TabIndex = 3;
            this.chkSerializeMethod.Text = "Generate serialize/deserialize method";
            this.chkSerializeMethod.UseVisualStyleBackColor = true;
            this.chkSerializeMethod.CheckedChanged += new System.EventHandler(this.chkSerializeMethod_CheckedChanged);
            // 
            // chkEnableSummaryComment
            // 
            this.chkEnableSummaryComment.AutoSize = true;
            this.chkEnableSummaryComment.Location = new System.Drawing.Point(17, 65);
            this.chkEnableSummaryComment.Name = "chkEnableSummaryComment";
            this.chkEnableSummaryComment.Size = new System.Drawing.Size(295, 17);
            this.chkEnableSummaryComment.TabIndex = 2;
            this.chkEnableSummaryComment.Text = "Generate summary comment from XmlSchema annotation";
            this.chkEnableSummaryComment.UseVisualStyleBackColor = true;
            // 
            // chkHideInIDE
            // 
            this.chkHideInIDE.AutoSize = true;
            this.chkHideInIDE.Location = new System.Drawing.Point(17, 42);
            this.chkHideInIDE.Name = "chkHideInIDE";
            this.chkHideInIDE.Size = new System.Drawing.Size(279, 17);
            this.chkHideInIDE.TabIndex = 1;
            this.chkHideInIDE.Text = "Hide private field in IDE (EditorBrowsableState.Never)";
            this.chkHideInIDE.UseVisualStyleBackColor = true;
            // 
            // chkIPropertyNotifyChanged
            // 
            this.chkIPropertyNotifyChanged.AutoSize = true;
            this.chkIPropertyNotifyChanged.Location = new System.Drawing.Point(17, 19);
            this.chkIPropertyNotifyChanged.Name = "chkIPropertyNotifyChanged";
            this.chkIPropertyNotifyChanged.Size = new System.Drawing.Size(189, 17);
            this.chkIPropertyNotifyChanged.TabIndex = 0;
            this.chkIPropertyNotifyChanged.Text = "Implement INotifyPropertyChanged";
            this.chkIPropertyNotifyChanged.UseVisualStyleBackColor = true;
            // 
            // lblNameSpace
            // 
            this.lblNameSpace.AutoSize = true;
            this.lblNameSpace.Location = new System.Drawing.Point(24, 13);
            this.lblNameSpace.Name = "lblNameSpace";
            this.lblNameSpace.Size = new System.Drawing.Size(72, 13);
            this.lblNameSpace.TabIndex = 4;
            this.lblNameSpace.Text = "NameSpace :";
            // 
            // txtNameSpace
            // 
            this.txtNameSpace.Location = new System.Drawing.Point(116, 10);
            this.txtNameSpace.Name = "txtNameSpace";
            this.txtNameSpace.Size = new System.Drawing.Size(322, 20);
            this.txtNameSpace.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Code :";
            // 
            // cbxCodeType
            // 
            this.cbxCodeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCodeType.FormattingEnabled = true;
            this.cbxCodeType.Location = new System.Drawing.Point(116, 37);
            this.cbxCodeType.Name = "cbxCodeType";
            this.cbxCodeType.Size = new System.Drawing.Size(322, 21);
            this.cbxCodeType.TabIndex = 7;
            // 
            // cbxCollection
            // 
            this.cbxCollection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCollection.FormattingEnabled = true;
            this.cbxCollection.Location = new System.Drawing.Point(101, 22);
            this.cbxCollection.Name = "cbxCollection";
            this.cbxCollection.Size = new System.Drawing.Size(322, 21);
            this.cbxCollection.TabIndex = 8;
            this.cbxCollection.SelectionChangeCommitted += new System.EventHandler(this.cbxCollection_SelectionChangeCommitted);
            this.cbxCollection.SelectedIndexChanged += new System.EventHandler(this.cbxCollection_SelectedIndexChanged);
            this.cbxCollection.SelectedValueChanged += new System.EventHandler(this.cbxCollection_SelectedValueChanged);
            this.cbxCollection.TextUpdate += new System.EventHandler(this.cbxCollection_TextUpdate);
            this.cbxCollection.TextChanged += new System.EventHandler(this.cbxCollection_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 25);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Collection :";
            // 
            // Collection
            // 
            this.Collection.Controls.Add(this.label5);
            this.Collection.Controls.Add(this.txtCollectionBase);
            this.Collection.Controls.Add(this.cbxCollection);
            this.Collection.Controls.Add(this.label3);
            this.Collection.Location = new System.Drawing.Point(15, 65);
            this.Collection.Name = "Collection";
            this.Collection.Size = new System.Drawing.Size(429, 82);
            this.Collection.TabIndex = 14;
            this.Collection.TabStop = false;
            this.Collection.Text = "Collection";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 52);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Collection Base:";
            // 
            // txtCollectionBase
            // 
            this.txtCollectionBase.Enabled = false;
            this.txtCollectionBase.Location = new System.Drawing.Point(101, 49);
            this.txtCollectionBase.MaxLength = 250;
            this.txtCollectionBase.Name = "txtCollectionBase";
            this.txtCollectionBase.Size = new System.Drawing.Size(322, 20);
            this.txtCollectionBase.TabIndex = 15;
            this.txtCollectionBase.WordWrap = false;
            // 
            // txtUsings
            // 
            this.txtUsings.Location = new System.Drawing.Point(116, 231);
            this.txtUsings.MaxLength = 250;
            this.txtUsings.Name = "txtUsings";
            this.txtUsings.Size = new System.Drawing.Size(279, 20);
            this.txtUsings.TabIndex = 16;
            this.txtUsings.WordWrap = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(29, 234);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Custom Usings :";
            // 
            // lslUsing
            // 
            this.lslUsing.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lslUsing.ContextMenuStrip = this.popUsing;
            this.lslUsing.FormattingEnabled = true;
            this.lslUsing.Location = new System.Drawing.Point(28, 156);
            this.lslUsing.Name = "lslUsing";
            this.lslUsing.Size = new System.Drawing.Size(410, 67);
            this.lslUsing.TabIndex = 17;
            // 
            // popUsing
            // 
            this.popUsing.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem});
            this.popUsing.Name = "popUsing";
            this.popUsing.Size = new System.Drawing.Size(118, 26);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // btnAddUsing
            // 
            this.btnAddUsing.Location = new System.Drawing.Point(401, 231);
            this.btnAddUsing.Name = "btnAddUsing";
            this.btnAddUsing.Size = new System.Drawing.Size(37, 19);
            this.btnAddUsing.TabIndex = 18;
            this.btnAddUsing.Text = "add";
            this.btnAddUsing.UseVisualStyleBackColor = true;
            this.btnAddUsing.Click += new System.EventHandler(this.btnAddUsing_Click);
            // 
            // FormOption
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(456, 519);
            this.Controls.Add(this.lslUsing);
            this.Controls.Add(this.btnAddUsing);
            this.Controls.Add(this.txtUsings);
            this.Controls.Add(this.Collection);
            this.Controls.Add(this.cbxCodeType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtNameSpace);
            this.Controls.Add(this.lblNameSpace);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormOption";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Xsd2Code Generator 2.6";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FormOption_KeyPress);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.Collection.ResumeLayout(false);
            this.Collection.PerformLayout();
            this.popUsing.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblNameSpace;
        private System.Windows.Forms.TextBox txtNameSpace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbxCodeType;
        private System.Windows.Forms.ComboBox cbxCollection;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkIPropertyNotifyChanged;
        private System.Windows.Forms.CheckBox chkHideInIDE;
        private System.Windows.Forms.CheckBox chkEnableSummaryComment;
        private System.Windows.Forms.GroupBox Collection;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCollectionBase;
        private System.Windows.Forms.TextBox txtUsings;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lslUsing;
        private System.Windows.Forms.ContextMenuStrip popUsing;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.Button btnAddUsing;
        private System.Windows.Forms.TextBox txtDeserializeMethodName;
        private System.Windows.Forms.TextBox txtSerializeMethodName;
        private System.Windows.Forms.Label lblDeserialize;
        private System.Windows.Forms.Label lblSerialize;
        private System.Windows.Forms.CheckBox chkSerializeMethod;
        private System.Windows.Forms.TextBox txtLoadFromFileMethodName;
        private System.Windows.Forms.TextBox txtSaveToFileMethodName;
        private System.Windows.Forms.Label lblLoadFromFile;
        private System.Windows.Forms.Label lblSaveToFile;
        private System.Windows.Forms.Panel panel2;

    }
}