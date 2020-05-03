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
    public class NewHavokAnimation_InterleavedUncompressed : NewHavokAnimation
    {
        public HavokAnimationData_InterleavedUncompressed data_interleaved => (HavokAnimationData_InterleavedUncompressed)data;

        public int TransformTrackCount => data_interleaved.TransformTrackCount;
        public List<NewBlendableTransform> Transforms => data_interleaved.Transforms;

        // Index into array = hkx bone index, result = transform track index.
        private int[] HkxBoneIndexToTransformTrackMap => data_interleaved.HkxBoneIndexToTransformTrackMap;

        private int[] TransformTrackIndexToHkxBoneMap => data_interleaved.TransformTrackIndexToHkxBoneMap;

        public NewHavokAnimation_InterleavedUncompressed(string name, NewAnimSkeleton skeleton, 
            HKX.HKADefaultAnimatedReferenceFrame refFrame, HKX.HKAAnimationBinding binding, HKX.HKAInterleavedUncompressedAnimation anim, NewAnimationContainer container)
            : base(new HavokAnimationData_InterleavedUncompressed(name, skeleton.OriginalHavokSkeleton, refFrame, binding, anim), skeleton, container)
        {
        }
    }
}
