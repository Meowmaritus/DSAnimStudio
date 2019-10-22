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

        public static void AddModel(Model mdl)
        {
            lock (_lock_ModelLoad_Draw)
            {
                Models.Add(mdl);
            }
        }

        public static void UpdateAnimation()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var mdl in Models)
                {
                    mdl?.UpdateAnimation();
                }
            }
        }

        public static void Draw()
        {
            if (DbgMenus.DbgMenuItem.MenuOpenState == DbgMenus.DbgMenuOpenState.Open && DbgMenus.DbgMenuItem.IsPauseRendering)
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

                foreach (var mdl in Models)
                {
                    mdl.Draw();
                }

            }
        }

    }
}
