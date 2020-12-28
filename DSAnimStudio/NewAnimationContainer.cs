using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NMatrix = System.Numerics.Matrix4x4;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;

namespace DSAnimStudio
{
    public class NewAnimationContainer
    {
        public NewAnimSkeleton_HKX Skeleton = new NewAnimSkeleton_HKX();
        private object _lock_animDict = new object();

        private object _lock_animCache = new object();

        private object _lock_timeactDict = new object();

        private Dictionary<string, byte[]> timeactFiles = new Dictionary<string, byte[]>();

        public object _lock_AnimationLayers = new object();

        public object _lock_AdditiveOverlays = new object();

        public bool ForcePlayAnim = false;

        public float DebugAnimWeight = 1;

        public bool IsAnimLoaded(string name)
        {
            return AnimationCache.ContainsKey(name);
        }

        public void MarkAllAnimsReferenceBlendWeights()
        {
            lock (_lock_AnimationLayers)
            {
                for (int i = 0; i < AnimationLayers.Count - 1; i++)
                {
                    var layer = AnimationLayers[i];
                    layer.ReferenceWeight = layer.Weight;
                }
            }
        }

        public void RemoveAnimsWithDeadReferenceWeights()
        {
            lock (_lock_AnimationLayers)
            {
                var layersToClear = new List<NewHavokAnimation>();
                for (int i = 0; i < AnimationLayers.Count - 1; i++)
                {
                    var layer = AnimationLayers[i];
                    if (layer.ReferenceWeight < 0.001f)
                        layersToClear.Add(layer);
                }
                foreach (var layer in layersToClear)
                {
                    AnimationLayers.Remove(layer);
                }
            }
        }

