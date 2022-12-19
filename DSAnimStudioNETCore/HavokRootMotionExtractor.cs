using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class HavokRootMotionExtractor
    {
        public NewAnimSkeleton_HKX TargetSkeleton;

        public NewBlendableTransform GetTargetFKTransform(int frame, int boneIndex)
        {
            if (boneIndex < 0)
                return NewBlendableTransform.Identity;
            var bone = TargetSkeleton.HkxSkeleton[boneIndex];
            var m = target_blendableTransformsForWholeAnim[frame][boneIndex].GetMatrix().ToXna();
            while (bone.ParentIndex >= 0)
            {
                int index = bone.ParentIndex;
                bone = TargetSkeleton.HkxSkeleton[index];
                m *= target_blendableTransformsForWholeAnim[frame][index].GetMatrix().ToXna();
            }
            return MatToTransform(m);
        }

        NewBlendableTransform MatToTransform(Matrix m)
        {
            if (m.Decompose(out Vector3 aS, out Quaternion aR, out Vector3 aT))
            {
                return new NewBlendableTransform(aT.ToCS(), aS.ToCS(), Quaternion.Normalize(aR).ToCS());
            }
            return NewBlendableTransform.Identity;
        }

        public void SetTargetFKTransform(int f, int boneIndex, NewBlendableTransform t)
        {
            void WalkTree_Target(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                if (i == boneIndex)
                {
                    target_blendableTransformsForWholeAnim[f][i] = MatToTransform((t.GetMatrix().ToXna() * Matrix.Invert(currentMatrix)));
                    return;
                }
                currentMatrix = target_blendableTransformsForWholeAnim[f][i].GetMatrix().ToXna() * currentMatrix;
                currentScale *= target_blendableTransformsForWholeAnim[f][i].Scale.ToXna();
                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_Target(c, currentMatrix, currentScale);
            }
            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_Target(root, Matrix.Identity, Vector3.One);
        }

        private List<NewBlendableTransform[]> target_blendableTransformsForWholeAnim = new List<NewBlendableTransform[]>();
        private List<NewBlendableTransform[]> target_absoluteMatricesForWholeAnim = new List<NewBlendableTransform[]>();
        private NewBlendableTransform[] target_blendableTransforms = new NewBlendableTransform[0];

        Dictionary<string, int> targetBoneIndicesByName = new Dictionary<string, int>();

        public HavokRootMotionExtractor(NewAnimSkeleton_HKX targetSkeleton)
        {
            TargetSkeleton = targetSkeleton;

            target_blendableTransformsForWholeAnim = new List<NewBlendableTransform[]>();
            target_absoluteMatricesForWholeAnim = new List<NewBlendableTransform[]>();
            target_blendableTransforms = new NewBlendableTransform[targetSkeleton.HkxSkeleton.Count];

            for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
            {
                target_blendableTransforms[i] = NewBlendableTransform.Identity;
                targetBoneIndicesByName.Add(TargetSkeleton.HkxSkeleton[i].Name, i);
            }
        }

        private void WriteIntFrameToAbsolute(NewHavokAnimation anim, int frame)
        {
            for (int i = 0; i < target_blendableTransforms.Length; i++)
            {
                target_blendableTransforms[i] = TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform;
            }

            while (target_blendableTransformsForWholeAnim.Count <= frame)
            {
                var newArr = new NewBlendableTransform[TargetSkeleton.HkxSkeleton.Count];
                for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
                {
                    newArr[i] = TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform;
                }
                target_blendableTransformsForWholeAnim.Add(newArr);
            }

            while (target_absoluteMatricesForWholeAnim.Count <= frame)
            {
                var newArr = new NewBlendableTransform[TargetSkeleton.HkxSkeleton.Count];
                for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
                {
                    newArr[i] = TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform;
                }
                target_absoluteMatricesForWholeAnim.Add(newArr);
            }

            anim.CurrentFrame = frame;

            void WalkTree_Input(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                target_blendableTransformsForWholeAnim[frame][i] = anim.GetBlendableTransformOnCurrentFrame(i);
                currentMatrix = target_blendableTransformsForWholeAnim[frame][i].GetMatrix().ToXna() * currentMatrix;
                currentScale *= target_blendableTransformsForWholeAnim[frame][i].Scale.ToXna();

                target_absoluteMatricesForWholeAnim[frame][i] = MatToTransform(currentMatrix);

                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_Input(c, currentMatrix, currentScale);
            }
            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_Input(root, Matrix.Identity, Vector3.One);
        }

        private void WriteFloatFrameToTransforms(float frame)
        {
            for (int i = 0; i < target_blendableTransforms.Length; i++)
            {
                target_blendableTransforms[i] = TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform;
            }

            void WalkTree_Output(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                float frameBlendRatio = frame % 1;
                int frameA = (int)Math.Floor(frame);
                int frameB = (int)Math.Ceiling(frame);

                var frameATransform = target_blendableTransformsForWholeAnim[frameA][i];
                var frameBTransform = target_blendableTransformsForWholeAnim[frameB][i];

                var finalTransform = NewBlendableTransform.Lerp(frameATransform, frameBTransform, frameBlendRatio);

                target_blendableTransforms[i] = finalTransform;
                currentMatrix = finalTransform.GetMatrix().ToXna() * currentMatrix;
                currentScale *= finalTransform.Scale.ToXna();

                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_Output(c, currentMatrix, currentScale);
            }
            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_Output(root, Matrix.Identity, Vector3.One);
        }

        public enum FootType
        {
            Left,
            Right,
            Both
        }

        public struct FootPlantedTimeframe
        {
            public FootType Foot;
            public int StartFrame;
            public int EndFrame;
            public FootPlantedTimeframe(FootType foot, int startFrame, int endFrame)
            {
                Foot = foot;
                StartFrame = startFrame;
                EndFrame = endFrame;
            }
        }

        //public struct FootPlantedStickingEnvelope
        //{
        //    public FootType Foot;
        //    public FootPlantedStickingEnvelope(FootType foot, )
        //    {
        //        Foot = foot;
        //        StartFrame = startFrame;
        //        EndFrame = endFrame;
        //    }
        //}

        public class RootMotionExtractorConfig
        {
            public string LeftFootBone = "L_Toe0";
            public string RightFootBone = "R_Toe0";
            public string LeftAnkleBone = "L_Foot";
            public string RightAnkleBone = "R_Foot";
            public string MasterBone = "Master";
            public bool VerticalMotionToMaster = true;
            public List<FootPlantedTimeframe> FootPlants = new List<FootPlantedTimeframe>();
        }

        public SoulsAssetPipeline.AnimationImporting.ImportedAnimation ProcessAnimation(NewHavokAnimation anim, RootMotionExtractorConfig cfg)
        {
            anim.Reset(System.Numerics.Vector4.Zero);
            anim.EnableLooping = false;

            var thing = new SoulsAssetPipeline.AnimationImporting.ImportedAnimation();
            thing.BlendHint = anim.BlendHint;

            thing.FrameCount = anim.FrameCount;

            //// If source animation is 30 FPS and not 60, then double frame count
            //if (anim.FrameDuration > 0.02f)
            //{
            //    //thing.Duration = (float)((thing.FrameCount) * (1.0 / 30.0));
            //    thing.FrameCount *= 2;
                
            //}
            //else
            //{
            //    //thing.Duration = (float)((thing.FrameCount) * (1.0 / 60.0));
            //}

            

            thing.Duration = anim.Duration;



            //if (anim.RootMotion.Data != null)
            //    anim.RootMotion.Data.Duration = thing.Duration;

            //thing.FrameCount--;
            thing.FrameDuration = anim.FrameDuration;


            //thing.Duration -= thing.FrameDuration;





            // MEMES

            //thing.FrameCount = 180;
            //thing.Duration = 3;
            //anim.data.FrameCount = 90;
            //anim.data.Duration = 3;
            //targetIdleAnim.CurrentTime = (float)((1.0 / 30.0) * 58.0);



            thing.HkxBoneIndexToTransformTrackMap = new int[TargetSkeleton.HkxSkeleton.Count];
            thing.TransformTrackIndexToHkxBoneMap = new int[TargetSkeleton.HkxSkeleton.Count];

            for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
            {
                thing.HkxBoneIndexToTransformTrackMap[i] = i;
                thing.TransformTrackIndexToHkxBoneMap[i] = i;
                thing.TransformTrackNames.Add(TargetSkeleton.HkxSkeleton[i].Name);
                thing.TransformTrackToBoneIndices.Add(TargetSkeleton.HkxSkeleton[i].Name, i);
            }

            for (int f = 0; f <= anim.FrameCount; f++)
            {
                WriteIntFrameToAbsolute(anim, f);
            }

            //for (int f = 1; f <= anim.FrameCount; f++)
            //{
            //    for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
            //    {
            //        var needsFix = fixEntries.Where(fe => fe.BoneName == TargetSkeleton.HkxSkeleton[i].Name).Where(fe => f >= fe.StartFrame && f <= fe.EndFrame).Any();

            //        if (needsFix)
            //        {
            //            var oldTransformAbsLocation = GetTargetFKTransform(f, i);
            //            target_blendableTransformsForWholeAnim[f][i] *= MatToTransform(Matrix.CreateRotationX(MathHelper.Pi));
            //            var newTransformAbsLocation = GetTargetFKTransform(f, i);
            //            var absDelta = newTransformAbsLocation * NewBlendableTransform.Invert(oldTransformAbsLocation);
            //            foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
            //                target_blendableTransformsForWholeAnim[f][c] *= NewBlendableTransform.Invert(absDelta);
            //        }
            //    }
            //}

            float currentTime = 0;

            while (currentTime < thing.Duration)
            {
                anim.CurrentTime = currentTime;

                WriteFloatFrameToTransforms(currentTime / anim.FrameDuration);




                var frame = new SoulsAssetPipeline.AnimationImporting.ImportedAnimation.Frame();
                frame.BoneTransforms = target_blendableTransforms.ToList();

                if (anim.RootMotion.Data != null)
                {
                    anim.RootMotion.SetTime(currentTime);
                    frame.RootMotionRotation = anim.RootMotion.CurrentTransform.W;
                    frame.RootMotionTranslation = anim.RootMotion.CurrentTransform.XYZ();
                }
                else
                {
                    if (thing.Frames.Count > 0)
                    {
                        frame.RootMotionTranslation = thing.Frames[thing.Frames.Count - 1].RootMotionTranslation;
                        frame.RootMotionRotation = thing.Frames[thing.Frames.Count - 1].RootMotionRotation;
                    }
                    else
                    {
                        frame.RootMotionTranslation = System.Numerics.Vector3.Zero;
                        frame.RootMotionRotation = 0;
                    }
                }
                thing.Frames.Add(frame);

                currentTime += thing.FrameDuration;
            }

            List<System.Numerics.Vector3> rootMotionDeltas = new List<System.Numerics.Vector3>();
            System.Numerics.Vector3[] masterDeltas = new System.Numerics.Vector3[thing.Frames.Count];
            rootMotionDeltas.Add(System.Numerics.Vector3.Zero);
            masterDeltas[0] = System.Numerics.Vector3.Zero;
            for (int f = 1; f < thing.Frames.Count; f++)
            {
                rootMotionDeltas.Add(thing.Frames[f].RootMotionTranslation - thing.Frames[f - 1].RootMotionTranslation);
                masterDeltas[f] = (thing.Frames[f].BoneTransforms[targetBoneIndicesByName[cfg.MasterBone]].Translation
                    - thing.Frames[f - 1].BoneTransforms[targetBoneIndicesByName[cfg.MasterBone]].Translation);
            }

            foreach (var footPlant in cfg.FootPlants)
            {
                bool memeAnkleShit = false;
                for (int f = Math.Max(footPlant.StartFrame, 1); f < Math.Min(footPlant.EndFrame, thing.Frames.Count); f++)
                {
                    System.Numerics.Vector3 footPos = System.Numerics.Vector3.Zero;
                    System.Numerics.Vector3 footPosLastFrame = System.Numerics.Vector3.Zero;
                    System.Numerics.Vector3 anklePos = System.Numerics.Vector3.Zero;
                    System.Numerics.Vector3 anklePosLastFrame = System.Numerics.Vector3.Zero;
                    if (footPlant.Foot == FootType.Right)
                    {
                        footPos = target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.RightFootBone]].Translation;
                        footPosLastFrame = target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.RightFootBone]].Translation;

                        anklePos = target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.RightAnkleBone]].Translation;
                        anklePosLastFrame = target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.RightAnkleBone]].Translation;
                    }
                    else if (footPlant.Foot == FootType.Left)
                    {
                        footPos = target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.LeftFootBone]].Translation;
                        footPosLastFrame = target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.LeftFootBone]].Translation;

                        anklePos = target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.LeftAnkleBone]].Translation;
                        anklePosLastFrame = target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.LeftAnkleBone]].Translation;
                    }
                    else if (footPlant.Foot == FootType.Both)
                    {
                        footPos = (target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.LeftFootBone]].Translation
                            + target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.RightFootBone]].Translation) / 2;
                        footPosLastFrame = (target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.LeftFootBone]].Translation
                            + target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.RightFootBone]].Translation) / 2;

                        anklePos = (target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.LeftAnkleBone]].Translation
                            + target_absoluteMatricesForWholeAnim[f][targetBoneIndicesByName[cfg.RightAnkleBone]].Translation) / 2;
                        anklePosLastFrame = (target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.LeftAnkleBone]].Translation
                            + target_absoluteMatricesForWholeAnim[f - 1][targetBoneIndicesByName[cfg.RightAnkleBone]].Translation) / 2;
                    }

                    //if (cfg.VerticalMotionToMaster)
                    //{
                    //    var masterTransform = thing.Frames[f].BoneTransforms[targetBoneIndicesByName[cfg.MasterBone]];
                    //    float masterDistAboveFoot = masterTransform.Translation.Y - footPos.Y;

                    //    // 0 is new desired foot height
                    //    masterTransform.Translation.Y = 0 + masterDistAboveFoot;
                    //    thing.Frames[f].BoneTransforms[targetBoneIndicesByName[cfg.MasterBone]] = masterTransform;
                    //}

                    if (memeAnkleShit || anklePos.Y < footPos.Y)
                    {
                        footPos.Y = 0;
                        footPosLastFrame.Y = 0;
                        memeAnkleShit = true;
                    }

                    var footPosDelta = footPos - footPosLastFrame;
                    footPosDelta = System.Numerics.Vector3.Transform(footPosDelta, System.Numerics.Matrix4x4.CreateRotationY(thing.Frames[f - 1].RootMotionRotation));

                    rootMotionDeltas[f] = -footPosDelta;
                }
            }

            thing.Frames[0].RootMotionTranslation = System.Numerics.Vector3.Zero;

            for (int f = 1; f < thing.Frames.Count; f++)
            {
                if (cfg.VerticalMotionToMaster)
                {
                    var verticalMotion = rootMotionDeltas[f].Y;
                    masterDeltas[f].Y += verticalMotion;
                    var horizontalMotion = rootMotionDeltas[f];
                    horizontalMotion.Y = 0;
                    thing.Frames[f].RootMotionTranslation = thing.Frames[f - 1].RootMotionTranslation + horizontalMotion;
                }
                else
                {
                    thing.Frames[f].RootMotionTranslation = thing.Frames[f - 1].RootMotionTranslation + rootMotionDeltas[f];
                }

                var masterTransform = thing.Frames[f].BoneTransforms[targetBoneIndicesByName[cfg.MasterBone]];
                masterTransform.Translation.Y = (thing.Frames[f - 1].BoneTransforms[targetBoneIndicesByName[cfg.MasterBone]].Translation.Y + masterDeltas[f].Y);
                thing.Frames[f].BoneTransforms[targetBoneIndicesByName[cfg.MasterBone]] = masterTransform;
            }

            thing.Duration = (thing.Frames.Count - 1) * thing.FrameDuration;
            thing.FrameCount = thing.Frames.Count;
            return thing;
        }


    }
}
