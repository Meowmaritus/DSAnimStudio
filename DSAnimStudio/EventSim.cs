using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class EventSim
    {

        public static IReadOnlyDictionary<string, EventSimEntry> Entries => entries;

        private static Dictionary<string, EventSimEntry> entries = new Dictionary<string, EventSimEntry>
        {

            {
                "EventSimAttackBehaviors",
                new EventSimEntry("Simulate Hitbox Events", true, 
                    "InvokeAttackBehavior", "InvokePCBehavior", "InvokeCommonBehavior", "InvokeThrowDamageBehavior")
                {
                    NewAnimSelectedAction = (entry) =>
                    {
                        DummyPolyManager.ClearAllHitboxPrimitives();
                        DummyPolyManager.BuildAllHitboxPrimitives();
                    },
                    EnterAction = (entry, evBox) =>
                    {
                        if (DummyPolyManager.HitboxPrimitiveInfos.ContainsKey(evBox))
                            foreach (var hitbox in DummyPolyManager.HitboxPrimitiveInfos[evBox].Primitives)
                                hitbox.EnableDraw = true;
                    },
                    ExitAction = (entry, evBox) =>
                    {
                       if (DummyPolyManager.HitboxPrimitiveInfos.ContainsKey(evBox))
                            foreach (var hitbox in DummyPolyManager.HitboxPrimitiveInfos[evBox].Primitives)
                                hitbox.EnableDraw = false;
                    },
                }
            },

            {
                "EventSimOpacity",
                new EventSimEntry("Simulate Opacity Change Events", true, 
                    "SetOpacityKeyframe")
                {
                    NewAnimSelectedAction = (entry) =>
                    {
                        GFX.FlverOpacity = 1.0f;
                    },
                    SimulationStartAction = (entry) =>
                    {
                        GFX.FlverOpacity = 1.0f;
                    },
                    SimulationScrubAction = (entry) =>
                    {
                        GFX.FlverOpacity = 1.0f;
                    },
                    DuringAction = (entry, evBox) =>
                    {
                        float fadeDuration = evBox.MyEvent.EndTime - evBox.MyEvent.StartTime;
                        float timeSinceFadeStart = (float)TaeInterop.PlaybackCursor.CurrentTime - evBox.MyEvent.StartTime;
                        float fadeStartOpacity = (float)evBox.MyEvent.Parameters["GhostVal1"];
                        float fadeEndOpacity = (float)evBox.MyEvent.Parameters["GhostVal2"];

                        GFX.FlverOpacity = MathHelper.Lerp(fadeStartOpacity, fadeEndOpacity, timeSinceFadeStart / fadeDuration);
                    },
                    SimulationEndAction = (entry) =>
                    {
                        GFX.FlverOpacity = 1.0f;
                    },
                }
            },


            {
                "EventSimDrawMasks",
                new EventSimEntry("Simulate Draw Mask Changes", true, 
                    "ShowModelMask", "HideModelMask", "ChangeChrDrawMask")
                {
                    NewAnimSelectedAction = (entry) =>
                    {
                        entry.Vars.ClearAllData();

                        GFX.ModelDrawer.DefaultAllMaskValues();
                    },
                    SimulationStartAction = (entry) =>
                    {
                        GFX.ModelDrawer.DefaultAllMaskValues();
                    },
                    SimulationScrubAction = (entry) =>
                    {
                        //// Don't do this for the other mask ones being overrided with this.
                        //if (entry == entries["ChangeChrDrawMask"])
                        //    GFX.ModelDrawer.DefaultAllMaskValues();
                    },
                    EnterAction = (entry, evBox) =>
                    {
                        if (GFX.ModelDrawer.Mask == null)

                            return;
                        if (evBox.MyEvent.TypeName == "ShowModelMask" || evBox.MyEvent.TypeName == "HideModelMask")
                        {
                            if (entry.Vars["OriginalMask"] == null)
                            entry.Vars["OriginalMask"] = new EventSimEntryVariableContainer();

                            var maskVar = (EventSimEntryVariableContainer)(entry.Vars["OriginalMask"]);

                            var newMaskCopy = maskVar[evBox] != null ? ((bool[])maskVar[evBox]) : new bool[GFX.ModelDrawer.Mask.Length];

                            Array.Copy(GFX.ModelDrawer.Mask, newMaskCopy, GFX.ModelDrawer.Mask.Length);
                            maskVar[evBox] = newMaskCopy;
                        }

                        for (int i = 0; i < 32; i++)
                        {
                            if (evBox.MyEvent.Parameters.Template.ContainsKey($"Mask{(i + 1)}"))
                            {
                                if (evBox.MyEvent.TypeName == "ChangeChrDrawMask")
                                {
                                    var maskByte = (byte)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"];

                                    // Before you get out the torch, be aware that the game 
                                    // uses some value other than 0 to SKIP
                                    if (maskByte == 0)
                                        GFX.ModelDrawer.Mask[i] = false;
                                    else if (maskByte == 1)
                                        GFX.ModelDrawer.Mask[i] = true;
                                }
                            }
                        }
                    },
                    DuringAction = (entry, evBox) =>
                    {
                        if (GFX.ModelDrawer.Mask == null)
                            return;

                        for (int i = 0; i < 32; i++)
                        {
                            if (evBox.MyEvent.Parameters.Template.ContainsKey($"Mask{(i + 1)}"))
                            {
                                if (evBox.MyEvent.TypeName == "ShowModelMask")
                                {
                                    if ((bool)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"])
                                        GFX.ModelDrawer.Mask[i] = true;
                                }
                                else if (evBox.MyEvent.TypeName == "HideModelMask")
                                {
                                    if ((bool)evBox.MyEvent.Parameters[$"Mask{(i + 1)}"])
                                        GFX.ModelDrawer.Mask[i] = false;
                                }
                            }
                        }
                    },
                    ExitAction = (entry, evBox) =>
                    {
                        if (GFX.ModelDrawer.Mask == null)
                            return;

                         if (evBox.MyEvent.TypeName == "ShowModelMask" || evBox.MyEvent.TypeName == "HideModelMask")
                         {
                             if (entry.Vars["OriginalMask"] != null)
                             {
                                 var maskVar = (EventSimEntryVariableContainer)(entry.Vars["OriginalMask"]);
                                 if (maskVar != null)
                                 {
                                     GFX.ModelDrawer.Mask = (bool[])(maskVar[evBox]);
                                 }
                         
                             }
                         }
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

        public static void BuildEventSimMenuBar(TaeMenuBarBuilder menu)
        {
            if (menu["Simulation"] != null)
                menu.ClearItem("Simulation");

            foreach (var kvp in Entries)
            {
                if (kvp.Value.MenuOptionName != null)
                    menu.AddItem("Simulation", kvp.Value.MenuOptionName, () => GetSimEnabled(kvp.Key), b => SetSimEnabled(kvp.Key, b));
            }
        }

        private static bool GetSimEnabled(string simName)
        {
            if (simName == null)
                return false;

            if (!Main.TAE_EDITOR.Config.EventSimulationsEnabled.ContainsKey(simName))
                Main.TAE_EDITOR.Config.EventSimulationsEnabled.Add(simName, Entries[simName].IsEnabledByDefault);

            return Main.TAE_EDITOR.Config.EventSimulationsEnabled[simName];
        }

        private static void SetSimEnabled(string simName, bool enabled)
        {
            if (!Main.TAE_EDITOR.Config.EventSimulationsEnabled.ContainsKey(simName))
                Main.TAE_EDITOR.Config.EventSimulationsEnabled.Add(simName, enabled);
            else
                Main.TAE_EDITOR.Config.EventSimulationsEnabled[simName] = enabled;
        }

        public static void OnNewAnimSelected()
        {
            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.NewAnimSelectedAction?.Invoke(kvp.Value);
            }
        }

        private static string GetSimEntryForEventBox(TaeEditAnimEventBox evBox)
        {
            return entries.FirstOrDefault(kvp => kvp.Value.EventTypes.Contains(evBox.MyEvent.TypeName)).Key;
        }

        public static void OnSimulationStart()
        {
            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.SimulationStartAction?.Invoke(kvp.Value);
            }
        }

        public static void OnSimulationEnd()
        {
            if (TaeInterop.IsLoadingAnimation)
                return;

            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.SimulationEndAction?.Invoke(kvp.Value);
            }
        }

        public static void OnSimulationScrub()
        {
            if (TaeInterop.IsLoadingAnimation)
                return;

            foreach (var kvp in entries)
            {
                if (!GetSimEnabled(kvp.Key))
                    continue;

                kvp.Value.SimulationScrubAction?.Invoke(kvp.Value);
            }
        }

        public static void OnEventEnter(TaeEditAnimEventBox evBox)
        {
            if (TaeInterop.IsLoadingAnimation || evBox.MyEvent.Template == null)
                return;

            var matchingSim = GetSimEntryForEventBox(evBox);

            if (!GetSimEnabled(matchingSim))
                return;

            if (entries[matchingSim] != null)
                entries[matchingSim].EnterAction?.Invoke(entries[matchingSim], evBox);
        }

        public static void OnEventMidFrame(TaeEditAnimEventBox evBox)
        {
            if (TaeInterop.IsLoadingAnimation || evBox.MyEvent.Template == null)
                return;

            var matchingSim = GetSimEntryForEventBox(evBox);

            if (!GetSimEnabled(matchingSim))
                return;

            if (entries[matchingSim] != null)
                entries[matchingSim].DuringAction?.Invoke(entries[matchingSim], evBox);
        }

        public static void OnEventExit(TaeEditAnimEventBox evBox)
        {
            if (TaeInterop.IsLoadingAnimation || evBox.MyEvent.Template == null)
                return;

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

            public Action<EventSimEntry> NewAnimSelectedAction;
            public Action<EventSimEntry> SimulationStartAction;
            public Action<EventSimEntry> SimulationEndAction;
            public Action<EventSimEntry> SimulationScrubAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> EnterAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> DuringAction;
            public Action<EventSimEntry, TaeEditAnimEventBox> ExitAction;
            public EventSimEntryVariableContainer Vars = new EventSimEntryVariableContainer();

            public readonly string MenuOptionName;
            public readonly bool IsEnabledByDefault;

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
