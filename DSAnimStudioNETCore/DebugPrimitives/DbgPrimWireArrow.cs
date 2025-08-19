using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireArrow : DbgPrimWire
    {
        public const int Segments = 6;

        public const float TipLength = 0.25f;
        public const float TipRadius = 0.10f;
        public const float StemRadius = 0.04f;

        public const float OrientingSpinesLength = 0.5f;

        private static DbgPrimGeometryData GeometryData = null;

        DbgPrimSolidArrow ExtraVisibilityPrim = null;

        public static bool ForceEnableExtraVisibilityOnAll = false;
        public bool EnableExtraVisibility = false;
        public bool ExtraVisibilityIgnoresAlpha = false;

        //public float ExtraVisibilityThickness = 1.2f;
        //public static float GlobalExtraVisibilityThicknessMult = 1;

        protected override void PreDraw(bool forceDraw, IDbgPrim parentPrim, Matrix world)
        {
            if (EnableExtraVisibility || ForceEnableExtraVisibilityOnAll)
            {
                //float scale = ExtraVisibilityThickness * GlobalExtraVisibilityThicknessMult;
                ExtraVisibilityPrim.Transform = Transform;// new Transform(Matrix.CreateScale(scale, scale, 1) * Transform.WorldMatrix);
                ExtraVisibilityPrim.BackfaceCulling = false;
                ExtraVisibilityPrim.DisableLighting = true;
                ExtraVisibilityPrim.OverrideColor = Color.Black * (ExtraVisibilityIgnoresAlpha ? 1 : ((float)(OverrideColor?.A ?? 255) / 255f));
                ExtraVisibilityPrim.Wireframe = false;
                ExtraVisibilityPrim.BackfaceCulling = true;
                ExtraVisibilityPrim.Draw(forceDraw, parentPrim, world);
            }
        }

        public DbgPrimWireArrow(Transform location, Color color)
        {
            ExtraVisibilityPrim = new DbgPrimSolidArrow(location, color, 0.08f);

            KeepBuffersAlive = true;
            Transform = location;

            if (GeometryData != null)
            {
                SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
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

                AddLine(Vector3.Zero, Vector3.Up * OrientingSpinesLength);
                AddLine(Vector3.Zero, -Vector3.Right * OrientingSpinesLength * 0.5f);

                Vector3 tip = Vector3.Forward;

                for (int i = 0; i <= Segments; i++)
                {
                    var ptBase = GetPoint(i, StemRadius, 0);
                    var ptTipStartInner = GetPoint(i, StemRadius, 1 - TipLength);
                    var ptTipStartOuter = GetPoint(i, TipRadius, 1 - TipLength);

                    AddLine(ptBase, ptTipStartInner, color);
                    AddLine(ptTipStartInner, ptTipStartOuter, color);
                    AddLine(ptTipStartOuter, tip, color);

                    // Connect this vertical spline to the previous one with horizontal lines.
                    if (i > 0)
                    {
                        var prevPtBase = GetPoint(i - 1, StemRadius, 0);
                        var prevPtTipStartInner = GetPoint(i - 1, StemRadius, 1 - TipLength);
                        var prevPtTipStartOuter = GetPoint(i - 1, TipRadius, 1 - TipLength);

                        AddLine(prevPtBase, ptBase, color);
                        AddLine(prevPtTipStartInner, ptTipStartInner, color);
                        AddLine(prevPtTipStartOuter, ptTipStartOuter, color);
                    }
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
