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
            get => Parameters["World"].GetValueMatrix();
            set => Parameters["World"].SetValue(value);
        }

        public Matrix View
        {
            get => Parameters["View"].GetValueMatrix();
            set => Parameters["View"].SetValue(value);
        }

        public Matrix Projection
        {
            get => Parameters["Projection"].GetValueMatrix();
            set => Parameters["Projection"].SetValue(value);
        }

        public Vector3 EyePosition
        {
            get => Parameters["EyePosition"].GetValueVector3();
            set => Parameters["EyePosition"].SetValue(value);
        }

        public TextureCube EnvironmentMap
        {
            get => Parameters["EnvironmentMap"].GetValueTextureCube();
            set => Parameters["EnvironmentMap"]?.SetValue(value);
        }

        public float DS3AmbientBrightness
        {
            get => Parameters["DS3AmbientBrightness"].GetValueSingle();
            set => Parameters["DS3AmbientBrightness"].SetValue(value);
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
        }
    }
}
