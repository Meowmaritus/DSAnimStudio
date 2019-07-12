using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DarkSoulsScripting.Hook;

namespace DarkSoulsScripting
{
    public class Enemy : Chr<EnemyMovementCtrl, EnemyController>
    {
        protected override void InitSubStructures()
        {
            base.InitSubStructures();

        }

        public static Enemy FromPtr(int ptr)
        {
            return new Enemy() { AddressReadFunc = () => ptr };
        }
    }
}
