using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using DSAnimStudio.TaeEditor;

namespace DSAnimStudio
{
    public class DBG
    {
        public static bool ShowPrimitiveNametags = true;
        public static bool SimpleTextLabelSize = false;
        public static float PrimitiveNametagSize = 1;

        public static DbgPrimWireGrid DbgPrim_Grid_XZ;
        public static DbgPrimWireGrid DbgPrim_Grid_XY;
        public static DbgPrimSkybox DbgPrim_Skybox;

        public static DbgPrimWireSphere DbgPrim_SoundEvent;

        public static bool ShowModelNames = true;
        public static bool ShowModelBoundingBoxes = false;
        public static bool ShowModelSubmeshBoundingBoxes = false;

        public static object _lock_DebugDrawEnablers = new object();

        public static Dictionary<DbgPrimCategory, bool> CategoryEnableDraw = new Dictionary<DbgPrimCategory, bool>();
        public static Dictionary<DbgPrimCategory, bool> CategoryEnableDbgLabelDraw = new Dictionary<DbgPrimCategory, bool>();
        public static Dictionary<DbgPrimCategory, bool> CategoryEnableNameDraw = new Dictionary<DbgPrimCategory, bool>();

        public static bool GetCategoryEnableDraw(DbgPrimCategory category)
        {
            bool result = false;
            lock (DBG._lock_DebugDrawEnablers)
            {
                result = CategoryEnableDraw[category];
            }
            return result;
        }

        public static bool GetCategoryEnableDbgLabelDraw(DbgPrimCategory category)
        {
            bool result = false;
            lock (DBG._lock_DebugDrawEnablers)
            {
                result = CategoryEnableDbgLabelDraw[category];
            }
            return result;
        }

        public static bool GetCategoryEnableNameDraw(DbgPrimCategory category)
        {
            bool result = false;
            lock (DBG._lock_DebugDrawEnablers)
            {
                result = CategoryEnableNameDraw[category];
            }
            return result;
        }

        public static Texture2D UV_CHECK_TEX { get; private set; }
        static string UV_CHECK_TEX_NAME => $@"{Main.Directory}\Content\UVCheck";
        public static SpriteFont DEBUG_FONT { get; private set; }
        static string DEBUG_FONT_NAME => $@"{Main.Directory}\Content\Fonts\DbgMenuFont";

        public static SpriteFont DEBUG_FONT_SMALL { get; private set; }
        static string DEBUG_FONT_SMALL_NAME => $@"{Main.Directory}\Content\Fonts\DbgMenuFontSmall";

        public static SpriteFont DEBUG_FONT_VERY_SMALL { get; private set; }
        static string DEBUG_FONT_VERY_SMALL_NAME => $@"{Main.Directory}\Content\Fonts\DbgMenuFontSmaller";

        public static SpriteFont DEBUG_FONT_SIMPLE { get; private set; }
        static string DEBUG_FONT_SIMPLE_NAME => $@"{Main.Directory}\Content\Fonts\DbgMenuFontSimple";

        public static SpriteFont TRUE_DEBUG_FONT { get; private set; }
        public static SoulsFormats.CCM TrueDebugFontCCM = null;

        //private static VertexPositionColor[] DebugLinePositionBuffer = new VertexPositionColor[2];
        //private static VertexBuffer DebugLineVertexBuffer;
        //private static readonly int[] DebugLineIndexBuffer = new int[] { 0, 1 };

        //private static VertexPositionColor[] DebugBoundingBoxPositionBuffer = new VertexPositionColor[8];
        //private static VertexBuffer DebugBoundingBoxVertexBuffer;
        /*
         * 0 = Top Front Left
         * 1 = Top Front Right
         * 2 = Bottom Front Right
         * 3 = Bottom Front Left
         * 4 = Top Back Left
         * 5 = Top Back Right
         * 6 = Bottom Back Right
         * 7 = Bottom Back Left
         */
        private static readonly int[] DebugBoundingBoxIndexBuffer = new int[]
        {
            0, 1, 1, 5, 5, 4, 4, 0, // Top Face
            2, 3, 3, 7, 7, 6, 6, 2, // Bottom Face
            0, 3, // Front Left Pillar
            1, 2, // Front Right Pillar
            4, 7, // Back Left Pillar
            5, 6, // Back Right Pillar
        };

        static DBG()
        {
            lock (DBG._lock_DebugDrawEnablers)
            {
                CategoryEnableDraw.Clear();
                CategoryEnableDbgLabelDraw.Clear();
                CategoryEnableNameDraw.Clear();

                var categories = (DbgPrimCategory[])Enum.GetValues(typeof(DbgPrimCategory));
                foreach (var c in categories)
                {
                    CategoryEnableDraw.Add(c, false);
                    CategoryEnableDbgLabelDraw.Add(c, false);
                    CategoryEnableNameDraw.Add(c, false);
                }

                CategoryEnableDraw[DbgPrimCategory.FlverBone] = false;
                CategoryEnableDraw[DbgPrimCategory.HkxBone] = false;

                DbgPrimXRay = true;

                CategoryEnableDraw[DbgPrimCategory.DummyPolyHelper] = true;

                CategoryEnableDraw[DbgPrimCategory.DummyPolySpawnArrow] = true;

                CategoryEnableNameDraw[DbgPrimCategory.DummyPolyHelper] = true;

                CategoryEnableDraw[DbgPrimCategory.SoundEvent] = false;
                //CategoryEnableNameDraw[DbgPrimCategory.SoundEvent] = true;

                CategoryEnableDraw[DbgPrimCategory.Skybox] = true;
                CategoryEnableDraw[DbgPrimCategory.Other] = true;

                CategoryEnableDraw[DbgPrimCategory.RootMotionPath] = true;
            }
        }

