using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public class DbgPrimSolidShader : BasicEffect, IGFXShader<DbgPrimSolidShader>
    {
        public DbgPrimSolidShader Effect => this;

        public DbgPrimSolidShader(GraphicsDevice device) : base(device)
        {
            LightingEnabled = true;
            VertexColorEnabled = true;
            DiffuseColor = Vector3.One * 2;
            TextureEnabled = false;
        }

        protected DbgPrimSolidShader(BasicEffect cloneSource) : base(cloneSource)
        {
            LightingEnabled = true;
            VertexColorEnabled = true;
            DiffuseColor = Vector3.One * 2;
            TextureEnabled = false;
        }

        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
        }
    }
}
