using Microsoft.Xna.Framework;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class NewMesh : IDisposable
    {
        public List<FlverSubmeshRenderer> Submeshes = new List<FlverSubmeshRenderer>();

        private bool[] DefaultDrawMask = new bool[Model.DRAW_MASK_LENGTH];
        public bool[] DrawMask = new bool[Model.DRAW_MASK_LENGTH];

        private object _lock_submeshes = new object();

        public BoundingBox Bounds;

        public bool TextureReloadQueued = false;

        public void DefaultAllMaskValues()
        {
            for (int i = 0; i < Model.DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i];
            }
        }

        public List<string> GetAllTexNamesToLoad()
        {
            List<string> result = new List<string>();

            lock (_lock_submeshes)
            {
                if (Submeshes != null)
                {
                    foreach (var sm in Submeshes)
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

        private void LoadFLVER2(FLVER2 flver, bool useSecondUV, Dictionary<string, int> boneIndexRemap = null, 
            bool ignoreStaticTransforms = false)
        {
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

        public void Draw(int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
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

                foreach (var submesh in Submeshes)
                {
                    submesh.Draw(lod, motionBlur, DrawMask, forceNoBackfaceCulling, isSkyboxLol);
                }
            }
        }

        public void TryToLoadTextures()
        {
            lock (_lock_submeshes)
            {
                if (Submeshes == null)
                    return;

                foreach (var sm in Submeshes)
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
