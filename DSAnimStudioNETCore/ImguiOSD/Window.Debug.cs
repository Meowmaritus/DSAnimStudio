using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Debug : Window
        {
            public override string Title => "Debug";
            public override string ImguiTag => $"{nameof(Window)}.{nameof(Debug)}";
            protected override void BuildContents()
            {
                //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color", DBG.DbgPrim_Grid.OverrideColor.Value);

                if (OSD.RequestExpandAllTreeNodes || OSD.IsInit)
                    ImGui.SetNextItemOpen(true);

                if (OSD.EnableDebugMenuFull)
                {
                    _QuickDebug.BuildDebugMenu();
                    ImGui.Separator();
                }

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

                    ImGui.Separator();

                    foreach (var hkxBone in Scene.MainModel.AnimContainer.Skeleton.HkxSkeleton)
                    {
                        float boneWeight = hkxBone.Weight;
                        ImGui.SliderFloat($"{hkxBone.Name}", ref boneWeight, 0, 1);
                        hkxBone.Weight = boneWeight;
                    }
                }

                if (OSD.EnableDebugMenuFull)
                {
                    ImGui.Button("Hot Reload FlverShader.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                    if (ImGui.IsItemClicked())
                        GFX.ReloadFlverShader();

                    ImGui.Button("Hot Reload FlverTonemapShader.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                    if (ImGui.IsItemClicked())
                        GFX.ReloadTonemapShader();

                    ImGui.Button("Hot Reload CubemapSkyboxShader.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                    if (ImGui.IsItemClicked())
                        GFX.ReloadCubemapSkyboxShader();
                }
            }
        }
    }
}
