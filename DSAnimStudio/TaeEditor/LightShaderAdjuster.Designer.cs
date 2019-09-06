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
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDirectMult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIndirectMult)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Direct Light Mult:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(94, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Indirect Light Mult:";
            // 
            // trackBarDirectMult
            // 
            this.trackBarDirectMult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarDirectMult.AutoSize = false;
            this.trackBarDirectMult.LargeChange = 10;
            this.trackBarDirectMult.Location = new System.Drawing.Point(96, 18);
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
            // 
            // trackBarIndirectMult
            // 
            this.trackBarIndirectMult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.trackBarIndirectMult.AutoSize = false;
            this.trackBarIndirectMult.LargeChange = 10;
            this.trackBarIndirectMult.Location = new System.Drawing.Point(96, 37);
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
            // 
            // checkBoxDrawSkybox
            // 
            this.checkBoxDrawSkybox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxDrawSkybox.AutoSize = true;
            this.checkBoxDrawSkybox.Checked = true;
            this.checkBoxDrawSkybox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDrawSkybox.Location = new System.Drawing.Point(79, 56);
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
            this.label4.Location = new System.Drawing.Point(88, 2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(95, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "[Shader Config]";
            this.label4.Click += new System.EventHandler(this.Label4_Click);
            // 
            // LightShaderAdjuster
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.label4);
            this.Controls.Add(this.checkBoxDrawSkybox);
            this.Controls.Add(this.trackBarIndirectMult);
            this.Controls.Add(this.trackBarDirectMult);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "LightShaderAdjuster";
            this.Size = new System.Drawing.Size(185, 75);
            this.Load += new System.EventHandler(this.LightShaderAdjuster_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarDirectMult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIndirectMult)).EndInit();
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
    }
}
