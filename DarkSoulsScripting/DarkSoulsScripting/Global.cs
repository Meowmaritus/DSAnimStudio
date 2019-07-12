using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public static class Global
    {
        public static int Address => RInt32(0x1378560);


        public static byte AreaID
        {
            get { return RByte(Address + 0xB); }
            set { WByte(Address + 0xB, value); }
        }

        public static byte BlockID
        {
            get { return RByte(Address + 0xA); }
            set { WByte(Address + 0xA, value); }
        }

        public static bool IsAliveMotion
        {
            get { return RBool(Address + 0x1C); }
            set { WBool(Address + 0x1C, value); }
        }

        public static bool IsReviveWait
        {
            get { return RBool(Address + 0x1F); }
            set { WBool(Address + 0x1F, value); }
        }
    }
}
