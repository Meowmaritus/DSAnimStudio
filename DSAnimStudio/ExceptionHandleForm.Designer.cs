namespace DSAnimStudio
{
    partial class ExceptionHandleForm
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
            this.labelHeader = new System.Windows.Forms.Label();
            this.textBoxError = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonInfiniteLoop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxBackup = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // labelHeader
            // 
            this.labelHeader.AutoSize = true;
            this.labelHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelHeader.Location = new System.Drawing.Point(13, 13);
            this.labelHeader.Name = "labelHeader";
            this.labelHeader.Size = new System.Drawing.Size(229, 26);
            this.labelHeader.TabIndex = 0;
            this.labelHeader.Text = "[Logged to DSAnimStudio_ErrorLog.txt]\r\nFatal error encountered:";
            // 
            // textBoxError
            // 
            this.textBoxError.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxError.Location = new System.Drawing.Point(13, 42);
            this.textBoxError.Multiline = true;
            this.textBoxError.Name = "textBoxError";
            this.textBoxError.ReadOnly = true;
            this.textBoxError.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxError.Size = new System.Drawing.Size(674, 178);
            this.textBoxError.TabIndex = 1;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(537, 322);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(150, 26);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "IGNORE";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonInfiniteLoop
            // 
            this.buttonInfiniteLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInfiniteLoop.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.buttonInfiniteLoop.Location = new System.Drawing.Point(370, 322);
            this.buttonInfiniteLoop.Name = "buttonInfiniteLoop";
            this.buttonInfiniteLoop.Size = new System.Drawing.Size(161, 26);
            this.buttonInfiniteLoop.TabIndex = 2;
            this.buttonInfiniteLoop.Text = "ABORT";
            this.buttonInfiniteLoop.UseVisualStyleBackColor = true;
            this.buttonInfiniteLoop.Click += new System.EventHandler(this.buttonInfiniteLoop_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(10, 223);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(677, 33);
            this.label1.TabIndex = 3;
            this.label1.Text = "Click ABORT to close the application (useful for infinite error message loops).\r\n" +
    "Click IGNORE to ignore the error and continue running the application";
            // 
            // checkBoxBackup
            // 
            this.checkBoxBackup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxBackup.Checked = true;
            this.checkBoxBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBackup.Location = new System.Drawing.Point(12, 259);
            this.checkBoxBackup.Name = "checkBoxBackup";
            this.checkBoxBackup.Size = new System.Drawing.Size(675, 57);
            this.checkBoxBackup.TabIndex = 4;
            this.checkBoxBackup.Text = "Create emergency backup file of last saved version before continuing: \r\nC:\\Progra" +
    "m Files (x86)\\Steam\\steamapps\\common\\DARK SOULS REMASTERED\\chr\\x4100.anibnd.dcx." +
    "20201227_052000.errbak";
            this.checkBoxBackup.UseVisualStyleBackColor = true;
            // 
            // ExceptionHandleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(699, 355);
            this.Controls.Add(this.checkBoxBackup);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonInfiniteLoop);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBoxError);
            this.Controls.Add(this.labelHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 320);
            this.Name = "ExceptionHandleForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Fatal Error Encountered";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExceptionHandleForm_FormClosing);
            this.Load += new System.EventHandler(this.ExceptionHandleForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelHeader;
        private System.Windows.Forms.TextBox textBoxError;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonInfiniteLoop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxBackup;
    }
}