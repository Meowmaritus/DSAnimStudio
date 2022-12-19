namespace DSAnimStudio.TaeEditor
{
    partial class TaeLoadFromArchivesWizard
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxPathInterroot = new System.Windows.Forms.TextBox();
            this.buttonPathInterroot = new System.Windows.Forms.Button();
            this.buttonPathAnibnd = new System.Windows.Forms.Button();
            this.textBoxPathAnibnd = new System.Windows.Forms.TextBox();
            this.buttonPathChrbnd = new System.Windows.Forms.Button();
            this.textBoxPathChrbnd = new System.Windows.Forms.TextBox();
            this.buttonPathSaveLoose = new System.Windows.Forms.Button();
            this.textBoxPathSaveLoose = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonHelpSaveLoose = new System.Windows.Forms.Button();
            this.buttonHelpChrbnd = new System.Windows.Forms.Button();
            this.buttonHelpAnibnd = new System.Windows.Forms.Button();
            this.buttonHelpGameEXE = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonCreateProject = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.labelLoadingText = new System.Windows.Forms.Label();
            this.progressBarLoading = new System.Windows.Forms.ProgressBar();
            this.checkBoxLoadLooseParams = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonClearModEngineDir = new System.Windows.Forms.Button();
            this.buttonHelpCfgLoadUnpackedGameFiles = new System.Windows.Forms.Button();
            this.checkBoxCfgLoadUnpackedGameFiles = new System.Windows.Forms.CheckBox();
            this.buttonHelpCfgLoadLooseGameParam = new System.Windows.Forms.Button();
            this.buttonHelpCfgModEngineDir = new System.Windows.Forms.Button();
            this.buttonBrowseModEngineDir = new System.Windows.Forms.Button();
            this.textBoxPathModEngineDir = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "1. Choose Game EXE";
            // 
            // textBoxPathInterroot
            // 
            this.textBoxPathInterroot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPathInterroot.Location = new System.Drawing.Point(207, 5);
            this.textBoxPathInterroot.Name = "textBoxPathInterroot";
            this.textBoxPathInterroot.Size = new System.Drawing.Size(312, 23);
            this.textBoxPathInterroot.TabIndex = 1;
            this.textBoxPathInterroot.Leave += new System.EventHandler(this.textBoxPathInterroot_Leave);
            this.textBoxPathInterroot.Validating += new System.ComponentModel.CancelEventHandler(this.textBoxPathInterroot_Validating);
            this.textBoxPathInterroot.Validated += new System.EventHandler(this.textBoxPathInterroot_Validated);
            // 
            // buttonPathInterroot
            // 
            this.buttonPathInterroot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPathInterroot.Location = new System.Drawing.Point(539, 5);
            this.buttonPathInterroot.Name = "buttonPathInterroot";
            this.buttonPathInterroot.Size = new System.Drawing.Size(33, 23);
            this.buttonPathInterroot.TabIndex = 2;
            this.buttonPathInterroot.Text = "...";
            this.buttonPathInterroot.UseVisualStyleBackColor = true;
            this.buttonPathInterroot.Click += new System.EventHandler(this.buttonPathInterroot_Click);
            // 
            // buttonPathAnibnd
            // 
            this.buttonPathAnibnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPathAnibnd.Enabled = false;
            this.buttonPathAnibnd.Location = new System.Drawing.Point(539, 34);
            this.buttonPathAnibnd.Name = "buttonPathAnibnd";
            this.buttonPathAnibnd.Size = new System.Drawing.Size(33, 23);
            this.buttonPathAnibnd.TabIndex = 4;
            this.buttonPathAnibnd.Text = "...";
            this.buttonPathAnibnd.UseVisualStyleBackColor = true;
            this.buttonPathAnibnd.Click += new System.EventHandler(this.buttonPathAnibnd_Click);
            // 
            // textBoxPathAnibnd
            // 
            this.textBoxPathAnibnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPathAnibnd.Location = new System.Drawing.Point(207, 34);
            this.textBoxPathAnibnd.Name = "textBoxPathAnibnd";
            this.textBoxPathAnibnd.ReadOnly = true;
            this.textBoxPathAnibnd.Size = new System.Drawing.Size(312, 23);
            this.textBoxPathAnibnd.TabIndex = 3;
            this.textBoxPathAnibnd.Leave += new System.EventHandler(this.textBoxPathAnibnd_Leave);
            // 
            // buttonPathChrbnd
            // 
            this.buttonPathChrbnd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPathChrbnd.Enabled = false;
            this.buttonPathChrbnd.Location = new System.Drawing.Point(539, 63);
            this.buttonPathChrbnd.Name = "buttonPathChrbnd";
            this.buttonPathChrbnd.Size = new System.Drawing.Size(33, 23);
            this.buttonPathChrbnd.TabIndex = 6;
            this.buttonPathChrbnd.Text = "...";
            this.buttonPathChrbnd.UseVisualStyleBackColor = true;
            this.buttonPathChrbnd.Click += new System.EventHandler(this.buttonPathChrbnd_Click);
            // 
            // textBoxPathChrbnd
            // 
            this.textBoxPathChrbnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPathChrbnd.Location = new System.Drawing.Point(207, 63);
            this.textBoxPathChrbnd.Name = "textBoxPathChrbnd";
            this.textBoxPathChrbnd.ReadOnly = true;
            this.textBoxPathChrbnd.Size = new System.Drawing.Size(312, 23);
            this.textBoxPathChrbnd.TabIndex = 5;
            this.textBoxPathChrbnd.Leave += new System.EventHandler(this.textBoxPathChrbnd_Leave);
            // 
            // buttonPathSaveLoose
            // 
            this.buttonPathSaveLoose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPathSaveLoose.Enabled = false;
            this.buttonPathSaveLoose.Location = new System.Drawing.Point(539, 92);
            this.buttonPathSaveLoose.Name = "buttonPathSaveLoose";
            this.buttonPathSaveLoose.Size = new System.Drawing.Size(33, 23);
            this.buttonPathSaveLoose.TabIndex = 8;
            this.buttonPathSaveLoose.Text = "...";
            this.buttonPathSaveLoose.UseVisualStyleBackColor = true;
            this.buttonPathSaveLoose.Click += new System.EventHandler(this.buttonPathSaveLoose_Click);
            // 
            // textBoxPathSaveLoose
            // 
            this.textBoxPathSaveLoose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPathSaveLoose.Location = new System.Drawing.Point(207, 92);
            this.textBoxPathSaveLoose.Name = "textBoxPathSaveLoose";
            this.textBoxPathSaveLoose.ReadOnly = true;
            this.textBoxPathSaveLoose.Size = new System.Drawing.Size(312, 23);
            this.textBoxPathSaveLoose.TabIndex = 7;
            this.textBoxPathSaveLoose.Leave += new System.EventHandler(this.textBoxPathSaveLoose_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "2. Choose ANIBND";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(110, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "3. Choose CHRBND";
            // 
            // buttonHelpSaveLoose
            // 
            this.buttonHelpSaveLoose.Location = new System.Drawing.Point(178, 92);
            this.buttonHelpSaveLoose.Name = "buttonHelpSaveLoose";
            this.buttonHelpSaveLoose.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpSaveLoose.TabIndex = 14;
            this.buttonHelpSaveLoose.Text = "?";
            this.buttonHelpSaveLoose.UseVisualStyleBackColor = true;
            this.buttonHelpSaveLoose.Click += new System.EventHandler(this.buttonHelpSaveLoose_Click);
            // 
            // buttonHelpChrbnd
            // 
            this.buttonHelpChrbnd.Location = new System.Drawing.Point(178, 63);
            this.buttonHelpChrbnd.Name = "buttonHelpChrbnd";
            this.buttonHelpChrbnd.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpChrbnd.TabIndex = 13;
            this.buttonHelpChrbnd.Text = "?";
            this.buttonHelpChrbnd.UseVisualStyleBackColor = true;
            this.buttonHelpChrbnd.Click += new System.EventHandler(this.buttonHelpChrbnd_Click);
            // 
            // buttonHelpAnibnd
            // 
            this.buttonHelpAnibnd.Location = new System.Drawing.Point(178, 34);
            this.buttonHelpAnibnd.Name = "buttonHelpAnibnd";
            this.buttonHelpAnibnd.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpAnibnd.TabIndex = 12;
            this.buttonHelpAnibnd.Text = "?";
            this.buttonHelpAnibnd.UseVisualStyleBackColor = true;
            this.buttonHelpAnibnd.Click += new System.EventHandler(this.buttonHelpAnibnd_Click);
            // 
            // buttonHelpGameEXE
            // 
            this.buttonHelpGameEXE.Location = new System.Drawing.Point(178, 5);
            this.buttonHelpGameEXE.Name = "buttonHelpGameEXE";
            this.buttonHelpGameEXE.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpGameEXE.TabIndex = 11;
            this.buttonHelpGameEXE.Text = "?";
            this.buttonHelpGameEXE.UseVisualStyleBackColor = true;
            this.buttonHelpGameEXE.Click += new System.EventHandler(this.buttonHelpGameEXE_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(162, 15);
            this.label4.TabIndex = 15;
            this.label4.Text = "4. Choose Project Save Folder";
            // 
            // buttonCreateProject
            // 
            this.buttonCreateProject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCreateProject.Enabled = false;
            this.buttonCreateProject.Location = new System.Drawing.Point(453, 226);
            this.buttonCreateProject.Name = "buttonCreateProject";
            this.buttonCreateProject.Size = new System.Drawing.Size(119, 23);
            this.buttonCreateProject.TabIndex = 16;
            this.buttonCreateProject.Text = "Create Project";
            this.buttonCreateProject.UseVisualStyleBackColor = true;
            this.buttonCreateProject.Click += new System.EventHandler(this.buttonCreateProject_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // labelLoadingText
            // 
            this.labelLoadingText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelLoadingText.Location = new System.Drawing.Point(12, 226);
            this.labelLoadingText.Name = "labelLoadingText";
            this.labelLoadingText.Size = new System.Drawing.Size(223, 23);
            this.labelLoadingText.TabIndex = 17;
            this.labelLoadingText.Text = "Decrypting archive headers, please wait...";
            this.labelLoadingText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBarLoading
            // 
            this.progressBarLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBarLoading.Location = new System.Drawing.Point(241, 226);
            this.progressBarLoading.MarqueeAnimationSpeed = 10;
            this.progressBarLoading.Maximum = 1;
            this.progressBarLoading.Name = "progressBarLoading";
            this.progressBarLoading.Size = new System.Drawing.Size(116, 23);
            this.progressBarLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarLoading.TabIndex = 18;
            this.progressBarLoading.Value = 1;
            // 
            // checkBoxLoadLooseParams
            // 
            this.checkBoxLoadLooseParams.AutoSize = true;
            this.checkBoxLoadLooseParams.Location = new System.Drawing.Point(10, 44);
            this.checkBoxLoadLooseParams.Name = "checkBoxLoadLooseParams";
            this.checkBoxLoadLooseParams.Size = new System.Drawing.Size(151, 19);
            this.checkBoxLoadLooseParams.TabIndex = 19;
            this.checkBoxLoadLooseParams.Text = "Load loose GameParam";
            this.checkBoxLoadLooseParams.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonClearModEngineDir);
            this.groupBox1.Controls.Add(this.buttonHelpCfgLoadUnpackedGameFiles);
            this.groupBox1.Controls.Add(this.checkBoxCfgLoadUnpackedGameFiles);
            this.groupBox1.Controls.Add(this.buttonHelpCfgLoadLooseGameParam);
            this.groupBox1.Controls.Add(this.buttonHelpCfgModEngineDir);
            this.groupBox1.Controls.Add(this.buttonBrowseModEngineDir);
            this.groupBox1.Controls.Add(this.textBoxPathModEngineDir);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.checkBoxLoadLooseParams);
            this.groupBox1.Location = new System.Drawing.Point(7, 121);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(565, 99);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // buttonClearModEngineDir
            // 
            this.buttonClearModEngineDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearModEngineDir.Location = new System.Drawing.Point(501, 15);
            this.buttonClearModEngineDir.Name = "buttonClearModEngineDir";
            this.buttonClearModEngineDir.Size = new System.Drawing.Size(23, 23);
            this.buttonClearModEngineDir.TabIndex = 27;
            this.buttonClearModEngineDir.Text = "X";
            this.buttonClearModEngineDir.UseVisualStyleBackColor = true;
            this.buttonClearModEngineDir.Click += new System.EventHandler(this.buttonClearModEngineDir_Click);
            // 
            // buttonHelpCfgLoadUnpackedGameFiles
            // 
            this.buttonHelpCfgLoadUnpackedGameFiles.Location = new System.Drawing.Point(229, 68);
            this.buttonHelpCfgLoadUnpackedGameFiles.Name = "buttonHelpCfgLoadUnpackedGameFiles";
            this.buttonHelpCfgLoadUnpackedGameFiles.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpCfgLoadUnpackedGameFiles.TabIndex = 26;
            this.buttonHelpCfgLoadUnpackedGameFiles.Text = "?";
            this.buttonHelpCfgLoadUnpackedGameFiles.UseVisualStyleBackColor = true;
            this.buttonHelpCfgLoadUnpackedGameFiles.Click += new System.EventHandler(this.buttonHelpCfgLoadUnpackedGameFiles_Click);
            // 
            // checkBoxCfgLoadUnpackedGameFiles
            // 
            this.checkBoxCfgLoadUnpackedGameFiles.AutoSize = true;
            this.checkBoxCfgLoadUnpackedGameFiles.Location = new System.Drawing.Point(10, 71);
            this.checkBoxCfgLoadUnpackedGameFiles.Name = "checkBoxCfgLoadUnpackedGameFiles";
            this.checkBoxCfgLoadUnpackedGameFiles.Size = new System.Drawing.Size(168, 19);
            this.checkBoxCfgLoadUnpackedGameFiles.TabIndex = 25;
            this.checkBoxCfgLoadUnpackedGameFiles.Text = "Load Unpacked Game Files";
            this.checkBoxCfgLoadUnpackedGameFiles.UseVisualStyleBackColor = true;
            // 
            // buttonHelpCfgLoadLooseGameParam
            // 
            this.buttonHelpCfgLoadLooseGameParam.Location = new System.Drawing.Point(229, 42);
            this.buttonHelpCfgLoadLooseGameParam.Name = "buttonHelpCfgLoadLooseGameParam";
            this.buttonHelpCfgLoadLooseGameParam.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpCfgLoadLooseGameParam.TabIndex = 24;
            this.buttonHelpCfgLoadLooseGameParam.Text = "?";
            this.buttonHelpCfgLoadLooseGameParam.UseVisualStyleBackColor = true;
            this.buttonHelpCfgLoadLooseGameParam.Click += new System.EventHandler(this.buttonHelpCfgLoadLooseGameParam_Click);
            // 
            // buttonHelpCfgModEngineDir
            // 
            this.buttonHelpCfgModEngineDir.Location = new System.Drawing.Point(229, 15);
            this.buttonHelpCfgModEngineDir.Name = "buttonHelpCfgModEngineDir";
            this.buttonHelpCfgModEngineDir.Size = new System.Drawing.Size(23, 23);
            this.buttonHelpCfgModEngineDir.TabIndex = 23;
            this.buttonHelpCfgModEngineDir.Text = "?";
            this.buttonHelpCfgModEngineDir.UseVisualStyleBackColor = true;
            this.buttonHelpCfgModEngineDir.Click += new System.EventHandler(this.buttonHelpCfgModEngineDir_Click);
            // 
            // buttonBrowseModEngineDir
            // 
            this.buttonBrowseModEngineDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseModEngineDir.Location = new System.Drawing.Point(526, 15);
            this.buttonBrowseModEngineDir.Name = "buttonBrowseModEngineDir";
            this.buttonBrowseModEngineDir.Size = new System.Drawing.Size(33, 23);
            this.buttonBrowseModEngineDir.TabIndex = 22;
            this.buttonBrowseModEngineDir.Text = "...";
            this.buttonBrowseModEngineDir.UseVisualStyleBackColor = true;
            this.buttonBrowseModEngineDir.Click += new System.EventHandler(this.buttonBrowseModEngineDir_Click);
            // 
            // textBoxPathModEngineDir
            // 
            this.textBoxPathModEngineDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPathModEngineDir.Location = new System.Drawing.Point(257, 15);
            this.textBoxPathModEngineDir.Name = "textBoxPathModEngineDir";
            this.textBoxPathModEngineDir.Size = new System.Drawing.Size(240, 23);
            this.textBoxPathModEngineDir.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 19);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(220, 15);
            this.label5.TabIndex = 20;
            this.label5.Text = "ModEngine \'/mod/\' directory (optional):";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Location = new System.Drawing.Point(363, 226);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(84, 23);
            this.buttonCancel.TabIndex = 21;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // TaeLoadFromArchivesWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 261);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.progressBarLoading);
            this.Controls.Add(this.labelLoadingText);
            this.Controls.Add(this.buttonCreateProject);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonHelpSaveLoose);
            this.Controls.Add(this.buttonHelpChrbnd);
            this.Controls.Add(this.buttonHelpAnibnd);
            this.Controls.Add(this.buttonHelpGameEXE);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonPathSaveLoose);
            this.Controls.Add(this.textBoxPathSaveLoose);
            this.Controls.Add(this.buttonPathChrbnd);
            this.Controls.Add(this.textBoxPathChrbnd);
            this.Controls.Add(this.buttonPathAnibnd);
            this.Controls.Add(this.textBoxPathAnibnd);
            this.Controls.Add(this.buttonPathInterroot);
            this.Controls.Add(this.textBoxPathInterroot);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "TaeLoadFromArchivesWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Create Project From Packed Game Data";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaeLoadFromArchivesWizard_FormClosing);
            this.Load += new System.EventHandler(this.TaeLoadFromArchivesWizard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPathInterroot;
        private System.Windows.Forms.Button buttonPathInterroot;
        private System.Windows.Forms.Button buttonPathAnibnd;
        private System.Windows.Forms.TextBox textBoxPathAnibnd;
        private System.Windows.Forms.Button buttonPathChrbnd;
        private System.Windows.Forms.TextBox textBoxPathChrbnd;
        private System.Windows.Forms.Button buttonPathSaveLoose;
        private System.Windows.Forms.TextBox textBoxPathSaveLoose;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonHelpSaveLoose;
        private System.Windows.Forms.Button buttonHelpChrbnd;
        private System.Windows.Forms.Button buttonHelpAnibnd;
        private System.Windows.Forms.Button buttonHelpGameEXE;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonCreateProject;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.Label labelLoadingText;
        private System.Windows.Forms.ProgressBar progressBarLoading;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBoxPathModEngineDir;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBoxLoadLooseParams;
        private System.Windows.Forms.Button buttonBrowseModEngineDir;
        private System.Windows.Forms.Button buttonHelpCfgLoadLooseGameParam;
        private System.Windows.Forms.Button buttonHelpCfgModEngineDir;
        private System.Windows.Forms.Button buttonHelpCfgLoadUnpackedGameFiles;
        private System.Windows.Forms.CheckBox checkBoxCfgLoadUnpackedGameFiles;
        private System.Windows.Forms.Button buttonClearModEngineDir;
        private System.Windows.Forms.Button buttonCancel;
    }
}