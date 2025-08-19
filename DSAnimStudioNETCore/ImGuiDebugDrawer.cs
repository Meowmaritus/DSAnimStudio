using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework.Graphics;
using NVector4 = System.Numerics.Vector4;

namespace DSAnimStudio
{
    public static class ImGuiDebugDrawer
    {
        private static Stack<bool> imguiDisabledFlagStack = new Stack<bool>();

        private static bool _enableState = true;
        public static bool CurrentEnableState
        {
            get => _enableState;
            set
            {
                _enableState = value;
                if (_enableState)
                    ImGui.EndDisabled();
                else 
                    ImGui.BeginDisabled();
            }
        }
        
        public static void PushDisabled(bool disabled = true)
        {
            imguiDisabledFlagStack.Push(CurrentEnableState);
            CurrentEnableState = !disabled;
        }

        public static void PopDisabled()
        {
            if (imguiDisabledFlagStack.TryPop(out bool lastEnableState))
            {
                CurrentEnableState = lastEnableState;
            }
            else
            {
                CurrentEnableState = true;
            }
        }

        public static bool Active = false;
        
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
                ImGuiWindowFlags.NoInputs | 
                ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, TaeEditorScreen.TopMenuBarMargin));
            ImGui.SetWindowSize(new System.Numerics.Vector2(
                Program.MainInstance.Window.ClientBounds.Width,
                Program.MainInstance.Window.ClientBounds.Height - TaeEditorScreen.TopMenuBarMargin));

            Active = true;
        }

        public static void End()
        {
            try
            {
                ImGui.End();
                
                ImGui.PopStyleColor(2);
            }
            catch (AccessViolationException)
            {

            }

            Active = false;

        }
        
        public static Vector2 ViewportOffset;

        public static unsafe bool FakeButton(Vector2 pos, Vector2 size, string text, float cornerRounding, out bool isHovering,
            float? overrideFontSize = null, RectF? absoluteClippingRect = null, bool disableInput = false)
        {
            

            bool result = false;
            NVector4 bgColor = NVector4.Zero;

            var withinAbsClippingRect = absoluteClippingRect?.Contains(Main.Input.MousePosition) ?? true;
            
            var relMouse = (Main.Input.MousePosition) - (GFX.Device.Viewport.Bounds.TopLeftCorner() / Main.DPIVector);
            if (!disableInput && withinAbsClippingRect && relMouse.X >= pos.X && relMouse.Y >= pos.Y && relMouse.X <= (pos.X + size.X) && relMouse.Y <= (pos.Y + size.Y))
            {
                isHovering = true;
                if (Main.Input.LeftClickHeld && !disableInput)
                {
                   
                    bgColor = *ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive);
                }
                else if (Main.Input.LeftClickUp && !disableInput)
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
                isHovering = false;
                bgColor = *ImGui.GetStyleColorVec4(ImGuiCol.Button);
            }

            var prevViewport = GFX.Device.Viewport;
            
            if (absoluteClippingRect != null)
            {
                //GFX.Device.Viewport = new Viewport(absoluteClippingRect.Value);
                DrawRect(pos, size, new Color(bgColor), cornerRounding, customClippingRect: absoluteClippingRect?.DpiScaled());
                //GFX.Device.Viewport = prevViewport;
            }
            else
            {
                DrawRect(pos, size, new Color(bgColor), cornerRounding);
            }
            //var oldViewport = GFX.Device.Viewport;
            //GFX.Device.Viewport = new Microsoft.Xna.Framework.Graphics.Viewport(new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y));
            //{
            //    var strSize = MeasureString(text, Main.ImGuiFontPixelSize);
            //    var textColor = *ImGui.GetStyleColorVec4(ImGuiCol.Text);
            //    DrawText(text, (size / 2) - strSize / 2, new Color(textColor), new Color(0, 0, 0, 0), Main.ImGuiFontPixelSize);
            //}
            //GFX.Device.Viewport = oldViewport;



            var strSize = MeasureString(text, overrideFontSize ?? Main.ImGuiFontPixelSize);
            var textColor = *ImGui.GetStyleColorVec4(ImGuiCol.Text);
            DrawText(text, pos + ((size / 2) - strSize / 2), new Color(textColor), new Color(0, 0, 0, 0), overrideFontSize ?? Main.ImGuiFontPixelSize);

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

        public static Vector2 MeasureString(string text, float fontSize = Main.ImGuiFontPixelSize)
        {

            return (ImGui.CalcTextSize(text) * (fontSize / Main.ImGuiFontPixelSize)).ToXna() / Main.DPIVector;
        }

        public static void PushClipRect(RectF? customClippingRect)
        {
            if (customClippingRect == null)
            {
                ImGui.PushClipRect(new System.Numerics.Vector2(
                        ViewportOffset.X + GFX.Device.Viewport.X,
                        ViewportOffset.Y + GFX.Device.Viewport.Y),
                    new System.Numerics.Vector2(
                        ViewportOffset.X + GFX.Device.Viewport.X + GFX.Device.Viewport.Width,
                        ViewportOffset.Y + GFX.Device.Viewport.Y + GFX.Device.Viewport.Height),
                    true);
            }
            else
            {
                var clipRect = customClippingRect.Value;
                ImGui.PushClipRect(new System.Numerics.Vector2(
                        clipRect.X,
                        clipRect.Y),
                    new System.Numerics.Vector2(
                        clipRect.Right,
                        clipRect.Bottom),
                    false);
            }
        }

        public static void DrawText3D(string text, Vector3 pos, Color? color = null, Color? shadowColor = null,
            float fontSize = Main.ImGuiFontPixelSize, bool includeCardinalShadows = false, float shadowThickness = 1,
            bool disableClippingRect = false, RectF? customClippingRect = null, bool dpiScale = true, bool inverseSSAAScale = false)
        {
            Vector2 calculatedPos = GFX.CurrentWorldView.Project3DPosToScreen(pos);
            DrawText(text, calculatedPos, color, shadowColor, fontSize, includeCardinalShadows, shadowThickness, disableClippingRect, customClippingRect, dpiScale, inverseSSAAScale);
        }

        public static void DrawLine3D(Vector3 a, Vector3 b, Color c, RectF? customClippingRect = null, bool dpiScale = true, float thickness = 1)
        {
            var calculatedPosA = GFX.CurrentWorldView.Project3DPosToScreen(a);
            var calculatedPosB = GFX.CurrentWorldView.Project3DPosToScreen(b);
            DrawLine(calculatedPosA, calculatedPosB, c, customClippingRect, dpiScale, thickness);
        }

        public static void DrawLine(Vector2 a, Vector2 b, Color c, RectF? customClippingRect = null, bool dpiScale = true, float thickness = 1)
        {
            if (dpiScale)
            {
                a *= Main.DPIVector;
                b *= Main.DPIVector;
            }

            PushClipRect(customClippingRect);

            a += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            b += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;

            var drawList = ImGui.GetWindowDrawList();

            drawList.AddLine(a.ToCS(), b.ToCS(), c.PackedValue, thickness);

            ImGui.PopClipRect();
        }



        //public static void DrawInfiniteLineH(float y, Color color)
        //{
        //    DrawLine(new Vector2(float.MinValue, y), new Vector2(float.MaxValue, y), color);
        //}

        //public static void DrawInfiniteLineV(float x, Color color)
        //{
        //    DrawLine(new Vector2(x, float.MinValue), new Vector2(x, float.MaxValue), color);
        //}

        public static void DrawCircle(Vector2 a, float r, Color color, int numSegments = 0, float thickness = 0, RectF? customClippingRect = null, bool dpiScale = true)
        {
            if (dpiScale)
            {
                a *= Main.DPIVector;
                r *= Main.DPIVector.X;
            }

            PushClipRect(customClippingRect);

            var drawList = ImGui.GetWindowDrawList();
            a += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            if (thickness <= 0)
                drawList.AddCircleFilled(a.ToCS(), r, color.PackedValue, numSegments);
            else
                drawList.AddCircle(a.ToCS(), r, color.PackedValue, numSegments, thickness);
            ImGui.PopClipRect();
        }

        public static void DrawTriangle(Vector2 center, float radius, Color color, float thickness = 0, RectF? customClippingRect = null, bool dpiScale = true)
        {
            Vector2 spoke = new Vector2(0, -1) * radius;

            Vector2 a = center + spoke;
            spoke = Vector2.Transform(spoke, Matrix.CreateRotationZ((float)(Math.PI * 2.0 / 3.0)));
            Vector2 b = center + spoke;
            spoke *= new Vector2(-1, 1);
            Vector2 c = center + spoke;

            float triHeight = c.Y - a.Y;
            float triMidY = a.Y + (triHeight / 2);
            Vector2 triPosCorrect = new Vector2(0, center.Y - triMidY);

            a += triPosCorrect;
            b += triPosCorrect;
            c += triPosCorrect;

            if (dpiScale)
            {
                a *= Main.DPIVector;
                b *= Main.DPIVector;
                c *= Main.DPIVector;
            }


            PushClipRect(customClippingRect);

            var drawList = ImGui.GetWindowDrawList();
            a += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            b += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            c += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;

            //drawList.AddRectFilled(pos.ToCS(), (pos + size).ToCS(), color.PackedValue, cornerRounding);
            if (thickness <= 0)
                drawList.AddTriangleFilled(a.ToCS(), b.ToCS(), c.ToCS(), color.PackedValue);
            else
                drawList.AddTriangle(a.ToCS(), b.ToCS(), c.ToCS(), color.PackedValue, thickness);
            ImGui.PopClipRect();
        }


        public static void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Color color, float thickness = 0, RectF? customClippingRect = null, bool dpiScale = true)
        {
            if (dpiScale)
            {
                a *= Main.DPIVector;
                b *= Main.DPIVector;
                c *= Main.DPIVector;
            }
            

            PushClipRect(customClippingRect);

            var drawList = ImGui.GetWindowDrawList();
            a += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            b += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            c += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;

            //drawList.AddRectFilled(pos.ToCS(), (pos + size).ToCS(), color.PackedValue, cornerRounding);
            if (thickness <= 0)
                drawList.AddTriangleFilled(a.ToCS(), b.ToCS(), c.ToCS(), color.PackedValue);
            else
                drawList.AddTriangle(a.ToCS(), b.ToCS(), c.ToCS(), color.PackedValue, thickness);
            ImGui.PopClipRect();
        }
        public static void DrawRect(Vector2 pos, Vector2 size, Color color, float cornerRounding = 0, float thickness = 0, RectF? customClippingRect = null, bool dpiScale = true, ImDrawFlags drawFlags = ImDrawFlags.None)
        {
            if (dpiScale)
            {
                pos *= Main.DPIVector;
                size *= Main.DPIVector;
            }

            PushClipRect(customClippingRect);

            var drawList = ImGui.GetWindowDrawList();
            pos += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;

            if (thickness <= 0)
                drawList.AddRectFilled(pos.ToCS(), (pos + size).ToCS(), color.PackedValue, cornerRounding, drawFlags);
            else
                drawList.AddRect(pos.ToCS(), (pos + size).ToCS(), color.PackedValue, cornerRounding, drawFlags, thickness);
            ImGui.PopClipRect();
        }

        public static void DrawRect(RectF rect, Color color, float cornerRounding = 0, float thickness = 0, RectF? customClippingRect = null, bool dpiScale = true, ImDrawFlags drawFlags = ImDrawFlags.None)
        {
            DrawRect(rect.Location, rect.Size, color, cornerRounding, thickness, customClippingRect, dpiScale, drawFlags);
        }

        private static float sqrt2 = (float)Math.Sqrt(2);
        public static void DrawText(string text, Vector2 pos, Color? color = null, Color? shadowColor = null,
            float fontSize = Main.ImGuiFontPixelSize, bool includeCardinalShadows = false, float shadowThickness = 1, 
            bool disableClippingRect = false, RectF? customClippingRect = null, bool dpiScale = true, bool inverseSSAAScale = false)
        {
            if (dpiScale)
            {
                pos *= Main.DPIVector;
                fontSize *= Main.DPIVector.Y;
            }
            if (inverseSSAAScale)
                pos /= GFX.EffectiveSSAA;

            shadowColor = shadowColor ?? Color.Black;
            
            if (shadowColor.HasValue && color.HasValue)
            {
                var colorVec4 = color.Value.ToVector4();
                var shadowColorVec4 = shadowColor.Value.ToVector4();

                shadowColorVec4.W *= colorVec4.W * colorVec4.W * colorVec4.W * colorVec4.W;

                shadowThickness *= colorVec4.W * colorVec4.W;
                
                shadowColor = new Color(shadowColorVec4);
            }

            if (!disableClippingRect)
            {
                PushClipRect(customClippingRect);
            }

            var drawList = ImGui.GetWindowDrawList();

            pos += new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y) + ViewportOffset;
            var posCS = pos.ToNumerics();
            if (shadowThickness > 0)
            {
                drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(1, 1) * shadowThickness / sqrt2),
                    shadowColor?.PackedValue ?? 0xFF000000, text);
                drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(1, -1) * shadowThickness / sqrt2),
                    shadowColor?.PackedValue ?? 0xFF000000, text);
                drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(-1, 1) * shadowThickness / sqrt2),
                    shadowColor?.PackedValue ?? 0xFF000000, text);
                drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(-1, -1) * shadowThickness / sqrt2),
                    shadowColor?.PackedValue ?? 0xFF000000, text);

                if (includeCardinalShadows)
                {
                    // Rather than dividing diagonals by sqrt(2) to shorten them to correct length, we are
                    // multiplying the cardinals by sqrt(2) to make them longer, so diagonals are same thickness they were originally
                    // and the new cardinals are the correct length, or something like that
                    //shadowThickness *= sqrt2;

                    drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(0, 1) * shadowThickness),
                        shadowColor?.PackedValue ?? 0xFF000000, text);
                    drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(0, -1) * shadowThickness),
                        shadowColor?.PackedValue ?? 0xFF000000, text);
                    drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(-1, 0) * shadowThickness),
                        shadowColor?.PackedValue ?? 0xFF000000, text);
                    drawList.AddText(Main.ImGuiFontPointer, fontSize, posCS + (new System.Numerics.Vector2(1, 0) * shadowThickness),
                        shadowColor?.PackedValue ?? 0xFF000000, text);
                }
            }

            drawList.AddText(Main.ImGuiFontPointer, fontSize, new System.Numerics.Vector2(pos.X, pos.Y),
                color?.PackedValue ?? 0xFFFFFFFF, text);

            if (!disableClippingRect)
                ImGui.PopClipRect();

        }

        
    }
}
