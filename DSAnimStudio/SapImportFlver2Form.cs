using SoulsAssetPipeline.FLVERImporting;
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
    public partial class SapImportFlver2Form : Form
    {
        public SapImportFlver2Form()
        {
            InitializeComponent();
        }

        public FLVER2Importer.FLVER2ImportSettings ImportConfig 
            = new FLVER2Importer.FLVER2ImportSettings();

        public void LoadValuesFromConfig()
        {
            textBoxFBX.Text = ImportConfig.AssetPath;
            flver2_numericUpDownScale.Value = (decimal)(ImportConfig.SceneScale * 100);
            flver2_checkBoxConvertFromZUp.Checked = ImportConfig.ConvertFromZUp;
        }

        public void SaveValuesToConfig()
        {
            ImportConfig.AssetPath = textBoxFBX.Text;
            ImportConfig.ConvertFromZUp = flver2_checkBoxConvertFromZUp.Checked;
            ImportConfig.SceneScale = (float)((double)flver2_numericUpDownScale.Value / 100.0);
        }

        private bool _lockState = false;
        private void SetGuiLock(bool isLocked)
        {
            if (_lockState != isLocked)
            {
                textBoxFBX.Enabled = !isLocked;
                buttonImport.Enabled = !isLocked;
                flver2_checkBoxConvertFromZUp.Enabled = !isLocked;
                flver2_numericUpDownScale.Enabled = !isLocked;

                _lockState = isLocked;
            }
        }

        public void UpdateGuiLockout()
        {
            SetGuiLock(LoadingTaskMan.IsTaskRunning("SoulsAssetPipeline_FLVER2Import"));
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {

            LoadingTaskMan.DoLoadingTask("SoulsAssetPipeline_FLVER2Import", "Importing Model...", prog =>
            {
                SaveValuesToConfig();

                ImportConfig.Game = GameDataManager.GameType;
                ImportConfig.FlverHeader = Scene.MainModel.MainMesh.FlverHeader;

                SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model importedFlver = null;

                using (var importer = new SoulsAssetPipeline.FLVERImporting.FLVER2Importer())
                {
                    importedFlver = importer.ImportFBX(textBoxFBX.Text, ImportConfig);
                }




                foreach (var tex in importedFlver.Textures)
                {
                    TexturePool.AddFetchDDS(tex.Value, tex.Key);
                }

                Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

                for (int i = 0; i < Scene.MainModel.Skeleton.FlverSkeleton.Count; i++)
                {
                    if (!boneIndexRemap.ContainsKey(Scene.MainModel.Skeleton.FlverSkeleton[i].Name))
                        boneIndexRemap.Add(Scene.MainModel.Skeleton.FlverSkeleton[i].Name, i);
                }

                var oldMainMesh = Scene.MainModel.MainMesh;
                var newMainMesh = new NewMesh(importedFlver.Flver, false, boneIndexRemap);

                lock (Scene._lock_ModelLoad_Draw)
                {
                    Scene.MainModel.MainMesh = newMainMesh;
                }

                oldMainMesh?.Dispose();

                Scene.ForceTextureReloadImmediate();

            }, disableProgressBarByDefault: true, isUnimportant: true);

            Main.WinForm.Focus();
        }

        private void buttonImport_DragEnter(object sender, DragEventArgs e)
        {
            bool isValid = false;
            

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (files.Length == 1 && files[0].ToLower().EndsWith(".fbx"))
                    isValid = true;
            }

            e.Effect = isValid ? DragDropEffects.Link : DragDropEffects.None;
        }

        private void SapImportFlver2Form_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (files.Length == 1 && files[0].ToLower().EndsWith(".fbx"))
                {
                    textBoxFBX.Text = files[0];
                    SaveValuesToConfig();
                }
            }
        }
    }
}
