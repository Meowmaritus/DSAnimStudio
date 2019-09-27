using Microsoft.Xna.Framework;
using SoulsFormats;
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

        private object _lock_timeactDict = new object();

        private Dictionary<string, byte[]> timeactFiles = new Dictionary<string, byte[]>();

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

        private string lastLoadedAnimName = null;
        private NewHavokAnimation lastLoadedAnim;

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
                            if (MODEL.ChrAsm.RightWeaponModel.AnimContainer.Animations.ContainsKey(value))
                                MODEL.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = value;
                            //else
                            //    MODEL.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName
                            //        = MODEL.ChrAsm.RightWeaponModel.AnimContainer.Animations.Keys.First();
                        }
                    }

                    if (MODEL.ChrAsm.LeftWeaponModel != null)
                    {
                        if (MODEL.ChrAsm.LeftWeaponModel.AnimContainer.Animations.Count > 0)
                        {
                            if (MODEL.ChrAsm.LeftWeaponModel.AnimContainer.Animations.ContainsKey(value))
                                MODEL.ChrAsm.LeftWeaponModel.AnimContainer.CurrentAnimationName = value;
                            //else
                            //    MODEL.ChrAsm.LeftWeaponModel.AnimContainer.CurrentAnimationName
                            //        = MODEL.ChrAsm.LeftWeaponModel.AnimContainer.Animations.Keys.First();
                        }
                    }
                }
            }
        }

        public NewHavokAnimation CurrentAnimation
        {
            get
            {
                NewHavokAnimation anim = null;
                lock (_lock_animDict)
                {
                    if (CurrentAnimationName != null)
                    {
                        if (lastLoadedAnimName != CurrentAnimationName)
                        {
                            lastLoadedAnim?.Scrub(0, false, forceUpdate: true);

                            if (animHKXsToLoad.ContainsKey(CurrentAnimationName))
                            {
                                LoadAnimHKX(animHKXsToLoad[CurrentAnimationName]);
                                lastLoadedAnimName = CurrentAnimationName;
                            }
                            else
                            {
                                lastLoadedAnim = null;
                            }
                        }
                    }
                    else
                    {
                        lastLoadedAnim?.Scrub(0, false, forceUpdate: true);
                        lastLoadedAnim = null;
                    }

                    anim = lastLoadedAnim;
                }
                return anim;
            }
        }

        public float CurrentAnimTime => CurrentAnimation?.CurrentTime ?? 0;
        public float? CurrentAnimDuration => CurrentAnimation?.Duration;

        public float? CurrentAnimFrameDuration => CurrentAnimation?.FrameDuration;

        public static bool AutoPlayAnimContainersUponLoading = true;

        public bool IsPlaying = true;
        public bool IsLoop = true;

        public Matrix CurrentAnimRootMotionMatrix => CurrentAnimation?.CurrentRootMotionMatrix ?? Matrix.Identity;

        public bool Paused = false; 

        public NewAnimationContainer(Model mdl)
        {
            MODEL = mdl;
            IsPlaying = AutoPlayAnimContainersUponLoading;
        }

        public void ScrubCurrentAnimation(float newTime, bool forceUpdate = false, bool stopPlaying = true)
        {
            if (stopPlaying)
                IsPlaying = false;

            CurrentAnimation?.Scrub(newTime, IsLoop, forceUpdate);
        }

        public void Update()
        {
            if (IsPlaying && CurrentAnimation != null)
            {
                CurrentAnimation.Play(Main.DELTA_UPDATE, IsLoop);
            }
        }

        private void LoadAnimHKX(byte[] hkxBytes)
        {
            var hkxVariation = GameDataManager.GetCurrentLegacyHKXType();
            var hkx = HKX.Read(hkxBytes, hkxVariation);
            LoadAnimHKX(hkx);
        }

        private void AddAnimHKXFetch(string name, byte[] hkx)
        {
            if (!animHKXsToLoad.ContainsKey(name))
                animHKXsToLoad.Add(name, hkx);
            else
                animHKXsToLoad[name] = hkx;
        }

        private void LoadAnimHKX(HKX hkx)
        {
            HKX.HKASplineCompressedAnimation anim = null;
            HKX.HKAAnimationBinding animBinding = null;
            HKX.HKADefaultAnimatedReferenceFrame animRefFrame = null;

            foreach (var cl in hkx.DataSection.Objects)
            {
                if (cl is HKX.HKASplineCompressedAnimation asAnim)
                {
                    anim = asAnim;
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

            if (anim != null)
            {
                lastLoadedAnim = new NewHavokAnimation_SplineCompressed(MODEL.Skeleton, animRefFrame, animBinding, anim);
            }
            else
            {
                lastLoadedAnim = null;
            }
        }

        public void LoadAdditionalANIBND(IBinder anibnd, IProgress<double> progress)
        {
            var hkxVariation = GameDataManager.GetCurrentLegacyHKXType();

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

                CurrentAnimation?.Scrub(0, loop: false, forceUpdate: true);
            }

            progress?.Report(1.0);
        }

        public void LoadBaseANIBND(IBinder anibnd, IProgress<double> progress)
        {
            var hkxVariation = GameDataManager.GetCurrentLegacyHKXType();

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
                    animHKXs.Add(shortName, f.Bytes);
                }
                else if (shortName.EndsWith(".tae") || TAE.Is(f.Bytes))
                {
                    taes.Add(shortName, f.Bytes);
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
                    CurrentAnimation?.Scrub(0, loop: false, forceUpdate: true);
                }
            }

            progress.Report(1.0);
        }

    }
}
