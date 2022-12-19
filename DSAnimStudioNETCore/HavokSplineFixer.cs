using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class HavokSplineFixer
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

        public HavokSplineFixer(NewAnimSkeleton_HKX targetSkeleton)
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

        public abstract class SplineFixEntry
        {
            public abstract void DoFix(HavokSplineFixer fixer);
            public abstract void FixFinalizedFrames(HavokSplineFixer fixer, SoulsAssetPipeline.AnimationImporting.ImportedAnimation thing);

            public class GenericSplineFixEntry : SplineFixEntry
            {
                public Action<HavokSplineFixer> DoFixAction;
                public Action<HavokSplineFixer, SoulsAssetPipeline.AnimationImporting.ImportedAnimation> FixFinalizedFramesAction;

                public GenericSplineFixEntry(Action<HavokSplineFixer> doFixAction, Action<HavokSplineFixer, SoulsAssetPipeline.AnimationImporting.ImportedAnimation> fixFinalizedFramesAction)
                {
                    DoFixAction = doFixAction;
                    FixFinalizedFramesAction = fixFinalizedFramesAction;
                }

                public override void DoFix(HavokSplineFixer fixer)
                {
                    DoFixAction?.Invoke(fixer);
                }

                public override void FixFinalizedFrames(HavokSplineFixer fixer, SoulsAssetPipeline.AnimationImporting.ImportedAnimation thing)
                {
                    FixFinalizedFramesAction?.Invoke(fixer, thing);
                }
            }

            public class NF_EnemyAttackAim : SplineFixEntry
            {
                public int InFrames;
                public int OutFrames;
                public int OutHoldFrames;
                public string AimBoneName;
                public float AimAngle;

                public NF_EnemyAttackAim(int inFrames, int outFrames, int outHoldFrames, string aimBoneName, float aimAngle)
                {
                    InFrames = inFrames;
                    OutFrames = outFrames;
                    OutHoldFrames = outHoldFrames;
                    AimBoneName = aimBoneName;
                    AimAngle = aimAngle;
                }

                public override void DoFix(HavokSplineFixer fixer)
                {
                    if (string.IsNullOrWhiteSpace(AimBoneName) || !fixer.targetBoneIndicesByName.ContainsKey(AimBoneName))
                    {
                        return;
                    }

                    int aimBone = fixer.targetBoneIndicesByName[AimBoneName];

                    for (int f = 0; f < fixer.target_absoluteMatricesForWholeAnim.Count; f++)
                    {
                        var curTransform = fixer.GetTargetFKTransform(f, aimBone);

                        var adjustedTransform = new NewBlendableTransform(System.Numerics.Matrix4x4.CreateRotationX(AimAngle)) * curTransform;

                        float lerp = 0;

                        if (f < InFrames)
                        {
                            lerp = ((float)f / (float)InFrames);
                            lerp *= lerp;
                        }
                        else if (f >= (fixer.target_absoluteMatricesForWholeAnim.Count - OutHoldFrames))
                        {
                            lerp = 0;
                        }
                        else if (f >= fixer.target_absoluteMatricesForWholeAnim.Count - OutHoldFrames - OutFrames)
                        {
                            lerp = (((float)f - ((float)fixer.target_absoluteMatricesForWholeAnim.Count
                                - (float)OutHoldFrames - (float)OutFrames)) / (float)OutFrames);
                            lerp *= lerp;
                            lerp = 1 - lerp;
                        }
                        else
                        {
                            lerp = 1;
                        }

                       


                        fixer.SetTargetFKTransform(f, aimBone, NewBlendableTransform.Lerp(curTransform, adjustedTransform, lerp));
                    }
                }

                public override void FixFinalizedFrames(HavokSplineFixer fixer, SoulsAssetPipeline.AnimationImporting.ImportedAnimation thing)
                {
                    //int aimBone = fixer.targetBoneIndicesByName[AimBoneName];

                    //for (int f = 0; f < thing.Frames.Count; f++)
                    //{
                    //    var adjustedTransform = thing.Frames[f].BoneTransforms[aimBone] * new NewBlendableTransform(System.Numerics.Matrix4x4.CreateRotationY(AimAngle));

                    //    float lerp = 0;

                    //    if (f < InFrames)
                    //        lerp = ((float)f / (float)InFrames);
                    //    else if (f >= (thing.Frames.Count - OutHoldFrames))
                    //        lerp = 0;
                    //    else if (f >= thing.Frames.Count - OutHoldFrames - OutFrames)
                    //        lerp = 1 - (((float)f - ((float)thing.Frames.Count - (float)OutHoldFrames - (float)OutFrames)) / (float)OutFrames);
                    //    else
                    //        lerp = 1;
                        

                    //    thing.Frames[f].BoneTransforms[aimBone] = NewBlendableTransform.Lerp(thing.Frames[f].BoneTransforms[aimBone], adjustedTransform, lerp);
                    //}
                }
            }


            public class NF_ArklusFix : SplineFixEntry
            {
                public NF_ArklusFix()
                    : base()
                {

                }

                public override void DoFix(HavokSplineFixer fixer)
                {
                    
                }

                public override void FixFinalizedFrames(HavokSplineFixer fixer, SoulsAssetPipeline.AnimationImporting.ImportedAnimation thing)
                {
                    //int frameFactor = 3;

                    //var resampledFrames = new List<SoulsAssetPipeline.AnimationImporting.ImportedAnimation.Frame>();

                    //for (int f = 0; f < thing.Frames.Count; f++)
                    //{
                    //    for (int i = 0; i < ((f < thing.Frames.Count - 1) ? frameFactor : 1); i++)
                    //    {
                    //        float sampleFrame = (float)f + ((float)i * (1f / (float)frameFactor));
                    //        var nextFrame = thing.GetSampledFrame(sampleFrame);
                    //        //nextFrame.RootMotionTranslation *= new System.Numerics.Vector3(frameFactor, 1, frameFactor);
                    //        resampledFrames.Add(nextFrame);
                    //    }
                    //}

                    //thing.Frames = resampledFrames;







                    //for (int f = 0; f < thing.Frames.Count; f++)
                    //{
                    //    thing.Frames[f].RootMotionTranslation *= new System.Numerics.Vector3(frameFactor, 1, frameFactor);
                    //}






                    //for (int f = 0; f < thing.Frames.Count; f++)
                    //{
                    //    for (int t = 0; t < thing.Frames[f].BoneTransforms.Count; t++)
                    //    {
                    //        var tr = thing.Frames[f].BoneTransforms[t];
                    //        tr.Translation *= 1.5f;
                    //        thing.Frames[f].BoneTransforms[t] = tr;
                    //    }
                    //}





                    int masterIndex = fixer.targetBoneIndicesByName["Master"];
                    int[] paralizeRoot = {
                        fixer.targetBoneIndicesByName["L Thigh"],
                        fixer.targetBoneIndicesByName["LThighTwist"],
                        fixer.targetBoneIndicesByName["R Thigh"],
                        fixer.targetBoneIndicesByName["RThighTwist"],
                    };

                    for (int f = 0; f < thing.Frames.Count; f++)
                    {
                        void Paralyze(int index)
                        {
                            thing.Frames[f].BoneTransforms[index] = fixer.TargetSkeleton.HkxSkeleton[index].RelativeReferenceTransform;
                            foreach (var c in fixer.TargetSkeleton.HkxSkeleton[index].ChildIndices)
                            {
                                Paralyze(c);
                            }
                        }
                        foreach (var r in paralizeRoot)
                        {
                            Paralyze(r);
                        }

                        var tr = thing.Frames[f].BoneTransforms[masterIndex];
                        tr.Translation.Y *= 0.5f;
                        thing.Frames[f].BoneTransforms[masterIndex] = tr;
                    }




                    List<System.Numerics.Vector3> rootMotionDeltas = new List<System.Numerics.Vector3>();
                    rootMotionDeltas.Add(System.Numerics.Vector3.Zero);
                    for (int f = 1; f < thing.Frames.Count; f++)
                    {
                        rootMotionDeltas.Add(thing.Frames[f].RootMotionTranslation - thing.Frames[f - 1].RootMotionTranslation);
                    }

                    List<System.Numerics.Vector3> newRootMotionVals = new List<System.Numerics.Vector3>();
                    System.Numerics.Vector3 currentDelta = System.Numerics.Vector3.Zero;
                    newRootMotionVals.Add(thing.Frames[0].RootMotionTranslation);
                    for (int f = 1; f < thing.Frames.Count; f++)
                    {
                        var nextDelta = rootMotionDeltas[f];
                        //if (Math.Abs(nextDelta.X) >= Math.Abs(currentDelta.X))
                        //    currentDelta.X = nextDelta.X;
                        //else
                        //    currentDelta.X = MathHelper.Lerp(currentDelta.X, nextDelta.X, 0.6f);

                        //if (Math.Abs(nextDelta.Z) >= Math.Abs(currentDelta.Z))
                        //    currentDelta.Z = nextDelta.Z;
                        //else
                        //    currentDelta.Z = MathHelper.Lerp(currentDelta.Z, nextDelta.Z, 0.6f);

                        if (Math.Abs(nextDelta.X) > Math.Abs(currentDelta.X))
                            currentDelta.X = MathHelper.Lerp(currentDelta.X, nextDelta.X, 0.65f);
                        else
                            currentDelta.X = MathHelper.Lerp(currentDelta.X, nextDelta.X, 0.1f);

                        if (Math.Abs(nextDelta.Z) > Math.Abs(currentDelta.Z))
                            currentDelta.Z = MathHelper.Lerp(currentDelta.Z, nextDelta.Z, 0.65f);
                        else
                            currentDelta.Z = MathHelper.Lerp(currentDelta.Z, nextDelta.Z, 0.1f);

                        newRootMotionVals.Add(newRootMotionVals[newRootMotionVals.Count - 1] + currentDelta);
                    }

                    for (int f = 0; f < thing.Frames.Count; f++)
                    {
                        thing.Frames[f].RootMotionTranslation = newRootMotionVals[f];
                    }
                }
            }

            public class GimbalFlip : SplineFixEntry
            {
                public List<string> BoneNames = new List<string>();
                public int StartFrame = -1;
                public int EndFrame = -1;

                public GimbalFlip(int startFrame, int endFrame, params string[] boneNames)
                    : base()
                {
                    StartFrame = startFrame;
                    EndFrame = endFrame;
                    BoneNames = boneNames.ToList();
                }

                public override void DoFix(HavokSplineFixer fixer)
                {
                    if (StartFrame >= 1)
                    {
                        foreach (var bn in BoneNames)
                        {
                            var i = fixer.targetBoneIndicesByName[bn];

                            var deltaTransform = fixer.target_blendableTransformsForWholeAnim[StartFrame - 1][i] *
                                NewBlendableTransform.Invert(fixer.target_blendableTransformsForWholeAnim[StartFrame][i]);

                            var deltaTwist = SoulsAssetPipeline.SapMath.FindQuaternionTwist(deltaTransform.Rotation, System.Numerics.Vector3.UnitX);

                            for (int f = StartFrame; f <= EndFrame; f++)
                            {
                                fixer.target_blendableTransformsForWholeAnim[f][i].Rotation = System.Numerics.Quaternion.CreateFromAxisAngle(System.Numerics.Vector3.UnitX, deltaTwist)
                                    * fixer.target_blendableTransformsForWholeAnim[f][i].Rotation;
                            }
                        }

                    }
                    else
                    {
                        throw new NotImplementedException(); //lol
                    }
                }

                public override void FixFinalizedFrames(HavokSplineFixer fixer, SoulsAssetPipeline.AnimationImporting.ImportedAnimation thing)
                {

                }
            }
        }

        public SoulsAssetPipeline.AnimationImporting.ImportedAnimation ProcessAnimation(NewHavokAnimation anim, List<SplineFixEntry> fixEntries)
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

            foreach (var fe in fixEntries)
            {
                fe.DoFix(this);
                
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

            foreach (var fe in fixEntries)
            {
                fe.FixFinalizedFrames(this, thing);

            }

            thing.Duration = (thing.Frames.Count - 1) * thing.FrameDuration;
            thing.FrameCount = thing.Frames.Count;
            return thing;
        }


    }
}
