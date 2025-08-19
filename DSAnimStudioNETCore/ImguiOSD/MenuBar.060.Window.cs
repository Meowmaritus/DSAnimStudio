using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static void BuildMenuBar_060_Window(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu("Window"))
            {
                isAnyMenuExpanded = true;
                
                DoWindow(OSD.WindowHelp);
                ImGui.Separator();
                DoWindow(OSD.WindowProject);
                DoWindow(OSD.WindowEntity);
                DoWindow(OSD.WindowEquipment);
                DoWindow(OSD.WindowScene);
                DoWindow(OSD.WindowSound);
                DoWindow(OSD.WindowToolbox);
                DoWindow(OSD.SpWindowNotifications);
                DoWindow(OSD.SpWindowERRORS);

                if (OSD.EnableDebugMenu)
                {
                    ImGui.Separator();
                    DoWindow(OSD.WindowDebug);
                }

                if (Main.Debug.EnableImguiDebugListAllStaticWindows)
                {
                    ImGui.Separator();
                    
                    OSD.ForAllStaticWindows(window =>
                    {
                        DoWindow(window);
                    });
                }
                else
                {
                    if (!Main.IsDebugBuild)
                        OSD.WindowDebug.IsOpen = false;
                }

                ImGui.EndMenu();
            }

        }
    }
}
