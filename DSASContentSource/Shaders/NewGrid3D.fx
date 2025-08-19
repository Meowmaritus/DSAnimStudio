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

float4x4 InverseWorld;
float4x4 InverseView;
float4x4 InverseProjection;

float SSAA = 1;

#define AXIS_TYPE_XY 1
#define AXIS_TYPE_XZ 2
#define AXIS_TYPE_YZ 3


#define MAX_CFGS 8

bool WireframeOverlay = false;
float4 WireframeOverlayColor = float4(1,0,1,1);

int AxisType;

float3 Origin;
float3 OriginOffsetForWrap;
float3 OriginSnappedToUnits;
float3 CameraPosition;

float MainAnisotropicPower = 5;

float UVScaleFactor = 1;

int GridCfgCount = 1;

float GridCfg_UnitSize[MAX_CFGS];
int GridCfg_IsOrigin[MAX_CFGS];
float GridCfg_FadeStartDist[MAX_CFGS];
float GridCfg_FadeEndDist[MAX_CFGS];
float GridCfg_CameraFadeStartDist[MAX_CFGS];
float GridCfg_CameraFadeEndDist[MAX_CFGS];
float GridCfg_CameraFadePower[MAX_CFGS];
float4 GridCfg_LineColor[MAX_CFGS];
float GridCfg_LineThickness[MAX_CFGS];
float GridCfg_LineThicknessFade[MAX_CFGS];
float GridCfg_AnisoDistFadePower[MAX_CFGS];
float GridCfg_LineThicknessIncreaseFromCameraDist[MAX_CFGS];
float GridCfg_LineThicknessFadeIncreaseFromCameraDist[MAX_CFGS];
float GridCfg_LineThicknessIncreaseFromAnisotropic[MAX_CFGS];
float GridCfg_LineThicknessFadeIncreaseFromAnisotropic[MAX_CFGS];
float GridCfg_LineThicknessFadePower[MAX_CFGS];

float Depth = 0;

// The input for the VertexShader
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
	float4 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
};

// The output from the vertex shader, used for later processing
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
	float4 Normal : NORMAL0;
    float4 WorldPosition : POSITION1;
    float2 UV : TEXCOORD0;
};

