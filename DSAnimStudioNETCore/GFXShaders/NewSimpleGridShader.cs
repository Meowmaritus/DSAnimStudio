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
    public class NewSimpleGridShader : Effect, IGFXShader<NewSimpleGridShader>
    {
        static NewSimpleGridShader()
        {

        }

        public Texture2D GridCellTexture
        {
            get => Parameters[nameof(GridCellTexture)].GetValueTexture2D();
            set => Parameters[nameof(GridCellTexture)]?.SetValue(value);
        }

        public Texture2D GridCellTextureThickX
        {
            get => Parameters[nameof(GridCellTextureThickX)].GetValueTexture2D();
            set => Parameters[nameof(GridCellTextureThickX)]?.SetValue(value);
        }

        public Texture2D GridCellTextureThickY
        {
            get => Parameters[nameof(GridCellTextureThickY)].GetValueTexture2D();
            set => Parameters[nameof(GridCellTextureThickY)]?.SetValue(value);
        }

        public Texture2D GridOriginCrossTexture
        {
            get => Parameters[nameof(GridOriginCrossTexture)].GetValueTexture2D();
            set => Parameters[nameof(GridOriginCrossTexture)]?.SetValue(value);
        }

        public float UnitSize
        {
            get => Parameters[nameof(UnitSize)].GetValueSingle();
            set => Parameters[nameof(UnitSize)].SetValue(value);
        }

        public float ModelSizeMult
        {
            get => Parameters[nameof(ModelSizeMult)].GetValueSingle();
            set => Parameters[nameof(ModelSizeMult)].SetValue(value);
        }

        public float DistFalloffStart
        {
            get => Parameters[nameof(DistFalloffStart)].GetValueSingle();
            set => Parameters[nameof(DistFalloffStart)].SetValue(value);
        }

        public float DistFalloffEnd
        {
            get => Parameters[nameof(DistFalloffEnd)].GetValueSingle();
            set => Parameters[nameof(DistFalloffEnd)].SetValue(value);
        }

        public float DistFalloffPower
        {
            get => Parameters[nameof(DistFalloffPower)].GetValueSingle();
            set => Parameters[nameof(DistFalloffPower)].SetValue(value);
        }

        public float AlphaPower
        {
            get => Parameters[nameof(AlphaPower)].GetValueSingle();
            set => Parameters[nameof(AlphaPower)].SetValue(value);
        }

        public Vector4 LineColor
        {
            get => Parameters[nameof(LineColor)].GetValueVector4();
            set => Parameters[nameof(LineColor)].SetValue(value);
        }

        public Vector4 OriginLineColor
        {
            get => Parameters[nameof(OriginLineColor)].GetValueVector4();
            set => Parameters[nameof(OriginLineColor)].SetValue(value);
        }

        public float Depth
        {
            get => Parameters[nameof(Depth)].GetValueSingle();
            set => Parameters[nameof(Depth)].SetValue(value);
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

        public NewSimpleGrid.SimpleGridAxisTypes AxisType
        {
            get => (NewSimpleGrid.SimpleGridAxisTypes)Parameters[nameof(AxisType)].GetValueInt32();
            set => Parameters[nameof(AxisType)].SetValue((int)value);
        }

        public Vector3 Origin
        {
            get => Parameters[nameof(Origin)].GetValueVector3();
            set => Parameters[nameof(Origin)].SetValue(value);
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

        public NewSimpleGridShader Effect => this;

        public NewSimpleGridShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            
        }

        public NewSimpleGridShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
            
        }

        public NewSimpleGridShader(Effect cloneSource) : base(cloneSource)
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

        //public Matrix ViewInverse
        //{
        //    get => Parameters[nameof(ViewInverse)].GetValueMatrix();
        //    set => Parameters[nameof(ViewInverse)].SetValue(value);
        //}

        public Matrix Projection
        {
            get => Parameters[nameof(Projection)].GetValueMatrix();
            set => Parameters[nameof(Projection)].SetValue(value);
        }


        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
            //ViewInverse = Matrix.Invert(view);
            //FlipSkybox = Matrix.CreateRotationX(MathHelper.Pi);
        }
    }
}
