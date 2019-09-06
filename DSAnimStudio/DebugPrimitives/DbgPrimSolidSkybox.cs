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
    public class DbgPrimSolidSkybox : DbgPrim<SkyboxShader>
    {
        public override IGFXShader<SkyboxShader> Shader => GFX.SkyboxShader;
        private int[] Indices = new int[0];
        private VertexPosition[] Vertices = new VertexPosition[0];
        private VertexBuffer VertBuffer;
        private IndexBuffer IndexBuffer;
        private bool NeedToRecreateVertBuffer = true;
        private bool NeedToRecreateIndexBuffer = true;

        static float Radius = 100;

        public DbgPrimSolidSkybox()
        {
            Category = DbgPrimCategory.Skybox;

            Vector3 min = -Vector3.One * Radius;
            Vector3 max = Vector3.One * Radius;

            // 3 Letters of below names: 
            // [T]op/[B]ottom, [F]ront/[B]ack, [L]eft/[R]ight
            var tfl = new Vector3(min.X, max.Y, max.Z);
            var tfr = new Vector3(max.X, max.Y, max.Z);
            var bfr = new Vector3(max.X, min.Y, max.Z);
            var bfl = new Vector3(min.X, min.Y, max.Z);
            var tbl = new Vector3(min.X, max.Y, min.Z);
            var tbr = new Vector3(max.X, max.Y, min.Z);
            var bbr = new Vector3(max.X, min.Y, min.Z);
            var bbl = new Vector3(min.X, min.Y, min.Z);

            //front face
            AddTri(bfl, tfl, tfr);
            AddTri(bfl, tfr, bfr);

            // top face
            AddTri(tfl, tbl, tbr);
            AddTri(tfl, tbr, tfr);

            // back face
            AddTri(bbl, tbl, tbr);
            AddTri(bbl, tbr, bbr);

            // bottom face
            AddTri(bfl, bbl, bbr);
            AddTri(bfl, bbr, bfr);

            // left face
            AddTri(bbl, tbl, tfl);
            AddTri(bbl, tfl, bfl);

            // right face
            AddTri(bbr, tbr, tfr);
            AddTri(bbr, tfr, bfr);
        }

        public int VertexCount => Vertices.Length;
        public int LineCount => Indices.Length / 2;
        public int IndexCount => Indices.Length;

        private void AddVertex(Vector3 pos, Color color, Vector3 normal)
        {
            Array.Resize(ref Vertices, Vertices.Length + 1);
            Vertices[Vertices.Length - 1].Position = pos;
            NeedToRecreateVertBuffer = true;
        }

        private void AddVertex(VertexPosition vert)
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

        public void AddTri(Vector3 a, Vector3 b, Vector3 c)
        {
            //var dir = Vector3.Cross(b - a, c - a);
            //var norm = Vector3.Normalize(dir);

            var vertA = new VertexPosition(a);
            var vertB = new VertexPosition(b);
            var vertC = new VertexPosition(c);

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
                    typeof(VertexPosition), Vertices.Length, BufferUsage.WriteOnly);
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
            GFX.BackfaceCulling = false;
            GFX.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Indices.Length);
        }

        protected override void DisposeBuffers()
        {
            VertBuffer?.Dispose();
        }

        public override DbgPrim<SkyboxShader> Instantiate(string newName, Transform newLocation, Color? newNameColor = null)
        {
            var newPrim = new DbgPrimSolidSkybox();
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
