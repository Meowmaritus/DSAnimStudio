using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.TaeEditor
{
    public class TaeProjectJson
    {
        public string GameDirectory { get; set; }
        public string ModEngineDirectory { get; set; }
        public bool LoadLooseParams { get; set; } = false;
        public bool LoadUnpackedGameFiles { get; set; } = false;
        public bool DisableInterrootDCX { get; set; } = false;

        public List<WorldView> WorldViews { get; set; } = new List<WorldView>();

        private Dictionary<int, string> StateInfoSelectConfig_Names { get; set; } = new Dictionary<int, string>();
        private Dictionary<int, bool> StateInfoSelectConfig_Enabled { get; set; } = new Dictionary<int, bool>();

        private Dictionary<string, string> WwiseSwitchControlValues { get; set; } = new Dictionary<string, string>();

        public void InitDefaults(zzz_DocumentIns doc)
        {
            StateInfoSelectConfig_Enabled = ImguiOSD.OSD.WindowEntity.GetStateInfoSelectConfig_Enabled();
            StateInfoSelectConfig_Names = ImguiOSD.OSD.WindowEntity.GetStateInfoSelectConfig_Names();

            doc.SoundManager.WwiseManager.InitDefaultSwitchGroups();
            WwiseSwitchControlValues = doc.SoundManager.WwiseManager.GetSwitchGroupValues();

            if (WorldViews == null)
                WorldViews = new List<WorldView>();
            if (WorldViews.Count == 0)
                WorldViews.Add(new WorldView("Default"));

            doc.WorldViewManager.SetWorldViewList(WorldViews);
        }

        public void Save(string file, zzz_DocumentIns doc)
        {
            WorldViews = doc.WorldViewManager.GetWorldViewList();

            StateInfoSelectConfig_Enabled = ImguiOSD.OSD.WindowEntity.GetStateInfoSelectConfig_Enabled();
            StateInfoSelectConfig_Names = ImguiOSD.OSD.WindowEntity.GetStateInfoSelectConfig_Names();

            WwiseSwitchControlValues = doc.SoundManager.WwiseManager.GetSwitchGroupValues();

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            var dir = System.IO.Path.GetDirectoryName(file);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(file, json);
        }

        public static TaeProjectJson Load(string file, zzz_DocumentIns doc)
        {
            var proj = Newtonsoft.Json.JsonConvert.DeserializeObject<TaeProjectJson>(System.IO.File.ReadAllText(file));

            var worldViews = proj.WorldViews;
            if (worldViews.Count == 0)
                worldViews.Add(new WorldView("Default"));
            doc.WorldViewManager.SetWorldViewList(worldViews);

            ImguiOSD.OSD.WindowEntity.SetStateInfoSelectConfig_Names(proj.StateInfoSelectConfig_Names);
            ImguiOSD.OSD.WindowEntity.SetStateInfoSelectConfig_Enabled(proj.StateInfoSelectConfig_Enabled);

            doc.SoundManager.WwiseManager.SetSwitchGroupValues(proj.WwiseSwitchControlValues);

            return proj;
        }
    }
}
