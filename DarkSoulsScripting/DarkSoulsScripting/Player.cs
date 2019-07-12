using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsScripting
{
    public class Player : Chr<PlayerMovementCtrl, PlayerController>
    {
        public PlayerStats Stats { get; private set; } = null;

        protected override void InitSubStructures()
        {
            base.InitSubStructures();

            Stats = new PlayerStats() { AddressReadFunc = () => StatsPtr };
        }
    }
}
