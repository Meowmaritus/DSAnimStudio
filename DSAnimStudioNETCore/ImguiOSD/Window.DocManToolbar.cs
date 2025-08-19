using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class DocManToolbar : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;
            public override string NewImguiWindowTitle => "Documents";
            protected override void Init()
            {
                
            }
            
            protected override void PreUpdate()
            {
                IsOpen = MainProj != null;
                Flags = ImGuiWindowFlags.None
                    | ImGuiWindowFlags.NoDecoration
                    | ImGuiWindowFlags.NoMove
                    | ImGuiWindowFlags.NoResize
                    | ImGuiWindowFlags.NoDocking
                    | ImGuiWindowFlags.NoSavedSettings
                    | ImGuiWindowFlags.NoTitleBar
                    | ImGuiWindowFlags.NoBackground
                    | ImGuiWindowFlags.NoNavInputs
                    | ImGuiWindowFlags.NoScrollbar
                    //| ImGuiWindowFlags.MenuBar
                    ;

                ClipDockTabArea = false;

                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

                
            }

            protected override void PostUpdate()
            {
                ImGui.PopStyleVar();
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                ImGuiViewportPtr viewport = ImGui.GetMainViewport();
                var vpWorkSize = viewport.Size;
                var vpWorkPos = viewport.Pos;
                ImGui.SetWindowPos(new Vector2(vpWorkPos.X, vpWorkPos.Y + OSD.DocManToolbarOffset));
                ImGui.SetWindowSize(new Vector2(vpWorkSize.X, OSD.DocManToolbarHeight));


                //ImGui.Text("DocMan Test");

                //ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (6 * Main.DPI));

                //ImGuiDebugDrawer.DrawRect(pos / Main.DPIVector, size / Main.DPIVector, Color.Yellow);
                bool innerFieldFocused = false;
                zzz_DocumentManager.DrawImgui(ref innerFieldFocused);
                //if (innerFieldFocused)
                //    anyFieldFocused = true;
                //ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 16);
            }
        }
    }
}
