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
        public static FmodManager.FmodEventUpdater StreamedBGM = null;
        public static void StartStreamedBGM()
        {
            StreamedBGM = FmodManager.PlaySE(4, (int.Parse(RemoName.Substring(3, 6)) * 1000) + 1, 
                () => GFX.World.CameraLocationInWorld.Position, null);
        }
        public static void StopStreamedBGM()
        {
            StreamedBGM.Stop(true);
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

            ViewportInteractor.Graph.MainScreen.SelectNewAnimRef(ViewportInteractor.Graph.MainScreen.SelectedTae,
                ViewportInteractor.Graph.MainScreen.SelectedTae.Animations.First());
            ViewportInteractor.Graph.MainScreen.HardReset();

            ViewportInteractor.Graph.PlaybackCursor.Scrubbing = false;
            ViewportInteractor.Graph.PlaybackCursor.IsPlaying = true;

            StartStreamedBGM();
        }

        public static TaeViewportInteractor ViewportInteractor = null;

        public static NewAnimationContainer AnimContainer = null;

        public static SIBCAM CurrentCutSibcam = null;

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
            public HKX.HKAAnimationBinding hk_binding = null;
            public HKX.HKASplineCompressedAnimation hk_anim = null;
            public HKX.HKASkeleton hk_skeleton = null;
        }

        public static bool EnableRemoCameraInViewport = true;
        public static bool EnableDummyPrims = true;


        public static NewBlendableTransform[] CurrentCutCameraMotion;
        public static float[] CurrentCutFoV;

        public static List<string> CurrentCutHits = new List<string>();
        public static List<int> CurrentCutOtherBlocks = new List<int>();

        private static Dictionary<string, RemoCutCache> remoCutsLoaded = new Dictionary<string, RemoCutCache>();
        private static Dictionary<string, byte[]> remoCutHkxDict = new Dictionary<string, byte[]>();
        private static Dictionary<string, SIBCAM> remoCutSibcamDict = new Dictionary<string, SIBCAM>();


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
            CurrentCutCameraMotion = null;
            CurrentCutFoV = null;
            GFX.World.ClearCustomCameraView_Override();
            GFX.World.ClearCustomCameraView_Additive();
            CurrentCutHits.Clear();
            CurrentCutOtherBlocks.Clear();
            DisposeAllModels();
        }



        public static void LoadRemoDict(TaeFileContainer fileContainer)
        {
            CurrentCut = null;

            lock (_lock_remodict)
            {
                remoCutHkxDict.Clear();
                foreach (var hkx in fileContainer.AllHKXDict)
                {
                    remoCutHkxDict.Add(Utils.GetShortIngameFileName(hkx.Key) + ".hkx", hkx.Value);
                }

                remoCutSibcamDict.Clear();
                foreach (var sibcam in fileContainer.AllSibcamDict)
                {
                    remoCutSibcamDict.Add(Utils.GetShortIngameFileName(sibcam.Key) + ".hkx", SIBCAM.Read(sibcam.Value));
                }

                remoCutsLoaded.Clear();
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
            foreach (var m in remoModelDict)
            {
                m.Value?.Dispose();
            }
            remoModelDict?.Clear();
            Scene.ClearScene();
            TexturePool.Flush();
            remoModelDict = new Dictionary<string, Model>();
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
                    BakeSibcam();
                }
                else
                    CurrentCutSibcam = null;
            }
        }

        
        

        private static void BakeSibcam()
        {
            int hkxFrameCount = (int)CurrentCutSibcam.NumFrames;
            CurrentCutCameraMotion = new NewBlendableTransform[hkxFrameCount];
            CurrentCutFoV = new float[hkxFrameCount];

            int lastKeyIndex = -1;
            var lastKeyValue_Motion = NewBlendableTransform.Identity;
            for (int frame = 0; frame < CurrentCutSibcam.CameraAnimation.Count; frame++)
            //foreach (var keyPos in CurrentCutSibcam.CameraAnimation)
            {
                var keyPos = CurrentCutSibcam.CameraAnimation[frame];
                //int frame = (int)keyPos.Index;

                var currentKeyValue_Motion = SibcamAnimFrameToTransform(keyPos);

                if (frame >= 0 && frame < CurrentCutCameraMotion.Length)
                    CurrentCutCameraMotion[frame] = currentKeyValue_Motion;

                // Fill in from the last keyframe to this one
                for (int f = Math.Max(lastKeyIndex + 1, 0); f <= Math.Min(frame - 1, CurrentCutCameraMotion.Length - 1); f++)
                {
                    float lerpS = 1f * (f - lastKeyIndex) / (frame - lastKeyIndex);
                    var blendFrom = lastKeyValue_Motion;
                    var blendTo = currentKeyValue_Motion;

                    var blended = NewBlendableTransform.Lerp(blendFrom, blendTo, lerpS);

                    CurrentCutCameraMotion[f] = blended;
                }
                lastKeyIndex = frame;
                lastKeyValue_Motion = currentKeyValue_Motion;
            }
            // Fill in from last key to end of animation.
            for (int f = Math.Max(lastKeyIndex + 1, 0); f <= CurrentCutCameraMotion.Length - 1; f++)
            {
                CurrentCutCameraMotion[f] = lastKeyValue_Motion;
            }



            lastKeyIndex = -1;
            float lastKeyValue_Fov = CurrentCutSibcam.InitialFoV;
            foreach (var keyPos in CurrentCutSibcam.FoVDataList)
            {
                int frame = (int)keyPos.FrameIdx;

                float currentKeyValue_Fov = keyPos.FoV;

                if (frame >= 0 && frame < CurrentCutFoV.Length)
                    CurrentCutFoV[frame] = currentKeyValue_Fov;

                // Fill in from the last keyframe to this one
                for (int f = Math.Max(lastKeyIndex + 1, 0); f <= Math.Min(frame - 1, CurrentCutFoV.Length - 1); f++)
                {
                    float lerpS = 1f * (f - lastKeyIndex) / (frame - lastKeyIndex);
                    var blendFrom = lastKeyValue_Fov;
                    var blendTo = currentKeyValue_Fov;

                    var blended = MathHelper.Lerp(blendFrom, blendTo, lerpS);

                    CurrentCutFoV[f] = blended;
                }
                lastKeyIndex = frame;
                lastKeyValue_Fov = currentKeyValue_Fov;
            }
            // Fill in from last key to end of animation.
            for (int f = Math.Max(lastKeyIndex + 1, 0); f <= CurrentCutFoV.Length - 1; f++)
            {
                CurrentCutFoV[f] = lastKeyValue_Fov;
            }
        }

        private static NewBlendableTransform SibcamAnimFrameToTransform(SIBCAM.CameraFrame f)
        {
            return new NewBlendableTransform()
            {
                Translation = f.Position * new System.Numerics.Vector3(1,1,-1),
                Scale = f.Scale,
                Rotation = Quaternion.CreateFromRotationMatrix(

                    Matrix.CreateRotationX(-(f.Rotation.X + SapMath.Pi * 0.5f)) *
                    Matrix.CreateRotationZ(f.Rotation.Z) *
                    Matrix.CreateRotationY(-f.Rotation.Y)
                    
                    
                    
                    




                    ).ToCS(),
            };
        }

        public static bool UpdateCutAdvance()
        {
            if (ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO
                            && ViewportInteractor.Graph.PlaybackCursor.IsPlaying && !ViewportInteractor.Graph.PlaybackCursor.Scrubbing)
            {
                int nextFrame = (int)Math.Round((ViewportInteractor.Graph.PlaybackCursor.CurrentTime + Main.DELTA_UPDATE) / (1f / 30f));
                int maxFrame = (int)Math.Round((RemoManager.AnimContainer?.CurrentAnimation?.Duration ?? ViewportInteractor.Graph.PlaybackCursor.MaxTime) / (1f / 30f));
                if (nextFrame >= maxFrame)
                {
                    ViewportInteractor.Graph.MainScreen.REMO_HOTFIX_REQUEST_CUT_ADVANCE = true;
                    ViewportInteractor.Graph.PlaybackCursor.IsPlaying = false;
                    return true;
                }
            }
            return false;
        }

        public static void UpdateRemoTime(float time)
        {
            // JUST FOR MATH TESTING
            //BakeSibcam();

            float frame = time / (1f / 30f);
            int frameFloor = Math.Min(Math.Max((int)Math.Floor(frame), 0), CurrentCutCameraMotion.Length - 1);
            int frameCeil = Math.Min(Math.Max((int)Math.Ceiling(frame), 0), CurrentCutCameraMotion.Length - 1);
            float frameMod = frame % 1;

            if (CurrentCutCameraMotion == null || CurrentCutFoV == null)
                return;

            var a = CurrentCutCameraMotion[frameFloor];
            var fovA = CurrentCutFoV[frameFloor];
            if (frameCeil < CurrentCutSibcam.CameraAnimation.Count)
            {
                var b = CurrentCutCameraMotion[frameCeil];
                a = NewBlendableTransform.Lerp(a, b, frameMod);

                var fovB = CurrentCutFoV[frameCeil];
                fovA = MathHelper.Lerp(fovA, fovB, frameMod);
            }

            if (EnableRemoCameraInViewport)
            {
                GFX.World.SetCustomCameraView_Override(a, fovA);
            }
            else
            {
                GFX.World.ClearCustomCameraView_Override();
            }

            

            

            RemoEventSim.Update(ViewportInteractor.Graph, time);
        }

       

        public static Model LookupModelOfEventGroup(TAE.EventGroup group)
        {
            var sb = new StringBuilder();
            if (group.GroupData is TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity entitySpecifier)
            {
                if (entitySpecifier.Block >= 0 || entitySpecifier.Area >= 0)
                {
                    sb.Append($"A{entitySpecifier.Area:D2}_{entitySpecifier.Block:D2}_");
                }

                if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.Character)
                    sb.Append($"c{entitySpecifier.CutsceneEntityIDPart1:D4}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                else if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.Object)
                    sb.Append($"o{entitySpecifier.CutsceneEntityIDPart1:D4}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                else if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.DummyNode)
                    sb.Append($"d{entitySpecifier.CutsceneEntityIDPart1:D4}_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                else if (entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.MapPiece)
                {
                    if (entitySpecifier.Block >= 0)
                        sb.Append($"m{entitySpecifier.CutsceneEntityIDPart1:D4}B{entitySpecifier.Block}");
                    else
                        sb.Append($"m{entitySpecifier.CutsceneEntityIDPart1:D4}B{RemoManager.BlockInt}");

                    if (entitySpecifier.CutsceneEntityIDPart2 > 0)
                    {
                        sb.Append($"_{entitySpecifier.CutsceneEntityIDPart2:D4}");
                    }
                }

                var mdls = Scene.Models.ToList();
                var foundModel = mdls.FirstOrDefault(m => m.Name == sb.ToString());
                if (foundModel == null && 
                    entitySpecifier.CutsceneEntityType == TAE.EventGroup.EventGroupData.ApplyToSpecificCutsceneEntity.EntityTypes.MapPiece
                    && entitySpecifier.CutsceneEntityIDPart2 == 0)
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
            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();

            HKX.HKAAnimationBinding hk_binding = null;
            HKX.HKASplineCompressedAnimation hk_anim = null;
            HKX.HKASkeleton hk_skeleton = null;

            if (remoCutsLoaded.ContainsKey(animName))
            {
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
                    hk_binding = hk_binding,
                    hk_anim = hk_anim,
                    hk_skeleton = hk_skeleton,
                });
            }

            

            var animContainer = new NewAnimationContainer();

            AnimContainer = animContainer;

            animContainer.ClearAnimations();

            animContainer.Skeleton.LoadHKXSkeleton(hk_skeleton);

            var testIdleAnimThing = new NewHavokAnimation_SplineCompressed(animName,
                animContainer.Skeleton, null, hk_binding, hk_anim, animContainer);

            animContainer.AddNewAnimation(animName, testIdleAnimThing);

            animContainer.CurrentAnimationName = animName;

            var modelNames = animContainer.Skeleton.TopLevelHkxBoneIndices.Select(b => animContainer.Skeleton.HkxSkeleton[b].Name).ToList();

            CurrentCutHits.Clear();
            CurrentCutOtherBlocks.Clear();
            Scene.Models.Clear();
            foreach (var name in modelNames)
            {
                Model mdl = null;

                if (!remoModelDict.ContainsKey(name))
                {
                    if (name.StartsWith("c"))
                    {
                        string shortName = name.Substring(0, 5);
                        mdl = GameDataManager.LoadCharacter(shortName);
                        FmodManager.LoadInterrootFEV(shortName);

                        if (mdl.IS_PLAYER)
                        {
                            ViewportInteractor.InitializeCharacterModel(mdl, isRemo: true);
                        }
                    }
                    else if (name.StartsWith("o"))
                    {
                        string shortName = name.Substring(0, 5);
                        mdl = GameDataManager.LoadObject(shortName);
                        FmodManager.LoadInterrootFEV(shortName);
                    }
                    else if (name.StartsWith("m"))
                    {
                        mdl = GameDataManager.LoadMapPiece(AreaInt, BlockInt, 0, 0, int.Parse(name.Substring(1, 4)));
                    }
                    else if (name.StartsWith("A"))
                    {
                        int a = int.Parse(name.Substring(1, 2));
                        int b = int.Parse(name.Substring(4, 2));

                        if (b != BlockInt && !CurrentCutOtherBlocks.Contains(b))
                            CurrentCutOtherBlocks.Add(b);

                        mdl = GameDataManager.LoadMapPiece(a, b, 0, 0, int.Parse(name.Substring(8, 4)));
                    }
                    else if (name.StartsWith("d"))
                    {
                        // TODO
                        // Dummy entity e.g. 'd0000_0000'. Apparently just acts as a single DummyPoly?
                        mdl = GameDataManager.LoadCharacter("c1000");
                        mdl.RemoDummyTransformPrim = 
                            new DebugPrimitives.DbgPrimWireArrow(name, new Transform(Microsoft.Xna.Framework.Matrix.CreateScale(0.25f) 
                            * mdl.CurrentTransform.WorldMatrix), Microsoft.Xna.Framework.Color.Lime)
                            {
                                Category = DebugPrimitives.DbgPrimCategory.AlwaysDraw
                            };
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
                    mdl.SkeletonFlver.MapToSkeleton(animContainer.Skeleton, isRemo: true);
                    mdl.UpdateSkeleton();

                    Scene.Models.Add(mdl);
                }
            }



            var msbName = GameDataManager.GetInterrootPath($@"map\MapStudio\m{AreaInt:D2}_{BlockInt:D2}_00_00.msb");

            var msb = MSB1.Read(msbName);

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
                        mdl.SkeletonFlver.MapToSkeleton(animContainer.Skeleton, isRemo: true);
                        mdl.UpdateSkeleton();
                        Scene.Models.Add(mdl);
                        continue;
                    }
                    
                    mdl = GameDataManager.LoadMapPiece(AreaInt, BlockInt, 0, 0, int.Parse(mapPiece.ModelName.Substring(1, 4)));
                    mdl.AnimContainer = animContainer;
                    mdl.IsRemoModel = true;
                    mdl.Name = thisEntityName;
                    mdl.SkeletonFlver.RevertToReferencePose();

                    mdl.StartTransform.Position = mapPiece.Position.ToXna() - mapOffset;
                    mdl.StartTransform.Rotation = Utils.EulerToQuaternion((mapPiece.Rotation * (SapMath.Pi / 180f)).ToXna());
                    mdl.StartTransform.Scale = mapPiece.Scale.ToXna();
                    mdl.CurrentTransform = mdl.StartTransform;

                    mdl.IS_REMO_NOTSKINNED = true;

                    mdl.SkeletonFlver.MapToSkeleton(animContainer.Skeleton, isRemo: true);
                    mdl.UpdateSkeleton();

                    

                    Scene.Models.Add(mdl);
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
                        mdl.SkeletonFlver.MapToSkeleton(animContainer.Skeleton, isRemo: true);
                        mdl.UpdateSkeleton();
                        Scene.Models.Add(mdl);
                        continue;
                    }

                    mdl = GameDataManager.LoadObject(mapPiece.ModelName);
                    mdl.AnimContainer = animContainer;
                    mdl.IsRemoModel = true;
                    mdl.Name = thisEntityName;

                    mdl.StartTransform.Position = mapPiece.Position.ToXna() - mapOffset;
                    mdl.StartTransform.Rotation = Utils.EulerToQuaternion((mapPiece.Rotation * (SapMath.Pi / 180f)).ToXna());
                    mdl.StartTransform.Scale = mapPiece.Scale.ToXna();
                    mdl.CurrentTransform = mdl.StartTransform;

                    mdl.IS_REMO_NOTSKINNED = true;

                    mdl.SkeletonFlver.RevertToReferencePose();
                    mdl.SkeletonFlver.MapToSkeleton(animContainer.Skeleton, isRemo: true);

                    mdl.UpdateSkeleton();

                    

                    Scene.Models.Add(mdl);
                    remoModelDict.Add(thisEntityName, mdl);
                }
            }

            Scene.Models = Scene.Models.OrderBy(m => m.IS_PLAYER ? 0 : 1).ToList();

            CurrentCut = animName;

            animContainer.ScrubRelative(0);

            var mdls = Scene.Models.ToList();
            foreach (var m in mdls)
            {
                m.UpdateSkeleton();
            }

            GFX.World.Update(0);

            Scene.EnableModelDrawing();
            Scene.EnableModelDrawing2();
        }
    }
}
