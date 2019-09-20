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
        public DbgPrimWireBox(Transform location, Vector3 size, Color color)
            : this(location, size, color, color)
        {

        }

        public DbgPrimWireBox(Transform location, Vector3 min, Vector3 max, Color color)
        {
            Transform = location;
            NameColor = color;

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
            AddLine(tfl, tfr, color);
            AddLine(tfr, tbr, color);
            AddLine(tbr, tbl, color);
            AddLine(tbl, tfl, color);

            // Bottom Face
            AddLine(bfl, bfr, color);
            AddLine(bfr, bbr, color);
            AddLine(bbr, bbl, color);
            AddLine(bbl, bfl, color);

            // Four Vertical Pillars
            AddLine(bfl, tfl, color, color);
            AddLine(bfr, tfr, color, color);
            AddLine(bbl, tbl, color, color);
            AddLine(bbr, tbr, color, color);
        }

        public DbgPrimWireBox(Transform location, Vector3 size, Color topColor, Color bottomColor)
        {
            Transform = location;
            NameColor = new Color((topColor.ToVector4() + bottomColor.ToVector4()) / 2);

            Vector3 max = size / 2;
            Vector3 min = -max;

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
            AddLine(tfl, tfr, topColor);
            AddLine(tfr, tbr, topColor);
            AddLine(tbr, tbl, topColor);
            AddLine(tbl, tfl, topColor);

            // Bottom Face
            AddLine(bfl, bfr, bottomColor);
            AddLine(bfr, bbr, bottomColor);
            AddLine(bbr, bbl, bottomColor);
            AddLine(bbl, bfl, bottomColor);

            // Four Vertical Pillars
            AddLine(bfl, tfl, bottomColor, topColor);
            AddLine(bfr, tfr, bottomColor, topColor);
            AddLine(bbl, tbl, bottomColor, topColor);
            AddLine(bbr, tbr, bottomColor, topColor);
        }
    }
}
