#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

//#define NO_SKINNING

#define MAXLIGHTS 3

//TEMP DISABLED FOR COLOR TESTING

#define WORKFLOW_ASS 0
#define WORKFLOW_GLOSS 1
#define WORKFLOW_ROUGHNESS 2
#define WORKFLOW_METALNESS 3

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

cbuffer cbSkinned2
{
    float4x4 Bones2[255];
    float4x4 Bones3[255];
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
bool IsSkybox = false;

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

// Modern
float AmbientLightMult = 1.0;
float DirectLightMult = 1.0;
float IndirectLightMult = 1.0;
float EmissiveMapMult = 1.0;
float SceneBrightness = 1.0;
bool UseSpecularMapBB = false;

// Textures
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

texture2D SpecularMapBB;
sampler2D SpecularMapBBSampler = sampler_state
{
	Texture = <SpecularMapBB>;
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
	float4 DebugColor : TEXCOORD6;
};

float4 SkinShit(VertexShaderInput input, float4 shit)
{
    [branch]
    if (IsSkybox)
    {
        return shit;
    }
    
    //TEMP DISABLED FOR COLOR TESTING
    #ifndef NO_SKINNING
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
    else if (input.BoneIndicesBank.x == 2)
    {
        posA = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else if (input.BoneIndicesBank.x == 3)
    {
        posA = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.x)]) * input.BoneWeights.x;
    }
    else
    {
        posA = shit * input.BoneWeights.x;
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
    else if (input.BoneIndicesBank.y == 3)
    {
        posB = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.y)]) * input.BoneWeights.y;
    }
    else
    {
        posB = shit * input.BoneWeights.y;
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
    else if (input.BoneIndicesBank.z == 3)
    {
        posC = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.z)]) * input.BoneWeights.z;
    }
    else
    {
        posC = shit * input.BoneWeights.z;
    }
    
    if (input.BoneIndicesBank.w == 0)
    {
        posD = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones0[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 1)
    {
        posD = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones1[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 2)
    {
        posD = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones2[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else if (input.BoneIndicesBank.w == 3)
    {
        posD = mul(shit, /*float4x4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)*/Bones3[int(input.BoneIndices.w)]) * input.BoneWeights.w;
    }
    else
    {
        posD = shit * input.BoneWeights.w;
    }
    
    return ((posA + posB + posC + posD) / (input.BoneWeights.x + input.BoneWeights.y + input.BoneWeights.z + input.BoneWeights.w));
    #else
    return shit;
    #endif
    //return shit;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
    
    float4 inPos = SkinShit(input, input.Position);

    

    //inPos.xyz = mul(inPos, skinning);

    //inPos.xyz = mul(inPos.xyz, skinning);

	//float4 worldPosition = mul(mul(inPos, transpose(input.InstanceWorld)), World);
    float4 worldPosition = mul(inPos, World);
    
    //output.PositionWS = worldPosition.xyz;
    
    float4 viewPosition = mul(worldPosition, View);
    
    
    
    output.Position = mul(viewPosition, Projection);
	output.TexCoord = input.TexCoord;
	output.TexCoord2.xy = input.TexCoord2.xy;// * input.AtlasScale.xy + input.AtlasOffset.xy;

    output.Normal = normalize(SkinShit(input, float4(input.Normal, 0)).xyz);

	output.WorldToTangentSpace[0] = mul(normalize(SkinShit(input, float4(input.Bitangent.xyz, 0)).xyz), World);
	output.WorldToTangentSpace[1] = mul(normalize(SkinShit(input, float4(input.Binormal, 0)).xyz), World);
	output.WorldToTangentSpace[2] = mul(normalize(output.Normal), World);
	
	output.View = normalize(EyePosition - worldPosition);
    output.Bitangent = input.Bitangent;
	
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

float4 MainPS(VertexShaderOutput input, bool isFrontFacing : SV_IsFrontFace) : COLOR
{
	float4 color = tex2D(ColorMapSampler, input.TexCoord);
    //color = pow(color, 1.0 / 2.2);
    
    float dissolve = B16(input.Position.xy);
    
    if ((((Opacity * color.a) * 1.05) + 0.125) < dissolve)
    {
        clip(-1);
    }
    
    [branch]
    if (IsSkybox)
    {
        return color;
    }
    
    float4 specularMapColor = tex2D(SpecularMapSampler, input.TexCoord);
    
    //specularMapColor *= tex2D(SpecularMapBBSampler, input.TexCoord);
    
    //specularMapColor *= specularMapColor;
    
    float4 emissiveMapColor = tex2D(EmissiveMapSampler, input.TexCoord);
    
    //return float4(1,2,3,4);
    
    //color = color * float4(color.w, color.w, color.w, color.w);
    
    /*
    if (color.w < AlphaTest)
    {
        clip(-1);
        return color;
        
        //DEBUG:
        //return float4(1,0,1,1);
    }
    */
    float3 nmapcol = tex2D(NormalMapSampler, input.TexCoord);
    
    [branch]
    if (WorkflowType != WORKFLOW_ASS)
    {
        //float3 normal = input.Normal;
        
        float normalMapBlueChannel = nmapcol.z;
        
        
        [branch]
        if (UseSpecularMapBB)
        {
            normalMapBlueChannel *= tex2D(SpecularMapBBSampler, input.TexCoord).r;
        }
        
        
        float roughness = normalMapBlueChannel;
        float metalness = normalMapBlueChannel;
        
        [branch]
        if (WorkflowType == WORKFLOW_GLOSS)
        {
            roughness = 1 - roughness;
        }
        
        [branch]
        if (WorkflowType == WORKFLOW_METALNESS)
        {
            metalness = normalMapBlueChannel;
        }
        
        float3 normalMap;
        normalMap.xy = nmapcol.xy * 2.0 - 1.0;
        normalMap.z =  sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0));
        normalMap = normalize(normalMap);

        normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
        float3 N = (!isFrontFacing ? normalMap : -normalMap);
        
        float3 viewVec = normalize(input.View);
        float3 diffuseColor = color.xyz;
        float3 L = -LightDirection;
        
        float3 H = normalize(L + viewVec);
        
        float3 F0 = float3(0,0,0);
        
        [branch]
        if (WorkflowType == WORKFLOW_METALNESS)
        {
            F0 = lerp(float3(0.04,0.04,0.04), diffuseColor, metalness);
            diffuseColor *= 1 - metalness;
        }
        else
        {
            F0 = specularMapColor.rgb;
        }
        
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
        specPower = max(1.0, specPower / (specPower * 0.01 + 1.0));
        float D = pow(NdotH, specPower) * (specPower * 0.125 + 0.25);
        
        //float V = LdotN * sqrt(alphasquare + ((1.0 - alphasquare) * (NdotV * NdotV ))) +
        //  NdotV * sqrt(alphasquare + ((1.0 - alphasquare) * (LdotN * LdotN )));
        //V = min(0.5 / max(V, Epsilon), 1.0);
        
        float3 specular = D * F * LdotN;//D * F * V * LdotN;
        
        uint envWidth, envHeight, envLevels;
        
        EnvironmentMap.GetDimensions(0, envWidth, envHeight, envLevels);
        
        float envMip =  min(6.0, -(1 - roughness) * 6.5 + 6.5);//log2(alpha * float(envWidth));
        float3 reflectVec = reflect(-viewVec, N);
        
        float3 ambientSpec = texCUBElod(EnvironmentMapSampler, float4(reflectVec * float3(1,1,-1), envMip));
        //ambientSpec *= ambientSpec;
        ambientSpec *= AmbientLightMult;
        
        float3 ambientDiffuse = texCUBElod(EnvironmentMapSampler, float4(N * float3(1,1,-1), 5));
        //ambientDiffuse *= ambientDiffuse;
        ambientDiffuse *= AmbientLightMult;
        
        NdotV = max(NdotV, Epsilon);
        float K = roughness * roughness * 0.5;
        float G = (NdotV/ (NdotV* (1.0 - K) + K));
        //float iV = min(G / (4.0 * NdotV), 1.0);
        
        float3 aF = pow(1.0 - NdotV, 5) * (1 - roughness) * (1 - roughness) * (1.0 - F0) + F0;//pow(1.0 - NdotV, 5) * (1.0 - F0) + F0;
        
        float3 diffuse = finalDiffuse * (1 - F0);
        
        float3 indirectDiffuse = finalDiffuse * ambientDiffuse * (1 - F0);
        
        float3 indirectSpecular = ambientSpec * aF;// * iV;
        
        float reflectionThing = saturate(dot(reflectVec, N) + 1.0);
        reflectionThing  *= reflectionThing;
        indirectSpecular *= reflectionThing;
        
        float3 direct = diffuse + specular;
        float3 indirect = indirectDiffuse + indirectSpecular;
        
        return float4((((direct * DirectLightMult) + (indirect * IndirectLightMult) + (emissiveMapColor * EmissiveMapMult)) * SceneBrightness), 1);
    }
    else
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
        float3 normalMap;
        normalMap.xy = nmapcol.xy * 2.0 - 1.0;
        normalMap.z =  sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0));
        normalMap = normalize(normalMap);

        normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
        float3 normal = (!isFrontFacing ? normalMap : -normalMap);
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

        outputColor = float4(outputColor.xyz * Legacy_SceneBrightness, Opacity);
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