        public void RemoveAllAnimsExceptTopmost()
        {
            lock (_lock_AnimationLayers)
            {
                if (AnimationLayers.Count > 0)
                {
                    var anim = AnimationLayers[AnimationLayers.Count - 1];
                    anim.Weight = 1;
                    anim.ReferenceWeight = 1;
                    AnimationLayers = new List<NewHavokAnimation> { anim };
                }
            }
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

        public void AddNewHKXToLoad(string name, byte[] data)
        {
            lock (_lock_animDict)
            {
                if (animHKXsToLoad.ContainsKey(name))
                    animHKXsToLoad.Remove(name);
                animHKXsToLoad.Add(name, data);
            }

            lock (_lock_AdditiveOverlays)
            {
                var overlay = _additiveBlendOverlays.FirstOrDefault(a => a.Name == name);
                if (overlay != null)
                    _additiveBlendOverlays.Remove(overlay);

                if (_additiveBlendOverlayNames.Contains(name))
                    _additiveBlendOverlayNames.Remove(name);
            }

            lock (_lock_animCache)
            {
                if (AnimationCache.ContainsKey(name))
                    AnimationCache.Remove(name);
            }
        }

        public void AddNewAnimation(string name, NewHavokAnimation anim)
        {
            lock (_lock_animDict)
            {
                if (animHKXsToLoad.ContainsKey(name))
                    animHKXsToLoad.Remove(name);
            }

            lock (_lock_AdditiveOverlays)
            {
                var overlay = _additiveBlendOverlays.FirstOrDefault(a => a.Name == name);
                if (overlay != null)
                    _additiveBlendOverlays.Remove(overlay);

                if (_additiveBlendOverlayNames.Contains(name))
                    _additiveBlendOverlayNames.Remove(name);

                if (anim.IsAdditiveBlend)
                {
                    _additiveBlendOverlayNames.Add(anim.Name);
                    _additiveBlendOverlays.Add(NewHavokAnimation.Clone(anim));
                }
            }

            lock (_lock_animCache)
            {
                if (AnimationCache.ContainsKey(name))
                    AnimationCache.Remove(name);

                AnimationCache.Add(name, anim);
            }
        }

        public IReadOnlyDictionary<string, byte[]> Animations => animHKXsToLoad;

        public List<NewHavokAnimation> AnimationLayers = new List<NewHavokAnimation>();

        private Dictionary<string, NewHavokAnimation> AnimationCache = new Dictionary<string, NewHavokAnimation>();

        public void ClearAnimationCache()
        {
            lock (_lock_animCache)
            {
                AnimationCache.Clear();
            }
        }

        public int GetAnimLayerIndexByName(string name)
        {
            if (!Scene.CheckIfDrawing())
                return 0;

            lock (_lock_AnimationLayers)
            {
                
                for (int i = AnimationLayers.Count - 1; i >= 0; i--)
                {
                    if (AnimationLayers[i].Name == name)
                        return i;
                }
                return -1;
            }
            
        }



        private void ScanAllAnimations()
        {
            //if (MODEL.IS_PLAYER)
            //    return; //todo
            //lock (_lock_AdditiveOverlays)
            //{
            //    _additiveBlendOverlays = new List<NewHavokAnimation>();
            //    _additiveBlendOverlayNames = new List<string>();
            //}
            
                
            List<string> allAnimNames = new List<string>();
            lock (_lock_animDict)
            {
                foreach (var kvp in animHKXsToLoad)
                    allAnimNames.Add(kvp.Key);
            }

            LoadingTaskMan.DoLoadingTaskSynchronous("ScanningAllAnimations", "Loading animations...", prog =>
            {
                for (int i = 0; i < allAnimNames.Count; i++)
                {
                    prog?.Report(1.0 * i / allAnimNames.Count);
                    CurrentAnimationName = allAnimNames[i];
                }
                CurrentAnimationName = null;
                prog?.Report(1.0);
            });

                
            
        }
        private List<NewHavokAnimation> _additiveBlendOverlays = new List<NewHavokAnimation>();
        private List<string> _additiveBlendOverlayNames = new List<string>();

        public IReadOnlyList<NewHavokAnimation> AdditiveBlendOverlays => _additiveBlendOverlays;
        public IReadOnlyList<string> AdditiveBlendOverlayNames => _additiveBlendOverlayNames;

        public bool InitializeNewAnimLayersUnweighted = true;

        private string _currentAnimationName = null;
        public string CurrentAnimationName
        {
            get => _currentAnimationName;
            set
            {
                _currentAnimationName = value;

                

                if (value == null)
                {
                    // If current anim name set to null, remove all active animation layers, 
                    // reset all cached animations to time of 0, and stop playback.
                    lock (_lock_animCache)
                    {
                        foreach (var cachedAnimName in AnimationCache.Keys)
                            AnimationCache[cachedAnimName].Reset(RootMotionTransform.GetRootMotionVector4());
                    }
                    
                    lock (_lock_AnimationLayers)
                    {
                        AnimationLayers.Clear();
                    }
                }
                else
                {
                    

                    if (animHKXsToLoad.ContainsKey(value) || AnimationCache.ContainsKey(value))
                    {
                        //LoadAnimHKX(animHKXsToLoad[name], name);


                        //try
                        //{

                            NewHavokAnimation anim = null;
                            NewHavokAnimation cachedAnim = null;

                            lock (_lock_animCache)
                            {
                                if (AnimationCache.ContainsKey(value))
                                {
                                    cachedAnim = AnimationCache[value];
                                }
                            }

                            if (cachedAnim != null)
                            {
                                anim = NewHavokAnimation.Clone(cachedAnim);
                            }
                            else if (animHKXsToLoad.ContainsKey(value))
                            {
                                anim = LoadAnimHKX(animHKXsToLoad[value], value);
                                lock (_lock_animCache)
                                {
                                    if (anim != null)
                                        AnimationCache.Add(value, anim);
                                }
                            }

                            if (anim != null)
                            {
                                MarkAllAnimsReferenceBlendWeights();

                                anim.Weight = InitializeNewAnimLayersUnweighted ? 0 : 1;

                                anim.RootMotion.ResetToStart(RootMotionTransform.GetRootMotionVector4());

                                AnimationLayers.Add(anim);
                            }

                            




                        //V2.0: Testing - Even out anim layer weights
                        //foreach (var layer in AnimationLayers)
                        //{
                        //    layer.Weight = (float)(1.0 / AnimationLayers.Count);
                        //}
                    //}
                    //catch
                    //{
                    //    lock (_lock_animDict)
                    //    {
                    //        animHKXsToLoad.Remove(value);
                    //    }
                    //}

                    }
                    else
                    {
                        lock (_lock_animCache)
                        {
                            foreach (var cachedAnimName in AnimationCache.Keys)
                                AnimationCache[cachedAnimName].Reset(RootMotionTransform.GetRootMotionVector4());
                        }

                        lock (_lock_AnimationLayers)
                        {
                            AnimationLayers.Clear();
                        }
                        
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
                NewHavokAnimation result = null;
                lock (_lock_AnimationLayers)
                {
                    result = AnimationLayers.LastOrDefault();
                }
                return result;
            }
        }

        public float CurrentAnimTime => CurrentAnimation?.CurrentTime ?? 0;
        public float? CurrentAnimDuration => CurrentAnimation?.Duration;

        public float? CurrentAnimFrameDuration => CurrentAnimation?.FrameDuration;

        //public static bool AutoPlayAnimContainersUponLoading = true;

        //public bool IsPlaying = true;
        //public bool IsLoop = true;

        public bool EnableLooping = true;

        public NewBlendableTransform RootMotionTransform { get; private set; } = NewBlendableTransform.Identity;
        public NVector4 RootMotionTransformVec { get; private set; } = NVector4.Zero;



        public NewBlendableTransform GetRootMotionTransform(float? moduloUnit)
        {
            var tr = RootMotionTransform;
            if (moduloUnit != null)
            {
                tr.Translation.X = tr.Translation.X % moduloUnit.Value;
                tr.Translation.Z = tr.Translation.Z % moduloUnit.Value;
            }
            return tr;
        }

        public void ResetRootMotion()
        {
            RootMotionTransform = NewBlendableTransform.Identity;
        }

        public void ClearAnimations()
        {
            lock (_lock_AnimationLayers)
            {
                AnimationLayers.Clear();
            }

            lock (_lock_animCache)
            {
                AnimationCache.Clear();
            }

            lock (_lock_AdditiveOverlays)
            {
                _additiveBlendOverlays.Clear();
            }
        }

        public void ResetAll()
        {
            lock (_lock_AnimationLayers)
            {
                foreach (var anim in AnimationLayers)
                {
                    anim.Reset(RootMotionTransform.GetRootMotionVector4());
                }
            }

            lock (_lock_AdditiveOverlays)
            {
                foreach (var overlay in AdditiveBlendOverlays)
                {
                    overlay.Reset(RootMotionTransform.GetRootMotionVector4());
                }
            }

        }

        private void OnScrubUpdateAnimLayersRootMotion(List<NewHavokAnimation> animLayers)
        {
            //if (!EnableRootMotion)
            //{
            //    ResetRootMotion();
            //    return;
            //}

            if (animLayers.Count > 0)
            {
                //NewBlendableTransform currentRootMotion = RootMotionTransform;

                float totalWeight = 0;

                for (int i = 0; i < animLayers.Count; i++)
                {
                    if (animLayers[i].Weight * DebugAnimWeight <= 0)
                        continue;

                    totalWeight += animLayers[i].Weight;
                    RootMotionTransformVec = NVector4.Lerp(RootMotionTransformVec, 
                        animLayers[i].RootMotion.CurrentTransform, (animLayers[i].Weight / totalWeight) * DebugAnimWeight);
                }

                for (int i = 0; i < animLayers.Count; i++)
                {
                    animLayers[i].RootMotion.ApplyExternalTransformSuchThatCurrentTransformMatches(RootMotionTransformVec);
                }
            }
            else
            {
                ResetRootMotion();
            }

            RootMotionTransform = NewBlendableTransform.FromRootMotionSample(RootMotionTransformVec);

        }

        //public bool Paused = false; 

        public NewAnimationContainer()
        {
            //IsPlaying = AutoPlayAnimContainersUponLoading;
        }

        public void ScrubRelative(float timeDelta)
        {
            //ForcePlayAnim = false;

            //if (stopPlaying)
            //    IsPlaying = false;

            //CurrentAnimation?.Scrub(newTime, false, forceUpdate, loopCount, forceAbsoluteRootMotion);

            if (Skeleton == null)
                return;

            //Skeleton.RevertToReferencePose();

            float totalWeight = 0;

            List<NewHavokAnimation> animLayersCopy = null;

            lock (_lock_AnimationLayers)
            {
                animLayersCopy = AnimationLayers.ToList();
            }
            for (int i = 0; i < animLayersCopy.Count; i++)
            {
                animLayersCopy[i].EnableLooping = EnableLooping;
                animLayersCopy[i].ScrubRelative(timeDelta);
            }

            for (int t = 0; t < Skeleton.HkxSkeleton.Count; t++)
            {
                if (animLayersCopy.Count == 0)
                {
                    Skeleton.HkxSkeleton[t].CurrentHavokTransform = Skeleton.HkxSkeleton[t].RelativeReferenceTransform;

                }
                else
                {
                    var tr = NewBlendableTransform.Identity;
                    float weight = 0;
                    for (int i = 0; i < animLayersCopy.Count; i++)
                    {
                        if (animLayersCopy[i].Weight * DebugAnimWeight <= 0)
                            continue;

                        var frame = animLayersCopy[i].GetBlendableTransformOnCurrentFrame(t);



                        if (animLayersCopy[i].IsAdditiveBlend)
                        {
                            frame = Skeleton.HkxSkeleton[t].RelativeReferenceTransform * frame;
                        }

                        weight += animLayersCopy[i].Weight;
                        if (animLayersCopy.Count > 1)
                            tr = NewBlendableTransform.Lerp(tr, frame, animLayersCopy[i].Weight / weight);
                        else
                            tr = frame;


                    }
                    Skeleton.HkxSkeleton[t].CurrentHavokTransform = NewBlendableTransform.Lerp(Skeleton.HkxSkeleton[t].RelativeReferenceTransform, tr, DebugAnimWeight);
                }

                    
            }

            void WalkTree(int i, Matrix currentMatrix, Matrix scaleMatrix)
            {
                var parentTransformation = currentMatrix;
                var parentScaleMatrix = scaleMatrix;

                currentMatrix = Skeleton.HkxSkeleton[i].CurrentHavokTransform.GetMatrix().ToXna();

                scaleMatrix = Skeleton.HkxSkeleton[i].CurrentHavokTransform.GetMatrixScale().ToXna();

                //if (AnimationLayers[0].IsAdditiveBlend && (i >= 0 && i < MODEL.Skeleton.HkxSkeleton.Count))
                //    currentMatrix = MODEL.Skeleton.HkxSkeleton[i].RelativeReferenceMatrix * currentMatrix;

                lock (_lock_AdditiveOverlays)
                {
                    foreach (var overlay in _additiveBlendOverlays)
                    {
                        if (overlay.Weight > 0)
                            currentMatrix *= NewBlendableTransform.Lerp(NewBlendableTransform.Identity, overlay.GetBlendableTransformOnCurrentFrame(i), overlay.Weight).GetMatrix().ToXna();
                    }
                }

                currentMatrix *= parentTransformation;
                scaleMatrix *= parentScaleMatrix;

                Skeleton.HkxSkeleton[i].CurrentMatrix = scaleMatrix * currentMatrix;

                foreach (var c in Skeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree(c, currentMatrix, scaleMatrix);
            }

            foreach (var root in Skeleton.TopLevelHkxBoneIndices)
                WalkTree(root, Matrix.Identity, Matrix.Identity);



            OnScrubUpdateAnimLayersRootMotion(animLayersCopy);
        }

        public void Update()
        {
            //V2.0 TODO: If this is ever gonna be used, actually implement this, using .Scrub
            //           to simulate the old behavior of .Play
            if (CurrentAnimation != null)
            {
                if (ForcePlayAnim)
                    ScrubRelative(Main.DELTA_UPDATE);
            }
            else
            {
                Skeleton.RevertToReferencePose();
            }
            lock (_lock_AdditiveOverlays)
            {
                foreach (var overlay in _additiveBlendOverlays)
                {
                    if (overlay.Weight > 0)
                    {
                        overlay.ScrubRelative(Main.DELTA_UPDATE);
                    }
                }
            }
        }

        private NewHavokAnimation LoadAnimHKX(byte[] hkxBytes, string name)
        {
            var hkxVariation = GameDataManager.GetCurrentLegacyHKXType();
            HKX hkx = null;
            try
            {
                hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT));
            }
            catch
            {

            }

            if (hkx == null)
            {
                hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);
            }
            
            var anim = LoadAnimHKX(hkx, name);

            if (anim != null)
            {
                if (anim.IsAdditiveBlend)
                {
                    lock (_lock_AdditiveOverlays)
                    {
                        if (!_additiveBlendOverlayNames.Contains(anim.Name))
                        {
                            var clone = NewHavokAnimation.Clone(anim);
                            clone.Weight = -1;
                            _additiveBlendOverlays.Add(clone);
                            _additiveBlendOverlayNames.Add(anim.Name);
                            _additiveBlendOverlays = _additiveBlendOverlays.OrderBy(x => x.Name).ToList();
                        }
                    }


                }
            }

            return anim;
        }

        private void AddAnimHKXFetch(string name, byte[] hkx)
        {
            lock (_lock_animDict)
            {
                if (!animHKXsToLoad.ContainsKey(name))
                    animHKXsToLoad.Add(name, hkx);
                else
                    animHKXsToLoad[name] = hkx;
            }
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
                anim =  new NewHavokAnimation_SplineCompressed(name, Skeleton, animRefFrame, animBinding, animSplineCompressed, this);
            }
            else if (animInterleavedUncompressed != null)
            {
                anim = new NewHavokAnimation_InterleavedUncompressed(name, Skeleton, animRefFrame, animBinding, animInterleavedUncompressed, this);
            }

            return anim;
        }

        public void LoadAdditionalANIBND(IBinder anibnd, IProgress<double> progress, bool scanAnims)
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
                        if (!animHKXs.ContainsKey(shortName))
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

                if (scanAnims)
                {
                    ScanAllAnimations();
                }
                

                if (CurrentAnimationName == null && animHKXsToLoad.Count > 0)
                {
                    string firstAnim = null;
                    lock (_lock_animDict)
                    {
                        firstAnim = animHKXsToLoad.Keys.First();
                    }
                    CurrentAnimationName = firstAnim;

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
                        skeletonHKX = HKX.Read(f.Bytes, hkxVariation, isDS1RAnimHotfix: (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R));
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

                Skeleton = new NewAnimSkeleton_HKX();
                Skeleton.LoadHKXSkeleton(hkaSkeleton);

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

                ScanAllAnimations();

                lock (_lock_animDict)
                {
                    if (animHKXsToLoad.Count > 0)
                    {
                        CurrentAnimationName = animHKXsToLoad.Keys.First();
                        CurrentAnimation?.ScrubRelative(0);
                    }
                }
            }

            

            progress.Report(1.0);
        }

    }
}
