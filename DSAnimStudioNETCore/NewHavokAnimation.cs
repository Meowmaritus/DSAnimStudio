using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSAnimStudio.TaeEditor;
using NMatrix = System.Numerics.Matrix4x4;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;


namespace DSAnimStudio
{
    public class NewHavokAnimation
    {
        public HavokAnimationData Data;

        public enum OverlayTypes
        {
            Normal = 0,
            FromTPose = 1,
            BoneMask = 2,
        }
        public static HavokAnimationData ReadAnimationDataFromHkx(zzz_DocumentIns doc, byte[] hkxBytes, SplitAnimID id, string name)
        {
            HKX hkx = null;

            if (doc.GameRoot.GameTypeIsHavokTagfile)
            {
                hkx = HKX.GenFakeFromTagFile(hkxBytes);
            }
            else
            {
                var hkxVariation = doc.GameRoot.GetCurrentLegacyHKXType();

                hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (doc.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                        || doc.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));

                if (hkx == null)
                {
                    hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);


                }
            }

            foreach (var cl in hkx.DataSection.Objects)
            {
                if (cl is HKX.HKASplineCompressedAnimation asSplineCompressedAnim)
                {
                    return new HavokAnimationData_SplineCompressed(id.GetFullID(doc.GameRoot), name, asSplineCompressedAnim);
                }
                else if (cl is HKX.HKAInterleavedUncompressedAnimation asInterleavedUncompressedAnim)
                {
                    return new HavokAnimationData_InterleavedUncompressed(id.GetFullID(doc.GameRoot), name, asInterleavedUncompressedAnim);
                }
            }

