using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class FlverShaderConfig
    {
        //public string Name;

        public static FlverShaderConfig GetShaderConfig(string name)
        {
            FlverShaderConfig result = null;
            lock (_lock_GlobalConfig)
            {
                result = LoadGlobalDefault(name ?? "NULL");
            }
            return result;
        }

        public static void ClearCache()
        {
            lock (_lock_GlobalConfig)
            {
                globalShaderCfgCache.Clear();
            }
        }

        [JsonIgnore]
        public List<FlverMaterial.ChrCustomizeTypes> chrCustomizeType_Values = Enum.GetValues<FlverMaterial.ChrCustomizeTypes>().ToList();
        [JsonIgnore]
        public string[] chrCustomizeType_Names = Enum.GetNames<FlverMaterial.ChrCustomizeTypes>();
        public FlverMaterial.ChrCustomizeTypes ChrCustomizeType;

        public bool EnableSSS;
        public Vector3 SSSColor = new Vector3(1, 0.48f, 0.35f);
        public float SSSIntensity = 1;

        public bool IsMetallic;
        public Vector3 NonMetallicSpecColor;
        public float UndefinedMetallicValue = 0.5f;
        public float UndefinedBlendMaskValue = 0;

        public bool UseShininessMap;

        public bool SwapNormalXY = false;

        public bool IsDS2EmissiveFlow;
        public bool IsShaderDoubleFaceCloth;
        public bool IsDS3Veil = false;
        public bool InvertBlendMaskMap = false;
        public bool EnableBlendMask = true;

        public bool BlendMaskFromNormalMap1Alpha = false;
        public bool BlendMaskFromNormalMap1Alpha_IsReverse = false;
        public bool BlendMaskMultByAlbedoMap2Alpha = false;
        public bool BlendMaskMultByAlbedoMap2Alpha_IsReverse = false;

        public List<SamplerConfig> SamplerConfigs = new List<SamplerConfig>();

        public NewBlendOperations NewBlendOperation_Diffuse = NewBlendOperations.Multiply;
        public NewBlendOperations NewBlendOperation_Specular = NewBlendOperations.Multiply;
        public NewBlendOperations NewBlendOperation_Normal = NewBlendOperations.NormalMapBlend;
        public NewBlendOperations NewBlendOperation_Shininess = NewBlendOperations.Multiply;
        public NewBlendOperations NewBlendOperation_Emissive = NewBlendOperations.Lerp;

        public bool NewBlendReverseDir_Diffuse = false;
        public bool NewBlendReverseDir_Specular = false;
        public bool NewBlendReverseDir_Normal = false;
        public bool NewBlendReverseDir_Shininess = false;
        public bool NewBlendReverseDir_Emissive = false;

        public bool NewBlendInverseVal_Diffuse = false;
        public bool NewBlendInverseVal_Specular = false;
        public bool NewBlendInverseVal_Normal = false;
        public bool NewBlendInverseVal_Shininess = false;
        public bool NewBlendInverseVal_Emissive = false;

        public bool IsReflectMultInNormalAlpha = false;

        public float EmissiveMult = 1;
        public bool EmissiveColorFromAlbedo = false;

        public float DirectLightingMult = 1;
        public float IndirectLightingMult = 1;

        public float DirectDiffuseMult = 1;
        public float DirectSpecularMult = 1;
        public float IndirectDiffuseMult = 1;
        public float IndirectSpecularMult = 1;

        public float SpecularPowerMult = 1;
        public float LdotNPowerMult = 1;

        public bool UseFancyAlphas = true;
        public bool EnableAlphas = true;
        public float FancyAlphaCutoff = 1;

        private static volatile System.Reflection.FieldInfo[] _fields = null;
        static FlverShaderConfig()
        {
            _fields = typeof(FlverShaderConfig).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        }
        public FlverShaderConfig GetClone()
        {
            //var x = new FlverShaderConfig();
            //foreach (var f in _fields)
            //    f.SetValue(x, f.GetValue(this));
            //// Dereference lists
            //x.SamplerConfigs = SamplerConfigs.Select(x => x.GetClone()).ToList();
            //return x;
            var jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<FlverShaderConfig>(jsonText);
        }

        public class SamplerConfig
        {
            public string Name;
            public FlverMaterial.TextureTypes TexType;
            public string DefaultTexPath = "";
            [Newtonsoft.Json.JsonIgnore]
            public Texture2D DefaultTex = null;
            public int UVIndex;
            public int UVGroup;

            public SamplerConfig GetClone()
            {
                return new SamplerConfig
                {
                    Name = this.Name,
                    TexType = this.TexType,
                    UVIndex = this.UVIndex,
                    UVGroup = this.UVGroup,
                    DefaultTexPath = this.DefaultTexPath,
                    DefaultTex = this.DefaultTex,
                };
            }
        }

        //public class GlobalShaderConfigJsonFile
        //{
        //    public List<FlverShaderConfig> Configs = new List<FlverShaderConfig>();
        //}
        //private static string GlobalConfigFilePath = null;
        //private static GlobalShaderConfigJsonFile GlobalConfigFile = null;

        //private static void LoadGlobalConfigFileOrDefault()
        //{
        //    if (GlobalConfigFilePath == null)
        //    {
        //        var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        //        var currentAssemblyDir = System.IO.Path.GetDirectoryName(currentAssemblyPath);
        //        GlobalConfigFilePath = System.IO.Path.Combine(currentAssemblyDir, "DSAnimStudio_Shader");
        //    }
        //}

        private static string GetShaderGlobalDefaultFilePath(string shaderName)
        {
            var result = System.IO.Path.Combine(Main.Directory, @$"ShaderConfig\{GameRoot.GameType}\{shaderName}.json");
            var dir = System.IO.Path.GetDirectoryName(result);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            return result;
        }

        private void AfterJsonLoad()
        {
            List<SamplerConfig> trimmedSamplerConfigList = new List<SamplerConfig>();
            List<string> samplerNamesAlreadyRegistered = new List<string>();
            foreach (var s in SamplerConfigs)
            {
                if (samplerNamesAlreadyRegistered.Contains(s.Name))
                    continue;

                trimmedSamplerConfigList.Add(s);
                samplerNamesAlreadyRegistered.Add(s.Name);
            }
            SamplerConfigs = trimmedSamplerConfigList;
        }

        private static Dictionary<string, FlverShaderConfig> globalShaderCfgCache = new Dictionary<string, FlverShaderConfig>();
        private static object _lock_GlobalConfig = new object();
        public static FlverShaderConfig LoadGlobalDefault(string shaderName)
        {
            if (shaderName == null)
                return null;
            FlverShaderConfig config = null;
            string exception = null;
            lock (_lock_GlobalConfig)
            {
                if (globalShaderCfgCache.ContainsKey(shaderName))
                    config = globalShaderCfgCache[shaderName];
                else
                {
                    var jsonFilePath = GetShaderGlobalDefaultFilePath(shaderName);
                    if (System.IO.File.Exists(jsonFilePath))
                    {
                        try
                        {
                            var jsonText = System.IO.File.ReadAllText(jsonFilePath);
                            config = Newtonsoft.Json.JsonConvert.DeserializeObject<FlverShaderConfig>(jsonText);
                            config.AfterJsonLoad();
                        }
                        catch (Newtonsoft.Json.JsonException ex)
                        {
                            exception = $"Failed to parse shader config json file '{jsonFilePath}'.\nError shown below:\n\n\n{ex}";
                            config = null;
                        }
                        
                    }
                    else
                    {
                        config = null;
                    }
                    globalShaderCfgCache.Add(shaderName, config);
                }

                
            }
            if (exception != null)
            {
                System.Windows.Forms.MessageBox.Show(exception, "Configuration File Parse Failure",
                                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
            return config;
        }

        public static void SaveGlobalDefault(FlverShaderConfig config, string shaderName)
        {
            lock (_lock_GlobalConfig)
            {
                //if (globalShaderCfgCache.ContainsKey(config.Name))
                //    globalShaderCfgCache[config.Name] = config;
                //else
                //    globalShaderCfgCache.Add(config.Name, config);

                var jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                var jsonFilePath = GetShaderGlobalDefaultFilePath(shaderName);
                System.IO.File.WriteAllText(jsonFilePath, jsonText);
            }
        }
    }
}
