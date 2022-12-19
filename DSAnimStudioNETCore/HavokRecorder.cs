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

        public HavokRecorder(List<NewAnimSkeleton_HKX.HkxBoneInfo> boneTransforms)
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

        public void AddFrame(Vector4 rootMotionDelta, List<NewAnimSkeleton_HKX.HkxBoneInfo> boneTransforms)
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
                BoneTransforms = boneTransforms.Select(x => x.CurrentHavokTransform).ToList(),
            });
        }

        public void FinalizeRecording(TaeEditor.TaeEditorScreen mainScreen, TaeComboEntry[] combo)
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

            AnimData.hkaSkeleton = Scene.MainModel.AnimContainer.Skeleton.OriginalHavokSkeleton;
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
                DialogManager.AskForInputString("Input to TAE Animation ID", "Enter the animation ID to save the events of the recording to as well as to save the HKX to.\n" +
                    "Accepts the full string with prefix or just the ID as a number.\n" +
                    "EXISTING TAE ENTRY AND ANIMATION FILES WITH THIS NAME WILL BE OVERRIDDEN WITHOUT ASKING.\nSAVE / BACKUP YOUR WORK IF UNSURE..",
                GameRoot.CurrentAnimIDFormatType.ToString(), idResult =>
                {
                
                    if (int.TryParse(idResult.Replace("a", "").Replace("_", ""), out int id))
                    {
                        var shortName = idResult;

                        var splitID = GameRoot.SplitAnimID.FromFullID(id);

                        if (GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXX_YYYY)
                        {
                            shortName = $"a{splitID.TaeID:D2}_{splitID.SubID:D4}";
                        }
                        else if (GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXXX_YYYYYY)
                        {
                            shortName = $"a{splitID.TaeID:D3}_{splitID.SubID:D6}";
                        }
                        else if (GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXX_YY_ZZZZ)
                        {
                            shortName = $"a{splitID.TaeID:D3}_{splitID.SubID:D6}";
                            shortName.Insert(6, "_");
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                        mainScreen.FileContainer.AddNewHKX(shortName, readyForGame.DataForGame, out byte[] dataForAnimContainer);//, readyForGame.Data2010);

                        if (dataForAnimContainer == null)
                        {
                            DialogManager.DialogOK("Failed", "Failed to save (TagTools refused to work), just try again.");
                            return;
                        }

                        mainScreen.Graph.ViewportInteractor.CurrentModel.AnimContainer.AddNewHKXToLoad(shortName + ".hkx", readyForGame.DataForGame);
                        mainScreen.ReselectCurrentAnimation();
                        mainScreen.HardReset();

                        if (!RecordHkxOnly)
                        {

                            var anim = mainScreen.CreateNewAnimWithFullID(id, "[REC]" + shortName, nukeExisting: true);

                            anim.SetIsModified(true);

                            TAE.Event blendEvent = null;
                            List<TaeComboEntry> validComboEntries = new List<TaeComboEntry>();
                            List<List<TAE.Event>> eventLists = new List<List<TAE.Event>>();
                            List<List<TAE.Event>> eventLists_BeforeStart = new List<List<TAE.Event>>();
                            List<List<TAE.Event>> eventLists_AfterEnd = new List<List<TAE.Event>>();

                            int toFrame(float t)
                            {
                                return (int)Math.Round(t / (1f / 30f));
                            }

                            float toTime(int f)
                            {
                                return (float)(f * (1f / 30f));
                            }

                            float currentOutputAnimTime = 0;

                            int currentEventListIndex = 0;

                            var cloneToOriginalMap = new Dictionary<TAE.Event, TAE.Event>();

                            for (int i = 0; i < combo.Length; i++)
                            {

                                

                                if (combo[i].ResolvedAnimRef != null)
                                {
                                    validComboEntries.Add(combo[i]);

                                    float comboClipLength = (validComboEntries[currentEventListIndex].ResolvedEndTime - validComboEntries[currentEventListIndex].ResolvedStartTime);

                                    var newEventList = new List<TAE.Event>();
                                    var newEventList_BeforeStart = new List<TAE.Event>();
                                    var newEventList_AfterEnd = new List<TAE.Event>();

                                    int timespanMinFrame = toFrame(combo[i].ResolvedStartTime);
                                    int timespanMaxFrame = toFrame(combo[i].ResolvedEndTime);
                                    foreach (var ev in combo[i].ResolvedAnimRef.Events)
                                    {
                                        if (ev.Type == 16)
                                        {
                                            if (blendEvent == null)
                                                blendEvent = ev.GetClone(GameRoot.IsBigEndianGame);

                                            continue;
                                        }
                                        //int evStartFrame = toFrame(ev.StartTime);
                                        //int evEndFrame = toFrame(ev.EndTime);
                                        //bool startsInThis = evStartFrame >= timespanMinFrame;
                                        //bool endsInThis = evEndFrame <= timespanMaxFrame;

                                        

                                        if (ev.EndTime >= combo[i].ResolvedStartTime && ev.StartTime <= combo[i].ResolvedEndTime)
                                        {
                                            var evClone = ev.GetClone(GameRoot.IsBigEndianGame);

                                            cloneToOriginalMap[evClone] = ev;

                                            newEventList.Add(evClone);

                                            //Slide event over so it's relative to time 0
                                            evClone.StartTime -= validComboEntries[currentEventListIndex].ResolvedStartTime;
                                            evClone.EndTime -= validComboEntries[currentEventListIndex].ResolvedStartTime;
                                            // Trim events that start before this clip
                                            if (evClone.StartTime < 0)
                                            {
                                                evClone.StartTime = 0;
                                                newEventList_BeforeStart.Add(evClone);
                                            }
                                            //Trim events that end after this clip
                                            if (evClone.EndTime > comboClipLength)
                                            {
                                                evClone.EndTime = comboClipLength;
                                                newEventList_AfterEnd.Add(evClone);
                                            }

                                            // Shift event to final clip position in output anim
                                            evClone.StartTime += currentOutputAnimTime;
                                            evClone.EndTime += currentOutputAnimTime;
                                        }

                                    }

                                    if (currentEventListIndex > 0)
                                    {
                                        var prevClipEventsContinuingIntoThisOne = eventLists_AfterEnd[currentEventListIndex - 1];
                                        foreach (var ev in prevClipEventsContinuingIntoThisOne)
                                        {
                                            var matchingContinuationEv = newEventList_BeforeStart.FirstOrDefault(e => e.IsIdenticalTo(ev, GameRoot.IsBigEndianGame));
                                            if (matchingContinuationEv != null)
                                            {
                                                ev.EndTime = matchingContinuationEv.EndTime;
                                                newEventList_BeforeStart.Remove(ev);
                                                newEventList.Remove(ev);
                                            }
                                        }
                                    }

                                    eventLists.Add(newEventList);
                                    eventLists_BeforeStart.Add(newEventList_BeforeStart);
                                    eventLists_AfterEnd.Add(newEventList_AfterEnd);

                                    currentEventListIndex++;

                                    currentOutputAnimTime += comboClipLength;
                                }
                            }

                            

                            
                            
                            for (int i = 0; i < eventLists.Count; i++)
                            {

                                foreach (var ev in eventLists[i])
                                {
                                    if (!anim.Events.Contains(ev))
                                        anim.Events.Add(ev);
                                }

                                //foreach (var ev in eventLists_BeforeStart[i])
                                //{
                                //    if (!anim.Events.Contains(ev))
                                //        anim.Events.Add(ev);
                                //}

                                //foreach (var ev in eventLists_AfterEnd[i])
                                //{
                                //    if (!anim.Events.Contains(ev))
                                //        anim.Events.Add(ev);
                                //}
                            }

                            


                            var rowCacheEntries = new Dictionary<TAE.Event, int>();
                            foreach (var comboEntry in combo)
                            {
                                foreach (var kvp in comboEntry.EventRowCache)
                                {
                                    rowCacheEntries[kvp.Key] = kvp.Value;
                                }
                            }
                            anim.Events = anim.Events.OrderBy(e => rowCacheEntries.ContainsKey(cloneToOriginalMap[e]) ? rowCacheEntries[cloneToOriginalMap[e]] : -1).ToList();

                            if (blendEvent != null)
                            {
                                blendEvent.StartTime = 0;
                                blendEvent.EndTime = (float)(8.0 * (1.0 / 30.0));
                                anim.Events.Insert(0, blendEvent);
                            }

                            mainScreen.RecreateAnimList();

                            DialogManager.DialogOK("Import Complete", $"Finished importing animation HKX and TAE.");
                            mainScreen.Graph.ViewportInteractor.IsComboRecordingEnding = false;
                        }
                        else
                        {
                            DialogManager.DialogOK("Import Complete", $"Finished importing animation HKX.");
                            mainScreen.Graph.ViewportInteractor.IsComboRecordingEnding = false;
                        }
                    }
                    else
                    {
                        DialogManager.DialogOK("Invalid Import ID", $"\"{idResult}\" is not a valid animation ID.");
                        AskForIDInput();
                    }
                }, canBeCancelled: true);
            }

            AskForIDInput();
        }


    }
}
