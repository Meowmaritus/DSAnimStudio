using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class GameTendency : GameStruct
    {
        protected override void InitSubStructures()
        {
            
        }

        public float CharacterBlackWhiteTendency
        {
            get { return RFloat(Address + 0x8); }
            set { WFloat(Address + 0x8, value); }
        }

        public float CharacterLeftRightTendency
        {
            get { return RFloat(Address + 0xC); }
            set { WFloat(Address + 0xC, value); }
        }
    }
}
