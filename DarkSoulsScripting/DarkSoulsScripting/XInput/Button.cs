using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsScripting.XInput
{
    [Flags]
    public enum Button : ushort
    {
        None                = 0b0000000000000000,
        FaceTop             = 0b1000000000000000,
        FaceLeft            = 0b0100000000000000,
        FaceRight           = 0b0010000000000000,
        FaceBottom          = 0b0001000000000000,
        Unk1                = 0b0000100000000000,
        MenuTertiary        = 0b0000010000000000,
        ShoulderRight       = 0b0000001000000000,
        ShoulderLeft        = 0b0000000100000000,
        StickPressRight     = 0b0000000010000000,
        StickPressLeft      = 0b0000000001000000,
        MenuSecondary       = 0b0000000000100000,
        MenuPrimary         = 0b0000000000010000,
        DirectionalPadRight = 0b0000000000001000,
        DirectionalPadLeft  = 0b0000000000000100,
        DirectionalPadDown  = 0b0000000000000010,
        DirectionalPadUp    = 0b0000000000000001,
    }
}
