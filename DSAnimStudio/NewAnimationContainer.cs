using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewAnimationContainer
    {
        public readonly Model MODEL;

        private object _lock_animDict = new object();

        private object _lock_animCache = new object();

        private object _lock_timeactDict = new object();

        private Dictionary<string, byte[]> timeactFiles = new Dictionary<string, byte[]>();

        public bool EnableRootMotion = true;
        public bool EnableRootMotionWrap = true;

        public object _lock_AnimationLayers = new object();

        public object _lock_AdditiveOverlays = new object();

        public bool ForcePlayAnim = false;

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

        private string _currentAnimationName = null;
        public string CurrentAnimationName
        {
            get => _currentAnimationName;
            set
            {
                _currentAnimationName = value;

                string GetMatchingAnim(IEnumerable<string> animNames, string desiredName)
                {
                    if (desiredName == null)
                        return null;

                    desiredName = desiredName.Replace(".hkx", "");
                    foreach (var a in animNames)
                    {
                        if (a.StartsWith(desiredName))
                            return a;
                    }
                    return null;
                }

                if (MODEL.ChrAsm != null)
                {
                    void DoWPNAnims(Model wpnMdl)
                    {
                        if (wpnMdl != null)
                        {
                            if (wpnMdl.AnimContainer.Animations.Count > 0)
                            {
                                var matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, value);
                                if (value == null || matching != null)
                                {
                                    wpnMdl.AnimContainer.CurrentAnimationName = matching;
                                }
                                else
                                {
                                    matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, GameDataManager.GameTypeHasLongAnimIDs ? "a999_000000.hkx" : "a99_0000.hkx");
                                    if (matching != null)
                                    {
                                        wpnMdl.AnimContainer.CurrentAnimationName = matching;
                                    }
                                    else
                                    {
                                        matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, GameDataManager.GameTypeHasLongAnimIDs ? "a000_000000.hkx" : "a00_0000.hkx");
                                        if (matching != null)
                                        {
                                            wpnMdl.AnimContainer.CurrentAnimationName = matching;
                                        }
                                        else
                                        {
                                            wpnMdl.AnimContainer.CurrentAnimationName =
                                                wpnMdl.AnimContainer.Animations.Keys.First();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    lock (MODEL.ChrAsm._lock_doingAnythingWithWeaponModels)
                    {

                        DoWPNAnims(MODEL.ChrAsm.RightWeaponModel0);
                        DoWPNAnims(MODEL.ChrAsm.RightWeaponModel1);
                        DoWPNAnims(MODEL.ChrAsm.RightWeaponModel2);
                        DoWPNAnims(MODEL.ChrAsm.RightWeaponModel3);

                        DoWPNAnims(MODEL.ChrAsm.LeftWeaponModel0);
                        DoWPNAnims(MODEL.ChrAsm.LeftWeaponModel1);
                        DoWPNAnims(MODEL.ChrAsm.LeftWeaponModel2);
                        DoWPNAnims(MODEL.ChrAsm.LeftWeaponModel3);
                    }

                   
                }

                if (value == null)
                {
                    // If current anim name set to null, remove all active animation layers, 
                    // reset all cached animations to time of 0, and stop playback.
                    lock (_lock_animCache)
                    {
                        foreach (var cachedAnimName in AnimationCache.Keys)
                            AnimationCache[cachedAnimName].Reset();
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


                        try
                        {

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
                                lock (_lock_AnimationLayers)
                                {
                                    if (AnimationLayers.Count < 2)
                                    {
                                        if (AnimationLayers.Count == 0)
                                            anim.Weight = 1;
                                        else
                                            anim.Weight = 0;
                                        AnimationLayers.Add(anim);
                                    }
                                    else
                                    {
                                        anim.Weight = 0;

                                        AnimationLayers[1].Weight = 1;
                                        AnimationLayers[0] = AnimationLayers[1];

                                        AnimationLayers[1] = anim;
                                    }
                                }
                            }

                            




                        //V2.0: Testing - Even out anim layer weights
                        //foreach (var layer in AnimationLayers)
                        //{
                        //    layer.Weight = (float)(1.0 / AnimationLayers.Count);
                        //}
                    }
                    catch
                    {
                        lock (_lock_animDict)
                        {
                            animHKXsToLoad.Remove(value);
                        }
                    }

                    }
                    else
                    {
                        lock (_lock_animCache)
                        {
                            foreach (var cachedAnimName in AnimationCache.Keys)
                                AnimationCache[cachedAnimName].Reset();
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

        private Vector4 currentRootMotionVec = Vector4.Zero;
        public Matrix FinalRootMotion => CurrentAnimation?.data.RootMotion?.ConvertSampleToMatrixWithViewNoRotation(currentRootMotionVec.ToCS()).ToXna() ?? Matrix.Identity;

        public void ResetRootMotion()
        {
            currentRootMotionVec = Vector4.Zero;
            MODEL.ChrAsm?.RightWeaponModel0?.AnimContainer?.ResetRootMotion();
            MODEL.ChrAsm?.RightWeaponModel1?.AnimContainer?.ResetRootMotion();
            MODEL.ChrAsm?.RightWeaponModel2?.AnimContainer?.ResetRootMotion();
            MODEL.ChrAsm?.RightWeaponModel3?.AnimContainer?.ResetRootMotion();
            MODEL.ChrAsm?.LeftWeaponModel0?.AnimContainer?.ResetRootMotion();
            MODEL.ChrAsm?.LeftWeaponModel1?.AnimContainer?.ResetRootMotion();
            MODEL.ChrAsm?.LeftWeaponModel2?.AnimContainer?.ResetRootMotion();
            MODEL.ChrAsm?.LeftWeaponModel3?.AnimContainer?.ResetRootMotion();
        }

        public void ResetAll()
        {
            lock (_lock_AnimationLayers)
            {
                foreach (var anim in AnimationLayers)
                {
                    anim.Reset();
                }
            }

            lock (_lock_AdditiveOverlays)
            {
                foreach (var overlay in AdditiveBlendOverlays)
                {
                    overlay.Reset();
                }
            }

            MODEL.ChrAsm?.ForeachWeaponModel(m => m.AnimContainer?.ResetAll());
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

                lock (_lock_AnimationLayers)
                {
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
                        AnimationLayers[1].ApplyExternalRotation(AnimationLayers[0].RootMotionDeltaOfLastScrub.W * (1 - AnimationLayers[1].Weight));
                        var b = RotateDeltaToCurrentDirectionMat(AnimationLayers[1].RootMotionDeltaOfLastScrub, AnimationLayers[1].RotMatrixAtStartOfAnim);

                        var blended = Vector4.Lerp(a, b, AnimationLayers[1].Weight);

                        var deltaMatrix = Matrix.CreateTranslation(new Vector3(blended.X, blended.Y, blended.Z));
                        MODEL.CurrentRootMotionTranslation *= deltaMatrix;
                        MODEL.CurrentDirection += blended.W;
                    }
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

            lock (_lock_AnimationLayers)
            {
                for (int i = 0; i < AnimationLayers.Count; i++)
                {
                    AnimationLayers[i].ScrubRelative(timeDelta, doNotCheckRootMotionRotation);
                    totalWeight += AnimationLayers[i].Weight;
                    //AnimationLayers[i].ApplyWeightedMotionToSkeleton(finalizeHkxMatrices: i == AnimationLayers.Count - 1, 1 - totalWeight);
                    //AnimationLayers[i].CalculateCurrentFrame();

                    for (int t = 0; t < MODEL.Skeleton.HkxSkeleton.Count; t++)
                    {
                        var curTransform = AnimationLayers[i].GetBlendableTransformOnCurrentFrame(t);
                        if (i == 0)
                        {
                            transA.Add(curTransform);
                            if (AnimationLayers.Count == 1)
                                MODEL.Skeleton.HkxSkeleton[t].CurrentHavokTransform = curTransform;
                        }
                        else if (i == 1)
                        {
                            transB.Add(curTransform);
                            if (AnimationLayers.Count == 2)
                                MODEL.Skeleton.HkxSkeleton[t].CurrentHavokTransform = NewBlendableTransform.Lerp(transA[t], transB[t], AnimationLayers[1].Weight);
                        }
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

                void WalkTree(int i, Matrix currentMatrix, Matrix scaleMatrix)
                {
                    if (AnimationLayers.Count == 2)
                    {
                        var parentTransformation = currentMatrix;
                        var parentScaleMatrix = scaleMatrix;

                        currentMatrix = AnimationLayers[1].IsAdditiveBlend ? transB[i].GetMatrix().ToXna() :
                            NewBlendableTransform.Lerp(transA[i], transB[i], AnimationLayers[1].Weight).GetMatrix().ToXna();

                        scaleMatrix = AnimationLayers[1].IsAdditiveBlend ? transB[i].GetMatrixScale().ToXna() :
                            NewBlendableTransform.Lerp(transA[i], transB[i], AnimationLayers[1].Weight).GetMatrixScale().ToXna();

                        if (AnimationLayers[0].IsAdditiveBlend && (i >= 0 && i < MODEL.Skeleton.HkxSkeleton.Count))
                            currentMatrix = MODEL.Skeleton.HkxSkeleton[i].RelativeReferenceMatrix * currentMatrix;

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
                    }
                    else if (AnimationLayers.Count == 1)
                    {
                        var parentTransformation = currentMatrix;
                        var parentScaleMatrix = scaleMatrix;

                        currentMatrix = transA[i].GetMatrix().ToXna();

                        scaleMatrix = transA[i].GetMatrixScale().ToXna();

                        if (AnimationLayers[0].IsAdditiveBlend && (i >= 0 && i < MODEL.Skeleton.HkxSkeleton.Count))
                            currentMatrix = MODEL.Skeleton.HkxSkeleton[i].RelativeReferenceMatrix * currentMatrix;

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
                    }

                    MODEL.Skeleton.SetHkxBoneMatrix(i, scaleMatrix * currentMatrix);

                    foreach (var c in MODEL.Skeleton.HkxSkeleton[i].ChildIndices)
                        WalkTree(c, currentMatrix, scaleMatrix);
                }

                if (AnimationLayers.Count > 0)
                {
                    foreach (var root in MODEL.Skeleton.RootBoneIndices)
                        WalkTree(root, Matrix.Identity, Matrix.Identity);
                }



                if (timeDelta != 0)
                    OnScrubUpdateAnimLayersRootMotion();
            }
        }

        public void Update()
        {
            //V2.0 TODO: If this is ever gonna be used, actually implement this, using .Scrub
            //           to simulate the old behavior of .Play
            if (ForcePlayAnim && CurrentAnimation != null)
            {
                ScrubRelative(Main.DELTA_UPDATE);
            }
            lock (_lock_AdditiveOverlays)
            {
                foreach (var overlay in _additiveBlendOverlays)
                {
                    if (overlay.Weight > 0)
                    {
                        overlay.ScrubRelative(Main.DELTA_UPDATE, doNotCheckRootMotionRotation: true);
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
                hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (GameDataManager.GameType == GameDataManager.GameTypes.DS1R
                || GameDataManager.GameType == GameDataManager.GameTypes.SDT));
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
                anim =  new NewHavokAnimation_SplineCompressed(name, MODEL.Skeleton, animRefFrame, animBinding, animSplineCompressed, this);
            }
            else if (animInterleavedUncompressed != null)
            {
                anim = new NewHavokAnimation_InterleavedUncompressed(name, MODEL.Skeleton, animRefFrame, animBinding, animInterleavedUncompressed, this);
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
                        skeletonHKX = HKX.Read(f.Bytes, hkxVariation, isDS1RAnimHotfix: (GameDataManager.GameType == GameDataManager.GameTypes.DS1R));
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

                ScanAllAnimations();

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
