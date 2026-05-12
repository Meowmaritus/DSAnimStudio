using DSAnimStudio.ImguiOSD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NAudio.Wave;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Audio.Wwise;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private object _lock_NeedsReload = new object();
        public bool _needsReload = true;
        public void RequestReload()
        {
            lock (_lock_NeedsReload)
            {
                _needsReload = true;
            }
        }

        private bool CheckNeedsReloadAndClearFlag()
        {
            bool result = false;
            lock (_lock_NeedsReload)
            {
                if (_needsReload)
                {
                    result = true;
                    _needsReload = false;
                }
                else
                {
                    result = false;
                }
            }
            return result;
        }

        private object _lock_loadedBankData = new object();
        private Dictionary<uint, Func<IWwiseObject>> LoadedReadObjectFuncs = new Dictionary<uint, Func<IWwiseObject>>();

        private Dictionary<uint, Func<byte[]>> LoadedReadWemFuncs = new Dictionary<uint, Func<byte[]>>();

        public IWwiseObject GetWwiseObject(uint id)
        {
            IWwiseObject result = null;
            lock (_lock_loadedBankData)
            {
                if (LoadedReadObjectFuncs.ContainsKey(id))
                    result = LoadedReadObjectFuncs[id].Invoke();
            }
            return result;
        }

        public Func<byte[]> GetReadWemFunc(uint id)
        {
            Func<byte[]> result = null;
            lock (_lock_loadedBankData)
            {
                if (LoadedReadWemFuncs.ContainsKey(id))
                    result = LoadedReadWemFuncs[id];
            }
            return result;
        }

        public void LoadDataFromBanks(zzz_SoundManagerIns soundMan)
        {
            lock (_lock_loadedBankData)
            {
                List<string> bankNames = soundMan.GetAdditionalSoundBankNames();

                LoadedReadObjectFuncs.Clear();
                LoadedReadWemFuncs.Clear();

                foreach (var bnkName in bankNames)
                {
                    var bank = GetBank(bnkName);
                    if (bank != null)
                    {
                        foreach (var kvp in bank.HIRC.ObjectInfos)
                        {
                            if (LoadedReadObjectFuncs.ContainsKey(kvp.Key))
                            {
                                // TODO: Warn for duplicate functions
                            }

                            var captureKey = kvp.Key;
                            var captureBank = bank;

                            LoadedReadObjectFuncs[kvp.Key] = () => captureBank.HIRC.LoadObjectDynamic(captureKey);
                        }


                        if (bank.DIDX != null)
                        {

                            foreach (var f in bank.DIDX.Files)
                            {

                                var didxItem = f.Value;

                                var captureBankData = bank.DATA;
                                var captureItemStart = didxItem.DataSectionStart;
                                var captureItemLength = didxItem.DataSectionNumBytes;

                                var readWemBytesAct = () => captureBankData.GetSection(captureItemStart, captureItemLength);
                                LoadedReadWemFuncs[f.Key] = readWemBytesAct;
                            }
                        }



                    }
                }
            }
        }













        public bool SwitchPropHandlersNeedInit = true;
        private object _lock_wwiseSwitchPropDict = new object();
        private Dictionary<uint, uint> wwiseSwitchPropDict = new Dictionary<uint, uint>();

        private List<WwiseSwitchPropHandler> switchPropHandlers = new List<WwiseSwitchPropHandler>();
        private Dictionary<string, WwiseSwitchPropHandler> switchPropsByName = new Dictionary<string, WwiseSwitchPropHandler>();

        public void Update()
        {
            lock (_lock_wwiseSwitchPropDict)
            {
                foreach (var handler in switchPropHandlers)
                {
                    wwiseSwitchPropDict[handler.SwitchPropKeyHash] = handler.GetSwitchPropValueHash(this);
                }
            }
        }

        public void BuildSwitchPropImGui(ref bool anyItemFocused)
        {
            lock (_lock_wwiseSwitchPropDict)
            {
                foreach (var handler in switchPropHandlers)
                {
                    bool focused = false;
                    handler.BuildImguiIfApplicable(this, ref focused);
                    if (focused)
                        anyItemFocused = true;
                }
            }
        }

        public void INNER_InitWwiseSwitchPropHandlersIfNeeded()
        {
            if (SwitchPropHandlersNeedInit)
            {
                
                switchPropHandlers.Clear();
                switchPropsByName.Clear();

                void AddParam(string switchParamName, string switchPropKey, int defaultParamRowID,
                WwiseSwitchPropHandler.Param.AutomationTypes automationType = WwiseSwitchPropHandler.Param.AutomationTypes.None, 
                bool enableAutomationByDefault = true)
                {
                    var newSwitchProp = new WwiseSwitchPropHandler.Param(switchParamName, switchPropKey, defaultParamRowID,
                        automationType, enableAutomationByDefault);
                    switchPropHandlers.Add(newSwitchProp);
                    switchPropsByName[switchPropKey] = newSwitchProp;
                }

                void AddText(string switchPropKey, string initValue, WwiseSwitchPropHandler.ManualInput.AutomationTypes automationType,
                     bool enableAutomationByDefault = true)
                {
                    var newSwitchProp = new WwiseSwitchPropHandler.ManualInput(switchPropKey, initValue,
                        automationType, enableAutomationByDefault);
                    switchPropHandlers.Add(newSwitchProp);
                    switchPropsByName[switchPropKey] = newSwitchProp;
                }

                void AddList(string switchPropKey, Dictionary<string, string> entries, string initValue, WwiseSwitchPropHandler.ListSelector.AutomationTypes automationType,
                     bool enableAutomationByDefault = true)
                {
                    var newSwitchProp = new WwiseSwitchPropHandler.ListSelector(switchPropKey, entries, initValue,
                        automationType, enableAutomationByDefault);
                    switchPropHandlers.Add(newSwitchProp);
                    switchPropsByName[switchPropKey] = newSwitchProp;
                }

                switch (ParentDocument.GameRoot.GameType)
                {
                    case SoulsGames.ER:

                        AddParam("WwiseValueToStrParam_Switch_PlayerEquipmentTops", "PlayerEquipmentTops", 0, 
                            WwiseSwitchPropHandler.Param.AutomationTypes.PlayerEquipmentTops);

                        AddParam("WwiseValueToStrParam_Switch_PlayerEquipmentBottoms", "PlayerEquipmentBottoms", 0, 
                            WwiseSwitchPropHandler.Param.AutomationTypes.PlayerEquipmentBottoms);

                        AddParam("WwiseValueToStrParam_Switch_DeffensiveMaterial", "DeffensiveMaterial", 0, 
                            WwiseSwitchPropHandler.Param.AutomationTypes.DeffensiveMaterial);

                        AddParam("WwiseValueToStrParam_Switch_PlayerVoiceType", "PlayerVoice", 0,
                            WwiseSwitchPropHandler.Param.AutomationTypes.None);

                        break;
                    case SoulsGames.ERNR:

                        AddParam("WwiseValueToStrParam_Switch_PlayerEquipmentTops", "PlayerEquipmentTops", 0, 
                            WwiseSwitchPropHandler.Param.AutomationTypes.PlayerEquipmentTops);

                        AddParam("WwiseValueToStrParam_Switch_PlayerEquipmentBottoms", "PlayerEquipmentBottoms", 0, 
                            WwiseSwitchPropHandler.Param.AutomationTypes.PlayerEquipmentBottoms);

                        AddParam("WwiseValueToStrParam_Switch_DeffensiveMaterial", "DeffensiveMaterial", 0,
                            WwiseSwitchPropHandler.Param.AutomationTypes.DeffensiveMaterial);

                        AddParam("WwiseValueToStrParam_Switch_PlayerVoiceType", "PlayerVoice", 71,
                            WwiseSwitchPropHandler.Param.AutomationTypes.None);

                        AddParam("WwiseValueToStrParam_Switch_HeroId", "HeroId", 1,
                            WwiseSwitchPropHandler.Param.AutomationTypes.None);

                        break;
                    case SoulsGames.AC6:

                        AddList("leg_type", new Dictionary<string, string>
                        {
                            { "None", "no" },
                            { "Bipedal", "two_legs" },
                            { "Reverse-Joint", "reverse_legs" },
                            { "Tetrapod", "four_legs" },
                            { "Tank", "crawler_track" },
                        }, "two_legs", WwiseSwitchPropHandler.ListSelector.AutomationTypes.AC6_Legs_Type);

                        AddText("leg_id", "lg000000000", WwiseSwitchPropHandler.ManualInput.AutomationTypes.AC6_Legs_ID);

                        break;
                    default:
                        return;
                }






                // Add any WwiseValueToStringParams that didn't get added manually


                const string wwiseSwitchParamPrefix = "WwiseValueToStrParam_Switch_";
                int wwiseSwitchParamPrefixLength = wwiseSwitchParamPrefix.Length;

                Dictionary<string, WwiseSwitchPropHandler.Param> paramSwitchPropHandlers = new Dictionary<string, WwiseSwitchPropHandler.Param>();
                var allParamNames = ParentDocument.ParamManager.GetAllParamNames();
                var wwiseSwitchParamNames_Long = allParamNames
                    .Where(x => x.StartsWith(wwiseSwitchParamPrefix))
                    .ToList();

                var wwiseSwitchParamNames_Short = wwiseSwitchParamNames_Long
                    .Select(x => x.Substring(wwiseSwitchParamPrefixLength))
                    .ToList();

                for (int i = 0; i < wwiseSwitchParamNames_Short.Count; i++)
                {
                    if (!switchPropHandlers.Any(x => (x is WwiseSwitchPropHandler.Param) && (x as WwiseSwitchPropHandler.Param).ParamName.ToLower() == wwiseSwitchParamNames_Long[i].ToLower()))
                    {
                        var newSwitchProp = new WwiseSwitchPropHandler.Param(wwiseSwitchParamNames_Long[i], wwiseSwitchParamNames_Short[i], 0, WwiseSwitchPropHandler.Param.AutomationTypes.None, false);
                        switchPropHandlers.Add(newSwitchProp);
                        switchPropsByName[wwiseSwitchParamNames_Short[i]] = newSwitchProp;
                    }
                }

                foreach (var handler in switchPropHandlers)
                {
                    wwiseSwitchPropDict[handler.SwitchPropKeyHash] = handler.GetSwitchPropValueHash(this);
                }


                SwitchPropHandlersNeedInit = false;
            }
            
        }



       


        




        public static bool DEBUG_DUMP_ALL_WEM = false;
        public static bool DEBUG_KEEP_CONVERT_TEMP_FILES = false;

        public Random RAND = new Random();

        

        

        public void InitDefaultSwitchGroups()
        {
            SwitchPropHandlersNeedInit = true;
            lock (_lock_wwiseSwitchPropDict)
            {
                INNER_InitWwiseSwitchPropHandlersIfNeeded();
            }
        }

        public Dictionary<string, string> GetSwitchGroupValues()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            lock (_lock_wwiseSwitchPropDict)
            {
                foreach (var h in switchPropHandlers)
                {
                    result[h.SwitchPropKey] = h.Config_GetValue(this);
                }

            }
            return result;
        }

        public Dictionary<string, bool> GetSwitchGroupAutoEnabled()
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            lock (_lock_wwiseSwitchPropDict)
            {
                foreach (var h in switchPropHandlers)
                {
                    result[h.SwitchPropKey] = h.Config_GetAutoEnabled();
                }
            }
            return result;
        }

        public void SetSwitchGroupValues(Dictionary<string, string> switchGroupValues)
        {
            lock (_lock_wwiseSwitchPropDict)
            {
                foreach (var h in switchPropHandlers)
                {
                    if (switchGroupValues.ContainsKey(h.SwitchPropKey))
                    {
                        h.Config_SetValue(this, switchGroupValues[h.SwitchPropKey]);
                    }
                }
            }
        }

        public void SetSwitchGroupAutoEnabled(Dictionary<string, bool> switchGroupAutoEnabled)
        {
            lock (_lock_wwiseSwitchPropDict)
            {
                foreach (var h in switchPropHandlers)
                {
                    if (switchGroupAutoEnabled.ContainsKey(h.SwitchPropKey))
                    {
                        h.Config_SetAutoEnabled(switchGroupAutoEnabled[h.SwitchPropKey]);
                    }
                }
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
                        if (wem != null && wem.WavBytes != null)
                        {
                            info.ByteCount += wem.WavBytes.Length;
                            info.SoundFileCount++;
                        }
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
                lock (_lock_wemCache)
                {
                    vorbisConvertCache.Clear();
                }
                lock (_lock_GetBank)
                {
                    foreach (var kvp in loadedBanks)
                        kvp.Value.Dispose();
                    loadedBanks.Clear();
                }
                lock (_lock_loadedBankData)
                {
                    LoadedReadObjectFuncs.Clear();
                    LoadedReadWemFuncs.Clear();
                }

                RequestReload();
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
                        try
                        {
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
                                if (Main.Debug.DumpWEMs)
                                {
                                    string wemDumpDir = $"{ParentDocument.GameData.InterrootPath}\\sd\\_dsas_wem_dump";
                                    if (!Directory.Exists(wemDumpDir))
                                        Directory.CreateDirectory(wemDumpDir);

                                    string wemDumpPath = $"{wemDumpDir}\\{wemID}.wem";
                                    
                                    File.WriteAllBytes(wemDumpPath, wemBytes);
                                }

                                WEMSharp.WEMFile wemConvert = new WEMSharp.WEMFile(wemStream, WEMSharp.WEMForcePacketFormat.NoForcePacketFormat);
                                //oggBytes = wemConvert.GenerateOGG($"{Main.Directory}\\Res\\codebooks.bin", false, false);
                                loadedWem.LoopEnabled = wemConvert.LoopEnabled != 0;
                                loadedWem.LoopStart = wemConvert.LoopStart;
                                loadedWem.LoopEnd = wemConvert.LoopEnd;
                                loadedWem.TotalSampleCount = wemConvert.SampleCount;
                            }

                            //loadedOgg.FixedOggBytes = Wwise.FixOggWithRevorb(oggBytes);
                            loadedWem.WavBytes = ConvertWEMtoWAV(wemBytes);
                        }
                        catch (Exception ex)
                        {
                            zzz_NotificationManagerIns.PushNotificationWarn($"Failed to convert {wemID}.wem\n\n{ex.Message}\n\n{ex.StackTrace}");
                        }
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
        //private Dictionary<int, string> ArmorMaterialIdToNameMap = new Dictionary<int, string>();

        public string[] DefensiveMaterialNames = null;
        public int[] DefensiveMaterialIDs_ForPlaySoundByFloor = null;
        public int DefensiveMaterialIndex = 1;

        //public string[] ArmorMaterialNames = new string[]
        //{
        //    "Nude",
        //    "Cloth",
        //    "ChainMail",
        //    "LeatherArmour",
        //    "Armour",
        //};

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
            }
        }

        //public string GetDefensiveMaterialName()
        //{
        //    InitNamesAndIDs();

        //    if (DefensiveMaterialIndex >= 0 && DefensiveMaterialIndex < DefensiveMaterialNames.Length)
        //        return DefensiveMaterialNames[DefensiveMaterialIndex];
        //    else
        //        return "None";
        //}

        public int GetDefensiveMaterialParamID()
        {
            InitNamesAndIDs();

            if (DefensiveMaterialIndex >= 0 && DefensiveMaterialIndex < DefensiveMaterialIDs_ForPlaySoundByFloor.Length)
                return DefensiveMaterialIDs_ForPlaySoundByFloor[DefensiveMaterialIndex];
            else
                return 0;
        }

        //public string GetArmorMaterialName_Top()
        //{
        //    InitNamesAndIDs();

        //    if (ArmorMaterialIdToNameMap.ContainsKey(ArmorMaterial_Top))
        //        return ArmorMaterialIdToNameMap[ArmorMaterial_Top];
        //    else
        //        return "None";
        //}

        //public string GetArmorMaterialName_Bottom()
        //{
        //    InitNamesAndIDs();

        //    if (ArmorMaterialIdToNameMap.ContainsKey(ArmorMaterial_Bottom))
        //        return ArmorMaterialIdToNameMap[ArmorMaterial_Bottom];
        //    else
        //        return "None";
        //}



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

        private WwiseBNK GetBank_AddBeforeCurrentIfNew(string currentBankName, string bankName)
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
                            ParentDocument.SoundManager.RegisterAdditionalSoundBankBeforeOther(bankName, currentBankName);
                        }
                    }

                    if (result == null && ParentDocument.GameData.WwiseSoundFileExists($"/enus/{bankName}.bnk"))
                    {
                        var bytes = ParentDocument.GameData.ReadWwiseSoundFile($"/enus/{bankName}.bnk");
                        if (bytes != null)
                        {
                            result = WwiseBNK.Read(bytes);
                            loadedBanks.Add(bankName, result);
                            ParentDocument.SoundManager.RegisterAdditionalSoundBankBeforeOther(bankName, currentBankName);
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
                bankNames.Reverse();
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
            if (CheckNeedsReloadAndClearFlag())
            {
                LoadDataFromBanks(soundMan);
            }

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

            //string bankFoundPlayActionIn = null;

            float paramsVolume = MathF.Pow(10, -12f / 20f);
            float paramsPitchInSemitones = 1;

            bool foundPlayEvent = false;
            bool foundStopEvent = false;

            //bool? paramsLoop = null;
            //int? paramsLoopStart = null;
            //int? paramsLoopEnd = null;

            float fadeOutDelay = 0;
            float fadeOutDuration = 0;

            float fadeInDelay = 0;
            float fadeInDuration = 0;

            bool NewTryFindStopEvent()
            {
                bool result = false;

                //lock (bank.ThreadLockObject)
                //{

                //}
                var stopEventNameHash = soundMan.Hash("Stop" + sound.Substring(4));
                var stopEventObj = GetWwiseObject(stopEventNameHash);
                if (stopEventObj is WwiseObject.CAkEvent asEvent)
                {
                    foreach (var act in asEvent.Actions)
                    {
                        var actionObj = GetWwiseObject(act);
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

            bool old__tryFindStopEvent(WwiseBNK bank)
            {
                bool result = false;

                if (bank.HIRC == null)
                {
                    return result;
                }

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
                const int PROP_LOOP = 0x3A;
                const int PROP_LOOP_START = 0x22;
                const int PROP_LOOP_END = 0x23;
                if (props != null)
                {
                    var propVolumeIndex = props.PropTypes.IndexOf(PROP_VOLUME);
                    var propPitchIndex = props.PropTypes.IndexOf(PROP_PITCH);
                    //var propLoopIndex = props.PropTypes.IndexOf(PROP_LOOP);
                    //var propLoopStartIndex = props.PropTypes.IndexOf(PROP_LOOP_START);
                    //var propLoopEndIndex = props.PropTypes.IndexOf(PROP_LOOP_END);
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
                    //if (propLoopIndex >= 0)
                    //{
                    //    paramsLoop = true;
                    //}
                    //if (propLoopStartIndex >= 0)
                    //{
                    //    paramsLoopStart = props.PropValues[propLoopStartIndex].ValueAsInt;
                    //}
                    //if (propLoopEndIndex >= 0)
                    //{
                    //    paramsLoopEnd = props.PropValues[propLoopEndIndex].ValueAsInt;
                    //}

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


            bool NewDoObject(uint objectID)
            {
                var obj = GetWwiseObject(objectID);

                if (obj == null)
                {
                    return false;
                }

                if (obj is WwiseObject.CAkEvent asEvent)
                {
                    var firstActionID = asEvent.Actions.FirstOrDefault();
                    return NewDoObject(firstActionID);
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
                    return NewDoObject(playItemID);
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

                        var initReadWemFunc = GetReadWemFunc(asSound.WemFileID);

                        if (initReadWemFunc != null)
                        {
                            readWemBytesAct = initReadWemFunc;



                            //return true;
                        }
                        else if (ParentDocument.GameData.StreamedWEMExists(captureWemFileID))
                        {
                            foundPlayEvent = true;
                            var captureWemData = ParentDocument.GameData.ReadStreamedWEM(captureWemFileID);
                            readWemBytesAct = () => captureWemData;
                        }

                        if (readWemBytesAct != null)
                        {

                            foundPlayEvent = true;
                            var instanceGetTask = Task.Run(() =>
                            {
                                var wem = LoadWEM(captureWemFileID, readWemBytesAct);

                                return new WemPlaybackInstance(captureWemFileID, wem, captureParamsVolume, MathF.Pow(2, captureParamsPitchInSemitones / 12),
                                GFX.CurrentWorldView.CameraLocationInWorld.WorldMatrix, 20, captureLoop, captureFadeOutDuration, captureFadeOutDelay,
                                captureFadeInDuration, captureFadeInDelay);
                            });

                            instancesSpawnedByThisSound.Add(instanceGetTask);
                            return true;
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
                        foreach (var switchGroupHandler in wwiseSwitchPropDict)
                        {
                            if (asSwitchCntr.GroupID == switchGroupHandler.Key)
                            {
                                switchID = switchGroupHandler.Value;
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
                                if (NewDoObject(n))
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
                        NewDoObject(c);
                    }
                    return true;
                }

                else if (obj is WwiseObject.CAkAction asAction)
                {

                    //bankFoundPlayActionIn = bnkName;

                    if (NewTryFindStopEvent())
                        foundStopEvent = true;

                    if (asAction.ActionType == WwiseObject.CAkAction.ActionTypes.Play && asAction.ActionArgs is WwiseObject.CAkAction.ActArgs_Play asActArgs_Play)
                    {
                        int propIndex_TransitionDelay = asAction.Props.PropTypes.IndexOf((byte)WwiseEnums.PropTypes.InitialDelay);
                        if (propIndex_TransitionDelay >= 0)
                            fadeInDelay = (float)asAction.Props.PropValues[propIndex_TransitionDelay].ValueAsInt / 1000f;
                        int propIndex_TransitionTime = asAction.Props.PropTypes.IndexOf((byte)WwiseEnums.PropTypes.TransitionTime);
                        if (propIndex_TransitionTime >= 0)
                            fadeInDuration = (float)asAction.Props.PropValues[propIndex_TransitionTime].ValueAsInt / 1000f;

                        uint bankID = asActArgs_Play.BankID; // todo: check if this is needed at all
                    }

                    NewDoObject(asAction.RefID);


                }

                else if (obj is WwiseObject.CAkDialogueEvent asDialogueEvent)
                {
                    bool innerResult = false;
                    lock (_lock_wwiseSwitchPropDict)
                    {
                        INNER_InitWwiseSwitchPropHandlersIfNeeded();

                        bool StepIntoCheck(int currentNestLevel, WwiseObject.CAkDialogueEvent.Node node)
                        {
                            if (node.Target is WwiseObject.CAkDialogueEvent.NodeTargetEnd asEndNode)
                            {
                                return NewDoObject(asEndNode.AudioNodeID);
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
                                        if (child.IsPassCheck(wwiseSwitchPropDict) && child.PassesProbabilityCheck(RAND))
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
                                        if (child.IsPassCheck(wwiseSwitchPropDict) && child.PassesProbabilityCheck(RAND))
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
                            innerResult = true;
                    }
                    return innerResult;
                    //Console.WriteLine("test");
                }

                else
                {
                    throw new NotImplementedException();
                    //return false;
                }

                return false;
            }




















            bool old__DoObjectInBank(WwiseBNK bnk, uint objectID, string bnkName, List<string> alreadyCheckedBanks)
            {
                //lock (bnk.ThreadLockObject)
                //{


                //    return false;
                //}

                if (bnk.HIRC == null)
                {
                    zzz_NotificationManagerIns.PushNotificationWarn($"Wwise bank '{bnkName}'s HIRC is null.");
                    return false;
                }

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
                                var refBank = GetBank_AddBeforeCurrentIfNew(bnkName, referencedBankName);
                                if (refBank != null)
                                {
                                    var wasInOtherBank = old__DoObjectInBank(refBank, objectID, referencedBankName, alreadyCheckedBanks);
                                    if (wasInOtherBank)
                                        return true;
                                }
                            }

                            if (!alreadyCheckedBanks.Contains("cs_main"))
                            {
                                var mainBank = GetBank_AddBeforeCurrentIfNew(bnkName, "cs_main");
                                var wasInMainBank = old__DoObjectInBank(mainBank, objectID, "cs_main", alreadyCheckedBanks);
                                if (wasInMainBank)
                                    return true;
                            }
                        }
                        return false;
                    }

                    if (obj is WwiseObject.CAkEvent asEvent)
                    {
                        var firstActionID = asEvent.Actions.FirstOrDefault();
                        return old__DoObjectInBank(bnk, firstActionID, bnkName, new List<string>());
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
                        return old__DoObjectInBank(bnk, playItemID, bnkName, new List<string>());
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

                            var initReadWemFunc = GetReadWemFunc(asSound.WemFileID);

                            if (initReadWemFunc != null)
                            {
                                readWemBytesAct = initReadWemFunc;



                                //return true;
                            }
                            else if (ParentDocument.GameData.StreamedWEMExists(captureWemFileID))
                            {
                                foundPlayEvent = true;
                                var captureWemData = ParentDocument.GameData.ReadStreamedWEM(captureWemFileID);
                                readWemBytesAct = () => captureWemData;
                            }

                            if (readWemBytesAct != null)
                            {

                                foundPlayEvent = true;
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
                            foreach (var switchGroupHandler in wwiseSwitchPropDict)
                            {
                                if (asSwitchCntr.GroupID == switchGroupHandler.Key)
                                {
                                    switchID = switchGroupHandler.Value;
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
                                    if (old__DoObjectInBank(bnk, n, bnkName, new List<string>()))
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
                            old__DoObjectInBank(bnk, c, bnkName, new List<string>());
                        }
                        return true;
                    }

                    else if (obj is WwiseObject.CAkAction asAction)
                    {
                        var selectedBank = bnk;
                        string selectedBankName = null;

                        //bankFoundPlayActionIn = bnkName;
                        
                        if (old__tryFindStopEvent(bnk))
                            foundStopEvent = true;

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
                                        zzz_NotificationManagerIns.PushNotificationWarn($"Could not find Wwise bank name matching hash '{bankID}'.");
                                }
                                else
                                {
                                    var referencedBank = GetBank_AddBeforeCurrentIfNew(bnkName, selectedBankName);
                                    if (referencedBank != null)
                                    {
                                        selectedBank = referencedBank;
                                    }
                                    else
                                    {
                                        if (Main.Config.Wwise_ShowMissingBankWarnings)
                                            zzz_NotificationManagerIns.PushNotificationWarn($"Wwise sound bank '{selectedBankName}' not found.");
                                    }
                                }


                            }
                        }

                        var actionInTargetBank = old__DoObjectInBank(selectedBank, asAction.RefID, selectedBankName, new List<string>());

                        if (foundPlayEvent)
                        {
                            return actionInTargetBank;
                        }
                        else
                        {
                            return old__DoObjectInBank(bnk, asAction.RefID, selectedBankName, new List<string>());
                        }

                            
                    }

                    else if (obj is WwiseObject.CAkDialogueEvent asDialogueEvent)
                    {
                        //var wwiseSwitchProps = new Dictionary<uint, uint>();
                        //if (soundMan.ParentDocument.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
                        //{
                        //    wwiseSwitchProps[soundMan.Hash("PlayerEquipmentTops")] = soundMan.Hash(GetArmorMaterialName_Top());
                        //    wwiseSwitchProps[soundMan.Hash("PlayerEquipmentBottoms")] = soundMan.Hash(GetArmorMaterialName_Bottom());
                        //    wwiseSwitchProps[soundMan.Hash("DeffensiveMaterial")] = soundMan.Hash(GetDefensiveMaterialName());
                        //}
                        //else if (soundMan.ParentDocument.GameRoot.GameType is SoulsGames.AC6)
                        //{
                        //    //wwiseSwitchProps[soundMan.Hash("PlayerEquipmentTops")] = soundMan.Hash(GetArmorMaterialName_Top());
                        //    //wwiseSwitchProps[soundMan.Hash("PlayerEquipmentBottoms")] = soundMan.Hash(GetArmorMaterialName_Bottom());
                        //    //wwiseSwitchProps[soundMan.Hash("DeffensiveMaterial")] = soundMan.Hash(GetDefensiveMaterialName());
                        //    //wwiseSwitchProps[soundMan.Hash("AttackPowerType")] = soundMan.Hash("pow_s");
                        //    //wwiseSwitchProps[soundMan.Hash("BGMStatus")] = soundMan.Hash("normal");
                        //    //wwiseSwitchProps[soundMan.Hash("BulletGenerateType")] = soundMan.Hash("hit");
                        //    //wwiseSwitchProps[soundMan.Hash("DestroyedSeType")] = soundMan.Hash("No");
                        //    //wwiseSwitchProps[soundMan.Hash("GeneratorType")] = soundMan.Hash("normal");
                        //    //wwiseSwitchProps[soundMan.Hash("Material")] = soundMan.Hash("stone_pavement");
                        //    //wwiseSwitchProps[soundMan.Hash("ShootSeCategory")] = soundMan.Hash("Handgun");
                        //    //wwiseSwitchProps[soundMan.Hash("Weapon_Type")] = soundMan.Hash("hand_gun");
                        //    //wwiseSwitchProps[soundMan.Hash("WeaponAtkAttribute")] = soundMan.Hash("slash");
                        //    //wwiseSwitchProps[soundMan.Hash("WeaponMaterial")] = soundMan.Hash("iron");


                        //    //wwiseSwitchProps[soundMan.Hash("leg_id")] = 0;
                        //    uint leg_type = 0;
                        //    uint leg_id = 0;
                        //    ParentDocument.Scene.AccessMainModel(m =>
                        //    {
                        //        m.ChrAsm.AccessArmorSlot(NewChrAsm.EquipSlotTypes.Legs, lg =>
                        //        {
                        //            var legType = lg.EquipParam.AC6LegTypeModelID;

                        //            leg_id = soundMan.Hash($"lg{((ushort)lg.EquipParam.AC6LegTypeSEID).ToString("000000000")}");

                        //            switch (legType)
                        //            {
                        //                case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Bipedal:
                        //                    leg_type = soundMan.Hash("two_legs");
                        //                    break;
                        //                case ParamData.EquipParamProtector.AC6LegTypeModelIDs.ReverseJoint:
                        //                    leg_type = soundMan.Hash("reverse_legs");
                        //                    break;
                        //                case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tetrapod:
                        //                    leg_type = soundMan.Hash("four_legs");
                        //                    break;
                        //                case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tank1:
                        //                case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tank2:
                        //                case ParamData.EquipParamProtector.AC6LegTypeModelIDs.Tank3:
                        //                    leg_type = soundMan.Hash("crawler_track");
                        //                    break;
                        //            }
                        //        });
                        //    });

                        //    wwiseSwitchProps[soundMan.Hash("leg_id")] = leg_id;
                        //    wwiseSwitchProps[soundMan.Hash("leg_type")] = leg_type;
                        //    wwiseSwitchProps[soundMan.Hash("charactor_type")] = soundMan.Hash("player");
                        //}
                        bool result = false;
                        lock (_lock_wwiseSwitchPropDict)
                        {
                            INNER_InitWwiseSwitchPropHandlersIfNeeded();

                            bool StepIntoCheck(int currentNestLevel, WwiseObject.CAkDialogueEvent.Node node)
                            {
                                if (node.Target is WwiseObject.CAkDialogueEvent.NodeTargetEnd asEndNode)
                                {
                                    return (old__DoObjectInBank(bnk, asEndNode.AudioNodeID, bnkName, new List<string>()));
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
                                            if (child.IsPassCheck(wwiseSwitchPropDict) && child.PassesProbabilityCheck(RAND))
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
                                            if (child.IsPassCheck(wwiseSwitchPropDict) && child.PassesProbabilityCheck(RAND))
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
                                result = true;
                        }
                        return result;
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

            //bool success = false;

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




            // New method

            if (NewTryFindStopEvent())
                foundStopEvent = true;
            int countBeforeCheck = instancesSpawnedByThisSound.Count;
            NewDoObject(eventNameHash);

            // New sounds found pog
            if (instancesSpawnedByThisSound.Count > countBeforeCheck)
            {
                foundPlayEvent = true;
            }





            //List<string> checkedBanks = new List<string>();
            //foreach (var bn in wwiseBankNames)
            //{
            //    if (checkedBanks.Contains(bn))
            //        continue;
            //    checkedBanks.Add(bn);
            //    var bnk = GetBank(bn);
            //    if (bnk == null)
            //        continue;
            //    if (tryFindStopEvent(bnk))
            //        foundStopEvent = true;
            //    if (!foundPlayEvent)
            //    {
            //        int countBeforeCheck = instancesSpawnedByThisSound.Count;
            //        if (DoObjectInBank(bnk, eventNameHash, bn, new List<string>()))
            //        {

            //        }
            //        // New sounds found pog
            //        if (instancesSpawnedByThisSound.Count > countBeforeCheck)
            //        {
            //            foundPlayEvent = true;
            //            //break;
            //            //if (bankFoundPlayActionIn != null)
            //            //{
            //            //    var bankPlayEventWasIn = GetBank(bankFoundPlayActionIn);
            //            //    if (tryFindStopEvent(bankPlayEventWasIn))
            //            //        foundStopEvent = true;
            //            //}
            //        }
            //    }

            //    if ((foundPlayEvent && foundStopEvent))
            //        break;
            //}

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
                int defMatID = 0;

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
