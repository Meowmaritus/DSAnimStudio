using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using ImGuiNET;

namespace DSAnimStudio
{
    public partial class DSAProj
    {
        public class ErrorContainerClass
        {
            public readonly string GUID = Guid.NewGuid().ToString();
            private List<ErrorState> Errors = new List<ErrorState>();
            private StringBuilder TooltipStringBuilder = new StringBuilder();
            private string TooltipString = "";

            private bool RightClickMenuOpen = false;
            
            private object _lock = new object();
            public void AddError(ErrorState error)
            {
                lock (_lock)
                {
                    if (!Errors.Contains(error))
                        Errors.Add(error);
                    RebuildTooltipString();
                }
            }

            public void RemoveError(ErrorState error)
            {
                lock (_lock)
                {
                    if (Errors.Contains(error))
                        Errors.Remove(error);
                    RebuildTooltipString();
                }
            }

            public void ClearErrors()
            {
                lock (_lock)
                {
                    Errors.Clear();
                    RebuildTooltipString();
                }
            }

            public bool AnyDuplicateIDErrors(SplitAnimID id)
            {
                bool result = false;
                lock (_lock)
                {
                    foreach (var error in Errors)
                    {
                        if (error is ErrorState.DuplicateAnimID asDupeAnimID)
                        {
                            if (asDupeAnimID.AnimID == id)
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
                return result;
            }

            public bool AnyErrors()
            {
                bool result = true;
                lock (_lock)
                {
                    result = Errors.Count > 0;
                }

                return result;
            }

            private void RebuildTooltipString()
            {
                lock (_lock)
                {
                    TooltipStringBuilder.Clear();
                    TooltipStringBuilder.AppendLine($"Found {Errors.Count} error{(Errors.Count != 1 ? "s" : "")}:");
                    foreach (var er in Errors)
                    {
                        TooltipStringBuilder.AppendLine($"    -{er.ShortDescription}");
                    }

                    TooltipStringBuilder.AppendLine("\nRight click for a shortcut to the error window.");

                    TooltipString = TooltipStringBuilder.ToString();
                }
            }

            public void CreateJumpToErrorImguiControls(TaeEditorScreen mainScreen)
            {
                lock (_lock)
                {
                    //ImGui.PushClipRect(OSD.SpWindowERRORS.LastPosition, OSD.SpWindowERRORS.LastSize, false);
                    //ImGui.PushTextWrapPos(100);
                    int errorIndex = 0;
                    foreach (var err in Errors)
                    {
                        var selected = mainScreen.SelectedAnimCategory == err.SourceAnimCategory &&
                                       mainScreen.SelectedAnim == err.SourceAnimation;
                        var prevSelected = selected;
                        
                        float padding = 8;
                        
                        float textWrapX = (float)Math.Round(OSD.SpWindowERRORS.LastSize.X - 16 - (padding * 2));
                        
                        
                        
                        var text =
                            $"[Category {err.SourceAnimCategory.CategoryID} -> Anim {err.SourceAnimation.SplitID.GetFormattedIDString(mainScreen.Proj)}] {err.ShortDescription}";

                         
                        
                        //var selectableTextMeasure = ImGui.CalcTextSize(text, textWrapX);



                        

                        var cursorPos = ImGui.GetCursorPos();
                        
                        
                        ImGui.SetCursorPos(cursorPos + new Vector2(padding, padding));
                        ImGui.PushTextWrapPos(textWrapX);
                        ImGui.PushStyleColor(ImGuiCol.Text, Vector4.Zero);
                        //ImGui.PushStyleVar(ImGuiStyleVar., new Vector2(32, 32));
                        var cursorPosBeforeText = ImGui.GetCursorPos();
                        ImGui.TextWrapped(text);
                        var cursorPosAfterText = ImGui.GetCursorPos();
                        //ImGui.PopStyleVar();
                        ImGui.PopStyleColor();
                        ImGui.PopTextWrapPos();
                        
                        var cursorPosAfter = ImGui.GetCursorPos();
                        
                        var selectableSize = (cursorPosAfterText - cursorPosBeforeText) + new Vector2(padding * 2, padding * 2);

                        selectableSize.X = 0;
                        
                        ImGui.SetCursorPos(cursorPos);
                        
                        ImGui.Selectable($"##AnimContainer_{GUID}_JumpToError_{errorIndex}"
                            ,
                            ref selected, ImGuiSelectableFlags.SpanAllColumns, selectableSize);
                        if (selected && !prevSelected)
                            mainScreen.SelectNewAnimRef(err.SourceAnimCategory, err.SourceAnimation, isPushCurrentToHistBackwardStack: true);
                        
                        ImGui.SetCursorPos(cursorPos + new Vector2(padding, padding));
                        ImGui.PushTextWrapPos(textWrapX);
                        //ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(32, 32));
                        ImGui.TextWrapped(text);
                        //ImGui.PopStyleVar();
                        ImGui.PopTextWrapPos();

                        
                        ImGui.SetCursorPos(cursorPosAfter + new Vector2(0, padding * 2));
                        
                        //ImGuiDebugDrawer.DrawText(text);
                        
                        errorIndex++;
                    }
                    //ImGui.PopTextWrapPos();
                    //ImGui.PopClipRect();
                }
            }
            
            public void UpdateAndBuildImGui(bool isMouseHovering, FancyInputHandler input)
            {
                if (!AnyErrors())
                    return;

                lock (_lock)
                {
                    if (string.IsNullOrWhiteSpace(TooltipString))
                        RebuildTooltipString();
                    if (isMouseHovering)
                    {
                        if (!input.AnyMouseButtonHeld)
                            OSD.TooltipManager_AnimationList.DoTooltipManual($"ErrorContainer{GUID}",
                                TooltipString ?? "Invalid error info. If you see this, tell Meow about it.",
                                !RightClickMenuOpen);

                        if (input.RightClickUp)
                        {
                            RightClickMenuOpen = true;
                            ImGui.OpenPopup($"Popup{GUID}");
                        }
                    }
                    else
                    {
                        RightClickMenuOpen = false;
                        // close popup?
                    }
                    
                    if (ImGui.BeginPopup($"Popup{GUID}"))
                    {
                        OSD.SetAuxFocus();
                        RightClickMenuOpen = true;
                        //foreach (var er in Errors)
                        //{
                        //    er.AddImguiRightClickEntry();
                        //}

                        ImGui.MenuItem($"Open ERRORS panel");

                        if (ImGui.IsItemClicked())
                        {
                            OSD.SpWindowERRORS.IsOpen = true;
                            OSD.SpWindowERRORS.IsRequestFocus = true;
                        }


                        ImGui.EndPopup();
                    }
                    
                    RightClickMenuOpen = ImGui.IsPopupOpen($"Popup{GUID}");
                    if (RightClickMenuOpen)
                        OSD.TooltipManager_AnimationList.CancelTooltip();
                }
            }
            
            
            
        }
    }
}
