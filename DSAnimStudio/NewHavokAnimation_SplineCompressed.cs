using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SoulsFormats.Havok;

namespace DSAnimStudio
{
    public class NewHavokAnimation_SplineCompressed : NewHavokAnimation
    {
        public List<SplineCompressedAnimation.TransformTrack[]> Tracks => data_compressed.Tracks;

        // Index into array = hkx bone index, result = transform track index.
        private int[] HkxBoneIndexToTransformTrackMap => data_compressed.HkxBoneIndexToTransformTrackMap;

        private int[] TransformTrackIndexToHkxBoneMap => data_compressed.TransformTrackIndexToHkxBoneMap;

        private HavokAnimationData_SplineCompressed data_compressed => (HavokAnimationData_SplineCompressed)data;

        public int BlockCount => data_compressed.BlockCount;
        public int NumFramesPerBlock => data_compressed.NumFramesPerBlock;

        int CurrentBlock => data_compressed.GetBlock(CurrentFrame);

        public NewHavokAnimation_SplineCompressed(string name, NewAnimSkeleton skeleton, 
            HKX.HKADefaultAnimatedReferenceFrame refFrame, HKX.HKAAnimationBinding binding, HKX.HKASplineCompressedAnimation anim, NewAnimationContainer container)
            : base(new HavokAnimationData_SplineCompressed(name, skeleton.OriginalHavokSkeleton, refFrame, binding, anim), skeleton, container)
        {
        }
    }
}
