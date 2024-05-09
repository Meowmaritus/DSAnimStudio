using DSAnimStudio.DbgMenus;
using DSAnimStudio.GFXShaders;
using DSAnimStudio.ImguiOSD;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.DirectWrite;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        public const string VERSION = "4.9.9 [PUBLIC]";

        public static void GCCollect()
        {
            Task.Run(() =>
            {
                GC.Collect();
            });
        }

        public enum SplashScreenStates
        {
            FadeIn,
            Showing,
            FadeOut,
            Complete,
        }

        public class LazyDispatchAction
        {
            public Action Act;
            public int GhettoTimer = 3;
        }

        private static object _lock_LazyDispatch = new object();
        private static Queue<LazyDispatchAction> LazyDispatchActionQueue = new Queue<LazyDispatchAction>();
        private static List<Action> LazyDispatchASAPActionList = new List<Action>();
        public static void MainThreadLazyDispatch(Action act, int frameDelay = 8)
        {
            lock (_lock_LazyDispatch)
            {
                LazyDispatchActionQueue.Enqueue(new LazyDispatchAction() { Act = act, GhettoTimer = frameDelay });
            }
        }

        public static void MainThreadNextFrameDispatch(Action act)
        {
            lock (_lock_LazyDispatch)
            {
                LazyDispatchASAPActionList.Add(act);
            }
        }

        private static void UpdateLazyDispatch()
        {
            List<Action> asapActionListCopy = null;
            Action nextLazyAction = null;
            lock (_lock_LazyDispatch)
            {
                asapActionListCopy = LazyDispatchASAPActionList.ToList();
                // Clear while locked to prevent desync
                LazyDispatchASAPActionList.Clear();

                if (LazyDispatchActionQueue.Count > 0)
                {
                    var nextLazy = LazyDispatchActionQueue.Peek();

                    nextLazy.GhettoTimer--;

                    if (nextLazy.GhettoTimer <= 0)
                    {
                        nextLazyAction = LazyDispatchActionQueue.Dequeue().Act;
                    }
                }
                
            }

            foreach (var act in asapActionListCopy)
            {
                act?.Invoke();
            }

            if (nextLazyAction != null)
            {
                nextLazyAction?.Invoke();
            }
        }


        public static SplashScreenStates SplashState = SplashScreenStates.FadeIn;
        public static float SplashAlpha = 0;
        public static float SplashTimer = 0;
        public static float SplashFadeDuration = 0.5f;
        public static float SplashMinShowBeforeDismiss = 1;
        public static bool SplashDismissed = false;
        public static Texture2D SplashTex;
        public static string SplashText;
        public static SpriteFont SplashFont;

        public static void LoadSplash(ContentManager c)
        {
            SplashTex = c.Load<Texture2D>($@"{Main.Directory}\Content\Utility\Credits");
            SplashFont = c.Load<SpriteFont>($@"{Main.Directory}\Content\Fonts\CreditsFont");

            for (int i = 0; i < SplashFont.Glyphs.Length; i++)
            {
                //if (SplashFont.Glyphs[i].Character == 'l')
                //{
                //    SplashFont.Glyphs[i].BoundsInTexture.X -= 2;
                //    SplashFont.Glyphs[i].BoundsInTexture.Width += 2;
                //}
                //else if (SplashFont.Glyphs[i].Character == 'i')
                //{
                //    SplashFont.Glyphs[i].BoundsInTexture.X -= 2;
                //    SplashFont.Glyphs[i].BoundsInTexture.Width += 2;
                //}
                //else if (SplashFont.Glyphs[i].Character == 'w')
                //{
                //    SplashFont.Glyphs[i].BoundsInTexture.X += 1;

                //}
                //else if (SplashFont.Glyphs[i].Character == 'e')
                //{
                //    SplashFont.Glyphs[i].BoundsInTexture.X += 1;

                //}
                //else if (SplashFont.Glyphs[i].Character == 'g')
                //{
                //    SplashFont.Glyphs[i].BoundsInTexture.X -= 2;
                //    SplashFont.Glyphs[i].BoundsInTexture.Width += 2;

                //}



                //else if (SplashFont.Glyphs[i].Character == 'A')
                //{
                //    SplashFont.Glyphs[i].Cropping.X -= 2;
                //}
                //else if (SplashFont.Glyphs[i].Character == ' ')
                //{
                //    SplashFont.Glyphs[i].Width = 8;
                //}
                //SplashFont.Glyphs[i].Cropping.X -= 2;
                //SplashFont.Glyphs[i].Cropping.Width += 4;
                //SplashFont.Glyphs[i].BoundsInTexture.Width += 2;


                if (SplashFont.Glyphs[i].Character == ' ')
                {
                    SplashFont.Glyphs[i].Width = 8;
                }
            }


            SplashText = File.ReadAllText($@"{Main.Directory}\Credits.txt");
        }

        public static void DrawSplash(bool clear)
        {
            GFX.SpriteBatchBegin();
            if (clear)
                GFX.Device.Clear(Color.Black);

            var measureText = ImGuiDebugDrawer.MeasureString(SplashText, 20);

            float aspectRatio = (float)GFX.Device.Viewport.Bounds.Width / (float)GFX.Device.Viewport.Bounds.Height;

            Rectangle rect = GFX.Device.Viewport.Bounds;

            if (aspectRatio >= 16f / 9f)
            {
                float height = GFX.Device.Viewport.Bounds.Height; 
                float width = height * (16f / 9f);

                rect = new Rectangle((int)((GFX.Device.Viewport.Bounds.Width / 2f) - (width / 2f)), 0,
                (int)width, (int)height);
            }
            else
            {
                float width = GFX.Device.Viewport.Bounds.Width;
                float height = width * (9f / 16f);
                rect = new Rectangle(0, (int)((GFX.Device.Viewport.Bounds.Height / 2f) - (height / 2f)),
                (int)width, (int)height);
            }

            

            GFX.SpriteBatch.Draw(TAE_EDITOR_BLANK_TEX, GFX.Device.Viewport.Bounds, Color.Black * SplashAlpha);
            GFX.SpriteBatch.Draw(SplashTex, rect, Color.White * SplashAlpha);
            GFX.SpriteBatchEnd();

            //GFX.SpriteBatchBeginForText();// Matrix.CreateScale(2));
            //GFX.SpriteBatch.DrawString(SplashFont, SplashText, new Vector2(rect.X + 64, rect.Y + 64), Color.White * SplashAlpha);
            //GFX.SpriteBatchEnd();

            DBG.DrawOutlinedText(SplashText, new Vector2(64, rect.Center.Y - measureText.Y / 2f), Color.White * SplashAlpha);
        }

        public static void UpdateSplash(float deltaTime)
        {
            if (SplashState == SplashScreenStates.FadeIn)
            {
                SplashTimer += deltaTime;

                float timerRatio = (SplashTimer / SplashFadeDuration);
                SplashAlpha = timerRatio;

                if (SplashTimer >= SplashFadeDuration)
                {
                    SplashState = SplashScreenStates.Showing;
                    SplashTimer = 0;
                    SplashAlpha = 1;
                }
            }
            else if (SplashState == SplashScreenStates.Showing)
            {
                if (Active && Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Enter) || Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Space) ||
                    Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Escape)
                    || Input.LeftClickHeld)
                    SplashDismissed = true;

                SplashTimer += deltaTime;

                if (SplashDismissed && SplashTimer >= SplashMinShowBeforeDismiss)
                {
                    SplashState = SplashScreenStates.FadeOut;
                    SplashTimer = 0;
                    SplashAlpha = 1;
                }
            }
            else if (SplashState == SplashScreenStates.FadeOut)
            {
                SplashTimer += deltaTime;

                float timerRatio = (SplashTimer / SplashFadeDuration);
                SplashAlpha = 1 - timerRatio;

                if (SplashTimer >= SplashFadeDuration)
                {
                    SplashState = SplashScreenStates.Complete;
                    SplashTimer = 0;
                    SplashAlpha = 0;
                }
            }
        }

        public static void Invoke(Action doStuff)
        {
            WinForm.Invoke(doStuff);
        }

        //public static string ImGuiFontName => $@"{Directory}\Content\Fonts\NotoSansCJKjp-Medium.otf";
        //public static float ImGuiFontGlyphMinAdvanceX = 5.0f;
        //public static int ImGuiFontOversampleH = 5;
        //public static int ImGuiFontOversampleV = 5;
        //public static bool ImGuiFontPixelSnapH = true;
        //public static float ImGuiFontPixelSize = 18.0f;

        public static string ImGuiFontName => $@"{Directory}\Content\Fonts\NotoSansMonoCJKjp-Bold.otf";
        public static float ImGuiFontGlyphMinAdvanceX = 5.0f;
        public static int ImGuiFontOversampleH = 1;
        public static int ImGuiFontOversampleV = 1;
        public static bool ImGuiFontPixelSnapH = true;
        public static float ImGuiFontPixelSize = 19.0f;


        

        public static bool WindowShown = false;

        static object _lock_ChangeRenderTarget = new object();
        static System.Threading.Tasks.Task renderTargetBuildTask = null;

        public static int WindowResizeCommitTimer = 0;
        public static int WindowResizeCommitTimerMax = 20;
        public static bool HasUncommittedWindowResize => WindowResizeCommitTimer < WindowResizeCommitTimerMax;
        Rectangle boundsLastUpdatedFor;
        Rectangle prevWinBounds;

        public static void OnClosing()
        {
            SoundManager.DisposeAll();
            GameData.ClearAll();
        }

        public static DX11FlverRenderer TEST_DX11FLVER;

        void UpdateWindowResizeStable()
        {
            var bounds = Window.ClientBounds;

            var windowMinimized = bounds.Width < 1000 || bounds.Height < 500;

            void doActualUpdate()
            {
                if (IgnoreSizeChanges)
                    return;

                WindowResizeCommitTimer = WindowResizeCommitTimerMax;

                RequestHideOSD = RequestHideOSD_MAX;
                UpdateActiveState();

                TAE_EDITOR?.HandleWindowResize(lastActualBounds, bounds);

                LastBounds = bounds;
                lastActualBounds = bounds;

                boundsLastUpdatedFor = bounds;
            }

            if (!WindowShown)
            {

                if (!windowMinimized)
                {
                    WindowShown = true;
                    TAE_EDITOR.Rect = new Rectangle(0, 0, bounds.Width, bounds.Height - 2);
                    TAE_EDITOR.DefaultLayout();

                    doActualUpdate();

                    //ImguiOSD.DialogManager.DialogOK("Welcome", "Thank you for being a Patreon supporter.\n\n" +
                    //    "This build has only been tested on Elden Ring. Past games might be broken.\nAdditionally, most textures will not work.\n" +
                    //    "However, it runs better than old versions and has some bugfixes." +
                    //    "\n\nSpecial thanks: " +
                    //    "\n    Skyth (blueskythlikesclouds on GitHub) - Created Havoc library which allows this application to load Havok tagfiles natively without downgrading." +
                    //    "\n    lingsamuel - Added some rough but perfectly functional Havok 2018.1 support to Havoc for Elden Ring animation support." +
                    //    "\n\nPlease enjoy and report any bugs to my Discord server, the link to which can be found under 'Help' in the menu bar.");
                }
            }
            else
            {

                //Console.WriteLine($"bounds:[{bounds.X},{bounds.Y},{bounds.Width},{bounds.Height}]");

                if (prevWinBounds != bounds || windowMinimized)
                {
                    WindowResizeCommitTimer = 0;
                }

                if (WindowResizeCommitTimer >= WindowResizeCommitTimerMax)
                {
                    if (bounds != boundsLastUpdatedFor)
                    {
                        doActualUpdate();
                    }
                }
                else
                {
                    WindowResizeCommitTimer++;
                }

                prevWinBounds = bounds;
            }
        }

        public static T ReloadMonoGameContent<T>(string path, string origPath)
        {
            path = Path.GetFullPath(path);

            if (path.ToLower().EndsWith(".xnb"))
                path = path.Substring(0, path.Length - 4);
            MainContentManager.UnloadAsset(origPath);
            MainContentManager.UnloadAsset(origPath.Replace("\\", "/"));
            MainContentManager.UnloadAsset(path);
            MainContentManager.UnloadAsset(path.Replace("\\", "/"));
            return MainContentManager.Load<T>(path);
        }

        public static void CenterForm(Form form)
        {
            var x = Main.WinForm.Location.X + (Main.WinForm.Width - form.Width) / 2;
            var y = Main.WinForm.Location.Y + (Main.WinForm.Height - form.Height) / 2;
            form.Location = new System.Drawing.Point(Math.Max(x, 0), Math.Max(y, 0));
        }

        protected override void Dispose(bool disposing)
        {
            WindowsMouseHook.Unhook();

            RemoManager.DisposeAllModels();

            GameData.ClearAll();

            base.Dispose(disposing);
        }

        public static bool IgnoreSizeChanges = false;

        public static Rectangle LastBounds = Rectangle.Empty;
        private static Rectangle lastActualBounds = Rectangle.Empty;

        public static ColorConfig Colors = new ColorConfig();

        public static Form WinForm;

        public static float DPICustomMultX = 1;
        public static float DPICustomMultY = 1;

        private static float BaseDPIX = 1;
        private static float BaseDPIY = 1;

        public static float DPIX => BaseDPIX * DPICustomMultX;
        public static float DPIY => BaseDPIY * DPICustomMultY;

        public static FancyInputHandler Input;

        public const int ConfigFileIOMaxTries = 10;

        public static bool NeedsToLoadConfigFileForFirstTime { get; private set; } = true;

        public static bool DisableConfigFileAutoSave = false;

        private static object _lock_actualConfig = new object();
        private static TaeEditor.TaeConfigFile actualConfig = new TaeEditor.TaeConfigFile();
        public static TaeEditor.TaeConfigFile Config
        {
            get
            {
                TaeEditor.TaeConfigFile result = null;
                lock (_lock_actualConfig)
                {
                    result = actualConfig;
                }
                return result;
            }
            set
            {
                lock (_lock_actualConfig)
                {
                    actualConfig = value;
                }
            }
        }

        private static string ConfigFilePath = null;

        public const string ConfigFileShortName = "DSAnimStudio_Config.json";

        private static void CheckConfigFilePath()
        {
            if (ConfigFilePath == null)
            {
                var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentAssemblyDir = System.IO.Path.GetDirectoryName(currentAssemblyPath);
                ConfigFilePath = System.IO.Path.Combine(currentAssemblyDir, ConfigFileShortName);
            }
        }

        public static void LoadConfig(bool isManual = false)
        {
            if (!isManual && DisableConfigFileAutoSave)
                return;
            CheckConfigFilePath();
            if (!System.IO.File.Exists(ConfigFilePath))
            {
                Config = new TaeEditor.TaeConfigFile();
                SaveConfig();
            }
            string jsonText = null;
            int tryCounter = 0;

            while (jsonText == null)
            {
                bool giveUp = false;
                try
                {
                    if (tryCounter < ConfigFileIOMaxTries)
                    {
                        jsonText = System.IO.File.ReadAllText(ConfigFilePath);
                    }
                    else
                    {
                        var ask = System.Windows.Forms.MessageBox.Show($"Failed 10 times in a row to read configuration file '{ConfigFileShortName}' from the application folder. " +
                            "It may have been in use by another " +
                            "application (e.g. another instance of DS Anim Studio). " +
                            "\n\nWould you like to RETRY the configuration file reading operation or CANCEL, " +
                            "disabling configuration file autosaving to be safe?", "Configuration File IO Failure",
                            MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                        if (ask == DialogResult.Retry)
                        {
                            giveUp = false;
                            tryCounter = 0;
                        }
                        else
                        {
                            giveUp = true;
                        }
                    }
                }
                catch
                {
                    tryCounter++;
                }

                if (giveUp)
                {
                    DisableConfigFileAutoSave = true;
                    return;
                }
            }

            void jsonFailure()
            {
                var ask = System.Windows.Forms.MessageBox.Show($"Failed to parse configuration file '{ConfigFileShortName}' in the application folder. " +
                            "It may have been saved by an incompatible version of the application or corrupted. " +
                            "\n\nWould you like to overwrite it with default settings? " +
                            "\n\nIf not, configuration file autosaving will be disabled to keep the file as-is.", "Configuration File Parse Failure",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ask == DialogResult.Yes)
                {
                    Config = new TaeEditor.TaeConfigFile();
                    SaveConfig(isManual: true);
                }
                else
                {
                    DisableConfigFileAutoSave = true;
                }
            }

            try
            {
                Config = Newtonsoft.Json.JsonConvert.DeserializeObject<TaeEditor.TaeConfigFile>(jsonText);

                
            }
            catch (Newtonsoft.Json.JsonException)
            {
                jsonFailure();
            }

            if (Config == null)
            {
                jsonFailure();
            }

            Config.AfterLoading(TAE_EDITOR);

            if (NeedsToLoadConfigFileForFirstTime)
            {
                Config.AfterLoadingFirstTime(TAE_EDITOR);
            }

            NeedsToLoadConfigFileForFirstTime = false;
        }

        public static void SaveConfig(bool isManual = false)
        {

            if (!isManual && DisableConfigFileAutoSave)
                return;
            lock (Main.Config._lock_ThreadSensitiveStuff)
            {
                if (TAE_EDITOR?.Graph != null)
                {
                    // I'm sorry; this is pecularily placed.
                    TAE_EDITOR.Graph?.ViewportInteractor?.SaveChrAsm();
                }

                Config.BeforeSaving(TAE_EDITOR);
                CheckConfigFilePath();

                var jsonText = Newtonsoft.Json.JsonConvert
                    .SerializeObject(Config,
                    Newtonsoft.Json.Formatting.Indented);

                bool success = false;

                int tryCounter = 0;

                while (!success)
                {
                    bool giveUp = false;
                    try
                    {
                        if (tryCounter < ConfigFileIOMaxTries)
                        {
                            System.IO.File.WriteAllText(ConfigFilePath, jsonText);
                            success = true;
                        }
                        else
                        {
                            var ask = System.Windows.Forms.MessageBox.Show($"Failed 10 times in a row to write configuration file '{ConfigFileShortName}' in the application folder. " +
                                "It may have been in use by another " +
                                "application (e.g. another instance of DS Anim Studio). " +
                                "\n\nWould you like to RETRY the configuration file writing operation or CANCEL, " +
                                "disabling configuration file autosaving to be safe?", "Configuration File IO Failure",
                                MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);

                            if (ask == DialogResult.Retry)
                            {
                                giveUp = false;
                                tryCounter = 0;
                            }
                            else
                            {
                                giveUp = true;
                            }
                        }
                    }
                    catch
                    {
                        tryCounter++;
                    }

                    if (giveUp)
                    {
                        DisableConfigFileAutoSave = true;
                        return;
                    }
                }

            }
        }

        public static Vector2 DPIVector => new Vector2(DPIX, DPIY);
        public static System.Numerics.Vector2 DPIVectorN => new System.Numerics.Vector2(DPIX, DPIY);

        public static Matrix DPIMatrix => Matrix.CreateScale(DPIX, DPIY, 1);

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

        

        public static bool FIXED_TIME_STEP = false;

        public static bool REQUEST_EXIT = false;

        public static bool REQUEST_REINIT_EDITOR = false;

        public static bool REQUEST_DISABLE_SOUND = false;

        public static float DELTA_UPDATE_RATIO_VS_60HZ => DELTA_UPDATE / (1f / 60f);

        public static float DELTA_UPDATE;
        public static float DELTA_UPDATE_ROUNDED;
        public static float DELTA_DRAW;

        public static ImGuiRenderer ImGuiDraw;

        public static Vector2 GlobalTaeEditorFontOffset = new Vector2(0, -3);

        public static IServiceProvider MainContentServiceProvider = null;

        private bool prevFrameWasLoadingTaskRunning = false;
        public static bool Active { get; private set; }
        public static HysteresisBool ActiveHyst = new HysteresisBool(0, 5);
        public static bool prevActive { get; private set; }

        public static bool IsFirstUpdateLoop { get; private set; } = true;
        public static bool IsFirstFrameActive { get; private set; } = false;

        public static bool Minimized { get; private set; }

        public static bool DISABLE_DRAW_ERROR_HANDLE = true;

        private static float MemoryUsageCheckTimer = 0;
        private static long MemoryUsage_Unmanaged = 0;
        private static long MemoryUsage_Managed = 0;
        private const float MemoryUsageCheckInterval = 0.25f;

        public static readonly Color SELECTED_MESH_COLOR = Color.Yellow * 0.05f;
        //public static readonly Color SELECTED_MESH_WIREFRAME_COLOR = Color.Yellow;

        public static Texture2D WHITE_TEXTURE;
        public static Texture2D BLACK_TEXTURE;
        public static Texture2D GRAY_SRGB_TEXTURE;
        public static Texture2D DEFAULT_TEXTURE_DIFFUSE;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR_DS2;
        public static Texture2D DEFAULT_TEXTURE_NORMAL;
        public static Texture2D DEFAULT_TEXTURE_NORMAL_DS2;
        public static Texture2D DEFAULT_TEXTURE_MISSING;
        public static TextureCube DEFAULT_TEXTURE_MISSING_CUBE;
        public static Texture2D DEFAULT_TEXTURE_EMISSIVE;
        public static Texture2D DEFAULT_TEXTURE_METALLIC;
        public string DEFAULT_TEXTURE_MISSING_NAME => $@"{Main.Directory}\Content\Utility\MissingTexture";

        public static TaeEditor.TaeEditorScreen TAE_EDITOR;
        private static SpriteBatch TaeEditorSpriteBatch;
        public static Texture2D TAE_EDITOR_BLANK_TEX;
        public static SpriteFont TAE_EDITOR_FONT;
        public static SpriteFont TAE_EDITOR_FONT_SMALL;
        public static Texture2D TAE_EDITOR_SCROLLVIEWER_ARROW;

        public static FlverTonemapShader MainFlverTonemapShader = null;
        public static string FlverTonemapShader__Name => $@"{Main.Directory}\Content\Shaders\FlverTonemapShader";

        //public static Stopwatch UpdateStopwatch = new Stopwatch();
        //public static TimeSpan MeasuredTotalTime = TimeSpan.Zero;
        //public static TimeSpan MeasuredElapsedTime = TimeSpan.Zero;

        public bool IsLoadingTaskRunning = false;
        public HysteresisBool IsLoadingTaskRunningHyst = new HysteresisBool(0, 5);

        public static ContentManager MainContentManager = null;

        public static RenderTarget2D SceneRenderTarget = null;
        //public static RenderTarget2D UnusedRendertarget0 = null;
        public static int UnusedRenderTarget0Padding = 0;

        public static int RequestHideOSD = 0;
        public static int RequestHideOSD_MAX = 10;

        public static bool RequestViewportRenderTargetResolutionChange = false;
        private const float TimeBeforeNextRenderTargetUpdate_Max = 0.5f;
        private static float TimeBeforeNextRenderTargetUpdate = 0;

        public static ImFontPtr ImGuiFontPointer;

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

            if (GFX.MSAA > 0)
            {
                graphics.PreferMultiSampling = true;
                graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = GFX.MSAA;
            }
            else
            {
                graphics.PreferMultiSampling = false;
                graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 1;
            }

            //graphics.PreferMultiSampling = false;
            //graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 1;

            graphics.ApplyChanges();
        }



        //MCG MCGTEST_MCG;



        public Main()
        {
            

            WinForm = (Form)Form.FromHandle(Window.Handle);

            WinForm.KeyPreview = true;
            

            WindowsMouseHook.Hook(Window.Handle);
            WinForm.AutoScaleMode = AutoScaleMode.Dpi;

            WinForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);

            BaseDPIX = BaseDPIY = WinForm.DeviceDpi / 96f;
            WinForm.DpiChanged += WinForm_DpiChanged;

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
            graphics.PreferredBackBufferWidth = (int)Math.Round(GFX.Display.Width * DPIX);
            graphics.PreferredBackBufferHeight = (int)Math.Round(GFX.Display.Height * DPIY);
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

            WinForm.Shown += (o, e) =>
            {
                LoadConfig();
                FmodManager.InitTest();
            };

            this.Activated += Main_Activated;
            this.Deactivated += Main_Deactivated;

            GFX.Display.SetFromDisplayMode(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);


            Input = new FancyInputHandler();
            //GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        private void WinForm_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            UpdateDpiStuff();
        }

        public void UpdateDpiStuff()
        {
            float newDpi = WinForm.DeviceDpi / 96f;
            BaseDPIX = BaseDPIY = newDpi;

            RequestViewportRenderTargetResolutionChange = true;
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //        FmodManager.Shutdown();

        //    base.Dispose(disposing);
        //}

        private void Main_Deactivated(object sender, EventArgs e)
        {
            UpdateActiveState();
#if !DEBUG
            SoundManager.StopAllSounds();
#endif
        }

        private void Main_Activated(object sender, EventArgs e)
        {
            IsFirstFrameActive = true;
            UpdateActiveState();
        }

        

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            WindowResizeCommitTimer = 0;
        }

        public void RebuildRenderTarget(bool urgent)
        {
           

            void doRenderTargetBuild(bool useLock)
            {
                GFX.ClampAntialiasingOptions();

                int msaa = GFX.MSAA;
                int ssaa = GFX.SSAA;

                //SceneRenderTarget?.Dispose();
                //GC.Collect();

                var newSceneRenderTarget = new RenderTarget2D(GFX.Device, TAE_EDITOR.ModelViewerBounds.DpiScaled().Width * ssaa,
                       TAE_EDITOR.ModelViewerBounds.DpiScaled().Height * ssaa, ssaa > 1, SurfaceFormat.Vector4, DepthFormat.Depth24,
                       ssaa > 1 ? 1 : msaa, RenderTargetUsage.DiscardContents);

                RenderTarget2D oldSceneRenderTarget = null;
                if (useLock)
                {
                    lock (_lock_ChangeRenderTarget)
                    {
                        oldSceneRenderTarget = SceneRenderTarget;
                        SceneRenderTarget = newSceneRenderTarget;
                    }
                }
                else
                {
                    oldSceneRenderTarget = SceneRenderTarget;
                    SceneRenderTarget = newSceneRenderTarget;
                }

                oldSceneRenderTarget?.Dispose();


                TimeBeforeNextRenderTargetUpdate = TimeBeforeNextRenderTargetUpdate_Max;

                RequestViewportRenderTargetResolutionChange = false;

                GFX.EffectiveSSAA = ssaa;
                GFX.EffectiveMSAA = msaa;
            }

            if (urgent)
            {
                renderTargetBuildTask = null;
                doRenderTargetBuild(useLock: false);
            }
            else
            {
                if (TimeBeforeNextRenderTargetUpdate > 0 || renderTargetBuildTask?.Status == System.Threading.Tasks.TaskStatus.Running)
                    return;

                renderTargetBuildTask = System.Threading.Tasks.Task.Run(() =>
                {
                    doRenderTargetBuild(useLock: true);
                });
            }
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            CFG.Save();

            Main.SaveConfig();

            DSAnimStudio.LiveRefresh.Memory.CloseHandle();

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
                DEFAULT_TEXTURE_DIFFUSE.SetData(new Color[] { new Color(1f, 1f, 1f) });

                WHITE_TEXTURE = new Texture2D(GraphicsDevice, 1, 1);
                WHITE_TEXTURE.SetData(new Color[] { new Color(1.0f, 1.0f, 1.0f) });

                BLACK_TEXTURE = new Texture2D(GraphicsDevice, 1, 1);
                BLACK_TEXTURE.SetData(new Color[] { new Color(0.0f, 0.0f, 0.0f) });

                GRAY_SRGB_TEXTURE = new Texture2D(GraphicsDevice, 1, 1);
                GRAY_SRGB_TEXTURE.SetData(new Color[] { new Color(0.5f * 0.5f, 0.5f * 0.5f, 0.5f * 0.5f) });

                DEFAULT_TEXTURE_SPECULAR = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_SPECULAR.SetData(new Color[] { new Color(0f, 0f, 0f) });

                DEFAULT_TEXTURE_SPECULAR_DS2 = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_SPECULAR_DS2.SetData(new Color[] { new Color(0.5f, 0.5f, 0.5f) });

                DEFAULT_TEXTURE_NORMAL = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_NORMAL.SetData(new Color[] { new Color(0.5f, 0.5f, 0.0f) });

                DEFAULT_TEXTURE_NORMAL_DS2 = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_NORMAL_DS2.SetData(new Color[] { new Color(0.5f, 0.5f, 0.5f, 0.5f) });

                DEFAULT_TEXTURE_EMISSIVE = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_EMISSIVE.SetData(new Color[] { Color.Black });

                DEFAULT_TEXTURE_METALLIC = new Texture2D(GraphicsDevice, 1, 1);
                DEFAULT_TEXTURE_METALLIC.SetData(new Color[] { new Color(0.5f, 0f, 1f) });

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

        

        private static Microsoft.Xna.Framework.Vector3 currModelAddOffset = Microsoft.Xna.Framework.Vector3.Zero;
        private void GameWindowForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] modelFiles = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            //TAE_EDITOR.

            void LoadOneFile(string file)
            {
                if (file.ToUpper().EndsWith(".FLVER") || file.ToUpper().EndsWith(".FLVER.DCX"))
                {
                    if (FLVER2.Is(file))
                    {
                        //currModelAddOffset.X += 3;
                        var m = new Model(FLVER2.Read(file), false);
                        m.StartTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                        Scene.ClearSceneAndAddModel(m);
                    }
                    else if (FLVER0.Is(file))
                    {
                        //currModelAddOffset.X += 3;
                        var m = new Model(FLVER0.Read(file), false);
                        m.StartTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                        Scene.ClearSceneAndAddModel(m);
                    }
                }
                else if (file.ToUpper().EndsWith(".CHRBND") || file.ToUpper().EndsWith(".CHRBND.DCX"))
                {
                    Scene.ClearScene();
                    //currModelAddOffset.X += 3;
                    GameRoot.InitializeFromBND(file);
                    var m = GameRoot.LoadCharacter(Utils.GetShortIngameFileName(file), null);
                    m.StartTransform = m.CurrentTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                    m.AnimContainer?.ChangeToNewAnimation(m.AnimContainer.Animations.Keys.FirstOrDefault(), animWeight: 1, startTime: 0, clearOldLayers: true);
                    m.AnimContainer.ForcePlayAnim = true;
                    m.UpdateAnimation();
                    //Scene.ClearSceneAndAddModel(m);
                }
                else if (file.ToUpper().EndsWith(".OBJBND") || file.ToUpper().EndsWith(".OBJBND.DCX"))
                {
                    Scene.ClearScene();
                    //currModelAddOffset.X += 3;
                    GameRoot.InitializeFromBND(file);
                    var m = GameRoot.LoadObject(Utils.GetShortIngameFileName(file));
                    m.StartTransform = m.CurrentTransform = new Transform(currModelAddOffset, Microsoft.Xna.Framework.Quaternion.Identity);
                    m.AnimContainer?.ChangeToNewAnimation(m.AnimContainer.Animations.Keys.FirstOrDefault(), animWeight: 1, startTime: 0, clearOldLayers: true);
                    m.AnimContainer.ForcePlayAnim = true;
                    m.UpdateAnimation();
                    //Scene.ClearSceneAndAddModel(m);
                }
                //else if (file.ToUpper().EndsWith(".HKX"))
                //{
                //    var anim = KeyboardInput.Show("Enter Anim ID", "Enter name to save the dragged and dropped HKX file to e.g. a01_3000.");
                //    string name = anim.Result;
                //    byte[] animData = File.ReadAllBytes(file);
                //    TAE_EDITOR.FileContainer.AddNewHKX(name, animData, out byte[] dataForAnimContainer);
                //    if (dataForAnimContainer != null)
                //        TAE_EDITOR.Graph.ViewportInteractor.CurrentModel.AnimContainer.AddNewHKXToLoad(name + ".hkx", dataForAnimContainer);
                //    else
                //        DialogManager.DialogOK("Failed", "Failed to save (TagTools refused to work), just try again.");
                //}
            }

            if (modelFiles.Length == 1)
            {
                string f = modelFiles[0].ToLower();

                if (f.EndsWith(".fbx"))
                {
                    TAE_EDITOR.Config.LastUsedImportConfig_FLVER2.AssetPath = modelFiles[0];
                    TAE_EDITOR.BringUpImporter_FLVER2();
                    TAE_EDITOR.Config.LastUsedImportConfig_FLVER2.AssetPath = modelFiles[0];
                    TAE_EDITOR.ImporterWindow_FLVER2.LoadValuesFromConfig();
                }
                else
                {
                    LoadOneFile(modelFiles[0]);
                }
            }
            else
            {

                LoadingTaskMan.DoLoadingTask("LoadingDroppedModel", "Loading dropped model(s)...", prog =>
                {
                    foreach (var file in modelFiles)
                    {
                        LoadOneFile(file);
                    }

                }, disableProgressBarByDefault: true);
            }

            //LoadDragDroppedFiles(modelFiles.ToDictionary(f => f, f => File.ReadAllBytes(f)));
        }

        static bool IsValidDragDropModelFile(string f)
        {
            return (/*f.ToUpper().EndsWith(".HKX") || */BND3.Is(f) || BND4.Is(f) || FLVER2.Is(f));
        }

        private void GameWindowForm_DragEnter(object sender, DragEventArgs e)
        {
            IsFirstFrameActive = true;

            bool isValid = false;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) && !LoadingTaskMan.AnyTasksRunning())
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                

                if (files.Length == 1)
                {
                    string f = files[0].ToLower();

                    if (f.EndsWith(".fbx"))
                    {
                        isValid = Scene.IsModelLoaded;
                    }
                    else if (f.EndsWith(".flver.dcx") || f.EndsWith(".flver") || f.EndsWith(".chrbnd") || f.EndsWith(".chrbnd.dcx") || f.EndsWith(".objbnd") || f.EndsWith(".objbnd.dcx"))
                    {
                        isValid = true;
                    }
                    else if (f.EndsWith(".hkx"))
                    {
                        isValid = true;
                    }
                }
                // If multiple files are dragged they must all be regularly 
                // loadable rather than the specific case ones above
                else if (files.All(f => IsValidDragDropModelFile(f)))
                    isValid = true;


            }

            e.Effect = isValid ? DragDropEffects.Link : DragDropEffects.None;
        }

        public static Rectangle GetTaeEditorRect()
        {
            return new Rectangle(0, 0, Program.MainInstance.boundsLastUpdatedFor.Width, Program.MainInstance.boundsLastUpdatedFor.Height - 2).InverseDpiScaled();
        }

        public static void RESET_ALL()
        {
            TAE_EDITOR.CleanupForReinit();
            TAE_EDITOR = new TaeEditor.TaeEditorScreen((Form)Form.FromHandle(Program.MainInstance.Window.Handle), GetTaeEditorRect());
            Scene.ClearScene();
        }

        protected override void LoadContent()
        {
            

            MainContentServiceProvider = Content.ServiceProvider;
            MainContentManager = Content;

            GFX.Init(Content);
            DBG.LoadContent(Content);
            //InterrootLoader.OnLoadError += InterrootLoader_OnLoadError;

            DBG.CreateDebugPrimitives();

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

            TAE_EDITOR = new TaeEditor.TaeEditorScreen((Form)Form.FromHandle(Window.Handle), GetTaeEditorRect());

            TaeEditorSpriteBatch = new SpriteBatch(GFX.Device);

            if (Program.ARGS.Length > 0)
            {
                TAE_EDITOR.FileContainerName = Program.ARGS[0];

                if (Program.ARGS.Length > 1)
                    TAE_EDITOR.FileContainerName_Model = Program.ARGS[1];

                LoadingTaskMan.DoLoadingTask("ProgramArgsLoad", "Loading ANIBND and associated model(s)...", progress =>
                {
                    TAE_EDITOR.LoadCurrentFile();
                }, disableProgressBarByDefault: true);

                //LoadDragDroppedFiles(Program.ARGS.ToDictionary(f => f, f => File.ReadAllBytes(f)));
            }

            MainFlverTonemapShader = new FlverTonemapShader(Content.Load<Effect>(FlverTonemapShader__Name));

            BuildImguiFonts();

            TAE_EDITOR.LoadContent(Content);

            UpdateDpiStuff();

            LoadSplash(Content);
        }

        private static void BuildImguiFonts()
        {
            var fonts = ImGuiNET.ImGui.GetIO().Fonts;
            //var fontFile = File.ReadAllBytes($@"{Directory}\Content\Fonts\NotoSansCJKjp-Medium.otf");
            var fontFile = File.ReadAllBytes(ImGuiFontName);
            fonts.Clear();
            unsafe
            {
                fixed (byte* p = fontFile)
                {
                    ImVector ranges;
                    ImFontGlyphRangesBuilder* rawPtr = ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();
                    var builder = new ImFontGlyphRangesBuilderPtr(rawPtr);
                    var ccm = CCM.Read($@"{Directory}\Content\Fonts\dbgfont14h_ds3.ccm");
                    foreach (var g in ccm.Glyphs)
                        builder.AddChar((ushort)g.Key);

                    builder.BuildRanges(out ranges);
                    var ptr = ImGuiNET.ImGuiNative.ImFontConfig_ImFontConfig();
                    var cfg = new ImGuiNET.ImFontConfigPtr(ptr);
                    cfg.GlyphMinAdvanceX = ImGuiFontGlyphMinAdvanceX;
                    cfg.OversampleH = ImGuiFontOversampleH;
                    cfg.OversampleV = ImGuiFontOversampleV;
                    cfg.PixelSnapH = ImGuiFontPixelSnapH;
                    ImGuiFontPointer = fonts.AddFontFromMemoryTTF((IntPtr)p, fontFile.Length, ImGuiFontPixelSize, cfg, ranges.Data);
                }
            }
            fonts.Build();
            ImGuiDraw.RebuildFontAtlas();
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

        private Color GetMemoryUseColor(long MemoryUsage, bool audio = false)
        {
            //const double MEM_KB = 1024f;
            //const double MEM_MB = 1024f * 1024f;
            const double MEM_GB = 1024f * 1024f * 1024f;

            if (MemoryUsage < MEM_GB / (audio ? 4 : 1))
                return Colors.GuiColorMemoryUseTextGood;
            else if (MemoryUsage < (MEM_GB * 2) / (audio ? 4 : 1))
                return Colors.GuiColorMemoryUseTextOkay;
            else
                return Colors.GuiColorMemoryUseTextBad;
        }

        private void DrawMemoryUsage()
        {
            float scale = Config.ViewportMemoryTextSize / 100f;
            if (scale <= 0)
                return;
            
            var str_managed = GetMemoryUseString("CLR Mem:  ", MemoryUsage_Managed);
            var str_unmanaged = GetMemoryUseString("RAM USE:  ", MemoryUsage_Unmanaged);

            //var strSize_managed = DBG.DEBUG_FONT_SMALL.MeasureString(str_managed);
            //var strSize_unmanaged = DBG.DEBUG_FONT_SMALL.MeasureString(str_unmanaged);

            var strSize_managed = ImGuiDebugDrawer.MeasureString(str_managed, 16 * scale);
            var strSize_unmanaged = ImGuiDebugDrawer.MeasureString(str_unmanaged, 16 * scale);

            GFX.SpriteBatchBeginForText();

            if (GameRoot.GameTypeUsesWwise)
            {
                var wwinfo = Wwise.GetMemoryInfo();
                var str_wwise = $"{(wwinfo.AnySoundsLoading ? "[LOADING SOUNDS...]\n" : "")}Sounds Loaded: {wwinfo.SoundFileCount}\n" + GetMemoryUseString("Sound Memory:  ", wwinfo.ByteCount);
                //var strSize_wwise = DBG.DEBUG_FONT_SMALL.MeasureString(str_wwise);
                var strSize_wwise = ImGuiDebugDrawer.MeasureString(str_wwise, 16 * scale);
                //DBG.DrawOutlinedText(str_wwise, new Vector2(GFX.Device.Viewport.Width - 6,
                //GFX.Device.Viewport.Height - 40) / DPIVector,
                //GetMemoryUseColor(wwinfo.ByteCount, audio: true), DBG.DEBUG_FONT_SMALL, scale: 1, scaleOrigin: strSize_wwise);

                ImGuiDebugDrawer.DrawText(str_wwise, (new Vector2(GFX.Device.Viewport.Width - 6,
                GFX.Device.Viewport.Height - 32 * scale) / DPIVector) - strSize_wwise, GetMemoryUseColor(wwinfo.ByteCount, audio: true), 
                Color.Black, 16 * scale);
            }

            //DBG.DrawOutlinedText(str_unmanaged, new Vector2(GFX.Device.Viewport.Width - 6,
            //    GFX.Device.Viewport.Height - 16) / DPIVector,
            //    GetMemoryUseColor(MemoryUsage_Unmanaged), DBG.DEBUG_FONT_SMALL, scale: 1, scaleOrigin: strSize_unmanaged);
            //DBG.DrawOutlinedText(str_managed, new Vector2(GFX.Device.Viewport.Width - 6,
            //    GFX.Device.Viewport.Height) / DPIVector,
            //    GetMemoryUseColor(MemoryUsage_Managed), DBG.DEBUG_FONT_SMALL, scale: 1, scaleOrigin: strSize_managed);

            ImGuiDebugDrawer.DrawText(str_unmanaged, (new Vector2(GFX.Device.Viewport.Width - 6,
                GFX.Device.Viewport.Height - 16 * scale) / DPIVector) - strSize_unmanaged, GetMemoryUseColor(MemoryUsage_Unmanaged),
                Color.Black, 16 * scale);

            ImGuiDebugDrawer.DrawText(str_managed, (new Vector2(GFX.Device.Viewport.Width - 6,
                GFX.Device.Viewport.Height) / DPIVector) - strSize_managed, GetMemoryUseColor(MemoryUsage_Managed),
                Color.Black, 16 * scale);

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

        private void UpdateActiveState(bool? forceActive = null)
        {
            Minimized = !(Window.ClientBounds.Width > 0 && Window.ClientBounds.Height > 0);

            Active = forceActive ?? (!Minimized && IsActive && ApplicationIsActivated());

            ActiveHyst.Update(Active);

            TargetElapsedTime = (ActiveHyst || LoadingTaskMan.AnyTasksRunning()) ? TimeSpan.FromTicks(166667) : TimeSpan.FromSeconds(0.25);

            if (!prevActive && Active)
            {
                IsFirstFrameActive = true;
            }

            prevActive = Active;
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
            UpdateWindowResizeStable();

            if (SplashState != SplashScreenStates.Complete)
            {
                UpdateActiveState(true);
                GlobalInputState.Update();
                Input.Update(GFX.Device.Viewport.Bounds);
                DELTA_UPDATE = (float)gameTime.ElapsedGameTime.TotalSeconds;
                
                UpdateSplash(DELTA_UPDATE);
            }

            if (SplashState is SplashScreenStates.FadeIn or SplashScreenStates.Showing)
                return;

            if (REQUEST_REINIT_EDITOR)
            {
                REQUEST_REINIT_EDITOR = false;
                RESET_ALL();
            }

            if (REQUEST_DISABLE_SOUND)
            {
                Main.Config.SetEventSimulationEnabled("EventSimSE", false);
                SoundManager.DisposeAll();
                REQUEST_DISABLE_SOUND = false;
            }

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            NotificationManager.UpdateAll(elapsed);

            

            IsLoadingTaskRunning = LoadingTaskMan.AnyTasksRunning();
            IsLoadingTaskRunningHyst.Update(IsLoadingTaskRunning);

#if !DEBUG
            try
            {
#endif
            bool isMainFormFocused = false;
            if (WinForm.ContainsFocus)
            {
                isMainFormFocused = true;
            }

            foreach (System.Windows.Forms.Form form in Application.OpenForms)
            {
                if (form.Modal)
                {
                    UpdateActiveState();
                    GlobalInputState.Update();
                    try
                    {
                        form.Invoke(new Action(() =>
                        {
                            try
                            {
                                if (isMainFormFocused && Active && GlobalInputState.Mouse.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
                                {
                                    //UpdateActiveState(false);
                                    form.Activate();
                                }
                            }
                            catch
                            {

                            }
                        }));
                    }
                    catch
                    {

                    }

                    return;
                }
            }

            UpdateActiveState();

                if (ActiveHyst || LoadingTaskMan.AnyTasksRunning())
                {
                    GlobalInputState.Update();

                    UpdateLazyDispatch();

                FlverMaterialDefInfo.UpdateMem();

                    DELTA_UPDATE = (float)gameTime.ElapsedGameTime.TotalSeconds;//(float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);

                    //GFX.FlverDitherTime += DELTA_UPDATE;
                    //GFX.FlverDitherTime = GFX.FlverDitherTime % GFX.FlverDitherTimeMod;

                    if (!FIXED_TIME_STEP && GFX.AverageFPS >= 200)
                    {
                        DELTA_UPDATE_ROUNDED = (float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);
                    }
                    else
                    {
                        DELTA_UPDATE_ROUNDED = DELTA_UPDATE;
                    }

                    if (!LoadingTaskMan.AnyTasksRunning())
                        Scene.UpdateAnimation();

                

                    

                    
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


                        }));

                        // Undo an infinite loading cursor on an aborted file load.
                        if (!IsLoadingTaskRunning)
                        {
                            Mouse.SetCursor(MouseCursor.Arrow);
                        }
                    }

                    if (!IsLoadingTaskRunning)
                    {
                        //MeasuredElapsedTime = UpdateStopwatch.Elapsed;
                        //MeasuredTotalTime = MeasuredTotalTime.Add(MeasuredElapsedTime);

                        //UpdateStopwatch.Restart();

                        if (!TAE_EDITOR.Rect.Contains(TAE_EDITOR.Input.MousePositionPoint))
                            TAE_EDITOR.Input.CursorType = MouseCursorType.Arrow;

                        
                        
                        if (Active)
                        {
                            if (Scene.CheckIfDrawing())
                                TAE_EDITOR.Update();
                        }
                        else
                        {
                            TAE_EDITOR.Input.CursorType = MouseCursorType.Arrow;
                        }
                        

                       


                }

                if (!string.IsNullOrWhiteSpace(TAE_EDITOR.FileContainerName))
                    Window.Title = $"{System.IO.Path.GetFileName(TAE_EDITOR.FileContainerName)}" +
                        $"{(TAE_EDITOR.IsModified ? "*" : "")}" +
                        $"{(TAE_EDITOR.IsReadOnlyFileMode ? " !READ ONLY!" : "")}" +
                        $" - DS ANIM STUDIO {VERSION}";
                else
                    Window.Title = $"DS ANIM STUDIO {VERSION}";
#if NIGHTFALL
                Window.Title += " [NIGHTFALL MOD DEV VER]";
#endif

                if (IsLoadingTaskRunning)
                {
                    Window.Title = "[Loading...] " + Window.Title;
                }

                prevFrameWasLoadingTaskRunning = IsLoadingTaskRunning;

                    IsFirstFrameActive = false;

                    GFX.CurrentWorldView.Update(DELTA_UPDATE);

                    SoundManager.Update(DELTA_UPDATE, GFX.CurrentWorldView.CameraLocationInWorld.WorldMatrix, TAE_EDITOR);

                    TAE_EDITOR?.Graph?.AllBoxesEveryFrameUpdate();

                    base.Update(gameTime);
                }
#if !DEBUG
            }
            catch (Exception ex)
            {
                if (!ErrorLog.HandleException(ex, "Fatal error encountered during update loop"))
                {
                    WinForm.Close();
                }
            }
