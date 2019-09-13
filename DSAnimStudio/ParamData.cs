using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public abstract class ParamData
    {
        public string Name;

        public abstract void Read(BinaryReaderEx br, HKX.HKXVariation game);

        public class AtkParam : ParamData
        {
            public enum HitTypes : byte
            {
                Tip = 0,
                Middle = 1,
                Root = 2,
            }

            public struct Hit
            {
                public float Radius;
                public short DmyPoly1;
                public short DmyPoly2;
                public HitTypes HitType;
                public bool IsCapsule => DmyPoly1 >= 0 && DmyPoly2 >= 0;
            }

            public Hit[] Hits;

            public override void Read(BinaryReaderEx br, HKX.HKXVariation game)
            {
                if (game == HKX.HKXVariation.HKXDS3)
                {
                    Hits = new Hit[16];
                }
                else
                {
                    Hits = new Hit[4];
                }

                for (int i = 0; i < Hits.Length; i++)
                {
                    Hits[i].DmyPoly1 = -1;
                    Hits[i].DmyPoly2 = -1;
                }



                for (int i = 0; i <= 3; i++)
                {
                    Hits[i].Radius = br.ReadSingle();
                }

                //f32 knockbackDist
                //f32 hitStopTime
                //s32 spEffectId0
                //s32 spEffectId1
                //s32 spEffectId2
                //s32 spEffectId3
                //s32 spEffectId4
                br.Position += (4 * 7);


                for (int i = 0; i <= 3; i++)
                {
                    Hits[i].DmyPoly1 = br.ReadInt16();
                }

                for (int i = 0; i <= 3; i++)
                {
                    Hits[i].DmyPoly2 = br.ReadInt16();
                }

                // s16 Blowing Correction
                // s16 AtkPhysCorrection
                // s16 AtkMagCorrection
                // s16 AtkFireCorrection
                // s16 AtkThunCorrection
                // s16 AtkStamCorrection
                // s16 GuardAtkRateCorrection
                // s16 GuardBreakCorrection
                // s16 AtkThrowEscapeCorrection
                // s16 AtkSuperArmorCorrection
                // s16 AtkPhys
                // s16 AtkMag
                // s16 AtkFire
                // s16 AtkThun
                // s16 AtkStam
                // s16 GuardAtkRate
                // s16 GuardBreakRate
                // s16 AtkSuperArmor
                // s16 AtkThrowEscape
                // s16 AtkObj
                // s16 GuardStaminaCutRate
                // s16 GuardRate
                // s16 ThrowTypeID
                br.Position += (23 * 2);

                for (int i = 0; i <= 3; i++)
                {
                    Hits[i].HitType = br.ReadEnum8<HitTypes>();
                }

                //TODO: Read DS3 hit 4-15
                if (game == HKX.HKXVariation.HKXDS3)
                {
                    //u8 Hit0_hitType
                    //u8 Hit1_hitType
                    //u8 Hit2_hitType
                    //u8 Hit3_hitType
                    //u8 hit0_Priority
                    //u8 hit1_Priority
                    //u8 hit2_Priority
                    //u8 hit3_Priority
                    //u8 damageLevel
                    //u8 mapHitType
                    //u8 GuardCutCancelRate
                    //s8 AtkAttribute
                    //s8 spAttribute
                    //s8 atkType
                    //s8 atkMaterial
                    //s8 atkSize
                    //s8 DefMaterial
                    //s8 DefSfxMaterial
                    //u8 HitSourceType
                    //u8 ThrowFlag
                    //b8 disableGuard
                    //b8 disableStaminaAttack
                    //b8 disableHitSpEffect
                    //b8 IgnoreNotifyMissSwingForAI
                    //b8 repeatHitSfx
                    //b8 IsArrowAtk
                    //b8 IsGhostAtk
                    //b8 isDisableNoDamage
                    //u8 atkPowForSfxSe
                    //u8 atkDirForSfxSe
                    //b8 opposeTarget
                    //b8 friendlyTarget
                    //b8 selfTarget
                    //b8 isChargeAtk
                    //b8 isShareHitList
                    //b8 isCheckObjPenetration
                    //b8 0x81
                    //b8 0x81
                    //u8 pad1
                    //u8 regainableSlotId
                    //s32 deathCauseId
                    //s32 decalId1
                    //s32 decalId2
                    //s32 spawnAiSoundId
                    //s32 HitAiSoundId
                    //s32 RumbleId0
                    //s32 RumbleId1
                    //s32 RumbleId2
                    //s32 RumbleId3
                    //s32 Hit0_VfxId
                    //s32 Hit0_DummyPolyId0
                    //s32 Hit0_DummyPolyId1
                    //s32 Hit1_VfxId1
                    //s32 Hit1_DummyPolyId0
                    //s32 Hit1_DummyPolyId1
                    //s32 Hit2_VfxId
                    //s32 Hit2_DummyPolyId0
                    //s32 Hit2_DummyPolyId1
                    //s32 Hit3_VfxId
                    //s32 Hit3_DummyPolyId0
                    //s32 Hit3_DummyPolyId1
                    //s32 Hit4_VfxId
                    //s32 Hit4_DummyPolyId0
                    //s32 Hit4_DummyPolyId1
                    //s32 Hit5_VfxId
                    //s32 Hit5_DummyPolyId0
                    //s32 Hit5_DummyPolyId1
                    //s32 Hit6_VfxId
                    //s32 Hit6_DummyPolyId0
                    //s32 Hit6_DummyPolyId1
                    //s32 Hit7_VfxId
                    //s32 Hit7_DummyPolyId0
                    //s32 Hit7_DummyPolyId1
                    br.Position += ((1 * 22) + (4 * 33));
                    for (int i = 4; i <= 15; i++)
                    {
                        Hits[i].Radius = br.ReadSingle();
                    }

                    for (int i = 4; i <= 15; i++)
                    {
                        Hits[i].DmyPoly1 = br.ReadInt16();
                    }

                    for (int i = 4; i <= 15; i++)
                    {
                        Hits[i].DmyPoly2 = br.ReadInt16();
                    }

                    for (int i = 4; i <= 15; i++)
                    {
                        Hits[i].HitType = br.ReadEnum8<HitTypes>();
                    }
                }
            }
        }




        public class BehaviorParam : ParamData
        {
            public int VariationID;
            public int BehaviorJudgeID;
            public byte EzStateBehaviorType_Old;
            public byte RefType;
            public int RefID;
            public int SFXVariationID;
            public int Stamina;
            public int MP;
            public byte Category;
            public byte HeroPoint;

            public override void Read(BinaryReaderEx br, HKX.HKXVariation game)
            {
                VariationID = br.ReadInt32();
                BehaviorJudgeID = br.ReadInt32();
                EzStateBehaviorType_Old = br.ReadByte();
                RefType = br.ReadByte();
                br.Position += 2; //dummy8 pad0[2]
                RefID = br.ReadInt32();
                SFXVariationID = br.ReadInt32();
                Stamina = br.ReadInt32();
                MP = br.ReadInt32();
                Category = br.ReadByte();
                HeroPoint = br.ReadByte();
            }
        }

        public class NpcParam : ParamData
        {
            public int BehaviorVariationID;

            public override void Read(BinaryReaderEx br, HKX.HKXVariation game)
            {
                BehaviorVariationID = br.ReadInt32();
            }
        }

        public enum EquipModelGenders : byte
        {
            Unisex = 0,
            MaleOnly = 1,
            FemaleOnly = 2,
            Both = 3,
            UseMaleForBoth = 4,
        }

        public class EquipParamWeapon : ParamData
        {
            public int BehaviorVariationID;
            public short EquipModelID;
            public byte SpAtkCategory;

            public bool IsPairedWeaponDS3 => DS3PairedSpAtkCategories.Contains(SpAtkCategory);

            public static readonly int[] DS3PairedSpAtkCategories = new int[]
            {
                137, //Sellsword Twinblades, Warden Twinblades
                138, //Winged Knight Twinaxes
                139, //Dancer's Enchanted Swords
                141, //Brigand Twindaggers
                142, //Gotthard Twinswords
                144, //Onikiri and Ubadachi
                145, //Drang Twinspears
                148, //Drang Hammers
                232, //Friede's Great Scythe
                236, //Valorheart
                237, //Crow Quills
                250, //Giant Door Shield
                253, //Ringed Knight Paired Greatswords
            };

            public override void Read(BinaryReaderEx br, HKX.HKXVariation game)
            {
                long start = br.Position;
                BehaviorVariationID = br.ReadInt32();

                br.Position = start + 0xB8;
                EquipModelID = br.ReadInt16();

                br.Position = start + 0xEA;
                SpAtkCategory = br.ReadByte();
            }
        }

        public class EquipParamProtector : ParamData
        {
            public short EquipModelID;
            public EquipModelGenders EquipModelGender;
            public bool HeadEquip;
            public bool BodyEquip;
            public bool ArmEquip;
            public bool LegEquip;
            public List<bool> InvisibleFlags = new List<bool>();

            private string GetPartFileNameStart()
            {
                if (HeadEquip)
                    return "HD";
                else if (BodyEquip)
                    return "BD";
                else if (ArmEquip)
                    return "AM";
                else if (LegEquip)
                    return "LG";
                else
                    return null;
            }

            public string GetPartBndName(bool isFemale)
            {
                string start = GetPartFileNameStart();

                if (start == null)
                    return null;

                switch (EquipModelGender)
                {
                    case EquipModelGenders.Unisex: return $"{start}_A_{EquipModelID:D4}.partsbnd";
                    case EquipModelGenders.MaleOnly:
                    case EquipModelGenders.UseMaleForBoth:
                        return $"{start}_M_{EquipModelID:D4}.partsbnd";
                    case EquipModelGenders.FemaleOnly:
                        return $"{start}_F_{EquipModelID:D4}.partsbnd";
                    case EquipModelGenders.Both:
                        return $"{start}_{(isFemale ? "F" : "M")}_{EquipModelID:D4}.partsbnd";
                }

                return null;
            }

            public override void Read(BinaryReaderEx br, HKX.HKXVariation game)
            {
                long start = br.Position;

                br.Position = start + 0xA0;
                EquipModelID = br.ReadInt16();

                br.Position = start + 0xD1;
                EquipModelGender = br.ReadEnum8<EquipModelGenders>();

                br.Position = start + 0xD8;

                if (game == HKX.HKXVariation.HKXDS1 || game == HKX.HKXVariation.HKXBloodBorne)
                {
                    var firstBitmask = ReadBitmask(br, 6 + 48);
                    //IsDeposit = firstBitmask[0]
                    HeadEquip = firstBitmask[1];
                    BodyEquip = firstBitmask[2];
                    ArmEquip = firstBitmask[3];
                    LegEquip = firstBitmask[4];
                    //UseFaceScale = firstBitmask[5];
                    InvisibleFlags.Clear();
                    for (int i = 0; i <= 47; i++)
                    {
                        InvisibleFlags.Add(firstBitmask[i + 6]);
                    }

                    if (game == HKX.HKXVariation.HKXBloodBorne)
                    {
                        br.Position = start + 0xFD;
                        var mask48to62 = ReadBitmask(br, 15);

                        InvisibleFlags.AddRange(mask48to62);
                    }
                }
                else if (game == HKX.HKXVariation.HKXDS3)
                {
                    var firstBitmask = ReadBitmask(br, 5);
                    //IsDeposit = firstBitmask[0]
                    HeadEquip = firstBitmask[1];
                    BodyEquip = firstBitmask[2];
                    ArmEquip = firstBitmask[3];
                    LegEquip = firstBitmask[4];

                    br.Position = start + 0x12E;
                    for (int i = 0; i < 96; i++)
                    {
                        InvisibleFlags.Add(br.ReadByte() == 1);
                    }
                }
            }
        }

        private static List<bool> ReadBitmask(BinaryReaderEx br, int numBits)
        {
            List<bool> result = new List<bool>(numBits);
            var maskBytes = br.ReadBytes((int)Math.Ceiling(numBits / 8.0f));
            for (int i = 0; i < numBits; i++)
            {
                result.Add((maskBytes[i / 8] & (byte)(1 << (i % 8))) != 0);
            }
            return result;
        }
    }
}