        public static bool DbgPrimXRay = false;

        public static Color COLOR_FLVER_BONE = Color.Yellow;
        //public static string COLOR_FLVER_BONE_NAME = "Yellow";

        //public static Color COLOR_HKX_BONE = Color.Aqua;
        //public static string COLOR_HKX_BONE_NAME = "Aqua";

        public static Color COLOR_FLVER_BONE_BBOX = Color.Lime;
        //public static string COLOR_FLVER_BONE_BBOX_NAME = "Lime";

        public static Color COLOR_DUMMY_POLY = Color.MonoGameOrange;
        public static Color COLOR_DUMMY_POLY_DBG = Color.Yellow;
        //public static string COLOR_DUMMY_POLY_NAME = "Orange";

        public static Color COLOR_SOUND_EVENT = Color.Red;
        //public static string COLOR_SOUND_EVENT_NAME = "Red";

        private static object _lock_primitives = new object();

        public static bool EnableMenu = false;

        public static bool EnableKeyboardInput = false;
        public static bool EnableMouseInput = true;
        public static bool EnableGamePadInput = false;

        public static MouseState DisabledMouseState;
        public static KeyboardState DisabledKeyboardState;
        public static GamePadState DisabledGamePadState;

        public static Dictionary<string, SoundEffect> Sounds = new Dictionary<string, SoundEffect>();

        private static SoundEffectInstance BeepSound;

        private static BasicEffect SpriteBatch3DBillboardEffect;

        private static float _beepVolume = 0;
        public static float BeepVolume
        {
            get => _beepVolume;
            set
            {
                if (value > 0.01f)
                    BeepSound.Volume = 0.5f;
                else
                    BeepSound.Volume = 0;

                _beepVolume = value;
            }
        }

        public static string[] ImguiDummyPolyToggleList = new string[] { };
        public static int ImguiDummyPolyToggleListIndex = -1;

        public static void CreateDebugPrimitives()
        {
            // This is the green grid, which is just hardcoded lel
            DbgPrim_Grid_XZ = new DbgPrimWireGrid(new Color(127, 127, 127), new Color(127, 127, 127), 100, 1);
            DbgPrim_Grid_XZ.OverrideColor = new Color(32, 112, 39);
            DbgPrim_Grid_XZ.Transform = Transform.Default;

            DbgPrim_Grid_XY = new DbgPrimWireGrid(new Color(127, 127, 127), new Color(127, 127, 127), 100, 1);
            DbgPrim_Grid_XY.OverrideColor = new Color(32, 39, 112);
            DbgPrim_Grid_XY.Transform = new Transform(Matrix.CreateRotationX(MathHelper.PiOver2));

            //DbgPrim_Grid.OverrideColor = Color.Green;
            DbgPrim_Skybox = new DbgPrimSkybox();

            DbgPrim_SoundEvent = new DbgPrimWireSphere(Transform.Default, COLOR_SOUND_EVENT);
            DbgPrim_SoundEvent.NameColor = COLOR_SOUND_EVENT;
            DbgPrim_SoundEvent.Category = DbgPrimCategory.SoundEvent;

            
            DbgPrim_Skybox.Transform = new Transform(Matrix.CreateScale(100));


            // If you want to disable the grid on launch uncomment the next line.
            //ShowGrid = false;

            // If you want the menu to be CLOSED on launch uncomment the next line.
            DbgMenus.DbgMenuItem.MenuOpenState = DbgMenus.DbgMenuOpenState.Closed;


            // Put stuff below for testing:

           

        }

        public static void DrawDepthRespectPrims()
        {
            Main.TAE_EDITOR.Graph?.ViewportInteractor?.DrawDebug();

            GFX.CurrentWorldView.DrawPrims();
        }

        public static void DrawDepthDisrespectPrims()
        {
            GFX.CurrentWorldView.DrawOverPrims();
        }

        public static void DrawSkybox()
        {
            if (Environment.DrawCubemap)
            {
                GFX.CurrentWorldView.ApplyViewToShader_Skybox(GFX.SkyboxShader);
                DbgPrim_Skybox.Draw(null, Matrix.Identity);
            }
        }

        public static void DrawBehindPrims()
        {

            if (GFX.CurrentWorldView.ShowGrid)
            {
                DbgPrim_Grid_XZ.Draw(null, Matrix.Identity);
                //DbgPrim_Grid_XY.Draw(null, Matrix.Identity);
            }
        }

