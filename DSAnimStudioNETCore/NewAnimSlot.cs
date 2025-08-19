using System;
using System.Collections.Generic;
using System.Linq;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using Vector4 = System.Numerics.Vector4;

namespace DSAnimStudio
{
    public class NewAnimSlot
    {
        public struct Request
        {
            public SplitAnimID TaeAnimID;
            public bool ForceNew;
            public float BlendDuration;
            public bool BlendDurationIsFrames;
            public float DesiredWeight;
            public float AnimStartTime;
            public bool AnimStartTimeIsFrames;
            public bool EnableLoop;
            public bool ClearOnEnd;
            public bool KeepPlayingUntilEnd;
            public float NF_RequestedLerpS;
            public float NF_EvInputLerpS;
            public float NF_WeightAtEvStart;

            public static Request Empty => new Request()
            {
                TaeAnimID = SplitAnimID.Invalid,
            };

            public static Request ForceClear => new Request()
            {
                TaeAnimID = SplitAnimID.Invalid,
                ForceNew = true,
            };
        }
        
        public static Request ShowImguiWidgetForRequest(ref SlotTypes slotType, Request request, NewAnimationContainer container)
        {
            ImGui.PushID("Debug.AnimSlotRequest");
            
            ImGui.InputInt($"{nameof(request.TaeAnimID)}.{nameof(request.TaeAnimID.CategoryID)}", ref request.TaeAnimID.CategoryID);
            ImGui.InputInt($"{nameof(request.TaeAnimID)}.{nameof(request.TaeAnimID.SubID)}", ref request.TaeAnimID.SubID);

            Tools.EnumPicker("SlotType", ref slotType);
            ImGui.Checkbox(nameof(request.ForceNew), ref request.ForceNew);
            ImGui.InputFloat(nameof(request.BlendDuration), ref request.BlendDuration);
            ImGui.Checkbox(nameof(request.BlendDurationIsFrames), ref request.BlendDurationIsFrames);
            ImGui.InputFloat(nameof(request.DesiredWeight), ref request.DesiredWeight);
            ImGui.InputFloat(nameof(request.AnimStartTime), ref request.AnimStartTime);
            ImGui.Checkbox(nameof(request.AnimStartTimeIsFrames), ref request.AnimStartTimeIsFrames);
            ImGui.Checkbox(nameof(request.EnableLoop), ref request.EnableLoop);
            ImGui.Checkbox(nameof(request.ClearOnEnd), ref request.ClearOnEnd);
            ImGui.Checkbox(nameof(request.KeepPlayingUntilEnd), ref request.KeepPlayingUntilEnd);

            if (Tools.SimpleClickButton("Request"))
            {
                container.NewSetSlotRequest(slotType, request);
            }
            
            ImGui.PopID();
            return request;
        }
        
        public class DebugReportAnimEntry
        {
            public SplitAnimID ID;
            public string Name;
            public float Weight;
            public float Time;
            public int LoopCount;
            public float PrevTime;
            public float PrevLoopCount;
            public float Duration;
            public float FrameDuration;
            public bool IsLoop;
            public int AnimFileSize;
        }

        public class DebugReport
        {
            public List<DebugReportAnimEntry> AnimEntries = new List<DebugReportAnimEntry>();
        }
        
        public NewHavokAnimation GetForegroundAnimation()
        {
            return ForegroundAnimation;
        }

        private DebugReportAnimEntry GetDebugReportOfAnim(DSAProj proj, NewHavokAnimation anim)
        {
            var report = new DebugReportAnimEntry();
            report.ID = anim.GetID(proj);
            report.Name = anim.Name;
            report.Weight = anim.Weight;
            report.Time = anim.CurrentTime;
            report.LoopCount = anim.LoopCount;
            report.PrevTime = anim.TaePrevTime;
            report.PrevLoopCount = anim.TaePrevLoopCount;
            report.Duration = anim.Duration;
            report.FrameDuration = anim.FrameDuration;
            report.IsLoop = anim.EnableLooping;
            report.AnimFileSize = anim.FileSize;
            return report;
        }
        
