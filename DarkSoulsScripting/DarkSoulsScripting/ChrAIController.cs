using System;
using DarkSoulsScripting.Injection;

namespace DarkSoulsScripting
{
    public class ChrAIController<TChr, TChrMovementCtrl, TChrController> : GameStruct
        where TChrController : ChrController, new()
        where TChrMovementCtrl : ChrMovementCtrl<TChrController>, new()
        where TChr : Chr<TChrMovementCtrl, TChrController>, new()
	{
        protected override void InitSubStructures()
        {
            
        }

        public int ChrPtr {
			get { return Hook.RInt32(Address + 0x14); }
			set { Hook.WInt32(Address + 0x14, value); }
		}

        public TChr GetChr() => new TChr() { AddressReadFunc = () => ChrPtr };

		public int AIScript {
			get { return Hook.RInt32(Address + 0x78); }
			set { Hook.WInt32(Address + 0x78, value); }
		}

		public int AnimationID {
			get { return Hook.RInt32(Address + 0x9c); }
			set { Hook.WInt32(Address + 0x9c, value); }
		}

		public int AIScript2 {
			get { return Hook.RInt32(Address + 0x80); }
			set { Hook.WInt32(Address + 0x80, value); }
		}

		public float PosX {
			get { return Hook.RFloat(Address + 0x1e0); }
			set { Hook.WFloat(Address + 0x1e0, value); }
		}

		public float PosY {
			get { return Hook.RFloat(Address + 0x1e4); }
			set { Hook.WFloat(Address + 0x1e4, value); }
		}

		public float PosZ {
			get { return Hook.RFloat(Address + 0x1e8); }
			set { Hook.WFloat(Address + 0x1e8, value); }
		}

		public float RotZ {
			get { return Hook.RFloat(Address + 0x1e8); }
			set { Hook.WFloat(Address + 0x1e8, value); }
		}

		public float RotY {
			get { return Hook.RFloat(Address + 0x1f0); }
			set { Hook.WFloat(Address + 0x1f0, value); }
		}

		public float RotX {
			get { return Hook.RFloat(Address + 0x1f4); }
			set { Hook.WFloat(Address + 0x1f4, value); }
		}

		public int AnimationID2 {
			get { return Hook.RInt32(Address + 0x208); }
			set { Hook.WInt32(Address + 0x208, value); }
		}
    }

}
