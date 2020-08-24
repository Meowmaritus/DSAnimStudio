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
            this.groupBoxMiniHeader = new System.Windows.Forms.GroupBox();
            this.textBoxMHImportAnimUnknown = new System.Windows.Forms.TextBox();
            this.labelMHImportAnimUnknown = new System.Windows.Forms.Label();
            this.textBoxMHBothImportFrom = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxMHStandardImportHKX = new System.Windows.Forms.CheckBox();
            this.checkBoxMHStandardImportEvents = new System.Windows.Forms.CheckBox();
            this.checkBoxMHStandardLoopByDefault = new System.Windows.Forms.CheckBox();
            this.textBoxAnimSubID = new System.Windows.Forms.TextBox();
            this.textBoxDisplayName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.radioButtonMHStandard = new System.Windows.Forms.RadioButton();
            this.radioButtonMHImportOtherAnimation = new System.Windows.Forms.RadioButton();
            this.groupBoxMiniHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonDiscardChanges
            // 
            this.buttonDiscardChanges.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonDiscardChanges.Location = new System.Drawing.Point(279, 194);
            this.buttonDiscardChanges.Name = "buttonDiscardChanges";
            this.buttonDiscardChanges.Size = new System.Drawing.Size(114, 23);
            this.buttonDiscardChanges.TabIndex = 1;
            this.buttonDiscardChanges.Text = "Discard Changes";
            this.buttonDiscardChanges.UseVisualStyleBackColor = true;
            this.buttonDiscardChanges.Click += new System.EventHandler(this.buttonDiscardChanges_Click);
            // 
            // buttonSaveChanges
            // 
            this.buttonSaveChanges.Location = new System.Drawing.Point(168, 194);
            this.buttonSaveChanges.Name = "buttonSaveChanges";
            this.buttonSaveChanges.Size = new System.Drawing.Size(105, 23);
            this.buttonSaveChanges.TabIndex = 2;
            this.buttonSaveChanges.Text = "Keep Changes";
            this.buttonSaveChanges.UseVisualStyleBackColor = true;
            this.buttonSaveChanges.Click += new System.EventHandler(this.buttonSaveChanges_Click);
            // 
            // buttonDeleteAnim
            // 
            this.buttonDeleteAnim.Location = new System.Drawing.Point(10, 194);
            this.buttonDeleteAnim.Name = "buttonDeleteAnim";
            this.buttonDeleteAnim.Size = new System.Drawing.Size(108, 23);
            this.buttonDeleteAnim.TabIndex = 3;
            this.buttonDeleteAnim.Text = "Delete Anim...";
            this.buttonDeleteAnim.UseVisualStyleBackColor = true;
            this.buttonDeleteAnim.Click += new System.EventHandler(this.buttonDeleteAnim_Click);
            // 
            // labelAnimSubIDPrefix
            // 
            this.labelAnimSubIDPrefix.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelAnimSubIDPrefix.Location = new System.Drawing.Point(10, 13);
            this.labelAnimSubIDPrefix.Name = "labelAnimSubIDPrefix";
            this.labelAnimSubIDPrefix.Size = new System.Drawing.Size(138, 13);
            this.labelAnimSubIDPrefix.TabIndex = 4;
            this.labelAnimSubIDPrefix.Text = "Animation Sub-ID: aXXX_";
            this.labelAnimSubIDPrefix.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Display Name:";
            // 
            // groupBoxMiniHeader
            // 
            this.groupBoxMiniHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMiniHeader.Controls.Add(this.textBoxMHImportAnimUnknown);
            this.groupBoxMiniHeader.Controls.Add(this.labelMHImportAnimUnknown);
            this.groupBoxMiniHeader.Controls.Add(this.textBoxMHBothImportFrom);
            this.groupBoxMiniHeader.Controls.Add(this.label1);
            this.groupBoxMiniHeader.Controls.Add(this.checkBoxMHStandardImportHKX);
            this.groupBoxMiniHeader.Controls.Add(this.checkBoxMHStandardImportEvents);
            this.groupBoxMiniHeader.Controls.Add(this.checkBoxMHStandardLoopByDefault);
            this.groupBoxMiniHeader.Location = new System.Drawing.Point(12, 97);
            this.groupBoxMiniHeader.Name = "groupBoxMiniHeader";
            this.groupBoxMiniHeader.Size = new System.Drawing.Size(404, 91);
            this.groupBoxMiniHeader.TabIndex = 6;
            this.groupBoxMiniHeader.TabStop = false;
            this.groupBoxMiniHeader.Text = "Mini-Header Data: Standard";
            // 
            // textBoxMHImportAnimUnknown
            // 
            this.textBoxMHImportAnimUnknown.Location = new System.Drawing.Point(78, 51);
            this.textBoxMHImportAnimUnknown.Name = "textBoxMHImportAnimUnknown";
            this.textBoxMHImportAnimUnknown.Size = new System.Drawing.Size(100, 20);
            this.textBoxMHImportAnimUnknown.TabIndex = 6;
            this.textBoxMHImportAnimUnknown.Text = "0";
            // 
            // labelMHImportAnimUnknown
            // 
            this.labelMHImportAnimUnknown.AutoSize = true;
            this.labelMHImportAnimUnknown.Location = new System.Drawing.Point(15, 54);
            this.labelMHImportAnimUnknown.Name = "labelMHImportAnimUnknown";
            this.labelMHImportAnimUnknown.Size = new System.Drawing.Size(56, 13);
            this.labelMHImportAnimUnknown.TabIndex = 5;
            this.labelMHImportAnimUnknown.Text = "Unknown:";
            // 
            // textBoxMHBothImportFrom
            // 
            this.textBoxMHBothImportFrom.Location = new System.Drawing.Point(78, 25);
            this.textBoxMHBothImportFrom.Name = "textBoxMHBothImportFrom";
            this.textBoxMHBothImportFrom.Size = new System.Drawing.Size(100, 20);
            this.textBoxMHBothImportFrom.TabIndex = 4;
            this.textBoxMHBothImportFrom.Text = "aXXX_YYYYYY";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Import From:";
            // 
            // checkBoxMHStandardImportHKX
            // 
            this.checkBoxMHStandardImportHKX.AutoSize = true;
            this.checkBoxMHStandardImportHKX.Location = new System.Drawing.Point(284, 27);
            this.checkBoxMHStandardImportHKX.Name = "checkBoxMHStandardImportHKX";
            this.checkBoxMHStandardImportHKX.Size = new System.Drawing.Size(80, 17);
            this.checkBoxMHStandardImportHKX.TabIndex = 2;
            this.checkBoxMHStandardImportHKX.Text = "Import HKX";
            this.checkBoxMHStandardImportHKX.UseVisualStyleBackColor = true;
            // 
            // checkBoxMHStandardImportEvents
            // 
            this.checkBoxMHStandardImportEvents.AutoSize = true;
            this.checkBoxMHStandardImportEvents.Location = new System.Drawing.Point(188, 27);
            this.checkBoxMHStandardImportEvents.Name = "checkBoxMHStandardImportEvents";
            this.checkBoxMHStandardImportEvents.Size = new System.Drawing.Size(91, 17);
            this.checkBoxMHStandardImportEvents.TabIndex = 1;
            this.checkBoxMHStandardImportEvents.Text = "Import Events";
            this.checkBoxMHStandardImportEvents.UseVisualStyleBackColor = true;
            // 
            // checkBoxMHStandardLoopByDefault
            // 
            this.checkBoxMHStandardLoopByDefault.AutoSize = true;
            this.checkBoxMHStandardLoopByDefault.Location = new System.Drawing.Point(226, 50);
            this.checkBoxMHStandardLoopByDefault.Name = "checkBoxMHStandardLoopByDefault";
            this.checkBoxMHStandardLoopByDefault.Size = new System.Drawing.Size(107, 17);
            this.checkBoxMHStandardLoopByDefault.TabIndex = 0;
            this.checkBoxMHStandardLoopByDefault.Text = "Loops By Default";
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
            this.textBoxDisplayName.Location = new System.Drawing.Point(93, 36);
            this.textBoxDisplayName.Name = "textBoxDisplayName";
            this.textBoxDisplayName.Size = new System.Drawing.Size(318, 20);
            this.textBoxDisplayName.TabIndex = 8;
            this.textBoxDisplayName.Text = "aXXX_YYYYYY.hkt";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Mini-Header Type:";
            // 
            // radioButtonMHStandard
            // 
            this.radioButtonMHStandard.AutoSize = true;
            this.radioButtonMHStandard.Checked = true;
            this.radioButtonMHStandard.Location = new System.Drawing.Point(113, 71);
            this.radioButtonMHStandard.Name = "radioButtonMHStandard";
            this.radioButtonMHStandard.Size = new System.Drawing.Size(68, 17);
            this.radioButtonMHStandard.TabIndex = 10;
            this.radioButtonMHStandard.TabStop = true;
            this.radioButtonMHStandard.Text = "Standard";
            this.radioButtonMHStandard.UseVisualStyleBackColor = true;
            this.radioButtonMHStandard.CheckedChanged += new System.EventHandler(this.radioButtonMHStandard_CheckedChanged);
            // 
            // radioButtonMHImportOtherAnimation
            // 
            this.radioButtonMHImportOtherAnimation.AutoSize = true;
            this.radioButtonMHImportOtherAnimation.Location = new System.Drawing.Point(186, 71);
            this.radioButtonMHImportOtherAnimation.Name = "radioButtonMHImportOtherAnimation";
            this.radioButtonMHImportOtherAnimation.Size = new System.Drawing.Size(106, 17);
            this.radioButtonMHImportOtherAnimation.TabIndex = 11;
            this.radioButtonMHImportOtherAnimation.TabStop = true;
            this.radioButtonMHImportOtherAnimation.Text = "Direct Reference";
            this.radioButtonMHImportOtherAnimation.UseVisualStyleBackColor = true;
            this.radioButtonMHImportOtherAnimation.CheckedChanged += new System.EventHandler(this.radioButtonMHImportOtherAnimation_CheckedChanged);
            // 
            // TaeEditAnimPropertiesForm
            // 
            this.AcceptButton = this.buttonSaveChanges;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.buttonDiscardChanges;
            this.ClientSize = new System.Drawing.Size(401, 224);
            this.ControlBox = false;
            this.Controls.Add(this.radioButtonMHImportOtherAnimation);
            this.Controls.Add(this.radioButtonMHStandard);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxDisplayName);
            this.Controls.Add(this.textBoxAnimSubID);
            this.Controls.Add(this.groupBoxMiniHeader);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.labelAnimSubIDPrefix);
            this.Controls.Add(this.buttonDeleteAnim);
            this.Controls.Add(this.buttonSaveChanges);
            this.Controls.Add(this.buttonDiscardChanges);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(380, 198);
            this.Name = "TaeEditAnimPropertiesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Anim Info";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaeEditAnimPropertiesForm_FormClosing);
            this.Load += new System.EventHandler(this.TaeEditAnimPropertiesForm_Load);
            this.Shown += new System.EventHandler(this.TaeEditAnimPropertiesForm_Shown);
            this.groupBoxMiniHeader.ResumeLayout(false);
            this.groupBoxMiniHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonDiscardChanges;
        private System.Windows.Forms.Button buttonSaveChanges;
        private System.Windows.Forms.Button buttonDeleteAnim;
        private System.Windows.Forms.Label labelAnimSubIDPrefix;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxMiniHeader;
        private System.Windows.Forms.TextBox textBoxAnimSubID;
        private System.Windows.Forms.TextBox textBoxDisplayName;
        private System.Windows.Forms.CheckBox checkBoxMHStandardImportEvents;
        private System.Windows.Forms.CheckBox checkBoxMHStandardLoopByDefault;
        private System.Windows.Forms.CheckBox checkBoxMHStandardImportHKX;
        private System.Windows.Forms.TextBox textBoxMHBothImportFrom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxMHImportAnimUnknown;
        private System.Windows.Forms.Label labelMHImportAnimUnknown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radioButtonMHStandard;
        private System.Windows.Forms.RadioButton radioButtonMHImportOtherAnimation;
    }
}