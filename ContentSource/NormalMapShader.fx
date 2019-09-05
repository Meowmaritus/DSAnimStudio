#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

#define MAXLIGHTS 3

//TEMP DISABLED FOR COLOR TESTING

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
};


static const float Pi = 3.141592;
static const float Epsilon = 0.00001;
// Constant normal incidence Fresnel factor for all dielectrics.
static const float3 Fdielectric = 0.04;

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

bool DS3IsSpecularWorkflow = true;

float AlphaTest;
float DiffusePower;

//Default 2
float DEBUG_ValueA;

float NormalMapCustomZ;

bool EnableGhettoDS3Renderer;

//Light

int DS3LightAmount = 1;

float3 DS3LightDirection[MAXLIGHTS];
float DS3LightRadiance[MAXLIGHTS];

float cols = 20.0f;
float rows = 10.0f;
/*uint*/float TiledListLength[200];

const float OUTPUTCONST = 5;//0.1f;

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
	MinFilter = point;
	MagFilter = point;
	MipFilter = point;
    AddressU = Wrap;
    AddressV = Wrap;
    AddressW = Wrap;
};

textureCUBE EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
	Texture = <EnvironmentMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
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
    float4 Bitangent : TANGENT0;
	float4 DebugColor : TEXCOORD6;
};

SamplerState defaultSampler : register(s0);

