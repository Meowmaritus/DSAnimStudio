using DSAnimStudio.DebugPrimitives;
using System;
using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public class TaeConfigFile
    {
        public bool EnableColorBlindMode { get; set; } = false;

        public bool EnableSnapTo30FPSIncrements { get; set; } = true;
        public bool LockFramerateToOriginalAnimFramerate { get; set; } = false;

        public bool EnableFancyScrollingStrings { get; set; } = true;
        public float FancyScrollingStringsScrollSpeed { get; set; } = 64;
        public bool FancyTextScrollSnapsToPixels { get; set; } = true;
        public bool AutoCollapseAllTaeSections { get; set; } = false;
        public bool AutoScrollDuringAnimPlayback { get; set; } = true;
        public List<string> RecentFilesList { get; set; } = new List<string>();
        public bool LiveRefreshOnSave { get; set; } = true;

        public bool CameraFollowsRootMotion { get; set; } = true;

        public Dictionary<string, bool> EventSimulationsEnabled { get; set; }
            = new Dictionary<string, bool>();

        public string LastCubemapUsed { get; set; } = "m32_00_GILM0131";

        public Dictionary<DbgPrimCategory, bool> CategoryEnableDraw = new Dictionary<DbgPrimCategory, bool>();
        public Dictionary<DbgPrimCategory, bool> CategoryEnableDbgLabelDraw = new Dictionary<DbgPrimCategory, bool>();
        public Dictionary<DbgPrimCategory, bool> CategoryEnableNameDraw = new Dictionary<DbgPrimCategory, bool>();

        public void BeforeSaving()
        {
            CategoryEnableDraw = DBG.CategoryEnableDraw;
            CategoryEnableDbgLabelDraw = DBG.CategoryEnableDbgLabelDraw;
            CategoryEnableNameDraw = DBG.CategoryEnableNameDraw;
        }

        public void AfterLoading()
        {
            DBG.CategoryEnableDraw = CategoryEnableDraw;
            DBG.CategoryEnableDbgLabelDraw = CategoryEnableDbgLabelDraw;
            DBG.CategoryEnableNameDraw = CategoryEnableNameDraw;

            var categories = (DbgPrimCategory[])Enum.GetValues(typeof(DbgPrimCategory));
            foreach (var c in categories)
            {
                if (!CategoryEnableDraw.ContainsKey(c))
                    CategoryEnableDraw.Add(c, true);

                if (!CategoryEnableDbgLabelDraw.ContainsKey(c))
                    CategoryEnableDbgLabelDraw.Add(c, true);

                if (!CategoryEnableNameDraw.ContainsKey(c))
                    CategoryEnableNameDraw.Add(c, true);
            }
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
