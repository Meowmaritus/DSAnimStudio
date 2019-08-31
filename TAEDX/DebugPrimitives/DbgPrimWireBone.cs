using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.DebugPrimitives
{
    public class DbgPrimWireBone : DbgPrimWire
    {
        public DbgPrimWireBone(string name, Transform location, Quaternion rotation, float thickness, float length, Color color)
        {
            Category = DbgPrimCategory.HkxBone;

            Transform = location;
            NameColor = color;
            Name = name;

            thickness = Math.Min(length / 2, thickness);

            Vector3 pt_start = Vector3.Zero;
            Vector3 pt_cardinal1 = Vector3.Up * thickness + Vector3.Right * thickness;
            Vector3 pt_cardinal2 = Vector3.Forward * thickness + Vector3.Right * thickness;
            Vector3 pt_cardinal3 = Vector3.Down * thickness + Vector3.Right * thickness;
            Vector3 pt_cardinal4 = Vector3.Backward * thickness + Vector3.Right * thickness;
            Vector3 pt_tip = Vector3.Right * length;

            pt_start = Vector3.Transform(pt_start, Matrix.CreateFromQuaternion(rotation));
            pt_cardinal1 = Vector3.Transform(pt_cardinal1, Matrix.CreateFromQuaternion(rotation));
            pt_cardinal2 = Vector3.Transform(pt_cardinal2, Matrix.CreateFromQuaternion(rotation));
            pt_cardinal3 = Vector3.Transform(pt_cardinal3, Matrix.CreateFromQuaternion(rotation));
            pt_cardinal4 = Vector3.Transform(pt_cardinal4, Matrix.CreateFromQuaternion(rotation));
            pt_tip = Vector3.Transform(pt_tip, Matrix.CreateFromQuaternion(rotation));

            //Start to cardinals
            AddLine(pt_start, pt_cardinal1, color);
            AddLine(pt_start, pt_cardinal2, color);
            AddLine(pt_start, pt_cardinal3, color);
            AddLine(pt_start, pt_cardinal4, color);

            //Cardinals to end
            AddLine(pt_cardinal1, pt_tip, color);
            AddLine(pt_cardinal2, pt_tip, color);
            AddLine(pt_cardinal3, pt_tip, color);
            AddLine(pt_cardinal4, pt_tip, color);

            //Connecting the cardinals
            AddLine(pt_cardinal1, pt_cardinal2, color);
            AddLine(pt_cardinal2, pt_cardinal3, color);
            AddLine(pt_cardinal3, pt_cardinal4, color);
            AddLine(pt_cardinal4, pt_cardinal1, color);
        }
    }
}