        public DebugReport GetDebugReport(DSAProj proj)
        {
            var entries = new List<DebugReportAnimEntry>();
            if (ForegroundAnimation != null)
            {
                entries.Add(GetDebugReportOfAnim(proj, ForegroundAnimation));
            }

            foreach (var bg in BackgroundSlots)
            {
                entries.Add(GetDebugReportOfAnim(proj, bg.Anim));
            }

            //entries = entries.OrderBy(x => (long)x.ID).ToList();

            var result = new DebugReport()
            {
                AnimEntries = entries,
            };
            return result;
        }
        
        

        public class BackgroundAnimSlot
        {
            public float WeightSnapshot;
            public Request OrigRequest;
            public NewHavokAnimation Anim;
        }

        public enum SlotTypes
        {
            None = 0,
            Base = 1,
            UpperBody = 2,
            SekiroFaceAnim = 100,
            TaeExtraAnim0 = 200,
            TaeExtraAnim1 = 201,
            TaeExtraAnim2 = 202,
            TaeExtraAnim3 = 203,
            TaeExtraAnim4 = 204,
            TaeExtraAnim5 = 205,
            TaeExtraAnim6 = 206,
            TaeExtraAnim7 = 207,
            TaeExtraAnim8 = 208,
            TaeExtraAnim9 = 209,
            TaeExtraAnim10 = 210,
            EldenRingHandPose = 300,
            DebugNormal1 = 1000,
            DebugAddRelativeToTpose1 = 1100,
            DebugAddRelativeToStartFrame1 = 1200,
        }

        public enum BlendModes
        {
            Normal,
            Additive_RelativeToStartFrame,
            Additive_RelativeToTPose,
        }


        public readonly SlotTypes SlotType;
        public readonly NewAnimationContainer Container;
        public BlendModes BlendMode;
        public bool FixedBlendMode = false;
        public readonly NewBone.BoneMasks BoneMask;
        public float SlotWeight = 1;
        public bool IgnoreBlendFromInvalid = false;
        
        public bool TaeEnabled = true;
        
        
        public NewAnimSlot(SlotTypes slotType, NewAnimationContainer container, BlendModes blendMode, NewBone.BoneMasks boneMask, bool ignoreBlendFromInvalid = false, bool taeEnabled = true, bool fixedBlendMode = false)
        {
            SlotType = slotType;
            Container = container;
            BlendMode = blendMode;
            BoneMask = boneMask;
            IgnoreBlendFromInvalid = ignoreBlendFromInvalid;
            TaeEnabled = taeEnabled;
            FixedBlendMode = fixedBlendMode;
        }

        private List<BackgroundAnimSlot> BackgroundSlots = new List<BackgroundAnimSlot>();

        private void UpdateBackgroundSlots(float backgroundWeight, bool absolute, float time, bool ignoreRootMotion)
        {
            float totalWeights = BackgroundSlots.Sum(x => x.WeightSnapshot);
            foreach (var bg in BackgroundSlots)
            {
                bg.Anim.Scrub(absolute, time, out _, out _, ignoreRootMotion);
                if (totalWeights > 0)
                    bg.Anim.Weight = (bg.WeightSnapshot / totalWeights) * backgroundWeight;
                else
                    bg.Anim.Weight = 0;
            }
        }

        private Request CurrentRequest = new Request()
        {
            TaeAnimID = SplitAnimID.Invalid,
            BlendDuration = 0,
        };

        private NewHavokAnimation ForegroundAnimation = null;

        public void ChangeBlendDurationOfCurrentRequest(float blendDuration, bool isInFrames)
        {
            CurrentRequest.BlendDuration = blendDuration;
            CurrentRequest.BlendDurationIsFrames = isInFrames;
        }

        public void ChangeIsLoopOfCurrentRequest(bool isLoop)
        {
            CurrentRequest.EnableLoop = isLoop;
        }
        