float4 SampleGridAtCoord_SpecificCfg(float3 worldPos, float3 view, float2 p, float2 v, float howAnisotropic, float2 surfaceSizeOnscreen, float surfaceSizeOnscreenOverall, bool isFrontFacing, int i)
{
    

    float3 posFromOrigin = (worldPos - (Origin+OriginOffsetForWrap));
    
    
    
    v = abs(v);
    
    float2 uncappedP = p;
    
    float unitSize = GridCfg_UnitSize[i];
    
    
    p = float2(p.x % unitSize, p.y % unitSize);
    
    float2 effectiveLineThicknessFade = float2(GridCfg_LineThicknessFade[i], GridCfg_LineThicknessFade[i]);
    float2 effectiveLineThickness = float2(GridCfg_LineThickness[i], GridCfg_LineThickness[i]);
    
    p = float2(abs(p.x), abs(p.y));
    p -= float2(unitSize / 2, unitSize / 2);
    p = abs(p);
    
    float powerMeme = 1;
    
    float distFromCamera = clamp(length(worldPos - CameraPosition) - 0, 0, 100000);
    
    float distFromCamera_YOnly = clamp(abs(worldPos.y - CameraPosition.y), 0, 100000);
    //distFromCamera = pow(distFromCamera, 1);
    
  
    
    //distFromCamera /= (GridCfg_FadeStartDist[i]/2);
    //if (distFromCamera < 1)
    //{
    //    distFromCamera = pow(distFromCamera, 2);
    //}
    //distFromCamera *= (GridCfg_FadeStartDist[i]/2);
    
    
    effectiveLineThickness += pow(effectiveLineThickness *  float2(   1/surfaceSizeOnscreen.x,1/surfaceSizeOnscreen.y    ) * GridCfg_LineThicknessIncreaseFromCameraDist[i], powerMeme);
    
    //effectiveLineThickness.x = clamp(effectiveLineThickness.x, 0.01, 1);
    //effectiveLineThickness.y = clamp(effectiveLineThickness.y, 0.01, 1);
    
    //effectiveLineThicknessFade += pow(effectiveLineThicknessFade * clamp(distFromCamera,1,GridCfg_FadeStartDist[i]) * GridCfg_LineThicknessFadeIncreaseFromCameraDist[i], clamp(powerMeme * 1 * (1+howAnisotropic), 1, 10000));
    
    effectiveLineThicknessFade += effectiveLineThicknessFade * float2(   1/surfaceSizeOnscreen.x,1/surfaceSizeOnscreen.y    ) * GridCfg_LineThicknessFadeIncreaseFromCameraDist[i];
    
    
    
    view = isFrontFacing ? view : -view;
    
    
    
    float howAnisotropic_ForDist = pow(howAnisotropic, MainAnisotropicPower);
    
    //return float4(float3(1,1,1)*howAnisotropic_ForDist,1);
    
    
    
    effectiveLineThickness += v * pow(effectiveLineThickness * GridCfg_LineThicknessIncreaseFromAnisotropic[i] * howAnisotropic_ForDist, powerMeme);
    effectiveLineThicknessFade += v * pow(effectiveLineThicknessFade * GridCfg_LineThicknessFadeIncreaseFromAnisotropic[i] * howAnisotropic_ForDist, powerMeme);
    
    float2 fadeEnd = float2((unitSize / 2) - effectiveLineThickness.x, (unitSize / 2) - effectiveLineThickness.y);
    float2 fadeStart = fadeEnd - effectiveLineThicknessFade;
    
    float lineBaseAlphaX = (p.x - fadeStart.x);
    float lineBaseAlphaY = (p.y - fadeStart.y);
    
    //return float4(float3(1,1,1)*saturate(lineBaseAlphaX),1);
    
    //lineBaseAlphaX /= effectiveLineThicknessFade.x;
    //lineBaseAlphaY /= effectiveLineThicknessFade.y;
    
    lineBaseAlphaX = pow(saturate(lineBaseAlphaX / effectiveLineThicknessFade.x), GridCfg_LineThicknessFadePower[i]);
    lineBaseAlphaY = pow(saturate(lineBaseAlphaY / effectiveLineThicknessFade.y), GridCfg_LineThicknessFadePower[i]);
    
    
    
    //lineBaseAlphaX = pow(abs(lineBaseAlphaX / effectiveLineThicknessFade.x), GridCfg_LineThicknessFadePower[i]);
    //lineBaseAlphaY = pow(abs(lineBaseAlphaY / effectiveLineThicknessFade.y), GridCfg_LineThicknessFadePower[i]);
    
    [branch]
    if (GridCfg_IsOrigin[i] > 0)
    {
        lineBaseAlphaX = 1-saturate((abs(uncappedP.x) - effectiveLineThickness.x) / effectiveLineThicknessFade.x);
        lineBaseAlphaY = 1-saturate((abs(uncappedP.y) - effectiveLineThickness.y) / effectiveLineThicknessFade.y);
    }
    
    float lineBaseAlpha = 1 - ((1 - saturate(lineBaseAlphaX)) * (1 - saturate(lineBaseAlphaY)));
    
    lineBaseAlpha = saturate(pow(lineBaseAlpha, 1));
    
    float dist = length(posFromOrigin);
    float distanceAlpha = 1 - saturate((dist - GridCfg_FadeStartDist[i]) / (GridCfg_FadeEndDist[i] - GridCfg_FadeStartDist[i]));
    
    //distanceAlpha *= (howAnisotropic_ForDist);
    
    /*
    float depthDist = (1-(input.Position.z / input.Position.w));
    depthDist = saturate(pow(depthDist, GridCfg_AnisoDistFadePower[i]));
    float anisoDist = depthDist * howAnisotropic_ForDist;
    
    //lineBaseAlpha = lerp(lineBaseAlpha, 1, anisoDist);
    
    float anisoDistFalloff = (1 - anisoDist*anisoDist);
    
    anisoDistFalloff = (anisoDistFalloff - 0.6) / (1 - 0.6);
    
    distanceAlpha *= anisoDistFalloff;
    */
    distanceAlpha = pow(saturate(distanceAlpha),1);
    
    //return float4(float3(1,1,1)*anisoDist,1);
    
    float finalLineBaseAlpha = saturate(lineBaseAlpha);
    
    float finalAlpha = saturate(GridCfg_LineColor[i].a * finalLineBaseAlpha * saturate(distanceAlpha));
    
    
    //return float4(float3(1,1,1)*finalLineBaseAlpha,1);
    
    
    
    //float gridPixelSizeRatio = (log2(1/distFromCamera))*(1-howAnisotropic);
    
    //return float4(float3(1,1,1)*gridPixelSizeRatio*200,1);
    
    //float memeDistFromCamera = 1/surfaceSizeOnscreen;
    
    
    
    // float surfaceSizeOnscreen_Sum = surfaceSizeOnscreen.y;
    
    // surfaceSizeOnscreen_Sum = 1 / surfaceSizeOnscreen_Sum;
    
    float memeDist = 1/surfaceSizeOnscreenOverall;
    
    memeDist = (memeDist - GridCfg_CameraFadeStartDist[i]) / (GridCfg_CameraFadeEndDist[i] - GridCfg_CameraFadeStartDist[i]);
    
    memeDist = saturate(memeDist);
    
    //memeDist = 1/memeDist;
    
    memeDist = pow(memeDist, GridCfg_CameraFadePower[i]);
    
    float fadeIntoNothingAlpha = 1;// - (memeDist*0.5);
    
    //float fadeIntoNothingAlpha = 1;
    
    
    return float4(GridCfg_LineColor[i].rgb, finalAlpha * fadeIntoNothingAlpha);
}

