using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DSAnimStudio.ImguiOSD;
using Microsoft.Xna.Framework.Input;
using static DSAnimStudio.DSAProj;

namespace DSAnimStudio.TaeEditor
{
    public class NewGraphInput : IDisposable
    {
        public enum DragTypes
        {
            None,
            SelectionRect,
            Move,
            ResizeLeft,
            ResizeRight,
            TimelineScrub,
            TimelineScrub_Relative,
            InteractingWithOtherPanels,
            CancelledDragStillHoldingMouse,
        }

        private NewGraphLayoutManager Layout => Graph.LayoutManager;
        public DragTypes CurrentDragType;
        /// <summary>
        /// X = seconds, Y = rows
        /// </summary>
        public Vector2 CurrentMouseStartPoint;
        public Vector2 CurrentMouseStartPoint_Pixels;
        public Vector2 CurrentMouseSustainPoint;
        public Vector2 CurrentMouseSustainPoint_Pixels;
        public Vector2 CurrentMouseHoverPoint;
        public Vector2 CurrentMouseHoverPoint_Pixels;
        public Vector2 CurrentMousePointGeneric;
        public Vector2 CurrentMousePointGeneric_Pixels;

        public Vector2 CurrentDragMouseAccumulatedPixels;
        
        public List<DSAProj.Action> CurrentDragActions = new List<DSAProj.Action>();
        private bool IsFirstUpdateCycleOfDrag = true;
        private bool WasPlayingBeforeTimelineDrag = false;

        public void ResetAllDrag()
        {
            CurrentDragActions.Clear();
            CurrentDragType = DragTypes.None;
        }

        //public static List<Rectangle> DEBUG_EVENTS = new List<Rectangle>();
        //public static Vector2 DEBUG_MOUSE;

        public void MoveMouseToTimelineLocation(FancyInputHandler input)
        {
            if (Main.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor)
            {
                float newMouseX =
                    (float)(Graph.PlaybackCursor.GUICurrentTimeMod * Layout.SecondsPixelSize);
                newMouseX -= Graph.ScrollViewer.RoundedScroll.X;
                newMouseX += Graph.ScrollViewerContentsRect.X;

                float newMouseY = input.LeftClickDownAnchor.Y;

                int x = (int)Math.Round(newMouseX);
                int y = (int)Math.Round(newMouseY);

                input.LockMouseCursor(x, y);

                // Main.WinForm.Invoke(() =>
                // {
                //     using (var g = Main.WinForm.CreateGraphics())
                //     {
                //         // Create pen.
                //         System.Drawing.Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 1);
                //
                //         // Create points that define line.
                //         System.Drawing.Point point1 = new System.Drawing.Point(x, y);
                //         System.Drawing.Point point2 = new System.Drawing.Point(x, y + 100);
                //
                //         // Draw line to screen.
                //         g.DrawLine(blackPen, point1, point2);
                //         g.Flush();
                //     }
                // });
            }
        }
        
        public class FakeEventForDragPreview
        {
            public float StartTime;
            public float EndTime;
            public int Row;
        }

        public void ClampMoveResizeDragFrameDelta()
        {
            var snap = GetFrameSnap();
            var events = GetCurrentDragActions();
            if (CurrentDragType == DragTypes.ResizeLeft)
            {
                foreach (var ev in events)
                {
                    MoveResizeDragFrameDelta = (float)Math.Min(MoveResizeDragFrameDelta, (ev.EndTime - ev.StartTime) - snap);
                    MoveResizeDragFrameDelta = (float)Math.Max(MoveResizeDragFrameDelta, 0 - ev.StartTime);
                }
            }
            else if (CurrentDragType == DragTypes.ResizeRight)
            {
                foreach (var ev in events)
                {
                    MoveResizeDragFrameDelta = (float)Math.Max(MoveResizeDragFrameDelta, (ev.StartTime - ev.EndTime) + snap);
                    MoveResizeDragFrameDelta = (float)Math.Max(MoveResizeDragFrameDelta, 0 - ev.EndTime + snap);
                }
            }
            else if (CurrentDragType == DragTypes.Move)
            {
                foreach (var ev in events)
                {
                    MoveResizeDragFrameDelta = (float)Math.Max(MoveResizeDragFrameDelta, 0 - ev.StartTime);

                    MoveResizeDragRowDelta = (int)Math.Max(MoveResizeDragRowDelta, -ev.TrackIndex);
                }
            }

        }

        //public List<FakeEventForDragPreview> FakeEventsForDragPreview = new List<FakeEventForDragPreview>();

        public float MoveResizeDragFrameDelta = 0;
        public int MoveResizeDragRowDelta = 0;

        public List<FakeEventForDragPreview> GetFakeActionsForDragPreview()
        {
            var result = new List<FakeEventForDragPreview>();
            if (CurrentDragType is DragTypes.ResizeLeft or DragTypes.ResizeRight or DragTypes.Move)
            {
                var dragEvents = GetCurrentDragActions();
                foreach (var de in dragEvents)
                {
                    var ev = new FakeEventForDragPreview()
                    {
                        StartTime = de.StartTime + (CurrentDragType is DragTypes.ResizeLeft or DragTypes.Move ? MoveResizeDragFrameDelta : 0),
                        EndTime = de.EndTime + (CurrentDragType is DragTypes.ResizeRight or DragTypes.Move ? MoveResizeDragFrameDelta : 0),
                        Row = de.FixAndGetRow(Graph.AnimRef) + (CurrentDragType == DragTypes.Move ? MoveResizeDragRowDelta : 0),
                    };
                    

                    if (CurrentDragType is DragTypes.ResizeLeft)
                    {
                        var frameSnap = GetFrameSnap();
                        ev.StartTime = (float)Math.Min(ev.StartTime, ev.EndTime - frameSnap);
                    }
                    else if (CurrentDragType is DragTypes.ResizeRight)
                    {
                        var frameSnap = GetFrameSnap();
                        ev.EndTime = (float)Math.Max(ev.EndTime, ev.StartTime + frameSnap);
                    }
                    

                    result.Add(ev);
                }
            }
            return result;
        }

        public List<DSAProj.Action> GetCurrentDragActions()
        {
            return CurrentDragActions.ToList();
        }

        public double GetFrameSnap()
        {
            double frameDuration = 0;

            if (Graph.MainScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS60)
                frameDuration = (TaeExtensionMethods.TAE_FRAME_60);
            else if (Graph.MainScreen.Config.ActionSnapType == TaeConfigFile.ActionSnapTypes.FPS30)
                frameDuration = (TaeExtensionMethods.TAE_FRAME_30);

            return frameDuration;
        }

        public float GetHorizontalDragDistanceInFrameSnaps()
        {
            var distanceInTime = (CurrentMouseSustainPoint.X - CurrentMouseStartPoint.X);
            float sign = distanceInTime > 0 ? 1 : -1;
            distanceInTime = Math.Abs(distanceInTime);
            var frameSnap = GetFrameSnap();
            return (frameSnap >= 0 ? (float)(Math.Round(distanceInTime / frameSnap) * frameSnap) : distanceInTime) * sign;
        }

