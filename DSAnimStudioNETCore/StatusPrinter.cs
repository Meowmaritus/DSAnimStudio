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
        private object _lock = new object();

        public class StatusLine
        {
            public Color? Color;
            public string Text;
            public float? ShadowThickness = null;
            public bool? EnableShadowCardinals = null;
        }

        public Vector2? Position;
        public Vector3 Position3D = Vector3.Zero;
        public Color? Color;
        public Color? ShadowColor;
        public float? ShadowThickness = null;
        public bool? EnableShadowCardinals = null;

        public float BaseScale = 1.0f;

        public bool ScaleByEffectiveSSAA = false;

        public StatusPrinter(Vector2? pos = null, Color? color = null, Color? shadowColor = null, float? shadowThickness = null, bool? enableShadowCardinals = null)
        {
            Position = pos;
            Color = color;
            ShadowColor = shadowColor;
            ShadowThickness = shadowThickness;
            EnableShadowCardinals = enableShadowCardinals;
        }

        private List<StatusLine> statusLines = new List<StatusLine>();

        public void Clear()
        {
            statusLines.Clear();
        }

        public void AppendLine()
        {
            AppendLine("");
        }

        public void AppendLine(string text, Color? color = null, float? shadowThickness = null, bool? enableShadowCardinals = null)
        {
            statusLines.Add(new StatusLine()
            {
                Text = text,
                Color = color,
                ShadowThickness = shadowThickness,
                EnableShadowCardinals = enableShadowCardinals,
            });
        }

        public void InvokeLocked(Action invoke)
        {
            lock (_lock)
            {
                invoke.Invoke();
            }
        }

        public int LineCount => statusLines.Count;

        public void DoAsImGuiLayout()
        {
            lock (_lock)
            {
                foreach (var line in statusLines)
                {
                    ImGuiNET.ImGui.PushStyleColor(ImGuiNET.ImGuiCol.Text, (line.Color ?? Color ?? Microsoft.Xna.Framework.Color.White).PackedValue);
                    if (string.IsNullOrWhiteSpace(line.Text))
                        ImGuiNET.ImGui.NewLine();
                    else
                        ImGuiNET.ImGui.Text(line.Text);
                    ImGuiNET.ImGui.PopStyleColor();
                }
            }
        }

        public void Draw(out float finalHeight)
        {
            lock (_lock)
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
                    GFX.CurrentWorldView.Matrix_Projection, GFX.CurrentWorldView.Matrix_View, GFX.CurrentWorldView.Matrix_World);

                    screenPos3D -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);

                    if (screenPos3D.Z >= 1)
                    {
                        finalHeight = 0;
                        return;
                    }

                    //screenPos3D += new Vector3(4, 4, 0) * scale;

                    currentPos = new Vector2(screenPos3D.X, screenPos3D.Y) / Main.DPIVector / (float)GFX.EffectiveSSAA;
                }

                currentPos = new Vector2((float)Math.Round(currentPos.X), (float)Math.Round(currentPos.Y)) * GFX.EffectiveSSAA;

                var posBeforeDraw = currentPos;
                
                foreach (var line in statusLines)
                {
                    if (string.IsNullOrWhiteSpace(line.Text))
                    {
                        currentPos.Y += ((Main.ImGuiFontPixelSize * scale) / 2) * GFX.EffectiveSSAA;
                        continue;
                    }

                    var color = line.Color ?? Color ?? Microsoft.Xna.Framework.Color.Cyan;
                    float shadowThickness = ShadowThickness ?? line.ShadowThickness ?? 1;
                    bool shadowCardinals = EnableShadowCardinals ?? line.EnableShadowCardinals ?? false;
                    ImGuiDebugDrawer.DrawText(line.Text, currentPos, color, shadowColor: ShadowColor, 
                        fontSize: Main.ImGuiFontPixelSize * scale, includeCardinalShadows: shadowCardinals, 
                        shadowThickness: shadowThickness, inverseSSAAScale: true);

                    currentPos.Y += ((Main.ImGuiFontPixelSize * scale) * line.Text.Split("\n").Length) * GFX.EffectiveSSAA;
                }
                
                var posAfterDraw = currentPos;

                finalHeight = posAfterDraw.Y - posBeforeDraw.Y;
            }
        }
    }
}
