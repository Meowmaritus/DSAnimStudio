namespace DSAnimStudio.TaeEditor
{
    partial class TaeGameDirPicker
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxGameDir = new System.Windows.Forms.TextBox();
            this.textBoxModDir = new System.Windows.Forms.TextBox();
            this.buttonBrowseGameDir = new System.Windows.Forms.Button();
            this.buttonBrowseModDir = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonClearModDir = new System.Windows.Forms.Button();
            this.checkBoxLoadLooseParams = new System.Windows.Forms.CheckBox();
            this.checkBoxLoadUnpackedGameFiles = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxProjectDirectory = new System.Windows.Forms.TextBox();
            this.buttonHelpLoadLooseParams = new System.Windows.Forms.Button();
            this.buttonHelpLoadUxmFiles = new System.Windows.Forms.Button();
            this.noPaddingButtonHelpModEngindDir = new DSAnimStudio.NoPaddingButton();
            this.buttonABORT = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Game Data Directory:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 97);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(223, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "ModEngine \'/mod/\' Directory (Optional):";
            // 
            // textBoxGameDir
            // 
            this.textBoxGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGameDir.Location = new System.Drawing.Point(12, 71);
            this.textBoxGameDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxGameDir.Name = "textBoxGameDir";
            this.textBoxGameDir.Size = new System.Drawing.Size(531, 23);
            this.textBoxGameDir.TabIndex = 2;
            // 
            // textBoxModDir
            // 
            this.textBoxModDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxModDir.Location = new System.Drawing.Point(12, 115);
            this.textBoxModDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxModDir.Name = "textBoxModDir";
            this.textBoxModDir.Size = new System.Drawing.Size(450, 23);
            this.textBoxModDir.TabIndex = 3;
            // 
            // buttonBrowseGameDir
            // 
            this.buttonBrowseGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseGameDir.Location = new System.Drawing.Point(550, 71);
            this.buttonBrowseGameDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonBrowseGameDir.Name = "buttonBrowseGameDir";
            this.buttonBrowseGameDir.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseGameDir.TabIndex = 4;
            this.buttonBrowseGameDir.Text = "Browse...";
            this.buttonBrowseGameDir.UseVisualStyleBackColor = true;
            this.buttonBrowseGameDir.Click += new System.EventHandler(this.buttonBrowseGameDir_Click);
            // 
            // buttonBrowseModDir
            // 
            this.buttonBrowseModDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseModDir.Location = new System.Drawing.Point(550, 114);
            this.buttonBrowseModDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonBrowseModDir.Name = "buttonBrowseModDir";
            this.buttonBrowseModDir.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseModDir.TabIndex = 5;
            this.buttonBrowseModDir.Text = "Browse...";
            this.buttonBrowseModDir.UseVisualStyleBackColor = true;
            this.buttonBrowseModDir.Click += new System.EventHandler(this.buttonBrowseModDir_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(524, 166);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(100, 23);
            this.buttonSave.TabIndex = 6;
            this.buttonSave.Text = "Apply";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonClearModDir
            // 
            this.buttonClearModDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearModDir.Location = new System.Drawing.Point(468, 114);
            this.buttonClearModDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonClearModDir.Name = "buttonClearModDir";
            this.buttonClearModDir.Size = new System.Drawing.Size(75, 23);
            this.buttonClearModDir.TabIndex = 7;
            this.buttonClearModDir.Text = "Clear";
            this.buttonClearModDir.UseVisualStyleBackColor = true;
            this.buttonClearModDir.Click += new System.EventHandler(this.buttonClearModDir_Click);
            // 
            // checkBoxLoadLooseParams
            // 
            this.checkBoxLoadLooseParams.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxLoadLooseParams.AutoSize = true;
            this.checkBoxLoadLooseParams.Location = new System.Drawing.Point(12, 145);
            this.checkBoxLoadLooseParams.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxLoadLooseParams.Name = "checkBoxLoadLooseParams";
            this.checkBoxLoadLooseParams.Size = new System.Drawing.Size(366, 19);
            this.checkBoxLoadLooseParams.TabIndex = 8;
            this.checkBoxLoadLooseParams.Text = "Load loose GameParam instead of loading them from regulation";
            this.checkBoxLoadLooseParams.UseVisualStyleBackColor = true;
            // 
            // checkBoxLoadUnpackedGameFiles
            // 
            this.checkBoxLoadUnpackedGameFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxLoadUnpackedGameFiles.AutoSize = true;
            this.checkBoxLoadUnpackedGameFiles.Location = new System.Drawing.Point(12, 170);
            this.checkBoxLoadUnpackedGameFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxLoadUnpackedGameFiles.Name = "checkBoxLoadUnpackedGameFiles";
            this.checkBoxLoadUnpackedGameFiles.Size = new System.Drawing.Size(193, 19);
            this.checkBoxLoadUnpackedGameFiles.TabIndex = 9;
            this.checkBoxLoadUnpackedGameFiles.Text = "Load UXM unpacked game files";
            this.checkBoxLoadUnpackedGameFiles.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Project Directory:";
            // 
            // textBoxProjectDirectory
            // 
            this.textBoxProjectDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxProjectDirectory.Location = new System.Drawing.Point(12, 27);
            this.textBoxProjectDirectory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxProjectDirectory.Name = "textBoxProjectDirectory";
            this.textBoxProjectDirectory.ReadOnly = true;
            this.textBoxProjectDirectory.Size = new System.Drawing.Size(612, 23);
            this.textBoxProjectDirectory.TabIndex = 11;
            // 
            // buttonHelpLoadLooseParams
            // 
            this.buttonHelpLoadLooseParams.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonHelpLoadLooseParams.Location = new System.Drawing.Point(386, 141);
            this.buttonHelpLoadLooseParams.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonHelpLoadLooseParams.Name = "buttonHelpLoadLooseParams";
            this.buttonHelpLoadLooseParams.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpLoadLooseParams.TabIndex = 12;
            this.buttonHelpLoadLooseParams.Text = "?";
            this.buttonHelpLoadLooseParams.UseVisualStyleBackColor = true;
            this.buttonHelpLoadLooseParams.Click += new System.EventHandler(this.buttonHelpLoadLooseParams_Click);
            // 
            // buttonHelpLoadUxmFiles
            // 
            this.buttonHelpLoadUxmFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonHelpLoadUxmFiles.Location = new System.Drawing.Point(215, 166);
            this.buttonHelpLoadUxmFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonHelpLoadUxmFiles.Name = "buttonHelpLoadUxmFiles";
            this.buttonHelpLoadUxmFiles.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpLoadUxmFiles.TabIndex = 13;
            this.buttonHelpLoadUxmFiles.Text = "?";
            this.buttonHelpLoadUxmFiles.UseVisualStyleBackColor = true;
            this.buttonHelpLoadUxmFiles.Click += new System.EventHandler(this.buttonHelpLoadUxmFiles_Click);
            // 
            // noPaddingButtonHelpModEngindDir
            // 
            this.noPaddingButtonHelpModEngindDir.Location = new System.Drawing.Point(242, 94);
            this.noPaddingButtonHelpModEngindDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.noPaddingButtonHelpModEngindDir.Name = "noPaddingButtonHelpModEngindDir";
            this.noPaddingButtonHelpModEngindDir.OwnerDrawText = null;
            this.noPaddingButtonHelpModEngindDir.Size = new System.Drawing.Size(20, 21);
            this.noPaddingButtonHelpModEngindDir.TabIndex = 14;
            this.noPaddingButtonHelpModEngindDir.Text = "?";
            this.noPaddingButtonHelpModEngindDir.UseVisualStyleBackColor = true;
            this.noPaddingButtonHelpModEngindDir.Click += new System.EventHandler(this.noPaddingButtonHelpModEngindDir_Click);
            // 
            // buttonABORT
            // 
            this.buttonABORT.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonABORT.Location = new System.Drawing.Point(444, 166);
            this.buttonABORT.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonABORT.Name = "buttonABORT";
            this.buttonABORT.Size = new System.Drawing.Size(74, 23);
            this.buttonABORT.TabIndex = 15;
            this.buttonABORT.Text = "Cancel";
            this.buttonABORT.UseVisualStyleBackColor = true;
            this.buttonABORT.Click += new System.EventHandler(this.buttonABORT_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 200);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(373, 15);
            this.label4.TabIndex = 16;
            this.label4.Text = "Tip: Press Enter key to quickly accept the current values and continue.";
            // 
            // TaeGameDirPicker
            // 
            this.AcceptButton = this.buttonSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonABORT;
            this.ClientSize = new System.Drawing.Size(636, 224);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonABORT);
            this.Controls.Add(this.noPaddingButtonHelpModEngindDir);
            this.Controls.Add(this.buttonHelpLoadUxmFiles);
            this.Controls.Add(this.buttonHelpLoadLooseParams);
            this.Controls.Add(this.textBoxProjectDirectory);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBoxLoadUnpackedGameFiles);
            this.Controls.Add(this.checkBoxLoadLooseParams);
            this.Controls.Add(this.buttonClearModDir);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonBrowseModDir);
            this.Controls.Add(this.buttonBrowseGameDir);
            this.Controls.Add(this.textBoxModDir);
            this.Controls.Add(this.textBoxGameDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(600, 263);
            this.Name = "TaeGameDirPicker";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Setup Project Directories";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaeGameDirPicker_FormClosing);
            this.Load += new System.EventHandler(this.TaeGameDirPicker_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxGameDir;
        private System.Windows.Forms.TextBox textBoxModDir;
        private System.Windows.Forms.Button buttonBrowseGameDir;
        private System.Windows.Forms.Button buttonBrowseModDir;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonClearModDir;
        private System.Windows.Forms.CheckBox checkBoxLoadLooseParams;
        private System.Windows.Forms.CheckBox checkBoxLoadUnpackedGameFiles;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxProjectDirectory;
        private System.Windows.Forms.Button buttonHelpLoadLooseParams;
        private System.Windows.Forms.Button buttonHelpLoadUxmFiles;
        private NoPaddingButton noPaddingButtonHelpModEngindDir;
        private System.Windows.Forms.Button buttonABORT;
        private System.Windows.Forms.Label label4;
    }
}