        public bool IsMouseOnEvent(DSAProj.Action ev, Vector2 inputMousePos)
        {
            var mouseHoverLocation = Graph.ScrollViewer.RoundedScroll + inputMousePos - Graph.WholeScrollViewerRect.TopLeftCorner();

            int x = (int)(ev.StartTime * Graph.LayoutManager.SecondsPixelSize);
            int y = (int)(ev.FixAndGetRow(Graph.AnimRef) * Graph.LayoutManager.RowHeight);
            int w = (int)(ev.EndTime * Graph.LayoutManager.SecondsPixelSize) - x;
            int h = (int)(Graph.LayoutManager.RowHeight);
            var eventRect = new Rectangle(x, y, w, h);

            // Mouse on same row as event.
            if (mouseHoverLocation.Y >= eventRect.Top && mouseHoverLocation.Y <= eventRect.Bottom)
            {
                if (mouseHoverLocation.X >= eventRect.Left && mouseHoverLocation.X <= eventRect.Right)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateCurrentDrag(FancyInputHandler input, float deltaTime, bool isReadOnly, bool forceReleaseDrag)
        {
            var safetyRect = new Rectangle(Graph.WholeScrollViewerRect.X,
                Graph.WholeScrollViewerRect.Y - Graph.TimeLineHeight,
                Graph.WholeScrollViewerRect.Width - Graph.ScrollViewer.ScrollBarThickness,
                Graph.WholeScrollViewerRect.Height + Graph.TimeLineHeight - Graph.ScrollViewer.ScrollBarThickness);

            if (OSD.Hovered)
            {
                forceReleaseDrag = true;
            }

            if (!forceReleaseDrag && input.LeftClickHeld && !safetyRect.Contains(input.LeftClickDownAnchor.ToPoint()))
            {
                CurrentDragType = DragTypes.InteractingWithOtherPanels;
                IsFirstUpdateCycleOfDrag = true;
                return;
            }

            var currentCursor = MouseCursorType.Arrow;


            if (isReadOnly && CurrentDragType is DragTypes.Move or DragTypes.ResizeLeft or DragTypes.ResizeRight)
            {
                ResetAllDrag();
            }


            if (!forceReleaseDrag)
                SetMousePointGenericToMousePoint(input.MousePosition - Graph.WholeScrollViewerRect.TopLeftCorner());

            if (!forceReleaseDrag && input.LeftClickDown)
            {
                //Check for timeline drag here
                if (input.LeftClickDownAnchor.X >= Graph.Rect.X + Graph.ActionTrackNamePaneEffectiveWidth
                    && input.LeftClickDownAnchor.X <= Graph.Rect.Right
                    && input.LeftClickDownAnchor.Y >= Graph.Rect.Top
                    && input.LeftClickDownAnchor.Y <= Graph.Rect.Top + Graph.TimeLineHeight)
                {
                    WasPlayingBeforeTimelineDrag = Graph.PlaybackCursor.IsPlaying;
                    Graph.PlaybackCursor.IsPlaying = false;
                    Graph.PlaybackCursor.Scrubbing = true;
                    if (Main.Config.NewRelativeTimelineScrubEnabled)
                    {
                        if (Main.Config.NewRelativeTimelineScrubHideMouseCursor)
                            Program.MainInstance.IsMouseVisible = false;
                        CurrentDragType = DragTypes.TimelineScrub_Relative;
                        if (Main.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor)
                            MoveMouseToTimelineLocation(Main.Input);
                        else
                            Main.Input.LockMouseCursor((int)Math.Round(Main.Input.LeftClickDownAnchor.X * Main.DPI), 
                                (int)Math.Round(Main.Input.LeftClickDownAnchor.Y * Main.DPI));
                    }
                    else
                    {
                        CurrentDragType = DragTypes.TimelineScrub;
                    }

                    Graph.ActionSim.IsMouseClickScrub = true;

                    IsFirstUpdateCycleOfDrag = true;
                }
                else
                {

                }

                //If drag type still none, Check for move/resize drags here
                if (CurrentDragType == DragTypes.None)
                {
                    var mouseHoverLocation = CurrentMouseHoverPoint * new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);

                    var actions = Graph.GetActionListCopy_UsesLock(useGhostIfAvailable: true);

                    bool checkActionDrag(DSAProj.Action act)
                    {
                        if (isReadOnly)
                            return false;

                        int x = (int)(act.StartTime * Graph.LayoutManager.SecondsPixelSize);
                        int y = (int)(act.FixAndGetRow(Graph.AnimRef) * Graph.LayoutManager.RowHeight);
                        int w = (int)(act.EndTime * Graph.LayoutManager.SecondsPixelSize) - x;
                        int h = (int)(Graph.LayoutManager.RowHeight);
                        var eventRect = new Rectangle(x, y, w, h);

                        // Mouse on same row as action.
                        if (mouseHoverLocation.Y >= eventRect.Top && mouseHoverLocation.Y <= eventRect.Bottom)
                        {
                            // Mouse on left edge of action.
                            if (mouseHoverLocation.X >= (eventRect.Left - 4) && mouseHoverLocation.X <= (eventRect.Left + 4))
                            {
                                currentCursor = MouseCursorType.DragX;
                                CurrentDragType = DragTypes.ResizeLeft;
                                IsFirstUpdateCycleOfDrag = true;
                            }
                            else if (mouseHoverLocation.X >= (eventRect.Right - 4) && mouseHoverLocation.X <= (eventRect.Right + 4))
                            {
                                currentCursor = MouseCursorType.DragX;
                                CurrentDragType = DragTypes.ResizeRight;
                                IsFirstUpdateCycleOfDrag = true;
                            }
                            else if (mouseHoverLocation.X >= (eventRect.Left + 4) && mouseHoverLocation.X <= (eventRect.Right - 4))
                            {
                                currentCursor = MouseCursorType.Arrow;
                                CurrentDragType = DragTypes.Move;
                                IsFirstUpdateCycleOfDrag = true;
                            }

                            // If one of the move/resize action drags was just now selected
                            if (CurrentDragType != DragTypes.None)
                            {
                                // If using old action select behavior then select whatever action is being dragged if it isn't already selected, overriding selection.
                                if (Main.Config.UseOldActionSelectBehavior && !MainScreen.NewSelectedActions.Contains(act))
                                {
                                    MainScreen.NewSelectedActions = new List<DSAProj.Action> { act };
                                }

                                // Take a snapshot of the selected actions and associate them with the drag for sanity
                                CurrentDragActions = MainScreen.NewSelectedActions.ToList();

                                // Store mouse start position
                                SetDragStartPosToMousePoint(input.LeftClickDownAnchor - Graph.WholeScrollViewerRect.TopLeftCorner());

                                CurrentDragMouseAccumulatedPixels = Vector2.Zero;
                                
                                return true;
                            }
                        }

                        return false;
                    }

                    bool foundDrag = false;

                    if (!isReadOnly)
                    {

                        foreach (var ev in Graph.MainScreen.NewSelectedActions)
                        {
                            if (checkActionDrag(ev))
                            {
                                foundDrag = true;
                                break;
                            }
                        }

                        if (!foundDrag)
                        {
                            foreach (var act in actions)
                            {
                                // If not using the old action selection behavior then ignore dragging on actions that are not selected
                                if (!Main.Config.UseOldActionSelectBehavior && !Graph.MainScreen.NewSelectedActions.Contains(act))
                                    continue;

                                if (checkActionDrag(act))
                                {
                                    foundDrag = true;
                                    break;
                                }
                            }
                        }
                    }

                    
                }


                //TODO: Check for row name interact here

                // If drag type still none, check generic drag
                if (CurrentDragType == DragTypes.None)
                {
                    CurrentDragType = DragTypes.SelectionRect;
                    IsFirstUpdateCycleOfDrag = true;
                    SetDragStartPosToMousePoint(input.LeftClickDownAnchor - Graph.WholeScrollViewerRect.TopLeftCorner());
                    CurrentDragMouseAccumulatedPixels = Vector2.Zero;
                }
            }

            if (!forceReleaseDrag && input.LeftClickHeld)
            {
                if (input.RightClickDown)
                {
                    CurrentDragType = DragTypes.CancelledDragStillHoldingMouse;
                    input.UnlockMouseCursor();
                }

                CurrentDragMouseAccumulatedPixels += new Vector2(MathF.Abs(input.MousePositionDelta.X), MathF.Abs(input.MousePositionDelta.Y));
                
                SetDragSustainPosToMousePoint(input.MousePosition - Graph.WholeScrollViewerRect.TopLeftCorner());

                Vector2 dragRectTopLeft = new Vector2(Math.Min(CurrentMouseStartPoint.X, CurrentMouseSustainPoint.X), Math.Min(CurrentMouseStartPoint.Y, CurrentMouseSustainPoint.Y)) * new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);
                Vector2 dragRectBottomRight = new Vector2(Math.Max(CurrentMouseStartPoint.X, CurrentMouseSustainPoint.X), Math.Max(CurrentMouseStartPoint.Y, CurrentMouseSustainPoint.Y)) * new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);

                var dragRect = new Rectangle((int)dragRectTopLeft.X, (int)dragRectTopLeft.Y, (int)(dragRectBottomRight.X - dragRectTopLeft.X), (int)(dragRectBottomRight.Y - dragRectTopLeft.Y));

                if (CurrentDragType == DragTypes.SelectionRect)
                {
                    ClearDragActions();
                    var actions = Graph.GetActionListCopy_UsesLock(useGhostIfAvailable: true);
                    foreach (var act in actions)
                    {
                        int x = (int)(act.StartTime * Graph.LayoutManager.SecondsPixelSize);
                        int y = (int)(act.FixAndGetRow(Graph.AnimRef) * Graph.LayoutManager.RowHeight);
                        int w = (int)(act.EndTime * Graph.LayoutManager.SecondsPixelSize) - x;
                        int h = (int)(Graph.LayoutManager.RowHeight);
                        var actionRect = new Rectangle(x, y, w, h);
                        if (dragRect.Intersects(actionRect) && Graph.ScrollViewer.RelativeViewport.Intersects(actionRect))
                        {
                            if (!CurrentDragActions.Contains(act))
                                CurrentDragActions.Add(act);
                        }
                    }

                    
                }

                // CurrentDragType will be reset to None before reaching here if isReadOnly is true and a resize or drag is trying to happen, so no code is needed here.

                else if (!isReadOnly && CurrentDragType is DragTypes.ResizeLeft or DragTypes.ResizeRight or DragTypes.Move)
                {
                    SetDragSustainPosToMousePoint(input.MousePosition - Graph.WholeScrollViewerRect.TopLeftCorner());
                    MoveResizeDragFrameDelta = GetHorizontalDragDistanceInFrameSnaps();

                    var distanceInRows = (CurrentMouseSustainPoint.Y - CurrentMouseStartPoint.Y);
                    MoveResizeDragRowDelta = (int)Math.Round(distanceInRows);


                    ClampMoveResizeDragFrameDelta();

                    if (CurrentDragType is DragTypes.ResizeLeft or DragTypes.ResizeRight)
                        currentCursor = MouseCursorType.DragX;

                    if (Main.Config.ScrollWhileDraggingActions_Enabled)
                    {
                        float totalDragDist = (new Vector2(CurrentDragMouseAccumulatedPixels.X, 0)).Length();
                        float s = (totalDragDist - Main.Config.ScrollWhileDraggingActions_DragDistBlendMin)
                                  / (Main.Config.ScrollWhileDraggingActions_DragDistBlendMax -
                                     Main.Config.ScrollWhileDraggingActions_DragDistBlendMin);
                        if (s < 0)
                            s = 0;
                        if (s > 1)
                            s = 1;

                        if (Main.Config.ScrollWhileDraggingActions_DragDistBlendMax <= 0)
                            s = 1;
                        
                        Layout.ScrollTowardTime(CurrentMouseSustainPoint.X, deltaTime, 
                            Main.Config.ScrollWhileDraggingActions_ThreshMin, 
                            Main.Config.ScrollWhileDraggingActions_ThreshMax, 
                            Main.Config.ScrollWhileDraggingActions_Speed,
                            Main.Config.ScrollWhileDraggingActions_LimitSpeed,
                            Main.Config.ScrollWhileDraggingActions_SpeedPower,
                            s);
                    }

                    

                    //Graph?.ViewportInteractor?.NewScrub(absolute: false, time: 0, forceRefreshTimeact: true);
                    //Main.MainThreadLazyDispatch(() =>
                    //{
                    //    Graph?.ViewportInteractor?.CurrentModel?.NewForceSyncUpdate();
                    //}, frameDelay: 1);
                }

                else if (CurrentDragType == DragTypes.TimelineScrub_Relative)
                {
                    if (IsFirstUpdateCycleOfDrag && Main.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor)
                    {
                        // Move time to mouse location
                        SetDragSustainPosToMousePoint(input.MousePosition - Graph.WholeScrollViewerRect.TopLeftCorner());
                        var time = CurrentMouseSustainPoint.X;
                        if (time < 0)
                            time = 0;
                        Graph.ViewportInteractor.ActionSim.GhettoFix_DisableSimulationTemporarily = 4;
                        try
                        {
                            Graph.PlaybackCursor.CurrentTime = time;
                            Graph.PlaybackCursor.StartTime = time;
                            Graph.PlaybackCursor.Scrubbing = true;
                            Graph.PlaybackCursor.IsPlaying = false;
                            Graph.PlaybackCursor.UpdateScrubbing();
                        }
                        finally
                        {
                            //Graph.ViewportInteractor.ActionSim.GhettoFix_DisableSimulationTemporarily = false;
                        }
                    }
                    
                    if (Main.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor)
                        MoveMouseToTimelineLocation(Main.Input);
                    else
                        Main.Input.LockMouseCursor((int)Math.Round(Main.Input.LeftClickDownAnchor.X * Main.DPI), 
                            (int)Math.Round(Main.Input.LeftClickDownAnchor.Y * Main.DPI));
                    
                    if (Main.Config.NewRelativeTimelineScrubHideMouseCursor)
                        Program.MainInstance.IsMouseVisible = false;
                    
                    
                }
                

                else if (CurrentDragType == DragTypes.TimelineScrub)
                {
                    // Move time to mouse location
                    SetDragSustainPosToMousePoint(input.MousePosition - Graph.WholeScrollViewerRect.TopLeftCorner());
                    var time = CurrentMouseSustainPoint.X;
                    if (time < 0)
                        time = 0;
                    // Graph.PlaybackCursor.CurrentTime = time;
                    // Graph.PlaybackCursor.StartTime = time;
                    // Graph.PlaybackCursor.Scrubbing = true;

                    if (IsFirstUpdateCycleOfDrag)
                    {
                        Graph.ViewportInteractor.ActionSim.GhettoFix_DisableSimulationTemporarily = 4;
                        try
                        {
                            Graph.PlaybackCursor.CurrentTime = time;
                            Graph.PlaybackCursor.StartTime = time;
                            Graph.PlaybackCursor.Scrubbing = true;
                            Graph.PlaybackCursor.IsPlaying = false;
                            Graph.PlaybackCursor.UpdateScrubbing();
                        }
                        finally
                        {
                            //Graph.ViewportInteractor.ActionSim.GhettoFix_DisableSimulationTemporarily = false;
                        }
                       
                    }
                    else
                    {
                        Graph.PlaybackCursor.CurrentTime = time;
                        Graph.PlaybackCursor.StartTime = time;
                        Graph.PlaybackCursor.Scrubbing = true;
                        Graph.PlaybackCursor.IsPlaying = false;
                        Graph.PlaybackCursor.UpdateScrubbing();
                    }


                    if (!Main.Input.ShiftHeld)
                    {
                        float mouseOnscreenRatioX =
                            (float)(input.MousePosition.X - (Graph.Rect.X + Graph.ActionTrackNamePaneEffectiveWidth)) /
                            (float)(Graph.Rect.Width - Graph.ActionTrackNamePaneEffectiveWidth);
                        //mouseOnscreenRatioX = MathHelper.Clamp(mouseOnscreenRatioX, 0, 1);
                        if (mouseOnscreenRatioX <= 0.4f)
                        {
                            float lerpS = (1 - (mouseOnscreenRatioX * (1f / 0.4f)));
                            lerpS *= lerpS;
                            Graph.LayoutManager.ScrollToPlaybackCursor(lerpS * 0.1f, modTime: false, clampTime: true);
                        }
                        else if (mouseOnscreenRatioX >= 0.6f)
                        {
                            float lerpS = ((mouseOnscreenRatioX - 0.6f)) * (1f / 0.4f);
                            lerpS *= lerpS;
                            Graph.LayoutManager.ScrollToPlaybackCursor(lerpS * 0.1f, modTime: false, clampTime: true);
                        }
                    }
                }

                

                IsFirstUpdateCycleOfDrag = false;
            }
            else
            {
                CurrentDragMouseAccumulatedPixels = Vector2.Zero;
                
                // Upon letting go of left click
                if (input.LeftClickUp)
                {
                    if (CurrentDragType == DragTypes.SelectionRect)
                    {
                        var dragActions = GetCurrentDragActions();

                        if (input.ShiftHeld)
                            Graph.MainScreen.NewSelectedActions.AddRange(dragActions);
                        else if (input.CtrlHeld)
                            Graph.MainScreen.NewSelectedActions.RemoveAll(x => dragActions.Contains(x));
                        else
                            Graph.MainScreen.NewSelectedActions = dragActions.ToList();
                    }
                    else if (!isReadOnly && CurrentDragType is DragTypes.ResizeLeft)
                    {
                        var dragActions = GetCurrentDragActions();

                        MainScreen.CurrentAnimUndoMan.NewAction(() =>
                        {
                            foreach (var act in dragActions)
                            {
                                act.StartTime += MoveResizeDragFrameDelta;
                                var frameSnap = GetFrameSnap();
                                act.StartTime = (float)Math.Min(act.StartTime, act.EndTime - frameSnap);
                            }
                        }, () => { }, "Resize left of action(s)");

                        Graph.AnimRef.SAFE_SetIsModified(true);
                    }
                    else if (!isReadOnly && CurrentDragType is DragTypes.ResizeRight)
                    {
                        MainScreen.CurrentAnimUndoMan.NewAction(() =>
                        {
                            var dragActions = GetCurrentDragActions();
                            foreach (var act in dragActions)
                            {
                                act.EndTime += MoveResizeDragFrameDelta;
                                var frameSnap = GetFrameSnap();
                                act.EndTime = (float)Math.Max(act.EndTime, act.StartTime + frameSnap);
                            }
                        }, () => { }, "Resize right of action(s)");

                        Graph.AnimRef.SAFE_SetIsModified(true);
                    }
                    else if (!isReadOnly && CurrentDragType is DragTypes.Move)
                    {
                        var dragActions = GetCurrentDragActions();

                        MainScreen.CurrentAnimUndoMan.NewAction(() =>
                        {
                            int maxRow = 0;
                            foreach (var act in dragActions)
                            {
                                act.StartTime += MoveResizeDragFrameDelta;
                                act.EndTime += MoveResizeDragFrameDelta;
                                act.TrackIndex += MoveResizeDragRowDelta;
                                // Failsafe
                                if (act.TrackIndex < 0)
                                    act.TrackIndex = 0;
                                if (act.TrackIndex > maxRow) 
                                    maxRow = act.TrackIndex;
                            }
                            Graph.AnimRef.SAFE_EnsureMinimumActionTrackCount(maxRow + 1);
                        }, () => { }, "Move action(s)");

                        Graph.AnimRef.SAFE_SetIsModified(true);
                    }
                    else if (CurrentDragType is DragTypes.TimelineScrub_Relative)
                    {
                        if (Main.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor)
                            MoveMouseToTimelineLocation(input);
                        Graph.PlaybackCursor.IsPlaying = WasPlayingBeforeTimelineDrag;
                        if (Graph.PlaybackCursor.IsPlaying)
                            Graph.PlaybackCursor.JustStartedPlaying = true;
                        Graph.PlaybackCursor.Scrubbing = false;
                        Program.MainInstance.IsMouseVisible = true;
                    }
                    else if (CurrentDragType is DragTypes.TimelineScrub)
                    {
                        Graph.PlaybackCursor.IsPlaying = WasPlayingBeforeTimelineDrag;
                        if (Graph.PlaybackCursor.IsPlaying)
                            Graph.PlaybackCursor.JustStartedPlaying = true;
                        Graph.PlaybackCursor.Scrubbing = false;
                    }


                    Graph?.ViewportInteractor?.NewScrub(absolute: false, time: 0, forceRefreshTimeact: true);
                    Main.MainThreadLazyDispatch(() =>
                    {
                        Graph?.ViewportInteractor?.CurrentModel?.NewForceSyncUpdate();
                    }, frameDelay: 1);
                }

                // Mouse not held, hovering over drags

                SetMouseHoverPosToMousePoint(input.MousePosition - Graph.WholeScrollViewerRect.TopLeftCorner());
                //DEBUG_ACTIONS.Clear();

                var mouseHoverLocation = CurrentMouseHoverPoint * new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);

