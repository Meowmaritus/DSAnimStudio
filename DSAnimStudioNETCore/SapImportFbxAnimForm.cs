using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using SoulsAssetPipeline.Animation;
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
    public partial class SapImportFbxAnimForm : Form
    {
        public SapImportConfigs.ImportConfigAnimFBX ImportConfig = new SapImportConfigs.ImportConfigAnimFBX();

        public TaeEditorScreen MainScreen;

        public void LoadValuesFromConfig()
        {
            Invoke(new Action(() =>
            {
                textBoxFBX.Text = ImportConfig.AssetPath;
                flver2_numericUpDownScale.Value = (decimal)(ImportConfig.SceneScale * 100.0);
                flver2_checkBoxConvertFromZUp.Checked = ImportConfig.ConvertFromZUp;

                fbxAnim_RootMotionNodeName.Text = ImportConfig.RootMotionNodeName;

                fbxAnim_checkboxMirrorQuaternions.Checked = ImportConfig.TurnQuaternionsInsideOut;

                fbxAnim_RotationTolerance.Value = (decimal)ImportConfig.RotationTolerance;
                fbxAnim_RotationQuantizationType.SelectedIndex = (int)ImportConfig.RotationQuantizationType;

                fbxAnim_NegateQuaternionX.Checked = ImportConfig.NegateQuaternionX;
                fbxAnim_NegateQuaternionY.Checked = !ImportConfig.NegateQuaternionY;
                fbxAnim_NegateQuaternionZ.Checked = !ImportConfig.NegateQuaternionZ;
                fbxAnim_NegateQuaternionW.Checked = ImportConfig.NegateQuaternionW;

                fbxAnim_BonesToFlipBackwards.Text = string.Join("\r\n", ImportConfig.BonesToFlipBackwardsForHotfix);

                fbxAnim_SampleToFramerate.Value = (decimal)ImportConfig.SampleToFramerate;

                fbxAnim_InitializeTracksToTPose.Checked = ImportConfig.InitializeTransformTracksToTPose;
                fbxAnim_EnableRootMotionRotation.Checked = ImportConfig.EnableRotationalRootMotion;

                fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Checked = ImportConfig.ExcludeRootMotionNodeFromTransformTracks;

                fbxAnim_OverrideRootMotionScale.Checked = ImportConfig.UseRootMotionScaleOverride;
                fbxAnim_OverrideRootMotionScale_Amount.Value = (decimal)(ImportConfig.RootMotionScaleOverride * 100.0);
            }));
        }

        public void SaveValuesToConfig()
        {
            Invoke(new Action(() =>
            {
                ImportConfig.AssetPath = textBoxFBX.Text;
                ImportConfig.ConvertFromZUp = flver2_checkBoxConvertFromZUp.Checked;
                ImportConfig.SceneScale = (float)((double)flver2_numericUpDownScale.Value / 100.0);

                ImportConfig.RootMotionNodeName = fbxAnim_RootMotionNodeName.Text;

                ImportConfig.TurnQuaternionsInsideOut = fbxAnim_checkboxMirrorQuaternions.Checked;

                ImportConfig.RotationTolerance = (float)((double)fbxAnim_RotationTolerance.Value);
                if (fbxAnim_RotationQuantizationType.SelectedIndex < 0)
                    fbxAnim_RotationQuantizationType.SelectedIndex = 0;
                ImportConfig.RotationQuantizationType = (SplineCompressedAnimation.RotationQuantizationType)fbxAnim_RotationQuantizationType.SelectedIndex;

                ImportConfig.NegateQuaternionX = fbxAnim_NegateQuaternionX.Checked;
                ImportConfig.NegateQuaternionY = !fbxAnim_NegateQuaternionY.Checked;
                ImportConfig.NegateQuaternionZ = !fbxAnim_NegateQuaternionZ.Checked;
                ImportConfig.NegateQuaternionW = fbxAnim_NegateQuaternionW.Checked;

                ImportConfig.BonesToFlipBackwardsForHotfix = fbxAnim_BonesToFlipBackwards.Text.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                ImportConfig.SampleToFramerate = (float)fbxAnim_SampleToFramerate.Value;

                ImportConfig.InitializeTransformTracksToTPose = fbxAnim_InitializeTracksToTPose.Checked;
                ImportConfig.EnableRotationalRootMotion = fbxAnim_EnableRootMotionRotation.Checked;

                ImportConfig.ExcludeRootMotionNodeFromTransformTracks = fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Checked;

                ImportConfig.UseRootMotionScaleOverride = fbxAnim_OverrideRootMotionScale.Checked;
                ImportConfig.RootMotionScaleOverride = (float)((double)fbxAnim_OverrideRootMotionScale_Amount.Value / 100.0);
            }));
        }

        public SapImportFbxAnimForm()
        {
            InitializeComponent();

        }

        private void SapImportFbxAnimForm_Load(object sender, EventArgs e)
        {
            richTextBoxHint.Select(0, richTextBoxHint.Text.IndexOf(":"));
            richTextBoxHint.SelectionFont = new Font(richTextBoxHint.SelectionFont, FontStyle.Bold);
            richTextBoxHint.DeselectAll();
        }

        private SoulsAssetPipeline.AnimationImporting.ImportedAnimation ImportAnim()
        {
            SaveValuesToConfig();

            Main.SaveConfig();

            var boneDefaultTransforms = Scene.MainModel.AnimContainer.Skeleton.HkxSkeleton.ToDictionary(x => x.Name, x => x.RelativeReferenceTransform);
            var boneNames = boneDefaultTransforms.Keys.ToList();
            var importSettings = new SoulsAssetPipeline.AnimationImporting.AnimationImporter.AnimationImportSettings
            {
                SceneScale = ImportConfig.SceneScale,
                ExistingBoneDefaults = boneDefaultTransforms,
                ExistingHavokAnimationTemplate = Scene.MainModel.AnimContainer.CurrentAnimation.data,
                ResampleToFramerate = ImportConfig.SampleToFramerate,
                RootMotionNodeName = ImportConfig.RootMotionNodeName,
                FlipQuaternionHandedness = ImportConfig.TurnQuaternionsInsideOut,
                ConvertFromZUp = ImportConfig.ConvertFromZUp,
                BonesToFlipBackwardsAboutYAxis = ImportConfig.BonesToFlipBackwardsForHotfix,
                NegateQuaternionX = ImportConfig.NegateQuaternionX,
                NegateQuaternionY = ImportConfig.NegateQuaternionY,
                NegateQuaternionZ = ImportConfig.NegateQuaternionZ,
                NegateQuaternionW = ImportConfig.NegateQuaternionW,

                InitalizeUnanimatedTracksToTPose = ImportConfig.InitializeTransformTracksToTPose,
                EnableRotationalRootMotion = ImportConfig.EnableRotationalRootMotion,

                ExcludeRootMotionNodeFromTransformTracks = ImportConfig.ExcludeRootMotionNodeFromTransformTracks,

                RootMotionScaleOverride = ImportConfig.RootMotionScaleOverride,
                UseRootMotionScaleOverride = ImportConfig.UseRootMotionScaleOverride,
            };

            var importedAnim = SoulsAssetPipeline.AnimationImporting.AnimationImporter.ImportFBX(
                ImportConfig.AssetPath, importSettings);

            return importedAnim;
        }

        private byte[] ImportAnimToHKX()
        {
            var importedAnim = ImportAnim();

            var readyForGame = ImportedAnimationConverter.GetAnimReadyToPutIntoGameFromImported(importedAnim);

            return readyForGame.DataForGame;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (!(GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3))
            {
                ImguiOSD.DialogManager.DialogOK("Not Supported", "Only supported on DS1 PTDE, DS1R, and DS3 for now.");
                return;
            }

            LoadingTaskMan.DoLoadingTask("SoulsAssetPipeline_AnimFBXImport", "Importing Animation To Current...", prog =>
            {
                Invoke(new Action(() =>
                {
                    buttonImport.Enabled = false;
                    buttonImportToLooseHKX.Enabled = false;
                }));

                try
                {
                    var finalHkxDataToImport = ImportAnimToHKX();

                    MainScreen.FileContainer.AddNewHKX(Utils.GetFileNameWithoutAnyExtensions(Scene.MainModel.AnimContainer.CurrentAnimationName), finalHkxDataToImport, out byte[] dataForAnimContainer);

                    if (dataForAnimContainer != null)
                    {
                        MainScreen.Graph.ViewportInteractor.CurrentModel.AnimContainer.AddNewHKXToLoad(Scene.MainModel.AnimContainer.CurrentAnimationName, dataForAnimContainer);

                        MainScreen.ReselectCurrentAnimation();
                        MainScreen.HardReset();

                        Invoke(new Action(() =>
                        {
                            Main.WinForm.Focus();
                        }));
                    }
                    else
                    {
                        DialogManager.DialogOK("Failed", "Failed to save (TagTools refused to work), just try again.");
                    }

                    

                   
                }
                catch (Exception ex)
                {
                    ImguiOSD.DialogManager.DialogOK("Import Failed", $"Failed to import animation. Error below:\n\n{ex}");
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        buttonImport.Enabled = true;
                        buttonImportToLooseHKX.Enabled = true;
                    }));
                }
            }, disableProgressBarByDefault: true, isUnimportant: true);

        }

        private void buttonBrowseFBX_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Autodesk FBX Scene (*.FBX)|*.FBX",
                Title = "Open FBX Animation File",
            };

            if (!string.IsNullOrWhiteSpace(ImportConfig.AssetPath))
            {
                string potentialDefaultDir = Path.GetDirectoryName(ImportConfig.AssetPath);
                if (Directory.Exists(potentialDefaultDir))
                    dlg.InitialDirectory = potentialDefaultDir;
            }

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxFBX.Text = dlg.FileName;
                SaveValuesToConfig();
            }
        }

        private void buttonImportToLooseHKX_Click(object sender, EventArgs e)
        {
            if (!(GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3))
            {
                ImguiOSD.DialogManager.DialogOK("Not Supported", "Only supported on DS1 PTDE, DS1R, and DS3 for now.");
                return;
            }

            LoadingTaskMan.DoLoadingTask("SoulsAssetPipeline_AnimFBXImport", "Importing Animation To Current...", prog =>
            {
                Invoke(new Action(() =>
                {
                    buttonImport.Enabled = false;
                    buttonImportToLooseHKX.Enabled = false;
                }));

                try
                {
                    var finalHkxDataToImport = ImportAnimToHKX();

                    var dlg = new SaveFileDialog()
                    {
                        Filter = "Havok Animation File (*.HKX)|*.HKX",
                        Title = "Save Havok Animation File",
                        
                    };

                    if (!string.IsNullOrWhiteSpace(ImportConfig.AssetPath))
                    {
                        string potentialDefaultDir = Path.GetDirectoryName(ImportConfig.AssetPath);
                        if (Directory.Exists(potentialDefaultDir))
                            dlg.InitialDirectory = potentialDefaultDir;
                    }

                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1)
                    {
                        dlg.Filter = "HKX 2010 x32 - For DS1: PTDE (*.HKX)|*.HKX";
                        dlg.Title = "Save Dark Souls: PTDE Format Animation File";
                    }
                    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                    {
                        dlg.Filter = "HKX 2015 x64 - For DS1R (*.HKX)|*.HKX";
                        dlg.Title = "Save Dark Souls Remastered Format Animation File";
                    }
                    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                    {
                        dlg.Filter = "HKX 2014 x64 - For DS3 (*.HKX)|*.HKX";
                        dlg.Title = "Save Dark Souls III Format Animation File";
                    }
                    else
                    {
                        throw new NotImplementedException($"Current GameType not supported by the save loose HKX code: '{GameRoot.GameType}'");
                    }


                    if (!string.IsNullOrWhiteSpace(ImportConfig.AssetPath))
                    {
                        dlg.InitialDirectory = Path.GetDirectoryName(ImportConfig.AssetPath);
                    }

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(dlg.FileName, finalHkxDataToImport);
                    }
                }
                catch (Exception ex)
                {
                    ImguiOSD.DialogManager.DialogOK("Import Failed", $"Failed to import animation. Error below:\n\n{ex}");
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        buttonImport.Enabled = true;
                        buttonImportToLooseHKX.Enabled = true;
                    }));
                }
            }, disableProgressBarByDefault: true, isUnimportant: true);
        }

        private void buttonImportDirectlyTest_Click(object sender, EventArgs e)
        {
            LoadingTaskMan.DoLoadingTask("SoulsAssetPipeline_AnimFBXImport", "Importing Animation To Current...", prog =>
            {
                Invoke(new Action(() =>
                {
                    buttonImport.Enabled = false;
                    buttonImportToLooseHKX.Enabled = false;
                }));

                try
                {
                    var anim = ImportAnim();
                    Scene.MainModel.AnimContainer.AddNewAnimation(Scene.MainModel.AnimContainer.CurrentAnimationName, new NewHavokAnimation(anim, fileSize: -1));
                    MainScreen.ReselectCurrentAnimation();
                    MainScreen.HardReset();

                    Invoke(new Action(() =>
                    {
                        Main.WinForm.Focus();
                    }));
                }
                catch (Exception ex)
                {
                    ImguiOSD.DialogManager.DialogOK("Import Failed", $"Failed to import animation. Error below:\n\n{ex}");
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        buttonImport.Enabled = true;
                        buttonImportToLooseHKX.Enabled = true;
                    }));
                }
            }, disableProgressBarByDefault: true, isUnimportant: true);

            
        }

        private void fbxAnim_OverrideRootMotionScale_CheckedChanged(object sender, EventArgs e)
        {
            fbxAnim_OverrideRootMotionScale_Amount.Enabled = fbxAnim_OverrideRootMotionScale.Checked;
        }
    }
}
