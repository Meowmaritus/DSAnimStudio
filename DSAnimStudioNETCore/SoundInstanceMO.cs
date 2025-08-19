using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SoundInstanceMO : SoundInstance
    {
        public zzz_MagicOrchestraManagerInst.MOSoundFileKey Key => moSound.Key;
        private MOPlaybackInstance moSound;
        public float Volume => moSound.Volume;
        public float Pitch => moSound.Pitch;
        public float Pan => moSound.Pan;
        public MOPlaybackInstance.States State => moSound.State;

        public SoundInstanceMO(MOPlaybackInstance moSound)
        {
            this.moSound = moSound;
        }

        public override bool IsCompletelyFinished => moSound.State == MOPlaybackInstance.States.Stopped;

        public override void InnerUpdate(zzz_SoundManagerIns soundMan, float deltaTime, Matrix listener, bool stopRequested, Vector3 position)
        {
            if (stopRequested)
                moSound.Stop(immediate: false);
            moSound.Update(deltaTime, listener, position, PlayInfo);
        }

        public override void DisposeKill()
        {
            moSound.Stop(true);
            moSound.Dispose();
        }

        public override void Play()
        {
            moSound.Play();
        }
    }
}
