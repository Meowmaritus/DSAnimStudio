using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewRootMotionHandler
    {
        //public Matrix CurrentAbsoluteRootMotion = Matrix.Identity;
        private Vector4 prevFrameData;


        public readonly Vector4 Up;
        public readonly Vector4 Forward;
        public readonly float Duration;
        public readonly Vector4[] Frames;

        /// <summary>
        /// The accumulative root motion delta applied by playing the entire anim from the beginning to the end.
        /// </summary>
        public readonly Vector4 LoopDeltaForward;

        /// <summary>
        /// The accumulative root motion delta applied by playing the entire anim in reverse from the end to the beginning.
        /// </summary>
        public readonly Vector4 LoopDeltaBackward;


        public NewRootMotionHandler(Vector4 up, Vector4 forward, float duration, Vector4[] frames)
        {
            Up = up;
            Forward = forward;
            Duration = duration;
            Frames = frames;

            LoopDeltaForward = frames[frames.Length - 1] - frames[0];
            LoopDeltaBackward = frames[0] - frames[frames.Length - 1];
        }


        public bool Accumulate = true;

        private Matrix GetMatrixFromSample(Vector4 sample)
        {
            return Matrix.CreateRotationY(sample.W) *
                Matrix.CreateWorld(
                    new Vector3(sample.X, sample.Y, sample.Z),
                    new Vector3(Forward.X, Forward.Y, -Forward.Z),
                    new Vector3(Up.X, Up.Y, Up.Z));
        }

        private Vector4 GetSample(float frame)
        {
            float frameFloor = (float)Math.Floor(frame % (Frames.Length - 1));
            Vector4 sample = Frames[(int)frameFloor];

            if (frame != frameFloor)
            {
                float frameMod = frame % 1;

                Vector4 nextFrameRootMotion;

                //if (frame >= Frames.Length - 1)
                //    nextFrameRootMotion = Frames[0];
                //else
                //    nextFrameRootMotion = Frames[(int)(frameFloor + 1)];

                nextFrameRootMotion = Frames[(int)(frameFloor + 1)];

                sample.X = MathHelper.Lerp(sample.X, nextFrameRootMotion.X, frameMod);
                sample.Y = MathHelper.Lerp(sample.Y, nextFrameRootMotion.Y, frameMod);
                sample.Z = MathHelper.Lerp(sample.Z, nextFrameRootMotion.Z, frameMod);
                sample.W = MathHelper.Lerp(sample.W, nextFrameRootMotion.W, frameMod);
            }

            return sample;
        }

        public void Reset(float frame)
        {
            prevFrameData = GetSample(frame);
        }

        private static Vector4 AddRootMotion(Vector4 start, Vector4 toAdd, float direction, bool dontAddRotation = false)
        {
            if (!dontAddRotation)
                start.W += toAdd.W;

            Vector3 displacement = Vector3.Transform(new Vector3(toAdd.X, toAdd.Y, toAdd.Z), Matrix.CreateRotationY(direction));
            start.X += displacement.X;
            start.Y += displacement.Y;
            start.Z += displacement.Z;
            return start;
        }

        public (Vector4 Motion, float Direction) UpdateRootMotion(Vector4 currentRootMotion, float currentDirection, float currentFrame, int loopCountDelta, bool forceAbsoluteRootMotion)
        {
            if (forceAbsoluteRootMotion)
                return (currentRootMotion, currentDirection);

            var nextFrameData = GetSample(currentFrame);

            if (Accumulate)
            {
                //currentRootMotion *= GetMatrixFromSample(nextFrameData - prevFrameData);

                currentRootMotion = AddRootMotion(currentRootMotion, nextFrameData - prevFrameData, currentDirection, dontAddRotation: true);

                currentRootMotion.W = nextFrameData.W;

                while (loopCountDelta != 0)
                {
                    if (loopCountDelta > 0)
                    {
                        currentRootMotion = AddRootMotion(currentRootMotion, LoopDeltaForward, currentDirection, dontAddRotation: true);
                        currentDirection += LoopDeltaForward.W;
                        loopCountDelta--;
                    }
                    else if (loopCountDelta < 0)
                    {
                        currentRootMotion = AddRootMotion(currentRootMotion, LoopDeltaBackward, currentDirection, dontAddRotation: true);
                        currentDirection += LoopDeltaBackward.W;
                        loopCountDelta++;
                    }
                }
            }
            else
            {
                currentDirection = 0;
                currentRootMotion = AddRootMotion(Vector4.Zero, nextFrameData, currentDirection); 
            }

            prevFrameData = nextFrameData;

            //CurrentAbsoluteRootMotion = GetMatrixFromSample(GetSample(currentFrame));

            //if (forceAbsoluteRootMotion)
            //{

            //}
            //else
            //{
            //    CurrentAbsoluteRootMotion *= GetMatrixFromSample(nextFrameData - prevFrameData);
            //}



            return (currentRootMotion, currentDirection);
        }
    }
}
