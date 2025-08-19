//using DSAnimStudio.GFXShaders;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DSAnimStudio.DebugPrimitives
//{
//    public class DbgPrimNewGrid3D_Blur : DbgPrim<NewGrid3DShader_Blur>
//    {
//        public override IGFXShader<NewGrid3DShader_Blur> Shader => GFX.NewGrid3DShader_Blur;

//        protected override PrimitiveType PrimType => PrimitiveType.TriangleList;

//        protected bool KeepBuffersAlive = false;

//        private static DbgPrimGeometryData GeometryData = null;

//        public DbgPrimNewGrid3D_Blur()
//        {
//            BackfaceCulling = false;
//            KeepBuffersAlive = true;

//            if (GeometryData != null)
//            {
//                SetBuffers(GeometryData.VertBuffer, GeometryData.IndexBuffer);
//            }
//            else
//            {
//                AddVertex(new Vector3(-1, 0, -1), Color.White, Vector3.Up);
//                AddVertex(new Vector3(-1, 0, 1), Color.White, Vector3.Up);
//                AddVertex(new Vector3(1, 0, -1), Color.White, Vector3.Up);
//                AddVertex(new Vector3(1, 0, 1), Color.White, Vector3.Up);

//                AddIndex(0);
//                AddIndex(1);
//                AddIndex(2);

//                AddIndex(1);
//                AddIndex(3);
//                AddIndex(2);

//                //AddIndex(2);
//                //AddIndex(1);
//                //AddIndex(0);

//                //AddIndex(2);
//                //AddIndex(3);
//                //AddIndex(1);

//                FinalizeBuffers(true);

//                GeometryData = new DbgPrimGeometryData()
//                {
//                    VertBuffer = VertBuffer,
//                    IndexBuffer = IndexBuffer,
//                };
//            }
//        }

//        protected override void DisposeBuffers()
//        {
//            if (!KeepBuffersAlive)
//            {
//                VertBuffer?.Dispose();
//                IndexBuffer?.Dispose();
//            }
//            //VertBuffer?.Dispose();
//        }

//        public override DbgPrim<NewGrid3DShader_Blur> Instantiate(Transform newLocation)
//        {
//            var newPrim = new DbgPrimNewGrid3D_Blur();
//            newPrim.Indices = Indices;
//            newPrim.VertBuffer = VertBuffer;
//            newPrim.IndexBuffer = IndexBuffer;
//            newPrim.Vertices = Vertices;
//            newPrim.NeedToRecreateVertBuffer = NeedToRecreateVertBuffer;
//            newPrim.NeedToRecreateIndexBuffer = NeedToRecreateIndexBuffer;

//            newPrim.Transform = newLocation;
            

//            return newPrim;
//        }
//    }
//}