float3 UnprojectPoint(float4 p) 
{
    //p = float4(p.x/500,p.y/500,p.z,p.w);
    
    p = mul(InverseProjection, mul(InverseView, mul(InverseWorld, p)));
    return p.xyz / p.w;
}



float4 SampleGridAtCoord(float3 worldPos, float3 view, float2 p, float2 v, float howAnisotropic, float2 surfaceSizeOnscreen, float surfaceSizeOnscreenOverall, bool isFrontFacing)
{
    float4 outputColor = float4(0,0,0,0);
    
    for (int i = 0; i < GridCfgCount; i++)
    {
        float4 grid = SampleGridAtCoord_SpecificCfg(worldPos, view, p, v, howAnisotropic, surfaceSizeOnscreen, surfaceSizeOnscreenOverall, isFrontFacing, i);
        outputColor.rgb = lerp(outputColor.rgb, grid.rgb, grid.a);
        outputColor.a += grid.a;
    }
    
    
    return outputColor;
}


float4 TakeGridSample(float3 worldPos, float3 normal, bool isFrontFacing)
{
     
    float3 posFromOrigin = (worldPos.xyz - (Origin+OriginOffsetForWrap));
    
    float3 posFromOriginSnappedToUnits = (worldPos.xyz - (OriginSnappedToUnits+OriginOffsetForWrap));
    float3 view = normalize(CameraPosition - worldPos.xyz);
    
   
    float3 wtf = float3(length(float2(ddx(worldPos.x),ddy(worldPos.x))), length(float2(ddx(worldPos.y),ddy(worldPos.y))), length(float2(ddx(worldPos.z),ddy(worldPos.z))));
    
    float3 surfaceSizeOnscreen3D = 1/wtf;
    float so = 1/length(wtf);
    
    //return float4(testMeme,1);
    
    //return float4(float3(1,1,1)*surfaceSizeOnscreenY,1);
    
    //return float4(testVal,0,0,1);
    
    //float2 surfaceSizeOnscreen = float2(surfaceSizeOnscreenX, surfaceSizeOnscreenY);
    
    
                           
    
    
    //return float4((posFromOrigin*0.1)+float3(0.5,0.5,0.5),1);
    //test
    //view = float3(0,1,0);
    
    view = isFrontFacing ? view : -view;
    
    //view = -view;
    //return float4((CameraPosition.x * 0.5) + 0.5, (CameraPosition.y * 0.5) + 0.5, (CameraPosition.z * 0.5) + 0.5, 1);
    //return float4(0.5 + (input.Normal.x * 0.5), 0.5 + (input.Normal.y * 0.5), 0.5 + (input.Normal.z * 0.5), 1);
    
    float2 p = float2(0, 0);
    float2 v = float2(0, 0);
    float2 s = float2(0, 0);
    [branch]
    if (AxisType == AXIS_TYPE_XZ)
    {
        p = posFromOriginSnappedToUnits.xz;
        v = view.xz;
        s = surfaceSizeOnscreen3D.xz;
    }
    else if (AxisType == AXIS_TYPE_XY)
    {
        p = posFromOriginSnappedToUnits.xy;
        v = view.xy;
        s = surfaceSizeOnscreen3D.xy;
    }
    else if (AxisType == AXIS_TYPE_YZ)
    {
        p = posFromOriginSnappedToUnits.yz;
        v = view.yz;
        s = surfaceSizeOnscreen3D.yz;
    }
    
    
    
    //test
    //p = input.UV * UVScaleFactor;
    
    
    

    
    float howAnisotropic = 1 - saturate(dot(view, normal));
    
    //return float4(float3((surfaceSizeOnscreen.x*1)+0,0,(surfaceSizeOnscreen.y*1)+0),1);
    
    //howAnisotropic *= pow(saturate((length(posFromOrigin) - 1) / 1), 1);
    
    
    return SampleGridAtCoord(worldPos, view, p, v, howAnisotropic, s, so, isFrontFacing);
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
   
   
    float ssaaInv = 1/SSAA;
    float4 gridSample = float4(0,0,0,0);
    [branch]
    for (int x = 0; x < SSAA; x++)
    {
        [branch]
        for (int y = 0; y < SSAA; y++)
        {
            float3 worldPos = input.WorldPosition;
            worldPos += (ddx_fine(worldPos)*(x*ssaaInv))+(ddy_fine(worldPos)*(y*ssaaInv));
            gridSample += TakeGridSample(worldPos, input.Normal, isFrontFacing);
        }
    }
    gridSample *= (ssaaInv*ssaaInv);
    return gridSample;
}


VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output;
    
    float4 inPos = input.Position;
    float4 worldPosition = mul(inPos, World);
    
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Normal.xyz = normalize(mul(input.Normal.xyz, World));
    output.Color = input.Color;
    
    output.WorldPosition = worldPosition;
    
    output.Position += float4(0, 0, -Depth, 0);
    
    output.UV = input.UV;
    
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
