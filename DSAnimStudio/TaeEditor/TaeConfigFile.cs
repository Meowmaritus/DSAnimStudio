using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public class TaeConfigFile
    {
        public bool EnableColorBlindMode { get; set; } = false;

        public bool EnableSnapTo30FPSIncrements { get; set; } = true;

        public bool EnableFancyScrollingStrings { get; set; } = true;
        public float FancyScrollingStringsScrollSpeed { get; set; } = 64;
        public bool FancyTextScrollSnapsToPixels { get; set; } = true;
        public bool AutoCollapseAllTaeSections { get; set; } = false;
        public bool AutoScrollDuringAnimPlayback { get; set; } = true;
        public List<string> RecentFilesList { get; set; } = new List<string>();
        public bool LiveRefreshOnSave { get; set; } = true;

        public Dictionary<string, bool> EventSimulationsEnabled { get; set; }
            = new Dictionary<string, bool>();

        public string LastCubemapUsed { get; set; } = "m32_00_GILM0131";

        public class ChrAsmConfig
        {
            public int HeadID = -1;
            public int BodyID = -1;
            public int ArmsID = -1;
            public int LegsID = -1;
            public int RightWeaponID = -1;
            public int RightWeaponModelIndex = 0;
            public int LeftWeaponID = -1;
            public int LeftWeaponModelIndex = 0;
            public bool LeftWeaponFlip_ForDS1Shields = true;
            public bool FlipAnimatedLeftWeaponBackwards = true;
            public bool FlipAnimatedRightWeaponBackwards = true;
            public bool FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows = false;

            public void WriteToChrAsm(NewChrAsm chrAsm)
            {
                chrAsm.HeadID = HeadID;
                chrAsm.BodyID = BodyID;
                chrAsm.ArmsID = ArmsID;
                chrAsm.LegsID = LegsID;
                chrAsm.RightWeaponID = RightWeaponID;
                chrAsm.RightWeaponModelIndex = RightWeaponModelIndex;
                chrAsm.LeftWeaponID = LeftWeaponID;
                chrAsm.LeftWeaponModelIndex = LeftWeaponModelIndex;
                chrAsm.LeftWeaponFlip_ForDS1Shields = LeftWeaponFlip_ForDS1Shields;
                chrAsm.FlipAnimatedLeftWeaponBackwards = FlipAnimatedLeftWeaponBackwards;
                chrAsm.FlipAnimatedRightWeaponBackwards = FlipAnimatedRightWeaponBackwards;
                chrAsm.FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows = FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows;

                chrAsm.UpdateModels();
            }

            public void CopyFromChrAsm(NewChrAsm chrAsm)
            {
                HeadID = chrAsm.HeadID;
                BodyID = chrAsm.BodyID;
                ArmsID = chrAsm.ArmsID;
                LegsID = chrAsm.LegsID;
                RightWeaponID = chrAsm.RightWeaponID;
                RightWeaponModelIndex = chrAsm.RightWeaponModelIndex;
                LeftWeaponID = chrAsm.LeftWeaponID;
                LeftWeaponModelIndex = chrAsm.LeftWeaponModelIndex;
                LeftWeaponFlip_ForDS1Shields = chrAsm.LeftWeaponFlip_ForDS1Shields;
                FlipAnimatedLeftWeaponBackwards = chrAsm.FlipAnimatedLeftWeaponBackwards;
                FlipAnimatedRightWeaponBackwards = chrAsm.FlipAnimatedRightWeaponBackwards;
                FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows = chrAsm.FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows;
            }
        }

        public Dictionary<GameDataManager.GameTypes, ChrAsmConfig> ChrAsmConfigurations { get; set; }
        = new Dictionary<GameDataManager.GameTypes, ChrAsmConfig>
        {
            { GameDataManager.GameTypes.DS1, new ChrAsmConfig()
                {
                    HeadID = 100000,
                    BodyID = 111000,
                    ArmsID = 292000,
                    LegsID = 113000,
                    RightWeaponID = 306000,
                    RightWeaponModelIndex = 0,
                    LeftWeaponID = 1455000,
                    LeftWeaponModelIndex = 0,
                    LeftWeaponFlip_ForDS1Shields = true,
                    FlipAnimatedLeftWeaponBackwards = true,
                    FlipAnimatedRightWeaponBackwards = true,
                }
            },
             { GameDataManager.GameTypes.DS3, new ChrAsmConfig()
                {
                    HeadID = 52000000,
                    BodyID = 52001000,
                    ArmsID = 52002000,
                    LegsID = 52003000,
                    RightWeaponID = 2060000,
                    RightWeaponModelIndex = 0,
                    LeftWeaponID = 21010000,
                    LeftWeaponModelIndex = 0,
                    LeftWeaponFlip_ForDS1Shields = true,
                    FlipAnimatedLeftWeaponBackwards = true,
                    FlipAnimatedRightWeaponBackwards = true,
                    FlipAnimatedLeftWeaponBackwardsOtherWay_ForDS3Bows = true,
                }
            },
             { GameDataManager.GameTypes.BB, new ChrAsmConfig()
                {
                    HeadID = 230000,
                    BodyID = 231000,
                    ArmsID = 232000,
                    LegsID = 233000,
                    RightWeaponID = 7100000,
                    RightWeaponModelIndex = 0,
                    LeftWeaponID = 6000000,
                    LeftWeaponModelIndex = 0,
                    LeftWeaponFlip_ForDS1Shields = true,
                    FlipAnimatedLeftWeaponBackwards = false,
                    FlipAnimatedRightWeaponBackwards = false,
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
