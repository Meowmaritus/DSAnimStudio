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
using SoulsAssetPipeline.Animation;
using System.Diagnostics;
using static DSAnimStudio.ImguiOSD.Window;
using static DSAnimStudio.NewDummyPolyManager;
using SharpDX.Direct2D1;

namespace DSAnimStudio
{
    public class Model : IDisposable, IHighlightableThing
    {
        public zzz_DocumentIns Document;

        public readonly int ModelIdx;
        
        public Vector3 OriginOffset = Vector3.Zero;
        public Vector3 OriginRotation = Vector3.Zero;
        public Vector3 OriginScale = Vector3.One;
        
        public Matrix OriginOffsetMatrix => Matrix.CreateScale(OriginScale) 
                                            * Matrix.CreateRotationZ(OriginRotation.Z) 
                                            * Matrix.CreateRotationX(OriginRotation.X) 
                                            * Matrix.CreateRotationY(OriginRotation.Y) 
                                            * Matrix.CreateTranslation(OriginOffset);
        
        public static bool DEBUG_FORCE_NO_GLOBAL_BONE_MATRIX => true;
        public bool USE_GLOBAL_BONE_MATRIX = false;

        public SplitAnimID EldenRingHandPoseAnimID = new SplitAnimID() { CategoryID = 0, SubID = 9 };
        public NewAnimSlot.Request EldenRingHandPoseAnimRequest = new NewAnimSlot.Request();

        public void SetBoneMatrixOfSubmesh(int submeshIndex, int boneMatrixIndex, Matrix m)
        {
            // if (MainMesh != null && submeshIndex >= 0 && submeshIndex < MainMesh.Submeshes.Count && boneMatrixIndex >= 0 && boneMatrixIndex < MainMesh.Submeshes[submeshIndex].BoneMatrices.Length)
            // {
            //     
            //     MainMesh.Submeshes[submeshIndex].BoneMatrices[boneMatrixIndex] = m;
            // }
            MainMesh.Submeshes[submeshIndex].BoneMatrices[boneMatrixIndex] = m;
        }
        public void SetBoneMatrixOfSubmesh_RefPose(int submeshIndex, int boneMatrixIndex, Matrix m)
        {
            MainMesh.Submeshes[submeshIndex].BoneMatrices_RefPose[boneMatrixIndex] = m;
        }

        public void SetDebugBoneWeightViewOfAllSubmeshes(int i)
        {
            foreach (var sm in MainMesh.Submeshes)
                sm.DebugViewWeightOfBoneIndex = i;
        }

        public void SetDebugBoneWeightViewOfSubmesh(int submeshIndex, int boneIndex)
        {
            MainMesh.Submeshes[submeshIndex].DebugViewWeightOfBoneIndex = boneIndex;
        }
        
        public readonly string GUID = Guid.NewGuid().ToString();
        public NewSkeletonMapper SkeletonRemapper = null;
        public NewBoneGluer BoneGluer = null;

        public NewChrAsmWpnTaeManager TaeManager_ForParts = null;

        public bool DebugDispDummyPolyTransforms = false;
        public bool DebugDispDummyPolyText = false;
        //public bool DebugDispFlverSkeletonTransforms = false;
        //public bool DebugDispFlverSkeletonLines = false;
        //public bool DebugDispFlverSkeletonBoxes = false;
        //public bool DebugDispFlverSkeletonText = false;
        //public bool DebugDispHkxSkeletonTransforms = false;
        //public bool DebugDispHkxSkeletonLines = false;
        //public bool DebugDispHkxSkeletonText = false;
        public bool DebugDispBoneGluers = false;

        //public NewAnimSkeleton_HKX HavokSkeleton => AnimContainer?.Skeleton;

        public void ForAllAC6NpcParts(Action<int, AC6NpcPartsEquipper.Part, Model> doAction)
        {
            AC6NpcParts?.AccessModelsOfAllParts(doAction);
        }


        public bool HasValidFlverSkeleton()
        {
            return SkeletonFlver != null 
                && SkeletonFlver.Bones != null 
                && SkeletonFlver.Bones.Count > 0;
        }

        public bool HasValidHavokSkeleton()
        {
            return AnimContainer != null 
                && AnimContainer.Skeleton != null
                && AnimContainer.Skeleton.Bones != null
                && AnimContainer.Skeleton.Bones.Count > 0;
        }

        public static Model[] GetDummyModel(zzz_DocumentIns doc)
        {
            var mdl = new Model(doc)
            {
                Name = "c1000",
                EnableSkinning = false,
                IsVisibleByUser = false,
                MainMesh = NewMesh.GetDummyMesh(),
                NpcParam = new ParamData.NpcParam(),
                
            };

            mdl.AnimContainer = new NewAnimationContainer(doc.Proj, mdl, doc);

            mdl.ChrAsm = new NewChrAsm(doc, mdl);
            mdl.DbgPrimDrawer = new DBG.DbgPrimDrawer(mdl);
            mdl.DummyPolyMan = new NewDummyPolyManager(mdl);

            return new Model[] { mdl };
        }

