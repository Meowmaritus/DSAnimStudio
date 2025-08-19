using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using NMatrix = System.Numerics.Matrix4x4;
using NVector3 = System.Numerics.Vector3;
using NVector4 = System.Numerics.Vector4;
using NQuaternion = System.Numerics.Quaternion;
using System.Windows.Forms;

namespace DSAnimStudio
{
    [Flags]
    public enum ScrubTypes : byte
    {
        None = 0,
        Foreground = 1 << 0,
        Background = 1 << 1,
        
        All = Foreground | Background,
    }

    public class NewAnimationContainer
    {
        public zzz_DocumentIns Document;
        public readonly string GUID = Guid.NewGuid().ToString();

        public readonly DSAProj Proj;
        public NewChrAsmWpnTaeManager EquipmentTaeManager = null;

        public Dictionary<NewAnimSlot.SlotTypes, NewAnimSlot.DebugReport> GetAllSlotsDebugReports()
        {

            var result = new Dictionary<NewAnimSlot.SlotTypes, NewAnimSlot.DebugReport>();


            lock (_lock_NewAnimSlots)
            {
                foreach (var kvp in NewAnimSlots)
                {
                    result.Add(kvp.Key, kvp.Value.GetDebugReport(Proj));
                }
            }

            return result;
        }


        public void DrawDebug(ref Vector2 pos)
        {
            float fontSize = 12;
            float verticalAdvanceAfterText = 12;
            float barWidthPerSecond = 128;
            float barHeight = 12;
            float verticalAdvanceAfterBar = 12;
            //Vector2 innerBarOutline = new Vector2(2, 2);

            lock (_lock_NewAnimSlots)
            {
                foreach (var kvp in NewAnimSlots)
                {
                    ImGuiDebugDrawer.DrawText($"{ModelName_ForDebug}->{kvp.Key}", pos, Main.Colors.GuiColorViewportStatus, fontSize: fontSize);
                    pos += new Vector2(0, verticalAdvanceAfterText);

                    var posCapture = pos;

                    kvp.Value.AccessAllAnimations(layer =>
                    {
                        float barWidth = (barWidthPerSecond * layer.Duration);

                        var color = Color.Lerp(Color.Red, Color.Lime, layer.Weight);
                        ImGuiDebugDrawer.DrawText($"{layer.GetID(Proj).GetFormattedIDString(Proj)}", posCapture, color, fontSize: fontSize);
                        posCapture += new Vector2(0, verticalAdvanceAfterText);

                        //ImGuiDebugDrawer.DrawRect(pos, new Vector2(barWidth, barHeight), color);

                        ImGuiDebugDrawer.DrawLine(new Vector2(posCapture.X, posCapture.Y + (barHeight / 2)),
                            new Vector2(posCapture.X + barWidth, posCapture.Y + (barHeight / 2)), color);

                        ImGuiDebugDrawer.DrawLine(new Vector2(posCapture.X + (barWidthPerSecond * layer.CurrentTime), posCapture.Y),
                            new Vector2(posCapture.X + (barWidthPerSecond * layer.CurrentTime), posCapture.Y + (barHeight)), color);

                        posCapture += new Vector2(0, verticalAdvanceAfterBar);
                    });

                    pos = posCapture;
                }
            }
        }






        public NewHavokAnimation NewGetAnimFromRequest(NewAnimSlot.Request request)
        {
            if (EquipmentTaeManager != null)
            {
                var wpnAnim = EquipmentTaeManager?.GetTaeAnimSolveRefs(Proj.ParentDocument, request.TaeAnimID);
                if (wpnAnim != null)
                {
                    var hkxID = wpnAnim.SplitID;
                    wpnAnim.SafeAccessHeader(header =>
                    {
                        if (header is TAE.Animation.AnimFileHeader.Standard asStandard)
                        {
                            if (asStandard.ImportsHKX && asStandard.ImportHKXSourceAnimID >= 0)
                                hkxID = SplitAnimID.FromFullID(Proj, asStandard.ImportHKXSourceAnimID);


                        }
                    });


                    var result = FindAnimation(hkxID);
                    if (result != null)
                    {
                        result.TaeAnimation = wpnAnim;
                    }

                    return result;
                }
            }

            var anim = Proj?.SAFE_GetFirstAnimationFromFullID(request.TaeAnimID);
            if (anim != null)
            {
                var taeAnimActionSrc = Proj.SAFE_SolveAnimRefChain(request.TaeAnimID);
                var hkxID = anim.GetHkxID(Proj);
                if (hkxID.IsValid)
                {
                    var result = FindAnimation(hkxID);
                    if (result != null)
                    {
                        result.TaeAnimation = taeAnimActionSrc;
                    }

                    return result;
                }
            }
            else
            {
                var result = FindAnimation(request.TaeAnimID);

                return result;
            }


            

            return null;
        }

        private Dictionary<NewAnimSlot.SlotTypes, NewAnimSlot> NewAnimSlots = new();
        public object _lock_NewAnimSlots = new object();

        public void AccessAnimSlots(Action<Dictionary<NewAnimSlot.SlotTypes, NewAnimSlot>> access)
        {
            //lock (_lock_NewAnimSlots)
            //{
            //    access(NewAnimSlots);
            //}
            access(NewAnimSlots);
        }

