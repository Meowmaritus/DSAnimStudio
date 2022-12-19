using DSAnimStudio.TaeEditor;
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
        private static object _lock = new object();
        public static void UpdateDialogs()
        {
            lock (_lock)
            {
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
            }
        }

        private static void AddDialog(Dialog dlg)
        {
            lock (_lock)
            {
                dialogsChanged = true;
                dialogs.Insert(0, dlg);
            }
        }

        public static void DialogOK(string title, string text)
        {
            AskForMultiChoice(title ?? "Notice", text, null, Dialog.CancelTypes.Combo_All, "OK");
        }

        public static void ShowTaeAnimPropertiesEditor(TAE.Animation anim)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.TaeAnimPropertiesEdit(anim, Tae.FileContainer.AllTAEDict.Count > 1);
                dlg.OnDismiss = () =>
                {
                    if (dlg.CancelType == Dialog.CancelTypes.ClickedAcceptButton)
                    {
                        if (dlg.WasAnimDeleted)
                        {
                            if (Tae.SelectedTae.Animations.Count <= 1)
                            {
                                DialogOK("Can't Delete Last Animation",
                                    "Cannot delete the only animation remaining in the TAE.");
                            }
                            else
                            {
                                var indexOfCurrentAnim = Tae.SelectedTae.Animations.IndexOf(Tae.SelectedTaeAnim);
                                Tae.SelectedTae.Animations.Remove(Tae.SelectedTaeAnim);

                                Tae.RecreateAnimList();
                                Tae.UpdateSelectedTaeAnimInfoText();

                                if (indexOfCurrentAnim > Tae.SelectedTae.Animations.Count - 1)
                                    indexOfCurrentAnim = Tae.SelectedTae.Animations.Count - 1;

                                if (indexOfCurrentAnim >= 0)
                                    Tae.SelectNewAnimRef(Tae.SelectedTae, Tae.SelectedTae.Animations[indexOfCurrentAnim]);
                                else
                                    Tae.SelectNewAnimRef(Tae.SelectedTae, Tae.SelectedTae.Animations[0]);

                                Tae.SelectedTae.SetIsModified(!Tae.IsReadOnlyFileMode);
                            }


                        }
                        else
                        {
                            anim.MiniHeader = dlg.TaeAnimHeader;
                            anim.ID = dlg.TaeAnimID_Value.Value;
                            anim.AnimFileName = dlg.TaeAnimName;
                            anim.SetIsModified(true);
                            Tae.RecreateAnimList();
                            Tae.UpdateSelectedTaeAnimInfoText();
                        }
                    }
                };
                AddDialog(dlg);
            }));
        }

        public static void AskYesNo(string title, string question, Action<bool> onChoose,
            bool allowCancel = true, bool enterKeyForYes = true)
        {
            Task.Run(new Action(() =>
            {
                var allowedCancelTypes = Dialog.CancelTypes.None;
                if (allowCancel)
                    allowedCancelTypes |= Dialog.CancelTypes.Combo_ClickTitleBarX_PressEscape;
                if (enterKeyForYes)
                    allowedCancelTypes |= Dialog.CancelTypes.PressEnter;
                AskForMultiChoice(title, question, (cancelType, answer) =>
                {
                    if (cancelType == Dialog.CancelTypes.PressEnter)
                        onChoose?.Invoke(true);
                    else if (cancelType == Dialog.CancelTypes.PressEscape || cancelType == Dialog.CancelTypes.ClickTitleBarX)
                        onChoose?.Invoke(false);
                    else if (answer == "YES")
                        onChoose?.Invoke(true);
                    else if (answer == "NO")
                        onChoose?.Invoke(false);

                }, allowedCancelTypes, "YES", "NO");
            }));
        }

        public static void AskForMultiChoice(string title, string question,
            Action<Dialog.CancelTypes, string> onAnswer, Dialog.CancelTypes allowedCancelTypes, params string[] answers)
        {
            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.MultiChoice()
                {
                    AllowedCancelTypes = allowedCancelTypes,
                    Answers = answers.ToList(),
                    Question = question,
                    Title = title,
                };
                dlg.OnDismiss += () => onAnswer?.Invoke(dlg.CancelType, dlg.SelectedAnswer);
                AddDialog(dlg);
            }));
        }

        public static void AskForInputString(string title, string question, string textHint,
            Action<string> onResult, bool canBeCancelled, string startingText = null)
        {

            Task.Run(new Action(() =>
            {
                var dlg = new Dialog.TextInput()
                {
                    AllowedCancelTypes = canBeCancelled ?
                    Dialog.CancelTypes.Combo_ClickTitleBarX_PressEscape :
                    Dialog.CancelTypes.None,
                    TextHint = textHint,
                    Question = question,
                    Title = title,
                    InputtedString = startingText ?? string.Empty,
                };

                dlg.OnDismiss += () =>
                {
                    if (!dlg.WasCancelled)
                        onResult?.Invoke(dlg.InputtedString);
                };

                AddDialog(dlg);
            }));

            
        }
    }
}
