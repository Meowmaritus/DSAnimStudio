using Microsoft.Xna.Framework;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class WemPlaybackInstance : IDisposable
    {
        public enum States
        {
            NotStartedYet,
            StartFadeInDelay,
            StartFadeInTransition,
            Playing,
            StopFadeOutDelay,
            StopFadeOutTransition,
            Stopped,
        }

        public uint WEMID;

        public States State = States.NotStartedYet;

        MemoryStream vorbisStream;
        WaveFileReader vorbisWaveReader;
        WavLoopingSampleProvider vorbisLoop;

        //NAudio.Vorbis.VorbisWaveReader vorbisWaveReader;
        //VorbisLoopingSampleProvider vorbisLoop;

        ISampleProvider MixerOut;

        public float Volume => vorbisLoop.Volume;
        public float Pitch => BasePitch;
        public float Pan => vorbisLoop.Pan;


        public bool StopRequested = false;

        public readonly float BaseVolume;
        private static float fadeInOutVolumeRatio = 1;
        public readonly float BasePitch;

        public float DistanceFalloff = 20;
        public float PanWidth = 5;

        public float FadeOutDuration = 0;
        public float FadeOutDelay = 0;
        private float FadeOutTimer = 0;

        public float FadeInDuration = 0;
        public float FadeInDelay = 0;
        private float FadeInTimer = 0;



        private object _locker = new object();

        private void TryRequestStop()
        {
            if (State != States.Playing)
                return;

            State = States.StopFadeOutDelay;

            if (FadeOutDelay == 0)
            {
                fadeInOutVolumeRatio = 1;
                State = States.StopFadeOutTransition;
                if (FadeOutDuration == 0)
                {
                    fadeInOutVolumeRatio = 0;
                    State = States.Stopped;
                    Wwise.StopSound(MixerOut);
                    vorbisStream?.Dispose();
                    vorbisStream = null;
                    vorbisWaveReader?.Dispose();
                    vorbisWaveReader = null;
                    vorbisLoop.EndEarly = true;
                }
            }

            StopRequested = false;
        }

        public void Stop(bool immediate)
        {
            StopRequested = true;

            lock (_locker)
            {
                if (immediate)
                {
                    Wwise.StopSound(MixerOut);
                    vorbisStream?.Dispose();
                    vorbisStream = null;
                    vorbisWaveReader?.Dispose();
                    vorbisWaveReader = null;
                    vorbisLoop.EndEarly = true;
                    State = States.Stopped;
                }
                else
                {
                    StopRequested = true;
                    
                }
            }
        }

        public void Play()
        {
            lock (_locker)
            {
                State = States.StartFadeInDelay;
                if (FadeInDelay == 0)
                {
                    fadeInOutVolumeRatio = 0;
                    State = States.StartFadeInTransition;
                    lock (_locker)
                    {
                        Wwise.PlaySound(MixerOut);
                    }
                    if (FadeInDuration == 0)
                    {
                        fadeInOutVolumeRatio = 1;
                        State = States.Playing;
                    }
                }
                
            }
        }

        public WemPlaybackInstance(uint wemid, Wwise.LoadedWEM loadedOgg, float volume, float pitch, Matrix listener, float distanceFalloff, bool enableLoop,
            float fadeOutDuration, float fadeOutDelay, float fadeInDuration, float fadeInDelay)
        {
            WEMID = wemid;
            lock (_locker)
            {
                
                //vorbisStream = new MemoryStream(loadedOgg.FixedOggBytes);
                //vorbisWaveReader = new NAudio.Vorbis.VorbisWaveReader(vorbisStream);


                //vorbisLoop = new VorbisLoopingSampleProvider(vorbisWaveReader, pitch);
                vorbisStream = new MemoryStream(loadedOgg.WavBytes);

                vorbisWaveReader = new WaveFileReader(vorbisStream);
                vorbisLoop = new WavLoopingSampleProvider(vorbisWaveReader, pitch);

                vorbisLoop.LoopEnabled = loadedOgg.LoopEnabled && enableLoop;
                vorbisLoop.LoopStart = loadedOgg.LoopStart;
                vorbisLoop.LoopEnd = loadedOgg.LoopEnd;
                vorbisLoop.SampleCount = loadedOgg.TotalSampleCount;

                var pitchChange = new WdlResamplingSampleProvider_Meme(vorbisLoop, vorbisLoop.WaveFormat.SampleRate, vorbisLoop.WaveFormat.SampleRate, pitch);

                MixerOut = pitchChange;


                FadeOutDuration = fadeOutDuration;
                FadeOutDelay = fadeOutDelay;

                FadeInDuration = fadeInDuration;
                FadeInDelay = fadeInDelay;

                BaseVolume = volume;
                BasePitch = pitch;

                DistanceFalloff = distanceFalloff;

                if (Wwise.DEBUG_DUMP_ALL_WEM)
                {
                    var timestamp = DateTime.Now.ToString("yyyy_MM_dd__HH_mm_ss_fffffff");

                    


                    //Wwise.NAudioDebugDump(vorbisWaveReader, (int)vorbisLoop.SampleCount, $"WEM_DUMP\\WEM_{loadedOgg.WEMID}____{timestamp}____vorbisWaveReader.wav");
                    //vorbisLoop.VorbisSamplePosition = 0;
                    //Wwise.NAudioDebugDump(vorbisLoop, (int)vorbisLoop.SampleCount, $"WEM_DUMP\\WEM_{loadedOgg.WEMID}____{timestamp}____vorbisLoop.wav");
                    //vorbisLoop.VorbisSamplePosition = 0;
                    //Wwise.NAudioDebugDump(MixerOut, (int)vorbisLoop.SampleCount, $"WEM_DUMP\\WEM_{loadedOgg.WEMID}____{timestamp}____MixerOut.wav");
                    //vorbisLoop.VorbisSamplePosition = 0;



                    File.WriteAllBytes($"WEM_DUMP\\WEM_{loadedOgg.WEMID}____{timestamp}____OGG.ogg", loadedOgg.FixedOggBytes);
                }
            }

            //Update(0, listener, isInit: true);
        }

        public static float Calculate2DPan(Vector3 pan3D, float panWidth)
        {
            float angle = MathF.Atan2(pan3D.Z, pan3D.X);
            //float pan = MathF.Cos(angle);
            float pan = pan3D.X / pan3D.Length();

            //TODO
            //float pan = ((leftChannel / (leftChannel + rightChannel)) * 2) - 1;

            //var panMult = MathF.Sqrt(1f / pan3D.Length());
            var panMult = pan3D.Length() / 10;
            panMult = MathHelper.Clamp(panMult, 0.5f, 1);

            //panMult = panMult * panMult;
            return pan * panMult;
        }

        public void Update(float deltaTime, Matrix listener, Vector3 position)
        {
            if (StopRequested)
            {
                TryRequestStop();
            }

            if (State == States.StopFadeOutDelay)
            {
                if (FadeOutTimer < FadeOutDelay)
                    FadeOutTimer += deltaTime;

                fadeInOutVolumeRatio = 1;

                if (FadeOutTimer >= FadeOutDelay)
                {
                    State = States.StopFadeOutTransition;
                }
            }
            else if (State == States.StopFadeOutTransition)
            {
                if (FadeOutTimer < FadeOutDelay + FadeOutDuration)
                    FadeOutTimer += deltaTime;

                fadeInOutVolumeRatio = 1 - MathHelper.Clamp((FadeOutTimer - FadeOutDelay) / FadeOutDuration, 0, 1);

                if (FadeOutTimer >= FadeOutDelay + FadeOutDuration)
                {
                    Stop(immediate: true);
                    return;
                }
            }
            else if (State == States.StartFadeInDelay)
            {
                if (FadeInTimer < FadeOutDelay)
                    FadeInTimer += deltaTime;

                fadeInOutVolumeRatio = 0;

                if (FadeInTimer >= FadeInDelay)
                {
                    lock (_locker)
                    {
                        Wwise.PlaySound(MixerOut);
                    }
                    State = States.StartFadeInTransition;
                }
            }
            else if (State == States.StartFadeInTransition)
            {
                if (FadeInTimer < FadeInDelay + FadeInDuration)
                    FadeInTimer += deltaTime;

                fadeInOutVolumeRatio = MathHelper.Clamp((FadeInTimer - FadeInDelay) / FadeInDuration, 0, 1);

                if (FadeInTimer >= FadeInDelay + FadeInDuration)
                {

                    State = States.Playing;
                }
            }
            else if (State == States.Playing)
            {
                fadeInOutVolumeRatio = 1;
            }
            else if (State == States.Stopped)
            {
                return;
            }

            Vector3 offsetFromListener = Vector3.Transform(position * new Vector3(1,1,-1), Matrix.Invert(listener));

            float volumeRatio = 10 / offsetFromListener.Length();
            //volumeRatio *= volumeRatio;
            volumeRatio = MathHelper.Clamp(volumeRatio, 0, 1);
            float volume = BaseVolume * volumeRatio * fadeInOutVolumeRatio * (SoundManager.AdjustSoundVolume / 100f);

            var pan = Calculate2DPan(offsetFromListener, PanWidth);
            pan = MathHelper.Clamp(pan, -1, 1);

            bool streamStopped = false;

            lock (_locker)
            {
                vorbisLoop.Volume = volume;
                vorbisLoop.Pan = pan;
                streamStopped = vorbisLoop.IsActuallyOver && !vorbisLoop.LoopEnabled;
            }

            if (streamStopped)
                StopRequested = true;
        }

        public void Dispose()
        {
            Stop(true);

        }
    }
}
