using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DSAnimStudio.GFXShaders;
using FMOD;

namespace DSAnimStudio
{
    public class Model : IDisposable
    {
        public string Name { get; set; } = "Model";

        public bool IsVisible { get; set; } = true;
        public void SetIsVisible(bool isVisible)
        {
            IsVisible = isVisible;
        }

        public NewMesh MainMesh;

        public NewAnimSkeleton Skeleton;
        public NewAnimationContainer AnimContainer;
        public NewDummyPolyManager DummyPolyMan;
        public DBG.DbgPrimDrawer DbgPrimDrawer;
        public NewChrAsm ChrAsm = null;
        public ParamData.NpcParam NpcParam = null;

        public bool IsStatic => Skeleton?.OriginalHavokSkeleton == null && GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS2SOTFS;

        public float BaseTrackingSpeed = 360;
        public float CurrentTrackingSpeed = 0;

        public float CharacterTrackingRotation = 0;

        public float TrackingTestInput = 0;

        public void UpdateTrackingTest(float elapsedTime)
        {
            try
            {
                TrackingTestInput = MathHelper.Clamp(TrackingTestInput, -1, 1);
                float delta = (MathHelper.ToRadians(CurrentTrackingSpeed)) * elapsedTime * TrackingTestInput;
                CharacterTrackingRotation += delta;
                CurrentDirection += delta;
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

        public Matrix CurrentRootMotionTranslation = Matrix.Identity;

        public void ResetRootMotionTranslation()
        {
            CurrentRootMotionTranslation = Matrix.Identity;
        }

        public Matrix CurrentRootMotionRotation => Matrix.CreateRotationY(CurrentDirection);

        public float CurrentDirection;

        public void ResetCurrentDirection()
        {
            CurrentDirection = 0;
        }

        public Transform CurrentTransform = Transform.Default;

        public Action<Vector3> OnRootMotionWrap = null;

        /// <summary>
        /// This is needed to make weapon hitboxes work.
        /// </summary>
        public bool IS_PLAYER = false;

        public bool IS_PLAYER_WEAPON = false;


        private Model()
        {
            AnimContainer = new NewAnimationContainer(this);

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
            bool ignoreStaticTransforms = false, IBinder additionalTexbnd = null)
            : this()
        {
            AnimContainer = new NewAnimationContainer(this);

            Name = name;
            List<BinderFile> flverFileEntries = new List<BinderFile>();

            List<TPF> tpfsUsed = new List<TPF>();

            if (additionalTpfNames != null)
            {
                foreach (var t in additionalTpfNames)
                {
                    if (File.Exists(t))
                        tpfsUsed.Add(TPF.Read(t));
                }
            }

            FLVER2 flver = null;
            foreach (var f in chrbnd.Files)
            {
                if (TPF.Is(f.Bytes))
                {
                    tpfsUsed.Add(TPF.Read(f.Bytes));
                }

                if ((f.ID % 10) != modelIndex && GameDataManager.GameType != SoulsAssetPipeline.SoulsGames.DS2SOTFS)
                    continue;

                var nameCheck = f.Name.ToLower();
                if (flver == null && (nameCheck.EndsWith(".flver") || nameCheck.EndsWith(".flv") || FLVER2.Is(f.Bytes)))
                {
                    //if (nameCheck.EndsWith($"_{modelIndex}.flver") || modelIndex == 0)
                    flver = FLVER2.Read(f.Bytes);
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

            if (flver == null)
            {
                //throw new ArgumentException("No FLVERs found within CHRBND.");
                return;
            }

            LoadFLVER2(flver, useSecondUV: false, baseDmyPolyID, ignoreStaticTransforms);

            loadingProgress.Report(1.0 / 4.0);

            if (anibnd != null)
            {
                
                    LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_ANIBND", $"Loading ANIBND for {Name}...", innerProg =>
                    {
                        try
                        {
                            AnimContainer.LoadBaseANIBND(anibnd, innerProg);
                        }
                        catch
                        {
                            System.Windows.Forms.MessageBox.Show("Failed to load animations.", "Error",
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        }
                    });
               
            }
            else
            {
                // This just messes up the model cuz they're already in 
                // reference pose, whoops
                //Skeleton.ApplyBakedFlverReferencePose();
            }

            loadingProgress.Report(2.0 / 3.0);

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

            loadingProgress.Report(3.0 / 4.0);

            if (texbnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TEXBND", $"Loading TEXBND for {Name}...", innerProg =>
                {
                    TexturePool.AddTextureBnd(texbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress.Report(3.5 / 4.0);

            if (additionalTexbnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_AdditionalTEXBND", 
                    $"Loading extra TEXBND for {Name}...", innerProg =>
                {
                    TexturePool.AddTextureBnd(additionalTexbnd, innerProg);
                    MainMesh.TextureReloadQueued = true;
                });
            }

            loadingProgress.Report(3.9 / 4.0);
            if (possibleLooseTpfFolder != null)
            {
                TexturePool.AddInterrootTPFFolder(possibleLooseTpfFolder);
                MainMesh.TextureReloadQueued = true;
            }

            MainMesh.TextureReloadQueued = true;

            loadingProgress.Report(1.0);
        }

        public void CreateChrAsm()
        {
            ChrAsm = new NewChrAsm(this);
            ChrAsm.InitSkeleton(Skeleton);

            MainMesh.Bounds = new BoundingBox(
                new Vector3(-0.5f, 0, -0.5f) * 1.75f, 
                new Vector3(0.5f, 1, 0.5f) * 1.75f);
        }

        private void LoadFLVER2(FLVER2 flver, bool useSecondUV, int baseDmyPolyID = 0, bool ignoreStaticTransforms = false)
        {
            //Type = ModelType.ModelTypeFlver;

            Skeleton = new NewAnimSkeleton(this, flver.Bones);

            MainMesh = new NewMesh(flver, useSecondUV, null, ignoreStaticTransforms);

            Bounds = MainMesh.Bounds;

            DummyPolyMan.AddAllDummiesFromFlver(flver);

            //DEBUG//
            //Console.WriteLine($"{flver.Meshes[0].DefaultBoneIndex}");
            //Console.WriteLine();
            //Console.WriteLine();
            //foreach (var mat in flver.Materials)
            //{
            //    Console.WriteLine($"{mat.Name}: {mat.MTD}");
            //}
            /////////
        }

        public Model(FLVER2 flver, bool useSecondUV)
            : this()
        {
            AnimContainer = new NewAnimationContainer(this);
            LoadFLVER2(flver, useSecondUV);
        }



        public void AfterAnimUpdate(float timeDelta, bool ignorePosWrap = false)
        {
            if (AnimContainer?.EnableRootMotion == false)
            {
                CurrentDirection = 0;
                CurrentRootMotionTranslation = Matrix.Identity;
            }

            var newTransform = new Transform(StartTransform.WorldMatrix * CurrentRootMotionRotation * CurrentRootMotionTranslation);



            // TEST: modulo world pos
            Vector3 locationWithNewTransform = Vector3.Transform(Vector3.Zero, newTransform.WorldMatrix);

            bool justWrapped = false;

            Vector3 translationDeltaToGetToMod = Vector3.Zero;

            if (!ignorePosWrap && AnimContainer?.EnableRootMotionWrap == true && (locationWithNewTransform.LengthSquared() > 100))
            {
                Vector3 locationWithNewTransform_Mod = new Vector3(locationWithNewTransform.X % 1, locationWithNewTransform.Y, locationWithNewTransform.Z % 1);
                translationDeltaToGetToMod = locationWithNewTransform_Mod - locationWithNewTransform;
                CurrentRootMotionTranslation *= Matrix.CreateTranslation(translationDeltaToGetToMod);

                justWrapped = true;
                
            }


            //newTransform = new Transform(newTransform.WorldMatrix * );
            ////////

            CurrentTransform = new Transform(StartTransform.WorldMatrix * CurrentRootMotionRotation * CurrentRootMotionTranslation);

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

            OnRootMotionWrap?.Invoke(translationDeltaToGetToMod);
        }

        public void UpdateAnimation()
        {
            if (IsStatic)
                Skeleton?.ApplyBakedFlverReferencePose();

            if (GameDataManager.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS)
            {
                Skeleton?.RevertToReferencePose();
                AfterAnimUpdate(Main.DELTA_UPDATE);
            }

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
                ChrAsm.RightWeaponModel0?.Skeleton?.DrawPrimitives();

                ChrAsm.RightWeaponModel1?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel1?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel1?.Skeleton?.DrawPrimitives();

                ChrAsm.RightWeaponModel2?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel2?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel2?.Skeleton?.DrawPrimitives();

                ChrAsm.RightWeaponModel3?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.RightWeaponModel3?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.RightWeaponModel3?.Skeleton?.DrawPrimitives();

                ChrAsm.LeftWeaponModel0?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel0?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel0?.Skeleton?.DrawPrimitives();

                ChrAsm.LeftWeaponModel1?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel1?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel1?.Skeleton?.DrawPrimitives();

                ChrAsm.LeftWeaponModel2?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel2?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel2?.Skeleton?.DrawPrimitives();

                ChrAsm.LeftWeaponModel3?.DummyPolyMan.DrawAllHitPrims();
                ChrAsm.LeftWeaponModel3?.DbgPrimDrawer.DrawPrimitives();
                ChrAsm.LeftWeaponModel3?.Skeleton?.DrawPrimitives();
            }

            Skeleton?.DrawPrimitives();
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

        public void Draw(int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            GFX.World.ApplyViewToShader(GFX.FlverShader, CurrentTransform);

            if (isSkyboxLol)
            {
                //((FlverShader)shader).Bones0 = new Matrix[] { Matrix.Identity };
                GFX.FlverShader.Effect.IsSkybox = true;
            }
            else if (Skeleton != null)
            {
                if (ChrAsm != null)
                {
                    GFX.FlverShader.Effect.Bones0 = Skeleton.ShaderMatrices0;

                    if (Skeleton.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray)
                    {
                        GFX.FlverShader.Effect.Bones1 = Skeleton.ShaderMatrices1;

                        if (Skeleton.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 2)
                        {
                            GFX.FlverShader.Effect.Bones2 = Skeleton.ShaderMatrices2;

                            if (Skeleton.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 3)
                            {
                                GFX.FlverShader.Effect.Bones3 = Skeleton.ShaderMatrices3;

                                if (Skeleton.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 4)
                                {
                                    GFX.FlverShader.Effect.Bones4 = Skeleton.ShaderMatrices4;

                                    if (Skeleton.FlverSkeleton.Count >= FlverShader.MaxBonePerMatrixArray * 5)
                                    {
                                        GFX.FlverShader.Effect.Bones5 = Skeleton.ShaderMatrices5;
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
                GFX.FlverShader.Effect.Bones0 = NewAnimSkeleton.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones1 = NewAnimSkeleton.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones2 = NewAnimSkeleton.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones3 = NewAnimSkeleton.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones4 = NewAnimSkeleton.IDENTITY_MATRICES;
                GFX.FlverShader.Effect.Bones5 = NewAnimSkeleton.IDENTITY_MATRICES;
            }

            if (IsVisible)
            {
                MainMesh.DrawMask = DrawMask;
                MainMesh.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol, Skeleton);
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
            Skeleton = null;
            AnimContainer = null;
            MainMesh?.Dispose();
            // Do not need to dispose DummyPolyMan because it goes 
            // stores its primitives in the model's DbgPrimDrawer
        }
    }
}
