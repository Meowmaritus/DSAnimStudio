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
        public class NewTransport : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;

            public override string NewImguiWindowTitle => "Transport";
            protected override void Init()
            {
                
            }
            
            protected override void PreUpdate()
            {
                IsOpen = true;
                Flags = ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | 
                        ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground;
                CustomBackgroundColor = System.Numerics.Vector4.Zero;
                CustomBorderColor_Focused = System.Numerics.Vector4.Zero;
                CustomBorderColor_Unfocused = System.Numerics.Vector4.Zero;
                
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                Vector2 pos = new Vector2(Program.MainInstance.ClientBounds.Width - (380 * Main.DPI), -16 * Main.DPI);
                Vector2 size = new Vector2(380 * Main.DPI, 100 * Main.DPI);
                ImGui.SetWindowPos(pos);
                ImGui.SetWindowSize(size);
                
                //ImGuiDebugDrawer.DrawRect(pos / Main.DPIVector, size / Main.DPIVector, Color.Yellow);
                
                
            }
        }
    }
}
