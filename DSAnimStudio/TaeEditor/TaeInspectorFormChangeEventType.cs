using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class TaeInspectorFormChangeEventType : Form
    {
        public TAE.Template.EventTemplate CurrentTemplate;
        public int NewEventType;
        public TAE TAEReference;

        private List<TAE.Template.EventTemplate> TemplatesList;

        public TaeInspectorFormChangeEventType()
        {
            InitializeComponent();
        }

        private void InspectorFormChangeEventType_Load(object sender, EventArgs e)
        {
            TemplatesList = new List<TAE.Template.EventTemplate>();
            listBoxEventTypes.Items.Clear();
            foreach (var v in TAEReference.BankTemplate.Values)
            {
                TemplatesList.Add(v);
                listBoxEventTypes.Items.Add($"{(v.ID):D3}: {v.Name}");
            }

            listBoxEventTypes.SelectedIndex = TemplatesList.IndexOf(CurrentTemplate);

            if (listBoxEventTypes.SelectedIndex < 0)
                listBoxEventTypes.SelectedIndex = 0;

            listBoxEventTypes.Focus();
            listBoxEventTypes.KeyDown += ListBoxEventTypes_KeyDown;

            //var eventTypes = (st[])Enum.GetValues(typeof(TimeActEventType));
            //listBoxEventTypes.Items.Clear();
            //foreach (var et in eventTypes)
            //{
            //    listBoxEventTypes.Items.Add($"{((int)et):D3}: {et.ToString()}");
            //}
            //listBoxEventTypes.SelectedIndex = listBoxEventTypes.Items.IndexOf($"{((int)NewEventType):D3}: {NewEventType.ToString()}");
            //listBoxEventTypes.Focus();
            //listBoxEventTypes.KeyDown += ListBoxEventTypes_KeyDown;
        }

        private void SelectEventAndClose()
        {
            NewEventType = TemplatesList[listBoxEventTypes.SelectedIndex].ID;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ListBoxEventTypes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectEventAndClose();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectEventAndClose();
        }
    }
}
