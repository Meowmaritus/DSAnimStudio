using DarkSoulsScripting.Injection;
using System.Collections.Generic;

namespace DarkSoulsScripting
{
    public static class Map
    {
        public static int Address => Hook.RInt32((0x137D644, 0));

        public static int PlayerPointer {
			get { return Hook.RInt32(Address + 0x3C); }
            set { Hook.WInt32(Address + 0x3C, value); }
		}

		public static int MapEntryCount {
			get { return Hook.RInt32(Address + 0x70); }
		}

		public static List<MapEntry> GetEntries()
		{
            List<MapEntry> result = new List<MapEntry>();
			for (int i = 0; i < MapEntryCount; i++) {
                var addr = Hook.RInt32(Address + 0x74 + (4 * i));
                result.Add(new MapEntry() { AddressReadFunc = () => addr });
			}
			return result;
		}

        public static MapEntry Find(int area, int block)
        {
            foreach (var e in GetEntries())
            {
                if (e.Area == (byte)area && e.Block == (byte)block)
                    return e;
            }

            return null;
        }

        //TODO: REPLACE FUNCTION CALLS WITH DIRECT READS
        public static MapEntry GetCurrent()
        {
            return Find(IngameFuncs.GetCurrentMapAreaNo(), IngameFuncs.GetCurrentMapBlockNo());
        }
	}

}
