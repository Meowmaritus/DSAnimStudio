using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class MultiChoice : Dialog
        {
            

            public string Question;
            public List<string> Answers;
            public string SelectedAnswer = null;

            protected override void BuildInsideOfWindow()
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 400);
                ImGui.TextWrapped(Question);
                ImGui.PopTextWrapPos();

                for (int i = 0; i < Answers.Count; i++)
                {
                    if (i > 0)
                        ImGui.SameLine();
                    ImGui.Button(Answers[i]);
                    if (ImGui.IsItemClicked())
                    {
                        SelectedAnswer = Answers[i];
                        Dismiss();
                        return;
                    }
                }
            }
        }
    }
}
