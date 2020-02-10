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
        public readonly NewAnimationContainer ParentContainer;

        public HKX.AnimationBlendHint BlendHint = HKX.AnimationBlendHint.NORMAL;

        public string Name;

        public override string ToString()
        {
            return $"{Name} [{Math.Round(1 / FrameDuration)} FPS]";
        }

        public readonly NewAnimSkeleton Skeleton;

        public NewRootMotionHandler RootMotion = null;

        private object _lock_boneMatrixStuff = new object();

        private List<NewBlendableTransform> blendableTransforms = new List<NewBlendableTransform>();
        private List<int> bonesAlreadyCalculated = new List<int>();

        public bool IsAdditiveBlend =>
            BlendHint == HKX.AnimationBlendHint.ADDITIVE ||
            BlendHint == HKX.AnimationBlendHint.ADDITIVE_DEPRECATED;

        public float Duration;
        public float FrameDuration;
        public int FrameCount;

        public bool HasEnded => CurrentTime >= Duration;

        public float CurrentTime;

        private int loopCountThisFrame = 0;
        private int prevLoopCount = 0;
        private bool forceAbsoluteRootMotionThisFrame = false;

        public float CurrentFrame => CurrentTime / FrameDuration;

        public abstract NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex);

        public void ApplyMotionToSkeleton()
        {
            WriteCurrentFrameToSkeleton();
            UpdateCurrentRootMotion();
        }

        public void Scrub(float newTime, bool loop, bool forceUpdate, int loopCount, bool forceAbsoluteRootMotion)
        {
            if (!forceAbsoluteRootMotionThisFrame && forceAbsoluteRootMotion)
                forceAbsoluteRootMotionThisFrame = forceAbsoluteRootMotion;

            loopCountThisFrame = loopCount;

            if (newTime != CurrentTime)
            {
                float deltaTime = newTime - CurrentTime;
                CurrentTime = newTime;

                if (loop)
                {
                    if (deltaTime > 0)
                    {
                        while (CurrentTime >= Duration)
                        {
                            CurrentTime -= Duration;
                            //loopCountDeltaThisFrame++;
                            //loopCountThisFrame++;
                        }
                    }
                    else if (deltaTime < 0)
                    {
                        while (CurrentTime < 0)
                        {
                            CurrentTime += Duration;
                            //loopCountDeltaThisFrame--;
                            //loopCountThisFrame--;
                        }
                    }
                }
                //else
                //{
                //    if (CurrentTime > (Duration - FrameDuration))
                //    {
                //        CurrentTime = (Duration - FrameDuration);
                //    }
                //}  

                ApplyMotionToSkeleton();
            }
            else if (forceUpdate)
            {
                ApplyMotionToSkeleton();
            }
        }

        public void Play(float deltaTime, bool loop, bool forceUpdate, bool forceAbsoluteRootMotion)
        {
            //loopCountDeltaThisFrame = 0;

            if (!forceAbsoluteRootMotionThisFrame && forceAbsoluteRootMotion)
                forceAbsoluteRootMotionThisFrame = forceAbsoluteRootMotion;

            float oldTime = CurrentTime;

            if (loop)
            {
                CurrentTime += deltaTime;

                if (deltaTime > 0)
                {
                    while (CurrentTime >= Duration)
                    {
                        CurrentTime -= Duration;
                        loopCountThisFrame++;
                    }
                }
                else if (deltaTime < 0)
                {
                    while (CurrentTime < 0)
                    {
                        CurrentTime += Duration;
                        loopCountThisFrame--;
                    }
                }
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
            int loopCountDelta = loopCountThisFrame - prevLoopCount;

            if (RootMotion != null)
            {
                (Vector4 motion, float direction) rootMotionState = RootMotion.UpdateRootMotion(ParentContainer.CurrentRootMotionVector,
                    ParentContainer.CurrentRootMotionDirection, CurrentFrame, loopCountDelta,
                    forceAbsoluteRootMotionThisFrame);

                ParentContainer.CurrentRootMotionVector = rootMotionState.motion;
                ParentContainer.CurrentRootMotionDirection = rootMotionState.direction;

                forceAbsoluteRootMotionThisFrame = false;
                prevLoopCount = loopCountThisFrame;
            }
            else
            {
                ParentContainer.CurrentRootMotionDirection = 0;
                ParentContainer.CurrentRootMotionVector = Vector4.Zero;
            }


           

            
        }

        public NewHavokAnimation(NewAnimSkeleton skeleton, HKX.HKADefaultAnimatedReferenceFrame refFrame, 
            HKX.HKAAnimationBinding binding, NewAnimationContainer container)
        {
            ParentContainer = container;
            Skeleton = skeleton;
            if (refFrame != null)
            {
                var rootMotionFrames = new Vector4[refFrame.ReferenceFrameSamples.Size];
                for (int i = 0; i < refFrame.ReferenceFrameSamples.Size; i++)
                {
                    rootMotionFrames[i] = new Vector4(
                        refFrame.ReferenceFrameSamples[i].Vector.X, 
                        refFrame.ReferenceFrameSamples[i].Vector.Y, 
                        refFrame.ReferenceFrameSamples[i].Vector.Z, 
                        refFrame.ReferenceFrameSamples[i].Vector.W);
                }
                var rootMotionUp = new Vector4(refFrame.Up.X, refFrame.Up.Y, refFrame.Up.Z, refFrame.Up.W);
                var rootMotionForward = new Vector4(refFrame.Forward.X, refFrame.Forward.Y, refFrame.Forward.Z, refFrame.Forward.W);

                RootMotion = new NewRootMotionHandler(rootMotionUp, rootMotionForward, refFrame.Duration, rootMotionFrames);
            }

            lock (_lock_boneMatrixStuff)
            {
                blendableTransforms = new List<NewBlendableTransform>();
                for (int i = 0; i < skeleton.HkxSkeleton.Count; i++)
                {
                    blendableTransforms.Add(NewBlendableTransform.Identity);
                }
            }

            BlendHint = binding.BlendHint;
        }

        public void WriteCurrentFrameToSkeleton()
        {
            bonesAlreadyCalculated.Clear();

            lock (_lock_boneMatrixStuff)
            {
                void WalkTree(int i, Matrix currentMatrix, Vector3 currentScale)
                {
                    if (!bonesAlreadyCalculated.Contains(i))
                    {
                        blendableTransforms[i] = GetBlendableTransformOnCurrentFrame(i);
                        currentMatrix = blendableTransforms[i].GetMatrix() * currentMatrix;
                        currentScale *= blendableTransforms[i].Scale;
                        Skeleton.SetHkxBoneMatrix(i, Matrix.CreateScale(currentScale) * currentMatrix);
                        bonesAlreadyCalculated.Add(i);
                    }

                    foreach (var c in Skeleton.HkxSkeleton[i].ChildIndices)
                        WalkTree(c, currentMatrix, currentScale);
                }

                foreach (var root in Skeleton.RootBoneIndices)
                    WalkTree(root, Matrix.Identity, Vector3.One);
            }

        }
    }
}
