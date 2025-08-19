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
        public class AC6NpcEquipPartsParam : ParamData
        {
            public struct PartInfo
            {
                public ushort ModelID;
                public int TargetMountDummyPoly;
                public int AttachObjID;
                public byte IsInitVisible;
                public int BreakSfxID;
                public int BreakDummyPolyID;

                public IBinder GetPartBnd(bool disableCache)
                {
                    return zzz_DocumentManager.CurrentDocument.GameData.ReadBinder($@"/parts/{GetPartBndName()}", disableCache: disableCache);
                }

                public string GetPartBndName()
                {
                    return $"WP_E_{ModelID:D4}.partsbnd.dcx";
                }

                public override bool Equals(object obj)
                {
                    var other = (PartInfo)obj;
                    return other.ModelID == ModelID && other.TargetMountDummyPoly == TargetMountDummyPoly &&
                           other.AttachObjID == AttachObjID && other.IsInitVisible == IsInitVisible && other.BreakSfxID == BreakSfxID &&
                           other.BreakDummyPolyID == BreakDummyPolyID;
                }

                public static bool operator ==(PartInfo a, PartInfo b) => a.Equals(b);
                public static bool operator !=(PartInfo a, PartInfo b) => !a.Equals(b);
            }

            public PartInfo[] Parts = new PartInfo[32];

            public List<int> GetValidParts()
            {
                var result = new List<int>();
                for (int i = 0; i < 32; i++)
                {
                    if (Parts[i] != null && Parts[i].ModelID > 0)
                    {
                        result.Add(i);
                    }
                }
                return result;
            }

            public override void Read(BinaryReaderEx br)
            {
                br.Position += 4;
                Parts = new PartInfo[32];
                for (int i = 0; i < 32; i++)
                    Parts[i] = new PartInfo();
                for (int i = 0; i < 32; i++)
                    Parts[i].ModelID = br.ReadUInt16();
                for (int i = 0; i < 32; i++)
                    Parts[i].TargetMountDummyPoly = br.ReadInt32();
                for (int i = 0; i < 32; i++)
                    Parts[i].AttachObjID = br.ReadInt32();
                for (int i = 0; i < 32; i++)
                    Parts[i].IsInitVisible = br.ReadByte();
                for (int i = 0; i < 32; i++)
                    Parts[i].BreakSfxID = br.ReadInt32();
                for (int i = 0; i < 32; i++)
                    Parts[i].BreakDummyPolyID = br.ReadInt32();
            }
        }

    }
}
