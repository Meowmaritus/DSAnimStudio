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
            GFX.DirectLightMult = trackBarDirectMult.Value / 100f;
        }

        private void TrackBarIndirectMult_ValueChanged(object sender, EventArgs e)
        {
            GFX.IndirectLightMult = trackBarIndirectMult.Value / 100f;
        }

        private void CheckBoxDrawSkybox_CheckedChanged(object sender, EventArgs e)
        {
            GFX.DrawSkybox = checkBoxDrawSkybox.Checked;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Label4_Click(object sender, EventArgs e)
        {
            if (Height == 18)
            {
                checkBoxDrawSkybox.Checked = GFX.DrawSkybox;
                trackBarDirectMult.Value = (int)(GFX.DirectLightMult * 100);
                trackBarIndirectMult.Value = (int)(GFX.IndirectLightMult * 100);

                Height = 75;
                Width = 185;

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
