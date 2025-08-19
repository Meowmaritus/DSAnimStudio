using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.GFXShaders;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewSimpleGrid : IDisposable
    {
        private VertexBuffer VertBuffer;
        private IndexBuffer IndexBuffer;

        private void CreateGeomBuffers()
        {
            VertexPositionNormalTexture[] verts = new VertexPositionNormalTexture[4];
            verts[0] = new VertexPositionNormalTexture(new Vector3(-1, 0, -1), Vector3.Up, new Vector2(-1, -1));
            verts[1] = new VertexPositionNormalTexture(new Vector3(-1, 0, 1), Vector3.Up, new Vector2(-1, 1));
            verts[2] = new VertexPositionNormalTexture(new Vector3(1, 0, -1), Vector3.Up, new Vector2(1, -1));
            verts[3] = new VertexPositionNormalTexture(new Vector3(1, 0, 1), Vector3.Up, new Vector2(1, 1));

            VertBuffer = new VertexBuffer(GFX.Device, typeof(VertexPositionNormalTexture), verts.Length, BufferUsage.WriteOnly);
            VertBuffer.SetData(verts);
            
            int[] indices = new int[6] 
            { 
                0, 1, 2, 
                2, 1, 3,
            };

            IndexBuffer = new IndexBuffer(GFX.Device, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData(indices);
        }

        public bool AutoGenEntries = true;

        public List<SimpleGridEntry> SimpleGridEntries = new List<SimpleGridEntry>
        {
            new SimpleGridEntry(),
        };


        public NewSimpleGrid()
        {
            CreateGeomBuffers();
            GenerateDefaults();
        }

        public enum SimpleGridOriginTypes
        {
            WorldOrigin = 0,
            CameraX = 11,
            CameraY = 12,
            CameraZ = 13,
            CameraXY = 14,
            CameraXZ = 15,
            CameraYZ = 16,
            CameraXYZ = 17,
            ModelX = 21,
            ModelY = 22,
            ModelZ = 23,
            ModelXY = 24,
            ModelXZ = 25,
            ModelYZ = 26,
            ModelXYZ = 27,
        }

        public enum SimpleGridAxisTypes
        {
            None,
            XY = 1,
            XZ = 2,
            YZ = 3,
        }

        public enum SimpleGridColorTypes
        {
            Custom = 0,
            Primary = 1,
            Secondary = 2,
            //Origin = 10,
        }

        public System.Numerics.Vector4 GetGridColor(SimpleGridColorTypes type)
        {
            switch (type)
            {
                case SimpleGridColorTypes.Primary: return Main.Colors.ColorGrid1u.ToNVector4();
                case SimpleGridColorTypes.Secondary: return Main.Colors.ColorGrid10u.ToNVector4();
                //case SimpleGridColorTypes.Origin: return Main.Colors.ColorGridOrigin.ToNVector4();
                default:
                    return Main.Colors.ColorGrid1u.ToNVector4();
            }
        }

        

        public class SimpleGridEntry
        {
            public bool Enabled = true;

            public SimpleGridAxisTypes AxisType = SimpleGridAxisTypes.XZ;
            public SimpleGridOriginTypes OriginType = SimpleGridOriginTypes.WorldOrigin;
            public bool DrawPlaneWireframe = false;
            public System.Numerics.Vector4 PlaneWireframeColor = new System.Numerics.Vector4(1, 0, 1, 1);

            public float UnitSize = 1;
            public float DistFalloffStart = 200;
            public float DistFalloffEnd = 300;
            public float DistFalloffPower = 1;
            public float AlphaPower = 1;
            public SimpleGridColorTypes ColorType = SimpleGridColorTypes.Primary;
            public System.Numerics.Vector4 CustomColor = new System.Numerics.Vector4(1, 1, 1, 1);
            //public SimpleGridColorTypes OriginColorType = SimpleGridColorTypes.Origin;
            //public System.Numerics.Vector4 OriginCustomColor = new System.Numerics.Vector4(1, 1, 1, 1);
        }

        public void GenerateDefaults()
        {
            SimpleGridEntries.Clear();
            

            SimpleGridEntry secondaryEntry = null;
            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                var mainEntry = new SimpleGridEntry();
                mainEntry.DistFalloffStart *= 10;
                mainEntry.DistFalloffEnd *= 10;
                SimpleGridEntries.Add(mainEntry);

                secondaryEntry = new SimpleGridEntry();
                secondaryEntry.ColorType = SimpleGridColorTypes.Secondary;
                secondaryEntry.DistFalloffStart *= 10;
                secondaryEntry.DistFalloffEnd *= 10;
                secondaryEntry.UnitSize *= 10;
                SimpleGridEntries.Add(secondaryEntry);
            }
            else
            {
                SimpleGridEntries.Add(new SimpleGridEntry());
            }

        }

        public void Draw(WorldView worldView)
        {
            if (AutoGenEntries)
                GenerateDefaults();

            bool oldGFXWireframe = GFX.Wireframe;
            bool oldGFXBackfaceCulling = GFX.BackfaceCulling;

            float depth = 0;

            float currentAlpha = 1;

            float physicalDepth = 0;

            float layerDepthChange = 0.005f;

            //float meme = Vector3.Dot(Vector3.Up, Vector3.Transform(Vector3.Forward, worldView.CameraLocationInWorld.RotationMatrix));

            ////layerDepthChange *= 1-meme;

            //float memeMin = 0.1f;
            //float memeMax = 0.15f;

            //meme = (MathHelper.Clamp(MathF.Abs(meme) - memeMin, 0, (memeMax - memeMin)) / (memeMax - memeMin));

            //meme *= meme;
            //meme = 1 - meme;

            //layerDepthChange = MathHelper.Lerp(0.005f, 0.00001f, meme);

            for (int i = 0; i < SimpleGridEntries.Count; i++)
            {
                var ge = SimpleGridEntries[i];
                if (!ge.Enabled)
                    continue;

                if (ge.AxisType == SimpleGridAxisTypes.None)
                    return;


                var effect = GFX.NewSimpleGridShader.Effect;
                //var blurEffect = GFX.NewGrid3DShader_Blur.Effect;

                Vector3 origin = Vector3.Zero;
                var camPos = worldView.CameraLocationInWorld.Position;

                var camFollowPos = -(worldView.CameraOrbitOrigin + worldView.CameraOrbitOriginOffset + Vector3.Transform(worldView.RootMotionFollow_Translation, worldView.Matrix_World));
                camFollowPos.X *= -1;
                camFollowPos.Y *= -1;

                var modelFollowPos = Vector3.Zero;

                zzz_DocumentManager.CurrentDocument.Scene.AccessMainModel(model =>
                {
                    modelFollowPos = model.CurrentTransformPosition;
                });

                if (ge.OriginType == SimpleGridOriginTypes.WorldOrigin)
                    origin = Vector3.Zero;
                else if (ge.OriginType == SimpleGridOriginTypes.CameraX)
                    origin = camFollowPos * new Vector3(1, 0, 0);
                else if (ge.OriginType == SimpleGridOriginTypes.CameraY)
                    origin = camFollowPos * new Vector3(0, 1, 0);
                else if (ge.OriginType == SimpleGridOriginTypes.CameraZ)
                    origin = camFollowPos * new Vector3(0, 0, 1);
                else if (ge.OriginType == SimpleGridOriginTypes.CameraXY)
                    origin = camFollowPos * new Vector3(1, 1, 0);
                else if (ge.OriginType == SimpleGridOriginTypes.CameraXZ)
                    origin = camFollowPos * new Vector3(1, 0, 1);
                else if (ge.OriginType == SimpleGridOriginTypes.CameraYZ)
                    origin = camFollowPos * new Vector3(0, 1, 1);
                else if (ge.OriginType == SimpleGridOriginTypes.CameraXYZ)
                    origin = camFollowPos;
                else if (ge.OriginType == SimpleGridOriginTypes.ModelX)
                    origin = modelFollowPos * new Vector3(1, 0, 0);
                else if (ge.OriginType == SimpleGridOriginTypes.ModelY)
                    origin = modelFollowPos * new Vector3(0, 1, 0);
                else if (ge.OriginType == SimpleGridOriginTypes.ModelZ)
                    origin = modelFollowPos * new Vector3(0, 0, 1);
                else if (ge.OriginType == SimpleGridOriginTypes.ModelXZ)
                    origin = modelFollowPos * new Vector3(1, 0, 1);
                else if (ge.OriginType == SimpleGridOriginTypes.ModelXY)
                    origin = modelFollowPos * new Vector3(1, 1, 0);
                else if (ge.OriginType == SimpleGridOriginTypes.ModelYZ)
                    origin = modelFollowPos * new Vector3(0, 1, 1);
                else if (ge.OriginType == SimpleGridOriginTypes.ModelXYZ)
                    origin = modelFollowPos;
                else
                    throw new NotImplementedException();

                Matrix rotationMatrix = Matrix.Identity;

                if (ge.AxisType == SimpleGridAxisTypes.XZ)
                    rotationMatrix = Matrix.Identity;
                else if (ge.AxisType == SimpleGridAxisTypes.XY)
                    rotationMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2);
                else if (ge.AxisType == SimpleGridAxisTypes.YZ)
                    rotationMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY(-MathHelper.PiOver2);
                else
                    throw new NotImplementedException();
                Vector3 translation = new Vector3(origin.X % ge.UnitSize, origin.Y % ge.UnitSize, origin.Z % ge.UnitSize);

                float gridPrimScale = ge.DistFalloffEnd + ge.UnitSize;

                //if (!Main.Config.WrapRootMotion)
                //    translation = Vector3.Zero;

                Matrix gridWorldMatrix = Matrix.CreateScale(gridPrimScale) * rotationMatrix * Matrix.CreateTranslation(origin) * Matrix.CreateTranslation(0, physicalDepth, 0);

                effect.GridCellTexture = (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && i == 1) ? Main.GRID_CELL_TEXTURE_2 : Main.GRID_CELL_TEXTURE;
                effect.GridCellTextureThickX = Main.GRID_CELL_TEXTURE_THICK_X;
                effect.GridCellTextureThickY = Main.GRID_CELL_TEXTURE_THICK_Y;

                effect.GridOriginCrossTexture = Main.GRID_ORIGIN_CROSS_TEXTURE;

                //effect.GridCellTexture = Main.GRID_CELL_TEXTURE;

                //origin = -origin;
                effect.AxisType = ge.AxisType;
                effect.Origin = (origin * new Vector3(1, 1, -1));
                effect.OriginSnappedToUnits = ((origin - translation) * new Vector3(1, 1, -1));
                //effect.Origin = Vector3.Zero;

                //camPos = new Vector3(camPos.X % GFX.CurrentWorldView.RootMotionWrapUnit, camPos.Y % GFX.CurrentWorldView.RootMotionWrapUnit, camPos.Z % GFX.CurrentWorldView.RootMotionWrapUnit);

                effect.CameraPosition = camPos * new Vector3(1, 1, 1);

                effect.Depth = depth;
                effect.WireframeOverlay = false;

                effect.AlphaPower = ge.AlphaPower;

                effect.UnitSize = ge.UnitSize;
                effect.ModelSizeMult = gridPrimScale;
                effect.DistFalloffStart = ge.DistFalloffStart;
                effect.DistFalloffEnd = ge.DistFalloffEnd;
                effect.DistFalloffPower = ge.DistFalloffPower;

                if (ge.ColorType == SimpleGridColorTypes.Custom)
                    effect.LineColor = ge.CustomColor;
                else
                    effect.LineColor = GetGridColor(ge.ColorType);

                //if (ge.OriginColorType == SimpleGridColorTypes.Custom)
                //    effect.OriginLineColor = ge.OriginCustomColor;
                //else
                //    effect.OriginLineColor = GetGridColor(ge.OriginColorType);


                GFX.BackfaceCulling = false;
                GFX.Wireframe = false;
                GFX.Device.SetVertexBuffer(VertBuffer);
                GFX.Device.Indices = IndexBuffer;

                GFX.CurrentWorldView.ApplyViewToShader(GFX.NewSimpleGridShader, gridWorldMatrix);

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GFX.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount);
                }


                //depth += 0.0001f;
                physicalDepth += layerDepthChange;

                if (ge.DrawPlaneWireframe)
                {
                    effect.Depth = depth;
                    effect.WireframeOverlay = true;
                    effect.WireframeOverlayColor = ge.PlaneWireframeColor;

                    foreach (var pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GFX.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, IndexBuffer.IndexCount);
                    }

                    //depth += 0.0001f;
                    physicalDepth += layerDepthChange;
                }

                
            }

            GFX.Wireframe = oldGFXWireframe;
            GFX.BackfaceCulling = oldGFXBackfaceCulling;
        }

        public void Dispose()
        {
            VertBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}
