#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

cbuffer cbSkinned
{
    // This is really bad I'm sorry.
    // There's literally a second index to say which array to choose from
    // Since some bug prevents arrays from over 255 long from actually compiling 
    // right with MonoGame Content Pipeline
    float4x4 Bones0[255];
    float4x4 Bones1[255];
    //float4x4 Bones2[32];
};

cbuffer cbSkinned2
{
    float4x4 Bones2[255];
};

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Light related
float4 AmbientColor;
float AmbientIntensity;

float3 LightDirection;
float4 DiffuseColor;
float DiffuseIntensity;

float4 SpecularColor;
float SpecularPower;
float3 EyePosition;
//
//float DebugBlend_Normal;
//float DebugBlend_Normal_ColorStart;
//float DebugBlend_Normal_ColorScale;
//
//float DebugBlend_NormalAsVertexColor;

float AlphaTest;
float DiffusePower;

//Default 2
float DEBUG_ValueA;

float NormalMapCustomZ;

texture2D ColorMap;
sampler2D ColorMapSampler = sampler_state
{
	Texture = <ColorMap>;
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

texture2D SpecularMap;
sampler2D SpecularMapSampler = sampler_state
{
	Texture = <SpecularMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};
/*
texture2D LightMap1;
sampler2D LightMap1Sampler = sampler_state
{
	Texture = <LightMap1>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

texture2D LightMap2;
sampler2D LightMap2Sampler = sampler_state
{
	Texture = <LightMap2>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};
*/
// The input for the VertexShader
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
	float3 Normal : NORMAL0;
	float3 Binormal : BINORMAL0;
	float3 Tangent : TANGENT0;
    float4 Color : COLOR0;
	float4 BoneIndices : BLENDINDICES;
    float4 BoneWeights : BLENDWEIGHT;
    float4 BoneIndicesBank : BLENDINDICES1;
    float4x4 InstanceWorld : TEXCOORD2;
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
	float4 DebugColor : TEXCOORD6;
};

float4 SkinShit(VertexShaderInput input, float4 shit)
{
    float4 posA;
    float4 posB;
    float4 posC;
    float4 posD;
    
    if (input.BoneIndicesBank.x == 0)
    {
        posA = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else if (input.BoneIndicesBank.x == 1)
    {
        posA = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else
    {
        posA = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    
    if (input.BoneIndicesBank.y == 0)
    {
        posB = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else if (input.BoneIndicesBank.y == 1)
    {
        posB = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else if (input.BoneIndicesBank.y == 2)
    {
        posB = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    
    if (input.BoneIndicesBank.z == 0)
    {
        posC = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else if (input.BoneIndicesBank.z == 1)
    {
        posC = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else if (input.BoneIndicesBank.z == 2)
    {
        posC = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    
    if (input.BoneIndicesBank.w == 0)
    {
        posD = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 1)
    {
        posD = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else
    {
        posD = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    
    
    //posB = mul(inPos, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones[int(input.BoneIndices[1])]) * input.BoneWeights[1];
    //posC = mul(inPos, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones[int(input.BoneIndices[2])]) * input.BoneWeights[2];
    //posD = mul(inPos, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones[int(input.BoneIndices[3])]) * input.BoneWeights[3];
    
    
    return ((posA + posB + posC + posD) / (input.BoneWeights.x + input.BoneWeights.y + input.BoneWeights.z + input.BoneWeights.w));
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
    
    float4 inPos = SkinShit(input, input.Position);

    //inPos.xyz = mul(inPos, skinning);

    //inPos.xyz = mul(inPos.xyz, skinning);

	float4 worldPosition = mul(mul(inPos, transpose(input.InstanceWorld)), World);
    
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.TexCoord2.xy = input.TexCoord2.xy;// * input.AtlasScale.xy + input.AtlasOffset.xy;

    output.Normal = normalize(SkinShit(input, float4(input.Normal, 0)).xyz);

	output.WorldToTangentSpace[0] = mul(normalize(SkinShit(input, float4(input.Tangent, 0)).xyz), World);
	output.WorldToTangentSpace[1] = mul(normalize(SkinShit(input, float4(input.Binormal, 0)).xyz), World);
	output.WorldToTangentSpace[2] = mul(normalize(output.Normal), World);
	
	output.View = normalize(float4(EyePosition,1.0) - worldPosition);

	
	//output.Normal = mul((float3x3)skinning, output.Normal);
	//output.DebugColor.xy = input.AtlasScale.xy;
	//output.DebugColor.zw = input.AtlasOffset.xy;
    
    //output.Position.xy
    
    //output.WorldToTangentSpace = mul(output.WorldToTangentSpace, skinning);
    
    //output.DebugColor = input.BoneIndices;
    
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(ColorMapSampler, input.TexCoord);
    
    color = color * float4(color.w, color.w, color.w, color.w);
    
    if (color.w < AlphaTest)
    {
        clip(-1);
        return color;
        
        //DEBUG:
        //return float4(1,0,1,1);
    }
    
    color = pow(color,DiffusePower);
    
	color = color * color.w;

    float3 nmapcol = tex2D(NormalMapSampler, input.TexCoord);
    
	//Ignore the Z of the normal map to be accurate to the game.
	nmapcol = float3(nmapcol.x, nmapcol.y, NormalMapCustomZ);

    //nmapcol.x = -nmapcol.x;
    
	float3 normalMap = 2.0 *(nmapcol)-1.0;

	normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
	float4 normal = float4(normalMap,1.0);

	//float4 debugNormalColor = float4(
	//	(normal.x * DebugBlend_Normal_ColorScale) + DebugBlend_Normal_ColorStart,
	//	(normal.y * DebugBlend_Normal_ColorScale) + DebugBlend_Normal_ColorStart,
	//	(normal.z * DebugBlend_Normal_ColorScale) + DebugBlend_Normal_ColorStart,
	//	1
	//	);

	float4 diffuse = saturate(dot(-LightDirection,normal));
	float4 reflect = normalize(2*diffuse*normal-float4(LightDirection,1.0));
	float4 specular = pow(saturate(dot(reflect,input.View)),SpecularPower);

	//float4 lightmap = tex2D(LightMap1Sampler, input.TexCoord2);
	
    float4 outputColor =  color * AmbientColor * AmbientIntensity + 
			color * DiffuseIntensity * DiffuseColor * diffuse + 
			color * tex2D(SpecularMapSampler, input.TexCoord) * specular;

	outputColor = float4(outputColor.xyz * color.w, color.w);
	outputColor = outputColor;// * 0.001;
	//outputColor += input.DebugColor;
    
    //return float4(input.DebugColor.xyz, outputColor.w);
    
	return outputColor;

	//float4 outputAndDbgNorm = lerp(outputColor, debugNormalColor, float4(DebugBlend_Normal, DebugBlend_Normal, DebugBlend_Normal, DebugBlend_Normal));

	//float4 dbgNormAsVertColor = saturate(float4(input.Normal.x, input.Normal.y, input.Normal.z, 1));

	//return lerp(outputAndDbgNorm, dbgNormAsVertColor,
	//	float4(DebugBlend_NormalAsVertexColor, DebugBlend_NormalAsVertexColor, DebugBlend_NormalAsVertexColor, DebugBlend_NormalAsVertexColor)
	//		);
    
    
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

