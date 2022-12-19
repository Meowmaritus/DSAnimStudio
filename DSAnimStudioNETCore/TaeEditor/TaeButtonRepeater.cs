namespace DSAnimStudio.TaeEditor
{
    public enum TaeButtonRepeatState
    {
        ButtonNotHeld,
        ButtonHeldInitialWait,
        ButtonRepeating
    }

    public class TaeButtonRepeater
    {
        public readonly float InitialRepeatDelay;
        public readonly float RepeatInterval;

        public bool State = false;

        private bool prevHeldState = false;

        public bool IsInitalButtonTap { get; private set; } = false;

        private TaeButtonRepeatState RepeatState = TaeButtonRepeatState.ButtonNotHeld;
        private float timer = 0;


        public TaeButtonRepeater(float initialRepeatDelay, float repeatInterval)
        {
            InitialRepeatDelay = initialRepeatDelay;
            RepeatInterval = repeatInterval;
        }

        public bool Update(float elapsedSeconds, bool curHeldState)
        {
            if (!curHeldState)
            {
                RepeatState = TaeButtonRepeatState.ButtonNotHeld;
                timer = InitialRepeatDelay;
                State = false;
                IsInitalButtonTap = false;
            }
            else
            {
                if (RepeatState == TaeButtonRepeatState.ButtonNotHeld)
                {
                    //First frame of pressing button.
                    if (!prevHeldState && curHeldState)
                    {
                        State = true;
                        IsInitalButtonTap = true;
                        //Start counting down the initial wait timer.
                        RepeatState = TaeButtonRepeatState.ButtonHeldInitialWait;
                        timer = InitialRepeatDelay;
                    }
                }
                else if (RepeatState == TaeButtonRepeatState.ButtonHeldInitialWait)
                {
                    State = false;
                    IsInitalButtonTap = false;
                    timer -= elapsedSeconds;
                    if (timer <= 0)
                    {
                        State = true;
                        RepeatState = TaeButtonRepeatState.ButtonRepeating;
                        timer = RepeatInterval;
                    }
                }
                else if (RepeatState == TaeButtonRepeatState.ButtonRepeating)
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
