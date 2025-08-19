using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.NewAnimSkeleton_HKX;

namespace DSAnimStudio
{
    public class NewAnimSkeleton_FLVER : NewAnimSkeleton
    {
        protected override bool GetGlobalEnableDrawTransforms() => Main.HelperDraw.EnableFlverBoneTransforms;
        protected override bool GetGlobalEnableDrawLines() => Main.HelperDraw.EnableFlverBoneLines;
        protected override bool GetGlobalEnableDrawBoxes() => Main.HelperDraw.EnableFlverBoneBoxes;
        protected override bool GetGlobalEnableDrawText() => Main.HelperDraw.EnableFlverBoneNames;
        protected override Color GetDrawColorBoneBoxes() => Main.Colors.ColorHelperFlverBoneBoundingBox;
        protected override Color GetDrawColorBoneLines() => Main.Colors.ColorHelperFlverBoneLines;
        protected override Color GetDrawColorBoneTransforms() => Main.Colors.ColorHelperFlverBoneTransforms;
        protected override Color GetDrawColorBoneText() => Main.Colors.ColorHelperFlverBoneTexts;

        
        

        public Matrix[] ShaderMatricesNew = new Matrix[GFXShaders.FlverShader.BoneMatrixSize];

        public Matrix[] ShaderMatricesNew_RefPose = new Matrix[0];

        public struct SubmeshBoneMatrixMapEntry
        {
            public int SubmeshIndex;
            public int MatrixIndex;
        }

        public class SubmeshBoneMatrixMapStruct
        {
            public List<SubmeshBoneMatrixMapEntry> Entries = new List<SubmeshBoneMatrixMapEntry>();
        }
        
        private SubmeshBoneMatrixMapStruct[] submeshBoneMatrixMap;
        

        public void RegistPerMeshBoneIndices(int submeshIndex, int[] boneIndices)
        {
            for (int i = 0; i < boneIndices.Length; i++)
            {
                var map = boneIndices[i] >= 0 ? submeshBoneMatrixMap[boneIndices[i]] : new();
                var existing = map.Entries.FindIndex(x => x.SubmeshIndex == submeshIndex);
                if (existing < 0)
                {
                    map.Entries.Add(new SubmeshBoneMatrixMapEntry() { SubmeshIndex = submeshIndex, MatrixIndex = i });
                }
            }
        }

        void InitShaderMatrices()
        {
            ShaderMatricesNew = new Matrix[Bones.Count];
            ShaderMatricesNew_RefPose = new Matrix[Bones.Count];
            submeshBoneMatrixMap = new SubmeshBoneMatrixMapStruct[Bones.Count];

            for (int i = 0; i < Bones.Count; i++)
            {
                ShaderMatricesNew[i] = Matrix.Identity;
                ShaderMatricesNew_RefPose[i] = Matrix.Identity;
                submeshBoneMatrixMap[i] = new SubmeshBoneMatrixMapStruct();
            }
        }
        
        static NewAnimSkeleton_FLVER()
        {
            
        }

        

        public bool EnableRefPoseMatrices = true;

        

        public Model MODEL;
        public void LoadFLVERSkeleton(Model mdl, List<FLVER.Node> flverBones)
        {
            MODEL = mdl;

            int[] childCounts = new int[flverBones.Count];

            Bones.Clear();
            BoneIndices_ByName.Clear();
            TopLevelBoneIndices.Clear();

            for (int i = 0; i < flverBones.Count; i++)
            {
                if (flverBones[i].ParentIndex < 0)
                    TopLevelBoneIndices.Add(i);
                var newBone = new NewBone(flverBones[i], flverBones);
                newBone.Index = i;
                if (flverBones[i].ParentIndex >= 0)
                    childCounts[flverBones[i].ParentIndex]++;

                if (newBone.Name != null)
                    BoneIndices_ByName[newBone.Name] = i;

                Bones.Add(newBone);
            }
            
            InitBoneTree();
            
            InitShaderMatrices();
        }
        
        public void SetDebugWeightViewBoneIndex(int i)
        {
            if (MODEL.USE_GLOBAL_BONE_MATRIX || i < 0)
            {
                MODEL.SetDebugBoneWeightViewOfAllSubmeshes(i);
            }
            else
            {
                if (i >= 0)
                    MODEL.SetDebugBoneWeightViewOfAllSubmeshes(-2); //-2 makes it still operate in bone weight view mode but with no weights highlighted.
                else
                    MODEL.SetDebugBoneWeightViewOfAllSubmeshes(-1);

                foreach (var mapEntry in submeshBoneMatrixMap[i].Entries)
                {
                    MODEL.SetDebugBoneWeightViewOfSubmesh(mapEntry.SubmeshIndex, mapEntry.MatrixIndex);
                }
            }
            DebugViewWeightOfBone_ImguiIndex = i;
        }

        public void CopyBoneToShaderMatrices(int i)
        {
            var bone = Bones[i];
            var inputMatrix = Matrix.Invert(bone.ReferenceFKMatrix) * bone.FKMatrix;

            if (bone.Weight != 1)
            {
                inputMatrix = NewBlendableTransform.Lerp(NewBlendableTransform.Identity, inputMatrix.ToNewBlendableTransform(), bone.Weight).GetXnaMatrixFull();
            }


            if (i < ShaderMatricesNew.Length)
            {
                if (MODEL.USE_GLOBAL_BONE_MATRIX)
                {
                    ShaderMatricesNew[i] = inputMatrix;
                    if (EnableRefPoseMatrices)
                        ShaderMatricesNew_RefPose[i] = Bones[i].ReferenceFKMatrix * inputMatrix;
                }
                else
                {
                    if (EnableRefPoseMatrices)
                    {
                        var refPoseMatrix = Bones[i].ReferenceFKMatrix * inputMatrix;
                        foreach (var mapEntry in submeshBoneMatrixMap[i].Entries)
                        {
                            MODEL.SetBoneMatrixOfSubmesh(mapEntry.SubmeshIndex, mapEntry.MatrixIndex, inputMatrix);
                            MODEL.SetBoneMatrixOfSubmesh_RefPose(mapEntry.SubmeshIndex, mapEntry.MatrixIndex, refPoseMatrix);
                        }
                    }
                    else
                    {
                        foreach (var mapEntry in submeshBoneMatrixMap[i].Entries)
                        {
                            MODEL.SetBoneMatrixOfSubmesh(mapEntry.SubmeshIndex, mapEntry.MatrixIndex, inputMatrix);
                        }
                    }
                }
             

                
            }

            MODEL.DummyPolyMan?.UpdateFlverBone(i, inputMatrix, bone.FKMatrix);
        }
        
        

        
        
    }
}
