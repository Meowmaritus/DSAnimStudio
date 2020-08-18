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

        public static NewHavokAnimation Clone(NewHavokAnimation anim)
        {
            if (anim is NewHavokAnimation_SplineCompressed spline)
            {
                return new NewHavokAnimation_SplineCompressed(spline);
            }
            else if (anim is NewHavokAnimation_InterleavedUncompressed interleaved)
            {
                return new NewHavokAnimation_InterleavedUncompressed(interleaved);
            }
            else
            {
                throw new NotImplementedException();
            }
            
        }

        public HKX.AnimationBlendHint BlendHint => data.BlendHint;

        public float Weight = 1.0f;

        public string Name => data.Name;

        public override string ToString()
        {
            return $"{Name} [{Math.Round(1 / FrameDuration)} FPS]";
        }

        public readonly NewAnimSkeleton Skeleton;

        private object _lock_boneMatrixStuff = new object();

        public NewBlendableTransform[] blendableTransforms = new NewBlendableTransform[0];
        private List<int> bonesAlreadyCalculated = new List<int>();

        public bool IsAdditiveBlend => data.IsAdditiveBlend;

        public float Duration => data.Duration;
        public float FrameDuration => data.FrameDuration;
        public int FrameCount => data.FrameCount;

        //public bool HasEnded => CurrentTime >= Duration;

        public float CurrentTime { get; private set; } = 0;
        private float oldTime = 0;

        public Matrix RotMatrixAtStartOfAnim { get; private set; } = Matrix.Identity;

        public void ApplyExternalRotation(float r)
        {
            RotMatrixAtStartOfAnim *= Matrix.CreateRotationY(r);
            var keys = StartingRootMotionsPerLoop.Keys.ToList();
            foreach (var k in keys)
            {
                StartingRootMotionsPerLoop[k] = StartingRootMotionsPerLoop[k] * Matrix.CreateRotationY(r);
            }
        }

        public void Reset()
        {
            CurrentTime = 0;
            oldTime = 0;
            currentRootMotionVector = Vector4.Zero;
            oldRootMotionVector = Vector4.Zero;
            RootMotionDeltaOfLastScrub = Vector4.Zero;

            RotMatrixAtStartOfAnim = ParentContainer.MODEL.CurrentRootMotionRotation;

            StartingRootMotionsPerLoop.Clear();
            StartingRootMotionsPerLoop.Add(0, RotMatrixAtStartOfAnim);
        }

        public float CurrentFrame => CurrentTime / FrameDuration;

        private Vector4 currentRootMotionVector = Vector4.Zero;
        private Vector4 oldRootMotionVector = Vector4.Zero;

        public Vector4 RootMotionDeltaOfLastScrub = Vector4.Zero;

        public NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex)
        {
            return data.GetBlendableTransformOnFrame(hkxBoneIndex, CurrentFrame);
        }

        //public void ApplyWeightedMotionToSkeleton(bool finalizeHkxMatrices, float unusedWeight)
        //{
        //    ApplyCurrentFrameToSkeletonWeighted(finalizeHkxMatrices, unusedWeight);
        //}

        //public List<float> GetLoopTimesInWindow(double start, double end)
        //{

        //}

        private Dictionary<int, Matrix> StartingRootMotionsPerLoop = new Dictionary<int, Matrix>();

        public void ScrubRelative(float timeDelta, bool doNotCheckRootMotionRotation)
        {
            CurrentTime += timeDelta;

            if (CurrentTime < 0)
            {
                var timeBeforeJump = CurrentTime;
                CurrentTime  = 0;
                oldTime += (CurrentTime - timeBeforeJump);

                if (data.RootMotion != null)
                    oldRootMotionVector += data.RootMotion.Frames[data.RootMotion.Frames.Length - 1].ToXna();
            }

            OnScrubAccumulateRootMotion();

            int oldLoopCount = (int)Math.Floor(oldTime / Duration);
            int loopCount = (int)Math.Floor(CurrentTime / Duration);

            if (loopCount != oldLoopCount)
            {
                if (StartingRootMotionsPerLoop.ContainsKey(loopCount))
                {
                    RotMatrixAtStartOfAnim = StartingRootMotionsPerLoop[loopCount];
                }
                else if (!doNotCheckRootMotionRotation)
                {
                    // Go to start of this loop
                    var deltaTimeToGoToStartOfThisLoop = (loopCount * Duration) - CurrentTime;
                    ParentContainer.ScrubRelative(deltaTimeToGoToStartOfThisLoop, doNotCheckRootMotionRotation: true);
                    // Grab the rotation there at start of this loop
                    var newRotMatrix = ParentContainer.MODEL.CurrentRootMotionRotation;
                    // Go back to current time
                    ParentContainer.ScrubRelative(-deltaTimeToGoToStartOfThisLoop, doNotCheckRootMotionRotation: true);
                    // Set the grabbed rotation
                    RotMatrixAtStartOfAnim = newRotMatrix;

                    StartingRootMotionsPerLoop.Add(loopCount, RotMatrixAtStartOfAnim);
                }

                
            }

            oldTime = CurrentTime;
            oldRootMotionVector = currentRootMotionVector;
        }

        //public void Play(float deltaTime, bool loop, bool forceUpdate, bool forceAbsoluteRootMotion)
        //{
        //    //loopCountDeltaThisFrame = 0;

        //    if (!forceAbsoluteRootMotionThisFrame && forceAbsoluteRootMotion)
        //        forceAbsoluteRootMotionThisFrame = forceAbsoluteRootMotion;

        //    float oldTime = CurrentTime;

        //    if (loop)
        //    {
        //        CurrentTime += deltaTime;

        //        if (deltaTime > 0)
        //        {
        //            while (CurrentTime >= Duration)
        //            {
        //                CurrentTime -= Duration;
        //                loopCountThisFrame++;
        //            }
        //        }
        //        else if (deltaTime < 0)
        //        {
        //            while (CurrentTime < 0)
        //            {
        //                CurrentTime += Duration;
        //                loopCountThisFrame--;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        CurrentTime += deltaTime;
        //        if (CurrentTime > (Duration - FrameDuration))
        //            CurrentTime = (Duration - FrameDuration);
        //    }

        //    if (forceUpdate || (oldTime != CurrentTime))
        //    {
        //        ApplyMotionToSkeleton();
        //    }
        //}

        private void OnScrubAccumulateRootMotion()
        {
            if (data.RootMotion != null)
            {
                currentRootMotionVector = data.RootMotion.GetSampleOnFrame(CurrentFrame).ToXna();
                RootMotionDeltaOfLastScrub = currentRootMotionVector - oldRootMotionVector;
            }
            
        }

        protected NewHavokAnimation(HavokAnimationData data, NewAnimSkeleton skeleton, NewAnimationContainer container)
        {
            this.data = data;

            ParentContainer = container;
            Skeleton = skeleton;

            lock (_lock_boneMatrixStuff)
            {
                blendableTransforms = new NewBlendableTransform[skeleton.HkxSkeleton.Count];
            }

            RotMatrixAtStartOfAnim = ParentContainer.MODEL.CurrentRootMotionRotation;
        }

        public void CalculateCurrentFrame()
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
                        //Skeleton.ModHkxBoneMatrix(i, Matrix.CreateScale(currentScale) * currentMatrix, Weight, finalizeHkxMatrices, unusedWeight);
                        bonesAlreadyCalculated.Add(i);
                    }

                    foreach (var c in Skeleton.HkxSkeleton[i].ChildIndices)
                        WalkTree(c, currentMatrix, currentScale);
                }

                foreach (var root in Skeleton.RootBoneIndices)
                    WalkTree(root, Matrix.Identity, Vector3.One);
            }
        }

        //public void ApplyCurrentFrameToSkeletonWeighted()
        //{
            

        //}
    }
}
