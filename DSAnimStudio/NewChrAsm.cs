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
            int ds3ConditionFlag = GameDataManager.GameType == GameDataManager.GameTypes.DS3
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
            int ds3ConditionFlag = GameDataManager.GameType == GameDataManager.GameTypes.DS3
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

        public bool IsFemale = false;

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

        public NewAnimSkeleton Skeleton { get; private set; } = null;

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

        public bool LeftWeaponFlipBackwards = true;
        public bool LeftWeaponFlipSideways = true;
        public bool RightWeaponFlipBackwards = true;
        public bool RightWeaponFlipSideways = false;

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
            if (GameDataManager.GameType != GameDataManager.GameTypes.DS3)
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

                    if (GameDataManager.GameType == GameDataManager.GameTypes.BB)
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
                    else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3 || 
                        GameDataManager.GameType == GameDataManager.GameTypes.SDT)
                    {
                        var dummyPolyID = isLeft ? wpn.DS3_GetLeftWeaponDummyPoly(this, modelIdx)
                            : wpn.DS3_GetRightWeaponDummyPoly(this, modelIdx);
                        if (dummyPolyID >= 0 && MODEL.DummyPolyMan.DummyPolyByRefID.ContainsKey(dummyPolyID))
                        {
                            absoluteWeaponTransform = MODEL.DummyPolyMan.DummyPolyByRefID[dummyPolyID][0].CurrentMatrix;
                        }
                        else
                        {
                            absoluteWeaponTransform = Matrix.CreateScale(0);
                        }
                    }
                    else if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 || 
                        GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
                    {
                        // Weapon
                        if (modelIdx == 0)
                        {
                            absoluteWeaponTransform =
                                Skeleton.FlverSkeleton[defaultBoneIndex].ReferenceMatrix
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
                            absoluteWeaponTransform = Matrix.CreateTranslation(-0.5f, 0, 0);
                        else if (modelIdx == 1)
                            absoluteWeaponTransform = Matrix.CreateTranslation(-1.5f, 0, 0);
                        else if (modelIdx == 2)
                            absoluteWeaponTransform = Matrix.CreateTranslation(2.5f, 0, 0);
                        else if (modelIdx == 3)
                            absoluteWeaponTransform = Matrix.CreateTranslation(-3.5f, 0, 0);
                    }
                    else if (DebugLeftWeaponModelPositions && isLeft)
                    {
                        if (modelIdx == 0)
                            absoluteWeaponTransform = Matrix.CreateTranslation(0.5f, 0, 0);
                        else if (modelIdx == 1)
                            absoluteWeaponTransform = Matrix.CreateTranslation(1.5f, 0, 0);
                        else if (modelIdx == 2)
                            absoluteWeaponTransform = Matrix.CreateTranslation(2.5f, 0, 0);
                        else if (modelIdx == 3)
                            absoluteWeaponTransform = Matrix.CreateTranslation(3.5f, 0, 0);
                    }

                    wpnMdl.StartTransform = new Transform(
                            (backward ? Matrix.CreateRotationX(MathHelper.Pi) : Matrix.Identity)
                            * (sideways ? Matrix.CreateRotationY(MathHelper.Pi) : Matrix.Identity)
                            * absoluteWeaponTransform
                            * ((MODEL.AnimContainer?.EnableRootMotion == true) ? (MODEL.CurrentRootMotionRotation * MODEL.CurrentRootMotionTranslation) : Matrix.Identity));

                    wpnMdl.CurrentRootMotionTranslation = Matrix.Identity;

                    wpnMdl.AfterAnimUpdate(timeDelta, ignorePosWrap: true);
                }
            }

            DoWPN(RightWeapon, RightWeaponModel0, 0, RightWeaponBoneIndex, isLeft: false, RightWeaponFlipBackwards, RightWeaponFlipSideways);
            DoWPN(RightWeapon, RightWeaponModel1, 1, RightWeaponBoneIndex, isLeft: false, RightWeaponFlipBackwards, RightWeaponFlipSideways);
            DoWPN(RightWeapon, RightWeaponModel2, 2, RightWeaponBoneIndex, isLeft: false, RightWeaponFlipBackwards, RightWeaponFlipSideways);
            DoWPN(RightWeapon, RightWeaponModel3, 3, RightWeaponBoneIndex, isLeft: false, RightWeaponFlipBackwards, RightWeaponFlipSideways);

            DoWPN(LeftWeapon, LeftWeaponModel0, 0, LeftWeaponBoneIndex, isLeft: true, LeftWeaponFlipBackwards, LeftWeaponFlipSideways);
            DoWPN(LeftWeapon, LeftWeaponModel1, 1, LeftWeaponBoneIndex, isLeft: true, LeftWeaponFlipBackwards, LeftWeaponFlipSideways);
            DoWPN(LeftWeapon, LeftWeaponModel2, 2, LeftWeaponBoneIndex, isLeft: true, LeftWeaponFlipBackwards, LeftWeaponFlipSideways);
            DoWPN(LeftWeapon, LeftWeaponModel3, 3, LeftWeaponBoneIndex, isLeft: true, LeftWeaponFlipBackwards, LeftWeaponFlipSideways);
        }

        public void UpdateWeaponAnimation(float timeDelta)
        {
            void DoWPN(Model wpnMdl)
            {
                if (wpnMdl == null)
                    return;

                if (wpnMdl != null && wpnMdl.AnimContainer != null)
                {
                    if (wpnMdl.IsStatic)
                    {
                        //V2.0
                        //RightWeaponModel.AnimContainer.IsLoop = false;
                        if (wpnMdl.AnimContainer.AnimationLayers.Count == 2)
                        {
                            if (MODEL.AnimContainer.AnimationLayers.Count == 2)
                            {
                                wpnMdl.AnimContainer.AnimationLayers[0].Weight = MODEL.AnimContainer.AnimationLayers[0].Weight;
                                wpnMdl.AnimContainer.AnimationLayers[1].Weight = MODEL.AnimContainer.AnimationLayers[1].Weight;
                            }
                            else
                            {
                                wpnMdl.AnimContainer.AnimationLayers[0].Weight = 0;
                                wpnMdl.AnimContainer.AnimationLayers[1].Weight = 1;
                            }

                        }
                        else if (wpnMdl.AnimContainer.AnimationLayers.Count == 1)
                        {
                            wpnMdl.AnimContainer.AnimationLayers[0].Weight = 1;
                        }

                        wpnMdl.AnimContainer.ScrubRelative(timeDelta);

                        if (MODEL.AnimContainer.CurrentAnimDuration.HasValue && wpnMdl.AnimContainer.CurrentAnimDuration.HasValue)
                        {
                            float curModTime = MODEL.AnimContainer.CurrentAnimTime % MODEL.AnimContainer.CurrentAnimDuration.Value;

                            // Make subsequent loops of player anim be the subsequent loops of the weapon anim so it will not blend
                            if (MODEL.AnimContainer.CurrentAnimTime >= MODEL.AnimContainer.CurrentAnimDuration.Value)
                                curModTime += wpnMdl.AnimContainer.CurrentAnimDuration.Value;

                            wpnMdl.AnimContainer.ScrubRelative(curModTime - wpnMdl.AnimContainer.CurrentAnimTime);
                        }
                    }
                    else
                    {
                        wpnMdl.Skeleton.ApplyBakedFlverReferencePose();
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

        public void InitSkeleton(NewAnimSkeleton skeleton)
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
            FLVER2 flver = null;
            foreach (var f in partsbnd.Files)
            {
                string nameCheck = f.Name.ToLower();
                if (nameCheck.EndsWith(".tpf") || TPF.Is(f.Bytes))
                {
                    tpfs.Add(TPF.Read(f.Bytes));
                }
                else if (flver == null && nameCheck.EndsWith(".flver") || FLVER2.Is(f.Bytes))
                {
                    flver = FLVER2.Read(f.Bytes);
                }
            }

            foreach (var tpf in tpfs)
            {
                TexturePool.AddTpf(tpf);
            }

            NewMesh mesh = null;

            if (flver != null)
            {
                mesh = new NewMesh(flver, false, boneIndexRemap);
            }
            Scene.RequestTextureLoad();

            return mesh;
        }

        public void LoadArmorPartsbnd(IBinder partsbnd, EquipSlot slot)
        {
            if (slot == EquipSlot.Head)
            {
                HeadMesh?.Dispose();
                HeadMesh = LoadArmorMesh(partsbnd);
            }
            else if (slot == EquipSlot.Body)
            {
                BodyMesh?.Dispose();
                BodyMesh = LoadArmorMesh(partsbnd);
            }
            else if (slot == EquipSlot.Arms)
            {
                ArmsMesh?.Dispose();
                ArmsMesh = LoadArmorMesh(partsbnd);
            }
            else if (slot == EquipSlot.Legs)
            {
                LegsMesh?.Dispose();
                LegsMesh = LoadArmorMesh(partsbnd);
            }
            else if (slot == EquipSlot.Face)
            {
                FaceMesh?.Dispose();
                FaceMesh = LoadArmorMesh(partsbnd);
            }
            else if (slot == EquipSlot.Facegen)
            {
                FacegenMesh?.Dispose();
                FacegenMesh = LoadArmorMesh(partsbnd);
            }
        }

        public void LoadArmorPartsbnd(string partsbndPath, EquipSlot slot)
        {
            if (System.IO.File.Exists(partsbndPath))
            {
                if (BND3.Is(partsbndPath))
                {
                    LoadArmorPartsbnd(BND3.Read(partsbndPath), slot);
                }
                else
                {
                    LoadArmorPartsbnd(BND4.Read(partsbndPath), slot);
                }
            }
            else
            {
                if (slot == EquipSlot.Head)
                {
                    HeadMesh?.Dispose();
                    HeadMesh = null;
                }
                else if (slot == EquipSlot.Body)
                {
                    BodyMesh?.Dispose();
                    BodyMesh = null;
                }
                else if (slot == EquipSlot.Arms)
                {
                    ArmsMesh?.Dispose();
                    ArmsMesh = null;
                }
                else if (slot == EquipSlot.Legs)
                {
                    LegsMesh?.Dispose();
                    LegsMesh = null;
                }
                else if (slot == EquipSlot.Face)
                {
                    FaceMesh?.Dispose();
                    FaceMesh = null;
                }
                else if (slot == EquipSlot.Facegen)
                {
                    FacegenMesh?.Dispose();
                    FacegenMesh = null;
                }
            }
           
        }

        public void Draw(bool[] mask, int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            UpdateWeaponTransforms(0);

            if (FaceMesh != null)
            {
                FaceMesh.DrawMask = mask;
                FaceMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }

            if (FacegenMesh != null)
            {
                FacegenMesh.DrawMask = mask;
                FacegenMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }

            if (HeadMesh != null)
            {
                HeadMesh.DrawMask = mask;
                HeadMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }

            if (BodyMesh != null)
            {
                BodyMesh.DrawMask = mask;
                BodyMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }

            if (ArmsMesh != null)
            {
                ArmsMesh.DrawMask = mask;
                ArmsMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }

            if (LegsMesh != null)
            {
                LegsMesh.DrawMask = mask;
                LegsMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }

            void DrawWeapon(ParamData.EquipParamWeapon wpn, Model wpnMdl, int modelIdx, bool isLeft)
            {
                if (wpn == null || wpnMdl == null)
                {
                    return;
                }

                bool renderWpn = true;

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS1 || GameDataManager.GameType == GameDataManager.GameTypes.DS1R)
                {
                    renderWpn = (modelIdx == 0);
                }
                else if (GameDataManager.GameType == GameDataManager.GameTypes.DS3 || GameDataManager.GameType == GameDataManager.GameTypes.SDT)
                {
                    var dummyPolyID = isLeft ? wpn.DS3_GetLeftWeaponDummyPoly(this, modelIdx)
                            : wpn.DS3_GetRightWeaponDummyPoly(this, modelIdx);

                    if (dummyPolyID < 0)
                    {
                        renderWpn = false;
                    }
                }
                else if (GameDataManager.GameType == GameDataManager.GameTypes.BB)
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
                    float prevOpacity = GFX.FlverShader.Effect.Opacity;
                    GFX.FlverShader.Effect.Opacity = 0.2f;
                    wpnMdl?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
                    GFX.FlverShader.Effect.Opacity = prevOpacity;
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

        public void UpdateModels(bool isAsync = false, Action onCompleteAction = null)
        {
            LoadingTaskMan.DoLoadingTask("ChrAsm_UpdateModels", "Updating c0000 models...", progress =>
            {
                MODEL.ResetDrawMaskToAllVisible();

                if (GameDataManager.GameType == GameDataManager.GameTypes.DS3)
                {
                    LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                    LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fg_a_0100.partsbnd.dcx"), EquipSlot.Facegen);
                }
                else if (GameDataManager.GameType == GameDataManager.GameTypes.BB)
                {
                    LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                    LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fg_a_0100.partsbnd.dcx"), EquipSlot.Facegen);
                }
                else if (GameDataManager.GameType == GameDataManager.GameTypes.SDT)
                {
                    LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fc_m_0100.partsbnd.dcx"), EquipSlot.Face);

                    FacegenMesh?.Dispose();
                    FacegenMesh = null;

                }

                if (HeadID != lastHeadLoaded)
                {
                    if (Head != null)
                    {
                        LoadArmorPartsbnd(Head.GetFullPartBndPath(IsFemale), EquipSlot.Head);
                        lastHeadLoaded = HeadID;
                    }
                    else
                    {
                        HeadMesh?.Dispose();
                        HeadMesh = null;
                    }
                }

                progress.Report(1.0 / 6.0);

                if (BodyID != lastBodyLoaded)
                {
                    if (Body != null)
                    {
                        LoadArmorPartsbnd(Body.GetFullPartBndPath(IsFemale), EquipSlot.Body);
                        lastBodyLoaded = BodyID;
                    }
                    else
                    {
                        BodyMesh?.Dispose();
                        BodyMesh = null;
                    }
                }

                progress.Report(2.0 / 6.0);

                if (ArmsID != lastArmsLoaded)
                {
                    if (Arms != null)
                    {
                        LoadArmorPartsbnd(Arms.GetFullPartBndPath(IsFemale), EquipSlot.Arms);
                        lastArmsLoaded = ArmsID;
                    }
                    else
                    {
                        ArmsMesh?.Dispose();
                        ArmsMesh = null;
                    }
                }

                progress.Report(3.0 / 6.0);

                if (LegsID != lastLegsLoaded)
                {
                    if (Legs != null)
                    {
                        LoadArmorPartsbnd(Legs.GetFullPartBndPath(IsFemale), EquipSlot.Legs);
                        lastLegsLoaded = LegsID;
                    }
                    else
                    {
                        LegsMesh?.Dispose();
                        LegsMesh = null;
                    }
                }

                progress.Report(4.0 / 6.0);

                if ((RightWeaponID != lastRightWeaponLoaded))
                {
                    if (RightWeapon != null)
                    {
                        lastRightWeaponLoaded = RightWeaponID;
                        //TODO
                        LoadingTaskMan.DoLoadingTaskSynchronous("ChrAsm_RightWeapon", "Loading c0000 right weapon...", wpProgress =>
                        {
                            try
                            {
                                var weaponName = RightWeapon.GetFullPartBndPath();
                                var shortWeaponName = RightWeapon.GetPartBndName();
                                IBinder weaponBnd = null;

                                if (System.IO.File.Exists(weaponName))
                                {
                                    if (BND3.Is(weaponName))
                                        weaponBnd = BND3.Read(weaponName);
                                    else
                                        weaponBnd = BND4.Read(weaponName);

                                    RightWeaponModel0 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 0, null, ignoreStaticTransforms: true);
                                    if (RightWeaponModel0.AnimContainer == null)
                                    {
                                        RightWeaponModel0?.Dispose();
                                        RightWeaponModel0 = null;
                                    }
                                    else
                                    {
                                        RightWeaponModel0.IS_PLAYER = true;
                                        RightWeaponModel0.IS_PLAYER_WEAPON = true;
                                        RightWeaponModel0.DummyPolyMan.GlobalDummyPolyIDOffset = 10000;
                                        RightWeaponModel0.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 0 - ";
                                    }

                                    RightWeaponModel1 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 1, null, ignoreStaticTransforms: true);
                                    if (RightWeaponModel1.AnimContainer == null)
                                    {
                                        RightWeaponModel1?.Dispose();
                                        RightWeaponModel1 = null;
                                    }
                                    else
                                    {
                                        RightWeaponModel1.IS_PLAYER = true;
                                        RightWeaponModel1.IS_PLAYER_WEAPON = true;
                                        RightWeaponModel1.DummyPolyMan.GlobalDummyPolyIDOffset = 11000;
                                        RightWeaponModel1.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 1 - ";
                                    }

                                    RightWeaponModel2 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 2, null, ignoreStaticTransforms: true);
                                    if (RightWeaponModel2.AnimContainer == null)
                                    {
                                        RightWeaponModel2?.Dispose();
                                        RightWeaponModel2 = null;
                                    }
                                    else
                                    {
                                        RightWeaponModel2.IS_PLAYER = true;
                                        RightWeaponModel2.IS_PLAYER_WEAPON = true;
                                        RightWeaponModel2.DummyPolyMan.GlobalDummyPolyIDOffset = 12000;
                                        RightWeaponModel2.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 2 - ";
                                    }

                                    RightWeaponModel3 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 3, null, ignoreStaticTransforms: true);
                                    if (RightWeaponModel3.AnimContainer == null)
                                    {
                                        RightWeaponModel3?.Dispose();
                                        RightWeaponModel3 = null;
                                    }
                                    else
                                    {
                                        RightWeaponModel3.IS_PLAYER = true;
                                        RightWeaponModel3.IS_PLAYER_WEAPON = true;
                                        RightWeaponModel3.DummyPolyMan.GlobalDummyPolyIDOffset = 13000;
                                        RightWeaponModel3.DummyPolyMan.GlobalDummyPolyIDPrefix = "R WPN Model 3 - ";
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                lastRightWeaponLoaded = -1;
                                RightWeaponModel0?.Dispose();
                                RightWeaponModel0 = null;
                                RightWeaponModel1?.Dispose();
                                RightWeaponModel1 = null;
                                RightWeaponModel2?.Dispose();
                                RightWeaponModel2 = null;
                                RightWeaponModel3?.Dispose();
                                RightWeaponModel3 = null;
                                System.Windows.Forms.MessageBox.Show(
                                    $"Failed to load right-hand weapon model:\n\n{ex}",
                                    "Failed To Load RH Weapon Model",
                                    System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Error);
                            }
                        });
                    }
                }

                progress.Report(5.0 / 6.0);

                if ((LeftWeaponID != lastLeftWeaponLoaded))
                {
                    if (LeftWeapon != null)
                    {
                        lastLeftWeaponLoaded = LeftWeaponID;
                        //TODO
                        LoadingTaskMan.DoLoadingTaskSynchronous("ChrAsm_LeftWeapon", "Loading c0000 right weapon...", wpProgress =>
                        {
                            try
                            {
                                var weaponName = LeftWeapon.GetFullPartBndPath();
                                var shortWeaponName = LeftWeapon.GetPartBndName();
                                IBinder weaponBnd = null;

                                if (System.IO.File.Exists(weaponName))
                                {
                                    if (BND3.Is(weaponName))
                                        weaponBnd = BND3.Read(weaponName);
                                    else
                                        weaponBnd = BND4.Read(weaponName);

                                    LeftWeaponModel0 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 0, null, ignoreStaticTransforms: true);
                                    if (LeftWeaponModel0.AnimContainer == null)
                                    {
                                        LeftWeaponModel0?.Dispose();
                                        LeftWeaponModel0 = null;
                                    }
                                    else
                                    {
                                        LeftWeaponModel0.IS_PLAYER = true;
                                        LeftWeaponModel0.IS_PLAYER_WEAPON = true;
                                        LeftWeaponModel0.DummyPolyMan.GlobalDummyPolyIDOffset = 20000;
                                        LeftWeaponModel0.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 0 - ";
                                    }

                                    LeftWeaponModel1 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 1, null, ignoreStaticTransforms: true);
                                    if (LeftWeaponModel1.AnimContainer == null)
                                    {
                                        LeftWeaponModel1?.Dispose();
                                        LeftWeaponModel1 = null;
                                    }
                                    else
                                    {
                                        LeftWeaponModel1.IS_PLAYER = true;
                                        LeftWeaponModel1.IS_PLAYER_WEAPON = true;
                                        LeftWeaponModel1.DummyPolyMan.GlobalDummyPolyIDOffset = 21000;
                                        LeftWeaponModel1.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 1 - ";
                                    }

                                    LeftWeaponModel2 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 2, null, ignoreStaticTransforms: true);
                                    if (LeftWeaponModel2.AnimContainer == null)
                                    {
                                        LeftWeaponModel2?.Dispose();
                                        LeftWeaponModel2 = null;
                                    }
                                    else
                                    {
                                        LeftWeaponModel2.IS_PLAYER = true;
                                        LeftWeaponModel2.IS_PLAYER_WEAPON = true;
                                        LeftWeaponModel2.DummyPolyMan.GlobalDummyPolyIDOffset = 22000;
                                        LeftWeaponModel2.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 2 - ";
                                    }

                                    LeftWeaponModel3 = new Model(wpProgress, shortWeaponName, weaponBnd, modelIndex: 3, null, ignoreStaticTransforms: true);
                                    if (LeftWeaponModel3.AnimContainer == null)
                                    {
                                        LeftWeaponModel3?.Dispose();
                                        LeftWeaponModel3 = null;
                                    }
                                    else
                                    {
                                        LeftWeaponModel3.IS_PLAYER = true;
                                        LeftWeaponModel3.IS_PLAYER_WEAPON = true;
                                        LeftWeaponModel3.DummyPolyMan.GlobalDummyPolyIDOffset = 23000;
                                        LeftWeaponModel3.DummyPolyMan.GlobalDummyPolyIDPrefix = "L WPN Model 3 - ";
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                lastLeftWeaponLoaded = -1;
                                LeftWeaponModel0?.Dispose();
                                LeftWeaponModel0 = null;
                                LeftWeaponModel1?.Dispose();
                                LeftWeaponModel1 = null;
                                LeftWeaponModel2?.Dispose();
                                LeftWeaponModel2 = null;
                                LeftWeaponModel3?.Dispose();
                                LeftWeaponModel3 = null;
                                System.Windows.Forms.MessageBox.Show(
                                    $"Failed to load left-hand weapon model:\n\n{ex}",
                                    "Failed To Load LH Weapon Model",
                                    System.Windows.Forms.MessageBoxButtons.OK,
                                    System.Windows.Forms.MessageBoxIcon.Error);
                            }


                        });
                    }
                    else
                    {
                        lastLeftWeaponLoaded = -1;
                        LeftWeaponModel0?.Dispose();
                        LeftWeaponModel0 = null;
                        LeftWeaponModel1?.Dispose();
                        LeftWeaponModel1 = null;
                        LeftWeaponModel2?.Dispose();
                        LeftWeaponModel2 = null;
                        LeftWeaponModel3?.Dispose();
                        LeftWeaponModel3 = null;
                    }

                }

                if (Head != null)
                    MODEL.DefaultDrawMask = Head.ApplyInvisFlagsToMask(MODEL.DrawMask);
                if (Body != null)
                    MODEL.DefaultDrawMask = Body.ApplyInvisFlagsToMask(MODEL.DrawMask);
                if (Arms != null)
                    MODEL.DefaultDrawMask = Arms.ApplyInvisFlagsToMask(MODEL.DrawMask);
                if (Legs != null)
                    MODEL.DefaultDrawMask = Legs.ApplyInvisFlagsToMask(MODEL.DrawMask);

                MODEL.ResetDrawMaskToDefault();

                progress.Report(6.0 / 6.0);
                onCompleteAction?.Invoke();
                OnEquipmentModelsUpdated();
            }, waitForTaskToComplete: !isAsync);
                
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

        public NewDummyPolyManager GetDummyPolySpawnPlace(ParamData.AtkParam.DummyPolySource defaultDummySource, int dmy)
        {
            if (dmy == -1)
                return null;
            int check = dmy / 1000;

            if (check == 0)
                return GetDummyManager(defaultDummySource);
            else if (check == 10)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon0);
            else if (check == 11)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon1);
            else if (check == 12)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon2);
            else if (check == 13)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.RightWeapon3);
            else if (check == 20)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon0);
            else if (check == 21)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon1);
            else if (check == 22)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon2);
            else if (check == 23)
                return GetDummyManager(ParamData.AtkParam.DummyPolySource.LeftWeapon3);

            return null;
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
