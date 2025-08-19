using System;
using SoulsAssetPipeline.Animation;

namespace DSAnimStudio.TaeEditor
{
    public class TaeUndoableAction_Anim : TaeUndoableAction
    {
        private DSAProj.Animation Anim;
        public TaeUndoableAction_Anim(TaeEditorScreen mainScreen, DSAProj.Animation anim, Action doAction, Action undoAction, Action<TaeUndoableAction> registAction,
            string actionDescription, TAE.Template template)
            : base(mainScreen, doAction, undoAction, registAction, actionDescription, template)
        {
            Anim = anim;
        }
            
        public override void PerformDo()
        {
            Main.WinForm.Invoke(() =>
            {
                lock (zzz_DocumentManager._lock_CurrentDocument)
                {
                    StateOnUndo = new TaeRestorableState_Anim(MainScreen, Anim, TaeTemplate, 
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
                    StateOnRedo = new TaeRestorableState_Anim(MainScreen, Anim, TaeTemplate, 
                        GetIsCustomRegist(), GetRegisteredAnimCategories(), GetRegisteredAnims());
                    UndoAction?.Invoke();
                    StateOnUndo?.RestoreState();
                }
            });
            
        }
    }
}