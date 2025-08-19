using Microsoft.Xna.Framework;
using NAudio.Wave;
using SoulsAssetPipeline.Audio.Wwise;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using DSAnimStudio.ImguiOSD;

namespace DSAnimStudio
{
    public class zzz_WwiseManagerInst
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_SoundManagerIns SoundManager;
        public zzz_WwiseManagerInst(zzz_DocumentIns doc, zzz_SoundManagerIns soundMan)
        {
            ParentDocument = doc;
            SoundManager = soundMan;
            InitDefaultSwitchGroups();
        }
       
        




        




        public static bool DEBUG_DUMP_ALL_WEM = false;
        public static bool DEBUG_KEEP_CONVERT_TEMP_FILES = false;

        public Random RAND = new Random();

        private object _lock_SwitchGroups = new object();

        public List<SwitchGroupHandler> SwitchGroupHandlers = new List<SwitchGroupHandler>();

        

        public void InitDefaultSwitchGroups()
        {
            lock (_lock_SwitchGroups)
            {
                SwitchGroupHandlers = new List<SwitchGroupHandler>()
                {
                    new SwitchGroupHandler("PlayerVoice", "_00_PC_Male_Young_Neutral", new List<string>
                    {
                        "_00_PC_Male_Young_Neutral",
                        "_01_PC_Male_Young_Deeper",
                        "_02_PC_Male_Mature_Neutral",
                        "_03_PC_Male_Mature_Deeper",
                        "_04_PC_Male_Aged_Neutral",
                        "_05_PC_Male_Aged_Deeper",
                        "_10_PC_Female_Young_Neutral",
                        "_11_PC_Female_Young_Deeper",
                        "_12_PC_Female_Mature_Neutral",
                        "_13_PC_Female_Mature_Deeper",
                        "_14_PC_Female_Aged_Neutral",
                        "_15_PC_Female_Aged_Deeper",
                        "_50_NPC_Male_Young",
                        "_51_NPC_Male_Middle",
                        "_52_NPC_Male_Old",
                        "_60_NPC_Female_Young",
                        "_61_NPC_Female_Middle",
                        "_62_NPC_Female_Old",
                        "NPC_vc300",
                        "NPC_vc301",
                        "NPC_vc302",
                        "NPC_vc303",
                        "NPC_vc304",
                        "NPC_vc305",
                        "NPC_vc306",
                        "NPC_vc307",
                        "NPC_vc308",
                        "NPC_vc309",
                        "NPC_vc310",
                        "NPC_vc311",
                        "NPC_vc312",
                        "NPC_vc313",
                        "NPC_vc314",
                        "NPC_vc316",
                        "NPC_vc318",
                        "NPC_vc319",
                        "NPC_vc320",
                        "NPC_vc321",
                        "NPC_vc322",
                        "NPC_vc323",
                        "NPC_vc324",
                        "NPC_vc325",
                        "NPC_vc326",
                        "NPC_vc331",
                        "NPC_vc332",
                        "NPC_vc333",
                        "NPC_vc334",
                        "NPC_vc335",
                        "NPC_vc337",
                        "NPC_vc341",
                        "NPC_vc348",
                        "NPC_vc349",
                        "NPC_vc351",
                        "NPC_vc352",
                        "NPC_vc400",
                        "NPC_vc401",
                    }),
                    new SwitchGroupHandler("GrassHitType", "NotHit", new List<string>
                    {
                        "NotHit",
                        "HitNormal"
                    })
                };
            }
        }

