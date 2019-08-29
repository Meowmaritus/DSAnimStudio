using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaePlaybackCursor
    {
        public double CurrentTime = 0;

        private double previousFrameTime = 0;

        public double StartTime = 0;
        private double MaxTime = 1;
        public double Speed = 1;

        public double? HkxAnimationLength = null;

        const double FRAME30FPS = 0.0333333333333333;

        public int CurrentFrame30FPS => (int)Math.Round(CurrentTime / FRAME30FPS);
        public int MaxFrame30FPS => (int)Math.Round(MaxTime / FRAME30FPS);

        public bool IsRepeat = true;

        private bool _isPlaying = false;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                if (value != _isPlaying)
                {
                    CurrentTime = StartTime;
                    _isPlaying = value;
                }
            }
        }

        public bool Scrubbing = false;

        private bool isFirstFrameAfterLooping = false;

        public void Update(bool playPauseBtnDown, GameTime gameTime, IEnumerable<TaeEditAnimEventBox> eventBoxes)
        {
            bool prevPlayState = IsPlaying;

            if (CurrentTime < 0)
                CurrentTime = 0;

            if (playPauseBtnDown)
            {
                IsPlaying = !IsPlaying;
                if (!IsPlaying)
                {
                    foreach (var box in eventBoxes)
                    {
                        box.PlaybackHighlight = false;
                    }
                }
            }

            bool justStartedPlaying = !prevPlayState && IsPlaying;

            if (HkxAnimationLength.HasValue)
                MaxTime = HkxAnimationLength.Value;

            if (IsPlaying)
            {
                if (!Scrubbing)
                    CurrentTime += gameTime.ElapsedGameTime.TotalSeconds;

                bool justReachedAnimEnd = (CurrentTime >= MaxTime);

                if (!HkxAnimationLength.HasValue)
                    MaxTime = 0;

                foreach (var box in eventBoxes)
                {
                    if (!HkxAnimationLength.HasValue)
                    {
                        if (box.MyEvent.EndTime > MaxTime)
                            MaxTime = box.MyEvent.EndTime;
                    }

                    var currentlyInEvent = CurrentTime >= box.MyEvent.StartTime && CurrentTime <= box.MyEvent.EndTime;
                    var prevFrameInEvent = previousFrameTime >= box.MyEvent.StartTime && previousFrameTime <= box.MyEvent.EndTime;

                    if (currentlyInEvent)
                    {
                        //Also check if we looped playback
                        if (!prevFrameInEvent || isFirstFrameAfterLooping)
                        {
                            TaeInterop.PlaybackHitEventStart(box.MyEvent);
                        }

                        TaeInterop.PlaybackDuringEventSpan(box.MyEvent);

                        box.PlaybackHighlight = true;
                    }
                    else
                    {
                        box.PlaybackHighlight = false;
                    }

                    if (justReachedAnimEnd)
                    {
                        box.PlaybackHighlight = false;
                    }

                }

                if (justReachedAnimEnd && !Scrubbing)
                {
                    if (justStartedPlaying)
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
                            if (CurrentTime > MaxTime)
                                CurrentTime %= MaxTime;
                        }
                        else
                        {
                            CurrentTime = MaxTime;
                        }
                    }

                    

                    isFirstFrameAfterLooping = true;
                }  
                else
                {
                    isFirstFrameAfterLooping = false;
                }
            }

            previousFrameTime = CurrentTime;
        }
    }
}
