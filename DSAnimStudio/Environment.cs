using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class Environment
    {
        //public static float AmbientLightMult = 1.0f;
        public static float FlverDirectLightMult = 1.0f;
        public static float FlverIndirectLightMult = 1.0f;
        public static float FlverSceneBrightness = 1.0f;
        public static float FlverEmissiveMult = 1.0f;

        private static Dictionary<string, TextureCube> cubemaps 
            = new Dictionary<string, TextureCube>();
        public static IReadOnlyDictionary<string, TextureCube> Cubemaps => cubemaps;

        public static string CurrentCubemapName = null;

        public static TextureCube CurrentCubemap => CurrentCubemapName != null ? Cubemaps[CurrentCubemapName] : null;

        public static bool DrawCubemap = true;

        public static void LoadContent(ContentManager c)
        {
            var cubemapNames = Directory.GetFiles($@"{Main.Directory}\Content\Cubemaps", "*.xnb");
            foreach (var kvp in cubemaps)
                kvp.Value.Dispose();
            cubemaps.Clear();
            foreach (var cube in cubemapNames)
            {
                cubemaps.Add(Utils.GetFileNameWithoutDirectoryOrExtension(cube), 
                    c.Load<TextureCube>(Utils.GetFileNameWithoutAnyExtensions(cube)));
            }
            CurrentCubemapName = cubemaps.Keys.First();

            //var testFetch = new TextureFetchRequest(File.ReadAllBytes($@"{Main.Directory}\Content\Cubemaps\m30_00_GILM0000.dds.bc3.dds"), "TEST");

            //var test = testFetch.FetchCube();

            //cubemaps.Add("[TEST]", test);

            //Console.WriteLine("OOF");
        }


    }
}
