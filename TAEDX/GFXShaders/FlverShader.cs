using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.GFXShaders
{
    public class FlverShader : Effect, IGFXShader<FlverShader>
    {
        public FlverShader Effect => this;

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

        public Vector4 AmbientColor
        {
            get => Parameters["AmbientColor"].GetValueVector4();
            set => Parameters["AmbientColor"].SetValue(value);
        }

        public float AmbientIntensity
        {
            get => Parameters["AmbientIntensity"].GetValueSingle();
            set => Parameters["AmbientIntensity"].SetValue(value);
        }

        public Vector3 LightDirection
        {
            get => Parameters["LightDirection"].GetValueVector3();
            set => Parameters["LightDirection"].SetValue(value);
        }

        public Vector4 DiffuseColor
        {
            get => Parameters["DiffuseColor"].GetValueVector4();
            set => Parameters["DiffuseColor"].SetValue(value);
        }

        public float DiffuseIntensity
        {
            get => Parameters["DiffuseIntensity"].GetValueSingle();
            set => Parameters["DiffuseIntensity"].SetValue(value);
        }

        public Vector4 SpecularColor
        {
            get => Parameters["SpecularColor"].GetValueVector4();
            set => Parameters["SpecularColor"].SetValue(value);
        }

        public float SpecularPower
        {
            get => Parameters["SpecularPower"].GetValueSingle();
            set => Parameters["SpecularPower"].SetValue(value);
        }

        public Vector3 EyePosition
        {
            get => Parameters["EyePosition"].GetValueVector3();
            set => Parameters["EyePosition"].SetValue(value);
        }

        public float NormalMapCustomZ
        {
            get => Parameters["NormalMapCustomZ"].GetValueSingle();
            set => Parameters["NormalMapCustomZ"].SetValue(value);
        }

        public Texture2D ColorMap
        {
            get => Parameters["ColorMap"].GetValueTexture2D();
            set => Parameters["ColorMap"].SetValue(value);
        }

        public Texture2D NormalMap
        {
            get => Parameters["NormalMap"].GetValueTexture2D();
            set => Parameters["NormalMap"].SetValue(value);
        }

        public Texture2D SpecularMap
        {
            get => Parameters["SpecularMap"].GetValueTexture2D();
            set => Parameters["SpecularMap"].SetValue(value);
        }

        public Texture2D LightMap1
        {
            get => Parameters["LightMap1"].GetValueTexture2D();
            set => Parameters["LightMap1"].SetValue(value);
        }

        public Texture2D LightMap2
        {
            get => Parameters["LightMap2"].GetValueTexture2D();
            set => Parameters["LightMap2"].SetValue(value);
        }

        public FlverShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public FlverShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }

        public FlverShader(Effect cloneSource) : base(cloneSource)
        {
        }

        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
        }
    }
}
