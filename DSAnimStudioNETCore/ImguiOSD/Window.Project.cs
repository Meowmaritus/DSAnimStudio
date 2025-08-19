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
        public class Project : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "Project";

            public DSAProj Proj;

            public bool RequestShowTagsSection = false;
            private bool RequestShowTagsSection_ScrollNextFrame = false;
            private bool RequestShowTagsSection_OpenTreeNextFrame = false;

            protected override void Init()
            {
                
            }

            private string[] helpText = null;
            private object _lock_helpText = new object();

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                bool isRequestShowTagsSectionThisFrame = RequestShowTagsSection;
                RequestShowTagsSection = false;

                bool isRequestShowTagsSectionThisFrame_DoScroll = RequestShowTagsSection_ScrollNextFrame;
                RequestShowTagsSection_ScrollNextFrame = false;

                bool isRequestShowTagsSectionThisFrame_OpenTree = RequestShowTagsSection_OpenTreeNextFrame;
                RequestShowTagsSection_OpenTreeNextFrame = false;

                Proj = zzz_DocumentManager.CurrentDocument.EditorScreen.Proj;

                if (Proj == null)
                    return;

                ImGui.Text($"Game Type: {Proj.GameType}");
                ImGui.Text($".DSAPROJ Format Version: {Proj.FILE_VERSION}");

                //testing
                //ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 500);
                //ImGui.Text("TEST");

                

                if (isRequestShowTagsSectionThisFrame)
                {
                    RequestShowTagsSection_OpenTreeNextFrame = true;
                    RequestShowTagsSection_ScrollNextFrame = false;
                    
                    
                }
                if (isRequestShowTagsSectionThisFrame_DoScroll)
                {
                    ImGui.SetScrollHereY();
                    RequestShowTagsSection_OpenTreeNextFrame = false;
                    RequestShowTagsSection_ScrollNextFrame = false;
                }
                if (isRequestShowTagsSectionThisFrame_OpenTree)
                {
                    ImGui.SetNextItemOpen(true);
                    RequestShowTagsSection_OpenTreeNextFrame = false;
                    RequestShowTagsSection_ScrollNextFrame = true;
                }

                if (ImGui.TreeNode("Tags##Proj_TagList_TreeNode"))
                {




                    lock (Proj._lock_Tags)
                    {
                        foreach (var t in Proj.Tags)
                        {
                            ImGui.ColorButton($"{t.Name}##TagColorButton_{t.GUID}",
                                (t.CustomColor ?? Main.Colors.ColorProjectTagDefault).ToNVector4());
                            anyFieldFocused |= ImGui.IsItemActive();
                            ImGui.SameLine();
                            var tagNodeOpen = ImGui.TreeNode($"{t.Name}##Proj_TagList_Tag_{t.GUID}");
                            //ImGui.SameLine();

                            if (tagNodeOpen)
                            {
                                ImGui.Button("Rename Tag...");
                                anyFieldFocused |= ImGui.IsItemActive();
                                if (ImGui.IsItemClicked())
                                {
                                    ImguiOSD.DialogManager.AskForInputString("Change Tag Name", "Enter new name for tag." +
                                        $"\nCurrent tag name: '{t.Name}'." +
                                        "\n\nThis will change the name of this tag in all locations it is used.", "", result =>
                                    {
                                        lock (Proj._lock_Tags)
                                        {
                                            result = result.Trim();
                                            t.Name = result;
                                        }
                                    }, errCheck =>
                                    {
                                        string result = null;
                                        lock (Proj._lock_Tags)
                                        {
                                            errCheck = errCheck.Trim();
                                            if (Proj.Tags.Any(tag => tag.Name == errCheck && tag.GUID != t.GUID))
                                                result = $"Another tag named '{errCheck}' already exists.";
                                        }
                                        return result;
                                    }, canBeCancelled: true);
                                }
                                ImguiOSD.Tools.CustomColorPicker("Custom Tag Color", $"TagCustomColor_{t.GUID}", ref t.CustomColor, Main.Colors.ColorProjectTagDefault);
                                ImGui.Button("Delete Tag...");
                                anyFieldFocused |= ImGui.IsItemActive();
                                if (ImGui.IsItemClicked())
                                {
                                    var tagToDelete = t;
                                    DialogManager.AskYesNo("Delete Tag",
                                        "Delete this tag from the project? All instances of it being used throughout the project will be lost." +
                                        "\n\nThis is not undoable.", choice =>
                                        {
                                            lock (Proj._lock_Tags)
                                            {
                                                Proj.Tags.Remove(tagToDelete);
                                            }
                                        },
                                        allowCancel: true, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                                }

                                ImGui.TreePop();
                            }

                        }

                        ImGui.Separator();
                        ImGui.Button("Add New Tag...");
                        anyFieldFocused |= ImGui.IsItemActive();
                        if (ImGui.IsItemClicked())
                        {
                            ImguiOSD.DialogManager.AskForInputString("Add New Tag", "Enter name for new tag.", "", result =>
                            {
                                lock (Proj._lock_Tags)
                                {
                                    result = result.Trim();
                                    var resultTag = Proj.Tags.FirstOrDefault(t => t.Name == result);
                                    if (resultTag == null)
                                    {
                                        resultTag = new DSAProj.Tag(result);
                                        Proj.Tags.Add(resultTag);
                                    }
                                }
                            }, errCheck =>
                            {
                                string result = null;
                                lock (Proj._lock_Tags)
                                {
                                    errCheck = errCheck.Trim();
                                    if (Proj.Tags.Any(tag => tag.Name == errCheck))                                    
                                        result = $"Tag named '{errCheck}' already exists.";
                                }
                                return result;
                            }, canBeCancelled: true);
                        }
                        ImGui.TreePop();
                    }




                }

            }
        }
    }
}
