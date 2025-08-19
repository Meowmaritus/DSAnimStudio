using System;
using SoulsAssetPipeline.Animation;

namespace DSAnimStudio.TaeEditor
{
    public class TaeUndoableAction_AnimCategory : TaeUndoableAction
    {
        private DSAProj.AnimCategory AnimCategory;
        public TaeUndoableAction_AnimCategory(TaeEditorScreen mainScreen, DSAProj.AnimCategory animCategory, Action doAction, Action undoAction, Action<TaeUndoableAction> registAction,
            string actionDescription, TAE.Template template)
            : base(mainScreen, doAction, undoAction, registAction, actionDescription, template)
        {
            AnimCategory = animCategory;
        }
            
        public override void PerformDo()
        {
            Main.WinForm.Invoke(() =>
            {
                lock (zzz_DocumentManager._lock_CurrentDocument)
                {
                    StateOnUndo = new TaeRestorableState_AnimCategory(MainScreen, AnimCategory, TaeTemplate, 
                        GetIsCustomRegist(), GetRegisteredAnimCategories(), GetRegisteredAnims());
                    DoAction?.Invoke();
                    StateOnRedo?.RestoreState();
                }
            });
            
        }

        public override void PerformUndo()
        {
            Main.WinForm.Invoke(() =>
            {
                lock (zzz_DocumentManager._lock_CurrentDocument)
                {
                    StateOnRedo = new TaeRestorableState_AnimCategory(MainScreen, AnimCategory, TaeTemplate, 
                        GetIsCustomRegist(), GetRegisteredAnimCategories(), GetRegisteredAnims());
                    UndoAction?.Invoke();
                    StateOnUndo?.RestoreState();
                }
            });
            
        }
    }
}