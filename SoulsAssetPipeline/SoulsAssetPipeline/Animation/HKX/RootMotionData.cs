using System;
using System.Linq;
using System.Numerics;

namespace SoulsAssetPipeline.Animation
{
    public class RootMotionData
    {
        //public Matrix CurrentAbsoluteRootMotion = Matrix.Identity;

        public readonly Vector4 Up;
        public readonly Vector4 Forward;
        public readonly float Duration;
        public readonly Vector4[] Frames;

        public RootMotionData(HKX.HKADefaultAnimatedReferenceFrame refFrame) : this(refFrame.Up, refFrame.Forward, refFrame.Duration, refFrame.ReferenceFrameSamples.GetArrayData().Elements.Select(hkxVector => hkxVector.Vector).ToArray())
        {
        }

        public RootMotionData(Vector4 up, Vector4 forward, float duration, Vector4[] frames)
        {
            Up = up;
            Forward = forward;
            Duration = duration;
            Frames = frames;
        }


        public Matrix4x4 ConvertSampleToMatrixWithView(Vector4 sample)
        {
            return Matrix4x4.CreateRotationY(sample.W) *
                Matrix4x4.CreateWorld(
                    new Vector3(sample.X, sample.Y, sample.Z),
                    new Vector3(Forward.X, Forward.Y, -Forward.Z),
                    new Vector3(Up.X, Up.Y, Up.Z));
        }

        public Matrix4x4 ConvertSampleToMatrixWithViewNoRotation(Vector4 sample)
        {
            return Matrix4x4.CreateWorld(
                    new Vector3(sample.X, sample.Y, sample.Z),
                    new Vector3(Forward.X, Forward.Y, -Forward.Z),
                    new Vector3(Up.X, Up.Y, Up.Z));
        }

        public Vector4 GetSampleOnExactFrame(int frame)
        {
            if (frame < 0)
                frame = 0;
            int frameDataIndex = frame % (Frames.Length - 1);
            int loopIndex = frame / (Frames.Length - 1);
            return Frames[frameDataIndex] + (Frames[Frames.Length - 1] * loopIndex);
        }

        public Vector4 GetSampleOnFrame(float frame)
        {
            if (frame == (int)frame)
                return GetSampleOnExactFrame((int)frame);

            return Vector4.Lerp(
                GetSampleOnExactFrame((int)Math.Floor(frame)),
                GetSampleOnExactFrame((int)Math.Ceiling(frame)),
                frame % 1.0f);
        }
    }
}
