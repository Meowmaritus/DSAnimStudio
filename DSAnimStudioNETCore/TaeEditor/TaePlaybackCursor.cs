using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsAssetPipeline.Animation;
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
        public readonly NewGraph Graph;

        public TaePlaybackCursor(NewGraph graph)
        {
            Graph = graph;
        }

        public void NewApplyRelativeScrub(double t)
        {
            CurrentTime += t;
            if (CurrentTime < 0)
                CurrentTime = 0;
            if (!Main.Config.LoopEnabled && CurrentTime > MaxTime)
                CurrentTime = MaxTime;
            StartTime = CurrentTime;
            Scrubbing = true;
            IsPlaying = false;
        }
        
        public void CopyValuesFrom(TaePlaybackCursor pb)
        {
            this.MaxTime = pb.MaxTime;
            this.ContinuousTimeDelta = pb.ContinuousTimeDelta;
            this.CurrentTime = pb.CurrentTime;
            this.HkxAnimationFrameLength = pb.HkxAnimationFrameLength;
            this.HkxAnimationLength = pb.HkxAnimationLength;
            this.HKXDeltaTime = pb.HKXDeltaTime;
            this.IsPlaying = pb.IsPlaying;
            this.IsPlayingCombo = pb.IsPlayingCombo;
            this.IsPlayingRemoFullPreview = pb.IsPlayingRemoFullPreview;
            this.IsRepeat = pb.IsRepeat;
            this.IsStepping = pb.IsStepping;
            this.JustStartedPlaying = pb.JustStartedPlaying;
            this.ModPlaybackSpeed_Event603 = pb.ModPlaybackSpeed_Event603;
            this.ModPlaybackSpeed_Event608 = pb.ModPlaybackSpeed_Event608;
            this.ModPlaybackSpeed_GrabityRate =     pb.ModPlaybackSpeed_GrabityRate;
            this.ModPlaybackSpeed_NightfallEvent7032 = pb.ModPlaybackSpeed_NightfallEvent7032;
            this.OldCurrentTime = pb.OldCurrentTime;
            this.OldGUICurrentFrame = pb.OldGUICurrentFrame;
            this.OldGUICurrentTime = pb.OldGUICurrentTime;
            this.OldIsPlaying = pb.OldIsPlaying;
            this.prevScrubbing = pb.prevScrubbing;
            this.Scrubbing = pb.Scrubbing;
            this.SnapInterval = pb.SnapInterval;
            this.StartTime = pb.StartTime;
            this._currentTimeVal = pb._currentTimeVal;
            this.__snapInterval = pb.__snapInterval;
        }


        private double _currentTimeVal = 0;
        public double CurrentTime
        {
            get => _currentTimeVal;
            set
            {
                if (double.IsNaN(value))
                {
                    _currentTimeVal = 0;
                    //throw new InvalidOperationException("Current time was set to NaN.");
                }
                else if (double.IsInfinity(value))
                {
                    _currentTimeVal = 0;
                    //throw new InvalidOperationException("Current time was set to infinity.");
                }
                else
                {
                    _currentTimeVal = value;
                }

                //if (value <= 0.1 && Main.IsDebugBuild)
                //{
                //    Console.WriteLine("test");
                //}
                
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
            double guiCurFrameMod = GUICurrentFrameMod;
            if (guiCurFrameMod < 0)
                guiCurFrameMod = 0;
            double curTimeMod = CurrentTimeMod;
            if (curTimeMod < 0)
                curTimeMod = 0;
            return new[] { "Frame: " + (roundToNearestFrame ?
                    $"{((int)(guiCurFrameMod)),4:####}.000" :
                    $"{(MaxFrame <= 0 ? 0 : (Math.Truncate((guiCurFrameMod) * 1000) / 1000)),8:###0.000}") +
                    $" / {((int)((Math.Max(Math.Floor(LastMaxFrameGreaterThanZero), 0))))}", 
                    $"Time:  {(MaxFrame <= 0 ? 0 : (Math.Truncate((curTimeMod) * 1000) / 1000)),8:###0.000} / {LastMaxTimeGreaterThanZero:0.000}" };
        }

        public int CurrentLoopCount => MaxTime >= 0 ? (int)Math.Floor(CurrentTime / MaxTime) : 0;
        public int OldLoopCount => MaxTime >= 0 ? (int)Math.Floor(OldCurrentTime / MaxTime) : 0;
        public int CurrentLoopCountDelta => CurrentLoopCount - OldLoopCount;

        public static double LastMaxTimeGreaterThanZero = 0;

        public double CurrentTimeMod => Main.Config.LoopEnabled ? (MaxTime > 0 ? CurrentTime % MaxTime : 0) : CurrentTime;

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

        // public void ForceScrub(float hkxDeltaTime, float taeDeltaTime)
        // {
        //     HKXDeltaTime = hkxDeltaTime;
        //     ContinuousTimeDelta = taeDeltaTime;
        //
        //     if (!IsPlaying)
        //         OnScrubFrameChange();
        //     else
        //         OnPlaybackFrameChange();
        // }

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
            
            if (!Main.Config.LoopEnabled && StartTime > MaxTime)
                StartTime = MaxTime;

            bool synced = false;

            var animContainer = Graph?.ViewportInteractor?.CurrentModel?.AnimContainer;
            if (animContainer != null)
            {
                var syncTime = animContainer.PopSyncTime();
                if (syncTime.HasValue && !NewAnimationContainer.GLOBAL_SYNC_FORCE_REFRESH)
                {
                    var prevTime = CurrentTime;

                    if (!Main.Config.LoopEnabled && syncTime.Value > MaxTime)
                        syncTime = (float)MaxTime;

                    CurrentTime = syncTime.Value;
                    var timeShift = CurrentTime - prevTime;
                    OldCurrentTime += timeShift;
                    Graph.ViewportInteractor.NewScrub(absolute: true, time: syncTime.Value, foreground: true, background: true);
                    synced = true;
                }
            }

            //if (!IsPlaying || !OldIsPlaying || CurrentLoopCount != OldLoopCount)
            //{
            //    Graph.ViewportInteractor.NewScrub(absolute: true, time: (float)(OldGUICurrentTime), foreground: true, background: true);
            //}
            
            if (!synced)
                Graph.ViewportInteractor.NewScrub(absolute: false, time: (float)(GUICurrentTime - OldGUICurrentTime), foreground: true, background: true);

            OldGUICurrentTime = GUICurrentTime;
            OldCurrentTime = CurrentTime;
            OldGUICurrentFrame = GUICurrentFrame;
        }

        public double GUICurrentTime => zzz_DocumentManager.CurrentDocument.EditorScreen.Config.LockFramerateToOriginalAnimFramerate 
            ? (Math.Floor(CurrentTime / (SnapInterval ?? SnapInterval_Default)) 
            * (SnapInterval ?? SnapInterval_Default)) : CurrentTime;

        //public double HitWindowStart => CurrentTime - 0.001;
        //public double HitWindowEnd => CurrentTime + 0.001;

        //public double OldHitWindowStart => OldCurrentTime;
        //public double OldHitWindowEnd => OldCurrentTime + (CurrentSnapInterval / 2.0);

        public double GUICurrentTimeMod => Main.Config.LoopEnabled ? ModuloByMaxTime(GUICurrentTime) : GUICurrentTime;

        public double GUIStartTime => zzz_DocumentManager.CurrentDocument.EditorScreen.Config.LockFramerateToOriginalAnimFramerate 
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

        public double GUICurrentFrame => zzz_DocumentManager.CurrentDocument.EditorScreen.Config.LockFramerateToOriginalAnimFramerate
            ? (Math.Floor(CurrentTime / CurrentSnapInterval))
            : (CurrentTime / CurrentSnapInterval);

        public double CurrentFrame => (CurrentTime / CurrentSnapInterval);
        public double OldCurrentFrame => (OldCurrentTime / CurrentSnapInterval);

        public double GUICurrentFrameMod => Main.Config.LoopEnabled ? ModuloByMaxFrame(GUICurrentFrame) : GUICurrentFrame;

        public double MaxFrame => zzz_DocumentManager.CurrentDocument.EditorScreen.Config.LockFramerateToOriginalAnimFramerate
            ? (Math.Floor(MaxTime / CurrentSnapInterval))
            : (MaxTime / CurrentSnapInterval);

        public static double LastMaxFrameGreaterThanZero;

        public bool IsRepeat = true;

        private bool _isPlaying = false;

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                if (!_isPlaying)
                    IsTempPausedUntilAnimChange = false;
            }
        }
        
        public bool IsTempPausedUntilAnimChange = false;
        public bool IsPlayingRemoFullPreview = false;
        public bool OldIsPlaying { get; private set; } = false;



        private bool _scrubbing = false;

        public bool Scrubbing
        {
            get => _scrubbing;
            set
            {
                _scrubbing = value;
                if (_scrubbing)
                    IsTempPausedUntilAnimChange = false;
            }
        }
        public bool prevScrubbing = false;

        public static float GlobalBasePlaybackSpeed = 1.0f;
        public float BasePlaybackSpeed => GlobalBasePlaybackSpeed;
        public float ModPlaybackSpeed_GrabityRate = 1.0f;
        public float ModPlaybackSpeed_Event603 = 1.0f;
        public float ModPlaybackSpeed_Event608 = 1.0f;
        public float ModPlaybackSpeed_NightfallEvent7032 = 1.0f;
        public float ModPlaybackSpeed_AC6Event9700 = 1.0f;

        public float EffectivePlaybackSpeed => BasePlaybackSpeed 
            * ((zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS1 or SoulsAssetPipeline.SoulsGames.DS1R) ? ModPlaybackSpeed_GrabityRate : 1)
            * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3 ? ModPlaybackSpeed_Event603 : 1)
            * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR ? ModPlaybackSpeed_Event608 : 1)
            * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 ? ModPlaybackSpeed_Event608 : 1)
            * ((zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS1R && Main.IsNightfallBuild) ? ModPlaybackSpeed_NightfallEvent7032 : 1)
            * ((zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6) ? ModPlaybackSpeed_AC6Event9700 : 1);

        public bool IsStepping = false;

        public bool JustStartedPlaying = false;

        public double ContinuousTimeDelta;

        public bool JustLooped => CurrentLoopCountDelta != 0;

        public void RestartFromBeginning()
        {
            CurrentTime = 0;
            StartTime = 0;
            OldIsPlaying = false;
            // if (IsTempPausedUntilAnimChange && IsPlaying)
            // {
            //     IsPlaying = false;
            // }
            
            //IsTempPausedUntilAnimChange = false;
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
        }

        public void ResetAll()
        {
            CurrentTime = 0;
            //IsPlaying = false;
            //IsRepeat = true;
            //IsStepping = false;
            JustStartedPlaying = false;
            OldIsPlaying = false;
            MaxTime = 0;
            OldGUICurrentFrame = 0;
            OldGUICurrentTime = 0;
            //prevScrubbing = false;
            //Scrubbing = false;
            StartTime = 0;

            //PlaybackSpeed = 1.0f;
        }

        public void Transport_PlayPause(bool isUserInput = true)
        {
            IsStepping = false;

            // Reset to start if end is reached while loop is off.
            if (isUserInput && !Main.Config.LoopEnabled && !(IsPlaying && !IsTempPausedUntilAnimChange) && CurrentTime >= MaxTime)
            {
                Graph.MainScreen.HardReset(startPlayback: true);
                IsTempPausedUntilAnimChange = false;
                //IsPlaying = true;
                return;
            }

            if (IsPlaying)
            {
                IsPlaying = false;
            }
            else
            {
                IsPlaying = true;
                JustStartedPlaying = true;
            }

            if (IsPlayingRemoFullPreview)
            {
                if (IsPlaying)
                    RemoManager.ResumeStreamedBGM();
                else
                    RemoManager.PauseStreamBGM();
            }
        }

        public void Update(TaeEditorScreen mainScreen, IEnumerable<DSAProj.Action> eventBoxes, bool ignoreDeltaTime = false)
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

            //if (JustStartedPlaying || (CurrentLoopCount != OldLoopCount))
            //    OnPlaybackStarted();

            if (HkxAnimationLength.HasValue)
                MaxTime = HkxAnimationLength.Value;

            if ((IsPlaying && !IsTempPausedUntilAnimChange) || Scrubbing || IsStepping)
            {
               

                if (IsPlaying)
                {
                    if (!ignoreDeltaTime)
                        CurrentTime += (Main.DELTA_UPDATE * EffectivePlaybackSpeed);
                    if (!Main.Config.LoopEnabled && CurrentTime > MaxTime && !IsPlayingCombo)
                    {
                        CurrentTime = MaxTime;
                        IsTempPausedUntilAnimChange = true;
                    }
                }

                if (!HkxAnimationLength.HasValue)
                    MaxTime = 0;

                try
                {
                    // Thread locked from outside this function
                    foreach (var ev in eventBoxes)
                    {
                        bool CheckHighlight(double playbackCursorPos, DSAProj.Action mouseHoverBox)
                        {
                            //if (!IsActive)
                            //    return false;

                            playbackCursorPos = Math.Round(playbackCursorPos, 4);

                            if (!mainScreen.Config.SoloHighlightActionOnHover ||
                                mouseHoverBox == null || IsPlaying ||
                                Scrubbing)
                            {
                                return playbackCursorPos >= Math.Round(ev.StartTime, 4) && playbackCursorPos <= Math.Round(ev.EndTime, 4);
                            }
                            else
                            {
                                return mouseHoverBox == ev;
                            }

                        }

                        bool GetWasJustEnteredDuringPlayback()
                        {
                            var curHighlight = CheckHighlight(GUICurrentTimeMod, mainScreen.NewHoverAction);
                            var prevHighlight = !JustStartedPlaying &&
                                CheckHighlight(OldGUICurrentTimeMod, mainScreen.NewHoverActionPrevFrame);

                            if (OldCurrentTime == 0)
                            {
                                // Prevent an occasional double register?
                                prevHighlight = true;
                            }

                            if (Scrubbing || IsPlaying)
                            {
                                if (curHighlight && (!prevHighlight || JustStartedPlaying))
                                    return true;

                                if (IsPlaying)
                                {
                                    float timeDirection = (float)(EffectivePlaybackSpeed);

                                    if (timeDirection > 0)
                                    {
                                        if (CurrentTimeMod >= ev.StartTime)
                                        {
                                            if (OldCurrentTimeModWrapped < ev.StartTime ||
                                                JustLooped || JustStartedPlaying ||
                                                OldCurrentTime == 0)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    else if (timeDirection < 0)
                                    {
                                        if (CurrentTimeMod <= ev.EndTime)
                                        {
                                            if (OldCurrentTimeModWrapped >= ev.EndTime ||
                                                JustLooped || JustStartedPlaying ||
                                                CurrentTime == MaxTime)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                    else
                                    {

                                        Console.WriteLine(DateTime.Now.Millisecond + " - Fatcat");
                                    }

                                    //if ((timeDirection > 0 && (OldCurrentTimeModWrapped < MyEvent.StartTime && CurrentTimeMod >= MyEvent.StartTime)) ||
                                    //    (timeDirection < 0 && (OldCurrentTimeModWrapped >= MyEvent.EndTime && CurrentTimeMod < MyEvent.EndTime)))
                                    //{
                                    //    return true;
                                    //}
                                }


                            }

                            if (curHighlight && !OldIsPlaying && IsPlaying)
                            {
                                return true;
                            }

                            return false;
                        }


                        //if (ev.GroupIndex >= 0 && ev.GroupIndex < mainScreen.Graph.AnimRef.EventGroups.Count)
                        //{
                        //    ev.WasJustEnteredDuringPlayback = GetWasJustEnteredDuringPlayback();
                        //    ev.PlaybackHighlightMidst = CheckHighlight(GUICurrentTimeMod, mainScreen.NewHoverEvent);
                        //    ev.PlaybackHighlight = ev.PlaybackHighlightMidst || ev.WasJustEnteredDuringPlayback;
                        //}
                        //else
                        //{
                        //    ev.WasJustEnteredDuringPlayback = false;
                        //    ev.PlaybackHighlightMidst = false;
                        //    ev.PlaybackHighlight = false;
                        //}
                        

                        

                        if (!HkxAnimationLength.HasValue)
                        {
                            if (ev.EndTime > MaxTime)
                                MaxTime = ev.EndTime;
                        }

                        //ev.PrevCyclePlaybackHighlight = ev.PlaybackHighlight;


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

            UpdateScrubbing();

            JustStartedPlaying = false;

            OldIsPlaying = IsPlaying;
        }
    }
}
