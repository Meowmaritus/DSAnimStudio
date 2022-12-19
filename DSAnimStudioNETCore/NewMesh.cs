using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewMesh : IDisposable, IHighlightableThing
    {
        public static NewMesh GetDummyMesh()
        {
            NewMesh mesh = new NewMesh();
            mesh.Name = "Empty Mesh";
            mesh.Flver2Header = new FLVER2.FLVERHeader();
            mesh.Materials = new List<FlverMaterial>();
            mesh.Submeshes = new List<FlverSubmeshRenderer>();
            return mesh;
        }

        private NewMesh()
        {

        }

        public string Name;

        private object _lock_Materials = new object();
        public List<FlverMaterial> Materials { get; set; } = new List<FlverMaterial>();

        public List<FlverSubmeshRenderer> Submeshes = new List<FlverSubmeshRenderer>();

        private bool[] DefaultDrawMask = new bool[Model.DRAW_MASK_LENGTH];
        public bool[] DrawMask = new bool[Model.DRAW_MASK_LENGTH];

        private object _lock_submeshes = new object();

        public BoundingBox Bounds;

        public bool TextureReloadQueued = false;

        public FLVER2.FLVERHeader Flver2Header = null;

        public void TryLoadGlobalShaderConfigs()
        {
            lock (_lock_Materials)
            {
                foreach (var m in Materials)
                    m.TryLoadGlobalShaderConfig();
            }
        }

        public void DefaultAllMaskValues()
        {
            for (int i = 0; i < Model.DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i];
            }
        }

        public void HideAllDrawMask()
        {
            for (int i = 0; i < Model.DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = false;
            }
        }

        public List<string> GetAllTexNamesToLoad()
        {
            List<string> result = new List<string>();

            lock (_lock_Materials)
            {
                if (Materials != null)
                {
                    foreach (var sm in Materials)
                    {
                        result.AddRange(sm.GetAllTexNamesToLoad());
                    }
                }
            }

            
            return result;
        }

        public NewMesh(FLVER2 flver, bool useSecondUV, Dictionary<string, int> boneIndexRemap = null,
            bool ignoreStaticTransforms = false)
        {
            LoadFLVER2(flver, useSecondUV, boneIndexRemap, ignoreStaticTransforms);
        }

        public NewMesh(FLVER0 flver, bool useSecondUV, Dictionary<string, int> boneIndexRemap = null,
            bool ignoreStaticTransforms = false)
        {
            LoadFLVER0(flver, useSecondUV, boneIndexRemap, ignoreStaticTransforms);
        }

        private void LoadFLVER2(FLVER2 flver, bool useSecondUV, Dictionary<string, int> boneIndexRemap = null,
            bool ignoreStaticTransforms = false)
        {
            Flver2Header = flver.Header;


            lock (_lock_Materials)
            {
                Materials = new List<FlverMaterial>();

                foreach (var mat in flver.Materials)
                {
                    var m = new FlverMaterial();
                    m.Init(this, flver, mat);
                    Materials.Add(m);
                }
            }

            lock (_lock_submeshes)
            {
                Submeshes = new List<FlverSubmeshRenderer>();
            }

            
            foreach (var submesh in flver.Meshes)
            {
                // Blacklist some materials that don't have good shaders and just make the viewer look like a mess
                MTD mtd = null;// InterrootLoader.GetMTD(Path.GetFileName(flver.Materials[submesh.MaterialIndex].MTD));
                if (mtd != null)
                {
                    if (mtd.ShaderPath.Contains("FRPG_Water_Env"))
                        continue;
                    if (mtd.ShaderPath.Contains("FRPG_Water_Reflect.spx"))
                        continue;
                }
                var smm = new FlverSubmeshRenderer(this, flver, submesh, useSecondUV, boneIndexRemap, ignoreStaticTransforms);

                Bounds = new BoundingBox();

                lock (_lock_submeshes)
                {
                    Submeshes.Add(smm);
                    Bounds = BoundingBox.CreateMerged(Bounds, smm.Bounds);
                }
            }
        }

        private void LoadFLVER0(FLVER0 flver, bool useSecondUV, Dictionary<string, int> boneIndexRemap = null,
            bool ignoreStaticTransforms = false)
        {
            Flver2Header = null;

            lock (_lock_Materials)
            {
                Materials = new List<FlverMaterial>();

                foreach (var mat in flver.Materials)
                {
                    var m = new FlverMaterial();
                    m.Init(this, flver, mat);
                    Materials.Add(m);
                }
            }

            lock (_lock_submeshes)
            {
                Submeshes = new List<FlverSubmeshRenderer>();
            }

            foreach (var submesh in flver.Meshes)
            {
                // Blacklist some materials that don't have good shaders and just make the viewer look like a mess
                MTD mtd = null;// InterrootLoader.GetMTD(Path.GetFileName(flver.Materials[submesh.MaterialIndex].MTD));
                if (mtd != null)
                {
                    if (mtd.ShaderPath.Contains("FRPG_Water_Env"))
                        continue;
                    if (mtd.ShaderPath.Contains("FRPG_Water_Reflect.spx"))
                        continue;
                }
                var smm = new FlverSubmeshRenderer(this, flver, submesh, useSecondUV, boneIndexRemap, ignoreStaticTransforms);

                Bounds = new BoundingBox();

                lock (_lock_submeshes)
                {
                    Submeshes.Add(smm);
                    Bounds = BoundingBox.CreateMerged(Bounds, smm.Bounds);
                }
            }
        }

        public void Draw(int lod, bool motionBlur, bool forceNoBackfaceCulling, bool isSkyboxLol, Model model,
            NewAnimSkeleton_FLVER skeleton = null, Action<Exception> onDrawFail = null)
        {
            if (TextureReloadQueued)
            {
                TryToLoadTextures();
                TextureReloadQueued = false;
            }

            lock (_lock_submeshes)
            {
                if (Submeshes == null)
                    return;

                FlverMaterial soloViewMaterial = null;
                foreach (var m in Materials)
                {
                    if (m.IsSoloVisible)
                    {
                        soloViewMaterial = m;
                        break;
                    }
                }

                foreach (var submesh in Submeshes)
                {
                    try
                    {
                        if (soloViewMaterial == null || soloViewMaterial == submesh.NewMaterial)
                            submesh.Draw(lod, motionBlur, DrawMask, forceNoBackfaceCulling, skeleton, onDrawFail, model);
                    }
                    catch (Exception ex)
                    {
                        onDrawFail?.Invoke(ex);
                    }
                }
            }
        }

        public void TryToLoadTextures()
        {
            lock (_lock_Materials)
            {
                if (Materials == null)
                    return;

                foreach (var sm in Materials)
                    sm.TryToLoadTextures();
            }
        }

        public void Dispose()
        {
            if (Submeshes != null)
            {
                for (int i = 0; i < Submeshes.Count; i++)
                {
                    if (Submeshes[i] != null)
                        Submeshes[i].Dispose();
                }

                Submeshes = null;
            }
        }
    }
}
