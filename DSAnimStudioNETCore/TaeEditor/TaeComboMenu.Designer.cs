namespace DSAnimStudio.TaeEditor
{
    partial class TaeComboMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaeComboMenu));
            this.buttonPlayCombo = new System.Windows.Forms.Button();
            this.buttonCancelCombo = new System.Windows.Forms.Button();
            this.checkBoxLoop = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxComboSeq = new System.Windows.Forms.TextBox();
            this.checkBoxRecord = new System.Windows.Forms.CheckBox();
            this.checkBoxRecordHkxOnly = new System.Windows.Forms.CheckBox();
            this.checkBoxRecord60FPS = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonPlayCombo
            // 
            this.buttonPlayCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlayCombo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPlayCombo.ForeColor = System.Drawing.Color.White;
            this.buttonPlayCombo.Location = new System.Drawing.Point(353, 349);
            this.buttonPlayCombo.Name = "buttonPlayCombo";
            this.buttonPlayCombo.Size = new System.Drawing.Size(88, 40);
            this.buttonPlayCombo.TabIndex = 1;
            this.buttonPlayCombo.Text = "Play Combo";
            this.buttonPlayCombo.UseVisualStyleBackColor = true;
            this.buttonPlayCombo.Click += new System.EventHandler(this.buttonPlayCombo_Click);
            // 
            // buttonCancelCombo
            // 
            this.buttonCancelCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelCombo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCancelCombo.ForeColor = System.Drawing.Color.White;
            this.buttonCancelCombo.Location = new System.Drawing.Point(256, 349);
            this.buttonCancelCombo.Name = "buttonCancelCombo";
            this.buttonCancelCombo.Size = new System.Drawing.Size(91, 40);
            this.buttonCancelCombo.TabIndex = 2;
            this.buttonCancelCombo.Text = "Stop Combo";
            this.buttonCancelCombo.UseVisualStyleBackColor = true;
            this.buttonCancelCombo.Click += new System.EventHandler(this.buttonCancelCombo_Click);
            // 
            // checkBoxLoop
            // 
            this.checkBoxLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxLoop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxLoop.ForeColor = System.Drawing.Color.White;
            this.checkBoxLoop.Location = new System.Drawing.Point(447, 372);
            this.checkBoxLoop.Name = "checkBoxLoop";
            this.checkBoxLoop.Size = new System.Drawing.Size(60, 17);
            this.checkBoxLoop.TabIndex = 5;
            this.checkBoxLoop.Text = "Loop";
            this.checkBoxLoop.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(463, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Available Commands:";
            // 
            // textBoxComboSeq
            // 
            this.textBoxComboSeq.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxComboSeq.AutoCompleteCustomSource.AddRange(new string[] {
            "EnemyAtk",
            "EnemyComboAtk",
            "EnemyMove",
            "EnemyDodge",
            "PlayerRH",
            "PlayerMove",
            "PlayerLH",
            "PlayerGuard",
            "PlayerDodge",
            "PlayerEstus",
            "PlayerItem",
            "PlayerWeaponSwitch",
            "ThrowEscape"});
            this.textBoxComboSeq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.textBoxComboSeq.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxComboSeq.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxComboSeq.ForeColor = System.Drawing.Color.White;
            this.textBoxComboSeq.Location = new System.Drawing.Point(12, 12);
            this.textBoxComboSeq.Multiline = true;
            this.textBoxComboSeq.Name = "textBoxComboSeq";
            this.textBoxComboSeq.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxComboSeq.Size = new System.Drawing.Size(429, 331);
            this.textBoxComboSeq.TabIndex = 9;
            this.textBoxComboSeq.Text = "EnemyComboAtk 3000\r\nEnemyComboAtk 3001\r\nEnemyComboAtk 3002\r\n";
            // 
            // checkBoxRecord
            // 
            this.checkBoxRecord.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxRecord.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxRecord.ForeColor = System.Drawing.Color.White;
            this.checkBoxRecord.Location = new System.Drawing.Point(12, 349);
            this.checkBoxRecord.Name = "checkBoxRecord";
            this.checkBoxRecord.Size = new System.Drawing.Size(127, 17);
            this.checkBoxRecord.TabIndex = 10;
            this.checkBoxRecord.Text = "Record (Experimental)";
            this.checkBoxRecord.UseVisualStyleBackColor = true;
            // 
            // checkBoxRecordHkxOnly
            // 
            this.checkBoxRecordHkxOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxRecordHkxOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxRecordHkxOnly.ForeColor = System.Drawing.Color.White;
            this.checkBoxRecordHkxOnly.Location = new System.Drawing.Point(12, 372);
            this.checkBoxRecordHkxOnly.Name = "checkBoxRecordHkxOnly";
            this.checkBoxRecordHkxOnly.Size = new System.Drawing.Size(116, 17);
            this.checkBoxRecordHkxOnly.TabIndex = 11;
            this.checkBoxRecordHkxOnly.Text = "Record HKX Only";
            this.checkBoxRecordHkxOnly.UseVisualStyleBackColor = true;
            // 
            // checkBoxRecord60FPS
            // 
            this.checkBoxRecord60FPS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxRecord60FPS.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxRecord60FPS.ForeColor = System.Drawing.Color.White;
            this.checkBoxRecord60FPS.Location = new System.Drawing.Point(130, 372);
            this.checkBoxRecord60FPS.Name = "checkBoxRecord60FPS";
            this.checkBoxRecord60FPS.Size = new System.Drawing.Size(112, 17);
            this.checkBoxRecord60FPS.TabIndex = 12;
            this.checkBoxRecord60FPS.Text = "Record 60 FPS";
            this.checkBoxRecord60FPS.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(447, 29);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(143, 314);
            this.textBox1.TabIndex = 13;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // TaeComboMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(604, 401);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.checkBoxRecord60FPS);
            this.Controls.Add(this.checkBoxRecordHkxOnly);
            this.Controls.Add(this.checkBoxRecord);
            this.Controls.Add(this.textBoxComboSeq);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.checkBoxLoop);
            this.Controls.Add(this.buttonCancelCombo);
            this.Controls.Add(this.buttonPlayCombo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(620, 200);
            this.Name = "TaeComboMenu";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Combo Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaeComboMenu_FormClosing);
            this.Load += new System.EventHandler(this.TaeComboMenu_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonPlayCombo;
        private System.Windows.Forms.Button buttonCancelCombo;
        private System.Windows.Forms.CheckBox checkBoxLoop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxComboSeq;
        private System.Windows.Forms.CheckBox checkBoxRecord;
        private System.Windows.Forms.CheckBox checkBoxRecordHkxOnly;
        private System.Windows.Forms.CheckBox checkBoxRecord60FPS;
        private System.Windows.Forms.TextBox textBox1;
    }
}