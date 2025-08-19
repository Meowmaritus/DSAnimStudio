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
            components = new System.ComponentModel.Container();
            label1 = new System.Windows.Forms.Label();
            textBoxPathInterroot = new System.Windows.Forms.TextBox();
            buttonPathInterroot = new System.Windows.Forms.Button();
            buttonPathAnibnd = new System.Windows.Forms.Button();
            textBoxPathMainBinder = new System.Windows.Forms.TextBox();
            buttonPathChrbnd = new System.Windows.Forms.Button();
            textBox_Chr_PathChrbnd = new System.Windows.Forms.TextBox();
            buttonPathSaveLoose = new System.Windows.Forms.Button();
            textBoxPathSaveLoose = new System.Windows.Forms.TextBox();
            labelStep3A = new System.Windows.Forms.Label();
            labelStep3B = new System.Windows.Forms.Label();
            buttonHelpSaveLoose = new System.Windows.Forms.Button();
            buttonHelpSecondaryBinder = new System.Windows.Forms.Button();
            buttonHelpPrimaryBinder = new System.Windows.Forms.Button();
            buttonHelpGameEXE = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            buttonCreateProject = new System.Windows.Forms.Button();
            errorProvider1 = new System.Windows.Forms.ErrorProvider(components);
            labelLoadingText = new System.Windows.Forms.Label();
            progressBarLoading = new System.Windows.Forms.ProgressBar();
            checkBoxLoadLooseParams = new System.Windows.Forms.CheckBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            buttonHelpDisableInterrootDCX = new System.Windows.Forms.Button();
            checkBoxDisableInterrootDCX = new System.Windows.Forms.CheckBox();
            buttonClearModEngineDir = new System.Windows.Forms.Button();
            buttonHelpCfgLoadUnpackedGameFiles = new System.Windows.Forms.Button();
            checkBoxCfgLoadUnpackedGameFiles = new System.Windows.Forms.CheckBox();
            buttonHelpCfgLoadLooseGameParam = new System.Windows.Forms.Button();
            buttonHelpCfgModEngineDir = new System.Windows.Forms.Button();
            buttonBrowseModEngineDir = new System.Windows.Forms.Button();
            textBoxPathModEngineDir = new System.Windows.Forms.TextBox();
            label5 = new System.Windows.Forms.Label();
            buttonCancel = new System.Windows.Forms.Button();
            comboBoxFileType = new System.Windows.Forms.ComboBox();
            label6 = new System.Windows.Forms.Label();
            buttonHelpFileType = new System.Windows.Forms.Button();
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(115, 15);
            label1.TabIndex = 0;
            label1.Text = "1. Choose Game EXE";
            // 
            // textBoxPathInterroot
            // 
            textBoxPathInterroot.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxPathInterroot.Location = new System.Drawing.Point(236, 5);
            textBoxPathInterroot.Name = "textBoxPathInterroot";
            textBoxPathInterroot.Size = new System.Drawing.Size(283, 23);
            textBoxPathInterroot.TabIndex = 1;
            textBoxPathInterroot.Validating += textBoxPathInterroot_Validating;
            // 
            // buttonPathInterroot
            // 
            buttonPathInterroot.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonPathInterroot.Location = new System.Drawing.Point(539, 5);
            buttonPathInterroot.Name = "buttonPathInterroot";
            buttonPathInterroot.Size = new System.Drawing.Size(33, 23);
            buttonPathInterroot.TabIndex = 2;
            buttonPathInterroot.Text = "...";
            buttonPathInterroot.UseVisualStyleBackColor = true;
            buttonPathInterroot.Click += buttonPathInterroot_Click;
            // 
            // buttonPathAnibnd
            // 
            buttonPathAnibnd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonPathAnibnd.Enabled = false;
            buttonPathAnibnd.Location = new System.Drawing.Point(539, 63);
            buttonPathAnibnd.Name = "buttonPathAnibnd";
            buttonPathAnibnd.Size = new System.Drawing.Size(33, 23);
            buttonPathAnibnd.TabIndex = 4;
            buttonPathAnibnd.Text = "...";
            buttonPathAnibnd.UseVisualStyleBackColor = true;
            buttonPathAnibnd.Click += buttonPathAnibnd_Click;
            // 
            // textBoxPathMainBinder
            // 
            textBoxPathMainBinder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxPathMainBinder.Location = new System.Drawing.Point(236, 63);
            textBoxPathMainBinder.Name = "textBoxPathMainBinder";
            textBoxPathMainBinder.Size = new System.Drawing.Size(283, 23);
            textBoxPathMainBinder.TabIndex = 3;
            textBoxPathMainBinder.Validating += textBoxPathMainBinder_Validating;
            // 
            // buttonPathChrbnd
            // 
            buttonPathChrbnd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonPathChrbnd.Enabled = false;
            buttonPathChrbnd.Location = new System.Drawing.Point(539, 92);
            buttonPathChrbnd.Name = "buttonPathChrbnd";
            buttonPathChrbnd.Size = new System.Drawing.Size(33, 23);
            buttonPathChrbnd.TabIndex = 6;
            buttonPathChrbnd.Text = "...";
            buttonPathChrbnd.UseVisualStyleBackColor = true;
            buttonPathChrbnd.Click += buttonPathChrbnd_Click;
            // 
            // textBox_Chr_PathChrbnd
            // 
            textBox_Chr_PathChrbnd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBox_Chr_PathChrbnd.Location = new System.Drawing.Point(236, 92);
            textBox_Chr_PathChrbnd.Name = "textBox_Chr_PathChrbnd";
            textBox_Chr_PathChrbnd.Size = new System.Drawing.Size(283, 23);
            textBox_Chr_PathChrbnd.TabIndex = 5;
            textBox_Chr_PathChrbnd.Validating += textBox_Chr_PathChrbnd_Validating;
            // 
            // buttonPathSaveLoose
            // 
            buttonPathSaveLoose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonPathSaveLoose.Enabled = false;
            buttonPathSaveLoose.Location = new System.Drawing.Point(539, 121);
            buttonPathSaveLoose.Name = "buttonPathSaveLoose";
            buttonPathSaveLoose.Size = new System.Drawing.Size(33, 23);
            buttonPathSaveLoose.TabIndex = 8;
            buttonPathSaveLoose.Text = "...";
            buttonPathSaveLoose.UseVisualStyleBackColor = true;
            buttonPathSaveLoose.Click += buttonPathSaveLoose_Click;
            // 
            // textBoxPathSaveLoose
            // 
            textBoxPathSaveLoose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxPathSaveLoose.Location = new System.Drawing.Point(236, 121);
            textBoxPathSaveLoose.Name = "textBoxPathSaveLoose";
            textBoxPathSaveLoose.Size = new System.Drawing.Size(283, 23);
            textBoxPathSaveLoose.TabIndex = 7;
            textBoxPathSaveLoose.Validating += textBoxPathSaveLoose_Validating;
            // 
            // labelStep3A
            // 
            labelStep3A.AutoSize = true;
            labelStep3A.Location = new System.Drawing.Point(11, 68);
            labelStep3A.Name = "labelStep3A";
            labelStep3A.Size = new System.Drawing.Size(112, 15);
            labelStep3A.TabIndex = 9;
            labelStep3A.Text = "3a. Choose ANIBND";
            // 
            // labelStep3B
            // 
            labelStep3B.AutoSize = true;
            labelStep3B.Location = new System.Drawing.Point(11, 97);
            labelStep3B.Name = "labelStep3B";
            labelStep3B.Size = new System.Drawing.Size(117, 15);
            labelStep3B.TabIndex = 10;
            labelStep3B.Text = "3b. Choose CHRBND";
            // 
            // buttonHelpSaveLoose
            // 
            buttonHelpSaveLoose.Location = new System.Drawing.Point(207, 121);
            buttonHelpSaveLoose.Name = "buttonHelpSaveLoose";
            buttonHelpSaveLoose.Size = new System.Drawing.Size(23, 23);
            buttonHelpSaveLoose.TabIndex = 14;
            buttonHelpSaveLoose.Text = "?";
            buttonHelpSaveLoose.UseVisualStyleBackColor = true;
            buttonHelpSaveLoose.Click += buttonHelpSaveLoose_Click;
            // 
            // buttonHelpSecondaryBinder
            // 
            buttonHelpSecondaryBinder.Location = new System.Drawing.Point(207, 92);
            buttonHelpSecondaryBinder.Name = "buttonHelpSecondaryBinder";
            buttonHelpSecondaryBinder.Size = new System.Drawing.Size(23, 23);
            buttonHelpSecondaryBinder.TabIndex = 13;
            buttonHelpSecondaryBinder.Text = "?";
            buttonHelpSecondaryBinder.UseVisualStyleBackColor = true;
            buttonHelpSecondaryBinder.Click += buttonHelpSecondaryBinder_Click;
            // 
            // buttonHelpPrimaryBinder
            // 
            buttonHelpPrimaryBinder.Location = new System.Drawing.Point(207, 63);
            buttonHelpPrimaryBinder.Name = "buttonHelpPrimaryBinder";
            buttonHelpPrimaryBinder.Size = new System.Drawing.Size(23, 23);
            buttonHelpPrimaryBinder.TabIndex = 12;
            buttonHelpPrimaryBinder.Text = "?";
            buttonHelpPrimaryBinder.UseVisualStyleBackColor = true;
            buttonHelpPrimaryBinder.Click += buttonHelpPrimaryBinder_Click;
            // 
            // buttonHelpGameEXE
            // 
            buttonHelpGameEXE.Location = new System.Drawing.Point(207, 5);
            buttonHelpGameEXE.Name = "buttonHelpGameEXE";
            buttonHelpGameEXE.Size = new System.Drawing.Size(23, 23);
            buttonHelpGameEXE.TabIndex = 11;
            buttonHelpGameEXE.Text = "?";
            buttonHelpGameEXE.UseVisualStyleBackColor = true;
            buttonHelpGameEXE.Click += buttonHelpGameEXE_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 125);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(156, 15);
            label4.TabIndex = 15;
            label4.Text = "4. Choose Workspace Folder";
            // 
            // buttonCreateProject
            // 
            buttonCreateProject.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCreateProject.Enabled = false;
            buttonCreateProject.Location = new System.Drawing.Point(453, 300);
            buttonCreateProject.Name = "buttonCreateProject";
            buttonCreateProject.Size = new System.Drawing.Size(119, 23);
            buttonCreateProject.TabIndex = 16;
            buttonCreateProject.Text = "Create Project";
            buttonCreateProject.UseVisualStyleBackColor = true;
            buttonCreateProject.Click += buttonCreateProject_Click;
            // 
            // errorProvider1
            // 
            errorProvider1.ContainerControl = this;
            // 
            // labelLoadingText
            // 
            labelLoadingText.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            labelLoadingText.Location = new System.Drawing.Point(12, 300);
            labelLoadingText.Name = "labelLoadingText";
            labelLoadingText.Size = new System.Drawing.Size(223, 23);
            labelLoadingText.TabIndex = 17;
            labelLoadingText.Text = "Decrypting archive headers, please wait...";
            labelLoadingText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBarLoading
            // 
            progressBarLoading.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            progressBarLoading.Location = new System.Drawing.Point(241, 300);
            progressBarLoading.MarqueeAnimationSpeed = 10;
            progressBarLoading.Maximum = 1;
            progressBarLoading.Name = "progressBarLoading";
            progressBarLoading.Size = new System.Drawing.Size(116, 23);
            progressBarLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            progressBarLoading.TabIndex = 18;
            progressBarLoading.Value = 1;
            // 
            // checkBoxLoadLooseParams
            // 
            checkBoxLoadLooseParams.AutoSize = true;
            checkBoxLoadLooseParams.Location = new System.Drawing.Point(10, 44);
            checkBoxLoadLooseParams.Name = "checkBoxLoadLooseParams";
            checkBoxLoadLooseParams.Size = new System.Drawing.Size(151, 19);
            checkBoxLoadLooseParams.TabIndex = 19;
            checkBoxLoadLooseParams.Text = "Load loose GameParam";
            checkBoxLoadLooseParams.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBox1.Controls.Add(buttonHelpDisableInterrootDCX);
            groupBox1.Controls.Add(checkBoxDisableInterrootDCX);
            groupBox1.Controls.Add(buttonClearModEngineDir);
            groupBox1.Controls.Add(buttonHelpCfgLoadUnpackedGameFiles);
            groupBox1.Controls.Add(checkBoxCfgLoadUnpackedGameFiles);
            groupBox1.Controls.Add(buttonHelpCfgLoadLooseGameParam);
            groupBox1.Controls.Add(buttonHelpCfgModEngineDir);
            groupBox1.Controls.Add(buttonBrowseModEngineDir);
            groupBox1.Controls.Add(textBoxPathModEngineDir);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(checkBoxLoadLooseParams);
            groupBox1.Location = new System.Drawing.Point(7, 150);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(565, 144);
            groupBox1.TabIndex = 20;
            groupBox1.TabStop = false;
            groupBox1.Text = "Options";
            // 
            // buttonHelpDisableInterrootDCX
            // 
            buttonHelpDisableInterrootDCX.Location = new System.Drawing.Point(229, 93);
            buttonHelpDisableInterrootDCX.Name = "buttonHelpDisableInterrootDCX";
            buttonHelpDisableInterrootDCX.Size = new System.Drawing.Size(23, 23);
            buttonHelpDisableInterrootDCX.TabIndex = 29;
            buttonHelpDisableInterrootDCX.Text = "?";
            buttonHelpDisableInterrootDCX.UseVisualStyleBackColor = true;
            buttonHelpDisableInterrootDCX.Click += buttonHelpDisableInterrootDCX_Click;
            // 
            // checkBoxDisableInterrootDCX
            // 
            checkBoxDisableInterrootDCX.AutoSize = true;
            checkBoxDisableInterrootDCX.Location = new System.Drawing.Point(10, 96);
            checkBoxDisableInterrootDCX.Name = "checkBoxDisableInterrootDCX";
            checkBoxDisableInterrootDCX.Size = new System.Drawing.Size(139, 19);
            checkBoxDisableInterrootDCX.TabIndex = 28;
            checkBoxDisableInterrootDCX.Text = "Disable Interroot DCX";
            checkBoxDisableInterrootDCX.UseVisualStyleBackColor = true;
            // 
            // buttonClearModEngineDir
            // 
            buttonClearModEngineDir.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonClearModEngineDir.Location = new System.Drawing.Point(501, 15);
            buttonClearModEngineDir.Name = "buttonClearModEngineDir";
            buttonClearModEngineDir.Size = new System.Drawing.Size(23, 23);
            buttonClearModEngineDir.TabIndex = 27;
            buttonClearModEngineDir.Text = "X";
            buttonClearModEngineDir.UseVisualStyleBackColor = true;
            buttonClearModEngineDir.Click += buttonClearModEngineDir_Click;
            // 
            // buttonHelpCfgLoadUnpackedGameFiles
            // 
            buttonHelpCfgLoadUnpackedGameFiles.Location = new System.Drawing.Point(229, 68);
            buttonHelpCfgLoadUnpackedGameFiles.Name = "buttonHelpCfgLoadUnpackedGameFiles";
            buttonHelpCfgLoadUnpackedGameFiles.Size = new System.Drawing.Size(23, 23);
            buttonHelpCfgLoadUnpackedGameFiles.TabIndex = 26;
            buttonHelpCfgLoadUnpackedGameFiles.Text = "?";
            buttonHelpCfgLoadUnpackedGameFiles.UseVisualStyleBackColor = true;
            buttonHelpCfgLoadUnpackedGameFiles.Click += buttonHelpCfgLoadUnpackedGameFiles_Click;
            // 
            // checkBoxCfgLoadUnpackedGameFiles
            // 
            checkBoxCfgLoadUnpackedGameFiles.AutoSize = true;
            checkBoxCfgLoadUnpackedGameFiles.Location = new System.Drawing.Point(10, 71);
            checkBoxCfgLoadUnpackedGameFiles.Name = "checkBoxCfgLoadUnpackedGameFiles";
            checkBoxCfgLoadUnpackedGameFiles.Size = new System.Drawing.Size(168, 19);
            checkBoxCfgLoadUnpackedGameFiles.TabIndex = 25;
            checkBoxCfgLoadUnpackedGameFiles.Text = "Load Unpacked Game Files";
            checkBoxCfgLoadUnpackedGameFiles.UseVisualStyleBackColor = true;
            // 
            // buttonHelpCfgLoadLooseGameParam
            // 
            buttonHelpCfgLoadLooseGameParam.Location = new System.Drawing.Point(229, 42);
            buttonHelpCfgLoadLooseGameParam.Name = "buttonHelpCfgLoadLooseGameParam";
            buttonHelpCfgLoadLooseGameParam.Size = new System.Drawing.Size(23, 23);
            buttonHelpCfgLoadLooseGameParam.TabIndex = 24;
            buttonHelpCfgLoadLooseGameParam.Text = "?";
            buttonHelpCfgLoadLooseGameParam.UseVisualStyleBackColor = true;
            buttonHelpCfgLoadLooseGameParam.Click += buttonHelpCfgLoadLooseGameParam_Click;
            // 
            // buttonHelpCfgModEngineDir
            // 
            buttonHelpCfgModEngineDir.Location = new System.Drawing.Point(229, 15);
            buttonHelpCfgModEngineDir.Name = "buttonHelpCfgModEngineDir";
            buttonHelpCfgModEngineDir.Size = new System.Drawing.Size(23, 23);
            buttonHelpCfgModEngineDir.TabIndex = 23;
            buttonHelpCfgModEngineDir.Text = "?";
            buttonHelpCfgModEngineDir.UseVisualStyleBackColor = true;
            buttonHelpCfgModEngineDir.Click += buttonHelpCfgModEngineDir_Click;
            // 
            // buttonBrowseModEngineDir
            // 
            buttonBrowseModEngineDir.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonBrowseModEngineDir.Location = new System.Drawing.Point(526, 15);
            buttonBrowseModEngineDir.Name = "buttonBrowseModEngineDir";
            buttonBrowseModEngineDir.Size = new System.Drawing.Size(33, 23);
            buttonBrowseModEngineDir.TabIndex = 22;
            buttonBrowseModEngineDir.Text = "...";
            buttonBrowseModEngineDir.UseVisualStyleBackColor = true;
            buttonBrowseModEngineDir.Click += buttonBrowseModEngineDir_Click;
            // 
            // textBoxPathModEngineDir
            // 
            textBoxPathModEngineDir.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxPathModEngineDir.Location = new System.Drawing.Point(257, 15);
            textBoxPathModEngineDir.Name = "textBoxPathModEngineDir";
            textBoxPathModEngineDir.Size = new System.Drawing.Size(240, 23);
            textBoxPathModEngineDir.TabIndex = 21;
            textBoxPathModEngineDir.Validating += textBoxPathModEngineDir_Validating;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(6, 19);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(220, 15);
            label5.TabIndex = 20;
            label5.Text = "ModEngine '/mod/' directory (optional):";
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.Enabled = false;
            buttonCancel.Location = new System.Drawing.Point(363, 300);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(84, 23);
            buttonCancel.TabIndex = 21;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // comboBoxFileType
            // 
            comboBoxFileType.Enabled = false;
            comboBoxFileType.FormattingEnabled = true;
            comboBoxFileType.Items.AddRange(new object[] { "Character (\"/chr/\")", "Equipment (\"/parts/\")" });
            comboBoxFileType.Location = new System.Drawing.Point(236, 34);
            comboBoxFileType.Name = "comboBoxFileType";
            comboBoxFileType.Size = new System.Drawing.Size(283, 23);
            comboBoxFileType.TabIndex = 22;
            comboBoxFileType.Text = "Character (\"/chr/\")";
            comboBoxFileType.SelectedIndexChanged += comboBoxFileType_SelectedIndexChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new System.Drawing.Point(12, 39);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(107, 15);
            label6.TabIndex = 23;
            label6.Text = "2. Choose File Type";
            // 
            // buttonHelpFileType
            // 
            buttonHelpFileType.Location = new System.Drawing.Point(207, 34);
            buttonHelpFileType.Name = "buttonHelpFileType";
            buttonHelpFileType.Size = new System.Drawing.Size(23, 23);
            buttonHelpFileType.TabIndex = 24;
            buttonHelpFileType.Text = "?";
            buttonHelpFileType.UseVisualStyleBackColor = true;
            buttonHelpFileType.Click += buttonHelpFileType_Click;
            // 
            // comboBox_PartsObjAeg_SelectAnibndInPartsbnd
            // 
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.FormattingEnabled = true;
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Location = new System.Drawing.Point(236, 92);
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Name = "comboBox_PartsObjAeg_SelectAnibndInPartsbnd";
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Size = new System.Drawing.Size(283, 23);
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.TabIndex = 25;
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.Text = "comboBoxSelectAnibndInPartsbnd";
            comboBox_PartsObjAeg_SelectAnibndInPartsbnd.SelectedIndexChanged += comboBox_PartsObjAeg_SelectAnibndInPartsbnd_SelectedIndexChanged;
            // 
            // TaeLoadFromArchivesWizard
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(584, 335);
            Controls.Add(buttonHelpFileType);
            Controls.Add(label6);
            Controls.Add(comboBoxFileType);
            Controls.Add(buttonCancel);
            Controls.Add(groupBox1);
            Controls.Add(progressBarLoading);
            Controls.Add(labelLoadingText);
            Controls.Add(buttonCreateProject);
            Controls.Add(label4);
            Controls.Add(buttonHelpSaveLoose);
            Controls.Add(buttonHelpPrimaryBinder);
            Controls.Add(buttonHelpGameEXE);
            Controls.Add(labelStep3B);
            Controls.Add(labelStep3A);
            Controls.Add(buttonPathSaveLoose);
            Controls.Add(textBoxPathSaveLoose);
            Controls.Add(buttonPathChrbnd);
            Controls.Add(buttonPathAnibnd);
            Controls.Add(textBoxPathMainBinder);
            Controls.Add(buttonPathInterroot);
            Controls.Add(textBoxPathInterroot);
            Controls.Add(label1);
            Controls.Add(textBox_Chr_PathChrbnd);
            Controls.Add(comboBox_PartsObjAeg_SelectAnibndInPartsbnd);
            Controls.Add(buttonHelpSecondaryBinder);
            MinimumSize = new System.Drawing.Size(600, 300);
            Name = "TaeLoadFromArchivesWizard";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "Open From Packed Game Data Archives";
            FormClosing += TaeLoadFromArchivesWizard_FormClosing;
            FormClosed += TaeLoadFromArchivesWizard_FormClosed;
            Load += TaeLoadFromArchivesWizard_Load;
            ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPathInterroot;
        private System.Windows.Forms.Button buttonPathInterroot;
        private System.Windows.Forms.Button buttonPathAnibnd;
        private System.Windows.Forms.TextBox textBoxPathMainBinder;
        private System.Windows.Forms.Button buttonPathChrbnd;
        private System.Windows.Forms.TextBox textBox_Chr_PathChrbnd;
        private System.Windows.Forms.Button buttonPathSaveLoose;
        private System.Windows.Forms.TextBox textBoxPathSaveLoose;
        private System.Windows.Forms.Label labelStep3A;
        private System.Windows.Forms.Label labelStep3B;
        private System.Windows.Forms.Button buttonHelpSaveLoose;
        private System.Windows.Forms.Button buttonHelpSecondaryBinder;
        private System.Windows.Forms.Button buttonHelpPrimaryBinder;
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
        private System.Windows.Forms.ComboBox comboBoxFileType;
        private System.Windows.Forms.Button buttonHelpFileType;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_PartsObjAeg_SelectAnibndInPartsbnd;
        private System.Windows.Forms.Button buttonHelpDisableInterrootDCX;
        private System.Windows.Forms.CheckBox checkBoxDisableInterrootDCX;
    }
}