#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

#define NO_SKINNING

#define MAXLIGHTS 3

#define WORKFLOW_HIGHLIGHT -2

#define WORKFLOW_TEXDEBUG_DIFFUSEMAP 0
#define WORKFLOW_TEXDEBUG_SPECULARMAP 1
#define WORKFLOW_TEXDEBUG_NORMALMAP 2
#define WORKFLOW_TEXDEBUG_EMISSIVEMAP 3
#define WORKFLOW_TEXDEBUG_BLENDMASKMAP 4
#define WORKFLOW_TEXDEBUG_SHININESSMAP 5
#define WORKFLOW_TEXDEBUG_NORMALMAP_BLUE 6
#define WORKFLOW_TEXDEBUG_UVCHECK_0 7
#define WORKFLOW_TEXDEBUG_UVCHECK_1 8

#define WORKFLOW_MESHDEBUG_NORMALS 100
#define WORKFLOW_MESHDEBUG_NORMALS_MESH_ONLY 101
#define WORKFLOW_MESHDEBUG_VERTEX_COLOR_ALPHA 102
#define WORKFLOW_MESHDEBUG_VERTEX_COLOR_RGB 103

#define WORKFLOW_LEGACY 200
#define WORKFLOW_PBR_GLOSS_DS3 201
#define WORKFLOW_PBR_GLOSS_BB 202
#define WORKFLOW_CLASSIC_DIFFUSE_PTDE 203

#define PTDE_MTD_TYPE_DEFAULT 0
#define PTDE_MTD_TYPE_METAL 1
#define PTDE_MTD_TYPE_WET 2
#define PTDE_MTD_TYPE_DULL 3

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

// Main
int WorkflowType = 0;
float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 FlipSkybox;
bool IsSkybox = false;

//Highlight
float3 HighlightColor;

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

// Modern
float AmbientLightMult = 1.0;
float DirectLightMult = 1.0;
float IndirectLightMult = 1.0;
float EmissiveMapMult = 1.0;
float SceneBrightness = 1.0;

float LdotNPower = 0.1;

float SpecularPowerMult = 1;

float2 ColorMapScale;
float2 NormalMapScale;
float2 SpecularMapScale;
float2 ColorMapScale2;
float2 NormalMapScale2;
float2 SpecularMapScale2;
float2 ShininessMapScale;
float2 ShininessMapScale2;
float2 EmissiveMapScale;
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
	float2 TexCoord : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
	float3 Normal : NORMAL0;
	float3 Binormal : BINORMAL0;
	float4 Bitangent : TANGENT0;
    float4 Color : COLOR0;
	float4 BoneIndices : BLENDINDICES;
    float4 BoneWeights : BLENDWEIGHT;
    float4 BoneIndicesBank : BLENDINDICES1;
    //float4x4 InstanceWorld : TEXCOORD2;
    //float4x4 InstanceWorldInverse : TEXCOORD6;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
	float3 View : TEXCOORD2;
	float3x3 WorldToTangentSpace : TEXCOORD3;
	float3 Normal : NORMAL0;
    float4 Bitangent : TANGENT0;
	float4 Color : TEXCOORD6;
};

