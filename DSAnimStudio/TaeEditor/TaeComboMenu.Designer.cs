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
            this.buttonPlayCombo = new System.Windows.Forms.Button();
            this.buttonCancelCombo = new System.Windows.Forms.Button();
            this.checkBoxLoop = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxComboSeq = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonPlayCombo
            // 
            this.buttonPlayCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlayCombo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPlayCombo.ForeColor = System.Drawing.Color.White;
            this.buttonPlayCombo.Location = new System.Drawing.Point(349, 326);
            this.buttonPlayCombo.Name = "buttonPlayCombo";
            this.buttonPlayCombo.Size = new System.Drawing.Size(88, 23);
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
            this.buttonCancelCombo.Location = new System.Drawing.Point(252, 326);
            this.buttonCancelCombo.Name = "buttonCancelCombo";
            this.buttonCancelCombo.Size = new System.Drawing.Size(91, 23);
            this.buttonCancelCombo.TabIndex = 2;
            this.buttonCancelCombo.Text = "Stop Combo";
            this.buttonCancelCombo.UseVisualStyleBackColor = true;
            this.buttonCancelCombo.Click += new System.EventHandler(this.buttonCancelCombo_Click);
            // 
            // checkBoxLoop
            // 
            this.checkBoxLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxLoop.AutoSize = true;
            this.checkBoxLoop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxLoop.ForeColor = System.Drawing.Color.White;
            this.checkBoxLoop.Location = new System.Drawing.Point(16, 331);
            this.checkBoxLoop.Name = "checkBoxLoop";
            this.checkBoxLoop.Size = new System.Drawing.Size(47, 17);
            this.checkBoxLoop.TabIndex = 5;
            this.checkBoxLoop.Text = "Loop";
            this.checkBoxLoop.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(452, 34);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 190);
            this.label1.TabIndex = 7;
            this.label1.Text = "EnemyAtk\r\nEnemyComboAtk\r\nEnemyMove\r\nEnemyDodge\r\nPlayerRH\r\nPlayerMove\r\nPlayerLH\r\nP" +
    "layerGuard\r\nPlayerDodge\r\nPlayerEstus\r\nPlayerItem\r\nPlayerWeaponSwitch\r\nThrowEscap" +
    "e";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(443, 12);
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
            this.textBoxComboSeq.Size = new System.Drawing.Size(425, 304);
            this.textBoxComboSeq.TabIndex = 9;
            this.textBoxComboSeq.Text = "EnemyComboAtk 3000\r\nEnemyComboAtk 3001\r\nEnemyComboAtk 3002\r\n";
            // 
            // TaeComboMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.textBoxComboSeq);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxLoop);
            this.Controls.Add(this.buttonCancelCombo);
            this.Controls.Add(this.buttonPlayCombo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 200);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxComboSeq;
    }
}