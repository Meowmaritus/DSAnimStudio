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
        public double CurrentTime;

        public double CurrentTimeMod => CurrentTime % MaxTime;

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

        public double GUICurrentTime => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate 
            ? (Math.Round(CurrentTime / (SnapInterval ?? SnapInterval_Default)) 
            * (SnapInterval ?? SnapInterval_Default)) : CurrentTime;

        public double GUICurrentTimeMod => GUICurrentTime % MaxTime;

        public double GUIStartTime => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate 
            ? (Math.Round(StartTime / (SnapInterval ?? SnapInterval_Default)) 
            * (SnapInterval ?? SnapInterval_Default)) : StartTime;

        public double OldGUICurrentTime { get; private set; } = 0;

        public double OldGUICurrentTimeMod => OldGUICurrentTime % MaxTime;

        public double OldGUICurrentFrame { get; private set; } = 0;

        public double OldGUICurrentFrameMod => OldGUICurrentFrame % MaxFrame;

        public double StartTime = 0;
        public double MaxTime { get; private set; } = 1;

        public double? HkxAnimationLength = null;

        public double? SnapInterval = null;

        public const double SnapInterval_Default = 0.0333333;

        public double CurrentSnapInterval => SnapInterval ?? SnapInterval_Default;

        public double GUICurrentFrame => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate 
            ? (Math.Round(CurrentTime / CurrentSnapInterval)) 
            :  (CurrentTime / CurrentSnapInterval);

        public double GUICurrentFrameMod => MaxFrame > 0 ? (GUICurrentFrame % MaxFrame) : 0;

        public double MaxFrame => Main.TAE_EDITOR.Config.LockFramerateToOriginalAnimFramerate 
            ? (Math.Round(MaxTime / CurrentSnapInterval)) 
            : (MaxTime / CurrentSnapInterval);

        public bool IsRepeat = true;
        public bool IsPlaying = false;

        public bool Scrubbing = false;
        public bool prevScrubbing = false;

        private int prevFrameLoopCount = 0;

        public float PlaybackSpeed = 1.0f;

        public bool IsStepping = false;

        public bool JustStartedPlaying = false;

        public void Update(bool playPauseBtnDown, bool shiftDown, IEnumerable<TaeEditAnimEventBox> eventBoxes)
        {
            bool prevPlayState = IsPlaying;

            //if (GUICurrentTime != oldGUICurrentTime)
            //{
            //    Scrubbing = true;
            //}

            if (CurrentTime < 0)
                CurrentTime = 0;

            int curFrameLoopCount = (int)(CurrentTime / MaxTime);

            if (Scrubbing)
            {
                IsStepping = false;
            }

            if (playPauseBtnDown)
            {
                IsStepping = false;

                IsPlaying = !IsPlaying;

                StartTime = CurrentTime;

                if (IsPlaying) // Just started playing
                {
                    if (shiftDown)
                    {
                        StartTime = 0;
                        CurrentTime = 0;
                    }
                }
                else // Just stoppped playing
                {
                    OnPlaybackEnded();
                }
            }

            JustStartedPlaying = !prevPlayState && IsPlaying;

            if (JustStartedPlaying || (curFrameLoopCount != prevFrameLoopCount))
                OnPlaybackStarted();

            if (HkxAnimationLength.HasValue)
                MaxTime = HkxAnimationLength.Value;

            if (IsPlaying || Scrubbing || IsStepping)
            {
                if (IsPlaying)
                {
                    CurrentTime += (Main.DELTA_UPDATE * PlaybackSpeed);
                }

                if (GUICurrentTime != OldGUICurrentTime)
                {
                    if (CurrentTime < 0)
                        CurrentTime += MaxTime;
                }

                bool justReachedAnimEnd = (CurrentTime >= MaxTime);

                // Single frame anim
                //if (MaxTime <= (SnapInterval))
                //{
                //    CurrentTime = 0;
                //}

                if (!HkxAnimationLength.HasValue)
                    MaxTime = 0;

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
                    //    int eventStartFrame = (int)Math.Round(box.MyEvent.StartTime / SnapIntervalHz);
                    //    int eventEndFrame = (int)Math.Round(box.MyEvent.EndTime / SnapIntervalHz);

                    //    currentlyInEvent = currentFrame >= eventStartFrame && currentFrame < eventEndFrame;
                    //    prevFrameInEvent = !justStartedPlaying && prevFrame >= eventStartFrame && prevFrame < eventEndFrame;
                    //}
                    //else
                    //{
                    //    currentlyInEvent = GUICurrentTime >= box.MyEvent.StartTime && GUICurrentTime < box.MyEvent.EndTime;
                    //    prevFrameInEvent = !justStartedPlaying && oldGUICurrentTime >= box.MyEvent.StartTime && oldGUICurrentTime < box.MyEvent.EndTime;
                    //}

                    if (box.PlaybackHighlight)
                    {
                        if (!box.PrevCyclePlaybackHighlight)
                        {
                            OnEventBoxEnter(box);
                        }

                        ////Also check if we looped playback
                        //if (!prevFrameInEvent || isFirstFrameAfterLooping)
                        //{

                        //}

                        OnEventBoxMidst(box);
                    }
                    else
                    {
                        if (box.PrevCyclePlaybackHighlight)
                        {
                            OnEventBoxExit(box);
                        }
                    }

                    

                    //if (IsPlaying && justReachedAnimEnd)
                    //{
                    //    box.PlaybackHighlight = false;
                    //}

                }

                if (GUICurrentTime != OldGUICurrentTime)
                {
                    if (Scrubbing)
                        OnScrubFrameChange();
                    else
                        OnPlaybackFrameChange();
                }

                if (IsPlaying && justReachedAnimEnd)
                {
                    if (JustStartedPlaying)
                    {
                        CurrentTime = 0;
                    }
                    else
                    {
                        if (IsRepeat && MaxTime > 0)
                        {
                            //while (CurrentTime >= MaxTime)
                            //{
                            //    CurrentTime -= MaxTime;
                            //    //previousFrameTime -= MaxTime;
                            //}

                            // way simpler
                            CurrentTime %= MaxTime;
                            OnPlaybackLooped();
                        }
                        else
                        {
                            CurrentTime = MaxTime;
                        }
                    }
                }

            }

            OldGUICurrentTime = GUICurrentTime;
            OldGUICurrentFrame = GUICurrentFrame;
            prevScrubbing = Scrubbing;
            prevFrameLoopCount = curFrameLoopCount;
        }
    }
}
