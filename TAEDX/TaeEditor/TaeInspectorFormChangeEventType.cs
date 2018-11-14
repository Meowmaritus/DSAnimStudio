using MeowDSIO.DataTypes.TAE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TAEDX.TaeEditor
{
    public partial class TaeInspectorFormChangeEventType : Form
    {
        public TimeActEventType NewEventType;

        public TaeInspectorFormChangeEventType()
        {
            InitializeComponent();
        }

        private void InspectorFormChangeEventType_Load(object sender, EventArgs e)
        {
            var eventTypes = (TimeActEventType[])Enum.GetValues(typeof(TimeActEventType));
            listBoxEventTypes.Items.Clear();
            foreach (var et in eventTypes)
            {
                listBoxEventTypes.Items.Add($"{((int)et):D3}: {et.ToString()}");
            }
            listBoxEventTypes.SelectedIndex = listBoxEventTypes.Items.IndexOf($"{((int)NewEventType):D3}: {NewEventType.ToString()}");
            listBoxEventTypes.Focus();
            listBoxEventTypes.KeyDown += ListBoxEventTypes_KeyDown;
        }

        private void ListBoxEventTypes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NewEventType = (TimeActEventType)Enum.Parse(typeof(TimeActEventType), listBoxEventTypes.SelectedItem.ToString().Substring(5));
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NewEventType = (TimeActEventType)Enum.Parse(typeof(TimeActEventType), listBoxEventTypes.SelectedItem.ToString());
            Close();
        }
    }
}
