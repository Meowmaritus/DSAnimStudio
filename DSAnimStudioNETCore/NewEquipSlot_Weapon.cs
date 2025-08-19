using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using static DSAnimStudio.NewChrAsm;

namespace DSAnimStudio
{
    public class NewEquipSlot_Weapon : NewEquipSlot
    {
        public ParamData.WepAbsorpPosParam AbsorpPosParam = null;
        public ParamData.WepAbsorpPosParam.AbsorpPosCluster Absorp = ParamData.WepAbsorpPosParam.AbsorpPosCluster.Default;

        public int FinalAbsorpModelDmyPoly0 = -1;
        public int FinalAbsorpModelDmyPoly1 = -1;
        public int FinalAbsorpModelDmyPoly2 = -1;
        public int FinalAbsorpModelDmyPoly3 = -1;

        public bool IsERHideGlovesModel => IsAbsorpSkeletonBind;
        public bool IsAbsorpSkeletonBind = false;
        public bool IsERApplyHideMask => IsAbsorpSkeletonBind;

        public ParamData.AtkParam.DummyPolySource DmySource0 = ParamData.AtkParam.DummyPolySource.Invalid;
        public ParamData.AtkParam.DummyPolySource DmySource1 = ParamData.AtkParam.DummyPolySource.Invalid;
        public ParamData.AtkParam.DummyPolySource DmySource2 = ParamData.AtkParam.DummyPolySource.Invalid;
        public ParamData.AtkParam.DummyPolySource DmySource3 = ParamData.AtkParam.DummyPolySource.Invalid;

        public NewDummyPolyManager GetDefaultActiveDummyPolyManager()
        {
            NewDummyPolyManager result = null;
            lock (_lock_MODEL)
            {
                result = model0?.DummyPolyMan ?? model1?.DummyPolyMan ?? model2?.DummyPolyMan ?? model3?.DummyPolyMan;
            }
            return result;
        }

        public NewDummyPolyManager GetDummyPolyManager(int modelIdx)
        {
            NewDummyPolyManager result = null;
            lock (_lock_MODEL)
            {
                if (modelIdx == 0)
                    result = model0?.DummyPolyMan;
                else if (modelIdx == 1)
                    result = model1?.DummyPolyMan;
                else if (modelIdx == 2)
                    result = model2?.DummyPolyMan;
                else if (modelIdx == 3)
                    result = model3?.DummyPolyMan;
            }
            return result;
        }

        public Matrix? GetModelTransform(int modelIdx)
        {
            Matrix? result = null;
            lock (_lock_MODEL)
            {
                if (modelIdx == 0)
                    result = model0?.CurrentTransform.WorldMatrix;
                else if (modelIdx == 1)
                    result = model1?.CurrentTransform.WorldMatrix;
                else if (modelIdx == 2)
                    result = model2?.CurrentTransform.WorldMatrix;
                else if (modelIdx == 3)
                    result = model3?.CurrentTransform.WorldMatrix;
            }
            return result;
        }

        public BB_WeaponState BloodborneWeaponState;

        public int GetFinalAbsorpModelDmyPoly(int modelIndex)
        {
            if (modelIndex == 0)
                return FinalAbsorpModelDmyPoly0;
            else if (modelIndex == 1)
                return FinalAbsorpModelDmyPoly1;
            else if (modelIndex == 2)
                return FinalAbsorpModelDmyPoly2;
            else if (modelIndex == 3)
                return FinalAbsorpModelDmyPoly3;
            return -1;
        }

        public bool SekiroProstheticOverrideTaeActive = false;
        public int SekiroProstheticOverrideDmy0 = -1;
        public int SekiroProstheticOverrideDmy1 = -1;
        public int SekiroProstheticOverrideDmy2 = -1;
        public int SekiroProstheticOverrideDmy3 = -1;

        public DS3MemeAbsorpTaeType Model0MemeAbsorp = DS3MemeAbsorpTaeType.None;
        public DS3MemeAbsorpTaeType Model1MemeAbsorp = DS3MemeAbsorpTaeType.None;
        public DS3MemeAbsorpTaeType Model2MemeAbsorp = DS3MemeAbsorpTaeType.None;
        public DS3MemeAbsorpTaeType Model3MemeAbsorp = DS3MemeAbsorpTaeType.None;

