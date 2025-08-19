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
    public class NewGrid3DShader_Blur : Effect, IGFXShader<NewGrid3DShader_Blur>
    {
        static NewGrid3DShader_Blur()
        {

        }

        public Vector3 CameraPosition
        {
            get => Parameters[nameof(CameraPosition)].GetValueVector3();
            set => Parameters[nameof(CameraPosition)].SetValue(value);
        }

        public float Depth
        {
            get => Parameters[nameof(Depth)].GetValueSingle();
            set => Parameters[nameof(Depth)].SetValue(value);
        }

        public float BlurAnisoPower
        {
            get => Parameters[nameof(BlurAnisoPower)].GetValueSingle();
            set => Parameters[nameof(BlurAnisoPower)].SetValue(value);
        }



        public float BlurDirections
        {
            get => Parameters[nameof(BlurDirections)].GetValueSingle();
            set => Parameters[nameof(BlurDirections)].SetValue(value);
        }

        public float BlurQuality
        {
            get => Parameters[nameof(BlurQuality)].GetValueSingle();
            set => Parameters[nameof(BlurQuality)].SetValue(value);
        }

        public float BlurSize
        {
            get => Parameters[nameof(BlurSize)].GetValueSingle();
            set => Parameters[nameof(BlurSize)].SetValue(value);
        }


        public Vector2 BufferTexSize
        {
            get => Parameters[nameof(BufferTexSize)].GetValueVector2();
            set => Parameters[nameof(BufferTexSize)].SetValue(value);
        }

        public Texture2D BufferTex
        {
            get => Parameters[nameof(BufferTex)].GetValueTexture2D();
            set => Parameters[nameof(BufferTex)].SetValue(value);
        }

        public NewGrid3DShader_Blur Effect => this;

        public NewGrid3DShader_Blur(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            
        }

        public NewGrid3DShader_Blur(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
            
        }

        public NewGrid3DShader_Blur(Effect cloneSource) : base(cloneSource)
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
