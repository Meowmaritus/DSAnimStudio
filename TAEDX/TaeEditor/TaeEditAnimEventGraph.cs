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
            Left,
            Right,
            Middle,
        }

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

        TaeScrollViewer ScrollViewer = new TaeScrollViewer();

        public float SecondsPixelSize = 128 * 4;
        public float SecondsPixelSizeFarAwayModeUpperBound = 128;
        public float SecondsPixelSizeMax = 128 * 400;
        public float SecondsPixelSizeScrollNotch = 128;

        public float TimeLineHeight = 24;

        public float BoxSideScrollMarginSize = 4;

        private BoxDragType currentDragType = BoxDragType.None;
        private TaeEditAnimEventBox currentDragBox = null;
        private Point currentDragOffset = Point.Zero;
        private float currentDragBoxOriginalWidth = 0;
        private float currentDragBoxOriginalStart = 0;
        private float currentDragBoxOriginalEnd = 0;
        private int currentDragBoxOriginalRow = 0;

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
            foreach (var ev in AnimRef.Anim.EventList)
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
            ChangeToNewAnimRef(MainScreen.TaeAnim);
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

                    AnimRef.Anim.EventList.Remove(box.MyEvent);

                    EventBoxes.Remove(box);

                    AnimRef.IsModified = true;
                    MainScreen.IsModified = true;
                },
                undoAction: () =>
                {
                    EventBoxes.Add(box);
                    AnimRef.Anim.EventList.Add(box.MyEvent);

                    if (!sortedByRow.ContainsKey(box.MyEvent.Row))
                        sortedByRow.Add(box.MyEvent.Row, new List<TaeEditAnimEventBox>());

                    if (!sortedByRow[box.MyEvent.Row].Contains(box))
                        sortedByRow[box.MyEvent.Row].Add(box);

                    box.RowChanged += Box_RowChanged;

                    MainScreen.SelectedEventBox = box;
                });
        }

        private void DragCurrentDragBoxToMouse(Point mouse)
        {
            if (currentDragType == BoxDragType.Middle)
            {
                currentDragBox.DragWholeBoxToVirtualUnitX(mouse.X - currentDragOffset.X);
            }
            else if (currentDragType == BoxDragType.Left)
            {
                currentDragBox.DragLeftSideOfBoxToVirtualUnitX(mouse.X - currentDragOffset.X);
            }
            else if (currentDragType == BoxDragType.Right)
            {
                currentDragBox.DragRightSideOfBoxToVirtualUnitX(mouse.X - currentDragOffset.X + currentDragBoxOriginalWidth);
            }
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

            newEvent.Index = MainScreen.TaeAnim.Anim.EventList.Count;

            var newBox = new TaeEditAnimEventBox(this, newEvent);

            newBox.MyEvent.Row = MouseRow;

            MainScreen.UndoMan.NewAction(
                doAction: () =>
                {
                    MainScreen.TaeAnim.Anim.EventList.Add(newEvent);

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

                    MainScreen.TaeAnim.Anim.EventList.Remove(newEvent);
                });

            
        }

        private void DeleteSelectedEvent()
        {
            var selectedEventBox = MainScreen.SelectedEventBox;

            //var yesNo = System.Windows.Forms.MessageBox.Show(
            //    "Would you really like to delete the selected event?" +
            //    " This cannot be undone.", "Delete Event?", 
            //    System.Windows.Forms.MessageBoxButtons.YesNo, 
            //    System.Windows.Forms.MessageBoxIcon.Question);

            //if (yesNo != System.Windows.Forms.DialogResult.Yes)
            //{
            //    return;
            //}

            DeleteEventBox(selectedEventBox);
        }

        public void Update(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            var isZooming = MainScreen.Input.KeyHeld(Keys.LeftControl) || MainScreen.Input.KeyHeld(Keys.RightControl);

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: !isZooming);

            if (MainScreen.SelectedEventBox != null && MainScreen.Input.KeyDown(Keys.Delete))
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

                if (MainScreen.Input.RightClickDown)
                {
                    PlaceNewEventAtMouse();
                }

                foreach (var box in GetRow(MouseRow))
                {
                    if (MainScreen.Input.LeftClickDown)
                    {
                        if (relMouse.X >= box.LeftFr && relMouse.X < box.RightFr)
                            MainScreen.SelectedEventBox = box;
                    }

                    if (currentDragType == BoxDragType.None && box.WidthFr >= 16)
                    {
                        if (relMouse.X <= box.LeftFr + BoxSideScrollMarginSize && relMouse.X >= box.LeftFr - BoxSideScrollMarginSize)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            if (MainScreen.Input.LeftClickDown)
                            {
                                currentDragType = BoxDragType.Left;
                                currentDragBox = box;
                                currentDragOffset = new Point((int)(relMouse.X - box.LeftFr), (int)(relMouse.Y - box.Top));
                                currentDragBoxOriginalWidth = box.Width;
                                currentDragBoxOriginalStart = box.MyEvent.StartTime;
                                currentDragBoxOriginalEnd = box.MyEvent.EndTime;
                                currentDragBoxOriginalRow = box.MyEvent.Row;
                            }
                        }
                        else if (relMouse.X >= box.RightFr - BoxSideScrollMarginSize && relMouse.X <= box.RightFr + BoxSideScrollMarginSize)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.DragX;
                            if (MainScreen.Input.LeftClickDown)
                            {
                                currentDragType = BoxDragType.Right;
                                currentDragBox = box;
                                currentDragOffset = new Point((int)(relMouse.X - box.LeftFr), (int)(relMouse.Y - box.Top));
                                currentDragBoxOriginalWidth = box.Width;
                                currentDragBoxOriginalStart = box.MyEvent.StartTime;
                                currentDragBoxOriginalEnd = box.MyEvent.EndTime;
                                currentDragBoxOriginalRow = box.MyEvent.Row;
                            }
                        }
                        else if (relMouse.X >= box.LeftFr && relMouse.X < box.RightFr)
                        {
                            MainScreen.Input.CursorType = MouseCursorType.Arrow;
                            if (MainScreen.Input.LeftClickDown)
                            {
                                currentDragType = BoxDragType.Middle;
                                currentDragBox = box;
                                currentDragOffset = new Point((int)(relMouse.X - box.LeftFr), (int)(relMouse.Y - box.Top));
                                currentDragBoxOriginalWidth = box.Width;
                                currentDragBoxOriginalStart = box.MyEvent.StartTime;
                                currentDragBoxOriginalEnd = box.MyEvent.EndTime;
                                currentDragBoxOriginalRow = box.MyEvent.Row;
                            }
                        }
                    }

                }

                if (MainScreen.Input.LeftClickHeld)
                {
                    if (currentDragType == BoxDragType.Left && currentDragBox != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.DragX;
                        DragCurrentDragBoxToMouse(relMouse.ToPoint());
                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                        //currentDragBox.DragLeftSide(MainScreen.Input.MousePositionDelta.X);
                    }
                    else if (currentDragType == BoxDragType.Right && currentDragBox != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.DragX;
                        DragCurrentDragBoxToMouse(relMouse.ToPoint());
                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                        //currentDragBox.DragRightSide(MainScreen.Input.MousePositionDelta.X);
                    }
                    else if (currentDragType == BoxDragType.Middle && currentDragBox != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.Arrow;
                        DragCurrentDragBoxToMouse(relMouse.ToPoint());
                        AnimRef.IsModified = true;
                        MainScreen.IsModified = true;
                        //currentDragBox.DragMiddle(MainScreen.Input.MousePositionDelta.X);
                        if (MouseRow >= 0)
                            currentDragBox.MyEvent.Row = MouseRow;
                    }
                }
                else
                {
                    if (currentDragType != BoxDragType.None)
                    {
                        TaeEditAnimEventBox copyOfBox = currentDragBox;

                        float copyOfOldBoxStart = currentDragBoxOriginalStart;
                        float copyOfOldBoxEnd = currentDragBoxOriginalEnd;
                        int copyOfOldBoxRow = currentDragBoxOriginalRow;

                        float copyOfCurrentBoxStart = copyOfBox.MyEvent.StartTime;
                        float copyOfCurrentBoxEnd = copyOfBox.MyEvent.EndTime;
                        int copyOfCurrentBoxRow = copyOfBox.MyEvent.Row;

                        MainScreen.UndoMan.NewAction(
                            doAction: () =>
                            {
                                copyOfBox.MyEvent.StartTime = copyOfCurrentBoxStart;
                                copyOfBox.MyEvent.EndTime = copyOfCurrentBoxEnd;
                                copyOfBox.MyEvent.Row = copyOfCurrentBoxRow;
                            },
                            undoAction: () =>
                            {
                                copyOfBox.MyEvent.StartTime = copyOfOldBoxStart;
                                copyOfBox.MyEvent.EndTime = copyOfOldBoxEnd;
                                copyOfBox.MyEvent.Row = copyOfOldBoxRow;
                            });

                        currentDragType = BoxDragType.None;
                        currentDragBox = null;
                    }
                }

                if (isZooming)
                {
                    Zoom(MainScreen.Input.ScrollDelta, MainScreen.Input.MousePosition.X - Rect.X);
                }
            }

            
        }

        public void UpdateMouseOutsideRect(float elapsedSeconds, bool allowMouseUpdate)
        {
            MouseRow = -1;
            if (!allowMouseUpdate)
                return;

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
                sb.DrawString(font, $"{(kvp.Key * 30)}", new Vector2(kvp.Value + 4 + 1, ScrollViewer.Scroll.Y + 1 + 1), Color.Black);
                sb.DrawString(font, $"{(kvp.Key * 30)}", new Vector2(kvp.Value + 4, ScrollViewer.Scroll.Y + 1), Color.White);
            }
        }

        public void Draw(GameTime gt, GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            ScrollViewer.SetDisplayRect(Rect, GetVirtualAreaSize());

            ScrollViewer.Draw(gd, sb, boxTex, font);

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport);
            {
                sb.Begin(transformMatrix: ScrollViewer.GetScrollMatrix());

                sb.Draw(texture: boxTex,
                    position: ScrollViewer.Scroll,
                    sourceRectangle: null,
                    color: Color.DimGray,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(ScrollViewer.Viewport.Width, ScrollViewer.Viewport.Height),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );

                foreach (var kvp in sortedByRow)
                {
                    foreach (var box in kvp.Value)
                    {
                        if (box == MainScreen.SelectedEventBox)
                            box.UpdateEventText();

                        if (box.LeftFr > ScrollViewer.RelativeViewport.Right || box.RightFr < ScrollViewer.RelativeViewport.Left)
                            continue;

                        bool eventStartsBeforeScreen = box.LeftFr < ScrollViewer.RelativeViewport.Left;

                        Vector2 pos = new Vector2((int)box.LeftFr, (int)box.Top);
                        Vector2 size = new Vector2(box.WidthFr, box.HeightFr);

                        sb.Draw(texture: boxTex,
                            position: pos + new Vector2(0, -1),
                            sourceRectangle: null,
                            color: box.ColorOutline,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: size + new Vector2(0, 2),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );

                        sb.Draw(texture: boxTex,
                            position: pos + new Vector2(1, 0),
                            sourceRectangle: null,
                            color: (MainScreen.SelectedEventBox == box) ? TaeMiscUtils.GetPastelRainbow((float)gt.TotalGameTime.TotalSeconds / 4) : box.ColorBG,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: size + new Vector2(-2, 0),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );

                        var namePos = new Vector2((int)MathHelper.Max(ScrollViewer.Scroll.X, pos.X), pos.Y);

                        var nameSize = font.MeasureString(box.EventText);

                        if ((namePos.X + nameSize.X) <= box.RightFr)
                        {
                            sb.DrawString(font, $"{(eventStartsBeforeScreen ? "<-- " : "")}{box.EventText}", namePos + new Vector2(2 + 1, 1), box.ColorOutline);
                            sb.DrawString(font, $"{(eventStartsBeforeScreen ? "<-- " : "")}{box.EventText}", namePos + new Vector2(2, 0), box.ColorFG);
                        }
                        else
                        {
                            sb.DrawString(font, $"{(eventStartsBeforeScreen ? "<-- " : "")}{((int)box.MyEvent.EventType)}", namePos + (Vector2.One), box.ColorOutline, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
                            sb.DrawString(font, $"{(eventStartsBeforeScreen ? "<-- " : "")}{((int)box.MyEvent.EventType)}", namePos, box.ColorFG, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
                        }
                    }
                }

                sb.Draw(texture: boxTex,
                            position: new Vector2(ScrollViewer.Scroll.X, TimeLineHeight + MouseRow * RowHeight),
                            sourceRectangle: null,
                            color: Color.LightGray * 0.25f,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: new Vector2(ScrollViewer.Viewport.Width, RowHeight),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );


                sb.Draw(texture: boxTex,
                       position: ScrollViewer.Scroll,
                       sourceRectangle: null,
                       color: Color.DarkGray,
                       rotation: 0,
                       origin: Vector2.Zero,
                       scale: new Vector2(ScrollViewer.Viewport.Width, TimeLineHeight),
                       effects: SpriteEffects.None,
                       layerDepth: 0
                       );

                var rowHorizontalLineYPositions = GetRowHorizontalLineYPositions();

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
                        color: Color.Black * 0.5f,
                        rotation: 0,
                        origin: Vector2.Zero,
                        scale: new Vector2(1, ScrollViewer.Viewport.Height),
                        effects: SpriteEffects.None,
                        layerDepth: 0
                        );
                    }
                }

                foreach (var kvp in rowHorizontalLineYPositions)
                {
                    sb.Draw(texture: boxTex,
                    position: new Vector2(ScrollViewer.Scroll.X, kvp.Value),
                    sourceRectangle: null,
                    color: Color.Black * 0.5f,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(ScrollViewer.Viewport.Width, 1),
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
