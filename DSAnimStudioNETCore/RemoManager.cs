using DSAnimStudio.TaeEditor;
using Microsoft.Xna.Framework;
using SharpDX.DirectWrite;
using SoulsAssetPipeline;
using SoulsAssetPipeline.Animation;
using SoulsAssetPipeline.Animation.SIBCAM;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class RemoManager
    {
        public static bool ContinuePlayingNextFrame = false;

        public static void CancelFullPlayback()
        {
            StopStreamedBGM();
            ViewportInteractor?.Graph?.PlaybackCursor?.ClearRemoState();
        }

        public static NewFmodIns.FmodEventUpdater StreamedBGM = null;
        public static void StartStreamedBGM()
        {
            zzz_DocumentManager.CurrentDocument.Fmod.PlaySE(4, (int.Parse(RemoName.Substring(3, 6)) * 1000) + 1, -1);
        }
        public static void StopStreamedBGM()
        {
            StreamedBGM?.Stop(true);
        }

        public static void PauseStreamBGM()
        {
            StreamedBGM?.Pause();
        }

        public static void ResumeStreamedBGM()
        {
            StreamedBGM?.Resume();
        }

        public static void StopFullPreview()
        {
            if (StreamedBGM != null && !StreamedBGM.EventIsOver)
                StreamedBGM.Stop(true);
        }

        public static void StartFullPreview()
        {
            if (StreamedBGM != null && !StreamedBGM.EventIsOver)
                StreamedBGM.Stop(true);

            //TaeActionSimulationEnvironment.GlobalPlaybackInstanceStop();

            zzz_DocumentManager.CurrentDocument.SoundManager.StopAllSounds(immediate: true);

            ViewportInteractor.Graph.MainScreen.SelectNewAnimRef(ViewportInteractor.Graph.MainScreen.SelectedAnimCategory,
                ViewportInteractor.Graph.MainScreen.SelectedAnimCategory.SAFE_GetFirstAnimInList(throwIfEmpty: true));
            ViewportInteractor.Graph.MainScreen.HardReset();

            ViewportInteractor.Graph.PlaybackCursor.Scrubbing = false;
            ViewportInteractor.Graph.PlaybackCursor.IsPlaying = true;
            ViewportInteractor.Graph.PlaybackCursor.IsStepping = false;

            StartStreamedBGM();

            ViewportInteractor?.Graph?.PlaybackCursor?.SetRemoState();
        }

        public static TaeViewportInteractor ViewportInteractor = null;

        public static NewAnimationContainer AnimContainer = null;

        public static SIBCAM2 CurrentCutSibcam = null;

        public static string RemoName = "scnXXXXXX";

        private static string CurrentCut = null;

        public static string BLOCK => RemoName.Substring(5, 2);
        public static string AREA => RemoName.Substring(3, 2);
        public static string MAP => $"m{AREA}_{BLOCK}_00_00";

        public static int BlockInt => int.Parse(BLOCK);
        public static int AreaInt => int.Parse(AREA);

        private static object _lock_remodict = new object();

        private static Dictionary<string, Model> remoModelDict = new Dictionary<string, Model>();

        private class RemoCutCache
        {
            public HKX Packfile = null;
            public HKX.HKAAnimationBinding hk_binding = null;
            public HKX.HKASplineCompressedAnimation hk_anim = null;
            public HKX.HKASkeleton hk_skeleton = null;
        }

        public static bool EnableRemoCameraInViewport = true;
        public static bool EnableDummyPrims = true;

        public static SibcamPlayer SibcamPlayer = null;
        

        public static List<string> CurrentCutHits = new List<string>();
        public static List<int> CurrentCutOtherBlocks = new List<int>();

        private static Dictionary<string, RemoCutCache> remoCutsLoaded = new Dictionary<string, RemoCutCache>();
        private static Dictionary<string, byte[]> remoCutHkxDict = new Dictionary<string, byte[]>();
        private static Dictionary<string, SIBCAM2> remoCutSibcamDict = new Dictionary<string, SIBCAM2>();


        public static void NukeEntireRemoSystemAndGoBackToNormalDSAnimStudio()
        {
            RemoEventSim.Clear();
            ViewportInteractor = null;
            AnimContainer = null;
            CurrentCutSibcam = null;
            CurrentCut = null;
            RemoName = "scnXXXXXX";
            remoCutsLoaded.Clear();
            remoCutHkxDict.Clear();
            remoCutSibcamDict.Clear();

            GFX.CurrentWorldView.RemoCamView = null;
            CurrentCutHits.Clear();
            CurrentCutOtherBlocks.Clear();
            DisposeAllModels();
        }



        public static void LoadRemoDict(TaeFileContainer fileContainer)
        {
            CurrentCut = null;

            lock (_lock_remodict)
            {
                // remoCutHkxDict.Clear();
                // foreach (var hkx in fileContainer.AllHKXDict)
                // {
                //     remoCutHkxDict.Add(Utils.GetShortIngameFileName(hkx.Key) + ".hkx", hkx.Value);
                // }
                //
                // remoCutSibcamDict.Clear();
                // foreach (var sibcam in fileContainer.AllSibcamDict)
                // {
                //     remoCutSibcamDict.Add(Utils.GetShortIngameFileName(sibcam.Key) + ".hkx", SIBCAM.Read(sibcam.Value));
                // }
                //
                // remoCutsLoaded.Clear();
            }

            DisposeAllModels();
            remoModelDict = new Dictionary<string, Model>();

            if (remoCutHkxDict.Count > 0)
            {
                LoadRemoCut(remoCutHkxDict.Keys.First());
            }


        }

        public static bool RemoCutLoaded(string cutName)
        {
            bool result = false;
            lock (_lock_remodict)
            {
                if (remoCutsLoaded.ContainsKey(cutName))
                {
                    result = true;
                }
            }
            return result;
        }

        public static void DisposeAllModels()
        {

            //foreach (var m in remoModelDict)
            //{
            //    m.Value?.Dispose();
            //}
            //remoModelDict?.Clear();
            //zzz_DocumentManager.CurrentDocument.Scene?.ClearScene();
            //zzz_DocumentManager.CurrentDocument.TexturePool.Flush();
            //remoModelDict = new Dictionary<string, Model>();
        }

        public static void LoadRemoCut(string cutHkxName)
        {

            if (cutHkxName == CurrentCut)
                return;

            byte[] cut = null;
            lock (_lock_remodict)
            {
                if (remoCutHkxDict.ContainsKey(cutHkxName))
                {
                    cut = remoCutHkxDict[cutHkxName];
                }
            }

            if (cut != null)
            {
                LoadRemoHKX(cut, cutHkxName);
                if (remoCutSibcamDict.ContainsKey(cutHkxName))
                {
                    CurrentCutSibcam = remoCutSibcamDict[cutHkxName];
                    SibcamPlayer = new SibcamPlayer(CurrentCutSibcam);
                }
                else
                    CurrentCutSibcam = null;
            }
            else
            {
                if (AnimContainer != null)
                    AnimContainer?.ClearAnimation();
            }
        }

        
        

        

        

        public static bool UpdateCutAdvance()
        {
            if (ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
                            && ViewportInteractor.Graph.PlaybackCursor.IsPlaying && !ViewportInteractor.Graph.PlaybackCursor.Scrubbing &&
                            ViewportInteractor?.Graph?.MainScreen?.REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME != true
                            && ViewportInteractor?.Graph?.MainScreen?.REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME != true)
            {
                int nextFrame = (int)Math.Round((ViewportInteractor.Graph.PlaybackCursor.CurrentTime) / (1f / 30f));
                int maxFrame = (int)Math.Round((RemoManager.AnimContainer?.CurrentAnimation?.Duration ?? ViewportInteractor.Graph.PlaybackCursor.MaxTime) / (1f / 30f));
                if (nextFrame >= maxFrame)
                {
                    ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = true;

                    ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV = false;
                    ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT = false;
                    ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL = false;

                    ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = null;
                    ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = null;

                    return true;
                }
            }
            return false;
        }

        public static void UpdateRemoTime(float time)
        {
            // JUST FOR MATH TESTING
            //BakeSibcam();

            SibcamPlayer.SetTime(time);

            if (EnableRemoCameraInViewport)
            {
                GFX.CurrentWorldView.RemoCamView = SibcamPlayer.CurrentView;
            }
            else
            {
                GFX.CurrentWorldView.RemoCamView = null;
            }

            

            

            RemoEventSim.Update(ViewportInteractor.Graph, time);
        }

       

        public static Model LookupModelOfEventGroup(DSAProj.ActionTrack group)
        {
            var sb = new StringBuilder();
            if (group.TrackData.DataType == TAE.ActionTrack.ActionTrackDataType.ApplyToSpecificCutsceneEntity)
            {
                if (group.TrackData.Block >= 0 || group.TrackData.Area >= 0)
                {
                    sb.Append($"A{group.TrackData.Area:D2}_{group.TrackData.Block:D2}_");
                }

                if (group.TrackData.CutsceneEntityType == TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.Character)
                    sb.Append($"c{group.TrackData.CutsceneEntityIDPart1:D4}_{group.TrackData.CutsceneEntityIDPart2:D4}");
                else if (group.TrackData.CutsceneEntityType == TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.Object)
                    sb.Append($"o{group.TrackData.CutsceneEntityIDPart1:D4}_{group.TrackData.CutsceneEntityIDPart2:D4}");
                else if (group.TrackData.CutsceneEntityType == TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.DummyNode)
                    sb.Append($"d{group.TrackData.CutsceneEntityIDPart1:D4}_{group.TrackData.CutsceneEntityIDPart2:D4}");
                else if (group.TrackData.CutsceneEntityType == TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.MapPiece)
                {
                    if (group.TrackData.Block >= 0)
                        sb.Append($"m{group.TrackData.CutsceneEntityIDPart1:D4}B{group.TrackData.Block}");
                    else
                        sb.Append($"m{group.TrackData.CutsceneEntityIDPart1:D4}B{RemoManager.BlockInt}");

                    if (group.TrackData.CutsceneEntityIDPart2 > 0)
                    {
                        sb.Append($"_{group.TrackData.CutsceneEntityIDPart2:D4}");
                    }
                }

                var mdls = zzz_DocumentManager.CurrentDocument.Scene.Models.ToList();
                var foundModel = mdls.FirstOrDefault(m => m.Name == sb.ToString());
                if (foundModel == null &&
                    group.TrackData.CutsceneEntityType == TAE.ActionTrack.ActionTrackDataStruct.EntityTypes.MapPiece
                    && group.TrackData.CutsceneEntityIDPart2 == 0)
                {
                    sb.Append("_0000");
                    foundModel = mdls.FirstOrDefault(m => m.Name == sb.ToString());
                }
                return foundModel;
            }

            return null;
        }

        private static void LoadRemoHKX(byte[] hkxBytes, string animName)
        {
            zzz_DocumentManager.CurrentDocument.Scene.DisableModelDrawing();
            zzz_DocumentManager.CurrentDocument.Scene.DisableModelDrawing2();

            HKX packfile = null;
            HKX.HKAAnimationBinding hk_binding = null;
            HKX.HKASplineCompressedAnimation hk_anim = null;
            HKX.HKASkeleton hk_skeleton = null;

            if (remoCutsLoaded.ContainsKey(animName))
            {
                packfile = remoCutsLoaded[animName].Packfile;
                hk_binding = remoCutsLoaded[animName].hk_binding;
                hk_anim = remoCutsLoaded[animName].hk_anim;
                hk_skeleton = remoCutsLoaded[animName].hk_skeleton;
            }
            else
            {
                var hkx = HKX.Read(hkxBytes, HKX.HKXVariation.HKXDS1, false);

                foreach (var o in hkx.DataSection.Objects)
                {
                    if (o is HKX.HKASkeleton asSkeleton)
                        hk_skeleton = asSkeleton;
                    else if (o is HKX.HKAAnimationBinding asBinding)
                        hk_binding = asBinding;
                    else if (o is HKX.HKASplineCompressedAnimation asAnim)
                        hk_anim = asAnim;
                }

                remoCutsLoaded.Add(animName, new RemoCutCache()
                {
                    Packfile = hkx,
                    hk_binding = hk_binding,
                    hk_anim = hk_anim,
                    hk_skeleton = hk_skeleton,
                });

                
            }

            

            var animContainer = new NewAnimationContainer(zzz_DocumentManager.CurrentDocument.Proj, null, zzz_DocumentManager.CurrentDocument);

            AnimContainer = animContainer;

            //animContainer.ClearAnimations();

            animContainer.Skeleton.LoadHKXSkeleton(packfile);

            var testIdleAnimThing = new NewHavokAnimation_SplineCompressed(-1, animName,
                animContainer.Skeleton, null, hk_binding, hk_anim, animContainer, fileSize: -1);

            animContainer.AddNewAnimation_Deprecated(SplitAnimID.Invalid, animName, testIdleAnimThing);

            //animContainer.ChangeToNewAnimation(-1, animWeight: 1, startTime: 0, clearOldLayers: true);

            var modelNames = animContainer.Skeleton.TopLevelBoneIndices.Select(b => animContainer.Skeleton.Bones[b].Name).ToList();

            var fmod = zzz_DocumentManager.CurrentDocument.Fmod;

            CurrentCutHits.Clear();
            CurrentCutOtherBlocks.Clear();
            lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
            {
                zzz_DocumentManager.CurrentDocument.Scene.Models.Clear();
            }
            foreach (var name in modelNames)
            {
                Model mdl = null;

                if (!remoModelDict.ContainsKey(name))
                {
                    PauseStreamBGM();

                    if (name.StartsWith("c"))
                    {
                        string shortName = name.Substring(0, 5);
                        mdl = zzz_DocumentManager.CurrentDocument.GameRoot.LoadCharacter(shortName, shortName)[0];
                        fmod.LoadInterrootFEV(shortName);

                        if (mdl.IS_PLAYER)
                        {
                            ViewportInteractor.InitializeCharacterModel(mdl, isRemo: true);
                        }
                    }
                    else if (name.StartsWith("o"))
                    {
                        string shortName = name.Substring(0, 5);
                        mdl = zzz_DocumentManager.CurrentDocument.GameRoot.LoadObject(shortName);
                        fmod.LoadInterrootFEV(shortName);
                    }
                    else if (name.StartsWith("m"))
                    {
                        mdl = zzz_DocumentManager.CurrentDocument.GameRoot.LoadMapPiece(AreaInt, BlockInt, 0, 0, int.Parse(name.Substring(1, 4)));
                    }
                    else if (name.StartsWith("A"))
                    {
                        int a = int.Parse(name.Substring(1, 2));
                        int b = int.Parse(name.Substring(4, 2));

                        if (b != BlockInt && !CurrentCutOtherBlocks.Contains(b))
                            CurrentCutOtherBlocks.Add(b);

                        mdl = zzz_DocumentManager.CurrentDocument.GameRoot.LoadMapPiece(a, b, 0, 0, int.Parse(name.Substring(8, 4)));
                    }
                    else if (name.StartsWith("d"))
                    {
                        // TODO
                        // Dummy entity e.g. 'd0000_0000'. Apparently just acts as a single DummyPoly?
                        mdl = zzz_DocumentManager.CurrentDocument.GameRoot.LoadCharacter("c1000", "c1000")[0];
                        mdl.RemoDummyTransformPrim =
                            new DebugPrimitives.DbgPrimWireArrow(new Transform(
                                Microsoft.Xna.Framework.Matrix.CreateScale(0.25f)
                                * mdl.CurrentTransform.WorldMatrix), Microsoft.Xna.Framework.Color.Lime);
                        mdl.RemoDummyTransformTextPrint = new StatusPrinter(null, Microsoft.Xna.Framework.Color.Lime);
                        mdl.RemoDummyTransformTextPrint.AppendLine(name);
                        mdl.IS_REMO_DUMMY = true;
                    }
                    else if (name.StartsWith("h"))
                    {
                        // Collision.
                        CurrentCutHits.Add(name);
                    }
                    else
                    {
                        throw new NotImplementedException($"Cannot tell what object type '{name}' is in remo HKX");
                    }

                    if (mdl != null)
                    {
                        mdl.Name = name;

                        remoModelDict.Add(name, mdl);
                    }
                }
                else
                {
                    mdl = remoModelDict[name];
                }

                if (mdl != null)
                {
                    mdl.AnimContainer = animContainer;
                    mdl.IsRemoModel = true;
                    mdl.Name = name;
                    mdl.SkeletonFlver.RevertToReferencePose();
                    mdl.SkeletonFlver.MapToOtherSkeleton(animContainer.Skeleton);
                    //mdl.UpdateSkeleton();

                    lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
                    {
                        zzz_DocumentManager.CurrentDocument.Scene.Models.Add(mdl);
                    }
                }
            }



            var msbBytes = zzz_DocumentManager.CurrentDocument.GameData.ReadFile($@"/map/MapStudio/m{AreaInt:D2}_{BlockInt:D2}_00_00.msb");

            var msb = MSB1.Read(msbBytes);

            Vector3 mapOffset = msb.Events.MapOffsets.FirstOrDefault()?.Position.ToXna() ?? Vector3.Zero;

            

            uint dg1=0, dg2=0, dg3=0, dg4=0;

            foreach (var hitName in CurrentCutHits)
            {
                var hit = msb.Parts.Collisions.FirstOrDefault(h => h.Name == hitName);
                dg1 |= hit.DrawGroups[0];
                dg2 |= hit.DrawGroups[1];
                dg3 |= hit.DrawGroups[2];
                dg4 |= hit.DrawGroups[3];
            }

            bool IsThingVisible(uint[] drawGroups)
            {
                return ((drawGroups[0] & dg1) == dg1) &&
                    ((drawGroups[1] & dg2) == dg2) &&
                    ((drawGroups[2] & dg3) == dg3) &&
                    ((drawGroups[3] & dg4) == dg4);
            }

            foreach (var mapPiece in msb.Parts.MapPieces)
            {
                var thisEntityName = CurrentCutOtherBlocks.Count > 0 ? $"A{AreaInt:D2}B{BlockInt:D2}_{mapPiece.Name}"
                        : mapPiece.Name;

                

                if (IsThingVisible(mapPiece.DrawGroups))
                {
                    Model mdl = null;

                    if (remoModelDict.ContainsKey(thisEntityName))
                    {
                        mdl = remoModelDict[thisEntityName];
                        mdl.AnimContainer = animContainer;
                        mdl.IsRemoModel = true;
                        mdl.SkeletonFlver.RevertToReferencePose();
                        mdl.SkeletonFlver.MapToOtherSkeleton(animContainer.Skeleton);
                        //mdl.UpdateSkeleton();
                        lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
                        {
                            zzz_DocumentManager.CurrentDocument.Scene.Models.Add(mdl);
                        }
                        continue;
                    }
                    
                    mdl = zzz_DocumentManager.CurrentDocument.GameRoot.LoadMapPiece(AreaInt, BlockInt, 0, 0, int.Parse(mapPiece.ModelName.Substring(1, 4)));
                    mdl.AnimContainer = animContainer;
                    mdl.IsRemoModel = true;
                    mdl.Name = thisEntityName;
                    mdl.SkeletonFlver.RevertToReferencePose();

                    mdl.StartTransform.Position = mapPiece.Position.ToXna() - mapOffset;
                    mdl.StartTransform.Rotation = Utils.EulerToQuaternion((mapPiece.Rotation * (SapMath.Pi / 180f)).ToXna());
                    mdl.StartTransform.Scale = mapPiece.Scale.ToXna();
                    mdl.CurrentTransform = mdl.StartTransform;

                    mdl.IS_REMO_NOTSKINNED = true;

                    mdl.SkeletonFlver.MapToOtherSkeleton(animContainer.Skeleton);
                    //mdl.UpdateSkeleton();

                    lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
                    {
                        zzz_DocumentManager.CurrentDocument.Scene.Models.Add(mdl);
                    }

                    remoModelDict.Add(thisEntityName, mdl);
                }
            }

            foreach (var mapPiece in msb.Parts.Objects)
            {
                var thisEntityName = CurrentCutOtherBlocks.Count > 0 ? $"A{AreaInt:D2}B{BlockInt:D2}_{mapPiece.Name}"
                        : mapPiece.Name;

                if (IsThingVisible(mapPiece.DrawGroups))
                {
                    Model mdl = null;

                    if (remoModelDict.ContainsKey(thisEntityName))
                    {
                        mdl = remoModelDict[thisEntityName];
                        mdl.AnimContainer = animContainer;
                        mdl.IsRemoModel = true;
                        mdl.SkeletonFlver.RevertToReferencePose();
                        mdl.SkeletonFlver.MapToOtherSkeleton(animContainer.Skeleton);
                        //mdl.UpdateSkeleton();
                        lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
                        {
                            zzz_DocumentManager.CurrentDocument.Scene.Models.Add(mdl);
                        }
                        continue;
                    }

                    mdl = zzz_DocumentManager.CurrentDocument.GameRoot.LoadObject(mapPiece.ModelName);
                    mdl.AnimContainer = animContainer;
                    mdl.IsRemoModel = true;
                    mdl.Name = thisEntityName;

                    mdl.StartTransform.Position = mapPiece.Position.ToXna() - mapOffset;
                    mdl.StartTransform.Rotation = Utils.EulerToQuaternion((mapPiece.Rotation * (SapMath.Pi / 180f)).ToXna());
                    mdl.StartTransform.Scale = mapPiece.Scale.ToXna();
                    mdl.CurrentTransform = mdl.StartTransform;

                    mdl.IS_REMO_NOTSKINNED = true;

                    mdl.SkeletonFlver.RevertToReferencePose();
                    mdl.SkeletonFlver.MapToOtherSkeleton(animContainer.Skeleton);

                    //mdl.UpdateSkeleton();

                    lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
                    {
                        zzz_DocumentManager.CurrentDocument.Scene.Models.Add(mdl);
                    }

                    remoModelDict.Add(thisEntityName, mdl);
                }
            }

            lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
            {
                zzz_DocumentManager.CurrentDocument.Scene.Models = zzz_DocumentManager.CurrentDocument.Scene.Models.OrderBy(m => m.IS_PLAYER ? 0 : 1).ToList();
            }
            

            CurrentCut = animName;

            //animContainer.ScrubAbsolute(0, ScrubTypes.All);

            List<Model> mdls = null;

            lock (zzz_DocumentManager.CurrentDocument.Scene._lock_ModelLoad_Draw)
            {
                mdls = zzz_DocumentManager.CurrentDocument.Scene.Models.ToList();
            }
            
            //foreach (var m in mdls)
            //{
            //    m.UpdateSkeleton();
            //}

            GFX.CurrentWorldView.Update(0);

            zzz_DocumentManager.CurrentDocument.Scene.EnableModelDrawing();
            zzz_DocumentManager.CurrentDocument.Scene.EnableModelDrawing2();

            ResumeStreamedBGM();

            ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_PLAY_RESUME_NEXT_FRAME = true;

            ViewportInteractor.Graph.MainScreen.HardReset();
        }
    }
}