        public Dictionary<string, string> GetSwitchGroupValues()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            lock (_lock_SwitchGroups)
            {
                foreach (var sgh in SwitchGroupHandlers)
                {
                    result.Add(sgh.GroupType, sgh.SelectedSwitch);
                }
            }
            return result;
        }

        public void SetSwitchGroupValues(Dictionary<string, string> switchGroupValues)
        {
            lock (_lock_SwitchGroups)
            {
                foreach (var sgh in SwitchGroupHandlers)
                {
                    if (switchGroupValues.ContainsKey(sgh.GroupType))
                    {
                        sgh.SelectedSwitch = switchGroupValues[sgh.GroupType];
                    }
                }
            }
        }

        public class SwitchGroupHandler
        {
            public string GroupType;
            public string[] Switches;
            private int selectedIndex = 0;

            public string SelectedSwitch
            {
                get
                {
                    if (selectedIndex >= Switches.Length)
                        selectedIndex = Switches.Length - 1;
                    return selectedIndex >= 0 ? Switches[selectedIndex] : null;
                }
                set
                {
                    selectedIndex = Switches.ToList().IndexOf(value);
                }
            }

            public SwitchGroupHandler(string groupType, string defaultSwitch, List<string> switches)
            {
                GroupType = groupType;
                Switches = switches.ToArray();
                selectedIndex = switches.IndexOf(defaultSwitch);
            }

            public void DoImGui()
            {
                ImGuiNET.ImGui.ListBox($"{GroupType}###Wwise_SwitchGroup_{GroupType}", ref selectedIndex, Switches, Switches.Length);

            }
        }

        public class MemoryInfo
        {
            public long ByteCount;
            public int SoundFileCount;
            public bool AnySoundsLoading;
        }

        public MemoryInfo GetMemoryInfo()
        {
            var info = new MemoryInfo();
            lock (_lock_wemCache)
            {
                foreach (var kvp in vorbisConvertCache)
                {
                    if (kvp.Value.IsCompleted)
                    {
                        var wem = kvp.Value.Result;
                        info.ByteCount += wem.WavBytes.Length;
                        info.SoundFileCount++;
                    }
                    else
                    {
                        info.AnySoundsLoading = true;
                    }

                }
            }

            return info;
        }

        public void PurgeLoadedAssets(zzz_SoundManagerIns soundMan)
        {

            lock (soundMan._LOCK)
            {
                vorbisConvertCache.Clear();
                foreach (var kvp in loadedBanks)
                    kvp.Value.Dispose();
                loadedBanks.Clear();
            }

        }

        public class LoadedWEM
        {
            public uint WEMID;
            public byte[] FixedOggBytes;
            public byte[] WavBytes;
            public bool LoopEnabled;
            public long LoopStart;
            public long LoopEnd;
            public long TotalSampleCount;
        }

        private Dictionary<uint, Task<LoadedWEM>> vorbisConvertCache = new Dictionary<uint, Task<LoadedWEM>>();

        private object _lock_wemCache = new object();

        public LoadedWEM LoadWEM(uint wemID, Func<byte[]> getWemBytesIfNotLoaded)
        {
            Task<LoadedWEM> wemLoadTask = null;

            lock (_lock_wemCache)
            {
                if (!vorbisConvertCache.ContainsKey(wemID))
                {
                    var loadTask = Task.Run(() =>
                    {
                        LoadedWEM loadedWem = null;
                        if (loadedWem != null)
                        {
                            return loadedWem;
                        }

                        loadedWem = new LoadedWEM();
                        loadedWem.WEMID = wemID;

                        //byte[] oggBytes = null;
                        byte[] wemBytes = getWemBytesIfNotLoaded.Invoke();
                        using (var wemStream = new MemoryStream(wemBytes))
                        {
                            WEMSharp.WEMFile wemConvert = new WEMSharp.WEMFile(wemStream, WEMSharp.WEMForcePacketFormat.NoForcePacketFormat);
                            //oggBytes = wemConvert.GenerateOGG($"{Main.Directory}\\Res\\codebooks.bin", false, false);
                            loadedWem.LoopEnabled = wemConvert.LoopEnabled != 0;
                            loadedWem.LoopStart = wemConvert.LoopStart;
                            loadedWem.LoopEnd = wemConvert.LoopEnd;
                            loadedWem.TotalSampleCount = wemConvert.SampleCount;
                        }

                        //loadedOgg.FixedOggBytes = Wwise.FixOggWithRevorb(oggBytes);
                        loadedWem.WavBytes = ConvertWEMtoWAV(wemBytes);

                        return loadedWem;
                    });



                    vorbisConvertCache.Add(wemID, loadTask);
                }

                wemLoadTask = vorbisConvertCache[wemID];
            }

            wemLoadTask.Wait();
            return wemLoadTask.Result;
        }




        public volatile byte[] BLANK_WAV_BYTES = {
            0x52, 0x49, 0x46, 0x46, 0x30, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20,
            0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x44, 0xAC, 0x00, 0x00, 0x10, 0xB1, 0x02, 0x00,
            0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xFE, 0xFF
        };

        public byte[] ConvertWEMtoWAV(byte[] wem)
        {
            var guid = Guid.NewGuid().ToString();
            var exePath = $@"{Main.Directory}\Res\vgmstream\vgmstream_cmd.exe";
            var temp = $@"{Main.Directory}\Temp\WEM";
            //var tid = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            var pathIn = $@"{temp}\{guid}_in.wem";
            var pathOut = $@"{temp}\{guid}_out.wav";

            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);

            File.WriteAllBytes(pathIn, wem);

            var procStart = new ProcessStartInfo(exePath, $"-o \"{pathOut}\" \"{pathIn}\"");

            procStart.CreateNoWindow = true;
            procStart.WindowStyle = ProcessWindowStyle.Hidden;
            procStart.UseShellExecute = false;
            procStart.WorkingDirectory = temp;

            byte[] result = null;

            try
            {
                using (var proc = Process.Start(procStart))
                {
                    proc.WaitForExit();

                    result = File.ReadAllBytes(pathOut);
                }
            }
            catch
            {
                Main.REQUEST_DISABLE_SOUND = true;
                System.Windows.Forms.MessageBox.Show("Unable to decode audio with vgmstream. Make sure you or your antivirus have" +
                    " not deleted the '/Res/vgmstream/vgmstream_cmd.exe' file (re-extract the .zip to get another copy if needed)." +
                    "\n\nDisabling sound simulation now.\n" +
                    "After fixing the issue, you may re-enable it from the 'Sound' tab.", "",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

            }
            finally
            {
                if (!DEBUG_KEEP_CONVERT_TEMP_FILES)
                {
                    if (File.Exists(pathIn))
                        File.Delete(pathIn);
                    if (File.Exists(pathOut))
                        File.Delete(pathOut);
                }
            }





            return result ?? BLANK_WAV_BYTES;
        }

        public byte[] FixOggWithRevorb(byte[] ogg)
        {
            var exePath = $@"{Main.Directory}\Res\WEM\ReVorb.exe";
            var temp = $@"{Main.Directory}\Temp\WEM";
            var tid = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            var pathIn = $@"{temp}\{tid}_in.ogg";
            var pathOut = $@"{temp}\{tid}_out.ogg";

            if (!Directory.Exists(temp))
                Directory.CreateDirectory(temp);

            File.WriteAllBytes(pathIn, ogg);

            var procStart = new ProcessStartInfo(exePath, $"\"{pathIn}\" \"{pathOut}\"");

            procStart.CreateNoWindow = true;
            procStart.WindowStyle = ProcessWindowStyle.Hidden;
            procStart.UseShellExecute = false;
            procStart.WorkingDirectory = temp;

            byte[] result = null;

            using (var proc = Process.Start(procStart))
            {
                proc.WaitForExit();

                result = File.ReadAllBytes(pathOut);
            }

            return result;
        }


        private Dictionary<string, WwiseBNK> loadedBanks = new Dictionary<string, WwiseBNK>();

        //private object _lock = new object();

        public int ArmorMaterial_Top = 0;
        public int ArmorMaterial_Bottom = 0;
        private Dictionary<int, string> ArmorMaterialIdToNameMap = new Dictionary<int, string>();

        public string[] DefensiveMaterialNames = null;
        public int[] DefensiveMaterialIDs_ForPlaySoundByFloor = null;
        public int DefensiveMaterialIndex = 1;

        public string[] ArmorMaterialNames = new string[]
        {
            "Nude",
            "Cloth",
            "ChainMail",
            "LeatherArmour",
            "Armour",
        };

        string GetNameOfHitMaterialParam(int materialID)
        {
            if (ParentDocument.ParamManager.HitMtrlParamEntries.ContainsKey(materialID) &&
                !string.IsNullOrWhiteSpace(ParentDocument.ParamManager.HitMtrlParamEntries[materialID]))
            {
                return ParentDocument.ParamManager.HitMtrlParamEntries[materialID];
            }
            else if (ParentDocument.ParamManager.WwiseValueToStrParam_Switch_DeffensiveMaterial.ContainsKey(materialID) &&
                !string.IsNullOrWhiteSpace(ParentDocument.ParamManager.WwiseValueToStrParam_Switch_DeffensiveMaterial[materialID].Name))
            {
                return ParentDocument.ParamManager.WwiseValueToStrParam_Switch_DeffensiveMaterial[materialID].Name;
            }
            else
            {
                return $"Material{materialID}";
            }
        }

        public void InitNamesAndIDs()
        {
            if (DefensiveMaterialNames == null || DefensiveMaterialNames.Length == 0 || DefensiveMaterialIDs_ForPlaySoundByFloor == null)
            {
                DefensiveMaterialNames = ParentDocument.ParamManager.WwiseValueToStrParam_Switch_DeffensiveMaterial
                    .Select(x => $"[{x.Key}] {GetNameOfHitMaterialParam((int)x.Key)} (\"{x.Value.WwiseString}\")")
                    .ToArray();
                DefensiveMaterialIDs_ForPlaySoundByFloor = ParentDocument.ParamManager.WwiseValueToStrParam_Switch_DeffensiveMaterial
                    .Select(x => (int)x.Key).ToArray();
                ArmorMaterialIdToNameMap = ParentDocument.ParamManager.WwiseValueToStrParam_Switch_DeffensiveMaterial.ToDictionary(p => (int)p.Key, p => p.Value.WwiseString);
            }
        }



        public string GetDefensiveMaterialName()
        {
            InitNamesAndIDs();

            if (DefensiveMaterialIndex >= 0 && DefensiveMaterialIndex < DefensiveMaterialNames.Length)
                return DefensiveMaterialNames[DefensiveMaterialIndex];
            else
                return "None";
        }

        public int GetDefensiveMaterialParamID()
        {
            InitNamesAndIDs();

            if (DefensiveMaterialIndex >= 0 && DefensiveMaterialIndex < DefensiveMaterialNames.Length)
                return DefensiveMaterialIDs_ForPlaySoundByFloor[DefensiveMaterialIndex];
            else
                return 0;
        }

        public string GetArmorMaterialName_Top()
        {
            InitNamesAndIDs();

            if (ArmorMaterialIdToNameMap.ContainsKey(ArmorMaterial_Top))
                return ArmorMaterialIdToNameMap[ArmorMaterial_Top];
            else
                return "None";
        }

        public string GetArmorMaterialName_Bottom()
        {
            InitNamesAndIDs();

            if (ArmorMaterialIdToNameMap.ContainsKey(ArmorMaterial_Bottom))
                return ArmorMaterialIdToNameMap[ArmorMaterial_Bottom];
            else
                return "None";
        }

        private object _lock_GetBank = new object();
        private WwiseBNK GetBank(string bankName)
        {
            WwiseBNK result = null;

            lock (_lock_GetBank)
            {
                if (loadedBanks.ContainsKey(bankName))
                {
                    result = loadedBanks[bankName];
                }
                else
                {
                    if (ParentDocument.GameData.WwiseSoundFileExists($"{bankName}.bnk"))
                    {
                        var bytes = ParentDocument.GameData.ReadWwiseSoundFile($"{bankName}.bnk");
                        if (bytes != null)
                        {
                            result = WwiseBNK.Read(bytes);
                            loadedBanks.Add(bankName, result);
                            ParentDocument.SoundManager.RegisterAdditionalSoundBank(bankName);
                        }
                    }

                    if (result == null && ParentDocument.GameData.WwiseSoundFileExists($"/enus/{bankName}.bnk"))
                    {
                        var bytes = ParentDocument.GameData.ReadWwiseSoundFile($"/enus/{bankName}.bnk");
                        if (bytes != null)
                        {
                            result = WwiseBNK.Read(bytes);
                            loadedBanks.Add(bankName, result);
                            ParentDocument.SoundManager.RegisterAdditionalSoundBank(bankName);
                        }
                    }
                }
            }





            return result;
        }

        public List<WemPlaybackInstance> GetPlaybackInstances(zzz_SoundManagerIns soundMan, SoundPlayInfo info)
        {
            string sound = GetSoundName(info.SoundType, info.SoundID);
            if (sound != null)
            {
                List<string> bankNames = soundMan.GetAdditionalSoundBankNames();
                return GetPlaybackInstances(soundMan, bankNames, sound);
            }
            else
            {
                return new List<WemPlaybackInstance>();
            }
        }

        public List<WemPlaybackInstance> GetPlaybackInstances(zzz_SoundManagerIns soundMan, List<string> wwiseBankNames,
            string sound)
        {
            //if (soundMan.NeedsWwiseRefresh)
            //{
            //    soundMan.NeedsWwiseRefresh = false;
            //    var soundBankNames = soundMan.GetAdditionalSoundBankNames();
            //    Wwise.PurgeLoadedAssets(soundMan);
            //    Wwise.AddLookupBanks(soundBankNames);
            //}

            List<Task<WemPlaybackInstance>> instancesSpawnedByThisSound = new List<Task<WemPlaybackInstance>>();

            //var wwiseBank1 = GetBank(wwiseBankName1);
            //var wwiseBank2 = GetBank(wwiseBankName2);
            var eventNameHash = soundMan.Hash(sound);

            float paramsVolume = MathF.Pow(10, -12f / 20f);
            float paramsPitchInSemitones = 1;

            float fadeOutDelay = 0;
            float fadeOutDuration = 0;

            float fadeInDelay = 0;
            float fadeInDuration = 0;

            bool tryFindStopEvent(WwiseBNK bank)
            {
                bool result = false;
                //lock (bank.ThreadLockObject)
                //{

                //}
                var stopEventNameHash = soundMan.Hash("Stop" + sound.Substring(4));
                var stopEventObj = bank.HIRC.LoadObjectDynamic(stopEventNameHash);
                if (stopEventObj is WwiseObject.CAkEvent asEvent)
                {
                    foreach (var act in asEvent.Actions)
                    {
                        var actionObj = bank.HIRC.LoadObjectDynamic(act);
                        if (actionObj is WwiseObject.CAkAction asAction)
                        {
                            if (asAction.ActionType is WwiseObject.CAkAction.ActionTypes.Stop_E or WwiseObject.CAkAction.ActionTypes.Stop_E_O)
                            {
                                int propIndex_DelayTime = asAction.Props.PropTypes.IndexOf((byte)WwiseEnums.PropTypes.DelayTime);
                                int propIndex_TransitionTime = asAction.Props.PropTypes.IndexOf((byte)WwiseEnums.PropTypes.TransitionTime);

                                if (propIndex_DelayTime >= 0)
                                    fadeOutDelay = asAction.Props.PropValues[propIndex_DelayTime].ValueAsInt / 1000f;

                                if (propIndex_TransitionTime >= 0)
                                    fadeOutDuration = asAction.Props.PropValues[propIndex_TransitionTime].ValueAsInt / 1000f;

                                result = true;
                                break;
                            }
                        }
                    }
                }

                return result;
            }




            void DoProps(WwiseObject.PropertyBundle props, WwiseObject.RangedPropertyBundle rangedProps)
            {
                const int PROP_VOLUME = 0;
                const int PROP_PITCH = 2;
                if (props != null)
                {
                    var propVolumeIndex = props.PropTypes.IndexOf(PROP_VOLUME);
                    var propPitchIndex = props.PropTypes.IndexOf(PROP_PITCH);
                    if (propVolumeIndex >= 0)
                    {
                        var volumeDb = props.PropValues[propVolumeIndex].ValueAsFloat;
                        paramsVolume *= MathF.Pow(10, volumeDb / 20f);
                    }
                    if (propPitchIndex >= 0)
                    {
                        var pitchCents = props.PropValues[propPitchIndex].ValueAsFloat;
                        float semitones = pitchCents / 100;
                        paramsPitchInSemitones += semitones;
                    }
                }


                if (rangedProps != null)
                {
                    var propRangedVolumeIndex = rangedProps.PropTypes.IndexOf(PROP_VOLUME);
                    var propRangedPitchIndex = rangedProps.PropTypes.IndexOf(PROP_PITCH);
                    if (propRangedVolumeIndex >= 0)
                    {
                        var volumeMin = rangedProps.PropValues[propRangedVolumeIndex].Min.ValueAsFloat;
                        var volumeMax = rangedProps.PropValues[propRangedVolumeIndex].Max.ValueAsFloat;
                        var rand = new Random();
                        var volumeDb = (float)(volumeMin + ((volumeMax - volumeMin) * rand.NextDouble()));
                        paramsVolume *= MathF.Pow(10, volumeDb / 20f);
                    }
                    if (propRangedPitchIndex >= 0)
                    {
                        var pitchMin = rangedProps.PropValues[propRangedPitchIndex].Min.ValueAsFloat;
                        var pitchMax = rangedProps.PropValues[propRangedPitchIndex].Max.ValueAsFloat;
                        var rand = new Random();
                        float pitchCents = (float)(pitchMin + ((pitchMax - pitchMin) * rand.NextDouble()));
                        float semitones = pitchCents / 100;
                        paramsPitchInSemitones += semitones;
                    }
                }
            }

            bool DoObjectInBank(WwiseBNK bnk, uint objectID, string bnkName, List<string> alreadyCheckedBanks)
            {
                //lock (bnk.ThreadLockObject)
                //{


                //    return false;
                //}

                if (objectID > 0)
                {
                    if (!alreadyCheckedBanks.Contains(bnkName))
                        alreadyCheckedBanks.Add(bnkName);

                    var obj = bnk.HIRC.LoadObjectDynamic(objectID);

                    if (obj == null)
                    {
                        // If it's not in the main bank it's not anywhere lol.
                        //if (isMainBank)
                        //    return false;

                        if (bnk.STID != null)
                        {
                            foreach (var referencedBankName in bnk.STID.BankFileNames.Values)
                            {
                                if (alreadyCheckedBanks.Contains(referencedBankName))
                                    continue;
                                if (referencedBankName == "cs_main")
                                    continue;
                                var refBank = GetBank(referencedBankName);
                                if (refBank != null)
                                {
                                    var wasInOtherBank = DoObjectInBank(refBank, objectID, referencedBankName, alreadyCheckedBanks);
                                    if (wasInOtherBank)
                                        return true;
                                }
                            }

                            if (!alreadyCheckedBanks.Contains("cs_main"))
                            {
                                var mainBank = GetBank("cs_main");
                                var wasInMainBank = DoObjectInBank(mainBank, objectID, "cs_main", alreadyCheckedBanks);
                                if (wasInMainBank)
                                    return true;
                            }
                        }
                        return false;
                    }

                    if (obj is WwiseObject.CAkEvent asEvent)
                    {
                        var firstActionID = asEvent.Actions.FirstOrDefault();
                        return DoObjectInBank(bnk, firstActionID, bnkName, new List<string>());
                    }

                    else if (obj is WwiseObject.CAkRanSeqCntr asRanSeqCntr)
                    {
                        DoProps(asRanSeqCntr.Params.Props, asRanSeqCntr.Params.RangedProps);

                        // CrossFadePower
                        if (asRanSeqCntr.TransitionMode == 2)
                        {
                            float transitionMod = (float)(asRanSeqCntr.TransitionTimeModMin + (RAND.NextDouble() * (asRanSeqCntr.TransitionTimeModMax - asRanSeqCntr.TransitionTimeModMin)));
                            float transition = asRanSeqCntr.TransitionTime + transitionMod;
                            fadeInDuration = transition / 1000f;
                        }
                        // Delay
                        else if (asRanSeqCntr.TransitionMode == 3)
                        {
                            float transitionMod = (float)(asRanSeqCntr.TransitionTimeModMin + (RAND.NextDouble() * (asRanSeqCntr.TransitionTimeModMax - asRanSeqCntr.TransitionTimeModMin)));
                            float transition = asRanSeqCntr.TransitionTime + transitionMod;
                            fadeInDelay = transition / 1000f;
                        }

                        var playItemID = asRanSeqCntr.RollRandomPlayListItem(RAND);
                        return DoObjectInBank(bnk, playItemID, bnkName, new List<string>());
                    }



                    else if (obj is SoulsAssetPipeline.Audio.Wwise.WwiseObject.CAkSound asSound)
                    {
                        // VORBIS
                        if (asSound.PluginID == 0x00040001)
                        {
                            DoProps(asSound.Params.Props, asSound.Params.RangedProps);

                            bool loop = asSound.Params.Props.PropTypes.Contains((byte)WwiseEnums.PropTypes.Loop);

                            var captureParamsVolume = paramsVolume;
                            var captureParamsPitchInSemitones = paramsPitchInSemitones;
                            var captureLoop = loop;
                            var captureFadeInDelay = fadeInDelay;
                            var captureFadeInDuration = fadeInDuration;
                            var captureFadeOutDelay = fadeOutDelay;
                            var captureFadeOutDuration = fadeOutDuration;
                            uint captureWemFileID = asSound.WemFileID;

                            Func<byte[]> readWemBytesAct = null;

                            if (bnk.DIDX.Files.ContainsKey(asSound.WemFileID))
                            {
                                var didxItem = bnk.DIDX.Files[captureWemFileID];

                                var captureBankData = bnk.DATA;
                                var captureItemStart = didxItem.DataSectionStart;
                                var captureItemLength = didxItem.DataSectionNumBytes;

                                readWemBytesAct = () => captureBankData.GetSection(captureItemStart, captureItemLength);



                                //return true;
                            }
                            else if (ParentDocument.GameData.StreamedWEMExists(captureWemFileID))
                            {
                                var captureWemData = ParentDocument.GameData.ReadStreamedWEM(captureWemFileID);
                                readWemBytesAct = () => captureWemData;
                            }

                            if (readWemBytesAct != null)
                            {
                                var instanceGetTask = Task.Run(() =>
                                {
                                    var wem = LoadWEM(captureWemFileID, readWemBytesAct);

                                    return new WemPlaybackInstance(captureWemFileID, wem, captureParamsVolume, MathF.Pow(2, captureParamsPitchInSemitones / 12),
                                    GFX.CurrentWorldView.CameraLocationInWorld.WorldMatrix, 20, captureLoop, captureFadeOutDuration, captureFadeOutDelay,
                                    captureFadeInDuration, captureFadeInDelay);
                                });

                                instancesSpawnedByThisSound.Add(instanceGetTask);
                            }

                        }
                        // Wwise Silence
                        else if (asSound.PluginID == 0x650002)
                        {
                            return true;
                        }

                    }


                    else if (obj is WwiseObject.CAkSwitchCntr asSwitchCntr)
                    {
                        DoProps(asSwitchCntr.Params.Props, asSwitchCntr.Params.RangedProps);

                        bool validSwitchFound = false;
                        uint switchID = 0;
                        if (asSwitchCntr.GroupType == 0)
                        {
                            foreach (var switchGroupHandler in SwitchGroupHandlers)
                            {
                                if (asSwitchCntr.GroupID == soundMan.Hash(switchGroupHandler.GroupType))
                                {
                                    switchID = soundMan.Hash(switchGroupHandler.SelectedSwitch);
                                    validSwitchFound = true;
                                    break;
                                }
                            }

                            //if (asSwitchCntr.GroupID == FnvHash("PlayerVoice"))
                            //{
                            //    if (PlayerVoiceIndex >= PlayerVoiceTypes.Length)
                            //        PlayerVoiceIndex = PlayerVoiceTypes.Length - 1;
                            //    if (PlayerVoiceIndex < 0)
                            //        PlayerVoiceIndex = 0;
                            //    switchID = FnvHash(PlayerVoiceTypes[PlayerVoiceIndex]);
                            //    validSwitchFound = true;
                            //    dfgdfgedfg
                            //}
                        }

                        if (!validSwitchFound)
                            switchID = asSwitchCntr.DefaultSwitch;

                        bool any = false;
                        foreach (var sw in asSwitchCntr.SwitchGroups)
                        {

                            if (sw.SwitchID == switchID)
                            {
                                foreach (var n in sw.NodeObjIDs)
                                {
                                    if (DoObjectInBank(bnk, n, bnkName, new List<string>()))
                                    {
                                        any = true;
                                    }
                                }
                                break;
                            }

                        }
                        return any;

                        //if (validSwitchFound)
                        //{
                        //    bool any = false;
                        //    foreach (var sw in asSwitchCntr.SwitchGroups)
                        //    {

                        //        if (sw.SwitchID == switchID)
                        //        {
                        //            foreach (var n in sw.NodeObjIDs)
                        //            {
                        //                if (DoObjectInBank(bnk, n, bnkName, new List<string>()))
                        //                {
                        //                    any = true;
                        //                }
                        //            }
                        //        }

                        //    }
                        //    return any;
                        //}
                        //else
                        //{
                        //    var defaultChildID = asSwitchCntr.Children.FirstOrDefault();
                        //    return DoObjectInBank(bnk, defaultChildID, bnkName, new List<string>());
                        //}

                    }

                    else if (obj is WwiseObject.CAkLayerCntr asLayerCntr)
                    {
                        DoProps(asLayerCntr.Params.Props, asLayerCntr.Params.RangedProps);
                        foreach (var c in asLayerCntr.Children)
                        {
                            DoObjectInBank(bnk, c, bnkName, new List<string>());
                        }
                        return true;
                    }

                    else if (obj is WwiseObject.CAkAction asAction)
                    {
                        var selectedBank = bnk;
                        string selectedBankName = null;

                        if (asAction.ActionType == WwiseObject.CAkAction.ActionTypes.Play && asAction.ActionArgs is WwiseObject.CAkAction.ActArgs_Play asActArgs_Play)
                        {
                            int propIndex_TransitionDelay = asAction.Props.PropTypes.IndexOf((byte)WwiseEnums.PropTypes.InitialDelay);
                            if (propIndex_TransitionDelay >= 0)
                                fadeInDelay = (float)asAction.Props.PropValues[propIndex_TransitionDelay].ValueAsInt / 1000f;
                            int propIndex_TransitionTime = asAction.Props.PropTypes.IndexOf((byte)WwiseEnums.PropTypes.TransitionTime);
                            if (propIndex_TransitionTime >= 0)
                                fadeInDuration = (float)asAction.Props.PropValues[propIndex_TransitionTime].ValueAsInt / 1000f;

                            uint bankID = asActArgs_Play.BankID;

                            if (bankID == bnk.BKHD.SoundBankID)
                            {
                                selectedBank = bnk;
                                selectedBankName = bnkName;
                            }
                            else
                            {
                                if (bnk.STID != null && bnk.STID.BankFileNames.ContainsKey(bankID))
                                {
                                    selectedBankName = bnk.STID.BankFileNames[bankID];
                                }
                                else
                                {
                                    selectedBankName = soundMan.GetBankNameFromHash(bankID);
                                }
                                //else
                                //{
                                //    throw new FileNotFoundException($"Referenced bank file ID '{bankID}' not found in hash lookup table of current bank.");
                                //}

                                if (selectedBankName == null)
                                {
                                    if (Main.Config.Wwise_ShowMissingBankWarnings)
                                        zzz_NotificationManagerIns.PushNotification($"Could not find Wwise bank name matching hash '{bankID}'.");
                                }
                                else
                                {
                                    var referencedBank = GetBank(selectedBankName);
                                    if (referencedBank != null)
                                    {
                                        selectedBank = referencedBank;
                                    }
                                    else
                                    {
                                        if (Main.Config.Wwise_ShowMissingBankWarnings)
                                            zzz_NotificationManagerIns.PushNotification($"Wwise sound bank '{selectedBankName}' not found.");
                                    }
                                }


                            }
                        }

                        return DoObjectInBank(selectedBank, asAction.RefID, selectedBankName, new List<string>());
                    }

                    else if (obj is WwiseObject.CAkDialogueEvent asDialogueEvent)
                    {
                        var wwiseSwitchProps = new Dictionary<uint, uint>();
                        wwiseSwitchProps[soundMan.Hash("PlayerEquipmentTops")] = soundMan.Hash(GetArmorMaterialName_Top());
                        wwiseSwitchProps[soundMan.Hash("PlayerEquipmentBottoms")] = soundMan.Hash(GetArmorMaterialName_Bottom());
                        wwiseSwitchProps[soundMan.Hash("DeffensiveMaterial")] = soundMan.Hash(GetDefensiveMaterialName());

                        bool StepIntoCheck(int currentNestLevel, WwiseObject.CAkDialogueEvent.Node node)
                        {
                            if (node.Target is WwiseObject.CAkDialogueEvent.NodeTargetEnd asEndNode)
                            {
                                return (DoObjectInBank(bnk, asEndNode.AudioNodeID, bnkName, new List<string>()));
                            }
                            else if (node.Target is WwiseObject.CAkDialogueEvent.NodeTargetChildren asParent)
                            {
                                // Best Match
                                if (asDialogueEvent.Mode == 0)
                                {
                                    foreach (var child in asParent.Children)
                                    {
                                        if (asParent.DefaultChild == child)
                                            continue;
                                        if (child.IsPassCheck(wwiseSwitchProps) && child.PassesProbabilityCheck(RAND))
                                        {
                                            return (StepIntoCheck(currentNestLevel + 1, child));
                                        }
                                    }
                                    // If no children pass check, do default
                                    if (asParent.DefaultChild != null)
                                    {
                                        return (StepIntoCheck(currentNestLevel + 1, asParent.DefaultChild));
                                    }
                                    else if (asParent.Children.Count > 0)
                                    {
                                        return (StepIntoCheck(currentNestLevel + 1, asParent.Children.First()));
                                    }
                                }
                                // Weighted
                                else if (asDialogueEvent.Mode == 1)
                                {
                                    var childrenToChooseFrom = new List<WwiseObject.CAkDialogueEvent.Node>();
                                    foreach (var child in asParent.Children)
                                    {
                                        if (asParent.DefaultChild == child)
                                            continue;
                                        if (child.IsPassCheck(wwiseSwitchProps) && child.PassesProbabilityCheck(RAND))
                                        {
                                            //if (StepIntoCheck(currentNestLevel + 1, child))
                                            //    return true;
                                            childrenToChooseFrom.Add(child);
                                        }
                                    }
                                    if (childrenToChooseFrom.Count == 0)
                                    {
                                        // If no children pass check, do default
                                        if (asParent.DefaultChild != null)
                                        {
                                            return (StepIntoCheck(currentNestLevel + 1, asParent.DefaultChild));
                                        }
                                        else if (asParent.Children.Count > 0)
                                        {
                                            return (StepIntoCheck(currentNestLevel + 1, asParent.Children.First()));
                                        }
                                    }

                                    int totalWeight = 0;
                                    foreach (var child in childrenToChooseFrom)
                                    {
                                        totalWeight += child.Weight;
                                    }
                                    int fate = RAND.Next(0, totalWeight);
                                    int currentWeight = 0;
                                    foreach (var child in childrenToChooseFrom)
                                    {
                                        currentWeight += child.Weight;

                                        if (fate <= currentWeight)
                                        {
                                            if (child.PassesProbabilityCheck(RAND))
                                                return (StepIntoCheck(currentNestLevel + 1, child));
                                        }
                                    }
                                }
                            }


                            return false;
                        }

                        var check = StepIntoCheck(0, asDialogueEvent.RootNode);
                        if (check)
                            return true;

                        //Console.WriteLine("test");
                    }

                    else
                    {
                        throw new NotImplementedException();
                        //return false;
                    }

                }

                return false;
            }

            //if (!tryFindStopEvent(wwiseBank1) && wwiseBankName1 != wwiseBankName2)
            //    tryFindStopEvent(wwiseBank2);

            //var cAkEvent = wwiseBank.HIRC.LoadObjectDynamic(testHash);

            bool success = false;

            //if (DoObjectInBank(wwiseBank1, eventNameHash))
            //{
            //    success = true;
            //}

            //if (wwiseBankName1 != wwiseBankName2)
            //{
            //    if (DoObjectInBank(wwiseBank2, eventNameHash))
            //    {
            //        success = true;
            //    }
            //}

            List<string> checkedBanks = new List<string>();
            foreach (var bn in wwiseBankNames)
            {
                if (checkedBanks.Contains(bn))
                    continue;
                checkedBanks.Add(bn);
                var bnk = GetBank(bn);
                if (bnk == null)
                    continue;
                tryFindStopEvent(bnk);
                int countBeforeCheck = instancesSpawnedByThisSound.Count;
                DoObjectInBank(bnk, eventNameHash, bn, new List<string>());
                // New sounds found pog
                if (instancesSpawnedByThisSound.Count > countBeforeCheck)
                {
                    success = true;
                    break;
                }
            }

            var result = new List<WemPlaybackInstance>();

            foreach (var inst in instancesSpawnedByThisSound)
            {
                inst.Wait();
                result.Add(inst.Result);
            }

            return result;
        }

        public void DisposeAll()
        {
            //lock (_lock_NAudio)
            //{
            //    foreach (var kvp in NAudioOutputs)
            //    {
            //        kvp.Value.Output.Stop();
            //        kvp.Value.Output.Dispose();
            //    }
            //    NAudioOutputs.Clear();
            //}
        }

        //private WemDef GetWemDefFromBnk(WwiseBNK bnk, uint wemID)
        //{
        //    WemDef result = null;
        //    lock (_lock)
        //    {
        //        if (!loadedWEMs.ContainsKey(wemID))
        //        {
        //            if (bnk.DIDX.Files.ContainsKey(wemID))
        //            {
        //                var didxItem = bnk.DIDX.Files[wemID];
        //                var wemFileBytes = bnk.DATA.GetSection(didxItem.DataSectionStart, didxItem.DataSectionNumBytes);

        //                var newDef = new WemDef(wemFileBytes);

        //                result = loadedWEMs[wemID];
        //            }


        //        }
        //        else
        //        {
        //            result = loadedWEMs[wemID];
        //        }
        //    }

        //    return result;
        //}


        public string GetSoundName(int category, int id)
        {
            string soundName = null;
            if (category == 0)
                soundName = $"Play_a{id:D9}";
            else if (category == 1)
                soundName = $"Play_c{id:D9}";
            else if (category == 2)
                soundName = $"Play_f{id:D9}";
            else if (category == 3)
                soundName = $"Play_o{id:D9}";
            else if (category == 4)
                soundName = $"Play_p{id:D9}";
            else if (category == 5)
                soundName = $"Play_s{id:D9}";
            else if (category == 6)
                soundName = $"Play_m{id:D9}";
            else if (category == 7)
                soundName = $"Play_v{id:D9}";

            // 8 = Floor Material Determined
            else if (category == 8)
            {
                if (DefensiveMaterialIndex >= DefensiveMaterialIDs_ForPlaySoundByFloor.Length)
                    DefensiveMaterialIndex = DefensiveMaterialIDs_ForPlaySoundByFloor.Length - 1;
                soundName = $"Play_c{(id + (DefensiveMaterialIDs_ForPlaySoundByFloor[DefensiveMaterialIndex] - 1)):D9}";
            }
            // 9 = Armor Material Determined
            else if (category == 9)
                soundName = $"Play_c{(id + (int)ArmorMaterial_Top):D9}";

            else if (category == 10)
                soundName = $"Play_g{id:D9}";

            else if (category == 15)
                soundName = $"Dialogue_d{id:D9}";

            return soundName;
        }

        

        
    }
}
