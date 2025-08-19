using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaternion = System.Numerics.Quaternion;

namespace DSAnimStudio
{
    public class NewSkeletonMapper
    {
        public static bool GlobalEnable = true;
        
        public readonly string GUID = Guid.NewGuid().ToString();
        public enum RemapModes
        {
            DirectBoneMap,
            DirectFKOrientationOnly,
            RetargetRelative,
            RetargetRelativeAndDirectFKOrientation,
        }
        
        public RemapModes Mode = RemapModes.DirectBoneMap;
        
        public bool Enabled = true;
        public bool NotImplemented_IsDebugDisp = false;
        
        public Model LeaderModel;
        public Model FollowerModel;
        
        public NewSkeletonMapper(Model leader, Model follower, RemapModes mode)
        {
            LeaderModel = leader;
            FollowerModel = follower;
            Mode = mode;
            
            var followerModelName_ForDebug = FollowerModel.Name;

            if (LeaderModel == null || FollowerModel == null)
                return;
        }
        
        private Matrix? GetLocalBoneMatrixForFollower_SpecificSkel(NewAnimSkeleton leaderSkel, NewAnimSkeleton followerSkel, string boneName)
        {
            //test
            // if (leaderSkel.OtherSkeletonThisIsMappedTo != null)
            //     leaderSkel = leaderSkel.OtherSkeletonThisIsMappedTo;
            
            var leaderBoneIndex = leaderSkel.GetBoneIndexByName(boneName);
            var followerBoneIndex = followerSkel.GetBoneIndexByName(boneName);

            if (leaderBoneIndex < 0 || followerBoneIndex < 0)
                return null;

            var leaderBone = leaderSkel.Bones[leaderBoneIndex];
            var followerBone = followerSkel.Bones[followerBoneIndex];

            
            //Test
            //return followerBone.ReferenceFKMatrix;
            
            
            
            
            var fk = followerBone.FKMatrix;
            
            //return fk;
            
            if (Mode == RemapModes.RetargetRelative)
            {
                string parentBoneName = null;
                if (leaderSkel.OtherSkeletonThisIsMappedTo != null)
                    parentBoneName = leaderSkel.OtherSkeletonThisIsMappedTo.GetParentOfBone(leaderBone.Name)?.Name;
                
                
                if (parentBoneName == null)
                    parentBoneName = leaderSkel.GetParentOfBone(leaderBone.Name)?.Name;
                
                
                if (parentBoneName == null)
                    parentBoneName = followerSkel.GetParentOfBone(leaderBone.Name)?.Name;
                
                var leaderParentBone = leaderSkel.GetBoneByName(parentBoneName);
                var followerParentBone = followerSkel.GetBoneByName(parentBoneName);
                
                Matrix parentFK = Matrix.Identity;
                Matrix parentRefFK = Matrix.Identity;
                
                var localMatrix = fk;
                var localRefMatrix = followerBone.ReferenceFKMatrix;
                if (followerParentBone != null)
                {
                    parentFK = followerParentBone.FKMatrix;
                    parentRefFK = followerParentBone.ReferenceFKMatrix;
                }
                
                //localMatrix *= Matrix.Invert(parentFK);
                localRefMatrix *= Matrix.Invert(parentRefFK);
                
                var leaderLocalMatrix = leaderBone.FKMatrix;
                var leaderLocalRefMatrix = leaderBone.ReferenceFKMatrix;
                
                if (leaderParentBone != null)
                {
                    leaderLocalMatrix *= Matrix.Invert(leaderParentBone.FKMatrix);
                    leaderLocalRefMatrix *= Matrix.Invert(leaderParentBone.ReferenceFKMatrix);
                }
                
                localMatrix = (leaderLocalMatrix * Matrix.Invert(leaderLocalRefMatrix)) * localRefMatrix;
                
                fk = localMatrix * parentFK;
                
                // if (Main.Debug.BreakOnBadBoneFKMatrixWrite && (fk.HasAnyInfinity() || fk.HasAnyNaN()))
                //     Console.Write("break");
            }
            else if (Mode == RemapModes.RetargetRelativeAndDirectFKOrientation)
            {
                string parentBoneName = null;
                if (leaderSkel.OtherSkeletonThisIsMappedTo != null)
                    parentBoneName = leaderSkel.OtherSkeletonThisIsMappedTo.GetParentOfBone(leaderBone.Name)?.Name;
                
                
                if (parentBoneName == null)
                    parentBoneName = leaderSkel.GetParentOfBone(leaderBone.Name)?.Name;
                
                
                if (parentBoneName == null)
                    parentBoneName = followerSkel.GetParentOfBone(leaderBone.Name)?.Name;
                
                var leaderParentBone = leaderSkel.GetBoneByName(parentBoneName);
                var followerParentBone = followerSkel.GetBoneByName(parentBoneName);
                
                Matrix parentFK = Matrix.Identity;
                Matrix parentRefFK = Matrix.Identity;
                
                var localMatrix = fk;
                var localRefMatrix = followerBone.ReferenceFKMatrix;
                if (followerParentBone != null)
                {
                    parentFK = followerParentBone.FKMatrix;
                    parentRefFK = followerParentBone.ReferenceFKMatrix;
                }
                
                //localMatrix *= Matrix.Invert(parentFK);
                localRefMatrix *= Matrix.Invert(parentRefFK);
                
                var leaderLocalMatrix = leaderBone.FKMatrix;
                var leaderLocalRefMatrix = leaderBone.ReferenceFKMatrix;
                
                if (leaderParentBone != null)
                {
                    leaderLocalMatrix *= Matrix.Invert(leaderParentBone.FKMatrix);
                    leaderLocalRefMatrix *= Matrix.Invert(leaderParentBone.ReferenceFKMatrix);
                }
                
                localMatrix = (leaderLocalMatrix * Matrix.Invert(leaderLocalRefMatrix)) * localRefMatrix;
                
                fk = localMatrix * parentFK;
                
                var fkTrans = fk.ToNewBlendableTransform();
                fkTrans.Rotation = leaderBone.FKMatrix.ToNewBlendableTransform().Rotation;
                fk = fkTrans.GetXnaMatrixFull();
                
                // if (Main.Debug.BreakOnBadBoneFKMatrixWrite && (fk.HasAnyInfinity() || fk.HasAnyNaN()))
                //     Console.Write("break");
            }
            else if (Mode == RemapModes.DirectBoneMap)
            {
                fk = leaderBone.FKMatrix;

                // if (Main.Debug.BreakOnBadBoneFKMatrixWrite && (fk.HasAnyInfinity() || fk.HasAnyNaN()))
                //     Console.Write("break");
            }
            else if (Mode == RemapModes.DirectFKOrientationOnly)
            {
                var fkTrans = fk.ToNewBlendableTransform();
                fkTrans.Rotation = leaderBone.FKMatrix.ToNewBlendableTransform().Rotation;
                fk = fkTrans.GetXnaMatrixFull();

                // if (Main.Debug.BreakOnBadBoneFKMatrixWrite && (fk.HasAnyInfinity() || fk.HasAnyNaN()))
                //     Console.Write("break");
            }
            else
            {
                throw new NotImplementedException();
            }

            if (followerBone.Debug_SkeletonMapperWeight != 1)
            {
                fk = Utils.MatrixMemeLerp(followerBone.FKMatrix, fk, followerBone.Debug_SkeletonMapperWeight);
            }
            
            return fk;
        }
        
