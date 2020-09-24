using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public class SkyboxShader : Effect, IGFXShader<SkyboxShader>
    {
        public SkyboxShader Effect => this;

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

        public Matrix FlipSkybox
        {
            get => Parameters[nameof(FlipSkybox)].GetValueMatrix();
            set => Parameters[nameof(FlipSkybox)].SetValue(value);
        }

        public Vector3 EyePosition
        {
            get => Parameters[nameof(EyePosition)].GetValueVector3();
            set => Parameters[nameof(EyePosition)].SetValue(value);
        }

        //public Vector3 MotionBlurVector
        //{
        //    get => Parameters[nameof(MotionBlurVector)].GetValueVector3();
        //    set => Parameters[nameof(MotionBlurVector)].SetValue(value);
        //}

        //public int NumMotionBlurSamples
        //{
        //    get => Parameters[nameof(NumMotionBlurSamples)].GetValueInt32();
        //    set => Parameters[nameof(NumMotionBlurSamples)].SetValue(value);
        //}

        public TextureCube EnvironmentMap
        {
            get => Parameters[nameof(EnvironmentMap)].GetValueTextureCube();
            set => Parameters[nameof(EnvironmentMap)]?.SetValue(value);
        }

        public float AmbientLightMult
        {
            get => Parameters[nameof(AmbientLightMult)].GetValueSingle();
            set => Parameters[nameof(AmbientLightMult)].SetValue(value);
        }

        public float SceneBrightness
        {
            get => Parameters[nameof(SceneBrightness)].GetValueSingle();
            set => Parameters[nameof(SceneBrightness)].SetValue(value);
        }

        

        public SkyboxShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public SkyboxShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }

        public SkyboxShader(Effect cloneSource) : base(cloneSource)
        {
        }

        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
            //ViewInverse = Matrix.Invert(view);
            FlipSkybox = Matrix.CreateRotationX(MathHelper.Pi);
        }
    }
}
