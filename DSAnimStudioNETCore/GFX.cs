using DSAnimStudio.DbgMenus;
using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.FancyShaders;
using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public enum GFXDrawStep : byte
    {
        DbgPrimPrepass = 1,
        Opaque = 2,
        AlphaEdge = 3,
        DbgPrimOverlay,
        GUI = 5,
        DbgPrimAlwaysRespectDepth = 6,
        DbgPrimAlwaysDisrespectDepth = 7,
    }

    public enum LODMode : int
    {
        Automatic = -1,
        ForceFullRes = 0,
        ForceLOD1 = 1,
        ForceLOD2 = 2,
    }

    public static class GFX
    {
        public static Viewport LastViewport = new Viewport(0, 0, 100, 100);

        public static SurfaceFormat BackBufferFormat => SurfaceFormat.Rgba1010102;

        public static class Display
        {
            public static void SetFromDisplayMode(DisplayMode mode)
            {
                Width = mode.Width;
                Height = mode.Height;
                Format = mode.Format;
            }
            public static int Width = 1600;
            public static int Height = 900;
            public static SurfaceFormat Format = SurfaceFormat.Color;
            public static bool Vsync = true;
            public static bool Fullscreen = false;
            public static void Apply()
            {
                Main.ApplyPresentationParameters((int)Math.Round(Width * Main.DPIX), (int)Math.Round(Height * Main.DPIY), Format, Vsync, Fullscreen);
            }
        }

        public static void ClampAntialiasingOptions()
        {
            if (SSAA > 4)
                SSAA = 4;

            if (MSAA > 32)
                MSAA = 32;

            if (SSAA < 1)
                SSAA = 1;

            if (MSAA < 1)
                MSAA = 1;
        }

        public static int SSAA = 1;

        public static int EffectiveSSAA = 1;

        public static int MSAA = 2;

        public static int EffectiveMSAA = 2;

        public static void InitShaders()
        {
            FlverShader.Effect.Legacy_AmbientColor = new Vector4(1, 1, 1, 1);
            FlverShader.Effect.Legacy_AmbientIntensity = 0.25f;
            FlverShader.Effect.Legacy_DiffuseColor = new Vector4(1, 1, 1, 1);
            FlverShader.Effect.Legacy_DiffuseIntensity = 0.85f;
            FlverShader.Effect.Legacy_SpecularColor = new Vector4(1, 1, 1, 1);
            FlverShader.Effect.Legacy_SpecularPower = 8;
            FlverShader.Effect.Legacy_NormalMapCustomZ = 1.0f;
            FlverShader.Effect.Legacy_DiffusePower = 1 / 1.5f;
            FlverShader.Effect.AlphaTest = 0.1f;

            FlverShader.Effect.LightDirection = Vector3.Forward;

            FlverShader.Effect.EmissiveMapMult = 1;

            FlverShader.Effect.EnvironmentMap = Environment.CurrentCubemap;
            SkyboxShader.Effect.EnvironmentMap = Environment.CurrentCubemap;

            //SkyboxShader.Effect.NumMotionBlurSamples = 0;

            //GFX.FlverOpacity = 0.15f;
        }

        public static readonly ImguiOSD.ImGuiEnumListBoxPicker<FlverShadingModes> FlverShadingModes_Picker = new ImguiOSD.ImGuiEnumListBoxPicker<FlverShadingModes>(new Dictionary<FlverShadingModes, string>
        {
            { FlverShadingModes.DEFAULT, "Default" },

            { FlverShadingModes.PBR_GLOSS, "PBR (DS2/DS3/SDT/ER/DS1R)" },
            //{ FlverShadingModes.PBR_GLOSS_BB, "Bloodborne PBR" },
            { FlverShadingModes.CLASSIC_DIFFUSE_PTDE, "Diffuse to PBR Approximation (DS1:PTDE)" },
            { FlverShadingModes.LEGACY, "Legacy Diffuse (MVDX)" },

            //{ FlverShadingModes.TEXDEBUG_DIFFUSEMAP, "TEX DEBUG: Diffuse/Albedo Map" },
            //{ FlverShadingModes.TEXDEBUG_SPECULARMAP, "TEX DEBUG: Specular/Reflectance Map" },
            //{ FlverShadingModes.TEXDEBUG_NORMALMAP, "TEX DEBUG: Normal Map" },
            //{ FlverShadingModes.TEXDEBUG_EMISSIVEMAP, "TEX DEBUG: Emissive Map" },
            //{ FlverShadingModes.TEXDEBUG_BLENDMASKMAP, "TEX DEBUG: Blend Mask Map" },
            //{ FlverShadingModes.TEXDEBUG_SHININESSMAP, "TEX DEBUG: Shininess Map" },
            //{ FlverShadingModes.TEXDEBUG_NORMALMAP_BLUE, "TEX DEBUG: Normal Map (Blue Channel)" },

            //{ FlverShadingModes.TEXDEBUG_UVCHECK_0, "TEX DEBUG: UV Check (TEXCOORD0)" },
            //{ FlverShadingModes.TEXDEBUG_UVCHECK_1, "TEX DEBUG: UV Check (TEXCOORD1)" },
            //{ FlverShadingModes.TEXDEBUG_UVCHECK_2, "TEX DEBUG: UV Check (TEXCOORD2)" },
            //{ FlverShadingModes.TEXDEBUG_UVCHECK_3, "TEX DEBUG: UV Check (TEXCOORD3)" },
            //{ FlverShadingModes.TEXDEBUG_UVCHECK_4, "TEX DEBUG: UV Check (TEXCOORD4)" },
            //{ FlverShadingModes.TEXDEBUG_UVCHECK_5, "TEX DEBUG: UV Check (TEXCOORD5)" },
            //{ FlverShadingModes.TEXDEBUG_UVCHECK_6, "TEX DEBUG: UV Check (TEXCOORD6)" },
            //{ FlverShadingModes.TEXDEBUG_UVCHECK_7, "TEX DEBUG: UV Check (TEXCOORD7)" },

            //{ FlverShadingModes.MESHDEBUG_NORMALS, "MESH DEBUG: Normals" },
            //{ FlverShadingModes.MESHDEBUG_NORMALS_MESH_ONLY, "MESH DEBUG: Normals (Mesh Only)" },
            //{ FlverShadingModes.MESHDEBUG_VERTEX_COLOR_ALPHA, "MESH DEBUG: Vertex Color Alpha" },
            //{ FlverShadingModes.MESHDEBUG_VERTEX_COLOR_RGB, "MESH DEBUG: Vertex Color RGB" },
        });

        static GFX()
        {
            DRAW_STEP_LIST = (GFXDrawStep[])Enum.GetValues(typeof(GFXDrawStep));
        }

        

        public static FlverShadingModes ForcedFlverShadingMode = FlverShadingModes.DEFAULT;

        private static List<FlverShadingModes> _flverNonDebugShadingModes = new List<FlverShadingModes>
        {
            FlverShadingModes.DEFAULT,
            FlverShadingModes.LEGACY,
            FlverShadingModes.PBR_GLOSS,
            //FlverShadingModes.PBR_GLOSS_BB,
            FlverShadingModes.CLASSIC_DIFFUSE_PTDE,
        };

        public static IReadOnlyList<FlverShadingModes> FlverNonDebugShadingModes => _flverNonDebugShadingModes;

        public static bool IsInDebugShadingMode => !((GFX.ForcedFlverShadingMode == FlverShadingModes.DEFAULT || GFX.FlverNonDebugShadingModes.Contains(GFX.ForcedFlverShadingMode)));

        public static bool FlverAutoRotateLight = false;
        private static float LightSpinTimer = 0;
        public static bool FlverLightFollowsCamera = true;

        //public static float FlverDitherTime = 0;
        //public const float FlverDitherTimeMod = 100;

        public static bool FlverEnableTextureBlending = true;
        public static bool FlverEnableTextureAlphas = true;
        public static bool FlverUseFancyAlpha = true;
        public static float FlverFancyAlphaEdgeCutoff = 0.25f;
        //public static bool FlverInvertSimpleTextureAlphas = false;

        public static bool UseTonemap = true;

        //public static float FlverOpacity = 1.0f;

        public static float LdotNPower = 0.1f;
        public static float SpecularPowerMult = 1;

        public static GFXDrawStep CurrentStep = GFXDrawStep.Opaque;

        public static readonly GFXDrawStep[] DRAW_STEP_LIST;

        public static bool HideFLVERs = false;

        public const int LOD_MAX = 2;
        public static LODMode LODMode = LODMode.Automatic;
        public static float LOD1Distance = 200;
        public static float LOD2Distance = 400;

        public static IGFXShader<FlverShader> FlverShader;
        public static IGFXShader<SkyboxShader> SkyboxShader;
        public static Vector3 SkyboxShader_PrevFrameLookDir;
        public static IGFXShader<CollisionShader> CollisionShader;
        public static IGFXShader<DbgPrimWireShader> DbgPrimWireShader;
        public static IGFXShader<DbgPrimSolidShader> DbgPrimSolidShader;
        public static Stopwatch FpsStopwatch = new Stopwatch();
        private static FrameCounter FpsCounter = new FrameCounter();

        public static FlverMaterial.Sampler NewDebug_ShowTex_Sampler = null;
        public static FlverShaderConfig.SamplerConfig NewDebug_ShowTex_SamplerConfig = null;
        public static FlverMaterial NewDebug_ShowTex_Material = null;

        public static NewDebugTypes GlobalNewDebugType = NewDebugTypes.None;

        public static IHighlightableThing HighlightedThing = null;
        public static Color HighlightColor = Color.Yellow;
        public static float HighlightOpacity = 0;
        private static float HighlightOpacityTimer = 0;
        public static float HighlightOpacityMin = 0.3f;
        public static float HighlightOpacityMax = 0.6f;
        public static float HighlightOpacityInterval = 2f;

        public static void UpdateHighlightColor(float deltaTime)
        {
            HighlightOpacityTimer += deltaTime * (1 / HighlightOpacityInterval);
            HighlightOpacityTimer %= 1;
            float s = 1 - ((MathF.Cos(MathHelper.TwoPi * HighlightOpacityTimer) / 2f) + 0.5f);
            HighlightOpacity = MathHelper.Lerp(HighlightOpacityMin, HighlightOpacityMax, s);
        }


        //public static BokehShader Bokeh;
        //public static Texture2D BokehShapeHexagon;
        //public static RenderTarget2D BokehRenderTarget;

        public static float BokehBrightness = 1;
        public static float BokehSize = 16;
        public static int BokehDownsize = 1;
        public static bool BokehIsFullPrecision = true;
        public static bool BokehIsDynamicDownsize = true;

        public static float FPS => FpsCounter.CurrentFramesPerSecond;
        public static float AverageFPS => FpsCounter.AverageFramesPerSecond;

        public static bool EnableFrustumCulling = false;
        public static bool EnableTextures = true;
        //public static bool EnableLightmapping = true;

        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOff_WireframeOff;
        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOn_WireframeOff;
        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOff_WireframeOn;
        private static RasterizerState HotSwapRasterizerState_BackfaceCullingOn_WireframeOn;

        private static DepthStencilState DepthStencilState_Normal;
        private static DepthStencilState DepthStencilState_DontWriteDepth;

        public static WorldView CurrentWorldView
        {
            get => WorldViewManager.CurrentView;
        }

        public static GraphicsDevice Device;

        //public static int MSAA = 0;

        //public static FlverShader FlverShader;
        //public static DbgPrimShader DbgPrimShader;
        public static SpriteBatch SpriteBatch;
        public static string FlverShader__Name => $@"{Main.Directory}\Content\Shaders\FlverShader";
        public static string CollisionShader__Name => $@"{Main.Directory}\Content\Shaders\CollisionShader";
        public static string SkyboxShader__Name => $@"{Main.Directory}\Content\Shaders\CubemapSkyboxShader";

        private static bool _wireframe = false;
        public static bool Wireframe
        {
            set
            {
                _wireframe = value;
                UpdateRasterizerState();
            }
            get => _wireframe;
        }

        private static bool _backfaceCulling = false;
        public static bool BackfaceCulling
        {
            set
            {
                _backfaceCulling = value;
                UpdateRasterizerState();
            }
            get => _backfaceCulling;
        }

        private static void UpdateRasterizerState()
        {
            if (!BackfaceCulling && !Wireframe)
                Device.RasterizerState = HotSwapRasterizerState_BackfaceCullingOff_WireframeOff;
            else if (!BackfaceCulling && Wireframe)
                Device.RasterizerState = HotSwapRasterizerState_BackfaceCullingOff_WireframeOn;
            else if (BackfaceCulling && !Wireframe)
                Device.RasterizerState = HotSwapRasterizerState_BackfaceCullingOn_WireframeOff;
            else if (BackfaceCulling && Wireframe)
                Device.RasterizerState = HotSwapRasterizerState_BackfaceCullingOn_WireframeOn;
        }

        private static void CompletelyChangeRasterizerState(RasterizerState rs)
        {
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOff = rs.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOff.CullMode = CullMode.None;
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOff.FillMode = FillMode.Solid;

            HotSwapRasterizerState_BackfaceCullingOn_WireframeOff = rs.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOff.CullMode = CullMode.CullClockwiseFace;
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOff.FillMode = FillMode.Solid;

            HotSwapRasterizerState_BackfaceCullingOff_WireframeOn = rs.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOn.CullMode = CullMode.None;
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOn.FillMode = FillMode.WireFrame;

            HotSwapRasterizerState_BackfaceCullingOn_WireframeOn = rs.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOn.CullMode = CullMode.CullClockwiseFace;
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOn.FillMode = FillMode.WireFrame;

            UpdateRasterizerState();
        }

        public static void InitDepthStencil(bool writeDepth)
        {
            Device.DepthStencilState = writeDepth ? DepthStencilState_Normal : DepthStencilState_DontWriteDepth;
        }
        //private static ContentManager DebugReloadContentManager = null;
        public static void ReloadFlverShader()
        {
            lock (Scene._lock_ModelLoad_Draw)
            {
                if (File.Exists($@"{Main.Directory}\..\..\..\..\Content\Shaders\FlverShader.xnb"))
                {
                    FlverShader?.Effect?.Dispose();
                    FlverShader = null;
                    FlverShader = new FlverShader(Main.ReloadMonoGameContent<Effect>($@"{Main.Directory}\..\..\..\..\Content\Shaders\FlverShader", GFX.FlverShader__Name));
                    InitShaders();
                }
                else
                {
                    var fullPath = Path.GetFullPath($@"{Main.Directory}\..\..\..\..\Content\Shaders\FlverShader.xnb");
                    ImguiOSD.DialogManager.DialogOK("File Not Found", $@"Could not find shader file '{fullPath}'.");
                }
            }
        }

        public static void ReloadTonemapShader()
        {
            lock (Scene._lock_ModelLoad_Draw)
            {
                if (File.Exists($@"{Main.Directory}\..\..\..\..\Content\Shaders\CubemapSkyboxShader.xnb"))
                {
                    Main.MainFlverTonemapShader?.Effect?.Dispose();
                    Main.MainFlverTonemapShader = null;
                    Main.MainFlverTonemapShader = new FlverTonemapShader(Main.ReloadMonoGameContent<Effect>($@"{Main.Directory}\..\..\..\..\Content\Shaders\FlverTonemapShader", Main.FlverTonemapShader__Name));
                    InitShaders();
                }
                else
                {
                    var fullPath = Path.GetFullPath($@"{Main.Directory}\..\..\..\..\Content\Shaders\FlverTonemapShader.xnb");
                    ImguiOSD.DialogManager.DialogOK("File Not Found", $@"Could not find shader file '{fullPath}'.");
                }
            }
        }

        public static void ReloadCubemapSkyboxShader()
        {
            lock (Scene._lock_ModelLoad_Draw)
            {
                if (File.Exists($@"{Main.Directory}\..\..\..\..\Content\Shaders\CubemapSkyboxShader.xnb"))
                {
                    SkyboxShader?.Effect?.Dispose();
                    SkyboxShader = null;
                    SkyboxShader = new SkyboxShader(Main.ReloadMonoGameContent<Effect>($@"{Main.Directory}\..\..\..\..\Content\Shaders\CubemapSkyboxShader", GFX.SkyboxShader__Name));
                    InitShaders();
                }
                else
                {
                    var fullPath = Path.GetFullPath($@"{Main.Directory}\..\..\..\..\Content\Shaders\CubemapSkyboxShader.xnb");
                    ImguiOSD.DialogManager.DialogOK("File Not Found", $@"Could not find shader file '{fullPath}'.");
                }
            }
        }

        public static void Init(ContentManager c)
        {
            DX11.Init(Device);
            
            DepthStencilState_Normal = new DepthStencilState()
            {
                //CounterClockwiseStencilDepthBufferFail = Device.DepthStencilState.CounterClockwiseStencilDepthBufferFail,
                //CounterClockwiseStencilFail = Device.DepthStencilState.CounterClockwiseStencilFail,
                //CounterClockwiseStencilFunction = Device.DepthStencilState.CounterClockwiseStencilFunction,
                //CounterClockwiseStencilPass = Device.DepthStencilState.CounterClockwiseStencilPass,
                //DepthBufferEnable = Device.DepthStencilState.DepthBufferEnable,
                //DepthBufferFunction = Device.DepthStencilState.DepthBufferFunction,
                DepthBufferWriteEnable = true,
                //ReferenceStencil = Device.DepthStencilState.ReferenceStencil,
                //StencilDepthBufferFail = Device.DepthStencilState.StencilDepthBufferFail,
                //StencilEnable = Device.DepthStencilState.StencilEnable,
                //StencilFail = Device.DepthStencilState.StencilFail,
                //StencilFunction = Device.DepthStencilState.StencilFunction,
                //StencilMask = Device.DepthStencilState.StencilMask,
                //StencilPass = Device.DepthStencilState.StencilPass,
                //StencilWriteMask = Device.DepthStencilState.StencilWriteMask,
                //TwoSidedStencilMode = Device.DepthStencilState.TwoSidedStencilMode,
            };

            DepthStencilState_DontWriteDepth = new DepthStencilState()
            {
                //CounterClockwiseStencilDepthBufferFail = Device.DepthStencilState.CounterClockwiseStencilDepthBufferFail,
                //CounterClockwiseStencilFail = Device.DepthStencilState.CounterClockwiseStencilFail,
                //CounterClockwiseStencilFunction = Device.DepthStencilState.CounterClockwiseStencilFunction,
                //CounterClockwiseStencilPass = Device.DepthStencilState.CounterClockwiseStencilPass,
                //DepthBufferEnable = Device.DepthStencilState.DepthBufferEnable,
                //DepthBufferFunction = Device.DepthStencilState.DepthBufferFunction,
                DepthBufferWriteEnable = false,
                //ReferenceStencil = Device.DepthStencilState.ReferenceStencil,
                //StencilDepthBufferFail = Device.DepthStencilState.StencilDepthBufferFail,
                //StencilEnable = Device.DepthStencilState.StencilEnable,
                //StencilFail = Device.DepthStencilState.StencilFail,
                //StencilFunction = Device.DepthStencilState.StencilFunction,
                //StencilMask = Device.DepthStencilState.StencilMask,
                //StencilPass = Device.DepthStencilState.StencilPass,
                //StencilWriteMask = Device.DepthStencilState.StencilWriteMask,
                //TwoSidedStencilMode = Device.DepthStencilState.TwoSidedStencilMode,
            };

            Environment.LoadContent(c);

            FlverShader = new FlverShader(c.Load<Effect>(FlverShader__Name));
            SkyboxShader = new SkyboxShader(c.Load<Effect>(SkyboxShader__Name));

            InitShaders();

            DbgPrimWireShader = new DbgPrimWireShader(Device);
            DbgPrimSolidShader = new DbgPrimSolidShader(Device);

            DbgPrimSolidShader.Effect.AmbientLightColor = new Vector3(FlverShader.Effect.Legacy_AmbientColor.X, FlverShader.Effect.Legacy_AmbientColor.Y, FlverShader.Effect.Legacy_AmbientColor.Z) * FlverShader.Effect.Legacy_AmbientIntensity;
            DbgPrimSolidShader.Effect.DiffuseColor = new Vector3(FlverShader.Effect.Legacy_DiffuseColor.X, FlverShader.Effect.Legacy_DiffuseColor.Y, FlverShader.Effect.Legacy_DiffuseColor.Z) * FlverShader.Effect.Legacy_DiffuseIntensity;
            DbgPrimSolidShader.Effect.SpecularColor = new Vector3(FlverShader.Effect.Legacy_SpecularColor.X, FlverShader.Effect.Legacy_SpecularColor.Y, FlverShader.Effect.Legacy_SpecularColor.Z);
            DbgPrimSolidShader.Effect.SpecularPower = FlverShader.Effect.Legacy_SpecularPower;

            CollisionShader = new CollisionShader(c.Load<Effect>(CollisionShader__Name));
            CollisionShader.Effect.AmbientColor = new Vector4(0.2f, 0.5f, 0.9f, 1.0f);
            CollisionShader.Effect.DiffuseColor = new Vector4(0.2f, 0.5f, 0.9f, 1.0f);

            SpriteBatch = new SpriteBatch(Device);

            HotSwapRasterizerState_BackfaceCullingOff_WireframeOff = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOff.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOff.CullMode = CullMode.None;
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOff.FillMode = FillMode.Solid;

            HotSwapRasterizerState_BackfaceCullingOn_WireframeOff = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOff.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOff.CullMode = CullMode.CullClockwiseFace;
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOff.FillMode = FillMode.Solid;

            HotSwapRasterizerState_BackfaceCullingOff_WireframeOn = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOn.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOn.CullMode = CullMode.None;
            HotSwapRasterizerState_BackfaceCullingOff_WireframeOn.FillMode = FillMode.WireFrame;

            HotSwapRasterizerState_BackfaceCullingOn_WireframeOn = Device.RasterizerState.GetCopyOfState();
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOn.MultiSampleAntiAlias = true;
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOn.CullMode = CullMode.CullClockwiseFace;
            HotSwapRasterizerState_BackfaceCullingOn_WireframeOn.FillMode = FillMode.WireFrame;

            //Bokeh = new BokehShader();
            //Bokeh.ShaderEffect = c.Load<Effect>(@"Content\Shaders\Bokeh");
            //Bokeh.Initialize(Device);
            //BokehShapeHexagon = c.Load<Texture2D>(@"Content\Shaders\BokehShapeHexagon");
        }

        public static bool SpriteBatchHasBegun { get; set; } = false;

        public static void SpriteBatchBeginForText(Matrix? transformMatrix = null)
        {
            if (SpriteBatchHasBegun)
                SpriteBatchEnd();
            GFX.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, transformMatrix: (transformMatrix ?? Matrix.Identity) * Main.DPIMatrix, samplerState: SamplerState.AnisotropicClamp);
            SpriteBatchHasBegun = true;
        }

        public static void SpriteBatchBegin(SpriteSortMode sortMode = SpriteSortMode.Deferred, 
            BlendState blendState = null, SamplerState samplerState = null, 
            DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, 
            Effect effect = null, Matrix? transformMatrix = null)
        {
            if (SpriteBatchHasBegun)
                SpriteBatchEnd();
            Matrix m = Main.DPIMatrix;
            if (transformMatrix.HasValue)
                m *= transformMatrix.Value;
            SpriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, 
                rasterizerState, effect, m);
            SpriteBatchHasBegun = true;
        }

        public static void SpriteBatchEnd()
        {
            if (SpriteBatchHasBegun)
            GFX.SpriteBatch.End();
            SpriteBatchHasBegun = false;
        }

        public static void BeginDraw()
        {
            InitDepthStencil(CurrentStep == GFXDrawStep.Opaque);
            //InitBlendState();

            Device.BlendState = BlendState.NonPremultiplied;

            CurrentWorldView.ApplyViewToShader(DbgPrimWireShader, Matrix.Identity);
            CurrentWorldView.ApplyViewToShader(DbgPrimSolidShader, Matrix.Identity);
            CurrentWorldView.ApplyViewToShader(FlverShader, Matrix.Identity);
            CurrentWorldView.ApplyViewToShader(CollisionShader, Matrix.Identity);

            //foreach (var m in ModelDrawer.Models)
            //{
            //    m.ApplyWorldToInstances(World.CameraTransform.RotationMatrix);
            //    m.ReinitInstanceData();
            //}

            Device.SamplerStates[0] = SamplerState.LinearWrap;

            //FlverShader.Effect.EyePosition = World.NewCameraTransform.Position;

            FlverShader.Effect.EyePosition = CurrentWorldView.CameraLocationInWorld.Position;

            //SkyboxShader.Effect.EyePosition = World.CameraTransform.Position;

            //var curForw = World.GetCameraForward();
            //if (curForw != SkyboxShader_PrevFrameLookDir)
            //{
            //    curForw = XnaExtensions.Vector3Slerp(SkyboxShader_PrevFrameLookDir, World.GetCameraForward(), 0.6f);
            //    var nextSkyboxMotionBlurVec = -((curForw - SkyboxShader_PrevFrameLookDir) * Environment.MotionBlurStrength);
            //    var nextSkyboxMotionBlurVec_Dir = Vector3.Normalize(nextSkyboxMotionBlurVec);
            //    if (float.IsNaN(nextSkyboxMotionBlurVec_Dir.X))
            //    {
            //        nextSkyboxMotionBlurVec_Dir = Vector3.Zero;
            //    }
            //    if (float.IsNaN(SkyboxShader.Effect.MotionBlurVector.X))
            //    {
            //        SkyboxShader.Effect.MotionBlurVector = Vector3.Zero;
            //    }
            //    var nextSkyboxMotionBlurVec_Len = nextSkyboxMotionBlurVec.Length();
            //    //nextSkyboxMotionBlurVec_Len = Math.Max(nextSkyboxMotionBlurVec_Len, 0.01f);
            //    nextSkyboxMotionBlurVec = nextSkyboxMotionBlurVec_Dir * nextSkyboxMotionBlurVec_Len;
            //    SkyboxShader.Effect.MotionBlurVector = nextSkyboxMotionBlurVec;
            //}
            //SkyboxShader_PrevFrameLookDir = curForw;

            //SkyboxShader.Effect.MotionBlurVector = Vector3.Transform(Vector3.Right, World.CameraLookDirection) * Environment.MotionBlurStrength;

            SkyboxShader.Effect.EyePosition = Vector3.Zero;


            Matrix matLightDir = Matrix.CreateRotationY(Environment.LightRotationH) * Matrix.CreateRotationX(Environment.LightRotationV);

            if (FlverAutoRotateLight)
            {
                LightSpinTimer = (LightSpinTimer + Main.DELTA_UPDATE);
                FlverShader.Effect.LightDirection = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationY(MathHelper.Pi * LightSpinTimer * 0.05f));
            }
            else 
            {
                Vector3 camUpDir = CurrentWorldView.GetCameraUp() * new Vector3(1, 1, 1);
                Vector3 camLookDir = CurrentWorldView.GetCameraForward() * new Vector3(1, 1, 1);
                Matrix matCamLook = Matrix.CreateWorld(Vector3.Zero, camLookDir, camUpDir);

                if (FlverLightFollowsCamera)
                    FlverShader.Effect.LightDirection = Vector3.TransformNormal(Vector3.Forward, matLightDir * matCamLook);
                else
                    FlverShader.Effect.LightDirection = Vector3.TransformNormal(Vector3.Forward, matLightDir);

                //if (FlverLightFollowsCamera)
                //    FlverShader.Effect.LightDirection = Vector3.TransformNormal(camLookDir, matLightDir);
                //else
                //    FlverShader.Effect.LightDirection = Vector3.TransformNormal(Vector3.Forward, matLightDir);
            }

            //FlverShader.Effect.LightDirection = World.CameraTransform.RotationMatrix;
            FlverShader.Effect.ColorMap = Main.DEFAULT_TEXTURE_DIFFUSE;
            FlverShader.Effect.NormalMap = (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS2SOTFS ?
                Main.DEFAULT_TEXTURE_NORMAL_DS2 : Main.DEFAULT_TEXTURE_NORMAL);
            FlverShader.Effect.SpecularMap = Main.DEFAULT_TEXTURE_SPECULAR;
            FlverShader.Effect.EmissiveMap = Main.DEFAULT_TEXTURE_EMISSIVE;
            FlverShader.Effect.BlendmaskMap = Main.DEFAULT_TEXTURE_EMISSIVE;

            FlverShader.Effect.EnvironmentMap = Environment.CurrentCubemap;
            SkyboxShader.Effect.EnvironmentMap = Environment.CurrentCubemap;

            FlverShader.Effect.AmbientLightMult = Environment.AmbientLightMult;
            FlverShader.Effect.DirectLightMult = Environment.FlverDirectLightMult * 1 * (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ? 0.6f : 1);
            FlverShader.Effect.IndirectLightMult = Environment.FlverIndirectLightMult * (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ? 0.6f : 1);

            FlverShader.Effect.DirectDiffuseMult = Environment.DirectDiffuseMult;
            FlverShader.Effect.DirectSpecularMult = Environment.DirectSpecularMult;
            FlverShader.Effect.IndirectDiffuseMult = Environment.IndirectDiffuseMult;
            FlverShader.Effect.IndirectSpecularMult = Environment.IndirectSpecularMult;

            FlverShader.Effect.SceneBrightness = Environment.FlverSceneBrightness * 1.45f * 2;
            FlverShader.Effect.EmissiveMapMult = Environment.FlverEmissiveMult;
            FlverShader.Effect.Legacy_SceneBrightness = Environment.FlverSceneBrightness * 1.45f;
            FlverShader.Effect.Opacity = 1;
            FlverShader.Effect.Parameters["SpecularPowerMult"].SetValue(SpecularPowerMult * (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R ? 0.3f : 1));
            FlverShader.Effect.Parameters["LdotNPower"].SetValue(LdotNPower);

            SkyboxShader.Effect.AmbientLightMult = Environment.FlverIndirectLightMult * Environment.SkyboxBrightnessMult;
            SkyboxShader.Effect.SceneBrightness = Environment.FlverSceneBrightness * 1.45f;

            Main.MainFlverTonemapShader.Effect.Parameters["SceneContrast"].SetValue(Environment.FlverSceneContrast);

            DbgPrimSolidShader.Effect.DirectionalLight0.DiffuseColor = Vector3.One * 0.45f;
            DbgPrimSolidShader.Effect.DirectionalLight0.SpecularColor = Vector3.One * 0.35f;
            DbgPrimSolidShader.Effect.DirectionalLight0.Direction = FlverShader.Effect.LightDirection;
            DbgPrimSolidShader.Effect.DirectionalLight1.Enabled = false;
            DbgPrimSolidShader.Effect.DirectionalLight2.Enabled = false;

            CollisionShader.Effect.EyePosition = CurrentWorldView.CameraLocationInWorld.Position;

        }

        private static void DoDrawStep()
        {
            switch (CurrentStep)
            {
                case GFXDrawStep.Opaque:
                case GFXDrawStep.AlphaEdge:
                    Scene.Draw();
                    break;
                case GFXDrawStep.DbgPrimOverlay:
                    DBG.DrawPrimitives();
                    break;
                case GFXDrawStep.DbgPrimPrepass:
                    DBG.DrawBehindPrims();
                    break;
                case GFXDrawStep.DbgPrimAlwaysRespectDepth:
                    DBG.DrawDepthRespectPrims();
                    break;
                case GFXDrawStep.DbgPrimAlwaysDisrespectDepth:
                    DBG.DrawDepthDisrespectPrims();
                    break;
                case GFXDrawStep.GUI:
                    DBG.DrawPrimitiveTexts();
                    if (DBG.EnableMenu)
                        DbgMenuItem.CurrentMenu.Draw();
                    break;
            }
        }

        private static void DoDraw()
        {
            if (Main.DISABLE_DRAW_ERROR_HANDLE)
            {
                DoDrawStep();
            }
            else
            {
                try
                {
                    DoDrawStep();
                }
                catch (Exception ex)
                {
                    var errText = $"Draw Call Failed ({CurrentStep.ToString()}):\n\n{ex.ToString()}";
                    var errTextSize = DBG.DEBUG_FONT_SIMPLE.MeasureString(errText);
                    float hScale = ((GFX.LastViewport.Width) / (errTextSize.X)) * GFX.EffectiveSSAA;

                    if (SpriteBatchHasBegun)
                        SpriteBatchEnd();

                    //SpriteBatchBeginForText();

                    //DBG.DrawOutlinedText(errText, new Vector2(0, GFX.LastViewport.Height / 2 - (errTextSize.Y / 2)), 
                    //    Color.Yellow, DBG.DEBUG_FONT_SIMPLE, 0, new Vector2(hScale, GFX.EffectiveSSAA), Vector2.Zero);

                    //SpriteBatchEnd();
                    ImGuiDebugDrawer.DrawText(errText, new Vector2(0, GFX.LastViewport.Height / 2 - (errTextSize.Y / 2)), Color.Red, fontSize: 8);
                }
            }
        }

        public static void DrawScene3D()
        {
            CurrentStep = GFXDrawStep.DbgPrimPrepass;
            BeginDraw();
            DoDraw();

            CurrentStep = GFXDrawStep.Opaque;
            BeginDraw();
            DoDraw();

            CurrentStep = GFXDrawStep.AlphaEdge;
            BeginDraw();
            DoDraw();
        }

        public static void DrawPrimRespectDepth()
        {
            CurrentStep = GFXDrawStep.DbgPrimAlwaysRespectDepth;
            BeginDraw();
            DoDraw();
        }

        public static void DrawPrimDisrespectDepth()
        {
            CurrentStep = GFXDrawStep.DbgPrimAlwaysDisrespectDepth;
            BeginDraw();
            DoDraw();
        }

        public static void DrawSceneOver3D()
        {
            CurrentStep = GFXDrawStep.DbgPrimOverlay;
            BeginDraw();
            DoDraw();
        }

        public static void DrawSceneGUI()
        {
            
            CurrentStep = GFXDrawStep.GUI;
            BeginDraw();
            DoDraw();

            GFX.UpdateFPS((float)FpsStopwatch.Elapsed.TotalSeconds);
            if (Main.SceneRenderTarget != null)
            {
                float scale = Main.Config.ViewportFramerateTextSize / 100f;
                if (scale > 0)
                {
                    GFX.SpriteBatchBeginForText();
                    //DBG.DrawOutlinedText($"Rendering {(Main.SceneRenderTarget.Width)}x{(Main.SceneRenderTarget.Height)} @ {(Math.Round(GFX.AverageFPS))} FPS",
                    //    new Vector2(4, (GFX.Device.Viewport.Height / Main.DPIY) - 24), Color.Cyan, font: DBG.DEBUG_FONT_SMALL);

                    ImGuiDebugDrawer.DrawText($"Rendering {(Main.SceneRenderTarget.Width)}x{(Main.SceneRenderTarget.Height)} @ {(Math.Round(GFX.AverageFPS))} FPS",
                        new Vector2(4, (GFX.Device.Viewport.Height / Main.DPIY) - (16 * scale)), Color.Cyan, Color.Black, 16 * scale);

                    GFX.SpriteBatchEnd();
                }
            }
            //DBG.DrawOutlinedText($"FPS: {(Math.Round(1 / (float)gameTime.ElapsedGameTime.TotalSeconds))}", new Vector2(0, GFX.Device.Viewport.Height - 20), Color.Cyan, font: DBG.DEBUG_FONT_SMALL);
            FpsStopwatch.Restart();
        }

        public static void UpdateFPS(float elapsedSeconds)
        {
            UpdateHighlightColor(elapsedSeconds);
            FpsCounter.Update(elapsedSeconds);
        }
    }
}
