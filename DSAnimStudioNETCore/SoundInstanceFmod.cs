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
        private FmodManager.FmodEventUpdater fmod = null;
        public override bool IsCompletelyFinished => fmod?.EventIsOver ?? true;

        public SoundInstanceFmod(FmodManager.FmodEventUpdater fmod)
        {
            this.fmod = fmod;
            if (fmod == null)
                Console.WriteLine("breakpoint");
        }

        public override void InnerUpdate(float deltaTime, Matrix listener, bool stopRequested, Vector3 position)
        {
            if (stopRequested)
                fmod?.Stop(false);
            fmod?.Update(deltaTime, Matrix.Identity, position);
        }

        public override void DisposeKill()
        {
            fmod?.Stop(true);
        }

        public override void Play()
        {
            fmod?.StartPlay();
        }
    }
}
