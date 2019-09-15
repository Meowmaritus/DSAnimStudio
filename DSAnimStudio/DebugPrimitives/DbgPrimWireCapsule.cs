using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireCapsule : DbgPrimWire
    {
        public class DbgPrimWireCapsule_End : DbgPrimWire
        {
            public DbgPrimWireCapsule_End(int segments)
            {
                if (!(segments >= 4))
                    throw new ArgumentException($"Number of segments must be >= 4", nameof(segments));

                var topPoint = Vector3.Up * 1;
                var bottomPoint = Vector3.Down * 1;
                var points = new Vector3[segments, segments];

                int verticalSegments = segments / 2;

                for (int i = 0; i <= verticalSegments; i++)
                {
                    for (int j = 0; j < segments; j++)
                    {
                        float horizontalAngle = (1.0f * j / segments) * MathHelper.TwoPi;
                        float verticalAngle = ((1.0f * (i) / (verticalSegments)) * MathHelper.PiOver2);
                        float altitude = (float)Math.Sin(verticalAngle);
                        float horizontalDist = (float)Math.Cos(verticalAngle);
                        points[i, j] = new Vector3((float)Math.Cos(horizontalAngle) * horizontalDist, altitude, (float)Math.Sin(horizontalAngle) * horizontalDist) * 1;
                    }
                }

                for (int i = 0; i <= verticalSegments; i++)
                {
                    for (int j = 0; j < segments; j++)
                    {
                        //// On the bottom, we must connect each to the bottom point
                        //if (i == 0)
                        //{
                        //    AddLine(points[i, j], bottomPoint, Color.White);
                        //}

                        // On the top, we must connect each point to the top
                        // Note: this isn't "else if" because with 2 segments, 
                        // these are both true for the only ring
                        if (i == segments - 1)
                        {
                            AddLine(points[i, j], topPoint, Color.White);
                        }

                        // Make vertical lines that connect from this 
                        // horizontal ring to the one above
                        // Since we are connecting 
                        // (current) -> (the one above current)
                        // we dont need to do this for the very last one.
                        if (i < segments - 1)
                        {
                            AddLine(points[i, j], points[i + 1, j], Color.White);
                        }


                        // Make lines that connect points horizontally
                        //---- if we reach end, we must wrap around, 
                        //---- otherwise, simply make line to next one
                        if (j == segments - 1)
                        {
                            AddLine(points[i, j], points[i, 0], Color.White);
                        }
                        else
                        {
                            AddLine(points[i, j], points[i, j + 1], Color.White);
                        }
                    }
                }


            }
        }

        public class DbgPrimWireCapsule_Middle : DbgPrimWire
        {
            public DbgPrimWireCapsule_Middle(int segments)
            {
                for (int i = 0; i < segments; i++)
                {
                    float horizontalAngle = (1.0f * i / segments) * MathHelper.TwoPi;
                    Vector3 a = new Vector3((float)Math.Cos(horizontalAngle), 0, (float)Math.Sin(horizontalAngle));
                    Vector3 b = new Vector3(a.X, 1, a.Z);
                    AddLine(a, b, Color.White);
                }
            }
        }

        public readonly DbgPrimWireCapsule_End HemisphereA;
        public readonly DbgPrimWireCapsule_Middle Midst;
        public readonly DbgPrimWireCapsule_End HemisphereB;

        public void UpdateCapsuleEndPoints(Vector3 a, Vector3 b, float radius)
        {
            float dist = (b - a).Length();

            var mtHemisphereA = Matrix.Identity;
            var mtMidst = Matrix.Identity;
            var mtHemisphereB = Matrix.Identity;

            mtHemisphereA *= Matrix.CreateScale(Vector3.One * radius);
            mtHemisphereA *= Matrix.CreateRotationX(-MathHelper.PiOver2);

            mtMidst *= Matrix.CreateScale(new Vector3(radius, dist, radius));
            mtMidst *= Matrix.CreateRotationX(MathHelper.PiOver2);

            mtHemisphereB *= Matrix.CreateScale(Vector3.One * radius);
            mtHemisphereB *= Matrix.CreateRotationX(MathHelper.PiOver2);
            mtHemisphereB *= Matrix.CreateTranslation(new Vector3(0, 0, dist));

            HemisphereA.Transform = new Transform(mtHemisphereA);
            Midst.Transform = new Transform(mtMidst);
            HemisphereB.Transform = new Transform(mtHemisphereB);

            Matrix hitboxMatrix = Matrix.CreateWorld(a, -Vector3.Normalize(b - a), Vector3.Up);

            //Matrix hitboxMatrix = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.Normalize(b - a), 0)) * Matrix.CreateTranslation(a);

            Transform = new Transform(hitboxMatrix);
        }

        public DbgPrimWireCapsule(int segments, Color color)
        {
            HemisphereA = new DbgPrimWireCapsule_End(segments)
            {
                Category = DbgPrimCategory.DummyPolyHelper,
                OverrideColor = color
            };
            Midst = new DbgPrimWireCapsule_Middle(segments)
            {
                Category = DbgPrimCategory.DummyPolyHelper,
                OverrideColor = color
            };
            HemisphereB = new DbgPrimWireCapsule_End(segments)
            {
                Category = DbgPrimCategory.DummyPolyHelper,
                OverrideColor = color
            };

            Children.Add(HemisphereA);
            Children.Add(Midst);
            Children.Add(HemisphereB);
        }
    }
}
