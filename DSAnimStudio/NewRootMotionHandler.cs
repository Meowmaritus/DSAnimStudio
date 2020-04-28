using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats.Havok;
namespace DSAnimStudio
{
    public class NewRootMotionHandler
    {
        //public Matrix CurrentAbsoluteRootMotion = Matrix.Identity;
        private Vector4 prevFrameData;


        public Vector4 Up => data.Up.ToXna();
        public Vector4 Forward => data.Forward.ToXna();
        public float Duration => data.Duration;
        public Vector4[] Frames => data.Frames.Select(XnaExtensions.ToXna).ToArray();

        /// <summary>
        /// The accumulative root motion delta applied by playing the entire anim from the beginning to the end.
        /// </summary>
        public Vector4 LoopDeltaForward => data.LoopDeltaForward.ToXna();

        /// <summary>
        /// The accumulative root motion delta applied by playing the entire anim in reverse from the end to the beginning.
        /// </summary>
        public Vector4 LoopDeltaBackward => data.LoopDeltaBackward.ToXna();

        private NewRootMotionHandlerData data;

        public NewRootMotionHandler(NewRootMotionHandlerData data)
        {
            this.data = data;
        }

        public NewRootMotionHandler(Vector4 up, Vector4 forward, float duration, Vector4[] frames) : this(new NewRootMotionHandlerData(up.ToCS(), forward.ToCS(), duration, frames.Select(XnaExtensions.ToCS).ToArray()))
        {
        }

        // TODO: Move this value's behavior to NewRootMotionHandler
        public bool Accumulate { get { return data.Accumulate; } set { data.Accumulate = value; } }

        // TODO: Move this to data
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

        public (Vector4 Motion, float Direction) UpdateRootMotion(Vector4 currentRootMotion, float currentDirection, float currentFrame, int loopCountDelta, bool forceAbsoluteRootMotion)
        {
            var returned = data.UpdateRootMotion(currentRootMotion.ToCS(), prevFrameData.ToCS(), currentDirection, currentFrame, loopCountDelta, forceAbsoluteRootMotion);

            // TODO: this code is left from UpdateRootMotion implementation
            // that was moved into SoulsFormats; maybe remove it
            if (!forceAbsoluteRootMotion)
            {
                prevFrameData = GetSample(currentFrame);
            }

            return (returned.Motion.ToXna(), returned.Direction);

        }
    }
}
