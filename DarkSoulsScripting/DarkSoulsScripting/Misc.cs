using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public static class Misc
    {

        public static bool DisablePad
        {
            get { return RBool(0x1378622); }
            set { WBool(0x1378622, value); }
        }

        public static float Unknown_MakeEveryoneTPose
        {
            get { return RFloat(0x11E7CA0); }
            set { WFloat(0x11E7CA0, value); }
        }

        public static float Unknown_StrangeLighting
        {
            get { return RFloat(0x11E7CEC); }
            set { WFloat(0x11E7CEC, value); }
        }

        public static float Unknown_LightRelated
        {
            get { return RFloat(0x11E7D34); }
            set { WFloat(0x11E7D34, value); }
        }

        public static float Unknown_FOVRelated_1_14_Default
        {
            get { return RFloat(0x11E7DD4); }
            set { WFloat(0x11E7DD4, value); }
        }

        public static int ResistRegenRate
        {
            get { return RInt32(0x12DF020); }
            set { WInt32(0x12DF020, value); }
        }

        public static bool IsRevengeRequested
        {
            get { return RBool(0x13786F8); }
            set { WBool(0x13786F8, value); }
        }

        public static int GetPlayerCurrentAnimationAddress() => RInt32(RInt32(RInt32(RInt32(0x12E29E8) + 0x10) + 0x38) + 0x46C) + 0x60;

        public static int PlayerCurrentAnimation
        {
            get { return RInt32(GetPlayerCurrentAnimationAddress()); }
            set { WInt32(GetPlayerCurrentAnimationAddress(), value); }
        }
    }
}
