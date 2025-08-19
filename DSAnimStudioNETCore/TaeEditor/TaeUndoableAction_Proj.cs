
using System;
using System.Collections.Generic;
using SoulsAssetPipeline.Animation;

namespace DSAnimStudio.TaeEditor
{
    public class TaeUndoableAction_Proj : TaeUndoableAction
    {
        private DSAProj Proj;

        


        public TaeUndoableAction_Proj(TaeEditorScreen mainScreen, DSAProj proj, Action doAction, Action undoAction, Action<TaeUndoableAction> registAction,
            string actionDescription, TAE.Template template)
            : base(mainScreen, doAction, undoAction, registAction, actionDescription, template)
        {
            Proj = proj;
        }
            
        public override void PerformDo()
        {
            Main.WinForm.Invoke(() =>
            {
                lock (zzz_DocumentManager._lock_CurrentDocument)
                {
                    StateOnUndo = new TaeRestorableState_Proj(MainScreen, Proj, TaeTemplate, 
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
                    StateOnRedo = new TaeRestorableState_Proj(MainScreen, Proj, TaeTemplate, 
                        GetIsCustomRegist(), GetRegisteredAnimCategories(), GetRegisteredAnims());
                    UndoAction?.Invoke();
                    StateOnUndo?.RestoreState();
                }
            });
            
        }
    }
}