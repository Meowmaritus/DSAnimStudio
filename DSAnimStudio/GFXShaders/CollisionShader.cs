using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public class CollisionShader : Effect, IGFXShader<CollisionShader>
    {
        public CollisionShader Effect => this;

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

        public Vector4 DiffuseColor
        {
            get => Parameters["DiffuseColor"].GetValueVector4();
            set => Parameters["DiffuseColor"].SetValue(value);
        }

        public Vector3 EyePosition
        {
            get => Parameters["EyePosition"].GetValueVector3();
            set => Parameters["EyePosition"].SetValue(value);
        }

        public CollisionShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public CollisionShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }

        public CollisionShader(Effect cloneSource) : base(cloneSource)
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