        public bool SetRequest(Request req, System.Numerics.Vector4 currentRootMotion, out NewHavokAnimation newlyStartedAnim)
        {
            newlyStartedAnim = null;
            if ((CurrentRequest.KeepPlayingUntilEnd && !req.ForceNew) && ForegroundAnimation != null &&
                ForegroundAnimation.CurrentTime < ForegroundAnimation.Duration)
                return false;

            bool result = false;

            var prevReq = CurrentRequest;
            CurrentRequest = req;

            var newForegroundAnimation = Container.NewGetAnimFromRequest(req);

            if (CurrentRequest.BlendDuration < 0 && newForegroundAnimation?.TaeAnimation != null)
            {
                CurrentRequest.BlendDuration = newForegroundAnimation.TaeAnimation.SAFE_GetBlendDuration();
            }
            
            if (!CurrentRequest.TaeAnimID.IsValid)
            {
                ForegroundAnimation = null;
                BackgroundSlots.Clear();
            }
            else if (CurrentRequest.TaeAnimID != prevReq.TaeAnimID || CurrentRequest.ForceNew)
            {
                // if (CurrentRequest.TaeAnimID == 0)
                // {
                //     Console.WriteLine("Test");
                // }
                
                float weightForNewForegroundAnim = 0;
                if (CurrentRequest.BlendDuration <= 0)
                {
                    BackgroundSlots.Clear();
                    weightForNewForegroundAnim = 1;
                }
                else
                {
                    if (ForegroundAnimation != null)
                    {
                        var bgAnimsToDelete = new List<BackgroundAnimSlot>();
                        foreach (var bg in BackgroundSlots)
                        {
                            if (bg.Anim.Weight < 0.01f)
                            {
                                bgAnimsToDelete.Add(bg);
                            }

                            bg.WeightSnapshot = bg.Anim.Weight;
                        }

                        foreach (var bg in bgAnimsToDelete)
                        {
                            BackgroundSlots.Remove(bg);
                        }
                        
                        var bgSlot = new BackgroundAnimSlot()
                        {
                            WeightSnapshot = ForegroundAnimation.Weight,
                            OrigRequest = prevReq,
                            Anim = ForegroundAnimation,
                        };
                        BackgroundSlots.Add(bgSlot);
                        
                        float totalWeights = BackgroundSlots.Sum(x => x.WeightSnapshot);
                        foreach (var bg in BackgroundSlots)
                        {
                            if (totalWeights > 0)
                                bg.Anim.Weight = (bg.WeightSnapshot / totalWeights);
                            else
                                bg.Anim.Weight = 0;
                        }

                        
                    }

                    weightForNewForegroundAnim = 0;
                }



                ForegroundAnimation = newForegroundAnimation;
                if (ForegroundAnimation == null)
                {
                    BackgroundSlots.Clear();
                }
                else
                {
                    ForegroundAnimation.Weight = weightForNewForegroundAnim;
                    float startTime = req.AnimStartTime;
                    if (CurrentRequest.AnimStartTimeIsFrames)
                        startTime *= ForegroundAnimation.FrameDuration;
                    Scrub(absolute: true, time: startTime, foreground: true, background: true, out _, out _);
                }

                newlyStartedAnim = newForegroundAnimation;

                result = true;
            }

            return result;
        }
        
        public bool AnyAnimations()
        {
            int count = BackgroundSlots.Count + (ForegroundAnimation != null ? 1 : 0);
            return count > 0;
        }

        public void AccessAllAnimations(Action<NewHavokAnimation> access)
        {
            foreach (var bg in BackgroundSlots)
            {
                access(bg.Anim);
            }

            if (ForegroundAnimation != null)
                access(ForegroundAnimation);
        }
        
