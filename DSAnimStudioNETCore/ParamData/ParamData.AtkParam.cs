using ImGuiNET;
using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using static DSAnimStudio.ParamData.AtkParam;
using static DSAnimStudio.ParamData.WepAbsorpPosParam;

namespace DSAnimStudio
{
    public abstract partial class ParamData
    {
        public class AtkParam : ParamData
        {
            public enum HitTypes : byte
            {
                Tip = 0,
                Middle = 1,
                Root = 2,
                EldenRingHitType3 = 3,
                AC6Test_FF = 0xFF,
            }

            public enum HitSourceTypes : int
            {
                Weapon = 0,
                Body = 1,
                Parts0 = 2,
                // etc lol
                Parts31 = 33,
            }

            public enum DummyPolySource
            {
                Invalid = -1,
                BaseModel = 0,
                HeadPart = 1,
                BodyPart = 2,
                ArmsPart = 3,
                LegsPart = 4,
                RightWeapon0 = 10,
                RightWeapon1 = 11,
                RightWeapon2 = 12,
                RightWeapon3 = 13,
                LeftWeapon0 = 20,
                LeftWeapon1 = 21,
                LeftWeapon2 = 22,
                LeftWeapon3 = 23,
                AC6BackRightWeaponRail = 17,
                AC6BackLeftWeaponRail = 19,

                AC6Parts0 = 100,
                AC6Parts1 = 101,
                AC6Parts2 = 102,
                AC6Parts3 = 103,
                AC6Parts4 = 104,
                AC6Parts5 = 105,
                AC6Parts6 = 106,
                AC6Parts7 = 107,
                AC6Parts8 = 108,
                AC6Parts9 = 109,
                AC6Parts10 = 110,
                AC6Parts11 = 111,
                AC6Parts12 = 112,
                AC6Parts13 = 113,
                AC6Parts14 = 114,
                AC6Parts15 = 115,
                AC6Parts16 = 116,
                AC6Parts17 = 117,
                AC6Parts18 = 118,
                AC6Parts19 = 119,
                AC6Parts20 = 120,
                AC6Parts21 = 121,
                AC6Parts22 = 122,
                AC6Parts23 = 123,
                AC6Parts24 = 124,
                AC6Parts25 = 125,
                AC6Parts26 = 126,
                AC6Parts27 = 127,
                AC6Parts28 = 128,
                AC6Parts29 = 129,
                AC6Parts30 = 130,
                AC6Parts31 = 131,
            }

            public enum AC6HitboxTypes : byte
            {
                Normal = 0,
                Hit0Extend = 1,
                Fan = 2,
            }

            public AC6HitboxTypes AC6HitboxType;

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

                public bool DmyPoly1FallbackToBody;
                public bool DmyPoly2FallbackToBody;


                public bool AC6_IsFan;
                public ushort AC6Fan_Reach;
                public short AC6Fan_RotationX;
                public short AC6Fan_RotationY;
                public short AC6Fan_RotationZ;
                public float AC6Fan_ThicknessTop;
                public float AC6Fan_ThicknessBottom;
                public byte AC6Fan_SpanAngle;


                public bool AC6_IsExtend;

                public float AC6_Extend_LengthStart;
                public float AC6_Extend_LengthEnd;
                public float AC6_Extend_LengthSpreadTime;
                public float AC6_Extend_RadiusEnd;
                public float AC6_Extend_RadiusSpreadTime;
                public float AC6_Extend_SpreadDelay;


                public NewDummyPolyManager GetDmyPoly1SpawnPlace(Model mdl, DummyPolySource defaultDummyPolySource)
                {
                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6 && !mdl.IS_PLAYER && mdl.AC6NpcParts != null)
                        return mdl?.AC6NpcParts?.GetDummyPolySpawnPlace(defaultDummyPolySource, DmyPoly1, mdl.DummyPolyMan);
                    return mdl?.ChrAsm?.GetDummyPolySpawnPlace(defaultDummyPolySource, DmyPoly1, mdl.DummyPolyMan);
                }

                public NewDummyPolyManager GetDmyPoly2SpawnPlace(Model mdl, DummyPolySource defaultDummyPolySource)
                {
                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6 && !mdl.IS_PLAYER && mdl.AC6NpcParts != null)
                        return mdl?.AC6NpcParts?.GetDummyPolySpawnPlace(defaultDummyPolySource, DmyPoly2, mdl.DummyPolyMan);
                    return mdl?.ChrAsm?.GetDummyPolySpawnPlace(defaultDummyPolySource, DmyPoly2, mdl.DummyPolyMan);
                }

