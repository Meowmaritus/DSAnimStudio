using System;
using DarkSoulsScripting.Injection;
using System.Collections.Generic;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class MapEntry : GameStruct
	{
        public const int MAX_NAME_LENGTH = 12;

        protected override void InitSubStructures()
        {

        }

        public string GetName()
        {
            return RUnicodeStr(RInt32(RInt32(Address + 0x60) + 4), MAX_NAME_LENGTH);
        }

        public Enemy FindEnemy(int modelID, int instanceNum)
        {
            string nameFormat = $"c{modelID:4}_{instanceNum:4}";
            foreach (var e in GetChrsAsEnemies())
            {
                if (e.GetName() == nameFormat)
                    return e;
            }
            return null;
        }
	
	public Enemy FindEnemyByMsbStr(string name)
        {
            foreach (var e in GetChrsAsEnemies())
            {
                if (e.GetName() == name)
                    return e;
            }
            return null;
        }

        public int PointerToBlockAndArea {
			get { return RInt32(Address + 0x4); }
		}

		public byte Block {
			get { return RByte(PointerToBlockAndArea + 0x6); }
		}

		public byte Area {
			get { return RByte(PointerToBlockAndArea + 0x7); }
		}

		public int ChrCount {
			get { return RInt32(Address + 0x3C); }
		}

        public int StartOfChrStruct {
            get { return RInt32(Address + 0x40); }
        }

        public List<ChrSlot> GetChrSlots()
		{
            List<ChrSlot> result = new List<ChrSlot>();

			for (int i = 0; i < ChrCount; i++) {
                var addr = StartOfChrStruct + (0x20 * i);
                result.Add(new ChrSlot() { AddressReadFunc = () => addr });
			}

			return result;
		}

		public List<Enemy> GetChrsAsEnemies()
		{
            List<Enemy> result = new List<Enemy>();

			for (int i = 0; i < ChrCount; i++) {
                var addr = RInt32(StartOfChrStruct + (0x20 * i));
                result.Add(new ChrSlot() { AddressReadFunc = () => addr }.GetChrAsEnemy());
			}

			return result;
		}

		public List<ChrTransform> GetChrTransforms()
		{
            List<ChrTransform> result = new List<ChrTransform>();

			for (int i = 0; i < ChrCount; i++) {
                var addr = StartOfChrStruct + (0x20 * i);
                result.Add(new ChrSlot() { AddressReadFunc = () => addr }.Transform);
			}

			return result;
		}
    }

}
