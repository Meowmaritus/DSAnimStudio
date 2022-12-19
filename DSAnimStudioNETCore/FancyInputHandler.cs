using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DSAnimStudio
{
    public enum MouseCursorType
    {
        StopUpdating,
        Arrow,
        DragX,
        DragY,
        DragXY,
        GrabPan,
        DragBottomLeftResize
    }

    public class FancyInputHandler
    {
        private Rectangle MouseCursorUpdateRect;

        private MouseState Mouse;
        private KeyboardState Keyboard;

        public MouseCursorType CursorType;

        public bool ClickStartedOutsideWindow = false;
        public bool IsIgnoringMouseButtonsUntilAllAreUp = false;

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

        public bool ShiftHeld => KeyHeld(Keys.LeftShift) || KeyHeld(Keys.RightShift);
        public bool ShiftDown => KeyDown(Keys.LeftShift) || KeyDown(Keys.RightShift);
        public bool ShiftUp => KeyUp(Keys.LeftShift) || KeyUp(Keys.RightShift);

        public bool CtrlHeld => KeyHeld(Keys.LeftControl) || KeyHeld(Keys.RightControl);
        public bool CtrlDown => KeyDown(Keys.LeftControl) || KeyDown(Keys.RightControl);
        public bool CtrlUp => KeyUp(Keys.LeftControl) || KeyUp(Keys.RightControl);

        public bool AltHeld => KeyHeld(Keys.LeftAlt) || KeyHeld(Keys.RightAlt);
        public bool AltDown => KeyDown(Keys.LeftAlt) || KeyDown(Keys.RightAlt);
        public bool AltUp => KeyUp(Keys.LeftAlt) || KeyUp(Keys.RightAlt);

        public bool AnyModifiersHeld => CtrlHeld || ShiftHeld || AltHeld;

        public bool CtrlOnlyHeld => CtrlHeld && !ShiftHeld && !AltHeld;
        public bool ShiftOnlyHeld => !CtrlHeld && ShiftHeld && !AltHeld;
        public bool AltOnlyHeld => !CtrlHeld && !ShiftHeld && AltHeld;

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

        private void UpdateKeyboard()
        {
            Keyboard = GlobalInputState.Keyboard;

            foreach (var k in keysToKeepTrackOf)
            {
                ////////////////////////////////////////////////////////////////////////////////
                // Get the current state.
                ////////////////////////////////////////////////////////////////////////////////
                keysHeld[k] = Keyboard.IsKeyDown(k);

                ////////////////////////////////////////////////////////////////////////////////
                // Cancel delta state when you alt+tab or click back into window.
                ////////////////////////////////////////////////////////////////////////////////
                if (Main.IsFirstFrameActive)
                {
                    oldKeysHeld[k] = keysHeld[k];
                }

                ////////////////////////////////////////////////////////////////////////////////
                // Get the delta state.
                ////////////////////////////////////////////////////////////////////////////////
                keysDown[k] = keysHeld[k] && !oldKeysHeld[k];
                keysUp[k] = !keysHeld[k] && oldKeysHeld[k];

                ////////////////////////////////////////////////////////////////////////////////
                // Store current state for getting the next delta state. 
                ////////////////////////////////////////////////////////////////////////////////
                oldKeysHeld[k] = keysHeld[k];
            }
        }

        private bool IsMouseLocationInWindow(Vector2 location)
        {
            return (location.X >= 0 && location.Y >= 0 && location.X < Main.WinForm.Width && location.Y < Main.WinForm.Height);
        }

        private void UpdateMouse(Rectangle mouseCursorUpdateRect)
        {
            Mouse = GlobalInputState.Mouse;

            MouseCursorUpdateRect = mouseCursorUpdateRect;//.DpiScaled();

            

            ////////////////////////////////////////////////////////////////////////////////
            // Get the current state.
            ////////////////////////////////////////////////////////////////////////////////
            
            LeftClickHeld = Mouse.LeftButton == ButtonState.Pressed;
            RightClickHeld = Mouse.RightButton == ButtonState.Pressed;
            MiddleClickHeld = Mouse.MiddleButton == ButtonState.Pressed;

            if (Main.IsFirstFrameActive || ImguiOSD.OSD.Hovered)
            {
                IsIgnoringMouseButtonsUntilAllAreUp = true;
            }
            else if (IsIgnoringMouseButtonsUntilAllAreUp)
            {
                if (!LeftClickHeld && !MiddleClickHeld && !RightClickHeld)
                    IsIgnoringMouseButtonsUntilAllAreUp = false;

                LeftClickHeld = false;
                MiddleClickHeld = false;
                RightClickHeld = false;
            }

            AccumulatedScroll = Mouse.ScrollWheelValue / 150f;
            MousePosition = new Vector2(Mouse.X / Main.DPIX, Mouse.Y / Main.DPIY);
            MousePositionPoint = new Point((int)Math.Round(Mouse.Position.X / Main.DPIX), (int)Math.Round(Mouse.Position.Y / Main.DPIY));

            var inUpdateCursorRect = MouseCursorUpdateRect.Contains(MousePositionPoint);
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
                    case MouseCursorType.DragBottomLeftResize:
                        Microsoft.Xna.Framework.Input.Mouse.SetCursor(MouseCursor.SizeNESW);
                        //System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.SizeWE;
                        break;
                    case MouseCursorType.StopUpdating:
                        return;
                    default:
                        throw new NotImplementedException($"Mouse cursor type {CursorType} not implemented.");
                }
            }
            else if (prevInUpdateCursorRect)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Cancel delta state when you alt+tab or click back into window.
            ////////////////////////////////////////////////////////////////////////////////

            if (Main.IsFirstFrameActive)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;

                oldAccumulatedScroll = AccumulatedScroll;
                oldLeftClickHeld = LeftClickHeld;
                oldMiddleClickHeld = MiddleClickHeld;
                oldMousePosition = MousePosition;
                oldRightClickHeld = RightClickHeld;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Get the delta state.
            ////////////////////////////////////////////////////////////////////////////////

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

            ClickStartedOutsideWindow = (LeftClickHeld && !IsMouseLocationInWindow(LeftClickDownAnchor * Main.DPIVector))
                || (MiddleClickHeld && !IsMouseLocationInWindow(MiddleClickDownAnchor * Main.DPIVector))
                || (RightClickHeld && !IsMouseLocationInWindow(RightClickDownAnchor * Main.DPIVector));

            if (ClickStartedOutsideWindow)
            {
                LeftClickHeld = false;
                LeftClickDown = false;
                MiddleClickHeld = false;
                MiddleClickDown = false;
                RightClickHeld = false;
                RightClickDown = false;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Store current state for getting the next delta state.
            ////////////////////////////////////////////////////////////////////////////////

            oldLeftClickHeld = LeftClickHeld;
            oldRightClickHeld = RightClickHeld;
            oldMiddleClickHeld = MiddleClickHeld;
            oldAccumulatedScroll = AccumulatedScroll;
            oldMousePosition = MousePosition;
        }

        public void Update(Rectangle mouseCursorUpdateRect)
        {
            UpdateMouse(mouseCursorUpdateRect);
            UpdateKeyboard();
        }
    }
}
