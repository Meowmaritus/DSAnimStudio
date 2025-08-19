using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimSolidArrow : DbgPrimSolid
    {
        public const int Segments = 6;
        public const float TipLength = 0.25f;
        public const float TipRadius = 0.10f;
        public const float StemRadius = 0.04f;

        public const float OrientingSpinesLength = 0.5f;

        public const float OrientingSpineThickness = 0.01f;

        private static DbgPrimGeometryData GeometryData = null;

        private static float ThicknessThisGeometryIsFor = 0;

        public static float ExtraVisibilityThicknessSetting => ThicknessThisGeometryIsFor;

        public static void SetExtraVisibilityThickness(float thickness)
        {
            if (thickness != ThicknessThisGeometryIsFor)
            {
                RegenGeometry(Color.Black, thickness);
                ThicknessThisGeometryIsFor = thickness;
            }
        }

        public static void RegenGeometry(Color c, float extraThickness)
        {
            if (GeometryData != null)
            {
                GeometryData?.IndexBuffer?.Dispose();
                GeometryData?.VertBuffer?.Dispose();
               
            }
            GeometryData = null;

            // Create a prim to generate new global geometry data
            var prim = new DbgPrimSolidArrow(Transform.Default, c, extraThickness);
            prim.Dispose();
        }

        public DbgPrimSolidArrow(Transform location, Color c, float extraThickness)
        {
            KeepBuffersAlive = true;
            //BackfaceCulling = false;

            Transform = location;

            if (GeometryData != null)
            {
                GetGeomDataBuffer = () => GeometryData;
            }
            else
            {
                Vector3 GetPoint(int segmentIndex, float radius, float depth)
                {
                    float horizontalAngle = (1.0f * segmentIndex / Segments) * MathHelper.TwoPi;
                    float x = (float)Math.Cos(horizontalAngle);
                    float y = (float)Math.Sin(horizontalAngle);
                    return new Vector3(x * radius, y * radius, depth * Vector3.Forward.Z);
                }

                var th = extraThickness / 2;

                Vector3 spineTip1 = Vector3.Up * (OrientingSpinesLength + th);
                Vector3 spineTip2 = -Vector3.Right * ((OrientingSpinesLength * 0.5f) + th);

                

                Vector3 spineTip1_N = spineTip1 + (Vector3.Forward * th);
                Vector3 spineTip1_S = spineTip1 + (Vector3.Backward * th);
                Vector3 spineTip1_W = spineTip1 + (Vector3.Left * th);
                Vector3 spineTip1_E = spineTip1 + (Vector3.Right * th);

                Vector3 spineBase1_N = (Vector3.Forward * th);
                Vector3 spineBase1_S = (Vector3.Backward * th);
                Vector3 spineBase1_W = (Vector3.Left * th);
                Vector3 spineBase1_E = (Vector3.Right * th);

                Vector3 spineTip2_N = spineTip2 + (Vector3.Up * th);
                Vector3 spineTip2_S = spineTip2 + (Vector3.Down * th);
                Vector3 spineTip2_W = spineTip2 + (Vector3.Backward * th);
                Vector3 spineTip2_E = spineTip2 + (Vector3.Forward * th);

                Vector3 spineBase2_N = (Vector3.Up * th);
                Vector3 spineBase2_S = (Vector3.Down * th);
                Vector3 spineBase2_W = (Vector3.Backward * th);
                Vector3 spineBase2_E = (Vector3.Forward * th);

                AddQuad(spineBase1_N, spineTip1_N, spineTip1_W, spineBase1_W, c);
                AddQuad(spineBase1_W, spineTip1_W, spineTip1_S, spineBase1_S, c);
                AddQuad(spineBase1_S, spineTip1_S, spineTip1_E, spineBase1_E, c);
                AddQuad(spineBase1_E, spineTip1_E, spineTip1_N, spineBase1_N, c);
                AddQuad_Backward(spineTip1_N, spineTip1_W, spineTip1_S, spineTip1_E, c);

                AddQuad_Backward(spineBase2_N, spineTip2_N, spineTip2_W, spineBase2_W, c);
                AddQuad_Backward(spineBase2_W, spineTip2_W, spineTip2_S, spineBase2_S, c);
                AddQuad_Backward(spineBase2_S, spineTip2_S, spineTip2_E, spineBase2_E, c);
                AddQuad_Backward(spineBase2_E, spineTip2_E, spineTip2_N, spineBase2_N, c);
                AddQuad(spineTip2_N, spineTip2_W, spineTip2_S, spineTip2_E, c);

                Vector3 tip = Vector3.Forward + (Vector3.Forward * (extraThickness * 1.25f));

                for (int i = 1; i <= Segments; i++)
                {
                    var ptBase = GetPoint(i, StemRadius + extraThickness, 0 - extraThickness);

                    var prevPtBase = GetPoint(i - 1, StemRadius + extraThickness, 0 - extraThickness);

                    //Face: Part of Bottom
                    AddTri_Backward(Vector3.Zero + (Vector3.Backward * extraThickness), prevPtBase, ptBase, c);
                }

                for (int i = 1; i <= Segments; i++)
                {
                    var ptBase = GetPoint(i, StemRadius + extraThickness, 0 - extraThickness);
                    var ptTipStartInner = GetPoint(i, StemRadius + extraThickness, (1 - TipLength - extraThickness));

                    var prevPtBase = GetPoint(i - 1, StemRadius + extraThickness, 0 - extraThickness);
                    var prevPtTipStartInner = GetPoint(i - 1, StemRadius + extraThickness, (1 - TipLength - extraThickness));

                    // Face: Base to Tip Inner
                    AddQuad_Backward(prevPtBase, prevPtTipStartInner, ptTipStartInner, ptBase, c);
                }

                for (int i = 1; i <= Segments; i++)
                {
                    var ptTipStartInner = GetPoint(i, StemRadius + extraThickness, (1 - TipLength - extraThickness));
                    var ptTipStartOuter = GetPoint(i, TipRadius + extraThickness, (1 - TipLength - extraThickness));

                    var prevPtTipStartInner = GetPoint(i - 1, StemRadius + extraThickness, (1 - TipLength - extraThickness));
                    var prevPtTipStartOuter = GetPoint(i - 1, TipRadius + extraThickness, (1 - TipLength - extraThickness));

                    // Face: Tip Start Inner to Tip Start Outer
                    AddQuad_Backward(prevPtTipStartInner, prevPtTipStartOuter, ptTipStartOuter, ptTipStartInner, c);
                }

                for (int i = 1; i <= Segments; i++)
                {
                    var ptTipStartOuter = GetPoint(i, TipRadius + extraThickness, (1 - TipLength - extraThickness));
                    var prevPtTipStartOuter = GetPoint(i - 1, TipRadius + extraThickness, (1 - TipLength - extraThickness));

                    // Face: Tip Start to Tip
                    AddTri_Backward(prevPtTipStartOuter, tip, ptTipStartOuter, c);
                }

                FinalizeBuffers(true);

                GeometryData = new DbgPrimGeometryData()
                {
                    VertBuffer = VertBuffer,
                    IndexBuffer = IndexBuffer,
                };
            }
        }
    }
}
