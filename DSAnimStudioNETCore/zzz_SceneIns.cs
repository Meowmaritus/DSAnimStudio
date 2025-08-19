using DSAnimStudio.DebugPrimitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class zzz_SceneIns
    {
        public zzz_DocumentIns ParentDocument;
        public zzz_SceneIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }







        

        public bool IsModelLoaded => Models.Count > 0;

        private object _lock_MaterialSoloVisible = new object();
        private Model MaterialSoloVisible_Model;
        private NewMesh MaterialSoloVisible_Mesh;
        private FlverMaterial MaterialSoloVisible_Material;

        public void SetMaterialSoloVisible(Model mdl, NewMesh mesh, FlverMaterial mat)
        {
            lock (_lock_MaterialSoloVisible)
            {
                MaterialSoloVisible_Model = mdl;
                MaterialSoloVisible_Mesh = mesh;
                MaterialSoloVisible_Material = mat;
            }

        }

        public void ClearMaterialSoloVisible()
        {
            lock (_lock_MaterialSoloVisible)
            {
                MaterialSoloVisible_Mesh = null;
                MaterialSoloVisible_Material = null;
                MaterialSoloVisible_Mesh = null;
            }
        }

        public bool AnyMaterialSoloVisible()
        {
            bool any = false;
            lock (_lock_MaterialSoloVisible)
            {
                any = MaterialSoloVisible_Model != null && MaterialSoloVisible_Mesh != null && MaterialSoloVisible_Material != null;
            }
            return any;
        }

        public bool GetMaterialSoloVisible(Model mdl, NewMesh mesh, FlverMaterial mat)
        {
            bool visible = true;
            lock (_lock_MaterialSoloVisible)
            {
                if (MaterialSoloVisible_Model != null && MaterialSoloVisible_Mesh != null && MaterialSoloVisible_Material != null)
                {
                    visible = (MaterialSoloVisible_Model == mdl && MaterialSoloVisible_Mesh == mesh && MaterialSoloVisible_Material == mat);
                }
            }
            return visible;
        }

        public void TryLoadGlobalShaderConfigs()
        {
            lock (_lock_ModelLoad_Draw)
            {
                var models = Models.ToList();
                foreach (var model in models)
                {
                    model.TryLoadGlobalShaderConfigs();
                }
            }
        }

        public void SetMainModelAsDummy()
        {
            lock (_lock_ModelLoad_Draw)
            {
                var dummyMdl = Model.GetDummyModel(ParentDocument)[0];
                Models.Add(dummyMdl);
                _mainModel = dummyMdl;
            }
        }

        public void SetMainModel(Model model, bool hideNonMainModels)
        {
            _mainModel = model;
            if (hideNonMainModels && Models.Count > 0)
            {
                var mdls = Models.ToList();
                foreach (var m in mdls)
                    m.IsVisibleByUser = _mainModel == m;
            }
        }

        public void SetMainModelToFirst(bool hideNonMainModels)
        {
            SetMainModel(Models.Count > 0 ? Models[0] : null, hideNonMainModels);
        }

        public bool AccessMainModel(Action<Model> action)
        {
            bool result = false;
            lock (_lock_ModelLoad_Draw)
            {
                if (_mainModel != null)
                {
                    action(_mainModel);
                    result = true;
                }
            }
            return result;
        }

        public void AccessAllModels(Action<Model> action)
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var mdl in Models)
                {
                    action?.Invoke(mdl);
                }
            }
        }

        private Model _mainModel = null;
        public Model MainModel
        {
            get
            {
                if (!Models.Contains(_mainModel))
                    _mainModel = null;
                if (_mainModel == null)
                {
                    _mainModel = Models.Count > 0 ? Models[0] : null;
                }
                return _mainModel;
            }
        }

        private bool DO_NOT_DRAW = false;
        private bool DO_NOT_DRAW2 = false;
        private object _lock_DO_NOT_DRAW = new object();

        public bool IsEmpty
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

        public void ForeachModel(Action<Model> doStuff)
        {
            List<Model> mdls = null;
            lock (_lock_ModelLoad_Draw)
                mdls = Models.ToList();
            foreach (var m in mdls)
                doStuff(m);
        }

        public bool CheckIfDrawing()
        {
            var doNotDraw = false;
            lock (_lock_DO_NOT_DRAW)
            {
                doNotDraw = DO_NOT_DRAW || DO_NOT_DRAW2;
            }
            return !doNotDraw;
        }

        public void DisableModelDrawing()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW = true;
            }
        }

        public void EnableModelDrawing()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW = false;
            }
        }

        public void DisableModelDrawing2()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW2 = true;
            }
        }

        public void EnableModelDrawing2()
        {
            lock (_lock_DO_NOT_DRAW)
            {
                DO_NOT_DRAW2 = false;
            }
        }

        private const float LINEUP_PADDING = 0.5f;

        private bool IsTextureLoadRequested = false;
        public void RequestTextureLoad() => IsTextureLoadRequested = true;

        public void ForceTextureReloadImmediate()
        {
            lock (_lock_ModelLoad_Draw)
            {
                foreach (var ins in Models)
                    ins.TryToLoadTextures();
            }
        }

        internal object _lock_ModelLoad_Draw = new object();
        public List<Model> Models = new List<Model>();

        public void ClearScene()
        {
            lock (_lock_ModelLoad_Draw)
            {
                MainModel?.Dispose();
                // Prevent calling dispose twice.
                if (Models.Contains(MainModel))
                    Models.Remove(MainModel);

                foreach (var mi in Models)
                {
                    mi.Dispose();
                }

                ParentDocument.TexturePool.Flush();
                Models?.Clear();
            }

            ParentDocument.RumbleCamManager.ClearAll();
        }

        public void ClearSceneAndAddModel(Model m)
        {
            lock (_lock_ModelLoad_Draw)
            {
                MainModel?.Dispose();
                // Prevent calling dispose twice.
                if (Models.Contains(MainModel))
                    Models.Remove(MainModel);

                foreach (var mi in Models)
                {
                    if (mi == m)
                        continue;
                    mi.Dispose();
                }

                ParentDocument.TexturePool.Flush();
                Models?.Clear();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);

                Models.Add(m);
                if (Models.Count == 1)
                {
                    _mainModel = m;
                    m.IsVisibleByUser = true;
                }
                else
                {
                    m.IsVisibleByUser = false;
                }
            }
        }

        public void DeleteModel(Model m)
        {
            lock (_lock_ModelLoad_Draw)
            {
                Models.Remove(m);
                m.Dispose();
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);
            }
        }

        public void AddModel(Model mdl, bool doLock = true)
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

            if (Models.Count == 1)
            {
                _mainModel = mdl;
                mdl.IsVisibleByUser = true;
            }
            else
            {
                mdl.IsVisibleByUser = false;
            }

        }

        //public void UpdateAnimation()
        //{
        //    if (!CheckIfDrawing())
        //        return;

        //    List<Model> mdls = null;

        //    lock (_lock_ModelLoad_Draw)
        //    {
        //        mdls = Models.ToList();
        //    }

        //    foreach (var mdl in mdls)
        //    {
        //        mdl?.UpdateAnimation();
        //        if (!CheckIfDrawing())
        //            return;
        //    }
        //}

        public void Draw()
        {
            DBG.CalcTransformSizeMatrix(Main.HelperDraw.TransformScale, Main.HelperDraw.TransformScale_Bones, Main.HelperDraw.TransformScale_DummyPoly);

            DBG.BoostTransformVisibility_Bones = Main.HelperDraw.BoostTransformVisibility_Bones;
            DBG.BoostTransformVisibility_DummyPoly = Main.HelperDraw.BoostTransformVisibility_DummyPoly;

            DBG.BoostTransformVisibility_Bones_IgnoreAlpha = Main.HelperDraw.BoostTransformVisibility_Bones_IgnoreAlpha;
            DBG.BoostTransformVisibility_DummyPoly_IgnoreAlpha = Main.HelperDraw.BoostTransformVisibility_DummyPoly_IgnoreAlpha;

            DbgPrimSolidArrow.SetExtraVisibilityThickness(Main.HelperDraw.ExtraTransformVisibilityThickness);


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
                    mdl.Draw(0, false, false, false, null);
                    if (!CheckIfDrawing())
                        return;
                }

            }
        }
    }
}
