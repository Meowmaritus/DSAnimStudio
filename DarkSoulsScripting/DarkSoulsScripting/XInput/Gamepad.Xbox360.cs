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
    public sealed partial class Gamepad
    {
        public class Xbox360 : GamepadBase
        {
            public bool    A      { get => curState.Button[Button.FaceBottom];          set => curState.Button[Button.FaceBottom] = value; }
            public bool    B      { get => curState.Button[Button.FaceRight];           set => curState.Button[Button.FaceRight] = value; }
            public bool    X      { get => curState.Button[Button.FaceLeft];            set => curState.Button[Button.FaceLeft] = value; }
            public bool    Y      { get => curState.Button[Button.FaceTop];             set => curState.Button[Button.FaceTop] = value; }
            public bool    LB     { get => curState.Button[Button.ShoulderLeft];        set => curState.Button[Button.ShoulderLeft] = value; }
            public bool    RB     { get => curState.Button[Button.ShoulderRight];       set => curState.Button[Button.ShoulderRight] = value; }
            public bool    Start  { get => curState.Button[Button.MenuPrimary];         set => curState.Button[Button.MenuPrimary] = value; }
            public bool    Back   { get => curState.Button[Button.MenuSecondary];       set => curState.Button[Button.MenuSecondary] = value; }
            public bool    Guide  { get => curState.Button[Button.MenuTertiary];        set => curState.Button[Button.MenuTertiary] = value; }
            public bool    LThumb { get => curState.Button[Button.StickPressLeft];      set => curState.Button[Button.StickPressLeft] = value; }
            public bool    RThumb { get => curState.Button[Button.StickPressRight];     set => curState.Button[Button.StickPressRight] = value; }
            public float   LT     { get => curState.LeftTrigger;                        set => curState.LeftTrigger = value; }
            public float   RT     { get => curState.RightTrigger;                       set => curState.RightTrigger = value; }

            public bool    PrevA      => prevState.Button[Button.FaceBottom];
            public bool    PrevB      => prevState.Button[Button.FaceRight];
            public bool    PrevX      => prevState.Button[Button.FaceLeft];
            public bool    PrevY      => prevState.Button[Button.FaceTop];
            public bool    PrevLB     => prevState.Button[Button.ShoulderLeft];
            public bool    PrevRB     => prevState.Button[Button.ShoulderRight];
            public bool    PrevStart  => prevState.Button[Button.MenuPrimary];
            public bool    PrevBack   => prevState.Button[Button.MenuSecondary];
            public bool    PrevGuide  => prevState.Button[Button.MenuTertiary];
            public bool    PrevLThumb => prevState.Button[Button.StickPressLeft];
            public bool    PrevRThumb => prevState.Button[Button.StickPressRight];
            public float   PrevLT     => prevState.LeftTrigger;
            public float   PrevRT     => prevState.RightTrigger;
        }
    }
}
