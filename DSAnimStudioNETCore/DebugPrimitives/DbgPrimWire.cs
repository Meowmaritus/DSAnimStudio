using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimWire : DbgPrim<DbgPrimWireShader>
    {
        public override IGFXShader<DbgPrimWireShader> Shader => GFX.DbgPrimWireShader;

        protected override PrimitiveType PrimType => PrimitiveType.LineList;

        public int LineCount => Indices.Length / 2;

        protected bool KeepBuffersAlive = false;

        public void AddLine(Vector3 start, Vector3 end)
        {
            AddLine(start, end, Color.White);
        }

        public void AddLine(Vector3 start, Vector3 end, Color color)
        {
            AddLine(start, end, color, color);
        }

        public void AddLine(Vector3 start, Vector3 end, Color startColor, Color endColor)
        {
            var startVert = new VertexPositionColorNormal(start, startColor, Vector3.Zero);
            var endVert = new VertexPositionColorNormal(end, endColor, Vector3.Zero);
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

            for (int i = 0; i < Indices.Length; i += 2)
            {
                int lineStart = Indices[i];
                if ((i + 1) < Indices.Length)
                {
                    int lineEnd = Indices[i + 1];

                    if (lineStart == startIndex && lineEnd == endIndex)
                    {
                        // Line literally already exists lmao
                        return;
                    }
                }
            }

            AddIndex(startIndex);
            AddIndex(endIndex);
        }

        protected override void DisposeBuffers()
        {
            if (!KeepBuffersAlive)
            {
                VertBuffer?.Dispose();
                IndexBuffer?.Dispose();
            }
            //VertBuffer?.Dispose();
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
