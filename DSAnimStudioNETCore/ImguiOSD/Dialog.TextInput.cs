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
            public Func<string, string> CheckErrorFunc;
            
            public string Question;
            public string TextHint;
            public string InputtedString = string.Empty;
            public string CurrentError = null;

            public bool RefreshTextFocus = true;

            ImGuiInputTextCallback TextCallbackDelegate;

            public unsafe TextInput(string title)
                : base(title)
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

                EnterKeyToAccept = false;

                if (RefreshTextFocus)
                {
                    ImGui.SetKeyboardFocusHere();
                    RefreshTextFocus = false;
                }

                var oldText = InputtedString;
                
                bool accepted = ImGui.InputTextWithHint(Question, TextHint, ref InputtedString,
                    256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways,
                    TextCallbackDelegate);

                if (oldText != InputtedString)
                {
                    CurrentError = CheckErrorFunc?.Invoke(InputtedString);
                }

                if (CurrentError != null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
                    ImGui.Text(CurrentError);
                    ImGui.PopStyleColor();
                }
                
                
                if (accepted)
                {
                    if (CurrentError == null)
                    {
                        ResultType = ResultTypes.Accept;
                        Dismiss();
                    }
                    else
                        RefreshTextFocus = true;
                }
            }
        }
    }
}
