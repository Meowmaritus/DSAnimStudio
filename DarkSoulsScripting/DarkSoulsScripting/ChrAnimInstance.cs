using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class ChrAnimInstance : GameStruct
    {
        protected override void InitSubStructures()
        {

        }

        public float ElapsedTime
        {
            get { return RFloat(Address + 0x8); }
            set { WFloat(Address + 0x8, value); }
        }

        public float Weight
        {
            get { return RFloat(Address + 0x3C); }
            set { WFloat(Address + 0x3C, value); }
        }

        public float Speed
        {
            get { return RFloat(Address + 0x40); }
            set { WFloat(Address + 0x40, value); }
        }

        public int LoopCount
        {
            get { return RInt32(Address + 0x44); }
            set { WInt32(Address + 0x44, value); }
        }
    }
}
