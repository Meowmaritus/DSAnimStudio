using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using DSAnimStudio.GFXShaders;

namespace DSAnimStudio
{
    public class Model : IDisposable
    {
        public string Name { get; set; } = "Model";

        public bool IsVisible { get; set; } = true;

        public NewAnimSkeleton Skeleton;
        public NewAnimationContainer AnimContainer;
        public DummyPolyManager DummyPolyMan;
        public DBG.DbgPrimDrawer DbgPrimDrawer;
        public NewChrAsm ChrAsm = null;
        public ParamData.NpcParam NpcParam = null;

        public Model ParentModelForChrAsm = null;

        private Model()
        {
            DummyPolyMan = new DummyPolyManager(this);
            DbgPrimDrawer = new DBG.DbgPrimDrawer(this);

            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DefaultDrawMask[i] = DrawMask[i] = true;
            }
        }

        public const int DRAW_MASK_LENGTH = 96;

        private bool[] DefaultDrawMask = new bool[DRAW_MASK_LENGTH];
        public bool[] DrawMask = new bool[DRAW_MASK_LENGTH];

        public void DefaultAllMaskValues()
        {
            for (int i = 0; i < DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i];
            }
        }

        public enum ModelType
        {
            ModelTypeFlver,
            ModelTypeCollision,
        };
        ModelType Type;

        public Transform StartTransform = Transform.Default;

        public Transform CurrentRootMotionTransform => new Transform(AnimContainer.CurrentAnimRootMotionMatrix);

        public Transform CurrentTransform => new Transform(StartTransform.WorldMatrix * AnimContainer.CurrentAnimRootMotionMatrix);

        /// <summary>
        /// This is needed to make weapon hitboxes work.
        /// </summary>
        public bool IS_PLAYER = false;

        public bool IS_PLAYER_WEAPON = false;

        public Model(IProgress<double> loadingProgress, string name, IBinder chrbnd, int modelIndex, 
            IBinder anibnd, IBinder texbnd = null, List<string> additionalTpfNames = null, 
            string possibleLooseDdsFolder = null, int baseDmyPolyID = 0)
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

            LoadFLVER2(flver, useSecondUV: false, baseDmyPolyID);

            loadingProgress.Report(1.0 / 4.0);

            AnimContainer = new NewAnimationContainer(this);

            if (anibnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_ANIBND", $"Loading ANIBND for {Name}...", innerProg =>
                {
                    AnimContainer.LoadBaseANIBND(anibnd, innerProg);
                });
            }
            else
            {
                Skeleton.ApplyBakedFlverReferencePose();
            }

            loadingProgress.Report(2.0 / 3.0);

            if (tpfsUsed.Count > 0)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TPFs", $"Loading TPFs for {Name}...", innerProg =>
                {
                    for (int i = 0; i < tpfsUsed.Count; i++)
                    {
                        TexturePool.AddTpf(tpfsUsed[i]);
                        Scene.RequestTextureLoad();
                        innerProg.Report(1.0 * i / tpfsUsed.Count);
                    }
                    Scene.RequestTextureLoad();
                });

            }

            loadingProgress.Report(3.0 / 4.0);

            if (texbnd != null)
            {
                LoadingTaskMan.DoLoadingTaskSynchronous($"{Name}_TEXBND", $"Loading TEXBND for {Name}...", innerProg =>
                {
                    TexturePool.AddTextureBnd(texbnd, innerProg);
                    Scene.RequestTextureLoad();
                });
            }

            // This will only be for PTDE so it will be extremely fast lol, 
            // not gonna bother with progress bar update.
            if (possibleLooseDdsFolder != null && Directory.Exists(possibleLooseDdsFolder))
            {
                TexturePool.AddLooseDDSFolder(possibleLooseDdsFolder);
                Scene.RequestTextureLoad();
            }

            Scene.RequestTextureLoad();

            loadingProgress.Report(1.0);
        }

        public void CreateChrAsm()
        {
            ChrAsm = new NewChrAsm(this);
            ChrAsm.InitSkeleton(Skeleton);
        }

        public NewMesh MainMesh;

        private void LoadFLVER2(FLVER2 flver, bool useSecondUV, int baseDmyPolyID = 0)
        {
            Type = ModelType.ModelTypeFlver;

            Skeleton = new NewAnimSkeleton(this, flver.Bones);

            MainMesh = new NewMesh(flver, useSecondUV);

            DummyPolyMan.LoadDummiesFromFLVER(flver, baseDmyPolyID);

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

        public void UpdateAnimation(GameTime gameTime)
        {
            AnimContainer.Update(gameTime);

            if (ChrAsm != null)
            {
                ChrAsm.UpdateWeaponTransforms();
                ChrAsm.UpdateWeaponAnimation(gameTime);
            }
        }

        public void TryToLoadTextures()
        {
            MainMesh?.TryToLoadTextures();
            ChrAsm?.TryToLoadTextures();
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
                GFX.FlverShader.Effect.Bones0 = Skeleton.ShaderMatrix0;

                if (Skeleton.FlverSkeleton.Count >= FlverShader.NUM_BONES)
                {
                    GFX.FlverShader.Effect.Bones1 = Skeleton.ShaderMatrix1;

                    if (Skeleton.FlverSkeleton.Count >= FlverShader.NUM_BONES * 2)
                    {
                        GFX.FlverShader.Effect.Bones2 = Skeleton.ShaderMatrix2;

                        if (Skeleton.FlverSkeleton.Count >= FlverShader.NUM_BONES * 3)
                        {
                            GFX.FlverShader.Effect.Bones3 = Skeleton.ShaderMatrix3;
                        }

                    }
                }

                GFX.FlverShader.Effect.IsSkybox = false;
            }

            MainMesh.Draw(lod, motionBlur, forceNoBackfaceCulling, isSkyboxLol);
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
