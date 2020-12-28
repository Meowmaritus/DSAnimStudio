using System;
using System.Collections.Generic;
using System.Linq;

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
            private TaeEditAnimEventGraph Graph;
            private IEnumerable<ITaeClonable> ItemsToCapture;
            private Action DoAction;
            private Action UndoAction;
            private TaeUndoRestorableGraphState StateOnRedo;
            private TaeUndoRestorableGraphState StateOnUndo;

            private bool IsCustomPreState = false;

            public TaeUndoableAction(TaeEditAnimEventGraph graph, Action doAction, Action undoAction, TaeUndoRestorableGraphState customPreState)
            {
                Graph = graph;
                ItemsToCapture = null;

                StateOnUndo = customPreState;
                IsCustomPreState = true;
                ItemsToCapture = new List<ITaeClonable>();

                DoAction = doAction;
                UndoAction = undoAction;
            }

            public TaeUndoableAction(TaeEditAnimEventGraph graph, Action doAction, Action undoAction, IEnumerable<ITaeClonable> itemsToCapture)
            {
                Graph = graph;
                ItemsToCapture = itemsToCapture;
                DoAction = doAction;
                UndoAction = undoAction;
            }

            public void PerformDo()
            {
                if (!IsCustomPreState)
                    StateOnUndo = new TaeUndoRestorableGraphState(Graph, ItemsToCapture);

                DoAction?.Invoke();
                StateOnRedo?.RestoreState();
            }

            public void PerformUndo()
            {
                StateOnRedo = new TaeUndoRestorableGraphState(Graph, ItemsToCapture);
                UndoAction?.Invoke();
                StateOnUndo?.RestoreState();
            }
        }

        private Stack<TaeUndoableAction> UndoStack = new Stack<TaeUndoableAction>();
        private Stack<TaeUndoableAction> RedoStack = new Stack<TaeUndoableAction>();

        public bool CanUndo => UndoStack.Count > 0;
        public bool CanRedo => RedoStack.Count > 0;

        public void NewActionCustomPreState(TaeUndoRestorableGraphState customPreState, Action doAction, Action undoAction)
        {
            RedoStack.Clear();
            var newAction = new TaeUndoableAction(MainScreen.Graph, doAction, undoAction, customPreState);
            newAction.PerformDo();
            UndoStack.Push(newAction);
        }

        public void NewAction(Action doAction, Action undoAction, List<ITaeClonable> captureItems)
        {
            NewActionOptional(true, doAction, undoAction, captureItems);
        }

        public void NewActionOptional(bool enableUndoOnAction, Action doAction, Action undoAction, List<ITaeClonable> captureItems)
        {
            if (enableUndoOnAction && MainScreen?.Graph != null)
            {
                RedoStack.Clear();
                var newAction = new TaeUndoableAction(MainScreen.Graph, doAction, undoAction, captureItems);
                newAction.PerformDo();
                UndoStack.Push(newAction);
            }
            else
            {
                doAction?.Invoke();
            }
        }

        public void Redo()
        {
            MainScreen.Graph?.ViewportInteractor?.EventSim?.ClearBoxStuff();

            if (RedoStack.Count > 0)
            {
                var action = RedoStack.Pop();
                action.PerformDo();
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
                lastAction.PerformUndo();
                RedoStack.Push(lastAction);
            }

            MainScreen.Graph?.ViewportInteractor?.EventSim?.ClearBoxStuff();
        }
    }
}
