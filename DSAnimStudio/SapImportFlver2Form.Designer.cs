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
            this.flver2_checkBoxKeepExistingDummyPoly = new System.Windows.Forms.CheckBox();
            this.flver2_checkBoxConvertFromZUp = new System.Windows.Forms.CheckBox();
            this.buttonSaveImportedData = new System.Windows.Forms.Button();
            this.buttonRestoreBackups = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.flver2_numericUpDownScale)).BeginInit();
            this.groupBoxSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonImport.Location = new System.Drawing.Point(68, 169);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(123, 42);
            this.buttonImport.TabIndex = 0;
            this.buttonImport.Text = "IMPORT FBX TO DS ANIM STUDIO";
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
            this.textBoxFBX.Size = new System.Drawing.Size(480, 20);
            this.textBoxFBX.TabIndex = 2;
            // 
            // buttonBrowseFBX
            // 
            this.buttonBrowseFBX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseFBX.Location = new System.Drawing.Point(553, 13);
            this.buttonBrowseFBX.Name = "buttonBrowseFBX";
            this.buttonBrowseFBX.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseFBX.TabIndex = 3;
            this.buttonBrowseFBX.Text = "Browse...";
            this.buttonBrowseFBX.UseVisualStyleBackColor = true;
            this.buttonBrowseFBX.Click += new System.EventHandler(this.buttonBrowseFBX_Click);
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
            this.groupBoxSettings.Controls.Add(this.flver2_checkBoxKeepExistingDummyPoly);
            this.groupBoxSettings.Controls.Add(this.flver2_checkBoxConvertFromZUp);
            this.groupBoxSettings.Controls.Add(this.flver2_numericUpDownScale);
            this.groupBoxSettings.Controls.Add(this.label2);
            this.groupBoxSettings.Location = new System.Drawing.Point(15, 40);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(613, 123);
            this.groupBoxSettings.TabIndex = 6;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Import Settings";
            // 
            // flver2_checkBoxKeepExistingDummyPoly
            // 
            this.flver2_checkBoxKeepExistingDummyPoly.AutoSize = true;
            this.flver2_checkBoxKeepExistingDummyPoly.Location = new System.Drawing.Point(183, 76);
            this.flver2_checkBoxKeepExistingDummyPoly.Name = "flver2_checkBoxKeepExistingDummyPoly";
            this.flver2_checkBoxKeepExistingDummyPoly.Size = new System.Drawing.Size(147, 17);
            this.flver2_checkBoxKeepExistingDummyPoly.TabIndex = 7;
            this.flver2_checkBoxKeepExistingDummyPoly.Text = "Keep existing DummyPoly";
            this.flver2_checkBoxKeepExistingDummyPoly.UseVisualStyleBackColor = true;
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
            // buttonSaveImportedData
            // 
            this.buttonSaveImportedData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSaveImportedData.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSaveImportedData.Location = new System.Drawing.Point(197, 169);
            this.buttonSaveImportedData.Name = "buttonSaveImportedData";
            this.buttonSaveImportedData.Size = new System.Drawing.Size(256, 42);
            this.buttonSaveImportedData.TabIndex = 7;
            this.buttonSaveImportedData.Text = "SAVE IMPORTED FBX TO GAME DATA (and save *.dsibak backups)";
            this.buttonSaveImportedData.UseVisualStyleBackColor = true;
            this.buttonSaveImportedData.Click += new System.EventHandler(this.buttonSaveImportedData_Click);
            // 
            // buttonRestoreBackups
            // 
            this.buttonRestoreBackups.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRestoreBackups.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRestoreBackups.Location = new System.Drawing.Point(459, 169);
            this.buttonRestoreBackups.Name = "buttonRestoreBackups";
            this.buttonRestoreBackups.Size = new System.Drawing.Size(169, 42);
            this.buttonRestoreBackups.TabIndex = 8;
            this.buttonRestoreBackups.Text = "RESTORE BACKUPS (*.dsibak)";
            this.buttonRestoreBackups.UseVisualStyleBackColor = true;
            this.buttonRestoreBackups.Click += new System.EventHandler(this.buttonRestoreBackups_Click);
            // 
            // SapImportFlver2Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 223);
            this.Controls.Add(this.buttonRestoreBackups);
            this.Controls.Add(this.buttonSaveImportedData);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.buttonBrowseFBX);
            this.Controls.Add(this.textBoxFBX);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonImport);
            this.MinimumSize = new System.Drawing.Size(600, 250);
            this.Name = "SapImportFlver2Form";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Character Model From FBX";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SapImportFlver2Form_Load);
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
        private System.Windows.Forms.Button buttonSaveImportedData;
        private System.Windows.Forms.CheckBox flver2_checkBoxKeepExistingDummyPoly;
        private System.Windows.Forms.Button buttonRestoreBackups;
    }
}