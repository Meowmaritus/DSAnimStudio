using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using NAudio.Gui;
using Org.BouncyCastle.Asn1.X509;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsAssetPipeline.Audio.Wwise.WwiseBlock;

namespace DSAnimStudio.TaeEditor
{
    public class TaeActionSimulationEnvironment
    {
        public bool AnimRestartRequested = false;
        public void RequestAnimRestart()
        {
            AnimRestartRequested = true;
        }

        private NewHavokAnimation FakeAnimationForSim = new NewHavokAnimation_Dummy();
        
        public bool IsRemoModeEnabled = false;
        public NewGraph GraphForRemoSpecifically = null;

        private NewAnimSlot.Request[] TaeAnimRequest_TaeAddAnim = new NewAnimSlot.Request[10];
        NewAnimSlot.Request TaeAnimRequest_SekiroFace = NewAnimSlot.Request.Empty;

        public Vector3? TestChrTargetPos = null;

        public bool IsMouseClickScrub = false;

        //public bool SimEnabled_BasicBlending = true;
        //public bool SimEnabled_BasicBlending_ComboViewer = true;
        //public bool SimEnabled_Hitboxes = true;
        //public bool SimEnabled_Sounds = true;
        //public bool SimEnabled_RumbleCam = true;
        //public bool SimEnabled_WeaponStyle = true;
        //public bool SimEnabled_Tracking = true;
        //public bool SimEnabled_AdditiveAnims = true;
        //public bool SimEnabled_WeaponLocationOverrides = true;
        //public bool SimEnabled_SpEffects = true;
        //public bool SimEnabled_DS3DebugAnimSpeed = false;
        //public bool SimEnabled_ERAnimSpeedGradient = true;
        //public bool SimEnabled_SetOpacity = true;
        //public bool SimEnabled_ModelMasks = true;
        //public bool SimEnabled_Bullets = true;
        //public bool SimEnabled_FFX = true;
        public float ModPlaybackSpeed_GrabityRate = 1.0f;
        public float ModPlaybackSpeed_Event603 = 1.0f;
        public float ModPlaybackSpeed_Event608 = 1.0f;
        public float ModPlaybackSpeed_NightfallEvent7032 = 1.0f;
        public float ModPlaybackSpeed_AC6Event9700 = 1.0f;


        //public bool SimEnabled_NF_SetTurnSpeedGradient = false;
        //public bool SimEnabled_NF_SetTaeExtraAnim = false;
        //public bool SimEnabled_NF_AnimSpeedGradient = false;
        //public bool SimEnabled_NF_RootMotionScale = false;
        
        private void PrepareModelForTaeFrame(Model mdl)
        {
            mdl.Opacity = 1;

            if (mdl.ModelType == Model.ModelTypes.BaseModel)
            {

                mdl.ForAllAC6NpcParts((partIndex, part, model) =>
                {
                    model.DummyPolyMan.ClearAllShowAttacks();
                    model.DummyPolyMan.ClearAllSFXSpawns();
                    model.DummyPolyMan.ClearAllBulletSpawns();
                });

                mdl.DummyPolyMan.ClearAllShowAttacks();
                mdl.DummyPolyMan.ClearAllSFXSpawns();
                mdl.DummyPolyMan.ClearAllBulletSpawns();

                mdl.ChrAsm?.ForAllWeaponModels(m =>
                {
                    m.DummyPolyMan.ClearAllShowAttacks();
                    m.DummyPolyMan.ClearAllSFXSpawns();
                    m.DummyPolyMan.ClearAllBulletSpawns();
                    m.Opacity = 1;
                });

                mdl.ChrAsm?.ForAllArmorModels(m =>
                {
                    m.DummyPolyMan.ClearAllShowAttacks();
                    m.DummyPolyMan.ClearAllSFXSpawns();
                    m.DummyPolyMan.ClearAllBulletSpawns();
                    m.Opacity = 1;
                });
            }

            mdl.ResetDrawMaskToDefault();

            if (mdl.ChrAsm != null)
            {
                mdl.ChrAsm.ClearAllWeaponHiddenByTae();
                
            }
        }

