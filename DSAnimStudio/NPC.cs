using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NPC
    {
        public static long CurrentNPCParamID = -1;
        public static ParamData.NpcParam CurrentNPCParam =>
            CurrentNPCParamID >= 0 && ParamManager.NpcParam.ContainsKey(CurrentNPCParamID)
            ? ParamManager.NpcParam[CurrentNPCParamID] : null;
    }
}