            return null;
        }

        //public readonly NewAnimationContainer ParentContainer;

        public static NewHavokAnimation Clone(NewHavokAnimation anim, bool upperBodyAnim)
        {
            NewHavokAnimation result = null;
            if (anim is NewHavokAnimation_SplineCompressed spline)
            {
                result = new NewHavokAnimation_SplineCompressed(spline) 
                    { IsUpperBody = upperBodyAnim };
            }
            else if (anim is NewHavokAnimation_InterleavedUncompressed interleaved)
            {
                result = new NewHavokAnimation_InterleavedUncompressed(interleaved) 
                    { IsUpperBody = upperBodyAnim };
            }
            else
            {
                result = new NewHavokAnimation(anim.Data, anim.FileSize)
                    { IsUpperBody = upperBodyAnim };
            }

            result.CurrentTime = 0;
            result.LoopCount = 0;

            return result;
        }

        public HKX.AnimationBlendHint BlendHint => Data.BlendHint;

        private float _weight;
        public float Weight
        {
            get => _weight;
            set
            {
                // if (float.IsInfinity(value))
                //     Console.WriteLine("breakpoint hit");
                _weight = value;
            }
        }

        /// <summary>
        /// Used when blending multiple animations.
        /// The weight ratio used for previous animations when blending to the next one.
        /// </summary>
        public float ReferenceWeight = 0;

        public SplitAnimID GetID(DSAProj proj)
        {
            return SplitAnimID.FromFullID(proj, Data.ID);
        }
        
        public string Name => Data.Name;

        public override string ToString()
        {
            return $"{Name} [{Math.Round(1 / FrameDuration)} FPS]";
        }

        //public readonly NewAnimSkeleton_HKX Skeleton;

        private object _lock_boneMatrixStuff = new object();

        public DSAProj.Animation TaeAnimation;

        public bool IsAdditiveBlend => Data.IsAdditiveBlend;

        public bool IsUpperBody { get; set; } = false;

        public float Duration => Data.Duration;
        public float FrameDuration => Data.FrameDuration;
        public int FrameCount => Data.FrameCount;

        // public bool TaeEnabled = false;
        // private bool TaeNeedsInit = true;
        // public TaeActionSimulationEnvironment TaeActionSim;
        // public DSAProj.Animation TaeAnimation;

        //public bool HasEnded => CurrentTime >= Duration;

        private float _currentTime;
        public float CurrentTime
        {
            get => _currentTime;
            set
            {
                //if (value < 1)
                //    Console.WriteLine("breakpoint hit");
                // if (_currentTime == 0 && value > 0 && Main.IsDebugBuild)
                //     Console.WriteLine("breakpoint");
                
                _currentTime = value;
            }
        }

        public float TaePrevTime;
        public int TaePrevLoopCount;

        public bool TaePrevFramePlaybackCursorMoving;

        public bool TaeScrubHasReactivatedActionsAlready;

        public float TaePrevTimeUnlooped => TaePrevTime + (LoopCount * Duration);
        
        public RootMotionDataPlayer RootMotion { get; private set; }

        public NVector4 RootMotionTransformLastFrame;
        public NVector4 RootMotionTransformDelta;

        //public float ExternalRotation { get; private set; }

        public bool EnableLooping;

        public int LoopCount;

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
            TaePrevTime = 0;
            LoopCount = 0;
            TaePrevLoopCount = 0;
            RootMotion.SyncTimeAndLocation(startRootMotionTransform, startTime: 0);
        }

        public float CurrentFrame
        {
            get => CurrentTime / FrameDuration;
            set => CurrentTime = FrameDuration * value;
        }


        public NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex)
        {
            return Data.GetTransformOnFrameByBone(hkxBoneIndex, CurrentFrame, EnableLooping);
        }
        
        public NewBlendableTransform GetBlendableTransformOnSpecificFrame(int hkxBoneIndex, float frame)
        {
            return Data.GetTransformOnFrameByBone(hkxBoneIndex, frame, EnableLooping);
        }


        public float CurrentTimeUnlooped => CurrentTime + (Duration * LoopCount);

        
        public void Scrub(bool absolute, float time, out float timeDelta, out float? syncTime, bool ignoreRootMotion)
        {
            syncTime = null;
            if (absolute)
            {
                if (time < 0)
                    time = 0;
                
                var oldUnloopedTime = CurrentTimeUnlooped;

                if (EnableLooping)
                {
                    CurrentTime = time % Duration;
                    LoopCount = (int)(time / Duration);
                    if (!ignoreRootMotion)
                        RootMotion.SyncTimeAndLocation(RootMotion.CurrentTransform, CurrentTime);
                }
                else
                {
                    CurrentTime = time;
                    if (CurrentTime > Duration)
                    {
                        CurrentTime = Duration;
                        syncTime = CurrentTimeUnlooped;
                    }
                    if (CurrentTime < 0)
                    {
                        CurrentTime = 0;
                        syncTime = CurrentTimeUnlooped;
                    }
                    LoopCount = 0;
                    if (!ignoreRootMotion)
                        RootMotion.SyncTimeAndLocation(RootMotion.CurrentTransform, CurrentTime);
                }
                
                
                timeDelta = CurrentTimeUnlooped - oldUnloopedTime;
            }
            else
            {
                timeDelta = time;
                CurrentTime += time;
                
                if (EnableLooping)
                {
                    if (CurrentTime >= Duration)
                    {
                        if (!ignoreRootMotion)
                            RootMotion.SetTime(CurrentTime);

                        CurrentTime -= Duration;
                        TaePrevTime -= Duration;

                        if (!ignoreRootMotion)
                            RootMotion.SyncTimeAndLocation(RootMotion.CurrentTransform, CurrentTime);
                        LoopCount++;
                        syncTime = CurrentTimeUnlooped;
                    }
                    if (CurrentTime < 0)
                    {
                        if (LoopCount > 0)
                        {
                            CurrentTime += Duration;
                            TaePrevTime += Duration;
                            if (!ignoreRootMotion)
                                RootMotion.SyncTimeAndLocation(RootMotion.CurrentTransform, CurrentTime);
                            LoopCount--;
                            syncTime = CurrentTimeUnlooped;
                        }
                        else
                        {
                            var curTime = CurrentTime;
                            CurrentTime = 0;
                            var timeShift = CurrentTime - curTime;
                            TaePrevTime += timeShift;
                            if (!ignoreRootMotion)
                                RootMotion.SyncTimeAndLocation(RootMotion.CurrentTransform, CurrentTime);
                            LoopCount = 0;
                            syncTime = CurrentTimeUnlooped;
                        }
                    }

                    if (!ignoreRootMotion)
                        RootMotion.SetTime(CurrentTime);
                }
                else
                {
                    LoopCount = 0;
                    if (CurrentTime > Duration)
                    {
                        CurrentTime = Duration;
                        syncTime = CurrentTimeUnlooped;
                    }
                    else if (CurrentTime < 0)
                    {
                        CurrentTime = 0;
                        syncTime = CurrentTimeUnlooped;
                    }

                    if (!ignoreRootMotion)
                        RootMotion.SetTime(CurrentTime);
                }
                
                
                
            }

            if (ignoreRootMotion)
            {
                RootMotion.SyncTimeAndLocation(RootMotion.CurrentTransform, CurrentTime);
            }

        }

        // public void SetTime(float time)
        // {
        //     CurrentTimeUnlooped = time;
        //     if (!EnableLooping)
        //         time = MathHelper.Clamp(time, 0, Duration);
        //     float prevTime = CurrentTime;
        //     CurrentTime = time;
        //     if (!CurrentTime.ApproxEquals(prevTime))
        //     {
        //         RootMotion.SetTime(CurrentTime);
        //     }
        //     
        //     prevFrameTime = CurrentTime;
        // }

        public readonly int FileSize;

        public NewHavokAnimation(HavokAnimationData data, int fileSize)
        {
            this.Data = data;

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
