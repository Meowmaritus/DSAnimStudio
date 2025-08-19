using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoulsAssetPipeline;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class FlverMaterial : IHighlightableThing, IDisposable
    {
        public string Name;
        public List<Sampler> Samplers = new List<Sampler>();

        bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                if (Samplers != null)
                {
                    foreach (var s in Samplers)
                    {
                        // Only dispose if this texture is somehow not in the texture pool
                        // because otherwise the texture pool will handle disposal.
                        if (s.Tex != null && !zzz_DocumentManager.CurrentDocument.TexturePool.AnyTexturesMatch(s.Tex))
                            s.Tex?.Dispose();
                    }
                    
                    Samplers = null;
                }
                _disposed = true;
            }
        }

        public static Vector4 GetDefaultChrCustomizeColor(ChrCustomizeTypes type)
        {
            if (type == ChrCustomizeTypes.Skin)
                return Vector4.One;
            else if (type == ChrCustomizeTypes.Eyes)
                return new Vector4(0, 1, 1, 1);
            else
                return new Vector4(0, 0, 0, 1);
        }

        public enum TextureTypes
        {
            None = 0,

            Albedo1 = 1,
            Specular1 = 2,
            Normal1 = 3,
            Shininess1 = 4,
            Emissive1 = 5,
            
            Blend1To2 = 10,
            Blend3Mask = 11,

            Albedo2 = 101,
            Specular2 = 102,
            Normal2 = 103,
            Shininess2 = 104,
            Emissive2 = 105,
        }

        public enum ChrCustomizeTypes
        {
            None,
            Skin,
            Eyes,
            Hair,
            FacialHair,
            Eyebrows,
            BodyHair,
            BodyHair_Base,
        }

        public bool IsVisible = true;
        //public bool IsSoloVisible = false;

        public string MtdName;

        public string ShaderConfigName;
        public FlverShaderConfig ShaderConfig;// => FlverShaderConfig.GetShaderConfig(ShaderConfigName);

        public FlverMaterialDefInfo MtdInfo = new FlverMaterialDefInfo();

        public Sampler GetSampler(string name)
        {
            return Samplers.FirstOrDefault(s => s.Name == name);
        }

        public FlverShaderConfig.SamplerConfig GetSamplerConfig(string name)
        {
            return ShaderConfig.SamplerConfigs.FirstOrDefault(s => s.Name == name);
        }

        public void TryLoadGlobalShaderConfig()
        {
            var globalConfigOverride = FlverShaderConfig.LoadGlobalDefault(ShaderConfigName);
            if (globalConfigOverride != null)
                ShaderConfig = globalConfigOverride.GetClone();
        }

        public GFXDrawStep DrawStep;

        public bool IsUndefinedMetallic;
        public bool IsUndefinedBlendMask;

        public FlverShadingModes ShadingMode = FlverShadingModes.PBR_GLOSS;

        public NewDebugTypes NewDebugType = NewDebugTypes.None;

        public PtdeMtdTypes PtdeMtdType = PtdeMtdTypes.PTDE_MTD_TYPE_DEFAULT;
        public bool Ptde_UsesDetailBump = false;

        public int ModelMaskIndex;

        public Vector2 UVTileMult1 = Vector2.One;
        public Vector2 UVTileMult2 = Vector2.One;
        public Vector2 UVTileMult3 = Vector2.One;
        public Vector2 UVTileMult4 = Vector2.One;
        public Vector2 UVTileMult5 = Vector2.One;
        public Vector2 UVTileMult6 = Vector2.One;
        public Vector2 UVTileMult7 = Vector2.One;
        public Vector2 UVTileMult8 = Vector2.One;

        public Vector2 GetUVTileMult(int uvIndex)
        {
            switch (uvIndex)
            {
                case 0: return UVTileMult1;
                case 1: return UVTileMult2;
                case 2: return UVTileMult3;
                case 3: return UVTileMult4;
                case 4: return UVTileMult5;
                case 5: return UVTileMult6;
                case 6: return UVTileMult7;
                case 7: return UVTileMult8;
            }
            return Vector2.One;
        }
        
        public Vector2 DebugUVOffset = Vector2.Zero;





        public Texture2D DefaultDiffuseTex => (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS ?
                Main.WHITE_TEXTURE : Main.DEFAULT_TEXTURE_DIFFUSE);
        public Texture2D DefaultNormalTex => (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS ?
                Main.DEFAULT_TEXTURE_NORMAL_DS2 : Main.DEFAULT_TEXTURE_NORMAL);

        public Texture2D DefaultSpecularTex => (ShaderConfig.IsMetallic ? (Main.DEFAULT_TEXTURE_METALLIC) : (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS ?
                Main.DEFAULT_TEXTURE_SPECULAR_DS2 : Main.DEFAULT_TEXTURE_SPECULAR));

        public Texture2D DefaultShininessTex => Main.DEFAULT_TEXTURE_EMISSIVE;

        public Texture2D DefaultEmissiveTex => Main.DEFAULT_TEXTURE_EMISSIVE;

        public Texture2D DefaultBlend1To2Tex => Main.BLACK_TEXTURE;
        public Texture2D DefaultMask3Tex => Main.BLACK_TEXTURE;

        private static int Util_GetModelMaskIndex(string matName)
        {
            if (string.IsNullOrEmpty(matName))
                return -1;

            int firstHashtag = matName.IndexOf("#");
            if (firstHashtag == -1)
                return -1;
            int secondHashtagSearchStart = firstHashtag + 1;
            int secondHashtag = matName.Substring(secondHashtagSearchStart).IndexOf("#");
            if (secondHashtag == -1)
                return -1;
            else
                secondHashtag += secondHashtagSearchStart;

            string maskText = matName.Substring(secondHashtagSearchStart, secondHashtag - secondHashtagSearchStart);

            if (int.TryParse(maskText, out int mask))
                return mask;
            else
                return -1;
        }

        public void Init(NewMesh parent, FLVER0 flvr, FLVER0.Material mat)
        {
            MtdName = mat.MTD;
            Name = mat.Name;

            //MtdInfo = FlverMaterialDefInfo.Lookup(MtdName);
            ////ShaderConfig = new FlverShaderConfig()
            ////{
            ////    Name = MtdInfo.ShaderName,
            ////};
            //ShaderConfigName = MtdInfo.ShaderName;
            //ShaderConfig = new FlverShaderConfig();
            MtdInfo = FlverMaterialDefInfo.Lookup(MtdName);
            if (MtdInfo != null)
            {
                //ShaderConfig = new FlverShaderConfig()
                //{
                //    Name = MtdInfo.ShaderName,
                //};
                ShaderConfigName = MtdInfo.ShaderName;
                ShaderConfig = FlverShaderConfig.GetDefaultConfigForGame(zzz_DocumentManager.CurrentDocument.GameRoot.GameType);
            }
            else
            {
                MtdInfo = new FlverMaterialDefInfo();
            }

            ModelMaskIndex = Util_GetModelMaskIndex(Name);

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3 or SoulsAssetPipeline.SoulsGames.BB 
                or SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6 or SoulsAssetPipeline.SoulsGames.DS1R)
            {
                ShadingMode = FlverShadingModes.PBR_GLOSS;
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
            {
                ShadingMode = FlverShadingModes.CLASSIC_DIFFUSE_PTDE;
            }
            else
            {
                ShadingMode = FlverShadingModes.PBR_GLOSS;
            }

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.BB)
            {
                ShaderConfig.UseShininessMap = true;
            }

            var shortMaterialName = Utils.GetFileNameWithoutDirectoryOrExtension(mat.MTD).ToLower();
            if (shortMaterialName.EndsWith("_alp") ||
                shortMaterialName.Contains("_edge") ||
                shortMaterialName.Contains("_e_") ||
                shortMaterialName.EndsWith("_e") ||
                shortMaterialName.Contains("_decal") ||
                shortMaterialName.Contains("_cloth") ||
                shortMaterialName.Contains("_al") ||
                shortMaterialName.Contains("blendopacity"))
            {
                DrawStep = GFXDrawStep.AlphaEdge;
            }
            else
            {
                DrawStep = GFXDrawStep.Opaque;
            }

            if (shortMaterialName.Contains("metal"))
            {
                PtdeMtdType = PtdeMtdTypes.PTDE_MTD_TYPE_METAL;
            }
            else if (shortMaterialName.Contains("wet"))
            {
                PtdeMtdType = PtdeMtdTypes.PTDE_MTD_TYPE_WET;
            }
            else if (shortMaterialName.Contains("dull"))
            {
                PtdeMtdType = PtdeMtdTypes.PTDE_MTD_TYPE_DULL;
            }

            ShaderConfig.IsShaderDoubleFaceCloth = shortMaterialName.Contains("_df_");
            ShaderConfig.IsDS3Veil = shortMaterialName.Contains("veil");
            ShaderConfig.IsMetallic = false;
            ShaderConfig.IsDS2EmissiveFlow = false;



            int index_Albedo = 0;
            int index_Specular = 0;
            int index_Normal = 0;
            int index_Shininess = 0;
            int index_Emissive = 0;
            int index_BlendMask = 0;
            int index_BlendMask3 = 0;

            foreach (var matParam in mat.Textures)
            {
                var paramNameCheck = matParam.Type?.ToUpper() ?? "null";
                string shortTexPath = Utils.GetShortIngameFileName(matParam.Path).ToLower();

                Vector2 texScaleVal = Vector2.One;// new Vector2(matParam.Scale.X, matParam.Scale.Y);

                if (paramNameCheck == "G_DETAILBUMPMAP")
                {
                    texScaleVal *= MtdInfo.GetFloat2ParameterOrDefault("g_DetailBump_UVScale", Vector2.One);
                    IsUndefinedBlendMask = true;
                    Ptde_UsesDetailBump = true;
                    //ShaderConfig.UndefinedBlendMaskValue = Ptde_DetailBump_BumpPower;
                }


                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var mtdTex = FlverMaterialDefInfo.LookupSampler(mat.MTD, matParam.Type);
                    if (mtdTex != null)
                    {
                        if (mtdTex.TexPath != null)
                        {
                            shortTexPath = Utils.GetShortIngameFileName(mtdTex.TexPath).ToLower();
                        }
                        texScaleVal *= mtdTex.UVScale;

                        if (string.IsNullOrWhiteSpace(shortTexPath))
                        {
                            shortTexPath = Utils.GetShortIngameFileName(matParam.Path).ToLower();
                        }
                    }
                }

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS && paramNameCheck.Contains("FLOWMAP"))
                {
                    ShaderConfig.IsDS2EmissiveFlow = true;
                }

                TextureTypes texType = TextureTypes.None;
                int uvIndex = 0;

                var smp = new Sampler();

                if (paramNameCheck.Contains("METALLIC"))
                {
                    ShaderConfig.IsMetallic = true;
                }

                if (!string.IsNullOrWhiteSpace(shortTexPath))
                {
                    smp.TexPath = shortTexPath;
                    smp.UVScale = texScaleVal;

                    if ((paramNameCheck.Contains("DIFFUSE") || paramNameCheck.Contains("ALBEDO")) && index_Albedo < 2)
                    {
                        texType = (index_Albedo == 0) ? TextureTypes.Albedo1 : TextureTypes.Albedo2;

                        // Hotfix for Elden Ring fur materials
                        if (smp.TexPath.Contains("furblurnoise"))
                        {
                            texType = TextureTypes.None;
                        }
                        else
                        {
                            index_Albedo++;
                        }



                    }
                    else if ((paramNameCheck.Contains("SPECULAR") || paramNameCheck.Contains("REFLECTANCE") || paramNameCheck.Contains("METALLIC")) && index_Specular < 2)
                    {
                        texType = (index_Specular == 0) ? TextureTypes.Specular1 : TextureTypes.Specular2;
                        index_Specular++;
                    }
                    else if (((paramNameCheck.Contains("BUMPMAP") && !paramNameCheck.Contains("DETAILBUMP"))
                        || paramNameCheck.Contains("NORMALMAP")) && index_Normal < 2)
                    {
                        texType = (index_Normal == 0) ? TextureTypes.Normal1 : TextureTypes.Normal2;
                        index_Normal++;
                    }
                    else if (paramNameCheck.Contains("EMISSIVE") && index_Emissive < 2)
                    {
                        texType = (index_Emissive == 0) ? TextureTypes.Emissive1 : TextureTypes.Emissive2;
                        index_Emissive++;
                    }
                    else if (paramNameCheck.Contains("SHININESS") && index_Shininess < 2)
                    {
                        texType = (index_Shininess == 0) ? TextureTypes.Shininess1 : TextureTypes.Shininess2;
                        index_Shininess++;
                    }
                    else if ((paramNameCheck.Contains("BLENDMASK") || paramNameCheck.Contains("MASK1")) && index_BlendMask < 1)
                    {
                        texType = TextureTypes.Blend1To2;
                        index_BlendMask++;
                    }
                    else if ((paramNameCheck.Contains("MASK3")) && index_BlendMask3 < 1)
                    {
                        texType = TextureTypes.Blend3Mask;
                        index_BlendMask3++;
                    }
                    else
                    {
                        Console.WriteLine($"\nUnrecognized Material Param:\n    [{matParam.Type}]\n    [{matParam.Path}]\n");
                    }
                }

                if ((int)texType >= (int)TextureTypes.Albedo2)
                {
                    uvIndex = 2;
                }
                else if (texType == TextureTypes.Blend1To2)
                {
                    uvIndex = 1;
                }

                //smp.TextureType = texType;
                smp.Name = matParam.Type;
                //smp.UVIndex = uvIndex;

                if (paramNameCheck == "G_DETAILBUMPMAP")
                {
                    texType = TextureTypes.Normal2;
                }

                ShaderConfig.SamplerConfigs.Add(new FlverShaderConfig.SamplerConfig()
                {
                    Name = matParam.Type,
                    TexType = texType,
                    UVIndex = uvIndex,
                });

                Samplers.Add(smp);

                // Alternate material params that work as diffuse
            }

            var samplerConfigs = ShaderConfig.SamplerConfigs;

            // Elden Ring Fur Hotfix
            var mtdCheck = MtdName.ToLower();
            if ((mtdCheck.Contains("fur") || mtdCheck.Contains("hair")) && zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesMetallicShaders)
            {

                var map1Samplers = samplerConfigs.Where(x => (int)x.TexType < (int)TextureTypes.Blend1To2);
                var maskSamplers = samplerConfigs.Where(x => (int)x.TexType == (int)TextureTypes.Blend1To2);
                var map2Samplers = samplerConfigs.Where(x => (int)x.TexType > (int)TextureTypes.Blend1To2);
                foreach (var s in map1Samplers)
                    if (map2Samplers.Any(x => x.TexType == (TextureTypes)((int)s.TexType + 100)))
                        s.TexType = TextureTypes.None;
                foreach (var s in map2Samplers)
                    s.TexType = (TextureTypes)((int)s.TexType - 100);
                foreach (var s in maskSamplers)
                    s.TexType = TextureTypes.None;

            }

            var mtd = FlverMaterialDefInfo.Lookup(MtdName);
            UVTileMult1 = mtd.GetFloat2ParameterOrDefault($"group_1_CommonUV-UVParam", Vector2.One);
            UVTileMult2 = mtd.GetFloat2ParameterOrDefault($"group_2_CommonUV-UVParam", Vector2.One);
            UVTileMult3 = mtd.GetFloat2ParameterOrDefault($"group_3_CommonUV-UVParam", Vector2.One);
            UVTileMult4 = mtd.GetFloat2ParameterOrDefault($"group_4_CommonUV-UVParam", Vector2.One);
            UVTileMult5 = mtd.GetFloat2ParameterOrDefault($"group_5_CommonUV-UVParam", Vector2.One);
            UVTileMult6 = mtd.GetFloat2ParameterOrDefault($"group_6_CommonUV-UVParam", Vector2.One);
            UVTileMult7 = mtd.GetFloat2ParameterOrDefault($"group_7_CommonUV-UVParam", Vector2.One);
            UVTileMult8 = mtd.GetFloat2ParameterOrDefault($"group_8_CommonUV-UVParam", Vector2.One);


            foreach (var s in ShaderConfig.SamplerConfigs)
            {
                if ((int)s.TexType >= (int)TextureTypes.Albedo2)
                {
                    s.UVGroup = 1;
                }
                else
                {
                    s.UVGroup = 0;
                }
            }


            TryLoadGlobalShaderConfig();
            if (ShaderConfig.IsMetallic && !ShaderConfig.SamplerConfigs.Any(s => (s.TexType == TextureTypes.Specular1 || s.TexType == TextureTypes.Specular2)))
            {
                IsUndefinedMetallic = true;
            }
            TryToLoadTextures();

            foreach (var s in ShaderConfig.SamplerConfigs)
            {
                var info = FlverMaterialDefInfo.LookupSampler(MtdName, s.Name);
                if (info != null)
                {
                    if (info.UVGroup >= 1)
                        s.UVGroup = info.UVGroup - 1;
                    if (info.DefaultTexPath != null)
                    {
                        s.DefaultTexPath = info.DefaultTexPath;
                        s.DefaultTex = zzz_DocumentManager.CurrentDocument.TexturePool.FetchTexture2D(info.DefaultTexPath);
                    }
                }
            }
        }

        public void Init(NewMesh parent, FLVER2 flvr, FLVER2.Material mat)
        {
            MtdName = mat.MTD;
            Name = mat.Name;

            MtdInfo = FlverMaterialDefInfo.Lookup(MtdName);
            if (MtdInfo != null)
            {
                //ShaderConfig = new FlverShaderConfig()
                //{
                //    Name = MtdInfo.ShaderName,
                //};
                ShaderConfigName = MtdInfo.ShaderName;
                ShaderConfig = FlverShaderConfig.GetDefaultConfigForGame(zzz_DocumentManager.CurrentDocument.GameRoot.GameType);
            }
            else
            {
                MtdInfo = new FlverMaterialDefInfo();
            }

            ModelMaskIndex = Util_GetModelMaskIndex(Name);

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.DS3 or SoulsAssetPipeline.SoulsGames.BB
                or SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6 or SoulsAssetPipeline.SoulsGames.DS1R)
            {
                ShadingMode = FlverShadingModes.PBR_GLOSS;
            }
            else if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
            {
                ShadingMode = FlverShadingModes.CLASSIC_DIFFUSE_PTDE;
            }
            else
            {
                ShadingMode = FlverShadingModes.PBR_GLOSS;
            }

            if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.BB)
            {
                ShaderConfig.UseShininessMap = true;
            }

            var shortMaterialName = Utils.GetFileNameWithoutDirectoryOrExtension(mat.MTD).ToLower();
            if (shortMaterialName.EndsWith("_alp") ||
                shortMaterialName.Contains("_edge") ||
                shortMaterialName.Contains("_e_") ||
                shortMaterialName.EndsWith("_e") ||
                shortMaterialName.Contains("_decal") ||
                shortMaterialName.Contains("_cloth") ||
                shortMaterialName.Contains("_al") ||
                shortMaterialName.Contains("blendopacity"))
            {
                DrawStep = GFXDrawStep.AlphaEdge;
            }
            else
            {
                DrawStep = GFXDrawStep.Opaque;
            }

            if (shortMaterialName.Contains("metal"))
            {
                PtdeMtdType = PtdeMtdTypes.PTDE_MTD_TYPE_METAL;
            }
            else if (shortMaterialName.Contains("wet"))
            {
                PtdeMtdType = PtdeMtdTypes.PTDE_MTD_TYPE_WET;
            }
            else if (shortMaterialName.Contains("dull"))
            {
                PtdeMtdType = PtdeMtdTypes.PTDE_MTD_TYPE_DULL;
            }

            if (shortMaterialName.Contains("_df_"))
                ShaderConfig.IsShaderDoubleFaceCloth = true;
            if (shortMaterialName.Contains("veil"))
                ShaderConfig.IsDS3Veil = true;
            //ShaderConfig.IsMetallic = false;
            //ShaderConfig.IsDS2EmissiveFlow = false;

            

            int index_Albedo = 0;
            int index_Specular = 0;
            int index_Normal = 0;
            int index_Shininess = 0;
            int index_Emissive = 0;
            int index_BlendMask = 0;
            int index_BlendMask3 = 0;

            foreach (var matParam in mat.Textures)
            {
                var paramNameCheck = matParam.Type.ToUpper();
                string shortTexPath = Utils.GetShortIngameFileName(matParam.Path).ToLower();

                Vector2 texScaleVal = new Vector2(matParam.Scale.X, matParam.Scale.Y);

                if (paramNameCheck == "G_DETAILBUMPMAP")
                {
                    texScaleVal *= MtdInfo.GetFloat2ParameterOrDefault("g_DetailBump_UVScale", Vector2.One);
                    IsUndefinedBlendMask = true;
                    Ptde_UsesDetailBump = true;
                    //ShaderConfig.UndefinedBlendMaskValue = Ptde_DetailBump_BumpPower;

                }


                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.ER or SoulsAssetPipeline.SoulsGames.ERNR or SoulsAssetPipeline.SoulsGames.AC6)
                {
                    var mtdTex = FlverMaterialDefInfo.LookupSampler(mat.MTD, matParam.Type);
                    if (mtdTex != null)
                    {
                        if (mtdTex.TexPath != null)
                        {
                            shortTexPath = Utils.GetShortIngameFileName(mtdTex.TexPath).ToLower();
                        }
                        texScaleVal *= mtdTex.UVScale;

                        if (string.IsNullOrWhiteSpace(shortTexPath))
                        {
                            shortTexPath = Utils.GetShortIngameFileName(matParam.Path).ToLower();
                        }
                    }
                }

                if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS && paramNameCheck.Contains("FLOWMAP"))
                {
                    ShaderConfig.IsDS2EmissiveFlow = true;
                }

                TextureTypes texType = TextureTypes.None;
                int uvIndex = 0;

                var smp = new Sampler();

                if (paramNameCheck.Contains("METALLIC"))
                {
                    ShaderConfig.IsMetallic = true;
                }

                bool isIgnoreCompletely = false;

                

                if (!string.IsNullOrWhiteSpace(shortTexPath))
                {
                    smp.TexPath = shortTexPath;

                    if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType is SoulsGames.AC6 && shortTexPath.StartsWith("aet"))
                    {
                        if (int.TryParse(shortTexPath.Substring(3, 3), out int idA))
                        {
                            if (int.TryParse(shortTexPath.Substring(7, 3), out int idB))
                            {
                                zzz_DocumentManager.CurrentDocument.TexturePool.AddAC6AET(idA, idB);
                            }
                        }
                    }

                    smp.UVScale = texScaleVal;

                    if (Utils.GetShortIngameFileName(shortTexPath).ToLower() is "systex_dummyburn_em" or "systex_dummyburn_m")
                    {
                        isIgnoreCompletely = true;
                    }

                    if (!isIgnoreCompletely)
                    {
                        if ((paramNameCheck.Contains("DIFFUSE") || paramNameCheck.Contains("ALBEDO")) && index_Albedo < 2)
                        {
                            texType = (index_Albedo == 0) ? TextureTypes.Albedo1 : TextureTypes.Albedo2;

                            // Hotfix for Elden Ring fur materials
                            if (smp.TexPath.Contains("furblurnoise"))
                            {
                                texType = TextureTypes.None;
                            }
                            else
                            {
                                index_Albedo++;
                            }



                        }
                        else if ((paramNameCheck.Contains("SPECULAR") || paramNameCheck.Contains("REFLECTANCE") || paramNameCheck.Contains("METALLIC")) && index_Specular < 2)
                        {
                            texType = (index_Specular == 0) ? TextureTypes.Specular1 : TextureTypes.Specular2;
                            index_Specular++;
                        }
                        else if (((paramNameCheck.Contains("BUMPMAP") && !paramNameCheck.Contains("DETAILBUMP"))
                            || paramNameCheck.Contains("NORMALMAP")) && index_Normal < 2)
                        {
                            texType = (index_Normal == 0) ? TextureTypes.Normal1 : TextureTypes.Normal2;
                            index_Normal++;
                        }
                        else if (paramNameCheck.Contains("EMISSIVE") && index_Emissive < 2)
                        {
                            texType = (index_Emissive == 0) ? TextureTypes.Emissive1 : TextureTypes.Emissive2;
                            index_Emissive++;
                        }
                        else if (paramNameCheck.Contains("SHININESS") && index_Shininess < 2)
                        {
                            texType = (index_Shininess == 0) ? TextureTypes.Shininess1 : TextureTypes.Shininess2;
                            index_Shininess++;
                        }
                        else if ((paramNameCheck.Contains("BLENDMASK") || paramNameCheck.Contains("MASK1")) && index_BlendMask < 1)
                        {
                            texType = TextureTypes.Blend1To2;
                            index_BlendMask++;
                        }
                        else if (paramNameCheck.Contains("MASK3") && index_BlendMask3 < 1)
                        {
                            texType = TextureTypes.Blend3Mask;
                            index_BlendMask3++;
                        }
                        else
                        {
                            Console.WriteLine($"\nUnrecognized Material Param:\n    [{matParam.Type}]\n    [{matParam.Path}]\n");
                        }
                    }
                }

                if ((int)texType >= (int)TextureTypes.Albedo2)
                {
                    uvIndex = 2;
                }
                else if (texType == TextureTypes.Blend1To2)
                {
                    uvIndex = 1;
                }

                //smp.TextureType = texType;
                smp.Name = matParam.Type;
                //smp.UVIndex = uvIndex;


                if (paramNameCheck == "G_DETAILBUMPMAP")
                {
                    texType = TextureTypes.Normal2;
                }

                if (paramNameCheck == "G_SCATTERINGMASKTEXTURE")
                {
                    texType = TextureTypes.Blend3Mask;
                }

                var sampCfg = new FlverShaderConfig.SamplerConfig()
                {
                    Name = matParam.Type,
                    TexType = texType,
                    UVIndex = uvIndex,
                };

                if (isIgnoreCompletely)
                {
                    sampCfg.TexType = TextureTypes.None;
                }

                ShaderConfig.SamplerConfigs.Add(sampCfg);

                Samplers.Add(smp);

                // Alternate material params that work as diffuse
            }

            var samplerConfigs = ShaderConfig.SamplerConfigs;

            // Elden Ring Fur Hotfix
            var mtdCheck = MtdName.ToLower();
            if ((mtdCheck.Contains("fur") || mtdCheck.Contains("hair")) && zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesMetallicShaders)
            {
                
                var map1Samplers = samplerConfigs.Where(x => (int)x.TexType < (int)TextureTypes.Blend1To2);
                var maskSamplers = samplerConfigs.Where(x => (int)x.TexType == (int)TextureTypes.Blend1To2);
                var map2Samplers = samplerConfigs.Where(x => (int)x.TexType > (int)TextureTypes.Blend1To2);
                foreach (var s in map1Samplers)
                    if (map2Samplers.Any(x => x.TexType == (TextureTypes)((int)s.TexType + 100)))
                        s.TexType = TextureTypes.None;
                foreach (var s in map2Samplers)
                    s.TexType = (TextureTypes)((int)s.TexType - 100);
                foreach (var s in maskSamplers)
                    s.TexType = TextureTypes.None;
                
            }

            var mtd = FlverMaterialDefInfo.Lookup(MtdName);
            UVTileMult1 = mtd.GetFloat2ParameterOrDefault($"group_1_CommonUV-UVParam", Vector2.One);
            UVTileMult2 = mtd.GetFloat2ParameterOrDefault($"group_2_CommonUV-UVParam", Vector2.One);
            UVTileMult3 = mtd.GetFloat2ParameterOrDefault($"group_3_CommonUV-UVParam", Vector2.One);
            UVTileMult4 = mtd.GetFloat2ParameterOrDefault($"group_4_CommonUV-UVParam", Vector2.One);
            UVTileMult5 = mtd.GetFloat2ParameterOrDefault($"group_5_CommonUV-UVParam", Vector2.One);
            UVTileMult6 = mtd.GetFloat2ParameterOrDefault($"group_6_CommonUV-UVParam", Vector2.One);
            UVTileMult7 = mtd.GetFloat2ParameterOrDefault($"group_7_CommonUV-UVParam", Vector2.One);
            UVTileMult8 = mtd.GetFloat2ParameterOrDefault($"group_8_CommonUV-UVParam", Vector2.One);


            foreach (var s in ShaderConfig.SamplerConfigs)
            {
                if ((int)s.TexType >= (int)TextureTypes.Albedo2)
                {
                    s.UVGroup = 1;
                }
                else
                {
                    s.UVGroup = 0;
                }
            }


            TryLoadGlobalShaderConfig();
            if (ShaderConfig.IsMetallic && !ShaderConfig.SamplerConfigs.Any(s => (s.TexType == TextureTypes.Specular1 || s.TexType == TextureTypes.Specular2)))
            {
                IsUndefinedMetallic = true;
            }
            TryToLoadTextures();

            foreach (var s in ShaderConfig.SamplerConfigs)
            {
                var info = FlverMaterialDefInfo.LookupSampler(MtdName, s.Name);
                if (info != null)
                {
                    if (info.UVGroup >= 1)
                        s.UVGroup = info.UVGroup - 1;
                    if (info.DefaultTexPath != null)
                    {
                        s.DefaultTexPath = info.DefaultTexPath;
                        s.DefaultTex = zzz_DocumentManager.CurrentDocument.TexturePool.FetchTexture2D(info.DefaultTexPath);
                    }
                }
            }
        }

        public void TryToLoadTextures()
        {
            bool hasAnyMetallic = false;
            bool hasAnyBlendMask = false;
            foreach (var s in Samplers)
            {
                if (!string.IsNullOrWhiteSpace(s.TexPath))
                {
                    s.Tex = zzz_DocumentManager.CurrentDocument.TexturePool.FetchTexture2D(s.TexPath);
                    var sc = GetSamplerConfig(s.Name);

                    if (s.Tex == null)
                        continue;

                    if (sc == null)
                        continue;

                    if ((sc.TexType is TextureTypes.Specular1 or TextureTypes.Specular2) || sc.Name.ToLower().Contains("metallic"))
                    {
                        hasAnyMetallic = true;
                    }
                    else if (sc.TexType is TextureTypes.Blend1To2 || sc.Name.ToLower().Contains("mask1map"))
                    {
                        hasAnyBlendMask = true;
                    }
                }
            }

            IsUndefinedMetallic = (ShaderConfig.IsMetallic && !hasAnyMetallic);

            IsUndefinedBlendMask = (ShaderConfig.EnableBlendMask && !hasAnyBlendMask);
        }

        public List<string> GetAllTexNamesToLoad()
        {
            var res = new List<string>();
            foreach (var s in Samplers)
            {
                if (!string.IsNullOrWhiteSpace(s.TexPath) && !res.Contains(s.TexPath))
                {
                    res.Add(s.TexPath);
                }
            }
            return res;
        }


        public Texture2D GetMissingTexForBlendType(NewBlendOperations blendOp, Texture2D blendWithTex)
        {
            switch (blendOp)
            {
                case NewBlendOperations.Always0:
                case NewBlendOperations.Always1:
                    return null;// Main.DEFAULT_TEXTURE_MISSING;
                case NewBlendOperations.Lerp:
                    return blendWithTex;
                case NewBlendOperations.Add:
                case NewBlendOperations.Subtract:
                    return Main.BLACK_TEXTURE;
                case NewBlendOperations.Multiply:
                    return Main.GRAY_SRGB_TEXTURE;
                case NewBlendOperations.Divide:
                    return Main.WHITE_TEXTURE;
                case NewBlendOperations.NormalMapBlend:
                    return DefaultNormalTex;
            }

            return null;// Main.DEFAULT_TEXTURE_MISSING;
        }


        public bool ApplyToFlverShader(NewMesh parentMesh, FlverSubmeshRenderer submesh, Model model, Model basePlayerModel)
        {
            var game = zzz_DocumentManager.CurrentDocument.GameRoot.GameType;
            if (GFX.NewDebug_ShowTex_Material != null && GFX.NewDebug_ShowTex_Material != this)
            {
                return false;
            }

            var PtdeSpecularPower = MtdInfo.GetFloatParameterOrDefault("g_SpecularPower", 8.5f);



            var PtdeDiffuseMapColor = MtdInfo.GetFloat3ParameterOrDefault("g_DiffuseMapColor", Vector3.One);
            var PtdeDiffuseMapPower = MtdInfo.GetFloatParameterOrDefault("g_DiffuseMapColorPower", 1);
            var PtdeSpecularMapColor = MtdInfo.GetFloat3ParameterOrDefault("g_SpecularMapColor", Vector3.One);
            var PtdeSpecularMapPower = MtdInfo.GetFloatParameterOrDefault("g_SpecularMapColorPower", 1);

            var Ptde_DetailBump_UVScale = MtdInfo.GetFloat2ParameterOrDefault("g_DetailBump_UVScale", Vector2.One);
            var Ptde_DetailBump_BumpPower = MtdInfo.GetFloatParameterOrDefault("g_DetailBump_BumpPower", 1);

            //if (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsGames.DES)
            //{
            //    //PtdeDiffuseMapColor *= 0.5f;
            //    PtdeSpecularMapColor *= 2;
            //}

            if (game == SoulsGames.DES)
            {
                PtdeDiffuseMapPower *= 2f;
            }

            var newDbgType = NewDebugTypes.None;
            
            if (GFX.NewDebug_ShowTex_Sampler != null && GFX.NewDebug_ShowTex_SamplerConfig != null)
            {
                var s = GFX.NewDebug_ShowTex_Sampler;

                newDbgType = NewDebugTypes.ShowTex;
                GFX.FlverShader.Effect.NewDebug_ShowTex_Tex = s.Tex ?? GFX.NewDebug_ShowTex_SamplerConfig.DefaultTex;
                GFX.FlverShader.Effect.NewDebug_ShowTex_UVIndex = GFX.NewDebug_ShowTex_SamplerConfig.UVIndex;
                GFX.FlverShader.Effect.NewDebug_ShowTex_UVScale = s.UVScale * GetUVTileMult(GFX.NewDebug_ShowTex_SamplerConfig.UVGroup);
                GFX.FlverShader.Effect.NewDebug_ShowTex_ChannelConfig = s.ChannelConfig_ForDebug;
            }
            else
            {
                if (GFX.GlobalNewDebugType == NewDebugTypes.None)
                    newDbgType = NewDebugType;
                else
                    newDbgType = GFX.GlobalNewDebugType;
            }

            bool anyTexDebugUsed = (int)newDbgType > 0 && (int)newDbgType <= 200;

            GFX.FlverShader.Effect.NewDebugType = newDbgType;
            
            var isPtdeOrDes = game == SoulsAssetPipeline.SoulsGames.DS1 || game == SoulsAssetPipeline.SoulsGames.DES;

            GFX.FlverShader.Effect.EmissiveMapMult = ViewportEnvironment.FlverEmissiveMult * ShaderConfig.EmissiveMult;
            GFX.FlverShader.Effect.SpecularPowerMult = GFX.SpecularPowerMult
                * (game == SoulsAssetPipeline.SoulsGames.DS1R ? 0.3f : 1)
                * ShaderConfig.SpecularPowerMult
                //* (isPbrConvertShading ? 0.4f : 1)
                * PtdeSpecularPower
                * (game == SoulsAssetPipeline.SoulsGames.AC6 ? 0.5f : 1)
                * (game is SoulsGames.ER or SoulsGames.ERNR ? 0.0625f : 1)
                * (game is SoulsGames.DES ? 0.5f : 1)
                ;

            if (game is SoulsAssetPipeline.SoulsGames.DS3 or SoulsAssetPipeline.SoulsGames.SDT or SoulsAssetPipeline.SoulsGames.BB && PtdeSpecularPower > 1)
            {
                GFX.FlverShader.Effect.SpecularPowerMult /= 256.0f;
            }

            PtdeSpecularMapColor *= ViewportEnvironment.SpecularMapBrightness;
            PtdeDiffuseMapColor *= ViewportEnvironment.DiffuseMapBrightness;

            if (game is SoulsGames.DES)
            {
                PtdeSpecularMapColor *= 1.4f;
                PtdeDiffuseMapColor *= 1.4f;
            }



            GFX.FlverShader.Effect.Parameters["LdotNPower"].SetValue(GFX.LdotNPower * ShaderConfig.LdotNPowerMult * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6 ? 1 : 1));
            GFX.FlverShader.Effect.DirectLightMult = ViewportEnvironment.FlverDirectLightMult * 1 * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ? 0.6f : 1) * ShaderConfig.DirectLightingMult * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6 ? 0.9f : 1);
            GFX.FlverShader.Effect.IndirectLightMult = ViewportEnvironment.FlverIndirectLightMult * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ? 0.6f : 1) * ShaderConfig.IndirectLightingMult * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6 ? 1.1f : 1);

            
            

            GFX.FlverShader.Effect.DirectDiffuseMult = ViewportEnvironment.DirectDiffuseMult * ShaderConfig.DirectDiffuseMult * (isPtdeOrDes ? 0.85f : 1) * 0.5f;
            GFX.FlverShader.Effect.DirectSpecularMult = ViewportEnvironment.DirectSpecularMult * ShaderConfig.DirectSpecularMult * (isPtdeOrDes ? 0.85f : 1);
            GFX.FlverShader.Effect.IndirectDiffuseMult = ViewportEnvironment.IndirectDiffuseMult * ShaderConfig.IndirectDiffuseMult * 0.5f;
            GFX.FlverShader.Effect.IndirectSpecularMult = ViewportEnvironment.IndirectSpecularMult * ShaderConfig.IndirectSpecularMult * (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.AC6 ? 1 : 1);

            //if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES)
            //{
            //    GFX.FlverShader.Effect.DirectDiffuseMult *= 2;
            //    //GFX.FlverShader.Effect.DirectSpecularMult *= 0.5f;
            //    GFX.FlverShader.Effect.IndirectDiffuseMult *= 2;
            //}

            //GFX.FlverShader.Effect.DirectDiffuseMult /= 2;
            //GFX.FlverShader.Effect.IndirectDiffuseMult /= 2;

            if (isPtdeOrDes)
            {
                GFX.FlverShader.Effect.DirectSpecularMult *= 0.25f * (PtdeSpecularPower / 8.5f);
                GFX.FlverShader.Effect.IndirectSpecularMult *= (0.25f * (PtdeSpecularPower / 8.5f)) / 3f;
                GFX.FlverShader.Effect.DirectDiffuseMult *= 0.5f;
                GFX.FlverShader.Effect.IndirectDiffuseMult *= (0.5f) / 3f;
                //GFX.FlverShader.Effect.Parameters["LdotNPower"].SetValue(GFX.LdotNPower * 4);
                GFX.FlverShader.Effect.SpecularPowerMult *= 0.5f;

                //if (game is SoulsGames.DES)
                //{
                //    PtdeSpecularMapColor *= 0.75f;
                //}
            }

            GFX.FlverShader.Effect.DiffuseMapColor = PtdeDiffuseMapColor;
            GFX.FlverShader.Effect.DiffuseMapColorPower = PtdeDiffuseMapPower;
            GFX.FlverShader.Effect.SpecularMapColor = PtdeSpecularMapColor;
            GFX.FlverShader.Effect.SpecularMapColorPower = PtdeSpecularMapPower;


            //if (PtdeMtdType == PtdeMtdTypes.PTDE_MTD_TYPE_WET)
            //{
            //    //GFX.FlverShader.Effect.Parameters["SpecularPowerMult"].SetValue(GFX.FlverShader.Effect.Parameters["SpecularPowerMult"].GetValueSingle() * 1.15f);
            //    //GFX.FlverShader.Effect.DirectSpecularMult *= 15f;
            //    //GFX.FlverShader.Effect.IndirectSpecularMult *= 15f;
            //    //GFX.FlverShader.Effect.DirectDiffuseMult *= 0.1f;
            //    //GFX.FlverShader.Effect.IndirectDiffuseMult *= 0.1f;
            //    //GFX.FlverShader.Effect.Parameters["LdotNPower"].SetValue(GFX.LdotNPower * 0.25f);
            //    GFX.FlverShader.Effect.DirectDiffuseMult *= 0.25f;
            //    GFX.FlverShader.Effect.IndirectDiffuseMult *= 0.25f;
            //}
            //else if (PtdeMtdType == PtdeMtdTypes.PTDE_MTD_TYPE_DEFAULT)
            //{
            //    //GFX.FlverShader.Effect.Parameters["SpecularPowerMult"].SetValue(GFX.FlverShader.Effect.Parameters["SpecularPowerMult"].GetValueSingle() * 1.15f);
            //    //GFX.FlverShader.Effect.DirectSpecularMult *= 15f;
            //    //GFX.FlverShader.Effect.IndirectSpecularMult *= 15f;
            //    //GFX.FlverShader.Effect.DirectDiffuseMult *= 0.1f;
            //    //GFX.FlverShader.Effect.IndirectDiffuseMult *= 0.1f;
            //    //GFX.FlverShader.Effect.Parameters["LdotNPower"].SetValue(GFX.LdotNPower * 0.25f);
            //    GFX.FlverShader.Effect.DirectDiffuseMult *= 0.25f;
            //    GFX.FlverShader.Effect.IndirectDiffuseMult *= 0.25f;
            //}

            //if (PtdeMtdType == PtdeMtdTypes.PTDE_MTD_TYPE_METAL || PtdeMtdType == PtdeMtdTypes.PTDE_MTD_TYPE_WET)
            //{
            //    GFX.FlverShader.Effect.Parameters["SpecularPowerMult"].SetValue(GFX.FlverShader.Effect.Parameters["SpecularPowerMult"].GetValueSingle() * 1.15f);
            //    //GFX.FlverShader.Effect.DirectDiffuseMult /= PtdeSpecularMapPower;
            //    //GFX.FlverShader.Effect.IndirectDiffuseMult /= PtdeSpecularMapPower;
            //}

            GFX.FlverShader.Effect.NewBlendOperation_Diffuse = ShaderConfig.NewBlendOperation_Diffuse;
            GFX.FlverShader.Effect.NewBlendOperation_Specular = ShaderConfig.NewBlendOperation_Specular;
            GFX.FlverShader.Effect.NewBlendOperation_Normal = ShaderConfig.NewBlendOperation_Normal;
            GFX.FlverShader.Effect.NewBlendOperation_Shininess = ShaderConfig.NewBlendOperation_Shininess;
            GFX.FlverShader.Effect.NewBlendOperation_Emissive = ShaderConfig.NewBlendOperation_Emissive;

            GFX.FlverShader.Effect.NewBlendReverseDir_Diffuse = ShaderConfig.NewBlendReverseDir_Diffuse;
            GFX.FlverShader.Effect.NewBlendReverseDir_Specular = ShaderConfig.NewBlendReverseDir_Specular;
            GFX.FlverShader.Effect.NewBlendReverseDir_Normal = ShaderConfig.NewBlendReverseDir_Normal;
            GFX.FlverShader.Effect.NewBlendReverseDir_Shininess = ShaderConfig.NewBlendReverseDir_Shininess;
            GFX.FlverShader.Effect.NewBlendReverseDir_Emissive = ShaderConfig.NewBlendReverseDir_Emissive;

            GFX.FlverShader.Effect.NewBlendInverseVal_Diffuse = ShaderConfig.NewBlendInverseVal_Diffuse;
            GFX.FlverShader.Effect.NewBlendInverseVal_Specular = ShaderConfig.NewBlendInverseVal_Specular;
            GFX.FlverShader.Effect.NewBlendInverseVal_Normal = ShaderConfig.NewBlendInverseVal_Normal;
            GFX.FlverShader.Effect.NewBlendInverseVal_Shininess = ShaderConfig.NewBlendInverseVal_Shininess;
            GFX.FlverShader.Effect.NewBlendInverseVal_Emissive = ShaderConfig.NewBlendInverseVal_Emissive;



            if (GFX.HighlightedThing == this || GFX.HighlightedThing == parentMesh || GFX.HighlightedThing == submesh || GFX.HighlightedThing == model)
            {
                GFX.FlverShader.Effect.HighlightColor = GFX.HighlightColor.ToVector3();
                GFX.FlverShader.Effect.HighlightOpacity = GFX.HighlightOpacity;
            }
            else
            {
                GFX.FlverShader.Effect.HighlightOpacity = 0;
            }

            GFX.FlverShader.Effect.IsDS2NormalMapChannels = game == SoulsAssetPipeline.SoulsGames.DS2SOTFS;
            GFX.FlverShader.Effect.IsDS2EmissiveFlow = ShaderConfig.IsDS2EmissiveFlow;
            GFX.FlverShader.Effect.IsMetallic = ShaderConfig.IsMetallic;// = GameDataManager.GameTypeUsesMetallicShaders;

            GFX.FlverShader.Effect.MetallicSpecularIncreasePower = ShaderConfig.MetallicSpecularIncreasePower;
            GFX.FlverShader.Effect.MetallicSpecularIncreaseMult  = ShaderConfig.MetallicSpecularIncreaseMult;
            GFX.FlverShader.Effect.MetallicDiffuseDecreaseMult   = ShaderConfig.MetallicDiffuseDecreaseMult;


            GFX.FlverShader.Effect.InvertMetallic = ShaderConfig.InvertMetallic;// = GameDataManager.GameTypeUsesMetallicShaders;
            GFX.FlverShader.Effect.IsAlbedoAlphaMultInNormalAlpha = ShaderConfig.IsAlbedoAlphaMultInNormalAlpha;
            GFX.FlverShader.Effect.IsReflectMultInNormalAlpha = ShaderConfig.IsReflectMultInNormalAlpha || GFX.FlverShader.Effect.NewDebugType == NewDebugTypes.ReflectanceMult;
            GFX.FlverShader.Effect.IsMetallicMultInNormalAlpha = ShaderConfig.IsMetallicInNormalAlpha;
            bool swapNormalXY = !ShaderConfig.SwapNormalXY;
            //if (GameRoot.GameType is SoulsGames.AC6)
            //    swapNormalXY = !swapNormalXY;
            if (GFX.FlverDebugSwapAllNormalXY)
                swapNormalXY = !swapNormalXY;
            GFX.FlverShader.Effect.SwapNormalXY = swapNormalXY;

            GFX.FlverShader.Effect.IsUndefinedMetallic = IsUndefinedMetallic;
            GFX.FlverShader.Effect.IsUndefinedBlendMask = IsUndefinedBlendMask;
            GFX.FlverShader.Effect.UndefinedBlendMaskValue = Ptde_UsesDetailBump ? Ptde_DetailBump_BumpPower : ShaderConfig.UndefinedBlendMaskValue;
            GFX.FlverShader.Effect.UndefinedMetallicValue = ShaderConfig.UndefinedMetallicValue;
            GFX.FlverShader.Effect.NonMetallicSpecColor = new Vector3(0.04f, 0.04f, 0.04f);// ShaderConfig.NonMetallicSpecColor;

            if (!ShaderConfig.IsMetallic && zzz_DocumentManager.CurrentDocument.GameRoot.GameTypeUsesMetallicShaders)
            {
                GFX.FlverShader.Effect.IsMetallic = true;
                GFX.FlverShader.Effect.IsUndefinedMetallic = true;
                GFX.FlverShader.Effect.UndefinedMetallicValue = 0.5f;
            }


            GFX.FlverShader.Effect.UseShininessMap = ShaderConfig.UseShininessMap;
            GFX.FlverShader.Effect.BlendMaskFromNormalMap1Alpha = ShaderConfig.BlendMaskFromNormalMap1Alpha;
            GFX.FlverShader.Effect.BlendMaskFromNormalMap1Alpha_IsReverse = ShaderConfig.BlendMaskFromNormalMap1Alpha_IsReverse;
            GFX.FlverShader.Effect.BlendMaskMultByAlbedoMap2Alpha = ShaderConfig.BlendMaskMultByAlbedoMap2Alpha;
            GFX.FlverShader.Effect.BlendMaskMultByAlbedoMap2Alpha_IsReverse = ShaderConfig.BlendMaskMultByAlbedoMap2Alpha_IsReverse;

            GFX.FlverShader.Effect.PtdeMtdType = PtdeMtdType;

            GFX.FlverShader.Effect.IsDS1R = zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R;
            GFX.FlverShader.Effect.IsDoubleFaceCloth = ShaderConfig.IsShaderDoubleFaceCloth;

            GFX.FlverShader.Effect.EnableSSS = ShaderConfig.EnableSSS;
            GFX.FlverShader.Effect.SSSColor = ShaderConfig.SSSColor;
            GFX.FlverShader.Effect.SSSIntensity = ShaderConfig.SSSIntensity;


            if (game == SoulsAssetPipeline.SoulsGames.BB)
            {


                GFX.FlverShader.Effect.DirectDiffuseMult *= MtdInfo.GetFloatParameterOrDefault("g_BaseReflectance", 1) * 2;

                ////GFX.FlverShader.Effect.DirectSpecularMult *= MtdInfo.GetFloatParameterOrDefault("g_BaseReflectance", 1) * 8f;
                ////GFX.FlverShader.Effect.IndirectSpecularMult *= MtdInfo.GetFloatParameterOrDefault("g_BaseReflectance", 1) * 2f;
                GFX.FlverShader.Effect.IndirectLightMult *= MtdInfo.GetFloatParameterOrDefault("g_AmbientReflectionRate", 1) * 1;



                //GFX.FlverShader.Effect.AmbientLightMult *= 2;
                //GFX.FlverShader.Effect.DirectDiffuseMult *= 0.5f;
                //GFX.FlverShader.Effect.IndirectDiffuseMult *= 0.5f;
                GFX.FlverShader.Effect.SpecularPowerMult *= 0.5f;
                //GFX.FlverShader.Effect.SSSColor = MtdInfo.GetFloat3ParameterOrDefault("g_SSSStrength", Vector3.Zero);
            }

            GFX.FlverShader.Effect.SSS_UseWidth = false;
            if (MtdInfo.ShaderParameters.ContainsKey("g_SSSWidth"))
            {
                GFX.FlverShader.Effect.EnableSSS = true;
                GFX.FlverShader.Effect.SSS_UseWidth = false;
                //GFX.FlverShader.Effect.SSS_Width = MtdInfo.GetFloatParameterOrDefault("g_SSSWidth", 1) * 1;
                GFX.FlverShader.Effect.SSSIntensity *= (float)Math.Sqrt(MtdInfo.GetFloatParameterOrDefault("g_SSSWidth", 1)) * ViewportEnvironment.SSSMult;
            }
            if (MtdInfo.ShaderParameters.ContainsKey("g_SSSStrength"))
            {
                var sp = MtdInfo.ShaderParameters["g_SSSStrength"];
                if (sp is float[] asVec)
                {
                    if (asVec.Length >= 3)
                    {
                        GFX.FlverShader.Effect.SSSColor *= new Vector3(asVec[0], asVec[1], asVec[2]);
                        MtdInfo.ShaderParameters_AreUsedByDSAS["g_SSSStrength"] = true;
                    }
                }
            }

            

            if (ShaderConfig.ChrCustomizeType != ChrCustomizeTypes.None && basePlayerModel?.ChrAsm != null)
            {
                GFX.FlverShader.Effect.UseChrCustomize = true;
                if (!basePlayerModel.ChrAsm.ChrCustomize.ContainsKey(ShaderConfig.ChrCustomizeType))
                    basePlayerModel.ChrAsm.ChrCustomize.Add(ShaderConfig.ChrCustomizeType, FlverMaterial.GetDefaultChrCustomizeColor(ShaderConfig.ChrCustomizeType));
                GFX.FlverShader.Effect.ChrCustomizeColor = basePlayerModel.ChrAsm.ChrCustomize[ShaderConfig.ChrCustomizeType];
                GFX.FlverShader.Effect.ChrCustomizeUseNormalMapAlpha = ShaderConfig.ChrCustomizeUseNormalMapAlpha;
            }
            else
            {
                GFX.FlverShader.Effect.UseChrCustomize = false;
            }

            var invertBlendMaskMap = ShaderConfig.InvertBlendMaskMap;

            //if (MtdName.ToLower().Contains("cloak") || MtdName.ToLower().Contains("chain"))
            //{
            //    invertBlendMaskMap = !invertBlendMaskMap;
            //}

            GFX.FlverShader.Effect.InvertBlendMaskMap = invertBlendMaskMap;

            FlverShadingModes shadingMode;
            if (GFX.ForcedFlverShadingMode == FlverShadingModes.DEFAULT)
                shadingMode = ShadingMode;
            else
                shadingMode = GFX.ForcedFlverShadingMode;

            GFX.FlverShader.Effect.DisableAlpha = !(GFX.FlverEnableTextureAlphas && ShaderConfig.EnableAlphas);
            GFX.FlverShader.Effect.FancyAlpha_Enable = GFX.FlverUseFancyAlpha && ShaderConfig.UseFancyAlphas;
            GFX.FlverShader.Effect.FancyAlpha_EdgeCutoff = (ShaderConfig.FancyAlphaCutoff < 1) ? ShaderConfig.FancyAlphaCutoff : GFX.FlverFancyAlphaEdgeCutoff;

            GFX.FlverShader.Effect.EmissiveColorFromAlbedo = ShaderConfig.EmissiveColorFromAlbedo;
            
            if ((int)shadingMode >= (int)FlverShadingModes.TEXDEBUG_UVCHECK_0 && (int)shadingMode <= (int)FlverShadingModes.TEXDEBUG_UVCHECK_7)
            {
                GFX.FlverShader.Effect.UVCheckMap = DBG.UV_CHECK_TEX;
            }

            if ((int)shadingMode >= (int)FlverShadingModes.TEXDEBUG_DIFFUSEMAP && (int)shadingMode <= (int)FlverShadingModes.TEXDEBUG_UVCHECK_7)
            {
                GFX.FlverShader.Effect.FancyAlpha_EdgeCutoff = -1;
                GFX.FlverShader.Effect.FancyAlpha_Enable = false;
                GFX.FlverShader.Effect.DisableAlpha = true;
            }

            

            if (anyTexDebugUsed)
            {
                GFX.FlverShader.Effect.WorkflowType = FlverShadingModes.PBR_GLOSS;
            }
            else
            {
                GFX.FlverShader.Effect.WorkflowType = shadingMode;
            }

            if (ShaderConfig.IsDS3Veil)
            {
                GFX.FlverShader.Effect.FancyAlpha_EdgeCutoff = -1;
                GFX.FlverShader.Effect.FancyAlpha_Enable = false;
            }

            if (GFX.CurrentStep == GFXDrawStep.AlphaEdge)
            {
                if (/*DrawStep != GFXDrawStep.AlphaEdge ||*/ !GFX.FlverShader.Effect.FancyAlpha_Enable)
                    return false;

                GFX.FlverShader.Effect.FancyAlpha_IsEdgeStep = true;
            }
            else
            {
                GFX.FlverShader.Effect.FancyAlpha_IsEdgeStep = false;
            }

            bool isAnyTex2Defined = false;
            bool isBlendMaskDefined = false;

            List<TextureTypes> unsetTextureTypes = ((TextureTypes[])Enum.GetValues(typeof(TextureTypes))).ToList();

            void SetTexture(TextureTypes type, Texture2D tex, Vector2 uvScale, int uvIndex, bool isUnset = false)
            {
                if (unsetTextureTypes.Contains(type) && (tex != null))
                    unsetTextureTypes.Remove(type);
                else if (!isUnset)
                    return;

                if (type == TextureTypes.None)
                    return;

                if (type == TextureTypes.Albedo1)
                {
                    GFX.FlverShader.Effect.ColorMap = tex ?? DefaultDiffuseTex;
                    GFX.FlverShader.Effect.ColorMapScale = uvScale;
                    GFX.FlverShader.Effect.Albedo1UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Albedo2)
                {
                    isAnyTex2Defined = tex != null;
                    GFX.FlverShader.Effect.ColorMap2 = tex ?? DefaultDiffuseTex;
                    GFX.FlverShader.Effect.ColorMapScale2 = uvScale;
                    GFX.FlverShader.Effect.Albedo2UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Normal1)
                {
                    GFX.FlverShader.Effect.NormalMap = tex ?? DefaultNormalTex;
                    GFX.FlverShader.Effect.NormalMapScale = uvScale;
                    GFX.FlverShader.Effect.Normal1UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Normal2)
                {
                    isAnyTex2Defined = tex != null;
                    GFX.FlverShader.Effect.NormalMap2 = tex ?? DefaultNormalTex;
                    GFX.FlverShader.Effect.NormalMapScale2 = uvScale;
                    GFX.FlverShader.Effect.Normal2UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Specular1)
                {
                    GFX.FlverShader.Effect.SpecularMap = tex ?? DefaultSpecularTex;
                    GFX.FlverShader.Effect.SpecularMapScale = uvScale;
                    GFX.FlverShader.Effect.Specular1UVIndex = uvIndex;
                }
                else if (type  == TextureTypes.Specular2)
                {
                    isAnyTex2Defined = tex != null;
                    GFX.FlverShader.Effect.SpecularMap2 = tex ?? DefaultSpecularTex;
                    GFX.FlverShader.Effect.SpecularMapScale2 = uvScale;
                    GFX.FlverShader.Effect.Specular2UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Shininess1)
                {
                    GFX.FlverShader.Effect.ShininessMap = tex ?? DefaultShininessTex;
                    GFX.FlverShader.Effect.ShininessMapScale = uvScale;
                    GFX.FlverShader.Effect.Shininess1UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Shininess2)
                {
                    isAnyTex2Defined = tex != null;
                    GFX.FlverShader.Effect.ShininessMap2 = tex ?? DefaultShininessTex;
                    GFX.FlverShader.Effect.ShininessMapScale2 = uvScale;
                    GFX.FlverShader.Effect.Shininess2UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Emissive1)
                {
                    GFX.FlverShader.Effect.EmissiveMap = tex ?? DefaultEmissiveTex;
                    GFX.FlverShader.Effect.EmissiveMapScale = uvScale;
                    GFX.FlverShader.Effect.Emissive1UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Emissive2)
                {
                    isAnyTex2Defined = tex != null;
                    GFX.FlverShader.Effect.EmissiveMap2 = tex ?? DefaultEmissiveTex;
                    GFX.FlverShader.Effect.EmissiveMapScale2 = uvScale;
                    GFX.FlverShader.Effect.Emissive2UVIndex = uvIndex;
                }
                else if (type == TextureTypes.Blend1To2)
                {
                    isBlendMaskDefined = tex != null;
                    GFX.FlverShader.Effect.BlendmaskMap = tex ?? DefaultBlend1To2Tex;
                    GFX.FlverShader.Effect.BlendmaskMapScale = uvScale;
                    GFX.FlverShader.Effect.BlendMaskUVIndex = uvIndex;
                }
                else if (type == TextureTypes.Blend3Mask)
                {
                    GFX.FlverShader.Effect.Mask3Map = tex ?? DefaultMask3Tex;
                    GFX.FlverShader.Effect.Mask3MapScale = uvScale;
                    GFX.FlverShader.Effect.Mask3UVIndex = uvIndex;
                }
            }

            
            foreach (var s in Samplers)
            {
                var sc = GetSamplerConfig(s.Name);
                var uvScale = s.UVScale;
                if (sc != null)
                {
                    uvScale *= GetUVTileMult(sc.UVGroup);
                }
                SetTexture(sc?.TexType ?? TextureTypes.None, s.Tex ?? sc?.DefaultTex, uvScale, sc?.UVIndex ?? 0);
            }

            Sampler getMatchingTexInPair(TextureTypes s)
            {
                int type = (int)s;
                if (type < (int)TextureTypes.Blend1To2)
                {
                    type += 100;
                }
                else if (type >= (int)TextureTypes.Albedo2)
                {
                    type -= 100;
                }
                else
                {
                    return null;
                }
                var match = ShaderConfig.SamplerConfigs.FirstOrDefault(x => x.TexType == (TextureTypes)type);
                if (match != null)
                {
                    return Samplers.FirstOrDefault(s => s.Name == match.Name);
                }
                return null;
            }

            GFX.FlverShader.Effect.SSS_UseDefaultMask = false;
            var unsetTexTypesCopy = unsetTextureTypes.ToList();
            foreach (var unset in unsetTexTypesCopy)
            {
                if (unset is TextureTypes.Normal2)
                    continue;

                // shitcode
                if (unset is TextureTypes.Blend3Mask && GFX.FlverShader.Effect.EnableSSS)
                {
                    GFX.FlverShader.Effect.SSS_UseDefaultMask = true;
                }

                var match = getMatchingTexInPair(unset);

                var uvScale = Vector2.One;
                Texture2D t = null;
                int uvIndex = 0;
                //if (unset == TextureTypes.Blend1To2)
                //    uvIndex = 1;
                //else if ((int)unset >= (int)TextureTypes.Albedo2)
                //    uvIndex = 2;

                if (match != null)
                {
                    //t = match.Tex;


                    var sc = GetSamplerConfig(match.Name);
                    uvScale = match.UVScale;
                    if (sc != null)
                    {
                        if (sc.TexType is TextureTypes.Albedo1 or TextureTypes.Albedo2)
                            t = GetMissingTexForBlendType(ShaderConfig.NewBlendOperation_Diffuse, match.Tex);
                        else if (sc.TexType is TextureTypes.Normal1 or TextureTypes.Normal2)
                            t = GetMissingTexForBlendType(ShaderConfig.NewBlendOperation_Normal, match.Tex);
                        else if (sc.TexType is TextureTypes.Specular1 or TextureTypes.Specular2)
                            t = GetMissingTexForBlendType(ShaderConfig.NewBlendOperation_Specular, match.Tex);
                        else if (sc.TexType is TextureTypes.Shininess1 or TextureTypes.Shininess2)
                            t = GetMissingTexForBlendType(ShaderConfig.NewBlendOperation_Shininess, match.Tex);
                        else if (sc.TexType is TextureTypes.Emissive1 or TextureTypes.Emissive2)
                            t = Main.BLACK_TEXTURE;
                        uvScale *= GetUVTileMult(sc.UVGroup);

                        if (t == null)
                        {
                            t = sc.DefaultTex;
                        }
                        
                    }
                    uvIndex = GetSamplerConfig(match.Name)?.UVIndex ?? 0;
                }


                

                SetTexture(unset, t, uvScale, uvIndex, isUnset: true);
            }

            var isDS1 = (zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R || zzz_DocumentManager.CurrentDocument.GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DES);

            GFX.FlverShader.Effect.EnableBlendMaskMap = (isBlendMaskDefined || (isDS1 && isAnyTex2Defined)) && GFX.FlverEnableTextureBlending && ShaderConfig.EnableBlendMask;

            GFX.FlverShader.Effect.GlobalUVOffset = DebugUVOffset;
            
            if (!isBlendMaskDefined)
            {
                GFX.FlverShader.Effect.IsUndefinedBlendMask = IsUndefinedBlendMask = true;
            }
            
            return true;
        }

        public static List<TextureTypes> AllTextureTypes = null;
        public static string[] AllTextureTypeNames = null;
        static FlverMaterial()
        {
            AllTextureTypes = ((TextureTypes[])Enum.GetValues(typeof(TextureTypes))).ToList();
            AllTextureTypeNames = AllTextureTypes.Select(x => x.ToString()).ToArray();
        }

        public class Sampler
        {
            public string Name;
            public string TexPath;
            public Texture2D Tex;
            //public int UVIndex = 0;
            public Vector2 UVScale = Vector2.One;
            
            // Now stored in ShaderConfig
            //public TextureTypes TextureType;
            public NewDebug_ShowTex_ChannelConfigs ChannelConfig_ForDebug = NewDebug_ShowTex_ChannelConfigs.RGBA;

            public bool IsDebugView = false;
        }
    }
}
