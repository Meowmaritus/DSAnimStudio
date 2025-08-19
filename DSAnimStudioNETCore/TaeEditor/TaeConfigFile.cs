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
        public bool WelcomeMessageDisabled = false;

        public OSD.DefaultLayoutTypes DesiredUILayoutType = OSD.DefaultLayoutTypes.V5;

        public bool ResetFloorOnAnimStart = true;
        public bool ResetHeadingOnAnimStart = false;

        public bool ShowActionIDs = true;

        public bool ResetScrollWhenChangingAnimations = true;

        //public bool TemplateEditorAutoRefresh = false;

        [JsonIgnore]
        public object _lock_ThreadSensitiveStuff = new object();

        public bool HasAcceptedSplashBefore = false;

        public bool EnableGameDataIOLogging = false;

        public bool AutoReloadParams = false;

        public enum ViewportStatusTypes
        {
            None,
            Condensed,
            Full,
        }

        public ViewportStatusTypes ShowStatusInViewport = ViewportStatusTypes.Condensed;

        public ParamData.AtkParam.DummyPolySource HitViewDummyPolySource = ParamData.AtkParam.DummyPolySource.RightWeapon0;

        public bool CharacterTrackingTestIsIngameTime = true;

        public bool GoToFirstAnimInCategoryWhenChangingCategory = true;

        //public bool DbgPrimXRay = true;
        public enum ActionSnapTypes
        {
            //None,
            FPS30,
            FPS60,
        }
        public ActionSnapTypes ActionSnapType = ActionSnapTypes.FPS30;
        public bool LockFramerateToOriginalAnimFramerate = false;
        public bool EnableFancyScrollingStrings = true;
        public float FancyScrollingStringsScrollSpeed = 64;
        public bool FancyTextScrollSnapsToPixels = true;
        public bool AutoCollapseAllTaeSections = false;
        public bool AnimListShowHkxNames = true;
        public bool AutoScrollDuringAnimPlayback = true;
        public bool SoloHighlightActionOnHover = false;
        public bool SoloHighlightActionOnHover_IgnoresStateInfo = true;
        public bool ShowActionTooltips = true;
        public bool UseOldActionSelectBehavior = false;


        public bool SimulateTaeOfOverlayedAnims = true;
        
        public bool SimulateOneShotActionsInReverse = false;


        public bool ScrollWhileDraggingActions_Enabled = true;
        public bool ScrollWhileDraggingActions_LimitSpeed = false;
        public float ScrollWhileDraggingActions_ThreshMin = 0.25f;
        public float ScrollWhileDraggingActions_ThreshMax = 0.9f;
        public float ScrollWhileDraggingActions_Speed = 3000f;
        public float ScrollWhileDraggingActions_SpeedPower = 2f;
        
        public float ScrollWhileDraggingActions_DragDistBlendMin = 20;
        public float ScrollWhileDraggingActions_DragDistBlendMax = 40;
        
        
        public bool NewRelativeTimelineScrubEnabled = true;
        public bool NewRelativeTimelineScrubSyncMouseToPlaybackCursor = true;
        public bool NewRelativeTimelineScrubHideMouseCursor = false;
        public bool NewRelativeTimelineScrubScrollScreen = true;
        public bool NewRelativeTimelineScrubScalesWithZoom = false;
        public bool NewRelativeTimelineScrubLockToAnimSpeed = false;
        public float NewRelativeTimelineScrubSpeed = 1f;
        //public float NewRelativeTimelineScrubSmoothing = 0.25f;


        public bool FlverShader_DebugViewWeightOfBone_EnableLighting = true;
        public bool FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry = false;
        public float FlverShader_DebugViewWeightOfBone_LightingPower = 4;
        public float FlverShader_DebugViewWeightOfBone_LightingMult = 1;
        public float FlverShader_DebugViewWeightOfBone_LightingGain = 0;
        public System.Numerics.Vector3 FlverShader_DebugViewWeightOfBone_BaseColor = new System.Numerics.Vector3(0, 0.25f, 0.5f);
        public System.Numerics.Vector3 FlverShader_DebugViewWeightOfBone_WeightColor = new System.Numerics.Vector3(1, 0, 0);
        public System.Numerics.Vector4 FlverShader_DebugViewWeightOfBone_WireframeWeightColor = new System.Numerics.Vector4(2, 0, 0, 1);
        public float FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult = 0.5f;
        public float FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult = 2f;
        public float FlverShader_DebugViewWeightOfBone_Lighting_Gloss = 0.15f;

        public bool FlverShader_DebugViewWeightOfBone_EnableTextureAlphas = false;

        public bool FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled = true;
        public bool FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas = false;
        public System.Numerics.Vector4 FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color = System.Numerics.Vector4.One;

        public bool FlverWireframeOverlay_Enabled = false;
        public bool FlverWireframeOverlay_ObeysTextureAlphas = true;
        public System.Numerics.Vector4 FlverWireframeOverlay_Color = new System.Numerics.Vector4(0, 0, 0.25f, 1);

        public bool Wwise_ShowMissingBankWarnings = false;

        public TaeLoadFromArchivesWizard.JsonSaveData TaeLoadFromArchivesWizardJsonSaveData = new TaeLoadFromArchivesWizard.JsonSaveData();

        public TaeLoadFromArchivesWizard.JsonSaveData GetTaeLoadFromArchivesWizardJsonSaveData()
        {
            TaeLoadFromArchivesWizard.JsonSaveData result = null;
            lock (_lock_ThreadSensitiveStuff)
                result = TaeLoadFromArchivesWizardJsonSaveData.GetCopy();
            return result;
        }

        public class TaeContainerInfoJsonVer
        {
            public DSAProj.TaeContainerInfo.ContainerTypes ContainerType;
            public string MainBinderPath;
            public string SecondaryBinderPath;
            public int BindID;

            public static implicit operator DSAProj.TaeContainerInfo(TaeContainerInfoJsonVer json)
            {
                if (json.ContainerType == DSAProj.TaeContainerInfo.ContainerTypes.Anibnd)
                    return new DSAProj.TaeContainerInfo.ContainerAnibnd(json.MainBinderPath, json.SecondaryBinderPath);
                else if (json.ContainerType == DSAProj.TaeContainerInfo.ContainerTypes.AnibndInBinder)
                    return new DSAProj.TaeContainerInfo.ContainerAnibndInBinder(json.MainBinderPath, json.BindID);
                else
                    throw new NotImplementedException();
            }

            public static implicit operator TaeContainerInfoJsonVer(DSAProj.TaeContainerInfo info)
            {
                if (info is DSAProj.TaeContainerInfo.ContainerAnibnd asAnibnd)
                {
                    return new TaeContainerInfoJsonVer()
                    {
                        ContainerType = DSAProj.TaeContainerInfo.ContainerTypes.Anibnd,
                        MainBinderPath = asAnibnd.AnibndPath,
                        SecondaryBinderPath = asAnibnd.ChrbndPath,
                    };
                }
                else if (info is DSAProj.TaeContainerInfo.ContainerAnibndInBinder asAnibndInBinder)
                {
                    return new TaeContainerInfoJsonVer()
                    {
                        ContainerType = DSAProj.TaeContainerInfo.ContainerTypes.AnibndInBinder,
                        MainBinderPath = asAnibndInBinder.BinderPath,
                        BindID = asAnibndInBinder.BindID,
                    };
                }
                else
                {
                    throw new NotImplementedException();
                }    
            }
        }

        public List<TaeContainerInfoJsonVer> RecentFilesList = new List<TaeContainerInfoJsonVer>();

        public bool ExtensiveBackupsEnabled = false;
        public bool LiveRefreshOnSave = false;


        public bool CameraFollowsRootMotionZX = true;
        //public float CameraFollowsRootMotionZX_Interpolation = 0.1f;


        public bool CameraFollowsRootMotionY = true;
        //public float CameraFollowsRootMotionY_Interpolation = 0.1f;


        public bool CameraFollowsRootMotionRotation = true;
        public float CameraFollowsRootMotionRotation_Interpolation = 0.1f;
        //public bool CameraPansVerticallyToFollowModel = true;


        public enum CameraFollowTypes
        {
            RootMotion,
            BodyDummyPoly
        }

        public CameraFollowTypes CameraFollowType = CameraFollowTypes.RootMotion;
        public int CameraFollowDummyPolyID = 220;

        //public bool ResetGridOriginAtAnimStart = false;

        //public bool WrapRootMotion = true;
        //public bool AccumulateRootMotion = true;
        public bool EnableAnimRootMotion = true;
        public float RootMotionTranslationMultiplierXZ = 1;
        public float RootMotionTranslationMultiplierY = 1;
        public float RootMotionRotationMultiplier = 1;

        public float RootMotionTranslationPowerXZ = 1;
        public float RootMotionTranslationPowerY = 1;
        public float RootMotionRotationPower = 1;

        //public bool SimulateReferencedEvents = true;

        //public bool CamAngleSnapEnable = false;
        //public bool ShowCameraPivotCube = false;

        public float ViewportStatusTextSize = 100;
        public float ViewportMemoryTextSize = 100;
        public float ViewportFramerateTextSize = 100;
        public float ToolboxGuiScale = 100;
        public float ToolboxItemWidthScale = 100;

        public int TooltipDelayMS = 350;

        public bool EnableFileCaching = true; 

        public ColorConfig Colors = new ColorConfig();

        //public float MouseSpeedMult = 1;


        //public bool IsNewGraphVisiMode = true;

        public int MSAA = 8;
        public int SSAA = 1;

        public bool EnableVSync = true;
        public int TargetFPS = 60;
        public int AverageFPSSampleSize = 20;

        public bool LimitFPSWhenWindowUnfocused = true;
        public bool StopUpdatingWhenWindowUnfocused = true;

        public SapImportConfigs.ImportConfigFlver2 LastUsedImportConfig_FLVER2 
            = new SapImportConfigs.ImportConfigFlver2();

        public SapImportConfigs.ImportConfigAnimFBX LastUsedImportConfig_AnimFBX 
            = new SapImportConfigs.ImportConfigAnimFBX();

        public bool SimEnabled_BasicBlending = true;
        public bool SimEnabled_BasicBlending_ComboViewer = true;
        public bool SimEnabled_Hitboxes = true;
        public bool SimEnabled_Hitboxes_CommonBehavior = true;
        public bool SimEnabled_Hitboxes_ThrowAttackBehavior = true;
        public bool SimEnabled_Hitboxes_PCBehavior = true;
        public bool SimEnabled_Sounds = true;
        public bool SimEnabled_RumbleCam = true;
        public bool SimEnabled_WeaponStyle = true;
        public bool SimEnabled_Tracking = true;
        public bool SimEnabled_AdditiveAnims = true;
        public bool SimEnabled_WeaponLocationOverrides = true;
        public bool SimEnabled_SpEffects = true;
        public bool SimEnabled_DS3DebugAnimSpeed = false;
        public bool SimEnabled_ERAnimSpeedGradient = true;
        public bool SimEnabled_AC6SpeedGradient9700 = true;
        public bool SimEnabled_SetOpacity = true;
        public bool SimEnabled_ModelMasks = true;
        public bool SimEnabled_Bullets = true;
        public bool SimEnabled_FFX = true;



        public bool SimEnabled_NF_SetTurnSpeedGradient = false;
        public bool SimEnabled_NF_SetTaeExtraAnim = false;
        public bool SimEnabled_NF_AnimSpeedGradient = false;
        public bool SimEnabled_NF_RootMotionScale = false;
        public bool SimEnabled_NF_MoveRelative = false;
        public bool SimOption_NF_MoveRelative_UseCameraAsTarget = false;

        public string LastCubemapUsed = "DefaultCubemap";
        public float SkyboxBrightness = 0.25f;
        public bool ShowCubemapAsSkybox = false;

        public HelperDrawConfig HelperDraw_NotAtRuntime = HelperDrawConfig.Default;

        //public bool ShowFullWeaponDummyPolyIDs = true;

        public int LayoutWindowX = -1;
        public int LayoutWindowY = -1;
        public int LayoutWindowWidth = 1280;
        public int LayoutWindowHeight = 720;

        public bool EnableBoneScale_NormalAnims = true;
        public bool EnableBoneScale_AdditiveAnims = true;
        public bool EnableBoneTranslation_NormalAnims = true;
        public bool EnableBoneTranslation_AdditiveAnims = true;

        public bool EnableRumbleCamSmoothing = false;
        
        public bool LoopEnabled = true;
        /// <summary>
        /// This is so cursed I'm sorry. It works just fine though with way less refactoring lol.
        /// </summary>
        public bool LoopEnabled_BeforeCombo = true;

        /// <summary>
        /// Obviously if you choose not to auto-save you'll have to manually save to save the setting.
        /// </summary>
        public bool DisableConfigFileAutoSave = false;

        //public bool RootMotionPathEnabled = true;
        



        public string ToolExportAnims_LastDestinationPathUsed = "";

        public List<ImguiOSD.WindowOpenStateEntry> WindowOpenStateEntries { get; set; } = new List<WindowOpenStateEntry>();

        public void BeforeSaving(TaeEditorScreen editor)
        {
            WindowOpenStateEntries = OSD.GetWindowOpenStateEntries();

            lock (DBG._lock_DebugDrawEnablers)
            {
                HelperDraw_NotAtRuntime = Main.HelperDraw;
            }
            AutoReloadParams = zzz_ParamManagerIns.AutoParamReloadEnabled;

            EnableFileCaching = zzz_GameDataIns.EnableFileCaching;

            LastCubemapUsed = ViewportEnvironment.CurrentCubemapName;

            MSAA = GFX.MSAA;
            SSAA = GFX.SSAA;

            EnableVSync = GFX.Display.Vsync;
            TargetFPS = GFX.Display.TargetFPS;
            AverageFPSSampleSize = GFX.Display.AverageFPSSampleSize;

            LimitFPSWhenWindowUnfocused = GFX.Display.LimitFPSWhenWindowUnfocused;
            StopUpdatingWhenWindowUnfocused = GFX.Display.StopUpdatingWhenWindowUnfocused;

            Colors = Main.Colors;
            Colors.WriteColorsToConfig();

            //ShowFullWeaponDummyPolyIDs = NewDummyPolyManager.ShowGlobalIDOffset;

            //CamAngleSnapEnable = WorldView.AngleSnapEnable;

            //MouseSpeedMult = WorldView.OverallMouseSpeedMult;

            ToolboxGuiScale = OSD.RenderScale * 100;
            ToolboxItemWidthScale = OSD.WidthScale * 100;

            SkyboxBrightness = ViewportEnvironment.SkyboxBrightnessMult;
            ShowCubemapAsSkybox = ViewportEnvironment.DrawCubemap;

            //ShowCameraPivotCube = WorldView.PivotPrimIsEnabled;
            
            LayoutWindowWidth = Main.WinForm.Size.Width;
            LayoutWindowHeight = Main.WinForm.Size.Height;
            LayoutWindowX = Main.WinForm.Location.X;
            LayoutWindowY = Main.WinForm.Location.Y;

            LoopEnabled = LoopEnabled_BeforeCombo;

            DisableConfigFileAutoSave = Main.DisableConfigFileAutoSave;

            FlverShader_DebugViewWeightOfBone_EnableLighting = GFX.FlverShader_DebugViewWeightOfBone_EnableLighting;
            FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry = GFX.FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry;
            FlverShader_DebugViewWeightOfBone_LightingPower = GFX.FlverShader_DebugViewWeightOfBone_LightingPower;
            FlverShader_DebugViewWeightOfBone_LightingMult = GFX.FlverShader_DebugViewWeightOfBone_LightingMult;
            FlverShader_DebugViewWeightOfBone_LightingGain = GFX.FlverShader_DebugViewWeightOfBone_LightingGain;
            FlverShader_DebugViewWeightOfBone_BaseColor = GFX.FlverShader_DebugViewWeightOfBone_BaseColor;
            FlverShader_DebugViewWeightOfBone_WeightColor = GFX.FlverShader_DebugViewWeightOfBone_WeightColor;
            FlverShader_DebugViewWeightOfBone_WireframeWeightColor = GFX.FlverShader_DebugViewWeightOfBone_WireframeWeightColor;
            FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult = GFX.FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult;
            FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult = GFX.FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult;
            FlverShader_DebugViewWeightOfBone_Lighting_Gloss = GFX.FlverShader_DebugViewWeightOfBone_Lighting_Gloss;

            FlverShader_DebugViewWeightOfBone_EnableTextureAlphas = GFX.FlverShader_DebugViewWeightOfBone_EnableTextureAlphas;

            FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled = GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled;
            FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas = GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas;
            FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color = GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color;

            FlverWireframeOverlay_Enabled = GFX.FlverWireframeOverlay_Enabled;
            FlverWireframeOverlay_ObeysTextureAlphas = GFX.FlverWireframeOverlay_ObeysTextureAlphas;
            FlverWireframeOverlay_Color = GFX.FlverWireframeOverlay_Color;
        }

        public void AfterLoadingFirstTime(TaeEditorScreen editor)
        {
            OSD.SetWindowOpenStatesFromConfig(WindowOpenStateEntries);

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
            if (DesiredUILayoutType == OSD.DefaultLayoutTypes.None)
                DesiredUILayoutType = OSD.DefaultLayoutTypes.V5;

            var imguiIniFile = $"{Main.Directory}\\imgui.ini";

            if (!System.IO.File.Exists(imguiIniFile))
            {
                OSD.RequestLoadDefaultLayout = DesiredUILayoutType;
            }

            lock (DBG._lock_DebugDrawEnablers)
            {
                Main.HelperDraw = HelperDraw_NotAtRuntime;
            }

            zzz_GameDataIns.EnableFileCaching = EnableFileCaching;
            
            zzz_ParamManagerIns.AutoParamReloadEnabled = AutoReloadParams;

            ViewportEnvironment.CubemapNameIndex = !string.IsNullOrWhiteSpace(LastCubemapUsed) 
                ? ViewportEnvironment.CubemapNames.ToList().IndexOf(LastCubemapUsed) : 0;

            GFX.MSAA = MSAA;
            GFX.SSAA = SSAA;

            if (TargetFPS < 10)
                TargetFPS = 10;

            if (AverageFPSSampleSize < 1)
                AverageFPSSampleSize = 1;
            
            GFX.Display.Width = GFX.Device.Viewport.Width;
            GFX.Display.Height = GFX.Device.Viewport.Height;
            GFX.Display.Fullscreen = false;
            GFX.Display.TargetFPS = GFX.Display.TargetFPSTarget = TargetFPS;
            GFX.Display.AverageFPSSampleSize = AverageFPSSampleSize;
            GFX.Display.LimitFPSWhenWindowUnfocused = LimitFPSWhenWindowUnfocused;
            GFX.Display.StopUpdatingWhenWindowUnfocused = StopUpdatingWhenWindowUnfocused;
            GFX.Display.Vsync = !EnableVSync;
            GFX.Display.Apply();
            GFX.Display.Vsync = EnableVSync;
            GFX.Display.Apply();

            Main.RequestViewportRenderTargetResolutionChange = true;

            Colors.ReadColorsFromConfig();
            Main.Colors = Colors;

            //NewDummyPolyManager.ShowGlobalIDOffset = ShowFullWeaponDummyPolyIDs;

            //WorldView.AngleSnapEnable = CamAngleSnapEnable;
            //WorldView.OverallMouseSpeedMult = MouseSpeedMult;

            OSD.RenderScale = Main.DPICustomMult = (OSD.RenderScaleTarget = ToolboxGuiScale) / 100;
            OSD.WidthScale = (OSD.WidthScaleTarget = ToolboxItemWidthScale) / 100;

            ViewportEnvironment.SkyboxBrightnessMult = SkyboxBrightness;
            ViewportEnvironment.DrawCubemap = ShowCubemapAsSkybox;

            //WorldView.PivotPrimIsEnabled = ShowCameraPivotCube;


            GFX.FlverShader_DebugViewWeightOfBone_EnableLighting = FlverShader_DebugViewWeightOfBone_EnableLighting;
            GFX.FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry = FlverShader_DebugViewWeightOfBone_ClipUnweightedGeometry;
            GFX.FlverShader_DebugViewWeightOfBone_LightingPower = FlverShader_DebugViewWeightOfBone_LightingPower;
            GFX.FlverShader_DebugViewWeightOfBone_LightingMult = FlverShader_DebugViewWeightOfBone_LightingMult;
            GFX.FlverShader_DebugViewWeightOfBone_LightingGain = FlverShader_DebugViewWeightOfBone_LightingGain;
            GFX.FlverShader_DebugViewWeightOfBone_BaseColor = FlverShader_DebugViewWeightOfBone_BaseColor;
            GFX.FlverShader_DebugViewWeightOfBone_WeightColor = FlverShader_DebugViewWeightOfBone_WeightColor;
            GFX.FlverShader_DebugViewWeightOfBone_WireframeWeightColor = FlverShader_DebugViewWeightOfBone_WireframeWeightColor;
            GFX.FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult = FlverShader_DebugViewWeightOfBone_Lighting_AlbedoMult;
            GFX.FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult = FlverShader_DebugViewWeightOfBone_Lighting_ReflectanceMult;
            GFX.FlverShader_DebugViewWeightOfBone_Lighting_Gloss = FlverShader_DebugViewWeightOfBone_Lighting_Gloss;

            GFX.FlverShader_DebugViewWeightOfBone_EnableTextureAlphas = FlverShader_DebugViewWeightOfBone_EnableTextureAlphas;

            GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled = FlverShader_DebugViewWeightOfBone_WireframeOverlay_Enabled;
            GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas = FlverShader_DebugViewWeightOfBone_WireframeOverlay_ObeysTextureAlphas;
            GFX.FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color = FlverShader_DebugViewWeightOfBone_WireframeOverlay_Color;

            GFX.FlverWireframeOverlay_Enabled = FlverWireframeOverlay_Enabled;
            GFX.FlverWireframeOverlay_ObeysTextureAlphas = FlverWireframeOverlay_ObeysTextureAlphas;
            GFX.FlverWireframeOverlay_Color = FlverWireframeOverlay_Color;

            if (!Main.IsDebugBuild)
            {
                var memes = new List<NewChrAsm.EquipSlotTypes>()
                {
                    NewChrAsm.EquipSlotTypes.Debug1,
                    NewChrAsm.EquipSlotTypes.Debug2,
                    NewChrAsm.EquipSlotTypes.Debug3,
                    NewChrAsm.EquipSlotTypes.Debug4,
                    NewChrAsm.EquipSlotTypes.Debug5,
                };

                foreach (var jsonCfg in ChrAsmConfigurations)
                {
                    foreach (var m in memes)
                    {
                        if (jsonCfg.Value.EquipIDs.ContainsKey(m))
                            jsonCfg.Value.EquipIDs.Remove(m);
                        if (jsonCfg.Value.DirectEquipInfos.ContainsKey(m))
                            jsonCfg.Value.DirectEquipInfos.Remove(m);
                    }
                }
            }
        }

        public Dictionary<SoulsAssetPipeline.SoulsGames, NewChrAsmCfgJson> ChrAsmConfigurations
        = new Dictionary<SoulsAssetPipeline.SoulsGames, NewChrAsmCfgJson>
        {
            { SoulsAssetPipeline.SoulsGames.DS1, new NewChrAsmCfgJson()
                {
                    IsFemale = false,
                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 350000 },
                        { NewChrAsm.EquipSlotTypes.Body, 351000 },
                        { NewChrAsm.EquipSlotTypes.Arms, 352000 },
                        { NewChrAsm.EquipSlotTypes.Legs, 353000 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 201000 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 1456000 },
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.DS1R, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 350000 },
                        { NewChrAsm.EquipSlotTypes.Body, 351000 },
                        { NewChrAsm.EquipSlotTypes.Arms, 352000 },
                        { NewChrAsm.EquipSlotTypes.Legs, 353000 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 201000 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 1456000 },
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.DS3, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 54000000 },
                        { NewChrAsm.EquipSlotTypes.Body, 54001000 },
                        { NewChrAsm.EquipSlotTypes.Arms, 54002000 },
                        { NewChrAsm.EquipSlotTypes.Legs, 54003000 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 16160000 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 21100000 },
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.RightBoth,
                }
            },
            { SoulsAssetPipeline.SoulsGames.BB, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 130000 },
                        { NewChrAsm.EquipSlotTypes.Body, 321000 },
                        { NewChrAsm.EquipSlotTypes.Arms, 132000 },
                        { NewChrAsm.EquipSlotTypes.Legs, 133000 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 7000000 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 14000000 },
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.DES, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 100000 },
                        { NewChrAsm.EquipSlotTypes.Body, 200700 },
                        { NewChrAsm.EquipSlotTypes.Arms, 300700 },
                        { NewChrAsm.EquipSlotTypes.Legs, 400700 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 20200 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 150800 },
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.ER, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 980000 },
                        { NewChrAsm.EquipSlotTypes.Body, 981100 },
                        { NewChrAsm.EquipSlotTypes.Arms, 980200 },
                        { NewChrAsm.EquipSlotTypes.Legs, 980300 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 2180000 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 31190000 },
                    },

                    DirectEquipInfos = new Dictionary<NewChrAsm.EquipSlotTypes, NewEquipSlot_Armor.DirectEquipInfo>()
                    {
                        {
                            NewChrAsm.EquipSlotTypes.Face, new NewEquipSlot_Armor.DirectEquipInfo()
                            {
                                PartPrefix = NewEquipSlot_Armor.DirectEquipPartPrefix.FC,
                                Gender = NewEquipSlot_Armor.DirectEquipGender.BothGendersUseMF,
                                ModelID = 0
                            }
                        },
                        {
                            NewChrAsm.EquipSlotTypes.Facegen1, new NewEquipSlot_Armor.DirectEquipInfo()
                            {
                                PartPrefix = NewEquipSlot_Armor.DirectEquipPartPrefix.FG,
                                Gender = NewEquipSlot_Armor.DirectEquipGender.UnisexUseA,
                                ModelID = 100
                            }
                        }
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            //ERNR TODO
            { SoulsAssetPipeline.SoulsGames.ERNR, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 980000 },
                        { NewChrAsm.EquipSlotTypes.Body, 981100 },
                        { NewChrAsm.EquipSlotTypes.Arms, 980200 },
                        { NewChrAsm.EquipSlotTypes.Legs, 980300 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 2180000 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 31190000 },
                    },

                    DirectEquipInfos = new Dictionary<NewChrAsm.EquipSlotTypes, NewEquipSlot_Armor.DirectEquipInfo>()
                    {
                        {
                            NewChrAsm.EquipSlotTypes.Face, new NewEquipSlot_Armor.DirectEquipInfo()
                            {
                                PartPrefix = NewEquipSlot_Armor.DirectEquipPartPrefix.FC,
                                Gender = NewEquipSlot_Armor.DirectEquipGender.BothGendersUseMF,
                                ModelID = 0
                            }
                        },
                        {
                            NewChrAsm.EquipSlotTypes.Facegen1, new NewEquipSlot_Armor.DirectEquipInfo()
                            {
                                PartPrefix = NewEquipSlot_Armor.DirectEquipPartPrefix.FG,
                                Gender = NewEquipSlot_Armor.DirectEquipGender.UnisexUseA,
                                ModelID = 100
                            }
                        }
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,
                }
            },
            { SoulsAssetPipeline.SoulsGames.SDT, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 100000 },
                        { NewChrAsm.EquipSlotTypes.Body, 101000 },
                        { NewChrAsm.EquipSlotTypes.Arms, 102000 },
                        { NewChrAsm.EquipSlotTypes.Legs, 103000 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 8000000 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 73000 },
                        { NewChrAsm.EquipSlotTypes.SekiroMortalBlade, 9700000 },
                        { NewChrAsm.EquipSlotTypes.SekiroGrapplingHook, 9500000 },
                    },

                    DirectEquipInfos = new Dictionary<NewChrAsm.EquipSlotTypes, NewEquipSlot_Armor.DirectEquipInfo>()
                    {
                        { 
                            NewChrAsm.EquipSlotTypes.Face, new NewEquipSlot_Armor.DirectEquipInfo() 
                            {
                                PartPrefix = NewEquipSlot_Armor.DirectEquipPartPrefix.FC, 
                                Gender = NewEquipSlot_Armor.DirectEquipGender.UnisexUseMForBoth, 
                                ModelID = 200
                            }
                        }
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.OneHand,

                    
                }
            },

            { SoulsAssetPipeline.SoulsGames.AC6, new NewChrAsmCfgJson()
                {
                    IsFemale = false,

                    EquipIDs = new Dictionary<NewChrAsm.EquipSlotTypes, int>()
                    {
                        { NewChrAsm.EquipSlotTypes.Head, 50020000 },
                        { NewChrAsm.EquipSlotTypes.Body, 51020000 },
                        { NewChrAsm.EquipSlotTypes.Arms, 52020000 },
                        { NewChrAsm.EquipSlotTypes.Legs, 53020000 },
                        { NewChrAsm.EquipSlotTypes.RightWeapon, 10070100 },
                        { NewChrAsm.EquipSlotTypes.LeftWeapon, 20030000 },
                        { NewChrAsm.EquipSlotTypes.AC6BackLeftWeapon, -1 },
                        { NewChrAsm.EquipSlotTypes.AC6BackRightWeapon, 10000000 },
                    },

                    WeaponStyle = NewChrAsm.WeaponStyleType.RightBoth,
                }
            },
        };

        public TaeConfigFile()
        {
            //lock (_lock_ThreadSensitiveStuff)
            //{
            //    _eventSimulationsEnabled = new Dictionary<string, bool>();
            //}
            //foreach (var kvp in TaeEventSimulationEnvironment.Entries)
            //{
            //    if (kvp.Value.MenuOptionName != null)
            //        EventSimulationsEnabled.Add(kvp.Key, kvp.Value.IsEnabledByDefault);
            //}

            var checkFPS = GFX.Display.GetCurrentWindowsDisplayFrequency();
            if (checkFPS >= 20 && checkFPS < 1000)
            {
                TargetFPS = checkFPS;
            }
        }


    }
}
