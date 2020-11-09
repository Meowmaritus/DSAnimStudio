using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.FLVERImporting;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class _QuickDebug
    {
        public static void BuildDebugMenu()
        {
            if (Scene.MainModel?.AnimContainer != null)
            {
                float animWeight = Scene.MainModel.AnimContainer.DebugAnimWeight;
                ImGui.SliderFloat("HKX Skel -> HKX Anim Weight", ref animWeight, 0, 1);
                Scene.MainModel.AnimContainer.DebugAnimWeight = animWeight;

                float animWeight2 = Scene.MainModel.DebugAnimWeight_Deprecated;
                ImGui.SliderFloat("FLVER Skel -> HKX Skel Weight", ref animWeight2, 0, 1);
                Scene.MainModel.DebugAnimWeight_Deprecated = animWeight2;

                bool bind = Scene.MainModel.EnableSkinning;
                ImGui.Checkbox("Enable FLVER Skel -> HKX Skel", ref bind);
                Scene.MainModel.EnableSkinning = bind;
            }


            if (DebugButton("REMO HKX TEST"))
            {
                var hkxTestPath = @"E:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\remo\scn180000-remobnd\cut0030\hkxwin32\a0030.hkx";
                HKX.HKAAnimationBinding hk_binding = null;
                HKX.HKASplineCompressedAnimation hk_anim = null;
                HKX.HKASkeleton hk_skeleton = null;

                var hkx = HKX.Read(hkxTestPath, HKX.HKXVariation.HKXDS1, false);

                foreach (var o in hkx.DataSection.Objects)
                {
                    if (o is HKX.HKASkeleton asSkeleton)
                        hk_skeleton = asSkeleton;
                    else if (o is HKX.HKAAnimationBinding asBinding)
                        hk_binding = asBinding;
                    else if (o is HKX.HKASplineCompressedAnimation asAnim)
                        hk_anim = asAnim;
                }

                Scene.MainModel.AnimContainer.ClearAnimations();

                Scene.MainModel.AnimContainer.Skeleton.LoadHKXSkeleton(hk_skeleton);

                var testIdleAnimThing = new NewHavokAnimation_SplineCompressed("a00_0000.hkx",
                    Scene.MainModel.AnimContainer.Skeleton, null, hk_binding, hk_anim, Scene.MainModel.AnimContainer);

                Scene.MainModel.AnimContainer.AddNewAnimation("a00_0000.hkx", testIdleAnimThing);

                Scene.MainModel.AnimContainer.CurrentAnimationName = "a00_0000.hkx";

                Scene.MainModel.IsRemoModel = true;
                Scene.MainModel.Name = "c0000_0000";

                Scene.MainModel.AnimContainer.ForcePlayAnim = true;

                Scene.MainModel.SkeletonFlver.RevertToReferencePose();

                Scene.MainModel.SkeletonFlver.MapToSkeleton(Scene.MainModel.AnimContainer.Skeleton, isRemo: true);
            }
            


            if (DebugButton("DS2 IMPORT TEST"))
            {
                LoadingTaskMan.DoLoadingTask(null, "DS2 IMPORT TEST", prog =>
                {
                    GameDataManager.SoftInit(SoulsAssetPipeline.SoulsGames.DS2SOTFS);

                    var bndPath_ORIG = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\parts\head\hd_2021_m.bnd";
                    var bnd_ORIG = BND4.Read(bndPath_ORIG);

                    

                    var bndPath = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\sap_test\model\parts\head\hd_2021_m.bnd";

                    var bndPath_BaseChr = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls II Scholar of the First Sin\Game\model\chr\c0001.bnd";

                    var bnd_BaseChr = BND4.Read(bndPath_BaseChr);

                    var flver_BaseChr = FLVER2.Read(bnd_BaseChr.Files.First(x => x.Name.ToLower().EndsWith(".flv")).Bytes);


                    var flver_orig = FLVER2.Read(bnd_ORIG.Files.First(x => x.Name.ToLower().EndsWith(".flv")).Bytes);

                    var acb_orig = ACB.Read(bnd_ORIG.Files.First(x => x.Name.ToLower().EndsWith(".acb")).Bytes);

                    

                    var fbx = @"E:\DarkSoulsModding\_FBX\DS2HelmDangel.FBX";
                    var settings = new FLVER2Importer.FLVER2ImportSettings()
                    {
                        AssetPath = fbx,
                        ConvertFromZUp = true,
                        SceneScale = 100,
                        FlverHeader = flver_orig.Header,
                        Game = SoulsAssetPipeline.SoulsGames.DS2,
                        RootNodeName = "Root",
                        SkeletonTransformsOverride = flver_orig.Bones,
                        //SceneCorrectMatrix = System.Numerics.Matrix4x4.CreateRotationY(SoulsAssetPipeline.SapMath.Pi),
                        //BoneNameRemapper = new Dictionary<string, string>
                        //{
                        //    { "Pelvis001", "Pelvis" },
                        //}
                    };

                    var importer = new FLVER2Importer();
                    var imp = importer.ImportFBX(fbx, settings);

                    var rootBone = imp.Flver.Bones.FindIndex(bn => bn.Name == "SMDImporter.Mesh.c1500");
                    if (rootBone >= 0)
                    {
                        //imp.Flver.Bones[rootBone].Rotation = new System.Numerics.Vector3(imp.Flver.Bones[rootBone].Rotation.X,
                        //    imp.Flver.Bones[rootBone].Rotation.Y + SoulsAssetPipeline.SapMath.Pi, imp.Flver.Bones[rootBone].Rotation.Z);
                    }

                    void AddMemeBone(string memeBoneName)
                    {
                        int flverLastRootBoneIndex = imp.Flver.Bones.FindLastIndex(b => b.ParentIndex == -1);
                        // Register this new bone as a sibling.
                        if (flverLastRootBoneIndex >= 0)
                            imp.Flver.Bones[flverLastRootBoneIndex].NextSiblingIndex = (short)imp.Flver.Bones.Count;
                        //imp.Flver.Bones.Add(new FLVER.Bone()
                        //{
                        //    Name = memeBoneName,
                        //    Translation = System.Numerics.Vector3.Zero,
                        //    Rotation = System.Numerics.Vector3.Zero,
                        //    Scale = System.Numerics.Vector3.One,
                        //    BoundingBoxMin = new System.Numerics.Vector3(float.MaxValue),
                        //    BoundingBoxMax = new System.Numerics.Vector3(float.MinValue),
                        //    // Cross-register sibling from above.
                        //    PreviousSiblingIndex = (short)flverLastRootBoneIndex,
                        //    NextSiblingIndex = -1,
                        //    ParentIndex = -1,
                        //    ChildIndex = -1,
                        //    Unk3C = 1,
                        //});
                    }

                    //AddMemeBone($"{Path.GetFileNameWithoutExtension(bndPath).ToUpper()}");
                    //AddMemeBone($"{Path.GetFileNameWithoutExtension(bndPath).ToUpper()}_A");

                    //foreach (var m in imp.Flver.Materials)
                    //{
                    //    m.Name = m.Name.Replace("#", "");
                    //}



                    Scene.ClearScene();
                    lock (Scene._lock_ModelLoad_Draw)
                    {
                        var mdl = new Model(imp.Flver, false);
                        mdl.AnimContainer.ForcePlayAnim = true;
                        Scene.Models.Add(mdl);
                        var tpf = new TPF();
                        tpf.Textures.AddRange(imp.Textures);
                        TexturePool.AddTpf(tpf);
                        Scene.ForceTextureReloadImmediate();
                    }

                    

                    

                    bnd_ORIG.Files.Clear();

                    foreach (var t in imp.Textures)
                    {
                        var tpf = new TPF();
                        tpf.Textures.Add(t);
                        bnd_ORIG.Files.Add(new BinderFile(Binder.FileFlags.Flag1, bnd_ORIG.Files.Count, t.Name, tpf.Write()));
                    }

                    ////TESTING
                    //foreach (var m in imp.Flver.Meshes)
                    //{
                    //    foreach (var fs in m.FaceSets)
                    //    {
                    //        fs.Indices = fs.Indices.Select(x => x >= 65535 ? 0xFFFF : x).Take(65535).ToList();
                    //    }
                    //    m.Vertices = m.Vertices.Take(65535).ToList();
                    //}

                    bnd_ORIG.Files.Add(new BinderFile(Binder.FileFlags.Flag1, bnd_ORIG.Files.Count, $"{Path.GetFileNameWithoutExtension(bndPath)}.flv", imp.Flver.Write()));
                    //bnd_ORIG.Files.Add(new BinderFile(Binder.FileFlags.Flag1, bnd_ORIG.Files.Count, $"{Path.GetFileNameWithoutExtension(bndPath)}.acb", acb_orig.Write()));

                    if (File.Exists(bndPath) && !File.Exists(bndPath + ".bak"))
                    {
                        File.Copy(bndPath, bndPath + ".bak");
                    }

                    bnd_ORIG.Write(bndPath);



                    var hkxTestPath = @"E:\DarkSoulsModding\_DS2ANIM\DS2_PLAYER_TEST_ANIM.hkx";
                    HKX.HKAAnimationBinding hk_binding = null;
                    HKX.HKAInterleavedUncompressedAnimation hk_anim = null;
                    HKX.HKASkeleton hk_skeleton = null;

                    var hkx = HKX.Read(hkxTestPath, HKX.HKXVariation.HKXDS3, false);

                    foreach ( var o in hkx.DataSection.Objects)
                    {
                        if (o is HKX.HKASkeleton asSkeleton)
                            hk_skeleton = asSkeleton;
                        else if (o is HKX.HKAAnimationBinding asBinding)
                            hk_binding = asBinding;
                        else if (o is HKX.HKAInterleavedUncompressedAnimation asAnim)
                            hk_anim = asAnim;
                    }

                    Scene.MainModel.AnimContainer.Skeleton.LoadHKXSkeleton(hk_skeleton);

                    var testIdleAnimThing = new NewHavokAnimation_InterleavedUncompressed("a00_00_0000.hkx",
                        Scene.MainModel.AnimContainer.Skeleton, null, hk_binding, hk_anim, Scene.MainModel.AnimContainer);

                    Scene.MainModel.AnimContainer.AddNewAnimation("a00_00_0000.hkx", testIdleAnimThing);

                    Scene.MainModel.AnimContainer.CurrentAnimationName = "a00_00_0000.hkx";

                    Scene.MainModel.AnimContainer.ForcePlayAnim = true;
                }, disableProgressBarByDefault: true, isUnimportant: true);

                
            }

            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();

            if (DebugButton("SoulsAssetPipeline_Anim_Test"))
            {
                var boneNames = Scene.MainModel.AnimContainer.Skeleton.HkxSkeleton.Select(x => x.Name).ToList();
                var importSettings = new SoulsAssetPipeline.AnimationImporting.AnimationImporter.AnimationImportSettings
                {
                    SceneScale = 100f,
                    ExistingBoneNameList = boneNames,
                    ExistingHavokAnimationTemplate = Scene.MainModel.AnimContainer.CurrentAnimation.data,
                    ResampleToFramerate = 60,
                    RootMotionNodeName = "root",
                    FlipQuaternionHandedness = true,
                };

                var importedAnim = SoulsAssetPipeline.AnimationImporting.AnimationImporter.ImportFBX(
                    @"C:\DarkSoulsModding\_FBX_ANIM\muffin2.fbx", importSettings);

                //importedFlver.WriteToHavok2010InterleavedUncompressedXML(@"C:\DarkSoulsModding\CUSTOM ANIM\c2570\ShieldBashVanilla.fbx.saptest.xml");

                lock (Scene._lock_ModelLoad_Draw)
                {
                    var anim = new NewHavokAnimation(importedAnim, Scene.MainModel.AnimContainer.Skeleton, Scene.MainModel.AnimContainer);
                    string animName = Scene.MainModel.AnimContainer.CurrentAnimationName;
                    Scene.MainModel.AnimContainer.AddNewAnimation(animName, anim);
                    Scene.MainModel.AnimContainer.CurrentAnimationName = null;
                    Scene.MainModel.AnimContainer.CurrentAnimationName = animName;
                    Scene.MainModel.AnimContainer.ResetAll();
                    Main.TAE_EDITOR?.HardReset();
                }
                //Console.WriteLine("fatcat");
            }
        }

        private static bool DebugButton(string name)
        {
            ImGui.Button(name);
            return ImGui.IsItemClicked();
        }
    }
}
