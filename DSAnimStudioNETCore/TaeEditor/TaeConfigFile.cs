using DSAnimStudio.DebugPrimitives;
using DSAnimStudio.ImguiOSD;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SoulsAssetPipeline.FLVERImporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSAnimStudio.TaeEditor
{
    public class TaeConfigFile
    {
        [JsonIgnore]
        public object _lock_ThreadSensitiveStuff = new object();

        public bool EnableGameDataIOLogging { get; set; } = false;

        public enum ViewportStatusTypes
        {
            None,
            Condensed,
            Full,
        }

        public ViewportStatusTypes ShowStatusInViewport { get; set; } = ViewportStatusTypes.Condensed;

        public ParamData.AtkParam.DummyPolySource HitViewDummyPolySource { get; set; } = ParamData.AtkParam.DummyPolySource.RightWeapon0;

        public bool CharacterTrackingTestIsIngameTime { get; set; } = true;

        public bool SaveAdditionalEventRowInfoToLegacyGames { get; set; } = false;

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


        public TaeLoadFromArchivesWizard.JsonSaveData TaeLoadFromArchivesWizardJsonSaveData { get; set; } = new TaeLoadFromArchivesWizard.JsonSaveData();

        public TaeLoadFromArchivesWizard.JsonSaveData GetTaeLoadFromArchivesWizardJsonSaveData()
        {
            TaeLoadFromArchivesWizard.JsonSaveData result = null;
            lock (_lock_ThreadSensitiveStuff)
                result = TaeLoadFromArchivesWizardJsonSaveData.GetCopy();
            return result;
        }

        public class TaeRecentFileEntry
        {
            public string TaeFile = null;
            public string ModelFile = null;
            public TaeRecentFileEntry()
            {

            }
            public TaeRecentFileEntry(string taeFile, string modelFile)
            {
                TaeFile = taeFile;
                ModelFile = modelFile;
            }
        }

        public List<TaeRecentFileEntry> RecentFilesList { get; set; } = new List<TaeRecentFileEntry>();

        public bool ExtensiveBackupsEnabled { get; set; } = false;
        public bool LiveRefreshOnSave { get; set; } = false;


        public bool CameraFollowsRootMotionZX { get; set; } = true;
        public float CameraFollowsRootMotionZX_Interpolation { get; set; } = 0.1f;


        public bool CameraFollowsRootMotionY { get; set; } = true;
        public float CameraFollowsRootMotionY_Interpolation { get; set; } = 0.1f;


        public bool CameraFollowsRootMotionRotation { get; set; } = true;
        public float CameraFollowsRootMotionRotation_Interpolation { get; set; } = 0.1f;
        //public bool CameraPansVerticallyToFollowModel { get; set; } = true;

        public enum CameraFollowTypes
        {
            RootMotion,
            BodyDummyPoly
        }

        public CameraFollowTypes CameraFollowType { get; set; } = CameraFollowTypes.RootMotion;
        public int CameraFollowDummyPolyID { get; set; } = 220;

        public bool WrapRootMotion { get; set; } = true;
        public bool AccumulateRootMotion { get; set; } = true;
        public bool EnableAnimRootMotion { get; set; } = true;
        public float RootMotionTranslationMultiplierXZ { get; set; } = 1;
        public float RootMotionTranslationMultiplierY { get; set; } = 1;
        public float RootMotionRotationMultiplier { get; set; } = 1;

        public float RootMotionTranslationPowerXZ { get; set; } = 1;
        public float RootMotionTranslationPowerY { get; set; } = 1;
        public float RootMotionRotationPower { get; set; } = 1;

        //public bool SimulateReferencedEvents { get; set; } = true;

        public bool CamAngleSnapEnable { get; set; } = false;
        public bool ShowCameraPivotCube { get; set; } = false;

        public float ViewportStatusTextSize { get; set; } = 100;
        public float ViewportMemoryTextSize { get; set; } = 100;
        public float ViewportFramerateTextSize { get; set; } = 100;
        public float ToolboxGuiScale { get; set; } = 100;
        public float ToolboxItemWidthScale { get; set; } = 100;

        public ColorConfig Colors { get; set; } = new ColorConfig();

        public float MouseSpeedMult { get; set; } = 1;


        public bool IsNewGraphVisiMode { get; set; } = true;

        public int MSAA = 2;
        public int SSAA = 1;

        public bool EnableVSync { get; set; } = true;

        public SapImportConfigs.ImportConfigFlver2 LastUsedImportConfig_FLVER2 { get; set; } 
            = new SapImportConfigs.ImportConfigFlver2();

        public SapImportConfigs.ImportConfigAnimFBX LastUsedImportConfig_AnimFBX { get; set; } 
            = new SapImportConfigs.ImportConfigAnimFBX();

        [JsonProperty]
        private Dictionary<string, bool> _eventSimulationsEnabled { get; set; } = new Dictionary<string, bool>();

        [JsonIgnore]
        public IReadOnlyDictionary<string, bool> EventSimulationsEnabled
        {
            get
            {
                Dictionary<string, bool> result = null;
                lock (_lock_ThreadSensitiveStuff)
                {
                    result = _eventSimulationsEnabled.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }
        }

        public bool GetEventSimulationEnabled(string simKey)
        {
            if (string.IsNullOrWhiteSpace(simKey))
                return false;

            bool result = false;
            lock (_lock_ThreadSensitiveStuff)
            {
                if (!_eventSimulationsEnabled.ContainsKey(simKey))
                    _eventSimulationsEnabled.Add(simKey, TaeEditor.TaeEventSimulationEnvironment.GetEntryEnabledByDefault(simKey));
                result = _eventSimulationsEnabled[simKey];
            }
            return result;
        }

        public void SetEventSimulationEnabled(string simKey, bool enabled)
        {
            lock (_lock_ThreadSensitiveStuff)
            {
                _eventSimulationsEnabled[simKey] = enabled;
            }
        }

        public string LastCubemapUsed { get; set; } = "DefaultCubemap";
        public float SkyboxBrightness { get; set; } = 0.25f;
        public bool ShowCubemapAsSkybox { get; set; } = false;

        public Dictionary<string, bool> CategoryEnableDraw = new Dictionary<string, bool>();
        public Dictionary<string, bool> CategoryEnableDbgLabelDraw = new Dictionary<string, bool>();
        public Dictionary<string, bool> CategoryEnableNameDraw = new Dictionary<string, bool>();

        public bool ShowFullWeaponDummyPolyIDs { get; set; } = true;

        public float LayoutAnimListWidth { get; set; } = 236;
        public float LayoutViewportWidth { get; set; } = 600;
        public float LayoutViewportHeight { get; set; } = 600;

        public int LayoutWindowX { get; set; } = -1;
        public int LayoutWindowY { get; set; } = -1;
        public int LayoutWindowWidth { get; set; } = 1280;
        public int LayoutWindowHeight { get; set; } = 720;

        public bool LoopEnabled { get; set; } = true;
        /// <summary>
        /// This is so cursed I'm sorry. It works just fine though with way less refactoring lol.
        /// </summary>
        public bool LoopEnabled_BeforeCombo { get; set; } = true;

        /// <summary>
        /// Obviously if you choose not to auto-save you'll have to manually save to save the setting.
        /// </summary>
        public bool DisableConfigFileAutoSave { get; set; } = false;

        public bool RootMotionPathEnabled { get; set; } = true;
        public float RootMotionPathUpdateRate { get; set; } = 30;
        public int RootMotionPathSampleMax { get; set; } = 200;
        public const int RootMotionPathSampleMaxInfinityValue = 1000;



        public string ToolExportAnims_LastDestinationPathUsed { get; set; } = "";

        

        public void BeforeSaving(TaeEditorScreen editor)
        {
            lock (DBG._lock_DebugDrawEnablers)
            {
                CategoryEnableDraw = DBG.CategoryEnableDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
                CategoryEnableDbgLabelDraw = DBG.CategoryEnableDbgLabelDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
                CategoryEnableNameDraw = DBG.CategoryEnableNameDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            }

            LastCubemapUsed = Environment.CurrentCubemapName;

            MSAA = GFX.MSAA;
            SSAA = GFX.SSAA;

            EnableVSync = GFX.Display.Vsync;

            Colors = Main.Colors;
            Colors.WriteColorsToConfig();

            ShowFullWeaponDummyPolyIDs = NewDummyPolyManager.ShowGlobalIDOffset;

            CamAngleSnapEnable = GFX.CurrentWorldView.AngleSnapEnable;

            MouseSpeedMult = GFX.CurrentWorldView.OverallMouseSpeedMult;

            ToolboxGuiScale = OSD.RenderScale * 100;
            ToolboxItemWidthScale = OSD.WidthScale * 100;

            SkyboxBrightness = Environment.SkyboxBrightnessMult;
            ShowCubemapAsSkybox = Environment.DrawCubemap;

            ShowCameraPivotCube = GFX.CurrentWorldView.PivotPrimIsEnabled;

            LayoutAnimListWidth = editor?.LeftSectionWidth ?? -1;
            LayoutViewportWidth = editor?.RightSectionWidth ?? -1;
            LayoutViewportHeight = editor?.TopRightPaneHeight ?? -1;

            LayoutWindowWidth = Main.WinForm.Size.Width;
            LayoutWindowHeight = Main.WinForm.Size.Height;
            LayoutWindowX = Main.WinForm.Location.X;
            LayoutWindowY = Main.WinForm.Location.Y;

            LoopEnabled = LoopEnabled_BeforeCombo;

            DisableConfigFileAutoSave = Main.DisableConfigFileAutoSave;
        }

        public void AfterLoadingFirstTime(TaeEditorScreen editor)
        {
            Main.DisableConfigFileAutoSave = DisableConfigFileAutoSave;

            Main.IgnoreSizeChanges = true;

            int w = LayoutWindowWidth;
            int h = LayoutWindowHeight;
            int x = LayoutWindowX >= 0 ? LayoutWindowX : Main.WinForm.Location.X;
            int y = LayoutWindowY >= 0 ? LayoutWindowY : Main.WinForm.Location.Y;

            bool isPointVisibleOnAScreen(Point p)
            {
                foreach (System.Windows.Forms.Screen s in System.Windows.Forms.Screen.AllScreens)
                {
                    if (p.X < s.Bounds.Right && p.X > s.Bounds.Left && p.Y > s.Bounds.Top && p.Y < s.Bounds.Bottom)
                        return true;
                }
                return false;
            }

            bool isEntireWindowVisibleWithLocationSavedInConfig = 
                isPointVisibleOnAScreen(new Point(x, y)) &&
                isPointVisibleOnAScreen(new Point(x + w, y)) &&
                isPointVisibleOnAScreen(new Point(x, y + h)) &&
                isPointVisibleOnAScreen(new Point(x + w, y + h));

            if (isEntireWindowVisibleWithLocationSavedInConfig)
            {
                Main.WinForm.Location = new System.Drawing.Point(x, y);
                Main.WinForm.Size = new System.Drawing.Size(w, h);
            }

            Main.IgnoreSizeChanges = false;
        }

        public void AfterLoading(TaeEditorScreen editor)
        {
            lock (DBG._lock_DebugDrawEnablers)
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

            }

            Environment.CubemapNameIndex = !string.IsNullOrWhiteSpace(LastCubemapUsed) 
                ? Environment.CubemapNames.ToList().IndexOf(LastCubemapUsed) : 0;

            GFX.MSAA = MSAA;
            GFX.SSAA = SSAA;

            if (GFX.Display.Vsync != EnableVSync)
            {
                GFX.Display.Vsync = EnableVSync;
                GFX.Display.Width = GFX.Device.Viewport.Width;
                GFX.Display.Height = GFX.Device.Viewport.Height;
                GFX.Display.Fullscreen = false;
                GFX.Display.Apply();
            }

            Main.RequestViewportRenderTargetResolutionChange = true;

            Colors.ReadColorsFromConfig();
            Main.Colors = Colors;

            NewDummyPolyManager.ShowGlobalIDOffset = ShowFullWeaponDummyPolyIDs;

            GFX.CurrentWorldView.AngleSnapEnable = CamAngleSnapEnable;
            GFX.CurrentWorldView.OverallMouseSpeedMult = MouseSpeedMult;

            OSD.RenderScale = Main.DPICustomMultX = Main.DPICustomMultY = (OSD.RenderScaleTarget = ToolboxGuiScale) / 100;
            OSD.WidthScale = (OSD.WidthScaleTarget = ToolboxItemWidthScale) / 100;

            Environment.SkyboxBrightnessMult = SkyboxBrightness;
            Environment.DrawCubemap = ShowCubemapAsSkybox;

            GFX.CurrentWorldView.PivotPrimIsEnabled = ShowCameraPivotCube;
        }

        public Dictionary<SoulsAssetPipeline.SoulsGames, NewChrAsmCfgJson> ChrAsmConfigurations { get; set; }
        = new Dictionary<SoulsAssetPipeline.SoulsGames, NewChrAsmCfgJson>
        {
            { SoulsAssetPipeline.SoulsGames.DS1, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    HeadID = 100000,
                    BodyID = 111000,
                    ArmsID = 292000,
                    LegsID = 113000,
                    RightWeaponID = 306000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 1455000,
                    //LeftWeaponModelIndex = 0,
                    //LeftWeaponFlipBackwards = false,
                    //LeftWeaponFlipSideways = false,
                    //RightWeaponFlipBackwards = false,
                    //RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.DS1R, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    HeadID = 100000,
                    BodyID = 111000,
                    ArmsID = 292000,
                    LegsID = 113000,
                    RightWeaponID = 306000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 1455000,
                    //LeftWeaponModelIndex = 0,
                    //LeftWeaponFlipBackwards = false,
                    //LeftWeaponFlipSideways = false,
                    //RightWeaponFlipBackwards = false,
                    //RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.DS3, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    HeadID = 54000000,
                    BodyID = 54001000,
                    ArmsID = 54002000,
                    LegsID = 54003000,
                    RightWeaponID = 16160000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 21100000,
                    //LeftWeaponModelIndex = 0,
                    //LeftWeaponFlipBackwards = false,
                    //LeftWeaponFlipSideways = false,
                    //RightWeaponFlipBackwards = false,
                    //RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.TwoHandR,
                }
            },
            { SoulsAssetPipeline.SoulsGames.BB, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    HeadID = 130000,
                    BodyID = 361000,
                    ArmsID = 402000,
                    LegsID = 353000,
                    RightWeaponID = 24000000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 14000000,
                    //LeftWeaponModelIndex = 0,
                    //LeftWeaponFlipBackwards = false,
                    //LeftWeaponFlipSideways = false,
                    //RightWeaponFlipBackwards = false,
                    //RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.DES, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    HeadID = 100700,
                    BodyID = 200700,
                    ArmsID = 300700,
                    LegsID = 400700,
                    RightWeaponID = 20200,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 150800,
                    //LeftWeaponModelIndex = 0,
                    //LeftWeaponFlipBackwards = false,
                    //LeftWeaponFlipSideways = false,
                    //RightWeaponFlipBackwards = false,
                    //RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.ER, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    HeadID = 651000,
                    BodyID = 650100,
                    ArmsID = 650200,
                    LegsID = 650300,
                    RightWeaponID = 2000000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 31330000,
                    //LeftWeaponModelIndex = 0,
                    //LeftWeaponFlipBackwards = true,
                    //LeftWeaponFlipSideways = false,
                    //RightWeaponFlipBackwards = true,
                    //RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.SDT, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    HeadID = 100000,
                    BodyID = 101000,
                    ArmsID = 102000,
                    LegsID = 103000,
                    RightWeaponID = 8000000,
                    //RightWeaponModelIndex = 0,
                    LeftWeaponID = 73000,
                    //LeftWeaponModelIndex = 0,
                    //LeftWeaponFlipBackwards = true,
                    //LeftWeaponFlipSideways = false,
                    //RightWeaponFlipBackwards = true,
                    //RightWeaponFlipSideways = false,
                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
        };

        public TaeConfigFile()
        {
            lock (_lock_ThreadSensitiveStuff)
            {
                _eventSimulationsEnabled = new Dictionary<string, bool>();
            }
            //foreach (var kvp in TaeEventSimulationEnvironment.Entries)
            //{
            //    if (kvp.Value.MenuOptionName != null)
            //        EventSimulationsEnabled.Add(kvp.Key, kvp.Value.IsEnabledByDefault);
            //}
        }


    }
}
