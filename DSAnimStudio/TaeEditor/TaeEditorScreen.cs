using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DSAnimStudio.TaeEditor.TaeEditAnimEventGraph;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditorScreen
    {
        public LightShaderAdjuster ShaderAdjuster = null;

        private ContentManager DebugReloadContentManager = null;
        private void BuildDebugMenuBar()
        {
            //MenuBar.AddTopItem("[TEST: NEW MODEL VIEWER]", () =>
            //{
            //    //GameDataManager.InterrootPath = @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game";
            //    //GameDataManager.GameType = GameDataManager.GameTypes.DS3;

            //    //GameDataManager.LoadCharacter("c6200");

            //    LoadingTaskMan.DoLoadingTask("TEST MODEL VIEWER", "MODEL VIEWER TEST...", progress =>
            //    {
            //        //GameDataManager.Init(
            //        //    GameDataManager.GameTypes.DS1, 
            //        //    @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA");

            //        //GameDataManager.Init(
            //        //   GameDataManager.GameTypes.BB,
            //        //   @"E:\BloodborneRips\BB Fake Interroot\dvdroot_ps4");

            //        //var player = GameDataManager.LoadCharacter("c0000");

            //        GameDataManager.Init(
            //          GameDataManager.GameTypes.DS3,
            //          @"C:\Program Files (x86)\Steam\steamapps\common\DARK SOULS III\Game");

            //        var player = GameDataManager.LoadCharacter("c0000");

            //        player.CreateChrAsm();


            //        //player.ChrAsm.RightWeaponID = 306000; // DS1 Stone Greatsword


            //        //BB Foreign Set
            //        //player.ChrAsm.HeadID = 230000;
            //        //player.ChrAsm.BodyID = 231000;
            //        //player.ChrAsm.ArmsID = 232000;
            //        //player.ChrAsm.LegsID = 233000;

            //        //player.ChrAsm.RightWeaponID = 7100000; // BB Saw Spear

            //        //DS3 Firelink Set
            //        player.ChrAsm.HeadID = 21000000;
            //        player.ChrAsm.BodyID = 21001000;
            //        player.ChrAsm.ArmsID = 21002000;
            //        player.ChrAsm.LegsID = 21003000;
            //        player.ChrAsm.RightWeaponID = 12000000; // DS3 Whip
            //        player.ChrAsm.LeftWeaponID = 1455000;

            //        player.ChrAsm.UpdateModels();

            //        var test = Scene.Models;

            //        //foreach (var hitbox in player.ChrAsm.RightWeaponModel.DummyPolyMan.HitboxPrimitiveInfos)
            //        //{
            //        //    foreach (var prim in hitbox.Value.Primitives)
            //        //    {
            //        //        prim.EnableDraw = true;
            //        //    }
            //        //}

            //        //player.ChrAsm.RightWeaponModel.DummyPolyMan.ActivateAllHitboxes();
            //        player.DummyPolyMan.ActivateHitbox(ParamManager.AtkParam_Pc[4300000]);

            //        player.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = "a043_030000.hkx";
            //        player.AnimContainer.CurrentAnimationName = "a043_030000.hkx";

            //        GameWindowAsForm.Invoke(new Action(() =>
            //        {
            //            var cboiceDict = new Dictionary<string, Action>();
            //            foreach (var kvp in player.ChrAsm.RightWeaponModel.AnimContainer.Animations)
            //            {
            //                cboiceDict.Add(kvp.Key, () => player.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName = kvp.Key);


            //            }

            //            MenuBar.AddItem("[WPN TEST]", "WPN Anim", cboiceDict, () => player.ChrAsm.RightWeaponModel.AnimContainer.CurrentAnimationName);
            //        }));

            //        Console.WriteLine("fatcat");
            //    });

            //    //GameDataManager.InterrootPath = @"E:\BloodborneRips\BB Fake Interroot\dvdroot_ps4";
            //    //GameDataManager.GameType = GameDataManager.GameTypes.BB;

            //    //GameDataManager.LoadCharacter("c2510");

            //    DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = true;
                
            //});

            //MenuBar.AddTopItem("[Reload Shaders]", () =>
            //{
            //    if (DebugReloadContentManager != null)
            //    {
            //        DebugReloadContentManager.Unload();
            //        DebugReloadContentManager.Dispose();
            //    }

            //    DebugReloadContentManager = new ContentManager(Main.ContentServiceProvider);


            //    GFX.FlverShader.Effect.Dispose();
            //    GFX.FlverShader = null;
            //    GFX.FlverShader = new FlverShader(DebugReloadContentManager.Load<Effect>(GFX.FlverShader__Name));

            //    Main.MainFlverTonemapShader.Effect.Dispose();
            //    Main.MainFlverTonemapShader = null;
            //    Main.MainFlverTonemapShader  = new FlverTonemapShader(DebugReloadContentManager.Load<Effect>($@"Content\Shaders\FlverTonemapShader"));

            //    GFX.InitShaders();
            //});

            //var useTonemapperCheckbox = new System.Windows.Forms.ToolStripMenuItem("Use Tonemapper");
            //useTonemapperCheckbox.CheckOnClick = true;
            //useTonemapperCheckbox.Checked = GFX.UseTonemap;
            //useTonemapperCheckbox.Text = $"Use Tonemapper: {(useTonemapperCheckbox.Checked ? "YES" : "NO")}";
            //useTonemapperCheckbox.CheckedChanged += (x, y) =>
            //{
            //    GFX.UseTonemap = useTonemapperCheckbox.Checked;
            //    useTonemapperCheckbox.Text = $"Use Tonemapper: {(useTonemapperCheckbox.Checked ? "YES" : "NO")}";
            //};

            //WinFormsMenuStrip.Items.Add(useTonemapperCheckbox);gon

            //MenuBar.AddItem("Debug", "Scan All TAE Test", () =>
            //{
            //    var dir = @"C:\Program Files (x86)\Steam\steamapps\common\Dark Souls Prepare to Die Edition\DATA\chr";
            //    var anibndNames = System.IO.Directory.GetFiles(dir, "*.anibnd");
            //    foreach (var anibnd in anibndNames)
            //    {
            //        var bnd = BND3.Read(anibnd);
            //        foreach (var bndFile in bnd.Files)
            //        {
            //            if (TAE.Is(bndFile.Bytes))
            //            {
            //                var tae = TAE.Read(bndFile.Bytes);
            //                tae.ApplyTemplate(TAE.Template.ReadXMLFile("Res\\TAE.Template.DS1.xml"));
            //                foreach (var anim in tae.Animations)
            //                {
            //                    foreach (var ev in anim.Events)
            //                    {
            //                        if (ev.Type == 120)
            //                        {
            //                            //Console.WriteLine("Breakpoint hit");
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //});
        }

        enum DividerDragMode
        {
            None,
            LeftVertical,
            RightVertical,
            RightPaneHorizontal,
        }

        public enum ScreenMouseHoverKind
        {
            None,
            AnimList,
            EventGraph,
            Inspector,
            ModelViewer,
            DividerBetweenCenterAndLeftPane,
            DividerBetweenCenterAndRightPane,
            DividerRightPaneHorizontal,
            ShaderAdjuster
        }

        public DbgMenus.DbgMenuPadRepeater NextAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadDown, 0.4f, 0.05f);
        public DbgMenus.DbgMenuPadRepeater PrevAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadUp, 0.4f, 0.05f);

        public DbgMenus.DbgMenuPadRepeater NextFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadLeft, 0.3f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadRight, 0.3f, 0.016666667f);

        public static bool CurrentlyEditingSomethingInInspector;
        
        public class FindInfoKeep
        {
            public string SearchQuery;
            public bool MatchEntireString;
            public List<TaeFindResult> Results;
            public int HighlightedIndex;
        }

        public FindInfoKeep LastFindInfo = null;

        public TaePlaybackCursor PlaybackCursor => Graph?.PlaybackCursor;

        public Rectangle ModelViewerBounds;

        private const int RECENT_FILES_MAX = 32;

        private int TopMenuBarMargin = 32;

        private int TopOfGraphAnimInfoMargin = 24;
        private int ButtonEditCurrentAnimInfoWidth = 200;
        private System.Windows.Forms.Button ButtonEditCurrentAnimInfo;

        private int EditTaeHeaderButtonMargin = 32;
        private int EditTaeHeaderButtonHeight = 20;
        private System.Windows.Forms.Button ButtonEditCurrentTaeHeader;

        public bool CtrlHeld;
        public bool ShiftHeld;
        public bool AltHeld;

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
            "    Delete highlighted event.\n" +
            "Ctrl+X/Ctrl+C/Ctrl+V:\n" +
            "    CUT/COPY/PASTE\n" +
            "Ctrl+Z/Ctrl+Y:\n" +
            "    UNDO/REDO\n" +
            "F1 Key:\n" +
            "    Change type of highlighted event.\n" +
            "Space Bar:\n" +
            "    Play/Pause Anim.\n" +
            "Shift+Space Bar:\n" +
            "    Play Anim from beginning.\n";

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

        public Dictionary<TAE.Animation, TaeUndoMan> UndoManDictionary 
            = new Dictionary<TAE.Animation, TaeUndoMan>();

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

        public bool IsModified
        {
            get
            {
                try
                {
                    return (SelectedTae?.Animations.Any(a => a.GetIsModified()) ?? false) ||
            (FileContainer?.AllTAE.Any(t => t.GetIsModified()) ?? false);
                }
                catch
                {
                    return false;
                }
            }
        }
            

        public void UpdateIsModifiedStuff()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                MenuBar["File/Save"].Enabled = IsModified;
            }));
        }

        public TaeMenuBarBuilder MenuBar { get; private set; }

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
            GameWindowAsForm.Invoke(new Action(() =>
            {
                MenuBar["File/Recent Files"].DropDownItems.Clear();
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
                MenuBar["File/Recent Files"].DropDownItems.Add(toolStripFileRecentClear);
                MenuBar["File/Recent Files"].DropDownItems.Add(new System.Windows.Forms.ToolStripSeparator());
                foreach (var f in Config.RecentFilesList)
                {
                    var thisRecentFileEntry = new System.Windows.Forms.ToolStripMenuItem(f);
                    thisRecentFileEntry.Click += (s, e) =>
                    {
                        DirectOpenFile(f);
                    };
                    MenuBar["File/Recent Files"].DropDownItems.Add(thisRecentFileEntry);
                }
            }));
        }

        private void UndoMan_CanRedoMaybeChanged(object sender, EventArgs e)
        {
            MenuBar["Edit/Redo"].Enabled = UndoMan.CanRedo;
        }

        private void UndoMan_CanUndoMaybeChanged(object sender, EventArgs e)
        {
            MenuBar["Edit/Undo"].Enabled = UndoMan.CanUndo;
        }


        private TaeButtonRepeater UndoButton = new TaeButtonRepeater(0.4f, 0.05f);
        private TaeButtonRepeater RedoButton = new TaeButtonRepeater(0.4f, 0.05f);

        private float LeftSectionWidth = 150;
        private const float LeftSectionWidthMin = 150;
        private float DividerLeftVisibleStartX => Rect.Left + LeftSectionWidth;
        private float DividerLeftVisibleEndX => Rect.Left + LeftSectionWidth + DividerVisiblePad;

        private float RightSectionWidth = 600; //not weed
        private const float RightSectionWidthMin = 320;
        private float DividerRightVisibleStartX => Rect.Right - RightSectionWidth - DividerVisiblePad;
        private float DividerRightVisibleEndX => Rect.Right - RightSectionWidth;


        private float DividerRightCenterX => DividerRightVisibleStartX + ((DividerRightVisibleEndX - DividerRightVisibleStartX) / 2);
        private float DividerLeftCenterX => DividerLeftVisibleStartX + ((DividerLeftVisibleEndX - DividerLeftVisibleStartX) / 2);

        private float DividerRightGrabStartX => DividerRightCenterX - (DividerHitboxPad / 2);
        private float DividerRightGrabEndX => DividerRightCenterX + (DividerHitboxPad / 2);

        private float DividerLeftGrabStartX => DividerLeftCenterX - (DividerHitboxPad / 2);
        private float DividerLeftGrabEndX => DividerLeftCenterX + (DividerHitboxPad / 2);

        private float TopRightPaneHeight = 600;
        private const float TopRightPaneHeightMinNew = 128;
        private const float BottomRightPaneHeightMinNew = 256;

        private float DividerRightPaneHorizontalVisibleStartY => Rect.Top + TopRightPaneHeight + TopMenuBarMargin;
        private float DividerRightPaneHorizontalVisibleEndY => Rect.Top + TopRightPaneHeight + DividerVisiblePad + TopMenuBarMargin;
        private float DividerRightPaneHorizontalCenterY => DividerRightPaneHorizontalVisibleStartY + ((DividerRightPaneHorizontalVisibleEndY - DividerRightPaneHorizontalVisibleStartY) / 2);

        private float DividerRightPaneHorizontalGrabStartY => DividerRightPaneHorizontalCenterY - (DividerHitboxPad / 2);
        private float DividerRightPaneHorizontalGrabEndY => DividerRightPaneHorizontalCenterY + (DividerHitboxPad / 2);

        private float LeftSectionStartX => Rect.Left;
        private float MiddleSectionStartX => DividerLeftVisibleEndX;
        private float RightSectionStartX => Rect.Right - RightSectionWidth;

        private float MiddleSectionWidth => DividerRightVisibleStartX - DividerLeftVisibleEndX;
        private const float MiddleSectionWidthMin = 500;

        private float DividerVisiblePad = 3;
        private float DividerHitboxPad = 10;

        private DividerDragMode CurrentDividerDragMode = DividerDragMode.None;

        public ScreenMouseHoverKind MouseHoverKind = ScreenMouseHoverKind.None;
        private ScreenMouseHoverKind oldMouseHoverKind = ScreenMouseHoverKind.None;
        public ScreenMouseHoverKind WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;

        public TaeFileContainer FileContainer;

        public TAE SelectedTae { get; private set; }

        public TAE.Animation SelectedTaeAnim { get; private set; }
        private TaeScrollingString SelectedTaeAnimInfoScrollingText = new TaeScrollingString();

        public readonly System.Windows.Forms.Form GameWindowAsForm;

        public void UpdateInspectorToSelection()
        {
            GameWindowAsForm.Invoke(new Action(() =>
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
                        inspectorWinFormsControl.labelEventType.Text = "Multiple Events Selected\nSelect single event to edit.";
                        inspectorWinFormsControl.buttonChangeType.Enabled = false;
                        inspectorWinFormsControl.buttonChangeType.Visible = false;
                    }
                    else
                    {
                        inspectorWinFormsControl.labelEventType.Text = "No Event Selected\nSelect event to edit.";
                        inspectorWinFormsControl.buttonChangeType.Enabled = false;
                        inspectorWinFormsControl.buttonChangeType.Visible = false;
                    }
                }
                else
                {
                    inspectorWinFormsControl.labelEventType.Text =
                        (SelectedEventBox.MyEvent.TypeName != null ? SelectedEventBox.MyEvent.TypeName : $"Event Type {SelectedEventBox.MyEvent.Type}")
                        + $"\n{SelectedEventBox.MyEvent.StartTime:0.00} -> {SelectedEventBox.MyEvent.EndTime:0.00} (Duration: {(SelectedEventBox.MyEvent.EndTime - SelectedEventBox.MyEvent.StartTime):0.00})";
                        inspectorWinFormsControl.buttonChangeType.Enabled = true;
                        inspectorWinFormsControl.buttonChangeType.Visible = true;
                }
            }));
        }

        public void RefocusInspectorToPreventBeepWhenYouHitSpace()
        {
            inspectorWinFormsControl.Focus();
        }

        public TaeEditAnimEventBox HoveringOverEventBox = null;

        private TaeEditAnimEventBox _selectedEventBox = null;
        public TaeEditAnimEventBox SelectedEventBox
        {
            get => _selectedEventBox;
            set
            {
                //inspectorWinFormsControl.DumpDataGridValuesToEvent();

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
                if (inspectorWinFormsControl.SelectedEventBox != null)
                    inspectorWinFormsControl.SelectedEventBox.UpdateEventText();

                inspectorWinFormsControl.SelectedEventBox = _selectedEventBox;

                UpdateInspectorToSelection();
            }
        }

        public List<TaeEditAnimEventBox> MultiSelectedEventBoxes = new List<TaeEditAnimEventBox>();

        private TaeEditAnimList editScreenAnimList;
        public TaeEditAnimEventGraph Graph { get; private set; }
        //private TaeEditAnimEventGraphInspector editScreenGraphInspector;

        private Color ColorInspectorBG = Color.DarkGray;
        private TaeInspectorWinFormsControl inspectorWinFormsControl;

        public TaeInputHandler Input;

        private System.Windows.Forms.MenuStrip WinFormsMenuStrip;

        public string FileContainerName = "";

        public bool IsReadOnlyFileMode = false;

        public TaeConfigFile Config = new TaeConfigFile();

        private static string ConfigFilePath = null;

        private static void CheckConfigFilePath()
        {
            if (ConfigFilePath == null)
            {
                var currentAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var currentAssemblyDir = System.IO.Path.GetDirectoryName(currentAssemblyPath);
                ConfigFilePath = System.IO.Path.Combine(currentAssemblyDir, "DSAnimStudio_Config.json");
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

            Config.AfterLoading();
        }

        public void SaveConfig()
        {
            if (Graph != null)
            {
                // I'm sorry; this is pecularily placed.
                Graph.ViewportInteractor.SaveChrAsm();
            }

            Config.BeforeSaving();

            CheckConfigFilePath();

            var jsonText = Newtonsoft.Json.JsonConvert
                .SerializeObject(Config,
                Newtonsoft.Json.Formatting.Indented);

            System.IO.File.WriteAllText(ConfigFilePath, jsonText);
        }

        public bool? LoadCurrentFile()
        {
            // Even if it fails to load, just always push it to the recent files list
            PushNewRecentFile(FileContainerName);

            //string templateName = BrowseForXMLTemplate();

            //if (templateName == null)
            //{
            //    return false;
            //}

            if (System.IO.File.Exists(FileContainerName))
            {
                FileContainer = new TaeFileContainer();

                try
                {
                    FileContainer.LoadFromPath(FileContainerName);
                }
                catch (System.DllNotFoundException)
                {
                    System.Windows.Forms.MessageBox.Show("Cannot open Sekiro files unless you " +
                        "copy the `oo2core_6_win64.dll` file from the Sekiro folder into the " +
                        "same folder as this editor's EXE.", "Additional DLL Required", 
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    return false;
                }
                

                if (!FileContainer.AllTAE.Any())
                {
                    return true;
                }

                LoadTaeFileContainer(FileContainer);

                GameWindowAsForm.Invoke(new Action(() =>
                {
                    MenuBar["File/Save As..."].Enabled = !IsReadOnlyFileMode;
                    MenuBar["File/Force Ingame Character Reload Now (DS3 Only)"].Enabled = !IsReadOnlyFileMode;
                }));

                //if (templateName != null)
                //{
                //    LoadTAETemplate(templateName);
                //}

                return true;
            }
            else
            {
                return null;
            }
        }

        private void CheckAutoLoadXMLTemplate()
        {
            var objCheck = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName)).ToLower().StartsWith("o");

            var xmlPath = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
                $@"Res\TAE.Template.{(FileContainer.IsBloodborne ? "BB" : SelectedTae.Format.ToString())}{(objCheck ? ".OBJ" : "")}.xml");

            if (System.IO.File.Exists(xmlPath))
                LoadTAETemplate(xmlPath);
        }

        public void SaveCurrentFile(Action afterSaveAction = null, string saveMessage = "Saving ANIBND...")
        {
            if (IsReadOnlyFileMode)
            {
                System.Windows.Forms.MessageBox.Show("Read-only mode is" +
                    " active so nothing was saved. To open a file in re-saveable mode," +
                    " make sure the Read-Only checkbox is unchecked in the open" +
                    " file dialog.", "Read-Only Mode Active",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Stop);
                return;
            }

            if (System.IO.File.Exists(FileContainerName) && 
                !System.IO.File.Exists(FileContainerName + ".taedxbak"))
            {
                System.IO.File.Copy(FileContainerName, FileContainerName + ".taedxbak");
                System.Windows.Forms.MessageBox.Show(
                    "A backup was not found and was created:\n" + FileContainerName + ".taedxbak",
                    "Backup Created", System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Information);
            }

            LoadingTaskMan.DoLoadingTask("SaveFile", saveMessage, progress =>
            {
                FileContainer.SaveToPath(FileContainerName, progress);

                if (Config.LiveRefreshOnSave)
                {
                    LiveRefresh();
                }

                progress.Report(1.0);

                afterSaveAction?.Invoke();
            });

            
        }

        private void LoadTaeFileContainer(TaeFileContainer fileContainer)
        {
            TaeExtensionMethods.ClearMemes();
            FileContainer = fileContainer;
            SelectedTae = FileContainer.AllTAE.First();
            GameWindowAsForm.Invoke(new Action(() =>
            {
                ButtonEditCurrentTaeHeader.Enabled = false;
            }));
            SelectedTaeAnim = SelectedTae.Animations[0];
            editScreenAnimList = new TaeEditAnimList(this);
            Graph = new TaeEditAnimEventGraph(this);
            //if (FileContainer.ContainerType != TaeFileContainer.TaeFileContainerType.TAE)
            //{
            //    TaeInterop.OnLoadANIBND(MenuBar, progress);
            //}
            CheckAutoLoadXMLTemplate();
            SelectNewAnimRef(SelectedTae, SelectedTae.Animations[0]);
            GameWindowAsForm.Invoke(new Action(() =>
            {
                ButtonEditCurrentAnimInfo.Enabled = true;
                ButtonEditCurrentAnimInfo.Visible = true;
                //MenuBar["Edit/Find First Event of Type..."].Enabled = true;
                MenuBar["Edit/Find Value..."].Enabled = true;
                MenuBar["Edit/Go To Animation ID..."].Enabled = true;
                MenuBar["Edit/Collapse All TAE Sections"].Enabled = true;
                MenuBar["Edit/Expand All TAE Sections"].Enabled = true;
                MenuBar["Edit/Go To Animation ID..."].Enabled = true;
                LastFindInfo = null;
            }));
        }

        public void RecreateAnimList()
        {
            Vector2 oldScroll = editScreenAnimList.ScrollViewer.Scroll;
            var sectionsCollapsed = editScreenAnimList
                .AnimTaeSections
                .ToDictionary(x => x.SectionName, x => x.Collapsed);

            editScreenAnimList = new TaeEditAnimList(this);

            foreach (var section in editScreenAnimList.AnimTaeSections)
            {
                if (sectionsCollapsed.ContainsKey(section.SectionName))
                    section.Collapsed = sectionsCollapsed[section.SectionName];
            }
            
            editScreenAnimList.ScrollViewer.Scroll = oldScroll;
            
        }

        public void AddNewAnimation()
        {
            var newAnimRef = new TAE.Animation(
                SelectedTaeAnim.ID, SelectedTaeAnim.AnimFileReference, 
                SelectedTaeAnim.Unknown1, SelectedTaeAnim.Unknown2, SelectedTaeAnim.AnimFileName);

            var index = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
            SelectedTae.Animations.Insert(index + 1, newAnimRef);

            RecreateAnimList();

            SelectNewAnimRef(SelectedTae, newAnimRef);
        }

        public void LoadTAETemplate(string xmlFile)
        {
            try
            {
                foreach (var tae in FileContainer.AllTAE)
                {
                    tae.ApplyTemplate(TAE.Template.ReadXMLFile(xmlFile));
                }

                foreach (var box in Graph.EventBoxes)
                {
                    box.UpdateEventText();
                }

                var wasSelecting = SelectedEventBox;
                SelectedEventBox = null;
                SelectedEventBox = wasSelecting;

                if (SelectedTae.BankTemplate != null)
                {
                    GameWindowAsForm.Invoke(new Action(() =>
                    {
                        inspectorWinFormsControl.buttonChangeType.Enabled = true;
                        inspectorWinFormsControl.buttonChangeType.Visible = true;
                    }));
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Failed to apply TAE template:\n\n{ex}",
                    "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public TaeEditorScreen(System.Windows.Forms.Form gameWindowAsForm)
        {
            LoadConfig();

            gameWindowAsForm.FormClosing += GameWindowAsForm_FormClosing;

            GameWindowAsForm = gameWindowAsForm;

            GameWindowAsForm.MinimumSize = new System.Drawing.Size(1280  - 64, 720 - 64);

            Input = new TaeInputHandler();

            //editScreenAnimList = new TaeEditAnimList(this);
            //editScreenCurrentAnim = new TaeEditAnimEventGraph(this);
            //editScreenGraphInspector = new TaeEditAnimEventGraphInspector(this);

            inspectorWinFormsControl = new TaeInspectorWinFormsControl();

            //// This might change in the future if I actually add text description attributes to some things.
            //inspectorWinFormsControl.propertyGrid.HelpVisible = false;

            //inspectorWinFormsControl.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            //inspectorWinFormsControl.propertyGrid.ToolbarVisible = false;

            ////inspectorPropertyGrid.ViewBackColor = System.Drawing.Color.FromArgb(
            ////    ColorInspectorBG.A, ColorInspectorBG.R, ColorInspectorBG.G, ColorInspectorBG.B);

            //inspectorWinFormsControl.propertyGrid.LargeButtons = true;

            //inspectorWinFormsControl.propertyGrid.CanShowVisualStyleGlyphs = false;

            inspectorWinFormsControl.buttonChangeType.Enabled = false;
            inspectorWinFormsControl.buttonChangeType.Visible = false;

            inspectorWinFormsControl.buttonChangeType.Click += ButtonChangeType_Click;

            inspectorWinFormsControl.TaeEventValueChanged += InspectorWinFormsControl_TaeEventValueChanged;

            GameWindowAsForm.Controls.Add(inspectorWinFormsControl);

            WinFormsMenuStrip = new System.Windows.Forms.MenuStrip();

            WinFormsMenuStrip.BackColor = inspectorWinFormsControl.BackColor;
            WinFormsMenuStrip.ForeColor = inspectorWinFormsControl.ForeColor;

            MenuBar = new TaeMenuBarBuilder(WinFormsMenuStrip);

            //////////
            // File //
            //////////

            MenuBar.AddItem("File", "Open...", () => File_Open());
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Load Custom TAE Data Template XML...", () =>
            {
                var selectedTemplate = BrowseForXMLTemplate();
                if (selectedTemplate != null)
                    LoadTAETemplate(selectedTemplate);
            });
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Recent Files");
            CreateRecentFilesList();
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Save", () => SaveCurrentFile(), startDisabled: true);
            MenuBar.AddItem("File", "Save As...", () => File_SaveAs(), startDisabled: true);
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Force Ingame Character Reload Now (DS3 Only)", () => LiveRefresh(), startDisabled: true);
            MenuBar.AddItem("File", "Force Ingame Character Reload When Saving (DS3 Only)", () => Config.LiveRefreshOnSave, b => Config.LiveRefreshOnSave = b);
            MenuBar.AddSeparator("File");
            MenuBar.AddItem("File", "Exit", () => GameWindowAsForm.Close());

            //////////////////
            // NPC Settings //
            //////////////////

            MenuBar.AddTopItem("NPC Settings");
            MenuBar["NPC Settings"].Enabled = false;
            MenuBar["NPC Settings"].Visible = false;
            MenuBar["NPC Settings"].Font = 
                new System.Drawing.Font(MenuBar["NPC Settings"].Font, 
                System.Drawing.FontStyle.Bold);
            MenuBar["NPC Settings"].ForeColor = System.Drawing.Color.Cyan;

            MenuBar.AddTopItem("Player Settings");
            MenuBar["Player Settings"].Enabled = false;
            MenuBar["Player Settings"].Visible = false;
            MenuBar["Player Settings"].Font = 
                new System.Drawing.Font(MenuBar["Player Settings"].Font, 
                System.Drawing.FontStyle.Bold);
            MenuBar["Player Settings"].ForeColor = System.Drawing.Color.Cyan;

            MenuBar.AddItem("Player Settings", "Show Equip Change Menu", () => Graph?.ViewportInteractor?.BringUpEquipForm());

            MenuBar.AddTopItem("Object Settings");
            MenuBar["Object Settings"].Enabled = false;
            MenuBar["Object Settings"].Visible = false;
            MenuBar["Object Settings"].Font = 
                new System.Drawing.Font(MenuBar["Object Settings"].Font, 
                System.Drawing.FontStyle.Bold);
            MenuBar["Object Settings"].ForeColor = System.Drawing.Color.Cyan;

            MenuBar.AddTopItem("Animated Equipment Settings");
            MenuBar["Animated Equipment Settings"].Enabled = false;
            MenuBar["Animated Equipment Settings"].Visible = false;
            MenuBar["Animated Equipment Settings"].Font =
                new System.Drawing.Font(MenuBar["Animated Equipment Settings"].Font,
                System.Drawing.FontStyle.Bold);
            MenuBar["Animated Equipment Settings"].ForeColor = System.Drawing.Color.Cyan;

            MenuBar.AddTopItem("Cutscene Settings");
            MenuBar["Cutscene Settings"].Enabled = false;
            MenuBar["Cutscene Settings"].Visible = false;
            MenuBar["Cutscene Settings"].Font =
                new System.Drawing.Font(MenuBar["Cutscene Settings"].Font,
                System.Drawing.FontStyle.Bold);
            MenuBar["Cutscene Settings"].ForeColor = System.Drawing.Color.Cyan;

            //////////
            // Edit //
            //////////

            MenuBar.AddItem("Edit", "Undo|Ctrl+Z", () => UndoMan.Undo(), startDisabled: true);
            MenuBar.AddItem("Edit", "Redo|Ctrl+Y", () => UndoMan.Redo(), startDisabled: true);
            MenuBar.AddSeparator("Edit");
            MenuBar.AddItem("Edit", "Collapse All TAE Sections", () =>
            {
                foreach (var kvp in editScreenAnimList.AnimTaeSections)
                {
                    kvp.Collapsed = true;
                }
            }, startDisabled: true);
            MenuBar.AddItem("Edit", "Expand All TAE Sections", () =>
            {
                foreach (var kvp in editScreenAnimList.AnimTaeSections)
                {
                    kvp.Collapsed = false;
                }
            }, startDisabled: true);
            MenuBar.AddSeparator("Edit");
            //MenuBar.AddItem("Edit", "Find First Event of Type...|Ctrl+F", () => ShowDialogFind(), startDisabled: true);
            MenuBar.AddItem("Edit", "Find Value...|Ctrl+F", () => ShowDialogFind(), startDisabled: true);
            MenuBar.AddItem("Edit", "Go To Animation ID...|Ctrl+G", () => ShowDialogGoto(), startDisabled: true);

            /////////////////
            // Event Graph //
            /////////////////

            MenuBar.AddItem("Event Graph", "Snap To 30 Hz Increments", () => Config.EnableSnapTo30FPSIncrements, b => Config.EnableSnapTo30FPSIncrements = b);
            //MenuBar.AddItem("Event Graph", "High Contrast Mode", () => Config.EnableColorBlindMode, b => Config.EnableColorBlindMode = b);
            MenuBar.AddSeparator("Event Graph");
            MenuBar.AddItem("Event Graph", "Use Fancy Text Scrolling", () => Config.EnableFancyScrollingStrings, b => Config.EnableFancyScrollingStrings = b);
            MenuBar.AddItem("Event Graph", "Fancy Text Scroll Speed", new Dictionary<string, Action>
                {
                    { "Extremely Slow (4 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 4 },
                    { "Very Slow (8 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 8 },
                    { "Slow (16 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 16 },
                    { "Medium Speed (32 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 32 },
                    { "Fast (64 px/s)",  () => Config.FancyScrollingStringsScrollSpeed = 64 },
                    { "Very Fast (128 px/s)",  () => Config.FancyScrollingStringsScrollSpeed = 128 },
                    {  "Extremely Fast (256 px/s)", () => Config.FancyScrollingStringsScrollSpeed = 256 },
                },
                defaultChoice: "Fast (64 px/s)");
            MenuBar.AddSeparator("Event Graph");
            MenuBar.AddItem("Event Graph", "Start with all TAE sections collapsed", () => Config.AutoCollapseAllTaeSections, b => Config.AutoCollapseAllTaeSections = b);
            MenuBar.AddSeparator("Event Graph");
            MenuBar.AddItem("Event Graph", "Auto-scroll During Anim Playback", () => Config.AutoScrollDuringAnimPlayback, b => Config.AutoScrollDuringAnimPlayback = b);

            MenuBar.AddSeparator("Event Graph");

            //MenuBar.AddItem("Event Graph", "Show SFX Spawn Events With Cyan Markers", () => TaeInterop.ShowSFXSpawnWithCyanMarkers, b => TaeInterop.ShowSFXSpawnWithCyanMarkers = b);
            //MenuBar.AddItem("Event Graph", "Beep Upon Hitting Sound Events", () => TaeInterop.PlaySoundEffectOnSoundEvents, b => TaeInterop.PlaySoundEffectOnSoundEvents = b);
            //MenuBar.AddItem("Event Graph", "Beep Upon Hitting Highlighted Event(s)", () => TaeInterop.PlaySoundEffectOnHighlightedEvents, b => TaeInterop.PlaySoundEffectOnHighlightedEvents = b);
            //MenuBar.AddItem("Event Graph", "Sustain Sound Effect Loop For Duration Of Highlighted Event(s)", () => TaeInterop.PlaySoundEffectOnHighlightedEvents_Loop, b => TaeInterop.PlaySoundEffectOnHighlightedEvents_Loop = b);

            ///////////
            // Scene //
            ///////////

            MenuBar.AddItem("Scene", "Render Meshes", () => !GFX.HideFLVERs,
                    b => GFX.HideFLVERs = !b);

            MenuBar.AddItem("Scene", "Render HKX Skeleton (Yellow)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone],
                    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.HkxBone] = b);

            MenuBar.AddItem("Scene", "Render FLVER Skeleton (Purple)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] = b);

            MenuBar.AddItem("Scene", "Render DummyPoly (Red/Green/Blue)", () => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                    b => DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] = b);

            ///////////////
            // Animation //
            ///////////////

            MenuBar.AddItem("Animation", "Set Playback Speed...", () =>
            {
                PauseUpdate = true;
                var speed = KeyboardInput.Show("Set Playback Speed", "Set animation playback speed.", "1.00");
                if (float.TryParse(speed.Result, out float newSpeed))
                {
                    PlaybackCursor.PlaybackSpeed = newSpeed;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Not a valid number.");
                }
                PauseUpdate = false;
            });

            //////////////
            // Viewport //
            //////////////

            MenuBar.AddItem("Viewport", "Vsync", () => GFX.Display.Vsync, b =>
            {
                GFX.Display.Vsync = b;
                GFX.Display.Width = GFX.Device.Viewport.Width;
                GFX.Display.Height = GFX.Device.Viewport.Height;
                GFX.Display.Fullscreen = false;
                GFX.Display.Apply();
            });

            MenuBar.AddItem("Viewport", "Slow Light Spin (overrides below option)", () => GFX.FlverAutoRotateLight, b => GFX.FlverAutoRotateLight = b);
            MenuBar.AddItem("Viewport", "Light Follows Camera", () => GFX.FlverLightFollowsCamera, b => GFX.FlverLightFollowsCamera = b);
            MenuBar.AddItem("Viewport", "Shader Workflow Type", new Dictionary<string, Action>
            {
                { "Legacy (Epic 2005 Style)", () => GFX.FlverShaderWorkflowType = FlverShader.FSWorkflowType.Ass },
                { "Modern (Gloss Channel)", () => GFX.FlverShaderWorkflowType = FlverShader.FSWorkflowType.Gloss },
                { "Modern (Roughness Channel)", () => GFX.FlverShaderWorkflowType = FlverShader.FSWorkflowType.Roughness },
                { "Modern (Metalness Channel)", () => GFX.FlverShaderWorkflowType = FlverShader.FSWorkflowType.Metalness },
            }, () =>
            {
                switch (GFX.FlverShaderWorkflowType)
                {
                    case FlverShader.FSWorkflowType.Ass: return "Legacy (Epic 2005 Style)";
                    case FlverShader.FSWorkflowType.Gloss: return "Modern (Gloss Channel)";
                    case FlverShader.FSWorkflowType.Roughness: return "Modern (Roughness Channel)";
                    case FlverShader.FSWorkflowType.Metalness: return "Modern (Metalness Channel)";
                    default: return "";
                }
            });

            MenuBar.AddItem("Viewport", $@"Reload FLVER Shader (.\Content\Shaders\FlverShader.xnb)", () =>
            {
                if (DebugReloadContentManager != null)
                {
                    DebugReloadContentManager.Unload();
                    DebugReloadContentManager.Dispose();
                }

                DebugReloadContentManager = new ContentManager(Main.ContentServiceProvider);

                GFX.FlverShader.Effect.Dispose();
                GFX.FlverShader = null;
                GFX.FlverShader = new FlverShader(DebugReloadContentManager.Load<Effect>(GFX.FlverShader__Name));

                GFX.InitShaders();
            });

            if (Environment.Cubemaps.ContainsKey(Config.LastCubemapUsed))
            {
                Environment.CurrentCubemapName = Config.LastCubemapUsed;
            }

            MenuBar.AddItem("Viewport", "Cubemap", () =>
            {
                var result = new Dictionary<string, Action>();
                foreach (var kvp in Environment.Cubemaps)
                {
                    result.Add(kvp.Key, () =>
                    {
                        Environment.CurrentCubemapName = kvp.Key;
                        Config.LastCubemapUsed = kvp.Key;
                    });
                }
                return result;
            },
            () => Environment.CurrentCubemapName);

            MenuBar.AddItem("Viewport", "Resolution Multiplier", new Dictionary<string, Action>
            {
                { "1x", () => GFX.SSAA = 1 },
                { "2x", () => GFX.SSAA = 2 },
                { "3x", () => GFX.SSAA = 3 },
                { "4x", () => GFX.SSAA = 4 },
                { "5x", () => GFX.SSAA = 5 },
                { "6x", () => GFX.SSAA = 6 },
                { "7x", () => GFX.SSAA = 7 },
                { "8x", () => GFX.SSAA = 8 },
                { "16x", () => GFX.SSAA = 16 },
                { "32x", () => GFX.SSAA = 32 },
            }, () => $"{GFX.SSAA}x");


            MenuBar.AddItem("Help", "Basic Controls", () => System.Windows.Forms.MessageBox.Show(HELP_TEXT, "TAE Editor Help",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information));

            BuildDebugMenuBar();

            WinFormsMenuStrip.MenuActivate += WinFormsMenuStrip_MenuActivate;
            WinFormsMenuStrip.MenuDeactivate += WinFormsMenuStrip_MenuDeactivate;

            GameWindowAsForm.Controls.Add(WinFormsMenuStrip);

            ButtonEditCurrentAnimInfo = new System.Windows.Forms.Button();
            ButtonEditCurrentAnimInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ButtonEditCurrentAnimInfo.TabStop = false;
            ButtonEditCurrentAnimInfo.Text = "Edit Anim Info...";
            ButtonEditCurrentAnimInfo.Click += ButtonEditCurrentAnimInfo_Click;
            ButtonEditCurrentAnimInfo.BackColor = inspectorWinFormsControl.BackColor;
            ButtonEditCurrentAnimInfo.ForeColor = inspectorWinFormsControl.ForeColor;
            ButtonEditCurrentAnimInfo.Enabled = false;
            ButtonEditCurrentAnimInfo.Visible = false;

            GameWindowAsForm.Controls.Add(ButtonEditCurrentAnimInfo);

            ButtonEditCurrentTaeHeader = new System.Windows.Forms.Button();
            ButtonEditCurrentTaeHeader.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            ButtonEditCurrentTaeHeader.TabStop = false;
            ButtonEditCurrentTaeHeader.Text = "Edit TAE Header...";
            ButtonEditCurrentTaeHeader.Click += ButtonEditCurrentTaeHeader_Click;
            ButtonEditCurrentTaeHeader.BackColor = inspectorWinFormsControl.BackColor;
            ButtonEditCurrentTaeHeader.ForeColor = inspectorWinFormsControl.ForeColor;
            ButtonEditCurrentTaeHeader.Enabled = false;
            ButtonEditCurrentTaeHeader.Visible = false;

            GameWindowAsForm.Controls.Add(ButtonEditCurrentTaeHeader);

            ShaderAdjuster = new LightShaderAdjuster();

            //ShaderAdjuster.BackColor = inspectorWinFormsControl.BackColor;
            //ShaderAdjuster.ForeColor = inspectorWinFormsControl.ForeColor;

            //GameWindowAsForm.BackColor = System.Drawing.Color.Fuchsia;
            GameWindowAsForm.AllowTransparency = false;
            //GameWindowAsForm.TransparencyKey = System.Drawing.Color.Fuchsia;

            GameWindowAsForm.Controls.Add(ShaderAdjuster);

            UpdateLayout();
        }

        private void InspectorWinFormsControl_TaeEventValueChanged(object sender, EventArgs e)
        {
            //var gridReference = (System.Windows.Forms.DataGridView)sender;
            var boxReference = SelectedEventBox;
            //var newValReference = e.ChangedItem.Value;
            //var oldValReference = e.OldValue;

            //UndoMan.NewAction(doAction: () =>
            //{
            //    e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, newValReference);

            //    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
            //    IsModified = true;

            //    gridReference.Refresh();
            //},
            //undoAction: () =>
            //{
            //    e.ChangedItem.PropertyDescriptor.SetValue(boxReference.MyEvent, oldValReference);

            //    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
            //    IsModified = true;

            //    gridReference.Refresh();
            //});

            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);

            //gridReference.Refresh();
        }

        private void LiveRefresh()
        {
            var chrNameBase = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName)).ToLower();

            if (chrNameBase.EndsWith("c"))
            {
                DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                    DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Chr, chrNameBase);
            }
            else if (chrNameBase.StartsWith("o"))
            {
                DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                    DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Object, chrNameBase);
            }

            
            //if (FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_PTDE || FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_DS1R)
            //{
            //    var chrName = Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName);

            //    //In case of .anibnd.dcx
            //    chrName = Utils.GetFileNameWithoutDirectoryOrExtension(chrName);

            //    if (chrName.ToLower().StartsWith("c") && chrName.Length == 5)
            //    {
            //        if (FileContainer.ReloadType == TaeFileContainer.TaeFileContainerReloadType.CHR_PTDE)
            //        {
            //            TaeLiveRefresh.ForceReloadCHR_PTDE(chrName);
            //        }
            //        else
            //        {
            //            TaeLiveRefresh.ForceReloadCHR_DS1R(chrName);
            //        }
            //    }
            //}
        }

        private void ButtonEditCurrentTaeHeader_Click(object sender, EventArgs e)
        {
            inspectorWinFormsControl.Focus();
            ShowDialogEditTaeHeader();
        }

        public void ShowDialogEditTaeHeader()
        {
            PauseUpdate = true;
            var editForm = new TaeEditTaeHeaderForm(SelectedTae);
            editForm.Owner = GameWindowAsForm;
            editForm.ShowDialog();

            if (editForm.WereThingsChanged)
            {
                SelectedTae.SetIsModified(true);

                UpdateSelectedTaeAnimInfoText();
            }

            PauseUpdate = false;
        }

        public bool GotoAnimID(int id, bool scrollOnCenter)
        {
            foreach (var s in editScreenAnimList.AnimTaeSections)
            {
                var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == id);
                if (matchedAnims.Any())
                {
                    var anim = matchedAnims.First().Value.Ref;
                    SelectNewAnimRef(s.Tae, anim, scrollOnCenter);
                    return true;
                }
            }
            return false;
        }

        public void ShowDialogEditCurrentAnimInfo()
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
                        SelectNewAnimRef(SelectedTae, SelectedTae.Animations[0]);

                    SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                }
            }
            else
            {
                bool needsAnimReload = false;
                if (editForm.WasAnimIDChanged)
                {
                    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                    SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                    RecreateAnimList();
                    UpdateSelectedTaeAnimInfoText();
                    needsAnimReload = true;
                }

                if (editForm.WereThingsChanged)
                {
                    SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                    SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                    UpdateSelectedTaeAnimInfoText();
                    needsAnimReload = true;
                }

                //if (needsAnimReload)
                //    TaeInterop.OnAnimationSelected(FileContainer.AllTAEDict, SelectedTae, SelectedTaeAnim);
            }

            PauseUpdate = false;
        }

        private void ButtonEditCurrentAnimInfo_Click(object sender, EventArgs e)
        {
            inspectorWinFormsControl.Focus();
            ShowDialogEditCurrentAnimInfo();
        }

        private void GameWindowAsForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            SaveConfig();

            var unsavedChanges = IsModified && !IsReadOnlyFileMode;

            if (!unsavedChanges && FileContainer != null)
            {
                if (FileContainer.IsModified)
                {
                    unsavedChanges = true;
                }
                else
                {
                    foreach (var tae in FileContainer.AllTAE)
                    {
                        foreach (var anim in tae.Animations)
                        {
                            if (anim.GetIsModified() && !IsReadOnlyFileMode)
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
                e.Cancel = true;

                var confirmDlg = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(FileContainerName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"closing?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (confirmDlg == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile(afterSaveAction: () =>
                    {
                        Main.REQUEST_EXIT = true;
                    },
                    saveMessage: "Saving ANIBND and then exiting...");
                }
                else if (confirmDlg == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = false;
                }
            }
            else
            {
                e.Cancel = false;
            }

            
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
            
            void DoActualFileOpen()
            {
                LoadingTaskMan.DoLoadingTask("DirectOpenFile", "Opening ANIBND and associated model(s)...", progress =>
                {

                    FileContainerName = fileName;
                    var loadFileResult = LoadCurrentFile();
                    if (loadFileResult == false)
                    {
                        FileContainerName = "";
                        return;
                    }
                    else if (loadFileResult == null)
                    {
                        System.Windows.Forms.ToolStripMenuItem matchingRecentFileItem = null;

                        foreach (var x in MenuBar["File/Recent Files"].DropDownItems)
                        {
                            if (x is System.Windows.Forms.ToolStripMenuItem item)
                            {
                                if (item.Text == fileName)
                                {
                                    matchingRecentFileItem = item;
                                }
                            }
                        }

                        if (matchingRecentFileItem == null)
                        {
                            System.Windows.Forms.MessageBox.Show(
                                $"File '{fileName}' no longer exists.",
                                "File Does Not Exist",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Warning);
                        }
                        else
                        {
                            var ask = System.Windows.Forms.MessageBox.Show(
                                $"File '{fileName}' no longer exists. Would you like to " +
                                $"remove it from the recent files list?",
                                "File Does Not Exist",
                                System.Windows.Forms.MessageBoxButtons.YesNo,
                                System.Windows.Forms.MessageBoxIcon.Warning)
                                    == System.Windows.Forms.DialogResult.Yes;

                            if (ask)
                            {
                                if (MenuBar["File/Recent Files"].DropDownItems.Contains(matchingRecentFileItem))
                                    MenuBar["File/Recent Files"].DropDownItems.Remove(matchingRecentFileItem);

                                if (Config.RecentFilesList.Contains(fileName))
                                    Config.RecentFilesList.Remove(fileName);
                            }
                        }

                        FileContainerName = "";
                        return;
                    }

                    if (!FileContainer.AllTAE.Any())
                    {
                        FileContainerName = "";
                        System.Windows.Forms.MessageBox.Show(
                            "Selected file had no TAE files within. " +
                            "Cancelling load operation.", "Invalid File",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                    else if (loadFileResult == null)
                    {
                        FileContainerName = "";
                        System.Windows.Forms.MessageBox.Show(
                            "File did not exist.", "File Does Not Exist",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                }, disableProgressBarByDefault: true);
            }

            if (FileContainer != null && !IsReadOnlyFileMode && (IsModified || FileContainer.AllTAE.Any(x => x.Animations.Any(a => a.GetIsModified()))))
            {
                var yesNoCancel = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(FileContainerName)}\" has " +
                    $"unsaved changes. Would you like to save these changes before " +
                    $"loading a new file?", "Save Unsaved Changes?",
                    System.Windows.Forms.MessageBoxButtons.YesNoCancel,
                    System.Windows.Forms.MessageBoxIcon.None);

                if (yesNoCancel == System.Windows.Forms.DialogResult.Yes)
                {
                    SaveCurrentFile(afterSaveAction: () =>
                    {
                        DoActualFileOpen();
                    },
                    saveMessage: "Saving ANIBND then loading new one...");
                    return;
                }
                else if (yesNoCancel == System.Windows.Forms.DialogResult.Cancel)
                {
                    return;
                }
                //If they chose no, continue as normal.
            }

            DoActualFileOpen();
                
            
        }

        public void File_Open()
        {
            if (FileContainer != null && !IsReadOnlyFileMode && FileContainer.AllTAE.Any(x => x.Animations.Any(a => a.GetIsModified())))
            {
                var yesNoCancel = System.Windows.Forms.MessageBox.Show(
                    $"File \"{System.IO.Path.GetFileName(FileContainerName)}\" has " +
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
                Filter = TaeFileContainer.DefaultSaveFilter,
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
                //ShowReadOnly = true,
            };

            if (System.IO.File.Exists(FileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(FileContainerName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsReadOnlyFileMode = browseDlg.ReadOnlyChecked;
                FileContainerName = browseDlg.FileName;

                LoadingTaskMan.DoLoadingTask("File_Open", "Loading ANIBND and associated model(s)...", progress =>
                {
                    var loadFileResult = LoadCurrentFile();
                    if (loadFileResult == false || !FileContainer.AllTAE.Any())
                    {
                        FileContainerName = "";
                        System.Windows.Forms.MessageBox.Show(
                            "Selected file had no TAE files within. " +
                            "Cancelling load operation.", "Invalid File",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                    else if (loadFileResult == null)
                    {
                        FileContainerName = "";
                        System.Windows.Forms.MessageBox.Show(
                            "Selected file did not exist (how did you " +
                            "get this message to appear, anyways?).", "File Does Not Exist",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                }, disableProgressBarByDefault: true);

               
            }
        }

        private string BrowseForXMLTemplateLoop()
        {
            string selectedTemplate = null;
            do
            {
                var templateOption = System.Windows.Forms.MessageBox.Show("Select an XML template file to open.", "Open Template", System.Windows.Forms.MessageBoxButtons.OKCancel, System.Windows.Forms.MessageBoxIcon.Information);
                if (templateOption == System.Windows.Forms.DialogResult.Cancel)
                {
                    return null;
                }
                else
                {
                    selectedTemplate = BrowseForXMLTemplate();
                }
            }
            while (selectedTemplate == null);

            return selectedTemplate;
        }

        private string BrowseForXMLTemplate()
        {
            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Title = "Open TAE Template XML File",
                Filter = "TAE Templates (*.XML)|*.XML",
                ValidateNames = true,
                CheckFileExists = true,
                CheckPathExists = true,
            };

            browseDlg.InitialDirectory = System.IO.Path.Combine(new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName, "Res");

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsReadOnlyFileMode = browseDlg.ReadOnlyChecked;
                return browseDlg.FileName;
            }

            return null;
        }

        private void File_SaveAs()
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = FileContainer?.GetResaveFilter()
                           ?? TaeFileContainer.DefaultSaveFilter,
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(FileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(FileContainerName);
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileContainerName = browseDlg.FileName;
                SaveCurrentFile();
            }
        }

        private void ChangeTypeOfSelectedEvent()
        {
            if (SelectedEventBox == null)
                return;

            PauseUpdate = true;

            var changeTypeDlg = new TaeInspectorFormChangeEventType();
            changeTypeDlg.TAEReference = SelectedTae;
            changeTypeDlg.CurrentTemplate = SelectedEventBox.MyEvent.Template;
            changeTypeDlg.NewEventType = SelectedEventBox.MyEvent.Type;

            if (changeTypeDlg.ShowDialog(GameWindowAsForm) == System.Windows.Forms.DialogResult.OK)
            {
                if (changeTypeDlg.NewEventType != SelectedEventBox.MyEvent.Type)
                {
                    var referenceToEventBox = SelectedEventBox;
                    var referenceToPreviousEvent = referenceToEventBox.MyEvent;
                    int index = SelectedTaeAnim.Events.IndexOf(referenceToEventBox.MyEvent);
                    int row = referenceToEventBox.Row;

                    UndoMan.NewAction(
                        doAction: () =>
                        {
                            SelectedTaeAnim.Events.Remove(referenceToPreviousEvent);

                            referenceToEventBox.ChangeEvent(
                                new TAE.Event(referenceToPreviousEvent.StartTime, referenceToPreviousEvent.EndTime,
                                changeTypeDlg.NewEventType, referenceToPreviousEvent.Unk04, SelectedTae.BigEndian,
                                SelectedTae.BankTemplate[changeTypeDlg.NewEventType]));

                            SelectedTaeAnim.Events.Insert(index, referenceToEventBox.MyEvent);

                            SelectedEventBox = null;
                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.Row = row;

                            Graph.RegisterEventBoxExistance(SelectedEventBox);

                            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                            SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                        },
                        undoAction: () =>
                        {
                            SelectedTaeAnim.Events.RemoveAt(index);
                            referenceToEventBox.ChangeEvent(referenceToPreviousEvent);
                            SelectedTaeAnim.Events.Insert(index, referenceToPreviousEvent);

                            SelectedEventBox = null;
                            SelectedEventBox = referenceToEventBox;

                            SelectedEventBox.Row = row;

                            Graph.RegisterEventBoxExistance(SelectedEventBox);

                            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                            SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                        });
                }
            }

            PauseUpdate = false;
        }

        private void ButtonChangeType_Click(object sender, EventArgs e)
        {
            inspectorWinFormsControl.dataGridView1.Focus();
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
                stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown1}\"]");
                stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown2}\"]");

                //SFTODO

                //if (SelectedTaeAnim.AnimFileReference)
                //{
                //    stringBuilder.Append($" [RefID: {SelectedTaeAnim.RefAnimID}]");
                //}
                //else
                //{
                //    stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown1}\"]");
                //    stringBuilder.Append($" [\"{SelectedTaeAnim.Unknown2}\"]");

                //    //if (SelectedTaeAnim.IsLoopingObjAnim)
                //    //    stringBuilder.Append($" [ObjLoop]");

                //    //if (SelectedTaeAnim.UseHKXOnly)
                //    //    stringBuilder.Append($" [HKXOnly]");

                //    //if (SelectedTaeAnim.TAEDataOnly)
                //    //    stringBuilder.Append($" [TAEOnly]");

                //    //if (SelectedTaeAnim.OriginalAnimID >= 0)
                //    //    stringBuilder.Append($" [OrigID: {SelectedTaeAnim.OriginalAnimID}]");
                //}
            }

            SelectedTaeAnimInfoScrollingText.SetText(stringBuilder.ToString());
        }

        public void SelectEvent(TAE.Event ev)
        {
            var box = Graph.EventBoxes.First(x => x.MyEvent == ev);
            SelectedEventBox = box;

            float left = Graph.ScrollViewer.Scroll.X;
            float top = Graph.ScrollViewer.Scroll.Y;
            float right = Graph.ScrollViewer.Scroll.X + Graph.ScrollViewer.Viewport.Width;
            float bottom = Graph.ScrollViewer.Scroll.Y + Graph.ScrollViewer.Viewport.Height;

            Graph.ScrollViewer.Scroll.X = box.LeftFr - (Graph.ScrollViewer.Viewport.Width / 2f);
            Graph.ScrollViewer.Scroll.Y = (box.Row * Graph.RowHeight) - (Graph.ScrollViewer.Viewport.Height / 2f);
            Graph.ScrollViewer.ClampScroll();
        }

        public void SelectNewAnimRef(TAE tae, TAE.Animation animRef, bool scrollOnCenter = false)
        {
            PlaybackCursor.IsStepping = false;

            SelectedTae = tae;

            GameWindowAsForm.Invoke(new Action(() =>
            {
                ButtonEditCurrentTaeHeader.Enabled = true;
                ButtonEditCurrentTaeHeader.Visible = true;
            }));

            SelectedTaeAnim = animRef;

            UpdateSelectedTaeAnimInfoText();

            if (SelectedTaeAnim != null)
            {
                GameWindowAsForm.Invoke(new Action(() =>
                {
                    MenuBar["Edit/Undo"].Enabled = UndoMan.CanUndo;
                    MenuBar["Edit/Redo"].Enabled = UndoMan.CanRedo;
                }));
                
                SelectedEventBox = null;

                if (Graph == null)
                    Graph = new TaeEditAnimEventGraph(this);

                Graph.ChangeToNewAnimRef(SelectedTaeAnim);

                UpdateLayout(); // Fixes scroll when you first open anibnd (hopefully)

                editScreenAnimList.ScrollToAnimRef(SelectedTaeAnim, scrollOnCenter);

                Graph.ViewportInteractor.OnNewAnimSelected();

                Graph.PlaybackCursor.CurrentTime = 0;

                //TaeInterop.OnAnimationSelected(FileContainer.AllTAEDict, SelectedTae, SelectedTaeAnim);

                
            }
            else
            {
                GameWindowAsForm.Invoke(new Action(() =>
                {
                    MenuBar["Edit/Undo"].Enabled = false;
                    MenuBar["Edit/Redo"].Enabled = false;
                }));
                
                SelectedEventBox = null;

                Graph = null;
            }
        }


        public void ShowDialogFind()
        {
            if (FileContainerName == null || SelectedTae == null)
                return;
            PauseUpdate = true;

            var findWindow = new TaeFindValueDialog();
            findWindow.LastFindInfo = LastFindInfo;
            findWindow.EditorRef = this;
            findWindow.Owner = GameWindowAsForm;
            findWindow.ShowDialog();

            //var find = KeyboardInput.Show("Quick Find Event", "Finds the very first animation containing the event with the specified ID number or name (according to template).", "");
            //if (int.TryParse(find.Result, out int typeID))
            //{
            //    var gotoAnim = SelectedTae.Animations.Where(x => x.Events.Any(ev => (int)ev.Type == typeID));
            //    if (gotoAnim.Any())
            //        SelectNewAnimRef(SelectedTae, gotoAnim.First());
            //    else
            //        MessageBox.Show("None Found", "No events of that type found within the currently loaded files.", new[] { "OK" });
            //}
            //else 
            //{
            //    var gotoAnim = SelectedTae.Animations.Where(x => x.Events.Any(ev => ev.TypeName == find.Result));
            //    if (gotoAnim.Any())
            //        SelectNewAnimRef(SelectedTae, gotoAnim.First());
            //    else
            //        MessageBox.Show("None Found", "No events of that type found within the currently loaded files.", new[] { "OK" });
            //}


            
            PauseUpdate = false;
        }

        public void ShowDialogGoto()
        {
            if (FileContainer == null || SelectedTae == null)
                return;
            PauseUpdate = true;
            var anim = KeyboardInput.Show("Goto Anim", "Goes to the animation with the ID\n" +
                "entered, if applicable.");

            if (!anim.IsCanceled && anim.Result != null)
            {
                if (int.TryParse(anim.Result.Replace("a", "").Replace("_", ""), out int id))
                {
                    if (!GotoAnimID(id, scrollOnCenter: true))
                    {
                        MessageBox.Show("Goto Failed", $"Unable to find anim {id}.", new[] { "OK" });
                    }
                }
                else
                {
                    MessageBox.Show("Goto Failed", $"\"{anim.Result}\" is not a valid integer.", new[] { "OK" });
                }
            }
            
            PauseUpdate = false;
        }

        private void NextAnim(bool shiftPressed, bool ctrlPressed)
        {
            try
            {
                if (SelectedTae != null)
                {
                    if (SelectedTaeAnim != null)
                    {
                        var taeList = FileContainer.AllTAE.ToList();

                        int currentAnimIndex = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
                        int currentTaeIndex = taeList.IndexOf(SelectedTae);

                        int startingTaeIndex = currentTaeIndex;

                        void DoSmallStep()
                        {
                            if (currentAnimIndex >= taeList[currentTaeIndex].Animations.Count - 1)
                            {
                                currentAnimIndex = 0;

                                if (taeList.Count > 1)
                                {
                                    if (currentTaeIndex >= taeList.Count - 1)
                                    {
                                        currentTaeIndex = 0;
                                    }
                                    else
                                    {
                                        currentTaeIndex++;
                                    }
                                }
                            }
                            else
                            {
                                currentAnimIndex++;
                            }
                        }

                        void DoBigStep()
                        {
                            if (taeList.Count > 1)
                            {
                                while (currentTaeIndex == startingTaeIndex)
                                {
                                    DoSmallStep();
                                }
                            }

                            currentAnimIndex = 0;
                        }

                        void DoStep()
                        {
                            if (ctrlPressed)
                                DoBigStep();
                            else
                                DoSmallStep();
                        }

                        if (shiftPressed)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                DoStep();
                            }
                        }
                        else
                        {
                            DoStep();
                        }

                        try
                        {
                            SelectNewAnimRef(taeList[currentTaeIndex], taeList[currentTaeIndex].Animations[currentAnimIndex], scrollOnCenter: ShiftHeld || CtrlHeld);
                        }
                        catch// (Exception innerEx)
                        {
                            //Console.WriteLine("FATCAT");
                        }
                        
                    }
                }
            }
            catch// (Exception ex)
            {
                //Console.WriteLine("Fatcat");
            }
        }

        public void PrevAnim(bool shiftPressed, bool ctrlPressed)
        {
            try
            {
                if (SelectedTae != null)
                {
                    if (SelectedTaeAnim != null)
                    {
                        var taeList = FileContainer.AllTAE.ToList();

                        int currentAnimIndex = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
                        int currentTaeIndex = taeList.IndexOf(SelectedTae);

                        int startingTaeIndex = currentTaeIndex;

                        void DoSmallStep()
                        {
                            if (currentAnimIndex <= 0)
                            {
                                if (taeList.Count > 1)
                                {
                                    if (currentTaeIndex <= 0)
                                    {
                                        currentTaeIndex = taeList.Count - 1;
                                    }
                                    else
                                    {
                                        currentTaeIndex--;
                                    }
                                }

                                currentAnimIndex = taeList[currentTaeIndex].Animations.Count - 1;
                            }
                            else
                            {
                                currentAnimIndex--;
                            }
                        }

                        void DoBigStep()
                        {
                            if (taeList.Count > 1)
                            {
                                while (currentTaeIndex == startingTaeIndex)
                                {
                                    DoSmallStep();
                                }
                            }

                            currentAnimIndex = 0;
                        }

                        void DoStep()
                        {
                            if (ctrlPressed)
                                DoBigStep();
                            else
                                DoSmallStep();
                        }

                        if (shiftPressed)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                DoStep();
                            }
                        }
                        else
                        {
                            DoStep();
                        }

                        SelectNewAnimRef(taeList[currentTaeIndex], taeList[currentTaeIndex].Animations[currentAnimIndex], scrollOnCenter: ShiftHeld || CtrlHeld);
                    }
                }
            }
            catch
            {

            }
        }

        public void Update()
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

            //bad hotfix warning
            if (Graph != null && 
                !Graph.ScrollViewer.DisableVerticalScroll && 
                Input.MousePosition.X >= 
                  Graph.Rect.Right - 
                  Graph.ScrollViewer.ScrollBarThickness && 
                Input.MousePosition.X < DividerRightGrabStartX && 
                Input.MousePosition.Y >= Graph.Rect.Top && 
                Input.MousePosition.Y < Graph.Rect.Bottom)
            {
                Input.CursorType = MouseCursorType.Arrow;
            }

            Input.Update(Rect);

            if (!Input.LeftClickHeld)
                Graph?.MouseReleaseStuff();

            // Always update playback regardless of GUI memes.
            // Still only allow hitting spacebar to play/pause
            // if the window is in focus.
            // Also for Interactor
            if (Graph != null)
            {
                Graph.UpdatePlaybackCursor(allowPlayPauseInput: Main.Active);
                Graph.ViewportInteractor?.GeneralUpdate();
            }

            if (Main.Active)
            {
                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F1))
                    ChangeTypeOfSelectedEvent();

                CtrlHeld = Input.KeyHeld(Keys.LeftControl) || Input.KeyHeld(Keys.RightControl);
                ShiftHeld = Input.KeyHeld(Keys.LeftShift) || Input.KeyHeld(Keys.RightShift);
                AltHeld = Input.KeyHeld(Keys.LeftAlt) || Input.KeyHeld(Keys.RightAlt);

                var zHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Z);
                var yHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Y);

                if (CtrlHeld && !ShiftHeld && !AltHeld)
                {
                    if (Input.KeyDown(Keys.OemPlus))
                    {
                        Graph?.ZoomInOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if (Input.KeyDown(Keys.OemMinus))
                    {
                        Graph?.ZoomOutOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if (Input.KeyDown(Keys.D0) || Input.KeyDown(Keys.NumPad0))
                    {
                        Graph?.ResetZoom(0);
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.C) && WhereCurrentMouseClickStarted != ScreenMouseHoverKind.Inspector)
                    {
                        Graph?.DoCopy();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.X) && WhereCurrentMouseClickStarted != ScreenMouseHoverKind.Inspector)
                    {
                        Graph?.DoCut();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.V) && WhereCurrentMouseClickStarted != ScreenMouseHoverKind.Inspector)
                    {
                        Graph?.DoPaste(isAbsoluteLocation: false);
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.A))
                    {
                        if (Graph != null && Graph.currentDrag.DragType == BoxDragType.None)
                        {
                            SelectedEventBox = null;
                            MultiSelectedEventBoxes.Clear();
                            foreach (var box in Graph.EventBoxes)
                            {
                                MultiSelectedEventBoxes.Add(box);
                            }
                            UpdateInspectorToSelection();
                        }
                    }
                    else if (Input.KeyDown(Keys.F))
                    {
                        ShowDialogFind();
                    }
                    else if (Input.KeyDown(Keys.G))
                    {
                        ShowDialogGoto();
                    }
                    else if (Input.KeyDown(Keys.S))
                    {
                        SaveCurrentFile();
                    }
                }

                if (CtrlHeld && ShiftHeld && !AltHeld)
                {
                    if (Input.KeyDown(Keys.V))
                    {
                        Graph.DoPaste(isAbsoluteLocation: true);
                    }
                    if (Input.KeyDown(Keys.S))
                    {
                        File_SaveAs();
                    }
                }

                if (!CtrlHeld && ShiftHeld && !AltHeld)
                {
                    if (Input.KeyDown(Keys.D))
                    {
                        if (SelectedEventBox != null)
                            SelectedEventBox = null;
                        if (MultiSelectedEventBoxes.Count > 0)
                            MultiSelectedEventBoxes.Clear();
                    }
                }

                if (Input.KeyDown(Keys.Delete))
                {
                    Graph.DeleteSelectedEvent();
                }

                if (Graph != null && Input.KeyDown(Keys.Home) && !Graph.PlaybackCursor.Scrubbing)
                {
                    Graph.PlaybackCursor.IsPlaying = false;
                    Graph.PlaybackCursor.CurrentTime = Graph.PlaybackCursor.StartTime;
                }

                if (Graph != null && Input.KeyDown(Keys.End) && !Graph.PlaybackCursor.Scrubbing)
                {
                    Graph.PlaybackCursor.IsPlaying = false;
                    Graph.PlaybackCursor.CurrentTime = Graph.PlaybackCursor.MaxTime;
                }

                NextAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.PageDown));

                if (NextAnimRepeaterButton.State)
                {
                    NextAnim(ShiftHeld, CtrlHeld);
                }

                PrevAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.PageUp));

                if (PrevAnimRepeaterButton.State)
                {
                    PrevAnim(ShiftHeld, CtrlHeld);
                }

                if (PlaybackCursor != null && !PlaybackCursor.IsPlaying)
                {
                    NextFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Right));

                    if (NextFrameRepeaterButton.State)
                    {
                        PlaybackCursor.IsStepping = true;
                        PlaybackCursor.CurrentTime = PlaybackCursor.GUICurrentTime;
                        PlaybackCursor.CurrentTime += 0.03333333f
                            * (NextFrameRepeaterButton.IsInitalButtonTap ? 1 : 0.25f) * (ShiftHeld ? 5 : 1);
                        if (PlaybackCursor.CurrentTime > PlaybackCursor.MaxTime)
                            PlaybackCursor.CurrentTime %= PlaybackCursor.MaxTime;
                    }

                    PrevFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Left));

                    if (PrevFrameRepeaterButton.State)
                    {
                        PlaybackCursor.IsStepping = true;
                        PlaybackCursor.CurrentTime = PlaybackCursor.GUICurrentTime;
                        PlaybackCursor.CurrentTime -= 0.03333333f
                            * (PrevFrameRepeaterButton.IsInitalButtonTap ? 1 : 0.25f) * (ShiftHeld ? 5 : 1);
                        if (PlaybackCursor.CurrentTime < 0)
                            PlaybackCursor.CurrentTime += PlaybackCursor.MaxTime;
                    }
                }

                

                if (UndoButton.Update(Main.DELTA_UPDATE, (CtrlHeld && !ShiftHeld && !AltHeld) && (zHeld && !yHeld)))
                {
                    UndoMan.Undo();
                }

                if (RedoButton.Update(Main.DELTA_UPDATE, (CtrlHeld && !ShiftHeld && !AltHeld) && (!zHeld && yHeld)))
                {
                    UndoMan.Redo();
                }
            }

            if (!Input.LeftClickHeld)
                WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;


            if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.None)
            {
                if (Input.MousePosition.Y >= TopMenuBarMargin && Input.MousePosition.Y <= Rect.Bottom
                    && Input.MousePosition.X >= DividerLeftGrabStartX && Input.MousePosition.X <= DividerLeftGrabEndX)
                {
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.LeftVertical;
                        WhereCurrentMouseClickStarted = ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane;
                    }
                }
                else if (Input.MousePosition.Y >= TopMenuBarMargin && Input.MousePosition.Y <= Rect.Bottom
                    && Input.MousePosition.X >= DividerRightGrabStartX && Input.MousePosition.X <= DividerRightGrabEndX)
                {
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.RightVertical;
                        WhereCurrentMouseClickStarted = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    }
                }
                else if (Input.MousePosition.X >= RightSectionStartX && Input.MousePosition.X <= Rect.Right 
                    && Input.MousePosition.Y >= DividerRightPaneHorizontalGrabStartY && Input.MousePosition.Y <= DividerRightPaneHorizontalGrabEndY)
                {
                    MouseHoverKind = ScreenMouseHoverKind.DividerRightPaneHorizontal;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.RightPaneHorizontal;
                        WhereCurrentMouseClickStarted = ScreenMouseHoverKind.DividerRightPaneHorizontal;
                    }
                }
                else if (MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                    || MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane
                    || MouseHoverKind == ScreenMouseHoverKind.DividerRightPaneHorizontal)
                {
                    MouseHoverKind = ScreenMouseHoverKind.None;
                }
            }

            if (MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane 
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane 
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane)
            {
                Input.CursorType = MouseCursorType.DragX;
            }
            else if (MouseHoverKind == ScreenMouseHoverKind.DividerRightPaneHorizontal 
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerRightPaneHorizontal)
            {
                Input.CursorType = MouseCursorType.DragY;
            }

            if (CurrentDividerDragMode == DividerDragMode.LeftVertical)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragX;
                    LeftSectionWidth = MathHelper.Max((Input.MousePosition.X - Rect.X) - (DividerVisiblePad / 2), LeftSectionWidthMin);
                    LeftSectionWidth = MathHelper.Min(LeftSectionWidth, Rect.Width - MiddleSectionWidthMin - RightSectionWidth - (DividerVisiblePad * 2));
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.RightVertical)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragX;
                    RightSectionWidth = MathHelper.Max((Rect.Right - Input.MousePosition.X) + (DividerVisiblePad / 2), RightSectionWidthMin);
                    RightSectionWidth = MathHelper.Min(RightSectionWidth, Rect.Width - MiddleSectionWidthMin - LeftSectionWidth - (DividerVisiblePad * 2));
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.RightPaneHorizontal)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragY;
                    TopRightPaneHeight = MathHelper.Max((Input.MousePosition.Y - Rect.Top - TopMenuBarMargin) + (DividerVisiblePad / 2), TopRightPaneHeightMinNew);
                    TopRightPaneHeight = MathHelper.Min(TopRightPaneHeight, Rect.Height - BottomRightPaneHeightMinNew - DividerVisiblePad - TopMenuBarMargin);
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }

            if (!Rect.Contains(Input.MousePositionPoint))
            {
                MouseHoverKind = ScreenMouseHoverKind.None;
            }

            // Very specific edge case to handle before you load an anibnd so that
            // it won't have the resize cursor randomly. This box spans all the way
            // from left of screen to the hitbox of the right vertical divider and
            // just immediately clears the resize cursor in that entire huge region.
            if (editScreenAnimList == null && Graph == null
                    && new Rectangle(Rect.Left, Rect.Top, (int)(DividerRightGrabStartX - Rect.Left), Rect.Height).Contains(Input.MousePositionPoint))
            {
                MouseHoverKind = ScreenMouseHoverKind.None;
                Input.CursorType = MouseCursorType.Arrow;
            }

            // Check if currently dragging to resize panes.
            if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane)
            {
                Input.CursorType = MouseCursorType.DragX;
                GFX.World.DisableAllInput = true;
                return;
            }
            else if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerRightPaneHorizontal)
            {
                Input.CursorType = MouseCursorType.DragY;
                GFX.World.DisableAllInput = true;
                return;
            }
            else if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.ShaderAdjuster)
            {
                GFX.World.DisableAllInput = true;
                return;
            }
            else if (!(MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerRightPaneHorizontal))
            {
                if (editScreenAnimList != null && editScreenAnimList.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.AnimList;
                else if (Graph != null && Graph.Rect.Contains(Input.MousePositionPoint))
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
                else if (ShaderAdjuster.Bounds.Contains(new System.Drawing.Point(Input.MousePositionPoint.X, Input.MousePositionPoint.Y)))
                    MouseHoverKind = ScreenMouseHoverKind.ShaderAdjuster;
                else if (
                    ModelViewerBounds.Contains(Input.MousePositionPoint))
                {
                    MouseHoverKind = ScreenMouseHoverKind.ModelViewer;
                }
                else
                    MouseHoverKind = ScreenMouseHoverKind.None;

                if (Input.LeftClickDown)
                {
                    WhereCurrentMouseClickStarted = MouseHoverKind;
                }

                if (editScreenAnimList != null)
                {

                    if (MouseHoverKind == ScreenMouseHoverKind.AnimList || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.AnimList)
                    {
                        Input.CursorType = MouseCursorType.Arrow;
                        editScreenAnimList.Update(Main.DELTA_UPDATE, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                    else
                    {
                        editScreenAnimList.UpdateMouseOutsideRect(Main.DELTA_UPDATE, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                }

                if (Graph != null)
                {
                    if (MouseHoverKind == ScreenMouseHoverKind.EventGraph || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.EventGraph)
                        Graph.Update(allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    else
                        Graph.UpdateMouseOutsideRect(Main.DELTA_UPDATE, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                }

                if (MouseHoverKind == ScreenMouseHoverKind.ModelViewer || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.ModelViewer)
                {
                    Input.CursorType = MouseCursorType.Arrow;
                    GFX.World.DisableAllInput = false;
                }
                else
                {
                    //GFX.World.DisableAllInput = true;
                }

                if (MouseHoverKind == ScreenMouseHoverKind.Inspector || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.Inspector)
                {
                    Input.CursorType = MouseCursorType.Arrow;
                }
            }

            //else
            //{
            //    Input.CursorType = MouseCursorType.Arrow;
            //}

            //if (MouseHoverKind == ScreenMouseHoverKind.Inspector)
            //    Input.CursorType = MouseCursorType.StopUpdating;

            //if (editScreenGraphInspector.Rect.Contains(Input.MousePositionPoint))
            //    editScreenGraphInspector.Update(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
            //else
            //    editScreenGraphInspector.UpdateMouseOutsideRect(elapsedSeconds, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);

            oldMouseHoverKind = MouseHoverKind;
        }

        private void UpdateLayout()
        {
            if (Rect.IsEmpty || !Main.Active)
            {
                return;
            }

            if (TopRightPaneHeight < TopRightPaneHeightMinNew)
                TopRightPaneHeight = TopRightPaneHeightMinNew;




            if (RightSectionWidth < RightSectionWidthMin)
                RightSectionWidth = RightSectionWidthMin;

            if (TopRightPaneHeight > (Rect.Height - BottomRightPaneHeightMinNew - TopMenuBarMargin))
            {
                TopRightPaneHeight = (Rect.Height - BottomRightPaneHeightMinNew - TopMenuBarMargin);
                Main.RequestViewportRenderTargetResolutionChange = true;
            }

            if (editScreenAnimList != null && Graph != null)
            {
                if (LeftSectionWidth < LeftSectionWidthMin)
                {
                    LeftSectionWidth = LeftSectionWidthMin;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }


                if (MiddleSectionWidth < MiddleSectionWidthMin)
                {
                    var adjustment = MiddleSectionWidthMin - MiddleSectionWidth;
                    RightSectionWidth -= adjustment;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }

                editScreenAnimList.Rect = new Rectangle(
                    (int)LeftSectionStartX,
                    Rect.Top + TopMenuBarMargin, 
                    (int)LeftSectionWidth, 
                    Rect.Height - TopMenuBarMargin - EditTaeHeaderButtonMargin);

                Graph.Rect = new Rectangle(
                    (int)MiddleSectionStartX, 
                    Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                    (int)MiddleSectionWidth,
                    Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

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

            ButtonEditCurrentTaeHeader.Bounds = new System.Drawing.Rectangle(
                    (int)LeftSectionStartX,
                    Rect.Bottom - EditTaeHeaderButtonHeight,
                    (int)LeftSectionWidth,
                    EditTaeHeaderButtonHeight);

            //editScreenGraphInspector.Rect = new Rectangle(Rect.Width - LayoutInspectorWidth, 0, LayoutInspectorWidth, Rect.Height);


            //inspectorWinFormsControl.Bounds = new System.Drawing.Rectangle((int)RightSectionStartX, Rect.Top + TopMenuBarMargin, (int)RightSectionWidth, (int)(Rect.Height - TopMenuBarMargin - BottomRightPaneHeight - DividerVisiblePad));
            //ModelViewerBounds = new Rectangle((int)RightSectionStartX, (int)(Rect.Bottom - BottomRightPaneHeight), (int)RightSectionWidth, (int)(BottomRightPaneHeight));

            //ShaderAdjuster.Size = new System.Drawing.Size((int)RightSectionWidth, ShaderAdjuster.Size.Height);
            ModelViewerBounds = new Rectangle((int)RightSectionStartX, Rect.Top + TopMenuBarMargin, (int)RightSectionWidth, (int)(TopRightPaneHeight));
            inspectorWinFormsControl.Bounds = new System.Drawing.Rectangle((int)RightSectionStartX, (int)(Rect.Top + TopMenuBarMargin + TopRightPaneHeight + DividerVisiblePad), (int)RightSectionWidth, (int)(Rect.Height - TopRightPaneHeight - DividerVisiblePad - TopMenuBarMargin));
            ShaderAdjuster.Location = new System.Drawing.Point(Rect.Right - ShaderAdjuster.Size.Width, Rect.Top + TopMenuBarMargin);
        }

        public void DrawDimmingRect(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex)
        {
            sb.Begin();
            sb.Draw(boxTex, new Rectangle(Rect.Left, Rect.Top, (int)RightSectionStartX - Rect.X, Rect.Height), Color.Black * 0.25f);
            sb.End();
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex, SpriteFont font, float elapsedSeconds)
        {
            sb.Begin();
            sb.Draw(boxTex, new Rectangle(Rect.X, Rect.Y, (int)RightSectionStartX - Rect.X, Rect.Height), Config.EnableColorBlindMode ? Color.Black : new Color(0.2f, 0.2f, 0.2f));

            // Draw model viewer background lel
            //sb.Draw(boxTex, ModelViewerBounds, Color.Gray);

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

            if (Graph != null)
            {
                Graph.Draw(gd, sb, boxTex, font, elapsedSeconds);
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
