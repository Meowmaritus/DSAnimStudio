using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class FrameCounter
    {
        public FrameCounter()
        {
        }

        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }

        public int MAXIMUM_SAMPLES => GFX.Display.AverageFPSSampleSize;

        private Queue<float> _sampleBuffer = new Queue<float>();

        private int maxSamplesLastFrame = -1;

        public bool Update(float deltaTime)
        {
            int maxSamples = MAXIMUM_SAMPLES;
            if (maxSamplesLastFrame != maxSamples)
                _sampleBuffer.Clear();

            CurrentFramesPerSecond = 1.0f / deltaTime;

            _sampleBuffer.Enqueue(CurrentFramesPerSecond);

            if (_sampleBuffer.Count > maxSamples)
            {
                _sampleBuffer.Dequeue();
                AverageFramesPerSecond = _sampleBuffer.Average(i => i);
            }
            else
            {
                AverageFramesPerSecond = CurrentFramesPerSecond;
            }

            TotalFrames++;
            TotalSeconds += deltaTime;

            maxSamplesLastFrame = maxSamples;

            return true;
        }
    }
}
