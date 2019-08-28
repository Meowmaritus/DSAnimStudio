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
    public class DbgPrimWire : DbgPrim<DbgPrimWireShader>
    {
        public override IGFXShader<DbgPrimWireShader> Shader => GFX.DbgPrimWireShader;

        private int[] Indices = new int[0];
        private VertexPositionColor[] Vertices = new VertexPositionColor[0];
        private VertexBuffer VertBuffer;
        private IndexBuffer IndexBuffer;
        private bool NeedToRecreateVertBuffer = true;
        private bool NeedToRecreateIndexBuffer = true;

        public int VertexCount => Vertices.Length;
        public int LineCount => Indices.Length / 2;
        public int IndexCount => Indices.Length;

        private void AddVertex(Vector3 pos, Color color)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1].Position = pos;
            Vertices[Vertices.Length - 1].Color = color;

            NeedToRecreateVertBuffer = true;
        }

        private void AddVertex(VertexPositionColor vert)
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

        public void AddLine(Vector3 start, Vector3 end, Color color)
        {
            AddLine(start, end, color, color);
        }

        public void AddLine(Vector3 start, Vector3 end, Color startColor, Color endColor)
        {
            var startVert = new VertexPositionColor(start, startColor);
            var endVert = new VertexPositionColor(end, endColor);
            int startIndex = Array.IndexOf(Vertices, startVert);
            int endIndex = Array.IndexOf(Vertices, endVert);

            //If start vertex can't be recycled from an old one, make a new one.
            if (startIndex == -1)
            {
                AddVertex(startVert);
                startIndex = Vertices.Length - 1;
            }

            //If end vertex can't be recycled from an old one, make a new one.
            if (endIndex == -1)
            {
                AddVertex(endVert);
                endIndex = Vertices.Length - 1;
            }

            AddIndex(startIndex);
            AddIndex(endIndex);

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
                    typeof(VertexPositionColor), Vertices.Length, BufferUsage.WriteOnly);
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
            GFX.Device.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, Indices.Length);
        }

        protected override void DisposeBuffers()
        {
            VertBuffer.Dispose();
        }

        public override DbgPrim<DbgPrimWireShader> Instantiate(string newName, Transform newLocation, Color? newNameColor = null)
        {
            var newPrim = new DbgPrimWire();
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
