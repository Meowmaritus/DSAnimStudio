using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
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
        public Vector4 BoneIndices;
        public Vector4 BoneWeights;
        public Vector4 BoneIndicesBank;

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
            List<VertexElement> elements = new List<VertexElement>();
            int offset = 0;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0));
            offset += sizeof(float) * 3;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
            offset += sizeof(float) * 2;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1));
            offset += sizeof(float) * 2;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0));
            offset += sizeof(float) * 3;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0));
            offset += sizeof(float) * 3;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0));
            offset += sizeof(float) * 3;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.Color, 0));
            offset += sizeof(float) * 4;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0));
            offset += sizeof(float) * 4;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0));
            offset += sizeof(float) * 4;

            elements.Add(new VertexElement(offset, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 1));
            //offset += sizeof(float) * 2;

            VertexDeclaration = new VertexDeclaration(elements.ToArray());
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
                hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
                hashCode = (hashCode * 397) ^ TextureCoordinate2.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ Binormal.GetHashCode();
                hashCode = (hashCode * 397) ^ Tangent.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ BoneIndices.GetHashCode();
                hashCode = (hashCode * 397) ^ BoneWeights.GetHashCode();
                hashCode = (hashCode * 397) ^ BoneIndicesBank.GetHashCode();
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
                (left.TextureCoordinate == right.TextureCoordinate) &&
                (left.TextureCoordinate2 == right.TextureCoordinate2) &&
                (left.Normal == right.Normal) &&
                (left.Binormal == right.Binormal) &&
                (left.Tangent == right.Tangent) &&
                (left.Color == right.Color) &&
                (left.BoneIndices == right.BoneIndices) &&
                (left.BoneWeights == right.BoneWeights) &&
                (left.BoneIndicesBank == right.BoneIndicesBank)
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
