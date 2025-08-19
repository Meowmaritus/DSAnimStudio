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
        public SoundSlot()
        {

        }

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
        public Vector3 PositionOffset;

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
        public void Play(zzz_SoundManagerIns soundMan, Matrix listener)
        {
            foreach (var inst in Instances)
            {
                inst.Update(soundMan, 0, listener, Position + PositionOffset);
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
            if (Instances.Count > 0 && Instances.Any(inst => !inst.IsCompletelyFinished))
            {
                Console.WriteLine("test");
            }
            foreach (var inst in Instances)
                inst.DisposeKill();
            Instances.Clear();
            SlotCompletelyEmpty = true;
        }
        public void Update(zzz_SoundManagerIns soundMan, float deltaTime, Matrix listener)
        {
            if (SlotCompletelyEmpty)
                return;
            
            //System.Diagnostics.Debug.WriteLine($"SoundSlot Update - {State} - UpdatingPosition={UpdatingPosition}");
            if (State is States.InitializingInBgThread)
            {
                if (BackgroundInitializeTask.IsCompleted)
                {
                    Instances = BackgroundInitializeTask.Result;
                    if (Instances == null || Instances.Count == 0)
                    {
                        KillImmediately();
                        return;
                    }

                    Play(soundMan, listener);
                }
                else
                {
                    return;
                }
            }

            if (UpdatingPosition && State is States.InitializingInBgThread or States.Playing)
            {
                Position = GetPosFunc();
                PositionOffset = Vector3.Zero;
            }

            bool anyStillPlaying = false;
            foreach (var inst in Instances)
            {
                inst.Update(soundMan, deltaTime, listener, Position + PositionOffset);
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
