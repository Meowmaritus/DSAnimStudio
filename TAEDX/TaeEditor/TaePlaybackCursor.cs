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
        public event EventHandler CurrentTimeChanged;
        protected void OnCurrentTimeChanged()
        {
            CurrentTimeChanged?.Invoke(this, EventArgs.Empty);
        }

        private double _currentTime;
        public double CurrentTime
        {
            get => _currentTime;
            set
            {
                if (value != _currentTime)
                {
                    _currentTime = value;
                    OnCurrentTimeChanged();
                }
            }
        }

        public double GUICurrentTime => TaeInterop.IsSnapTo30FPS ? (Math.Round(CurrentTime / FRAME30FPS) * FRAME30FPS) : CurrentTime;
        public double GUIStartTime => TaeInterop.IsSnapTo30FPS ? (Math.Round(StartTime / FRAME30FPS) * FRAME30FPS) : StartTime;

        private double oldGUICurrentTime = 0;

        public double StartTime = 0;
        public double MaxTime { get; private set; } = 1;
        public double Speed = 1;

        public double? HkxAnimationLength = null;

        public const double FRAME30FPS = 1.0 / 30.0;

        public double GUICurrentFrame => TaeInterop.IsSnapTo30FPS ? (Math.Round(CurrentTime / FRAME30FPS)) :  (CurrentTime / FRAME30FPS);
        public double MaxFrame => TaeInterop.IsSnapTo30FPS ? (Math.Round(MaxTime / FRAME30FPS)) : (MaxTime / FRAME30FPS);

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
        public bool prevScrubbing = false;

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

            if (prevScrubbing && !Scrubbing)
            {
                foreach (var box in eventBoxes)
                {
                    box.PlaybackHighlight = false;
                }
            }

            bool justStartedPlaying = !prevPlayState && IsPlaying;

            if (HkxAnimationLength.HasValue)
                MaxTime = HkxAnimationLength.Value;

            if (IsPlaying || Scrubbing)
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

                    bool currentlyInEvent = false;
                    bool prevFrameInEvent = false;

                    if (TaeInterop.IsSnapTo30FPS)
                    {
                        int currentFrame = (int)Math.Round(GUICurrentTime / FRAME30FPS);
                        int prevFrame = (int)Math.Round(oldGUICurrentTime / FRAME30FPS);
                        int eventStartFrame = (int)Math.Round(box.MyEvent.StartTime / FRAME30FPS);
                        int eventEndFrame = (int)Math.Round(box.MyEvent.EndTime / FRAME30FPS);

                        currentlyInEvent = currentFrame >= eventStartFrame && currentFrame < eventEndFrame;
                        prevFrameInEvent = !justStartedPlaying && prevFrame >= eventStartFrame && prevFrame < eventEndFrame;
                    }
                    else
                    {
                        currentlyInEvent = GUICurrentTime >= (TaeInterop.IsSnapTo30FPS ? box.MyEvent.GetStartTimeFr() : box.MyEvent.StartTime) && GUICurrentTime < box.MyEvent.EndTime;
                        prevFrameInEvent = !justStartedPlaying && oldGUICurrentTime >= (TaeInterop.IsSnapTo30FPS ? box.MyEvent.GetEndTimeFr() : box.MyEvent.StartTime) && oldGUICurrentTime < box.MyEvent.EndTime;
                    }

                    if (currentlyInEvent)
                    {
                        //Also check if we looped playback
                        if (!prevFrameInEvent || isFirstFrameAfterLooping)
                        {
                            TaeInterop.PlaybackHitEventStart(box);
                        }

                        TaeInterop.PlaybackDuringEventSpan(box);

                        box.PlaybackHighlight = true;
                    }
                    else
                    {
                        box.PlaybackHighlight = false;
                    }

                    if (IsPlaying && justReachedAnimEnd)
                    {
                        box.PlaybackHighlight = false;
                    }

                }

                if (justReachedAnimEnd)
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

            oldGUICurrentTime = GUICurrentTime;
            prevScrubbing = Scrubbing;
        }
    }
}
