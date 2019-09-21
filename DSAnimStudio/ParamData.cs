using Microsoft.Xna.Framework;
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
        public long ID;
        public string Name;

        public abstract void Read(BinaryReaderEx br);

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

                //public void ShiftDmyPolyIDIntoPlayerWpnDmyPolyID(bool isLeftHand)
                //{
                //    if (DmyPoly1 >= 0 && DmyPoly1 < 10000)
                //    {
                //        var dmy1mod = DmyPoly1 % 1000;
                //        //if (dmy1mod >= 100 && dmy1mod <= 130)
                //        //{
                //        //    DmyPoly1 = (short)(dmy1mod + (isLeftHand ? 11000 : 10000));
                //        //}
                //        DmyPoly1 = (short)(dmy1mod + (isLeftHand ? 11000 : 10000));
                //    }

                //    if (DmyPoly2 >= 0 && DmyPoly2 < 10000)
                //    {
                //        var dmy2mod = DmyPoly2 % 1000;
                //        //if (dmy2mod >= 100 && dmy2mod <= 130)
                //        //{
                //        //    DmyPoly2 = (short)(dmy2mod + (isLeftHand ? 11000 : 10000));
                //        //}
                //        DmyPoly2 = (short)(dmy2mod + (isLeftHand ? 11000 : 10000));
                //    }
                //}

                public bool IsCapsule => DmyPoly1 >= 0 && DmyPoly2 >= 0 && DmyPoly1 != DmyPoly2;
            }

            public Hit[] Hits;

            public short BlowingCorrection;
            public short AtkPhysCorrection;
            public short AtkMagCorrection;
            public short AtkFireCorrection;
            public short AtkThunCorrection;
            public short AtkStamCorrection;
            public short GuardAtkRateCorrection;
            public short GuardBreakCorrection;
            public short AtkThrowEscapeCorrection;
            public short AtkSuperArmorCorrection;
            public short AtkPhys;
            public short AtkMag;
            public short AtkFire;
            public short AtkThun;
            public short AtkStam;
            public short GuardAtkRate;
            public short GuardBreakRate;
            public short AtkSuperArmor;
            public short AtkThrowEscape;
            public short AtkObj;
            public short GuardStaminaCutRate;
            public short GuardRate;
            public short ThrowTypeID;

            // DS3 Only
            public short AtkDarkCorrection;
            public short AtkDark;

            public Color GetCapsuleColor(Hit hit)
            {
                if (ThrowTypeID > 0)
                {
                    return Color.Cyan;
                }
                else if ((AtkPhys > 0 || AtkMag > 0 || AtkFire > 0 || AtkThun > 0 || AtkDark > 0) || 
                    (AtkPhysCorrection > 0 || AtkMagCorrection > 0 || AtkFireCorrection > 0 || AtkThunCorrection > 0 || AtkDarkCorrection > 0))
                {
                    switch (hit.HitType)
                    {
                        case HitTypes.Tip: return new Color(231, 186, 50);
                        case HitTypes.Middle: return new Color(230, 26, 26);
                        case HitTypes.Root: return new Color(26, 26, 230);
                        default: return Color.Fuchsia;
                    }
                }
                else
                {
                    return Color.DarkGreen;
                }
            }

            public override void Read(BinaryReaderEx br)
            {
                var start = br.Position;

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
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

                BlowingCorrection = br.ReadInt16();
                AtkPhysCorrection = br.ReadInt16();
                AtkMagCorrection = br.ReadInt16();
                AtkFireCorrection = br.ReadInt16();
                AtkThunCorrection = br.ReadInt16();
                AtkStamCorrection = br.ReadInt16();
                GuardAtkRateCorrection = br.ReadInt16();
                GuardBreakCorrection = br.ReadInt16();
                AtkThrowEscapeCorrection = br.ReadInt16();
                AtkSuperArmorCorrection = br.ReadInt16();
                AtkPhys = br.ReadInt16();
                AtkMag = br.ReadInt16();
                AtkFire = br.ReadInt16();
                AtkThun = br.ReadInt16();
                AtkStam = br.ReadInt16();
                GuardAtkRate = br.ReadInt16();
                GuardBreakRate = br.ReadInt16();
                AtkSuperArmor = br.ReadInt16();
                AtkThrowEscape = br.ReadInt16();
                AtkObj = br.ReadInt16();
                GuardStaminaCutRate = br.ReadInt16();
                GuardRate = br.ReadInt16();


                br.Position = start + 0x68;
                ThrowTypeID = br.ReadInt16();

                

                for (int i = 0; i <= 3; i++)
                {
                    Hits[i].HitType = br.ReadEnum8<HitTypes>();
                }

                //TODO: Read DS3 hit 4-15
                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
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

                    //u8 Hit4_hitType
                    //u8 Hit5_hitType
                    //u8 Hit6_hitType
                    //u8 Hit7_hitType
                    //u8 Hit8_hitType
                    //u8 Hit9_hitType
                    //u8 Hit10_hitType
                    //u8 Hit11_hitType
                    //u8 Hit12_hitType
                    //u8 Hit13_hitType
                    //u8 Hit14_hitType
                    //u8 Hit15_hitType
                    br.Position += (1 * 12);

                    //s32 0x174
                    //s32 0x178
                    //s32 0x17C
                    br.Position += (4 * 3);

                    //s16 defMaterialVal0
                    //s16 defMaterialVal1
                    //s16 defMaterialVal2
                    br.Position += (2 * 3);

                    AtkDarkCorrection = br.ReadInt16();
                    AtkDark = br.ReadInt16();
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

            public override void Read(BinaryReaderEx br)
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

            public bool[] DrawMask;

            public void ApplyMaskToModel(Model mdl)
            {
                for (int i = 0; i < Math.Min(Model.DRAW_MASK_LENGTH, DrawMask.Length); i++)
                {
                    mdl.DrawMask[i] = DrawMask[i];
                }
            }

            public override void Read(BinaryReaderEx br)
            {
                var start = br.Position;
                BehaviorVariationID = br.ReadInt32();

                br.Position = start + 0x146;

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3 ||
                    GameDataManager.GameType == GameDataManager.GameTypes.BB)
                {
                    DrawMask = new bool[32];
                }
                else
                {
                    DrawMask = new bool[16];
                }

                byte mask1 = br.ReadByte();
                byte mask2 = br.ReadByte();
                for (int i = 0; i < 8; i++)
                    DrawMask[i] = ((mask1 & (1 << i)) != 0);
                for (int i = 0; i < 8; i++)
                    DrawMask[8 + i] = ((mask2 & (1 << i)) != 0);

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3 || 
                    GameDataManager.GameType == GameDataManager.GameTypes.BB)
                {
                    br.Position = start + 0x14A;
                    byte mask3 = br.ReadByte();
                    byte mask4 = br.ReadByte();
                    for (int i = 0; i < 8; i++)
                        DrawMask[16 + i] = ((mask3 & (1 << i)) != 0);
                    for (int i = 0; i < 8; i++)
                        DrawMask[24 + i] = ((mask4 & (1 << i)) != 0);
                }
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
            public byte WepMotionCategory;
            public short SpAtkCategory;
            public int WepAbsorpPosID = -1;

            public bool IsPairedWeaponDS3 => GameDataManager.GameType == GameDataManager.GameTypes.DS3 && (DS3PairedSpAtkCategories.Contains(SpAtkCategory)
                || (WepMotionCategory == 42)) // DS3 Fist weapons
                ;

            public string GetFullPartBndPath()
            {
                var name = GetPartBndName();
                var partsbndPath = $@"{GameDataManager.InterrootPath}\parts\{name}";

                if (System.IO.File.Exists(partsbndPath + ".dcx"))
                    partsbndPath = partsbndPath + ".dcx";

                return partsbndPath;
            }

            public string GetPartBndName()
            {
                return $"WP_A_{EquipModelID:D4}.partsbnd";
            }

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
                161, //Farron Greatsword
                232, //Friede's Great Scythe
                236, //Valorheart
                237, //Crow Quills
                250, //Giant Door Shield
                253, //Ringed Knight Paired Greatswords
            };

            public override void Read(BinaryReaderEx br)
            {
                long start = br.Position;
                BehaviorVariationID = br.ReadInt32();

                br.Position = start + 0xB8;
                EquipModelID = br.ReadInt16();

                br.Position = start + 0xE3;
                WepMotionCategory = br.ReadByte();

                br.Position = start + 0xEA;
                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                    SpAtkCategory = br.ReadInt16();
                else
                    SpAtkCategory = br.ReadByte();

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                {
                    br.Position = start + 0x170;
                    WepAbsorpPosID = br.ReadInt32();
                }
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

            public void ApplyInvisFlagsToMask(ref bool[] mask)
            {
                for (int i = 0; i < InvisibleFlags.Count; i++)
                {
                    if (i > mask.Length)
                        return;

                    if (InvisibleFlags[i])
                        mask[i] = false;
                }
            }

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

            public string GetFullPartBndPath(bool isFemale)
            {
                var name = GetPartBndName(isFemale);
                var partsbndPath = $@"{GameDataManager.InterrootPath}\parts\{name}";

                if (System.IO.File.Exists(partsbndPath + ".dcx"))
                    partsbndPath = partsbndPath + ".dcx";

                return partsbndPath;
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

            public override void Read(BinaryReaderEx br)
            {
                long start = br.Position;

                br.Position = start + 0xA0;
                EquipModelID = br.ReadInt16();

                br.Position = start + 0xD1;
                EquipModelGender = br.ReadEnum8<EquipModelGenders>();

                br.Position = start + 0xD8;

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 ||
                    GameDataManager.GameType == GameDataManager.GameTypes.BB)
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

                    if (GameDataManager.GameType == GameDataManager.GameTypes.BB)
                    {
                        br.Position = start + 0xFD;
                        var mask48to62 = ReadBitmask(br, 15);

                        InvisibleFlags.AddRange(mask48to62);
                    }
                }
                else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
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
        
        public class WepAbsorpPosParam : ParamData
        {
            public enum WepAbsorpPosType
            {
                OneHand0,
                OneHand1,
                OneHand2,
                OneHand3,
                OneHand4,
                OneHand5,
                OneHand6,
                OneHand7,
                BothHand0,
                BothHand1,
                BothHand2,
                BothHand3,
                BothHand4,
                BothHand5,
                BothHand6,
                BothHand7,
                Sheath0,
                Sheath1,
                Sheath2,
                Sheath3,
                Sheath4,
                Sheath5,
                Sheath6,
                Sheath7,
            }

            public Dictionary<WepAbsorpPosType, ushort> AbsorpPos = new Dictionary<WepAbsorpPosType, ushort>();

            public override void Read(BinaryReaderEx br)
            {
                AbsorpPos = new Dictionary<WepAbsorpPosType, ushort>();

                //u8 SheathTime;
                //u8 pad[3]

                br.Position += 4;

                AbsorpPos.Add(WepAbsorpPosType.OneHand0, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.OneHand1, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.BothHand0, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath0, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath1, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.OneHand2, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.OneHand3, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.BothHand1, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath2, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath3, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.OneHand4, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.OneHand5, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.BothHand2, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath4, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath5, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.OneHand6, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.OneHand7, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.BothHand3, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath6, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.Sheath7, br.ReadUInt16());

                br.Position += 4;

                AbsorpPos.Add(WepAbsorpPosType.BothHand4, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.BothHand5, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.BothHand6, br.ReadUInt16());
                AbsorpPos.Add(WepAbsorpPosType.BothHand7, br.ReadUInt16());
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
