using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarkSoulsScripting
{
    public class EnemyController : ChrController
    {
        public EnemyAIController AIController { get; private set; } = null;

        protected override void InitSubStructures()
        {
            base.InitSubStructures();

            AIController = new EnemyAIController() { AddressReadFunc = () => AIControllerPtr };
        }

        public int AIControllerPtr {
			get { return Hook.RInt32(Address + 0x230); }
			set { Hook.WInt32(Address + 0x230, value); }
		}

        public int AnimationID {
			get { return Hook.RInt32(Address + 0x1E8); }
			set { Hook.WInt32(Address + 0x1E8, value); }
		}
    }
}
