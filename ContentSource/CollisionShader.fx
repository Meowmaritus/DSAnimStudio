#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Matrix
float4x4 World;
float4x4 View;
float4x4 Projection;

// Light related
float4 AmbientColor;

float4 DiffuseColor;

float3 EyePosition;

//Default 2
float DEBUG_ValueA;

// The input for the VertexShader
struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float2 TexCoord2 : TEXCOORD1;
	float3 Normal : NORMAL0;
	float3 Binormal : BINORMAL0;
	float3 Tangent : TANGENT0;
	float4x4 InstanceWorld : TEXCOORD2;
	float2 AtlasScale : TEXCOORD6;
	float2 AtlasOffset : TEXCOORD7;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 View : TEXCOORD0;
	float3 Normal : NORMAL0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;

	float4 worldPosition = mul(mul(input.Position, transpose(input.InstanceWorld)), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
	
	output.View = normalize(float4(EyePosition,1.0) - worldPosition);

	output.Normal = input.Normal;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 diffuse = saturate(abs(dot(-input.View, input.Normal)));
	
    float4 outputColor =  AmbientColor * 0.3 + 
			DiffuseColor * diffuse * 0.7;

	outputColor = float4(outputColor.xyz, 1.0);
	return outputColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

