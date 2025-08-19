using Assimp;
using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.GFXShaders;
using FMOD;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.NewSimpleGrid;

namespace DSAnimStudio
{
    public class NewGrid3D : IDisposable
    {
        public void Dispose()
        {
            GridPrim?.Dispose();
        }

        public DbgPrimNewGrid3D GridPrim = new DbgPrimNewGrid3D();
        //public DbgPrimNewGrid3D_Blur GridPrim_Blur = new DbgPrimNewGrid3D_Blur();

        public bool AutoGenEntries = true;

        public GridEntry[] GridEntries = new GridEntry[3];

        public NewGrid3D()
        {
            //GenerateDefaults(GFX.CurrentWorldView);
        }

        public enum OriginTypes
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

        public enum AxisTypes
        {
            None,
            XY = 1,
            XZ = 2,
            YZ = 3,
        }

        public enum GridColorTypes
        {
            Custom = 0,
            Main1u = 1,
            Main10u = 2,
            Main100u = 3,
            Origin = 10,
        }

        public System.Numerics.Vector4 GetGridColor(GridColorTypes type)
        {
            switch (type)
            {
                case GridColorTypes.Main1u: return Main.Colors.ColorGrid1u.ToNVector4();
                case GridColorTypes.Main10u: return Main.Colors.ColorGrid10u.ToNVector4();
                case GridColorTypes.Main100u: return Main.Colors.ColorGrid100u.ToNVector4();
                //case GridColorTypes.Origin: return Main.Colors.ColorGridOrigin.ToNVector4();
                default:
                    return Main.Colors.ColorGrid1u.ToNVector4();
            }
        }

        public AxisTypes AxisType = AxisTypes.XZ;
        public OriginTypes OriginType = OriginTypes.WorldOrigin;
        public bool DrawPlaneWireframe = false;
        public System.Numerics.Vector4 PlaneWireframeColor = new System.Numerics.Vector4(1, 0, 1, 1);


        public class GridEntry
        {
            public bool Enabled = false;
            public GridColorTypes ColorType = GridColorTypes.Main1u;
            public float AlphaMod = 1;
            public System.Numerics.Vector4 CustomColor = new System.Numerics.Vector4(1, 1, 1, 1);
            public float UnitSize = 1;
            public bool IsOrigin = false;
            public float FadeStartDist_Base = 1;
            public float FadeStartDist_Mult = 1;
            public float FadeEndDist_Mult = 1;
            public float FadeEndDist_Base = 1;
            public float CameraFadeStartDist_Base = 1;
            public float CameraFadeStartDist_Mult = 1;
            public float CameraFadeEndDist_Base = 1;
            public float CameraFadeEndDist_Mult = 1;
            public float CameraFadePower = 1;
            public float LineThickness = 0.002f;
            public float LineThicknessFade = 0.01f;
            public float LineThicknessIncreaseFromCameraDist = 32;
            public float LineThicknessIncreaseFromAnisotropic = 1;
            public float LineThicknessFadeIncreaseFromCameraDist = 32;
            public float LineThicknessFadeIncreaseFromAnisotropic = 1;
            public float LineThicknessFadePower = 1;
            public float AnisoDistFadePower_Base = 1;
            public float AnisoDistFadePower_Mult = 1;

            public GridEntry(float hitRadius, WorldView worldView)
            {
                FadeStartDist_Base = hitRadius * 25;
                FadeEndDist_Base = hitRadius * 50;
                FadeStartDist_Mult = worldView.ShowGridDistMult;
                FadeEndDist_Mult = worldView.ShowGridDistMult;
                AnisoDistFadePower_Base = hitRadius * 25;
                AnisoDistFadePower_Mult = worldView.GridFadePowerMult;

                CameraFadeStartDist_Base = 1;
                CameraFadeEndDist_Base = 1;
                CameraFadeStartDist_Mult = 1;
                CameraFadeEndDist_Mult = 1;
            }
        }

