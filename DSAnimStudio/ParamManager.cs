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

        public static void LoadParamBND(HKX.HKXVariation game, string interroot)
        {
            if (ParamBNDs.ContainsKey(game))
                return;
            
            ParamBNDs.Add(game, null);

            if (game == HKX.HKXVariation.HKXDS1)
            {
                ParamBNDs[game] = BND3.Read($"{interroot}\\param\\GameParam\\GameParam.parambnd");
            }
            else if (game == HKX.HKXVariation.HKXBloodBorne)
            {
                ParamBNDs[game] = BND4.Read($"{interroot}\\param\\GameParam\\GameParam.parambnd.dcx");
            }
            else if (game == HKX.HKXVariation.HKXDS3)
            {
                if (File.Exists($"{interroot}\\param\\GameParam\\GameParam_dlc2.parambnd.dcx"))
                {
                    ParamBNDs[game] = BND4.Read($"{interroot}\\param\\GameParam\\GameParam_dlc2.parambnd.dcx");
                }
                else if (File.Exists($"{interroot}\\Data0.bdt"))
                {
                    ParamBNDs[game] = SFUtil.DecryptDS3Regulation($"{interroot}\\Data0.bdt");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
