namespace DSAnimStudio.TaeEditor
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.labelEventType = new System.Windows.Forms.Label();
            this.buttonChangeType = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.columnType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
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
            this.labelEventType.Text = "(Nothing Selected)";
            this.labelEventType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonChangeType
            // 
            this.buttonChangeType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChangeType.Enabled = false;
            this.buttonChangeType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChangeType.Location = new System.Drawing.Point(177, 4);
            this.buttonChangeType.Name = "buttonChangeType";
            this.buttonChangeType.Size = new System.Drawing.Size(150, 23);
            this.buttonChangeType.TabIndex = 3;
            this.buttonChangeType.TabStop = false;
            this.buttonChangeType.Text = "Change Event Type (F1)";
            this.buttonChangeType.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ColumnHeadersVisible = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnType,
            this.columnName,
            this.columnValue});
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.dataGridView1.Location = new System.Drawing.Point(5, 33);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            dataGridViewCellStyle6.BackColor = System.Drawing.Color.Gray;
            dataGridViewCellStyle6.ForeColor = System.Drawing.Color.White;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.Size = new System.Drawing.Size(321, 417);
            this.dataGridView1.TabIndex = 4;
            this.dataGridView1.TabStop = false;
            // 
            // columnType
            // 
            this.columnType.HeaderText = "Type";
            this.columnType.Name = "columnType";
            this.columnType.ReadOnly = true;
            this.columnType.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.columnType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnType.Width = 48;
            // 
            // columnName
            // 
            this.columnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.columnName.HeaderText = "Name";
            this.columnName.MinimumWidth = 48;
            this.columnName.Name = "columnName";
            this.columnName.ReadOnly = true;
            this.columnName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.columnName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.columnName.Width = 48;
            // 
            // columnValue
            // 
            this.columnValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnValue.HeaderText = "Value";
            this.columnValue.MinimumWidth = 48;
            this.columnValue.Name = "columnValue";
            this.columnValue.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // TaeInspectorWinFormsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.buttonChangeType);
            this.Controls.Add(this.labelEventType);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "TaeInspectorWinFormsControl";
            this.Size = new System.Drawing.Size(331, 453);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.Button buttonChangeType;
        public System.Windows.Forms.Label labelEventType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnType;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnValue;
        public System.Windows.Forms.DataGridView dataGridView1;
    }
}
