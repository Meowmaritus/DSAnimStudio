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
        public class EquipParamWeapon : ParamData
        {
            public static int WP_ID_DS3_FriedeScythe = 10180000;

            public static int WP_ID_AC6_UnkWpn299000 = 299000;
            public static int WP_ID_AC6_UnkWpn299100 = 299100;
            public static int WP_ID_AC6_UnkWpn299200 = 299200;
            public static int WP_ID_AC6_UnkWpn299300 = 299300;
            public static int WP_ID_AC6_UnkWpn299400 = 299400;
            public static int WP_ID_AC6_UnkWpn299500 = 299500;
            public static int WP_ID_AC6_UnkWpn3800500 = 3800500;

            

            public int BehaviorVariationID;
            public int FallbackBehaviorVariationID => (BehaviorVariationID / 100) * 100;
            public short EquipModelID;
            public short WepMotionCategory;
            public short SpAtkCategory = -1;
            public int WepAbsorpPosID = -1;

            public byte FDPSoundType = 0;

            public byte BB_SheathType = 0;

            public WepAbsorpPosParam.WepInvisibleTypes GetAborpPosWepInvisibleType(int index)
            {
                var absorpPosParam = GetAbsorpPosParam();
                if (absorpPosParam != null)
                {
                    if (index == 0)
                        return absorpPosParam.WepInvisibleType_Model0;
                    else if (index == 1)
                        return absorpPosParam.WepInvisibleType_Model1;
                    else if (index == 2)
                        return absorpPosParam.WepInvisibleType_Model2;
                    else if (index == 3)
                        return absorpPosParam.WepInvisibleType_Model3;
                }

                return WepAbsorpPosParam.WepInvisibleTypes.Undefined;
            }


            public WepAbsorpPosParam GetAbsorpPosParam()
            {
                if (zzz_DocumentManager.CurrentDocument.ParamManager.WepAbsorpPosParam.ContainsKey(WepAbsorpPosID))
                {
                    return zzz_DocumentManager.CurrentDocument.ParamManager.WepAbsorpPosParam[WepAbsorpPosID];
                }

                return null;
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

            //ac6 todo read this from actual param
            public bool IsLeftHandEquippable;
            public bool IsRightHandEquippable;
            public bool IsBothHandEquippable;
            public bool AC6IsBackLeftSlotEquippable;
            public bool AC6IsBackRightSlotEquippable;


            public IBinder GetPartBnd(NewChrAsm.EquipSlotTypes slotType, bool disableCache)
            {
                return zzz_DocumentManager.CurrentDocument.GameData.ReadBinder($@"/parts/{GetPartBndName(slotType)}", disableCache: disableCache);
            }

            public void LoadAC6PartBndTextureFile()
            {
                var texbndBytes = zzz_DocumentManager.CurrentDocument.GameData.ReadFile($@"/parts/WP_A_{EquipModelID:D4}.partsbnd.dcx");
                if (texbndBytes != null)
                {
                    var texbnd = BND4.Read(texbndBytes);
                    foreach (var f in texbnd.Files)
                    {
                        if (TPF.IsRead(f.Bytes, out TPF asTPF))
                        {
                            zzz_DocumentManager.CurrentDocument.TexturePool.AddTpf(asTPF, null);
                        }
                    }
                }

                var looseTpfBytes = zzz_DocumentManager.CurrentDocument.GameData.ReadFile($@"/parts/WP_{EquipModelID:D4}.tpf.dcx");
                if (looseTpfBytes != null)
                {
                    if (TPF.IsRead(looseTpfBytes, out TPF asTPF))
                    {
                        zzz_DocumentManager.CurrentDocument.TexturePool.AddTpf(asTPF, null);
                    }
                }

            }

            public string GetPartBndName(NewChrAsm.EquipSlotTypes slotType)
            {
                return GetPartBndName_Short(slotType) + ".partsbnd.dcx";
            }

            public string GetPartBndName_A_ForAC6Textures()
            {
                return $"WP_A_{EquipModelID:D4}.partsbnd.dcx";
            }

            public string GetPartBndName_Short(NewChrAsm.EquipSlotTypes slotType)
            {
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    if (slotType is NewChrAsm.EquipSlotTypes.RightWeapon && IsRightHandEquippable)
                        return $"WP_R_{EquipModelID:D4}";
                    else if (slotType is NewChrAsm.EquipSlotTypes.LeftWeapon && IsLeftHandEquippable)
                        return $"WP_L_{EquipModelID:D4}";
                    else if (slotType is NewChrAsm.EquipSlotTypes.AC6BackRightWeapon && AC6IsBackRightSlotEquippable)
                        return $"WP_R_{EquipModelID:D4}";
                    else if (slotType is NewChrAsm.EquipSlotTypes.AC6BackLeftWeapon && AC6IsBackLeftSlotEquippable)
                        return $"WP_L_{EquipModelID:D4}";
                    else if (slotType is NewChrAsm.EquipSlotTypes.AC6BackRightWeaponRail && AC6IsBackRightSlotEquippable)
                        return $"WP_R_{EquipModelID:D4}";
                    else if (slotType is NewChrAsm.EquipSlotTypes.AC6BackLeftWeaponRail && AC6IsBackLeftSlotEquippable)
                        return $"WP_L_{EquipModelID:D4}";
                    //ac6 weapons
                    //else if (slotType is NewChrAsm.EquipSlot.BackRightWeapon && AC6IsBackRightSlot)
                    //    return $"WP_R_{EquipModelID:D4}";
                    //else if (slotType is NewChrAsm.EquipSlot.BackLeftWeapon && AC6IsBackLeftSlot)
                    //    return $"WP_L_{EquipModelID:D4}";
                }
                return $"WP_A_{EquipModelID:D4}";
            }

            public override void Read(BinaryReaderEx br)
            {
                // Empty bytes added at start
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                    br.Position += 4;

                long start = br.Position;

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                {
                    br.Position = start + 0x02;
                    EquipModelID = br.ReadInt16();
                    br.Position = start + 0x07;
                    WepMotionCategory = br.ReadByte();
                    br.Position = start + 0xA;
                    IsRightHandEquippable = br.ReadByte() != 0;
                    IsLeftHandEquippable = br.ReadByte() != 0;
                    IsBothHandEquippable = (br.ReadByte() != 0);
                    br.Position = start + 0x1C;
                    BehaviorVariationID = br.ReadInt32();

                    return;
                }

                BehaviorVariationID = br.ReadInt32();

                br.Position = start + 0xB8;
                EquipModelID = br.ReadInt16();

                br.Position = start + 0xE3;
                WepMotionCategory = br.ReadByte();

                var isEquipBitmask = GetBitmask(br, start + (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.BB or SoulsGames.DS1 ? 0x100 : 0x101), 5);

                IsRightHandEquippable = isEquipBitmask[0];
                IsLeftHandEquippable = isEquipBitmask[1];
                IsBothHandEquippable = isEquipBitmask[2];
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    AC6IsBackRightSlotEquippable = isEquipBitmask[3];
                    AC6IsBackLeftSlotEquippable = isEquipBitmask[4];
                }

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    WepMotionCategory = br.GetInt16(start - 0x4 + 0x308);

                    
                }

                br.Position = start + 0xEA;
                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6)
                    SpAtkCategory = br.ReadInt16();
                else
                    SpAtkCategory = br.ReadByte();

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
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

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3 ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ERNR ||
                    zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6)
                {


                    br.Position = start + 0x170;
                    WepAbsorpPosID = br.ReadInt32();
                }


            }
        }

    }
}
