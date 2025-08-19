using DSAnimStudio.TaeEditor;
using HKX2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoulsAssetPipeline;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace DSAnimStudio
{
    public class NewChrAsm : IDisposable
    {


        public bool AC6LeftWeaponBaySwapped = false;
        public bool AC6RightWeaponBaySwapped = false;

        private object _lock_boneMappersAndGluersUpdateOrder = new object();
        private List<NewEquipSlot> boneMappersAndGluersUpdateOrder = new List<NewEquipSlot>(); 

        private void CreateGluers()
        {
            ForAllArmorSlots(slot =>
            {
                if (slot.EquipSlotType >= EquipSlotTypes.Facegen1 && slot.EquipSlotType <= EquipSlotTypes.FacegenMax)
                {
                    slot.AccessAllModels(model =>
                    {
                        model.BoneGluer = new NewBoneGluer(leader: MODEL, follower: model, mode: NewBoneGluer.GlueModes.ShiftEntireSkeleton, NewBoneGluer.GlueMethods.MatchEverything);
                        model.BoneGluer.AddGlueEntry(leaderBone: "Head", followerBone: "Head");
                    });

                }
                else if (slot.EquipSlotType == EquipSlotTypes.OldShittyFacegen)
                {
                    slot.AccessAllModels(model =>
                    {
                        model.BoneGluer = new NewBoneGluer(leader: MODEL, follower: model, 
                            mode: NewBoneGluer.GlueModes.ShiftEntireSkeleton, NewBoneGluer.GlueMethods.MatchEverything);
                        model.BoneGluer.AddGlueEntry(leaderBone: "Head", followerBone: "Head");
                    });

                }
                else if (slot.EquipSlotType == EquipSlotTypes.Arms && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var bodySlot = GetArmorSlot(EquipSlotTypes.Body);
                    bodySlot.AccessAllModels(bodyModel =>
                    {
                        slot.AccessAllModels(armsModel =>
                        {
                            armsModel.BoneGluer = new NewBoneGluer(leader: bodyModel, follower: armsModel, 
                                mode: NewBoneGluer.GlueModes.ShiftChildBones, NewBoneGluer.GlueMethods.MatchEverything);
                            // armsModel.BoneGluer.AddGlueEntry(leaderBone: "Offset_L_Clavicle", followerBone: "Offset_L_Clavicle");
                            // armsModel.BoneGluer.AddGlueEntry(leaderBone: "Offset_R_Clavicle", followerBone: "Offset_R_Clavicle");

                            armsModel.BoneGluer.AddGlueEntry(leaderBone: "Offset_L_Clavicle", followerBone: "Offset_L_Clavicle");
                            armsModel.BoneGluer.AddGlueEntry(leaderBone: "Offset_R_Clavicle", followerBone: "Offset_R_Clavicle");
                        });
                    });
                    
                }
                else if (slot.EquipSlotType == EquipSlotTypes.Body && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var legsSlot = GetArmorSlot(EquipSlotTypes.Legs);
                    legsSlot.AccessAllModels(legsModel =>
                    {
                        slot.AccessAllModels(bodyModel =>
                        {
                            bodyModel.BoneGluer = new NewBoneGluer(leader: legsModel, follower: bodyModel, 
                                mode: NewBoneGluer.GlueModes.ShiftEntireSkeleton, NewBoneGluer.GlueMethods.MatchEverything);
                            bodyModel.BoneGluer.AddGlueEntry(leaderBone: "BD_Root", followerBone: "BD_Root");
                            //bodyModel.BoneGluer.AddGlueEntry(leaderBone: "SpineLink", followerBone: "SpineLink");
                        });
                    });

                    
                }
                else if (slot.EquipSlotType == EquipSlotTypes.Head && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var bodySlot = GetArmorSlot(EquipSlotTypes.Body);
                    bodySlot.AccessAllModels(bodyModel =>
                    {
                        slot.AccessAllModels(headModel =>
                        {
                            headModel.BoneGluer = new NewBoneGluer(leader: bodyModel, follower: headModel, 
                                mode: NewBoneGluer.GlueModes.ShiftChildBones, NewBoneGluer.GlueMethods.MatchEverything);
                            headModel.BoneGluer.AddGlueEntry(leaderBone: "Neck", followerBone: "Neck");
                            //headModel.BoneGluer.AddGlueEntry(leaderBone: "Head", followerBone: "Head");

                            //headModel.EquipPartsSkeletonRemapper = new ModelSkeletonRemapper(leader: bodyModel, follower: headModel);
                        });
                    });

                }

                // else if (slot.EquipSlotType == EquipSlotTypes.Legs && GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                // {
                //     
                //         slot.AccessAllModels(legsModel =>
                //         {
                //             legsModel.UpdateSkeleton();
                //
                //             legsModel.EquipPartsSkeletonRemapper = new ModelSkeletonRemapper(leader: MODEL, follower: legsModel);
                //         });
                //
                // }
            });

            lock (_lock_boneMappersAndGluersUpdateOrder)
            {
                boneMappersAndGluersUpdateOrder.Clear();
                var slot_Head = GetGenericEquipSlot(EquipSlotTypes.Head);
                var slot_Body = GetGenericEquipSlot(EquipSlotTypes.Body);
                var slot_Arms = GetGenericEquipSlot(EquipSlotTypes.Arms);
                var slot_Legs = GetGenericEquipSlot(EquipSlotTypes.Legs);
                if (slot_Legs != null)
                    boneMappersAndGluersUpdateOrder.Add(slot_Legs);
                if (slot_Body != null)
                    boneMappersAndGluersUpdateOrder.Add(slot_Body);
                if (slot_Arms != null)
                    boneMappersAndGluersUpdateOrder.Add(slot_Arms);
                if (slot_Head != null)
                    boneMappersAndGluersUpdateOrder.Add(slot_Head);

                ForAllArmorSlots(slot =>
                {
                    if (!boneMappersAndGluersUpdateOrder.Contains(slot))
                        boneMappersAndGluersUpdateOrder.Add(slot);
                });

                // ForAllWeaponSlots(slot =>
                // {
                //     if (!boneMappersAndGluersUpdateOrder.Contains(slot))
                //         boneMappersAndGluersUpdateOrder.Add(slot);
                // });
            }
        }

        public bool UpdateAllMappersAndGluers()
        {
            bool any = false;
            lock (_lock_boneMappersAndGluersUpdateOrder)
            {
                foreach (var slot in boneMappersAndGluersUpdateOrder)
                {
                    slot.AccessAllModels(model =>
                    {
                        if (model.SkeletonRemapper != null)
                        {
                            any = true;
                            // This writes the FK of all bones to .FKMatrix, grabbing the remapped values as needed.
                            model.SkeletonRemapper.Update();
                        }
                    });
                }


                foreach (var slot in boneMappersAndGluersUpdateOrder)
                {
                    slot.AccessAllModels(model =>
                    {
                        if (model.BoneGluer != null)
                        {
                            any = true;
                            // This modifies .FKMatrix of some bones.
                            model.BoneGluer.Update();
                        }
                    });
                }
            }

            return any;
        }

        public void CreateSkelRemappersForArmorModel(EquipSlotTypes slot)
        {
            var armorSlot = GetArmorSlot(slot);
            armorSlot.AccessAllModels(model =>
            {
                CreateSkelRemappersForArmorModel(slot, model);
            });
        }

        private void CreateSkelRemappersForArmorModel(EquipSlotTypes slotType, Model m)
        {
            if (m != null)
            {
                // if (m.EquipPartsSkeletonRemapper != null)
                //     m.EquipPartsSkeletonRemapper?.Dispose();
                var remapMode = Document.GameRoot.CurrentGameSkeletonRemapperMode;

                // if (slotType == EquipSlotTypes.Legs)
                // {
                //     remapMode = NewSkeletonMapper.RemapModes.RetargetRelativeAndDirectFKOrientation;
                // }
                
                m.SkeletonRemapper = new NewSkeletonMapper(leader: MODEL, follower: m, remapMode);
            }
        }



        public void RegenSkelRemappersAndGluers()
        {
            ForAllArmorSlots(slot =>
            {
                slot.AccessAllModels(model => CreateSkelRemappersForArmorModel(slot.EquipSlotType, model));
                
            });

            CreateGluers();
        }

        public void Update(float timeDelta, bool forceSyncUpdate)
        {
            
            if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.Wwise)
            {
                Document.SoundManager.WwiseManager.ArmorMaterial_Top = GetArmorSlot(EquipSlotTypes.Body)?.EquipParam?.DefMaterialType ?? -1;
                Document.SoundManager.WwiseManager.ArmorMaterial_Bottom = GetArmorSlot(EquipSlotTypes.Legs)?.EquipParam?.DefMaterialType ?? -1;
            }
            else if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.FMOD)
            {
                int defType = GetArmorSlot(EquipSlotTypes.Body)?.EquipParam?.DefMaterialType ?? -1;
                if (defType >= 0)
                {
                    if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3 && Document.ParamManager.SeMaterialConvertParam.ContainsKey(defType))
                        defType = Document.ParamManager.SeMaterialConvertParam[defType].MaterialSe;
                    Document.Fmod.ArmorMaterial = defType;
                }
                else
                {
                    Document.Fmod.ArmorMaterial = 0;
                }
                
            }
            else if (Document.SoundManager.EngineType is zzz_SoundManagerIns.EngineTypes.MagicOrchestra)
            {
                int defType = GetArmorSlot(EquipSlotTypes.Body)?.EquipParam?.DefMaterialType ?? -1;
                if (defType >= 0)
                {
                    Document.Fmod.ArmorMaterial = defType;
                }
                else
                {
                    Document.Fmod.ArmorMaterial = 0;
                }
            }

            
            UpdateWeaponTransforms(timeDelta);
            UpdateArmorTransforms(timeDelta);

            // This is called last and ends up applying all the needed sync updates to the equipment models.
            UpdateEquipmentAnimation(timeDelta, forceSyncUpdate);

            //ForAllWeaponModels(model =>
            //{
            //    model.DummyPolyMan?.UpdateAllHitPrims();
            //});
            //ForAllArmorModels(model =>
            //{
            //    model.DummyPolyMan?.UpdateAllHitPrims();
            //});

        }

        public enum EquipSlotTypes
        {
            Head = 0,
            Body = 1,
            Arms = 2,
            Legs = 3,
            Face = 4,
            Hair = 5,
            OldShittyFacegen = 6,

            AC6Booster_NotImlementedYet = 10,

            RightWeapon = 100,
            LeftWeapon = 101,

            AC6BackRightWeapon = 110,
            AC6BackLeftWeapon = 111,

            AC6BackRightWeaponRail = 120,
            AC6BackLeftWeaponRail = 121,

            SekiroMortalBlade = 130,
            SekiroGrapplingHook = 131,

            Facegen1 = 1000,
            Facegen2 = 1001,
            Facegen3 = 1002,
            Facegen4 = 1003,
            Facegen5 = 1004,
            Facegen6 = 1005,
            Facegen7 = 1006,
            Facegen8 = 1007,
            Facegen9 = 1008,
            Facegen10 = 1009,
            FacegenMax = Facegen10,

            
            
            Debug1 = 100000000,
            Debug2,
            Debug3,
            Debug4,
            Debug5,
        }

        public enum DS3MemeAbsorpTaeType : sbyte
        {
            None = -1,
            Left = 0,
            Right = 1,
            AnyBoth = 2,
            AnyHang = 3,
            MaintainPreviousValue = 4,
            PositionForFriedeScythe = 5,
            Unknown6 = 6,
        }

        public void ClearSekiroProstheticOverrideTae()
        {
            ForAllWeaponSlots(slot =>
            {
                //slot.SekiroProstheticOverrideDmy0 = -1;
                //slot.SekiroProstheticOverrideDmy1 = -1;
                //slot.SekiroProstheticOverrideDmy2 = -1;
                //slot.SekiroProstheticOverrideDmy3 = -1;
                slot.SekiroProstheticOverrideTaeActive = false;
            });
        }

        public void RegistSekiroProstheticOverride(EquipSlotTypes slotType, 
            int model0DummyPoly, int model1DummyPoly, int model2DummyPoly, int model3DummyPoly)
        {
            ForAllWeaponSlots(slot =>
            {
                if (slot.EquipSlotType == slotType)
                {
                    slot.SekiroProstheticOverrideDmy0 = model0DummyPoly;
                    slot.SekiroProstheticOverrideDmy1 = model1DummyPoly;
                    slot.SekiroProstheticOverrideDmy2 = model2DummyPoly;
                    slot.SekiroProstheticOverrideDmy3 = model3DummyPoly;
                    slot.SekiroProstheticOverrideTaeActive = true;
                }
            });
        }

        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeR0 = DS3PairedWpnMemeKind.None;
        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeR1 = DS3PairedWpnMemeKind.None;
        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeR2 = DS3PairedWpnMemeKind.None;
        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeR3 = DS3PairedWpnMemeKind.None;
        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeL0 = DS3PairedWpnMemeKind.None;
        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeL1 = DS3PairedWpnMemeKind.None;
        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeL2 = DS3PairedWpnMemeKind.None;
        //public DS3PairedWpnMemeKind DS3PairedWeaponMemeL3 = DS3PairedWpnMemeKind.None;

        public void ClearDS3PairedWeaponMeme()
        {
            ForAllWeaponSlots(slot =>
            {
                slot.Model0MemeAbsorp = DS3MemeAbsorpTaeType.None;
                slot.Model1MemeAbsorp = DS3MemeAbsorpTaeType.None;
                slot.Model2MemeAbsorp = DS3MemeAbsorpTaeType.None;
                slot.Model3MemeAbsorp = DS3MemeAbsorpTaeType.None;
            });
        }

        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterR0 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;
        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterR1 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;
        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterR2 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;
        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterR3 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;
        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterL0 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;
        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterL1 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;
        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterL2 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;
        //public ParamData.WepAbsorpPosParam.WepInvisibleTypes WepInvisibleTypeFilterL3 = ParamData.WepAbsorpPosParam.WepInvisibleTypes.Undefined;

        public void SetWeaponVisibleByUser(EquipSlotTypes slot, bool isVisible)
        {
            var wpn = GetWpnSlot(slot);
            wpn.SetWeaponVisibleByUser(isVisible, isVisible, isVisible, isVisible);
        }

        public void ClearAllWeaponHiddenByTae()
        {
            ForAllWeaponSlots(slot => slot.SetWeaponHide_Tae(false, false, false, false));
        }

        public void SetWeaponHiddenByTae(EquipSlotTypes slot, bool? hide0, bool? hide1, bool? hide2, bool? hide3)
        {
            var wpn = GetWpnSlot(slot);
            wpn.SetWeaponHide_Tae(hide0, hide1, hide2, hide3);
        }

        public void SetWeaponHiddenByAbsorpPos(EquipSlotTypes slot, bool? hide0, bool? hide1, bool? hide2, bool? hide3)
        {
            var wpn = GetWpnSlot(slot);
            wpn.SetWeaponHide_AbsorpPos(hide0, hide1, hide2, hide3);
        }

        public enum WeaponStyleType : int
        {
            None = 0,
            OneHand = 1,
            LeftBoth = 2,
            RightBoth = 3,
            OneHandTransformedL = 4,
            OneHandTransformedR = 5,
        }

        public enum BB_WeaponState
        {
            Sheathed,
            FormA,
            FormB,
        }

        public bool IsFemale;
        public ParamData.PartSuffixType CurrentPartSuffixType = ParamData.PartSuffixType.None;
        public bool EnablePartsFileCaching = true;

        private object _lock_ArmorSlots = new object();
        public List<NewEquipSlot_Armor> ArmorSlots = new List<NewEquipSlot_Armor>();
        public Dictionary<EquipSlotTypes, NewEquipSlot_Armor> ArmorSlots_ByEquipSlot = new Dictionary<EquipSlotTypes, NewEquipSlot_Armor>();

        //TODO:BoosterModel

        //public Model AC6BackRightWeaponRailModel = null;
        //public Model AC6BackLeftWeaponRailModel = null;

        public void ForAllArmorSlots(Action<NewEquipSlot_Armor> doAction)
        {
            lock (_lock_ArmorSlots)
            {
                foreach (var slot in ArmorSlots)
                {
                    doAction(slot);
                }
            }
        }

        public void ForAllArmorModels(Action<Model> doAction)
        {
            ForAllArmorSlots(slot => slot.AccessAllModels(model => doAction(model)));
        }

        public NewAnimSkeleton_FLVER Skeleton { get; private set; } = null;

        //private Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

        public zzz_DocumentIns Document;

        public Model MODEL;

        public List<NewEquipSlot_Weapon> WeaponSlots = new List<NewEquipSlot_Weapon>();
        //public List<NewChrAsmArmorSlot> NewArmorSlots = new List<NewChrAsmArmorSlot>();

        private static Dictionary<EquipSlotTypes, NewEquipSlot_Weapon> WeaponSlots_ByEquipSlot = new Dictionary<EquipSlotTypes, NewEquipSlot_Weapon>();


        public void SetWeaponID(EquipSlotTypes slotType, int id)
        {
            WeaponSlots_ByEquipSlot[slotType].EquipID = id;
        }

        public int GetWeaponID(EquipSlotTypes slotType)
        {
            return WeaponSlots_ByEquipSlot[slotType].EquipID;
        }

        private void InitWeaponSlots()
        {
            WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.RightWeapon, "Right Weapon", "R WPN"));
            WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.LeftWeapon, "Left Weapon", "L WPN"));
            if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.AC6BackRightWeapon, "Back-Right Weapon", "BK-R WPN"));
                WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.AC6BackLeftWeapon, "Back-Left Weapon", "BK-L WPN"));

                WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.AC6BackRightWeaponRail, "Back-Right Weapon Rail", "BK-R RAIL"));
                WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.AC6BackLeftWeaponRail, "Back-Left Weapon Rail", "BK-L RAIL"));
                
            }

            if (Document.GameRoot.GameType is SoulsGames.SDT)
            {
                WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.SekiroMortalBlade, "Mortal Blade", "MB"));
                WeaponSlots.Add(new NewEquipSlot_Weapon(this, EquipSlotTypes.SekiroGrapplingHook, "Grappling Hook", "GH"));
            }
            //ac6 additional weapons
            //if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            //{
            //    WeaponSlots.Add(new NewChrAsmWpnSlot(this, EquipSlot.RightBackWeapon, "Right Back Weapon", "R Back WPN"));
            //    WeaponSlots.Add(new NewChrAsmWpnSlot(this, EquipSlot.LeftBackWeapon, "Left Back Weapon", "L Back WPN"));
            //}

            WeaponSlots_ByEquipSlot.Clear();
            foreach (var slot in WeaponSlots)
            {
                WeaponSlots_ByEquipSlot[slot.EquipSlotType] = slot;
            }

            if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                WeaponSlots_ByEquipSlot[EquipSlotTypes.RightWeapon].SlotDisplayName = "Right Front Weapon";
                WeaponSlots_ByEquipSlot[EquipSlotTypes.RightWeapon].SlotDisplayNameShort = "R Front Weapon";

                WeaponSlots_ByEquipSlot[EquipSlotTypes.LeftWeapon].SlotDisplayName = "Left Front Weapon";
                WeaponSlots_ByEquipSlot[EquipSlotTypes.LeftWeapon].SlotDisplayNameShort = "L Front Weapon";
            }


            ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Head, "Head", "HD", usesEquipParam: true));
            ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Body, "Body", "BD", usesEquipParam: true));
            ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Arms, "Arms", "AM", usesEquipParam: true));
            ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Legs, "Legs", "LG", usesEquipParam: true));

            ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Face, "Face", "FC", usesEquipParam: false));
            ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Hair, "Hair", "HR", usesEquipParam: false));

            if (Document.GameRoot.GameTypeUsesOldShittyFacegen)
            {
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.OldShittyFacegen, "Legacy Facegen", "FG", usesEquipParam: false));
            }
            else
            {
                

                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen1, "Facegen 1", "FG1", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen2, "Facegen 2", "FG2", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen3, "Facegen 3", "FG3", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen4, "Facegen 4", "FG4", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen5, "Facegen 5", "FG5", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen6, "Facegen 6", "FG6", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen7, "Facegen 7", "FG7", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen8, "Facegen 8", "FG8", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen9, "Facegen 9", "FG9", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Facegen10, "Facegen 10", "FG10", usesEquipParam: false));
            }

            if (Main.IsDebugBuild)
            {
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Debug1, "Debug 1", "DBG1", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Debug2, "Debug 2", "DBG2", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Debug3, "Debug 3", "DBG3", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Debug4, "Debug 4", "DBG4", usesEquipParam: false));
                ArmorSlots.Add(new NewEquipSlot_Armor(this, EquipSlotTypes.Debug5, "Debug 5", "DBG5", usesEquipParam: false));
            }

            //NewArmorSlots.Add(new NewChrAsmArmorSlot(this, EquipSlot.Head, "Head", "HD"));
            //NewArmorSlots.Add(new NewChrAsmArmorSlot(this, EquipSlot.Body, "Body", "BD"));
            //NewArmorSlots.Add(new NewChrAsmArmorSlot(this, EquipSlot.Arms, "Arms", "AM"));
            //NewArmorSlots.Add(new NewChrAsmArmorSlot(this, EquipSlot.Legs, "Legs", "LG"));

            ArmorSlots_ByEquipSlot.Clear();
            foreach (var slot in ArmorSlots)
            {
                ArmorSlots_ByEquipSlot[slot.EquipSlotType] = slot;
            }
        }

        public NewChrAsm(zzz_DocumentIns doc, Model mdl)
        {
            MODEL = mdl;
            Document = doc;
            InitWeaponSlots();
        }

        public void UpdateArmorTransforms(float timeDelta)
        {
            ForAllArmorModels(m =>
            {
                m.CurrentTransform = m.StartTransform = MODEL.CurrentTransform;
                //m.SkeletonFlver?.RevertToReferencePose();
                
                //m.BoneGluer?.ForceGlueUpdate();
            });
        }

        public void UpdateWeaponTransforms(float timeDelta)
        {
            if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                WeaponStyle = WeaponStyleType.None;
            }
            else
            {
                AC6LeftWeaponBaySwapped = false;
                AC6RightWeaponBaySwapped = false;
            }

            var LeftWeaponFlipBackwards = Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.SDT;
            var LeftWeaponFlipSideways = false;
            var RightWeaponFlipBackwards = Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.SDT;
            var RightWeaponFlipSideways = false;


            var isOldGame = Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 ||
                Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ||
                Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES;



            //todo: test new backwards/sideways memes



            foreach (var slot in WeaponSlots)
            {
                //slot.WeaponFlipBackwards = false;
                //slot.WeaponFlipSideways = false;
                //slot.WeaponFlipUpsideDown = false;

                //if (slot.WeaponModel0 != null && slot.WeaponModel0.AnimContainer == null)
                //    slot.WeaponModel0.ApplyBindPose = false;

                //if (slot.WeaponModel1 != null && slot.WeaponModel1.AnimContainer == null)
                //    slot.WeaponModel1.ApplyBindPose = false;

                //if (slot.WeaponModel2 != null && slot.WeaponModel2.AnimContainer == null)
                //    slot.WeaponModel2.ApplyBindPose = false;

                //if (slot.WeaponModel3 != null && slot.WeaponModel3.AnimContainer == null)
                //    slot.WeaponModel3.ApplyBindPose = false;

                //slot.WeaponModel0?.AnimContainer?.Update();
                //slot.WeaponModel1?.AnimContainer?.Update();
                //slot.WeaponModel2?.AnimContainer?.Update();
                //slot.WeaponModel3?.AnimContainer?.Update();



                slot.UpdateAbsorp(WeaponStyle, this);
            }

            if (Document.GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DS3)
            {
                ClearDS3PairedWeaponMeme();
            }

            void DoWPN(ParamData.EquipParamWeapon wpn, Model wpnMdl, int modelIdx, int defaultBoneIndex, NewEquipSlot_Weapon wpnSlot)
            {
                if (wpn == null || wpnMdl == null)
                    return;

                if (wpnMdl != null)
                {
                    Matrix absoluteWeaponTransform = Matrix.Identity;
                    bool hiddenByAbsorpPos = false;

                    bool directSetTransform = false;

                    int modelIdxForAbsorpPos = modelIdx;

                    //if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    //{
                    //    if (wpnSlot.EquipSlotType == EquipSlot.RightWeapon)
                    //        modelIdxForAbsorpPos = 0;
                    //    else if (wpnSlot.EquipSlotType == EquipSlot.AC6BackRightWeapon)
                    //        modelIdxForAbsorpPos = 1;
                    //    if (wpnSlot.EquipSlotType == EquipSlot.LeftWeapon)
                    //        modelIdxForAbsorpPos = 0;
                    //    if (wpnSlot.EquipSlotType == EquipSlot.AC6BackLeftWeapon)
                    //        modelIdxForAbsorpPos = 1;
                    //}
                    

                    // if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                    // {
                    //     var dummyPolyID = wpnSlot.GetFinalAbsorpModelDmyPoly(modelIdx);
                    //     dummyPolyID = dummyPolyID % 1000;
                    //     if (dummyPolyID >= 0 && MODEL.DummyPolyMan.DummyPolyByRefID.ContainsKey(dummyPolyID))
                    //     {
                    //         absoluteWeaponTransform = MODEL.DummyPolyMan.DummyPolyByRefID[dummyPolyID][0].CurrentMatrix;
                    //     }
                    //     else
                    //     {
                    //         //absoluteWeaponTransform = Matrix.CreateScale(0);
                    //         //TEST REMOVE LATER:
                    //         absoluteWeaponTransform =
                    //                 Skeleton.Bones[defaultBoneIndex].ReferenceMatrix
                    //                 * Skeleton[defaultBoneIndex];
                    //         hiddenByAbsorpPos = true;
                    //     }
                    // }
                    // else 
                    if (Document.GameRoot.GameTypeUsesWepAbsorpPosParam || Document.GameRoot.GameType is SoulsGames.BB)
                    {
                        var dummyPolyID = wpnSlot.GetFinalAbsorpModelDmyPoly(modelIdx);
                        var dummyPolyMan = GetDummyManager(ParamData.AtkParam.DummyPolySource.BaseModel);

                        Matrix hotfix_OffsetMatrix_ForWeaponFollowWeapon = Matrix.Identity;

                        //TODO: Test ER
                        if (dummyPolyID >= 1000 && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 or SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR)
                        {
                            dummyPolyMan = GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource.BaseModel, dummyPolyID, MODEL.DummyPolyMan);

                            //Fix for weapon dummypoly being relative to weapon's current transform
                            if (dummyPolyMan?.MODEL != null && dummyPolyMan.MODEL.IS_PLAYER_WEAPON)
                                hotfix_OffsetMatrix_ForWeaponFollowWeapon = dummyPolyMan.MODEL.CurrentTransform.WorldMatrix 
                                    * Matrix.Invert(MODEL.CurrentTransform.WorldMatrix);

                            dummyPolyID %= 1000;
                        }


                        if (dummyPolyID >= 0 && dummyPolyMan.NewCheckDummyPolyExists(dummyPolyID))
                        {
                            absoluteWeaponTransform = dummyPolyMan.NewGetDummyPolyByRefID(dummyPolyID)[0].CurrentMatrix
                                * hotfix_OffsetMatrix_ForWeaponFollowWeapon;

                            if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.AC6)
                            {
                                absoluteWeaponTransform = Matrix.CreateRotationX(MathHelper.Pi) * absoluteWeaponTransform;

                                //if (wpnSlot.IsLeftHandSlotType)
                                //    absoluteWeaponTransform = Matrix.CreateRotationX(MathHelper.Pi) * absoluteWeaponTransform;

                            }
                        }
                        else
                        {
                            absoluteWeaponTransform = Matrix.CreateScale(0);
                            hiddenByAbsorpPos = true;
                        }



                        if (wpnSlot.IsAbsorpSkeletonBind && Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
                        {
                            int boneIndex = dummyPolyID;
                            List<NewBone> flverBones = null;
                            wpnSlot.ManipModels(manip =>
                            {
                                if (modelIdx == 0)
                                    flverBones = manip.Model0?.SkeletonFlver?.Bones;
                                else if (modelIdx == 1)
                                    flverBones = manip.Model1?.SkeletonFlver?.Bones;
                                else if (modelIdx == 2)
                                    flverBones = manip.Model2?.SkeletonFlver?.Bones;
                                else if (modelIdx == 3)
                                    flverBones = manip.Model3?.SkeletonFlver?.Bones;
                            });
                            if (flverBones != null && boneIndex >= 0 && boneIndex < flverBones.Count)
                            {
                                var bone = flverBones[boneIndex];
                                var matchingBone = MODEL.AnimContainer.Skeleton.GetBoneByName(bone.Name);
                                if (matchingBone != null)
                                {
                                    absoluteWeaponTransform = Matrix.Invert(bone.FKMatrix) * matchingBone.FKMatrix;
                                    //absoluteWeaponTransform = matchingBone.FKMatrix * Matrix.Invert(bone.FKMatrix);
                                    //absoluteWeaponTransform = Matrix.CreateRotationY(MathHelper.Pi);
                                    //absoluteWeaponTransform = Matrix.Identity;
                                    hiddenByAbsorpPos = false;
                                    directSetTransform = true;
                                }
                            }
                        }
                    }
                    else if (Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
                        Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ||
                        Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                    {
                        // Weapon
                        if (modelIdx == 0)
                        {
                            absoluteWeaponTransform = Skeleton.Bones[defaultBoneIndex].FKMatrix;
                        }
                        // Sheath
                        else if (modelIdx == 1)
                        {
                            if (wpnSlot.IsLeftHandSlotType)
                            {
                                //TODO: Get sheath pos of left weapon here
                            }
                            else
                            {
                                //TODO: Get sheath pos of right weapon here
                            }

                            // TEMP: Just make sheaths invisible PepeHands
                            absoluteWeaponTransform = Matrix.CreateScale(0);
                            hiddenByAbsorpPos = true;
                        }
                        
                    }
                    else
                    {
                        // Make me forgetting to do this in DS3/SDT obvious by having all weapons at origin lol
                        absoluteWeaponTransform = Matrix.Identity;
                    }

                    if (wpnSlot.DebugWeaponModelPositions)
                    {
                        if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                        {
                            Vector3 offset = Vector3.Zero;
                            offset.X = (4 * modelIdx);


                            if (wpnSlot.AC6IsRailSlotType)
                                offset.Z = 4;
                            else if (wpnSlot.AC6IsBackSlotType)
                                offset.Z = 8;
                            else
                                offset.Z = 12;

                            if (wpnSlot.IsLeftHandSlotType)
                                offset.X *= -1;

                            absoluteWeaponTransform = Matrix.CreateTranslation(offset);
                            directSetTransform = false;
                        }
                        else
                        {
                            if (!wpnSlot.IsLeftHandSlotType)
                            {
                                if (modelIdx == 0)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(-0.5f, 0, 2);
                                else if (modelIdx == 1)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(-1.5f, 0, 2);
                                else if (modelIdx == 2)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(-2.5f, 0, 2);
                                else if (modelIdx == 3)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(-3.5f, 0, 2);
                                directSetTransform = false;
                            }
                            else
                            {
                                if (modelIdx == 0)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(0.5f, 0, 2);
                                else if (modelIdx == 1)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(1.5f, 0, 2);
                                else if (modelIdx == 2)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(2.5f, 0, 2);
                                else if (modelIdx == 3)
                                    absoluteWeaponTransform = Matrix.CreateTranslation(3.5f, 0, 2);
                                directSetTransform = false;
                            }
                        }
                    }

                    

                    if (hiddenByAbsorpPos)
                    {
                        wpnMdl.IsHiddenByAbsorpPos = true;
                        // Undo scale(0) matrix if debug force shown is enabled
                        if (wpnMdl.Debug_ForceShowNoMatterWhat)
                        {
                            absoluteWeaponTransform = Matrix.Identity;
                        }
                    }
                    else
                    {
                        wpnMdl.IsHiddenByAbsorpPos = false;
                    }





                    

                    //absoluteWeaponTransform = Matrix.CreateRotationX(MathHelper.PiOver2) * absoluteWeaponTransform;

                    var rback = isOldGame ? !RightWeaponFlipBackwards : RightWeaponFlipBackwards;
                    var rside = isOldGame ? !RightWeaponFlipSideways : !RightWeaponFlipSideways;
                    var lback = isOldGame ? !LeftWeaponFlipBackwards : LeftWeaponFlipBackwards;
                    var lside = isOldGame ? LeftWeaponFlipSideways : !LeftWeaponFlipSideways;

                    if (wpnSlot.IsLeftHandSlotType)
                    {
                        wpnSlot.WeaponFlipBackwards = lback;
                        wpnSlot.WeaponFlipSideways = lside;
                    }
                    else
                    {
                        wpnSlot.WeaponFlipBackwards = rback;
                        wpnSlot.WeaponFlipSideways = rside;
                    }

                    if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                        wpnSlot.WeaponFlipUpsideDown = true;
                    else
                        wpnSlot.WeaponFlipUpsideDown = false;

                    if (directSetTransform)
                    {
                        wpnMdl.StartTransform = wpnMdl.CurrentTransform = new Transform(absoluteWeaponTransform * MODEL.CurrentTransform.WorldMatrix);
                    }
                    else
                    {
                        wpnMdl.StartTransform = wpnMdl.CurrentTransform = new Transform(
                            (wpnSlot.WeaponFlipBackwards ? Matrix.CreateRotationX(MathHelper.Pi) : Matrix.Identity)
                            * (wpnSlot.WeaponFlipSideways ? Matrix.CreateRotationY(MathHelper.Pi) : Matrix.Identity)
                            * (wpnSlot.WeaponFlipUpsideDown ? Matrix.CreateRotationZ(MathHelper.Pi) : Matrix.Identity)
                            * absoluteWeaponTransform * MODEL.CurrentTransform.WorldMatrix);
                    }

                    

                    wpnMdl.AnimContainer.ResetRootMotion();

                    //wpnMdl.NewUpdateByAnimTick();
                }
            }


            // For AC6, update weapon rails before everything else.
            if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                foreach (var slot in WeaponSlots)
                {
                    if (slot.EquipSlotType is EquipSlotTypes.AC6BackLeftWeaponRail or EquipSlotTypes.AC6BackRightWeaponRail)
                    {
                        slot.AccessModel(0, model => DoWPN(slot.EquipParam, model, 0, slot.WeaponBoneIndex, slot));
                        slot.AccessModel(1, model => DoWPN(slot.EquipParam, model, 1, slot.WeaponBoneIndex, slot));
                        slot.AccessModel(2, model => DoWPN(slot.EquipParam, model, 2, slot.WeaponBoneIndex, slot));
                        slot.AccessModel(3, model => DoWPN(slot.EquipParam, model, 3, slot.WeaponBoneIndex, slot));
                    }
                    
                }
            }
            

            foreach (var slot in WeaponSlots)
            {
                if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6 && slot.EquipSlotType is EquipSlotTypes.AC6BackLeftWeaponRail or EquipSlotTypes.AC6BackRightWeaponRail)
                    continue;

                slot.AccessModel(0, model => DoWPN(slot.EquipParam, model, 0, slot.WeaponBoneIndex, slot));
                slot.AccessModel(1, model => DoWPN(slot.EquipParam, model, 1, slot.WeaponBoneIndex, slot));
                slot.AccessModel(2, model => DoWPN(slot.EquipParam, model, 2, slot.WeaponBoneIndex, slot));
                slot.AccessModel(3, model => DoWPN(slot.EquipParam, model, 3, slot.WeaponBoneIndex, slot));
            }
        }

        public void SelectPartsAnimations(SplitAnimID playerTaeAnimID)
        {
            foreach (var slot in WeaponSlots)
            {
                slot.SelectAnimation(Document, playerTaeAnimID);
            }

            ForAllArmorModels(m =>
            {
                m.TaeManager_ForParts?.SelectTaeAnimation(Document, playerTaeAnimID, forceNew: true);
            });
        }
        
        private void DoEquipmentModelAnimUpdate(Model model, float timeDelta, bool forceSyncUpdate, bool isArmor)
        {
            
            if (model == null)
                return;

            //if (model.AnimContainer.Skeleton?.OriginalHavokSkeleton == null)
            //{
            //    model.SkeletonFlver?.RevertToReferencePose();
            //}


            if (model != null && model.AnimContainer != null)
            {
                if (!model.ApplyBindPose)
                {
                    if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                    {
                        model.AnimContainer.EnableLooping = true;
                        model.NewScrubSimTime(absolute: false, timeDelta, foreground: true, background: true, out _, forceSyncUpdate: forceSyncUpdate);
                    }
                    else
                    {
                        

                        if (isArmor)
                        {
                            model.AnimContainer.EnableLooping = MODEL.AnimContainer.EnableLooping;
                            model.NewScrubSimTime(absolute: false, timeDelta, foreground: false, background: true, out _, forceSyncUpdate: forceSyncUpdate);

                            var unloopedTime = MODEL.AnimContainer.CurrentAnimTime;

                            MODEL.AnimContainer.AccessAnimSlots(slots =>
                            {
                                if (slots.ContainsKey(NewAnimSlot.SlotTypes.Base))
                                {
                                    var foregroundAnim = slots[NewAnimSlot.SlotTypes.Base].GetForegroundAnimation();
                                    if (foregroundAnim != null)
                                    {
                                        unloopedTime = foregroundAnim.CurrentTimeUnlooped;
                                    }
                                }
                            });

                            model.NewScrubSimTime(absolute: true, unloopedTime, foreground: true, background: false, out _, forceSyncUpdate: forceSyncUpdate);
                        }
                        else
                        {
                            model.AnimContainer.EnableLooping = false;
                            model.NewScrubSimTime(absolute: false, timeDelta, foreground: false, background: true, out _, forceSyncUpdate: forceSyncUpdate);

                            if (MODEL.AnimContainer.CurrentAnimDuration.HasValue && model.AnimContainer.CurrentAnimDuration.HasValue)
                            {
                                float curModTime = MODEL.AnimContainer.CurrentAnimTime % MODEL.AnimContainer.CurrentAnimDuration.Value;
                                // Limit time
                                if (curModTime > model.AnimContainer.CurrentAnimDuration.Value)
                                    curModTime = model.AnimContainer.CurrentAnimDuration.Value;

                                model.NewScrubSimTime(absolute: true, curModTime, foreground: true, background: false, out _, forceSyncUpdate: forceSyncUpdate);
                            }
                        }

                    }
                    
                }

                //if (model.TaeManager_ForParts != null)
                //    model.TaeManager_ForParts.UpdateTae();

            }

            //model?.NewUpdateByAnimTick();


        }

        public void UpdateEquipmentAnimation(float timeDelta, bool forceSyncUpdate)
        {
            ForAllWeaponModels(model => DoEquipmentModelAnimUpdate(model, timeDelta, forceSyncUpdate, isArmor: false));
            ForAllArmorModels(model => DoEquipmentModelAnimUpdate(model, timeDelta, forceSyncUpdate, isArmor: true));
        }

        public void InitSkeleton(NewAnimSkeleton_FLVER skeleton)
        {
            Skeleton = skeleton;
            //boneIndexRemap = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.Bones.Count; i++)
            {
                if (skeleton.Bones[i].Name == "R_Weapon")
                {
                    var rightWeapon = GetWpnSlot(EquipSlotTypes.RightWeapon);
                    rightWeapon.WeaponBoneIndex = i;
                }
                else if (skeleton.Bones[i].Name == "L_Weapon")
                {
                    var leftWeapon = GetWpnSlot(EquipSlotTypes.LeftWeapon);
                    leftWeapon.WeaponBoneIndex = i;
                }
                //if (!boneIndexRemap.ContainsKey(skeleton.FlverSkeleton[i].Name))
                //{
                //    boneIndexRemap.Add(skeleton.FlverSkeleton[i].Name, i);
                //}
            }
        }

        //public void InitWeapon(bool isLeft)
        //{
        //    var wpn = isLeft ? LeftWeapon : RightWeapon;
        //    var wpnMdl = isLeft ? LeftWeaponModel : RightWeaponModel;

        //    //if (wpn != null && wpnMdl != null)
        //    //{
        //    //    MODEL.DummyPolyMan.ClearAllHitboxPrimitives();
        //    //    MODEL.DummyPolyMan.BuildAllHitboxPrimitives(wpn, isLeft);
        //    //}
        //}

        public Model LoadArmorMesh(IBinder partsbnd, bool ignoreBindIDs)
        {
            // doesn't need _lock_doingAnythingWithModels because the thing it's called from has that and it's private.
            List<TPF> tpfs = new List<TPF>();
            FLVER2 flver2 = null;
            FLVER0 flver0 = null;

            //IBinder anibnd = null;

            foreach (var f in partsbnd.Files)
            {
                string nameCheck = f.Name.ToLower();
                if (nameCheck.EndsWith(".tpf") || TPF.Is(f.Bytes))
                {
                    try
                    {
                        var readTpf = TPF.Read(f.Bytes);
                        tpfs.Add(readTpf);
                    }
                    catch (Exception ex)
                    {
                        zzz_NotificationManagerIns.PushNotification($"Failed to read TPF '{f.Name}' from inside binder. Exception:\n{ex}");
                    }
                }
                else if (Document.GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DES && (flver2 == null && nameCheck.EndsWith(".flver") || FLVER2.Is(f.Bytes)))
                {
                    try
                    {
                        var readFlver2 = FLVER2.Read(f.Bytes);
                        flver2 = readFlver2;
                    }
                    catch (Exception ex)
                    {
                        zzz_NotificationManagerIns.PushNotification($"Failed to read FLVER2 '{f.Name}' from inside binder. Exception:\n{ex}");
                        flver2 = null;
                    }
                }
                else if (Document.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES && (flver0 == null && nameCheck.EndsWith(".flver") || FLVER0.Is(f.Bytes)))
                {
                    flver0 = FLVER0.Read(f.Bytes);
                    try
                    {
                        var readFlver0 = FLVER0.Read(f.Bytes);
                        flver0 = readFlver0;
                    }
                    catch (Exception ex)
                    {
                        zzz_NotificationManagerIns.PushNotification($"Failed to read FLVER0 '{f.Name}' from inside binder. Exception:\n{ex}");
                        flver0 = null;
                    }
                }

                //if (nameCheck.EndsWith(".anibnd"))
                //{
                //    if (BND3.IsRead(f.Bytes, out BND3 asBND3))
                //        anibnd = asBND3;
                //    else if (BND4.IsRead(f.Bytes, out BND4 asBND4))
                //        anibnd = asBND4;
                //}
            }

            foreach (var tpf in tpfs)
            {
                Document.TexturePool.AddTpf(tpf);
            }

            Model mdl = new Model(Document, null, "ArmorModel", partsbnd, modelIndex: ignoreBindIDs ? -1 : 0, null, ignoreStaticTransforms: true, 
                isBodyPart: true);

            mdl.ModelType = Model.ModelTypes.ChrAsmChildModel;
            mdl.PARENT_PLAYER_MODEL = MODEL;

            //mdl.SkeletonFlver.RevertToReferencePose();

            //NewMesh mesh = null;

            //if (GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DES && flver2 != null)
            //{
            //    mesh = new NewMesh(flver2, false, boneIndexRemap);
            //}
            //else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES && flver0 != null)
            //{
            //    mesh = new NewMesh(flver0, false, boneIndexRemap);
            //}
            Document.Scene.RequestTextureLoad();

            return mdl;
        }

        



        public void Draw(bool[] mask, int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            bool erHideGauntletsForBeastClaw = false;

            //try
            //{
            //    //UpdateWeaponTransforms(0);
            //    //UpdateArmorTransforms(0);
            //    MODEL.NewUpdate(0);
            //}
            //catch (Exception handled_ex) when (Main.EnableErrorHandler.NewChrAsm_Draw_AfterAnimUpdateCall)
            //{
            //    Main.HandleError(nameof(Main.EnableErrorHandler.NewChrAsm_Draw_AfterAnimUpdateCall), handled_ex);
            //}

            if (Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
            {
                ForAllWeaponSlots(slot =>
                {
                    if (slot.IsERHideGlovesModel)
                    {
                        erHideGauntletsForBeastClaw = true;
                    }
                });
            }

            ForAllArmorSlots(slot =>
            {
                if (slot.EquipSlotType == EquipSlotTypes.Arms && (Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR) && erHideGauntletsForBeastClaw)
                {
                    return;
                }

                slot.AccessAllModels(model =>
                {
                    model.Opacity = MODEL.Opacity;

                    if ((Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR) && slot.EquipSlotType >= EquipSlotTypes.Facegen1 &&
                        slot.EquipSlotType <= EquipSlotTypes.FacegenMax)
                    {
                        if (model.DrawMask == mask)
                        {
                            // Dereference.
                            model.DrawMask = new bool[mask.Length];
                        }

                        for (int i = 0; i < model.DrawMask.Length; i++)
                        {
                            if (i <= 2)
                                model.DrawMask[i] = MODEL.DrawMask[i];
                            else
                                model.DrawMask[i] = false;
                        }
                    }
                    else
                    {
                        model.DrawMask = mask;
                    }

                    model.CurrentTransform = MODEL.CurrentTransform;
                    //model.DebugAnimWeight_Deprecated = MODEL.DebugAnimWeight_Deprecated;
                    model.Draw(0, false, false, false, MODEL);
                });
                
            });


            void DrawWeapon(ParamData.EquipParamWeapon wpn, Model wpnMdl, int modelIdx, bool isLeft, NewEquipSlot_Weapon wpnSlot)
            {
                if (wpn == null || wpnMdl == null)
                {
                    return;
                }

                wpnMdl.Opacity = MODEL.Opacity;

                bool renderWpn = !wpnMdl.IsHiddenByAbsorpPos;

                

                //if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                //{
                //    renderWpn = (modelIdx == 0);
                //}
                //else if (GameRoot.GameTypeUsesWepAbsorpPosParam)
                //{
                //    var dummyPolyID = isLeft ? wpn.DS3_GetLeftWeaponDummyPoly(this, modelIdx)
                //            : wpn.DS3_GetRightWeaponDummyPoly(this, modelIdx);

                //    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT && SekiroProstheticOverrideTaeActive)
                //    {
                //        dummyPolyID = GetSekiroProstheticDummyPoly(isLeft ? 
                //            ProstheticOverrideModelType.LeftWeapon : ProstheticOverrideModelType.RightWeapon, modelIdx) ?? dummyPolyID;
                //    }

                //    // Temp workaround
                //    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER && dummyPolyID > 0)
                //    {
                //        dummyPolyID %= 1000;
                //    }

                //    if (dummyPolyID < 0)
                //    {
                //        renderWpn = false;
                //    }
                //}
                //else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                //{
                //    var dummyPolyID = isLeft ? wpn.BB_GetLeftWeaponDummyPoly(this, modelIdx)
                //            : wpn.BB_GetRightWeaponDummyPoly(this, modelIdx);

                //    if (dummyPolyID < 0)
                //    {
                //        renderWpn = false;
                //    }
                //}

                if (renderWpn)
                {
                    wpnMdl?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL);
                }
                else if (wpnSlot.DebugWeaponModelPositions)
                {
                    bool prevForceDraw = wpnMdl.Debug_ForceShowNoMatterWhat;
                    wpnMdl.Debug_ForceShowNoMatterWhat = true;
                    float prevOpacity = wpnMdl.Opacity;
                    wpnMdl.Opacity = 0.2f;

                    wpnMdl?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL);
                    wpnMdl.Opacity = prevOpacity;
                    wpnMdl.Debug_ForceShowNoMatterWhat = prevForceDraw;
                }
            }

            foreach (var slot in WeaponSlots)
            {
                slot.AccessModel(0, model => DrawWeapon(slot.EquipParam, model, 0, slot.IsLeftHandSlotType, slot));
                slot.AccessModel(1, model => DrawWeapon(slot.EquipParam, model, 1, slot.IsLeftHandSlotType, slot));
                slot.AccessModel(2, model => DrawWeapon(slot.EquipParam, model, 2, slot.IsLeftHandSlotType, slot));
                slot.AccessModel(3, model => DrawWeapon(slot.EquipParam, model, 3, slot.IsLeftHandSlotType, slot));
            }
        }

        public WeaponStyleType StartWeaponStyle = WeaponStyleType.OneHand;
        public WeaponStyleType WeaponStyle = WeaponStyleType.OneHand;

        public BB_WeaponState BB_RightWeaponState
        {
            get
            {
                if (WeaponStyle == WeaponStyleType.OneHand || WeaponStyle == WeaponStyleType.OneHandTransformedL)
                    return BB_WeaponState.FormA;
                else if (WeaponStyle == WeaponStyleType.RightBoth || WeaponStyle == WeaponStyleType.OneHandTransformedR)
                    return BB_WeaponState.FormB;
                else
                    return BB_WeaponState.Sheathed;
            }
        }

        public BB_WeaponState BB_LeftWeaponState
        {
            get
            {
                if (WeaponStyle == WeaponStyleType.OneHand || WeaponStyle == WeaponStyleType.OneHandTransformedR)
                    return BB_WeaponState.FormA;
                else if (WeaponStyle == WeaponStyleType.LeftBoth || WeaponStyle == WeaponStyleType.OneHandTransformedL)
                    return BB_WeaponState.FormB;
                else
                    return BB_WeaponState.Sheathed;
            }
        }


        public event EventHandler<EquipSlotTypes> EquipmentChanged;

        private void OnEquipmentChanged(EquipSlotTypes slot)
        {
            EquipmentChanged?.Invoke(this, slot);
        }

        public event EventHandler EquipmentModelsUpdated;
        private void OnEquipmentModelsUpdated()
        {
            EquipmentModelsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateMasks()
        {
            MODEL.ResetDrawMaskToAllVisible();

            //if (Head != null)
            //    MODEL.DefaultDrawMask = Head.ApplyInvisFlagsToMask(MODEL.DrawMask);
            //if (Body != null)
            //    MODEL.DefaultDrawMask = Body.ApplyInvisFlagsToMask(MODEL.DrawMask);
            //if (Arms != null)
            //    MODEL.DefaultDrawMask = Arms.ApplyInvisFlagsToMask(MODEL.DrawMask);
            //if (Legs != null)
            //    MODEL.DefaultDrawMask = Legs.ApplyInvisFlagsToMask(MODEL.DrawMask);

            bool erHideGauntletsForBeastClaw = false;

            if (Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR)
            {
                ForAllWeaponSlots(slot =>
                {
                    if (slot.IsERHideGlovesModel)
                    {
                        erHideGauntletsForBeastClaw = true;
                    }
                    if (slot.IsERApplyHideMask)
                    {
                        var both = (WeaponStyle is WeaponStyleType.LeftBoth && slot.IsLeftHandSlotType) || 
                        (WeaponStyle is WeaponStyleType.RightBoth && !slot.IsLeftHandSlotType);

                        var oneL = slot.IsLeftHandSlotType && (WeaponStyle is WeaponStyleType.OneHand or WeaponStyleType.OneHandTransformedL or WeaponStyleType.OneHandTransformedR);
                        var oneR = !slot.IsLeftHandSlotType && (WeaponStyle is WeaponStyleType.OneHand or WeaponStyleType.OneHandTransformedL or WeaponStyleType.OneHandTransformedR);

                        var absorp = slot.AbsorpPosParam;
                        if (oneL || both)
                        {
                            if (absorp.ERHideMaskLH1 >= 0 && absorp.ERHideMaskLH1 < MODEL.DrawMask.Length)
                                MODEL.DrawMask[absorp.ERHideMaskLH1] = false;
                            if (absorp.ERHideMaskLH2 >= 0 && absorp.ERHideMaskLH2 < MODEL.DrawMask.Length)
                                MODEL.DrawMask[absorp.ERHideMaskLH2] = false;
                        }

                        if (oneR || both)
                        {
                            if (absorp.ERHideMaskRH1 >= 0 && absorp.ERHideMaskRH1 < MODEL.DrawMask.Length)
                                MODEL.DrawMask[absorp.ERHideMaskRH1] = false;
                            if (absorp.ERHideMaskRH2 >= 0 && absorp.ERHideMaskRH2 < MODEL.DrawMask.Length)
                                MODEL.DrawMask[absorp.ERHideMaskRH2] = false;
                        }
                    }
                });
            }

            ForAllArmorSlots(slot =>
            {
                if (slot.EquipSlotType == EquipSlotTypes.Arms && (Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR) && erHideGauntletsForBeastClaw)
                {
                    return;
                }

                if (slot.EquipParam != null)
                    MODEL.DefaultDrawMask = slot.EquipParam.ApplyInvisFlagsToMask(MODEL.DrawMask);
            });

            //TODO:Booster

            MODEL.ResetDrawMaskToDefault();
        }

        public Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> ChrCustomize = new Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4>();

        public void UpdateModels(bool isAsync, Action onCompleteAction, bool forceReloadUnchanged, bool disableCache)
        {
            Document.LoadingTaskMan.DoLoadingTask("ChrAsm_UpdateModels", "Updating c0000 models...", progress =>
            {
                var progMax = 1.0 * GetWpnSlotCount() + GetArmorSlotCount();
                var progCurrent = 0.0;
                void incrementProg()
                {
                    progCurrent += 1.0;
                    progress.Report(progCurrent / progMax);
                }

                ForAllArmorSlots(slot =>
                {
                    bool equipChanged = false;

                    if (slot.UsesEquipParam)
                        equipChanged = slot.EquipID != slot.lastEquipIDLoaded;
                    else
                        equipChanged = slot.DirectEquip != slot.lastDirectEquipLoaded;

                    if (slot.EquipSlotType == EquipSlotTypes.OldShittyFacegen)
                    {
                        if (!slot.CheckIfModelLoaded())
                            equipChanged = true;
                    }

                        if (equipChanged || forceReloadUnchanged)
                    {
                        Model newMesh = null;
                        if (slot.CanEquipOnGender(IsFemale))
                        {
                            var partsName = Utils.GetShortIngameFileName(slot.GetPartsbndName(IsFemale, CurrentPartSuffixType));
                            var partsbnd = slot.GetPartsbnd(IsFemale, CurrentPartSuffixType, disableCache);
                            if (partsbnd != null)
                                newMesh = LoadArmorMesh(partsbnd, ignoreBindIDs: slot.EquipSlotType == EquipSlotTypes.OldShittyFacegen);

                            if (newMesh != null)
                            {
                                newMesh.Name = partsName ?? slot.SlotDisplayName;
                                newMesh.SkeletonFlver?.RevertToReferencePose();
                                newMesh.AnimContainer?.Skeleton?.RevertToReferencePose();

                                CreateSkelRemappersForArmorModel(slot.EquipSlotType, newMesh);


                                newMesh.TaeManager_ForParts = NewChrAsmWpnTaeManager.LoadPartsbnd(Document, newMesh, slot.EquipSlotType == EquipSlotTypes.OldShittyFacegen ? -1 : 0, partsbnd);
                                newMesh.AnimContainer.EquipmentTaeManager = newMesh.TaeManager_ForParts;
                            }

                            
                        }

                        slot.ManipModel(manip =>
                        {
                            var oldModel = manip.Model;
                            manip.Model = newMesh;
                            oldModel?.Dispose();
                        });

                        if (slot.UsesEquipParam)
                            slot.lastEquipIDLoaded = slot.EquipID;
                        else
                            slot.lastDirectEquipLoaded = slot.DirectEquip;
                    }
                    incrementProg();
                });

               


                void DoWeaponSlot(NewEquipSlot_Weapon slot)
                {
                    if ((slot.EquipID != slot.lastEquipIDLoaded) || forceReloadUnchanged)
                    {
                        if (slot.EquipParam != null)
                        {

                            //TODO
                            try
                            {
                                var weaponBnd = slot.EquipParam.GetPartBnd(slot.EquipSlotType, disableCache);

                                if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                                {
                                    slot.EquipParam.LoadAC6PartBndTextureFile();
                                }

                                var shortWeaponName = slot.EquipParam.GetPartBndName(slot.EquipSlotType);

                               

                                if (weaponBnd != null)
                                {
                                    Model[] newWeaponModels = new Model[4];

                                    for (int i = 0; i < 4; i++)
                                    {
                                        var newWpnMdl = new Model(Document, null, shortWeaponName, weaponBnd, modelIndex: i, null, ignoreStaticTransforms: true);
                                        if (newWpnMdl.AnimContainer == null)
                                        {
                                            newWpnMdl?.Dispose();
                                            newWpnMdl = null;
                                        }
                                        else
                                        {
                                            newWpnMdl.IS_PLAYER_WEAPON = true;
                                            newWpnMdl.ModelType = Model.ModelTypes.ChrAsmChildModel;
                                            newWpnMdl.PARENT_PLAYER_MODEL = MODEL;
                                            newWpnMdl.DummyPolyMan.GlobalDummyPolyIDOffset = (slot.IsLeftHandSlotType ? 20000 : 10000) + (i * 1000);
                                            newWpnMdl.DummyPolyMan.GlobalDummyPolyIDPrefix = slot.SlotDisplayNameShort + " - ";
                                        }
                                        newWeaponModels[i] = newWpnMdl;
                                    }



                                    slot.ManipModels(manip =>
                                    {
                                        var oldRightWeaponModel0 = manip.Model0;
                                        var oldRightWeaponModel1 = manip.Model1;
                                        var oldRightWeaponModel2 = manip.Model2;
                                        var oldRightWeaponModel3 = manip.Model3;

                                        manip.Model0 = newWeaponModels[0];
                                        manip.Model1 = newWeaponModels[1];
                                        manip.Model2 = newWeaponModels[2];
                                        manip.Model3 = newWeaponModels[3];

                                        if (manip.Model0 != null && manip.Model0.MainMesh != null)
                                            manip.Model0.MainMesh.Name = manip.Model0.Name;
                                        if (manip.Model1 != null && manip.Model1.MainMesh != null)
                                            manip.Model1.MainMesh.Name = manip.Model1.Name;
                                        if (manip.Model2 != null && manip.Model2.MainMesh != null)
                                            manip.Model2.MainMesh.Name = manip.Model2.Name;
                                        if (manip.Model3 != null && manip.Model3.MainMesh != null)
                                            manip.Model3.MainMesh.Name = manip.Model3.Name;

                                        


                                        oldRightWeaponModel0?.Dispose();
                                        oldRightWeaponModel1?.Dispose();
                                        oldRightWeaponModel2?.Dispose();
                                        oldRightWeaponModel3?.Dispose();
                                    });
                                    slot.LoadPartsbndAfterModels(Document, shortWeaponName, weaponBnd);
                                    


                                }
                            }
                            catch (Exception ex)
                            {
                                slot.lastEquipIDLoaded = -1;


                                slot.ManipModels(manip =>
                                {
                                    var oldRightWeaponModel0 = manip.Model0;
                                    var oldRightWeaponModel1 = manip.Model1;
                                    var oldRightWeaponModel2 = manip.Model2;
                                    var oldRightWeaponModel3 = manip.Model3;

                                    manip.Model0 = null;
                                    manip.Model1 = null;
                                    manip.Model2 = null;
                                    manip.Model3 = null;

                                    oldRightWeaponModel0?.Dispose();
                                    oldRightWeaponModel1?.Dispose();
                                    oldRightWeaponModel2?.Dispose();
                                    oldRightWeaponModel3?.Dispose();
                                });

                                

                                System.Windows.Forms.MessageBox.Show(
                                    $"Failed to load right-hand weapon model:\n\n{ex}",
                                    "Failed To Load RH Weapon Model",
                                    System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Error);
                            }
                            slot.lastEquipIDLoaded = slot.EquipID;
                        }
                        else
                        {
                            slot.lastEquipIDLoaded = -1;


                            slot.ManipModels(manip =>
                            {
                                var oldRightWeaponModel0 = manip.Model0;
                                var oldRightWeaponModel1 = manip.Model1;
                                var oldRightWeaponModel2 = manip.Model2;
                                var oldRightWeaponModel3 = manip.Model3;

                                manip.Model0 = null;
                                manip.Model1 = null;
                                manip.Model2 = null;
                                manip.Model3 = null;

                                oldRightWeaponModel0?.Dispose();
                                oldRightWeaponModel1?.Dispose();
                                oldRightWeaponModel2?.Dispose();
                                oldRightWeaponModel3?.Dispose();
                            });
                        }
                    }


                    incrementProg();
                }

                int wpnSlotIndex = 0;

                foreach (var slot in WeaponSlots)
                {
                    DoWeaponSlot(slot);
                    wpnSlotIndex++;
                }

                UpdateWeaponTransforms(0);
                UpdateMasks();

                //if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                //{
                //    if (HeadModel != null)
                //        HeadModel.DummyPolyMan.GlobalDummyPolyIDOffset = 1000;
                //    if (BodyModel != null)
                //        BodyModel.DummyPolyMan.GlobalDummyPolyIDOffset = 2000;
                //    if (ArmsModel != null)
                //        ArmsModel.DummyPolyMan.GlobalDummyPolyIDOffset = 3000;
                //    if (LegsModel != null)
                //        LegsModel.DummyPolyMan.GlobalDummyPolyIDOffset = 4000;
                //}

                progress.Report(1.0);
                onCompleteAction?.Invoke();

                //ForAllArmorModels(m =>
                //{
                //    if (m.EquipPartsSkeletonRemapper == null)
                //    {
                //        m.SkeletonFlver.RevertToReferencePose();

                //        m.EquipPartsSkeletonRemapper = new ModelSkeletonRemapper(
                //            ModelSkeletonRemapper.RemapTypes.FollowerHavokToLeaderHavok,
                //            leader: MODEL, follower: m);
                //    }
                //});

                RegenSkelRemappersAndGluers();

                UpdateWeaponTransforms(0);
                UpdateEquipmentAnimation(0, forceSyncUpdate: true);
                
                OnEquipmentModelsUpdated();

                FlverMaterialDefInfo.FlushBinderCache();
            }, waitForTaskToComplete: !isAsync);
                
        }

        public void AccessArmorSlot(EquipSlotTypes slot, Action<NewEquipSlot_Armor> doAction)
        {
            lock (_lock_ArmorSlots)
            {
                if (ArmorSlots_ByEquipSlot.ContainsKey(slot))
                    doAction(ArmorSlots_ByEquipSlot[slot]);
            }
        }

        public int GetWpnSlotCount()
        {
            int result = 0;
            lock (_lock_ArmorSlots)
            {
                result = WeaponSlots.Count;
            }
            return result;
        }
        public int GetArmorSlotCount()
        {
            int result = 0;
            lock (_lock_ArmorSlots)
            {
                result = ArmorSlots.Count;
            }
            return result;
        }

        public NewEquipSlot GetGenericEquipSlot(EquipSlotTypes slotType)
        {
            if (ArmorSlots_ByEquipSlot.ContainsKey(slotType))
                return ArmorSlots_ByEquipSlot[slotType];
            else if (WeaponSlots_ByEquipSlot.ContainsKey(slotType))
                return WeaponSlots_ByEquipSlot[slotType];
            return null;
        }

        public NewEquipSlot_Armor GetArmorSlot(EquipSlotTypes slotType)
        {
            return ArmorSlots_ByEquipSlot[slotType];
        }

        public NewEquipSlot_Weapon GetWpnSlot(EquipSlotTypes slotType)
        {
            return WeaponSlots_ByEquipSlot[slotType];
        }

        public ParamData.AtkParam.DummyPolySource GetDummySourceFromManager(NewDummyPolyManager manager)
        {
            if (manager == null || manager == MODEL.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.BaseModel;

            ParamData.AtkParam.DummyPolySource? result = null;

            if (Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                var headSlot = GetArmorSlot(EquipSlotTypes.Head);
                headSlot.AccessAllModels(model =>
                {
                    if (model.DummyPolyMan == manager)
                        result = ParamData.AtkParam.DummyPolySource.HeadPart;
                });
                if (result != null)
                    return result.Value;

                var bodySlot = GetArmorSlot(EquipSlotTypes.Body);
                bodySlot.AccessAllModels(model =>
                {
                    if (model.DummyPolyMan == manager)
                        result = ParamData.AtkParam.DummyPolySource.BodyPart;
                });
                if (result != null)
                    return result.Value;

                var armsSlot = GetArmorSlot(EquipSlotTypes.Arms);
                armsSlot.AccessAllModels(model =>
                {
                    if (model.DummyPolyMan == manager)
                        result = ParamData.AtkParam.DummyPolySource.ArmsPart;
                });
                if (result != null)
                    return result.Value;

                var legsSlot = GetArmorSlot(EquipSlotTypes.Legs);
                legsSlot.AccessAllModels(model =>
                {
                    if (model.DummyPolyMan == manager)
                        result = ParamData.AtkParam.DummyPolySource.LegsPart;
                });
                if (result != null)
                    return result.Value;

            }

            foreach (var slot in WeaponSlots)
            {
                if (slot.GetDummyPolyManager(0) == manager)
                    return slot.DmySource0;
                else if (slot.GetDummyPolyManager(1) == manager)
                    return slot.DmySource1;
                else if (slot.GetDummyPolyManager(2) == manager)
                    return slot.DmySource2;
                else if (slot.GetDummyPolyManager(3) == manager)
                    return slot.DmySource3;
            }


            //if (manager == HeadModel?.DummyPolyMan && GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            //    return ParamData.AtkParam.DummyPolySource.HeadPart;
            //else if (manager == BodyModel?.DummyPolyMan && GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            //    return ParamData.AtkParam.DummyPolySource.BodyPart;
            //else if (manager == ArmsModel?.DummyPolyMan && GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            //    return ParamData.AtkParam.DummyPolySource.ArmsPart;
            //else if (manager == LegsModel?.DummyPolyMan && GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            //    return ParamData.AtkParam.DummyPolySource.LegsPart;


            return ParamData.AtkParam.DummyPolySource.BaseModel;
        }

        public NewDummyPolyManager GetDummyManager(ParamData.AtkParam.DummyPolySource src)
        {


            if (src == ParamData.AtkParam.DummyPolySource.BaseModel)
                return MODEL.DummyPolyMan;

            NewDummyPolyManager result = null;
            if (src == ParamData.AtkParam.DummyPolySource.HeadPart && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                var headSlot = GetArmorSlot(EquipSlotTypes.Head);
                headSlot.AccessAllModels(headModel =>
                {
                    result = headModel.DummyPolyMan;
                });
            }
            else if (src == ParamData.AtkParam.DummyPolySource.BodyPart && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                var bodySlot = GetArmorSlot(EquipSlotTypes.Body);
                bodySlot.AccessAllModels(bodyModel =>
                {
                    result = bodyModel.DummyPolyMan;
                });
            }
            else if (src == ParamData.AtkParam.DummyPolySource.ArmsPart && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                var armsSlot = GetArmorSlot(EquipSlotTypes.Arms);
                armsSlot.AccessAllModels(armsModel =>
                {
                    result = armsModel.DummyPolyMan;
                });
            }
            else if (src == ParamData.AtkParam.DummyPolySource.LegsPart && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
            {
                var legsSlot = GetArmorSlot(EquipSlotTypes.Legs);
                legsSlot.AccessAllModels(legsModel =>
                {
                    result = legsModel.DummyPolyMan;
                });
            }

            if (result != null)
                return result;


            foreach (var slot in WeaponSlots)
            {
                if (slot.DmySource0 == src)
                    return slot.GetDummyPolyManager(0);
                else if (slot.DmySource1 == src)
                    return slot.GetDummyPolyManager(1);
                if (slot.DmySource2 == src)
                    return slot.GetDummyPolyManager(2);
                if (slot.DmySource3 == src)
                    return slot.GetDummyPolyManager(3);
            }

            return null;
        }

        public NewDummyPolyManager GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource defaultDummySource, int dmy, NewDummyPolyManager bodyDmyForFallback)
        {
            if (dmy == -1)
                return null;
            int check = dmy / 1000;

            NewDummyPolyManager wpnDmy = null;
            if (check == 0)
                wpnDmy = GetDummyManager(defaultDummySource);

            else if (check == 1 && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.HeadPart);
            else if (check == 2 && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.BodyPart);
            else if (check == 3 && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.ArmsPart);
            else if (check == 4 && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LegsPart);

            else if (check == 10)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon0);
            else if (check == 11)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon1);
            else if (check == 12 && Document.GameRoot.GameType is not SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon2);
            else if (check == 13 && Document.GameRoot.GameType is not SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon3);
            else if (check == 20)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon0);
            else if (check == 21)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon1);
            else if (check == 22 && Document.GameRoot.GameType is not SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon2);
            else if (check == 23 && Document.GameRoot.GameType is not SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon3);


            //tentatively
            else if (check == 17 && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.AC6BackRightWeaponRail);
            else if (check == 19 && Document.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.AC6)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.AC6BackLeftWeaponRail);


            else
                wpnDmy = bodyDmyForFallback;

            if (wpnDmy == null)
                wpnDmy = bodyDmyForFallback;

            if (!wpnDmy.NewCheckDummyPolyExists(dmy % 1000))
                return bodyDmyForFallback;
            else
                return wpnDmy;
        }

        public void ForAllWeaponSlots(Action<NewEquipSlot_Weapon> doAction)
        {
            foreach (var slot in WeaponSlots)
            {
                doAction(slot);
            }
        }

        public void ForAllWeaponModels(Action<Model> doAction)
        {
            foreach (var slot in WeaponSlots)
            {
                slot.AccessAllModels(model =>
                {
                    doAction(model);
                });
            }
        }
        
        public void DrawAnimLayerDebug(ref Vector2 pos)
        {
            foreach (var slot in WeaponSlots)
            {
                slot.DrawAnimLayerDebug(ref pos);
            }

            foreach (var slot in ArmorSlots)
            {
                slot.DrawAnimLayerDebug(ref pos);
            }
        }

        public void TryToLoadTextures()
        {

            ForAllArmorModels(model => model.TryToLoadTextures());

            foreach (var slot in WeaponSlots)
                slot.TryToLoadTextures();
            
        }

        public void Dispose()
        {
            foreach (var slot in ArmorSlots)
                slot.Dispose();
            ArmorSlots.Clear();
            ArmorSlots = null;


            foreach (var slot in WeaponSlots)
                slot.Dispose();
            WeaponSlots.Clear();
            WeaponSlots = null;


            this.boneMappersAndGluersUpdateOrder?.Clear();
            boneMappersAndGluersUpdateOrder = null;

            this.ChrCustomize?.Clear();
            ChrCustomize = null;

            this.EquipmentChanged = null;
            this.EquipmentModelsUpdated = null;

            if (Skeleton != null)
            {
                Skeleton.MODEL = null;
                Skeleton = null;
            }


            MODEL = null;
            Document = null;
        }
    }
}