                bool foundRightClickAction = false;

                DSAProj.Action nextHoverAction = null;
                DSAProj.Action nextHoverAction_NeedsNoSelection = null;

                var selActions = Graph.MainScreen.NewSelectedActions.ToList();

                var virtualViewportRect = Graph.ScrollViewer.RelativeViewport;
                
                foreach (var act in selActions)
                {
                    // If not using the old event selection behavior then ignore dragging on events that are not selected
                    if (!Main.Config.UseOldActionSelectBehavior && !Graph.MainScreen.NewSelectedActions.Contains(act))
                        continue;
                    int x = (int)(act.StartTime * Graph.LayoutManager.SecondsPixelSize);
                    int y = (int)(act.FixAndGetRow(Graph.AnimRef) * Graph.LayoutManager.RowHeight);
                    int w = (int)(act.EndTime * Graph.LayoutManager.SecondsPixelSize) - x;
                    int h = (int)(Graph.LayoutManager.RowHeight);
                    var actionRect = new Rectangle(x, y, w, h);

                    //DEBUG_ACTIONS.Add(actionRect);

                    // Mouse on same row as action.
                    if (!MenuBar.IsAnyMenuOpen && !DialogManager.AnyDialogsShowing && Graph.ScrollViewerContentsRect.Contains(Main.Input.MousePositionPoint) &&
                        virtualViewportRect.Intersects(actionRect) &&
                        mouseHoverLocation.Y >= actionRect.Top && mouseHoverLocation.Y <= actionRect.Bottom)
                    {
                        // Mouse on left edge of action
                        if (!isReadOnly && mouseHoverLocation.X >= (actionRect.Left - 4) && mouseHoverLocation.X <= (actionRect.Left + 4))
                        {
                            currentCursor = MouseCursorType.DragX;
                        }
                        else if (!isReadOnly && mouseHoverLocation.X >= (actionRect.Right - 4) && mouseHoverLocation.X <= (actionRect.Right + 4))
                        {
                            currentCursor = MouseCursorType.DragX;
                        }
                        else if (!isReadOnly && mouseHoverLocation.X >= (actionRect.Left + 4) && mouseHoverLocation.X <= (actionRect.Right - 4))
                        {
                            currentCursor = MouseCursorType.Arrow;
                        }



                        if (mouseHoverLocation.X >= actionRect.Left && mouseHoverLocation.X <= actionRect.Right)
                        {
                            if (nextHoverAction == null)
                                nextHoverAction = act;
                            if (!foundRightClickAction && input.RightClickDown)
                            {
                                Graph.ViewportInteractor.ActionSim.NewSimulation_DoRightClick(act);
                                foundRightClickAction = true;
                            }
                        }


                    }
                }

