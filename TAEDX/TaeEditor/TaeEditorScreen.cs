using MeowDSIO.DataFiles;
using MeowDSIO.DataTypes.TAE;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAEDX.TaeEditor
{
    public class TaeEditorScreen
    {
        enum DividerDragMode
        {
            None,
            //Left,
            Right,
        }

        enum ScreenMouseHoverKind
        {
            None,
            AnimList,
            EventGraph,
            Inspector
        }

        private const int RECENT_FILES_MAX = 24;

        private int TopMenuBarMargin = 32;

        private int TopOfGraphAnimInfoMargin = 24;
        private int ButtonEditCurrentAnimInfoWidth = 200;
        private System.Windows.Forms.Button ButtonEditCurrentAnimInfo;

        const string HELP_TEXT = 
            "Left Click + Drag Middle of Event:\n" +
            "    Move whole event\n" +
            "Left Click + Drag Left/Right Side of Event:\n" +
            "    Move start/end of event\n" +
            "Left Click:\n" +
            "    Highlight event under mouse cursor\n" +
            "Right Click:\n" +
            "    Place copy of last highlighted event at mouse cursor\n" +
            "Delete Key:\n" +
            "    Delete highlighted event.\n\n\n" +
            "The pane on the right shows the parameters of the highlighted event." +
            "Click \"Change Type\" on the upper-right corner to change the event type of the highlighted event." +
            "F1 Key:\n" +
            "    Change type of highlighted event.\n";

        private static object _lock_PauseUpdate = new object();
        private bool _PauseUpdate;
        private bool PauseUpdate
        {
            get
            {
                lock (_lock_PauseUpdate)
                    return _PauseUpdate;
            }
            set
            {
                lock (_lock_PauseUpdate)
                    _PauseUpdate = value;
            }
        }
        //private float _PauseUpdateTotalTime;
        //private float PauseUpdateTotalTime
        //{
        //    get
        //    {
        //        lock (_lock_PauseUpdate)
        //            return _PauseUpdateTotalTime;
        //    }
        //    set
        //    {
        //        lock (_lock_PauseUpdate)
        //            _PauseUpdateTotalTime = value;
        //    }
        //}

        public Rectangle Rect;

        public Dictionary<AnimationRef, TaeUndoMan> UndoManDictionary 
            = new Dictionary<AnimationRef, TaeUndoMan>();

        public TaeUndoMan UndoMan
        {
            get
            {
                if (!UndoManDictionary.ContainsKey(SelectedTaeAnim))
                {
                    var newUndoMan = new TaeUndoMan();
                    newUndoMan.CanUndoMaybeChanged += UndoMan_CanUndoMaybeChanged;
                    newUndoMan.CanRedoMaybeChanged += UndoMan_CanRedoMaybeChanged;
                    UndoManDictionary.Add(SelectedTaeAnim, newUndoMan);
                }
                return UndoManDictionary[SelectedTaeAnim];
            }
        }

        private bool _IsModified = false;
        public bool IsModified
        {
            get => _IsModified;
            set
            {
                _IsModified = value;
                ToolStripFileSave.Enabled = value;
            }
        }

        private System.Windows.Forms.ToolStripMenuItem ToolStripFileSave;
        private System.Windows.Forms.ToolStripMenuItem ToolStripFileSaveAs;
        private System.Windows.Forms.ToolStripMenuItem ToolStripFileRecent;

        private void PushNewRecentFile(string fileName)
        {
            while (Config.RecentFilesList.Contains(fileName))
                Config.RecentFilesList.Remove(fileName);

            while (Config.RecentFilesList.Count >= RECENT_FILES_MAX)
                Config.RecentFilesList.RemoveAt(Config.RecentFilesList.Count - 1);

            Config.RecentFilesList.Insert(0, fileName);

            SaveConfig();

            CreateRecentFilesList();
        }

        private void CreateRecentFilesList()
        {
            ToolStripFileRecent.DropDownItems.Clear();
            var toolStripFileRecentClear = new System.Windows.Forms.ToolStripMenuItem("Clear All Recent Files...");
            toolStripFileRecentClear.Click += (s, e) =>
            {
                var askYesNoResult = System.Windows.Forms.MessageBox.Show(
                    "Are you sure you wish to remove all recent files?",
                    "Remove All Recent Files?",
                    System.Windows.Forms.MessageBoxButtons.YesNo);

                if (askYesNoResult == System.Windows.Forms.DialogResult.Yes)
                {
                    Config.RecentFilesList.Clear();
                    SaveConfig();
                }
            };
            ToolStripFileRecent.DropDownItems.Add(toolStripFileRecentClear);
            ToolStripFileRecent.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());
            foreach (var f in Config.RecentFilesList)
            {
                var thisRecentFileEntry = new System.Windows.Forms.ToolStripMenuItem(f);
                thisRecentFileEntry.Click += (s, e) =>
                {
                    DirectOpenFile(f);
                };
                ToolStripFileRecent.DropDownItems.Add(thisRecentFileEntry);
            }
        }

        private void UndoMan_CanRedoMaybeChanged(object sender, EventArgs e)
        {
            ToolStripEditRedo.Enabled = UndoMan.CanRedo;
        }

        private void UndoMan_CanUndoMaybeChanged(object sender, EventArgs e)
        {
            ToolStripEditUndo.Enabled = UndoMan.CanUndo;
        }


        private TaeButtonRepeater UndoButton = new TaeButtonRepeater(0.4f, 0.05f);
        private TaeButtonRepeater RedoButton = new TaeButtonRepeater(0.4f, 0.05f);
        private System.Windows.Forms.ToolStripMenuItem ToolStripEditUndo;
        private System.Windows.Forms.ToolStripMenuItem ToolStripEditRedo;

        //private System.Windows.Forms.ToolStripMenuItem ToolStripAccessibilityDisableRainbow;
        private System.Windows.Forms.ToolStripMenuItem ToolStripConfigColorBlindMode;
        private System.Windows.Forms.ToolStripMenuItem ToolStripConfigFancyTextScroll;

        private float LeftSectionWidth = 135;
        private const float LeftSectionWidthMin = 135;
        private float DividerLeftGrabStart => Rect.Left + LeftSectionWidth;
        private float DividerLeftGrabEnd => Rect.Left + LeftSectionWidth + DividerHitboxPad;

        private float RightSectionWidth = 320;
        private const float RightSectionWidthMin = 128;
        private float DividerRightGrabStart => Rect.Right - RightSectionWidth - DividerHitboxPad;
        private float DividerRightGrabEnd => Rect.Right - RightSectionWidth;

        private float LeftSectionStartX => Rect.Left;
        private float MiddleSectionStartX => DividerLeftGrabEnd;
        private float RightSectionStartX => Rect.Right - RightSectionWidth;

        private float MiddleSectionWidth => DividerRightGrabStart - DividerLeftGrabEnd;

        //private float DividerVisiblePad = 12;
        private float DividerHitboxPad = 4;

        private DividerDragMode CurrentDividerDragMode = DividerDragMode.None;

        private ScreenMouseHoverKind MouseHoverKind = ScreenMouseHoverKind.None;
        private ScreenMouseHoverKind oldMouseHoverKind = ScreenMouseHoverKind.None;

        public ANIBND Anibnd;

        public TAE SelectedTae { get; private set; }

        public AnimationRef SelectedTaeAnim { get; private set; }
        private TaeScrollingString SelectedTaeAnimInfoScrollingText = new TaeScrollingString();

        public readonly System.Windows.Forms.Form GameWindowAsForm;

        public void UpdateInspectorToSelection()
        {
            if (SelectedEventBox == null)
            {
                //if (MultiSelectedEventBoxes.Count == 1)
                //{
                //    SelectedEventBox = MultiSelectedEventBoxes[0];
                //    MultiSelectedEventBoxes.Clear();
                //    inspectorWinFormsControl.labelEventType.Text =
                //        SelectedEventBox.MyEvent.EventType.ToString();
                //    inspectorWinFormsControl.buttonChangeType.Enabled = true;
                //}
                if (MultiSelectedEventBoxes.Count > 0)
                {
                    inspectorWinFormsControl.labelEventType.Text = "(Multiple Selected)";
                    inspectorWinFormsControl.buttonChangeType.Enabled = false;
                }
                else
                {
                    inspectorWinFormsControl.labelEventType.Text = "(Nothing Selected)";
                    inspectorWinFormsControl.buttonChangeType.Enabled = false;
                }
            }
            else
            {
                inspectorWinFormsControl.labelEventType.Text =
                    SelectedEventBox.MyEvent.EventType.ToString();
                inspectorWinFormsControl.buttonChangeType.Enabled = true;
            }
        }

        public TaeEditAnimEventBox HoveringOverEventBox = null;

        private TaeEditAnimEventBox _selectedEventBox = null;
        public TaeEditAnimEventBox SelectedEventBox
        {
            get => _selectedEventBox;
            set
            {
                _selectedEventBox = value;

                if (_selectedEventBox == null)
                {
                    //inspectorWinFormsControl.buttonChangeType.Enabled = false;
                }
                else
                {
                    //inspectorWinFormsControl.buttonChangeType.Enabled = true;

                    // If one box was just selected, clear the multi-select
                    MultiSelectedEventBoxes.Clear();
                }
                inspectorWinFormsControl.propertyGrid.SelectedObject = _selectedEventBox?.MyEvent;

                UpdateInspectorToSelection();
            }
        }

        public List<TaeEditAnimEventBox> MultiSelectedEventBoxes = new List<TaeEditAnimEventBox>();

        private TaeEditAnimList editScreenAnimList;
        private TaeEditAnimEventGraph editScreenCurrentAnim;
        //private TaeEditAnimEventGraphInspector editScreenGraphInspector;

        private Color ColorInspectorBG = Color.DarkGray;
        private TaeInspectorWinFormsControl inspectorWinFormsControl;

        public TaeInputHandler Input;

        private System.Windows.Forms.MenuStrip WinFormsMenuStrip;

        public string AnibndFileName = "";

        public TaeConfigFile Config = new TaeConfigFile();

        private static string ConfigFilePath = null;

        private static void CheckConfigFilePath()
        {
            if (ConfigFilePath == null)
            {
                var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentAssemblyDir = System.IO.Path.GetDirectoryName(currentAssemblyPath);
                ConfigFilePath = System.IO.Path.Combine(currentAssemblyDir, "TAE Editor DX - Configuration.json");
            }
        }

        public void LoadConfig()
        {
            CheckConfigFilePath();
            if (!System.IO.File.Exists(ConfigFilePath))
            {
                Config = new TaeConfigFile();
                SaveConfig();
            }

            var jsonText = System.IO.File.ReadAllText(ConfigFilePath);

            Config = Newtonsoft.Json.JsonConvert.DeserializeObject<TaeConfigFile>(jsonText);
        }

        public void SaveConfig()
        {
            CheckConfigFilePath();

            var jsonText = Newtonsoft.Json.JsonConvert
                .SerializeObject(Config,
                Newtonsoft.Json.Formatting.Indented);

            System.IO.File.WriteAllText(ConfigFilePath, jsonText);
        }

        public bool? LoadCurrentFile()
        {
            if (System.IO.File.Exists(AnibndFileName))
            {
                ANIBND newAnibnd = null;
                if (AnibndFileName.ToUpper().EndsWith(".DCX"))
                    newAnibnd = MeowDSIO.DataFile.LoadFromDcxFile<ANIBND>(AnibndFileName);
                else
                    newAnibnd = MeowDSIO.DataFile.LoadFromFile<ANIBND>(AnibndFileName);

                if (newAnibnd.AllTAE.Any())
                {
                    LoadANIBND(newAnibnd);
                    PushNewRecentFile(AnibndFileName);
                    ToolStripFileSaveAs.Enabled = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return null;
            }
        }

        public void SaveCurrentFile()
        {
            if (System.IO.File.Exists(AnibndFileName) && 
                !System.IO.File.Exists(AnibndFileName + ".taedxbak"))
            {
                System.IO.File.Copy(AnibndFileName, AnibndFileName + ".taedxbak");
                System.Windows.Forms.MessageBox.Show(
                    "A backup was not found and was created:\n" + AnibndFileName + ".taedxbak",
                    "Backup Created", System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            if (AnibndFileName.ToUpper().EndsWith(".DCX"))
                MeowDSIO.DataFile.SaveToDcxFile(Anibnd, AnibndFileName);
            else
                MeowDSIO.DataFile.SaveToFile(Anibnd, AnibndFileName);

            foreach (var tae in Anibnd.AllTAE)
            {
                foreach (var animRef in tae.Animations)
                {
                    animRef.IsModified = false;
                }
            }
            
            IsModified = false;
        }

        private void LoadANIBND(ANIBND anibnd)
        {
            Anibnd = anibnd;
            SelectedTae = Anibnd.AllTAE.First();
            SelectedTaeAnim = SelectedTae.Animations[0];
            editScreenAnimList = new TaeEditAnimList(this);
            editScreenCurrentAnim = new TaeEditAnimEventGraph(this);
            SelectNewAnimRef(SelectedTae, SelectedTae.Animations[0]);
            ButtonEditCurrentAnimInfo.Enabled = true;
        }

        public void RecreateAnimList()
        {
            Vector2 oldScroll = editScreenAnimList.ScrollViewer.Scroll;
            editScreenAnimList = new TaeEditAnimList(this);
            editScreenAnimList.ScrollViewer.Scroll = oldScroll;
        }

        public void AddNewAnimation()
        {
            var newAnimRef = new AnimationRef()
            {
                ID = SelectedTaeAnim.ID,
                IsReference = SelectedTaeAnim.IsReference,
                FileName = SelectedTaeAnim.FileName,
                OriginalAnimID = SelectedTaeAnim.OriginalAnimID,
                RefAnimID = SelectedTaeAnim.RefAnimID,
                TAEDataOnly = SelectedTaeAnim.TAEDataOnly,
                UseHKXOnly = SelectedTaeAnim.UseHKXOnly,
                IsLoopingObjAnim = SelectedTaeAnim.IsLoopingObjAnim,
                IsModified = true,
            };

            var index = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
            SelectedTae.Animations.Insert(index + 1, newAnimRef);

            RecreateAnimList();

            SelectNewAnimRef(SelectedTae, newAnimRef);
        }

        public TaeEditorScreen(System.Windows.Forms.Form gameWindowAsForm)
        {
            LoadConfig();

            gameWindowAsForm.FormClosing += GameWindowAsForm_FormClosing;

            GameWindowAsForm = gameWindowAsForm;

            GameWindowAsForm.MinimumSize = new System.Drawing.Size(720, 480);

            Input = new TaeInputHandler();

            //editScreenAnimList = new TaeEditAnimList(this);
            //editScreenCurrentAnim = new TaeEditAnimEventGraph(this);
            //editScreenGraphInspector = new TaeEditAnimEventGraphInspector(this);

            inspectorWinFormsControl = new TaeInspectorWinFormsControl();

            // This might change in the future if I actually add text description attributes to some things.
            inspectorWinFormsControl.propertyGrid.HelpVisible = false;

            inspectorWinFormsControl.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            inspectorWinFormsControl.propertyGrid.ToolbarVisible = false;

            //inspectorPropertyGrid.ViewBackColor = System.Drawing.Color.FromArgb(
            //    ColorInspectorBG.A, ColorInspectorBG.R, ColorInspectorBG.G, ColorInspectorBG.B);

            inspectorWinFormsControl.propertyGrid.LargeButtons = true;

            inspectorWinFormsControl.propertyGrid.CanShowVisualStyleGlyphs = false;

            inspectorWinFormsControl.buttonChangeType.Click += ButtonChangeType_Click;

            inspectorWinFormsControl.propertyGrid.PropertyValueChanged += PropertyGrid_PropertyValueChanged;

            GameWindowAsForm.Controls.Add(inspectorWinFormsControl);

            var toolstripFile = new System.Windows.Forms.ToolStripMenuItem("File");
            {
                var toolstripFile_Open = new System.Windows.Forms.ToolStripMenuItem("Open");
                toolstripFile_Open.Click += ToolstripFile_Open_Click;
                toolstripFile.DropDownItems.Add(toolstripFile_Open);

                toolstripFile.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

                ToolStripFileRecent = new System.Windows.Forms.ToolStripMenuItem("Recent Files");
                CreateRecentFilesList();
                toolstripFile.DropDownItems.Add(ToolStripFileRecent);

                toolstripFile.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

                ToolStripFileSave = new System.Windows.Forms.ToolStripMenuItem("Save");
                ToolStripFileSave.Enabled = false;
                ToolStripFileSave.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S;
                ToolStripFileSave.Click += ToolstripFile_Save_Click;
                toolstripFile.DropDownItems.Add(ToolStripFileSave);

                ToolStripFileSaveAs = new System.Windows.Forms.ToolStripMenuItem("Save As...");
                ToolStripFileSaveAs.Enabled = false;
                ToolStripFileSaveAs.Click += ToolstripFile_SaveAs_Click;
                toolstripFile.DropDownItems.Add(ToolStripFileSaveAs);

                
            }

            var toolstripEdit = new System.Windows.Forms.ToolStripMenuItem("Edit");
            {
                ToolStripEditUndo = new System.Windows.Forms.ToolStripMenuItem("Undo");
                ToolStripEditUndo.ShortcutKeyDisplayString = "Ctrl+Z";
                ToolStripEditUndo.Click += ToolStripEditUndo_Click;
                toolstripEdit.DropDownItems.Add(ToolStripEditUndo);

                ToolStripEditRedo = new System.Windows.Forms.ToolStripMenuItem("Redo");
                ToolStripEditRedo.ShortcutKeyDisplayString = "Ctrl+Y";
                ToolStripEditRedo.Click += ToolStripEditRedo_Click;
                toolstripEdit.DropDownItems.Add(ToolStripEditRedo);

                toolstripEdit.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

                var toolStripEditCollapseAllTaeSections = new System.Windows.Forms.ToolStripMenuItem("Collapse All TAE Sections");
                toolStripEditCollapseAllTaeSections.Click += (s, e) =>
                {
                    foreach (var kvp in editScreenAnimList.AnimTaeSections)
                    {
                        kvp.Collapsed = true;
                    }
                };
                toolstripEdit.DropDownItems.Add(toolStripEditCollapseAllTaeSections);

                var toolStripEditExpandAllTaeSections = new System.Windows.Forms.ToolStripMenuItem("Expand All TAE Sections");
                toolStripEditExpandAllTaeSections.Click += (s, e) =>
                {
                    foreach (var kvp in editScreenAnimList.AnimTaeSections)
                    {
                        kvp.Collapsed = false;
                    }
                };
                toolstripEdit.DropDownItems.Add(toolStripEditExpandAllTaeSections);
            }

            var toolstripConfig = new System.Windows.Forms.ToolStripMenuItem("Config");
            {
                //ToolStripAccessibilityDisableRainbow = new System.Windows.Forms.ToolStripMenuItem("Use brightness for selection instead of rainbow pulsing");
                //ToolStripAccessibilityDisableRainbow.CheckOnClick = true;
                //ToolStripAccessibilityDisableRainbow.CheckedChanged += ToolStripAccessibilityDisableRainbow_CheckedChanged;
                //ToolStripAccessibilityDisableRainbow.Checked = Config.DisableRainbow;
                //toolstripAccessibility.DropDownItems.Add(ToolStripAccessibilityDisableRainbow);

                ToolStripConfigColorBlindMode = new System.Windows.Forms.ToolStripMenuItem("Color-Blind + High Contrast Mode");
                ToolStripConfigColorBlindMode.CheckOnClick = true;
                ToolStripConfigColorBlindMode.CheckedChanged += (s, e) =>
                {
                    Config.EnableColorBlindMode = ToolStripConfigColorBlindMode.Checked;
                    SaveConfig();
                };
                ToolStripConfigColorBlindMode.Checked = Config.EnableColorBlindMode;
                toolstripConfig.DropDownItems.Add(ToolStripConfigColorBlindMode);

                toolstripConfig.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

                ToolStripConfigFancyTextScroll = new System.Windows.Forms.ToolStripMenuItem("Use Fancy Text Scrolling");
                ToolStripConfigFancyTextScroll.CheckOnClick = true;
                ToolStripConfigFancyTextScroll.CheckedChanged += (s, e) =>
                {
                    Config.EnableFancyScrollingStrings = ToolStripConfigFancyTextScroll.Checked;
                    SaveConfig();
                };
                ToolStripConfigFancyTextScroll.Checked = Config.EnableFancyScrollingStrings;
                toolstripConfig.DropDownItems.Add(ToolStripConfigFancyTextScroll);

                var toolStripConfigFancyScrollSpeed = new System.Windows.Forms.ToolStripMenuItem("Fancy Text Scroll Speed");
                {
                    

                    var toolStripConfigFancyScrollSpeed_ExtremelySlow 
                        = new System.Windows.Forms.ToolStripMenuItem("Extremely Slow (4 px/s)");
                    var toolStripConfigFancyScrollSpeed_VerySlow
                        = new System.Windows.Forms.ToolStripMenuItem("Very Slow (8 px/s)");
                    var toolStripConfigFancyScrollSpeed_Slow
                        = new System.Windows.Forms.ToolStripMenuItem("Slow (16 px/s)");
                    var toolStripConfigFancyScrollSpeed_Medium
                        = new System.Windows.Forms.ToolStripMenuItem("Medium Speed (32 px/s)");
                    var toolStripConfigFancyScrollSpeed_Fast
                        = new System.Windows.Forms.ToolStripMenuItem("Fast (64 px/s)");
                    var toolStripConfigFancyScrollSpeed_VeryFast
                        = new System.Windows.Forms.ToolStripMenuItem("Very Fast (128 px/s)");
                    var toolStripConfigFancyScrollSpeed_ExtremelyFast
                        = new System.Windows.Forms.ToolStripMenuItem("Extremely Fast (256 px/s)");

                    void updateToolStripConfigFancyScrollSpeedChecks()
                    {
                        toolStripConfigFancyScrollSpeed_ExtremelySlow.Checked = Config.FancyScrollingStringsScrollSpeed == 4;
                        toolStripConfigFancyScrollSpeed_VerySlow.Checked = Config.FancyScrollingStringsScrollSpeed == 8;
                        toolStripConfigFancyScrollSpeed_Slow.Checked = Config.FancyScrollingStringsScrollSpeed == 16;
                        toolStripConfigFancyScrollSpeed_Medium.Checked = Config.FancyScrollingStringsScrollSpeed == 32;
                        toolStripConfigFancyScrollSpeed_Fast.Checked = Config.FancyScrollingStringsScrollSpeed == 64;
                        toolStripConfigFancyScrollSpeed_VeryFast.Checked = Config.FancyScrollingStringsScrollSpeed == 128;
                        toolStripConfigFancyScrollSpeed_ExtremelyFast.Checked = Config.FancyScrollingStringsScrollSpeed == 256;
                    }

                    toolStripConfigFancyScrollSpeed_ExtremelySlow.Click += (s, e) =>
                    {
                        Config.FancyScrollingStringsScrollSpeed = 4;
                        SaveConfig();
                        updateToolStripConfigFancyScrollSpeedChecks();
                    };
                    toolStripConfigFancyScrollSpeed.DropDownItems
                        .Add(toolStripConfigFancyScrollSpeed_ExtremelySlow);

                    
                    toolStripConfigFancyScrollSpeed_VerySlow.Click += (s, e) =>
                    {
                        Config.FancyScrollingStringsScrollSpeed = 8;
                        SaveConfig();
                        updateToolStripConfigFancyScrollSpeedChecks();
                    };
                    toolStripConfigFancyScrollSpeed.DropDownItems
                        .Add(toolStripConfigFancyScrollSpeed_VerySlow);

                    
                    toolStripConfigFancyScrollSpeed_Slow.Click += (s, e) =>
                    {
                        Config.FancyScrollingStringsScrollSpeed = 16;
                        SaveConfig();
                        updateToolStripConfigFancyScrollSpeedChecks();
                    };
                    toolStripConfigFancyScrollSpeed.DropDownItems
                        .Add(toolStripConfigFancyScrollSpeed_Slow);

                    
                    toolStripConfigFancyScrollSpeed_Medium.Click += (s, e) =>
                    {
                        Config.FancyScrollingStringsScrollSpeed = 32;
                        SaveConfig();
                        updateToolStripConfigFancyScrollSpeedChecks();
                    };
                    toolStripConfigFancyScrollSpeed.DropDownItems
                        .Add(toolStripConfigFancyScrollSpeed_Medium);

                    
                    toolStripConfigFancyScrollSpeed_Fast.Click += (s, e) =>
                    {
                        Config.FancyScrollingStringsScrollSpeed = 64;
                        SaveConfig();
                        updateToolStripConfigFancyScrollSpeedChecks();
                    };
                    toolStripConfigFancyScrollSpeed.DropDownItems
                        .Add(toolStripConfigFancyScrollSpeed_Fast);

                    
                    toolStripConfigFancyScrollSpeed_VeryFast.Click += (s, e) =>
                    {
                        Config.FancyScrollingStringsScrollSpeed = 128;
                        SaveConfig();
                        updateToolStripConfigFancyScrollSpeedChecks();
                    };
                    toolStripConfigFancyScrollSpeed.DropDownItems
                        .Add(toolStripConfigFancyScrollSpeed_VeryFast);

                    
                    toolStripConfigFancyScrollSpeed_ExtremelyFast.Click += (s, e) =>
                    {
                        Config.FancyScrollingStringsScrollSpeed = 256;
                        SaveConfig();
                        updateToolStripConfigFancyScrollSpeedChecks();
                    };
                    toolStripConfigFancyScrollSpeed.DropDownItems
                        .Add(toolStripConfigFancyScrollSpeed_ExtremelyFast);

                    updateToolStripConfigFancyScrollSpeedChecks();
                }
                toolstripConfig.DropDownItems.Add(toolStripConfigFancyScrollSpeed);

                var toolStripConfigFancyScrollSnapsToPixels = 
                    new System.Windows.Forms.ToolStripMenuItem("Fancy Scroll Snaps to Pixels");

                toolStripConfigFancyScrollSnapsToPixels.CheckedChanged += (s, e) =>
                {
                    Config.FancyTextScrollSnapsToPixels = toolStripConfigFancyScrollSnapsToPixels.Checked;
                    SaveConfig();
                };

                toolStripConfigFancyScrollSnapsToPixels.CheckOnClick = true;

                toolStripConfigFancyScrollSnapsToPixels.Checked = Config.FancyTextScrollSnapsToPixels;

                toolstripConfig.DropDownItems.Add(toolStripConfigFancyScrollSnapsToPixels);

                toolstripConfig.DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());

                var toolStripConfigCollapseAllTaeSectionsByDefault = new System.Windows.Forms.ToolStripMenuItem("Auto Collapse All TAE Sections");

                toolStripConfigCollapseAllTaeSectionsByDefault.CheckOnClick = true;

                toolStripConfigCollapseAllTaeSectionsByDefault.CheckedChanged += (s, e) =>
                {
                    Config.AutoCollapseAllTaeSections = toolStripConfigCollapseAllTaeSectionsByDefault.Checked;
                    SaveConfig();
                };

                toolStripConfigCollapseAllTaeSectionsByDefault.Checked = Config.AutoCollapseAllTaeSections;

                toolstripConfig.DropDownItems.Add(toolStripConfigCollapseAllTaeSectionsByDefault);
            }

            var toolstripHelp = new System.Windows.Forms.ToolStripMenuItem("Help");
            toolstripHelp.Click += ToolstripHelp_Click;

            WinFormsMenuStrip = new System.Windows.Forms.MenuStrip();
            WinFormsMenuStrip.Items.Add(toolstripFile);
            WinFormsMenuStrip.Items.Add(toolstripEdit);
            WinFormsMenuStrip.Items.Add(toolstripConfig);
            WinFormsMenuStrip.Items.Add(toolstripHelp);

            WinFormsMenuStrip.MenuActivate += WinFormsMenuStrip_MenuActivate;
            WinFormsMenuStrip.MenuDeactivate += WinFormsMenuStrip_MenuDeactivate;

            GameWindowAsForm.Controls.Add(WinFormsMenuStrip);

            ButtonEditCurrentAnimInfo = new System.Windows.Forms.Button();
            ButtonEditCurrentAnimInfo.Text = "Edit Anim Info...";
            ButtonEditCurrentAnimInfo.Click += ButtonEditCurrentAnimInfo_Click;
            ButtonEditCurrentAnimInfo.Enabled = false;

            GameWindowAsForm.Controls.Add(ButtonEditCurrentAnimInfo);
        }

        private void ButtonEditCurrentAnimInfo_Click(object sender, EventArgs e)
        {
            PauseUpdate = true;
            var editForm = new TaeEditAnimPropertiesForm(SelectedTaeAnim);
            editForm.Owner = GameWindowAsForm;
            editForm.ShowDialog();

            if (editForm.WasAnimDeleted)
            {
                if (SelectedTae.Animations.Count <= 1)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Cannot delete the only animation remaining in the TAE.", 
                        "Can't Delete Last Animation",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Stop);
                }
                else
                {
                    var indexOfCurrentAnim = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
                    SelectedTae.Animations.Remove(SelectedTaeAnim);
                    RecreateAnimList();

                    if (indexOfCurrentAnim > SelectedTae.Animations.Count - 1)
                        indexOfCurrentAnim = SelectedTae.Animations.Count - 1;

                    if (indexOfCurrentAnim >= 0)
                        SelectNewAnimRef(SelectedTae, SelectedTae.Animations[indexOfCurrentAnim]);
                    else
                        SelectNewAnimRef(SelectedTae, SelectedTae[0]);

                    IsModified = true;
                }
            }
            else
            {
                if (editForm.WasAnimIDChanged)
                {
                    SelectedTaeAnim.IsModified = true;
                    IsModified = true;
                    RecreateAnimList();
                    UpdateSelectedTaeAnimInfoText();
                }

                if (editForm.WereThingsChanged)
                {
                    SelectedTaeAnim.IsModified = true;
                    IsModified = true;
                    UpdateSelectedTaeAnimInfoText();
                }
            }
            
            PauseUpdate = false;
        }

        private void GameWindowAsForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            SaveConfig();

            var unsavedChanges = IsModified;

            if (!unsavedChanges && Anibnd != null)
            {
                if (Anibnd.IsModified)
                {
                    unsavedChanges = true;
                }
                else
                {
                    foreach (var tae in Anibnd.AllTAE)
                    {
                        foreach (var anim in tae.Animations)
                        {
                            if (anim.IsModified)
                            {
                                unsavedChanges = true;
                                break;
                            }
                        }
                    }
                }
            }

            if (unsavedChanges)
            {
                var confirmDlg = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(AnibndFileName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"closing?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (confirmDlg == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile();
                }
                else if (confirmDlg == System.Windows.Forms.DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = false;
            }

            
        }

        private void PropertyGrid_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
        {
            var gridReference = (System.Windows.Forms.PropertyGrid)s;
            var boxReference = SelectedEventBox;
            var newValReference = e.ChangedItem.Value;
            var oldValReference = e.OldValue;

            UndoMan.NewAction(doAction: () =>
            {
                e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, newValReference);

                SelectedTaeAnim.IsModified = true;
                IsModified = true;

                gridReference.Refresh();
            },
            undoAction: () =>
            {
                e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, oldValReference);

                SelectedTaeAnim.IsModified = true;
                IsModified = true;

                gridReference.Refresh();
            });
        }

        private void ToolStripEditRedo_Click(object sender, EventArgs e)
        {
            UndoMan.Redo();
        }

        private void ToolStripEditUndo_Click(object sender, EventArgs e)
        {
            UndoMan.Undo();
        }

        private void ToolstripHelp_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(HELP_TEXT, "TAE Editor Help",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        private void WinFormsMenuStrip_MenuDeactivate(object sender, EventArgs e)
        {
            PauseUpdate = false;
        }

        private void WinFormsMenuStrip_MenuActivate(object sender, EventArgs e)
        {
            PauseUpdate = true;
            Input.CursorType = MouseCursorType.Arrow;
        }

        private void DirectOpenFile(string fileName)
        {
            if (Anibnd != null && Anibnd.AllTAE.Any(x => x.Animations.Any(a => a.IsModified)))
            {
                var yesNoCancel = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(AnibndFileName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"loading a new file?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (yesNoCancel == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile();
                }
                else if (yesNoCancel == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                //If they chose no, continue as normal.
            }

            AnibndFileName = fileName;
            var loadFileResult = LoadCurrentFile();
            if (loadFileResult == false)
            {
                AnibndFileName = "";
                System.Windows.Forms.MessageBox.Show(
                    "Selected ANIBND file had no TAE files within. " +
                    "Cancelling load operation.", "Invalid ANIBND",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Stop);
            }
            else if (loadFileResult == null)
            {
                AnibndFileName = "";
                System.Windows.Forms.MessageBox.Show(
                    "ANIBND file did not exist.", "ANIBND Does Not Exist",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Stop);
            }
        }

        private void ToolstripFile_Open_Click(object sender, EventArgs e)
        {
            if (Anibnd != null && Anibnd.AllTAE.Any(x => x.Animations.Any(a => a.IsModified)))
            {
                var yesNoCancel = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(AnibndFileName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"loading a new file?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (yesNoCancel == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile();
                }
                else if (yesNoCancel == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                //If they chose no, continue as normal.
            }

            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "ANIBND Files (*.ANIBND)|*.ANIBND|Compressed ANIBND Files(*.ANIBND.DCX)|*.ANIBND.DCX|All Files|*.*",
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };

            if (AnibndFileName != null)
            {
                if (AnibndFileName.ToUpper().EndsWith(".DCX"))
                    browseDlg.FilterIndex = 1;
                else
                    browseDlg.FilterIndex = 0;
            }

            if (System.IO.File.Exists(AnibndFileName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(AnibndFileName);
                browseDlg.FileName = System.IO.Path.GetFileName(AnibndFileName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AnibndFileName = browseDlg.FileName;
                var loadFileResult = LoadCurrentFile();
                if (loadFileResult == false)
                {
                    AnibndFileName = "";
                    System.Windows.Forms.MessageBox.Show(
                        "Selected ANIBND file had no TAE files within. " +
                        "Cancelling load operation.", "Invalid ANIBND",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Stop);
                }
                else if (loadFileResult == null)
                {
                    AnibndFileName = "";
                    System.Windows.Forms.MessageBox.Show(
                        "Selected ANIBND file did not exist (how did you " +
                        "get this message to appear, anyways?).", "ANIBND Does Not Exist",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Stop);
                }
            }
        }


        private void ToolstripFile_Save_Click(object sender, EventArgs e)
        {
            SaveCurrentFile();
        }

        private void ToolstripFile_SaveAs_Click(object sender, EventArgs e)
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "ANIBND Files (*.ANIBND)|*.ANIBND|Compressed ANIBND Files(*.ANIBND.DCX)|*.ANIBND.DCX|All Files|*.*",
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (AnibndFileName != null)
            {
                if (AnibndFileName.ToUpper().EndsWith(".DCX"))
                    browseDlg.FilterIndex = 1;
                else
                    browseDlg.FilterIndex = 0;
            }

            if (System.IO.File.Exists(AnibndFileName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(AnibndFileName);
                browseDlg.FileName = System.IO.Path.GetFileName(AnibndFileName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                AnibndFileName = browseDlg.FileName;
                SaveCurrentFile();
            }
        }

        private void ChangeTypeOfSelectedEvent()
        {
            if (SelectedEventBox == null)
                return;

            PauseUpdate = true;

            var changeTypeDlg = new TaeInspectorFormChangeEventType();
            changeTypeDlg.NewEventType = SelectedEventBox.MyEvent.EventType;

            if (changeTypeDlg.ShowDialog(GameWindowAsForm) == System.Windows.Forms.DialogResult.OK)
            {
                if (changeTypeDlg.NewEventType != SelectedEventBox.MyEvent.EventType)
                {
                    var referenceToEventBox = SelectedEventBox;
                    var referenceToPreviousEvent = referenceToEventBox.MyEvent;
                    int index = SelectedTaeAnim.EventList.IndexOf(referenceToEventBox.MyEvent);
                    int row = referenceToEventBox.MyEvent.Row;

                    UndoMan.NewAction(
                        doAction: () =>
                        {
                            SelectedTaeAnim.EventList.Remove(referenceToPreviousEvent);
                            referenceToEventBox.ChangeEvent(
                                TimeActEventBase.GetNewEvent(
                                    changeTypeDlg.NewEventType,
                                    referenceToPreviousEvent.StartTimeFr,
                                    referenceToPreviousEvent.EndTimeFr));

                            SelectedTaeAnim.EventList.Insert(index, referenceToEventBox.MyEvent);

                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.MyEvent.Row = row;

                            editScreenCurrentAnim.RegisterEventBoxExistance(SelectedEventBox);

                            SelectedTaeAnim.IsModified = true;
                            IsModified = true;
                        },
                        undoAction: () =>
                        {
                            SelectedTaeAnim.EventList.RemoveAt(index);
                            referenceToEventBox.ChangeEvent(referenceToPreviousEvent);
                            SelectedTaeAnim.EventList.Insert(index, referenceToPreviousEvent);

                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.MyEvent.Row = row;

                            editScreenCurrentAnim.RegisterEventBoxExistance(SelectedEventBox);

                            SelectedTaeAnim.IsModified = true;
                            IsModified = true;
                        });
                }
            }

            PauseUpdate = false;
        }

        private void ButtonChangeType_Click(object sender, EventArgs e)
        {
            ChangeTypeOfSelectedEvent();
        }

        public void UpdateSelectedTaeAnimInfoText()
        {
            var stringBuilder = new StringBuilder();

            if (SelectedTaeAnim == null)
            {
                stringBuilder.Append("(No Animation Selected)");
            }
            else
            {
                stringBuilder.Append(SelectedTaeAnim.ID);

                if (SelectedTaeAnim.IsReference)
                {
                    stringBuilder.Append($" [RefID: {SelectedTaeAnim.RefAnimID}]");
                }
                else
                {
                    stringBuilder.Append($" [\"{SelectedTaeAnim.FileName}\"]");

                    if (SelectedTaeAnim.IsLoopingObjAnim)
                        stringBuilder.Append($" [ObjLoop]");

                    if (SelectedTaeAnim.UseHKXOnly)
                        stringBuilder.Append($" [HKXOnly]");

                    if (SelectedTaeAnim.TAEDataOnly)
                        stringBuilder.Append($" [TAEOnly]");

                    if (SelectedTaeAnim.OriginalAnimID >= 0)
                        stringBuilder.Append($" [OrigID: {SelectedTaeAnim.OriginalAnimID}]");
                }
            }

            SelectedTaeAnimInfoScrollingText.SetText(stringBuilder.ToString());
        }

        public void SelectNewAnimRef(TAE tae, AnimationRef animRef)
        {
            SelectedTae = tae;
            SelectedTaeAnim = animRef;

            UpdateSelectedTaeAnimInfoText();

            if (SelectedTaeAnim != null)
            {
                ToolStripEditUndo.Enabled = UndoMan.CanUndo;
                ToolStripEditRedo.Enabled = UndoMan.CanRedo;
                SelectedEventBox = null;

                if (editScreenCurrentAnim == null)
                    editScreenCurrentAnim = new TaeEditAnimEventGraph(this);

                editScreenCurrentAnim.ChangeToNewAnimRef(SelectedTaeAnim);
            }
            else
            {
                ToolStripEditUndo.Enabled = false;
                ToolStripEditRedo.Enabled = false;
                SelectedEventBox = null;

                editScreenCurrentAnim = null;
            }

            
        }

        public void Update(float elapsedSeconds)
        {
            if (PauseUpdate)
            {
                //PauseUpdateTotalTime += elapsedSeconds;
                return;
            }
            else
            {
                //PauseUpdateTotalTime = 0;
            }

            


            Input.Update(Rect);

            if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F1))
                ChangeTypeOfSelectedEvent();

            var ctrlHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.LeftControl) 
                || Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.RightControl);

            var zHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Z);
            var yHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Y);

            if (UndoButton.Update(elapsedSeconds, ctrlHeld && (zHeld && !yHeld)))
            {
                UndoMan.Undo();
            }

            if (RedoButton.Update(elapsedSeconds, ctrlHeld && (!zHeld && yHeld)))
            {
                UndoMan.Redo();
            }

            if (CurrentDividerDragMode == DividerDragMode.None)
            {
                //if (Input.MousePosition.X >= DividerLeftGrabStart && Input.MousePosition.X <= DividerLeftGrabEnd)
                //{
                //    MouseHoverKind = ScreenMouseHoverKind.None;
                //    //Input.CursorType = MouseCursorType.DragX;
                //    if (Input.LeftClickDown)
                //    {
                //        CurrentDividerDragMode = DividerDragMode.Left;
                //    }
                //}
                if (Input.MousePosition.X >= DividerRightGrabStart && Input.MousePosition.X <= DividerRightGrabEnd)
                {
                    MouseHoverKind = ScreenMouseHoverKind.None;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.Right;
                    }
                }
            }
            //else if (CurrentDividerDragMode == DividerDragMode.Left)
            //{
            //    if (Input.LeftClickHeld)
            //    {
            //        //Input.CursorType = MouseCursorType.DragX;
            //        LeftSectionWidth = MathHelper.Max(Input.MousePosition.X - (DividerHitboxPad / 2), LeftSectionWidthMin);
            //    }
            //    else
            //    {
            //        //Input.CursorType = MouseCursorType.Arrow;
            //        CurrentDividerDragMode = DividerDragMode.None;
            //    }
            //}
            else if (CurrentDividerDragMode == DividerDragMode.Right)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragX;
                    RightSectionWidth = MathHelper.Max((Rect.Right - Input.MousePosition.X) + (DividerHitboxPad / 2), RightSectionWidthMin);
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                }
            }

            if (editScreenAnimList != null && editScreenCurrentAnim != null)
            {
                if (editScreenAnimList.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.AnimList;
                else if (editScreenCurrentAnim.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.EventGraph;
                else if (
                    new Rectangle(
                        inspectorWinFormsControl.Bounds.Left,
                        inspectorWinFormsControl.Bounds.Top,
                        inspectorWinFormsControl.Bounds.Width,
                        inspectorWinFormsControl.Bounds.Height
                        )
                        .Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.Inspector;
                else
                    MouseHoverKind = ScreenMouseHoverKind.None;

                if (MouseHoverKind == ScreenMouseHoverKind.AnimList)
                    editScreenAnimList.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                else
                    editScreenAnimList.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

                if (MouseHoverKind == ScreenMouseHoverKind.EventGraph)
                    editScreenCurrentAnim.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                else
                    editScreenCurrentAnim.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

            }
            else
            {
                if (new Rectangle(
                inspectorWinFormsControl.Bounds.Left,
                inspectorWinFormsControl.Bounds.Top,
                inspectorWinFormsControl.Bounds.Width,
                inspectorWinFormsControl.Bounds.Height)
                .Contains(Input.MousePositionPoint))
                {
                    MouseHoverKind = ScreenMouseHoverKind.Inspector;
                }
                else
                {
                    MouseHoverKind = ScreenMouseHoverKind.None;
                }

                Input.CursorType = MouseCursorType.StopUpdating;
            }

            


            if (MouseHoverKind != ScreenMouseHoverKind.None && oldMouseHoverKind == ScreenMouseHoverKind.None)
            {
                //Input.CursorType = MouseCursorType.Arrow;
            }

            if (MouseHoverKind == ScreenMouseHoverKind.Inspector)
                Input.CursorType = MouseCursorType.StopUpdating;

            //if (editScreenGraphInspector.Rect.Contains(Input.MousePositionPoint))
            //    editScreenGraphInspector.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
            //else
            //    editScreenGraphInspector.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

            oldMouseHoverKind = MouseHoverKind;
        }

        private void UpdateLayout()
        {
            if (editScreenAnimList != null && editScreenCurrentAnim != null)
            {
                editScreenAnimList.Rect = new Rectangle(
                    (int)LeftSectionStartX,
                    Rect.Top + TopMenuBarMargin, 
                    (int)LeftSectionWidth, 
                    Rect.Height - TopMenuBarMargin);

                editScreenCurrentAnim.Rect = new Rectangle(
                    (int)MiddleSectionStartX, 
                    Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                    (int)MiddleSectionWidth,
                    Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                ButtonEditCurrentAnimInfo.Bounds = new System.Drawing.Rectangle(
                    editScreenCurrentAnim.Rect.Right - ButtonEditCurrentAnimInfoWidth,
                    Rect.Top + TopMenuBarMargin,
                    ButtonEditCurrentAnimInfoWidth, 
                    TopOfGraphAnimInfoMargin);
            }
            else
            {
                var plannedGraphRect = new Rectangle(
                    (int)MiddleSectionStartX,
                    Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                    (int)MiddleSectionWidth,
                    Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                ButtonEditCurrentAnimInfo.Bounds = new System.Drawing.Rectangle(
                    plannedGraphRect.Right - ButtonEditCurrentAnimInfoWidth, 
                    Rect.Top + TopMenuBarMargin, 
                    ButtonEditCurrentAnimInfoWidth, 
                    TopOfGraphAnimInfoMargin);

            }
            //editScreenGraphInspector.Rect = new Rectangle(Rect.Width - LayoutInspectorWidth, 0, LayoutInspectorWidth, Rect.Height);
            inspectorWinFormsControl.Bounds = new System.Drawing.Rectangle((int)RightSectionStartX, Rect.Top + TopMenuBarMargin, (int)RightSectionWidth, Rect.Height - TopMenuBarMargin);
        }

        public void Draw(GameTime gt, GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font, float elapsedSeconds)
        {
            sb.Begin();
            sb.Draw(boxTex, Rect, Config.EnableColorBlindMode ? Color.Black : new Color(0.2f, 0.2f, 0.2f));
            sb.End();
            //throw new Exception("TaeUndoMan");

            //throw new Exception("Make left/right edges of events line up to same vertical lines so the rounding doesnt make them 1 pixel off");
            //throw new Exception("Make dragging edges of scrollbar box do zoom");
            //throw new Exception("make ctrl+scroll zoom centered on mouse cursor pos");

            UpdateLayout();

            if (editScreenAnimList != null)
            {
                editScreenAnimList.Draw(gd, sb, boxTex, font);

                Rectangle curAnimInfoTextRect = new Rectangle(
                    (int)(MiddleSectionStartX),
                    Rect.Top + TopMenuBarMargin,
                    (int)(MiddleSectionWidth - ButtonEditCurrentAnimInfoWidth),
                    TopOfGraphAnimInfoMargin);

                sb.Begin();

                if (Config.EnableFancyScrollingStrings)
                {
                    SelectedTaeAnimInfoScrollingText.Draw(gd, sb, Matrix.Identity, curAnimInfoTextRect, font, elapsedSeconds);
                }
                else
                {
                    var curAnimInfoTextPos = curAnimInfoTextRect.Location.ToVector2();

                    sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + Vector2.One, Color.Black);
                    sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + (Vector2.One * 2), Color.Black);
                    sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos, Color.White);
                }

                //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + Vector2.One, Color.Black);
                //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + (Vector2.One * 2), Color.Black);
                //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos, Color.White);
                sb.End();
            }

            if (editScreenCurrentAnim != null)
            {
                editScreenCurrentAnim.Draw(gt, gd, sb, boxTex, font, elapsedSeconds);
            }
            //editScreenGraphInspector.Draw(gd, sb, boxTex, font);

            //var oldViewport = gd.Viewport;
            //gd.Viewport = new Viewport(Rect.X, Rect.Y, Rect.Width, TopMargin);
            //{
            //    sb.Begin();

            //    sb.DrawString(font, $"{TaeFileName}", new Vector2(4, 4) + Vector2.One, Color.Black);
            //    sb.DrawString(font, $"{TaeFileName}", new Vector2(4, 4), Color.White);

            //    sb.End();
            //}
            //gd.Viewport = oldViewport;
        }
    }
}
