using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.NewAnimSkeleton_HKX;

namespace DSAnimStudio
{
    public interface INewAnimSkeletonHelper
    {
        public int GetBoneIndexByName(string boneName);
        public int GetBoneParentIndex(int boneIndex);
        public NewBlendableTransform GetBoneTransform(int boneIndex);
        public Matrix GetBoneMatrix(int boneIndex);
        public void SetBoneTransform(int boneIndex, NewBlendableTransform transform);
        public void SetBoneMatrix(int boneIndex, Matrix matrix);

        // Bone name input functions
        public int GetBoneParentIndex(string boneName)
        {
            return GetBoneParentIndex(GetBoneIndexByName(boneName));
        }
        public NewBlendableTransform GetBoneTransform(string boneName)
        {
            return GetBoneTransform(GetBoneIndexByName(boneName));
        }
        public NewBlendableTransform GetBoneTransformFK(string boneName)
        {
            return GetBoneTransformFK(GetBoneIndexByName(boneName));
        }
        public void SetBoneTransform(string boneName, NewBlendableTransform transform)
        {
            SetBoneTransform(GetBoneIndexByName(boneName), transform);
        }
        public void SetBoneTransformFK(string boneName, NewBlendableTransform transformFK)
        {
            SetBoneTransformFK(GetBoneIndexByName(boneName), transformFK);
        }
        
        
        
        public Matrix GetBoneMatrix(string boneName)
        {
            return GetBoneMatrix(GetBoneIndexByName(boneName));
        }
        public Matrix GetBoneMatrixFK(string boneName)
        {
            return GetBoneMatrixFK(GetBoneIndexByName(boneName));
        }
        public void SetBoneMatrix(string boneName, Matrix matrix)
        {
            SetBoneMatrix(GetBoneIndexByName(boneName), matrix);
        }
        public void SetBoneMatrixFK(string boneName, Matrix matrixFK)
        {
            SetBoneMatrixFK(GetBoneIndexByName(boneName), matrixFK);
        }
        
        
        
        public void ModifyBoneTransformFK(int boneIndex, Func<NewBlendableTransform, NewBlendableTransform> modification)
        {
            if (boneIndex < 0)
                return;

            int parentIndex = GetBoneParentIndex(boneIndex);
            
            if (parentIndex >= 0)
            {
                NewBlendableTransform fk = GetBoneTransformFK(boneIndex);
                fk = modification(fk);

                SetBoneTransformFK(boneIndex, fk);
            }
            else
            {
                // Root bone, so FK and local transform are same
                SetBoneTransform(boneIndex, modification(GetBoneTransform(boneIndex)));
            }
            
        }
        public void ModifyBoneMatrixFK(int boneIndex, Func<Matrix, Matrix> modification)
        {
            if (boneIndex < 0)
                return;

            int parentIndex = GetBoneParentIndex(boneIndex);
            
            if (parentIndex >= 0)
            {
                Matrix fk = GetBoneMatrixFK(boneIndex);
                fk = modification(fk);

                SetBoneMatrixFK(boneIndex, fk);
            }
            else
            {
                // Root bone, so FK and local transform are same
                SetBoneMatrix(boneIndex, modification(GetBoneMatrix(boneIndex)));
            }
            
        }
        public void ModifyBoneTransformFK(string boneName, Func<NewBlendableTransform, NewBlendableTransform> modification)
        {
            ModifyBoneTransformFK(GetBoneIndexByName(boneName), modification);
        }
        public void ModifyBoneMatrixFK(string boneName, Func<Matrix, Matrix> modification)
        {
            ModifyBoneMatrixFK(GetBoneIndexByName(boneName), modification);
        }
        public NewBlendableTransform GetBoneTransformFK(int boneIndex)
        {
            return GetBoneMatrixFK(boneIndex).ToNewBlendableTransform();
        }
        public void SetBoneTransformFK(int boneIndex, NewBlendableTransform transformFK)
        {
            if (boneIndex < 0)
                return;

            int parentIndex = GetBoneParentIndex(boneIndex);
            if (parentIndex >= 0)
            {
                //Detailed step-by-step for debugging:

                //var inputMatrix = transformFK.GetXnaMatrix();
                //var inputMatrixScale = transformFK.GetXnaMatrixScale();
                //var parentFK = GetBoneTransformFK(parentIndex);
                //var parentMatrix = parentFK.GetXnaMatrix();
                //var parentMatrixScale = parentFK.GetXnaMatrixScale();
                //var inverseParentMatrix = Matrix.Invert(parentMatrix);
                //var inverseParentMatrixScale = Matrix.Invert(parentMatrixScale);
                //var matrix = inputMatrix * inverseParentMatrix;
                //var matrixScale = inputMatrixScale * inverseParentMatrixScale;
                //var finalMatrix = (matrixScale * matrix);
                //var finalTransform = finalMatrix.ToNewBlendableTransform();
                //SetBoneTransform(boneIndex, finalTransform, setOverrideFlag);

                var matrix = transformFK.GetXnaMatrix() * Matrix.Invert(GetBoneTransformFK(parentIndex).GetXnaMatrix());
                var matrixScale = transformFK.GetXnaMatrixScale() * Matrix.Invert(GetBoneTransformFK(parentIndex).GetXnaMatrixScale());
                SetBoneTransform(boneIndex, (matrixScale * matrix).ToNewBlendableTransform());
            }
            else
            {
                // Root bone, so FK is the same as current relative matrix.
                SetBoneTransform(boneIndex, transformFK);
            }
        }
        
        public Matrix GetBoneMatrixFK(int boneIndex)
        {
            if (boneIndex < 0)
                return Matrix.Identity;

            int parentBoneIndex = boneIndex;
            Matrix currentTransform = Matrix.Identity;
            Matrix currentTransformScale = Matrix.Identity;
            do
            {
                currentTransform *= GetBoneTransform(parentBoneIndex).GetXnaMatrix();
                currentTransformScale *= GetBoneTransform(parentBoneIndex).GetXnaMatrixScale();

                //if (currentTransform.HasAnyNaN() || currentTransform.HasAnyInfinity())
                //{
                //    Console.WriteLine("scree");
                //}

                parentBoneIndex = GetBoneParentIndex(parentBoneIndex);
            }
            while (parentBoneIndex >= 0);

            return (currentTransformScale * currentTransform);
        }
        public void SetBoneMatrixFK(int boneIndex, Matrix matrixFK)
        {
            if (boneIndex < 0)
                return;

            int parentIndex = GetBoneParentIndex(boneIndex);
            if (parentIndex >= 0)
            {
                SetBoneMatrix(boneIndex, matrixFK * Matrix.Invert(GetBoneMatrixFK(parentIndex)));
            }
            else
            {
                // Root bone, so FK is the same as current relative matrix.
                SetBoneMatrix(boneIndex, matrixFK);
            }
        }
        
        
    }
}
