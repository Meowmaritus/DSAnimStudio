using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.ParamData.AtkParam;
using static DSAnimStudio.ParamData.WepAbsorpPosParam;

namespace DSAnimStudio
{
    public abstract partial class ParamData
    {
        public class WwiseValueToStrParam_Switch_DeffensiveMaterial : ParamData
        {
            public string WwiseString;

            public override void Read(BinaryReaderEx br)
            {
                br.Position += 4;
                WwiseString = br.ReadASCII(32);
                int endMark = WwiseString.IndexOf('\0');
                if (endMark >= 0)
                    WwiseString = WwiseString.Substring(0, endMark);
            }
        }

    }
}



