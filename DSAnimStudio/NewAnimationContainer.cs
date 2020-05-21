using Microsoft.Xna.Framework;
using SoulsFormats;
using SFAnimExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFAnimExtensions.Havok;

namespace DSAnimStudio
{
    public class NewAnimationContainer
    {
        public readonly Model MODEL;

        private object _lock_animDict = new object();

        private object _lock_timeactDict = new object();

        private Dictionary<string, byte[]> timeactFiles = new Dictionary<string, byte[]>();

        public bool EnableRootMotion = true;
        public bool EnableRootMotionWrap = true;

        public bool IsAnimLoaded(string name)
        {
            return AnimationCache.ContainsKey(name);
        }

        public IReadOnlyDictionary<string, byte[]> TimeActFiles
        {
            get
            {
                IReadOnlyDictionary<string, byte[]> result = null;
                lock (_lock_timeactDict)
                {
                    result = timeactFiles;
                }
                return result;
            }
        }


        private Dictionary<string, byte[]> animHKXsToLoad = new Dictionary<string, byte[]>();

        public IReadOnlyDictionary<string, byte[]> Animations => animHKXsToLoad;

        public List<NewHavokAnimation> AnimationLayers = new List<NewHavokAnimation>();

        private Dictionary<string, NewHavokAnimation> AnimationCache = new Dictionary<string, NewHavokAnimation>();

        public void ClearAnimationCache()
        {
            AnimationCache.Clear();
        }

        public int GetAnimLayerIndexByName(string name)
        {
            for (int i = 0; i < AnimationLayers.Count; i++)
            {
                if (AnimationLayers[i].Name == name)
                    return i;
            }
            return -1;
        }

        private string _currentAnimationName = null;
        public string CurrentAnimationName
        {
            get => _currentAnimationName;
            set
            {
                _currentAnimationName = value;

                if (MODEL.ChrAsm != null)
                {
                    if (MODEL.ChrAsm.RightWeaponModel != null)
                    {
                        if (MODEL.ChrAsm.RightWeaponModel.AnimContainer.Animations.Count > 0)
                        {
                            if (value == null || MODEL.ChrAsm.RightWeaponModel.AnimContainer.Animations.ContainsKey(value))
                                MODEL.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = value;
                            else
                                MODEL.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = null;
                        }
                    }

                    if (MODEL.ChrAsm.LeftWeaponModel != null)
                    {
                        if (MODEL.ChrAsm.LeftWeaponModel.AnimContainer.Animations.Count > 0)
                        {
                            if (value == null || MODEL.ChrAsm.LeftWeaponModel.AnimContainer.Animations.ContainsKey(value))
                                MODEL.ChrAsm.LeftWeaponModel.AnimContainer.CurrentAnimationName = value;
                            else
                                MODEL.ChrAsm.LeftWeaponModel.AnimContainer.CurrentAnimationName = null;
                        }
                    }
                }

                if (value == null)
                {
                    // If current anim name set to null, remove all active animation layers, 
                    // reset all cached animations to time of 0, and stop playback.
                    foreach (var cachedAnimName in AnimationCache.Keys)
                        AnimationCache[cachedAnimName].Reset();
                    AnimationLayers.Clear();
                }
                else
                {
                    

                    if (animHKXsToLoad.ContainsKey(value))
                    {
                        //LoadAnimHKX(animHKXsToLoad[name], name);

                        try
                        {
                            NewHavokAnimation anim = null;

                            if (AnimationCache.ContainsKey(value))
                            {
                                anim = NewHavokAnimation.Clone(AnimationCache[value]);
                            }
                            else
                            {
                                anim = LoadAnimHKX(animHKXsToLoad[value], value);
                                if (anim != null)
                                    AnimationCache.Add(value, anim);
                            }


                            if (AnimationLayers.Count < 2)
                            {
                                anim.Weight = 1;
                                AnimationLayers.Add(anim);
                            }
                            else
                            {
                                anim.Weight = 0;

                                AnimationLayers[1].Weight = 1;
                                AnimationLayers[0] = AnimationLayers[1];

                                AnimationLayers[1] = anim;
                            }


                            //V2.0: Testing - Even out anim layer weights
                            //foreach (var layer in AnimationLayers)
                            //{
                            //    layer.Weight = (float)(1.0 / AnimationLayers.Count);
                            //}
                        }
                        catch
                        {
                            animHKXsToLoad.Remove(value);
                        }
                    }
                    else
                    {
                        foreach (var cachedAnimName in AnimationCache.Keys)
                            AnimationCache[cachedAnimName].Reset();
                        AnimationLayers.Clear();
                    }
                }
                //else
                //{
                //    CurrentAnimation?.Reset();
                //}
            }
        }

