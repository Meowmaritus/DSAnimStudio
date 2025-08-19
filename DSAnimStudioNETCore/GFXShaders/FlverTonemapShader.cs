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

        public int SSAA
        {
            get => Parameters[nameof(SSAA)].GetValueInt32();
            set => Parameters[nameof(SSAA)].SetValue(value);
        }

        public Vector2 ScreenSize
        {
            get => Parameters[nameof(ScreenSize)].GetValueVector2();
            set => Parameters[nameof(ScreenSize)].SetValue(value);
        }

        public Texture2D SpriteTexture
        {
            get => Parameters[nameof(SpriteTexture)].GetValueTexture2D();
            set => Parameters[nameof(SpriteTexture)].SetValue(value);
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
