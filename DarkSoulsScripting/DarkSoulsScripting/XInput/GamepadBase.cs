using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Managed.X86;
using static DarkSoulsScripting.Hook;
using System.Numerics;

namespace DarkSoulsScripting.XInput
{
    public abstract class GamepadBase
    {
        protected GamepadState curState;
        protected GamepadState prevState;

        public GamepadState CurrentState => curState;
        public GamepadState PreviousState => prevState;

        public bool    Up       { get => curState.Button[Button.DirectionalPadUp];    set => curState.Button[Button.DirectionalPadUp] = value; }
        public bool    Down     { get => curState.Button[Button.DirectionalPadDown];  set => curState.Button[Button.DirectionalPadDown] = value; }
        public bool    Left     { get => curState.Button[Button.DirectionalPadLeft];  set => curState.Button[Button.DirectionalPadLeft] = value; }
        public bool    Right    { get => curState.Button[Button.DirectionalPadRight]; set => curState.Button[Button.DirectionalPadRight] = value; }
        public Vector2 LS       { get => curState.LeftStick;                          set => curState.LeftStick = value; }
        public Vector2 RS       { get => curState.RightStick;                         set => curState.RightStick = value; }

        public bool    PrevUp    => prevState.Button[Button.DirectionalPadUp];
        public bool    PrevDown  => prevState.Button[Button.DirectionalPadDown];
        public bool    PrevLeft  => prevState.Button[Button.DirectionalPadLeft];
        public bool    PrevRight => prevState.Button[Button.DirectionalPadRight];
        public Vector2 PrevLS    => prevState.LeftStick;
        public Vector2 PrevRS    => prevState.RightStick;

        public void Read()
        {
            prevState = curState;
            curState.ReadFrom(GetAddress());
        }

        public void Write()
        {
            curState.WriteTo(GetAddress());
        }
        
        private static uint __xinput = 0xFFFFFFFF;
        public static uint XInputModuleAddress
        {
            get
            {
                if (__xinput == 0xFFFFFFFF)
                {
                    __xinput = DARKSOULS.ModuleOffsets["XINPUT1_3.DLL"][0];
                }
                return __xinput;
            }
        }

        public static uint GetAddress() => RUInt32(RUInt32(XInputModuleAddress + 0x10C44));

        public static void ApplyInputDisablePatch()
        {
            DARKSOULS.Suspend();
            WBytes(XInputModuleAddress + 0x650A, new byte[] { 0x90, 0x90, 0x90 });
            WBytes(XInputModuleAddress + 0x6516, new byte[] { 0x90, 0x90, 0x90 });
            WUInt32(XInputModuleAddress + 0x6597, 0x90909090);
            WUInt32(XInputModuleAddress + 0x65AB, 0x90909090);
            WUInt32(XInputModuleAddress + 0x652F, 0x90909090);
            WUInt32(XInputModuleAddress + 0x6549, 0x90909090);
            WUInt32(XInputModuleAddress + 0x6563, 0x90909090);
            WUInt32(XInputModuleAddress + 0x657D, 0x90909090);
            WBytes(0xF72543, new byte[] { 0xB0, 0x01, 0x90 });
            DARKSOULS.Resume();
        }

        public static void UndoInputDisablePatch()
        {
            DARKSOULS.Suspend();
            WBytes(XInputModuleAddress + 0x650A, new byte[] { 0x88, 0x50, 0x2A });
            WBytes(XInputModuleAddress + 0x6516, new byte[] { 0x88, 0x50, 0x2B });
            WUInt32(XInputModuleAddress + 0x6597, 0x66894A28);
            WUInt32(XInputModuleAddress + 0x65AB, 0x66894A28);
            WUInt32(XInputModuleAddress + 0x652F, 0x66894A2C);
            WUInt32(XInputModuleAddress + 0x6549, 0x66894A2E);
            WUInt32(XInputModuleAddress + 0x6563, 0x66894A30);
            WUInt32(XInputModuleAddress + 0x657D, 0x66894A32);
            WBytes(0xF72543, new byte[] { 0x0F, 0x94, 0xC0 });
            DARKSOULS.Resume();
        }
    }
}