        void WalkTree(NewAnimSkeleton skel, int i, Matrix currentMatrix, Matrix scaleMatrix, Matrix shiftThisBoneFK)
        {
            Matrix shiftNextChildFK = Matrix.Identity;
            
            
            
            var bone = skel.Bones[i];

            Matrix thisBoneFKBefore = bone.FKMatrix;
            
            var parentTransformation = currentMatrix;
            var parentScaleMatrix = scaleMatrix;

            bool wasOverriden = false;
            
            if (FollowerModel.EnableSkeletonMappers && Enabled && GlobalEnable)
            {
                var overrideFollowMatrix = GetLocalBoneMatrixForFollower_SpecificSkel(LeaderModel.SkeletonFlver, skel, bone.Name);
                if (overrideFollowMatrix.HasValue)
                {
                    bone.FKMatrix = shiftThisBoneFK * overrideFollowMatrix.Value;
                    wasOverriden = true;
                }
            }

            if (!wasOverriden)
            {

                var curTransform = skel.Bones[i].LocalTransform;

                int hkxBoneIndex = bone.MapToOtherBoneIndex;
                if (skel.OtherSkeletonThisIsMappedTo != null && hkxBoneIndex >= 0 && hkxBoneIndex < skel.OtherSkeletonThisIsMappedTo.Bones.Count)
                {
                    var otherBone = skel.OtherSkeletonThisIsMappedTo.Bones[hkxBoneIndex];
                    bone.FKMatrix = shiftThisBoneFK * otherBone.FKMatrix;
                    var otherBoneLocalMatrix = otherBone.FKMatrix;
                    if (otherBone.ParentIndex >= 0)
                    {
                        var otherBoneParent = skel.OtherSkeletonThisIsMappedTo.Bones[otherBone.ParentIndex];
                        otherBoneLocalMatrix *= Matrix.Invert(otherBoneParent.FKMatrix);
                    }

                    curTransform = otherBoneLocalMatrix.ToNewBlendableTransform();
                }

                var lerpedTransform = (NewBlendableTransform.Lerp(bone.ReferenceLocalTransform, curTransform, bone.Weight));
                currentMatrix = lerpedTransform.GetMatrix().ToXna();

                scaleMatrix = lerpedTransform.GetMatrixScale().ToXna();



                currentMatrix *= parentTransformation;
                scaleMatrix *= parentScaleMatrix;

                var finalMat = scaleMatrix * currentMatrix;

                bone.FKMatrix = shiftThisBoneFK * finalMat;



                // if (skel.OtherSkeletonThisIsMappedTo != null && hkxBoneIndex >= 0 && hkxBoneIndex < skel.OtherSkeletonThisIsMappedTo.Bones.Count)
                // {
                //     var otherBone = skel.OtherSkeletonThisIsMappedTo.Bones[hkxBoneIndex];
                //     bone.FKMatrix = shiftThisBoneFK * otherBone.FKMatrix;
                // }
                // else
                // {
                //     bone.FKMatrix = shiftThisBoneFK * finalMat;
                // }
            }



            var weight = bone.Weight * (FollowerModel.AnimContainer?.DebugAnimWeight ?? 1);
            if (weight != 1)
            {
                var parentFK = Matrix.Identity;
                var parentRefFK = Matrix.Identity;
                var fk = bone.FKMatrix;
                if (bone.ParentIndex >= 0)
                {
                    parentFK = skel.Bones[bone.ParentIndex].FKMatrix;
                    parentRefFK = skel.Bones[bone.ParentIndex].ReferenceFKMatrix;
                }

                var localMatrix = fk * Matrix.Invert(parentFK);

                var refLocalMatrix = bone.ReferenceFKMatrix * Matrix.Invert(parentRefFK);

                localMatrix = Utils.MatrixMemeLerp(refLocalMatrix, localMatrix, weight);

                var oldFKMatrix = bone.FKMatrix;
                bone.FKMatrix = localMatrix * parentFK;
                
                shiftNextChildFK *= fk * Matrix.Invert(oldFKMatrix);
            }
            
            currentMatrix = bone.FKMatrix;
            //
            // if (skel.OtherSkeletonThisIsMappedTo != null && bone.MapToOtherBoneIndex >= 0)
            // {
            //     
            // }
            
            //Test
            //shiftNextChildFK *= currentMatrix * Matrix.Invert(thisBoneFKBefore);
            
            if (skel is NewAnimSkeleton_FLVER asFlverSkeleton)
                asFlverSkeleton.CopyBoneToShaderMatrices(i);
            
            skel.OnBoneMatrixSet(i);

            foreach (var c in bone.ChildIndices)
                WalkTree(skel, c, currentMatrix, scaleMatrix, shiftNextChildFK);
        }
        
        private void NewUpdateForSkeleton(NewAnimSkeleton skel)
        {

            foreach (var root in skel.TopLevelBoneIndices)
                WalkTree(skel, root, Matrix.Identity, Matrix.Identity, Matrix.Identity);
        }
        
        public void Update()
        {
            try
            {
                if (FollowerModel.SkeletonFlver != null)
                {
                    // if (FollowerModel.SkeletonFlver.OtherSkeletonThisIsMappedTo != null)
                    // {
                    //     NewUpdateForSkeleton(FollowerModel.SkeletonFlver.OtherSkeletonThisIsMappedTo);
                    // }
                    
                    NewUpdateForSkeleton(FollowerModel.SkeletonFlver);
                }
                
                //FollowerModel.AnimContainer?.Scrub(false, 0, true, true, out _);
                //FollowerModel.SkeletonFlver?.CopyMatricesDirectlyFromOtherSkeleton(writeToShaderMatrices: true);
                
            }
            catch (Exception handled_ex) when (Main.EnableErrorHandler.ModelSkeletonRemapper)
            {
                Main.HandleError(nameof(Main.EnableErrorHandler.ModelSkeletonRemapper), handled_ex);
            }
            
        }
        
    }
}
