#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

//#define NO_SKINNING

float DebugAnimWeight;

#define MAXLIGHTS 3

#define WORKFLOW_HIGHLIGHT -2

#define WORKFLOW_TEXDEBUG_DIFFUSEMAP 0
#define WORKFLOW_TEXDEBUG_SPECULARMAP 1
#define WORKFLOW_TEXDEBUG_NORMALMAP 2
#define WORKFLOW_TEXDEBUG_EMISSIVEMAP 3
#define WORKFLOW_TEXDEBUG_BLENDMASKMAP 4
#define WORKFLOW_TEXDEBUG_SHININESSMAP 5
#define WORKFLOW_TEXDEBUG_NORMALMAP_BLUE 6
#define WORKFLOW_TEXDEBUG_UVCHECK_0 10
#define WORKFLOW_TEXDEBUG_UVCHECK_1 11
#define WORKFLOW_TEXDEBUG_UVCHECK_2 12
#define WORKFLOW_TEXDEBUG_UVCHECK_3 13
#define WORKFLOW_TEXDEBUG_UVCHECK_4 14
#define WORKFLOW_TEXDEBUG_UVCHECK_5 15
#define WORKFLOW_TEXDEBUG_UVCHECK_6 16
#define WORKFLOW_TEXDEBUG_UVCHECK_7 17

#define WORKFLOW_MESHDEBUG_NORMALS 100
#define WORKFLOW_MESHDEBUG_NORMALS_MESH_ONLY 101
#define WORKFLOW_MESHDEBUG_VERTEX_COLOR_ALPHA 102
#define WORKFLOW_MESHDEBUG_VERTEX_COLOR_RGB 103

#define WORKFLOW_LEGACY 200
#define WORKFLOW_PBR_GLOSS 201
//#define WORKFLOW_PBR_GLOSS_BB 202
#define WORKFLOW_CLASSIC_DIFFUSE_PTDE 203

#define PTDE_MTD_TYPE_DEFAULT 0
#define PTDE_MTD_TYPE_METAL 1
#define PTDE_MTD_TYPE_WET 2
#define PTDE_MTD_TYPE_DULL 3

int Normal1TangentIdx = 1;
int Normal2TangentIdx = 1;


int NewDebugType = 0;
#define NewDebugTypes__None 0

#define NewDebugTypes__Albedo 100
#define NewDebugTypes__Specular 101
#define NewDebugTypes__Metalness 102
#define NewDebugTypes__Roughness 103
#define NewDebugTypes__Normals 104
#define NewDebugTypes__ReflectanceMult 105
#define NewDebugTypes__BlendMask 106
#define NewDebugTypes__PBR_Direct 300
#define NewDebugTypes__PBR_Indirect 301
#define NewDebugTypes__PBR_DirectDiffuse 302
#define NewDebugTypes__PBR_DirectSpecular 303
#define NewDebugTypes__PBR_IndirectDiffuse 304
#define NewDebugTypes__PBR_IndirectSpecular 305
#define NewDebugTypes__PBR_Diffuse 306
#define NewDebugTypes__PBR_Specular 307

#define NewDebugTypes__ShowTex 200