        public void Scrub(bool absolute, float time, bool foreground, 
            bool background, out float timeDelta, out float? syncTime,
            bool ignoreRootMotion = false)
        {
            syncTime = null;
            //test
            if (SlotType == SlotTypes.Base)
                IgnoreBlendFromInvalid = true;
            
            //if (SlotType == SlotTypes.TaeExtraAnim0 && absolute && time == 0)
            //{
            //    Console.WriteLine("test");
            //}

            timeDelta = 0;
            if (ForegroundAnimation != null)
            {
                if (foreground)
                {
                    if (!FixedBlendMode)
                    {
                        BlendMode = (ForegroundAnimation.IsAdditiveBlend)
                            ? BlendModes.Additive_RelativeToStartFrame
                            : BlendModes.Normal;
                    }

                    ForegroundAnimation.Scrub(absolute, time, out timeDelta, out float? foregroundSyncTime, ignoreRootMotion);
                    syncTime = foregroundSyncTime;
                    
                    if ((ForegroundAnimation.LoopCount > 0 ||
                         ForegroundAnimation.CurrentTime >= ForegroundAnimation.Duration) && CurrentRequest.ClearOnEnd)
                    {
                        SetRequest(Request.ForceClear, Vector4.Zero, out _);
                        timeDelta = 0;
                        return;
                    }
                }
                else
                {
                    timeDelta = 0;
                }

                float blendDuration = CurrentRequest.BlendDuration;
                if (CurrentRequest.BlendDurationIsFrames)
                    blendDuration *= ForegroundAnimation.FrameDuration;
                
                float startTime = CurrentRequest.AnimStartTime;
                if (CurrentRequest.AnimStartTimeIsFrames)
                    startTime *= ForegroundAnimation.FrameDuration;
                
                float blend = (CurrentRequest.BlendDuration > 0)
                    ? ((ForegroundAnimation.CurrentTimeUnlooped - startTime) / blendDuration) : 1;
                
                if (blend < 0)
                    blend = 0;
                if (blend > 1)
                    blend = 1;

                if (BackgroundSlots.Count == 0 && IgnoreBlendFromInvalid)
                {
                    blend = 1;
                }

                ForegroundAnimation.Weight = blend * CurrentRequest.DesiredWeight;
                if (background)
                    UpdateBackgroundSlots(1 - blend, false, timeDelta, ignoreRootMotion);
            }
            else
            {
                timeDelta = 0;
            }
        }

        // public float GetWeightOfWholeSlot()
        // {
        //     if (ForegroundAnimation != null)
        // }
        
