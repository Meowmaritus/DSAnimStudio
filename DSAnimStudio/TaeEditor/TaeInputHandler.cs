using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public enum MouseCursorType
    {
        StopUpdating,
        Arrow,
        DragX,
        DragY,
        DragXY,
        GrabPan,
    }

    public class TaeInputHandler
    {
        private Rectangle MouseCursorUpdateRect;

        private MouseState Mouse;
        private KeyboardState Keyboard;

        public MouseCursorType CursorType;

        public bool LeftClickDown;
        public bool LeftClickUp;
        public bool LeftClickHeld;

        public bool RightClickDown;
        public bool RightClickUp;
        public bool RightClickHeld;

        public bool MiddleClickDown;
        public bool MiddleClickUp;
        public bool MiddleClickHeld;

        public float AccumulatedScroll;
        public float ScrollDelta;

        public Vector2 MousePosition;
        public Point MousePositionPoint;
        public Vector2 MousePositionDelta;
        public Point MousePositionDeltaPoint;

        public Vector2 LeftClickDownAnchor;
        public Vector2 RightClickDownAnchor;
        public Vector2 MiddleClickDownAnchor;

        public Vector2 LeftClickDownOffset;
        public Vector2 RightClickDownOffset;
        public Vector2 MiddleClickDownOffset;

        private static bool oldLeftClickHeld;
        private static bool oldRightClickHeld;
        private static bool oldMiddleClickHeld;
        private static float oldAccumulatedScroll;
        private static Vector2 oldMousePosition;

        private List<Keys> keysToKeepTrackOf = new List<Keys>();
        private Dictionary<Keys, bool> keysHeld = new Dictionary<Keys, bool>();
        private Dictionary<Keys, bool> keysDown = new Dictionary<Keys, bool>();
        private Dictionary<Keys, bool> keysUp = new Dictionary<Keys, bool>();
        private Dictionary<Keys, bool> oldKeysHeld = new Dictionary<Keys, bool>();

        private void KeepTrackOfNewKey(Keys k)
        {
            keysToKeepTrackOf.Add(k);
            keysHeld.Add(k, false);
            keysDown.Add(k, false);
            keysUp.Add(k, false);
            oldKeysHeld.Add(k, false);
        }

        public bool KeyHeld(Keys k)
        {
            if (!keysToKeepTrackOf.Contains(k))
                KeepTrackOfNewKey(k);

            return keysHeld[k];
        }

        public bool KeyDown(Keys k)
        {
            if (!keysToKeepTrackOf.Contains(k))
                KeepTrackOfNewKey(k);

            return keysDown[k];
        }

        public bool KeyUp(Keys k)
        {
            if (!keysToKeepTrackOf.Contains(k))
                KeepTrackOfNewKey(k);

            return keysUp[k];
        }

        public void Update(Rectangle mouseCursorUpdateRect)
        {
            Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();

            MouseCursorUpdateRect = mouseCursorUpdateRect;

            var inUpdateCursorRect = MouseCursorUpdateRect.Contains(Mouse.Position);
            var prevInUpdateCursorRect = MouseCursorUpdateRect.Contains(new Point((int)oldMousePosition.X, (int)oldMousePosition.Y));

            if (inUpdateCursorRect)
            {
                switch (CursorType)
                {
                    case MouseCursorType.Arrow:
                        Microsoft.Xna.Framework.Input.Mouse.SetCursor(MouseCursor.Arrow);
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
                        break;
                    case MouseCursorType.DragX:
                        Microsoft.Xna.Framework.Input.Mouse.SetCursor(MouseCursor.SizeWE);
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
                        break;
                    case MouseCursorType.DragY:
                        Microsoft.Xna.Framework.Input.Mouse.SetCursor(MouseCursor.SizeNS);
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
                        break;
                    case MouseCursorType.DragXY:
                        Microsoft.Xna.Framework.Input.Mouse.SetCursor(MouseCursor.SizeAll);
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
                        break;
                    case MouseCursorType.GrabPan:
                        Microsoft.Xna.Framework.Input.Mouse.SetCursor(MouseCursor.Crosshair);
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
                        break;
                }
            }
            else if (prevInUpdateCursorRect)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
            }

            Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            LeftClickHeld = Mouse.LeftButton == ButtonState.Pressed;
            RightClickHeld = Mouse.RightButton == ButtonState.Pressed;
            MiddleClickHeld = Mouse.MiddleButton == ButtonState.Pressed;
            AccumulatedScroll = Mouse.ScrollWheelValue / 150f;
            MousePosition = new Vector2(Mouse.X, Mouse.Y);
            MousePositionPoint = Mouse.Position;

            LeftClickDown = LeftClickHeld && !oldLeftClickHeld;
            LeftClickUp = !LeftClickHeld && oldLeftClickHeld;

            RightClickDown = RightClickHeld && !oldRightClickHeld;
            RightClickUp = !RightClickHeld && oldRightClickHeld;

            MiddleClickDown = MiddleClickHeld && !oldMiddleClickHeld;
            MiddleClickUp = !MiddleClickHeld && oldMiddleClickHeld;

            ScrollDelta = AccumulatedScroll - oldAccumulatedScroll;

            MousePositionDelta = MousePosition - oldMousePosition;
            MousePositionDeltaPoint = new Point((int)MousePositionDelta.X, (int)MousePositionDelta.Y);

            if (LeftClickDown)
                LeftClickDownAnchor = MousePosition;

            if (RightClickDown)
                RightClickDownAnchor = MousePosition;

            if (MiddleClickDown)
                MiddleClickDownAnchor = MousePosition;

            if (LeftClickHeld)
                LeftClickDownOffset = MousePosition - LeftClickDownAnchor;

            if (RightClickHeld)
                RightClickDownOffset = MousePosition - RightClickDownAnchor;

            if (MiddleClickHeld)
                MiddleClickDownOffset = MousePosition - MiddleClickDownAnchor;

            foreach (var k in keysToKeepTrackOf)
            {
                keysHeld[k] = Keyboard.IsKeyDown(k);

                keysDown[k] = keysHeld[k] && !oldKeysHeld[k];
                keysUp[k] = !keysHeld[k] && oldKeysHeld[k];

                oldKeysHeld[k] = keysHeld[k];
            }


            oldLeftClickHeld = LeftClickHeld;
            oldRightClickHeld = RightClickHeld;
            oldMiddleClickHeld = MiddleClickHeld;
            oldAccumulatedScroll = AccumulatedScroll;
            oldMousePosition = MousePosition;
        }
    }
}
