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
            public long TaeAnimID;
            public string TaeAnimName;
            public TAE.Animation.AnimMiniHeader TaeAnimHeader;
            public bool WasAnimDeleted = false;

            public TaeAnimPropertiesEdit(TAE.Animation anim)
            {
                Title = "Edit Animation Properties";

                TaeAnimID = anim.ID;
                TaeAnimName = anim.AnimFileName;
                if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
                {
                    TaeAnimHeader = new TAE.Animation.AnimMiniHeader.ImportOtherAnim()
                    {
                        ImportFromAnimID = asImportOtherAnim.ImportFromAnimID,
                        Unknown = asImportOtherAnim.Unknown,
                    };
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
                }
            }

            protected override void BuildInsideOfWindow()
            {
                bool isCurrentlyStandard = TaeAnimHeader.Type == TAE.Animation.MiniHeaderType.Standard;
                bool isCurrentlyImportOther = TaeAnimHeader.Type == TAE.Animation.MiniHeaderType.ImportOtherAnim;

                TaeAnimID = MenuBar.IntItem("Animation ID", (int)TaeAnimID);

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
                        asImportOtherAnim.ImportFromAnimID = MenuBar.TaeAnimIDItem("Import From Anim ID", asImportOtherAnim.ImportFromAnimID);
                        asImportOtherAnim.Unknown = MenuBar.IntItem("Unknown Value", asImportOtherAnim.Unknown);
                    }
                    ImGui.Unindent();
                }
                else if (TaeAnimHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
                {
                    ImGui.Text("Properties - Standard Animation:");
                    ImGui.Indent();
                    {
                        asStandard.ImportsHKX = MenuBar.CheckboxBig("Imports HKX From Elsewhere", asStandard.ImportsHKX);
                        if (asStandard.ImportsHKX)
                            asStandard.ImportHKXSourceAnimID = MenuBar.IntItem("Import HKX From ID", asStandard.ImportHKXSourceAnimID);
                        else
                            asStandard.ImportHKXSourceAnimID = -1;
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

                ImGui.Button("Apply & Save Changes");
                if (ImGui.IsItemClicked())
                {
                    CancelType = CancelTypes.ClickedAcceptButton;
                    Dismiss();
                }

                //throw new NotImplementedException();
            }
        }
    }
}