        private void NewInitAnimSlots()
        {
            lock (_lock_NewAnimSlots)
            {
                void addSlot(NewAnimSlot slot)
                {
                    NewAnimSlots.Add(slot.SlotType, slot);
                }
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.Base, this, NewAnimSlot.BlendModes.Normal, boneMask: NewBone.BoneMasks.None, ignoreBlendFromInvalid: true));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.UpperBody, this, NewAnimSlot.BlendModes.Normal, boneMask: NewBone.BoneMasks.UpperBody));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.SekiroFaceAnim, this, NewAnimSlot.BlendModes.Normal, boneMask: NewBone.BoneMasks.SekiroFace, ignoreBlendFromInvalid: true));

                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim0, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim1, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim2, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim3, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim4, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim5, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim6, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim7, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim8, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim9, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.TaeExtraAnim10, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None));

                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.EldenRingHandPose, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None, fixedBlendMode: true, taeEnabled: false));

                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.DebugNormal1, this, NewAnimSlot.BlendModes.Normal, boneMask: NewBone.BoneMasks.None, fixedBlendMode: true));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.DebugAddRelativeToTpose1, this, NewAnimSlot.BlendModes.Additive_RelativeToTPose, boneMask: NewBone.BoneMasks.None, fixedBlendMode: true));
                addSlot(new NewAnimSlot(NewAnimSlot.SlotTypes.DebugAddRelativeToStartFrame1, this, NewAnimSlot.BlendModes.Additive_RelativeToStartFrame, boneMask: NewBone.BoneMasks.None, fixedBlendMode: true));
                
            }
        }

        public void AnyAnimsInAnySlots()
        {

        }

        public bool IsAnyTaeAddAnimWithID(SplitAnimID id)
        {
            for (int i = (int)NewAnimSlot.SlotTypes.TaeExtraAnim0; i <= (int)NewAnimSlot.SlotTypes.TaeExtraAnim10; i++)
            {
                var slotType = (NewAnimSlot.SlotTypes)i;
                if (NewAnimSlots.ContainsKey(slotType))
                {
                    if (NewAnimSlots[slotType].AnyAnimations() && NewAnimSlots[slotType].GetForegroundAnimation()?.TaeAnimation?.SplitID == id)
                        return true;
                }
            }
            return false;
        }

        public void NewSetSlotRequest(NewAnimSlot.SlotTypes slotType, NewAnimSlot.Request request)
        {
            //Testing
            //if (request.TaeAnimID < 0 && slotType == NewAnimSlot.SlotTypes.TaeExtraAnim0)
            //{
            //    Console.WriteLine("test");
            //}

            if (slotType == NewAnimSlot.SlotTypes.None)
                return;
            lock (_lock_NewAnimSlots)
            {
                if (NewAnimSlots[slotType].SetRequest(request, RootMotionTransformVec, out NewHavokAnimation newlyStartedAnim))
                {
                    if (newlyStartedAnim != null && slotType == NewAnimSlot.SlotTypes.Base)
                    {
                        newlyStartedAnim.SyncRootMotion(RootMotionTransformVec);
                    }
                }
            }
            Scrub(absolute: false, 0, foreground: true, background: true, out _);
            //if (!HasAnyValidAnimations())
            //{
            //    Scrub(absolute: false, 0, foreground: true, background: true, out _, forceRefresh: true);
            //}
        }

        public float GetAnimSlotForegroundWeight(NewAnimSlot.SlotTypes slotType)
        {
            float result = -1;
            lock (_lock_NewAnimSlots)
            {
                result = NewAnimSlots[slotType].GetForegroundAnimation()?.Weight ?? -1;
            }

            return result;
        }

        public bool HasAnyValidAnimations()
        {
            lock (_lock_NewAnimSlots)
            {
                foreach (var kvp in NewAnimSlots)
                {
                    var foregroundAnim = kvp.Value.GetForegroundAnimation();
                    if (foregroundAnim != null)
                        return true;
                }
            }
            return false;
        }

        private NewBlendableTransform NewGetBoneTransformFromSlots(int i, Matrix parentMatrix, Matrix parentScaleMatrix)
        {
            var refTransform = Skeleton.Bones[i].ReferenceLocalTransform;
            NewBlendableTransform tr = refTransform;




            lock (_lock_NewAnimSlots)
            {
                foreach (var kvp in NewAnimSlots)
                {
                    bool slotValid = kvp.Value.BoneMask == NewBone.BoneMasks.None ||
                                     (kvp.Value.BoneMask & Skeleton.Bones[i].Masks) == kvp.Value.BoneMask;

                    if (Main.Debug.DebugSoloSlotType != NewAnimSlot.SlotTypes.None)
                        slotValid = kvp.Key == Main.Debug.DebugSoloSlotType;


                    if (!kvp.Value.AnyAnimations())
                        slotValid = false;

                    if (slotValid)
                    {
                        // if (kvp.Value.BlendMode == NewAnimSlot.BlendModes.Normal)
                        // {
                        //     tr = NewBlendableTransform.Lerp(tr, kvp.Value.GetBoneTransform(i, Skeleton, DebugAnimWeight, tr), kvp.Value.SlotWeight);
                        // }
                        // else if (kvp.Value.BlendMode is NewAnimSlot.BlendModes.Additive_RelativeToStartFrame or NewAnimSlot.BlendModes.Additive_RelativeToTPose)
                        // {
                        //     tr = NewBlendableTransform.Lerp(tr, tr * kvp.Value.GetBoneTransform(i, Skeleton, DebugAnimWeight, tr), kvp.Value.SlotWeight);
                        // }
                        var nextTransform =  kvp.Value.GetBoneTransform(i, Skeleton, DebugAnimWeight, tr, parentMatrix, parentScaleMatrix);

                        // Debug
                        //nextTransform.Rotate(new NVector3(0, 0, 0));

                        tr = NewBlendableTransform.Lerp(tr, nextTransform, kvp.Value.SlotWeight);
                    }


                }
            }

            return tr;
        }


        public Model Model_ForDebug;
        public string ModelName_ForDebug => Model_ForDebug?.Name;

        public NewAnimSkeleton_HKX Skeleton = new NewAnimSkeleton_HKX();

        private object _lock_AnimCacheAndDict = new object();


        public bool ForcePlayAnim = false;

        public float DebugAnimWeight = 1;

        public bool ForceDisableAnimLayerSystem = false;

        public bool EnableRootMotionBlending = false;

        public class AnimHkxInfo
        {
            public byte[] HkxBytes;
            public byte[] CompendiumBytes;
        }
        public AnimHkxInfo FindAnimationBytes(SplitAnimID animID)
        {
            var result = new AnimHkxInfo();

            lock (_lock_AnimCacheAndDict)
            {
                if (animID.IsValid)
                {
                    // If anim is in anibnd, then return it
                    if (animHKXsToLoad.ContainsKey(animID))
                    {
                        if (animHKXsCompendiumAssign.ContainsKey(animID))
                        {
                            result.CompendiumBytes = compendiumsToLoad[animHKXsCompendiumAssign[animID]];
                        }
                        result.HkxBytes = animHKXsToLoad[animID];
                    }
                }
            }

            return result;
        }

        public AnimHkxInfo FindAnimationBytes(string animName)
        {
            var result = new AnimHkxInfo();

            lock (_lock_AnimCacheAndDict)
            {
                if (animName != null)
                {
                    foreach (var kvp in animHKXsNameAssign)
                    {
                        if (kvp.Value == animName)
                        {
                            result.HkxBytes = animHKXsToLoad[kvp.Key];
                            if (animHKXsCompendiumAssign.ContainsKey(kvp.Key))
                                result.CompendiumBytes = compendiumsToLoad[animHKXsCompendiumAssign[kvp.Key]];
                            break;
                        }
                    }

                }
            }

            return result;
        }

        public NewHavokAnimation FindAnimation(SplitAnimID animID, bool returnNewInstance = true)
        {
            NewHavokAnimation selectedAnim = null;

            lock (_lock_AnimCacheAndDict)
            {
                if (animID.IsValid)
                {
                    // If anim isn't loaded but is in anibnd, then load it.
                    if (animHKXsToLoad.ContainsKey(animID) && !AnimationCache.ContainsKey(animID))
                    {
                        byte[] compendium = null;
                        if (animHKXsCompendiumAssign.ContainsKey(animID))
                        {
                            compendium = compendiumsToLoad[animHKXsCompendiumAssign[animID]];
                        }
                        var newlyLoadedAnim = LoadAnimHKX(animHKXsToLoad[animID], animID, animHKXsNameAssign[animID], compendium);
                        if (newlyLoadedAnim != null)
                        {
                            AnimationCache.Add(animID, newlyLoadedAnim);
                            selectedAnim = newlyLoadedAnim;
                        }
                    }

                    // If anim is loaded, select it
                    if (returnNewInstance)
                    {
                        if (AnimationCache.ContainsKey(animID))
                            selectedAnim = NewHavokAnimation.Clone(AnimationCache[animID], false);
                    }
                    else
                    {
                        if (AnimationCache.ContainsKey(animID))
                            selectedAnim = AnimationCache[animID];
                    }
                }
            }

            return selectedAnim;
        }

        public bool IsAnimLoaded(SplitAnimID id)
        {
            bool result = false;
            lock (_lock_AnimCacheAndDict)
            {
                result = AnimationCache.ContainsKey(id);
            }
            return result;
        }

        private Dictionary<SplitAnimID, byte[]> animHKXsToLoad = new Dictionary<SplitAnimID, byte[]>();
        private Dictionary<SplitAnimID, string> animHKXsCompendiumAssign = new Dictionary<SplitAnimID, string>();
        private Dictionary<SplitAnimID, string> animHKXsNameAssign = new Dictionary<SplitAnimID, string>();
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

        public void AddNewHKXToLoad(SplitAnimID id, string name, byte[] data, string compendiumName = null)
        {
            lock (_lock_AnimCacheAndDict)
            {


                animHKXsToLoad.Add(id, data);

                animHKXsToLoad[id] = data;
                animHKXsNameAssign[id] = name;

                if (!string.IsNullOrEmpty(compendiumName))
                {
                    animHKXsCompendiumAssign[id] = compendiumName;
                }
            }



            // Clear this anim from loaded anims cache so it will be reloaded upon request.

            // lock (_lock_NewAnimSlots)
            // {
            //     foreach (var kvp in NewAnimSlots)
            //     {
            //         kvp.Value.Clear();
            //     }
            // }

            lock (_lock_AnimCacheAndDict)
            {
                if (AnimationCache.ContainsKey(id))
                    AnimationCache.Remove(id);
            }
        }

        public void AddNewAnimation_Deprecated(SplitAnimID id, string name, NewHavokAnimation anim)
        {
            lock (_lock_AnimCacheAndDict)
            {
                if (animHKXsToLoad.ContainsKey(id))
                    animHKXsToLoad.Remove(id);
            }

            lock (_lock_AnimCacheAndDict)
            {
                if (AnimationCache.ContainsKey(id))
                    AnimationCache.Remove(id);

                AnimationCache.Add(id, anim);
            }
        }

        public IReadOnlyDictionary<SplitAnimID, byte[]> Animations => animHKXsToLoad;

        public Dictionary<SplitAnimID, string> GetAnimationNameMap()
        {
            Dictionary<SplitAnimID, string> result = new();
            lock (_lock_AnimCacheAndDict)
            {
                foreach (var kvp in animHKXsNameAssign)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            return result;
        }

        private Dictionary<SplitAnimID, NewHavokAnimation> AnimationCache = new Dictionary<SplitAnimID, NewHavokAnimation>();

        public bool AnimExists(SplitAnimID animID)
        {
            bool result = false;
            lock (_lock_AnimCacheAndDict)
            {
                result = animHKXsToLoad.Keys.Contains(animID);
            }

            return result;
        }

        public List<SplitAnimID> GetAllAnimationIDs()
        {
            List<SplitAnimID> allAnimIDs = new List<SplitAnimID>();
            lock (_lock_AnimCacheAndDict)
            {
                foreach (var kvp in animHKXsToLoad)
                {
                    allAnimIDs.Add(kvp.Key);
                }
            }
            return allAnimIDs;
        }

        public List<string> GetAllAnimationNames()
        {
            List<string> allAnimNames = new List<string>();
            lock (_lock_AnimCacheAndDict)
            {
                foreach (var kvp in animHKXsNameAssign)
                {
                    allAnimNames.Add(kvp.Value);
                }
            }
            return allAnimNames.OrderBy(x => x).ToList();
        }

        public void ClearAnimationCache()
        {
            lock (_lock_AnimCacheAndDict)
            {
                AnimationCache.Clear();
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


            List<SplitAnimID> allAnimIDs = new List<SplitAnimID>();
            lock (_lock_AnimCacheAndDict)
            {
                foreach (var kvp in animHKXsToLoad)
                    allAnimIDs.Add(kvp.Key);
            }

            Document.LoadingTaskMan.DoLoadingTaskSynchronous("ScanningAllAnimations", "Loading animations...", prog =>
            {
                for (int i = 0; i < allAnimIDs.Count; i++)
                {
                    prog?.Report(1.0 * i / allAnimIDs.Count);
                    FindAnimation(allAnimIDs[i]);
                }
                // ClearAnimation();
                prog?.Report(1.0);
            });



        }

        public void ClearAnimation()
        {
            lock (_lock_NewAnimSlots)
            {
                foreach (var kvp in NewAnimSlots)
                {
                    kvp.Value.Clear();
                }
            }
        }

        public void RequestDefaultAnim()
        {
            if (animHKXsToLoad.Count > 0)
                RequestAnim(NewAnimSlot.SlotTypes.Base, animHKXsToLoad.Keys.First(), true, 1, 0, 0);
        }

        public void CompletelyClearAllSlots(bool clearRootMotion = true)
        {
            var slotTypes = (NewAnimSlot.SlotTypes[])Enum.GetValues(typeof(NewAnimSlot.SlotTypes));
            var req = new NewAnimSlot.Request()
            {

                TaeAnimID = SplitAnimID.Invalid,
                ForceNew = true,
            };

            if (clearRootMotion)
            {
                RootMotionTransform = NewBlendableTransform.Identity;
                RootMotionTransformVec = NVector4.Zero;
                RootMotionTransformVec_Prev = NVector4.Zero;
            }

            lock (_lock_NewAnimSlots)
            {
                foreach (var s in slotTypes)
                {
                    if (s is NewAnimSlot.SlotTypes.None)
                        continue;
                    NewAnimSlots[s].SetRequest(req, RootMotionTransformVec, out _);
                }
            }

        }

        public void RequestAnim(NewAnimSlot.SlotTypes slotType, SplitAnimID animID, bool forceNew, float animWeight, float startTime, float blendDuration, bool? isLoop = null,
            bool startTimeInFrames = false, bool blendDurationInFrames = false)
        {
            ////Testing
            //if (animID < 0)
            //{
            //    Console.WriteLine("test");
            //}

            if (ForceDisableAnimLayerSystem)
            {
                // lol
                animWeight = 1;
                blendDuration = -1;
            }

            NewSetSlotRequest(slotType, new NewAnimSlot.Request()
            {
                TaeAnimID = animID,
                DesiredWeight = animWeight,
                AnimStartTime = startTime,
                BlendDuration = blendDuration,
                EnableLoop = isLoop ?? EnableLooping,
                ForceNew = forceNew,
                AnimStartTimeIsFrames = startTimeInFrames,
                BlendDurationIsFrames = blendDurationInFrames,
            });
        }

        public SplitAnimID CurrentAnimationID => CurrentAnimation?.GetID(Proj) ?? SplitAnimID.Invalid;

        public NewHavokAnimation CurrentAnimation
        {
            get
            {
                NewHavokAnimation result = null;
                lock (_lock_NewAnimSlots)
                {
                    result = NewAnimSlots[NewAnimSlot.SlotTypes.Base].GetForegroundAnimation();
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



        public NewBlendableTransform GetRootMotionTransform(float? moduloUnitZX, float? moduloUnitY, out NVector3 translationDelta)
        {

            var currentTranslation = RootMotionTransform.Translation;
            var prevTranslation = currentTranslation;
            if (moduloUnitZX != null)
            {
                currentTranslation.X = currentTranslation.X % moduloUnitZX.Value;
                currentTranslation.Z = currentTranslation.Z % moduloUnitZX.Value;
            }
            if (moduloUnitY != null)
            {
                currentTranslation.Y = currentTranslation.Y % moduloUnitY.Value;
            }
            var tr = RootMotionTransform;
            translationDelta = currentTranslation - prevTranslation;
            if (translationDelta.LengthSquared() != 0)
            {
                tr.Translation += translationDelta;
                RootMotionTransform = tr;
                var delta4 = new NVector4(translationDelta.X, translationDelta.Y, translationDelta.Z, 0);
                RootMotionTransformVec += delta4;
                RootMotionTransformVec_Prev += delta4;
                lock (_lock_NewAnimSlots)
                {
                    NewAnimSlots[NewAnimSlot.SlotTypes.Base].AccessAllAnimations(anim =>
                    {
                        anim.RootMotionTransformLastFrame += delta4;
                        anim.RootMotion.ApplyExternalTransform(delta4);
                    });
                }
            }
            return tr;
        }

        public void ResetRootMotion()
        {
            RootMotionTransform = NewBlendableTransform.Identity;
            RootMotionTransformVec = NVector4.Zero;
            RootMotionTransformVec_Prev = NVector4.Zero;
            lock (_lock_NewAnimSlots)
            {
                NewAnimSlots[NewAnimSlot.SlotTypes.Base].AccessAllAnimations(al =>
                {
                    //al.Reset(NVector4.Zero);
                    al.RootMotionTransformDelta = NVector4.Zero;
                    al.RootMotionTransformLastFrame = NVector4.Zero;
                    al.RootMotion.SyncTimeAndLocation(NVector4.Zero, al.RootMotion.CurrentTime);
                });
            }
        }

        public void ResetRootMotionYOnly(out float yValue)
        {
            var tr = RootMotionTransform;
            yValue = tr.Translation.Y;
            tr.Translation.Y = 0;
            RootMotionTransform = tr;

            RootMotionTransformVec *= new NVector4(1, 0, 1, 1);

            RootMotionTransformVec_Prev *= new NVector4(1, 0, 1, 1);


            lock (_lock_NewAnimSlots)
            {
                NewAnimSlots[NewAnimSlot.SlotTypes.Base].AccessAllAnimations(al =>
                {
                    var startTransform = al.RootMotion.CurrentTransform * new NVector4(1, 0, 1, 1);
                    //al.Reset(startTransform);
                    al.RootMotionTransformDelta *= new NVector4(1, 0, 1, 1);
                    al.RootMotionTransformLastFrame *= new NVector4(1, 0, 1, 1);
                    al.RootMotion.SyncTimeAndLocation(startTransform, al.RootMotion.CurrentTime);
                });
            }
        }

        public void ResetRootMotionWOnly()
        {
            var tr = RootMotionTransform;
            tr.Rotation = Quaternion.Identity.ToCS();
            RootMotionTransform = tr;

            RootMotionTransformVec *= new NVector4(1, 1, 1, 0);

            RootMotionTransformVec_Prev *= new NVector4(1, 1, 1, 0);


            lock (_lock_NewAnimSlots)
            {
                NewAnimSlots[NewAnimSlot.SlotTypes.Base].AccessAllAnimations(al =>
                {
                    var startTransform = al.RootMotion.CurrentTransform * new NVector4(1, 1, 1, 0);
                    //al.Reset(startTransform);
                    al.RootMotionTransformDelta *= new NVector4(1, 1, 1, 0);
                    al.RootMotionTransformLastFrame *= new NVector4(1, 1, 1, 0);
                    al.RootMotion.SyncTimeAndLocation(startTransform, al.RootMotion.CurrentTime);
                });
            }
        }

        public void MoveRootMotionRelative(NewBlendableTransform transform)
        {
            var deltaToPrev = RootMotionTransformVec_Prev - RootMotionTransformVec;
            RootMotionTransform *= transform;
            var newRootMotionVec4 = RootMotionTransform.GetRootMotionVector4();

            RootMotionTransformVec = newRootMotionVec4;
            RootMotionTransformVec_Prev = newRootMotionVec4 + deltaToPrev;
            lock (_lock_NewAnimSlots)
            {
                NewAnimSlots[NewAnimSlot.SlotTypes.Base].AccessAllAnimations(al =>
                {
                    //al.Reset(newRootMotionVec4);
                    al.RootMotionTransformDelta = NVector4.Zero;
                    al.RootMotionTransformLastFrame = NVector4.Zero;
                    al.RootMotion.SyncTimeAndLocation(newRootMotionVec4, al.RootMotion.CurrentTime);
                });
            }
        }



        public void RemoveTransition()
        {
            lock (_lock_NewAnimSlots)
            {
                foreach (var anim in NewAnimSlots)
                {
                    anim.Value.ClearBackgroundSlots();
                }

            }
        }

        public void ResetAll()
        {
            
            lock (_lock_NewAnimSlots)
            {
                foreach (var anim in NewAnimSlots)
                {
                    anim.Value.ResetToStart(RootMotionTransform.GetRootMotionVector4());
                }
                
            }
            
            Scrub(absolute: false, time: 0, foreground: true, background: true, out _, forceRefresh: true);
            PopSyncTime();
        }

        public void NewSetBlendDurationOfSlot(NewAnimSlot.SlotTypes slotType, SplitAnimID matchID, float blendDuration, bool isInFrames)
        {
            if (!matchID.IsValid || matchID == NewAnimSlots[slotType].GetForegroundAnimation()?.GetID(Proj))
                NewAnimSlots[slotType].ChangeBlendDurationOfCurrentRequest(blendDuration, isInFrames);
        }

        public void AddRelativeRootMotionRotation(float delta)
        {
            OnScrubUpdateAnimLayersRootMotion(delta);
            
        }

        private void OnScrubUpdateAnimLayersRootMotion(float externalRotation)
        {
            if (EnableRootMotionBlending)
            {
                lock (_lock_NewAnimSlots)
                {
                    if (NewAnimSlots[NewAnimSlot.SlotTypes.Base].AnyAnimations())
                    {
                        float totalWeight = 0;

                        NewAnimSlots[NewAnimSlot.SlotTypes.Base].AccessAllAnimations(anim =>
                        {
                            if (externalRotation != 0)
                            {
                                anim.ApplyExternalRotation(externalRotation);
                            }

                            if (anim.Weight * DebugAnimWeight <= 0)
                                return;

                            totalWeight += anim.Weight;
                            if (totalWeight > 0)
                            {
                                RootMotionTransformVec = NVector4.Lerp(RootMotionTransformVec,
                                    anim.RootMotion.CurrentTransform, (anim.Weight / totalWeight) * DebugAnimWeight);
                            }
                        });

                        NewAnimSlots[NewAnimSlot.SlotTypes.Base].AccessAllAnimations(anim =>
                        {
                            anim.RootMotion.ApplyExternalTransformSuchThatCurrentTransformMatches(RootMotionTransformVec);
                        });

                    }
                    else
                    {
                        ResetRootMotion();
                    }
                }
            }
            else
            {
                
                lock (_lock_NewAnimSlots)
                {
                    if (NewAnimSlots[NewAnimSlot.SlotTypes.Base].AnyAnimations())
                    {
                        var foreground = NewAnimSlots[NewAnimSlot.SlotTypes.Base].GetForegroundAnimation();

                        if (externalRotation != 0)
                        {
                            foreground.ApplyExternalRotation(externalRotation);
                        }

                        RootMotionTransformVec = foreground.RootMotion.CurrentTransform;
                    }
                }
                
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

            delta *= new NVector4(Main.Config.RootMotionTranslationMultiplierXZ * TaeEditor.TaeActionSimulationEnvironment.TaeRootMotionScaleXZ, Main.Config.RootMotionTranslationMultiplierY,
                    Main.Config.RootMotionTranslationMultiplierXZ * TaeEditor.TaeActionSimulationEnvironment.TaeRootMotionScaleXZ, Main.Config.RootMotionRotationMultiplier);

            RootMotionTransform *= NewBlendableTransform.FromRootMotionSample(delta);

            RootMotionTransformVec_Prev = RootMotionTransformVec;
            
        }



        //public bool Paused = false; 

        public NewAnimationContainer(DSAProj proj, Model model_ForDebug, zzz_DocumentIns document)
        {
            Document = document;
            Proj = proj;
            Model_ForDebug = model_ForDebug;
            //IsPlaying = AutoPlayAnimContainersUponLoading;
            
            NewInitAnimSlots();
        }

        public static bool GLOBAL_SYNC_FORCE_REFRESH = false;

        //public bool NeedsSkeletonRecalculated { get; private set; } = false;

        //private object _lock_skeletonCalculation = new object();

        private float? currentSyncTime = null;
        private object _lock_syncTime = new object();

        public float? PopSyncTime()
        {
            float? result = null;
            lock (_lock_syncTime)
            {
                result = currentSyncTime;
                currentSyncTime = null;
            }
            return result;
        }

        public void Scrub(bool absolute, float time, bool foreground, bool background, 
            out float timeDelta, bool baseSlotOnly = false, bool forceRefresh = false,
            bool ignoreRootMotion = false)
        {
            timeDelta = 0;

            if (GLOBAL_SYNC_FORCE_REFRESH)
                forceRefresh = true;

            if (Skeleton == null)
            {
                return;
            }

            if (forceRefresh)
            {
                // if (Main.IsDebugBuild)
                //     Console.WriteLine("Test");
            }

            lock (_lock_NewAnimSlots)
            {
                foreach (var kvp in NewAnimSlots)
                {
                    if (baseSlotOnly && kvp.Key != NewAnimSlot.SlotTypes.Base)
                        continue;

                    kvp.Value.ChangeIsLoopOfCurrentRequest(EnableLooping);

                    bool slotValid = kvp.Value.AnyAnimations();

                    if (Main.Debug.DebugSoloSlotType != NewAnimSlot.SlotTypes.None)
                        slotValid = kvp.Key == Main.Debug.DebugSoloSlotType;


                    if (slotValid || forceRefresh)
                    {
                        try
                        {
                            kvp.Value.Scrub(absolute, time, foreground, background, 
                                out float slotDelta, out float? syncTime, ignoreRootMotion);
                            // GLOBAL_SYNC_FORCE_REFRESH overrides the local sync of the active animation layer.
                            if (syncTime != null && kvp.Key == NewAnimSlot.SlotTypes.Base && !GLOBAL_SYNC_FORCE_REFRESH)
                            {
                                lock (_lock_syncTime)
                                {
                                    currentSyncTime = syncTime;

                                    //if (Main.IsDebugBuild && currentSyncTime.HasValue && currentSyncTime < 0.1f)
                                    //{
                                    //    Console.WriteLine("test");
                                    //}
                                }
                            }
                            if (Math.Abs(slotDelta) > Math.Abs(timeDelta))
                                timeDelta = slotDelta;
                        }
                        catch
                        {

                        }
                    }
                }


                if (!timeDelta.ApproxEquals(0) || forceRefresh)
                {
                    Skeleton.CalculateFKFromLocalTransforms(NewGetBoneTransformFromSlots);
                }
            }

            OnScrubUpdateAnimLayersRootMotion(externalRotation: 0);
        }

        //public void CalculateSkeletonForCurrentFrame()
        //{
        //    lock (_lock_skeletonCalculation)
        //    {
        //        if (NeedsSkeletonRecalculated)
        //        {
        //            Skeleton.CalculateFKFromLocalTransforms(NewGetBoneTransformFromSlots);
        //            NeedsSkeletonRecalculated = false;
        //        }
        //    }
        //}

        

        public void Update()
        {
            //V2.0 TODO: If this is ever gonna be used, actually implement this, using .Scrub
            //           to simulate the old behavior of .Play
            if (CurrentAnimation != null)
            {
                if (ForcePlayAnim)
                    Scrub(absolute: false, Main.DELTA_UPDATE, foreground: true, background: true, out _);
            }
            else
            {
                Skeleton.RevertToReferencePose();
                Skeleton.CalculateFKFromLocalTransforms();
            }
        }

        public HKX GetHkxStructOfAnim(byte[] hkxBytes, byte[] compendiumBytes)
        {
            HKX hkx = null;

            if (Document.GameRoot.GameTypeIsHavokTagfile)
            {
                hkx = HKX.GenFakeFromTagFile(hkxBytes, compendiumBytes);
            }
            else
            {
                var hkxVariation = Document.GameRoot.GetCurrentLegacyHKXType();

                try
                {
                    hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                                                                              || Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));
                }
                catch (Exception handled_ex) when (Main.EnableErrorHandler.LoadHKX)
                {
                    Main.HandleError(nameof(Main.EnableErrorHandler.LoadHKX), handled_ex);
                }

                if (hkx == null)
                {
                    try
                    {
                        hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);
                    }
                    catch (Exception handled_ex) when (Main.EnableErrorHandler.LoadHKX)
                    {
                        Main.HandleError(nameof(Main.EnableErrorHandler.LoadHKX), handled_ex);
                        hkx = null;
                    }


                }
            }


            return hkx;
        }

        private NewHavokAnimation LoadAnimHKX(byte[] hkxBytes, SplitAnimID id, string name, byte[] compendiumBytes)
        {
            HKX hkx = null;

            if (Document.GameRoot.GameTypeIsHavokTagfile)
            {
                hkx = HKX.GenFakeFromTagFile(hkxBytes, compendiumBytes);
            }
            else
            {
                var hkxVariation = Document.GameRoot.GetCurrentLegacyHKXType();

                try
                {
                    hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: (Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R
                                                                              || Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT));
                }
                catch (Exception handled_ex) when (Main.EnableErrorHandler.LoadHKX)
                {
                    Main.HandleError(nameof(Main.EnableErrorHandler.LoadHKX), handled_ex);
                }
                if (hkx == null)
                {
                    try
                    {
                        hkx = HKX.Read(hkxBytes, hkxVariation, isDS1RAnimHotfix: false);
                    }
                    catch (Exception handled_ex) when (Main.EnableErrorHandler.LoadHKX)
                    {
                        Main.HandleError(nameof(Main.EnableErrorHandler.LoadHKX), handled_ex);
                        hkx = null;
                    }


                }
            }
            
            

            

            if (hkx == null)
                return null;

            NewHavokAnimation anim = null;

            try
            {
                anim = LoadAnimHKX(hkx, id, name, hkxBytes.Length);
            }
            catch (Exception handled_ex) when (Main.EnableErrorHandler.LoadHKX)
            {
                Main.HandleError(nameof(Main.EnableErrorHandler.LoadHKX), handled_ex);
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

        private void AddAnimHKXFetch(SplitAnimID id, byte[] hkx, string compendiumName)
        {
            lock (_lock_AnimCacheAndDict)
            {
                if (!animHKXsToLoad.ContainsKey(id))
                    animHKXsToLoad.Add(id, hkx);
                //else
                //    animHKXsToLoad[name] = hkx;
                if (!string.IsNullOrEmpty(compendiumName))
                {
                    if (!animHKXsCompendiumAssign.ContainsKey(id))
                        animHKXsCompendiumAssign.Add(id, compendiumName);
                }
            }
        }

        private NewHavokAnimation LoadAnimHKX(HKX hkx, SplitAnimID id, string name, int fileSize)
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
                anim =  new NewHavokAnimation_SplineCompressed(id.GetFullID(Proj), name, Skeleton, animRefFrame, animBinding, animSplineCompressed, this, fileSize);
            }
            else if (animInterleavedUncompressed != null)
            {
                anim = new NewHavokAnimation_InterleavedUncompressed(id.GetFullID(Proj), name, Skeleton, animRefFrame, animBinding, animInterleavedUncompressed, this, fileSize);
            }

            return anim;
        }

        public void LoadAdditionalANIBND(IBinder anibnd, IProgress<double> progress, bool scanAnims)
        {
            var hkxVariation = Document.GameRoot.GetCurrentLegacyHKXType();

            if (hkxVariation == HKX.HKXVariation.Invalid)
            {
                //TODO
                Console.WriteLine("NewAnimationContainer.LoadAdditionalANIBND: Invalid legacy HKX type.");
            }
            else
            {
                Dictionary<int, byte[]> animHKXs = new Dictionary<int, byte[]>();
                Dictionary<int, string> animHkxNames = new Dictionary<int, string>();

                string compendiumName = null;
                byte[] compendiumBytes = null;

                
                int animBindIDMin = (Document.GameRoot.GameType is SoulsGames.DES or SoulsGames.DS1 or SoulsGames.DS1R) ? 0 : 1000000000;
                int animBindIDMax = animBindIDMin + ((Document.GameRoot.GameType is SoulsGames.DES or SoulsGames.DS1 or SoulsGames.DS1R) ? 255_9999 : 999_999999);
                
                double i = 1;
                int fileCount = anibnd.Files.Count;
                foreach (var f in anibnd.Files)
                {
                    if (f.ID >= animBindIDMin && f.ID <= animBindIDMax)
                    {
                        animHKXs[f.ID % 1_000_000000] = f.Bytes;
                        animHkxNames[f.ID % 1_000_000000] = Path.GetFileName(f.Name);
                    }
                    else if (f.ID == 7000000 && Document.GameRoot.GameTypeIsHavokTagfile)
                    {
                        compendiumName = f.Name;
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
                    AddAnimHKXFetch(SplitAnimID.FromFullID(Proj, kvp.Key), kvp.Value, compendiumName);

                    progress?.Report(0.5 + (((i++) / fileCount) / 2.0));
                }

                lock (_lock_AnimCacheAndDict)
                {
                    foreach (var kvp in animHkxNames)
                    {
                        animHKXsNameAssign[SplitAnimID.FromFullID(Proj, kvp.Key)] = kvp.Value;
                    }
                }

                if (scanAnims)
                {
                    ScanAllAnimations();
                }
                

                if (!CurrentAnimationID.IsValid && animHKXsToLoad.Count > 0)
                {
                    SplitAnimID firstAnim = SplitAnimID.Invalid;
                    lock (_lock_AnimCacheAndDict)
                    {
                        firstAnim = animHKXsToLoad.Keys.First();
                    }
                    RequestAnim(NewAnimSlot.SlotTypes.Base, firstAnim, forceNew: true, animWeight: 1, startTime: 0, blendDuration: 0);

                }

            }

            progress?.Report(1.0);
        }

        public void LoadBaseANIBND(IBinder anibnd, IProgress<double> progress)
        {
            var hkxVariation = Document.GameRoot.GetCurrentLegacyHKXType();

            if (hkxVariation == HKX.HKXVariation.Invalid)
            {
                //TODO
                Console.WriteLine("NewAnimationContainer.LoadBaseANIBND: Invalid legacy HKX type.");
            }
            else
            {
                HKX.HKASkeleton hkaSkeleton = null;
                byte[] skeletonHKX = null;
                
                Dictionary<int, byte[]> animHKXs = new Dictionary<int, byte[]>();
                Dictionary<int, string> animHkxNames = new Dictionary<int, string>();

                string compendiumName = null;
                byte[] compendiumBytes = null;

                
                int animBindIDMin = (Document.GameRoot.GameType is SoulsGames.DES or SoulsGames.DS1 or SoulsGames.DS1R) ? 0 : 1000000000;
                int animBindIDMax = animBindIDMin + ((Document.GameRoot.GameType is SoulsGames.DES or SoulsGames.DS1 or SoulsGames.DS1R) ? 255_9999 : 999_999999);

                bool ver_0001 = anibnd.Files.Any(f => f.ID == 9999999) && Document.GameRoot.GameType != SoulsGames.DES;
                
                int skeletonBindID = ver_0001 ? 4000000 : 1000000;

                //lol, lmao, 
                if (animBindIDMin < skeletonBindID && animBindIDMax >= skeletonBindID)
                {
                    animBindIDMax = skeletonBindID - 1;
                }
                
                double i = 1;
                int fileCount = anibnd.Files.Count;
                foreach (var f in anibnd.Files)
                {
                    if (f.ID >= animBindIDMin && f.ID <= animBindIDMax)
                    {
                        animHKXs[f.ID % 1_000_000000] = f.Bytes;
                        animHkxNames[f.ID % 1_000_000000] = Path.GetFileName(f.Name);
                    }
                    else if (f.ID == 7000000 && Document.GameRoot.GameTypeIsHavokTagfile)
                    {
                        compendiumName = f.Name;
                        compendiumBytes = f.Bytes;
                    }
                    else if (f.ID == skeletonBindID)
                    {
                        skeletonHKX = f.Bytes;
                    }
                    
                    progress.Report(((i++) / fileCount) / 2.0);
                }

                if (compendiumBytes != null)
                {
                    AddCompendiumHkxFetch(compendiumName, compendiumBytes);
                }

                Skeleton = new NewAnimSkeleton_HKX();

                HKX skeletonHkxParsed = null;

                if (Document.GameRoot.GameTypeIsHavokTagfile)
                {
                    skeletonHkxParsed = HKX.GenFakeFromTagFile(skeletonHKX, compendiumBytes);
                }
                else
                {
                    skeletonHkxParsed = HKX.Read(skeletonHKX, hkxVariation, isDS1RAnimHotfix: (Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R));
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
                    AddAnimHKXFetch(SplitAnimID.FromFullID(Proj, kvp.Key), kvp.Value, compendiumName);

                    progress.Report(0.5 + (((i++) / fileCount) / 2.0));
                }
                
                lock (_lock_AnimCacheAndDict)
                {
                    foreach (var kvp in animHkxNames)
                    {
                        animHKXsNameAssign[SplitAnimID.FromFullID(Proj, kvp.Key)] = kvp.Value;
                    }
                }

                //ScanAllAnimations();

                // lock (_lock_AnimCacheAndDict)
                // {
                //     if (animHKXsToLoad.Count > 0)
                //     {
                //         ChangeToNewAnimation(animHKXsToLoad.Keys.First(), 1, 0, true);
                //         var curAnim = CurrentAnimation;
                //         if (curAnim != null)
                //         {
                //             curAnim.ScrubRelative(0);
                //         }
                //     }
                // }
            }

            

            

            progress.Report(1.0);
        }

    }
}
