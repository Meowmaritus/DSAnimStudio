namespace TAEDX.TaeEditor
{
    partial class TaeInspectorWinFormsControl
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
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.labelEventType = new System.Windows.Forms.Label();
            this.buttonChangeType = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(4, 32);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid.Size = new System.Drawing.Size(324, 418);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // labelEventType
            // 
            this.labelEventType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEventType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.labelEventType.Location = new System.Drawing.Point(4, 2);
            this.labelEventType.Name = "labelEventType";
            this.labelEventType.Size = new System.Drawing.Size(209, 28);
            this.labelEventType.TabIndex = 2;
            this.labelEventType.Text = "(None Selected)";
            this.labelEventType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonChangeType
            // 
            this.buttonChangeType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChangeType.Enabled = false;
            this.buttonChangeType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChangeType.Location = new System.Drawing.Point(219, 4);
            this.buttonChangeType.Name = "buttonChangeType";
            this.buttonChangeType.Size = new System.Drawing.Size(108, 23);
            this.buttonChangeType.TabIndex = 3;
            this.buttonChangeType.Text = "Change Type...";
            this.buttonChangeType.UseVisualStyleBackColor = true;
            // 
            // InspectorWinFormsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.buttonChangeType);
            this.Controls.Add(this.labelEventType);
            this.Controls.Add(this.propertyGrid);
            this.DoubleBuffered = true;
            this.Name = "InspectorWinFormsControl";
            this.Size = new System.Drawing.Size(331, 453);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PropertyGrid propertyGrid;
        public System.Windows.Forms.Button buttonChangeType;
        public System.Windows.Forms.Label labelEventType;
    }
}
