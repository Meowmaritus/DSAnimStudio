using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class StructExtensions
    {
        public static void StartInCenterOf(this System.Windows.Forms.Form f, System.Windows.Forms.Form form)
        {
            f.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            f.Location = new System.Drawing.Point(
                form.Location.X + (form.Size.Width / 2) - (f.Size.Width / 2),
                form.Location.Y + (form.Size.Height / 2) - (f.Size.Height / 2));
        }

        public static Rectangle SubtractScroll(this Rectangle r, Vector2 scroll)
        {
            return new Rectangle((int)(r.X - scroll.X), (int)(r.Y - scroll.Y), r.Width, r.Height);
        }

        public static string ExtractDigits(this string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
                if (char.IsDigit(c))
                    sb.Append(c);
            return sb.ToString();
        }

        public static int ExtractDigitsToInt(this string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
                if (char.IsDigit(c))
                    sb.Append(c);
            return int.Parse(sb.ToString());
        }

        public static FLVER.Dummy GetClone(this FLVER.Dummy d)
        {
            var c = new FLVER.Dummy();
            c.AttachBoneIndex = d.AttachBoneIndex;
            c.Color = d.Color;
            c.Flag1 = d.Flag1;
            c.Forward = d.Forward;
            c.ParentBoneIndex = d.ParentBoneIndex;
            c.Position = d.Position;
            c.ReferenceID = d.ReferenceID;
            c.Unk30 = d.Unk30;
            c.Unk34 = d.Unk34;
            c.Upward = d.Upward;
            c.UseUpwardVector = d.UseUpwardVector;
            return c;
        }

        public static Rectangle ApplyPadding(this Rectangle rect, 
            float t, float b, 
            float l, float r)
        {
            return new Rectangle(
                (int)(rect.X + l),
                (int)(rect.Y + t),
                (int)(rect.Width - l - r),
                (int)(rect.Height - t - b));
        }

        public static Rectangle ApplyPadding(this Rectangle r, float p)
        {
            return r.ApplyPadding(p, p, p, p);
        }

        public static Rectangle ApplyPadding(this Rectangle r, float x, float y)
        {
            return r.ApplyPadding(y, y, x, x);
        }

        public static Vector3 XYZ(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static System.Numerics.Vector3 XYZ(this System.Numerics.Vector4 v)
        {
            return new System.Numerics.Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 GetCenter(this BoundingBox bb)
        {
            return (bb.Min + bb.Max) / 2;
        }

        public static RasterizerState GetCopyOfState(this RasterizerState rs)
        {
            return new RasterizerState()
            {
                CullMode = rs.CullMode,
                DepthBias = rs.DepthBias,
                DepthClipEnable = rs.DepthClipEnable,
                FillMode = rs.FillMode,
                MultiSampleAntiAlias = true,
                Name = rs.Name,
                ScissorTestEnable = rs.ScissorTestEnable,
                SlopeScaleDepthBias = rs.SlopeScaleDepthBias,
            };
        }

        public static Vector2 TopLeftCorner(this Rectangle r) => new Vector2(r.Left, r.Top);
        public static Vector2 TopRightCorner(this Rectangle r) => new Vector2(r.Right, r.Top);
        public static Vector2 BottomLeftCorner(this Rectangle r) => new Vector2(r.Left, r.Bottom);
        public static Vector2 BottomRightCorner(this Rectangle r) => new Vector2(r.Right, r.Bottom);
        public static System.Numerics.Vector2 TopLeftCornerN(this Rectangle r) => new System.Numerics.Vector2(r.Left, r.Top);
        public static System.Numerics.Vector2 TopRightCornerN(this Rectangle r) => new System.Numerics.Vector2(r.Right, r.Top);
        public static System.Numerics.Vector2 BottomLeftCornerN(this Rectangle r) => new System.Numerics.Vector2(r.Left, r.Bottom);
        public static System.Numerics.Vector2 BottomRightCornerN(this Rectangle r) => new System.Numerics.Vector2(r.Right, r.Bottom);
        public static Vector2 Center(this Rectangle r) => new Vector2(r.Center.X, r.Center.Y);

        public static Rectangle GetUniformShrunkFromBorder(this Rectangle r, int shrinkAmount)
        {
            return new Rectangle(Math.Min(r.X + shrinkAmount, r.X + r.Width / 2),
                Math.Min(r.Y + shrinkAmount, r.Y + r.Height / 2), 
                Math.Max(r.Width - shrinkAmount * 2, 0), 
                Math.Max(r.Height - shrinkAmount * 2, 0));
        }

        public static Vector3 Rad2Deg(this Vector3 v)
        {
            return new Vector3(MathHelper.ToDegrees(v.X), MathHelper.ToDegrees(v.Y), MathHelper.ToDegrees(v.Z));
        }

        public static Vector3 Deg2Rad(this Vector3 v)
        {
            return new Vector3(MathHelper.ToRadians(v.X), MathHelper.ToRadians(v.Y), MathHelper.ToRadians(v.Z));
        }


    }
}
