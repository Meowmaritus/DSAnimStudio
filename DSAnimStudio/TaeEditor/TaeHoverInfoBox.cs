using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSAnimStudio.TaeEditor
{
    public class TaeHoverInfoBox
    {
        public string Title = "?ToolTipTitle?";

        //private string _text;
        public string Text = "?ToolTipText?";
        //{
        //    get => _text;
        //    set
        //    {
        //        if (value != _text)
        //        {
        //            _text = value;
        //            TextSize = DBG.DEBUG_FONT_SMALL.MeasureString(_text);
        //        }
        //    }
        //}

        public ToolTip Tooltip { get; private set; } = null;
        System.Drawing.Font TooltipFont;
        System.Drawing.Font TooltipTitleFont;
        System.Drawing.Brush TooltipBackgroundBrush;

        //System.Drawing.Brush TooltipBackgroundBrush;

        public Vector2 DrawPosition = Vector2.Zero;

        //private Vector2 TextSize = Vector2.Zero;
        public float PopupDelay = 0.5f;

        private float PopupTimer = 0;

        public int OutlineThickness = 2;
        public int TextPadding = 8;

        public TaeEditAnimEventBox Box;

        public int PaddingBetweenTitleAndText = 4;

        private System.Drawing.Size titleSize;

        //public int BoxShadowOffset = 3;


        public Color ColorOutline => Main.Colors.GuiColorEventGraphHoverInfoBoxOutline;
        public Color ColorBox => Main.Colors.GuiColorEventGraphHoverInfoBoxFill;
        //public Color ColorBoxShadow = Color.Black * 0.5f;
        public Color ColorText => Main.Colors.GuiColorEventGraphHoverInfoBoxText;
        //public Color ColorTextShadow = Color.Black;

        public bool IsVisible => PopupTimer <= 0;

        private bool mousePreviouslyInside = false;

        private bool ToolTipShowing = false;

        public void Update(bool mouseInside, float deltaTime)
        {
            if (!mouseInside)
            {
                PopupTimer = PopupDelay;
                //if (ToolTip != null && ToolTipShowing)
                //{
                //    ToolTip.RemoveAll();
                //    ToolTipShowing = false;
                //}
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


        private System.Drawing.Size MemeGetTextSize(string text, System.Drawing.Font font)
        {
            //return TextRenderer.MeasureText(text, font);

            using (var lbl = new Label())
            {
                lbl.UseCompatibleTextRendering = true;
                lbl.Font = font;
                lbl.Text = text;
                return lbl.GetPreferredSize(new System.Drawing.Size(1000, 1000));
            }
        }

        private void Tooltip_Popup(object sender, PopupEventArgs e)
        {
            titleSize = MemeGetTextSize(Title, TooltipTitleFont);

            var textBodySize = MemeGetTextSize(Text, TooltipFont);

            

            //e.ToolTipSize = new System.Drawing.Size(testControl.Bounds.Width, testControl.Bounds.Height);
            e.ToolTipSize = new System.Drawing.Size((TextPadding * 2) + 
                (Math.Max(titleSize.Width, textBodySize.Width + TextPadding)), 
                (TextPadding * 2) + titleSize.Height + (!string.IsNullOrWhiteSpace(Text) ? (PaddingBetweenTitleAndText + textBodySize.Height) : 0));
        }

        private void ToolTip_Draw(object sender, System.Windows.Forms.DrawToolTipEventArgs e)
        {
            e.DrawBackground();

            using (var pen = new System.Drawing.Pen(
                System.Drawing.Color.FromArgb(255, ColorOutline.R, ColorOutline.G, ColorOutline.B), OutlineThickness))
            {
                e.Graphics.DrawRectangle(pen, 
                    new System.Drawing.Rectangle(e.Bounds.X + (OutlineThickness - 1), e.Bounds.Y + (OutlineThickness - 1), 
                    e.Bounds.Width - OutlineThickness, e.Bounds.Height - OutlineThickness));
            }

            e.Graphics.DrawString(Title, TooltipTitleFont, new System.Drawing.SolidBrush(
                System.Drawing.Color.FromArgb(255, ColorText.R, ColorText.G, ColorText.B)), TextPadding, TextPadding);

            if (!string.IsNullOrWhiteSpace(Text))
                e.Graphics.DrawString(Text, TooltipFont, new System.Drawing.SolidBrush(
                System.Drawing.Color.FromArgb(255, ColorText.R, ColorText.G, ColorText.B)), 
                    TextPadding, TextPadding + titleSize.Height + PaddingBetweenTitleAndText);
        }

        public void UpdateTooltip(System.Windows.Forms.Form form)
        {
            if (Tooltip == null)
            {
                TooltipFont = new System.Drawing.Font("Consolas", 10.0f);
                TooltipTitleFont = new System.Drawing.Font(TooltipFont, System.Drawing.FontStyle.Bold);

                Tooltip = new ToolTip();
                Tooltip.OwnerDraw = true;
                Tooltip.Draw += ToolTip_Draw;
                Tooltip.Popup += Tooltip_Popup;
                Tooltip.BackColor = System.Drawing.Color.FromArgb(255, ColorBox.R, ColorBox.G, ColorBox.B);
                Tooltip.ForeColor = System.Drawing.Color.FromArgb(255, ColorText.R, ColorText.G, ColorText.B);
                TooltipBackgroundBrush = new System.Drawing.SolidBrush(Tooltip.BackColor);
            }

            if (IsVisible)
            {
                if (!ToolTipShowing)
                {
                    Title = Box.GetPopupTitle();
                    Text = Box.GetPopupText();

                    Tooltip.ToolTipTitle = null;
                    Tooltip.ToolTipIcon = ToolTipIcon.None;
                    Tooltip.Show("0", form, (int)DrawPosition.X, (int)DrawPosition.Y);

                    ToolTipShowing = true;
                }
               
            }
            else
            {
                if (ToolTipShowing)
                {
                    Tooltip.RemoveAll();

                    ToolTipShowing = false;
                }
            }

        }
    }
}
