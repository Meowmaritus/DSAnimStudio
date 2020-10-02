namespace DSAnimStudio
{
    partial class SapImportFlver2Form
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
            this.buttonImport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFBX = new System.Windows.Forms.TextBox();
            this.buttonBrowseFBX = new System.Windows.Forms.Button();
            this.flver2_numericUpDownScale = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.flver2_checkBoxConvertFromZUp = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.flver2_numericUpDownScale)).BeginInit();
            this.groupBoxSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonImport.Location = new System.Drawing.Point(286, 166);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(143, 42);
            this.buttonImport.TabIndex = 0;
            this.buttonImport.Text = "IMPORT";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            this.buttonImport.DragEnter += new System.Windows.Forms.DragEventHandler(this.buttonImport_DragEnter);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "FBX File:";
            // 
            // textBoxFBX
            // 
            this.textBoxFBX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFBX.Location = new System.Drawing.Point(67, 14);
            this.textBoxFBX.Name = "textBoxFBX";
            this.textBoxFBX.Size = new System.Drawing.Size(281, 20);
            this.textBoxFBX.TabIndex = 2;
            // 
            // buttonBrowseFBX
            // 
            this.buttonBrowseFBX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseFBX.Location = new System.Drawing.Point(354, 13);
            this.buttonBrowseFBX.Name = "buttonBrowseFBX";
            this.buttonBrowseFBX.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseFBX.TabIndex = 3;
            this.buttonBrowseFBX.Text = "Browse...";
            this.buttonBrowseFBX.UseVisualStyleBackColor = true;
            // 
            // flver2_numericUpDownScale
            // 
            this.flver2_numericUpDownScale.DecimalPlaces = 2;
            this.flver2_numericUpDownScale.Location = new System.Drawing.Point(139, 36);
            this.flver2_numericUpDownScale.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.flver2_numericUpDownScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.flver2_numericUpDownScale.Name = "flver2_numericUpDownScale";
            this.flver2_numericUpDownScale.Size = new System.Drawing.Size(120, 20);
            this.flver2_numericUpDownScale.TabIndex = 4;
            this.flver2_numericUpDownScale.Value = new decimal(new int[] {
            1000,
            0,
            0,
            65536});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Scene Scale Percent:";
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSettings.Controls.Add(this.flver2_checkBoxConvertFromZUp);
            this.groupBoxSettings.Controls.Add(this.flver2_numericUpDownScale);
            this.groupBoxSettings.Controls.Add(this.label2);
            this.groupBoxSettings.Location = new System.Drawing.Point(15, 40);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(414, 120);
            this.groupBoxSettings.TabIndex = 6;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Import Settings";
            // 
            // flver2_checkBoxConvertFromZUp
            // 
            this.flver2_checkBoxConvertFromZUp.AutoSize = true;
            this.flver2_checkBoxConvertFromZUp.Location = new System.Drawing.Point(25, 76);
            this.flver2_checkBoxConvertFromZUp.Name = "flver2_checkBoxConvertFromZUp";
            this.flver2_checkBoxConvertFromZUp.Size = new System.Drawing.Size(152, 17);
            this.flver2_checkBoxConvertFromZUp.TabIndex = 6;
            this.flver2_checkBoxConvertFromZUp.Text = "Convert from Z-Up to Y-Up";
            this.flver2_checkBoxConvertFromZUp.UseVisualStyleBackColor = true;
            // 
            // SapImportFlver2Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 220);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.buttonBrowseFBX);
            this.Controls.Add(this.textBoxFBX);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonImport);
            this.Name = "SapImportFlver2Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Character Model From FBX";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.SapImportFlver2Form_DragDrop);
            ((System.ComponentModel.ISupportInitialize)(this.flver2_numericUpDownScale)).EndInit();
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFBX;
        private System.Windows.Forms.Button buttonBrowseFBX;
        private System.Windows.Forms.NumericUpDown flver2_numericUpDownScale;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.CheckBox flver2_checkBoxConvertFromZUp;
    }
}