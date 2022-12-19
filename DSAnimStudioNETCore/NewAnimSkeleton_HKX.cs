using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewAnimSkeleton_HKX
    {
        public Transform CurrentTransform = Transform.Default;

        public void RevertToReferencePose()
        {
            foreach (var h in HkxSkeleton)
            {
                //TODO: See if I need to reset the NewBlendableTransform thing here as well.
                h.CurrentMatrix = Matrix.Identity;
            }
        }

        static NewAnimSkeleton_HKX()
        {
            DebugDrawTransformOfFlverBonePrim =
            new DbgPrimWireArrow("", Transform.Default, Color.White)
            {
                Category = DbgPrimCategory.AlwaysDraw,
            };
            DebugDrawTransformOfFlverBoneTextDrawer = new StatusPrinter(null, Color.White);
        }

        public List<HkxBoneInfo> HkxSkeleton = new List<HkxBoneInfo>();



        public List<int> TopLevelHkxBoneIndices = new List<int>();

        public HKX.HKASkeleton OriginalHavokSkeleton = null;

        public static int DebugDrawTransformOfFlverBoneIndex = -1;
        private static DbgPrimWireArrow DebugDrawTransformOfFlverBonePrim;
        private static StatusPrinter DebugDrawTransformOfFlverBoneTextDrawer;

        public List<int> UpperBodyIndices = new List<int>();

        public List<short> GetParentIndices() => HkxSkeleton.Select(b => b.ParentIndex).ToList();

        public List<string> GetAncestorsBoneDescendsFrom(HkxBoneInfo bone, params string[] ancestorFilter)
        {
            var result = new List<string>();
            var parent = bone.ParentIndex;
            while (parent >= 0)
            {
                var ancestorCheck = HkxSkeleton[parent];
                if (ancestorFilter.Contains(ancestorCheck.Name) && !result.Contains(ancestorCheck.Name))
                    result.Add(ancestorCheck.Name);
                parent = ancestorCheck.ParentIndex;

                // If result is same length as filter, all ancestors were found
                if (result.Count == ancestorFilter.Length)
                    return result;
            }
            return result;
        }

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
            HkxSkeleton.Clear();
            TopLevelHkxBoneIndices.Clear();
            UpperBodyIndices.Clear();
            for (int i = 0; i < skeleton.Bones.Size; i++)
            {
                var newHkxBone = new HkxBoneInfo();
                newHkxBone.Name = skeleton.Bones[i].Name.GetString();
                newHkxBone.ParentIndex = skeleton.ParentIndices[i].data;

                newHkxBone.RelativeReferenceTransform = new NewBlendableTransform()
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

                newHkxBone.RelativeReferenceMatrix =
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



                HkxSkeleton.Add(newHkxBone);
            }

            void GetAbsoluteReferenceMatrix(int i)
            {
                Matrix result = Matrix.Identity;
                int j = i;

                do
                {
                    result = HkxSkeleton[j].RelativeReferenceMatrix * result;
                    j = HkxSkeleton[j].ParentIndex;
                }
                while (j >= 0);

                HkxSkeleton[i].ReferenceMatrix = result;
                HkxSkeleton[i].ReferenceMatrixRootBoneIndex = j;
            }

            for (int i = 0; i < HkxSkeleton.Count; i++)
            {
                if (HkxSkeleton[i].ParentIndex < 0)
                    TopLevelHkxBoneIndices.Add(i);

                HkxSkeleton[i].IsUpperBody = false;

                var ancestors = GetAncestorsBoneDescendsFrom(HkxSkeleton[i], "Upper_Root");

                if (ancestors.Contains("Upper_Root"))
                {
                    UpperBodyIndices.Add(i);
                    HkxSkeleton[i].IsUpperBody = true;
                }

                for (int j = 0; j < HkxSkeleton.Count; j++)
                {
                    if (HkxSkeleton[j].ParentIndex == i)
                    {
                        HkxSkeleton[i].ChildIndices.Add(j);
                    }
                }

                GetAbsoluteReferenceMatrix(i);
            }

        }

        public void DrawPrimitives()
        {
            for (int i = 0; i < HkxSkeleton.Count; i++)
            {
                DebugDrawTransformOfFlverBonePrim.Transform = new Transform(
                    Matrix.CreateRotationY(MathHelper.Pi) *
                    Matrix.CreateScale(0.1f, 0.1f, 0.1f) * HkxSkeleton[i].CurrentMatrix);

                DebugDrawTransformOfFlverBonePrim.OverrideColor = Color.Red;

                DebugDrawTransformOfFlverBonePrim.Draw(null, CurrentTransform.WorldMatrix);
            }
        }

        public class HkxBoneInfo
        {
            public string Name;
            public short ParentIndex = -1;
            public Matrix RelativeReferenceMatrix = Matrix.Identity;
            public NewBlendableTransform RelativeReferenceTransform = NewBlendableTransform.Identity;

            public int ReferenceMatrixRootBoneIndex = -1;
            public Matrix ReferenceMatrix = Matrix.Identity;

            public List<int> ChildIndices = new List<int>();
            public NewBlendableTransform CurrentHavokTransform = NewBlendableTransform.Identity;
            public Matrix CurrentMatrix = Matrix.Identity;

            public float Weight = 1;

            public bool IsUpperBody = false;
        }

    }
}
