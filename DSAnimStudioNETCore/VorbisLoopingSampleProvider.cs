using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    internal class VorbisLoopingSampleProvider : ISampleProvider, IDisposable
    {
        public VorbisWaveReader Vorbis;
        public bool LoopEnabled;
        public long LoopStart;
        public long LoopEnd;
        public long SampleCount;

        VolumeSampleProvider provVolume;
        PanningSampleProvider provPan;
        int SourceSampleRate;

        public float Pan
        {
            get => provPan != null ? provPan.Pan : 0;
            set
            {
                if (provPan != null)
                    provPan.Pan = value;
            }
        }
        public float Volume
        {
            get => provVolume.Volume;
            set => provVolume.Volume = value;
        }
        public bool IsActuallyOver => (((Vorbis.Position / (Vorbis.WaveFormat.BitsPerSample / 8))) >= SampleCount - 1 && SampleCount > 0) || EndEarly;

        public bool EndEarly = false;

        public long VorbisSamplePosition
        {
            get => Vorbis.Position / (Vorbis.WaveFormat.BitsPerSample / 8);
            set => Vorbis.Position = value * (Vorbis.WaveFormat.BitsPerSample / 8);
        }

        //public WaveFormat FakeWaveFormat;

        public VorbisLoopingSampleProvider(VorbisWaveReader vorbis, float pitch)
        {
            Vorbis = vorbis;
            if (vorbis.WaveFormat.Channels == 1)
            {
                provPan = new PanningSampleProvider(vorbis);

                provPan.PanStrategy = new LinearPanStrategy();

                provVolume = new VolumeSampleProvider(provPan);
            }
            else
            {
                provVolume = new VolumeSampleProvider(vorbis);
            }
            
            
            SourceSampleRate = vorbis.WaveFormat.SampleRate;

            //FakeWaveFormat = WaveFormat.CreateCustomFormat(provVolume.WaveFormat.Encoding, (int)(SourceSampleRate * (48000 / SourceSampleRate) / (pitch)),
            //    provVolume.WaveFormat.Channels, provVolume.WaveFormat.AverageBytesPerSecond, provVolume.WaveFormat.BlockAlign, provVolume.WaveFormat.BitsPerSample);


        }

        public int Read(float[] buffer, int offset, int count)
        {


            if (EndEarly)
            {
                return 0;
            }


            if (LoopEnabled)
            {
                int samplesUntilLoopEnd = (int)(LoopEnd - VorbisSamplePosition);
                if (samplesUntilLoopEnd > 0 && samplesUntilLoopEnd < count)
                {
                    var numSamplesAfterLoop = count - samplesUntilLoopEnd;
                    int samplesReadUntilLoopEnd = provVolume.Read(buffer, offset, samplesUntilLoopEnd);

                    VorbisSamplePosition = LoopStart;

                    int samplesReadAfterLoopStart = provVolume.Read(buffer, offset + samplesUntilLoopEnd, numSamplesAfterLoop);

                    return samplesReadUntilLoopEnd + samplesReadAfterLoopStart;
                }
                else
                {
                    return provVolume.Read(buffer, offset, count);
                }
            }
            else
            {
                if (IsActuallyOver)
                    return 0;

                //if (((Vorbis.Position / (Vorbis.WaveFormat.BitsPerSample / 8))) >= totalSampleCount - 1 && totalSampleCount > 0)
                //    return 0;
                //else if (Vorbis.Position > 0)
                //    Console.WriteLine("TEST");

                return provVolume.Read(buffer, offset, count);
            }
        }

        public WaveFormat WaveFormat => provVolume.WaveFormat;

        public void Dispose()
        {
            Vorbis?.Dispose();
        }
    }
}
