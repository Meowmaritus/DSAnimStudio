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
            protected override void BuildContents()
            {
                //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color", DBG.DbgPrim_Grid.OverrideColor.Value);

                if (OSD.RequestExpandAllTreeNodes || OSD.IsInit)
                    ImGui.SetNextItemOpen(true);

                _QuickDebug.BuildDebugMenu();

                ImGui.Separator();

                ImGui.Button("Hot Reload FlverShader.xnb");
                if (ImGui.IsItemClicked())
                    GFX.ReloadFlverShader();

                ImGui.Button("Hot Reload FlverTonemapShader.xnb");
                if (ImGui.IsItemClicked())
                    GFX.ReloadTonemapShader();

                ImGui.Button("Hot Reload CubemapSkyboxShader.xnb");
                if (ImGui.IsItemClicked())
                    GFX.ReloadCubemapSkyboxShader();
            }
        }
    }
}
