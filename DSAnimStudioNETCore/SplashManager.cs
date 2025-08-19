using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class SplashManager
    {
        public enum SplashScreenStates
        {
            FadeIn,
            Showing,
            FadeOut,
            Complete,
        }

        public static SplashScreenStates SplashState = SplashScreenStates.FadeIn;
        public static float SplashAlpha = 0;
        public static float SplashTimer = 0;
        public static float SplashFadeInDuration = 0.5f;
        public static float SplashFadeOutDuration = 0.25f;
        public static float SplashMinShowBeforeDismiss = 5;
        public static bool SplashDismissed = false;
        public static Texture2D SplashTex;
        public static string SplashText;
        public static float SplashTextFontSize = 20;
        public static int SplashTextFitMarginLeft = 80;
        public static int SplashTextFitMarginRight = 300;
        public static int SplashTextFitMarginTop = 80;
        public static int SplashTextFitMarginBottom = 100;
        public static SpriteFont SplashFont;
        
        // Splash Bar - Deprecated
        public static bool SplashBarEnabled = false;
        public static float SplashBarMarginX = 200;
        public static float SplashBarMarginY = 12;
        public static float SplashBarInnerPadding = 2;
        public static float SplashBarThickness = 10;




        public static float SplashAcceptTextFontSize = 24;
        public static float SplashAcceptTextDistBelowSplashText = 16;

        public static float SplashAcceptTextSineTimer = 0;
        public static float SplashAcceptTextSineCycleLength = 1.5f;
        public static float SplashAcceptTextSineOutputMin = 0;
        public static float SplashAcceptTextSineOutputMax = 1;
        public static float SplashAcceptTextSineOutput;

        public static bool Debug_SplashCannotBeDismissed = false;
        public static bool Debug_SplashLayoutDraw = false;
        public static bool Debug_IgnoreAlreadySeenFlag = false;

        private static void UpdateSplashSine(float elapsed)
        {
            SplashAcceptTextSineTimer += elapsed;
            SplashAcceptTextSineTimer %= (SplashAcceptTextSineCycleLength * 2);
            var ratio = SplashAcceptTextSineTimer / (SplashAcceptTextSineCycleLength * 2);
            var radians = MathHelper.TwoPi * ratio;
            float outputRatio = MathF.Abs(MathF.Sin(radians));
            SplashAcceptTextSineOutput = Utils.MapRange(outputRatio, 0, 1, SplashAcceptTextSineOutputMin, SplashAcceptTextSineOutputMax);
        }

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

            SplashText = Main.GetEmbeddedResourceText($@"/EmbRes/Credits.txt");
        }

        public static void DrawSplash(bool clear)
        {
            GFX.SpriteBatchBegin(transformMatrix: Matrix.CreateScale(1 / Main.DPI, 1 / Main.DPI, 1 / Main.DPI));
            if (clear)
                GFX.Device.Clear(Color.Black);

            var measureText = ImGuiDebugDrawer.MeasureString(SplashText, SplashTextFontSize);


            float aspectRatio = (float)GFX.Device.Viewport.Bounds.Width / (float)GFX.Device.Viewport.Bounds.Height;

            Rectangle rect = GFX.Device.Viewport.Bounds;
            Rectangle texRect = rect;
            Rectangle splashTextFitRect = new Rectangle(rect.X + SplashTextFitMarginLeft, rect.Y + SplashTextFitMarginTop,
                rect.Width - SplashTextFitMarginLeft - SplashTextFitMarginRight, rect.Height - SplashTextFitMarginTop - SplashTextFitMarginBottom);




            

            float splashBarRatio = 1;

            if (SplashState == SplashScreenStates.Showing)
            {
                splashBarRatio = (MathHelper.Clamp(SplashTimer / SplashMinShowBeforeDismiss, 0, 1));
            }
            else if (SplashState is SplashScreenStates.FadeIn)
            {
                splashBarRatio = 0;
            }

            Vector2 howMuchBiggerTextIsThanWindow = new Vector2(measureText.X / splashTextFitRect.Width, measureText.Y / splashTextFitRect.Height);
            float biggestScale = Math.Max(howMuchBiggerTextIsThanWindow.X, howMuchBiggerTextIsThanWindow.Y);

            float newFontSize = SplashTextFontSize / biggestScale;

            float newFontSize_AcceptText = SplashAcceptTextFontSize / biggestScale;

            float newSplashAcceptTextDistBelowSplashText = SplashAcceptTextDistBelowSplashText / biggestScale;


            float newSplashBarMarginX = SplashBarMarginX / biggestScale;
            float newSplashBarMarginY = SplashBarMarginY / biggestScale;
            float newSplashBarThickness = SplashBarThickness / biggestScale;
            float newSplashBarInnerPadding = SplashBarInnerPadding / biggestScale;



            Rectangle splashBar = new Rectangle((int)Math.Round(rect.X + newSplashBarMarginX),
                (int)Math.Round(rect.Bottom - newSplashBarMarginY - newSplashBarThickness),
                (int)Math.Round(rect.Width - (newSplashBarMarginX * 2)),
                (int)Math.Round(newSplashBarThickness));

            Rectangle splashBarFill = splashBar;

            splashBarFill = new Rectangle((int)Math.Round(splashBarFill.X + newSplashBarInnerPadding), 
                (int)Math.Round(splashBarFill.Y + newSplashBarInnerPadding),
                (int)Math.Round(splashBarFill.Width - (newSplashBarInnerPadding * 2)), 
                (int)Math.Round(splashBarFill.Height - (newSplashBarInnerPadding * 2)));

            splashBarFill.Width = (int)Math.Round(splashBarFill.Width * splashBarRatio);


            if (aspectRatio >= 16f / 9f)
            {
                float height = GFX.Device.Viewport.Bounds.Height;
                float width = height * (16f / 9f);

                texRect = new Rectangle((int)((GFX.Device.Viewport.Bounds.Width / 2f) - (width / 2f)), 0,
                (int)width, (int)height);
            }
            else
            {
                float width = GFX.Device.Viewport.Bounds.Width;
                float height = width * (9f / 16f);
                texRect = new Rectangle(0, (int)((GFX.Device.Viewport.Bounds.Height / 2f) - (height / 2f)),
                (int)width, (int)height);
            }



            GFX.SpriteBatch.Draw(Main.TAE_EDITOR_BLANK_TEX, GFX.Device.Viewport.Bounds, Color.Black * SplashAlpha);


            GFX.SpriteBatch.Draw(SplashTex, texRect, Color.White * SplashAlpha);

            if (SplashBarEnabled)
            {
                GFX.SpriteBatch.Draw(Main.TAE_EDITOR_BLANK_TEX, splashBar, Color.Gray * SplashAlpha);
                GFX.SpriteBatch.Draw(Main.TAE_EDITOR_BLANK_TEX, splashBarFill, Color.White * SplashAlpha);
            }

            

            //DBG.DrawOutlinedText("Please take a moment to read about everyone who has contributed to the project." +
            //        "\nYou only have to read this the first time you load the application.",
            //        new Vector2(splashBar.X, splashBar.Y - SplashWaitTextHeightAboveBar), Color.Cyan * SplashAlpha, scale: SplashAcceptTextScale);

            GFX.SpriteBatchEnd();

            //GFX.SpriteBatchBeginForText();// Matrix.CreateScale(2));
            //GFX.SpriteBatch.DrawString(SplashFont, SplashText, new Vector2(rect.X + 64, rect.Y + 64), Color.White * SplashAlpha);
            //GFX.SpriteBatchEnd();
            measureText /= biggestScale;

            var rectCenter = rect.Center();
            var textFinalPos = new Vector2(splashTextFitRect.X, rectCenter.Y - (measureText.Y / 2));

            Rectangle finalTextRect = new Rectangle((int)Math.Round(textFinalPos.X), (int)Math.Round(textFinalPos.Y), (int)Math.Round(measureText.X), (int)Math.Round(measureText.Y));

            if (Debug_SplashLayoutDraw)
            {
                ImGuiDebugDrawer.DrawRect(rect.InverseDpiScaled(), Color.Cyan, 0, thickness: 1);
                ImGuiDebugDrawer.DrawRect(splashTextFitRect.InverseDpiScaled(), Color.Lime, 0, thickness: 1);
                ImGuiDebugDrawer.DrawRect(finalTextRect.InverseDpiScaled(), Color.Fuchsia, 0, thickness: 1);
            }



            ImGuiDebugDrawer.DrawText(SplashText, textFinalPos / Main.DPIVector, Color.White * SplashAlpha, fontSize: newFontSize / Main.DPI, shadowColor: Color.Black * SplashAlpha);


            if ((SplashState == SplashScreenStates.Showing && SplashTimer >= SplashMinShowBeforeDismiss)
                || SplashState == SplashScreenStates.FadeOut)
            {
                //DBG.DrawOutlinedText("Click anywhere to dismiss this screen.",
                //    new Vector2(splashBar.X, splashBar.Y - SplashAcceptTexHeightAboveBar), Color.Yellow * SplashAlpha, scale: SplashAcceptTextScale);
                string acceptTextStr = "Click anywhere to dismiss this screen.";
                float alpha = SplashAcceptTextSineOutput * SplashAlpha;

                var acceptTextScale = ImGuiDebugDrawer.MeasureString(acceptTextStr, newFontSize_AcceptText);

                var acceptTextPos = new Vector2(finalTextRect.Center.X - (acceptTextScale.X / 2), finalTextRect.Bottom + newSplashAcceptTextDistBelowSplashText);

                ImGuiDebugDrawer.DrawText(acceptTextStr,
                    acceptTextPos / Main.DPIVector, Color.Yellow * alpha, fontSize: newFontSize_AcceptText / Main.DPI,
                    shadowColor: Color.Black * alpha);
            }


            //DBG.DrawOutlinedText(SplashText, rect.Center() - (measureText / 2), Color.White * SplashAlpha, scale: newFontSize);
        }

        public static void UpdateSplash(float deltaTime)
        {
            if (SplashState == SplashScreenStates.FadeIn)
            {
                SplashTimer += deltaTime;

                float timerRatio = (SplashTimer / SplashFadeInDuration);
                SplashAlpha = timerRatio;

                if (SplashTimer >= SplashFadeInDuration)
                {
                    SplashState = SplashScreenStates.Showing;
                    SplashTimer = 0;
                    SplashAlpha = 1;
                }
            }
            else if (SplashState == SplashScreenStates.Showing)
            {




                if (SplashTimer >= SplashMinShowBeforeDismiss)
                {
                    UpdateSplashSine(deltaTime);

                    if (Main.Active && Main.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Enter) || Main.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Space) ||
                    Main.Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Escape)
                    || Main.Input.LeftClickHeld)
                        SplashDismissed = true && !Debug_SplashCannotBeDismissed;

                    if (SplashDismissed)
                    {
                        SplashState = SplashScreenStates.FadeOut;
                        SplashTimer = 0;
                        SplashAlpha = 1;
                        Main.Config.HasAcceptedSplashBefore = true;
                        Main.SaveConfig(isManual: false);
                    }

                }
                else
                {
                    SplashTimer += deltaTime;
                }
            }
            else if (SplashState == SplashScreenStates.FadeOut)
            {
                SplashTimer += deltaTime;

                float timerRatio = (SplashTimer / SplashFadeOutDuration);
                SplashAlpha = 1 - timerRatio;

                if (SplashTimer >= SplashFadeOutDuration)
                {
                    SplashState = SplashScreenStates.Complete;
                    SplashTimer = 0;
                    SplashAlpha = 0;
                }
            }
        }
    }
}
