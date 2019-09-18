using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }

        public bool IsFemale = false;

        private int _headID = -1;
        private int _bodyID = -1;
        private int _armsID = -1;
        private int _legsID = -1;
        private int _rightWeaponID = -1;
        private int _rightWeaponModelIndex = 0;
        private int _leftWeaponID = -1;
        private int _leftWeaponModelIndex = 0;

        public NewMesh HeadMesh = null;
        public NewMesh BodyMesh = null;
        public NewMesh ArmsMesh = null;
        public NewMesh LegsMesh = null;

        public NewAnimSkeleton Skeleton { get; private set; } = null;

        public int RightWeaponBoneIndex = -1;
        public Model RightWeaponModel = null;

        public int LeftWeaponBoneIndex = -1;
        public Model LeftWeaponModel = null;

        public bool LeftWeaponFlip_ForDS1Shields = true;

        public bool FlipAnimatedLeftWeaponBackwards = true;
        public bool FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows = false;
        public bool FlipAnimatedRightWeaponBackwards = true;

        private Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

        public readonly Model MODEL;

        public NewChrAsm(Model mdl)
        {
            MODEL = mdl;
        }

        public void UpdateWeaponTransforms()
        {
            if (RightWeaponModel != null)
            {
                if (RightWeaponModel.AnimContainer.CurrentAnimation == null)
                {
                    RightWeaponModel.StartTransform = new Transform(
                        Matrix.CreateRotationX(MathHelper.Pi)
                        * Matrix.CreateRotationY(MathHelper.Pi)
                        * Skeleton.FlverSkeleton[RightWeaponBoneIndex].ReferenceMatrix
                        * Skeleton[RightWeaponBoneIndex]
                        * MODEL.AnimContainer.CurrentAnimRootMotionMatrix);
                }
                else
                {
                    RightWeaponModel.StartTransform = new Transform(
                        (FlipAnimatedRightWeaponBackwards ? Matrix.CreateRotationX(MathHelper.Pi) : Matrix.Identity)
                        * Skeleton.FlverSkeleton[RightWeaponBoneIndex].ReferenceMatrix
                        * Skeleton[RightWeaponBoneIndex]
                        * MODEL.AnimContainer.CurrentAnimRootMotionMatrix);
                }
            }

            if (LeftWeaponModel != null)
            {
                if (LeftWeaponModel.AnimContainer.CurrentAnimation == null)
                {
                    LeftWeaponModel.StartTransform = new Transform(
                    Matrix.CreateRotationX(MathHelper.Pi)
                    * (LeftWeaponFlip_ForDS1Shields ? Matrix.Identity : Matrix.CreateRotationY(MathHelper.Pi))
                    * Skeleton.FlverSkeleton[LeftWeaponBoneIndex].ReferenceMatrix
                    * Skeleton[LeftWeaponBoneIndex]
                    * MODEL.AnimContainer.CurrentAnimRootMotionMatrix);
                }
                else
                {
                    LeftWeaponModel.StartTransform = new Transform(
                        (FlipAnimatedLeftWeaponBackwards ? Matrix.CreateRotationX(MathHelper.Pi) : Matrix.Identity)
                        * (FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows ? Matrix.CreateRotationY(MathHelper.Pi) : Matrix.Identity)
                        * Skeleton.FlverSkeleton[LeftWeaponBoneIndex].ReferenceMatrix
                        * Skeleton[LeftWeaponBoneIndex]
                        * MODEL.AnimContainer.CurrentAnimRootMotionMatrix);
                }
            }
        }

        public void UpdateWeaponAnimation(GameTime gameTime)
        {
            if (RightWeaponModel != null)
            {
                RightWeaponModel.AnimContainer.IsLoop = false;
                RightWeaponModel.AnimContainer.ScrubCurrentAnimation(MODEL.AnimContainer.CurrentAnimTime);
            }

            if (LeftWeaponModel != null)
            {
                LeftWeaponModel.AnimContainer.IsLoop = false;
                LeftWeaponModel.AnimContainer.ScrubCurrentAnimation(MODEL.AnimContainer.CurrentAnimTime);
            }
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

        public void InitWeapon(bool isLeft)
        {
            var wpn = isLeft ? LeftWeapon : RightWeapon;
            var wpnMdl = isLeft ? LeftWeaponModel : RightWeaponModel;

            if (wpn != null && wpnMdl != null)
            {
                MODEL.DummyPolyMan.BuildAllHitboxPrimitives(wpn, isLeft);
            }
        }

        private NewMesh LoadArmorMesh(IBinder partsbnd)
        {
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
                }
                else if (slot == EquipSlot.Body)
                {
                    BodyMesh?.Dispose();
                }
                else if (slot == EquipSlot.Arms)
                {
                    ArmsMesh?.Dispose();
                }
                else if (slot == EquipSlot.Legs)
                {
                    LegsMesh?.Dispose();
                }
            }
            
        }

        public void Draw(bool[] mask, int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
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

            if (RightWeaponModel != null)
            {
                RightWeaponModel.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }

            if (LeftWeaponModel != null)
            {
                LeftWeaponModel.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }
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

        public int RightWeaponModelIndex
        {
            get => _rightWeaponModelIndex;
            set
            {
                if (_rightWeaponModelIndex != value)
                {
                    _rightWeaponModelIndex = value;
                    OnEquipmentChanged(EquipSlot.RightWeapon);
                }
            }
        }

        public int LeftWeaponModelIndex
        {
            get => _leftWeaponModelIndex;
            set
            {
                if (_leftWeaponModelIndex != value)
                {
                    _leftWeaponModelIndex = value;
                    OnEquipmentChanged(EquipSlot.LeftWeapon);
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

        public event EventHandler<EquipSlot> EquipmentChanged;

        private void OnEquipmentChanged(EquipSlot slot)
        {
            EquipmentChanged?.Invoke(this, slot);
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

        public void UpdateModels()
        {
            MODEL.DefaultAllMaskValues();
            if (Head != null)
            {
                LoadArmorPartsbnd(Head.GetFullPartBndPath(IsFemale), EquipSlot.Head);
                Head.ApplyInvisFlagsToMask(ref MODEL.DrawMask);
            }

            if (Body != null)
            {
                LoadArmorPartsbnd(Body.GetFullPartBndPath(IsFemale), EquipSlot.Body);
                Body.ApplyInvisFlagsToMask(ref MODEL.DrawMask);
            }

            if (Arms != null)
            {
                LoadArmorPartsbnd(Arms.GetFullPartBndPath(IsFemale), EquipSlot.Arms);
                Arms.ApplyInvisFlagsToMask(ref MODEL.DrawMask);
            }

            if (Legs != null)
            {
                LoadArmorPartsbnd(Legs.GetFullPartBndPath(IsFemale), EquipSlot.Legs);
                Legs.ApplyInvisFlagsToMask(ref MODEL.DrawMask);
            }

            if (RightWeapon != null)
            {
                //TODO
                LoadingTaskMan.DoLoadingTaskSynchronous("ChrAsm_RightWeapon", "Loading right weapon...", progress =>
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
                        RightWeaponModel = new Model(progress, shortWeaponName, weaponBnd,
                            modelIndex: RightWeaponModelIndex, null/*, baseDmyPolyID: 10000*/);
                        RightWeaponModel.IS_PLAYER = true;
                        RightWeaponModel.IS_PLAYER_WEAPON = true;
                        RightWeaponModel.ParentModelForChrAsm = MODEL;
                        InitWeapon(isLeft: false);
                    }

                       
                });
                
            }

            if (LeftWeapon != null)
            {
                //TODO
                LoadingTaskMan.DoLoadingTaskSynchronous("ChrAsm_LeftWeapon", "Loading right weapon...", progress =>
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
                        LeftWeaponModel = new Model(progress, shortWeaponName, weaponBnd,
                            modelIndex: LeftWeaponModelIndex, null/*, baseDmyPolyID: 11000*/);
                        LeftWeaponModel.IS_PLAYER = true;
                        LeftWeaponModel.IS_PLAYER_WEAPON = true;
                        LeftWeaponModel.ParentModelForChrAsm = MODEL;
                        InitWeapon(isLeft: true);
                    }

                   
                });
            }
        }

        public void TryToLoadTextures()
        {
            HeadMesh?.TryToLoadTextures();
            BodyMesh?.TryToLoadTextures();
            ArmsMesh?.TryToLoadTextures();
            LegsMesh?.TryToLoadTextures();
            RightWeaponModel?.TryToLoadTextures();
            LeftWeaponModel?.TryToLoadTextures();
        }

        public void Dispose()
        {
            HeadMesh?.Dispose();
            BodyMesh?.Dispose();
            ArmsMesh?.Dispose();
            LegsMesh?.Dispose();
            RightWeaponModel?.Dispose();
            LeftWeaponModel?.Dispose();
        }
    }
}
