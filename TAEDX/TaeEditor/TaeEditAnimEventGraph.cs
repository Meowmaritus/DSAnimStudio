using MeowDSIO.DataTypes.TAE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeEditAnimEventGraph
    {
        enum BoxDragType
        {
            None,
            LeftOfEventBox,
            RightOfEventBox,
            MiddleOfEventBox,
            MultiSelectionRectangle,
            MultiDragLeftOfEventBox,
            MultiDragRightOfEventBox,
            MultiDragMiddleOfEventBox,
        }

        private class TaeDragState
        {
            public BoxDragType DragType = BoxDragType.None;
            public TaeEditAnimEventBox Box = null;
            public Point Offset = Point.Zero;
            public float BoxOriginalWidth = 0;
            public float BoxOriginalStart = 0;
            public float BoxOriginalEnd = 0;
            public float BoxOriginalDuration => BoxOriginalEnd - BoxOriginalStart;
            public int BoxOriginalRow = 0;
            public int StartMouseRow = 0;
            public Point StartDragPoint = Point.Zero;
            public Point CurrentDragPoint = Point.Zero;
            public Rectangle GetVirtualDragRect() =>
                new Rectangle(MathHelper.Min(CurrentDragPoint.X, StartDragPoint.X),
                    MathHelper.Min(CurrentDragPoint.Y, StartDragPoint.Y),
                    Math.Abs(CurrentDragPoint.X - StartDragPoint.X),
                    Math.Abs(CurrentDragPoint.Y - StartDragPoint.Y));

            public void DragBoxToMouse(Point mouse)
            {
                if (DragType == BoxDragType.MiddleOfEventBox)
                {
                    Box.DragWholeBoxToVirtualUnitX(mouse.X - Offset.X);
                }
                else if (DragType == BoxDragType.LeftOfEventBox)
                {
                    Box.DragLeftSideOfBoxToVirtualUnitX(mouse.X - Offset.X);
                }
                else if (DragType == BoxDragType.RightOfEventBox)
                {
                    Box.DragRightSideOfBoxToVirtualUnitX(mouse.X - Offset.X + BoxOriginalWidth);
                }
            }

            public void ShiftBoxRow(int newMouseRow)
            {
                if (newMouseRow >= 0)
                    Box.MyEvent.Row = BoxOriginalRow + (newMouseRow - StartMouseRow);
            }
        }

        public int MultiSelectRectOutlineThickness => MainScreen.Config.EnableColorBlindMode ? 4 : 1;
        public Color MultiSelectRectFillColor => Color.LightGray * 0.5f;
        public Color MultiSelectRectOutlineColor => MainScreen.Config.EnableColorBlindMode ? Color.Black : Color.White;

        public readonly TaeEditorScreen MainScreen;
        public AnimationRef AnimRef { get; private set; }
        public Rectangle Rect;

        private Dictionary<TimeActEventType, MeowDSIO.DataTypes.TAE.Events.Tae016_SetEventEditorColors> EventTypeColorInfo
            = new Dictionary<TimeActEventType, MeowDSIO.DataTypes.TAE.Events.Tae016_SetEventEditorColors>();

        public MeowDSIO.DataTypes.TAE.Events.Tae016_SetEventEditorColors GetColorInfo(TimeActEventType type)
        {
            if (EventTypeColorInfo.ContainsKey(type))
                return EventTypeColorInfo[type];

            return null;
        }

        public List<TaeEditAnimEventBox> EventBoxes = new List<TaeEditAnimEventBox>();

        public TaeScrollViewer ScrollViewer { get; private set; } = new TaeScrollViewer();

        const float SecondsPixelSizeDefault = 128 * 4;
        public float SecondsPixelSize = SecondsPixelSizeDefault;
        public float SecondsPixelSizeFarAwayModeUpperBound = SecondsPixelSizeDefault;
        public float SecondsPixelSizeMax = (SecondsPixelSizeDefault * 2048);
        public float SecondsPixelSizeScrollNotch = 128;

        public float TimeLineHeight = 24;

        public float BoxSideScrollMarginSize = 4;


        private TaeDragState currentDrag = new TaeDragState();
        private List<TaeDragState> currentMultiDrag = new List<TaeDragState>();


        private void ZoomOutOneNotch(float mouseScreenPosX)
        {
            float mousePointTime = (mouseScreenPosX + ScrollViewer.Scroll.X) / SecondsPixelSize;

            SecondsPixelSize /= 2f;
            if (SecondsPixelSize < 8f)
                SecondsPixelSize = 8f;

            //if (SecondsPixelSize <= SecondsPixelSizeFarAwayModeUpperBound)
            //{
            //    SecondsPixelSize /= 2f;
            //    if (SecondsPixelSize < 8f)
            //        SecondsPixelSize = 8f;
            //}
            //else
            //{
            //    SecondsPixelSize -= SecondsPixelSizeScrollNotch;
            //}

            float newOnscreenOffset = (mousePointTime * SecondsPixelSize) - ScrollViewer.Scroll.X;
            float scrollAmountToCorrectOffset = (newOnscreenOffset - mouseScreenPosX);
            ScrollViewer.ScrollByVirtualScrollUnits(new Vector2(scrollAmountToCorrectOffset, 0));
        }

        private void ZoomInOneNotch(float mouseScreenPosX)
        {
            float mousePointTime = (mouseScreenPosX + ScrollViewer.Scroll.X) / SecondsPixelSize;

            SecondsPixelSize *= 2;

            if (SecondsPixelSize > SecondsPixelSizeMax)
                SecondsPixelSize = SecondsPixelSizeMax;

            //if (SecondsPixelSize < SecondsPixelSizeFarAwayModeUpperBound)
            //{
            //    SecondsPixelSize *= 2;
            //}
            //else
            //{
            //    SecondsPixelSize += SecondsPixelSizeScrollNotch;
            //}

            float newOnscreenOffset = (mousePointTime * SecondsPixelSize) - ScrollViewer.Scroll.X;
            float scrollAmountToCorrectOffset = (newOnscreenOffset - mouseScreenPosX);
            ScrollViewer.ScrollByVirtualScrollUnits(new Vector2(scrollAmountToCorrectOffset, 0));
        }

        private void ResetZoom(float mouseScreenPosX)
        {
            float mousePointTime = (mouseScreenPosX + ScrollViewer.Scroll.X) / SecondsPixelSize;

            SecondsPixelSize = SecondsPixelSizeDefault;

            //if (SecondsPixelSize < SecondsPixelSizeFarAwayModeUpperBound)
            //{
            //    SecondsPixelSize *= 2;
            //}
            //else
            //{
            //    SecondsPixelSize += SecondsPixelSizeScrollNotch;
            //}

            float newOnscreenOffset = (mousePointTime * SecondsPixelSize) - ScrollViewer.Scroll.X;
            float scrollAmountToCorrectOffset = (newOnscreenOffset - mouseScreenPosX);
            ScrollViewer.ScrollByVirtualScrollUnits(new Vector2(scrollAmountToCorrectOffset, 0));
        }

        public void Zoom(float zoomDelta, float mouseScreenPosX)
        {
            var zoomIn = zoomDelta >= 0;
            var zoomAmt = Math.Abs(zoomDelta);

            for (int i = 0; i < zoomAmt; i++)
            {
                if (zoomIn)
                    ZoomInOneNotch(mouseScreenPosX);
                else
                    ZoomOutOneNotch(mouseScreenPosX);
            }
        }

        public float RowHeight = 24;

        private Dictionary<int, List<TaeEditAnimEventBox>> sortedByRow = new Dictionary<int, List<TaeEditAnimEventBox>>();

        private Vector2 relMouse => new Vector2(
            MainScreen.Input.MousePosition.X - Rect.X + ScrollViewer.Scroll.X, 
            MainScreen.Input.MousePosition.Y - Rect.Y + ScrollViewer.Scroll.Y - TimeLineHeight);

        private int MouseRow = 0;

        private List<TaeEditAnimEventBox> GetRow(int row)
        {
            if (sortedByRow.ContainsKey(row))
                return sortedByRow[row];
            else return new List<TaeEditAnimEventBox>();
        }

        public void RegisterEventBoxExistance(TaeEditAnimEventBox box)
        {
            if (!EventBoxes.Contains(box))
                EventBoxes.Add(box);

            if (!sortedByRow.ContainsKey(box.MyEvent.Row))
                sortedByRow.Add(box.MyEvent.Row, new List<TaeEditAnimEventBox>());

            sortedByRow[box.MyEvent.Row].Add(box);
        }

        public void ChangeToNewAnimRef(AnimationRef newAnimRef)
        {
            foreach (var box in EventBoxes)
                box.RowChanged -= Box_RowChanged;

            EventBoxes.Clear();
            sortedByRow.Clear();
            EventTypeColorInfo.Clear();
            AnimRef = newAnimRef;

            void RegisterBoxToRow(TaeEditAnimEventBox box, int row)
            {
                if (sortedByRow.ContainsKey(row))
                    sortedByRow[row].Add(box);
                else
                    sortedByRow.Add(row, new List<TaeEditAnimEventBox> { box });
            }

            int currentRow = 0;
            float farthestRightOnCurrentRow = 0;
            foreach (var ev in AnimRef.EventList)
            {

                if (ev is MeowDSIO.DataTypes.TAE.Events.Tae016_SetEventEditorColors colorEvent)
                {
                    if (!EventTypeColorInfo.ContainsKey(colorEvent.EventKind))
                        EventTypeColorInfo.Add(colorEvent.EventKind, colorEvent);
                    //continue;
                }

                var newBox = new TaeEditAnimEventBox(this, ev);

                if (ev.Row >= 0)
                {
                    currentRow = ev.Row;
                    farthestRightOnCurrentRow = newBox.RightFr;
                }
                else
                {
                    newBox.MyEvent.Row = currentRow;

                    if (newBox.LeftFr < farthestRightOnCurrentRow)
                    {
                        newBox.MyEvent.Row++;
                        currentRow++;
                        farthestRightOnCurrentRow = newBox.RightFr;
                    }
                    else
                    {
                        if (newBox.RightFr > farthestRightOnCurrentRow)
                            farthestRightOnCurrentRow = newBox.RightFr;
                    }
                }

                newBox.RowChanged += Box_RowChanged;

                RegisterBoxToRow(newBox, currentRow);
                EventBoxes.Add(newBox);
            }
        }

        public TaeEditAnimEventGraph(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;
            ChangeToNewAnimRef(MainScreen.SelectedTaeAnim);
        }

        private void Box_RowChanged(object sender, int e)
        {
            var box = (TaeEditAnimEventBox)sender;
            if (sortedByRow.ContainsKey(e) && sortedByRow[e].Contains(box))
                sortedByRow[e].Remove(box);
            if (!sortedByRow.ContainsKey(box.MyEvent.Row))
                sortedByRow.Add(box.MyEvent.Row, new List<TaeEditAnimEventBox>());
            if (!sortedByRow[box.MyEvent.Row].Contains(box))
                sortedByRow[box.MyEvent.Row].Add(box);
        }

        public void DeleteMultipleEventBoxes(IEnumerable<TaeEditAnimEventBox> boxes)
        {
            var copyOfBoxes = new List<TaeEditAnimEventBox>();

            foreach (var box in boxes)
            {
                if (box.OwnerPane != this)
                {
                    throw new ArgumentException($"This {nameof(TaeEditAnimEventGraph)} can only " +
                    $"delete {nameof(TaeEditAnimEventBox)}'s that it owns!", nameof(boxes));
                }

                copyOfBoxes.Add(box);
            }

            MainScreen.UndoMan.NewAction(
                doAction: () =>
                {
                    foreach (var box in copyOfBoxes)
                    {
                        if (MainScreen.SelectedEventBox == box)
                            MainScreen.SelectedEventBox = null;

                        if (MainScreen.MultiSelectedEventBoxes.Contains(box))
                            MainScreen.MultiSelectedEventBoxes.Remove(box);

                        box.RowChanged -= Box_RowChanged;

                        if (sortedByRow.ContainsKey(box.MyEvent.Row))
                            if (sortedByRow[box.MyEvent.Row].Contains(box))
                                sortedByRow[box.MyEvent.Row].Remove(box);

                        AnimRef.EventList.Remove(box.MyEvent);

                        EventBoxes.Remove(box);

                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                    }
                },
                undoAction: () =>
                {
                    MainScreen.MultiSelectedEventBoxes.Clear();
                    foreach (var box in copyOfBoxes)
                    {
                        EventBoxes.Add(box);
                        AnimRef.EventList.Add(box.MyEvent);

                        if (!sortedByRow.ContainsKey(box.MyEvent.Row))
                            sortedByRow.Add(box.MyEvent.Row, new List<TaeEditAnimEventBox>());

                        if (!sortedByRow[box.MyEvent.Row].Contains(box))
                            sortedByRow[box.MyEvent.Row].Add(box);

                        box.RowChanged += Box_RowChanged;

                        if (!MainScreen.MultiSelectedEventBoxes.Contains(box))
                            MainScreen.MultiSelectedEventBoxes.Add(box);

                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                    }
                });
        }

        public void DeleteEventBox(TaeEditAnimEventBox box)
        {
            if (box.OwnerPane != this)
            {
                throw new ArgumentException($"This {nameof(TaeEditAnimEventGraph)} can only " +
                    $"delete {nameof(TaeEditAnimEventBox)}'s that it owns!", nameof(box));
            }

            MainScreen.UndoMan.NewAction(
                doAction: () =>
                {
                    if (MainScreen.SelectedEventBox == box)
                        MainScreen.SelectedEventBox = null;
                    box.RowChanged -= Box_RowChanged;

                    if (sortedByRow.ContainsKey(box.MyEvent.Row))
                        if (sortedByRow[box.MyEvent.Row].Contains(box))
                            sortedByRow[box.MyEvent.Row].Remove(box);

                    AnimRef.EventList.Remove(box.MyEvent);

                    EventBoxes.Remove(box);

                    AnimRef.IsModified = true;
                    MainScreen.IsModified = true;
                },
                undoAction: () =>
                {
                    EventBoxes.Add(box);
                    AnimRef.EventList.Add(box.MyEvent);

                    if (!sortedByRow.ContainsKey(box.MyEvent.Row))
                        sortedByRow.Add(box.MyEvent.Row, new List<TaeEditAnimEventBox>());

                    if (!sortedByRow[box.MyEvent.Row].Contains(box))
                        sortedByRow[box.MyEvent.Row].Add(box);

                    box.RowChanged += Box_RowChanged;

                    MainScreen.SelectedEventBox = box;

                    AnimRef.IsModified = true;
                    MainScreen.IsModified = true;
                });
        }

        private void PlaceNewEventAtMouse()
        {
            float mouseTime = ((MainScreen.Input.MousePosition.X - Rect.X + ScrollViewer.Scroll.X) / SecondsPixelSize);

            TimeActEventBase newEvent = null;

            if (MainScreen.SelectedEventBox != null)
            {
                newEvent = TimeActEventBase.GetNewEvent(
                    MainScreen.SelectedEventBox.MyEvent.EventType,
                    mouseTime, mouseTime + (MainScreen.SelectedEventBox.MyEvent.EndTimeFr 
                    - MainScreen.SelectedEventBox.MyEvent.StartTimeFr));

                TimeActEventBase.CopyEventParameters(MainScreen.SelectedEventBox.MyEvent, newEvent);
            }
            else
            {
                newEvent = TimeActEventBase.GetNewEvent(
                    TimeActEventType.DoCommand,
                    mouseTime, mouseTime + 1);
            }

            newEvent.Index = MainScreen.SelectedTaeAnim.EventList.Count;

            var newBox = new TaeEditAnimEventBox(this, newEvent);

            newBox.MyEvent.Row = MouseRow;

            MainScreen.UndoMan.NewAction(
                doAction: () =>
                {
                    MainScreen.SelectedTaeAnim.EventList.Add(newEvent);

                    if (!sortedByRow.ContainsKey(newBox.MyEvent.Row))
                        sortedByRow.Add(newBox.MyEvent.Row, new List<TaeEditAnimEventBox>());

                    sortedByRow[newBox.MyEvent.Row].Add(newBox);

                    EventBoxes.Add(newBox);

                    AnimRef.IsModified = true;
                    MainScreen.IsModified = true;
                },
                undoAction: () =>
                {
                    EventBoxes.Remove(newBox);

                    if (sortedByRow.ContainsKey(newBox.MyEvent.Row))
                        if (sortedByRow[newBox.MyEvent.Row].Contains(newBox))
                            sortedByRow[newBox.MyEvent.Row].Remove(newBox);

                    MainScreen.SelectedTaeAnim.EventList.Remove(newEvent);

                    AnimRef.IsModified = true;
                    MainScreen.IsModified = true;
                });

            
        }

        private void DeleteSelectedEvent()
        {
            if (MainScreen.MultiSelectedEventBoxes.Count > 0)
            {
                DeleteMultipleEventBoxes(MainScreen.MultiSelectedEventBoxes);
            }
            else if (MainScreen.SelectedEventBox != null)
            {
                DeleteEventBox(MainScreen.SelectedEventBox);
            }
        }

        public void Update(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            var ctrlHeld = MainScreen.Input.KeyHeld(Keys.LeftControl) || MainScreen.Input.KeyHeld(Keys.RightControl);
            var shiftHeld = MainScreen.Input.KeyHeld(Keys.LeftShift) || MainScreen.Input.KeyHeld(Keys.RightShift);
            var altHeld = MainScreen.Input.KeyHeld(Keys.LeftAlt) || MainScreen.Input.KeyHeld(Keys.RightAlt);

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: !ctrlHeld);

            if (ctrlHeld && !shiftHeld && !altHeld)
            {
                if (MainScreen.Input.KeyDown(Keys.OemPlus))
                {
                    ZoomInOneNotch(0);
                }
                else if (MainScreen.Input.KeyDown(Keys.OemMinus))
                {
                    ZoomOutOneNotch(0);
                }
                else if (MainScreen.Input.KeyDown(Keys.D0) || MainScreen.Input.KeyDown(Keys.NumPad0))
                {
                    ResetZoom(0);
                }
                else if (MainScreen.Input.KeyDown(Keys.A))
                {
                    if (currentDrag.DragType == BoxDragType.None)
                    {
                        MainScreen.SelectedEventBox = null;
                        MainScreen.MultiSelectedEventBoxes.Clear();
                        foreach (var box in EventBoxes)
                        {
                            MainScreen.MultiSelectedEventBoxes.Add(box);
                        }
                        MainScreen.UpdateInspectorToSelection();
                    }
                }
            }

            if (!ctrlHeld && shiftHeld && !altHeld)
            {
                if (MainScreen.Input.KeyDown(Keys.D))
                {
                    if (MainScreen.SelectedEventBox != null)
                        MainScreen.SelectedEventBox = null;
                    if (MainScreen.MultiSelectedEventBoxes.Count > 0)
                        MainScreen.MultiSelectedEventBoxes.Clear();
                }
            }

            if (MainScreen.Input.KeyDown(Keys.Delete))
            {
                DeleteSelectedEvent();
            }

            if (ScrollViewer.Viewport.Contains(new Point((int)MainScreen.Input.MousePosition.X, (int)MainScreen.Input.MousePosition.Y)))
            {
                MouseRow = (int)(relMouse.Y / RowHeight);

                MainScreen.Input.CursorType = MouseCursorType.Arrow;

                if (MainScreen.Input.LeftClickDown)
                {
                    MainScreen.SelectedEventBox = null;
                }

                if (MainScreen.Input.RightClickDown &&
                    currentDrag.DragType == BoxDragType.None)
                {
                    PlaceNewEventAtMouse();
                }

                IEnumerable<TaeEditAnimEventBox> masterRowBoxList = GetRow(MouseRow);
                var rowOrderedByTime = masterRowBoxList.OrderByDescending(x => x.MyEvent.StartTime);

                MainScreen.HoveringOverEventBox = null;

                foreach (var box in rowOrderedByTime)
                {
                    bool canManipulateBox = (MainScreen.MultiSelectedEventBoxes.Count == 0
                        || MainScreen.MultiSelectedEventBoxes.Contains(box));

                    if (currentDrag.DragType == BoxDragType.None)
                    {
                        if (canManipulateBox && box.WidthFr >= 16 && 
                            relMouse.X <= box.LeftFr + BoxSideScrollMarginSize && 
                            relMouse.X >= box.LeftFr - BoxSideScrollMarginSize)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            if (MainScreen.Input.LeftClickDown)
                            {
                                if (MainScreen.MultiSelectedEventBoxes.Count == 0)
                                {
                                    currentMultiDrag.Clear();

                                    currentDrag.DragType = BoxDragType.LeftOfEventBox;
                                    currentDrag.Box = box;
                                    currentDrag.Offset = new Point((int)(relMouse.X - box.LeftFr), (int)(relMouse.Y - box.Top));
                                    currentDrag.BoxOriginalWidth = box.Width;
                                    currentDrag.BoxOriginalStart = box.MyEvent.StartTime;
                                    currentDrag.BoxOriginalEnd = box.MyEvent.EndTime;
                                    currentDrag.BoxOriginalRow = box.MyEvent.Row;
                                    currentDrag.StartMouseRow = MouseRow;
                                    currentDrag.StartDragPoint = currentDrag.CurrentDragPoint = relMouse.ToPoint();
                                }
                                else
                                {
                                    currentDrag.DragType = BoxDragType.MultiDragLeftOfEventBox;
                                    currentMultiDrag.Clear();
                                    foreach (var multiBox in MainScreen.MultiSelectedEventBoxes)
                                    {
                                        var newDrag = new TaeDragState();

                                        newDrag.DragType = BoxDragType.LeftOfEventBox;
                                        newDrag.Box = multiBox;
                                        newDrag.Offset = new Point((int)(relMouse.X - multiBox.LeftFr), (int)(relMouse.Y - multiBox.Top));
                                        newDrag.BoxOriginalWidth = multiBox.Width;
                                        newDrag.BoxOriginalStart = multiBox.MyEvent.StartTime;
                                        newDrag.BoxOriginalEnd = multiBox.MyEvent.EndTime;
                                        newDrag.BoxOriginalRow = multiBox.MyEvent.Row;
                                        newDrag.StartMouseRow = MouseRow;
                                        newDrag.StartDragPoint = newDrag.CurrentDragPoint = relMouse.ToPoint();

                                        currentMultiDrag.Add(newDrag);
                                    }
                                }
                                
                            }
                        }
                        else if (canManipulateBox && 
                            box.WidthFr >= 16 && 
                            relMouse.X >= box.RightFr - BoxSideScrollMarginSize && 
                            relMouse.X <= box.RightFr + BoxSideScrollMarginSize)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            if (MainScreen.Input.LeftClickDown)
                            {
                                if (MainScreen.MultiSelectedEventBoxes.Count == 0)
                                {
                                    currentMultiDrag.Clear();

                                    currentDrag.DragType = BoxDragType.RightOfEventBox;
                                    currentDrag.Box = box;
                                    currentDrag.Offset = new Point(
                                        (int)(relMouse.X - box.LeftFr), (int)(relMouse.Y - box.Top));
                                    currentDrag.BoxOriginalWidth = box.Width;
                                    currentDrag.BoxOriginalStart = box.MyEvent.StartTime;
                                    currentDrag.BoxOriginalEnd = box.MyEvent.EndTime;
                                    currentDrag.BoxOriginalRow = box.MyEvent.Row;
                                    currentDrag.StartMouseRow = MouseRow;
                                    currentDrag.StartDragPoint =
                                        currentDrag.CurrentDragPoint = relMouse.ToPoint();
                                }
                                else
                                {
                                    currentDrag.DragType = BoxDragType.MultiDragRightOfEventBox;
                                    currentMultiDrag.Clear();
                                    foreach (var multiBox in MainScreen.MultiSelectedEventBoxes)
                                    {
                                        var newDrag = new TaeDragState();

                                        newDrag.DragType = BoxDragType.RightOfEventBox;
                                        newDrag.Box = multiBox;
                                        newDrag.Offset = new Point(
                                            (int)(relMouse.X - multiBox.LeftFr), 
                                            (int)(relMouse.Y - multiBox.Top));
                                        newDrag.BoxOriginalWidth = multiBox.Width;
                                        newDrag.BoxOriginalStart = multiBox.MyEvent.StartTime;
                                        newDrag.BoxOriginalEnd = multiBox.MyEvent.EndTime;
                                        newDrag.BoxOriginalRow = multiBox.MyEvent.Row;
                                        newDrag.StartMouseRow = MouseRow;
                                        newDrag.StartDragPoint =
                                            newDrag.CurrentDragPoint = relMouse.ToPoint();

                                        currentMultiDrag.Add(newDrag);
                                    }
                                }
                                
                            }
                        }
                        else if (relMouse.X >= box.LeftFr && relMouse.X < box.RightFr)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.Arrow;
                            MainScreen.HoveringOverEventBox = box;
                            if (MainScreen.Input.LeftClickDown)
                            {
                                if (MainScreen.MultiSelectedEventBoxes.Count == 0)
                                {
                                    currentDrag.DragType = BoxDragType.MiddleOfEventBox;
                                    currentDrag.Box = box;
                                    currentDrag.Offset = new Point((int)(relMouse.X - box.LeftFr), (int)(relMouse.Y - box.Top));
                                    currentDrag.BoxOriginalWidth = box.Width;
                                    currentDrag.BoxOriginalStart = box.MyEvent.StartTime;
                                    currentDrag.BoxOriginalEnd = box.MyEvent.EndTime;
                                    currentDrag.BoxOriginalRow = box.MyEvent.Row;
                                    currentDrag.StartMouseRow = MouseRow;
                                    currentDrag.StartDragPoint = currentDrag.CurrentDragPoint = relMouse.ToPoint();
                                }
                                else
                                {
                                    if (MainScreen.MultiSelectedEventBoxes.Contains(box))
                                    {
                                        currentDrag.DragType = BoxDragType.MultiDragMiddleOfEventBox;
                                        currentMultiDrag.Clear();
                                        foreach (var multiBox in MainScreen.MultiSelectedEventBoxes)
                                        {
                                            var newDrag = new TaeDragState();

                                            newDrag.DragType = BoxDragType.MiddleOfEventBox;
                                            newDrag.Box = multiBox;
                                            newDrag.Offset = new Point((int)(relMouse.X - multiBox.LeftFr), (int)(relMouse.Y - multiBox.Top));
                                            newDrag.BoxOriginalWidth = multiBox.Width;
                                            newDrag.BoxOriginalStart = multiBox.MyEvent.StartTime;
                                            newDrag.BoxOriginalEnd = multiBox.MyEvent.EndTime;
                                            newDrag.BoxOriginalRow = multiBox.MyEvent.Row;
                                            newDrag.StartMouseRow = MouseRow;
                                            newDrag.StartDragPoint = newDrag.CurrentDragPoint = relMouse.ToPoint();

                                            currentMultiDrag.Add(newDrag);
                                        }
                                    }
                                    else
                                    {
                                        MainScreen.MultiSelectedEventBoxes.Clear();
                                        MainScreen.SelectedEventBox = box;
                                        MainScreen.UpdateInspectorToSelection();
                                    }
                                }
                            }
                        }
                    }

                    var isSingleSelect = (currentDrag.DragType == BoxDragType.None) ||
                        (currentDrag.DragType == BoxDragType.MiddleOfEventBox && currentDrag.StartDragPoint == currentDrag.CurrentDragPoint);


                    if (isSingleSelect && MainScreen.Input.LeftClickDown)
                    {
                        if (relMouse.X >= box.LeftFr && relMouse.X < box.RightFr)
                        {
                            MainScreen.SelectedEventBox = box;

                            currentMultiDrag.Clear();
                            MainScreen.MultiSelectedEventBoxes.Clear();

                            MainScreen.UpdateInspectorToSelection();

                            break;
                        }

                    }

                }



                if (MainScreen.SelectedEventBox == null && 
                    currentDrag.DragType == BoxDragType.None && 
                    MainScreen.Input.LeftClickDown)
                {
                    currentDrag.DragType = BoxDragType.MultiSelectionRectangle;
                    currentDrag.StartDragPoint = relMouse.ToPoint();
                }

                if (MainScreen.Input.LeftClickHeld)
                {
                    currentDrag.CurrentDragPoint = relMouse.ToPoint();

                    if (currentDrag.DragType == BoxDragType.LeftOfEventBox && currentDrag.Box != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.DragX;
                        currentDrag.DragBoxToMouse(relMouse.ToPoint());
                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                        //currentDrag.Box.DragLeftSide(MainScreen.Input.MousePositionDelta.X);
                    }
                    else if (currentDrag.DragType == BoxDragType.RightOfEventBox && currentDrag.Box != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.DragX;
                        currentDrag.DragBoxToMouse(relMouse.ToPoint());
                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                        //currentDrag.Box.DragRightSide(MainScreen.Input.MousePositionDelta.X);
                    }
                    else if (currentDrag.DragType == BoxDragType.MiddleOfEventBox && currentDrag.Box != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.Arrow;
                        currentDrag.DragBoxToMouse(relMouse.ToPoint());
                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                        //currentDrag.Box.DragMiddle(MainScreen.Input.MousePositionDelta.X);
                        currentDrag.ShiftBoxRow(MouseRow);
                    }
                    else if (currentDrag.DragType == BoxDragType.MultiDragLeftOfEventBox)
                    {
                        var earliestEndingDrag = currentMultiDrag.OrderBy(x => x.BoxOriginalEnd).First();
                        int mouseMaxX = MathHelper.Max((int)earliestEndingDrag.StartDragPoint.X, (int)((earliestEndingDrag.BoxOriginalEnd * SecondsPixelSize) - (TimeActEventBase.FRAME * SecondsPixelSize)));

                        var earliestDrag_preventNegative = currentMultiDrag.OrderBy(x => x.BoxOriginalStart).First();
                        int mouseMinX_preventNegative = earliestDrag_preventNegative.Offset.X + (int)earliestDrag_preventNegative.BoxOriginalStart;

                        var actualMousePoint = new Point(MathHelper.Max(MathHelper.Min((int)relMouse.X, mouseMaxX), mouseMinX_preventNegative), (int)(relMouse.Y));

                        foreach (var multiDrag in currentMultiDrag)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            multiDrag.DragBoxToMouse(actualMousePoint);
                            AnimRef.IsModified = true;
                            MainScreen.IsModified = true;
                        }
                    }
                    else if (currentDrag.DragType == BoxDragType.MultiDragRightOfEventBox)
                    {
                        var shortestDrag = currentMultiDrag.OrderBy(x => x.BoxOriginalDuration).First();
                        int mouseMinX = shortestDrag.StartDragPoint.X - (int)((shortestDrag.BoxOriginalDuration * SecondsPixelSize));
                        var actualMousePoint = new Point(MathHelper.Max((int)relMouse.X, mouseMinX), (int)(relMouse.Y));

                        foreach (var multiDrag in currentMultiDrag)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            multiDrag.DragBoxToMouse(actualMousePoint);
                            AnimRef.IsModified = true;
                            MainScreen.IsModified = true;
                        }
                    }
                    else if (currentDrag.DragType == BoxDragType.MultiDragMiddleOfEventBox)
                    {
                        var highestDragRow = currentMultiDrag.OrderBy(x => x.BoxOriginalRow).First();
                        var minimumMouseRow = (highestDragRow.StartMouseRow - highestDragRow.BoxOriginalRow);

                        var earliestDrag = currentMultiDrag.OrderBy(x => x.BoxOriginalStart).First();
                        int mouseMinX = earliestDrag.Offset.X + (int)earliestDrag.BoxOriginalStart;
                        var actualMousePoint = new Point(MathHelper.Max((int)relMouse.X, mouseMinX), (int)(relMouse.Y));

                        foreach (var multiDrag in currentMultiDrag)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.Arrow;
                            multiDrag.DragBoxToMouse(actualMousePoint);
                            AnimRef.IsModified = true;
                            MainScreen.IsModified = true;
                            multiDrag.ShiftBoxRow(MathHelper.Max(MouseRow, minimumMouseRow));
                        }
                    }
                    else if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle)
                    {
                        MainScreen.MultiSelectedEventBoxes.Clear();
                        var dragRect = currentDrag.GetVirtualDragRect();
                        int firstRow = (int)(dragRect.Top / RowHeight) - 1;
                        int lastRow = (int)(dragRect.Bottom / RowHeight) + 1;
                        for (int i = firstRow; i <= lastRow; i++)
                        {
                            if (sortedByRow.ContainsKey(i))
                            {
                                foreach (var box in sortedByRow[i])
                                {
                                    var boxRect = new Rectangle((int)box.Left, (int)(box.Top - TimeLineHeight), (int)box.WidthFr, (int)box.HeightFr);
                                    if (boxRect.Intersects(dragRect))
                                    {
                                        MainScreen.MultiSelectedEventBoxes.Add(box);
                                    }
                                }
                            }
                        }
                        MainScreen.SelectedEventBox = null;
                        MainScreen.UpdateInspectorToSelection();
                    }
                }
                else
                {
                    if (currentDrag.DragType != BoxDragType.None)
                    {
                        if (currentDrag.DragType == BoxDragType.LeftOfEventBox ||
                        currentDrag.DragType == BoxDragType.RightOfEventBox ||
                        currentDrag.DragType == BoxDragType.MiddleOfEventBox)
                        {
                            TaeEditAnimEventBox copyOfBox = currentDrag.Box;

                            float copyOfOldBoxStart = currentDrag.BoxOriginalStart;
                            float copyOfOldBoxEnd = currentDrag.BoxOriginalEnd;
                            int copyOfOldBoxRow = currentDrag.BoxOriginalRow;

                            float copyOfCurrentBoxStart = copyOfBox.MyEvent.StartTime;
                            float copyOfCurrentBoxEnd = copyOfBox.MyEvent.EndTime;
                            int copyOfCurrentBoxRow = copyOfBox.MyEvent.Row;

                            MainScreen.UndoMan.NewAction(
                                doAction: () =>
                                {
                                    copyOfBox.MyEvent.StartTime = copyOfCurrentBoxStart;
                                    copyOfBox.MyEvent.EndTime = copyOfCurrentBoxEnd;
                                    copyOfBox.MyEvent.Row = copyOfCurrentBoxRow;

                                    MainScreen.IsModified = true;
                                    MainScreen.SelectedTaeAnim.IsModified = true;
                                },
                                undoAction: () =>
                                {
                                    copyOfBox.MyEvent.StartTime = copyOfOldBoxStart;
                                    copyOfBox.MyEvent.EndTime = copyOfOldBoxEnd;
                                    copyOfBox.MyEvent.Row = copyOfOldBoxRow;

                                    MainScreen.IsModified = true;
                                    MainScreen.SelectedTaeAnim.IsModified = true;
                                });

                            currentDrag.DragType = BoxDragType.None;
                            currentDrag.Box = null;
                        }
                        else if (currentDrag.DragType == BoxDragType.MultiDragLeftOfEventBox ||
                            currentDrag.DragType == BoxDragType.MultiDragRightOfEventBox ||
                            currentDrag.DragType == BoxDragType.MultiDragMiddleOfEventBox)
                        {
                            List<TaeEditAnimEventBox> copiesOfBox = new List<TaeEditAnimEventBox>();

                            List<float> copiesOfOldBoxStart = new List<float>();
                            List<float> copiesOfOldBoxEnd = new List<float>();
                            List<int> copiesOfOldBoxRow = new List<int>();

                            List<float> copiesOfCurrentBoxStart = new List<float>();
                            List<float> copiesOfCurrentBoxEnd = new List<float>();
                            List<int> copiesOfCurrentBoxRow = new List<int>();

                            foreach (var multiDrag in currentMultiDrag)
                            {
                                copiesOfBox.Add(multiDrag.Box);

                                copiesOfOldBoxStart.Add(multiDrag.BoxOriginalStart);
                                copiesOfOldBoxEnd.Add(multiDrag.BoxOriginalEnd);
                                copiesOfOldBoxRow.Add(multiDrag.BoxOriginalRow);

                                copiesOfCurrentBoxStart.Add(multiDrag.Box.MyEvent.StartTime);
                                copiesOfCurrentBoxEnd.Add(multiDrag.Box.MyEvent.EndTime);
                                copiesOfCurrentBoxRow.Add(multiDrag.Box.MyEvent.Row);
                            }

                            MainScreen.UndoMan.NewAction(
                                    doAction: () =>
                                    {
                                        for (int i = 0; i < copiesOfBox.Count; i++)
                                        {
                                            copiesOfBox[i].MyEvent.StartTime = copiesOfCurrentBoxStart[i];
                                            copiesOfBox[i].MyEvent.EndTime = copiesOfCurrentBoxEnd[i];
                                            copiesOfBox[i].MyEvent.Row = copiesOfCurrentBoxRow[i];
                                        }

                                        MainScreen.IsModified = true;
                                        MainScreen.SelectedTaeAnim.IsModified = true;
                                    },
                                    undoAction: () =>
                                    {
                                        for (int i = 0; i < copiesOfBox.Count; i++)
                                        {
                                            copiesOfBox[i].MyEvent.StartTime = copiesOfOldBoxStart[i];
                                            copiesOfBox[i].MyEvent.EndTime = copiesOfOldBoxEnd[i];
                                            copiesOfBox[i].MyEvent.Row = copiesOfOldBoxRow[i];
                                        }

                                        MainScreen.IsModified = true;
                                        MainScreen.SelectedTaeAnim.IsModified = true;
                                    });


                            //foreach (var multiDrag in currentMultiDrag)
                            //{
                            //    TaeEditAnimEventBox copyOfBox = multiDrag.Box;

                            //    float copyOfOldBoxStart = multiDrag.BoxOriginalStart;
                            //    float copyOfOldBoxEnd = multiDrag.BoxOriginalEnd;
                            //    int copyOfOldBoxRow = multiDrag.BoxOriginalRow;

                            //    float copyOfCurrentBoxStart = copyOfBox.MyEvent.StartTime;
                            //    float copyOfCurrentBoxEnd = copyOfBox.MyEvent.EndTime;
                            //    int copyOfCurrentBoxRow = copyOfBox.MyEvent.Row;

                            //    MainScreen.UndoMan.NewAction(
                            //        doAction: () =>
                            //        {
                            //            copyOfBox.MyEvent.StartTime = copyOfCurrentBoxStart;
                            //            copyOfBox.MyEvent.EndTime = copyOfCurrentBoxEnd;
                            //            copyOfBox.MyEvent.Row = copyOfCurrentBoxRow;

                            //            MainScreen.IsModified = true;
                            //            MainScreen.SelectedTaeAnim.IsModified = true;
                            //        },
                            //        undoAction: () =>
                            //        {
                            //            copyOfBox.MyEvent.StartTime = copyOfOldBoxStart;
                            //            copyOfBox.MyEvent.EndTime = copyOfOldBoxEnd;
                            //            copyOfBox.MyEvent.Row = copyOfOldBoxRow;

                            //            MainScreen.IsModified = true;
                            //            MainScreen.SelectedTaeAnim.IsModified = true;
                            //        });

                            //    multiDrag.DragType = BoxDragType.None;
                            //    multiDrag.Box = null;
                            //}

                            currentMultiDrag.Clear();
                            currentDrag.DragType = BoxDragType.None;
                        }
                        else if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle)
                        {
                            currentDrag.DragType = BoxDragType.None;
                        }

                        
                    }
                }

                if (ctrlHeld)
                {
                    Zoom(MainScreen.Input.ScrollDelta, MainScreen.Input.MousePosition.X - Rect.X);
                }
            }

            
        }

        public void UpdateMouseOutsideRect(float elapsedSeconds, bool allowMouseUpdate)
        {
            MouseRow = -1;
            if (!allowMouseUpdate)
            {
                if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle)
                {
                    currentDrag.DragType = BoxDragType.None;
                }

                return;
            }
                

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: false);
        }

        private Point GetVirtualAreaSize()
        {
            if (EventBoxes.Count == 0)
                return Point.Zero;

            return new Point(
                (int)EventBoxes.OrderByDescending(x => x.MyEvent.EndTime).First().Right + 64, 
                (int)EventBoxes.OrderByDescending(x => x.MyEvent.Row).First().Bottom + 64);
        }

        private Dictionary<int, float> GetSecondVerticalLineXPositions()
        {
            var result = new Dictionary<int, float>();
            float startTimeSeconds = ScrollViewer.Scroll.X / SecondsPixelSize;
            float endTimeSeconds = (ScrollViewer.Scroll.X + ScrollViewer.Viewport.Width) / SecondsPixelSize;

            for (int i = (int)startTimeSeconds; i <= (int)endTimeSeconds; i++)
            {
                result.Add(i, (i * SecondsPixelSize));
            }

            return result;
        }

        private Dictionary<int, float> GetRowHorizontalLineYPositions()
        {
            var result = new Dictionary<int, float>();
            float startRow = ScrollViewer.Scroll.Y / RowHeight;
            float endRow = (ScrollViewer.Scroll.Y + ScrollViewer.Viewport.Height) / RowHeight;

            for (int i = (int)startRow; i <= (int)endRow; i++)
            {
                result.Add(i, (i * RowHeight));
            }

            return result;
        }

        private void DrawTimeLine(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, 
            SpriteFont font, Dictionary<int, float> secondVerticalLineXPositions)
        {
            foreach (var kvp in secondVerticalLineXPositions)
            {
                if (!MainScreen.Config.EnableColorBlindMode)
                    sb.DrawString(font, $"{(kvp.Key * 30)}", new Vector2(kvp.Value + 4 + 1, (int)ScrollViewer.Scroll.Y + 1 + 1), Color.Black);
                sb.DrawString(font, $"{(kvp.Key * 30)}", new Vector2(kvp.Value + 4, (int)ScrollViewer.Scroll.Y + 1), MainScreen.Config.EnableColorBlindMode ? Color.Black : Color.White);
            }
        }

        public void Draw(GameTime gt, GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font, float elapsedSeconds)
        {
            ScrollViewer.SetDisplayRect(Rect, GetVirtualAreaSize());

            ScrollViewer.Draw(gd, sb, boxTex, font);

            var scrollMatrix = ScrollViewer.GetScrollMatrix();

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport);
            {
                sb.Begin(transformMatrix: scrollMatrix);

                sb.Draw(texture: boxTex,
                    position: ScrollViewer.Scroll - (Vector2.One * 2),
                    sourceRectangle: null,
                    color: MainScreen.Config.EnableColorBlindMode ? Color.Gray : Color.DimGray,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(ScrollViewer.Viewport.Width, ScrollViewer.Viewport.Height) + (Vector2.One * 4),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );

                var rowHorizontalLineYPositions = GetRowHorizontalLineYPositions();

                foreach (var kvp in rowHorizontalLineYPositions)
                {
                    sb.Draw(texture: boxTex,
                    position: new Vector2(ScrollViewer.Scroll.X, kvp.Value),
                    sourceRectangle: null,
                    color: MainScreen.Config.EnableColorBlindMode ? Color.White * 0.5f : Color.Black * 0.25f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(ScrollViewer.Viewport.Width, 1),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );
                }

                foreach (var kvp in sortedByRow)
                {
                    var boxesOrderedByTime = kvp.Value.OrderBy(x => x.MyEvent.StartTime);
                    foreach (var box in boxesOrderedByTime)
                    {
                        var isBoxSelected = (MainScreen.SelectedEventBox == box) || (MainScreen.MultiSelectedEventBoxes.Contains(box));

                        if (isBoxSelected)
                            box.UpdateEventText();

                        if (box.LeftFr > ScrollViewer.RelativeViewport.Right
                            || box.RightFr < ScrollViewer.RelativeViewport.Left)
                            continue;

                        bool eventStartsBeforeScreen = box.LeftFr < ScrollViewer.Scroll.X;

                        Vector2 pos = new Vector2(box.LeftFr, box.Top);
                        Vector2 size = new Vector2(box.WidthFr, box.HeightFr);

                        int boxOutlineThickness = isBoxSelected ? 2 : 1;

                        Color textFG = Color.White;
                        Color textBG = Color.Black;

                        Color boxOutlineColor = Color.Black;

                        Color thisBoxBgColor = new Color(box.ColorBG.ToVector3() * 0.4f);
                        Color thisBoxBgColorSelected = new Color(box.ColorBG.ToVector3() * 0.8f);
                        Color boxOutlineColorSelected = Color.White;
                        

                        if (MainScreen.Config.EnableColorBlindMode)
                        {
                            thisBoxBgColor = Color.Black;
                            thisBoxBgColorSelected = Color.White;
                            boxOutlineColor = Color.Gray;
                            boxOutlineColorSelected = Color.Black;
                            textFG = isBoxSelected ? Color.Black : Color.White;
                            textBG = Color.Transparent;
                        }

                        sb.Draw(texture: boxTex,
                            position: pos + new Vector2(0, -1),
                            sourceRectangle: null,
                            color: isBoxSelected ? boxOutlineColorSelected : boxOutlineColor,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: size + new Vector2(0, 2),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );

                        

                        sb.Draw(texture: boxTex,
                            position: pos + new Vector2(boxOutlineThickness, boxOutlineThickness - 1),
                            sourceRectangle: null,
                            color: isBoxSelected ? thisBoxBgColorSelected : thisBoxBgColor,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: size + new Vector2(-boxOutlineThickness * 2, (-boxOutlineThickness * 2) + 2),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );

                        var namePos = new Vector2((int)MathHelper.Max(ScrollViewer.Scroll.X, pos.X), (int)pos.Y);

                        const string fixedPrefix = "< ";

                        var boxRect = box.GetTextRect(boxOutlineThickness);

                        if (MainScreen.Config.EnableFancyScrollingStrings && boxRect.Width >= 64)
                        {
                            var fancyTextRect = boxRect;
                            if (eventStartsBeforeScreen)
                            {
                                var fixedPrefixSize = font.MeasureString(fixedPrefix).ToPoint();

                                int amountOutOfScreen = (int)ScrollViewer.Scroll.X - fancyTextRect.X;

                                fancyTextRect = new Rectangle(
                                    fancyTextRect.X + fixedPrefixSize.X + amountOutOfScreen,
                                    fancyTextRect.Y,
                                    fancyTextRect.Width - fixedPrefixSize.X - amountOutOfScreen,
                                    fancyTextRect.Height);

                                var prefixPos = new Vector2((int)ScrollViewer.Scroll.X, boxRect.Y);

                                sb.DrawString(font, fixedPrefix,
                                    prefixPos + Vector2.One, textBG);
                                sb.DrawString(font, fixedPrefix,
                                    prefixPos + (Vector2.One * 2), textBG);
                                sb.DrawString(font, fixedPrefix,
                                    prefixPos, textFG);
                            }

                            box.EventText.TextColor = textFG;
                            box.EventText.TextShadowColor = textBG;
                            box.EventText.ScrollPixelsPerSecond = MainScreen.Config.FancyScrollingStringsScrollSpeed;
                            box.EventText.ScrollingSnapsToPixels = MainScreen.Config.FancyTextScrollSnapsToPixels;

                            if (MainScreen.HoveringOverEventBox == box)
                            {
                                box.EventText.Draw(gd, sb, scrollMatrix, fancyTextRect, font, elapsedSeconds);
                            }
                            else
                            {
                                box.EventText.ResetScroll(startImmediatelyNextTime: true);
                                box.EventText.Draw(gd, sb, scrollMatrix, fancyTextRect, font, 0);
                            }
                        }
                        else
                        {
                            box.EventText.ResetScroll(startImmediatelyNextTime: true);

                            var thicknessOffset = new Vector2(boxOutlineThickness * 2, 0);

                            string fullTextWithPrefix = $"{(eventStartsBeforeScreen ? fixedPrefix : "")}{box.EventText.Text}";

                            var nameSize = font.MeasureString(fullTextWithPrefix);

                            if ((namePos.X + nameSize.X) <= box.RightFr)
                            {
                                sb.DrawString(font, fullTextWithPrefix,
                                    namePos + new Vector2(1, 0) + Vector2.One + thicknessOffset, textBG);
                                sb.DrawString(font, fullTextWithPrefix,
                                    namePos + new Vector2(1, 0) + (Vector2.One * 2) + thicknessOffset, textBG);
                                sb.DrawString(font, fullTextWithPrefix,
                                    namePos + new Vector2(1, 0) + thicknessOffset, textFG);
                            }
                            else
                            {
                                string shortTextWithPrefix = $"{(eventStartsBeforeScreen ? fixedPrefix : "")}" +
                                    $"{((int)box.MyEvent.EventType)}";
                                sb.DrawString(font, shortTextWithPrefix, namePos + (Vector2.One) + thicknessOffset,
                                    textBG, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
                                sb.DrawString(font, shortTextWithPrefix, namePos + (Vector2.One * 2) + thicknessOffset,
                                    textBG, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
                                sb.DrawString(font, shortTextWithPrefix, namePos + thicknessOffset,
                                    textFG, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
                            }
                        }
                    }
                }

                sb.Draw(texture: boxTex,
                       position: new Vector2((int)ScrollViewer.Scroll.X, (int)ScrollViewer.Scroll.Y),
                       sourceRectangle: null,
                       color: MainScreen.Config.EnableColorBlindMode ? Color.White : Color.DarkGray,
                       rotation: 0,
                       origin: Vector2.Zero,
                       scale: new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight),
                       effects: SpriteEffects.None,
                       layerDepth: 0
                       );

                sb.Draw(texture: boxTex,
                            position: new Vector2(ScrollViewer.Scroll.X, TimeLineHeight + MouseRow * RowHeight),
                            sourceRectangle: null,
                            color: Color.LightGray * 0.25f,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: new Vector2(ScrollViewer.Viewport.Width, RowHeight),
                            effects: SpriteEffects.None,
                            layerDepth: 0.01f
                            );

                if (SecondsPixelSize >= 4f)
                {
                    var secondVerticalLineXPositions = GetSecondVerticalLineXPositions();

                    if (SecondsPixelSize >= 32f)
                    {
                        DrawTimeLine(gd, sb, boxTex, font, secondVerticalLineXPositions);
                    }

                    foreach (var kvp in secondVerticalLineXPositions)
                    {
                        sb.Draw(texture: boxTex,
                        position: new Vector2(kvp.Value, ScrollViewer.Scroll.Y),
                        sourceRectangle: null,
                        color: MainScreen.Config.EnableColorBlindMode ? Color.White * 0.5f : Color.Black * 0.25f,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(1, ScrollViewer.Viewport.Height),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );
                    }
                }

                

                if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle)
                {
                    var multiSelectRect = currentDrag.GetVirtualDragRect();

                    // FILL RECT:
                    sb.Draw(texture: boxTex,
                        position: new Vector2(multiSelectRect.Left, multiSelectRect.Top + TimeLineHeight),
                        sourceRectangle: null,
                        color: MultiSelectRectFillColor,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(multiSelectRect.Width, multiSelectRect.Height),
                        effects: SpriteEffects.None,
                        layerDepth: 0.01f
                        );

                    // THICK OUTLINE:

                    //-- LEFT Side
                    sb.Draw(texture: boxTex,
                        position: new Vector2(multiSelectRect.Left, multiSelectRect.Top + TimeLineHeight),
                        sourceRectangle: null,
                        color: MultiSelectRectOutlineColor,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(MultiSelectRectOutlineThickness, multiSelectRect.Height),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );

                    //-- TOP Side
                    sb.Draw(texture: boxTex,
                        position: new Vector2(multiSelectRect.Left, multiSelectRect.Top + TimeLineHeight),
                        sourceRectangle: null,
                        color: MultiSelectRectOutlineColor,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(multiSelectRect.Width, MultiSelectRectOutlineThickness),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );

                    //-- RIGHT Side
                    sb.Draw(texture: boxTex,
                        position: new Vector2(multiSelectRect.Right - MultiSelectRectOutlineThickness, multiSelectRect.Top + TimeLineHeight),
                        sourceRectangle: null,
                        color: MultiSelectRectOutlineColor,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(MultiSelectRectOutlineThickness, multiSelectRect.Height),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );

                    //-- BOTTOM Side
                    sb.Draw(texture: boxTex,
                        position: new Vector2(multiSelectRect.Left, multiSelectRect.Bottom + TimeLineHeight - MultiSelectRectOutlineThickness),
                        sourceRectangle: null,
                        color: MultiSelectRectOutlineColor,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(multiSelectRect.Width, MultiSelectRectOutlineThickness),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );
                }

                sb.End();
            }
            gd.Viewport = oldViewport;
        }
    }


}