        public static void DrawPrimitives()
        {
            if (BeepVolume > 0)
                BeepVolume -= (0.016666667f * 30);
            else
                BeepVolume = 0;

            GFX.CurrentWorldView.Update(0);

            lock (Scene._lock_ModelLoad_Draw)
            {
                foreach (var mdl in Scene.Models)
                {
                    mdl.DrawAllPrimitiveShapes();
                }
            }

            if (CategoryEnableDraw[DbgPrimCategory.SoundEvent])
            {
                Dictionary<Vector3, List<string>> textsOnLocations = SoundManager.GetDebugDrawInstances();

                var sb = new StringBuilder();

                foreach (var kvp in textsOnLocations)
                {
                    DbgPrim_SoundEvent.Transform = new Transform(Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(kvp.Key));
                    DbgPrim_SoundEvent.Draw(null, Matrix.Identity);
                }

                GFX.SpriteBatchBeginForText();
                foreach (var kvp in textsOnLocations)
                {
                    sb.Clear();

                    var sorted = kvp.Value.OrderBy(x => x);

                    foreach (var thing in sorted)
                        sb.AppendLine(thing);

                    DrawTextOn3DLocation_FixedPixelSize(
                        Matrix.CreateTranslation(kvp.Key),
                        Vector3.Zero,
                        sb.ToString(), COLOR_SOUND_EVENT, 2,
                        startAndEndSpriteBatchForMe: false, 
                        screenPixelOffset: Vector2.One * 16, 
                        fontOverride: DEBUG_FONT_SMALL);
                }
                GFX.SpriteBatchEnd();

                //FmodManager.DbgPrimCamPos.Draw(null, Matrix.Identity);
            }

            if (RemoManager.EnableDummyPrims)
            {
                List<Model> mdls = null;
                lock (Scene._lock_ModelLoad_Draw)
                {
                    mdls = Scene.Models.ToList();
                }

                foreach (var m in mdls)
                {
                    m.DrawRemoPrims();
                }
            }

            //GFX.SpriteBatchBeginForText();

                //GFX.World.DbgPrintA.Draw();
                //GFX.World.DbgPrintB.Draw();

                //GFX.SpriteBatchEnd();
        }


        public static void DrawPrimitiveTexts()
        {
            if (BeepVolume > 0)
                BeepVolume -= (0.016666667f * 30);
            else
                BeepVolume = 0;

            lock (Scene._lock_ModelLoad_Draw)
            {
                GFX.SpriteBatchBeginForText();

                foreach (var mdl in Scene.Models)
                {
                    mdl.DrawAllPrimitiveTexts();
                }

                GFX.SpriteBatchEnd();
            }
        }

        public static void LoadDebugFontCCM()
        {
            if (TrueDebugFontCCM == null)
            {
                TrueDebugFontCCM = SoulsFormats.CCM.Read($@"{Main.Directory}\Content\Fonts\DS1_DEBUG_FONT.ccm");
            }
        }

        public static void LoadContent(ContentManager c)
        {
            UV_CHECK_TEX = c.Load<Texture2D>(UV_CHECK_TEX_NAME);

            DEBUG_FONT = c.Load<SpriteFont>(DEBUG_FONT_NAME);
            DEBUG_FONT_SMALL = c.Load<SpriteFont>(DEBUG_FONT_SMALL_NAME);
            DEBUG_FONT_VERY_SMALL = c.Load<SpriteFont>(DEBUG_FONT_VERY_SMALL_NAME);
            DEBUG_FONT_SIMPLE = c.Load<SpriteFont>(DEBUG_FONT_SIMPLE_NAME);

            Sounds = new Dictionary<string, SoundEffect>();

            foreach (var se in Directory.GetFiles($@"{Main.Directory}\Content\Sounds"))
            {
                var seInfo = new FileInfo(se);
                using (var stream = File.OpenRead(se))
                {
                    Sounds.Add(seInfo.Name.ToLower(), SoundEffect.FromStream(stream));
                }
            }

            BeepSound = Sounds["selected_event_loop.wav"].CreateInstance();
            BeepSound.IsLooped = true;
            BeepVolume = 0;
            BeepSound.Play();
            //BeepSound.Pause();

            SpriteBatch3DBillboardEffect = new BasicEffect(GFX.Device)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };

            //var trueDebugFontTex = new TextureFetchRequest(
            //    File.ReadAllBytes($@"{Main.Directory}\Content\Fonts\DS1_DEBUG_FONT.dds"), "TRUE_DEBUG_FONT").Fetch2D();

            //LoadDebugFontCCM();

            //var tdfGlyphBounds = new List<Rectangle>();
            //var tdfCropping = new List<Rectangle>();
            //var tdfChars = new List<char>();
            //int tdfLineSpacing = 20;
            //float tdfSpacing = 1;
            //var tdfKerning = new List<Vector3>();
            //char tdfDefaultChar = '?';

            //var ccm = TrueDebugFontCCM;

            //foreach (var g in ccm.Glyphs)
            //{
            //    var rect = new Rectangle(
            //        (int)Math.Round(g.Value.UV1.X * ccm.TexWidth),
            //        (int)Math.Round(g.Value.UV1.Y * ccm.TexHeight),
            //        (int)Math.Round((g.Value.UV2.X - g.Value.UV1.X) * ccm.TexWidth),
            //        (int)Math.Round((g.Value.UV2.Y - g.Value.UV1.Y) * ccm.TexHeight));