        public int GhettoFix_DisableSimulationTemporarily = 0;
        
        
        public void NewSimulationScrub(NewGraph Graph, Func<SplitAnimID, DSAProj.Animation> getAnimFunc, SplitAnimID taeAnimID, 
            bool justStartedPlayback, bool ignoreBlendProcess = false)
        {
            bool anyValidSlots = false;
            bool validBaseAnimSlot = false;
            var enableOverlayTae = Main.Config.SimulateTaeOfOverlayedAnims;

            if (GhettoFix_DisableSimulationTemporarily > 0)
            {
                MODEL.AnimContainer?.AccessAnimSlots(animSlots =>
                {
                    foreach (var kvp in animSlots)
                    {
                        if (kvp.Value != null && kvp.Value.TaeEnabled && (enableOverlayTae || kvp.Key == NewAnimSlot.SlotTypes.Base))
                        {
                            var foregroundAnim = kvp.Value.GetForegroundAnimation();
                            if (foregroundAnim != null && foregroundAnim.TaeAnimation != null)
                            {
                                foregroundAnim.TaePrevTime = foregroundAnim.CurrentTime;
                                foregroundAnim.TaePrevLoopCount = foregroundAnim.LoopCount;
                                foregroundAnim.TaePrevFramePlaybackCursorMoving = false;
                                anyValidSlots = true;
                            }
                        }
                    }
                });

                if ((!validBaseAnimSlot) && Graph?.PlaybackCursor != null && MODEL?.AnimContainer != null)
                {
                    FakeAnimationForSim.TaePrevTime = FakeAnimationForSim.CurrentTime;
                    FakeAnimationForSim.TaePrevLoopCount = FakeAnimationForSim.LoopCount;
                    FakeAnimationForSim.TaePrevFramePlaybackCursorMoving = false;
                }

                return;
            }

            // if (justStartedPlayback)
            // {
            //     timeLastFrame_Deprecated = -1;
            //     timeLastFrame = -1;
            // }

            // if (time < 0)
            //     Console.WriteLine("test");

            //timeLastFrame -= (loopDelta * animDuration);
            

            

            MODEL.AnimContainer?.AccessAnimSlots(animSlots =>
            {
                foreach (var kvp in animSlots)
                {
                    if (kvp.Value != null && kvp.Value.TaeEnabled && (enableOverlayTae || kvp.Key == NewAnimSlot.SlotTypes.Base))
                    {
                        var foregroundAnim = kvp.Value.GetForegroundAnimation();
                        if (foregroundAnim != null && foregroundAnim.TaeAnimation != null)
                        {
                            anyValidSlots = true;
                            if (kvp.Key == NewAnimSlot.SlotTypes.Base)
                                validBaseAnimSlot = true;
                        }
                    }
                }
            });

            //if (!anyValidSlots || !validBaseAnimSlot)
            //    return;


            var cfg = Main.Config;

            

            if (!ignoreBlendProcess && anyValidSlots && validBaseAnimSlot)
            {
                bool isInComboViewMode = Graph?.ViewportInteractor?.NewIsComboActive ?? false;
                float blendDuration = 0;
                if ((cfg.SimEnabled_BasicBlending && !isInComboViewMode) || (cfg.SimEnabled_BasicBlending_ComboViewer && isInComboViewMode))
                {
                    blendDuration = getAnimFunc?.Invoke(taeAnimID)?.SAFE_GetBlendDuration() ?? 0;
                    MODEL.AnimContainer.NewSetBlendDurationOfSlot(NewAnimSlot.SlotTypes.Base, SplitAnimID.Invalid, blendDuration, isInFrames: false);
                }
                
            }

            var entitySettings = ImguiOSD.OSD.WindowEntity;
            Dictionary<int, bool> stateInfosEnabled = null;
            Dictionary<int, string> stateInfoNames = null;
            if (entitySettings != null)
            {
                stateInfosEnabled = entitySettings.GetStateInfoSelectConfig_Enabled();
                stateInfoNames = entitySettings.GetStateInfoSelectConfig_Names();
            }

            var desiredWeaponStyle = NewChrAsm.WeaponStyleType.None;

            if (MODEL.ChrAsm != null)
            {
                
                // When viewing combos, retain weapon style from before.
                if (Graph?.ViewportInteractor?.NewIsComboActive ?? false)
                {
                    desiredWeaponStyle = MODEL.ChrAsm.WeaponStyle;
                    
                }
                else
                {
                    desiredWeaponStyle = MODEL.ChrAsm.StartWeaponStyle;
                }
            }

            float activeTrackingSpeedThisFrame = MODEL.BaseTrackingSpeed;
            bool trackingDisabledThisFrame = false;

            

            bool anyWeaponOverrideHappeningThisFrame = false;


            ModPlaybackSpeed_GrabityRate = 1.0f;
            ModPlaybackSpeed_Event603 = 1.0f;
            ModPlaybackSpeed_Event608 = 1.0f;
            ModPlaybackSpeed_NightfallEvent7032 = 1.0f;
            ModPlaybackSpeed_AC6Event9700 = 1.0f;


            TaeRootMotionScaleXZ = 1;

            SimulatedActiveSpEffects.Clear();

            PrepareModelForTaeFrame(MODEL);

            TestChrTargetPos = (cfg.SimOption_NF_MoveRelative_UseCameraAsTarget) ? 
                Vector3.Transform(Vector3.Zero, Document.WorldViewManager.CurrentView.CameraLocationInWorld.WorldMatrix) : null;

            List<int> attackIndicesUsed = new List<int>();

            var soloHoverAction = Main.Config.SoloHighlightActionOnHover ? Main.TAE_EDITOR?.NewHoverAction_NeedsNoSelection : null;

       
            if (Document.GameRoot.GameType is SoulsGames.SDT)
            {
                TaeAnimRequest_SekiroFace.ForceNew = false;
                TaeAnimRequest_SekiroFace.TaeAnimID = SplitAnimID.Invalid;
                TaeAnimRequest_SekiroFace.EnableLoop = true;
                TaeAnimRequest_SekiroFace.DesiredWeight = 1;
                TaeAnimRequest_SekiroFace.ClearOnEnd = false;
                TaeAnimRequest_SekiroFace.BlendDuration = 0;
            }
            
            MODEL.ChrAsm?.ClearSekiroProstheticOverrideTae();

            var NF_PlayDummyPolyFollowSoundEx_ExInfosUsedThisFrame = new List<int>();



            if (GhettoFix_DisableSimulationTemporarily <= 0)
            {

                void DoActions(NewAnimSlot.SlotTypes slotType, NewHavokAnimation slotAnim)
                {
                    // -------
                    // ACTIONS
                    // -------

                    var slotTaeAnim = slotAnim.TaeAnimation;
                    if (slotTaeAnim == null)
                        return;

                    slotTaeAnim.SafeAccessActions(acts =>
                    {
                        var slotTime = slotAnim.CurrentTime;
                        var slotAnimDuration = slotAnim.Duration;
                        var slotNonModTime = slotAnim.CurrentTimeUnlooped;
                        var slotPrevNonModTime = slotAnim.TaePrevTimeUnlooped;

                        var slotTimeDelta = slotNonModTime - slotPrevNonModTime;

                        var slotPrevTime = slotAnim.TaePrevTime;
                        var slotLoopCount = slotAnim.LoopCount;
                        var slotPrevLoopCount = slotAnim.TaePrevLoopCount;
                        var slotLoopDelta = slotLoopCount - slotPrevLoopCount;



                        if (slotPrevTime.ApproxEquals(0) || slotTime.ApproxEquals(0))
                            justStartedPlayback = true;

                        // if (slotAnim.CurrentTimeUnlooped < slotAnim.TaePrevTimeUnlooped || slotLoopDelta < 0)
                        // {
                        //     slotAnim.TaePrevTime = slotTime;
                        //     slotAnim.TaePrevLoopCount = slotLoopCount;
                        //     return;
                        // }


                        if (slotType is NewAnimSlot.SlotTypes.Base)
                        {
                            for (int i = 0; i < TaeAnimRequest_TaeAddAnim.Length; i++)
                            {
                                TaeAnimRequest_TaeAddAnim[i] = NewAnimSlot.Request.Empty;
                                TaeAnimRequest_TaeAddAnim[i].ForceNew = false;
                            }



                            if (Document.GameRoot.GameType is SoulsGames.DS1 or SoulsGames.DS1R)
                            {
                                TaeAnimRequest_TaeAddAnim[0].KeepPlayingUntilEnd = true;
                            }
                        }

                        bool playbackCursorMovingForward = ((slotTime - slotPrevTime) > 0.001f) && !IsMouseClickScrub;
                        bool playbackCursorMovingBackward = ((slotTime - slotPrevTime) < -0.001f) && !IsMouseClickScrub;

                        if (playbackCursorMovingBackward || IsMouseClickScrub)
                            slotAnim.TaeScrubHasReactivatedActionsAlready = false;

                        bool playbackCursorMoving = playbackCursorMovingForward;// || playbackCursorMovingBackward;
                        bool playbackCursorStartedMovingForwardThisFrame = playbackCursorMovingForward && !slotAnim.TaePrevFramePlaybackCursorMoving && !slotAnim.TaeScrubHasReactivatedActionsAlready;

                        if (playbackCursorStartedMovingForwardThisFrame)
                            slotAnim.TaeScrubHasReactivatedActionsAlready = true;


                        var currentSoloActions = acts.Where(act => act.Solo).ToList();

                        var simulateOneshotActionsInReverse = Main.Config.SimulateOneShotActionsInReverse;

                        //slotNewPrevFrameTime -= (slotLoopDelta * slotAnimDuration);

                        /////////////
                        // ACTIONS //
                        /////////////

                        #region ACTIONS
                        foreach (var act in acts)
                        {


                            var br = act.GetParamBinReader(Document.GameRoot.IsBigEndianGame);





                            if (soloHoverAction != null)
                            {
                                act.NewSimulationEnter = false;
                                act.NewSimulationExit = false;
                                act.NewSimulationActive = act == soloHoverAction;
                            }
                            else
                            {
                                float meme = 0;// 0.0001f;
                                float startTime = act.StartTime - meme;
                                float endTime = act.EndTime + meme;
                                act.NewSimulationEnter = (slotPrevTime < startTime && slotTime >= startTime) || (simulateOneshotActionsInReverse && (slotPrevTime >= startTime && slotTime < startTime));
                                act.NewSimulationExit = (slotPrevTime <= endTime && slotTime > endTime) || (simulateOneshotActionsInReverse && (slotPrevTime > endTime && slotTime <= endTime));
                                act.NewSimulationActive = (slotTime >= startTime && slotTime <= endTime) || act.NewSimulationEnter || act.NewSimulationExit;
                            }

                            if (justStartedPlayback || playbackCursorStartedMovingForwardThisFrame)
                            {
                                if (act.NewSimulationActive)// && (MathF.Abs(act.StartTime - slotTime) < 0.0001f))
                                {
                                    act.NewSimulationEnter = true;
                                }
                            }

                            // if ((AnimRestartRequested || loopDelta != 0) && act.NewSimulationActive)
                            // {
                            //     act.NewSimulationEnter = true;
                            // }


                            // Meme events that have special active logic
                            if (cfg.SimEnabled_WeaponStyle && act.Type == 32 && MODEL.ChrAsm != null)
                            {
                                if (slotTime >= act.StartTime)
                                    desiredWeaponStyle = (NewChrAsm.WeaponStyleType)Convert.ToInt32(act.ReadInternalSimField("WeaponStyle"));
                                continue;
                            }
                            else if (cfg.SimEnabled_ModelMasks && act.Type == 233 && slotTime >= act.StartTime)
                            {
                                int maskLength = 32;
                                if (Document.GameRoot.GameType is SoulsGames.DS1 or SoulsGames.DS1R or SoulsGames.DES)
                                    maskLength = 8;

                                if (Document.GameRoot.GameType is SoulsGames.BB)
                                    maskLength = 16;

                                byte[] mask = br.GetBytes(0, maskLength);
                                for (int i = 0; i < mask.Length; i++)
                                {
                                    if (mask[i] == 0)
                                        MODEL.DrawMask[i] = false;
                                    else if (mask[i] == 1)
                                        MODEL.DrawMask[i] = true;
                                    // FF is skip
                                }
                            }

                            if (currentSoloActions.Count > 0)
                                act.IsActive_BasedOnMuteSolo = act.Solo;
                            else
                                act.IsActive_BasedOnMuteSolo = !act.Mute;







                            //Defaults to true.
                            act.IsActive_BasedOnStateInfo = true;

                            var stateInfoCheck = act.HasInternalSimField("StateInfo");
                            if (stateInfoCheck && stateInfosEnabled != null)
                            {
                                var stateInfo = Convert.ToInt32(act.ReadInternalSimField("StateInfo"));
                                if (stateInfo <= 0)
                                {
                                    act.IsActive_BasedOnStateInfo = true;
                                }
                                else
                                {
                                    if (!stateInfosEnabled.ContainsKey(stateInfo))
                                        stateInfosEnabled.Add(stateInfo, false);
                                    if (!stateInfoNames.ContainsKey(stateInfo))
                                        stateInfoNames.Add(stateInfo, "");
                                    act.IsActive_BasedOnStateInfo = stateInfosEnabled[stateInfo];
                                }
                            }

                            if (act == soloHoverAction)
                            {
                                act.IsActive_BasedOnMuteSolo = true;

                                if (Main.Config.SoloHighlightActionOnHover_IgnoresStateInfo)
                                    act.IsActive_BasedOnStateInfo = true;
                            }

                            if (!act.IsActive)
                            {
                                //if (act.IsActive_BasedOnMuteSolo)
                                //    Console.WriteLine("wtf");

                                continue;
                            }

                            if (!act.NewSimulationActive)
                            {
                                //if (act.IsActive_BasedOnMuteSolo)
                                //    Console.WriteLine("wtf");

                                continue;
                            }

                            if (act.Type == 0)
                            {
                                var jumpTableID = Convert.ToInt32(act.ReadInternalSimField("JumpTableID"));
                                if (jumpTableID == 7)
                                    trackingDisabledThisFrame = true;
                            }

                            // HITBOXES
                            else if ((cfg.SimEnabled_Hitboxes && act.Type == 1)
                                || (cfg.SimEnabled_Hitboxes_CommonBehavior && act.Type == 5)
                                || (cfg.SimEnabled_Hitboxes_PCBehavior && act.Type == 307)
                                || (cfg.SimEnabled_Hitboxes_ThrowAttackBehavior && act.Type == 304))
                            {
                                NewDummyPolyManager.HitboxSpawnTypes spawnType = NewDummyPolyManager.HitboxSpawnTypes.None;

                                if (act.NewSimulationExit)
                                    continue;

                                var mdl = GetModelOfBox(act);
                                var dmyPolySrcToUse = HitViewDummyPolySource;

                                int index = -1;
                                if (act.HasInternalSimField("Index"))
                                {
                                    index = Convert.ToInt32(act.ReadInternalSimField("Index"));
                                }

                                if (act.Type == 1)
                                {
                                    spawnType = NewDummyPolyManager.HitboxSpawnTypes.AttackBehavior;
                                    //SDT and onward have a field for this finally, so use it
                                    if (act.HasInternalSimField("Source"))
                                    {
                                        byte sourceType = Convert.ToByte(act.ReadInternalSimField("Source"));
                                        if (sourceType == 1)
                                            dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                                        else if (sourceType == 2)
                                            dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                                    }
                                }
                                else if (act.Type == 5)
                                {
                                    spawnType = NewDummyPolyManager.HitboxSpawnTypes.CommonBehavior;
                                }
                                else if (act.Type == 304)
                                {
                                    spawnType = NewDummyPolyManager.HitboxSpawnTypes.ThrowAttackBehavior;
                                }
                                else if (act.Type == 307)
                                {
                                    spawnType = NewDummyPolyManager.HitboxSpawnTypes.PCBehavior;
                                    int pcBehaviorType = br.GetInt32(4);
                                    if (pcBehaviorType == 1 || pcBehaviorType == 16)
                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                                    else if (pcBehaviorType == 2 || pcBehaviorType == 128)
                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                                    //else
                                    //    dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.BaseModel;
                                }

                                var atkParam = GetAtkParamFromEventBox(act, dmyPolySrcToUse, out bool isNpcAtk, out ParamData.BehaviorParam behaviorParam);

                                //if (Document.GameRoot.GameType is SoulsGames.AC6 && mdl.AC6NpcParts != null && behaviorParam != null)
                                //{
                                //    if (behaviorParam.AC6AttackActionParam == null)
                                //    {
                                //        var matchingAttackActionParams = Document.ParamManager.AC6AttackActionParam_NPC.Where(x => (x.Key / 1_0000) == behaviorParam.VariationID).ToList();
                                //        foreach (var aa in matchingAttackActionParams)
                                //        {
                                //            bool outerBreak = false;
                                //            int memeCount = behaviorParam.BehaviorJudgeID % 10;
                                //            for (int i = memeCount; i >= 0; i--)
                                //            {
                                //                if (aa.Value.BehaviorJudgeID == (behaviorParam.BehaviorJudgeID - memeCount) + i)
                                //                {
                                //                    behaviorParam.AC6AttackActionParam = aa.Value;
                                //                    outerBreak = true;
                                //                    break;
                                //                }
                                //            }
                                //            if (outerBreak)
                                //                break;
                                //        }
                                //    }

                                //    if (behaviorParam.AC6AttackActionParam != null)
                                //    {
                                //        if (behaviorParam.AC6AttackActionParam.ConnectPartsSlotIndex >= 0)
                                //        {
                                //            dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.AC6Parts0 + behaviorParam.AC6AttackActionParam.ConnectPartsSlotIndex;
                                //        }
                                //    }
                                //}
                                
                                if (Document.GameRoot.GameType is SoulsGames.AC6 && mdl.AC6NpcParts != null)
                                {
                                    dmyPolySrcToUse = mdl.AC6NpcParts?.SelectedDummyPolySource ?? ParamData.AtkParam.DummyPolySource.BaseModel;
                                }

                                if (atkParam != null)
                                {
                                    if (index < 0)
                                        index = mdl.DummyPolyMan.GetDynamicAttackSlotForEvent(act);
                                    mdl.DummyPolyMan.SetAttackVisibility(index, spawnType, isNpcAtk, atkParam, true, -1 /*dummyPolyOverride*/, dmyPolySrcToUse, elapsedTime: slotTime - act.StartTime);
                                    attackIndicesUsed.Add(index);

                                }
                            }


                            else if (cfg.SimEnabled_Bullets && act.Type == 2 || (act.Type == 4 && Document.GameRoot.GameType is SoulsGames.SDT))
                            {
                                var mdl = GetModelOfBox(act);
                                
                                var dmyPolySrcToUse = HitViewDummyPolySource;

                                if (act.HasInternalSimField("Source"))
                                {
                                    byte sourceType = Convert.ToByte(act.ReadInternalSimField("Source"));
                                    if (sourceType == 1)
                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                                    else if (sourceType == 2)
                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                                }
                                
                                var bulletParamID = GetBulletParamIDFromEvBox(act, dmyPolySrcToUse);
                                if (bulletParamID >= 0)
                                {
                                    


                                    if (Document.GameRoot.GameType is SoulsGames.AC6)
                                    {
                                        var behaviorParam = GetBehaviorParamFromEvBox(act, dmyPolySrcToUse);
                                        //int dummyPolyID = Convert.ToInt32(ev.ReadInternalSimField("DummyPolyID"));
                                        int dummyPolyMin = behaviorParam?.AC6DummyPolyStart ?? -1;
                                        int dummyPolyMax = behaviorParam?.AC6DummyPolyEnd ?? -1;

                                        if (dummyPolyMin != -1)
                                        {
                                            if (dummyPolyMax == -1)
                                                dummyPolyMax = dummyPolyMin;

                                            if (bulletParamID >= 0)
                                            {
                                                for (int d = dummyPolyMin; d <= dummyPolyMax; d++)
                                                {
                                                    var asmSrc = mdl.ChrAsm?.GetDummyPolySpawnPlace(dmyPolySrcToUse, d, mdl.DummyPolyMan);
                                                    if (asmSrc == null || asmSrc == mdl.DummyPolyMan)
                                                    {
                                                        mdl.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, d);
                                                    }
                                                    else
                                                    {
                                                        asmSrc.SpawnBulletOnDummyPoly(bulletParamID, d % 10000);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            int dummyPolyID = Convert.ToInt32(act.ReadInternalSimField("DummyPolyID"));

                                            if (bulletParamID >= 0)
                                            {
                                                var asmSrc = mdl.ChrAsm?.GetDummyPolySpawnPlace(dmyPolySrcToUse, dummyPolyID, mdl.DummyPolyMan);
                                                if (asmSrc == null || asmSrc == mdl.DummyPolyMan)
                                                {
                                                    mdl.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID);
                                                }
                                                else
                                                {
                                                    asmSrc.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID % 10000);
                                                }
                                            }
                                        }

                                    }
                                    else
                                    {
                                        int dummyPolyID = Convert.ToInt32(act.ReadInternalSimField("DummyPolyID"));

                                        if (bulletParamID >= 0)
                                        {
                                            var asmSrc = mdl.ChrAsm?.GetDummyPolySpawnPlace(dmyPolySrcToUse, dummyPolyID, mdl.DummyPolyMan);
                                            if (asmSrc == null || asmSrc == mdl.DummyPolyMan)
                                            {
                                                mdl.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID);
                                            }
                                            else
                                            {
                                                asmSrc.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID % 10000);
                                            }
                                        }
                                    }
                                }

                            }

                            else if (cfg.SimEnabled_Bullets && (act.Type == 8 && Document.GameRoot.GameType is SoulsGames.AC6))
                            {
                                var mdl = GetModelOfBox(act);
                                
                                var dmyPolySrcToUse = HitViewDummyPolySource;

                                if (act.HasInternalSimField("Source"))
                                {
                                    byte sourceType = Convert.ToByte(act.ReadInternalSimField("Source"));
                                    if (sourceType == 1)
                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                                    else if (sourceType == 2)
                                        dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                                }
                                
                                var behaviorParam = GetBehaviorParamFromEvBox(act, dmyPolySrcToUse);
                                var bulletParamID = GetBulletParamIDFromEvBox(act, dmyPolySrcToUse);
                                //int dummyPolyID = Convert.ToInt32(ev.ReadInternalSimField("DummyPolyID"));
                                int dummyPolyMin = behaviorParam?.AC6DummyPolyStart ?? -1;
                                int dummyPolyMax = behaviorParam?.AC6DummyPolyEnd ?? -1;

                                if (dummyPolyMin != -1)
                                {
                                    if (dummyPolyMax == -1)
                                        dummyPolyMax = dummyPolyMin;

                                    if (bulletParamID >= 0)
                                    {
                                        for (int d = dummyPolyMin; d <= dummyPolyMax; d++)
                                        {
                                            var asmSrc = mdl.ChrAsm?.GetDummyPolySpawnPlace(dmyPolySrcToUse, d, mdl.DummyPolyMan);
                                            if (asmSrc == null || asmSrc == mdl.DummyPolyMan)
                                            {
                                                mdl.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, d);
                                            }
                                            else
                                            {
                                                asmSrc.SpawnBulletOnDummyPoly(bulletParamID, d % 10000);
                                            }
                                        }
                                    }
                                }


                            }


                            else if (cfg.SimEnabled_AdditiveAnims && act.Type == 303 && Document.GameRoot.GameType is SoulsGames.DS1 or SoulsGames.DS1R)
                            {
                                int animID = Convert.ToInt32(br.GetInt32(0x0));

                                if (animID >= 0)
                                {
                                    TaeAnimRequest_TaeAddAnim[0].ForceNew = false;
                                    TaeAnimRequest_TaeAddAnim[0].TaeAnimID = SplitAnimID.FromFullID(Graph.MainScreen.Proj, animID);
                                    TaeAnimRequest_TaeAddAnim[0].EnableLoop = false;
                                    TaeAnimRequest_TaeAddAnim[0].DesiredWeight = 1;
                                    TaeAnimRequest_TaeAddAnim[0].ClearOnEnd = true;
                                    TaeAnimRequest_TaeAddAnim[0].KeepPlayingUntilEnd = true;
                                    TaeAnimRequest_TaeAddAnim[0].BlendDuration = 0;
                                }


                            }

                            else if (cfg.SimEnabled_AdditiveAnims && act.Type == 607 && Document.GameRoot.GameType is SoulsGames.SDT)
                            {
                                var unk00 = br.ReadInt32();
                                if (unk00 == 0)
                                {
                                    int inputVal = br.ReadInt32();
                                    if (inputVal < 0)
                                        inputVal = 0;
                                    if (inputVal > 15)
                                        inputVal = 15;
                                    var animID = 790000 + (inputVal * 10);
                                    float a = br.GetSingle(0x8);
                                    float b = br.GetSingle(0xC);

                                    float s = MathHelper.Clamp((slotTime - act.StartTime) / (act.EndTime - act.StartTime), 0, 1);

                                    float weight = MathHelper.Lerp(a, b, s);

                                    TaeAnimRequest_SekiroFace.ForceNew = false;
                                    TaeAnimRequest_SekiroFace.TaeAnimID = SplitAnimID.FromFullID(Document.GameRoot, animID);
                                    TaeAnimRequest_SekiroFace.EnableLoop = true;

                                    // Replicate a bug in Sekiro
                                    if (weight > 0)
                                        TaeAnimRequest_SekiroFace.DesiredWeight = 1;
                                    else
                                        TaeAnimRequest_SekiroFace.DesiredWeight = 0;

                                    TaeAnimRequest_SekiroFace.ClearOnEnd = false;
                                    TaeAnimRequest_SekiroFace.BlendDuration =
                                        getAnimFunc?.Invoke(SplitAnimID.FromFullID(Document.GameRoot, animID))?.SAFE_GetBlendDuration() ?? 0;
                                }
                            }


                            // SOUNDS
                            else if (act.Type == 7052 && Document.GameRoot.GameType is SoulsGames.DS1R && Main.IsNightfallBuild)
                            {

                                var exInfoID = Convert.ToInt32(act.ReadInternalSimField("ExInfoID"));
                                if (!NF_PlayDummyPolyFollowSoundEx_ExInfosUsedThisFrame.Contains(exInfoID))
                                    NF_PlayDummyPolyFollowSoundEx_ExInfosUsedThisFrame.Add(exInfoID);

                                if (playbackCursorMoving)
                                    NF_PlayDummyPolyFollowSoundEx(act, act.NewSimulationEnter);
                            }
                            else if (cfg.SimEnabled_Sounds && NewSimulation_EvCheck_PlaySound(act))
                            {
                                if (act.NewSimulationEnter && playbackCursorMoving)
                                    PlayOneShotSoundOfBox(act, true);

                                // TESTING
                                //if (!act.NewSimulationEnter && slotLoopDelta != 0)
                                //    Console.WriteLine("test");
                            }



                            // Rumble Cam
                            else if (cfg.SimEnabled_RumbleCam && NewSimulation_EvCheck_RumbleCam(act))
                            {
                                if (act.NewSimulationEnter && playbackCursorMoving)
                                    PlayRumbleCamOfBox(act);
                            }




                            else if (cfg.SimEnabled_Tracking && act.Type == 224)
                            {
                                activeTrackingSpeedThisFrame = (float)act.ReadInternalSimField("TurnSpeed");
                            }

                            else if (cfg.SimEnabled_AdditiveAnims && NewSimulation_EvCheck_SetAdditiveAnim(act))
                            {
                                NewSimualation_EvProcess_SetAdditiveAnim(act, slotTime, getAnimFunc);
                            }

                            else if (cfg.SimEnabled_WeaponLocationOverrides && NewSimulation_EvCheck_WeaponLocationOverrides(act))
                            {
                                NewSimulation_EvProcess_WeaponLocationOverrides(act, ref anyWeaponOverrideHappeningThisFrame);
                            }



                            else if (cfg.SimEnabled_SpEffects && NewSimulation_EvCheck_SpEffects(act))
                            {
                                NewSimulation_EvProcess_SpEffects(act, act.NewSimulationEnter);
                            }


                            else if (cfg.SimEnabled_DS3DebugAnimSpeed && Document.GameRoot.GameType == SoulsGames.DS3 && act.Type == 603)
                            {
                                var eventFrames = br.GetUInt32(0);
                                ModPlaybackSpeed_Event603 = (((act.EndTime - act.StartTime) * 30.0f) / (float)eventFrames);
                            }

                            else if (cfg.SimEnabled_AC6SpeedGradient9700 && Document.GameRoot.GameType == SoulsGames.AC6 && act.Type == 9700)
                            {
                                float startSpeed = br.GetSingle(0);
                                float endSpeed = br.GetSingle(4);

                                float fadeDuration = act.EndTime - act.StartTime;
                                if (fadeDuration > 0)
                                {
                                    float timeSinceFadeStart = slotTime - act.StartTime;
                                    float lerpS = MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1);

                                    ModPlaybackSpeed_AC6Event9700 = MathHelper.Lerp(startSpeed, endSpeed, lerpS);
                                }
                            }

                            else if (cfg.SimEnabled_ERAnimSpeedGradient && (Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR) && act.Type == 608)
                            {
                                float startSpeed = br.GetSingle(0);
                                float endSpeed = br.GetSingle(4);

                                float fadeDuration = act.EndTime - act.StartTime;
                                if (fadeDuration > 0)
                                {
                                    float timeSinceFadeStart = slotTime - act.StartTime;
                                    float lerpS = MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1);

                                    ModPlaybackSpeed_Event608 = MathHelper.Lerp(startSpeed, endSpeed, lerpS);
                                }
                            }

                            // SetOpacityKeyframe
                            else if (cfg.SimEnabled_SetOpacity && act.Type == 193)
                            {
                                float fadeDuration = act.EndTime - act.StartTime;
                                float timeSinceFadeStart = slotTime - act.StartTime;

                                float fadeStartOpacity = br.GetSingle(0);
                                float fadeEndOpacity = br.GetSingle(4);

                                MODEL.Opacity = MathHelper.Lerp(fadeStartOpacity, fadeEndOpacity, timeSinceFadeStart / fadeDuration);
                            }

                            // DS1 Remo SetOpacity
                            else if (cfg.SimEnabled_SetOpacity && act.Type == 160
                                && (Document.GameRoot.GameType is SoulsGames.DS1 or SoulsGames.DS1R)
                                && Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                            {
                                float fadeDuration = act.EndTime - act.StartTime;
                                float timeSinceFadeStart = slotTime - act.StartTime;

                                float fadeStartOpacity = (float)br.GetByte(0) / 255f;
                                float fadeEndOpacity = (float)br.GetByte(1) / 255f;

                                var mdl = GetModelOfBox(act);

                                if (mdl != null)
                                    mdl.Opacity = MathHelper.Lerp(fadeStartOpacity, fadeEndOpacity, timeSinceFadeStart / fadeDuration);
                            }

                            // HideEquippedWeapon
                            else if (cfg.SimEnabled_ModelMasks && act.Type == 710 && (Document.GameRoot.GameType is SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6)
                                && MODEL.ChrAsm != null)
                            {
                                bool leftHand = br.GetBoolean(0);
                                bool rightHand = br.GetBoolean(1);
                                bool leftHand_withModel1 = br.GetBoolean(2);
                                bool rightHand_withModel1 = br.GetBoolean(3);

                                if (rightHand)
                                    MODEL.ChrAsm.SetWeaponHiddenByTae(NewChrAsm.EquipSlotTypes.RightWeapon, true, true, true, true);

                                if (leftHand)
                                    MODEL.ChrAsm.SetWeaponHiddenByTae(NewChrAsm.EquipSlotTypes.LeftWeapon, true, true, true, true);

                                if (leftHand_withModel1)
                                    MODEL.ChrAsm.SetWeaponHiddenByTae(NewChrAsm.EquipSlotTypes.RightWeapon, true, null, null, null);

                                if (rightHand_withModel1)
                                    MODEL.ChrAsm.SetWeaponHiddenByTae(NewChrAsm.EquipSlotTypes.LeftWeapon, true, null, null, null);
                            }


                            else if (cfg.SimEnabled_ModelMasks && (act.Type == 711 || act.Type == 713)
                                && (Document.GameRoot.GameType is SoulsGames.BB or SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6))
                            {
                                byte[] mask = br.GetBytes(0, 32);
                                for (int i = 0; i < mask.Length; i++)
                                {
                                    if (act.Type == 711 && mask[i] == 1) // HideModelMask
                                        MODEL.DrawMask[i] = false;
                                    else if (act.Type == 713 && mask[i] == 1) // ShowModelMask
                                        MODEL.DrawMask[i] = true;
                                }
                            }

                            else if (cfg.SimEnabled_FFX && act.GetInternalSimTypeName().StartsWith("FFX__"))
                            {
                                NewSimulation_EvProcess_FFX(act);
                            }



                            // ---------
                            // NIGHTFALL
                            // ---------
                            else if (Main.IsNightfallBuild && Document.GameRoot.GameType == SoulsGames.DS1R)
                            {

                                if (cfg.SimEnabled_NF_SetTaeExtraAnim && NewSimulation_EvCheck_NF_SetTaeExtraAnim(act))
                                {
                                    NewSimulation_EvProcess_NF_SetTaeExtraAnim(act, slotTime, br);
                                }

                                else if (cfg.SimEnabled_NF_MoveRelative && act.Type == 7007)
                                {
                                    Vector3 pSpeedAtStart = br.ReadVector3();
                                    Vector3 pSpeedAtEnd = br.ReadVector3();
                                    byte pMoveRelativeTo = br.ReadByte();
                                    bool cancelIfNoTarget = br.ReadByte() != 0;
                                    byte pCurveType = br.ReadByte();

                                    float lerpS = MathHelper.Clamp((slotTime - act.StartTime) / (act.EndTime - act.StartTime), 0, 1);
                                    lerpS = ProcessNightfallCurve(lerpS, pCurveType);

                                    float x = MathHelper.Lerp(pSpeedAtStart.X, pSpeedAtEnd.X, lerpS) * slotTimeDelta;
                                    float y = MathHelper.Lerp(pSpeedAtStart.Y, pSpeedAtEnd.Y, lerpS) * slotTimeDelta;
                                    float z = MathHelper.Lerp(pSpeedAtStart.Z, pSpeedAtEnd.Z, lerpS) * slotTimeDelta;

                                    var heading = MODEL.AnimContainer.RootMotionTransform.GetWrappedYawAngle();

                                    const byte ORIGIN_TYPE_SELF_FACING_DIR = 0;
                                    const byte ORIGIN_TYPE_TARGET = 1;
                                    const byte ORIGIN_TYPE_TARGET_IF_POSSIBLE_OR_SELF = 2;

                                    if (TestChrTargetPos == null && cancelIfNoTarget)
                                    {
                                        //Test if this is right way to skip? Probably is.
                                        continue;
                                    }

                                    if ((pMoveRelativeTo == ORIGIN_TYPE_TARGET || pMoveRelativeTo == ORIGIN_TYPE_TARGET_IF_POSSIBLE_OR_SELF)
                                        && TestChrTargetPos != null && TestChrTargetPos.Value != MODEL.CurrentTransformPosition)
                                    {
                                        var dirVectorToTarget = (TestChrTargetPos.Value - MODEL.CurrentTransformPosition);
                                        float distToTarget = dirVectorToTarget.Length();

                                        if (x != 0)
                                        {

                                            float angleToTarget_YAxis = (float)Math.Atan2(-dirVectorToTarget.Z, dirVectorToTarget.X);


                                            // X COMPONENT: ROTATE AROUND CIRCLE (STRAFE LEFT/RIGHT)
                                            float angleToRotateXComponent = -(float)Math.Asin(Math.Abs(x) / (2 * distToTarget));
                                            if (x < 0)
                                                angleToRotateXComponent *= -1;

                                            //NFConsole.PushConsoleTextLine($"angleToRotateXComponent = {angleToRotateXComponent / Math.PI}xPI");
                                            var vectorX = new Vector3(x, 0, 0);
                                            vectorX = Vector3.Transform(vectorX, Matrix.CreateRotationY(angleToRotateXComponent));
                                            vectorX = Vector3.Transform(vectorX, Matrix.CreateRotationY(angleToTarget_YAxis));
                                            vectorX = Vector3.Transform(vectorX, Matrix.CreateRotationY((float)(Math.PI / 2)));


                                            //pos += vectorX;
                                            MODEL.AddRootMotion(Matrix.CreateTranslation(vectorX * new Vector3(-1, 1, -1)));

                                        }


                                        // Y COMPONENT: ROTATE AROUND CIRCLE VERTICALLY (????)
                                        if (y != 0)
                                        {
                                            dirVectorToTarget = (TestChrTargetPos.Value - MODEL.CurrentTransformPosition);
                                            float angleToTarget_XAxis = (float)Math.Atan2(-dirVectorToTarget.Z, -dirVectorToTarget.Y);
                                            float angleToTarget_YAxis = (float)Math.Atan2(-dirVectorToTarget.Z, dirVectorToTarget.X);

                                            float angleToRotateYComponent = -(float)Math.Asin(Math.Abs(y) / (2 * distToTarget));
                                            if (y < 0)
                                                angleToRotateYComponent *= -1;

                                            //NFConsole.PushConsoleTextLine($"angleToRotateYComponent = {angleToRotateYComponent / Math.PI}xPI");
                                            var vectorY = new Vector3(0, y, 0);
                                            vectorY = Vector3.Transform(vectorY, Matrix.CreateRotationX(angleToRotateYComponent));
                                            vectorY = Vector3.Transform(vectorY, Matrix.CreateRotationX(angleToTarget_XAxis));
                                            vectorY = Vector3.Transform(vectorY, Matrix.CreateRotationX((float)(Math.PI / 2)));

                                            vectorY = Vector3.Transform(vectorY, Matrix.CreateRotationY(angleToTarget_YAxis));

                                            //pos += vectorY;
                                            MODEL.AddRootMotion(Matrix.CreateTranslation(vectorY));
                                        }

                                        // Z COMPONENT: CHANGE CIRCLE RADIUS (APPROACH/LEAVE)
                                        if (z != 0)
                                        {
                                            dirVectorToTarget = (TestChrTargetPos.Value - MODEL.CurrentTransformPosition);
                                            var angleToTarget_YAxis = (float)Math.Atan2(-dirVectorToTarget.X, -dirVectorToTarget.Z);
                                            var vectorZ = new Vector3(0, 0, -z);
                                            vectorZ = Vector3.Transform(vectorZ, Matrix.CreateRotationY(angleToTarget_YAxis));

                                            MODEL.AddRootMotion(Matrix.CreateTranslation(vectorZ));
                                        }
                                    }
                                    else if (pMoveRelativeTo == ORIGIN_TYPE_SELF_FACING_DIR || pMoveRelativeTo == ORIGIN_TYPE_TARGET_IF_POSSIBLE_OR_SELF)
                                    {
                                        var worldRootMotionVector = Vector3.Transform(new Vector3(-x, y, -z), Matrix.CreateRotationY(heading));
                                        MODEL.AddRootMotion(Matrix.CreateTranslation(worldRootMotionVector));
                                    }
                                }

                                else if (cfg.SimEnabled_NF_SetTurnSpeedGradient && act.Type == 7021)
                                {
                                    var turnSpeedBlend = new System.Numerics.Vector2(br.GetSingle(0), br.GetSingle(4));
                                    var curveType = br.GetByte(8);


                                    float fadeDuration = act.EndTime - act.StartTime;
                                    if (fadeDuration > 0)
                                    {
                                        float timeSinceFadeStart = slotTime - act.StartTime;
                                        float lerpS = MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1);

                                        lerpS = ProcessNightfallCurve(lerpS, curveType);


                                        activeTrackingSpeedThisFrame = MathHelper.Lerp(turnSpeedBlend.X, turnSpeedBlend.Y, lerpS);
                                    }

                                }

                                else if (cfg.SimEnabled_NF_AnimSpeedGradient && act.Type == 7032)
                                {
                                    var blend = new System.Numerics.Vector2(br.GetSingle(0), br.GetSingle(4));
                                    var curveType = br.GetByte(8);

                                    float fadeDuration = act.EndTime - act.StartTime;
                                    if (fadeDuration > 0)
                                    {
                                        float timeSinceFadeStart = slotTime - act.StartTime;
                                        float lerpS = MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1);

                                        lerpS = ProcessNightfallCurve(lerpS, curveType);

                                        ModPlaybackSpeed_NightfallEvent7032 *= MathHelper.Lerp(blend.X, blend.Y, lerpS);
                                    }
                                }

                                else if (cfg.SimEnabled_NF_RootMotionScale && act.Type == 7027)
                                {
                                    float fadeDuration = act.EndTime - act.StartTime;
                                    float timeSinceFadeStart = slotTime - act.StartTime;

                                    var grad = new System.Numerics.Vector2(br.GetSingle(0), br.GetSingle(4));

                                    TaeRootMotionScaleXZ *= MathHelper.Lerp(grad.X, grad.Y, MathHelper.Clamp(timeSinceFadeStart / fadeDuration, 0, 1));
                                }

                            }
                        }
                        #endregion

