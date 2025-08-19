using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

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
        DragBottomLeftResize,
        Loading,
    }

    public class FancyInputHandler
    {
        private void DebugPrint(string str)
        {
            zzz_NotificationManagerIns.PushNotification($"INPUT: {str}", 0.5f, 0.1f, Color.Orange);
        }
        
        private Rectangle MouseCursorUpdateRect;

        private MouseState Mouse;
        private KeyboardState Keyboard;

        public MouseCursorType CursorType;

        public bool ClickStartedOutsideWindow = false;
        public bool IsIgnoringMouseButtonsUntilAllAreUp = false;



        public bool LeftClickDown;
        public Vector2 LeftClickDownAnchor;
        public Vector2 LeftClickDownOffset;
        public bool LeftClickUp;
        public bool LeftClickHeld;
        private bool oldLeftClickHeld;

        public bool MiddleClickDown;
        public Vector2 MiddleClickDownAnchor;
        public Vector2 MiddleClickDownOffset;
        public bool MiddleClickUp;
        public bool MiddleClickHeld;
        private bool oldMiddleClickHeld;

        public bool RightClickDown;
        public Vector2 RightClickDownAnchor;
        public Vector2 RightClickDownOffset;
        public bool RightClickUp;
        public bool RightClickHeld;
        private bool oldRightClickHeld;

        public bool XButton1Down;
        public Vector2 XButton1DownAnchor;
        public Vector2 XButton1DownOffset;
        public bool XButton1Up;
        public bool XButton1Held;
        private bool oldXButton1Held;

        public bool XButton2Down;
        public Vector2 XButton2DownAnchor;
        public Vector2 XButton2DownOffset;
        public bool XButton2Up;
        public bool XButton2Held;
        private bool oldXButton2Held;
        
        public bool AnyMouseButtonDown;
        public Vector2 AnyMouseButtonDownAnchor;
        public Vector2 AnyMouseButtonDownOffset;
        public bool AnyMouseButtonUp;
        public bool AnyMouseButtonHeld;
        private bool oldAnyMouseButtonHeld;




        public float AccumulatedScroll;
        public float ScrollDelta;
        private float oldAccumulatedScroll;



        public Vector2 MousePosition;
        public Point MousePositionPoint;
        public Vector2 MousePositionDelta;
        public Point MousePositionDeltaPoint;
        private Vector2 oldMousePosition;











        
        

        private List<Keys> keysToKeepTrackOf = new List<Keys>();
        private Dictionary<Keys, bool> keysHeld = new Dictionary<Keys, bool>();
        private Dictionary<Keys, bool> keysDown = new Dictionary<Keys, bool>();
        private Dictionary<Keys, bool> keysUp = new Dictionary<Keys, bool>();
        private Dictionary<Keys, bool> oldKeysHeld = new Dictionary<Keys, bool>();

        public bool ShiftHeld
        {
            get
            {
                var state = KeyHeld(Keys.LeftShift) || KeyHeld(Keys.RightShift);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(ShiftHeld)}==TRUE");
                return state;
            }
        }
        public bool ShiftDown
        {
            get
            {
                var state = KeyDown(Keys.LeftShift) || KeyDown(Keys.RightShift);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(ShiftDown)}==TRUE");
                return state;
            }
        }
        public bool ShiftUp
        {
            get
            {
                var state = KeyUp(Keys.LeftShift) || KeyUp(Keys.RightShift);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(ShiftUp)}==TRUE");
                return state;
            }
        }

        public bool CtrlHeld
        {
            get
            {
                var state = KeyHeld(Keys.LeftControl) || KeyHeld(Keys.RightControl);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(CtrlHeld)}==TRUE");
                return state;
            }
        }
        public bool CtrlDown
        {
            get
            {
                var state = KeyDown(Keys.LeftControl) || KeyDown(Keys.RightControl);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(CtrlDown)}==TRUE");
                return state;
            }
        }
        public bool CtrlUp
        {
            get
            {
                var state = KeyUp(Keys.LeftControl) || KeyUp(Keys.RightControl);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(CtrlUp)}==TRUE");
                return state;
            }
        }

        public bool AltHeld
        {
            get
            {
                var state = KeyHeld(Keys.LeftAlt) || KeyHeld(Keys.RightAlt);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(AltHeld)}==TRUE");
                return state;
            }
        }
        public bool AltDown
        {
            get
            {
                var state = KeyDown(Keys.LeftAlt) || KeyDown(Keys.RightAlt);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(AltDown)}==TRUE");
                return state;
            }
        }
        public bool AltUp
        {
            get
            {
                var state = KeyUp(Keys.LeftAlt) || KeyUp(Keys.RightAlt);
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(AltUp)}==TRUE");
                return state;
            }
        }

        public bool AnyModifiersHeld
        {
            get
            {
                var state = CtrlHeld || ShiftHeld || AltHeld;
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(AnyModifiersHeld)}==TRUE");
                return state;
            }
        }

        public bool CtrlOnlyHeld
        {
            get
            {
                var state = CtrlHeld && !ShiftHeld && !AltHeld;
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(CtrlOnlyHeld)}==TRUE");
                return state;
            }
        }
        public bool ShiftOnlyHeld
        {
            get
            {
                var state = !CtrlHeld && ShiftHeld && !AltHeld;
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(ShiftOnlyHeld)}==TRUE");
                return state;
            }
        }
        public bool AltOnlyHeld
        {
            get
            {
                var state = !CtrlHeld && !ShiftHeld && AltHeld;
                if (state && Main.Debug.InputPrintTrueKeys)
                    DebugPrint($"{nameof(AltOnlyHeld)}==TRUE");
                return state;
            }
        }

        private void KeepTrackOfNewKey(Keys k)
        {
            keysToKeepTrackOf.Add(k);
            keysHeld.Add(k, false);
            keysDown.Add(k, false);
            keysUp.Add(k, false);
            oldKeysHeld.Add(k, false);
        }

        public bool KeyHeldNoMods(Keys k)
        {
            var state = KeyHeld(k) && !CtrlHeld && !ShiftHeld && !AltHeld;
            if (state && Main.Debug.InputPrintTrueKeys)
                DebugPrint($"{nameof(KeyHeldNoMods)}({k})==TRUE");
            return state;
        }

        public bool KeyDownNoMods(Keys k)
        {
            var state = KeyDown(k) && !CtrlHeld && !ShiftHeld && !AltHeld;
            if (state && Main.Debug.InputPrintTrueKeys)
                DebugPrint($"{nameof(KeyDownNoMods)}({k})==TRUE");
            return state;
        }

        public bool KeyUpNoMods(Keys k)
        {
            var state = KeyUp(k) && !CtrlHeld && !ShiftHeld && !AltHeld;
            if (state && Main.Debug.InputPrintTrueKeys)
                DebugPrint($"{nameof(KeyUpNoMods)}({k})==TRUE");
            return state;
        }

        public bool KeyHeld(Keys k)
        {
            if (!keysToKeepTrackOf.Contains(k))
                KeepTrackOfNewKey(k);

            var state = keysHeld[k];
            if (state && Main.Debug.InputPrintTrueKeys)
                DebugPrint($"{nameof(KeyHeld)}({k})==TRUE");
            return state;
        }

        public bool KeyDown(Keys k)
        {
            if (!keysToKeepTrackOf.Contains(k))
                KeepTrackOfNewKey(k);

            var state = keysDown[k];
            if (state && (Main.Debug.InputPrintTrueKeys || Main.Debug.InputPrintTrueKeys_DownOnly))
                DebugPrint($"{nameof(KeyDown)}({k})==TRUE");
            return state;
        }

        public bool KeyUp(Keys k)
        {
            if (!keysToKeepTrackOf.Contains(k))
                KeepTrackOfNewKey(k);

            var state = keysUp[k];
            if (state && Main.Debug.InputPrintTrueKeys || Main.Debug.InputPrintTrueKeys_UpOnly && (k != Keys.Left && k != Keys.Right))
                DebugPrint($"{nameof(KeyUp)}({k})==TRUE");
            return state;
        }

        private void UpdateKeyboard()
        {
            Keyboard = GlobalInputState.Keyboard;

            foreach (var k in keysToKeepTrackOf)
            {
                // Push current to old
                //oldKeysHeld[k] = keysHeld[k];

                ////////////////////////////////////////////////////////////////////////////////
                // Get the current state.
                ////////////////////////////////////////////////////////////////////////////////
                keysHeld[k] = Keyboard.IsKeyDown(k);

                ////////////////////////////////////////////////////////////////////////////////
                // Cancel delta state when you alt+tab or click back into window.
                ////////////////////////////////////////////////////////////////////////////////
                if (Main.IsFirstFrameActive)
                {
                    //keysHeld[k] = oldKeysHeld[k];
                    oldKeysHeld[k] = keysHeld[k];
                }

                ////////////////////////////////////////////////////////////////////////////////
                // Get the delta state.
                ////////////////////////////////////////////////////////////////////////////////
                keysDown[k] |= keysHeld[k] && !oldKeysHeld[k];
                keysUp[k] |= !keysHeld[k] && oldKeysHeld[k];
            }
        }

        private bool IsMouseLocationInWindow(Vector2 location)
        {
            return (location.X >= 0 && location.Y >= 0 && location.X < Main.WinForm.Width && location.Y < Main.WinForm.Height);
        }

        private void UpdateMouse(Rectangle mouseCursorUpdateRect, bool forceUpdate)
        {
            Mouse = GlobalInputState.Mouse;

            MouseCursorUpdateRect = mouseCursorUpdateRect;//.DpiScaled();



            ////////////////////////////////////////////////////////////////////////////////
            // Get the current state.
            ////////////////////////////////////////////////////////////////////////////////

            LeftClickHeld = Mouse.LeftButton == ButtonState.Pressed;
            MiddleClickHeld = Mouse.MiddleButton == ButtonState.Pressed;
            RightClickHeld = Mouse.RightButton == ButtonState.Pressed;
            XButton1Held = Mouse.XButton1 == ButtonState.Pressed;
            XButton2Held = Mouse.XButton2 == ButtonState.Pressed;
            //Any
            AnyMouseButtonHeld = LeftClickHeld || MiddleClickHeld || RightClickHeld || XButton1Held || XButton2Held;

            if (!AnyMouseButtonHeld)
            {
                UnlockMouseCursor();
            }


            if (Main.IsFirstFrameActive || ImguiOSD.OSD.AnyFieldFocused)
            {
                IsIgnoringMouseButtonsUntilAllAreUp = true;
            }
            
            if (!forceUpdate && IsIgnoringMouseButtonsUntilAllAreUp)
            {
                if (!LeftClickHeld && !MiddleClickHeld && !RightClickHeld && !XButton1Held && !XButton2Held)
                    IsIgnoringMouseButtonsUntilAllAreUp = false;

                LeftClickHeld = false;
                MiddleClickHeld = false;
                RightClickHeld = false;
                XButton1Held = false;
                XButton2Held = false;
                AnyMouseButtonHeld = false;
            }

            AccumulatedScroll = Mouse.ScrollWheelValue / 150f;
            MousePosition = new Vector2(Mouse.X / Main.DPI, Mouse.Y / Main.DPI);
            MousePositionPoint = new Point((int)Math.Round(Mouse.Position.X / Main.DPI), (int)Math.Round(Mouse.Position.Y / Main.DPI));

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
                    case MouseCursorType.Loading:
                        Microsoft.Xna.Framework.Input.Mouse.SetCursor(MouseCursor.WaitArrow);
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

                // Cancel out delta states
                oldLeftClickHeld = LeftClickHeld;
                oldMiddleClickHeld = MiddleClickHeld;
                oldRightClickHeld = RightClickHeld;
                oldXButton1Held = XButton1Held;
                oldXButton2Held = XButton2Held;
                oldAnyMouseButtonHeld = AnyMouseButtonHeld;

                oldAccumulatedScroll = AccumulatedScroll;
                oldMousePosition = MousePosition;
            }

            ////////////////////////////////////////////////////////////////////////////////
            // Get the delta state.
            ////////////////////////////////////////////////////////////////////////////////

            LeftClickDown |= LeftClickHeld && !oldLeftClickHeld;
            LeftClickUp |= !LeftClickHeld && oldLeftClickHeld;

            MiddleClickDown |= MiddleClickHeld && !oldMiddleClickHeld;
            MiddleClickUp |= !MiddleClickHeld && oldMiddleClickHeld;

            RightClickDown |= RightClickHeld && !oldRightClickHeld;
            RightClickUp |= !RightClickHeld && oldRightClickHeld;

            XButton1Down |= XButton1Held && !oldXButton1Held;
            XButton1Up |= !XButton1Held && oldXButton1Held;

            XButton2Down |= XButton2Held && !oldXButton2Held;
            XButton2Up |= !XButton2Held && oldXButton2Held;

            AnyMouseButtonDown |= AnyMouseButtonHeld && !oldAnyMouseButtonHeld;
            AnyMouseButtonUp |= !AnyMouseButtonHeld && oldAnyMouseButtonHeld;

            ScrollDelta = AccumulatedScroll - oldAccumulatedScroll;

            MousePositionDelta = MousePosition - oldMousePosition;
            MousePositionDeltaPoint = new Point((int)MousePositionDelta.X, (int)MousePositionDelta.Y);


            if (LeftClickDown)
                LeftClickDownAnchor = MousePosition;

            if (MiddleClickDown)
                MiddleClickDownAnchor = MousePosition;

            if (RightClickDown)
                RightClickDownAnchor = MousePosition;

            if (XButton1Down)
                XButton1DownAnchor = MousePosition;

            if (XButton2Down)
                XButton2DownAnchor = MousePosition;

            if (AnyMouseButtonDown)
                AnyMouseButtonDownAnchor = MousePosition;


            if (LeftClickHeld)
                LeftClickDownOffset = MousePosition - LeftClickDownAnchor;

            if (MiddleClickHeld)
                MiddleClickDownOffset = MousePosition - MiddleClickDownAnchor;

            if (RightClickHeld)
                RightClickDownOffset = MousePosition - RightClickDownAnchor;

            if (XButton1Held)
                XButton1DownOffset = MousePosition - XButton1DownAnchor;

            if (XButton2Held)
                XButton2DownOffset = MousePosition - XButton2DownAnchor;

            if (AnyMouseButtonHeld)
                AnyMouseButtonDownOffset = MousePosition - AnyMouseButtonDownAnchor;



            ClickStartedOutsideWindow = (LeftClickHeld && !IsMouseLocationInWindow(LeftClickDownAnchor * Main.DPIVector))
                || (MiddleClickHeld && !IsMouseLocationInWindow(MiddleClickDownAnchor * Main.DPIVector))
                || (RightClickHeld && !IsMouseLocationInWindow(RightClickDownAnchor * Main.DPIVector))
                || (XButton1Held && !IsMouseLocationInWindow(XButton1DownAnchor * Main.DPIVector))
                || (XButton2Held && !IsMouseLocationInWindow(XButton2DownAnchor * Main.DPIVector));

            if (ClickStartedOutsideWindow)
            {
                LeftClickHeld = false;
                LeftClickDown = false;
                MiddleClickHeld = false;
                MiddleClickDown = false;
                RightClickHeld = false;
                RightClickDown = false;
                XButton1Held = false;
                XButton1Down = false;
                XButton2Held = false;
                XButton2Down = false;
                AnyMouseButtonHeld = false;
                AnyMouseButtonDown = false;
            }

            //MEME
            //if (!Main.ActiveHyst)
            //{
            //    LeftClickHeld = false;
            //    LeftClickDown = false;
            //    LeftClickUp = false;
            //    MiddleClickHeld = false;
            //    MiddleClickDown = false;
            //    MiddleClickUp = false;
            //    RightClickHeld = false;
            //    RightClickDown = false;
            //    RightClickUp = false;
            //    XButton1Held = false;
            //    XButton1Down = false;
            //    XButton1Up = false;
            //    XButton2Held = false;
            //    XButton2Down = false;
            //    XButton2Up = false;
            //    AnyMouseButtonHeld = false;
            //    AnyMouseButtonDown = false;
            //    AnyMouseButtonUp = false;

            //    ScrollDelta = 0;
            //    foreach (var k in keysToKeepTrackOf)
            //    {
            //        keysHeld[k] = false;
            //        keysDown[k] = false;
            //        keysUp[k] = false;
            //        oldKeysHeld[k] = false;
            //    }
            //    this.

            //    return;
            //}

            ////////////////////////////////////////////////////////////////////////////////
            // Store current state for getting the next delta state.
            ////////////////////////////////////////////////////////////////////////////////

            //oldLeftClickHeld = LeftClickHeld;
            //oldMiddleClickHeld = MiddleClickHeld;
            //oldRightClickHeld = RightClickHeld;
            //oldXButton1Held = XButton1Held;
            //oldXButton2Held = XButton2Held;
            //oldAnyMouseButtonHeld = AnyMouseButtonHeld;
            
            //oldAccumulatedScroll = AccumulatedScroll;
            //oldMousePosition = MousePosition;
        }

        public void UpdateEmergencyMouseUnlock()
        {
            if (framesMouseLocked > 0)
            {
                framesMouseLocked--;
                if (framesMouseLocked <= 0 && MouseCursorLocked)
                {
                    UnlockMouseCursor();
                }
            }
        }

        public string GetDebugReport(bool includeMouse, bool includeKeyboard)
        {
            var sb = new StringBuilder();
            if (includeMouse)
            {
                sb.AppendLine($"LC:【{(LeftClickHeld ? "○" : "　")}{(LeftClickUp ? "↑" : "　")}{(LeftClickDown ? "↓" : "　")}】");
                sb.AppendLine($"MC:【{(MiddleClickHeld ? "○" : "　")}{(MiddleClickUp ? "↑" : "　")}{(MiddleClickDown ? "↓" : "　")}】");
                sb.AppendLine($"RC:【{(RightClickHeld ? "○" : "　")}{(RightClickUp ? "↑" : "　")}{(RightClickDown ? "↓" : "　")}】");
                sb.AppendLine($"X1:【{(XButton1Held ? "○" : "　")}{(XButton1Up ? "↑" : "　")}{(XButton1Down ? "↓" : "　")}】");
                sb.AppendLine($"X2:【{(XButton2Held ? "○" : "　")}{(XButton2Up ? "↑" : "　")}{(XButton2Down ? "↓" : "　")}】");
            }
            return sb.ToString();
        }

        public void PreUpdate()
        {
            oldLeftClickHeld = LeftClickHeld;
            oldMiddleClickHeld = MiddleClickHeld;
            oldRightClickHeld = RightClickHeld;
            oldXButton1Held = XButton1Held;
            oldXButton2Held = XButton2Held;
            oldAnyMouseButtonHeld = AnyMouseButtonHeld;

            oldAccumulatedScroll = AccumulatedScroll;
            oldMousePosition = MousePosition;

            // Clear down and up
            LeftClickUp = LeftClickDown = false;
            MiddleClickUp = MiddleClickDown = false;
            RightClickUp = RightClickDown = false;
            XButton1Up = XButton1Down = false;
            XButton2Up = XButton2Down = false;
            AnyMouseButtonUp = AnyMouseButtonDown = false;

            foreach (var k in keysToKeepTrackOf)
            {
                // Push current to old
                oldKeysHeld[k] = keysHeld[k];

                //Clear down and up 
                keysDown[k] = keysUp[k] = false;
            }

            

        }

        public void Update(Rectangle mouseCursorUpdateRect, bool forceUpdate, bool disableIfFieldsFocused)
        {


            if (!Main.ActiveHyst || (ImguiOSD.OSD.AnyFieldFocused && disableIfFieldsFocused))
            {
                LeftClickHeld = oldLeftClickHeld = LeftClickDown = LeftClickUp = false;
                MiddleClickHeld = oldMiddleClickHeld = MiddleClickDown = MiddleClickUp = false;
                RightClickHeld = oldRightClickHeld = RightClickDown = RightClickUp = false;
                XButton1Held = oldXButton1Held = XButton1Down = XButton1Up = false;
                XButton2Held = oldXButton2Held = XButton2Down = XButton2Up = false;
                //Any
                AnyMouseButtonHeld = oldAnyMouseButtonHeld = AnyMouseButtonDown = AnyMouseButtonUp = false;
            }
            UpdateMouse(mouseCursorUpdateRect, forceUpdate);
            UpdateKeyboard();
        }

        public bool MouseCursorLocked { get; private set; }
        private System.Drawing.Rectangle oldCursorClipRect;

        private object _lock_CursorLock = new object();

        int framesMouseLocked = 0;

        public void LockMouseCursor(int x, int y)
        {
            lock (_lock_CursorLock)
            {
                framesMouseLocked = 10; 
                if (!MouseCursorLocked)
                    oldCursorClipRect = System.Windows.Forms.Cursor.Clip;
                var winBounds = Main.LastBounds;
                System.Windows.Forms.Cursor.Clip = new System.Drawing.Rectangle(winBounds.X + x, winBounds.Y + y, 1, 1);

                Microsoft.Xna.Framework.Input.Mouse.SetPosition(x, y);
                MouseCursorLocked = true;
            }
        }

        public void UnlockMouseCursor()
        {
            lock (_lock_CursorLock)
            {
                framesMouseLocked = 0;
                if (MouseCursorLocked)
                {
                    System.Windows.Forms.Cursor.Clip = oldCursorClipRect;
                }

                MouseCursorLocked = false;
            }
        }
    }
}
