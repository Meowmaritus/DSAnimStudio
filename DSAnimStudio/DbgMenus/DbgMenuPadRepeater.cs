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
        public static List<DbgMenuPadRepeater> ALL_INSTANCES = new List<DbgMenuPadRepeater>();

        public readonly Buttons Button;
        public readonly float InitialRepeatDelay;
        public readonly float RepeatInterval;

        public bool State = false;

        private bool prevHeldState = false;

        public bool IsInitalButtonTap { get; private set; } = false;

        private DbgMenuPadRepeatState RepeatState = DbgMenuPadRepeatState.ButtonNotHeld;
        private float timer = 0;

        private bool alternateKeyboardInput = false;
        private bool curHeldState = false;

        private long DEBUG_UPDATE_COUNT = 0;

        public DbgMenuPadRepeater(Buttons b, float initialRepeatDelay, float repeatInterval)
        {
            Button = b;
            InitialRepeatDelay = initialRepeatDelay;
            RepeatInterval = repeatInterval;
            if (!ALL_INSTANCES.Contains(this))
                ALL_INSTANCES.Add(this);
        }

        public override string ToString()
        {
            return $"DbgMenuPadRepeater[{DEBUG_UPDATE_COUNT:D10} | State:{(State ? 1 : 0)} KB:{(alternateKeyboardInput ? 1 : 0)} CurHeld:{(curHeldState ? 1 : 0)} IsInit:{(IsInitalButtonTap ? 1 : 0)} Timer:{timer:0.0000} RepeatState:{RepeatState}]";
        }

        public bool Update(GamePadState gamepad, float elapsedSeconds, bool alternateKeyboardInput)
        {
            this.alternateKeyboardInput = alternateKeyboardInput;
            curHeldState = gamepad.IsButtonDown(Button) || (Main.Active && alternateKeyboardInput);
            
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

            DEBUG_UPDATE_COUNT++;

            return State;
        }
    }
}