                //DEBUG_MOUSE = mouseHoverLocation;
                var actions = Graph.GetActionListCopy_UsesLock();
                foreach (var act in actions)
                {
                    act.CheckRefreshGUID();

                    int x = (int)(act.StartTime * Graph.LayoutManager.SecondsPixelSize);
                    int y = (int)(act.FixAndGetRow(Graph.AnimRef) * Graph.LayoutManager.RowHeight);
                    int w = (int)(act.EndTime * Graph.LayoutManager.SecondsPixelSize) - x;
                    int h = (int)(Graph.LayoutManager.RowHeight);
                    var actionRect = new Rectangle(x, y, w, h);
                    // Mouse on same row as action.
                    if (!MenuBar.IsAnyMenuOpen && !DialogManager.AnyDialogsShowing && Graph.ScrollViewerContentsRect.Contains(Main.Input.MousePositionPoint) &&
                        virtualViewportRect.Intersects(actionRect) &&
                        mouseHoverLocation.Y >= actionRect.Top && mouseHoverLocation.Y <= actionRect.Bottom)
                    {
                        // Mouse in horizontal bounds of action.
                        if (mouseHoverLocation.X >= actionRect.Left && mouseHoverLocation.X <= actionRect.Right)
                        {
                            if (nextHoverAction_NeedsNoSelection == null)
                                nextHoverAction_NeedsNoSelection = act;
                            if (!foundRightClickAction && input.RightClickDown)
                            {
                                Graph.ViewportInteractor.ActionSim.NewSimulation_DoRightClick(act);
                                foundRightClickAction = true;
                            }
                        }
                    }

                    // If not using the old event selection behavior then ignore dragging on events that are not selected
                    if (!Main.Config.UseOldActionSelectBehavior && !Graph.MainScreen.NewSelectedActions.Contains(act))
                        continue;
                    

                    //DEBUG_ACTIONS.Add(actionRect);

                    // Mouse on same row as action.
                    if (!MenuBar.IsAnyMenuOpen && !DialogManager.AnyDialogsShowing && Graph.ScrollViewerContentsRect.Contains(Main.Input.MousePositionPoint) &&
                        virtualViewportRect.Intersects(actionRect) && mouseHoverLocation.Y >= actionRect.Top && mouseHoverLocation.Y <= actionRect.Bottom)
                    {
                        // Mouse on left edge of action
                        if (!isReadOnly && mouseHoverLocation.X >= (actionRect.Left - 4) && mouseHoverLocation.X <= (actionRect.Left + 4))
                        {
                            currentCursor = MouseCursorType.DragX;
                        }
                        else if (!isReadOnly && mouseHoverLocation.X >= (actionRect.Right - 4) && mouseHoverLocation.X <= (actionRect.Right + 4))
                        {
                            currentCursor = MouseCursorType.DragX;
                        }
                        else if (!isReadOnly && mouseHoverLocation.X >= (actionRect.Left + 4) && mouseHoverLocation.X <= (actionRect.Right - 4))
                        {
                            currentCursor = MouseCursorType.Arrow;
                        }



                        if (mouseHoverLocation.X >= actionRect.Left && mouseHoverLocation.X <= actionRect.Right)
                        {
                            if (nextHoverAction == null)
                                nextHoverAction = act;
                            if (!foundRightClickAction && input.RightClickDown)
                            {
                                Graph.ViewportInteractor.ActionSim.NewSimulation_DoRightClick(act);
                                foundRightClickAction = true;
                            }
                        }
                    }
                }

