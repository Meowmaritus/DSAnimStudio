using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public abstract class SoundInstance
    {
        public SoundPlayInfo PlayInfo { get; set; }
        public bool RequestStop = false;
        public abstract bool IsCompletelyFinished { get; }
        public abstract void InnerUpdate(zzz_SoundManagerIns soundMan, float deltaTime, Matrix listener, bool stopRequested, Vector3 position);
        public void Update(zzz_SoundManagerIns soundMan, float deltaTime, Matrix listener, Vector3 position)
        {
            InnerUpdate(soundMan, deltaTime, listener, RequestStop, position);
        }
        public abstract void DisposeKill();
        public abstract void Play();
    }
}
