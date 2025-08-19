using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public struct RectF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public Vector2 Location
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public float Top
        {
            get => Y;
            set => Y = value;
        }

        public float Left
        {
            get => X;
            set => X = value;
        }

        public float Bottom
        {
            get => Y + Height;
            set => Height = value - Y;
        }

        public float Right
        {
            get => X + Width;
            set => Width = value - X;
        }

        public RectF(Vector4 vector)
        {
            X = vector.X;
            Y = vector.Y;
            Width = vector.Z;
            Height = vector.W;
        }

        public RectF(Vector2 location, Vector2 size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.X;
            Height = size.Y;
        }

        public RectF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static implicit operator RectF(Rectangle rect)
        {
            return new RectF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public bool Contains(Vector2 point)
        {
            return point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;
        }
    }
}
