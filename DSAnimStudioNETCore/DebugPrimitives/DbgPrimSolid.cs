﻿using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DebugPrimitives
{
    public class DbgPrimSolid : DbgPrim<DbgPrimSolidShader>
    {
        public override IGFXShader<DbgPrimSolidShader> Shader => GFX.DbgPrimSolidShader;

        protected override PrimitiveType PrimType => PrimitiveType.TriangleList;

        protected bool KeepBuffersAlive;

        public int TriCount => Indices.Length / 3;

        public void AddQuad_Backward(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            AddTri_Backward(a, b, c, color);
            AddTri_Backward(a, c, d, color);
        }

        public void AddQuad_Backward(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color colorA, Color colorB, Color colorC, Color colorD)
        {
            AddTri_Backward(a, b, c, colorA, colorB, colorC);
            AddTri_Backward(c, d, a, colorC, colorD, colorA);
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            AddTri(a, b, c, color);
            AddTri(a, c, d, color);
        }

        public void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color colorA, Color colorB, Color colorC, Color colorD)
        {
            AddTri(a, b, c, colorA, colorB, colorC);
            AddTri(c, d, a, colorC, colorD, colorA);
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            AddTri(a, b, c, color, color, color);
        }

        public void AddTri_Backward(Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            AddTri(c, b, a, color, color, color);
        }

        public void AddTri_Backward(Vector3 a, Vector3 b, Vector3 c, Color colorA, Color colorB, Color colorC, Vector2? uvA = null, Vector2? uvB = null, Vector2? uvC = null)
        {
            AddTri(c, b, a, colorC, colorB, colorA, uvC, uvB, uvA);
        }

        public void AddTri(Vector3 a, Vector3 b, Vector3 c, Color colorA, Color colorB, Color colorC, Vector2? uvA = null, Vector2? uvB = null, Vector2? uvC = null)
        {
            var dir = Vector3.Cross(b - a, c - a);
            var norm = Vector3.Normalize(dir);

            var vertA = new VertexPositionColorNormalTexture(a, colorA, norm, uvA ?? Vector2.Zero);
            var vertB = new VertexPositionColorNormalTexture(b, colorB, norm, uvB ?? Vector2.Zero);
            var vertC = new VertexPositionColorNormalTexture(c, colorC, norm, uvC ?? Vector2.Zero);

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

            AddIndex(vertIndexC);
            AddIndex(vertIndexB);
            AddIndex(vertIndexA);

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

        protected override void DisposeBuffers()
        {
            if (!KeepBuffersAlive)
            {
                VertBuffer?.Dispose();
                IndexBuffer?.Dispose();
            }
        }

        public override DbgPrim<DbgPrimSolidShader> Instantiate(Transform newLocation)
        {
            var newPrim = new DbgPrimSolid();
            newPrim.Indices = Indices;
            newPrim.VertBuffer = VertBuffer;
            newPrim.IndexBuffer = IndexBuffer;
            newPrim.Vertices = Vertices;
            newPrim.NeedToRecreateVertBuffer = NeedToRecreateVertBuffer;
            newPrim.NeedToRecreateIndexBuffer = NeedToRecreateIndexBuffer;

            newPrim.Transform = newLocation;

            return newPrim;
        }
    }
}
