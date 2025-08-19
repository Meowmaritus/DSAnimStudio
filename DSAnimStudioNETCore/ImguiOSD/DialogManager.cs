using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static class DialogManager
    {
        public static bool AnyDialogsShowing { get; private set; } = false;

        private static List<Dialog> dialogs = new List<Dialog>();
        private static bool dialogsChanged = false;
        public static TaeEditorScreen Tae => Main.TAE_EDITOR;

        private static Window windowFocusBeforeDialog = null;

        public static FancyInputHandler Input = new FancyInputHandler();

        public static void ClearAll()
        {
            lock (_lock)
            {
                dialogs.Clear();
                AnyDialogsShowing = false;
                windowFocusBeforeDialog = null;
                dialogsChanged = false;
            }
        }

        private static object _lock = new object();
        public static void UpdateDialogs()
        {
            DialogManager.Input.Update(new Rectangle(0, 0, Program.MainInstance.BoundsLastUpdatedFor.Width, Program.MainInstance.BoundsLastUpdatedFor.Height).InverseDpiScaled(),
                                        forceUpdate: true, disableIfFieldsFocused: false);

            lock (_lock)
            {
                int dialogCountBeforeDismiss = dialogs.Count;

                List<Dialog> dialogsThatHaveFinished = new List<Dialog>();

                for (int i = 0; i < dialogs.Count; i++)
                {
                    if (!dialogs[i].IsDismissed)
                        dialogs[i].Update(i == 0);
                    if (dialogs[i].IsDismissed)
                        dialogsThatHaveFinished.Add(dialogs[i]);
                    if (dialogsChanged)
                    {
                        dialogsChanged = false;
                        break;
                    }
                }
                foreach (var q in dialogsThatHaveFinished)
                {
                    dialogs.Remove(q);
                }

                AnyDialogsShowing = dialogs.Count > 0;

                if (dialogs.Count != dialogCountBeforeDismiss)
                {
                    ImGui.SetWindowFocus(windowFocusBeforeDialog?.ImguiTag);
                    OSD.ActualFocusedWindow = windowFocusBeforeDialog;
                }
            }
        }

        private static void AddDialog(Dialog dlg)
        {
            lock (_lock)
            {
                if (dialogs.Count == 0)
                    windowFocusBeforeDialog = OSD.ActualFocusedWindow;
                dialogsChanged = true;
                dialogs.Insert(0, dlg);
            }
        }

        public static void DialogOK(string title, string text)
        {
            AskForMultiChoice(title ?? "Notice", text, null, allowedResults: Dialog.ResultTypes.Accept | Dialog.ResultTypes.Cancel,
                inputFlags: Dialog.InputFlag.EnterKeyToAccept | Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel,  "OK");
        }

        public static void ShowWelcome()
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.Welcome();
                AddDialog(dlg);
            }));
        }

        public static void ShowRootTaePropertiesEditor(DSAProj proj)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.RootTaePropertiesEdit(proj);
                dlg.OnDismiss = () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                    {
                        proj.RootTaeProperties = dlg.RootTaeProperties;
                        proj.SAFE_MarkAllModified();
                    }
                };
                AddDialog(dlg);
            }));
        }

        public static void ShowTagPickDialog(string title, DSAProj proj, Action<DSAProj.Tag> onTagPicked)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.TagPicker(title, proj);
                dlg.OnDismiss = () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                    {
                        onTagPicked?.Invoke(dlg.ChosenTag);
                    }
                };
                AddDialog(dlg);
            }));
        }

        public static void ShowTaeAnimCategoryPropertiesEditor(DSAProj proj, DSAProj.AnimCategory category)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.AnimCategoryPropertiesEdit(proj, category);
                dlg.OnDismiss = () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                    {
                        

                        

                        if (dlg.WasAnimCategoryDeleted)
                        {




                            if (proj.SAFE_GetAnimCategoriesCount() <= 1)
                            {
                                DialogOK("Can't Delete Last Animation Category",
                                    "Cannot delete the only animation category remaining in the project.");
                            }
                            else
                            {
                                Tae.GlobalUndoMan.NewAction(() =>
                                {
                                    var cate = Tae.SelectedAnimCategory;

                                    // go to next category
                                    Tae.NextAnim(shiftPressed: false, ctrlPressed: true); 
                                    // Delete category
                                    proj.SAFE_RemoveAnimCategory(cate);
                                }, actionDescription: $"Delete animation category {category.CategoryID}");

                                

                                Tae.SelectedAnimCategory.SAFE_SetIsModified(!Tae.IsReadOnlyFileMode);
                            }


                        }
                        else
                        {
                            var captureNewCategoryID = dlg.NewState_CategoryID;
                            var captureNewActionSetVersion = dlg.NewState_ActionSetVersion_ForMultiTaeOutput;
                            var captureIsMultiTae = proj.RootTaeProperties.SaveEachCategoryToSeparateTae;
                            Tae.GlobalUndoMan.NewAction(() =>
                            {
                                category.Info = dlg.NewState_Info;
                                //category.CategoryID = dlg.NewState_CategoryID;
                                proj.SAFE_RegistCategoryIDChange(category, category.CategoryID, captureNewCategoryID);
                                if (captureIsMultiTae)
                                    category.ActionSetVersion_ForMultiTaeOutput = captureNewActionSetVersion;
                                else
                                    category.ActionSetVersion_ForMultiTaeOutput = -1;
                                //anim.Header = dlg.TaeAnimHeader;
                                //anim.NewID = dlg.TaeAnimID_Value.Value;
                                //if (dlg.TaeAnimName == "%null%")
                                //    anim.Header.AnimFileName = null;
                                //else
                                //    anim.Header.AnimFileName = dlg.TaeAnimName;
                                //anim.Info.CustomColor = dlg.TaeAnimCustomColor;
                            }, actionDescription: $"Edit animation category {category.CategoryID} properties");

                            
                            category.SAFE_SetIsModified(true);
                            Tae.RecreateAnimList();
                            Tae.UpdateSelectedTaeAnimInfoText();
                            
                        }
                    }
                };
                AddDialog(dlg);
            }));
        }



        public static void ShowTaeAnimPropertiesEditor(DSAProj proj, DSAProj.Animation anim)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.TaeAnimPropertiesEdit(proj, anim);
                dlg.OnDismiss = () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                    {
                        if (dlg.WasAnimDeleted)
                        {


                            Tae.DeleteCurrentAnimation();
                            
                        }
                        else
                        {
                            var undoMan = Tae.GetUndoManForSpecificAnim(anim);
                            undoMan.NewAction(() =>
                            {
                                
                                proj.SAFE_RegistAnimationIDChange(anim, anim.SplitID, dlg.TaeAnimID_Value.Value);
                                //anim.RegistUserChangeID(dlg.TaeAnimID_Value.Value);

                                if (dlg.NewTaeAnimName == "%null%")
                                    dlg.TaeAnimHeader.AnimFileName = null;
                                else
                                    dlg.TaeAnimHeader.AnimFileName = dlg.NewTaeAnimName;

                                anim.SAFE_SetHeader(dlg.TaeAnimHeader);

                                anim.Info = dlg.NewEditInfo;

                                //anim.Info.CustomColor = dlg.TaeAnimCustomColor;
                                
                            }, actionDescription: $"Change animation {anim.SplitID.GetFormattedIDString(proj)} properties");
                            anim.SAFE_SetIsModified(true);
                            Tae.RecreateAnimList();
                            Tae.UpdateSelectedTaeAnimInfoText();
                        }
                        Tae.HardReset();
                    }
                };
                AddDialog(dlg);
            }));
        }
        
        
        public static void ShowTaeActionTrackPropertiesEditor(DSAProj.Animation anim, int trackIndex, DSAProj.ActionTrack track)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.TaeActionTrackPropertiesEdit(anim, trackIndex, track);
                dlg.OnDismiss = () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                    {
                        var undoMan = Tae.GetUndoManForSpecificAnim(anim);
                        undoMan.NewAction(() =>
                        {
                            //anim.ActionTracks[trackIndex] = dlg.TrackCopy;
                            anim.SAFE_SetActionTrackAtIndex(trackIndex, dlg.TrackCopy);
                        }, actionDescription: $"Changed action track {trackIndex} properties");
                            
                    }
                };
                AddDialog(dlg);
            }));
        }

        public static void AskYesNo(string title, string question, Action<bool> onChoose, bool allowCancel = true,
            Dialog.InputFlag inputFlags = Dialog.InputFlag.EnterKeyToAccept | Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel)
        {
            Task.Run(new Action(() =>
            {
                Dialog.ResultTypes allowedResults = Dialog.ResultTypes.Accept | Dialog.ResultTypes.Refuse;
                if (allowCancel)
                    allowedResults |= Dialog.ResultTypes.Cancel;
                AskForMultiChoice(title, question, (cancelType, answer) =>
                {
                    if (answer == "YES")
                        onChoose?.Invoke(true);
                    else if (answer == "NO")
                        onChoose?.Invoke(false);
                    else if (cancelType == Dialog.ResultTypes.Accept)
                        onChoose?.Invoke(true);
                    //else if (cancelType == Dialog.ResultTypes.Ca)
                    //    onChoose?.Invoke(true);

                }, allowedResults, inputFlags, "YES", "NO");
            }));
        }

        public static void AskForMultiChoice(string title, string question,
            Action<Dialog.ResultTypes, string> onAnswer, Dialog.ResultTypes allowedResults, Dialog.InputFlag inputFlags, params string[] answers)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.MultiChoice(title)
                {
                    AllowedResultTypes = allowedResults,
                    InputFlags = inputFlags,
                    Answers = answers.ToList(),
                    Question = question,
                };
                dlg.OnDismiss += () => onAnswer?.Invoke(dlg.ResultType, dlg.SelectedAnswer);
                AddDialog(dlg);
            }));
        }

        public static void ShowDialogChangeActionColors(List<DSAProj.Action> selActions)
        {
            var actionsCapture = selActions.ToList();
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.EditActionColors()
                {
                };

                dlg.OnDismiss += () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                    {
                        foreach (var act in actionsCapture)
                        {
                            if (dlg.IsCustomColorSet)
                                act.Info.CustomColor = dlg.SelectedColor;
                            else
                                act.Info.CustomColor = null;
                        }
                    }
                };

                AddDialog(dlg);
            }));
        }

        public static void ShowDialogAnimCategoryDuplicate(DSAProj proj, DSAProj.AnimCategory tae, Action<Dialog.DuplicateAnimCategory.Result> onResult)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.DuplicateAnimCategory(proj, tae)
                {
                    AllowedResultTypes = Dialog.ResultTypes.Accept | Dialog.ResultTypes.Cancel,
                    
                };

                dlg.CurrentAnimCategoryID = tae.CategoryID;

                dlg.OnDismiss += () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                        onResult?.Invoke(dlg.Res);
                };

                AddDialog(dlg);
            }));
        }

        public static void AskForInputString(string title, string question, string textHint,
            Action<string> onResult, Func<string, string> checkError, bool canBeCancelled, string startingText = null)
        {

            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.TextInput(title)
                {
                    AllowsResultCancel = canBeCancelled,
                    AllowsResultAccept = true,
                    EnterKeyToAccept = true,
                    AcceptHandledByInheritor = true,
                    TextHint = textHint,
                    Question = question,
                    InputtedString = startingText ?? string.Empty,
                    CheckErrorFunc =  checkError,
                };

                dlg.OnDismiss += () =>
                {
                    if (dlg.ResultType == Dialog.ResultTypes.Accept)
                        onResult?.Invoke(dlg.InputtedString);
                };

                AddDialog(dlg);
            }));

            
        }
    }
}
