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
    public class TaeEditorScreen
    {
        public Rectangle Rect;

        private int LayoutAnimListWidth = 256;
        private int LayoutInspectorWidth = 320;
        private int LayoutPad = 8;

        public readonly TAE Tae;

        public AnimationRef TaeAnim { get; private set; }
        public TaeEditAnimEventBox SelectedEventBox;

        private TaeEditAnimList editScreenAnimList;
        private TaeEditAnimEventGraph editScreenCurrentAnim;
        private TaeEditAnimEventGraphInspector editScreenGraphInspector;

        public TaeInputHandler Input;

        public TaeEditorScreen(TAE tae)
        {
            Tae = tae;
            TaeAnim = Tae.Animations[0];

            Input = new TaeInputHandler();

            editScreenAnimList = new TaeEditAnimList(this);
            editScreenCurrentAnim = new TaeEditAnimEventGraph(this);
            editScreenGraphInspector = new TaeEditAnimEventGraphInspector(this);
        }

        public void SelectNewAnimRef(AnimationRef animRef)
        {
            TaeAnim = animRef;
            SelectedEventBox = null;
            editScreenCurrentAnim.ChangeToNewAnimRef(TaeAnim);
        }

        public void Update(float elapsedSeconds)
        {
            Input.Update(Rect);

            if (editScreenAnimList.Rect.Contains(Input.MousePositionPoint))
                editScreenAnimList.Update(elapsedSeconds);
            else
                editScreenAnimList.UpdateMouseOutsideRect(elapsedSeconds);

            if (editScreenCurrentAnim.Rect.Contains(Input.MousePositionPoint))
                editScreenCurrentAnim.Update(elapsedSeconds);
            else
                editScreenCurrentAnim.UpdateMouseOutsideRect(elapsedSeconds);

            if (editScreenGraphInspector.Rect.Contains(Input.MousePositionPoint))
                editScreenGraphInspector.Update(elapsedSeconds);
            else
                editScreenGraphInspector.UpdateMouseOutsideRect(elapsedSeconds);
        }

        private void UpdateLayout()
        {
            editScreenAnimList.Rect = new Rectangle(0, 0, LayoutAnimListWidth, Rect.Height);
            editScreenCurrentAnim.Rect = new Rectangle(LayoutAnimListWidth + LayoutPad, 0,
                Rect.Width - LayoutAnimListWidth - LayoutInspectorWidth - (LayoutPad * 2), Rect.Height);
            editScreenGraphInspector.Rect = new Rectangle(Rect.Width - LayoutInspectorWidth, 0, LayoutInspectorWidth, Rect.Height);
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font)
        {
            throw new Exception("Make left/right edges of events line up to same vertical lines so the rounding doesnt make them 1 pixel off");
            throw new Exception("Make dragging edges of scrollbar box do zoom");
            throw new Exception("make ctrl+scrolll zoom centered on mouse cursor pos");

            UpdateLayout();
            editScreenAnimList.Draw(gd, sb, boxTex, font);
            editScreenCurrentAnim.Draw(gd, sb, boxTex, font);
            editScreenGraphInspector.Draw(gd, sb, boxTex, font);
        }
    }
}
