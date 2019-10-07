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
                    if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon && MODEL.ChrAsm?.RightWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.RightWeapon.BehaviorVariationID * 1_000) + behaviorSubID;
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon && MODEL.ChrAsm?.LeftWeapon != null)
                        id = 10_0000_000 + (MODEL.ChrAsm.LeftWeapon.BehaviorVariationID * 1_000) + behaviorSubID;
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                        id = behaviorSubID;

                    if (ParamManager.BehaviorParam_PC.ContainsKey(id))
                        return ParamManager.BehaviorParam_PC[id];

                    if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.RightWeapon && MODEL.ChrAsm?.RightWeapon != null)
                        id = 10_0000_000 + ((MODEL.ChrAsm.RightWeapon.WepMotionCategory * 100) * 1_000) + behaviorSubID;
                    else if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.LeftWeapon && MODEL.ChrAsm?.LeftWeapon != null)
                        id = 10_0000_000 + ((MODEL.ChrAsm.LeftWeapon.WepMotionCategory * 100) * 1_000) + behaviorSubID;

                    if (ParamManager.BehaviorParam_PC.ContainsKey(id))
                        return ParamManager.BehaviorParam_PC[id];
                }
                else
                {
                    if (MODEL.NpcParam == null)
                        return null;

                    if (HitViewDummyPolySource == ParamData.AtkParam.DummyPolySource.Body)
                    {
                        var id = 2_00000_000 + (MODEL.NpcParam.BehaviorVariationID * 1_000) + behaviorSubID;

                        if (ParamManager.BehaviorParam.ContainsKey(id))
                            return ParamManager.BehaviorParam[id];
                    }
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

        private void InitAllEntries()
        {
            entries = new Dictionary<string, EventSimEntry>
            {

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
                        SimulationFrameChangeAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.HideAllHitboxes();

                            if (MODEL.ChrAsm?.RightWeaponModel != null)
                                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.HideAllHitboxes();

                            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.HideAllHitboxes();

                            foreach (var evBox in evBoxes.Where(b => entry.DoesEventMatch(b)))
                            {
                                var atkParam = GetAtkParamFromEventBox(evBox);

                                if (atkParam != null)
                                {
                                    if (evBox.PlaybackHighlight)
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
                                }
                            }
                        }
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
                        SimulationFrameChangeAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllBulletSpawns();
                            foreach (var evBox in evBoxes
                                .Where(evb => evb.MyEvent.Template != null && 
                                     evb.PlaybackHighlight && entry.DoesEventMatch(evb))
                                .OrderBy(evb => evb.MyEvent.StartTime))
                            {
                                if (evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolyID"))
                                {
                                    var bulletParamID = GetBulletParamIDFromEvBox(evBox);
                                    int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);
                                    if (bulletParamID >= 0)
                                    {
                                        MODEL.DummyPolyMan.SpawnBulletOnDummyPoly(bulletParamID, dummyPolyID);
                                    }
                                }

                            }
                        }
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
                        SimulationFrameChangeAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllSFXSpawns();

                            foreach (var evBox in evBoxes
                                .Where(evb => evb.MyEvent.Template != null && evb.PlaybackHighlight)
                                .OrderBy(evb => evb.MyEvent.StartTime))
                            {

                                if (evBox.MyEvent.Parameters.Template.ContainsKey("FFXID") &&
                                    evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolyID"))
                                {
                                    // Using convert here since they might be various numeric
                                    // value types.
                                    int ffxid = Convert.ToInt32(evBox.MyEvent.Parameters["FFXID"]);
                                    int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);

                                    MODEL.DummyPolyMan.SpawnSFXOnDummyPoly(ffxid, dummyPolyID);
                                }
                            }
                        }
                    }
                },

                {
                    "EventSimMiscDummyPolySpawns",
                    new EventSimEntry("Simulate Misc. DummyPoly Spawn Events", isEnabledByDefault: true,
                        "N/A")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            MODEL.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.RightWeaponModel != null)
                                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.ClearAllMiscSpawns();
                        },
                        SimulationFrameChangeAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.RightWeaponModel != null)
                                MODEL.ChrAsm?.RightWeaponModel.DummyPolyMan.ClearAllMiscSpawns();

                            if (MODEL.ChrAsm?.LeftWeaponModel != null)
                                MODEL.ChrAsm?.LeftWeaponModel.DummyPolyMan.ClearAllMiscSpawns();

                            foreach (var evBox in evBoxes
                                .Where(evb => evb.MyEvent.Template != null && evb.PlaybackHighlight)
                                .OrderBy(evb => evb.MyEvent.StartTime))
                            {
                                if (evBox.MyEvent.TypeName == "InvokeBulletBehavior")
                                    continue;

                                if (!evBox.MyEvent.Parameters.Template.ContainsKey("FFXID") &&
                                    evBox.MyEvent.Parameters.Template.ContainsKey("DummyPolyID"))
                                {
                                    // Using convert here since they might be various numeric
                                    // value types.
                                    int dummyPolyID = Convert.ToInt32(evBox.MyEvent.Parameters["DummyPolyID"]);


                                    if (evBox.MyEvent.TypeName == "SpawnFFX_ChrType")
                                    {
                                        GetCurrentDummyPolyMan()?.SpawnMiscOnDummyPoly(evBox.MyEvent.TypeName, dummyPolyID);
                                    }
                                    else
                                    {
                                        GetCurrentDummyPolyMan()?.SpawnMiscOnDummyPoly(evBox.EventText.Text, dummyPolyID);
                                    }
                                }
                            }
                        }
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
                        SimulationFrameChangeAction = (entry, evBoxes, time) =>
                        {
                            bool anyOpacityHappeningThisFrame = false;

                            foreach (var evBox in evBoxes.Where(b => b.MyEvent.Template != null && 
                                b.PlaybackHighlight && entry.DoesEventMatch(b)))
                            {
                                float fadeDuration = evBox.MyEvent.EndTime - evBox.MyEvent.StartTime;
                                float timeSinceFadeStart = (float)Main.TAE_EDITOR.PlaybackCursor.CurrentTime - evBox.MyEvent.StartTime;
                                float fadeStartOpacity = (float)evBox.MyEvent.Parameters["GhostVal1"];
                                float fadeEndOpacity = (float)evBox.MyEvent.Parameters["GhostVal2"];

                                GFX.FlverOpacity = MathHelper.Lerp(fadeStartOpacity, fadeEndOpacity, timeSinceFadeStart / fadeDuration);
                                anyOpacityHappeningThisFrame = true;
                            }
                            if (!anyOpacityHappeningThisFrame)
                                GFX.FlverOpacity = 1.0f;
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
                        SimulationFrameChangeAction = (entry, evBoxes, time) =>
                        {
                            // Start from beginning of simulation and simulate to current time.

                            int maskLength = 32;

                            if (GameDataManager.GameType == GameDataManager.GameTypes.DS1)
                                maskLength = 8;

                            MODEL.ResetDrawMaskToDefault();

                            if (MODEL.ChrAsm != null)
                            {
                                if (MODEL.ChrAsm.RightWeaponModel != null)
                                    MODEL.ChrAsm.RightWeaponModel.IsVisible = true;

                                if (MODEL.ChrAsm.LeftWeaponModel != null)
                                    MODEL.ChrAsm.LeftWeaponModel.IsVisible = true;
                            }
                            

                            foreach (var evBox in evBoxes.Where(evb => evb.MyEvent.Template != null)
                                .OrderBy(evb => evb.MyEvent.StartTime))
                            {
                                if (evBox.MyEvent.StartTime > time)
                                    break;

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

        private bool GetSimEnabled(string simName)
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
            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.SimulationFrameChangeAction?.Invoke(kvp.Value, evBoxes, time);
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
            public Action<EventSimEntry, List<TaeEditAnimEventBox>, float> SimulationFrameChangeAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> EnterAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> DuringAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> ExitAction;
            public EventSimEntryVariableContainer Vars = new EventSimEntryVariableContainer();

            public readonly string MenuOptionName;
            public readonly bool IsEnabledByDefault;

            public bool DoesEventMatch(TaeEditAnimEventBox evBox)
            {
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
