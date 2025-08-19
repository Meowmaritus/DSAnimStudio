using HKX2;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Vector4 = Microsoft.Xna.Framework.Vector4;

namespace DSAnimStudio
{
    public static class XnaExtensions
    {
        //public static Quaternion QuatFromDirectionVector(Vector3 direction)
        //{
        //    if (direction == Vector3.Up)
        //}

        public static Matrix ExtractTranslationAndHeading(this Matrix m)
        {
            var root = Vector3.Transform(Vector3.Zero, m);
            var forward = Vector3.Transform(Vector3.Forward, m);
            var direction = Vector3.Normalize(forward - root);
            float headingAngle = MathF.Atan2(-direction.X, -direction.Z);
            return Matrix.CreateRotationY(headingAngle) * Matrix.CreateTranslation(root);
        }

        public static float ExtractHeading(this Matrix m)
        {
            var root = Vector3.Transform(Vector3.Zero, m);
            var forward = Vector3.Transform(Vector3.Forward, m);
            var direction = Vector3.Normalize(forward - root);
            return MathF.Atan2(-direction.X, -direction.Z);
        }

        public static Color MultiplyAlpha(this Color c, float mult)
        {
            float r = c.R / 255f;
            float g = c.G / 255f;
            float b = c.B / 255f;
            float a = c.A / 255f;
            return new Color(r, g, b, a * mult);
        }

        public static Vector3 GetHSL(this Color c)
        {
            float _R = (c.R / 255f);
            float _G = (c.G / 255f);
            float _B = (c.B / 255f);

            float _Min = Math.Min(Math.Min(_R, _G), _B);
            float _Max = Math.Max(Math.Max(_R, _G), _B);
            float _Delta = _Max - _Min;

            float H = 0;
            float S = 0;
            float L = (float)((_Max + _Min) / 2.0f);

            if (_Delta != 0)
            {
                if (L < 0.5f)
                {
                    S = (float)(_Delta / (_Max + _Min));
                }
                else
                {
                    S = (float)(_Delta / (2.0f - _Max - _Min));
                }


                if (_R == _Max)
                {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max)
                {
                    H = 2f + (_B - _R) / _Delta;
                }
                else if (_B == _Max)
                {
                    H = 4f + (_R - _G) / _Delta;
                }
            }

            return new Vector3(H, S, L);
        }
        
        public static bool ApproxEquals(this float f, float other, float precision = 0.001f)
        {
            // Test
            return f == other;
            //return Math.Abs(f - other) <= precision;
        }
        
        public static uint GetImguiPackedValue(this Color c)
        {
            return new Color(new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f) * (c.A / 255f)).PackedValue;
        }

        public static bool HasAnyNaN(this Matrix m)
        {
            return float.IsNaN(m.M11) || float.IsNaN(m.M12) || float.IsNaN(m.M13) || float.IsNaN(m.M14) ||
                float.IsNaN(m.M21) || float.IsNaN(m.M22) || float.IsNaN(m.M23) || float.IsNaN(m.M24) ||
                float.IsNaN(m.M31) || float.IsNaN(m.M32) || float.IsNaN(m.M33) || float.IsNaN(m.M34) ||
                float.IsNaN(m.M41) || float.IsNaN(m.M42) || float.IsNaN(m.M43) || float.IsNaN(m.M44);
        }

        public static bool HasAnyInfinity(this Matrix m)
        {
            return float.IsInfinity(m.M11) || float.IsInfinity(m.M12) || float.IsInfinity(m.M13) || float.IsInfinity(m.M14) ||
                float.IsInfinity(m.M21) || float.IsInfinity(m.M22) || float.IsInfinity(m.M23) || float.IsInfinity(m.M24) ||
                float.IsInfinity(m.M31) || float.IsInfinity(m.M32) || float.IsInfinity(m.M33) || float.IsInfinity(m.M34) ||
                float.IsInfinity(m.M41) || float.IsInfinity(m.M42) || float.IsInfinity(m.M43) || float.IsInfinity(m.M44);
        }

        public static Matrix GetXnaMatrix(this NewBlendableTransform t)
        {
            return t.GetMatrix().ToXna();
        }

        public static Matrix GetXnaMatrixScale(this NewBlendableTransform t)
        {
            return t.GetMatrixScale().ToXna();
        }

        public static Matrix GetXnaMatrixFull(this NewBlendableTransform t)
        {
            return t.GetMatrixFull().ToXna();
        }

