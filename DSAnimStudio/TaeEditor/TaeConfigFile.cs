using System.Collections.Generic;

namespace DSAnimStudio.TaeEditor
{
    public class TaeConfigFile
    {
        public bool EnableColorBlindMode { get; set; } = false;
        public bool EnableFancyScrollingStrings { get; set; } = true;
        public float FancyScrollingStringsScrollSpeed { get; set; } = 64;
        public bool FancyTextScrollSnapsToPixels { get; set; } = true;
        public bool AutoCollapseAllTaeSections { get; set; } = false;
        public bool AutoScrollDuringAnimPlayback { get; set; } = true;
        public List<string> RecentFilesList { get; set; } = new List<string>();
        public bool LiveRefreshOnSave { get; set; } = true;

        public string DS1_Parts_HD { get; set; } = "HD_A_9220";
        public string DS1_Parts_BD { get; set; } = "BD_M_9510";
        public string DS1_Parts_AM { get; set; } = "AM_M_9400";
        public string DS1_Parts_LG { get; set; } = "LG_M_9220";
        public string DS1_Parts_WP_L { get; set; } = "WP_A_1504";
        public int DS1_Parts_WP_L_ModelIndex { get; set; } = 0;
        public string DS1_Parts_WP_R { get; set; } = "WP_A_0220";
        public int DS1_Parts_WP_R_ModelIndex { get; set; } = 0;

        public string BB_Parts_HD { get; set; } = "HD_M_8000";
        public string BB_Parts_BD { get; set; } = "BD_M_8000";
        public string BB_Parts_AM { get; set; } = "AM_M_8000";
        public string BB_Parts_LG { get; set; } = "LG_M_8000";
        public string BB_Parts_WP_L { get; set; } = "WP_A_1801";
        public int BB_Parts_WP_L_ModelIndex { get; set; } = 0;
        public string BB_Parts_WP_R { get; set; } = "WP_A_0800";
        public int BB_Parts_WP_R_ModelIndex { get; set; } = 0;

        public string DS3_Parts_HD { get; set; } = "HD_M_2100";
        public string DS3_Parts_BD { get; set; } = "BD_M_2100";
        public string DS3_Parts_AM { get; set; } = "AM_M_2100";
        public string DS3_Parts_LG { get; set; } = "LG_M_2100";
        public string DS3_Parts_WP_L { get; set; } = "WP_A_1101";
        public int DS3_Parts_WP_L_ModelIndex { get; set; } = 0;
        public string DS3_Parts_WP_R { get; set; } = "WP_A_0634";
        public int DS3_Parts_WP_R_ModelIndex { get; set; } = 0;
    }
}
