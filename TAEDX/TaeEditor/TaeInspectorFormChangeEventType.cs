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
                listBoxEventTypes.Items.Add(Text = et.ToString());
            }
            listBoxEventTypes.SelectedIndex = 0;
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
