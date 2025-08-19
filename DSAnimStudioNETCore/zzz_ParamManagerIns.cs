using SoulsAssetPipeline;
using SoulsFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class zzz_ParamManagerIns
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_ParamManagerIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }

        public void Dispose()
        {
            foreach (var kvp in this.LoadedParams)
            {
                foreach (var kvp2 in kvp.Value)
                {
                    kvp2.Value.DisposeRowReader();
                }
                kvp.Value?.Clear();
            }
            LoadedParams?.Clear();
            LoadedParams = null;

            AC6NpcEquipPartsParam?.Clear();
            AC6AttachObjParam_Pc?.Clear();
            AC6AttachObjParam_Npc?.Clear();
            AC6AttackActionParam_NPC?.Clear();
            AC6AttackActionParam_PC?.Clear();
            AtkParam_Npc?.Clear();
            AtkParam_Pc?.Clear();
            BehaviorParam?.Clear();
            BehaviorParam_PC?.Clear();
            //DS2ArmorParam?.Clear();
            //DS2WeaponParam?.Clear();
            EquipParamProtector?.Clear();
            EquipParamWeapon?.Clear();
            NpcParam?.Clear();
            SeMaterialConvertParam?.Clear();
            SpEffectParam?.Clear();
            WepAbsorpPosParam?.Clear();
            WwiseValueToStrParam_Switch_DeffensiveMaterial?.Clear();

            AC6NpcEquipPartsParam = null;
            AC6AttachObjParam_Npc = null;
            AC6AttachObjParam_Pc = null;
            AC6AttackActionParam_NPC = null;
            AC6AttackActionParam_PC = null;
            AtkParam_Npc = null;
            AtkParam_Pc = null;
            BehaviorParam = null;
            BehaviorParam_PC = null;
            //DS2ArmorParam = null;
            //DS2WeaponParam = null;
            EquipParamProtector = null;
            EquipParamWeapon = null;
            NpcParam = null;
            SeMaterialConvertParam = null;
            SpEffectParam = null;
            WepAbsorpPosParam = null;
            WwiseValueToStrParam_Switch_DeffensiveMaterial = null;


            ParamBNDs = null;
            

            HitMtrlParamEntries = null;
            
            fileSystemWatcher?.Dispose();
        }




        private object _lock_Params = new object();



        public static bool AutoParamReloadEnabled;

        public bool AutoReloadQueued = false;

        public float AutoParamReloadRequestPollHz = 20.0f;
        private float paramReloadRequestPollTimer = 0;
        private object _lock_AutoReload = new object();


        private FileSystemWatcher fileSystemWatcher = null;
        public void InitAutoReload(string paramBndFilePath)
        {
            lock (_lock_AutoReload)
            {
                if (fileSystemWatcher != null)
                {
                    fileSystemWatcher.Changed -= OnParamBndChanged;
                    fileSystemWatcher.Created -= OnParamBndChanged;
                    fileSystemWatcher.Dispose();
                    fileSystemWatcher = null;
                }

                if (paramBndFilePath != null && File.Exists(paramBndFilePath))
                {
                    var dir = Path.GetDirectoryName(paramBndFilePath);
                    var fileName = Path.GetFileName(paramBndFilePath);
                    fileSystemWatcher = new FileSystemWatcher(dir);
                    fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime;
                    fileSystemWatcher.Changed += OnParamBndChanged;
                    fileSystemWatcher.Created += OnParamBndChanged;
                    fileSystemWatcher.Filter = fileName;
                    fileSystemWatcher.IncludeSubdirectories = false;
                    fileSystemWatcher.EnableRaisingEvents = true;
                }
            }





        }

        private void OnParamBndChanged(object sender, FileSystemEventArgs e)
        {
            lock (_lock_AutoReload)
            {
                if (e.ChangeType == WatcherChangeTypes.Changed)
                {
                    AutoReloadQueued = true;
                    paramReloadRequestPollTimer = 0;
                }
            }

        }

        public void UpdateAutoParamReload(float elapsedTime)
        {
            if (ParentDocument.LoadingTaskMan.AnyInteractionBlockingTasks())
                return;

            if (!AutoParamReloadEnabled)
                return;

            if (ParentDocument.GameRoot.GameType == SoulsGames.None || ParentDocument.Scene.IsEmpty)
                return;

            if (AutoReloadQueued)
            {
                lock (_lock_AutoReload)
                {
                    if (paramReloadRequestPollTimer > 0)
                        paramReloadRequestPollTimer -= elapsedTime;

                    if (paramReloadRequestPollTimer <= 0)
                    {
                        try
                        {
                            LoadParamBND(forceReload: true);
                            Main.AddForceDrawTime(1f);
                            AutoReloadQueued = false;
                            paramReloadRequestPollTimer = 0;
                        }
                        catch
                        {
                            AutoReloadQueued = true;
                            paramReloadRequestPollTimer = (1f / AutoParamReloadRequestPollHz);
                        }
                    }


                }
            }
        }

        private ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, IBinder> ParamBNDs
            = new ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, IBinder>();

        private ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, ConcurrentDictionary<string, PARAM_Hack>> LoadedParams
            = new ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, ConcurrentDictionary<string, PARAM_Hack>>();

        public ConcurrentDictionary<long, ParamData.BehaviorParam> BehaviorParam_PC
            = new ConcurrentDictionary<long, ParamData.BehaviorParam>();

        public ConcurrentDictionary<long, ParamData.BehaviorParam> BehaviorParam
            = new ConcurrentDictionary<long, ParamData.BehaviorParam>();

        public ConcurrentDictionary<long, ParamData.AtkParam> AtkParam_Pc
            = new ConcurrentDictionary<long, ParamData.AtkParam>();

        public ConcurrentDictionary<long, ParamData.AtkParam> AtkParam_Npc
            = new ConcurrentDictionary<long, ParamData.AtkParam>();

        public ConcurrentDictionary<long, ParamData.NpcParam> NpcParam
            = new ConcurrentDictionary<long, ParamData.NpcParam>();

        public ConcurrentDictionary<long, ParamData.SpEffectParam> SpEffectParam
            = new ConcurrentDictionary<long, ParamData.SpEffectParam>();

        public ConcurrentDictionary<long, ParamData.EquipParamWeapon> EquipParamWeapon
            = new ConcurrentDictionary<long, ParamData.EquipParamWeapon>();

        public ConcurrentDictionary<long, ParamData.EquipParamProtector> EquipParamProtector
            = new ConcurrentDictionary<long, ParamData.EquipParamProtector>();

        public ConcurrentDictionary<long, ParamData.WepAbsorpPosParam> WepAbsorpPosParam
           = new ConcurrentDictionary<long, ParamData.WepAbsorpPosParam>();

        //public ConcurrentDictionary<long, ParamDataDS2.WeaponParam> DS2WeaponParam
        //    = new ConcurrentDictionary<long, ParamDataDS2.WeaponParam>();

        //public ConcurrentDictionary<long, ParamDataDS2.ArmorParam> DS2ArmorParam
        //     = new ConcurrentDictionary<long, ParamDataDS2.ArmorParam>();

        public ConcurrentDictionary<long, ParamData.WwiseValueToStrParam_Switch_DeffensiveMaterial> WwiseValueToStrParam_Switch_DeffensiveMaterial
            = new ConcurrentDictionary<long, ParamData.WwiseValueToStrParam_Switch_DeffensiveMaterial>();


        public ConcurrentDictionary<long, ParamData.AC6NpcEquipPartsParam> AC6NpcEquipPartsParam
             = new ConcurrentDictionary<long, ParamData.AC6NpcEquipPartsParam>();

        public ConcurrentDictionary<long, ParamData.AC6AttachObj> AC6AttachObjParam_Pc
            = new ConcurrentDictionary<long, ParamData.AC6AttachObj>();

        public ConcurrentDictionary<long, ParamData.AC6AttachObj> AC6AttachObjParam_Npc
            = new ConcurrentDictionary<long, ParamData.AC6AttachObj>();

        public ConcurrentDictionary<long, ParamData.AC6AttackActionParam> AC6AttackActionParam_NPC
            = new ConcurrentDictionary<long, ParamData.AC6AttackActionParam>();

        public ConcurrentDictionary<long, ParamData.AC6AttackActionParam> AC6AttackActionParam_PC
            = new ConcurrentDictionary<long, ParamData.AC6AttackActionParam>();

        public ConcurrentDictionary<long, ParamData.SeMaterialConvertParam> SeMaterialConvertParam
             = new ConcurrentDictionary<long, ParamData.SeMaterialConvertParam>();


        public Dictionary<long, string> HitMtrlParamEntries = new Dictionary<long, string>();

        private SoulsAssetPipeline.SoulsGames GameTypeCurrentLoadedParamsAreFrom = SoulsAssetPipeline.SoulsGames.None;

        public PARAM_Hack GetParam(string paramName)
        {
            PARAM_Hack foundParam = null;

            lock (_lock_Params)
            {

                if (!ParamBNDs.ContainsKey(ParentDocument.GameRoot.GameType))
                    throw new InvalidOperationException("ParamBND not loaded :tremblecat:");

                if (!LoadedParams.ContainsKey(ParentDocument.GameRoot.GameType))
                    LoadedParams.AddOrUpdate(ParentDocument.GameRoot.GameType, new ConcurrentDictionary<string, PARAM_Hack>(), (k, v) => v);

                if (LoadedParams[ParentDocument.GameRoot.GameType].ContainsKey(paramName))
                {
                    return LoadedParams[ParentDocument.GameRoot.GameType][paramName];
                }
                else
                {
                    var thisGamesParambnd = ParamBNDs[ParentDocument.GameRoot.GameType];
                    if (thisGamesParambnd == null)
                        ImguiOSD.DialogManager.DialogOK("Error", "Unable to load GameParam. Make sure your project directory options such as game data directory, ModEngine /mod/ path, and Load Loose Params options are all correct for your game/mod setup.");
                    else
                    {
                        foreach (var f in ParamBNDs[ParentDocument.GameRoot.GameType].Files)
                        {
                            var paramShortName = Utils.GetShortIngameFileName(f.Name.ToUpper());
                            if (paramShortName == paramName.ToUpper())
                            {
                                var p = PARAM_Hack.Read(f.Bytes);
                                LoadedParams[ParentDocument.GameRoot.GameType].AddOrUpdate(paramName, p, (k, v) => v);
                                foundParam = p;
                                break;
                            }
                        }
                    }



                }
            }
            //if (foundParam == null)
            //    throw new InvalidOperationException($"Param '{paramName}' not found.");

            return foundParam;
        }

        private bool CheckNpcParamForCurrentGameType(int chrId, ParamData.NpcParam r, bool isFirst, bool matchCXXX0)
        {
            long checkId = r.ID;

            if (matchCXXX0)
            {
                chrId /= 10;
                checkId /= 10;
                chrId *= 10;
                checkId *= 10;
            }


            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT
                || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER
                || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR
                || ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6)
            {
                return ((checkId % 1_0000_0000) / 1_0000) == chrId;
            }
            else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES ||
                ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ||
                ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
            {
                return ((checkId % 1_0000_00) / 1_00 == chrId);
            }
            else
            {
                throw new NotImplementedException(
                    $"ParamManager.CheckNpcParamForCurrentGameType not implemented for game type {ParentDocument.GameRoot.GameType}");
            }
        }

        public List<ParamData.NpcParam> FindNpcParams(string modelName, bool matchCXXX0 = false)
        {
            int chrId = int.Parse(modelName.Substring(1, 4));

            var npcParams = new List<ParamData.NpcParam>();
            foreach (var kvp in NpcParam.Where(r
                => CheckNpcParamForCurrentGameType(chrId, r.Value, npcParams.Count == 0, matchCXXX0)))
            {
                if (!npcParams.Contains(kvp.Value))
                    npcParams.Add(kvp.Value);
            }
            npcParams = npcParams.OrderBy(x => x.ID).ToList();
            return npcParams;
        }



        private void LoadStuffFromParamBND(bool isDS2)
        {
            void AddParam<T>(ConcurrentDictionary<long, T> paramDict, string paramName)
                where T : ParamData, new()
            {
                paramDict.Clear();
                var param = GetParam(paramName);
                if (param == null)
                    return;
                foreach (var row in param.Rows)
                {
                    var rowData = new T();
                    rowData.ID = row.ID;
                    rowData.Name = row.Name;
                    try
                    {
                        rowData.Read(param.GetRowReader(row));
                        if (!paramDict.ContainsKey(row.ID))
                            paramDict.AddOrUpdate(row.ID, rowData, (k, v) => rowData);
                    }
                    catch (Exception ex) when (Main.EnableErrorHandler.ParamManager)
                    {
                        throw new Exception($"Failed to read row {row.ID} ({row.Name ?? "<No Name>"}) of param '{paramName}': {ex.ToString()}");
                    }

                }
            }

            BehaviorParam?.Clear();
            BehaviorParam_PC?.Clear();
            AtkParam_Pc?.Clear();
            AtkParam_Npc?.Clear();
            NpcParam?.Clear();
            SpEffectParam?.Clear();
            EquipParamWeapon?.Clear();
            EquipParamProtector?.Clear();
            WepAbsorpPosParam?.Clear();
            WwiseValueToStrParam_Switch_DeffensiveMaterial?.Clear();

            HitMtrlParamEntries?.Clear();

            //DS2WeaponParam?.Clear();
            //DS2ArmorParam?.Clear();

            AC6NpcEquipPartsParam?.Clear();
            AC6AttachObjParam_Pc?.Clear();
            AC6AttachObjParam_Npc?.Clear();
            AC6AttackActionParam_NPC?.Clear();
            AC6AttackActionParam_PC?.Clear();

            SeMaterialConvertParam?.Clear();

            AddParam(BehaviorParam, "BehaviorParam");
            if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                BehaviorParam_PC = new ConcurrentDictionary<long, ParamData.BehaviorParam>(BehaviorParam.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            else
                AddParam(BehaviorParam_PC, "BehaviorParam_PC");
            AddParam(AtkParam_Pc, "AtkParam_Pc");
            AddParam(AtkParam_Npc, "AtkParam_Npc");
            AddParam(NpcParam, "NpcParam");
            AddParam(EquipParamWeapon, "EquipParamWeapon");
            AddParam(EquipParamProtector, "EquipParamProtector");
            if (ParentDocument.GameRoot.GameTypeUsesWepAbsorpPosParam)
                AddParam(WepAbsorpPosParam, "WepAbsorpPosParam");
            AddParam(SpEffectParam, "SpEffectParam");
            if (ParentDocument.GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DES)
            {
                var hitMtrlParam = GetParam("HitMtrlParam");
                if (hitMtrlParam != null)
                {
                    foreach (var row in hitMtrlParam.Rows)
                    {
                        if (!HitMtrlParamEntries.ContainsKey(row.ID))
                            HitMtrlParamEntries.Add(row.ID, row.Name);
                    }
                    //HitMtrlParamEntries = HitMtrlParamEntries.OrderBy(x => x.Key).ToList();
                }

            }

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR)
            {
                AddParam(WwiseValueToStrParam_Switch_DeffensiveMaterial, "WwiseValueToStrParam_Switch_DeffensiveMaterial");
            }

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                AddParam(AC6NpcEquipPartsParam, "NpcEquipPartsParam");

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3)
                AddParam(SeMaterialConvertParam, "SeMaterialConvertParam");

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                AddParam(AC6AttachObjParam_Pc, "AttachObj_Pc");

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                AddParam(AC6AttachObjParam_Npc, "AttachObj_Npc");

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                AddParam(AC6AttackActionParam_NPC, "AttackActionParam_NPC");

            if (ParentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                AddParam(AC6AttackActionParam_PC, "AttackActionParam_PC");

            GameTypeCurrentLoadedParamsAreFrom = ParentDocument.GameRoot.GameType;
        }

        public ParamData.AtkParam GetPlayerCommonAttack(int absoluteBehaviorID)
        {
            if (!BehaviorParam_PC.ContainsKey(absoluteBehaviorID))
                return null;

            var behaviorParamEntry = BehaviorParam_PC[absoluteBehaviorID];

            if (behaviorParamEntry.RefType != 0)
                return null;

            if (!AtkParam_Pc.ContainsKey(behaviorParamEntry.RefID))
                return null;

            return AtkParam_Pc[behaviorParamEntry.RefID];
        }

        public ParamData.AtkParam GetPlayerBasicAtkParam(ParamData.EquipParamWeapon wpn, int behaviorJudgeID, bool isLeftHand)
        {
            if (wpn == null)
                return null;

            // Format: 10VVVVJJJ
            // V = BehaviorVariationID
            // J = BehaviorJudgeID
            long behaviorParamID = 10_0000_000 + (wpn.BehaviorVariationID * 1_000) + behaviorJudgeID;

            // If behavior 10VVVVJJJ doesn't exist, check for fallback behavior 10VV00JJJ.
            if (!BehaviorParam_PC.ContainsKey(behaviorParamID))
            {
                long baseBehaviorVariationID = (wpn.BehaviorVariationID / 100) * 100;

                if (baseBehaviorVariationID == wpn.BehaviorVariationID)
                {
                    // Fallback is just the same thing, which we already know doesn't exist.
                    return null;
                }

                long baseBehaviorParamID = 10_0000_000 + (baseBehaviorVariationID * 1_000) + behaviorJudgeID;

                if (BehaviorParam_PC.ContainsKey(baseBehaviorParamID))
                {
                    behaviorParamID = baseBehaviorParamID;
                }
                else
                {
                    return null;
                }
            }

            ParamData.BehaviorParam behaviorParamEntry = BehaviorParam_PC[behaviorParamID];

            // Make sure behavior is an attack behavior.
            if (behaviorParamEntry.RefType != 0)
                return null;

            // Make sure referenced attack exists.
            if (!AtkParam_Pc.ContainsKey(behaviorParamEntry.RefID))
                return null;

            return AtkParam_Pc[behaviorParamEntry.RefID];
        }

        public ParamData.AtkParam GetNpcBasicAtkParam(ParamData.NpcParam npcParam, int behaviorJudgeID)
        {
            if (npcParam == null)
                return null;

            // Format: 2VVVVVJJJ
            // V = BehaviorVariationID
            // J = BehaviorJudgeID
            long behaviorParamID = 2_00000_000 + (npcParam.BehaviorVariationID * 1_000) + behaviorJudgeID;

            if (!BehaviorParam.ContainsKey(behaviorParamID))
                return null;

            ParamData.BehaviorParam behaviorParamEntry = BehaviorParam[behaviorParamID];

            // Make sure behavior is an attack behavior.
            if (behaviorParamEntry.RefType != 0)
                return null;

            // Make sure referenced attack exists.
            if (!AtkParam_Npc.ContainsKey(behaviorParamEntry.RefID))
                return null;

            return AtkParam_Npc[behaviorParamEntry.RefID];
        }

        public string GetParambndFileName(SoulsGames game)
        {
            if (ParentDocument.GameRoot.GameTypeUsesRegulation && ParentDocument.GameData.IsLoadingRegulationParams)
            {
                if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 && ParentDocument.GameData.FileExists("/Data0.bdt", alwaysLoose: true))
                {
                    return $"/Data0.bdt";
                }
                else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER && ParentDocument.GameData.FileExists("/regulation.bin", alwaysLoose: true))
                {
                    return $"/regulation.bin";
                }
                else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR && ParentDocument.GameData.FileExists("/regulation.bin", alwaysLoose: true))
                {
                    return $"/regulation.bin";
                }
                else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6 && ParentDocument.GameData.FileExists("/regulation.bin", alwaysLoose: true))
                {
                    return $"/regulation.bin";
                }
                else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS && ParentDocument.GameData.FileExists("/enc_regulation.bnd.dcx", alwaysLoose: true))
                {
                    return $"/enc_regulation.bnd.dcx";
                }
            }
            else
            {
                string genericParambndName = $"/param/GameParam/GameParam{(ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 ? "_dlc2" : "")}.parambnd.dcx";
                if (ParentDocument.GameData.FileExists(genericParambndName))
                {
                    return genericParambndName;
                }
            }
            return null;
        }

        public bool LoadParamBND(bool forceReload)
        {
            string interroot = ParentDocument.GameRoot.InterrootPath;

            bool justNowLoadedParamBND = false;

            if (forceReload)
            {
                ParamBNDs.Clear();
                LoadedParams.Clear();

                BehaviorParam?.Clear();
                BehaviorParam_PC?.Clear();
                AtkParam_Pc?.Clear();
                AtkParam_Npc?.Clear();
                NpcParam?.Clear();
                SpEffectParam?.Clear();

                EquipParamWeapon?.Clear();
                EquipParamProtector?.Clear();
                WepAbsorpPosParam?.Clear();
                WwiseValueToStrParam_Switch_DeffensiveMaterial?.Clear();
                AC6NpcEquipPartsParam?.Clear();
                AC6AttachObjParam_Pc?.Clear();
                AC6AttachObjParam_Npc?.Clear();
                AC6AttackActionParam_NPC?.Clear();
                AC6AttackActionParam_PC?.Clear();
                HitMtrlParamEntries?.Clear();

                SeMaterialConvertParam?.Clear();
            }

            if (forceReload || !ParamBNDs.ContainsKey(ParentDocument.GameRoot.GameType) || Main.Config?.EnableFileCaching == false)
            {
                lock (_lock_Params)
                {
                    ParamBNDs.AddOrUpdate(ParentDocument.GameRoot.GameType, (k) => null, (k, v) => null);
                }


                IBinder chosenParambnd = null;

                if (ParentDocument.GameRoot.GameTypeUsesRegulation && ParentDocument.GameData.IsLoadingRegulationParams)
                {
                    if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 && ParentDocument.GameData.FileExists($"/Data0.bdt", alwaysLoose: true))
                    {
                        var encryptedRegulationBinder = ParentDocument.GameData.ReadFile($"/Data0.bdt", alwaysLoose: true, disableCache: true);
                        chosenParambnd = ParamCryptoUtil.DecryptDS3Regulation(encryptedRegulationBinder);
                        InitAutoReload(ParentDocument.GameData.GetLooseFilePath($"/Data0.bdt"));
                    }
                    else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER && ParentDocument.GameData.FileExists($"/regulation.bin", alwaysLoose: true))
                    {
                        var encryptedRegulationBinder = ParentDocument.GameData.ReadFile($"/regulation.bin", alwaysLoose: true, disableCache: true);
                        chosenParambnd = ParamCryptoUtil.DecryptERRegulation(encryptedRegulationBinder);
                        InitAutoReload(ParentDocument.GameData.GetLooseFilePath($"/regulation.bin"));
                    }
                    else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR && ParentDocument.GameData.FileExists($"/regulation.bin", alwaysLoose: true))
                    {
                        var encryptedRegulationBinder = ParentDocument.GameData.ReadFile($"/regulation.bin", alwaysLoose: true, disableCache: true);
                        chosenParambnd = ParamCryptoUtil.DecryptENRRRegulation(encryptedRegulationBinder);
                        InitAutoReload(ParentDocument.GameData.GetLooseFilePath($"/regulation.bin"));
                    }
                    else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6 && ParentDocument.GameData.FileExists($"/regulation.bin", alwaysLoose: true))
                    {
                        var encryptedRegulationBinder = ParentDocument.GameData.ReadFile($"/regulation.bin", alwaysLoose: true, disableCache: true);
                        chosenParambnd = ParamCryptoUtil.DecryptAC6Regulation(encryptedRegulationBinder);
                        InitAutoReload(ParentDocument.GameData.GetLooseFilePath($"/regulation.bin"));
                    }
                    else if (ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS && ParentDocument.GameData.FileExists($"/enc_regulation.bnd.dcx", alwaysLoose: true))
                    {
                        var encryptedRegulationBinder = ParentDocument.GameData.ReadFile($"/enc_regulation.bnd.dcx", alwaysLoose: true, disableCache: true);
                        chosenParambnd = ParamCryptoUtil.DecryptDS2Regulation(encryptedRegulationBinder);
                        InitAutoReload(ParentDocument.GameData.GetLooseFilePath($"/enc_regulation.bnd.dcx"));
                    }
                }
                else
                {
                    string gameParamName_TryFirst = "/param/GameParam/GameParam.parambnd.dcx";
                    string gameParamName_TrySecond = "/param/GameParam/GameParam.parambnd.dcx";
                    if (ParentDocument.GameRoot.GameType is SoulsGames.DS3)
                        gameParamName_TryFirst = "/param/GameParam/GameParam_dlc2.parambnd.dcx";
                    else if (ParentDocument.GameRoot.GameType is SoulsGames.DES)
                        gameParamName_TryFirst = "/param/GameParam/gameparamna.parambnd.dcx";

                    if (ParentDocument.GameData.FileExists(gameParamName_TryFirst))
                    {
                        chosenParambnd = ParentDocument.GameData.ReadBinder(gameParamName_TryFirst, disableCache: true);
                        InitAutoReload(ParentDocument.GameData.GetLooseFilePath(gameParamName_TryFirst));
                    }
                    else if (ParentDocument.GameData.FileExists(gameParamName_TrySecond))
                    {
                        chosenParambnd = ParentDocument.GameData.ReadBinder(gameParamName_TrySecond, disableCache: true);
                        InitAutoReload(ParentDocument.GameData.GetLooseFilePath(gameParamName_TrySecond));
                    }
                }

                lock (_lock_Params)
                {
                    if (chosenParambnd != null)
                    {
                        ParamBNDs[ParentDocument.GameRoot.GameType] = chosenParambnd;
                    }
                }



                justNowLoadedParamBND = true;
            }

            if (justNowLoadedParamBND || forceReload || GameTypeCurrentLoadedParamsAreFrom != ParentDocument.GameRoot.GameType)
            {
                LoadStuffFromParamBND(isDS2: ParentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS);
            }

            return true;
        }
    }
}