        public NewHavokAnimation CurrentAnimation
        {
            get
            {
                return AnimationLayers.LastOrDefault();
            }
        }

        public float CurrentAnimTime => CurrentAnimation?.CurrentTime ?? 0;
        public float? CurrentAnimDuration => CurrentAnimation?.Duration;

        public float? CurrentAnimFrameDuration => CurrentAnimation?.FrameDuration;

        //public static bool AutoPlayAnimContainersUponLoading = true;

        //public bool IsPlaying = true;
        //public bool IsLoop = true;

        private Vector4 currentRootMotionVec = Vector4.Zero;
        public Matrix FinalRootMotion => CurrentAnimation?.data.RootMotion?.ConvertSampleToMatrixWithViewNoRotation(currentRootMotionVec.ToCS()).ToXna() ?? Matrix.Identity;

        public void ResetRootMotion()
        {
            currentRootMotionVec = Vector4.Zero;
        }

        public void ResetAll()
        {
            foreach (var anim in AnimationLayers)
            {
                anim.Reset();
            }
        }

        private void OnScrubUpdateAnimLayersRootMotion()
        {
            if (!EnableRootMotion)
            {
                ResetRootMotion();
                return;
            }

            if (AnimationLayers.Count > 0)
            {
                //int totalCount = AnimationLayers.Count;
                //float totalWeight = 0;

                //Vector4 rootMotionDelta = Vector4.Zero;

                Vector4 AddDelta(Vector4 cur, Vector4 delta)
                {
                    //var deltaInDirection = Vector3.Transform(new Vector3(delta.X, delta.Y, delta.Z), Matrix.CreateRotationY(cur.W));
                    cur.X += delta.X;
                    cur.Y += delta.Y;
                    cur.Z += delta.Z;
                    cur.W += delta.W;

                    return cur;
                }

                Vector4 RotateDeltaToCurrentDirection(Vector4 v, float currentDirection)
                {
                    var rotated = Vector3.Transform(new Vector3(v.X, v.Y, v.Z), Matrix.CreateRotationY(currentDirection));
                    return new Vector4(rotated.X, rotated.Y, rotated.Z, v.W);
                }

                Vector4 RotateDeltaToCurrentDirectionMat(Vector4 v, Matrix currentDirection)
                {
                    var rotated = Vector3.Transform(new Vector3(v.X, v.Y, v.Z), currentDirection);
                    return new Vector4(rotated.X, rotated.Y, rotated.Z, v.W);
                }

                if (AnimationLayers.Count == 1)
                {
                    var a = RotateDeltaToCurrentDirectionMat(AnimationLayers[0].RootMotionDeltaOfLastScrub, AnimationLayers[0].RotMatrixAtStartOfAnim);

                    var deltaMatrix = Matrix.CreateTranslation(new Vector3(a.X, a.Y, a.Z));
                    MODEL.CurrentRootMotionTranslation *= deltaMatrix;
                    MODEL.CurrentDirection += a.W;
                }
                else if (AnimationLayers.Count == 2)
                {
                    var a = RotateDeltaToCurrentDirectionMat(AnimationLayers[0].RootMotionDeltaOfLastScrub, AnimationLayers[0].RotMatrixAtStartOfAnim);
                    var b = RotateDeltaToCurrentDirectionMat(AnimationLayers[1].RootMotionDeltaOfLastScrub, AnimationLayers[1].RotMatrixAtStartOfAnim);

                    var blended = Vector4.Lerp(a, b, AnimationLayers[1].Weight);

                    var deltaMatrix = Matrix.CreateTranslation(new Vector3(blended.X, blended.Y, blended.Z));
                    MODEL.CurrentRootMotionTranslation *= deltaMatrix;
                    MODEL.CurrentDirection += blended.W;
                }

                //// If our animations don't even add up to a total weight of 1, weigh them
                //// alongside the default value for the remaining weight
                //if (totalWeight < 1)
                //{
                //    rootMotionDelta += (Vector4.Zero) * (1 - totalWeight);
                //    totalCount++;
                //}

                //var debug_BeforeDelta = FinalRootMotion;

                //Matrix.

                //currentRootMotionVec += rootMotionDelta;



                //if (float.IsNaN(FinalRootMotion.M11) || float.IsNaN(FinalRootMotion.M12) || float.IsNaN(FinalRootMotion.M13) || float.IsNaN(FinalRootMotion.M14) ||
                //    float.IsNaN(FinalRootMotion.M21) || float.IsNaN(FinalRootMotion.M22) || float.IsNaN(FinalRootMotion.M23) || float.IsNaN(FinalRootMotion.M24) ||
                //    float.IsNaN(FinalRootMotion.M31) || float.IsNaN(FinalRootMotion.M32) || float.IsNaN(FinalRootMotion.M33) || float.IsNaN(FinalRootMotion.M34) ||
                //    float.IsNaN(FinalRootMotion.M41) || float.IsNaN(FinalRootMotion.M42) || float.IsNaN(FinalRootMotion.M43) || float.IsNaN(FinalRootMotion.M44))
                //{
                //    //throw new Exception("TREMBELCAT");
                //}
            }
            else
            {
                ResetRootMotion();
            }

        }

