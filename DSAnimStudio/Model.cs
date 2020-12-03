using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DSAnimStudio.GFXShaders;
using FMOD;
using SoulsAssetPipeline;
using DSAnimStudio.ImguiOSD;
using DSAnimStudio.DebugPrimitives;

namespace DSAnimStudio
{
    public class Model : IDisposable
    {
        public string Name { get; set; } = "Model";

        //public FLVER2 FlverFileForDebug = null;

        public bool IsVisible { get; set; } = true;
        public void SetIsVisible(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public float Opacity = 1;

        public Vector3? GetLockonPoint()
        {
            if (IS_REMO_DUMMY || IS_REMO_NOTSKINNED)
                return CurrentTransformPosition;

            var possible = DummyPolyMan.GetDummyPosByID(220,
                            Matrix.Identity, ignoreModelTransform: false);
            if (possible.Any())
                return possible.First();
            else
                return null;
        }

        public NewMesh MainMesh;

        public NewAnimSkeleton_FLVER SkeletonFlver;
        public NewAnimationContainer AnimContainer;

        public bool IsRemoModel = false;

        public NewDummyPolyManager DummyPolyMan;
        public DBG.DbgPrimDrawer DbgPrimDrawer;
        public NewChrAsm ChrAsm = null;
        public ParamData.NpcParam NpcParam = null;

        public List<ParamData.NpcParam> PossibleNpcParams = new List<ParamData.NpcParam>();
        public Dictionary<int, string> NpcMaterialNamesPerMask = new Dictionary<int, string>();
        public List<int> NpcMasksEnabledOnAllNpcParams = new List<int>();

        private int _selectedNpcParamIndex = -1;
        public int SelectedNpcParamIndex
        {
            get => _selectedNpcParamIndex;
            set
            {
                NpcParam = (value >= 0 && value < PossibleNpcParams.Count)
                        ? PossibleNpcParams[value] : null;
                _selectedNpcParamIndex = value;
                if (NpcParam != null)
                {
                    //CurrentModel.DummyPolyMan.RecreateAllHitboxPrimitives(CurrentModel.NpcParam);
                    NpcParam.ApplyToNpcModel(this);
                }
            }
        }

        public void RescanNpcParams()
        {
            PossibleNpcParams = ParamManager.FindNpcParams(Name);
            PossibleNpcParams.AddRange(ParamManager.FindNpcParams(Name, matchCXXX0: true));

            if (PossibleNpcParams.Count > 0)
                SelectedNpcParamIndex = 0;
            else
                SelectedNpcParamIndex = -1;
        }

        public bool ApplyBindPose = false;

        public float BaseTrackingSpeed = 360;
        public float CurrentTrackingSpeed = 0;

        public float CharacterTrackingRotation = 0;

        public float TrackingTestInput = 0;

        public float DebugAnimWeight_Deprecated = 1;

        public bool EnableSkinning = true;

        public void UpdateTrackingTest(float elapsedTime)
        {
            try
            {
                TrackingTestInput = MathHelper.Clamp(TrackingTestInput, -1, 1);
                float delta = (MathHelper.ToRadians(CurrentTrackingSpeed)) * elapsedTime * TrackingTestInput;
                CharacterTrackingRotation += delta;
                AnimContainer.Skeleton.CurrentDirection += delta;
                if (AnimContainer != null)
                {
                    lock (Scene._lock_ModelLoad_Draw)
                    {
                        foreach (var anim in AnimContainer.AnimationLayers)
                        {
                            anim.ApplyExternalRotation(delta);
                        }
                    }
                }
            }
            catch
            {

            }
           
        }

        public static float GlobalTrackingInput = 0;

        public BoundingBox Bounds;

        public const int DRAW_MASK_LENGTH = 98;

        public bool[] DefaultDrawMask = new bool[DRAW_MASK_LENGTH];
        public bool[] DrawMask = new bool[DRAW_MASK_LENGTH];

        //public enum ModelType
        //{
        //    ModelTypeFlver,
        //    ModelTypeCollision,
        //};
        //ModelType Type;

        public Transform StartTransform = Transform.Default;

        

        public Transform CurrentTransform = Transform.Default;

        public Vector3 CurrentTransformPosition => Vector3.Transform(Vector3.Zero, CurrentTransform.WorldMatrix);

        /// <summary>
        /// This is needed to make weapon hitboxes work.
        /// </summary>
        public bool IS_PLAYER => Name == "c0000" || Name == "c0000_0000";

        public bool IS_REMO_DUMMY = false;
        public bool IS_REMO_NOTSKINNED = false;
        public DbgPrimWireArrow RemoDummyTransformPrim = null;
        public StatusPrinter RemoDummyTransformTextPrint = null;

        public bool IS_PLAYER_WEAPON = false;


        public Model()
        {
            AnimContainer = new NewAnimationContainer();

            DummyPolyMan = new NewDummyPolyManager(this);
            DbgPrimDrawer = new DBG.DbgPrimDrawer(this);

            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DefaultDrawMask[i] = DrawMask[i] = true;
            }
        }

