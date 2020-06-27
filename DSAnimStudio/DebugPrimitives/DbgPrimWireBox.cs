using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWireBox : DbgPrimWire
    {
        public Vector3 LocalMax = Vector3.One / 2;
        public Vector3 LocalMin = -Vector3.One / 2;

        public void UpdateTransform(Transform newTransform)
        {
            Vector3 center = ((LocalMax + LocalMin) / 2);
            Vector3 size = (LocalMax - LocalMin) / 2;

            Transform = new Transform(Matrix.CreateScale(size) 
                * Matrix.CreateTranslation(center) 
                * newTransform.WorldMatrix);
        }

        private static DbgPrimGeometryData GeometryData = null;

        public DbgPrimWireBox(Transform location, Vector3 localMin, Vector3 localMax, Color color)
        {
            KeepBuffersAlive = true;

            Transform = location;
            NameColor = color;
            OverrideColor = color;

            LocalMin = localMin;
            LocalMax = localMax;

            if (GeometryData != null)
            {
                SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
            }
            else
            {
                var min = -Vector3.One;
                var max = Vector3.One;

                // 3 Letters of below names: 
                // [T]op/[B]ottom, [F]ront/[B]ack, [L]eft/[R]ight
                var tfl = new Vector3(min.X, max.Y, max.Z);
                var tfr = new Vector3(max.X, max.Y, max.Z);
                var bfr = new Vector3(max.X, min.Y, max.Z);
                var bfl = new Vector3(min.X, min.Y, max.Z);
                var tbl = new Vector3(min.X, max.Y, min.Z);
                var tbr = new Vector3(max.X, max.Y, min.Z);
                var bbr = new Vector3(max.X, min.Y, min.Z);
                var bbl = new Vector3(min.X, min.Y, min.Z);

                // Top Face
                AddLine(tfl, tfr);
                AddLine(tfr, tbr);
                AddLine(tbr, tbl);
                AddLine(tbl, tfl);

                // Bottom Face
                AddLine(bfl, bfr);
                AddLine(bfr, bbr);
                AddLine(bbr, bbl);
                AddLine(bbl, bfl);

                // Four Vertical Pillars
                AddLine(bfl, tfl);
                AddLine(bfr, tfr);
                AddLine(bbl, tbl);
                AddLine(bbr, tbr);

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
