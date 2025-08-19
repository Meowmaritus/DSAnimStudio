#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_5_0
	#define PS_SHADERMODEL ps_5_0
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

#define AXIS_TYPE_XY 1
#define AXIS_TYPE_XZ 2
#define AXIS_TYPE_YZ 3

Texture2D GridCellTexture;
sampler2D GridCellTextureSampler = sampler_state
{
	Texture = <GridCellTexture>;
	MinFilter = anisotropic;
    MagFilter = anisotropic;
    MipFilter = anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
    MipLODBias = 0;
    MaxAnisotropy = 256;
};

Texture2D GridCellTextureThickX;
sampler2D GridCellTextureThickXSampler = sampler_state
{
	Texture = <GridCellTextureThickX>;
	MinFilter = anisotropic;
    MagFilter = anisotropic;
    MipFilter = anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
    MipLODBias = 0;
    MaxAnisotropy = 256;
};

Texture2D GridCellTextureThickY;
sampler2D GridCellTextureThickYSampler = sampler_state
{
	Texture = <GridCellTextureThickY>;
	MinFilter = anisotropic;
    MagFilter = anisotropic;
    MipFilter = anisotropic;
    AddressU = Wrap;
    AddressV = Wrap;
    MipLODBias = 0;
    MaxAnisotropy = 256;
};

Texture2D GridOriginCrossTexture;
sampler2D GridOriginCrossTextureSampler = sampler_state
{
	Texture = <GridOriginCrossTexture>;
	MinFilter = anisotropic;
    MagFilter = anisotropic;
    MipFilter = anisotropic;
    AddressU = Clamp;
    AddressV = Clamp;
    MipLODBias = 0;
    MaxAnisotropy = 256;
};

bool WireframeOverlay = false;
float4 WireframeOverlayColor = float4(1,0,1,1);

int AxisType;

float3 Origin;
float3 OriginSnappedToUnits;
float3 CameraPosition;

float Depth = 0;

float UnitSize = 1;
float ModelSizeMult = 1;
float DistFalloffStart = 200;
float DistFalloffEnd = 300;
float DistFalloffPower = 1;
float AlphaPower = 1;
float4 LineColor = float4(0, 1, 0, 1);
float4 OriginLineColor = float4(1, 0, 0, 1);

float ThicknessTest = 0;

// The input for the VertexShader
// VertexPositionNormalTexture
struct VertexShaderInput
{
    float3 Position : POSITION0;
	float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float4 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
    float4 WorldPosition : POSITION1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
    
    float4 inPos = float4(input.Position, 1);
    float4 worldPosition = mul(inPos, World);
    
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Normal.xyz = normalize(mul(input.Normal.xyz, World));
    output.UV = input.UV;
    
    output.WorldPosition = worldPosition;
    
    output.Position += float4(0, 0, -Depth, 0);
    
    return output;
}

float4 MainPS(VertexShaderOutput input, bool isFrontFacing : SV_IsFrontFace) : COLOR
{
    //test
    //return GridCfg_LineColor[0];
    [branch]
    if (WireframeOverlay)
    {
        return WireframeOverlayColor;
    }
    
    float3 posFromOrigin = (input.WorldPosition.xyz - Origin);
    float3 uv3D = (input.WorldPosition.xyz / UnitSize);
    
    float3 view = normalize(CameraPosition - input.WorldPosition.xyz);
    float2 v = float2(0, 0);
    
    float distFalloffRatio = (length(posFromOrigin)-DistFalloffStart) / (DistFalloffEnd - DistFalloffStart);
    distFalloffRatio = pow(saturate(distFalloffRatio), DistFalloffPower);
    
    float3 uv3DOrigin = (posFromOrigin / UnitSize);
    
    float2 uv = float2(0,0);
    float2 uvOrigin = float2(0,0);
    
    [branch]
    if (AxisType == AXIS_TYPE_XY)
    {
        uv = uv3D.xy;
        uvOrigin = uv3DOrigin.xy;
        v = float2(view.x, view.y);
    }
    else if (AxisType == AXIS_TYPE_XZ)
    {
        uv = uv3D.xz;
        uvOrigin = uv3DOrigin.xz;
        v = float2(view.x, view.z);
    }
    else if (AxisType == AXIS_TYPE_YZ)
    {
        uv = uv3D.yz;
        uvOrigin = uv3DOrigin.yz;
        v = float2(view.y, view.z);
    }
    
    float howAnisotropic = 1 - saturate(dot(view, input.Normal));
    float2 thicknessBlends = float2(abs(v.x), abs(v.y)) * howAnisotropic;
    
    
    //test
    thicknessBlends.x = 0;
    thicknessBlends.y = 1;
    
    float4 srcColor = tex2D(GridCellTextureSampler, uv);
    
    //srcColor = lerp(srcColor, tex2D(GridCellTextureThickXSampler, uv), thicknessBlends.x);
    //srcColor = lerp(srcColor, tex2D(GridCellTextureThickYSampler, uv), thicknessBlends.y);
    
    float4 originColor = tex2D(GridOriginCrossTextureSampler, (uv/4) + float2(UnitSize / 2, UnitSize / 2));
    
    //test 
    //return float4(float3(1,1,1)*srcColor.r,1);
    
    float4 srcAlpha = float4(1,1,1,saturate(pow(srcColor.r, AlphaPower)));
    float4 originAlpha = float4(1,1,1,saturate(pow(originColor.r, AlphaPower)));
    
    //return originAlpha;
    
    float4 outputColor = LineColor * srcAlpha * float4(1,1,1,1 - distFalloffRatio);
    float4 originOutputColor = OriginLineColor * originAlpha * float4(1,1,1,1 - distFalloffRatio);
    
    //outputColor.rgb *= (outputColor.a);
    
    float4 color = lerp(outputColor, originOutputColor, originColor.r);
    
    
    //if (color.a > 1)
    //{
    //    float alphaAdjust = (color.a - 1) / color.a;
    //    color *= alphaAdjust;
    //}
    
    return color * float4(1,1,1,1);
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
