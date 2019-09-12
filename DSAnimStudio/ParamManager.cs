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
        private static Dictionary<HKX.HKXVariation, IBinder> ParamBNDs 
            = new Dictionary<HKX.HKXVariation, IBinder>();

        private static Dictionary<HKX.HKXVariation, Dictionary<string, PARAM>> LoadedParams 
            = new Dictionary<HKX.HKXVariation, Dictionary<string, PARAM>>();

        public static Dictionary<long, ParamData.BehaviorParam> BehaviorParam_PC = new Dictionary<long, ParamData.BehaviorParam>();
        public static Dictionary<long, ParamData.BehaviorParam> BehaviorParam = new Dictionary<long, ParamData.BehaviorParam>();

        public static Dictionary<long, ParamData.AtkParam> AtkParam_Pc = new Dictionary<long, ParamData.AtkParam>();
        public static Dictionary<long, ParamData.AtkParam> AtkParam_Npc = new Dictionary<long, ParamData.AtkParam>();

        public static Dictionary<long, ParamData.NpcParam> NpcParam = new Dictionary<long, ParamData.NpcParam>();
        public static Dictionary<long, ParamData.EquipParamWeapon> EquipParamWeapon = new Dictionary<long, ParamData.EquipParamWeapon>();

        public static PARAM GetParam(HKX.HKXVariation game, string paramName)
        {
            if (!ParamBNDs.ContainsKey(game))
                throw new InvalidOperationException("ParamBND not loaded :tremblecat:");

            if (!LoadedParams.ContainsKey(game))
                LoadedParams.Add(game, new Dictionary<string, PARAM>());

            if (LoadedParams[game].ContainsKey(paramName))
            {
                return LoadedParams[game][paramName];
            }
            else
            {
                foreach (var f in ParamBNDs[game].Files)
                {
                    if (f.Name.ToUpper().Contains(paramName.ToUpper()))
                    {
                        var p = PARAM.Read(f.Bytes);
                        LoadedParams[game].Add(paramName, p);
                        return p;
                    }
                }
            }
            throw new InvalidOperationException($"Param '{paramName}' not found :tremblecat:");
        }

        private static void LoadStuffFromParamBND(HKX.HKXVariation game)
        {
            void AddParam<T>(Dictionary<long, T> paramDict, string paramName)
                where T : ParamData, new()
            {
                paramDict.Clear();
                var param = GetParam(game, paramName);
                foreach (var row in param.Rows)
                {
                    var rowData = new T();
                    rowData.Name = row.Name;
                    rowData.Read(param.GetRowReader(row), game);
                    paramDict.Add(row.ID, rowData);
                }
            }

            AddParam(BehaviorParam, "BehaviorParam");
            AddParam(BehaviorParam_PC, "BehaviorParam_PC");
            AddParam(AtkParam_Pc, "AtkParam_Pc");
            AddParam(AtkParam_Npc, "AtkParam_Npc");
            AddParam(NpcParam, "NpcParam");
            AddParam(EquipParamWeapon, "EquipParamWeapon");

            Console.WriteLine("TEST");
        }

        public static ParamData.AtkParam GetNpcBasicAtkParam(int behaviorSubID)
        {
            long behaviorParamID = 2_00000_000 + (NpcParam[GFX.ModelDrawer.CurrentNpcParamID].BehaviorVariationID * 1_000) + behaviorSubID;
            ParamData.BehaviorParam behaviorParamEntry = BehaviorParam[behaviorParamID];
            if (behaviorParamEntry.RefType != 0)
                throw new InvalidOperationException($"NPC Behavior {behaviorParamID} does not reference an attack.");

            if (!AtkParam_Npc.ContainsKey(behaviorParamEntry.RefID))
                return null;

            return AtkParam_Npc[behaviorParamEntry.RefID];
        }

        public static bool LoadParamBND(HKX.HKXVariation game, string interroot)
        {
            if (ParamBNDs.ContainsKey(game))
                return true;
            
            ParamBNDs.Add(game, null);

            if (game == HKX.HKXVariation.HKXDS1)
            {
                if (Directory.Exists($"{interroot}\\param\\GameParam\\") && File.Exists($"{interroot}\\param\\GameParam\\GameParam.parambnd"))
                    ParamBNDs[game] = BND3.Read($"{interroot}\\param\\GameParam\\GameParam.parambnd");
                else
                    return false;
            }
            else if (game == HKX.HKXVariation.HKXBloodBorne)
            {
                if (Directory.Exists($"{interroot}\\param\\GameParam\\") && File.Exists($"{interroot}\\param\\GameParam\\GameParam.parambnd.dcx"))
                    ParamBNDs[game] = BND4.Read($"{interroot}\\param\\GameParam\\GameParam.parambnd.dcx");
                else
                    return false;
            }
            else if (game == HKX.HKXVariation.HKXDS3)
            {
                if (Directory.Exists($"{interroot}\\param\\GameParam\\") && File.Exists($"{interroot}\\param\\GameParam\\GameParam_dlc2.parambnd.dcx"))
                {
                    ParamBNDs[game] = BND4.Read($"{interroot}\\param\\GameParam\\GameParam_dlc2.parambnd.dcx");
                }
                else if (File.Exists($"{interroot}\\Data0.bdt"))
                {
                    ParamBNDs[game] = SFUtil.DecryptDS3Regulation($"{interroot}\\Data0.bdt");
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

            LoadStuffFromParamBND(game);

            return true;
        }
    }
}