                        slotAnim.TaePrevTime = slotTime;
                        slotAnim.TaePrevLoopCount = slotLoopCount;
                        slotAnim.TaePrevFramePlaybackCursorMoving = playbackCursorMovingForward;

                        if (slotType is NewAnimSlot.SlotTypes.Base)
                        {
                            var anySlots = TaeAnimRequest_TaeAddAnim.Any(x => x.TaeAnimID.IsValid);
                            if (!anySlots)
                            {
                                MODEL?.AnimContainer?.AccessAnimSlots(slots =>
                                {
                                    if (slots[NewAnimSlot.SlotTypes.Base].GetForegroundAnimation()?.IsAdditiveBlend == true)
                                    {
                                        for (int i = 0; i < TaeAnimRequest_TaeAddAnim.Length; i++)
                                        {
                                            MODEL.AnimContainer?.NewSetSlotRequest(NewAnimSlot.SlotTypes.TaeExtraAnim0 + i, NewAnimSlot.Request.ForceClear);
                                        }
                                    }
                                });
                                
                            }
                        }
                    });

                    //var acts = slotTaeAnim.Actions;
                    
                }

                
                
                //DoActions(actions, time, timeLastFrame_Deprecated, loopDelta, animDuration);
                
                MODEL.AnimContainer?.AccessAnimSlots(animSlots =>
                {
                    foreach (var kvp in animSlots)
                    {
                        if (kvp.Value != null && kvp.Value.TaeEnabled && (enableOverlayTae || kvp.Key == NewAnimSlot.SlotTypes.Base))
                        {
                            var foregroundAnim = kvp.Value.GetForegroundAnimation();
                            if (foregroundAnim != null && foregroundAnim.TaeAnimation != null)
                            {
                                DoActions(kvp.Key, foregroundAnim);
                                anyValidSlots = true;
                            }
                        }
                    }
                });

                if ((!validBaseAnimSlot) && Graph?.PlaybackCursor != null && MODEL?.AnimContainer != null)
                {
                    FakeAnimationForSim.Data.Duration = (float)Graph.PlaybackCursor.MaxTime;
                    FakeAnimationForSim.EnableLooping = MODEL.AnimContainer.EnableLooping;
                    FakeAnimationForSim.Scrub(absolute: true, (float)Graph.PlaybackCursor.GUICurrentTime, out _, out _, ignoreRootMotion: true);
                    FakeAnimationForSim.TaeAnimation = Graph.MainScreen.SelectedAnim;
                    DoActions(NewAnimSlot.SlotTypes.Base, FakeAnimationForSim);
                }

                Document.SoundManager.NF_PlayDummyPolyFollowSoundEx_ReportUsedExInfoIDsUsedThisFrame(NF_PlayDummyPolyFollowSoundEx_ExInfosUsedThisFrame);
            
            }
            
            // ------------
            // AFTER EVENTS
            // ------------

            for (int i = 0; i < TaeAnimRequest_TaeAddAnim.Length; i++)
            {
                MODEL.AnimContainer?.NewSetSlotRequest(NewAnimSlot.SlotTypes.TaeExtraAnim0 + i, TaeAnimRequest_TaeAddAnim[i]);
            }

            MODEL.AnimContainer?.NewSetSlotRequest(NewAnimSlot.SlotTypes.SekiroFaceAnim, TaeAnimRequest_SekiroFace);

            if (entitySettings != null)
            {
                entitySettings.SetStateInfoSelectConfig_Enabled(stateInfosEnabled);
                entitySettings.SetStateInfoSelectConfig_Names(stateInfoNames);
            }


            MODEL.DummyPolyMan?.ClearAllHitboxesExceptActive(attackIndicesUsed);
            MODEL.ForAllAC6NpcParts((partIndex, part, model) =>
            {
                model.DummyPolyMan?.ClearAllHitboxesExceptActive(attackIndicesUsed);
            });

            if (MODEL.ChrAsm != null)
            {
                if (MODEL.ChrAsm.WeaponStyle != desiredWeaponStyle)
                    MODEL.ChrAsm.WeaponStyle = desiredWeaponStyle;

                if (!anyWeaponOverrideHappeningThisFrame)
                {
                    if (Document.GameRoot.GameType == SoulsGames.DS3)
                        MODEL.ChrAsm?.ClearDS3PairedWeaponMeme();
                    // else if (GameRoot.GameType == SoulsGames.SDT)
                    //     MODEL.ChrAsm?.ClearSekiroProstheticOverrideTae();
                }
            }
            

            if (trackingDisabledThisFrame)
                activeTrackingSpeedThisFrame = 0;

            MODEL.CurrentTrackingSpeed = activeTrackingSpeedThisFrame;
            if (!Main.Input.AnyModifiersHeld && !ImguiOSD.DialogManager.AnyDialogsShowing && !OSD.AnyFieldFocused)
            {
                float trackDir = 0;
                if (Main.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.D))
                    trackDir += 1;
                if (Main.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.A))
                    trackDir -= 1;

                MODEL.TrackingTestInput = Model.GlobalTrackingInput + trackDir;
            }
            
            
            

            

            AnimRestartRequested = false;
            IsMouseClickScrub = false;
        }



        //public static void GlobalPlaybackInstanceStop()
        //{
        //    Document.SoundManager.StopAllSounds();
        //    Document.RumbleCamManager.ClearActive();
        //}

        public readonly zzz_DocumentIns Document;
        public readonly Model MODEL;
        //public NewGraph Graph;

        

        public void NewSimulationInit(List<DSAProj.Action> actions)
        {
            

            foreach (var act in actions)
            {
                act.NewSimulationActive = false;
                act.NewSimulationEnter = false;
                act.NewSimulationExit = false;
            }

            PrepareModelForTaeFrame(MODEL);

            NightfallTaeAddAnimCurWeightAtEventStart = -1;

        }

        public void NewSimulation_DoRightClick(DSAProj.Action ev)
        {
            if (NewSimulation_EvCheck_PlaySound(ev))
                PlayOneShotSoundOfBox(ev, true);
            else if (NewSimulation_EvCheck_RumbleCam(ev))
                PlayRumbleCamOfBox(ev);
        }

        public bool NewSimulation_EvCheck_WeaponLocationOverrides(DSAProj.Action ev)
        {
            return (Document.GameRoot.GameType is SoulsGames.DS3 or SoulsGames.ER or SoulsGames.ERNR && ev.Type == 712)
                || (Document.GameRoot.GameType is SoulsGames.SDT && ev.Type == 715);

        }

        public bool NewSimulation_EvCheck_SpEffects(DSAProj.Action ev)
        {
            if (ev.Type == 66 || ev.Type == 67 || ev.Type == 302 || ev.Type == 401)
                return true;
            if (Document.GameRoot.GameType is SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6)
            {
                if (ev.Type == 331 || ev.Type == 797)
                    return true;
            }

            return false;
        }

        public void NewSimulation_EvProcess_FFX(DSAProj.Action ev)
        {
            if (ev.HasInternalSimField("FFXID") && ev.HasInternalSimField("DummyPolyID"))
            {
                // Using convert here since they might be various numeric
                // value types.
                int ffxid = Convert.ToInt32(ev.ReadInternalSimField("FFXID"));
                int dummyPolyID = Convert.ToInt32(ev.ReadInternalSimField("DummyPolyID"));


                var mdl = GetModelOfBox(ev);

                if (ev.HasInternalSimField("DummyPolySource"))
                {
                    ParamData.AtkParam.DummyPolySource dummyPolySrc = ParamData.AtkParam.DummyPolySource.BaseModel;

                    int dplsVal = Convert.ToInt32(ev.ReadInternalSimField("DummyPolySource"));
                    if (dplsVal == 0)
                    {
                        dummyPolySrc = ParamData.AtkParam.DummyPolySource.BaseModel;
                    }
                    else if (dplsVal == 1)
                    {
                        if (mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHandTransformedL ||
                        mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.LeftBoth)
                        {
                            dummyPolySrc = ParamData.AtkParam.DummyPolySource.LeftWeapon2;
                        }
                        else
                        {
                            dummyPolySrc = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                        }

                    }
                    else if (dplsVal == 2)
                    {
                        if (mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHandTransformedR ||
                        mdl.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.RightBoth)
                        {
                            dummyPolySrc = ParamData.AtkParam.DummyPolySource.RightWeapon2;
                        }
                        else
                        {
                            dummyPolySrc = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                        }

                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    var asmSrc = mdl.ChrAsm?.GetDummyManager(dummyPolySrc);

                    if (asmSrc != null)
                    {
                        asmSrc.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                    }
                    else
                    {
                        mdl.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                    }
                }

                else
                {
                    var asmSrc = mdl.ChrAsm?.GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource.BaseModel, dummyPolyID, mdl.DummyPolyMan);
                    if (asmSrc == null || asmSrc == mdl.DummyPolyMan)
                    {
                        mdl.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                    }
                    else
                    {
                        asmSrc.SpawnSFXOnDummyPoly(ffxid, dummyPolyID % 1000);
                    }
                }


            }
        }

        public void NewSimulation_EvProcess_SpEffects(DSAProj.Action ev, bool wasEvJustEntered)
        {
            int spEffectID = Convert.ToInt32(ev.ReadInternalSimField("SpEffectID"));

            if (Document.ParamManager.SpEffectParam.ContainsKey(spEffectID))
            {
                var spEffect = Document.ParamManager.SpEffectParam[spEffectID];

                if (Document.GameRoot.GameType == SoulsGames.DS1 || Document.GameRoot.GameType == SoulsGames.DS1R)
                {
                    if (spEffect.GrabityRate > 0)
                    {
                        float newSpeed = ModPlaybackSpeed_GrabityRate * spEffect.GrabityRate;
                        //Console.WriteLine($"{ModPlaybackSpeed_GrabityRate} * {spEffect.GrabityRate} = {newSpeed}");
                        ModPlaybackSpeed_GrabityRate = newSpeed;
                    }
                }

                SimulatedActiveSpEffects.Add($"{spEffect.GetDisplayName()}");
            }
            else
            {
                SimulatedActiveSpEffects.Add($"[Doesn't Exist] {spEffectID}");
            }
        }

        public void NewSimulation_EvProcess_WeaponLocationOverrides(DSAProj.Action ev, ref bool anyWeaponLocationOverrideThisFrame)
        {
            if (MODEL.ChrAsm == null)
                return;
            // DS3
            if (ev.Type == 712)
            {
                if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHand)
                {
                    if (Convert.ToByte(ev.ReadInternalSimField("IsEnabledWhile1Handing")) != 1)
                        return;
                }
                else if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.RightBoth)
                {
                    if (Convert.ToByte(ev.ReadInternalSimField("IsEnabledWhile2HandingRightWeapon")) != 1)
                        return;
                }
                else if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.LeftBoth)
                {
                    if (Convert.ToByte(ev.ReadInternalSimField("IsEnabledWhile2HandingLeftWeapon")) != 1)
                        return;
                }

                anyWeaponLocationOverrideThisFrame = true;

                var rightWpnSlot = MODEL.ChrAsm.GetWpnSlot(NewChrAsm.EquipSlotTypes.RightWeapon);
                var leftWpnSlot = MODEL.ChrAsm.GetWpnSlot(NewChrAsm.EquipSlotTypes.LeftWeapon);

                var wepInvisibleTypeFilterR0 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("RHModel0_WepInvisibleTypeFilter"));
                var wepInvisibleTypeFilterR1 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("RHModel1_WepInvisibleTypeFilter"));
                var wepInvisibleTypeFilterR2 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("RHModel2_WepInvisibleTypeFilter"));
                var wepInvisibleTypeFilterR3 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("RHModel3_WepInvisibleTypeFilter"));

                var wepInvisibleTypeFilterL0 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("LHModel0_WepInvisibleTypeFilter"));
                var wepInvisibleTypeFilterL1 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("LHModel1_WepInvisibleTypeFilter"));
                var wepInvisibleTypeFilterL2 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("LHModel2_WepInvisibleTypeFilter"));
                var wepInvisibleTypeFilterL3 = (ParamData.WepAbsorpPosParam.WepInvisibleTypes)Convert.ToByte(ev.ReadInternalSimField("LHModel3_WepInvisibleTypeFilter"));

                var changeTypeR0 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("RHModel0_ChangeType"));
                var changeTypeR1 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("RHModel1_ChangeType"));
                var changeTypeR2 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("RHModel2_ChangeType"));
                var changeTypeR3 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("RHModel3_ChangeType"));
                var changeTypeL0 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("LHModel0_ChangeType"));
                var changeTypeL1 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("LHModel1_ChangeType"));
                var changeTypeL2 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("LHModel2_ChangeType"));
                var changeTypeL3 = (NewChrAsm.DS3MemeAbsorpTaeType)Convert.ToSByte(ev.ReadInternalSimField("LHModel3_ChangeType"));

                

                if (changeTypeR0 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && rightWpnSlot.WepInvisibleTypeFilter0 == wepInvisibleTypeFilterR0)
                    rightWpnSlot.Model0MemeAbsorp = changeTypeR0;

                if (changeTypeR1 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && rightWpnSlot.WepInvisibleTypeFilter1 == wepInvisibleTypeFilterR1)
                    rightWpnSlot.Model1MemeAbsorp = changeTypeR1;

                if (changeTypeR2 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && rightWpnSlot.WepInvisibleTypeFilter2 == wepInvisibleTypeFilterR2)
                    rightWpnSlot.Model2MemeAbsorp = changeTypeR2;

                if (changeTypeR3 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && rightWpnSlot.WepInvisibleTypeFilter3 == wepInvisibleTypeFilterR3)
                    rightWpnSlot.Model3MemeAbsorp = changeTypeR3;

                if (changeTypeL0 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && leftWpnSlot.WepInvisibleTypeFilter0 == wepInvisibleTypeFilterL0)
                    leftWpnSlot.Model0MemeAbsorp = changeTypeL0;

                if (changeTypeL1 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && leftWpnSlot.WepInvisibleTypeFilter1 == wepInvisibleTypeFilterL1)
                    leftWpnSlot.Model1MemeAbsorp = changeTypeL1;

                if (changeTypeL2 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && leftWpnSlot.WepInvisibleTypeFilter2 == wepInvisibleTypeFilterL2)
                    leftWpnSlot.Model2MemeAbsorp = changeTypeL2;

                if (changeTypeL3 != NewChrAsm.DS3MemeAbsorpTaeType.MaintainPreviousValue && leftWpnSlot.WepInvisibleTypeFilter3 == wepInvisibleTypeFilterL3)
                    leftWpnSlot.Model3MemeAbsorp = changeTypeL3;
            }
            else if (ev.Type == 715)
            {
                anyWeaponLocationOverrideThisFrame = true;
                var modelType = Convert.ToByte(ev.ReadInternalSimField("WeaponModelType"));
                var model0DummyPoly = Convert.ToInt32(ev.ReadInternalSimField("Model0DummyPolyID"));
                var model1DummyPoly = Convert.ToInt32(ev.ReadInternalSimField("Model1DummyPolyID"));
                var model2DummyPoly = Convert.ToInt32(ev.ReadInternalSimField("Model2DummyPolyID"));
                var model3DummyPoly = Convert.ToInt32(ev.ReadInternalSimField("Model3DummyPolyID"));
                
                if (modelType == 0) 
                    MODEL.ChrAsm.RegistSekiroProstheticOverride(NewChrAsm.EquipSlotTypes.RightWeapon,
                        model0DummyPoly, model1DummyPoly, model2DummyPoly, model3DummyPoly);
                else if (modelType == 1) 
                    MODEL.ChrAsm.RegistSekiroProstheticOverride(NewChrAsm.EquipSlotTypes.LeftWeapon,
                        model0DummyPoly, model1DummyPoly, model2DummyPoly, model3DummyPoly);
                else if (modelType == 2) 
                    MODEL.ChrAsm.RegistSekiroProstheticOverride(NewChrAsm.EquipSlotTypes.SekiroMortalBlade,
                        model0DummyPoly, model1DummyPoly, model2DummyPoly, model3DummyPoly);
                else if (modelType == 3) 
                    MODEL.ChrAsm.RegistSekiroProstheticOverride(NewChrAsm.EquipSlotTypes.SekiroGrapplingHook,
                        model0DummyPoly, model1DummyPoly, model2DummyPoly, model3DummyPoly);
            }
        }

        public bool NewSimulation_EvCheck_RumbleCam(DSAProj.Action ev)
        {
            return ev.Type == 144 || ev.Type == 145 
                || (Document.GameRoot.GameType is SoulsGames.SDT && (ev.Type == 146 || ev.Type == 147));
        }

        public void NewSimualation_EvProcess_SetAdditiveAnim(DSAProj.Action ev, float time, Func<SplitAnimID, DSAProj.Animation> getAnimFunc)
        {


            int animType = Convert.ToInt32(ev.ReadInternalSimField("AnimType"));

            if (animType >= 0 && animType < 10)
            {

                float a = Convert.ToSingle(ev.ReadInternalSimField("WeightAtEventStart"));
                float b = Convert.ToSingle(ev.ReadInternalSimField("WeightAtEventEnd"));
                float weight = ev.ParameterLerp(time, a, b);

                int directAnimID = -1;

                if (Document.GameRoot.GameType is SoulsGames.AC6)
                {
                    directAnimID = Convert.ToInt32(ev.ReadInternalSimField("DirectAnimID"));
                }

                int finalAnimID = animType;

                if (Document.GameRoot.GameType is SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6)
                    finalAnimID += 40000;
                else if (Document.GameRoot.GameType is SoulsGames.BB)
                    finalAnimID += 10;

                if (ev.HasInternalSimField("AnimType2"))
                {
                    finalAnimID += (10 * Convert.ToInt32(ev.ReadInternalSimField("AnimType2")));

                }

                if (directAnimID >= 0)
                {
                    finalAnimID = directAnimID;
                }

                if (finalAnimID < 0)
                    return;


                TaeAnimRequest_TaeAddAnim[animType].ForceNew = false;
                TaeAnimRequest_TaeAddAnim[animType].TaeAnimID = SplitAnimID.FromFullID(Document.GameRoot, finalAnimID);
                TaeAnimRequest_TaeAddAnim[animType].EnableLoop = true;
                TaeAnimRequest_TaeAddAnim[animType].DesiredWeight = weight;
                TaeAnimRequest_TaeAddAnim[animType].ClearOnEnd = false;
                TaeAnimRequest_TaeAddAnim[animType].KeepPlayingUntilEnd = false;
                TaeAnimRequest_TaeAddAnim[animType].BlendDuration = getAnimFunc?.Invoke(SplitAnimID.FromFullID(Document.GameRoot, finalAnimID))?.SAFE_GetBlendDuration() ?? 0;
            }
        }



        public bool NewSimulation_EvCheck_SetAdditiveAnim(DSAProj.Action ev)
        {
            if (Document.GameRoot.GameType == SoulsGames.BB)
                return ev.Type == 313;
            else if (Document.GameRoot.GameType is SoulsGames.DS3 or SoulsGames.SDT or SoulsGames.ER or SoulsGames.ERNR)
                return ev.Type == 601;
            else if (Document.GameRoot.GameType is SoulsGames.AC6)
                return ev.Type == 601 || ev.Type == 303;

            return false;
        }

        public bool NewSimulation_EvCheck_PlaySound(DSAProj.Action ev)
        {
            if (ev.Type == 128)
            {
                return true;
            }
            else if (ev.Type == 129)
            {
                return true;
            }
            else if (ev.Type == 130 && Document.GameRoot.GameType is
                SoulsGames.DES or
                SoulsGames.DS1 or
                SoulsGames.DS1R or
                SoulsGames.DS3 or
                SoulsGames.SDT or
                SoulsGames.BB)
            {
                return true;
            }
            else if (ev.Type == 131 && Document.GameRoot.GameType is
                SoulsGames.DS3 or
                SoulsGames.SDT or
                SoulsGames.BB)
            {
                return true;
            }
            else if (ev.Type == 132 && Document.GameRoot.GameType is
                SoulsGames.DS3 or
                SoulsGames.ER or
                SoulsGames.ERNR or
                SoulsGames.AC6 or
                SoulsGames.SDT or
                SoulsGames.BB)
            {
                return true;
            }
            else if (ev.Type == 133 && Document.GameRoot.GameType is
                SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsGames.AC6)
            {
                return true;
            }
            else if (ev.Type == 134 && Document.GameRoot.GameType is
                SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsGames.AC6)
            {
                return true;
            }

            else if (ev.Type == 7003 && Main.IsNightfallBuild && Document.GameRoot.GameType is
                SoulsAssetPipeline.SoulsGames.DS1R)
            {
                return true;
            }
            else if (ev.Type == 7031 && Main.IsNightfallBuild && Document.GameRoot.GameType is
                SoulsAssetPipeline.SoulsGames.DS1R)
            {
                return true;
            }
            //else if (ev.Type == 7052 && Main.IsNightfallBuild && GameRoot.GameType is
            //    SoulsAssetPipeline.SoulsGames.DS1R)
            //{
            //    return true;
            //}


            return false;
        }

        public bool NewSimulation_EvCheck_NF_SetTaeExtraAnim(DSAProj.Action ev)
        {
            return (ev.Type == 7029 && Document.GameRoot.GameType is SoulsGames.DS1R);
        }

        public void NewSimulation_EvProcess_NF_SetTaeExtraAnim(DSAProj.Action ev, float time, BinaryReaderEx br)
        {

            int animID = Convert.ToInt32(br.GetInt32(0));

            if (animID < 0)
                return;

            if (ev.NewSimulationEnter || NightfallTaeAddAnimCurWeightAtEventStart < 0)
            {
                NightfallTaeAddAnimCurWeightAtEventStart = MODEL.AnimContainer.GetAnimSlotForegroundWeight(NewAnimSlot.SlotTypes.TaeExtraAnim0);
            }

            var weightGrad = new System.Numerics.Vector2(br.GetSingle(4), br.GetSingle(8));
            float weight = ev.ParameterLerp(time, weightGrad.X, weightGrad.Y);
            float reqLerpS = Convert.ToSingle(br.GetSingle(0xC));
            float evLerpS = ev.GetLerpS(time);
            
            
            TaeAnimRequest_TaeAddAnim[0].ForceNew = false;
            TaeAnimRequest_TaeAddAnim[0].TaeAnimID = SplitAnimID.FromFullID(Document.GameRoot, animID);
            TaeAnimRequest_TaeAddAnim[0].EnableLoop = true;
            TaeAnimRequest_TaeAddAnim[0].DesiredWeight = weight;
            TaeAnimRequest_TaeAddAnim[0].ClearOnEnd = false;
            TaeAnimRequest_TaeAddAnim[0].KeepPlayingUntilEnd = false;
            TaeAnimRequest_TaeAddAnim[0].NF_RequestedLerpS = reqLerpS;
            TaeAnimRequest_TaeAddAnim[0].NF_EvInputLerpS = evLerpS;
            TaeAnimRequest_TaeAddAnim[0].NF_WeightAtEvStart = NightfallTaeAddAnimCurWeightAtEventStart;
            
        }
        
        public static float TaeRootMotionScaleXZ = 1;

        private ParamData.AtkParam.DummyPolySource HitViewDummyPolySource => 
            MODEL.IS_PLAYER ? (OverrideHitViewDummyPolySource ?? Main.Config?.HitViewDummyPolySource ?? ParamData.AtkParam.DummyPolySource.BaseModel) 
            : ParamData.AtkParam.DummyPolySource.BaseModel;

        public ParamData.AtkParam.DummyPolySource? OverrideHitViewDummyPolySource = null;

        public int NF_CurrentDummyPolySoundSlot = 10000;
        public void NF_IncrementCurrentDummyPolySoundSlot()
        {
            NF_CurrentDummyPolySoundSlot++;
            if (NF_CurrentDummyPolySoundSlot >= 30000)
                NF_CurrentDummyPolySoundSlot = 10000;
        }



        public void DoHardReset()
        {
            NightfallTaeAddAnimCurWeightAtEventStart = -1;
            
        }

        private float NightfallTaeAddAnimCurWeightAtEventStart = 0;

        public static float ProcessNightfallCurve(float input, byte curveType)
        {
            switch (curveType)
            {
                case 0:
                    return input;
                case 1:
                    return 1 - (float)(Math.Cos(input * Math.PI) / 2.0 + 0.5);
                case 2:
                    return (float)Math.Sin(input * Math.PI * 0.5);
                default:
                    throw new NotImplementedException($"NIGHTFALL CURVE TYPE {curveType} NOT IMPLEMENTED");
            }
        }

        public TaeActionSimulationEnvironment(zzz_DocumentIns doc, Model mdl)
        {
            Document = doc;
            MODEL = mdl;
        }

        public void ClearBoxStuff()
        {
            //lock (_lock_bladeFFX)
            //{
            //    SpawnFfxBladeEventMap.Clear();
            //}
        }

        //private Dictionary<DSAProj.Event, NewDummyPolyManager.DummyPolyBladeSFX> SpawnFfxBladeEventMap = 
        //    new Dictionary<DSAProj.Event, NewDummyPolyManager.DummyPolyBladeSFX>();

        //private object _lock_bladeFFX = new object();

        private static ParamData.AtkParam.DummyPolySource GetDmySrcFromCStyleType(short cStyleType)
        {
            switch (cStyleType)
            {
                case 0: return ParamData.AtkParam.DummyPolySource.BaseModel;
                case 1: return ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                case 2: return ParamData.AtkParam.DummyPolySource.RightWeapon0;
                default:
                    throw new NotImplementedException($"Unknown SpawnFFX_Blade CStyleType Value: {cStyleType}");
            }
        }



        public bool IsAnimBlendingEnabled = true;

        //private void ActivateBladeFfx(DSAProj.Event evBox)
        //{
        //    if (evBox.Template == null || !evBox.IsStateInfoEnabled)
        //        return;

        //    lock (_lock_bladeFFX)
        //    {
        //        if (!SpawnFfxBladeEventMap.ContainsKey(evBox))
        //        {
        //            var ffxid = Convert.ToInt32(evBox.Parameters["FFXID"]);
        //            var styleType = Convert.ToInt16(evBox.Parameters["CStyleType"]);
        //            var dmy1 = Convert.ToInt16(evBox.Parameters["DummyPolyBladeBaseID"]);
        //            var dmy2 = Convert.ToInt16(evBox.Parameters["DummyPolyBladeTipID"]);

        //            var newBlade = new NewDummyPolyManager.DummyPolyBladeSFX(ffxid, dmy1, dmy2, 
        //                GetDmySrcFromCStyleType(styleType), () => evBox.PlaybackHighlight);

        //            newBlade.UpdateLive(MODEL.DummyPolyMan);
        //            newBlade.UpdateLowHz(MODEL.DummyPolyMan);

        //            SpawnFfxBladeEventMap.Add(evBox, newBlade);
        //        }
        //    }
        //}

        //public void UpdateAllBladeSFXsLowHz()
        //{
        //    lock (_lock_bladeFFX)
        //    {
        //        foreach (var kvp in SpawnFfxBladeEventMap)
        //        {
        //            kvp.Value.UpdateLowHz(MODEL.DummyPolyMan);
        //        }
        //    }
        //}

        //public void UpdateAllBladeSFXsLive()
        //{
        //    lock (_lock_bladeFFX)
        //    {
        //        var deadBlades = new List<DSAProj.Event>();
        //        foreach (var kvp in SpawnFfxBladeEventMap)
        //        {
        //            kvp.Value.UpdateLive(MODEL.DummyPolyMan);

        //            if (!kvp.Value.IsAlive)
        //                deadBlades.Add(kvp.Key);
        //        }

        //        foreach (var k in deadBlades)
        //        {
        //            SpawnFfxBladeEventMap.Remove(k);
        //        }
        //    }
        //}

        //public void DrawAllBladeSFXs(Matrix m)
        //{
        //    lock (_lock_bladeFFX)
        //    {
        //        foreach (var kvp in SpawnFfxBladeEventMap)
        //        {
        //            kvp.Value.DoDebugDraw(m);
        //        }
        //    }
        //}


        public List<string> SimulatedActiveSpEffects = new List<string>();

        public const int EVID_InvokeAttackBehavior = 1;
        public const int EVID_InvokeThrowDamageBehavior = 304; //DS1
        public const int EVID_InvokeBulletBehavior = 2;
        public const int EVID_InvokeBulletBehavior_Type4_SDT = 4;
        public const int EVID_InvokeCommonBehavior = 5;
        public const int EVID_InvokeBulletBehaviorDefaultLocation = 8; //ac6
        public const int EVID_InvokePCBehavior = 307; //ds1
        public const int EVID_InvokeGunBehavior = 318; //bb



        private ParamData.BehaviorParam GetBehaviorParamFromEvBox(DSAProj.Action evBox, ParamData.AtkParam.DummyPolySource dummyPolySource)
        {
            if (evBox.Type == EVID_InvokeAttackBehavior || 
                evBox.Type == EVID_InvokeThrowDamageBehavior ||
                evBox.Type == EVID_InvokeBulletBehavior ||
                (evBox.Type == EVID_InvokeBulletBehavior_Type4_SDT && Document.GameRoot.GameType is SoulsGames.SDT) ||
                evBox.Type == EVID_InvokeCommonBehavior ||
                evBox.Type == EVID_InvokePCBehavior ||
                evBox.Type == EVID_InvokeGunBehavior ||
                evBox.Type == EVID_InvokeBulletBehaviorDefaultLocation)
            {


                int behaviorSubID = Convert.ToInt32(evBox.ReadInternalSimField("BehaviorJudgeID"));
                if (MODEL.IS_PLAYER)
                {
                    int behBase = 10_0000_000;
                    int behVariationMult = 1_000;

                    if ((Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR) && behaviorSubID >= 1000 && behaviorSubID <= 9999)
                    {
                        behBase = 10_0000_000 * (behaviorSubID / 1_000);
                        behaviorSubID %= 1000;
                    }

                    var rightWeapon = MODEL.ChrAsm?.GetWpnSlot(NewChrAsm.EquipSlotTypes.RightWeapon).EquipParam;
                    var leftWeapon = MODEL.ChrAsm?.GetWpnSlot(NewChrAsm.EquipSlotTypes.LeftWeapon).EquipParam;

                    var id = -1;
                    if (evBox.Type == EVID_InvokeCommonBehavior)
                        id = behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0 && rightWeapon != null)
                        id = behBase + (rightWeapon.BehaviorVariationID * behVariationMult) + behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0 && leftWeapon != null)
                        id = behBase + (leftWeapon.BehaviorVariationID * behVariationMult) + behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.BaseModel)
                        id = behaviorSubID;

                    if (Document.ParamManager.BehaviorParam_PC.ContainsKey(id))
                        return Document.ParamManager.BehaviorParam_PC[id];

                    if (dummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0 && rightWeapon != null)
                        id = behBase + (rightWeapon.FallbackBehaviorVariationID * behVariationMult) + behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0 && leftWeapon != null)
                        id = behBase + (leftWeapon.FallbackBehaviorVariationID * behVariationMult) + behaviorSubID;

                    if (Document.ParamManager.BehaviorParam_PC.ContainsKey(id))
                        return Document.ParamManager.BehaviorParam_PC[id];

                    if (Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
                    {
                        if (dummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0 && rightWeapon != null)
                            id = behBase + (0000 * behVariationMult) + behaviorSubID;
                        else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0 && leftWeapon != null)
                            id = behBase + (0000 * behVariationMult) + behaviorSubID;

                        if (Document.ParamManager.BehaviorParam_PC.ContainsKey(id))
                            return Document.ParamManager.BehaviorParam_PC[id];
                    }
                }
                else
                {
                    if (MODEL.NpcParam == null)
                        return null;

                    int id = -1;

                    if (evBox.Type == EVID_InvokeCommonBehavior)
                    {
                        id = behaviorSubID;
                    }
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.BaseModel)
                    {
                        id = 2_00000_000 + (MODEL.NpcParam.BehaviorVariationID * 1_000) + behaviorSubID;
                    }

                    if (Document.ParamManager.BehaviorParam.ContainsKey(id))
                        return Document.ParamManager.BehaviorParam[id];
                }
            }

            return null;
        }

        private ParamData.AtkParam GetAtkParamFromEventBox(DSAProj.Action evBox, ParamData.AtkParam.DummyPolySource dummyPolySource, out bool isNpcAtk, out ParamData.BehaviorParam behaviorParam)
        {
            behaviorParam = GetBehaviorParamFromEvBox(evBox, dummyPolySource);

            if (behaviorParam == null)
            {
                isNpcAtk = false;
                return null;
            }

            if (behaviorParam.RefType == ParamData.BehaviorParam.RefTypes.Attack)
            {
                if (MODEL.IS_PLAYER)
                {
                    if (Document.ParamManager.AtkParam_Pc.ContainsKey(behaviorParam.RefID))
                    {
                        isNpcAtk = false;
                        return Document.ParamManager.AtkParam_Pc[behaviorParam.RefID];
                    }
                }
                else
                {
                    if (Document.ParamManager.AtkParam_Npc.ContainsKey(behaviorParam.RefID))
                    {
                        isNpcAtk = true;
                        return Document.ParamManager.AtkParam_Npc[behaviorParam.RefID];
                    }
                }
            }

            isNpcAtk = false;
            return null;
        }

        private int GetBulletParamIDFromEvBox(DSAProj.Action evBox, ParamData.AtkParam.DummyPolySource dummyPolySource)
        {
            var behaviorParam = GetBehaviorParamFromEvBox(evBox, dummyPolySource);

            if (behaviorParam == null)
                return -1;

            if (behaviorParam.RefType == ParamData.BehaviorParam.RefTypes.Bullet)
            {
                return behaviorParam.RefID;
            }

            return -1;
        }

        //private NewDummyPolyManager GetCurrentDummyPolyMan()
        //{
        //    if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
        //    {
        //        return MODEL.DummyPolyMan;
        //    }
        //    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon &&
        //        MODEL.ChrAsm?.RightWeaponModel != null)
        //    {
        //        return MODEL.ChrAsm.RightWeaponModel.DummyPolyMan;
        //    }
        //    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon &&
        //        MODEL.ChrAsm?.LeftWeaponModel != null)
        //    {
        //        return MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan;
        //    }

        //    return null;
        //}

        public Model GetModelOfBox(DSAProj.Action evBox)
        {
            var mdl = MODEL;

            if (IsRemoModeEnabled)
            {
                mdl = RemoManager.LookupModelOfEventGroup(evBox.GetTrack(GraphForRemoSpecifically.AnimRef)) ?? MODEL;
            }

            return mdl;
        }

        public void PlayRumbleCamOfBox(DSAProj.Action ev)
        {
            if (NewSimulation_EvCheck_RumbleCam(ev) && ev.TypeName != null)
            {
                var rumbleCamID = Convert.ToInt16(ev.ReadInternalSimField("RumbleCamID"));
                Vector3? location = null;
                float falloffStart = -1;
                float falloffEnd = -1;
                if (ev.Type == 144 || (ev.Type == 147 && Document.GameRoot.GameType == SoulsGames.SDT))
                {
                    var dummyPolyID = Convert.ToUInt16(ev.ReadInternalSimField("DummyPolyID"));
                    var locations = MODEL.DummyPolyMan.GetDummyPosByID(dummyPolyID, getAbsoluteWorldPos: true);

                    if (locations.Count > 0)
                        location = locations[0];
                    else
                        location = MODEL.CurrentTransformPosition;

                    falloffStart = (float)ev.ReadInternalSimField("FalloffStart");
                    falloffEnd = (float)ev.ReadInternalSimField("FalloffEnd");
                }

                // Note: type 146 and 147 in sekiro have 'bool UnkFlag', figure out what it does

                //RumbleCamManager.ClearActive();
                Document.RumbleCamManager.AddRumbleCam(Document, rumbleCamID, location, falloffStart, falloffEnd);
            }
        }

        //public string PeekSoundNameOfBox(DSAProj.Event evBox)
        //{
        //    if (evBox.TypeName == null)
        //        return null;

        //    if (!(evBox.TypeName.Contains("Play") && evBox.TypeName.Contains("Sound")))
        //        return null;

        //    int soundType = Convert.ToInt32(evBox.Parameters["SoundType"]);
        //    int soundID = Convert.ToInt32(evBox.Parameters["SoundID"]);

        //    if (GameRoot.GameTypeUsesWwise && evBox.TypeName.StartsWith("Wwise"))
        //    {
        //        if (soundType == 8) // 8 = Floor Material Determined
        //        {
        //            soundType = 1;
        //            soundID += (Wwise.GetDefensiveMaterialParamID() - 1);
        //        }
        //        else if (soundType == 9) // 9 = Armor Material Determined
        //        {
        //            soundType = 1;
        //            soundID += ((int)FmodManager.ArmorMaterial);
        //        }

        //        var wwiseEventName = Wwise.GetSoundName(soundType, soundID);

        //        return wwiseEventName;
        //    }
        //    else
        //    {
        //        return FmodManager.GetSoundName(soundType, soundID);
        //    }
        //}

        //public DSAProj.Event MouseClickPreviewSlot_EvBox = null;
        //public int MouseClickPreviewSlot_SlotID = -1;
        //public string MouseClickPreviewSlot_EventName = null;
        //public int MouseClickPreviewSlot_DummyPolyID = -1;

        //public void DoMouseClickPreviewOfBoxSound(DSAProj.Event evBox)
        //{
        //    if (MouseClickPreviewSlot_EvBox != null && MouseClickPreviewSlot_SlotID >= 0 && MouseClickPreviewSlot_EventName != null)
        //    {
        //        return;
        //    }

        //    if (evBox.Template.ContainsKey("SlotID"))
        //    {
        //        int slot = Convert.ToInt32(evBox.Parameters["SlotID"]);
        //        int dummyPoly = -1;


        //        MouseClickPreviewSlot_SlotID = slot;
        //        MouseClickPreviewSlot_EvBox = evBox;
        //        MouseClickPreviewSlot_EventName = PeekSoundNameOfBox(evBox);
        //    }
            
        //}

        public SoundPlayInfo GetSoundPlayInfoOfBox(DSAProj.Action evBox, bool isForOneShot)
        {
            var internalTemplateTypeName = evBox.GetInternalSimTypeName();

            if (internalTemplateTypeName == null)
                return null;

            if (!(internalTemplateTypeName.Contains("Play") && internalTemplateTypeName.Contains("Sound")))
                return null;

            //Graph?.ViewportInteractor?.LoadSoundsForCurrentModel(fresh: false);

            int soundType = Convert.ToInt32(evBox.ReadInternalSimField("SoundType"));
            int soundID = Convert.ToInt32(evBox.ReadInternalSimField("SoundID"));
            float lifetime = -1;

            bool updatingPosition = true;


            if ((Document.GameRoot.GameType == SoulsGames.DS1 || Document.GameRoot.GameType == SoulsGames.DS1R) && isForOneShot)
            {
                updatingPosition = false;
            }

            int dummyPolyID = -1;

            if (evBox.HasInternalSimField("DummyPolyID"))
            {
                dummyPolyID = Convert.ToInt32(evBox.ReadInternalSimField("DummyPolyID"));

                if (Main.IsNightfallBuild && internalTemplateTypeName.StartsWith("NF_PlaySoundEx"))
                {
                    if (dummyPolyID <= 0)
                        dummyPolyID = -1;
                }



            }


            if (Main.IsNightfallBuild && internalTemplateTypeName == "NF_PlayDummyPolyFollowSound")
            {
                lifetime = Convert.ToSingle(evBox.ReadInternalSimField("Lifetime"));
                updatingPosition = true;
            }
            else if (Main.IsNightfallBuild && internalTemplateTypeName == "NF_PlayDummyPolyFollowSoundEx")
            {
                var lifetimeMin = Convert.ToSingle(evBox.ReadInternalSimField("LifetimeMin"));
                var lifetimeMax = Convert.ToSingle(evBox.ReadInternalSimField("LifetimeMax"));

                if (lifetimeMax < lifetimeMin)
                    lifetimeMax = lifetimeMin;

                lifetime = (float)(lifetimeMin + (Main.Rand.NextDouble() * (lifetimeMax - lifetimeMin)));
                updatingPosition = true;
            }


            var soundPlaybackModel = MODEL;

            if (IsRemoModeEnabled)
            {
                soundPlaybackModel = RemoManager.LookupModelOfEventGroup(evBox.GetTrack(GraphForRemoSpecifically.AnimRef)) ?? MODEL;
            }

            if (IsRemoModeEnabled)
            {
                //getPosFunc = () => GFX.World.CameraLocationInWorld_CloserForSound.Position * new Vector3(1, 1, -1);

                if (soundType == 9) // Armor material dependant
                {
                    soundID -= 56;
                }
            }




            if (soundType == 8) // Floor Material Dependent
            {
                
                soundType = 1; // Change to character sound
                if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.Wwise)
                    soundID += (Document.SoundManager.WwiseManager.GetDefensiveMaterialParamID() - 1);
                else if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.FMOD)
                    soundID += (Document.Fmod.FloorMaterial - 1);
                else if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.MagicOrchestra)
                    soundID += (Document.Fmod.FloorMaterial - 1);

            }
            else if (soundType == 9) // Armor Material Dependent
            {
                soundType = 1; // Change to character sound
                if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.Wwise)
                    soundID += Document.SoundManager.WwiseManager.ArmorMaterial_Top;
                else if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.FMOD)
                    soundID += Document.Fmod.ArmorMaterial;
                else if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.MagicOrchestra)
                    soundID += Document.Fmod.ArmorMaterial;
            }

            bool killSoundOnEventEnd = true;
            //if (GameRoot.GameTypeUsesWwise && evBox.HasInternalSimField("KillSoundOnEventEnd"))
            //{
            //    killSoundOnEventEnd = (bool)evBox.ReadInternalSimField("KillSoundOnEventEnd");
            //}

            bool dontKillOneShotUntilComplete = false;

            bool isWwise = Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.Wwise;

            if (isWwise && evBox.HasInternalSimField("PlaybackBehaviorType"))
            {
                byte playbackBehaviorType = Convert.ToByte(evBox.ReadInternalSimField("PlaybackBehaviorType"));

                killSoundOnEventEnd = playbackBehaviorType > 0;

                //if (playbackBehaviorType == 255)
                //    dontKillOneShotUntilComplete = true;
            }
            else if (isWwise && evBox.HasInternalSimField("PlaybackBehaviorType_AC6"))
            {
                byte playbackBehaviorType = Convert.ToByte(evBox.ReadInternalSimField("PlaybackBehaviorType_AC6"));

                killSoundOnEventEnd = playbackBehaviorType > 0;

                //if (playbackBehaviorType == 255)
                //    dontKillOneShotUntilComplete = true;
            }

            if (evBox.Type == 130 && Document.GameRoot.GameType is
                SoulsGames.DES or
                SoulsGames.DS1 or
                SoulsGames.DS1R or
                SoulsGames.DS3 or
                SoulsGames.SDT or
                SoulsGames.BB)
            {
                if (MODEL.ChrAsm != null)
                {
                    if (MODEL.ChrAsm.IsFemale)
                    {
                        soundID++;
                    }
                }
            }

                return new SoundPlayInfo(Document)
            {
                DummyPolyID = dummyPolyID,
                SoundType = soundType,
                SoundID = soundID,
                SourceModel = soundPlaybackModel,
                UpdatingPosition = updatingPosition,
                NightfallLifetime = lifetime,
                KillSoundOnActionEnd = killSoundOnEventEnd,
                DoNotKillOneShotUntilComplete = dontKillOneShotUntilComplete,
            };

            
        }

        public void NF_PlayDummyPolyFollowSoundEx(DSAProj.Action actBox, bool firstFrameEntered)
        {
            var soundInfo = GetSoundPlayInfoOfBox(actBox, isForOneShot: true);
            if (soundInfo == null)
                return;

            if (actBox.Type == 7052 && Document.GameRoot.GameType is SoulsGames.DS1R && Main.IsNightfallBuild)
            {
                // Already done in GetSoundPlayInfoOfBox
                //float lifetimeMin = Convert.ToSingle(actBox.ReadInternalSimField("LifetimeMin"));
                //float lifetimeMax = Convert.ToSingle(actBox.ReadInternalSimField("LifetimeMax"));
                //soundInfo.NightfallLifetime = (float)(lifetimeMin + (Main.Rand.NextDouble() * (lifetimeMax - lifetimeMin)));

                int exInfoID = Convert.ToInt32(actBox.ReadInternalSimField("ExInfoID"));
                float cooldownMin = Convert.ToSingle(actBox.ReadInternalSimField("CooldownMin"));
                float cooldownMax = Convert.ToSingle(actBox.ReadInternalSimField("CooldownMax"));

                if (cooldownMax < cooldownMin)
                    cooldownMax = cooldownMin;

                float cooldown = (float)(cooldownMin + (Main.Rand.NextDouble() * (cooldownMax - cooldownMin)));

                bool pRepeatDuringAction = Convert.ToBoolean(actBox.ReadInternalSimField("RepeatDuringAction"));
                bool pStopAllSoundsWhenActionEnds = Convert.ToBoolean(actBox.ReadInternalSimField("StopAllSoundsWhenActionEnds"));

                if (!pRepeatDuringAction && !firstFrameEntered)
                    return;

                var exInfo = Document.SoundManager.NF_Act7052_PlayDummyPolyFollowSoundEx_GetExInfo(exInfoID);

                if (exInfo.Cooldown <= 0)
                {
                    soundInfo.UpdatingPosition = true;
                    exInfo.Cooldown = cooldown;
                    exInfo.ClearWhenCooldownIDNotInUse = pStopAllSoundsWhenActionEnds;
                    var newSlot = Document.SoundManager.PlayOneShotSound(soundInfo);
                    if (!exInfo.UsedSlots.Contains(newSlot))
                        exInfo.UsedSlots.Add(newSlot);
                }


            }
        }

        public void PlayOneShotSoundOfBox(DSAProj.Action actBox, bool firstFrameEntered)
        {
            var soundInfo = GetSoundPlayInfoOfBox(actBox, isForOneShot: true);

            if (soundInfo == null)
                return;

            int slot = -1;
            if (actBox.HasInternalSimField("SlotID"))
            {
                slot = Convert.ToInt32(actBox.ReadInternalSimField("SlotID"));
            }

            bool isAutoSlot = false;

            bool killSoundOnEventEnd = false;

            bool isWwise = Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.Wwise;

            if (isWwise && actBox.HasInternalSimField("PlaybackBehaviorType"))
            {
                byte playbackBehaviorType = Convert.ToByte(actBox.ReadInternalSimField("PlaybackBehaviorType"));

                killSoundOnEventEnd = playbackBehaviorType > 0;

                isAutoSlot = killSoundOnEventEnd;
            }
            else if (isWwise && actBox.HasInternalSimField("PlaybackBehaviorType_AC6"))
            {
                byte playbackBehaviorType = Convert.ToByte(actBox.ReadInternalSimField("PlaybackBehaviorType_AC6"));

                killSoundOnEventEnd = playbackBehaviorType > 0;

                isAutoSlot = killSoundOnEventEnd;
            }

            //if (evBox.HasInternalSimField("KillSoundOnEventEnd"))
            //{
            //    isAutoSlot = (bool)(evBox.ReadInternalSimField("KillSoundOnEventEnd"));
            //}

            var internalTemplateTypeName = actBox.GetInternalSimTypeName();

            if (Main.IsNightfallBuild && internalTemplateTypeName.StartsWith("NF_PlaySoundEx"))
            {
                if (soundInfo.DummyPolyID > 0)
                    slot = Convert.ToInt32(actBox.ReadInternalSimField("SlotID"));
            }
            else if (Main.IsNightfallBuild && internalTemplateTypeName == "NF_PlayDummyPolyFollowSound")
            {
                soundInfo.NightfallLifetime = Convert.ToSingle(actBox.ReadInternalSimField("Lifetime"));
                
                //slot = NF_CurrentDummyPolySoundSlot;
                slot = -1;
                soundInfo.UpdatingPosition = true;
            }


            if (slot >= 0 || isAutoSlot)
                return;

            Document.SoundManager.PlayOneShotSound(soundInfo);

        }

        public void OnNewAnimSelected(List<DSAProj.Action> actions)
        {


            if (MODEL == null)
                return;

            NewSimulationInit(actions);
        }

        public void OnSimulationFrameChange(NewGraph graphOptional, Func<SplitAnimID, DSAProj.Animation> getAnimFunc, SplitAnimID taeAnimID, bool justStartedPlayback, bool ignoreBlendProcess = false)
        {
            if (MODEL == null)
                return;

            try
            {
                NewSimulationScrub(graphOptional, getAnimFunc, taeAnimID, justStartedPlayback, ignoreBlendProcess);
            }
            catch (Exception ex) when (Main.EnableErrorHandler.ActionSimUpdate)
            {
                //Console.WriteLine("bruh");
                zzz_NotificationManagerIns.PushNotification($"ERROR SIMULATING ACTIONS:\n{ex}");
            }

            //NewSimulationScrub(time, (float)Graph.PlaybackCursor.CurrentTime, evBoxes);
        }
    }
}
