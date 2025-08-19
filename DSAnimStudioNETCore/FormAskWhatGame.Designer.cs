namespace DSAnimStudio
{
    partial class FormAskWhatGame
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
            label1 = new System.Windows.Forms.Label();
            radioButtonDES = new System.Windows.Forms.RadioButton();
            radioButtonDS1 = new System.Windows.Forms.RadioButton();
            radioButtonDS1R = new System.Windows.Forms.RadioButton();
            radioButtonDS3 = new System.Windows.Forms.RadioButton();
            radioButtonBB = new System.Windows.Forms.RadioButton();
            radioButtonSDT = new System.Windows.Forms.RadioButton();
            radioButtonER = new System.Windows.Forms.RadioButton();
            radioButtonAC6 = new System.Windows.Forms.RadioButton();
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            radioButtonERNR = new System.Windows.Forms.RadioButton();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(143, 15);
            label1.TabIndex = 0;
            label1.Text = "What game is this file for?";
            // 
            // radioButtonDES
            // 
            radioButtonDES.AutoSize = true;
            radioButtonDES.Location = new System.Drawing.Point(12, 41);
            radioButtonDES.Name = "radioButtonDES";
            radioButtonDES.Size = new System.Drawing.Size(103, 19);
            radioButtonDES.TabIndex = 1;
            radioButtonDES.Text = "Demon's Souls";
            radioButtonDES.UseVisualStyleBackColor = true;
            radioButtonDES.CheckedChanged += radioButtonDES_CheckedChanged;
            // 
            // radioButtonDS1
            // 
            radioButtonDS1.AutoSize = true;
            radioButtonDS1.Location = new System.Drawing.Point(12, 66);
            radioButtonDS1.Name = "radioButtonDS1";
            radioButtonDS1.Size = new System.Drawing.Size(114, 19);
            radioButtonDS1.TabIndex = 2;
            radioButtonDS1.Text = "Dark Souls: PTDE";
            radioButtonDS1.UseVisualStyleBackColor = true;
            radioButtonDS1.CheckedChanged += radioButtonDS1_CheckedChanged;
            // 
            // radioButtonDS1R
            // 
            radioButtonDS1R.AutoSize = true;
            radioButtonDS1R.Location = new System.Drawing.Point(12, 166);
            radioButtonDS1R.Name = "radioButtonDS1R";
            radioButtonDS1R.Size = new System.Drawing.Size(145, 19);
            radioButtonDS1R.TabIndex = 3;
            radioButtonDS1R.Text = "Dark Souls Remastered";
            radioButtonDS1R.UseVisualStyleBackColor = true;
            radioButtonDS1R.CheckedChanged += radioButtonDS1R_CheckedChanged;
            // 
            // radioButtonDS3
            // 
            radioButtonDS3.AutoSize = true;
            radioButtonDS3.Location = new System.Drawing.Point(12, 116);
            radioButtonDS3.Name = "radioButtonDS3";
            radioButtonDS3.Size = new System.Drawing.Size(92, 19);
            radioButtonDS3.TabIndex = 4;
            radioButtonDS3.Text = "Dark Souls III";
            radioButtonDS3.UseVisualStyleBackColor = true;
            radioButtonDS3.CheckedChanged += radioButtonDS3_CheckedChanged;
            // 
            // radioButtonBB
            // 
            radioButtonBB.AutoSize = true;
            radioButtonBB.Location = new System.Drawing.Point(12, 91);
            radioButtonBB.Name = "radioButtonBB";
            radioButtonBB.Size = new System.Drawing.Size(87, 19);
            radioButtonBB.TabIndex = 5;
            radioButtonBB.Text = "Bloodborne";
            radioButtonBB.UseVisualStyleBackColor = true;
            radioButtonBB.CheckedChanged += radioButtonBB_CheckedChanged;
            // 
            // radioButtonSDT
            // 
            radioButtonSDT.AutoSize = true;
            radioButtonSDT.Location = new System.Drawing.Point(12, 141);
            radioButtonSDT.Name = "radioButtonSDT";
            radioButtonSDT.Size = new System.Drawing.Size(57, 19);
            radioButtonSDT.TabIndex = 6;
            radioButtonSDT.Text = "Sekiro";
            radioButtonSDT.UseVisualStyleBackColor = true;
            radioButtonSDT.CheckedChanged += radioButtonSDT_CheckedChanged;
            // 
            // radioButtonER
            // 
            radioButtonER.AutoSize = true;
            radioButtonER.Checked = true;
            radioButtonER.Location = new System.Drawing.Point(12, 191);
            radioButtonER.Name = "radioButtonER";
            radioButtonER.Size = new System.Drawing.Size(81, 19);
            radioButtonER.TabIndex = 7;
            radioButtonER.TabStop = true;
            radioButtonER.Text = "Elden Ring";
            radioButtonER.UseVisualStyleBackColor = true;
            radioButtonER.CheckedChanged += radioButtonER_CheckedChanged;
            // 
            // radioButtonAC6
            // 
            radioButtonAC6.AutoSize = true;
            radioButtonAC6.Location = new System.Drawing.Point(12, 216);
            radioButtonAC6.Name = "radioButtonAC6";
            radioButtonAC6.Size = new System.Drawing.Size(204, 19);
            radioButtonAC6.TabIndex = 8;
            radioButtonAC6.Text = "Armored Core VI: Fires of Rubicon";
            radioButtonAC6.UseVisualStyleBackColor = true;
            radioButtonAC6.CheckedChanged += radioButtonAC6_CheckedChanged;
            // 
            // buttonOK
            // 
            buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOK.Location = new System.Drawing.Point(12, 266);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(214, 41);
            buttonOK.TabIndex = 9;
            buttonOK.Text = "LOAD";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(12, 313);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(214, 23);
            buttonCancel.TabIndex = 10;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // radioButtonERNR
            // 
            radioButtonERNR.AutoSize = true;
            radioButtonERNR.Location = new System.Drawing.Point(12, 241);
            radioButtonERNR.Name = "radioButtonERNR";
            radioButtonERNR.Size = new System.Drawing.Size(141, 19);
            radioButtonERNR.TabIndex = 11;
            radioButtonERNR.Text = "Elden Ring Nightreign";
            radioButtonERNR.UseVisualStyleBackColor = true;
            radioButtonERNR.CheckedChanged += radioButtonERNR_CheckedChanged;
            // 
            // FormAskWhatGame
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(238, 350);
            Controls.Add(radioButtonERNR);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Controls.Add(radioButtonAC6);
            Controls.Add(radioButtonER);
            Controls.Add(radioButtonSDT);
            Controls.Add(radioButtonBB);
            Controls.Add(radioButtonDS3);
            Controls.Add(radioButtonDS1R);
            Controls.Add(radioButtonDS1);
            Controls.Add(radioButtonDES);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            MaximumSize = new System.Drawing.Size(254, 389);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(254, 389);
            Name = "FormAskWhatGame";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Choose Game";
            Load += FormAskWhatGame_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButtonDES;
        private System.Windows.Forms.RadioButton radioButtonDS1;
        private System.Windows.Forms.RadioButton radioButtonDS1R;
        private System.Windows.Forms.RadioButton radioButtonDS3;
        private System.Windows.Forms.RadioButton radioButtonBB;
        private System.Windows.Forms.RadioButton radioButtonSDT;
        private System.Windows.Forms.RadioButton radioButtonER;
        private System.Windows.Forms.RadioButton radioButtonAC6;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.RadioButton radioButtonERNR;
    }
}