using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SoundManager
    {
        public static object _LOCK = new object();

        private static Dictionary<int, SoundSlot> slots = new Dictionary<int, SoundSlot>();
        private static List<SoundSlot> oneShotAutoPlaySlots = new List<SoundSlot>();
        private static Dictionary<TaeEditor.TaeEditAnimEventBox, SoundSlot> eventBoxAutoSlots = new Dictionary<TaeEditor.TaeEditAnimEventBox, SoundSlot>();

        private static List<int> alreadyRequestedSlots = new List<int>();
        private static List<TaeEditor.TaeEditAnimEventBox> alreadyRequestedEventBoxSlots = new List<TaeEditor.TaeEditAnimEventBox>();

        public static bool DebugShowDiagnostics = false;

        public static float AdjustSoundVolume = 100;

        public static string GetDebugDiagnosticString()
        {
            var sb = new StringBuilder();
            lock (_LOCK)
            {
                void doInst(SoundInstance inst)
                {
                    var w = (inst as SoundInstanceWwise);
                    sb.AppendLine($"    {w.WEMID}.wem [State:{w.State}] [Vol:{w.Volume}] [Pitch:{w.Pitch}] [Pan:{w.Pan}]");
                }

                sb.AppendLine("[RIGHT CLICK HOLD PREVIEW]");
                if (mouseClickPreviewSoundSlot != null)
                {
                    sb.AppendLine($"  {mouseClickPreviewSoundSlot.PlayInfo.SoundEventName} @ Dmy {mouseClickPreviewSoundSlot.PlayInfo.DummyPolyID}");
                    var instances = mouseClickPreviewSoundSlot.PeekInstances();
                    foreach (var inst in instances)
                    {
                        doInst(inst);
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("\n\n\n[ONE SHOT]");
                foreach (var slot in oneShotAutoPlaySlots)
                {
                    sb.AppendLine($"  {slot.PlayInfo.SoundEventName} @ Dmy {slot.PlayInfo.DummyPolyID}");
                    var instances = slot.PeekInstances();
                    foreach (var inst in instances)
                    {
                        doInst(inst);
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("\n\n\n[NORMAL SLOT]");
                foreach (var slot in slots)
                {
                    sb.AppendLine($"  [{slot.Key:D2}] {slot.Value.PlayInfo.SoundEventName} @ Dmy {slot.Value.PlayInfo.DummyPolyID}");
                    var instances = slot.Value.PeekInstances();
                    foreach (var inst in instances)
                    {
                        doInst(inst);
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("\n\n\n[AUTO SLOT]");
                foreach (var slot in eventBoxAutoSlots)
                {
                    sb.AppendLine($"  [\"{slot.Key.EventText.Text}\"] {slot.Value.PlayInfo.SoundEventName} @ Dmy {slot.Value.PlayInfo.DummyPolyID}");
                    var instances = slot.Value.PeekInstances();
                    foreach (var inst in instances)
                    {
                        doInst(inst);
                    }
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public static Dictionary<Vector3, List<string>> GetDebugDrawInstances()
        {
            const float Tolerance = 0.01f;
            Dictionary<Vector3, List<string>> res = new Dictionary<Vector3, List<string>>();
            lock (_LOCK)
            {
                var allSlots = GetAllSoundSlotsFromAllSources();

                foreach (var s in allSlots)
                {
                    if (s.State is SoundSlot.States.InitializingInBgThread or SoundSlot.States.Stopped)
                        continue;
                    Vector3 key = s.Position + GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;

                    foreach (var r in res)
                    {
                        if ((r.Key - key).LengthSquared() <= (Tolerance * Tolerance))
                        {
                            key = r.Key;
                            break;
                        }
                    }

                    if (!res.ContainsKey(key))
                        res.Add(key, new List<string>());

                    if (!res[key].Contains(s.PlayInfo.SoundEventName))
                        res[key].Add(s.PlayInfo.SoundEventName);
                }
            }

            return res;

        }

        private static List<SoundSlot> GetAllSoundSlotsFromAllSources()
        {
            var x = slots.Values.Concat(oneShotAutoPlaySlots).Concat(eventBoxAutoSlots.Values).ToList();
            if (mouseClickPreviewSoundSlot != null)
                x.Add(mouseClickPreviewSoundSlot);
            return x;
        }

        private static SoundSlot mouseClickPreviewSoundSlot = null;

        public static void PlayOneShotSound(SoundPlayInfo info)
        {
            lock (_LOCK)
            {
                var slot = CreateSlot_NoLock(info);
                oneShotAutoPlaySlots.Add(slot);
            }
        }

        private static SoundSlot CreateSlot_NoLock(SoundPlayInfo info)
        {
            var newSlot = new SoundSlot();
            newSlot.PlayInfo = info;
            newSlot.GetPosFunc = info.GetGetPosFunc();
            newSlot.UpdatingPosition = info.UpdatingPosition;
            newSlot.Position = newSlot.GetPosFunc();
            var captureInfo = info;
            if (GameRoot.GameTypeUsesWwise)
            {
                newSlot.BackgroundInitializeTask = Task.Run(() =>
                {
                    var wems = Wwise.GetPlaybackInstances(captureInfo);
                    var instances = new List<SoundInstance>();
                    foreach (var w in wems)
                    {
                        var inst = new SoundInstanceWwise(w);
                        instances.Add(inst);
                    }
                    return instances;
                });
            }
            else
            {
                newSlot.BackgroundInitializeTask = Task.Run(() =>
                {
                    var fmodUpdater = FmodManager.PlaySE(captureInfo.SoundType, captureInfo.SoundID, captureInfo.NightfallLifetime);
                    var inst = new SoundInstanceFmod(fmodUpdater);
                    return new List<SoundInstance>() { inst };
                });
            }

            return newSlot;
        }

        private static void AddEventBoxSlot(TaeEditor.TaeEditAnimEventBox evBox, SoundPlayInfo info, bool isLock)
        {
            if (isLock)
            {
                lock (_LOCK)
                {
                    var newSlot = CreateSlot_NoLock(info);
                    eventBoxAutoSlots.Add(evBox, newSlot);
                }
            }
            else
            {
                var newSlot = CreateSlot_NoLock(info);
                eventBoxAutoSlots.Add(evBox, newSlot);
            }
        }

        private static void AddSlot(int slot, SoundPlayInfo info, bool isLock)
        {
            if (isLock)
            {
                lock (_LOCK)
                {
                    var newSlot = CreateSlot_NoLock(info);
                    slots.Add(slot, newSlot);
                }
            }
            else
            {
                var newSlot = CreateSlot_NoLock(info);
                slots.Add(slot, newSlot);
            }
        }

        public static void PurgeLoadedAssets()
        {
            lock (_LOCK)
            {
                foreach (var kvp in slots)
                    kvp.Value.KillImmediately();
                slots.Clear();
                foreach (var s in oneShotAutoPlaySlots)
                    s.KillImmediately();
                oneShotAutoPlaySlots.Clear();
                foreach (var kvp in eventBoxAutoSlots)
                    kvp.Value.KillImmediately();
            }
            if (GameRoot.GameTypeUsesWwise)
                Wwise.PurgeLoadedAssets();
            else
                FmodManager.Purge();
        }

        public static void DisposeAll()
        {
            PurgeLoadedAssets();
            if (GameRoot.GameTypeUsesWwise)
                Wwise.DisposeAll();
            else
                FmodManager.Shutdown();
        }

        public static void StopAllSounds(bool immediate = true)
        {
            if (!GameRoot.GameTypeUsesWwise)
                FmodManager.StopAllSounds();

            lock (_LOCK)
            {
                var allSlots = GetAllSoundSlotsFromAllSources();
                if (immediate)
                {
                    foreach (var s in allSlots)
                        s.KillImmediately();
                    slots.Clear();
                    eventBoxAutoSlots.Clear();
                    oneShotAutoPlaySlots.Clear();
                    mouseClickPreviewSoundSlot = null;
                }
                else
                {
                    foreach (var s in allSlots)
                        s.Stop();
                }

            }
        }

        private static void InsideLock_UpdateSlots(float deltaTime, Matrix listener, Dictionary<int, SoundPlayInfo> requestedSoundSlots)
        {
            var deadSlots = new List<int>();
            foreach (var s in slots)
            {
                if (s.Value.State == SoundSlot.States.InitializingInBgThread)
                {
                    s.Value.Update(deltaTime, listener);
                    continue;
                }
                // if the slot is no longer being played
                // or if the slot has the wrong ID vs the one requested
                // AND KillSoundOnEventEnd is true
                // THEN, request sound to stop.
                if ((!requestedSoundSlots.ContainsKey(s.Key) || !requestedSoundSlots[s.Key].Matches(s.Value.PlayInfo))
                    && s.Value.PlayInfo.KillSoundOnEventEnd)
                    s.Value.Stop();

                s.Value.Update(deltaTime, listener);
                // If slot is empty (all sound samples have finished playing)
                // Then slot is "dead", queue for deletion.
                if (s.Value.SlotCompletelyEmpty)
                    deadSlots.Add(s.Key);
            }
            foreach (var s in deadSlots)
            {
                slots[s].KillImmediately();
                slots.Remove(s);
            }

            foreach (var s in requestedSoundSlots)
            {
                if (!slots.ContainsKey(s.Key))
                {
                    if (alreadyRequestedSlots.Contains(s.Key) && !s.Value.PlayRepeatedlyWhileEventActive)
                        continue;
                    AddSlot(s.Key, s.Value, isLock: false);
                }
            }
        }

        private static void InsideLock_UpdateOneShotSlots(float deltaTime, Matrix listener)
        {
            var deadSlots = new List<SoundSlot>();
            foreach (var s in oneShotAutoPlaySlots)
            {
                if (s.State == SoundSlot.States.InitializingInBgThread)
                {
                    s.Update(deltaTime, listener);
                    continue;
                }

                s.Update(deltaTime, listener);
                if (s.SlotCompletelyEmpty)
                    deadSlots.Add(s);
            }
            foreach (var s in deadSlots)
            {
                s.KillImmediately();
                oneShotAutoPlaySlots.Remove(s);
            }
        }

        private static void InsideLock_UpdateEventBoxSlots(float deltaTime, Matrix listener,
            Dictionary<TaeEditor.TaeEditAnimEventBox, SoundPlayInfo> requestedSoundSlots)
        {
            var deadSlots = new List<TaeEditor.TaeEditAnimEventBox>();
            foreach (var s in eventBoxAutoSlots)
            {
                if (s.Value.State == SoundSlot.States.InitializingInBgThread)
                {
                    s.Value.Update(deltaTime, listener);
                    continue;
                }

                // if the slot is no longer being played
                // or if the slot has the wrong ID vs the one requested
                // Since these are event box slots, we know KillSoundOnEventEnd is true, but whatever
                if ((!requestedSoundSlots.ContainsKey(s.Key) || !requestedSoundSlots[s.Key].Matches(s.Value.PlayInfo))
                    && s.Value.PlayInfo.KillSoundOnEventEnd)
                    s.Value.Stop();

                s.Value.Update(deltaTime, listener);
                // If slot is empty (all sound samples have finished playing)
                // Then slot is "dead", queue for deletion.
                if (s.Value.SlotCompletelyEmpty)
                    deadSlots.Add(s.Key);
            }
            foreach (var s in deadSlots)
            {
                eventBoxAutoSlots[s].KillImmediately();
                eventBoxAutoSlots.Remove(s);
            }

            foreach (var s in requestedSoundSlots)
            {
                if (!eventBoxAutoSlots.ContainsKey(s.Key))
                {
                    if (alreadyRequestedEventBoxSlots.Contains(s.Key) && !s.Value.PlayRepeatedlyWhileEventActive)
                        continue;
                    AddEventBoxSlot(s.Key, s.Value, isLock: false);
                }
            }
        }

        public static void Update(float deltaTime, Matrix listener, TaeEditor.TaeEditorScreen tae)
        {
            // ??????? lmao
            if (GFX.CurrentWorldView.RootMotionFollow_EnableWrap)
            {
                listener *= Matrix.CreateTranslation(-GFX.CurrentWorldView.RootMotionFollow_Translation);
            }

            if (!GameRoot.GameTypeUsesWwise)
                FmodManager.Update();

            if (!(Main.Config?.GetEventSimulationEnabled("EventSimSE") ?? false))
            {
                StopAllSounds();
                return;
            }

            lock (_LOCK)
            {
                Dictionary<int, SoundPlayInfo> requestedSoundSlots = new Dictionary<int, SoundPlayInfo>();
                Dictionary<TaeEditor.TaeEditAnimEventBox, SoundPlayInfo> requestedEventBoxSlots = new Dictionary<TaeEditor.TaeEditAnimEventBox, SoundPlayInfo>();
                SoundPlayInfo mouseClickPreviewRequest = null;

                var sim = tae?.Graph?.ViewportInteractor?.EventSim;

                var animJustLooped = tae?.Graph?.PlaybackCursor?.JustLooped ?? false;

                if (animJustLooped)
                {
                    alreadyRequestedEventBoxSlots.Clear();
                }

                if (sim != null)
                {
                    var evBoxesCopy = tae.Graph.EventBoxes.ToList();
                    foreach (var evBox in evBoxesCopy)
                    {
                        if (evBox.MyEvent.TypeName == null ||
                            !(evBox.MyEvent.TypeName.Contains("Play") && evBox.MyEvent.TypeName.Contains("Sound")) ||
                            !evBox.MyEvent.Template.ContainsKey("SlotID"))
                            continue;

                        int slot = Convert.ToInt32(evBox.MyEvent.Parameters["SlotID"]);
                        bool killSoundOnEventEnd = false;
                        if (evBox.MyEvent.Template.ContainsKey("PlaybackBehaviorType"))
                        {
                            killSoundOnEventEnd = Convert.ToByte(evBox.MyEvent.Parameters["PlaybackBehaviorType"]) == 1;
                        }

                        if (evBox == tae.HoveringOverEventBox && tae.Input.RightClickHeld && (slot >= 0 || killSoundOnEventEnd))
                        {
                            mouseClickPreviewRequest = sim.GetSoundPlayInfoOfBox(evBox, isForOneShot: false);
                            continue;
                        }

                        bool isBoxSoundActive = evBox.PlaybackHighlight && (tae.Graph.PlaybackCursor.IsPlaying || tae.Graph.PlaybackCursor.Scrubbing);
                        if (!isBoxSoundActive)
                            continue;



                        if (!(slot >= 0 || killSoundOnEventEnd))
                            continue;

                        var playInfo = sim.GetSoundPlayInfoOfBox(evBox, isForOneShot: false);

                        if (!GameRoot.GameTypeUsesWwise)
                        {
                            killSoundOnEventEnd = playInfo.KillSoundOnEventEnd = slot >= 0;
                        }

                        if (killSoundOnEventEnd)
                        {
                            if (!requestedEventBoxSlots.ContainsKey(evBox))
                                requestedEventBoxSlots.Add(evBox, playInfo);
                        }
                        else if (slot >= 0)
                        {
                            if (!requestedSoundSlots.ContainsKey(slot))
                                requestedSoundSlots.Add(slot, playInfo);
                        }
                    }
                }

                var requestedSlotsFreed = new List<int>();
                foreach (var s in alreadyRequestedSlots)
                {
                    if (!requestedSoundSlots.ContainsKey(s))
                        requestedSlotsFreed.Add(s);
                }
                foreach (var s in requestedSlotsFreed)
                    alreadyRequestedSlots.Remove(s);

                var requestedEventBoxSlotsFreed = new List<TaeEditor.TaeEditAnimEventBox>();
                foreach (var s in alreadyRequestedEventBoxSlots)
                {
                    if (!requestedEventBoxSlots.ContainsKey(s))
                        requestedEventBoxSlotsFreed.Add(s);
                }
                foreach (var s in requestedEventBoxSlotsFreed)
                    alreadyRequestedEventBoxSlots.Remove(s);

                InsideLock_UpdateSlots(deltaTime, listener, requestedSoundSlots);
                InsideLock_UpdateOneShotSlots(deltaTime, listener);
                InsideLock_UpdateEventBoxSlots(deltaTime, listener, requestedEventBoxSlots);

                foreach (var s in requestedSoundSlots)
                {
                    if (!alreadyRequestedSlots.Contains(s.Key))
                        alreadyRequestedSlots.Add(s.Key);
                }

                foreach (var s in requestedEventBoxSlots)
                {
                    if (!alreadyRequestedEventBoxSlots.Contains(s.Key))
                        alreadyRequestedEventBoxSlots.Add(s.Key);
                }

                if (mouseClickPreviewSoundSlot != null)
                {
                    if (!mouseClickPreviewSoundSlot.PlayInfo.Matches(mouseClickPreviewRequest))
                        mouseClickPreviewSoundSlot.Stop();
                    mouseClickPreviewSoundSlot.Update(deltaTime, listener);
                }

                if (mouseClickPreviewRequest != null)
                {
                    if (mouseClickPreviewSoundSlot == null)
                    {
                        mouseClickPreviewSoundSlot = CreateSlot_NoLock(mouseClickPreviewRequest);
                    }
                }
                else
                {
                    if (mouseClickPreviewSoundSlot != null)
                    {
                        mouseClickPreviewSoundSlot.Stop();
                        if (mouseClickPreviewSoundSlot.SlotCompletelyEmpty)
                        {
                            mouseClickPreviewSoundSlot.KillImmediately();
                            mouseClickPreviewSoundSlot = null;
                        }
                    }
                }
            }
        }
    }
}
