namespace DSAnimStudio.TaeEditor
{
    partial class TaeFindValueDialog
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
            this.textBoxSearchQuery = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.labelResults = new System.Windows.Forms.Label();
            this.listViewResults = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonSearch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxSearchQuery
            // 
            this.textBoxSearchQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSearchQuery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.textBoxSearchQuery.ForeColor = System.Drawing.Color.White;
            this.textBoxSearchQuery.Location = new System.Drawing.Point(106, 12);
            this.textBoxSearchQuery.Name = "textBoxSearchQuery";
            this.textBoxSearchQuery.Size = new System.Drawing.Size(479, 20);
            this.textBoxSearchQuery.TabIndex = 0;
            this.textBoxSearchQuery.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBoxSearchQuery_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Search Query:";
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(672, 16);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(116, 17);
            this.checkBox1.TabIndex = 2;
            this.checkBox1.Text = "Match Entire Value";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // labelResults
            // 
            this.labelResults.AutoSize = true;
            this.labelResults.Location = new System.Drawing.Point(12, 57);
            this.labelResults.Name = "labelResults";
            this.labelResults.Size = new System.Drawing.Size(45, 13);
            this.labelResults.TabIndex = 3;
            this.labelResults.Text = "Results:";
            this.labelResults.Click += new System.EventHandler(this.Label2_Click);
            // 
            // listViewResults
            // 
            this.listViewResults.Activation = System.Windows.Forms.ItemActivation.TwoClick;
            this.listViewResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.listViewResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listViewResults.ForeColor = System.Drawing.Color.White;
            this.listViewResults.FullRowSelect = true;
            this.listViewResults.GridLines = true;
            this.listViewResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewResults.HideSelection = false;
            this.listViewResults.Location = new System.Drawing.Point(15, 73);
            this.listViewResults.MultiSelect = false;
            this.listViewResults.Name = "listViewResults";
            this.listViewResults.Size = new System.Drawing.Size(773, 365);
            this.listViewResults.TabIndex = 4;
            this.listViewResults.UseCompatibleStateImageBehavior = false;
            this.listViewResults.View = System.Windows.Forms.View.Details;
            this.listViewResults.VirtualMode = true;
            this.listViewResults.ItemActivate += new System.EventHandler(this.ListViewResults_ItemActivate);
            this.listViewResults.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.ListViewResults_RetrieveVirtualItem);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "TAE";
            this.columnHeader1.Width = 52;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Animation";
            this.columnHeader2.Width = 100;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Event Type";
            this.columnHeader3.Width = 300;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Parameter Name";
            this.columnHeader4.Width = 200;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Matched Value";
            this.columnHeader5.Width = 100;
            // 
            // buttonSearch
            // 
            this.buttonSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSearch.Location = new System.Drawing.Point(591, 10);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(75, 24);
            this.buttonSearch.TabIndex = 5;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
            // 
            // TaeFindValueDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.listViewResults);
            this.Controls.Add(this.labelResults);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxSearchQuery);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "TaeFindValueDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find Value";
            this.Load += new System.EventHandler(this.TaeFindValueDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSearchQuery;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label labelResults;
        private System.Windows.Forms.ListView listViewResults;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button buttonSearch;
    }
}