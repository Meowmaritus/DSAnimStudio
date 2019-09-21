using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewHavokAnimation_SplineCompressed : NewHavokAnimation
    {
        public List<Havok.SplineCompressedAnimation.TransformTrack[]> Tracks;

        // Index into array = hkx bone index, result = transform track index.
        private int[] HkxBoneIndexToTransformTrackMap;

        private int[] TransformTrackIndexToHkxBoneMap;

        public int BlockCount = 1;
        public int NumFramesPerBlock = 256;

        int CurrentBlock => (int)((CurrentFrame % FrameCount) / NumFramesPerBlock);

        public NewHavokAnimation_SplineCompressed(NewAnimSkeleton skeleton, HKX.HKADefaultAnimatedReferenceFrame refFrame, HKX.HKAAnimationBinding binding, HKX.HKASplineCompressedAnimation anim)
            : base(skeleton, refFrame, binding)
        {
            Duration = anim.Duration;
            FrameCount = anim.FrameCount;

            FrameDuration = anim.FrameDuration;

            BlockCount = anim.BlockCount;
            NumFramesPerBlock = anim.FramesPerBlock;

            HkxBoneIndexToTransformTrackMap = new int[skeleton.HkxSkeleton.Count];
            TransformTrackIndexToHkxBoneMap = new int[binding.TransformTrackToBoneIndices.Size];

            for (int i = 0; i < binding.TransformTrackToBoneIndices.Size; i++)
            {
                short boneIndex = binding.TransformTrackToBoneIndices[i].data;
                if (boneIndex >= 0)
                {
                    HkxBoneIndexToTransformTrackMap[boneIndex] = i;
                    TransformTrackIndexToHkxBoneMap[i] = boneIndex;
                }
            }

            Tracks = Havok.SplineCompressedAnimation.ReadSplineCompressedAnimByteBlock(
                isBigEndian: false, anim.GetData(), anim.TransformTrackCount, anim.BlockCount);
        }

        private struct BlendableTransform
        {
            public Vector3 Translation;
            public Vector3 Scale;
            public Quaternion Rotation;

            public static BlendableTransform Lerp(float lerp, BlendableTransform a, BlendableTransform b)
            {
                float posX = MathHelper.Lerp(a.Translation.X, b.Translation.X, lerp);
                float posY = MathHelper.Lerp(a.Translation.Y, b.Translation.Y, lerp);
                float posZ = MathHelper.Lerp(a.Translation.Z, b.Translation.Z, lerp);

                float scaleX = MathHelper.Lerp(a.Scale.X, b.Scale.X, lerp);
                float scaleY = MathHelper.Lerp(a.Scale.Y, b.Scale.Y, lerp);
                float scaleZ = MathHelper.Lerp(a.Scale.Z, b.Scale.Z, lerp);

                float rotationX = MathHelper.Lerp(a.Rotation.X, b.Rotation.X, lerp);
                float rotationY = MathHelper.Lerp(a.Rotation.Y, b.Rotation.Y, lerp);
                float rotationZ = MathHelper.Lerp(a.Rotation.Z, b.Rotation.Z, lerp);
                float rotationW = MathHelper.Lerp(a.Rotation.W, b.Rotation.W, lerp);

                return new BlendableTransform()
                {
                    Translation = new Vector3(posX, posY, posZ),
                    Scale = new Vector3(scaleX, scaleY, scaleZ),
                    Rotation = new Quaternion(rotationX, rotationY, rotationZ, rotationW),
                };
            }

            public Matrix GetMatrix()
            {
                return Matrix.CreateScale(Scale) * 
                    Matrix.CreateFromQuaternion(Quaternion.Normalize(Rotation)) * 
                    Matrix.CreateTranslation(Translation);
            }
        }

        private BlendableTransform GetTransformOnSpecificBlockAndFrame(int transformIndex, int block, float frame)
        {
            frame = ((frame % FrameCount) + block) % NumFramesPerBlock;

            BlendableTransform result = new BlendableTransform();
            var track = Tracks[block][transformIndex];

            var skeleTransform = Skeleton.OriginalHavokSkeleton.Transforms[TransformTrackIndexToHkxBoneMap[transformIndex]];

            result.Scale.X = track.SplineScale?.ChannelX == null
                ? (IsAdditiveBlend ? 1 : track.StaticScale.X) : track.SplineScale.GetValueX(frame);
            result.Scale.Y = track.SplineScale?.ChannelY == null
                ? (IsAdditiveBlend ? 1 : track.StaticScale.Y) : track.SplineScale.GetValueY(frame);
            result.Scale.Z = track.SplineScale?.ChannelZ == null
                ? (IsAdditiveBlend ? 1 : track.StaticScale.Z) : track.SplineScale.GetValueZ(frame);

            if (!IsAdditiveBlend && (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) &&
                !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX)))
            {
                result.Scale.X = skeleTransform.Scale.Vector.X;
            }

            if (!IsAdditiveBlend && (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) &&
                !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY)))
            {
                result.Scale.Y = skeleTransform.Scale.Vector.Y;
            }

            if (!IsAdditiveBlend && (!track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) &&
                !track.Mask.ScaleTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ)))
            {
                result.Scale.Z = skeleTransform.Scale.Vector.Z;
            }

            if (IsAdditiveBlend)
            {
                result.Scale.X *= skeleTransform.Scale.Vector.X;
                result.Scale.Y *= skeleTransform.Scale.Vector.Y;
                result.Scale.Z *= skeleTransform.Scale.Vector.Z;
            }

            if (track.HasSplineRotation)
            {
                result.Rotation = track.SplineRotation.GetValue(frame);
            }
            else if (track.HasStaticRotation)
            {
                // We actually need static rotation or Gael hands become unbent among others
                result.Rotation = track.StaticRotation;
            }
            else
            {
                result.Rotation = IsAdditiveBlend ? Quaternion.Identity : new Quaternion(
                    skeleTransform.Rotation.Vector.X,
                    skeleTransform.Rotation.Vector.Y,
                    skeleTransform.Rotation.Vector.Z,
                    skeleTransform.Rotation.Vector.W);
            }

            if (IsAdditiveBlend)
            {
                result.Rotation *= new Quaternion(
                    skeleTransform.Rotation.Vector.X,
                    skeleTransform.Rotation.Vector.Y,
                    skeleTransform.Rotation.Vector.Z,
                    skeleTransform.Rotation.Vector.W);
            }

            // We use skeleton transform here instead of static position, which fixes Midir, Ariandel, and other enemies
            // at the cost of Ringed Knight's weapons being misplaced from their hands. This way more shit works 
            // right but I wish I knew WHY and I wish ALL worked.
            //var posX = track.SplinePosition?.ChannelX == null
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.X) : track.SplinePosition.GetValueX(frame);
            //var posY = track.SplinePosition?.ChannelY == null
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Y) : track.SplinePosition.GetValueY(frame);
            //var posZ = track.SplinePosition?.ChannelZ == null
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Z) : track.SplinePosition.GetValueZ(frame);

            result.Translation.X = track.SplinePosition?.ChannelX == null
                ? (IsAdditiveBlend ? 0 : track.StaticPosition.X) : track.SplinePosition.GetValueX(frame);
            result.Translation.Y = track.SplinePosition?.ChannelY == null
                ? (IsAdditiveBlend ? 0 : track.StaticPosition.Y) : track.SplinePosition.GetValueY(frame);
            result.Translation.Z = track.SplinePosition?.ChannelZ == null
                ? (IsAdditiveBlend ? 0 : track.StaticPosition.Z) : track.SplinePosition.GetValueZ(frame);


            //var posX = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX)
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.X + track.StaticPosition.X) : track.SplinePosition.GetValueX(frame);
            //var posY = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY)
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Y + track.StaticPosition.Y) : track.SplinePosition.GetValueY(frame);
            //var posZ = !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ)
            //    ? (additiveBlend ? 0 : skeleTransform.Position.Vector.Z + track.StaticPosition.Z) : track.SplinePosition.GetValueZ(frame);


            //var nullPos = false;// (posX == 0 && posY == 0 && posZ == 0);

            if (!IsAdditiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineX) &&
                !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticX)))
            {
                result.Translation.X = skeleTransform.Position.Vector.X;
            }

            if (!IsAdditiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineY) &&
                !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticY)))
            {
                result.Translation.Y = skeleTransform.Position.Vector.Y;
            }

            if (!IsAdditiveBlend && (!track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.SplineZ) &&
                !track.Mask.PositionTypes.Contains(Havok.SplineCompressedAnimation.FlagOffset.StaticZ)))
            {
                result.Translation.Z = skeleTransform.Position.Vector.Z;
            }

            if (IsAdditiveBlend)
            {
                result.Translation.X += skeleTransform.Position.Vector.X;
                result.Translation.Y += skeleTransform.Position.Vector.Y;
                result.Translation.Z += skeleTransform.Position.Vector.Z;
            }

            return result;
        }

        public override Matrix GetBoneMatrixOnCurrentFrame(int hkxBoneIndex)
        {
            var track = HkxBoneIndexToTransformTrackMap[hkxBoneIndex];

            // Blend between blocks
            if (CurrentFrame >= NumFramesPerBlock - 1 && CurrentBlock < Tracks.Count - 1)
            {
                BlendableTransform currentFrame = GetTransformOnSpecificBlockAndFrame(track, 
                    block: CurrentBlock, frame: (float)Math.Floor(CurrentFrame));
                BlendableTransform nextFrame = GetTransformOnSpecificBlockAndFrame(track, block: CurrentBlock + 1, frame: 1);
                currentFrame = BlendableTransform.Lerp(CurrentFrame % 1, currentFrame, nextFrame);
                return currentFrame.GetMatrix();
            }
            // Blend between loops
            else if (CurrentFrame >= FrameCount - 1)
            {
                BlendableTransform currentFrame = GetTransformOnSpecificBlockAndFrame(track, 
                    block: CurrentBlock, frame: (float)Math.Floor(CurrentFrame));
                BlendableTransform nextFrame = GetTransformOnSpecificBlockAndFrame(track, block: 0, frame: 0);
                currentFrame = BlendableTransform.Lerp(CurrentFrame % 1, currentFrame, nextFrame);
                return currentFrame.GetMatrix();
            }
            // Regular frame
            else
            {
                BlendableTransform currentFrame = GetTransformOnSpecificBlockAndFrame(track, 
                    block: CurrentBlock, frame: CurrentFrame);
                return currentFrame.GetMatrix();
            }
        }
    }
}
