using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public abstract class NewHavokAnimation
    {
        public HKX.AnimationBlendHint BlendHint = HKX.AnimationBlendHint.NORMAL;

        public readonly NewAnimSkeleton Skeleton;

        public readonly Vector4[] RootMotionFrames = null;

        private Vector4 currentRootMotionVector4 = Vector4.Zero;
        public Matrix CurrentRootMotionMatrix = Matrix.Identity;

        private object _lock_boneMatrixStuff = new object();

        private Dictionary<int, Matrix> boneMatrixCache = new Dictionary<int, Matrix>();
        private List<Matrix> boneMatrices = new List<Matrix>();

        public bool IsAdditiveBlend =>
            BlendHint == HKX.AnimationBlendHint.ADDITIVE ||
            BlendHint == HKX.AnimationBlendHint.ADDITIVE_DEPRECATED;

        public float Duration;
        public float FrameDuration;
        public int FrameCount;

        public bool HasEnded => CurrentTime >= Duration;

        public float CurrentTime;

        public float CurrentFrame => CurrentTime / FrameDuration;

        public abstract Matrix GetBoneMatrixOnCurrentFrame(int hkxBoneIndex);

        public void ApplyMotionToSkeleton()
        {
            WriteCurrentFrameToSkeleton();
            UpdateCurrentRootMotion();
        }

        public void Scrub(float newTime, bool loop, bool forceUpdate = false)
        {
            if (newTime != CurrentTime)
            {
                CurrentTime = newTime;

                if (loop)
                {
                    if (CurrentTime >= Duration)
                    {
                        CurrentTime = CurrentTime % Duration;
                    }
                }
                else
                {
                    if (CurrentTime > (Duration - FrameDuration))
                    {
                        CurrentTime = (Duration - FrameDuration);
                    }
                }  

                ApplyMotionToSkeleton();
            }
            else if (forceUpdate)
            {
                ApplyMotionToSkeleton();
            }
        }

        public void Play(float deltaTime, bool loop, bool forceUpdate = false)
        {
            float oldTime = CurrentTime;

            if (loop)
            {
                CurrentTime += deltaTime;
                CurrentTime = CurrentTime % Duration;
            }
            else
            {
                CurrentTime += deltaTime;
                if (CurrentTime > (Duration - FrameDuration))
                    CurrentTime = (Duration - FrameDuration);
            }

            if (forceUpdate || (oldTime != CurrentTime))
            {
                ApplyMotionToSkeleton();
            }
        }

        private void UpdateCurrentRootMotion()
        {
            if (RootMotionFrames != null)
            {
                float frameFloor = (float)Math.Floor(CurrentFrame % RootMotionFrames.Length);
                currentRootMotionVector4 = RootMotionFrames[(int)frameFloor];

                if (CurrentFrame != frameFloor)
                {
                    float frameMod = CurrentFrame % 1;

                    Vector4 nextFrameRootMotion;
                    if (CurrentFrame >= RootMotionFrames.Length - 1)
                        nextFrameRootMotion = RootMotionFrames[0];
                    else
                        nextFrameRootMotion = RootMotionFrames[(int)(frameFloor + 1)];

                    currentRootMotionVector4.X = MathHelper.Lerp(currentRootMotionVector4.X, nextFrameRootMotion.X, frameMod);
                    currentRootMotionVector4.Y = MathHelper.Lerp(currentRootMotionVector4.Y, nextFrameRootMotion.Y, frameMod);
                    currentRootMotionVector4.Z = MathHelper.Lerp(currentRootMotionVector4.Z, nextFrameRootMotion.Z, frameMod);
                    currentRootMotionVector4.W = MathHelper.Lerp(currentRootMotionVector4.W, nextFrameRootMotion.W, frameMod);
                }
            }
            else
            {
                currentRootMotionVector4 = Vector4.Zero;
            }

            CurrentRootMotionMatrix = Matrix.CreateRotationY(currentRootMotionVector4.W)
                * Matrix.CreateTranslation(currentRootMotionVector4.X, currentRootMotionVector4.Y, currentRootMotionVector4.Z);
        }

        public NewHavokAnimation(NewAnimSkeleton skeleton, HKX.HKADefaultAnimatedReferenceFrame refFrame, HKX.HKAAnimationBinding binding)
        {
            Skeleton = skeleton;
            if (refFrame != null)
            {
                RootMotionFrames = new Vector4[refFrame.ReferenceFrameSamples.Size];
                for (int i = 0; i < refFrame.ReferenceFrameSamples.Size; i++)
                {
                    RootMotionFrames[i] = new Vector4(
                        refFrame.ReferenceFrameSamples[i].Vector.X, 
                        refFrame.ReferenceFrameSamples[i].Vector.Y, 
                        refFrame.ReferenceFrameSamples[i].Vector.Z, 
                        refFrame.ReferenceFrameSamples[i].Vector.W);
                }
            }

            lock (_lock_boneMatrixStuff)
            {
                boneMatrices = new List<Matrix>();
                for (int i = 0; i < skeleton.HkxSkeleton.Count; i++)
                {
                    boneMatrices.Add(Matrix.Identity);
                }
            }
           
            BlendHint = binding.BlendHint;
        }

        public void WriteCurrentFrameToSkeleton()
        {
            lock (_lock_boneMatrixStuff)
            {
                boneMatrixCache.Clear();

                Matrix GetParentTransformedBoneMatrix(int i)
                {
                    Matrix result = Matrix.Identity;

                    do
                    {
                        Matrix thisBone = Matrix.Identity;

                        if (boneMatrixCache.ContainsKey(i))
                        {
                            thisBone *= boneMatrixCache[i];
                        }
                        else
                        {
                            thisBone *= boneMatrices[i];
                            boneMatrixCache.Add(i, thisBone);
                        }

                        result *= thisBone;

                        i = Skeleton.HkxSkeleton[i].ParentIndex;
                    }
                    while (i >= 0);

                    return result;
                }

                for (int i = 0; i < Skeleton.HkxSkeleton.Count; i++)
                {
                    boneMatrices[i] = GetBoneMatrixOnCurrentFrame(i);
                }

                for (int i = 0; i < Skeleton.HkxSkeleton.Count; i++)
                {
                    Skeleton.SetHkxBoneMatrix(i, GetParentTransformedBoneMatrix(i));
                }
            }

        }
    }
}
