using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVector4 = System.Numerics.Vector4;

namespace DSAnimStudio
{
    public static class ImGuiDebugDrawer
    {
        public const float BaseFontSize = 20;

        public static void Begin()
        {
            ImGui.PushStyleColor(ImGuiCol.Border, System.Numerics.Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, System.Numerics.Vector4.Zero);
            ImGui.Begin("CUSTOM_DRAW_DUMMY_WINDOW",
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoInputs);
            ImGui.SetWindowPos(System.Numerics.Vector2.Zero);
            ImGui.SetWindowSize(new System.Numerics.Vector2(
                Program.MainInstance.ClientBounds.Width,
                Program.MainInstance.ClientBounds.Height));
        }

        public static Vector2 ViewportOffset;

        public static unsafe bool FakeButton(Vector2 pos, Vector2 size, string text, float cornerRounding)
        {
            

            bool result = false;
            NVector4 bgColor = NVector4.Zero;
            var relMouse = (Main.Input.MousePosition) - (GFX.Device.Viewport.Bounds.TopLeftCorner() / Main.DPIVector);
            if (relMouse.X >= pos.X && relMouse.Y >= pos.Y && relMouse.X <= (pos.X + size.X) && relMouse.Y <= (pos.Y + size.Y))
            {
                if (Main.Input.LeftClickDown)
                {
                    result = true;
                    bgColor = *ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive);
                }
                else
                {
                    bgColor = *ImGui.GetStyleColorVec4(ImGuiCol.ButtonHovered);
                }
            }
            else
            {
                bgColor = *ImGui.GetStyleColorVec4(ImGuiCol.Button);
            }

            DrawRect(pos, size, new Color(bgColor), cornerRounding);

            //var oldViewport = GFX.Device.Viewport;
            //GFX.Device.Viewport = new Microsoft.Xna.Framework.Graphics.Viewport(new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y));
            //{
            //    var strSize = MeasureString(text, Main.ImGuiFontPixelSize);
            //    var textColor = *ImGui.GetStyleColorVec4(ImGuiCol.Text);
            //    DrawText(text, (size / 2) - strSize / 2, new Color(textColor), new Color(0, 0, 0, 0), Main.ImGuiFontPixelSize);
            //}
            //GFX.Device.Viewport = oldViewport;



            var strSize = MeasureString(text, Main.ImGuiFontPixelSize);
            var textColor = *ImGui.GetStyleColorVec4(ImGuiCol.Text);
            DrawText(text, pos + ((size / 2) - strSize / 2), new Color(textColor), new Color(0, 0, 0, 0), Main.ImGuiFontPixelSize);

            return result;
        }

        public static void DrawTtest()
        {
            //var drawList = ImGui.GetWindowDrawList();
            //for (int i = 0; i < 10000; i++)
            //{
            //    drawList.AddText(new System.Numerics.Vector2(602, 102), 0xFF000000, "this is a test text at 600,100");
            //    drawList.AddText(new System.Numerics.Vector2(600, 100), 0xFFFFFFFF, "this is a test text at 600,100");
            //}
        }

        public static Vector2 MeasureString(string text, float fontSize = BaseFontSize)
        {

            return (ImGui.CalcTextSize(text) * (fontSize / Main.ImGuiFontPixelSize)).ToXna() / Main.DPIVector;
        }

        public static void DrawText3D(string text, Vector3 pos, Color? color = null, Color? shadowColor = null,
            float fontSize = BaseFontSize)
        {
            Vector2 calculatedPos = GFX.CurrentWorldView.Project3DPosToScreen(pos);
            DrawText(text, calculatedPos, color, shadowColor, fontSize);
        }

        public static void DrawLine(Vector2 a, Vector2 b, Color c)
        {
            a *= Main.DPIVector;
            b *= Main.DPIVector;

            ImGui.PushClipRect(new System.Numerics.Vector2(
                ViewportOffset.X + GFX.Device.Viewport.X,
                ViewportOffset.Y + GFX.Device.Viewport.Y),
                new System.Numerics.Vector2(
                ViewportOffset.X + GFX.Device.Viewport.X + GFX.Device.Viewport.Width,
                ViewportOffset.Y + GFX.Device.Viewport.Y + GFX.Device.Viewport.Height),
                false);

            a += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            b += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddLine(a.ToCS(), b.ToCS(), c.PackedValue);

            ImGui.PopClipRect();
        }

        public static void DrawRect(Vector2 pos, Vector2 size, Color color, float cornerRounding = 0)
        {
            pos *= Main.DPIVector;
            size *= Main.DPIVector;

            ImGui.PushClipRect(new System.Numerics.Vector2(
                ViewportOffset.X + GFX.Device.Viewport.X,
                ViewportOffset.Y + GFX.Device.Viewport.Y),
                new System.Numerics.Vector2(
                ViewportOffset.X + GFX.Device.Viewport.X + GFX.Device.Viewport.Width,
                ViewportOffset.Y + GFX.Device.Viewport.Y + GFX.Device.Viewport.Height),
                false);

            var drawList = ImGui.GetWindowDrawList();
            pos += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;

            drawList.AddRectFilled(pos.ToCS(), (pos + size).ToCS(), color.PackedValue, cornerRounding);

            ImGui.PopClipRect();
        }

        public static void DrawRect(Rectangle rect, Color color, float cornerRounding = 0)
        {
            DrawRect(rect.TopLeftCorner(), rect.Size.ToVector2(), color, cornerRounding);
        }

        public static void DrawText(string text, Vector2 pos, Color? color = null, Color? shadowColor = null,
            float fontSize = BaseFontSize)
        {
            pos *= Main.DPIVector;
            fontSize *= Main.DPIVector.Y;

            ImGui.PushClipRect(new System.Numerics.Vector2(
                    ViewportOffset.X + GFX.Device.Viewport.X,
                    ViewportOffset.Y + GFX.Device.Viewport.Y),
                    new System.Numerics.Vector2(
                    ViewportOffset.X + GFX.Device.Viewport.X + GFX.Device.Viewport.Width,
                    ViewportOffset.Y + GFX.Device.Viewport.Y + GFX.Device.Viewport.Height),
                    false);

            var drawList = ImGui.GetWindowDrawList();

            pos += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;

            drawList.AddText(Main.ImGuiFontPointer, fontSize, new System.Numerics.Vector2(pos.X + 1, pos.Y + 1),
                shadowColor?.PackedValue ?? 0xFF000000, text);
            drawList.AddText(Main.ImGuiFontPointer, fontSize, new System.Numerics.Vector2(pos.X + 1, pos.Y - 1),
                shadowColor?.PackedValue ?? 0xFF000000, text);
            drawList.AddText(Main.ImGuiFontPointer, fontSize, new System.Numerics.Vector2(pos.X - 1, pos.Y + 1),
                shadowColor?.PackedValue ?? 0xFF000000, text);
            drawList.AddText(Main.ImGuiFontPointer, fontSize, new System.Numerics.Vector2(pos.X - 1, pos.Y - 1),
                shadowColor?.PackedValue ?? 0xFF000000, text);

            drawList.AddText(Main.ImGuiFontPointer, fontSize, new System.Numerics.Vector2(pos.X, pos.Y),
                color?.PackedValue ?? 0xFFFFFFFF, text);

            ImGui.PopClipRect();

        }

        public static void End()
        {
            try
            {
                ImGui.End();
            }
            catch (AccessViolationException)
            {

            }
            ImGui.PopStyleColor(2);
        }
    }
}
