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
        public const int Segments = 12;
        public const float TipLength = 0.25f;
        public const float TipRadius = 0.25f;
        public const float StemRadius = 0.1f;

        private static DbgPrimGeometryData GeometryData = null;

        public DbgPrimSolidArrow(string name, Transform location, Color color)
        {
            KeepBuffersAlive = true;
            //BackfaceCulling = false;

            Category = DbgPrimCategory.HkxBone;

            Transform = location;
            NameColor = color;
            Name = name;

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

                Vector3 tip = Vector3.Forward;

                for (int i = 1; i <= Segments; i++)
                {
                    var ptBase = GetPoint(i, StemRadius, 0);

                    var prevPtBase = GetPoint(i - 1, StemRadius, 0);

                    //Face: Part of Bottom
                    AddTri(Vector3.Zero, prevPtBase, ptBase, color);
                }

                for (int i = 1; i <= Segments; i++)
                {
                    var ptBase = GetPoint(i, StemRadius, 0);
                    var ptTipStartInner = GetPoint(i, StemRadius, 1 - TipLength);

                    var prevPtBase = GetPoint(i - 1, StemRadius, 0);
                    var prevPtTipStartInner = GetPoint(i - 1, StemRadius, 1 - TipLength);

                    // Face: Base to Tip Inner
                    AddQuad(prevPtBase, prevPtTipStartInner, ptTipStartInner, ptBase, color);
                }

                for (int i = 1; i <= Segments; i++)
                {
                    var ptTipStartInner = GetPoint(i, StemRadius, 1 - TipLength);
                    var ptTipStartOuter = GetPoint(i, TipRadius, 1 - TipLength);

                    var prevPtTipStartInner = GetPoint(i - 1, StemRadius, 1 - TipLength);
                    var prevPtTipStartOuter = GetPoint(i - 1, TipRadius, 1 - TipLength);

                    // Face: Tip Start Inner to Tip Start Outer
                    AddQuad(prevPtTipStartInner, prevPtTipStartOuter, ptTipStartOuter, ptTipStartInner, color);
                }

                for (int i = 1; i <= Segments; i++)
                {
                    var ptTipStartOuter = GetPoint(i, TipRadius, 1 - TipLength);
                    var prevPtTipStartOuter = GetPoint(i - 1, TipRadius, 1 - TipLength);

                    // Face: Tip Start to Tip
                    AddTri(prevPtTipStartOuter, tip, ptTipStartOuter, color);
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
