using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class StatusPrinter
    {
        public class StatusLine
        {
            public SpriteFont Font;
            public Color? Color;
            public string Text;
        }

        public Vector2 Position;
        public Color? Color;
        public SpriteFont Font;

        public StatusPrinter(Vector2 pos, Color? color = null, SpriteFont font = null)
        {
            Position = pos;
            Color = color;
            Font = font;
        }

        private List<StatusLine> statusLines = new List<StatusLine>();

        public void AppendLine(string text, Color? color = null, SpriteFont font = null)
        {
            statusLines.Add(new StatusLine()
            {
                Text = text,
                Color = color,
                Font = font
            });
        }

        public void Draw()
        {
            GFX.SpriteBatch.Begin();
            Vector2 currentPos = Position;
            foreach (var line in statusLines)
            {
                var font = line.Font ?? Font ?? DBG.DEBUG_FONT_SMALL;
                var color = line.Color ?? Color ?? Microsoft.Xna.Framework.Color.Cyan;
                var textSize = font.MeasureString(line.Text);
                GFX.SpriteBatch.DrawString(font, line.Text, currentPos + Vector2.One * 2, Microsoft.Xna.Framework.Color.Black);
                GFX.SpriteBatch.DrawString(font, line.Text, currentPos, color);
                currentPos.Y += textSize.Y;
            }
            GFX.SpriteBatch.End();
        }
    }
}
