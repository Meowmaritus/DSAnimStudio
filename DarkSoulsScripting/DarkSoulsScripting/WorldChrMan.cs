using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class WorldChrMan
    {
        //Taken from the Dark Souls 1 Overhaul IDA Workspace.
        public const int CHR_STRUCT_SIZE = 0x5F8;

        public static int Address => RInt32(0x137DC70);

        public static Player LocalPlayer { get; private set; } = null;

        static WorldChrMan()
        {
            LocalPlayer = new Player() { AddressReadFunc = () => RInt32(ChrsBegin + 0x0) };
        }

        //TODO: SEE IF THESE ARE ALL ENEMIES OR WHAT.
        public static List<Enemy> GetEnemies()
        {
            var result = new List<Enemy>();
            for (int i = ChrsBegin; i <= ChrsEnd; i += 4)
            {
                int thisEnemyAddress = RInt32(i);
                result.Add(new Enemy() { AddressReadFunc = () => thisEnemyAddress });
            }
            return result;
        }

        public static EnemyPtrAccessor EnemyPtr = new EnemyPtrAccessor();

        public class EnemyPtrAccessor
        {
            public int this[int index]
            {
                get => RInt32(ChrsBegin + (index * 0x4));
                set => WInt32(ChrsBegin + (index * 0x4), value);
            }
        }

        public static int ChrsBegin
        {
            get => RInt32(Address + 0x4);
            set => WInt32(Address + 0x4, value);
        }

        public static int ChrsEnd
        {
            get => RInt32(Address + 0x8);
            set => WInt32(Address + 0x8, value);
        }
    }
}