        //public Vector3 Origin;

        public Vector3 OriginOffsetForWrap = Vector3.Zero;
        public Matrix OriginOffsetForWrap_RotationMatrix = Matrix.Identity;

        public float UnitSizeMult = 1;
        public float LineThicknessMult = 2;
        public float LineThicknessFadeMult = 2;
        public float LineThicknessFadePowerMult = 1;
        public float FadeStartDistMult = 1;
        public float FadeEndDistMult = 1;
        public float CameraFadeStartDistMult = 1;
        public float CameraFadeEndDistMult = 1;

        public Vector3 WorldShiftOffset;

        public void ResetWorldShiftOffset()
        {
            WorldShiftOffset = Vector3.Zero;
        }
        
        public GridEntry InitGridEntry(int index, float hitRadius, WorldView worldView)
        {
            int prevLength = GridEntries.Length;
            if (prevLength <= index)
            {
                Array.Resize(ref GridEntries, index + 1);
                for (int i = prevLength; i < GridEntries.Length; i++)
                {
                    GridEntries[i] = new GridEntry(hitRadius, worldView);
                }
            }
            GridEntries[index].Enabled = true;
            return GridEntries[index];
        }

        public void GenerateDefaults(WorldView worldView, Model mdl)
        {
            //GridEntries.Clear();

            float hitRadius = mdl?.NpcParam?.HitRadius ?? 1;

            if (hitRadius < 1)
                hitRadius = 1;

            if (GridEntries.Length != 3)
            {
                Array.Resize(ref GridEntries, 3);
            }

            for (int i = 0; i < 3; i++)
            {
                var entry = GridEntries[i];
                if (entry == null)
                    entry = (GridEntries[i] = new GridEntry(hitRadius, worldView));
                entry.Enabled = false;
                entry.ColorType = GridColorTypes.Main1u;
                entry.AlphaMod = 1;
                entry.CustomColor = new System.Numerics.Vector4(1, 1, 1, 1);
                entry.UnitSize = 1;
                entry.IsOrigin = false;
                entry.FadeStartDist_Base = hitRadius * 25;
                entry.FadeStartDist_Mult = worldView.ShowGridDistMult;
                entry.FadeEndDist_Base = hitRadius * 50;
                entry.FadeEndDist_Mult = worldView.ShowGridDistMult;
                entry.CameraFadeStartDist_Base = 1;
                entry.CameraFadeStartDist_Mult = 1;
                entry.CameraFadeEndDist_Mult = 1;
                entry.CameraFadeEndDist_Base = 1;
                entry.CameraFadePower = 1;
                entry.LineThickness = 0.002f;
                entry.LineThicknessFade = 0.01f;
                entry.LineThicknessIncreaseFromCameraDist = 32;
                entry.LineThicknessIncreaseFromAnisotropic = 1;
                entry.LineThicknessFadeIncreaseFromCameraDist = 32;
                entry.LineThicknessFadeIncreaseFromAnisotropic = 1;
                entry.LineThicknessFadePower = 1;
                entry.AnisoDistFadePower_Base = hitRadius * 25;
                entry.AnisoDistFadePower_Mult = worldView.GridFadePowerMult;
            }

            var mainEntry = GridEntries[0];
            mainEntry.Enabled = true;
            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                //mainEntry.AnisoDistFadePower *= 8;
                mainEntry.LineThickness *= 2;
                //mainEntry.FadeStartDist *= 10;
                //mainEntry.FadeEndDist *= 10;
            }


            GridEntry entry10u = GridEntries[1];
            entry10u.Enabled = worldView.ShowGrid10u;
            if (entry10u.Enabled)
            {
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    //entry10u.AnisoDistFadePower *= 8;
                    entry10u.LineThickness *= 2;
                    //entry10u.FadeStartDist_Mult *= 10;
                    //entry10u.FadeEndDist_Mult *= 10;
                }
                entry10u.UnitSize = 10;
                entry10u.ColorType = GridColorTypes.Main10u;

