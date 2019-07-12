using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class Pointers
    {
        public static int PtrToPtrToCharDataPtr1 => RInt32(0x137DC70);
        public static int PtrToCharDataPtr1 => RInt32(PtrToPtrToCharDataPtr1 + 0x4);
        public static int CharDataPtr1 => RInt32(PtrToCharDataPtr1);
        public static int GameStatsPtr => RInt32(0x1378700);
        public static int CharDataPtr2 => RInt32(GameStatsPtr + 0x8);
        public static int CharMapDataPtr => RInt32(CharDataPtr1 + 0x28);
        public static int MenuPtr => RInt32(0x13786D0);
        public static int LinePtr => RInt32(0x1378388);
        public static int KeyPtr => RInt32(0x1378640);
        public static int EntityControllerPtr => RInt32(CharMapDataPtr + 0x54);
        public static int EntityAnimPtr => RInt32(CharMapDataPtr + 0x14);
        public static int PtrToPtrToEntityAnimInstancePtr => RInt32(EntityAnimPtr + 0xC);
        public static int PtrToEntityAnimInstancePtr => RInt32(PtrToPtrToEntityAnimInstancePtr + 0x10);
        public static int EntityAnimInstancePtr => RInt32(PtrToEntityAnimInstancePtr);
    }
}