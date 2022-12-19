using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SoundInstanceWwise : SoundInstance
    {
        public uint WEMID => wem.WEMID;
        private WemPlaybackInstance wem;
        public float Volume => wem.Volume;
        public float Pitch => wem.Pitch;
        public float Pan => wem.Pan;
        public WemPlaybackInstance.States State => wem.State;

        public SoundInstanceWwise(WemPlaybackInstance wem)
        {
            this.wem = wem;
        }

        public override bool IsCompletelyFinished => wem.State == WemPlaybackInstance.States.Stopped;

        public override void InnerUpdate(float deltaTime, Matrix listener, bool stopRequested, Vector3 position)
        {
            if (stopRequested)
                wem.Stop(immediate: false);
            wem.Update(deltaTime, listener, position);
        }

        public override void DisposeKill()
        {
            wem.Stop(true);
            wem.Dispose();
        }

        public override void Play()
        {
            wem.Play();
        }
    }
}
