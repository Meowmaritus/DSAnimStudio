namespace DSAnimStudio
{
    partial class NewChrAsmEquipForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewChrAsmEquipForm));
            this.comboBoxHD = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBoxRWeaponFlipBackwards = new System.Windows.Forms.CheckBox();
            this.checkBoxRWeaponFlipSideways = new System.Windows.Forms.CheckBox();
            this.checkBoxLWeaponFlipSideways = new System.Windows.Forms.CheckBox();
            this.checkBoxLWeaponFlipBackwards = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.comboBoxBD = new System.Windows.Forms.ComboBox();
            this.comboBoxAM = new System.Windows.Forms.ComboBox();
            this.comboBoxLG = new System.Windows.Forms.ComboBox();
            this.comboBoxWPR = new System.Windows.Forms.ComboBox();
            this.comboBoxWPL = new System.Windows.Forms.ComboBox();
            this.buttonApplyChanges = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxWPRIndex = new System.Windows.Forms.ComboBox();
            this.checkBoxDbgRHMdlPos = new System.Windows.Forms.CheckBox();
            this.checkBoxDbgLHMdlPos = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxHD
            // 
            this.comboBoxHD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxHD.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxHD.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxHD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBoxHD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxHD.ForeColor = System.Drawing.Color.White;
            this.comboBoxHD.FormattingEnabled = true;
            this.comboBoxHD.Location = new System.Drawing.Point(47, 27);
            this.comboBoxHD.Name = "comboBoxHD";
            this.comboBoxHD.Size = new System.Drawing.Size(557, 21);
            this.comboBoxHD.TabIndex = 1;
            this.comboBoxHD.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Head:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Body:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Arms:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Legs:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 139);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "R Weapon:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 194);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "L Weapon:";
            // 
            // checkBoxRWeaponFlipBackwards
            // 
            this.checkBoxRWeaponFlipBackwards.AutoSize = true;
            this.checkBoxRWeaponFlipBackwards.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxRWeaponFlipBackwards.Location = new System.Drawing.Point(90, 165);
            this.checkBoxRWeaponFlipBackwards.Name = "checkBoxRWeaponFlipBackwards";
            this.checkBoxRWeaponFlipBackwards.Size = new System.Drawing.Size(150, 17);
            this.checkBoxRWeaponFlipBackwards.TabIndex = 6;
            this.checkBoxRWeaponFlipBackwards.TabStop = false;
            this.checkBoxRWeaponFlipBackwards.Text = "R Weapon Flip Backwards";
            this.checkBoxRWeaponFlipBackwards.UseVisualStyleBackColor = true;
            this.checkBoxRWeaponFlipBackwards.CheckedChanged += new System.EventHandler(this.CheckBoxRWeaponFlipBackwards_CheckedChanged);
            // 
            // checkBoxRWeaponFlipSideways
            // 
            this.checkBoxRWeaponFlipSideways.AutoSize = true;
            this.checkBoxRWeaponFlipSideways.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxRWeaponFlipSideways.Location = new System.Drawing.Point(249, 165);
            this.checkBoxRWeaponFlipSideways.Name = "checkBoxRWeaponFlipSideways";
            this.checkBoxRWeaponFlipSideways.Size = new System.Drawing.Size(142, 17);
            this.checkBoxRWeaponFlipSideways.TabIndex = 7;
            this.checkBoxRWeaponFlipSideways.TabStop = false;
            this.checkBoxRWeaponFlipSideways.Text = "R Weapon Flip Sideways";
            this.checkBoxRWeaponFlipSideways.UseVisualStyleBackColor = true;
            this.checkBoxRWeaponFlipSideways.CheckedChanged += new System.EventHandler(this.CheckBoxRWeaponFlipSideways_CheckedChanged);
            // 
            // checkBoxLWeaponFlipSideways
            // 
            this.checkBoxLWeaponFlipSideways.AutoSize = true;
            this.checkBoxLWeaponFlipSideways.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxLWeaponFlipSideways.Location = new System.Drawing.Point(249, 220);
            this.checkBoxLWeaponFlipSideways.Name = "checkBoxLWeaponFlipSideways";
            this.checkBoxLWeaponFlipSideways.Size = new System.Drawing.Size(140, 17);
            this.checkBoxLWeaponFlipSideways.TabIndex = 10;
            this.checkBoxLWeaponFlipSideways.TabStop = false;
            this.checkBoxLWeaponFlipSideways.Text = "L Weapon Flip Sideways";
            this.checkBoxLWeaponFlipSideways.UseVisualStyleBackColor = true;
            this.checkBoxLWeaponFlipSideways.CheckedChanged += new System.EventHandler(this.CheckBoxLWeaponFlipSideways_CheckedChanged);
            // 
            // checkBoxLWeaponFlipBackwards
            // 
            this.checkBoxLWeaponFlipBackwards.AutoSize = true;
            this.checkBoxLWeaponFlipBackwards.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxLWeaponFlipBackwards.Location = new System.Drawing.Point(90, 220);
            this.checkBoxLWeaponFlipBackwards.Name = "checkBoxLWeaponFlipBackwards";
            this.checkBoxLWeaponFlipBackwards.Size = new System.Drawing.Size(148, 17);
            this.checkBoxLWeaponFlipBackwards.TabIndex = 9;
            this.checkBoxLWeaponFlipBackwards.TabStop = false;
            this.checkBoxLWeaponFlipBackwards.Text = "L Weapon Flip Backwards";
            this.checkBoxLWeaponFlipBackwards.UseVisualStyleBackColor = true;
            this.checkBoxLWeaponFlipBackwards.CheckedChanged += new System.EventHandler(this.CheckBoxLWeaponFlipBackwards_CheckedChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(616, 24);
            this.menuStrip1.TabIndex = 11;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.importToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.exportToolStripMenuItem.Text = "Export...";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.ImportToolStripMenuItem_Click);
            // 
            // comboBoxBD
            // 
            this.comboBoxBD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxBD.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxBD.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxBD.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBoxBD.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxBD.ForeColor = System.Drawing.Color.White;
            this.comboBoxBD.FormattingEnabled = true;
            this.comboBoxBD.Location = new System.Drawing.Point(47, 54);
            this.comboBoxBD.Name = "comboBoxBD";
            this.comboBoxBD.Size = new System.Drawing.Size(557, 21);
            this.comboBoxBD.TabIndex = 2;
            this.comboBoxBD.TabStop = false;
            // 
            // comboBoxAM
            // 
            this.comboBoxAM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxAM.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxAM.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxAM.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBoxAM.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxAM.ForeColor = System.Drawing.Color.White;
            this.comboBoxAM.FormattingEnabled = true;
            this.comboBoxAM.Location = new System.Drawing.Point(47, 81);
            this.comboBoxAM.Name = "comboBoxAM";
            this.comboBoxAM.Size = new System.Drawing.Size(557, 21);
            this.comboBoxAM.TabIndex = 3;
            this.comboBoxAM.TabStop = false;
            // 
            // comboBoxLG
            // 
            this.comboBoxLG.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLG.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxLG.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxLG.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBoxLG.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxLG.ForeColor = System.Drawing.Color.White;
            this.comboBoxLG.FormattingEnabled = true;
            this.comboBoxLG.Location = new System.Drawing.Point(47, 108);
            this.comboBoxLG.Name = "comboBoxLG";
            this.comboBoxLG.Size = new System.Drawing.Size(557, 21);
            this.comboBoxLG.TabIndex = 4;
            this.comboBoxLG.TabStop = false;
            // 
            // comboBoxWPR
            // 
            this.comboBoxWPR.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxWPR.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxWPR.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxWPR.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBoxWPR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxWPR.ForeColor = System.Drawing.Color.White;
            this.comboBoxWPR.FormattingEnabled = true;
            this.comboBoxWPR.Location = new System.Drawing.Point(76, 135);
            this.comboBoxWPR.Name = "comboBoxWPR";
            this.comboBoxWPR.Size = new System.Drawing.Size(528, 21);
            this.comboBoxWPR.TabIndex = 5;
            this.comboBoxWPR.TabStop = false;
            // 
            // comboBoxWPL
            // 
            this.comboBoxWPL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxWPL.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxWPL.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxWPL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBoxWPL.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxWPL.ForeColor = System.Drawing.Color.White;
            this.comboBoxWPL.FormattingEnabled = true;
            this.comboBoxWPL.Location = new System.Drawing.Point(76, 190);
            this.comboBoxWPL.Name = "comboBoxWPL";
            this.comboBoxWPL.Size = new System.Drawing.Size(528, 21);
            this.comboBoxWPL.TabIndex = 8;
            this.comboBoxWPL.TabStop = false;
            // 
            // buttonApplyChanges
            // 
            this.buttonApplyChanges.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApplyChanges.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonApplyChanges.Location = new System.Drawing.Point(11, 297);
            this.buttonApplyChanges.Name = "buttonApplyChanges";
            this.buttonApplyChanges.Size = new System.Drawing.Size(591, 23);
            this.buttonApplyChanges.TabIndex = 11;
            this.buttonApplyChanges.TabStop = false;
            this.buttonApplyChanges.Text = "Apply Changes And Reload Models";
            this.buttonApplyChanges.UseVisualStyleBackColor = true;
            this.buttonApplyChanges.Click += new System.EventHandler(this.ButtonApplyChanges_Click);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(293, 265);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(44, 13);
            this.label7.TabIndex = 12;
            this.label7.Text = "STYLE:";
            // 
            // comboBoxWPRIndex
            // 
            this.comboBoxWPRIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxWPRIndex.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.comboBoxWPRIndex.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxWPRIndex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBoxWPRIndex.ForeColor = System.Drawing.Color.White;
            this.comboBoxWPRIndex.FormattingEnabled = true;
            this.comboBoxWPRIndex.Items.AddRange(new object[] {
            "None",
            "One-Handed",
            "Left Weapon Two-Handed",
            "Right Weapon Two-Handed",
            "One-Handed (Left Weapon Transformed)",
            "One-Handed (Right Weapon Transformed)"});
            this.comboBoxWPRIndex.Location = new System.Drawing.Point(343, 260);
            this.comboBoxWPRIndex.Name = "comboBoxWPRIndex";
            this.comboBoxWPRIndex.Size = new System.Drawing.Size(231, 21);
            this.comboBoxWPRIndex.TabIndex = 14;
            this.comboBoxWPRIndex.TabStop = false;
            // 
            // checkBoxDbgRHMdlPos
            // 
            this.checkBoxDbgRHMdlPos.AutoSize = true;
            this.checkBoxDbgRHMdlPos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxDbgRHMdlPos.Location = new System.Drawing.Point(11, 265);
            this.checkBoxDbgRHMdlPos.Name = "checkBoxDbgRHMdlPos";
            this.checkBoxDbgRHMdlPos.Size = new System.Drawing.Size(125, 17);
            this.checkBoxDbgRHMdlPos.TabIndex = 15;
            this.checkBoxDbgRHMdlPos.TabStop = false;
            this.checkBoxDbgRHMdlPos.Text = "Debug RH MDL POS";
            this.checkBoxDbgRHMdlPos.UseVisualStyleBackColor = true;
            this.checkBoxDbgRHMdlPos.CheckedChanged += new System.EventHandler(this.checkBoxDbgRHMdlPos_CheckedChanged);
            // 
            // checkBoxDbgLHMdlPos
            // 
            this.checkBoxDbgLHMdlPos.AutoSize = true;
            this.checkBoxDbgLHMdlPos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxDbgLHMdlPos.Location = new System.Drawing.Point(142, 265);
            this.checkBoxDbgLHMdlPos.Name = "checkBoxDbgLHMdlPos";
            this.checkBoxDbgLHMdlPos.Size = new System.Drawing.Size(123, 17);
            this.checkBoxDbgLHMdlPos.TabIndex = 16;
            this.checkBoxDbgLHMdlPos.TabStop = false;
            this.checkBoxDbgLHMdlPos.Text = "Debug LH MDL POS";
            this.checkBoxDbgLHMdlPos.UseVisualStyleBackColor = true;
            this.checkBoxDbgLHMdlPos.CheckedChanged += new System.EventHandler(this.checkBoxDbgLHMdlPos_CheckedChanged);
            // 
            // NewChrAsmEquipForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(616, 326);
            this.Controls.Add(this.checkBoxDbgLHMdlPos);
            this.Controls.Add(this.checkBoxDbgRHMdlPos);
            this.Controls.Add(this.comboBoxWPRIndex);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.buttonApplyChanges);
            this.Controls.Add(this.comboBoxWPL);
            this.Controls.Add(this.comboBoxWPR);
            this.Controls.Add(this.comboBoxLG);
            this.Controls.Add(this.comboBoxAM);
            this.Controls.Add(this.comboBoxBD);
            this.Controls.Add(this.checkBoxLWeaponFlipSideways);
            this.Controls.Add(this.checkBoxLWeaponFlipBackwards);
            this.Controls.Add(this.checkBoxRWeaponFlipSideways);
            this.Controls.Add(this.checkBoxRWeaponFlipBackwards);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxHD);
            this.Controls.Add(this.menuStrip1);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(508, 365);
            this.Name = "NewChrAsmEquipForm";
            this.Text = "c0000 Equipment Customizer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NewChrAsmEquipForm_FormClosing);
            this.Load += new System.EventHandler(this.NewChrAsmEquipForm_Load);
            this.Shown += new System.EventHandler(this.NewChrAsmEquipForm_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxHD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBoxRWeaponFlipBackwards;
        private System.Windows.Forms.CheckBox checkBoxRWeaponFlipSideways;
        private System.Windows.Forms.CheckBox checkBoxLWeaponFlipSideways;
        private System.Windows.Forms.CheckBox checkBoxLWeaponFlipBackwards;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxBD;
        private System.Windows.Forms.ComboBox comboBoxAM;
        private System.Windows.Forms.ComboBox comboBoxLG;
        private System.Windows.Forms.ComboBox comboBoxWPR;
        private System.Windows.Forms.ComboBox comboBoxWPL;
        private System.Windows.Forms.Button buttonApplyChanges;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBoxWPRIndex;
        private System.Windows.Forms.CheckBox checkBoxDbgRHMdlPos;
        private System.Windows.Forms.CheckBox checkBoxDbgLHMdlPos;
    }
}