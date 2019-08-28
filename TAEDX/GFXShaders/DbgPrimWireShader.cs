using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.GFXShaders
{
    public class DbgPrimWireShader : BasicEffect, IGFXShader<DbgPrimWireShader>
    {
        public DbgPrimWireShader Effect => this;

        public DbgPrimWireShader(GraphicsDevice device) : base(device)
        {
            LightingEnabled = false;
            VertexColorEnabled = true;
            DiffuseColor = Main.SELECTED_MESH_WIREFRAME_COLOR.ToVector3();
            TextureEnabled = false;
        }

        protected DbgPrimWireShader(BasicEffect cloneSource) : base(cloneSource)
        {
            LightingEnabled = false;
            VertexColorEnabled = true;
            DiffuseColor = Main.SELECTED_MESH_WIREFRAME_COLOR.ToVector3();
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
