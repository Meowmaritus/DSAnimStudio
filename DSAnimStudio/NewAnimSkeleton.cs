﻿using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SFAnimExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFAnimExtensions.Havok;

namespace DSAnimStudio
{
    public class NewAnimSkeleton
    {
        public bool BoneLimitExceeded => FlverSkeleton.Count > MaxBoneCount;

        public const int MaxBoneCount = 
            // There is no point to writing this out like this
            // other than to remind me to update it if I add
            // another bone list
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*0*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*1*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*2*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*3*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*4*/ +
            GFXShaders.FlverShader.MaxBonePerMatrixArray/*5*/;

        public Matrix[] ShaderMatrices0 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices1 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices2 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices3 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices4 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];
        public Matrix[] ShaderMatrices5 = new Matrix[GFXShaders.FlverShader.MaxBonePerMatrixArray];

        public List<FlverBoneInfo> FlverSkeleton = new List<FlverBoneInfo>();
        public List<HkxBoneInfo> HkxSkeleton = new List<HkxBoneInfo>();

        public List<int> RootBoneIndices = new List<int>();

        public HKX.HKASkeleton OriginalHavokSkeleton = null;

        public readonly Model MODEL;

        public NewAnimSkeleton(Model mdl, List<FLVER.Bone> flverBones)
        {
            MODEL = mdl;

            int[] childCounts = new int[flverBones.Count];

            FlverSkeleton = new List<FlverBoneInfo>();

            for (int i = 0; i < flverBones.Count; i++)
            {
                var newBone = new FlverBoneInfo(flverBones[i], flverBones);
                if (flverBones[i].ParentIndex >= 0)
                    childCounts[flverBones[i].ParentIndex]++;
                FlverSkeleton.Add(newBone);
            }

            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                //FlverSkeleton[i].Length = Math.Max(0.1f, 
                //    (flverBones[i].BoundingBoxMax.Z - flverBones[i].BoundingBoxMin.Z) * 0.8f);

                //if (childCounts[i] == 1 && flverBones[i].ChildIndex >= 0)
                //{
                //    var parentChildDifference = Vector3.Transform(Vector3.Zero, 
                //        FlverSkeleton[flverBones[i].ChildIndex].ReferenceMatrix) -
                //        Vector3.Transform(Vector3.Zero, FlverSkeleton[i].ReferenceMatrix);

                //    var parentChildDirection = Vector3.Normalize(parentChildDifference);

                //    var parentDir = Vector3.TransformNormal(Vector3.Backward,
                //        Matrix.CreateRotationX(flverBones[i].Rotation.X) *
                //        Matrix.CreateRotationZ(flverBones[i].Rotation.Z) *
                //        Matrix.CreateRotationY(flverBones[i].Rotation.Y));

                //    var dot = Vector3.Dot(parentDir, parentChildDirection);

                //    FlverSkeleton[i].Length = parentChildDifference.Length() * (float)Math.Cos(dot);
                //}
                //else
                //{
                //     FlverSkeleton[i].Length = Math.Max(0.1f, 
                //    (flverBones[i].BoundingBoxMax.Z - flverBones[i].BoundingBoxMin.Z) * 0.8f);
                //}

                if (flverBones[i].ParentIndex >= 0 && flverBones[i].ParentIndex < flverBones.Count)
                {
                    FlverSkeleton[flverBones[i].ParentIndex].ChildBones.Add(FlverSkeleton[i]);
                }
            }

