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
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Label4_Click(object sender, EventArgs e)
        {
            if (Height == 18)
            {
                checkBoxDrawSkybox.Checked = Environment.DrawCubemap;
                trackBarDirectMult.Value = (int)(Environment.FlverDirectLightMult * 100);
                trackBarIndirectMult.Value = (int)(Environment.FlverIndirectLightMult * 100);
                trackBarEmissiveMult.Value = (int)(Environment.FlverEmissiveMult * 100);

                Height = 135;
                Width = 191;

            }
            else
            {
                Height = 18;
                Width = 100;
            }
        }

        private void LightShaderAdjuster_Load(object sender, EventArgs e)
        {
            Height = 18;
            Width = 100;
        }

        private void TrackBarDirectMult_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void TrackBarIndirectMult_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
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
        }

        private void TrackBarEmissiveMult_ValueChanged(object sender, EventArgs e)
        {
            Environment.FlverEmissiveMult = trackBarEmissiveMult.Value / 100f;
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
