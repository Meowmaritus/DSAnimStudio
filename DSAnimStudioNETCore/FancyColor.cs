using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace DSAnimStudio
{
    public struct FancyColor
    {
        public bool Enabled;
        public Vector4 Data;

        public float R
        {
            get => Data.X;
            set => Data.X = value;
        }

        public float G
        {
            get => Data.Y;
            set => Data.Y = value;
        }

        public float B
        {
            get => Data.Z;
            set => Data.Z = value;
        }

        public float A
        {
            get => Data.W;
            set => Data.W = value;
        }

        public Microsoft.Xna.Framework.Color CompressToMonogameColor()
        {
            return new Microsoft.Xna.Framework.Color(Data);
        }

        public Vector4 GetValueOrDefault(Vector4 defaultColor)
        {
            return Enabled ? Data : defaultColor;
        }

        public FancyColor(Vector4 color, bool enabled)
        {
            Data = color;
            Enabled = enabled;
        }

        public FancyColor(Microsoft.Xna.Framework.Color color, bool enabled)
        {
            Data = new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
            Enabled = enabled;
        }

    }
}
