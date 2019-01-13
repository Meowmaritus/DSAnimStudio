using MeowDSIO;
using MeowDSIO.DataFiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TAEDX.TaeEditor;

namespace TAEDX
{
    public class TAEDX : Game
    {
        public const string VERSION = "v1.9";

        public string AutoLoadAnibnd = null;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont eventLabelFont;

        TaeEditorScreen testEditorScreen;

        public static Texture2D Blank;

        public TAEDX()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;

            IsFixedTimeStep = true;
            TargetElapsedTime = TimeSpan.FromTicks(166667);
            MaxElapsedTime = TimeSpan.FromTicks(333334);

            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.ApplyChanges();

            Window.AllowUserResizing = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            eventLabelFont = Content.Load<SpriteFont>("DbgMenuFontSmall");

            Blank = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Blank.SetData(new Color[] { Color.White }, 0, 1);

            var winForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);

            testEditorScreen = new TaeEditorScreen(winForm);
            //testEditorScreen.TaeFileName = @"G:\SteamLibrary\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr\c0000.anibnd.yabber\Model\chr\c0000\taeNew\win32\a20.tae";
            //testEditorScreen.LoadCurrentFile();

            if (AutoLoadAnibnd != null)
            {
                if (System.IO.File.Exists(AutoLoadAnibnd))
                {
                    testEditorScreen.FileContainerName = AutoLoadAnibnd;
                    testEditorScreen.LoadCurrentFile();
                }
            }
            
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
                testEditorScreen.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            if (!string.IsNullOrWhiteSpace(testEditorScreen.FileContainerName))
                Window.Title = $"{System.IO.Path.GetFileName(testEditorScreen.FileContainerName)}" +
                    $"{(testEditorScreen.IsModified ? "*" : "")}" +
                    $"{(testEditorScreen.IsReadOnlyFileMode ? " !READ ONLY!" : "")}" +
                    $" - TAE Editor DX {VERSION}";
            else
                Window.Title = $"TAE Editor DX {VERSION}";

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0.15f, 0.15f, 0.15f));

            testEditorScreen.Rect = new Rectangle(8, 0, GraphicsDevice.Viewport.Width - 16, GraphicsDevice.Viewport.Height - 20);

            testEditorScreen.Draw(gameTime, GraphicsDevice, spriteBatch,
                Blank, eventLabelFont, (float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Draw(gameTime);
        }
    }
}
