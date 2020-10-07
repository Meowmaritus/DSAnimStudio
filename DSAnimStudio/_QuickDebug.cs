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

            if (false && DebugButton("SoulsAssetPipeline_Anim_Test"))
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
