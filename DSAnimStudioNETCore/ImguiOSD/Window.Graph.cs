using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Graph : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;

            public override string NewImguiWindowTitle => "Graph";

            protected override void Init()
            {
                
            }

            protected override void PreUpdate()
            {
                ClipDockTabArea = false;
                IsOpen = true;
                Flags = ImGuiWindowFlags.NoInputs;
                CustomBackgroundColor = System.Numerics.Vector4.Zero;
                CustomBorderColor_Focused = System.Numerics.Vector4.Zero;
                CustomBorderColor_Unfocused = System.Numerics.Vector4.Zero;
            }
            
            protected override void BuildContents(ref bool anyFieldFocused)
            {
                float cursorX = ImGui.GetCursorPosX() - 8;

                CustomBackgroundColor = System.Numerics.Vector4.Zero;
                
                if (MainTaeScreen.TaeAnimInfoIsClone)
                {
                    var thisWindowRect = GetRect(subtractTitleBar: false);
                    if (ImGuiDebugDrawer.FakeButton(new Vector2(thisWindowRect.X + 10, thisWindowRect.Y + (ImGui.GetCursorPosY() / Main.DPI) + 1), 
                            new Vector2((int)(170), 20) - new Vector2(2,2), "Go To Original (F4 Key)", 0, out bool isHovering))
                    {
                        MainTaeScreen.RequestGoToEventSource = true;
                        
                    }
                    cursorX += (170 + 8) * Main.DPI;
                }
                
                ImGui.SetCursorPosX(cursorX);
                
                ImGui.Text("  " + (MainTaeScreen?.SelectedTaeAnimInfoText ?? ""));
                //ImGui.Text(OSD.FocusedWindow?.NewImguiWindowTitle ?? "");
                
                var dsid = ImGui.GetID("DockSpace_NewActionGraph");
                // ImGuiDockNodeFlags (1 << 12) is ImGuiDockNodeFlags_NoTabBar
                ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.PassthruCentralNode | (ImGuiDockNodeFlags)(1 << 12));

                
                
                ImGui.SetWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                //ImGui.Text(" ");
                
            }
        }
    }
}
