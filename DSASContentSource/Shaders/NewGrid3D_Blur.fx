#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

texture2D BufferTex;
sampler2D BufferTexSampler = sampler_state
{
	Texture = <BufferTex>;
	MinFilter = linear;
	MagFilter = linear;
	MipFilter = linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4x4 World;
float4x4 View;
float4x4 Projection;

float Depth = 0;

float BlurAnisoPower = 32;
float BlurDirections = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
float BlurQuality = 3.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
float BlurSize = 8.0; // BLUR SIZE (Radius)

float3 CameraPosition;

float2 BufferTexSize = float2(1,1);



// The input for the VertexShader
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
	float4 Normal : NORMAL0;
    float4 WorldPosition : POSITION1;
};

float4 GetBlurredPixel(float2 centerPixel)
{
    float Pi = 6.28318530718; // Pi*2
    
    // GAUSSIAN BLUR SETTINGS {{{
    
    // GAUSSIAN BLUR SETTINGS }}}
   
    float2 Radius = BlurSize/BufferTexSize.xy;
    
    // Normalized pixel coordinates (from 0 to 1)
    float2 uv = centerPixel/BufferTexSize.xy;
    // Pixel colour
    float4 Color = tex2D(BufferTexSampler, uv);
    
    // Blur calculations
    for( float d=0; d<Pi; d+=Pi/BlurDirections)
    {
		for(float i=1.0/BlurQuality; i<=1.0; i+=1.0/BlurQuality)
        {
			Color += tex2D(BufferTexSampler, uv+float2(cos(d+Pi/2),sin(d+Pi/2))*Radius*i);		
        }
    }
    
    // Output to screen
    Color /= BlurQuality * BlurDirections - 15.0;
    return Color;
}

float4 MainPS(VertexShaderOutput input, bool isFrontFacing : SV_IsFrontFace) : COLOR
{
    return float4(1,1,1,1);

    float4 mainPixel = tex2D(BufferTexSampler, input.Position.xy / BufferTexSize);
    float4 blurredPixel = GetBlurredPixel(input.Position.xy);
    
    float3 view = normalize(CameraPosition - input.WorldPosition.xyz);
    
    view = isFrontFacing ? view : -view;
    
    float howAnisotropic = 1 - saturate(dot(view, input.Normal));
    
    howAnisotropic *= howAnisotropic;
    howAnisotropic *= howAnisotropic;
    
    float depthDist = (1-(input.Position.z / input.Position.w));
    depthDist = saturate(pow(depthDist, BlurAnisoPower));
    float anisoDist = depthDist * howAnisotropic;
    
    return float4(1,1,1,mainPixel.a + 5);
    
    return lerp(mainPixel, blurredPixel, anisoDist);
}


VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    
    float4 inPos = input.Position;
    float4 worldPosition = mul(inPos, World);
    
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Normal.xyz = normalize(mul(input.Normal.xyz, World));
    output.Color = input.Color;
    
    output.WorldPosition = worldPosition;
    
    output.Position +=  float4(0, 0, -Depth, 0);
    
    return output;
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
