using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionColorNormalTangentTexture : IVertexType
    {
        public Vector3 Position;
        public Vector2 TextureCoordinate;
        public Vector2 TextureCoordinate2;
        public Vector3 Normal;
        public Vector3 Binormal;
        public Vector3 Tangent;
        public Vector4 Color;

        /// <summary>
        /// Vertex declaration object.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration;

        /// <summary>
        /// Vertex declaration.
        /// </summary>
        VertexDeclaration IVertexType.VertexDeclaration
        {
            get
            {
                return VertexDeclaration;
            }
        }



        /// <summary>
        /// Static constructor to init vertex declaration.
        /// </summary>
        static VertexPositionColorNormalTangentTexture()
        {
            VertexElement[] elements = new VertexElement[] {
                new VertexElement(sizeof(float) * (0), VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(sizeof(float) * (0 + 3), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(sizeof(float) * (0 + 3 + 2), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * (0 + 3 + 2 + 2), VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                new VertexElement(sizeof(float) * (0 + 3 + 2 + 2 + 3), VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0),
                new VertexElement(sizeof(float) * (0 + 3 + 2 + 2 + 3 + 3), VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0),
                new VertexElement(sizeof(float) * (0 + 3 + 2 + 2 + 3 + 3 + 3), VertexElementFormat.Vector4, VertexElementUsage.Color, 0),

            };
            VertexDeclaration declaration = new VertexDeclaration(elements);
            VertexDeclaration = declaration;
        }

        /// <summary>
        /// Get if equals another object.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>If objects are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return (this == ((VertexPositionColorNormalTangentTexture)obj));
        }

        /// <summary>
        /// Get the hash code of this vertex.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ Binormal.GetHashCode();
                hashCode = (hashCode * 397) ^ Tangent.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Return if two vertices are equal.
        /// </summary>
        /// <param name="left">Left side to compare.</param>
        /// <param name="right">Right side to compare.</param>
        /// <returns>If equal.</returns>
        public static bool operator ==(VertexPositionColorNormalTangentTexture left, VertexPositionColorNormalTangentTexture right)
        {
            return (
                (left.Position == right.Position) &&
                (left.Color == right.Color) &&
                (left.Normal == right.Normal) &&
                (left.Binormal == right.Binormal) &&
                (left.Tangent == right.Tangent)
                );
        }

        /// <summary>
        /// Return if two vertices are not equal.
        /// </summary>
        /// <param name="left">Left side to compare.</param>
        /// <param name="right">Right side to compare.</param>
        /// <returns>If not equal.</returns>
        public static bool operator !=(VertexPositionColorNormalTangentTexture left, VertexPositionColorNormalTangentTexture right)
        {
            return !(left == right);
        }
    }
}
