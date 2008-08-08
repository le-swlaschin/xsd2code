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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkHideInIDE = new System.Windows.Forms.CheckBox();
            this.chkIPropertyNotifyChanged = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtNameSpace = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbxCodeType = new System.Windows.Forms.ComboBox();
            this.cbxCollection = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.btnGenerate);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 180);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(431, 32);
            this.panel1.TabIndex = 2;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(352, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(273, 4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 2;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkHideInIDE);
            this.groupBox1.Controls.Add(this.chkIPropertyNotifyChanged);
            this.groupBox1.Location = new System.Drawing.Point(12, 97);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(407, 73);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // chkHideInIDE
            // 
            this.chkHideInIDE.AutoSize = true;
            this.chkHideInIDE.Location = new System.Drawing.Point(6, 42);
            this.chkHideInIDE.Name = "chkHideInIDE";
            this.chkHideInIDE.Size = new System.Drawing.Size(279, 17);
            this.chkHideInIDE.TabIndex = 1;
            this.chkHideInIDE.Text = "Hide private field in IDE (EditorBrowsableState.Never)";
            this.chkHideInIDE.UseVisualStyleBackColor = true;
            // 
            // chkIPropertyNotifyChanged
            // 
            this.chkIPropertyNotifyChanged.AutoSize = true;
            this.chkIPropertyNotifyChanged.Location = new System.Drawing.Point(6, 19);
            this.chkIPropertyNotifyChanged.Name = "chkIPropertyNotifyChanged";
            this.chkIPropertyNotifyChanged.Size = new System.Drawing.Size(189, 17);
            this.chkIPropertyNotifyChanged.TabIndex = 0;
            this.chkIPropertyNotifyChanged.Text = "Implement INotifyPropertyChanged";
            this.chkIPropertyNotifyChanged.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "NameSpace :";
            // 
            // txtNameSpace
            // 
            this.txtNameSpace.Location = new System.Drawing.Point(91, 5);
            this.txtNameSpace.Name = "txtNameSpace";
            this.txtNameSpace.Size = new System.Drawing.Size(328, 20);
            this.txtNameSpace.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Code :";
            // 
            // cbxCodeType
            // 
            this.cbxCodeType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCodeType.FormattingEnabled = true;
            this.cbxCodeType.Location = new System.Drawing.Point(91, 32);
            this.cbxCodeType.Name = "cbxCodeType";
            this.cbxCodeType.Size = new System.Drawing.Size(328, 21);
            this.cbxCodeType.TabIndex = 7;
            // 
            // cbxCollection
            // 
            this.cbxCollection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCollection.FormattingEnabled = true;
            this.cbxCollection.Location = new System.Drawing.Point(91, 60);
            this.cbxCollection.Name = "cbxCollection";
            this.cbxCollection.Size = new System.Drawing.Size(328, 21);
            this.cbxCollection.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Collection :";
            // 
            // FormOption
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 212);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbxCollection);
            this.Controls.Add(this.cbxCodeType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtNameSpace);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormOption";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Xsd2Code Generator";
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtNameSpace;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbxCodeType;
        private System.Windows.Forms.ComboBox cbxCollection;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkIPropertyNotifyChanged;
        private System.Windows.Forms.CheckBox chkHideInIDE;

    }
}