using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DSAnimStudio
{
    public class NewHavokAnimation
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
                return new NewHavokAnimation(anim.data, anim.Skeleton, anim.ParentContainer);
            }
            
        }

        public HKX.AnimationBlendHint BlendHint => data.BlendHint;

        public float Weight = 1.0f;

        /// <summary>
        /// Used when blending multiple animations.
        /// The weight ratio used for previous animations when blending to the next one.
        /// </summary>
        public float ReferenceWeight = 1.0f;

        public string Name => data.Name;

        public override string ToString()
        {
            return $"{Name} [{Math.Round(1 / FrameDuration)} FPS]";
        }

        public readonly NewAnimSkeleton_HKX Skeleton;

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

        public RootMotionDataPlayer RootMotion { get; private set; }

        public float ExternalRotation { get; private set; }

        public void ApplyExternalRotation(float r)
        {
            ExternalRotation += r;
        }

        public void Reset(System.Numerics.Vector4 startRootMotionTransform)
        {
            CurrentTime = 0;
            oldTime = 0;
            RootMotion.ResetToStart(startRootMotionTransform);
        }

        public float CurrentFrame => CurrentTime / FrameDuration;


        public NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex)
        {
            return data.GetTransformOnFrameByBone(hkxBoneIndex, CurrentFrame);
        }

        public void ScrubRelative(float timeDelta)
        {
            CurrentTime += timeDelta;
            RootMotion.SetTime(CurrentTime);
            oldTime = CurrentTime;
        }

        public NewHavokAnimation(HavokAnimationData data, NewAnimSkeleton_HKX skeleton, NewAnimationContainer container)
        {
            this.data = data;

            ParentContainer = container;
            Skeleton = skeleton;

            lock (_lock_boneMatrixStuff)
            {
                blendableTransforms = new NewBlendableTransform[skeleton.HkxSkeleton.Count];
            }

            RootMotion = new RootMotionDataPlayer(data.RootMotion);
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

                foreach (var root in Skeleton.TopLevelHkxBoneIndices)
                    WalkTree(root, Matrix.Identity, Vector3.One);
            }
        }

        //public void ApplyCurrentFrameToSkeletonWeighted()
        //{
            

        //}
    }
}
