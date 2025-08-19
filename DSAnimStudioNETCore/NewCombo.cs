using System.Collections.Generic;
using DSAnimStudio.TaeEditor;

namespace DSAnimStudio
{
    public class NewCombo
    {
        public enum PlaybackStates
        {
            Init,
            Playing,
            Finished,
            Failed,
        }
        
        public NewAnimationContainer AnimContainer;
        public TaeEditorScreen MainScreen;

        public PlaybackStates PlaybackState = PlaybackStates.Init;
        public float PlaybackSpeed = 1;
        

        public void Init(NewAnimationContainer animContainer, TaeEditorScreen mainScreen)
        {
            AnimContainer = animContainer;
            MainScreen = mainScreen;
            TotalPlaybackPosition = 0;
        }

        public void StartPlayback()
        {
            CurrentIndex = -1;
            TotalPlaybackPosition = 0;
            CurrentEntryAnim = null;
            CurrentEntryPlaybackPosition = 0;
            
            AnimContainer.CompletelyClearAllSlots();
            PlaybackState = PlaybackStates.Playing;
            GoToNextComboEntry(0);
        }
        
        public int CurrentIndex { get; private set; } = 0;
        
        public float CurrentEntryPlaybackPosition { get; private set; }
        
        public float TotalPlaybackPosition { get; private set; } = 0;
        
        public NewHavokAnimation CurrentEntryAnim { get; private set; }
        
        public List<Entry> Entries = new List<Entry>();

        private float FpsSnapAccumTime = 0;
        
        
        private void SetTime(float time)
        {

            //CurrentEntryAnim.Scrub(absolute: false, time, out _);
            
            
            MainScreen.Graph?.ViewportInteractor?.NewScrub(false, time, true, true);
            CurrentEntryPlaybackPosition += time;
            var playbackCursor = MainScreen.PlaybackCursor;
            if (playbackCursor != null)
            {
                playbackCursor.IsPlaying = false;
                playbackCursor.Scrubbing = false;
                playbackCursor.CurrentTime = CurrentEntryPlaybackPosition;
                playbackCursor.IgnoreCurrentRelativeScrub();
            }
        }

        private void GoToNextComboEntry(float timeOffset)
        {
            if (CurrentIndex >= Entries.Count - 1)
            {
                PlaybackState = PlaybackStates.Finished;
                return;
            }

            CurrentIndex++;
            var next = Entries[CurrentIndex];

            var taeCategory = MainScreen.Proj.SAFE_GetFirstAnimCategoryFromFullID(next.AnimationID);
            var taeAnim = MainScreen.Proj.SAFE_GetFirstAnimationFromFullID(next.AnimationID);

            if (taeCategory != null && taeAnim != null)
            {
                float finalTime = (next.StartFrame * TaeExtensionMethods.GetCurrentSnapInterval()) + timeOffset;
                float blendDuration = next.BlendDuration * TaeExtensionMethods.GetCurrentSnapInterval();

                if (next.BlendDuration < 0)
                {
                    blendDuration = taeAnim.SAFE_GetBlendDuration();
                }
                
                var hkxID = taeAnim.GetHkxID(MainScreen.Proj);
                MainScreen.SelectNewAnimRef(taeCategory, taeAnim, disableHkxSelect: true);
                AnimContainer.RequestAnim(NewAnimSlot.SlotTypes.Base, hkxID, true, next.Weight, finalTime, blendDuration);
                AnimContainer.AccessAnimSlots(slots =>
                {
                    CurrentEntryAnim = slots[NewAnimSlot.SlotTypes.Base].GetForegroundAnimation();
                    //slots[NewAnimSlot.SlotTypes.Base].ChangeBlendDurationOfCurrentRequest(blendDuration, false);
                });

                CurrentEntryAnim.EnableLooping = next.IsLoop;
                
                CurrentEntryPlaybackPosition = finalTime;
                SetTime(0);
            }
            else
            {
                PlaybackState = PlaybackStates.Failed;
            }
            
            
        }
        
        
        public void Update(float elapsedTime)
        {
            elapsedTime *= PlaybackSpeed;

            if (Main.Config.LockFramerateToOriginalAnimFramerate)
            {
                float snap = (float)MainScreen.PlaybackCursor.CurrentSnapInterval;
                FpsSnapAccumTime += elapsedTime;
                if (FpsSnapAccumTime >= snap)
                {
                    FpsSnapAccumTime -= snap;
                    elapsedTime = snap;
                }
                else
                {
                    elapsedTime = 0;
                    return;
                }
            }
            
            if (PlaybackState == PlaybackStates.Playing)
            {
                var entry = Entries[CurrentIndex];
                var isFirst = CurrentIndex == 0;
                var isLast = CurrentIndex >= Entries.Count - 1;

                float upcomingPlaybackPos = CurrentEntryPlaybackPosition + elapsedTime;

                float endTime = entry.EndFrame * TaeExtensionMethods.GetCurrentSnapInterval();
                
                if (upcomingPlaybackPos >= endTime)
                {
                    if (isLast)
                    {
                        SetTime(endTime - CurrentEntryPlaybackPosition);
                        PlaybackState = PlaybackStates.Finished;
                    }
                    else
                    {
                        float nextAnimStartOffset = (upcomingPlaybackPos - endTime);
                        SetTime(endTime - CurrentEntryPlaybackPosition);
                        GoToNextComboEntry(0);
                        SetTime(nextAnimStartOffset);
                    }

                }
                else
                {
                    SetTime(upcomingPlaybackPos - CurrentEntryPlaybackPosition);
                }

                TotalPlaybackPosition += elapsedTime;
            }
        }
        
        
        
        public class Entry
        {
            public SplitAnimID AnimationID;
            public bool IsLoop;
            public float Weight = 1;
            public float BlendDuration;
            public float StartFrame;
            public float EndFrame;
        }
    }
}