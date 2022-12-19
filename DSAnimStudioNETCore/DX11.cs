using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dx = SharpDX.Direct3D11;

namespace DSAnimStudio
{
    public class DX11
    {
        public static dx.Device Device;
        public static void Init(Microsoft.Xna.Framework.Graphics.GraphicsDevice monogameDevice)
        {
            Device = (dx.Device)monogameDevice.Handle;
        }
    }
}
