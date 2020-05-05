using Microsoft.Xna.Framework;
using SoulsFormats;
using SFAnimExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewHavokAnimation_InterleavedUncompressed : NewHavokAnimation
    {
        public int TransformTrackCount;
        public List<NewBlendableTransform> Transforms;

        public NewBlendableTransform GetTransformOnFrame(int frame, int trackIndex)
        {
            return Transforms[(TransformTrackCount * frame) + trackIndex];
        }

        // Index into array = hkx bone index, result = transform track index.
        private int[] HkxBoneIndexToTransformTrackMap;

        private int[] TransformTrackIndexToHkxBoneMap;

        public NewHavokAnimation_InterleavedUncompressed(NewAnimSkeleton skeleton, 
            HKX.HKADefaultAnimatedReferenceFrame refFrame, HKX.HKAAnimationBinding binding, HKX.HKAInterleavedUncompressedAnimation anim, NewAnimationContainer container)
            : base(skeleton, refFrame, binding, container)
        {
            Duration = anim.Duration;// Math.Max(anim.Duration, anim.FrameDuration * anim.FrameCount);
            TransformTrackCount = anim.TransformTrackCount;
            FrameCount = (int)anim.Transforms.Capacity / anim.TransformTrackCount;

            FrameDuration = Duration / FrameCount;

            HkxBoneIndexToTransformTrackMap = new int[skeleton.HkxSkeleton.Count];
            TransformTrackIndexToHkxBoneMap = new int[binding.TransformTrackToBoneIndices.Size];

            for (int i = 0; i < binding.TransformTrackToBoneIndices.Size; i++)
            {
                TransformTrackIndexToHkxBoneMap[i] = -1;
            }

            for (int i = 0; i < skeleton.HkxSkeleton.Count; i++)
            {
                HkxBoneIndexToTransformTrackMap[i] = -1;
            }

            for (int i = 0; i < binding.TransformTrackToBoneIndices.Size; i++)
            {
                short boneIndex = binding.TransformTrackToBoneIndices[i].data;
                if (boneIndex >= 0)
                    HkxBoneIndexToTransformTrackMap[boneIndex] = i;
                TransformTrackIndexToHkxBoneMap[i] = boneIndex;
            }

            Transforms = new List<NewBlendableTransform>((int)anim.Transforms.Capacity);

            foreach (var t in anim.Transforms.GetArrayData().Elements)
            {
                Transforms.Add(new NewBlendableTransform()
                {
                    Translation = new Vector3(t.Position.Vector.X, t.Position.Vector.Y, t.Position.Vector.Z),
                    Scale = new Vector3(t.Scale.Vector.X, t.Scale.Vector.Y, t.Scale.Vector.Z),
                    Rotation = new Quaternion(t.Rotation.Vector.X, t.Rotation.Vector.Y, t.Rotation.Vector.Z, t.Rotation.Vector.W)
                });
            }
        }

        public override NewBlendableTransform GetBlendableTransformOnCurrentFrame(int hkxBoneIndex)
        {
            var track = HkxBoneIndexToTransformTrackMap[hkxBoneIndex];

            if (track == -1)
            {
                var skeleTransform = Skeleton.OriginalHavokSkeleton.Transforms[hkxBoneIndex];

                NewBlendableTransform defaultBoneTransformation = new NewBlendableTransform();

                defaultBoneTransformation.Scale.X = skeleTransform.Scale.Vector.X;
                defaultBoneTransformation.Scale.Y = skeleTransform.Scale.Vector.Y;
                defaultBoneTransformation.Scale.Z = skeleTransform.Scale.Vector.Z;

                defaultBoneTransformation.Rotation = new Quaternion(
                    skeleTransform.Rotation.Vector.X,
                    skeleTransform.Rotation.Vector.Y,
                    skeleTransform.Rotation.Vector.Z,
                    skeleTransform.Rotation.Vector.W);

                defaultBoneTransformation.Translation.X = skeleTransform.Position.Vector.X;
                defaultBoneTransformation.Translation.Y = skeleTransform.Position.Vector.Y;
                defaultBoneTransformation.Translation.Z = skeleTransform.Position.Vector.Z;

                return defaultBoneTransformation;
            }

            float frame = (CurrentFrame % (FrameCount - 1));

            NewBlendableTransform currentFrame = GetTransformOnFrame((int)Math.Floor(frame), track);
            NewBlendableTransform nextFrame = GetTransformOnFrame((frame >= FrameCount - 2) ? 0 : (int)(Math.Ceiling(frame)), track);
            return NewBlendableTransform.Lerp(frame % 1, currentFrame, nextFrame);
        }
    }
}