        public static NewBlendableTransform ToNewBlendableTransform(this Matrix m)
        {
            if (m.Decompose(out Vector3 aS, out Quaternion aR, out Vector3 aT))
            {
                return new NewBlendableTransform(aT.ToCS(), aS.ToCS(), Quaternion.Normalize(aR).ToCS());
            }
            
            return NewBlendableTransform.Identity;
        }
        
        public static NewBlendableTransform ToNewBlendableTransform(this Matrix4x4 m)
        {
            if (Matrix4x4.Decompose(m, out System.Numerics.Vector3 aS, out System.Numerics.Quaternion aR, out System.Numerics.Vector3 aT))
            {
                return new NewBlendableTransform(aT, aS, System.Numerics.Quaternion.Normalize(aR));
            }
            
            return NewBlendableTransform.Identity;
        }

        public static Vector2 RoundInt(this Vector2 v) => Vector2.Round(v);

        public static Vector3 Vector3Slerp(Vector3 start, Vector3 end, float percent)
        {
            // Dot product - the cosine of the angle between 2 vectors.
            float dot = Vector3.Dot(start, end);
            // Clamp it to be in the range of Acos()
            // This may be unnecessary, but floating point
            // precision can be a fickle mistress.
            dot = MathHelper.Clamp(dot, -1.0f, 1.0f);
            // Acos(dot) returns the angle between start and end,
            // And multiplying that by percent returns the angle between
            // start and the final result.
            float theta = (float)(Math.Acos(dot) * percent);
            Vector3 RelativeVec = end - start * dot;
            RelativeVec.Normalize();
            // Orthonormal basis
            // The final result.
            return ((start * (float)Math.Cos(theta)) + (RelativeVec * (float)Math.Sin(theta)));
        }

        public static System.Numerics.Vector4 ToNVector4(this Color c)
        {
            return new System.Numerics.Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }

        public static Rectangle DpiScaled(this Rectangle rect)
        {
            return new Rectangle(
                (int)Math.Round(rect.X * Main.DPI),
                (int)Math.Round(rect.Y * Main.DPI),
                (int)Math.Round(rect.Width * Main.DPI),
                (int)Math.Round(rect.Height * Main.DPI));
        }

        public static System.Drawing.Rectangle DpiScaled(this System.Drawing.Rectangle rect)
        {
            return new System.Drawing.Rectangle(
                (int)Math.Round(rect.X * Main.DPI),
                (int)Math.Round(rect.Y * Main.DPI),
                (int)Math.Round(rect.Width * Main.DPI),
                (int)Math.Round(rect.Height * Main.DPI));
        }

        public static RectF DpiScaled(this RectF rect)
        {
            return new RectF(
                rect.X * Main.DPI,
                rect.Y * Main.DPI,
                rect.Width * Main.DPI,
                rect.Height * Main.DPI);
        }

        public static Rectangle InverseDpiScaled(this Rectangle rect)
        {
            return new Rectangle(
                (int)Math.Round(rect.X / Main.DPI),
                (int)Math.Round(rect.Y / Main.DPI),
                (int)Math.Round(rect.Width / Main.DPI),
                (int)Math.Round(rect.Height / Main.DPI));
        }

        public static System.Drawing.Rectangle InverseDpiScaled(this System.Drawing.Rectangle rect)
        {
            return new System.Drawing.Rectangle(
                (int)Math.Round(rect.X / Main.DPI),
                (int)Math.Round(rect.Y / Main.DPI),
                (int)Math.Round(rect.Width / Main.DPI),
                (int)Math.Round(rect.Height / Main.DPI));
        }

        public static RectF InverseDpiScaled(this RectF rect)
        {
            return new RectF(
                rect.X / Main.DPI,
                rect.Y / Main.DPI,
                rect.Width / Main.DPI,
                rect.Height / Main.DPI);
        }

        public static Rectangle DpiScaledExcludePos(this Rectangle rect)
        {
            return new Rectangle(
                rect.X,
                rect.Y,
                (int)Math.Round(rect.Width * Main.DPI),
                (int)Math.Round(rect.Height * Main.DPI));
        }

        public static System.Drawing.Rectangle DpiScaledExcludePos(this System.Drawing.Rectangle rect)
        {
            return new System.Drawing.Rectangle(
                rect.X,
                rect.Y,
                (int)Math.Round(rect.Width * Main.DPI),
                (int)Math.Round(rect.Height * Main.DPI));
        }

        public static Rectangle InverseDpiScaledExcludePos(this Rectangle rect)
        {
            return new Rectangle(
                rect.X,
                rect.Y,
                (int)Math.Round(rect.Width / Main.DPI),
                (int)Math.Round(rect.Height / Main.DPI));
        }

