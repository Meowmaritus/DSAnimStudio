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
        public static float AmbientLightMult = 1.0f;
        public static float FlverDirectLightMult = 0.65f;
        public static float FlverIndirectLightMult = 0.65f;
        public static float SkyboxBrightnessMult = 0.25f;
        public static float FlverSceneBrightness = 1.0f;
        public static float FlverSceneContrast = 0.6f;
        public static float FlverEmissiveMult = 1.0f;

        public static float DirectDiffuseMult = 1;
        public static float DirectSpecularMult = 1;
        public static float IndirectDiffuseMult = 1;
        public static float IndirectSpecularMult = 1;

        //public static float MotionBlurStrength = 1;

        public static float LightRotationH = -0.75f;
        public static float LightRotationV = -0.75f;

        private static Dictionary<string, TextureCube> cubemaps 
            = new Dictionary<string, TextureCube>();
        public static IReadOnlyDictionary<string, TextureCube> Cubemaps => cubemaps;

        public static string[] CubemapNames;
        public static int CubemapNameIndex = -1;

        public static string CurrentCubemapName => 
            (CubemapNameIndex >= 0 && CubemapNameIndex < CubemapNames.Length)
            ? CubemapNames[CubemapNameIndex] : null;

        public static TextureCube CurrentCubemap => CurrentCubemapName != null ? Cubemaps[CurrentCubemapName] : null;

        public static bool DrawCubemap = false;

        public static void LoadContent(ContentManager c)
        {
            //var cubemapNames = Directory.GetFiles($@"{Main.Directory}\Content\Cubemaps", "*.xnb");
            //foreach (var kvp in cubemaps)
            //    kvp.Value.Dispose();
            //cubemaps.Clear();
            //foreach (var cube in cubemapNames)
            //{
            //    cubemaps.Add(Utils.GetFileNameWithoutDirectoryOrExtension(cube), 
            //        c.Load<TextureCube>(Utils.GetFileNameWithoutAnyExtensions(cube)));
            //}
            //CurrentCubemapName = cubemaps.Keys.First();

            var cubemapNames = Directory.GetFiles($@"{Main.Directory}\Content\Cubemaps", "*.dds");
            foreach (var kvp in cubemaps)
                kvp.Value.Dispose();
            cubemaps.Clear();
            foreach (var cube in cubemapNames)
            {
                if (cube.Contains("debug_cube"))
                {
                    cubemaps.Add(Utils.GetFileNameWithoutDirectoryOrExtension(cube),
                    c.Load<TextureCube>("debug_cube"));
                }
                else
                {
                    cubemaps.Add(Utils.GetFileNameWithoutDirectoryOrExtension(cube),
                    new TextureFetchRequest(File.ReadAllBytes(cube), cube).FetchCube());
                }

            }

            cubemaps.Add("debug_cube", c.Load<TextureCube>(@"Content\Cubemaps\debug_cube"));
        

            CubemapNames = Cubemaps.Keys.ToArray();

            CubemapNameIndex = 0;

            //var testFetch = new TextureFetchRequest(File.ReadAllBytes($@"{Main.Directory}\Content\Cubemaps\m30_00_GILM0000.dds.bc3.dds"), "TEST");

            //var test = testFetch.FetchCube();

            //cubemaps.Add("[TEST]", test);

            //Console.WriteLine("OOF");
        }


    }
}
