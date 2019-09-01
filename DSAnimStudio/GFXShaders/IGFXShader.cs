using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public interface IGFXShader<T>
        where T : Effect
    {
        T Effect { get; }
        void ApplyWorldView(Matrix world, Matrix view, Matrix projection);
    }
}
