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

                fbxAnim_BonesToFlipBackwards.Text = string.Join("\n", ImportConfig.BonesToFlipBackwardsForHotfix);

                fbxAnim_SampleToFramerate.Value = (decimal)ImportConfig.SampleToFramerate;

                fbxAnim_InitializeTracksToTPose.Checked = ImportConfig.InitializeTransformTracksToTPose;
                fbxAnim_EnableRootMotionRotation.Checked = ImportConfig.EnableRotationalRootMotion;

                fbxAnim_ExcludeRootMotionNodeFromTransformTracks.Checked = ImportConfig.ExcludeRootMotionNodeFromTransformTracks;
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
            }));
        }

        public SapImportFbxAnimForm()
        {
            InitializeComponent();

        }

        private void SapImportFbxAnimForm_Load(object sender, EventArgs e)
        {

        }

        private SoulsAssetPipeline.AnimationImporting.ImportedAnimation ImportAnim()
        {
            SaveValuesToConfig();

            MainScreen.SaveConfig();

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
            };

            var importedAnim = SoulsAssetPipeline.AnimationImporting.AnimationImporter.ImportFBX(
                ImportConfig.AssetPath, importSettings);

            return importedAnim;
        }

        private byte[] ImportAnimToHKX()
        {
            var importedAnim = ImportAnim();

            byte[] finalHkxDataToImport = null;

            var compressed2010Hkx = importedAnim.WriteToSplineCompressedHKX2010Bytes(ImportConfig.RotationQuantizationType, ImportConfig.RotationTolerance);

            if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                finalHkxDataToImport = HavokDowngrade.UpgradeHkx2010to2015(compressed2010Hkx);
            }
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1)
            {
                finalHkxDataToImport = compressed2010Hkx;
            }
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3)
            {



                HKX.HKAAnimationBinding hk_binding = null;
                HKX.HKASplineCompressedAnimation hk_anim = null;
                HKX.HKASkeleton hk_skeleton = null;
                HKX.HKADefaultAnimatedReferenceFrame hk_refFrame = null;

                var hkx = HKX.Read(compressed2010Hkx);

                foreach (var o in hkx.DataSection.Objects)
                {
                    if (o is HKX.HKASkeleton asSkeleton)
                        hk_skeleton = asSkeleton;
                    else if (o is HKX.HKAAnimationBinding asBinding)
                        hk_binding = asBinding;
                    else if (o is HKX.HKASplineCompressedAnimation asAnim)
                        hk_anim = asAnim;
                    else if (o is HKX.HKADefaultAnimatedReferenceFrame asRefFrame)
                        hk_refFrame = asRefFrame;
                }

                var root = new HKX2.hkRootLevelContainer();

                var animBinding = new HKX2.hkaAnimationBinding();

                var anim = new HKX2.hkaSplineCompressedAnimation();

                animBinding.m_animation = anim;
                animBinding.m_originalSkeletonName = hk_binding.OriginalSkeletonName;
                animBinding.m_transformTrackToBoneIndices = hk_binding.TransformTrackToBoneIndices.GetArrayData().Elements.Select(x => x.data).ToList();
                animBinding.m_floatTrackToFloatSlotIndices = new List<short>();
                animBinding.m_partitionIndices = new List<short>();
                animBinding.m_blendHint = (HKX2.BlendHint)(int)hk_binding.BlendHint;

                anim.m_blockDuration = hk_anim.BlockDuration;
                anim.m_blockInverseDuration = hk_anim.InverseBlockDuration;
                anim.m_data = hk_anim.Data.GetArrayData().Elements.Select(x => x.data).ToList();
                anim.m_duration = hk_anim.Duration;
                anim.m_endian = hk_anim.Endian;

                if (hk_refFrame != null)
                {
                    var rootMotion = new HKX2.hkaDefaultAnimatedReferenceFrame();

                    rootMotion.m_duration = hk_refFrame.Duration;
                    rootMotion.m_forward = hk_refFrame.Forward;
                    rootMotion.m_referenceFrameSamples = new List<System.Numerics.Vector4>();
                    foreach (var rf in hk_refFrame.ReferenceFrameSamples.GetArrayData().Elements)
                    {
                        rootMotion.m_referenceFrameSamples.Add(rf.Vector);
                    }
                    rootMotion.m_up = hk_refFrame.Up;

                    anim.m_extractedMotion = rootMotion;
                }

                anim.m_frameDuration = hk_anim.FrameDuration;
                anim.m_maskAndQuantizationSize = (int)hk_anim.MaskAndQuantization;
                anim.m_maxFramesPerBlock = hk_anim.FramesPerBlock;
                anim.m_numberOfFloatTracks = hk_anim.FloatTrackCount;
                anim.m_numberOfTransformTracks = hk_anim.TransformTrackCount;
                anim.m_numBlocks = hk_anim.BlockCount;
                anim.m_numFrames = hk_anim.FrameCount;
                anim.m_floatBlockOffsets = hk_anim.FloatBlockOffsets.GetArrayData().Elements.Select(b => b.data).ToList();
                anim.m_type = HKX2.AnimationType.HK_SPLINE_COMPRESSED_ANIMATION;
                anim.m_blockOffsets = hk_anim.BlockOffsets.GetArrayData().Elements.Select(b => b.data).ToList();
                anim.m_floatOffsets = new List<uint>();
                anim.m_transformOffsets = new List<uint>();

                //TODO: IMPLEMENT ANNOTATION TRACK READ IN LEGACY HKX TO TRANSFER IT TO HKX2

                //anim.m_annotationTracks = new List<HKX2.hkaAnnotationTrack>();
                //for (int i = 0; i < hk_anim.TransformTrackCount; i++)
                //{
                //    var boneIndex = animBinding.m_transformTrackToBoneIndices[i];
                //    string boneName = boneNames[boneIndex];

                //    anim.m_annotationTracks.Add(new HKX2.hkaAnnotationTrack()
                //    {
                //        m_trackName = boneName,
                //        m_annotations = new List<HKX2.hkaAnnotationTrackAnnotation>(),
                //    });
                //}





                var animContainer = new HKX2.hkaAnimationContainer();

                animContainer.m_attachments = new List<HKX2.hkaBoneAttachment>();
                animContainer.m_skins = new List<HKX2.hkaMeshBinding>();
                animContainer.m_skeletons = new List<HKX2.hkaSkeleton>();

                animContainer.m_animations = new List<HKX2.hkaAnimation>();
                animContainer.m_animations.Add(anim);

                animContainer.m_bindings = new List<HKX2.hkaAnimationBinding>();
                animContainer.m_bindings.Add(animBinding);

                root.m_namedVariants = new List<HKX2.hkRootLevelContainerNamedVariant>();
                root.m_namedVariants.Add(new HKX2.hkRootLevelContainerNamedVariant()
                {
                    m_className = "hkaAnimationContainer",
                    m_name = "Merged Animation Container",
                    m_variant = animContainer
                });

                using (MemoryStream s2 = new MemoryStream())
                {
                    BinaryWriterEx bw = new BinaryWriterEx(false, s2);
                    var s = new HKX2.PackFileSerializer();
                    s.Serialize(root, bw);

                    finalHkxDataToImport = s2.ToArray();
                }
            }

            return finalHkxDataToImport;
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            if (!(GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3))
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
                    MainScreen.Graph.ViewportInteractor.CurrentModel.AnimContainer.AddNewHKXToLoad(Scene.MainModel.AnimContainer.CurrentAnimationName, dataForAnimContainer);

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
            if (!(GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3))
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

                    if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1)
                    {
                        dlg.Filter = "HKX 2010 x32 - For DS1: PTDE (*.HKX)|*.HKX";
                        dlg.Title = "Save Dark Souls: PTDE Format Animation File";
                    }
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                    {
                        dlg.Filter = "HKX 2015 x64 - For DS1R (*.HKX)|*.HKX";
                        dlg.Title = "Save Dark Souls Remastered Format Animation File";
                    }
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                    {
                        dlg.Filter = "HKX 2014 x64 - For DS3 (*.HKX)|*.HKX";
                        dlg.Title = "Save Dark Souls III Format Animation File";
                    }
                    else
                    {
                        throw new NotImplementedException($"Current GameType not supported by the save loose HKX code: '{GameDataManager.GameType}'");
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
                    Scene.MainModel.AnimContainer.AddNewAnimation(Scene.MainModel.AnimContainer.CurrentAnimationName, new NewHavokAnimation(anim, Scene.MainModel.AnimContainer.Skeleton, Scene.MainModel.AnimContainer));
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
    }
}
