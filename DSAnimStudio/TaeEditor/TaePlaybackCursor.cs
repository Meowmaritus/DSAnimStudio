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

        public double GUICurrentTime => TaeInterop.IsSnapTo30FPS ? (Math.Round(CurrentTime / SnapInterval) * SnapInterval) : CurrentTime;
        public double GUIStartTime => TaeInterop.IsSnapTo30FPS ? (Math.Round(StartTime / SnapInterval) * SnapInterval) : StartTime;

        private double oldGUICurrentTime = 0;

        public double StartTime = 0;
        public double MaxTime { get; private set; } = 1;
        public double Speed = 1;

        public double? HkxAnimationLength = null;

        public static double SnapInterval => TaeInterop.CurrentAnimationFrameDuration;

        public double GUICurrentFrame => TaeInterop.IsSnapTo30FPS ? (Math.Round(CurrentTime / SnapInterval)) :  (CurrentTime / SnapInterval);
        public double MaxFrame => TaeInterop.IsSnapTo30FPS ? (Math.Round(MaxTime / SnapInterval)) : (MaxTime / SnapInterval);

        public bool IsRepeat = true;
        public bool IsPlaying = false;

        public bool Scrubbing = false;
        public bool prevScrubbing = false;

        private bool isFirstFrameAfterLooping = false;

        public void Update(bool playPauseBtnDown, bool shiftDown, GameTime gameTime, IEnumerable<TaeEditAnimEventBox> eventBoxes)
        {
            bool prevPlayState = IsPlaying;

            if (CurrentTime < 0)
                CurrentTime = 0;

            TaeInterop.OnAnimFrameChange(Scrubbing);


            if (playPauseBtnDown)
            {
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
                    foreach (var box in eventBoxes)
                        box.PlaybackHighlight = false;
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

            if (justStartedPlaying || isFirstFrameAfterLooping)
                TaeInterop.PlaybackJustStarted();

            if (HkxAnimationLength.HasValue)
                MaxTime = HkxAnimationLength.Value;

            if (IsPlaying || Scrubbing)
            {
                if (!Scrubbing)
                    CurrentTime += gameTime.ElapsedGameTime.TotalSeconds;

                bool justReachedAnimEnd = (CurrentTime >= MaxTime);

                if (Math.Round(MaxTime / TaeInterop.CurrentAnimationFrameDuration) == 1)
                {
                    CurrentTime = 0;
                }

                if (!HkxAnimationLength.HasValue)
                    MaxTime = 0;

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
                        int currentFrame = (int)Math.Round(GUICurrentTime / SnapInterval);
                        int prevFrame = (int)Math.Round(oldGUICurrentTime / SnapInterval);
                        int eventStartFrame = (int)Math.Round(box.MyEvent.StartTime / SnapInterval);
                        int eventEndFrame = (int)Math.Round(box.MyEvent.EndTime / SnapInterval);

                        currentlyInEvent = currentFrame >= eventStartFrame && currentFrame < eventEndFrame;
                        prevFrameInEvent = !justStartedPlaying && prevFrame >= eventStartFrame && prevFrame < eventEndFrame;
                    }
                    else
                    {
                        //currentlyInEvent = GUICurrentTime >= box.MyEvent.StartTime && GUICurrentTime < box.MyEvent.EndTime;
                        //prevFrameInEvent = !justStartedPlaying && oldGUICurrentTime >= box.MyEvent.StartTime && oldGUICurrentTime < box.MyEvent.EndTime;

                        int currentFrame = (int)Math.Floor(GUICurrentTime / SnapInterval);
                        int prevFrame = (int)Math.Floor(oldGUICurrentTime / SnapInterval);
                        int eventStartFrame = (int)Math.Round(box.MyEvent.StartTime / SnapInterval);
                        int eventEndFrame = (int)Math.Round(box.MyEvent.EndTime / SnapInterval);

                        currentlyInEvent = currentFrame >= eventStartFrame && currentFrame < eventEndFrame;
                        prevFrameInEvent = !justStartedPlaying && prevFrame >= eventStartFrame && prevFrame < eventEndFrame;
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

                    //if (IsPlaying && justReachedAnimEnd)
                    //{
                    //    box.PlaybackHighlight = false;
                    //}

                }

                
            }

            oldGUICurrentTime = GUICurrentTime;
            prevScrubbing = Scrubbing;
        }
    }
}
