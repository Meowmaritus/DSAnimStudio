using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class ChrAsm
    {
        public enum PartsType
        {
            Head,
            Body,
            Arms,
            Legs,
            RightWeapon,
            LeftWeapon,
            Face,
            FaceGen
        }

        static Dictionary<PartsType, Model> PartsModels 
            = new Dictionary<PartsType, Model>();

        public static bool IsFemale = false;

        public static long EquipHead = -1;
        public static long EquipBody = -1;
        public static long EquipArms = -1;
        public static long EquipLegs = -1;
        public static long EquipRWeapon = -1;
        public static long EquipLWeapon = -1;

        public static ParamData.EquipParamProtector ParamHead { get; private set; }
        public static ParamData.EquipParamProtector ParamBody { get; private set; }
        public static ParamData.EquipParamProtector ParamArms { get; private set; }
        public static ParamData.EquipParamProtector ParamLegs { get; private set; }

        public static ParamData.EquipParamWeapon ParamRWeapon { get; private set; }
        public static ParamData.EquipParamWeapon ParamLWeapon { get; private set; }

        static bool prevIsFemale = false;
        static long prevEquipHead = -1;
        static long prevEquipBody = -1;
        static long prevEquipArms = -1;
        static long prevEquipLegs = -1;
        static long prevEquipRWeapon = -1;
        static long prevEquipLWeapon = -1;

        private static object _lock_UpdateEquipment = new object();

        private static void UpdateEquipmentModel(long rowID, PartsType type)
        {
            if (type == PartsType.LeftWeapon || type == PartsType.RightWeapon)
            {
                if (ParamManager.EquipParamWeapon.ContainsKey(rowID))
                {
                    var weaponParam = ParamManager.EquipParamWeapon[rowID];
                    var weaponName = $@"{TaeInterop.InterrootPath}\parts\WP_A_{weaponParam.EquipModelID:D4}.partsbnd";
                    int modelIdx = (TaeInterop.CurrentHkxVariation == HKX.HKXVariation.HKXDS3 && weaponParam.IsPairedWeaponDS3 && type == PartsType.LeftWeapon) ? 1 : 0;
                    try
                    {
                        if (type == PartsType.LeftWeapon)
                            ParamLWeapon = weaponParam;
                        else
                            ParamRWeapon = weaponParam;

                        LoadPartsFile(weaponName, modelIdx, type);
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        ClearPart(type);
                    }
                    
                }
                else
                {
                    ClearPart(type);
                }
            }
            else
            {
                if (ParamManager.EquipParamProtector.ContainsKey(rowID))
                {
                    var protectorParam = ParamManager.EquipParamProtector[rowID];
                    var protectorName = protectorParam.GetPartBndName(IsFemale);
                    if (protectorName != null)
                    {
                        switch (type)
                        {
                            case PartsType.Head:
                                ParamHead = protectorParam;
                                break;
                            case PartsType.Body:
                                ParamBody = protectorParam;
                                break;
                            case PartsType.Arms:
                                ParamArms = protectorParam;
                                break;
                            case PartsType.Legs:
                                ParamLegs = protectorParam;
                                break;
                        }

                        try
                        {
                            LoadPartsFile($@"{TaeInterop.InterrootPath}\parts\{protectorName}", 0, type);
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            ClearPart(type);
                        }
                    }
                    else
                    {
                        ClearPart(type);
                    }
                }
                else
                {
                    ClearPart(type);
                }
            }
        }

        public static void UpdateEquipment(bool forceReload = false)
        {
            LoadingTaskMan.DoLoadingTask("ChrAsm.UpdateEquipment", "Loading equipment...", progress =>
            {
                lock (_lock_UpdateEquipment)
                {
                    if (forceReload || EquipHead != prevEquipHead || IsFemale != prevIsFemale)
                        UpdateEquipmentModel(EquipHead, PartsType.Head);

                    progress.Report(1.0 / 6.0);

                    if (forceReload || EquipBody != prevEquipBody || IsFemale != prevIsFemale)
                        UpdateEquipmentModel(EquipBody, PartsType.Body);

                    progress.Report(2.0 / 6.0);

                    if (forceReload || EquipArms != prevEquipArms || IsFemale != prevIsFemale)
                        UpdateEquipmentModel(EquipArms, PartsType.Arms);

                    progress.Report(3.0 / 6.0);

                    if (forceReload || EquipLegs != prevEquipLegs || IsFemale != prevIsFemale)
                        UpdateEquipmentModel(EquipLegs, PartsType.Legs);

                    progress.Report(4.0 / 6.0);

                    if (forceReload || EquipRWeapon != prevEquipRWeapon)
                        UpdateEquipmentModel(EquipRWeapon, PartsType.RightWeapon);

                    progress.Report(5.0 / 6.0);

                    if (forceReload || EquipLWeapon != prevEquipLWeapon)
                        UpdateEquipmentModel(EquipLWeapon, PartsType.LeftWeapon);

                    prevEquipHead = EquipHead;
                    prevEquipBody = EquipBody;
                    prevEquipArms = EquipArms;
                    prevEquipLegs = EquipLegs;
                    prevEquipRWeapon = EquipRWeapon;
                    prevEquipLWeapon = EquipLWeapon;
                    prevIsFemale = IsFemale;

                    progress.Report(6.0 / 6.0);
                }
            });

            
        }

        private static System.Numerics.Vector4 TransformVertVec4ByBone(SoulsFormats.FLVER2.Bone b,
                System.Numerics.Vector4 vertVec4, bool upsideDown = true, bool isNormals = false, bool backward = true)
        {
            SoulsFormats.FLVER2.Bone parentBone = b;

            var result = Matrix.Identity;

            if (backward)
                result *= Matrix.CreateRotationY(MathHelper.Pi);

            if (upsideDown)
                result *= Matrix.CreateRotationZ(MathHelper.Pi);

           

            do
            {
                result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                if (parentBone.ParentIndex >= 0)
                {
                    parentBone = TaeInterop.CurrentModel.Bones[parentBone.ParentIndex];
                }
                else
                {
                    parentBone = null;
                }
            }
            while (parentBone != null);

            Vector4 newPos = Vector4.Zero;

            if (isNormals)
            {
                var thing = Vector3.TransformNormal(new Vector3(vertVec4.X, vertVec4.Y, vertVec4.Z), result);
                newPos = new Vector4(thing.X, thing.Y, thing.Z, vertVec4.W);
            }
            else
            {
                newPos = Vector4.Transform(new Vector4(vertVec4.X, vertVec4.Y, vertVec4.Z, vertVec4.W), result);
            }

            return new System.Numerics.Vector4(newPos.X, newPos.Y, newPos.Z, newPos.W);
        }

        private static System.Numerics.Vector3 TransformVertVec3ByBone(SoulsFormats.FLVER2.Bone b,
            System.Numerics.Vector3 vertVec3, bool upsideDown = true, bool isNormals = false, bool backward = true)
        {
            SoulsFormats.FLVER2.Bone parentBone = b;

            var result = Matrix.Identity;

            if (backward)
                result *= Matrix.CreateRotationY(MathHelper.Pi);

            if (upsideDown)
                result *= Matrix.CreateRotationZ(MathHelper.Pi);

            do
            {
                result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                if (parentBone.ParentIndex >= 0)
                {
                    parentBone = TaeInterop.CurrentModel.Bones[parentBone.ParentIndex];
                }
                else
                {
                    parentBone = null;
                }
            }
            while (parentBone != null);

            var newPos = isNormals ? Vector3.TransformNormal(new Vector3(vertVec3.X, vertVec3.Y, vertVec3.Z), result)
                : Vector3.Transform(new Vector3(vertVec3.X, vertVec3.Y, vertVec3.Z), result);
            return new System.Numerics.Vector3(newPos.X, newPos.Y, newPos.Z);
        }

        private static void ClearPart(PartsType t)
        {
            if (PartsModels.ContainsKey(t))
            {
                GFX.ModelDrawer.DeleteModel(PartsModels[t]);
            }
        }

        public static void LoadFace(string faceName)
        {
            LoadPartsFile($@"{TaeInterop.InterrootPath}\parts\{faceName}", 0, PartsType.Face);
        }

        public static void LoadFaceGen(string facegenName)
        {
            LoadPartsFile($@"{TaeInterop.InterrootPath}\parts\{facegenName}", 0, PartsType.FaceGen);
        }

        private static void LoadPartsFile(string p, int modelIdx, PartsType t)
        {
            Dictionary<string, int> baseChrBoneMap = new Dictionary<string, int>();
            for (int i = 0; i < TaeInterop.CurrentModel.Bones.Count; i++)
            {
                baseChrBoneMap.Add(TaeInterop.CurrentModel.Bones[i].Name, i);
            }

            List<TPF> tpfsUsed = new List<TPF>();

            if (!File.Exists(p))
            {
                if (File.Exists(p + ".dcx"))
                {
                    p = p + ".dcx";
                }
                else
                {
                    ClearPart(t);
                    return;
                }
            }

            FLVER2 partFlver = null;
            string debug_PartFlverName = null;

            bool CheckFlverName(string flverName)
            {
                var toUpper = flverName.ToUpper();

                if (modelIdx > 0)
                    return toUpper.EndsWith($"_{modelIdx}.FLVER");
                else if (modelIdx > 0)
                    return toUpper.EndsWith($"_{modelIdx}.FLVER");
                else
                    return toUpper.EndsWith($".FLVER");
            }

            if (BND4.Is(p))
            {
                var bnd = BND4.Read(p);

                foreach (var f in bnd.Files)
                {
                    if (partFlver == null && FLVER2.Is(f.Bytes) && CheckFlverName(f.Name))
                    {
                        partFlver = FLVER2.Read(f.Bytes);
                        debug_PartFlverName = f.Name;
                    }
                    else if (TPF.Is(f.Bytes))
                    {
                        tpfsUsed.Add(TPF.Read(f.Bytes));
                    }
                }
            }
            else if (BND3.Is(p))
            {
                var bnd = BND3.Read(p);

                foreach (var f in bnd.Files)
                {
                    if (partFlver == null && FLVER2.Is(f.Bytes))
                    {
                        partFlver = FLVER2.Read(f.Bytes);
                        debug_PartFlverName = f.Name;
                    }
                    else if (TPF.Is(f.Bytes))
                    {
                        tpfsUsed.Add(TPF.Read(f.Bytes));
                    }
                }
            }

            int weaponLBoneIndex = TaeInterop.CurrentModel.Bones.IndexOf(TaeInterop.CurrentModel.Bones.FirstOrDefault(b => b.Name == "L_Weapon"));
            int weaponRBoneIndex = TaeInterop.CurrentModel.Bones.IndexOf(TaeInterop.CurrentModel.Bones.FirstOrDefault(b => b.Name == "R_Weapon"));

            FLVER2.Bone weaponLBone = null;
            FLVER2.Bone weaponRBone = null;

            if (weaponLBoneIndex >= 0)
                weaponLBone = TaeInterop.CurrentModel.Bones[weaponLBoneIndex];

            if (weaponRBoneIndex >= 0)
                weaponRBone = TaeInterop.CurrentModel.Bones[weaponRBoneIndex];

            int RemapBone(int boneIndex)
            {
                if (boneIndex == -1)
                    return -1;

                if (baseChrBoneMap.ContainsKey(partFlver.Bones[boneIndex].Name))
                    return baseChrBoneMap[partFlver.Bones[boneIndex].Name];
                else
                    return -1;
            }

            foreach (var mesh in partFlver.Meshes)
            {
                // Check if DS1 memes
                if (partFlver.Header.Version <= 0x2000D)
                {
                    if (t == PartsType.LeftWeapon || t == PartsType.RightWeapon)
                    {
                        foreach (var vert in mesh.Vertices)
                        {
                            if (vert.BoneIndices != null)
                            {
                                vert.Position = TransformVertVec3ByBone(partFlver.Bones[mesh.BoneIndices[vert.BoneIndices[0]]], vert.Position, false);
                                vert.Normal = TransformVertVec4ByBone(partFlver.Bones[mesh.BoneIndices[vert.BoneIndices[0]]], vert.Normal, false, true);
                                vert.Tangents[0] = TransformVertVec4ByBone(partFlver.Bones[mesh.BoneIndices[vert.BoneIndices[0]]], vert.Tangents[0], false, true);
                                vert.Bitangent = TransformVertVec4ByBone(partFlver.Bones[mesh.BoneIndices[vert.BoneIndices[0]]], vert.Bitangent, false, true);
                            }
                            else if (mesh.DefaultBoneIndex >= 0)
                            {
                                vert.Position = TransformVertVec3ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Position, false);
                                vert.Normal = TransformVertVec4ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Normal, false, true);
                                vert.Tangents[0] = TransformVertVec4ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Tangents[0], false, true);
                                vert.Bitangent = TransformVertVec4ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Bitangent, false, true);
                            }

                            if (t == PartsType.RightWeapon)
                            {
                                vert.Position = TransformVertVec3ByBone(weaponRBone, vert.Position, true);
                                vert.Normal = TransformVertVec4ByBone(weaponRBone, vert.Normal, true, true);
                                vert.Tangents[0] = TransformVertVec4ByBone(weaponRBone, vert.Tangents[0], true, true);
                                vert.Bitangent = TransformVertVec4ByBone(weaponRBone, vert.Bitangent, true, true);
                            }
                            else if (t == PartsType.LeftWeapon)
                            {
                                vert.Position = TransformVertVec3ByBone(weaponLBone, vert.Position, true);
                                vert.Normal = TransformVertVec4ByBone(weaponLBone, vert.Normal, true, true);
                                vert.Tangents[0] = TransformVertVec4ByBone(weaponLBone, vert.Tangents[0], true, true);
                                vert.Bitangent = TransformVertVec4ByBone(weaponLBone, vert.Bitangent, true, true);
                            }

                            vert.BoneIndices = new int[] { 0, 0, 0, 0 };
                            vert.BoneWeights = new float[] { 1, 0, 0, 0 };
                        }

                        if (t == PartsType.RightWeapon)
                        {
                            mesh.BoneIndices = new List<int> { weaponRBoneIndex };
                            mesh.DefaultBoneIndex = weaponRBoneIndex;
                        }
                        else if (t == PartsType.LeftWeapon)
                        {
                            mesh.BoneIndices = new List<int> { weaponLBoneIndex };
                            mesh.DefaultBoneIndex = weaponLBoneIndex;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < mesh.BoneIndices.Count; i++)
                        {
                            mesh.BoneIndices[i] = RemapBone(mesh.BoneIndices[i]);
                        }

                        mesh.DefaultBoneIndex = RemapBone(mesh.DefaultBoneIndex);
                    }
                }
                else
                {
                    foreach (var vert in mesh.Vertices)
                    {
                        if (t == PartsType.LeftWeapon || t == PartsType.RightWeapon)
                        {
                            if (vert.BoneIndices != null)
                            {
                                vert.Position = TransformVertVec3ByBone(partFlver.Bones[vert.BoneIndices[0]], vert.Position, false, backward: false);
                                vert.Normal = TransformVertVec4ByBone(partFlver.Bones[vert.BoneIndices[0]], vert.Normal, false, true, backward: false);
                                vert.Tangents[0] = TransformVertVec4ByBone(partFlver.Bones[vert.BoneIndices[0]], vert.Tangents[0], false, true, backward: false);
                                vert.Bitangent = TransformVertVec4ByBone(partFlver.Bones[vert.BoneIndices[0]], vert.Bitangent, false, true, backward: false);
                            }
                            else if (mesh.DefaultBoneIndex >= 0)
                            {
                                vert.Position = TransformVertVec3ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Position, false, backward: false);
                                vert.Normal = TransformVertVec4ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Normal, false, true, backward: false);
                                vert.Tangents[0] = TransformVertVec4ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Tangents[0], false, true, backward: false);
                                vert.Bitangent = TransformVertVec4ByBone(partFlver.Bones[mesh.DefaultBoneIndex], vert.Bitangent, false, true, backward: false);
                            }

                            if (t == PartsType.RightWeapon)
                            {
                                vert.Position = TransformVertVec3ByBone(weaponRBone, vert.Position, true, backward: false);
                                vert.Normal = TransformVertVec4ByBone(weaponRBone, vert.Normal, true, true, backward: false);
                                vert.Tangents[0] = TransformVertVec4ByBone(weaponRBone, vert.Tangents[0], true, true, backward: false);
                                vert.Bitangent = TransformVertVec4ByBone(weaponRBone, vert.Bitangent, true, true, backward: false);
                            }
                            else if (t == PartsType.LeftWeapon)
                            {
                                vert.Position = TransformVertVec3ByBone(weaponLBone, vert.Position, true, backward: false);
                                vert.Normal = TransformVertVec4ByBone(weaponLBone, vert.Normal, true, true, backward: false);
                                vert.Tangents[0] = TransformVertVec4ByBone(weaponLBone, vert.Tangents[0], true, true, backward: false);
                                vert.Bitangent = TransformVertVec4ByBone(weaponLBone, vert.Bitangent, true, true, backward: false);
                            }

                            if (t == PartsType.RightWeapon)
                                vert.BoneIndices = new int[] { weaponRBoneIndex, 0, 0, 0 };
                            else if (t == PartsType.LeftWeapon)
                                vert.BoneIndices = new int[] { weaponLBoneIndex, 0, 0, 0 };

                            vert.BoneWeights = new float[] { 1, 0, 0, 0 };
                        }
                        else
                        {
                            for (int i = 0; i < vert.BoneIndices.Length; i++)
                            {
                                vert.BoneIndices[i] = RemapBone(vert.BoneIndices[i]);
                            }
                        }
                    }
                }

                //if (mesh.MaterialIndex >= partFlver.Materials.Count)
                //{
                //    mesh.MaterialIndex = partFlver.Materials.Count - 1;
                //}

                //TaeInterop.CurrentModel.Materials.Add(partFlver.Materials[mesh.MaterialIndex]);


                //if (partFlver.Materials[mesh.MaterialIndex].GXIndex >= 0 && partFlver.Materials[mesh.MaterialIndex].GXIndex < partFlver.GXLists.Count)
                //{
                //    baseChr.GXLists.Add(partFlver.GXLists[partFlver.Materials[mesh.MaterialIndex].GXIndex]);
                //    partFlver.Materials[mesh.MaterialIndex].GXIndex = baseChr.GXLists.Count - 1;
                //}

                //mesh.MaterialIndex = baseChr.Materials.Count - 1;

                //foreach (var vb in mesh.VertexBuffers)
                //{
                //    baseChr.BufferLayouts.Add(partFlver.BufferLayouts[vb.LayoutIndex]);
                //    vb.LayoutIndex = baseChr.BufferLayouts.Count - 1;
                //}

                //baseChr.Meshes.Add(mesh);

                
            }

            var partsModel = new Model(partFlver);

            if (!PartsModels.ContainsKey(t))
            {
                PartsModels.Add(t, partsModel);
            }
            else
            {
                GFX.ModelDrawer.DeleteModel(PartsModels[t]);
                PartsModels[t] = partsModel;
            }

            GFX.ModelDrawer.AddModelInstance(partsModel, new FileInfo(p).Name, Transform.Default);

            FLVER2.Bone wpDummyBone = null;
            int wpDummyBoneIndex = -1;

            if (t == PartsType.RightWeapon)
            {
                wpDummyBone = weaponRBone;
                wpDummyBoneIndex = weaponRBoneIndex;
            }
            else if (t == PartsType.LeftWeapon)
            {
                wpDummyBone = weaponLBone;
                wpDummyBoneIndex = weaponLBoneIndex;
            }

            //TexturePool.FlushDataOnly();

            if (t != PartsType.LeftWeapon && t != PartsType.RightWeapon)
            {
                var folder = new FileInfo(p).DirectoryName;

                var commonTpfPath = $@"{folder}\common_body.tpf";
                var commonTpfMPath = $@"{folder}\common_body_m.tpf";

                if (File.Exists(commonTpfPath + ".dcx"))
                    TexturePool.AddTpfFromPath(commonTpfPath + ".dcx");
                else if (File.Exists(commonTpfPath))
                    TexturePool.AddTpfFromPath(commonTpfPath);

                if (File.Exists(commonTpfMPath + ".dcx"))
                    TexturePool.AddTpfFromPath(commonTpfMPath + ".dcx");
                else if (File.Exists(commonTpfMPath))
                    TexturePool.AddTpfFromPath(commonTpfMPath);
            }

            foreach (var tpf in tpfsUsed)
            {
                TexturePool.AddTpf(tpf);
            }
            
            GFX.ModelDrawer.ForceTextureReloadImmediate();
            //TexturePool.DestroyUnusedTextures();
            GC.Collect();

            if (wpDummyBone != null)
            {
                List<FLVER2.Dummy> wpDummies = new List<FLVER2.Dummy>();

                var baseFlverDmyBone = TaeInterop.CurrentModel.Bones.Last(b => 
                b.Name == "Model_Dmy" || b.Name == "Model_Dmy_Weapon" || b.Name == "Dummy_Weapon_Base");

                var baseFlverDmyBoneIndex = TaeInterop.CurrentModel.Bones.IndexOf(baseFlverDmyBone);

                foreach (var dmy in partFlver.Dummies)
                {
                    dmy.AttachBoneIndex = (short)wpDummyBoneIndex;

                    if (t == PartsType.LeftWeapon)
                    {
                        dmy.ReferenceID = (short)(dmy.ReferenceID * -1);
                    }

                    dmy.Position = TransformVertVec3ByBone(wpDummyBone, dmy.Position, isNormals: false, backward: false);
                    dmy.Upward = TransformVertVec3ByBone(wpDummyBone, dmy.Upward, isNormals: true, backward: false);
                    dmy.Forward = TransformVertVec3ByBone(wpDummyBone, dmy.Forward, isNormals: true, backward: false);

                    dmy.DummyBoneIndex = (short)baseFlverDmyBoneIndex;

                    bool needToAdd = true;

                    foreach (var baseDmy in TaeInterop.CurrentModel.Dummies)
                    {
                        if (baseDmy.ReferenceID == dmy.ReferenceID)
                        {
                            baseDmy.AttachBoneIndex = dmy.AttachBoneIndex;
                            baseDmy.DummyBoneIndex = dmy.DummyBoneIndex;
                            baseDmy.Position = dmy.Position;
                            baseDmy.UseUpwardVector = dmy.UseUpwardVector;
                            baseDmy.Forward = dmy.Forward;
                            baseDmy.Upward = dmy.Upward;
                            baseDmy.Color = dmy.Color;

                            needToAdd = false;
                        }
                    }

                    if (needToAdd)
                    {
                        TaeInterop.CurrentModel.Dummies.Add(dmy);
                    }
                }


                DummyPolyManager.ClearAllHitboxPrimitives();
                DummyPolyManager.LoadDummiesFromFLVER(TaeInterop.CurrentModel);
                DummyPolyManager.BuildAllHitboxPrimitives();
                
            }

            TaeInterop.CreateMenuBarViewportSettings(Main.TAE_EDITOR.MenuBar);
        }

    }
}
