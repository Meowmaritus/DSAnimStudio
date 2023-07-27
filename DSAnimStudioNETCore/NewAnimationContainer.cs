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
#if DEBUG
        public static bool DISABLE_ANIM_LOAD_ERROR_HANDLER = true;
#else
        public static bool DISABLE_ANIM_LOAD_ERROR_HANDLER = false;
#endif

        public static bool Debug_ForceReferenceBoneLengths = false;

        public NewAnimSkeleton_HKX Skeleton = new NewAnimSkeleton_HKX();

        private object _lock_AnimCacheAndDict = new object();

        private object _lock_timeactDict = new object();

        private Dictionary<string, byte[]> timeactFiles = new Dictionary<string, byte[]>();

        public object _lock_AnimationLayers = new object();

        public object _lock_AdditiveOverlays = new object();

        public bool ForcePlayAnim = false;

        public float DebugAnimWeight = 1;

        public bool ForceDisableAnimLayerSystem = true;

        
        public class AnimHkxInfo
        {
            public byte[] HkxBytes;
            public byte[] CompendiumBytes;
        }
        public AnimHkxInfo FindAnimationBytes(string animName)
        {
            var result = new AnimHkxInfo();

            lock (_lock_AnimCacheAndDict)
            {
                if (animName != null)
                {
                    // If anim is in anibnd, then return it
                    if (animHKXsToLoad.ContainsKey(animName))
                    {
                        if (animHKXsCompendiumAssign.ContainsKey(animName))
                        {
                            result.CompendiumBytes = compendiumsToLoad[animHKXsCompendiumAssign[animName]];
                        }
                        result.HkxBytes = animHKXsToLoad[animName];
                    }
                }
            }

            return result;
        }

        public NewHavokAnimation FindAnimation(string animName, bool returnNewInstance = true)
        {
            NewHavokAnimation selectedAnim = null;

            lock (_lock_AnimCacheAndDict)
            {
                if (animName != null)
                {
                    // If anim isn't loaded but is in anibnd, then load it.
                    if (animHKXsToLoad.ContainsKey(animName) && !AnimationCache.ContainsKey(animName))
                    {
                        byte[] compendium = null;
                        if (animHKXsCompendiumAssign.ContainsKey(animName))
                        {
                            compendium = compendiumsToLoad[animHKXsCompendiumAssign[animName]];
                        }
                        var newlyLoadedAnim = LoadAnimHKX(animHKXsToLoad[animName], animName, compendium);
                        if (newlyLoadedAnim != null)
                        {
                            AnimationCache.Add(animName, newlyLoadedAnim);
                            selectedAnim = newlyLoadedAnim;
                        }
                    }

                    // If anim is loaded, select it
                    if (returnNewInstance)
                    {
                        if (AnimationCache.ContainsKey(animName))
                            selectedAnim = NewHavokAnimation.Clone(AnimationCache[animName], false);
                    }
                    else
                    {
                        if (AnimationCache.ContainsKey(animName))
                            selectedAnim = AnimationCache[animName];
                    }
                }
            }

            return selectedAnim;
        }

        public bool IsAnimLoaded(string name)
        {
            bool result = false;
            lock (_lock_AnimCacheAndDict)
            {
                result = AnimationCache.ContainsKey(name);
            }
            return result;
        }

        public void MarkAllAnimsReferenceBlendWeights()
        {
            lock (_lock_AnimationLayers)
            {
                for (int i = 0; i < AnimationLayers.Count; i++)
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
                var layersCopy_Base = AnimationLayers.Where(la => !la.IsUpperBody).ToList();
                var layersCopy_UpperBody = AnimationLayers.Where(la => la.IsUpperBody).ToList();
                for (int i = 0; i < layersCopy_Base.Count - 1; i++)
                {
                    var layer = layersCopy_Base[i];
                    if (layer.ReferenceWeight < 0.01f)
                        layersToClear.Add(layer);
                }
                for (int i = 0; i < layersCopy_UpperBody.Count - 1; i++)
                {
                    var layer = layersCopy_UpperBody[i];
                    if (layer.ReferenceWeight < 0.01f)
                        layersToClear.Add(layer);
                }
                foreach (var layer in layersToClear)
                {
                    AnimationLayers.Remove(layer);
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
        private Dictionary<string, string> animHKXsCompendiumAssign = new Dictionary<string, string>();
        private Dictionary<string, byte[]> compendiumsToLoad = new Dictionary<string, byte[]>();

        public void AddNewCompendiumToLoad(string name, byte[] data)
        {
            lock (_lock_AnimCacheAndDict)
            {
                if (compendiumsToLoad.ContainsKey(name))
                    compendiumsToLoad.Remove(name);
                compendiumsToLoad.Add(name, data);
            }
        }

        public void AddNewHKXToLoad(string name, byte[] data, string compendiumName = null)
        {
            lock (_lock_AnimCacheAndDict)
            {
                if (animHKXsToLoad.ContainsKey(name))
                    animHKXsToLoad.Remove(name);
                animHKXsToLoad.Add(name, data);

                if (!string.IsNullOrEmpty(compendiumName))
                {
                    if (animHKXsToLoad.ContainsKey(name))
                        animHKXsToLoad.Remove(name);
                    animHKXsCompendiumAssign.Add(name, compendiumName);
                }
            }

            int animID = name.ExtractDigitsToInt();

            // Clear this anim from loaded anims cache so it will be reloaded upon request.

            lock (_lock_AdditiveOverlays)
            {
                if (_additiveBlendOverlays.ContainsKey(animID))
                    _additiveBlendOverlays.Remove(animID);
            }

            lock (_lock_AnimCacheAndDict)
            {
                if (AnimationCache.ContainsKey(name))
                    AnimationCache.Remove(name);
            }
        }

        public void AddNewAnimation(string name, NewHavokAnimation anim)
        {
            lock (_lock_AnimCacheAndDict)
            {
                if (animHKXsToLoad.ContainsKey(name))
                    animHKXsToLoad.Remove(name);
            }

            int animID = name.ExtractDigitsToInt();

            lock (_lock_AdditiveOverlays)
            {
                if (_additiveBlendOverlays.ContainsKey(animID))
                {
                    _additiveBlendOverlays.Remove(animID);

                }

                if (anim.IsAdditiveBlend)
                {
                    var newOverlay = NewHavokAnimation.Clone(anim, false);
                    newOverlay.EnableLooping = true;
                    _additiveBlendOverlays.Add(animID, newOverlay);
                }
            }

            lock (_lock_AnimCacheAndDict)
            {
                if (AnimationCache.ContainsKey(name))
                    AnimationCache.Remove(name);

                AnimationCache.Add(name, anim);
            }
        }

        public IReadOnlyDictionary<string, byte[]> Animations => animHKXsToLoad;

        public List<NewHavokAnimation> AnimationLayers = new List<NewHavokAnimation>();

        private Dictionary<string, NewHavokAnimation> AnimationCache = new Dictionary<string, NewHavokAnimation>();

        public List<string> GetAllAnimationNames()
        {
            List<string> allAnimNames = new List<string>();
            lock (_lock_AnimCacheAndDict)
            {
                foreach (var kvp in animHKXsToLoad)
                {
                    allAnimNames.Add(kvp.Key);
                }
            }
            return allAnimNames;
        }

        public List<NewHavokAnimation> GetAllAnimations()
        {
            string curAnimName = CurrentAnimationName;
            List<NewHavokAnimation> result = new List<NewHavokAnimation>();
            List<string> allAnimNames = new List<string>();
            lock (_lock_AnimCacheAndDict)
            {
                foreach (var kvp in Animations)
                {
                    allAnimNames.Add(kvp.Key);
                }
            }

            foreach (var animName in allAnimNames)
            {
                bool includedInCache = false;
                lock (_lock_AnimCacheAndDict)
                {
                    includedInCache = AnimationCache.ContainsKey(animName);
                }
                if (!includedInCache)
                {
                    ChangeToNewAnimation(animName, 1, 0, true, false);
                }
                NewHavokAnimation anim = null;
                lock (_lock_AnimCacheAndDict)
                {
                    if (AnimationCache.ContainsKey(animName))
                        anim = AnimationCache[animName];
                }
                if (anim != null)
                    result.Add(anim);
            }

            ChangeToNewAnimation(curAnimName, 1, 0, true, false);
            ResetAll();

            return result;
        }

        public void ClearAnimationCache()
        {
            lock (_lock_AnimCacheAndDict)
            {
                AnimationCache.Clear();
            }
        }

        public int GetAnimLayerIndexByName(string name)
        {
            if (!Scene.CheckIfDrawing())
                return 0;

            int index = -1;

            lock (_lock_AnimationLayers)
            {
                
                for (int i = AnimationLayers.Count - 1; i >= 0; i--)
                {
                    if (AnimationLayers[i].Name == name)
                    {
                        index = i;
                        break;
                    }
                }
            }

            return index;
            
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
            lock (_lock_AnimCacheAndDict)
            {
                foreach (var kvp in animHKXsToLoad)
                    allAnimNames.Add(kvp.Key);
            }

            LoadingTaskMan.DoLoadingTaskSynchronous("ScanningAllAnimations", "Loading animations...", prog =>
            {
                for (int i = 0; i < allAnimNames.Count; i++)
                {
                    prog?.Report(1.0 * i / allAnimNames.Count);
                    ChangeToNewAnimation(allAnimNames[i], 1, 0, true);
                }
                ChangeToNewAnimation(null, 1, 0, true);
                prog?.Report(1.0);
            });

                
            
        }
        private Dictionary<int, NewHavokAnimation> _additiveBlendOverlays = new Dictionary<int, NewHavokAnimation>();
        //private List<string> _additiveBlendOverlayNames = new List<string>();

        public float GetAdditiveOverlayWeight(int animID)
        {
            float result = -1;
            lock (_lock_AdditiveOverlays)
            {
                if (_additiveBlendOverlays.ContainsKey(animID))
                    result = _additiveBlendOverlays[animID].Weight;
            }
            return result;
        }

        public bool AnyAdditiveLayers()
        {
            bool result = false;
            lock (_lock_AdditiveOverlays)
            {
                result = _additiveBlendOverlays.Any(x => !(x.Value.CurrentTime >= x.Value.Duration || x.Value.Weight <= 0.001f));
            }
            return result;
        }

        public void SetAdditiveLayers(Dictionary<int, NewHavokAnimation.AnimOverlayRequest> add)
        {
            lock (_lock_AdditiveOverlays)
            {
                // Stop playing any that are no longer requested.
                foreach (var a in _additiveBlendOverlays)
                {
                    if (!add.ContainsKey(a.Key) || add[a.Key].Weight <= 0)
                    {
                        a.Value.Weight = -1;
                        a.Value.Reset(RootMotionTransform.GetRootMotionVector4());
                    }
                }

                // Play any that are requested and not playing yet. Update any that are playing currently.
                foreach (var a in add)
                {
                    if (_additiveBlendOverlays.ContainsKey(a.Key))
                    {
                        if (_additiveBlendOverlays[a.Key].Weight < 0)
                        {
                            _additiveBlendOverlays[a.Key].Reset(RootMotionTransform.GetRootMotionVector4());
                            _additiveBlendOverlays[a.Key].EnableLooping = true;
                        }
                        _additiveBlendOverlays[a.Key].OverlayRequest = a.Value;

#if NIGHTFALL

                        if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R && a.Value.NF_RequestedLerpS != 0)
                        {
                            
                            if (a.Value.NF_RequestedLerpS > 0)
                            {
                                // Above 0: Blend from <any current weight> to the desired weight using the requested value for Lerp S
                                _additiveBlendOverlays[a.Key].Weight = MathHelper.Lerp(_additiveBlendOverlays[a.Key].Weight, a.Value.Weight, a.Value.NF_RequestedLerpS);
                            }
                            else
                            {
                                // Below 0 = lerp from <any current weight> to the desired weight over course of event.
                                _additiveBlendOverlays[a.Key].Weight = MathHelper.Lerp(a.Value.NF_WeightAtEvStart, a.Value.Weight, a.Value.NF_EvInputLerpS);
                            }
                        }
                        else
                        {
                            // Equal to 0: Set weight directly
                            _additiveBlendOverlays[a.Key].Weight = a.Value.Weight;
                        }
#else
                        _additiveBlendOverlays[a.Key].Weight = a.Value.Weight;
#endif


                    }
                }
            }
        }

        public IReadOnlyDictionary<int, NewHavokAnimation> AdditiveBlendOverlays => _additiveBlendOverlays;
        public IReadOnlyList<string> AdditiveBlendOverlayNames => _additiveBlendOverlays.Select(a => a.Value.Name).ToList();

        public void ClearAnimation()
        {
            ChangeToNewAnimation(null, 0, 0, false);
        }

        public void ChangeToNewAnimation(string animName, float animWeight, float startTime, bool clearOldLayers, bool isUpperBody = false)
        {
            if (ForceDisableAnimLayerSystem)
            {
                // lol
                animWeight = 1;
                clearOldLayers = true;
            }

            lock (_lock_AnimCacheAndDict)
            {
                lock (_lock_AnimationLayers)
                {
                    NewHavokAnimation selectedAnim = null;

                    if (animName != null)
                    {
                        // If anim isn't loaded but is in anibnd, then load it.
                        if (animHKXsToLoad.ContainsKey(animName) && !AnimationCache.ContainsKey(animName))
                        {
                            byte[] compendium = null;
                            if (animHKXsCompendiumAssign.ContainsKey(animName))
                            {
                                compendium = compendiumsToLoad[animHKXsCompendiumAssign[animName]];
                            }

                            var newlyLoadedAnim = LoadAnimHKX(animHKXsToLoad[animName], animName, compendium);
                            if (newlyLoadedAnim != null)
                            {
                                AnimationCache.Add(animName, newlyLoadedAnim);
                                selectedAnim = newlyLoadedAnim;
                            }
                        }

                        // If anim is loaded, select it
                        if (AnimationCache.ContainsKey(animName))
                            selectedAnim = NewHavokAnimation.Clone(AnimationCache[animName], isUpperBody);
                    }

                    if (selectedAnim != null)
                    {
                        if (clearOldLayers)
                        {
                            ClearAnimationLayers(upperBodyOnly: isUpperBody);

                        }

                        MarkAllAnimsReferenceBlendWeights();
                        selectedAnim.Weight = animWeight;
                        selectedAnim.RootMotion.SyncTimeAndLocation(RootMotionTransform.GetRootMotionVector4(), startTime);
                        selectedAnim.ScrubRelative(startTime);
                        AnimationLayers.Add(selectedAnim);
                    }
                    else
                    {
                        // If current anim name set to null, remove all active animation layers, 
                        // reset all cached animations to time of 0, and stop playback.
                        foreach (var cachedAnimName in AnimationCache.Keys)
                            AnimationCache[cachedAnimName].Reset(RootMotionTransform.GetRootMotionVector4());

                        ClearAnimationLayers(upperBodyOnly: isUpperBody);
                    }
                }
            }
        }

        public string CurrentAnimationName => CurrentAnimation?.Name ?? "None";

        public NewHavokAnimation CurrentAnimation
        {
            get
            {
                NewHavokAnimation result = null;
                lock (_lock_AnimationLayers)
                {
                    result = AnimationLayers.LastOrDefault(la => !la.IsUpperBody);
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
        public NVector4 RootMotionTransformVec_Prev { get; private set; } = NVector4.Zero;




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
            RootMotionTransformVec = NVector4.Zero;
            RootMotionTransformVec_Prev = NVector4.Zero;
            lock (_lock_AnimationLayers)
            {
                foreach (var al in AnimationLayers)
                {
                    al.Reset(NVector4.Zero);
                    al.RootMotionTransformDelta = NVector4.Zero;
                    al.RootMotionTransformLastFrame = NVector4.Zero;
                    al.RootMotion.SyncTimeAndLocation(NVector4.Zero, al.RootMotion.CurrentTime);
                }
            }
        }

        public void ClearAnimationLayers(bool upperBodyOnly)
        {
            if (upperBodyOnly)
            {
                AnimationLayers.RemoveAll(layer => layer.IsUpperBody);
            }
            else
            {
                AnimationLayers.Clear();
            }
        }

        public void ClearAnimations()
        {
            lock (_lock_AnimationLayers)
            {
                ClearAnimationLayers(upperBodyOnly: false);
            }

            lock (_lock_AnimCacheAndDict)
            {
                AnimationCache.Clear();
            }

            lock (_lock_AdditiveOverlays)
            {
                _additiveBlendOverlays.Clear();
            }
        }

        /// <summary>
        /// Make sure you fucking lock _lock_AnimationLayers to call this reeee
        /// </summary>
        public void RemoveCurrentTransitions(bool removeBase = true, bool removeUpper = true)
        {
            var layersToKeep = new List<NewHavokAnimation>();

            if (removeBase)
            {
                var lastBaseAnim = AnimationLayers.LastOrDefault(layer => !layer.IsUpperBody);
                if (lastBaseAnim != null)
                {
                    lastBaseAnim.Weight = 1;
                    lastBaseAnim.ReferenceWeight = 1;
                    layersToKeep.Add(lastBaseAnim);
                }
            }
            else
            {
                layersToKeep.AddRange(AnimationLayers.Where(la => !la.IsUpperBody));
            }

            if (removeUpper)
            {
                var lastUpperBodyAnim = AnimationLayers.LastOrDefault(layer => layer.IsUpperBody);

                if (lastUpperBodyAnim != null)
                {
                    lastUpperBodyAnim.Weight = 1;
                    lastUpperBodyAnim.ReferenceWeight = 1;
                    layersToKeep.Add(lastUpperBodyAnim);
                }
            }
            else
            {
                layersToKeep.AddRange(AnimationLayers.Where(la => la.IsUpperBody));
            }



            AnimationLayers.Clear();
            AnimationLayers.AddRange(layersToKeep);
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
                    overlay.Value.Reset(RootMotionTransform.GetRootMotionVector4());
                }
            }

        }

        public void AddRelativeRootMotionRotation(float delta)
        {
            List<NewHavokAnimation> animLayersCopy_Base = null;

            lock (_lock_AnimationLayers)
            {
                animLayersCopy_Base = AnimationLayers.Where(la => !la.IsUpperBody).ToList();
                OnScrubUpdateAnimLayersRootMotion(animLayersCopy_Base, delta);
            }

            
        }

        private void OnScrubUpdateAnimLayersRootMotion(List<NewHavokAnimation> animLayers, float externalRotation)
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
                    if (externalRotation != 0)
                        animLayers[i].ApplyExternalRotation(externalRotation);

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

            var delta = (RootMotionTransformVec - RootMotionTransformVec_Prev);

            //delta *= new NVector4(Main.Config.RootMotionTranslationMultiplierXZ, Main.Config.RootMotionTranslationMultiplierY,
            //        Main.Config.RootMotionTranslationMultiplierXZ, Main.Config.RootMotionRotationMultiplier);

            float signX = delta.X > 0 ? 1 : -1;
            float signY = delta.Y > 0 ? 1 : -1;
            float signZ = delta.Z > 0 ? 1 : -1;
            float signW = delta.W > 0 ? 1 : -1;

            if (delta.X != 0)
                delta.X = (float)Math.Pow(Math.Abs(delta.X), Main.Config.RootMotionTranslationPowerXZ) * signX;

            if (delta.Y != 0)
                delta.Y = (float)Math.Pow(Math.Abs(delta.Y), Main.Config.RootMotionTranslationPowerY) * signY;

            if (delta.Z != 0)
                delta.Z = (float)Math.Pow(Math.Abs(delta.Z), Main.Config.RootMotionTranslationPowerXZ) * signZ;

            if (delta.W != 0)
                delta.W = (float)Math.Pow(Math.Abs(delta.W), Main.Config.RootMotionRotationPower) * signW;

            delta *= new NVector4(Main.Config.RootMotionTranslationMultiplierXZ * TaeEditor.TaeEventSimulationEnvironment.TaeRootMotionScaleXZ, Main.Config.RootMotionTranslationMultiplierY,
                    Main.Config.RootMotionTranslationMultiplierXZ * TaeEditor.TaeEventSimulationEnvironment.TaeRootMotionScaleXZ, Main.Config.RootMotionRotationMultiplier);

            RootMotionTransform *= NewBlendableTransform.FromRootMotionSample(delta);

            RootMotionTransformVec_Prev = RootMotionTransformVec;
        }

        //public bool Paused = false; 

        public NewAnimationContainer()
        {
            //IsPlaying = AutoPlayAnimContainersUponLoading;
        }

        public void ScrubRelative(float timeDelta, bool doNotScrubBackgroundLayers = false)
        {
            //ForcePlayAnim = false;

            //if (stopPlaying)
            //    IsPlaying = false;

            //CurrentAnimation?.Scrub(newTime, false, forceUpdate, loopCount, forceAbsoluteRootMotion);

            if (Skeleton == null)
                return;

            lock (_lock_AdditiveOverlays)
            {
                foreach (var overlay in _additiveBlendOverlays)
                {
                    if (overlay.Value.Weight > 0)
                    {
                        overlay.Value.EnableLooping = true;
                        overlay.Value.ScrubRelative(timeDelta);
                    }
                }
            }

            //Skeleton.RevertToReferencePose();

            float totalWeight = 0;

            List<NewHavokAnimation> animLayersCopy_Base = null;
            List<NewHavokAnimation> animLayersCopy_UpperBody = null;

            lock (_lock_AnimationLayers)
            {
                animLayersCopy_Base = AnimationLayers.Where(la => !la.IsUpperBody).ToList();
                animLayersCopy_UpperBody = AnimationLayers.Where(la => la.IsUpperBody).ToList();

                for (int i = 0; i < animLayersCopy_Base.Count; i++)
                {
                    if (!doNotScrubBackgroundLayers || i == animLayersCopy_Base.Count - 1)
                    {
                        animLayersCopy_Base[i].EnableLooping = EnableLooping;
                        animLayersCopy_Base[i].ScrubRelative(timeDelta);
                    }
                }

                for (int i = 0; i < animLayersCopy_UpperBody.Count; i++)
                {
                    if (!doNotScrubBackgroundLayers || i == animLayersCopy_UpperBody.Count - 1)
                    {
                        animLayersCopy_UpperBody[i].EnableLooping = EnableLooping;
                        animLayersCopy_UpperBody[i].ScrubRelative(timeDelta);
                    }
                }

                for (int t = 0; t < Skeleton.HkxSkeleton.Count; t++)
                {
                    if (animLayersCopy_Base.Count == 0)
                    {
                        Skeleton.HkxSkeleton[t].CurrentHavokTransform = Skeleton.HkxSkeleton[t].RelativeReferenceTransform;

                    }
                    else
                    {
                        var tr = NewBlendableTransform.Identity;
                        float weight = 0;
                        for (int i = 0; i < animLayersCopy_Base.Count; i++)
                        {
                            if (animLayersCopy_Base[i].Weight * DebugAnimWeight <= 0)
                                continue;

                            var frame = animLayersCopy_Base[i].GetBlendableTransformOnCurrentFrame(t);

                            if (animLayersCopy_Base[i].IsAdditiveBlend)
                            {
                                frame = Skeleton.HkxSkeleton[t].RelativeReferenceTransform * frame;
                            }

                            weight += animLayersCopy_Base[i].Weight;
                            if (animLayersCopy_Base.Count > 1)
                                tr = NewBlendableTransform.Lerp(tr, frame, animLayersCopy_Base[i].Weight / weight);
                            else
                                tr = frame;


                        }
                        Skeleton.HkxSkeleton[t].CurrentHavokTransform = NewBlendableTransform.Lerp(Skeleton.HkxSkeleton[t].RelativeReferenceTransform, tr, DebugAnimWeight);
                    }

                    if (Skeleton.HkxSkeleton[t].IsUpperBody)
                    {
                        if (animLayersCopy_UpperBody.Count == 0)
                        {
                            //Skeleton.HkxSkeleton[t].CurrentHavokTransform = Skeleton.HkxSkeleton[t].RelativeReferenceTransform;

                        }
                        else
                        {
                            var tr = NewBlendableTransform.Identity;
                            float weight = 0;
                            for (int i = 0; i < animLayersCopy_UpperBody.Count; i++)
                            {
                                if (animLayersCopy_UpperBody[i].Weight * DebugAnimWeight <= 0)
                                    continue;

                                var frame = animLayersCopy_UpperBody[i].GetBlendableTransformOnCurrentFrame(t);

                                if (animLayersCopy_UpperBody[i].IsAdditiveBlend)
                                {
                                    frame = Skeleton.HkxSkeleton[t].RelativeReferenceTransform * frame;
                                }

                                weight += animLayersCopy_UpperBody[i].Weight;
                                if (animLayersCopy_UpperBody.Count > 1)
                                    tr = NewBlendableTransform.Lerp(tr, frame, animLayersCopy_UpperBody[i].Weight / weight);
                                else
                                    tr = frame;


                            }
                            Skeleton.HkxSkeleton[t].CurrentHavokTransform = NewBlendableTransform.Lerp(Skeleton.HkxSkeleton[t].RelativeReferenceTransform, tr, DebugAnimWeight);
                        }
                    }


                    if (Debug_ForceReferenceBoneLengths)
                    {
                        Skeleton.HkxSkeleton[t].CurrentHavokTransform.Translation = NVector3.Normalize(Skeleton.HkxSkeleton[t].CurrentHavokTransform.Translation) * Skeleton.HkxSkeleton[t].RelativeReferenceTransform.Translation.Length();
                    }
                }

            

                

   
            }

            void WalkTree(int i, Matrix currentMatrix, Matrix scaleMatrix)
            {
                var parentTransformation = currentMatrix;
                var parentScaleMatrix = scaleMatrix;

                var lerpedTransform = (NewBlendableTransform.Lerp(Skeleton.HkxSkeleton[i].RelativeReferenceTransform, Skeleton.HkxSkeleton[i].CurrentHavokTransform, Skeleton.HkxSkeleton[i].Weight));
                currentMatrix = lerpedTransform.GetMatrix().ToXna();

                scaleMatrix = lerpedTransform.GetMatrixScale().ToXna();

                //if (AnimationLayers[0].IsAdditiveBlend && (i >= 0 && i < MODEL.Skeleton.HkxSkeleton.Count))
                //    currentMatrix = MODEL.Skeleton.HkxSkeleton[i].RelativeReferenceMatrix * currentMatrix;

                lock (_lock_AdditiveOverlays)
                {
                    foreach (var overlay in _additiveBlendOverlays)
                    {
                        if (overlay.Value.Weight > 0)
                            currentMatrix *= NewBlendableTransform.Lerp(NewBlendableTransform.Identity, overlay.Value.GetBlendableTransformOnCurrentFrame(i) * NewBlendableTransform.Invert(overlay.Value.data.GetTransformOnFrameByBone(i, 0, false)), overlay.Value.Weight).GetMatrix().ToXna();
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


            //if (timeDelta != 0)
            //    OnScrubUpdateAnimLayersRootMotion(animLayersCopy_Base, externalRotation: 0);
            OnScrubUpdateAnimLayersRootMotion(animLayersCopy_Base, externalRotation: 0);
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
        }

        public static HKX GetHkxStructOfAnim(byte[] hkxBytes, byte[] compendiumBytes)
        {
            HKX hkx = null;

            if (GameRoot.GameTypeIsHavokTagfile)
            {
                hkx = HKX.GenFakeFromTagFile(hkxBytes, compendiumBytes);
            }
            else
            {
                var hkxVariation = GameRoot.GetCurrentLegacyHKXType();

                if (DISABLE_ANIM_LOAD_ERROR_HANDLER)
                {
                    hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                        || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));
                }
                else
                {
                    try
                    {
                        hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                        || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));
                    }
                    catch
                    {

                    }
                }

                if (hkx == null)
                {
                    if (DISABLE_ANIM_LOAD_ERROR_HANDLER)
                    {
                        hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);
                    }
                    else
                    {
                        try
                        {
                            hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);
                        }
                        catch
                        {
                            hkx = null;
                        }
                    }


                }
            }


            return hkx;
        }

        private NewHavokAnimation LoadAnimHKX(byte[] hkxBytes, string name, byte[] compendiumBytes)
        {
            HKX hkx = null;

            if (GameRoot.GameTypeIsHavokTagfile)
            {
                hkx = HKX.GenFakeFromTagFile(hkxBytes, compendiumBytes);
            }
            else
            {
                var hkxVariation = GameRoot.GetCurrentLegacyHKXType();

                if (DISABLE_ANIM_LOAD_ERROR_HANDLER)
                {
                    hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                        || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));
                }
                else
                {
                    try
                    {
                        hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                        || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));
                    }
                    catch
                    {

                    }
                }

                if (hkx == null)
                {
                    if (DISABLE_ANIM_LOAD_ERROR_HANDLER)
                    {
                        hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);
                    }
                    else
                    {
                        try
                        {
                            hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);
                        }
                        catch
                        {
                            hkx = null;
                        }
                    }


                }
            }
            
            

            

            if (hkx == null)
                return null;

            NewHavokAnimation anim = null;

            if (DISABLE_ANIM_LOAD_ERROR_HANDLER)
            {
                anim = LoadAnimHKX(hkx, name, hkxBytes.Length);
            }
            else
            {
                try
                {
                    anim = LoadAnimHKX(hkx, name, hkxBytes.Length);
                }
                catch (Exception fuck)
                {
                    Console.WriteLine("fuck");
                }
            }

            

            if (anim != null)
            {
                if (anim.IsAdditiveBlend)
                {
                    lock (_lock_AdditiveOverlays)
                    {
                        int animID = anim.Name.ExtractDigitsToInt();
                        if (!_additiveBlendOverlays.ContainsKey(animID))
                        {
                            var clone = NewHavokAnimation.Clone(anim, false);
                            clone.Weight = -1;
                            _additiveBlendOverlays.Add(animID, clone);
                        }
                    }


                }
            }

            return anim;
        }

        private void AddCompendiumHkxFetch(string name, byte[] hkx)
        {
            lock (_lock_AnimCacheAndDict)
            {
                if (!compendiumsToLoad.ContainsKey(name))
                    compendiumsToLoad.Add(name, hkx);
            }

        }

        private void AddAnimHKXFetch(string name, byte[] hkx, string compendiumName)
        {
            lock (_lock_AnimCacheAndDict)
            {
                if (!animHKXsToLoad.ContainsKey(name))
                    animHKXsToLoad.Add(name, hkx);
                //else
                //    animHKXsToLoad[name] = hkx;
                if (!string.IsNullOrEmpty(compendiumName))
                {
                    if (!animHKXsCompendiumAssign.ContainsKey(name))
                        animHKXsCompendiumAssign.Add(name, compendiumName);
                }
            }
        }

        private NewHavokAnimation LoadAnimHKX(HKX hkx, string name, int fileSize)
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
                anim =  new NewHavokAnimation_SplineCompressed(name, Skeleton, animRefFrame, animBinding, animSplineCompressed, this, fileSize);
            }
            else if (animInterleavedUncompressed != null)
            {
                anim = new NewHavokAnimation_InterleavedUncompressed(name, Skeleton, animRefFrame, animBinding, animInterleavedUncompressed, this, fileSize);
            }

            return anim;
        }

        public void LoadAdditionalANIBND(IBinder anibnd, IProgress<double> progress, bool scanAnims)
        {
            var hkxVariation = GameRoot.GetCurrentLegacyHKXType();

            if (hkxVariation == HKX.HKXVariation.Invalid)
            {
                //TODO
                Console.WriteLine("NewAnimationContainer.LoadAdditionalANIBND: Invalid legacy HKX type.");
            }
            else
            {
                Dictionary<string, byte[]> animHKXs = new Dictionary<string, byte[]>();
                Dictionary<string, byte[]> taes = new Dictionary<string, byte[]>();

                string compendiumName = null;
                byte[] compendiumBytes = null;

                double i = 1;
                int fileCount = anibnd.Files.Count;
                foreach (var f in anibnd.Files)
                {
                    //DESR .hkt hotfix
                    if (f.Name.ToLowerInvariant().EndsWith(".hkt"))
                        f.Name = f.Name.Substring(0, f.Name.Length - 3) + "hkx";

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
                    else if (shortName.EndsWith(".compendium"))
                    {
                        compendiumName = shortName;
                        compendiumBytes = f.Bytes;
                    }
                    progress?.Report(((i++) / fileCount) / 2.0);
                }

                i = 0;
                fileCount = animHKXs.Count;

                if (compendiumBytes != null)
                {
                    AddCompendiumHkxFetch(compendiumName, compendiumBytes);
                }

                foreach (var kvp in animHKXs)
                {
                    AddAnimHKXFetch(kvp.Key, kvp.Value, compendiumName);

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
                    lock (_lock_AnimCacheAndDict)
                    {
                        firstAnim = animHKXsToLoad.Keys.First();
                    }
                    ChangeToNewAnimation(firstAnim, 1, 0, true);

                    ScrubRelative(0);

                }

            }

            progress?.Report(1.0);
        }

        public void LoadBaseANIBND(IBinder anibnd, IProgress<double> progress)
        {
            var hkxVariation = GameRoot.GetCurrentLegacyHKXType();

            if (hkxVariation == HKX.HKXVariation.Invalid)
            {
                //TODO
                Console.WriteLine("NewAnimationContainer.LoadBaseANIBND: Invalid legacy HKX type.");
            }
            else
            {
                HKX.HKASkeleton hkaSkeleton = null;
                byte[] skeletonHKX = null;
                Dictionary<string, byte[]> animHKXs = new Dictionary<string, byte[]>();
                Dictionary<string, byte[]> taes = new Dictionary<string, byte[]>();

                string compendiumName = null;
                byte[] compendiumBytes = null;

                double i = 1;
                int fileCount = anibnd.Files.Count;
                foreach (var f in anibnd.Files)
                {
                    //DESR .hkt hotfix
                    if (f.Name.ToLowerInvariant().EndsWith(".hkt"))
                        f.Name = f.Name.Substring(0, f.Name.Length - 3) + "hkx";

                    string shortName = new FileInfo(f.Name).Name.ToLower();
                    if (shortName == "skeleton.hkx"  //fatcat
                        || shortName == "skeleton_1.hkx"
                        || shortName == "skeleton_2.hkx"
                        || shortName == "skeleton_3.hkx")
                    {
                        skeletonHKX = f.Bytes;
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
                    else if (shortName.EndsWith(".compendium"))
                    {
                        compendiumName = shortName;
                        compendiumBytes = f.Bytes;
                    }
                    progress.Report(((i++) / fileCount) / 2.0);
                }

                if (compendiumBytes != null)
                {
                    AddCompendiumHkxFetch(compendiumName, compendiumBytes);
                }

                Skeleton = new NewAnimSkeleton_HKX();

                HKX skeletonHkxParsed = null;

                if (GameRoot.GameTypeIsHavokTagfile)
                {
                    skeletonHkxParsed = HKX.GenFakeFromTagFile(skeletonHKX, compendiumBytes);
                }
                else
                {
                    skeletonHkxParsed = HKX.Read(skeletonHKX, hkxVariation, isDS1RAnimHotfix: (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R));
                }

                if (skeletonHkxParsed == null)
                {
                    throw new InvalidOperationException("HKX file had no skeleton.");
                }

                foreach (var o in skeletonHkxParsed.DataSection.Objects)
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

                Skeleton.LoadHKXSkeleton(skeletonHkxParsed);





                i = 0;
                fileCount = animHKXs.Count;

                

                foreach (var kvp in animHKXs)
                {
                    AddAnimHKXFetch(kvp.Key, kvp.Value, compendiumName);

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

                lock (_lock_AnimCacheAndDict)
                {
                    if (animHKXsToLoad.Count > 0)
                    {
                        ChangeToNewAnimation(animHKXsToLoad.Keys.First(), 1, 0, true);
                        var curAnim = CurrentAnimation;
                        if (curAnim != null)
                        {
                            curAnim.ScrubRelative(0);
                        }
                    }
                }
            }

            

            

            progress.Report(1.0);
        }

    }
}
