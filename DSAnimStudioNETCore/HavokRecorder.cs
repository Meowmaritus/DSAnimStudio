using DSAnimStudio.ImguiOSD;
using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.AnimationImporting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DSAnimStudio
{
    public class HavokRecorder
    {
        private ImportedAnimation AnimData = new ImportedAnimation();

        public bool RecordHkxOnly;

        public double SampleFrameRate = 60.0;
        public double DeltaTime => 1.0 / SampleFrameRate;

        public HavokRecorder(List<NewBone> boneTransforms)
        {
            foreach (var transform in boneTransforms)
            {
                AnimData.TransformTrackNames.Add(transform.Name);
            }
        }

        public void ClearRecording()
        {
            AnimData.Frames.Clear();
        }

        public int FrameCount => AnimData.Frames.Count;

        public void AddFrame(Vector4 rootMotionDelta, List<NewBone> boneTransforms)
        {
            float rootMotionRotation = 0;
            System.Numerics.Vector3 rootMotionTranslation = System.Numerics.Vector3.Zero;
            if (AnimData.Frames.Count > 0)
            {
                rootMotionRotation = AnimData.Frames[AnimData.Frames.Count - 1].RootMotionRotation;
                rootMotionTranslation = AnimData.Frames[AnimData.Frames.Count - 1].RootMotionTranslation;
            }
            rootMotionRotation += rootMotionDelta.W;
            rootMotionTranslation += new System.Numerics.Vector3(rootMotionDelta.X, rootMotionDelta.Y, rootMotionDelta.Z);

            AnimData.Frames.Add(new ImportedAnimation.Frame()
            {
                RootMotionRotation = rootMotionRotation,
                RootMotionTranslation = rootMotionTranslation,
                BoneTransforms = boneTransforms.Select(x => x.LocalTransform).ToList(),
            });
        }

       
        
        public void FinalizeRecording(TaeEditor.TaeEditorScreen mainScreen)
        {
            AnimData.BlendHint = HKX.AnimationBlendHint.NORMAL;
            AnimData.FrameDuration = (float)DeltaTime;
            AnimData.FrameCount = AnimData.Frames.Count;
            AnimData.Duration = (float)((AnimData.FrameCount - 1) * DeltaTime);
            if (AnimData.Duration < AnimData.FrameDuration)
                AnimData.Duration = AnimData.FrameDuration;

            var rootMotionStartRot = AnimData.Frames[0].RootMotionRotation;
            var rootMotionStartTranslation = AnimData.Frames[0].RootMotionTranslation;

            for (int f = 0; f < AnimData.Frames.Count; f++)
            {
                AnimData.Frames[f].RootMotionTranslation -= rootMotionStartTranslation;
                AnimData.Frames[f].RootMotionTranslation = System.Numerics.Vector3.Transform(AnimData.Frames[f].RootMotionTranslation, System.Numerics.Matrix4x4.CreateRotationY(-rootMotionStartRot));
                AnimData.Frames[f].RootMotionRotation -= rootMotionStartRot;
            }

            AnimData.hkaSkeleton = mainScreen.ParentDocument.Scene.MainModel.AnimContainer.Skeleton.OriginalHavokSkeleton;
            AnimData.HkxBoneIndexToTransformTrackMap = new int[AnimData.Frames[0].BoneTransforms.Count];
            AnimData.TransformTrackIndexToHkxBoneMap = new int[AnimData.Frames[0].BoneTransforms.Count];
            AnimData.TransformTrackToBoneIndices = new Dictionary<string, int>();
            for (int i = 0; i < AnimData.Frames[0].BoneTransforms.Count; i++)
            {
                AnimData.TransformTrackToBoneIndices.Add(AnimData.TransformTrackNames[i], i);
                AnimData.HkxBoneIndexToTransformTrackMap[i] = i;
                AnimData.TransformTrackIndexToHkxBoneMap[i] = i;
            }

            var readyForGame = ImportedAnimationConverter.GetAnimReadyToPutIntoGameFromImported(AnimData);

            void AskForIDInput()
            {
                DialogManager.AskForInputString("Input to TAE Animation ID", "Enter the animation ID to save the actions of the recording to as well as to save the HKX to.\n" +
                    "Accepts the full string with prefix or just the ID as a number.\n" +
                    "EXISTING TAE ENTRY AND ANIMATION FILES WITH THIS NAME WILL BE OVERRIDDEN WITHOUT ASKING.\nSAVE / BACKUP YOUR WORK IF UNSURE..",
                mainScreen.ParentDocument.GameRoot.CurrentAnimIDFormatType.ToString(), idResult =>
                {
                
                    if (SplitAnimID.TryParse(mainScreen.ParentDocument.GameRoot, idResult, out SplitAnimID id, out string detailedError))
                    {
                        var shortName = id.ToString();

                        mainScreen.FileContainer.AddNewHKX(id, shortName, readyForGame.DataForGame, out byte[] dataForAnimContainer);//, readyForGame.Data2010);

                        if (dataForAnimContainer == null)
                        {
                            DialogManager.DialogOK("Failed", "Failed to save (TagTools refused to work), just try again.");
                            return;
                        }

                        mainScreen.Graph.ViewportInteractor.CurrentModel.AnimContainer.AddNewHKXToLoad(id, shortName + ".hkx", readyForGame.DataForGame);
                        mainScreen.ReselectCurrentAnimation();
                        mainScreen.HardReset();

                        if (!RecordHkxOnly)
                        {
                            //var animRef = TimeActRecorder.RecordActions(mainScreen, id, shortName, entries)
                            
                        }
                        else
                        {
                            DialogManager.DialogOK("Import Complete", $"Finished importing animation HKX.");
                            //mainScreen.Graph.ViewportInteractor.IsComboRecordingEnding = false;
                        }
                    }
                    else
                    {
                        DialogManager.DialogOK("Invalid Import ID", $"\"{idResult}\" is not a valid animation ID.");
                        AskForIDInput();
                    }
                }, checkError: input =>
                {
                    bool parseSuccess = SplitAnimID.TryParse(mainScreen.Proj, input, out SplitAnimID parsed, out string detailedError);
                    if (!parseSuccess)
                        return detailedError;
                    return null;
                }, canBeCancelled: true);
            }

            AskForIDInput();
        }


    }
}
