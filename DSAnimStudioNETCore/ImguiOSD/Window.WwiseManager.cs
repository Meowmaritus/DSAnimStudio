using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class WwiseManager : Window
        {
            public override string Title => "Wwise Options";
            public override string ImguiTag => $"{nameof(Window)}.{nameof(WwiseManager)}";
            protected override void BuildContents()
            {
                Wwise.InitNamesAndIDs();

                ImGui.Checkbox("SHOW DEBUG DIAGNOSTICS", ref SoundManager.DebugShowDiagnostics);

                ImGui.Button("Stop All Sounds");
                if (ImGui.IsItemClicked())
                    SoundManager.StopAllSounds();

                ImGui.Button("Purge Sound Cache");
                if (ImGui.IsItemClicked())
                {
                    SoundManager.PurgeLoadedAssets();
                    GC.Collect();
                }

                ImGui.Separator();

                if (Wwise.DefensiveMaterialNames != null && Wwise.DefensiveMaterialNames.Length > 0)
                    ImGui.ListBox("Floor Material", ref Wwise.DefensiveMaterialIndex, Wwise.DefensiveMaterialNames, Wwise.DefensiveMaterialNames.Length);


                //ImGui.ListBox("Armor Material (Top)", ref Wwise.ArmorMaterialIndexTop, Wwise.ArmorMaterialNames, Wwise.ArmorMaterialNames.Length);
                //ImGui.ListBox("Armor Material (Bottom)", ref Wwise.ArmorMaterialIndexBottom, Wwise.ArmorMaterialNames, Wwise.ArmorMaterialNames.Length);

                //ImGui.ListBox("Player Voice Type", ref Wwise.PlayerVoiceIndex, Wwise.PlayerVoiceTypes, Wwise.PlayerVoiceTypes.Length);

                foreach (var switchGroupHandler in Wwise.SwitchGroupHandlers)
                {
                    switchGroupHandler.DoImGui();
                }

                
            }
        }
    }
}
