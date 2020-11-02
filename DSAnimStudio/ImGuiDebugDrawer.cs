using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void DrawTtest()
        {
            //var drawList = ImGui.GetWindowDrawList();
            //for (int i = 0; i < 10000; i++)
            //{
            //    drawList.AddText(new System.Numerics.Vector2(602, 102), 0xFF000000, "this is a test text at 600,100");
            //    drawList.AddText(new System.Numerics.Vector2(600, 100), 0xFFFFFFFF, "this is a test text at 600,100");
            //}
        }

        public static void DrawText(string text, Vector2 pos, Color? color = null, Color? shadowColor = null,
            float fontSize = BaseFontSize)
        {
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
