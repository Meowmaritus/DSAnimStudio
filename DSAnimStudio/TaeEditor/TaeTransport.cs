using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeTransport
    {
        public Rectangle Rect = new Rectangle(0, 0, 64, 64);

        public TaeEditorScreen MainScreen;

        List<TransportButton> Buttons = new List<TransportButton>();

        TaePlaybackCursor PlaybackCursor => MainScreen.Graph.PlaybackCursor;

        int ButtonCageThickness = 4;
        int ButtonSeparatorThickness = 2;
        int ButtonSize = 22;

        public TaeTransport(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;

            Buttons.Add(new TransportButton()
            {
                GetDebugText = () => "|<<",
                GetIsEnabled = () => PlaybackCursor.IsPlaying || (PlaybackCursor.CurrentTime != 0),
                OnClick = () =>
                {
                    PlaybackCursor.IsPlaying = false;
                    PlaybackCursor.CurrentTime = 0;
                    MainScreen.Graph?.ViewportInteractor?.CurrentModel?.AnimContainer?.ResetAll();
                    PlaybackCursor.StartTime = 0;
                    MainScreen.Graph.ViewportInteractor.OnScrubFrameChange(forceCustomTimeDelta: 0);
                    MainScreen.Graph.ViewportInteractor.CurrentModel.AnimContainer.Skeleton.CurrentDirection = 0;

                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel0?.AnimContainer?.Skeleton?.ResetCurrentDirection();
                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel1?.AnimContainer?.Skeleton?.ResetCurrentDirection();
                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel2?.AnimContainer?.Skeleton?.ResetCurrentDirection();
                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.RightWeaponModel3?.AnimContainer?.Skeleton?.ResetCurrentDirection();

                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel0?.AnimContainer?.Skeleton?.ResetCurrentDirection();
                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel1?.AnimContainer?.Skeleton?.ResetCurrentDirection();
                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel2?.AnimContainer?.Skeleton?.ResetCurrentDirection();
                    MainScreen.Graph.ViewportInteractor.CurrentModel.ChrAsm?.LeftWeaponModel3?.AnimContainer?.Skeleton?.ResetCurrentDirection();

                    MainScreen.Graph.ViewportInteractor.ResetRootMotion();
                    MainScreen.Graph.ScrollToPlaybackCursor(1);
                    PlaybackCursor.IgnoreCurrentRelativeScrub();
                },
                GetHotkey = b => MainScreen.Input.KeyHeld(Keys.Home),
            });

            Buttons.Add(new TransportButton()
            {
                GetDebugText = () => "[]",
                GetIsEnabled = () => PlaybackCursor.CurrentTime != PlaybackCursor.StartTime,//MainScreen.Graph.PlaybackCursor.IsPlaying,
                OnClick = () =>
                {
                    var start = PlaybackCursor.StartTime;
                    PlaybackCursor.IsPlaying = false;
                    PlaybackCursor.CurrentTime = PlaybackCursor.StartTime;
                    //MainScreen.Graph.ViewportInteractor.OnScrubFrameChange(forceCustomTimeDelta: 0);
                    MainScreen.Graph.PlaybackCursor.UpdateScrubbing();
                    //MainScreen.Graph.ViewportInteractor.ResetRootMotion((float)MainScreen.Graph.PlaybackCursor.StartTime);
                    MainScreen.Graph.ScrollToPlaybackCursor(1);
                },
                GetHotkey = b => (MainScreen.Input.ShiftHeld && !MainScreen.Input.AltHeld && !MainScreen.Input.CtrlHeld) && MainScreen.Input.KeyHeld(Keys.Space),
            });

            Buttons.Add(new TransportButton()
            {
                GetDebugText = () => PlaybackCursor.IsPlaying ? "||" : ">",
                GetIsEnabled = () => true,
                OnClick = () => PlaybackCursor.Transport_PlayPause(),
                GetHotkey = b => (!MainScreen.Input.ShiftHeld && !MainScreen.Input.AltHeld && !MainScreen.Input.CtrlHeld) && MainScreen.Input.KeyDown(Keys.Space),
            });


            Buttons.Add(new TransportButton()
            {
                GetDebugText = () => ">>|",
                GetIsEnabled = () => PlaybackCursor.IsPlaying || (PlaybackCursor.CurrentTime != PlaybackCursor.MaxTime),
                OnClick = () =>
                {
                    PlaybackCursor.IsPlaying = false;
                    PlaybackCursor.CurrentTime = PlaybackCursor.MaxTime;
                    PlaybackCursor.StartTime = PlaybackCursor.MaxTime;
                    MainScreen.Graph.PlaybackCursor.UpdateScrubbing();
                    //MainScreen.Graph.ViewportInteractor.ResetRootMotion((float)MainScreen.Graph.PlaybackCursor.MaxTime);
                    MainScreen.Graph.ScrollToPlaybackCursor(1);
                },
                GetHotkey = b => MainScreen.Input.KeyHeld(Keys.End),
            });


            Buttons.Add(new TransportButtonSeparator());

            Buttons.Add(new TransportButton()
            {
                GetDebugText = () => "<|",
                GetIsEnabled = () => true,
                OnClick = () => MainScreen.TransportPreviousFrame(),
                GetHotkey = b =>
                {
                    if (MainScreen.Input.KeyUp(Keys.Left))
                    {
                        b.prevState = b.state = TransportButton.TransportButtonState.Normal;
                    }
                    return MainScreen.Input.KeyHeld(Keys.Left);
                },
            });

            Buttons.Add(new TransportButton()
            {
                GetDebugText = () => "|>",
                GetIsEnabled = () => true,
                OnClick = () => MainScreen.TransportNextFrame(),
                GetHotkey = b =>
                {
                    if (MainScreen.Input.KeyUp(Keys.Right))
                    {
                        b.prevState = b.state = TransportButton.TransportButtonState.Normal;
                    }
                    return MainScreen.Input.KeyHeld(Keys.Right);
                },
            });

        }

        public void LoadContent(ContentManager c)
        {

        }

        public void Update(float elapsedSeconds)
        {
            if (MainScreen.Graph == null)
                return;

            //// Count + 1 because for example 4 buttons would have these cages: |B|B|B|B|
            //// Notice 5 |'s but only 4 B's
            //int buttonCageWidth = ((Buttons.Count + 1) * ButtonCageThickness) + (ButtonSize * Buttons.Count);

            int buttonCageWidth = ButtonCageThickness;

            foreach (var thing in Buttons)
            {
                if (thing.IsSeparator)
                {
                    buttonCageWidth += ButtonSeparatorThickness;
                    buttonCageWidth += ButtonCageThickness;
                }
                else
                {
                    buttonCageWidth += ButtonSize;
                    buttonCageWidth += ButtonCageThickness;
                }
            }

            int buttonCageHeight = ButtonSize + (ButtonCageThickness * 2);
            int buttonCageStartX = Rect.Width - buttonCageWidth - 8;
            int buttonCageStartY = (int)Math.Round((Rect.Height / 2.0) - (buttonCageHeight / 2.0));

            int horizontalOffset = buttonCageStartX;

            for (int i = 0; i < Buttons.Count; i++)
            {
                if (Buttons[i].IsSeparator)
                {
                    horizontalOffset += ButtonCageThickness;

                    Buttons[i].Rect = new Rectangle(horizontalOffset,
                    buttonCageStartY + ButtonCageThickness,
                    ButtonSeparatorThickness,
                    ButtonSize);

                    horizontalOffset += ButtonSeparatorThickness;
                }
                else
                {
                    horizontalOffset += ButtonCageThickness;

                    Buttons[i].Rect = new Rectangle(horizontalOffset,
                    buttonCageStartY + ButtonCageThickness,
                    ButtonSize,
                    ButtonSize);

                    horizontalOffset += ButtonSize;

                    
                }

                

                Buttons[i].Update(MainScreen.Input, this);
            }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont smallFont)
        {
            if (MainScreen.Graph == null)
                return;

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(Rect.DpiScaled());
            {
                sb.Begin(transformMatrix: Main.DPIMatrix);
                try
                {
                    var str = MainScreen.Graph.PlaybackCursor.GetFrameCounterText(MainScreen.Config.LockFramerateToOriginalAnimFramerate);
                    //sb.Draw(boxTex, new Rectangle(0, 0, Rect.Width, Rect.Height), Color.DarkGray);
                    var strSize = smallFont.MeasureString(str);

                    sb.DrawString(smallFont, str, new Vector2(8, (float)Math.Round((Rect.Height / 2.0) - (strSize.Y / 2))), Color.Yellow);

                    for (int i = 0; i < Buttons.Count; i++)
                    {
                        Buttons[i].Draw(sb, boxTex, smallFont);
                    }
                }
                finally { sb.End(); }
            }
            gd.Viewport = oldViewport;
        }

        public class TransportButtonSeparator : TransportButton
        {
            public override bool IsSeparator => true;
        }

        public class TransportButton
        {
            public enum TransportButtonState
            {
                Normal,
                Hover,
                HoldingClick
            }

            public virtual bool IsSeparator => false;

            public Rectangle Rect;

            public Func<string> GetDebugText;
            public Func<bool> GetIsEnabled;
            public Action OnClick;

            public TransportButtonState state;

            public string InfoName = "?ButtonName?";
            public string InfoDescription = "?ButtonDesc?";

            public Func<TransportButton, bool> GetHotkey;

            public TransportButtonState prevState = TransportButtonState.Normal;

            private bool prevMouseHover;

            public void Update(FancyInputHandler input, TaeTransport parentTransport)
            {
                if (IsSeparator)
                    return;

                if (!(GetIsEnabled?.Invoke() ?? true) || Rect.IsEmpty)
                {
                    state = prevState = TransportButtonState.Normal;
                    return;
                }

                bool hotkeyPressed = GetHotkey?.Invoke(this) ?? false;

                var relativeMousePos = input.MousePositionPoint - parentTransport.Rect.Location;

                bool mouseHover = Rect.Contains(relativeMousePos);

                if (mouseHover)
                {
                    if (input.LeftClickDown && state != TransportButtonState.HoldingClick)
                    {
                        state = TransportButtonState.HoldingClick;
                    }
                    else if (!input.LeftClickHeld)
                    {
                        state = TransportButtonState.Hover;
                    }
                }
                else
                {

                    if (prevMouseHover)
                    {
                        state = prevState = TransportButtonState.Normal;
                    }

                    
                }

                //if (input.LeftClickHeld && !hotkeyPressed)
                //    prevState = state;

                if (hotkeyPressed)
                {
                    state = TransportButtonState.HoldingClick;
                }
                else if (!mouseHover)
                {
                    state = TransportButtonState.Normal;
                }

                if (state != TransportButtonState.HoldingClick && prevState == TransportButtonState.HoldingClick)
                {
                    OnClick?.Invoke();
                }

                prevState = state;
                prevMouseHover = mouseHover;
            }

            public void Draw(SpriteBatch sb, Texture2D boxTex, SpriteFont smallFont)
            {
                if (Rect.IsEmpty)
                    return;

                if (IsSeparator)
                {
                    sb.Draw(boxTex, Rect, Color.DarkGray * 0.35f);

                    return;
                }

                Color bgColor = Color.DarkGray;
                Color fgColor = Color.White;

                if (state == TransportButtonState.Normal)
                {
                    bgColor = Color.Gray;
                    fgColor = Color.White;
                }
                else if (state == TransportButtonState.Hover)
                {
                    bgColor = Color.DarkGray;
                    fgColor = Color.White;
                }
                else if (state == TransportButtonState.HoldingClick)
                {
                    bgColor = Color.White;
                    fgColor = Color.Black;
                }

                if (!(GetIsEnabled?.Invoke() ?? true))
                {
                    bgColor *= 0.5f;
                    fgColor *= 0.5f;
                }

                sb.Draw(boxTex, Rect, bgColor);

                string dbgTxt = GetDebugText?.Invoke();

                if (dbgTxt != null)
                {
                    var txtSize = smallFont.MeasureString(dbgTxt);
                    sb.DrawString(smallFont, dbgTxt, new Vector2(
                        (float)Math.Round(Rect.X + (Rect.Width / 2f) - (txtSize.X / 2)),
                        (float)Math.Round(Rect.Y + (Rect.Height / 2f) - (txtSize.Y / 2))), fgColor);
                }

                
            }
        }
    }
}
