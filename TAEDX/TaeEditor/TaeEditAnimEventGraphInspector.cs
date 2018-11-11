using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeEditAnimEventGraphInspector
    {
        public Rectangle Rect;
        public readonly TaeEditorScreen MainScreen;

        public TaeEditAnimEventGraphInspector(TaeEditorScreen mainScreen)
        {
            MainScreen = mainScreen;
        }

        public void Update(float elapsedSeconds)
        {

        }

        public void UpdateMouseOutsideRect(float elapsedSeconds)
        {

        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            var oldViewport = gd.Viewport;
            gd.Viewport = new Viewport(Rect);
            {
                sb.Begin();

                sb.Draw(texture: boxTex,
                    position: Vector2.Zero,
                    sourceRectangle: null,
                    color: Color.DarkGreen,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2(Rect.Width, Rect.Height),
                    effects: SpriteEffects.None,
                    layerDepth: 0
                    );

                sb.DrawString(font, MainScreen.SelectedEventBox?.EventTextTall ?? "(No Event Selected)", Vector2.One * 8 + Vector2.One, Color.Black);
                sb.DrawString(font, MainScreen.SelectedEventBox?.EventTextTall ?? "(No Event Selected)", Vector2.One * 8, Color.White);



                sb.End();
            }
            gd.Viewport = oldViewport;
        }
    }
}
