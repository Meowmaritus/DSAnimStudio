using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class _QuickDebug
    {
        

        public static void BuildDebugMenu()
        {
            if (DebugButton("SoulsAssetPipeline_Test"))
            {
                LoadingTaskMan.DoLoadingTask("SoulsAssetPipeline_Test", "TEST: SoulsAssetPipeline_Model", prog =>
                {
                    var importSettings = new SoulsAssetPipeline.FLVERImporting.FLVER2Importer.FLVER2ImportSettings()
                    {
                        SceneScale = 0.01f,
                        Game = SoulsAssetPipeline.SoulsGames.DS1,
                    };

                    SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model importedFlver = null;

                    using (var importer = new SoulsAssetPipeline.FLVERImporting.FLVER2Importer())
                    {
                        importedFlver = importer.ImportFBX(@"C:\DarkSoulsModding_Nightfall\AbyssKnight (2)\AbyssKnight.fbx", importSettings);
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

               
            }

            if (DebugButton("SoulsAssetPipeline_Anim_Test"))
            {
                var boneNames = Scene.MainModel.Skeleton.HkxSkeleton.Select(x => x.Name).ToList();
                var importSettings = new SoulsAssetPipeline.AnimationImporting.AnimationImporter.AnimationImportSettings
                {
                    SceneScale = 0.01f,
                    ExistingBoneNameList = boneNames,
                    ExistingHavokAnimationTemplate = Scene.MainModel.AnimContainer.CurrentAnimation.data,
                    ResampleToFramerate = 60,
                    RootMotionNodeName = "root"
                };

                var importedAnim = SoulsAssetPipeline.AnimationImporting.AnimationImporter.ImportFBX(
                    @"C:\DarkSoulsModding\CUSTOM ANIM\c2570\ShieldBashVanilla.fbx", importSettings);

                //importedFlver.WriteToHavok2010InterleavedUncompressedXML(@"C:\DarkSoulsModding\CUSTOM ANIM\c2570\ShieldBashVanilla.fbx.saptest.xml");

                lock (Scene._lock_ModelLoad_Draw)
                {
                    var anim = new NewHavokAnimation(importedAnim, Scene.MainModel.Skeleton, Scene.MainModel.AnimContainer);
                    string animName = Scene.MainModel.AnimContainer.CurrentAnimationName;
                    Scene.MainModel.AnimContainer.AddNewAnimation(animName, anim);
                    Scene.MainModel.AnimContainer.CurrentAnimationName = null;
                    Scene.MainModel.AnimContainer.CurrentAnimationName = animName;
                    Scene.MainModel.AnimContainer.ResetAll();
                    Main.TAE_EDITOR?.HardReset();
                }
                Console.WriteLine("fatcat");
            }
        }

        private static bool DebugButton(string name)
        {
            ImGui.Button(name);
            return ImGui.IsItemClicked();
        }
    }
}