        private string _name = "Model";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (_name == null)
                {
                    Console.WriteLine("why the fuck is model name null");
                }
            }
        }

        //public FLVER2 FlverFileForDebug = null;

        public bool IsVisibleByUser = true;
        public void SetVisibleByUser(bool isVisible)
        {
            IsVisibleByUser = isVisible;
        }

        public void SetHiddenByAbsorpPos(bool hidden)
        {
            IsHiddenByAbsorpPos = hidden;
        }

        public bool IsHiddenByAbsorpPos = false;

        public bool IsHiddenByTae = false;
        public void SetHiddenByTae(bool hidden)
        {
            IsHiddenByTae = hidden;
        }

        public bool Debug_ForceShowNoMatterWhat = false;

        public bool EffectiveVisibility => (IsVisibleByUser && !IsHiddenByAbsorpPos && !IsHiddenByTae) || Debug_ForceShowNoMatterWhat;

        public float Opacity = 1;
        public float DebugOpacity = 1;

        public Vector3? GetLockonPoint(bool isAbsoluteRootMotion = false)
        {
            if (IS_REMO_DUMMY || IS_REMO_NOTSKINNED)
                return CurrentTransformPosition;

            if (isAbsoluteRootMotion)
            {
                if (DummyPolyMan.NewCheckDummyPolyExists(220))
                {
                    var mat = DummyPolyMan.NewGetDummyPolyByRefID(220)[0].CurrentMatrix;
                    return Vector3.Transform(Vector3.Zero, mat);
                }
                else
                {
                    return null;
                }
            }

            var possible = DummyPolyMan.GetDummyPosByID(220, getAbsoluteWorldPos: true);
            if (possible.Any())
                return possible.First();
            else
                return null;
        }

        public NewMesh MainMesh;
        public bool ExceedsBoneCount = false;
        
        public NewAnimSkeleton_FLVER SkeletonFlver;
        public NewAnimationContainer AnimContainer;

        public bool IsRemoModel = false;

        public NewDummyPolyManager DummyPolyMan;
        public DBG.DbgPrimDrawer DbgPrimDrawer;
        public NewChrAsm ChrAsm = null;
        public ParamData.NpcParam NpcParam = null;
        public AC6NpcPartsEquipper AC6NpcParts = null;

        public float ChrHitCapsuleRadius = 0.4f;
        public float ChrHitCapsuleHeight = 1.5f;
        public float ChrHitCapsuleYOffset = 0;

        public object _lock_NpcParams = new object();
        public List<ParamData.NpcParam> PossibleNpcParams = new List<ParamData.NpcParam>();
        public Dictionary<int, List<string>> NpcMaterialNamesPerMask = new Dictionary<int, List<string>>();
        public List<int> NpcMasksEnabledOnAllNpcParams = new List<int>();

        public DbgPrimWireCapsule ChrHitCapsulePrim = new DbgPrimWireCapsule(Color.White);

        private int _selectedNpcParamIndex = -1;
        public int SelectedNpcParamIndex
        {
            get => _selectedNpcParamIndex;
            set
            {
                lock (_lock_NpcParams)
                {
                    NpcParam = (value >= 0 && value < PossibleNpcParams.Count)
                            ? PossibleNpcParams[value] : null;
                    _selectedNpcParamIndex = value;
                    if (NpcParam != null)
                    {
                        //CurrentModel.DummyPolyMan.RecreateAllHitboxPrimitives(CurrentModel.NpcParam);
                        NpcParam.ApplyToNpcModel(Document, this);
                    }
                }
            }
        }

        public void RescanNpcParams()
        {
            lock (_lock_NpcParams)
            {
                if (!IS_PLAYER)
                {
                    PossibleNpcParams = Document.ParamManager.FindNpcParams(Name);
                    PossibleNpcParams = PossibleNpcParams.OrderBy(x => x.ID).ToList();
                    var additional = Document.ParamManager.FindNpcParams(Name, matchCXXX0: true);
                    foreach (var n in additional)
                    {
                        if (!PossibleNpcParams.Contains(n))
                            PossibleNpcParams.Add(n);
                    }

                    if (PossibleNpcParams.Count > 0)
                    {
                        SelectedNpcParamIndex = 0;
                    }
                    else if (PossibleNpcParams.Count == 0)
                    {
                        if (Document.ParamManager.NpcParam.ContainsKey(0))
                        {
                            PossibleNpcParams.Add(Document.ParamManager.NpcParam[0]);
                            SelectedNpcParamIndex = 0;
                        }
                        else
                        {
                            SelectedNpcParamIndex = -1;
                        }
                    }
                }
            }
        }

        public bool ApplyBindPose = false;

        public float BaseTrackingSpeed = 360;
        public float CurrentTrackingSpeed = 0;

        public float CharacterTrackingRotation = 0;

        public float TrackingTestInput = 0;

        public float DebugAnimWeight_Deprecated = 1;

        public bool EnableSkinning = true;
        
        public bool EnableBoneGluers = true;
        public bool EnableSkeletonMappers = true;

        public void AddRootMotion(Matrix m)
        {
            //AnimContainer.ResetRootMotion();
            //CurrentTransform = StartTransform = new Transform(CurrentTransform.WorldMatrix * m);
            AnimContainer.MoveRootMotionRelative(m.ToNewBlendableTransform());
        }

        public void UpdateTrackingTest(float elapsedTime)
        {
            try
            {
                TrackingTestInput = MathHelper.Clamp(TrackingTestInput, -1, 1);
                float delta = (MathHelper.ToRadians(CurrentTrackingSpeed)) * elapsedTime * TrackingTestInput;
                CharacterTrackingRotation += delta;

                if (Main.IsDebugBuild && float.IsNaN(CharacterTrackingRotation))
                    Console.WriteLine("Breakpoint hit");

                lock (Document.Scene._lock_ModelLoad_Draw)
                {
                    AnimContainer?.AddRelativeRootMotionRotation(delta);
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

        public void SetAnimStartTransform(Matrix m)
        {
            CurrentTransform_StartOfAnim = new Transform(m);
        }

        public Transform CurrentTransform = Transform.Default;

        public Transform CurrentTransform_StartOfAnim = Transform.Default;

        public Vector3 CurrentTransformPosition => Vector3.Transform(Vector3.Zero, CurrentTransform.WorldMatrix);

        public Vector3 CurrentTransformPosition_StartOfAnim => Vector3.Transform(Vector3.Zero, CurrentTransform_StartOfAnim.WorldMatrix);

        /// <summary>
        /// This is needed to make weapon hitboxes work.
        /// </summary>
        public bool IS_PLAYER => Document.GameRoot.IsChrPlayerChr(Name);


        public bool IS_REMO_DUMMY = false;
        public bool IS_REMO_NOTSKINNED = false;
        public DbgPrimWireArrow RemoDummyTransformPrim = null;
        public StatusPrinter RemoDummyTransformTextPrint = null;

        public enum ModelTypes
        {
            BaseModel,
            ChrAsmChildModel,
            AC6NpcEquipModel,
        }

        public bool IS_PLAYER_WEAPON = false;
        //public bool IS_CHRASM_CHILD_MODEL = false;

        public ModelTypes ModelType = ModelTypes.BaseModel;

        public Model PARENT_PLAYER_MODEL = null;


        public Model(zzz_DocumentIns doc)
        {
            Document = doc;
            ModelIdx = 0;
            
            AnimContainer = new NewAnimationContainer(doc.Proj, this, doc);

            DummyPolyMan = new NewDummyPolyManager(this);
            DbgPrimDrawer = new DBG.DbgPrimDrawer(this);

            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DefaultDrawMask[i] = DrawMask[i] = true;
            }
        }

        public Dictionary<int, List<string>> GetMaterialNamesPerMask()
        {
            Dictionary<int, List<FlverMaterial>> materialsByMask = 
                new Dictionary<int, List<FlverMaterial>>();

            if (MainMesh == null)
                return  new Dictionary<int, List<string>>();

            foreach (var mat in MainMesh.Materials)
            {
                if (!materialsByMask.ContainsKey(mat.ModelMaskIndex))
                    materialsByMask.Add(mat.ModelMaskIndex, new List<FlverMaterial>());

                if (!materialsByMask[mat.ModelMaskIndex].Contains(mat))
                    materialsByMask[mat.ModelMaskIndex].Add(mat);
            }

            var result = new Dictionary<int, List<string>>();

            foreach (var kvp in materialsByMask.OrderBy(kvp => kvp.Key))
            {
                result.Add(kvp.Key, kvp.Value.Select(sm => sm.Name).ToList());
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

        public void TryLoadGlobalShaderConfigs()
        {
            //FlverShaderConfig.ClearCache();
            MainMesh?.TryLoadGlobalShaderConfigs();
            if (ChrAsm != null)
            {

                foreach (var slot in ChrAsm.WeaponSlots)
                {
                    slot.AccessAllModels(model =>
                    {
                        model.TryLoadGlobalShaderConfigs();
                    });
                }

                ChrAsm?.ForAllArmorModels(m =>
                {
                    m.TryLoadGlobalShaderConfigs();
                });

            }
        }

        public Model(zzz_DocumentIns doc, IProgress<double> loadingProgress, string name, IBinder chrbnd, int modelIndex, 
            IBinder anibnd, IBinder texbnd = null, List<string> additionalTpfNames = null, 
            string possibleLooseTpfFolder = null, int baseDmyPolyID = 0, 
            bool ignoreStaticTransforms = false, IBinder additionalTexbnd = null,
            SoulsAssetPipeline.FLVERImporting.FLVER2Importer.ImportedFLVER2Model modelToImportDuringLoad = null,
            SapImportConfigs.ImportConfigFlver2 modelImportConfig = null, List<TPF> tpfsUsed = null,
            bool isBodyPart = false)
            : this(doc)
        {
            ModelIdx = modelIndex;
            
            AnimContainer = new NewAnimationContainer(doc.Proj, this, doc);

            Name = name;

            if (Name == null)
            {
                Console.WriteLine("why the fuck is model.Name == NULL");
            }

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

                if (modelIndex >= 0 && (f.ID % 10) != modelIndex && Document.GameRoot.GameType != SoulsAssetPipeline.SoulsGames.DS2SOTFS)
                    continue;

                var nameCheck = f.Name.ToLower();
                if (Document.GameRoot.GameType != SoulsGames.DES && flver2 == null && (nameCheck.EndsWith(".flver") || nameCheck.EndsWith(".flv") || FLVER2.Is(f.Bytes)))
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
                                if (dmy.ParentBoneIndex < 0 || dmy.ParentBoneIndex > flver2.Nodes.Count)
                                    dmy.ParentBoneIndex = 0;

                                if (dmy.AttachBoneIndex < 0 || dmy.AttachBoneIndex > flver2.Nodes.Count)
                                    dmy.AttachBoneIndex = 0;

                                // Remap bone indices.
                                dmy.ParentBoneIndex = (short)modelToImportDuringLoad.Flver.Nodes.FindIndex(b => b.Name == flver2.Nodes[dmy.ParentBoneIndex].Name);
                                dmy.AttachBoneIndex = (short)modelToImportDuringLoad.Flver.Nodes.FindIndex(b => b.Name == flver2.Nodes[dmy.AttachBoneIndex].Name);

                                // Hotfix
                                if (dmy.ParentBoneIndex == 1)
                                    dmy.ParentBoneIndex = 0;

                                modelToImportDuringLoad.Flver.Dummies.Add(dmy);
                            }

                            //var solarDmy = new FLVER.Dummy();

                            //var vanillaDmy = modelToImportDuringLoad.Flver.Dummies.FirstOrDefault(x => x.ReferenceID == 13);

                            //solarDmy.ReferenceID = 14;
                            //solarDmy.AttachBoneIndex = vanillaDmy.AttachBoneIndex;
                            //solarDmy.Forward = vanillaDmy.Forward;
                            //solarDmy.Upward = vanillaDmy.Upward;
                            //solarDmy.ParentBoneIndex = vanillaDmy.ParentBoneIndex;
                            //solarDmy.Unk30 = vanillaDmy.Unk30;
                            //solarDmy.Unk34 = vanillaDmy.Unk34;
                            //solarDmy.Position = vanillaDmy.Position;

                            //solarDmy.Position = new System.Numerics.Vector3(4.8f, solarDmy.Position.Y, -3f);

                            //solarDmy.UseUpwardVector = vanillaDmy.UseUpwardVector;
                            //solarDmy.Flag1 = vanillaDmy.Flag1;


                            //modelToImportDuringLoad.Flver.Dummies.RemoveAll(d => d.ReferenceID == 14);

                            // modelToImportDuringLoad.Flver.Dummies.Add(solarDmy);
                        }
                        flver2 = modelToImportDuringLoad.Flver;

                        f.Bytes = flver2.Write();
                    }
                }
                else if (Document.GameRoot.GameType == SoulsGames.DES && flver0 == null && (nameCheck.EndsWith(".flver") || nameCheck.EndsWith(".flv") || FLVER0.Is(f.Bytes)))
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

            //if (GameRoot.GameType is SoulsGames.AC6 && isBodyPart && anibnd == null)
            //{
            //    anibnd = BND4.Read(GameData.ReadFile("/chr/c0000.anibnd.dcx"));
            //}

            if (Document.GameRoot.GameType == SoulsGames.DES)
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
                
                    Document.LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_ANIBND", $"Loading ANIBND for {Name}...", innerProg =>
                    {
                        try
                        {
                            AnimContainer.LoadBaseANIBND(anibnd, innerProg);
                            //SkeletonFlver.MapToSkeleton(AnimContainer.Skeleton, false);
                        }
                        catch (Exception ex) when (Main.EnableErrorHandler.AnimContainer_LoadBaseANIBND)
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
                //AnimContainer = null;
                // This just messes up the model cuz they're already in 
                // reference pose, whoops
                SkeletonFlver.RevertToReferencePose();
            }

            loadingProgress?.Report(2.0 / 3.0);

            if (tpfsUsed.Count > 0)
            {
                Document.LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TPFs", $"Loading TPFs for {Name}...", innerProg =>
                {
                    for (int i = 0; i < tpfsUsed.Count; i++)
                    {
                        Document.TexturePool.AddTpf(tpfsUsed[i]);
                        MainMesh.TextureReloadQueued = true;
                        innerProg.Report(1.0 * i / tpfsUsed.Count);
                    }
                    MainMesh.TextureReloadQueued = true;
                });

            }

            loadingProgress?.Report(3.0 / 4.0);

            if (texbnd != null)
            {
                Document.LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TEXBND", $"Loading TEXBND for {Name}...", innerProg =>
                {
                    Document.TexturePool.AddTextureBnd(texbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress?.Report(3.5 / 4.0);

            if (additionalTexbnd != null)
            {
                Document.LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_AdditionalTEXBND", 
                    $"Loading extra TEXBND for {Name}...", innerProg =>
                {
                    Document.TexturePool.AddTextureBnd(additionalTexbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress?.Report(3.9 / 4.0);
            if (possibleLooseTpfFolder != null)
            {
                Document.TexturePool.AddInterrootTPFFolder(possibleLooseTpfFolder);
                MainMesh.TextureReloadQueued = true;
            }

            MainMesh.TextureReloadQueued = true;

            NewForceSyncUpdate();

            if (this == Document.Scene.MainModel)
                GFX.CurrentWorldView.RequestRecenter = true;

            Document.EditorScreen.HardReset();
            
            loadingProgress?.Report(1.0);
        }

        public void CreateChrAsm()
        {
            ChrAsm = new NewChrAsm(Document, this);
            ChrAsm.InitSkeleton(SkeletonFlver);

            MainMesh.Bounds = new BoundingBox(
                new Vector3(-0.5f, 0, -0.5f) * 1.75f, 
                new Vector3(0.5f, 1, 0.5f) * 1.75f);
        }

        private void LoadFLVER2(FLVER2 flver, bool useSecondUV, int baseDmyPolyID = 0, bool ignoreStaticTransforms = false)
        {
            SkeletonFlver = new NewAnimSkeleton_FLVER();
            SkeletonFlver.LoadFLVERSkeleton(this, flver.Nodes);
            MainMesh = new NewMesh(this, flver, useSecondUV, null, ignoreStaticTransforms);
            ExceedsBoneCount = MainMesh.ExceedsBoneCount;
            Bounds = MainMesh.Bounds;
            DummyPolyMan.AddAllDummiesFromFlver(flver);
        }

        private void LoadFLVER0(FLVER0 flver, bool useSecondUV, int baseDmyPolyID = 0, bool ignoreStaticTransforms = false)
        {
            SkeletonFlver = new NewAnimSkeleton_FLVER();
            SkeletonFlver.LoadFLVERSkeleton(this, flver.Nodes);
            MainMesh = new NewMesh(this, flver, useSecondUV, null, ignoreStaticTransforms);
            ExceedsBoneCount = MainMesh.ExceedsBoneCount;
            Bounds = MainMesh.Bounds;
            DummyPolyMan.AddAllDummiesFromFlver(flver);
        }

        public Model(zzz_DocumentIns doc, FLVER2 flver, bool useSecondUV)
            : this(doc)
        {
            AnimContainer = new NewAnimationContainer(doc.Proj, this, doc);
            LoadFLVER2(flver, useSecondUV);
        }

        public Model(zzz_DocumentIns doc, FLVER0 flver, bool useSecondUV)
            : this(doc)
        {
            AnimContainer = new NewAnimationContainer(doc.Proj, this, doc);
            LoadFLVER0(flver, useSecondUV);
        }


        //public void AfterAnimUpdate(float timeDelta, bool ignorePosWrap = false)
        //{
        //    if (IsRemoModel)
        //        CurrentTransform = new Transform(StartTransform.WorldMatrix * (AnimContainer?.Skeleton?.CurrentTransform.WorldMatrix ?? Matrix.Identity));


        //    //DEBUG TEST
        //    //if (HavokSkeleton != null)
        //    //{
        //    //    ((INewAnimSkeletonHelper)HavokSkeleton).SetAllBoneOverrideFlags(false);

        //    //    ((INewAnimSkeletonHelper)HavokSkeleton).ModifyBoneTransformFK("L_UpperArm", fk =>
        //    //    {
        //    //        fk.Translation.X = 3;
        //    //        return fk;
        //    //    }, setOverrideFlag: true);

        //    //    ((INewAnimSkeletonHelper)HavokSkeleton).ModifyBoneTransformFK("R_UpperArm", fk =>
        //    //    {
        //    //        fk.Translation.X = 3;
        //    //        return fk;
        //    //    }, setOverrideFlag: true);

        //    //}

        //    try
        //    {
        //        ChrAsm?.Update(timeDelta);
        //    }
        //    catch (Exception handled_ex) when (Main.EnableErrorHandler.NewChrAsmUpdate)
        //    {
        //        Main.HandleError(nameof(Main.EnableErrorHandler.NewChrAsmUpdate), handled_ex);
        //    }

        //    try
        //    {
        //        AC6NpcParts?.Update(timeDelta, this);
        //    }
        //    catch (Exception handled_ex) when (Main.EnableErrorHandler.AC6NpcPartsUpdate)
        //    {
        //        Main.HandleError(nameof(Main.EnableErrorHandler.AC6NpcPartsUpdate), handled_ex);
        //    }

        //    //UpdateSkeleton();

        //    DummyPolyMan.UpdateAllHitPrims();

        //    if (ChrAsm != null)
        //    {
        //        foreach (var slot in ChrAsm.WeaponSlots)
        //        {
        //            slot.AccessAllModels(model =>
        //            {
        //                model.DummyPolyMan?.UpdateAllHitPrims();
        //            });
        //        }

        //        ChrAsm?.ForAllArmorModels(m =>
        //        {
        //            m.DummyPolyMan?.UpdateAllHitPrims();
        //            //m.AfterAnimUpdate(timeDelta, ignorePosWrap);
        //        });
        //    }

        //    if (AC6NpcParts != null)
        //    {
        //        AC6NpcParts.AccessModelsOfAllParts((partIndex, part, model) =>
        //        {
        //            model.DummyPolyMan?.UpdateAllHitPrims();
        //        });
        //    }

        //    if (Main.Config.CharacterTrackingTestIsIngameTime && timeDelta != 0)
        //    {
        //        UpdateTrackingTest(timeDelta);
        //    }
        //}

        //public void ScrubAnim(bool absolute, float time, bool foreground, bool background, out float timeDelta, bool forceRefresh = false)
        //{
        //    if (!absolute && time.ApproxEquals(0) && !forceRefresh)
        //    {
        //        timeDelta = 0;
        //        return;
        //    }

        //    if (AnimContainer != null)
        //    {
        //        AnimContainer.Scrub(absolute, time, foreground, background, out timeDelta);
        //    }
        //    else
        //    {
        //        timeDelta = 0;
        //    }

        //    // if (SkeletonFlver != null)
        //    //     UpdateSkeleton();
        //    // ChrAsm?.ForAllArmorModels(m =>
        //    // {
        //    //     m.AnimContainer?.Scrub(absolute, time, foreground, background, out _, forceRefresh);
        //    //     m.DummyPolyMan.UpdateAllHitPrims();
        //    // });
        //    //AfterAnimUpdate(0);
        //}

        //public void UpdateAnimation()
        //{
        //    if (!Main.Config.CharacterTrackingTestIsIngameTime)
        //    {
        //        UpdateTrackingTest(Main.DELTA_UPDATE);
        //    }

        //    if (AnimContainer == null || AnimContainer?.Skeleton?.OriginalHavokSkeleton == null)
        //        SkeletonFlver?.RevertToReferencePose();

        //    AnimContainer?.Update();

        //    UpdateSkeleton();

        //    ChrAsm?.ForAllArmorModels(m =>
        //    {
        //        m.UpdateAnimation();
        //        m.UpdateSkeleton();
        //    });

        //    if (TaeManager_ForParts != null)
        //    {
        //        TaeManager_ForParts.UpdateTae();
        //    }

        //    //V2.0
        //    //if (AnimContainer.IsPlaying)
        //    //    AfterAnimUpdate();
        //}

        public void TryToLoadTextures()
        {
            MainMesh?.TryToLoadTextures();
            ChrAsm?.TryToLoadTextures();
        }

        public void TryToLoadTexturesFromBinder(string path)
        {
            List<string> textures = MainMesh.GetAllTexNamesToLoad();
            Document.TexturePool.AddSpecificTexturesFromBinder(path, textures);
        }

        public enum MeasureAttackDistType
        {
            Furthest = 0,
            Nearest = 1,
            Middle = 2,
        }

        public struct DrawAttackDistLineSetup
        {
            public bool OnlyZX;
            public MeasureAttackDistType MeasureDistType;
            public bool IncludeUnlockedDirections;
            public AttackDirType DirType;
            public Color MainLineColor;
            public Color MainTextColor;
            public Color SecondaryLineColor;
            public Color SecondaryTextColor;
            //public Color AngleLineColor;
            //public Color AngleTextColor;
            public bool RelativeToChrHitCapsule;
            public bool RelativeToPosAtStartOfFrame;
            public bool DrawLine;
            public bool DrawText;
            public float FloorVerticalShift;
            public bool IncludeHitboxRadius;
            public bool Accumulate;
        }

        public struct DrawAttackDistLineAccumResult
        {
            public bool FurthestInitialized;
            public Vector3 FurthestPoint;
            public Vector3 FurthestPointLockedToDir;
            public float FurthestDist;

            public bool NearestInitialized;
            public Vector3 NearestPoint;
            public Vector3 NearestPointLockedToDir;
            public float NearestDist;

           
        }

        public DbgPrimWireSphere DbgSphere_ForDrawAttackDist = new DbgPrimWireSphere(Transform.Default, Color.White);

        public DbgPrimWireArrow DbgArrow_ForDrawAttackDist = new DbgPrimWireArrow(Transform.Default, Color.White);

        private DrawAttackDistLineAccumResult DrawAttackDistLine_Inner(Vector3 measureToPoint, Vector3 measureToPointLockedToDir, 
            float measureToPointLockedDir_RawDist, DrawAttackDistLineSetup setup, DrawAttackDistLineAccumResult accum, bool showAccumOnly)
        {
            var modelCurrentTransform = CurrentTransform.WorldMatrix.ExtractTranslationAndHeading();
            var modelCurrentTransform_StartOfAnim = CurrentTransform_StartOfAnim.WorldMatrix.ExtractTranslationAndHeading();

            if (!setup.DrawLine && !setup.DrawText)
                return accum;

            if (showAccumOnly && !setup.Accumulate)
                return accum;

            if (showAccumOnly && !(setup.MeasureDistType is MeasureAttackDistType.Furthest or MeasureAttackDistType.Nearest))
                return accum;

            if (showAccumOnly && setup.MeasureDistType == MeasureAttackDistType.Furthest && !accum.FurthestInitialized)
                return accum;

            if (showAccumOnly && setup.MeasureDistType == MeasureAttackDistType.Nearest && !accum.NearestInitialized)
                return accum;

            if (setup.DirType != AttackDirType.Any)
            {
                if (DbgArrow_ForDrawAttackDist == null)
                {
                    DbgArrow_ForDrawAttackDist = new DbgPrimWireArrow(Transform.Default, Color.White);
                }
                var originOffset = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(NewDummyPolyManager.AttackDirTypeToAngle(setup.DirType))) * ChrHitCapsuleRadius;
                var originTransform = Matrix.CreateTranslation(originOffset) * modelCurrentTransform_StartOfAnim;
                DbgArrow_ForDrawAttackDist.Transform = new Transform(Matrix.CreateScale(2f) * originTransform * Matrix.CreateTranslation(0, setup.FloorVerticalShift + 0.2f, 0));
                DbgArrow_ForDrawAttackDist.OverrideColor = setup.MainLineColor;
                DbgArrow_ForDrawAttackDist.Draw(true, null, Matrix.Identity);
            }


            if (setup.Accumulate)
            {
                if (!showAccumOnly)
                {
                    if (setup.MeasureDistType == MeasureAttackDistType.Furthest && (measureToPointLockedDir_RawDist > accum.FurthestDist || !accum.FurthestInitialized))
                    {
                        accum.FurthestDist = measureToPointLockedDir_RawDist;
                        accum.FurthestPoint = measureToPoint;
                        accum.FurthestPointLockedToDir = measureToPointLockedToDir;
                        accum.FurthestInitialized = true;
                    }

                    if (setup.MeasureDistType == MeasureAttackDistType.Nearest && (measureToPointLockedDir_RawDist > accum.NearestDist || !accum.NearestInitialized))
                    {
                        accum.NearestDist = measureToPointLockedDir_RawDist;
                        accum.NearestPoint = measureToPoint;
                        accum.NearestPointLockedToDir = measureToPointLockedToDir;
                        accum.NearestInitialized = true;
                    }

                    //if (measureToPointLockedDir_RawDist < accum.NearestDist || !accum.NearestInitialized)
                    //{
                    //    accum.NearestDist = measureToPointLockedDir_RawDist;
                    //    accum.NearestPoint = measureToPoint;
                    //    accum.NearestPointLockedToDir = measureToPointLockedToDir;
                    //    accum.NearestInitialized = true;
                    //}
                }

                if (setup.MeasureDistType == MeasureAttackDistType.Furthest)
                {
                    measureToPoint = accum.FurthestPoint;
                    measureToPointLockedToDir = accum.FurthestPointLockedToDir;
                    measureToPointLockedDir_RawDist = accum.FurthestDist;
                }
                else if (setup.MeasureDistType == MeasureAttackDistType.Nearest)
                {
                    measureToPoint = accum.NearestPoint;
                    measureToPointLockedToDir = accum.NearestPointLockedToDir;
                    measureToPointLockedDir_RawDist = accum.NearestDist;
                }
            }







            //Debugging
            //DbgSphere_ForDrawAttackDist.Transform = new Transform(Matrix.CreateScale(0.1f) * modelCurrentTransform);
            //DbgSphere_ForDrawAttackDist.OverrideColor = Color.Fuchsia;
            //DbgSphere_ForDrawAttackDist.Draw(true, null, Matrix.Identity);


            //DbgSphere_ForDrawAttackDist.Transform = new Transform(Matrix.CreateScale(0.1f) * modelCurrentTransform_StartOfAnim);
            //DbgSphere_ForDrawAttackDist.OverrideColor = Color.Fuchsia;
            //DbgSphere_ForDrawAttackDist.Draw(true, null, Matrix.Identity);


            Vector3 measureToPoint_OnGround = measureToPoint * new Vector3(1, 0, 1);
            Vector3 measureToPointLockedToDir_OnGround = measureToPointLockedToDir * new Vector3(1, 0, 1);

            Matrix mFrom = !setup.RelativeToPosAtStartOfFrame ? modelCurrentTransform : (modelCurrentTransform_StartOfAnim);
            //Matrix mTo = CurrentTransform.WorldMatrix;
            Matrix mTo = Matrix.Identity;
            measureToPoint = Vector3.Transform(measureToPoint, mTo);
            measureToPointLockedToDir = Vector3.Transform(measureToPointLockedToDir, mTo);
            measureToPoint_OnGround = Vector3.Transform(measureToPoint_OnGround, mTo);
            measureToPointLockedToDir_OnGround = Vector3.Transform(measureToPointLockedToDir_OnGround, mTo);


            Vector3 measureToPointNormalized = (measureToPoint.LengthSquared() > 0) ? Vector3.Normalize(measureToPoint) : Vector3.Zero;
            Vector3 measureToPointLockedToDirNormalized = (measureToPointLockedToDir.LengthSquared() > 0) ? Vector3.Normalize(measureToPointLockedToDir) : Vector3.Zero;

            var measureFromPoint = measureToPointNormalized * (setup.RelativeToChrHitCapsule ? ChrHitCapsuleRadius : 0);
            var measureFromPointLockedToDir = measureToPointLockedToDirNormalized * (setup.RelativeToChrHitCapsule ? ChrHitCapsuleRadius : 0);

            measureFromPoint = Vector3.Transform(measureFromPoint, mFrom);
            measureFromPointLockedToDir = Vector3.Transform(measureFromPointLockedToDir, mFrom);

            measureFromPoint.Y = 0;
            measureFromPointLockedToDir.Y = 0;

            // Dumb shit
            if (setup.DirType != AttackDirType.Any)
            {
                measureFromPointLockedToDir =  Vector3.Transform(Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(NewDummyPolyManager.AttackDirTypeToAngle(setup.DirType))) * ChrHitCapsuleRadius, mFrom);
            }
           


            //measureFromPoint = Vector3.Transform(measureFromPoint, mFrom);
            //measureFromPointLockedToDir = Vector3.Transform(measureFromPointLockedToDir, mFrom);
            

            var finalMeasureToPoint = measureToPoint * new Vector3(1, setup.OnlyZX ? 0 : 1, 1);
            var finalMeasureFromPoint = measureFromPoint * new Vector3(1, setup.OnlyZX ? 0 : 1, 1);
            var finalMeasureVector = finalMeasureToPoint - finalMeasureFromPoint;

            var dist = finalMeasureVector.Length();

            if (dist != 0)
            {
                Vector3 finalMeasureVectorNormalized = (finalMeasureVector.LengthSquared() > 0) ? Vector3.Normalize(finalMeasureVector) : Vector3.Zero;
                dist *= (Vector3.Dot(finalMeasureVectorNormalized, measureToPointNormalized) < 0 ? -1 : 1);
            }


            var finalMeasureToPointLockedToDir = measureToPointLockedToDir * new Vector3(1, setup.OnlyZX ? 0 : 1, 1);
            var finalMeasureFromPointLockedToDir = measureFromPointLockedToDir * new Vector3(1, setup.OnlyZX ? 0 : 1, 1);
            var finalMeasureVectorLockedToDir = finalMeasureToPointLockedToDir - finalMeasureFromPointLockedToDir;

            var distLockedToDir = finalMeasureVectorLockedToDir.Length();

            if (distLockedToDir != 0)
            {
                Vector3 finalMeasureVectorLockedToDirNormalized = (finalMeasureVectorLockedToDir.LengthSquared() > 0) ? Vector3.Normalize(finalMeasureVectorLockedToDir) : Vector3.Zero;
                distLockedToDir *= (Vector3.Dot(finalMeasureVectorLockedToDirNormalized, measureToPointLockedToDirNormalized) < 0 ? -1 : 1);
            }

            if (setup.DirType != AttackDirType.Any)
                distLockedToDir = measureToPointLockedDir_RawDist;



            Color freeangleLineColor = setup.MainLineColor;
            Color freeangleTextColor = setup.MainTextColor;
            float freeangleLineThickness = 6;

            // Quick hotfix - move the "floor" points downward:
            measureFromPointLockedToDir.Y += setup.FloorVerticalShift;
            measureToPointLockedToDir_OnGround.Y += setup.FloorVerticalShift;
            measureFromPoint.Y += setup.FloorVerticalShift;
            measureToPoint_OnGround.Y += setup.FloorVerticalShift;

            DbgSphere_ForDrawAttackDist.Transform = new Transform(Matrix.CreateScale(0.2f) * Matrix.CreateTranslation(measureFromPointLockedToDir));
            DbgSphere_ForDrawAttackDist.OverrideColor = setup.MainLineColor;
            DbgSphere_ForDrawAttackDist.Draw(true, null, Matrix.Identity);

            if (setup.DirType != AttackDirType.Any)
            {
                freeangleLineColor = setup.SecondaryLineColor;
                freeangleTextColor = setup.SecondaryTextColor;
                freeangleLineThickness = 3;

                if (setup.DrawLine)
                {
                    if (setup.OnlyZX)
                    {
                        ImGuiDebugDrawer.DrawLine3D(measureFromPointLockedToDir, measureToPointLockedToDir, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureToPointLockedToDir, measureToPointLockedToDir_OnGround, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureFromPointLockedToDir, measureToPointLockedToDir_OnGround, setup.MainLineColor, thickness: 6 * GFX.EffectiveSSAA);
                    }
                    else
                    {
                        ImGuiDebugDrawer.DrawLine3D(measureFromPointLockedToDir, measureToPointLockedToDir, setup.MainLineColor, thickness: 6 * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureToPointLockedToDir, measureToPointLockedToDir_OnGround, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureFromPointLockedToDir, measureToPointLockedToDir_OnGround, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                    }

                    
                }

                if (setup.DrawText)
                {
                    if (setup.OnlyZX)
                        ImGuiDebugDrawer.DrawText3D($"{distLockedToDir:0.000} m", measureToPointLockedToDir_OnGround, setup.MainTextColor);
                    else
                        ImGuiDebugDrawer.DrawText3D($"{distLockedToDir:0.000} m", measureToPointLockedToDir, setup.MainTextColor);
                }
            }

            if (setup.DirType is AttackDirType.Any || setup.IncludeUnlockedDirections)
            {
                if (setup.DrawLine)
                {

                    if (setup.OnlyZX)
                    {
                        ImGuiDebugDrawer.DrawLine3D(measureFromPoint, measureToPoint, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureToPoint, measureToPoint_OnGround, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureFromPoint, measureToPoint_OnGround, freeangleLineColor, thickness: freeangleLineThickness * GFX.EffectiveSSAA);
                    }
                    else
                    {
                        ImGuiDebugDrawer.DrawLine3D(measureFromPoint, measureToPoint, freeangleLineColor, thickness: freeangleLineThickness * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureToPoint, measureToPoint_OnGround, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                        ImGuiDebugDrawer.DrawLine3D(measureFromPoint, measureToPoint_OnGround, setup.SecondaryLineColor, thickness: 3 * GFX.EffectiveSSAA);
                    }
                }

                if (setup.DrawText)
                {
                    if (setup.OnlyZX)
                        ImGuiDebugDrawer.DrawText3D($"{dist:0.000} m", measureToPoint_OnGround, freeangleTextColor);
                    else
                        ImGuiDebugDrawer.DrawText3D($"{dist:0.000} m", measureToPoint, freeangleTextColor);
                }
            }

            return accum;

        }

        public DrawAttackDistLineAccumResult AttackDistMeasureAccumulation;
        public void ResetAttackDistMeasureAccumulation()
        {
            AttackDistMeasureAccumulation = new DrawAttackDistLineAccumResult();
        }

        public void DrawAttackDistLine(DrawAttackDistLineSetup setup)
        {
            bool anyHitboxes = false;


            var compareOrigin = setup.RelativeToPosAtStartOfFrame ? CurrentTransform_StartOfAnim.WorldMatrix : CurrentTransform.WorldMatrix;
            if (setup.MeasureDistType is MeasureAttackDistType.Furthest)
            {
                if (DummyPolyMan.GetFurthestHitboxDummyPolyLocation(setup.OnlyZX, isNearest: false, 
                    CurrentTransform.WorldMatrix, compareOrigin, setup.DirType, 
                    out Vector3 furthestPoint, out Vector3 furthestPointLockedToDir, out float furthestPointLockedToDir_RawDist,
                    setup.IncludeHitboxRadius))
                {
                    AttackDistMeasureAccumulation = DrawAttackDistLine_Inner(furthestPoint, furthestPointLockedToDir, furthestPointLockedToDir_RawDist, 
                        setup, AttackDistMeasureAccumulation, showAccumOnly: false);
                    anyHitboxes = true;
                }
            }
            else if (setup.MeasureDistType is MeasureAttackDistType.Nearest)
            {
                if (DummyPolyMan.GetFurthestHitboxDummyPolyLocation(setup.OnlyZX, isNearest: true, 
                    CurrentTransform.WorldMatrix, compareOrigin, setup.DirType,
                    out Vector3 nearestPoint, out Vector3 nearestPointLockedToDir, out float nearestPointLockedToDir_RawDist,
                    setup.IncludeHitboxRadius))
                {
                    AttackDistMeasureAccumulation = DrawAttackDistLine_Inner(nearestPoint, nearestPointLockedToDir, nearestPointLockedToDir_RawDist, 
                        setup, AttackDistMeasureAccumulation, showAccumOnly: false);
                    anyHitboxes = true;
                }
            }
            else if (setup.MeasureDistType is MeasureAttackDistType.Middle)
            {
                if (DummyPolyMan.GetFurthestHitboxDummyPolyLocation(setup.OnlyZX, isNearest: false, 
                    CurrentTransform.WorldMatrix, compareOrigin, setup.DirType, 
                    out Vector3 furthestPoint, out Vector3 furthestPointLockedToDir, out float furthestPointLockedToDir_RawDist,
                    setup.IncludeHitboxRadius))
                {
                    if (DummyPolyMan.GetFurthestHitboxDummyPolyLocation(setup.OnlyZX, isNearest: true, 
                        CurrentTransform.WorldMatrix, compareOrigin, setup.DirType, 
                        out Vector3 nearestPoint, out Vector3 nearestPointLockedToDir, out float nearestPointLockedToDir_RawDist,
                    setup.IncludeHitboxRadius))
                    {
                        var point = (furthestPoint + nearestPoint) * new Vector3(0.5f, 0.5f, 0.5f);
                        var pointLockedToDir = (furthestPointLockedToDir + nearestPointLockedToDir) * new Vector3(0.5f, 0.5f, 0.5f);
                        float pointLockedToDir_RawDist = (furthestPointLockedToDir_RawDist + nearestPointLockedToDir_RawDist) / 2;


                        AttackDistMeasureAccumulation = DrawAttackDistLine_Inner(point, pointLockedToDir, pointLockedToDir_RawDist, 
                            setup, AttackDistMeasureAccumulation, showAccumOnly: false);
                        anyHitboxes = true;
                    }
                }
            }

            if (!anyHitboxes)
            {
                AttackDistMeasureAccumulation = DrawAttackDistLine_Inner(Vector3.Zero, Vector3.Zero, 0, setup, AttackDistMeasureAccumulation, showAccumOnly: true);
            }
            
        }

        //public List<DummyPolyInfo> DummyPolyDrawList = new List<DummyPolyInfo>();

        public void DrawAllPrimitiveShapes()
        {
            //DummyPolyDrawList.Clear();
            if (EffectiveVisibility)
            {
                DummyPolyMan.DrawAllHitPrims(DebugDispDummyPolyTransforms);

                var helpers = Main.HelperDraw;

                if (ModelType == ModelTypes.BaseModel)
                {

                    

                    if (helpers.EnableChrHitCapsule_StartOfAnim)
                    {

                        ChrHitCapsulePrim.OverrideColor = Main.Colors.ColorHelperChrHitCapsule * helpers.ChrHitCapsuleOpacity_StartOfAnim;
                        Vector3 a = CurrentTransformPosition_StartOfAnim + new Vector3(0, ChrHitCapsuleYOffset, 0);
                        Vector3 b = CurrentTransformPosition_StartOfAnim + new Vector3(0, ChrHitCapsuleHeight + ChrHitCapsuleYOffset, 0);

                        a.Y += ChrHitCapsuleRadius;
                        b.Y -= ChrHitCapsuleRadius;

                        ChrHitCapsulePrim.UpdateCapsuleEndPoints_Simple(a, b, ChrHitCapsuleRadius);
                        ChrHitCapsulePrim.Draw(true, null, Matrix.Identity);
                    }

                    if (helpers.EnableChrHitCapsule)
                    {
                        ChrHitCapsulePrim.OverrideColor = Main.Colors.ColorHelperChrHitCapsule * helpers.ChrHitCapsuleOpacity;
                        Vector3 a = CurrentTransformPosition + new Vector3(0, ChrHitCapsuleYOffset, 0);
                        Vector3 b = CurrentTransformPosition + new Vector3(0, ChrHitCapsuleHeight + ChrHitCapsuleYOffset, 0);

                        a.Y += ChrHitCapsuleRadius;
                        b.Y -= ChrHitCapsuleRadius;

                        ChrHitCapsulePrim.UpdateCapsuleEndPoints_Simple(a, b, ChrHitCapsuleRadius);
                        ChrHitCapsulePrim.Draw(true, null, Matrix.Identity);
                    }
                }

                if (helpers.EnableAttackDistanceLine || helpers.EnableAttackDistanceText)
                {
                    //Testing
                    var setup = new DrawAttackDistLineSetup();
                    setup.OnlyZX = true;
                    setup.MeasureDistType = MeasureAttackDistType.Furthest;
                    setup.DirType = AttackDirType.Forward;
                    setup.MainLineColor = Main.Colors.ColorHelperAttackDistanceLine.MultiplyAlpha(helpers.AttackDistanceLineOpacity);
                    setup.MainTextColor = Main.Colors.ColorHelperAttackDistanceText.MultiplyAlpha(helpers.AttackDistanceTextOpacity);
                    setup.SecondaryLineColor = Main.Colors.ColorHelperAttackDistanceLine.MultiplyAlpha(helpers.AttackDistanceLineOpacity_Secondary);
                    setup.SecondaryTextColor = Main.Colors.ColorHelperAttackDistanceText.MultiplyAlpha(helpers.AttackDistanceTextOpacity_Secondary);
                    setup.RelativeToChrHitCapsule = true;
                    setup.RelativeToPosAtStartOfFrame = true;
                    setup.DrawLine = helpers.EnableAttackDistanceLine;
                    setup.DrawText = helpers.EnableAttackDistanceText;
                    setup.IncludeUnlockedDirections = false;
                    setup.FloorVerticalShift = ChrHitCapsuleYOffset + ChrHitCapsuleHeight + 0.4f;
                    setup.IncludeHitboxRadius = true;
                    setup.Accumulate = true;
                    DrawAttackDistLine(setup);

                    //setup.MeasureDistType = MeasureAttackDistType.Middle;
                    //DrawAttackDistLine(setup);

                    setup.MeasureDistType = MeasureAttackDistType.Nearest;

                    setup.MainLineColor = Color.Cyan;
                    setup.MainTextColor = Color.Cyan;
                    setup.SecondaryLineColor = Color.Cyan.MultiplyAlpha(0.5f);
                    setup.SecondaryTextColor = Color.Cyan.MultiplyAlpha(0.5f);
                    setup.RelativeToChrHitCapsule = true;

                    setup.FloorVerticalShift -= 0.4f;

                    DrawAttackDistLine(setup);



                    //if (DummyPolyMan.GetFurthestHitboxDummyPolyLocation(onlyZX: true, ))
                    //{
                    //    asdf
                    //}

                    //if (furthestDummyPolyPoint.HasValue)
                    //{
                    //    var vectorZX = furthestDummyPolyPoint.Value * new Vector3(1, 0, 1);
                    //    var dirVec = Vector3.Normalize(vectorZX);
                    //    var distZXFromOrigin = vectorZX.Length();

                    //    float y = furthestDummyPolyPoint.Value.Y;

                    //    Vector3 a = (dirVec * ChrHitCapsuleRadius);
                    //    Vector3 b = (dirVec * distZXFromOrigin);
                    //    Vector3 c = new Vector3(b.X, y, b.Z);

                    //    a = Vector3.Transform(a, CurrentTransform.WorldMatrix);
                    //    b = Vector3.Transform(b, CurrentTransform.WorldMatrix);
                    //    c = Vector3.Transform(c, CurrentTransform.WorldMatrix);

                    //    if (helpers.AttackDistanceLine)
                    //    {
                    //        ImGuiDebugDrawer.DrawLine3D(a, b, Main.Colors.ColorHelperAttackDistanceLine * helpers.AttackDistanceLineOpacity, thickness: 1.5f);
                    //        ImGuiDebugDrawer.DrawLine3D(b, c, Main.Colors.ColorHelperAttackDistanceLine * helpers.AttackDistanceLineOpacity, thickness: 1.5f);
                    //        // Hypotenuse, dunno if i want it yet
                    //        ImGuiDebugDrawer.DrawLine3D(c, a, Main.Colors.ColorHelperAttackDistanceLine * helpers.AttackDistanceLineOpacity, thickness: 1.5f);
                    //    }

                    //    if (helpers.AttackDistanceText)
                    //        ImGuiDebugDrawer.DrawText3D($"{distZXFromOrigin:0.000} m", b, Main.Colors.ColorHelperAttackDistanceText * helpers.AttackDistanceTextOpacity);
                    //}


                }
                
               

                DbgPrimDrawer.DrawPrimitives(DebugDispDummyPolyTransforms);
                SkeletonFlver?.DrawPrimitives(this);
                AnimContainer?.Skeleton?.DrawPrimitives(this);

                if (ChrAsm != null)
                {
                    foreach (var slot in ChrAsm.WeaponSlots)
                    {
                        slot.AccessAllModels(model =>
                        {
                            model.DrawAllPrimitiveShapes();
                        });
                    }


                }
            }    

            
            
            ChrAsm?.ForAllArmorModels(m =>
            {
                //if (m.IsVisible)
                //{
                //    m.DummyPolyMan.DrawAllHitPrims();
                //    m.DbgPrimDrawer.DrawPrimitives();
                //    m.SkeletonFlver?.DrawPrimitives();
                //}
                m.DrawAllPrimitiveShapes();
            });

            //if (!IS_CHRASM_CHILD_MODEL)
            //{

            //    var camPosition = Vector3.Transform(Vector3.Zero, Document.WorldViewManager.CurrentView.CameraLocationInWorld.WorldMatrix) * new Vector3(1, 1, -1);
            //    var dmyPolyInOrder = DummyPolyDrawList.Where(dmy => dmy.Draw_IsDraw).OrderByDescending(dmy => (Vector3.Transform(Vector3.Zero, dmy.Draw_CalculatedMatrix) - camPosition).LengthSquared()).ToList();

            //    int i = 0;
            //    foreach (var dmy in dmyPolyInOrder)
            //    {
            //        dmy.DrawPrim(dmy.Draw_CalculatedMatrix, dmy.Draw_IsForce, dmy.Draw_IsForceBigger, dmy.Draw_Opacity);
            //        dmy.Draw_CalculatedOrder = i++;
            //    }
            //}

            AC6NpcParts?.AccessModelsOfAllParts((i, p, m) =>
            {
                m.DrawAllPrimitiveShapes();
            });
        }

        

        public void DrawAllPrimitiveTexts()
        {
            if (EffectiveVisibility)
            {
                DummyPolyMan.DrawAllHitPrimTexts(DebugDispDummyPolyText);

                //DbgPrimDrawer.DrawPrimitiveNames(DebugDispDummyPolyText);

                if (ChrAsm != null)
                {
                    foreach (var slot in ChrAsm.WeaponSlots)
                    {
                        slot.AccessAllModels(model =>
                        {
                            model.DrawAllPrimitiveTexts();
                        });
                    }

                    
                }
            }

            ChrAsm?.ForAllArmorModels(m =>
            {
                m.DrawAllPrimitiveTexts();
            });

            AC6NpcParts?.AccessModelsOfAllParts((i, p, m) =>
            {
                m.DrawAllPrimitiveTexts();
            });
        }

        private bool syncUpdateRequested = false;
        private object _lock_syncUpdate = new object();
        public void RequestSyncUpdate()
        {
            lock (_lock_syncUpdate)
            {
                syncUpdateRequested = true;
            }
        }

        public void NewUpdateByFixedProgramTick(float deltaTime)
        {
            if (!Main.Config.CharacterTrackingTestIsIngameTime)
            {
                UpdateTrackingTest(deltaTime);
            }

            lock (_lock_syncUpdate)
            {
                if (syncUpdateRequested)
                {
                    NewForceSyncUpdate();
                    syncUpdateRequested = false;
                }
            }
        }

        public void NewForceSyncUpdate()
        {
            NewScrubSimTime(false, 0, true, true, out _, false, true);
        }

        public void NewScrubSimTime(bool absolute, float time, bool foreground, bool background, 
            out float actualTimeDelta, bool baseSlotOnly = false, bool forceSyncUpdate = false, bool ignoreRootMotion = false)
        {
            actualTimeDelta = 0;
            //float actualDeltaTime = specificDeltaTime ?? 0;

            //if (specificDeltaTime != null)
            //{
            //    actualDeltaTime = specificDeltaTime.Value;
            //}
            //else
            //{
            //    if (updateStopwatch == null)
            //    {
            //        updateStopwatch = Stopwatch.StartNew();
            //    }
            //    else
            //    {
            //        actualDeltaTime = (float)updateStopwatch.Elapsed.TotalSeconds;
            //        updateStopwatch.Restart();
            //    }
            //}


            if (Document.GameRoot.GameType is SoulsGames.ER or SoulsGames.ERNR && ModelType == ModelTypes.BaseModel)
            {
                EldenRingHandPoseAnimRequest.TaeAnimID = EldenRingHandPoseAnimID;
                EldenRingHandPoseAnimRequest.DesiredWeight = 1;
                EldenRingHandPoseAnimRequest.EnableLoop = true;
                EldenRingHandPoseAnimRequest.ClearOnEnd = false;
                AnimContainer.NewSetSlotRequest(NewAnimSlot.SlotTypes.EldenRingHandPose, EldenRingHandPoseAnimRequest);
            }
            //else
            //{
            //    EldenRingHandPoseAnimRequest.TaeAnimID = SplitAnimID.Invalid;
            //    EldenRingHandPoseAnimRequest.ClearOnEnd = true;
            //    EldenRingHandPoseAnimRequest.EnableLoop = false;
            //    AnimContainer.NewSetSlotRequest(NewAnimSlot.SlotTypes.EldenRingHandPose, EldenRingHandPoseAnimRequest);
            //}


            if (SkeletonFlver != null)
            {
                SkeletonFlver.DebugName = $"[FLVER]{Name}";
                SkeletonFlver.EnableRefPoseMatrices = MainMesh.Submeshes.Any(x => x.UsesRefPose);
            }

            if (AnimContainer?.Skeleton?.OriginalHavokSkeleton == null && SkeletonRemapper == null)
            {
                SkeletonFlver?.RevertToReferencePose();
            }

            var animSkel = AnimContainer?.Skeleton;

            if (animSkel != null)
            {
                animSkel.DebugName = $"[HAVOK]{Name}";
            }

            if (TaeManager_ForParts != null)
            {
                TaeManager_ForParts.UpdateTae();
            }

            if (AnimContainer != null)
            {

                AnimContainer.Scrub(absolute, time, foreground, background, out float animDeltaTime, baseSlotOnly, forceSyncUpdate, ignoreRootMotion);

                if (ModelType == ModelTypes.BaseModel)
                {
                    var rootMotion = AnimContainer.GetRootMotionTransform(Main.Config.CameraFollowsRootMotionZX ? GFX.CurrentWorldView.RootMotionWrapUnit : null,
                        Main.Config.CameraFollowsRootMotionY ? GFX.CurrentWorldView.RootMotionWrapUnit : null, 
                        out System.Numerics.Vector3 translationDelta);
                    
                    CurrentTransform = StartTransform = new Transform(OriginOffsetMatrix * rootMotion.GetXnaMatrixFull());


                    if (Main.Config.CameraFollowsRootMotionZX)
                    {
                        GFX.CurrentWorldView.RootMotionFollow_Translation.Z = rootMotion.Translation.Z;
                        GFX.CurrentWorldView.RootMotionFollow_Translation.X = rootMotion.Translation.X;
                    }

                    if (Main.Config.CameraFollowsRootMotionY)
                    {
                        GFX.CurrentWorldView.RootMotionFollow_Translation.Y = rootMotion.Translation.Y;
                    }

                    float lerpSMult = animDeltaTime / (1f / 60f);

                    if (Main.Config.CameraFollowsRootMotionRotation)
                    {
                        var curRootMotion = NewBlendableTransform.FromRootMotionSample(new System.Numerics.Vector4(0, 0, 0, GFX.CurrentWorldView.RootMotionFollow_Rotation));
                        var nextRootMotion = rootMotion;
                        var lerpS = MathHelper.Clamp((1 - Main.Config.CameraFollowsRootMotionRotation_Interpolation) * lerpSMult, 0, 1);

                        if (lerpS == 1)
                            GFX.CurrentWorldView.RootMotionFollow_Rotation = NewBlendableTransform.Lerp(curRootMotion, nextRootMotion, lerpS).GetWrappedYawAngle();
                        else
                            GFX.CurrentWorldView.RootMotionFollow_Rotation = nextRootMotion.GetWrappedYawAngle();
                    }

                    //GFX.CurrentWorldView.RootMotionFollow_Translation = rootMotion.Translation;
                    //GFX.CurrentWorldView.RootMotionFollow_Translation += new Vector3(0.0001f, 0.0001f, 0.0001f);

                    if (translationDelta.LengthSquared() > 0)
                        GFX.CurrentWorldView.RegisterWorldShift(translationDelta);

                    GFX.CurrentWorldView.Update(0);
                }
                if (animDeltaTime != 0 || forceSyncUpdate)
                {
                    actualTimeDelta = animDeltaTime;

                    

                    if (SkeletonFlver != null)
                    {
                        lock (SkeletonFlver._lock_OtherSkeletonMapping)
                        {
                            if (animSkel != null && animSkel.Bones.Count > 0 &&
                                !IsRemoModel)
                            {
                                if (SkeletonFlver.OtherSkeletonThisIsMappedTo == null)
                                    SkeletonFlver.MapToOtherSkeleton(animSkel);
                            }

                            // Only do if skeleton remapper is null because skeleton remapper incorporates this functionality in it
                            if (SkeletonFlver.OtherSkeletonThisIsMappedTo != null && SkeletonRemapper == null)
                            {
                                SkeletonFlver.CopyMatricesDirectlyFromOtherSkeleton(writeToShaderMatrices: true);
                            }
                        }
                    }

                    DummyPolyMan?.UpdateAllHitPrims();

                    if (ChrAsm != null)
                    {
                        SkeletonFlver?.CalculateFKFromLocalTransforms();
                        //SkeletonFlver?.CopyToShaderMatrices();
                        ChrAsm?.UpdateAllMappersAndGluers();
                        try
                        {
                            ChrAsm?.Update(animDeltaTime, forceSyncUpdate);
                        }
                        catch (Exception handled_ex) when (Main.EnableErrorHandler.NewChrAsmUpdate)
                        {
                            Main.HandleError(nameof(Main.EnableErrorHandler.NewChrAsmUpdate), handled_ex);
                        }

                    }

                    AC6NpcParts?.UpdateAttach(this);



                    try
                    {
                        AC6NpcParts?.Update(animDeltaTime);
                    }
                    catch (Exception handled_ex) when (Main.EnableErrorHandler.AC6NpcPartsUpdate)
                    {
                        Main.HandleError(nameof(Main.EnableErrorHandler.AC6NpcPartsUpdate), handled_ex);
                    }

                    //UpdateSkeleton();

                    if (ModelType == ModelTypes.BaseModel && Main.Config.CharacterTrackingTestIsIngameTime && animDeltaTime != 0)
                    {
                        UpdateTrackingTest(animDeltaTime);
                    }
                }
            }

        }

        //public void UpdateSkeleton()
        //{
        //    if (SkeletonFlver != null)
        //    {
        //        SkeletonFlver.DebugName = $"[FLVER]{Name}";
        //        SkeletonFlver.EnableRefPoseMatrices = MainMesh.Submeshes.Any(x => x.UsesRefPose);
        //    }

        //    var animSkel = AnimContainer?.Skeleton;

        //    if (animSkel != null)
        //    {
        //        animSkel.DebugName = $"[HAVOK]{Name}";
        //    }

        //    if (SkeletonFlver != null)
        //    {
        //        lock (SkeletonFlver._lock_OtherSkeletonMapping)
        //        {
        //            if (animSkel != null && animSkel.Bones.Count > 0 &&
        //                !IsRemoModel)
        //            {
        //                if (SkeletonFlver.OtherSkeletonThisIsMappedTo == null)
        //                    SkeletonFlver.MapToOtherSkeleton(animSkel);
        //            }

        //            // Only do if skeleton remapper is null because skeleton remapper incorporates this functionality in it
        //            if (SkeletonFlver.OtherSkeletonThisIsMappedTo != null && SkeletonRemapper == null)
        //            {
        //                SkeletonFlver.CopyMatricesDirectlyFromOtherSkeleton(writeToShaderMatrices: true);
        //            }
        //        }
        //    }

        //    ChrAsm?.ForAllArmorModels(m => { m.UpdateSkeleton(); });
        //}

        
        public void DrawRemoPrims()
        {
            if (IS_REMO_DUMMY && RemoDummyTransformPrim != null)
            {
                RemoDummyTransformPrim.Transform = CurrentTransform;
                //RemoDummyTransformPrim.Name = Name;
                RemoDummyTransformPrim.Draw(false, null, Matrix.Identity);
                //RemoDummyTransformTextPrint.Clear();
                //RemoDummyTransformTextPrint.AppendLine(Name);
                RemoDummyTransformTextPrint.Position3D = CurrentTransformPosition;
                RemoDummyTransformTextPrint.Draw(out _);
            }
        }

        //private Stopwatch updateStopwatch = null;

        public void Draw(int lod, bool motionBlur, bool forceNoBackfaceCulling, bool isSkyboxLol, Model basePlayerModel)
        {
            if (_isDisposed)
                return;

            try
            {
                if (IS_REMO_DUMMY)
                {
                    return;
                }

                //if (basePlayerModel == null)
                //{
                //    var pb = Main.TAE_EDITOR?.PlaybackCursor;

                //    if (pb.Scrubbing)
                //        NewUpdate();
                //}

                // ChrAsm?.ForAllArmorModels(m =>
                // {
                //     if (m.AnimContainer?.Skeleton != null)
                //     {
                //         for (int i = 0; i < m.AnimContainer.Skeleton.Bones.Count; i++)
                //         {
                //             m.AnimContainer.Skeleton.OnHkxBoneTransformSet(i);
                //         }
                //     }
                // });

                
                
                

                


                //if (this == Scene.MainModel)
                //{
                //    var fk = (ChrAsm.BodyModel.AnimContainer.Skeleton as INewAnimSkeletonHelper).GetBoneTransformFK("Neck");
                //    DBG.DbgPrim_SoundEvent.OverrideColor = Color.Red;
                //    DBG.DbgPrim_SoundEvent.Category = DbgPrimCategory.AlwaysDraw;
                //    DBG.DbgPrim_SoundEvent.Transform = new Transform(Matrix.CreateScale(0.1f) * fk.GetXnaMatrixFull());
                //    DBG.DbgPrim_SoundEvent.Draw(null, Matrix.Identity);
                //    (ChrAsm.HeadModel.AnimContainer.Skeleton as INewAnimSkeletonHelper).SetBoneTransformFK("Neck", fk, true);
                //    (ChrAsm.HeadModel.SkeletonFlver as INewAnimSkeletonHelper).SetBoneTransformFK("Neck", fk, true);

                //    DBG.DbgPrim_SoundEvent.OverrideColor = Color.Blue;
                //    fk = (ChrAsm.HeadModel.AnimContainer.Skeleton as INewAnimSkeletonHelper).GetBoneTransformFK("Neck");
                //    DBG.DbgPrim_SoundEvent.Category = DbgPrimCategory.AlwaysDraw;
                //    DBG.DbgPrim_SoundEvent.Transform = new Transform(Matrix.CreateScale(0.1f) * fk.GetXnaMatrixFull());
                //    DBG.DbgPrim_SoundEvent.Draw(null, Matrix.Identity);


                //}

                GFX.FlverShader.Effect.Opacity = Opacity * DebugOpacity;

                GFX.CurrentWorldView.ApplyViewToShader(GFX.FlverShader, CurrentTransform);
                
                
                // if (isSkyboxLol)
                // {
                //     //((FlverShader)shader).Bones0 = new Matrix[] { Matrix.Identity };
                //     GFX.FlverShader.Effect.IsSkybox = true;
                // }
                // else if (SkeletonFlver != null)
                // {
                //     // test
                //
                //     //GFX.FlverShader.Effect.Bones0 = SkeletonFlver.ShaderMatrices0;
                //
                //     //if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray)
                //     //{
                //     //    GFX.FlverShader.Effect.Bones1 = SkeletonFlver.ShaderMatrices1;
                //
                //     //    if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 2)
                //     //    {
                //     //        GFX.FlverShader.Effect.Bones2 = SkeletonFlver.ShaderMatrices2;
                //
                //     //        if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 3)
                //     //        {
                //     //            GFX.FlverShader.Effect.Bones3 = SkeletonFlver.ShaderMatrices3;
                //
                //     //            if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 4)
                //     //            {
                //     //                GFX.FlverShader.Effect.Bones4 = SkeletonFlver.ShaderMatrices4;
                //
                //     //                if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 5)
                //     //                {
                //     //                    GFX.FlverShader.Effect.Bones5 = SkeletonFlver.ShaderMatrices5;
                //     //                }
                //     //            }
                //     //        }
                //     //    }
                //     //}
                //
                //     GFX.FlverShader.Effect.Bones0 = SkeletonFlver.ShaderMatrices0;
                //
                //     if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray)
                //     {
                //         GFX.FlverShader.Effect.Bones1 = SkeletonFlver.ShaderMatrices1;
                //
                //         if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 2)
                //         {
                //             GFX.FlverShader.Effect.Bones2 = SkeletonFlver.ShaderMatrices2;
                //
                //             if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 3)
                //             {
                //                 GFX.FlverShader.Effect.Bones3 = SkeletonFlver.ShaderMatrices3;
                //
                //                 if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 4)
                //                 {
                //                     GFX.FlverShader.Effect.Bones4 = SkeletonFlver.ShaderMatrices4;
                //
                //                     if (SkeletonFlver.Bones.Count >= FlverShader.MaxBonePerMatrixArray * 5)
                //                     {
                //                         GFX.FlverShader.Effect.Bones5 = SkeletonFlver.ShaderMatrices5;
                //                     }
                //                 }
                //             }
                //         }
                //     }
                //
                //
                //
                //
                //
                //     GFX.FlverShader.Effect.IsSkybox = false;
                // }
                // else
                // {
                //     GFX.FlverShader.Effect.Bones0 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                //     GFX.FlverShader.Effect.Bones1 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                //     GFX.FlverShader.Effect.Bones2 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                //     GFX.FlverShader.Effect.Bones3 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                //     GFX.FlverShader.Effect.Bones4 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                //     GFX.FlverShader.Effect.Bones5 = NewAnimSkeleton_FLVER.IDENTITY_MATRICES;
                // }

                GFX.FlverShader.Effect.EnableSkinning = EnableSkinning;

                GFX.FlverShader.Effect.DebugAnimWeight = DebugAnimWeight_Deprecated;


                ChrAsm?.Draw(DrawMask, lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);

                

                if (EffectiveVisibility && MainMesh != null)
                {
                    //if (SkeletonFlver != null)
                    //{
                    //    GFX.FlverShader.Effect.DebugViewWeightOfBoneIndex =
                    //        SkeletonFlver.DebugViewWeightOfBone_ImguiIndex - 1;

                    //}
                    

                    MainMesh.DrawMask = DrawMask;
                    MainMesh.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, this, SkeletonFlver,
                        onDrawFail: (ex) =>
                        {
                            //ImGuiDebugDrawer.DrawText3D($"{Name} failed to draw:\n\n{ex}", CurrentTransformPosition, Color.Red, Color.Black, 20);
                            if (!DialogManager.AnyDialogsShowing)
                            {
                                //DialogManager.DialogOK("DRAW ERROR", $"{Name} failed to draw:\n\n{ex}");
                                zzz_NotificationManagerIns.PushNotification($"{Name} failed to draw:\n\n{ex}");
                            }
                        }, basePlayerModel);

                    AC6NpcParts?.Draw();
                }

                
            }
            catch (Exception handled_ex) when (Main.EnableErrorHandler.ModelDraw)
            {
                Main.HandleError(nameof(Main.EnableErrorHandler.ModelDraw), handled_ex);
            }
        }

        private bool _isDisposed = false;
        public void Dispose()
        {
            if (!_isDisposed)
            {
                //EquipPartsSkeletonRemapper?.Dispose();

                BoneGluer?.Dispose();
                BoneGluer = null;

                DbgPrimDrawer?.Dispose();
                ChrAsm?.Dispose();

                NpcParam = null;

                if (SkeletonFlver != null)
                {
                    SkeletonFlver.MODEL = null;
                    SkeletonFlver = null;
                }

                if (AnimContainer != null)
                {
                    AnimContainer.ClearAnimationCache();
                    AnimContainer.Model_ForDebug = null;
                    AnimContainer.Document = null;
                    AnimContainer = null;
                }

                MainMesh?.Dispose();

                AC6NpcParts?.Dispose();

                Document = null;

                if (DummyPolyMan != null)
                {
                    DummyPolyMan.MODEL = null;
                    DummyPolyMan = null;
                }

                if (TaeManager_ForParts != null)
                {
                    TaeManager_ForParts.Mdl = null;
                    TaeManager_ForParts = null;
                }

                if (SkeletonRemapper != null)
                {
                    SkeletonRemapper.FollowerModel = null;
                    SkeletonRemapper.LeaderModel = null;
                    SkeletonRemapper = null;
                }

                // Do not need to dispose DummyPolyMan because it goes 
                // stores its primitives in the model's DbgPrimDrawer
                _isDisposed = true;
            }
        }
    }
}
