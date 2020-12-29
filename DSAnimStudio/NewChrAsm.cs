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
            int ds3ConditionFlag = GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3
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
            int ds3ConditionFlag = GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3
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
            if (GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS3)
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

                    if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
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
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 || 
                        GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
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
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
                        GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R ||
                        GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DES)
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

            var isOldGame = GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || 
                GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R || 
                GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DES;
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
                        if (matching != null)
                        {
                            wpnMdl.AnimContainer.CurrentAnimationName = matching;
                        }
                        else
                        {
                            matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, GameDataManager.GameTypeHasLongAnimIDs ? "a999_000000.hkx" : "a99_0000.hkx");
                            if (matching != null)
                            {
                                wpnMdl.AnimContainer.CurrentAnimationName = matching;
                            }
                            else
                            {
                                matching = GetMatchingAnim(wpnMdl.AnimContainer.Animations.Keys, GameDataManager.GameTypeHasLongAnimIDs ? "a000_000000.hkx" : "a00_0000.hkx");
                                if (matching != null)
                                {
                                    wpnMdl.AnimContainer.CurrentAnimationName = matching;
                                }
                                else
                                {
                                    wpnMdl.AnimContainer.CurrentAnimationName =
                                        wpnMdl.AnimContainer.Animations.Keys.First();
                                }
                            }
                        }
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

                        wpnMdl.AnimContainer.ScrubRelative(timeDelta);
                        //wpnMdl.AfterAnimUpdate(timeDelta);

                        if (MODEL.AnimContainer.CurrentAnimDuration.HasValue && wpnMdl.AnimContainer.CurrentAnimDuration.HasValue)
                        {
                            float curModTime = MODEL.AnimContainer.CurrentAnimTime % MODEL.AnimContainer.CurrentAnimDuration.Value;

                            // Make subsequent loops of player anim be the subsequent loops of the weapon anim so it will not blend
                            if (MODEL.AnimContainer.CurrentAnimTime >= MODEL.AnimContainer.CurrentAnimDuration.Value)
                                curModTime += wpnMdl.AnimContainer.CurrentAnimDuration.Value;

                            wpnMdl.AnimContainer.ScrubRelative(curModTime - wpnMdl.AnimContainer.CurrentAnimTime);
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
                    tpfs.Add(TPF.Read(f.Bytes));
                }
                else if (GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DES && (flver2 == null && nameCheck.EndsWith(".flver") || FLVER2.Is(f.Bytes)))
                {
                    flver2 = FLVER2.Read(f.Bytes);
                }
                else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DES && (flver0 == null && nameCheck.EndsWith(".flver") || FLVER0.Is(f.Bytes)))
                {
                    flver0 = FLVER0.Read(f.Bytes);
                }
            }

            foreach (var tpf in tpfs)
            {
                TexturePool.AddTpf(tpf);
            }

            NewMesh mesh = null;

            if (GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DES && flver2 != null)
            {
                mesh = new NewMesh(flver2, false, boneIndexRemap);
            }
            else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DES && flver0 != null)
            {
                mesh = new NewMesh(flver0, false, boneIndexRemap);
            }
            Scene.RequestTextureLoad();

            return mesh;
        }

        public void LoadArmorPartsbnd(IBinder partsbnd, EquipSlot slot)
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

            NewMesh newMesh = LoadArmorMesh(partsbnd);

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

            oldMesh?.Dispose();
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
            }
           
        }

        public void Draw(bool[] mask, int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            UpdateWeaponTransforms(0);
            MODEL.AfterAnimUpdate(0);

            if (FaceMesh != null)
            {
                FaceMesh.DrawMask = mask;
                FaceMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>FACE failed to draw:\n\n{ex}", 
                        Vector3.Transform(new Vector3(-3f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix), 
                        Color.Red, Color.Black, 10);
                });
            }

            if (FacegenMesh != null)
            {
                FacegenMesh.DrawMask = mask;
                FacegenMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>FACE failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(-2f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (HeadMesh != null)
            {
                HeadMesh.DrawMask = mask;
                HeadMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>HEAD failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(-1f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (BodyMesh != null)
            {
                BodyMesh.DrawMask = mask;
                BodyMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>BODY failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(1f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (ArmsMesh != null)
            {
                ArmsMesh.DrawMask = mask;
                ArmsMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, onDrawFail: (ex) =>
                {
                    ImGuiDebugDrawer.DrawText3D($"{MODEL.Name}=>ARMS failed to draw:\n\n{ex}",
                        Vector3.Transform(new Vector3(2f, -0.5f, 0.25f), MODEL.CurrentTransform.WorldMatrix),
                        Color.Red, Color.Black, 10);
                });
            }

            if (LegsMesh != null)
            {
                LegsMesh.DrawMask = mask;
                LegsMesh?.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, onDrawFail: (ex) =>
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

                bool renderWpn = true;

                if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                {
                    renderWpn = (modelIdx == 0);
                }
                else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3 || GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                {
                    var dummyPolyID = isLeft ? wpn.DS3_GetLeftWeaponDummyPoly(this, modelIdx)
                            : wpn.DS3_GetRightWeaponDummyPoly(this, modelIdx);

                    if (dummyPolyID < 0)
                    {
                        renderWpn = false;
                    }
                }
                else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
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

        public void UpdateModels(bool isAsync = false, Action onCompleteAction = null, bool updateFaceAndBody = true, bool forceReloadArmor = false)
        {
            LoadingTaskMan.DoLoadingTask("ChrAsm_UpdateModels", "Updating c0000 models...", progress =>
            {
                if (updateFaceAndBody)
                {
                    var oldMesh = FaceMesh;
                    FaceMesh = null;
                    oldMesh?.Dispose();

                    oldMesh = FacegenMesh;
                    FacegenMesh = null;
                    oldMesh?.Dispose();

                    if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS3)
                    {
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fg_a_0100.partsbnd.dcx"), EquipSlot.Facegen);
                    }
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.BB)
                    {
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fg_a_0100.partsbnd.dcx"), EquipSlot.Facegen);
                    }
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.SDT)
                    {
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fc_m_0100.partsbnd.dcx"), EquipSlot.Face);
                    }
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DES)
                    {
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\fc_{(IsFemale ? "f" : "m")}_0000.partsbnd.dcx"), EquipSlot.Face);
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"facegen\facegen.fgbnd"), EquipSlot.Facegen);
                    }
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                    {
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\FC_{(IsFemale ? "F" : "M")}_0000.partsbnd.dcx"), EquipSlot.Face);
                        //LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"facegen\FaceGen.fgbnd"), EquipSlot.Facegen);
                    }
                    else if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS1)
                    {
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"parts\FC_{(IsFemale ? "F" : "M")}_0000.partsbnd"), EquipSlot.Face);
                        LoadArmorPartsbnd(GameDataManager.GetInterrootPath($@"facegen\FaceGen.fgbnd"), EquipSlot.Facegen);
                    }
                }

                if (HeadID != lastHeadLoaded || forceReloadArmor)
                {
                    if (Head != null && Head.CanEquipOnGender(IsFemale))
                    {
                        LoadArmorPartsbnd(Head.GetFullPartBndPath(IsFemale), EquipSlot.Head);
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
                        LoadArmorPartsbnd(Body.GetFullPartBndPath(IsFemale), EquipSlot.Body);
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
                        LoadArmorPartsbnd(Arms.GetFullPartBndPath(IsFemale), EquipSlot.Arms);
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
                        LoadArmorPartsbnd(Legs.GetFullPartBndPath(IsFemale), EquipSlot.Legs);
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
                            var weaponName = RightWeapon.GetFullPartBndPath();
                            var shortWeaponName = RightWeapon.GetPartBndName();
                            IBinder weaponBnd = null;

                            if (System.IO.File.Exists(weaponName))
                            {
                                if (BND3.Is(weaponName))
                                    weaponBnd = BND3.Read(weaponName);
                                else
                                    weaponBnd = BND4.Read(weaponName);

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
                            var weaponName = LeftWeapon.GetFullPartBndPath();
                            var shortWeaponName = LeftWeapon.GetPartBndName();
                            IBinder weaponBnd = null;

                            if (System.IO.File.Exists(weaponName))
                            {
                                if (BND3.Is(weaponName))
                                    weaponBnd = BND3.Read(weaponName);
                                else
                                    weaponBnd = BND4.Read(weaponName);

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
