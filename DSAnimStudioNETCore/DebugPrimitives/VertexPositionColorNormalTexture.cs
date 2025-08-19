using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public struct VertexPositionColorNormalTexture : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;
        public Vector2 UV;

        public VertexPositionColorNormalTexture(Vector3 position, Color color, Vector3 normal, Vector2 uv)
        {
            Position = position;
            Color = color;
            Normal = normal;
            UV = uv;
        }

        public VertexDeclaration VertexDeclaration => new VertexDeclaration(new VertexElement[]
        {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(0 + 12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(0 + 12 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(0 + 12 + 4 + 12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        });
    }
}