        //public bool Paused = false; 

        public NewAnimationContainer(Model mdl)
        {
            MODEL = mdl;
            //IsPlaying = AutoPlayAnimContainersUponLoading;
        }

        public void ScrubRelative(float timeDelta, bool doNotCheckRootMotionRotation = false)
        {
            //if (stopPlaying)
            //    IsPlaying = false;

            //CurrentAnimation?.Scrub(newTime, false, forceUpdate, loopCount, forceAbsoluteRootMotion);

            MODEL.Skeleton.ClearHkxBoneMatrices();

            var transA = new List<NewBlendableTransform>();
            var transB = new List<NewBlendableTransform>();

            float totalWeight = 0;
            for (int i = 0; i < AnimationLayers.Count; i++)
            {
                AnimationLayers[i].ScrubRelative(timeDelta, doNotCheckRootMotionRotation);
                totalWeight += AnimationLayers[i].Weight;
                //AnimationLayers[i].ApplyWeightedMotionToSkeleton(finalizeHkxMatrices: i == AnimationLayers.Count - 1, 1 - totalWeight);
                //AnimationLayers[i].CalculateCurrentFrame();

                for (int t = 0; t < MODEL.Skeleton.HkxSkeleton.Count; t++)
                {
                    if (i == 0)
                        transA.Add(AnimationLayers[i].GetBlendableTransformOnCurrentFrame(t));
                    else if (i == 1)
                        transB.Add(AnimationLayers[i].GetBlendableTransformOnCurrentFrame(t));
                }
            }

            //if (AnimationLayers.Count == 2)
            //{
            //    for (int t = 0; t < MODEL.Skeleton.HkxSkeleton.Count; t++)
            //    {

            //    }
            //        MODEL.Skeleton.SetHkxBoneMatrix(t, (NewBlendableTransform.Lerp(transA[t], transB[t], AnimationLayers[1].Weight)).GetMatrix().ToXna());
            //}
            //else if (AnimationLayers.Count == 1)
            //{
            //    for (int t = 0; t < MODEL.Skeleton.HkxSkeleton.Count; t++)
            //        MODEL.Skeleton.SetHkxBoneMatrix(t, transA[t].GetMatrix().ToXna());
            //}

            void WalkTree(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                if (AnimationLayers.Count == 2)
                {
                    currentMatrix = NewBlendableTransform.Lerp(transA[i], transB[i], AnimationLayers[1].Weight).GetMatrix().ToXna() * currentMatrix;
                }
                else if (AnimationLayers.Count == 1)
                {
                    currentMatrix = transA[i].GetMatrix().ToXna() * currentMatrix;
                }

                MODEL.Skeleton.SetHkxBoneMatrix(i, currentMatrix);

                foreach (var c in MODEL.Skeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree(c, currentMatrix, currentScale);
            }

            if (AnimationLayers.Count > 0)
            {
                foreach (var root in MODEL.Skeleton.RootBoneIndices)
                    WalkTree(root, Matrix.Identity, Vector3.One);
            }

                

            if (timeDelta != 0)
                OnScrubUpdateAnimLayersRootMotion();
        }

        public void Update()
        {
            //V2.0 TODO: If this is ever gonna be used, actually implement this, using .Scrub
            //           to simulate the old behavior of .Play
            //if (IsPlaying && CurrentAnimation != null)
            //{
            //    CurrentAnimation.Play(Main.DELTA_UPDATE, IsLoop, false, false);
            //}
        }

        private NewHavokAnimation LoadAnimHKX(byte[] hkxBytes, string name)
        {
            var hkxVariation = GameDataManager.GetCurrentLegacyHKXType();
            var hkx = HKX.Read(hkxBytes, hkxVariation);
            return LoadAnimHKX(hkx, name);
        }

        private void AddAnimHKXFetch(string name, byte[] hkx)
        {
            if (!animHKXsToLoad.ContainsKey(name))
                animHKXsToLoad.Add(name, hkx);
            else
                animHKXsToLoad[name] = hkx;
        }

        private NewHavokAnimation LoadAnimHKX(HKX hkx, string name)
        {
            //if (AnimationCache.ContainsKey(name))
            //    return AnimationCache[name];

            

            HKX.HKASplineCompressedAnimation animSplineCompressed = null;
            HKX.HKAInterleavedUncompressedAnimation animInterleavedUncompressed = null;

            HKX.HKAAnimationBinding animBinding = null;
            HKX.HKADefaultAnimatedReferenceFrame animRefFrame = null;

            foreach (var cl in hkx.DataSection.Objects)
            {
                if (cl is HKX.HKASplineCompressedAnimation asSplineCompressedAnim)
                {
                    animSplineCompressed = asSplineCompressedAnim;
                }
                else if (cl is HKX.HKAInterleavedUncompressedAnimation asInterleavedUncompressedAnim)
                {
                    animInterleavedUncompressed = asInterleavedUncompressedAnim;
                }
                else if (cl is HKX.HKAAnimationBinding asBinding)
                {
                    animBinding = asBinding;
                }
                else if (cl is HKX.HKADefaultAnimatedReferenceFrame asRefFrame)
                {
                    animRefFrame = asRefFrame;
                }
            }

            NewHavokAnimation anim = null;

            if (animSplineCompressed != null)
            {
                anim =  new NewHavokAnimation_SplineCompressed(name, MODEL.Skeleton, animRefFrame, animBinding, animSplineCompressed, this);
            }
            else if (animInterleavedUncompressed != null)
            {
                anim = new NewHavokAnimation_InterleavedUncompressed(name, MODEL.Skeleton, animRefFrame, animBinding, animInterleavedUncompressed, this);
            }

            return anim;
        }

        public void LoadAdditionalANIBND(IBinder anibnd, IProgress<double> progress)
        {
            var hkxVariation = GameDataManager.GetCurrentLegacyHKXType();

            if (hkxVariation == HKX.HKXVariation.Invalid)
            {
                //TODO
                Console.WriteLine("NewAnimationContainer.LoadAdditionalANIBND: Invalid legacy HKX type.");
            }
            else
            {
                Dictionary<string, byte[]> animHKXs = new Dictionary<string, byte[]>();
                Dictionary<string, byte[]> taes = new Dictionary<string, byte[]>();
                double i = 1;
                int fileCount = anibnd.Files.Count;
                foreach (var f in anibnd.Files)
                {
                    string shortName = new FileInfo(f.Name).Name.ToLower();
                    if (shortName.StartsWith("a") && shortName.EndsWith(".hkx"))
                    {
                        animHKXs.Add(shortName, f.Bytes);
                    }
                    else if (shortName.EndsWith(".tae") || TAE.Is(f.Bytes))
                    {
                        taes.Add(shortName, f.Bytes);
                    }
                    progress?.Report(((i++) / fileCount) / 2.0);
                }

                i = 0;
                fileCount = animHKXs.Count;

                foreach (var kvp in animHKXs)
                {
                    AddAnimHKXFetch(kvp.Key, kvp.Value);

                    progress?.Report(0.5 + (((i++) / fileCount) / 2.0));
                }

                foreach (var kvp in taes)
                {
                    lock (_lock_timeactDict)
                    {
                        if (!timeactFiles.ContainsKey(kvp.Key))
                            timeactFiles.Add(kvp.Key, kvp.Value);
                        else
                            timeactFiles[kvp.Key] = kvp.Value;
                    }
                }

                if (CurrentAnimationName == null && animHKXsToLoad.Count > 0)
                {
                    lock (_lock_animDict)
                    {
                        CurrentAnimationName = animHKXsToLoad.Keys.First();
                    }

                    ScrubRelative(0);
                }

            }

            progress?.Report(1.0);
        }

        public void LoadBaseANIBND(IBinder anibnd, IProgress<double> progress)
        {
            var hkxVariation = GameDataManager.GetCurrentLegacyHKXType();

            if (hkxVariation == HKX.HKXVariation.Invalid)
            {
                //TODO
                Console.WriteLine("NewAnimationContainer.LoadBaseANIBND: Invalid legacy HKX type.");
            }
            else
            {
                HKX.HKASkeleton hkaSkeleton = null;
                HKX skeletonHKX = null;
                Dictionary<string, byte[]> animHKXs = new Dictionary<string, byte[]>();
                Dictionary<string, byte[]> taes = new Dictionary<string, byte[]>();
                double i = 1;
                int fileCount = anibnd.Files.Count;
                foreach (var f in anibnd.Files)
                {
                    string shortName = new FileInfo(f.Name).Name.ToLower();
                    if (shortName == "skeleton.hkx"  //fatcat
                        || shortName == "skeleton_1.hkx"
                        || shortName == "skeleton_2.hkx"
                        || shortName == "skeleton_3.hkx")
                    {
                        skeletonHKX = HKX.Read(f.Bytes, hkxVariation);
                    }
                    else if (shortName.StartsWith("a") && shortName.EndsWith(".hkx"))
                    {
                        if (!animHKXs.ContainsKey(shortName))
                            animHKXs.Add(shortName, f.Bytes);
                        else
                            animHKXs[shortName] = f.Bytes;

                    }
                    else if (shortName.EndsWith(".tae") || TAE.Is(f.Bytes))
                    {
                        if (!taes.ContainsKey(shortName))
                            taes.Add(shortName, f.Bytes);
                        else
                            taes[shortName] = f.Bytes;
                    }
                    progress.Report(((i++) / fileCount) / 2.0);
                }

                if (skeletonHKX == null)
                {
                    throw new InvalidOperationException("HKX file had no skeleton.");
                }

                foreach (var o in skeletonHKX.DataSection.Objects)
                {
                    if (o is HKX.HKASkeleton asSkeleton)
                    {
                        hkaSkeleton = asSkeleton;
                    }
                }

                if (hkaSkeleton == null)
                {
                    throw new InvalidOperationException("HKX skeleton file had no actual skeleton class");
                }

                MODEL.Skeleton.LoadHKXSkeleton(hkaSkeleton);

                i = 0;
                fileCount = animHKXs.Count;

                foreach (var kvp in animHKXs)
                {
                    AddAnimHKXFetch(kvp.Key, kvp.Value);

                    progress.Report(0.5 + (((i++) / fileCount) / 2.0));
                }

                foreach (var kvp in taes)
                {
                    lock (_lock_timeactDict)
                    {
                        if (!timeactFiles.ContainsKey(kvp.Key))
                            timeactFiles.Add(kvp.Key, kvp.Value);
                        else
                            timeactFiles[kvp.Key] = kvp.Value;
                    }
                }

                lock (_lock_animDict)
                {
                    if (animHKXsToLoad.Count > 0)
                    {
                        CurrentAnimationName = animHKXsToLoad.Keys.First();
                        CurrentAnimation?.ScrubRelative(0, doNotCheckRootMotionRotation: true);
                    }
                }
            }

            

            progress.Report(1.0);
        }

    }
}