            //    if (g.Value.TexIndex == 1)
            //    {
            //        rect.X += 512;
            //    }
            //    else if (g.Value.TexIndex == 2)
            //    {
            //        rect.X += 1024;
            //    }
            //    else if (g.Value.TexIndex == 3)
            //    {
            //        rect.Y += 512;
            //    }
            //    else if (g.Value.TexIndex == 4)
            //    {
            //        rect.Y += 512;
            //        rect.X += 512;
            //    }
            //    else if (g.Value.TexIndex == 5)
            //    {
            //        rect.Y += 512;
            //        rect.X += 1024;
            //    }
            //    else if (g.Value.TexIndex == 6)
            //    {
            //        rect.Y += 1024;
            //    }
            //    else if (g.Value.TexIndex == 7)
            //    {
            //        rect.Y += 1024;
            //        rect.X += 512;
            //    }
            //    else if (g.Value.TexIndex == 8)
            //    {
            //        rect.Y += 1024;
            //        rect.X += 1024;
            //    }

            //    tdfGlyphBounds.Add(rect);
            //    tdfCropping.Add(new Rectangle(0, 0, rect.Width, rect.Height));
            //    tdfChars.Add((char)g.Key);
            //    tdfKerning.Add(new Vector3(g.Value.PreSpace, 0, g.Value.Advance));
            //}

            //TRUE_DEBUG_FONT = new SpriteFont(trueDebugFontTex, tdfGlyphBounds, tdfCropping, tdfChars, tdfLineSpacing, tdfSpacing, tdfKerning, tdfDefaultChar);
        }

        //public static void Draw3DBillboard(string text, Transform t, Color c)
        //{
        //    Matrix m = Matrix.CreateScale(1, -1, 1) * t.WorldMatrix;

        //    if (TaeInterop.CameraFollowsRootMotion)
        //        m *= Matrix.CreateTranslation(-TaeInterop.CurrentRootMotionDisplacement.XYZ());

        //    SpriteBatch3DBillboardEffect.World = m;
        //    SpriteBatch3DBillboardEffect.View = GFX.World.CameraTransform.CameraViewMatrix * Matrix.Invert(GFX.World.MatrixWorld);
        //    SpriteBatch3DBillboardEffect.Projection = GFX.World.MatrixProjection;

        //    GFX.SpriteBatch.Begin(0, null, null, null, null, SpriteBatch3DBillboardEffect);

        //    GFX.SpriteBatch.DrawString(DEBUG_FONT, text, Vector2.Zero, c);

        //    GFX.SpriteBatch.End();
        //}

