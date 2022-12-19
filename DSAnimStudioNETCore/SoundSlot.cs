using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SoundSlot
    {
        public SoundPlayInfo PlayInfo;
        public enum States
        {
            InitializingInBgThread,
            Playing,
            Stopping,
            Stopped
        }
        public bool UpdatingPosition = true;
        public Vector3 Position;

        public Func<Vector3> GetPosFunc { get; set; }
        public Task<List<SoundInstance>> BackgroundInitializeTask;
        public States State { get; private set; } = States.InitializingInBgThread;
        private List<SoundInstance> Instances = new List<SoundInstance>();
        public bool SlotCompletelyEmpty { get; private set; } = false;

        public IReadOnlyList<SoundInstance> PeekInstances() => Instances;

        public void AddInstance(SoundInstance inst)
        {
            Instances.Add(inst);
        }
        public void Play(Matrix listener)
        {
            foreach (var inst in Instances)
            {
                inst.Update(0, listener, Position);
                inst.Play();
            }
            State = States.Playing;
        }
        public void Stop()
        {
            foreach (var inst in Instances)
                inst.RequestStop = true;
        }
        public void KillImmediately()
        {
            foreach (var inst in Instances)
                inst.DisposeKill();
            Instances.Clear();
            SlotCompletelyEmpty = true;
        }
        public void Update(float deltaTime, Matrix listener)
        {
            //System.Diagnostics.Debug.WriteLine($"SoundSlot Update - {State} - UpdatingPosition={UpdatingPosition}");
            if (State is States.InitializingInBgThread)
            {
                if (BackgroundInitializeTask.IsCompleted)
                {
                    Instances = BackgroundInitializeTask.Result;
                    Play(listener);
                }
                else
                {
                    return;
                }
            }

            if (UpdatingPosition && State is States.InitializingInBgThread or States.Playing)
            {
                Position = GetPosFunc();
            }

            bool anyStillPlaying = false;
            foreach (var inst in Instances)
            {
                inst.Update(deltaTime, listener, Position);
                if (!inst.IsCompletelyFinished)
                    anyStillPlaying = true;
            }
            if (!anyStillPlaying)
            {
                SlotCompletelyEmpty = true;
            }
        }
    }
}
