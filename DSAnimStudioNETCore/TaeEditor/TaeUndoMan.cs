using System;
using System.Collections.Generic;
using System.Linq;
using SoulsAssetPipeline.Animation;
using static DSAnimStudio.ImguiOSD.Window;

namespace DSAnimStudio.TaeEditor
{
    public sealed class TaeUndoMan
    {
        public enum UndoTypes
        {
            Anim,
            Proj,
        }

        public readonly UndoTypes UndoType;
        public readonly TaeEditorScreen MainScreen;

        public TaeUndoMan(TaeEditorScreen mainScreen, UndoTypes undoType)
        {
            MainScreen = mainScreen;
            UndoType = undoType;
        }

       
        
        

        public bool UseCompression = true;

        private Stack<TaeUndoableAction> UndoStack = new Stack<TaeUndoableAction>();
        private Stack<TaeUndoableAction> RedoStack = new Stack<TaeUndoableAction>();

        public bool CanUndo => UndoStack.Count > 0;
        public bool CanRedo => RedoStack.Count > 0;

        public long StackSizeBytes { get; private set; } = 0;

        public void RecalculateStackSizeBytes()
        {
            StackSizeBytes = 0;
            foreach (var undo in UndoStack)
                StackSizeBytes += undo.GetByteCount();
            foreach (var redo in RedoStack)
                StackSizeBytes += redo.GetByteCount();
        }

        public string GetNextUndoName()
        {
            if (UndoStack.Count > 0)
                return UndoStack.Peek().ActionDescription;
            else
                return null;
        }
        
        public string GetNextRedoName()
        {
            if (RedoStack.Count > 0)
                return RedoStack.Peek().ActionDescription;
            else
                return null;
        }


        public void NewAction_AnimCategory(Action doAction, Action undoAction = null, string actionDescription = null, Action<TaeUndoableAction> registAction = null)
        {
            TaeUndoableAction newAction = null;
            //if (UndoType == UndoTypes.Anim)
            //{
            //    newAction = new TaeUndoableAction_Anim(MainScreen, MainScreen.Graph.AnimRef, doAction, undoAction, registAction,
            //        actionDescription, MainScreen?.FileContainer?.TaeTemplate);
            //}
            //else if (UndoType == UndoTypes.Proj)
            //{
            //    newAction = new TaeUndoableAction_Proj(MainScreen, MainScreen.FileContainer.Proj, doAction, undoAction, registAction,
            //        actionDescription, MainScreen?.FileContainer?.TaeTemplate);
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}

            newAction = new TaeUndoableAction_AnimCategory(MainScreen, MainScreen.SelectedAnimCategory, doAction, undoAction, registAction,
                    actionDescription, MainScreen?.FileContainer?.TaeTemplate);

            newAction?.PerformRegist();

            RedoStack.Clear();

            newAction.PerformDo();
            UndoStack.Push(newAction);
            RecalculateStackSizeBytes();
        }


        public void NewAction(Action doAction, Action undoAction = null, string actionDescription = null, Action<TaeUndoableAction> registAction = null)
        {
            TaeUndoableAction newAction = null;
            if (UndoType == UndoTypes.Anim)
            {
                newAction = new TaeUndoableAction_Anim(MainScreen, MainScreen.Graph.AnimRef, doAction, undoAction, registAction,
                    actionDescription, MainScreen?.FileContainer?.TaeTemplate);
            }
            else if (UndoType == UndoTypes.Proj)
            {
                newAction = new TaeUndoableAction_Proj(MainScreen, MainScreen.FileContainer.Proj, doAction, undoAction, registAction,
                    actionDescription, MainScreen?.FileContainer?.TaeTemplate);
            }
            else
            {
                throw new NotImplementedException();
            }

            newAction?.PerformRegist();

            RedoStack.Clear();
            
            newAction.PerformDo();
            UndoStack.Push(newAction);
            RecalculateStackSizeBytes();
        }
        
        

        public void Redo()
        {
            MainScreen.Graph?.ViewportInteractor?.ActionSim?.ClearBoxStuff();

            if (RedoStack.Count > 0)
            {
                var action = RedoStack.Pop();
                action.PerformDo();
                //NotificationManager.PushNotification($"Redid '{action.ActionDescription}'.", showDuration: 1, fadeDuration: 0.25f);
                UndoStack.Push(action);
                MainScreen.Graph.AnimRef.SAFE_SetIsModified(true);
                MainScreen.Graph?.InputMan?.ResetAllDrag();
                MainScreen.FixActionSelectionAfterUndoRedo();
                MainScreen.FixAnimSelectionAfterUndoRedo();

                MainScreen.Graph?.ViewportInteractor?.NewScrub(absolute: false, time: 0, forceRefreshTimeact: true);
                Main.MainThreadLazyDispatch(() =>
                {
                    MainScreen.Graph?.ViewportInteractor?.CurrentModel?.NewForceSyncUpdate();
                }, frameDelay: 1);
            }

            RecalculateStackSizeBytes();

            MainScreen.Graph?.ViewportInteractor?.ActionSim?.ClearBoxStuff();
        }

        public void Undo()
        {
            MainScreen.Graph?.ViewportInteractor?.ActionSim?.ClearBoxStuff();

            if (UndoStack.Count > 0)
            {
                var lastAction = UndoStack.Pop();
                lastAction.PerformUndo();
                //NotificationManager.PushNotification($"Undid '{lastAction.ActionDescription}'.", showDuration: 1, fadeDuration: 0.25f);
                RedoStack.Push(lastAction);
                MainScreen.Graph.AnimRef.SAFE_SetIsModified(true);
                MainScreen.Graph?.InputMan?.ResetAllDrag();
                MainScreen.FixActionSelectionAfterUndoRedo();
                MainScreen.FixAnimSelectionAfterUndoRedo();

                MainScreen.Graph?.ViewportInteractor?.NewScrub(absolute: false, time: 0, forceRefreshTimeact: true);
                Main.MainThreadLazyDispatch(() =>
                {
                    MainScreen.Graph?.ViewportInteractor?.CurrentModel?.NewForceSyncUpdate();
                }, frameDelay: 1);
            }

            RecalculateStackSizeBytes();

            MainScreen.Graph?.ViewportInteractor?.ActionSim?.ClearBoxStuff();
            
        }
    }
}
