using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.Animation.SIBCAM;
using SoulsAssetPipeline.FLVERImporting;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMatrix = System.Numerics.Matrix4x4;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;

namespace DSAnimStudio
{
    public static class _QuickDebug
    {
        public static void BuildDebugMenu()
        {
            

            if (DebugTestButton("Misc2"))
            {
                var test = new Dictionary<string, FLVER2>();
                var mapFlverNames = Directory.GetFiles(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS REMASTERED - Nightfall\map", "*.flver.dcx", SearchOption.AllDirectories);
                foreach (var ff in mapFlverNames)
                {
                    var flver = FLVER2.Read(ff);
                    foreach (var mat in flver.Materials)
                    {
                        if (mat.MTD.ToLower().EndsWith("m[d][l].mtd"))
                        {
                            test.Add(Path.GetFileName(ff), flver);
                            break;
                        }
                    }
                }

                Console.WriteLine("fatcsat");
            }

            if (DebugTestButton("Misc"))
            {
                var test = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS REMASTERED - Nightfall\map\m10_02_00_00\m4100B2A10_Crash.flver";
                var flver = FLVER2.Read(test);

                //foreach (var m in flver.Meshes)
                //{
                //    foreach (var v in m.Vertices)
                //    {
                //        if (v.Tangents.Count == 0)
                //            v.Tangents.Add(new NVector4(NVector3.TransformNormal(v.Normal, NMatrix.CreateRotationZ(MathHelper.PiOver2)), 1));

                //        v.NormalW = 0;
                //    }

                //    foreach (var vb in m.VertexBuffers)
                //    {
                //        vb.LayoutIndex = 0;
                //    }

                //    var fs = m.FaceSets[0];
                //    var new1 = new FLVER2.FaceSet(FLVER2.FaceSet.FSFlags.LodLevel1, fs.TriangleStrip, fs.CullBackfaces, fs.Unk06, fs.Indices.ToList());
                //    var new2 = new FLVER2.FaceSet(FLVER2.FaceSet.FSFlags.LodLevel2, fs.TriangleStrip, fs.CullBackfaces, fs.Unk06, fs.Indices.ToList());

                //    m.FaceSets.Add(new1);
                //    m.FaceSets.Add(new2);
                //}

                //flver.BufferLayouts = flver.BufferLayouts.Take(1).ToList();

                //foreach (var layout in flver.BufferLayouts)
                //{
                //    if (!layout.Any(mb => mb.Semantic == FLVER.LayoutSemantic.Tangent))
                //    {
                //        var normalIndex = layout.FindIndex(mb => mb.Semantic == FLVER.LayoutSemantic.Normal);
                //        if (normalIndex >= 0)
                //        {
                //            var n = layout[normalIndex];
                //            var newMember = new FLVER.LayoutMember(n.Type, FLVER.LayoutSemantic.Tangent, 0, n.Unk00);
                //            layout.Insert(normalIndex + 1, newMember);
                //        }
                //    }
                //}

                //int xx = 0;

                //foreach (var m in flver.Materials)
                //{
                //    m.GXIndex = -1;
                //    var g_Lightmap_Index = m.Textures.FindIndex(tx => tx.Type == "g_Lightmap");
                //    if (g_Lightmap_Index >= 0)
                //    {
                //        var lm = m.Textures[g_Lightmap_Index];
                //        var g_Specular = new FLVER2.Texture("g_Specular", lm.Path, lm.Scale, lm.Unk10, lm.Unk11, 0, 0, lm.Unk1C);
                //        var g_Bumpmap = new FLVER2.Texture("g_Bumpmap", lm.Path, lm.Scale, lm.Unk10, lm.Unk11, 0, 0, lm.Unk1C);
                //        m.Textures.Insert(g_Lightmap_Index, g_Bumpmap);
                //        m.Textures.Insert(g_Lightmap_Index, g_Specular);
                        
                //    }

                //    foreach (var tx in m.Textures)
                //    {
                //        //tx.Path = tx.Path.Replace("+", "").Replace("~", "");
                //        if (tx.Type != "g_DetailBumpmap")
                //            tx.Path = @"N:\FRPG\data\Model\map\m10\tex\PleaseDontCrash.tga";
                //    }

                //    m.Name = $"Material{xx}";

                //    xx++;
                //}

                //flver.GXLists.Clear();


                // Max 189
                // 90 fine
                // 140 fine
                // 160 fine
                // 165 fine
                // 166 CRASH
                // 170 CRASH
                // First 180 CRASH
                flver.Meshes = flver.Meshes.Skip(165).ToList();

                //var culprit = flver.Meshes[165];

                //foreach (var v in culprit.Vertices)
                //{
                //    //if (v.BoneIndices[0] != 0 || v.BoneIndices[1] != 0 || v.BoneIndices[2] != 0 || v.BoneIndices[3] != 0 ||
                //    //    v.BoneWeights[0] != 0 || v.BoneWeights[1] != 0 || v.BoneWeights[2] != 0 || v.BoneWeights[3] != 0 ||
                //    //    v.Colors[0].R != 1 || v.Colors[0].G != 1 || v.Colors[0].B != 1 || v.Colors[0].A != 1)
                //    //{
                //    //    Console.WriteLine("SDFLJSHDF");
                //    //}
                //    v.UVs[0] = new NVector3(0, 0, 0);
                //    v.UVs[1] = new NVector3(0, 0, 0);

                //    v.Position = new NVector3(0, 0, 0);
                //    v.Normal = NVector3.UnitY;
                //}

                flver.Materials = flver.Materials.Skip(165).ToList();

                flver.Write(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS REMASTERED - Nightfall\map\m10_02_00_00\m4100B2A10.flver");

                flver.Compression = DCX.Type.DCX_DFLT_10000_24_9;

                flver.Write(@"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS REMASTERED - Nightfall\map\m10_02_00_00\m4100B2A10.flver.dcx");

                Console.WriteLine("fatxat");
            }


            if (DebugTestButton("DEMONS SOULS TAE CONVERT"))
            {
                //var tae = TAE.Read(@"C:\RPCS3\dev_hdd0\disc\BLUS30443\PS3_GAME\USRDIR\chr\c0000\c0000-anibnd\Model\chr\c0000\tae\a00.tae");

                var taeFileNames = Directory.GetFiles(@"E:\DarkSoulsModding\_DESANIM\_TaeToDS1", "*.tae");



                foreach (var t in taeFileNames)
                {
                    var tae = TAE.Read(t);

                    var templateDS1 = TAE.Template.ReadXMLFile(System.IO.Path.Combine(new System.IO.FileInfo(typeof(_QuickDebug).Assembly.Location).DirectoryName, $@"Res\TAE.Template.DS1.xml"));
                    var templateDES = TAE.Template.ReadXMLFile(System.IO.Path.Combine(new System.IO.FileInfo(typeof(_QuickDebug).Assembly.Location).DirectoryName, $@"Res\TAE.Template.DES.xml"));

                    tae.ApplyTemplate(templateDES);

                    tae.BigEndian = false;
                    tae.Format = TAE.TAEFormat.DS1;

                    tae.ChangeTemplateAfterLoading(templateDS1);

                    tae.Write($@"E:\DarkSoulsModding\_DESANIM\_TaeToDS1\out\{Path.GetFileNameWithoutExtension(t)}.tae");
                }

                Console.WriteLine("FATCAT");
            }

            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();

            if (DebugTestButton("DEMONS SOULS HKX CONVERT"))
            {
                //var tae = TAE.Read(@"C:\RPCS3\dev_hdd0\disc\BLUS30443\PS3_GAME\USRDIR\chr\c0000\c0000-anibnd\Model\chr\c0000\tae\a00.tae");

                DebugTest_RemapHK2010InterleavedAnims(@"E:\DarkSoulsModding\_DESANIM\_ToDS1");

                Console.WriteLine("FATCAT");
            }

            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();

            if (DebugTestButton("EXPORT CURRENT ANIMATION\n\n[ASSIMP TEST]"))
            {
                var a = Scene.MainModel?.AnimContainer?.CurrentAnimation?.data;

                if (a != null)
                {
                    SoulsAssetPipeline.AnimationExporting.AnimationExporter.ExportToFile(a, @"C:\Users\Green\OneDrive\Documents\SAP_ANIM_EXPORT_TEST.fbx", "fbx");
                }
                else
                {
                    ImguiOSD.DialogManager.DialogOK("CURRENT ANIMATION IS NULL", "CURRENT ANIMATION IS NULL");
                }

                Console.WriteLine("DFSDF");
            }

            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();
            ImGui.Separator();

            if (DebugTestButton("SIBCAM TEST"))
            {
                var s = SIBCAM.Read(@"E:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\remo\scn150000-remobnd\cut0060\camera_win32.sibcam");

                Console.WriteLine("DFSDF");
            }

            if (DebugTestButton("REMO HKX TEST"))
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
            


            if (DebugTestButton("DS2 IMPORT TEST"))
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

            if (DebugTestButton("SoulsAssetPipeline_Anim_Test"))
            {
                var boneDefaultTransforms = Scene.MainModel.AnimContainer.Skeleton.HkxSkeleton.ToDictionary(x => x.Name, x => x.RelativeReferenceTransform);
                var importSettings = new SoulsAssetPipeline.AnimationImporting.AnimationImporter.AnimationImportSettings
                {
                    SceneScale = 1,
                    ExistingBoneDefaults = boneDefaultTransforms,
                    ExistingHavokAnimationTemplate = Scene.MainModel.AnimContainer.CurrentAnimation.data,
                    ResampleToFramerate = 120,
                    RootMotionNodeName = "root",
                    FlipQuaternionHandedness = false,
                    //ConvertFromZUp = true,
                };

                var importedAnim = SoulsAssetPipeline.AnimationImporting.AnimationImporter.ImportFBX(
                    @"E:\DarkSoulsModding\_FBX_ANIM\meow_test.fbx", importSettings);

                string filePath_uncompressedXml = @"E:\DarkSoulsModding\_FBX_ANIM\meow_test.fbx.dsasimport.xml";
                string filePath_compressedHkx = @"E:\DarkSoulsModding\_FBX_ANIM\meow_test.fbx.dsasimport.hkx";

                var compressedHkx = importedAnim.WriteToSplineCompressedHKX2010Bytes(SplineCompressedAnimation.RotationQuantizationType.THREECOMP40, 0.001f);

                //byte[] animData = File.ReadAllBytes(filePath_compressedHkx);
                Main.TAE_EDITOR.FileContainer.AddNewHKX(Utils.GetFileNameWithoutAnyExtensions(Scene.MainModel.AnimContainer.CurrentAnimationName), compressedHkx, out byte[] dataForAnimContainer);
                Main.TAE_EDITOR.Graph.ViewportInteractor.CurrentModel.AnimContainer.AddNewHKXToLoad(Scene.MainModel.AnimContainer.CurrentAnimationName, dataForAnimContainer);

                //importedFlver.WriteToHavok2010InterleavedUncompressedXML(@"C:\DarkSoulsModding\CUSTOM ANIM\c2570\ShieldBashVanilla.fbx.saptest.xml");

                lock (Scene._lock_ModelLoad_Draw)
                {
                    Main.TAE_EDITOR.SelectNewAnimRef(Main.TAE_EDITOR.SelectedTae, Main.TAE_EDITOR.SelectedTaeAnim);
                    //var anim = new NewHavokAnimation(importedAnim, Scene.MainModel.AnimContainer.Skeleton, Scene.MainModel.AnimContainer);
                    //string animName = Scene.MainModel.AnimContainer.CurrentAnimationName;
                    //Scene.MainModel.AnimContainer.AddNewAnimation(animName, anim);
                    //Scene.MainModel.AnimContainer.CurrentAnimationName = null;
                    //Scene.MainModel.AnimContainer.CurrentAnimationName = animName;
                    //Scene.MainModel.AnimContainer.ResetAll();
                    Main.TAE_EDITOR?.HardReset();
                }
                //Console.WriteLine("fatcat");
            }
        }




        private static void DebugTest_RemapHK2010InterleavedAnims(string dir)
        {
            var hkxs = Directory.GetFiles(dir, "*.hkx");

            HKX.HKASkeleton source_skeleton = null;
            HKX.HKASkeleton target_skeleton = null;

            foreach (var h in hkxs)
            {
                if (h.ToLower().EndsWith("sourceskeleton.hkx"))
                {
                    var hkx = HKX.Read(h);
                    foreach (var o in hkx.DataSection.Objects)
                    {
                        if (o is HKX.HKASkeleton asSkeleton)
                            source_skeleton = asSkeleton;
                    }
                }
                else if (h.ToLower().EndsWith("targetskeleton.hkx"))
                {
                    var hkx = HKX.Read(h);
                    foreach (var o in hkx.DataSection.Objects)
                    {
                        if (o is HKX.HKASkeleton asSkeleton)
                            target_skeleton = asSkeleton;
                    }
                }
            }

            var sourceTpose = source_skeleton.Transforms.GetArrayData().Elements.Select(tp => NewBlendableTransform.FromHKXTransform(tp)).ToList();
            var targetTpose = target_skeleton.Transforms.GetArrayData().Elements.Select(tp => NewBlendableTransform.FromHKXTransform(tp)).ToList();

            var targetBonesChildren = new List<List<int>>();
            var targetBonesTopOfHierarchy = new List<int>();

            var sourceBonesChildren = new List<List<int>>();
            var sourceBonesTopOfHierarchy = new List<int>();

            for (int i = 0; i < targetTpose.Count; i++)
            {
                targetBonesChildren.Add(new List<int>());
            }

            for (int i = 0; i < sourceTpose.Count; i++)
            {
                sourceBonesChildren.Add(new List<int>());
            }

            for (int i = 0; i < targetTpose.Count; i++)
            {
                var parent = target_skeleton.ParentIndices[i].data;
                if (parent >= 0)
                {
                    if (parent < targetTpose.Count)
                    {
                        if (!targetBonesChildren[parent].Contains(i))
                            targetBonesChildren[parent].Add(i);
                    }
                }
                else
                {
                    targetBonesTopOfHierarchy.Add(i);
                }
            }

            for (int i = 0; i < sourceTpose.Count; i++)
            {
                var parent = source_skeleton.ParentIndices[i].data;
                if (parent >= 0)
                {
                    if (parent < sourceTpose.Count)
                    {
                        if (!sourceBonesChildren[parent].Contains(i))
                            sourceBonesChildren[parent].Add(i);
                    }
                }
                else
                {
                    sourceBonesTopOfHierarchy.Add(i);
                }
            }

            foreach (var h in hkxs)
            {
                if (h.ToLower().EndsWith("sourceskeleton.hkx"))
                {
                    continue;
                }
                else if (h.ToLower().EndsWith("targetskeleton.hkx"))
                {
                    continue;
                }

                var hkx = HKX.Read(h);

                HKX.HKAAnimationBinding hk_binding = null;
                HKX.HKAInterleavedUncompressedAnimation hk_anim = null;
                HKX.HKADefaultAnimatedReferenceFrame hk_refFrame = null;

                foreach (var o in hkx.DataSection.Objects)
                {
                    if (o is HKX.HKAAnimationBinding asBinding)
                        hk_binding = asBinding;
                    else if (o is HKX.HKAInterleavedUncompressedAnimation asAnim)
                        hk_anim = asAnim;
                    else if (o is HKX.HKADefaultAnimatedReferenceFrame asRefFrame)
                        hk_refFrame = asRefFrame;
                }

                var targetBoneList = target_skeleton.Bones.GetArrayData().Elements.Select(b => b.Name.GetString()).ToList();

                

                var sourceRootMotionSamples = (hk_refFrame != null) ? hk_refFrame.ReferenceFrameSamples.GetArrayData().Elements.Select(rfs => rfs.Vector).ToList() : null;

                if (hk_anim == null)
                    continue;

                var a = new HavokAnimationData_InterleavedUncompressed("fatcat", source_skeleton, hk_refFrame, hk_binding, hk_anim);
                var targetBoneMapping = a.GetTransformTrackBoneNameMapping(targetBoneList);
                var thing = new SoulsAssetPipeline.AnimationImporting.ImportedAnimation();

                thing.Duration = hk_anim.Duration;
                thing.BlendHint = hk_binding.BlendHint;
                thing.FrameCount = (int)hk_anim.Transforms.Capacity / hk_anim.TransformTrackCount;
                thing.FrameDuration = (thing.Duration / thing.FrameCount);

                thing.HkxBoneIndexToTransformTrackMap = new int[targetBoneList.Count];
                thing.TransformTrackIndexToHkxBoneMap = new int[targetBoneList.Count];

                for (int i = 0; i < targetBoneList.Count; i++)
                {
                    thing.HkxBoneIndexToTransformTrackMap[i] = i;
                    thing.TransformTrackIndexToHkxBoneMap[i] = i;
                    thing.TransformTrackNames.Add(targetBoneList[i]);
                    thing.TransformTrackToBoneIndices.Add(targetBoneList[i], i);
                }

                for (int f = 0; f < thing.FrameCount; f++)
                {
                    var newFrame = new SoulsAssetPipeline.AnimationImporting.ImportedAnimation.Frame();
                    if (sourceRootMotionSamples != null)
                    {
                        int rootMotionIndex = f % sourceRootMotionSamples.Count;

                        var sample = sourceRootMotionSamples[rootMotionIndex];

                        if (sourceRootMotionSamples.Count == 2)
                        {
                            if (hk_refFrame.Duration < 0.001f || thing.FrameDuration < 0.01f || thing.FrameDuration > 1f)
                                Console.WriteLine("DFSDF");
                            sample = (sourceRootMotionSamples[1] - sourceRootMotionSamples[0]) * ((f * thing.FrameDuration) / hk_refFrame.Duration);
                        }

                        newFrame.RootMotionRotation = sample.W;
                        newFrame.RootMotionTranslation = new System.Numerics.Vector3(
                            sample.X,
                            sample.Y,
                            sample.Z);
                    }

                    for (int i = 0; i < targetBoneList.Count; i++)
                    {
                        newFrame.BoneTransforms.Add(NewBlendableTransform.FromHKXTransform(target_skeleton.Transforms[i]));
                    }

                    thing.Frames.Add(newFrame);
                }

                var sourceTrackToBoneMap = hk_binding.TransformTrackToBoneIndices.GetArrayData().Elements.Select(bn => bn.data).ToList();
                var sourceBoneToTrackMap = new int[sourceTrackToBoneMap.Count];

                for (int i = 0; i < sourceBoneToTrackMap.Length; i++)
                {
                    sourceBoneToTrackMap[i] = -1;
                }

                for (int i = 0; i < sourceTrackToBoneMap.Count; i++)
                {
                    if (sourceTrackToBoneMap[i] >= 0)
                        sourceBoneToTrackMap[sourceTrackToBoneMap[i]] = i;
                }

                var sourceBoneNames = source_skeleton.Bones.GetArrayData().Elements.Select(bn => bn.Name.GetString()).ToList();

                var hotfixTrackIndex_R_Arm_Root = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_Arm_Root")];
                var hotfixTrackIndex_R_Clavicle = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_Clavicle")];
                var hotfixTrackIndex_R_UpperArm = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_UpperArm")];
                var hotfixTrackIndex_R_UpArmTwist = sourceBoneToTrackMap[sourceBoneNames.IndexOf("RUpArmTwist")];
                var hotfixTrackIndex_R_ForeTwist = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_ForeTwist")];
                var hotfixTrackIndex_R_Finger0 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_Finger0")];
                var hotfixTrackIndex_R_Finger1 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_Finger1")];
                var hotfixTrackIndex_R_Finger2 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_Finger2")];
                var hotfixTrackIndex_R_Finger3 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("R_Finger3")];

                var hotfixTrackIndex_L_Arm_Root = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_Arm_Root")];
                var hotfixTrackIndex_L_Clavicle = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_Clavicle")];
                var hotfixTrackIndex_L_UpperArm = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_UpperArm")];
                var hotfixTrackIndex_L_UpArmTwist = sourceBoneToTrackMap[sourceBoneNames.IndexOf("LUpArmTwist")];
                var hotfixTrackIndex_L_ForeTwist = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_ForeTwist")];
                var hotfixTrackIndex_L_Finger0 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_Finger0")];
                var hotfixTrackIndex_L_Finger1 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_Finger1")];
                var hotfixTrackIndex_L_Finger2 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_Finger2")];
                var hotfixTrackIndex_L_Finger3 = sourceBoneToTrackMap[sourceBoneNames.IndexOf("L_Finger3")];

                var sourceTransforms = hk_anim.Transforms.GetArrayData().Elements;

                foreach (var kvp in targetBoneMapping)
                {
                    int sourceTrackIndex = kvp.Key;

                    

                    int targetBoneIndex = targetBoneList.IndexOf(kvp.Value);

                    int srcBoneIndex = hk_binding.TransformTrackToBoneIndices[sourceTrackIndex].data;

                    var targetBoneTransform = NewBlendableTransform.FromHKXTransform(target_skeleton.Transforms[targetBoneIndex]);
                    var sourceBoneTransform = NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[srcBoneIndex]);

                    for (int f = 0; f < thing.FrameCount; f++)
                    {
                        var srcTransform = NewBlendableTransform.FromHKXTransform(
                            sourceTransforms[((hk_anim.TransformTrackCount * f) + sourceTrackIndex)]);

                        if (sourceTrackIndex == hotfixTrackIndex_L_Clavicle)
                        {
                            var rootThing = NewBlendableTransform.FromHKXTransform(
                                sourceTransforms[((hk_anim.TransformTrackCount * f) + hotfixTrackIndex_L_Arm_Root)]);

                            //srcTransform.Rotation = srcTransform.Rotation * rootThing.Rotation;
                            //srcTransform.Translation = rootThing.Translation;

                            srcTransform = new NewBlendableTransform(srcTransform.GetMatrixUnnormalized() * rootThing.GetMatrixUnnormalized());
                        }
                        else if (sourceTrackIndex == hotfixTrackIndex_R_Clavicle)
                        {
                            var rootThing = NewBlendableTransform.FromHKXTransform(
                                sourceTransforms[((hk_anim.TransformTrackCount * f) + hotfixTrackIndex_R_Arm_Root)]);

                            //srcTransform.Rotation = srcTransform.Rotation * rootThing.Rotation;
                            //srcTransform.Translation = rootThing.Translation;

                            srcTransform = new NewBlendableTransform(srcTransform.GetMatrixUnnormalized() * rootThing.GetMatrixUnnormalized());
                        }
                        //else if (sourceTrackIndex == hotfixTrackIndex_R_UpArmTwist)
                        //{
                        //    srcTransform.Rotation = NQuaternion.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0) * srcTransform.Rotation;
                        //}
                        //else if (sourceTrackIndex == hotfixTrackIndex_L_UpArmTwist)
                        //{
                        //    srcTransform.Rotation = NQuaternion.CreateFromYawPitchRoll(MathHelper.PiOver2, 0, 0) * srcTransform.Rotation;
                        //}
                        else if (sourceTrackIndex == hotfixTrackIndex_R_Finger2)
                        {
                            srcTransform = NewBlendableTransform.FromHKXTransform(
                                sourceTransforms[((hk_anim.TransformTrackCount * f) + hotfixTrackIndex_R_Finger3)]);
                        }
                        else if (sourceTrackIndex == hotfixTrackIndex_L_Finger2)
                        {
                            srcTransform = NewBlendableTransform.FromHKXTransform(
                                sourceTransforms[((hk_anim.TransformTrackCount * f) + hotfixTrackIndex_L_Finger3)]);
                        }

                        thing.Frames[f].BoneTransforms[targetBoneIndex] = srcTransform;// (srcTransform * NewBlendableTransform.Invert(sourceBoneTransform)) * targetBoneTransform;// NewBlendableTransform.ApplyFromToDeltaTransform(srcTransform, sourceTpose[srcBoneIndex], targetTpose[targetBoneIndex]);
                    }
                }

                ////void MultiplyAllAnimTransformFramesOfBone(int boneIdx, NewBlendableTransform mult, bool doRotation)
                ////{
                ////    for (int f = 0; f < thing.FrameCount; f++)
                ////    {
                ////        var tr = thing.Frames[f].BoneTransforms[boneIdx];

                ////        //tr.Translation = NVector3.Transform(tr.Translation, NMatrix.CreateFromQuaternion(mult.Rotation));

                ////        //tr.Translation += mult.Translation;

                ////        if (doRotation)
                ////            tr.Rotation *= mult.Rotation;

                ////        //tr.Scale *= mult.Scale;
                ////        thing.Frames[f].BoneTransforms[boneIdx] = tr;
                ////    }
                ////}

                ////void FixTransformTrack(int targetBoneIndex)
                ////{
                ////    int sourceBoneIndex = source_skeleton.Bones.GetArrayData().Elements.FindIndex(bn => bn.Name.GetString() == targetBoneList[targetBoneIndex]);
                ////    if (sourceBoneIndex >= 0)
                ////    {
                ////        var retargetDelta = NewBlendableTransform.GetDelta(sourceTpose[sourceBoneIndex], targetTpose[targetBoneIndex]);
                ////        MultiplyAllAnimTransformFramesOfBone(targetBoneIndex, retargetDelta, doRotation: true);
                ////        var retargetInverse = NewBlendableTransform.Inverse(retargetDelta);
                ////        foreach (var childIdx in targetBonesChildren[targetBoneIndex])
                ////        {
                ////            MultiplyAllAnimTransformFramesOfBone(childIdx, retargetInverse, doRotation: true);
                ////        }
                ////    }

                ////    foreach (var childIdx in targetBonesChildren[targetBoneIndex])
                ////    {
                ////        FixTransformTrack(childIdx);
                ////    }
                ////}

                ////foreach (var topBoneIdx in targetBonesTopOfHierarchy)
                ////{
                ////    FixTransformTrack(topBoneIdx);
                ////}






                //var absoluteBoneMatrices_TargetSkeleton = new NewBlendableTransform[targetTpose.Count];
                //var absoluteBoneMatrices_SourceSkeleton = new NewBlendableTransform[sourceTpose.Count];
                //// All bones on frame 0, all bones on frame 1, etc
                //var absoluteBoneMatrices_Source = new NewBlendableTransform[thing.FrameCount * sourceTpose.Count];
                //var absoluteBoneMatrices_Source_Fixed = new NewBlendableTransform[thing.FrameCount * sourceTpose.Count];

                //var absoluteBoneMatrices_Target = new NewBlendableTransform[thing.FrameCount * targetTpose.Count];

                //for (int i = 0; i < absoluteBoneMatrices_TargetSkeleton.Length; i++)
                //    absoluteBoneMatrices_TargetSkeleton[i] = NewBlendableTransform.Identity;

                //for (int i = 0; i < absoluteBoneMatrices_SourceSkeleton.Length; i++)
                //    absoluteBoneMatrices_SourceSkeleton[i] = NewBlendableTransform.Identity;

                //for (int i = 0; i < absoluteBoneMatrices_Source.Length; i++)
                //{
                //    absoluteBoneMatrices_Source[i] = NewBlendableTransform.Identity;
                //    absoluteBoneMatrices_Source_Fixed[i] = NewBlendableTransform.Identity;
                //}

                //for (int i = 0; i < absoluteBoneMatrices_Target.Length; i++)
                //{
                //    absoluteBoneMatrices_Target[i] = NewBlendableTransform.Identity;
                //}

                //void CalculateAbsoluteBoneMatrix_Source(int f, int boneIndex, NewBlendableTransform current)
                //{
                //    int sourceTransformTrackIndex = sourceBoneToTrackMap[boneIndex];

                //    var boneTrans = (sourceTransformTrackIndex >= 0 ? NewBlendableTransform.FromHKXTransform(
                //        sourceTransforms[((hk_anim.TransformTrackCount * f) + sourceTransformTrackIndex)])
                //        :

                //        NewBlendableTransform.FromHKXTransform(
                //        source_skeleton.Transforms[boneIndex])) * current;

                //    absoluteBoneMatrices_Source[((sourceTpose.Count * f) + boneIndex)] = boneTrans;

                //    foreach (var childIdx in sourceBonesChildren[boneIndex])
                //        CalculateAbsoluteBoneMatrix_Source(f, childIdx, boneTrans);
                //}

                //void CalculateAbsoluteBoneMatrix_SourceSkeleton(int boneIndex, NewBlendableTransform current)
                //{
                //    var boneTrans = current * NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[boneIndex]);

                //    absoluteBoneMatrices_SourceSkeleton[boneIndex] = boneTrans;

                //    foreach (var childIdx in sourceBonesChildren[boneIndex])
                //        CalculateAbsoluteBoneMatrix_SourceSkeleton(childIdx, boneTrans);
                //}

                //void CalculateAbsoluteBoneMatrix_TargetSkeleton(int boneIndex, NewBlendableTransform current)
                //{
                //    var boneTrans = current * NewBlendableTransform.FromHKXTransform(target_skeleton.Transforms[boneIndex]);

                //    absoluteBoneMatrices_TargetSkeleton[boneIndex] = boneTrans;

                //    foreach (var childIdx in targetBonesChildren[boneIndex])
                //        CalculateAbsoluteBoneMatrix_TargetSkeleton(childIdx, boneTrans);
                //}

                //foreach (var topBoneIdx in targetBonesTopOfHierarchy)
                //{
                //    CalculateAbsoluteBoneMatrix_TargetSkeleton(topBoneIdx, NewBlendableTransform.Identity);
                //}

                //foreach (var topBoneIdx in sourceBonesTopOfHierarchy)
                //{
                //    CalculateAbsoluteBoneMatrix_SourceSkeleton(topBoneIdx, NewBlendableTransform.Identity);
                //}

                //for (int f = 0; f < thing.FrameCount; f++)
                //{
                //    foreach (var topBoneIdx in sourceBonesTopOfHierarchy)
                //    {
                //        CalculateAbsoluteBoneMatrix_Source(f, topBoneIdx, NewBlendableTransform.Identity);
                //    }

                //    for (int t = 0; t < sourceTpose.Count; t++)
                //    {
                //        var thisBoneLocationOnThisFrame = absoluteBoneMatrices_Source[((sourceTpose.Count * f) + t)];

                //        var thisBoneLocationInTPose = absoluteBoneMatrices_SourceSkeleton[t];

                //        //if (sourceBoneNames[t] == "L_Clavicle")
                //        //{
                //        //    thisBoneLocationOnThisFrame = absoluteBoneMatrices_Source[((sourceTpose.Count * f) + sourceBoneNames.IndexOf("L_Arm_Root"))];
                //        //    thisBoneLocationOnThisFrame *= NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[sourceBoneNames.IndexOf("L_Clavicle")]) * thisBoneLocationOnThisFrame;

                //        //    thisBoneLocationInTPose = absoluteBoneMatrices_SourceSkeleton[sourceBoneNames.IndexOf("L_Arm_Root")];
                //        //    thisBoneLocationInTPose = NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[sourceBoneNames.IndexOf("L_Clavicle")]) * thisBoneLocationInTPose;

                //        //    thisBoneLocationOnThisFrame.Rotation = NQuaternion.Conjugate(thisBoneLocationOnThisFrame.Rotation);
                //        //}
                //        //else if (sourceBoneNames[t] == "R_Clavicle")
                //        //{
                //        //    thisBoneLocationOnThisFrame = absoluteBoneMatrices_Source[((sourceTpose.Count * f) + sourceBoneNames.IndexOf("R_Arm_Root"))];
                //        //    thisBoneLocationOnThisFrame = NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[sourceBoneNames.IndexOf("R_Clavicle")]) * thisBoneLocationOnThisFrame;

                //        //    thisBoneLocationInTPose = absoluteBoneMatrices_SourceSkeleton[sourceBoneNames.IndexOf("R_Arm_Root")];
                //        //    thisBoneLocationInTPose = NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[sourceBoneNames.IndexOf("R_Clavicle")]) * thisBoneLocationInTPose;

                //        //    thisBoneLocationOnThisFrame.Rotation = NQuaternion.Conjugate(thisBoneLocationOnThisFrame.Rotation);
                //        //}

                //        //if (sourceBoneNames[t] == "L_Clavicle")
                //        //{

                //        //}

                //        int targetBoneIndex = targetBoneList.IndexOf(source_skeleton.Bones[t].Name.GetString());

                //        if (targetBoneIndex >= 0)
                //        {
                //            var thisBoneLocationInTargetTPose = absoluteBoneMatrices_TargetSkeleton[targetBoneIndex];
                //            var thisBoneLocationOnThisFrameRelativeToTPose = thisBoneLocationOnThisFrame * NewBlendableTransform.Invert(thisBoneLocationInTPose);
                //            absoluteBoneMatrices_Source_Fixed[((sourceTpose.Count * f) + t)] = thisBoneLocationInTargetTPose * thisBoneLocationOnThisFrameRelativeToTPose;

                //            //absoluteBoneMatrices_Target[((targetTpose.Count * f) + targetBoneIndex)] = thisBoneLocationOnThisFrame * (thisBoneLocationInTargetTPose * NewBlendableTransform.Invert(thisBoneLocationInTPose));

                //            absoluteBoneMatrices_Target[((targetTpose.Count * f) + targetBoneIndex)] = thisBoneLocationOnThisFrame;// * NewBlendableTransform.Invert(thisBoneLocationInTPose) * thisBoneLocationInTargetTPose;

                //        }
                //        //else

                //        //{
                //        //    absoluteBoneMatrices_Target[((targetTpose.Count * f) + targetBoneIndex)] = new NewBlendableTransform() { Translation = new NVector3(10, 0, 0) };
                //        //}


                //    }
                //}

                ////void CalculateAbsoluteBoneMatrix_Target_FinalOutput(int f, int boneIndex, NewBlendableTransform current)
                ////{
                ////    var boneTrans = absoluteBoneMatrices_Target[((targetTpose.Count * f) + boneIndex)];

                ////    var finalMatrix = boneTrans * NewBlendableTransform.Invert(current);

                ////    //if (finalMatrix.Decompose(out Vector3 s, out Quaternion r, out Vector3 t))
                ////    //{
                ////    //    thing.Frames[f].BoneTransforms[boneIndex] = new NewBlendableTransform()
                ////    //    {
                ////    //        Translation = t.ToCS(),
                ////    //        Rotation = r.ToCS(),
                ////    //        //Scale = s.ToCS(),
                ////    //    };
                ////    //}

                ////    thing.Frames[f].BoneTransforms[boneIndex] = finalMatrix;

                ////    foreach (var childIdx in targetBonesChildren[boneIndex])
                ////        CalculateAbsoluteBoneMatrix_Target_FinalOutput(f, childIdx, boneTrans);
                ////}

                ////for (int f = 0; f < thing.FrameCount; f++)
                ////{
                ////    foreach (var topBoneIdx in targetBonesTopOfHierarchy)
                ////    {
                ////        CalculateAbsoluteBoneMatrix_Target_FinalOutput(f, topBoneIdx, NewBlendableTransform.Identity);
                ////    }
                ////}

                //var hotfixBoneIndex_R_Clavicle = targetBoneList.IndexOf("R_Clavicle");
                //var hotfixBoneIndex_L_Clavicle = targetBoneList.IndexOf("L_Clavicle");
                //var hotfixBoneIndex_Spine1 = sourceBoneNames.IndexOf("Spine1");

                //var hotfixBoneIndex_Source_R_Arm_Root = sourceBoneNames.IndexOf("R_Arm_Root");
                //var hotfixBoneIndex_Source_L_Arm_Root = sourceBoneNames.IndexOf("L_Arm_Root");

                //for (int f = 0; f < thing.FrameCount; f++)
                //{
                //    for (int t = 0; t < targetTpose.Count; t++)
                //    {
                //        var absoluteBonePos = absoluteBoneMatrices_Target[((targetTpose.Count * f) + t)];

                //        var sourceBoneIndex = -1;// sourceBoneNames.IndexOf(targetBoneList[t]);

                //        if (sourceBoneIndex < 0)
                //        {
                //            int parentBoneIndex = target_skeleton.ParentIndices[t].data;

                //            string parentBoneName = parentBoneIndex >= 0 ? targetBoneList[parentBoneIndex] : null;

                //            var boneName = targetBoneList[t];

                //            //if (boneName == "R_Clavicle")
                //            //{
                //            //    absoluteBonePos = absoluteBoneMatrices_Source[(sourceTpose.Count * f) + hotfixBoneIndex_Source_R_Arm_Root];
                //            //}
                //            //else if (boneName == "L_Clavicle")
                //            //{
                //            //    absoluteBonePos = absoluteBoneMatrices_Source[(sourceTpose.Count * f) + hotfixBoneIndex_Source_L_Arm_Root];
                //            //}

                //            if (boneName == "R_Clavicle" || boneName == "L_Clavicle")
                //            {
                //                //absoluteBonePos.Rotation *= NQuaternion.CreateFromYawPitchRoll(0, SoulsAssetPipeline.SapMath.PiOver2, 0);
                //            }
                //            else if (boneName == "R_UpperArm")
                //            {
                //                //absoluteBonePos.Rotation *= NQuaternion.CreateFromYawPitchRoll(0, 0, SoulsAssetPipeline.SapMath.PiOver2);

                //                //absoluteBonePos.Rotation.X *= -1;
                //                //absoluteBonePos.Rotation = SoulsAssetPipeline.SapMath.MirrorQuat(absoluteBonePos.Rotation);
                //            }
                //            else if (boneName == "L_UpperArm")
                //            {
                //                // THIS MAKES IT ALMOST RIGHT JUST UPSIDE DOWN FATCAT
                //                //absoluteBonePos.Rotation *= NQuaternion.CreateFromYawPitchRoll(SoulsAssetPipeline.SapMath.Pi, 0, 0);

                //                //absoluteBonePos.Rotation = NQuaternion.CreateFromRotationMatrix(NMatrix.CreateFromQuaternion(absoluteBonePos.Rotation) * NMatrix.CreateRotationY(-SoulsAssetPipeline.SapMath.Pi)
                //                //    * NMatrix.CreateRotationZ(-SoulsAssetPipeline.SapMath.PiOver2));

                //                //absoluteBonePos.Rotation = NQuaternion.CreateFromRotationMatrix(NMatrix.CreateFromQuaternion(absoluteBonePos.Rotation) * NMatrix.CreateRotationX(SoulsAssetPipeline.SapMath.Pi)
                //                //    * NMatrix.CreateRotationZ(SoulsAssetPipeline.SapMath.Pi));
                //            }
                //            //else if (boneName == "L_Forearm")
                //            //{
                //            //    // THIS MAKES IT ALMOST RIGHT JUST UPSIDE DOWN FATCAT
                //            //    absoluteBonePos.Rotation *= NQuaternion.CreateFromYawPitchRoll(0, 0, -SoulsAssetPipeline.SapMath.PiOver2);

                //            //    //absoluteBonePos.Rotation = NQuaternion.CreateFromRotationMatrix(NMatrix.CreateFromQuaternion(absoluteBonePos.Rotation) * NMatrix.CreateRotationY(-SoulsAssetPipeline.SapMath.Pi)
                //            //    //    * NMatrix.CreateRotationZ(-SoulsAssetPipeline.SapMath.PiOver2));
                //            //}

                //            if (parentBoneIndex >= 0)
                //            {
                //                var parentAbsolutePos = absoluteBoneMatrices_Target[((targetTpose.Count * f) + parentBoneIndex)];

                //                //if (boneName == "R_Clavicle")
                //                //{
                //                //    parentAbsolutePos = absoluteBoneMatrices_Source[(sourceTpose.Count * f) + hotfixBoneIndex_Spine1];
                //                //}
                //                //else if (boneName == "L_Clavicle")
                //                //{
                //                //    parentAbsolutePos = absoluteBoneMatrices_Source[(sourceTpose.Count * f) + hotfixBoneIndex_Spine1];
                //                //}


                //                absoluteBonePos *= NewBlendableTransform.Invert(parentAbsolutePos);

                //                if (boneName == "R_Clavicle")
                //                {
                //                    //var source_rootThing = absoluteBoneMatrices_Source[(hk_anim.TransformTrackCount * f) + sourceBoneNames.IndexOf("R_Arm_Root")];
                //                    //var source_clavicle = absoluteBoneMatrices_Source[(hk_anim.TransformTrackCount * f) + sourceBoneNames.IndexOf("R_Clavicle")];

                //                    //var memeQuat = SoulsAssetPipeline.SapMath.GetDeltaQuaternionWithDirectionVectors(source_clavicle.Rotation, source_rootThing.Rotation);

                //                    //var mat = NMatrix.CreateFromQuaternion(absoluteBonePos.Rotation) * NMatrix.CreateRotationZ(-MathHelper.PiOver4);

                //                    //absoluteBonePos.Rotation = NQuaternion.CreateFromRotationMatrix(mat);
                //                    //absoluteBonePos.Rotation *= NQuaternion.CreateFromYawPitchRoll(0, 0, -SoulsAssetPipeline.SapMath.Pi);
                //                }
                //                else if (boneName == "L_Clavicle")
                //                {
                //                    //absoluteBonePos = NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[hotfixBoneIndex_Source_L_Arm_Root]) * absoluteBonePos;

                //                    //absoluteBonePos.Rotation *= NQuaternion.Inverse(NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[hotfixBoneIndex_Source_L_Arm_Root]).Rotation);
                //                    //int sourceTransformTrack = sourceBoneToTrackMap[hotfixBoneIndex_Source_L_Arm_Root];
                //                    //var sourceMotion = NewBlendableTransform.FromHKXTransform(sourceTransforms[((hk_anim.TransformTrackCount * f) + sourceTransformTrack)]);
                //                    //absoluteBonePos *= sourceMotion;
                //                }
                //                else if (boneName == "R_UpperArm")
                //                {

                //                }
                //                else if (boneName == "L_UpperArm")
                //                {
                //                    //absoluteBonePos *= NewBlendableTransform.FromHKXTransform(source_skeleton.Transforms[hotfixBoneIndex_Source_L_Arm_Root]);
                //                    //int sourceTransformTrack = sourceBoneToTrackMap[hotfixBoneIndex_Source_L_Arm_Root];
                //                    //var sourceMotion = NewBlendableTransform.FromHKXTransform(sourceTransforms[((hk_anim.TransformTrackCount * f) + sourceTransformTrack)]);
                //                    //absoluteBonePos.Rotation *= sourceMotion.Rotation;
                //                }
                //            }
                //            //thing.Frames[f].BoneTransforms[t] = absoluteBonePos;
                //        }
                //        else
                //        {
                //            int parentBoneIndex = source_skeleton.ParentIndices[sourceBoneIndex].data;

                //            if (t == hotfixBoneIndex_R_Clavicle || t == hotfixBoneIndex_L_Clavicle)
                //            {
                //                parentBoneIndex = hotfixBoneIndex_Spine1;
                //            }

                //            if (parentBoneIndex >= 0)
                //            {
                //                var parentAbsolutePos = absoluteBoneMatrices_Source[((sourceTpose.Count * f) + parentBoneIndex)];
                //                absoluteBonePos *= NewBlendableTransform.Invert(parentAbsolutePos);
                //            }
                //            thing.Frames[f].BoneTransforms[t] = absoluteBonePos;
                //        }


                //    }
                //}

                for (int t = 0; t < targetBoneList.Count; t++)
                {
                    for (int f = 1; f < thing.FrameCount; f++)
                    {
                        var tr = thing.Frames[f].BoneTransforms[t];
                        tr.Rotation = NQuaternion.Normalize(tr.Rotation);
                        thing.Frames[f].BoneTransforms[t] = tr;
                    }
                }

                for (int t = 0; t < targetBoneList.Count; t++)
                {
                    for (int f = 1; f < thing.FrameCount; f++)
                    {
                        // Use dot product to see how drastically this transform rotated in the span of 1 frame.
                        var dot = NQuaternion.Dot(thing.Frames[f - 1].BoneTransforms[t].Rotation, thing.Frames[f].BoneTransforms[t].Rotation);
                        // If it did more than a 90 degree turn in a single frame, it's just flipping
                        // around backwards and we need to flip it back lol
                        if (dot < -0.5)
                        {
                            var tr = thing.Frames[f].BoneTransforms[t];
                            tr.Rotation = NQuaternion.Normalize(-tr.Rotation);
                            thing.Frames[f].BoneTransforms[t] = tr;
                        }
                    }
                }


                //for (int f = 0; f < thing.FrameCount; f++)
                //{
                //    for (int t = 0; t < targetBoneList.Count; t++)
                //    {
                //        var tr = thing.Frames[f].BoneTransforms[t];
                //        int sourceBoneIndex = source_skeleton.Bones.GetArrayData().Elements.FindIndex(bn => bn.Name.GetString() == targetBoneList[t]);
                //        if (sourceBoneIndex < 0)
                //        {
                //            thing.Frames[f].BoneTransforms[t] = targetTpose[t];
                //            continue;
                //        }
                //        var sourceRelativeToTpose = NewBlendableTransform.GetDelta(sourceTpose[sourceBoneIndex], tr);

                //        var matSourceTpose = sourceTpose[sourceBoneIndex].GetMatrixUnnormalized().ToXna();
                //        var matSourceCurr = tr.GetMatrixUnnormalized().ToXna();
                //        var matTargetTpose = targetTpose[t].GetMatrixUnnormalized().ToXna();

                //        var mat = (matSourceCurr * Matrix.Invert(matSourceTpose) * matTargetTpose).ToCS();
                //        if (NMatrix.Decompose(mat, out NVector3 ss, out NQuaternion rr, out NVector3 tt))
                //        {
                //            tr.Rotation = NQuaternion.Normalize(rr);
                //            tr.Translation = tt;

                //            thing.Frames[f].BoneTransforms[t] = tr;
                //        }


                //    }
                //}

                string shortHkxName = Path.GetFileNameWithoutExtension(h);
                string outDir = dir + "\\retargeted";
                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);

                byte[] hkxFinalBytes = thing.WriteToSplineCompressedHKX2010Bytes(SplineCompressedAnimation.RotationQuantizationType.THREECOMP40, 0.001f);

                File.WriteAllBytes($@"{outDir}\{shortHkxName}.hkx", hkxFinalBytes);
            }

            Console.WriteLine("FATCAT");
        }





        private static bool DebugTestButton(string name)
        {
            ImGui.Button("TEST: " + name);
            return ImGui.IsItemClicked();
        }
    }
}
