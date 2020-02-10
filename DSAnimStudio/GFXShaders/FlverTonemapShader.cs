using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public class FlverTonemapShader : Effect, IGFXShader<FlverTonemapShader>
    {
        public FlverTonemapShader Effect => this;

        public Vector2 ScreenSize
        {
            get
            {
                return Parameters["ScreenSize"].GetValueVector2();
            }
            set
            {
                Parameters["ScreenSize"].SetValue(value);
            }
        }


        public FlverTonemapShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
        }

        public FlverTonemapShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
        }

        public FlverTonemapShader(Effect cloneSource) : base(cloneSource)
        {
        }

        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            throw new NotImplementedException();
        }
    }
}
