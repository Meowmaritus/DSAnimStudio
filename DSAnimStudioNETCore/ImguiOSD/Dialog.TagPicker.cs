using DSAnimStudio.DbgMenus;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class TagPicker : Dialog
        {
            public DSAProj.Tag ChosenTag = null;

            //public Func<string, string> CheckErrorFunc;
            private string InputtedString = string.Empty;
            //public string CurrentError = null;

            private bool RefreshTextFocus = true;

            ImGuiInputTextCallback TextCallbackDelegate;

            private readonly DSAProj Proj;

            private readonly Dictionary<string, DSAProj.Tag> AvailableTags = new();
            private readonly Dictionary<string, int> AvailableTags_MatchScores = new();

            private string[] FilteredTags = new string[0];
            private int FilteredTags_SelectedIndex = -1;
            public bool FilteredTagsIncludesAddNew = false;
            public bool NothingInputted = false;

            DbgMenuPadRepeater RepeaterUp = new DbgMenuPadRepeater(Microsoft.Xna.Framework.Input.Buttons.A, 0.4f, 0.016666667f);
            DbgMenuPadRepeater RepeaterDown = new DbgMenuPadRepeater(Microsoft.Xna.Framework.Input.Buttons.A, 0.4f, 0.016666667f);

            public unsafe TagPicker(string title, DSAProj proj)
                : base(title)
            {
                Proj = proj;
                var tagDict = new Dictionary<string, DSAProj.Tag>();
                lock (Proj._lock_Tags)
                {
                    foreach (var t in Proj.Tags)
                    {
                        tagDict[t.Name] = t;
                    }
                }
                AvailableTags = tagDict;
                TextCallbackDelegate = (data) =>
                {
                    InputtedString = new string((sbyte*)(data->Buf));
                    return 0;
                };

                AcceptHandledByInheritor = true;
                AllowsResultAccept = true;
                AllowsResultCancel = true;
                CancelHandledByInheritor = false;

                UpdateTagSearch();
            }

            private void UpdateTagSearch()
            {
                FilteredTagsIncludesAddNew = true;
                NothingInputted = string.IsNullOrWhiteSpace(InputtedString);

                
                string query = InputtedString;
                string queryLower = InputtedString.ToLower();
                // Descending because higher numbers are at the top of the list (it all gets reversed later on).
                var keys = AvailableTags.Keys.OrderByDescending(a => a).ToList();

                bool hasExactMatch = false;

                for (int i = 0; i < keys.Count; i++)
                {
                    var key = keys[i];
                    var keyLower = key.ToLower();

                    // Perfect match
                    if (key == query)
                    {
                        hasExactMatch = true;
                        FilteredTagsIncludesAddNew = false;
                        AvailableTags_MatchScores[key] = 8000000 + i;
                    }
                    else if (key.Contains(query))
                        AvailableTags_MatchScores[key] = 4000000 + i;
                    else if (keyLower == queryLower)
                        AvailableTags_MatchScores[key] = 2000000 + i;
                    else if (keyLower.Contains(queryLower))
                        AvailableTags_MatchScores[key] = 1000000 + i;
                    else
                        AvailableTags_MatchScores[key] = -1;
                }
                var tagsList = keys.Where(k => AvailableTags_MatchScores[k] >= 0).OrderByDescending(k => AvailableTags_MatchScores[k]).ToList();

                if (!hasExactMatch)
                    tagsList.Add($"[Add new tag: \"{InputtedString}\"]");

                FilteredTags = tagsList.ToArray();

                if (FilteredTags.Length > 0)
                    FilteredTags_SelectedIndex = 0;
                else
                    FilteredTags_SelectedIndex = -1;


                if (NothingInputted)
                {
                    FilteredTags_SelectedIndex = -1;
                }


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
                
                bool accepted = ImGui.InputTextWithHint($"Search Tag Name##{GUID}_TextInput", "", ref InputtedString,
                    256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackAlways,
                    TextCallbackDelegate);






                if (oldText != InputtedString)
                {
                    UpdateTagSearch();
                }

                ImGui.ListBox($"Filtered Tags List##{GUID}_FilteredTagList", ref FilteredTags_SelectedIndex, FilteredTags, FilteredTags.Length);

                RepeaterUp.Update(GamePadState.Default, Main.DELTA_UPDATE, DialogManager.Input.KeyHeld(Keys.Up));
                if (RepeaterUp.State)
                {
                    FilteredTags_SelectedIndex--;
                    if (FilteredTags_SelectedIndex < 0)
                        FilteredTags_SelectedIndex = FilteredTags.Length - 1;
                }
                RepeaterDown.Update(GamePadState.Default, Main.DELTA_UPDATE, DialogManager.Input.KeyHeld(Keys.Down));
                if (RepeaterDown.State)
                {
                    FilteredTags_SelectedIndex++;
                    if (FilteredTags_SelectedIndex >= FilteredTags.Length)
                        FilteredTags_SelectedIndex = 0;
                }

                ChosenTag = null;

                if (FilteredTagsIncludesAddNew && FilteredTags_SelectedIndex == FilteredTags.Length - 1)
                {
                    ChosenTag = new DSAProj.Tag(InputtedString, null);
                }
                else
                {
                    if (FilteredTags_SelectedIndex >= 0 && AvailableTags.ContainsKey(FilteredTags[FilteredTags_SelectedIndex]))
                    {
                        ChosenTag = AvailableTags[FilteredTags[FilteredTags_SelectedIndex]];
                    }
                }



                //if (CurrentError != null)
                //{
                //    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
                //    ImGui.Text(CurrentError);
                //    ImGui.PopStyleColor();
                //}


                var noTagChosen = ChosenTag == null;

                if (noTagChosen)
                    ImGui.BeginDisabled();
                ImGui.Button("Add Selected Tag");
                if (noTagChosen)
                    ImGui.EndDisabled();

                if (ImGui.IsItemClicked())
                {
                    accepted = true;
                }

                if (DialogManager.Input.KeyDown(Keys.Enter))
                    accepted = true;

                if (accepted)
                {
                    RefreshTextFocus = true;

                    if (FilteredTags_SelectedIndex >= 0 && ChosenTag != null)
                    {
                        if (ChosenTag != null)
                        {
                            ResultType = ResultTypes.Accept;
                            Dismiss();
                        }
                        //if (CurrentError == null)
                        //{
                        //    ResultType = ResultTypes.Accept;
                        //    Dismiss();
                        //}
                        //else
                        //    RefreshTextFocus = true;
                    }
                }


            }
        }
    }
}
