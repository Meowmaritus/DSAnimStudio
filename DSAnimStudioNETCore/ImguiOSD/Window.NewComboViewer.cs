using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class NewComboViewer : Window
        {
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            //public readonly DSAProj Proj;
            //public NewComboViewer(DSAProj proj)
            //{
            //    Proj = proj;
            //}

            public override string NewImguiWindowTitle => "Combo Viewer";

            private List<NewCombo.Entry> entries = new List<NewCombo.Entry>();
            
            protected override void Init()
            {
                IsOpen = true;
                Flags = ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs;
            }

            protected override void PreUpdate()
            {
                //IsOpen = true;
                if (MainProj == null)
                    IsOpen = false;
            }

            protected override void PostUpdate()
            {
                
            }

            protected override void BuildContents(ref bool anyFieldFocused)
            {
                ImGui.PushID(GUID);
                if (ImGui.BeginTable($"##NewComboViewerTable", 5,
                        ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable |
                        ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.ScrollY))
                {
                    ImGui.TableSetupColumn("Anim", ImGuiTableColumnFlags.None, 5);
                    ImGui.TableSetupColumn("Loop", ImGuiTableColumnFlags.None, 1);
                    ImGui.TableSetupColumn("Start", ImGuiTableColumnFlags.None, 4);
                    ImGui.TableSetupColumn("End", ImGuiTableColumnFlags.None, 4);
                    ImGui.TableSetupColumn("Blend", ImGuiTableColumnFlags.None, 4);
                        
                    ImGui.TableSetupScrollFreeze(5, 1);
    
                    ImGui.TableHeadersRow();
                }
                else
                {
                    return;
                }


                NewCombo.Entry entryToDelete = null;


                for (int i = 0; i < entries.Count; i++)
                {
                    if (i > 0)
                        ImGui.TableNextRow();

                    var entry = entries[i];
                    ImGui.TableNextColumn();
                    if (Tools.SimpleClickButton("X"))
                    {
                        entryToDelete = entry;
                    }
                    ImGui.SameLine();
                    ImGui.PushItemWidth(80);
                    ImGui.InputInt($"##ComboEntry[{i}]_Category", ref entry.AnimationID.CategoryID, 0, 0);
                    ImGui.PopItemWidth();
                    ImGui.SameLine();
                    ImGui.InputInt($"##ComboEntry[{i}]_AnimSubID", ref entry.AnimationID.SubID, 0, 0);

                    ImGui.TableNextColumn();

                    ImGui.Checkbox($"##ComboEntry[{i}]_Loop", ref entry.IsLoop);

                    ImGui.TableNextColumn();

                    ImGui.InputFloat($"##ComboEntry[{i}]_Start", ref entry.StartFrame, 0, 0);
                    
                    ImGui.TableNextColumn();

                    ImGui.InputFloat($"##ComboEntry[{i}]_End", ref entry.EndFrame, 0, 0);
                    
                    ImGui.TableNextColumn();

                    ImGui.InputFloat($"##ComboEntry[{i}]_Blend", ref entry.BlendDuration, 0, 0);
                }
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (Tools.SimpleClickButton("Add New"))
                { 
                    entries.Add(new NewCombo.Entry());   
                }
                
                if (Tools.SimpleClickButton("Play Combo"))
                { 
                    MainTaeScreen.Graph?.ViewportInteractor?.RequestCombo(entries);
                }
                
                if (Tools.SimpleClickButton("Copy combo to clipboard"))
                {
                    var sb = new StringBuilder();
                    foreach (var e in entries)
                    {
                        sb.AppendLine($"{e.AnimationID.GetFormattedIDString(MainProj)} {e.IsLoop} {e.StartFrame} {e.EndFrame} {e.BlendDuration}");
                    }
                    Clipboard.SetText(sb.ToString());
                }
                
                if (Tools.SimpleClickButton("Paste combo from clipboard"))
                {
                    var newEntries = new List<NewCombo.Entry>();
                    var lines = Clipboard.GetText().Replace("\r", "").Split("\n");
                    foreach (var ln in lines)
                    {
                        var splitLine = ln.Split(' ').Select(x => x.Trim()).ToList();
                        if (SplitAnimID.TryParse(MainProj, splitLine[0], out SplitAnimID animID, out string detailedError))
                        {
                            if (bool.TryParse(splitLine[1], out bool isLoop))
                            {
                                if (float.TryParse(splitLine[2], out float startFrame))
                                {
                                    if (float.TryParse(splitLine[3], out float endFrame))
                                    {
                                        if (float.TryParse(splitLine[4], out float blendDuration))
                                        {
                                            newEntries.Add(new NewCombo.Entry()
                                            {
                                                AnimationID = animID,
                                                IsLoop = isLoop,
                                                StartFrame = startFrame,
                                                EndFrame = endFrame,
                                                BlendDuration = blendDuration,
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        
                    }

                    if (newEntries.Count > 0)
                    {
                        entries = newEntries;
                    }
                }
                
                ImGui.EndTable();
                
                if (entryToDelete != null)
                {
                    entries.Remove(entryToDelete);
                }

                

                
                
                
                ImGui.PopID();
            }
        }
    }
}
