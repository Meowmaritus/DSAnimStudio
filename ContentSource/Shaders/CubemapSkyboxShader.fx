#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 EyePosition;

float AmbientLightMult;
float SceneBrightness;

textureCUBE EnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
	Texture = <EnvironmentMap>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float3 View : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.View = normalize(EyePosition - worldPosition);
    
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float3 viewVec = normalize(input.View);
    
    float3 mipA = texCUBElod(EnvironmentMapSampler, float4(viewVec, 0));
    float3 mipB = texCUBElod(EnvironmentMapSampler, float4(viewVec, 1));
    float3 mipC = texCUBElod(EnvironmentMapSampler, float4(viewVec, 2));
    float3 mipD = texCUBElod(EnvironmentMapSampler, float4(viewVec, 3));
    
    mipA *= mipA;
    mipB *= mipB;
    mipC *= mipC;
    mipD *= mipD;
    
    return float4(((mipA + mipB + mipC + mipD) / 4) * AmbientLightMult * SceneBrightness, 1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};