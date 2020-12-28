namespace DSAnimStudio
{
    partial class SapImportFbxAnimForm
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
            this.groupBoxSettings = new System.Windows.Forms.GroupBox();
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks = new System.Windows.Forms.CheckBox();
            this.fbxAnim_InitializeTracksToTPose = new System.Windows.Forms.CheckBox();
            this.fbxAnim_EnableRootMotionRotation = new System.Windows.Forms.CheckBox();
            this.fbxAnim_NegateQuaternionW = new System.Windows.Forms.CheckBox();
            this.fbxAnim_NegateQuaternionZ = new System.Windows.Forms.CheckBox();
            this.fbxAnim_NegateQuaternionY = new System.Windows.Forms.CheckBox();
            this.fbxAnim_NegateQuaternionX = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.fbxAnim_BonesToFlipBackwards = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.fbxAnim_RootMotionNodeName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.fbxAnim_RotationQuantizationType = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.fbxAnim_SampleToFramerate = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.fbxAnim_RotationTolerance = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.fbxAnim_checkboxMirrorQuaternions = new System.Windows.Forms.CheckBox();
            this.flver2_checkBoxConvertFromZUp = new System.Windows.Forms.CheckBox();
            this.flver2_numericUpDownScale = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonBrowseFBX = new System.Windows.Forms.Button();
            this.textBoxFBX = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonImport = new System.Windows.Forms.Button();
            this.buttonImportToLooseHKX = new System.Windows.Forms.Button();
            this.buttonImportDirectlyTest = new System.Windows.Forms.Button();
            this.richTextBoxHint = new System.Windows.Forms.RichTextBox();
            this.fbxAnim_OverrideRootMotionScale = new System.Windows.Forms.CheckBox();
            this.fbxAnim_OverrideRootMotionScale_Amount = new System.Windows.Forms.NumericUpDown();
            this.groupBoxSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fbxAnim_SampleToFramerate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fbxAnim_RotationTolerance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.flver2_numericUpDownScale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fbxAnim_OverrideRootMotionScale_Amount)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxSettings
            // 
            this.groupBoxSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSettings.Controls.Add(this.fbxAnim_OverrideRootMotionScale_Amount);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_OverrideRootMotionScale);
            this.groupBoxSettings.Controls.Add(this.richTextBoxHint);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_InitializeTracksToTPose);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_EnableRootMotionRotation);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_NegateQuaternionW);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_NegateQuaternionZ);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_NegateQuaternionY);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_NegateQuaternionX);
            this.groupBoxSettings.Controls.Add(this.label8);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_BonesToFlipBackwards);
            this.groupBoxSettings.Controls.Add(this.label7);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_RootMotionNodeName);
            this.groupBoxSettings.Controls.Add(this.label6);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_RotationQuantizationType);
            this.groupBoxSettings.Controls.Add(this.label5);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_SampleToFramerate);
            this.groupBoxSettings.Controls.Add(this.label4);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_RotationTolerance);
            this.groupBoxSettings.Controls.Add(this.label3);
            this.groupBoxSettings.Controls.Add(this.fbxAnim_checkboxMirrorQuaternions);
            this.groupBoxSettings.Controls.Add(this.flver2_checkBoxConvertFromZUp);
            this.groupBoxSettings.Controls.Add(this.flver2_numericUpDownScale);
            this.groupBoxSettings.Controls.Add(this.label2);
            this.groupBoxSettings.Location = new System.Drawing.Point(15, 31);
            this.groupBoxSettings.Name = "groupBoxSettings";
            this.groupBoxSettings.Size = new System.Drawing.Size(857, 368);
            this.groupBoxSettings.TabIndex = 13;
            this.groupBoxSettings.TabStop = false;
            this.groupBoxSettings.Text = "Import Settings";
            // 
            // fbxAnim_ExcludeRootMotionNodeFromTransformTracks
            // 
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Checked = true;
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Location = new System.Drawing.Point(585, 85);
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Name = "fbxAnim_ExcludeRootMotionNodeFromTransformTracks";
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Size = new System.Drawing.Size(234, 33);
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.TabIndex = 25;
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Text = "Exclude Specified Root Motion Node From Animation Tracks";
            this.fbxAnim_ExcludeRootMotionNodeFromTransformTracks.UseVisualStyleBackColor = true;
            // 
            // fbxAnim_InitializeTracksToTPose
            // 
            this.fbxAnim_InitializeTracksToTPose.AutoSize = true;
            this.fbxAnim_InitializeTracksToTPose.Location = new System.Drawing.Point(25, 150);
            this.fbxAnim_InitializeTracksToTPose.Name = "fbxAnim_InitializeTracksToTPose";
            this.fbxAnim_InitializeTracksToTPose.Size = new System.Drawing.Size(188, 17);
            this.fbxAnim_InitializeTracksToTPose.TabIndex = 24;
            this.fbxAnim_InitializeTracksToTPose.Text = "Put Un-Animated Bones In T-Pose";
            this.fbxAnim_InitializeTracksToTPose.UseVisualStyleBackColor = true;
            // 
            // fbxAnim_EnableRootMotionRotation
            // 
            this.fbxAnim_EnableRootMotionRotation.AutoSize = true;
            this.fbxAnim_EnableRootMotionRotation.Checked = true;
            this.fbxAnim_EnableRootMotionRotation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fbxAnim_EnableRootMotionRotation.Location = new System.Drawing.Point(585, 59);
            this.fbxAnim_EnableRootMotionRotation.Name = "fbxAnim_EnableRootMotionRotation";
            this.fbxAnim_EnableRootMotionRotation.Size = new System.Drawing.Size(171, 17);
            this.fbxAnim_EnableRootMotionRotation.TabIndex = 23;
            this.fbxAnim_EnableRootMotionRotation.Text = "Enable Rotational Root Motion";
            this.fbxAnim_EnableRootMotionRotation.UseVisualStyleBackColor = true;
            // 
            // fbxAnim_NegateQuaternionW
            // 
            this.fbxAnim_NegateQuaternionW.AutoSize = true;
            this.fbxAnim_NegateQuaternionW.Location = new System.Drawing.Point(169, 121);
            this.fbxAnim_NegateQuaternionW.Name = "fbxAnim_NegateQuaternionW";
            this.fbxAnim_NegateQuaternionW.Size = new System.Drawing.Size(37, 17);
            this.fbxAnim_NegateQuaternionW.TabIndex = 22;
            this.fbxAnim_NegateQuaternionW.Text = "W";
            this.fbxAnim_NegateQuaternionW.UseVisualStyleBackColor = true;
            // 
            // fbxAnim_NegateQuaternionZ
            // 
            this.fbxAnim_NegateQuaternionZ.AutoSize = true;
            this.fbxAnim_NegateQuaternionZ.Location = new System.Drawing.Point(130, 121);
            this.fbxAnim_NegateQuaternionZ.Name = "fbxAnim_NegateQuaternionZ";
            this.fbxAnim_NegateQuaternionZ.Size = new System.Drawing.Size(33, 17);
            this.fbxAnim_NegateQuaternionZ.TabIndex = 21;
            this.fbxAnim_NegateQuaternionZ.Text = "Z";
            this.fbxAnim_NegateQuaternionZ.UseVisualStyleBackColor = true;
            // 
            // fbxAnim_NegateQuaternionY
            // 
            this.fbxAnim_NegateQuaternionY.AutoSize = true;
            this.fbxAnim_NegateQuaternionY.Location = new System.Drawing.Point(91, 121);
            this.fbxAnim_NegateQuaternionY.Name = "fbxAnim_NegateQuaternionY";
            this.fbxAnim_NegateQuaternionY.Size = new System.Drawing.Size(33, 17);
            this.fbxAnim_NegateQuaternionY.TabIndex = 20;
            this.fbxAnim_NegateQuaternionY.Text = "Y";
            this.fbxAnim_NegateQuaternionY.UseVisualStyleBackColor = true;
            // 
            // fbxAnim_NegateQuaternionX
            // 
            this.fbxAnim_NegateQuaternionX.AutoSize = true;
            this.fbxAnim_NegateQuaternionX.Location = new System.Drawing.Point(52, 121);
            this.fbxAnim_NegateQuaternionX.Name = "fbxAnim_NegateQuaternionX";
            this.fbxAnim_NegateQuaternionX.Size = new System.Drawing.Size(33, 17);
            this.fbxAnim_NegateQuaternionX.TabIndex = 19;
            this.fbxAnim_NegateQuaternionX.Text = "X";
            this.fbxAnim_NegateQuaternionX.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(22, 105);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(162, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Negate Quaternion Components:";
            // 
            // fbxAnim_BonesToFlipBackwards
            // 
            this.fbxAnim_BonesToFlipBackwards.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fbxAnim_BonesToFlipBackwards.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fbxAnim_BonesToFlipBackwards.Location = new System.Drawing.Point(14, 234);
            this.fbxAnim_BonesToFlipBackwards.Multiline = true;
            this.fbxAnim_BonesToFlipBackwards.Name = "fbxAnim_BonesToFlipBackwards";
            this.fbxAnim_BonesToFlipBackwards.Size = new System.Drawing.Size(662, 120);
            this.fbxAnim_BonesToFlipBackwards.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 217);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(347, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Names of Bones To Rotate 180 Degrees About the Y Axis (for hotfixing):";
            // 
            // fbxAnim_RootMotionNodeName
            // 
            this.fbxAnim_RootMotionNodeName.Location = new System.Drawing.Point(719, 33);
            this.fbxAnim_RootMotionNodeName.Name = "fbxAnim_RootMotionNodeName";
            this.fbxAnim_RootMotionNodeName.Size = new System.Drawing.Size(100, 20);
            this.fbxAnim_RootMotionNodeName.TabIndex = 15;
            this.fbxAnim_RootMotionNodeName.Text = "root";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(582, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(128, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "Root Motion Node Name:";
            // 
            // fbxAnim_RotationQuantizationType
            // 
            this.fbxAnim_RotationQuantizationType.FormattingEnabled = true;
            this.fbxAnim_RotationQuantizationType.Items.AddRange(new object[] {
            "POLAR32 (DS Anim Studio cannot load)",
            "THREECOMP40 (Recommended)",
            "THREECOMP48",
            "THREECOMP24 (DS Anim Studio cannot load)",
            "STRAIGHT16",
            "UNCOMPRESSED"});
            this.fbxAnim_RotationQuantizationType.Location = new System.Drawing.Point(297, 85);
            this.fbxAnim_RotationQuantizationType.Name = "fbxAnim_RotationQuantizationType";
            this.fbxAnim_RotationQuantizationType.Size = new System.Drawing.Size(244, 82);
            this.fbxAnim_RotationQuantizationType.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(294, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(139, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Rotation Quantization Type:";
            // 
            // fbxAnim_SampleToFramerate
            // 
            this.fbxAnim_SampleToFramerate.DecimalPlaces = 2;
            this.fbxAnim_SampleToFramerate.Location = new System.Drawing.Point(139, 167);
            this.fbxAnim_SampleToFramerate.Maximum = new decimal(new int[] {
            240,
            0,
            0,
            0});
            this.fbxAnim_SampleToFramerate.Minimum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.fbxAnim_SampleToFramerate.Name = "fbxAnim_SampleToFramerate";
            this.fbxAnim_SampleToFramerate.Size = new System.Drawing.Size(120, 20);
            this.fbxAnim_SampleToFramerate.TabIndex = 10;
            this.fbxAnim_SampleToFramerate.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(22, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Sample To Framerate:";
            // 
            // fbxAnim_RotationTolerance
            // 
            this.fbxAnim_RotationTolerance.DecimalPlaces = 5;
            this.fbxAnim_RotationTolerance.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.fbxAnim_RotationTolerance.Location = new System.Drawing.Point(401, 36);
            this.fbxAnim_RotationTolerance.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.fbxAnim_RotationTolerance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            327680});
            this.fbxAnim_RotationTolerance.Name = "fbxAnim_RotationTolerance";
            this.fbxAnim_RotationTolerance.Size = new System.Drawing.Size(120, 20);
            this.fbxAnim_RotationTolerance.TabIndex = 8;
            this.fbxAnim_RotationTolerance.Value = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(294, 39);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Rotation Tolerance:";
            // 
            // fbxAnim_checkboxMirrorQuaternions
            // 
            this.fbxAnim_checkboxMirrorQuaternions.AutoSize = true;
            this.fbxAnim_checkboxMirrorQuaternions.Location = new System.Drawing.Point(25, 85);
            this.fbxAnim_checkboxMirrorQuaternions.Name = "fbxAnim_checkboxMirrorQuaternions";
            this.fbxAnim_checkboxMirrorQuaternions.Size = new System.Drawing.Size(175, 17);
            this.fbxAnim_checkboxMirrorQuaternions.TabIndex = 7;
            this.fbxAnim_checkboxMirrorQuaternions.Text = "Turn Bone Rotations Inside Out";
            this.fbxAnim_checkboxMirrorQuaternions.UseVisualStyleBackColor = true;
            // 
            // flver2_checkBoxConvertFromZUp
            // 
            this.flver2_checkBoxConvertFromZUp.AutoSize = true;
            this.flver2_checkBoxConvertFromZUp.Location = new System.Drawing.Point(25, 62);
            this.flver2_checkBoxConvertFromZUp.Name = "flver2_checkBoxConvertFromZUp";
            this.flver2_checkBoxConvertFromZUp.Size = new System.Drawing.Size(217, 17);
            this.flver2_checkBoxConvertFromZUp.TabIndex = 6;
            this.flver2_checkBoxConvertFromZUp.Text = "Convert from Z-Up to Y-Up (Deprecated)";
            this.flver2_checkBoxConvertFromZUp.UseVisualStyleBackColor = true;
            // 
            // flver2_numericUpDownScale
            // 
            this.flver2_numericUpDownScale.DecimalPlaces = 2;
            this.flver2_numericUpDownScale.Location = new System.Drawing.Point(139, 36);
            this.flver2_numericUpDownScale.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.flver2_numericUpDownScale.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.flver2_numericUpDownScale.Name = "flver2_numericUpDownScale";
            this.flver2_numericUpDownScale.Size = new System.Drawing.Size(120, 20);
            this.flver2_numericUpDownScale.TabIndex = 4;
            this.flver2_numericUpDownScale.Value = new decimal(new int[] {
            1000,
            0,
            0,
            65536});
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Scene Scale Percent:";
            // 
            // buttonBrowseFBX
            // 
            this.buttonBrowseFBX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowseFBX.Location = new System.Drawing.Point(797, 3);
            this.buttonBrowseFBX.Name = "buttonBrowseFBX";
            this.buttonBrowseFBX.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseFBX.TabIndex = 12;
            this.buttonBrowseFBX.Text = "Browse...";
            this.buttonBrowseFBX.UseVisualStyleBackColor = true;
            this.buttonBrowseFBX.Click += new System.EventHandler(this.buttonBrowseFBX_Click);
            // 
            // textBoxFBX
            // 
            this.textBoxFBX.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFBX.Location = new System.Drawing.Point(67, 5);
            this.textBoxFBX.Name = "textBoxFBX";
            this.textBoxFBX.Size = new System.Drawing.Size(724, 20);
            this.textBoxFBX.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "FBX File:";
            // 
            // buttonImport
            // 
            this.buttonImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImport.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonImport.Location = new System.Drawing.Point(586, 405);
            this.buttonImport.Name = "buttonImport";
            this.buttonImport.Size = new System.Drawing.Size(286, 42);
            this.buttonImport.TabIndex = 9;
            this.buttonImport.Text = "IMPORT FBX TO CURRENT ANIMATION\r\n(Not recommended for c0000)";
            this.buttonImport.UseVisualStyleBackColor = true;
            this.buttonImport.Click += new System.EventHandler(this.buttonImport_Click);
            // 
            // buttonImportToLooseHKX
            // 
            this.buttonImportToLooseHKX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImportToLooseHKX.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonImportToLooseHKX.Location = new System.Drawing.Point(363, 405);
            this.buttonImportToLooseHKX.Name = "buttonImportToLooseHKX";
            this.buttonImportToLooseHKX.Size = new System.Drawing.Size(217, 42);
            this.buttonImportToLooseHKX.TabIndex = 14;
            this.buttonImportToLooseHKX.Text = "IMPORT FBX TO LOOSE HKX FILE";
            this.buttonImportToLooseHKX.UseVisualStyleBackColor = true;
            this.buttonImportToLooseHKX.Click += new System.EventHandler(this.buttonImportToLooseHKX_Click);
            // 
            // buttonImportDirectlyTest
            // 
            this.buttonImportDirectlyTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonImportDirectlyTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonImportDirectlyTest.Location = new System.Drawing.Point(12, 405);
            this.buttonImportDirectlyTest.Name = "buttonImportDirectlyTest";
            this.buttonImportDirectlyTest.Size = new System.Drawing.Size(345, 42);
            this.buttonImportDirectlyTest.TabIndex = 15;
            this.buttonImportDirectlyTest.Text = "IMPORT STRAIGHT INTO DSAS UNCOMPRESSED\r\n(Does NOT save to game data; for testing " +
    "only)";
            this.buttonImportDirectlyTest.UseVisualStyleBackColor = true;
            this.buttonImportDirectlyTest.Click += new System.EventHandler(this.buttonImportDirectlyTest_Click);
            // 
            // richTextBoxHint
            // 
            this.richTextBoxHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxHint.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxHint.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxHint.Location = new System.Drawing.Point(682, 234);
            this.richTextBoxHint.Name = "richTextBoxHint";
            this.richTextBoxHint.ReadOnly = true;
            this.richTextBoxHint.Size = new System.Drawing.Size(169, 120);
            this.richTextBoxHint.TabIndex = 26;
            this.richTextBoxHint.Text = "For DS3 c0000 try these:\n\nRootPos\nL_Foot_Target2\nR_Foot_Target2";
            // 
            // fbxAnim_OverrideRootMotionScale
            // 
            this.fbxAnim_OverrideRootMotionScale.AutoSize = true;
            this.fbxAnim_OverrideRootMotionScale.Location = new System.Drawing.Point(585, 124);
            this.fbxAnim_OverrideRootMotionScale.Name = "fbxAnim_OverrideRootMotionScale";
            this.fbxAnim_OverrideRootMotionScale.Size = new System.Drawing.Size(160, 17);
            this.fbxAnim_OverrideRootMotionScale.TabIndex = 27;
            this.fbxAnim_OverrideRootMotionScale.Text = "Override Root Motion Scale:";
            this.fbxAnim_OverrideRootMotionScale.UseVisualStyleBackColor = true;
            this.fbxAnim_OverrideRootMotionScale.CheckedChanged += new System.EventHandler(this.fbxAnim_OverrideRootMotionScale_CheckedChanged);
            // 
            // fbxAnim_OverrideRootMotionScale_Amount
            // 
            this.fbxAnim_OverrideRootMotionScale_Amount.DecimalPlaces = 2;
            this.fbxAnim_OverrideRootMotionScale_Amount.Enabled = false;
            this.fbxAnim_OverrideRootMotionScale_Amount.Location = new System.Drawing.Point(744, 121);
            this.fbxAnim_OverrideRootMotionScale_Amount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.fbxAnim_OverrideRootMotionScale_Amount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.fbxAnim_OverrideRootMotionScale_Amount.Name = "fbxAnim_OverrideRootMotionScale_Amount";
            this.fbxAnim_OverrideRootMotionScale_Amount.Size = new System.Drawing.Size(75, 20);
            this.fbxAnim_OverrideRootMotionScale_Amount.TabIndex = 28;
            this.fbxAnim_OverrideRootMotionScale_Amount.Value = new decimal(new int[] {
            1000,
            0,
            0,
            65536});
            // 
            // SapImportFbxAnimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 459);
            this.Controls.Add(this.buttonImportDirectlyTest);
            this.Controls.Add(this.buttonImportToLooseHKX);
            this.Controls.Add(this.groupBoxSettings);
            this.Controls.Add(this.buttonBrowseFBX);
            this.Controls.Add(this.textBoxFBX);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonImport);
            this.MinimumSize = new System.Drawing.Size(900, 498);
            this.Name = "SapImportFbxAnimForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Animation FBX";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SapImportFbxAnimForm_Load);
            this.groupBoxSettings.ResumeLayout(false);
            this.groupBoxSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fbxAnim_SampleToFramerate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fbxAnim_RotationTolerance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.flver2_numericUpDownScale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fbxAnim_OverrideRootMotionScale_Amount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxSettings;
        private System.Windows.Forms.CheckBox fbxAnim_checkboxMirrorQuaternions;
        private System.Windows.Forms.CheckBox flver2_checkBoxConvertFromZUp;
        private System.Windows.Forms.Button buttonBrowseFBX;
        private System.Windows.Forms.TextBox textBoxFBX;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonImport;
        private System.Windows.Forms.NumericUpDown fbxAnim_RotationTolerance;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown fbxAnim_SampleToFramerate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown flver2_numericUpDownScale;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox fbxAnim_RotationQuantizationType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox fbxAnim_RootMotionNodeName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox fbxAnim_NegateQuaternionW;
        private System.Windows.Forms.CheckBox fbxAnim_NegateQuaternionZ;
        private System.Windows.Forms.CheckBox fbxAnim_NegateQuaternionY;
        private System.Windows.Forms.CheckBox fbxAnim_NegateQuaternionX;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox fbxAnim_BonesToFlipBackwards;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonImportToLooseHKX;
        private System.Windows.Forms.Button buttonImportDirectlyTest;
        private System.Windows.Forms.CheckBox fbxAnim_EnableRootMotionRotation;
        private System.Windows.Forms.CheckBox fbxAnim_InitializeTracksToTPose;
        private System.Windows.Forms.CheckBox fbxAnim_ExcludeRootMotionNodeFromTransformTracks;
        private System.Windows.Forms.RichTextBox richTextBoxHint;
        private System.Windows.Forms.NumericUpDown fbxAnim_OverrideRootMotionScale_Amount;
        private System.Windows.Forms.CheckBox fbxAnim_OverrideRootMotionScale;
    }
}