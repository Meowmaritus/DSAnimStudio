using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SoundInstanceFmod : SoundInstance
    {
        public enum States
        {
            HasntStartedYet,
            Playing,
            Paused,
            Finished,
        }

        private readonly SafeFmod.SafeFmodEventSys fmodEventSystem;
        private readonly long fmodEventHandle;

        private States lastFmodEventState;

        //private NewFmodIns.FmodEventUpdater fmod = null;
        public override bool IsCompletelyFinished => lastFmodEventState == States.Finished;

        public SoundInstanceFmod(SafeFmod.SafeFmodEventSys fEventSys, long fEventHandle)
        {
            fmodEventSystem = fEventSys;
            fmodEventHandle = fEventHandle;
        }

        public override void InnerUpdate(zzz_SoundManagerIns soundMan, float deltaTime, Matrix listener, bool stopRequested, Vector3 position)
        {
            //if (stopRequested)
            //    fmod?.Stop(false);
            //fmod?.Update(soundMan, deltaTime, Matrix.Identity, position);

            if (stopRequested)
            {
                fmodEventSystem.Event_Stop(fmodEventHandle, false);
            }

            float volume = soundMan.FmodBaseSoundVolume * (soundMan.AdjustSoundVolume / 100);

            lastFmodEventState = fmodEventSystem.Event_Update(fmodEventHandle, deltaTime, volume, Matrix.Identity, position);
        }

        public override void DisposeKill()
        {
            fmodEventSystem?.Event_Stop(fmodEventHandle, true);
            //fmod?.Stop(true);
        }

        public override void Play()
        {
            fmodEventSystem.Event_StartPlay(fmodEventHandle);
            //fmod?.StartPlay();
        }
    }
}
