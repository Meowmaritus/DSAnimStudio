﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireCylinder : DbgPrimWire
    {
        public DbgPrimWireCylinder(Transform location, float range, float height, int numSegments, Color color)
        {
            Transform = location;

            float top = height / 2;
            float bottom = -top;

            for (int i = 0; i < numSegments; i++)
            {
                float angle = (1.0f * i / numSegments) * MathHelper.TwoPi;
                float x = (float)Math.Cos(angle) * range;
                float z = (float)Math.Sin(angle) * range;
                //Very last one wraps around to the first one
                if (i == numSegments - 1)
                {
                    float x_next = (float)Math.Cos(0);
                    float z_next = (float)Math.Sin(0);
                    // Create line to wrap around to first point
                    //---- Top
                    AddLine(new Vector3(x, top, z), new Vector3(x_next, top, z_next), color);
                    //---- Bottom
                    AddLine(new Vector3(x, bottom, z), new Vector3(x_next, bottom, z_next), color);
                }
                else
                {
                    float angle_next = (1.0f * (i + 1) / numSegments) * MathHelper.TwoPi;
                    float x_next = (float)Math.Cos(angle_next);
                    float z_next = (float)Math.Sin(angle_next);

                    // Create line to next point in ring
                    //---- Top
                    AddLine(new Vector3(x, top, z), new Vector3(x_next, top, z_next), color);
                    //---- Bottom
                    AddLine(new Vector3(x, bottom, z), new Vector3(x_next, bottom, z_next), color);
                }

                // Make pillar from top to bottom
                AddLine(new Vector3(x, bottom, z), new Vector3(x, top, z), color);
            }
        }
    }
}
