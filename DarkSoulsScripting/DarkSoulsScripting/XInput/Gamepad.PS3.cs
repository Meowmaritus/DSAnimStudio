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
        public class PS3 : GamepadBase
        {
            public bool    Cross    { get => curState.Button[Button.FaceBottom];          set => curState.Button[Button.FaceBottom] = value; }
            public bool    Circle   { get => curState.Button[Button.FaceRight];           set => curState.Button[Button.FaceRight] = value; }
            public bool    Square   { get => curState.Button[Button.FaceLeft];            set => curState.Button[Button.FaceLeft] = value; }
            public bool    Triangle { get => curState.Button[Button.FaceTop];             set => curState.Button[Button.FaceTop] = value; }
            public bool    L1       { get => curState.Button[Button.ShoulderLeft];        set => curState.Button[Button.ShoulderLeft] = value; }
            public bool    R1       { get => curState.Button[Button.ShoulderRight];       set => curState.Button[Button.ShoulderRight] = value; }
            public bool    Start    { get => curState.Button[Button.MenuPrimary];         set => curState.Button[Button.MenuPrimary] = value; }
            public bool    Select   { get => curState.Button[Button.MenuSecondary];       set => curState.Button[Button.MenuSecondary] = value; }
            public bool    PS       { get => curState.Button[Button.MenuTertiary];        set => curState.Button[Button.MenuTertiary] = value; }
            public bool    L3       { get => curState.Button[Button.StickPressLeft];      set => curState.Button[Button.StickPressLeft] = value; }
            public bool    R3       { get => curState.Button[Button.StickPressRight];     set => curState.Button[Button.StickPressRight] = value; }
            public float   L2       { get => curState.LeftTrigger;                        set => curState.LeftTrigger = value; }
            public float   R2       { get => curState.RightTrigger;                       set => curState.RightTrigger = value; }

            public bool    PrevCross    => prevState.Button[Button.FaceBottom];
            public bool    PrevCircle   => prevState.Button[Button.FaceRight];
            public bool    PrevSquare   => prevState.Button[Button.FaceLeft];
            public bool    PrevTriangle => prevState.Button[Button.FaceTop];
            public bool    PrevL1       => prevState.Button[Button.ShoulderLeft];
            public bool    PrevR1       => prevState.Button[Button.ShoulderRight];
            public bool    PrevStart    => prevState.Button[Button.MenuPrimary];
            public bool    PrevSelect   => prevState.Button[Button.MenuSecondary];
            public bool    PrevPS       => prevState.Button[Button.MenuTertiary];
            public bool    PrevL3       => prevState.Button[Button.StickPressLeft];
            public bool    PrevR3       => prevState.Button[Button.StickPressRight];
            public float   PrevL2       => prevState.LeftTrigger;
            public float   PrevR2       => prevState.RightTrigger;
            
            //For people who don't know the name of the Cross button:
            public bool X { get => curState.Button[Button.FaceBottom]; set => curState.Button[Button.FaceBottom] = value; }
            public bool PrevX => curState.Button[Button.FaceBottom];
        }
    }
}
