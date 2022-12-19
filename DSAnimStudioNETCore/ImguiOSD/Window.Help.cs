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
        public class Help : Window
        {
            public override string Title => "Help";
            public override string ImguiTag => $"{nameof(Window)}.{nameof(Help)}";
            protected override void BuildContents()
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 400);
                ImGui.TextWrapped(TaeEditorScreen.HELP_TEXT);
                ImGui.PopTextWrapPos();
            }
        }
    }
}
