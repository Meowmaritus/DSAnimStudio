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
        public class BehaviorParam : ParamData
        {
            public int VariationID;
            public int BehaviorJudgeID;
            public byte EzStateBehaviorType_Old;
            public enum RefTypes : byte
            {
                Attack = 0,
                Bullet = 1,
                SpEffect = 2,
                UnkAC6_Type3 = 3,
            }
            public RefTypes RefType;
            public int RefID;
            public int SFXVariationID;
            public int Stamina;
            public int MP;
            public byte Category;
            public byte HeroPoint;

            public int AC6DummyPolyStart = -1;
            public int AC6DummyPolyEnd = -1;

            public AC6AttackActionParam AC6AttackActionParam = null;

            public override void Read(BinaryReaderEx br)
            {


                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    br.Position += 4;

                var start = br.Position;

                VariationID = br.ReadInt32();
                BehaviorJudgeID = br.ReadInt32();
                EzStateBehaviorType_Old = br.ReadByte();
                RefType = br.ReadEnum8<RefTypes>();
                br.Position += 2; //dummy8 pad0[2]

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    AC6DummyPolyStart = br.GetInt32(start - 0x4 + 0x24);
                    AC6DummyPolyEnd = br.GetInt32(start - 0x4 + 0x28);
                }

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                {
                    var idAtk = br.ReadInt32();
                    var idBullet = br.ReadInt32();
                    var idSpEffect = br.ReadInt32();
                    if (RefType == RefTypes.Attack)
                        RefID = idAtk;
                    else if (RefType == RefTypes.Bullet)
                        RefID = idBullet;
                    else if (RefType == RefTypes.SpEffect)
                        RefID = idSpEffect;

                    SFXVariationID = br.ReadInt32();
                    Category = br.ReadByte();

                    br.Position = start + 0x2C;
                    Stamina = br.ReadInt32();
                    MP = br.ReadInt32();
                }
                else
                {
                    RefID = br.ReadInt32();
                    SFXVariationID = br.ReadInt32();
                    Stamina = br.ReadInt32();
                    MP = br.ReadInt32();
                    Category = br.ReadByte();
                    HeroPoint = br.ReadByte();
                }



            }
        }

    }
}
