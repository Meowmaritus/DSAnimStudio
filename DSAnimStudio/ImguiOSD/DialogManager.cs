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
        private static List<Dialog> dialogs = new List<Dialog>();
        private static bool dialogsChanged = false;
        public static TaeEditorScreen Tae => Main.TAE_EDITOR;
        public static void UpdateDialogs()
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
        }

        private static void AddDialog(Dialog dlg)
        {
            dialogsChanged = true;
            dialogs.Insert(0, dlg);
        }

        public static void DialogOK(string title, string text)
        {
            AskForMultiChoice(title ?? "Notice", text, null, Dialog.CancelTypes.Combo_All, "OK");
        }

        public static void ShowTaeAnimPropertiesEditor(TAE.Animation anim)
        {
            var dlg = new Dialog.TaeAnimPropertiesEdit(anim);
            dlg.OnDismiss = () =>
            {
                if (dlg.CancelType == Dialog.CancelTypes.None)
                {
                    anim.MiniHeader = dlg.TaeAnimHeader;
                    anim.ID = dlg.TaeAnimID;
                    anim.AnimFileName = dlg.TaeAnimName;
                    Tae.RecreateAnimList();
                    Tae.UpdateSelectedTaeAnimInfoText();
                }
            };
            AddDialog(dlg);
        }

        public static void AskYesNo(string title, string question, Action<bool> onChoose,
            bool allowCancel = true, bool enterKeyForYes = true)
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
        }

        public static void AskForMultiChoice(string title, string question,
            Action<Dialog.CancelTypes, string> onAnswer, Dialog.CancelTypes allowedCancelTypes, params string[] answers)
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
        }

        public static void AskForInputString(string title, string question, string textHint,
            Action<string> onResult, bool canBeCancelled)
        {
            var dlg = new Dialog.TextInput()
            {
                AllowedCancelTypes = canBeCancelled ? 
                    Dialog.CancelTypes.Combo_ClickTitleBarX_PressEscape : 
                    Dialog.CancelTypes.None,
                TextHint = textHint,
                Question = question,
                Title = title,
            };

            dlg.OnDismiss += () =>
            {
                if (!dlg.WasCancelled)
                    onResult?.Invoke(dlg.InputtedString);
            };

            AddDialog(dlg);
        }
    }
}
