#region Assembly NAudio.Core, Version=2.1.0.0, Culture=neutral, PublicKeyToken=e279aa5131008a41
// C:\Users\Green\.nuget\packages\naudio.core\2.1.0\lib\netstandard2.0\NAudio.Core.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using NAudio.Dsp;

namespace NAudio.Wave.SampleProviders
{
    public class WdlResamplingSampleProvider_Meme : ISampleProvider
    {
        private readonly WdlResampler_Meme resampler;

        private readonly WaveFormat outFormat;

        private readonly ISampleProvider source;

        private readonly int channels;

        public WaveFormat WaveFormat => outFormat;

        public WdlResamplingSampleProvider_Meme(ISampleProvider source, int ogSampleRate, int newSampleRate, double speedMult)
        {
            channels = source.WaveFormat.Channels;
            outFormat = WaveFormat.CreateIeeeFloatWaveFormat(newSampleRate, channels);
            this.source = source;
            resampler = new WdlResampler_Meme();
            resampler.SetMode(interp: true, 2, sinc: false);
            resampler.SetFilterParms();
            resampler.SetFeedMode(wantInputDriven: false);
            resampler.SetRates(source.WaveFormat.SampleRate, newSampleRate / speedMult);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int num = count / channels;
            float[] inbuffer;
            int inbufferOffset;
            int num2 = resampler.ResamplePrepare(num, outFormat.Channels, out inbuffer, out inbufferOffset);
            int nsamples_in = source.Read(inbuffer, inbufferOffset, num2 * channels) / channels;
            return resampler.ResampleOut(buffer, offset, nsamples_in, num, channels) * channels;
        }
    }
}