using DSAnimStudio.TaeEditor;
using SoulsAssetPipeline;
using SoulsAssetPipeline.FLVERImporting;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    public partial class SapImportFlver2Form : Form
    {
        List<Action> DoSaveImport;

        public SapImportFlver2Form()
        {
            InitializeComponent();
        }



        public SapImportConfigs.ImportConfigFlver2 ImportConfig = new SapImportConfigs.ImportConfigFlver2();

        public TaeEditorScreen MainScreen;

        public void LoadValuesFromConfig()
        {
            textBoxFBX.Text = ImportConfig.AssetPath;
            flver2_numericUpDownScale.Value = (decimal)(ImportConfig.SceneScale * 100.0);
            flver2_checkBoxConvertFromZUp.Checked = ImportConfig.ConvertFromZUp;
            flver2_checkBoxKeepExistingDummyPoly.Checked = ImportConfig.KeepOriginalDummyPoly;
        }

        public void SaveValuesToConfig()
        {
            ImportConfig.AssetPath = textBoxFBX.Text;
            ImportConfig.ConvertFromZUp = flver2_checkBoxConvertFromZUp.Checked;
            ImportConfig.SceneScale = (float)((double)flver2_numericUpDownScale.Value / 100.0);
            ImportConfig.KeepOriginalDummyPoly = flver2_checkBoxKeepExistingDummyPoly.Checked;
        }

        private bool _lockState = false;
        private void SetGuiLock(bool isLocked)
        {
            if (_lockState != isLocked)
            {
                textBoxFBX.Enabled = !isLocked;
                buttonImport.Enabled = !isLocked;
                buttonSaveImportedData.Enabled = !isLocked;
                buttonRestoreBackups.Enabled = !isLocked;
                flver2_checkBoxConvertFromZUp.Enabled = !isLocked;
                flver2_numericUpDownScale.Enabled = !isLocked;
                flver2_checkBoxKeepExistingDummyPoly.Enabled = !isLocked;

                _lockState = isLocked;
            }
        }

        public void UpdateGuiLockout()
        {
            SetGuiLock(LoadingTaskMan.AnyTasksRunning());
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            LoadingTaskMan.DoLoadingTask("SoulsAssetPipeline_FLVER2Import", "Importing Model...", prog =>
            {
                SaveValuesToConfig();

                Main.SaveConfig();

                var cfg = new FLVER2Importer.FLVER2ImportSettings();

                cfg.Game = GameRoot.GameType;
                cfg.FlverHeader = Scene.MainModel.MainMesh.Flver2Header;

                cfg.AssetPath = ImportConfig.AssetPath;
                cfg.ConvertFromZUp = ImportConfig.ConvertFromZUp;
                cfg.SceneScale = ImportConfig.SceneScale;

                //nightfall testing
                //var fatcat = FLVER2.Read(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS REMASTERED - Nightfall\chr\c2250bakbeforemodel\c2250-chrbnd-dcx\chr\c2250\c2250.flver");
                //cfg.SkeletonTransformsOverride = fatcat.Bones;

                SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model importedFlver = null;

                using (var importer = new SoulsAssetPipeline.FLVERImporting.FLVER2Importer())
                {
                    importedFlver = importer.ImportFBX(textBoxFBX.Text, cfg);
                }

                // nightfall
                //importedFlver.Flver.Dummies = fatcat.Dummies;

                
                //solarDmy.

                

                List<Action> doImport = new List<Action>();

#if DEBUG
                byte[] flver = importedFlver.Flver.Write();
                File.WriteAllBytes($"{Scene.MainModel.Name}.flver", flver);
#endif

                var importedAnimStudioModel = GameRoot.LoadCharacter(Scene.MainModel.Name, Scene.MainModel.Name, importedFlver, doImport, ImportConfig);

                DoSaveImport = doImport;

                
                var oldMainModel = Scene.MainModel;

                lock (Scene._lock_ModelLoad_Draw)
                {
                    importedAnimStudioModel.NpcParam = oldMainModel.NpcParam;
                    importedAnimStudioModel.DefaultDrawMask = oldMainModel.DefaultDrawMask;
                    importedAnimStudioModel.DrawMask = oldMainModel.DrawMask;
                    importedAnimStudioModel.NpcParam?.ApplyToNpcModel(importedAnimStudioModel);


                    Scene.Models.Remove(oldMainModel);
                    oldMainModel?.Dispose();
                    importedAnimStudioModel.AnimContainer?.ClearAnimation();
                }

                Main.TAE_EDITOR.Graph.ViewportInteractor.InitializeForCurrentModel();
                Main.TAE_EDITOR.ReselectCurrentAnimation();
                Main.TAE_EDITOR.HardReset();

                Invoke(new Action(() =>
                {
                    buttonSaveImportedData.Enabled = true;
                }));

                //foreach (var tex in importedFlver.Textures)
                //{
                //    TexturePool.AddFetchDDS(tex.Value, tex.Key);
                //}

                //Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

                //for (int i = 0; i < Scene.MainModel.Skeleton.FlverSkeleton.Count; i++)
                //{
                //    if (!boneIndexRemap.ContainsKey(Scene.MainModel.Skeleton.FlverSkeleton[i].Name))
                //        boneIndexRemap.Add(Scene.MainModel.Skeleton.FlverSkeleton[i].Name, i);
                //}

                //var oldMainMesh = Scene.MainModel.MainMesh;
                //var newMainMesh = new NewMesh(importedFlver.Flver, false, boneIndexRemap);

                //lock (Scene._lock_ModelLoad_Draw)
                //{
                //    Scene.MainModel.MainMesh = newMainMesh;
                //}

                //oldMainMesh?.Dispose();

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

        private void buttonBrowseFBX_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Autodesk FBX Scene (*.FBX)|*.FBX",
                Title = "Open FBX Model File",
            };

            if (!string.IsNullOrWhiteSpace(ImportConfig.AssetPath))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(ImportConfig.AssetPath);
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxFBX.Text = dlg.FileName;
                SaveValuesToConfig();
            }
        }

        private void buttonSaveImportedData_Click(object sender, EventArgs e)
        {
            if (DoSaveImport == null)
            {
                System.Windows.Forms.MessageBox.Show("Nothing to save.");
            }
            else
            {

                GameRoot.CreateCharacterModelBackup(Scene.MainModel.Name);

                LoadingTaskMan.DoLoadingTask("SapFlver2SaveImportedData", "Saving imported model and textures...", prog =>
                {
                    for (int i = 0; i < DoSaveImport.Count; i++)
                    {
                        DoSaveImport[i]?.Invoke();
                        prog?.Report(1.0 * (i + 1) / DoSaveImport.Count);
                    }

                    System.Windows.Forms.MessageBox.Show("Saved file.");
                }, isUnimportant: true);

                
            }
        }

        private void SapImportFlver2Form_Load(object sender, EventArgs e)
        {
            buttonSaveImportedData.Enabled = false;
        }

        private void buttonRestoreBackups_Click(object sender, EventArgs e)
        {
            GameRoot.RestoreCharacterModelBackup(Scene.MainModel.Name, isAsync: true);
        }
    }
}
