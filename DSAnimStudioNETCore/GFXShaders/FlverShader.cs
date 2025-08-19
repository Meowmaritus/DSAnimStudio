using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.GFXShaders
{
    public class FlverShader : Effect, IGFXShader<FlverShader>
    {
        public const int BoneMatrixSize = 512;
        public static Matrix[] IdentityBoneMatrix;

        static FlverShader()
        {
            IdentityBoneMatrix = new Matrix[BoneMatrixSize];
            for (int i = 0; i < BoneMatrixSize; i++)
            {
                IdentityBoneMatrix[i] = Matrix.Identity;
            }
        }

        public FlverShader Effect => this;


        public bool ForcedSSS_Enable
        {
            get => Parameters[nameof(ForcedSSS_Enable)].GetValueBoolean();
            set => Parameters[nameof(ForcedSSS_Enable)]?.SetValue(value);
        }

        public float ForcedSSS_Intensity
        {
            get => Parameters[nameof(ForcedSSS_Intensity)].GetValueSingle();
            set => Parameters[nameof(ForcedSSS_Intensity)]?.SetValue(value);
        }

        public bool SSS_UseWidth
        {
            get => Parameters[nameof(SSS_UseWidth)].GetValueBoolean();
            set => Parameters[nameof(SSS_UseWidth)]?.SetValue(value);
        }

        public float SSS_Width
        {
            get => Parameters[nameof(SSS_Width)].GetValueSingle();
            set => Parameters[nameof(SSS_Width)]?.SetValue(value);
        }

        public bool SSS_UseDefaultMask
        {
            get => Parameters[nameof(SSS_UseDefaultMask)].GetValueBoolean();
            set => Parameters[nameof(SSS_UseDefaultMask)]?.SetValue(value);
        }

        public float SSS_DefaultMask
        {
            get => Parameters[nameof(SSS_DefaultMask)].GetValueSingle();
            set => Parameters[nameof(SSS_DefaultMask)]?.SetValue(value);
        }



        public bool WireframeColorOverride_Enabled
        {
            get => Parameters[nameof(WireframeColorOverride_Enabled)].GetValueBoolean();
            set => Parameters[nameof(WireframeColorOverride_Enabled)]?.SetValue(value);
        }

        public Vector4 WireframeColorOverride_Color
        {
            get => Parameters[nameof(WireframeColorOverride_Color)].GetValueVector4();
            set => Parameters[nameof(WireframeColorOverride_Color)]?.SetValue(value);
        }

        public int DebugViewWeightOfBone_Index
        {
            get => Parameters[nameof(DebugViewWeightOfBone_Index)].GetValueInt32();
            set => Parameters[nameof(DebugViewWeightOfBone_Index)]?.SetValue(value);
        }

        public bool DebugViewWeightOfBone_EnableLighting
        {
            get => Parameters[nameof(DebugViewWeightOfBone_EnableLighting)].GetValueBoolean();
            set => Parameters[nameof(DebugViewWeightOfBone_EnableLighting)]?.SetValue(value);
        }

        public bool DebugViewWeightOfBone_ClipUnweightedGeometry
        {
            get => Parameters[nameof(DebugViewWeightOfBone_ClipUnweightedGeometry)].GetValueBoolean();
            set => Parameters[nameof(DebugViewWeightOfBone_ClipUnweightedGeometry)]?.SetValue(value);
        }

        public float DebugViewWeightOfBone_LightingPower
        {
            get => Parameters[nameof(DebugViewWeightOfBone_LightingPower)].GetValueSingle();
            set => Parameters[nameof(DebugViewWeightOfBone_LightingPower)]?.SetValue(value);
        }

        public float DebugViewWeightOfBone_LightingMult
        {
            get => Parameters[nameof(DebugViewWeightOfBone_LightingMult)].GetValueSingle();
            set => Parameters[nameof(DebugViewWeightOfBone_LightingMult)]?.SetValue(value);
        }

        public float DebugViewWeightOfBone_LightingGain
        {
            get => Parameters[nameof(DebugViewWeightOfBone_LightingGain)].GetValueSingle();
            set => Parameters[nameof(DebugViewWeightOfBone_LightingGain)]?.SetValue(value);
        }

        public Vector3 DebugViewWeightOfBone_BaseColor
        {
            get => Parameters[nameof(DebugViewWeightOfBone_BaseColor)].GetValueVector3();
            set => Parameters[nameof(DebugViewWeightOfBone_BaseColor)]?.SetValue(value);
        }

        public Vector3 DebugViewWeightOfBone_WeightColor
        {
            get => Parameters[nameof(DebugViewWeightOfBone_WeightColor)].GetValueVector3();
            set => Parameters[nameof(DebugViewWeightOfBone_WeightColor)]?.SetValue(value);
        }

        public Vector4 DebugViewWeightOfBone_WireframeWeightColor
        {
            get => Parameters[nameof(DebugViewWeightOfBone_WireframeWeightColor)].GetValueVector4();
            set => Parameters[nameof(DebugViewWeightOfBone_WireframeWeightColor)]?.SetValue(value);
        }

        public float DebugViewWeightOfBone_Lighting_AlbedoMult
        {
            get => Parameters[nameof(DebugViewWeightOfBone_Lighting_AlbedoMult)].GetValueSingle();
            set => Parameters[nameof(DebugViewWeightOfBone_Lighting_AlbedoMult)]?.SetValue(value);
        }

        public float DebugViewWeightOfBone_Lighting_ReflectanceMult
        {
            get => Parameters[nameof(DebugViewWeightOfBone_Lighting_ReflectanceMult)].GetValueSingle();
            set => Parameters[nameof(DebugViewWeightOfBone_Lighting_ReflectanceMult)]?.SetValue(value);
        }

        public float DebugViewWeightOfBone_Lighting_Gloss
        {
            get => Parameters[nameof(DebugViewWeightOfBone_Lighting_Gloss)].GetValueSingle();
            set => Parameters[nameof(DebugViewWeightOfBone_Lighting_Gloss)]?.SetValue(value);
        }


        public bool UseShininessMap
        {
            get => Parameters[nameof(UseShininessMap)].GetValueBoolean();
            set => Parameters[nameof(UseShininessMap)]?.SetValue(value);
        }

        public bool UseChrCustomize
        {
            get => Parameters[nameof(UseChrCustomize)].GetValueBoolean();
            set => Parameters[nameof(UseChrCustomize)]?.SetValue(value);
        }

        public bool ChrCustomizeUseNormalMapAlpha
        {
            get => Parameters[nameof(ChrCustomizeUseNormalMapAlpha)].GetValueBoolean();
            set => Parameters[nameof(ChrCustomizeUseNormalMapAlpha)]?.SetValue(value);
        }

        public Vector4 ChrCustomizeColor
        {
            get => Parameters[nameof(ChrCustomizeColor)].GetValueVector4();
            set => Parameters[nameof(ChrCustomizeColor)]?.SetValue(value);
        }

        public bool EnableSSS
        {
            get => Parameters[nameof(EnableSSS)].GetValueBoolean();
            set => Parameters[nameof(EnableSSS)]?.SetValue(value);
        }

        public Vector3 SSSColor
        {
            get => Parameters[nameof(SSSColor)].GetValueVector3();
            set => Parameters[nameof(SSSColor)]?.SetValue(value);
        }

        public float SSSIntensity
        {
            get => Parameters[nameof(SSSIntensity)].GetValueSingle();
            set => Parameters[nameof(SSSIntensity)]?.SetValue(value);
        }

        public Texture2D Mask3Map
        {
            get => Parameters[nameof(Mask3Map)].GetValueTexture2D();
            set => Parameters[nameof(Mask3Map)]?.SetValue(value);
        }

        public Vector2 Mask3MapScale
        {
            get => Parameters[nameof(Mask3MapScale)].GetValueVector2();
            set => Parameters[nameof(Mask3MapScale)]?.SetValue(value);
        }

        public int Mask3UVIndex
        {
            get => Parameters[nameof(Mask3UVIndex)].GetValueInt32();
            set => Parameters[nameof(Mask3UVIndex)]?.SetValue(value);
        }

        public bool IsMetallicDiffuseDarkenMultInNormalAlpha
        {
            get => Parameters[nameof(IsMetallicDiffuseDarkenMultInNormalAlpha)].GetValueBoolean();
            set => Parameters[nameof(IsMetallicDiffuseDarkenMultInNormalAlpha)].SetValue(value);
        }

        public bool IsAlbedoAlphaMultInNormalAlpha
        {
            get => Parameters[nameof(IsAlbedoAlphaMultInNormalAlpha)].GetValueBoolean();
            set => Parameters[nameof(IsAlbedoAlphaMultInNormalAlpha)].SetValue(value);
        }

        public bool IsReflectMultInNormalAlpha
        {
            get => Parameters[nameof(IsReflectMultInNormalAlpha)].GetValueBoolean();
            set => Parameters[nameof(IsReflectMultInNormalAlpha)].SetValue(value);
        }

        public bool IsMetallicMultInNormalAlpha
        {
            get => Parameters[nameof(IsMetallicMultInNormalAlpha)].GetValueBoolean();
            set => Parameters[nameof(IsMetallicMultInNormalAlpha)].SetValue(value);
        }

        public bool SwapNormalXY
        {
            get => Parameters[nameof(SwapNormalXY)].GetValueBoolean();
            set => Parameters[nameof(SwapNormalXY)].SetValue(value);
        }

        #region NewDebug

        public NewBlendOperations NewBlendOperation_Diffuse
        {
            get => (NewBlendOperations)Parameters[nameof(NewBlendOperation_Diffuse)].GetValueInt32();
            set => Parameters[nameof(NewBlendOperation_Diffuse)].SetValue((int)value);
        }

        public NewBlendOperations NewBlendOperation_Specular
        {
            get => (NewBlendOperations)Parameters[nameof(NewBlendOperation_Specular)].GetValueInt32();
            set => Parameters[nameof(NewBlendOperation_Specular)].SetValue((int)value);
        }

        public NewBlendOperations NewBlendOperation_Normal
        {
            get => (NewBlendOperations)Parameters[nameof(NewBlendOperation_Normal)].GetValueInt32();
            set => Parameters[nameof(NewBlendOperation_Normal)].SetValue((int)value);
        }

        public NewBlendOperations NewBlendOperation_Shininess
        {
            get => (NewBlendOperations)Parameters[nameof(NewBlendOperation_Shininess)].GetValueInt32();
            set => Parameters[nameof(NewBlendOperation_Shininess)].SetValue((int)value);
        }

        public NewBlendOperations NewBlendOperation_Emissive
        {
            get => (NewBlendOperations)Parameters[nameof(NewBlendOperation_Emissive)].GetValueInt32();
            set => Parameters[nameof(NewBlendOperation_Emissive)].SetValue((int)value);
        }

        public bool NewBlendReverseDir_Diffuse
        {
            get => Parameters[nameof(NewBlendReverseDir_Diffuse)].GetValueBoolean();
            set => Parameters[nameof(NewBlendReverseDir_Diffuse)].SetValue(value);
        }

        public bool NewBlendReverseDir_Specular
        {
            get => Parameters[nameof(NewBlendReverseDir_Specular)].GetValueBoolean();
            set => Parameters[nameof(NewBlendReverseDir_Specular)].SetValue(value);
        }

        public bool NewBlendReverseDir_Normal
        {
            get => Parameters[nameof(NewBlendReverseDir_Normal)].GetValueBoolean();
            set => Parameters[nameof(NewBlendReverseDir_Normal)].SetValue(value);
        }

        public bool NewBlendReverseDir_Shininess
        {
            get => Parameters[nameof(NewBlendReverseDir_Shininess)].GetValueBoolean();
            set => Parameters[nameof(NewBlendReverseDir_Shininess)].SetValue(value);
        }

        public bool NewBlendReverseDir_Emissive
        {
            get => Parameters[nameof(NewBlendReverseDir_Emissive)].GetValueBoolean();
            set => Parameters[nameof(NewBlendReverseDir_Emissive)].SetValue(value);
        }




        public bool NewBlendInverseVal_Diffuse
        {
            get => Parameters[nameof(NewBlendInverseVal_Diffuse)].GetValueBoolean();
            set => Parameters[nameof(NewBlendInverseVal_Diffuse)].SetValue(value);
        }

        public bool NewBlendInverseVal_Specular
        {
            get => Parameters[nameof(NewBlendInverseVal_Specular)].GetValueBoolean();
            set => Parameters[nameof(NewBlendInverseVal_Specular)].SetValue(value);
        }

        public bool NewBlendInverseVal_Normal
        {
            get => Parameters[nameof(NewBlendInverseVal_Normal)].GetValueBoolean();
            set => Parameters[nameof(NewBlendInverseVal_Normal)].SetValue(value);
        }

        public bool NewBlendInverseVal_Shininess
        {
            get => Parameters[nameof(NewBlendInverseVal_Shininess)].GetValueBoolean();
            set => Parameters[nameof(NewBlendInverseVal_Shininess)].SetValue(value);
        }

        public bool NewBlendInverseVal_Emissive
        {
            get => Parameters[nameof(NewBlendInverseVal_Emissive)].GetValueBoolean();
            set => Parameters[nameof(NewBlendInverseVal_Emissive)].SetValue(value);
        }






        public NewDebugTypes NewDebugType
        {
            get => (NewDebugTypes)Parameters[nameof(NewDebugType)].GetValueInt32();
            set => Parameters[nameof(NewDebugType)].SetValue((int)value);
        }

        public Texture2D NewDebug_ShowTex_Tex
        {
            get => Parameters[nameof(NewDebug_ShowTex_Tex)].GetValueTexture2D();
            set => Parameters[nameof(NewDebug_ShowTex_Tex)]?.SetValue(value);
        }

        public NewDebug_ShowTex_ChannelConfigs NewDebug_ShowTex_ChannelConfig
        {
            get => (NewDebug_ShowTex_ChannelConfigs)Parameters[nameof(NewDebug_ShowTex_ChannelConfig)].GetValueInt32();
            set => Parameters[nameof(NewDebug_ShowTex_ChannelConfig)].SetValue((int)value);
        }

        public int NewDebug_ShowTex_UVIndex
        {
            get => Parameters[nameof(NewDebug_ShowTex_UVIndex)].GetValueInt32();
            set => Parameters[nameof(NewDebug_ShowTex_UVIndex)].SetValue(value);
        }

        public Vector2 NewDebug_ShowTex_UVScale
        {
            get => Parameters[nameof(NewDebug_ShowTex_UVScale)].GetValueVector2();
            set => Parameters[nameof(NewDebug_ShowTex_UVScale)].SetValue(value);
        }

        #endregion

        public Vector3 HighlightColor
        {
            get => Parameters[nameof(HighlightColor)].GetValueVector3();
            set => Parameters[nameof(HighlightColor)].SetValue(value);
        }

        public Vector3 NonMetallicSpecColor
        {
            get => Parameters[nameof(NonMetallicSpecColor)].GetValueVector3();
            set => Parameters[nameof(NonMetallicSpecColor)].SetValue(value);
        }

        public float UndefinedMetallicValue
        {
            get => Parameters[nameof(UndefinedMetallicValue)].GetValueSingle();
            set => Parameters[nameof(UndefinedMetallicValue)].SetValue(value);
        }

        public bool InvertMetallic
        {
            get => Parameters[nameof(InvertMetallic)].GetValueBoolean();
            set => Parameters[nameof(InvertMetallic)].SetValue(value);
        }

        public bool IsUndefinedMetallic
        {
            get => Parameters[nameof(IsUndefinedMetallic)].GetValueBoolean();
            set => Parameters[nameof(IsUndefinedMetallic)].SetValue(value);
        }

        public bool IsUndefinedBlendMask
        {
            get => Parameters[nameof(IsUndefinedBlendMask)].GetValueBoolean();
            set => Parameters[nameof(IsUndefinedBlendMask)].SetValue(value);
        }

        public bool BlendMaskFromNormalMap1Alpha
        {
            get => Parameters[nameof(BlendMaskFromNormalMap1Alpha)].GetValueBoolean();
            set => Parameters[nameof(BlendMaskFromNormalMap1Alpha)].SetValue(value);
        }

        public bool BlendMaskFromNormalMap1Alpha_IsReverse
        {
            get => Parameters[nameof(BlendMaskFromNormalMap1Alpha_IsReverse)].GetValueBoolean();
            set => Parameters[nameof(BlendMaskFromNormalMap1Alpha_IsReverse)].SetValue(value);
        }

        public bool BlendMaskMultByAlbedoMap2Alpha
        {
            get => Parameters[nameof(BlendMaskMultByAlbedoMap2Alpha)].GetValueBoolean();
            set => Parameters[nameof(BlendMaskMultByAlbedoMap2Alpha)].SetValue(value);
        }

        public bool BlendMaskMultByAlbedoMap2Alpha_IsReverse
        {
            get => Parameters[nameof(BlendMaskMultByAlbedoMap2Alpha_IsReverse)].GetValueBoolean();
            set => Parameters[nameof(BlendMaskMultByAlbedoMap2Alpha_IsReverse)].SetValue(value);
        }

        public float UndefinedBlendMaskValue
        {
            get => Parameters[nameof(UndefinedBlendMaskValue)].GetValueSingle();
            set => Parameters[nameof(UndefinedBlendMaskValue)].SetValue(value);
        }

        public float HighlightOpacity
        {
            get => Parameters[nameof(HighlightOpacity)].GetValueSingle();
            set => Parameters[nameof(HighlightOpacity)].SetValue(value);
        }


        public int Albedo1UVIndex
        {
            get => Parameters[nameof(Albedo1UVIndex)].GetValueInt32();
            set => Parameters[nameof(Albedo1UVIndex)].SetValue(value);
        }

        public int Albedo2UVIndex
        {
            get => Parameters[nameof(Albedo2UVIndex)].GetValueInt32();
            set => Parameters[nameof(Albedo2UVIndex)].SetValue(value);
        }

        public int Specular1UVIndex
        {
            get => Parameters[nameof(Specular1UVIndex)].GetValueInt32();
            set => Parameters[nameof(Specular1UVIndex)].SetValue(value);
        }

        public int Specular2UVIndex
        {
            get => Parameters[nameof(Specular2UVIndex)].GetValueInt32();
            set => Parameters[nameof(Specular2UVIndex)].SetValue(value);
        }

        public int Shininess1UVIndex
        {
            get => Parameters[nameof(Shininess1UVIndex)].GetValueInt32();
            set => Parameters[nameof(Shininess1UVIndex)].SetValue(value);
        }

        public int Shininess2UVIndex
        {
            get => Parameters[nameof(Shininess2UVIndex)].GetValueInt32();
            set => Parameters[nameof(Shininess2UVIndex)].SetValue(value);
        }

        public int Normal1UVIndex
        {
            get => Parameters[nameof(Normal1UVIndex)].GetValueInt32();
            set => Parameters[nameof(Normal1UVIndex)].SetValue(value);
        }

        public int Normal2UVIndex
        {
            get => Parameters[nameof(Normal2UVIndex)].GetValueInt32();
            set => Parameters[nameof(Normal2UVIndex)].SetValue(value);
        }

        public int Emissive1UVIndex
        {
            get => Parameters[nameof(Emissive1UVIndex)].GetValueInt32();
            set => Parameters[nameof(Emissive1UVIndex)].SetValue(value);
        }

        public int Emissive2UVIndex
        {
            get => Parameters[nameof(Emissive2UVIndex)].GetValueInt32();
            set => Parameters[nameof(Emissive2UVIndex)].SetValue(value);
        }

        public int BlendMaskUVIndex
        {
            get => Parameters[nameof(BlendMaskUVIndex)].GetValueInt32();
            set => Parameters[nameof(BlendMaskUVIndex)].SetValue(value);
        }

        public float DebugAnimWeight
        {
            get => Parameters[nameof(DebugAnimWeight)].GetValueSingle();
            set => Parameters[nameof(DebugAnimWeight)].SetValue(value);
        }

        public bool EnableSkinning
        {
            get => Parameters[nameof(EnableSkinning)].GetValueBoolean();
            set => Parameters[nameof(EnableSkinning)].SetValue(value);
        }

        public bool IsDS2NormalMapChannels
        {
            get => Parameters[nameof(IsDS2NormalMapChannels)].GetValueBoolean();
            set => Parameters[nameof(IsDS2NormalMapChannels)].SetValue(value);
        }

        public bool IsDS2EmissiveFlow
        {
            get => Parameters[nameof(IsDS2EmissiveFlow)].GetValueBoolean();
            set => Parameters[nameof(IsDS2EmissiveFlow)].SetValue(value);
        }

        public bool IsMetallic
        {
            get => Parameters[nameof(IsMetallic)].GetValueBoolean();
            set => Parameters[nameof(IsMetallic)].SetValue(value);
        }

        public float MetallicSpecularIncreasePower
        {
            get => Parameters[nameof(MetallicSpecularIncreasePower)].GetValueSingle();
            set => Parameters[nameof(MetallicSpecularIncreasePower)].SetValue(value);
        }

        public float MetallicSpecularIncreaseMult
        {
            get => Parameters[nameof(MetallicSpecularIncreaseMult)].GetValueSingle();
            set => Parameters[nameof(MetallicSpecularIncreaseMult)].SetValue(value);
        }

        public float MetallicDiffuseDecreaseMult
        {
            get => Parameters[nameof(MetallicDiffuseDecreaseMult)].GetValueSingle();
            set => Parameters[nameof(MetallicDiffuseDecreaseMult)].SetValue(value);
        }

        public float MetallicDiffuseDecreasePower
        {
            get => Parameters[nameof(MetallicDiffuseDecreasePower)].GetValueSingle();
            set => Parameters[nameof(MetallicDiffuseDecreasePower)].SetValue(value);
        }

        public bool IsDS1R
        {
            get => Parameters[nameof(IsDS1R)].GetValueBoolean();
            set => Parameters[nameof(IsDS1R)].SetValue(value);
        }


        public FlverShadingModes WorkflowType
        {
            get => (FlverShadingModes)Parameters[nameof(WorkflowType)].GetValueInt32();
            set => Parameters[nameof(WorkflowType)].SetValue((int)value);
        }

        public PtdeMtdTypes PtdeMtdType
        {
            get => (PtdeMtdTypes)Parameters[nameof(PtdeMtdType)].GetValueInt32();
            set => Parameters[nameof(PtdeMtdType)].SetValue((int)value);
        }

        //public float DitherTime
        //{
        //    get => Parameters[nameof(DitherTime)].GetValueSingle();
        //    set => Parameters[nameof(DitherTime)].SetValue(value);
        //}

        public bool DisableAlpha
        {
            get => Parameters[nameof(DisableAlpha)].GetValueBoolean();
            set => Parameters[nameof(DisableAlpha)].SetValue(value);
        }

        public bool FancyAlpha_Enable
        {
            get => Parameters[nameof(FancyAlpha_Enable)].GetValueBoolean();
            set => Parameters[nameof(FancyAlpha_Enable)].SetValue(value);
        }

        public bool FancyAlpha_IsEdgeStep
        {
            get => Parameters[nameof(FancyAlpha_IsEdgeStep)].GetValueBoolean();
            set => Parameters[nameof(FancyAlpha_IsEdgeStep)].SetValue(value);
        }

        public float FancyAlpha_EdgeCutoff
        {
            get => Parameters[nameof(FancyAlpha_EdgeCutoff)].GetValueSingle();
            set => Parameters[nameof(FancyAlpha_EdgeCutoff)].SetValue(value);
        }

        public bool EnableBlendTextures
        {
            get => Parameters[nameof(EnableBlendTextures)].GetValueSingle() > 0;
            set => Parameters[nameof(EnableBlendTextures)].SetValue(value ? 1.0f : 0.0f);
        }

        public bool EmissiveColorFromAlbedo
        {
            get => Parameters[nameof(EmissiveColorFromAlbedo)].GetValueBoolean();
            set => Parameters[nameof(EmissiveColorFromAlbedo)].SetValue(value);
        }

        public bool InvertBlendMaskMap
        {
            get => Parameters[nameof(InvertBlendMaskMap)].GetValueBoolean();
            set => Parameters[nameof(InvertBlendMaskMap)].SetValue(value);
        }

        public bool EnableBlendMaskMap
        {
            get => Parameters[nameof(EnableBlendMaskMap)].GetValueBoolean();
            set => Parameters[nameof(EnableBlendMaskMap)].SetValue(value);
        }

        public bool IsDoubleFaceCloth
        {
            get => Parameters[nameof(IsDoubleFaceCloth)].GetValueBoolean();
            set => Parameters[nameof(IsDoubleFaceCloth)].SetValue(value);
        }

        public Vector3 LightDirection
        {
            get => Parameters[nameof(LightDirection)].GetValueVector3();
            set => Parameters[nameof(LightDirection)].SetValue(value);
        }

        public float AlphaTest
        {
            get => Parameters[nameof(AlphaTest)].GetValueSingle();
            set => Parameters[nameof(AlphaTest)].SetValue(value);
        }

        public Vector3 EyePosition
        {
            get => Parameters[nameof(EyePosition)].GetValueVector3();
            set => Parameters[nameof(EyePosition)].SetValue(value);
        }

        public float AmbientLightMult
        {
            get => Parameters[nameof(AmbientLightMult)].GetValueSingle();
            set => Parameters[nameof(AmbientLightMult)].SetValue(value);
        }

        public float DirectLightMult
        {
            get => Parameters[nameof(DirectLightMult)].GetValueSingle();
            set => Parameters[nameof(DirectLightMult)].SetValue(value);
        }

        public float IndirectLightMult
        {
            get => Parameters[nameof(IndirectLightMult)].GetValueSingle();
            set => Parameters[nameof(IndirectLightMult)].SetValue(value);
        }

        public float SpecularPowerMult
        {
            get => Parameters[nameof(SpecularPowerMult)].GetValueSingle();
            set => Parameters[nameof(SpecularPowerMult)].SetValue(value);
        }

        public float DiffuseMapColorPower
        {
            get => Parameters[nameof(DiffuseMapColorPower)].GetValueSingle();
            set => Parameters[nameof(DiffuseMapColorPower)].SetValue(value);
        }

        public float SpecularMapColorPower
        {
            get => Parameters[nameof(SpecularMapColorPower)].GetValueSingle();
            set => Parameters[nameof(SpecularMapColorPower)].SetValue(value);
        }

        public Vector3 DiffuseMapColor
        {
            get => Parameters[nameof(DiffuseMapColor)].GetValueVector3();
            set => Parameters[nameof(DiffuseMapColor)].SetValue(value);
        }

        public Vector3 SpecularMapColor
        {
            get => Parameters[nameof(SpecularMapColor)].GetValueVector3();
            set => Parameters[nameof(SpecularMapColor)].SetValue(value);
        }

        public float EmissiveMapMult
        {
            get => Parameters[nameof(EmissiveMapMult)].GetValueSingle();
            set => Parameters[nameof(EmissiveMapMult)].SetValue(value);
        }

        public float SceneBrightness
        {
            get => Parameters[nameof(SceneBrightness)].GetValueSingle();
            set => Parameters[nameof(SceneBrightness)].SetValue(value);
        }

        public float DirectDiffuseMult
        {
            get => Parameters[nameof(DirectDiffuseMult)].GetValueSingle();
            set => Parameters[nameof(DirectDiffuseMult)].SetValue(value);
        }

        public float DirectSpecularMult
        {
            get => Parameters[nameof(DirectSpecularMult)].GetValueSingle();
            set => Parameters[nameof(DirectSpecularMult)].SetValue(value);
        }


        public float IndirectDiffuseMult
        {
            get => Parameters[nameof(IndirectDiffuseMult)].GetValueSingle();
            set => Parameters[nameof(IndirectDiffuseMult)].SetValue(value);
        }

        public float IndirectSpecularMult
        {
            get => Parameters[nameof(IndirectSpecularMult)].GetValueSingle();
            set => Parameters[nameof(IndirectSpecularMult)].SetValue(value);
        }

        public float Opacity
        {
            get => Parameters[nameof(Opacity)].GetValueSingle();
            set => Parameters[nameof(Opacity)].SetValue(value);
        }

        public bool IsSkybox
        {
            get => Parameters[nameof(IsSkybox)].GetValueBoolean();
            set => Parameters[nameof(IsSkybox)].SetValue(value);
        }

        #region MATRICES
        public Matrix World
        {
            get => Parameters[nameof(World)].GetValueMatrix();
            set => Parameters[nameof(World)].SetValue(value);
        }

        public Matrix View
        {
            get => Parameters[nameof(View)].GetValueMatrix();
            set => Parameters[nameof(View)].SetValue(value);
        }

        //public Matrix ViewInverse
        //{
        //    get => Parameters[nameof(ViewInverse)].GetValueMatrix();
        //    set => Parameters[nameof(ViewInverse)].SetValue(value);
        //}

        public Matrix Projection
        {
            get => Parameters[nameof(Projection)].GetValueMatrix();
            set => Parameters[nameof(Projection)].SetValue(value);
        }

        public Matrix FlipSkybox
        {
            get => Parameters[nameof(FlipSkybox)].GetValueMatrix();
            set => Parameters[nameof(FlipSkybox)].SetValue(value);
        }
        #endregion


        #region LEGACY
        public float Legacy_NormalMapCustomZ
        {
            get => Parameters[nameof(Legacy_NormalMapCustomZ)].GetValueSingle();
            set => Parameters[nameof(Legacy_NormalMapCustomZ)].SetValue(value);
        }

        public float Legacy_DiffusePower
        {
            get => Parameters[nameof(Legacy_DiffusePower)].GetValueSingle();
            set => Parameters[nameof(Legacy_DiffusePower)].SetValue(value);
        }

        public Vector4 Legacy_SpecularColor
        {
            get => Parameters[nameof(Legacy_SpecularColor)].GetValueVector4();
            set => Parameters[nameof(Legacy_SpecularColor)].SetValue(value);
        }

        public float Legacy_SpecularPower
        {
            get => Parameters[nameof(Legacy_SpecularPower)].GetValueSingle();
            set => Parameters[nameof(Legacy_SpecularPower)].SetValue(value);
        }

        public Vector4 Legacy_AmbientColor
        {
            get => Parameters[nameof(Legacy_AmbientColor)].GetValueVector4();
            set => Parameters[nameof(Legacy_AmbientColor)].SetValue(value);
        }

        public float Legacy_AmbientIntensity
        {
            get => Parameters[nameof(Legacy_AmbientIntensity)].GetValueSingle();
            set => Parameters[nameof(Legacy_AmbientIntensity)].SetValue(value);
        }

        public Vector4 Legacy_DiffuseColor
        {
            get => Parameters[nameof(Legacy_DiffuseColor)].GetValueVector4();
            set => Parameters[nameof(Legacy_DiffuseColor)].SetValue(value);
        }

        public float Legacy_DiffuseIntensity
        {
            get => Parameters[nameof(Legacy_DiffuseIntensity)].GetValueSingle();
            set => Parameters[nameof(Legacy_DiffuseIntensity)].SetValue(value);
        }

        public float Legacy_SceneBrightness
        {
            get => Parameters[nameof(Legacy_SceneBrightness)].GetValueSingle();
            set => Parameters[nameof(Legacy_SceneBrightness)].SetValue(value);
        }

        #endregion

        #region TEXTURE MAPS
        public Vector2 GlobalUVOffset
        {
            get => Parameters[nameof(GlobalUVOffset)].GetValueVector2();
            set => Parameters[nameof(GlobalUVOffset)]?.SetValue(value);
        }
        public Texture2D ColorMap
        {
            get => Parameters[nameof(ColorMap)].GetValueTexture2D();
            set => Parameters[nameof(ColorMap)]?.SetValue(value);
        }

        public Texture2D NormalMap
        {
            get => Parameters[nameof(NormalMap)].GetValueTexture2D();
            set => Parameters[nameof(NormalMap)]?.SetValue(value);
        }

        public Texture2D SpecularMap
        {
            get => Parameters[nameof(SpecularMap)].GetValueTexture2D();
            set => Parameters[nameof(SpecularMap)]?.SetValue(value);
        }

        public Texture2D ColorMap2
        {
            get => Parameters[nameof(ColorMap2)].GetValueTexture2D();
            set => Parameters[nameof(ColorMap2)]?.SetValue(value);
        }

        public Texture2D NormalMap2
        {
            get => Parameters[nameof(NormalMap2)].GetValueTexture2D();
            set => Parameters[nameof(NormalMap2)]?.SetValue(value);
        }

        public Texture2D SpecularMap2
        {
            get => Parameters[nameof(SpecularMap2)].GetValueTexture2D();
            set => Parameters[nameof(SpecularMap2)]?.SetValue(value);
        }

        public Texture2D ShininessMap
        {
            get => Parameters[nameof(ShininessMap)].GetValueTexture2D();
            set => Parameters[nameof(ShininessMap)]?.SetValue(value);
        }

        public Texture2D ShininessMap2
        {
            get => Parameters[nameof(ShininessMap2)].GetValueTexture2D();
            set => Parameters[nameof(ShininessMap2)]?.SetValue(value);
        }

        public Texture2D EmissiveMap
        {
            get => Parameters[nameof(EmissiveMap)].GetValueTexture2D();
            set => Parameters[nameof(EmissiveMap)]?.SetValue(value);
        }

        public Texture2D EmissiveMap2
        {
            get => Parameters[nameof(EmissiveMap2)].GetValueTexture2D();
            set => Parameters[nameof(EmissiveMap2)]?.SetValue(value);
        }

        public Texture2D BlendmaskMap
        {
            get => Parameters[nameof(BlendmaskMap)].GetValueTexture2D();
            set => Parameters[nameof(BlendmaskMap)]?.SetValue(value);
        }

        public TextureCube EnvironmentMap
        {
            get => Parameters[nameof(EnvironmentMap)].GetValueTextureCube();
            set => Parameters[nameof(EnvironmentMap)]?.SetValue(value);
        }

        public Texture2D UVCheckMap
        {
            get => Parameters[nameof(UVCheckMap)].GetValueTexture2D();
            set => Parameters[nameof(UVCheckMap)]?.SetValue(value);
        }

        public Vector2 ColorMapScale
        {
            get => Parameters[nameof(ColorMapScale)].GetValueVector2();
            set => Parameters[nameof(ColorMapScale)]?.SetValue(value);
        }

        public Vector2 NormalMapScale
        {
            get => Parameters[nameof(NormalMapScale)].GetValueVector2();
            set => Parameters[nameof(NormalMapScale)]?.SetValue(value);
        }

        public Vector2 SpecularMapScale
        {
            get => Parameters[nameof(SpecularMapScale)].GetValueVector2();
            set => Parameters[nameof(SpecularMapScale)]?.SetValue(value);
        }

        public Vector2 ColorMapScale2
        {
            get => Parameters[nameof(ColorMapScale2)].GetValueVector2();
            set => Parameters[nameof(ColorMapScale2)]?.SetValue(value);
        }

        public Vector2 NormalMapScale2
        {
            get => Parameters[nameof(NormalMapScale2)].GetValueVector2();
            set => Parameters[nameof(NormalMapScale2)]?.SetValue(value);
        }

        public Vector2 SpecularMapScale2
        {
            get => Parameters[nameof(SpecularMapScale2)].GetValueVector2();
            set => Parameters[nameof(SpecularMapScale2)]?.SetValue(value);
        }

        public Vector2 ShininessMapScale
        {
            get => Parameters[nameof(ShininessMapScale)].GetValueVector2();
            set => Parameters[nameof(ShininessMapScale)]?.SetValue(value);
        }

        public Vector2 ShininessMapScale2
        {
            get => Parameters[nameof(ShininessMapScale2)].GetValueVector2();
            set => Parameters[nameof(ShininessMapScale2)]?.SetValue(value);
        }

        public Vector2 EmissiveMapScale
        {
            get => Parameters[nameof(EmissiveMapScale)].GetValueVector2();
            set => Parameters[nameof(EmissiveMapScale)]?.SetValue(value);
        }

        public Vector2 EmissiveMapScale2
        {
            get => Parameters[nameof(EmissiveMapScale2)].GetValueVector2();
            set => Parameters[nameof(EmissiveMapScale2)]?.SetValue(value);
        }

        public Vector2 BlendmaskMapScale
        {
            get => Parameters[nameof(BlendmaskMapScale)].GetValueVector2();
            set => Parameters[nameof(BlendmaskMapScale)]?.SetValue(value);
        }

        #endregion

        public Matrix[] BonesNew
        {
            get => Parameters[nameof(BonesNew)].GetValueMatrixArray(BoneMatrixSize);
            set => Parameters[nameof(BonesNew)]?.SetValue(value);
        }

        public FlverShader(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            
        }

        public FlverShader(GraphicsDevice graphicsDevice, byte[] effectCode, int index, int count) : base(graphicsDevice, effectCode, index, count)
        {
            
        }

        public FlverShader(Effect cloneSource) : base(cloneSource)
        {
            
        }

        public void ApplyWorldView(Matrix world, Matrix view, Matrix projection)
        {
            World = world;
            View = view;
            Projection = projection;
            //ViewInverse = Matrix.Invert(view);
            FlipSkybox = Matrix.CreateRotationX(MathHelper.Pi);
        }
    }
}
