using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsAssetPipeline
{
    public static class SapMath
    {
        public static System.Numerics.Vector3 XYZ(this System.Numerics.Vector4 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        public static float Lerp(float a, float b, float s)
        {
            return a + ((b - a) * s);
        }
    }
}
