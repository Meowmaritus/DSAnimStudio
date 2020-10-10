using System;
using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public sealed class TaeUndoMan
    {
        public readonly TaeEditorScreen MainScreen;

        public TaeUndoMan(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;
        }

        public class TaeUndoableAction
        {
            public Action Do;
            public Action Undo;
        }

        private Stack<TaeUndoableAction> UndoStack = new Stack<TaeUndoableAction>();
        private Stack<TaeUndoableAction> RedoStack = new Stack<TaeUndoableAction>();

        public bool CanUndo => UndoStack.Count > 0;
        public bool CanRedo => RedoStack.Count > 0;

        public void NewAction(Action doAction, Action undoAction)
        {
            RedoStack.Clear();
            var newAction = new TaeUndoableAction()
            {
                Do = doAction,
                Undo = undoAction
            };
            newAction.Do();
            UndoStack.Push(newAction);
        }

        public void Redo()
        {
            MainScreen.Graph?.ViewportInteractor?.EventSim?.ClearBoxStuff();

            if (RedoStack.Count > 0)
            {
                var action = RedoStack.Pop();
                action.Do();
                UndoStack.Push(action);
            }

            MainScreen.Graph?.ViewportInteractor?.EventSim?.ClearBoxStuff();
        }

        public void Undo()
        {
            MainScreen.Graph?.ViewportInteractor?.EventSim?.ClearBoxStuff();

            if (UndoStack.Count > 0)
            {
                var lastAction = UndoStack.Pop();
                lastAction.Undo();
                RedoStack.Push(lastAction);
            }

            MainScreen.Graph?.ViewportInteractor?.EventSim?.ClearBoxStuff();
        }
    }
}
