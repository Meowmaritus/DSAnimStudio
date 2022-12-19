using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DSAnimStudio.TaeEditor
{
    public class TaeScrollingString
    {
        public Color TextColor = Color.White;
        public Color TextShadowColor = Color.Black;
        public string Text { get; private set; }
        private Vector2 TextSize;
        private float WaitToScrollTimer = 0;
        public float WaitToScrollDuration = 1;

        private int ScrollDirection = 1;
        public float ScrollPixelsPerSecond = 64;
        private float Scroll;
        private float MaxPixelsToScrollTo = -1;

        public bool ScrollingSnapsToPixels = true;


        public void ResetScroll(bool startImmediatelyNextTime = false)
        {
            WaitToScrollTimer = startImmediatelyNextTime ? -1 : 0;
            Scroll = 0;
            ScrollDirection = 1;
        }

        public void SetText(string newText)
        {
            if (Text != newText)
            {
                Text = newText;
                TextSize = -Vector2.One;
            }
        }

        private void UpdateTimer(float elapsedSeconds, float maxHorizontalScroll)
        {
            MaxPixelsToScrollTo = maxHorizontalScroll;

            if (Scroll > MaxPixelsToScrollTo)
                Scroll = MaxPixelsToScrollTo;

            if (WaitToScrollTimer >= 0)
            {
                WaitToScrollTimer += elapsedSeconds;

                if (WaitToScrollTimer >= WaitToScrollDuration)
                {
                    WaitToScrollTimer = -1;
                    ScrollDirection = -ScrollDirection;
                }
            }
            else
            {
                if (ScrollDirection > 0)
                {
                    Scroll += (ScrollPixelsPerSecond * elapsedSeconds);

                    if (Scroll > MaxPixelsToScrollTo)
                        Scroll = MaxPixelsToScrollTo;

                    if (Scroll == MaxPixelsToScrollTo)
                    {
                        WaitToScrollTimer = 0;
                    }
                }
                else if (ScrollDirection < 0)
                {
                    Scroll -= (ScrollPixelsPerSecond * elapsedSeconds);

                    if (Scroll < 0)
                        Scroll = 0;

                    if (Scroll == 0)
                    {
                        WaitToScrollTimer = 0;
                    }
                }
            }
            
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Matrix spriteBatchMatrix, Rectangle rect, float fontSize, float elapsedSeconds, Vector2 fontOffset, bool restrictToParentViewport = true)
        {
            if (Text == null)
                return;

            // If string is literally not visible then return;
            if ((rect.Right - rect.Left) <= 0)
                return;


            //if (TextSize.X < 0 || TextSize.Y < 0)
            //{
            //    //TextSize = font.MeasureString(Text);
            //    TextSize = ImGuiDebugDrawer.MeasureString(Text, fontSize);
            //}
            TextSize = ImGuiDebugDrawer.MeasureString(Text, fontSize);

            Vector3 transformedTextPos = Vector3.Transform(new Vector3(rect.Left, rect.Top, 0), spriteBatchMatrix);
            Vector3 transformedTextMaxPos = Vector3.Transform(new Vector3(rect.Right, rect.Bottom, 0), spriteBatchMatrix);

            int transformedLeft = (int)transformedTextPos.X;
            int transformedTop = (int)transformedTextPos.Y;
            int transformedRight = (int)transformedTextMaxPos.X;
            int transformedBottom = (int)transformedTextMaxPos.Y;

            rect = new Rectangle(
                transformedLeft,
                transformedTop,
                transformedRight - transformedLeft,
                transformedBottom - transformedTop);


            Vector2 textPos = new Vector2(ScrollingSnapsToPixels ? (float)Math.Round(-Scroll) : -Scroll,
                    0);

            sb.End(); // not in finally {} because it is a restart so it has finally { sb.Begin(...) }
            try
            {

                var oldViewport = gd.Viewport;
                Rectangle finalRect = rect;
                finalRect.X += oldViewport.X;
                finalRect.Y += oldViewport.Y;
                if (restrictToParentViewport)
                {
                    // Get sub-viewport inside this one.
                    int finalRectLeft = MathHelper.Max(oldViewport.Bounds.Left, oldViewport.Bounds.Left + rect.Left);
                    int finalRectTop = MathHelper.Max(oldViewport.Bounds.Top, oldViewport.Bounds.Top + rect.Top);
                    int finalRectRight = MathHelper.Min(oldViewport.Bounds.Right, oldViewport.Bounds.Left + rect.Right);
                    int finalRectBottom = MathHelper.Min(oldViewport.Bounds.Bottom, oldViewport.Bounds.Top + rect.Bottom);

                    finalRect = new Rectangle(finalRectLeft, finalRectTop, finalRectRight - finalRectLeft, finalRectBottom - finalRectTop);
                }

                gd.Viewport = new Viewport(finalRect);


                if ((TextSize.X * Main.DPIVector.X) <= rect.Width)
                {
                    UpdateTimer(elapsedSeconds, 0);
                }
                else
                {
                    UpdateTimer(elapsedSeconds, ((TextSize.X * Main.DPIVector.X) - (finalRect.Width)) / Main.DPIVector.X);
                }

                

                sb.Begin(transformMatrix: Main.DPIMatrix);
                try
                {

                    //sb.DrawString(font, Text, (textPos + new Vector2(0, 1) + fontOffset).RoundInt(), TextShadowColor);
                    //sb.DrawString(font, Text, (textPos + new Vector2(0, -1) + fontOffset).RoundInt(), TextShadowColor);
                    //sb.DrawString(font, Text, (textPos + new Vector2(1, 0) + fontOffset).RoundInt(), TextShadowColor);
                    //sb.DrawString(font, Text, (textPos + new Vector2(-1, 0) + fontOffset).RoundInt(), TextShadowColor);

                    //sb.DrawString(font, Text, (textPos + new Vector2(1, 1) + fontOffset).RoundInt(), TextShadowColor);

                    //sb.DrawString(font, Text, (textPos + new Vector2(2, 2) + fontOffset).RoundInt(), TextShadowColor);
                    //sb.DrawString(font, Text, (textPos + new Vector2(2, 1) + fontOffset).RoundInt(), TextShadowColor);
                    //sb.DrawString(font, Text, (textPos + new Vector2(1, 2) + fontOffset).RoundInt(), TextShadowColor);

                    //sb.DrawString(font, Text, textPos + fontOffset, TextColor);

                    ImGuiDebugDrawer.DrawText(Text, (textPos + fontOffset + new Vector2(0, 0)), TextColor, fontSize: fontSize);
                }
                finally { sb.End(); }

                gd.Viewport = oldViewport;
            }
            finally { sb.Begin(transformMatrix: spriteBatchMatrix); }

            //if (TextSize.X <= rect.Width)
            //{
            //    sb.End();

            //    sb.Begin();

            //    ResetScroll();

            //    var pos = new Vector2(rect.Left, (float)Math.Round((rect.Top + (rect.Height / 2f)) - font.LineSpacing / 2f));
            //    sb.DrawString(font, Text, pos + Vector2.One, TextShadowColor);
            //    //sb.DrawString(font, Text, pos + (Vector2.One * 2), TextShadowColor);
            //    sb.DrawString(font, Text, pos, TextColor);

            //    sb.End();

            //    sb.Begin(transformMatrix: spriteBatchMatrix);
            //}
            //else
            //{


            //    Vector2 textPos = new Vector2(ScrollingSnapsToPixels ? (float)Math.Round(-Scroll) : -Scroll, 
            //        (float)Math.Round((rect.Height / 2f) - font.LineSpacing / 2f));

            //    sb.End();

            //    var oldViewport = gd.Viewport;
            //    // Get sub-viewport inside this one.
            //    int finalRectLeft = MathHelper.Max(oldViewport.Bounds.Left, oldViewport.Bounds.Left + rect.Left);
            //    int finalRectTop = MathHelper.Max(oldViewport.Bounds.Top, oldViewport.Bounds.Top + rect.Top);
            //    int finalRectRight = MathHelper.Min(oldViewport.Bounds.Right, oldViewport.Bounds.Left + rect.Right);
            //    int finalRectBottom = MathHelper.Min(oldViewport.Bounds.Bottom, oldViewport.Bounds.Top + rect.Bottom);

            //    UpdateTimer(elapsedSeconds, TextSize.X - (finalRectRight - finalRectLeft));

            //    gd.Viewport = new Viewport(
            //        finalRectLeft,
            //        finalRectTop,
            //        finalRectRight - finalRectLeft,
            //        finalRectBottom - finalRectTop);

            //    sb.Begin(transformMatrix: Matrix.Identity);

            //    {
            //        sb.DrawString(font, Text, textPos + Vector2.One, TextShadowColor);
            //        //sb.DrawString(font, Text, textPos + (Vector2.One * 2), TextShadowColor);
            //        sb.DrawString(font, Text, textPos, TextColor);
            //    }

            //    sb.End();

            //    gd.Viewport = oldViewport;

            //    sb.Begin(transformMatrix: spriteBatchMatrix);
            //}


        }
    }
}
