using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.IO;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public partial class TaeEditTaeHeaderForm : Form
    {
        TAE Tae;

        //readonly TAEHeader copyOfOriginalHeader;

        private bool ReadyToExit = false;

        public bool WereThingsChanged = false;

        public TaeEditTaeHeaderForm(TAE tae)
        {
            Tae = tae;

            //copyOfOriginalHeader = new TAEHeader();

            //copyOfOriginalHeader.FileID = tae.Header.FileID;
            //copyOfOriginalHeader.FileID2 = tae.Header.FileID2;
            //copyOfOriginalHeader.FileID3 = tae.Header.FileID3;
            //copyOfOriginalHeader.Version = tae.Header.Version;
            //copyOfOriginalHeader.IsBigEndian = tae.Header.IsBigEndian;
            //copyOfOriginalHeader.UnknownB00 = tae.Header.UnknownB00;
            //copyOfOriginalHeader.UnknownB01 = tae.Header.UnknownB01;
            //copyOfOriginalHeader.UnknownB02 = tae.Header.UnknownB02;
            //copyOfOriginalHeader.UnknownB03 = tae.Header.UnknownB03;
            //copyOfOriginalHeader.UnknownC = tae.Header.UnknownC;
            //copyOfOriginalHeader.UnknownE00 = tae.Header.UnknownE00;
            //copyOfOriginalHeader.UnknownE01 = tae.Header.UnknownE01;
            //copyOfOriginalHeader.UnknownE02 = tae.Header.UnknownE02;
            //copyOfOriginalHeader.UnknownE03 = tae.Header.UnknownE03;
            //copyOfOriginalHeader.UnknownE04 = tae.Header.UnknownE04;
            //copyOfOriginalHeader.UnknownE07 = tae.Header.UnknownE07;
            //copyOfOriginalHeader.UnknownE08 = tae.Header.UnknownE08;
            //copyOfOriginalHeader.UnknownE09 = tae.Header.UnknownE09;

            //copyOfOriginalHeader.UnknownFlags = new byte[tae.Header.UnknownFlags.Length];
            //for (int i = 0; i < tae.Header.UnknownFlags.Length; i++)
            //{
            //    copyOfOriginalHeader.UnknownFlags[i] = tae.Header.UnknownFlags[i];
            //}

            InitializeComponent();

            //Text = $"Editing Header of {Path.GetFileName(Tae.FilePath ?? Tae.VirtualUri)}";
        }

        private void TaeEditAnimPropertiesForm_Shown(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = Tae;
        }

        private void buttonSaveChanges_Click(object sender, EventArgs e)
        {
            ReadyToExit = true;
            WereThingsChanged = true;
            Close();
        }

        private void buttonDiscardChanges_Click(object sender, EventArgs e)
        {
            //Tae.Header.FileID = copyOfOriginalHeader.FileID;
            //Tae.Header.FileID2 = copyOfOriginalHeader.FileID2;
            //Tae.Header.FileID3 = copyOfOriginalHeader.FileID3;
            //Tae.Header.Version = copyOfOriginalHeader.Version;
            //Tae.Header.IsBigEndian = copyOfOriginalHeader.IsBigEndian;
            //Tae.Header.UnknownB00 = copyOfOriginalHeader.UnknownB00;
            //Tae.Header.UnknownB01 = copyOfOriginalHeader.UnknownB01;
            //Tae.Header.UnknownB02 = copyOfOriginalHeader.UnknownB02;
            //Tae.Header.UnknownB03 = copyOfOriginalHeader.UnknownB03;
            //Tae.Header.UnknownC = copyOfOriginalHeader.UnknownC;
            //Tae.Header.UnknownE00 = copyOfOriginalHeader.UnknownE00;
            //Tae.Header.UnknownE01 = copyOfOriginalHeader.UnknownE01;
            //Tae.Header.UnknownE02 = copyOfOriginalHeader.UnknownE02;
            //Tae.Header.UnknownE03 = copyOfOriginalHeader.UnknownE03;
            //Tae.Header.UnknownE04 = copyOfOriginalHeader.UnknownE04;
            //Tae.Header.UnknownE07 = copyOfOriginalHeader.UnknownE07;
            //Tae.Header.UnknownE08 = copyOfOriginalHeader.UnknownE08;
            //Tae.Header.UnknownE09 = copyOfOriginalHeader.UnknownE09;

            //Tae.Header.UnknownFlags = new byte[copyOfOriginalHeader.UnknownFlags.Length];
            //for (int i = 0; i < copyOfOriginalHeader.UnknownFlags.Length; i++)
            //{
            //    Tae.Header.UnknownFlags[i] = copyOfOriginalHeader.UnknownFlags[i];
            //}

            ReadyToExit = true;
            WereThingsChanged = false;
            Close();
        }

        private void TaeEditAnimPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !ReadyToExit;
        }

        private void TaeEditTaeHeaderForm_Load(object sender, EventArgs e)
        {

        }
    }
}
