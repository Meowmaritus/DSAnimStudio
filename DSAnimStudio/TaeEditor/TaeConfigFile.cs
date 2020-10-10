using DSAnimStudio.DebugPrimitives;
using Microsoft.Xna.Framework;
using SoulsAssetPipeline.FLVERImporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSAnimStudio.TaeEditor
{
    public class TaeConfigFile
    {
        public ParamData.AtkParam.DummyPolySource HitViewDummyPolySource { get; set; } = ParamData.AtkParam.DummyPolySource.RightWeapon0;

        public bool UseGamesMenuSounds { get; set; } = true;

        public bool DbgPrimXRay { get; set; } = true;
        public enum EventSnapTypes
        {
            None,
            FPS30,
            FPS60,
        }
        public EventSnapTypes EventSnapType { get; set; } = EventSnapTypes.FPS30;
        public bool LockFramerateToOriginalAnimFramerate { get; set; } = false;
        public bool EnableFancyScrollingStrings { get; set; } = true;
        public float FancyScrollingStringsScrollSpeed { get; set; } = 64;
        public bool FancyTextScrollSnapsToPixels { get; set; } = true;
        public bool AutoCollapseAllTaeSections { get; set; } = false;
        public bool AutoScrollDuringAnimPlayback { get; set; } = true;
        public bool SoloHighlightEventOnHover { get; set; } = true;
        public bool ShowEventHoverInfo { get; set; } = true;
        public List<string> RecentFilesList { get; set; } = new List<string>();
        public bool LiveRefreshOnSave { get; set; } = false;
        public bool CameraFollowsRootMotion { get; set; } = true;
        public bool CameraFollowsRootMotionRotation { get; set; } = true;
        public bool CameraPansVerticallyToFollowModel { get; set; } = true;
        public bool WrapRootMotion { get; set; } = true;
        public bool AccumulateRootMotion { get; set; } = true;
        public bool EnableAnimRootMotion { get; set; } = true;
        //public bool SimulateReferencedEvents { get; set; } = true;

        public bool CamAngleSnapEnable { get; set; } = false;
        public bool ShowCameraPivotCube { get; set; } = false;

        public float ViewportStatusTextSize { get; set; } = 100;
        public float ToolboxGuiScale { get; set; } = 100;
        public float ToolboxItemWidthScale { get; set; } = 100;

        public ColorConfig Colors { get; set; } = new ColorConfig();

        public float MouseSpeedMult { get; set; } = 1;


        public bool IsNewGraphVisiMode { get; set; } = true;

        public int MSAA = 2;
        public int SSAA = 1;

        public SapImportConfigs.ImportConfigFlver2 LastUsedImportConfig_FLVER2 { get; set; } 
            = new SapImportConfigs.ImportConfigFlver2();

        public Dictionary<string, bool> EventSimulationsEnabled { get; set; }
            = new Dictionary<string, bool>();

        public string LastCubemapUsed { get; set; } = "DefaultCubemap";
        public float SkyboxBrightness { get; set; } = 0.25f;
        public bool ShowCubemapAsSkybox { get; set; } = true;

        public Dictionary<string, bool> CategoryEnableDraw = new Dictionary<string, bool>();
        public Dictionary<string, bool> CategoryEnableDbgLabelDraw = new Dictionary<string, bool>();
        public Dictionary<string, bool> CategoryEnableNameDraw = new Dictionary<string, bool>();

        public bool ShowFullWeaponDummyPolyIDs { get; set; } = true;

        public float LayoutAnimListWidth { get; set; } = 236;
        public float LayoutViewportWidth { get; set; } = 600;
        public float LayoutViewportHeight { get; set; } = 600;

        public int LayoutWindowWidth { get; set; } = 1280;
        public int LayoutWindowHeight { get; set; } = 720;

        public void BeforeSaving(TaeEditorScreen editor)
        {
            CategoryEnableDraw = DBG.CategoryEnableDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            CategoryEnableDbgLabelDraw = DBG.CategoryEnableDbgLabelDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            CategoryEnableNameDraw = DBG.CategoryEnableNameDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);

            LastCubemapUsed = Environment.CurrentCubemapName;

            MSAA = GFX.MSAA;
            SSAA = GFX.SSAA;

            Colors = Main.Colors;
            Colors.WriteColorsToConfig();

            ShowFullWeaponDummyPolyIDs = NewDummyPolyManager.ShowGlobalIDOffset;

            CamAngleSnapEnable = GFX.World.AngleSnapEnable;

            MouseSpeedMult = GFX.World.OverallMouseSpeedMult;

            ViewportStatusTextSize = TaeViewportInteractor.StatusTextScale;

            ToolboxGuiScale = OSD.RenderScale * 100;
            ToolboxItemWidthScale = OSD.WidthScale * 100;

            SkyboxBrightness = Environment.SkyboxBrightnessMult;
            ShowCubemapAsSkybox = Environment.DrawCubemap;

            ShowCameraPivotCube = GFX.World.PivotPrimIsEnabled;

            LayoutAnimListWidth = editor.LeftSectionWidth;
            LayoutViewportWidth = editor.RightSectionWidth;
            LayoutViewportHeight = editor.TopRightPaneHeight;

            LayoutWindowWidth = Main.WinForm.Size.Width;
            LayoutWindowHeight = Main.WinForm.Size.Height;
        }

        public void AfterLoading(TaeEditorScreen editor)
        {
            DBG.CategoryEnableDraw.Clear();
            DBG.CategoryEnableDbgLabelDraw.Clear();
            DBG.CategoryEnableNameDraw.Clear();

            var categories = (DbgPrimCategory[])Enum.GetValues(typeof(DbgPrimCategory));
            foreach (var c in categories)
            {
                if (!CategoryEnableDraw.ContainsKey(c.ToString()))
                    CategoryEnableDraw.Add(c.ToString(), true);

                if (!CategoryEnableDbgLabelDraw.ContainsKey(c.ToString()))
                    CategoryEnableDbgLabelDraw.Add(c.ToString(), true);

                if (!CategoryEnableNameDraw.ContainsKey(c.ToString()))
                    CategoryEnableNameDraw.Add(c.ToString(), true);
            }

            foreach (var kvp in CategoryEnableDraw)
                if (Enum.TryParse<DbgPrimCategory>(kvp.Key, out DbgPrimCategory category))
                    DBG.CategoryEnableDraw[category] = kvp.Value;

            foreach (var kvp in CategoryEnableDbgLabelDraw)
                if (Enum.TryParse<DbgPrimCategory>(kvp.Key, out DbgPrimCategory category))
                    DBG.CategoryEnableDbgLabelDraw[category] = kvp.Value;

            foreach (var kvp in CategoryEnableNameDraw)
                if (Enum.TryParse<DbgPrimCategory>(kvp.Key, out DbgPrimCategory category))
                    DBG.CategoryEnableNameDraw[category] = kvp.Value;

            Environment.CubemapNameIndex = !string.IsNullOrWhiteSpace(LastCubemapUsed) 
                ? Environment.CubemapNames.ToList().IndexOf(LastCubemapUsed) : 0;

            GFX.MSAA = MSAA;
            GFX.SSAA = SSAA;

            Main.RequestViewportRenderTargetResolutionChange = true;


            Colors.ReadColorsFromConfig();
            Main.Colors = Colors;

            NewDummyPolyManager.ShowGlobalIDOffset = ShowFullWeaponDummyPolyIDs;

            GFX.World.AngleSnapEnable = CamAngleSnapEnable;
            GFX.World.OverallMouseSpeedMult = MouseSpeedMult;

            TaeViewportInteractor.StatusTextScale = ViewportStatusTextSize;

            OSD.RenderScale = Main.DPICustomMultX = Main.DPICustomMultY = (OSD.RenderScaleTarget = ToolboxGuiScale) / 100;
            OSD.WidthScale = (OSD.WidthScaleTarget = ToolboxItemWidthScale) / 100;

            Environment.SkyboxBrightnessMult = SkyboxBrightness;
            Environment.DrawCubemap = ShowCubemapAsSkybox;

            GFX.World.PivotPrimIsEnabled = ShowCameraPivotCube;

            try
            {
                Main.IgnoreSizeChanges = true;

                // Set window bounds before restoring pane sizes since it will try to auto correct them.
                Main.WinForm.Size = new System.Drawing.Size(LayoutWindowWidth, LayoutWindowHeight);
                //GFX.Display.Width = LayoutWindowWidth;
                //GFX.Display.Height = LayoutWindowHeight;
                //GFX.Display.Fullscreen = false;
                //GFX.Display.Apply();
                //GFX.Display.Apply();

                editor.UpdateLayout();

                editor.LeftSectionWidth = LayoutAnimListWidth;
                editor.RightSectionWidth = LayoutViewportWidth;
                editor.TopRightPaneHeight = LayoutViewportHeight;
            }
            finally
            {
                Main.IgnoreSizeChanges = false;
            }

            
        }

        public Dictionary<SoulsAssetPipeline.SoulsGames, NewChrAsmCfgJson> ChrAsmConfigurations { get; set; }
        = new Dictionary<SoulsAssetPipeline.SoulsGames, NewChrAsmCfgJson>
        {
            { SoulsAssetPipeline.SoulsGames.DS1, new NewChrAsmCfgJson()
                {
                    HeadID = 100000,
                    BodyID = 111000,
                    ArmsID = 292000,
                    LegsID = 113000,
                    RightWeaponID = 306000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 1455000,
                    //LeftWeaponModelIndex = 0,
                    LeftWeaponFlipBackwards = false,
                    LeftWeaponFlipSideways = false,
                    RightWeaponFlipBackwards = false,
                    RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.DS1R, new NewChrAsmCfgJson()
                {
                    HeadID = 100000,
                    BodyID = 111000,
                    ArmsID = 292000,
                    LegsID = 113000,
                    RightWeaponID = 306000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 1455000,
                    //LeftWeaponModelIndex = 0,
                    LeftWeaponFlipBackwards = false,
                    LeftWeaponFlipSideways = false,
                    RightWeaponFlipBackwards = false,
                    RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
             { SoulsAssetPipeline.SoulsGames.DS3, new NewChrAsmCfgJson()
                {
                    HeadID = 54000000,
                    BodyID = 54001000,
                    ArmsID = 54002000,
                    LegsID = 54003000,
                    RightWeaponID = 16160000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 21100000,
                    //LeftWeaponModelIndex = 0,
                    LeftWeaponFlipBackwards = false,
                    LeftWeaponFlipSideways = false,
                    RightWeaponFlipBackwards = false,
                    RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.TwoHandR,
                }
            },
             { SoulsAssetPipeline.SoulsGames.BB, new NewChrAsmCfgJson()
                {
                    HeadID = 130000,
                    BodyID = 361000,
                    ArmsID = 402000,
                    LegsID = 353000,
                    RightWeaponID = 24000000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 14000000,
                    //LeftWeaponModelIndex = 0,
                    LeftWeaponFlipBackwards = false,
                    LeftWeaponFlipSideways = false,
                    RightWeaponFlipBackwards = false,
                    RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
        };

        public TaeConfigFile()
        {
            EventSimulationsEnabled = new Dictionary<string, bool>();
            //foreach (var kvp in TaeEventSimulationEnvironment.Entries)
            //{
            //    if (kvp.Value.MenuOptionName != null)
            //        EventSimulationsEnabled.Add(kvp.Key, kvp.Value.IsEnabledByDefault);
            //}
        }


    }
}