                Graph.MainScreen.NewHoverAction = nextHoverAction;
                Graph.MainScreen.NewHoverAction_NeedsNoSelection = nextHoverAction_NeedsNoSelection;



                ResetAllDrag();

            }
            input.CursorType = currentCursor;

            if (CurrentDragType != DragTypes.None)
            {
                Layout.ScrollToRect = null;
            }
        }
        private void ClearDragActions()
        {
            CurrentDragActions.Clear();
        }
        private void SetDragStartPosToMousePoint(Vector2 mousePoint)
        {
            var absoluteMousePoint = Graph.ScrollViewer.RoundedScroll + mousePoint;
            CurrentMouseStartPoint = absoluteMousePoint / new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);
            CurrentMouseStartPoint_Pixels = absoluteMousePoint;
        }
        private void SetDragSustainPosToMousePoint(Vector2 mousePoint)
        {
            var absoluteMousePoint = Graph.ScrollViewer.RoundedScroll + mousePoint;
            CurrentMouseSustainPoint = absoluteMousePoint / new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);
            CurrentMouseSustainPoint_Pixels = absoluteMousePoint;
        }
        private void SetMouseHoverPosToMousePoint(Vector2 mousePoint)
        {
            var absoluteMousePoint = Graph.ScrollViewer.RoundedScroll + mousePoint;
            CurrentMouseHoverPoint = absoluteMousePoint / new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);
            CurrentMouseHoverPoint_Pixels = absoluteMousePoint;
        }

        private void SetMousePointGenericToMousePoint(Vector2 mousePoint)
        {
            var absoluteMousePoint = Graph.ScrollViewer.RoundedScroll + mousePoint;
            CurrentMousePointGeneric = absoluteMousePoint / new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight);
            CurrentMousePointGeneric_Pixels = absoluteMousePoint;
        }

        public readonly NewGraph Graph;
        public TaeEditorScreen MainScreen => Graph.MainScreen;
        public NewGraphInput(NewGraph graph)
        {
            Graph = graph;
            WindowsMouseHook.RawMouseMoved += RawMouseMoved;
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                WindowsMouseHook.RawMouseMoved -= RawMouseMoved;
                _disposed = true;
            }
        }

        //private float newRelativeTimelineDragTimeDeltaLastFrame = 0;

        private void RawMouseMoved(int x, int y)
        {
            if (Main.Input.LeftClickHeld && CurrentDragType == DragTypes.TimelineScrub_Relative)
            {
                bool speedLock = Main.Input.ShiftHeld;

                if (Main.Config.NewRelativeTimelineScrubLockToAnimSpeed)
                    speedLock = !speedLock;
                
                if (Main.Config.NewRelativeTimelineScrubHideMouseCursor)
                    Program.MainInstance.IsMouseVisible = false;
                
                var scrubSpeedMult = Main.Config.NewRelativeTimelineScrubSpeed;
                // if (speedLock)
                //     scrubSpeedMult = 1;

                float secondsPixelSize = Graph.LayoutManager.SecondsPixelSize;

                if (!Main.Config.NewRelativeTimelineScrubScalesWithZoom)
                    secondsPixelSize = 500;
                
                float timeDelta = (x * 0.5f * scrubSpeedMult) / secondsPixelSize;

                // float lerpS = (1 - Main.Config.NewRelativeTimelineScrubSmoothing);
                // if (speedLock)
                //     lerpS = 0.5f;
                
                
                // if (lerpS <= 0)
                //     timeDelta = 0;
                // else if (lerpS < 1)
                //     timeDelta = MathHelper.Lerp(newRelativeTimelineDragTimeDeltaLastFrame, timeDelta, lerpS);
                
               
                
                if (speedLock)
                {
                    float speedLimit = Main.DELTA_UPDATE * Graph.PlaybackCursor.EffectivePlaybackSpeed;
                    float speedThreshold = speedLimit;
                    if (timeDelta >= speedThreshold)
                        timeDelta = speedLimit;
                    else if (timeDelta <= -speedThreshold)
                        timeDelta = -speedLimit;
                    // else
                    //     timeDelta = 0;
                }

                if (IsFirstUpdateCycleOfDrag && Main.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor)
                {
                    timeDelta = 0;
                }
                
                if (timeDelta != 0)
                {

                    Graph.PlaybackCursor.NewApplyRelativeScrub(timeDelta);
                    //Graph.PlaybackCursor.UpdateScrubbing();

                    

                    //Graph.LayoutManager.ScrollToPlaybackCursor(1, modTime: false);
                }
                
                if (Main.Config.NewRelativeTimelineScrubScrollScreen)
                    Graph.LayoutManager.ScrollToPlaybackCursor(-1, modTime: true);

                if (Main.Config.NewRelativeTimelineScrubSyncMouseToPlaybackCursor)
                    MoveMouseToTimelineLocation(Main.Input);
                else
                    Main.Input.LockMouseCursor((int)Math.Round(Main.Input.LeftClickDownAnchor.X * Main.DPI), 
                        (int)Math.Round(Main.Input.LeftClickDownAnchor.Y * Main.DPI));
                
                //newRelativeTimelineDragTimeDeltaLastFrame = timeDelta;
            }
        }

        public void ZoomOutOneNotch(float mouseScreenPosX)
        {
            ZoomDelta(-1, mouseScreenPosX);
        }

        public void ZoomInOneNotch(float mouseScreenPosX)
        {
            ZoomDelta(1, mouseScreenPosX);
        }

        public void ResetZoom(float mouseScreenPosX)
        {
            SetZoom(NewGraphLayoutManager.SecondsPixelSizeDefault, mouseScreenPosX);
        }

        public void ZoomDelta(float zoomDelta, float mouseScreenPosX)
        {
            if (zoomDelta < 0)
            {
                while (Graph.LayoutManager.SecondsPixelSize * MathF.Pow(1.25f, zoomDelta) <
                       Graph.LayoutManager.SecondsPixelSizeMin)
                {
                    zoomDelta += 1;
                    if (zoomDelta > 0)
                        zoomDelta = 0;
                }
            }

            if (zoomDelta != 0)
                SetZoom(Graph.LayoutManager.SecondsPixelSize * MathF.Pow(1.25f, zoomDelta), mouseScreenPosX);
        }

        private void SetZoom(float newSecondsPixelSize, float mouseScreenPosX)
        {
            Graph.LayoutManager.ScrollToRect = null;

            if (newSecondsPixelSize > 50000)
                return;

            float mousePosInSeconds = (mouseScreenPosX + Graph.ScrollViewer.Scroll.X) / Graph.LayoutManager.SecondsPixelSize;

            Graph.LayoutManager.SecondsPixelSize = newSecondsPixelSize;

            float previousMousePosInPixels = mousePosInSeconds * Graph.LayoutManager.SecondsPixelSize;

            Graph.ScrollViewer.Scroll.X = previousMousePosInPixels - mouseScreenPosX;
        }

        public void DoCopy()
        {
            if (Graph.IsGhostGraph)
                return;

            var clipboardContents = new DSAProj.ClipboardContents()
            {
                Actions = Graph.MainScreen.NewSelectedActions.ToList(),
            };
            var clipboardText = clipboardContents.SerializeToBytesString(dcx: false, MainScreen.Proj);
            Clipboard.SetText(clipboardText);
        }

        public void DoCut()
        {
            if (Graph.IsGhostGraph)
                return;

            var clipboardContents = new DSAProj.ClipboardContents()
            {
                Actions = Graph.MainScreen.NewSelectedActions.ToList(),
            };
            var clipboardText = clipboardContents.SerializeToBytesString(dcx: false, MainScreen.Proj);
            Clipboard.SetText(clipboardText);
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                foreach (var ev in clipboardContents.Actions)
                {
                    Graph.AnimRef.SafeAccessActions(actions =>
                    {
                        actions.Remove(ev);
                    });
                    
                    Graph.MainScreen.NewSelectedActions.Remove(ev);
                }


            }, () => { }, "Cut event(s)");
            Graph.AnimRef.SAFE_SetIsModified(true);
        }

        public void DoSelectAll()
        {
            if (Graph.IsGhostGraph)
                return;

            if (Graph != null && Graph.CanChangeSelectionRightNow)
            {
                Graph.MainScreen.NewSelectedActions.Clear();
                Graph.AnimRef.SafeAccessActions(actions =>
                {
                    foreach (var act in actions)
                    {
                        Graph.MainScreen.NewSelectedActions.Add(act);
                    }
                });
                
            }
        }

        public void DoAddNewEventAtMouse()
        {
            if (Graph.IsGhostGraph)
                return;


            int mouseRow = (int)CurrentMousePointGeneric.Y;
            float mouseTime = CurrentMousePointGeneric.X;

            // Snap to frame interval
            var frameSnap = GetFrameSnap();
            mouseTime = (float)(Math.Round(mouseTime / frameSnap) * frameSnap);

            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                int newEventType = 0;
                byte[] newEventParameterBytes = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                float newEventLength = (float)(frameSnap * 4);
                float newEventStartTime = mouseTime;

                if (mouseRow < Graph.AnimRef.SAFE_GetActionTrackCount())
                {
                    var grp = Graph.AnimRef.SAFE_GetActionTrackByIndex(mouseRow);

                    if (grp.NewActionDefaultType >= 0)
                        newEventType = grp.NewActionDefaultType;

                    if (grp.NewActionDefaultParameters != null)
                        newEventParameterBytes = grp.NewActionDefaultParameters.ToArray();

                    if (grp.NewActionDefaultLength > 0)
                        newEventLength = grp.NewActionDefaultLength;
                }
                else
                {
                    while (Graph.AnimRef.SAFE_GetActionTrackCount() <= mouseRow)
                    {
                        Graph.AnimRef.SAFE_AddActionTrack(new ActionTrack()
                        {
                            Info = new EditorInfo("Empty Row"),
                            TrackData = new SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataStruct()
                            { DataType = SoulsAssetPipeline.Animation.TAE.ActionTrack.ActionTrackDataType.TrackData0 },
                            TrackType = 0,
                        });
                    }

                    Graph.AnimRef.SAFE_EnsureMinimumActionTrackCount(mouseRow + 1);

                    //var grp = Graph.AnimRef.ActionTracks[mouseRow];
                    //if (grp.GetActions(Graph.AnimRef).Count == 0)
                    //    grp.Info.DisplayName = "0: Do Nothing";
                }


                var act = new DSAProj.Action()
                {
                    StartTime = newEventStartTime,
                    EndTime = newEventStartTime + newEventLength,
                    ParameterBytes = newEventParameterBytes,
                    Type = newEventType,
                    TrackIndex = mouseRow,
                };

                if (Graph.MainScreen.FileContainer.Proj.Template.ContainsKey(act.Type))
                {
                    act.ApplyTemplate(Graph.MainScreen.SelectedAnimCategory, Graph.MainScreen.FileContainer.Proj.Template, 0, 0, act.Type);
                }

                Graph.AnimRef.SAFE_AddAction(act);


            }, () => { }, "Add new action");


            

            Graph.AnimRef.SAFE_SetIsModified(true);
        }

        public void DoPaste(bool isAbsoluteLocation)
        {
            if (Graph.IsGhostGraph)
                return;

            var clipboardText = Clipboard.GetText();
            var clipboardContents = new DSAProj.ClipboardContents();
            bool failed = false;
            try
            {
                clipboardContents.DeserializeFromBytesString(clipboardText, dcx: false, Graph.MainScreen.FileContainer.Proj);
            }
            catch
            {
                failed = true;
            }

            if (failed || clipboardContents.Actions.Count == 0)
                return;

            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                int maxRow = 0;

                if (!isAbsoluteLocation)
                {
                    int minRow = int.MaxValue;
                    float minTime = float.MaxValue;
                    foreach (var act in clipboardContents.Actions)
                    {
                        if (act.TrackIndex < minRow)
                            minRow = act.TrackIndex;
                        if (act.StartTime < minTime)
                            minTime = act.StartTime;
                    }
                    Graph.MainScreen.NewSelectedActions.Clear();
                    
                    foreach (var act in clipboardContents.Actions)
                    {
                        act.TrackIndex -= minRow;
                        act.StartTime -= minTime;
                        act.EndTime -= minTime;

                        int mouseRow = (int)CurrentMousePointGeneric.Y;
                        float mouseTime = CurrentMousePointGeneric.X;

                        // Snap to frame interval
                        var frameSnap = GetFrameSnap();
                        mouseTime = (float)(Math.Round(mouseTime / frameSnap) * frameSnap);

                        act.TrackIndex += mouseRow;
                        act.StartTime += mouseTime;
                        act.EndTime += mouseTime;

                        if (act.TrackIndex > maxRow)
                            maxRow = act.TrackIndex;

                        
                    }

                    
                }
                else
                {
                    foreach (var act in clipboardContents.Actions)
                    {
                        if (act.TrackIndex > maxRow)
                            maxRow = act.TrackIndex;
                    }
                }

                Graph.AnimRef.SAFE_EnsureMinimumActionTrackCount(maxRow + 1);

                foreach (var act in clipboardContents.Actions)
                {
                    Graph.AnimRef.SAFE_AddAction(act);
                    Graph.MainScreen.NewSelectedActions.Add(act);
                }
            }, () => { }, "Paste event(s)");
            Graph.AnimRef.SAFE_SetIsModified(true);
        }

        public void ToggleMuteOnSelectedActions(bool uniform)
        {
            if (MainScreen.NewSelectedActions.Count < 1)
                return;

            string actionName = "Toggle mute";
            if (uniform)
                actionName += " (uniform)";
            
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                bool currentUniformState = uniform ? (MainScreen.NewSelectedActions.Any(a => a.Mute)) : false;
                foreach (var act in MainScreen.NewSelectedActions)
                {
                    if (uniform)
                        act.Mute = !currentUniformState;
                    else
                        act.Mute = !act.Mute;
                }
                Graph.AnimRef.SAFE_SetIsModified(true);
            }, actionDescription: actionName);
        }

        public void ToggleSoloOnSelectedActions(bool uniform)
        {
            if (MainScreen.NewSelectedActions.Count < 1)
                return;

            string actionName = "Toggle solo";
            if (uniform)
                actionName += " (uniform)";
            
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                bool currentUniformState = uniform ? (MainScreen.NewSelectedActions.Any(a => a.Solo)) : false;
                foreach (var act in MainScreen.NewSelectedActions)
                {
                    if (uniform)
                        act.Solo = !currentUniformState;
                    else
                        act.Solo = !act.Solo;
                }
                Graph.AnimRef.SAFE_SetIsModified(true);
            }, actionDescription: actionName);
        }
        
        public void ToggleStateInfoOnSelectedActions(bool uniform)
        {
            if (MainScreen.NewSelectedActions.Count < 1)
                return;

            string actionName = "Toggle StateInfo";
            if (uniform)
                actionName += " (uniform)";
            
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                var entityWindow = OSD.WindowEntity;

                
                
                var stateInfos = entityWindow.GetStateInfoSelectConfig_Enabled();

                
                
                var relevantStateInfos = new List<int>();
                
                foreach (var act in MainScreen.NewSelectedActions)
                {
                    var stateInfoID = act.GetStateInfoID();
                    if (stateInfoID < 0)
                        continue;
                    if (!relevantStateInfos.Contains(stateInfoID))
                        relevantStateInfos.Add(stateInfoID);
                    if (!stateInfos.ContainsKey(stateInfoID))
                        stateInfos.Add(stateInfoID, false);
                }

                if (relevantStateInfos.Count == 0 || stateInfos.Count == 0)
                    return;
                
                bool currentUniformState = uniform ? (relevantStateInfos.Any(x => stateInfos[x])) : false;
                foreach (var si in relevantStateInfos)
                {
                    
                    if (uniform)
                        stateInfos[si] = !currentUniformState;
                    else
                        stateInfos[si] = !stateInfos[si];
                }
                
                entityWindow.SetStateInfoSelectConfig_Enabled(stateInfos);
                
                Graph.AnimRef.SAFE_SetIsModified(true);
            }, actionDescription: actionName);
        }

        public void ClearAllSoloAndMute()
        {
            Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
            {
                bool anyChange = false;
                Graph.SafeAccessActionList(actions =>
                {
                    foreach (var act in actions)
                    {
                        if (act.Mute)
                        {
                            act.Mute = false;
                            anyChange = true;
                        }

                        if (act.Solo)
                        {
                            act.Solo = false;
                            anyChange = true;
                        }
                       
                        
                    }
                });
                if (anyChange)
                    Graph.AnimRef.SAFE_SetIsModified(true);
            }, actionDescription: "Clear solo/mute");
            
        }
        
        public void DeleteSelectedActions()
        {
            if (Graph.IsGhostGraph)
                return;

            var eventsToDelete = Graph.MainScreen.NewSelectedActions.ToList();
            if (eventsToDelete.Count > 0)
            {
                Graph.MainScreen.CurrentAnimUndoMan.NewAction(() =>
                {
                    foreach (var act in eventsToDelete)
                    {
                        Graph.AnimRef.SAFE_RemoveAction(act);
                        Graph.MainScreen.NewSelectedActions.Remove(act);
                    }
                }, () => { }, "Delete action(s)");
                Graph.AnimRef.SAFE_SetIsModified(true);
            }
            
        }

        public void UpdateOutsideRect(float deltaTime, bool isReadOnly)
        {
            var input = Graph.MainScreen.Input;
            UpdateCurrentDrag(input, deltaTime, isReadOnly, forceReleaseDrag: false);
        }

        public void UpdateLoop(float deltaTime, bool isReadOnly)
        {
            var input = Graph.MainScreen.Input;
            if (input.KeyDown(Microsoft.Xna.Framework.Input.Keys.Home))
            {
                Graph.ScrollViewer.Scroll.X = 0;
                if (input.ShiftHeld && !input.AltHeld && !input.CtrlHeld)
                    Graph.ScrollViewer.Scroll.Y = 0;
                Layout.ScrollToRect = null;
            }
            UpdateCurrentDrag(input, deltaTime, isReadOnly, forceReleaseDrag: false);
        }

        public void DrawGizmos()
        {
            var startPointPixels = CurrentMouseStartPoint * new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight) - Graph.ScrollViewer.RoundedScroll;
            var sustainPointPixels = CurrentMouseSustainPoint * new Vector2(Graph.LayoutManager.SecondsPixelSize, Graph.LayoutManager.RowHeight) - Graph.ScrollViewer.RoundedScroll;
            if (CurrentDragType == DragTypes.SelectionRect)
            {
                float top = Math.Min(startPointPixels.Y, sustainPointPixels.Y);
                float bottom = Math.Max(startPointPixels.Y, sustainPointPixels.Y);
                float left = Math.Min(startPointPixels.X, sustainPointPixels.X);
                float right = Math.Max(startPointPixels.X, sustainPointPixels.X);
                ImGuiDebugDrawer.DrawRect(new Vector2(left, top), new Vector2(right - left, bottom - top), Main.Colors.GuiColorActionGraphSelectionRectangleFill);
                ImGuiDebugDrawer.DrawLine(new Vector2(left, top), new Vector2(right, top), Main.Colors.GuiColorActionGraphSelectionRectangleOutline);
                ImGuiDebugDrawer.DrawLine(new Vector2(right, top), new Vector2(right, bottom), Main.Colors.GuiColorActionGraphSelectionRectangleOutline);
                ImGuiDebugDrawer.DrawLine(new Vector2(left, bottom), new Vector2(right, bottom), Main.Colors.GuiColorActionGraphSelectionRectangleOutline);
                ImGuiDebugDrawer.DrawLine(new Vector2(left, top), new Vector2(left, bottom), Main.Colors.GuiColorActionGraphSelectionRectangleOutline);
            }


            if (CurrentDragType is DragTypes.SelectionRect)
            {
                foreach (var act in CurrentDragActions)
                {
                    int row = Layout.GetRowOfAction(act);



                    float start = act.StartTime * Graph.LayoutManager.SecondsPixelSize;
                    float end = act.EndTime * Graph.LayoutManager.SecondsPixelSize;
                    var actionTopLeft = new Vector2(start, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                    var actionBottomLeft = new Vector2(start, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                    var actionBottomRight = new Vector2(end, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                    var actionTopRight = new Vector2(end, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                    var diamondPointLeft = new Vector2(start - Layout.DiamondWidth, (row * Layout.RowHeight) + Layout.RowHeight / 2f) - Graph.ScrollViewer.RoundedScroll;
                    var diamondPointRight = new Vector2(end + Layout.DiamondWidth, (row * Layout.RowHeight) + Layout.RowHeight / 2f) - Graph.ScrollViewer.RoundedScroll;

                    var eventShapeFillColor = act.NewSimulationActive ? Color.Yellow : Color.Red;
                    //ImGuiDebugDrawer.DrawTriangle(diamondPointLeft, eventTopLeft, eventBottomLeft, eventShapeFillColor);
                    //ImGuiDebugDrawer.DrawTriangle(diamondPointRight, eventBottomRight, eventTopRight, eventShapeFillColor);
                    ImGuiDebugDrawer.DrawRect(actionTopLeft, actionBottomRight - actionTopLeft, Color.White * 0.85f);
                }
            }
            else if (CurrentDragType is DragTypes.ResizeLeft or DragTypes.ResizeRight or DragTypes.Move)
            {
                Vector2 virtualViewportScrollArea = Layout.GetVirtualAreaSize().ToVector2();

                //Rectangle getEventRectangle(Event ev)
                //{
                //    int row = ev.Row;

               

                //    float start = ev.StartTime * Graph.LayoutManager.SecondsPixelSize;
                //    float end = ev.EndTime * Graph.LayoutManager.SecondsPixelSize;
                //    var eventTopLeft = new Vector2(start, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                //    var eventBottomLeft = new Vector2(start, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                //    var eventBottomRight = new Vector2(end, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                //    var eventTopRight = new Vector2(end, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                //    return new Rectangle((int)Math.Round(eventTopLeft.X), (int)Math.Round(eventTopLeft.Y), (int)Math.Round(eventBottomRight.X) - (int)Math.Round(eventTopLeft.X),
                //        (int)Math.Round(eventBottomRight.Y) - (int)Math.Round(eventTopLeft.Y));
                //}

                var fakeActions = GetFakeActionsForDragPreview();
                foreach (var act in fakeActions)
                {
                    int row = act.Row;



                    float start = act.StartTime * Graph.LayoutManager.SecondsPixelSize;
                    float end = act.EndTime * Graph.LayoutManager.SecondsPixelSize;
                    var actionTopLeft = new Vector2(start, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                    var actionBottomLeft = new Vector2(start, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                    var actionBottomRight = new Vector2(end, (row * Layout.RowHeight) + Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;
                    var actionTopRight = new Vector2(end, row * Layout.RowHeight) - Graph.ScrollViewer.RoundedScroll;

                    var diamondPointLeft = new Vector2(start - Layout.DiamondWidth, (row * Layout.RowHeight) + Layout.RowHeight / 2f) - Graph.ScrollViewer.RoundedScroll;
                    var diamondPointRight = new Vector2(end + Layout.DiamondWidth, (row * Layout.RowHeight) + Layout.RowHeight / 2f) - Graph.ScrollViewer.RoundedScroll;

                    ImGuiDebugDrawer.DrawRect(actionTopLeft, actionBottomRight - actionTopLeft, Color.White * 0.75f);
                    
                   

                    //ImGuiDebugDrawer.DrawLine(new Vector2(0, eventTopLeft.Y), new Vector2(virtualViewportScrollArea.X, eventTopLeft.Y), Color.White);
                    //ImGuiDebugDrawer.DrawLine(new Vector2(0, eventBottomRight.Y), new Vector2(virtualViewportScrollArea.X, eventBottomRight.Y), Color.White);

                    //var rect = getEventRectangle(ev);
                    var dragActions = GetCurrentDragActions();

                    Graph.AnimRef.SafeAccessActions(actions =>
                    {
                        foreach (var otherAct in actions)
                        {
                            if (dragActions.Contains(otherAct))
                                continue;

                            float minY = Math.Min(Layout.RowHeight * act.Row, Layout.RowHeight * otherAct.TrackIndex);
                            float maxY = Math.Max(Layout.RowHeight * act.Row, Layout.RowHeight * otherAct.TrackIndex) + Layout.RowHeight;

                            minY -= Graph.ScrollViewer.RoundedScroll.Y;
                            maxY -= Graph.ScrollViewer.RoundedScroll.Y;

                            if ((Math.Abs(act.StartTime - otherAct.StartTime) < 0.01f) || (Math.Abs(act.StartTime - otherAct.EndTime) < 0.01f))
                            {
                                ImGuiDebugDrawer.DrawLine(new Vector2(actionTopLeft.X, minY), new Vector2(actionTopLeft.X, maxY), Color.White);
                            }

                            if ((Math.Abs(act.EndTime - otherAct.EndTime) < 0.01f) || (Math.Abs(act.EndTime - otherAct.StartTime) < 0.01f))
                            {
                                ImGuiDebugDrawer.DrawLine(new Vector2(actionBottomRight.X, minY), new Vector2(actionBottomRight.X, maxY), Color.White);
                            }

                            if (act.Row == otherAct.TrackIndex)
                            {
                                float minX = Math.Min(Layout.SecondsPixelSize * act.StartTime, Layout.SecondsPixelSize * otherAct.StartTime);
                                minX = Math.Min(minX, Math.Min(Layout.SecondsPixelSize * act.EndTime, Layout.SecondsPixelSize * otherAct.EndTime));

                                float maxX = Math.Max(Layout.SecondsPixelSize * act.StartTime, Layout.SecondsPixelSize * otherAct.StartTime);
                                maxX = Math.Max(maxX, Math.Max(Layout.SecondsPixelSize * act.EndTime, Layout.SecondsPixelSize * otherAct.EndTime));

                                minX -= Graph.ScrollViewer.RoundedScroll.X;
                                maxX -= Graph.ScrollViewer.RoundedScroll.X;

                                ImGuiDebugDrawer.DrawLine(new Vector2(minX, actionTopLeft.Y), new Vector2(maxX, actionTopLeft.Y), Color.White);
                                ImGuiDebugDrawer.DrawLine(new Vector2(minX, actionBottomRight.Y), new Vector2(maxX, actionBottomRight.Y), Color.White);
                            }
                        }
                    });

                    
                }

                var snapInterval = GetFrameSnap();

                var dragStartPoint = CurrentMouseStartPoint;// new Vector2((float)(Math.Round(CurrentMouseStartPoint.X / snapInterval) * snapInterval), (float)(Math.Round(CurrentMouseStartPoint.Y - 0.5) + 0.5));
                var dragEndPoint = CurrentMouseSustainPoint;

                

                var dragDelta = dragEndPoint - dragStartPoint;
                dragDelta = new Vector2((float)(Math.Round(dragDelta.X / snapInterval) * snapInterval), (float)(Math.Round(dragDelta.Y)));

                dragEndPoint = dragStartPoint + dragDelta;

                if (CurrentDragType is DragTypes.ResizeLeft or DragTypes.ResizeRight)
                    dragEndPoint.Y = dragStartPoint.Y;

                Vector2 dragStartPx = dragStartPoint * new Vector2(Layout.SecondsPixelSize, Layout.RowHeight);
                Vector2 dragEndPx = dragEndPoint * new Vector2(Layout.SecondsPixelSize, Layout.RowHeight);
                Vector2 dragRightAnglePx = new Vector2(dragStartPx.X, dragEndPx.Y);

                dragStartPx -= Graph.ScrollViewer.RoundedScroll;
                dragEndPx -= Graph.ScrollViewer.RoundedScroll;
                dragRightAnglePx -= Graph.ScrollViewer.RoundedScroll;


                ImGuiDebugDrawer.DrawCircle(dragStartPx, 4f, Color.White);
                //ImGuiDebugDrawer.DrawLine(dragStartPx, dragRightAnglePx, Color.White);
                //ImGuiDebugDrawer.DrawLine(dragRightAnglePx, dragEndPx, Color.White);
                //ImGuiDebugDrawer.DrawLine(dragStartPx, dragEndPx, Color.White);

            }

            

            // TESTING
            //ImGuiDebugDrawer.DrawText($"NewGraph.CurrentDragType: {CurrentDragType}" +
            // $"\n MainScreen.CurrentDividerDragMode: {MainScreen.CurrentDividerDragMode}" +
            // $"\n MainScreen.MouseHoverKind: {MainScreen.MouseHoverKind}" +
            // $"\n MainScreen.WhereCurrentMouseClickStarted: {MainScreen.WhereCurrentMouseClickStarted}" +
            // $"\n MainScreen.WhereLastMouseClickStarted: {MainScreen.WhereLastMouseClickStarted}"
            // , Vector2.One * 32, Color.Lime);

            //foreach (var dbg in DEBUG_EVENTS)
            //{
            //    ImGuiDebugDrawer.DrawRect(dbg, Color.Cyan);
            //}
            //ImGuiDebugDrawer.DrawRect(DEBUG_MOUSE, Vector2.One * 4, Color.Red);
        }

        //TODO: Move to layout manager
        public void UpdateMiddleClickPan()
        {
            if (MainScreen.Input.MiddleClickHeld && (Graph.ScrollViewerContentsRect.Contains(MainScreen.Input.MiddleClickDownAnchor)
                || Graph.ActionTrackHeaderTextRect.Contains(MainScreen.Input.MiddleClickDownAnchor)))
            {
                //MainScreen.Input.CursorType = MouseCursorType.GrabPan;
                Layout.ScrollToRect = null;
                Graph.ScrollViewer.Scroll -= MainScreen.Input.MousePositionDelta;
                Graph.ScrollViewer.ClampScroll();
            }
        }
    }
}
