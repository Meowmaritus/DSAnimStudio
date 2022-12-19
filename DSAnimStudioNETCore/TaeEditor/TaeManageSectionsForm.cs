using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class TaeManageSectionsForm : Form
    {
        private List<ManagerAction> ManagerActions = new List<ManagerAction>();

        private List<string> InitialSectionList = new List<string>();

        public void SetInitialSectionList(List<string> sectionList)
        {
            InitialSectionList = sectionList.ToList();
            ApplyManagerActions();
        }

        private void ApplyManagerActions()
        {
            var result = InitialSectionList.ToList();
            foreach (var act in ManagerActions)
            {
                if (act is ManagerAction.Add add)
                {
                    //result.Add($"{add.Name} <New>");
                    result.Add(add.Name);
                    result = result.OrderBy(x => x).ToList();
                }
                else if (act is ManagerAction.Clone clone)
                {
                    //result.Add($"{clone.NameDest} <Clone of {clone.NameSource}>");
                    result.Add(clone.NameDest);
                    result = result.OrderBy(x => x).ToList();
                }
                else if (act is ManagerAction.Remove remove)
                {
                    if (result.Contains(remove.Name))
                        result.Remove(remove.Name);
                    result.RemoveAll(s => s.StartsWith(remove.Name + " <"));
                }
                else if (act is ManagerAction.Rename rename)
                {
                    if (result.Contains(rename.NameSource))
                        result.Remove(rename.NameSource);
                    result.RemoveAll(s => s.StartsWith(rename.NameSource + " <"));

                    //result.Add($"{clone.NameDest} <Renamed from {clone.NameSource}>");
                    result.Add(rename.NameDest);
                    result = result.OrderBy(x => x).ToList();
                }
            }
            listBoxSections.Items.Clear();
            foreach (var x in result)
                listBoxSections.Items.Add(x);

            listBoxActions.Items.Clear();
            foreach (var x in ManagerActions)
                listBoxActions.Items.Add(x);
            //listBoxActions.SelectedIndex = listBoxActions.Items.Count - 1;
            listBoxActions.TopIndex = listBoxActions.Items.Count - 1;
        }

        public TaeManageSectionsForm()
        {
            InitializeComponent();
        }

        private void TaeManageSectionsForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var renameForm = new RenameForm();
            renameForm.Init("Add New Section", $"Enter name for new section:", "");
            if (renameForm.ShowDialog() == DialogResult.OK)
            {
                ManagerActions.Add(new ManagerAction.Add()
                {
                    Name = renameForm.NameEntered,
                });
            }
            ApplyManagerActions();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (listBoxSections.SelectedItem == null)
                return;

            ManagerActions.Add(new ManagerAction.Remove()
            {
                Name = listBoxSections.SelectedItem as string,
            });
            ApplyManagerActions();
        }

        private void buttonRename_Click(object sender, EventArgs e)
        {
            if (listBoxSections.SelectedItem == null)
                return;
            string selected = listBoxSections.SelectedItem as string;
            var renameForm = new RenameForm();
            renameForm.Init("Rename Section", $"Enter new name for section '{selected}':", selected);
            if (renameForm.ShowDialog() == DialogResult.OK)
            {
                ManagerActions.Add(new ManagerAction.Rename()
                {
                    NameSource = selected,
                    NameDest = renameForm.NameEntered,
                });
            }
            ApplyManagerActions();
        }

        private void buttonClone_Click(object sender, EventArgs e)
        {
            if (listBoxSections.SelectedItem == null)
                return;
            string selected = listBoxSections.SelectedItem as string;
            var renameForm = new RenameForm();
            renameForm.Init("Clone", $"Enter name for clone of section '{selected}':", "");
            if (renameForm.ShowDialog() == DialogResult.OK)
            {
                ManagerActions.Add(new ManagerAction.Clone()
                {
                    NameSource = selected,
                    NameDest = renameForm.NameEntered,
                });
            }
            ApplyManagerActions();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {

        }

        private void buttonUndoLast_Click(object sender, EventArgs e)
        {
            ManagerActions.RemoveAt(ManagerActions.Count - 1);
            ApplyManagerActions();
        }
    }
}
