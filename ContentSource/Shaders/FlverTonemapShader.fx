#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

float Epsilon = 0.0000001;

float2 ScreenSize = float2(1280, 720);

float SceneContrast = 0.5;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler,input.TextureCoordinates);
    //color /= color * 0.25 + 1.0;
    //color *= 1.25 / (color + 1.0);
    
    //color *= 4;
    
    color = float4(saturate(float3(1,1,1) - exp2(max(color.rgb, Epsilon) * -1.44269502)), 1);
    
    //color = float4(pow(color.rgb, 2.2), 1);
    
    //float A = 0.30; // Shoulder strength 
    //float B = 0.50; // Linear strength
    //float C = 0.10; // Linear angle
    //float D = 0.20; // Toe strength
    //float E = 0.02; // Toe numerator
    //float F = 0.30; // Toe denominator
    //color.a = 4;
    //color = ((color * (A * color + C * B) + D * E) / (color * (A * Color + B) + D * F)) - (E / F);
    //color.rgb *= 1.0 / color.a;
    
    color.rgb *= lerp(float3(1,1,1), color.rgb * (3.0 - 2.0 * color), SceneContrast);
    
    color = sqrt(color);
    
    float noise = B16(input.TextureCoordinates.xy * ScreenSize.xy);
    float dither = (noise - 0.5) / 255.0;
    color += dither;
    
    //color = float4(noise, noise, noise, 1);
    
	return float4(color.rgb, 1);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};