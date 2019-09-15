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

        public double? HkxAnimationLength = null;

        public static double SnapInterval => TaeInterop.CurrentAnimationFrameDuration > 0 ? TaeInterop.CurrentAnimationFrameDuration : 30;

        public double GUICurrentFrame => TaeInterop.IsSnapTo30FPS ? (Math.Round(CurrentTime / SnapInterval)) :  (CurrentTime / SnapInterval);
        public double MaxFrame => TaeInterop.IsSnapTo30FPS ? (Math.Round(MaxTime / SnapInterval)) : (MaxTime / SnapInterval);

        public bool IsRepeat = true;
        public bool IsPlaying = false;

        public bool Scrubbing = false;
        public bool prevScrubbing = false;

        private int prevFrameLoopCount = 0;

        public float PlaybackSpeed = 1.0f;

        public bool IsStepping = false;

        public void Update(bool playPauseBtnDown, bool shiftDown, GameTime gameTime, IEnumerable<TaeEditAnimEventBox> eventBoxes)
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
                    foreach (var box in eventBoxes)
                        box.PlaybackHighlight = false;

                    EventSim.OnSimulationEnd();
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

            if (justStartedPlaying || (curFrameLoopCount != prevFrameLoopCount))
                EventSim.OnSimulationStart();

            if (HkxAnimationLength.HasValue)
                MaxTime = HkxAnimationLength.Value;

            if (IsPlaying || Scrubbing || IsStepping)
            {
                if (IsPlaying)
                {
                    CurrentTime += (gameTime.ElapsedGameTime.TotalSeconds * PlaybackSpeed);
                }

                if (GUICurrentTime != oldGUICurrentTime)
                {
                    if (CurrentTime < 0)
                        CurrentTime += MaxTime;
                    TaeInterop.OnAnimFrameChange(Scrubbing);
                }

                bool justReachedAnimEnd = (CurrentTime > MaxTime);

                if (Math.Round(MaxTime / TaeInterop.CurrentAnimationFrameDuration) == 1)
                {
                    CurrentTime = 0;
                }

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
                        int currentFrame = (int)Math.Round((GUICurrentTime % MaxTime) / SnapInterval);
                        int prevFrame = (int)Math.Round((oldGUICurrentTime % MaxTime) / SnapInterval);
                        int eventStartFrame = (int)Math.Round(box.MyEvent.StartTime / SnapInterval);
                        int eventEndFrame = (int)Math.Round(box.MyEvent.EndTime / SnapInterval);

                        currentlyInEvent = currentFrame >= eventStartFrame && currentFrame < eventEndFrame;
                        prevFrameInEvent = !justStartedPlaying && prevFrame >= eventStartFrame && prevFrame < eventEndFrame;
                    }
                    else
                    {
                        if (Main.TAE_EDITOR.Config.EnableSnapTo30FPSIncrements)
                        {
                            int currentFrame = (int)Math.Floor((GUICurrentTime % MaxTime) / SnapInterval);
                            int prevFrame = (int)Math.Floor((oldGUICurrentTime % MaxTime) / SnapInterval);
                            int eventStartFrame = (int)Math.Round(box.MyEvent.StartTime / SnapInterval);
                            int eventEndFrame = (int)Math.Round(box.MyEvent.EndTime / SnapInterval);

                            currentlyInEvent = currentFrame >= eventStartFrame && currentFrame < eventEndFrame;
                            prevFrameInEvent = !justStartedPlaying && prevFrame >= eventStartFrame && prevFrame < eventEndFrame;
                        }
                        else
                        {
                            currentlyInEvent = GUICurrentTime >= box.MyEvent.StartTime && GUICurrentTime < box.MyEvent.EndTime;
                            prevFrameInEvent = !justStartedPlaying && oldGUICurrentTime >= box.MyEvent.StartTime && oldGUICurrentTime < box.MyEvent.EndTime;
                        }
                    }

                    if (currentlyInEvent)
                    {
                        box.PlaybackHighlight = true;

                        if (!prevFrameInEvent || !box.PrevCyclePlaybackHighlight || (curFrameLoopCount != prevFrameLoopCount) )
                        {
                            EventSim.OnEventEnter(box);
                        }

                        ////Also check if we looped playback
                        //if (!prevFrameInEvent || isFirstFrameAfterLooping)
                        //{

                        //}

                        EventSim.OnEventMidFrame(box);
                        
                    }
                    else
                    {
                        if (prevFrameInEvent || box.PrevCyclePlaybackHighlight)
                        {
                            EventSim.OnEventExit(box);
                        }

                        box.PlaybackHighlight = false;
                    }

                    box.PrevCyclePlaybackHighlight = box.PlaybackHighlight;

                    //if (IsPlaying && justReachedAnimEnd)
                    //{
                    //    box.PlaybackHighlight = false;
                    //}

                }


                if (IsPlaying && justReachedAnimEnd)
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
                }

            }

            oldGUICurrentTime = GUICurrentTime;
            prevScrubbing = Scrubbing;
            prevFrameLoopCount = curFrameLoopCount;
        }
    }
}
