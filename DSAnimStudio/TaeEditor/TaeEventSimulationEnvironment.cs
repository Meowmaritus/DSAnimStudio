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

        public TaeEventSimulationEnvironment(Model mdl)
        {
            MODEL = mdl;

            InitAllEntries();
        }

        private Dictionary<string, EventSimEntry> entries;
        public IReadOnlyDictionary<string, EventSimEntry> Entries => entries;

        private ParamData.AtkParam GetAtkParamFromEventBox(TaeEditAnimEventBox evBox)
        {
            ParamData.AtkParam atkParam = null;
            if (evBox.MyEvent.TypeName == "InvokeAttackBehavior" || evBox.MyEvent.TypeName == "InvokeThrowDamageBehavior")
            {


                if (!MODEL.IS_PLAYER)
                {
                     atkParam = ParamManager.GetNpcBasicAtkParam(MODEL.NpcParam, (int)evBox.MyEvent.Parameters["BehaviorSubID"]);
                }
                else
                {
                    bool isLeftHand = MODEL.DummyPolyMan.IsViewingLeftHandHit;

                    if (evBox.MyEvent.TypeName == "InvokeAttackBehavior")
                    {
                        var atkType = (int)evBox.MyEvent.Parameters["AttackType"];

                        if (atkType == 64/*Parry*/)
                        {
                            isLeftHand = true;
                        }
                        else if (atkType == 2 /*Forward+R1*/ || atkType == 62 /*Plunging attack*/)
                        {
                            isLeftHand = false;
                        }
                    }
                    

                    atkParam = ParamManager.GetPlayerBasicAtkParam(
                        isLeftHand ? MODEL.ChrAsm.LeftWeapon : MODEL.ChrAsm.RightWeapon,
                        (int)evBox.MyEvent.Parameters["BehaviorSubID"],
                        isLeftHand);
                }
            }
            else if (evBox.MyEvent.TypeName == "InvokeCommonBehavior")
            {
                atkParam = ParamManager.GetPlayerCommonAttack((int)evBox.MyEvent.Parameters["BehaviorParamID"]);
            }
            else if (evBox.MyEvent.TypeName == "InvokePCBehavior")
            {
                if (MODEL.IS_PLAYER)
                {
                    int condition = (int)evBox.MyEvent.Parameters["Condition"];
                    if (condition == 4)
                    {
                        atkParam = ParamManager.GetPlayerCommonAttack((int)evBox.MyEvent.Parameters["BehaviorSubID"]);
                    }
                    else if (condition == 8)
                    {
                        atkParam = ParamManager.GetPlayerBasicAtkParam(
                            MODEL.DummyPolyMan.IsViewingLeftHandHit ? 
                            MODEL.ChrAsm.LeftWeapon : MODEL.ChrAsm.RightWeapon,
                            (int)evBox.MyEvent.Parameters["BehaviorSubID"], 
                            isLeftHand: MODEL.DummyPolyMan.IsViewingLeftHandHit);
                    }
                    else if (condition == 2)
                    {
                        atkParam = ParamManager.GetPlayerBasicAtkParam(
                            MODEL.ChrAsm.LeftWeapon,
                            (int)evBox.MyEvent.Parameters["BehaviorSubID"], isLeftHand: true);
                    }
                    else
                    {
                        Console.WriteLine($"Unknown InvokePCBehavior condition: {condition}");
                    }
                }
            }

            return atkParam;
        }

        private void InitAllEntries()
        {
            entries = new Dictionary<string, EventSimEntry>
            {

                {
                    "EventSimAttackBehaviors",
                    new EventSimEntry("Simulate Hitbox Events", true,
                        "InvokeAttackBehavior", "InvokePCBehavior", "InvokeCommonBehavior", "InvokeThrowDamageBehavior")
                    {
                        NewAnimSelectedAction = (entry, evBoxes) =>
                        {
                            MODEL.DummyPolyMan.DeactivateAllHitboxes();
                        },
                        //EnterAction = (entry, evBox) =>
                        //{
                        //    var atkParam = GetAtkParamFromEventBox(evBox);

                        //    if (atkParam != null)
                        //    {
                        //        MODEL.DummyPolyMan.ActivateHitbox(atkParam);
                        //    }
                        //    //if (MODEL.DummyPolyMan.HitboxPrimitiveInfos.ContainsKey(evBox))
                        //    //    foreach (var hitbox in MODEL.DummyPolyMan.HitboxPrimitiveInfos[evBox].Primitives)
                        //    //        hitbox.EnableDraw = true;
                        //},
                        //ExitAction = (entry, evBox) =>
                        //{
                        //    var atkParam = GetAtkParamFromEventBox(evBox);

                        //    if (atkParam != null)
                        //    {
                        //        MODEL.DummyPolyMan.DeactivateHitbox(atkParam);
                        //    }

                        //   //if (MODEL.DummyPolyMan.HitboxPrimitiveInfos.ContainsKey(evBox))
                        //   //     foreach (var hitbox in MODEL.DummyPolyMan.HitboxPrimitiveInfos[evBox].Primitives)
                        //   //         hitbox.EnableDraw = false;
                        //},
                        SimulationFrameChangeAction = (entry, evBoxes, time) =>
                        {
                            MODEL.DummyPolyMan.DeactivateAllHitboxes();
                            foreach (var evBox in evBoxes.Where(b => entry.DoesEventMatch(b)))
                            {
                                var atkParam = GetAtkParamFromEventBox(evBox);

                                if (atkParam != null)
                                {
                                    if (evBox.PlaybackHighlight)
                                        MODEL.DummyPolyMan.ActivateHitbox(atkParam);
                                }
                            }
                        }
                    }
                },

                {
                    "EventSimOpacity",
                    new EventSimEntry("Simulate Opacity Change Events", true,
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

                            foreach (var evBox in evBoxes.Where(b => b.PlaybackHighlight && entry.DoesEventMatch(b)))
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
                    new EventSimEntry("Simulate Draw Mask Changes", true,
                        "ShowModelMask", "HideModelMask", "ChangeChrDrawMask")
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

                            foreach (var evBox in evBoxes.OrderBy(evb => evb.MyEvent.StartTime))
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
                    menu.AddItem("Simulation", kvp.Value.MenuOptionName, () => GetSimEnabled(kvp.Key), b => SetSimEnabled(kvp.Key, b));
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
