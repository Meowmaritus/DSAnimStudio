using TAEDX.DbgMenus;
using TAEDX.DebugPrimitives;
using TAEDX.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX
{
    public enum GFXDrawStep : byte
    {
        Opaque = 1,
        AlphaEdge = 2,
        DbgPrim = 3,
        GUI = 4,
        GUILoadingTasks = 5,
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
            public static bool SimpleMSAA = true;
            public static void Apply()
            {
                Main.ApplyPresentationParameters(Width, Height, Format, Vsync, Fullscreen, SimpleMSAA);
            }
        }

        public static void InitFlverMainShader()
        {
            FlverShader.Effect.AmbientColor = new Vector4(1, 1, 1, 1);
            FlverShader.Effect.AmbientIntensity = 0.25f;
            FlverShader.Effect.DiffuseColor = new Vector4(1, 1, 1, 1);
            FlverShader.Effect.DiffuseIntensity = 0.85f;
            FlverShader.Effect.SpecularColor = new Vector4(1, 1, 1, 1);
            FlverShader.Effect.SpecularPower = 8;
            FlverShader.Effect.NormalMapCustomZ = 1.0f;
            FlverShader.Effect.DiffusePower = 1 / 1.5f;
            FlverShader.Effect.AlphaTest = 0.1f;
        }

        public static GFXDrawStep CurrentStep = GFXDrawStep.Opaque;

        public static readonly GFXDrawStep[] DRAW_STEP_LIST;
        static GFX()
        {
            DRAW_STEP_LIST = (GFXDrawStep[])Enum.GetValues(typeof(GFXDrawStep));
        }

        public static bool HideFLVERs = false;

        public const int LOD_MAX = 2;
        public static LODMode LODMode = LODMode.Automatic;
        public static float LOD1Distance = 200;
        public static float LOD2Distance = 400;

        public static IGFXShader<FlverShader> FlverShader;
        public static IGFXShader<CollisionShader> CollisionShader;
        public static IGFXShader<DbgPrimWireShader> DbgPrimWireShader;
        public static IGFXShader<DbgPrimSolidShader> DbgPrimSolidShader;

        public static Stopwatch FpsStopwatch = new Stopwatch();
        private static FrameCounter FpsCounter = new FrameCounter();

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

        public static WorldView World = new WorldView();

        public static ModelDrawer ModelDrawer = new ModelDrawer();

        public static GraphicsDevice Device;
        //public static FlverShader FlverShader;
        //public static DbgPrimShader DbgPrimShader;
        public static SpriteBatch SpriteBatch;
        const string FlverShader__Name = @"Content\NormalMapShader";
        const string CollisionShader__Name = @"Content\CollisionShader";

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

        public static void InitDepthStencil()
        {
            switch (CurrentStep)
            {
                case GFXDrawStep.Opaque:
                    Device.DepthStencilState = DepthStencilState_Normal;
                    break;
                default:
                    Device.DepthStencilState = DepthStencilState_DontWriteDepth;
                    break;
            }
        }

        public static void Init(ContentManager c)
        {

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

            FlverShader = new FlverShader(c.Load<Effect>(FlverShader__Name));

            InitFlverMainShader();

            DbgPrimWireShader = new DbgPrimWireShader(Device);
            DbgPrimSolidShader = new DbgPrimSolidShader(Device);

            DbgPrimSolidShader.Effect.AmbientLightColor = new Vector3(FlverShader.Effect.AmbientColor.X, FlverShader.Effect.AmbientColor.Y, FlverShader.Effect.AmbientColor.Z) * FlverShader.Effect.AmbientIntensity;
            DbgPrimSolidShader.Effect.DiffuseColor = new Vector3(FlverShader.Effect.DiffuseColor.X, FlverShader.Effect.DiffuseColor.Y, FlverShader.Effect.DiffuseColor.Z) * FlverShader.Effect.DiffuseIntensity;
            DbgPrimSolidShader.Effect.SpecularColor = new Vector3(FlverShader.Effect.SpecularColor.X, FlverShader.Effect.SpecularColor.Y, FlverShader.Effect.SpecularColor.Z);
            DbgPrimSolidShader.Effect.SpecularPower = FlverShader.Effect.SpecularPower;

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
        }

        public static void BeginDraw()
        {
            InitDepthStencil();
            //InitBlendState();

            World.ApplyViewToShader(DbgPrimWireShader);
            World.ApplyViewToShader(DbgPrimSolidShader);
            World.ApplyViewToShader(FlverShader);
            World.ApplyViewToShader(CollisionShader);

            //foreach (var m in ModelDrawer.Models)
            //{
            //    m.ApplyWorldToInstances(World.CameraTransform.RotationMatrix);
            //    m.ReinitInstanceData();
            //}

            Device.SamplerStates[0] = SamplerState.LinearWrap;

            FlverShader.Effect.EyePosition = World.CameraTransform.Position;
            //FlverShader.Effect.EyePosition = Vector3.Backward;
            FlverShader.Effect.LightDirection = -Vector3.Normalize(World.CameraTransform.Position);
            //FlverShader.Effect.LightDirection = World.CameraTransform.RotationMatrix;
            FlverShader.Effect.ColorMap = Main.DEFAULT_TEXTURE_DIFFUSE;
            FlverShader.Effect.NormalMap = Main.DEFAULT_TEXTURE_NORMAL;
            FlverShader.Effect.SpecularMap = Main.DEFAULT_TEXTURE_SPECULAR;

            DbgPrimSolidShader.Effect.DirectionalLight0.Enabled = true;
            DbgPrimSolidShader.Effect.DirectionalLight0.DiffuseColor = Vector3.One * 0.45f;
            DbgPrimSolidShader.Effect.DirectionalLight0.SpecularColor = Vector3.One * 0.35f;
            DbgPrimSolidShader.Effect.DirectionalLight0.Direction = FlverShader.Effect.LightDirection;
            DbgPrimSolidShader.Effect.DirectionalLight1.Enabled = false;
            DbgPrimSolidShader.Effect.DirectionalLight2.Enabled = false;

            CollisionShader.Effect.EyePosition = World.CameraTransform.Position;

        }

        private static void DoDrawStep(GameTime gameTime)
        {
            switch (CurrentStep)
            {
                case GFXDrawStep.Opaque:
                case GFXDrawStep.AlphaEdge:
                    if (!HideFLVERs)
                    {
                        ModelDrawer.Draw();
                        ModelDrawer.DebugDrawAll();
                    }
                    break;
                case GFXDrawStep.DbgPrim:
                    DBG.DrawPrimitives();
                    ModelDrawer.DrawSelected();
                    break;
                case GFXDrawStep.GUI:
                    if (DBG.EnableMenu)
                        DbgMenuItem.CurrentMenu.Draw((float)gameTime.ElapsedGameTime.TotalSeconds);
                    break;
                case GFXDrawStep.GUILoadingTasks:
                    LoadingTaskMan.DrawAllTasks();
                    break;
            }
        }

        private static void DoDraw(GameTime gameTime)
        {
            if (Main.DISABLE_DRAW_ERROR_HANDLE)
            {
                DoDrawStep(gameTime);
            }
            else
            {
                try
                {
                    DoDrawStep(gameTime);
                }
                catch (Exception ex)
                {
                    var errText = $"Draw Call Failed ({CurrentStep.ToString()}):\n\n{ex.ToString()}";
                    var errTextSize = DBG.DEBUG_FONT_SIMPLE.MeasureString(errText);
                    DBG.DrawOutlinedText(errText, new Vector2(Device.Viewport.Width / 2, Device.Viewport.Height / 2), Color.Red, DBG.DEBUG_FONT_SIMPLE, 0, 0.25f, errTextSize / 2);
                }
            }
        }

        public static void DrawBegin(GameTime gameTime)
        {
            Device.Clear(Color.Gray);
        }

        public static void DrawScene(GameTime gameTime)
        {
            //Device.Viewport = new Viewport(new Rectangle(200, 200, 500, 500));

            for (int i = 0; i < DRAW_STEP_LIST.Length; i++)
            {
                CurrentStep = DRAW_STEP_LIST[i];
                BeginDraw();
                DoDraw(gameTime);
            }

            GFX.UpdateFPS((float)FpsStopwatch.Elapsed.TotalSeconds);
            DBG.DrawOutlinedText($"FPS: {(Math.Round(GFX.AverageFPS))}", new Vector2(0, GFX.Device.Viewport.Height - 16), Color.Yellow, scale: 0.75f);
            FpsStopwatch.Restart();
        }

        public static void UpdateFPS(float elapsedSeconds)
        {
            FpsCounter.Update(elapsedSeconds);
        }
    }
}
