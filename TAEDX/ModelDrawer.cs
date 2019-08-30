using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace TAEDX
{
    public class ModelDrawer
    {
        private const float LINEUP_PADDING = 0.5f;

        private bool IsTextureLoadRequested = false;
        public void RequestTextureLoad() => IsTextureLoadRequested = true;

        internal static object _lock_ModelLoad_Draw = new object();
        public List<Model> Models = new List<Model>();

        public ModelInstance Selected = null;
        public bool HighlightSelectedPiece = true;
        public bool WireframeSelectedPiece = false;
        public bool GoToModelsAsTheySpawn = true;

        public Dictionary<string, Vector4> LightmapAtlasMap = new Dictionary<string, Vector4>();
        public Dictionary<string, int> LightmapAtlasIndexMap = new Dictionary<string, int>();

        //public long Debug_VertexCount = 0;
        //public long Debug_SubmeshCount = 0;

        public void ClearScene()
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

        public void InvertVisibility()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var m in Models)
                    m.IsVisible = !m.IsVisible;
            }
        }

        public void HideAll()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var m in Models)
                    m.IsVisible = false;
            }
        }

        public void ShowAll()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var m in Models)
                    m.IsVisible = true;
            }
        }

        public void AddModelInstance(Model model, string name, Transform location)
        {
            lock (_lock_ModelLoad_Draw)
            {
                if (!Models.Contains(model))
                    Models.Add(model);

                var instance = new ModelInstance(name, model, location, -1, -1, -1, -1);
                //if (LightmapAtlasMap.ContainsKey(name))
                //{
                //    instance.Data.atlasScale = new Vector2(LightmapAtlasMap[name].X, LightmapAtlasMap[name].Y);
                //    instance.Data.atlasOffset = new Vector2(LightmapAtlasMap[name].Z, LightmapAtlasMap[name].W);
                //}
                model.AddNewInstance(instance);
            }

        }

        private void ModelLineup(List<int> ids, Func<int, List<Model>> loadModels, IProgress<double> prog)
        {
            float currentX = 0;

            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                List<Model> models = loadModels(id);
                GFX.ModelDrawer.RequestTextureLoad();

                for (int j = 0; j < models.Count; j++)
                {
                    Model model = models[j];
                    // Offset to align model
                    Transform transform = new Transform(currentX - model.Bounds.Min.X, 0, 0, 0, 0, 0);
                    AddModelInstance(model, $"c{id:D4}{(j > 0 ? $"[{j + 1}]" : "")}", transform);
                    // Model width + padding
                    currentX += model.Bounds.Max.X - model.Bounds.Min.X + LINEUP_PADDING;

                    if (GoToModelsAsTheySpawn)
                    {
                        GFX.World.GoToTransformAndLookAtIt(
                            new Transform(transform.Position + model.Bounds.GetCenter() - new Vector3(model.Bounds.Min.X, 0, 0), transform.EulerRotation),
                            (model.Bounds.Max - model.Bounds.Min).Length() * 1.5f);
                    }
                }

                prog?.Report(1.0 * i / ids.Count);
            }
        }

        //public void TestAddAllChr()
        //{
        //    LoadingTaskMan.DoLoadingTask($"{nameof(TestAddAllChr)}", "Loading lineup of all characters...", prog =>
        //    {
        //        ModelLineup(DbgMenus.DbgMenuItemSpawnChr.IDList, InterrootLoader.LoadModelChr, prog);
        //        if (InterrootLoader.Type == InterrootLoader.InterrootType.InterrootDS1)
        //            TexturePool.AddAllExternalDS1TexturesInBackground();
        //    });
        //}

        //public void TestAddAllObj()
        //{
        //    LoadingTaskMan.DoLoadingTask($"{nameof(TestAddAllObj)}", "Loading lineup of all objects...", prog =>
        //    {
        //        ModelLineup(DbgMenus.DbgMenuItemSpawnObj.IDList, InterrootLoader.LoadModelObj, prog);
        //        if (InterrootLoader.Type == InterrootLoader.InterrootType.InterrootDS1)
        //            TexturePool.AddAllExternalDS1TexturesInBackground();
        //    });
        //}

        //public List<Model> AddChr(int id, Transform location)
        //{
        //    var models = InterrootLoader.LoadModelChr(id);

        //    var returnedModelInstances = new List<Model>();

        //    for (int i = 0; i < models.Count; i++)
        //    {
        //        AddModelInstance(models[i], $"c{id:D4}{(i > 0 ? $"[{i + 1}]" : "")}", location);
        //        returnedModelInstances.Add(models[i]);
        //    }

        //    GFX.ModelDrawer.RequestTextureLoad();

        //    return returnedModelInstances;
        //}

        //public List<Model> AddObj(int id, Transform location)
        //{
        //    var models = InterrootLoader.LoadModelObj(id);

        //    var returnedModelInstances = new List<Model>();

        //    for (int i = 0; i < models.Count; i++)
        //    {
        //        if (InterrootLoader.Type == InterrootLoader.InterrootType.InterrootDS3)
        //            AddModelInstance(models[i], $"o{id:D6}{(i > 0 ? $"[{i + 1}]" : "")}", location);
        //        else
        //            AddModelInstance(models[i], $"o{id:D4}{(i > 0 ? $"[{i + 1}]" : "")}", location);

        //        returnedModelInstances.Add(models[i]);
        //    }

        //    GFX.ModelDrawer.RequestTextureLoad();

        //    return returnedModelInstances;
        //}

        //public void AddMap(string mapName, bool excludeScenery)
        //{
        //    SoulsFormats.BTAB btab = InterrootLoader.LoadMapBtab(mapName);
        //    LightmapAtlasMap = new Dictionary<string, Vector4>();
        //    LightmapAtlasIndexMap = new Dictionary<string, int>();
        //    if (btab != null)
        //    {
        //        foreach (var entry in btab.Entries)
        //        {
        //            if (!LightmapAtlasMap.ContainsKey(entry.MSBPartName))
        //            {
        //                LightmapAtlasMap.Add(entry.MSBPartName, new Vector4(entry.AtlasScale.X, entry.AtlasScale.Y, entry.AtlasOffset.X, entry.AtlasOffset.Y));
        //                LightmapAtlasIndexMap.Add(entry.MSBPartName, entry.AtlasIndex);
        //            }
        //        }
        //    }
        //    InterrootLoader.LoadMapInBackground(mapName, excludeScenery, AddModelInstance);
        //}

        //public void AddMapCollision(string mapName, bool excludeScenery)
        //{
        //    InterrootLoader.LoadCollisionInBackground(mapName, excludeScenery, AddModelInstance);
        //}

        //private void DrawFlverAt(Model flver, Transform transform)
        //{
        //    GFX.World.ApplyViewToShader(GFX.FlverShader, transform);
        //    flver.Draw(transform);
        //}

        //public void DrawSpecific(int index)
        //{
        //    DrawFlverAt(ModelInstanceList[index].Model, ModelInstanceList[index].Transform);
        //}

        public void Draw()
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

        public void DrawSelected()
        {
            lock (_lock_ModelLoad_Draw)
            {
                if (Selected != null && (HighlightSelectedPiece || WireframeSelectedPiece))
                {
                    if (Selected.ModelReference == null)
                    {
                        Selected = null;
                        return;
                    }

                    //GFX.World.ApplyViewToShader(GFX.DbgPrimShader, Selected.Transform);

                    var lod = GFX.World.GetLOD(Selected.Transform);

                    var oldWireframeSetting = GFX.Wireframe;

                    var effect = ((BasicEffect)GFX.DbgPrimWireShader.Effect);

                    if (HighlightSelectedPiece)
                    {
                        throw new NotImplementedException();
                        //GFX.Wireframe = false;

                        //effect.VertexColorEnabled = true;

                        //foreach (var submesh in Selected.ModelReference.Submeshes)
                        //    submesh.Draw(lod, GFX.DbgPrimShader, forceNoBackfaceCulling: true);
                    }

                    if (WireframeSelectedPiece)
                    {
                        throw new NotImplementedException();
                        //GFX.Wireframe = true;
                        //effect.VertexColorEnabled = false;

                        //foreach (var submesh in Selected.ModelReference.Submeshes)
                        //    submesh.Draw(lod, GFX.DbgPrimShader, forceNoBackfaceCulling: true);

                        //GFX.Wireframe = oldWireframeSetting;
                    }

                    effect.VertexColorEnabled = true;
                }
            }
        }

        public void DebugDrawAll()
        {
            lock (_lock_ModelLoad_Draw)
            {
                GFX.SpriteBatch.Begin();
                foreach (var ins in Models)
                {
                    ins.DebugDraw();
                }
                GFX.SpriteBatch.End();
            }
        }


    }
}
