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
                    var importSettings = new SoulsAssetPipeline.FLVER2Importer.FLVER2ImportSettings()
                    {
                        SceneScale = 0.01f,
                    };

                    var importedFlver = SoulsAssetPipeline.FLVER2Importer.ImportFBX(
                        @"C:\DarkSoulsModding_Nightfall\AbyssKnight (2)\AbyssKnight.fbx", importSettings);


                    foreach (var tex in importedFlver.Textures)
                    {
                        TexturePool.AddFetchDDS(tex.Value, tex.Key);
                    }

                    Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

                    for (int i = 0; i < Scene.Models[0].Skeleton.FlverSkeleton.Count; i++)
                    {
                        if (!boneIndexRemap.ContainsKey(Scene.Models[0].Skeleton.FlverSkeleton[i].Name))
                            boneIndexRemap.Add(Scene.Models[0].Skeleton.FlverSkeleton[i].Name, i);
                    }

                    var oldMainMesh = Scene.Models[0].MainMesh;
                    var newMainMesh = new NewMesh(importedFlver.Flver, false, boneIndexRemap);

                    lock (Scene._lock_ModelLoad_Draw)
                    {
                        Scene.Models[0].MainMesh = newMainMesh;
                    }

                    oldMainMesh?.Dispose();

                    Scene.ForceTextureReloadImmediate();

                }, disableProgressBarByDefault: true, isUnimportant: true);

               
            }

            if (DebugButton("SoulsAssetPipeline_Anim_Test"))
            {
                var boneNames = Scene.Models[0].Skeleton.HkxSkeleton.Select(x => x.Name).ToList();
                var importSettings = new SoulsAssetPipeline.AnimationImporter.AnimationImportSettings
                {
                    SceneScale = 0.01f,
                    ExistingBoneNameList = boneNames,
                    ExistingHavokAnimationTemplate = Scene.Models[0].AnimContainer.CurrentAnimation.data,
                    ResampleToFramerate = 60,
                    RootMotionNodeName = "root"
                };

                var importedAnim = SoulsAssetPipeline.AnimationImporter.ImportFBX(
                    @"C:\DarkSoulsModding\CUSTOM ANIM\c2570 shield bash test.FBX", importSettings);

                //importedFlver.WriteToHavok2010InterleavedUncompressedXML(@"C:\DarkSoulsModding\CUSTOM ANIM\c2570\ShieldBashVanilla.fbx.saptest.xml");

                lock (Scene._lock_ModelLoad_Draw)
                {
                    var anim = new NewHavokAnimation(importedAnim, Scene.Models[0].Skeleton, Scene.Models[0].AnimContainer);
                    string animName = Scene.Models[0].AnimContainer.CurrentAnimationName;
                    Scene.Models[0].AnimContainer.AddNewAnimation(animName, anim);
                    Scene.Models[0].AnimContainer.CurrentAnimationName = null;
                    Scene.Models[0].AnimContainer.CurrentAnimationName = animName;
                    Scene.Models[0].AnimContainer.ResetAll();
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
