using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.DbgMenus
{
    public enum DbgMenuPadRepeatState
    {
        ButtonNotHeld,
        ButtonHeldInitialWait,
        ButtonRepeating
    }

    public class DbgMenuPadRepeater
    {
        public readonly Buttons Button;
        public readonly float InitialRepeatDelay;
        public readonly float RepeatInterval;

        public bool State = false;

        private bool prevHeldState = false;

        public bool IsInitalButtonTap { get; private set; } = false;

        private DbgMenuPadRepeatState RepeatState = DbgMenuPadRepeatState.ButtonNotHeld;
        private float timer = 0;


        public DbgMenuPadRepeater(Buttons b, float initialRepeatDelay, float repeatInterval)
        {
            Button = b;
            InitialRepeatDelay = initialRepeatDelay;
            RepeatInterval = repeatInterval;
        }

        public bool Update(GamePadState gamepad, float elapsedSeconds, bool alternateKeyboardInput)
        {
            bool curHeldState = gamepad.IsButtonDown(Button) || (Main.Active && alternateKeyboardInput);
            
            if (!curHeldState)
            {
                RepeatState = DbgMenuPadRepeatState.ButtonNotHeld;
                timer = InitialRepeatDelay;
                State = false;
                IsInitalButtonTap = false;
            }
            else
            {
                if (RepeatState == DbgMenuPadRepeatState.ButtonNotHeld)
                {
                    //First frame of pressing button.
                    if (!prevHeldState && curHeldState)
                    {
                        State = true;
                        IsInitalButtonTap = true;
                        //Start counting down the initial wait timer.
                        RepeatState = DbgMenuPadRepeatState.ButtonHeldInitialWait;
                        timer = InitialRepeatDelay;
                    }
                }
                else if (RepeatState == DbgMenuPadRepeatState.ButtonHeldInitialWait)
                {
                    State = false;
                    IsInitalButtonTap = false;
                    timer -= elapsedSeconds;
                    if (timer <= 0)
                    {
                        State = true;
                        RepeatState = DbgMenuPadRepeatState.ButtonRepeating;
                        timer = RepeatInterval;
                    }
                }
                else if (RepeatState == DbgMenuPadRepeatState.ButtonRepeating)
                {
                    State = false;
                    IsInitalButtonTap = false;
                    timer -= elapsedSeconds;
                    if (timer <= 0)
                    {
                        State = true;
                        timer = RepeatInterval;
                    }
                }
            }

            prevHeldState = curHeldState;


            return State;
        }
    }
}
