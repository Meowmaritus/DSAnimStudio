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
        public Color ColorHelperDummyPolyDbg { get; set; } = Color.Yellow;
        public Color ColorHelperSoundEvent { get; set; } = Color.Red;

        public Color ColorHelperDummyPolySpawnSFX { get; set; } = Color.Cyan;
        public Color ColorHelperDummyPolySpawnBulletsMisc { get; set; } = Color.Yellow;
        public Color ColorHelperDummyPolySpawnSFXBulletsMisc { get; set; } = Color.Lime;

        // Things that don't need to be read/written:

        public Color ColorHelperCameraPivot { get; set; } = Color.Cyan;

        public Color MainColorBackground { get; set; } = new Color(35, 35, 35, 255);
        public Color MainColorDivider { get; set; } = new Color(90, 90, 90, 255);

        public Color MainColorViewportBackground { get; set; } = new Color(0.1f, 0.1f, 0.1f);



        public Color GuiColorMemoryUseTextGood { get; set; } = Color.Yellow;
        public Color GuiColorMemoryUseTextOkay { get; set; } = Color.Orange;
        public Color GuiColorMemoryUseTextBad { get; set; } = Color.Red;

        public Color GuiColorViewportStatus { get; set; } = Color.Yellow;
        public Color GuiColorViewportStatusMaxBoneCountExceeded { get; set; } = Color.Orange;
        public Color GuiColorViewportStatusAnimDoesntExist { get; set; } = Color.Red;
        public Color GuiColorViewportStatusCombo { get; set; } = Color.Cyan;



        public Color GuiColorEventBox_Normal_Fill { get; set; } = new Color(80, 80, 80, 255);
        public Color GuiColorEventBox_Normal_Outline { get; set; } = Color.Black;
        public Color GuiColorEventBox_Normal_Text { get; set; } = Color.White;
        public Color GuiColorEventBox_Normal_TextShadow { get; set; } = Color.Black;

        public Color GuiColorEventBox_Highlighted_Fill { get; set; } = new Color((30.0f / 255.0f) * 0.75f, (144.0f / 255.0f) * 0.75f, 1 * 0.75f, 1);
        public Color GuiColorEventBox_Highlighted_Outline { get; set; } = Color.Yellow;
        public Color GuiColorEventBox_Highlighted_Text { get; set; } = Color.Yellow;
        public Color GuiColorEventBox_Highlighted_TextShadow { get; set; } = Color.Black;

        public Color GuiColorEventBox_Hover_TextOutline { get; set; } = Color.Black;

        public Color GuiColorEventBox_SelectionDimmingOverlay { get; set; } = Color.Black * 0.5f;

        public Color GuiColorEventGraphBackground { get; set; } = new Color(120, 120, 120, 255);
        public Color GuiColorEventGraphGhostOverlay { get; set; } = new Color(116, 116, 116, 188);
        public Color GuiColorEventGraphAnimEndVerticalLine { get; set; } = Color.White;
        public Color GuiColorEventGraphAnimEndDarkenRect { get; set; } = Color.Black * 0.25f;
        public Color GuiColorEventGraphRowHorizontalLines { get; set; } = Color.Black * 0.25f;
        public Color GuiColorEventGraphTimelineFill { get; set; } = new Color(75, 75, 75, 255);
        public Color GuiColorEventGraphTimelineFrameVerticalLines { get; set; } = Color.Black * 0.125f;
        public Color GuiColorEventGraphTimelineFrameNumberText { get; set; } = Color.White;
        public Color GuiColorEventGraphVerticalFrameLines { get; set; } = Color.LightGray * 0.5f;
        public Color GuiColorEventGraphVerticalSecondLines { get; set; } = Color.LightGray * 0.75f;
        public Color GuiColorEventGraphSelectionRectangleFill { get; set; } = Color.DodgerBlue * 0.5f;
        public Color GuiColorEventGraphSelectionRectangleOutline { get; set; } = Color.White;
        public Color GuiColorEventGraphSliceToolLine { get; set; } = Color.Cyan;
        public Color GuiColorEventGraphPlaybackCursor { get; set; } = Color.Black;
        public Color GuiColorEventGraphPlaybackStartTime { get; set; } = Color.Blue;
        public Color GuiColorEventGraphHoverInfoBoxFill { get; set; } = new Color(64, 64, 64, 1);
        public Color GuiColorEventGraphHoverInfoBoxText { get; set; } = Color.White;
        public Color GuiColorEventGraphHoverInfoBoxOutline { get; set; } = new Color(32, 32, 32, 1);

        public Color GuiColorEventGraphScrollbarBackground { get; set; } = new Color(0.25f, 0.25f, 0.25f);
        public Color GuiColorEventGraphScrollbarForegroundInactive { get; set; } = new Color(0.45f, 0.45f, 0.45f);
        public Color GuiColorEventGraphScrollbarForegroundActive { get; set; } = new Color(0.55f, 0.55f, 0.55f);
        public Color GuiColorEventGraphScrollbarArrowButtonForegroundInactive { get; set; } = new Color(0.35f, 0.35f, 0.35f);
        public Color GuiColorEventGraphScrollbarArrowButtonForegroundActive { get; set; } = new Color(0.45f, 0.45f, 0.45f);

        public Color GuiColorAnimListCollapsePlusMinusForeground { get; set; } = Color.Black;
        public Color GuiColorAnimListCollapsePlusMinusBackground { get; set; } = Color.White;
        public Color GuiColorAnimListAnimSectionHeaderRectOutline { get; set; } = Color.White;
        public Color GuiColorAnimListAnimSectionHeaderRectFill { get; set; } = Color.Gray;
        public Color GuiColorAnimListTextAnimSectionName { get; set; } = Color.White;
        public Color GuiColorAnimListTextAnimName { get; set; } = Color.White;
        public Color GuiColorAnimListTextAnimNameMinBlend { get; set; } = Color.Gray;
        public Color GuiColorAnimListTextAnimNameMaxBlend { get; set; } = Color.Yellow;
        public Color GuiColorAnimListTextAnimDevName { get; set; } = Color.PaleGoldenrod;
        public Color GuiColorAnimListTextShadow { get; set; } = Color.Black;
        
        public Color GuiColorAnimListHighlightRectFill { get; set; } = Color.DodgerBlue;
        public Color GuiColorAnimListHighlightRectOutline { get; set; } = new Color(200, 200, 200);

        public Color ColorHelperRootMotionStartLocation { get; set; } = Color.Red;
        public Color ColorHelperRootMotionTrail { get; set; } = Color.Yellow;
        public Color ColorHelperRootMotionCurrentLocation { get; set; } = Color.Lime;

        public Color ColorHelperRootMotionStartLocation_PrevLoop { get; set; } = new Color(Color.Red.ToVector4() * new Vector4(1, 1, 1, 0.5f));
        public Color ColorHelperRootMotionTrail_PrevLoop { get; set; } = new Color(Color.Yellow.ToVector4() * new Vector4(1, 1, 1, 0.5f));
        public Color ColorHelperRootMotionCurrentLocation_PrevLoop { get; set; } = new Color(Color.Lime.ToVector4() * new Vector4(1, 1, 1, 0.5f));


        public void ReadColorsFromConfig()
        {
            DBG.DbgPrim_Grid_XZ.OverrideColor = ColorGrid;

            ParamData.AtkParam.Hit.ColorRoot = ColorHelperHitboxRoot;
            ParamData.AtkParam.Hit.ColorMiddle = ColorHelperHitboxMiddle;
            ParamData.AtkParam.Hit.ColorTip = ColorHelperHitboxTip;

            DBG.COLOR_FLVER_BONE = ColorHelperFlverBone;
            DBG.COLOR_FLVER_BONE_BBOX = ColorHelperFlverBoneBoundingBox;
            DBG.COLOR_DUMMY_POLY = ColorHelperDummyPoly;
            DBG.COLOR_DUMMY_POLY_DBG = ColorHelperDummyPolyDbg;
            DBG.COLOR_SOUND_EVENT = ColorHelperSoundEvent;

            NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFX = ColorHelperDummyPolySpawnSFX;
            NewDummyPolyManager.DummyPolyInfo.ColorSpawnBulletsMisc = ColorHelperDummyPolySpawnBulletsMisc;
            NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFXBulletsMisc = ColorHelperDummyPolySpawnSFXBulletsMisc;
        }

        public void WriteColorsToConfig()
        {
            ColorGrid = DBG.DbgPrim_Grid_XZ?.OverrideColor ?? new Color(32, 112, 39);

            ColorHelperHitboxRoot = ParamData.AtkParam.Hit.ColorRoot;
            ColorHelperHitboxMiddle = ParamData.AtkParam.Hit.ColorMiddle;
            ColorHelperHitboxTip = ParamData.AtkParam.Hit.ColorTip;

            ColorHelperFlverBone = DBG.COLOR_FLVER_BONE;
            ColorHelperFlverBoneBoundingBox = DBG.COLOR_FLVER_BONE_BBOX;
            ColorHelperDummyPoly = DBG.COLOR_DUMMY_POLY;
            ColorHelperDummyPolyDbg = DBG.COLOR_DUMMY_POLY_DBG;
            ColorHelperSoundEvent = DBG.COLOR_SOUND_EVENT;

            ColorHelperDummyPolySpawnSFX = NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFX;
            ColorHelperDummyPolySpawnBulletsMisc = NewDummyPolyManager.DummyPolyInfo.ColorSpawnBulletsMisc;
            ColorHelperDummyPolySpawnSFXBulletsMisc = NewDummyPolyManager.DummyPolyInfo.ColorSpawnSFXBulletsMisc;
        }
    }
}
