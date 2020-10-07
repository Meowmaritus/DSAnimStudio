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
            private static List<YesNoPrompt> yesNoPromptList = new List<YesNoPrompt>();

            private class YesNoPrompt
            {
                public string Title;
                public string Question;
                public List<string> Answers;
                public Action<string> OnAnswer;
                public bool HasAnswered = false;
                public void Update()
                {
                    ImGui.OpenPopup(Title);
                    bool isOpen = true;
                    if (ImGui.BeginPopupModal(Title, ref isOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings))
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
                                OnAnswer?.Invoke(Answers[i]);
                                HasAnswered = true;
                                return;
                            }
                        }

                        
                        ImGui.EndPopup();
                    }
                }
            }

            public static void ToolsUpdateLoop()
            {
                List<YesNoPrompt> promptsThatHaveFinished = new List<YesNoPrompt>();
                foreach (var q in yesNoPromptList)
                {
                    q.Update();
                    if (q.HasAnswered)
                    {
                        promptsThatHaveFinished.Add(q);
                    }
                }
                foreach (var q in promptsThatHaveFinished)
                {
                    yesNoPromptList.Remove(q);
                }
            }

            public static void AskQuestion(string title, string question, Action<string> onAnswer, params string[] answers)
            {
                yesNoPromptList.Add(new YesNoPrompt()
                {
                    Answers = answers.ToList(),
                    OnAnswer = onAnswer,
                    Question = question,
                    Title = title,
                });
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
