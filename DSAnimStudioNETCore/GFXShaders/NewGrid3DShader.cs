using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public class NewGrid3DShader : Effect, IGFXShader<NewGrid3DShader>
    {
        static NewGrid3DShader()
        {

        }

        public const int MAX_CFGS = 8;

        public int GridCfgCount = 1;

        public float[] GridCfg_UnitSize = new float[MAX_CFGS];
        public int[] GridCfg_IsOrigin = new int[MAX_CFGS];
        public float[] GridCfg_FadeStartDist = new float[MAX_CFGS];
        public float[] GridCfg_FadeEndDist = new float[MAX_CFGS];
        public float[] GridCfg_CameraFadeStartDist = new float[MAX_CFGS];
        public float[] GridCfg_CameraFadeEndDist = new float[MAX_CFGS];
        public float[] GridCfg_CameraFadePower = new float[MAX_CFGS];
        public Vector4[] GridCfg_LineColor = new Vector4[MAX_CFGS];
        public float[] GridCfg_LineThickness = new float[MAX_CFGS];
        public float[] GridCfg_LineThicknessFade = new float[MAX_CFGS];
        public float[] GridCfg_AnisoDistFadePower = new float[MAX_CFGS];
        public float[] GridCfg_LineThicknessIncreaseFromCameraDist = new float[MAX_CFGS];
        public float[] GridCfg_LineThicknessFadeIncreaseFromCameraDist = new float[MAX_CFGS];
        public float[] GridCfg_LineThicknessIncreaseFromAnisotropic = new float[MAX_CFGS];
        public float[] GridCfg_LineThicknessFadeIncreaseFromAnisotropic = new float[MAX_CFGS];
        public float[] GridCfg_LineThicknessFadePower = new float[MAX_CFGS];

        public void WriteGridCfgToShader()
        {
            Parameters[nameof(GridCfg_UnitSize)]?.SetValue(GridCfg_UnitSize);
            Parameters[nameof(GridCfg_IsOrigin)]?.SetValue(GridCfg_IsOrigin);
            Parameters[nameof(GridCfg_FadeStartDist)]?.SetValue(GridCfg_FadeStartDist);
            Parameters[nameof(GridCfg_FadeEndDist)]?.SetValue(GridCfg_FadeEndDist);
            Parameters[nameof(GridCfg_CameraFadeStartDist)]?.SetValue(GridCfg_CameraFadeStartDist);
            Parameters[nameof(GridCfg_CameraFadeEndDist)]?.SetValue(GridCfg_CameraFadeEndDist);
            Parameters[nameof(GridCfg_CameraFadePower)]?.SetValue(GridCfg_CameraFadePower);
            Parameters[nameof(GridCfg_LineColor)]?.SetValue(GridCfg_LineColor);
            Parameters[nameof(GridCfg_LineThickness)]?.SetValue(GridCfg_LineThickness);
            Parameters[nameof(GridCfg_LineThicknessFade)]?.SetValue(GridCfg_LineThicknessFade);
            Parameters[nameof(GridCfg_AnisoDistFadePower)]?.SetValue(GridCfg_AnisoDistFadePower);
            Parameters[nameof(GridCfg_LineThicknessIncreaseFromCameraDist)]?.SetValue(GridCfg_LineThicknessIncreaseFromCameraDist);
            Parameters[nameof(GridCfg_LineThicknessFadeIncreaseFromCameraDist)]?.SetValue(GridCfg_LineThicknessFadeIncreaseFromCameraDist);
            Parameters[nameof(GridCfg_LineThicknessIncreaseFromAnisotropic)]?.SetValue(GridCfg_LineThicknessIncreaseFromAnisotropic);
            Parameters[nameof(GridCfg_LineThicknessFadeIncreaseFromAnisotropic)]?.SetValue(GridCfg_LineThicknessFadeIncreaseFromAnisotropic);
            Parameters[nameof(GridCfg_LineThicknessFadePower)]?.SetValue(GridCfg_LineThicknessFadePower);

            Parameters[nameof(GridCfgCount)]?.SetValue(GridCfgCount);
        }

        public float UVScaleFactor
        {
            get => Parameters[nameof(UVScaleFactor)].GetValueSingle();
            set => Parameters[nameof(UVScaleFactor)].SetValue(value);
        }

        public float Depth
        {
            get => Parameters[nameof(Depth)].GetValueSingle();
            set => Parameters[nameof(Depth)].SetValue(value);
        }

        public float AnisoDistFadePower
        {
            get => Parameters[nameof(AnisoDistFadePower)].GetValueSingle();
            set => Parameters[nameof(AnisoDistFadePower)].SetValue(value);
        }

        public bool WireframeOverlay
        {
            get => Parameters[nameof(WireframeOverlay)].GetValueBoolean();
            set => Parameters[nameof(WireframeOverlay)].SetValue(value);
        }

        public Vector4 WireframeOverlayColor
        {
            get => Parameters[nameof(WireframeOverlayColor)].GetValueVector4();
            set => Parameters[nameof(WireframeOverlayColor)].SetValue(value);
        }

        public NewGrid3D.AxisTypes AxisType
        {
            get => (NewGrid3D.AxisTypes)Parameters[nameof(AxisType)].GetValueInt32();
            set => Parameters[nameof(AxisType)].SetValue((int)value);
        }

        public Vector3 Origin
        {
            get => Parameters[nameof(Origin)].GetValueVector3();
            set => Parameters[nameof(Origin)].SetValue(value);
        }

        public Vector3 OriginOffsetForWrap
        {
            get => Parameters[nameof(OriginOffsetForWrap)].GetValueVector3();
            set => Parameters[nameof(OriginOffsetForWrap)].SetValue(value);
        }

        public Matrix GridCoordShiftMatrix
        {
            get => Parameters[nameof(GridCoordShiftMatrix)].GetValueMatrix();
            set => Parameters[nameof(GridCoordShiftMatrix)].SetValue(value);
        }

        public Vector3 OriginSnappedToUnits
        {
            get => Parameters[nameof(OriginSnappedToUnits)].GetValueVector3();
            set => Parameters[nameof(OriginSnappedToUnits)].SetValue(value);
        }

        public Vector3 CameraPosition
        {
            get => Parameters[nameof(CameraPosition)].GetValueVector3();
            set => Parameters[nameof(CameraPosition)].SetValue(value);
        }

        public NewGrid3DShader Effect => this;

        public NewGrid3DShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            
        }

        public NewGrid3DShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
            
        }

        public NewGrid3DShader(Effect cloneSource) : base(cloneSource)
        {
            
        }

        public Matrix World
        {
            get => Parameters[nameof(World)].GetValueMatrix();
            set => Parameters[nameof(World)].SetValue(value);
        }

        public Matrix View
        {
            get => Parameters[nameof(View)].GetValueMatrix();
            set => Parameters[nameof(View)].SetValue(value);
        }

        public Matrix Projection
        {
            get => Parameters[nameof(Projection)].GetValueMatrix();
            set => Parameters[nameof(Projection)].SetValue(value);
        }

        public Matrix InverseView
        {
            get => Parameters[nameof(InverseView)].GetValueMatrix();
            set => Parameters[nameof(InverseView)].SetValue(value);
        }

        public Matrix InverseProjection
        {
            get => Parameters[nameof(InverseProjection)].GetValueMatrix();
            set => Parameters[nameof(InverseProjection)].SetValue(value);
        }

        public Matrix InverseWorld
        {
            get => Parameters[nameof(InverseWorld)].GetValueMatrix();
            set => Parameters[nameof(InverseWorld)].SetValue(value);
        }


        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
            InverseView = Matrix.Invert(view);
            InverseProjection = Matrix.Invert(projection);
            InverseWorld = Matrix.Invert(world);
            //ViewInverse = Matrix.Invert(view);
            //FlipSkybox = Matrix.CreateRotationX(MathHelper.Pi);
        }
    }
}
