using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class ChrUNK1 : GameStruct
    {
        protected override void InitSubStructures()
        {
            
        }

        public int LastHitMask {
			get { return RInt32(Address + 0x2C); }
			set { WInt32(Address + 0x2C, value); }
		}
    }
}
