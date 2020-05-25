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
            MODEL.IS_PLAYER ? (Graph?.MainScreen.Config.HitViewDummyPolySource ?? ParamData.AtkParam.DummyPolySource.Body) 
            : ParamData.AtkParam.DummyPolySource.Body;

        public TaeEventSimulationEnvironment(TaeEditAnimEventGraph graph, Model mdl)
        {
            MODEL = mdl;
            Graph = graph;

            InitAllEntries();
        }

        private Dictionary<string, EventSimEntry> entries;
        public IReadOnlyDictionary<string, EventSimEntry> Entries => entries;

        public List<string> SimulatedActiveSpEffects = new List<string>();

        private ParamData.BehaviorParam GetBehaviorParamFromEvBox(TaeEditAnimEventBox evBox)
        {
            if (evBox.MyEvent.TypeName == "InvokeAttackBehavior" || 
                evBox.MyEvent.TypeName == "InvokeThrowDamageBehavior" ||
                evBox.MyEvent.TypeName == "InvokeBulletBehavior" ||
                evBox.MyEvent.TypeName == "InvokeCommonBehavior" ||
                evBox.MyEvent.TypeName == "InvokePCBehavior" ||
                evBox.MyEvent.TypeName == "InvokeGunBehavior")
            {
                int behaviorSubID = (int)evBox.MyEvent.Parameters["BehaviorSubID"];
                if (MODEL.IS_PLAYER)
                {
                    var id = -1;
                    if (evBox.MyEvent.TypeName == "InvokeCommonBehavior")
                        id = behaviorSubID;
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon && MODEL.ChrAsm?.RightWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.RightWeapon.BehaviorVariationID * 1_000) + behaviorSubID;
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon && MODEL.ChrAsm?.LeftWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.LeftWeapon.BehaviorVariationID * 1_000) + behaviorSubID;
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                        id = behaviorSubID;

                    if (ParamManager.BehaviorParam_PC.ContainsKey(id))
                        return ParamManager.BehaviorParam_PC[id];

                    if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon && MODEL.ChrAsm?.RightWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.RightWeapon.FallbackBehaviorVariationID * 1_000) + behaviorSubID;
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon && MODEL.ChrAsm?.LeftWeapon != null)
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
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                    {
                        id = 2_00000_000 + (MODEL.NpcParam.BehaviorVariationID * 1_000) + behaviorSubID;
                    }

                    if (ParamManager.BehaviorParam.ContainsKey(id))
                        return ParamManager.BehaviorParam[id];
                }
            }

            return null;
        }

        private ParamData.AtkParam GetAtkParamFromEventBox(TaeEditAnimEventBox evBox)
        {
            var behaviorParam = GetBehaviorParamFromEvBox(evBox);

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
            var behaviorParam = GetBehaviorParamFromEvBox(evBox);

            if (behaviorParam == null)
                return -1;

            if (behaviorParam.RefType == ParamData.BehaviorParam.RefTypes.Bullet)
            {
                return behaviorParam.RefID;
            }

            return -1;
        }

        private NewDummyPolyManager GetCurrentDummyPolyMan()
        {
            if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
            {
                return MODEL.DummyPolyMan;
            }
            else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon &&
                MODEL.ChrAsm?.RightWeaponModel != null)
            {
                return MODEL.ChrAsm.RightWeaponModel.DummyPolyMan;
            }
            else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon &&
                MODEL.ChrAsm?.LeftWeaponModel != null)
            {
                return MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan;
            }

            return null;
        }

        public void PlaySoundEffectOfBox(TaeEditAnimEventBox evBox)
        {
            if (evBox.MyEvent.TypeName == null || !evBox.MyEvent.TypeName.StartsWith("PlaySound"))
                return;

            int soundType = Convert.ToInt32(evBox.MyEvent.Parameters["SoundType"]);
            int soundID = Convert.ToInt32(evBox.MyEvent.Parameters["SoundID"]);

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

            FmodManager.PlaySE(soundType, soundID, getPosFunc);
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
                            if (evBox.WasJustEnteredDuringPlayback)
                            {
                                PlaySoundEffectOfBox(evBox);
                            }
                        },
                    }
                },
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

                            if (MODEL.ChrAsm?.RightWeaponModel != null)
                                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.HideAllHitboxes();

                            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.HideAllHitboxes();
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.HideAllHitboxes();

                            if (MODEL.ChrAsm?.RightWeaponModel != null)
                                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.HideAllHitboxes();

                            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.HideAllHitboxes();
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlight)
                                return;

                            var atkParam = GetAtkParamFromEventBox(evBox);

                            if (atkParam != null)
                            {
                                var dmyPolySource = HitViewDummyPolySource;

                                if (atkParam.SuggestedDummyPolySource != ParamData.AtkParam.DummyPolySource.None)
                                {
                                    dmyPolySource = atkParam.SuggestedDummyPolySource;
                                }

                                if (dmyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                                {
                                    MODEL.DummyPolyMan.SetAttackVisibility(
                                        atkParam, true, ParamData.AtkParam.DummyPolySource.Body);
                                }
                                else if (dmyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon &&
                                    MODEL.ChrAsm?.RightWeaponModel != null)
                                {
                                    MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.SetAttackVisibility(
                                        atkParam, true, ParamData.AtkParam.DummyPolySource.Body);
                                }
                                else if (dmyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon &&
                                    MODEL.ChrAsm?.LeftWeaponModel != null)
                                {
                                    MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.SetAttackVisibility(
                                        atkParam, true, ParamData.AtkParam.DummyPolySource.Body);
                                }

                                if (MODEL.ChrAsm?.RightWeaponModel != null)
                                {
                                    MODEL.ChrAsm.RightWeaponModel.DummyPolyMan.SetAttackVisibility(
                                        atkParam, true, ParamData.AtkParam.DummyPolySource.RightWeapon);
                                }

                                    if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                {
                                    MODEL.ChrAsm.LeftWeaponModel.DummyPolyMan.SetAttackVisibility(
                                        atkParam, true, ParamData.AtkParam.DummyPolySource.LeftWeapon);
                                }
                            }
                        },
                    }
                },

                { 
                    "EventSimSpEffects", 
                    new EventSimEntry("Simulate SpEffects", true,
                        "InvokeSpEffectBehavior_Multiplayer",
                        "InvokeSpEffectBehavior", 
                        "InvokeSpEffect", 
                        "InvokeSpEffect_Multiplayer")
                    {

                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            SimulatedActiveSpEffects.Clear();

                            Graph.PlaybackCursor.ModPlaybackSpeed = 1;
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlight)
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
                    "EventSimBulletBehaviors",
                    new EventSimEntry("Simulate Bullet Spawns", isEnabledByDefault: true,
                        "InvokeBulletBehavior")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            MODEL.DummyPolyMan.ClearAllBulletSpawns();
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllBulletSpawns();
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
                                    MODEL.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID);
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
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllSFXSpawns();
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

                                MODEL.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                            }
                        },
                    }
                },

                //{
                //    "EventSimSoundDummyPolySpawns", new EventSimEntry("Display")
                //},

                {
                    "EventSimMiscDummyPolySpawns",
                    new EventSimEntry("Simulate Misc. DummyPoly Spawn Events", isEnabledByDefault: true)
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            MODEL.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.RightWeaponModel != null)
                                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.ClearAllMiscSpawns();
                        },
                        SimulationFrameChangePreBoxesAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.RightWeaponModel != null)
                                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.ClearAllMiscSpawns();
                        },
                        SimulationFrameChangePerMatchingBoxAction = (entry, evBoxes, evBox, time) =>
                        {
                            if (!evBox.PlaybackHighlight)
                                return;

                            if (evBox.MyEvent.TypeName == "InvokeBulletBehavior" || evBox.MyEvent.TypeName.Contains("PlaySound"))
                                return;

                            if (!evBox.MyEvent.Parameters.Template.ContainsKey("FFXID") &&
                                evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolyID"))
                            {
                                int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);

                                if (evBox.MyEvent.TypeName == "SpawnFFX_ChrType")
                                {
                                    // This is way too long to show TypeName(Args) so just do TypeName
                                    GetCurrentDummyPolyMan()?.SpawnMiscOnDummyPoly(evBox.MyEvent.TypeName, dummyPolyID);
                                }
                                else
                                {
                                    GetCurrentDummyPolyMan()?.SpawnMiscOnDummyPoly(evBox.EventText.Text, dummyPolyID);
                                }
                            }
                        },
                    }
                },

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
                            float fadeStartOpacity = (float)evBox.MyEvent.Parameters["GhostVal1"];
                            float fadeEndOpacity = (float)evBox.MyEvent.Parameters["GhostVal2"];

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
                                if (MODEL.ChrAsm.RightWeaponModel != null)
                                    MODEL.ChrAsm.RightWeaponModel.IsVisible = true;

                                if (MODEL.ChrAsm.LeftWeaponModel != null)
                                    MODEL.ChrAsm.LeftWeaponModel.IsVisible = true;
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

                            if (GameDataManager.GameType == GameDataManager.GameTypes.DS1)
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
                                        if (MODEL.ChrAsm.RightWeaponModel != null)
                                            MODEL.ChrAsm.RightWeaponModel.IsVisible = false;
                                    }

                                    if ((bool)evBox.MyEvent.Parameters["LeftHand"])
                                    {
                                        if (MODEL.ChrAsm.LeftWeaponModel != null)
                                            MODEL.ChrAsm.LeftWeaponModel.IsVisible = false;
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
