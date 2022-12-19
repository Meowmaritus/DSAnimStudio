using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSAnimStudio
{
    public static class Scene
    {
        public static bool IsModelLoaded => Models.Count > 0;
        public static Model MainModel => Models.Count > 0 ? Models[0] : null;

        private static bool DO_NOT_DRAW = false;
        private static bool DO_NOT_DRAW2 = false;
        private static object _lock_DO_NOT_DRAW = new object();

        public static bool IsEmpty
        {
            get
            {
                bool result = true;
                lock (_lock_ModelLoad_Draw)
                {
                    result = Models.Count == 0;
                }
                return result;
            }
        }

        public static void ForeachModel(Action<Model> doStuff)
        {
            List<Model> mdls = null;
            lock (_lock_ModelLoad_Draw)
                mdls = Models.ToList();
            foreach (var m in mdls)
                doStuff(m);
        }

        public static bool CheckIfDrawing()
        {
            var doNotDraw = false;
            lock (_lock_DO_NOT_DRAW)
            {
                doNotDraw = DO_NOT_DRAW || DO_NOT_DRAW2;
            }
            return !doNotDraw;
        }

        public static void DisableModelDrawing()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW = true;
            }
        }

        public static void EnableModelDrawing()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW = false;
            }
        }

        public static void DisableModelDrawing2()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW2 = true;
            }
        }

        public static void EnableModelDrawing2()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW2 = false;
            }
        }

        private const float LINEUP_PADDING = 0.5f;

        private static bool IsTextureLoadRequested = false;
        public static void RequestTextureLoad() => IsTextureLoadRequested = true;

        public static void ForceTextureReloadImmediate()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var ins in Models)
                    ins.TryToLoadTextures();
            }
        }

        internal static object _lock_ModelLoad_Draw = new object();
        public static List<Model> Models = new List<Model>();

        public static void ClearScene()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var mi in Models)
                {
                    mi.Dispose();
                }

                TexturePool.Flush();
                Models?.Clear();
                GC.Collect();
            }

            RumbleCamManager.ClearAll();
        }

        public static void ClearSceneAndAddModel(Model m)
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var mi in Models)
                {
                    if (mi == m)
                        continue;
                    mi.Dispose();
                }

                TexturePool.Flush();
                Models?.Clear();
                GC.Collect();

                Models.Add(m);
            }
        }

        public static void DeleteModel(Model m)
        {
            lock (_lock_ModelLoad_Draw)
            {
                Models.Remove(m);
                m.Dispose();
                GC.Collect();
            }
        }

        public static void AddModel(Model mdl, bool doLock = true)
        {
            if (mdl == null)
                throw new ArgumentNullException();

            if (doLock)
            {
                lock (_lock_ModelLoad_Draw)
                {
                    Models.Add(mdl);
                }
            }
            else
            {
                Models.Add(mdl);
            }
           
        }

        public static void UpdateAnimation()
        {
            if (!CheckIfDrawing())
                return;

            List<Model> mdls = null;

            lock (_lock_ModelLoad_Draw)
            {
                mdls = Models.ToList();
            }

            foreach (var mdl in mdls)
            {
                mdl?.UpdateAnimation();
                if (!CheckIfDrawing())
                    return;
            }
        }

        public static void Draw()
        {
            if (DbgMenus.DbgMenuItem.MenuOpenState == DbgMenus.DbgMenuOpenState.Open && DbgMenus.DbgMenuItem.IsPauseRendering)
                return;

            if (!CheckIfDrawing())
                return;

            lock (_lock_ModelLoad_Draw)
            {
                if (IsTextureLoadRequested)
                {
                    foreach (var ins in Models)
                        ins.TryToLoadTextures();

                    IsTextureLoadRequested = false;
                }

                //var drawOrderSortedModelInstances = ModelInstanceList
                //.Where(x => x.IsVisible && GFX.World.IsInFrustum(x.Model.Bounds, x.Transform))
                //.OrderByDescending(m => GFX.World.GetDistanceSquaredFromCamera(m.Transform));

                //if (Selected != null)
                //{
                //    foreach (var ins in drawOrderSortedModelInstances)
                //    {
                //        if (Selected.DrawgroupMatch(ins))
                //            DrawFlverAt(ins.Model, ins.Transform);
                //    }
                //}
                //else
                //{
                //    foreach (var ins in drawOrderSortedModelInstances)
                //    {
                //        DrawFlverAt(ins.Model, ins.Transform);
                //    }
                //}

                var mdls = Models.ToList();
                foreach (var mdl in mdls)
                {
                    //mdl.UpdateSkeleton();
                    if (!CheckIfDrawing())
                        return;
                }

                GFX.CurrentWorldView.Update(0);

                if (GFX.HideFLVERs)
                    return;

                foreach (var mdl in mdls)
                {
                    mdl.Draw();
                    if (!CheckIfDrawing())
                        return;
                }

            }
        }

    }
}
