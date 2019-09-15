namespace DSAnimStudio.TaeEditor
{
    partial class LightShaderAdjuster
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBarDirectMult = new System.Windows.Forms.TrackBar();
            this.trackBarIndirectMult = new System.Windows.Forms.TrackBar();
            this.checkBoxDrawSkybox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.trackBarEmissiveMult = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxShowGrid = new System.Windows.Forms.CheckBox();
            this.trackBarExposure = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDirectMult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIndirectMult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEmissiveMult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExposure)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Direct Light Mult:";
            this.label2.Click += new System.EventHandler(this.Label2_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Indirect Light Mult:";
            this.label3.Click += new System.EventHandler(this.Label3_Click);
            // 
            // trackBarDirectMult
            // 
            this.trackBarDirectMult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarDirectMult.AutoSize = false;
            this.trackBarDirectMult.LargeChange = 10;
            this.trackBarDirectMult.Location = new System.Drawing.Point(102, 18);
            this.trackBarDirectMult.Maximum = 200;
            this.trackBarDirectMult.Name = "trackBarDirectMult";
            this.trackBarDirectMult.Size = new System.Drawing.Size(87, 20);
            this.trackBarDirectMult.TabIndex = 4;
            this.trackBarDirectMult.TabStop = false;
            this.trackBarDirectMult.TickFrequency = 100;
            this.trackBarDirectMult.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarDirectMult.Value = 100;
            this.trackBarDirectMult.ValueChanged += new System.EventHandler(this.TrackBarDirectMult_ValueChanged);
            this.trackBarDirectMult.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TrackBarDirectMult_KeyPress);
            this.trackBarDirectMult.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TrackBarDirectMult_MouseUp);
            // 
            // trackBarIndirectMult
            // 
            this.trackBarIndirectMult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarIndirectMult.AutoSize = false;
            this.trackBarIndirectMult.LargeChange = 10;
            this.trackBarIndirectMult.Location = new System.Drawing.Point(102, 37);
            this.trackBarIndirectMult.Maximum = 200;
            this.trackBarIndirectMult.Name = "trackBarIndirectMult";
            this.trackBarIndirectMult.Size = new System.Drawing.Size(87, 20);
            this.trackBarIndirectMult.TabIndex = 5;
            this.trackBarIndirectMult.TabStop = false;
            this.trackBarIndirectMult.TickFrequency = 100;
            this.trackBarIndirectMult.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarIndirectMult.Value = 100;
            this.trackBarIndirectMult.ValueChanged += new System.EventHandler(this.TrackBarIndirectMult_ValueChanged);
            this.trackBarIndirectMult.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TrackBarIndirectMult_KeyPress);
            this.trackBarIndirectMult.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TrackBarIndirectMult_MouseUp);
            // 
            // checkBoxDrawSkybox
            // 
            this.checkBoxDrawSkybox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxDrawSkybox.AutoSize = true;
            this.checkBoxDrawSkybox.Checked = true;
            this.checkBoxDrawSkybox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDrawSkybox.Location = new System.Drawing.Point(87, 96);
            this.checkBoxDrawSkybox.Name = "checkBoxDrawSkybox";
            this.checkBoxDrawSkybox.Size = new System.Drawing.Size(99, 17);
            this.checkBoxDrawSkybox.TabIndex = 6;
            this.checkBoxDrawSkybox.TabStop = false;
            this.checkBoxDrawSkybox.Text = "Render Skybox";
            this.checkBoxDrawSkybox.UseVisualStyleBackColor = true;
            this.checkBoxDrawSkybox.CheckedChanged += new System.EventHandler(this.CheckBoxDrawSkybox_CheckedChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Cursor = System.Windows.Forms.Cursors.Default;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(94, 2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "[Shader Config]";
            this.label4.Click += new System.EventHandler(this.Label4_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReset.Location = new System.Drawing.Point(111, 142);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 8;
            this.buttonReset.Text = "Reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.ButtonReset_Click);
            // 
            // trackBarEmissiveMult
            // 
            this.trackBarEmissiveMult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarEmissiveMult.AutoSize = false;
            this.trackBarEmissiveMult.LargeChange = 10;
            this.trackBarEmissiveMult.Location = new System.Drawing.Point(102, 56);
            this.trackBarEmissiveMult.Maximum = 200;
            this.trackBarEmissiveMult.Name = "trackBarEmissiveMult";
            this.trackBarEmissiveMult.Size = new System.Drawing.Size(87, 20);
            this.trackBarEmissiveMult.TabIndex = 9;
            this.trackBarEmissiveMult.TabStop = false;
            this.trackBarEmissiveMult.TickFrequency = 100;
            this.trackBarEmissiveMult.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarEmissiveMult.Value = 100;
            this.trackBarEmissiveMult.ValueChanged += new System.EventHandler(this.TrackBarEmissiveMult_ValueChanged);
            this.trackBarEmissiveMult.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TrackBarEmissiveMult_KeyPress);
            this.trackBarEmissiveMult.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TrackBarEmissiveMult_MouseUp);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Emissive Light Mult:";
            this.label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // checkBoxShowGrid
            // 
            this.checkBoxShowGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowGrid.AutoSize = true;
            this.checkBoxShowGrid.Checked = true;
            this.checkBoxShowGrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowGrid.Location = new System.Drawing.Point(111, 119);
            this.checkBoxShowGrid.Name = "checkBoxShowGrid";
            this.checkBoxShowGrid.Size = new System.Drawing.Size(75, 17);
            this.checkBoxShowGrid.TabIndex = 11;
            this.checkBoxShowGrid.TabStop = false;
            this.checkBoxShowGrid.Text = "Show Grid";
            this.checkBoxShowGrid.UseVisualStyleBackColor = true;
            this.checkBoxShowGrid.CheckedChanged += new System.EventHandler(this.CheckBoxShowGrid_CheckedChanged);
            // 
            // trackBarExposure
            // 
            this.trackBarExposure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarExposure.AutoSize = false;
            this.trackBarExposure.LargeChange = 10;
            this.trackBarExposure.Location = new System.Drawing.Point(102, 75);
            this.trackBarExposure.Maximum = 200;
            this.trackBarExposure.Name = "trackBarExposure";
            this.trackBarExposure.Size = new System.Drawing.Size(87, 20);
            this.trackBarExposure.TabIndex = 12;
            this.trackBarExposure.TabStop = false;
            this.trackBarExposure.TickFrequency = 100;
            this.trackBarExposure.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarExposure.Value = 100;
            this.trackBarExposure.ValueChanged += new System.EventHandler(this.TrackBarExposure_ValueChanged);
            this.trackBarExposure.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TrackBarExposure_MouseUp);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(88, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Scene Exposure:";
            // 
            // LightShaderAdjuster
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.trackBarExposure);
            this.Controls.Add(this.checkBoxShowGrid);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarEmissiveMult);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBoxDrawSkybox);
            this.Controls.Add(this.trackBarIndirectMult);
            this.Controls.Add(this.trackBarDirectMult);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "LightShaderAdjuster";
            this.Size = new System.Drawing.Size(191, 175);
            this.Load += new System.EventHandler(this.LightShaderAdjuster_Load);
            this.Click += new System.EventHandler(this.LightShaderAdjuster_Click);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDirectMult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIndirectMult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarEmissiveMult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarExposure)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBarDirectMult;
        private System.Windows.Forms.TrackBar trackBarIndirectMult;
        private System.Windows.Forms.CheckBox checkBoxDrawSkybox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.TrackBar trackBarEmissiveMult;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxShowGrid;
        private System.Windows.Forms.TrackBar trackBarExposure;
        private System.Windows.Forms.Label label5;
    }
}
