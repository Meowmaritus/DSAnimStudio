using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static partial class OSD
    {
        public static class Tools
        {
            private static List<DialogPrompThing> yesNoPromptList = new List<DialogPrompThing>();

            private interface IDialogPromptThing
            {

            }

            private abstract class DialogPrompThing
            {
                public string Title;
                public bool CanBeCancelled = false;
                public bool DoesEscapeCancel = true;
                public bool WasCancelled { get; private set; } = false;
                public bool IsDismissed { get; private set; }
                protected void Dismiss()
                {
                    IsDismissed = true;
                    Task.Run(OnDismiss);
                    //OnDismiss?.Invoke();
                    
                }

                public Action OnDismiss;

                protected abstract void BuildInsideOfWindow();

                public void Update()
                {
                    ImGui.OpenPopup(Title);
                    bool isOpen = true;

                    bool didPopupWindow;
                    if (CanBeCancelled)
                    {
                        didPopupWindow = ImGui.BeginPopupModal(Title, ref isOpen,
                            ImGuiWindowFlags.AlwaysAutoResize |
                            ImGuiWindowFlags.NoSavedSettings |
                            ImGuiWindowFlags.NoCollapse);
                    }
                    else
                    {
                        didPopupWindow = ImGuiEx.BeginPopupModal(Title,
                               ImGuiWindowFlags.AlwaysAutoResize |
                               ImGuiWindowFlags.NoSavedSettings |
                               ImGuiWindowFlags.NoCollapse);
                    }

                    if (didPopupWindow)
                    {
                        BuildInsideOfWindow();



                        ImGui.EndPopup();
                    }

                    if (CanBeCancelled && (!isOpen || (DoesEscapeCancel && Main.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))))
                    {
                        WasCancelled = true;
                        Dismiss();
                    }
                }
            }

            private class MultiChoiceDialogThing : DialogPrompThing
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

            private class TextStringDialogThing : DialogPrompThing
            {
                public string Question;
                public string TextHint;
                public string InputtedString = string.Empty;

                ImGuiInputTextCallback TextCallbackDelegate;

                public unsafe TextStringDialogThing()
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

                    bool accepted = ImGui.InputTextWithHint(Question, TextHint, InputtedString, 
                        256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways, 
                        TextCallbackDelegate);
                    if (accepted)
                    {
                        Dismiss();
                    }
                }
            }

            public static void ToolsUpdateLoop()
            {
                List<DialogPrompThing> promptsThatHaveFinished = new List<DialogPrompThing>();
                foreach (var q in yesNoPromptList)
                {
                    if (!q.IsDismissed)
                        q.Update();
                    if (q.IsDismissed)
                        promptsThatHaveFinished.Add(q);
                }
                foreach (var q in promptsThatHaveFinished)
                {
                    yesNoPromptList.Remove(q);
                }
            }

            public static void DialogOK(string title, string text)
            {
                AskForMultiChoice(title, text, null, true, "OK");
            }

            public static void AskForMultiChoice(string title, string question, 
                Action<string> onAnswer, bool canBeCancelled, params string[] answers)
            {
                var dlg = new MultiChoiceDialogThing()
                {
                    CanBeCancelled = canBeCancelled,
                    Answers = answers.ToList(),
                    Question = question,
                    Title = title,
                };
                dlg.OnDismiss += () => onAnswer?.Invoke(dlg.SelectedAnswer);
                yesNoPromptList.Add(dlg);
            }

            public static void AskForInputString(string title, string question, string textHint, 
                Action<string> onResult, bool canBeCancelled)
            {
                var dlg = new TextStringDialogThing()
                {
                    CanBeCancelled = canBeCancelled,
                    TextHint = textHint,
                    Question = question,
                    Title = title,
                };

                dlg.OnDismiss += () =>
                {
                    if (!dlg.WasCancelled)
                        onResult?.Invoke(dlg.InputtedString);
                };

                yesNoPromptList.Add(dlg);
            }

            public static void Notice(string text)
            {
                lock (_lock_messageBoxes)
                    messageBoxTexts.Add(text);
                if (messageBoxIndex < 0)
                    messageBoxIndex = 0;
            }

            // Would be very hard to implement lol
            //public static bool AskYesNo(string question)
            //{

            //}
        }
    }
}
