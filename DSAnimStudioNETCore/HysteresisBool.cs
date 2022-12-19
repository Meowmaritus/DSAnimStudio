using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class HysteresisBool
    {
        public HysteresisBool(int cyclesToEnable, int cyclesToDisable)
        {
            HysteresisCyclesToEnable = cyclesToEnable;
            HysteresisCyclesToDisable = cyclesToDisable;
        }

        private bool isFirstUpdate = true;
        public int HysteresisCyclesToEnable = 5;
        public int HysteresisCyclesToDisable = 5;
        private bool prevActualState;
        private int hystVal = 0;
        public bool State { get; private set; } = false;
        public void Update(bool liveState)
        {
            if (isFirstUpdate)
            {
                State = liveState;
                isFirstUpdate = false;
                return;
            }

            if (liveState != prevActualState)
            {
                hystVal = liveState ? HysteresisCyclesToEnable : HysteresisCyclesToDisable;
            }

            if (hystVal <= 0)
                State = liveState;
            else
                hystVal--;

            prevActualState = liveState;
            isFirstUpdate = false;
        }

        public static implicit operator bool(HysteresisBool v)
        {
            return v.State;
        }
    }
}
