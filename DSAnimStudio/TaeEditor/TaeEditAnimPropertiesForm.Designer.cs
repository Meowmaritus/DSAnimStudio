namespace DSAnimStudio.TaeEditor
{
    partial class TaeEditAnimPropertiesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaeEditAnimPropertiesForm));
            this.buttonDiscardChanges = new System.Windows.Forms.Button();
            this.buttonSaveChanges = new System.Windows.Forms.Button();
            this.buttonDeleteAnim = new System.Windows.Forms.Button();
            this.labelAnimSubIDPrefix = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxMHStandard = new System.Windows.Forms.GroupBox();
            this.textBoxMHStandardImportHKXFrom = new System.Windows.Forms.TextBox();
            this.checkBoxMHStandardImportHKX = new System.Windows.Forms.CheckBox();
            this.checkBoxMHStandardImportEvents = new System.Windows.Forms.CheckBox();
            this.checkBoxMHStandardLoopByDefault = new System.Windows.Forms.CheckBox();
            this.textBoxAnimSubID = new System.Windows.Forms.TextBox();
            this.textBoxDisplayName = new System.Windows.Forms.TextBox();
            this.radioButtonMHStandard = new System.Windows.Forms.RadioButton();
            this.radioButtonMHImportOtherAnimation = new System.Windows.Forms.RadioButton();
            this.groupBoxMHDuplicate = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxMHDuplicateUnk = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxMHDuplicateSourceAnimID = new System.Windows.Forms.TextBox();
            this.groupBoxMHStandard.SuspendLayout();
            this.groupBoxMHDuplicate.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonDiscardChanges
            // 
            this.buttonDiscardChanges.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonDiscardChanges.Location = new System.Drawing.Point(396, 384);
            this.buttonDiscardChanges.Name = "buttonDiscardChanges";
            this.buttonDiscardChanges.Size = new System.Drawing.Size(114, 23);
            this.buttonDiscardChanges.TabIndex = 1;
            this.buttonDiscardChanges.Text = "Discard Changes";
            this.buttonDiscardChanges.UseVisualStyleBackColor = true;
            this.buttonDiscardChanges.Click += new System.EventHandler(this.buttonDiscardChanges_Click);
            // 
            // buttonSaveChanges
            // 
            this.buttonSaveChanges.Location = new System.Drawing.Point(285, 384);
            this.buttonSaveChanges.Name = "buttonSaveChanges";
            this.buttonSaveChanges.Size = new System.Drawing.Size(105, 23);
            this.buttonSaveChanges.TabIndex = 2;
            this.buttonSaveChanges.Text = "Keep Changes";
            this.buttonSaveChanges.UseVisualStyleBackColor = true;
            this.buttonSaveChanges.Click += new System.EventHandler(this.buttonSaveChanges_Click);
            // 
            // buttonDeleteAnim
            // 
            this.buttonDeleteAnim.Location = new System.Drawing.Point(15, 384);
            this.buttonDeleteAnim.Name = "buttonDeleteAnim";
            this.buttonDeleteAnim.Size = new System.Drawing.Size(136, 23);
            this.buttonDeleteAnim.TabIndex = 3;
            this.buttonDeleteAnim.Text = "Delete This Animation...";
            this.buttonDeleteAnim.UseVisualStyleBackColor = true;
            this.buttonDeleteAnim.Click += new System.EventHandler(this.buttonDeleteAnim_Click);
            // 
            // labelAnimSubIDPrefix
            // 
            this.labelAnimSubIDPrefix.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnimSubIDPrefix.Location = new System.Drawing.Point(10, 9);
            this.labelAnimSubIDPrefix.Name = "labelAnimSubIDPrefix";
            this.labelAnimSubIDPrefix.Size = new System.Drawing.Size(138, 21);
            this.labelAnimSubIDPrefix.TabIndex = 4;
            this.labelAnimSubIDPrefix.Text = "Animation Sub-ID: aXXX_";
            this.labelAnimSubIDPrefix.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 20);
            this.label2.TabIndex = 5;
            this.label2.Text = "Display Name:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBoxMHStandard
            // 
            this.groupBoxMHStandard.Controls.Add(this.textBoxMHStandardImportHKXFrom);
            this.groupBoxMHStandard.Controls.Add(this.checkBoxMHStandardImportHKX);
            this.groupBoxMHStandard.Controls.Add(this.checkBoxMHStandardImportEvents);
            this.groupBoxMHStandard.Controls.Add(this.checkBoxMHStandardLoopByDefault);
            this.groupBoxMHStandard.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxMHStandard.Location = new System.Drawing.Point(12, 103);
            this.groupBoxMHStandard.Name = "groupBoxMHStandard";
            this.groupBoxMHStandard.Size = new System.Drawing.Size(498, 101);
            this.groupBoxMHStandard.TabIndex = 6;
            this.groupBoxMHStandard.TabStop = false;
            this.groupBoxMHStandard.Text = "Mini-Header Data: Standard";
            // 
            // textBoxMHStandardImportHKXFrom
            // 
            this.textBoxMHStandardImportHKXFrom.Location = new System.Drawing.Point(347, 20);
            this.textBoxMHStandardImportHKXFrom.Name = "textBoxMHStandardImportHKXFrom";
            this.textBoxMHStandardImportHKXFrom.Size = new System.Drawing.Size(100, 20);
            this.textBoxMHStandardImportHKXFrom.TabIndex = 4;
            this.textBoxMHStandardImportHKXFrom.Text = "aXXX_YYYYYY";
            // 
            // checkBoxMHStandardImportHKX
            // 
            this.checkBoxMHStandardImportHKX.Location = new System.Drawing.Point(13, 20);
            this.checkBoxMHStandardImportHKX.Name = "checkBoxMHStandardImportHKX";
            this.checkBoxMHStandardImportHKX.Size = new System.Drawing.Size(327, 21);
            this.checkBoxMHStandardImportHKX.TabIndex = 2;
            this.checkBoxMHStandardImportHKX.Text = "Import the model animation data (.HKX) from this animation ID:";
            this.checkBoxMHStandardImportHKX.UseVisualStyleBackColor = true;
            this.checkBoxMHStandardImportHKX.CheckedChanged += new System.EventHandler(this.checkBoxMHStandardImportHKX_CheckedChanged);
            // 
            // checkBoxMHStandardImportEvents
            // 
            this.checkBoxMHStandardImportEvents.Location = new System.Drawing.Point(13, 43);
            this.checkBoxMHStandardImportEvents.Name = "checkBoxMHStandardImportEvents";
            this.checkBoxMHStandardImportEvents.Size = new System.Drawing.Size(302, 21);
            this.checkBoxMHStandardImportEvents.TabIndex = 1;
            this.checkBoxMHStandardImportEvents.Text = "Allow this animation to be pulled from DelayLoad ANIBNDs";
            this.checkBoxMHStandardImportEvents.UseVisualStyleBackColor = true;
            // 
            // checkBoxMHStandardLoopByDefault
            // 
            this.checkBoxMHStandardLoopByDefault.Location = new System.Drawing.Point(13, 66);
            this.checkBoxMHStandardLoopByDefault.Name = "checkBoxMHStandardLoopByDefault";
            this.checkBoxMHStandardLoopByDefault.Size = new System.Drawing.Size(192, 21);
            this.checkBoxMHStandardLoopByDefault.TabIndex = 0;
            this.checkBoxMHStandardLoopByDefault.Text = "Make this animation loop by default";
            this.checkBoxMHStandardLoopByDefault.UseVisualStyleBackColor = true;
            // 
            // textBoxAnimSubID
            // 
            this.textBoxAnimSubID.Location = new System.Drawing.Point(153, 10);
            this.textBoxAnimSubID.Name = "textBoxAnimSubID";
            this.textBoxAnimSubID.Size = new System.Drawing.Size(89, 20);
            this.textBoxAnimSubID.TabIndex = 7;
            this.textBoxAnimSubID.Text = "YYYYYY";
            // 
            // textBoxDisplayName
            // 
            this.textBoxDisplayName.Location = new System.Drawing.Point(97, 36);
            this.textBoxDisplayName.Name = "textBoxDisplayName";
            this.textBoxDisplayName.Size = new System.Drawing.Size(318, 20);
            this.textBoxDisplayName.TabIndex = 8;
            this.textBoxDisplayName.Text = "aXXX_YYYYYY.hkt";
            // 
            // radioButtonMHStandard
            // 
            this.radioButtonMHStandard.AutoSize = true;
            this.radioButtonMHStandard.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonMHStandard.Location = new System.Drawing.Point(12, 80);
            this.radioButtonMHStandard.Name = "radioButtonMHStandard";
            this.radioButtonMHStandard.Size = new System.Drawing.Size(167, 17);
            this.radioButtonMHStandard.TabIndex = 10;
            this.radioButtonMHStandard.Text = "Standard Animation Type";
            this.radioButtonMHStandard.UseVisualStyleBackColor = true;
            this.radioButtonMHStandard.CheckedChanged += new System.EventHandler(this.radioButtonMHStandard_CheckedChanged);
            // 
            // radioButtonMHImportOtherAnimation
            // 
            this.radioButtonMHImportOtherAnimation.AutoSize = true;
            this.radioButtonMHImportOtherAnimation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonMHImportOtherAnimation.Location = new System.Drawing.Point(11, 228);
            this.radioButtonMHImportOtherAnimation.Name = "radioButtonMHImportOtherAnimation";
            this.radioButtonMHImportOtherAnimation.Size = new System.Drawing.Size(196, 17);
            this.radioButtonMHImportOtherAnimation.TabIndex = 11;
            this.radioButtonMHImportOtherAnimation.Text = "Exact Duplicate of Other Anim";
            this.radioButtonMHImportOtherAnimation.UseVisualStyleBackColor = true;
            this.radioButtonMHImportOtherAnimation.CheckedChanged += new System.EventHandler(this.radioButtonMHImportOtherAnimation_CheckedChanged);
            // 
            // groupBoxMHDuplicate
            // 
            this.groupBoxMHDuplicate.Controls.Add(this.label4);
            this.groupBoxMHDuplicate.Controls.Add(this.textBoxMHDuplicateUnk);
            this.groupBoxMHDuplicate.Controls.Add(this.label5);
            this.groupBoxMHDuplicate.Controls.Add(this.textBoxMHDuplicateSourceAnimID);
            this.groupBoxMHDuplicate.Location = new System.Drawing.Point(11, 251);
            this.groupBoxMHDuplicate.Name = "groupBoxMHDuplicate";
            this.groupBoxMHDuplicate.Size = new System.Drawing.Size(499, 87);
            this.groupBoxMHDuplicate.TabIndex = 12;
            this.groupBoxMHDuplicate.TabStop = false;
            this.groupBoxMHDuplicate.Text = "Mini-Header Data: Duplicate";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(53, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(286, 21);
            this.label4.TabIndex = 7;
            this.label4.Text = "This animation is a duplicate of the animation with this ID:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxMHDuplicateUnk
            // 
            this.textBoxMHDuplicateUnk.Location = new System.Drawing.Point(348, 45);
            this.textBoxMHDuplicateUnk.Name = "textBoxMHDuplicateUnk";
            this.textBoxMHDuplicateUnk.Size = new System.Drawing.Size(100, 20);
            this.textBoxMHDuplicateUnk.TabIndex = 6;
            this.textBoxMHDuplicateUnk.Text = "0";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(245, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(96, 21);
            this.label5.TabIndex = 5;
            this.label5.Text = "Unknown Value:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxMHDuplicateSourceAnimID
            // 
            this.textBoxMHDuplicateSourceAnimID.Location = new System.Drawing.Point(348, 19);
            this.textBoxMHDuplicateSourceAnimID.Name = "textBoxMHDuplicateSourceAnimID";
            this.textBoxMHDuplicateSourceAnimID.Size = new System.Drawing.Size(100, 20);
            this.textBoxMHDuplicateSourceAnimID.TabIndex = 4;
            this.textBoxMHDuplicateSourceAnimID.Text = "aXXX_YYYYYY";
            // 
            // TaeEditAnimPropertiesForm
            // 
            this.AcceptButton = this.buttonSaveChanges;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonDiscardChanges;
            this.ClientSize = new System.Drawing.Size(523, 423);
            this.Controls.Add(this.groupBoxMHDuplicate);
            this.Controls.Add(this.radioButtonMHImportOtherAnimation);
            this.Controls.Add(this.radioButtonMHStandard);
            this.Controls.Add(this.textBoxDisplayName);
            this.Controls.Add(this.textBoxAnimSubID);
            this.Controls.Add(this.groupBoxMHStandard);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelAnimSubIDPrefix);
            this.Controls.Add(this.buttonDeleteAnim);
            this.Controls.Add(this.buttonSaveChanges);
            this.Controls.Add(this.buttonDiscardChanges);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(380, 198);
            this.Name = "TaeEditAnimPropertiesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Anim Info";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaeEditAnimPropertiesForm_FormClosing);
            this.Load += new System.EventHandler(this.TaeEditAnimPropertiesForm_Load);
            this.Shown += new System.EventHandler(this.TaeEditAnimPropertiesForm_Shown);
            this.groupBoxMHStandard.ResumeLayout(false);
            this.groupBoxMHStandard.PerformLayout();
            this.groupBoxMHDuplicate.ResumeLayout(false);
            this.groupBoxMHDuplicate.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonDiscardChanges;
        private System.Windows.Forms.Button buttonSaveChanges;
        private System.Windows.Forms.Button buttonDeleteAnim;
        private System.Windows.Forms.Label labelAnimSubIDPrefix;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxMHStandard;
        private System.Windows.Forms.TextBox textBoxAnimSubID;
        private System.Windows.Forms.TextBox textBoxDisplayName;
        private System.Windows.Forms.CheckBox checkBoxMHStandardImportEvents;
        private System.Windows.Forms.CheckBox checkBoxMHStandardLoopByDefault;
        private System.Windows.Forms.CheckBox checkBoxMHStandardImportHKX;
        private System.Windows.Forms.TextBox textBoxMHStandardImportHKXFrom;
        private System.Windows.Forms.RadioButton radioButtonMHStandard;
        private System.Windows.Forms.RadioButton radioButtonMHImportOtherAnimation;
        private System.Windows.Forms.GroupBox groupBoxMHDuplicate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxMHDuplicateUnk;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxMHDuplicateSourceAnimID;
    }
}