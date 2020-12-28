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
            public Color? Color;
            public string Text;
        }

        public Vector2? Position;
        public Vector3 Position3D = Vector3.Zero;
        public Color? Color;
        public Color? ShadowColor;

        public float BaseScale = 1.0f;

        public bool ScaleByEffectiveSSAA = false;

        public StatusPrinter(Vector2? pos, Color? color = null, Color? shadowColor = null)
        {
            Position = pos;
            Color = color;
            ShadowColor = shadowColor;
        }

        private List<StatusLine> statusLines = new List<StatusLine>();

        public void AppendLine()
        {
            AppendLine("");
        }

        public void AppendLine(string text, Color? color = null)
        {
            statusLines.Add(new StatusLine()
            {
                Text = text,
                Color = color,
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
                GFX.World.Matrix_Projection, GFX.World.Matrix_View, GFX.World.Matrix_World);

                screenPos3D -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);

                if (screenPos3D.Z >= 1)
                    return;

                //screenPos3D += new Vector3(4, 4, 0) * scale;

                currentPos = new Vector2(screenPos3D.X, screenPos3D.Y) / Main.DPIVector;
            }

            currentPos = new Vector2((float)Math.Round(currentPos.X), (float)Math.Round(currentPos.Y));

            foreach (var line in statusLines)
            {

                if (string.IsNullOrWhiteSpace(line.Text))
                {
                    currentPos.Y += (ImGuiDebugDrawer.BaseFontSize * scale) / 2;
                    continue;
                }

               
                var color = line.Color ?? Color ?? Microsoft.Xna.Framework.Color.Cyan;
                ImGuiDebugDrawer.DrawText(line.Text, currentPos, color, shadowColor: ShadowColor, fontSize: ImGuiDebugDrawer.BaseFontSize * scale);

                currentPos.Y += ImGuiDebugDrawer.BaseFontSize * scale;
            }
        }
    }
}
