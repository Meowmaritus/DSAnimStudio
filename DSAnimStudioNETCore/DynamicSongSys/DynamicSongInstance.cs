using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace DSAnimStudio.DynamicSongSys
{
    public class DynamicSongInstance : IDisposable
    {
        // Properties

        // Public

        public float Volume
        {
            get => dynamicSound.Volume;
            set => dynamicSound.Volume = Math.Min(Math.Max(0, value), 1);
        }

        public float Pitch
        {
            get => dynamicSound.Pitch;
            set => dynamicSound.Pitch = value;
        }

        public float Pan
        {
            get => dynamicSound.Pan;
            set => dynamicSound.Pan = value;
        }

        // Private

        private float originalVolume;
        private DynamicSoundEffectInstance dynamicSound;
        private byte[] byteArray;
        private int position;
        private int count;
        private int loopStartBytes;
        private int loopEndBytes;

        public bool EnableLoop => enableLoop;

        private float bytesOverMilliseconds;

        private bool enableLoop;

        public bool IsFinishedPlaying = false;

        // Methods

        // Public

        public DynamicSongInstance(DynamicSoundEffectInstance dynamicSound, byte[] byteArray, int count, int loopStartBytes, int loopEndBytes, float bytesOverMilliseconds, bool enableLoop)
        {
            this.dynamicSound = dynamicSound;
            this.byteArray = byteArray;
            this.count = count;
            this.loopStartBytes = loopStartBytes;
            this.loopEndBytes = loopEndBytes;
            this.bytesOverMilliseconds = bytesOverMilliseconds;

            this.dynamicSound.BufferNeeded += UpdateBuffer;
            this.enableLoop = enableLoop;
        }

        public void Play()
        {
            //dynamicSound.Pitch = 0;
            //if (MyGame.IsMuted) dynamicSound.Volume = 0;
            dynamicSound.Play();
        }

        public void Pause()
        {
            dynamicSound?.Pause();
        }

        public void Stop()
        {
            if (dynamicSound != null)
            {
                dynamicSound.BufferNeeded -= UpdateBuffer;
                dynamicSound.Stop();
            }
            IsFinishedPlaying = true;
        }

        public void SetPosition(float milliseconds)
        {
            position = (int)Math.Floor(milliseconds * bytesOverMilliseconds);
            while (position % 8 != 0) position -= 1;
        }

        public float GetPosition()
        {
            return position / bytesOverMilliseconds;
        }

        // Private

        private bool firstBuffer = true;
        private void UpdateBuffer(object sender, EventArgs e)
        {
            //New Version
            if (enableLoop && loopStartBytes > 0)
            {
                if (firstBuffer)
                {
                    dynamicSound.SubmitBuffer(byteArray, 0, loopStartBytes);
                    dynamicSound.SubmitBuffer(byteArray, loopStartBytes, loopEndBytes - loopStartBytes);
                }
                else
                {
                    dynamicSound.SubmitBuffer(byteArray, loopStartBytes, loopEndBytes - loopStartBytes);
                    dynamicSound.SubmitBuffer(byteArray, loopStartBytes, loopEndBytes - loopStartBytes);
                }
            }
            else
            {
                if (firstBuffer)
                {
                    dynamicSound.SubmitBuffer(byteArray, 0, byteArray.Length);
                }
                else
                {
                    IsFinishedPlaying = true;
                    Stop();
                }
            }
            firstBuffer = false;
            return;

            // Old Version
            int bytesUntilEnd = byteArray.Length - position;
            int bytesUntilLoop = loopEndBytes - position;

            if (enableLoop && bytesUntilLoop < count * 2)
            {
                int bytesToReadBeforeLoop = bytesUntilLoop;
                int bytesToReadAfterLoop = count * 2 - bytesUntilLoop;
                dynamicSound.SubmitBuffer(byteArray, position, bytesToReadBeforeLoop);
                position = loopStartBytes;
                dynamicSound.SubmitBuffer(byteArray, position, bytesToReadAfterLoop);
                position = position + bytesToReadAfterLoop;
            }
            else
            {
                int bytesToRead = Math.Min(count, bytesUntilEnd);

                if (bytesToRead == 0)
                    return;

                dynamicSound.SubmitBuffer(byteArray, position, bytesToRead);
                position += bytesToRead;

                if (position >= byteArray.Length)
                {
                    IsFinishedPlaying = true;
                    Stop();
                    //position = 0;
                }

                bytesUntilEnd = byteArray.Length - position;

                bytesToRead = Math.Min(count, bytesUntilEnd);

                if (bytesToRead == 0)
                    return;

                dynamicSound.SubmitBuffer(byteArray, position, bytesToRead);
                position += bytesToRead;

                if (position >= byteArray.Length)
                {
                    IsFinishedPlaying = true;
                    Stop();
                    //position = 0;
                }
            }

            
        }

        public void Dispose()
        {
            Stop();
            dynamicSound.Dispose();
        }
    }
}