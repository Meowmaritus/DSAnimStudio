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

        public NewMesh MainMesh;

        public NewAnimSkeleton Skeleton;
        public NewAnimationContainer AnimContainer;
        public NewDummyPolyManager DummyPolyMan;
        public DBG.DbgPrimDrawer DbgPrimDrawer;
        public NewChrAsm ChrAsm = null;
        public ParamData.NpcParam NpcParam = null;

        public BoundingBox Bounds;

        public const int DRAW_MASK_LENGTH = 96;

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

        public Matrix CurrentRootMotionRotation => Matrix.CreateRotationY(CurrentDirection);

        public float CurrentDirection;

        public Transform CurrentTransform = Transform.Default;

        /// <summary>
        /// This is needed to make weapon hitboxes work.
        /// </summary>
        public bool IS_PLAYER = false;

        public bool IS_PLAYER_WEAPON = false;


        private Model()
        {
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
                var nameCheck = f.Name.ToLower();
                if (flver == null && (nameCheck.EndsWith(".flver") || FLVER2.Is(f.Bytes)))
                {
                    if (nameCheck.EndsWith($"_{modelIndex}.flver") || modelIndex == 0)
                    {
                        flver = FLVER2.Read(f.Bytes);
                    }
                }
                else if (nameCheck.EndsWith(".tpf") || TPF.Is(f.Bytes))
                {
                    tpfsUsed.Add(TPF.Read(f.Bytes));
                }
                else if (anibnd == null && nameCheck.EndsWith(".anibnd"))
                {
                    if (nameCheck.EndsWith($"_{modelIndex}.anibnd") || modelIndex == 0)
                    {
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
            }

            if (flver == null)
            {
                throw new ArgumentException("No FLVERs found within CHRBND.");
            }

            LoadFLVER2(flver, useSecondUV: false, baseDmyPolyID, ignoreStaticTransforms);

            loadingProgress.Report(1.0 / 4.0);

            AnimContainer = new NewAnimationContainer(this);

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
            if (possibleLooseTpfFolder != null && Directory.Exists(possibleLooseTpfFolder))
            {
                TexturePool.AddTPFFolder(possibleLooseTpfFolder);
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

            if (!ignorePosWrap && AnimContainer?.EnableRootMotionWrap == true && (locationWithNewTransform.LengthSquared() > 100))
            {
                Vector3 locationWithNewTransform_Mod = new Vector3(locationWithNewTransform.X % 1, locationWithNewTransform.Y, locationWithNewTransform.Z % 1);
                Vector3 translationDeltaToGetToMod = locationWithNewTransform_Mod - locationWithNewTransform;
                CurrentRootMotionTranslation *= Matrix.CreateTranslation(translationDeltaToGetToMod);
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
                if (ChrAsm.RightWeaponModel != null)
                {
                    ChrAsm.RightWeaponModel.DummyPolyMan.UpdateAllHitPrims();
                }

                if (ChrAsm.LeftWeaponModel != null)
                {
                    ChrAsm.LeftWeaponModel.DummyPolyMan.UpdateAllHitPrims();
                }
            }
        }

        public void UpdateAnimation()
        {
            AnimContainer.Update();

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
                if (ChrAsm.RightWeaponModel != null)
                {
                    //ChrAsm.RightWeaponModel.DummyPolyMan.UpdateAllHitPrims();
                    ChrAsm.RightWeaponModel.DummyPolyMan.DrawAllHitPrims();
                    ChrAsm.RightWeaponModel.DbgPrimDrawer.DrawPrimitives();
                }

                if (ChrAsm.LeftWeaponModel != null)
                {
                    //ChrAsm.LeftWeaponModel.DummyPolyMan.UpdateAllHitPrims();
                    ChrAsm.LeftWeaponModel.DummyPolyMan.DrawAllHitPrims();
                    ChrAsm.LeftWeaponModel.DbgPrimDrawer.DrawPrimitives();
                }
            }

            Skeleton.DrawPrimitives();
        }

        public void DrawAllPrimitiveTexts()
        {
            DummyPolyMan.DrawAllHitPrimTexts();

            DbgPrimDrawer.DrawPrimitiveNames();

            if (ChrAsm != null)
            {
                if (ChrAsm.RightWeaponModel != null)
                {
                    ChrAsm.RightWeaponModel.DummyPolyMan.DrawAllHitPrimTexts();
                }

                if (ChrAsm.LeftWeaponModel != null)
                {
                    ChrAsm.LeftWeaponModel.DummyPolyMan.DrawAllHitPrimTexts();
                }
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
            else
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

                GFX.FlverShader.Effect.IsSkybox = false;
            }

            if (IsVisible)
            {
                MainMesh.DrawMask = DrawMask;
                MainMesh.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
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
