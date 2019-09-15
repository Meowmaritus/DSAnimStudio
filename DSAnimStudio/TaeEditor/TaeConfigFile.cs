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

        public TaeConfigFile()
        {
            EventSimulationsEnabled = new Dictionary<string, bool>();
            foreach (var kvp in EventSim.Entries)
            {
                if (kvp.Value.MenuOptionName != null)
                    EventSimulationsEnabled.Add(kvp.Key, kvp.Value.IsEnabledByDefault);
            }
        }


    }
}
