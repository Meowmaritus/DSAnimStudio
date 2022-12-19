using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMatrix = System.Numerics.Matrix4x4;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;


namespace DSAnimStudio
{
    public class NewHavokAnimation
    {
        public HavokAnimationData data;

        public struct AnimOverlayRequest
        {
            public float Weight;
            public bool LoopEnabled;
            public bool IsDS1PlayAtMaxWeightOneShotUntilEnd;
            public float NF_RequestedLerpS;
            public float NF_EvInputLerpS;
            public float NF_WeightAtEvStart;
        }

        public AnimOverlayRequest? OverlayRequest = null;

        public static HavokAnimationData ReadAnimationDataFromHkx(byte[] hkxBytes, string name)
        {
            HKX hkx = null;

            if (GameRoot.GameTypeIsHavokTagfile)
            {
                hkx = HKX.GenFakeFromTagFile(hkxBytes);
            }
            else
            {
                var hkxVariation = GameRoot.GetCurrentLegacyHKXType();

                hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                        || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));

                if (hkx == null)
                {
                    hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);


                }
            }

            foreach (var havokClass in hkx.DataSection.Objects)
            {
                foreach (var cl in hkx.DataSection.Objects)
                {
                    if (cl is HKX.HKASplineCompressedAnimation asSplineCompressedAnim)
                    {
                        return new HavokAnimationData_SplineCompressed(name, asSplineCompressedAnim);
                    }
                    else if (cl is HKX.HKAInterleavedUncompressedAnimation asInterleavedUncompressedAnim)
                    {
                        return new HavokAnimationData_InterleavedUncompressed(name, asInterleavedUncompressedAnim);
                    }
                }
            }

            return null;
        }

        //public readonly NewAnimationContainer ParentContainer;

        public static NewHavokAnimation Clone(NewHavokAnimation anim, bool upperBodyAnim)
        {
            if (anim is NewHavokAnimation_SplineCompressed spline)
            {
                return new NewHavokAnimation_SplineCompressed(spline) { IsUpperBody = upperBodyAnim };
            }
            else if (anim is NewHavokAnimation_InterleavedUncompressed interleaved)
            {
                return new NewHavokAnimation_InterleavedUncompressed(interleaved) { IsUpperBody = upperBodyAnim };
            }
            else
            {
                return new NewHavokAnimation(anim.data, anim.FileSize)
                    { IsUpperBody = upperBodyAnim };
            }
            
        }

        public HKX.AnimationBlendHint BlendHint => data.BlendHint;

        private float _weight;
        public float Weight
        {
            get => _weight;
            set
            {
                //if (value > 0.1f)
                //    Console.WriteLine("breakpoint hit");
                _weight = value;
            }
        }

        /// <summary>
        /// Used when blending multiple animations.
        /// The weight ratio used for previous animations when blending to the next one.
        /// </summary>
        public float ReferenceWeight = 0;

        public string Name => data.Name;

        public override string ToString()
        {
            return $"{Name} [{Math.Round(1 / FrameDuration)} FPS]";
        }

        //public readonly NewAnimSkeleton_HKX Skeleton;

        private object _lock_boneMatrixStuff = new object();

        

        public bool IsAdditiveBlend => data.IsAdditiveBlend;

        public bool IsUpperBody { get; set; } = false;

        public float Duration => data.Duration;
        public float FrameDuration => data.FrameDuration;
        public int FrameCount => data.FrameCount;

        //public bool HasEnded => CurrentTime >= Duration;

        private float _currentTime;
        public float CurrentTime
        {
            get => _currentTime;
            set
            {
                //if (value < 1)
                //    Console.WriteLine("breakpoint hit");
                _currentTime = value;
            }
        }
        private float oldTime = 0;

        public RootMotionDataPlayer RootMotion { get; private set; }

        public NVector4 RootMotionTransformLastFrame;
        public NVector4 RootMotionTransformDelta;

        //public float ExternalRotation { get; private set; }

        public bool EnableLooping;

        public void ApplyExternalRotation(float r)
        {
            RootMotion.ApplyExternalTransform(r, System.Numerics.Vector3.Zero);
            RootMotion.SetTime(CurrentTime);
        }

        public void SyncRootMotion(System.Numerics.Vector4 startRootMotionTransform)
        {
            RootMotion.SyncTimeAndLocation(startRootMotionTransform, startTime: CurrentTime);
        }

        public void Reset(System.Numerics.Vector4 startRootMotionTransform)
        {
            CurrentTime = 0;
            oldTime = 0;
            RootMotion.SyncTimeAndLocation(startRootMotionTransform, startTime: 0);
        }

        public float CurrentFrame
        {
            get => CurrentTime / FrameDuration;
            set => CurrentTime = FrameDuration * value;
        }


        public NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex)
        {
            return data.GetTransformOnFrameByBone(hkxBoneIndex, CurrentFrame, EnableLooping);
        }

        public void ScrubRelative(float timeDelta)
        {
            CurrentTime += timeDelta;
            if (!EnableLooping && CurrentTime > Duration)
                CurrentTime = Duration;

            if (timeDelta != 0)
            {
                RootMotion.SetTime(CurrentTime);
            }
            oldTime = CurrentTime;
        }

        public readonly int FileSize;

        public NewHavokAnimation(HavokAnimationData data, int fileSize)
        {
            this.data = data;

            //ParentContainer = container;
            //Skeleton = skeleton;

            RootMotion = new RootMotionDataPlayer(data.RootMotion);

            FileSize = fileSize;
        }

        

        //public void ApplyCurrentFrameToSkeletonWeighted()
        //{
            

        //}
    }
}
