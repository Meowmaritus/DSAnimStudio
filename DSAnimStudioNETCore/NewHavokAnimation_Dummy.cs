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
    public class NewHavokAnimation_Dummy : NewHavokAnimation
    {
        public List<SplineCompressedAnimation.TransformTrack[]> Tracks => data_compressed.Tracks;

        // Index into array = hkx bone index, result = transform track index.
        private int[] HkxBoneIndexToTransformTrackMap => data_compressed.HkxBoneIndexToTransformTrackMap;

        private int[] TransformTrackIndexToHkxBoneMap => data_compressed.TransformTrackIndexToHkxBoneMap;

        private HavokAnimationData_SplineCompressed data_compressed => (HavokAnimationData_SplineCompressed)Data;

        public int BlockCount => data_compressed.BlockCount;
        public int NumFramesPerBlock => data_compressed.NumFramesPerBlock;

        int CurrentBlock => data_compressed.GetBlock(CurrentFrame);

        public NewHavokAnimation_Dummy()
            : base(new HavokAnimationData_Dummy(-1, "Dummy"), 0)
        {
        }
    }
}
