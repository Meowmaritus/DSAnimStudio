using DSAnimStudio.TaeEditor;
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
        public class Graph_Actions : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.NoSave;

            public override string NewImguiWindowTitle => "Actions";

            protected override void Init()
            {
                IsOpen = true;
                Flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs;
            }

            protected override void PreUpdate()
            {
                IsOpen = true;
            }

            protected override void PostUpdate()
            {
                
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                ImGui.SetWindowSize(new System.Numerics.Vector2(300, 300), ImGuiCond.FirstUseEver);
            }
        }
    }
}