float4 SkinVert(VertexShaderInput input, float4 v)
{
    [branch]
    if (IsSkybox)
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
    
    return ((posA + posB + posC + posD) / (input.BoneWeights.x + input.BoneWeights.y + input.BoneWeights.z + input.BoneWeights.w));
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
	output.TexCoord2.xy = input.TexCoord2.xy;// * input.AtlasScale.xy + input.AtlasOffset.xy;

    [branch]
    if (WorkflowType == WORKFLOW_MESHDEBUG_NORMALS_MESH_ONLY)
    {
        output.Normal = normalize(mul(SkinVert(input, float4(input.Normal, 0)).xyz, World));
    }

	output.WorldToTangentSpace[0] = mul(normalize(SkinVert(input, input.Bitangent).xyz), World);
	output.WorldToTangentSpace[1] = mul(normalize(SkinVert(input, float4(input.Binormal, 0)).xyz), World);
	output.WorldToTangentSpace[2] = mul(normalize(SkinVert(input, float4(input.Normal, 0)).xyz), World);
	
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

float4 MainPS(VertexShaderOutput input, bool isFrontFacing : SV_IsFrontFace) : COLOR
{
	
	
    float4 blendmaskColor = tex2D(BlendmaskMapSampler, input.TexCoord / BlendmaskMapScale);

    float texBlendVal = (input.Color.a * EnableBlendTextures);

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

	float4 color = lerp(tex2D(ColorMapSampler, input.TexCoord / ColorMapScale), tex2D(ColorMap2Sampler, input.TexCoord2 / ColorMapScale2), texBlendVal);
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

    float4 specularMapColor_Source = lerp(tex2D(SpecularMapSampler, input.TexCoord / SpecularMapScale), tex2D(SpecularMap2Sampler, input.TexCoord2 / SpecularMapScale2), texBlendVal);
    float4 specularMapColor = specularMapColor_Source;
    float4 emissiveMapColor = tex2D(EmissiveMapSampler, input.TexCoord / EmissiveMapScale);
    
    emissiveMapColor.rgb *= emissiveMapColor.rgb;
    
    [branch]
    if (IsDS2NormalMapChannels && IsDS2EmissiveFlow)
    {
        emissiveMapColor *= inputDiffuseMapAlpha;
    }

    float4 nmapcol_full = lerp(tex2D(NormalMapSampler, input.TexCoord / NormalMapScale), tex2D(NormalMap2Sampler, input.TexCoord2 / NormalMapScale2), texBlendVal);
    
    float3 nmapcol = nmapcol_full.rgb;

    [branch]
    if (IsDS2NormalMapChannels)
    {
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

    float4 shininessMapColor = lerp(tex2D(ShininessMapSampler, input.TexCoord / ShininessMapScale), tex2D(ShininessMap2Sampler, input.TexCoord2 / ShininessMapScale2), texBlendVal);
    
    float3 normalMap;
    normalMap.xy = (nmapcol.yx * 2.0 - 1.0) * float2(1, 1);
    normalMap.z =  sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0));
    normalMap = normalize(normalMap);

    normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
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
	else if (WorkflowType == WORKFLOW_TEXDEBUG_UVCHECK_0)
	{
		return float4(tex2D(UVCheckMapSampler, input.TexCoord).xyz, color.a);
	}
	else if (WorkflowType == WORKFLOW_TEXDEBUG_UVCHECK_1)
	{
		return float4(tex2D(UVCheckMapSampler, input.TexCoord2).xyz, color.a);
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
    else if (WorkflowType == WORKFLOW_PBR_GLOSS_DS3)
    {
        //float3 normal = input.Normal;
        
        float normalMapBlueChannel = nmapcol.z;
        
        float roughness = 1 - normalMapBlueChannel;
        
        
        //return float4(N * 0.25 + 0.5, 1);

        //return float4((N * float3(1,1,1)) * 0.6666 + 0.3333, 1);

        float3 viewVec = -normalize(input.View);
        float3 diffuseColor = color.xyz * color.xyz;
        float3 L = -LightDirection;
        
        float3 H = normalize(L + viewVec);
        
        float3 F0 = float3(0,0,0);
        
        F0 = specularMapColor.rgb;
        
        // TEST
        F0 *= F0;
        //F0 = 1;
        ///////
        
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

        float3 ambientSpec = texCUBElod(EnvironmentMapSampler, float4(mul(float4(reflectVec, 1), FlipSkybox).xyz, envMip));

        //return float4(ambientSpec, 1);

        //float3 normalCubemapTest = texCUBElod(EnvironmentMapSampler, float4(reflectVec, envMip));

		//return float4(normalCubemapTest, 1);
        //return float4(ambientSpec, 1);
        //return float4(reflectVec, 1);
		
        //ambientSpec *= ambientSpec;
        ambientSpec *= AmbientLightMult;
        
        float3 ambientDiffuse = texCUBElod(EnvironmentMapSampler, float4(mul(float4(-N, 1), FlipSkybox).xyz, 5));
		
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
        
        float3 direct = diffuse + specular;
        float3 indirect = indirectDiffuse + indirectSpecular;
        
        return float4((((direct * DirectLightMult) + (indirect * IndirectLightMult) + (emissiveMapColor * EmissiveMapMult)) * SceneBrightness), color.a);
    }
    else if (WorkflowType == WORKFLOW_PBR_GLOSS_BB)
    {
        //float3 normal = input.Normal;
        
        float normalMapBlueChannel = shininessMapColor.r;
        
        
        //normalMapBlueChannel *= shininessMapColor.r * shininessMapColor.r;
        
        float roughness = 1 - normalMapBlueChannel;
        
        float3 viewVec = -normalize(input.View);
        float3 diffuseColor = color.xyz * color.xyz;
        float3 L = -LightDirection;
        
        float3 H = normalize(L + viewVec);
        
        float3 F0 = float3(0,0,0);
        
        F0 = specularMapColor.rgb;
        
        // TEST
        
        F0 *= F0;

        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG: F0
        //return float4(float3(1,1,1) * F0, 1);
        ////////////////////////////////////////////////////////////////////////////////
        
        float LdotN = saturate(dot(N, L));
        float NdotV = abs(saturate(dot(viewVec, N)));
        float NdotH = abs(saturate(dot(H, N)));
        float VdotH = saturate(dot(H, viewVec));
        
        float alpha = roughness * roughness;
        float alphasquare = alpha * alpha;
        
        float3 finalDiffuse = diffuseColor * (LdotN);
        
        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG: finalDiffuse
        //return float4(float3(1,1,1) * finalDiffuse, 1);
        ////////////////////////////////////////////////////////////////////////////////

        float3 F = pow(1.0 - VdotH, 5) * (1.0 - F0) + F0;

        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG: F
        //return float4(float3(1,1,1) * F, 1);
        ////////////////////////////////////////////////////////////////////////////////
        
        
        
        //[OLD D]
        //float denom = NdotH * NdotH * (alphasquare - 1.0) + 1.0;
        //float D = alphasquare / (Pi * denom * denom);
        
        float specPower = exp2((1 - roughness) * 13.0);
        specPower = max(1.0, specPower / (specPower * 0.01 + 1.0)) * SpecularPowerMult;
        float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);
        
        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG: D
        //return float4(float3(1,1,1) * D, 1);
        ////////////////////////////////////////////////////////////////////////////////

        float V = LdotN * sqrt(alphasquare + ((1.0 - alphasquare) * (NdotV * NdotV ))) +
          NdotV * sqrt(alphasquare + ((1.0 - alphasquare) * (LdotN * LdotN )));
        V = min(0.5 / max(V, Epsilon), 1.0);
        
        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG: V
        //return float4(float3(1,1,1) * V, 1);
        ////////////////////////////////////////////////////////////////////////////////


        float3 specular = D * F * pow(LdotN, LdotNPower);//D * F * V * LdotN;
        


        uint envWidth, envHeight, envLevels;
        
        EnvironmentMap.GetDimensions(0, envWidth, envHeight, envLevels);
        
        float envMip =  min(6.0, -(1 - roughness) * 6.5 + 6.5);//log2(alpha * float(envWidth));
        float3 reflectVec = reflect(viewVec, N);
        
        float3 ambientSpec = texCUBElod(EnvironmentMapSampler, float4(mul(float4(reflectVec, 1), FlipSkybox).xyz, envMip));
		
        //ambientSpec *= ambientSpec;
        ambientSpec *= AmbientLightMult;
        
        float3 ambientDiffuse = texCUBElod(EnvironmentMapSampler, float4(mul(float4(-N, 1), FlipSkybox).xyz, 5));
        //ambientDiffuse *= ambientDiffuse;
        ambientDiffuse *= AmbientLightMult;
        
        NdotV = max(NdotV, Epsilon);
        float K = roughness * roughness * 0.5;
        float G = (NdotV/ (NdotV* (1.0 - K) + K));
        


        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG: K
        //return float4(float3(1,1,1) * K, 1);
        ////////////////////////////////////////////////////////////////////////////////



        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG: G
        //return float4(float3(1,1,1) * G, 1);
        ////////////////////////////////////////////////////////////////////////////////
        


        float3 aF = pow(1.0 - NdotV, 5) * (1 - roughness) * (1 - roughness) * (1.0 - F0) + F0;
        
        float3 diffuse = finalDiffuse * (1 - F0);
        
        float3 indirectDiffuse = finalDiffuse * ambientDiffuse * (1 - F0);
        
        //float iV = min(G / (4.0 * NdotV), 1.0);
        float3 indirectSpecular = ambientSpec * aF;// * iV;
        
        float reflectionThing = saturate(dot(reflectVec, N) + 1.0);
        reflectionThing  *= reflectionThing;
        indirectSpecular *= reflectionThing;
        
        float3 direct = diffuse + specular;
        float3 indirect = indirectDiffuse + indirectSpecular;
        
        return float4((((direct * DirectLightMult) + (indirect * IndirectLightMult) + (emissiveMapColor * EmissiveMapMult)) * SceneBrightness), color.a);
    }
    else if (WorkflowType == WORKFLOW_CLASSIC_DIFFUSE_PTDE)
    {
        //float3 normal = input.Normal;
        
        float normalMapBlueChannel = nmapcol.z;
        
        float roughness = 1 - normalMapBlueChannel;
        
        float specMapGrayscale = 0.3086 * specularMapColor.x + 0.6094 * specularMapColor.y + 0.0820 * specularMapColor.z;
        float gloss = specMapGrayscale;
        float ptdeSpecPower = 1;

        [branch]
        if (PtdeMtdType == PTDE_MTD_TYPE_METAL)
        {
            //return float4(1,0,0,1);
            gloss = gloss * gloss;
            gloss = clamp(gloss * 1 + 0.35, 0, 1);
            color.xyz = clamp(color.xyz / (1 + (specMapGrayscale * 0.6)) - (specMapGrayscale * 0.2), 0, 1);
            specularMapColor.xyz = (specularMapColor.xyz * 1.25) + 0.1;
            ptdeSpecPower = 2;

            
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
        
        F0 = specularMapColor.rgb;
        
        float3 F0_Gray = specMapGrayscale;

        // TEST
        F0 *= F0;
        F0_Gray *= F0_Gray;
        //F0 = 1;
        ///////

        
        
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

        float3 ambientSpec = texCUBElod(EnvironmentMapSampler, float4(mul(float4(reflectVec, 1), FlipSkybox).xyz, envMip));

        //return float4(ambientSpec, 1);

        //float3 normalCubemapTest = texCUBElod(EnvironmentMapSampler, float4(reflectVec, envMip));

		//return float4(normalCubemapTest, 1);
        //return float4(ambientSpec, 1);
        //return float4(reflectVec, 1);
		
        //ambientSpec *= ambientSpec;
        ambientSpec *= AmbientLightMult;
        
        float3 ambientDiffuse = texCUBElod(EnvironmentMapSampler, float4(mul(float4(-N, 1), FlipSkybox).xyz, 5));
		
        //ambientDiffuse *= ambientDiffuse;
        ambientDiffuse *= AmbientLightMult;
        
        NdotV = max(NdotV, Epsilon);
        float K = roughness * roughness * 0.5;
        float G = (NdotV/ (NdotV* (1.0 - K) + K));
        //float iV = min(G / (4.0 * NdotV), 1.0);
        
        float3 aF = pow(1.0 - NdotV, 5) * (1 - roughness) * (1 - roughness) * (1.0 - F0) + F0;//pow(1.0 - NdotV, 5) * (1.0 - F0) + F0;
        
        float3 diffuse = finalDiffuse * (1 - F0_Gray);
        
        //return float4(diffuse.xyz, 1);

        float3 indirectDiffuse = finalDiffuse * ambientDiffuse * (1 - F0);
        
        float3 indirectSpecular = ambientSpec * aF;// * iV;
        
        float reflectionThing = saturate(dot(reflectVec, N) + 1.0);
        reflectionThing  *= reflectionThing;
        indirectSpecular *= reflectionThing;
        
        float3 direct = diffuse + specular;
        float3 indirect = indirectDiffuse + indirectSpecular;
        
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

        //float4 lightmap = tex2D(LightMap1Sampler, input.TexCoord2);
        
        float4 outputColor =  color * Legacy_AmbientColor * Legacy_AmbientIntensity + 
                color * Legacy_DiffuseIntensity * Legacy_DiffuseColor * diffuse + 
                color * tex2D(SpecularMapSampler, input.TexCoord) * specular;

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
