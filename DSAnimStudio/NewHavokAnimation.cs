using Microsoft.Xna.Framework;
using SoulsFormats;
using SFAnimExtensions;
using SFAnimExtensions.Havok;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DSAnimStudio
{
    public abstract class NewHavokAnimation
    {
        public HavokAnimationData data;

        public readonly NewAnimationContainer ParentContainer;

        public HKX.AnimationBlendHint BlendHint => data.BlendHint;

        public string Name => data.Name;

        public override string ToString()
        {
            return $"{Name} [{Math.Round(1 / FrameDuration)} FPS]";
        }

        public readonly NewAnimSkeleton Skeleton;

        public NewRootMotionHandler RootMotion = null;

        private object _lock_boneMatrixStuff = new object();

        private List<NewBlendableTransform> blendableTransforms = new List<NewBlendableTransform>();
        private List<int> bonesAlreadyCalculated = new List<int>();

        public bool IsAdditiveBlend => data.IsAdditiveBlend;

        public float Duration => data.Duration;
        public float FrameDuration => data.FrameDuration;
        public int FrameCount => data.FrameCount;

        public bool HasEnded => CurrentTime >= Duration;

        public float CurrentTime;

        private int loopCountThisFrame = 0;
        private int prevLoopCount = 0;
        private bool forceAbsoluteRootMotionThisFrame = false;

        public float CurrentFrame => CurrentTime / FrameDuration;

        public NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex)
        {
            return data.GetBlendableTransformOnFrame(hkxBoneIndex, CurrentFrame);
        }

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
                    ParentContainer.CurrentRootMotionDirection, CurrentFrame, /*FrameCount + ((this is NewHavokAnimation_InterleavedUncompressed) ? 1 : 0),*/ loopCountDelta,
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

        protected NewHavokAnimation(HavokAnimationData data, NewAnimSkeleton skeleton, NewAnimationContainer container)
        {
            this.data = data;

            ParentContainer = container;
            Skeleton = skeleton;

            if (data.RootMotion != null)
            {
                RootMotion = new NewRootMotionHandler(data.RootMotion);
            }

            lock (_lock_boneMatrixStuff)
            {
                blendableTransforms = new List<NewBlendableTransform>();
                for (int i = 0; i < skeleton.HkxSkeleton.Count; i++)
                {
                    blendableTransforms.Add(NewBlendableTransform.Identity);
                }
            }
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
                        currentMatrix = blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                        currentScale *= blendableTransforms[i].Scale.ToXna();
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
