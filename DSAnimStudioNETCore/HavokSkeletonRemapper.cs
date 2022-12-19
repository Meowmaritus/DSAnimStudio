using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class HavokSkeletonRemapperExtensions
    {
        public static NewBlendableTransform GetTScaled(this NewBlendableTransform t, float scale)
        {
            if (scale > 0)
                t.Translation *= scale;
            return t;
        }
    }
    public class HavokSkeletonRemapper
    {
        public struct IdleCorrectEnvelope
        {
            public float InStartFrame;
            public float InEndFrame;
            public float OutStartFrame;
            public float OutEndFrame;
            public float OutVal;
            public float InVal;

            public IdleCorrectEnvelope(float inStartFrame, float inEndFrame, float outStartFrame, float outEndFrame, (float Out, float In) val)
            {
                InStartFrame = inStartFrame;
                InEndFrame = inEndFrame;
                OutStartFrame = outStartFrame;
                OutEndFrame = outEndFrame;
                OutVal = val.Out;
                InVal = val.In;
            }

            public float GetIdleBlendSine(float frame, float frameCount)
            {
                return 1 - (float)(Math.Cos(GetIdleBlend(frame, frameCount) * Math.PI) / 2.0 + 0.5);
            }

            public float GetIdleBlend(float frame, float frameCount)
            {
                float inEnd = InEndFrame;
                float outEnd = OutEndFrame;
                float outStart = OutStartFrame;

                if (inEnd < 0)
                    inEnd = frameCount;

                if (outEnd < 0)
                    outEnd = frameCount;

                if (outStart < 0)
                    outStart = frameCount + outStart;

                float ratio = 0;

                if (frame < InStartFrame)
                    ratio = 0;
                else if (frame >= InEndFrame && frame < outStart)
                    ratio = 1;
                else if (frame >= outEnd)
                    ratio = 0;
                else if (InStartFrame >= 0 && inEnd >= 0 && frame >= InStartFrame && frame <= inEnd)
                {
                    float span = (inEnd - InStartFrame);
                    if (span != 0)
                        ratio = MathHelper.Clamp((frame - InStartFrame) / span, 0, 1);
                    else
                        ratio = 1;
                }
                else if (outStart >= 0 && outEnd >= 0 && frame >= outStart && frame <= outEnd)
                {
                    float span = (outEnd - outStart);
                    if (span != 0)
                        ratio = 1 - MathHelper.Clamp((frame - outStart) / span, 0, 1);
                    else
                        ratio = 0;
                }

                return MathHelper.Lerp(OutVal, InVal, ratio);
            }
        }

        public class IdleCorrectBlendConfig
        {
            public IdleCorrectEnvelope RightLegCorrect;
            public IdleCorrectEnvelope LeftLegCorrect;
            public IdleCorrectEnvelope RightArmCorrect;
            public IdleCorrectEnvelope LeftArmCorrect;
            public IdleCorrectEnvelope CoreCorrect;


            public float EndFullIdleBlendEnvelopeFrames;
            public float SourceSkeletonScale;
            public bool RaiseMasterUpToPelvis;

            public Dictionary<string, Matrix> SourceBoneAbsCorrectMatrices;

            public ArmIKConfig ArmIK = new ArmIKConfig();

            public bool MirrorAnimation = false;
        }

        public class ArmIKConfig
        {
            public bool L_Enabled;
            public IdleCorrectEnvelope L_Envelope;

            public string L_UpperArm = "L_UpperArm";
            public string L_Forearm = "L_Forearm";
            public string L_Hand = "L_Hand";
            public string L_Elbow = "L_Elbow";
            public string L_UpperArmTwist = "LUpArmTwist";
            public string L_ForearmTwist = "L_ForeTwist";
            public bool L_Elbow_IsSource = true;
            public float L_OffsetScale = 1;
        }

        public class LegacyIKSolver
        {
            public NewAnimSkeleton_HKX Skeleton;
            public List<int> BoneIndicesInChain;

            private NewBlendableTransform[] baseTransforms;
            private Quaternion[] testTransformDirections;
            private float[] testTransformMults;

            public float LearningRate = 0.1f;
            public float SamplingDistance = 0.1f;

            public LegacyIKSolver(NewAnimSkeleton_HKX skeleton, List<int> boneChain)
            {
                Skeleton = skeleton;
                BoneIndicesInChain = boneChain;

                baseTransforms = new NewBlendableTransform[skeleton.HkxSkeleton.Count];
                testTransformMults = new float[skeleton.HkxSkeleton.Count];
                testTransformDirections = new Quaternion[skeleton.HkxSkeleton.Count];

                for (int i = 0; i < Skeleton.HkxSkeleton.Count; i++)
                {
                    testTransformMults[i] = 1;
                }
            }

            private NewBlendableTransform GetTestTransform(int boneIndex)
            {
                var tr = baseTransforms[boneIndex];
                //tr.Rotation = tr.Rotation * Quaternion.Slerp(Quaternion.Identity, testTransformDirections[boneIndex], testTransformMults[boneIndex]).ToCS();
                tr.Rotation = tr.Rotation * Quaternion.CreateFromAxisAngle(Vector3.Up, testTransformMults[boneIndex]).ToCS();
                return tr;
            }

            private Matrix GetTestFKOfBone(int boneIndex)
            {
                var bone = Skeleton.HkxSkeleton[boneIndex];
                Matrix m = GetTestTransform(boneIndex).GetMatrix().ToXna();
                while (bone.ParentIndex >= 0)
                {
                    int index = bone.ParentIndex;
                    bone = Skeleton.HkxSkeleton[index];
                    m *= GetTestTransform(index).GetMatrix().ToXna();
                }
                return m;
            }

            public NewBlendableTransform[] IterateTowardTarget(Matrix targetPoint, NewBlendableTransform[] boneTransforms)
            {
                baseTransforms = boneTransforms.ToArray();

                for (int i = 0; i < Skeleton.HkxSkeleton.Count; i++)
                {
                    testTransformMults[i] = 0;
                    var dirBone = Vector3.Transform(Vector3.Forward, baseTransforms[i].Rotation.ToXna());
                    var dirTarget = Vector3.Normalize((Vector3.Transform(Vector3.Zero, baseTransforms[i].GetMatrix().ToXna()) - Vector3.Transform(Vector3.Zero, targetPoint)));
                    //if (BoneIndicesInChain.Contains(i))
                    //testTransformDirections[i] = SoulsAssetPipeline.SapMath.GetDeltaQuaternionWithDirectionVectors(dirBone.ToCS(), dirTarget.ToCS()).ToXna() * Quaternion.Inverse(new NewBlendableTransform(GetTestFKOfBone(Skeleton.HkxSkeleton[i].ParentIndex).ToCS()).Rotation.ToXna());
                    testTransformDirections[i] = boneTransforms[i].Rotation.ToXna();
                }

                float TestDistance()
                {
                    return (Vector3.Transform(Vector3.Zero, GetTestFKOfBone(BoneIndicesInChain[0])) - (Vector3.Transform(Vector3.Zero, targetPoint))).LengthSquared();
                }

                float PartialGradient(int i)
                {
                    float origTestFloat = testTransformMults[i];

                    float f_x = TestDistance();

                    testTransformMults[i] += SamplingDistance;

                    float f_x_plus_d = TestDistance();

                    float gradient = (f_x_plus_d - f_x) / SamplingDistance;

                    testTransformMults[i] = origTestFloat;

                    return gradient;
                }

                for (int i = 0; i < BoneIndicesInChain.Count; i++)
                {
                    float gradient = PartialGradient(BoneIndicesInChain[i]);

                    float chainRatio = ((float)(i + 1) / (float)BoneIndicesInChain.Count);
                    testTransformMults[BoneIndicesInChain[i]] -= (LearningRate * gradient) * (1 - (chainRatio * 0.5f)) * (TestDistance() * TestDistance());

                    //baseTransforms[BoneIndicesInChain[i]] = GetTestTransform(BoneIndicesInChain[i]);

                    //if (TestDistance() < 0.05f)
                    //{
                    //    for (int j = 0; j < BoneIndicesInChain.Count; j++)
                    //    {
                    //        baseTransforms[BoneIndicesInChain[j]] = GetTestTransform(BoneIndicesInChain[j]);
                    //    }
                    //    return baseTransforms;
                    //}
                }

                for (int i = 0; i < BoneIndicesInChain.Count; i++)
                {
                    baseTransforms[BoneIndicesInChain[i]] = GetTestTransform(BoneIndicesInChain[i]);
                }


                return baseTransforms;
            }
        }

        public enum BlendPartType
        {
            Core,
            ArmL,
            ArmR,
            LegL,
            LegR,
        }

        public NewAnimSkeleton_HKX SourceSkeleton;
        public NewAnimSkeleton_HKX TargetSkeleton;

        int targetBoneIndex_Master = -1;
        int targetBoneIndex_Pelvis = -1;

        public NewBlendableTransform GetTargetFKTransform(int boneIndex)
        {
            var bone = TargetSkeleton.HkxSkeleton[boneIndex];
            var m = target_blendableTransforms[boneIndex].GetMatrix().ToXna();
            while (bone.ParentIndex >= 0)
            {
                int index = bone.ParentIndex;
                bone = TargetSkeleton.HkxSkeleton[index];
                m *= target_blendableTransforms[index].GetMatrix().ToXna();
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

        public void SetTargetFKTransform(int boneIndex, NewBlendableTransform t)
        {
            //var bone = TargetSkeleton.HkxSkeleton[boneIndex];
            //var m = Matrix.Identity;
            //while (bone.ParentIndex >= 0)
            //{
            //    int index = bone.ParentIndex;
            //    bone = TargetSkeleton.HkxSkeleton[index];
            //    m *= target_blendableTransforms[index].GetMatrix().ToXna();
            //}
            //var relativeMatrix = t.GetMatrix().ToXna() * Matrix.Invert(m);
            //target_blendableTransforms[boneIndex] = MatToTransform(relativeMatrix);

            void WalkTree_Target(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                if (i == boneIndex)
                {
                    target_blendableTransforms[i] = MatToTransform((t.GetMatrix().ToXna() * Matrix.Invert(currentMatrix)));
                    return;
                }
                currentMatrix = target_blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                currentScale *= target_blendableTransforms[i].Scale.ToXna();
                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_Target(c, currentMatrix, currentScale);
            }
            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_Target(root, Matrix.Identity, Vector3.One);
        }

        Dictionary<int, int> sourceBonesByTargetBones = new Dictionary<int, int>();

        private NewBlendableTransform[] source_blendableTransforms = new NewBlendableTransform[0];
        private Matrix[] source_absoluteBoneMatrices = new Matrix[0];
        private Matrix[] source_absoluteBoneMatrices_ForIK = new Matrix[0];

        private Matrix[] target_absoluteBoneMatrices_targetBaseIdle = new Matrix[0];
        private Matrix[] target_absoluteBoneMatrices_retargetedSourceIdle = new Matrix[0];

        private Matrix[] target_absoluteBoneMatrices_targetBlendIdle = new Matrix[0];
        private Matrix[] target_absoluteBoneMatrices_twoHandIdle = new Matrix[0];

        private Matrix[] target_absoluteBoneMatrices = new Matrix[0];
        private NewBlendableTransform[] target_blendableTransforms = new NewBlendableTransform[0];
        private NewBlendableTransform[] target_blendableTransforms_lastFrameIK = new NewBlendableTransform[0];

        private BlendPartType[] target_blendPartType = new BlendPartType[0];

        public Dictionary<string, string> SourceBoneCustomMappingByTargetBone = new Dictionary<string, string>();

        public LegacyIKSolver legacyIKSolver_LeftArm = null;

        public ArmIKSolver newIKSolver_LeftArm = new ArmIKSolver();

        Dictionary<string, int> targetBoneIndicesByName = new Dictionary<string, int>();
        Dictionary<string, int> sourceBoneIndicesByName = new Dictionary<string, int>();

        private bool BoneNameMatches(string a, string b)
        {
            if (a == b)
                return true;
            a = a.ToUpper();
            b = b.ToUpper();
            if (a == b)
                return true;
            else if (a.StartsWith("L ") && ("L_" + a.Substring(2)) == b)
                return true;
            else if (a.StartsWith("R ") && ("R_" + a.Substring(2)) == b)
                return true;

            return false;
        }

        public HavokSkeletonRemapper(NewAnimSkeleton_HKX sourceSkeleton, NewAnimSkeleton_HKX targetSkeleton, 
            Dictionary<string, string> sourceBoneCustomMappingByTargetBone, string pelvisName, string masterName)
        {
            SourceSkeleton = sourceSkeleton;
            TargetSkeleton = targetSkeleton;
            source_blendableTransforms = new NewBlendableTransform[sourceSkeleton.HkxSkeleton.Count];
            source_absoluteBoneMatrices = new Matrix[sourceSkeleton.HkxSkeleton.Count];
            source_absoluteBoneMatrices_ForIK = new Matrix[SourceSkeleton.HkxSkeleton.Count];

            target_absoluteBoneMatrices_targetBaseIdle = new Matrix[TargetSkeleton.HkxSkeleton.Count];
            target_absoluteBoneMatrices_targetBlendIdle = new Matrix[TargetSkeleton.HkxSkeleton.Count];
            target_absoluteBoneMatrices_twoHandIdle = new Matrix[TargetSkeleton.HkxSkeleton.Count];
            target_absoluteBoneMatrices_retargetedSourceIdle = new Matrix[TargetSkeleton.HkxSkeleton.Count];
            target_absoluteBoneMatrices = new Matrix[TargetSkeleton.HkxSkeleton.Count];

            SourceBoneCustomMappingByTargetBone = sourceBoneCustomMappingByTargetBone;

            

            for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
            {
                if (TargetSkeleton.HkxSkeleton[i].Name == masterName)
                    targetBoneIndex_Master = i;
                else if (TargetSkeleton.HkxSkeleton[i].Name == pelvisName)
                    targetBoneIndex_Pelvis = i;
                if (SourceBoneCustomMappingByTargetBone.ContainsKey(TargetSkeleton.HkxSkeleton[i].Name))
                {
                    sourceBonesByTargetBones.Add(i, SourceSkeleton.HkxSkeleton.FindIndex(h => h.Name == SourceBoneCustomMappingByTargetBone[TargetSkeleton.HkxSkeleton[i].Name]));
                }
                else
                {
                    sourceBonesByTargetBones.Add(i, SourceSkeleton.HkxSkeleton.FindIndex(h => BoneNameMatches(TargetSkeleton.HkxSkeleton[i].Name, h.Name)));
                }

                targetBoneIndicesByName.Add(TargetSkeleton.HkxSkeleton[i].Name, i);
            }

            for (int i = 0; i < SourceSkeleton.HkxSkeleton.Count; i++)
            {
                sourceBoneIndicesByName.Add(SourceSkeleton.HkxSkeleton[i].Name, i);
            }

            var targetIKChain_leftArm = new List<int>();
            targetIKChain_leftArm.Add(targetBoneIndicesByName["L_Hand"]);
            targetIKChain_leftArm.Add(targetBoneIndicesByName["L_Forearm"]);
            targetIKChain_leftArm.Add(targetBoneIndicesByName["L_UpperArm"]);
            targetIKChain_leftArm.Add(targetBoneIndicesByName["L_Clavicle"]);
            legacyIKSolver_LeftArm = new LegacyIKSolver(targetSkeleton, targetIKChain_leftArm);

            var targetIKChain_rightArm = new List<int>();
            targetIKChain_rightArm.Add(targetBoneIndicesByName["R_Hand"]);
            targetIKChain_rightArm.Add(targetBoneIndicesByName["R_Forearm"]);
            targetIKChain_rightArm.Add(targetBoneIndicesByName["R_UpperArm"]);
            targetIKChain_rightArm.Add(targetBoneIndicesByName["R_Clavicle"]);
            //ikSolver_RightArm = new IKSolver(targetSkeleton, targetIKChain_leftArm);

            target_blendableTransforms = new NewBlendableTransform[targetSkeleton.HkxSkeleton.Count];
            target_blendableTransforms_lastFrameIK = new NewBlendableTransform[targetSkeleton.HkxSkeleton.Count];
            target_blendPartType = new BlendPartType[TargetSkeleton.HkxSkeleton.Count];

            // Target Anim upper root check
            void WalkTree_UpperRootCheck(int i, BlendPartType curBlendPartType)
            {
                if (TargetSkeleton.HkxSkeleton[i].Name == "L_UpperArm")
                    curBlendPartType = BlendPartType.ArmL;
                else if (TargetSkeleton.HkxSkeleton[i].Name == "R_UpperArm")
                    curBlendPartType = BlendPartType.ArmR;
                else if (TargetSkeleton.HkxSkeleton[i].Name == "L_Thigh")
                    curBlendPartType = BlendPartType.LegL;
                else if (TargetSkeleton.HkxSkeleton[i].Name == "R_Thigh")
                    curBlendPartType = BlendPartType.LegR;

                target_blendPartType[i] = curBlendPartType;

                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_UpperRootCheck(c, curBlendPartType);
            }

            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_UpperRootCheck(root, BlendPartType.Core);

            for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
            {
                target_absoluteBoneMatrices_targetBlendIdle[i] = Matrix.Identity;
                target_absoluteBoneMatrices_targetBaseIdle[i] = Matrix.Identity;
                target_absoluteBoneMatrices_retargetedSourceIdle[i] = Matrix.Identity;
                target_absoluteBoneMatrices_twoHandIdle[i] = Matrix.Identity;
                target_absoluteBoneMatrices[i] = Matrix.Identity;
            }
        }

        private void CalculateIdlePoses(NewHavokAnimation baseIdleAnim, NewHavokAnimation blendToIdleAnim, float sourceScale, NewHavokAnimation twoHandIdleAnim)
        {
            // Source Anim
            void WalkTree_Idle_Source(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                currentMatrix = target_blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                currentScale *= target_blendableTransforms[i].Scale.ToXna();
                target_absoluteBoneMatrices_retargetedSourceIdle[i] = Matrix.CreateScale(currentScale) * currentMatrix;
                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_Idle_Source(c, currentMatrix, currentScale);
            }
            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_Idle_Source(root, Matrix.Identity, Vector3.One);

            // Target Anim
            void WalkTree_Idle_Target(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                target_blendableTransforms[i] = baseIdleAnim.GetBlendableTransformOnCurrentFrame(i);
                currentMatrix = target_blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                currentScale *= target_blendableTransforms[i].Scale.ToXna();
                target_absoluteBoneMatrices_targetBaseIdle[i] = Matrix.CreateScale(currentScale) * currentMatrix;
                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_Idle_Target(c, currentMatrix, currentScale);
            }
            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_Idle_Target(root, Matrix.Identity, Vector3.One);

            if (blendToIdleAnim != null)
            {
                // Target Blend Anim
                void WalkTree_IdleBlend_Target(int i, Matrix currentMatrix, Vector3 currentScale)
                {
                    target_blendableTransforms[i] = blendToIdleAnim.GetBlendableTransformOnCurrentFrame(i);
                    currentMatrix = target_blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                    currentScale *= target_blendableTransforms[i].Scale.ToXna();
                    target_absoluteBoneMatrices_targetBlendIdle[i] = Matrix.CreateScale(currentScale) * currentMatrix;
                    foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                        WalkTree_IdleBlend_Target(c, currentMatrix, currentScale);
                }
                foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                    WalkTree_IdleBlend_Target(root, Matrix.Identity, Vector3.One);
            }
            else
            {
                for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
                {
                    target_absoluteBoneMatrices_targetBlendIdle[i] = target_absoluteBoneMatrices_targetBaseIdle[i];
                }
            }

            if (twoHandIdleAnim != null)
            {
                // 2H Idle Anim
                void WalkTree_IdleBlend_Target(int i, Matrix currentMatrix, Vector3 currentScale)
                {
                    target_blendableTransforms[i] = twoHandIdleAnim.GetBlendableTransformOnCurrentFrame(i);
                    currentMatrix = target_blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                    currentScale *= target_blendableTransforms[i].Scale.ToXna();
                    target_absoluteBoneMatrices_twoHandIdle[i] = Matrix.CreateScale(currentScale) * currentMatrix;
                    foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                        WalkTree_IdleBlend_Target(c, currentMatrix, currentScale);
                }
                foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                    WalkTree_IdleBlend_Target(root, Matrix.Identity, Vector3.One);
            }
            else
            {
                for (int i = 0; i < TargetSkeleton.HkxSkeleton.Count; i++)
                {
                    target_absoluteBoneMatrices_twoHandIdle[i] = target_absoluteBoneMatrices_targetBaseIdle[i];
                }
            }
            


        }

        
        private Matrix GetTargetAbsBoneMat(int i, bool doIdleRemap, IdleCorrectBlendConfig idleCorrectBlendConfig, float idleCorrectRefFrame, float frameCount)
        {
            var sourceBoneIndex = sourceBonesByTargetBones[i];
            var absoluteMatrix = source_absoluteBoneMatrices[sourceBoneIndex];
            if (doIdleRemap)
            {
                var idleRemapDeltaMatrix = target_absoluteBoneMatrices_targetBaseIdle[i] * Matrix.Invert(target_absoluteBoneMatrices_retargetedSourceIdle[i]);

                float idleCorrectBlend = -1;

                if (target_blendPartType[i] == BlendPartType.Core)
                    idleCorrectBlend = idleCorrectBlendConfig.CoreCorrect.GetIdleBlend(idleCorrectRefFrame, frameCount);
                else if (target_blendPartType[i] == BlendPartType.ArmL)
                    idleCorrectBlend = idleCorrectBlendConfig.LeftArmCorrect.GetIdleBlend(idleCorrectRefFrame, frameCount);
                else if (target_blendPartType[i] == BlendPartType.ArmR)
                    idleCorrectBlend = idleCorrectBlendConfig.RightArmCorrect.GetIdleBlend(idleCorrectRefFrame, frameCount);
                else if (target_blendPartType[i] == BlendPartType.LegL)
                    idleCorrectBlend = idleCorrectBlendConfig.LeftLegCorrect.GetIdleBlend(idleCorrectRefFrame, frameCount);
                else if (target_blendPartType[i] == BlendPartType.LegR)
                    idleCorrectBlend = idleCorrectBlendConfig.RightLegCorrect.GetIdleBlend(idleCorrectRefFrame, frameCount);


                //if (TargetSkeleton.HkxSkeleton[i].Name == "R_Weapon")
                //{
                //    var weaponFix = TargetSkeleton.HkxSkeleton[i].RelativeReferenceMatrix * Matrix.Invert(SourceSkeleton.HkxSkeleton[sourceBoneIndex].RelativeReferenceMatrix);
                //    absoluteMatrix = Utils.SlowAccurateMatrixLerp(absoluteMatrix, weaponFix * absoluteMatrix, 1);
                //    idleCorrectBlend = 0;
                //}

                float fullIdleBlend = -1;

                if (idleCorrectBlendConfig.EndFullIdleBlendEnvelopeFrames > 0)
                {
                    fullIdleBlend = MathHelper.Clamp(((idleCorrectRefFrame - (frameCount - idleCorrectBlendConfig.EndFullIdleBlendEnvelopeFrames)) / idleCorrectBlendConfig.EndFullIdleBlendEnvelopeFrames), 0, 1);
                }

                if (idleCorrectBlend > 0)
                {
                    absoluteMatrix = Utils.SlowAccurateMatrixLerp(absoluteMatrix, idleRemapDeltaMatrix * absoluteMatrix, idleCorrectBlend * idleCorrectBlend);
                }

                if (fullIdleBlend > 0)
                {
                    absoluteMatrix = Utils.SlowAccurateMatrixLerp(absoluteMatrix, target_absoluteBoneMatrices_targetBlendIdle[i], fullIdleBlend * fullIdleBlend);
                }

                
            }

            if (idleCorrectBlendConfig.SourceBoneAbsCorrectMatrices != null && idleCorrectBlendConfig.SourceBoneAbsCorrectMatrices.ContainsKey(SourceSkeleton.HkxSkeleton[sourceBoneIndex].Name))
            {
                var correctMatrix = idleCorrectBlendConfig.SourceBoneAbsCorrectMatrices[SourceSkeleton.HkxSkeleton[sourceBoneIndex].Name];
                absoluteMatrix = correctMatrix * absoluteMatrix;
            }

            return absoluteMatrix;

        }

        private Matrix GetMirroredBoneMatrix(Matrix m)
        {
            var tr = MatToTransform(m);
            //tr.Translation *= -1;
            //tr.Scale = System.Numerics.Vector3.One;
            //tr.Translation.Z *= -1;
            //tr.Translation.X *= -1;
            //tr.Rotation = SoulsAssetPipeline.SapMath.MirrorQuat(tr.Rotation);
            //tr.Rotation = SoulsAssetPipeline.SapMath.MirrorQuat(tr.Rotation);
            //tr.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromQuaternion(tr.Rotation.ToXna()) * Matrix.CreateRotationY(MathHelper.Pi)).ToCS();

            //var axisAngle = SoulsAssetPipeline.SapMath.QuaternionToAxisAngle(tr.Rotation);
            //tr.Rotation = System.Numerics.Quaternion.CreateFromAxisAngle(new System.Numerics.Vector3(-axisAngle.X, axisAngle.Y, axisAngle.Z), -axisAngle.W);

            tr.Rotation.X *= -1;
            tr.Rotation.Z *= -1;

            //tr.Translation.Y *= -1;

            tr.Translation = System.Numerics.Vector3.Transform(tr.Translation, System.Numerics.Matrix4x4.CreateRotationZ(MathHelper.Pi));

            return tr.GetMatrix().ToXna() * Matrix.CreateRotationZ(MathHelper.Pi);
        }

        private void MirrorBoneMatrices(ref Matrix[] matrices, int indexA, int indexB)
        {
            var prevA = matrices[indexA];
            var prevB = matrices[indexB];
            matrices[indexA] = GetMirroredBoneMatrix(prevB);
            matrices[indexB] = GetMirroredBoneMatrix(prevA);
        }

        private void RemapCurrentFrame(NewHavokAnimation anim, bool doIdleRemap, IdleCorrectBlendConfig cfg, float idleCorrectRefFrame, float frameCount)
        {
            for (int i = 0; i < target_blendableTransforms.Length; i++)
            {
                target_blendableTransforms[i] = TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform;
            }

            // Source Anim
            void WalkTree_SourceAnim(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                source_blendableTransforms[i] = anim.GetBlendableTransformOnCurrentFrame(i).GetTScaled(cfg.SourceSkeletonScale);
                currentMatrix = source_blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                currentScale *= source_blendableTransforms[i].Scale.ToXna();
                source_absoluteBoneMatrices[i] = Matrix.CreateScale(currentScale) * (currentMatrix);

                foreach (var c in SourceSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_SourceAnim(c, currentMatrix, currentScale);
            }
            foreach (var root in SourceSkeleton.TopLevelHkxBoneIndices)
                WalkTree_SourceAnim(root, Matrix.Identity, Vector3.One);

            void WalkTree_SourceAnim_ForIK(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                var transform = anim.GetBlendableTransformOnCurrentFrame(i);
                currentMatrix = transform.GetMatrix().ToXna() * currentMatrix;
                currentScale *= transform.Scale.ToXna();
                source_absoluteBoneMatrices_ForIK[i] = Matrix.CreateScale(currentScale) * (currentMatrix);

                foreach (var c in SourceSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_SourceAnim_ForIK(c, currentMatrix, currentScale);
            }
            foreach (var root in SourceSkeleton.TopLevelHkxBoneIndices)
                WalkTree_SourceAnim_ForIK(root, Matrix.Identity, Vector3.One);

            if (cfg.MirrorAnimation)
            {
                List<string> mirroredBones = new List<string>();
                var boneNames = sourceBoneIndicesByName.Keys.ToList();
                foreach (var b in boneNames)
                {
                    if (mirroredBones.Contains(b))
                        continue;

                    int boneIndexL = -1;
                    int boneIndexR = -1;

                    if (b.StartsWith("R") && boneNames.Contains("L" + b.Substring(1)))
                    {
                        boneIndexL = sourceBoneIndicesByName["L" + b.Substring(1)];
                        boneIndexR = sourceBoneIndicesByName[b];
                    }
                    else if (b.StartsWith("L") && boneNames.Contains("R" + b.Substring(1)))
                    {
                        boneIndexL = sourceBoneIndicesByName[b];
                        boneIndexR = sourceBoneIndicesByName["R" + b.Substring(1)];
                    }
                    else if (b.StartsWith("Support_R") && boneNames.Contains("Support_L" + b.Substring(9)))
                    {
                        boneIndexL = sourceBoneIndicesByName["Support_L" + b.Substring(9)];
                        boneIndexR = sourceBoneIndicesByName[b];
                    }
                    else if (b.StartsWith("Support_L") && boneNames.Contains("Support_R" + b.Substring(9)))
                    {
                        boneIndexL = sourceBoneIndicesByName[b];
                        boneIndexR = sourceBoneIndicesByName["Support_R" + b.Substring(9)];
                    }

                    if (boneIndexL != -1 && boneIndexR != -1)
                    {
                        MirrorBoneMatrices(ref source_absoluteBoneMatrices, boneIndexL, boneIndexR);
                        MirrorBoneMatrices(ref source_absoluteBoneMatrices_ForIK, boneIndexL, boneIndexR);
                        mirroredBones.Add(SourceSkeleton.HkxSkeleton[boneIndexL].Name);
                        mirroredBones.Add(SourceSkeleton.HkxSkeleton[boneIndexR].Name);
                    }
                    else
                    {
                        int idx = sourceBoneIndicesByName[b];
                        source_absoluteBoneMatrices[sourceBoneIndicesByName[b]] = GetMirroredBoneMatrix(source_absoluteBoneMatrices[idx]);
                        source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName[b]] = GetMirroredBoneMatrix(source_absoluteBoneMatrices_ForIK[idx]);

                        if (SourceSkeleton.HkxSkeleton[idx].Name == "RootPos")
                        {
                            source_absoluteBoneMatrices[sourceBoneIndicesByName[b]] = Matrix.CreateRotationZ(MathHelper.Pi) * source_absoluteBoneMatrices[sourceBoneIndicesByName[b]];
                            source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName[b]] =  Matrix.CreateRotationZ(MathHelper.Pi) * source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName[b]];
                        }

                        mirroredBones.Add(b);
                    }

                    
                }
            }

            if (cfg.SourceBoneAbsCorrectMatrices != null)
            {
                foreach (var kvp in cfg.SourceBoneAbsCorrectMatrices)
                {
                    var cur = source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName[kvp.Key]];
                    cur = kvp.Value * cur;
                    source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName[kvp.Key]] = cur;
                }
            }

            // Target Anim
            void WalkTree_TargetAnim(int i, Matrix currentMatrix, Vector3 currentScale)
            {
                if (sourceBonesByTargetBones.ContainsKey(i) && sourceBonesByTargetBones[i] != -1)
                {
                    var absoluteMatrix = GetTargetAbsBoneMat(i, doIdleRemap, cfg, idleCorrectRefFrame, frameCount);

                    //if (TargetSkeleton.HkxSkeleton[i].Name == "Pelvis")
                    //{
                    //    absoluteMatrix = Matrix.CreateRotationZ(MathHelper.Pi) * absoluteMatrix;
                    //}

                    if (cfg.RaiseMasterUpToPelvis && i == targetBoneIndex_Master)
                    {
                        var pelvisAbsMatrix = GetTargetAbsBoneMat(targetBoneIndex_Pelvis, doIdleRemap, cfg, idleCorrectRefFrame, frameCount);
                        var masterLevel = Vector3.Transform(Vector3.Zero, absoluteMatrix).Y;
                        var pelvisLevel = Vector3.Transform(Vector3.Zero, pelvisAbsMatrix).Y;
                        absoluteMatrix *= Matrix.CreateTranslation(0, pelvisLevel - masterLevel, 0);
                    }

                    //if (ik)
                    //{



                    //}

                    //target_absoluteBoneMatrices[i] = absoluteMatrix;

                    var relativeMatrix = absoluteMatrix * Matrix.Invert(currentMatrix);

                    var prevTransform = target_blendableTransforms[i];

                    if (relativeMatrix.Decompose(out Vector3 s, out Quaternion r, out Vector3 t))
                        target_blendableTransforms[i] = new NewBlendableTransform(t.ToCS(), s.ToCS(), r.ToCS());

                    if (TargetSkeleton.HkxSkeleton[i].Name.ToLower() != "master")
                    {
                        target_blendableTransforms[i].Translation = TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform.Translation;
                    }





                    //target_blendableTransforms[i].Translation *= (TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform.Translation.Length() / target_blendableTransforms[i].Translation.Length());

                    //target_blendableTransforms[i].Translation = TargetSkeleton.HkxSkeleton[i].RelativeReferenceTransform.Translation;
                }

                currentMatrix = target_blendableTransforms[i].GetMatrix().ToXna() * currentMatrix;
                target_absoluteBoneMatrices[i] = target_blendableTransforms[i].GetMatrixScale().ToXna() * currentMatrix;
                currentScale *= target_blendableTransforms[i].Scale.ToXna();

                foreach (var c in TargetSkeleton.HkxSkeleton[i].ChildIndices)
                    WalkTree_TargetAnim(c, currentMatrix, currentScale);
            }
            foreach (var root in TargetSkeleton.TopLevelHkxBoneIndices)
                WalkTree_TargetAnim(root, Matrix.Identity, Vector3.One);

            if (cfg.ArmIK.L_Enabled)
            {
                ////////////////////////////////////////////////////////////////////////////////
                // New IK
                ////////////////////////////////////////////////////////////////////////////////

                int idx_UpperArm = targetBoneIndicesByName[cfg.ArmIK.L_UpperArm];
                int idx_Forearm = targetBoneIndicesByName[cfg.ArmIK.L_Forearm];
                int idx_Hand = targetBoneIndicesByName[cfg.ArmIK.L_Hand];
                int idx_ForeTwist = targetBoneIndicesByName[cfg.ArmIK.L_ForearmTwist];
                int idx_UpArmTwist = targetBoneIndicesByName[cfg.ArmIK.L_UpperArmTwist];

                for (int i = 0; i < 50; i++)
                {
                    float stepRatio = 0.05f;

                    // Store current for comparison to transform twist bones.
                    var upperArmPreIK = target_blendableTransforms[idx_UpperArm];
                    var forearmPreIK = target_blendableTransforms[idx_Forearm];
                    var handPreIK = target_blendableTransforms[idx_Hand];

                    var offsetFromTarget = source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName["L_Hand"]]
                        * Matrix.Invert(source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName["R_Hand"]]);
                    offsetFromTarget.Translation *= cfg.ArmIK.L_OffsetScale;
                    // Temp Test


                    newIKSolver_LeftArm.target = MatToTransform(offsetFromTarget * GetTargetFKTransform(targetBoneIndicesByName["R_Hand"]).GetMatrix().ToXna());
                    //newIKSolver_LeftArm.target = MatToTransform(source_absoluteBoneMatrices[sourceBoneIndicesByName["L_Hand"]]);
                    //newIKSolver_LeftArm.target.Translation.Z += 0.15f;
                    newIKSolver_LeftArm.upperArmIndex = idx_UpperArm;
                    newIKSolver_LeftArm.forearmIndex = idx_Forearm;
                    newIKSolver_LeftArm.handIndex = idx_Hand;
                    newIKSolver_LeftArm.GetBoneLocal = (int fkBoneIndex) => target_blendableTransforms[fkBoneIndex];
                    newIKSolver_LeftArm.SetBoneLocal = (int fkBoneIndex, NewBlendableTransform fkVal) => target_blendableTransforms[fkBoneIndex] = fkVal;
                    newIKSolver_LeftArm.GetBoneFK = (int fkBoneIndex) => GetTargetFKTransform(fkBoneIndex);
                    newIKSolver_LeftArm.SetBoneFK = (int fkBoneIndex, NewBlendableTransform fkVal) => SetTargetFKTransform(fkBoneIndex, fkVal);
                    newIKSolver_LeftArm.RotateBoneLocal = (int fkBoneIndex, System.Numerics.Quaternion fkRot) =>
                    {
                        target_blendableTransforms[fkBoneIndex].Rotation *= fkRot;
                        return GetTargetFKTransform(fkBoneIndex);
                    };
                    var upArmTwistRelativeToUpArm = GetTargetFKTransform(idx_UpArmTwist) * NewBlendableTransform.Invert(newIKSolver_LeftArm.upperArm);
                    var foreTwistRelativeToForearm = GetTargetFKTransform(idx_ForeTwist) * NewBlendableTransform.Invert(newIKSolver_LeftArm.forearm);
                    newIKSolver_LeftArm.IterateTowardTarget(1);


                    var curHand = GetTargetFKTransform(idx_Hand);
                    curHand.Rotation = newIKSolver_LeftArm.target.Rotation;
                    SetTargetFKTransform(idx_Hand, curHand);


                    SetTargetFKTransform(idx_UpArmTwist, upArmTwistRelativeToUpArm * newIKSolver_LeftArm.upperArm);
                    SetTargetFKTransform(idx_ForeTwist, foreTwistRelativeToForearm * newIKSolver_LeftArm.forearm);
                }
            }

            if (cfg.ArmIK.L_Enabled)
            {
                ////////////////////////////////////////////////////////////////////////////////
                // New IK
                ////////////////////////////////////////////////////////////////////////////////

                int idx_UpperArm = targetBoneIndicesByName["R_UpperArm"];
                int idx_Forearm = targetBoneIndicesByName["R_Forearm"];
                int idx_Hand = targetBoneIndicesByName["R_Hand"];
                int idx_ForeTwist = targetBoneIndicesByName["R_ForeTwist"];
                int idx_UpArmTwist = targetBoneIndicesByName["RUpArmTwist"];

                //for (int i = 0; i < 50; i++)
                //{
                //    float stepRatio = 0.05f;

                //    // Store current for comparison to transform twist bones.
                //    var upperArmPreIK = target_blendableTransforms[idx_UpperArm];
                //    var forearmPreIK = target_blendableTransforms[idx_Forearm];
                //    var handPreIK = target_blendableTransforms[idx_Hand];
                //    var offsetFromTarget = NewBlendableTransform.Identity;
                //    // Temp Test
                //    MatToTransform(source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName["R_Hand"]]);
                //    //newIKSolver_LeftArm.target = MatToTransform(source_absoluteBoneMatrices_ForIK[sourceBoneIndicesByName["R_Hand"]]);
                //    newIKSolver_LeftArm.target.Translation.Z += 0.15f;
                //    newIKSolver_LeftArm.target.Rotation = System.Numerics.Quaternion.Identity;
                //    newIKSolver_LeftArm.upperArmIndex = idx_UpperArm;
                //    newIKSolver_LeftArm.forearmIndex = idx_Forearm;
                //    newIKSolver_LeftArm.handIndex = idx_Hand;
                //    newIKSolver_LeftArm.GetBoneLocal = (int fkBoneIndex) => target_blendableTransforms[fkBoneIndex];
                //    newIKSolver_LeftArm.SetBoneLocal = (int fkBoneIndex, NewBlendableTransform fkVal) => target_blendableTransforms[fkBoneIndex] = fkVal;
                //    newIKSolver_LeftArm.GetBoneFK = (int fkBoneIndex) => GetTargetFKTransform(fkBoneIndex);
                //    newIKSolver_LeftArm.SetBoneFK = (int fkBoneIndex, NewBlendableTransform fkVal) => SetTargetFKTransform(fkBoneIndex, fkVal);
                //    newIKSolver_LeftArm.RotateBoneLocal = (int fkBoneIndex, System.Numerics.Quaternion fkRot) =>
                //    {
                //        target_blendableTransforms[fkBoneIndex].Rotation *= fkRot;
                //        return GetTargetFKTransform(fkBoneIndex);
                //    };
                //    var upArmTwistRelativeToUpArm = GetTargetFKTransform(idx_UpArmTwist) * NewBlendableTransform.Invert(newIKSolver_LeftArm.upperArm);
                //    var foreTwistRelativeToForearm = GetTargetFKTransform(idx_ForeTwist) * NewBlendableTransform.Invert(newIKSolver_LeftArm.forearm);
                //    newIKSolver_LeftArm.IterateTowardTarget(1);

                //    var curHand = GetTargetFKTransform(idx_Hand);
                //    curHand.Rotation = newIKSolver_LeftArm.target.Rotation;
                //    SetTargetFKTransform(idx_Hand, curHand);

                //    SetTargetFKTransform(idx_UpArmTwist, upArmTwistRelativeToUpArm * newIKSolver_LeftArm.upperArm);
                //    SetTargetFKTransform(idx_ForeTwist, foreTwistRelativeToForearm * newIKSolver_LeftArm.forearm);
                //}
            }

            //var targetWeaponAbsPos = GetTargetFKTransform(targetBoneIndicesByName["R_Weapon"]);
            //var targetLHandAbsPos = GetTargetFKTransform(targetBoneIndicesByName["L_Hand"]);

            //var sourceWeaponAbsPos = MatToTransform(source_absoluteBoneMatrices[sourceBoneIndicesByName["R_Weapon"]]);
            //var sourceLHandAbsPos = MatToTransform(source_absoluteBoneMatrices[sourceBoneIndicesByName["L_Hand"]]);

            //var targetWeaponDir = targetLHandAbsPos.Translation - targetWeaponAbsPos.Translation;
            //var sourceWeaponDir = sourceLHandAbsPos.Translation - sourceWeaponAbsPos.Translation;



            //var curWeaponBone = GetTargetFKTransform(targetBoneIndicesByName["R_Weapon"]);
            //var weaponBoneRotation = SoulsAssetPipeline.SapMath.GetDeltaQuaternionWithDirectionVectors(curWeaponBone.GetForward(), targetWeaponDir);
            //curWeaponBone.Rotation = System.Numerics.Quaternion.CreateFromRotationMatrix(
            //    System.Numerics.Matrix4x4.CreateFromQuaternion(weaponBoneRotation) * System.Numerics.Matrix4x4.CreateFromQuaternion(curWeaponBone.Rotation));
            //SetTargetFKTransform(targetBoneIndicesByName["R_Weapon"], curWeaponBone);
        }

        public SoulsAssetPipeline.AnimationImporting.ImportedAnimation RemapAnim(NewHavokAnimation anim, 
            NewHavokAnimation targetIdleAnim, NewHavokAnimation sourceIdleAnim, 
            NewHavokAnimation blendToIdleAnim = null, NewHavokAnimation twoHandIdleAnim = null, IdleCorrectBlendConfig cfg = null)
        {
            if (cfg == null)
                cfg = new IdleCorrectBlendConfig();

            anim.Reset(System.Numerics.Vector4.Zero);
            anim.EnableLooping = false;

            var thing = new SoulsAssetPipeline.AnimationImporting.ImportedAnimation();
            thing.BlendHint = anim.BlendHint;

            thing.FrameCount = anim.FrameCount;

            // If source animation is 30 FPS and not 60, then double frame count
            if (anim.FrameDuration > 0.02f)
            {
                //thing.Duration = (float)((thing.FrameCount) * (1.0 / 30.0));
                thing.FrameCount *= 2;
                
            }
            else
            {
                //thing.Duration = (float)((thing.FrameCount) * (1.0 / 60.0));
            }

            

            thing.Duration = anim.Duration;



            //if (anim.RootMotion.Data != null)
            //    anim.RootMotion.Data.Duration = thing.Duration;

            //thing.FrameCount--;
            thing.FrameDuration = (float)(1.0 / 60.0);


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

                target_blendableTransforms_lastFrameIK[i] = NewBlendableTransform.Identity;
            }

            if (targetIdleAnim != null)
            {
                anim.EnableLooping = false;
                anim.CurrentTime = anim.Duration;
                RemapCurrentFrame(sourceIdleAnim, doIdleRemap: false, cfg, 0, anim.FrameCount);
                CalculateIdlePoses(targetIdleAnim, blendToIdleAnim, cfg.SourceSkeletonScale, twoHandIdleAnim);
            }

            float currentTime = 1;

            while (currentTime < thing.Duration)
            {
                anim.CurrentTime = currentTime;

                RemapCurrentFrame(anim, doIdleRemap: true, cfg, (anim.CurrentTime / anim.FrameDuration), anim.FrameCount);




                var frame = new SoulsAssetPipeline.AnimationImporting.ImportedAnimation.Frame();
                frame.BoneTransforms = target_blendableTransforms.ToList();

                if (anim.RootMotion.Data != null)
                {
                    anim.RootMotion.SetTime(currentTime);
                    frame.RootMotionRotation = anim.RootMotion.CurrentTransform.W;
                    frame.RootMotionTranslation = anim.RootMotion.CurrentTransform.XYZ() * cfg.SourceSkeletonScale;
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

                //TODO: Read a speed curve here!
                currentTime += thing.FrameDuration * ((currentTime >= (thing.FrameDuration * 52)) ? 1.5f : 1.25f);
            }

            thing.Duration = (thing.Frames.Count - 1) * thing.FrameDuration;
            thing.FrameCount = thing.Frames.Count;
            return thing;
        }


    }
}
