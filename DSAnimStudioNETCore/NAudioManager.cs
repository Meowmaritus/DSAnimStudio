using DSAnimStudio.ImguiOSD;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NAudioManager
    {
        public static void Shutdown()
        {
            lock (_lock_NAudio)
            {
                foreach (var kvp in NAudioOutputs)
                {
                    kvp.Value.Output.Stop();
                    kvp.Value.Output.Dispose();
                }
                NAudioOutputs.Clear();
            }
        }

        public static void NAudioDebugDump(ISampleProvider samp, int numSamples, string dumpFileName)
        {
            float[] buffer = new float[numSamples];
            samp.Read(buffer, 0, numSamples);
            var dir = Path.GetDirectoryName(dumpFileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            using (WaveFileWriter writer = new WaveFileWriter(dumpFileName, samp.WaveFormat))
            {
                writer.WriteSamples(buffer, 0, numSamples);
            }
        }

        public class MixerOut
        {
            //public WasapiOut Output;
            public IWavePlayer Output;
            public NAudio.Wave.SampleProviders.MixingSampleProvider Mixer;
            public NAudio.Wave.SampleProviders.VolumeSampleProvider VolumeAdjust;
            public MixerOut(ISampleProvider initSample)
            {
                Output = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 50);
                //Output = new WaveOutEvent()
                //{
                //    DesiredLatency = 300,

                //};
                //Output = new WaveOut();
                //Output = new DirectSoundOut();
                Mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider(initSample.WaveFormat);
                Mixer.ReadFully = true;
                VolumeAdjust = new NAudio.Wave.SampleProviders.VolumeSampleProvider(Mixer);
                Mixer.AddMixerInput(initSample);
                Output.Init(VolumeAdjust);
                Output.Play();
            }
            public void EnsurePlay()
            {
                if (Output.PlaybackState != PlaybackState.Playing)
                    Output.Play();
            }
        }

        public struct NAudioOutputKey
        {
            public int SampleRate;
            public int NumChannels;
        }

        private static Dictionary<NAudioOutputKey, MixerOut> NAudioOutputs = new Dictionary<NAudioOutputKey, MixerOut>();
        //private static NAudio.Wave.SampleProviders.VolumeSampleProvider MainVolumeSampleProvider;
        private static object _lock_NAudio = new object();

        //public static void SetOutputVolume(float volume)
        //{
        //    lock (_lock_NAudio)
        //    {
        //        foreach (var output in NAudioOutputs)
        //        {
        //            output.Value.VolumeAdjust.Volume = volume;
        //        }
        //    }
        //}

        //public static void NAudioInit(ISampleProvider initSample)
        //{
        //    lock (_lock_NAudio)
        //    {
        //        if (NAudioOutputs.ContainsKey(initSample.WaveFormat.SampleRate))
        //        {
        //            NAudioOutputs[initSample.WaveFormat.SampleRate].Mixer.AddMixerInput(initSample);
        //        }
        //        else
        //        {
        //            NAudioOutputs.Add(initSample.WaveFormat.SampleRate, new MixerOut(initSample));

        //        }
        //    }
        //}


        public static bool PlaySound(NAudio.Wave.ISampleProvider samp)
        {


            bool success = false;
            //NAudioInit(samp);
            lock (_lock_NAudio)
            {


                try
                {

                    var key = new NAudioOutputKey()
                    {
                        SampleRate = samp.WaveFormat.SampleRate,
                        NumChannels = samp.WaveFormat.Channels
                    };

                    // TEMP HOTFIX
                    if (key.NumChannels > 2)
                        return false;
                    
                    if (NAudioOutputs.ContainsKey(key))
                    {
                        var output = NAudioOutputs[key];
                        output.Mixer.AddMixerInput(samp);
                        output.EnsurePlay();
                        success = true;
                    }
                    else
                    {
                        var output = new MixerOut(samp);
                        NAudioOutputs.Add(key, output);
                        output.EnsurePlay();
                        success = true;
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    if (!zzz_SoundManagerIns.SOUND_DISABLED)
                    {
                        zzz_SoundManagerIns.SOUND_DISABLED = true;
                        DialogManager.DialogOK(null, "Failed to initialize audio output. " +
                            "Make sure you have an audio device connected and working and " +
                            "that no other app is taking exclusive control of the device.\n\n" +
                            "Once you free the device, go to the 'Sound' window and check 'Enable Audio System'\n" +
                            "or restart DS Anim Studio for Wwise games (Elden Ring and later)");
                    }
                }
            }

            return success;
        }

        public static void StopSound(NAudio.Wave.ISampleProvider samp)
        {
            //NAudioInit();
            lock (_lock_NAudio)
            {
                var key = new NAudioOutputKey()
                {
                    SampleRate = samp.WaveFormat.SampleRate,
                    NumChannels = samp.WaveFormat.Channels
                };
                if (NAudioOutputs.ContainsKey(key))
                {
                    var output = NAudioOutputs[key];
                    output.Mixer.RemoveMixerInput(samp);
                }
            }
        }
    }
}