        public Dictionary<int, string> GetMaterialNamesPerMask()
        {
            Dictionary<int, List<FlverSubmeshRenderer>> submeshesByMask = 
                new Dictionary<int, List<FlverSubmeshRenderer>>();

            foreach (var submesh in MainMesh.Submeshes)
            {
                if (!submeshesByMask.ContainsKey(submesh.ModelMaskIndex))
                    submeshesByMask.Add(submesh.ModelMaskIndex, new List<FlverSubmeshRenderer>());

                if (!submeshesByMask[submesh.ModelMaskIndex].Contains(submesh))
                    submeshesByMask[submesh.ModelMaskIndex].Add(submesh);
            }

            var result = new Dictionary<int, string>();

            foreach (var kvp in submeshesByMask)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");
                    sb.Append($"{kvp.Value[i].FullMaterialName }<{(MainMesh.Submeshes.IndexOf(kvp.Value[i]) + 1):D2}>");
                }
                result.Add(kvp.Key, sb.ToString());
            }

            return result;
        }

        public void ResetDrawMaskToAllVisible()
        {
            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i] = true;
            }
        }

        public void ResetDrawMaskToDefault()
        {
            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i];
            }
        }

        public Model(IProgress<double> loadingProgress, string name, IBinder chrbnd, int modelIndex, 
            IBinder anibnd, IBinder texbnd = null, List<string> additionalTpfNames = null, 
            string possibleLooseTpfFolder = null, int baseDmyPolyID = 0, 
            bool ignoreStaticTransforms = false, IBinder additionalTexbnd = null,
            SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model modelToImportDuringLoad = null,
            SapImportConfigs.ImportConfigFlver2 modelImportConfig = null, List<TPF> tpfsUsed = null)
            : this()
        {
            AnimContainer = new NewAnimationContainer();

            Name = name;
            List<BinderFile> flverFileEntries = new List<BinderFile>();

            if (tpfsUsed == null)
                tpfsUsed = new List<TPF>();

            if (additionalTpfNames != null)
            {
                foreach (var t in additionalTpfNames)
                {
                    if (File.Exists(t))
                        tpfsUsed.Add(TPF.Read(t));
                }
            }

            FLVER2 flver2 = null;
            FLVER0 flver0 = null;
            foreach (var f in chrbnd.Files)
            {
                if (TPF.Is(f.Bytes))
                {
                    var t = TPF.Read(f.Bytes);
                    if (modelToImportDuringLoad != null)
                    {
                        t.Textures.Clear();
                        foreach (var tx in modelToImportDuringLoad.Textures)
                        {
                            t.Textures.Add(tx);
                        }
                        f.Bytes = t.Write();
                    }
                    tpfsUsed.Add(t);
                }

                if ((f.ID % 10) != modelIndex && GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS2SOTFS)
                    continue;

                var nameCheck = f.Name.ToLower();
                if (GameDataManager.GameType != SoulsGames.DES && flver2 == null && (nameCheck.EndsWith(".flver") || nameCheck.EndsWith(".flv") || FLVER2.Is(f.Bytes)))
                {
                    //if (nameCheck.EndsWith($"_{modelIndex}.flver") || modelIndex == 0)
                    flver2 = FLVER2.Read(f.Bytes);



                    if (modelToImportDuringLoad != null)
                    {
                        if (modelImportConfig.KeepOriginalDummyPoly)
                        {
                            modelToImportDuringLoad.Flver.Dummies.Clear();

                            List<string> existingDummyPolyParentBoneNames = new List<string>();
                            List<string> existingDummyPolyAttachBoneNames = new List<string>();
                            for (int i = 0; i < flver2.Dummies.Count; i++)
                            {
                                var dmy = flver2.Dummies[i];

                                // Clamp original value (presumably) how the game does.
                                if (dmy.ParentBoneIndex < 0 || dmy.ParentBoneIndex > flver2.Bones.Count)
                                    dmy.ParentBoneIndex = 0;

                                if (dmy.AttachBoneIndex < 0 || dmy.AttachBoneIndex > flver2.Bones.Count)
                                    dmy.AttachBoneIndex = 0;

                                // Remap bone indices.
                                dmy.ParentBoneIndex = (short)modelToImportDuringLoad.Flver.Bones.FindIndex(b => b.Name == flver2.Bones[dmy.ParentBoneIndex].Name);
                                dmy.AttachBoneIndex = (short)modelToImportDuringLoad.Flver.Bones.FindIndex(b => b.Name == flver2.Bones[dmy.AttachBoneIndex].Name);

                                modelToImportDuringLoad.Flver.Dummies.Add(dmy);
                            }
                        }
                        flver2 = modelToImportDuringLoad.Flver;

                        f.Bytes = flver2.Write();
                    }
                }
                else if (GameDataManager.GameType == SoulsGames.DES && flver0 == null && (nameCheck.EndsWith(".flver") || nameCheck.EndsWith(".flv") || FLVER0.Is(f.Bytes)))
                {
                    //if (nameCheck.EndsWith($"_{modelIndex}.flver") || modelIndex == 0)
                    flver0 = FLVER0.Read(f.Bytes);
                }

                else if (anibnd == null && nameCheck.EndsWith(".anibnd"))
                {
                    //if (nameCheck.EndsWith($"_{modelIndex}.anibnd") || modelIndex == 0)
                    if (BND3.Is(f.Bytes))
                    {
                        anibnd = BND3.Read(f.Bytes);
                    }
                    else
                    {
                        anibnd = BND4.Read(f.Bytes);
                    }
                }
            }

            if (GameDataManager.GameType == SoulsGames.DES)
            {
                if (flver0 == null)
                    return;

                LoadFLVER0(flver0, useSecondUV: false, baseDmyPolyID, ignoreStaticTransforms);
            }
            else
            {
                if (flver2 == null)
                    return;

                LoadFLVER2(flver2, useSecondUV: false, baseDmyPolyID, ignoreStaticTransforms);
            }

           

            loadingProgress?.Report(1.0 / 4.0);

            if (anibnd != null)
            {
                
                    LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_ANIBND", $"Loading ANIBND for {Name}...", innerProg =>
                    {
                        try
                        {
                            AnimContainer.LoadBaseANIBND(anibnd, innerProg);
                            //SkeletonFlver.MapToSkeleton(AnimContainer.Skeleton, false);
                        }
                        catch
                        {
                            //DialogManager.DialogOK(null, "Failed to load animations.");
                            ErrorLog.LogWarning($"Failed to load animations for model '{Name}'.");
                            //System.Windows.Forms.MessageBox.Show("Failed to load animations.", "Error",
                            //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    });
               
            }
            else
            {
                // This just messes up the model cuz they're already in 
                // reference pose, whoops
                //Skeleton.ApplyBakedFlverReferencePose();
            }

            loadingProgress?.Report(2.0 / 3.0);

            if (tpfsUsed.Count > 0)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TPFs", $"Loading TPFs for {Name}...", innerProg =>
                {
                    for (int i = 0; i < tpfsUsed.Count; i++)
                    {
                        TexturePool.AddTpf(tpfsUsed[i]);
                        MainMesh.TextureReloadQueued = true;
                        innerProg.Report(1.0 * i / tpfsUsed.Count);
                    }
                    MainMesh.TextureReloadQueued = true;
                });

            }

            loadingProgress?.Report(3.0 / 4.0);

            if (texbnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TEXBND", $"Loading TEXBND for {Name}...", innerProg =>
                {
                    TexturePool.AddTextureBnd(texbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress?.Report(3.5 / 4.0);

            if (additionalTexbnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_AdditionalTEXBND", 
                    $"Loading extra TEXBND for {Name}...", innerProg =>
                {
                    TexturePool.AddTextureBnd(additionalTexbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress?.Report(3.9 / 4.0);
            if (possibleLooseTpfFolder != null)
            {
                TexturePool.AddInterrootTPFFolder(possibleLooseTpfFolder);
                MainMesh.TextureReloadQueued = true;
            }

            MainMesh.TextureReloadQueued = true;

            loadingProgress?.Report(1.0);
        }

        public void CreateChrAsm()
        {
            ChrAsm = new NewChrAsm(this);
            ChrAsm.InitSkeleton(SkeletonFlver);

            MainMesh.Bounds = new BoundingBox(
                new Vector3(-0.5f, 0, -0.5f) * 1.75f, 
                new Vector3(0.5f, 1, 0.5f) * 1.75f);
        }

        private void LoadFLVER2(FLVER2 flver, bool useSecondUV, int baseDmyPolyID = 0, bool ignoreStaticTransforms = false)
        {
            SkeletonFlver = new NewAnimSkeleton_FLVER(this, flver.Bones);
            MainMesh = new NewMesh(flver, useSecondUV, null, ignoreStaticTransforms);
            Bounds = MainMesh.Bounds;
            DummyPolyMan.AddAllDummiesFromFlver(flver);
        }

        private void LoadFLVER0(FLVER0 flver, bool useSecondUV, int baseDmyPolyID = 0, bool ignoreStaticTransforms = false)
        {
            SkeletonFlver = new NewAnimSkeleton_FLVER(this, flver.Bones);
            MainMesh = new NewMesh(flver, useSecondUV, null, ignoreStaticTransforms);
            Bounds = MainMesh.Bounds;
            DummyPolyMan.AddAllDummiesFromFlver(flver);
        }

        public Model(FLVER2 flver, bool useSecondUV)
            : this()
        {
            AnimContainer = new NewAnimationContainer();
            LoadFLVER2(flver, useSecondUV);
        }


        public void AfterAnimUpdate(float timeDelta, bool ignorePosWrap = false)
        {
            CurrentTransform = new Transform(StartTransform.WorldMatrix * (AnimContainer?.Skeleton?.CurrentTransform.WorldMatrix ?? Matrix.Identity));

            if (ChrAsm != null)
            {
                ChrAsm.UpdateWeaponTransforms(timeDelta);
                ChrAsm.UpdateWeaponAnimation(timeDelta);
            }

            DummyPolyMan.UpdateAllHitPrims();

            if (ChrAsm != null)
            {
                ChrAsm.RightWeaponModel0?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.RightWeaponModel1?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.RightWeaponModel2?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.RightWeaponModel3?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel0?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel1?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel2?.DummyPolyMan.UpdateAllHitPrims();
                ChrAsm.LeftWeaponModel3?.DummyPolyMan.UpdateAllHitPrims();
            }
        }

        public void UpdateAnimation()
        {
            if (AnimContainer.Skeleton.OriginalHavokSkeleton == null || AnimContainer == null)
                SkeletonFlver?.RevertToReferencePose();

            AnimContainer?.Update();
            UpdateTrackingTest(Main.DELTA_UPDATE);
            //V2.0
            //if (AnimContainer.IsPlaying)
            //    AfterAnimUpdate();
        }

        public void TryToLoadTextures()
        {
            MainMesh?.TryToLoadTextures();
            ChrAsm?.TryToLoadTextures();
        }

        public void TryToLoadTexturesFromBinder(string path)
        {
            List<string> textures = MainMesh.GetAllTexNamesToLoad();
            TexturePool.AddSpecificTexturesFromBinder(path, textures);
        }

        public void DrawAllPrimitiveShapes()
        {
            DummyPolyMan.DrawAllHitPrims();

            DbgPrimDrawer.DrawPrimitives();

            if (ChrAsm != null)
            {
                ChrAsm.RightWeaponModel0?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel0?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel0?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.RightWeaponModel1?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel1?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel1?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.RightWeaponModel2?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel2?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel2?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.RightWeaponModel3?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel3?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel3?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel0?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel0?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel0?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel1?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel1?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel1?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel2?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel2?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel2?.SkeletonFlver?.DrawPrimitives();

                ChrAsm.LeftWeaponModel3?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel3?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel3?.SkeletonFlver?.DrawPrimitives();
            }

            SkeletonFlver?.DrawPrimitives();

            if (DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone])
                AnimContainer?.Skeleton?.DrawPrimitives();
        }

        public void DrawAllPrimitiveTexts()
        {
            DummyPolyMan.DrawAllHitPrimTexts();

            DbgPrimDrawer.DrawPrimitiveNames();

            if (ChrAsm != null)
            {
                ChrAsm.RightWeaponModel0?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.RightWeaponModel1?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.RightWeaponModel2?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.RightWeaponModel3?.DummyPolyMan.DrawAllHitPrimTexts();

                ChrAsm.LeftWeaponModel0?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.LeftWeaponModel1?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.LeftWeaponModel2?.DummyPolyMan.DrawAllHitPrimTexts();
                ChrAsm.LeftWeaponModel3?.DummyPolyMan.DrawAllHitPrimTexts();
            }
        }

        public void UpdateSkeleton()
        {
            if (IS_REMO_DUMMY)
            {
                var mainMat = SkeletonFlver.HavokSkeletonThisIsMappedTo.HkxSkeleton.FirstOrDefault(b => b.Name == Name)?.CurrentHavokTransform.GetMatrix().ToXna() ?? Matrix.Identity;
                foreach (var b in SkeletonFlver.FlverSkeleton)
                {
                    b.CurrentMatrix = mainMat;
                }
                StartTransform = CurrentTransform = new Transform(mainMat);
                return;
            }

            if (AnimContainer != null && AnimContainer.Skeleton != null && AnimContainer.Skeleton.HkxSkeleton.Count > 0 && !IsRemoModel)
            {
                if (SkeletonFlver.HavokSkeletonThisIsMappedTo == null)
                    SkeletonFlver.MapToSkeleton(AnimContainer.Skeleton, IsRemoModel);
            }

            SkeletonFlver.CopyFromHavokSkeleton();
        }

        
        public void DrawRemoPrims()
        {
            if (IS_REMO_DUMMY && RemoDummyTransformPrim != null)
            {
                RemoDummyTransformPrim.Transform = CurrentTransform;
                RemoDummyTransformPrim.Name = Name;
                RemoDummyTransformPrim.Draw(null, Matrix.Identity);
                //RemoDummyTransformTextPrint.Clear();
                //RemoDummyTransformTextPrint.AppendLine(Name);
                RemoDummyTransformTextPrint.Position3D = CurrentTransformPosition;
                RemoDummyTransformTextPrint.Draw();
            }
        }

        public void Draw(int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            if (IS_REMO_DUMMY)
            {
                return;
            }

            GFX.FlverShader.Effect.Opacity = Opacity;

            GFX.World.ApplyViewToShader(GFX.FlverShader, CurrentTransform);

            if (isSkyboxLol)
            {
                //((FlverShader)shader).Bones0 = new Matrix[] { Matrix.Identity };
                GFX.FlverShader.Effect.IsSkybox = true;
            }
            else if (SkeletonFlver != null)
            {
                

                if (ChrAsm != null)
                {
                    GFX.FlverShader.Effect.Bones0 = SkeletonFlver.ShaderMatrices0;

                    if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray)
                    {
                        GFX.FlverShader.Effect.Bones1 = SkeletonFlver.ShaderMatrices1;

                        if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 2)
                        {
                            GFX.FlverShader.Effect.Bones2 = SkeletonFlver.ShaderMatrices2;

                            if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 3)
                            {
                                GFX.FlverShader.Effect.Bones3 = SkeletonFlver.ShaderMatrices3;

                                if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 4)
                                {
                                    GFX.FlverShader.Effect.Bones4 = SkeletonFlver.ShaderMatrices4;

                                    if (SkeletonFlver.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 5)
                                    {
                                        GFX.FlverShader.Effect.Bones5 = SkeletonFlver.ShaderMatrices5;
                                    }
                                }
                            }
                        }
                    }
                }

                

                GFX.FlverShader.Effect.IsSkybox = false;
            }
            else
            {
                GFX.FlverShader.Effect.Bones0 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones1 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones2 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones3 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones4 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones5 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
            }

            GFX.FlverShader.Effect.EnableSkinning = EnableSkinning;

            GFX.FlverShader.Effect.DebugAnimWeight = DebugAnimWeight_Deprecated;

            if (IsVisible && MainMesh != null)
            {
                MainMesh.DrawMask = DrawMask;
                MainMesh.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, SkeletonFlver);
            }
            
            if (ChrAsm != null)
            {
                ChrAsm.Draw(DrawMask, lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
            }
        }

        public void Dispose()
        {
            DbgPrimDrawer?.Dispose();
            ChrAsm?.Dispose();
            SkeletonFlver = null;
            AnimContainer = null;
            MainMesh?.Dispose();
            // Do not need to dispose DummyPolyMan because it goes 
            // stores its primitives in the model's DbgPrimDrawer
        }
    }
}
