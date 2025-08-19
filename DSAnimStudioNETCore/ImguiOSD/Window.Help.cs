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
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "Help";
            
            protected override void Init()
            {
                
            }

            private string[] helpText = null;
            private object _lock_helpText = new object();

            protected override void BuildContents(ref bool anyFieldFocused)
            {

                //ImGui.PushTextWrapPos(ImGui.GetFontSize() * 400);

                lock (_lock_helpText)
                {
                    if (helpText == null)
                    {
                        helpText = Main.GetEmbeddedResourceText("/EmbRes/Help.txt").Split('\n');
                    }

                    for (int i = 0; i < helpText.Length; i++)
                    {
                        bool isHeader = helpText[i].StartsWith("[");
                        bool isIndent = helpText[i].StartsWith("    ");
                        
                        if (isHeader)
                            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FFFF);
                        else if (!isIndent)
                            ImGui.PushStyleColor(ImGuiCol.Text, 0xFFFFFF00);
                        
                        ImGui.TextWrapped(helpText[i]);
                        
                        if (isHeader)
                            ImGui.PopStyleColor();
                        else if (!isIndent)
                            ImGui.PopStyleColor();
                    }
                    //ImGui.PopTextWrapPos();
                }
                
                
            }
        }
    }
}