            for (int i = 0; i < GFXShaders.FlverShader.MaxBonePerMatrixArray; i++)
            {
                ShaderMatrices0[i] = Matrix.Identity;
                ShaderMatrices1[i] = Matrix.Identity;
                ShaderMatrices2[i] = Matrix.Identity;
                ShaderMatrices3[i] = Matrix.Identity;
                ShaderMatrices4[i] = Matrix.Identity;
                ShaderMatrices5[i] = Matrix.Identity;
            }
        }

        public void LoadHKXSkeleton(HKX.HKASkeleton skeleton)
        {
            OriginalHavokSkeleton = skeleton;
            HkxSkeleton.Clear();
            for (int i = 0; i < skeleton.Bones.Size; i++)
            {
                var newHkxBone = new HkxBoneInfo();
                newHkxBone.Name = skeleton.Bones[i].Name.GetString();
                newHkxBone.ParentIndex = skeleton.ParentIndices[i].data;
                newHkxBone.RelativeReferenceMatrix = 
                    Matrix.CreateScale(new Vector3(
                        skeleton.Transforms[i].Scale.Vector.X,
                        skeleton.Transforms[i].Scale.Vector.Y,
                        skeleton.Transforms[i].Scale.Vector.Z))
                    * Matrix.CreateFromQuaternion(new Quaternion(
                        skeleton.Transforms[i].Rotation.Vector.X,
                        skeleton.Transforms[i].Rotation.Vector.Y,
                        skeleton.Transforms[i].Rotation.Vector.Z,
                        skeleton.Transforms[i].Rotation.Vector.W))
                    * Matrix.CreateTranslation(new Vector3(
                        skeleton.Transforms[i].Position.Vector.X,
                        skeleton.Transforms[i].Position.Vector.Y,
                        skeleton.Transforms[i].Position.Vector.Z));

                for (int j = 0; j < FlverSkeleton.Count; j++)
                {
                    if (FlverSkeleton[j].Name == newHkxBone.Name)
                    {
                        FlverSkeleton[j].HkxBoneIndex = i;
                        newHkxBone.FlverBoneIndex = j;
                        break;
                    }
                }

                HkxSkeleton.Add(newHkxBone);
            }

            Matrix GetAbsoluteReferenceMatrix(int i)
            {
                Matrix result = Matrix.Identity;

                do
                {
                    result *= HkxSkeleton[i].RelativeReferenceMatrix;
                    i = HkxSkeleton[i].ParentIndex;
                }
                while (i >= 0);

                return result;
            }

            var flverDeadBonesToApplyHkxChildrenTo = new Dictionary<int, int>();

            for (int i = 0; i < HkxSkeleton.Count; i++)
            {
                HkxSkeleton[i].ReferenceMatrix = GetAbsoluteReferenceMatrix(i);
                for (int j = 0; j < HkxSkeleton.Count; j++)
                {
                    if (HkxSkeleton[j].ParentIndex == i)
                    {
                        HkxSkeleton[i].ChildIndices.Add(j);
                    }
                }
                if (HkxSkeleton[i].ParentIndex < 0)
                    RootBoneIndices.Add(i);

                if (HkxSkeleton[i].FlverBoneIndex == -1)
                {
                    HkxSkeleton[i].FlverBoneIndex = FlverSkeleton.Count;
                    var newFlverBone = new FlverBoneInfo(HkxSkeleton[i], HkxSkeleton, FlverSkeleton);
                    FlverSkeleton.Add(newFlverBone);
                    if (!flverDeadBonesToApplyHkxChildrenTo.ContainsKey(HkxSkeleton[i].FlverBoneIndex))
                    {
                        flverDeadBonesToApplyHkxChildrenTo.Add(HkxSkeleton[i].FlverBoneIndex, i);
                    }
                }
                else if (FlverSkeleton[HkxSkeleton[i].FlverBoneIndex].IsNub)
                {
                    FlverSkeleton[HkxSkeleton[i].FlverBoneIndex].ApplyHkxBoneProperties(HkxSkeleton[i], HkxSkeleton, FlverSkeleton);
                    if (!flverDeadBonesToApplyHkxChildrenTo.ContainsKey(HkxSkeleton[i].FlverBoneIndex))
                    {
                        flverDeadBonesToApplyHkxChildrenTo.Add(HkxSkeleton[i].FlverBoneIndex, i);
                    }
                }

                foreach (var kvp in flverDeadBonesToApplyHkxChildrenTo)
                {
                    FlverSkeleton[kvp.Key].ChildBones.Clear();
                    var copyFromHkx = HkxSkeleton[kvp.Value];
                    foreach (var ci in copyFromHkx.ChildIndices)
                    {
                        if (ci >= 0 && ci < HkxSkeleton.Count)
                        {
                            var matchingFlverChildBone = FlverSkeleton.FirstOrDefault(b => b.Name == HkxSkeleton[ci].Name);
                            if (matchingFlverChildBone != null && !FlverSkeleton[kvp.Key].ChildBones.Contains(matchingFlverChildBone))
                            {
                                FlverSkeleton[kvp.Key].ChildBones.Add(matchingFlverChildBone);
                            }
                        }
                    }

                    if (HkxSkeleton[kvp.Value].ParentIndex >= 0 && HkxSkeleton[kvp.Value].ParentIndex < HkxSkeleton.Count)
                    {
                        var matchingFlverParentBone = FlverSkeleton.FirstOrDefault(b => b.Name == HkxSkeleton[HkxSkeleton[kvp.Value].ParentIndex].Name);
                        if (matchingFlverParentBone != null && !matchingFlverParentBone.ChildBones.Contains(FlverSkeleton[kvp.Key]))
                        {
                            matchingFlverParentBone.ChildBones.Add(FlverSkeleton[kvp.Key]);
                        }
                    }

                    

                }
            }
        }

        public void ClearHkxBoneMatrices()
        {
            foreach (var h in HkxSkeleton)
            {
                if (h.FlverBoneIndex >= 0)
                {
                    this[h.FlverBoneIndex] = Matrix.Identity;
                }
            }
        }

        public void SetHkxBoneMatrix(int hkxBoneIndex, Matrix m)
        {
            int flverBoneIndex = HkxSkeleton[hkxBoneIndex].FlverBoneIndex;
            if (flverBoneIndex >= 0)
            {
                this[flverBoneIndex] = Matrix.Invert(FlverSkeleton[flverBoneIndex].ReferenceMatrix) * m;
            }
        }

        public void DrawPrimitives()
        {
            foreach (var f in FlverSkeleton)
                f.DrawPrim(MODEL.CurrentTransform.WorldMatrix);
        }

        public Matrix this[int boneIndex]
        {
            get
            {
                int bank = boneIndex / GFXShaders.FlverShader.MaxBonePerMatrixArray;
                int bone = boneIndex % GFXShaders.FlverShader.MaxBonePerMatrixArray;

                if (bank == 0)
                    return ShaderMatrices0[bone];
                else if (bank == 1)
                    return ShaderMatrices1[bone];
                else if (bank == 2)
                    return ShaderMatrices2[bone];
                else if (bank == 3)
                    return ShaderMatrices3[bone];
                else if (bank == 4)
                    return ShaderMatrices4[bone];
                else if (bank == 5)
                    return ShaderMatrices5[bone];
                else
                    return Matrix.Identity;
            }
            set
            {
                FlverSkeleton[boneIndex].CurrentMatrix = FlverSkeleton[boneIndex].ReferenceMatrix * value;

                int bank = boneIndex / GFXShaders.FlverShader.MaxBonePerMatrixArray;
                int bone = boneIndex % GFXShaders.FlverShader.MaxBonePerMatrixArray;

                if (bank == 0)
                    ShaderMatrices0[bone] = value;
                else if (bank == 1)
                    ShaderMatrices1[bone] = value;
                else if (bank == 2)
                    ShaderMatrices2[bone] = value;
                else if (bank == 3)
                    ShaderMatrices3[bone] = value;
                else if (bank == 4)
                    ShaderMatrices4[bone] = value;
                else if (bank == 5)
                    ShaderMatrices5[bone] = value;

                MODEL.DummyPolyMan.UpdateFlverBone(boneIndex, value);
            }
        }

        public void ApplyBakedFlverReferencePose()
        {
            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                this[i] = FlverSkeleton[i].ReferenceMatrix;
                FlverSkeleton[i].CurrentMatrix = FlverSkeleton[i].ReferenceMatrix;
            }
        }

        public void RevertToReferencePose()
        {
            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                this[i] = Matrix.Identity;
            }
        }

        public class FlverBoneInfo
        {
            public string Name;
            public Matrix ParentReferenceMatrix = Matrix.Identity;
            public Matrix ReferenceMatrix = Matrix.Identity;
            public int HkxBoneIndex = -1;
            public Matrix CurrentMatrix = Matrix.Identity;

            public Matrix? NubReferenceMatrix = null;
            //public Matrix? NubCurrentMatrix = null;

            public List<FlverBoneInfo> ChildBones = new List<FlverBoneInfo>();

            //public float Length = 1.0f;
            //public IDbgPrim BonePrim;
            public DbgPrimWireBox BoundingBoxPrim;

            static IDbgPrim GlobalBonePrim;

            public StatusPrinter SpawnPrinter = new StatusPrinter(null);

            public bool IsNub = false;

            public void ApplyHkxBoneProperties(HkxBoneInfo copyFromHkx, List<HkxBoneInfo> hkxBoneList, List<FlverBoneInfo> flverBoneList)
            {
                Name = copyFromHkx.Name;
                ParentReferenceMatrix = copyFromHkx.ReferenceMatrix * Matrix.Invert(copyFromHkx.RelativeReferenceMatrix);
                ReferenceMatrix = copyFromHkx.ReferenceMatrix;
                CurrentMatrix = ReferenceMatrix;
                HkxBoneIndex = hkxBoneList.IndexOf(copyFromHkx);

                IsNub = false;
                ChildBones.Clear();
                foreach (var ci in copyFromHkx.ChildIndices)
                {
                    if (ci >= 0 && ci < hkxBoneList.Count)
                    {
                        var matchingFlverChildBone = flverBoneList.FirstOrDefault(b => b.Name == hkxBoneList[ci].Name);
                        if (matchingFlverChildBone != null && !ChildBones.Contains(matchingFlverChildBone))
                        {
                            ChildBones.Add(matchingFlverChildBone);
                        }
                    }
                }
            }

            public FlverBoneInfo(HkxBoneInfo copyFromHkx, List<HkxBoneInfo> hkxBoneList, List<FlverBoneInfo> flverBoneList)
            {
                ApplyHkxBoneProperties(copyFromHkx, hkxBoneList, flverBoneList);

                BoundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                        Vector3.One * -0.01f,
                        Vector3.One * 0.01f,
                        DBG.COLOR_FLVER_BONE_BBOX)
                {
                    Category = DbgPrimCategory.FlverBoneBoundingBox,
                };

                SpawnPrinter.AppendLine(Name, DBG.COLOR_FLVER_BONE);
            }

            public FlverBoneInfo(FLVER.Bone bone, List<FLVER.Bone> boneList)
            {
                if (GlobalBonePrim == null)
                {
                    GlobalBonePrim = new DbgPrimWireBone("(BONE)", new Transform(Matrix.Identity), DBG.COLOR_FLVER_BONE)
                    {
                        Category = DbgPrimCategory.FlverBone,
                    };
                }

                Matrix GetBoneMatrix(SoulsFormats.FLVER.Bone b, bool saveParentBone)
                {
                    SoulsFormats.FLVER.Bone parentBone = b;

                    var result = Matrix.Identity;

                    bool isTopBone = true;

                    do
                    {
                        result *= Matrix.CreateScale(parentBone.Scale.X, parentBone.Scale.Y, parentBone.Scale.Z);
                        result *= Matrix.CreateRotationX(parentBone.Rotation.X);
                        result *= Matrix.CreateRotationZ(parentBone.Rotation.Z);
                        result *= Matrix.CreateRotationY(parentBone.Rotation.Y);
                        result *= Matrix.CreateTranslation(parentBone.Translation.X, parentBone.Translation.Y, parentBone.Translation.Z);

                        if (parentBone.ParentIndex >= 0)
                            parentBone = boneList[parentBone.ParentIndex];
                        else
                            parentBone = null;

                        isTopBone = false;

                        if (saveParentBone && isTopBone)
                            ParentReferenceMatrix = GetBoneMatrix(parentBone, saveParentBone: false);
                    }
                    while (parentBone != null);

                    return result;
                }

                ReferenceMatrix = GetBoneMatrix(bone, saveParentBone: true);
                Name = bone.Name;

                SpawnPrinter.AppendLine(Name, DBG.COLOR_FLVER_BONE);

                if (bone.Unk3C == 0)
                {
                    var nubBone = boneList.Where(bn => bn.Name == bone.Name + "Nub").FirstOrDefault();

                    if (nubBone != null)
                    {
                        var nubMat = Matrix.Identity;
                        nubMat *= Matrix.CreateScale(nubBone.Scale.X, nubBone.Scale.Y, nubBone.Scale.Z);
                        nubMat *= Matrix.CreateRotationX(nubBone.Rotation.X);
                        nubMat *= Matrix.CreateRotationZ(nubBone.Rotation.Z);
                        nubMat *= Matrix.CreateRotationY(nubBone.Rotation.Y);
                        nubMat *= Matrix.CreateTranslation(nubBone.Translation.X, nubBone.Translation.Y, nubBone.Translation.Z);

                        NubReferenceMatrix = nubMat;
                    }

                    
                }

                IsNub = bone.Unk3C != 0;

                BoundingBoxPrim = new DbgPrimWireBox(Transform.Default,
                        new Vector3(bone.BoundingBoxMin.X, bone.BoundingBoxMin.Y, bone.BoundingBoxMin.Z),
                        new Vector3(bone.BoundingBoxMax.X, bone.BoundingBoxMax.Y, bone.BoundingBoxMax.Z),
                        DBG.COLOR_FLVER_BONE_BBOX)
                {
                    Category = DbgPrimCategory.FlverBoneBoundingBox,
                };

            }

            public void DrawPrim(Matrix world)
            {
                if (BoundingBoxPrim != null)
                {
                    //BonePrim.Transform = new Transform(Matrix.CreateScale(Length) * CurrentMatrix);
                    //BonePrim.Draw(null, world);
                    BoundingBoxPrim.UpdateTransform(new Transform(CurrentMatrix));
                    BoundingBoxPrim.Draw(null, world);
                }

                Vector3 boneStart = Vector3.Transform(Vector3.Zero, CurrentMatrix);

                void DrawToEndPoint(Vector3 endPoint)
                {
                    var forward = -Vector3.Normalize(endPoint - boneStart);

                    Matrix hitboxMatrix = Matrix.CreateWorld(boneStart, forward, Vector3.Up);

                    if (forward.X == 0 && forward.Z == 0)
                    {
                        if (forward.Y >= 0)
                            hitboxMatrix = Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateTranslation(boneStart);
                        else
                            hitboxMatrix = Matrix.CreateRotationX(-MathHelper.PiOver2) * Matrix.CreateTranslation(boneStart);
                    }

                    float boneLength = (endPoint - boneStart).Length();

                    GlobalBonePrim.Transform = new Transform(Matrix.CreateRotationY(-MathHelper.PiOver2) * Matrix.CreateScale(boneLength) * hitboxMatrix);
                    GlobalBonePrim.Draw(null, world);

                    //using (var tempBone = new DbgPrimWireBone(Name, new Transform(Matrix.CreateRotationY(-MathHelper.PiOver2) * hitboxMatrix), DBG.COLOR_FLVER_BONE, boneLength, boneLength * 0.2f))
                    //{
                    //    tempBone.Draw(null, world);
                    //}
                }

                if (DBG.CategoryEnableDraw[DbgPrimCategory.FlverBone])
                {
                    if (NubReferenceMatrix != null)
                    {
                        DrawToEndPoint(Vector3.Transform(Vector3.Zero, NubReferenceMatrix.Value * CurrentMatrix));
                    }

                    foreach (var cb in ChildBones)
                    {
                        DrawToEndPoint(Vector3.Transform(Vector3.Zero, cb.CurrentMatrix));
                    }
                }

                if (DBG.CategoryEnableNameDraw[DbgPrimCategory.FlverBone])
                {
                    SpawnPrinter.Position3D = Vector3.Transform(Vector3.Zero, CurrentMatrix * world);
                    SpawnPrinter.Draw();
                }

               
            }
        }

        public class HkxBoneInfo
        {
            public string Name;
            public short ParentIndex = -1;
            public Matrix RelativeReferenceMatrix = Matrix.Identity;
            public Matrix ReferenceMatrix = Matrix.Identity;
            public int FlverBoneIndex = -1;
            public List<int> ChildIndices = new List<int>();
            public NewBlendableTransform CurrentHavokTransform = NewBlendableTransform.Identity;
        }
    }
}
