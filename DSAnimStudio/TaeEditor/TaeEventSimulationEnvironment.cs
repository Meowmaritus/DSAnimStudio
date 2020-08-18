using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEventSimulationEnvironment
    {
        public Model MODEL;
        public TaeEditAnimEventGraph Graph;

        private ParamData.AtkParam.DummyPolySource HitViewDummyPolySource => 
            MODEL.IS_PLAYER ? (OverrideHitViewDummyPolySource ?? Graph?.MainScreen.Config.HitViewDummyPolySource ?? ParamData.AtkParam.DummyPolySource.Body) 
            : ParamData.AtkParam.DummyPolySource.Body;

        public ParamData.AtkParam.DummyPolySource? OverrideHitViewDummyPolySource = null;

        public TaeEventSimulationEnvironment(TaeEditAnimEventGraph graph, Model mdl)
        {
            MODEL = mdl;
            Graph = graph;

            InitAllEntries();
        }

        public void ClearBoxStuff()
        {
            lock (_lock_bladeFFX)
            {
                SpawnFfxBladeEventMap.Clear();
            }
        }

        private Dictionary<TaeEditAnimEventBox, NewDummyPolyManager.DummyPolyBladeSFX> SpawnFfxBladeEventMap = 
            new Dictionary<TaeEditAnimEventBox, NewDummyPolyManager.DummyPolyBladeSFX>();

        private object _lock_bladeFFX = new object();

        private static ParamData.AtkParam.DummyPolySource GetDmySrcFromCStyleType(short cStyleType)
        {
            switch (cStyleType)
            {
                case 0: return ParamData.AtkParam.DummyPolySource.Body;
                case 1: return ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                case 2: return ParamData.AtkParam.DummyPolySource.RightWeapon0;
                default:
                    throw new NotImplementedException($"Unknown SpawnFFX_Blade CStyleType Value: {cStyleType}");
            }
        }

        private void ActivateBladeFfx(TaeEditAnimEventBox evBox)
        {
            if (evBox.MyEvent.Template == null)
                return;

            lock (_lock_bladeFFX)
            {
                if (!SpawnFfxBladeEventMap.ContainsKey(evBox))
                {
                    var ffxid = Convert.ToInt32(evBox.MyEvent.Parameters["FFXID"]);
                    var styleType = Convert.ToInt16(evBox.MyEvent.Parameters["CStyleType"]);
                    var dmy1 = Convert.ToInt16(evBox.MyEvent.Parameters["DummyPolyBladeBaseID"]);
                    var dmy2 = Convert.ToInt16(evBox.MyEvent.Parameters["DummyPolyBladeTipID"]);

                    var newBlade = new NewDummyPolyManager.DummyPolyBladeSFX(ffxid, dmy1, dmy2, 
                        GetDmySrcFromCStyleType(styleType), () => evBox.PlaybackHighlight);

                    newBlade.UpdateLive(MODEL.DummyPolyMan);
                    newBlade.UpdateLowHz(MODEL.DummyPolyMan);

                    SpawnFfxBladeEventMap.Add(evBox, newBlade);
                }
            }
        }

        public void UpdateAllBladeSFXsLowHz()
        {
            lock (_lock_bladeFFX)
            {
                foreach (var kvp in SpawnFfxBladeEventMap)
                {
                    kvp.Value.UpdateLowHz(MODEL.DummyPolyMan);
                }
            }
        }

        public void UpdateAllBladeSFXsLive()
        {
            lock (_lock_bladeFFX)
            {
                var deadBlades = new List<TaeEditAnimEventBox>();
                foreach (var kvp in SpawnFfxBladeEventMap)
                {
                    kvp.Value.UpdateLive(MODEL.DummyPolyMan);

                    if (!kvp.Value.IsAlive)
                        deadBlades.Add(kvp.Key);
                }

                foreach (var k in deadBlades)
                {
                    SpawnFfxBladeEventMap.Remove(k);
                }
            }
        }

        public void DrawAllBladeSFXs(Matrix m)
        {
            lock (_lock_bladeFFX)
            {
                foreach (var kvp in SpawnFfxBladeEventMap)
                {
                    kvp.Value.DoDebugDraw(m);
                }
            }
        }

        private Dictionary<string, EventSimEntry> entries;
        public IReadOnlyDictionary<string, EventSimEntry> Entries => entries;

        public List<string> SimulatedActiveSpEffects = new List<string>();

        private ParamData.BehaviorParam GetBehaviorParamFromEvBox(TaeEditAnimEventBox evBox, ParamData.AtkParam.DummyPolySource dummyPolySource)
        {
            if (evBox.MyEvent.TypeName == "InvokeAttackBehavior" || 
                evBox.MyEvent.TypeName == "InvokeThrowDamageBehavior" ||
                evBox.MyEvent.TypeName == "InvokeBulletBehavior" ||
                evBox.MyEvent.TypeName == "InvokeCommonBehavior" ||
                evBox.MyEvent.TypeName == "InvokePCBehavior" ||
                evBox.MyEvent.TypeName == "InvokeGunBehavior")
            {
                int behaviorSubID = (int)evBox.MyEvent.Parameters["BehaviorJudgeID"];
                if (MODEL.IS_PLAYER)
                {
                    var id = -1;
                    if (evBox.MyEvent.TypeName == "InvokeCommonBehavior")
                        id = behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0 && MODEL.ChrAsm?.RightWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.RightWeapon.BehaviorVariationID * 1_000) + behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0 && MODEL.ChrAsm?.LeftWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.LeftWeapon.BehaviorVariationID * 1_000) + behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                        id = behaviorSubID;

                    if (ParamManager.BehaviorParam_PC.ContainsKey(id))
                        return ParamManager.BehaviorParam_PC[id];

                    if (dummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon0 && MODEL.ChrAsm?.RightWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.RightWeapon.FallbackBehaviorVariationID * 1_000) + behaviorSubID;
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon0 && MODEL.ChrAsm?.LeftWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.LeftWeapon.FallbackBehaviorVariationID * 1_000) + behaviorSubID;

                    if (ParamManager.BehaviorParam_PC.ContainsKey(id))
                        return ParamManager.BehaviorParam_PC[id];
                }
                else
                {
                    if (MODEL.NpcParam == null)
                        return null;

                    int id = -1;

                    if (evBox.MyEvent.TypeName == "InvokeCommonBehavior")
                    {
                        id = behaviorSubID;
                    }
                    else if (dummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                    {
                        id = 2_00000_000 + (MODEL.NpcParam.BehaviorVariationID * 1_000) + behaviorSubID;
                    }

                    if (ParamManager.BehaviorParam.ContainsKey(id))
                        return ParamManager.BehaviorParam[id];
                }
            }

            return null;
        }

        private ParamData.AtkParam GetAtkParamFromEventBox(TaeEditAnimEventBox evBox, ParamData.AtkParam.DummyPolySource dummyPolySource)
        {
            var behaviorParam = GetBehaviorParamFromEvBox(evBox, dummyPolySource);

            if (behaviorParam == null)
                return null;

            if (behaviorParam.RefType == ParamData.BehaviorParam.RefTypes.Attack)
            {
                if (MODEL.IS_PLAYER)
                {
                    if (ParamManager.AtkParam_Pc.ContainsKey(behaviorParam.RefID))
                        return ParamManager.AtkParam_Pc[behaviorParam.RefID];
                }
                else
                {
                    if (ParamManager.AtkParam_Npc.ContainsKey(behaviorParam.RefID))
                        return ParamManager.AtkParam_Npc[behaviorParam.RefID];
                }
            }

            return null;
        }

        private int GetBulletParamIDFromEvBox(TaeEditAnimEventBox evBox)
        {
            var behaviorParam = GetBehaviorParamFromEvBox(evBox, HitViewDummyPolySource);

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

        public void PlaySoundEffectOfBox(TaeEditAnimEventBox evBox)
        {
            if (evBox.MyEvent.TypeName == null || !evBox.MyEvent.TypeName.StartsWith("PlaySound"))
                return;

            int soundType = Convert.ToInt32(evBox.MyEvent.Parameters["SoundType"]);
            int soundID = Convert.ToInt32(evBox.MyEvent.Parameters["SoundID"]);

            int? stateInfoSlot = null;

            if (evBox.MyEvent.TypeName.StartsWith("PlaySound_ByStateInfo"))
            {
                stateInfoSlot = Convert.ToInt32(evBox.MyEvent.Parameters["SlotNumber"]);
            }

            Func<Vector3> getPosFunc = null;

            if (evBox.MyEvent.Template.ContainsKey("DummyPolyID"))
            {
                int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);

                getPosFunc = () =>
                {
                    if (dummyPolyID == -1)
                    {
                        return Vector3.Transform(Vector3.Zero, MODEL.CurrentTransform.WorldMatrix) + new Vector3(0, GFX.World.ModelHeight_ForOrbitCam / 2, 0);
                    }

                    if (MODEL.DummyPolyMan.DummyPolyByRefID.ContainsKey(dummyPolyID))
                    {
                        return Vector3.Transform(Vector3.Zero, 
                            MODEL.DummyPolyMan.DummyPolyByRefID[dummyPolyID][0].CurrentMatrix 
                            * MODEL.CurrentTransform.WorldMatrix);
                    }

                    return Vector3.Transform(Vector3.Zero, MODEL.CurrentTransform.WorldMatrix) + new Vector3(0, GFX.World.ModelHeight_ForOrbitCam / 2, 0);
                };

            }
            else
            {
                getPosFunc = () => Vector3.Transform(Vector3.Zero, MODEL.CurrentTransform.WorldMatrix) + new Vector3(0, GFX.World.ModelHeight_ForOrbitCam / 2, 0);
            }

            if (stateInfoSlot != null && stateInfoSlot.Value >= 0 && FmodManager.IsStateInfoAlreadyPlaying(stateInfoSlot.Value))
            {
                FmodManager.StopSE(stateInfoSlot.Value, true);
            }
            FmodManager.PlaySE(soundType, soundID, getPosFunc, stateInfoSlot == -1 ? null : stateInfoSlot);
        }

        public void RootMotionWrapForBlades(Vector3 wrap)
        {
            foreach (var kvp in SpawnFfxBladeEventMap)
            {
                var blade = kvp.Value;
                blade.CurrentPos.Start += wrap;
                blade.CurrentPos.End += wrap;

                var asList = blade.Keyframes.ToList();
                for (int i = 0; i < blade.Keyframes.Count; i++)
                {
                    var b = asList[i];
                    b.Start += wrap;
                    b.End += wrap;
                    asList[i] = b;
                }

                blade.Keyframes = new Queue<NewDummyPolyManager.DummyPolyBladePos>(asList);
            }
        }

        private void InitAllEntries()
        {
            entries = new Dictionary<string, EventSimEntry>
            {

                {
                    "EventSimSE",
                    new EventSimEntry("Simulate Sound Effects", true)
                    {
                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            var thisBoxEntered = evBox.WasJustEnteredDuringPlayback;
                            if (!evBox.PrevFrameEnteredState_ForSoundEffectPlayback && thisBoxEntered)
                            {
                                PlaySoundEffectOfBox(evBox);
                            }
                            evBox.PrevFrameEnteredState_ForSoundEffectPlayback = thisBoxEntered;

                            
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            List<int> allStateInfos = new List<int>();
                            List<int> activeStateInfos = new List<int>();
                            foreach (var evBox in evBoxes)
                            {
                                if (evBox.MyEvent.TypeName == null || !evBox.MyEvent.TypeName.StartsWith("PlaySound_ByStateInfo"))
                                    continue;

                                int stateInfo = Convert.ToInt32(evBox.MyEvent.Parameters["StateInfo"]);
                                if (evBox.PlaybackHighlight)
                                {
                                    if (!activeStateInfos.Contains(stateInfo))
                                        activeStateInfos.Add(stateInfo);
                                }


                                if (!allStateInfos.Contains(stateInfo))
                                    allStateInfos.Add(stateInfo);
                            }

                            foreach (var si in allStateInfos)
                            {
                                if (!activeStateInfos.Contains(si))
                                    FmodManager.StopSE(si, false);
                            }
                            
                        },
                    }
                },
                {
                    "EventSimWeaponStyle",
                    new EventSimEntry("Simulate Weapon Style Changes", true, "SetWeaponStyle")
                    {
                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
                        {
                            if (MODEL.ChrAsm == null)
                                return;

                            MODEL.ChrAsm.WeaponStyle = MODEL.ChrAsm.StartWeaponStyle;
                        },
                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (MODEL.ChrAsm == null)
                                return;

                            if (evBox.MyEvent.Type != 32)
                                return;

                            // Not checking if the box is active because the effects
                            // "accumulate" along the timeline.

                            // Only simulate until the current time.
                            if (evBox.MyEvent.StartTime > time)
                                return;

                            MODEL.ChrAsm.WeaponStyle = (NewChrAsm.WeaponStyleType)Convert.ToInt32(evBox.MyEvent.Parameters["WeaponStyle"]);
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            if (MODEL.ChrAsm == null)
                                return;

                            if (Graph.ViewportInteractor.CurrentComboIndex < 0)
                                MODEL.ChrAsm.WeaponStyle = MODEL.ChrAsm.StartWeaponStyle;
                        },
                    }
                },
                {
                    "EventSimTracking",
                    new EventSimEntry("Simulate Character Tracking (A/D Keys)", true)
                    {
                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
                        {
                            MODEL.TrackingTestInput = 0;
                            MODEL.CurrentTrackingSpeed = MODEL.BaseTrackingSpeed;
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            float activeTrackingSpeedThisFrame = MODEL.BaseTrackingSpeed;
                            foreach (var evBox in evBoxes)
                            {
                                if (evBox.PlaybackHighlight && evBox.MyEvent.Template != null)
                                {
                                    if (evBox.MyEvent.Type == 0 && Convert.ToInt32(evBox.MyEvent.Parameters["JumpTableID"]) == 7)
                                    {
                                        // No matter what, if any disable rotation event is active the character can NOT rotate.
                                        activeTrackingSpeedThisFrame = 0;
                                        break;
                                    }
                                    else if (evBox.MyEvent.Type == 224)
                                    {
                                        activeTrackingSpeedThisFrame = Convert.ToSingle(evBox.MyEvent.Parameters["TurnSpeed"]);
                                    }
                                }
                            }
                            MODEL.CurrentTrackingSpeed = activeTrackingSpeedThisFrame;

                            if (!Graph.MainScreen.Input.AnyModifiersHeld)
                            {
                                float trackDir = 0;
                                if (Graph.MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.D))
                                    trackDir += 1;
                                if (Graph.MainScreen.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.A))
                                    trackDir -= 1;

                                MODEL.TrackingTestInput = Model.GlobalTrackingInput + trackDir;
                            }
                        },
                    }
                },
                ///////////////////////////////////////
                // NOT READY YET - EXTREMELY UNSTABLE
                ///////////////////////////////////////
                //{
                //    "EventSimWeaponTrails",
                //    new EventSimEntry("Simulate Weapon Trails", true, "SpawnFFX_Blade")
                //    {
                //        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                //        {
                //            if (evBox.PlaybackHighlight)
                //            {
                //                ActivateBladeFfx(evBox);
                //            }
                //        },
                //    }
                //},
                {
                    "EventSimBasicBlending",
                    new EventSimEntry("Simulate Animation Blending", true)
                    {
                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
                        {
                            if (Graph.ViewportInteractor.CurrentComboIndex >= 0)
                                return;

                            while (MODEL.AnimContainer.AnimationLayers.Count > 1)
                            {
                                MODEL.AnimContainer.AnimationLayers.RemoveAt(0);
                            }
                            if (MODEL.AnimContainer.AnimationLayers.Count > 0)
                                MODEL.AnimContainer.AnimationLayers[0].Weight = 1;
                        },
                        SimulationFrameChangePreBoxesAction =  (entry, evBoxes, time) =>
                        {
                            if (Graph.ViewportInteractor.CurrentComboIndex >= 0)
                                return;

                            if (MODEL.AnimContainer.AnimationLayers.Count > 1)
                            {
                                while (MODEL.AnimContainer.AnimationLayers.Count > 2)
                                {
                                    MODEL.AnimContainer.AnimationLayers.RemoveAt(0);
                                }

                                var blend = evBoxes.FirstOrDefault(b => b.MyEvent.Type == 16);
                                if (blend != null)
                                {
                                    float blendRatio = MathHelper.Clamp((((float)Graph.PlaybackCursor.GUICurrentTime - blend.MyEvent.StartTime) / (blend.MyEvent.EndTime - blend.MyEvent.StartTime)), 0, 1);
                                    MODEL.AnimContainer.AnimationLayers[0].Weight = 1 - blendRatio;
                                    MODEL.AnimContainer.AnimationLayers[1].Weight = blendRatio;
                                }
                                else
                                {
                                    while (MODEL.AnimContainer.AnimationLayers.Count > 1)
                                        MODEL.AnimContainer.AnimationLayers.RemoveAt(0);
                                }
                            }
                            else if (MODEL.AnimContainer.AnimationLayers.Count == 1)
                            {
                                MODEL.AnimContainer.AnimationLayers[0].Weight = 1;
                            }

                        },
                    }
                },

                {
                    "EventSimDS3PairedWeaponOverrideMemeEvents",
                    new EventSimEntry("Simulate DS3 Weapon Model Location Override (Event Type 712)", true)
                    {
                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
                        {
                            MODEL.ChrAsm?.ClearDS3PairedWeaponMeme();
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            bool isAnyOverrideMemeHappeningThisFrame = false;

                            foreach (var evBox in evBoxes)
                            {
                                if (evBox.MyEvent.Type == 712 && evBox.MyEvent.Template != null && 
                                    GameDataManager.GameType == GameDataManager.GameTypes.DS3 && MODEL.ChrAsm != null &&
                                    evBox.PlaybackHighlight)
                                {
                                    if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHand)
                                    {
                                        if (Convert.ToByte(evBox.MyEvent.Parameters["IsEnabledWhile1Handing"]) != 1)
                                            continue;
                                    }
                                    else if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandR)
                                    {
                                        if (Convert.ToByte(evBox.MyEvent.Parameters["IsEnabledWhile2HandingRightWeapon"]) != 1)
                                            continue;
                                    }
                                    else if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandL)
                                    {
                                        if (Convert.ToByte(evBox.MyEvent.Parameters["IsEnabledWhile2HandingLeftWeapon"]) != 1)
                                            continue;
                                    }

                                    isAnyOverrideMemeHappeningThisFrame = true;

                                    MODEL.ChrAsm.DS3PairedWeaponMemeR0_Flag = Convert.ToByte(evBox.MyEvent.Parameters["RHModel0AbsorpPosParamCondition"]);
                                    MODEL.ChrAsm.DS3PairedWeaponMemeR1_Flag = Convert.ToByte(evBox.MyEvent.Parameters["RHModel1AbsorpPosParamCondition"]);
                                    MODEL.ChrAsm.DS3PairedWeaponMemeR2_Flag = Convert.ToByte(evBox.MyEvent.Parameters["RHModel2AbsorpPosParamCondition"]);
                                    MODEL.ChrAsm.DS3PairedWeaponMemeR3_Flag = Convert.ToByte(evBox.MyEvent.Parameters["RHModel3AbsorpPosParamCondition"]);
                                    MODEL.ChrAsm.DS3PairedWeaponMemeL0_Flag = Convert.ToByte(evBox.MyEvent.Parameters["LHModel0AbsorpPosParamCondition"]);
                                    MODEL.ChrAsm.DS3PairedWeaponMemeL1_Flag = Convert.ToByte(evBox.MyEvent.Parameters["LHModel1AbsorpPosParamCondition"]);
                                    MODEL.ChrAsm.DS3PairedWeaponMemeL2_Flag = Convert.ToByte(evBox.MyEvent.Parameters["LHModel2AbsorpPosParamCondition"]);
                                    MODEL.ChrAsm.DS3PairedWeaponMemeL3_Flag = Convert.ToByte(evBox.MyEvent.Parameters["LHModel3AbsorpPosParamCondition"]);

                                    var changeTypeR0 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["RHModel0ChangeType"]);
                                    var changeTypeR1 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["RHModel1ChangeType"]);
                                    var changeTypeR2 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["RHModel2ChangeType"]);
                                    var changeTypeR3 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["RHModel3ChangeType"]);
                                    var changeTypeL0 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["LHModel0ChangeType"]);
                                    var changeTypeL1 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["LHModel1ChangeType"]);
                                    var changeTypeL2 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["LHModel2ChangeType"]);
                                    var changeTypeL3 = (NewChrAsm.DS3PairedWpnMemeKind)Convert.ToSByte(evBox.MyEvent.Parameters["LHModel3ChangeType"]);


                                    if (changeTypeR0 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeR0 = changeTypeR0;

                                    if (changeTypeR1 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeR1 = changeTypeR1;

                                    if (changeTypeR2 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeR2 = changeTypeR2;

                                    if (changeTypeR3 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeR3 = changeTypeR3;

                                    if (changeTypeL0 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeL0 = changeTypeL0;

                                    if (changeTypeL1 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeL1 = changeTypeL1;

                                    if (changeTypeL2 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeL2 = changeTypeL2;

                                    if (changeTypeL3 != NewChrAsm.DS3PairedWpnMemeKind.MaintainPreviousValue)
                                        MODEL.ChrAsm.DS3PairedWeaponMemeL3 = changeTypeL3;
                                }
                            }

                            if (!isAnyOverrideMemeHappeningThisFrame)
                            {
                                MODEL.ChrAsm?.ClearDS3PairedWeaponMeme();
                            }
                        },
                    }
                },


                {
                    "EventSimBasicBlending_Combos",
                    new EventSimEntry("Simulate Animation Blending (Combo Viewer)", true)
                    {
                        SimulationFrameChangeDisabledAction = (entry, evBoxes, time) =>
                        {
                            if (Graph.ViewportInteractor.CurrentComboIndex < 0)
                                return;

                            while (MODEL.AnimContainer.AnimationLayers.Count > 1)
                            {
                                MODEL.AnimContainer.AnimationLayers.RemoveAt(0);
                            }
                            if (MODEL.AnimContainer.AnimationLayers.Count > 0)
                                MODEL.AnimContainer.AnimationLayers[0].Weight = 1;
                        },
                        SimulationFrameChangePreBoxesAction =  (entry, evBoxes, time) =>
                        {
                            if (Graph.ViewportInteractor.CurrentComboIndex < 0)
                                return;

                            if (MODEL.AnimContainer.AnimationLayers.Count > 1)
                            {
                                while (MODEL.AnimContainer.AnimationLayers.Count > 2)
                                {
                                    MODEL.AnimContainer.AnimationLayers.RemoveAt(0);
                                }

                                var blend = evBoxes.FirstOrDefault(b => b.MyEvent.Type == 16);
                                if (blend != null)
                                {
                                    float blendRatio = MathHelper.Clamp((((float)Graph.PlaybackCursor.CurrentTime - blend.MyEvent.StartTime) / (blend.MyEvent.EndTime - blend.MyEvent.StartTime)), 0, 1);
                                    MODEL.AnimContainer.AnimationLayers[0].Weight = 1 - blendRatio;
                                    MODEL.AnimContainer.AnimationLayers[1].Weight = blendRatio;
                                }
                                else
                                {
                                    while (MODEL.AnimContainer.AnimationLayers.Count > 1)
                                        MODEL.AnimContainer.AnimationLayers.RemoveAt(0);
                                }
                            }
                            else if (MODEL.AnimContainer.AnimationLayers.Count == 1)
                            {
                                MODEL.AnimContainer.AnimationLayers[0].Weight = 1;
                            }

                        },
                    }
                },

                {
                    "EventSimAttackBehaviors",
                    new EventSimEntry("Simulate Hitbox Events", isEnabledByDefault: true,
                        "InvokeAttackBehavior", "InvokePCBehavior", "InvokeCommonBehavior", "InvokeThrowDamageBehavior")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            MODEL.DummyPolyMan.HideAllHitboxes();
                            MODEL.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.HideAllHitboxes());

                            //if (MODEL.ChrAsm?.RightWeaponModel != null)
                            //    MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.HideAllHitboxes();

                            //if (MODEL.ChrAsm?.LeftWeaponModel != null)
                            //    MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.HideAllHitboxes();
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.HideAllHitboxes();
                            MODEL.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.HideAllHitboxes());

                            //if (MODEL.ChrAsm?.RightWeaponModel != null)
                            //    MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.HideAllHitboxes();

                            //if (MODEL.ChrAsm?.LeftWeaponModel != null)
                            //    MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.HideAllHitboxes();
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlight)
                                return;

                            var dmyPolySrcToUse = HitViewDummyPolySource;

                            var atkParam = GetAtkParamFromEventBox(evBox, dmyPolySrcToUse);

                            //int dummyPolyOverride = -1;

                            if (evBox.MyEvent.Template != null && evBox.MyEvent.TypeName == "InvokePCBehavior")
                            {
                                int pcBehaviorType = Convert.ToInt32(evBox.MyEvent.Parameters["PCBehaviorType"]);
                                if (pcBehaviorType == 1 || pcBehaviorType == 16)
                                    dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                                else if (pcBehaviorType == 2 || pcBehaviorType == 128)
                                    dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                                else
                                    dmyPolySrcToUse = ParamData.AtkParam.DummyPolySource.Body;
                            }

                            //MODEL.DummyPolyMan.defaultDummyPolySource = dmyPolySrcToUse;

                            if (atkParam != null)
                            {
                                MODEL.DummyPolyMan.SetAttackVisibility(atkParam, true, MODEL.ChrAsm, -1 /*dummyPolyOverride*/, dmyPolySrcToUse);
                            }
                        },
                    }
                },

                { 
                    "EventSimSpEffects", 
                    new EventSimEntry("Simulate SpEffects", true,
                        "AddSpEffect",
                        "AddSpEffect_Multiplayer",
                        "AddSpEffect_Multiplayer_401",
                        "AddSpEffect_DragonForm",
                        "AddSpEffect_WeaponArts",
                        "AddSpEffect_CultRitualCompletion")
                    {

                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            SimulatedActiveSpEffects.Clear();

                            Graph.PlaybackCursor.ModPlaybackSpeed = 1;
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlightMidst)
                                return;

                            if (evBox.MyEvent.Parameters.Template.ContainsKey("SpEffectID"))
                            {
                                int spEffectID = Convert.ToInt32(evBox.MyEvent.Parameters["SpEffectID"]);

                                if (ParamManager.SpEffectParam.ContainsKey(spEffectID))
                                {
                                    var spEffect = ParamManager.SpEffectParam[spEffectID];

                                    if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 || GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
                                    {
                                        if (spEffect.GrabityRate > 0)
                                            Graph.PlaybackCursor.ModPlaybackSpeed *= spEffect.GrabityRate;
                                    }

                                    SimulatedActiveSpEffects.Add($"{spEffect.GetDisplayName()}");
                                }
                                else
                                {
                                    SimulatedActiveSpEffects.Add($"[Doesn't Exist] {spEffectID}");
                                }


                            }
                        },
                    }
                },

                {
                    "EventSimDS3DebugAnimSpeed",
                    new EventSimEntry("Simulate DS3 DebugAnimSpeed", isEnabledByDefault: false, "DebugAnimSpeed")
                    {
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            Graph.PlaybackCursor.ModPlaybackSpeed = 1;
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3 && evBox.PlaybackHighlight)
                            {
                                var eventFrames = Convert.ToUInt32(evBox.MyEvent.Parameters["AnimSpeed"]);
                                Graph.PlaybackCursor.ModPlaybackSpeed = (((evBox.MyEvent.EndTime - evBox.MyEvent.StartTime) * 30.0f) / (float)eventFrames);
                            }
                        },
                    }
                },

                {
                    "EventSimBulletBehaviors",
                    new EventSimEntry("Simulate Bullet Spawns", isEnabledByDefault: true,
                        "InvokeBulletBehavior")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            MODEL.DummyPolyMan.ClearAllBulletSpawns();
                            MODEL.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllBulletSpawns());
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllBulletSpawns();
                            MODEL.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllBulletSpawns());
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlight)
                                return;

                            if (evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolyID"))
                            {

                                var bulletParamID = GetBulletParamIDFromEvBox(evBox);
                                int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);

                                if (bulletParamID >= 0)
                                {
                                    var asmSrc = MODEL.ChrAsm?.GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource.Body, dummyPolyID);
                                    if (asmSrc == null || asmSrc == MODEL.DummyPolyMan)
                                    {
                                        MODEL.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID);
                                    }
                                    else
                                    {
                                        asmSrc.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID % 1000);
                                    }
                                }

                            }
                        },
                    }
                },

                {
                    "EventSimSFXSpawns",
                    new EventSimEntry("Simulate SFX Spawns", isEnabledByDefault: true,
                        "N/A")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            MODEL.DummyPolyMan.ClearAllSFXSpawns();
                            MODEL.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllSFXSpawns());
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllSFXSpawns();
                            MODEL.ChrAsm?.ForeachWeaponModel(m => m.DummyPolyMan.ClearAllSFXSpawns());
                        },
                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlight)
                                return;

                            if (evBox.MyEvent.Template == null)
                                return;

                            if (evBox.MyEvent.Parameters.Template.ContainsKey("FFXID") &&
                                evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolyID"))
                            {
                                // Using convert here since they might be various numeric
                                // value types.
                                int ffxid = Convert.ToInt32(evBox.MyEvent.Parameters["FFXID"]);
                                int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);
                                

                                if (evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolySource"))
                                {
                                    ParamData.AtkParam.DummyPolySource dummyPolySrc = ParamData.AtkParam.DummyPolySource.Body;

                                    int dplsVal = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolySource"]);
                                    if (dplsVal == 0)
                                    {
                                        dummyPolySrc = ParamData.AtkParam.DummyPolySource.Body;
                                    }
                                    else if (dplsVal == 1)
                                    {
                                        if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHandTransformedL ||
                                        MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandL)
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
                                        if (MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHandTransformedR ||
                                        MODEL.ChrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandR)
                                        {
                                             dummyPolySrc = ParamData.AtkParam.DummyPolySource.RightWeapon2;
                                        }
                                        else
                                        {
                                             dummyPolySrc = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                                        }

                                    }

                                    var asmSrc = MODEL.ChrAsm?.GetDummyManager(dummyPolySrc);

                                    if (asmSrc != null)
                                    {
                                        asmSrc.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                                    }
                                    else
                                    {
                                        MODEL.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                                    }
                                }

                                else
                                {
                                    var asmSrc = MODEL.ChrAsm?.GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource.Body, dummyPolyID);
                                    if (asmSrc == null || asmSrc == MODEL.DummyPolyMan)
                                    {
                                        MODEL.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                                    }
                                    else
                                    {
                                        asmSrc.SpawnSFXOnDummyPoly(ffxid, dummyPolyID % 1000);
                                    }
                                }

                                
                            }
                        },
                    }
                },

                //{
                //    "EventSimSoundDummyPolySpawns", new EventSimEntry("Display")
                //},

                //{
                //    "EventSimMiscDummyPolySpawns",
                //    new EventSimEntry("Simulate Misc. DummyPoly Spawn Events", isEnabledByDefault: true)
                //    {
                //        NewAnimSelectedAction = (entry, evBoxes) =>
                //        {
                //            MODEL.DummyPolyMan.ClearAllMiscSpawns();

                //            if (MODEL.ChrAsm?.RightWeaponModel != null)
                //                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.ClearAllMiscSpawns();

                //            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                //                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.ClearAllMiscSpawns();
                //        },
                //        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                //        {
                //            MODEL.DummyPolyMan.ClearAllMiscSpawns();

                //            if (MODEL.ChrAsm?.RightWeaponModel != null)
                //                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.ClearAllMiscSpawns();

                //            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                //                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.ClearAllMiscSpawns();
                //        },
                //        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                //        {
                //            if (!evBox.PlaybackHighlight)
                //                return;

                //            if (evBox.MyEvent.TypeName == "InvokeBulletBehavior" || evBox.MyEvent.TypeName.Contains("PlaySound"))
                //                return;

                //            if (!evBox.MyEvent.Parameters.Template.ContainsKey("FFXID") &&
                //                evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolyID"))
                //            {
                //                int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);

                //                if (evBox.MyEvent.TypeName == "SpawnFFX_ChrType")
                //                {
                //                    // This is way too long to show TypeName(Args) so just do TypeName
                //                    GetCurrentDummyPolyMan()?.SpawnMiscOnDummyPoly(evBox.MyEvent.TypeName, dummyPolyID);
                //                }
                //                else
                //                {
                //                    GetCurrentDummyPolyMan()?.SpawnMiscOnDummyPoly(evBox.EventText.Text, dummyPolyID);
                //                }
                //            }
                //        },
                //    }
                //},

                {
                    "EventSimOpacity",
                    new EventSimEntry("Simulate Opacity Change Events", isEnabledByDefault: true,
                        "SetOpacityKeyframe")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            GFX.FlverOpacity = 1.0f;
                        },
                        //SimulationStartAction = (entry, evBoxes) =>
                        //{
                        //    GFX.FlverOpacity = 1.0f;
                        //},
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            GFX.FlverOpacity = 1;
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlight)
                                return;

                            float fadeDuration = evBox.MyEvent.EndTime - evBox.MyEvent.StartTime;
                            float timeSinceFadeStart = time - evBox.MyEvent.StartTime;
                            float fadeStartOpacity = (float)evBox.MyEvent.Parameters["OpacityAtEventStart"];
                            float fadeEndOpacity = (float)evBox.MyEvent.Parameters["OpacityAtEventEnd"];

                            GFX.FlverOpacity = MathHelper.Lerp(fadeStartOpacity, fadeEndOpacity, timeSinceFadeStart / fadeDuration);
                        },
                        //DuringAction = (entry, evBox) =>
                        //{
                            
                        //},
                        //SimulationEndAction = (entry, evBoxes) =>
                        //{
                        //    GFX.FlverOpacity = 1.0f;
                        //},
                        
                    }
                },


                {
                    "EventSimDrawMasks",
                    new EventSimEntry("Simulate Draw Mask Changes", isEnabledByDefault: true,
                        "ShowModelMask", "HideModelMask", "ChangeChrDrawMask", "HideEquippedWeapon")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            //entry.Vars.ClearAllData();

                            //MODEL.DefaultAllMaskValues();
                        },
                        SimulationStartAction = (entry, evBoxes) =>
                        {
                            //MODEL.DefaultAllMaskValues();
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.ResetDrawMaskToDefault();

                            if (MODEL.ChrAsm != null)
                            {
                                MODEL.ChrAsm.SetRightWeaponVisible(true);
                                MODEL.ChrAsm.SetLeftWeaponVisible(true);
                            }
                        },
                        SimulationFrameChangePerBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            // Not checking if the box is active because the effects
                            // "accumulate" along the timeline.

                            // Only simulate until the current time.
                            if (evBox.MyEvent.StartTime > time)
                                return;

                            int maskLength = 32;

                            if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 || 
                                GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
                                maskLength = 8;

                            if (evBox.MyEvent.Template == null)
                                return;

                            if (evBox.MyEvent.TypeName == "ChangeChrDrawMask")
                            {
                                for (int i = 0; i < maskLength; i++)
                                {
                                    var maskByte = (byte)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"];
                                    
                                    // Before you get out the torch, be aware that the game 
                                    // uses some value other than 0 to SKIP
                                    if (maskByte == 0)
                                        MODEL.DrawMask[i] = false;
                                    else if (maskByte == 1)
                                        MODEL.DrawMask[i] = true;
                                }
                            }
                            else if (MODEL.ChrAsm != null && evBox.MyEvent.TypeName == "HideEquippedWeapon")
                            {
                                if (evBox.PlaybackHighlight)
                                {
                                    if ((bool)evBox.MyEvent.Parameters["RightHand"])
                                    {
                                        MODEL.ChrAsm.SetRightWeaponVisible(false);
                                    }

                                    if ((bool)evBox.MyEvent.Parameters["LeftHand"])
                                    {
                                        MODEL.ChrAsm.SetLeftWeaponVisible(false);
                                    }
                                }

                            }
                            else if (evBox.PlaybackHighlight)
                            {
                                if (evBox.MyEvent.TypeName == "ShowModelMask")
                                {
                                    for (int i = 0; i < maskLength; i++)
                                    {
                                        if ((bool)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"])
                                                MODEL.DrawMask[i] = true;
                                    }
                                }
                                else if (evBox.MyEvent.TypeName == "HideModelMask")
                                {
                                    for (int i = 0; i < maskLength; i++)
                                    {
                                        if ((bool)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"])
                                                MODEL.DrawMask[i] = false;
                                    }
                                }
                            }
                        },
                        EnterAction = (entry, evBox) =>
                        {
                            //if (evBox.MyEvent.TypeName == "ShowModelMask" || evBox.MyEvent.TypeName == "HideModelMask")
                            //{
                            //    //if (entry.Vars["OriginalMask"] == null)
                            //    //entry.Vars["OriginalMask"] = new EventSimEntryVariableContainer();

                            //    //var maskVar = (EventSimEntryVariableContainer)(entry.Vars["OriginalMask"]);

                            //    //var newMaskCopy = maskVar[evBox] != null ? ((bool[])maskVar[evBox]) : new bool[MODEL.DrawMask.Length];

                            //    //Array.Copy(MODEL.DrawMask, newMaskCopy, MODEL.DrawMask.Length);
                            //    //maskVar[evBox] = newMaskCopy;
                            //}

                            //for (int i = 0; i < 32; i++)
                            //{
                            //    if (evBox.MyEvent.Parameters.Template.ContainsKey($"Mask{(i + 1)}"))
                            //    {
                            //        if (evBox.MyEvent.TypeName == "ChangeChrDrawMask")
                            //        {
                            //            var maskByte = (byte)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"];

                            //            // Before you get out the torch, be aware that the game 
                            //            // uses some value other than 0 to SKIP
                            //            if (maskByte == 0)
                            //                MODEL.DrawMask[i] = false;
                            //            else if (maskByte == 1)
                            //                MODEL.DrawMask[i] = true;
                            //        }
                            //    }
                            //}
                        },
                        DuringAction = (entry, evBox) =>
                        {
                            //for (int i = 0; i < 32; i++)
                            //{
                            //    if (evBox.MyEvent.Parameters.Template.ContainsKey($"Mask{(i + 1)}"))
                            //    {
                            //        if (evBox.MyEvent.TypeName == "ShowModelMask")
                            //        {
                            //            if ((bool)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"])
                            //                MODEL.DrawMask[i] = true;
                            //        }
                            //        else if (evBox.MyEvent.TypeName == "HideModelMask")
                            //        {
                            //            if ((bool)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"])
                            //                MODEL.DrawMask[i] = false;
                            //        }
                            //    }
                            //}
                        },
                        ExitAction = (entry, evBox) =>
                        {
                             //if (evBox.MyEvent.TypeName == "ShowModelMask" || evBox.MyEvent.TypeName == "HideModelMask")
                             //{
                             //    if (entry.Vars["OriginalMask"] != null)
                             //    {
                             //        var maskVar = (EventSimEntryVariableContainer)(entry.Vars["OriginalMask"]);
                             //        if (maskVar != null)
                             //        {
                             //            MODEL.DrawMask = (bool[])(maskVar[evBox]);
                             //        }

                             //    }
                             //}
                        },
                    }
                },

                // DEFAULT EVENT TEMPLATE FOR PASTING
                /*
                {
                    new[] { "EVENT_TYPE_NAME", },
                    new EventSimEntry("MENU_BAR_NAME")
                    {
                        NewAnimSelectedAction = (entry) =>
                        {

                        },
                        SimulationStartAction = (entry) =>
                        {

                        },
                        SimulationScrubAction = (entry) =>
                        {

                        },
                        EnterAction = (entry, evBox) =>
                        {

                        },
                        DuringAction = (entry, evBox) =>
                        {

                        },
                        ExitAction = (entry, evBox) =>
                        {

                        },
                        SimulationEndAction = (entry) =>
                        {

                        },
                    }
                },
                */

            };
        }

        public void BuildEventSimMenuBar(TaeMenuBarBuilder menu)
        {
            if (menu["Simulation"] != null)
            {
                menu.ClearItem("Simulation");
                menu["Simulation"].Visible = true;
                menu["Simulation"].Enabled = true;
            }

            foreach (var kvp in Entries)
            {
                if (kvp.Value.MenuOptionName != null)
                    menu.AddItem("Simulation", kvp.Value.MenuOptionName, 
                        () => GetSimEnabled(kvp.Key), b =>
                        {
                            OnNewAnimSelected(Graph.EventBoxes);
                            SetSimEnabled(kvp.Key, b);
                            OnNewAnimSelected(Graph.EventBoxes);
                            Graph.ViewportInteractor.OnScrubFrameChange();
                        });
            }
        }

        public bool GetSimEnabled(string simName)
        {
            if (simName == null)
                return false;

            if (!Main.TAE_EDITOR.Config.EventSimulationsEnabled.ContainsKey(simName))
                Main.TAE_EDITOR.Config.EventSimulationsEnabled.Add(simName, Entries[simName].IsEnabledByDefault);

            return Main.TAE_EDITOR.Config.EventSimulationsEnabled[simName];
        }

        private void SetSimEnabled(string simName, bool enabled)
        {
            if (!Main.TAE_EDITOR.Config.EventSimulationsEnabled.ContainsKey(simName))
                Main.TAE_EDITOR.Config.EventSimulationsEnabled.Add(simName, enabled);
            else
                Main.TAE_EDITOR.Config.EventSimulationsEnabled[simName] = enabled;
        }

        public void OnNewAnimSelected(List<TaeEditAnimEventBox> evBoxes)
        {
            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.NewAnimSelectedAction?.Invoke(kvp.Value, evBoxes);
            }
        }

        private string GetSimEntryForEventBox(TaeEditAnimEventBox evBox)
        {
            return entries.FirstOrDefault(kvp => kvp.Value.EventTypes.Contains(evBox.MyEvent.TypeName)).Key;
        }

        public void OnSimulationStart(List<TaeEditAnimEventBox> evBoxes)
        {
            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.SimulationStartAction?.Invoke(kvp.Value, evBoxes);
            }
        }

        public void OnSimulationEnd(List<TaeEditAnimEventBox> evBoxes)
        {
            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.SimulationEndAction?.Invoke(kvp.Value, evBoxes);
            }
        }

        public void OnSimulationFrameChange(List<TaeEditAnimEventBox> evBoxes, float time)
        {
            var enabledEntries = entries.Keys.ToList();

            foreach (var kvp in entries)
            {
                if (GetSimEnabled(kvp.Key))
                {
                    kvp.Value.SimulationFrameChangePreBoxesAction?.Invoke(kvp.Value, evBoxes, time);
                }
                else
                {
                    kvp.Value.SimulationFrameChangeDisabledAction?.Invoke(kvp.Value, evBoxes, time);
                    enabledEntries.Remove(kvp.Key);
                    continue;
                }
            }

            var orderedBoxes = evBoxes.OrderBy(evb => evb.MyEvent.StartTime).ToList();

            foreach (var evBox in orderedBoxes)
            {
                foreach (var entryName in enabledEntries)
                {
                    entries[entryName].SimulationFrameChangePerBoxAction?.Invoke(entries[entryName], evBoxes, evBox, time);

                    if (evBox.PlaybackHighlight && entries[entryName].DoesEventMatch(evBox))
                    {
                        entries[entryName].SimulationFrameChangePerMatchingBoxAction?.Invoke(entries[entryName], evBoxes, evBox, time);
                    }
                }
            }
        }

        public void OnEventEnter(TaeEditAnimEventBox evBox)
        {
            var matchingSim = GetSimEntryForEventBox(evBox);

            if (!GetSimEnabled(matchingSim))
                return;

            if (entries[matchingSim] != null)
                entries[matchingSim].EnterAction?.Invoke(entries[matchingSim], evBox);
        }

        public void OnEventMidFrame(TaeEditAnimEventBox evBox)
        {
            var matchingSim = GetSimEntryForEventBox(evBox);

            if (!GetSimEnabled(matchingSim))
                return;

            if (entries[matchingSim] != null)
                entries[matchingSim].DuringAction?.Invoke(entries[matchingSim], evBox);
        }

        public void OnEventExit(TaeEditAnimEventBox evBox)
        {
            var matchingSim = GetSimEntryForEventBox(evBox);

            if (!GetSimEnabled(matchingSim))
                return;

            if (entries[matchingSim] != null)
                entries[matchingSim].ExitAction?.Invoke(entries[matchingSim], evBox);

        }

        public class EventSimEntryVariableContainer
        {
            private Dictionary<object, object> Data = new Dictionary<object, object>();
            public object this[object name]
            {
                get
                {
                    if (Data.ContainsKey(name))
                        return Data[name];
                    else
                        return null;
                }
                set
                {
                    if (Data.ContainsKey(name))
                        Data[name] = value;
                    else
                        Data.Add(name, value);
                }
            }

            public void ClearAllData()
            {
                Data.Clear();
            }
        }

        public class EventSimEntry
        {
            public List<string> EventTypes = new List<string>();

            public Action<EventSimEntry, List<TaeEditAnimEventBox>> NewAnimSelectedAction;
            public Action<EventSimEntry, List<TaeEditAnimEventBox>> SimulationStartAction;
            public Action<EventSimEntry, List<TaeEditAnimEventBox>> SimulationEndAction;

            public Action<EventSimEntry, List<TaeEditAnimEventBox>, float>
                SimulationFrameChangePreBoxesAction;

            public Action<EventSimEntry, List<TaeEditAnimEventBox>, float>
                SimulationFrameChangeDisabledAction;

            public Action<EventSimEntry, List<TaeEditAnimEventBox>, TaeEditAnimEventBox, float> 
                SimulationFrameChangePerBoxAction;

            public Action<EventSimEntry, List<TaeEditAnimEventBox>, TaeEditAnimEventBox, float> 
                SimulationFrameChangePerMatchingBoxAction;

            public Action<EventSimEntry, TaeEditAnimEventBox> EnterAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> DuringAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> ExitAction;

            public EventSimEntryVariableContainer Vars = new EventSimEntryVariableContainer();

            public readonly string MenuOptionName;
            public readonly bool IsEnabledByDefault;

            public bool DoesEventMatch(TaeEditAnimEventBox evBox)
            {
                if (evBox.MyEvent.TypeName == null)
                    return false;

                return EventTypes.Contains(evBox.MyEvent.TypeName);
            }

            public EventSimEntry(string menuOptionName, bool isEnabledByDefault)
            {
                MenuOptionName = menuOptionName;
                IsEnabledByDefault = isEnabledByDefault;
            }

            public EventSimEntry(string menuOptionName, bool isEnabledByDefault, params string[] eventTypes)
            {
                MenuOptionName = menuOptionName;
                IsEnabledByDefault = isEnabledByDefault;
                EventTypes = eventTypes.ToList();
            }
        }
    }
}
