using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.TAE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeEditAnimList
    {
        public Rectangle Rect;
        public readonly TaeEditorScreen MainScreen;

        private TaeScrollViewer ScrollViewer;

        private int AnimHeight = 24;

        private Dictionary<string, AnimationRef> AnimNameMap = new Dictionary<string, AnimationRef>();

        public TaeEditAnimList(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;

            int i = 0;
            foreach (var anim in mainScreen.Tae.Animations)
            {
                AnimNameMap.Add($"{anim.ID} ({(anim.Anim.IsReference ? $"References Anim {anim.Anim.RefAnimID}" : $"\"{anim.Anim.FileName}\"")})", anim);
            }

            ScrollViewer = new TaeScrollViewer();
        }

        public void Update(float elapsedSeconds)
        {
            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, true);

            if (Rect.Contains(MainScreen.Input.MousePositionPoint))
            {
                MainScreen.Input.CursorType = MouseCursorType.Arrow;

                if (MainScreen.Input.LeftClickDown)
                {
                    var mouseCheckPoint = new Point(
                        (int)(MainScreen.Input.MousePosition.X - Rect.X + ScrollViewer.Scroll.X),
                        (int)(MainScreen.Input.MousePosition.Y - Rect.Y + ScrollViewer.Scroll.Y));

                    int i = 0;
                    foreach (var anim in AnimNameMap)
                    {
                        var thisAnimRect = new Rectangle(0, i * AnimHeight, ScrollViewer.Viewport.Width, AnimHeight);
                        if (thisAnimRect.Contains(mouseCheckPoint))
                            MainScreen.SelectNewAnimRef(anim.Value);
                        i++;
                    }
                }
            }

            
        }

        public void UpdateMouseOutsideRect(float elapsedSeconds)
        {
            ScrollViewer.UpdateInput(MainScreen.Input, elapsedSeconds, allowScrollWheel: false);
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            ScrollViewer.SetDisplayRect(Rect, new Point(Rect.Width, AnimNameMap.Count * AnimHeight));

            ScrollViewer.Draw(gd, sb, boxTex, font);

            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(ScrollViewer.Viewport);
            {
                sb.Begin(transformMatrix: ScrollViewer.GetScrollMatrix());

                //sb.Draw(texture: boxTex,
                //    position: Vector2.Zero,
                //    sourceRectangle: null,
                //    color: Color.DarkGray,
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(Rect.Width, Rect.Height),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                int i = 0;
                foreach (var anim in AnimNameMap)
                {
                    if (anim.Value == MainScreen.TaeAnim)
                    {
                        var thisAnimRect = new Rectangle(0, i * AnimHeight, ScrollViewer.Viewport.Width, AnimHeight);
                        sb.Draw(boxTex, thisAnimRect, Color.CornflowerBlue);
                    }

                    sb.DrawString(font, anim.Key, new Vector2(4, (int)(i * AnimHeight)) + Vector2.One, Color.Black);
                    sb.DrawString(font, anim.Key, new Vector2(4, (int)(i * AnimHeight)), Color.White);

                    i++;
                }

                sb.End();
            }
            gd.Viewport = oldViewport;
        }
    }
}
