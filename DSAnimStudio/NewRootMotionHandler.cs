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

        private RootMotionData data;

        public NewRootMotionHandler(RootMotionData data)
        {
            this.data = data;
        }

        public NewRootMotionHandler(Vector4 up, Vector4 forward, float duration, Vector4[] frames) : this(new RootMotionData(up.ToCS(), forward.ToCS(), duration, frames.Select(XnaExtensions.ToCS).ToArray()))
        {
        }

        public bool Accumulate;

        public void Reset(float frame)
        {
            lastFrame = frame;
        }

        private float lastFrame;

        public (Vector4 Motion, float Direction) UpdateRootMotion(Vector4 currentRootMotion, float currentDirection, float currentFrame, int loopCountDelta, bool forceAbsoluteRootMotion)
        {
            if (forceAbsoluteRootMotion)
            {
                return (currentRootMotion, currentDirection);
            }

            float lastFrameToUse = Accumulate ? lastFrame : 0;

            float lastTimeToUse = Duration * lastFrameToUse / Frames.Length;
            float currentTime = Duration* currentFrame / Frames.Length;

            float nextTimeToUse = currentTime + Duration * loopCountDelta;

            lastFrame = currentFrame;

            var rootMotionChange = data.ExtractRootMotion(lastTimeToUse, nextTimeToUse);

            if (Accumulate)
            {
                return (currentRootMotion + new Vector4(rootMotionChange.positionChange.ToXna(), 0), currentDirection + rootMotionChange.directionChange);
            }

            return (new Vector4(rootMotionChange.positionChange.ToXna(), 0), rootMotionChange.directionChange);

        }
    }
}