        public static System.Drawing.Rectangle InverseDpiScaledExcludePos(this System.Drawing.Rectangle rect)
        {
            return new System.Drawing.Rectangle(
                rect.X,
                rect.Y,
                (int)Math.Round(rect.Width / Main.DPI),
                (int)Math.Round(rect.Height / Main.DPI));
        }

        public static Quaternion QuatLookRotation(Vector3 forward, Vector3 up)
        {
            forward = Vector3.Normalize(forward);
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
            up = Vector3.Cross(forward, right);
            var m00 = right.X;
            var m01 = right.Y;
            var m02 = right.Z;
            var m10 = up.X;
            var m11 = up.Y;
            var m12 = up.Z;
            var m20 = forward.X;
            var m21 = forward.Y;
            var m22 = forward.Z;


            float num8 = (m00 + m11) + m22;
            var quaternion = new Quaternion();
            if (num8 > 0f)
            {
                var num = (float)System.Math.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float)System.Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                var num6 = (float)System.Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10 + m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion;
            }
            var num5 = (float)System.Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }

        public static Quaternion QuatFromToRotation(Vector3 fromDirection, Vector3 toDirection)
        {
            var fromQuat = QuatLookRotation(fromDirection, Vector3.Up);
            var toQuat = QuatLookRotation(toDirection, Vector3.Up);
            return toQuat * Quaternion.Inverse(fromQuat);
        }

        
        public static Vector3 GetLerpValARequiredForResult(Vector3 b, Vector3 r, float s)
        {
            return new Vector3(GetLerpValARequiredForResult(b.X, r.X, s),
                GetLerpValARequiredForResult(b.Y, r.Y, s),
                GetLerpValARequiredForResult(b.Z, r.Z, s));
        }

        // r = Lerp(a, b, s)
        // This returns 'a' given r, b, and s
        public static float GetLerpValARequiredForResult(float b, float r, float s)
        {
            return (r - (b * s)) / (1 - s);
        }

        //public static Quaternion GetQuatOfBladePosDelta(NewDummyPolyManager.DummyPolyBladePos fromPos, NewDummyPolyManager.DummyPolyBladePos toPos)
        //{
        //    return QuatFromToRotation(fromPos.Start - fromPos.End, toPos.Start - toPos.End);
        //}

        public static Microsoft.Xna.Framework.Matrix ToXna(this System.Numerics.Matrix4x4 matrix)
        {
            return new Microsoft.Xna.Framework.Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        public static System.Numerics.Matrix4x4 ToCS(this Matrix matrix)
        {
            return new System.Numerics.Matrix4x4(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44
            );
        }

        public static Microsoft.Xna.Framework.Vector2 ToXna(this System.Numerics.Vector2 vector)
        {
            return new Microsoft.Xna.Framework.Vector2(vector.X, vector.Y);
        }

        public static Microsoft.Xna.Framework.Vector3 ToXna(this System.Numerics.Vector3 vector)
        {
            return new Microsoft.Xna.Framework.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static Microsoft.Xna.Framework.Vector4 ToXna(this System.Numerics.Vector4 vector)
        {
            return new Microsoft.Xna.Framework.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static Color PackToXnaColor(this System.Numerics.Vector4 vector)
        {
            return new Color(vector.X, vector.Y, vector.Z, vector.W);
        }


        public static Microsoft.Xna.Framework.Quaternion ToXna(this System.Numerics.Quaternion quaternion)
        {
            return new Microsoft.Xna.Framework.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static System.Numerics.Vector2 ToCS(this Microsoft.Xna.Framework.Vector2 vector)
        {
            return new System.Numerics.Vector2(vector.X, vector.Y);
        }

        public static System.Numerics.Vector3 ToCS(this Microsoft.Xna.Framework.Vector3 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static System.Numerics.Vector4 ToCS(this Microsoft.Xna.Framework.Vector4 vector)
        {
            return new System.Numerics.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static System.Numerics.Quaternion ToCS(this Microsoft.Xna.Framework.Quaternion quaternion)
        {
            return new System.Numerics.Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        public static System.Numerics.Vector3 ToVector3(this System.Numerics.Vector4 vector)
        {
            return new System.Numerics.Vector3(vector.X, vector.Y, vector.Z);
        }

        public static System.Numerics.Quaternion ToQuat(this System.Numerics.Vector4 vector)
        {
            return new System.Numerics.Quaternion(vector.X, vector.Y, vector.Z, vector.W);
        }

        public static System.Drawing.Point ToDrawingPoint(this Vector2 v)
        {
            var p = v.ToPoint();
            return new System.Drawing.Point(p.X, p.Y);
        }
    }
}
