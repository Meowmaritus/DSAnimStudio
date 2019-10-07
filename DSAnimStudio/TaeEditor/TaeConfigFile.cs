using DSAnimStudio.DebugPrimitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DSAnimStudio.TaeEditor
{
    public class TaeConfigFile
    {
        public ParamData.AtkParam.DummyPolySource HitViewDummyPolySource { get; set; } = ParamData.AtkParam.DummyPolySource.Body;

        public bool EnableColorBlindMode { get; set; } = false;

        public bool DbgPrimXRay { get; set; } = false;
        public bool EnableSnapTo30FPSIncrements { get; set; } = true;
        public bool LockFramerateToOriginalAnimFramerate { get; set; } = false;
        public bool EnableFancyScrollingStrings { get; set; } = true;
        public float FancyScrollingStringsScrollSpeed { get; set; } = 64;
        public bool FancyTextScrollSnapsToPixels { get; set; } = true;
        public bool AutoCollapseAllTaeSections { get; set; } = false;
        public bool AutoScrollDuringAnimPlayback { get; set; } = true;
        public bool SoloHighlightEventOnHover { get; set; } = true;
        public bool ShowEventHoverInfo { get; set; } = true;
        public List<string> RecentFilesList { get; set; } = new List<string>();
        public bool LiveRefreshOnSave { get; set; } = true;
        public bool CameraFollowsRootMotion { get; set; } = true;
        public bool EnableAnimRootMotion { get; set; } = true;

        public Dictionary<string, bool> EventSimulationsEnabled { get; set; }
            = new Dictionary<string, bool>();

        public string LastCubemapUsed { get; set; } = "m32_00_GILM0131";

        public Dictionary<string, bool> CategoryEnableDraw = new Dictionary<string, bool>();
        public Dictionary<string, bool> CategoryEnableDbgLabelDraw = new Dictionary<string, bool>();
        public Dictionary<string, bool> CategoryEnableNameDraw = new Dictionary<string, bool>();

        public void BeforeSaving()
        {
            CategoryEnableDraw = DBG.CategoryEnableDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            CategoryEnableDbgLabelDraw = DBG.CategoryEnableDbgLabelDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
            CategoryEnableNameDraw = DBG.CategoryEnableNameDraw.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
        }

        public void AfterLoading()
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

        public Dictionary<GameDataManager.GameTypes, NewChrAsmCfgJson> ChrAsmConfigurations { get; set; }
        = new Dictionary<GameDataManager.GameTypes, NewChrAsmCfgJson>
        {
            { GameDataManager.GameTypes.DS1, new NewChrAsmCfgJson()
                {
                    HeadID = 100000,
                    BodyID = 111000,
                    ArmsID = 292000,
                    LegsID = 113000,
                    RightWeaponID = 306000,
                    RightWeaponModelIndex = 0,
                    LeftWeaponID = 1455000,
                    LeftWeaponModelIndex = 0,
                    LeftWeaponFlipSideways = true,
                    LeftWeaponFlipBackwards = false,
                    RightWeaponFlipBackwards = true,
                    RightWeaponFlipSideways = false,
                }
            },
             { GameDataManager.GameTypes.DS3, new NewChrAsmCfgJson()
                {
                    HeadID = 52000000,
                    BodyID = 52001000,
                    ArmsID = 52002000,
                    LegsID = 52003000,
                    RightWeaponID = 2060000,
                    RightWeaponModelIndex = 0,
                    LeftWeaponID = 21010000,
                    LeftWeaponModelIndex = 0,
                    LeftWeaponFlipSideways = true,
                    LeftWeaponFlipBackwards = false,
                    RightWeaponFlipBackwards = true,
                    RightWeaponFlipSideways = false,
                }
            },
             { GameDataManager.GameTypes.BB, new NewChrAsmCfgJson()
                {
                    HeadID = 230000,
                    BodyID = 231000,
                    ArmsID = 232000,
                    LegsID = 233000,
                    RightWeaponID = 7100000,
                    RightWeaponModelIndex = 0,
                    LeftWeaponID = 6000000,
                    LeftWeaponModelIndex = 0,
                    LeftWeaponFlipSideways = true,
                    LeftWeaponFlipBackwards = false,
                    RightWeaponFlipBackwards = true,
                    RightWeaponFlipSideways = false,
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
