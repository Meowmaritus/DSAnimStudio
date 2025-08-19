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
        
        public class AC6AttachObj : ParamData
        {
            public string[] ObjName = new string[8];
            public int[] AttachDmyID = new int[8];

            public override void Read(BinaryReaderEx br)
            {
                for (int i = 0; i < 8; i++)
                {
                    ObjName[i] = br.ReadFixStrW(64);
                }
                for (int i = 0; i < 8; i++)
                {
                    AttachDmyID[i] = br.ReadInt32();
                }
            }
        }

    }
}
