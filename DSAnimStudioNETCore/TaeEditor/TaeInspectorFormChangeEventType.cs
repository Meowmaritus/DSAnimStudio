using SoulsFormats;
using SoulsAssetPipeline.Animation;
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
            listViewEventType.Items.Clear();

            //ListViewItem currentEventType = null;

            foreach (var v in TAEReference.BankTemplate.Values)
            {
                TemplatesList.Add(v);
                listViewEventType.Items.Add(new ListViewItem(new string[] { v.ID.ToString(), v.Name } ));
            }

            //if (currentEventType != null)
            //{
            //    listViewEventType.SelectedItems.Clear();
            //    listViewEventType.Sel
            //}

            var selectedIndex = TemplatesList.IndexOf(CurrentTemplate);

            if (selectedIndex >= 0)
            {
                listViewEventType.SelectedIndices.Clear();
                listViewEventType.SelectedIndices.Add(selectedIndex);

                listViewEventType.Items[selectedIndex].Selected = true;
                listViewEventType.Items[selectedIndex].Focused = true;
                listViewEventType.Items[selectedIndex].EnsureVisible();
            }
            else
            {
                listViewEventType.SelectedIndices.Clear();
                

                if (listViewEventType.Items.Count > 0)
                {
                    listViewEventType.SelectedIndices.Add(0);

                    listViewEventType.Items[0].Selected = true;
                    listViewEventType.Items[0].Focused = true;
                    listViewEventType.Items[0].EnsureVisible();
                }
                else
                {
                    listViewEventType.SelectedIndices.Add(-1);
                }
            }

            //if (listBoxEventTypes.SelectedIndex < 0)
            //    listBoxEventTypes.SelectedIndex = 0;

            listViewEventType.Focus();
            listViewEventType.Select();
            listViewEventType.KeyDown += ListBoxEventTypes_KeyDown;

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
            NewEventType = TemplatesList[listViewEventType.SelectedIndices[0]].ID;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ListBoxEventTypes_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Enter)
            //{
            //    SelectEventAndClose();
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectEventAndClose();
        }

        private void ListViewEventType_ItemActivate(object sender, EventArgs e)
        {
            SelectEventAndClose();
        }
    }
}
