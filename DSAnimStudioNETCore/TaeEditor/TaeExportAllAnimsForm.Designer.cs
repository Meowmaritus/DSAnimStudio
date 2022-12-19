namespace DSAnimStudio.TaeEditor
{
    partial class TaeExportAllAnimsForm
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("a00_0000.hkx");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("a00_0001.hkx");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("a00", new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("a01_0000.hkx");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("a01", new System.Windows.Forms.TreeNode[] {
            treeNode4});
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("Skeleton.hkx");
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxDestinationFolder = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxExportAsFileType = new System.Windows.Forms.ListBox();
            this.buttonStartExport = new System.Windows.Forms.Button();
            this.progressBarExportProgress = new System.Windows.Forms.ProgressBar();
            this.labelExportStatus = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonSelectAll = new System.Windows.Forms.Button();
            this.buttonSelectNone = new System.Windows.Forms.Button();
            this.buttonSelectInvert = new System.Windows.Forms.Button();
            this.treeViewHkxSelect = new System.Windows.Forms.TreeView();
            this.buttonCancelExport = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Destination Folder:";
            // 
            // textBoxDestinationFolder
            // 
            this.textBoxDestinationFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDestinationFolder.BackColor = System.Drawing.Color.DimGray;
            this.textBoxDestinationFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxDestinationFolder.ForeColor = System.Drawing.Color.White;
            this.textBoxDestinationFolder.Location = new System.Drawing.Point(124, 12);
            this.textBoxDestinationFolder.Name = "textBoxDestinationFolder";
            this.textBoxDestinationFolder.Size = new System.Drawing.Size(494, 23);
            this.textBoxDestinationFolder.TabIndex = 1;
            this.textBoxDestinationFolder.TextChanged += new System.EventHandler(this.textBoxDestinationFolder_TextChanged);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonBrowse.ForeColor = System.Drawing.Color.White;
            this.buttonBrowse.Location = new System.Drawing.Point(624, 12);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(133, 23);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseCompatibleTextRendering = true;
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(11, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Export As File Type:";
            // 
            // listBoxExportAsFileType
            // 
            this.listBoxExportAsFileType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxExportAsFileType.BackColor = System.Drawing.Color.DimGray;
            this.listBoxExportAsFileType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxExportAsFileType.ForeColor = System.Drawing.Color.White;
            this.listBoxExportAsFileType.FormattingEnabled = true;
            this.listBoxExportAsFileType.ItemHeight = 15;
            this.listBoxExportAsFileType.Items.AddRange(new object[] {
            "Havok 2010.2 XML [General Purpose] (Faster)",
            "Havok 2010.2 Packfile x32 [For DS1:PTDE] (Fast)",
            "Havok 2016.1 Tagfile x64 [For DS1R/SDT/ER] (Very Slow)"});
            this.listBoxExportAsFileType.Location = new System.Drawing.Point(125, 51);
            this.listBoxExportAsFileType.Name = "listBoxExportAsFileType";
            this.listBoxExportAsFileType.Size = new System.Drawing.Size(493, 62);
            this.listBoxExportAsFileType.TabIndex = 5;
            // 
            // buttonStartExport
            // 
            this.buttonStartExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonStartExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStartExport.ForeColor = System.Drawing.Color.White;
            this.buttonStartExport.Location = new System.Drawing.Point(11, 311);
            this.buttonStartExport.Name = "buttonStartExport";
            this.buttonStartExport.Size = new System.Drawing.Size(107, 24);
            this.buttonStartExport.TabIndex = 6;
            this.buttonStartExport.Text = "Start Export";
            this.buttonStartExport.UseCompatibleTextRendering = true;
            this.buttonStartExport.UseVisualStyleBackColor = true;
            this.buttonStartExport.Click += new System.EventHandler(this.buttonStartExport_Click);
            // 
            // progressBarExportProgress
            // 
            this.progressBarExportProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarExportProgress.Location = new System.Drawing.Point(125, 311);
            this.progressBarExportProgress.Name = "progressBarExportProgress";
            this.progressBarExportProgress.Size = new System.Drawing.Size(493, 24);
            this.progressBarExportProgress.TabIndex = 7;
            // 
            // labelExportStatus
            // 
            this.labelExportStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelExportStatus.ForeColor = System.Drawing.Color.White;
            this.labelExportStatus.Location = new System.Drawing.Point(624, 311);
            this.labelExportStatus.Name = "labelExportStatus";
            this.labelExportStatus.Size = new System.Drawing.Size(133, 24);
            this.labelExportStatus.TabIndex = 8;
            this.labelExportStatus.Text = "XXXX/XXXX";
            this.labelExportStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(35, 119);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Files to Export:";
            // 
            // buttonSelectAll
            // 
            this.buttonSelectAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSelectAll.ForeColor = System.Drawing.Color.White;
            this.buttonSelectAll.Location = new System.Drawing.Point(624, 119);
            this.buttonSelectAll.Name = "buttonSelectAll";
            this.buttonSelectAll.Size = new System.Drawing.Size(133, 23);
            this.buttonSelectAll.TabIndex = 11;
            this.buttonSelectAll.Text = "Select All";
            this.buttonSelectAll.UseCompatibleTextRendering = true;
            this.buttonSelectAll.UseVisualStyleBackColor = true;
            this.buttonSelectAll.Click += new System.EventHandler(this.buttonSelectAll_Click);
            // 
            // buttonSelectNone
            // 
            this.buttonSelectNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectNone.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSelectNone.ForeColor = System.Drawing.Color.White;
            this.buttonSelectNone.Location = new System.Drawing.Point(624, 148);
            this.buttonSelectNone.Name = "buttonSelectNone";
            this.buttonSelectNone.Size = new System.Drawing.Size(133, 23);
            this.buttonSelectNone.TabIndex = 12;
            this.buttonSelectNone.Text = "Select None";
            this.buttonSelectNone.UseCompatibleTextRendering = true;
            this.buttonSelectNone.UseVisualStyleBackColor = true;
            this.buttonSelectNone.Click += new System.EventHandler(this.buttonSelectNone_Click);
            // 
            // buttonSelectInvert
            // 
            this.buttonSelectInvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectInvert.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSelectInvert.ForeColor = System.Drawing.Color.White;
            this.buttonSelectInvert.Location = new System.Drawing.Point(624, 177);
            this.buttonSelectInvert.Name = "buttonSelectInvert";
            this.buttonSelectInvert.Size = new System.Drawing.Size(133, 23);
            this.buttonSelectInvert.TabIndex = 13;
            this.buttonSelectInvert.Text = "Invert Selection";
            this.buttonSelectInvert.UseCompatibleTextRendering = true;
            this.buttonSelectInvert.UseVisualStyleBackColor = true;
            this.buttonSelectInvert.Click += new System.EventHandler(this.buttonSelectInvert_Click);
            // 
            // treeViewHkxSelect
            // 
            this.treeViewHkxSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewHkxSelect.CheckBoxes = true;
            this.treeViewHkxSelect.Location = new System.Drawing.Point(125, 119);
            this.treeViewHkxSelect.Name = "treeViewHkxSelect";
            treeNode1.Checked = true;
            treeNode1.Name = "Node1";
            treeNode1.Text = "a00_0000.hkx";
            treeNode2.Name = "Node2";
            treeNode2.Text = "a00_0001.hkx";
            treeNode3.Name = "Node0";
            treeNode3.Text = "a00";
            treeNode4.Name = "Node4";
            treeNode4.Text = "a01_0000.hkx";
            treeNode5.Name = "Node3";
            treeNode5.Text = "a01";
            treeNode6.Name = "Node5";
            treeNode6.Text = "Skeleton.hkx";
            this.treeViewHkxSelect.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode3,
            treeNode5,
            treeNode6});
            this.treeViewHkxSelect.Size = new System.Drawing.Size(493, 186);
            this.treeViewHkxSelect.TabIndex = 14;
            this.treeViewHkxSelect.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewHkxSelect_AfterCheck);
            // 
            // buttonCancelExport
            // 
            this.buttonCancelExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancelExport.Enabled = false;
            this.buttonCancelExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancelExport.ForeColor = System.Drawing.Color.White;
            this.buttonCancelExport.Location = new System.Drawing.Point(11, 281);
            this.buttonCancelExport.Name = "buttonCancelExport";
            this.buttonCancelExport.Size = new System.Drawing.Size(107, 24);
            this.buttonCancelExport.TabIndex = 15;
            this.buttonCancelExport.Text = "Cancel Export";
            this.buttonCancelExport.UseCompatibleTextRendering = true;
            this.buttonCancelExport.UseVisualStyleBackColor = true;
            this.buttonCancelExport.Click += new System.EventHandler(this.buttonCancelExport_Click);
            // 
            // TaeExportAllAnimsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(769, 347);
            this.Controls.Add(this.buttonCancelExport);
            this.Controls.Add(this.treeViewHkxSelect);
            this.Controls.Add(this.buttonSelectInvert);
            this.Controls.Add(this.buttonSelectNone);
            this.Controls.Add(this.buttonSelectAll);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelExportStatus);
            this.Controls.Add(this.progressBarExportProgress);
            this.Controls.Add(this.buttonStartExport);
            this.Controls.Add(this.listBoxExportAsFileType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxDestinationFolder);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(625, 320);
            this.Name = "TaeExportAllAnimsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export Skeleton & Animations";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaeExportAllAnimsForm_FormClosing);
            this.Load += new System.EventHandler(this.TaeComboMenu_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxDestinationFolder;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxExportAsFileType;
        private System.Windows.Forms.Button buttonStartExport;
        private System.Windows.Forms.ProgressBar progressBarExportProgress;
        private System.Windows.Forms.Label labelExportStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonSelectAll;
        private System.Windows.Forms.Button buttonSelectNone;
        private System.Windows.Forms.Button buttonSelectInvert;
        private System.Windows.Forms.TreeView treeViewHkxSelect;
        private System.Windows.Forms.Button buttonCancelExport;
    }
}