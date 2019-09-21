using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditAnimEventGraph
    {
        public enum BoxDragType
        {
            None,
            LeftOfEventBox,
            RightOfEventBox,
            MiddleOfEventBox,
            MultiSelectionRectangle,
            MultiSelectionRectangleADD,
            MultiSelectionRectangleSUBTRACT,
            MultiDragLeftOfEventBox,
            MultiDragRightOfEventBox,
            MultiDragMiddleOfEventBox,
        }

        public class TaeDragState
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

            public bool DragBoxToMouseAndCheckIsModified(Point mouse)
            {
                if (DragType == BoxDragType.MiddleOfEventBox)
                {
                    return Box.DragWholeBoxToVirtualUnitX(mouse.X - Offset.X);
                }
                else if (DragType == BoxDragType.LeftOfEventBox)
                {
                    return Box.DragLeftSideOfBoxToVirtualUnitX(mouse.X - Offset.X);
                }
                else if (DragType == BoxDragType.RightOfEventBox)
                {
                    return Box.DragRightSideOfBoxToVirtualUnitX(mouse.X - Offset.X + BoxOriginalWidth);
                }

                return false;
            }

            public void ShiftBoxRow(int newMouseRow)
            {
                if (newMouseRow >= 0)
                    Box.Row = BoxOriginalRow + (newMouseRow - StartMouseRow);
            }
        }

        public enum UnselectedMouseDragType
        {
            None,
            EventSelect,
            PlaybackCursorScrub,
            HorizontalScroll,
            VerticalScroll,
        }

        private Dictionary<int, TAE.EventGroup> EventGroupsByRow = new Dictionary<int, TAE.EventGroup>();

        public float ZoomSpeed = 1.25f;

        public float AfterAutoScrollHorizontalMargin = 48;

        public float ScrubLerp = 30;
        public float ScrubScrollSpeed = 8;
        public float ScrubScrollStartMargin = 96;

        UnselectedMouseDragType currentUnselectedMouseDragType = UnselectedMouseDragType.None;
        UnselectedMouseDragType previousUnselectedMouseDragType = UnselectedMouseDragType.None;

        public TaePlaybackCursor PlaybackCursor;

        public TaeViewportInteractor ViewportInteractor;

        public Color PlaybackCursorColor = Color.Black;

        public float PlaybackCursorThickness = 2;

        public int MultiSelectRectOutlineThickness => MainScreen.Config.EnableColorBlindMode ? 4 : 1;
        public Color MultiSelectRectFillColor => Color.LightGray * 0.5f;
        public Color MultiSelectRectOutlineColor => MainScreen.Config.EnableColorBlindMode ? Color.Black : Color.White;

        public readonly TaeEditorScreen MainScreen;
        public TAE.Animation AnimRef { get; private set; }
        public Rectangle Rect;

        public List<TaeEditAnimEventBox> EventBoxes = new List<TaeEditAnimEventBox>();

        public TaeScrollViewer ScrollViewer { get; private set; } = new TaeScrollViewer();

        const float SecondsPixelSizeDefault = 128 * 4;
        public float SecondsPixelSize = SecondsPixelSizeDefault;
        public float FramePixelSize => SecondsPixelSize / 30.0f;
        public float MinPixelsBetweenFramesForHelperLines = 8;
        public float MinPixelsBetweenFramesForFrameNumberText = 24;

        public float SecondsPixelSizeFarAwayModeUpperBound = SecondsPixelSizeDefault;
        public float SecondsPixelSizeMax = (SecondsPixelSizeDefault * 2048);
        public float SecondsPixelSizeScrollNotch = 128;

        public float TimeLineHeight = 24;

        public float BoxSideScrollMarginSize = 4;


        public TaeDragState currentDrag = new TaeDragState();
        public List<TaeDragState> currentMultiDrag = new List<TaeDragState>();


        public void ZoomOutOneNotch(float mouseScreenPosX)
        {
            float mousePointTime = (mouseScreenPosX + ScrollViewer.Scroll.X) / SecondsPixelSize;

            SecondsPixelSize /= ZoomSpeed;
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

        public void ZoomInOneNotch(float mouseScreenPosX)
        {
            float mousePointTime = (mouseScreenPosX + ScrollViewer.Scroll.X) / SecondsPixelSize;

            SecondsPixelSize *= ZoomSpeed;

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

        public void ResetZoom(float mouseScreenPosX)
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

            if (!sortedByRow.ContainsKey(box.Row))
                sortedByRow.Add(box.Row, new List<TaeEditAnimEventBox>());

            sortedByRow[box.Row].Add(box);
        }

        public void ChangeToNewAnimRef(TAE.Animation newAnimRef)
        {
            foreach (var box in EventBoxes)
                box.RowChanged -= Box_RowChanged;

            EventBoxes.Clear();
            sortedByRow.Clear();
            AnimRef = newAnimRef;

            void RegisterBoxToRow(TaeEditAnimEventBox box, int row)
            {
                if (sortedByRow.ContainsKey(row))
                    sortedByRow[row].Add(box);
                else
                    sortedByRow.Add(row, new List<TaeEditAnimEventBox> { box });
            }

            bool legacyRowMode = !(AnimRef.EventGroups != null && AnimRef.EventGroups.Count > 0);

            int currentRow = 0;
            float farthestRightOnCurrentRow = 0;
            int eventIndex = 0;
            foreach (var ev in AnimRef.Events)
            {
                int groupIndex = legacyRowMode ? -1 : AnimRef.EventGroups.IndexOf(AnimRef.EventGroups.FirstOrDefault(eg => eg.Indices.Contains(eventIndex)));

                var newBox = new TaeEditAnimEventBox(this, ev);

                if (newBox.Row >= 0)
                {
                    currentRow = newBox.Row;
                    farthestRightOnCurrentRow = newBox.RightFr;
                }
                else
                {
                    if (legacyRowMode)
                    {
                        newBox.Row = currentRow;

                        if (newBox.LeftFr < farthestRightOnCurrentRow)
                        {
                            newBox.Row++;
                            currentRow++;
                            farthestRightOnCurrentRow = newBox.RightFr;
                        }
                        else
                        {
                            if (newBox.RightFr > farthestRightOnCurrentRow)
                                farthestRightOnCurrentRow = newBox.RightFr;
                        }
                    }
                    else
                    {
                        if (groupIndex >= 0)
                            newBox.Row = currentRow = groupIndex;
                        else
                            newBox.Row = currentRow = 20;
                    }
                    
                }

                newBox.RowChanged += Box_RowChanged;

                RegisterBoxToRow(newBox, newBox.Row);
                EventBoxes.Add(newBox);

                eventIndex++;
            }

            //Console.WriteLine("EventGroups Start");
            //for (int i = 0; i < AnimRef.EventGroups.Count; i++)
            //{
                
            //    Console.WriteLine(AnimRef.EventGroups[i].EventType);
                
            //}
            //Console.WriteLine("EventGroups End");
        }

        public TaeEditAnimEventGraph(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;
            ChangeToNewAnimRef(MainScreen.SelectedTaeAnim);

            PlaybackCursor = new TaePlaybackCursor();

            ViewportInteractor = new TaeViewportInteractor(this);
        }

        private void RemoveEventBoxFromGroups(TaeEditAnimEventBox box)
        {
            if (MainScreen.SelectedTae.Animations.Any(a => a.EventGroups.Count > 0))
            {
                int thisIndex = MainScreen.SelectedTaeAnim.Events.IndexOf(box.MyEvent);
                if (thisIndex == -1)
                {
                    throw new Exception("Remove event from groups before removing from event list.");
                }

                int lastGroupWithAnythingInIt = 0;
                for (int i = 0; i < MainScreen.SelectedTaeAnim.EventGroups.Count; i++)
                {
                    if (MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Contains(thisIndex))
                        MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Remove(thisIndex);

                    if (MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Count > 0)
                        lastGroupWithAnythingInIt = i;
                }

                while (lastGroupWithAnythingInIt != MainScreen.SelectedTaeAnim.EventGroups.Count - 1)
                {
                    MainScreen.SelectedTaeAnim.EventGroups.RemoveAt(lastGroupWithAnythingInIt + 1);
                }
            }
        }

        private void RecreateAllAnimGroups()
        {
            if (MainScreen.SelectedTae.Animations.Any(a => a.EventGroups.Count > 0))
            {
                MainScreen.SelectedTaeAnim.EventGroups.Clear();

                foreach (var box in EventBoxes)
                {
                    AddBoxToEventGroups(box);
                }
            }
                
        }

        private void AddBoxToEventGroups(TaeEditAnimEventBox box)
        {
            if (MainScreen.SelectedTae.Animations.Any(a => a.EventGroups.Count > 0))
            {
                int thisIndex = MainScreen.SelectedTaeAnim.Events.IndexOf(box.MyEvent);
                if (thisIndex == -1)
                {
                    MainScreen.SelectedTaeAnim.Events.Add(box.MyEvent);
                    thisIndex = MainScreen.SelectedTaeAnim.Events.Count - 1;
                }

                while (MainScreen.SelectedTaeAnim.EventGroups.Count <= box.Row)
                {
                    MainScreen.SelectedTaeAnim.EventGroups.Add(
                        new TAE.EventGroup(MainScreen.SelectedTaeAnim.EventGroups.Count == box.Row
                           ? box.MyEvent.Type : 0));
                }
                int lastGroupWithAnythingInIt = 0;
                for (int i = 0; i < MainScreen.SelectedTaeAnim.EventGroups.Count; i++)
                {
                    if (i == box.Row)
                    {
                        if (!MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Contains(thisIndex))
                            MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Add(thisIndex);
                    }
                    else
                    {
                        if (MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Contains(thisIndex))
                            MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Remove(thisIndex);
                    }

                    if (MainScreen.SelectedTaeAnim.EventGroups[i].Indices.Count > 0)
                    {
                        if (!MainScreen.SelectedTaeAnim.EventGroups[i].Indices
                            .Any(index => MainScreen.SelectedTaeAnim.EventGroups[i].EventType 
                            == MainScreen.SelectedTaeAnim.Events[index].Type))
                        {
                            MainScreen.SelectedTaeAnim.EventGroups[i].EventType
                                = MainScreen.SelectedTaeAnim.Events[
                                    MainScreen.SelectedTaeAnim.EventGroups[i].Indices[0]].Type;
                        }

                        if (MainScreen.SelectedTaeAnim.EventGroups[i].EventType !=
                            MainScreen.SelectedTaeAnim.Events[MainScreen.SelectedTaeAnim.EventGroups[i].Indices[0]].Type)
                        {

                        }
                        lastGroupWithAnythingInIt = i;
                    }
                }

                while (lastGroupWithAnythingInIt != MainScreen.SelectedTaeAnim.EventGroups.Count - 1)
                {
                    MainScreen.SelectedTaeAnim.EventGroups.RemoveAt(lastGroupWithAnythingInIt + 1);
                }
            }
        }

        private void Box_RowChanged(object sender, int e)
        {
            var box = (TaeEditAnimEventBox)sender;
            if (sortedByRow.ContainsKey(e) && sortedByRow[e].Contains(box))
                sortedByRow[e].Remove(box);
            if (!sortedByRow.ContainsKey(box.Row))
                sortedByRow.Add(box.Row, new List<TaeEditAnimEventBox>());
            if (!sortedByRow[box.Row].Contains(box))
                sortedByRow[box.Row].Add(box);

            AddBoxToEventGroups(box);
        }

        public void DeleteMultipleEventBoxes(IEnumerable<TaeEditAnimEventBox> boxes, bool notUndoable)
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

            void DoDeleteAll()
            {
                foreach (var box in copyOfBoxes)
                {
                    if (MainScreen.SelectedEventBox == box)
                        MainScreen.SelectedEventBox = null;

                    if (MainScreen.MultiSelectedEventBoxes.Contains(box))
                        MainScreen.MultiSelectedEventBoxes.Remove(box);

                    box.RowChanged -= Box_RowChanged;

                    var r = box.Row;

                    if (sortedByRow.ContainsKey(r))
                        if (sortedByRow[r].Contains(box))
                            sortedByRow[r].Remove(box);

                    AnimRef.Events.Remove(box.MyEvent);

                    EventBoxes.Remove(box);

                    AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
                }

                RecreateAllAnimGroups();
            }

            if (notUndoable)
            {
                DoDeleteAll();
                return;
            }

            MainScreen.UndoMan.NewAction(
                doAction: () =>
                {
                    DoDeleteAll();
                },
                undoAction: () =>
                {
                    MainScreen.MultiSelectedEventBoxes.Clear();
                    foreach (var box in copyOfBoxes)
                    {
                        EventBoxes.Add(box);
                        AnimRef.Events.Add(box.MyEvent);

                        var r = box.Row;

                        if (!sortedByRow.ContainsKey(r))
                            sortedByRow.Add(r, new List<TaeEditAnimEventBox>());

                        if (!sortedByRow[r].Contains(box))
                            sortedByRow[r].Add(box);

                        box.RowChanged += Box_RowChanged;

                        if (!MainScreen.MultiSelectedEventBoxes.Contains(box))
                            MainScreen.MultiSelectedEventBoxes.Add(box);

                        AddBoxToEventGroups(box);

                        AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
                    }
                });
        }

        public void DeleteEventBox(TaeEditAnimEventBox box, bool notUndoable)
        {
            if (box.OwnerPane != this)
            {
                throw new ArgumentException($"This {nameof(TaeEditAnimEventGraph)} can only " +
                    $"delete {nameof(TaeEditAnimEventBox)}'s that it owns!", nameof(box));
            }

            void DoDelete()
            {
                if (MainScreen.SelectedEventBox == box)
                    MainScreen.SelectedEventBox = null;
                box.RowChanged -= Box_RowChanged;

                if (sortedByRow.ContainsKey(box.Row))
                    if (sortedByRow[box.Row].Contains(box))
                        sortedByRow[box.Row].Remove(box);

                AnimRef.Events.Remove(box.MyEvent);

                EventBoxes.Remove(box);

                AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);

                RecreateAllAnimGroups();
            }

            if (notUndoable)
            {
                DoDelete();
                return;
            }

            MainScreen.UndoMan.NewAction(
                doAction: () =>
                {
                    DoDelete();
                },
                undoAction: () =>
                {
                    EventBoxes.Add(box);
                    AnimRef.Events.Add(box.MyEvent);

                    if (!sortedByRow.ContainsKey(box.Row))
                        sortedByRow.Add(box.Row, new List<TaeEditAnimEventBox>());

                    if (!sortedByRow[box.Row].Contains(box))
                        sortedByRow[box.Row].Add(box);

                    box.RowChanged += Box_RowChanged;

                    MainScreen.SelectedEventBox = box;

                    AddBoxToEventGroups(box);

                    AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
                });
        }

        private TaeEditAnimEventBox PlaceNewEvent(TAE.Event ev, int row, bool notUndoable)
        {
            //ev.Index = MainScreen.SelectedTaeAnim.EventList.Count;

            if (MainScreen.SelectedTae?.BankTemplate != null && ev.Template == null && MainScreen.SelectedTae.BankTemplate.ContainsKey(ev.Type))
            {
                ev.ApplyTemplate(MainScreen.SelectedTae.BigEndian, MainScreen.SelectedTae.BankTemplate[ev.Type]);
            }

            var newBox = new TaeEditAnimEventBox(this, ev);

            newBox.Row = row;

            void DoPlaceEvent()
            {
                MainScreen.SelectedTaeAnim.Events.Add(ev);

                if (!sortedByRow.ContainsKey(newBox.Row))
                    sortedByRow.Add(newBox.Row, new List<TaeEditAnimEventBox>());

                sortedByRow[newBox.Row].Add(newBox);

                newBox.RowChanged += Box_RowChanged;

                EventBoxes.Add(newBox);

                AddBoxToEventGroups(newBox);

                AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
            }

            if (notUndoable)
            {
                DoPlaceEvent();
                return newBox;
            }

            MainScreen.UndoMan.NewAction(
                doAction: () =>
                {
                    DoPlaceEvent();
                },
                undoAction: () =>
                {
                    EventBoxes.Remove(newBox);

                    newBox.RowChanged -= Box_RowChanged;

                    if (sortedByRow.ContainsKey(newBox.Row))
                        if (sortedByRow[newBox.Row].Contains(newBox))
                            sortedByRow[newBox.Row].Remove(newBox);

                    RemoveEventBoxFromGroups(newBox);

                    MainScreen.SelectedTaeAnim.Events.Remove(ev);

                    AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
                });

            return newBox;
        }

        private void PlaceNewEventAtMouse()
        {
            float mouseTime = ((MainScreen.Input.MousePosition.X - Rect.X + ScrollViewer.Scroll.X) / SecondsPixelSize);

            TAE.Event newEvent = null;

            if (MainScreen.SelectedEventBox != null)
            {
                var curEvent = MainScreen.SelectedEventBox.MyEvent;

                var curEventDuration = (curEvent.EndTime - curEvent.StartTime);

                newEvent = new TAE.Event(mouseTime, mouseTime + curEventDuration, 
                    curEvent.Type, curEvent.Unk04, curEvent.GetParameterBytes(MainScreen.SelectedTae.BigEndian), MainScreen.SelectedTae.BigEndian);

                if (curEvent.Template != null)
                {
                    newEvent.ApplyTemplate(MainScreen.SelectedTae.BigEndian, curEvent.Template);
                }
            }
            else
            {
                if (MainScreen.SelectedTae.BankTemplate != null && MainScreen.SelectedTae.BankTemplate.ContainsKey(0))
                {
                    newEvent = new TAE.Event(mouseTime, mouseTime + 1, 0, 0, MainScreen.SelectedTae.BigEndian, MainScreen.SelectedTae.BankTemplate[0]);
                }
                else
                {
                    return;
                }
            }

            PlaceNewEvent(newEvent, MouseRow, notUndoable: false);
        }

        public void DeleteSelectedEvent()
        {
            if (MainScreen.MultiSelectedEventBoxes.Count > 0)
            {
                DeleteMultipleEventBoxes(MainScreen.MultiSelectedEventBoxes, notUndoable: false);
            }
            else if (MainScreen.SelectedEventBox != null)
            {
                DeleteEventBox(MainScreen.SelectedEventBox, notUndoable: false);
            }
        }

        public bool DoCut()
        {
            if (DoCopy())
            {
                DeleteSelectedEvent();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool DoCopy()
        {
            TaeClipboardContents clipboardContents = null;

            if (MainScreen.SelectedEventBox != null)
            {
                TaeClipboardContents.ParentGraph = this;
                clipboardContents = new TaeClipboardContents(
                    new List<TaeEditAnimEventBox> { MainScreen.SelectedEventBox },
                    MainScreen.SelectedEventBox.Row, MainScreen.SelectedEventBox.MyEvent.StartTime,
                    MainScreen.SelectedTae.BigEndian);
            }
            else if (MainScreen.MultiSelectedEventBoxes.Count > 0)
            {
                TaeClipboardContents.ParentGraph = this;
                var events = MainScreen.MultiSelectedEventBoxes;
                float startTime = events.OrderBy(x => x.MyEvent.StartTime).First().MyEvent.StartTime;
                int startRow = events.OrderBy(x => x.Row).First().Row;
                clipboardContents = new TaeClipboardContents(events, startRow, startTime, MainScreen.SelectedTae.BigEndian);
            }
            else
            {
                return false;
            }

            var testSerialize = Newtonsoft.Json.JsonConvert.SerializeObject(
                clipboardContents, Newtonsoft.Json.Formatting.Indented);

            System.Windows.Forms.Clipboard.SetText(testSerialize);

            return true;
        }

        public bool DoPaste(bool isAbsoluteLocation)
        {
            if (System.Windows.Forms.Clipboard.ContainsText())
            {
                try
                {
                    var jsonText = System.Windows.Forms.Clipboard.GetText();

                    TaeClipboardContents clipboardContents = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<TaeClipboardContents>(jsonText);

                    var events = clipboardContents.GetEvents().ToList();

                    var copyOfEvents = new List<TaeEditAnimEventBox>();
                    var copyOfSelectedEventBoxes = new List<TaeEditAnimEventBox>();
                    var copyOfSelectedEventBox = MainScreen.SelectedEventBox;

                    var copyOfRelMouse = relMouse;
                    var copyOfMouseRow = MouseRow;

                    var copyOfClipboardStartTime = clipboardContents.StartTime;

                    var copyOfEventStartTimes = new List<float>();
                    var copyOfEventEndTimes = new List<float>();
                    var copyOfEventRows = new List<int>();

                    foreach (var ev in events)
                    {
                        copyOfEvents.Add(ev);
                        copyOfEventStartTimes.Add(ev.MyEvent.StartTime);
                        copyOfEventEndTimes.Add(ev.MyEvent.EndTime);
                        copyOfEventRows.Add(ev.Row);
                    }

                    foreach (var ev in MainScreen.MultiSelectedEventBoxes)
                    {
                        copyOfSelectedEventBoxes.Add(ev);
                    }

                    if (events.Any())
                    {
                        MainScreen.UndoMan.NewAction(doAction: () =>
                        {
                            MainScreen.SelectedEventBox = null;
                            MainScreen.MultiSelectedEventBoxes.Clear();

                            for (int i = 0; i < copyOfEvents.Count; i++)
                            {
                                var evBox = copyOfEvents[i];
                                var ev = evBox.MyEvent;

                                float startTime = copyOfEventStartTimes[i];
                                float endTime = copyOfEventEndTimes[i];
                                int row = copyOfEventRows[i];

                                if (!isAbsoluteLocation)
                                {
                                    float start = copyOfEventStartTimes[i] - copyOfClipboardStartTime + TaeExtensionMethods.RoundTimeToFrame(copyOfRelMouse.X / SecondsPixelSize);
                                    float end = start + (copyOfEventEndTimes[i] - copyOfEventStartTimes[i]);

                                    startTime = start;
                                    endTime = end;

                                    row -= clipboardContents.StartRow;
                                    row += copyOfMouseRow;
                                }

                                ev.StartTime = startTime;
                                ev.EndTime = endTime;

                                var box = PlaceNewEvent(ev, row, notUndoable: true);

                                MainScreen.MultiSelectedEventBoxes.Add(box);
                            }

                            if (MainScreen.MultiSelectedEventBoxes.Count == 1)
                            {
                                MainScreen.SelectedEventBox = MainScreen.MultiSelectedEventBoxes[0];
                                MainScreen.MultiSelectedEventBoxes.Clear();
                            }
                        },
                        undoAction: () =>
                        {
                            for (int i = 0; i < copyOfEvents.Count; i++)
                            {
                                var matchedBox = EventBoxes.FirstOrDefault(eb => eb.MyEvent == copyOfEvents[i].MyEvent);
                                if (matchedBox != null)
                                    DeleteEventBox(matchedBox, notUndoable: true);
                            }

                            RecreateAllAnimGroups();

                            //DeleteMultipleEventBoxes(copyOfEvents, notUndoable: true);

                            //MainScreen.SelectedEventBox = copyOfSelectedEventBox;
                            //MainScreen.MultiSelectedEventBoxes = copyOfSelectedEventBoxes;
                        });

                       

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.ToString(), "Error During Paste Operation", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }

            return false;
        }

        public void UpdatePlaybackCursor(bool allowPlayPauseInput)
        {
            PlaybackCursor.Update(
                Main.Active && allowPlayPauseInput && MainScreen.Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Space),
                Main.Active && allowPlayPauseInput && 
                (MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftShift) || 
                MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.RightShift)), 
                EventBoxes);
        }

        public void MouseReleaseStuff()
        {
            currentUnselectedMouseDragType = UnselectedMouseDragType.None;
            PlaybackCursor.Scrubbing = false;
        }

        public void Update(bool allowMouseUpdate)
        {
            if (!MainScreen.Input.LeftClickHeld)
            {
                MouseReleaseStuff();
            }

            ScrollViewer.UpdateInput(MainScreen.Input, Main.DELTA_UPDATE, allowScrollWheel: !MainScreen.CtrlHeld);

            if (MainScreen.CtrlHeld)
            {
                Zoom(MainScreen.Input.ScrollDelta, MainScreen.Input.MousePosition.X - Rect.X);
            }

            if (currentUnselectedMouseDragType == UnselectedMouseDragType.PlaybackCursorScrub)
            {
                ScrollViewer.ClampScroll();

                float desiredTime = Math.Max(((MainScreen.Input.MousePosition.X - Rect.X) + ScrollViewer.Scroll.X) / SecondsPixelSize, 0);

                // If you just started clicking, immediately jump instead of smoothly going there.
                // The reason is: auto scroll going to the semi-lerped position
                // while zoomed in is incredibly annoying and bad design.
                if (MainScreen.Input.LeftClickDown || currentUnselectedMouseDragType != previousUnselectedMouseDragType)
                {
                    PlaybackCursor.CurrentTime = desiredTime;
                }
                else
                {
                    float timeDif = desiredTime - (float)PlaybackCursor.CurrentTime;
                    float clampedLerpF = MathHelper.Clamp(ScrubLerp * Main.DELTA_UPDATE, 0, 1);// MathHelper.Clamp(ScrubLerp / MathHelper.Clamp(Math.Abs(timeDif), 0.01f, 1), 0, 1);
                    PlaybackCursor.CurrentTime = MathHelper.Lerp((float)PlaybackCursor.CurrentTime, desiredTime, clampedLerpF);
                }

                if (!PlaybackCursor.IsPlaying)
                    PlaybackCursor.StartTime = PlaybackCursor.CurrentTime;

                PlaybackCursor.Scrubbing = true;

                MouseRow = -1;

                if (!MainScreen.Input.LeftClickHeld)
                    currentUnselectedMouseDragType = UnselectedMouseDragType.None;

                return;
            }
            else if (currentUnselectedMouseDragType == UnselectedMouseDragType.HorizontalScroll
                || currentUnselectedMouseDragType == UnselectedMouseDragType.VerticalScroll)
            {
                if (!MainScreen.Input.LeftClickHeld)
                    currentUnselectedMouseDragType = UnselectedMouseDragType.None;

                // Prevent any updates to anything on the graph except scrolling when your current drag
                // began on a scrollbar, so return here
                return;
            }
            else if (currentUnselectedMouseDragType == UnselectedMouseDragType.EventSelect)
            {
                if (!MainScreen.Input.LeftClickHeld)
                    currentUnselectedMouseDragType = UnselectedMouseDragType.None;
            }

            MouseRow = (int)(relMouse.Y / RowHeight);

            if (ScrollViewer.Viewport.Contains(new Point((int)MainScreen.Input.MousePosition.X, (int)MainScreen.Input.MousePosition.Y))
                || MainScreen.Input.LeftClickHeld)
            {
                if (currentUnselectedMouseDragType == UnselectedMouseDragType.None)
                {
                    if (MainScreen.Input.MousePosition.Y < ScrollViewer.Viewport.Top + TimeLineHeight)
                    {
                        if (MainScreen.Input.LeftClickDown)
                        {
                            currentUnselectedMouseDragType = UnselectedMouseDragType.PlaybackCursorScrub;
                            return;
                        }
                    }
                    else if (MainScreen.Input.MousePosition.Y > Rect.Bottom - ScrollViewer.ScrollBarThickness && !ScrollViewer.DisableHorizontalScroll)
                    {
                        if (MainScreen.Input.LeftClickDown)
                            currentUnselectedMouseDragType = UnselectedMouseDragType.HorizontalScroll;

                        // Return so our initial click down doesn't select events.
                        return;
                    }
                    else if (MainScreen.Input.MousePosition.X > Rect.Right - ScrollViewer.ScrollBarThickness && !ScrollViewer.DisableVerticalScroll)
                    {
                        if (MainScreen.Input.LeftClickDown)
                            currentUnselectedMouseDragType = UnselectedMouseDragType.VerticalScroll;

                        // Return so our initial click down doesn't select events.
                        return;
                    }
                    else
                    {
                        //MouseRow = (int)(relMouse.Y / RowHeight);

                        if (MainScreen.Input.LeftClickDown)
                        {
                            currentUnselectedMouseDragType = UnselectedMouseDragType.EventSelect;
                        }
                    }
                }

                MainScreen.Input.CursorType = MouseCursorType.Arrow;

                if (MainScreen.Input.LeftClickDown && !MainScreen.ShiftHeld && !MainScreen.CtrlHeld)
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
                            relMouse.X >= box.LeftFr - BoxSideScrollMarginSize && 
                            (!(PlaybackCursor.IsPlaying && MainScreen.Config.AutoScrollDuringAnimPlayback)))
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
                                    currentDrag.BoxOriginalRow = box.Row;
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
                                        newDrag.BoxOriginalRow = multiBox.Row;
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
                            relMouse.X <= box.RightFr + BoxSideScrollMarginSize &&
                            (!(PlaybackCursor.IsPlaying && MainScreen.Config.AutoScrollDuringAnimPlayback)))
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            if (MainScreen.Input.LeftClickDown && currentUnselectedMouseDragType == UnselectedMouseDragType.EventSelect)
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
                                    currentDrag.BoxOriginalRow = box.Row;
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
                                        newDrag.BoxOriginalRow = multiBox.Row;
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
                                    currentDrag.BoxOriginalRow = box.Row;
                                    currentDrag.StartMouseRow = MouseRow;
                                    currentDrag.StartDragPoint = currentDrag.CurrentDragPoint = relMouse.ToPoint();
                                }
                                else
                                {
                                    if (MainScreen.MultiSelectedEventBoxes.Contains(box) && !MainScreen.CtrlHeld)
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
                                            newDrag.BoxOriginalRow = multiBox.Row;
                                            newDrag.StartMouseRow = MouseRow;
                                            newDrag.StartDragPoint = newDrag.CurrentDragPoint = relMouse.ToPoint();

                                            currentMultiDrag.Add(newDrag);
                                        }
                                    }
                                    else
                                    {
                                        if (MainScreen.ShiftHeld && !MainScreen.CtrlHeld && !MainScreen.AltHeld)
                                        {
                                            if (MainScreen.SelectedEventBox == null 
                                                && MainScreen.MultiSelectedEventBoxes.Count == 0)
                                                MainScreen.SelectedEventBox = box;
                                            else if (MainScreen.SelectedEventBox != null)
                                            {
                                                MainScreen.MultiSelectedEventBoxes = new List<TaeEditAnimEventBox>
                                                {
                                                    MainScreen.SelectedEventBox,
                                                    box,
                                                };
                                                MainScreen.SelectedEventBox = null;
                                            }
                                            else if (MainScreen.SelectedEventBox == null
                                                && MainScreen.MultiSelectedEventBoxes.Count > 0 
                                                && !MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                MainScreen.MultiSelectedEventBoxes.Add(box);
                                        }
                                        else if (!MainScreen.ShiftHeld && MainScreen.CtrlHeld && !MainScreen.AltHeld)
                                        {
                                            if (MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                MainScreen.MultiSelectedEventBoxes.Remove(box);
                                            if (MainScreen.SelectedEventBox == box)
                                                MainScreen.SelectedEventBox = null;
                                        }
                                        else
                                        {
                                            MainScreen.MultiSelectedEventBoxes.Clear();
                                            MainScreen.SelectedEventBox = box;
                                            MainScreen.UpdateInspectorToSelection();
                                        }
                                        MainScreen.UpdateInspectorToSelection();
                                    }
                                }
                            }
                        }
                    }

                    var isSingleSelect = (currentDrag.DragType == BoxDragType.None) ||
                        (currentDrag.DragType == BoxDragType.MiddleOfEventBox && currentDrag.StartDragPoint == currentDrag.CurrentDragPoint);


                    if (isSingleSelect && MainScreen.Input.LeftClickDown && !(MainScreen.Input.MousePosition.Y < ScrollViewer.Viewport.Top + TimeLineHeight))
                    {
                        if (relMouse.X >= box.LeftFr && relMouse.X < box.RightFr)
                        {
                            currentMultiDrag.Clear();

                            if (MainScreen.ShiftHeld && !MainScreen.CtrlHeld && !MainScreen.AltHeld)
                            {
                                if (MainScreen.SelectedEventBox == null
                                    && MainScreen.MultiSelectedEventBoxes.Count == 0)
                                    MainScreen.SelectedEventBox = box;
                                else if (MainScreen.SelectedEventBox != null)
                                {
                                    MainScreen.MultiSelectedEventBoxes = new List<TaeEditAnimEventBox>
                                    {
                                        MainScreen.SelectedEventBox,
                                        box,
                                    };
                                    MainScreen.SelectedEventBox = null;
                                }
                                else if (MainScreen.SelectedEventBox == null
                                    && MainScreen.MultiSelectedEventBoxes.Count > 0
                                    && !MainScreen.MultiSelectedEventBoxes.Contains(box))
                                    MainScreen.MultiSelectedEventBoxes.Add(box);
                            }
                            else if (!MainScreen.ShiftHeld && MainScreen.CtrlHeld && !MainScreen.AltHeld)
                            {
                                if (MainScreen.MultiSelectedEventBoxes.Contains(box))
                                    MainScreen.MultiSelectedEventBoxes.Remove(box);
                                if (MainScreen.SelectedEventBox == box)
                                    MainScreen.SelectedEventBox = null;
                            }
                            else
                            {
                                MainScreen.SelectedEventBox = box;
                                MainScreen.MultiSelectedEventBoxes.Clear();
                            }

                            MainScreen.UpdateInspectorToSelection();

                            break;
                        }

                    }

                }

                if (currentDrag.DragType == BoxDragType.None && 
                    MainScreen.Input.LeftClickDown
                    && !(MainScreen.Input.MousePosition.Y < ScrollViewer.Viewport.Top + TimeLineHeight)
                    )
                {
                    if (MainScreen.SelectedEventBox != null)
                    {
                        MainScreen.MultiSelectedEventBoxes.Add(MainScreen.SelectedEventBox);
                        MainScreen.SelectedEventBox = null;
                    }

                    if (MainScreen.ShiftHeld && !MainScreen.CtrlHeld && !MainScreen.AltHeld)
                        currentDrag.DragType = BoxDragType.MultiSelectionRectangleADD;
                    else if (!MainScreen.ShiftHeld && MainScreen.CtrlHeld && !MainScreen.AltHeld)
                        currentDrag.DragType = BoxDragType.MultiSelectionRectangleSUBTRACT;
                    else
                        currentDrag.DragType = BoxDragType.MultiSelectionRectangle;

                    currentDrag.StartDragPoint = relMouse.ToPoint();
                }

                if (MainScreen.Input.LeftClickHeld)
                {
                    currentDrag.CurrentDragPoint = relMouse.ToPoint();

                    if (!(PlaybackCursor.IsPlaying && MainScreen.Config.AutoScrollDuringAnimPlayback))
                    {
                        if (currentDrag.DragType == BoxDragType.LeftOfEventBox && currentDrag.Box != null)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            var isModified = currentDrag.DragBoxToMouseAndCheckIsModified(relMouse.ToPoint());
                            AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            MainScreen.UpdateInspectorToSelection();
                            //currentDrag.Box.DragLeftSide(MainScreen.Input.MousePositionDelta.X);
                        }
                        else if (currentDrag.DragType == BoxDragType.RightOfEventBox && currentDrag.Box != null)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            var isModified = currentDrag.DragBoxToMouseAndCheckIsModified(relMouse.ToPoint());
                            AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            MainScreen.UpdateInspectorToSelection();
                            //currentDrag.Box.DragRightSide(MainScreen.Input.MousePositionDelta.X);
                        }
                        else if (currentDrag.DragType == BoxDragType.MiddleOfEventBox && currentDrag.Box != null)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragXY;
                            var isModified = currentDrag.DragBoxToMouseAndCheckIsModified(relMouse.ToPoint());
                            AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            MainScreen.UpdateInspectorToSelection();
                            //currentDrag.Box.DragMiddle(MainScreen.Input.MousePositionDelta.X);
                            currentDrag.ShiftBoxRow(MouseRow);
                        }
                        else if (currentDrag.DragType == BoxDragType.MultiDragLeftOfEventBox)
                        {
                            var earliestEndingDrag = currentMultiDrag.OrderBy(x => x.BoxOriginalEnd).First();
                            int mouseMaxX = MathHelper.Max((int)earliestEndingDrag.StartDragPoint.X, (int)((earliestEndingDrag.BoxOriginalEnd * SecondsPixelSize) - (TaeEditAnimEventBox.FRAME * SecondsPixelSize)));

                            var earliestDrag_preventNegative = currentMultiDrag.OrderBy(x => x.BoxOriginalStart).First();
                            int mouseMinX_preventNegative = earliestDrag_preventNegative.Offset.X + (int)earliestDrag_preventNegative.BoxOriginalStart;

                            var actualMousePoint = new Point(MathHelper.Max(MathHelper.Min((int)relMouse.X, mouseMaxX), mouseMinX_preventNegative), (int)(relMouse.Y));

                            foreach (var multiDrag in currentMultiDrag)
                            {
                                MainScreen.Input.CursorType = MouseCursorType.DragX;
                                bool isModified = multiDrag.DragBoxToMouseAndCheckIsModified(actualMousePoint);
                                AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            }

                            MainScreen.UpdateInspectorToSelection();
                        }
                        else if (currentDrag.DragType == BoxDragType.MultiDragRightOfEventBox)
                        {
                            var shortestDrag = currentMultiDrag.OrderBy(x => x.BoxOriginalDuration).First();
                            int mouseMinX = shortestDrag.StartDragPoint.X - (int)((shortestDrag.BoxOriginalDuration * SecondsPixelSize));
                            var actualMousePoint = new Point(MathHelper.Max((int)relMouse.X, mouseMinX), (int)(relMouse.Y));

                            foreach (var multiDrag in currentMultiDrag)
                            {
                                MainScreen.Input.CursorType = MouseCursorType.DragX;
                                bool isModified = multiDrag.DragBoxToMouseAndCheckIsModified(actualMousePoint);
                                AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            }

                            MainScreen.UpdateInspectorToSelection();
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
                                var isModified = multiDrag.DragBoxToMouseAndCheckIsModified(actualMousePoint);

                                AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));

                                multiDrag.ShiftBoxRow(MathHelper.Max(MouseRow, minimumMouseRow));
                            }

                            MainScreen.Input.CursorType = MouseCursorType.DragXY;

                            MainScreen.UpdateInspectorToSelection();
                        }
                        else if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle
                            || currentDrag.DragType == BoxDragType.MultiSelectionRectangleADD
                            || currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
                        {
                            if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle)
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
                                            if (currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
                                            {
                                                if (MainScreen.SelectedEventBox == box)
                                                    MainScreen.SelectedEventBox = null;
                                                if (MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                    MainScreen.MultiSelectedEventBoxes.Remove(box);
                                            }
                                            else
                                            {
                                                if (MainScreen.SelectedEventBox == null)
                                                {
                                                    if (!MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                        MainScreen.MultiSelectedEventBoxes.Add(box);
                                                }
                                                else
                                                {
                                                    if (!MainScreen.MultiSelectedEventBoxes.Contains(MainScreen.SelectedEventBox))
                                                        MainScreen.MultiSelectedEventBoxes.Add(MainScreen.SelectedEventBox);
                                                    if (!MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                        MainScreen.MultiSelectedEventBoxes.Add(box);
                                                    MainScreen.SelectedEventBox = null;
                                                }

                                            }


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
                        // This happens while playing and autoscrolling :fatcat:
                        if (currentDrag.DragType == BoxDragType.MiddleOfEventBox && currentDrag.Box != null)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.Arrow;
                        }
                        else if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle
                        || currentDrag.DragType == BoxDragType.MultiSelectionRectangleADD
                        || currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
                        {
                            if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle)
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
                                            if (currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
                                            {
                                                if (MainScreen.SelectedEventBox == box)
                                                    MainScreen.SelectedEventBox = null;
                                                if (MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                    MainScreen.MultiSelectedEventBoxes.Remove(box);
                                            }
                                            else
                                            {
                                                if (MainScreen.SelectedEventBox == null)
                                                {
                                                    if (!MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                        MainScreen.MultiSelectedEventBoxes.Add(box);
                                                }
                                                else
                                                {
                                                    if (!MainScreen.MultiSelectedEventBoxes.Contains(MainScreen.SelectedEventBox))
                                                        MainScreen.MultiSelectedEventBoxes.Add(MainScreen.SelectedEventBox);
                                                    if (!MainScreen.MultiSelectedEventBoxes.Contains(box))
                                                        MainScreen.MultiSelectedEventBoxes.Add(box);
                                                    MainScreen.SelectedEventBox = null;
                                                }

                                            }


                                        }
                                    }
                                }
                            }
                            MainScreen.SelectedEventBox = null;
                            MainScreen.UpdateInspectorToSelection();
                        }

                    }
                }
                else // Releasing click after a drag
                {
                    if (currentDrag.DragType != BoxDragType.None)
                    {
                        if (!(PlaybackCursor.IsPlaying && MainScreen.Config.AutoScrollDuringAnimPlayback))
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
                                int copyOfCurrentBoxRow = copyOfBox.Row;

                                bool copyOfIsMainScreenModified = MainScreen.IsModified;
                                bool copyOfIsAnimModified = MainScreen.SelectedTaeAnim.GetIsModified();

                                MainScreen.UndoMan.NewAction(
                                    doAction: () =>
                                    {
                                        copyOfBox.MyEvent.StartTime = copyOfCurrentBoxStart;
                                        copyOfBox.MyEvent.EndTime = copyOfCurrentBoxEnd;
                                        copyOfBox.Row = copyOfCurrentBoxRow;

                                        copyOfBox.MyEvent.ApplyRounding();

                                        MainScreen.SelectedTaeAnim.SetIsModified(
                                            MainScreen.SelectedTaeAnim.GetIsModified() ||
                                            (!MainScreen.IsReadOnlyFileMode && ((copyOfCurrentBoxStart != copyOfOldBoxStart) ||
                                            (copyOfCurrentBoxEnd != copyOfOldBoxEnd) ||
                                            (copyOfCurrentBoxRow != copyOfOldBoxRow))));
                                    },
                                    undoAction: () =>
                                    {
                                        copyOfBox.MyEvent.StartTime = copyOfOldBoxStart;
                                        copyOfBox.MyEvent.EndTime = copyOfOldBoxEnd;
                                        copyOfBox.Row = copyOfOldBoxRow;

                                    // Check if user saved, to flag as modified from the value changing
                                    // Otherwise, if it was still modified afterwards we can un-modified it
                                    if (!MainScreen.SelectedTaeAnim.GetIsModified())
                                        {
                                            MainScreen.SelectedTaeAnim.SetIsModified(
                                                MainScreen.SelectedTaeAnim.GetIsModified() ||
                                                (!MainScreen.IsReadOnlyFileMode && ((copyOfCurrentBoxStart != copyOfOldBoxStart) ||
                                                (copyOfCurrentBoxEnd != copyOfOldBoxEnd) ||
                                                (copyOfCurrentBoxRow != copyOfOldBoxRow))));
                                        }
                                        else
                                        {
                                            MainScreen.SelectedTaeAnim.SetIsModified(copyOfIsAnimModified);
                                        }
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

                                bool copyOfIsMainScreenModified = MainScreen.IsModified;
                                bool copyOfIsAnimModified = MainScreen.SelectedTaeAnim.GetIsModified();

                                foreach (var multiDrag in currentMultiDrag)
                                {
                                    copiesOfBox.Add(multiDrag.Box);

                                    copiesOfOldBoxStart.Add(multiDrag.BoxOriginalStart);
                                    copiesOfOldBoxEnd.Add(multiDrag.BoxOriginalEnd);
                                    copiesOfOldBoxRow.Add(multiDrag.BoxOriginalRow);

                                    copiesOfCurrentBoxStart.Add(multiDrag.Box.MyEvent.StartTime);
                                    copiesOfCurrentBoxEnd.Add(multiDrag.Box.MyEvent.EndTime);
                                    copiesOfCurrentBoxRow.Add(multiDrag.Box.Row);
                                }

                                MainScreen.UndoMan.NewAction(
                                        doAction: () =>
                                        {
                                            for (int i = 0; i < copiesOfBox.Count; i++)
                                            {
                                                copiesOfBox[i].MyEvent.StartTime = copiesOfCurrentBoxStart[i];
                                                copiesOfBox[i].MyEvent.EndTime = copiesOfCurrentBoxEnd[i];
                                                copiesOfBox[i].Row = copiesOfCurrentBoxRow[i];

                                                copiesOfBox[i].MyEvent.ApplyRounding();

                                                MainScreen.SelectedTaeAnim.SetIsModified(
                                                    MainScreen.SelectedTaeAnim.GetIsModified() ||
                                                    (!MainScreen.IsReadOnlyFileMode && ((copiesOfCurrentBoxStart[i] != copiesOfOldBoxStart[i]) ||
                                                    (copiesOfCurrentBoxEnd[i] != copiesOfOldBoxEnd[i]) ||
                                                    (copiesOfCurrentBoxRow[i] != copiesOfOldBoxRow[i]))));
                                            }
                                        },
                                        undoAction: () =>
                                        {
                                            for (int i = 0; i < copiesOfBox.Count; i++)
                                            {
                                                copiesOfBox[i].MyEvent.StartTime = copiesOfOldBoxStart[i];
                                                copiesOfBox[i].MyEvent.EndTime = copiesOfOldBoxEnd[i];
                                                copiesOfBox[i].Row = copiesOfOldBoxRow[i];

                                            // Check if user saved, to flag as modified from the value changing
                                            // Otherwise, if it was still modified afterwards we can un-modified it
                                            if (!MainScreen.SelectedTaeAnim.GetIsModified())
                                                {
                                                    MainScreen.SelectedTaeAnim.SetIsModified(
                                                    MainScreen.SelectedTaeAnim.GetIsModified() ||
                                                    (!MainScreen.IsReadOnlyFileMode && ((copiesOfCurrentBoxStart[i] != copiesOfOldBoxStart[i]) ||
                                                    (copiesOfCurrentBoxEnd[i] != copiesOfOldBoxEnd[i]) ||
                                                    (copiesOfCurrentBoxRow[i] != copiesOfOldBoxRow[i]))));
                                                }
                                                else
                                                {
                                                    MainScreen.SelectedTaeAnim.SetIsModified(copyOfIsAnimModified);
                                                }


                                            }
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
                            else if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle
                                || currentDrag.DragType == BoxDragType.MultiSelectionRectangleADD
                                || currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
                            {
                                currentDrag.DragType = BoxDragType.None;
                                if (MainScreen.MultiSelectedEventBoxes.Count == 1)
                                {
                                    MainScreen.SelectedEventBox = MainScreen.MultiSelectedEventBoxes[0];
                                    MainScreen.MultiSelectedEventBoxes.Clear();
                                }
                            }
                        }
                        else
                        {
                            // this happens after releasing drag while playing anim with autoscroll
                            if (currentDrag.DragType == BoxDragType.MiddleOfEventBox)
                            {
                                currentDrag.DragType = BoxDragType.None;
                                currentDrag.Box = null;
                            }
                            else if (currentDrag.DragType == BoxDragType.MultiDragLeftOfEventBox ||
                                currentDrag.DragType == BoxDragType.MultiDragRightOfEventBox ||
                                currentDrag.DragType == BoxDragType.MultiDragMiddleOfEventBox)
                            {
                                currentMultiDrag.Clear();
                                currentDrag.DragType = BoxDragType.None;
                            }
                            else if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle
                                || currentDrag.DragType == BoxDragType.MultiSelectionRectangleADD
                                || currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
                            {
                                currentDrag.DragType = BoxDragType.None;
                                if (MainScreen.MultiSelectedEventBoxes.Count == 1)
                                {
                                    MainScreen.SelectedEventBox = MainScreen.MultiSelectedEventBoxes[0];
                                    MainScreen.MultiSelectedEventBoxes.Clear();
                                }
                            }
                        }

                            

                        
                    }
                }

                
            }

            previousUnselectedMouseDragType = currentUnselectedMouseDragType;
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
                (int)EventBoxes.OrderByDescending(x => x.Row).First().Bottom + 64);
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
                    sb.DrawString(font, $"{(kvp.Key)}", new Vector2(kvp.Value + 4 + 1, (int)ScrollViewer.Scroll.Y + 1 + 1), Color.Black);
                sb.DrawString(font, $"{(kvp.Key)}", new Vector2(kvp.Value + 4, (int)ScrollViewer.Scroll.Y + 1), MainScreen.Config.EnableColorBlindMode ? Color.Black : Color.White);
            }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font, float elapsedSeconds)
        {
            ScrollViewer.SetDisplayRect(Rect, GetVirtualAreaSize());

            ScrollViewer.Draw(gd, sb, boxTex, font);

            var scrollMatrix = ScrollViewer.GetScrollMatrix();

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport);
            {
                sb.Begin(transformMatrix: scrollMatrix);

                // full bg
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

                sb.Draw(texture: boxTex,
                    position: ScrollViewer.Scroll - (Vector2.One * 4),
                    sourceRectangle: null,
                    color: MainScreen.Config.EnableColorBlindMode ? Color.Fuchsia : (new Color(64, 64, 64, 255)),
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight) + new Vector2(8, 4),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );

                //// timeline - anim duration
                //sb.Draw(texture: boxTex,
                //    position: ScrollViewer.Scroll - (Vector2.One * 2),
                //    sourceRectangle: null,
                //    color: MainScreen.Config.EnableColorBlindMode ? Color.Aqua : (Color.LightSeaGreen),
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2((float)PlaybackCursor.MaxTime * SecondsPixelSize, TimeLineHeight),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );


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

                        Color boxOutlineColor = box.ColorOutline;

                        Color thisBoxBgColor = new Color(box.ColorBG.ToVector3() * 0.4f);
                        Color thisBoxBgColorSelected = new Color(box.ColorBG.ToVector3() * 0.7f);
                        Color boxOutlineColorSelected = Color.White;
                        

                        if (MainScreen.Config.EnableColorBlindMode)
                        {
                            thisBoxBgColor = Color.Black;
                            thisBoxBgColorSelected = Color.White;
                            boxOutlineColor = Color.Gray;
                            boxOutlineColorSelected = Color.White;
                            textFG = box.PlaybackHighlight ? Color.Black : Color.White;
                            textBG = Color.Transparent;
                        }

                        // outline
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
                            color: (box.PlaybackHighlight) ? thisBoxBgColorSelected : thisBoxBgColor,
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

                            var thicknessOffset = new Vector2(2/*boxOutlineThickness*/ * 2, 0);

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
                                    $"{(box.MyEvent.Type)}";
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



                //if (MouseRow >= 0)
                //{
                //    sb.Draw(texture: boxTex,
                //            position: new Vector2(ScrollViewer.Scroll.X, TimeLineHeight + MouseRow * RowHeight),
                //            sourceRectangle: null,
                //            color: Color.LightGray * 0.25f,
                //            rotation: 0,
                //            origin: Vector2.Zero,
                //            scale: new Vector2(ScrollViewer.Viewport.Width, RowHeight),
                //            effects: SpriteEffects.None,
                //            layerDepth: 0.01f
                //            );
                //}

                // full timeline
                sb.Draw(texture: boxTex,
                    position: ScrollViewer.Scroll - (Vector2.One * 4),
                    sourceRectangle: null,
                    color: MainScreen.Config.EnableColorBlindMode ? Color.Fuchsia : (new Color(75, 75, 75, 255)),
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight) + new Vector2(8, 4),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );

                if (SecondsPixelSize >= (30 * MinPixelsBetweenFramesForHelperLines))
                {
                    int startFrame = (int)Math.Floor(ScrollViewer.Scroll.X / FramePixelSize);
                    int endFrame = (int)Math.Floor((ScrollViewer.Scroll.X + ScrollViewer.Viewport.Width) / FramePixelSize);

                    for (int i = startFrame; i <= endFrame; i++)
                    {
                        sb.Draw(texture: boxTex,
                        position: new Vector2(i * FramePixelSize, ScrollViewer.Scroll.Y),
                        sourceRectangle: null,
                        color: MainScreen.Config.EnableColorBlindMode ? Color.White * 0.25f : Color.Black * 0.125f,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(1, TimeLineHeight),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );

                        try
                        {
                            if (PlaybackCursor.SnapInterval > 0)
                            {
                                if (((i % PlaybackCursor.SnapInterval) != 0) && SecondsPixelSize >= (30 * MinPixelsBetweenFramesForFrameNumberText))
                                {
                                    sb.DrawString(font, i.ToString(), new Vector2(i * FramePixelSize + 2, ScrollViewer.Scroll.Y + 2), Color.Cyan);
                                }
                            }
                            
                        }
                        catch
                        {

                        }
                        
                    }
                }

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

                

                if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle
                    || currentDrag.DragType == BoxDragType.MultiSelectionRectangleADD
                    || currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
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

                var playbackCursorPixelX = SecondsPixelSize * (float)PlaybackCursor.GUICurrentTime;

                //-- BOTTOM Side <-- I have no idea what I meant by this <-- turns out i just copy pasted the draw call above
                // Draw PlaybackCursor CurrentTime vertical line
                sb.Draw(texture: boxTex,
                    position: new Vector2(playbackCursorPixelX - (PlaybackCursorThickness / 2), ScrollViewer.Scroll.Y),
                    sourceRectangle: null,
                    color: PlaybackCursorColor,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(PlaybackCursorThickness, Rect.Height),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );

                float centerOfScreenX = (ScrollViewer.Scroll.X + (ScrollViewer.Viewport.Width / 2));
                float rightOfScreenX = ScrollViewer.Viewport.Width + ScrollViewer.Scroll.X;

                float maxHorizontalScrollNeededForAnimEndToBeInScreen = 
                    ((SecondsPixelSize * (float)PlaybackCursor.MaxTime) 
                    + AfterAutoScrollHorizontalMargin) - ScrollViewer.Viewport.Width;

                if (PlaybackCursor.Scrubbing && 
                    !(MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftShift) || 
                    MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.RightShift)))
                {
                    float rightScrubScrollMarginStart = (ScrollViewer.Scroll.X + ScrollViewer.Viewport.Width) - ScrubScrollStartMargin;
                    float leftScrubScrollMarginStart = (ScrollViewer.Scroll.X + ScrubScrollStartMargin);
                    if (playbackCursorPixelX > rightScrubScrollMarginStart)
                    {
                        float scrubScrollSpeedMult = MathHelper.Max(
                            playbackCursorPixelX - rightScrubScrollMarginStart, 0) / ScrubScrollStartMargin;

                        ScrollViewer.Scroll.X += ScrubScrollSpeed * (scrubScrollSpeedMult * scrubScrollSpeedMult);

                        ScrollViewer.ClampScroll();
                    }
                    else if (playbackCursorPixelX < leftScrubScrollMarginStart)
                    {
                        float scrubScrollSpeedMult = MathHelper.Max(
                            leftScrubScrollMarginStart - playbackCursorPixelX, 0) / ScrubScrollStartMargin;

                        ScrollViewer.Scroll.X -= ScrubScrollSpeed * (scrubScrollSpeedMult * scrubScrollSpeedMult);
                        ScrollViewer.ClampScroll();
                    }
                }
                else if (MainScreen.Config.AutoScrollDuringAnimPlayback && PlaybackCursor.IsPlaying)
                {
                    if ((ScrollViewer.Scroll.X < maxHorizontalScrollNeededForAnimEndToBeInScreen) ||
                        playbackCursorPixelX < ScrollViewer.Scroll.X)
                    {
                        if (playbackCursorPixelX < ScrollViewer.Scroll.X)
                        {
                            ScrollViewer.Scroll.X = playbackCursorPixelX;
                        }
                        else

                        {
                            float distFromScrollToCursor = playbackCursorPixelX - centerOfScreenX;

                            ScrollViewer.Scroll.X += distFromScrollToCursor * 0.1f;
                            ScrollViewer.Scroll.X = Math.Min(ScrollViewer.Scroll.X, maxHorizontalScrollNeededForAnimEndToBeInScreen);
                            ScrollViewer.ClampScroll();
                        }

                        
                    }
                }

                // Draw PlaybackCursor StartTime vertical line
                sb.Draw(texture: boxTex,
                    position: new Vector2((float)(SecondsPixelSize * PlaybackCursor.GUIStartTime) - (PlaybackCursorThickness / 2), 0),
                    sourceRectangle: null,
                    color: PlaybackCursorColor * 0.25f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(PlaybackCursorThickness, Rect.Height),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );


                // This would draw a little +/- by the mouse cursor to signify that
                // you're adding/subtracting selection
                // HOWEVER it was super delayed because of the vsync 
                // and didn't follow the cursor well and looked weird

                //if (CtrlHeld && !ShiftHeld && !AltHeld)
                //{
                //    sb.DrawString(font, "－", relMouse + new Vector2(12, 12 + TimeLineHeight) + Vector2.One, Color.Black);
                //    sb.DrawString(font, "－", relMouse + new Vector2(12, 12 + TimeLineHeight), Color.White);
                //}
                //else if (!CtrlHeld && ShiftHeld && !AltHeld)
                //{
                //    sb.DrawString(font, "＋", relMouse + new Vector2(12, 12 + TimeLineHeight) + Vector2.One, Color.Black);
                //    sb.DrawString(font, "＋", relMouse + new Vector2(12, 12 + TimeLineHeight), Color.White);
                //}

                float animStopPixelX = (float)PlaybackCursor.MaxTime * SecondsPixelSize;

                float darkenedPortionWidth = rightOfScreenX - animStopPixelX;

                if (darkenedPortionWidth > 0)
                {
                    // full bg - anim duration
                    sb.Draw(texture: boxTex,
                          position: new Vector2(animStopPixelX, (int)ScrollViewer.Scroll.Y),
                          sourceRectangle: null,
                          color: (MainScreen.Config.EnableColorBlindMode ? Color.Green : Color.Black) * 0.25f,
                          rotation: 0,
                          origin: Vector2.Zero,
                          scale: new Vector2(darkenedPortionWidth, ScrollViewer.Viewport.Height),
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
