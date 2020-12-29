using ImGuiNET;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Dialog
    {
        public class TaeAnimPropertiesEdit : Dialog
        {
            string GetAnimIDString(long id)
            {
                if (id < 0)
                    return "";

                var animID_Lower = GameDataManager.GameTypeHasLongAnimIDs
                ? (id % 1_000000) : (id % 1_0000);

                var animID_Upper = GameDataManager.GameTypeHasLongAnimIDs
                    ? (id / 1_000000) : (id / 1_0000);

                string curIDText;
                if (GameDataManager.GameTypeHasLongAnimIDs)
                {
                    bool ds2Meme = GameDataManager.CurrentAnimIDFormatType == GameDataManager.AnimIDFormattingType.aXX_YY_ZZZZ;
                    if (ds2Meme)
                    {
                        curIDText = $"a{(animID_Upper):D2}_{animID_Lower:D6}";
                        curIDText = curIDText.Insert(curIDText.Length - 4, "_");
                    }
                    else
                    {
                        curIDText = $"a{(animID_Upper):D3}_{animID_Lower:D6}";
                    }
                }
                else
                {
                    curIDText = $"a{(animID_Upper):D2}_{animID_Lower:D4}";
                }

                return curIDText;
            }

            public string TaeAnimID_String;
            public long? TaeAnimID_Value;

            public string ImportFromAnimID_String;
            public int? ImportFromAnimID_Value;

            public string TaeAnimName;
            public TAE.Animation.AnimMiniHeader TaeAnimHeader;
            public bool WasAnimDeleted = false;

            public TaeAnimPropertiesEdit(TAE.Animation anim)
            {
                Title = "Edit Animation Properties";

                TaeAnimID_String = GetAnimIDString(anim.ID);
                TaeAnimID_Value = anim.ID;

                TaeAnimName = anim.AnimFileName;
                if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
                {
                    TaeAnimHeader = new TAE.Animation.AnimMiniHeader.ImportOtherAnim()
                    {
                        ImportFromAnimID = asImportOtherAnim.ImportFromAnimID,
                        Unknown = asImportOtherAnim.Unknown,
                    };
                    ImportFromAnimID_String = GetAnimIDString(asImportOtherAnim.ImportFromAnimID);
                    ImportFromAnimID_Value = asImportOtherAnim.ImportFromAnimID;
                }
                else if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
                {
                    TaeAnimHeader = new TAE.Animation.AnimMiniHeader.Standard()
                    {
                        AllowDelayLoad = asStandard.AllowDelayLoad,
                        ImportHKXSourceAnimID = asStandard.ImportHKXSourceAnimID,
                        ImportsHKX = asStandard.ImportsHKX,
                        IsLoopByDefault = asStandard.IsLoopByDefault,
                    };
                    ImportFromAnimID_String = GetAnimIDString(asStandard.ImportHKXSourceAnimID);
                    ImportFromAnimID_Value = asStandard.ImportHKXSourceAnimID;
                }
            }

            protected override void BuildInsideOfWindow()
            {
                bool isCurrentlyStandard = TaeAnimHeader.Type == TAE.Animation.MiniHeaderType.Standard;
                bool isCurrentlyImportOther = TaeAnimHeader.Type == TAE.Animation.MiniHeaderType.ImportOtherAnim;

                if (TaeAnimID_Value == null)
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
                ImGui.InputText("Animation ID", ref TaeAnimID_String, 256);
                if (TaeAnimID_Value == null)
                    ImGui.PopStyleColor();
                if (string.IsNullOrWhiteSpace(TaeAnimID_String))
                {
                    TaeAnimID_Value = null;
                }
                else if (long.TryParse(TaeAnimID_String.Replace("a", "").Replace("A", "").Replace("_", ""), out long animIdParsed))
                {
                    TaeAnimID_Value = animIdParsed;
                }
                else
                {
                    TaeAnimID_Value = null;
                }

                ImGui.Separator();

                ImGui.InputText("Animation Name", ref TaeAnimName, 256);

                ImGui.Separator();

                ImGui.Text("Animation Type:");

                ImGui.Indent();
                {
                    ImGui.MenuItem("Standard Animation", "", isCurrentlyStandard);
                    bool clickedStandard = ImGui.IsItemClicked();

                    ImGui.MenuItem("Duplicate of Other Anim Entry", "", TaeAnimHeader.Type == TAE.Animation.MiniHeaderType.ImportOtherAnim);
                    bool clickedImportOther = ImGui.IsItemClicked();

                    if (clickedStandard && !isCurrentlyStandard)
                    {
                        DialogManager.AskForMultiChoice("Change Animation Type",
                            "Change animation type, losing the values you entered?", 
                            (cancelType, answer) =>
                            {
                                if (cancelType != CancelTypes.None)
                                    return;

                                if (answer == "YES")
                                {
                                    TaeAnimHeader = new TAE.Animation.AnimMiniHeader.Standard();
                                }
                            }, CancelTypes.Combo_ClickTitleBarX_PressEscape, "YES", "NO");
                    }
                    else if (clickedImportOther && !isCurrentlyImportOther)
                    {
                        DialogManager.AskForMultiChoice("Change Animation Type",
                            "Change animation type, losing the values you entered?", 
                            (cancelType, answer) =>
                            {
                                if (cancelType != CancelTypes.None)
                                    return;

                                if (answer == "YES")
                                {
                                    TaeAnimHeader = new TAE.Animation.AnimMiniHeader.ImportOtherAnim();
                                }
                            }, CancelTypes.Combo_ClickTitleBarX_PressEscape, "YES", "NO");
                    }
                }
                ImGui.Unindent();
                




                if (TaeAnimHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
                {
                    ImGui.Text("Properties - Duplicate of Other Anim Entry:");
                    ImGui.Indent();
                    {
                        if (ImportFromAnimID_Value == null)
                            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
                        ImGui.InputText("Duplicate of Animation ID", ref ImportFromAnimID_String, 256);
                        if (ImportFromAnimID_Value == null)
                            ImGui.PopStyleColor();
                        if (string.IsNullOrWhiteSpace(ImportFromAnimID_String))
                        {
                            ImportFromAnimID_Value = asImportOtherAnim.ImportFromAnimID = -1;
                        }
                        else if (int.TryParse(ImportFromAnimID_String.Replace("a", "").Replace("A", "").Replace("_", ""), out int importFromIdParsed))
                        {
                            ImportFromAnimID_Value = asImportOtherAnim.ImportFromAnimID = importFromIdParsed;
                        }
                        else
                        {
                            ImportFromAnimID_Value = null;
                        }

                        asImportOtherAnim.Unknown = MenuBar.IntItem("Unknown Value", asImportOtherAnim.Unknown);
                    }
                    ImGui.Unindent();
                }
                else if (TaeAnimHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
                {
                    ImGui.Text("Properties - Standard Animation:");
                    ImGui.Indent();
                    {
                        asStandard.ImportsHKX = MenuBar.CheckboxBig("Import Other HKX", asStandard.ImportsHKX);

                        if (ImportFromAnimID_Value == null)
                            ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 0, 0, 1));
                        ImGui.InputText("Import HKX ID", ref ImportFromAnimID_String, 256);
                        if (ImportFromAnimID_Value == null)
                            ImGui.PopStyleColor();
                        if (string.IsNullOrWhiteSpace(ImportFromAnimID_String))
                        {
                            ImportFromAnimID_Value = asStandard.ImportHKXSourceAnimID = -1;
                        }
                        else if (int.TryParse(ImportFromAnimID_String.Replace("a", "").Replace("A", "").Replace("_", ""), out int importFromIdParsed))
                        {
                            ImportFromAnimID_Value = asStandard.ImportHKXSourceAnimID = importFromIdParsed;
                        }
                        else
                        {
                            ImportFromAnimID_Value = null;
                        }

                        asStandard.AllowDelayLoad = MenuBar.CheckboxBig("Allow loading from DelayLoad ANIBNDs", asStandard.AllowDelayLoad);
                        asStandard.IsLoopByDefault = MenuBar.CheckboxBig("Enable Looping", asStandard.IsLoopByDefault);
                    }
                    ImGui.Unindent();
                }

                ImGui.Separator();
                ImGui.Button("Delete This Animation...");
                if (ImGui.IsItemClicked())
                {
                    DialogManager.AskForMultiChoice("Permanently Delete Animation Entry?", $"Are you sure you want to delete the current animation?\nThis can NOT be undone!", (cancelType, answer) =>
                    {
                        if (answer == "YES")
                        {
                            WasAnimDeleted = true;
                            CancelType = CancelTypes.ClickedAcceptButton;
                            Dismiss();
                            DialogManager.DialogOK("Success", "Animation deleted successfully.");
                        }
                    }, CancelTypes.Combo_ClickTitleBarX_PressEscape, "YES", "NO");

                    
                }
                ImGui.Separator();

                ImGui.Button("Cancel & Discard Changes");
                if (ImGui.IsItemClicked())
                {
                    CancelType = CancelTypes.ClickTitleBarX;
                    Dismiss();
                }

                bool invalidState = (ImportFromAnimID_Value == null || TaeAnimID_Value == null);

                ImGui.Button("Apply & Save Changes");
                if (ImGui.IsItemClicked())
                {
                    if (invalidState)
                    {
                        DialogManager.DialogOK("Errors Present", "Cannot accept changes until the animation ID formatting errors (shown in red) are fixed.");
                    }
                    else
                    {
                        CancelType = CancelTypes.ClickedAcceptButton;
                        Dismiss();
                    }
                   
                }

                //throw new NotImplementedException();
            }
        }
    }
}
