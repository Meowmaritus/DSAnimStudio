using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public partial class RenameForm : Form
    {
        public RenameForm()
        {
            InitializeComponent();
        }

        public void Init(string title, string prompt, string defaultName)
        {
            Text = title;
            labelPrompt.Text = prompt;
            if (!string.IsNullOrWhiteSpace(defaultName))
            {
                textBoxName.Text = defaultName;
                textBoxName.SelectAll();
            }

            textBoxName.Focus();
        }

        public string NameEntered => textBoxName.Text;

        private void RenameForm_Load(object sender, EventArgs e)
        {

        }

        private void textBoxName_KeyPress(object sender, KeyPressEventArgs e)
        {

        }
    }
}
