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

        public Vector2? Position;
        public Vector3 Position3D = Vector3.Zero;
        public Color? Color;
        public SpriteFont Font;

        public float BaseScale = 1.0f;

        public bool ScaleByEffectiveSSAA = false;

        public StatusPrinter(Vector2? pos, Color? color = null, SpriteFont font = null)
        {
            Position = pos;
            Color = color;
            Font = font;
        }

        private List<StatusLine> statusLines = new List<StatusLine>();

        public void AppendLine()
        {
            AppendLine("");
        }

        public void AppendLine(string text, Color? color = null, SpriteFont font = null)
        {
            statusLines.Add(new StatusLine()
            {
                Text = text,
                Color = color,
                Font = font
            });
        }

        public void Clear()
        {
            statusLines.Clear();
        }

        public int LineCount => statusLines.Count;

        public void Draw()
        {
            Vector2 currentPos = Vector2.Zero;

            float scale = BaseScale;

            if (ScaleByEffectiveSSAA)
            {
                scale *= GFX.EffectiveSSAA;
            }

            if (Position != null)
            {
                currentPos = Position.Value;
            }
            else
            {
                Vector3 screenPos3D = GFX.Device.Viewport.Project(Position3D,
                GFX.World.MatrixProjection, GFX.World.CameraTransform.CameraViewMatrix
                * Matrix.Invert(GFX.World.MatrixWorld), GFX.World.WorldMatrixMOD);

                screenPos3D -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);

                if (screenPos3D.Z >= 1)
                    return;

                screenPos3D += new Vector3(4, 4, 0) * scale;

                currentPos = new Vector2(screenPos3D.X, screenPos3D.Y);
            }

            currentPos = new Vector2((float)Math.Round(currentPos.X), (float)Math.Round(currentPos.Y));

            foreach (var line in statusLines)
            {
                var font = line.Font ?? Font ?? DBG.DEBUG_FONT_SMALL;

                if (string.IsNullOrWhiteSpace(line.Text))
                {
                    currentPos.Y += font.LineSpacing / 2;
                    continue;
                }

               
                var color = line.Color ?? Color ?? Microsoft.Xna.Framework.Color.Cyan;
                var textSize = font.MeasureString(line.Text) * scale;

                GFX.SpriteBatch.DrawString(font, line.Text, currentPos + Vector2.One * scale, 
                    Microsoft.Xna.Framework.Color.Black, 0, Vector2.Zero, scale, SpriteEffects.None, 0.01f);

                GFX.SpriteBatch.DrawString(font, line.Text, currentPos, color, 0, 
                    Vector2.Zero, scale, SpriteEffects.None, 0);

                currentPos.Y += textSize.Y;
            }
        }
    }
}
