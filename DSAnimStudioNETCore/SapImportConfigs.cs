using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class SapImportConfigs
    {
        public class ImportConfigFlver2
        {
            public string AssetPath { get; set; }

            public float SceneScale { get; set; } = 1.0f;
            public bool ConvertFromZUp { get; set; } = true;

            public bool KeepOriginalDummyPoly { get; set; } = true;
        }

        //public class ImportConfigFlver0
        //{
        //    public string AssetPath { get; set; }

        //    public float SceneScale { get; set; } = 1.0f;
        //    public bool ConvertFromZUp { get; set; } = true;

        //    public bool KeepOriginalDummyPoly { get; set; } = true;
        //}

        public class ImportConfigAnimFBX
        {
            public string AssetPath { get; set; }

            public float SceneScale { get; set; } = 1.0f;

            public float RootMotionScaleOverride { get; set; } = 1.0f;
            public bool UseRootMotionScaleOverride = false;

            public bool ConvertFromZUp { get; set; } = false;

            public string RootMotionNodeName { get; set; } = "root";
            public bool TurnQuaternionsInsideOut { get; set; } = false;
            public float SampleToFramerate { get; set; } = 60;
            public SplineCompressedAnimation.RotationQuantizationType RotationQuantizationType { get; set; } = SplineCompressedAnimation.RotationQuantizationType.THREECOMP40;
            public float RotationTolerance = 0.001f;

            public bool NegateQuaternionX { get; set; } = false;
            public bool NegateQuaternionY { get; set; } = false;
            public bool NegateQuaternionZ { get; set; } = false;
            public bool NegateQuaternionW { get; set; } = false;

            public List<string> BonesToFlipBackwardsForHotfix = new List<string>();

            public bool EnableRotationalRootMotion { get; set; } = true;
            public bool InitializeTransformTracksToTPose { get; set; } = true;
            public bool ExcludeRootMotionNodeFromTransformTracks { get; set; } = true;
        }
    }
}
