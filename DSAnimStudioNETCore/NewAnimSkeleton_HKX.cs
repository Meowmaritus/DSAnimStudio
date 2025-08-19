using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsAssetPipeline;

namespace DSAnimStudio
{
    public class NewAnimSkeleton_HKX : NewAnimSkeleton
    {
        protected override bool GetGlobalEnableDrawTransforms() => Main.HelperDraw.EnableHkxBoneTransforms;
        protected override bool GetGlobalEnableDrawLines() => false;
        protected override bool GetGlobalEnableDrawBoxes() => false;
        protected override bool GetGlobalEnableDrawText() => Main.HelperDraw.EnableHkxBoneNames;
        protected override Color GetDrawColorBoneBoxes() => Color.Fuchsia;
        protected override Color GetDrawColorBoneLines() => Color.Fuchsia;
        protected override Color GetDrawColorBoneTransforms() => Main.Colors.ColorHelperHkxBone;
        protected override Color GetDrawColorBoneText() => Main.Colors.ColorHelperHkxBone;
        
        public HKX.HKASkeleton OriginalHavokSkeleton = null;
        public HKX SkeletonPackfile = null;
        
        public void LoadHKXSkeleton(HKX skeletonPackfile)
        {
            SkeletonPackfile = skeletonPackfile;
            HKX.HKASkeleton skeleton = null;
            foreach (var o in skeletonPackfile.DataSection.Objects)
            {
                if (o is HKX.HKASkeleton asSkeleton)
                    skeleton = asSkeleton;
            }
            
            OriginalHavokSkeleton = skeleton;
            Bones.Clear();
            BoneIndices_ByName.Clear();
            TopLevelBoneIndices.Clear();
            UpperBodyIndices.Clear();
            for (int i = 0; i < skeleton.Bones.Size; i++)
            {
                var newHkxBone = new NewBone();
                newHkxBone.Index = i;
                newHkxBone.Name = skeleton.Bones[i].Name.GetString();

                if (newHkxBone.Name != null)
                    BoneIndices_ByName[newHkxBone.Name] = i;

                newHkxBone.ParentIndex = skeleton.ParentIndices[i].data;

                newHkxBone.ReferenceLocalTransform = new NewBlendableTransform()
                {
                    Translation = new System.Numerics.Vector3(
                        skeleton.Transforms[i].Position.Vector.X,
                        skeleton.Transforms[i].Position.Vector.Y,
                        skeleton.Transforms[i].Position.Vector.Z),
                    Rotation = new System.Numerics.Quaternion(
                        skeleton.Transforms[i].Rotation.Vector.X,
                        skeleton.Transforms[i].Rotation.Vector.Y,
                        skeleton.Transforms[i].Rotation.Vector.Z,
                        skeleton.Transforms[i].Rotation.Vector.W),
                    Scale = new System.Numerics.Vector3(
                        skeleton.Transforms[i].Scale.Vector.X,
                        skeleton.Transforms[i].Scale.Vector.Y,
                        skeleton.Transforms[i].Scale.Vector.Z),
                };

                newHkxBone.ReferenceLocalMatrix =
                    Matrix.CreateScale(new Vector3(
                        skeleton.Transforms[i].Scale.Vector.X,
                        skeleton.Transforms[i].Scale.Vector.Y,
                        skeleton.Transforms[i].Scale.Vector.Z))
                    * Matrix.CreateFromQuaternion(Quaternion.Normalize(new Quaternion(
                        skeleton.Transforms[i].Rotation.Vector.X,
                        skeleton.Transforms[i].Rotation.Vector.Y,
                        skeleton.Transforms[i].Rotation.Vector.Z,
                        skeleton.Transforms[i].Rotation.Vector.W)))
                    * Matrix.CreateTranslation(new Vector3(
                        skeleton.Transforms[i].Position.Vector.X,
                        skeleton.Transforms[i].Position.Vector.Y,
                        skeleton.Transforms[i].Position.Vector.Z));



                Bones.Add(newHkxBone);
            }
            
            InitBoneTree();
        }
        
        
        
        
        
    }
}
