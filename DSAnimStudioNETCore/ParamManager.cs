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
    public static class ParamManager
    {
        private static object _lock_Params = new object();

        private static ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, IBinder> ParamBNDs 
            = new ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, IBinder>();

        private static ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, ConcurrentDictionary<string, PARAM_Hack>> LoadedParams 
            = new ConcurrentDictionary<SoulsAssetPipeline.SoulsGames, ConcurrentDictionary<string, PARAM_Hack>>();

        public static ConcurrentDictionary<long, ParamData.BehaviorParam> BehaviorParam_PC 
            = new ConcurrentDictionary<long, ParamData.BehaviorParam>();

        public static ConcurrentDictionary<long, ParamData.BehaviorParam> BehaviorParam 
            = new ConcurrentDictionary<long, ParamData.BehaviorParam>();

        public static ConcurrentDictionary<long, ParamData.AtkParam> AtkParam_Pc 
            = new ConcurrentDictionary<long, ParamData.AtkParam>();

        public static ConcurrentDictionary<long, ParamData.AtkParam> AtkParam_Npc 
            = new ConcurrentDictionary<long, ParamData.AtkParam>();

        public static ConcurrentDictionary<long, ParamData.NpcParam> NpcParam 
            = new ConcurrentDictionary<long, ParamData.NpcParam>();

        public static ConcurrentDictionary<long, ParamData.SpEffectParam> SpEffectParam
            = new ConcurrentDictionary<long, ParamData.SpEffectParam>();

        public static ConcurrentDictionary<long, ParamData.EquipParamWeapon> EquipParamWeapon 
            = new ConcurrentDictionary<long, ParamData.EquipParamWeapon>();

        public static ConcurrentDictionary<long, ParamData.EquipParamProtector> EquipParamProtector
            = new ConcurrentDictionary<long, ParamData.EquipParamProtector>();

        public static ConcurrentDictionary<long, ParamData.WepAbsorpPosParam> WepAbsorpPosParam
           = new ConcurrentDictionary<long, ParamData.WepAbsorpPosParam>();

        public static ConcurrentDictionary<long, ParamDataDS2.WeaponParam> DS2WeaponParam
            = new ConcurrentDictionary<long, ParamDataDS2.WeaponParam>();

        public static ConcurrentDictionary<long, ParamDataDS2.ArmorParam> DS2ArmorParam
             = new ConcurrentDictionary<long, ParamDataDS2.ArmorParam>();

        public static ConcurrentDictionary<long, ParamData.WwiseValueToStrParam_Switch_DeffensiveMaterial> WwiseValueToStrParam_Switch_DeffensiveMaterial
            = new ConcurrentDictionary<long, ParamData.WwiseValueToStrParam_Switch_DeffensiveMaterial>();



        public static Dictionary<long, string> HitMtrlParamEntries = new Dictionary<long, string>();

        private static SoulsAssetPipeline.SoulsGames GameTypeCurrentLoadedParamsAreFrom = SoulsAssetPipeline.SoulsGames.None;

        public static PARAM_Hack GetParam(string paramName)
        {
            PARAM_Hack foundParam = null;

            lock (_lock_Params)
            {

                if (!ParamBNDs.ContainsKey(GameRoot.GameType))
                    throw new InvalidOperationException("ParamBND not loaded :tremblecat:");

                if (!LoadedParams.ContainsKey(GameRoot.GameType))
                    LoadedParams.AddOrUpdate(GameRoot.GameType, new ConcurrentDictionary<string, PARAM_Hack>(), (k, v) => v);

                if (LoadedParams[GameRoot.GameType].ContainsKey(paramName))
                {
                    return LoadedParams[GameRoot.GameType][paramName];
                }
                else
                {
                    var thisGamesParambnd = ParamBNDs[GameRoot.GameType];
                    if (thisGamesParambnd == null)
                        ImguiOSD.DialogManager.DialogOK("Error", "Unable to load GameParam. Make sure your project directory options such as game data directory, ModEngine /mod/ path, and Load Loose Params options are all correct for your game/mod setup.");
                    else
                    {
                        foreach (var f in ParamBNDs[GameRoot.GameType].Files)
                        {
                            if (f.Name.ToUpper().Contains(paramName.ToUpper()))
                            {
                                var p = PARAM_Hack.Read(f.Bytes);
                                LoadedParams[GameRoot.GameType].AddOrUpdate(paramName, p, (k, v) => v);
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

        private static bool CheckNpcParamForCurrentGameType(int chrId, ParamData.NpcParam r, bool isFirst, bool matchCXXX0)
        {
            long checkId = r.ID;

            if (matchCXXX0)
            {
                chrId /= 10;
                checkId /= 10;
                chrId *= 10;
                checkId *= 10;
            }

            
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
            {
                return ((checkId % 1_0000_0000) / 1_0000) == chrId;
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES || 
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R || 
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
            {
                return ((checkId % 1_0000_00) / 1_00 == chrId);
            }
            else
            {
                throw new NotImplementedException(
                    $"ParamManager.CheckNpcParamForCurrentGameType not implemented for game type {GameRoot.GameType}");
            }
        }

        public static List<ParamData.NpcParam> FindNpcParams(string modelName, bool matchCXXX0 = false)
        {
            int chrId = int.Parse(modelName.Substring(1));

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
        
        private static void LoadStuffFromParamBND(bool isDS2)
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
                            paramDict.AddOrUpdate(row.ID, rowData, (k,v) => rowData);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Failed to read row {row.ID} ({row.Name ?? "<No Name>"}) of param '{paramName}': {ex.ToString()}");
                    }
                    
                }
            }

            BehaviorParam.Clear();
            BehaviorParam_PC.Clear();
            AtkParam_Pc.Clear();
            AtkParam_Npc.Clear();
            NpcParam.Clear();
            EquipParamWeapon.Clear();
            EquipParamProtector.Clear();
            WepAbsorpPosParam.Clear();
            SpEffectParam.Clear();

            HitMtrlParamEntries.Clear();

            DS2WeaponParam.Clear();
            DS2ArmorParam.Clear();

            if (isDS2)
            {
                AddParam(DS2WeaponParam, "WeaponParam");
                AddParam(DS2ArmorParam, "ArmorParam");
            }
            else
            {
                AddParam(BehaviorParam, "BehaviorParam");
                if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                    BehaviorParam_PC = new ConcurrentDictionary<long, ParamData.BehaviorParam>(BehaviorParam.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                else
                    AddParam(BehaviorParam_PC, "BehaviorParam_PC");
                AddParam(AtkParam_Pc, "AtkParam_Pc");
                AddParam(AtkParam_Npc, "AtkParam_Npc");
                AddParam(NpcParam, "NpcParam");
                AddParam(EquipParamWeapon, "EquipParamWeapon");
                AddParam(EquipParamProtector, "EquipParamProtector");
                if (GameRoot.GameTypeUsesWepAbsorpPosParam)
                    AddParam(WepAbsorpPosParam, "WepAbsorpPosParam");
                AddParam(SpEffectParam, "SpEffectParam");
                if (GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DES)
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

                if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
                {
                    AddParam(WwiseValueToStrParam_Switch_DeffensiveMaterial, "WwiseValueToStrParam_Switch_DeffensiveMaterial");
                }
            }

            GameTypeCurrentLoadedParamsAreFrom = GameRoot.GameType;
        }

        public static ParamData.AtkParam GetPlayerCommonAttack(int absoluteBehaviorID)
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

        public static ParamData.AtkParam GetPlayerBasicAtkParam(ParamData.EquipParamWeapon wpn, int behaviorJudgeID, bool isLeftHand)
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

        public static ParamData.AtkParam GetNpcBasicAtkParam(ParamData.NpcParam npcParam, int behaviorJudgeID)
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

        public static bool LoadParamBND(bool forceReload)
        {
            string interroot = GameRoot.InterrootPath;

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
                EquipParamWeapon?.Clear();
                EquipParamProtector?.Clear();
                WepAbsorpPosParam?.Clear();
            }

            if (forceReload || !ParamBNDs.ContainsKey(GameRoot.GameType))
            {
                ParamBNDs.AddOrUpdate(GameRoot.GameType, (k) => null, (k,v) => null);

                IBinder chosenParambnd = null;
                
                if (GameRoot.GameTypeUsesRegulation && GameData.IsLoadingRegulationParams)
                {
                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 && GameData.FileExists("/Data0.bdt", alwaysLoose: true))
                    {
                        chosenParambnd = ParamCryptoUtil.DecryptDS3Regulation(GameData.ReadFile($"/Data0.bdt", alwaysLoose: true));
                    }
                    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER && GameData.FileExists("/regulation.bin", alwaysLoose: true))
                    {
                        chosenParambnd = ParamCryptoUtil.DecryptERRegulation(GameData.ReadFile($"/regulation.bin", alwaysLoose: true));
                    }
                    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS && GameData.FileExists("/enc_regulation.bnd.dcx", alwaysLoose: true))
                    {
                        chosenParambnd = ParamCryptoUtil.DecryptDS2Regulation(GameData.ReadFile("/enc_regulation.bnd.dcx", alwaysLoose: true));
                    }
                }
                else
                {
                    string genericParambndName = $"/param/GameParam/GameParam{(GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 ? "_dlc2" : "")}.parambnd.dcx";
                    if (GameData.FileExists(genericParambndName))
                    {
                        var bnd = GameData.ReadFile(genericParambndName);
                        if (BND3.Is(bnd))
                            chosenParambnd = BND3.Read(bnd);
                        else if (BND4.Is(bnd))
                            chosenParambnd = BND4.Read(bnd);
                    }
                }

                if (chosenParambnd != null)
                {
                    lock (_lock_Params)
                    {
                        ParamBNDs[GameRoot.GameType] = chosenParambnd;
                    }
                }

                justNowLoadedParamBND = true;
            }

            if (justNowLoadedParamBND || forceReload || GameTypeCurrentLoadedParamsAreFrom != GameRoot.GameType)
            {
                LoadStuffFromParamBND(isDS2: GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS);
            }

            return true;
        }
    }
}
