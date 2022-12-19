using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class GlobalInputState
    {
        public static KeyboardState Keyboard;
        public static MouseState Mouse;
        public static GamePadState GamePad1;

        public static void Update()
        {
            Keyboard = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            GamePad1 = GamePad.GetState(0);
        }
    }
}
