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

        public class AC6AttackActionParam : ParamData
        {
            public ushort BehaviorJudgeID;
            public sbyte ConnectPartsSlotIndex;

            public override void Read(BinaryReaderEx br)
            {
                br.Position += 6;
                BehaviorJudgeID = br.ReadUInt16();
                ConnectPartsSlotIndex = br.ReadSByte();
            }
        }

    }
}
