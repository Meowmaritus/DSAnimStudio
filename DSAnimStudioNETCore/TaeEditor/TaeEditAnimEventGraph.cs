using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Numerics;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditAnimEventGraph
    {
        public void AllBoxesEveryFrameUpdate()
        {
            var boxesCopy = EventBoxes.ToList();
            var entitySettings = ImguiOSD.OSD.WindowEntitySettings;
            if (entitySettings == null)
                return;
            var stateInfosEnabled = entitySettings.GetStateInfoSelectConfig_Enabled();
            foreach (var evBox in boxesCopy)
            {
                //if (evBox.MyEvent?.Template != null)
                //{
                //    if (evBox.MyEvent.Template.ContainsKey("StateInfo"))
                //    {
                //        int stateInfo = Convert.ToInt32(evBox.MyEvent.Parameters["StateInfo"]);
                //        if (stateInfosEnabled.ContainsKey(stateInfo))
                //            evBox.IsStateInfoEnabled = stateInfosEnabled[stateInfo];
                //        else
                //            evBox.IsStateInfoEnabled = true;
                //    }
                //}
                //else
                //{
                //    evBox.IsStateInfoEnabled = true;
                //}
                evBox.IsStateInfoEnabled = true;
            }
        }
            

        public void GenerateFakeDS3EventGroups(bool threadLock = false)
        {
            void doWholeThing()
            {
                if (IsSimpleRemoGroupMode)
                {
                    // REMOVE REDUNDANT EVENT GROUPS
                    MainScreen.SelectedTaeAnim.EventGroups.Clear();

                    //var cutsceneEntitySpecifyEventGroupIndexMap = new Dictionary<TAE.EventGroup.EventGroupDataStruct, int>();
                    foreach (var ev in MainScreen.SelectedTaeAnim.Events)
                    {
                        if (ev.Group == null)
                        {
                            ev.Group = new TAE.EventGroup(0);
                            ev.Group.GroupData = new TAE.EventGroup.EventGroupDataStruct();
                        }
                        else
                        {
                            var gdt = ev.Group.GroupData;
                            ev.Group = new TAE.EventGroup(ev.Group.GroupType);
                            ev.Group.GroupData = gdt;
                        }

                        MainScreen.SelectedTaeAnim.EventGroups.Add(ev.Group);
                    }
                }
                else if (IsFancyCollapsingRemoGroupMode)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    // Always clear for anything not a REMO, which is the only example of event groups actually having meanings.
                    MainScreen.SelectedTaeAnim.EventGroups.Clear();
                    GroupRegions.Clear();

                    // Only generate if supported or if user has option enabled to force use these in games where they usually aren't.
                    if (EventBoxes.Count > 0 && GameRoot.GameTypeUsesBoilerplateEventGroups)
                    {
                        int maxRow = EventBoxes.Max(evBox => evBox.Row);
                        for (int i = 0; i <= maxRow; i++)
                        {
                            var eventsInThisGroup = EventBoxes.Where(ev => ev.Row == i).ToList();
                            int fakeGroupType = eventsInThisGroup.FirstOrDefault()?.MyEvent?.Type ?? 0;
                            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                            {
                                fakeGroupType = 0;
                            }
                            var group = new TAE.EventGroup(fakeGroupType);
                            if (!GameRoot.IsGame(SoulsAssetPipeline.SoulsGames.DS3 | SoulsAssetPipeline.SoulsGames.SDT | SoulsAssetPipeline.SoulsGames.BB))
                            {
                                group.GroupData = new TAE.EventGroup.EventGroupDataStruct();
                            }

                            foreach (var ev in eventsInThisGroup)
                                ev.MyEvent.Group = group;
                            MainScreen.SelectedTaeAnim.EventGroups.Add(group);
                            //Graph.GroupRegions.Add(new TaeEventGroupRegion(Graph, SelectedTaeAnim, group));
                        }
                    }
                    else
                    {
                        foreach (var ev in EventBoxes)
                        {
                            ev.MyEvent.Group = null;
                        }
                    }
                }
            }

            if (threadLock)
            {
                lock (_lock_EventBoxManagement)
                {
                    doWholeThing();
                }
            }
            else
            {
                doWholeThing();
            }
        }

        public List<TaeEventGroupRegion> GroupRegions = new List<TaeEventGroupRegion>();

        /// <summary>
        /// Temporarily disabled for 3.0
        /// </summary>
        public bool IsFancyCollapsingRemoGroupMode => false;// ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO;

        /// <summary>
        /// Temporarily enabled for 3.0 lol
        /// </summary>
        public bool IsSimpleRemoGroupMode => ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO; //false;

        public bool EventGroupDebugDrawEnable = false;

        public TaeEventGroupRegion GetGroupRegionOfRow(int row)
        {
            if (ViewportInteractor.EntityType != TaeViewportInteractor.TaeEntityType.REMO)
                return null;

            foreach (var gr in GroupRegions)
            {
                if (row >= gr.StartRow + 1 && row < gr.EndRow)
                    return gr;
            }
            return null;
        }

        private void ShiftAllGroupRegionsDownPastRow(int row, int shiftAmount)
        {
            var regionsToShift = GroupRegions.Where(gr => gr.StartRow >= row).ToList();
            foreach (var gr in regionsToShift)
            {
                gr.ShiftRow(shiftAmount);
            }
        }

        private void FixAllGroupBounds(bool ignoreUndoRedo)
        {
            if (!(IsFancyCollapsingRemoGroupMode))
                return;

            MainScreen.UndoMan.NewActionOptional(enableUndoOnAction: !ignoreUndoRedo,
                    doAction: () =>
                    {
                        int currentLogicalRow = 0;
                        foreach (var gr in GroupRegions)
                        {
                            gr.CropRegionToEvents();
                            if (gr.StartRow < 0)
                            {
                                gr.ShiftRow(0 - gr.StartRow);
                            }
                            if (gr.StartRow != currentLogicalRow)
                            {
                                gr.ShiftRow(currentLogicalRow - gr.StartRow);
                            }
                            currentLogicalRow += gr.RowCount;
                        }
                    },
                    undoAction: null,
                    new List<ITaeClonable>
                    {

                    });
        }

        //private void DrawAllEventGroupRegions(float opacity, GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
        //    SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Matrix scrollMatrix)
        //{
        //    var oldViewport = gd.Viewport;
        //    gd.Viewport = new Viewport(new Rectangle(Rect.X, (int)Math.Round(Rect.Y + TimeLineHeight), Rect.Width, (int)Math.Round(Rect.Height - TimeLineHeight)).DpiScaled());
        //    {
        //        sb.Begin(transformMatrix: Main.DPIMatrix);
        //        try
        //        {
        //            // full bg
        //            sb.Draw(texture: boxTex,
        //                position: new Vector2(0, 0),
        //                sourceRectangle: null,
        //                color: Main.Colors.GuiColorEventGraphBackground,
        //                rotation: 0,
        //                origin: Vector2.Zero,
        //                scale: new Vector2(EventGroupAreaWidth, (Rect.Height - TimeLineHeight) + 2),
        //                effects: SpriteEffects.None,
        //                layerDepth: 0
        //                );

        //            try
        //            {
        //                foreach (var gr in GroupRegions)
        //                {
        //                    sb.Draw(texture: boxTex,
        //                        position: new Vector2(0, (gr.StartRow * RowHeight) - ScrollViewer.Scroll.Y - 1),
        //                        sourceRectangle: null,
        //                        color: Color.Black,
        //                        rotation: 0,
        //                        origin: Vector2.Zero,
        //                        scale: new Vector2(Rect.Width + 4, 2),
        //                        effects: SpriteEffects.None,
        //                        layerDepth: 0
        //                        );

        //                    sb.DrawString(smallFont, gr.GetGroupText(), new Vector2(4, (gr.StartRow * RowHeight) + 4 - ScrollViewer.Scroll.Y), Color.White);
        //                }
        //            }
        //            catch
        //            {

        //            }
        //        }
        //        finally { sb.End(); }
        //    }
        //    gd.Viewport = oldViewport;

            
        //}

        public enum BoxDragType
        {
            None,
            LeftOfEventBox,
            RightOfEventBox,
            MiddleOfEventBox,
            MultiSelectionRectangle,
            MultiSelectionRectangleADD,
            MultiSelectionRectangleSUBTRACT,
            SliceTool,
            MultiDragLeftOfEventBox,
            MultiDragRightOfEventBox,
            MultiDragMiddleOfEventBox,
        }

        public class TaeDragState
        {
            public TaeUndoRestorableGraphState UndoRestoreState;
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
                if (StartMouseRow < 0)
                {
                    throw new InvalidOperationException($"Event graph drag ShiftBoxRow - drag start mouse row was an invalid row ({StartMouseRow}).");
                }

                if (newMouseRow < 0)
                {
                    throw new InvalidOperationException($"Event graph drag ShiftBoxRow - drag current mouse row was an invalid row ({newMouseRow}).");
                }

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

        public bool IsActive => MainScreen.Graph == this || MainScreen.Graph?.GhostEventGraph == this;

        public bool IsGhostEventGraph { get; private set; } = false;

        public TaeHoverInfoBox HoverInfoBox = new TaeHoverInfoBox();
        public Vector2 HoverInfoBoxOffsetFromMouse = Vector2.One * 16;

        public int TinyTextBoxWidth = 96;

        // Setting to 1 to remove smoothing, but lower values will add smoothing
        // later if I want it.
        public float AutoScrollLerpDistMult = 1;

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

        //public Color PlaybackCursorHitWindowStartColor = Color.DodgerBlue;
        //public Color PlaybackCursorHitWindowColor = Color.CornflowerBlue * 0.5f;

        public Color PlaybackCursorColor => Main.Colors.GuiColorEventGraphPlaybackCursor;

        public float PlaybackCursorThickness = 2;

        public int MultiSelectRectOutlineThickness => 1;
        public Color MultiSelectRectFillColor => Main.Colors.GuiColorEventGraphSelectionRectangleFill;
        public Color MultiSelectRectOutlineColor => Main.Colors.GuiColorEventGraphSelectionRectangleOutline;

        public readonly TaeEditorScreen MainScreen;
        public TAE.Animation AnimRef { get; private set; }

        public Rectangle Rect;
        public Rectangle ScrollViewerRect => new Rectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height);

        public List<TaeEditAnimEventBox> EventBoxes = new List<TaeEditAnimEventBox>();
        //public List<TaeEditAnimEventBox> EventBoxesToSimulate =>
        //    (GhostEventGraph != null && MainScreen.Config.SimulateReferencedEvents) ? (EventBoxes.Concat(GhostEventGraph.EventBoxes).ToList()) : EventBoxes;

        public List<TaeEditAnimEventBox> EventBoxesToSimulate =>
            GhostEventGraph != null ? GhostEventGraph.EventBoxes : EventBoxes;

        public TaeEditAnimEventGraph GhostEventGraph = null;

        public TaeScrollViewer ScrollViewer { get; private set; } = new TaeScrollViewer();

        const float SecondsPixelSizeDefault = 128 * 2;
        public float SecondsPixelSize = SecondsPixelSizeDefault;
        public float FramePixelSize_HKX => SecondsPixelSize / (1 / (float)PlaybackCursor.CurrentSnapInterval);
        public float MinPixelsBetweenFramesForHelperLines = 4;
        public float MinPixelsBetweenFramesForFrameNumberText = 16;

        public float SecondsPixelSizeFarAwayModeUpperBound = SecondsPixelSizeDefault;
        public float SecondsPixelSizeMax = (SecondsPixelSizeDefault * 256);
        public float SecondsPixelSizeScrollNotch = 128;

        public const float TimeLineHeight = 24;

        public float BoxSideScrollMarginSize = 4;

        public TaeDragState currentDrag = new TaeDragState();
        public List<TaeDragState> currentMultiDrag = new List<TaeDragState>();

        public bool IsBoxDragging => 
            currentDrag.DragType != BoxDragType.None &&
            currentDrag.DragType != BoxDragType.MultiSelectionRectangle &&
            currentDrag.DragType != BoxDragType.MultiSelectionRectangleADD &&
            currentDrag.DragType != BoxDragType.MultiSelectionRectangleSUBTRACT &&
            currentDrag.DragType != BoxDragType.SliceTool;


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

            float newAbsoluteOffset = (mousePointTime * SecondsPixelSize);
            float scrollAmountToCorrectOffset = (newAbsoluteOffset - (mouseScreenPosX + ScrollViewer.Scroll.X));
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

            float newAbsoluteOffset = (mousePointTime * SecondsPixelSize);
            float scrollAmountToCorrectOffset = (newAbsoluteOffset - (mouseScreenPosX + ScrollViewer.Scroll.X));
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

            float newAbsoluteOffset = (mousePointTime * SecondsPixelSize);
            float scrollAmountToCorrectOffset = (newAbsoluteOffset - (mouseScreenPosX + ScrollViewer.Scroll.X));
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

        public bool IsNewGraphVisiMode => true;// MainScreen.Config.IsNewGraphVisiMode;

        public float RowHeight => (IsNewGraphVisiMode ? 20 : 32) 
            * (IsSimpleRemoGroupMode ? 1.5f : 1);

        public object _lock_EventBoxManagement = new object();
        public Dictionary<int, List<TaeEditAnimEventBox>> sortedByRow = new Dictionary<int, List<TaeEditAnimEventBox>>();

        private object _lock_VisualRowToLogicalRowMap = new object();
        private List<int> VisualRowToLogicalRowMap = new List<int>();
        private List<int> LogicalRowToVisualRowMap = new List<int>();

        private Vector2 relMouse => new Vector2(
            MainScreen.Input.MousePosition.X - ScrollViewerRect.X + ScrollViewer.Scroll.X, 
            MainScreen.Input.MousePosition.Y - ScrollViewerRect.Y + ScrollViewer.Scroll.Y - TimeLineHeight);

        private int MouseVisualRow = 0;
        private int MouseLogicalRow = 0;

        private List<TaeEditAnimEventBox> GetBoxesInLogicalRow(int row)
        {
            var result = new List<TaeEditAnimEventBox>();
            lock (_lock_EventBoxManagement)
            {
                if (sortedByRow.ContainsKey(row))
                    result = sortedByRow[row];
                else 
                    result = new List<TaeEditAnimEventBox>();
            }
            return result;
        }

        public void RegisterEventBoxExistance(TaeEditAnimEventBox box)
        {
            lock (_lock_EventBoxManagement)
            {
                if (!EventBoxes.Contains(box))
                    EventBoxes.Add(box);

                if (!sortedByRow.ContainsKey(box.Row))
                    sortedByRow.Add(box.Row, new List<TaeEditAnimEventBox>());

                if (!sortedByRow[box.Row].Contains(box))
                    sortedByRow[box.Row].Add(box);
            }
        }

        public void ChangeToNewAnimRef(TAE.Animation newAnimRef)
        {
            lock (_lock_EventBoxManagement)
            {
                foreach (var box in EventBoxes)
                    box.RowChanged -= Box_RowChanged;

                EventBoxes.Clear();

                GroupRegions.Clear();

                GhostEventGraph = null;

                sortedByRow.Clear();

                AnimRef = newAnimRef;

                if (AnimRef == null)
                    return;

                void RegisterBoxToRow(TaeEditAnimEventBox box, int row)
                {
                    if (sortedByRow.ContainsKey(row))
                        sortedByRow[row].Add(box);
                    else
                        sortedByRow.Add(row, new List<TaeEditAnimEventBox> { box });
                }

                bool legacyRowMode = AnimRef.EventGroups == null || AnimRef.EventGroups.Count == 0 || IsSimpleRemoGroupMode;

                if (!GameRoot.GameTypeUsesBoilerplateEventGroups)
                {
                    legacyRowMode = !(AnimRef.EventGroups != null && AnimRef.EventGroups.Count > 0);
                }

                if (IsSimpleRemoGroupMode)
                {
                    legacyRowMode = true;
                }

                int currentRow = 0;
                float farthestRightOnCurrentRow = 0;
                int eventIndex = 0;

                var orderedEvents = AnimRef.Events;

                if ((IsFancyCollapsingRemoGroupMode || IsSimpleRemoGroupMode) && (AnimRef.EventGroups != null && AnimRef.EventGroups.Count > 0))
                {
                    GroupRegions.Clear();
                    orderedEvents = newAnimRef.Events.OrderBy(x => newAnimRef.EventGroups.IndexOf(x.Group)).ToList();
                }



                foreach (var ev in orderedEvents)
                {
                    int groupIndex = legacyRowMode ? -1 : AnimRef.EventGroups.IndexOf(ev.Group);

                    var newBox = new TaeEditAnimEventBox(this, ev, AnimRef);

                    if (newBox.Row >= 0)
                    {
                        currentRow = newBox.Row;
                        farthestRightOnCurrentRow = newBox.Right;
                    }
                    else
                    {
                        if (legacyRowMode)
                        {
                            newBox.Row = currentRow;

                            if (newBox.Left < farthestRightOnCurrentRow)
                            {
                                newBox.Row++;
                                currentRow++;
                                farthestRightOnCurrentRow = newBox.Right;
                            }
                            else
                            {
                                if (newBox.Right > farthestRightOnCurrentRow)
                                    farthestRightOnCurrentRow = newBox.Right;
                            }
                        }
                        else
                        {
                            if (groupIndex >= 0)
                                newBox.Row = currentRow = groupIndex;
                            else
                            {
                                if (currentRow < AnimRef.EventGroups.Count)
                                    currentRow = AnimRef.EventGroups.Count;
                                newBox.Row = currentRow;
                            }
                        }

                    }



                    newBox.RowChanged += Box_RowChanged;

                    RegisterBoxToRow(newBox, newBox.Row);
                    EventBoxes.Add(newBox);

                    eventIndex++;
                }

                if (AnimRef.EventGroups != null && AnimRef.EventGroups.Count > 0 && (IsFancyCollapsingRemoGroupMode || IsSimpleRemoGroupMode))
                {
                    GroupRegions.Clear();
                    foreach (var eg in newAnimRef.EventGroups)
                    {
                        var gr = new TaeEventGroupRegion(this, newAnimRef, eg);
                        GroupRegions.Add(gr);
                        var boxesInGroupRegion = EventBoxes.Where(evBox => evBox.MyEvent.Group == eg).ToList();
                        foreach (var b in boxesInGroupRegion)
                            gr.AddEvent(b);
                    }

                    foreach (var gr in GroupRegions)
                        gr.CropRegionToEvents();
                }
                else
                {
                    GroupRegions.Clear();
                }


                FixAllGroupBounds(ignoreUndoRedo: true);

                if (IsSimpleRemoGroupMode)
                {
                    GenerateFakeDS3EventGroups(threadLock: false);
                }

                if (!IsFancyCollapsingRemoGroupMode)
                {
                    foreach (var ev in EventBoxes)
                    {
                        ev.VisualRow = ev.Row;
                    }
                }

                

                //Console.WriteLine("EventGroups Start");
                //for (int i = 0; i < AnimRef.EventGroups.Count; i++)
                //{

                //    Console.WriteLine(AnimRef.EventGroups[i].EventType);

                //}
                //Console.WriteLine("EventGroups End");

                //GC.Collect();
            }

            InitGhostEventBoxes();
        }

        public void InitGhostEventBoxes()
        {
            MainScreen.GameWindowAsForm.Invoke(new Action(() =>
            {
                if (!IsGhostEventGraph)
                {
                    TAE.Animation getRecursiveAnimRef(TAE.Animation anim, bool isTopLevel)
                    {
                        if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim && asImportOtherAnim.ImportFromAnimID != -1)
                        {
                            var animRef = MainScreen.FileContainer.GetAnimRef(asImportOtherAnim.ImportFromAnimID);

                            if (animRef != null && animRef != anim)
                                return getRecursiveAnimRef(animRef, false);
                        }

                        return isTopLevel ? null : anim;
                    }

                    var recAnimRef = getRecursiveAnimRef(AnimRef, true);

                    if (recAnimRef != null)
                    {
                        GhostEventGraph = new TaeEditAnimEventGraph(MainScreen, true, recAnimRef);
                        GhostEventGraph.PlaybackCursor = PlaybackCursor;
                    }
                    else
                    {
                        GhostEventGraph = null;
                    }
                   

                }
                else
                {
                    GhostEventGraph = null;
                }
            }));

            
           
        }

        public TaeEditAnimEventGraph(TaeEditorScreen mainScreen, bool isGhostGraph, TAE.Animation startingAnimRef)
        {
            TaeClipboardContents.ParentGraph = this;

            MainScreen = mainScreen;

            IsGhostEventGraph = isGhostGraph;

            if (!isGhostGraph)
            {
                PlaybackCursor = new TaePlaybackCursor();

                ViewportInteractor = new TaeViewportInteractor(this);
                if (Main.REQUEST_REINIT_EDITOR)
                    return;
            }

            

            ChangeToNewAnimRef(startingAnimRef);

            

           
        }

        public void Box_RowChanged(object sender, int oldRow)
        {
            var box = (TaeEditAnimEventBox)sender;

            lock (_lock_EventBoxManagement)
            {
                if (sortedByRow.ContainsKey(oldRow) && sortedByRow[oldRow].Contains(box))
                    sortedByRow[oldRow].Remove(box);
                if (!sortedByRow.ContainsKey(box.Row))
                    sortedByRow.Add(box.Row, new List<TaeEditAnimEventBox>());
                if (!sortedByRow[box.Row].Contains(box))
                    sortedByRow[box.Row].Add(box);
            }

            if (IsFancyCollapsingRemoGroupMode)
                RegisterBoxGroupChange(box, ignoreUndoRedo: true);
        }

        private void RegisterBoxGroupChange(TaeEditAnimEventBox evBox, bool ignoreUndoRedo = false)
        {
            MainScreen.UndoMan.NewActionOptional(enableUndoOnAction: !ignoreUndoRedo,
                doAction: () =>
                {
                    if (IsSimpleRemoGroupMode)
                    {
                        if (evBox.CurrentGroupRegion == null || evBox.CurrentGroupRegion.Group != evBox.MyEvent.Group)
                        {
                            evBox.CurrentGroupRegion = new TaeEventGroupRegion(this, MainScreen.SelectedTaeAnim, evBox.MyEvent.Group);
                        }

                        if (!GroupRegions.Contains(evBox.CurrentGroupRegion))
                            GroupRegions.Add(evBox.CurrentGroupRegion);
                    }
                    else
                    {
                        evBox.CurrentGroupRegion?.RemoveEvent(evBox);
                        var gr = GetGroupRegionOfRow(evBox.Row);
                        gr?.AddEvent(evBox);
                        //FixAllGroupBounds(ignoreUndoRedo: true);
                    }


                },
                undoAction: () =>
                {

                },
                new List<ITaeClonable>
                {

                });
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

            MainScreen.UndoMan.NewActionOptional(enableUndoOnAction: !notUndoable,
                doAction: () =>
                {
                    foreach (var box in copyOfBoxes)
                    {
                        lock (_lock_EventBoxManagement)
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
                    }

                    GenerateFakeDS3EventGroups(threadLock: true);
                },
                undoAction: () =>
                {
                    MainScreen.MultiSelectedEventBoxes.Clear();
                    foreach (var box in copyOfBoxes)
                    {
                        lock (_lock_EventBoxManagement)
                        {
                            EventBoxes.Add(box);
                            AnimRef.Events.Add(box.MyEvent);
                        }

                        

                        var r = box.Row;

                        lock (_lock_EventBoxManagement)
                        {
                            if (!sortedByRow.ContainsKey(r))
                                sortedByRow.Add(r, new List<TaeEditAnimEventBox>());

                            if (!sortedByRow[r].Contains(box))
                                sortedByRow[r].Add(box);
                        }

                        box.RowChanged += Box_RowChanged;

                        if (!MainScreen.MultiSelectedEventBoxes.Contains(box))
                            MainScreen.MultiSelectedEventBoxes.Add(box);

                        RegisterBoxGroupChange(box, ignoreUndoRedo: true);

                        FixAllGroupBounds(ignoreUndoRedo: true);

                        AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
                    }

                    GenerateFakeDS3EventGroups(threadLock: true);
                },
                captureItems: new List<ITaeClonable>()
                {

                });
        }

        public void DeleteEventBox(TaeEditAnimEventBox box, bool notUndoable)
        {
            if (box.OwnerPane != this)
            {
                throw new ArgumentException($"This {nameof(TaeEditAnimEventGraph)} can only " +
                    $"delete {nameof(TaeEditAnimEventBox)}'s that it owns!", nameof(box));
            }

            MainScreen.UndoMan.NewActionOptional(enableUndoOnAction: !notUndoable,
                doAction: () =>
                {
                    if (MainScreen.SelectedEventBox == box)
                        MainScreen.SelectedEventBox = null;
                    box.RowChanged -= Box_RowChanged;

                    foreach (var kvp in sortedByRow)
                    {
                        if (kvp.Value.Contains(box))
                            kvp.Value.Remove(box);
                    }

                    lock (_lock_EventBoxManagement)
                    {
                        AnimRef.Events.Remove(box.MyEvent);

                        EventBoxes.Remove(box);
                    }

                    AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);

                    GenerateFakeDS3EventGroups(threadLock: true);
                },
                undoAction: () =>
                {
                    lock (_lock_EventBoxManagement)
                    {
                        EventBoxes.Add(box);
                        AnimRef.Events.Add(box.MyEvent);

                        if (!sortedByRow.ContainsKey(box.Row))
                            sortedByRow.Add(box.Row, new List<TaeEditAnimEventBox>());

                        if (!sortedByRow[box.Row].Contains(box))
                            sortedByRow[box.Row].Add(box);
                    }
                    box.RowChanged += Box_RowChanged;

                    MainScreen.SelectedEventBox = box;

                    AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
                }, 
                captureItems: new List<ITaeClonable>
                {

                });
        }

        private TaeEditAnimEventBox PlaceNewEvent(TAE.Event ev, int row, TAE.EventGroup grp, bool notUndoable, bool isBigEndianEventSource)
        {
            //ev.Index = MainScreen.SelectedTaeAnim.EventList.Count;

            if (MainScreen.SelectedTae?.BankTemplate != null && ev.Template == null && MainScreen.SelectedTae.BankTemplate.ContainsKey(ev.Type))
            {
                ev.ApplyTemplate(isBigEndianEventSource, MainScreen.SelectedTae.BankTemplate[ev.Type]);
            }

            var newBox = new TaeEditAnimEventBox(this, ev, AnimRef);

            newBox.MyEvent.Group = new TAE.EventGroup(0);

            if (grp != null)
            {
                newBox.MyEvent.Group.GroupType = grp.GroupType;
                newBox.MyEvent.Group.GroupData = grp.GroupData;
            }

            newBox.Row = row;

            void DoPlaceEvent()
            {
                lock (_lock_EventBoxManagement)
                {
                    MainScreen.SelectedTaeAnim.Events.Add(ev);
                }

                lock (_lock_EventBoxManagement)
                {
                    if (!sortedByRow.ContainsKey(newBox.Row))
                        sortedByRow.Add(newBox.Row, new List<TaeEditAnimEventBox>());

                    if (!sortedByRow[newBox.Row].Contains(newBox))
                        sortedByRow[newBox.Row].Add(newBox);
                }

                newBox.RowChanged += Box_RowChanged;

                lock (_lock_EventBoxManagement)
                {
                    EventBoxes.Add(newBox);
                }

                RegisterBoxGroupChange(newBox);

                AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
            }

            if (notUndoable)
            {
                DoPlaceEvent();
                return newBox;
            }

            MainScreen.UndoMan.NewActionOptional(enableUndoOnAction: !notUndoable,
                doAction: () =>
                {
                    DoPlaceEvent();
                },
                undoAction: () =>
                {
                    lock (_lock_EventBoxManagement)
                    {
                        EventBoxes.Remove(newBox);

                        newBox.RowChanged -= Box_RowChanged;

                 
                        if (sortedByRow.ContainsKey(newBox.Row))
                            if (sortedByRow[newBox.Row].Contains(newBox))
                                sortedByRow[newBox.Row].Remove(newBox);
                    


                        MainScreen.SelectedTaeAnim.Events.Remove(ev);

                        AnimRef.SetIsModified(!MainScreen.IsReadOnlyFileMode);
                    }
                },
                captureItems: new List<ITaeClonable>
                {

                });

            

            return newBox;
        }

        private void PlaceNewEventAtMouse()
        {
            if (MouseLogicalRow < 0)
                return;

            float mouseTime = ((MainScreen.Input.MousePosition.X - ScrollViewerRect.X + ScrollViewer.Scroll.X) / SecondsPixelSize);

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

            PlaceNewEvent(newEvent, MouseLogicalRow, MainScreen.SelectedTaeAnim.EventGroups.LastOrDefault(), notUndoable: false, MainScreen.SelectedTae.BigEndian);
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

            Newtonsoft.Json.JsonSerializerSettings jsonSerializeCfg = new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            var testSerialize = Newtonsoft.Json.JsonConvert.SerializeObject(
                clipboardContents, jsonSerializeCfg);

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

                    Newtonsoft.Json.JsonSerializerSettings jsonSerializeCfg = new Newtonsoft.Json.JsonSerializerSettings
                    {
                        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All,
                        Formatting = Newtonsoft.Json.Formatting.Indented
                    };

                    TaeClipboardContents clipboardContents = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<TaeClipboardContents>(jsonText, jsonSerializeCfg);

                    var events = clipboardContents.GetEvents().ToList();

                    var copyOfEvents = new List<TaeEditAnimEventBox>();
                    var copyOfSelectedEventBoxes = new List<TaeEditAnimEventBox>();
                    var copyOfSelectedEventBox = MainScreen.SelectedEventBox;

                    var copyOfRelMouse = relMouse;
                    var copyOfMouseRow = MouseLogicalRow;

                    var copyOfClipboardStartTime = clipboardContents.StartTime;

                    var copyOfEventStartTimes = new List<float>();
                    var copyOfEventEndTimes = new List<float>();
                    var copyOfEventRows = new List<int>();
                    var copyOfEventGroups = new List<TAE.EventGroup>();

                    foreach (var ev in events)
                    {
                        copyOfEvents.Add(ev);
                        copyOfEventStartTimes.Add(ev.MyEvent.StartTime);
                        copyOfEventEndTimes.Add(ev.MyEvent.EndTime);
                        copyOfEventRows.Add(ev.Row);
                        copyOfEventGroups.Add(ev.MyEvent.Group);
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
                                    float start = copyOfEventStartTimes[i] - copyOfClipboardStartTime + TaeExtensionMethods.RoundTimeToCurrentSnapInterval(copyOfRelMouse.X / SecondsPixelSize);
                                    float end = start + (copyOfEventEndTimes[i] - copyOfEventStartTimes[i]);

                                    startTime = start;
                                    endTime = end;

                                    row -= clipboardContents.StartRow;
                                    row += copyOfMouseRow;
                                }

                                ev.StartTime = startTime;
                                ev.EndTime = endTime;

                                var box = PlaceNewEvent(ev, row, copyOfEventGroups[i], notUndoable: true, clipboardContents.IsBigEndian);

                                MainScreen.MultiSelectedEventBoxes.Add(box);
                            }

                            if (MainScreen.MultiSelectedEventBoxes.Count == 1)
                            {
                                MainScreen.SelectedEventBox = MainScreen.MultiSelectedEventBoxes[0];
                                MainScreen.MultiSelectedEventBoxes.Clear();
                            }

                            GenerateFakeDS3EventGroups(threadLock: true);
                        },
                        undoAction: () =>
                        {
                            for (int i = 0; i < copyOfEvents.Count; i++)
                            {
                                var matchedBox = EventBoxes.FirstOrDefault(eb => eb.MyEvent == copyOfEvents[i].MyEvent);
                                if (matchedBox != null)
                                    DeleteEventBox(matchedBox, notUndoable: true);
                            }

                            GenerateFakeDS3EventGroups(threadLock: true);

                            //DeleteMultipleEventBoxes(copyOfEvents, notUndoable: true);

                            //MainScreen.SelectedEventBox = copyOfSelectedEventBox;
                            //MainScreen.MultiSelectedEventBoxes = copyOfSelectedEventBoxes;
                        },
                        captureItems: new List<ITaeClonable>
                        {

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
            if (GhostEventGraph != null)
            {
                lock (GhostEventGraph._lock_EventBoxManagement)
                {
                    PlaybackCursor.Update(GhostEventGraph.EventBoxes);
                }
            }
            else
            {
                lock (_lock_EventBoxManagement)
                {
                    PlaybackCursor.Update(EventBoxes);
                }
            }
            
        }

        public void MouseReleaseStuff()
        {
            currentUnselectedMouseDragType = UnselectedMouseDragType.None;
            PlaybackCursor.Scrubbing = false;
        }

        public void UpdateMiddleClickPan()
        {
            if (MainScreen.Input.MiddleClickHeld && ScrollViewerRect.Contains(MainScreen.Input.MiddleClickDownAnchor))
            {
                //MainScreen.Input.CursorType = MouseCursorType.GrabPan;
                ScrollViewer.Scroll -= MainScreen.Input.MousePositionDelta;
                ScrollViewer.ClampScroll();
            }
        }

        public int GetLogicalRowFromVisualRow(int visualRow)
        {
            if (!IsFancyCollapsingRemoGroupMode)
                return visualRow;

            int result = 0;
            lock (_lock_VisualRowToLogicalRowMap)
            {
                if (VisualRowToLogicalRowMap.Count == 0)
                {
                    return 0;
                }
                else if (visualRow >= VisualRowToLogicalRowMap.Count)
                {
                    result = VisualRowToLogicalRowMap.LastOrDefault(x => x >= 0) + 1;
                }
                else if (visualRow < 0)
                {
                    result = 0;
                }
                else
                {
                    result = VisualRowToLogicalRowMap[visualRow];
                }
            }
            return result;
        }

        public int GetVisualRowFromLogicalRow(int logicalRow)
        {
            if (!IsFancyCollapsingRemoGroupMode)
                return logicalRow;

            int result = 0;
            lock (_lock_VisualRowToLogicalRowMap)
            {
                if (LogicalRowToVisualRowMap.Count == 0)
                {
                    return 0;
                }
                else if (logicalRow >= LogicalRowToVisualRowMap.Count)
                {
                    result = LogicalRowToVisualRowMap.LastOrDefault(x => x >= 0) + 1;
                }
                else if (logicalRow < 0)
                {
                    result = 0;
                }
                else
                {
                    result = LogicalRowToVisualRowMap[logicalRow];
                }
            }
            return result;
        }

        public void Update(bool allowMouseUpdate)
        {
            RefreshLogicalAndVisualRowMappings();



            if (MainScreen.Input.MiddleClickHeld && ScrollViewerRect.Contains(MainScreen.Input.MiddleClickDownAnchor))
            {
                MainScreen.HoveringOverEventBox = null;
                MainScreen.PrevHoveringOverEventBox = null;
                HoverInfoBox.Update(mouseInside: false, 0);
                return;
            }

            //else if (MainScreen.Input.MiddleClickUp)
            //{
            //    MainScreen.Input.CursorType = MouseCursorType.Arrow;
            //}

            if (!MainScreen.Input.LeftClickHeld)
            {
                MouseReleaseStuff();
            }

            ScrollViewer.UpdateInput(MainScreen.Input, Main.DELTA_UPDATE, allowScrollWheel: !MainScreen.Input.CtrlHeld);

            if (MainScreen.Input.CtrlHeld)
            {
                Zoom(MainScreen.Input.ScrollDelta, MainScreen.Input.MousePosition.X - ScrollViewerRect.X);
            }

            if (currentUnselectedMouseDragType == UnselectedMouseDragType.PlaybackCursorScrub)
            {
                PlaybackCursor.Scrubbing = true;

                ScrollViewer.ClampScroll();

                float desiredTime = Math.Max(((MainScreen.Input.MousePosition.X - ScrollViewerRect.X) + ScrollViewer.Scroll.X) / SecondsPixelSize, 0);

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

                MouseLogicalRow = -1;

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

            

            MouseVisualRow = (int)(relMouse.Y / RowHeight);
            MouseLogicalRow = GetLogicalRowFromVisualRow(MouseVisualRow);

            // Fix for stuff being highlighted while mouse is over timeline
            if ((MainScreen.Input.MousePosition.Y - ScrollViewerRect.Y) < TimeLineHeight)
            {
                MouseLogicalRow = -1;
            }

            if ((ScrollViewer.Viewport.Contains(new Point((int)MainScreen.Input.MousePosition.X, (int)MainScreen.Input.MousePosition.Y))
                || MainScreen.Input.LeftClickHeld))
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
                    else if (MainScreen.Input.MousePosition.Y > ScrollViewerRect.Bottom - ScrollViewer.ScrollBarThickness && !ScrollViewer.DisableHorizontalScroll)
                    {
                        if (MainScreen.Input.LeftClickDown)
                            currentUnselectedMouseDragType = UnselectedMouseDragType.HorizontalScroll;

                        // Return so our initial click down doesn't select events.
                        return;
                    }
                    else if (MainScreen.Input.MousePosition.X > ScrollViewerRect.Right - ScrollViewer.ScrollBarThickness && !ScrollViewer.DisableVerticalScroll)
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

                if (GhostEventGraph != null)
                {
                    MainScreen.SelectedEventBox = null;
                    MainScreen.MultiSelectedEventBoxes.Clear();
                }

                if (MainScreen.Input.LeftClickDown && !MainScreen.Input.ShiftHeld && !MainScreen.Input.CtrlHeld)
                {
                    MainScreen.SelectedEventBox = null;
                }



                if (MainScreen.Input.RightClickDown && currentDrag.DragType == BoxDragType.None)
                {
                    if (MainScreen.HoveringOverEventBox != null && !MainScreen.Input.ShiftHeld)
                    {
                        ViewportInteractor.EventSim.PlayOneShotSoundOfBox(MainScreen.HoveringOverEventBox, true);
                        ViewportInteractor.EventSim.PlayRumbleCamOfBox(MainScreen.HoveringOverEventBox);
                    }
                    else if (GhostEventGraph == null && MainScreen.MultiSelectedEventBoxes.Count <= 1 && MainScreen.Input.ShiftHeld)
                    {
                        PlaceNewEventAtMouse();
                    }


                }


                if (MainScreen.Input.RightClickHeld && currentDrag.DragType == BoxDragType.None
                    && MainScreen.HoveringOverEventBox != null && !MainScreen.Input.ShiftHeld)
                {
                    ViewportInteractor.EventSim.DoMouseClickPreviewOfBoxSound(MainScreen.HoveringOverEventBox);
                }


                

                IEnumerable<TaeEditAnimEventBox> masterRowBoxList = GetBoxesInLogicalRow(MouseLogicalRow);
                var rowOrderedByTime = masterRowBoxList.OrderByDescending(x => x.MyEvent.StartTime);

                MainScreen.HoveringOverEventBox = null;

                if (GhostEventGraph != null)
                    return;

                if (MouseLogicalRow >= 0)
                {
                    foreach (var box in rowOrderedByTime)
                    {
                        if (box.CurrentGroupRegion?.Collapsed == true)
                            continue;

                        bool canManipulateBox = (MainScreen.MultiSelectedEventBoxes.Count == 0
                            || MainScreen.MultiSelectedEventBoxes.Contains(box));

                        if (currentDrag.DragType == BoxDragType.None)
                        {
                            if (canManipulateBox && box.Width >= 16 &&
                                relMouse.X <= box.Left + BoxSideScrollMarginSize &&
                                relMouse.X >= box.Left - BoxSideScrollMarginSize &&
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
                                        currentDrag.Offset = new Point((int)(relMouse.X - box.Left), (int)(relMouse.Y - box.Top));
                                        currentDrag.BoxOriginalWidth = box.Width;
                                        currentDrag.BoxOriginalStart = box.MyEvent.StartTime;
                                        currentDrag.BoxOriginalEnd = box.MyEvent.EndTime;
                                        currentDrag.BoxOriginalRow = box.Row;
                                        currentDrag.StartMouseRow = MouseLogicalRow;
                                        currentDrag.StartDragPoint = currentDrag.CurrentDragPoint = relMouse.ToPoint();
                                    }
                                    else
                                    {
                                        currentDrag.DragType = BoxDragType.MultiDragLeftOfEventBox;
                                        currentMultiDrag.Clear();
                                        foreach (var multiBox in MainScreen.MultiSelectedEventBoxes)
                                        {
                                            var newDrag = new TaeDragState();

                                            newDrag.UndoRestoreState = new TaeUndoRestorableGraphState(this, new List<ITaeClonable> { });
                                            newDrag.DragType = BoxDragType.LeftOfEventBox;
                                            newDrag.Box = multiBox;
                                            newDrag.Offset = new Point((int)(relMouse.X - multiBox.Left), (int)(relMouse.Y - multiBox.Top));
                                            newDrag.BoxOriginalWidth = multiBox.Width;
                                            newDrag.BoxOriginalStart = multiBox.MyEvent.StartTime;
                                            newDrag.BoxOriginalEnd = multiBox.MyEvent.EndTime;
                                            newDrag.BoxOriginalRow = multiBox.Row;
                                            newDrag.StartMouseRow = MouseLogicalRow;
                                            newDrag.StartDragPoint = newDrag.CurrentDragPoint = relMouse.ToPoint();

                                            currentMultiDrag.Add(newDrag);
                                        }
                                    }

                                }
                            }
                            else if (canManipulateBox &&
                                box.Width >= 16 &&
                                relMouse.X >= box.Right - BoxSideScrollMarginSize &&
                                relMouse.X <= box.Right + BoxSideScrollMarginSize &&
                                (!(PlaybackCursor.IsPlaying && MainScreen.Config.AutoScrollDuringAnimPlayback)))
                            {
                                MainScreen.Input.CursorType = MouseCursorType.DragX;
                                if (MainScreen.Input.LeftClickDown && currentUnselectedMouseDragType == UnselectedMouseDragType.EventSelect)
                                {
                                    if (MainScreen.MultiSelectedEventBoxes.Count == 0)
                                    {
                                        currentMultiDrag.Clear();

                                        currentDrag.UndoRestoreState = new TaeUndoRestorableGraphState(this, new List<ITaeClonable> { });
                                        currentDrag.DragType = BoxDragType.RightOfEventBox;
                                        currentDrag.Box = box;
                                        currentDrag.Offset = new Point(
                                            (int)(relMouse.X - box.Left), (int)(relMouse.Y - box.Top));
                                        currentDrag.BoxOriginalWidth = box.Width;
                                        currentDrag.BoxOriginalStart = box.MyEvent.StartTime;
                                        currentDrag.BoxOriginalEnd = box.MyEvent.EndTime;
                                        currentDrag.BoxOriginalRow = box.Row;
                                        currentDrag.StartMouseRow = MouseLogicalRow;
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
                                                (int)(relMouse.X - multiBox.Left),
                                                (int)(relMouse.Y - multiBox.Top));
                                            newDrag.BoxOriginalWidth = multiBox.Width;
                                            newDrag.BoxOriginalStart = multiBox.MyEvent.StartTime;
                                            newDrag.BoxOriginalEnd = multiBox.MyEvent.EndTime;
                                            newDrag.BoxOriginalRow = multiBox.Row;
                                            newDrag.StartMouseRow = MouseLogicalRow;
                                            newDrag.StartDragPoint =
                                                newDrag.CurrentDragPoint = relMouse.ToPoint();

                                            currentMultiDrag.Add(newDrag);
                                        }
                                    }

                                }
                            }
                            else if (relMouse.X >= box.Left && relMouse.X < box.Right)
                            {
                                MainScreen.Input.CursorType = MouseCursorType.Arrow;

                                if (ScrollViewerRect.Contains(MainScreen.Input.MousePositionPoint))
                                {
                                    MainScreen.HoveringOverEventBox = box;
                                }

                                if (MainScreen.Input.LeftClickDown)
                                {
                                    if (MainScreen.MultiSelectedEventBoxes.Count == 0)
                                    {
                                        currentDrag.UndoRestoreState = new TaeUndoRestorableGraphState(this, new List<ITaeClonable> { });
                                        currentDrag.DragType = BoxDragType.MiddleOfEventBox;
                                        currentDrag.Box = box;
                                        currentDrag.Offset = new Point((int)(relMouse.X - box.Left), (int)(relMouse.Y - box.Top));
                                        currentDrag.BoxOriginalWidth = box.Width;
                                        currentDrag.BoxOriginalStart = box.MyEvent.StartTime;
                                        currentDrag.BoxOriginalEnd = box.MyEvent.EndTime;
                                        currentDrag.BoxOriginalRow = box.Row;
                                        currentDrag.StartMouseRow = MouseLogicalRow;
                                        currentDrag.StartDragPoint = currentDrag.CurrentDragPoint = relMouse.ToPoint();
                                    }
                                    else
                                    {
                                        if (MainScreen.MultiSelectedEventBoxes.Contains(box) && !MainScreen.Input.CtrlHeld)
                                        {
                                            currentDrag.DragType = BoxDragType.MultiDragMiddleOfEventBox;

                                            currentDrag.UndoRestoreState = new TaeUndoRestorableGraphState(this, new List<ITaeClonable> { });

                                            currentMultiDrag.Clear();
                                            foreach (var multiBox in MainScreen.MultiSelectedEventBoxes)
                                            {
                                                var newDrag = new TaeDragState();

                                                newDrag.DragType = BoxDragType.MiddleOfEventBox;
                                                newDrag.Box = multiBox;
                                                newDrag.Offset = new Point((int)(relMouse.X - multiBox.Left), (int)(relMouse.Y - multiBox.Top));
                                                newDrag.BoxOriginalWidth = multiBox.Width;
                                                newDrag.BoxOriginalStart = multiBox.MyEvent.StartTime;
                                                newDrag.BoxOriginalEnd = multiBox.MyEvent.EndTime;
                                                newDrag.BoxOriginalRow = multiBox.Row;
                                                newDrag.StartMouseRow = MouseLogicalRow;
                                                newDrag.StartDragPoint = newDrag.CurrentDragPoint = relMouse.ToPoint();

                                                currentMultiDrag.Add(newDrag);
                                            }
                                        }
                                        else
                                        {
                                            if (MainScreen.Input.ShiftHeld && !MainScreen.Input.CtrlHeld && !MainScreen.Input.AltHeld)
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
                                            else if (!MainScreen.Input.ShiftHeld && MainScreen.Input.CtrlHeld && !MainScreen.Input.AltHeld)
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
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var isSingleSelect = (currentDrag.DragType == BoxDragType.None) ||
                            (currentDrag.DragType == BoxDragType.MiddleOfEventBox && currentDrag.StartDragPoint == currentDrag.CurrentDragPoint);


                        if (isSingleSelect && MainScreen.Input.LeftClickDown && !(MainScreen.Input.MousePosition.Y < ScrollViewer.Viewport.Top + TimeLineHeight))
                        {
                            if (relMouse.X >= box.Left && relMouse.X < box.Right)
                            {
                                currentMultiDrag.Clear();

                                if (MainScreen.Input.ShiftHeld && !MainScreen.Input.CtrlHeld && !MainScreen.Input.AltHeld)
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
                                else if (!MainScreen.Input.ShiftHeld && MainScreen.Input.CtrlHeld && !MainScreen.Input.AltHeld)
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

                                break;
                            }

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

                    if (!MainScreen.Input.CtrlHeld && MainScreen.Input.AltHeld)
                        currentDrag.DragType = BoxDragType.SliceTool;
                    else if (MainScreen.Input.ShiftHeld && !MainScreen.Input.CtrlHeld && !MainScreen.Input.AltHeld)
                        currentDrag.DragType = BoxDragType.MultiSelectionRectangleADD;
                    else if (!MainScreen.Input.ShiftHeld && MainScreen.Input.CtrlHeld && !MainScreen.Input.AltHeld)
                        currentDrag.DragType = BoxDragType.MultiSelectionRectangleSUBTRACT;
                    else
                        currentDrag.DragType = BoxDragType.MultiSelectionRectangle;
                    currentDrag.StartMouseRow = MouseLogicalRow;

                    if (currentDrag.DragType == BoxDragType.SliceTool)
                        currentDrag.StartDragPoint = currentDrag.CurrentDragPoint = new Point(TaeTime.RoundPixelsToCurrentSnapIntervalI(relMouse.X, SecondsPixelSize), (int)relMouse.Y);
                    else
                        currentDrag.StartDragPoint = relMouse.ToPoint();
                }

                if (MainScreen.Input.LeftClickHeld)
                {
                    if (currentDrag.DragType == BoxDragType.SliceTool)
                    {
                        if (MainScreen.Input.ShiftHeld)
                            currentDrag.CurrentDragPoint = new Point(TaeTime.RoundPixelsToCurrentSnapIntervalI(relMouse.X, SecondsPixelSize), (int)relMouse.Y);
                    }
                    else
                    {
                        currentDrag.CurrentDragPoint = relMouse.ToPoint();
                    }
                    

                    if (!(PlaybackCursor.IsPlaying && MainScreen.Config.AutoScrollDuringAnimPlayback))
                    {
                        if (currentDrag.DragType == BoxDragType.LeftOfEventBox && currentDrag.Box != null)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            var isModified = currentDrag.DragBoxToMouseAndCheckIsModified(relMouse.ToPoint());
                            AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            //currentDrag.Box.DragLeftSide(MainScreen.Input.MousePositionDelta.X);
                        }
                        else if (currentDrag.DragType == BoxDragType.RightOfEventBox && currentDrag.Box != null)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            var isModified = currentDrag.DragBoxToMouseAndCheckIsModified(relMouse.ToPoint());
                            AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            //currentDrag.Box.DragRightSide(MainScreen.Input.MousePositionDelta.X);
                        }
                        else if (currentDrag.DragType == BoxDragType.MiddleOfEventBox && currentDrag.Box != null)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragXY;
                            var isModified = currentDrag.DragBoxToMouseAndCheckIsModified(relMouse.ToPoint());
                            AnimRef.SetIsModified(AnimRef.GetIsModified() || (!MainScreen.IsReadOnlyFileMode && isModified));
                            //currentDrag.Box.DragMiddle(MainScreen.Input.MousePositionDelta.X);
                            if (MouseLogicalRow >= 0)
                                currentDrag.ShiftBoxRow(MouseLogicalRow);
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

                                if (MouseLogicalRow >= 0)
                                    multiDrag.ShiftBoxRow(MathHelper.Max(MouseLogicalRow, minimumMouseRow));
                            }

                            MainScreen.Input.CursorType = MouseCursorType.DragXY;
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
                                lock (_lock_EventBoxManagement)
                                {
                                    if (sortedByRow.ContainsKey(i))
                                    {
                                        foreach (var box in sortedByRow[i])
                                        {
                                            var boxRect = new Rectangle((int)box.Left, (int)(box.Top - TimeLineHeight), (int)box.Width, (int)box.HeightFr);
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
                            }
                            MainScreen.SelectedEventBox = null;
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
                                lock (_lock_EventBoxManagement)
                                {
                                    if (sortedByRow.ContainsKey(i))
                                    {
                                        foreach (var box in sortedByRow[i])
                                        {
                                            var boxRect = new Rectangle((int)box.Left, (int)(box.Top - TimeLineHeight), (int)box.Width, (int)box.HeightFr);
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
                            }
                            MainScreen.SelectedEventBox = null;
                        }

                    }
                }
                else // Releasing click after a drag
                {
                    ReleaseCurrentDrag();
                }

                
            }

            previousUnselectedMouseDragType = currentUnselectedMouseDragType;
        }

        public void ReleaseCurrentDrag()
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

                        MainScreen.UndoMan.NewActionCustomPreState(customPreState: currentDrag.UndoRestoreState,
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

                                FixAllGroupBounds(ignoreUndoRedo: true);
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

                                FixAllGroupBounds(ignoreUndoRedo: true);
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

                        MainScreen.UndoMan.NewActionCustomPreState(customPreState: currentDrag.UndoRestoreState,
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
                                    FixAllGroupBounds(ignoreUndoRedo: true);
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

                                        FixAllGroupBounds(ignoreUndoRedo: true);
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
                    else if (currentDrag.DragType == BoxDragType.SliceTool)
                    {
                        int capStartMouseRow = currentDrag.StartMouseRow;
                        float capStartTime = currentDrag.CurrentDragPoint.X / SecondsPixelSize;
                        int capCurrentMouseRow = MouseVisualRow;
                        var capOriginalEventEnds = new Dictionary<TaeEditAnimEventBox, float>();
                        int capDragMinRow = Math.Min(capStartMouseRow, capCurrentMouseRow) + 1;
                        int capDragMaxRow = Math.Max(capStartMouseRow, capCurrentMouseRow) - 1;

                        if (capDragMaxRow - capDragMinRow >= 0)
                        {
                            foreach (var evb in EventBoxes)
                            {
                                if (evb.MyEvent.StartTime < capStartTime && evb.MyEvent.EndTime > capStartTime &&
                                evb.Row >= capDragMinRow && evb.Row <= capDragMaxRow)
                                {
                                    capOriginalEventEnds[evb] = evb.MyEvent.EndTime;
                                }
                            }

                            MainScreen.UndoMan.NewAction(doAction: () =>
                            {

                                List<TAE.Event> newEvents = new List<TAE.Event>();
                                List<TaeEditAnimEventBox> newEventBoxes = new List<TaeEditAnimEventBox>();
                                foreach (var evb in EventBoxes)
                                {
                                    if (evb.MyEvent.StartTime < capStartTime && evb.MyEvent.EndTime > capStartTime &&
                                    evb.Row >= capDragMinRow && evb.Row <= capDragMaxRow)
                                    {
                                        var dupeEvent = evb.MyEvent.GetClone(GameRoot.IsBigEndianGame);
                                        dupeEvent.StartTime = capStartTime;
                                        newEvents.Add(dupeEvent);
                                        var dupeBox = new TaeEditAnimEventBox(this, dupeEvent, MainScreen.SelectedTaeAnim);
                                        dupeBox.Row = evb.Row;
                                        dupeBox.VisualRow = evb.VisualRow;
                                        newEventBoxes.Add(dupeBox);
                                        evb.MyEvent.EndTime = capStartTime;
                                    }
                                }
                                MainScreen.SelectedTaeAnim.Events.AddRange(newEvents);
                                foreach (var evb in newEventBoxes)
                                {
                                    RegisterEventBoxExistance(evb);
                                }

                            },
                            undoAction: () =>
                            {
                                foreach (var e in capOriginalEventEnds)
                                    e.Key.MyEvent.EndTime = e.Value;
                            },
                            captureItems: EventBoxes.Cast<ITaeClonable>().ToList());
                        }

                        

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
                    else if (currentDrag.DragType == BoxDragType.SliceTool)
                    {
                        currentDrag.DragType = BoxDragType.None;
                    }
                }




            }
        }

        public void UpdateMouseOutsideRect(float elapsedSeconds, bool allowMouseUpdate)
        {
            MouseLogicalRow = -1;
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
            int w = 0;
            int h = 0;

            if (GhostEventGraph != null)
            {
                var boxes = GhostEventGraph.EventBoxes;
                if (boxes.Count > 0)
                {
                    w = (int)boxes.OrderByDescending(x => x.MyEvent.EndTime).First().Right + 256;
                    h = (int)boxes.OrderByDescending(x => x.Row).First().Bottom + 256;
                }
            }
            else
            {
                var boxes = EventBoxes.ToList();
                if (boxes.Count > 0)
                {
                    w = (int)boxes.OrderByDescending(x => x.MyEvent.EndTime).First().Right + 256;
                    h = (int)boxes.OrderByDescending(x => x.Row).First().Bottom + 256;
                }
                
            }

            if ((PlaybackCursor.MaxTime * SecondsPixelSize + 256) > w)
            {
                w = (int)(PlaybackCursor.MaxTime * SecondsPixelSize + 256);
            }

            return new Point(w, h);
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
                result.Add(i, ((i * RowHeight)) + TimeLineHeight);
            }

            return result;
        }

        private void DrawTimeLine(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, 
            SpriteFont font, Dictionary<int, float> secondVerticalLineXPositions)
        {
            foreach (var kvp in secondVerticalLineXPositions)
            {
                //sb.DrawString(font, kvp.Key.ToString(), new Vector2((float)Math.Round(kvp.Value + 4 + 1), (float)Math.Round(ScrollViewer.Scroll.Y + 3 + 1)) + Main.GlobalTaeEditorFontOffset, Color.Black);
                //sb.DrawString(font, kvp.Key.ToString(), new Vector2((float)Math.Round(kvp.Value + 4), (float)Math.Round(ScrollViewer.Scroll.Y + 3)) + Main.GlobalTaeEditorFontOffset, Color.White);

                ImGuiDebugDrawer.DrawText(kvp.Key.ToString(), new Vector2((float)Math.Round(kvp.Value + 4 - ScrollViewer.Scroll.X), 0), Color.White, null, 20);
            }
        }

        private void DrawFrameSnapLines(Texture2D boxTex, SpriteBatch sb, Color col)
        {
            double framePixelSize_TAE = 0;

            if (MainScreen.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                framePixelSize_TAE = (SecondsPixelSize / (1 / TaeExtensionMethods.TAE_FRAME_60));
            else if (MainScreen.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                framePixelSize_TAE = (SecondsPixelSize / (1 / TaeExtensionMethods.TAE_FRAME_30));

            bool zoomedEnoughForFrameLines_TAE = false;

            if (MainScreen.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                zoomedEnoughForFrameLines_TAE = SecondsPixelSize >= ((1 / TaeExtensionMethods.TAE_FRAME_60) * MinPixelsBetweenFramesForHelperLines);
            else if (MainScreen.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS30)
                zoomedEnoughForFrameLines_TAE = SecondsPixelSize >= ((1 / TaeExtensionMethods.TAE_FRAME_30) * MinPixelsBetweenFramesForHelperLines);


            if (zoomedEnoughForFrameLines_TAE)
            {
                int startFrame = (int)Math.Floor(ScrollViewer.Scroll.X / framePixelSize_TAE);
                int endFrame = (int)Math.Ceiling((ScrollViewer.Scroll.X + ScrollViewer.Viewport.Width) / framePixelSize_TAE);

                for (int i = startFrame; i <= endFrame; i++)
                {
                    //sb.Draw(texture: boxTex,
                    //position: new Vector2((float)(i * framePixelSize_TAE), ScrollViewer.Scroll.Y + TimeLineHeight),
                    //sourceRectangle: null,
                    //color: col,
                    //rotation: 0,
                    //origin: Vector2.Zero,
                    //scale: new Vector2(1, ScrollViewer.Viewport.Height - TimeLineHeight),
                    //effects: SpriteEffects.None,
                    //layerDepth: 0
                    //);

                    int moduloFps = 30;

                    if (MainScreen.Config.EventSnapType == TaeConfigFile.EventSnapTypes.FPS60)
                        moduloFps = 60;

                    if (i % moduloFps == 0)
                        continue;

                        ImGuiDebugDrawer.DrawLine(new Vector2((float)(i * framePixelSize_TAE - ScrollViewer.Scroll.X), TimeLineHeight),
                        new Vector2((float)(i * framePixelSize_TAE - ScrollViewer.Scroll.X), ScrollViewer.Viewport.Height),
                        col);
                }
            }
        }

        private void DrawEventBox(float elapsedSeconds, Matrix scrollMatrix, GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font, SpriteFont smallFont, 
            TaeEditAnimEventBox box, bool isHover, float opacity)
        {
            if (IsSimpleRemoGroupMode)
            {
                box.VisualRow = box.Row;
            }

            var isBoxSelected = (MainScreen.SelectedEventBox == box) || (MainScreen.MultiSelectedEventBoxes.Contains(box));

            var boxHighlightedAndVisiActive = box.PlaybackHighlight && IsNewGraphVisiMode && (PlaybackCursor.Scrubbing || PlaybackCursor.IsPlaying);

            isHover = isHover && IsNewGraphVisiMode && (PlaybackCursor.Scrubbing || PlaybackCursor.IsPlaying);

            if (isBoxSelected)
                box.UpdateEventText();

            if (box.Left > ScrollViewer.RelativeViewport.Right
                || box.Right < ScrollViewer.RelativeViewport.Left)
                return;

            bool eventStartsBeforeScreen = box.Left < ScrollViewer.Scroll.X;

            float eventStartBeforeScreenSpaceOnScreen = box.Right - ScrollViewer.Scroll.X;
            bool forceNoScrollStr = eventStartBeforeScreenSpaceOnScreen <= 48;

            Vector2 pos = new Vector2(box.Left + 1, box.Top + 1);
            Vector2 size = new Vector2(box.Width - 1, box.HeightFr - 1);

            Vector2 outlinePos = pos + new Vector2(0, -1);
            Vector2 outlineSize = size + new Vector2(0, 2);

            int boxOutlineThickness = 1;// isBoxSelected ? 2 : 1;

            Color textFG = (box.PlaybackHighlight ? Main.Colors.GuiColorEventBox_Highlighted_Text : Main.Colors.GuiColorEventBox_Normal_Text);
            Color textBG = (box.PlaybackHighlight ? Main.Colors.GuiColorEventBox_Highlighted_TextShadow : Main.Colors.GuiColorEventBox_Normal_TextShadow);

            if (!box.IsStateInfoEnabled)
            {
                textFG = new Color(textFG.ToVector4() * new Vector4(0.5f, 0.5f, 0.5f, 1));
                textBG = new Color(textBG.ToVector4() * new Vector4(0.5f, 0.5f, 0.5f, 1));
            }

            Color boxOutlineColor = (box.PlaybackHighlight ? Main.Colors.GuiColorEventBox_Highlighted_Outline : Main.Colors.GuiColorEventBox_Normal_Outline);

            //if (updateColorPulse)
            //    box.UpdateSelectionColorPulse(elapsedSeconds, isBoxSelected);

            Color thisBoxBgColor = (box.PlaybackHighlight ? Main.Colors.GuiColorEventBox_Highlighted_Fill : Main.Colors.GuiColorEventBox_Normal_Fill);
            //Color thisBoxBgColorSelected = /* isBoxSelected ? box.ColorBGHighlighted_Selected : */ box.ColorBGHighlighted;

            const string fixedPrefix = "<-";

            var boxRect = box.GetTextRect(boxOutlineThickness, IsNewGraphVisiMode);

            var fontOffsetToUse = IsNewGraphVisiMode ? new Vector2(0, -1) : Main.GlobalTaeEditorFontOffset + new Vector2(0, -1);

            var fontToUse = (IsNewGraphVisiMode || !(MainScreen.Config.EnableFancyScrollingStrings && boxRect.Width >= TinyTextBoxWidth)) ? smallFont : font;

            

            string fullTextWithPrefix = $"{(eventStartsBeforeScreen ? fixedPrefix : "")}{box.EventText.Text}";

            var nameSize = fontToUse.MeasureString(fullTextWithPrefix);

            var namePos = new Vector2((float)MathHelper.Max((float)Math.Round(ScrollViewer.Scroll.X), (float)Math.Round(pos.X - (IsNewGraphVisiMode ? 0 : 1))), 
                (float)Math.Round(pos.Y + 4)) + fontOffsetToUse;


            void DrawActualBox()
            {
                ImGuiDebugDrawer.DrawRect((outlinePos - ScrollViewer.Scroll).RoundInt(), outlineSize, boxOutlineColor * opacity, 3);
                ImGuiDebugDrawer.DrawRect((outlinePos - ScrollViewer.Scroll + new Vector2(1,1)).RoundInt(), outlineSize + new Vector2(-2,-2), thisBoxBgColor * opacity, 3);

                //sb.Draw(texture: boxTex,
                //       position: outlinePos,
                //       sourceRectangle: null,
                //       color: boxOutlineColor * opacity,
                //       rotation: 0,
                //       origin: Vector2.Zero,
                //       scale: outlineSize,
                //       effects: SpriteEffects.None,
                //       layerDepth: 0
                //       );




                //sb.Draw(texture: boxTex,
                //    position: pos + new Vector2(boxOutlineThickness, boxOutlineThickness - 1),
                //    sourceRectangle: null,
                //    color: thisBoxBgColor * opacity,
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: size + new Vector2(-boxOutlineThickness * 2, (-boxOutlineThickness * 2) + 2),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                //if (!box.IsStateInfoEnabled)
                //{
                //    sb.Draw(texture: boxTex,
                //       position: outlinePos,
                //       sourceRectangle: null,
                //       color: Color.Black * 0.5f,
                //       rotation: 0,
                //       origin: Vector2.Zero,
                //       scale: outlineSize,
                //       effects: SpriteEffects.None,
                //       layerDepth: 0
                //       );
                //}
            }

            void DrawOverlayOutline()
            {
                //// Top
                //sb.Draw(texture: boxTex,
                //    position: new Vector2(outlinePos.X, outlinePos.Y),
                //    sourceRectangle: null,
                //    color: Color.Yellow * opacity,
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(outlineSize.X, boxOutlineThickness),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                //// Left
                //sb.Draw(texture: boxTex,
                //   position: new Vector2(outlinePos.X, outlinePos.Y),
                //   sourceRectangle: null,
                //   color: Color.Yellow * opacity,
                //   rotation: 0,
                //   origin: Vector2.Zero,
                //   scale: new Vector2(boxOutlineThickness, outlineSize.Y),
                //   effects: SpriteEffects.None,
                //   layerDepth: 0
                //   );

                //// Bottom
                //sb.Draw(texture: boxTex,
                //    position: new Vector2(outlinePos.X, outlinePos.Y + outlineSize.Y - boxOutlineThickness),
                //    sourceRectangle: null,
                //    color: Color.Yellow * opacity,
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(outlineSize.X, boxOutlineThickness),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                //// Right
                //sb.Draw(texture: boxTex,
                //   position: new Vector2(outlinePos.X + outlineSize.X - boxOutlineThickness, outlinePos.Y),
                //   sourceRectangle: null,
                //   color: Color.Yellow * opacity,
                //   rotation: 0,
                //   origin: Vector2.Zero,
                //   scale: new Vector2(boxOutlineThickness, outlineSize.Y),
                //   effects: SpriteEffects.None,
                //   layerDepth: 0
                //   );
            }

            //DrawOverlayOutline();

            if (!boxHighlightedAndVisiActive && !isHover && MainScreen.Config.EnableFancyScrollingStrings && boxRect.Width >= TinyTextBoxWidth && !forceNoScrollStr)
            {
                DrawActualBox();

                //var fontToUse = IsSmallEventsMode ? smallFont : font;

                // The weird offset of 1 here is so there's an extra pixel space for the shadow to render
                var fancyTextRect = new Rectangle(boxRect.X - 1, boxRect.Y - 1, boxRect.Width + 1, boxRect.Height + 1);

                // This fixes the text not being lined up right with the above offset.
                fontOffsetToUse.X += 1; // I'm so sorry.

                // Wasn't lined up for some reason. Works with this here.
                fontOffsetToUse.Y += 1;

                //if (IsNewGraphVisiMode)
                //{
                    
                //}

                if (eventStartsBeforeScreen)
                {
                    var fixedPrefixSize = (fontToUse.MeasureString(fixedPrefix)).ToPoint();

                    int amountOutOfScreen = (int)Math.Round(ScrollViewer.Scroll.X) - fancyTextRect.X;

                    fancyTextRect = new Rectangle(
                        fancyTextRect.X + fixedPrefixSize.X + amountOutOfScreen,
                        fancyTextRect.Y,
                        fancyTextRect.Width - fixedPrefixSize.X - amountOutOfScreen,
                        fancyTextRect.Height);

                    var prefixPos = namePos;// new Vector2((float)Math.Round(ScrollViewer.Scroll.X), (float)Math.Round((float)boxRect.Y));

                    if (!IsNewGraphVisiMode)
                    {
                        prefixPos.Y += 3;
                    }

                    //if (IsSmallEventsMode)
                    //{
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, 1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, -1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(-1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //}
                    //else
                    //{
                    //    sb.DrawString(fontToUse, fixedPrefix,
                    //    prefixPos + Vector2.One + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //}

                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, 1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, -1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(-1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);


                    ////sb.DrawString(fontToUse, fixedPrefix,
                    ////    prefixPos + (Vector2.One * 2), textBG);
                    //sb.DrawString(fontToUse, fixedPrefix,
                    //    prefixPos + fontOffsetToUse, textFG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    ImGuiDebugDrawer.DrawText(fixedPrefix, prefixPos + new Vector2(0, -7) - ScrollViewer.RoundedScroll * new Vector2(0, 1) + new Vector2(-ScrollViewer.RoundedScroll.X, 0), textFG * opacity, fontSize: 20);
                }

                box.EventText.TextColor = textFG * opacity;
                box.EventText.TextShadowColor = textBG * opacity;
                box.EventText.ScrollPixelsPerSecond = MainScreen.Config.FancyScrollingStringsScrollSpeed;
                box.EventText.ScrollingSnapsToPixels = MainScreen.Config.FancyTextScrollSnapsToPixels;

                if (MainScreen.HoveringOverEventBox == box)
                {
                    box.EventText.Draw(gd, sb, scrollMatrix * Main.DPIMatrix, fancyTextRect, 16f, elapsedSeconds, fontOffsetToUse);

                    if (isHover)
                        DrawOverlayOutline();

                    if (MainScreen.PrevHoveringOverEventBox != box)
                    {
                        //HoverInfoBox.Title = box.GetPopupTitle();
                        //HoverInfoBox.Text = box.GetPopupText();
                        HoverInfoBox.Update(mouseInside: false, elapsedSeconds);
                    }
                }
                else
                {
                    box.EventText.ResetScroll(startImmediatelyNextTime: true);
                    box.EventText.Draw(gd, sb, scrollMatrix * Main.DPIMatrix, fancyTextRect, 16f, 0, fontOffsetToUse);

                    if (isHover)
                        DrawOverlayOutline();
                }
            }
            else
            {
                

                box.EventText.ResetScroll(startImmediatelyNextTime: true);

                var thicknessOffset = new Vector2(2/*boxOutlineThickness*/ * 2, 0);


                // Draw dimming rect behind active event's text to make it readable.
                if (boxHighlightedAndVisiActive || isHover)
                {
                    ImGuiDebugDrawer.DrawRect(namePos - ScrollViewer.RoundedScroll - new Vector2(2, 6), nameSize + new Vector2(6, 10), Color.Black * 0.6f * opacity, 6);
                }

                DrawActualBox();

                

                string shortTextNoPrefix = /* $"{(eventStartsBeforeScreen ? fixedPrefix : "")}" + */
                       ((isHover || boxHighlightedAndVisiActive) ? box.EventText.Text : box.DispEventName);

                var fancyTextRect = new Rectangle(boxRect.X - 1, boxRect.Y - 1, boxRect.Width + 1, boxRect.Height + 1);

                if (eventStartsBeforeScreen)
                {
                    var fixedPrefixSize = (fontToUse.MeasureString(fixedPrefix)).ToPoint();

                    int amountOutOfScreen = (int)Math.Round(ScrollViewer.Scroll.X) - fancyTextRect.X;

                    fancyTextRect = new Rectangle(
                        fancyTextRect.X + fixedPrefixSize.X + amountOutOfScreen,
                        fancyTextRect.Y,
                        fancyTextRect.Width - fixedPrefixSize.X - amountOutOfScreen,
                        fancyTextRect.Height);

                    var prefixPos = namePos;// new Vector2((float)Math.Round(ScrollViewer.Scroll.X), (float)Math.Round((float)boxRect.Y));

                    if (!IsNewGraphVisiMode)
                    {
                        prefixPos.Y += 3;
                    }

                    //if (IsSmallEventsMode)
                    //{
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, 1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, -1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(-1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //    sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //}
                    //else
                    //{
                    //    sb.DrawString(fontToUse, fixedPrefix,
                    //    prefixPos + Vector2.One + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //}

                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, 1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(0, -1) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(-1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, fixedPrefix, prefixPos + new Vector2(1, 0) + fontOffsetToUse, textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);


                    ////sb.DrawString(fontToUse, fixedPrefix,
                    ////    prefixPos + (Vector2.One * 2), textBG);
                    //sb.DrawString(fontToUse, fixedPrefix,
                    //    prefixPos + fontOffsetToUse, textFG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    ImGuiDebugDrawer.DrawText(fixedPrefix, prefixPos + new Vector2(0, -7) - ScrollViewer.RoundedScroll * new Vector2(0,1) + new Vector2(-ScrollViewer.RoundedScroll.X,0), textFG * opacity, fontSize: 20);
                }


                if (isHover)
                {
                    var hoverTextOutlineColor = Main.Colors.GuiColorEventBox_Hover_TextOutline;

                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(2, 0) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(2, 1) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(2, 2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(1, 2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(0, 2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-1, 2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-2, 2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-2, 1) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-2, 0) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-2, -1) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-2, -2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-1, -2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(0, -2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(1, -2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(2, -2) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(2, -1) + thicknessOffset, hoverTextOutlineColor * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }
                else
                {
                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(0, 1) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(0, -1) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(-1, 0) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(1, 0) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);



                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(1, 1) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(2, 2) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(2, 1) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

                    //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + new Vector2(1, 2) + thicknessOffset,
                    //textBG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                }

                if (isHover)
                    DrawOverlayOutline();

                //sb.DrawString(fontToUse, shortTextNoPrefix, namePos + thicknessOffset,
                //    textFG * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, 0);


                ImGuiDebugDrawer.DrawText(shortTextNoPrefix, (namePos + thicknessOffset - ScrollViewer.RoundedScroll + new Vector2(eventStartsBeforeScreen ? 9 : 0, -5)).RoundInt(), textFG * opacity, fontSize: 16);

                //if ((namePos.X + nameSize.X) <= box.RightFr)
                //{
                //    sb.DrawString(font, fullTextWithPrefix,
                //        namePos + new Vector2(1, 0) + Vector2.One + thicknessOffset, textBG);
                //    //sb.DrawString(font, fullTextWithPrefix,
                //    //    namePos + new Vector2(1, 0) + (Vector2.One * 2) + thicknessOffset, textBG);
                //    sb.DrawString(font, fullTextWithPrefix,
                //        namePos + new Vector2(1, 0) + thicknessOffset, textFG);
                //}
                //else
                //{
                //    string shortTextWithPrefix = $"{(eventStartsBeforeScreen ? fixedPrefix : "")}" +
                //        $"{(box.MyEvent.TypeName)}";
                //    sb.DrawString(smallFont, shortTextWithPrefix, namePos + (Vector2.One) + thicknessOffset,
                //        textBG, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                //    //sb.DrawString(font, shortTextWithPrefix, namePos + (Vector2.One * 2) + thicknessOffset,
                //    //    textBG, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
                //    sb.DrawString(smallFont, shortTextWithPrefix, namePos + thicknessOffset,
                //        textFG, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                //}
            }
            if (box.MyEvent.Group != null && IsSimpleRemoGroupMode)
            {
                var strBld = new StringBuilder();
                var evBoxGrp = box.MyEvent.Group;

                Vector2 boxDebugTextPos = boxRect.BottomLeftCorner() + new Vector2(0, -12);

                if (evBoxGrp != null)
                {
                    if (boxDebugTextPos.X < ScrollViewer.Scroll.X)
                    {
                        boxDebugTextPos.X = (int)Math.Round(ScrollViewer.Scroll.X + 1);
                        strBld.Append("<-");
                    }

                    if (evBoxGrp.GroupType != 128)
                    {
                        strBld.Append($"GroupType{evBoxGrp.GroupType}");
                    }


                    if (evBoxGrp.GroupData.DataType == TAE.EventGroup.EventGroupDataType.ApplyToSpecificCutsceneEntity)
                    {
                        //sb.AppendLine();
                        //sb.Append("Affects Remo Entity ");
                        if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.Character)
                            strBld.Append($"c{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                        else if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.Object)
                            strBld.Append($"o{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                        else if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.DummyNode)
                            strBld.Append($"d{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                        else if (evBoxGrp.GroupData.CutsceneEntityType == TAE.EventGroup.EventGroupDataStruct.EntityTypes.MapPiece)
                        {
                            if (evBoxGrp.GroupData.Block >= 0)
                                strBld.Append($"m{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}B{evBoxGrp.GroupData.Block}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                            else
                                strBld.Append($"m{evBoxGrp.GroupData.CutsceneEntityIDPart1:D4}B{RemoManager.BlockInt}_{evBoxGrp.GroupData.CutsceneEntityIDPart2:D4}");
                        }

                        if (evBoxGrp.GroupData.CutsceneEntityType != TAE.EventGroup.EventGroupDataStruct.EntityTypes.MapPiece &&
                            (evBoxGrp.GroupData.Block >= 0 || evBoxGrp.GroupData.Area >= 0))
                        {
                            strBld.Append($" (from m{evBoxGrp.GroupData.Area:D2}_{evBoxGrp.GroupData.Block:D2})");
                        }
                    }
                }
                string boxDebugText = strBld.ToString();
               
                ImGuiDebugDrawer.DrawText(boxDebugText, boxDebugTextPos - ScrollViewer.Scroll, Color.Lime, null, 16);
            }
        }

        //private void DrawSelectedEventBoxThickOutline(SpriteBatch sb, Texture2D boxTex, TaeEditAnimEventBox box, float opacity)
        //{
        //    Vector2 pos = new Vector2((box.Left + 1) - 2, ((box.Top + 1) - 1) - 2);
        //    Vector2 size = new Vector2((box.Width - 1) + 4, ((box.HeightFr - 1) + 2) + 4);

        //    void DoCornerThing(Vector2 cornerPos, Vector2 cornerSize)
        //    {
        //        sb.Draw(texture: boxTex,
        //           position: cornerPos + new Vector2(-1, -1),
        //           sourceRectangle: null,
        //           color: (box.PlaybackHighlight ? Color.Yellow : Color.Black) * opacity,
        //           rotation: 0,
        //           origin: Vector2.Zero,
        //           scale: cornerSize + new Vector2(2, 2),
        //           effects: SpriteEffects.None,
        //           layerDepth: 0
        //           );

        //        sb.Draw(texture: boxTex,
        //           position: cornerPos,
        //           sourceRectangle: null,
        //           color: (box.PlaybackHighlight ? box.ColorBGHighlighted_Selected : box.ColorBG_Selected) * opacity,
        //           rotation: 0,
        //           origin: Vector2.Zero,
        //           scale: cornerSize,
        //           effects: SpriteEffects.None,
        //           layerDepth: 0
        //           );
        //    }

        //    int off = 1;

        //    //if (selectionBoxCornerPulseTimer < (selectionBoxCornerPulseTimerCycleLength * 0.25f))
        //    //    off = 0;
        //    //else if (selectionBoxCornerPulseTimer < (selectionBoxCornerPulseTimerCycleLength * 0.5f))
        //    //    off = 1;
        //    //else if (selectionBoxCornerPulseTimer < (selectionBoxCornerPulseTimerCycleLength * 0.75f))
        //    //    off = 0;
        //    //else
        //    //    off = 1;

        //    DoCornerThing(pos + new Vector2(off, off), new Vector2(4, 4));
        //    DoCornerThing(pos + new Vector2(size.X - 4, 0) + new Vector2(-off, off), new Vector2(4, 4));
        //    DoCornerThing(pos + new Vector2(size.X - 4, size.Y - 4) + new Vector2(-off, -off), new Vector2(4, 4));
        //    DoCornerThing(pos + new Vector2(0, size.Y - 4) + new Vector2(off, -off), new Vector2(4, 4));
        //}

        //private float selectionBoxCornerPulseTimer = 0;
        //private float selectionBoxCornerPulseTimerCycleLength = 1;

        private void DrawDebugTextInGraph(string text, Vector2 position, Color color)
        {

        }

        private void RefreshLogicalAndVisualRowMappings()
        {
            lock (_lock_VisualRowToLogicalRowMap)
            {
                LogicalRowToVisualRowMap.Clear();
                VisualRowToLogicalRowMap.Clear();


                List<int> logicalRowsMapped = new List<int>();

                void registerRow(int visual, int logical)
                {
                    if (logicalRowsMapped.Contains(logical) && logical >= 0)
                        logical = -1;
                    else
                        logicalRowsMapped.Add(logical);
                    while (VisualRowToLogicalRowMap.Count <= visual)
                    {
                        if (VisualRowToLogicalRowMap.Count > 0)
                        {
                            VisualRowToLogicalRowMap.Add(VisualRowToLogicalRowMap[VisualRowToLogicalRowMap.Count - 1]);
                        }
                        else
                        {
                            VisualRowToLogicalRowMap.Add(0);
                        }

                    }
                    VisualRowToLogicalRowMap[visual] = logical;

                    if (logical >= 0)
                    {
                        while (LogicalRowToVisualRowMap.Count <= visual)
                        {
                            if (LogicalRowToVisualRowMap.Count > 0)
                            {
                                LogicalRowToVisualRowMap.Add(LogicalRowToVisualRowMap[LogicalRowToVisualRowMap.Count - 1]);
                            }
                            else
                            {
                                LogicalRowToVisualRowMap.Add(0);
                            }

                        }
                        LogicalRowToVisualRowMap[logical] = visual;
                    }
                }

                int currentVisualRow = 0;
                int currentLogicalRow = 0;

                foreach (var group in GroupRegions)
                {
                    while (currentLogicalRow < group.StartRow)
                    {
                        registerRow(currentVisualRow, currentLogicalRow);
                        currentVisualRow++;
                        currentLogicalRow++;
                    }

                    // Group start - invalid logical row
                    registerRow(currentVisualRow, -1);
                    currentVisualRow++;
                    currentLogicalRow++;

                    for (int i = 1; i < group.RowCount; i++)
                    {
                        registerRow(currentVisualRow, currentLogicalRow);
                        currentVisualRow++;
                        currentLogicalRow++;
                    }
                }


                lock (_lock_EventBoxManagement)
                {
                    foreach (var b in EventBoxes)
                    {
                        b.VisualRow = IsFancyCollapsingRemoGroupMode ? GetVisualRowFromLogicalRow(b.Row) : b.Row;
                    }
                }
            }
        }

        private void DrawAllEventBoxes(float opacity, GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
            SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Matrix scrollMatrix)
        {
            //selectionBoxCornerPulseTimer += elapsedSeconds;
            //selectionBoxCornerPulseTimer = selectionBoxCornerPulseTimer % selectionBoxCornerPulseTimerCycleLength;

            void debugText(string text, Vector2 pos, Color c)
            {
                //sb.DrawString(smallFont, text, pos + new Vector2(0, 1), Color.Black);
                //sb.DrawString(smallFont, text, pos + new Vector2(0, -1), Color.Black);
                //sb.DrawString(smallFont, text, pos + new Vector2(-1, 0), Color.Black);
                //sb.DrawString(smallFont, text, pos + new Vector2(1, 0), Color.Black);
                //sb.DrawString(smallFont, text, pos, c);
                ImGuiDebugDrawer.DrawText(text, pos - ScrollViewer.Scroll, c, null, 16);
            }

            bool isHoverBoxAlsoSelected = false;

            List<TaeEditAnimEventBox> selectedBoxes = new List<TaeEditAnimEventBox>();

            bool oddRowAlternateFlag = false;

            foreach (var gr in GroupRegions)
            {
                var rowThisGroupHeaderIsOn = GetVisualRowFromLogicalRow(gr.StartRow);

                List<TaeEditAnimEventBox> highlightedBoxes = new List<TaeEditAnimEventBox>();

                // Placeholder colors lol
                var groupHeaderColorBG = oddRowAlternateFlag ? Color.Red : Color.Green;
                var groupRegionColorBG = (oddRowAlternateFlag ? Color.Red : Color.Green) * 0.5f;
                var groupHeaderColorFG = oddRowAlternateFlag ? Color.Black : Color.Black;

                oddRowAlternateFlag = !oddRowAlternateFlag;

                if (IsFancyCollapsingRemoGroupMode)
                {

                    if (!gr.Collapsed)
                    {
                        ImGuiDebugDrawer.DrawRect(new Rectangle(
                        (int)(ScrollViewer.Scroll.X - 8),
                        (int)(TimeLineHeight + (rowThisGroupHeaderIsOn * RowHeight)),
                        (int)(ScrollViewer.Viewport.Width + 16),
                        (int)(RowHeight * (gr.RowCount + 1))).SubtractScroll(ScrollViewer.Scroll), groupRegionColorBG);
                    }

                    ImGuiDebugDrawer.DrawRect(new Rectangle(
                        (int)(ScrollViewer.Scroll.X - 8),
                        (int)(TimeLineHeight + (rowThisGroupHeaderIsOn * RowHeight)),
                        (int)(ScrollViewer.Viewport.Width + 16),
                        (int)(RowHeight * 1)).SubtractScroll(ScrollViewer.Scroll), groupHeaderColorBG);

                    ImGuiDebugDrawer.DrawText(gr.GetGroupText(), new Vector2(24, TimeLineHeight + (rowThisGroupHeaderIsOn * RowHeight) + 2 - ScrollViewer.Scroll.Y), groupHeaderColorFG, null, 16f);

                }

                if (!gr.Collapsed)
                {
                    rowThisGroupHeaderIsOn += gr.RowCount;
                }
            }

            lock (_lock_EventBoxManagement)
            {
                foreach (var kvp in sortedByRow)
                {
                    var boxesOrderedByTime = sortedByRow[kvp.Key].OrderBy(x => x.MyEvent.StartTime);

                    var highlightedBoxes = new List<TaeEditAnimEventBox>();

                    foreach (var box in boxesOrderedByTime)
                    {
                        if (box.CurrentGroupRegion?.Collapsed == true)
                            continue;

                        var isBoxSelected = (MainScreen.SelectedEventBox == box) || (MainScreen.MultiSelectedEventBoxes.Contains(box));

                        if (MainScreen.HoveringOverEventBox == box)
                        {
                            isHoverBoxAlsoSelected = isBoxSelected;
                            continue;
                        }

                        if (isBoxSelected)
                        {
                            selectedBoxes.Add(box);
                        }
                        else if (box.PlaybackHighlight && IsNewGraphVisiMode && (PlaybackCursor.Scrubbing || PlaybackCursor.IsPlaying))
                        {
                            highlightedBoxes.Add(box);
                        }
                        else
                        {

                            DrawEventBox(elapsedSeconds, scrollMatrix, gd, sb, boxTex, font, smallFont, box, isHover: false, opacity);
                            //debugText($"EventLogicalRow{box.Row}", new Vector2(box.Left, box.Top), Color.Cyan);
                        }
                    }

                    foreach (var box in highlightedBoxes)
                    {

                        DrawEventBox(elapsedSeconds, scrollMatrix, gd, sb, boxTex, font, smallFont, box, isHover: false, opacity);
                        //debugText($"EventLogicalRow{box.Row}", new Vector2(box.Left, box.Top), Color.Cyan);
                    }
                }
            }

            if (MainScreen.HoveringOverEventBox != null && !isHoverBoxAlsoSelected)
            {
                
                DrawEventBox(elapsedSeconds, scrollMatrix, gd, sb, boxTex, font, smallFont, MainScreen.HoveringOverEventBox, isHover: true, opacity);
                //debugText($"EventLogicalRow{MainScreen.HoveringOverEventBox.Row}", new Vector2(MainScreen.HoveringOverEventBox.Left, MainScreen.HoveringOverEventBox.Top), Color.Cyan);
            }

            if (selectedBoxes.Count > 0 || isHoverBoxAlsoSelected)
            {
                // full bg
                //sb.Draw(texture: boxTex,
                //    position: ScrollViewer.Scroll - (Vector2.One * 2),
                //    sourceRectangle: null,
                //    color: Main.Colors.GuiColorEventBox_SelectionDimmingOverlay,
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(ScrollViewer.Viewport.Width, ScrollViewer.Viewport.Height) + (Vector2.One * 4),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                ImGuiDebugDrawer.DrawRect(new Rectangle(-2, -2, ScrollViewer.Viewport.Width + 4, ScrollViewer.Viewport.Height + 4), Main.Colors.GuiColorEventBox_SelectionDimmingOverlay);

                selectedBoxes = selectedBoxes.OrderBy(b => b.PlaybackHighlight).ToList();

                // Draw the extra outer outline on the selected event(s)
                //foreach (var box in selectedBoxes)
                //{
                //    box.UpdateSelectionColorPulse(elapsedSeconds, true);
                //    DrawSelectedEventBoxThickOutline(sb, boxTex, box, opacity);
                //}

                foreach (var box in selectedBoxes)
                {
                    DrawEventBox(elapsedSeconds, scrollMatrix, gd, sb, boxTex, font, smallFont, box, isHover: false, opacity);
                    //debugText($"EventLogicalRow{box.Row}", new Vector2(box.Left, box.Top), Color.Cyan);
                }

            }

            if (MainScreen.HoveringOverEventBox != null && isHoverBoxAlsoSelected)
            {
                //MainScreen.HoveringOverEventBox.UpdateSelectionColorPulse(elapsedSeconds, true);
                //DrawSelectedEventBoxThickOutline(sb, boxTex, MainScreen.HoveringOverEventBox, opacity);
                DrawEventBox(elapsedSeconds, scrollMatrix, gd, sb, boxTex, font, smallFont, MainScreen.HoveringOverEventBox, isHover: true, opacity);
                //debugText($"EventLogicalRow{MainScreen.HoveringOverEventBox.Row}", new Vector2(MainScreen.HoveringOverEventBox.Left, MainScreen.HoveringOverEventBox.Top), Color.Cyan);
            }


            if (EventGroupDebugDrawEnable)
            {
                for (int i = 0; i < VisualRowToLogicalRowMap.Count; i++)
                {
                    debugText($"LogicalRow:{VisualRowToLogicalRowMap[i]}", new Vector2(ScrollViewer.Scroll.X + 4, TimeLineHeight + (i * RowHeight) + 10), Color.Fuchsia);
                }

                debugText($"MouseLogicalRow:{MouseLogicalRow}", relMouse + new Vector2(12, TimeLineHeight), Color.Lime);
            }
        }

        public void ScrollToPlaybackCursor(float lerpAmount)
        {
            const float margin = 200;

            var playbackCursorPixelCheckX = SecondsPixelSize * (float)PlaybackCursor.CurrentTime;
            var endOfAnimCheckX = SecondsPixelSize * (float)PlaybackCursor.MaxTime;
            float centerOfScreenCheckX = (ScrollViewer.Scroll.X + (ScrollViewer.Viewport.Width / 2));
            float rightOfScreenCheckX = centerOfScreenCheckX;// (ScrollViewer.Scroll.X + (ScrollViewer.Viewport.Width) - margin);
            float leftOfScreenCheckX = centerOfScreenCheckX;// (ScrollViewer.Scroll.X + margin);

            if (playbackCursorPixelCheckX > rightOfScreenCheckX)
            {
                ScrollViewer.Scroll.X += (playbackCursorPixelCheckX - rightOfScreenCheckX) * lerpAmount;

                var maxScrollX = (endOfAnimCheckX + AfterAutoScrollHorizontalMargin - ScrollViewer.Viewport.Width);
                if (ScrollViewer.Scroll.X > maxScrollX)
                    ScrollViewer.Scroll.X = maxScrollX;

                ScrollViewer.ClampScroll();
            }
            else if (playbackCursorPixelCheckX < leftOfScreenCheckX)
            {
                ScrollViewer.Scroll.X += (playbackCursorPixelCheckX - leftOfScreenCheckX) * lerpAmount;
                ScrollViewer.ClampScroll();
            }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, 
            SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Texture2D scrollbarArrowTex)
        {
            var playbackCursorPixelCheckX = SecondsPixelSize * (float)(PlaybackCursor.Scrubbing ? PlaybackCursor.CurrentTime : PlaybackCursor.CurrentTimeMod);
            float centerOfScreenCheckX = (ScrollViewer.Scroll.X + (ScrollViewer.Viewport.Width / 2));
            //float rightOfScreenCheckX = ScrollViewer.Viewport.Width + ScrollViewer.Scroll.X;

            float maxHorizontalScrollNeededForAnimEndToBeInScreen =
                ((SecondsPixelSize * (float)PlaybackCursor.MaxTime)
                + AfterAutoScrollHorizontalMargin) - ScrollViewer.Viewport.Width;

            if (PlaybackCursor.Scrubbing &&
                !(MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.RightShift)))
            {
                float rightScrubScrollMarginStart = (ScrollViewer.Scroll.X + ScrollViewer.Viewport.Width) - ScrubScrollStartMargin;
                float leftScrubScrollMarginStart = (ScrollViewer.Scroll.X + ScrubScrollStartMargin);
                if (playbackCursorPixelCheckX > rightScrubScrollMarginStart)
                {
                    float scrubScrollSpeedMult = MathHelper.Max(
                        playbackCursorPixelCheckX - rightScrubScrollMarginStart, 0) / ScrubScrollStartMargin;

                    ScrollViewer.Scroll.X += ScrubScrollSpeed * (scrubScrollSpeedMult * scrubScrollSpeedMult);

                    ScrollViewer.ClampScroll();
                }
                else if (playbackCursorPixelCheckX < leftScrubScrollMarginStart)
                {
                    float scrubScrollSpeedMult = MathHelper.Max(
                        leftScrubScrollMarginStart - playbackCursorPixelCheckX, 0) / ScrubScrollStartMargin;

                    ScrollViewer.Scroll.X -= ScrubScrollSpeed * (scrubScrollSpeedMult * scrubScrollSpeedMult);
                    ScrollViewer.ClampScroll();
                }
            }
            else if (MainScreen.Config.AutoScrollDuringAnimPlayback && PlaybackCursor.IsPlaying)
            {
                if ((ScrollViewer.Scroll.X < maxHorizontalScrollNeededForAnimEndToBeInScreen) ||
                    playbackCursorPixelCheckX < ScrollViewer.Scroll.X)
                {
                    if (playbackCursorPixelCheckX < ScrollViewer.Scroll.X)
                    {
                        ScrollViewer.Scroll.X = playbackCursorPixelCheckX;
                    }
                    else

                    {
                        float distFromScrollToCursor = playbackCursorPixelCheckX - centerOfScreenCheckX;

                        ScrollViewer.Scroll.X += distFromScrollToCursor * AutoScrollLerpDistMult;
                        ScrollViewer.Scroll.X = Math.Min(ScrollViewer.Scroll.X, maxHorizontalScrollNeededForAnimEndToBeInScreen);
                        ScrollViewer.ClampScroll();
                    }


                }
            }


            ScrollViewer.SetDisplayRect(ScrollViewerRect, GetVirtualAreaSize());

            ScrollViewer.Draw(gd, sb, boxTex, scrollbarArrowTex);

            var scrollMatrix = ScrollViewer.GetScrollMatrix();

            
            

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport.DpiScaled());
            {
                sb.Begin(transformMatrix: scrollMatrix * Main.DPIMatrix);
                try
                {

                    // full bg
                    sb.Draw(texture: boxTex,
                        position: ScrollViewer.Scroll - (Vector2.One * 2),
                        sourceRectangle: null,
                        color: Main.Colors.GuiColorEventGraphBackground,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(ScrollViewer.Viewport.Width, ScrollViewer.Viewport.Height) + (Vector2.One * 4),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );

                    //sb.Draw(texture: boxTex,
                    //    position: ScrollViewer.Scroll - (Vector2.One * 4),
                    //    sourceRectangle: null,
                    //    color: Main.Colors.GuiColorEventGraphTimelineFill,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight) + new Vector2(8, 4),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );

                    ImGuiDebugDrawer.DrawRect((Vector2.One * -4), new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight) + new Vector2(8, 4), Main.Colors.GuiColorEventGraphTimelineFill);

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

                    DrawFrameSnapLines(boxTex, sb, Main.Colors.GuiColorEventGraphVerticalFrameLines);

                    bool zoomedEnoughForFrameNumbers = SecondsPixelSize >= ((1 / (float)PlaybackCursor.CurrentSnapInterval) * MinPixelsBetweenFramesForFrameNumberText);

                    Dictionary<int, float> secondVerticalLineXPositions = new Dictionary<int, float>();

                    if (SecondsPixelSize >= 4f)
                    {
                        secondVerticalLineXPositions = GetSecondVerticalLineXPositions();

                        foreach (var kvp in secondVerticalLineXPositions)
                        {
                            sb.Draw(texture: boxTex,
                            position: new Vector2(kvp.Value, ScrollViewer.Scroll.Y),
                            sourceRectangle: null,
                            color: Main.Colors.GuiColorEventGraphVerticalSecondLines,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: new Vector2(1, ScrollViewer.Viewport.Height),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );
                        }
                    }

                    if (!zoomedEnoughForFrameNumbers && SecondsPixelSize >= 32f)
                    {
                        foreach (var kvp in secondVerticalLineXPositions)
                        {
                            sb.Draw(texture: boxTex,
                            position: new Vector2(kvp.Value, ScrollViewer.Scroll.Y),
                            sourceRectangle: null,
                            color: Main.Colors.GuiColorEventGraphVerticalSecondLines,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: new Vector2(1, ScrollViewer.Viewport.Height),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );
                        }
                    }

                    float centerOfScreenX = (ScrollViewer.Scroll.X + (ScrollViewer.Viewport.Width / 2));
                    float rightOfScreenX = ScrollViewer.Viewport.Width + ScrollViewer.Scroll.X;


                    // This would draw a little +/- by the mouse cursor to signify that
                    // you're adding/subtracting selection
                    // HOWEVER it was super delayed because of the vsync 
                    // and didn't follow the cursor well and looked weird

                    //if (MainScreen.Input.CtrlHeld && !MainScreen.Input.ShiftHeld && !MainScreen.Input.AltHeld)
                    //{
                    //    sb.DrawString(font, "－", relMouse + new Vector2(12, 12 + TimeLineHeight) + Vector2.One, Color.Black);
                    //    sb.DrawString(font, "－", relMouse + new Vector2(12, 12 + TimeLineHeight), Color.White);
                    //}
                    //else if (!MainScreen.Input.CtrlHeld && MainScreen.Input.ShiftHeld && !MainScreen.Input.AltHeld)
                    //{
                    //    sb.DrawString(font, "＋", relMouse + new Vector2(12, 12 + TimeLineHeight) + Vector2.One, Color.Black);
                    //    sb.DrawString(font, "＋", relMouse + new Vector2(12, 12 + TimeLineHeight), Color.White);
                    //}

                    float animStopPixelX = (float)TaePlaybackCursor.LastMaxTimeGreaterThanZero * SecondsPixelSize;

                    float darkenedPortionWidth = rightOfScreenX - animStopPixelX;

                    if (darkenedPortionWidth > 0)
                    {
                        // full bg - anim duration

                        float cooldownRatio = 1;// 1 - Math.Max(0, Math.Min(1, MathHelper.Lerp(0, 1, MainScreen.AnimSwitchRenderCooldown / MainScreen.AnimSwitchRenderCooldownFadeLength)));

                        sb.Draw(texture: boxTex,
                              position: new Vector2(animStopPixelX - 1, (float)Math.Round(ScrollViewer.Scroll.Y)),
                              sourceRectangle: null,
                              color: Main.Colors.GuiColorEventGraphAnimEndVerticalLine * cooldownRatio,
                              rotation: 0,
                              origin: Vector2.Zero,
                              scale: new Vector2(2, ScrollViewer.Viewport.Height),
                              effects: SpriteEffects.None,
                              layerDepth: 0
                              );
                    }

                    if (GhostEventGraph != null)
                    {
                        GhostEventGraph.SecondsPixelSize = SecondsPixelSize;
                        GhostEventGraph.ScrollViewer.SetDisplayRect(ScrollViewerRect, GetVirtualAreaSize());
                        GhostEventGraph.ScrollViewer.Scroll = ScrollViewer.Scroll;
                        try
                        {
                            GhostEventGraph.DrawAllEventBoxes(1.0f, gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollMatrix);
                        }
                        catch
                        {

                        }

                        // Draw semitransparent overlay.
                        //sb.Draw(texture: boxTex,
                        //position: ScrollViewer.Scroll - (Vector2.One * 2),
                        //sourceRectangle: null,
                        //color: Main.Colors.GuiColorEventGraphGhostOverlay,
                        //rotation: 0,
                        //origin: Vector2.Zero,
                        //scale: new Vector2(ScrollViewer.Viewport.Width, ScrollViewer.Viewport.Height) + (Vector2.One * 4),
                        //effects: SpriteEffects.None,
                        //layerDepth: 0
                        //);
                        ImGuiDebugDrawer.DrawRect(Vector2.One * -2,
                            new Vector2(ScrollViewer.Viewport.Width + 4, ScrollViewer.Viewport.Height + 4),
                            Main.Colors.GuiColorEventGraphGhostOverlay);
                    }
                    else
                    {
                        try
                        {
                            DrawAllEventBoxes(1.0f, gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollMatrix);
                        }
                        catch
                        {

                        }

                    }

                    var rowHorizontalLineYPositions = GetRowHorizontalLineYPositions();

                    foreach (var kvp in rowHorizontalLineYPositions)
                    {
                        sb.Draw(texture: boxTex,
                        position: new Vector2(ScrollViewer.Scroll.X, kvp.Value),
                        sourceRectangle: null,
                        color: Main.Colors.GuiColorEventGraphRowHorizontalLines,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(ScrollViewer.Viewport.Width, 1),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );
                    }

                    // When you change which box (if any) is hovered, update the simulation and stuff.
                    if (MainScreen.PrevHoveringOverEventBox != MainScreen.HoveringOverEventBox)
                    {
                        ViewportInteractor.OnScrubFrameChange();
                        HoverInfoBox.Update(false, 0);
                    }

                    HoverInfoBox.Box = MainScreen.HoveringOverEventBox;

                    HoverInfoBox.Update(mouseInside: MainScreen.HoveringOverEventBox != null, elapsedSeconds);

                    MainScreen.PrevHoveringOverEventBox = MainScreen.HoveringOverEventBox;


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
                    //sb.Draw(texture: boxTex,
                    //    position: ScrollViewer.Scroll - (Vector2.One * 4),
                    //    sourceRectangle: null,
                    //    color: Main.Colors.GuiColorEventGraphTimelineFill,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight) + new Vector2(8, 4),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );

                    ImGuiDebugDrawer.DrawRect((Vector2.One * -4), new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight) + new Vector2(8, 4), Main.Colors.GuiColorEventGraphTimelineFill);


                    if (!zoomedEnoughForFrameNumbers)
                    {
                        foreach (var kvp in secondVerticalLineXPositions)
                        {
                            //sb.Draw(texture: boxTex,
                            //position: new Vector2(kvp.Value, ScrollViewer.Scroll.Y),
                            //sourceRectangle: null,
                            //color: Main.Colors.GuiColorEventGraphVerticalSecondLines,
                            //rotation: 0,
                            //origin: Vector2.Zero,
                            //scale: new Vector2(1, TimeLineHeight),
                            //effects: SpriteEffects.None,
                            //layerDepth: 0
                            //);

                            ImGuiDebugDrawer.DrawLine(new Vector2(kvp.Value - ScrollViewer.Scroll.X, 0), 
                                new Vector2(kvp.Value - ScrollViewer.Scroll.X, TimeLineHeight), Main.Colors.GuiColorEventGraphVerticalSecondLines);
                        }

                        DrawTimeLine(gd, sb, boxTex, font, secondVerticalLineXPositions);
                    }

                    if (SecondsPixelSize >= ((1 / (float)PlaybackCursor.CurrentSnapInterval) * MinPixelsBetweenFramesForHelperLines))
                    {
                        int startFrame = (int)Math.Floor(ScrollViewer.Scroll.X / FramePixelSize_HKX);
                        int endFrame = (int)Math.Ceiling((ScrollViewer.Scroll.X + ScrollViewer.Viewport.Width) / FramePixelSize_HKX);

                        //for (int i = startFrame; i <= endFrame; i++)
                        //{
                        //    ImGuiDebugDrawer.DrawLine(new Vector2(i * FramePixelSize_HKX - ScrollViewer.Scroll.X, 0), 
                        //        new Vector2(i * FramePixelSize_HKX - ScrollViewer.Scroll.X, TimeLineHeight), 
                        //        Main.Colors.GuiColorEventGraphTimelineFrameVerticalLines);

                        //    try
                        //    {
                        //        if (PlaybackCursor.SnapInterval > 0)
                        //        {
                        //            if (((i % PlaybackCursor.SnapInterval) != 0) && zoomedEnoughForFrameNumbers)
                        //            {
                        //                ImGuiDebugDrawer.DrawLine(new Vector2(i * FramePixelSize_HKX - ScrollViewer.Scroll.X, 0),
                        //                    new Vector2(i * FramePixelSize_HKX - ScrollViewer.Scroll.X, TimeLineHeight),
                        //                    Color.White);
                        //            }
                        //        }

                        //    }
                        //    catch
                        //    {
                        //        // todo: see why this was needed lol?
                        //    }

                        //}

                        for (int i = startFrame; i <= endFrame; i++)
                        {
                            ImGuiDebugDrawer.DrawLine(new Vector2(i * FramePixelSize_HKX - ScrollViewer.Scroll.X, 0),
                                new Vector2(i * FramePixelSize_HKX - ScrollViewer.Scroll.X, TimeLineHeight),
                                Main.Colors.GuiColorEventGraphTimelineFrameVerticalLines);

                            try
                            {
                                if (PlaybackCursor.SnapInterval > 0)
                                {
                                    if (((i % PlaybackCursor.SnapInterval) != 0) && zoomedEnoughForFrameNumbers)
                                    {
                                        ImGuiDebugDrawer.DrawText(i.ToString(), new Vector2(i * FramePixelSize_HKX + 4 - ScrollViewer.Scroll.X, 4),
                                            Main.Colors.GuiColorEventGraphTimelineFrameNumberText, null, 15);

                                    }
                                }

                            }
                            catch
                            {
                                // todo: see why this was needed lol?
                            }

                        }
                    }

                    //if (IsBoxDragging)
                    //{
                    //    DrawFrameSnapLines(boxTex, sb, Color.Cyan * 0.65f);
                    //}







                    if (currentDrag.DragType == BoxDragType.MultiSelectionRectangle
                        || currentDrag.DragType == BoxDragType.MultiSelectionRectangleADD
                        || currentDrag.DragType == BoxDragType.MultiSelectionRectangleSUBTRACT)
                    {
                        var multiSelectRect = currentDrag.GetVirtualDragRect();

                        multiSelectRect.Y += (int)TimeLineHeight;

                        // FILL RECT:
                        //sb.Draw(texture: boxTex,
                        //    position: new Vector2(multiSelectRect.Left, multiSelectRect.Top + TimeLineHeight),
                        //    sourceRectangle: null,
                        //    color: MultiSelectRectFillColor,
                        //    rotation: 0,
                        //    origin: Vector2.Zero,
                        //    scale: new Vector2(multiSelectRect.Width, multiSelectRect.Height),
                        //    effects: SpriteEffects.None,
                        //    layerDepth: 0.01f
                        //    );

                        ImGuiDebugDrawer.DrawRect(multiSelectRect.SubtractScroll(ScrollViewer.RoundedScroll), MultiSelectRectFillColor);

                        ImGuiDebugDrawer.DrawLine(multiSelectRect.TopLeftCorner() - ScrollViewer.RoundedScroll,
                            multiSelectRect.TopRightCorner() - ScrollViewer.RoundedScroll,
                            MultiSelectRectOutlineColor);

                        ImGuiDebugDrawer.DrawLine(multiSelectRect.TopRightCorner() - ScrollViewer.RoundedScroll,
                            multiSelectRect.BottomRightCorner() - ScrollViewer.RoundedScroll,
                            MultiSelectRectOutlineColor);

                        ImGuiDebugDrawer.DrawLine(multiSelectRect.BottomRightCorner() - ScrollViewer.RoundedScroll,
                            multiSelectRect.BottomLeftCorner() - ScrollViewer.RoundedScroll,
                            MultiSelectRectOutlineColor);

                        ImGuiDebugDrawer.DrawLine(multiSelectRect.BottomLeftCorner() - ScrollViewer.RoundedScroll,
                            multiSelectRect.TopLeftCorner() - ScrollViewer.RoundedScroll,
                            MultiSelectRectOutlineColor);
                    }
                    else if (currentDrag.DragType == BoxDragType.SliceTool)
                    {
                        //sb.Draw(texture: boxTex,
                        //    position: new Vector2(currentDrag.StartDragPoint.X - (MultiSelectRectOutlineThickness / 2f), Math.Min(currentDrag.StartDragPoint.Y + TimeLineHeight, relMouse.Y + TimeLineHeight)),
                        //    sourceRectangle: null,
                        //    color: Main.Colors.GuiColorEventGraphSliceToolLine,
                        //    rotation: 0,
                        //    origin: Vector2.Zero,
                        //    scale: new Vector2(MultiSelectRectOutlineThickness, Math.Abs(relMouse.Y - currentDrag.StartDragPoint.Y)),
                        //    effects: SpriteEffects.None,
                        //    layerDepth: 0
                        //    );

                        ImGuiDebugDrawer.DrawLine(new Vector2(currentDrag.CurrentDragPoint.X - ScrollViewer.RoundedScroll.X, currentDrag.StartDragPoint.Y - ScrollViewer.RoundedScroll.Y + TimeLineHeight),
                            new Vector2(currentDrag.CurrentDragPoint.X - ScrollViewer.RoundedScroll.X, relMouse.Y - ScrollViewer.RoundedScroll.Y + TimeLineHeight),
                            Main.Colors.GuiColorEventGraphSliceToolLine);
                    }

                    // Draw PlaybackCursor StartTime vertical line
                    //sb.Draw(texture: boxTex,
                    //    position: new Vector2((float)Math.Round(((float)(SecondsPixelSize * PlaybackCursor.GUIStartTime))), ScrollViewer.Scroll.Y),
                    //    sourceRectangle: null,
                    //    color: Main.Colors.GuiColorEventGraphPlaybackStartTime,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(PlaybackCursorThickness, ScrollViewerRect.Height),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );

                    ImGuiDebugDrawer.DrawLine(new Vector2(MathF.Round(((float)(SecondsPixelSize * PlaybackCursor.GUIStartTime))) - ScrollViewer.RoundedScroll.X, 0),
                        new Vector2(MathF.Round(((float)(SecondsPixelSize * PlaybackCursor.GUIStartTime))) - ScrollViewer.RoundedScroll.X, ScrollViewerRect.Height),
                        Main.Colors.GuiColorEventGraphPlaybackStartTime);





                    //var hitWindowStartX = (float)(SecondsPixelSize * PlaybackCursor.HitWindowStart);
                    //var hitWindowEndX = (float)(SecondsPixelSize * PlaybackCursor.HitWindowEnd);

                    //// Draw Hit Window Rect Under Cursor
                    //sb.Draw(texture: boxTex,
                    //    position: new Vector2((float)Math.Round(hitWindowStartX), ScrollViewer.Scroll.Y),
                    //    sourceRectangle: null,
                    //    color: PlaybackCursorHitWindowColor,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(Math.Max(hitWindowEndX - hitWindowStartX, 0), Rect.Height),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );

                    //// Draw Hit Window Line
                    //sb.Draw(texture: boxTex,
                    //    position: new Vector2((hitWindowStartX + hitWindowEndX) / 2, ScrollViewer.Scroll.Y),
                    //    sourceRectangle: null,
                    //    color: PlaybackCursorHitWindowStartColor,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(1, Rect.Height),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );


                    //// Draw Hit Register Playback Cursor
                    //sb.Draw(texture: boxTex,
                    //    position: new Vector2((float)Math.Round(SecondsPixelSize * PlaybackCursor.CurrentTime), ScrollViewer.Scroll.Y),
                    //    sourceRectangle: null,
                    //    color: PlaybackCursorHitWindowColor,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(1, Rect.Height),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );


                    var playbackCursorPixelX = (float)Math.Round(SecondsPixelSize * PlaybackCursor.GUICurrentTime);

                    // Draw PlaybackCursor CurrentTime vertical line
                    //sb.Draw(texture: boxTex,
                    //    position: new Vector2(playbackCursorPixelX - (PlaybackCursorThickness / 2), ScrollViewer.Scroll.Y),
                    //    sourceRectangle: null,
                    //    color: PlaybackCursorColor,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(PlaybackCursorThickness, ScrollViewerRect.Height),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );

                    ImGuiDebugDrawer.DrawLine(new Vector2(playbackCursorPixelX - ScrollViewer.RoundedScroll.X, 0),
                        new Vector2(playbackCursorPixelX - ScrollViewer.RoundedScroll.X, ScrollViewerRect.Height),
                        PlaybackCursorColor);

                    var playbackCursorPixelXMod = (float)Math.Round((SecondsPixelSize * (float)(PlaybackCursor.IsPlaying
                        ? PlaybackCursor.CurrentTimeMod : PlaybackCursor.GUICurrentTimeMod)));

                    // Draw PlaybackCursor CurrentTimeMod vertical line
                    //sb.Draw(texture: boxTex,
                    //    position: new Vector2(playbackCursorPixelXMod - (PlaybackCursorThickness / 2), ScrollViewer.Scroll.Y),
                    //    sourceRectangle: null,
                    //    color: PlaybackCursorColor * 0.25f,
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: new Vector2(PlaybackCursorThickness, ScrollViewerRect.Height),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );

                    ImGuiDebugDrawer.DrawLine(new Vector2(playbackCursorPixelXMod - ScrollViewer.RoundedScroll.X, 0),
                        new Vector2(playbackCursorPixelXMod - ScrollViewer.RoundedScroll.X, ScrollViewerRect.Height),
                        PlaybackCursorColor * 0.25f);


                    //var playbackCursorSmoothPixelX = (PlaybackCursor.IsPlaying || PlaybackCursor.Scrubbing) ?
                    //    (float)(SecondsPixelSize * PlaybackCursor.CurrentTime) : playbackCursorPixelX;

                    //Vector2 playbackCursorTextSize = font.MeasureString(playbackCursorText);

                    //playbackCursorTextSize = new Vector2(playbackCursorTextSize.X, TimeLineHeight - 1);

                    // Draw PlaybackCursor CurrentTime BG Rect

                    //sb.Draw(texture: boxTex,
                    //   position: new Vector2(playbackCursorSmoothPixelX + (PlaybackCursorThickness / 2) + 1, 
                    //   (float)Math.Round(ScrollViewer.Scroll.Y + 2)),
                    //   sourceRectangle: null,
                    //   color: Color.Black,
                    //   rotation: 0,
                    //   origin: Vector2.Zero,
                    //   scale: playbackCursorTextSize + new Vector2(10, -4),
                    //   effects: SpriteEffects.None,
                    //   layerDepth: 0
                    //   );

                    //sb.Draw(texture: boxTex,
                    //    position: new Vector2(playbackCursorSmoothPixelX + (PlaybackCursorThickness / 2) + 1, 
                    //    (float)Math.Round(ScrollViewer.Scroll.Y + 2)) + Vector2.One,
                    //    sourceRectangle: null,
                    //    color: new Color(64, 64, 64, 255),
                    //    rotation: 0,
                    //    origin: Vector2.Zero,
                    //    scale: playbackCursorTextSize + new Vector2(10,-4) - (Vector2.One * 2),
                    //    effects: SpriteEffects.None,
                    //    layerDepth: 0
                    //    );

                    //Vector2 playbackCursorTextPos = new Vector2(
                    //    playbackCursorSmoothPixelX + (PlaybackCursorThickness / 2) + 6, 
                    //    (float)Math.Round(ScrollViewer.Scroll.Y + 2));

                    //// Draw PlaybackCursor CurrentTime string
                    //sb.DrawString(font, playbackCursorText,
                    //    position: playbackCursorTextPos + Vector2.One + Main.GlobalTaeEditorFontOffset,
                    //    color: Color.Black
                    //    );

                    //sb.DrawString(font, playbackCursorText,
                    //    position: playbackCursorTextPos + Main.GlobalTaeEditorFontOffset,
                    //    color: Color.Cyan
                    //    );

                    if (darkenedPortionWidth > 0)
                    {
                        // full bg - anim duration

                        float cooldownRatio = 1;// 1 - Math.Max(0, Math.Min(1, MathHelper.Lerp(0, 1, MainScreen.AnimSwitchRenderCooldown / MainScreen.AnimSwitchRenderCooldownFadeLength)));

                        //sb.Draw(texture: boxTex,
                        //      position: new Vector2(animStopPixelX, (float)Math.Round(ScrollViewer.Scroll.Y)),
                        //      sourceRectangle: null,
                        //      color: Main.Colors.GuiColorEventGraphAnimEndDarkenRect * cooldownRatio,
                        //      rotation: 0,
                        //      origin: Vector2.Zero,
                        //      scale: new Vector2(darkenedPortionWidth, ScrollViewer.Viewport.Height),
                        //      effects: SpriteEffects.None,
                        //      layerDepth: 0
                        //      );

                        //sb.Draw(texture: boxTex,
                        //      position: new Vector2(animStopPixelX - 1, (float)Math.Round(ScrollViewer.Scroll.Y)),
                        //      sourceRectangle: null,
                        //      color: Main.Colors.GuiColorEventGraphAnimEndVerticalLine * cooldownRatio,
                        //      rotation: 0,
                        //      origin: Vector2.Zero,
                        //      scale: new Vector2(2, TimeLineHeight),
                        //      effects: SpriteEffects.None,
                        //      layerDepth: 0
                        //      );

                        ImGuiDebugDrawer.DrawRect(new Vector2(animStopPixelX - ScrollViewer.RoundedScroll.X, 0),
                            new Vector2(darkenedPortionWidth, ScrollViewer.Viewport.Height),
                            Main.Colors.GuiColorEventGraphAnimEndDarkenRect * cooldownRatio);

                        ImGuiDebugDrawer.DrawLine(new Vector2(animStopPixelX - ScrollViewer.RoundedScroll.X, 0),
                            new Vector2(animStopPixelX - ScrollViewer.RoundedScroll.X, ScrollViewer.Viewport.Height),
                            Main.Colors.GuiColorEventGraphAnimEndVerticalLine * cooldownRatio);
                    }
                }
                finally { sb.End(); }
            }

            if (MainScreen.Config.ShowEventHoverInfo)
            {
                gd.Viewport = new Viewport(MainScreen.Rect.DpiScaled());

                sb.Begin(transformMatrix: scrollMatrix * Main.DPIMatrix);
                try
                {
                    if (!ScrollViewerRect.Contains(MainScreen.Input.MousePositionPoint))
                        HoverInfoBox.Update(mouseInside: false, elapsedSeconds);

                    if (!HoverInfoBox.IsVisible)
                    {
                        //HoverInfoBox.DrawPosition = relMouse +
                        //    new Vector2(Rect.X + HoverInfoBoxOffsetFromMouse.X,
                        //    Rect.Y + HoverInfoBoxOffsetFromMouse.Y + TimeLineHeight);

                        HoverInfoBox.DrawPosition = (MainScreen.Input.MousePosition + new Vector2(16, 40)) * Main.DPIVector;

                        //MainScreen.SetInspectorVisibility(true);
                    }
                    else
                    {
                        //MainScreen.SetInspectorVisibility(false);
                    }

                    bool cancelTooltipCompletely = (MainScreen.Input.LeftClickHeld || MainScreen.Input.MiddleClickHeld ||
                        MainScreen.Input.RightClickHeld || !ScrollViewerRect.Contains(MainScreen.Input.MousePositionPoint));

                    if (cancelTooltipCompletely)
                    {
                        HoverInfoBox.Update(false, 0);
                    }

                    //HoverInfoBox.Draw(sb, boxTex);
                    HoverInfoBox.UpdateTooltip(MainScreen.GameWindowAsForm);
                }
                finally { sb.End(); }
            }

            gd.Viewport = oldViewport;
        }
    }


}
