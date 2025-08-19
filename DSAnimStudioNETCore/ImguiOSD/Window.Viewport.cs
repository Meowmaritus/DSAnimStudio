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
        public class Viewport : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;

            public override string NewImguiWindowTitle => "Viewport";
            protected override void Init()
            {
                
            }

            protected override void PreUpdate()
            {
                IsOpen = true;
                Flags = ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMove;
                CustomBackgroundColor = System.Numerics.Vector4.Zero;
                CustomBorderColor_Focused = System.Numerics.Vector4.Zero;
                CustomBorderColor_Unfocused = System.Numerics.Vector4.Zero;
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                var currentSize = ImGui.GetWindowSize();
                if (Math.Abs(currentSize.X - LastSize.X) > 0.01f || Math.Abs(currentSize.Y - LastSize.Y) > 0.01f)
                {
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }
                ImGui.SetWindowSize(new Vector2(300, 300), ImGuiCond.FirstUseEver);
                
                
            }
        }
    }
}
