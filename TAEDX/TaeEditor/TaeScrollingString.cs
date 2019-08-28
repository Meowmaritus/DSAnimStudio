using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TAEDX.TaeEditor
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

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Matrix spriteBatchMatrix, Rectangle rect, SpriteFont font, float elapsedSeconds)
        {
            // If string is literally not visible then return;
            if ((rect.Right - rect.Left) <= 0)
                return;

            if (TextSize.X < 0 || TextSize.Y < 0)
            {
                TextSize = font.MeasureString(Text);
            }

            Vector3 transformedTextPos = Vector3.Transform(new Vector3(rect.Left, rect.Top, 0), spriteBatchMatrix);
            Vector3 transformedTextMaxPos = Vector3.Transform(new Vector3(rect.Right, rect.Bottom, 0), spriteBatchMatrix);

            int transformedLeft = MathHelper.Max((int)transformedTextPos.X, 0);
            int transformedTop = MathHelper.Max((int)transformedTextPos.Y, 0);
            int transformedRight = MathHelper.Min((int)transformedTextMaxPos.X, gd.Viewport.Width);
            int transformedBottom = MathHelper.Min((int)transformedTextMaxPos.Y, gd.Viewport.Height);

            rect = new Rectangle(
                transformedLeft,
                transformedTop,
                transformedRight - transformedLeft,
                transformedBottom - transformedTop);



            if (TextSize.X <= rect.Width)
            {
                sb.End();

                sb.Begin();

                ResetScroll();

                var pos = new Vector2(rect.Left, rect.Center.Y - (TextSize.Y / 2f));
                sb.DrawString(font, Text, pos + Vector2.One, TextShadowColor);
                sb.DrawString(font, Text, pos + (Vector2.One * 2), TextShadowColor);
                sb.DrawString(font, Text, pos, TextColor);

                sb.End();

                sb.Begin(transformMatrix: spriteBatchMatrix);
            }
            else
            {
                

                Vector2 textPos = new Vector2(ScrollingSnapsToPixels ? (int)(-Scroll) : -Scroll, (rect.Height / 2f) - (TextSize.Y / 2f));

                sb.End();

                var oldViewport = gd.Viewport;
                // Get sub-viewport inside this one.
                int finalRectLeft = MathHelper.Max(oldViewport.Bounds.Left, oldViewport.Bounds.Left + rect.Left);
                int finalRectTop = MathHelper.Max(oldViewport.Bounds.Top, oldViewport.Bounds.Top + rect.Top);
                int finalRectRight = MathHelper.Min(oldViewport.Bounds.Right, oldViewport.Bounds.Left + rect.Right);
                int finalRectBottom = MathHelper.Min(oldViewport.Bounds.Bottom, oldViewport.Bounds.Top + rect.Bottom);

                UpdateTimer(elapsedSeconds, TextSize.X - (finalRectRight - finalRectLeft));

                gd.Viewport = new Viewport(
                    finalRectLeft,
                    finalRectTop,
                    finalRectRight - finalRectLeft,
                    finalRectBottom - finalRectTop);

                sb.Begin(transformMatrix: Matrix.Identity);

                {
                    sb.DrawString(font, Text, textPos + Vector2.One, TextShadowColor);
                    sb.DrawString(font, Text, textPos + (Vector2.One * 2), TextShadowColor);
                    sb.DrawString(font, Text, textPos, TextColor);
                }

                sb.End();

                gd.Viewport = oldViewport;

                sb.Begin(transformMatrix: spriteBatchMatrix);
            }

            
        }
    }
}
