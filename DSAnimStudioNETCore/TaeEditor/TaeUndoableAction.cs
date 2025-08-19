using System;
using System.Collections.Generic;
using SoulsAssetPipeline.Animation;

namespace DSAnimStudio.TaeEditor
{
    public abstract class TaeUndoableAction
    {
        protected TaeEditorScreen MainScreen;
        public string ActionDescription;
        protected Action DoAction;
        protected Action UndoAction;
        protected Action<TaeUndoableAction> RegistAction;
        protected TaeRestorableState StateOnRedo;
        protected TaeRestorableState StateOnUndo;
        protected TAE.Template TaeTemplate;

        public abstract void PerformDo();
        public abstract void PerformUndo();
        public void PerformRegist()
        {
            RegistAction?.Invoke(this);
        }





        private bool IsCustomRegist = false;
        private List<int> RegisteredAnimCategories = new List<int>();
        private List<SplitAnimID> RegisteredAnims = new List<SplitAnimID>();

        public bool GetIsCustomRegist() => IsCustomRegist;
        public List<int> GetRegisteredAnimCategories() => RegisteredAnimCategories;
        public List<SplitAnimID> GetRegisteredAnims() => RegisteredAnims;

        public void RegistAnimCategory(int categoryID)
        {
            if (!RegisteredAnimCategories.Contains(categoryID))
                RegisteredAnimCategories.Add(categoryID);
            IsCustomRegist = true;
        }

        public void RegistAnimation(SplitAnimID animID)
        {
            if (!RegisteredAnims.Contains(animID))
                RegisteredAnims.Add(animID);
            IsCustomRegist = true;
        }





        public int GetByteCount()
        {
            int result = 0;
            if (StateOnRedo != null)
                result += StateOnRedo.GetByteCount();
            if (StateOnUndo != null)
                result += StateOnUndo.GetByteCount();
            return result;
        }

        protected TaeUndoableAction(TaeEditorScreen mainScreen, Action doAction, Action undoAction, Action<TaeUndoableAction> registAction,
            string actionDescription, TAE.Template template)
        {
            MainScreen = mainScreen;
            ActionDescription = actionDescription;
            DoAction = doAction;
            UndoAction = undoAction;
            RegistAction = registAction;
            TaeTemplate = template;
        }

            
    }
}