        public static void Draw3DBillboard(string text, Matrix m, Color c)
        {
            //var camDot = Vector3.Dot(GFX.World.CameraTransform.WorldMatrix.Forward, m.Forward);

            //m = Matrix.CreateScale(camDot > 0 ? 1 : -1, -1, 1) * m;

            

            ////if (TaeInterop.CameraFollowsRootMotion)
            ////    m *= Matrix.CreateTranslation(-TaeInterop.CurrentRootMotionDisplacement.XYZ());

            //SpriteBatch3DBillboardEffect.World =
            //    Matrix.CreateScale(0.025f) 
            //    * m;
            //SpriteBatch3DBillboardEffect.View =
                
            //    GFX.World.CameraTransform.CameraViewMatrix
            //    * Matrix.Invert(GFX.World.MatrixWorld)
            //     * Matrix.CreateLookAt(Vector3.Transform(Vector3.Zero, m),
            //    GFX.World.CameraTransform.Position, Vector3.Up);
            //SpriteBatch3DBillboardEffect.Projection = GFX.World.MatrixProjection;

            //GFX.SpriteBatchBegin(0, null, null, DepthStencilState.DepthRead, RasterizerState.CullNone, SpriteBatch3DBillboardEffect);

            //var centerThing = DEBUG_FONT.MeasureString(text) / 2;

            //var se = camDot > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(1, 1), Color.Black, 0, centerThing, 1.0f, se, 0);
            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(1, -1), Color.Black, 0, centerThing, 1.0f, se, 0);
            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(-1, 1), Color.Black, 0, centerThing, 1.0f, se, 0);
            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(-1, -1), Color.Black, 0, centerThing, 1.0f, se, 0);

            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(0, 1), Color.Black, 0, centerThing, 1.0f, se, 0);
            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(0, -1), Color.Black, 0, centerThing, 1.0f, se, 0);
            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(-1, 0), Color.Black, 0, centerThing, 1.0f, se, 0);
            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, new Vector2(1, 0), Color.Black, 0, centerThing, 1.0f, se, 0);

            //GFX.SpriteBatch.DrawString(DEBUG_FONT, text, Vector2.Zero, c, 0, centerThing, 1.0f, se, 0);


            //GFX.SpriteBatchEnd();
        }

        //public static void DrawLine(Vector3 start, Vector3 end, Color startColor, Color? endColor = null, string startName = null, string endName = null)
        //{
        //    DebugLinePositionBuffer[0].Position = start;
        //    DebugLinePositionBuffer[0].Color = startColor;
        //    DebugLinePositionBuffer[1].Position = end;
        //    DebugLinePositionBuffer[1].Color = endColor ?? startColor;
        //    DebugLineIndexBuffer[0] = 0;
        //    DebugLineIndexBuffer[1] = 1;

        //    if (DebugLineVertexBuffer == null)
        //        DebugLineVertexBuffer = new VertexBuffer(GFX.Device, typeof(VertexPositionColor), 2, BufferUsage.None);

        //    GFX.World.ApplyViewToShader(GFX.DbgPrimShader);

        //    foreach (var pass in GFX.DbgPrimShader.Effect.CurrentTechnique.Passes)
        //    {
        //        pass.Apply();

        //        GFX.Device.SetVertexBuffer(DebugLineVertexBuffer);
        //        GFX.Device.DrawUserIndexedPrimitives(PrimitiveType.LineList,
        //            DebugLinePositionBuffer, 0, 2, DebugLineIndexBuffer, 0, 1);
        //    }

        //    if (startName != null)
        //    {
        //        DrawTextOn3DLocation(start, startName, startColor, 0.25f);
        //    }

        //    if (endName != null)
        //    {
        //        DrawTextOn3DLocation(end, endName, endColor ?? startColor, 0.25f);
        //    }
        //}

        public static void DrawTextOn3DLocation(Matrix m, Vector3 location, string text, Color color, float simpleFontScale, bool startAndEndSpriteBatchForMe = false, Vector2 screenPixelOffset = default)
        {
            //if (ShowFancyTextLabels)
            //    DrawTextOn3DLocation_Fancy(location, text, color, physicalHeight, startAndEndSpriteBatchForMe);
            //else
            //    DrawTextOn3DLocation_Fast(location, text, color, physicalHeight, startAndEndSpriteBatchForMe);

            //DrawTextOn3DLocation_Fast__OLD(m, location, text, color, simpleFontScale, startAndEndSpriteBatchForMe);

            DrawTextOn3DLocation_FixedPixelSize(m, location, text, color, simpleFontScale * 3, startAndEndSpriteBatchForMe, screenPixelOffset);
        }

        public static void DrawTextOn3DLocation_FixedPixelSize(Matrix m, Vector3 location, string text, 
            Color color, float simpleFontScale, bool startAndEndSpriteBatchForMe = false, Vector2 screenPixelOffset = default, 
            SpriteFont fontOverride = null)
        {
            if (startAndEndSpriteBatchForMe)
                GFX.SpriteBatchBeginForText();

            Vector3 screenPos3D = GFX.Device.Viewport.Project(location,
                GFX.CurrentWorldView.Matrix_Projection, 
                GFX.CurrentWorldView.Matrix_View, m * GFX.CurrentWorldView.Matrix_World);

            screenPos3D -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);

            if (screenPos3D.Z >= 1)
                return;

            //if (screenPos3D.X < GFX.Device.Viewport.Bounds.Left ||
            //    screenPos3D.X > GFX.Device.Viewport.Bounds.Right ||
            //    screenPos3D.Y < GFX.Device.Viewport.Bounds.Top ||
            //    screenPos3D.Y > GFX.Device.Viewport.Bounds.Bottom)
            //    return;

            screenPos3D += new Vector3(screenPixelOffset.X, screenPixelOffset.Y, 0);

            //if (screenPos3D.X < 0 ||
            //   screenPos3D.X > GFX.Device.Viewport.Width ||
            //   screenPos3D.Y < 0 ||
            //   screenPos3D.Y > GFX.Device.Viewport.Height)
            //    return;

            simpleFontScale *= (GFX.EffectiveSSAA / 2f);

            //screenPos3D += new Vector3(32, 32, 0);

            int shadowOffset = GFX.EffectiveSSAA;

            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2((int)screenPos3D.X, (int)screenPos3D.Y + shadowOffset).RoundInt(),
                Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0.0001f);

            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2(screenPos3D.X, screenPos3D.Y - shadowOffset).RoundInt(),
                Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0.0001f);

            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2((int)screenPos3D.X - shadowOffset, (int)screenPos3D.Y).RoundInt(),
                Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0.0001f);

            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2((int)screenPos3D.X + shadowOffset, (int)screenPos3D.Y).RoundInt(),
                Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0.0001f);



            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
               new Vector2(screenPos3D.X + shadowOffset, screenPos3D.Y + shadowOffset).RoundInt(),
               Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
               0.0001f);

            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2(screenPos3D.X - shadowOffset, screenPos3D.Y - shadowOffset).RoundInt(),
                Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0.0001f);

            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2(screenPos3D.X - shadowOffset, screenPos3D.Y + shadowOffset).RoundInt(),
                Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0.0001f);

            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2(screenPos3D.X + shadowOffset, screenPos3D.Y - shadowOffset).RoundInt(),
                Color.Black, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0.0001f);



            GFX.SpriteBatch.DrawString((fontOverride ?? DEBUG_FONT), text,
                new Vector2(screenPos3D.X, screenPos3D.Y).RoundInt(),
                color, 0, Vector2.Zero, simpleFontScale, SpriteEffects.None,
                0);

            if (startAndEndSpriteBatchForMe)
                GFX.SpriteBatchEnd();
        }

        private static void DrawTextOn3DLocation_Fast__OLD(Matrix m, Vector3 location, string text, 
            Color color, float physicalHeight, bool startAndEndSpriteBatchForMe = false)
        {
            // Project the 3d position first
            Vector3 screenPos3D_Top = GFX.Device.Viewport.Project(location + new Vector3(0, physicalHeight / 2, 0),
                GFX.CurrentWorldView.Matrix_Projection,
                GFX.CurrentWorldView.Matrix_View, m * GFX.CurrentWorldView.Matrix_World);

            Vector3 screenPos3D_Bottom = GFX.Device.Viewport.Project(location - new Vector3(0, physicalHeight / 2, 0),
                GFX.CurrentWorldView.Matrix_Projection,
                GFX.CurrentWorldView.Matrix_View, m * GFX.CurrentWorldView.Matrix_World);

            screenPos3D_Top -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);
            screenPos3D_Bottom -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);

            //Vector3 camNormal = Vector3.Transform(Vector3.Forward, CAMERA_ROTATION);
            //Vector3 directionFromCam = Vector3.Normalize(location - Vector3.Transform(WORLD_VIEW.CameraPosition, CAMERA_WORLD));
            //var normDot = Vector3.Dot(directionFromCam, camNormal);

            if (screenPos3D_Top.Z >= 1 || screenPos3D_Bottom.Z >= 1)
                return;

            // Just to make it easier to use we create a Vector2 from screenPos3D
            Vector2 screenPos2D_Top = new Vector2(screenPos3D_Top.X, screenPos3D_Top.Y);
            Vector2 screenPos2D_Bottom = new Vector2(screenPos3D_Bottom.X, screenPos3D_Bottom.Y);

            Vector2 screenPos2D_Center = (screenPos2D_Top + screenPos2D_Bottom) / 2;

            
            if (screenPos2D_Center.X < 0 || screenPos2D_Center.X > GFX.Device.Viewport.Width || screenPos2D_Center.Y < 0 || screenPos2D_Center.Y > GFX.Device.Viewport.Height)
                return;

            Vector2 labelSpritefontSize = DEBUG_FONT.MeasureString(text);

            float labelHeightInPixels = (screenPos2D_Top - screenPos2D_Bottom).Length();



            //text += $"[DBG]{screenPos3D.Z}";

            float scale = labelHeightInPixels / labelSpritefontSize.Y;

            if (scale < 0.05f)
                return;

            if (startAndEndSpriteBatchForMe)
                GFX.SpriteBatchBeginForText();

            //GFX.SpriteBatch.DrawString(DEBUG_FONT_SIMPLE, text, 
            //    new Vector2(screenPos3D.X, screenPos3D.Y) - 
            //    new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y), color);

            var textPos = new Vector2(screenPos2D_Center.X, screenPos2D_Center.Y) -
                new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y);

            GFX.SpriteBatch.DrawString(DEBUG_FONT, text,
                new Vector2((int)textPos.X + 2, (int)textPos.Y + 2),
                Color.Black, 0, !DBG.SimpleTextLabelSize ? labelSpritefontSize / 2 : Vector2.Zero, !DBG.SimpleTextLabelSize ? scale : 1.0f, SpriteEffects.None,
                ((screenPos3D_Top.Z + screenPos3D_Bottom.Z) / 2) + 0.0001f);

            GFX.SpriteBatch.DrawString(DEBUG_FONT, text,
                new Vector2((int)textPos.X, (int)textPos.Y),
                color, 0, !DBG.SimpleTextLabelSize ? labelSpritefontSize / 2 : Vector2.Zero, !DBG.SimpleTextLabelSize ? scale : 1.0f, SpriteEffects.None,
                ((screenPos3D_Top.Z + screenPos3D_Bottom.Z) / 2));


            GFX.SpriteBatch.DrawString(DEBUG_FONT, $"<{textPos.X}, {textPos.Y}>",
                new Vector2(64, 64),
                Color.Fuchsia, 0, Vector2.Zero, 1,
                SpriteEffects.None,
                ((screenPos3D_Top.Z + screenPos3D_Bottom.Z) / 2));


            if (startAndEndSpriteBatchForMe)
                GFX.SpriteBatchEnd();

        }

        //public static void DrawTextOn3DLocation_PhysicalSize(Matrix m, Vector3 location, 
        //    string text, Color color, float physicalHeight, bool startAndEndSpriteBatchForMe = false, 
        //    Vector2 screenPixelOffset = default)
        //{
            

        //    // Project the 3d position first
        //    Vector3 screenPos3D_Top = GFX.Device.Viewport.Project(location + new Vector3(0, physicalHeight / 2, 0),
        //        GFX.World.MatrixProjection, GFX.World.CameraTransform.CameraViewMatrix * Matrix.Invert(GFX.World.MatrixWorld), m);

        //    Vector3 screenPos3D_Bottom = GFX.Device.Viewport.Project(location - new Vector3(0, physicalHeight / 2, 0),
        //        GFX.World.MatrixProjection, GFX.World.CameraTransform.CameraViewMatrix * Matrix.Invert(GFX.World.MatrixWorld), m);

        //    screenPos3D_Top -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);
        //    screenPos3D_Bottom -= new Vector3(GFX.Device.Viewport.X, GFX.Device.Viewport.Y, 0);

        //    //Vector3 camNormal = Vector3.Transform(Vector3.Forward, CAMERA_ROTATION);
        //    //Vector3 directionFromCam = Vector3.Normalize(location - Vector3.Transform(WORLD_VIEW.CameraPosition, CAMERA_WORLD));
        //    //var normDot = Vector3.Dot(directionFromCam, camNormal);

        //    if (screenPos3D_Top.Z >= 1 || screenPos3D_Bottom.Z >= 1)
        //        return;

            

        //    // Just to make it easier to use we create a Vector2 from screenPos3D
        //    Vector2 screenPos2D_Top = screenPixelOffset + new Vector2(screenPos3D_Top.X, screenPos3D_Top.Y);

        //    Vector2 screenPos2D_Bottom = screenPixelOffset + new Vector2(screenPos3D_Bottom.X, screenPos3D_Bottom.Y);

        //    Vector2 screenPos2D_Center = screenPixelOffset + (screenPos2D_Top + screenPos2D_Bottom) / 2;

        //    if (screenPos2D_Center.X < 0 || screenPos2D_Center.X > GFX.Device.Viewport.Width || screenPos2D_Center.Y < 0 || screenPos2D_Center.Y > GFX.Device.Viewport.Height)
        //        return;

        //    Vector2 labelSpritefontSize = DEBUG_FONT.MeasureString(text);

        //    float labelHeightInPixels = (screenPos2D_Top - screenPos2D_Bottom).Length();



        //    //text += $"[DBG]{screenPos3D.Z}";

        //    float scale = labelHeightInPixels / labelSpritefontSize.Y;

        //    DrawOutlinedText(text, 
        //        (screenPos2D_Center) - new Vector2(GFX.Device.Viewport.X, GFX.Device.Viewport.Y),
        //        color, DEBUG_FONT,
        //        ((screenPos3D_Top.Z + screenPos3D_Bottom.Z) / 2), !DBG.SimpleTextLabelSize ? scale : 1.0f, !DBG.SimpleTextLabelSize ? labelSpritefontSize / 2 : Vector2.Zero, 
        //        startAndEndSpriteBatchForMe);
        //}

        //public static void DrawBoundingBox(BoundingBox bb, Color color, Transform transform)
        //{
        //    DebugBoundingBoxPositionBuffer[0].Position = new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z);
        //    DebugBoundingBoxPositionBuffer[1].Position = new Vector3(bb.Max.X, bb.Max.Y, bb.Max.Z);
        //    DebugBoundingBoxPositionBuffer[2].Position = new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z);
        //    DebugBoundingBoxPositionBuffer[3].Position = new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z);
        //    DebugBoundingBoxPositionBuffer[4].Position = new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z);
        //    DebugBoundingBoxPositionBuffer[5].Position = new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z);
        //    DebugBoundingBoxPositionBuffer[6].Position = new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z);
        //    DebugBoundingBoxPositionBuffer[7].Position = new Vector3(bb.Min.X, bb.Min.Y, bb.Min.Z);

        //    for (int i = 0; i < DebugBoundingBoxPositionBuffer.Length; i++)
        //    {
        //        DebugBoundingBoxPositionBuffer[i].Color = color;
        //    }

        //    if (DebugBoundingBoxVertexBuffer == null)
        //        DebugBoundingBoxVertexBuffer = new VertexBuffer(GFX.Device,
        //            typeof(VertexPositionColor), 8, BufferUsage.None);

        //    GFX.World.ApplyViewToShader(GFX.DbgPrimWireShader, transform);

        //    foreach (var pass in GFX.DbgPrimWireShader.Effect.CurrentTechnique.Passes)
        //    {
        //        pass.Apply();

        //        GFX.Device.SetVertexBuffer(DebugLineVertexBuffer);
        //        GFX.Device.DrawUserIndexedPrimitives(PrimitiveType.LineList,
        //            DebugBoundingBoxPositionBuffer, 0, 8, DebugBoundingBoxIndexBuffer, 0, 12);
        //    }

        //}

        public static void DrawOutlinedText(
            string text, Vector2 pos, Color color, SpriteFont font = null,
            float depth = 0, float scale = 1, Vector2 scaleOrigin = default(Vector2),
            bool startAndEndSpriteBatchForMe = false)
        {
            //if (startAndEndSpriteBatchForMe)
            //    GFX.SpriteBatchBeginForText();

            //// Top, Bottom, Left, Right
            //if (font != DEBUG_FONT_SMALL)
            //{
            //    GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(1, 0), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            //    GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(-1, 0), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            //    GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(0, 1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            //    GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(0, -1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            //}
            //// Top-Left, Top-Right, Bottom-Left, Bottom-Right
            ////GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(-1, 1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            ////GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(-1, -1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            ////GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(1, 1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            ////GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(1, -1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);

            //GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos, color, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth);

            //if (startAndEndSpriteBatchForMe)
            //    GFX.SpriteBatchEnd();
            ImGuiDebugDrawer.DrawText(text, pos - scaleOrigin, color, Color.Black * (color.A / 255f) * (color.A / 255f), 20 * scale);
        }

        public static void DrawOutlinedText(
            string text, Vector2 pos, Color color, SpriteFont font,
            float depth, Vector2 scale, Vector2 scaleOrigin = default(Vector2),
            bool startAndEndSpriteBatchForMe = false)
        {
            if (startAndEndSpriteBatchForMe)
                GFX.SpriteBatchBeginForText();

            // Top, Bottom, Left, Right
            GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(1, 0), Color.Black, 0, scaleOrigin, scale, SpriteEffects.None, depth + 0.000001f);
            GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(-1, 0), Color.Black, 0, scaleOrigin, scale, SpriteEffects.None, depth + 0.000001f);
            GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(0, 1), Color.Black, 0, scaleOrigin, scale, SpriteEffects.None, depth + 0.000001f);
            GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(0, -1), Color.Black, 0, scaleOrigin, scale, SpriteEffects.None, depth + 0.000001f);

            // Top-Left, Top-Right, Bottom-Left, Bottom-Right
            //GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(-1, 1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            //GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(-1, -1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            //GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(1, 1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);
            //GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos + new Vector2(1, -1), Color.Black, 0, scaleOrigin, Vector2.One * scale, SpriteEffects.None, depth + 0.000001f);

            GFX.SpriteBatch.DrawString(font ?? DEBUG_FONT, text, pos, color, 0, scaleOrigin, scale, SpriteEffects.None, depth);

            

            if (startAndEndSpriteBatchForMe)
                GFX.SpriteBatchEnd();
        }

        public class DbgPrimDrawer : IDisposable
        {
            private List<IDbgPrim> Primitives = new List<IDbgPrim>();

            private List<IDbgPrim> PrimitivesMarkedForDeletion = new List<IDbgPrim>();

            private readonly Model MODEL;

            public DbgPrimDrawer(Model mdl)
            {
                MODEL = mdl;
            }

            private IEnumerable<IDbgPrim> GetPrimitivesByDistance()
            {
                return Primitives.OrderByDescending(p => (GFX.CurrentWorldView.CameraLocationInWorld.Position - p.Transform.Position).LengthSquared());
            }

            public IEnumerable<IDbgPrim> GetPrimitives()
            {
                return Primitives;
            }

            public void ClearPrimitives(DbgPrimCategory category)
            {
                lock (_lock_primitives)
                {
                    var primsToClear = Primitives.Where(p => p.Category == category).ToList();
                    foreach (var p in primsToClear)
                    {
                        Primitives.Remove(p);
                        p.Dispose();
                    }
                }
            }

            public void AddPrimitive(IDbgPrim primitive)
            {
                lock (_lock_primitives)
                {
                    Primitives.Add(primitive);
                }
            }

            public void RemovePrimitive(IDbgPrim prim)
            {
                lock (_lock_primitives)
                {
                    if (Primitives.Contains(prim))
                        Primitives.Remove(prim);
                    prim.Dispose();
                }
            }

            public void MarkPrimitiveForDeletion(IDbgPrim prim)
            {
                if (!PrimitivesMarkedForDeletion.Contains(prim))
                    PrimitivesMarkedForDeletion.Add(prim);
            }

            public void DrawPrimitiveNames()
            {
                lock (_lock_primitives)
                {
                    foreach (var p in GetPrimitivesByDistance())
                    {
                        if (ShowPrimitiveNametags)
                        {
                            if (!string.IsNullOrWhiteSpace(p.Name) && p.EnableNameDraw &&
                                DBG.CategoryEnableNameDraw[p.Category])
                            {
                                DrawTextOn3DLocation(p.Transform.WorldMatrix * MODEL.CurrentTransform.WorldMatrix,
                                    Vector3.Zero,
                                    p.Name.Trim(), p.NameColor, PrimitiveNametagSize,
                                    startAndEndSpriteBatchForMe: false, screenPixelOffset: Vector2.One * 32);
                                //Draw3DBillboard(p.Name, p.Transform.WorldMatrix * 
                                //    MODEL.CurrentTransform.WorldMatrix, p.NameColor);
                            }

                            p.LabelDraw(MODEL.CurrentTransform.WorldMatrix);
                            //p.LabelDraw_Billboard(MODEL.CurrentTransform.WorldMatrix);
                        }
                    }
                }
            }

            public void DrawPrimitives()
            {
                PrimitivesMarkedForDeletion.Clear();

                //if (Environment.DrawCubemap)
                //    GFX.ModelDrawer.DrawSkyboxes();


                lock (_lock_primitives)
                {
                    foreach (var p in GetPrimitivesByDistance())
                    {
                        p.Draw(null, MODEL.CurrentTransform.WorldMatrix);
                    }
                }

                //GFX.SpriteBatch.Begin();
                //if (ShowGrid)
                //    DbgPrim_Grid.LabelDraw_Billboard();


                //GFX.SpriteBatch.End();

                foreach (var p in PrimitivesMarkedForDeletion)
                {
                    RemovePrimitive(p);
                }
            }

            public void Dispose()
            {
                lock (_lock_primitives)
                {
                    foreach (var prim in Primitives)
                    {
                        prim?.Dispose();
                    }
                }
            }
        }
    }




}
