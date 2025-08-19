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
        public class DuplicateAnimCategory : Dialog
        {
            public readonly DSAProj Proj;
            public readonly DSAProj.AnimCategory Tae;

            public class Result
            {
                public int SelectedAnimCategoryID;
                public bool ReferenceHkxOfOriginalCategory = true;
                public bool UserConfirmedTheyAreOkWithIDConflict = false;
            }

            public Result Res = new Result();

            public int CurrentAnimCategoryID;

            public unsafe DuplicateAnimCategory(DSAProj proj, DSAProj.AnimCategory tae)
                : base("Duplicate Anim Category")
            {
                Proj = proj;
                Tae = tae;
                CancelHandledByInheritor = true;
                AcceptHandledByInheritor = true;

            }

            protected override void BuildInsideOfWindow()
            {
                //ImGui.PushTextWrapPos(ImGui.GetFontSize() * 400);
                //ImGui.TextWrapped(Question);
                //ImGui.PopTextWrapPos();
                ImGui.Text("Anim Category ID to copy to: a");
                ImGui.SameLine();
                if (IsFirstFrameBuildingWindow)
                    ImGui.SetKeyboardFocusHere();
                ImGui.PushItemWidth(100);
                ImGui.InputInt("##Dialog.DuplicateTaeSection.SelectedAnimCategory", ref Res.SelectedAnimCategoryID);
                ImGui.PopItemWidth();

                if (Res.SelectedAnimCategoryID < 0)
                    Res.SelectedAnimCategoryID = 0;
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DES or SoulsAssetPipeline.SoulsGames.DS1 or SoulsAssetPipeline.SoulsGames.DS1R)
                {
                    if (Res.SelectedAnimCategoryID > 255)
                        Res.SelectedAnimCategoryID = 255;
                }
                else
                {
                    if (Res.SelectedAnimCategoryID > 999)
                        Res.SelectedAnimCategoryID = 999;
                }

                ImGui.Checkbox("Reference HKX animations of original category", ref Res.ReferenceHkxOfOriginalCategory);

                ImGui.Text(" ");

                if (Tools.SimpleClickButton("Cancel") || IsTitleBarXRequested || IsEscapeKeyRequested)
                {
                    ResultType = ResultTypes.Cancel;
                    Dismiss();
                }
                ImGui.SameLine();

                bool invalid = Proj.SAFE_CategoryExists(Res.SelectedAnimCategoryID);

                if (invalid)
                    ImGui.BeginDisabled();

                if (Tools.SimpleClickButton("Accept"))
                {
                    if (!invalid)
                    {
                        if (Proj.SAFE_CategoryExists(Res.SelectedAnimCategoryID))
                        {
                            DialogManager.AskYesNo("Anim Category ID Conflict", $"Anim Category ID {Res.SelectedAnimCategoryID} already exists. If you choose this ID, it will cause an ID conflict error. Are you sure you wish to continue?", res =>
                            {
                                if (res)
                                {
                                    Res.UserConfirmedTheyAreOkWithIDConflict = true;
                                    ResultType = ResultTypes.Accept;
                                    Dismiss();
                                }
                            }, allowCancel: true, inputFlags: InputFlag.EscapeKeyToCancel | InputFlag.TitleBarXToCancel | InputFlag.EnterKeyToAccept);
                        }
                        else
                        {
                            ResultType = ResultTypes.Accept;
                            Dismiss();
                        }
                    }
                    
                }

                if (invalid)
                    ImGui.EndDisabled();
            }
        }
    }
}
