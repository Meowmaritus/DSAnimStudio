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
        public class AnimCategoryPropertiesEdit : Dialog
        {
            public DSAProj Proj;
            public DSAProj.AnimCategory Category;
            public DSAProj.EditorInfo NewState_Info;
            public int NewState_CategoryID;
            public int NewState_ActionSetVersion_ForMultiTaeOutput;
            public bool WasAnimCategoryDeleted = false;

            public AnimCategoryPropertiesEdit(DSAProj proj, DSAProj.AnimCategory category)
                : base("Edit Animation Category Properties")
            {
                Proj = proj;
                Category = category;

                NewState_Info = new DSAProj.EditorInfo();
                NewState_Info.DeserializeFromBytes(category.Info.SerializeToBytes(proj), proj.Template, proj);

                NewState_CategoryID = category.CategoryID;
                NewState_ActionSetVersion_ForMultiTaeOutput = category.ActionSetVersion_ForMultiTaeOutput;

                CancelHandledByInheritor = true;
                AcceptHandledByInheritor = true;
                TitleBarXToCancel = true;
                EscapeKeyToCancel = true;
                AllowsResultCancel = true;
            }

            protected override void BuildInsideOfWindow()
            {
                bool anyFieldModifiedInInfo = false;
                NewState_Info.ShowImGui(ref anyFieldModifiedInInfo, Proj, 
                    GetRect(subtractScrollBar: true).DpiScaled(), 
                    GetRect(subtractScrollBar: false).DpiScaled(), modified =>
                {
                    if (modified)
                    {
                        Category.SAFE_SetIsModified(true);
                    }
                }, showTagsOnly: false, out _);


                ImGui.Separator();

                if (Proj.RootTaeProperties.SaveEachCategoryToSeparateTae)
                {
                    ImGui.InputInt("Action Set Version", ref NewState_ActionSetVersion_ForMultiTaeOutput);
                }
                else
                {
                    ImGui.Text("(Action Set Version is located in Root TAE Properties for non-c0000)");
                }

                ImGui.Separator();

                ImGui.Button("Delete This Animation Category...");
                if (ImGui.IsItemClicked())
                {
                    DialogManager.AskYesNo("Permanently Delete Animation Category Entry?", $"Are you sure you want to delete the current animation category and all animations within it?",
                        choice =>
                        {
                            if (choice)
                            {
                                WasAnimCategoryDeleted = true;
                                ResultType = ResultTypes.Accept;
                                Dismiss();
                                DialogManager.DialogOK("Success", "Animation category deleted successfully.");
                            }

                        });

                }
                ImGui.Separator();

                bool clickedCancel = Tools.SimpleClickButton("Cancel") || IsTitleBarXRequested;
                bool pressedEscape = IsEscapeKeyRequested;

                if (clickedCancel || pressedEscape)
                {
                    bool unsavedChanges = !Category.Info.SerializeToBytes(Proj).SequenceEqual(NewState_Info.SerializeToBytes(Proj))
                        || Category.CategoryID != NewState_CategoryID || 
                        (Proj.RootTaeProperties.SaveEachCategoryToSeparateTae 
                        && Category.ActionSetVersion_ForMultiTaeOutput != NewState_ActionSetVersion_ForMultiTaeOutput);

                    bool forceClose = Main.Input.ShiftHeld;
                    if (!unsavedChanges || forceClose)
                    {
                        ResultType = ResultTypes.Cancel;
                        Dismiss();
                    }
                    else
                    {
                        DialogManager.AskYesNo("Unsaved Changes", "You have unsaved changes, would you like to discard them and close this dialog?", choice =>
                        {
                            if (choice == true)
                            {
                                ResultType = ResultTypes.Cancel;
                                Dismiss();
                            }
                        });
                    }
                }

                if (Tools.SimpleClickButton("Save & Accept"))
                {
                    ResultType = ResultTypes.Accept;
                    Dismiss();
                }

            }
        }
    }
}
