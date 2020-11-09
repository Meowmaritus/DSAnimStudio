using DSAnimStudio.TaeEditor;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class RemoManager
    {
        public static TaeViewportInteractor ViewportInteractor = null;

        public static NewAnimationContainer AnimContainer = null;

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



        private static Dictionary<string, RemoCutCache> remoCutsLoaded = new Dictionary<string, RemoCutCache>();
        private static Dictionary<string, byte[]> remoCutHkxDict = new Dictionary<string, byte[]>();
        public static void LoadRemoDict(TaeFileContainer fileContainer)
        {
            lock (_lock_remodict)
            {
                remoCutHkxDict.Clear();
                foreach (var hkx in fileContainer.AllHKXDict)
                {
                    remoCutHkxDict.Add(Utils.GetShortIngameFileName(hkx.Key) + ".hkx", hkx.Value);
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
            }
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
                        // Collision. SKIP.
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
