using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimSolidBone : DbgPrimSolid
    {
        public DbgPrimSolidBone(bool isHkx, string name, Transform location, Quaternion rotation, float thickness, float length, Color color)
        {
            Category = isHkx ? DbgPrimCategory.HkxBone : DbgPrimCategory.FlverBone;

            Transform = location;
            NameColor = color;
            Name = name;

            thickness = Math.Min(length / 2, thickness);

            Vector3 start = Vector3.Zero;
            Vector3 a = Vector3.Up * thickness + Vector3.Right * thickness;
            Vector3 b = Vector3.Forward * thickness + Vector3.Right * thickness;
            Vector3 c = Vector3.Down * thickness + Vector3.Right * thickness;
            Vector3 d = Vector3.Backward * thickness + Vector3.Right * thickness;
            Vector3 end = Vector3.Right * length;

            start = Vector3.Transform(start, Matrix.CreateFromQuaternion(rotation));
            a = Vector3.Transform(a, Matrix.CreateFromQuaternion(rotation));
            b = Vector3.Transform(b, Matrix.CreateFromQuaternion(rotation));
            c = Vector3.Transform(c, Matrix.CreateFromQuaternion(rotation));
            d = Vector3.Transform(d, Matrix.CreateFromQuaternion(rotation));
            end = Vector3.Transform(end, Matrix.CreateFromQuaternion(rotation));

            AddTri(start, a, b, color);
            AddTri(start, b, c, color);
            AddTri(start, c, d, color);
            AddTri(start, d, a, color);

            AddTri(end, b, a, color);
            AddTri(end, c, b, color);
            AddTri(end, d, c, color);
            AddTri(end, a, d, color);
        }
    }
}