        public NewBlendableTransform GetBoneTransform(int t, NewAnimSkeleton_HKX Skeleton, float DebugAnimWeight, 
            NewBlendableTransform startFromTransform, Matrix parentMatrix, Matrix parentScaleMatrix)
        {

            int actualSlotCount = BackgroundSlots.Count + (ForegroundAnimation != null ? 1 : 0);
            
            if (actualSlotCount == 0)
            {

                return startFromTransform;

            }
            else
            {
                var tr = startFromTransform;
                float weight = 0;

                void DoAnim(NewHavokAnimation anim, Request origRequest, bool isForeground)
                {
                    if (anim.Weight * DebugAnimWeight <= 0)
                        return;

                    anim.EnableLooping = origRequest.EnableLoop;

                    var frameTransform = anim.GetBlendableTransformOnCurrentFrame(t);
                    var frameMat = frameTransform.GetXnaMatrix();
                    var frameMatScale = frameTransform.GetXnaMatrixScale();



                    if (anim.IsAdditiveBlend)
                    {
                        if (anim.BlendHint == HKX.AnimationBlendHint.ADDITIVE_CHILD_SPACE)
                        {
                            frameMat = (Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrix() * frameMat);
                            frameMatScale = (Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrixScale() * frameMatScale);
                        }
                        else
                        {
                            frameMat = (Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrix() * frameMat);
                            frameMatScale = (Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrixScale() * frameMatScale);
                        }
                    }


                    if (BlendMode == BlendModes.Additive_RelativeToTPose)
                    {
                        frameMat = (frameMat * Matrix.Invert(Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrix()));


                        frameMatScale = (frameMatScale * Matrix.Invert(Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrixScale()));
                    }
                    else if (BlendMode == BlendModes.Additive_RelativeToStartFrame)
                    {
                        var startFrameTransform = anim.GetBlendableTransformOnSpecificFrame(t, 0);

                        var startFrameMat = startFrameTransform.GetXnaMatrix();
                        if (anim.IsAdditiveBlend)
                            startFrameMat = Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrix() * startFrameMat;

                        var startFrameMatScale = startFrameTransform.GetXnaMatrixScale();
                        if (anim.IsAdditiveBlend)
                            startFrameMatScale = Skeleton.Bones[t].ReferenceLocalTransform.GetXnaMatrixScale() * startFrameMatScale;

                        frameMat = frameMat * Matrix.Invert(startFrameMat);

                        frameMatScale = frameMatScale * Matrix.Invert(startFrameMatScale);
                    }



                    weight += MathHelper.Clamp(anim.Weight, 0, 1);
                    float finalWeight = anim.Weight;
                    if (actualSlotCount > 1 && weight > 0)
                        finalWeight /= weight;

                    // test
                    //frame = frame * NewBlendableTransform.Invert(parentTransform);


                    if (BlendMode is BlendModes.Additive_RelativeToStartFrame or BlendModes.Additive_RelativeToTPose)
                    {
                        tr *= NewBlendableTransform.Lerp(NewBlendableTransform.Identity, (frameMatScale * frameMat).ToNewBlendableTransform(), finalWeight);
                    }
                    else
                    {
                        tr = NewBlendableTransform.Lerp(tr, (frameMatScale * frameMat).ToNewBlendableTransform(), finalWeight);
                    }
                    

                }




                if (!Main.Debug.AnimSlotDisableBlend)
                {
                    for (int i = 0; i < BackgroundSlots.Count; i++)
                    {
                        DoAnim(BackgroundSlots[i].Anim, BackgroundSlots[i].OrigRequest, false);
                    }
                }

                if (ForegroundAnimation != null)
                {
                    DoAnim(ForegroundAnimation, CurrentRequest, true);
                }

                NewBlendableTransform resultTransform = startFromTransform;

                if (BlendMode == BlendModes.Normal)
                {
                    resultTransform = NewBlendableTransform.Lerp(Skeleton.Bones[t].ReferenceLocalTransform, tr, DebugAnimWeight);

                    if (!Main.Config.EnableBoneTranslation_NormalAnims)
                        resultTransform.Translation = startFromTransform.Translation;
                    if (!Main.Config.EnableBoneScale_NormalAnims)
                        resultTransform.Scale = startFromTransform.Scale;
                }
                else if (BlendMode is BlendModes.Additive_RelativeToTPose or BlendModes.Additive_RelativeToStartFrame)
                {
                    resultTransform = NewBlendableTransform.Lerp(Skeleton.Bones[t].ReferenceLocalTransform, tr, DebugAnimWeight);

                    if (!Main.Config.EnableBoneTranslation_AdditiveAnims)
                        resultTransform.Translation = startFromTransform.Translation;
                    if (!Main.Config.EnableBoneScale_AdditiveAnims)
                        resultTransform.Scale = startFromTransform.Scale;
                }
                else
                {
                    throw new NotImplementedException();
                }

                return resultTransform;
            }
            
        }
        
        public void Clear()
        {
            BackgroundSlots.Clear();
            ForegroundAnimation = null;
            CurrentRequest.TaeAnimID = SplitAnimID.Invalid;
        }

        public void ClearBackgroundSlots()
        {
            BackgroundSlots.Clear();
            // Update blending
            Scrub(absolute: false, 0, true, true, out _, out _);
        }

        public void ResetToStart(System.Numerics.Vector4 startRootMotionTransform)
        {
            if (ForegroundAnimation != null)
            {
                ForegroundAnimation.Reset(startRootMotionTransform);
                ForegroundAnimation.Weight = CurrentRequest.DesiredWeight;
            }

            Scrub(true, 0, true, true, out _, out _);
            
            BackgroundSlots.Clear();
            // Update blending
            Scrub(absolute: true, 0, true, true, out _, out _);
        }
        
        
    }
}