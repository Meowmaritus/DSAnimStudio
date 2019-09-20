using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewAnimSkeleton
    {
        public Matrix[] ShaderMatrix0 = new Matrix[GFXShaders.FlverShader.NUM_BONES];
        public Matrix[] ShaderMatrix1 = new Matrix[GFXShaders.FlverShader.NUM_BONES];
        public Matrix[] ShaderMatrix2 = new Matrix[GFXShaders.FlverShader.NUM_BONES];
        public Matrix[] ShaderMatrix3 = new Matrix[GFXShaders.FlverShader.NUM_BONES];

        public List<FlverBoneInfo> FlverSkeleton = new List<FlverBoneInfo>();
        public List<HkxBoneInfo> HkxSkeleton = new List<HkxBoneInfo>();

        public HKX.HKASkeleton OriginalHavokSkeleton = null;

        public readonly Model MODEL;

        public NewAnimSkeleton(Model mdl, List<FLVER2.Bone> flverBones)
        {
            MODEL = mdl;
            FlverSkeleton = flverBones.Select(b => new FlverBoneInfo(b, flverBones)).ToList();

            for (int i = 0; i < GFXShaders.FlverShader.NUM_BONES; i++)
            {
                ShaderMatrix0[i] = Matrix.Identity;
                ShaderMatrix1[i] = Matrix.Identity;
                ShaderMatrix2[i] = Matrix.Identity;
                ShaderMatrix3[i] = Matrix.Identity;
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
                        skeleton.Transforms[i].Position.Vector.X,
                        skeleton.Transforms[i].Position.Vector.Y,
                        skeleton.Transforms[i].Position.Vector.Z,
                        skeleton.Transforms[i].Position.Vector.W))
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

            for (int i = 0; i < HkxSkeleton.Count; i++)
            {
                HkxSkeleton[i].ReferenceMatrix = GetAbsoluteReferenceMatrix(i);
            }
        }

        public void SetHkxBoneMatrix(int hkxBoneIndex, Matrix m)
        {
            int flverBoneIndex = HkxSkeleton[hkxBoneIndex].FlverBoneIndex;
            if (flverBoneIndex >= 0 && HkxSkeleton[hkxBoneIndex].FlverBoneIndex < FlverSkeleton.Count)
            {
                this[flverBoneIndex] = Matrix.Invert(FlverSkeleton[flverBoneIndex].ReferenceMatrix) * m;
            }
        }

        public Matrix this[int boneIndex]
        {
            get
            {
                if (boneIndex >= GFXShaders.FlverShader.NUM_BONES * 3)
                    return ShaderMatrix3[boneIndex % GFXShaders.FlverShader.NUM_BONES];
                else if (boneIndex >= GFXShaders.FlverShader.NUM_BONES * 2)
                    return ShaderMatrix2[boneIndex % GFXShaders.FlverShader.NUM_BONES];
                if (boneIndex >= GFXShaders.FlverShader.NUM_BONES * 1)
                    return ShaderMatrix1[boneIndex % GFXShaders.FlverShader.NUM_BONES];
                else
                    return ShaderMatrix0[boneIndex];
            }
            set
            {
                if (boneIndex >= GFXShaders.FlverShader.NUM_BONES * 3)
                    ShaderMatrix3[boneIndex % GFXShaders.FlverShader.NUM_BONES] = value;
                else if (boneIndex >= GFXShaders.FlverShader.NUM_BONES * 2)
                    ShaderMatrix2[boneIndex % GFXShaders.FlverShader.NUM_BONES] = value;
                if (boneIndex >= GFXShaders.FlverShader.NUM_BONES * 1)
                    ShaderMatrix1[boneIndex % GFXShaders.FlverShader.NUM_BONES] = value;
                else
                    ShaderMatrix0[boneIndex] = value;

                if (MODEL.DummyPolyMan.AnimatedDummyPolyClusters.ContainsKey(boneIndex))
                {
                    MODEL.DummyPolyMan.AnimatedDummyPolyClusters[boneIndex].UpdateWithBoneMatrix(value);
                }
            }
        }

        public void ApplyBakedFlverReferencePose()
        {
            for (int i = 0; i < FlverSkeleton.Count; i++)
            {
                this[i] = FlverSkeleton[i].ReferenceMatrix;
            }
        }

        public class FlverBoneInfo
        {
            public string Name;
            public Matrix ReferenceMatrix;
            public int HkxBoneIndex = -1;

            public FlverBoneInfo(FLVER2.Bone bone, List<FLVER2.Bone> boneList)
            {
                Matrix GetBoneMatrix(SoulsFormats.FLVER2.Bone b)
                {
                    SoulsFormats.FLVER2.Bone parentBone = b;

                    var result = Matrix.Identity;

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
                    }
                    while (parentBone != null);

                    return result;
                }

                ReferenceMatrix = GetBoneMatrix(bone);
                Name = bone.Name;
            }
        }

        public class HkxBoneInfo
        {
            public string Name;
            public short ParentIndex;
            public Matrix RelativeReferenceMatrix;
            public Matrix ReferenceMatrix;
            public int FlverBoneIndex;
        }
    }
}
