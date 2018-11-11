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

        private void ZoomOutOneNotch(float mouseScreenPosX)
        {
            float mousePointTime = (mouseScreenPosX + ScrollViewer.Scroll.X) / SecondsPixelSize;

            if (SecondsPixelSize <= SecondsPixelSizeFarAwayModeUpperBound)
            {
                SecondsPixelSize /= 2f;
                if (SecondsPixelSize < 8f)
                    SecondsPixelSize = 8f;
            }
            else
            {
                SecondsPixelSize -= SecondsPixelSizeScrollNotch;
            }

            float newOnscreenOffset = (mousePointTime * SecondsPixelSize) - ScrollViewer.Scroll.X;
            float scrollAmountToCorrectOffset = (newOnscreenOffset - mouseScreenPosX);
            ScrollViewer.ScrollByVirtualScrollUnits(new Vector2(scrollAmountToCorrectOffset, 0));
        }

        private void ZoomInOneNotch(float mouseScreenPosX)
        {
            float mousePointTime = (mouseScreenPosX + ScrollViewer.Scroll.X) / SecondsPixelSize;

            if (SecondsPixelSize < SecondsPixelSizeFarAwayModeUpperBound)
            {
                SecondsPixelSize *= 2;
            }
            else
            {
                SecondsPixelSize += SecondsPixelSizeScrollNotch;
            }

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

        public void ChangeToNewAnimRef(AnimationRef newAnimRef)
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

            int currentRow = 0;
            float farthestRightOnCurrentRow = 0;
            foreach (var ev in AnimRef.Anim.EventList)
            {
                var newBox = new TaeEditAnimEventBox(this, ev);
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
            if (!sortedByRow.ContainsKey(box.Row))
                sortedByRow.Add(box.Row, new List<TaeEditAnimEventBox>());
            if (!sortedByRow[box.Row].Contains(box))
                sortedByRow[box.Row].Add(box);
        }

        public void DeleteEventBox(TaeEditAnimEventBox box)
        {
            if (box.OwnerPane != this)
            {
                throw new ArgumentException($"This {nameof(TaeEditAnimEventGraph)} can only " +
                    $"delete {nameof(TaeEditAnimEventBox)}'s that it owns!", nameof(box));
            }
            if (MainScreen.SelectedEventBox == box)
                MainScreen.SelectedEventBox = null;
            box.RowChanged -= Box_RowChanged;
            AnimRef.Anim.EventList.Remove(box.MyEvent);
            EventBoxes.Remove(box);
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

        public void Update(float elapsedSeconds, bool allowMouseUpdate)
        {
            if (!allowMouseUpdate)
                return;

            var isZooming = MainScreen.Input.KeyHeld(Keys.LeftControl) || MainScreen.Input.KeyHeld(Keys.RightControl);

            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: !isZooming);

            if (ScrollViewer.Viewport.Contains(new Point((int)MainScreen.Input.MousePosition.X, (int)MainScreen.Input.MousePosition.Y)))
            {
                MouseRow = (int)(relMouse.Y / RowHeight);

                MainScreen.Input.CursorType = MouseCursorType.Arrow;

                if (MainScreen.Input.LeftClickDown)
                {
                    MainScreen.SelectedEventBox = null;
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
                        //currentDragBox.DragLeftSide(MainScreen.Input.MousePositionDelta.X);
                    }
                    else if (currentDragType == BoxDragType.Right && currentDragBox != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.DragX;
                        DragCurrentDragBoxToMouse(relMouse.ToPoint());
                        //currentDragBox.DragRightSide(MainScreen.Input.MousePositionDelta.X);
                    }
                    else if (currentDragType == BoxDragType.Middle && currentDragBox != null)
                    {
                        MainScreen.Input.CursorType = MouseCursorType.Arrow;
                        DragCurrentDragBoxToMouse(relMouse.ToPoint());
                        //currentDragBox.DragMiddle(MainScreen.Input.MousePositionDelta.X);
                        if (MouseRow >= 0)
                            currentDragBox.Row = MouseRow;
                    }
                }
                else
                {
                    currentDragType = BoxDragType.None;
                    currentDragBox = null;
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
                sb.DrawString(font, $"{(kvp.Key * 30)}", new Vector2(kvp.Value + 4 + 1, ScrollViewer.Scroll.Y + 1 + 1), Color.Black);
                sb.DrawString(font, $"{(kvp.Key * 30)}", new Vector2(kvp.Value + 4, ScrollViewer.Scroll.Y + 1), Color.White);
            }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
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

                        Vector2 pos = new Vector2((int)box.LeftFr, (int)box.Top);
                        Vector2 size = new Vector2(box.WidthFr, box.HeightFr);

                        sb.Draw(texture: boxTex,
                            position: pos + new Vector2(0, -1),
                            sourceRectangle: null,
                            color: Color.DarkBlue,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: size + new Vector2(0, 2),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );

                        sb.Draw(texture: boxTex,
                            position: pos + new Vector2(1, 0),
                            sourceRectangle: null,
                            color: (MainScreen.SelectedEventBox == box) ? Color.CornflowerBlue : box.BGColor,
                            rotation: 0,
                            origin: Vector2.Zero,
                            scale: size + new Vector2(-2, 0),
                            effects: SpriteEffects.None,
                            layerDepth: 0
                            );

                        var nameSize = font.MeasureString(box.EventText);
                        if (nameSize.X <= box.Width)
                        {
                            sb.DrawString(font, box.EventText, pos + new Vector2(2 + 1, 1), Color.Black);
                            sb.DrawString(font, box.EventText, pos + new Vector2(2, 0), Color.White);
                        }
                        else
                        {
                            sb.DrawString(font, $"{((int)box.MyEvent.EventType)}", pos + (Vector2.One), Color.Black, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
                            sb.DrawString(font, $"{((int)box.MyEvent.EventType)}", pos, Color.White, 0, Vector2.Zero, 0.75f, SpriteEffects.None, 0);
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
