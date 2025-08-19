using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVector4 = System.Numerics.Vector4;

namespace DSAnimStudio
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ColorEditCfgAttribute : Attribute
    {
        public readonly string DispName;
        public readonly NVector4 DefaultValue;
        public ColorEditCfgAttribute(string dispName, NVector4 defaultVal)
        {
            DispName = dispName;
            DefaultValue = defaultVal;
        }

        //public ColorEditCfgAttribute(string dispName, Microsoft.Xna.Framework.Color defaultVal)
        //{
        //    DispName = dispName;
        //    DefaultValue = defaultVal.ToNVector4();
        //}

        public ColorEditCfgAttribute(string dispName, uint packedColor)
        {
            int r = (int)((packedColor >> (24)) & 255);
            int g = (int)((packedColor >> (16)) & 255);
            int b = (int)((packedColor >> (8)) & 255);
            int a = (int)((packedColor >> (0)) & 255);
            DefaultValue = new NVector4(r / 255f, g / 255f, b / 255f, a / 255f);
            DispName = dispName;
        }

        public ColorEditCfgAttribute(string dispName, int r, int g, int b)
        {
            DispName = dispName;
            DefaultValue = new NVector4(r / 255f, g / 255f, b / 255f, 1.0f);
        }

        public ColorEditCfgAttribute(string dispName, int r, int g, int b, int a)
        {
            DispName = dispName;
            DefaultValue = new NVector4(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public ColorEditCfgAttribute(string dispName, float r, float g, float b)
        {
            DispName = dispName;
            DefaultValue = new NVector4(r, g, b, 1.0f);
        }

        public ColorEditCfgAttribute(string dispName, float r, float g, float b, float a)
        {
            DispName = dispName;
            DefaultValue = new NVector4(r, g, b, a);
        }
    }
}
