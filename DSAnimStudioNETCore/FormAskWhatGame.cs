using SoulsAssetPipeline;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public partial class FormAskWhatGame : Form
    {
        public SoulsGames SelectedGame = SoulsGames.ER;

        public FormAskWhatGame()
        {
            InitializeComponent();
        }

        private void FormAskWhatGame_Load(object sender, EventArgs e)
        {

        }

        private void radioButtonDES_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDES.Checked)
            {
                //radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.DES;
            }
        }

        private void radioButtonDS1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDS1.Checked)
            {
                radioButtonDES.Checked = false;
                //radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.DS1;
            }
        }

        private void radioButtonBB_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBB.Checked)
            {
                radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                //radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.BB;
            }
        }

        private void radioButtonDS3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDS3.Checked)
            {
                radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                //radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.DS3;
            }
        }

        private void radioButtonSDT_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSDT.Checked)
            {
                radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                //radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.SDT;
            }
        }

        private void radioButtonDS1R_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDS1R.Checked)
            {
                radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                //radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.DS1R;
            }
        }

        private void radioButtonER_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonER.Checked)
            {
                radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                //radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.ER;
            }
        }

        private void radioButtonAC6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonAC6.Checked)
            {
                radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                //radioButtonAC6.Checked = false;
                radioButtonERNR.Checked = false;

                SelectedGame = SoulsGames.AC6;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {

        }

        private void radioButtonERNR_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonERNR.Checked)
            {
                radioButtonDES.Checked = false;
                radioButtonDS1.Checked = false;
                radioButtonBB.Checked = false;
                radioButtonDS3.Checked = false;
                radioButtonSDT.Checked = false;
                radioButtonDS1R.Checked = false;
                radioButtonER.Checked = false;
                radioButtonAC6.Checked = false;

                SelectedGame = SoulsGames.ERNR;
            }
        }
    }
}
