using System.Collections.Generic;

namespace TAEDX.TaeEditor
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
    }
}
