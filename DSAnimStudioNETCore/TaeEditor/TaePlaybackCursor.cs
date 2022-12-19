using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{

    public class TaePlaybackCursor
    {
        private double _currentTimeVal = 0;
        public double CurrentTime
        {
            get => _currentTimeVal;
            set
            {
                if (double.IsNaN(value))
                {
                    _currentTimeVal = 0;
                    throw new InvalidOperationException("Current time was set to NaN.");
                }
                else if (double.IsInfinity(value))
                {
                    _currentTimeVal = 0;
                    throw new InvalidOperationException("Current time was set to infinity.");
                }
                else
                {
                    _currentTimeVal = value;
                }
                
            }
        }

        /// <summary>
        /// Makes it not stop IsPlaying state automatically once it reaches end of anim.
        /// </summary>
        public bool IsPlayingCombo = false;

        public void GotoFrame(float frame, bool ignoreTimeDelta)
        {
            CurrentTime = frame * CurrentSnapInterval;
            if (ignoreTimeDelta)
            {
                OldCurrentTime = CurrentTime;
            }
        }

        public string[] GetFrameCounterText(bool roundToNearestFrame)
        {
            return new[] { "Frame: " + (roundToNearestFrame ?
                    $"{((int)(GUICurrentFrameMod)),4:####}.000" :
                    $"{(MaxFrame <= 0 ? 0 : (Math.Truncate((GUICurrentFrameMod) * 1000) / 1000)),8:###0.000}") +
                    $" / {((int)((Math.Max(Math.Floor(LastMaxFrameGreaterThanZero), 0))))}", 
                    $"Time:  {(MaxFrame <= 0 ? 0 : (Math.Truncate((CurrentTimeMod) * 1000) / 1000)),8:###0.000} / {LastMaxTimeGreaterThanZero:0.000}" };
        }

        public int CurrentLoopCount { get; private set; } = 0;
        public int OldLoopCount { get; private set; } = 0;
        public int CurrentLoopCountDelta { get; private set; } = 0;

        public static double LastMaxTimeGreaterThanZero = 0;

        public double CurrentTimeMod => Main.Config.LoopEnabled ? (MaxTime > 0 ? CurrentTime % MaxTime : 0) : CurrentTime;

        public event EventHandler<TaeEditAnimEventBox> EventBoxEnter;
        private void OnEventBoxEnter(TaeEditAnimEventBox evBox) { EventBoxEnter?.Invoke(this, evBox); }

        public event EventHandler<TaeEditAnimEventBox> EventBoxMidst;
        private void OnEventBoxMidst(TaeEditAnimEventBox evBox) { EventBoxMidst?.Invoke(this, evBox); }

        public event EventHandler<TaeEditAnimEventBox> EventBoxExit;
        private void OnEventBoxExit(TaeEditAnimEventBox evBox) { EventBoxExit?.Invoke(this, evBox); }

        public event EventHandler PlaybackStarted;
        private void OnPlaybackStarted() { PlaybackStarted?.Invoke(this, EventArgs.Empty); }

        public event EventHandler PlaybackLooped;
        private void OnPlaybackLooped() { PlaybackLooped?.Invoke(this, EventArgs.Empty); }

        public event EventHandler PlaybackEnded;
        private void OnPlaybackEnded() { PlaybackEnded?.Invoke(this, EventArgs.Empty); }

        public event EventHandler PlaybackFrameChange;
        private void OnPlaybackFrameChange() { PlaybackFrameChange?.Invoke(this, EventArgs.Empty); }

        public event EventHandler ScrubFrameChange;
        private void OnScrubFrameChange() { ScrubFrameChange?.Invoke(this, EventArgs.Empty); }

        public void ClearRemoState()
        {
            IsPlayingRemoFullPreview = false;
        }

        public void SetRemoState()
        {
            IsPlayingRemoFullPreview = true;
        }

        public void ForceScrub(float hkxDeltaTime, float taeDeltaTime)
        {
            HKXDeltaTime = hkxDeltaTime;
            ContinuousTimeDelta = taeDeltaTime;

            if (!IsPlaying)
                OnScrubFrameChange();
            else
                OnPlaybackFrameChange();
        }

        public void UpdateScrubbing()
        {

            HKXDeltaTime = GUICurrentTime - OldGUICurrentTime;
            ContinuousTimeDelta = CurrentTime - OldCurrentTime;

            //if (GUICurrentTime != OldGUICurrentTime)
            //{


            //    if (Scrubbing)
            //        OnScrubFrameChange();
            //    else
            //        OnPlaybackFrameChange();
            //}

            if (!Main.Config.LoopEnabled && CurrentTime > MaxTime)
                CurrentTime = MaxTime;

            if (!IsPlaying)
                OnScrubFrameChange();
            else
                OnPlaybackFrameChange();

            OldGUICurrentTime = GUICurrentTime;
            OldCurrentTime = CurrentTime;
            OldGUICurrentFrame = GUICurrentFrame;
        }

        public double GUICurrentTime => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate 
            ? (Math.Floor(CurrentTime / (SnapInterval ?? SnapInterval_Default)) 
            * (SnapInterval ?? SnapInterval_Default)) : CurrentTime;

        //public double HitWindowStart => CurrentTime - 0.001;
        //public double HitWindowEnd => CurrentTime + 0.001;

        //public double OldHitWindowStart => OldCurrentTime;
        //public double OldHitWindowEnd => OldCurrentTime + (CurrentSnapInterval / 2.0);

        public double GUICurrentTimeMod => Main.Config.LoopEnabled ? ModuloByMaxTime(GUICurrentTime) : GUICurrentTime;

        public double GUIStartTime => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate 
            ? (Math.Floor(StartTime / CurrentSnapInterval) 
            * CurrentSnapInterval) : StartTime;

        public double OldGUICurrentTime { get; private set; } = 0;

        public double HKXDeltaTime = 0;

        public double OldCurrentTime { get; private set; } = 0;
        public double OldCurrentTimeMod => Main.Config.LoopEnabled ? ModuloByMaxTime(OldCurrentTime) : OldCurrentTime;

        public double OldCurrentTimeModWrapped
        {
            get
            {
                var result = OldCurrentTimeMod;

                // Hit end and wrapped back to start
                if (CurrentTime > OldCurrentTime && CurrentTimeMod < OldCurrentTimeMod)
                    result -= MaxTime;
                // Hit start and wrapped back to end
                else if (CurrentTime < OldCurrentTime && CurrentTimeMod > OldCurrentTimeMod)
                    result += MaxTime;

                return result;
            }
        }

        public double OldGUICurrentTimeMod => Main.Config.LoopEnabled ? ModuloByMaxTime(OldGUICurrentTime) : OldGUICurrentTime;

        public double OldGUICurrentFrame { get; private set; } = 0;

        public double OldGUICurrentFrameMod => Main.Config.LoopEnabled ? ModuloByMaxFrame(OldGUICurrentFrame) : OldGUICurrentFrame;

        public double StartTime = 0;
        public double MaxTime { get; private set; } = 1;

        public double ModuloByMaxTime(double inputValue)
        {
            return MaxTime > 0 ? (inputValue % MaxTime) : inputValue;
        }

        public double ModuloByMaxFrame(double inputValue)
        {
            return MaxFrame > 0 ? (inputValue % MaxFrame) : inputValue;
        }

        public double? HkxAnimationLength = null;
        public double? HkxAnimationFrameLength = null;

        private double? __snapInterval = null;
        public double? SnapInterval
        {
            get => __snapInterval;
            set
            {
                if (value.HasValue && (value.Value <= 0 || double.IsNaN(value.Value) || double.IsInfinity(value.Value)))
                {
                    __snapInterval = null;
                }
                else
                {
                    __snapInterval = value;
                }
            }
        }

        public const double SnapInterval_Default = 1.0 / 30.0;

        public double CurrentSnapInterval => SnapInterval ?? SnapInterval_Default;

        public double CurrentSnapFPS => 1 / CurrentSnapInterval;

        public double GUICurrentFrame => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate
            ? (Math.Floor(CurrentTime / CurrentSnapInterval))
            : (CurrentTime / CurrentSnapInterval);

        public double CurrentFrame => (CurrentTime / CurrentSnapInterval);
        public double OldCurrentFrame => (OldCurrentTime / CurrentSnapInterval);

        public double GUICurrentFrameMod => Main.Config.LoopEnabled ? ModuloByMaxFrame(GUICurrentFrame) : GUICurrentFrame;

        public double MaxFrame => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate
            ? (Math.Floor(MaxTime / CurrentSnapInterval))
            : (MaxTime / CurrentSnapInterval);

        public static double LastMaxFrameGreaterThanZero;

        public bool IsRepeat = true;
        public bool IsPlaying = false;
        public bool IsPlayingRemoFullPreview = false;
        public bool OldIsPlaying { get; private set; } = false;

        public bool Scrubbing = false;
        public bool prevScrubbing = false;

        public static float GlobalBasePlaybackSpeed = 1.0f;
        public float BasePlaybackSpeed => GlobalBasePlaybackSpeed;
        public float ModPlaybackSpeed_Event603 = 1.0f;
        public float ModPlaybackSpeed_Event608 = 1.0f;

        public float EffectivePlaybackSpeed => BasePlaybackSpeed * ModPlaybackSpeed_Event603 * ModPlaybackSpeed_Event608;

        public bool IsStepping = false;

        public bool JustStartedPlaying = false;

        public double ContinuousTimeDelta;

        public bool JustLooped => CurrentLoopCountDelta != 0;

        public void RestartFromBeginning()
        {
            CurrentTime = 0;
            StartTime = 0;
            OldIsPlaying = false;
            if (IsPlaying)
            {
                JustStartedPlaying = true;
            }
            //JustStartedPlaying = true;
            IgnoreCurrentRelativeScrub();
        }

        public void IgnoreCurrentRelativeScrub()
        {
            OldGUICurrentTime = GUICurrentTime;
            OldGUICurrentFrame = GUICurrentFrame;
            OldCurrentTime = CurrentTime;
            OldLoopCount = CurrentLoopCount;
            CurrentLoopCountDelta = 0;
        }

        public void ResetAll()
        {
            CurrentLoopCount = 0;
            CurrentLoopCountDelta = 0;
            CurrentTime = 0;
            //IsPlaying = false;
            //IsRepeat = true;
            //IsStepping = false;
            JustStartedPlaying = false;
            OldIsPlaying = false;
            MaxTime = 0;
            OldGUICurrentFrame = 0;
            OldGUICurrentTime = 0;
            OldLoopCount = 0;
            //prevScrubbing = false;
            //Scrubbing = false;
            StartTime = 0;

            //PlaybackSpeed = 1.0f;
        }

        public void Transport_PlayPause(bool isUserInput = true)
        {
            IsStepping = false;

            // Reset to start if end is reached while loop is off.
            if (isUserInput && !Main.Config.LoopEnabled && !IsPlaying && CurrentTime >= MaxTime)
            {
                CurrentTime = StartTime;
                UpdateScrubbing();
            }

            IsPlaying = !IsPlaying;

            if (IsPlayingRemoFullPreview)
            {
                if (IsPlaying)
                    RemoManager.ResumeStreamedBGM();
                else
                    RemoManager.PauseStreamBGM();
            }

            //StartTime = CurrentTime;

            if (!IsPlaying)
            {
                OnPlaybackEnded();
            }
        }

        public void Update(IEnumerable<TaeEditAnimEventBox> eventBoxes, bool ignoreDeltaTime = false)
        {
            if (double.IsNaN(CurrentTime))
                CurrentTime = 0;
            //bool prevPlayState = IsPlaying;

            //if (GUICurrentTime != oldGUICurrentTime)
            //{
            //    Scrubbing = true;
            //}

            //if (CurrentTime < 0)
            //    CurrentTime = 0;

            if (Scrubbing)
            {
                IsStepping = false;
            }

            if (Scrubbing || JustStartedPlaying)
            {
                CurrentLoopCount = MaxTime >= 0 ? (int)(CurrentTime / (MaxTime)) : 0;
            }

            if (JustStartedPlaying || (CurrentLoopCount != OldLoopCount))
                OnPlaybackStarted();

            if (HkxAnimationLength.HasValue)
                MaxTime = HkxAnimationLength.Value;

            if (IsPlaying || Scrubbing || IsStepping)
            {
               

                if (IsPlaying)
                {
                    if (!ignoreDeltaTime)
                        CurrentTime += (Main.DELTA_UPDATE * BasePlaybackSpeed * ModPlaybackSpeed_Event603 * ModPlaybackSpeed_Event608);
                    if (!Main.Config.LoopEnabled && CurrentTime > MaxTime && !IsPlayingCombo)
                    {
                        CurrentTime = MaxTime;
                        IsPlaying = false;
                    }
                }

                if (!HkxAnimationLength.HasValue)
                    MaxTime = 0;

                try
                {
                    // Thread locked from outside this function
                    foreach (var box in eventBoxes)
                    {
                        if (!HkxAnimationLength.HasValue)
                        {
                            if (box.MyEvent.EndTime > MaxTime)
                                MaxTime = box.MyEvent.EndTime;
                        }

                        //bool currentlyInEvent = false;
                        //bool prevFrameInEvent = false;

                        //if (Main.TAE_EDITOR.Config.EnableSnapTo30FPSIncrements)
                        //{
                        //    int currentFrame = (int)Math.Floor((GUICurrentTime % MaxTime) / SnapIntervalHz);
                        //    int prevFrame = (int)Math.Floor((oldGUICurrentTime % MaxTime) / SnapIntervalHz);
                        //    int eventStartFrame = (int)Math.Floor(box.MyEvent.StartTime / SnapIntervalHz);
                        //    int eventEndFrame = (int)Math.Floor(box.MyEvent.EndTime / SnapIntervalHz);

                        //    currentlyInEvent = currentFrame >= eventStartFrame && currentFrame < eventEndFrame;
                        //    prevFrameInEvent = !justStartedPlaying && prevFrame >= eventStartFrame && prevFrame < eventEndFrame;
                        //}
                        //else
                        //{
                        //    currentlyInEvent = GUICurrentTime >= box.MyEvent.StartTime && GUICurrentTime < box.MyEvent.EndTime;
                        //    prevFrameInEvent = !justStartedPlaying && oldGUICurrentTime >= box.MyEvent.StartTime && oldGUICurrentTime < box.MyEvent.EndTime;
                        //}

                        if (box.WasJustEnteredDuringPlayback)
                        {
                            OnEventBoxEnter(box);


                            ////Also check if we looped playback
                            //if (!prevFrameInEvent || isFirstFrameAfterLooping)
                            //{

                            //}


                        }
                        else if (box.PlaybackHighlight)
                        {
                            OnEventBoxMidst(box);
                        }
                        else if (box.PrevCyclePlaybackHighlight)
                        {
                            OnEventBoxExit(box);
                        }



                        //if (IsPlaying && justReachedAnimEnd)
                        //{
                        //    box.PlaybackHighlight = false;
                        //}

                    }
                }
                catch
                {

                }
                if (IsPlaying)
                {


                    if (JustStartedPlaying)
                    {
                        //CurrentTime = 0;
                    }
                    else if (MaxTime <= 0)
                    {
                        CurrentTime = MaxTime;
                        CurrentLoopCount = 0;
                    }
                    else
                    {
                        //if (IsRepeat)
                        //{
                        //    while (CurrentTime >= MaxTime)
                        //    {
                        //        CurrentTime -= MaxTime;
                        //        CurrentLoopCount++;
                        //        //previousFrameTime -= MaxTime;
                        //        OnPlaybackLooped();
                        //    }

                        //    // way simpler
                        //    //CurrentTime %= MaxTime;



                        //}
                    }


                }

                

               

            }

            if (MaxTime > 0)
            {
                LastMaxTimeGreaterThanZero = MaxTime;
            }

            if (MaxFrame > 0)
            {
                LastMaxFrameGreaterThanZero = MaxFrame;
            }

            
            prevScrubbing = Scrubbing;

            if (CurrentTime < 0 && CurrentLoopCount == 0)
                CurrentLoopCount--;

            CurrentLoopCountDelta = CurrentLoopCount - OldLoopCount;

            if (!ignoreDeltaTime)
                UpdateScrubbing();

            OldLoopCount = CurrentLoopCount;

            JustStartedPlaying = false;

            OldIsPlaying = IsPlaying;
        }
    }
}