float4 SkinShit(VertexShaderInput input, float4 shit)
{
    //TEMP DISABLED FOR COLOR TESTING
    
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
    
    return ((posA + posB + posC + posD) / (input.BoneWeights.x + input.BoneWeights.y + input.BoneWeights.z + input.BoneWeights.w));
    
    //return shit;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
    
    float4 inPos = SkinShit(input, input.Position);

    

    //inPos.xyz = mul(inPos, skinning);

    //inPos.xyz = mul(inPos.xyz, skinning);

	float4 worldPosition = mul(mul(inPos, transpose(input.InstanceWorld)), World);
    
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

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(ColorMapSampler, input.TexCoord);
    
    float4 specularMapColor = tex2D(SpecularMapSampler, input.TexCoord);
    
    //return float4(1,2,3,4);
    
    //color = color * float4(color.w, color.w, color.w, color.w);
    
    if (color.w < AlphaTest)
    {
        clip(-1);
        return color;
        
        //DEBUG:
        //return float4(1,0,1,1);
    }
    
    float3 nmapcol = tex2D(NormalMapSampler, input.TexCoord);
    
    [branch]
    if (EnableGhettoDS3Renderer)
    {
        //float3 normal = input.Normal;
        
        float gloss = nmapcol.z;
        /*
        //Ignore the Z of the normal map to be accurate to the game.
        nmapcol = float3(nmapcol.x, nmapcol.y, NormalMapCustomZ);
        nmapcol.z =  sqrt(1.0 - min(dot(nmapcol.xy, nmapcol.xy), 1.0));

        //nmapcol.x = -nmapcol.x;
        
        float3 normalMap = 2.0 *(nmapcol)-1.0;

        normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
        float3 N = normalMap;
        
        */
        
        float3 normalMap;
        normalMap.xy = nmapcol.xy * 2.0 - 1.0;
        normalMap.z =  sqrt(1.0 - min(dot(normalMap.xy, normalMap.xy), 1.0));
        normalMap = normalize(normalMap);

        normalMap = normalize(mul(normalMap, input.WorldToTangentSpace));
        float3 N = normalMap;
        
        float3 viewVec = normalize(input.View);
        
        //if (!(input.Bitangent.w < 0))
        //{
        //    N = -N;
        //}
        
        //Pre-determined look of object
        float3 diffuseColor = color.xyz;
        
        
        float3 L = -DS3LightDirection[0];
        
        float3 H = normalize(L + viewVec);
        
        float3 F0 = specularMapColor.rgb;
        
        float LdotN = saturate(dot(N, L));
        float NdotV = abs(saturate(dot(viewVec, N)));
        float NdotH = abs(saturate(dot(H, N)));
        float VdotH = saturate(dot(H, viewVec));
        
        float roughness = 1 - gloss;
        float alpha = roughness * roughness;
        float alphasquare = alpha * alpha;
        
        float3 finalDiffuse = diffuseColor * (LdotN / Pi);
        
        float3 F = pow(1.0 - VdotH, 5) * (1.0 - F0) + F0;
        
        float denom = NdotH * NdotH * (alphasquare - 1.0) + 1.0;
        float D = alphasquare / (Pi * denom * denom);
        
        float V = LdotN * sqrt(alphasquare + ((1.0 - alphasquare) * (NdotV * NdotV ))) +
          NdotV * sqrt(alphasquare + ((1.0 - alphasquare) * (LdotN * LdotN )));
        V = min(0.5 / max(V, Epsilon), 1.0);
        
        float3 specular = D * F * V * LdotN;
        
        // environment map UV fatcat
        float3 envUV = reflect(viewVec, N);
        float3 ambientSpec = texCUBE(EnvironmentMapSampler, envUV);
        ambientSpec = float3(ambientSpec.b, ambientSpec.b, ambientSpec.b);
        //float3 ambientDiffuse = EnvironmentMap.SampleLevel(EnvironmentMapSampler, N, 0);
        float3 ambientDiffuse = texCUBE(EnvironmentMapSampler, N);
        ambientDiffuse = float3(0.25,0.25,0.25);//float3(ambientDiffuse.b, ambientDiffuse.g, ambientDiffuse.r);
        
        NdotV = max(NdotV, Epsilon);
        float K= roughness * roughness * 0.5;
        float G= (NdotV/ (NdotV* (1.0 - K) + K));
        float iV = min(G / (4.0 * NdotV), 1.0);
        
        //float3 aF = min(pow(1.0 - NdotV, 5), 1.0 - alpha) * (1.0 - F0) + F0;
        //float3 aF = (pow(1.0 - NdotV, 5) * gloss) * (1.0 - F0) + F0;
        float3 aF = pow(1.0 - NdotV, 5) * (1.0 - F0) + F0;
        
        float3 diffuse = finalDiffuse * (1 - F0);
        
        float3 indirectDiffuse = finalDiffuse * ambientDiffuse * (1 - F0);
        
        // PLACEHOLDER
        //float3 indirectSpecular = ambientSpec * aF;
        float3 indirectSpecular = ambientSpec * aF * iV;
        
        
        
        
        float3 direct = diffuse + specular;
        float3 indirect = indirectDiffuse + indirectSpecular;
        
        return float4((direct + indirect) * 3.75, 1);
        //return float4(VdotH, VdotH, VdotH, 1);
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        //diffuseColor = pow(abs(diffuseColor), 1.2f);
        
        
        /*
        float3 albedo = diffuseColor;
        
        float roughness = 1  - normalMapZMeme;
         
        float metalness = normalMapZMeme; //0.1f
        
        //
        
        // Outgoing light direction (vector from world-space fragment position to the "eye").
        float3 Lo = -normalize(EyePosition - input.Position);
        
        float3 N = normal;
        
        // Angle between surface normal and outgoing light direction.
        float cosLo = max(0.0, dot(N, Lo));
            
        // Specular reflection vector.
        float3 Lr = 2.0 * cosLo * N - Lo;

        // Fresnel reflectance at normal incidence (for metals use albedo color).
        float3 F0 = float3(0,0,0);
        
        if (DS3IsSpecularWorkflow)
        {
            float3 F0 = specularMapColor.rgb;
        }
        else
        {
            albedo *= (1 - metalness);
            float3 F0 = lerp(Fdielectric, albedo, metalness);
        }
        

        // Direct lighting calculation for analytical lights.
        float3 directLighting = 0.0;
        for(uint i=0; i<MAXLIGHTS; ++i)
        {
            float3 Li = -DS3LightDirection[i];
            float3 Lradiance = DS3LightRadiance[i];

            // Half-vector between Li and Lo.
            float3 Lh = normalize(Li + Lo);

            // Calculate angles between surface normal and various light vectors.
            float cosLi = max(0.0, dot(N, Li));
            float cosLh = max(0.0, dot(N, Lh));

            float alpha = roughness * roughness;
            float alphasquare = alpha * alpha;

            // Calculate Fresnel term for direct lighting. 
            float3 F  = fresnelSchlick(F0, max(0.0, dot(Lh, Lo)));
            // Calculate normal distribution for specular BRDF.
            
            //float D = ndfGGX(cosLh, roughness);
            float D = ndfGGX(cosLh, alphasquare);
            
            
            
            // Calculate geometric attenuation for specular BRDF.
            float V = vHCSmith(cosLi, cosLo, alphasquare);

            // Diffuse scattering happens due to light being refracted multiple times by a dielectric medium.
            // Metals on the other hand either reflect or absorb energy, so diffuse contribution is always zero.
            // To be energy conserving we must scale diffuse BRDF contribution based on Fresnel factor & metalness.
            float3 kd = lerp(float3(1, 1, 1) - F, float3(0, 0, 0), metalness);

            // Lambert diffuse BRDF.
            // We don't scale by 1/PI for lighting & material units to be more convenient.
            // See: https://seblagarde.wordpress.com/2012/01/08/pi-or-not-to-pi-in-game-lighting-equation/
            float3 diffuseBRDF = kd * albedo;

            // Cook-Torrance specular microfacet BRDF.
            float3 specularBRDF = (F * D * V);//(F * D * G) / max(Epsilon, 4.0 * cosLi * cosLo);

            // Total contribution for this light.
            directLighting += (diffuseBRDF + specularBRDF) * Lradiance * cosLi;
        }
        
        float3 ambientLighting;
        {
            // Sample diffuse irradiance at normal direction.
            float3 irradiance = specularMapColor.xyz;//irradianceTexture.Sample(defaultSampler, N).rgb;

            // Calculate Fresnel term for ambient lighting.
            // Since we use pre-filtered cubemap(s) and irradiance is coming from many directions
            // use cosLo instead of angle with light's half-vector (cosLh above).
            // See: https://seblagarde.wordpress.com/2011/08/17/hello-world/
            float3 F = fresnelSchlick(F0, cosLo);

            // Get diffuse contribution factor (as with direct lighting).
            float3 kd = lerp(1.0 - F, 0.0, metalness);

            // Irradiance map contains exitant radiance assuming Lambertian BRDF, no need to scale by 1/PI here either.
            float3 diffuseIBL = kd * albedo * irradiance;

            // Sample pre-filtered specular reflection environment at correct mipmap level.
            uint specularTextureLevels = querySpecularTextureLevels();
            float3 specularIrradiance = SpecularMap.SampleLevel(defaultSampler, Lr, roughness * specularTextureLevels).rgb;

            

            // Split-sum approximation factors for Cook-Torrance specular BRDF.
            //float2 specularBRDF = GetBRDF(cosLo, roughness).xy;//specularBRDF_LUT.Sample(spBRDF_Sampler, float2(cosLo, roughness)).rg;

            // Total specular IBL contribution.
            //float3 specularIBL = (F0 * specularBRDF.x + specularBRDF.y) * specularIrradiance;
            
            float3 specularIBL = specularIrradiance * F;

            // Total ambient lighting contribution.
            ambientLighting = diffuseIBL + specularIBL;
            
            
            
            //ambientLighting = diffuseIBL + specularIrradiance;
        }
        
        // Final fragment color.
	    return float4(directLighting, 1.0);
        */
    }
    else
    {
        color = pow(color,DiffusePower);
        
        color = color * color.w;

        
        
        //Ignore the Z of the normal map to be accurate to the game.
        nmapcol = float3(nmapcol.x, nmapcol.y, NormalMapCustomZ);
        //nmapcol.z =  sqrt(1.0 - min(dot(nmapcol.xy, nmapcol.xy), 1.0));
        
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
    
    
    
    
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