        public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilter0 => 
            AbsorpPosParam?.WepInvisibleType_Model0 ?? ParamData.WepAbsorpPosParam.WepInvisibleTypes.None;
        public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilter1 => 
            AbsorpPosParam?.WepInvisibleType_Model1 ?? ParamData.WepAbsorpPosParam.WepInvisibleTypes.None;
        public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilter2 => 
            AbsorpPosParam?.WepInvisibleType_Model2 ?? ParamData.WepAbsorpPosParam.WepInvisibleTypes.None;
        public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilter3 => 
            AbsorpPosParam?.WepInvisibleType_Model3 ?? ParamData.WepAbsorpPosParam.WepInvisibleTypes.None;

        public void UpdateAbsorp(NewChrAsm.WeaponStyleType style, NewChrAsm asm)
        {
            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                if (EquipSlotType == EquipSlotTypes.RightWeapon)
                    DmySource0 = DmySource1 = DmySource2 = DmySource3 = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                else if (EquipSlotType == EquipSlotTypes.AC6BackRightWeapon)
                    DmySource0 = DmySource1 = DmySource2 = DmySource3 = ParamData.AtkParam.DummyPolySource.RightWeapon1;
                else if (EquipSlotType == EquipSlotTypes.AC6BackRightWeaponRail)
                    DmySource0 = DmySource1 = DmySource2 = DmySource3 = ParamData.AtkParam.DummyPolySource.AC6BackRightWeaponRail;
                else if (EquipSlotType == EquipSlotTypes.LeftWeapon)
                    DmySource0 = DmySource1 = DmySource2 = DmySource3 = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                else if (EquipSlotType == EquipSlotTypes.AC6BackLeftWeapon)
                    DmySource0 = DmySource1 = DmySource2 = DmySource3 = ParamData.AtkParam.DummyPolySource.LeftWeapon1;
                else if (EquipSlotType == EquipSlotTypes.AC6BackLeftWeaponRail)
                    DmySource0 = DmySource1 = DmySource2 = DmySource3 = ParamData.AtkParam.DummyPolySource.AC6BackLeftWeaponRail;
            }
            else
            {
                if (EquipSlotType == EquipSlotTypes.RightWeapon)
                {
                    DmySource0 = ParamData.AtkParam.DummyPolySource.RightWeapon0;
                    DmySource1 = ParamData.AtkParam.DummyPolySource.RightWeapon1;
                    DmySource2 = ParamData.AtkParam.DummyPolySource.RightWeapon2;
                    DmySource3 = ParamData.AtkParam.DummyPolySource.RightWeapon3;
                }
                else if (EquipSlotType == EquipSlotTypes.LeftWeapon)
                {
                    DmySource0 = ParamData.AtkParam.DummyPolySource.LeftWeapon0;
                    DmySource1 = ParamData.AtkParam.DummyPolySource.LeftWeapon1;
                    DmySource2 = ParamData.AtkParam.DummyPolySource.LeftWeapon2;
                    DmySource3 = ParamData.AtkParam.DummyPolySource.LeftWeapon3;
                }
            }

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesWepAbsorpPosParam)
            {
                
                
                if (EquipParam == null)
                    return;

                if (AbsorpPosParam == null || AbsorpPosParam.ID != EquipParam.WepAbsorpPosID)
                    AbsorpPosParam = EquipParam?.GetAbsorpPosParam();

                if (AbsorpPosParam != null)
                {


                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        int meme = 10000;

                        if (EquipSlotType == EquipSlotTypes.RightWeapon)
                        {
                            if (asm.AC6RightWeaponBaySwapped)
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang];
                            else
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Right];

                            meme = 10000;
                        }
                        else if (EquipSlotType == EquipSlotTypes.AC6BackRightWeapon)
                        {
                            if (asm.AC6RightWeaponBaySwapped)
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Right];
                            else
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang];

                            meme = 11000;
                        }
                        else if (EquipSlotType == EquipSlotTypes.LeftWeapon)
                        {
                            if (asm.AC6LeftWeaponBaySwapped)
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang];
                            else
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Left];
                            meme = 20000;
                        }
                        else if (EquipSlotType == EquipSlotTypes.AC6BackLeftWeapon)
                        {
                            if (asm.AC6LeftWeaponBaySwapped)
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Left];
                            else
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang];

                            meme = 21000;
                        }
                        else if (EquipSlotType == EquipSlotTypes.AC6BackLeftWeaponRail)
                        {
                            Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang];
                            Absorp.Model0DummyPoly = 2013;
                            Absorp.Model1DummyPoly = 2013;
                            Absorp.Model2DummyPoly = 2013;
                            Absorp.Model3DummyPoly = 2013;
                            meme = 17000;
                        }
                        else if (EquipSlotType == EquipSlotTypes.AC6BackRightWeaponRail)
                        {
                            Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang];
                            Absorp.Model0DummyPoly = 2014;
                            Absorp.Model1DummyPoly = 2014;
                            Absorp.Model2DummyPoly = 2014;
                            Absorp.Model3DummyPoly = 2014;
                            meme = 19000;
                        }

                        lock (_lock_MODEL)
                        {
                            if (model0?.DummyPolyMan != null)
                                model0.DummyPolyMan.GlobalDummyPolyIDOffset = meme;
                            if (model1?.DummyPolyMan != null)
                                model1.DummyPolyMan.GlobalDummyPolyIDOffset = meme;
                            if (model2?.DummyPolyMan != null)
                                model2.DummyPolyMan.GlobalDummyPolyIDOffset = meme;
                            if (model3?.DummyPolyMan != null)
                                model3.DummyPolyMan.GlobalDummyPolyIDOffset = meme;
                        }
                    }
                    else
                    {

                        if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
                        {
                            IsAbsorpSkeletonBind = AbsorpPosParam.IsSkeletonBind;
                        }
                        else
                        {
                            IsAbsorpSkeletonBind = false;
                        }

                        if (style == WeaponStyleType.OneHand)
                        {
                            if (IsLeftHandSlotType)
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Left];
                            else
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Right];
                        }
                        else if (style == WeaponStyleType.LeftBoth)
                        {
                            if (IsLeftHandSlotType)
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftBoth];
                            else
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang];
                        }
                        else if (style == WeaponStyleType.RightBoth)
                        {
                            if (IsLeftHandSlotType)
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang];
                            else
                                Absorp = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightBoth];
                        }


                        if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsGames.SDT && EquipSlotType is EquipSlotTypes.SekiroMortalBlade)
                        {
                            Absorp.Model0DummyPoly = 149;
                            Absorp.Model1DummyPoly = 149;
                            Absorp.Model2DummyPoly = -1;
                            Absorp.Model3DummyPoly = -1;
                        }
                        else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsGames.SDT && EquipSlotType is EquipSlotTypes.SekiroGrapplingHook)
                        {
                            Absorp.Model0DummyPoly = -1;
                            Absorp.Model1DummyPoly = -1;
                            Absorp.Model2DummyPoly = -1;
                            Absorp.Model3DummyPoly = -1;
                        }


                        // MODEL 0
                        if (Model0MemeAbsorp == DS3MemeAbsorpTaeType.PositionForFriedeScythe
                            && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3
                            && EquipID == ParamData.EquipParamWeapon.WP_ID_DS3_FriedeScythe
                            && !IsLeftHandSlotType)
                        {
                            Absorp.Model0DummyPoly = 21;
                        }
                        else if (Model0MemeAbsorp == DS3MemeAbsorpTaeType.Right)
                        {
                            Absorp.Model0DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Right].Model0DummyPoly;
                        }
                        else if (Model0MemeAbsorp == DS3MemeAbsorpTaeType.Left)
                        {
                            Absorp.Model0DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Left].Model0DummyPoly;
                        }
                        else if (Model0MemeAbsorp == DS3MemeAbsorpTaeType.AnyBoth)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model0DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftBoth].Model0DummyPoly;
                            else
                                Absorp.Model0DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightBoth].Model0DummyPoly;
                        }
                        else if (Model0MemeAbsorp == DS3MemeAbsorpTaeType.AnyHang)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model0DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang].Model0DummyPoly;
                            else
                                Absorp.Model0DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang].Model0DummyPoly;
                        }

                        //MODEL 1
                        if (Model1MemeAbsorp == DS3MemeAbsorpTaeType.PositionForFriedeScythe
                            && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3
                            && EquipID == ParamData.EquipParamWeapon.WP_ID_DS3_FriedeScythe
                            && !IsLeftHandSlotType)
                        {
                            Absorp.Model1DummyPoly = 21;
                        }
                        else if (Model1MemeAbsorp == DS3MemeAbsorpTaeType.Right)
                        {
                            Absorp.Model1DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Right].Model1DummyPoly;
                        }
                        else if (Model1MemeAbsorp == DS3MemeAbsorpTaeType.Left)
                        {
                            Absorp.Model1DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Left].Model1DummyPoly;
                        }
                        else if (Model1MemeAbsorp == DS3MemeAbsorpTaeType.AnyBoth)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model1DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftBoth].Model1DummyPoly;
                            else
                                Absorp.Model1DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightBoth].Model1DummyPoly;
                        }
                        else if (Model1MemeAbsorp == DS3MemeAbsorpTaeType.AnyHang)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model1DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang].Model1DummyPoly;
                            else
                                Absorp.Model1DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang].Model1DummyPoly;
                        }

                        //MODEL 2
                        if (Model2MemeAbsorp == DS3MemeAbsorpTaeType.PositionForFriedeScythe
                            && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3
                            && EquipID == ParamData.EquipParamWeapon.WP_ID_DS3_FriedeScythe
                            && !IsLeftHandSlotType)
                        {
                            Absorp.Model2DummyPoly = 21;
                        }
                        else if (Model2MemeAbsorp == DS3MemeAbsorpTaeType.Right)
                        {
                            Absorp.Model2DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Right].Model2DummyPoly;
                        }
                        else if (Model2MemeAbsorp == DS3MemeAbsorpTaeType.Left)
                        {
                            Absorp.Model2DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Left].Model2DummyPoly;
                        }
                        else if (Model2MemeAbsorp == DS3MemeAbsorpTaeType.AnyBoth)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model2DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftBoth].Model2DummyPoly;
                            else
                                Absorp.Model2DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightBoth].Model2DummyPoly;
                        }
                        else if (Model2MemeAbsorp == DS3MemeAbsorpTaeType.AnyHang)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model2DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang].Model2DummyPoly;
                            else
                                Absorp.Model2DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang].Model2DummyPoly;
                        }

                        //MODEL 3
                        if (Model3MemeAbsorp == DS3MemeAbsorpTaeType.PositionForFriedeScythe
                            && zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3
                            && EquipID == ParamData.EquipParamWeapon.WP_ID_DS3_FriedeScythe
                            && !IsLeftHandSlotType)
                        {
                            Absorp.Model3DummyPoly = 21;
                        }
                        else if (Model3MemeAbsorp == DS3MemeAbsorpTaeType.Right)
                        {
                            Absorp.Model3DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Right].Model3DummyPoly;
                        }
                        else if (Model3MemeAbsorp == DS3MemeAbsorpTaeType.Left)
                        {
                            Absorp.Model3DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.Left].Model3DummyPoly;
                        }
                        else if (Model3MemeAbsorp == DS3MemeAbsorpTaeType.AnyBoth)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model3DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftBoth].Model3DummyPoly;
                            else
                                Absorp.Model3DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightBoth].Model3DummyPoly;
                        }
                        else if (Model3MemeAbsorp == DS3MemeAbsorpTaeType.AnyHang)
                        {
                            if (IsLeftHandSlotType)
                                Absorp.Model3DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.LeftHang].Model3DummyPoly;
                            else
                                Absorp.Model3DummyPoly = AbsorpPosParam.NewAbsorpPos[ParamData.WepAbsorpPosParam.NewWepAbsorpPosType.RightHang].Model3DummyPoly;
                        }
                        
                    }


                }
                else
                {
                    Absorp = ParamData.WepAbsorpPosParam.AbsorpPosCluster.Default;
                }

                FinalAbsorpModelDmyPoly0 = Absorp.Model0DummyPoly;
                FinalAbsorpModelDmyPoly1 = Absorp.Model1DummyPoly;
                FinalAbsorpModelDmyPoly2 = Absorp.Model2DummyPoly;
                FinalAbsorpModelDmyPoly3 = Absorp.Model3DummyPoly;
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.BB)
            {
                if (EquipParam == null)
                    return;

                if (style == WeaponStyleType.OneHand)
                {
                    if (IsLeftHandSlotType)
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_FormA;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_FormA;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_FormA;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_FormA;
                    }
                    else
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_FormA;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_FormA;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_FormA;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_FormA;
                    }
                }
                else if (style == WeaponStyleType.RightBoth)
                {
                    if (IsLeftHandSlotType)
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_Sheath;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_Sheath;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_Sheath;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_Sheath;
                    }
                    else
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_FormB;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_FormB;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_FormB;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_FormB;
                    }
                }
                else if (style == WeaponStyleType.LeftBoth)
                {
                    if (IsLeftHandSlotType)
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_FormB;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_FormB;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_FormB;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_FormB;
                    }
                    else
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_Sheath;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_Sheath;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_Sheath;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_Sheath;
                    }
                }
                else if (style == WeaponStyleType.OneHandTransformedR)
                {
                    if (IsLeftHandSlotType)
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_FormA;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_FormA;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_FormA;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_FormA;
                    }
                    else
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_FormB;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_FormB;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_FormB;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_FormB;
                    }
                }
                else if (style == WeaponStyleType.OneHandTransformedL)
                {
                    if (IsLeftHandSlotType)
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_FormB;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_FormB;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_FormB;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_FormB;
                    }
                    else
                    {
                        FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_FormA;
                        FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_FormA;
                        FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_FormA;
                        FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_FormA;
                    }
                }
                
                
                //
                // if (!IsLeftHandSlotType)
                // {
                //     if (style == WeaponStyleType.OneHand || style == WeaponStyleType.OneHandTransformedL)
                //     {
                //         FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_FormA;
                //         FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_FormA;
                //         FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_FormA;
                //         FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_FormA;
                //     }
                //     else if (style == WeaponStyleType.LeftBoth || style == WeaponStyleType.OneHandTransformedR)
                //     {
                //         FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_FormB;
                //         FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_FormB;
                //         FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_FormB;
                //         FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_FormB;
                //     }
                //     else
                //     {
                //         FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_RH_Sheath;
                //         FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_RH_Sheath;
                //         FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_RH_Sheath;
                //         FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_RH_Sheath;
                //     }
                // }
                // else
                // {
                //     if (style == WeaponStyleType.OneHand || style == WeaponStyleType.OneHandTransformedL)
                //     {
                //         FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_FormA;
                //         FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_FormA;
                //         FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_FormA;
                //         FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_FormA;
                //     }
                //     else if (style == WeaponStyleType.RightBoth || style == WeaponStyleType.OneHandTransformedR)
                //     {
                //         FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_FormB;
                //         FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_FormB;
                //         FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_FormB;
                //         FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_FormB;
                //     }
                //     else
                //     {
                //         FinalAbsorpModelDmyPoly0 = EquipParam.BB_DummyPoly_Model0_LH_Sheath;
                //         FinalAbsorpModelDmyPoly1 = EquipParam.BB_DummyPoly_Model1_LH_Sheath;
                //         FinalAbsorpModelDmyPoly2 = EquipParam.BB_DummyPoly_Model2_LH_Sheath;
                //         FinalAbsorpModelDmyPoly3 = EquipParam.BB_DummyPoly_Model3_LH_Sheath;
                //     }
                // }
            }

            

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT && SekiroProstheticOverrideTaeActive)
            {
                FinalAbsorpModelDmyPoly0 = SekiroProstheticOverrideDmy0;
                FinalAbsorpModelDmyPoly1 = SekiroProstheticOverrideDmy1;
                FinalAbsorpModelDmyPoly2 = SekiroProstheticOverrideDmy2;
                FinalAbsorpModelDmyPoly3 = SekiroProstheticOverrideDmy3;
            }


        }

        public void LoadPartsbndAfterModels(zzz_DocumentIns doc, string partsbndName, IBinder partsbnd)
        {
            lock (_lock_MODEL)
            {
                if (model0 != null)
                    model0.TaeManager_ForParts = NewChrAsmWpnTaeManager.LoadPartsbnd(doc, model0, 0, partsbnd);
                if (model1 != null)
                    model1.TaeManager_ForParts = NewChrAsmWpnTaeManager.LoadPartsbnd(doc, model1, 1, partsbnd);
                if (model2 != null)
                    model2.TaeManager_ForParts = NewChrAsmWpnTaeManager.LoadPartsbnd(doc, model2, 2, partsbnd);
                if (model3 != null)
                    model3.TaeManager_ForParts = NewChrAsmWpnTaeManager.LoadPartsbnd(doc, model3, 3, partsbnd);
            }
        }

        public void SelectAnimation(zzz_DocumentIns doc, SplitAnimID basePlayerAnimTaeID)
        {
            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                return;

            lock (_lock_MODEL)
            {
                model0?.TaeManager_ForParts?.SelectTaeAnimation(doc, basePlayerAnimTaeID, forceNew: true);
                model1?.TaeManager_ForParts?.SelectTaeAnimation(doc, basePlayerAnimTaeID, forceNew: true);
                model2?.TaeManager_ForParts?.SelectTaeAnimation(doc, basePlayerAnimTaeID, forceNew: true);
                model3?.TaeManager_ForParts?.SelectTaeAnimation(doc, basePlayerAnimTaeID, forceNew: true);
            }
        }

        public bool IsLeftHandSlotType => EquipSlotType is EquipSlotTypes.LeftWeapon or EquipSlotTypes.AC6BackLeftWeapon or EquipSlotTypes.AC6BackLeftWeaponRail;

        public bool AC6IsBackSlotType => (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6) && (EquipSlotType is EquipSlotTypes.AC6BackLeftWeapon or EquipSlotTypes.AC6BackRightWeapon);
        public bool AC6IsRailSlotType => (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6) && (EquipSlotType is EquipSlotTypes.AC6BackLeftWeaponRail or EquipSlotTypes.AC6BackRightWeaponRail);


        public int WeaponBoneIndex = -1;


        public object _lock_MODEL = new object();

        private Model model0 = null;
        private Model model1 = null;
        private Model model2 = null;
        private Model model3 = null;

        public void AccessDefaultActiveModel(Action<Model> doAction)
        {
            if (model0?.IsHiddenByAbsorpPos == false)
                doAction(model0);
            else if (model1?.IsHiddenByAbsorpPos == false)
                doAction(model1);
            else if (model2?.IsHiddenByAbsorpPos == false)
                doAction(model2);
            else if (model3?.IsHiddenByAbsorpPos == false)
                doAction(model3);
        }

        public override void AccessAllModels(Action<Model> doAction)
        {
            lock (_lock_MODEL)
            {
                if (model0 != null)
                    doAction(model0);
                if (model1 != null)
                    doAction(model1);
                if (model2 != null)
                    doAction(model2);
                if (model3 != null)
                    doAction(model3);
            }
        }
        
        public void DrawAnimLayerDebug(ref Vector2 pos)
        {
            lock (_lock_MODEL)
            {
                model0?.AnimContainer?.DrawDebug(ref pos);
                model1?.AnimContainer?.DrawDebug(ref pos);
                model2?.AnimContainer?.DrawDebug(ref pos);
                model3?.AnimContainer?.DrawDebug(ref pos);
            }
        }

        public class ModelsManipStruct
        {
            public Model Model0;
            public Model Model1;
            public Model Model2;
            public Model Model3;
        }

        public void ManipModels(Action<ModelsManipStruct> doManip)
        {
            lock (_lock_MODEL)
            {
                var manipStruct = new ModelsManipStruct()
                {
                    Model0 = model0,
                    Model1 = model1,
                    Model2 = model2,
                    Model3 = model3,
                };
                doManip(manipStruct);
                model0 = manipStruct.Model0;
                model1 = manipStruct.Model1;
                model2 = manipStruct.Model2;
                model3 = manipStruct.Model3;
            }
        }

        public void AccessModel(int modelIdx, Action<Model> doAction)
        {
            if (modelIdx < 0 || modelIdx > 3)
                throw new ArgumentException($"Model index {modelIdx} not valid. Valid: 0, 1, 2, 3");
            if (modelIdx == 0 && model0 != null)
                doAction(model0);
            else if (modelIdx == 1 && model1 != null)
                doAction(model1);
            else if (modelIdx == 2 && model2 != null)
                doAction(model2);
            else if (modelIdx == 3 && model3 != null)
                doAction(model3);
        }



        public void SetWeaponHide_AbsorpPos(bool? hide0, bool? hide1, bool? hide2, bool? hide3)
        {
            lock (_lock_MODEL)
            {
                if (hide0.HasValue)
                    model0?.SetHiddenByAbsorpPos(hide0.Value);
                if (hide1.HasValue)
                    model1?.SetHiddenByAbsorpPos(hide1.Value);
                if (hide2.HasValue)
                    model2?.SetHiddenByAbsorpPos(hide2.Value);
                if (hide3.HasValue)
                    model3?.SetHiddenByAbsorpPos(hide3.Value);
            }
            
        }

        public void SetWeaponHide_Tae(bool? hide0, bool? hide1, bool? hide2, bool? hide3)
        {
            lock (_lock_MODEL)
            {
                if (hide0.HasValue)
                    model0?.SetHiddenByTae(hide0.Value);
                if (hide1.HasValue)
                    model1?.SetHiddenByTae(hide1.Value);
                if (hide2.HasValue)
                    model2?.SetHiddenByTae(hide2.Value);
                if (hide3.HasValue)
                    model3?.SetHiddenByTae(hide3.Value);
            }
        }

        public void SetWeaponVisibleByUser(bool vis0, bool vis1, bool vis2, bool vis3)
        {
            lock (_lock_MODEL)
            {
                model0?.SetVisibleByUser(vis0);
                model1?.SetVisibleByUser(vis1);
                model2?.SetVisibleByUser(vis2);
                model3?.SetVisibleByUser(vis3);
            }
        }

        public bool WeaponFlipBackwards = false;
        public bool WeaponFlipSideways = false;
        public bool WeaponFlipUpsideDown = false;

        public bool DebugWeaponModelPositions = false;

        public NewEquipSlot_Weapon(NewChrAsm asm, NewChrAsm.EquipSlotTypes equipSlot, string slotDisplayName, string slotDisplayNameShort)
            : base(asm, equipSlot, slotDisplayName, slotDisplayNameShort, usesEquipParam: true)
        {
            
        }

        public ParamData.EquipParamWeapon EquipParam
            => zzz_DocumentManager.CurrentDocument.ParamManager.EquipParamWeapon.ContainsKey(EquipID)
            ? zzz_DocumentManager.CurrentDocument.ParamManager.EquipParamWeapon[EquipID] : null;

        public override void TryToLoadTextures()
        {
            lock (_lock_MODEL)
            {
                model0?.TryToLoadTextures();
                model1?.TryToLoadTextures();
                model2?.TryToLoadTextures();
                model3?.TryToLoadTextures();
            }
        }

        protected override void InnerDispose()
        {
            lock (_lock_MODEL)
            {
                model0?.Dispose();
                model0 = null;
                model1?.Dispose();
                model1 = null;
                model2?.Dispose();
                model2 = null;
                model3?.Dispose();
                model3 = null;
            }

            AbsorpPosParam = null;
        }
    }
}