#endif
            IsFirstUpdateLoop = false;
        }

        private void InitTonemapShader()
        {

        }

        protected override void Draw(GameTime gameTime)
        {
            

            lock (_lock_ChangeRenderTarget)
            {



#if !DEBUG
            try
            {
#endif
                if (SplashState is SplashScreenStates.FadeIn or SplashScreenStates.Showing)
                {
                    GFX.Device.Clear(Color.Black);
                    
                    ImGuiDraw.BeforeLayout(gameTime, 0, 0, boundsLastUpdatedFor.Width, boundsLastUpdatedFor.Height, 0);
                    ImGuiDebugDrawer.Begin();
                    ImGuiDebugDrawer.ViewportOffset = Vector2.Zero;
                    
                    DrawSplash(clear: false);
                    GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
                    ImGuiDebugDrawer.DrawTtest();
                    NotificationManager.DrawAll();
                    ImGuiDebugDrawer.End();
                    ImGuiDraw.AfterLayout(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height, 0);
                    return;
                }

                if ((ActiveHyst || IsLoadingTaskRunningHyst) || LoadingTaskMan.AnyTasksRunning())
                {
                    Input.Update(new Rectangle(0, 0, boundsLastUpdatedFor.Width, boundsLastUpdatedFor.Height).InverseDpiScaled());

                    Colors.ReadColorsFromConfig();

                    DELTA_DRAW = (float)gameTime.ElapsedGameTime.TotalSeconds;// (float)(Math.Max(gameTime.ElapsedGameTime.TotalMilliseconds, 10) / 1000.0);

                    GFX.Device.Clear(Colors.MainColorBackground);

                    ImGuiDraw.BeforeLayout(gameTime, 0, 0, boundsLastUpdatedFor.Width, boundsLastUpdatedFor.Height, 0);

                    OSD.Build(Main.DELTA_DRAW, 0, 0);
                    ImGuiDebugDrawer.Begin();


                    if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
                    {
                        // Only update input if debug menu isnt fully open.
                        GFX.CurrentWorldView.UpdateInput();
                    }

                    if (TAE_EDITOR.ModelViewerBounds.Width > 0 && TAE_EDITOR.ModelViewerBounds.Height > 0)
                    {
                        //TEST_DX11FLVER?.Draw(GFX.World.Matrix_World, GFX.World.Matrix_View, GFX.World.Matrix_Projection);
                        //ImGuiDebugDrawer.DrawTtest();
                        //ImGuiDebugDrawer.End();
                        //ImGuiDraw.AfterLayout(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height, 0);
                        //return;

                        if (SceneRenderTarget == null)
                        {
                            RebuildRenderTarget(true);
                            if (TimeBeforeNextRenderTargetUpdate > 0)
                                TimeBeforeNextRenderTargetUpdate -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        else if (RequestViewportRenderTargetResolutionChange)
                        {
                            RebuildRenderTarget(false);

                            if (TimeBeforeNextRenderTargetUpdate > 0)
                                TimeBeforeNextRenderTargetUpdate -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        }


                        //GFX.Device.SetRenderTarget(UnusedRendertarget0);

                        //GFX.Device.Clear(Colors.MainColorViewportBackground);

                        //GFX.Device.Viewport = new Viewport(0, 0, UnusedRendertarget0.Width, UnusedRendertarget0.Height);

                        //GFX.LastViewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());

                        //GFX.BeginDraw();
                        ////GFX.InitDepthStencil(writeDepth: false);
                        //DBG.DrawSkybox();

                        //GFX.Device.SetRenderTarget(null);


                        //GFX.Bokeh.Draw(SkyboxRenderTarget, GFX.BokehShapeHexagon, GFX.BokehRenderTarget,
                        //    GFX.BokehBrightness, GFX.BokehSize, GFX.BokehDownsize, GFX.BokehIsFullPrecision, GFX.BokehIsDynamicDownsize);

                        GFX.Device.SetRenderTarget(null);

                        //GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());






                        //GFX.Device.SetRenderTarget(SceneRenderTarget);
                        GFX.Device.SetRenderTargets(SceneRenderTarget);

                        GFX.Device.Clear(Colors.MainColorViewportBackground);

                        GFX.Device.Viewport = new Viewport(0, 0, SceneRenderTarget.Width, SceneRenderTarget.Height);

                        GFX.LastViewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());

                        //GFX.SpriteBatchBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                        //GFX.SpriteBatch.Draw(SkyboxRenderTarget,
                        //new Rectangle(-SkyboxRenderTargetPadding, -SkyboxRenderTargetPadding,
                        //(TAE_EDITOR.ModelViewerBounds.Width + (SkyboxRenderTargetPadding * 2)) * GFX.EffectiveSSAA,
                        //(TAE_EDITOR.ModelViewerBounds.Height + (SkyboxRenderTargetPadding * 2)) * GFX.EffectiveSSAA), Color.White);
                        //GFX.SpriteBatchEnd();

                        GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);
                        //GFX.Device.Clear(ClearOptions.Stencil, Color.Transparent, 1, 0);
                        GFX.BeginDraw();
                        DBG.DrawSkybox();
                        //TaeInterop.TaeViewportDrawPre(gameTime);
                        GFX.DrawScene3D();

                        



                        //if (!DBG.DbgPrimXRay)
                        //    GFX.DrawSceneOver3D();

                        GFX.DrawPrimRespectDepth();

                        if (DBG.DbgPrimXRay)
                            GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

                        ImGuiDebugDrawer.ViewportOffset = TAE_EDITOR.ModelViewerBounds.DpiScaled().TopLeftCorner();

                        TAE_EDITOR?.Graph?.ViewportInteractor?.GeneralUpdate_BeforePrimsDraw();

                        GFX.DrawSceneOver3D();
                        ImGuiDebugDrawer.ViewportOffset = Vector2.Zero;

                        GFX.Device.Clear(ClearOptions.DepthBuffer, Color.Transparent, 1, 0);

                        GFX.DrawPrimDisrespectDepth();

                        //GFX.Device.SetRenderTarget(null);
                        GFX.Device.SetRenderTargets();

                        GFX.Device.Clear(Colors.MainColorBackground);

                        GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());

                        InitTonemapShader();
                        GFX.SpriteBatchBegin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

                        if (GFX.UseTonemap)
                        {
                            MainFlverTonemapShader.ScreenSize = new Vector2(
                                TAE_EDITOR.ModelViewerBounds.Width * Main.DPIX,
                                TAE_EDITOR.ModelViewerBounds.Height * Main.DPIY);
                            MainFlverTonemapShader.Effect.CurrentTechnique.Passes[0].Apply();
                        }



                        GFX.SpriteBatch.Draw(SceneRenderTarget,
                                new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width, TAE_EDITOR.ModelViewerBounds.Height), Color.White);

                        if (RemoEventSim.CurrentFadeColor.HasValue)
                        {
                            GFX.SpriteBatchEnd();

                            GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());
                            GFX.SpriteBatchBegin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);

                            GFX.SpriteBatch.Draw(TAE_EDITOR_BLANK_TEX,
                                    new Rectangle(0, 0, TAE_EDITOR.ModelViewerBounds.Width,
                                    TAE_EDITOR.ModelViewerBounds.Height), RemoEventSim.CurrentFadeColor.Value);
                        }

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



                    GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds.DpiScaled());
                    //DBG.DrawPrimitiveNames(gameTime);



                    //if (DBG.DbgPrimXRay)
                    //    GFX.DrawSceneOver3D();

                    GFX.DrawSceneGUI();


                    if (Config.ShowStatusInViewport is TaeEditor.TaeConfigFile.ViewportStatusTypes.Full 
                        or TaeEditor.TaeConfigFile.ViewportStatusTypes.Condensed)
                        TAE_EDITOR?.Graph?.ViewportInteractor?.DrawStatusInViewport(Config.ShowStatusInViewport);

                    DrawMemoryUsage();

                    LoadingTaskMan.DrawAllTasks();
                    


                    GFX.Device.Viewport = new Viewport(0, 0, (int)Math.Ceiling(Window.ClientBounds.Width * 1f), (int)Math.Ceiling(Window.ClientBounds.Height * 1f));

                    TAE_EDITOR.Rect = GetTaeEditorRect();

                    try
                    {
                        TAE_EDITOR.Draw(GraphicsDevice, TaeEditorSpriteBatch,
                            TAE_EDITOR_BLANK_TEX, TAE_EDITOR_FONT,
                            (float)gameTime.ElapsedGameTime.TotalSeconds, TAE_EDITOR_FONT_SMALL,
                            TAE_EDITOR_SCROLLVIEWER_ARROW);
                    }
                    catch
                    {

                    }
                    if (IsLoadingTaskRunning)
                    {
                        GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
                        TAE_EDITOR.DrawDimmingRect(GraphicsDevice, TaeEditorSpriteBatch, TAE_EDITOR_BLANK_TEX);
                    }

                    GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);

                    

                    ImGuiDebugDrawer.DrawTtest();

                    if (SplashState is SplashScreenStates.FadeOut)
                    {
                        DrawSplash(clear: false);
                    }

                    NotificationManager.DrawAll();

                    if (SoundManager.DebugShowDiagnostics)
                    {
                        GFX.SpriteBatchBeginForText();
                        var diag = SoundManager.GetDebugDiagnosticString();
                        var diagSize = ImGuiDebugDrawer.MeasureString(diag, 16);
                        var bgRect = new Rectangle((int)(64 - 16), (int)(64 - 16), Window.ClientBounds.Width - 96, TAE_EDITOR.Rect.Height - 96);
                        ImGuiDebugDrawer.DrawRect(bgRect, Color.Black * 0.6f, 4);
                        //GFX.SpriteBatch.Draw(TAE_EDITOR_BLANK_TEX, bgRect, null, Color.Black * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 0.1f);
                        //DBG.DrawOutlinedText(diag, Vector2.One * 64, Color.Cyan, Main.TAE_EDITOR_FONT_SMALL, scale);
                        ImGuiDebugDrawer.DrawText(diag, Vector2.One * 64, Color.Cyan, Color.Black, 16);
                        GFX.SpriteBatchEnd();
                    }

                    ImGuiDebugDrawer.End();
                    ImGuiDraw.AfterLayout(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height, 0);

                    

                    //DrawImGui(gameTime, 0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
                }
                //else
                //{
                //    // TESTING
                //    GFX.Device.Clear(Color.Fuchsia);
                //}
#if !DEBUG
            }
            catch (Exception ex)
            {
                if (!ErrorLog.HandleException(ex, "Fatal error ocurred during rendering"))
                {
                    Main.WinForm.Close();
                }
                GFX.Device.SetRenderTarget(null);
            }
#endif


                

            }
            
        }
    }
}
