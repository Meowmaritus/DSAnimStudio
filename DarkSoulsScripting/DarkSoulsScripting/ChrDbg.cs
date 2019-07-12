using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public static class ChrDbg
    {
        public static bool AllNoMagicQtyConsume
        {
            get { return RBool(0x1376EE7); }
            set { WBool(0x1376EE7, value); }
        }

        public static bool PlayerNoDead
        {
            get { return RBool(0x13784D2); }
            set { WBool(0x13784D2, value); }
        }

        public static bool PlayerExterminate
        {
            get { return RBool(0x13784D3); }
            set { WBool(0x13784D3, value); }
        }

        public static bool AllNoStaminaConsume
        {
            get { return RBool(0x13784E4); }
            set { WBool(0x13784E4, value); }
        }

        public static bool AllNoMPConsume
        {
            get { return RBool(0x13784E5); }
            set { WBool(0x13784E5, value); }
        }

        public static bool AllNoArrowConsume
        {
            get { return RBool(0x13784E6); }
            set { WBool(0x13784E6, value); }
        }

        public static bool PlayerHide
        {
            get { return RBool(0x13784E7); }
            set { WBool(0x13784E7, value); }
        }

        public static bool PlayerSilence
        {
            get { return RBool(0x13784E8); }
            set { WBool(0x13784E8, value); }
        }

        public static bool AllNoDead
        {
            get { return RBool(0x13784E9); }
            set { WBool(0x13784E9, value); }
        }

        public static bool AllNoDamage
        {
            get { return RBool(0x13784EA); }
            set { WBool(0x13784EA, value); }
        }

        public static bool AllNoHit
        {
            get { return RBool(0x13784EB); }
            set { WBool(0x13784EB, value); }
        }

        public static bool AllNoAttack
        {
            get { return RBool(0x13784EC); }
            set { WBool(0x13784EC, value); }
        }

        public static bool AllNoMove
        {
            get { return RBool(0x13784ED); }
            set { WBool(0x13784ED, value); }
        }

        public static bool AllNoUpdateAI
        {
            get { return RBool(0x13784EE); }
            set { WBool(0x13784EE, value); }
        }

        public static bool DisplayMiniCompass
        {
            get { return RBool(0x137851B); }
            set { WBool(0x137851B, value); }
        }

        public static bool DisplayHeightMarker
        {
            get { return RBool(0x1378524); }
            set { WBool(0x1378524, value); }
        }

        public static bool DisplayCompass
        {
            get { return RBool(0x1378525); }
            set { WBool(0x1378525, value); }
        }
    }
}
