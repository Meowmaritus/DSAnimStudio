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

        public void DefaultAllMaskValues()
        {
            for (int i = 0; i < Model.DRAW_MASK_LENGTH; i++)
            {
                DrawMask[i] = DefaultDrawMask[i];
            }
        }

        public NewMesh(FLVER2 flver, bool useSecondUV, Dictionary<string, int> boneIndexRemap = null)
        {
            LoadFLVER2(flver, useSecondUV, boneIndexRemap);
        }

        private void LoadFLVER2(FLVER2 flver, bool useSecondUV, Dictionary<string, int> boneIndexRemap = null)
        {
            Submeshes = new List<FlverSubmeshRenderer>();
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
                var smm = new FlverSubmeshRenderer(this, flver, submesh, useSecondUV, boneIndexRemap);
                Submeshes.Add(smm);
            }
        }

        public void Draw(int lod = 0, bool motionBlur = false, bool forceNoBackfaceCulling = false, bool isSkyboxLol = false)
        {
            foreach (var submesh in Submeshes)
            {
                submesh.Draw(lod, motionBlur, GFX.FlverShader, DrawMask, forceNoBackfaceCulling, isSkyboxLol);
            }
        }

        public void TryToLoadTextures()
        {
            foreach (var sm in Submeshes)
                sm.TryToLoadTextures();
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
