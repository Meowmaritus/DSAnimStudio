using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public enum ClearState : int
    {
        none = 0,
        good = 1,
        bad = 2
    }

    public static class Game
    {
        public static Func<int> AddressReadFunction = () => RInt32(0x1378700);
        public static int Address => AddressReadFunction();

        public static int LocalPlayerStatsPtr
        {
            get { return RInt32(Address + 0x8); }
            set { WInt32(Address + 0x8, value); }
        }

        public static int OptionsPtr
        {
            get { return RInt32(Address + 0x2C); }
            set { WInt32(Address + 0x2C, value); }
        }

        public static int TendencyPtr
        {
            get { return RInt32(Address + 0x38); }
            set { WInt32(Address + 0x38, value); }
        }

        public static PlayerStats LocalPlayerStats = null;
        public static GameOptions Options = null;
        public static GameTendency Tendency = null;

        static Game()
        {
            LocalPlayerStats = new PlayerStats() { AddressReadFunc = () => LocalPlayerStatsPtr };
            Options = new GameOptions() { AddressReadFunc = () => OptionsPtr };
            Tendency = new GameTendency() { AddressReadFunc = () => TendencyPtr };
        }

        public static int ClearCount
        {
            get { return RInt32(Address + 0x3C); }
            set { WInt32(Address + 0x3C, value); }
        }

        public static ClearState ClearState
        {
            get { return (ClearState)RInt32(Address + 0x40); }
            set { WInt32(Address + 0x40, (int)value); }
        }

        public static int FullRecover
        {
            get { return RInt32(Address + 0x44); }
            set { WInt32(Address + 0x44, value); }
        }

        public static int ItemComplete
        {
            get { return RInt32(Address + 0x48); }
            set { WInt32(Address + 0x48, value); }
        }

        public static int RescueWhite
        {
            get { return RInt32(Address + 0x4C); }
            set { WInt32(Address + 0x4C, value); }
        }

        //[sic] lol
        public static int KillBlack
        {
            get { return RInt32(Address + 0x50); }
            set { WInt32(Address + 0x50, value); }
        }

        public static int TrueDeath
        {
            get { return RInt32(Address + 0x54); }
            set { WInt32(Address + 0x54, value); }
        }

        public static int TrueDeathNum
        {
            get { return RInt32(Address + 0x58); }
            set { WInt32(Address + 0x58, value); }
        }

        public static int DeathNum
        {
            get { return RInt32(Address + 0x5C); }
            set { WInt32(Address + 0x5C, value); }
        }

        public static int IngameTime
        {
            get { return RInt32(Address + 0x68); }
            set { WInt32(Address + 0x68, value); }
        }

        public static int LanCutPoint
        {
            get { return RInt32(Address + 0x6C); }
            set { WInt32(Address + 0x6C, value); }
        }

        public static int LanCutPointTimer
        {
            get { return RInt32(Address + 0x70); }
            set { WInt32(Address + 0x70, value); }
        }

        public static int DeathState
        {
            get { return RInt32(Address + 0x78); }
            set { WInt32(Address + 0x78, value); }
        }
    }
}
