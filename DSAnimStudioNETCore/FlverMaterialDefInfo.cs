using Microsoft.Xna.Framework;
using SoulsAssetPipeline;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class FlverMaterialDefInfo
    {
        public string Name;
        public string ShaderName;

        public byte[] VPO = null;
        public byte[] PPO = null;

        public Dictionary<string, SamplerConfig> SamplerConfigs = new Dictionary<string, SamplerConfig>();

        public Dictionary<string, object> ShaderParameters = new Dictionary<string, object>();
        public Dictionary<string, bool> ShaderParameters_AreUsedByDSAS = new Dictionary<string, bool>();

        public bool IsParameterUsedByDSAS(string parameterName)
        {
            if (ShaderParameters_AreUsedByDSAS.ContainsKey(parameterName) && ShaderParameters_AreUsedByDSAS[parameterName])
                return true;

            return false;
        }

        public object GetShaderParameterOrDefault(string parameterName, object defaultValue)
        {
            if (ShaderParameters_AreUsedByDSAS.ContainsKey(parameterName))
                ShaderParameters_AreUsedByDSAS[parameterName] = true;

            if (ShaderParameters.ContainsKey(parameterName))
                return ShaderParameters[parameterName];
            else
                return defaultValue;
        }

        public float GetFloatParameterOrDefault(string parameterName, float defaultValue)
        {
            if (ShaderParameters_AreUsedByDSAS.ContainsKey(parameterName))
                ShaderParameters_AreUsedByDSAS[parameterName] = true;

            if (ShaderParameters.ContainsKey(parameterName))
            {
                return (float)ShaderParameters[parameterName];
            }
            else
                return defaultValue;
        }

        public Vector2 GetFloat2ParameterOrDefault(string parameterName, Vector2 defaultValue)
        {
            if (ShaderParameters_AreUsedByDSAS.ContainsKey(parameterName))
                ShaderParameters_AreUsedByDSAS[parameterName] = true;

            if (ShaderParameters.ContainsKey(parameterName))
            {
                float[] val = (float[])ShaderParameters[parameterName];
                return new Vector2(val[0], val[1]);
            }
            else
                return defaultValue;
        }

        public Vector4 GetFloat4ParameterOrDefault(string parameterName, Vector4 defaultValue)
        {
            if (ShaderParameters_AreUsedByDSAS.ContainsKey(parameterName))
                ShaderParameters_AreUsedByDSAS[parameterName] = true;

            if (ShaderParameters.ContainsKey(parameterName))
            {
                float[] val = (float[])ShaderParameters[parameterName];
                return new Vector4(val[0], val[1], val[2], val[3]);
            }
            else
                return defaultValue;
        }

        public (Vector4 Color, float Power) GetFloat5ParameterOrDefault(string parameterName, Vector4 defaultColor, float defaultPower)
        {
            if (ShaderParameters_AreUsedByDSAS.ContainsKey(parameterName))
                ShaderParameters_AreUsedByDSAS[parameterName] = true;

            if (ShaderParameters.ContainsKey(parameterName))
            {
                float[] val = (float[])ShaderParameters[parameterName];
                return (new Vector4(val[0], val[1], val[2], val[3]), val[4]);
            }
            else
                return (defaultColor, defaultPower);
        }

        public Vector3 GetFloat3ParameterOrDefault(string parameterName, Vector3 defaultValue)
        {
            if (ShaderParameters_AreUsedByDSAS.ContainsKey(parameterName))
                ShaderParameters_AreUsedByDSAS[parameterName] = true;

            if (ShaderParameters.ContainsKey(parameterName))
            {
                float[] val = (float[])ShaderParameters[parameterName];
                return new Vector3(val[0], val[1], val[2]);
            }
            else
                return defaultValue;
        }

        //public Vector2[] UVTileMult = new Vector2[]
        //{
        //    Vector2.One,
        //    Vector2.One,
        //    Vector2.One,
        //    Vector2.One,
        //    Vector2.One,
        //    Vector2.One,
        //    Vector2.One,
        //    Vector2.One,
        //};

        private static object _lock_MaterialBinderDefInfos = new object();

        public static string MaterialBinderPath;
        public static string MaterialBinderExtraPath;
        public static string ShaderBdleBinderPath;
        public static string ShaderGXFlverBdleBinderPath;



        //private static IBinder MaterialBinder = null;
        //private static IBinder MaterialBinderExtra = null;
        //private static IBinder ShaderBdleBinder = null;
        //private static IBinder ShaderBinder_GXFlver = null;

        private static Dictionary<string, IBinder> _binderCache = new Dictionary<string, IBinder>();

        public static void FlushBinderCache()
        {
            flushTimer = 30;
        }

        static int flushTimer = -1;

        public static void UpdateMem()
        {
            if (flushTimer == 0)
            {
                lock (_lock_MaterialBinderDefInfos)
                {
                    _binderCache.Clear();
                }
                GC.Collect();
            }

            if (flushTimer > -1)
                flushTimer--;
        }

        private static IBinder LoadBinder(string path)
        {
            if (path == null)
                return null;

            IBinder result = null;

            lock (_lock_MaterialBinderDefInfos)
            {
                if (_binderCache.ContainsKey(path))
                {
                    result = _binderCache[path];
                }
                else
                {
                    var data = GameData.ReadFile(path);
                    if (data != null)
                    {
                        if (BND3.Is(data))
                            result = BND3.Read(data);
                        else if (BND4.Is(data))
                            result = BND4.Read(data);

                        _binderCache.Add(path, result);
                    }

                }
            }

                

            
            return result;
        }



        private static Dictionary<string, FlverMaterialDefInfo> MaterialBinderDefInfos = new Dictionary<string, FlverMaterialDefInfo>();
        public static void LoadMaterialBinders(string path, string extraPath)
        {
            lock (_lock_MaterialBinderDefInfos)
            {
                MaterialBinderPath = path;
                MaterialBinderExtraPath = extraPath;

                MaterialBinderDefInfos.Clear();
            }
        }

        public static IBinder LookupShaderBdle(string shaderName)
        {
            BinderFile result = null;
            lock (_lock_MaterialBinderDefInfos)
            {
                var check = Utils.GetShortIngameFileName(shaderName).ToLower();
                if (check.StartsWith("gxflver"))
                {
                    return LoadBinder(ShaderGXFlverBdleBinderPath);
                }
                else
                {
                    var shaderBdleBinder = LoadBinder(ShaderBdleBinderPath);
                    if (shaderBdleBinder != null)
                        result = shaderBdleBinder.Files.FirstOrDefault(f => Utils.GetShortIngameFileName(f.Name) == Utils.GetShortIngameFileName(shaderName));
                }
            }
            if (result != null)
            {
                if (BND4.Is(result.Bytes))
                    return BND4.Read(result.Bytes);
                else if (BND3.Is(result.Bytes))
                    return BND3.Read(result.Bytes);
            }
            return null;
        }

        public static void LoadShaderBdleBinder(string shaderBdlePath, string gxFlverPath)
        {
            lock (_lock_MaterialBinderDefInfos)
            {
                ShaderBdleBinderPath = shaderBdlePath;
                ShaderGXFlverBdleBinderPath = gxFlverPath;
            }
        }

        public static SamplerConfig LookupSampler(string mtdName, string samplerName)
        {
            mtdName = mtdName.ToLower();
            if (samplerName == null)
                return null;
            var defInfo = Lookup(mtdName);
            if (defInfo != null && defInfo.SamplerConfigs.ContainsKey(samplerName))
            {
                return defInfo.SamplerConfigs[samplerName];
            }
            return null;
        }

        public static FlverMaterialDefInfo Lookup(string mtdName)
        {
            //bool isBinderLoaded = false;
            //lock (_lock_MaterialBinderDefInfos)
            //{
            //    if (MaterialBinder != null || MaterialBinderExtra != null)
            //    {
            //        isBinderLoaded = true;
            //    }
            //}
            //if (!isBinderLoaded)
            //    return null;

            var MaterialBinder = LoadBinder(MaterialBinderPath);
            var MaterialBinderExtra = LoadBinder(MaterialBinderExtraPath);

            FlverMaterialDefInfo result = null;
            string nameCheck = Utils.GetShortIngameFileName(mtdName).ToLower();
            lock (_lock_MaterialBinderDefInfos)
            {
                if (MaterialBinderDefInfos.ContainsKey(nameCheck))
                {
                    result = MaterialBinderDefInfos[nameCheck];
                }
                else
                {
                    if (MaterialBinderExtra != null)
                    {
                        var binderFileMatch = MaterialBinderExtra.Files.FirstOrDefault(f => Utils.GetShortIngameFileName(f.Name).ToLower() == nameCheck);
                        if (binderFileMatch != null && !MaterialBinderDefInfos.ContainsKey(nameCheck))
                        {
                            result = FromFile(nameCheck, binderFileMatch.Bytes, GameRoot.GameType);
                            MaterialBinderDefInfos.Add(nameCheck, result);
                        }
                    }

                    if (MaterialBinder != null)
                    {
                        var binderFileMatch = MaterialBinder.Files.FirstOrDefault(f => Utils.GetShortIngameFileName(f.Name).ToLower() == nameCheck);
                        if (binderFileMatch != null && !MaterialBinderDefInfos.ContainsKey(nameCheck))
                        {
                            result = FromFile(nameCheck, binderFileMatch.Bytes, GameRoot.GameType);
                            MaterialBinderDefInfos.Add(nameCheck, result);
                        }
                    }
                    
                }
            }

            return result ?? new FlverMaterialDefInfo();
        }

        enum MatbinValueType : int
        {
            Byte = 0,
            Int = 4,
            Int2 = 5,
            Float = 8,
            Float2 = 9,
            Float5 = 10,
            Float4 = 11,
            Float5B = 12,
        }

        public class EldenRingMetaparamInfo
        {
            public List<SamplerInfo> SamplerInfos = new List<SamplerInfo>();
            public class SamplerInfo
            {
                public string Name;
                public string DefaultTexturePath;
                public string GroupName;
            }
        }

        public static EldenRingMetaparamInfo ReadMetaParam(byte[] bytes)
        {
            var res = new EldenRingMetaparamInfo();
            var bin = new SoulsFormats.BinaryReaderEx(false, bytes);
            bin.Position = 0xC;
            int samplerCount = bin.ReadInt32();
            for (int i = 0; i < samplerCount; i++)
            {
                bin.Position = 0x98 + (0x30 * i);
                long samplerNameOffset = bin.ReadInt64();
                bin.Position += 8;
                long defaultTexturePathOffset = bin.ReadInt64();
                long samplerGroupNameOffset = bin.ReadInt64();

                res.SamplerInfos.Add(new EldenRingMetaparamInfo.SamplerInfo()
                {
                    Name = bin.GetUTF16(samplerNameOffset),
                    DefaultTexturePath = bin.GetUTF16(defaultTexturePathOffset),
                    GroupName = bin.GetUTF16(samplerGroupNameOffset),
                });
            }
            return res;
        }

        public static void UnloadAll()
        {
            lock (_lock_MaterialBinderDefInfos)
            {
                MaterialBinderDefInfos?.Clear();
                
            }

            lock (_lock_Shaderbdle)
            {
                //EldenRingShaderbdle = null;
                cache_Metaparam?.Clear();
            }
        }

        private static object _lock_Shaderbdle = new object();
        private static Dictionary<string, EldenRingMetaparamInfo> cache_Metaparam = new Dictionary<string, EldenRingMetaparamInfo>();
        public static EldenRingMetaparamInfo LoadMetaParam(string shaderName)
        {
            EldenRingMetaparamInfo result = null;
            lock (_lock_Shaderbdle)
            {
                

                if (cache_Metaparam.ContainsKey(shaderName))
                {
                    result = cache_Metaparam[shaderName];
                }
                else
                {
                    var EldenRingShaderbdle = LoadBinder($@"/shader/shaderbdle.shaderbdlebnd.dcx");

                    var matchingFile = EldenRingShaderbdle.Files.FirstOrDefault(x => Utils.GetShortIngameFileName(x.Name) == shaderName);
                    if (matchingFile != null)
                    {
                        var innerShaderbdle = BND4.Read(matchingFile.Bytes);
                        var matchingMetaparamFile = innerShaderbdle.Files.FirstOrDefault(x => x.Name.ToLower().EndsWith(".metaparam"));
                        var metaparamInfo = ReadMetaParam(matchingMetaparamFile.Bytes);
                        cache_Metaparam.Add(shaderName, metaparamInfo);
                        result = metaparamInfo;
                    }
                }

                
            }

            return result;
        }

        public static FlverMaterialDefInfo FromFile(string name, byte[] bytes, SoulsGames game)
        {
            if (bytes.Length == 0)
                return null;

            FlverMaterialDefInfo result = null;

            if (game == SoulsGames.ER)
            {
                var br = new BinaryReaderEx(false, bytes);

                result = new FlverMaterialDefInfo();
                result.Name = name;

                br.Position = 8;
                long shaderNameOffset = br.ReadInt64();
                br.Position = shaderNameOffset;
                string shaderName = br.ReadUTF16();
                result.ShaderName = Utils.GetShortIngameFileName(shaderName);

                var metaparam = LoadMetaParam(result.ShaderName);


                br.Position = 0x1C;
                int paramCount = br.ReadInt32();
                int samplerCount = br.ReadInt32();

                for (int i = 0; i < paramCount; i++)
                {
                    br.Position = 0x38 + (0x28 * i);
                    long nameOffset = br.ReadInt64();
                    long valueOffset = br.ReadInt64();
                    br.ReadInt32(); //key
                    MatbinValueType valueType = (MatbinValueType)br.ReadInt32();
                    br.Position = nameOffset;
                    string paramName = br.ReadUTF16();
                    br.Position = valueOffset;
                    object paramValue = null;
                    if (valueType == MatbinValueType.Byte)
                        paramValue = br.ReadByte();
                    else if (valueType == MatbinValueType.Int)
                        paramValue = br.ReadInt32();
                    else if (valueType == MatbinValueType.Int2)
                        paramValue = br.ReadInt32s(2);
                    else if (valueType == MatbinValueType.Float)
                        paramValue = br.ReadSingle();
                    else if (valueType == MatbinValueType.Float2)
                        paramValue = br.ReadSingles(2);
                    else if (valueType == MatbinValueType.Float5 || valueType == MatbinValueType.Float5B)
                        paramValue = br.ReadSingles(5);
                    else if (valueType == MatbinValueType.Float4)
                        paramValue = br.ReadSingles(4);
                    else
                        throw new NotImplementedException();

                    if (!result.ShaderParameters.ContainsKey(paramName))
                        result.ShaderParameters.Add(paramName, paramValue);
                    else
                        result.ShaderParameters[paramName] = paramValue;

                    if (!result.ShaderParameters_AreUsedByDSAS.ContainsKey(paramName))
                        result.ShaderParameters_AreUsedByDSAS.Add(paramName, false);
                }

                for (int i = 0; i < samplerCount; i++)
                {
                    br.Position = 0x38 + (0x28 * paramCount) + (i * 0x30);
                    long typeOffset = br.ReadInt64();
                    long pathOffset = br.ReadInt64();
                    br.Position += 4;
                    float uvScaleX = br.ReadSingle();
                    float uvScaleY = br.ReadSingle();
                    br.Position = typeOffset;
                    string typeStr = br.ReadUTF16();
                    br.Position = pathOffset;
                    string pathStr = br.ReadUTF16();

                    // Undefined means default
                    if (uvScaleX == 0)
                        uvScaleX = 1;

                    if (uvScaleY == 0)
                        uvScaleY = 1;

                    var metaparamEntry = metaparam?.SamplerInfos?.FirstOrDefault(x => x.Name == typeStr);
                    string defaultTexPath = null;
                    int uvGroupNo = -1;
                    if (metaparamEntry != null)
                    {
                        defaultTexPath = metaparamEntry.DefaultTexturePath;
                        uvGroupNo = (metaparamEntry.GroupName.Length >= 7) ? int.Parse(metaparamEntry.GroupName.Substring(6)) : -1;
                    }

                    result.SamplerConfigs.Add(typeStr, new SamplerConfig()
                    {
                        Name = typeStr,
                        TexPath = pathStr,
                        UVScale = new Vector2(uvScaleX, uvScaleY),
                        DefaultTexPath = defaultTexPath,
                        UVGroup = uvGroupNo,
                    });
                }
                //return result;
            }
            else// if (game == SoulsGames.DS1 || game == SoulsGames.DS1R || game == SoulsGames.BB || game == SoulsGames.DS3 || game == SoulsGames.SDT || game == SoulsGames.DES)
            {
                var mtd = MTD.Read(bytes);
                result = new FlverMaterialDefInfo();
                result.Name = name;
                result.ShaderName = Utils.GetShortIngameFileName(mtd.ShaderPath);
                foreach (var tx in mtd.Textures)
                {
                    var uvScaleX = 1f;
                    var uvScaleY = 1f;
                    if (tx.UnkFloats.Count == 2)
                    {
                        if (tx.UnkFloats[0] != 0)
                            uvScaleX = tx.UnkFloats[0];
                        if (tx.UnkFloats[1] != 0)
                            uvScaleY = tx.UnkFloats[1];
                    }

                    result.SamplerConfigs.Add(tx.Type, new SamplerConfig()
                    {
                        Name = tx.Type,
                        TexPath = tx.Path,
                        UVScale = new Vector2(uvScaleX, uvScaleY),
                    });
                }

                foreach (var p in mtd.Params)
                {
                    if (!result.ShaderParameters.ContainsKey(p.Name))
                        result.ShaderParameters.Add(p.Name, p.Value);
                    else
                        result.ShaderParameters[p.Name] = p.Value;

                    if (!result.ShaderParameters_AreUsedByDSAS.ContainsKey(p.Name))
                        result.ShaderParameters_AreUsedByDSAS.Add(p.Name, false);
                }

                //return result;
            }

            //if (result != null)
            //{
            //    var shaderbdle = LookupShaderBdle(result.ShaderName);
            //    if (shaderbdle != null)
            //    {
            //        var shortShaderName = Utils.GetShortIngameFileName(result.ShaderName);
            //        var vpoName = $"{shortShaderName}_Fwd.vpo";
            //        var ppoName = $"{shortShaderName}_Fwd.ppo";
            //        result.PPO = shaderbdle.Files.FirstOrDefault(f => f.Name == ppoName)?.Bytes;
            //        result.VPO = shaderbdle.Files.FirstOrDefault(f => f.Name == vpoName)?.Bytes;

            //        if (result.PPO == null || result.VPO == null)
            //        {
            //            Console.WriteLine("breakpoint");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine("breakpoint");
            //    }
                

            //}
            //else
            //{
            //    Console.WriteLine("breakpoint");
            //}

            return result;
        }

        public class SamplerConfig
        {
            public string Name;
            public string TexPath;
            public string DefaultTexPath;
            public int UVGroup;
            public Vector2 UVScale = Vector2.One;
        }
    }
}
