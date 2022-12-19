using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DSAnimStudio.TaeEditor
{
    public class TaeScrollViewer
    {
        enum ScrollbarType
        {
            None,
            Horizontal,
            Vertical,
        }

        public bool DisableHorizontalScroll = false;
        public bool DisableVerticalScroll = false;

        //private string ScrollArrowStr = "▲";

        private Point VirtualAreaSize;
        private Rectangle FullDisplayRect;
        private Rectangle ViewportDisplayRect;

        public Rectangle Viewport => ViewportDisplayRect;

        public Rectangle RelativeViewport => new Rectangle((int)Scroll.X, (int)Scroll.Y, ViewportDisplayRect.Width, ViewportDisplayRect.Height);

        public Vector2 Scroll;

        public Vector2 RoundedScroll => new Vector2(MathF.Round(Scroll.X), MathF.Round(Scroll.Y));

        private Vector2 MaxScroll;

        private Rectangle HorizontalBoxValidArea;
        private Rectangle HorizontalBox;
        private Point HorizontalBoxGrabOffset;

        private Rectangle VerticalBoxValidArea;
        private Rectangle VerticalBox;
        private Point VerticalBoxGrabOffset;

        public int ScrollWheelVirtualScrollAmountX = 200; //was 128
        public int ScrollWheelVirtualScrollAmountY = 100; //was 32

        public int ScrollArrowButtonVirtualScrollAmountX = 64;
        public int ScrollArrowButtonVirtualScrollAmountY = 32;

        private Rectangle ScrollLeftArrowBox;
        private Rectangle ScrollRightArrowBox;
        private Rectangle ScrollUpArrowBox;
        private Rectangle ScrollDownArrowBox;

        private TaeButtonRepeater ScrollUpButton = new TaeButtonRepeater(0.25f, 0.075f);
        private TaeButtonRepeater ScrollDownButton = new TaeButtonRepeater(0.25f, 0.075f);
        private TaeButtonRepeater ScrollLeftButton = new TaeButtonRepeater(0.25f, 0.075f);
        private TaeButtonRepeater ScrollRightButton = new TaeButtonRepeater(0.25f, 0.075f);

        private bool ScrollUpButtonHeld = false;
        private bool ScrollDownButtonHeld = false;
        private bool ScrollLeftButtonHeld = false;
        private bool ScrollRightButtonHeld = false;

        private ScrollbarType CurrentScroll = ScrollbarType.None;

        public int ScrollBarThickness = 16;

        //private void MoveHorizontalBarByPixelAmount(int pixelAmt)
        //{
        //    HorizontalBox.X = (int)MathHelper.Clamp(
        //        HorizontalBox.X + pixelAmt, 
        //        HorizontalBoxValidArea.Left, 
        //        HorizontalBoxValidArea.Right - HorizontalBox.Width);
        //}

        //private void MoveVerticalBarByPixelAmount(int pixelAmt)
        //{
        //    VerticalBox.Y = (int)MathHelper.Clamp(
        //        VerticalBox.Y + pixelAmt, 
        //        VerticalBoxValidArea.Top, 
        //        VerticalBoxValidArea.Bottom - VerticalBox.Height);
        //}

        private void CopyHorizontalBarPosToScroll()
        {
            Scroll.X = (1.0f * (HorizontalBox.X - HorizontalBoxValidArea.X) 
                / (HorizontalBoxValidArea.Width - HorizontalBox.Width)) 
                * MaxScroll.X;
        }

        private void CopyVerticalBarPosToScroll()
        {
            Scroll.Y = (1.0f * (VerticalBox.Y - VerticalBoxValidArea.Y) 
                / (VerticalBoxValidArea.Height - VerticalBox.Height)) 
                * MaxScroll.Y;
        }

        private void ScrollHorizontalBarToMouse(Point mouse)
        {
            HorizontalBox.X = (int)MathHelper.Clamp(
                mouse.X - HorizontalBoxGrabOffset.X,
                HorizontalBoxValidArea.Left,
                HorizontalBoxValidArea.Right - HorizontalBox.Width);

            CopyHorizontalBarPosToScroll();
        }

        private void ScrollVerticalBarToMouse(Point mouse)
        {
            VerticalBox.Y = (int)MathHelper.Clamp(
                mouse.Y - VerticalBoxGrabOffset.Y,
                VerticalBoxValidArea.Top,
                VerticalBoxValidArea.Bottom - VerticalBox.Height);

            CopyVerticalBarPosToScroll();
        }

        //private void ScrollByPixels(Vector2 pixelScroll)
        //{
        //    if (pixelScroll.X != 0)
        //    {
        //        MoveHorizontalBarByPixelAmount((int)Math.Round(pixelScroll.X));
        //        CopyHorizontalBarPosToScroll();
        //    }

        //    if (pixelScroll.Y != 0)
        //    {
        //        MoveVerticalBarByPixelAmount((int)Math.Round(pixelScroll.Y));
        //        CopyVerticalBarPosToScroll();
        //    }
        //}

        public void ScrollByVirtualScrollUnits(Vector2 virtualScroll)
        {
            if (virtualScroll.X != 0)
            {
                Scroll.X += virtualScroll.X;

                ClampScroll();

                HorizontalBox = new Rectangle((int)(HorizontalBoxValidArea.X +
                (Scroll.X / MaxScroll.X) * (HorizontalBoxValidArea.Width - HorizontalBox.Width)),
                HorizontalBoxValidArea.Y, HorizontalBox.Width, HorizontalBoxValidArea.Height);
            }

            if (virtualScroll.Y != 0)
            {
                Scroll.Y += virtualScroll.Y;

                ClampScroll();

                VerticalBox = new Rectangle(VerticalBoxValidArea.X, (int)(VerticalBoxValidArea.Y +
                   (Scroll.Y / MaxScroll.Y) * (VerticalBoxValidArea.Height - VerticalBox.Height)), VerticalBoxValidArea.Width, VerticalBox.Height);
            }
        }

        public void SetDisplayRect(Rectangle dispRect, Point virtualAreaSize)
        {
            FullDisplayRect = dispRect;
            VirtualAreaSize = new Point(virtualAreaSize.X - ScrollBarThickness, virtualAreaSize.Y - ScrollBarThickness);

            DisableHorizontalScroll = VirtualAreaSize.X < FullDisplayRect.Width;
            DisableVerticalScroll = VirtualAreaSize.Y < FullDisplayRect.Height;

            ViewportDisplayRect = new Rectangle(FullDisplayRect.X, FullDisplayRect.Y, 
                FullDisplayRect.Width - (DisableVerticalScroll ? 0 : ScrollBarThickness),
                FullDisplayRect.Height - (DisableHorizontalScroll ? 0 : ScrollBarThickness));

            MaxScroll = new Vector2(DisableHorizontalScroll ? 0 : VirtualAreaSize.X - ViewportDisplayRect.Width,
                DisableVerticalScroll ? 0 : VirtualAreaSize.Y - ViewportDisplayRect.Height);

            if (MaxScroll.X < 0)
                MaxScroll.X = 0;

            if (MaxScroll.Y < 0)
                MaxScroll.Y = 0;

            HorizontalBoxValidArea = new Rectangle(FullDisplayRect.X + ScrollBarThickness, 
                FullDisplayRect.Bottom - ScrollBarThickness,
                FullDisplayRect.Width - (ScrollBarThickness * 2) - (DisableVerticalScroll ? 0 : ScrollBarThickness), 
                ScrollBarThickness);

            VerticalBoxValidArea = new Rectangle(FullDisplayRect.X + FullDisplayRect.Width - ScrollBarThickness, 
                FullDisplayRect.Y + ScrollBarThickness, 
                ScrollBarThickness,
                FullDisplayRect.Height - (ScrollBarThickness * 2) - (DisableHorizontalScroll ? 0 : ScrollBarThickness));

            ClampScroll();

            int hBoxLength = (int)Math.Max((HorizontalBoxValidArea.Width * (1.0f * ViewportDisplayRect.Width / VirtualAreaSize.X)), ScrollBarThickness);
            int vBoxLength = (int)Math.Max((VerticalBoxValidArea.Height * (1.0f * ViewportDisplayRect.Height / VirtualAreaSize.Y)), ScrollBarThickness);

            HorizontalBox = new Rectangle((int)(HorizontalBoxValidArea.X +
                (Scroll.X / MaxScroll.X) * (HorizontalBoxValidArea.Width - hBoxLength)),
                HorizontalBoxValidArea.Y, hBoxLength, HorizontalBoxValidArea.Height);

            VerticalBox = new Rectangle(VerticalBoxValidArea.X, (int)(VerticalBoxValidArea.Y +
                (Scroll.Y / MaxScroll.Y) * (VerticalBoxValidArea.Height - vBoxLength)), VerticalBoxValidArea.Width, vBoxLength);

            ScrollLeftArrowBox = new Rectangle(
                FullDisplayRect.Left, 
                HorizontalBoxValidArea.Top, 
                ScrollBarThickness, 
                ScrollBarThickness);

            ScrollRightArrowBox = new Rectangle(
                FullDisplayRect.Right - (ScrollBarThickness) - (DisableVerticalScroll ? 0 : ScrollBarThickness), 
                HorizontalBoxValidArea.Top, 
                ScrollBarThickness, 
                ScrollBarThickness);

            ScrollUpArrowBox = new Rectangle(
                FullDisplayRect.Right - ScrollBarThickness, 
                FullDisplayRect.Top, 
                ScrollBarThickness, 
                ScrollBarThickness);

            ScrollDownArrowBox = new Rectangle(
                FullDisplayRect.Right - ScrollBarThickness, 
                FullDisplayRect.Bottom - (ScrollBarThickness) - (DisableHorizontalScroll ? 0 : ScrollBarThickness), 
                ScrollBarThickness, 
                ScrollBarThickness);
        }

        public void ClampScroll()
        {
            Scroll.X = MathHelper.Clamp(Scroll.X, 0, MaxScroll.X);
            Scroll.Y = MathHelper.Clamp(Scroll.Y, 0, MaxScroll.Y);
        }

        public Matrix GetScrollMatrix()
        {
            return Matrix.CreateTranslation(-(float)Math.Round(Scroll.X), -(float)Math.Round(Scroll.Y), 0);
        }

        public void UpdateInput(FancyInputHandler input, float elapsedSeconds, bool allowScrollWheel)
        {
            var relMouse = new Point((int)(input.MousePosition.X),
                (int)(input.MousePosition.Y));

            if (CurrentScroll == ScrollbarType.None && input.LeftClickDown)
            {
                if (!DisableHorizontalScroll && HorizontalBox.Contains(relMouse))
                {
                    CurrentScroll = ScrollbarType.Horizontal;
                    HorizontalBoxGrabOffset = new Point(relMouse.X - HorizontalBox.X, relMouse.Y - HorizontalBox.Y);
                }
                else if (!DisableVerticalScroll && VerticalBox.Contains(relMouse))
                {
                    CurrentScroll = ScrollbarType.Vertical;
                    VerticalBoxGrabOffset = new Point(relMouse.X - VerticalBox.X, relMouse.Y - VerticalBox.Y);
                }
            }
            else if (CurrentScroll == ScrollbarType.Horizontal)
            {
                //ScrollByPixels(new Vector2(input.MousePositionDelta.X, 0));
                ScrollHorizontalBarToMouse(relMouse);
            }
            else if (CurrentScroll == ScrollbarType.Vertical)
            {
                //ScrollByPixels(new Vector2(0, input.MousePositionDelta.Y));
                ScrollVerticalBarToMouse(relMouse);
            }

            if (!input.LeftClickHeld)
            {
                CurrentScroll = ScrollbarType.None;
            }

            ScrollUpButtonHeld = CurrentScroll == ScrollbarType.None && !DisableVerticalScroll && input.LeftClickHeld && ScrollUpArrowBox.Contains(relMouse);
            ScrollDownButtonHeld = CurrentScroll == ScrollbarType.None && !DisableVerticalScroll && input.LeftClickHeld && ScrollDownArrowBox.Contains(relMouse);
            ScrollLeftButtonHeld = CurrentScroll == ScrollbarType.None && !DisableHorizontalScroll && input.LeftClickHeld && ScrollLeftArrowBox.Contains(relMouse);
            ScrollRightButtonHeld = CurrentScroll == ScrollbarType.None && !DisableHorizontalScroll && input.LeftClickHeld && ScrollRightArrowBox.Contains(relMouse);

            if (!DisableVerticalScroll)
            {
                if (ScrollUpButton.Update(elapsedSeconds, ScrollUpButtonHeld))
                {
                    ScrollByVirtualScrollUnits(new Vector2(0, -ScrollArrowButtonVirtualScrollAmountY));
                }

                if (ScrollDownButton.Update(elapsedSeconds, ScrollDownButtonHeld))
                {
                    ScrollByVirtualScrollUnits(new Vector2(0, ScrollArrowButtonVirtualScrollAmountY));
                }
            }

            if (!DisableHorizontalScroll)
            {
                if (ScrollLeftButton.Update(elapsedSeconds, ScrollLeftButtonHeld))
                {
                    ScrollByVirtualScrollUnits(new Vector2(-ScrollArrowButtonVirtualScrollAmountX, 0));
                }

                if (ScrollRightButton.Update(elapsedSeconds, ScrollRightButtonHeld))
                {
                    ScrollByVirtualScrollUnits(new Vector2(ScrollArrowButtonVirtualScrollAmountX, 0));
                }
            }

            
                var scrollWheel = allowScrollWheel ? input.ScrollDelta : 0;
                if (scrollWheel != 0)
                {
                    if (input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftShift) || input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.RightShift))
                    {
                        if (!DisableHorizontalScroll)
                            ScrollByVirtualScrollUnits(new Vector2(-scrollWheel * ScrollWheelVirtualScrollAmountX, 0));
                    }
                    else
                    {
                        if (!DisableVerticalScroll)
                            ScrollByVirtualScrollUnits(new Vector2(0, -scrollWheel * ScrollWheelVirtualScrollAmountY));
                    }
                }
            
        }


        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, Texture2D arrowTex)
        {
            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(gd.Viewport.Bounds.DpiScaledExcludePos());
            //{
            sb.Begin(transformMatrix: Main.DPIMatrix);
            try
            {

                //var scrollArrowOrigin = font.MeasureString(ScrollArrowStr) / 2;

                if (!DisableHorizontalScroll)
                {
                    sb.Draw(boxTex, HorizontalBoxValidArea, Main.Colors.GuiColorEventGraphScrollbarBackground);
                    sb.Draw(boxTex, HorizontalBox, CurrentScroll == ScrollbarType.Horizontal ?
                        Main.Colors.GuiColorEventGraphScrollbarForegroundActive : Main.Colors.GuiColorEventGraphScrollbarForegroundInactive);

                    sb.Draw(boxTex, ScrollLeftArrowBox,
                        ScrollLeftButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive
                        : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive);
                    sb.Draw(boxTex, ScrollRightArrowBox,
                        ScrollRightButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive
                        : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive);

                    sb.Draw(arrowTex, new Vector2(ScrollLeftArrowBox.X, ScrollLeftArrowBox.Y) + Vector2.One * 8, null, ScrollLeftButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive
                    : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive, MathHelper.PiOver2 * 3, Vector2.One * 8, Vector2.One, SpriteEffects.None, 0);

                    sb.Draw(arrowTex, new Vector2(ScrollRightArrowBox.X, ScrollRightArrowBox.Y) + Vector2.One * 8, null, ScrollRightButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive
                   : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive, MathHelper.PiOver2 * 1, Vector2.One * 8, Vector2.One, SpriteEffects.None, 0);
                }

                if (!DisableVerticalScroll)
                {
                    sb.Draw(boxTex, VerticalBoxValidArea, Main.Colors.GuiColorEventGraphScrollbarBackground);
                    sb.Draw(boxTex, VerticalBox, CurrentScroll == ScrollbarType.Vertical ?
                        Main.Colors.GuiColorEventGraphScrollbarForegroundActive : Main.Colors.GuiColorEventGraphScrollbarForegroundInactive);

                    sb.Draw(boxTex, ScrollUpArrowBox,
                        ScrollUpButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive
                        : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive);
                    sb.Draw(boxTex, ScrollDownArrowBox,
                        ScrollDownButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive
                        : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive);

                    sb.Draw(arrowTex, new Vector2(ScrollUpArrowBox.X, ScrollUpArrowBox.Y) + Vector2.One * 8, null, ScrollUpButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive
                    : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive, 0, Vector2.One * 8, Vector2.One, SpriteEffects.None, 0);

                    sb.Draw(arrowTex, new Vector2(ScrollDownArrowBox.X, ScrollDownArrowBox.Y) + Vector2.One * 8, null, ScrollDownButtonHeld ? Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundInactive
                   : Main.Colors.GuiColorEventGraphScrollbarArrowButtonForegroundActive, MathHelper.Pi, Vector2.One * 8, Vector2.One, SpriteEffects.None, 0);
                }
            }
            finally 
            { 
                sb.End();
                gd.Viewport = oldViewport;
            }
            //}
            //gd.Viewport = oldViewport;
        }
    }
}
