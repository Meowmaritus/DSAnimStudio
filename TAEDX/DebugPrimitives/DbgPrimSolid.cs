using TAEDX.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.DebugPrimitives
{
    public class DbgPrimSolid : DbgPrim<DbgPrimSolidShader>
    {
        public override IGFXShader<DbgPrimSolidShader> Shader => GFX.DbgPrimSolidShader;

        private int[] Indices = new int[0];
        private VertexPositionColorNormal[] Vertices = new VertexPositionColorNormal[0];
        private VertexBuffer VertBuffer;
        private IndexBuffer IndexBuffer;
        private bool NeedToRecreateVertBuffer = true;
        private bool NeedToRecreateIndexBuffer = true;

        public int VertexCount => Vertices.Length;
        public int LineCount => Indices.Length / 2;
        public int IndexCount => Indices.Length;

        private void AddVertex(Vector3 pos, Color color, Vector3 normal)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1].Position = pos;
            Vertices[Vertices.Length - 1].Color = color;
            Vertices[Vertices.Length - 1].Normal = normal;
            NeedToRecreateVertBuffer = true;
        }

        private void AddVertex(VertexPositionColorNormal vert)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1] = vert;

            NeedToRecreateVertBuffer = true;
        }

        private void AddIndex(int index)
        {
            Array.Resize(ref Indices, Indices.Length + 1);
            Indices[Indices.Length - 1] = index;
            NeedToRecreateIndexBuffer = true;
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            AddTri(a, b, c, color, color, color);
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c, Color colorA, Color colorB, Color colorC)
        {
            var dir = Vector3.Cross(b - a, c - a);
            var norm = Vector3.Normalize(dir);

            var vertA = new VertexPositionColorNormal(a, colorA, norm);
            var vertB = new VertexPositionColorNormal(b, colorB, norm);
            var vertC = new VertexPositionColorNormal(c, colorC, norm);

            int vertIndexA = Array.IndexOf(Vertices, vertA);
            int vertIndexB = Array.IndexOf(Vertices, vertB);
            int vertIndexC = Array.IndexOf(Vertices, vertC);

            //If vertex A can't be recycled from an old one, make a new one.
            if (vertIndexA == -1)
            {
                AddVertex(vertA);
                vertIndexA = Vertices.Length - 1;
            }

            //If vertex B can't be recycled from an old one, make a new one.
            if (vertIndexB == -1)
            {
                AddVertex(vertB);
                vertIndexB = Vertices.Length - 1;
            }

            //If vertex C can't be recycled from an old one, make a new one.
            if (vertIndexC == -1)
            {
                AddVertex(vertC);
                vertIndexC = Vertices.Length - 1;
            }

            AddIndex(vertIndexA);
            AddIndex(vertIndexB);
            AddIndex(vertIndexC);

            //if (NeedToRecreateVertBuffer)
            //{
            //    VertBuffer = new VertexBuffer(GFX.Device, 
            //        typeof(VertexPositionColor), Vertices.Length, BufferUsage.WriteOnly);
            //    VertBuffer.SetData(Vertices);
            //    NeedToRecreateVertBuffer = false;
            //} 

            //if (NeedToRecreateIndexBuffer)
            //{
            //    IndexBuffer = new IndexBuffer(GFX.Device, IndexElementSize.ThirtyTwoBits, Indices.Length, BufferUsage.WriteOnly);
            //    IndexBuffer.SetData(Indices);
            //    NeedToRecreateIndexBuffer = false;
            //}
        }

        protected override void DrawPrimitive()
        {
            if (NeedToRecreateVertBuffer)
            {
                VertBuffer = new VertexBuffer(GFX.Device,
                    typeof(VertexPositionColorNormal), Vertices.Length, BufferUsage.WriteOnly);
                VertBuffer.SetData(Vertices);
                NeedToRecreateVertBuffer = false;
            }

            if (NeedToRecreateIndexBuffer)
            {
                IndexBuffer = new IndexBuffer(GFX.Device, IndexElementSize.ThirtyTwoBits, Indices.Length, BufferUsage.WriteOnly);
                IndexBuffer.SetData(Indices);
                NeedToRecreateIndexBuffer = false;
            }

            GFX.Device.SetVertexBuffer(VertBuffer);
            GFX.Device.Indices = IndexBuffer;
            GFX.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.Length);
        }

        protected override void DisposeBuffers()
        {
            VertBuffer.Dispose();
        }

        public override DbgPrim<DbgPrimSolidShader> Instantiate(string newName, Transform newLocation, Color? newNameColor = null)
        {
            var newPrim = new DbgPrimSolid();
            newPrim.Indices = Indices;
            newPrim.VertBuffer = VertBuffer;
            newPrim.IndexBuffer = IndexBuffer;
            newPrim.Vertices = Vertices;
            newPrim.NeedToRecreateVertBuffer = NeedToRecreateVertBuffer;
            newPrim.NeedToRecreateIndexBuffer = NeedToRecreateIndexBuffer;

            newPrim.Transform = newLocation;

            newPrim.Name = newName;

            newPrim.NameColor = newNameColor ?? NameColor;

            return newPrim;
        }
    }
}