texture2D NewDebug_ShowTex_Tex;
sampler2D NewDebug_ShowTex_TexSampler = sampler_state
{
	Texture = <NewDebug_ShowTex_Tex>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
int NewDebug_ShowTex_UVIndex = 0;
float2 NewDebug_ShowTex_UVScale = float2(1.0, 1.0);
int NewDebug_ShowTex_ChannelConfig = 0;
#define NewDebug_ShowTex_ChannelConfigs__RGBA 0
#define NewDebug_ShowTex_ChannelConfigs__RGB 1
#define NewDebug_ShowTex_ChannelConfigs__R 2
#define NewDebug_ShowTex_ChannelConfigs__G 3
#define NewDebug_ShowTex_ChannelConfigs__B 4
#define NewDebug_ShowTex_ChannelConfigs__A 5





#define NewBlendOperations__Always0 0
#define NewBlendOperations__Always1 1
#define NewBlendOperations__Multiply 2
#define NewBlendOperations__Lerp 3
#define NewBlendOperations__Divide 4
#define NewBlendOperations__Add 5
#define NewBlendOperations__Subtract 6
#define NewBlendOperations__NormalMapBlend 7

int NewBlendOperation_Diffuse = NewBlendOperations__Multiply;
int NewBlendOperation_Specular = NewBlendOperations__Multiply;
int NewBlendOperation_Normal = NewBlendOperations__NormalMapBlend;
int NewBlendOperation_Shininess = NewBlendOperations__Multiply;
int NewBlendOperation_Emissive = NewBlendOperations__Multiply;

bool NewBlendReverseDir_Diffuse = false;
bool NewBlendReverseDir_Specular = false;
bool NewBlendReverseDir_Normal = false;
bool NewBlendReverseDir_Shininess = false;
bool NewBlendReverseDir_Emissive = false;

bool NewBlendInverseVal_Diffuse = false;
bool NewBlendInverseVal_Specular = false;
bool NewBlendInverseVal_Normal = false;
bool NewBlendInverseVal_Shininess = false;
bool NewBlendInverseVal_Emissive = false;

float4 GetTexMapBlend(float4 a, float4 b, float blend, int blendOperation, bool reverseDir, bool inverseBlendVal) : COLOR
{
    if (inverseBlendVal)
    {
        blend = 1 - blend;
    }
    
    if (reverseDir)
    {
        float4 tempA = a;
        a = b;
        b = tempA;
    }
    
    //blend *= b.a;
    
    if (blendOperation == NewBlendOperations__Always0)
    {
        return a;
    }
    else if (blendOperation == NewBlendOperations__Always1)
    {
        return b;
    }
    else if (blendOperation == NewBlendOperations__Lerp)
    {
        return float4(lerp(a.rgb, b.rgb, blend), a.a);
    }
    else if (blendOperation == NewBlendOperations__Multiply)
    {
        float3 multBy = b.rgb;
        multBy = sqrt(b.rgb);
        multBy *= 2;
        multBy *= multBy;
        return float4(lerp(a, a.rgb * multBy, blend), a.a);
    }
    else if (blendOperation == NewBlendOperations__Divide)
    {
        return float4(lerp(a, a.rgb / b.rgb, blend), a.a);
    }
    else if (blendOperation == NewBlendOperations__Add)
    {
        return float4(lerp(a, a.rgb + b.rgb, blend), a.a);
    }
    else if (blendOperation == NewBlendOperations__Subtract)
    {
        return float4(lerp(a, a.rgb - b.rgb, blend), a.a);
    }
    else if (blendOperation == NewBlendOperations__NormalMapBlend)
    {
        float glossChannel = lerp(a.b, b.b, blend);
        float alphaChannel = a.a;
        float3 normalMapA;
        normalMapA.xy = (a.yx * 2.0 - 1.0) * float2(1, 1);
        normalMapA.z =  sqrt(1.0 - min(dot(normalMapA.xy, normalMapA.xy), 1.0));
        normalMapA = normalize(normalMapA);
        
        float3 normalMapB;
        normalMapB.xy = (b.yx * 2.0 - 1.0) * float2(1, 1);
        normalMapB.z =  sqrt(1.0 - min(dot(normalMapB.xy, normalMapB.xy), 1.0));
        normalMapB = normalize(normalMapB);
        
        normalMapB = lerp(float3(0, 0, 1), normalMapB, blend);
        
        float3 blendedNormal = float3(normalMapA.x + normalMapB.x, normalMapA.y + normalMapB.y, normalMapA.z * normalMapB.z);
        blendedNormal = normalize(blendedNormal);
        
        return float4(blendedNormal.x, blendedNormal.y, glossChannel, alphaChannel);
    }

    
    return a;
}



#ifndef NO_SKINNING
cbuffer cbSkinned
{
    // This is really bad I'm sorry.
    // There's literally a second index to say which array to choose from
    // Since some bug prevents arrays from over 255 long from actually compiling 
    // right with MonoGame Content Pipeline
    float4x4 Bones0[255];
    float4x4 Bones1[255];
};


cbuffer cbSkinned1
{
    float4x4 Bones2[255];
    float4x4 Bones3[255];
};

cbuffer cbSkinned2
{
    float4x4 Bones4[255];
    float4x4 Bones5[255];
};
#endif

static const float Pi = 3.141592;
static const float Epsilon = 0.00001;
// Constant normal incidence Fresnel factor for all dielectrics.
//static const float3 Fdielectric = 0.04;

float DirectDiffuseMult = 1;
float DirectSpecularMult = 1;
float IndirectDiffuseMult = 1;
float IndirectSpecularMult = 1;

bool UseChrCustomize;
float4 ChrCustomizeColor;
bool ChrCustomizeUseNormalMapAlpha = false;

bool EnableSSS;
float4 SSSColor;
float SSSIntensity;

// Main
int WorkflowType = 0;
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 FlipSkybox;
bool IsSkybox = false;
bool EnableSkinning = true;

bool SwapNormalXY = true;

//Highlight
float3 HighlightColor;
float HighlightOpacity = 0;

// Legacy
float4 Legacy_AmbientColor;
float Legacy_AmbientIntensity;
float4 Legacy_DiffuseColor;
float Legacy_DiffuseIntensity;
float4 Legacy_SpecularColor;
float Legacy_SpecularPower;
float Legacy_DiffusePower;
float Legacy_DEBUG_ValueA;
float Legacy_NormalMapCustomZ;
float Legacy_SceneBrightness = 1.0;

// Shared
float3 LightDirection;
float3 EyePosition;
float AlphaTest;
float Opacity;
bool DisableAlpha;
float EnableBlendTextures; // Just multiplies lerp s I'm sorry.
bool EnableBlendMaskMap;
bool InvertBlendMaskMap;
bool EmissiveColorFromAlbedo;

bool UseShininessMap = false;

// Modern
float AmbientLightMult = 1.0;
float DirectLightMult = 1.0;
float IndirectLightMult = 1.0;
float EmissiveMapMult = 1.0;
float SceneBrightness = 1.0;

float LdotNPower = 0.1;

float SpecularPowerMult = 1;

float3 DiffuseMapColor;
float DiffuseMapColorPower;
float3 SpecularMapColor;
float SpecularMapColorPower;

float2 ColorMapScale;
float2 NormalMapScale;
float2 SpecularMapScale;
float2 ColorMapScale2;
float2 NormalMapScale2;
float2 SpecularMapScale2;
float2 ShininessMapScale;
float2 ShininessMapScale2;
float2 EmissiveMapScale;
float2 EmissiveMapScale2;
float2 BlendmaskMapScale;

//NEW ALPHA
bool FancyAlpha_Enable;
bool FancyAlpha_IsEdgeStep;
float FancyAlpha_EdgeCutoff = 0.5;

//DOUBLE FACE CLOTH 
bool IsDoubleFaceCloth;

int PtdeMtdType = 0;

// DS2
bool IsDS2NormalMapChannels = true;
bool IsDS2EmissiveFlow = true;

// Metallic
bool IsMetallic = false;
bool IsUndefinedMetallic = false;
float UndefinedMetallicValue = 0.5;
float3 NonMetallicSpecColor = float3(0.04, 0.04, 0.04);


bool IsUndefinedBlendMask = false;
float UndefinedBlendMaskValue = 0;
bool BlendMaskFromNormalMap1Alpha = false;
bool BlendMaskFromNormalMap1Alpha_IsReverse = false;
bool BlendMaskMultByAlbedoMap2Alpha = false;
bool BlendMaskMultByAlbedoMap2Alpha_IsReverse = false;
bool IsReflectMultInNormalAlpha = false;

// DS1R
bool IsDS1R = false;


//UV Index Stuff
int Albedo1UVIndex = 0;
int Albedo2UVIndex = 1;
int Specular1UVIndex = 0;
int Specular2UVIndex = 1;
int Shininess1UVIndex = 0;
int Shininess2UVIndex = 1;
int Normal1UVIndex = 0;
int Normal2UVIndex = 1;
int Emissive1UVIndex = 0;
int Emissive2UVIndex = 1;
int BlendMaskUVIndex = 0;

//Various Memes


texture2D Mask3Map;
sampler2D Mask3MapSampler = sampler_state
{
	Texture = <Mask3Map>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};
int Mask3UVIndex = 0;
float2 Mask3MapScale;


// Textures
texture2D ColorMap;
sampler2D ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D ColorMap2;
sampler2D ColorMap2Sampler = sampler_state
{
	Texture = <ColorMap2>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D NormalMap;
sampler2D NormalMapSampler = sampler_state
{
	Texture = <NormalMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D NormalMap2;
sampler2D NormalMap2Sampler = sampler_state
{
	Texture = <NormalMap2>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D SpecularMap;
sampler2D SpecularMapSampler = sampler_state
{
	Texture = <SpecularMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D SpecularMap2;
sampler2D SpecularMap2Sampler = sampler_state
{
	Texture = <SpecularMap2>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D ShininessMap;
sampler2D ShininessMapSampler = sampler_state
{
	Texture = <ShininessMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D ShininessMap2;
sampler2D ShininessMap2Sampler = sampler_state
{
	Texture = <ShininessMap2>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

textureCUBE EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
	Texture = <EnvironmentMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D EmissiveMap;
sampler2D EmissiveMapSampler = sampler_state
{
	Texture = <EmissiveMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D EmissiveMap2;
sampler2D EmissiveMap2Sampler = sampler_state
{
	Texture = <EmissiveMap2>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D UVCheckMap;
sampler2D UVCheckMapSampler = sampler_state
{
	Texture = <UVCheckMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

texture2D BlendmaskMap;
sampler2D BlendmaskMapSampler = sampler_state
{
	Texture = <BlendmaskMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// The input for the VertexShader
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float3 Binormal : BINORMAL0;
	float4 Bitangent : TANGENT0;
    float4 Color : COLOR0;
	float4 BoneIndices : BLENDINDICES;
    float4 BoneWeights : BLENDWEIGHT;
    float4 BoneIndicesBank : BLENDINDICES1;
    float2 TexCoord : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
    float2 TexCoord3 : TEXCOORD2;
    float2 TexCoord4 : TEXCOORD3;
    float2 TexCoord5 : TEXCOORD4;
    float2 TexCoord6 : TEXCOORD5;
    float2 TexCoord7 : TEXCOORD6;
    float2 TexCoord8 : TEXCOORD7;
    float4 Bitangent2 : TANGENT1;
    float3 Binormal2 : BINORMAL1;
    //float4x4 InstanceWorld : TEXCOORD2;
    //float4x4 InstanceWorldInverse : TEXCOORD6;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    
    float2 TexCoord : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
    float2 TexCoord3 : TEXCOORD2;
    float2 TexCoord4 : TEXCOORD3;
    float2 TexCoord5 : TEXCOORD4;
    float2 TexCoord6 : TEXCOORD5;
    float2 TexCoord7 : TEXCOORD6;
    float2 TexCoord8 : TEXCOORD7;
    
	float3 View : TEXCOORD8;
	float3x3 WorldToTangentSpace : TEXCOORD9;
	float3 Normal : NORMAL0;
    float4 Bitangent : TANGENT0;
	float4 Color : TEXCOORD12;
    float3x3 WorldToTangentSpace2 : TEXCOORD13;
    float4 Bitangent2 : TANGENT1;
    
};

float4 SkinVert(VertexShaderInput input, float4 v)
{
    [branch]
    if (IsSkybox || !EnableSkinning)
    {
        return v;
    }
    
    if (!any(input.BoneWeights))
    {
        return v;
    }
    
    //TEMP DISABLED FOR COLOR TESTING
    #ifndef NO_SKINNING
    float4 posA;
    float4 posB;
    float4 posC;
    float4 posD;
    
    if (input.BoneIndicesBank.x == 0)
    {
        posA = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else if (input.BoneIndicesBank.x == 1)
    {
        posA = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else if (input.BoneIndicesBank.x == 2)
    {
        posA = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else if (input.BoneIndicesBank.x == 3)
    {
        posA = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else if (input.BoneIndicesBank.x == 4)
    {
        posA = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones4[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else if (input.BoneIndicesBank.x == 5)
    {
        posA = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones5[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else
    {
        return v;
    }
    
    if (input.BoneIndicesBank.y == 0)
    {
        posB = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else if (input.BoneIndicesBank.y == 1)
    {
        posB = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else if (input.BoneIndicesBank.y == 2)
    {
        posB = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else if (input.BoneIndicesBank.y == 3)
    {
        posB = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else if (input.BoneIndicesBank.y == 4)
    {
        posB = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones4[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else if (input.BoneIndicesBank.y == 5)
    {
        posB = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones5[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else
    {
        return v;
    }
    
    if (input.BoneIndicesBank.z == 0)
    {
        posC = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else if (input.BoneIndicesBank.z == 1)
    {
        posC = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else if (input.BoneIndicesBank.z == 2)
    {
        posC = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else if (input.BoneIndicesBank.z == 3)
    {
        posC = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else if (input.BoneIndicesBank.z == 4)
    {
        posC = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones4[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else if (input.BoneIndicesBank.z == 5)
    {
        posC = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones5[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else
    {
        return v;
    }
    
    if (input.BoneIndicesBank.w == 0)
    {
        posD = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 1)
    {
        posD = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 2)
    {
        posD = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 3)
    {
        posD = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 4)
    {
        posD = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones4[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 5)
    {
        posD = mul(v, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones5[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else
    {
        return v;
    }
    
    return lerp(v, ((posA + posB + posC + posD) / (input.BoneWeights.x + input.BoneWeights.y + input.BoneWeights.z + input.BoneWeights.w)), DebugAnimWeight);
    #else
    return v;
    #endif
    //return v;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
    
    float4 inPos = SkinVert(input, input.Position);

    

    //inPos.xyz = mul(inPos, skinning);

    //inPos.xyz = mul(inPos.xyz, skinning);

	//float4 worldPosition = mul(mul(inPos, transpose(input.InstanceWorld)), World);
    float4 worldPosition = mul(inPos, World);
    
    //output.PositionWS = worldPosition.xyz;
    
    float4 viewPosition = mul(worldPosition, View);
    
    
    
    output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.TexCoord2 = input.TexCoord2;// * input.AtlasScale.xy + input.AtlasOffset.xy;
    output.TexCoord3 = input.TexCoord3;
    output.TexCoord4 = input.TexCoord4;
    output.TexCoord5 = input.TexCoord5;
    output.TexCoord6 = input.TexCoord6;
    output.TexCoord7 = input.TexCoord7;
    output.TexCoord8 = input.TexCoord8;
    
    //[branch]
    //if (WorkflowType == WORKFLOW_MESHDEBUG_NORMALS_MESH_ONLY)
    //{
    //    output.Normal = normalize(mul(SkinVert(input, float4(input.Normal, 0)).xyz, World));
    //}
    
    output.Normal = normalize(mul(SkinVert(input, float4(input.Normal, 0)).xyz, World));

	output.WorldToTangentSpace[0] = mul(normalize(SkinVert(input, input.Bitangent).xyz), World);
	output.WorldToTangentSpace[1] = mul(normalize(SkinVert(input, float4(input.Binormal, 0)).xyz), World);
	output.WorldToTangentSpace[2] = mul(normalize(SkinVert(input, float4(input.Normal, 0)).xyz), World);
	
    output.WorldToTangentSpace2[0] = mul(normalize(SkinVert(input, input.Bitangent2).xyz), World);
	output.WorldToTangentSpace2[1] = mul(normalize(SkinVert(input, float4(input.Binormal2, 0)).xyz), World);
	output.WorldToTangentSpace2[2] = output.WorldToTangentSpace[2];
    
    //output.WorldToTangentSpace = mul(output.WorldToTangentSpace, View);

	output.View = normalize(worldPosition - EyePosition);
    //output.Bitangent = float4(normalize(SkinVert(input, float4(input.Bitangent.xyz, 0)).xyz), input.Bitangent.w);//input.Bitangent;
	
    output.Color = input.Color;
    
    //output.Normal = normalize(mul(output.Normal, World));
    //output.Bitangent = normalize(mul(output.Bitangent, World));
    //output.Normal = normalize(mul(output.Normal, World));

	//output.Normal = mul((float3x3)skinning, output.Normal);
	//output.DebugColor.xy = input.AtlasScale.xy;
	//output.DebugColor.zw = input.AtlasOffset.xy;
    
    //output.Position.xy
    
    //output.WorldToTangentSpace = mul(output.WorldToTangentSpace, skinning);
    
    //output.DebugColor = input.BoneIndices;
    
    output.Bitangent = input.Bitangent;
    output.Bitangent2 = input.Bitangent2;
    
    return output;
}

float B2(float2 avCoords)
{
    return fmod(2.0 * avCoords.y + avCoords.x + 1.0, 4.0);
}

float B16(float2 avCoords)
{
    float2 P1 = fmod(avCoords, 2.0);
    float2 P2 = floor(0.5 * fmod(avCoords, 4.0));
    float2 P4 = floor(0.25 * fmod(avCoords, 8.0));
    float2 P8 = floor(0.125 * fmod(avCoords, 16.0));
    return (1.0 / 255.0) * (4.0 * (4.0 * (4.0 * B2(P1) + B2(P2)) + B2(P4)) + B2(P8));
}

float4 Debug(float3 dbgVec)
{
    return float4(dbgVec * float3(0.5, 0.5, 0.5) + float3(0.5, 0.5, 0.5), 1);
}

float2 GetUV(VertexShaderOutput input, int uvIndex)
{
   
    if (uvIndex == 0)
    {
        return input.TexCoord;
    }
    else if (uvIndex == 1)
    {
        return input.TexCoord2;
    }
    else if (uvIndex == 2)
    {
        return input.TexCoord3;
    }
    else if (uvIndex == 3)
    {
        return input.TexCoord4;
    }
    else if (uvIndex == 4)
    {
        return input.TexCoord5;
    }
    else if (uvIndex == 5)
    {
        return input.TexCoord6;
    }
    else if (uvIndex == 6)
    {
        return input.TexCoord7;
    }
    else if (uvIndex == 7)
    {
        return input.TexCoord8;
    }
    else
    {
        return input.TexCoord;
    }
}

float4 GetNewDebugTextureOutput(VertexShaderOutput input) : COLOR
{
    float2 uv = GetUV(input, NewDebug_ShowTex_UVIndex);
    float4 color = tex2D(NewDebug_ShowTex_TexSampler, uv * NewDebug_ShowTex_UVScale);
    
    [branch]
    if (NewDebug_ShowTex_ChannelConfig == NewDebug_ShowTex_ChannelConfigs__RGBA)
    {
        return color;
    }
    else if (NewDebug_ShowTex_ChannelConfig == NewDebug_ShowTex_ChannelConfigs__RGB)
    {
        return float4(color.rgb, 1);
    }
    else if (NewDebug_ShowTex_ChannelConfig == NewDebug_ShowTex_ChannelConfigs__R)
    {
        color = float4(color.r, color.r, color.r, 1);
    }
    else if (NewDebug_ShowTex_ChannelConfig == NewDebug_ShowTex_ChannelConfigs__G)
    {
        color = float4(color.g, color.g, color.g, 1);
    }
    else if (NewDebug_ShowTex_ChannelConfig == NewDebug_ShowTex_ChannelConfigs__B)
    {
        color = float4(color.b, color.b, color.b, 1);
    }
    else if (NewDebug_ShowTex_ChannelConfig == NewDebug_ShowTex_ChannelConfigs__A)
    {
        color = float4(color.a, color.a, color.a, 1);
    }
    
    return color;
}

//u = main normal map, t is sub normal map, s = geom normal
float3 ReorientNormal(in float3 u, in float3 t, in float3 s)
{
    // Build the shortest-arc quaternion
    float4 q = float4(cross(s, t), dot(s, t) + 1) / sqrt(2 * (dot(s, t) + 1));
    // Rotate the normal
    return u * (q.w * q.w - dot(q.xyz, q.xyz)) + 2 * q.xyz * dot(q.xyz, u) + 2 * q.w * cross(q.xyz, u);
}

float ColorEpsilon = 1e-10;
 
float3 RGBtoHCV(in float3 RGB)
{
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + ColorEpsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 RGBtoHSL(in float3 RGB)
{
    float3 HCV = RGBtoHCV(RGB);
    float L = HCV.z - HCV.y * 0.5;
    float S = HCV.y / (1 - abs(L * 2 - 1) + ColorEpsilon);
    return float3(HCV.x, S, L);
}

float3 HUEtoRGB(in float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}


float3 HSVtoRGB(in float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}


float3 HSLtoRGB(in float3 HSL)
{
    float3 RGB = HUEtoRGB(HSL.x);
    float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
    return (RGB - 0.5) * C + HSL.z;
}

float4 GetMainColor(VertexShaderOutput input, bool isFrontFacing : SV_IsFrontFace) : COLOR
{
    float2 uv_Albedo1 = GetUV(input, Albedo1UVIndex);
    float2 uv_Albedo2 = GetUV(input, Albedo2UVIndex);
    float2 uv_Specular1 = GetUV(input, Specular1UVIndex);
    float2 uv_Specular2 = GetUV(input, Specular2UVIndex);
    float2 uv_Normal1 = GetUV(input, Normal1UVIndex);
    float2 uv_Normal2 = GetUV(input, Normal2UVIndex);
    float2 uv_Shininess1 = GetUV(input, Shininess1UVIndex);
    float2 uv_Shininess2 = GetUV(input, Shininess2UVIndex);
    float2 uv_Emissive1 = GetUV(input, Emissive1UVIndex);
    float2 uv_Emissive2 = GetUV(input, Emissive2UVIndex);
    float2 uv_BlendMask = GetUV(input, BlendMaskUVIndex);
    float2 uv_Mask3 = GetUV(input, Mask3UVIndex);
    
    float3 mask3 = tex2D(Mask3MapSampler, uv_Mask3 * Mask3MapScale);
    
    [branch]
    if (NewDebugType == NewDebugTypes__ShowTex)
    {
        return GetNewDebugTextureOutput(input);
    }

    float4 nmapcol_full = tex2D(NormalMapSampler, uv_Normal1 * NormalMapScale);
    
    float4 nmapcol_full_2 = tex2D(NormalMap2Sampler, uv_Normal2 * NormalMapScale2);
    
    //Test 
    //return float4(nmapcol_full_2.rgb, 1);
    
    float4 blendmaskColor = tex2D(BlendmaskMapSampler, uv_BlendMask * BlendmaskMapScale);

    float texBlendVal = 0;

    [branch]
	if (IsDoubleFaceCloth)
	{
		if (!isFrontFacing)
		{
			texBlendVal = 0;
		}
		else
		{
			texBlendVal = 1;
		}
	}
    else if (EnableBlendMaskMap)
    {
        texBlendVal = blendmaskColor.r;
    }
    else if (WorkflowType == WORKFLOW_CLASSIC_DIFFUSE_PTDE)
    {
        texBlendVal = (input.Color.a * EnableBlendTextures);
    }
    
    [branch]
    if (InvertBlendMaskMap)
    {
        texBlendVal = 1 - texBlendVal;
    }
    
    [branch]
    if (BlendMaskFromNormalMap1Alpha)
    {
        texBlendVal = nmapcol_full.a;
        [branch]
        if (BlendMaskFromNormalMap1Alpha_IsReverse)
        {
            texBlendVal = 1 - texBlendVal;
        }
    }
    else if (IsUndefinedBlendMask)
    {
        texBlendVal = UndefinedBlendMaskValue;
    }
    
    float4 tex1AlbedoColor = tex2D(ColorMapSampler, uv_Albedo1 * ColorMapScale);
    float4 tex2AlbedoColor = tex2D(ColorMap2Sampler, uv_Albedo2 * ColorMapScale2);
    
    [branch]
    if (BlendMaskMultByAlbedoMap2Alpha)
    {
        //tex2AlbedoColor *= 4;
        //float3 testMult = sqrt(tex2AlbedoColor) * 2;
        //testMult *= testMult;
        ////return float4(1,1,1, 1);
        //return float4(testMult, 1);
        //
        //// Test - will delete
        //float3 albedoMod = tex2AlbedoColor.rgb;
        ////[0,1] --> [-0.5,0.5]
        //albedoMod = albedoMod - float3(0.5,0.5,0.5);
        //
        //albedoMod *= 0.25;
        //
        //float3 newAlbedo = tex1AlbedoColor.rgb + albedoMod;
        //return float4(newAlbedo, 1);
        //
        //
        float blendMult = tex2AlbedoColor.a;
        [branch]
        if (NewBlendReverseDir_Diffuse)
        {
            blendMult = tex1AlbedoColor.a;
        }
        [branch]
        if (BlendMaskMultByAlbedoMap2Alpha_IsReverse)
        {
            blendMult = 1 - blendMult;
        }
        texBlendVal *= blendMult;
    }
    
	float4 color = GetTexMapBlend(tex1AlbedoColor, tex2AlbedoColor, texBlendVal, NewBlendOperation_Diffuse, NewBlendReverseDir_Diffuse, NewBlendInverseVal_Diffuse);
    color.rgb = color.rgb * DiffuseMapColor * DiffuseMapColorPower;
    
    if (UseChrCustomize)
    {
        color.rgb *= ChrCustomizeColor.rgb;
        [branch]
        if (ChrCustomizeUseNormalMapAlpha)
        {
            color.a = nmapcol_full.a * ChrCustomizeColor.a;
        }
        else
        {
            color.a = color.a * ChrCustomizeColor.a;
        }
        
    }
    
    float inputDiffuseMapAlpha = color.a;
    
    [branch]
    if (IsDS2NormalMapChannels && IsDS2EmissiveFlow)
    {
        color.a = 1;
    }


    //color = pow(color, 1.0 / 2.2);
    
    float inputTexAlpha = 1;
    
    [branch]
    if (!DisableAlpha)
    {
        inputTexAlpha = color.a;
    }
    
    //inputTexAlpha = 1 - inputTexAlpha;
    //inputTexAlpha = pow(inputTexAlpha, 2);
    //inputTexAlpha = 1 - inputTexAlpha;
    //inputTexAlpha *= 0.5;
    
	[branch]
	if (FancyAlpha_Enable)
	{
		[branch]
		if (FancyAlpha_IsEdgeStep)
		{
			if (inputTexAlpha > FancyAlpha_EdgeCutoff)
			{
				clip(-1);
			}
		}
		else
		{
			if (inputTexAlpha <= FancyAlpha_EdgeCutoff)
			{
				clip(-1);
			}
			
			color.a = 1;
		}

        float dissolve = B16(input.Position.xy);

        if ((((Opacity) * 1.05) + 0.125) < dissolve)
		{
			clip(-1);
		}
	}
	else
	{
		color.a = 1;
    
		float dissolve = B16(input.Position.xy);
		
		if ((((Opacity * inputTexAlpha) * 1.05) + 0.125) < dissolve)
		{
			clip(-1);
		}
	}
	

	
    [branch]
	if (WorkflowType == WORKFLOW_HIGHLIGHT)
	{
        //if ((((inputTexAlpha) * 1.05) + 0.125) < dissolve)
        //{
        //    clip(-1);
        //}
		
		return float4(HighlightColor.r, HighlightColor.g, HighlightColor.b, Opacity);
	}
        
    
    
    [branch]
    if (IsSkybox)
    {
        return color;
    }

    
    

    float4 specularMapColor_Source = GetTexMapBlend(tex2D(SpecularMapSampler, uv_Specular1 * SpecularMapScale), tex2D(SpecularMap2Sampler, uv_Specular2 * SpecularMapScale2), texBlendVal, NewBlendOperation_Specular, NewBlendReverseDir_Specular, NewBlendInverseVal_Specular);
    
    
    
    float4 specularMapColor = specularMapColor_Source;
    
    specularMapColor.rgb = specularMapColor.rgb * SpecularMapColor * SpecularMapColorPower;
    
    float4 emissiveMapColor = GetTexMapBlend(tex2D(EmissiveMapSampler, uv_Emissive1 * EmissiveMapScale), tex2D(EmissiveMap2Sampler, uv_Emissive2 * EmissiveMapScale2), texBlendVal, NewBlendOperation_Emissive, NewBlendReverseDir_Emissive, NewBlendInverseVal_Emissive);
    
    emissiveMapColor.rgb *= emissiveMapColor.rgb;
    
    [branch]
    if (IsDS2NormalMapChannels && IsDS2EmissiveFlow)
    {
        emissiveMapColor *= inputDiffuseMapAlpha;
    }

    [branch]
    if (EmissiveColorFromAlbedo)
    {
        emissiveMapColor.rgb *= color.rgb;
    }

    
    
    
    float3 nmapcol = nmapcol_full.rgb;
    float3 nmapcol_2 = nmapcol_full_2.rgb;

    [branch]
    if (IsDS2NormalMapChannels)
    {
        nmapcol.r = nmapcol_full.a;
        nmapcol.g = nmapcol.g;
        nmapcol.r = nmapcol_full.a;
        nmapcol.g = nmapcol.g;

        float _specPower = max(specularMapColor_Source.r * 256, 1);
        float _specPowerCalc = (_specPower * _specPower * 0.01);
        _specPowerCalc += _specPowerCalc;
        _specPowerCalc = max(_specPowerCalc, 1);

        nmapcol.b = clamp(log2(_specPowerCalc) / 13, 0, 1);

        color.rgb = color.rgb * 0.75;

        specularMapColor.rgb = float3(1,1,1) * specularMapColor_Source.g;
        specularMapColor.a = 1;
    }
    
    [branch]
    if (IsDS2NormalMapChannels)
    {
        nmapcol_2.r = nmapcol_full_2.a;
        nmapcol_2.g = nmapcol_2.g;
        nmapcol_2.r = nmapcol_full_2.a;
        nmapcol_2.g = nmapcol_2.g;

        float _specPower = max(specularMapColor_Source.r * 256, 1);
        float _specPowerCalc = (_specPower * _specPower * 0.01);
        _specPowerCalc += _specPowerCalc;
        _specPowerCalc = max(_specPowerCalc, 1);

        nmapcol_2.b = clamp(log2(_specPowerCalc) / 13, 0, 1);
    }

    

    float4 shininessMapColor = GetTexMapBlend(tex2D(ShininessMapSampler, uv_Shininess1 * ShininessMapScale), tex2D(ShininessMap2Sampler, uv_Shininess2 * ShininessMapScale2), texBlendVal, NewBlendOperation_Shininess, NewBlendReverseDir_Shininess, NewBlendInverseVal_Shininess);
    
    float3 normalMap;
    [branch]
    if (SwapNormalXY)
    {
        normalMap.xy = (nmapcol.yx * 2.0 - 1.0) * float2(1, 1);
    }
    else
    {
        normalMap.xy = (nmapcol.xy * 2.0 - 1.0) * float2(1, 1);
    }
    normalMap.z =  max(sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0)), 0);
    normalMap = normalize(normalMap);
    //normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
    
    float3 normalMap_2;
    [branch]
    if (SwapNormalXY)
    {
        normalMap_2.xy = (nmapcol_2.yx * 2.0 - 1.0) * float2(1, 1);
    }
    else
    {
        normalMap_2.xy = (nmapcol_2.xy * 2.0 - 1.0) * float2(1, 1);
    }
    normalMap_2.z =  max(sqrt(1.0 - min(dot(normalMap_2.xy, normalMap_2.xy), 1.0)), 0);
    normalMap_2 = normalize(normalMap_2);
    //normalMap_2 = normalize(mul(normalMap_2, input.WorldToTangentSpace));
    
    //return float4(float3(input.Bitangent.w * 0.5 + 0.5, input.Bitangent.w * 0.5 + 0.5, input.Bitangent.w * 0.5 + 0.5), 1);
    
    //if (input.Bitangent.w > 0)
    //{
    //    normalMap.y = -normalMap.y;
    //    normalMap_2.y = -normalMap_2.y;
    //}
    
    float normalBlendVal = texBlendVal;
    
    [branch]
    if (NewBlendReverseDir_Normal)
    {
        float3 tempNormal1 = normalMap;
        normalMap = normalMap_2;
        normalMap_2 = tempNormal1;
    }
    
    [branch]
    if (NewBlendInverseVal_Normal)
    {
        normalBlendVal = 1 - normalBlendVal;
    }
    
    float normal1Tangent = input.Bitangent.w;
    float3x3 normal1TanMatrix = input.WorldToTangentSpace;
    
    [branch]
    if (Normal1TangentIdx == 1)
    {
        normal1Tangent = input.Bitangent2.w;
        normal1TanMatrix = input.WorldToTangentSpace2;
    }
    
    if (normal1Tangent > 0)
    {
        normalMap.y *= -1;
    }
    
    float normal2Tangent = input.Bitangent.w;
    float3x3 normal2TanMatrix = input.WorldToTangentSpace;
    
    [branch]
    if (Normal2TangentIdx == 1)
    {
        normal2Tangent = input.Bitangent2.w;
        normal2TanMatrix = input.WorldToTangentSpace2;
    }
    
    if (normal2Tangent > 0)
    {
        normalMap_2.y *= -1;
    }
    
    
    
    [branch]
    if (NewBlendOperation_Normal == NewBlendOperations__Always0)
    {
        normalMap = normalize(mul(normalMap, normal1TanMatrix));
        
    }
    else if (NewBlendOperation_Normal == NewBlendOperations__Always1)
    {
        normalMap = normalize(mul(normalMap_2, normal2TanMatrix));
    }
    else if (NewBlendOperation_Normal == NewBlendOperations__NormalMapBlend)
    {
        normalMap_2 = lerp(float3(0, 0, 1), normalMap_2, normalBlendVal);
    
        //normalMap = float3(normalMap.xy + normalMap_2.xy, normalMap.z * normalMap_2.z);
        //normalMap = normalize(normalMap);
        
        float3 normal1Tan = mul(normalMap, normal1TanMatrix);
        float3 normal2Tan = mul(normalMap_2, normal2TanMatrix);
        //normalMap = normal1Tan;//normalize(lerp(normal1Tan, normal2Tan, normalBlendVal));
    
        normalMap = normalize(ReorientNormal(normal1Tan, normal2Tan, input.Normal));
    }
    
    //return float4((normalMap.rgb * 0.5 + 0.5), 1);
    
    
    //float3 normal1Tan = mul(normalMap, normal1TanMatrix);
    //float3 normal2Tan = mul(normalMap_2, normal2TanMatrix);
    //normalMap = normal1Tan;//normalize(lerp(normal1Tan, normal2Tan, normalBlendVal));
    
    //normalMap = ReorientNormal(normal1Tan, normal2Tan, mul(input.Normal, normal2TanMatrix));
    
    
    
    float3 N = (!isFrontFacing ? normalMap : -normalMap);
    

    [branch]
    if (WorkflowType == WORKFLOW_TEXDEBUG_DIFFUSEMAP)
    {
        return color;
    }
    else if (WorkflowType == WORKFLOW_TEXDEBUG_SPECULARMAP)
    {
        return float4(specularMapColor.rgb, color.a);
    }
    else if (WorkflowType == WORKFLOW_TEXDEBUG_NORMALMAP)
    {
        return float4(nmapcol.x, nmapcol.y, 1, color.a);
    }
    else if (WorkflowType == WORKFLOW_TEXDEBUG_EMISSIVEMAP)
    {
        return float4(emissiveMapColor.rgb, color.a);
    }
    else if (WorkflowType == WORKFLOW_TEXDEBUG_BLENDMASKMAP)
    {
        return float4(blendmaskColor.rgb, color.a);
    }
    else if (WorkflowType == WORKFLOW_TEXDEBUG_SHININESSMAP)
    {
        return float4(shininessMapColor.rgb, color.a);
    }
    else if (WorkflowType == WORKFLOW_TEXDEBUG_NORMALMAP_BLUE)
    {
        return float4(nmapcol.b, nmapcol.b, nmapcol.b, color.a);
    }
	else if (WorkflowType >= WORKFLOW_TEXDEBUG_UVCHECK_0 && WorkflowType <= WORKFLOW_TEXDEBUG_UVCHECK_7)
	{
		return float4(tex2D(UVCheckMapSampler, GetUV(input, WorkflowType - WORKFLOW_TEXDEBUG_UVCHECK_0)).xyz, color.a);
	}
    else if (WorkflowType == WORKFLOW_MESHDEBUG_NORMALS)
    {
        return float4(N * float3(1, 1, 1) * 0.25 + 0.5, color.a);
    }
    else if (WorkflowType == WORKFLOW_MESHDEBUG_NORMALS_MESH_ONLY)
    {
        return float4((input.Normal * float3(1,1,1)) * 0.25 + 0.5, color.a);
    }
    else if (WorkflowType == WORKFLOW_MESHDEBUG_VERTEX_COLOR_ALPHA)
    {
        return float4(input.Color.a, input.Color.a, input.Color.a, color.a);
    }
    else if (WorkflowType == WORKFLOW_MESHDEBUG_VERTEX_COLOR_RGB)
    {
        return float4(input.Color.r, input.Color.g, input.Color.b, color.a);
    }
    else if (WorkflowType == WORKFLOW_PBR_GLOSS)
    {
        //float3 normal = input.Normal;
        
        float normalMapBlueChannel = nmapcol.z;
        
        [branch]
        if (NewBlendOperation_Normal == NewBlendOperations__Always0)
        {
            normalMapBlueChannel = nmapcol.z;
        }
        else if (NewBlendOperation_Normal == NewBlendOperations__Always1)
        {
            normalMapBlueChannel = nmapcol_2.z;
        }
        else if (NewBlendOperation_Normal == NewBlendOperations__NormalMapBlend)
        {
            normalMapBlueChannel = lerp(nmapcol.z, nmapcol.z * nmapcol_2.z, normalBlendVal);
        }
        
        [branch]
        if (UseShininessMap)
        {
            normalMapBlueChannel = shininessMapColor.r;
            
            //test
            //return float4(shininessMapColor.rgb, 1);
        }
        
        float roughness = 1 - normalMapBlueChannel;
        
        //return float4(N * 0.25 + 0.5, 1);

        //return float4((N * float3(1,1,1)) * 0.6666 + 0.3333, 1);

        float3 viewVec = -normalize(input.View);
        float3 diffuseColor = color.xyz;// * color.xyz;
        float3 L = -LightDirection;
        
        float3 H = normalize(L + viewVec);
        
        float3 F0 = float3(0,0,0);
        
        
        
        [branch]
        if (IsDS1R)
        {
            //specularMapColor.rgb = specularMapColor.rgb * specularMapColor.rgb;
            
            float metallic = specularMapColor.g;// * specularMapColor.r;
            metallic = metallic * metallic;
            //metallic = 0;
            float nonMetallic = specularMapColor.b;
            //nonMetallic = nonMetallic * nonMetallic;
            float ds1rGloss = 1 - specularMapColor.r;
            //ds1rGloss = ds1rGloss * ds1rGloss;
            roughness = 1 - ds1rGloss;
            roughness = roughness * roughness;
            
            
            diffuseColor.rgb = diffuseColor.rgb * diffuseColor.rgb;
            //specularMapColor.rgb = specularMapColor.rgb * specularMapColor.rgb;
            specularMapColor.rgb = lerp(float3(nonMetallic, nonMetallic, nonMetallic), diffuseColor.rgb, metallic);
            //specularMapColor.rgb = specularMapColor.rgb * specularMapColor.rgb;
            //diffuseColor.rgb = diffuseColor.rgb * diffuseColor.rgb;
            //diffuseColor.rgb = diffuseColor.rgb * diffuseColor.rgb;
            
            float diffuseMult = 1.0 - metallic;
            //diffuseMult = diffuseMult;
            diffuseColor.rgb *= float3(diffuseMult, diffuseMult, diffuseMult);
            
            //diffuseColor.rgb = diffuseColor.rgb * diffuseColor.rgb;
            //specularMapColor.rgb = specularMapColor.rgb * specularMapColor.rgb;
            
            //return float4(specularMapColor.rgb, 1);
            //return float4(diffuseColor.rgb, 1);
            //return float4(roughness, 0, 0, 1);
        }
        else if (IsMetallic)
        {
            float metallic = specularMapColor.r;// * specularMapColor.r;
            
            [branch]
            if (IsUndefinedMetallic)
            {
                metallic = UndefinedMetallicValue;
            }
            
            if (NewDebugType == NewDebugTypes__Metalness)
            {
                return float4(metallic, metallic, metallic, 1);
            }
            
            specularMapColor.rgb = lerp(NonMetallicSpecColor, diffuseColor.rgb, metallic);
            diffuseColor.rgb *= float3(1.0 - metallic, 1.0 - metallic, 1.0 - metallic);
        }
        
        float reflectanceMultFromNormalAlpha = 1;
        
        [branch]
        if (IsReflectMultInNormalAlpha)
        {
            [branch]
            if (NewBlendOperation_Normal == NewBlendOperations__Always0)
            {
                reflectanceMultFromNormalAlpha = nmapcol_full.a;
            }
            else if (NewBlendOperation_Normal == NewBlendOperations__Always1)
            {
                reflectanceMultFromNormalAlpha = nmapcol_full_2.a;
            }
            else if (NewBlendOperation_Normal == NewBlendOperations__NormalMapBlend)
            {
                reflectanceMultFromNormalAlpha = lerp(nmapcol_full.a, nmapcol_full.a * nmapcol_full_2.a, normalBlendVal);
            }
        }
        specularMapColor.rgb *= reflectanceMultFromNormalAlpha;
        
        
        [branch]
        if (NewDebugType == NewDebugTypes__Albedo)
        {
            return float4(diffuseColor.rgb, 1);
        }
        else if (NewDebugType == NewDebugTypes__Specular)
        {
            return float4(specularMapColor.rgb, 1);
        }
        else if (NewDebugType == NewDebugTypes__Roughness)
        {
            return float4(roughness, roughness, roughness, 1);
        }
        else if (NewDebugType == NewDebugTypes__Normals)
        {
            return float4(N * float3(1, 1, 1) * 0.25 + 0.5, 1);
        }
        else if (NewDebugType == NewDebugTypes__BlendMask)
        {
            return float4(texBlendVal, texBlendVal, texBlendVal, 1);
        }
        else if (NewDebugType == NewDebugTypes__ReflectanceMult)
        {
            return float4(reflectanceMultFromNormalAlpha, reflectanceMultFromNormalAlpha, reflectanceMultFromNormalAlpha, 1);
        }
        
        F0 = specularMapColor.rgb;
        
        
        // Original
        //float LdotN = saturate(dot(N, L));
        //float3 finalDiffuse = diffuseColor * (LdotN);
        float LdotN = dot(L, N);
        float3 finalDiffuse;
        [branch]
        if (EnableSSS)
        {
            float sssMask = mask3.r;
            
            //float3 sssWrap = SSSColor.rgb * sssMask * SSSIntensity;
            //finalDiffuse.r = saturate(LdotN * (1.0 - sssWrap.r) + sssWrap.r);
            //finalDiffuse.g = saturate(LdotN * (1.0 - sssWrap.g) + sssWrap.g);
            //finalDiffuse.b = saturate(LdotN * (1.0 - sssWrap.b) + sssWrap.b);
            
            float3 SSS_Wrap_Add = float3(1,1,1) - (SSSColor.rgb * (sssMask * SSSIntensity * 0.99));
            float3 SSS_Wrap_Exp = (float3(1,1,1) / SSS_Wrap_Add) - float3(1,1,1);
            finalDiffuse.r = pow( max(0.0, LdotN + SSS_Wrap_Exp.r) * SSS_Wrap_Add.r, 1.0 + SSS_Wrap_Exp.r)
                           * (2.0 + SSS_Wrap_Exp.r) * (SSS_Wrap_Add.r * 0.5);
            finalDiffuse.g = pow( max(0.0, LdotN + SSS_Wrap_Exp.g) * SSS_Wrap_Add.g, 1.0 + SSS_Wrap_Exp.g)
                           * (2.0 + SSS_Wrap_Exp.g) * (SSS_Wrap_Add.g * 0.5);
            finalDiffuse.b = pow( max(0.0, LdotN + SSS_Wrap_Exp.b) * SSS_Wrap_Add.b, 1.0 + SSS_Wrap_Exp.b)
                           * (2.0 + SSS_Wrap_Exp.b) * (SSS_Wrap_Add.b * 0.5);
            
            
            finalDiffuse *= diffuseColor;
        }
        else
        {
            finalDiffuse = diffuseColor * (LdotN);
        }
        
        LdotN = saturate(LdotN);
        
        
        
        float NdotV = abs(saturate(dot(viewVec, N)));
        float NdotH = abs(saturate(dot(H, N)));
        float VdotH = saturate(dot(H, viewVec));
        
        float alpha = roughness * roughness;
        float alphasquare = alpha * alpha;
        
        float3 F = pow(1.0 - VdotH, 5) * (1.0 - F0) + F0;
        
        float denom = NdotH * NdotH * (alphasquare - 1.0) + 1.0;
        
        //[OLD D]
        //float D = alphasquare / (Pi * denom * denom);
        
        float specPower = exp2((1 - roughness) * 13.0);
        specPower = max(1.0, specPower / (specPower * 0.01 + 1.0)) * SpecularPowerMult;

        [branch]
        if (IsDS2NormalMapChannels)
        {
            specPower = max(specularMapColor_Source.r * 256, 1);
            float specPowerCalc = (specPower * specPower * 0.01);
            specPowerCalc += specPowerCalc;
            specPowerCalc = max(specPowerCalc, 1);

            roughness = 1 - clamp(log2(specPowerCalc) / 13, 0, 1);
            //return float4(float3(1,1,1) * (1 - roughness), 1);
        }

        
        float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);
        
        //float V = LdotN * sqrt(alphasquare + ((1.0 - alphasquare) * (NdotV * NdotV ))) +
        //  NdotV * sqrt(alphasquare + ((1.0 - alphasquare) * (LdotN * LdotN )));
        //V = min(0.5 / max(V, Epsilon), 1.0);
        
        float3 specular = D * F * pow(LdotN, LdotNPower);//D * F * V * LdotN;
        
        uint envWidth, envHeight, envLevels;
        
        EnvironmentMap.GetDimensions(0, envWidth, envHeight, envLevels);
        
        float envMip =  min(6.0, -(1 - roughness) * 6.5 + 6.5);//log2(alpha * float(envWidth));

        [branch]
        if (IsDS2NormalMapChannels)
        {
            envMip = min(4.0, log2(specPower) * -0.7 + 5.6);
        }

        float3 reflectVec = reflect(viewVec, N);
        
        //return float4(clamp(NdotV, 0, 1), clamp(NdotV, 0, 1), clamp(NdotV, 0, 1), 1);

        float3 ambientSpec = texCUBElod(EnvironmentMapSampler, float4(mul(float4(reflectVec, 1), FlipSkybox).xyz * float3(-1,1,-1), envMip));

        //return float4(ambientSpec, 1);

        //float3 normalCubemapTest = texCUBElod(EnvironmentMapSampler, float4(reflectVec, envMip));

		//return float4(normalCubemapTest, 1);
        //return float4(ambientSpec, 1);
        //return float4(reflectVec, 1);
		
        //ambientSpec *= ambientSpec;
        ambientSpec *= AmbientLightMult;
        
        float3 ambientDiffuse = texCUBElod(EnvironmentMapSampler, float4(mul(float4(-N, 1), FlipSkybox).xyz * float3(-1,1,-1), 5));
		
        //ambientDiffuse *= ambientDiffuse;
        ambientDiffuse *= AmbientLightMult;
        
        NdotV = max(NdotV, Epsilon);
        float K = roughness * roughness * 0.5;
        float G = (NdotV/ (NdotV* (1.0 - K) + K));
        //float iV = min(G / (4.0 * NdotV), 1.0);
        
        float3 aF = pow(1.0 - NdotV, 5) * (1 - roughness) * (1 - roughness) * (1.0 - F0) + F0;//pow(1.0 - NdotV, 5) * (1.0 - F0) + F0;
        
        float3 diffuse = finalDiffuse * (1 - F0);
        
        float3 indirectDiffuse = finalDiffuse * ambientDiffuse * (1 - F0);

        [branch]
        if (IsDS2NormalMapChannels)
        {
            indirectDiffuse = indirectDiffuse * nmapcol_full.b * input.Color.r;
        }
        
        float3 indirectSpecular = ambientSpec * aF;// * iV;
        
        [branch]
        if (IsDS2NormalMapChannels)
        {
            float AmbFresnelPower = specPower / (specPower + 30);
            float AmbFresnel = pow(1.0 - saturate(NdotV * 2), AmbFresnelPower * 4 + 1) * AmbFresnelPower;
            AmbFresnel = AmbFresnel * (1.0 - F0) + F0;

            indirectSpecular = ambientSpec * AmbFresnel * nmapcol_full.b * input.Color.r * 0.25;
        }

        float reflectionThing = saturate(dot(reflectVec, N) + 1.0);
        reflectionThing  *= reflectionThing;
        indirectSpecular *= reflectionThing;
        
        //test
        //specular *= 3.14;
        
        
        diffuse *= DirectDiffuseMult;
        specular *= DirectSpecularMult;
        indirectDiffuse *= IndirectDiffuseMult;
        indirectSpecular *= IndirectSpecularMult;        
        float3 direct = diffuse + specular;
        float3 indirect = indirectDiffuse + indirectSpecular;
        
        [branch]
        if (NewDebugType == NewDebugTypes__PBR_Direct)
        {
            return float4(direct * DirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_Indirect)
        {
            return float4(indirect * IndirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_DirectDiffuse)
        {
            return float4(diffuse * DirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_DirectSpecular)
        {
            return float4(specular * DirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_IndirectDiffuse)
        {
            return float4(indirectDiffuse * IndirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_IndirectSpecular)
        {
            return float4(indirectSpecular * IndirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_Diffuse)
        {
            return float4(((diffuse * DirectLightMult) + (indirectDiffuse * IndirectLightMult)) * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_Specular)
        {
            return float4(((specular * DirectLightMult) + (indirectSpecular * IndirectLightMult)) * SceneBrightness, color.a);
        }
        
        
        
        return float4((((direct * DirectLightMult) + (indirect * IndirectLightMult) + (emissiveMapColor * EmissiveMapMult)) * SceneBrightness), color.a);
    }
    else if (WorkflowType == WORKFLOW_CLASSIC_DIFFUSE_PTDE)
    {
        //float3 normal = input.Normal;
        
        float normalMapBlueChannel = nmapcol.z;
        
        float roughness = 1 - normalMapBlueChannel;
        
        
        
        //specularMapColor = pow(specularMapColor, 0.6);
        
        float specMapGrayscale = 0.3086 * specularMapColor_Source.x + 0.6094 * specularMapColor_Source.y + 0.0820 * specularMapColor_Source.z;
        float diffMapGrayscale = 0.3086 * color.x + 0.6094 * color.y + 0.0820 * color.z;
        float gloss = specMapGrayscale;
        float ptdeSpecPower = 1;

        [branch]
        if (PtdeMtdType == PTDE_MTD_TYPE_METAL)
        {
            //return float4(1,0,0,1);
            //float glossInverted = 1 - gloss;
            gloss = specMapGrayscale;// * diffMapGrayscale;
            gloss = (1 - pow(1 - gloss, 12));
            
            gloss = lerp(pow(diffMapGrayscale, 0.5), gloss, gloss);
            
            //gloss = clamp(pow(gloss, 0.25) / 4, 0, 1);
            
            gloss = clamp(gloss, 0, 1);
            
            specularMapColor.xyz = specularMapColor.xyz;// + color.xyz;
            
            float3 specHSL = RGBtoHSL(specularMapColor.rgb);
            specHSL.y = clamp(specHSL.y * 1.1, 0, 1);
            //specHSL.z = 0.2 + (specHSL.z * 0.6);
            //specularMapColor.rgb = HSLtoRGB(specHSL);
            
            //specularMapColor.xyz = float3(gloss, gloss, gloss) - pow(float3(gloss, gloss, gloss) - specularMapColor.xyz, 2);
            
            //color.xyz = clamp(color.xyz / (1 + (specMapGrayscale * 8)), 0, 1);
            
            float3 diffHSL = RGBtoHSL(color.rgb);
            diffHSL.z = clamp(diffHSL.z * 0.5, 0, 1);// + diffMapGrayscale * 0.45;
            //diffHSL.y /= (1 + specHSL.z * 5);
            //color.rgb = clamp(HSLtoRGB(diffHSL), 0, 1);
            
            ptdeSpecPower = 0.2;
            
            //return float4(gloss, gloss, gloss, 1);
            //return float4(specularMapColor.xyz, 1);
            //return float4(color.xyz, 1);
            
        }
        else if (PtdeMtdType == PTDE_MTD_TYPE_WET)
        {
            gloss = clamp(gloss * 0.85 + 0.15, 0, 1);
            color.xyz = color.xyz * 0.75;
            specularMapColor.xyz = specularMapColor.xyz * 1.15;
            ptdeSpecPower = 5;
        }
        else if (PtdeMtdType == PTDE_MTD_TYPE_DULL)
        {
            color.xyz = color.xyz * 0.65;
            specularMapColor.xyz = 0.1 + specularMapColor.xyz * 0.8;// * 0.75 * 0.45;
            gloss *= 0.75;
            //ptdeSpecPower = 0.85;
        }
        else 
        {
            color.xyz = color.xyz * 0.65;
            specularMapColor.xyz = 0.1 + specularMapColor.xyz * 0.8;// * 0.75 * 0.45;
            gloss = clamp(gloss * 0.90 + 0.10, 0, 1);

            //return float4(float3(1,1,1) * specularMapColor, 1);
        }
        
        float3 ogSpecHSL = RGBtoHSL(specularMapColor);
        
        roughness = 1 - gloss;

        //return float4(specularMapColor.xyz, 1);

        

        //return float4(float3(1,1,1) * gloss, 1);

        //return float4(N * 0.25 + 0.5, 1);

        //return float4((N * float3(1,1,1)) * 0.6666 + 0.3333, 1);

        float3 viewVec = -normalize(input.View);
        float3 diffuseColor = color.xyz * color.xyz;
        float3 L = -LightDirection;
        
        float3 H = normalize(L + viewVec);
        
        float3 F0 = float3(0,0,0);
        
        float3 F0_Gray = specMapGrayscale;
        F0 = specularMapColor.rgb;
        
        

        // TEST
        F0 *= F0;
        F0_Gray *= F0_Gray;
        //F0 = 1;
        ///////

        //return float4(F0, 1);
        
        float LdotN = saturate(dot(N, L));
        float NdotV = abs(saturate(dot(viewVec, N)));
        float NdotH = abs(saturate(dot(H, N)));
        float VdotH = saturate(dot(H, viewVec));
        
        float alpha = roughness * roughness;
        float alphasquare = alpha * alpha;
        
        float3 finalDiffuse = diffuseColor * (LdotN);
        
        float3 F = pow(1.0 - VdotH, 5) * (1.0 - F0) + F0;
        
        float denom = NdotH * NdotH * (alphasquare - 1.0) + 1.0;
        
        //[OLD D]
        //float D = alphasquare / (Pi * denom * denom);
        
        float specPower = exp2((1 - roughness) * 13.0);
        specPower = max(1.0, specPower / (specPower * 0.01 + 1.0)) * SpecularPowerMult * ptdeSpecPower;
        float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);
        
        //float V = LdotN * sqrt(alphasquare + ((1.0 - alphasquare) * (NdotV * NdotV ))) +
        //  NdotV * sqrt(alphasquare + ((1.0 - alphasquare) * (LdotN * LdotN )));
        //V = min(0.5 / max(V, Epsilon), 1.0);
        
        float3 specular = D * F * pow(LdotN, LdotNPower);//D * F * V * LdotN;
        
        uint envWidth, envHeight, envLevels;
        
        EnvironmentMap.GetDimensions(0, envWidth, envHeight, envLevels);
        
        float envMip =  min(6.0, -(1 - roughness) * 6.5 + 6.5);//log2(alpha * float(envWidth));

        

        float3 reflectVec = reflect(viewVec, N);
        
        //return float4(clamp(NdotV, 0, 1), clamp(NdotV, 0, 1), clamp(NdotV, 0, 1), 1);

        float3 ambientSpec = texCUBElod(EnvironmentMapSampler, float4(mul(float4(reflectVec, 1), FlipSkybox).xyz * float3(-1,1,-1), envMip));

        //return float4(ambientSpec, 1);

        //float3 normalCubemapTest = texCUBElod(EnvironmentMapSampler, float4(reflectVec, envMip));

		//return float4(normalCubemapTest, 1);
        //return float4(ambientSpec, 1);
        //return float4(reflectVec, 1);
		
        //ambientSpec *= ambientSpec;
        ambientSpec *= AmbientLightMult;
        
        float3 ambientDiffuse = texCUBElod(EnvironmentMapSampler, float4(mul(float4(-N, 1), FlipSkybox).xyz * float3(-1,1,-1), 5));
		
        //ambientDiffuse *= ambientDiffuse;
        ambientDiffuse *= AmbientLightMult;
        
        NdotV = max(NdotV, Epsilon);
        float K = roughness * roughness * 0.5;
        float G = (NdotV/ (NdotV* (1.0 - K) + K));
        //float iV = min(G / (4.0 * NdotV), 1.0);
        
        float3 aF = pow(1.0 - NdotV, 5) * (1 - roughness) * (1 - roughness) * (1.0 - F0) + F0;//pow(1.0 - NdotV, 5) * (1.0 - F0) + F0;
        
        float3 diffuse = finalDiffuse;
        
        //return float4(diffuse.xyz, 1);

        float3 indirectDiffuse = finalDiffuse * ambientDiffuse * (1 - F0_Gray);
        
        float3 indirectSpecular = ambientSpec * aF;// * iV;
        
        float reflectionThing = saturate(dot(reflectVec, N) + 1.0);
        reflectionThing  *= reflectionThing;
        indirectSpecular *= reflectionThing;
        
        diffuse *= DirectDiffuseMult;
        specular *= DirectSpecularMult * 1;
        indirectDiffuse *= IndirectDiffuseMult;
        indirectSpecular *= IndirectSpecularMult * 1;

        float3 finalSpecHSV = RGBtoHSL(specular.rgb);
        finalSpecHSV.x = ogSpecHSL.x;
        finalSpecHSV.y = ogSpecHSL.y * 1.5;
        //finalSpecHSV.z *= ogSpecHSL.z * 0.3;
        finalSpecHSV.z = pow(finalSpecHSV.z, 1);
        //finalSpecHSV.z *= 0.5;
        //specular.rgb = HSLtoRGB(finalSpecHSV);
        
        finalSpecHSV = RGBtoHSL(indirectSpecular.rgb);
        finalSpecHSV.x = ogSpecHSL.x;
        finalSpecHSV.y = ogSpecHSL.y * 1.5;
        finalSpecHSV.z *= ogSpecHSL.z;// * 0.6;
        //finalSpecHSV.z *= 0.5;
        //indirectSpecular.rgb = HSLtoRGB(finalSpecHSV);
        
        float3 direct = diffuse + specular;
        float3 indirect = indirectDiffuse + indirectSpecular;
        
        [branch]
        if (NewDebugType == NewDebugTypes__PBR_Direct)
        {
            return float4(direct * DirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_Indirect)
        {
            return float4(indirect * IndirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_DirectDiffuse)
        {
            return float4(diffuse * DirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_DirectSpecular)
        {
            return float4(specular * DirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_IndirectDiffuse)
        {
            return float4(indirectDiffuse * IndirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_IndirectSpecular)
        {
            return float4(indirectSpecular * IndirectLightMult * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_Diffuse)
        {
            return float4(((diffuse * DirectLightMult) + (indirectDiffuse * IndirectLightMult)) * SceneBrightness, color.a);
        }
        else if (NewDebugType == NewDebugTypes__PBR_Specular)
        {
            return float4(((specular * DirectLightMult) + (indirectSpecular * IndirectLightMult)) * SceneBrightness, color.a);
        }
        
        return float4((((direct * DirectLightMult) + (indirect * IndirectLightMult) + (emissiveMapColor * EmissiveMapMult)) * SceneBrightness), color.a);
    }
    else if (WorkflowType == WORKFLOW_LEGACY)
    {
        color = pow(color,Legacy_DiffusePower);
        
        color = color * color.w;

        
        
        ////Ignore the Z of the normal map to be accurate to the game.
        //nmapcol = float3(nmapcol.x, nmapcol.y, Legacy_NormalMapCustomZ);
        ////nmapcol.z =  sqrt(1.0 - min(dot(nmapcol.xy, nmapcol.xy), 1.0));
        //
        ////nmapcol.x = -nmapcol.x;
        //
        //float3 normalMap = 2.0 *(nmapcol)-1.0;
        //
        //normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
        //float4 normal = float4(normalMap,1.0);

        float3 normal = N;
        //float4 debugNormalColor = float4(
        //	(normal.x * DebugBlend_Normal_ColorScale) + DebugBlend_Normal_ColorStart,
        //	(normal.y * DebugBlend_Normal_ColorScale) + DebugBlend_Normal_ColorStart,
        //	(normal.z * DebugBlend_Normal_ColorScale) + DebugBlend_Normal_ColorStart,
        //	1
        //	);

        float4 diffuse = saturate(dot(-LightDirection,normal));
        float3 reflect = normalize(2*diffuse*normal-LightDirection);
        float4 specular = pow(saturate(dot(reflect,input.View)),Legacy_SpecularPower);

        //float4 lightmap = tex2D(LightMap1Sampler, texCoordB);
        
        float4 outputColor =  color * Legacy_AmbientColor * Legacy_AmbientIntensity + 
                color * Legacy_DiffuseIntensity * Legacy_DiffuseColor * diffuse + 
                color * float4(specularMapColor.rgb, 1) * specular;

        outputColor = float4(outputColor.xyz * Legacy_SceneBrightness, color.a);
        //outputColor = outputColor;// * 0.001;
        //outputColor += input.DebugColor;
        
        //return float4(input.DebugColor.xyz, outputColor.w);
        
        return outputColor;

        //float4 outputAndDbgNorm = lerp(outputColor, debugNormalColor, float4(DebugBlend_Normal, DebugBlend_Normal, DebugBlend_Normal, DebugBlend_Normal));

        //float4 dbgNormAsVertColor = saturate(float4(input.Normal.x, input.Normal.y, input.Normal.z, 1));

        //return lerp(outputAndDbgNorm, dbgNormAsVertColor,
        //	float4(DebugBlend_NormalAsVertexColor, DebugBlend_NormalAsVertexColor, DebugBlend_NormalAsVertexColor, DebugBlend_NormalAsVertexColor)
        //		);
    }
    
    // Default if nothing selected. Same as WorkflowType 0
    return color;
}

float4 MainPS(VertexShaderOutput input, bool isFrontFacing : SV_IsFrontFace) : COLOR
{
    float4 color = GetMainColor(input, isFrontFacing);
    color.rgb = lerp(color.rgb, HighlightColor, HighlightOpacity);
    return color;
}

technique BasicColorDrawing
{
	pass P0
	{
        AlphaBlendEnable = TRUE;
        DestBlend = INVSRCALPHA;
        SrcBlend = SRCALPHA;
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
