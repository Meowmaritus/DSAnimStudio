using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewChrAsm : IDisposable
    {
        public void Update(float timeDelta)
        {
            if (GameRoot.GameTypeUsesWwise)
            {
                Wwise.ArmorMaterial_Top = Body?.DefMaterialType ?? -1;
                Wwise.ArmorMaterial_Bottom = Legs?.DefMaterialType ?? -1;
            }
            else
            {
                FmodManager.ArmorMaterial = Body?.DefMaterialType ?? 0;
            }

            UpdateWeaponTransforms(timeDelta);
            UpdateWeaponAnimation(timeDelta);
        }

        public enum EquipSlot
        {
            Head,
            Body,
            Arms,
            Legs,
            RightWeapon,
            LeftWeapon,
            Face,
            Facegen,
            Hair,
        }

        public enum DS3PairedWpnMemeKind : sbyte
        {
            None = -1,
            OneHand_Left = 0,
            OneHand_Right = 1,
            BothHand = 2,
            Sheath = 3,
            MaintainPreviousValue = 4,
            PositionForFriedeScythe = 5,
            Unknown6 = 6,
        }

        public bool IsRightWeaponFriedesScythe()
        {
            return RightWeaponID == 10180000;
        }

        public enum ProstheticOverrideModelType
        {
            RightWeapon = 0,
            LeftWeapon = 1,
        }

        public class ProstheticOverrideEntry
        {
            public int Model0DummyPolyID = -1;
            public int Model1DummyPolyID = -1;
            public int Model2DummyPolyID = -1;
            public int Model3DummyPolyID = -1;
            public int this[int idx]
            {
                get
                {
                    if (idx == 0)
                        return Model0DummyPolyID;
                    else if (idx == 1)
                        return Model1DummyPolyID;
                    else if (idx == 2)
                        return Model2DummyPolyID;
                    else if (idx == 3)
                        return Model3DummyPolyID;
                    return -1;
                }
            }
        }

        private object _lock_ProstheticOverrides = new object();
        public void ClearSekiroProstheticOverrideTae()
        {
            
            lock (_lock_ProstheticOverrides)
            {
                SekiroProstheticOverrideTaeActive = false;
                SekiroProstheticOverrideEntries.Clear();
            }
        }

        public void RegistSekiroProstheticOverride(ProstheticOverrideModelType type, 
            int model0DummyPoly, int model1DummyPoly, int model2DummyPoly, int model3DummyPoly)
        {
            lock (_lock_ProstheticOverrides)
            {
                if (!SekiroProstheticOverrideEntries.ContainsKey(type))
                    SekiroProstheticOverrideEntries.Add(type, new ProstheticOverrideEntry()
                    {
                        Model0DummyPolyID = model0DummyPoly,
                        Model1DummyPolyID = model1DummyPoly,
                        Model2DummyPolyID = model2DummyPoly,
                        Model3DummyPolyID = model3DummyPoly,
                    });
            }
        }

        public bool SekiroProstheticOverrideTaeActive = false;

        public int? GetSekiroProstheticDummyPoly(ProstheticOverrideModelType type, int modelIdx)
        {
            int? result = null;
            lock (_lock_ProstheticOverrides)
            {
                if (SekiroProstheticOverrideEntries.ContainsKey(type))
                    result = SekiroProstheticOverrideEntries[type][modelIdx];
            }
            return result;
        }

        public bool GetSekiroProstheticVisible(ProstheticOverrideModelType type, int modelIdx)
        {
            bool result = true;
            lock (_lock_ProstheticOverrides)
            {
                result = SekiroProstheticOverrideEntries.ContainsKey(type) && SekiroProstheticOverrideEntries[type][modelIdx] >= 0;
            }
            return result;
        }

        public Dictionary<ProstheticOverrideModelType, ProstheticOverrideEntry> SekiroProstheticOverrideEntries
            = new Dictionary<ProstheticOverrideModelType, ProstheticOverrideEntry>();

        public DS3PairedWpnMemeKind DS3PairedWeaponMemeR0 = DS3PairedWpnMemeKind.None;
        public DS3PairedWpnMemeKind DS3PairedWeaponMemeR1 = DS3PairedWpnMemeKind.None;
        public DS3PairedWpnMemeKind DS3PairedWeaponMemeR2 = DS3PairedWpnMemeKind.None;
        public DS3PairedWpnMemeKind DS3PairedWeaponMemeR3 = DS3PairedWpnMemeKind.None;
        public DS3PairedWpnMemeKind DS3PairedWeaponMemeL0 = DS3PairedWpnMemeKind.None;
        public DS3PairedWpnMemeKind DS3PairedWeaponMemeL1 = DS3PairedWpnMemeKind.None;
        public DS3PairedWpnMemeKind DS3PairedWeaponMemeL2 = DS3PairedWpnMemeKind.None;
        public DS3PairedWpnMemeKind DS3PairedWeaponMemeL3 = DS3PairedWpnMemeKind.None;

        public void ClearDS3PairedWeaponMeme()
        {
            DS3PairedWeaponMemeR0 = DS3PairedWpnMemeKind.None;
            DS3PairedWeaponMemeR1 = DS3PairedWpnMemeKind.None;
            DS3PairedWeaponMemeR2 = DS3PairedWpnMemeKind.None;
            DS3PairedWeaponMemeR3 = DS3PairedWpnMemeKind.None;
            DS3PairedWeaponMemeL0 = DS3PairedWpnMemeKind.None;
            DS3PairedWeaponMemeL1 = DS3PairedWpnMemeKind.None;
            DS3PairedWeaponMemeL2 = DS3PairedWpnMemeKind.None;
            DS3PairedWeaponMemeL3 = DS3PairedWpnMemeKind.None;
        }

        public byte DS3PairedWeaponMemeR0_Flag = 255;
        public byte DS3PairedWeaponMemeR1_Flag = 255;
        public byte DS3PairedWeaponMemeR2_Flag = 255;
        public byte DS3PairedWeaponMemeR3_Flag = 255;
        public byte DS3PairedWeaponMemeL0_Flag = 255;
        public byte DS3PairedWeaponMemeL1_Flag = 255;
        public byte DS3PairedWeaponMemeL2_Flag = 255;
        public byte DS3PairedWeaponMemeL3_Flag = 255;

        public DS3PairedWpnMemeKind GetDS3PairedWpnMemeKindR(int modelIndex)
        {
            int ds3ConditionFlag = GameRoot.GameTypeUsesWepAbsorpPosParam
                ? (RightWeapon?.GetDS3TaeConditionFlag(modelIndex) ?? -1) : -1;

            if (modelIndex == 0 && DS3PairedWeaponMemeR0_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeR0;
            else if (modelIndex == 1 && DS3PairedWeaponMemeR1_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeR1;
            else if (modelIndex == 2 && DS3PairedWeaponMemeR2_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeR2;
            else if (modelIndex == 3 && DS3PairedWeaponMemeR3_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeR3;

            return DS3PairedWpnMemeKind.None;
        }

        public DS3PairedWpnMemeKind GetDS3PairedWpnMemeKindL(int modelIndex)
        {
            int ds3ConditionFlag = GameRoot.GameTypeUsesWepAbsorpPosParam
                ? (LeftWeapon?.GetDS3TaeConditionFlag(modelIndex) ?? -1) : -1;

            if (modelIndex == 0 && DS3PairedWeaponMemeL0_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeL0;
            else if (modelIndex == 1 && DS3PairedWeaponMemeL1_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeL1;
            else if (modelIndex == 2 && DS3PairedWeaponMemeL2_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeL2;
            else if (modelIndex == 3 && DS3PairedWeaponMemeL3_Flag == ds3ConditionFlag)
                return DS3PairedWeaponMemeL3;

            return DS3PairedWpnMemeKind.None;
        }

        public const int FRIEDE_SCYTHE_LH_DUMMYPOLY_ID = 21;

        public enum WeaponStyleType : int
        {
            None = 0,
            OneHand = 1,
            TwoHandL = 2,
            TwoHandR = 3,
            OneHandTransformedL = 4,
            OneHandTransformedR = 5,
        }

        public enum BB_WeaponState
        {
            Sheathed,
            FormA,
            FormB,
        }

        public object _lock_doingAnythingWithWeaponModels = new object();

        private bool _isFemale = false;
        public bool IsFemale
        {
            get
            {
                bool result = false;
                lock (_lock_EquipParam)
                {
                    result = _isFemale;
                }
                return result;
            }
            set
            {
                lock (_lock_EquipParam)
                {
                    _isFemale = value;
                }

                if (FaceIndex == -1)
                    FaceIndex = GetDefaultFaceIndexForCurrentGame(_isFemale);
                if (FaceIndex >= 0)
                    LoadNewFace(PossibleFaceModels[FaceIndex]);
            }
        }

        private int _headID = -1;
        private int _bodyID = -1;
        private int _armsID = -1;
        private int _legsID = -1;
        private int _rightWeaponID = -1;
        //private int _rightWeaponModelIndex = 0;
        private int _leftWeaponID = -1;
        //private int _leftWeaponModelIndex = 0;

        public NewMesh HeadMesh = null;
        public NewMesh BodyMesh = null;
        public NewMesh ArmsMesh = null;
        public NewMesh LegsMesh = null;
        public NewMesh FaceMesh = null;
        public NewMesh FacegenMesh = null;
        public NewMesh HairMesh = null;

        public NewAnimSkeleton_FLVER Skeleton { get; private set; } = null;

        public int RightWeaponBoneIndex = -1;
        private Model _rightWeaponModel0 = null;
        private Model _rightWeaponModel1 = null;
        private Model _rightWeaponModel2 = null;
        private Model _rightWeaponModel3 = null;

        public int LeftWeaponBoneIndex = -1;
        private Model _leftWeaponModel0 = null;
        private Model _leftWeaponModel1 = null;
        private Model _leftWeaponModel2 = null;
        private Model _leftWeaponModel3 = null;

        public Model RightWeaponModel0
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _rightWeaponModel0;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _rightWeaponModel0 = value;
                }
            }
        }

        public Model RightWeaponModel1
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _rightWeaponModel1;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _rightWeaponModel1 = value;
                }
            }
        }

        public Model RightWeaponModel2
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _rightWeaponModel2;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _rightWeaponModel2 = value;
                }
            }
        }

        public Model RightWeaponModel3
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _rightWeaponModel3;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _rightWeaponModel3 = value;
                }
            }
        }

        public void SetRightWeaponVisible(bool isVisible)
        {
            RightWeaponModel0?.SetIsVisible(isVisible);
            RightWeaponModel1?.SetIsVisible(isVisible);
            RightWeaponModel2?.SetIsVisible(isVisible);
            RightWeaponModel3?.SetIsVisible(isVisible);
        }

        public void SetLeftWeaponVisible(bool isVisible)
        {
            LeftWeaponModel0?.SetIsVisible(isVisible);
            LeftWeaponModel1?.SetIsVisible(isVisible);
            LeftWeaponModel2?.SetIsVisible(isVisible);
            LeftWeaponModel3?.SetIsVisible(isVisible);
        }

        public Model LeftWeaponModel0
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _leftWeaponModel0;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _leftWeaponModel0 = value;
                }
            }
        }

        public Model LeftWeaponModel1
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _leftWeaponModel1;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _leftWeaponModel1 = value;
                }
            }
        }

        public Model LeftWeaponModel2
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _leftWeaponModel2;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _leftWeaponModel2 = value;
                }
            }
        }

        public Model LeftWeaponModel3
        {
            get
            {
                Model result = null;
                lock (_lock_doingAnythingWithWeaponModels)
                    result = _leftWeaponModel3;
                return result;
            }
            private set
            {
                lock (_lock_doingAnythingWithWeaponModels)
                {
                    _leftWeaponModel3 = value;
                }
            }
        }

        private bool LeftWeaponFlipBackwards = false;
        private bool LeftWeaponFlipSideways = false;
        private bool RightWeaponFlipBackwards = false;
        private bool RightWeaponFlipSideways = false;

        private Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

        public readonly Model MODEL;

        public bool DebugRightWeaponModelPositions = false;
        public bool DebugLeftWeaponModelPositions = false;

        public NewChrAsm(Model mdl)
        {
            MODEL = mdl;
        }

        public void UpdateWeaponTransforms(float timeDelta)
        {
            LeftWeaponFlipBackwards = GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.SDT;
            LeftWeaponFlipSideways = false;
            RightWeaponFlipBackwards = GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.SDT;
            RightWeaponFlipSideways = false;

            if (GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DS3)
            {
                ClearDS3PairedWeaponMeme();
            }

            void DoWPN(ParamData.EquipParamWeapon wpn, Model wpnMdl, int modelIdx, int defaultBoneIndex, bool isLeft, bool backward, bool sideways)
            {
                if (wpn == null || wpnMdl == null)
                    return;

                if (wpnMdl != null)
                {
                    Matrix absoluteWeaponTransform = Matrix.Identity;

                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                    {
                        var dummyPolyID = isLeft ? wpn.BB_GetLeftWeaponDummyPoly(this, modelIdx)
                            : wpn.BB_GetRightWeaponDummyPoly(this, modelIdx);
                        dummyPolyID = dummyPolyID % 1000;
                        if (dummyPolyID >= 0 && MODEL.DummyPolyMan.DummyPolyByRefID.ContainsKey(dummyPolyID))
                        {
                            absoluteWeaponTransform = MODEL.DummyPolyMan.DummyPolyByRefID[dummyPolyID][0].CurrentMatrix;
                        }
                        else
                        {
                            //absoluteWeaponTransform = Matrix.CreateScale(0);
                            //TEST REMOVE LATER:
                            absoluteWeaponTransform =
                                    Skeleton.FlverSkeleton[defaultBoneIndex].ReferenceMatrix
                                    * Skeleton[defaultBoneIndex];
                        }
                    }
                    else if (GameRoot.GameTypeUsesWepAbsorpPosParam)
                    {
                        

                        var dummyPolyID = isLeft ? wpn.DS3_GetLeftWeaponDummyPoly(this, modelIdx)
                            : wpn.DS3_GetRightWeaponDummyPoly(this, modelIdx);


                        // Testing 
                        if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT && SekiroProstheticOverrideTaeActive)
                        {
                            dummyPolyID = GetSekiroProstheticDummyPoly(isLeft ?
                                ProstheticOverrideModelType.LeftWeapon : ProstheticOverrideModelType.RightWeapon, modelIdx) ?? dummyPolyID;
                        }

                        // Temp workaround
                        if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER && dummyPolyID > 0)
                        {
                            dummyPolyID %= 1000;
                        }

                        if (dummyPolyID >= 0 && MODEL.DummyPolyMan.DummyPolyByRefID.ContainsKey(dummyPolyID))
                        {
                            absoluteWeaponTransform = MODEL.DummyPolyMan.DummyPolyByRefID[dummyPolyID][0].CurrentMatrix;

                            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                            {
                                absoluteWeaponTransform = Matrix.CreateRotationX(MathHelper.Pi) * absoluteWeaponTransform;
                            }
                        }
                        else
                        {
                            absoluteWeaponTransform = Matrix.CreateScale(0);
                        }


                    }
                    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
                        GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ||
                        GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                    {
                        // Weapon
                        if (modelIdx == 0)
                        {
                            absoluteWeaponTransform = Skeleton.FlverSkeleton[defaultBoneIndex].ReferenceMatrix
                                * Skeleton[defaultBoneIndex];
                        }
                        // Sheath
                        else if (modelIdx == 1)
                        {
                            if (isLeft)
                            {
                                //TODO: Get sheath pos of left weapon here
                            }
                            else
                            {
                                //TODO: Get sheath pos of right weapon here
                            }

                            // TEMP: Just make sheaths invisible PepeHands
                            absoluteWeaponTransform = Matrix.CreateScale(0);
                        }
                        
                    }
                    else
                    {
                        // Make me forgetting to do this in DS3/SDT obvious by having all weapons at origin lol
                        absoluteWeaponTransform = Matrix.Identity;
                    }

                    if (DebugRightWeaponModelPositions && !isLeft)
                    {
                        if (modelIdx == 0)
                            absoluteWeaponTransform = Matrix.CreateTranslation(-0.5f, 0, 2);
                        else if (modelIdx == 1)
                            absoluteWeaponTransform = Matrix.CreateTranslation(-1.5f, 0, 2);
                        else if (modelIdx == 2)
                            absoluteWeaponTransform = Matrix.CreateTranslation(-2.5f, 0, 2);
                        else if (modelIdx == 3)
                            absoluteWeaponTransform = Matrix.CreateTranslation(-3.5f, 0, 2);
                    }
                    else if (DebugLeftWeaponModelPositions && isLeft)
                    {
                        if (modelIdx == 0)
                            absoluteWeaponTransform = Matrix.CreateTranslation(0.5f, 0, 2);
                        else if (modelIdx == 1)
                            absoluteWeaponTransform = Matrix.CreateTranslation(1.5f, 0, 2);
                        else if (modelIdx == 2)
                            absoluteWeaponTransform = Matrix.CreateTranslation(2.5f, 0, 2);
                        else if (modelIdx == 3)
                            absoluteWeaponTransform = Matrix.CreateTranslation(3.5f, 0, 2);
                    }

                    wpnMdl.StartTransform = wpnMdl.CurrentTransform = new Transform(
                            (backward ? Matrix.CreateRotationX(MathHelper.Pi) : Matrix.Identity)
                            * (sideways ? Matrix.CreateRotationY(MathHelper.Pi) : Matrix.Identity)
                            * absoluteWeaponTransform * MODEL.CurrentTransform.WorldMatrix);

                    wpnMdl.AnimContainer.ResetRootMotion();

                    wpnMdl.AfterAnimUpdate(timeDelta, ignorePosWrap: true);
                }
            }

            var isOldGame = GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R || 
                GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES;
            var rback = isOldGame ? !RightWeaponFlipBackwards : RightWeaponFlipBackwards;
            var rside = isOldGame ? !RightWeaponFlipSideways : !RightWeaponFlipSideways;
            var lback = isOldGame ? !LeftWeaponFlipBackwards : LeftWeaponFlipBackwards;
            var lside = isOldGame ? LeftWeaponFlipSideways : !LeftWeaponFlipSideways;

            DoWPN(RightWeapon, RightWeaponModel0, 0, RightWeaponBoneIndex, isLeft: false, rback, rside);
            DoWPN(RightWeapon, RightWeaponModel1, 1, RightWeaponBoneIndex, isLeft: false, rback, rside);
            DoWPN(RightWeapon, RightWeaponModel2, 2, RightWeaponBoneIndex, isLeft: false, rback, rside);
            DoWPN(RightWeapon, RightWeaponModel3, 3, RightWeaponBoneIndex, isLeft: false, rback, rside);

            DoWPN(LeftWeapon, LeftWeaponModel0, 0, LeftWeaponBoneIndex, isLeft: true, lback, lside);
            DoWPN(LeftWeapon, LeftWeaponModel1, 1, LeftWeaponBoneIndex, isLeft: true, lback, lside);
            DoWPN(LeftWeapon, LeftWeaponModel2, 2, LeftWeaponBoneIndex, isLeft: true, lback, lside);
            DoWPN(LeftWeapon, LeftWeaponModel3, 3, LeftWeaponBoneIndex, isLeft: true, lback, lside);
        }

        public void SelectWeaponAnimations(string playerAnimName)
        {
            string GetMatchingAnim(IEnumerable<string> animNames, string desiredName)
            {
                if (desiredName == null)
                    return null;

                desiredName = desiredName.Replace(".hkx", "");
                foreach (var a in animNames)
                {
                    if (a.StartsWith(desiredName))
                        return a;
                }
                return null;
            }

            void DoWPN(Model wpnMdl)
            {
                if (wpnMdl == null)
                    return;

                if (wpnMdl.AnimContainer.Skeleton?.OriginalHavokSkeleton == null)
                {
                    wpnMdl.SkeletonFlver?.RevertToReferencePose();
                }


                if (wpnMdl != null && wpnMdl.AnimContainer != null)
                {
                    if (wpnMdl.AnimContainer.Animations.Count > 0)
                    {
                        var matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, playerAnimName);
                        string selectedWpnAnimName = null;
                        if (matching != null)
                        {
                            selectedWpnAnimName = matching;
                        }
                        else
                        {
                            matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, GameRoot.GameTypeHasLongAnimIDs ? "a999_000000.hkx" : "a99_0000.hkx");
                            if (matching != null)
                            {
                                selectedWpnAnimName = matching;
                            }
                            else
                            {
                                matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, GameRoot.GameTypeHasLongAnimIDs ? "a000_000000.hkx" : "a00_0000.hkx");
                                if (matching != null)
                                {
                                    selectedWpnAnimName = matching;
                                }
                                else
                                {
                                    selectedWpnAnimName = wpnMdl.AnimContainer.Animations.Keys.First();
                                }
                            }
                        }

                        //TODO: Check if this is actually accurate and doesn't cause random issues with syncing
                        wpnMdl.AnimContainer.ChangeToNewAnimation(selectedWpnAnimName, animWeight: MODEL.AnimContainer?.CurrentAnimation?.Weight ?? 1, 
                            startTime: MODEL.AnimContainer?.CurrentAnimTime ?? 0, clearOldLayers: false);
                    }
                }
            }

            DoWPN(RightWeaponModel0);
            DoWPN(RightWeaponModel1);
            DoWPN(RightWeaponModel2);
            DoWPN(RightWeaponModel3);

            DoWPN(LeftWeaponModel0);
            DoWPN(LeftWeaponModel1);
            DoWPN(LeftWeaponModel2);
            DoWPN(LeftWeaponModel3);
        }

        

        public void UpdateWeaponAnimation(float timeDelta)
        {
            string GetMatchingAnim(IEnumerable<string> animNames, string desiredName)
            {
                if (desiredName == null)
                    return null;

                desiredName = desiredName.Replace(".hkx", "");
                foreach (var a in animNames)
                {
                    if (a.StartsWith(desiredName))
                        return a;
                }
                return null;
            }

            void DoWPN(Model wpnMdl)
            {
                if (wpnMdl == null)
                    return;

                if (wpnMdl.SkeletonFlver != null)
                    wpnMdl.UpdateSkeleton();

                if (wpnMdl.AnimContainer.Skeleton?.OriginalHavokSkeleton == null)
                {
                    wpnMdl.SkeletonFlver?.RevertToReferencePose();
                }


                if (wpnMdl != null && wpnMdl.AnimContainer != null)
                {
                    if (!wpnMdl.ApplyBindPose)
                    {
                        //V2.0
                        //RightWeaponModel.AnimContainer.IsLoop = false;

                        wpnMdl.ScrubAnimRelative(timeDelta);
                        //wpnMdl.AfterAnimUpdate(timeDelta);

                        if (MODEL.AnimContainer.CurrentAnimDuration.HasValue && wpnMdl.AnimContainer.CurrentAnimDuration.HasValue)
                        {
                            float curModTime = MODEL.AnimContainer.CurrentAnimTime % MODEL.AnimContainer.CurrentAnimDuration.Value;

                            // Make subsequent loops of player anim be the subsequent loops of the weapon anim so it will not blend
                            if (MODEL.AnimContainer.CurrentAnimTime >= MODEL.AnimContainer.CurrentAnimDuration.Value)
                                curModTime += wpnMdl.AnimContainer.CurrentAnimDuration.Value;

                            wpnMdl.ScrubAnimRelative(curModTime - wpnMdl.AnimContainer.CurrentAnimTime);
                            //wpnMdl.AfterAnimUpdate(curModTime - wpnMdl.AnimContainer.CurrentAnimTime);
                        }
                    }
                    //else
                    //{
                    //    wpnMdl?.Skeleton?.RevertToReferencePose();
                    //}
                }


            }

            DoWPN(RightWeaponModel0);
            DoWPN(RightWeaponModel1);
            DoWPN(RightWeaponModel2);
            DoWPN(RightWeaponModel3);

            DoWPN(LeftWeaponModel0);
            DoWPN(LeftWeaponModel1);
            DoWPN(LeftWeaponModel2);
            DoWPN(LeftWeaponModel3);
        }

        public void InitSkeleton(NewAnimSkeleton_FLVER skeleton)
        {
            Skeleton = skeleton;
            boneIndexRemap = new Dictionary<string, int>();
            for (int i = 0; i < skeleton.FlverSkeleton.Count; i++)
            {
                if (skeleton.FlverSkeleton[i].Name == "R_Weapon")
                {
                    RightWeaponBoneIndex = i;
                }
                else if (skeleton.FlverSkeleton[i].Name == "L_Weapon")
                {
                    LeftWeaponBoneIndex = i;
                }
                if (!boneIndexRemap.ContainsKey(skeleton.FlverSkeleton[i].Name))
                {
                    boneIndexRemap.Add(skeleton.FlverSkeleton[i].Name, i);
                }
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

        private NewMesh LoadArmorMesh(IBinder partsbnd)
        {
            // doesn't need _lock_doingAnythingWithModels because the thing it's called from has that and it's private.
            List<TPF> tpfs = new List<TPF>();
            FLVER2 flver2 = null;
            FLVER0 flver0 = null;
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
                        NotificationManager.PushNotification($"Failed to read TPF '{f.Name}' from inside binder. Exception:\n{ex}");
                    }
                }
                else if (GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DES && (flver2 == null && nameCheck.EndsWith(".flver") || FLVER2.Is(f.Bytes)))
                {
                    try
                    {
                        var readFlver2 = FLVER2.Read(f.Bytes);
                        flver2 = readFlver2;
                    }
                    catch (Exception ex)
                    {
                        NotificationManager.PushNotification($"Failed to read FLVER2 '{f.Name}' from inside binder. Exception:\n{ex}");
                        flver2 = null;
                    }
                }
                else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES && (flver0 == null && nameCheck.EndsWith(".flver") || FLVER0.Is(f.Bytes)))
                {
                    flver0 = FLVER0.Read(f.Bytes);

                    try
                    {
                        var readFlver0 = FLVER0.Read(f.Bytes);
                        flver0 = readFlver0;
                    }
                    catch (Exception ex)
                    {
                        NotificationManager.PushNotification($"Failed to read FLVER0 '{f.Name}' from inside binder. Exception:\n{ex}");
                        flver0 = null;
                    }
                }
            }

            foreach (var tpf in tpfs)
            {
                TexturePool.AddTpf(tpf);
            }

            NewMesh mesh = null;

            if (GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DES && flver2 != null)
            {
                mesh = new NewMesh(flver2, false, boneIndexRemap);
            }
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES && flver0 != null)
            {
                mesh = new NewMesh(flver0, false, boneIndexRemap);
            }
            Scene.RequestTextureLoad();

            return mesh;
        }

        public void LoadArmorPartsbnd(IBinder partsbnd, EquipSlot slot, string partsName)
        {
            NewMesh oldMesh = null;

            if (slot == EquipSlot.Head)
                oldMesh = HeadMesh;
            else if (slot == EquipSlot.Body)
                oldMesh = BodyMesh;
            else if (slot == EquipSlot.Arms)
                oldMesh = ArmsMesh;
            else if (slot == EquipSlot.Legs)
                oldMesh = LegsMesh;
            else if (slot == EquipSlot.Face)
                oldMesh = FaceMesh;
            else if (slot == EquipSlot.Facegen)
                oldMesh = FacegenMesh;
            else if (slot == EquipSlot.Hair)
                oldMesh = HairMesh;

            NewMesh newMesh = LoadArmorMesh(partsbnd);

            if (newMesh != null)
            {
                newMesh.Name = partsName;
            }

            if (slot == EquipSlot.Head)
                HeadMesh = newMesh;
            else if (slot == EquipSlot.Body)
                BodyMesh = newMesh;
            else if (slot == EquipSlot.Arms)
                ArmsMesh = newMesh;
            else if (slot == EquipSlot.Legs)
                LegsMesh = newMesh;
            else if (slot == EquipSlot.Face)
                FaceMesh = newMesh;
            else if (slot == EquipSlot.Facegen)
                FacegenMesh = newMesh;
            else if (slot == EquipSlot.Hair)
                HairMesh = newMesh;

            oldMesh?.Dispose();

            FlverMaterialDefInfo.FlushBinderCache();
        }

        public void LoadArmorPartsbnd(byte[] partsbndBytes, EquipSlot slot, string partsName)
        {
            if (partsbndBytes != null)
            {
                if (BND3.Is(partsbndBytes))
                {
                    LoadArmorPartsbnd(BND3.Read(partsbndBytes), slot, partsName);
                }
                else
                {
                    LoadArmorPartsbnd(BND4.Read(partsbndBytes), slot, partsName);
                }
            }
            else
            {
                if (slot == EquipSlot.Head)
                {
                    var oldMdl = HeadMesh;
                    HeadMesh = null;
                    oldMdl?.Dispose();
                }
                else if (slot == EquipSlot.Body)
                {
                    var oldMdl = BodyMesh;
                    BodyMesh = null;
                    oldMdl?.Dispose();
                }
                else if (slot == EquipSlot.Arms)
                {
                    var oldMdl = ArmsMesh;
                    ArmsMesh = null;
                    oldMdl?.Dispose();
                }
                else if (slot == EquipSlot.Legs)
                {
                    var oldMdl = LegsMesh;
                    LegsMesh = null;
                    oldMdl?.Dispose();
                }
                else if (slot == EquipSlot.Face)
                {
                    var oldMdl = FaceMesh;
                    FaceMesh = null;
                    oldMdl?.Dispose();
                }
                else if (slot == EquipSlot.Facegen)
                {
                    var oldMdl = FacegenMesh;
                    FacegenMesh = null;
                    oldMdl?.Dispose();
                }
                else if (slot == EquipSlot.Hair)
                {
                    var oldMdl = HairMesh;
                    HairMesh = null;
                    oldMdl?.Dispose();
                }
            }
           
        }

        public void Draw(bool[] mask, int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            try
            {
                UpdateWeaponTransforms(0);
                MODEL.AfterAnimUpdate(0);
            }
            catch
            {

            }

            if (FaceMesh != null)
            {
                FaceMesh.DrawMask = mask;
                FaceMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>FACE failed to draw:\n\n{ex}", 
                        Vector3.Transform(new Vector3(-3f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix), 
                        Color.Red, Color.Black, 10);
                });
            }

            if (HairMesh != null)
            {
                HairMesh.DrawMask = mask;
                HairMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>HAIR failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(-3f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (FacegenMesh != null)
            {
                if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        FacegenMesh.DrawMask[i] = mask[i];
                    }
                }
                else
                {
                    FacegenMesh.DrawMask = mask;
                }

                FacegenMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>FACEGEN failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(-2f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (HeadMesh != null)
            {
                //HeadMesh.DrawMask = mask;
                HeadMesh.DrawMask = new bool[98];
                for (int i = 0; i < 98; i++)
                    HeadMesh.DrawMask[i] = true;
                HeadMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>HEAD failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(-1f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (BodyMesh != null)
            {
                BodyMesh.DrawMask = mask;
                BodyMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>BODY failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(1f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (ArmsMesh != null)
            {
                ArmsMesh.DrawMask = mask;
                ArmsMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>ARMS failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(2f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (LegsMesh != null)
            {
                LegsMesh.DrawMask = mask;
                LegsMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, MODEL, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>LEGS failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(3f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            void DrawWeapon(ParamData.EquipParamWeapon wpn, Model wpnMdl, int modelIdx, bool isLeft)
            {
                if (wpn == null || wpnMdl == null)
                {
                    return;
                }

                wpnMdl.Opacity = MODEL.Opacity;

                bool renderWpn = true;

                

                if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                {
                    renderWpn = (modelIdx == 0);
                }
                else if (GameRoot.GameTypeUsesWepAbsorpPosParam)
                {
                    var dummyPolyID = isLeft ? wpn.DS3_GetLeftWeaponDummyPoly(this, modelIdx)
                            : wpn.DS3_GetRightWeaponDummyPoly(this, modelIdx);

                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT && SekiroProstheticOverrideTaeActive)
                    {
                        dummyPolyID = GetSekiroProstheticDummyPoly(isLeft ? 
                            ProstheticOverrideModelType.LeftWeapon : ProstheticOverrideModelType.RightWeapon, modelIdx) ?? dummyPolyID;
                    }

                    // Temp workaround
                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER && dummyPolyID > 0)
                    {
                        dummyPolyID %= 1000;
                    }

                    if (dummyPolyID < 0)
                    {
                        renderWpn = false;
                    }
                }
                else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                {
                    var dummyPolyID = isLeft ? wpn.BB_GetLeftWeaponDummyPoly(this, modelIdx)
                            : wpn.BB_GetRightWeaponDummyPoly(this, modelIdx);

                    if (dummyPolyID < 0)
                    {
                        renderWpn = false;
                    }
                }

                if (renderWpn)
                {
                    wpnMdl?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
                }
                else if ((DebugRightWeaponModelPositions && !isLeft) || (DebugLeftWeaponModelPositions && isLeft))
                {
                    float prevOpacity = wpnMdl.Opacity;
                    wpnMdl.Opacity = 0.2f;
                    wpnMdl?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
                    wpnMdl.Opacity = prevOpacity;
                }
            }

            DrawWeapon(RightWeapon, RightWeaponModel0, 0, isLeft: false);
            DrawWeapon(RightWeapon, RightWeaponModel1, 1, isLeft: false);
            DrawWeapon(RightWeapon, RightWeaponModel2, 2, isLeft: false);
            DrawWeapon(RightWeapon, RightWeaponModel3, 3, isLeft: false);

            DrawWeapon(LeftWeapon, LeftWeaponModel0, 0, isLeft: true);
            DrawWeapon(LeftWeapon, LeftWeaponModel1, 1, isLeft: true);
            DrawWeapon(LeftWeapon, LeftWeaponModel2, 2, isLeft: true);
            DrawWeapon(LeftWeapon, LeftWeaponModel3, 3, isLeft: true);
        }

        public int HeadID
        {
            get => _headID;
            set
            {
                if (_headID != value)
                {
                    _headID = value;
                    OnEquipmentChanged(EquipSlot.Head);
                }
            }
        }

        public int BodyID
        {
            get => _bodyID;
            set
            {
                if (_bodyID != value)
                {
                    _bodyID = value;
                    OnEquipmentChanged(EquipSlot.Body);
                }
            }
        }

        public int ArmsID
        {
            get => _armsID;
            set
            {
                if (_armsID != value)
                {
                    _armsID = value;
                    OnEquipmentChanged(EquipSlot.Arms);
                }
            }
        }

        public int LegsID
        {
            get => _legsID;
            set
            {
                if (_legsID != value)
                {
                    _legsID = value;
                    OnEquipmentChanged(EquipSlot.Legs);
                }
            }
        }

        public int RightWeaponID
        {
            get => _rightWeaponID;
            set
            {
                if (_rightWeaponID != value)
                {
                    _rightWeaponID = value;
                    OnEquipmentChanged(EquipSlot.RightWeapon);
                }
            }
        }

        public int LeftWeaponID
        {
            get => _leftWeaponID;
            set
            {
                if (_leftWeaponID != value)
                {
                    _leftWeaponID = value;
                    OnEquipmentChanged(EquipSlot.LeftWeapon);
                }
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
                else if (WeaponStyle == WeaponStyleType.TwoHandR || WeaponStyle == WeaponStyleType.OneHandTransformedR)
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
                else if (WeaponStyle == WeaponStyleType.TwoHandL || WeaponStyle == WeaponStyleType.OneHandTransformedL)
                    return BB_WeaponState.FormB;
                else
                    return BB_WeaponState.Sheathed;
            }
        }

        private int lastHeadLoaded = -1;
        private int lastBodyLoaded = -1;
        private int lastArmsLoaded = -1;
        private int lastLegsLoaded = -1;
        private int lastRightWeaponLoaded = -1;
        private int lastLeftWeaponLoaded = -1;

        public event EventHandler<EquipSlot> EquipmentChanged;

        private void OnEquipmentChanged(EquipSlot slot)
        {
            EquipmentChanged?.Invoke(this, slot);
        }

        public event EventHandler EquipmentModelsUpdated;
        private void OnEquipmentModelsUpdated()
        {
            EquipmentModelsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public ParamData.EquipParamProtector Head
            => ParamManager.EquipParamProtector.ContainsKey(HeadID)
            ? ParamManager.EquipParamProtector[HeadID] : null;

        public ParamData.EquipParamProtector Body
            => ParamManager.EquipParamProtector.ContainsKey(BodyID)
            ? ParamManager.EquipParamProtector[BodyID] : null;

        public ParamData.EquipParamProtector Arms
            => ParamManager.EquipParamProtector.ContainsKey(ArmsID)
            ? ParamManager.EquipParamProtector[ArmsID] : null;

        public ParamData.EquipParamProtector Legs
            => ParamManager.EquipParamProtector.ContainsKey(LegsID)
            ? ParamManager.EquipParamProtector[LegsID] : null;

        public ParamData.EquipParamWeapon RightWeapon
            => ParamManager.EquipParamWeapon.ContainsKey(RightWeaponID)
            ? ParamManager.EquipParamWeapon[RightWeaponID] : null;

        public ParamData.EquipParamWeapon LeftWeapon
            => ParamManager.EquipParamWeapon.ContainsKey(LeftWeaponID)
            ? ParamManager.EquipParamWeapon[LeftWeaponID] : null;

        public void UpdateMasks()
        {
            MODEL.ResetDrawMaskToAllVisible();

            if (Head != null)
                MODEL.DefaultDrawMask = Head.ApplyInvisFlagsToMask(MODEL.DrawMask);
            if (Body != null)
                MODEL.DefaultDrawMask = Body.ApplyInvisFlagsToMask(MODEL.DrawMask);
            if (Arms != null)
                MODEL.DefaultDrawMask = Arms.ApplyInvisFlagsToMask(MODEL.DrawMask);
            if (Legs != null)
                MODEL.DefaultDrawMask = Legs.ApplyInvisFlagsToMask(MODEL.DrawMask);

            MODEL.ResetDrawMaskToDefault();
        }

        private object _lock_EquipParam = new object();
        private List<string> _possibleFaceModels = new List<string>();
        private List<string> _possibleFacegenModels = new List<string>();
        private List<string> _possibleHairModels = new List<string>();
        private int _faceIndex_Male = -1;
        private int _faceIndex_Female = -1;
        private int _hairIndex_Male = -1;
        private int _hairIndex_Female = -1;

        public int FaceIndex_Male
        {
            get
            {
                int result = -1;
                lock (_lock_EquipParam)
                {
                    result = _faceIndex_Male;
                }
                return result;
            }
            set
            {
                int oldIndex = -1;
                lock (_lock_EquipParam)
                {
                    oldIndex = _faceIndex_Male;
                    _faceIndex_Male = value;
                }
                if (oldIndex != value && !IsFemale)
                {
                    if (value >= 0)
                        LoadNewFace(PossibleFaceModels[FaceIndex]);
                    else
                        LoadNewFace(null);
                    UpdateModels(isAsync: true, onCompleteAction: null, forceReloadArmor: false);
                }

            }
        }

        public int FaceIndex_Female
        {
            get
            {
                int result = -1;
                lock (_lock_EquipParam)
                {
                    result = _faceIndex_Female;
                }
                return result;
            }
            set
            {
                int oldIndex = -1;
                lock (_lock_EquipParam)
                {
                    oldIndex = _faceIndex_Female;
                    _faceIndex_Female = value;
                }
                if (oldIndex != value && IsFemale)
                {
                    if (value >= 0)
                        LoadNewFace(PossibleFaceModels[FaceIndex]);
                    else
                        LoadNewFace(null);
                    UpdateModels(isAsync: true, onCompleteAction: null, forceReloadArmor: false);
                }

            }
        }

        public int FaceIndex
        {
            get
            {
                int result = -1;
                lock (_lock_EquipParam)
                {
                    result = _isFemale ? _faceIndex_Female : _faceIndex_Male;
                }
                return result;
            }
            set
            {
                int oldIndex = -1;
                lock (_lock_EquipParam)
                {
                    if (_isFemale)
                    {
                        oldIndex = _faceIndex_Female;
                        _faceIndex_Female = value;
                    }
                    else
                    {
                        oldIndex = _faceIndex_Male;
                        _faceIndex_Male = value;
                    }
                }
                if (oldIndex != value)
                {
                    if (value >= 0)
                        LoadNewFace(PossibleFaceModels[FaceIndex]);
                    else
                        LoadNewFace(null);
                    UpdateModels(isAsync: true, onCompleteAction: null, forceReloadArmor: false);
                }

            }
        }

        public Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4> ChrCustomize = new Dictionary<FlverMaterial.ChrCustomizeTypes, Vector4>();

        private int _facegenIndex = -1;
        public int FacegenIndex
        {
            get
            {
                int result = -1;
                lock (_lock_EquipParam)
                {
                    result = _facegenIndex;
                }
                return result;
            }
            set
            {
                int oldIndex = -1;
                lock (_lock_EquipParam)
                {
                    oldIndex = _facegenIndex;
                    _facegenIndex = value;
                }
                if (value != oldIndex)
                {
                    if (value >= 0)
                        LoadNewFacegen(PossibleFacegenModels[FacegenIndex]);
                    else
                        LoadNewFacegen(null);
                    UpdateModels(isAsync: true, onCompleteAction: null, forceReloadArmor: false);
                }
            }
        }

        public int HairIndex_Male
        {
            get
            {
                int result = -1;
                lock (_lock_EquipParam)
                {
                    result = _hairIndex_Male;
                }
                return result;
            }
            set
            {
                int oldIndex = -1;
                lock (_lock_EquipParam)
                {
                    oldIndex = _hairIndex_Male;
                    _hairIndex_Male = value;
                }
                if (oldIndex != value && !IsFemale)
                {
                    if (value >= 0)
                        LoadNewHair(PossibleFaceModels[HairIndex]);
                    else
                        LoadNewHair(null);
                    UpdateModels(isAsync: true, onCompleteAction: null, forceReloadArmor: false);
                }

            }
        }

        public int HairIndex_Female
        {
            get
            {
                int result = -1;
                lock (_lock_EquipParam)
                {
                    result = _hairIndex_Female;
                }
                return result;
            }
            set
            {
                int oldIndex = -1;
                lock (_lock_EquipParam)
                {
                    oldIndex = _hairIndex_Female;
                    _hairIndex_Female = value;
                }
                if (oldIndex != value && IsFemale)
                {
                    if (value >= 0)
                        LoadNewHair(PossibleHairModels[HairIndex]);
                    else
                        LoadNewHair(null);
                    UpdateModels(isAsync: true, onCompleteAction: null, forceReloadArmor: false);
                }

            }
        }

        public int HairIndex
        {
            get
            {
                int result = -1;
                lock (_lock_EquipParam)
                {
                    result = _isFemale ? _hairIndex_Female : _hairIndex_Male;
                }
                return result;
            }
            set
            {
                int oldIndex = -1;
                lock (_lock_EquipParam)
                {
                    if (_isFemale)
                    {
                        oldIndex = _hairIndex_Female;
                        _hairIndex_Female = value;
                    }
                    else
                    {
                        oldIndex = _hairIndex_Male;
                        _hairIndex_Male = value;
                    }
                }
                if (oldIndex != value)
                {
                    if (value >= 0)
                        LoadNewHair(PossibleHairModels[HairIndex]);
                    else
                        LoadNewHair(null);
                    UpdateModels(isAsync: true, onCompleteAction: null, forceReloadArmor: false);
                }

            }
        }

        public List<string> PossibleFaceModels
        {
            get
            {
                List<string> result = null;
                lock (_lock_EquipParam)
                {
                    if (_possibleFaceModels == null || _possibleFaceModels.Count == 0)
                        _possibleFaceModels = GameData.SearchFiles(@"/parts", @".*fc_\w_\d\d\d\d.partsbnd.dcx").ToList();
                    result = _possibleFaceModels;
                }
                return result ?? new List<string>();
            }
        }

        public List<string> PossibleFacegenModels
        {
            get
            {
                List<string> result = null;
                lock (_lock_EquipParam)
                {
                    if (_possibleFacegenModels == null || _possibleFacegenModels.Count == 0)
                    {
                        _possibleFacegenModels = GameData.SearchFiles(@"/parts", @".*fg_\w_\d\d\d\d.partsbnd.dcx").ToList();
                        if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DES or SoulsAssetPipeline.SoulsGames.DS1 or SoulsAssetPipeline.SoulsGames.DS1R)
                        {
                            _possibleFacegenModels.Add("/facegen/facegen.fgbnd");
                        }
                    }
                    result = _possibleFacegenModels;
                }
                return result ?? new List<string>();
            }
        }

        public List<string> PossibleHairModels
        {
            get
            {
                List<string> result = null;
                lock (_lock_EquipParam)
                {
                    if (_possibleHairModels == null || _possibleHairModels.Count == 0)
                    {
                        _possibleHairModels = GameData.SearchFiles(@"/parts", @".*hr_\w_\d\d\d\d.partsbnd.dcx").ToList();
                    }
                    result = _possibleHairModels;
                }
                return result ?? new List<string>();
            }
        }

        private void LoadNewFace(string fileName)
        {
            if (fileName != null && FaceMesh != null && Utils.GetShortIngameFileName(fileName).ToLower() == Utils.GetShortIngameFileName(FaceMesh.Name).ToLower())
                return;

            var oldMesh = FaceMesh;
            FaceMesh = null;
            oldMesh?.Dispose();

            if (fileName != null)
                LoadArmorPartsbnd(GameData.ReadFile(fileName), EquipSlot.Face, Utils.GetShortIngameFileName(fileName));
        }

        public void LoadNewFacegen(string fileName)
        {
            if (fileName != null && FacegenMesh != null && Utils.GetShortIngameFileName(fileName).ToLower() == Utils.GetShortIngameFileName(FacegenMesh.Name).ToLower())
                return;

            var oldMesh = FacegenMesh;
            FacegenMesh = null;
            oldMesh?.Dispose();

            if (fileName != null)
                LoadArmorPartsbnd(GameData.ReadFile(fileName), EquipSlot.Facegen, Utils.GetShortIngameFileName(fileName));
        }

        public void LoadNewHair(string fileName)
        {
            if (fileName != null && HairMesh != null && Utils.GetShortIngameFileName(fileName).ToLower() == Utils.GetShortIngameFileName(HairMesh.Name).ToLower())
                return;

            var oldMesh = HairMesh;
            HairMesh = null;
            oldMesh?.Dispose();

            if (fileName != null)
                LoadArmorPartsbnd(GameData.ReadFile(fileName), EquipSlot.Hair, Utils.GetShortIngameFileName(fileName));
        }

        public int GetDefaultFaceIndexForCurrentGame(bool isFemale)
        {
            var faces = PossibleFaceModels;
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                return faces.IndexOf($@"/parts/fc_{(IsFemale ? "f" : "m")}_0210.partsbnd.dcx");
            else
                return faces.IndexOf($@"/parts/fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx");
        }

        public int GetDefaultHairIndexForCurrentGame(bool isFemale)
        {
            var hairs = PossibleHairModels;
            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                return -1;
            else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
                return hairs.IndexOf($@"/parts/hr_a_0000.partsbnd.dcx");
            else
                return hairs.IndexOf($@"/parts/hr_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx");
        }

        public int GetDefaultFacegenIndexForCurrentGame()
        {
            var facegens = PossibleFacegenModels;
            if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS1 
                or SoulsAssetPipeline.SoulsGames.DS1R 
                or SoulsAssetPipeline.SoulsGames.DES)
                return facegens.IndexOf($@"/facegen/facegen.fgbnd");
            else
                return facegens.IndexOf($@"/parts/fg_a_0100.partsbnd.dcx");
        }

        public void UpdateModels(bool isAsync = false, Action onCompleteAction = null, bool forceReloadArmor = false)
        {
            LoadingTaskMan.DoLoadingTask("ChrAsm_UpdateModels", "Updating c0000 models...", progress =>
            {
                //if (updateFaceAndBody)
                //{
                //    var oldMesh = FaceMesh;
                //    FaceMesh = null;
                //    oldMesh?.Dispose();

                //    oldMesh = FacegenMesh;
                //    FacegenMesh = null;
                //    oldMesh?.Dispose();

                //    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                //    {
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fg_a_0100.partsbnd.dcx"), EquipSlot.Facegen);
                //    }
                //    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.ER)
                //    {
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fg_a_0100.partsbnd.dcx"), EquipSlot.Facegen);
                //        FacegenMesh?.HideAllDrawMask();
                //    }
                //    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.BB)
                //    {
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fg_a_0100.partsbnd.dcx"), EquipSlot.Facegen);
                //    }
                //    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                //    {
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fc_m_0100.partsbnd.dcx"), EquipSlot.Face);
                //    }
                //    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
                //    {
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/facegen/facegen.fgbnd"), EquipSlot.Facegen);
                //    }
                //    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                //    {
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/FC_{(IsFemale ? "F" : "M")}_0000.partsbnd.dcx"), EquipSlot.Face);
                //        //LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"facegen\FaceGen.fgbnd"), EquipSlot.Facegen);
                //    }
                //    else if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1)
                //    {
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/parts/FC_{(IsFemale ? "F" : "M")}_0000.partsbnd"), EquipSlot.Face);
                //        LoadArmorPartsbnd(GameData.ReadFile($@"/facegen/FaceGen.fgbnd"), EquipSlot.Facegen);
                //    }
                //}

                if (HeadID != lastHeadLoaded || forceReloadArmor)
                {
                    if (Head != null && Head.CanEquipOnGender(IsFemale))
                    {
                        LoadArmorPartsbnd(Head.GetPartBndFile(IsFemale), EquipSlot.Head, Utils.GetShortIngameFileName(Head.GetPartBndName(IsFemale)));
                        lastHeadLoaded = HeadID;
                    }
                    else
                    {
                        var oldMesh = HeadMesh;
                        HeadMesh = null;
                        oldMesh?.Dispose();
                    }
                }

                progress.Report(1.0 / 6.0);

                if (BodyID != lastBodyLoaded || forceReloadArmor)
                {
                    if (Body != null && Body.CanEquipOnGender(IsFemale))
                    {
                        LoadArmorPartsbnd(Body.GetPartBndFile(IsFemale), EquipSlot.Body, Utils.GetShortIngameFileName(Body.GetPartBndName(IsFemale)));
                        lastBodyLoaded = BodyID;
                    }
                    else
                    {
                        var oldMesh = BodyMesh;
                        BodyMesh = null;
                        oldMesh?.Dispose();
                    }
                }

                progress.Report(2.0 / 6.0);

                if (ArmsID != lastArmsLoaded || forceReloadArmor)
                {
                    if (Arms != null && Arms.CanEquipOnGender(IsFemale))
                    {
                        LoadArmorPartsbnd(Arms.GetPartBndFile(IsFemale), EquipSlot.Arms, Utils.GetShortIngameFileName(Arms.GetPartBndName(IsFemale)));
                        lastArmsLoaded = ArmsID;
                    }
                    else
                    {
                        var oldMesh = ArmsMesh;
                        ArmsMesh = null;
                        oldMesh?.Dispose();
                    }
                }

                progress.Report(3.0 / 6.0);

                if (LegsID != lastLegsLoaded || forceReloadArmor)
                {
                    if (Legs != null && Legs.CanEquipOnGender(IsFemale))
                    {
                        LoadArmorPartsbnd(Legs.GetPartBndFile(IsFemale), EquipSlot.Legs, Utils.GetShortIngameFileName(Legs.GetPartBndName(IsFemale)));
                        lastLegsLoaded = LegsID;
                    }
                    else
                    {
                        var oldMesh = LegsMesh;
                        LegsMesh = null;
                        oldMesh?.Dispose();
                    }
                }

                progress.Report(4.0 / 6.0);

                if ((RightWeaponID != lastRightWeaponLoaded))
                {
                    if (RightWeapon != null)
                    {
                        
                        //TODO
                        try
                        {
                            var weaponFile = RightWeapon.GetPartBndFile();
                            var shortWeaponName = RightWeapon.GetPartBndName();
                            IBinder weaponBnd = null;

                            if (weaponFile != null)
                            {
                                if (BND3.Is(weaponFile))
                                    weaponBnd = BND3.Read(weaponFile);
                                else
                                    weaponBnd = BND4.Read(weaponFile);

                                var newRightWeaponModel0 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 0, null, ignoreStaticTransforms: true);
                                if (newRightWeaponModel0.AnimContainer == null)
                                {
                                    newRightWeaponModel0?.Dispose();
                                    newRightWeaponModel0 = null;
                                }
                                else
                                {
                                    newRightWeaponModel0.IS_PLAYER_WEAPON = true;
                                    newRightWeaponModel0.DummyPolyMan.GlobalDummyPolyIDOffset = 10000;
                                    newRightWeaponModel0.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 0 - ";
                                }
                                    
                                var newRightWeaponModel1 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 1, null, ignoreStaticTransforms: true);
                                if (newRightWeaponModel1.AnimContainer == null)
                                {
                                    newRightWeaponModel1?.Dispose();
                                    newRightWeaponModel1 = null;
                                }
                                else
                                {
                                    newRightWeaponModel1.IS_PLAYER_WEAPON = true;
                                    newRightWeaponModel1.DummyPolyMan.GlobalDummyPolyIDOffset = 11000;
                                    newRightWeaponModel1.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 1 - ";
                                }

                                var newRightWeaponModel2 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 2, null, ignoreStaticTransforms: true);
                                if (newRightWeaponModel2.AnimContainer == null)
                                {
                                    newRightWeaponModel2?.Dispose();
                                    newRightWeaponModel2 = null;
                                }
                                else
                                {
                                    newRightWeaponModel2.IS_PLAYER_WEAPON = true;
                                    newRightWeaponModel2.DummyPolyMan.GlobalDummyPolyIDOffset = 12000;
                                    newRightWeaponModel2.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 2 - ";
                                }

                                var newRightWeaponModel3 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 3, null, ignoreStaticTransforms: true);
                                if (newRightWeaponModel3.AnimContainer == null)
                                {
                                    newRightWeaponModel3?.Dispose();
                                    newRightWeaponModel3 = null;
                                }
                                else
                                {
                                    newRightWeaponModel3.IS_PLAYER_WEAPON = true;
                                    newRightWeaponModel3.DummyPolyMan.GlobalDummyPolyIDOffset = 13000;
                                    newRightWeaponModel3.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 3 - ";
                                }

                                var oldRightWeaponModel0 = RightWeaponModel0;
                                var oldRightWeaponModel1 = RightWeaponModel1;
                                var oldRightWeaponModel2 = RightWeaponModel2;
                                var oldRightWeaponModel3 = RightWeaponModel3;

                                RightWeaponModel0 = newRightWeaponModel0;
                                RightWeaponModel1 = newRightWeaponModel1;
                                RightWeaponModel2 = newRightWeaponModel2;
                                RightWeaponModel3 = newRightWeaponModel3;

                                if (RightWeaponModel0 != null && RightWeaponModel0.MainMesh != null)
                                    RightWeaponModel0.MainMesh.Name = RightWeaponModel0.Name;
                                if (RightWeaponModel1 != null && RightWeaponModel1.MainMesh != null)
                                    RightWeaponModel1.MainMesh.Name = RightWeaponModel1.Name;
                                if (RightWeaponModel2 != null && RightWeaponModel2.MainMesh != null)
                                    RightWeaponModel2.MainMesh.Name = RightWeaponModel2.Name;
                                if (RightWeaponModel3 != null && RightWeaponModel3.MainMesh != null)
                                    RightWeaponModel3.MainMesh.Name = RightWeaponModel3.Name;

                                oldRightWeaponModel0?.Dispose();
                                oldRightWeaponModel1?.Dispose();
                                oldRightWeaponModel2?.Dispose();
                                oldRightWeaponModel3?.Dispose();
                            }
                        }
                        catch (Exception ex)
                        {
                            lastRightWeaponLoaded = -1;

                            var oldRightWeaponModel0 = RightWeaponModel0;
                            var oldRightWeaponModel1 = RightWeaponModel1;
                            var oldRightWeaponModel2 = RightWeaponModel2;
                            var oldRightWeaponModel3 = RightWeaponModel3;

                            RightWeaponModel0 = null;
                            RightWeaponModel1 = null;
                            RightWeaponModel2 = null;
                            RightWeaponModel3 = null;

                            oldRightWeaponModel0?.Dispose();
                            oldRightWeaponModel1?.Dispose();
                            oldRightWeaponModel2?.Dispose();
                            oldRightWeaponModel3?.Dispose();

                            System.Windows.Forms.MessageBox.Show(
                                $"Failed to load right-hand weapon model:\n\n{ex}",
                                "Failed To Load RH Weapon Model",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
                        }
                        lastRightWeaponLoaded = RightWeaponID;
                    }
                    else
                    {
                        lastRightWeaponLoaded = -1;

                        var oldRightWeaponModel0 = RightWeaponModel0;
                        var oldRightWeaponModel1 = RightWeaponModel1;
                        var oldRightWeaponModel2 = RightWeaponModel2;
                        var oldRightWeaponModel3 = RightWeaponModel3;

                        RightWeaponModel0 = null;
                        RightWeaponModel1 = null;
                        RightWeaponModel2 = null;
                        RightWeaponModel3 = null;

                        oldRightWeaponModel0?.Dispose();
                        oldRightWeaponModel1?.Dispose();
                        oldRightWeaponModel2?.Dispose();
                        oldRightWeaponModel3?.Dispose();
                    }
                }

                progress.Report(5.0 / 6.0);

                if ((LeftWeaponID != lastLeftWeaponLoaded))
                {
                    if (LeftWeapon != null)
                    {
                        
                        //TODO
                        try
                        {
                            var weaponFile = LeftWeapon.GetPartBndFile();
                            var shortWeaponName = LeftWeapon.GetPartBndName();
                            IBinder weaponBnd = null;

                            if (weaponFile != null)
                            {
                                if (BND3.Is(weaponFile))
                                    weaponBnd = BND3.Read(weaponFile);
                                else
                                    weaponBnd = BND4.Read(weaponFile);

                                var newLeftWeaponModel0 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 0, null, ignoreStaticTransforms: true);
                                if (newLeftWeaponModel0.AnimContainer == null)
                                {
                                    newLeftWeaponModel0?.Dispose();
                                    newLeftWeaponModel0 = null;
                                }
                                else
                                {
                                    newLeftWeaponModel0.IS_PLAYER_WEAPON = true;
                                    newLeftWeaponModel0.DummyPolyMan.GlobalDummyPolyIDOffset = 20000;
                                    newLeftWeaponModel0.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 0 - ";
                                }

                                var newLeftWeaponModel1 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 1, null, ignoreStaticTransforms: true);
                                if (newLeftWeaponModel1.AnimContainer == null)
                                {
                                    newLeftWeaponModel1?.Dispose();
                                    newLeftWeaponModel1 = null;
                                }
                                else
                                {
                                    newLeftWeaponModel1.IS_PLAYER_WEAPON = true;
                                    newLeftWeaponModel1.DummyPolyMan.GlobalDummyPolyIDOffset = 21000;
                                    newLeftWeaponModel1.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 1 - ";
                                }

                                var newLeftWeaponModel2 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 2, null, ignoreStaticTransforms: true);
                                if (newLeftWeaponModel2.AnimContainer == null)
                                {
                                    newLeftWeaponModel2?.Dispose();
                                    newLeftWeaponModel2 = null;
                                }
                                else
                                {
                                    newLeftWeaponModel2.IS_PLAYER_WEAPON = true;
                                    newLeftWeaponModel2.DummyPolyMan.GlobalDummyPolyIDOffset = 22000;
                                    newLeftWeaponModel2.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 2 - ";
                                }

                                var newLeftWeaponModel3 = new Model(null, shortWeaponName, weaponBnd, modelIndex: 3, null, ignoreStaticTransforms: true);
                                if (newLeftWeaponModel3.AnimContainer == null)
                                {
                                    newLeftWeaponModel3?.Dispose();
                                    newLeftWeaponModel3 = null;
                                }
                                else
                                {
                                    newLeftWeaponModel3.IS_PLAYER_WEAPON = true;
                                    newLeftWeaponModel3.DummyPolyMan.GlobalDummyPolyIDOffset = 23000;
                                    newLeftWeaponModel3.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 3 - ";
                                }

                                var oldLeftWeaponModel0 = LeftWeaponModel0;
                                var oldLeftWeaponModel1 = LeftWeaponModel1;
                                var oldLeftWeaponModel2 = LeftWeaponModel2;
                                var oldLeftWeaponModel3 = LeftWeaponModel3;

                                LeftWeaponModel0 = newLeftWeaponModel0;
                                LeftWeaponModel1 = newLeftWeaponModel1;
                                LeftWeaponModel2 = newLeftWeaponModel2;
                                LeftWeaponModel3 = newLeftWeaponModel3;

                                if (LeftWeaponModel0 != null && LeftWeaponModel0.MainMesh != null)
                                    LeftWeaponModel0.MainMesh.Name = LeftWeaponModel0.Name;
                                if (LeftWeaponModel1 != null && LeftWeaponModel1.MainMesh != null)
                                    LeftWeaponModel1.MainMesh.Name = LeftWeaponModel1.Name;
                                if (LeftWeaponModel2 != null && LeftWeaponModel2.MainMesh != null)
                                    LeftWeaponModel2.MainMesh.Name = LeftWeaponModel2.Name;
                                if (LeftWeaponModel3 != null && LeftWeaponModel3.MainMesh != null)
                                    LeftWeaponModel3.MainMesh.Name = LeftWeaponModel3.Name;

                                oldLeftWeaponModel0?.Dispose();
                                oldLeftWeaponModel1?.Dispose();
                                oldLeftWeaponModel2?.Dispose();
                                oldLeftWeaponModel3?.Dispose();

                            }
                        }
                        catch (Exception ex)
                        {
                            lastLeftWeaponLoaded = -1;

                            var oldLeftWeaponModel0 = LeftWeaponModel0;
                            var oldLeftWeaponModel1 = LeftWeaponModel1;
                            var oldLeftWeaponModel2 = LeftWeaponModel2;
                            var oldLeftWeaponModel3 = LeftWeaponModel3;

                            LeftWeaponModel0 = null;
                            LeftWeaponModel1 = null;
                            LeftWeaponModel2 = null;
                            LeftWeaponModel3 = null;

                            oldLeftWeaponModel0?.Dispose();
                            oldLeftWeaponModel1?.Dispose();
                            oldLeftWeaponModel2?.Dispose();
                            oldLeftWeaponModel3?.Dispose();

                            System.Windows.Forms.MessageBox.Show(
                                $"Failed to load left-hand weapon model:\n\n{ex}",
                                "Failed To Load LH Weapon Model",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
                        }
                        lastLeftWeaponLoaded = LeftWeaponID;
                    }
                    else
                    {
                        lastLeftWeaponLoaded = -1;
                        var oldLeftWeaponModel0 = LeftWeaponModel0;
                        var oldLeftWeaponModel1 = LeftWeaponModel1;
                        var oldLeftWeaponModel2 = LeftWeaponModel2;
                        var oldLeftWeaponModel3 = LeftWeaponModel3;

                        LeftWeaponModel0 = null;
                        LeftWeaponModel1 = null;
                        LeftWeaponModel2 = null;
                        LeftWeaponModel3 = null;

                        oldLeftWeaponModel0?.Dispose();
                        oldLeftWeaponModel1?.Dispose();
                        oldLeftWeaponModel2?.Dispose();
                        oldLeftWeaponModel3?.Dispose();
                    }

                }

                UpdateMasks();

                progress.Report(6.0 / 6.0);
                onCompleteAction?.Invoke();
                OnEquipmentModelsUpdated();
            }, waitForTaskToComplete: !isAsync, isUnimportant: true);
                
        }

        public ParamData.AtkParam.DummyPolySource GetDummySourceFromManager(NewDummyPolyManager manager)
        {
            if (manager == null)
                return ParamData.AtkParam.DummyPolySource.Body;
            if (manager == MODEL.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.Body;
            else if (manager == RightWeaponModel0?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.RightWeapon0;
            else if (manager == RightWeaponModel1?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.RightWeapon1;
            else if (manager == RightWeaponModel2?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.RightWeapon2;
            else if (manager == RightWeaponModel3?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.RightWeapon3;
            else if (manager == LeftWeaponModel0?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.LeftWeapon0;
            else if (manager == LeftWeaponModel1?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.LeftWeapon1;
            else if (manager == LeftWeaponModel2?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.LeftWeapon2;
            else if (manager == LeftWeaponModel3?.DummyPolyMan)
                return ParamData.AtkParam.DummyPolySource.LeftWeapon3;
            return ParamData.AtkParam.DummyPolySource.Body;
        }

        public NewDummyPolyManager GetDummyManager(ParamData.AtkParam.DummyPolySource src)
        {
            if (src == ParamData.AtkParam.DummyPolySource.Body)
                return MODEL.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.RightWeapon0)
                return RightWeaponModel0?.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.RightWeapon1)
                return RightWeaponModel1?.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.RightWeapon2)
                return RightWeaponModel2?.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.RightWeapon3)
                return RightWeaponModel3?.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.LeftWeapon0)
                return LeftWeaponModel0?.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.LeftWeapon1)
                return LeftWeaponModel1?.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.LeftWeapon2)
                return LeftWeaponModel2?.DummyPolyMan;
            else if (src == ParamData.AtkParam.DummyPolySource.LeftWeapon3)
                return LeftWeaponModel3?.DummyPolyMan;

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
            else if (check == 10)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon0);
            else if (check == 11)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon1);
            else if (check == 12)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon2);
            else if (check == 13)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon3);
            else if (check == 20)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon0);
            else if (check == 21)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon1);
            else if (check == 22)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon2);
            else if (check == 23)
                wpnDmy = GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon3);
            else
                wpnDmy = bodyDmyForFallback;

            if (!wpnDmy.DummyPolyByRefID.ContainsKey(dmy % 1000))
                return bodyDmyForFallback;
            else
                return wpnDmy;
        }

        public void ForeachWeaponModel(Action<Model> doAction)
        {
            if (RightWeaponModel0 != null)
                doAction(RightWeaponModel0);

            if (RightWeaponModel1 != null)
                doAction(RightWeaponModel1);

            if (RightWeaponModel2 != null)
                doAction(RightWeaponModel2);

            if (RightWeaponModel3 != null)
                doAction(RightWeaponModel3);

            if (LeftWeaponModel0 != null)
                doAction(LeftWeaponModel0);

            if (LeftWeaponModel1 != null)
                doAction(LeftWeaponModel1);

            if (LeftWeaponModel2 != null)
                doAction(LeftWeaponModel2);

            if (LeftWeaponModel3 != null)
                doAction(LeftWeaponModel3);
        }

        public void TryToLoadTextures()
        {
            FaceMesh?.TryToLoadTextures();
            FacegenMesh?.TryToLoadTextures();
            HairMesh?.TryToLoadTextures();
            HeadMesh?.TryToLoadTextures();
            BodyMesh?.TryToLoadTextures();
            ArmsMesh?.TryToLoadTextures();
            LegsMesh?.TryToLoadTextures();
            RightWeaponModel0?.TryToLoadTextures();
            RightWeaponModel1?.TryToLoadTextures();
            RightWeaponModel2?.TryToLoadTextures();
            RightWeaponModel3?.TryToLoadTextures();
            LeftWeaponModel0?.TryToLoadTextures();
            LeftWeaponModel1?.TryToLoadTextures();
            LeftWeaponModel2?.TryToLoadTextures();
            LeftWeaponModel3?.TryToLoadTextures();
        }

        public void Dispose()
        {
            FaceMesh?.Dispose();
            FacegenMesh?.Dispose();
            HairMesh?.Dispose();
            HeadMesh?.Dispose();
            BodyMesh?.Dispose();
            ArmsMesh?.Dispose();
            LegsMesh?.Dispose();
            RightWeaponModel0?.Dispose();
            RightWeaponModel1?.Dispose();
            RightWeaponModel2?.Dispose();
            RightWeaponModel3?.Dispose();
            LeftWeaponModel0?.Dispose();
            LeftWeaponModel1?.Dispose();
            LeftWeaponModel2?.Dispose();
            LeftWeaponModel3?.Dispose();
        }
    }
}
