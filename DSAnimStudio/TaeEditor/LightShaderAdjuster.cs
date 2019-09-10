using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class LightShaderAdjuster : UserControl
    {
        public LightShaderAdjuster()
        {
            InitializeComponent();
        }

        private void TrackBarDirectMult_ValueChanged(object sender, EventArgs e)
        {
            Environment.FlverDirectLightMult = trackBarDirectMult.Value / 100f;
        }

        private void TrackBarIndirectMult_ValueChanged(object sender, EventArgs e)
        {
            Environment.FlverIndirectLightMult = trackBarIndirectMult.Value / 100f;
        }

        private void CheckBoxDrawSkybox_CheckedChanged(object sender, EventArgs e)
        {
            Environment.DrawCubemap = checkBoxDrawSkybox.Checked;
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void Label4_Click(object sender, EventArgs e)
        {
            if (Height == 18)
            {
                checkBoxDrawSkybox.Checked = Environment.DrawCubemap;
                trackBarDirectMult.Value = (int)(Environment.FlverDirectLightMult * 100);
                trackBarIndirectMult.Value = (int)(Environment.FlverIndirectLightMult * 100);
                trackBarEmissiveMult.Value = (int)(Environment.FlverEmissiveMult * 100);
                trackBarExposure.Value = (int)(Environment.FlverSceneBrightness * 100);
                //checkBoxShowGrid.Checked = GFX.UseTonemap;
                checkBoxShowGrid.Checked = DBG.ShowGrid;
                checkBoxDrawSkybox.Checked = Environment.DrawCubemap;

                Height = 175;
                Width = 191;

            }
            else
            {
                Height = 18;
                Width = 100;
            }
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void LightShaderAdjuster_Load(object sender, EventArgs e)
        {
            Height = 18;
            Width = 100;
        }

        private void TrackBarDirectMult_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void TrackBarIndirectMult_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void ButtonReset_Click(object sender, EventArgs e)
        {
            //if (TaeInterop.CurrentHkxVariation == SoulsFormats.HKX.HKXVariation.HKXDS1)
            //{
            //    checkBoxDrawSkybox.Checked = false;
            //}
            //else
            //{
            //    checkBoxDrawSkybox.Checked = true;
            //}

            checkBoxDrawSkybox.Checked = true;
            trackBarDirectMult.Value = 100;
            trackBarIndirectMult.Value = 100;
            trackBarEmissiveMult.Value = 100;
            checkBoxShowGrid.Checked = true;
            trackBarExposure.Value = 100;
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void TrackBarEmissiveMult_ValueChanged(object sender, EventArgs e)
        {
            Environment.FlverEmissiveMult = trackBarEmissiveMult.Value / 100f;
        }

        private void CheckBoxShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            //GFX.UseTonemap = checkBoxShowGrid.Checked;
            DBG.ShowGrid = checkBoxShowGrid.Checked;
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void TrackBarDirectMult_MouseUp(object sender, MouseEventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void TrackBarIndirectMult_MouseUp(object sender, MouseEventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void TrackBarEmissiveMult_MouseUp(object sender, MouseEventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void Label3_Click(object sender, EventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void Label2_Click(object sender, EventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void LightShaderAdjuster_Click(object sender, EventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void TrackBarEmissiveMult_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        private void TrackBarExposure_ValueChanged(object sender, EventArgs e)
        {
            Environment.FlverSceneBrightness = trackBarExposure.Value / 100f;
        }

        private void TrackBarExposure_MouseUp(object sender, MouseEventArgs e)
        {
            Main.TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
        }

        //protected override void OnPaintBackground(PaintEventArgs pevent)
        //{
        //    //base.OnPaintBackground(pevent);
        //}

        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    //base.OnPaint(e);
        //}
    }
}
