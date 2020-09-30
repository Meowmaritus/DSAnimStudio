using Assimp;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsAssetPipeline
{
    public static class AnimationImporter
    {

        public class AnimationImportSettings
        {
            public float SceneScale = 1.0f;
            public bool ConvertFromZUp = false; // Not needed for anims? need to see if it's just my test fbx
            public List<string> ExistingBoneNameList = null;
            public HavokAnimationData ExistingHavokAnimationTemplate = null;
            public double ResampleToFramerate = 60;
            public string RootMotionNodeName = "root";

        }

        public static ImportedAnimation ImportFBX(string fbxPath, AnimationImportSettings settings)
        {
            using (var context = new AssimpContext())
            {
                var fbx = context.ImportFile(fbxPath, PostProcessSteps.CalculateTangentSpace);
                return ImportFromAssimpScene(fbx, settings);
            }
        }

        public static ImportedAnimation ImportFromAssimpScene(Scene scene, 
            AnimationImportSettings settings)
        {
            ImportedAnimation result = new ImportedAnimation();

            var sceneMatrix = System.Numerics.Matrix4x4.CreateScale(System.Numerics.Vector3.One * settings.SceneScale);

            sceneMatrix *= System.Numerics.Matrix4x4.CreateScale(-1, 1, 1);

            if (settings.ExistingHavokAnimationTemplate == null)
            {
                throw new NotImplementedException("Reading skeleton/binding from assimp scene not supported yet. Please import using existing havok animation as template.");
            }
            else
            {
                result.hkaSkeleton = settings.ExistingHavokAnimationTemplate.hkaSkeleton;
                result.HkxBoneIndexToTransformTrackMap = settings.ExistingHavokAnimationTemplate.HkxBoneIndexToTransformTrackMap;
                result.TransformTrackIndexToHkxBoneMap = settings.ExistingHavokAnimationTemplate.TransformTrackIndexToHkxBoneMap;
            }

            

            if (settings.ConvertFromZUp)
            {
                sceneMatrix *= System.Numerics.Matrix4x4.CreateRotationZ((float)(Math.PI));
                sceneMatrix *= System.Numerics.Matrix4x4.CreateRotationX((float)(-Math.PI / 2.0));
            }

            foreach (var anim in scene.Animations)
            {
                if (anim.HasNodeAnimations)
                {
                    // Setup framerate.
                    double tickScaler = (settings.ResampleToFramerate / anim.TicksPerSecond);

                    result.Duration = anim.DurationInTicks != 0 ? // Don't divide by 0
                        (float)(anim.DurationInTicks / anim.TicksPerSecond) : 0;
                    result.FrameDuration = (float)(1 / settings.ResampleToFramerate);

                    int frameCount = (int)Math.Round((anim.DurationInTicks / anim.TicksPerSecond) * settings.ResampleToFramerate);

                    double resampleTickMult = settings.ResampleToFramerate / anim.TicksPerSecond;

                    Dictionary<string, int> transformTrackIndexRemapForExistingBoneNameList
                        = new Dictionary<string, int>();

                    List<string> transformTrackNames = new List<string>();

                    // Populate transform track names.
                    foreach (var nodeChannel in anim.NodeAnimationChannels)
                    {
                        if (nodeChannel.NodeName == settings.RootMotionNodeName)
                            continue;
                        // If we have a predefined transform track list
                        // e.g. for an existing character, then just give the
                        // info needed to remap
                        if (settings.ExistingBoneNameList != null)
                        {
                            transformTrackIndexRemapForExistingBoneNameList.Add(nodeChannel.NodeName, 
                                settings.ExistingBoneNameList.IndexOf(nodeChannel.NodeName));
                        }
                        else
                        {
                            transformTrackNames.Add(nodeChannel.NodeName);
                        }
                        
                    }

                    result.TransformTrackNames = settings.ExistingBoneNameList != null ?
                        transformTrackNames.OrderBy(x => settings.ExistingBoneNameList.IndexOf(x)).ToList()
                        : transformTrackNames;

                    if (settings.ExistingBoneNameList != null)
                    {
                        transformTrackNames.Clear();
                        foreach (var name in settings.ExistingBoneNameList)
                            transformTrackNames.Add(name);
                    }

                    result.TransformTrackNames = transformTrackNames;

                    result.Frames = new List<ImportedAnimation.Frame>();

                    for (int i = 0; i <= frameCount; i++)
                    {
                        var f = new ImportedAnimation.Frame();
                        for (int j = 0; j < transformTrackNames.Count; j++)
                            f.BoneTransforms.Add(NewBlendableTransform.Identity);
                        result.Frames.Add(f);
                    }

                    for (int i = 0; i < anim.NodeAnimationChannelCount; i++)
                    {
                        var nodeChannel = anim.NodeAnimationChannels[i];

                        int lastKeyIndex = -1;

                        if (nodeChannel.NodeName == settings.RootMotionNodeName)
                        {
                            lastKeyIndex = -1;
                            foreach (var keyPos in nodeChannel.PositionKeys)
                            {
                                int frame = (int)Math.Floor(keyPos.Time * resampleTickMult);
                                result.Frames[frame].RootMotionTranslation = 
                                    System.Numerics.Vector3.Transform(keyPos.Value.ToNumerics(), sceneMatrix);
                                // Fill in from the last keyframe to this one
                                for (int f = lastKeyIndex + 1; f <= Math.Min(frame - 1, result.Frames.Count - 1); f++)
                                {
                                    float lerpS = 1f * (f - lastKeyIndex) / (frame - lastKeyIndex);
                                    var blendFrom = result.Frames[lastKeyIndex].RootMotionTranslation;
                                    var blendTo = result.Frames[frame].RootMotionTranslation;

                                    result.Frames[f].RootMotionTranslation = System.Numerics.Vector3.Lerp(blendFrom, blendTo, lerpS);
                                }
                                lastKeyIndex = frame;
                            }
                            // Fill in from last key to end of animation.
                            for (int f = lastKeyIndex + 1; f <= result.Frames.Count - 1; f++)
                            {
                                result.Frames[f].RootMotionTranslation = result.Frames[lastKeyIndex].RootMotionTranslation;
                            }

                            lastKeyIndex = -1;
                            foreach (var keyPos in nodeChannel.RotationKeys)
                            {
                                int frame = (int)Math.Floor(keyPos.Time * resampleTickMult);

                                var quatOnFrame = keyPos.Value.ToNumerics();

                                quatOnFrame.Y *= -1;
                                quatOnFrame.Z *= -1;

                                System.Numerics.Vector3 directionVectorOnFrame =
                                    System.Numerics.Vector3.Transform(System.Numerics.Vector3.UnitX, quatOnFrame);

                                float angleOnFrame = (float)Math.Atan2(directionVectorOnFrame.Z, directionVectorOnFrame.X);

                                result.Frames[frame].RootMotionRotation = angleOnFrame;
                                // Fill in from the last keyframe to this one
                                for (int f = lastKeyIndex + 1; f <= Math.Min(frame - 1, result.Frames.Count - 1); f++)
                                {
                                    float lerpS = 1f * (f - lastKeyIndex) / (frame - lastKeyIndex);
                                    var blendFrom = result.Frames[lastKeyIndex].RootMotionTranslation;
                                    var blendTo = result.Frames[frame].RootMotionTranslation;

                                    result.Frames[f].RootMotionRotation = 
                                        SapMath.Lerp(result.Frames[lastKeyIndex].RootMotionRotation, angleOnFrame, lerpS);
                                }
                                lastKeyIndex = frame;
                            }
                            // Fill in from last key to end of animation.
                            for (int f = lastKeyIndex + 1; f <= result.Frames.Count - 1; f++)
                            {
                                result.Frames[f].RootMotionRotation = result.Frames[lastKeyIndex].RootMotionRotation;
                            }
                        }
                        else
                        {
                            int transformIndex = settings.ExistingBoneNameList != null ?
                            transformTrackIndexRemapForExistingBoneNameList[nodeChannel.NodeName] : i;

                            if (transformIndex >= 0 && transformIndex < transformTrackNames.Count)
                            {
                                // TRANSLATION
                                lastKeyIndex = -1;
                                foreach (var keyPos in nodeChannel.PositionKeys)
                                {
                                    int frame = (int)Math.Floor(keyPos.Time * resampleTickMult);

                                    var curFrameTransform = result.Frames[frame].BoneTransforms[transformIndex];
                                    curFrameTransform.Translation = System.Numerics.Vector3.Transform(keyPos.Value.ToNumerics(), sceneMatrix);
                                    result.Frames[frame].BoneTransforms[transformIndex] = curFrameTransform;

                                    // Fill in from the last keyframe to this one
                                    for (int f = lastKeyIndex + 1; f <= Math.Min(frame - 1, result.Frames.Count - 1); f++)
                                    {
                                        float lerpS = 1f * (f - lastKeyIndex) / (frame - lastKeyIndex);
                                        var blendFrom = result.Frames[lastKeyIndex].BoneTransforms[transformIndex].Translation;
                                        var blendTo = curFrameTransform.Translation;

                                        var blended = System.Numerics.Vector3.Lerp(blendFrom, blendTo, lerpS);

                                        var copyOfStruct = result.Frames[f].BoneTransforms[transformIndex];
                                        copyOfStruct.Translation = blended;
                                        result.Frames[f].BoneTransforms[transformIndex] = copyOfStruct;
                                    }
                                    lastKeyIndex = frame;
                                }
                                // Fill in from last key to end of animation.
                                for (int f = lastKeyIndex + 1; f <= result.Frames.Count - 1; f++)
                                {
                                    var x = result.Frames[f].BoneTransforms[transformIndex];
                                    x.Translation = result.Frames[lastKeyIndex].BoneTransforms[transformIndex].Translation;
                                    result.Frames[f].BoneTransforms[transformIndex] = x;
                                }




                                // SCALE
                                lastKeyIndex = -1;
                                foreach (var keyPos in nodeChannel.ScalingKeys)
                                {
                                    int frame = (int)Math.Floor(keyPos.Time * resampleTickMult);

                                    var curFrameTransform = result.Frames[frame].BoneTransforms[transformIndex];
                                    curFrameTransform.Scale = keyPos.Value.ToNumerics();
                                    result.Frames[frame].BoneTransforms[transformIndex] = curFrameTransform;

                                    // Fill in from the last keyframe to this one
                                    for (int f = lastKeyIndex + 1; f <= Math.Min(frame - 1, result.Frames.Count - 1); f++)
                                    {
                                        float lerpS = 1f * (f - lastKeyIndex) / (frame - lastKeyIndex);
                                        var blendFrom = result.Frames[lastKeyIndex].BoneTransforms[transformIndex].Scale;
                                        var blendTo = curFrameTransform.Scale;

                                        var blended = System.Numerics.Vector3.Lerp(blendFrom, blendTo, lerpS);

                                        var copyOfStruct = result.Frames[f].BoneTransforms[transformIndex];
                                        copyOfStruct.Scale = blended;
                                        result.Frames[f].BoneTransforms[transformIndex] = copyOfStruct;
                                    }
                                    lastKeyIndex = frame;
                                }
                                // Fill in from last key to end of animation.
                                for (int f = lastKeyIndex + 1; f <= result.Frames.Count - 1; f++)
                                {
                                    var x = result.Frames[f].BoneTransforms[transformIndex];
                                    x.Scale = result.Frames[lastKeyIndex].BoneTransforms[transformIndex].Scale;
                                    result.Frames[f].BoneTransforms[transformIndex] = x;
                                }

                                // ROTATION
                                lastKeyIndex = -1;
                                foreach (var keyPos in nodeChannel.RotationKeys)
                                {
                                    int frame = (int)Math.Floor(keyPos.Time * resampleTickMult);

                                    var curFrameTransform = result.Frames[frame].BoneTransforms[transformIndex];
                                    curFrameTransform.Rotation = keyPos.Value.ToNumerics();
                                    curFrameTransform.Rotation.Y *= -1;
                                    curFrameTransform.Rotation.Z *= -1;
                                    result.Frames[frame].BoneTransforms[transformIndex] = curFrameTransform;

                                    // Fill in from the last keyframe to this one
                                    for (int f = lastKeyIndex + 1; f <= Math.Min(frame - 1, result.Frames.Count - 1); f++)
                                    {
                                        float lerpS = 1f * (f - lastKeyIndex) / (frame - lastKeyIndex);
                                        var blendFrom = result.Frames[lastKeyIndex].BoneTransforms[transformIndex].Rotation;
                                        var blendTo = curFrameTransform.Rotation;

                                        var blended = System.Numerics.Quaternion.Slerp(blendFrom, blendTo, lerpS);

                                        var copyOfStruct = result.Frames[f].BoneTransforms[transformIndex];
                                        copyOfStruct.Rotation = blended;
                                        result.Frames[f].BoneTransforms[transformIndex] = copyOfStruct;
                                    }
                                    lastKeyIndex = frame;
                                }
                                // Fill in from last key to end of animation.
                                for (int f = lastKeyIndex + 1; f <= result.Frames.Count - 1; f++)
                                {
                                    var x = result.Frames[f].BoneTransforms[transformIndex];
                                    x.Rotation = result.Frames[lastKeyIndex].BoneTransforms[transformIndex].Rotation;
                                    result.Frames[f].BoneTransforms[transformIndex] = x;
                                }
                            }    





                            
                        }

                       

                    }

                    result.FrameCount = frameCount;

                    break;



                }
            }

            result.RootMotion = new RootMotionData(
                new System.Numerics.Vector4(0, 1, 0, 0),
                new System.Numerics.Vector4(0, 0, 1, 0),
                result.Duration, result.Frames.Select(f => f.RootMotion).ToArray());
            
            // Copy first frame for loop?
            //for (int i = 0; i < result.TransformTrackNames.Count; i++)
            //{
            //    result.Frames[result.Frames.Count - 1].BoneTransforms[i] = result.Frames[0].BoneTransforms[i];
            //}

            result.Name = settings.ExistingHavokAnimationTemplate?.Name ?? "SAP Custom Animation";

            

            return result;
        }
    }
}
