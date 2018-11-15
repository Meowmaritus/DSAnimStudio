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
    public partial class TaeEditAnimPropertiesForm : Form
    {
        AnimationRef AnimRef;

        readonly int originalID;
        readonly bool originalIsReference;
        readonly string originalFileName;
        readonly int originalRefAnimID;
        readonly bool originalIsDefaultObjAnim;
        readonly bool originalUseHKXOnly;
        readonly bool originalTAEDataOnly;
        readonly int originalOriginalAnimID;

        private bool ReadyToExit = false;

        public bool WereThingsChanged = false;

        public TaeEditAnimPropertiesForm(AnimationRef animRef)
        {
            AnimRef = animRef;
            originalID = animRef.ID;
            originalIsReference = animRef.IsReference;
            originalFileName = animRef.FileName + "";
            originalRefAnimID = animRef.RefAnimID;
            originalIsDefaultObjAnim = animRef.IsLoopingObjAnim;
            originalUseHKXOnly = animRef.UseHKXOnly;
            originalTAEDataOnly = animRef.TAEDataOnly;
            originalOriginalAnimID = animRef.OriginalAnimID;
            InitializeComponent();
        }

        private void TaeEditAnimPropertiesForm_Shown(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = AnimRef;
        }

        private void buttonSaveChanges_Click(object sender, EventArgs e)
        {
            ReadyToExit = true;
            WereThingsChanged = true;
            Close();
        }

        private void buttonDiscardChanges_Click(object sender, EventArgs e)
        {
            AnimRef.ID = originalID;
            AnimRef.FileName = originalFileName;
            AnimRef.IsReference = originalIsReference;
            AnimRef.RefAnimID = originalRefAnimID;
            AnimRef.IsLoopingObjAnim = originalIsDefaultObjAnim;
            AnimRef.UseHKXOnly = originalUseHKXOnly;
            AnimRef.TAEDataOnly = originalTAEDataOnly;
            AnimRef.OriginalAnimID = originalOriginalAnimID;
            ReadyToExit = true;
            WereThingsChanged = false;
            Close();
        }

        private void TaeEditAnimPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !ReadyToExit;
        }
    }
}
