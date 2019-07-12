using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting.XInput
{
    public struct GamepadState
    {
        public ButtonState Button;
        public float LeftTrigger;
        public float RightTrigger;
        public Vector2 LeftStick;
        public Vector2 RightStick;

        public void ReadFrom(long addr)
        {
            RightTrigger = RByte(addr + 0x26) / 255.0F;
            LeftTrigger = RByte(addr + 0x27) / 255.0F;
            Button = RUInt16(addr + 0x28);
            LeftStick.X = 1.0F * RInt16(addr + 0x2C) / short.MaxValue;
            LeftStick.Y = 1.0F * RInt16(addr + 0x2E) / short.MaxValue;
            RightStick.X = 1.0F * RInt16(addr + 0x30) / short.MaxValue;
            RightStick.Y = 1.0F * RInt16(addr + 0x32) / short.MaxValue;
        }

        public void WriteTo(long addr)
        {
            WByte(addr + 0x26, (byte)(RightTrigger / byte.MaxValue));
            WByte(addr + 0x27, (byte)(LeftTrigger / byte.MaxValue));
            WUInt16(addr + 0x28, Button);
            WInt16(addr + 0x2C, (short)(LeftStick.X * short.MaxValue));
            WInt16(addr + 0x2E, (short)(LeftStick.Y * short.MaxValue));
            WInt16(addr + 0x30, (short)(RightStick.X * short.MaxValue));
            WInt16(addr + 0x32, (short)(RightStick.Y * short.MaxValue));
        }
    }
}