                entry10u.CameraFadeStartDist_Base /= 10;
                entry10u.CameraFadeEndDist_Base /= 10;

            }
            else
            {
                entry10u.Enabled = false;
            }

            GridEntry entry100u = GridEntries[2];
            entry100u.Enabled = worldView.ShowGrid100u;
            if (entry100u.Enabled)
            {
                
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    //entry100u.AnisoDistFadePower *= 8;
                    entry100u.LineThickness *= 2;
                    //entry100u.FadeStartDist *= 10;
                    //entry100u.FadeEndDist *= 10;
                }
                entry100u.UnitSize = 100;
                entry100u.ColorType = GridColorTypes.Main100u;

                entry100u.CameraFadeStartDist_Base /= 100;
                entry100u.CameraFadeEndDist_Base /= 100;
            }
        }

        public void Draw(WorldView worldView)
        {
            worldView.Update(0);

            if (AutoGenEntries)
            {
                OriginType = OriginTypes.ModelXZ;
                bool hasModel = zzz_DocumentManager.CurrentDocument.Scene.AccessMainModel(m =>
                {
                    GenerateDefaults(worldView, m);
                });
                if (!hasModel)
                {
                    GenerateDefaults(worldView, null);
                }
            }

            float depth = 0;

            float currentAlpha = 1;

            if (AxisType == AxisTypes.None)
                return;


            var effect = GFX.NewGrid3DShader.Effect;
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

            if (OriginType == OriginTypes.WorldOrigin)
            {
                origin = Vector3.Zero;
                WorldShiftOffset *= new Vector3(1, 1, 1);
            }
            else if (OriginType == OriginTypes.CameraX)
            {
                origin = camFollowPos * new Vector3(1, 0, 0);
                WorldShiftOffset *= new Vector3(0, 1, 1);
            }
            else if (OriginType == OriginTypes.CameraY)
            {
                origin = camFollowPos * new Vector3(0, 1, 0);
                WorldShiftOffset *= new Vector3(1, 0, 1);
            }
            else if (OriginType == OriginTypes.CameraZ)
            {
                origin = camFollowPos * new Vector3(0, 0, 1);
                WorldShiftOffset *= new Vector3(1, 1, 0);
            }
            else if (OriginType == OriginTypes.CameraXY)
            {
                origin = camFollowPos * new Vector3(1, 1, 0);
                WorldShiftOffset *= new Vector3(0, 0, 1);
            }
            else if (OriginType == OriginTypes.CameraXZ)
            {
                origin = camFollowPos * new Vector3(1, 0, 1);
                WorldShiftOffset *= new Vector3(0, 1, 0);
            }
            else if (OriginType == OriginTypes.CameraYZ)
            {
                origin = camFollowPos * new Vector3(0, 1, 1);
                WorldShiftOffset *= new Vector3(1, 0, 0);
            }
            else if (OriginType == OriginTypes.CameraXYZ)
            {
                origin = camFollowPos;
                WorldShiftOffset *= new Vector3(0, 0, 0);
            }
            else if (OriginType == OriginTypes.ModelX)
            {
                origin = modelFollowPos * new Vector3(1, 0, 0);
                WorldShiftOffset *= new Vector3(0, 1, 1);
            }
            else if (OriginType == OriginTypes.ModelY)
            {
                origin = modelFollowPos * new Vector3(0, 1, 0);
                WorldShiftOffset *= new Vector3(1, 0, 1);
            }
            else if (OriginType == OriginTypes.ModelZ)
            {
                origin = modelFollowPos * new Vector3(0, 0, 1);
                WorldShiftOffset *= new Vector3(1, 1, 0);
            }
            else if (OriginType == OriginTypes.ModelXZ)
            {
                origin = modelFollowPos * new Vector3(1, 0, 1);
                WorldShiftOffset *= new Vector3(0, 1, 0);
            }
            else if (OriginType == OriginTypes.ModelXY)
            {
                origin = modelFollowPos * new Vector3(1, 1, 0);
                WorldShiftOffset *= new Vector3(0, 0, 1);
            }
            else if (OriginType == OriginTypes.ModelYZ)
            {
                origin = modelFollowPos * new Vector3(0, 1, 1);
                WorldShiftOffset *= new Vector3(1, 0, 0);
            }
            else if (OriginType == OriginTypes.ModelXYZ)
            {
                origin = modelFollowPos;
                WorldShiftOffset *= new Vector3(0, 0, 0);
            }
            else
                throw new NotImplementedException();

            Matrix rotationMatrix = Matrix.Identity;

            if (AxisType == AxisTypes.XZ)
                rotationMatrix = Matrix.Identity;
            else if (AxisType == AxisTypes.XY)
                rotationMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2);
            else if (AxisType == AxisTypes.YZ)
                rotationMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateRotationY(-MathHelper.PiOver2);
            else
                throw new NotImplementedException();
            float unitSize = worldView.RootMotionWrapUnit;
            Vector3 translation = new Vector3(origin.X % unitSize, origin.Y % unitSize, origin.Z % unitSize);

            float longestFadeDist = 0;
            foreach (var ge in GridEntries)
            {
                longestFadeDist = MathF.Max(longestFadeDist, MathF.Max(ge.FadeEndDist_Base * ge.FadeEndDist_Mult, ge.FadeStartDist_Base * ge.FadeStartDist_Mult));
            }

            float gridPrimScale = longestFadeDist;

            //if (!Main.Config.WrapRootMotion)
            //    translation = Vector3.Zero;

            GridPrim.Transform = new Transform(Matrix.CreateScale(gridPrimScale) * rotationMatrix * Matrix.CreateTranslation(origin + WorldShiftOffset));
            GridPrim.Wireframe = false;

            effect.UVScaleFactor = gridPrimScale;

            effect.Parameters["SSAA"].SetValue(GFX.EffectiveSSAA > 1 ? 1 : 2.0f);

            //origin = -origin;
            effect.AxisType = AxisType;
            effect.Origin = (origin * new Vector3(1, 1, -1));
            effect.OriginOffsetForWrap = OriginOffsetForWrap * new Vector3(1, 1, -1);
            //effect.GridCoordShiftMatrix = Matrix.Identity;
            effect.OriginSnappedToUnits = ((origin - translation) * new Vector3(1, 1, -1));
            //effect.Origin = Vector3.Zero;

            //camPos = new Vector3(camPos.X % GFX.CurrentWorldView.RootMotionWrapUnit, camPos.Y % GFX.CurrentWorldView.RootMotionWrapUnit, camPos.Z % GFX.CurrentWorldView.RootMotionWrapUnit);

            effect.CameraPosition = camPos * new Vector3(1, 1, 1);
            


            //effect.TestTexture = Main.RenderTarget0_Depth;
            //effect.TestTextureSize = new Vector2((float)Main.RenderTarget0_Depth.Width / (float)GFX.EffectiveSSAA, (float)Main.RenderTarget0_Depth.Height / (float)GFX.EffectiveSSAA);
            //effect.TestTexturePos = GFX.Device.Viewport.Bounds.TopLeftCorner();

            GridPrim.BackfaceCulling = false;

            //effect.NearClipDist = GFX.CurrentWorldView.ProjectionNearClipDist;
            //effect.FarClipDist = GFX.CurrentWorldView.ProjectionFarClipDist;

            int geIndex = 0;

            foreach (var ge in GridEntries)
            {
                if (!ge.Enabled)
                    continue;

                effect.GridCfg_UnitSize[geIndex] = ge.UnitSize;
                effect.GridCfg_IsOrigin[geIndex] = ge.IsOrigin ? 1 : 0;
                effect.GridCfg_FadeStartDist[geIndex] = ge.FadeStartDist_Base * ge.FadeStartDist_Mult * FadeStartDistMult;
                effect.GridCfg_FadeEndDist[geIndex] = ge.FadeEndDist_Base * ge.FadeEndDist_Mult * FadeEndDistMult;
                effect.GridCfg_CameraFadeStartDist[geIndex] = ge.CameraFadeStartDist_Base * ge.CameraFadeStartDist_Mult * CameraFadeStartDistMult;
                effect.GridCfg_CameraFadeEndDist[geIndex] = ge.CameraFadeEndDist_Base * ge.CameraFadeEndDist_Mult * CameraFadeEndDistMult;
                effect.GridCfg_CameraFadePower[geIndex] = ge.CameraFadePower;

                System.Numerics.Vector4 color = ge.CustomColor;

                if (ge.ColorType != GridColorTypes.Custom)
                    color = GetGridColor(ge.ColorType);

                color *= ge.AlphaMod;

                effect.GridCfg_LineColor[geIndex] = color;

                effect.GridCfg_LineThickness[geIndex] = ge.LineThickness * LineThicknessMult;
                effect.GridCfg_LineThicknessFade[geIndex] = ge.LineThicknessFade * LineThicknessFadeMult;
                effect.GridCfg_AnisoDistFadePower[geIndex] = ge.AnisoDistFadePower_Base * ge.AnisoDistFadePower_Mult;
                effect.GridCfg_LineThicknessIncreaseFromCameraDist[geIndex] = ge.LineThicknessIncreaseFromCameraDist;
                effect.GridCfg_LineThicknessIncreaseFromAnisotropic[geIndex] = ge.LineThicknessIncreaseFromAnisotropic;
                effect.GridCfg_LineThicknessFadeIncreaseFromCameraDist[geIndex] = ge.LineThicknessFadeIncreaseFromCameraDist;
                effect.GridCfg_LineThicknessFadeIncreaseFromAnisotropic[geIndex] = ge.LineThicknessFadeIncreaseFromAnisotropic;
                effect.GridCfg_LineThicknessFadePower[geIndex] = ge.LineThicknessFadePower * LineThicknessFadePowerMult;
                

                geIndex++;
                if (geIndex >= NewGrid3DShader.MAX_CFGS)
                    break;
            }
            effect.GridCfgCount = geIndex;
            effect.WriteGridCfgToShader();
            

            effect.Depth = depth;
            effect.WireframeOverlay = false;
            GridPrim.Wireframe = false;
            GridPrim.Draw(true, null, Matrix.Identity);

            depth += 0.001f;

            if (DrawPlaneWireframe)
            {
                effect.Depth = depth;
                effect.WireframeOverlay = true;
                effect.WireframeOverlayColor = PlaneWireframeColor;
                GridPrim.Wireframe = true;
                GridPrim.Draw(true, null, Matrix.Identity);

                depth += 0.001f;
            }

            //blurEffect.Depth = depth;
            //blurEffect.CameraPosition = camPos * new Vector3(1, 1, 1);

            //blurEffect.Parameters["BufferTex"]?.SetValue(Main.RenderTarget0_Color);
            //blurEffect.BufferTexSize = new Vector2(Main.RenderTarget0_Color.Width, Main.RenderTarget0_Color.Height);
            //blurEffect.BlurAnisoPower = ge.BlurAnisoPower;
            //blurEffect.BlurDirections = ge.BlurDirections;
            //blurEffect.BlurQuality = ge.BlurQuality;
            //blurEffect.BlurSize = ge.BlurSize;
            //GridPrim_Blur.BackfaceCulling = false;
            //GridPrim_Blur.Transform = new Transform(Matrix.CreateScale(gridPrimScale) * rotationMatrix * Matrix.CreateTranslation(origin));
            //GridPrim_Blur.Wireframe = false;
            //GridPrim_Blur.Draw(true, null, Matrix.Identity);
            //depth += 0.0001f;

            

            
        }
    }
}
