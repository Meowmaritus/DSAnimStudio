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

        public string GetDisplayName()
        {
            return $"{ID}{(!string.IsNullOrWhiteSpace(Name) ? $": {Name}" : "")}";
        }

        public abstract void Read(BinaryReaderEx br);

        public class AtkParam : ParamData
        {
            public enum HitTypes : byte
            {
                Tip = 0,
                Middle = 1,
                Root = 2,
            }

            public enum DummyPolySource
            {
                None,
                Body,
                RightWeapon0,
                RightWeapon1,
                RightWeapon2,
                RightWeapon3,
                LeftWeapon0,
                LeftWeapon1,
                LeftWeapon2,
                LeftWeapon3,

            }

            //public DummyPolySource SuggestedDummyPolySource;

            public struct Hit
            {
                public float Radius;
                public short DmyPoly1;
                public short DmyPoly2;
                public HitTypes HitType;

                const int DS3PairR = 10000;
                const int DS3PairL = 11000;

                const int SDTSomething1 = 10000;
                const int SDTSomething2 = 21000;

                public DummyPolySource DummyPolySourceSpawnedOn;

                public NewDummyPolyManager GetDmyPoly1SpawnPlace(NewChrAsm asm, DummyPolySource defaultDummyPolySource)
                {
                    return asm.GetDummyPolySpawnPlace(defaultDummyPolySource, DmyPoly1);
                }

                public NewDummyPolyManager GetDmyPoly2SpawnPlace(NewChrAsm asm, DummyPolySource defaultDummyPolySource)
                {
                    return asm.GetDummyPolySpawnPlace(defaultDummyPolySource, DmyPoly2);
                }

                public List<Matrix> GetDmyPoly1Locations(Model mdl, DummyPolySource defaultDummySource)
                {
                    if (DmyPoly1 == -1)
                        return new List<Matrix>() { Matrix.Identity };

                    var modMatrix = mdl.StartTransform.WorldMatrix * Matrix.Invert(mdl.CurrentRootMotionRotation * mdl.CurrentRootMotionTranslation);

                    if (mdl.ChrAsm == null)
                        return mdl.DummyPolyMan?.GetDummyMatricesByID(DmyPoly1, modMatrix) ?? new List<Matrix>() { modMatrix };

                    var place = mdl.ChrAsm.GetDummyPolySpawnPlace(defaultDummySource, DmyPoly1);

                    
                    //if (place == mdl.DummyPolyMan)
                    //{
                    //    modMatrix = Matrix.Identity;
                    //}

                    return place?.GetDummyMatricesByID(DmyPoly1 % 1000, modMatrix) ?? new List<Matrix>() { modMatrix };
                }

                public List<Matrix> GetDmyPoly2Locations(Model mdl, DummyPolySource defaultDummySource)
                {
                    if (DmyPoly2 == -1)
                        return new List<Matrix>() { Matrix.Identity };

                    var modMatrix = mdl.StartTransform.WorldMatrix * Matrix.Invert(mdl.CurrentRootMotionRotation * mdl.CurrentRootMotionTranslation);

                    if (mdl.ChrAsm == null)
                        return mdl.DummyPolyMan?.GetDummyMatricesByID(DmyPoly2, modMatrix) ?? new List<Matrix>() { modMatrix };

                    var place = mdl.ChrAsm.GetDummyPolySpawnPlace(defaultDummySource, DmyPoly2);

                    //if (place != mdl.DummyPolyMan)
                    //{
                    //    modMatrix = mdl.StartTransform.WorldMatrix * Matrix.Invert(mdl.CurrentRootMotionRotation * mdl.CurrentRootMotionTranslation);
                    //}

                    return place?.GetDummyMatricesByID(DmyPoly2 % 1000, modMatrix) ?? new List<Matrix>() { modMatrix };
                }

                //public static int GetFilteredDmyPolyID(ParamData.AtkParam.DummyPolySource dmyFilter, int id)
                //{
                //    if (id < 0)
                //        return -1;

                //    int check = id / 1000;
                //    id = id % 1000;

                //    if (dmyFilter == DummyPolySource.None)
                //    {
                //        return -1;
                //    }
                //    else if (dmyFilter == DummyPolySource.Body)
                //    {
                //        return (check < 10) ? id : -1;
                //    }
                //    else if (dmyFilter == DummyPolySource.RightWeapon)
                //    {
                //        return (check == 10 || check == 12) ? id : -1;
                //    }
                //    else if (dmyFilter == DummyPolySource.LeftWeapon)
                //    {
                //        return (check == 11 || check == 13 || check == 20) ? id : -1;
                //    }

                //    return -1;
                //}

                //public int GetFilteredDmyPoly1(ParamData.AtkParam.DummyPolySource dmyFilter)
                //{
                //    return GetFilteredDmyPolyID(dmyFilter, DmyPoly1);
                //}

                //public int GetFilteredDmyPoly2(ParamData.AtkParam.DummyPolySource dmyFilter)
                //{
                //    return GetFilteredDmyPolyID(dmyFilter, DmyPoly2);
                //}

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

                public static Color ColorTip = new Color(231, 186, 50);
                public static Color ColorMiddle = new Color(230, 26, 26);
                public static Color ColorRoot = new Color(26, 26, 230);

                public Color GetColor()
                {
                    switch (HitType)
                    {
                        case HitTypes.Tip: return ColorTip;
                        case HitTypes.Middle: return ColorMiddle;
                        case HitTypes.Root: return ColorRoot;
                        default: return Color.Fuchsia;
                    }
                }

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

            public byte HitSourceType;

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

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
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

                //[SDT]
                //f32 Unk01
                //f32 Unk02
                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                    br.Position += (4 * 2);

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

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                    br.Position += (4 * 2);

                ThrowTypeID = br.ReadInt16();

                

                for (int i = 0; i <= 3; i++)
                {
                    Hits[i].HitType = br.ReadEnum8<HitTypes>();
                }

                br.Position += 14;
                HitSourceType = br.ReadByte();

                //TODO: Read DS3 hit 4-15
                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                {
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
                    br.Position += ((1 * (22 - 15)) + (4 * 33));
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
                    //br.Position += (1 * 12);

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

                //SuggestedDummyPolySource = DummyPolySource.None;

                //for (int i = 0; i < Hits.Length; i++)
                //{
                //    if (Hits[i].DmyPoly1 >= 10000 && Hits[i].DmyPoly1 < 11000)
                //        SuggestedDummyPolySource = DummyPolySource.RightWeapon;
                //    else if (Hits[i].DmyPoly1 >= 11000 && Hits[i].DmyPoly1 < 12000)
                //        SuggestedDummyPolySource = DummyPolySource.LeftWeapon;
                //    else if (Hits[i].DmyPoly1 >= 12000 && Hits[i].DmyPoly1 < 13000)
                //        SuggestedDummyPolySource = DummyPolySource.RightWeapon;
                //    else if (Hits[i].DmyPoly1 >= 13000 && Hits[i].DmyPoly1 < 14000)
                //        SuggestedDummyPolySource = DummyPolySource.LeftWeapon;
                //    else if (Hits[i].DmyPoly1 >= 20000 && Hits[i].DmyPoly1 < 21000)
                //        SuggestedDummyPolySource = DummyPolySource.LeftWeapon;
                //}

                //if (SuggestedDummyPolySource == DummyPolySource.None)
                //{
                //    br.Position = start + 0x7C;
                //    byte hitSourceType = br.ReadByte();
                //    if (hitSourceType == 1)
                //        SuggestedDummyPolySource = DummyPolySource.Body;
                //}
            }
        }




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
            }
            public RefTypes RefType;
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
                RefType = br.ReadEnum8<RefTypes>();
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

            public float TurnVelocity;

            public bool[] DrawMask;

            public string GetMaskString(Dictionary<int, string> materialsPerMask, 
                List<int> masksEnabledOnAllNpcParamsForThisChr)
            {
                if (materialsPerMask.Any(kvp => kvp.Key >= 0))
                {
                    var sb = new StringBuilder();

                    bool isFirst = true;

                    bool nothingInHere = true;

                    foreach (var kvp in materialsPerMask)
                    {
                        if (kvp.Key < 0)
                            continue;

                        if (masksEnabledOnAllNpcParamsForThisChr.Contains(kvp.Key))
                            continue;

                        if (DrawMask[kvp.Key])
                        {
                            if (!isFirst)
                                sb.Append("  ");
                            else
                                isFirst = false;

                            sb.Append($"[{kvp.Value}]");
                            nothingInHere = false;
                        }
                    }

                    if (nothingInHere)
                    {
                        return "";
                    }
                    else
                    {
                        return sb.ToString();
                    }
                }
                else
                {
                    return "";
                }
                
            }

            public void ApplyToNpcModel(Model mdl)
            {
                for (int i = 0; i < Math.Min(Model.DRAW_MASK_LENGTH, DrawMask.Length); i++)
                {
                    mdl.DrawMask[i] = DrawMask[i];
                    mdl.DefaultDrawMask[i] = DrawMask[i];
                }
                mdl.BaseTrackingSpeed = TurnVelocity;
            }

            public override void Read(BinaryReaderEx br)
            {
                var start = br.Position;
                BehaviorVariationID = br.ReadInt32();
                //aiThinkId
                //nameId
                br.Position += 8;
                TurnVelocity = br.ReadSingle();

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                {
                    DrawMask = new bool[32];
                }
                else
                {
                    DrawMask = new bool[16];
                }

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                {
                    br.Position = start + 0x14E;
                }
                else
                {
                    br.Position = start + 0x146;
                }

                byte mask1 = br.ReadByte();
                byte mask2 = br.ReadByte();
                for (int i = 0; i < 8; i++)
                    DrawMask[i] = ((mask1 & (1 << i)) != 0);
                for (int i = 0; i < 8; i++)
                    DrawMask[8 + i] = ((mask2 & (1 << i)) != 0);

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 || 
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                {
                    if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                    {
                        br.Position = start + 0x152;
                    }
                    else
                    {
                        br.Position = start + 0x14A;
                    }
                    
                    byte mask3 = br.ReadByte();
                    byte mask4 = br.ReadByte();
                    for (int i = 0; i < 8; i++)
                        DrawMask[16 + i] = ((mask3 & (1 << i)) != 0);
                    for (int i = 0; i < 8; i++)
                        DrawMask[24 + i] = ((mask4 & (1 << i)) != 0);
                }
            }
        }

        public class SpEffectParam : ParamData
        {
            public float GrabityRate = 1.0f;

            public override void Read(BinaryReaderEx br)
            {
                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                {
                    br.Position += 0x104;
                    GrabityRate = br.ReadSingle();
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
            public int FallbackBehaviorVariationID => (BehaviorVariationID / 100) * 100;
            public short EquipModelID;
            public byte WepMotionCategory;
            public short SpAtkCategory;
            public int WepAbsorpPosID = -1;

            public byte FDPSoundType = 0;

            public byte BB_SheathType = 0;

            public int GetDS3TaeConditionFlag(int index)
            {
                var absorpPosParam = GetAbsorpPosParam();
                if (absorpPosParam != null)
                {
                    if (index == 0)
                        return absorpPosParam.Condition1;
                    else if (index == 1)
                        return absorpPosParam.Condition2;
                    else if (index == 2)
                        return absorpPosParam.Condition3;
                    else if (index == 3)
                        return absorpPosParam.Condition4;
                }

                return -1;
            }


            public WepAbsorpPosParam GetAbsorpPosParam()
            {
                if (ParamManager.WepAbsorpPosParam.ContainsKey(WepAbsorpPosID))
                {
                    return ParamManager.WepAbsorpPosParam[WepAbsorpPosID];
                }

                return null;
            }

            public int DS3_GetRightWeaponDummyPoly(NewChrAsm chrAsm, int modelIndex)
            {
                var ds3OverrideMeme = GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3
                    ? chrAsm.GetDS3PairedWpnMemeKindR(modelIndex) : NewChrAsm.DS3PairedWpnMemeKind.None;

                if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.PositionForFriedeScythe && chrAsm.IsRightWeaponFriedesScythe())
                {
                    return NewChrAsm.FRIEDE_SCYTHE_LH_DUMMYPOLY_ID;
                }

                if (ParamManager.WepAbsorpPosParam.ContainsKey(WepAbsorpPosID))
                {
                    var absorp = ParamManager.WepAbsorpPosParam[WepAbsorpPosID];

                    // DS3 OVERRIDE MEME - SHARED L AND R 
                    if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.OneHand_Left)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand1];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand3];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand5];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand7];
                    }
                    else if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.OneHand_Right)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand0];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand2];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand4];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand6];
                    }
                    // DS3 OVERRIDE MEME - RIGHT SPECIFIC
                    else if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.BothHand)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand0];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand1];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand2];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand3];
                    }
                    else if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.Sheath)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath1];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath3];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath5];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath7];
                    }
                    // REGULAR DS3 STUFF
                    else if (chrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHand)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand0];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand2];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand4];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand6];
                    }
                    else if (chrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandL)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath1];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath3];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath5];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath7];
                    }
                    else if (chrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandR)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand0];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand1];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand2];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand3];
                    }
                }

                return -1;
            }

            public int DS3_GetLeftWeaponDummyPoly(NewChrAsm chrAsm, int modelIndex)
            {
                var ds3OverrideMeme = GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 
                    ? chrAsm.GetDS3PairedWpnMemeKindL(modelIndex) : NewChrAsm.DS3PairedWpnMemeKind.None;

                // Friede meme value actually does not work on left weapon.

                if (ParamManager.WepAbsorpPosParam.ContainsKey(WepAbsorpPosID))
                {
                    var absorp = ParamManager.WepAbsorpPosParam[WepAbsorpPosID];

                    // DS3 OVERRIDE MEME - SHARED L AND R 
                    if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.OneHand_Left)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand1];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand3];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand5];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand7];
                    }
                    else if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.OneHand_Right)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand0];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand2];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand4];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand6];
                    }
                    // DS3 OVERRIDE MEME - LEFT SPECIFIC
                    else if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.BothHand)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand4];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand5];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand6];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand7];
                    }
                    else if (ds3OverrideMeme == NewChrAsm.DS3PairedWpnMemeKind.Sheath)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath0];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath2];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath4];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath6];
                    }
                    // REGULAR DS3 STUFF
                    else if (chrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.OneHand)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand1];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand3];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand5];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.OneHand7];
                    }
                    else if (chrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandL)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand4];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand5];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand6];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.BothHand7];
                    }
                    else if (chrAsm.WeaponStyle == NewChrAsm.WeaponStyleType.TwoHandR)
                    {
                        if (modelIndex == 0)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath0];
                        else if (modelIndex == 1)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath2];
                        else if (modelIndex == 2)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath4];
                        else if (modelIndex == 3)
                            return absorp.AbsorpPos[WepAbsorpPosParam.WepAbsorpPosType.Sheath6];
                    }
                }

                return -1;
            }

            public int BB_GetRightWeaponDummyPoly(NewChrAsm chrAsm, int modelIndex)
            {
                if (modelIndex == 0)
                {
                    if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model0_RH_Sheath;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model0_RH_FormA;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model0_RH_FormB;
                }
                else if (modelIndex == 1)
                {
                    if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model1_RH_Sheath;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model1_RH_FormA;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model1_RH_FormB;
                }
                else if (modelIndex == 2)
                {
                    if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model2_RH_Sheath;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model2_RH_FormA;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model2_RH_FormB;
                }
                else if (modelIndex == 3)
                {
                    if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model3_RH_Sheath;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model3_RH_FormA;
                    else if (chrAsm.BB_RightWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model3_RH_FormB;
                }
                return -1;
            }

            public int BB_GetLeftWeaponDummyPoly(NewChrAsm chrAsm, int modelIndex)
            {
                if (modelIndex == 0)
                {
                    if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model0_LH_Sheath;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model0_LH_FormA;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model0_LH_FormB;
                }
                else if (modelIndex == 1)
                {
                    if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model1_LH_Sheath;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model1_LH_FormA;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model1_LH_FormB;
                }
                else if (modelIndex == 2)
                {
                    if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model2_LH_Sheath;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model2_LH_FormA;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model2_LH_FormB;
                }
                else if (modelIndex == 3)
                {
                    if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.Sheathed)
                        return BB_DummyPoly_Model3_LH_Sheath;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormA)
                        return BB_DummyPoly_Model3_LH_FormA;
                    else if (chrAsm.BB_LeftWeaponState == NewChrAsm.BB_WeaponState.FormB)
                        return BB_DummyPoly_Model3_LH_FormB;
                }
                return -1;
            }

            public short BB_DummyPoly_Model0_RH_Sheath = -1;
            public short BB_DummyPoly_Model0_RH_FormA = -1;
            public short BB_DummyPoly_Model0_RH_FormB = -1;
            public short BB_DummyPoly_Model0_LH_Sheath = -1;
            public short BB_DummyPoly_Model0_LH_FormA = -1;
            public short BB_DummyPoly_Model0_LH_FormB = -1;
            public short BB_DummyPoly_Model1_RH_Sheath = -1;
            public short BB_DummyPoly_Model1_RH_FormA = -1;
            public short BB_DummyPoly_Model1_RH_FormB = -1;
            public short BB_DummyPoly_Model1_LH_Sheath = -1;
            public short BB_DummyPoly_Model1_LH_FormA = -1;
            public short BB_DummyPoly_Model1_LH_FormB = -1;
            public short BB_DummyPoly_Model2_RH_Sheath = -1;
            public short BB_DummyPoly_Model2_RH_FormA = -1;
            public short BB_DummyPoly_Model2_RH_FormB = -1;
            public short BB_DummyPoly_Model2_LH_Sheath = -1;
            public short BB_DummyPoly_Model2_LH_FormA = -1;
            public short BB_DummyPoly_Model2_LH_FormB = -1;
            public short BB_DummyPoly_Model3_RH_Sheath = -1;
            public short BB_DummyPoly_Model3_RH_FormA = -1;
            public short BB_DummyPoly_Model3_RH_FormB = -1;
            public short BB_DummyPoly_Model3_LH_Sheath = -1;
            public short BB_DummyPoly_Model3_LH_FormA = -1;
            public short BB_DummyPoly_Model3_LH_FormB = -1;

            public bool IsPairedWeaponDS3 => GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 
                && (DS3PairedSpAtkCategories.Contains(SpAtkCategory) || (WepMotionCategory == 42)) // DS3 Fist weapons
                ;

            public string GetFullPartBndPath()
            {
                var name = GetPartBndName();
                var partsbndPath = GameDataManager.GetInterrootPath($@"parts\{name}");

                return partsbndPath;
            }

            public string GetPartBndName()
            {
                return $"WP_A_{EquipModelID:D4}.partsbnd{(GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS1 ? ".dcx" : "")}";
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
                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                    SpAtkCategory = br.ReadInt16();
                else
                    SpAtkCategory = br.ReadByte();

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
                {
                    br.Position = start + 0x107;
                    BB_SheathType = br.ReadByte();
                    BB_DummyPoly_Model0_RH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model0_RH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model0_RH_FormB = br.ReadInt16();
                    BB_DummyPoly_Model0_LH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model0_LH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model0_LH_FormB = br.ReadInt16();
                    BB_DummyPoly_Model1_RH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model1_RH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model1_RH_FormB = br.ReadInt16();
                    BB_DummyPoly_Model1_LH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model1_LH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model1_LH_FormB = br.ReadInt16();
                    BB_DummyPoly_Model2_RH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model2_RH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model2_RH_FormB = br.ReadInt16();
                    BB_DummyPoly_Model2_LH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model2_LH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model2_LH_FormB = br.ReadInt16();
                    BB_DummyPoly_Model3_RH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model3_RH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model3_RH_FormB = br.ReadInt16();
                    BB_DummyPoly_Model3_LH_Sheath = br.ReadInt16();
                    BB_DummyPoly_Model3_LH_FormA = br.ReadInt16();
                    BB_DummyPoly_Model3_LH_FormB = br.ReadInt16();
                }

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 || 
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
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

            public bool[] ApplyInvisFlagsToMask(bool[] mask)
            {
                for (int i = 0; i < InvisibleFlags.Count; i++)
                {
                    if (i > mask.Length)
                        break;

                    if (InvisibleFlags[i])
                        mask[i] = false;
                }

                return mask;
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
                var partsbndPath = GameDataManager.GetInterrootPath($@"parts\{name}");

                return partsbndPath;
            }

            public string GetPartBndName(bool isFemale)
            {
                string start = GetPartFileNameStart();

                if (start == null)
                    return null;

                switch (EquipModelGender)
                {
                    case EquipModelGenders.Unisex: 
                        return $"{start}_A_{EquipModelID:D4}.partsbnd{(GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS1 ? ".dcx" : "")}";
                    case EquipModelGenders.MaleOnly:
                    case EquipModelGenders.UseMaleForBoth:
                        return $"{start}_M_{EquipModelID:D4}.partsbnd{(GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS1 ? ".dcx" : "")}";
                    case EquipModelGenders.FemaleOnly:
                        return $"{start}_F_{EquipModelID:D4}.partsbnd{(GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS1 ? ".dcx" : "")}";
                    case EquipModelGenders.Both:
                        return $"{start}_{(isFemale ? "F" : "M")}_{EquipModelID:D4}.partsbnd{(GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS1 ? ".dcx" : "")}";
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

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
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

                    if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
                    {
                        br.Position = start + 0xFD;
                        var mask48to62 = ReadBitmask(br, 15);

                        InvisibleFlags.AddRange(mask48to62);
                    }
                }
                else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                {
                    var firstBitmask = ReadBitmask(br, 5);
                    //IsDeposit = firstBitmask[0]
                    HeadEquip = firstBitmask[1];
                    BodyEquip = firstBitmask[2];
                    ArmEquip = firstBitmask[3];
                    LegEquip = firstBitmask[4];

                    br.Position = start + 0x12E;
                    for (int i = 0; i < 98; i++)
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

            public byte Condition1;
            public byte Condition2;
            public byte Condition3;
            public byte Condition4;

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

                Condition1 = br.ReadByte();
                Condition2 = br.ReadByte();
                Condition3 = br.ReadByte();
                Condition4 = br.ReadByte();

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
