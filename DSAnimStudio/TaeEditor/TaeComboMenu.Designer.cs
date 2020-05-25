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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewComboEntries = new System.Windows.Forms.DataGridView();
            this.columnAnimationID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnEvent0CancelType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonPlayCombo = new System.Windows.Forms.Button();
            this.buttonCancelCombo = new System.Windows.Forms.Button();
            this.buttonCopyToClipboard = new System.Windows.Forms.Button();
            this.buttonPasteFromClipboard = new System.Windows.Forms.Button();
            this.checkBoxLoop = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewComboEntries)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewComboEntries
            // 
            this.dataGridViewComboEntries.AllowUserToResizeRows = false;
            this.dataGridViewComboEntries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewComboEntries.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.dataGridViewComboEntries.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewComboEntries.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewComboEntries.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewComboEntries.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnAnimationID,
            this.columnEvent0CancelType});
            this.dataGridViewComboEntries.EnableHeadersVisualStyles = false;
            this.dataGridViewComboEntries.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.dataGridViewComboEntries.Location = new System.Drawing.Point(13, 13);
            this.dataGridViewComboEntries.Name = "dataGridViewComboEntries";
            this.dataGridViewComboEntries.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewComboEntries.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewComboEntries.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            this.dataGridViewComboEntries.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewComboEntries.Size = new System.Drawing.Size(559, 302);
            this.dataGridViewComboEntries.TabIndex = 0;
            this.dataGridViewComboEntries.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewComboEntries_CellEndEdit);
            this.dataGridViewComboEntries.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dataGridViewComboEntries_EditingControlShowing);
            this.dataGridViewComboEntries.NewRowNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.dataGridViewComboEntries_NewRowNeeded);
            this.dataGridViewComboEntries.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridViewComboEntries_MouseUp);
            // 
            // columnAnimationID
            // 
            this.columnAnimationID.HeaderText = "Animation ID";
            this.columnAnimationID.Name = "columnAnimationID";
            this.columnAnimationID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnAnimationID.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // columnEvent0CancelType
            // 
            this.columnEvent0CancelType.HeaderText = "JumpTable To Transition To Next";
            this.columnEvent0CancelType.Name = "columnEvent0CancelType";
            this.columnEvent0CancelType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.columnEvent0CancelType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnEvent0CancelType.ToolTipText = "The JumpTable it must reach in order to go to the next animation in the combo.";
            this.columnEvent0CancelType.Width = 400;
            // 
            // buttonPlayCombo
            // 
            this.buttonPlayCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPlayCombo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPlayCombo.ForeColor = System.Drawing.Color.White;
            this.buttonPlayCombo.Location = new System.Drawing.Point(428, 325);
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
            this.buttonCancelCombo.Location = new System.Drawing.Point(331, 325);
            this.buttonCancelCombo.Name = "buttonCancelCombo";
            this.buttonCancelCombo.Size = new System.Drawing.Size(91, 23);
            this.buttonCancelCombo.TabIndex = 2;
            this.buttonCancelCombo.Text = "Stop Combo";
            this.buttonCancelCombo.UseVisualStyleBackColor = true;
            this.buttonCancelCombo.Click += new System.EventHandler(this.buttonCancelCombo_Click);
            // 
            // buttonCopyToClipboard
            // 
            this.buttonCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopyToClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonCopyToClipboard.ForeColor = System.Drawing.Color.White;
            this.buttonCopyToClipboard.Location = new System.Drawing.Point(13, 326);
            this.buttonCopyToClipboard.Name = "buttonCopyToClipboard";
            this.buttonCopyToClipboard.Size = new System.Drawing.Size(131, 23);
            this.buttonCopyToClipboard.TabIndex = 3;
            this.buttonCopyToClipboard.Text = "Copy To Clipboard";
            this.buttonCopyToClipboard.UseVisualStyleBackColor = true;
            this.buttonCopyToClipboard.Click += new System.EventHandler(this.buttonCopyToClipboard_Click);
            // 
            // buttonPasteFromClipboard
            // 
            this.buttonPasteFromClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonPasteFromClipboard.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPasteFromClipboard.ForeColor = System.Drawing.Color.White;
            this.buttonPasteFromClipboard.Location = new System.Drawing.Point(150, 326);
            this.buttonPasteFromClipboard.Name = "buttonPasteFromClipboard";
            this.buttonPasteFromClipboard.Size = new System.Drawing.Size(131, 23);
            this.buttonPasteFromClipboard.TabIndex = 4;
            this.buttonPasteFromClipboard.Text = "Paste From Clipboard";
            this.buttonPasteFromClipboard.UseVisualStyleBackColor = true;
            this.buttonPasteFromClipboard.Click += new System.EventHandler(this.buttonPasteFromClipboard_Click);
            // 
            // checkBoxLoop
            // 
            this.checkBoxLoop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxLoop.AutoSize = true;
            this.checkBoxLoop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBoxLoop.ForeColor = System.Drawing.Color.White;
            this.checkBoxLoop.Location = new System.Drawing.Point(525, 329);
            this.checkBoxLoop.Name = "checkBoxLoop";
            this.checkBoxLoop.Size = new System.Drawing.Size(47, 17);
            this.checkBoxLoop.TabIndex = 5;
            this.checkBoxLoop.Text = "Loop";
            this.checkBoxLoop.UseVisualStyleBackColor = true;
            // 
            // TaeComboMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.checkBoxLoop);
            this.Controls.Add(this.buttonPasteFromClipboard);
            this.Controls.Add(this.buttonCopyToClipboard);
            this.Controls.Add(this.buttonCancelCombo);
            this.Controls.Add(this.buttonPlayCombo);
            this.Controls.Add(this.dataGridViewComboEntries);
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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewComboEntries)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewComboEntries;
        private System.Windows.Forms.Button buttonPlayCombo;
        private System.Windows.Forms.Button buttonCancelCombo;
        private System.Windows.Forms.Button buttonCopyToClipboard;
        private System.Windows.Forms.Button buttonPasteFromClipboard;
        private System.Windows.Forms.CheckBox checkBoxLoop;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnAnimationID;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnEvent0CancelType;
    }
}