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
        public class TextInput : Dialog
        {
            public string Question;
            public string TextHint;
            public string InputtedString = string.Empty;

            ImGuiInputTextCallback TextCallbackDelegate;

            public unsafe TextInput()
            {
                TextCallbackDelegate = (data) =>
                {
                    InputtedString = new string((sbyte*)(data->Buf));
                    return 0;
                };
            }

            protected override void BuildInsideOfWindow()
            {
                //ImGui.PushTextWrapPos(ImGui.GetFontSize() * 400);
                //ImGui.TextWrapped(Question);
                //ImGui.PopTextWrapPos();
                ImGui.SetKeyboardFocusHere();

                bool accepted = ImGui.InputTextWithHint(Question, TextHint, ref InputtedString,
                    256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways | ImGuiInputTextFlags.AutoSelectAll,
                    TextCallbackDelegate);
                if (accepted)
                {
                    Dismiss();
                }
            }
        }
    }
}
