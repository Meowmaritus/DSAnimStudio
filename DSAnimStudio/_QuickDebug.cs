using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class _QuickDebug
    {
        public static void BuildDebugMenu()
        {
            if (DebugButton("SoulsAssetPipeline_Test"))
            {
                var importSettings = new SoulsAssetPipeline.FLVER2Importer.FLVER2ImportSettings()
                {
                    SceneScale = 0.01f,
                };

                var importedFlver = SoulsAssetPipeline.FLVER2Importer.ImportFBX(
                    @"C:\DarkSoulsModding\_FBX\BlightedBoar\BlightedBoar.fbx", importSettings);
                lock (Scene._lock_ModelLoad_Draw)
                {
                    Scene.Models[0].MainMesh?.Dispose();
                    foreach (var tex in importedFlver.Textures)
                    {
                        TexturePool.AddFetchDDS(tex.Value, tex.Key);
                    }

                    Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

                    for (int i = 0; i < Scene.Models[0].Skeleton.FlverSkeleton.Count; i++)
                    {
                        if (!boneIndexRemap.ContainsKey(Scene.Models[0].Skeleton.FlverSkeleton[i].Name))
                            boneIndexRemap.Add(Scene.Models[0].Skeleton.FlverSkeleton[i].Name, i);
                    }

                    Scene.Models[0].MainMesh = new NewMesh(importedFlver.Flver, false, boneIndexRemap);

                    Scene.ForceTextureReloadImmediate();
                }
                Console.WriteLine("fatcat");
            }

            if (DebugButton("SoulsAssetPipeline_Anim_Test"))
            {
                var importSettings = new SoulsAssetPipeline.FLVER2Importer.FLVER2ImportSettings()
                {
                    SceneScale = 0.01f,
                };

                var importedFlver = SoulsAssetPipeline.FLVER2Importer.ImportFBX(
                    @"C:\DarkSoulsModding\_FBX\BlightedBoar\BlightedBoar.fbx", importSettings);
                lock (Scene._lock_ModelLoad_Draw)
                {
                    Scene.Models[0].MainMesh?.Dispose();
                    foreach (var tex in importedFlver.Textures)
                    {
                        TexturePool.AddFetchDDS(tex.Value, tex.Key);
                    }

                    Dictionary<string, int> boneIndexRemap = new Dictionary<string, int>();

                    for (int i = 0; i < Scene.Models[0].Skeleton.FlverSkeleton.Count; i++)
                    {
                        if (!boneIndexRemap.ContainsKey(Scene.Models[0].Skeleton.FlverSkeleton[i].Name))
                            boneIndexRemap.Add(Scene.Models[0].Skeleton.FlverSkeleton[i].Name, i);
                    }

                    Scene.Models[0].MainMesh = new NewMesh(importedFlver.Flver, false, boneIndexRemap);

                    Scene.ForceTextureReloadImmediate();
                }
                Console.WriteLine("fatcat");
            }
        }

        private static bool DebugButton(string name)
        {
            ImGui.Button(name);
            return ImGui.IsItemClicked();
        }
    }
}
