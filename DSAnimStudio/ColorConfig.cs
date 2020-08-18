using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class ColorConfig
    {
        public Color ColorGrid { get; set; } = new Color(32, 112, 39);

        public Color ColorHelperHitboxRoot { get; set; } = new Color(26, 26, 230);
        public Color ColorHelperHitboxMiddle { get; set; } = new Color(230, 26, 26);
        public Color ColorHelperHitboxTip { get; set; } = new Color(231, 186, 50);

        public Color ColorHelperFlverBone { get; set; } = Color.Yellow;
        public Color ColorHelperFlverBoneBoundingBox { get; set; } = Color.Lime;
        public Color ColorHelperDummyPoly { get; set; } = Color.MonoGameOrange;
        public Color ColorHelperSoundEvent { get; set; } = Color.Red;

        public Color ColorHelperDummyPolySpawnSFX { get; set; } = Color.Cyan;
        public Color ColorHelperDummyPolySpawnBulletsMisc { get; set; } = Color.Yellow;
        public Color ColorHelperDummyPolySpawnSFXBulletsMisc { get; set; } = Color.Lime;

        // Things that don't need to be read/written:
        public Color MainColorBackground = new Color(0.2f, 0.2f, 0.2f);

        public Color MainColorViewportBackground = Color.DimGray;



        public Color GuiColorMemoryUseTextGood = Color.Yellow;
        public Color GuiColorMemoryUseTextOkay = Color.Orange;
        public Color GuiColorMemoryUseTextBad = Color.Red;

        public Color GuiColorViewportStatus = Color.Yellow;
        public Color GuiColorViewportStatusMaxBoneCountExceeded = Color.Orange;
        public Color GuiColorViewportStatusAnimDoesntExist = Color.Red;
        public Color GuiColorViewportStatusCombo = Color.Cyan;



        public Color GuiColorEventBox_Normal_Fill = new Color(80, 80, 80, 255);
        public Color GuiColorEventBox_Normal_Outline = Color.Black;
        public Color GuiColorEventBox_Normal_Text = Color.White;
        public Color GuiColorEventBox_Normal_TextShadow = Color.Black;

        public Color GuiColorEventBox_Highlighted_Fill = new Color((30.0f / 255.0f) * 0.75f, (144.0f / 255.0f) * 0.75f, 1 * 0.75f, 1);
        public Color GuiColorEventBox_Highlighted_Outline = Color.Yellow;
        public Color GuiColorEventBox_Highlighted_Text = Color.Yellow;
        public Color GuiColorEventBox_Highlighted_TextShadow = Color.Black;

        public Color GuiColorEventBox_Hover_TextOutline = Color.Black;

        public Color GuiColorEventBox_SelectionDimmingOverlay = Color.Black * 0.5f;

        public Color GuiColorEventGraphBackground = new Color(120, 120, 120, 255);
        public Color GuiColorEventGraphGhostOverlay = new Color(120, 120, 120, 255) * 0.5f;
        public Color GuiColorEventGraphAnimEndVerticalLine = Color.White;
        public Color GuiColorEventGraphAnimEndDarkenRect = Color.Black * 0.25f;
        public Color GuiColorEventGraphRowHorizontalLines = Color.Black * 0.25f;
        public Color GuiColorEventGraphTimelineFill = new Color(75, 75, 75, 255);
        public Color GuiColorEventGraphTimelineFrameVerticalLines = Color.Black * 0.125f;
        public Color GuiColorEventGraphTimelineFrameNumberText = Color.White;
        public Color GuiColorEventGraphVerticalFrameLines = Color.LightGray * 0.5f;
        public Color GuiColorEventGraphVerticalSecondLines = Color.LightGray * 0.75f;
        public Color GuiColorEventGraphSelectionRectangleFill = Color.DodgerBlue * 0.5f;
        public Color GuiColorEventGraphSelectionRectangleOutline = Color.White;
        public Color GuiColorEventGraphPlaybackCursor = Color.Black;
        public Color GuiColorEventGraphPlaybackStartTime = Color.Blue;
        public Color GuiColorEventGraphHoverInfoBoxFill = new Color(64, 64, 64, 1);
        public Color GuiColorEventGraphHoverInfoBoxText = Color.White;
        public Color GuiColorEventGraphHoverInfoBoxOutline = new Color(32, 32, 32, 1);

        public Color GuiColorEventGraphScrollbarBackground = new Color(0.25f, 0.25f, 0.25f);
        public Color GuiColorEventGraphScrollbarForegroundInactive = new Color(0.45f, 0.45f, 0.45f);
        public Color GuiColorEventGraphScrollbarForegroundActive = new Color(0.55f, 0.55f, 0.55f);
        public Color GuiColorEventGraphScrollbarArrowButtonForegroundInactive = new Color(0.35f, 0.35f, 0.35f);
        public Color GuiColorEventGraphScrollbarArrowButtonForegroundActive = new Color(0.45f, 0.45f, 0.45f);

        public void ReadColorsFromConfig()
        {
            DBG.DbgPrim_Grid.OverrideColor = ColorGrid;

            ParamData.AtkParam.Hit.ColorRoot = ColorHelperHitboxRoot;
            ParamData.AtkParam.Hit.ColorMiddle = ColorHelperHitboxMiddle;
            ParamData.AtkParam.Hit.ColorTip = ColorHelperHitboxTip;

            DBG.COLOR_FLVER_BONE = ColorHelperFlverBone;
            DBG.COLOR_FLVER_BONE_BBOX = ColorHelperFlverBoneBoundingBox;
            DBG.COLOR_DUMMY_POLY = ColorHelperDummyPoly;
            DBG.COLOR_SOUND_EVENT = ColorHelperSoundEvent;

            NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFX = ColorHelperDummyPolySpawnSFX;
            NewDummyPolyManager.DummyPolyInfo.ColorSpawnBulletsMisc = ColorHelperDummyPolySpawnBulletsMisc;
            NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFXBulletsMisc = ColorHelperDummyPolySpawnSFXBulletsMisc;
        }

        public void WriteColorsToConfig()
        {
            ColorGrid = DBG.DbgPrim_Grid.OverrideColor ?? new Color(32, 112, 39);

            ColorHelperHitboxRoot = ParamData.AtkParam.Hit.ColorRoot;
            ColorHelperHitboxMiddle = ParamData.AtkParam.Hit.ColorMiddle;
            ColorHelperHitboxTip = ParamData.AtkParam.Hit.ColorTip;

            ColorHelperFlverBone = DBG.COLOR_FLVER_BONE;
            ColorHelperFlverBoneBoundingBox = DBG.COLOR_FLVER_BONE_BBOX;
            ColorHelperDummyPoly = DBG.COLOR_DUMMY_POLY;
            ColorHelperSoundEvent = DBG.COLOR_SOUND_EVENT;

            ColorHelperDummyPolySpawnSFX = NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFX;
            ColorHelperDummyPolySpawnBulletsMisc = NewDummyPolyManager.DummyPolyInfo.ColorSpawnBulletsMisc;
            ColorHelperDummyPolySpawnSFXBulletsMisc = NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFXBulletsMisc;
        }
    }
}
