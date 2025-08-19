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
            private static DbgPrimGeometryData GeometryData = null;

            public const int Segments = 12;

            public DbgPrimWireCapsule_End()
            {
                KeepBuffersAlive = true;
                //if (!(Segments >= 4))
                //    throw new ArgumentException($"Number of segments must be >= 4", nameof(Segments));

                if (GeometryData != null)
                {
                    SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
                }
                else
                {
                    var topPoint = Vector3.Up * 1;
                    var bottomPoint = Vector3.Down * 1;
                    var points = new Vector3[Segments, Segments];

                    int verticalSegments = Segments / 2;

                    for (int i = 0; i <= verticalSegments; i++)
                    {
                        for (int j = 0; j < Segments; j++)
                        {
                            float horizontalAngle = (1.0f * j / Segments) * MathHelper.TwoPi;
                            float verticalAngle = ((1.0f * (i) / (verticalSegments)) * MathHelper.PiOver2);
                            float altitude = (float)Math.Sin(verticalAngle);
                            float horizontalDist = (float)Math.Cos(verticalAngle);
                            points[i, j] = new Vector3((float)Math.Cos(horizontalAngle) * horizontalDist, altitude, (float)Math.Sin(horizontalAngle) * horizontalDist) * 1;
                        }
                    }

                    for (int i = 0; i <= verticalSegments; i++)
                    {
                        for (int j = 0; j < Segments; j++)
                        {
                            //// On the bottom, we must connect each to the bottom point
                            //if (i == 0)
                            //{
                            //    AddLine(points[i, j], bottomPoint, Color.White);
                            //}

                            // On the top, we must connect each point to the top
                            // Note: this isn't "else if" because with 2 segments, 
                            // these are both true for the only ring
                            if (i == Segments - 1)
                            {
                                AddLine(points[i, j], topPoint, Color.White);
                            }

                            // Make vertical lines that connect from this 
                            // horizontal ring to the one above
                            // Since we are connecting 
                            // (current) -> (the one above current)
                            // we dont need to do this for the very last one.
                            if (i < Segments - 1)
                            {
                                AddLine(points[i, j], points[i + 1, j], Color.White);
                            }


                            // Make lines that connect points horizontally
                            //---- if we reach end, we must wrap around, 
                            //---- otherwise, simply make line to next one
                            if (j == Segments - 1)
                            {
                                AddLine(points[i, j], points[i, 0], Color.White);
                            }
                            else
                            {
                                AddLine(points[i, j], points[i, j + 1], Color.White);
                            }
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

        public class DbgPrimWireCapsule_Middle : DbgPrimWire
        {
            private static DbgPrimGeometryData GeometryData = null;

            public const int Segments = 12;

            public DbgPrimWireCapsule_Middle()
            {
                KeepBuffersAlive = true;

                if (GeometryData != null)
                {
                    SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
                }
                else
                {
                    for (int i = 0; i < Segments; i++)
                    {
                        float horizontalAngle = (1.0f * i / Segments) * MathHelper.TwoPi;
                        Vector3 a = new Vector3((float)Math.Cos(horizontalAngle), 0, (float)Math.Sin(horizontalAngle));
                        Vector3 b = new Vector3(a.X, 1, a.Z);
                        AddLine(a, b, Color.White);
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

        public readonly DbgPrimWireCapsule_End HemisphereA;
        public readonly DbgPrimWireCapsule_Middle Midst;
        public readonly DbgPrimWireCapsule_End HemisphereB;

        private float elapsedTimeWhenABSet = -1;
        public Vector3 A;
        public Vector3 B;
        private Vector3 AC6Extend_LastValidForward = Vector3.Zero;

        public bool IsAC6ExtendFromMotionDir = true;

        public void UpdateCapsuleEndPoints_AC6Extend(Matrix location, ParamData.AtkParam.Hit hit, NewDummyPolyManager bodyDmy, NewDummyPolyManager dmy1)
        {
            float extendLength = hit.AC6_Extend_LengthStart;
            float extendRadius = hit.Radius;
            if (ElapsedTime > hit.AC6_Extend_SpreadDelay)
            {
                if (hit.AC6_Extend_RadiusEnd >= 0)
                {
                    if (hit.AC6_Extend_RadiusSpreadTime > 0)
                    {
                        float radiusSpreadRatio = (ElapsedTime - hit.AC6_Extend_SpreadDelay) / hit.AC6_Extend_RadiusSpreadTime;
                        if (radiusSpreadRatio > 1)
                            radiusSpreadRatio = 1;
                        extendRadius = hit.Radius + ((hit.AC6_Extend_RadiusEnd - hit.Radius) * radiusSpreadRatio);
                    }
                    else
                    {
                        extendRadius = hit.AC6_Extend_RadiusEnd;
                    }
                }

                if (hit.AC6_Extend_LengthEnd >= 0)
                {
                    if (hit.AC6_Extend_LengthSpreadTime > 0)
                    {
                        float lengthSpreadRatio = (ElapsedTime - hit.AC6_Extend_SpreadDelay) / hit.AC6_Extend_LengthSpreadTime;
                        if (lengthSpreadRatio > 1)
                            lengthSpreadRatio = 1;
                        extendLength = hit.AC6_Extend_LengthStart + ((hit.AC6_Extend_LengthEnd - hit.AC6_Extend_LengthStart) * lengthSpreadRatio);
                    }
                    else
                    {
                        extendLength = hit.AC6_Extend_LengthEnd;
                    }

                    
                }
            }



            var newA = Vector3.Transform(Vector3.Zero, location);
            Vector3 newB;

            if (IsAC6ExtendFromMotionDir && false)
            {
                var dirVector = newA - A;
                if (dirVector.X != 0 && dirVector.Y != 0 && dirVector.Z != 0)
                {
                    dirVector = Vector3.Normalize(dirVector);
                    AC6Extend_LastValidForward = dirVector;
                }
                else
                {
                    dirVector = AC6Extend_LastValidForward;
                }
                newB = newA + (dirVector * extendLength);
            }
            else
            {
                newB = Vector3.Transform(Vector3.Forward * extendLength, location);
            }

            UpdateCapsuleEndPoints(newA, newB, hit, bodyDmy, dmy1, dmy1, extendRadius);
        }

        public void UpdateCapsuleEndPoints_Simple(Vector3 a, Vector3 b, float radius)
        {
            float dist = (b - a).Length();

            var mtHemisphereA = Matrix.Identity;
            var mtMidst = Matrix.Identity;
            var mtHemisphereB = Matrix.Identity;

            //var radius = hit.Radius;


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

            Vector3 forward = Vector3.Forward;

            if (a != b)
                forward = -Vector3.Normalize(b - a);

            Matrix hitboxMatrix = Matrix.CreateWorld(a, forward, Vector3.Up);

            if (forward.X == 0 && forward.Z == 0)
            {
                if (forward.Y >= 0)
                    hitboxMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(a);
                else
                    hitboxMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(a);
            }

            //Matrix hitboxMatrix = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.Normalize(b - a), 0)) * Matrix.CreateTranslation(a);

            Transform = new Transform(hitboxMatrix);

            HemisphereA.OverrideColor = OverrideColor;
            HemisphereB.OverrideColor = OverrideColor;
            Midst.OverrideColor = OverrideColor;
        }

        public void UpdateCapsuleEndPoints(Vector3 a, Vector3 b, ParamData.AtkParam.Hit hit, NewDummyPolyManager bodyDmy, NewDummyPolyManager dmy1, NewDummyPolyManager dmy2, float radius)
        {
            if (elapsedTimeWhenABSet != ElapsedTime)
            {
                A = a;
                B = b;
                elapsedTimeWhenABSet = ElapsedTime;
            }
            if (hit.DummyPolySourceSpawnedOn != ParamData.AtkParam.DummyPolySource.BaseModel)
            {
                if (hit.DummyPolySourceSpawnedOn >= ParamData.AtkParam.DummyPolySource.AC6Parts0 && 
                    hit.DummyPolySourceSpawnedOn <= ParamData.AtkParam.DummyPolySource.AC6Parts31)
                {
                    if (dmy1 == bodyDmy && bodyDmy.MODEL?.AC6NpcParts != null)

                        bodyDmy.MODEL.AC6NpcParts.AccessModelOfPart((int)(hit.DummyPolySourceSpawnedOn - ParamData.AtkParam.DummyPolySource.AC6Parts0),
                            (p, m) =>
                            {
                                a = Vector3.Transform(a, bodyDmy.MODEL.CurrentTransform.WorldMatrix
                                    * Matrix.Invert(m.CurrentTransform.WorldMatrix));
                            });

                        

                    if (dmy2 == bodyDmy && bodyDmy.MODEL?.AC6NpcParts != null)
                            bodyDmy.MODEL.AC6NpcParts.AccessModelOfPart((int)(hit.DummyPolySourceSpawnedOn - ParamData.AtkParam.DummyPolySource.AC6Parts0),
                            (p, m) =>
                            {
                                b = Vector3.Transform(a, bodyDmy.MODEL.CurrentTransform.WorldMatrix
                                    * Matrix.Invert(m.CurrentTransform.WorldMatrix));
                            });
                }
                else
                {
                    if (dmy1 == bodyDmy)

                        a = Vector3.Transform(a, bodyDmy.MODEL.CurrentTransform.WorldMatrix 
                            * Matrix.Invert(bodyDmy.MODEL.ChrAsm.GetDummyManager(hit.DummyPolySourceSpawnedOn).MODEL.CurrentTransform.WorldMatrix));

                    if (dmy2 == bodyDmy)
                        b = Vector3.Transform(b, bodyDmy.MODEL.CurrentTransform.WorldMatrix 
                            * Matrix.Invert(bodyDmy.MODEL.ChrAsm.GetDummyManager(hit.DummyPolySourceSpawnedOn).MODEL.CurrentTransform.WorldMatrix));
                }

                
            }

            DmyPolySource = hit.DummyPolySourceSpawnedOn;

            UpdateCapsuleEndPoints_Simple(a, b, radius);
        }

        

        public DbgPrimWireCapsule(Color color)
        {
            KeepBuffersAlive = true;

            HemisphereA = new DbgPrimWireCapsule_End()
            {
                OverrideColor = color
            };
            Midst = new DbgPrimWireCapsule_Middle()
            {
                OverrideColor = color
            };
            HemisphereB = new DbgPrimWireCapsule_End()
            {
                OverrideColor = color
            };

            Children.Add(HemisphereA);
            Children.Add(Midst);
            Children.Add(HemisphereB);
        }
    }
}
