using SoulsFormats;
using System;
using System.Windows.Forms;

namespace TAEDX.TaeEditor
{
    public partial class TaeEditAnimPropertiesForm : Form
    {
        TAE.Animation AnimRef;

        readonly long originalID;
        readonly bool originalIsReference;
        readonly string originalFileName;
        readonly int originalUnknown1;
        readonly int originalUnknown2;

        private bool ReadyToExit = false;

        public bool WereThingsChanged = false;

        public bool WasAnimIDChanged = false;

        public bool WasAnimDeleted = false;

        public TaeEditAnimPropertiesForm(TAE.Animation animRef)
        {
            AnimRef = animRef;
            originalID = animRef.ID;
            originalIsReference = animRef.AnimFileReference;
            originalFileName = animRef.AnimFileName;
            originalUnknown1 = animRef.Unknown1;
            originalUnknown2 = animRef.Unknown2;
            InitializeComponent();
        }

        private void TaeEditAnimPropertiesForm_Shown(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = AnimRef;
        }

        private void buttonSaveChanges_Click(object sender, EventArgs e)
        {
            WasAnimIDChanged = (AnimRef.ID != originalID);

            ReadyToExit = true;
            WereThingsChanged = true;
            Close();
        }

        private void buttonDiscardChanges_Click(object sender, EventArgs e)
        {
            AnimRef.ID = originalID;
            AnimRef.AnimFileName = originalFileName;
            AnimRef.AnimFileReference = originalIsReference;
            AnimRef.Unknown1 = originalUnknown1;
            AnimRef.Unknown2 = originalUnknown2;
            ReadyToExit = true;
            WereThingsChanged = false;
            Close();
        }

        private void TaeEditAnimPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !ReadyToExit;
        }

        private void buttonDeleteAnim_Click(object sender, EventArgs e)
        {
            var yesNoDlgResult = MessageBox.Show(
                $"Are you sure you want to delete animation entry {originalID}?\nThis can NOT be undone!",
                "Permanently Delete Animation Entry?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (yesNoDlgResult == DialogResult.Yes)
            {
                WasAnimDeleted = true;
                ReadyToExit = true;
                Close();
            }
        }
    }
}
