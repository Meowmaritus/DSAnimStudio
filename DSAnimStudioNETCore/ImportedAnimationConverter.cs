using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.AnimationImporting;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class ImportedAnimationConverter
    {
        public class AnimReadyToPutIntoGame
        {
            public byte[] Data2010;
            public byte[] DataForGame;
        }

        public static byte[] HKXtoHKX2(HKX hkx)
        {
            HKX.HKAAnimationBinding hk_binding = null;
            HKX.HKASplineCompressedAnimation hk_anim = null;
            HKX.HKASkeleton hk_skeleton = null;
            HKX.HKADefaultAnimatedReferenceFrame hk_refFrame = null;

            foreach (var o in hkx.DataSection.Objects)
            {
                if (o is HKX.HKASkeleton asSkeleton)
                    hk_skeleton = asSkeleton;
                else if (o is HKX.HKAAnimationBinding asBinding)
                    hk_binding = asBinding;
                else if (o is HKX.HKASplineCompressedAnimation asAnim)
                    hk_anim = asAnim;
                else if (o is HKX.HKADefaultAnimatedReferenceFrame asRefFrame)
                    hk_refFrame = asRefFrame;
            }

            var root = new HKX2.hkRootLevelContainer();

            var animContainer = new HKX2.hkaAnimationContainer();

            animContainer.m_attachments = new List<HKX2.hkaBoneAttachment>();
            animContainer.m_skins = new List<HKX2.hkaMeshBinding>();
            animContainer.m_skeletons = new List<HKX2.hkaSkeleton>();
            animContainer.m_animations = new List<HKX2.hkaAnimation>();
            animContainer.m_bindings = new List<HKX2.hkaAnimationBinding>();

            if (hk_skeleton != null)
            {
                var skeleton = new HKX2.hkaSkeleton();
                skeleton.m_name = hk_skeleton.Name.GetString();

                skeleton.m_partitions = new List<HKX2.hkaSkeletonPartition>();
                skeleton.m_localFrames = new List<HKX2.hkaSkeletonLocalFrameOnBone>();
                skeleton.m_referenceFloats = new List<float>();
                skeleton.m_floatSlots = new List<string>();
                skeleton.m_referencePose = new List<System.Numerics.Matrix4x4>();

                skeleton.m_bones = new List<HKX2.hkaBone>();
                for (int i = 0; i < hk_skeleton.Bones.Size; i++)
                {
                    var b = hk_skeleton.Bones[i];
                    var newBone = new HKX2.hkaBone();
                    newBone.m_lockTranslation = b.LockTranslation != 0;
                    newBone.m_name = b.Name.GetString();
                    skeleton.m_bones.Add(newBone);
                }

                for (int i = 0; i < hk_skeleton.Transforms.Size; i++)
                {
                    var t = hk_skeleton.Transforms[i];
                    //TODO: Finish?
                }
            }

            HKX2.hkaSplineCompressedAnimation anim = null;


            if (hk_anim != null)
            {
                anim = new HKX2.hkaSplineCompressedAnimation();

                anim.m_blockDuration = hk_anim.BlockDuration;
                anim.m_blockInverseDuration = hk_anim.InverseBlockDuration;
                anim.m_data = hk_anim.Data.GetArrayData().Elements.Select(x => x.data).ToList();
                anim.m_duration = hk_anim.Duration;
                anim.m_endian = hk_anim.Endian;

                if (hk_refFrame != null)
                {
                    var rootMotion = new HKX2.hkaDefaultAnimatedReferenceFrame();

                    rootMotion.m_duration = hk_refFrame.Duration;
                    rootMotion.m_forward = hk_refFrame.Forward;
                    rootMotion.m_referenceFrameSamples = new List<System.Numerics.Vector4>();
                    foreach (var rf in hk_refFrame.ReferenceFrameSamples.GetArrayData().Elements)
                    {
                        rootMotion.m_referenceFrameSamples.Add(rf.Vector);
                    }
                    rootMotion.m_up = hk_refFrame.Up;

                    anim.m_extractedMotion = rootMotion;
                }

                anim.m_frameDuration = hk_anim.FrameDuration;
                anim.m_maskAndQuantizationSize = (int)hk_anim.MaskAndQuantization;
                anim.m_maxFramesPerBlock = hk_anim.FramesPerBlock;
                anim.m_numberOfFloatTracks = hk_anim.FloatTrackCount;
                anim.m_numberOfTransformTracks = hk_anim.TransformTrackCount;
                anim.m_numBlocks = hk_anim.BlockCount;
                anim.m_numFrames = hk_anim.FrameCount;
                anim.m_floatBlockOffsets = hk_anim.FloatBlockOffsets.GetArrayData().Elements.Select(b => b.data).ToList();
                anim.m_type = HKX2.AnimationType.HK_SPLINE_COMPRESSED_ANIMATION;
                anim.m_blockOffsets = hk_anim.BlockOffsets.GetArrayData().Elements.Select(b => b.data).ToList();
                anim.m_floatOffsets = new List<uint>();
                anim.m_transformOffsets = new List<uint>();

                animContainer.m_animations.Add(anim);
            }

            if (hk_binding != null)
            {
                var animBinding = new HKX2.hkaAnimationBinding();

                animBinding.m_animation = anim;
                animBinding.m_originalSkeletonName = hk_binding.OriginalSkeletonName;
                animBinding.m_transformTrackToBoneIndices = hk_binding.TransformTrackToBoneIndices.GetArrayData().Elements.Select(x => x.data).ToList();
                animBinding.m_floatTrackToFloatSlotIndices = new List<short>();
                animBinding.m_partitionIndices = new List<short>();
                animBinding.m_blendHint = (HKX2.BlendHint)(int)hk_binding.BlendHint;

                animContainer.m_bindings.Add(animBinding);
            }

            

            

            //TODO: IMPLEMENT ANNOTATION TRACK READ IN LEGACY HKX TO TRANSFER IT TO HKX2

            //anim.m_annotationTracks = new List<HKX2.hkaAnnotationTrack>();
            //for (int i = 0; i < hk_anim.TransformTrackCount; i++)
            //{
            //    var boneIndex = animBinding.m_transformTrackToBoneIndices[i];
            //    string boneName = boneNames[boneIndex];

            //    anim.m_annotationTracks.Add(new HKX2.hkaAnnotationTrack()
            //    {
            //        m_trackName = boneName,
            //        m_annotations = new List<HKX2.hkaAnnotationTrackAnnotation>(),
            //    });
            //}





            

            root.m_namedVariants = new List<HKX2.hkRootLevelContainerNamedVariant>();
            root.m_namedVariants.Add(new HKX2.hkRootLevelContainerNamedVariant()
            {
                m_className = "hkaAnimationContainer",
                m_name = "Merged Animation Container",
                m_variant = animContainer
            });

            byte[] resultFileBytes = null;

            using (MemoryStream s2 = new MemoryStream())
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, s2);
                var s = new HKX2.PackFileSerializer();
                s.Serialize(root, bw);

                resultFileBytes = s2.ToArray();
            }

            return resultFileBytes;
        }

        public static AnimReadyToPutIntoGame GetAnimReadyToPutIntoGameFromImported(ImportedAnimation importedAnim,
            SoulsAssetPipeline.SoulsGames? forceGame = null)
        {
            AnimReadyToPutIntoGame result = new AnimReadyToPutIntoGame();

            var compressed2010Hkx = importedAnim.WriteToSplineCompressedHKX2010Bytes(SoulsAssetPipeline.Animation.SplineCompressedAnimation.RotationQuantizationType.THREECOMP40, 0.001f, Main.Directory);

            result.Data2010 = compressed2010Hkx;

#if DEBUG
            File.WriteAllBytes("RECORDING_2010.HKX", compressed2010Hkx);
#endif

            byte[] finalHkxDataToImport = null;

            var selectedGame = forceGame ?? GameRoot.GameType;

            if (selectedGame == SoulsAssetPipeline.SoulsGames.DS1R)
            {
                int tries = 0;
                while (finalHkxDataToImport == null && tries < 2)
                {
                    finalHkxDataToImport = HavokDowngrade.UpgradeHkx2010to2015(compressed2010Hkx);
                    tries++;
                }
            }
            else if (selectedGame == SoulsAssetPipeline.SoulsGames.DS1)
            {
                finalHkxDataToImport = compressed2010Hkx;
            }
            //else if (selectedGame == SoulsAssetPipeline.SoulsGames.DS3)
            //{
            //    var hkx = HKX.Read(compressed2010Hkx);

            //    finalHkxDataToImport = HKXtoHKX2(hkx);
            //}

#if DEBUG
            File.WriteAllBytes("RECORDING.HKX", finalHkxDataToImport);
#endif

            result.DataForGame = finalHkxDataToImport;

            return result;
        }
    }
}
