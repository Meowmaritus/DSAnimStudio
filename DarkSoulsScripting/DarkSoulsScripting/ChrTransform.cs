using System;
using DarkSoulsScripting.Injection;

namespace DarkSoulsScripting
{
    public class ChrTransform : GameStruct
	{
        protected override void InitSubStructures()
        {

        }

        public float Heading {
			get { return (float)((Hook.RFloat(Address + 0x4) / Math.PI * 180) + 180); }
			set { Hook.WFloat(Address + 0x4, (float)(value * Math.PI / 180) - (float)Math.PI); }
		}

		public float Angle {
			get { return Hook.RFloat(Address + 0x4); }
			set { Hook.WFloat(Address + 0x4, value); }
		}

		public float X {
			get { return Hook.RFloat(Address + 0x10); }
			set { Hook.WFloat(Address + 0x10, value); }
		}

		public float Y {
			get { return Hook.RFloat(Address + 0x14); }
			set { Hook.WFloat(Address + 0x14, value); }
		}

		public float Z {
			get { return Hook.RFloat(Address + 0x18); }
			set { Hook.WFloat(Address + 0x18, value); }
		}

	}

}
