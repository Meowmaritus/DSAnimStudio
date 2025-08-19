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
    public class DbgPrimNewGrid3D : DbgPrim<NewGrid3DShader>
    {
        public override IGFXShader<NewGrid3DShader> Shader => GFX.NewGrid3DShader;

        protected override PrimitiveType PrimType => PrimitiveType.TriangleList;

        protected bool KeepBuffersAlive = false;

        private static DbgPrimGeometryData GeometryData = null;

        private void AddQuadMeme(float minX, float maxX, float minZ, float maxZ)
        {
            var index0 = AddVertex(new Vector3(minX, 0, minZ), Color.White, Vector3.Up);
            var index1 = AddVertex(new Vector3(minX, 0, maxZ), Color.White, Vector3.Up);
            var index2 = AddVertex(new Vector3(maxX, 0, maxZ), Color.White, Vector3.Up);
            var index3 = AddVertex(new Vector3(maxX, 0, minZ), Color.White, Vector3.Up);


            AddIndex(index0);
            AddIndex(index1);
            AddIndex(index2);
            AddIndex(index0);
            AddIndex(index2);
            AddIndex(index3);
        }

        public DbgPrimNewGrid3D()
        {
            BackfaceCulling = true;
            KeepBuffersAlive = false;

            if (KeepBuffersAlive && GeometryData != null)
            {
                SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
            }
            else
            {
                

                int slices = 10;

                for (int x = 0; x < slices; x++)
                {
                    for (int z = 0; z < slices; z++)
                    {
                        float actualX = (((float)x / slices) - 0.5f) * 2;
                        float actualZ = (((float)z / slices) - 0.5f) * 2;

                        AddQuadMeme(actualX, actualX + (2f / slices), actualZ, actualZ + (2f / slices));
                    }
                }
                

                //AddIndex(2);
                //AddIndex(1);
                //AddIndex(0);

                //AddIndex(2);
                //AddIndex(3);
                //AddIndex(1);

                FinalizeBuffers(true);

                if (KeepBuffersAlive)
                {
                    GeometryData = new DbgPrimGeometryData()
                    {
                        VertBuffer = VertBuffer,
                        IndexBuffer = IndexBuffer,
                    };
                }
            }
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

        public override DbgPrim<NewGrid3DShader> Instantiate(Transform newLocation)
        {
            var newPrim = new DbgPrimNewGrid3D();
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
