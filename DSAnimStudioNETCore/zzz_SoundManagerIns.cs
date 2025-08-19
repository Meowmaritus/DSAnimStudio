using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class zzz_SoundManagerIns
    {
        public int BankNameArray_SelectedIndex = -1;
        public string[] BankNameArray = new string[] { };
        private Dictionary<uint, string> _bankNameList = null;
        private object _lock_bankNameList = new object();
        public object _lock_BankNameArray = new object();

        public object _lock_LookupBanks = new object();

        public string[] LookupBanks = new string[] { };

        public int LookupBanks_SelectedIndex = -1;

        public zzz_WwiseManagerInst WwiseManager;
        public zzz_MagicOrchestraManagerInst MagicOrchestraManager;

        public zzz_DocumentIns ParentDocument;
        public zzz_SoundManagerIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
            WwiseManager = new zzz_WwiseManagerInst(parentDocument, this);
            MagicOrchestraManager = new zzz_MagicOrchestraManagerInst(parentDocument, this);
        }

        private List<string> GetMasterBankBlacklist()
        {
            var game = ParentDocument.GameRoot.GameType;
            switch (game)
            {
                case SoulsGames.DS1:
                case SoulsGames.DS1R:
                    return new() 
                    { 
                        "fdlc",
                        "fdlc_stationary",
                        "frpg",
                        "frpg_mixer",
                        "frpg_stationary",
                    };
                case SoulsGames.BB:
                    return new()
                    {
                        "sprj_mixer"
                    };
                case SoulsGames.DS3:
                    return new()
                    {
                        "fdp_mixer"
                    };
                case SoulsGames.SDT:
                    return new()
                    {
                        "mixer"
                    };
                case SoulsGames.ER:
                case SoulsGames.ERNR:
                case SoulsGames.AC6:
                    return new()
                    {
                        "init"
                    };
                default:
                    return new();
            }
        }

        private void PopulateMasterBankList()
        {
            var game = ParentDocument.GameRoot.GameType;
            if (EngineType is EngineTypes.FMOD)
            {
                string dir = "/sound/";
                //if (game == SoulsGames.BB)
                //    dir = "/_DSAS_CONVERTED_SOUNDS/";
                PopulateMasterBankList_Inner(dir, ".fev");
            }
            else if (EngineType is EngineTypes.Wwise)
            {
                PopulateMasterBankList_Inner(null, ".bnk");
            }
            else if (EngineType is EngineTypes.MagicOrchestra)
            {
                PopulateMasterBankList_Inner("/sound/", ".mosb");
            }
        }

        private void PopulateMasterBankList_Inner(string soundDirectory, string fileExtension)
        {
            if (_bankNameList == null || _bankNameList?.Count == 0)
            {
                var blacklist = GetMasterBankBlacklist();
                _bankNameList = new Dictionary<uint, string>();
                foreach (var thing in ParentDocument.GameData.UxmDictionary)
                {
                    var fn = thing.ToLower().Trim();
                    if (fn.EndsWith(fileExtension))
                    {
                        fn = Utils.GetShortIngameFileName(fn);
                        if (blacklist.Contains(fn))
                            continue;
                        _bankNameList[Hash(fn)] = fn;
                    }
                }

                var soundDirFiles = ParentDocument.GameData.GetFilesInDir(soundDirectory, zzz_GameDataIns.SearchType.AllFiles);
                foreach (var thing in soundDirFiles)
                {
                    var fn = thing.ToLower().Trim();
                    if (fn.EndsWith(fileExtension))
                    {
                        fn = Utils.GetShortIngameFileName(fn);
                        if (blacklist.Contains(fn))
                            continue;
                        _bankNameList[Hash(fn)] = fn;
                    }
                }

                lock (_lock_BankNameArray)
                {
                    BankNameArray = _bankNameList.Values.OrderBy(v => v).ToArray();
                    BankNameArray_SelectedIndex = -1;
                }

            }

            Console.WriteLine("test");
        }

        private ConcurrentDictionary<string, uint> FnvHashCache = new ConcurrentDictionary<string, uint>();
        public uint Hash(string input)//, bool use32bits)
        {
            return FnvHashCache.GetOrAdd(input, key => GetFnvHashOfBytes(Encoding.ASCII.GetBytes(key.ToLowerInvariant()).ToArray()));
        }
        public uint GetFnvHashOfBytes(byte[] input)//, bool use32bits)
        {
            uint prime = 16777619;
            uint offset = 2166136261;

            uint hash = offset;
            for (int i = 0; i < input.Length; i++)
            {
                hash *= prime;
                hash ^= input[i];
            }

            return hash;

            //uint mask = 1073741823; //Bitmask for first 30 bits
            //if (use32bits)
            //{
            //    return hash;
            //}
            //else
            //{
            //    return (hash >> 30) ^ (hash & mask); //XOR folding
            //}
        }

        public string GetBankNameFromHash(uint fnvHash)
        {
            string result = null;
            lock (_lock_bankNameList)
            {
                if (_bankNameList != null && _bankNameList.ContainsKey(fnvHash))
                {
                    result = _bankNameList[fnvHash];
                }
            }

            return result;
        }

        public void ClearBankLists(bool masterList, bool lookupList)
        {
            if (masterList)
            {
                lock (_lock_BankNameArray)
                {
                    BankNameArray = new string[] { };
                    BankNameArray_SelectedIndex = -1;
                }
                lock (_lock_bankNameList)
                {
                    _bankNameList = null;
                }
            }

            if (lookupList)
            {
                lock (_lock_LookupBanks)
                {
                    LookupBanks = new string[] { };
                    LookupBanks_SelectedIndex = -1;
                }
            }
            
        }


        public enum EngineTypes
        {
            None = 0,
            FMOD = 1,
            Wwise = 2,
            MagicOrchestra = 3,
        }

        public void SetEngineToCurrentGame(SoulsGames game, bool clearMasterBankList = true, bool clearLookupBankList = true)
        {
            EngineType = EngineTypes.None;
            switch (game)
            {
                case SoulsGames.DES:
                    EngineType = EngineTypes.MagicOrchestra;
                    break;
                case SoulsGames.DS1:
                case SoulsGames.DS1R:
                case SoulsGames.BB:
                case SoulsGames.DS3:
                case SoulsGames.SDT:
                    EngineType = EngineTypes.FMOD;
                    break;
                case SoulsGames.ER:
                case SoulsGames.ERNR:
                case SoulsGames.AC6:
                    EngineType = EngineTypes.Wwise;
                    break;
            }

            ClearBankLists(clearMasterBankList, clearLookupBankList);
            PopulateMasterBankList();

            if (EngineType == EngineTypes.FMOD)
            {
                NewFmodManager.SwitchToFmodIns(ParentDocument.Fmod);
            }
        }

        public EngineTypes EngineType = EngineTypes.None;

        

        //public bool NeedsWwiseRefresh = true;


        public object _LOCK = new object();

        //private List<string> _additionalSoundBanksLoaded = new List<string>();

        public void RegisterAdditionalSoundBank(string soundBankName)
        {
            lock (_LOCK)
            {
                if (ParentDocument?.Proj != null)
                {
                    lock (_lock_LookupBanks)
                    {
                        var prevSelected = LookupBanks_SelectedIndex >= 0 && LookupBanks_SelectedIndex < LookupBanks.Length
                            ? LookupBanks[LookupBanks_SelectedIndex] : null;
                        var list = LookupBanks.ToList();
                        if (!list.Contains(soundBankName))
                            list.Add(soundBankName);
                        LookupBanks = list.ToArray();
                        LookupBanks_SelectedIndex = prevSelected != null ? list.IndexOf(prevSelected) : -1;
                    }
                }
            }

            CopySoundBanksFromThisToProj();
        }

        public static zzz_DocumentIns CurrentDocInControl = null;

        public void CopySoundBanksFromThisToProj()
        {
            lock (_lock_LookupBanks)
            {
                ParentDocument.Proj.SAFE_SetSoundBanksToLoad(LookupBanks.ToList());
            }
        }

        public List<string> GetAdditionalSoundBankNames()
        {
            List<string> result = new List<string>();
            lock (_lock_LookupBanks)
            {
                result = LookupBanks.ToList();
            }
            return result;
        }

        public void CopySoundBanksFromProjToThis()
        {
            var projBanks = ParentDocument.Proj.SAFE_GetSoundBanksToLoad();

            if (projBanks.Count == 0)
            {
                ParentDocument.Proj.SAFE_InitDefaultSoundBanksToLoad();
                projBanks = ParentDocument.Proj.SAFE_GetSoundBanksToLoad();
            }

            lock (_lock_LookupBanks)
            {
                var prevSelected = LookupBanks_SelectedIndex >= 0 && LookupBanks_SelectedIndex < LookupBanks.Length
                            ? LookupBanks[LookupBanks_SelectedIndex] : null;
                var list = projBanks;
                LookupBanks = list.ToArray();
                LookupBanks_SelectedIndex = prevSelected != null ? list.IndexOf(prevSelected) : -1;
            }
        }

        public void RegisterWorldShift(Vector3 v)
        {
            if (v.LengthSquared() > 0)
            {
                lock (_LOCK)
                {
                    foreach (var s in slots)
                        s.Value.PositionOffset += v;
                    foreach (var s in oneShotAutoPlaySlots)
                        s.PositionOffset += v;
                    foreach (var s in actionBoxAutoSlots)
                        s.Value.PositionOffset += v;
                }
            }
        }

        public static bool SOUND_DISABLED = false;

        private Dictionary<int, SoundSlot> slots = new Dictionary<int, SoundSlot>();
        private List<SoundSlot> oneShotAutoPlaySlots = new List<SoundSlot>();
        private Dictionary<DSAProj.Action, SoundSlot> actionBoxAutoSlots = new Dictionary<DSAProj.Action, SoundSlot>();

        private List<int> alreadyRequestedSlots = new List<int>();
        private List<DSAProj.Action> alreadyRequestedActionBoxSlots = new List<DSAProj.Action>();

        // Act7052: NF_PlayDummyPolyFollowSound_Cooldown

        public class DummyPolyFollowSoundExInfo
        {
            public float Cooldown;
            public List<SoundSlot> UsedSlots = new List<SoundSlot>();
            public bool ClearWhenCooldownIDNotInUse = false;
            //public bool IsCooldownIDInUse = false;
        }

        private object _lock_nf_Act7052_ExInfos = new object();
        private Dictionary<int, DummyPolyFollowSoundExInfo> nf_Act7052_ExInfos = new Dictionary<int, DummyPolyFollowSoundExInfo>();

        public DummyPolyFollowSoundExInfo NF_Act7052_PlayDummyPolyFollowSoundEx_GetExInfo(int exInfoID)
        {
            DummyPolyFollowSoundExInfo result = null;
            lock (_lock_nf_Act7052_ExInfos)
            {
                if (!nf_Act7052_ExInfos.ContainsKey(exInfoID))
                    nf_Act7052_ExInfos.Add(exInfoID, new DummyPolyFollowSoundExInfo());
                result = nf_Act7052_ExInfos[exInfoID];
            }
            return result;
        }

        public bool DebugShowDiagnostics = false;

        public float FmodBaseSoundVolume = 0.5f;

        public float AdjustSoundVolume = 100;

        public string GetDebugDiagnosticString()
        {
            var sb = new StringBuilder();
            lock (_LOCK)
            {
                void doInst(SoundInstance inst)
                {
                    if (inst is SoundInstanceWwise w)
                        sb.AppendLine($"    {w.WEMID}.wem [State:{w.State}] [Vol:{w.Volume}] [Pitch:{w.Pitch}] [Pan:{w.Pan}]");
                    else if (inst is SoundInstanceMO m)
                        sb.AppendLine($"    {m.Key.WavebankName}:{m.Key.SoundIndex}.VAG [State:{m.State}] [Vol:{m.Volume}] [Pitch:{m.Pitch}] [Pan:{m.Pan}]");
                }

                sb.AppendLine("[RIGHT CLICK HOLD PREVIEW]");
                if (mouseClickPreviewSoundSlot != null)
                {
                    sb.AppendLine($"  {mouseClickPreviewSoundSlot.PlayInfo.GetSoundEventName()} @ Dmy {mouseClickPreviewSoundSlot.PlayInfo.DummyPolyID}");
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
                    sb.AppendLine($"  {slot.PlayInfo.GetSoundEventName()} @ Dmy {slot.PlayInfo.DummyPolyID}");
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
                    sb.AppendLine($"  [{slot.Key:D2}] {slot.Value.PlayInfo.GetSoundEventName()} @ Dmy {slot.Value.PlayInfo.DummyPolyID}");
                    var instances = slot.Value.PeekInstances();
                    foreach (var inst in instances)
                    {
                        doInst(inst);
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("\n\n\n[AUTO SLOT]");
                foreach (var slot in actionBoxAutoSlots)
                {
                    sb.AppendLine($"  [\"{slot.Key.DisplayStringForFmod}\"] {slot.Value.PlayInfo.GetSoundEventName()} @ Dmy {slot.Value.PlayInfo.DummyPolyID}");
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

        public class DebugDrawInst
        {
            public List<string> Info = new();
            public List<Vector3> PositionOffsets = new();
        }

        public Dictionary<Vector3, DebugDrawInst> GetDebugDrawInstances()
        {
            const float Tolerance = 0.01f;
            Dictionary<Vector3, DebugDrawInst> res = new();
            lock (_LOCK)
            {
                var allSlots = GetAllSoundSlotsFromAllSources();

                foreach (var s in allSlots)
                {
                    if (s.State is SoundSlot.States.InitializingInBgThread or SoundSlot.States.Stopped)
                        continue;
                    Vector3 key = s.Position + s.PositionOffset;// + GFX.CurrentWorldView.RootMotionOffsetFromWrappedCenter;

                    foreach (var r in res)
                    {
                        if ((r.Key - key).LengthSquared() <= (Tolerance * Tolerance))
                        {
                            key = r.Key;
                            break;
                        }
                    }

                    if (!res.ContainsKey(key))
                        res.Add(key, new());

                    var eventName = s.PlayInfo.GetSoundEventName();
                    if (!res[key].Info.Contains(eventName))
                        res[key].Info.Add(eventName);

                    if (!res[key].PositionOffsets.Contains(s.PositionOffset))
                        res[key].PositionOffsets.Add(s.PositionOffset);
                }
            }

            return res;

        }

        private List<SoundSlot> GetAllSoundSlotsFromAllSources()
        {
            var x = slots.Values.Concat(oneShotAutoPlaySlots).Concat(actionBoxAutoSlots.Values).ToList();
            if (mouseClickPreviewSoundSlot != null)
                x.Add(mouseClickPreviewSoundSlot);
            return x;
        }

        private SoundSlot mouseClickPreviewSoundSlot = null;

        public SoundSlot PlayOneShotSound(SoundPlayInfo info)
        {
            SoundSlot slot = null;

            if (!SOUND_DISABLED)
            {
                lock (_LOCK)
                {
                    slot = CreateSlot_NoLock(info);
                    oneShotAutoPlaySlots.Add(slot);
                }
            }

            return slot;

        }



        private SoundSlot CreateSlot_NoLock(SoundPlayInfo info)
        {
            var newSlot = new SoundSlot();
            newSlot.PlayInfo = info;
            newSlot.GetPosFunc = info.GetGetPosFunc();
            newSlot.UpdatingPosition = info.UpdatingPosition;
            newSlot.Position = newSlot.GetPosFunc();
            var captureInfo = info;
            
            if (EngineType is EngineTypes.Wwise)
            {
                newSlot.BackgroundInitializeTask = Task.Run(() =>
                {
                    List<WemPlaybackInstance> wems = null;
                    try
                    {
                        wems = WwiseManager.GetPlaybackInstances(this, captureInfo);
                    }
                    catch
                    {
                        wems = new List<WemPlaybackInstance>();
                    }
                    var instances = new List<SoundInstance>();
                    foreach (var w in wems)
                    {
                        var inst = new SoundInstanceWwise(w);
                        inst.PlayInfo = info;
                        instances.Add(inst);
                    }
                    return instances;
                });
            }
            else if (EngineType is EngineTypes.FMOD)
            {
                newSlot.BackgroundInitializeTask = Task.Run(() =>
                {
                    var fmodUpdater = ParentDocument.Fmod.PlaySE(captureInfo.SoundType, captureInfo.SoundID, captureInfo.NightfallLifetime);
                    if (fmodUpdater != null)
                    {
                        var inst = new SoundInstanceFmod(fmodUpdater);
                        inst.PlayInfo = info;
                        return new List<SoundInstance>() { inst };
                    }

                    return new List<SoundInstance>() { };
                });
            }
            else if (EngineType is EngineTypes.MagicOrchestra)
            {
                newSlot.BackgroundInitializeTask = Task.Run(() =>
                {
                    List<MOPlaybackInstance> soundFileInsts = null;
                    try
                    {
                        soundFileInsts = MagicOrchestraManager.GetPlaybackInstances(this, captureInfo);
                    }
                    catch
                    {
                        soundFileInsts = new List<MOPlaybackInstance>();
                    }
                    var instances = new List<SoundInstance>();
                    foreach (var sfInst in soundFileInsts)
                    {
                        var inst = new SoundInstanceMO(sfInst);
                        inst.PlayInfo = info;
                        instances.Add(inst);
                    }
                    return instances;
                });
            }

            return newSlot;
        }

        private void AddEventBoxSlot(DSAProj.Action evBox, SoundPlayInfo info, bool isLock)
        {
            if (SOUND_DISABLED)
                return;

            if (isLock)
            {
                lock (_LOCK)
                {
                    var newSlot = CreateSlot_NoLock(info);
                    actionBoxAutoSlots.Add(evBox, newSlot);
                }
            }
            else
            {
                var newSlot = CreateSlot_NoLock(info);
                actionBoxAutoSlots.Add(evBox, newSlot);
            }
        }

        private void AddSlot(int slot, SoundPlayInfo info, bool isLock)
        {
            if (SOUND_DISABLED)
                return;

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

        public void PurgeLoadedAssets()
        {
            lock (_LOCK)
            {
                foreach (var kvp in slots)
                    kvp.Value.KillImmediately();
                slots.Clear();
                foreach (var s in oneShotAutoPlaySlots)
                    s.KillImmediately();
                oneShotAutoPlaySlots.Clear();
                foreach (var kvp in actionBoxAutoSlots)
                    kvp.Value.KillImmediately();
                actionBoxAutoSlots.Clear();
            }
            if (EngineType is EngineTypes.Wwise)
                WwiseManager.PurgeLoadedAssets(this);
            else if (EngineType is EngineTypes.FMOD)
            {
                if (NewFmodManager.CurrentFmodIns == ParentDocument.Fmod)
                    NewFmodManager.SwitchToFmodIns(null);
            }
            else if (EngineType is EngineTypes.MagicOrchestra)
                MagicOrchestraManager.PurgeLoadedAssets(this);
        }

        public void DisposeAll()
        {
            PurgeLoadedAssets();
            if (EngineType is EngineTypes.Wwise)
            {
                WwiseManager.DisposeAll();
            }
            else if (EngineType is EngineTypes.FMOD)
            {
                //FmodManager.Shutdown();
                
            }
            else if (EngineType is EngineTypes.MagicOrchestra)
            {
                MagicOrchestraManager.DisposeAll();
            }
        }

        public void StopAllSounds(bool immediate = true)
        {
            if (EngineType is EngineTypes.FMOD)
                ParentDocument.Fmod.StopAllSounds();
            else if (EngineType is EngineTypes.MagicOrchestra)
            {
                // leaving this here to remind me to check later
            }

            lock (_LOCK)
            {
                var allSlots = GetAllSoundSlotsFromAllSources();
                if (immediate)
                {
                    foreach (var s in allSlots)
                        s.KillImmediately();
                    slots.Clear();
                    actionBoxAutoSlots.Clear();
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

        private void InsideLock_UpdateSlots(float deltaTime, Matrix listener, Dictionary<int, SoundPlayInfo> requestedSoundSlots)
        {
            var deadSlots = new List<int>();
            foreach (var s in slots)
            {
                if (s.Value.State == SoundSlot.States.InitializingInBgThread)
                {
                    s.Value.Update(this, deltaTime, listener);
                    continue;
                }
                // if the slot is no longer being played
                // or if the slot has the wrong ID vs the one requested
                // AND KillSoundOnEventEnd is true
                // THEN, request sound to stop.
                if ((!requestedSoundSlots.ContainsKey(s.Key) || !requestedSoundSlots[s.Key].Matches(s.Value.PlayInfo)))
                {
                    if (s.Value.PlayInfo.KillSoundOnActionEnd)
                    {
                        s.Value.Stop();
                    }
                }
                    

                s.Value.Update(this, deltaTime, listener);
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

        private void InsideLock_UpdateOneShotSlots(float deltaTime, Matrix listener)
        {
            var deadSlots = new List<SoundSlot>();
            foreach (var s in oneShotAutoPlaySlots)
            {
                if (s.State == SoundSlot.States.InitializingInBgThread)
                {
                    s.Update(this, deltaTime, listener);
                    continue;
                }

                s.Update(this, deltaTime, listener);
                if (s.SlotCompletelyEmpty)
                    deadSlots.Add(s);
            }
            foreach (var s in deadSlots)
            {
                s.KillImmediately();
                oneShotAutoPlaySlots.Remove(s);
            }
        }

        private void InsideLock_UpdateEventBoxSlots(float deltaTime, Matrix listener,
            Dictionary<DSAProj.Action, SoundPlayInfo> requestedSoundSlots)
        {
            var deadSlots = new List<DSAProj.Action>();
            foreach (var s in actionBoxAutoSlots)
            {
                if (s.Value.State == SoundSlot.States.InitializingInBgThread)
                {
                    s.Value.Update(this, deltaTime, listener);
                    continue;
                }

                // if the slot is no longer being played
                // or if the slot has the wrong ID vs the one requested
                // Since these are event box slots, we know KillSoundOnEventEnd is true, but whatever
                if ((!requestedSoundSlots.ContainsKey(s.Key) || !requestedSoundSlots[s.Key].Matches(s.Value.PlayInfo)))
                {
                    if (s.Value.PlayInfo.KillSoundOnActionEnd)
                    {
                        s.Value.Stop();
                    }
                }
                    

                s.Value.Update(this, deltaTime, listener);
                // If slot is empty (all sound samples have finished playing)
                // Then slot is "dead", queue for deletion.
                if (s.Value.SlotCompletelyEmpty)
                    deadSlots.Add(s.Key);
            }
            foreach (var s in deadSlots)
            {
                actionBoxAutoSlots[s].KillImmediately();
                actionBoxAutoSlots.Remove(s);
            }

            foreach (var s in requestedSoundSlots)
            {
                if (!actionBoxAutoSlots.ContainsKey(s.Key))
                {
                    if (alreadyRequestedActionBoxSlots.Contains(s.Key) && !s.Value.PlayRepeatedlyWhileEventActive)
                        continue;
                    AddEventBoxSlot(s.Key, s.Value, isLock: false);
                }
            }
        }

        public void SoftWipeAllSlots()
        {
            lock (_LOCK)
            {
                var listener = GFX.CurrentWorldView.CameraLocationInWorld.WorldMatrix;
                InsideLock_UpdateSlots(0, listener, new Dictionary<int, SoundPlayInfo>());
                InsideLock_UpdateEventBoxSlots(0, listener, new Dictionary<DSAProj.Action, SoundPlayInfo>());
            }
        }

        public void NF_PlayDummyPolyFollowSoundEx_ReportUsedExInfoIDsUsedThisFrame(List<int> exInfoIDs)
        {
            List<DummyPolyFollowSoundExInfo> unusedInfos = new();
            lock (_lock_nf_Act7052_ExInfos)
            {
                foreach (var kvp in nf_Act7052_ExInfos)
                {
                    if (!exInfoIDs.Contains(kvp.Key))
                    {
                        unusedInfos.Add(kvp.Value);
                    }
                }
            }

            foreach (var ui in unusedInfos)
            {
                if (ui.ClearWhenCooldownIDNotInUse)
                {
                    foreach (var slot in ui.UsedSlots)
                    {
                        slot.Stop();
                    }
                }
            }
        }

        public void Update(float deltaTime, Matrix listener, TaeEditor.TaeEditorScreen tae)
        {
            // ??????? lmao
            //if (GFX.CurrentWorldView.RootMotionFollow_EnableWrap)
            //{
            //    listener *= Matrix.CreateTranslation(GFX.CurrentWorldView.RootMotionFollow_Translation * new Vector3(1,1,-1));
            //}

            if (EngineType is EngineTypes.FMOD)
                ParentDocument.Fmod.Update();
            else if (EngineType is EngineTypes.MagicOrchestra)
            {
                // leaving this here as a note
            }

            //if (SOUND_DISABLED)
            //{
            //    DisposeAll();
            //    return;
            //}

            if (!Main.Config.SimEnabled_Sounds)
            {
                StopAllSounds();
                return;
            }

            lock (_lock_nf_Act7052_ExInfos)
            {
                List<int> nf_Act7052_DeadExInfos = new List<int>();
                foreach (var key in nf_Act7052_ExInfos.Keys)
                {
                    nf_Act7052_ExInfos[key].Cooldown -= deltaTime;

                    List<SoundSlot> deadSlots = new List<SoundSlot>();
                    lock (_LOCK)
                    {
                        foreach (var slot in nf_Act7052_ExInfos[key].UsedSlots)
                        {

                            if (!(slots.ContainsValue(slot) || oneShotAutoPlaySlots.Contains(slot)))
                            {
                                deadSlots.Add(slot);
                            }
                        }
                    }
                    foreach (var ds in deadSlots)
                    {
                        nf_Act7052_ExInfos[key].UsedSlots.Remove(ds);
                    }

                    if (nf_Act7052_ExInfos[key].Cooldown <= 0 && nf_Act7052_ExInfos[key].UsedSlots.Count == 0)
                        nf_Act7052_DeadExInfos.Add(key);
                }

                foreach (var k in nf_Act7052_DeadExInfos)
                    nf_Act7052_ExInfos.Remove(k);
            }

            lock (_LOCK)
            {
                Dictionary<int, SoundPlayInfo> requestedSoundSlots = new Dictionary<int, SoundPlayInfo>();
                Dictionary<DSAProj.Action, SoundPlayInfo> requestedActionBoxSlots = new Dictionary<DSAProj.Action, SoundPlayInfo>();
                SoundPlayInfo mouseClickPreviewRequest = null;

                var sim = tae?.Graph?.ViewportInteractor?.ActionSim;

                var animJustLooped = tae?.Graph?.PlaybackCursor?.JustLooped ?? false;

                if (animJustLooped || tae?.Graph?.PlaybackCursor?.JustStartedPlaying == true)
                {
                    alreadyRequestedActionBoxSlots.Clear();
                }

                if (sim != null && !SOUND_DISABLED)
                {
                    var actBoxesCopy = tae?.Graph?.GetActionListCopy_UsesLock();

                    if (actBoxesCopy != null && actBoxesCopy.Count > 0)
                    {

                        foreach (var actBox in actBoxesCopy)
                        {


                            var actTypeName = actBox.GetInternalSimTypeName();
                            if (actBox.TypeName == null ||
                                !(actTypeName.Contains("Play") && actTypeName.Contains("Sound")) ||
                                !actBox.HasInternalSimField("SlotID"))
                                continue;

                            int slot = Convert.ToInt32(actBox.ReadInternalSimField("SlotID"));
                            bool killSoundOnActionEnd = false;

                            if (actBox.HasInternalSimField("PlaybackBehaviorType"))
                            {
                                byte playbackBehaviorType = Convert.ToByte(actBox.ReadInternalSimField("PlaybackBehaviorType"));

                                killSoundOnActionEnd = playbackBehaviorType > 0;
                            }
                            else if (actBox.HasInternalSimField("PlaybackBehaviorType_AC6"))
                            {
                                byte playbackBehaviorType = Convert.ToByte(actBox.ReadInternalSimField("PlaybackBehaviorType_AC6"));

                                killSoundOnActionEnd = playbackBehaviorType > 0;
                            }

                            if (actBox == tae.NewHoverAction && tae.Input.RightClickHeld && (slot >= 0 || killSoundOnActionEnd))
                            {
                                mouseClickPreviewRequest = sim.GetSoundPlayInfoOfBox(actBox, isForOneShot: false);
                                continue;
                            }

                            if (!actBox.IsActive)
                                continue;

                            bool isBoxSoundActive = (actBox.NewSimulationActive /*&& !actBox.NewSimulationEnter*/) && (tae.Graph.PlaybackCursor.IsPlaying || tae.Graph.PlaybackCursor.Scrubbing);
                            if (!isBoxSoundActive)
                                continue;



                            if (!(slot >= 0 || killSoundOnActionEnd))
                                continue;

                            var playInfo = sim.GetSoundPlayInfoOfBox(actBox, isForOneShot: false);

                            if (EngineType is EngineTypes.FMOD or EngineTypes.MagicOrchestra)
                            {
                                killSoundOnActionEnd = playInfo.KillSoundOnActionEnd = slot >= 0;
                            }

                            if (slot >= 0)
                            {
                                if (!requestedSoundSlots.ContainsKey(slot))
                                    requestedSoundSlots.Add(slot, playInfo);
                            }
                            else if (killSoundOnActionEnd)
                            {
                                if (!requestedActionBoxSlots.ContainsKey(actBox))
                                    requestedActionBoxSlots.Add(actBox, playInfo);
                            }

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

                var requestedActionBoxSlotsFreed = new List<DSAProj.Action>();
                foreach (var s in alreadyRequestedActionBoxSlots)
                {
                    if (!requestedActionBoxSlots.ContainsKey(s))
                        requestedActionBoxSlotsFreed.Add(s);
                }
                foreach (var s in requestedActionBoxSlotsFreed)
                    alreadyRequestedActionBoxSlots.Remove(s);

                InsideLock_UpdateSlots(deltaTime, listener, requestedSoundSlots);
                InsideLock_UpdateOneShotSlots(deltaTime, listener);
                InsideLock_UpdateEventBoxSlots(deltaTime, listener, requestedActionBoxSlots);

                foreach (var s in requestedSoundSlots)
                {
                    if (!alreadyRequestedSlots.Contains(s.Key))
                        alreadyRequestedSlots.Add(s.Key);
                }

                foreach (var s in requestedActionBoxSlots)
                {
                    if (!alreadyRequestedActionBoxSlots.Contains(s.Key))
                        alreadyRequestedActionBoxSlots.Add(s.Key);
                }

                if (mouseClickPreviewSoundSlot != null)
                {
                    if (!mouseClickPreviewSoundSlot.PlayInfo.Matches(mouseClickPreviewRequest))
                        mouseClickPreviewSoundSlot.Stop();
                    mouseClickPreviewSoundSlot.Update(this, deltaTime, listener);
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
