using TAEDX.DbgMenus;
using TAEDX.DebugPrimitives;
using MeowDSIO;
using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TAEDX
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Main : Game
    {
        //public static Form WinForm;

        public const string VERSION = "v3.0";

        public static bool FIXED_TIME_STEP = true;

        public static bool REQUEST_EXIT = false;

        public static bool Active { get; private set; }

        public static bool DISABLE_DRAW_ERROR_HANDLE = true;

        private static float MemoryUsageCheckTimer = 0;
        private static long MemoryUsage_Unmanaged = 0;
        private static long MemoryUsage_Managed = 0;
        private const float MemoryUsageCheckInterval = 0.5f;

        public static readonly Color SELECTED_MESH_COLOR = Color.Yellow * 0.05f;
        public static readonly Color SELECTED_MESH_WIREFRAME_COLOR = Color.Yellow;

        public static Texture2D DEFAULT_TEXTURE_DIFFUSE;
        public static Texture2D DEFAULT_TEXTURE_SPECULAR;
        public static Texture2D DEFAULT_TEXTURE_NORMAL;
        public static Texture2D DEFAULT_TEXTURE_MISSING;
        public const string DEFAULT_TEXTURE_MISSING_NAME = "Content\\MissingTexture";

        public static TaeEditor.TaeEditorScreen TAE_EDITOR;
        public static Texture2D TAE_EDITOR_BLANK_TEX;
        public static SpriteFont TAE_EDITOR_FONT;

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
            bool vsync, bool fullscreen, bool simpleMsaa)
        {
            graphics.PreferMultiSampling = simpleMsaa;
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferFormat = format;
            graphics.IsFullScreen = fullscreen;
            FIXED_TIME_STEP = graphics.SynchronizeWithVerticalRetrace = vsync;
            
            graphics.ApplyChanges();
        }

       

        //MCG MCGTEST_MCG;

        

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.DeviceCreated += Graphics_DeviceCreated;
            graphics.DeviceReset += Graphics_DeviceReset;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166667);
            // Setting this max higher allows it to skip frames instead of do slow motion.
            MaxElapsedTime = TimeSpan.FromTicks(166667);

            //IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = GFX.Display.Vsync;
            graphics.IsFullScreen = GFX.Display.Fullscreen;
            graphics.PreferMultiSampling = GFX.Display.SimpleMSAA;
            graphics.PreferredBackBufferWidth = GFX.Display.Width;
            graphics.PreferredBackBufferHeight = GFX.Display.Height;
            graphics.PreferredBackBufferFormat = GFX.Display.Format;
            if (!GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef))
            {
                System.Windows.Forms.MessageBox.Show("MonoGame is detecting your GPU as too " +
                    "low-end and refusing to enter the non-mobile Graphics Profile, " +
                    "which is needed for Soulsborne files. It will likely crash.");

                graphics.GraphicsProfile = GraphicsProfile.Reach;
            }
            else
            {
                graphics.GraphicsProfile = GraphicsProfile.HiDef;
            }

            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

            GFX.Display.SetFromDisplayMode(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);

            //GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            CFG.Save();
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

            DEFAULT_TEXTURE_MISSING = Content.Load<Texture2D>(DEFAULT_TEXTURE_MISSING_NAME);

            GFX.Device = GraphicsDevice;

            base.Initialize();
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
            GFX.Init(Content);
            DBG.LoadContent(Content);
            //InterrootLoader.OnLoadError += InterrootLoader_OnLoadError;

            DBG.CreateDebugPrimitives();

            GFX.World.ResetCameraLocation();

            DbgMenuItem.Init();

            UpdateMemoryUsage();

            CFG.Init();

            TAE_EDITOR_FONT = Content.Load<SpriteFont>("Content\\DbgMenuFontSmall");
            TAE_EDITOR_BLANK_TEX = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            TAE_EDITOR_BLANK_TEX.SetData(new Color[] { Color.White }, 0, 1);

            TAE_EDITOR = new TaeEditor.TaeEditorScreen((System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle));

            TaeInterop.Init();

            if (Program.ARGS.Length > 0)
            {
                TAE_EDITOR.FileContainerName = Program.ARGS[0];
                TAE_EDITOR.LoadCurrentFile();
                //LoadDragDroppedFiles(Program.ARGS.ToDictionary(f => f, f => File.ReadAllBytes(f)));
            }
        }

        private void InterrootLoader_OnLoadError(string contentName, string error)
        {
            Console.WriteLine($"CONTENT LOAD ERROR\nCONTENT NAME:{contentName}\nERROR:{error}");
        }

        private string GetMemoryUseString(string prefix, long MemoryUsage)
        {
            const double MEM_KB = 1024f;
            const double MEM_MB = 1024f * 1024f;
            const double MEM_GB = 1024f * 1024f * 1024f;

            if (MemoryUsage < MEM_KB)
                return $"{prefix}{(1.0 * MemoryUsage):0} B";
            else if (MemoryUsage < MEM_MB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_KB):0.00} KB";
            else if (MemoryUsage < MEM_GB)
                return $"{prefix}{(1.0 * MemoryUsage / MEM_MB):0.00} MB";
            else
                return $"{prefix}{(1.0 * MemoryUsage / MEM_GB):0.00} GB";
        }

        private void DrawMemoryUsage()
        {
            var str_managed = GetMemoryUseString("CLR Mem:  ", MemoryUsage_Managed);
            var str_unmanaged = GetMemoryUseString("Process Mem:  ", MemoryUsage_Unmanaged);

            var strSize_managed = DBG.DEBUG_FONT_SMALL.MeasureString(str_managed);
            var strSize_unmanaged = DBG.DEBUG_FONT_SMALL.MeasureString(str_unmanaged);

            DBG.DrawOutlinedText(str_managed, new Vector2(GFX.Device.Viewport.Width - 2, 
                GFX.Device.Viewport.Height - 2 - (strSize_managed.Y * 0.75f) - (strSize_unmanaged.Y * 0.75f)),
                Color.Yellow, DBG.DEBUG_FONT_SMALL, scale: 0.75f, scaleOrigin: new Vector2(strSize_managed.X, 0));

            DBG.DrawOutlinedText(str_unmanaged, new Vector2(GFX.Device.Viewport.Width - 2, 
                GFX.Device.Viewport.Height - 2 - (strSize_unmanaged.Y * 0.75f)),
                Color.Yellow, DBG.DEBUG_FONT_SMALL, scale:0.75f, scaleOrigin: new Vector2(strSize_unmanaged.X, 0));
        }

        private void UpdateMemoryUsage()
        {
            using (var proc = Process.GetCurrentProcess())
            {
                MemoryUsage_Unmanaged = proc.PrivateMemorySize64;
            }
            MemoryUsage_Managed = GC.GetTotalMemory(forceFullCollection: false);
        }

        protected override void Update(GameTime gameTime)
        {
            if (!TAE_EDITOR.Rect.Contains(TAE_EDITOR.Input.MousePositionPoint))
                TAE_EDITOR.Input.CursorType = TaeEditor.MouseCursorType.Arrow;

            if (IsActive)
                TAE_EDITOR.Update(gameTime);
            else
                TAE_EDITOR.Input.CursorType = TaeEditor.MouseCursorType.Arrow;

            if (!string.IsNullOrWhiteSpace(TAE_EDITOR.FileContainerName))
                Window.Title = $"{System.IO.Path.GetFileName(TAE_EDITOR.FileContainerName)}" +
                    $"{(TAE_EDITOR.IsModified ? "*" : "")}" +
                    $"{(TAE_EDITOR.IsReadOnlyFileMode ? " !READ ONLY!" : "")}" +
                    $" - TimeAct Editor DX+ {VERSION}";
            else
                Window.Title = $"TimeAct Editor DX+ {VERSION}";

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            IsFixedTimeStep = FIXED_TIME_STEP;

            Active = IsActive;

            if (DBG.EnableMenu)
            {
                DbgMenuItem.UpdateInput(elapsed);
                DbgMenuItem.UICursorBlinkUpdate(elapsed);
            }

            if (DbgMenuItem.MenuOpenState != DbgMenuOpenState.Open)
            {
                // Only update input if debug menu isnt fully open.
                GFX.World.UpdateInput(this, gameTime);
            }

            GFX.World.UpdateMatrices(GraphicsDevice);

            GFX.World.CameraPositionDefault.Position = Vector3.Zero;

            GFX.World.CameraOrigin.Position = new Vector3(GFX.World.CameraPositionDefault.Position.X, 
                GFX.World.CameraOrigin.Position.Y, GFX.World.CameraPositionDefault.Position.Z);

            DBG.DbgPrim_Grid.Transform = GFX.World.CameraPositionDefault;

            if (REQUEST_EXIT)
                Exit();

            MemoryUsageCheckTimer += elapsed;
            if (MemoryUsageCheckTimer >= MemoryUsageCheckInterval)
            {
                MemoryUsageCheckTimer = 0;
                UpdateMemoryUsage();
            }

            LoadingTaskMan.Update(elapsed);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GFX.DrawBegin(gameTime);

            GFX.Device.Viewport = new Viewport(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);

            TAE_EDITOR.Rect = new Rectangle(2, 0, GraphicsDevice.Viewport.Width - 4, GraphicsDevice.Viewport.Height - 2);

            TAE_EDITOR.Draw(gameTime, GraphicsDevice, GFX.SpriteBatch,
                TAE_EDITOR_BLANK_TEX, TAE_EDITOR_FONT, (float)gameTime.ElapsedGameTime.TotalSeconds);

            GFX.Device.Viewport = new Viewport(TAE_EDITOR.ModelViewerBounds);

            TaeInterop.TaeViewportDrawPre(gameTime);

            GFX.DrawScene(gameTime);

            TaeInterop.TaeViewportDrawPost(gameTime);

            DrawMemoryUsage();
        }
    }
}
