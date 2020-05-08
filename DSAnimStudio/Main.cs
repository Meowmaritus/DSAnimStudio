using DSAnimStudio.DbgMenus;
using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DSAnimStudio
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        public static Form WinForm;

        public static Random Rand = new Random();
        public static float RandFloat()
        {
            return (float)Rand.NextDouble();
        }
        public static float RandSignedFloat()
        {
            return (float)((Rand.NextDouble() * 2) - 1);
        }
        public static Vector3 RandSignedVector3()
        {
            return new Vector3(RandSignedFloat(), RandSignedFloat(), RandSignedFloat());
        }

        public static string Directory = null;

        public const string VERSION = "Version 1.9-RC6";

        public static bool FIXED_TIME_STEP = false;

        public static bool REQUEST_EXIT = false;

        public static float DELTA_UPDATE;
        public static float DELTA_UPDATE_ROUNDED;
        public static float DELTA_DRAW;

        public static ImGuiRenderer ImGuiDraw;

        public static Vector2 GlobalTaeEditorFontOffset = new Vector2(0, -3);

        public static IServiceProvider ContentServiceProvider = null;

        private bool prevFrameWasLoadingTaskRunning = false;

        public static bool Active { get; private set; }

        public static bool DISABLE_DRAW_ERROR_HANDLE = false;

        private static float MemoryUsageCheckTimer = 0;
        private static long MemoryUsage_Unmanaged = 0;
        private static long MemoryUsage_Managed = 0;
        private const float MemoryUsageCheckInterval = 0.25f;

        public static readonly Color SELECTED_MESH_COLOR = Color.Yellow * 0.05f;
        //public static readonly Color SELECTED_MESH_WIREFRAME_COLOR = Color.Yellow;

        public static Texture2D DEFAULT_TEXTURE_DIFFUSE;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR;
        public static Texture2D DEFAULT_TEXTURE_NORMAL;
        public static Texture2D DEFAULT_TEXTURE_MISSING;
        public static TextureCube DEFAULT_TEXTURE_MISSING_CUBE;
        public static Texture2D DEFAULT_TEXTURE_EMISSIVE;
        public string DEFAULT_TEXTURE_MISSING_NAME => $@"{Main.Directory}\Content\Utility\MissingTexture";

        public static TaeEditor.TaeEditorScreen TAE_EDITOR;
        private static SpriteBatch TaeEditorSpriteBatch;
        public static Texture2D TAE_EDITOR_BLANK_TEX;
        public static SpriteFont TAE_EDITOR_FONT;
        public static SpriteFont TAE_EDITOR_FONT_SMALL;
        public static Texture2D TAE_EDITOR_SCROLLVIEWER_ARROW;

        public static FlverTonemapShader MainFlverTonemapShader = null;

        //public static Stopwatch UpdateStopwatch = new Stopwatch();
        //public static TimeSpan MeasuredTotalTime = TimeSpan.Zero;
        //public static TimeSpan MeasuredElapsedTime = TimeSpan.Zero;

        public bool IsLoadingTaskRunning = false;

        public static ContentManager CM = null;

        public static RenderTarget2D SceneRenderTarget = null;

        public static int RequestHideOSD = 0;
        public static int RequestHideOSD_MAX = 10;

        public static bool RequestViewportRenderTargetResolutionChange = false;
        private const float TimeBeforeNextRenderTargetUpdate_Max = 0.5f;
        private static float TimeBeforeNextRenderTargetUpdate = 0;

        public Rectangle TAEScreenBounds
        {
            get => GFX.Device.Viewport.Bounds;
            set
            {
                if (value != TAEScreenBounds)
                {
                    GFX.Device.Viewport = new Viewport(value);
                }
            }
        }

        public Rectangle ClientBounds => TAE_EDITOR.ModelViewerBounds;

        private static GraphicsDeviceManager graphics;
        //public ContentManager Content;
        //public bool IsActive = true;

        public static List<DisplayMode> GetAllResolutions()
        {
            List<DisplayMode> result = new List<DisplayMode>();
            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                result.Add(mode);
            }
            return result;
        }

        public static void ApplyPresentationParameters(int width, int height, SurfaceFormat format,
            bool vsync, bool fullscreen)
        {
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferFormat = GFX.BackBufferFormat;
            graphics.IsFullScreen = fullscreen;
            graphics.SynchronizeWithVerticalRetrace = vsync;

            //if (GFX.MSAA > 0)
            //{
            //    graphics.PreferMultiSampling = true;
            //    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = GFX.MSAA;
            //}
            //else
            //{
            //    graphics.PreferMultiSampling = false;
            //    graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 1;
            //}

            graphics.PreferMultiSampling = false;
            graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 1;

            graphics.ApplyChanges();
        }



        //MCG MCGTEST_MCG;



        public Main()
        {
            WinForm = (Form)Form.FromHandle(Window.Handle);

            Directory = new FileInfo(typeof(Main).Assembly.Location).DirectoryName;

            graphics = new GraphicsDeviceManager(this);
            graphics.DeviceCreated += Graphics_DeviceCreated;
            graphics.DeviceReset += Graphics_DeviceReset;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166667);
            // Setting this max higher allows it to skip frames instead of do slow motion.
            MaxElapsedTime = TimeSpan.FromSeconds(0.5);

            //IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = GFX.Display.Vsync;
            graphics.IsFullScreen = GFX.Display.Fullscreen;
            //graphics.PreferMultiSampling = GFX.Display.SimpleMSAA;
            graphics.PreferredBackBufferWidth = GFX.Display.Width;
            graphics.PreferredBackBufferHeight = GFX.Display.Height;
            if (!GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef))
            {
                System.Windows.Forms.MessageBox.Show("MonoGame is detecting your GPU as too " +
                    "low-end and refusing to enter the non-mobile Graphics Profile, " +
                    "which is needed for the model viewer. The app will likely crash now.");

                graphics.GraphicsProfile = GraphicsProfile.Reach;
            }
            else
            {
                graphics.GraphicsProfile = GraphicsProfile.HiDef;
            }

            graphics.PreferredBackBufferFormat = GFX.BackBufferFormat;

            graphics.PreferMultiSampling = false;

            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

            Window.ClientSizeChanged += Window_ClientSizeChanged;

            GFX.Display.SetFromDisplayMode(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);

            //GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            RequestHideOSD = RequestHideOSD_MAX;
        }

        public void RebuildRenderTarget()
        {
            if (TimeBeforeNextRenderTargetUpdate <= 0)
            {
                int msaa = GFX.MSAA;
                int ssaa = GFX.SSAA;

                SceneRenderTarget?.Dispose();
                GC.Collect();
                SceneRenderTarget = new RenderTarget2D(GFX.Device, TAE_EDITOR.ModelViewerBounds.Width * ssaa,
                       TAE_EDITOR.ModelViewerBounds.Height * ssaa, ssaa > 1, SurfaceFormat.Vector4, DepthFormat.Depth24, 
                       ssaa > 1 ? 1 : msaa, RenderTargetUsage.PlatformContents);

                TimeBeforeNextRenderTargetUpdate = TimeBeforeNextRenderTargetUpdate_Max;

                RequestViewportRenderTargetResolutionChange = false;

                GFX.EffectiveSSAA = ssaa;
                GFX.EffectiveMSAA = msaa;
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            CFG.Save();

            TAE_EDITOR.SaveConfig();

            TaeEditor.TaeSoundManager.DisposeAll();

            base.OnExiting(sender, args);
        }

        private void Graphics_DeviceCreated(object sender, System.EventArgs e)
        {
            GFX.Device = GraphicsDevice;
        }

        private void Graphics_DeviceReset(object sender, System.EventArgs e)
        {
            GFX.Device = GraphicsDevice;
        }

        protected override void Initialize()
        {
            try
            {
                var winForm = (Form)Control.FromHandle(Window.Handle);
                winForm.AllowDrop = true;
                winForm.DragEnter += GameWindowForm_DragEnter;
                winForm.DragDrop += GameWindowForm_DragDrop;


                IsMouseVisible = true;

                DEFAULT_TEXTURE_DIFFUSE = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_DIFFUSE.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

                DEFAULT_TEXTURE_SPECULAR = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_SPECULAR.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

                DEFAULT_TEXTURE_NORMAL = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_NORMAL.SetData(new Color[] { new Color(0.5f, 0.5f, 1.0f) });

                DEFAULT_TEXTURE_EMISSIVE = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_EMISSIVE.SetData(new Color[] { Color.Black });

                DEFAULT_TEXTURE_MISSING = Content.Load<Texture2D>(DEFAULT_TEXTURE_MISSING_NAME);

                DEFAULT_TEXTURE_MISSING_CUBE = new TextureCube(GraphicsDevice, 1, false, SurfaceFormat.Color);
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.PositiveX, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.PositiveY, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.PositiveZ, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.NegativeX, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.NegativeY, new Color[] { Color.Fuchsia });
                DEFAULT_TEXTURE_MISSING_CUBE.SetData(CubeMapFace.NegativeZ, new Color[] { Color.Fuchsia });

                GFX.Device = GraphicsDevice;

                ImGuiDraw = new ImGuiRenderer(this);
                //ImGuiDraw.RebuildFontAtlas();

                base.Initialize();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    $"Error occurred while initializing DS Anim Studio (please report):\n\n{ex.ToString()}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        private void GameWindowForm_DragDrop(object sender, DragEventArgs e)
        {
            //string[] modelFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            //TAE_EDITOR.

            //LoadDragDroppedFiles(modelFiles.ToDictionary(f => f, f => File.ReadAllBytes(f)));
        }

        private void GameWindowForm_DragEnter(object sender, DragEventArgs e)
        {
            //if (e.Data.GetDataPresent(DataFormats.FileDrop))
            //{
            //    e.Effect = DragDropEffects.All;
            //}
            //else
            //{
            //    e.Effect = DragDropEffects.None;
            //}
        }

        protected override void LoadContent()
        {
            ContentServiceProvider = Content.ServiceProvider;
            CM = Content;

            GFX.Init(Content);
            DBG.LoadContent(Content);
            //InterrootLoader.OnLoadError += InterrootLoader_OnLoadError;

            DBG.CreateDebugPrimitives();

            GFX.World.ResetCameraLocation();

            //DBG.EnableMenu = true;
            //DBG.EnableMouseInput = true;
            //DBG.EnableKeyboardInput = true;
            //DbgMenuItem.Init();

            UpdateMemoryUsage();

            CFG.AttemptLoadOrDefault();

            TAE_EDITOR_FONT = Content.Load<SpriteFont>($@"{Main.Directory}\Content\Fonts\DbgMenuFontSmall");
            TAE_EDITOR_FONT_SMALL = Content.Load<SpriteFont>($@"{Main.Directory}\Content\Fonts\DbgMenuFontSmaller");
            TAE_EDITOR_BLANK_TEX = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            TAE_EDITOR_BLANK_TEX.SetData(new Color[] { Color.White }, 0, 1);
            TAE_EDITOR_SCROLLVIEWER_ARROW = Content.Load<Texture2D>($@"{Main.Directory}\Content\Utility\TaeEditorScrollbarArrow");

            TAE_EDITOR = new TaeEditor.TaeEditorScreen((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle));

            TaeEditorSpriteBatch = new SpriteBatch(GFX.Device);

            if (Program.ARGS.Length > 0)
            {
                TAE_EDITOR.FileContainerName = Program.ARGS[0];

                LoadingTaskMan.DoLoadingTask("ProgramArgsLoad", "Loading ANIBND and associated model(s)...", progress =>
                {
                    TAE_EDITOR.LoadCurrentFile();
                }, disableProgressBarByDefault: true);

                //LoadDragDroppedFiles(Program.ARGS.ToDictionary(f => f, f => File.ReadAllBytes(f)));
            }

            MainFlverTonemapShader = new FlverTonemapShader(Content.Load<Effect>($@"Content\Shaders\FlverTonemapShader"));

            var fonts = ImGuiNET.ImGui.GetIO().Fonts;

            var fontFile = File.ReadAllBytes($@"{Directory}\Content\Fonts\NotoSansCJKjp-Medium.otf");

            fonts.Clear();

            unsafe
            {
                fixed (byte* p = fontFile)
                {
                    var ptr = ImGuiNET.ImGuiNative.ImFontConfig_ImFontConfig();
                    var cfg = new ImGuiNET.ImFontConfigPtr(ptr);
                    cfg.GlyphMinAdvanceX = 5.0f;
                    cfg.OversampleH = 5;
                    cfg.OversampleV = 5;
                    var f = fonts.AddFontFromMemoryTTF((IntPtr)p, fontFile.Length, 16.0f, cfg, fonts.GetGlyphRangesDefault());
                }
            }

            fonts.Build();

            ImGuiDraw.RebuildFontAtlas();
        }

        private static void DrawImGui(GameTime gameTime, int x, int y, int w, int h)
        {
            ImGuiDraw.BeforeLayout(gameTime, x, 0, w, h + y);

            OSD.Build(Main.DELTA_DRAW, x, y);

            ImGuiDraw.AfterLayout(x, 0, w, h + y);
        }

        private void InterrootLoader_OnLoadError(string contentName, string error)
        {
            Console.WriteLine($"CONTENT LOAD ERROR\nCONTENT NAME:{contentName}\nERROR:{error}");
        }

        private string GetMemoryUseString(string prefix, long MemoryUsage)
        {
            const double MEM_KB = 1024f;
            const double MEM_MB = 1024f * 1024f;
            //const double MEM_GB = 1024f * 1024f * 1024f;

            if (MemoryUsage < MEM_KB)
                return $"{prefix}{(1.0 * MemoryUsage):0} B";
            else if (MemoryUsage < MEM_MB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_KB):0.00} KB";
            else// if (MemoryUsage < MEM_GB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_MB):0.00} MB";
            //else
            //    return $"{prefix}{(1.0 * MemoryUsage / MEM_GB):0.00} GB";
        }

        private Color GetMemoryUseColor(long MemoryUsage)
        {
            const double MEM_KB = 1024f;
            const double MEM_MB = 1024f * 1024f;
            const double MEM_GB = 1024f * 1024f * 1024f;

            if (MemoryUsage < MEM_KB)
                return Color.Cyan;
            else if (MemoryUsage < MEM_MB)
                return Color.Lime;
            else if (MemoryUsage < MEM_GB)
                return Color.Yellow;
            else if (MemoryUsage < (MEM_GB * 2))
                return Color.Orange;
            else
                return Color.Red;
        }

        private void DrawMemoryUsage()
        {
            var str_managed = GetMemoryUseString("CLR Mem:  ", MemoryUsage_Managed);
            var str_unmanaged = GetMemoryUseString("RAM USE:  ", MemoryUsage_Unmanaged);

            var strSize_managed = DBG.DEBUG_FONT_SMALL.MeasureString(str_managed);
            var strSize_unmanaged = DBG.DEBUG_FONT.MeasureString(str_unmanaged);

            //DBG.DrawOutlinedText(str_managed, new Vector2(GFX.Device.Viewport.Width - 2, 
            //    GFX.Device.Viewport.Height - (strSize_managed.Y * 0.75f) - (strSize_unmanaged.Y * 0.75f)),
            //    Color.Cyan, DBG.DEBUG_FONT_SMALL, scale: 0.75f, scaleOrigin: new Vector2(strSize_managed.X, 0));
            GFX.SpriteBatchBeginForText();
            DBG.DrawOutlinedText(str_unmanaged, new Vector2(GFX.Device.Viewport.Width - 6,
                GFX.Device.Viewport.Height),
                GetMemoryUseColor(MemoryUsage_Unmanaged), DBG.DEBUG_FONT, scale: 1, scaleOrigin: strSize_unmanaged);
            GFX.SpriteBatchEnd();
        }

        private void UpdateMemoryUsage()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                MemoryUsage_Unmanaged = proc.PrivateMemorySize64;
            }
            MemoryUsage_Managed = GC.GetTotalMemory(forceFullCollection: false);
        }

        /// <summary>Returns true if the current application has focus, false otherwise</summary>
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


        protected override void Update(GameTime gameTime)
        {
            DELTA_UPDATE = (float)gameTime.ElapsedGameTime.TotalSeconds;//(float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);

            if (!FIXED_TIME_STEP && GFX.AverageFPS >= 200)
            {
                DELTA_UPDATE_ROUNDED = (float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);
            }
            else
            {
                DELTA_UPDATE_ROUNDED = DELTA_UPDATE;
            }



            Active = IsActive && ApplicationIsActivated();

            TargetElapsedTime = Active ? TimeSpan.FromTicks(166667) : TimeSpan.FromSeconds(0.25);

            IsLoadingTaskRunning = LoadingTaskMan.AnyTasksRunning();

            Scene.UpdateAnimation();

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            LoadingTaskMan.Update(elapsed);

            IsFixedTimeStep = FIXED_TIME_STEP;

            if (DBG.EnableMenu)
            {
                DbgMenuItem.UpdateInput(elapsed);
                DbgMenuItem.UICursorBlinkUpdate(elapsed);
            }

            //if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
            //{
            //    // Only update input if debug menu isnt fully open.
            //    GFX.World.UpdateInput(this, gameTime);
            //}

            GFX.World.UpdateMatrices(GraphicsDevice);

            GFX.World.CameraPositionDefault.Position = Vector3.Zero;

            GFX.World.CameraOrigin.Position = new Vector3(GFX.World.CameraPositionDefault.Position.X,
                GFX.World.CameraOrigin.Position.Y, GFX.World.CameraPositionDefault.Position.Z);

            if (DBG.DbgPrim_Grid != null)
                DBG.DbgPrim_Grid.Transform = GFX.World.CameraPositionDefault;

            if (REQUEST_EXIT)
                Exit();

            MemoryUsageCheckTimer += elapsed;
            if (MemoryUsageCheckTimer >= MemoryUsageCheckInterval)
            {
                MemoryUsageCheckTimer = 0;
                UpdateMemoryUsage();
            }


            // BELOW IS TAE EDITOR STUFF

            if (IsLoadingTaskRunning != prevFrameWasLoadingTaskRunning)
            {
                TAE_EDITOR.GameWindowAsForm.Invoke(new Action(() =>
                {
                    if (IsLoadingTaskRunning)
                    {
                        Mouse.SetCursor(MouseCursor.Wait);
                    }

                    foreach (Control c in TAE_EDITOR.GameWindowAsForm.Controls)
                    {
                        c.Enabled = !IsLoadingTaskRunning;
                    }

                    if (!IsLoadingTaskRunning)
                    {
                        TAE_EDITOR.RefocusInspectorToPreventBeepWhenYouHitSpace();
                    }


                }));
            }

            if (!IsLoadingTaskRunning)
            {
                //MeasuredElapsedTime = UpdateStopwatch.Elapsed;
                //MeasuredTotalTime = MeasuredTotalTime.Add(MeasuredElapsedTime);

                //UpdateStopwatch.Restart();

                if (!TAE_EDITOR.Rect.Contains(TAE_EDITOR.Input.MousePositionPoint))
                    TAE_EDITOR.Input.CursorType = TaeEditor.MouseCursorType.Arrow;

                if (Active)
                    TAE_EDITOR.Update();
                else
                    TAE_EDITOR.Input.CursorType = TaeEditor.MouseCursorType.Arrow;

                if (!string.IsNullOrWhiteSpace(TAE_EDITOR.FileContainerName))
                    Window.Title = $"{System.IO.Path.GetFileName(TAE_EDITOR.FileContainerName)}" +
                        $"{(TAE_EDITOR.IsModified ? "*" : "")}" +
                        $"{(TAE_EDITOR.IsReadOnlyFileMode ? " !READ ONLY!" : "")}" +
                        $" - DS Anim Studio {VERSION}";
                else
                    Window.Title = $"DS Anim Studio {VERSION}";
            }

            prevFrameWasLoadingTaskRunning = IsLoadingTaskRunning;

            base.Update(gameTime);
        }

        private void InitTonemapShader()
        {

        }

        protected override void Draw(GameTime gameTime)
        {
            DELTA_DRAW = (float)gameTime.ElapsedGameTime.TotalSeconds;// (float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);

            GFX.Device.Clear(Color.DimGray);

            if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
            {
                // Only update input if debug menu isnt fully open.
                GFX.World.UpdateInput(this, 0.0166667f);
            }

            if (TAE_EDITOR.ModelViewerBounds.Width > 0 && TAE_EDITOR.ModelViewerBounds.Height > 0)
            {
                if (SceneRenderTarget == null)
                {
                    RebuildRenderTarget();
                    if (TimeBeforeNextRenderTargetUpdate > 0)
                        TimeBeforeNextRenderTargetUpdate -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
                else if (RequestViewportRenderTargetResolutionChange)
                {
                    RebuildRenderTarget();

                    if (TimeBeforeNextRenderTargetUpdate > 0)
                        TimeBeforeNextRenderTargetUpdate -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                GFX.Device.SetRenderTarget(SceneRenderTarget);

                GFX.Device.Clear(Color.DimGray);

                GFX.Device.Viewport = new Viewport(0, 0, SceneRenderTarget.Width, SceneRenderTarget.Height);

                GFX.LastViewport = new Viewport(TAE_EDITOR.ModelViewerBounds);

                //TaeInterop.TaeViewportDrawPre(gameTime);
                GFX.DrawScene3D();

                //if (!DBG.DbgPrimXRay)
                //    GFX.DrawSceneOver3D();

                if (DBG.DbgPrimXRay)
                    GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

                GFX.DrawSceneOver3D();

                GFX.Device.SetRenderTarget(null);

                GFX.Device.Clear(Color.DimGray);

                GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);

                InitTonemapShader();
                GFX.SpriteBatchBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                if (GFX.UseTonemap && !GFX.IsInDebugShadingMode)
                {
                    MainFlverTonemapShader.ScreenSize = new Vector2(
                        TAE_EDITOR.ModelViewerBounds.Width, 
                        TAE_EDITOR.ModelViewerBounds.Height);
                    MainFlverTonemapShader.Effect.CurrentTechnique.Passes[0].Apply();
                }

                GFX.SpriteBatch.Draw(SceneRenderTarget,
                    new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height), Color.White);
                GFX.SpriteBatchEnd();

                //try
                //{
                //    using (var renderTarget3DScene = new RenderTarget2D(GFX.Device, TAE_EDITOR.ModelViewerBounds.Width * GFX.SSAA,
                //   TAE_EDITOR.ModelViewerBounds.Height * GFX.SSAA, true, SurfaceFormat.Rgba1010102, DepthFormat.Depth24))
                //    {
                //        GFX.Device.SetRenderTarget(renderTarget3DScene);

                //        GFX.Device.Clear(new Color(80, 80, 80, 255));

                //        GFX.Device.Viewport = new Viewport(0, 0, TAE_EDITOR.ModelViewerBounds.Width * GFX.SSAA, TAE_EDITOR.ModelViewerBounds.Height * GFX.SSAA);
                //        TaeInterop.TaeViewportDrawPre(gameTime);
                //        GFX.DrawScene3D(gameTime);

                //        GFX.Device.SetRenderTarget(null);

                //        GFX.Device.Clear(new Color(80, 80, 80, 255));

                //        GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);

                //        InitTonemapShader();
                //        GFX.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                //        //MainFlverTonemapShader.Effect.CurrentTechnique.Passes[0].Apply();
                //        GFX.SpriteBatch.Draw(renderTarget3DScene,
                //            new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height), Color.White);
                //        GFX.SpriteBatch.End();
                //    }
                //}
                //catch (SharpDX.SharpDXException ex)
                //{
                //    GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);
                //    GFX.Device.Clear(new Color(80, 80, 80, 255));

                //    GFX.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                //    //MainFlverTonemapShader.Effect.CurrentTechnique.Passes[0].Apply();
                //    var errorStr = $"FAILED TO RENDER VIEWPORT AT {(Main.TAE_EDITOR.ModelViewerBounds.Width * GFX.SSAA)}x{(Main.TAE_EDITOR.ModelViewerBounds.Height * GFX.SSAA)} Resolution";
                //    var errorStrPos = (Vector2.One * new Vector2(TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height) / 2.0f);

                //    errorStrPos -= DBG.DEBUG_FONT.MeasureString(errorStr) / 2.0f;

                //    GFX.SpriteBatch.DrawString(DBG.DEBUG_FONT, errorStr, errorStrPos - Vector2.One, Color.Black);
                //    GFX.SpriteBatch.DrawString(DBG.DEBUG_FONT, errorStr, errorStrPos, Color.Red);
                //    GFX.SpriteBatch.End();
                //}

            }

            

            GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);
            //DBG.DrawPrimitiveNames(gameTime);

            

            //if (DBG.DbgPrimXRay)
            //    GFX.DrawSceneOver3D();

            GFX.DrawSceneGUI();

            

            TAE_EDITOR?.Graph?.ViewportInteractor?.DrawDebug();

            DrawMemoryUsage();

            LoadingTaskMan.DrawAllTasks();

            GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);

            TAE_EDITOR.Rect = new Rectangle(2, 0, GraphicsDevice.Viewport.Width - 4, GraphicsDevice.Viewport.Height - 2);

            TAE_EDITOR.Draw(GraphicsDevice, TaeEditorSpriteBatch,
                TAE_EDITOR_BLANK_TEX, TAE_EDITOR_FONT, 
                (float)gameTime.ElapsedGameTime.TotalSeconds, TAE_EDITOR_FONT_SMALL,
                TAE_EDITOR_SCROLLVIEWER_ARROW);

            if (IsLoadingTaskRunning)
            {
                TAE_EDITOR.DrawDimmingRect(GraphicsDevice, TaeEditorSpriteBatch, TAE_EDITOR_BLANK_TEX);
            }

            //GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);
            DrawImGui(gameTime, TAE_EDITOR.ModelViewerBounds.X, TAE_EDITOR.ModelViewerBounds.Y, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height);

            GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
        }
    }
}
