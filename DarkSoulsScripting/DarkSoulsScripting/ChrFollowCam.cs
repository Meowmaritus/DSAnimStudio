using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public static class ChrFollowCam
    {
        public static int Address => RInt32(RInt32(RInt32(0x137D6DC) + 0x3C) + 0x60);

        public static float FovY
        {
            get => RFloat(Address + 0x50);
            set => WFloat(Address + 0x50, value);
        }

        public static float DrawDistance
        {
            get => RFloat(Address + 0x5C);
            set => WFloat(Address + 0x5C, value);
        }

        public static float RotX
        {
            get => RFloat(Address + 0xE0);
            set => WFloat(Address + 0xE0, value);
        }

        public static float RotY
        {
            get => RFloat(Address + 0xE4);
            set => WFloat(Address + 0xE4, value);
        }

        public static float RotZ
        {
            get => RFloat(Address + 0xE8);
            set => WFloat(Address + 0xE8, value);
        }

        public static float PosX
        {
            get => RFloat(Address + 0x100);
            set => WFloat(Address + 0x100, value);
        }

        public static float PosY
        {
            get => RFloat(Address + 0x104);
            set => WFloat(Address + 0x104, value);
        }

        public static float PosZ
        {
            get => RFloat(Address + 0x108);
            set => WFloat(Address + 0x108, value);
        }

        public static float CamRotX
        {
            get => RFloat(Address + 0x140);
            set => WFloat(Address + 0x140, value);
        }

        //TODO: CONFIRM
        public static float CamRotY
        {
            get => RFloat(Address + 0x144);
            set => WFloat(Address + 0x144, value);
        }

        //TODO: CONFIRM
        public static float CamRotZ
        {
            get => RFloat(Address + 0x148);
            set => WFloat(Address + 0x148, value);
        }

        public static float TargetRotX
        {
            get => RFloat(Address + 0x150);
            set => WFloat(Address + 0x150, value);
        }

        //TODO: CONFIRM
        public static float TargetRotY
        {
            get => RFloat(Address + 0x154);
            set => WFloat(Address + 0x154, value);
        }

        //TODO: CONFIRM
        public static float TargetRotZ
        {
            get => RFloat(Address + 0x158);
            set => WFloat(Address + 0x158, value);
        }

        /*
        +0x0204	float	X RotSpeedMin
        +0x0208	float	Y RotSpeedMin
        +0x020C	float	X RotSpeedMax
        +0x0210	float	Y RotSpeedMax
        +0x021C	float	X RotHiSpeedMin
        +0x0220	float	Y RotHiSpeedMin
        +0x0224	float	X RotHiSpeedMax
        +0x0228	float	Y RotHiSpeedMax

         */
    }
}
