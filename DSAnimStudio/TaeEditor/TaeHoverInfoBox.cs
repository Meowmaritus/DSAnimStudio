using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeHoverInfoBox
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (value != _text)
                {
                    _text = value;
                    TextSize = DBG.DEBUG_FONT_SMALL.MeasureString(_text);
                }
            }
        }

        public Vector2 DrawPosition = Vector2.Zero;

        private Vector2 TextSize = Vector2.Zero;
        public float PopupDelay = 0.5f;

        private float PopupTimer = 0;

        public int OutlineThickness = 1;
        public int TextPadding = 4;

        public int BoxShadowOffset = 3;
        

        public Color ColorOutline = Color.White;
        public Color ColorBox = Color.DodgerBlue;
        public Color ColorBoxShadow = Color.Black * 0.5f;
        public Color ColorText = Color.White;
        public Color ColorTextShadow = Color.Black;

        public bool IsVisible => PopupTimer <= 0;

        private bool mousePreviouslyInside = false;

        public void Update(bool mouseInside, float deltaTime)
        {
            if (!mouseInside)
            {
                PopupTimer = PopupDelay;
            }
            else
            {
                if (PopupTimer > 0)
                    PopupTimer -= deltaTime;
                else
                    PopupTimer = 0;
            }

            mousePreviouslyInside = mouseInside;
        }

        public void Draw(SpriteBatch sb, Texture2D squareTex)
        {
            if (IsVisible)
            {
                var rectOutline = new Rectangle((int)DrawPosition.X, (int)DrawPosition.Y,
                  (int)(TextSize.X + (2 * OutlineThickness) + (2 * TextPadding)),
                  (int)(TextSize.Y + (2 * OutlineThickness) + (2 * TextPadding)));

                var rectBoxShadow = new Rectangle(
                    rectOutline.X + BoxShadowOffset, 
                    rectOutline.Y + BoxShadowOffset,
                    rectOutline.Width, rectOutline.Height);

                var rectBG = new Rectangle(rectOutline.X + OutlineThickness,
                    rectOutline.Y + OutlineThickness,
                    rectOutline.Width - (OutlineThickness * 2),
                    rectOutline.Height - (OutlineThickness * 2));

                var textPosition = new Vector2(rectBG.X + TextPadding, rectBG.Y + TextPadding);

                sb.Draw(squareTex, rectBoxShadow, null, ColorBoxShadow);
                sb.Draw(squareTex, rectOutline, null, ColorOutline);
                sb.Draw(squareTex, rectBG, null, ColorBox);
                sb.DrawString(DBG.DEBUG_FONT_SMALL, Text, textPosition + Vector2.One, ColorTextShadow);
                sb.DrawString(DBG.DEBUG_FONT_SMALL, Text, textPosition, ColorText);
            }
        }
    }
}