                public List<Matrix> GetDmyPoly1Locations(Model mdl, DummyPolySource defaultDummySource, bool isPlayerWeapon)
                {
                    if (DmyPoly1 == -1)
                        return new List<Matrix>() { Matrix.Identity };

                    if (mdl.AC6NpcParts != null)
                    {
                        var placeAC6 = mdl.AC6NpcParts.GetDummyPolySpawnPlace(defaultDummySource, DmyPoly1, mdl.DummyPolyMan);
                        return placeAC6?.GetDummyMatricesByID(DmyPoly1) ?? new List<Matrix>() { Matrix.Identity };
                    }

                    if (mdl.ChrAsm == null)
                        return mdl.DummyPolyMan?.GetDummyMatricesByID(DmyPoly1) ?? new List<Matrix>() { Matrix.Identity };

                    

                    var place = mdl.ChrAsm.GetDummyPolySpawnPlace(defaultDummySource, DmyPoly1, mdl.DummyPolyMan);

                    return place?.GetDummyMatricesByID(DmyPoly1 % 1000) ?? new List<Matrix>() { Matrix.Identity };
                }

                public List<Matrix> GetDmyPoly2Locations(Model mdl, DummyPolySource defaultDummySource, bool isPlayerWeapon)
                {
                    if (DmyPoly2 == -1)
                        return new List<Matrix>() { mdl.CurrentTransform.WorldMatrix };

                    if (mdl.AC6NpcParts != null)
                    {
                        var placeAC6 = mdl.AC6NpcParts.GetDummyPolySpawnPlace(defaultDummySource, DmyPoly2, mdl.DummyPolyMan);
                        return placeAC6?.GetDummyMatricesByID(DmyPoly2) ?? new List<Matrix>() { Matrix.Identity };
                    }

                    if (mdl.ChrAsm == null)
                        return mdl.DummyPolyMan?.GetDummyMatricesByID(DmyPoly2) ?? new List<Matrix>() { Matrix.Identity };


                    var place = mdl.ChrAsm.GetDummyPolySpawnPlace(defaultDummySource, DmyPoly2, mdl.DummyPolyMan);
                    return place?.GetDummyMatricesByID(DmyPoly2 % 1000) ?? new List<Matrix>() { Matrix.Identity };
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

                //public static Color ColorTip = new Color(231, 186, 50);
                //public static Color ColorMiddle = new Color(230, 26, 26);
                //public static Color ColorRoot = new Color(26, 26, 230);

                public Color GetColor(NewDummyPolyManager.HitboxSpawnTypes spawnType)
                {
                    float opacity = 1;
                    
                    if (spawnType == NewDummyPolyManager.HitboxSpawnTypes.AttackBehavior)
                        opacity *= Main.HelperDraw.AttackBehaviorHitboxOpacity;
                    else if (spawnType == NewDummyPolyManager.HitboxSpawnTypes.CommonBehavior)
                        opacity *= Main.HelperDraw.CommonBehaviorHitboxOpacity;
                    else if (spawnType == NewDummyPolyManager.HitboxSpawnTypes.ThrowAttackBehavior)
                        opacity *= Main.HelperDraw.ThrowAttackBehaviorHitboxOpacity;
                    else if (spawnType == NewDummyPolyManager.HitboxSpawnTypes.PCBehavior)
                        opacity *= Main.HelperDraw.PCBehaviorHitboxOpacity;
                    
                    switch (HitType)
                    {
                        case HitTypes.Tip: return Main.Colors.ColorHelperHitboxTip * opacity;
                        case HitTypes.Middle: return Main.Colors.ColorHelperHitboxMiddle * opacity;
                        case HitTypes.Root: return Main.Colors.ColorHelperHitboxRoot * opacity;
                        default: return Main.Colors.ColorHelperHitboxUnknown * opacity;
                    }
                }

                public bool IsCapsule => (DmyPoly1 >= 0 && DmyPoly2 >= 0 && DmyPoly1 != DmyPoly2) || (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && AC6_IsExtend);
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

            public HitSourceTypes HitSourceType;

            // DS3 Only
            public short AtkDarkCorrection;
            public short AtkDark;

            //AC6 only
            public float AC6_Hit0Extend_LengthStart;
            public float AC6_Hit0Extend_LengthEnd;
            public float AC6_Hit0Extend_LengthSpreadTime;
            //Note: normal hit0 radius is start radius
            public float AC6_Hit0Extend_RadiusEnd;
            public float AC6_Hit0Extend_RadiusSpreadTime;
            public float AC6_Hit0Extend_SpreadDelay;

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
                //if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                //    br.Position += 0xC;

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    br.Position += 4;



                var start = br.Position;

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    AC6HitboxType = (AC6HitboxTypes)br.GetByte(start - 0x4 + 0x1D9);

                    br.Position = start - 0x4 + 0x1BC;

                    AC6_Hit0Extend_LengthStart = br.ReadSingle();
                    AC6_Hit0Extend_LengthEnd = br.ReadSingle();
                    AC6_Hit0Extend_LengthSpreadTime = br.ReadSingle();
                    AC6_Hit0Extend_RadiusEnd = br.ReadSingle();
                    AC6_Hit0Extend_RadiusSpreadTime = br.ReadSingle();
                    AC6_Hit0Extend_SpreadDelay = br.ReadSingle();

                    br.Position = start;
                }


                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6)
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

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                {
                    br.Position = start + 0x7F;
                    HitSourceType = (HitSourceTypes)br.ReadByte();

                    br.Position = start + 0x80;

                    for (int i = 0; i < 4; i++)
                    {
                        br.Position = start + 0x80 + (0x10 * i);

                        Hits[i].DmyPoly1 = br.ReadInt16();
                        Hits[i].DmyPoly2 = br.ReadInt16();
                        Hits[i].Radius = br.ReadSingle();
                        Hits[i].HitType = br.ReadEnum8<HitTypes>();
                    }

                    return;
                }


                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var returnToPos = br.Position;

                    br.Position = start - 0x4 + 0x228;

                    for (int i = 0; i < Hits.Length; i++)
                        Hits[i].AC6Fan_Reach = br.ReadUInt16();
                    for (int i = 0; i < Hits.Length; i++)
                        Hits[i].AC6Fan_RotationY = br.ReadInt16();
                    for (int i = 0; i < Hits.Length; i++)
                        Hits[i].AC6Fan_RotationZ = br.ReadInt16();
                    for (int i = 0; i < Hits.Length; i++)
                        Hits[i].AC6Fan_ThicknessTop = br.ReadSingle();
                    for (int i = 0; i < Hits.Length; i++)
                        Hits[i].AC6Fan_ThicknessBottom = br.ReadSingle();
                    for (int i = 0; i < Hits.Length; i++)
                        Hits[i].AC6Fan_SpanAngle = br.ReadByte();

                    br.Position = start + 0x360;
                    for (int i = 0; i < Hits.Length; i++)
                        Hits[i].AC6Fan_RotationX = br.ReadInt16();

                    br.Position = returnToPos;

                    if (AC6HitboxType == AC6HitboxTypes.Hit0Extend)
                    {
                        if (Hits.Length > 0)
                        {
                            Hits[0].AC6_IsExtend = true;
                            Hits[0].AC6_Extend_LengthStart = AC6_Hit0Extend_LengthStart;
                            Hits[0].AC6_Extend_LengthEnd = AC6_Hit0Extend_LengthEnd;
                            Hits[0].AC6_Extend_LengthSpreadTime = AC6_Hit0Extend_LengthSpreadTime;
                            Hits[0].AC6_Extend_RadiusEnd = AC6_Hit0Extend_RadiusEnd;
                            Hits[0].AC6_Extend_RadiusSpreadTime = AC6_Hit0Extend_RadiusSpreadTime;
                            Hits[0].AC6_Extend_SpreadDelay = AC6_Hit0Extend_SpreadDelay;
                        }
                    }
                    else if (AC6HitboxType == AC6HitboxTypes.Fan)
                    {
                        for (int i = 0; i < Hits.Length; i++)
                        {
                            Hits[i].AC6_IsFan = true;
                        }
                    }


                }



                //f32 knockbackDist
                //f32 hitStopTime

                //[SDT]
                //f32 Unk01
                //f32 Unk02
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
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

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                    br.Position += 0x8;

                    ThrowTypeID = br.ReadInt16();



                for (int i = 0; i <= 3; i++)
                {
                    Hits[i].HitType = (HitTypes)br.ReadByte();
                }

                br.Position += 14;
                HitSourceType = (HitSourceTypes)br.ReadByte();

                //TODO: Read DS3 hit 4-15
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 
                    || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT 
                    || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER
                    || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR
                    || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6)
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

                    br.Position += 0x8B;

                    long debugRelOffset = br.Position - (start - 4);

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
                        Hits[i].HitType = (HitTypes)br.ReadByte();
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

                //for (int i = 0; i < Hits.Length; i++)
                //{
                //    if (Hits[i].DmyPoly2 == 0)
                //        Hits[i].DmyPoly2 = -1;
                //}
            }
        }

    }
}
