using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class ParamManager
    {
        private static Dictionary<GameDataManager.GameTypes, IBinder> ParamBNDs 
            = new Dictionary<GameDataManager.GameTypes, IBinder>();

        private static Dictionary<GameDataManager.GameTypes, Dictionary<string, PARAM>> LoadedParams 
            = new Dictionary<GameDataManager.GameTypes, Dictionary<string, PARAM>>();

        public static Dictionary<long, ParamData.BehaviorParam> BehaviorParam_PC 
            = new Dictionary<long, ParamData.BehaviorParam>();

        public static Dictionary<long, ParamData.BehaviorParam> BehaviorParam 
            = new Dictionary<long, ParamData.BehaviorParam>();

        public static Dictionary<long, ParamData.AtkParam> AtkParam_Pc 
            = new Dictionary<long, ParamData.AtkParam>();

        public static Dictionary<long, ParamData.AtkParam> AtkParam_Npc 
            = new Dictionary<long, ParamData.AtkParam>();

        public static Dictionary<long, ParamData.NpcParam> NpcParam 
            = new Dictionary<long, ParamData.NpcParam>();

        public static Dictionary<long, ParamData.EquipParamWeapon> EquipParamWeapon 
            = new Dictionary<long, ParamData.EquipParamWeapon>();

        public static Dictionary<long, ParamData.EquipParamProtector> EquipParamProtector
            = new Dictionary<long, ParamData.EquipParamProtector>();

        public static Dictionary<long, ParamData.WepAbsorpPosParam> WepAbsorpPosParam
           = new Dictionary<long, ParamData.WepAbsorpPosParam>();

        private static GameDataManager.GameTypes GameTypeCurrentLoadedParamsAreFrom = GameDataManager.GameTypes.None;

        public static PARAM GetParam(string paramName)
        {
            if (!ParamBNDs.ContainsKey(GameDataManager.GameType))
                throw new InvalidOperationException("ParamBND not loaded :tremblecat:");

            if (!LoadedParams.ContainsKey(GameDataManager.GameType))
                LoadedParams.Add(GameDataManager.GameType, new Dictionary<string, PARAM>());

            if (LoadedParams[GameDataManager.GameType].ContainsKey(paramName))
            {
                return LoadedParams[GameDataManager.GameType][paramName];
            }
            else
            {
                foreach (var f in ParamBNDs[GameDataManager.GameType].Files)
                {
                    if (f.Name.ToUpper().Contains(paramName.ToUpper()))
                    {
                        var p = PARAM.Read(f.Bytes);
                        LoadedParams[GameDataManager.GameType].Add(paramName, p);
                        return p;
                    }
                }
            }
            throw new InvalidOperationException($"Param '{paramName}' not found :tremblecat:");
        }

        private static bool CheckNpcParamForCurrentGameType(int chrId, ParamData.NpcParam r, bool isFirst, bool matchCXXX0)
        {
            long checkId = r.ID;

            if (matchCXXX0)
            {
                chrId /= 10;
                checkId /= 10;
            }

            if (GameDataManager.GameType != GameDataManager.GameTypes.SDT)
            {
                if ((checkId / 100) == chrId)
                {
                    return true;
                }
                else if (isFirst && GameDataManager.GameType == GameDataManager.GameTypes.BB)
                {
                    return ((checkId % 1_0000_00) / 100 == chrId);
                }
                else
                {
                    return false;
                }
            }
            else if (GameDataManager.GameType == GameDataManager.GameTypes.SDT)
            {
                return (checkId / 1_0000) == chrId;
            }
            else
            {
                throw new NotImplementedException(
                    $"ParamManager.CheckNpcParamForCurrentGameType not implemented for game type {GameDataManager.GameType}");
            }
        }

        public static List<ParamData.NpcParam> FindNpcParams(string modelName, bool matchCXXX0 = false)
        {
            int chrId = int.Parse(modelName.Substring(1));

            var npcParams = new List<ParamData.NpcParam>();
            foreach (var kvp in NpcParam.Where(r 
                => CheckNpcParamForCurrentGameType(chrId, r.Value, npcParams.Count == 0, matchCXXX0)))
            {
                npcParams.Add(kvp.Value);
            }
            return npcParams;
        }
        
        private static void LoadStuffFromParamBND()
        {
            void AddParam<T>(Dictionary<long, T> paramDict, string paramName)
                where T : ParamData, new()
            {
                paramDict.Clear();
                var param = GetParam(paramName);
                foreach (var row in param.Rows)
                {
                    var rowData = new T();
                    rowData.ID = row.ID;
                    rowData.Name = row.Name;
                    rowData.Read(param.GetRowReader(row));
                    paramDict.Add(row.ID, rowData);
                }
            }

            AddParam(BehaviorParam, "BehaviorParam");
            AddParam(BehaviorParam_PC, "BehaviorParam_PC");
            AddParam(AtkParam_Pc, "AtkParam_Pc");
            AddParam(AtkParam_Npc, "AtkParam_Npc");
            AddParam(NpcParam, "NpcParam");
            AddParam(EquipParamWeapon, "EquipParamWeapon");
            AddParam(EquipParamProtector, "EquipParamProtector");
            if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                AddParam(WepAbsorpPosParam, "WepAbsorpPosParam");

            GameTypeCurrentLoadedParamsAreFrom = GameDataManager.GameType;
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

        public static ParamData.AtkParam GetPlayerBasicAtkParam(ParamData.EquipParamWeapon wpn, int behaviorSubID, bool isLeftHand)
        {
            if (wpn == null)
                return null;

            long behaviorParamID = 10_0000_000 + (wpn.BehaviorVariationID * 1_000) + behaviorSubID;

            if (!BehaviorParam_PC.ContainsKey(behaviorParamID))
            {
                if (wpn != null)
                {
                    long baseBehaviorParamID = 10_0000_000 + ((wpn.WepMotionCategory * 100) * 1_000) + behaviorSubID;

                    if (BehaviorParam_PC.ContainsKey(baseBehaviorParamID))
                    {
                        behaviorParamID = baseBehaviorParamID;
                    }
                    else
                    {
                        return null;
                    }
                }
                else

                {
                    return null;
                }

                
            }

            ParamData.BehaviorParam behaviorParamEntry = BehaviorParam_PC[behaviorParamID];
            if (behaviorParamEntry.RefType != 0)
                return null;

            if (!AtkParam_Pc.ContainsKey(behaviorParamEntry.RefID))
                return null;

            return AtkParam_Pc[behaviorParamEntry.RefID];
        }

        public static ParamData.AtkParam GetNpcBasicAtkParam(ParamData.NpcParam npcParam, int behaviorSubID)
        {
            if (npcParam == null)
                return null;

            long behaviorParamID = 2_00000_000 + (npcParam.BehaviorVariationID * 1_000) + behaviorSubID;

            if (!BehaviorParam.ContainsKey(behaviorParamID))
                return null;

            ParamData.BehaviorParam behaviorParamEntry = BehaviorParam[behaviorParamID];

            if (behaviorParamEntry.RefType != 0)
                return null;

            if (!AtkParam_Npc.ContainsKey(behaviorParamEntry.RefID))
                return null;

            return AtkParam_Npc[behaviorParamEntry.RefID];
        }

        public static bool LoadParamBND(bool forceReload)
        {
            string interroot = GameDataManager.InterrootPath;

            bool justNowLoadedParamBND = false;

            if (forceReload || !ParamBNDs.ContainsKey(GameDataManager.GameType))
            {
                ParamBNDs.Add(GameDataManager.GameType, null);

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS1)
                {
                    if (Directory.Exists($"{interroot}\\param\\GameParam\\") && File.Exists($"{interroot}\\param\\GameParam\\GameParam.parambnd"))
                        ParamBNDs[GameDataManager.GameType] = BND3.Read($"{interroot}\\param\\GameParam\\GameParam.parambnd");
                    else
                        return false;
                }
                else if (GameDataManager.GameType == GameDataManager.GameTypes.BB || GameDataManager.GameType == GameDataManager.GameTypes.SDT)
                {
                    if (Directory.Exists($"{interroot}\\param\\GameParam\\") && File.Exists($"{interroot}\\param\\GameParam\\GameParam.parambnd.dcx"))
                        ParamBNDs[GameDataManager.GameType] = BND4.Read($"{interroot}\\param\\GameParam\\GameParam.parambnd.dcx");
                    else
                        return false;
                }
                else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                {
                    if (Directory.Exists($"{interroot}\\param\\GameParam\\") && File.Exists($"{interroot}\\param\\GameParam\\GameParam_dlc2.parambnd.dcx"))
                    {
                        ParamBNDs[GameDataManager.GameType] = BND4.Read($"{interroot}\\param\\GameParam\\GameParam_dlc2.parambnd.dcx");
                    }
                    else if (File.Exists($"{interroot}\\Data0.bdt"))
                    {
                        ParamBNDs[GameDataManager.GameType] = SFUtil.DecryptDS3Regulation($"{interroot}\\Data0.bdt");
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

                justNowLoadedParamBND = true;
            }

            if (justNowLoadedParamBND || forceReload || GameTypeCurrentLoadedParamsAreFrom != GameDataManager.GameType)
            {
                LoadStuffFromParamBND();
            }

            return true;
        }
    }
}
