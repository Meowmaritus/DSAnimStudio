using DSAnimStudio.GFXShaders;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoulsFormats;
using SoulsAssetPipeline.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static DSAnimStudio.TaeEditor.TaeEditAnimEventGraph;
using System.Diagnostics;
using SharpDX.DirectWrite;
using System.IO;
using System.Threading.Tasks;
using DSAnimStudio.ImguiOSD;

namespace DSAnimStudio.TaeEditor
{
    public class TaeEditorScreen
    {
        public bool RequestGoToEventSource = false;
        public bool REMO_HOTFIX_REQUEST_PLAY_RESUME_NEXT_FRAME = false;
        public bool REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME { get; private set; } = false;

        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = false;

        public TAE REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = null;
        public TAE.Animation REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = null;
        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV = false;
        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT = false;
        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL = false;

        public bool REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME { get; private set; } = false;

        public const string BackupExtension = ".dsasbak";

        private ContentManager DebugReloadContentManager = null;
#if DEBUG
        public void Tools_ScanForUnusedAnimations()
        {
            List<string> usedAnims = new List<string>();
            List<string> unusedAnims = new List<string>();
            foreach (var anim in SelectedTae.Animations)
            {
                string hkx = Graph.ViewportInteractor.GetFinalAnimFileName(SelectedTae, anim);
                if (!usedAnims.Contains(hkx))
                    usedAnims.Add(hkx);
            }
            foreach (var anim in Graph.ViewportInteractor.CurrentModel.AnimContainer.Animations.Keys)
            {
                if (!usedAnims.Contains(anim) && !Graph.ViewportInteractor.CurrentModel.AnimContainer.AdditiveBlendOverlayNames.Contains(anim)
                && !unusedAnims.Contains(anim))
                    unusedAnims.Add(anim);
            }
            //var sb = new StringBuilder();
            foreach (var anim in unusedAnims)
            {
                //sb.AppendLine(anim);
                int id = int.Parse(anim.Replace(".hkx", "").Replace("_", "").Replace("a", ""));
                var newAnim = new TAE.Animation(9_000_000000 + id, new TAE.Animation.AnimMiniHeader.Standard()
                {
                    ImportHKXSourceAnimID = id,
                    ImportsHKX = true,
                }, $"UNUSED:{anim.Replace(".hkx", "")}");
                SelectedTae.Animations.Add(newAnim);
            }
            RecreateAnimList();
        }
#endif

        public void Tools_ExportCurrentTAE()
        {
            Main.WinForm.Invoke(new Action(() =>
            {
                var browseDlg = new System.Windows.Forms.SaveFileDialog()
                {
                    Filter = "TAE Files (*.tae)|*.tae",
                    ValidateNames = true,
                    CheckPathExists = true,
                    //ShowReadOnly = true,
                    Title = "Choose where to save loose TAE file.",

                };

                var decision = browseDlg.ShowDialog();

                if (decision == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        SelectedTae.Write(browseDlg.FileName);
                        System.Windows.Forms.MessageBox.Show("TAE saved successfully.", "Saved");
                    }
                    catch (Exception exc)
                    {
                        System.Windows.Forms.MessageBox.Show($"Error saving TAE file:\n\n{exc}", "Failed to Save",
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                }

            }));
        }

        public void BringUpImporter_FLVER2()
        {
            if (ImporterWindow_FLVER2 == null || ImporterWindow_FLVER2.IsDisposed || !ImporterWindow_FLVER2.Visible)
            {
                ImporterWindow_FLVER2?.Dispose();
                ImporterWindow_FLVER2 = null;
            }
            ImporterWindow_FLVER2 = new SapImportFlver2Form();
            ImporterWindow_FLVER2.ImportConfig = Config.LastUsedImportConfig_FLVER2;
            ImporterWindow_FLVER2.MainScreen = this;
            ImporterWindow_FLVER2.Show();
            // The CenterParent stuff just doesn't work for some reason, heck it.

            Main.CenterForm(ImporterWindow_FLVER2);

            ImporterWindow_FLVER2.LoadValuesFromConfig();
        }

        public void BringUpImporter_FBXAnim()
        {
            if (ImporterWindow_FBXAnim == null || ImporterWindow_FBXAnim.IsDisposed || !ImporterWindow_FBXAnim.Visible)
            {
                ImporterWindow_FBXAnim?.Dispose();
                ImporterWindow_FBXAnim = null;
            }
            ImporterWindow_FBXAnim = new SapImportFbxAnimForm();
            ImporterWindow_FBXAnim.ImportConfig = Config.LastUsedImportConfig_AnimFBX;
            ImporterWindow_FBXAnim.MainScreen = this;
            ImporterWindow_FBXAnim.Show();
            // The CenterParent stuff just doesn't work for some reason, heck it.

            Main.CenterForm(ImporterWindow_FBXAnim);

            ImporterWindow_FBXAnim.LoadValuesFromConfig();
        }

        public void ShowExportAllAnimsMenu()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                if (!IsFileOpen)
                    return;

                if (ExportAllAnimsMenu == null || ExportAllAnimsMenu.IsDisposed)
                {
                    ExportAllAnimsMenu = new TaeExportAllAnimsForm();
                    ExportAllAnimsMenu.Owner = GameWindowAsForm;
                    ExportAllAnimsMenu.MainScreen = this;
                    ExportAllAnimsMenu.ShownInitValues();
                }

                ExportAllAnimsMenu.Show();
                Main.CenterForm(ExportAllAnimsMenu);
                ExportAllAnimsMenu.Activate();
            }));
        }

        public void ImmediateExportAllEventsToTextFile(string textFilePath)
        {
            var sb = new StringBuilder();

            foreach (var tae in AnimationListScreen.AnimTaeSections)
            {
                foreach (var animSection in tae.Value.InfoMap)
                {
                    string animName = animSection.Value.GetName();
                    if (!string.IsNullOrWhiteSpace(animSection.Key.AnimFileName))
                        animName += " " + animSection.Key.AnimFileName;
                    sb.AppendLine("--------------------------------------------------------------------------------");
                    sb.AppendLine(animName);
                    sb.AppendLine("--------------------------------------------------------------------------------");
                    if (animSection.Key.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim && asImportOtherAnim.ImportFromAnimID >= 0)
                    {
                        sb.AppendLine($"  Imports all events from {GameRoot.SplitAnimID.FromFullID(asImportOtherAnim.ImportFromAnimID).GetFormattedIDString()}.");
                    }
                    else
                    {
                        var eventList = animSection.Key.Events.OrderBy(ev => ev.Type);
                        List<string> frameRangeTexts = new List<string>();
                        List<string> eventInfoTexts = new List<string>();
                        foreach (var ev in eventList)
                        {
                            int startFrame = (int)Math.Floor(ev.StartTime / TaeEditAnimEventBox.TAE_FRAME_30);
                            int endFrame = (int)Math.Floor(ev.EndTime / TaeEditAnimEventBox.TAE_FRAME_30);
                            string eventText = TaeEditAnimEventBox.GetEventBoxText(ev);
                            frameRangeTexts.Add($"  {startFrame}-{(ev.EndTime >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder ? "M" : endFrame.ToString())}");
                            eventInfoTexts.Add($"{eventText}");
                        }
                        if (frameRangeTexts.Count > 0)
                        {
                            int maxFrameRangeLength = frameRangeTexts.Max(x => x.Length);
                            for (int i = 0; i < frameRangeTexts.Count; i++)
                            {
                                sb.AppendLine($"{(frameRangeTexts[i] + new string(' ', (maxFrameRangeLength) - frameRangeTexts[i].Length))} {eventInfoTexts[i]}");
                            }
                        }
                        else
                        {
                            sb.AppendLine($"  Animation contains no events.");
                        }
                    }
                    
                    sb.AppendLine("");
                }
            }

            File.WriteAllText(textFilePath, sb.ToString());
            NotificationManager.PushNotification("Exported text file successfully.");
        }

        public void ImmediateExportAllAnimNamesToTextFile(string textFilePath)
        {
            var sb = new StringBuilder();

            foreach (var tae in AnimationListScreen.AnimTaeSections)
            {
                foreach (var animSection in tae.Value.InfoMap)
                {
                    string animIDString = animSection.Value.GetName();
                    string animNameString = !string.IsNullOrWhiteSpace(animSection.Key.AnimFileName) ? animSection.Key.AnimFileName : "";
                    sb.AppendLine($"{animIDString}={animNameString}");
                }
                sb.AppendLine("");
            }

            File.WriteAllText(textFilePath, sb.ToString());
            NotificationManager.PushNotification("Exported text file successfully.");
        }

        public void ShowExportAllAnimNamesDialog()
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "Text Files (*.TXT)|*.TXT",
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(FileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(FileContainerName + ".AnimNames.txt");
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmediateExportAllAnimNamesToTextFile(browseDlg.FileName);
            }
        }

        public void ImmediateImportAllAnimNamesFromTextFile(string textFilePath)
        {
            Dictionary<string, string> animNameDict = new Dictionary<string, string>();
            var txtFileLines = File.ReadAllLines(textFilePath);
            for (int i = 0; i < txtFileLines.Length; i++)
            {
                var t = txtFileLines[i].Trim();
                if (!string.IsNullOrWhiteSpace(t))
                {
                    var equalsIndex = t.IndexOf('=');
                    if (equalsIndex != -1)
                    {
                        var beforeEquals = t.Substring(0, equalsIndex).Trim().ToLower();
                        var afterEquals = equalsIndex < (t.Length - 1) ? t.Substring(equalsIndex + 1).Trim() : "";
                        if (!animNameDict.ContainsKey(beforeEquals))
                            animNameDict.Add(beforeEquals, afterEquals);
                        else
                            animNameDict[beforeEquals] = afterEquals;
                    }
                }
            }

            int numNamesEdited = 0;

            foreach (var tae in AnimationListScreen.AnimTaeSections)
            {
                foreach (var animSection in tae.Value.InfoMap)
                {
                    string animIDString = animSection.Value.GetName();
                    if (animNameDict.ContainsKey(animIDString))
                    {
                        var check = animSection.Key.AnimFileName;
                        if (string.IsNullOrWhiteSpace(check))
                            check = "";
                        if (check != animNameDict[animIDString])
                        {
                            animSection.Key.AnimFileName = animNameDict[animIDString];
                            animSection.Key.SetIsModified(true);
                            tae.Value.Tae.SetIsModified(true);
                            numNamesEdited++;
                        }
                    }
                }
            }

            NotificationManager.PushNotification($"Successfully imported animation names from text file." +
                $"\n{numNamesEdited} animation names were modified by the import process.");
        }

        public void ShowImportAllAnimNamesDialog()
        {
            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                Filter = "Text Files (*.TXT)|*.TXT",
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(FileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(FileContainerName + ".AnimNames.txt");
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmediateImportAllAnimNamesFromTextFile(browseDlg.FileName);
            }
        }

        public void ShowManageTaeSectionsDialog()
        {
            Task.Run(() =>
            {
                var dlg = new TaeManageSectionsForm();
                dlg.SetInitialSectionList(FileContainer.AllTAEDict.Keys.Select(x => Utils.GetShortIngameFileName(x)).ToList());
                dlg.ShowDialog();
            });
            
        }

        public void ShowManageAnimationsDialog()
        {
            throw new NotImplementedException();
        }

        public void ShowExportAllEventsToTextFileDialog()
        {
            var browseDlg = new System.Windows.Forms.SaveFileDialog()
            {
                Filter = "Text Files (*.TXT)|*.TXT",
                ValidateNames = true,
                CheckFileExists = false,
                CheckPathExists = true,
            };

            if (System.IO.File.Exists(FileContainerName))
            {
                browseDlg.InitialDirectory = System.IO.Path.GetDirectoryName(FileContainerName);
                browseDlg.FileName = System.IO.Path.GetFileName(FileContainerName + ".txt");
            }

            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ImmediateExportAllEventsToTextFile(browseDlg.FileName);
            }
        }

        public void ShowComboMenu()
        {
            GameWindowAsForm.Invoke(new Action(() =>
            {
                if (!IsFileOpen)
                    return;

                if (ComboMenu == null || ComboMenu.IsDisposed)
                {
                    ComboMenu = new TaeComboMenu();
                    ComboMenu.Owner = GameWindowAsForm;
                    ComboMenu.MainScreen = this;
                    ComboMenu.SetupTaeComboBoxes();
                }

                ComboMenu.Show();
                Main.CenterForm(ComboMenu);
                ComboMenu.Activate();
            }));
            
        }

        public TaeComboMenu ComboMenu = null;
        public TaeExportAllAnimsForm ExportAllAnimsMenu = null;

        public float AnimSwitchRenderCooldown = 0;
        public float AnimSwitchRenderCooldownMax = 0.3f;
        public float AnimSwitchRenderCooldownFadeLength = 0.1f;
        public Color AnimSwitchRenderCooldownColor = Color.Black * 0.35f;

        private bool HasntSelectedAnAnimYetAfterBuildingAnimList = true;

        enum DividerDragMode
        {
            None,
            LeftVertical,
            RightVertical,
            RightPaneHorizontal,
            BottomLeftCornerResize
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
            BottomLeftCornerOfModelViewer,
            ShaderAdjuster
        }

        public DbgMenus.DbgMenuPadRepeater NextAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadDown, 0.4f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevAnimRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadUp, 0.4f, 0.016666667f);

        public DbgMenus.DbgMenuPadRepeater NextAnimRepeaterButton_KeepSubID = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadDown, 0.4f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevAnimRepeaterButton_KeepSubID = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadUp, 0.4f, 0.016666667f);

        public DbgMenus.DbgMenuPadRepeater NextFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadLeft, 0.3f, 0.016666667f);
        public DbgMenus.DbgMenuPadRepeater PrevFrameRepeaterButton = new DbgMenus.DbgMenuPadRepeater(Buttons.DPadRight, 0.3f, 0.016666667f);

        public static bool CurrentlyEditingSomethingInInspector;
        
        public class FindInfoKeep
        {
            public enum TaeSearchType : int
            {
                ParameterValue = 0,
                ParameterName = 1,
                EventName = 2,
                EventType = 3,
            }

            public string SearchQuery;
            public bool MatchEntireString;
            public List<TaeFindResult> Results;
            public int HighlightedIndex;
            public TaeSearchType SearchType;
        }

        public SapImportFlver2Form ImporterWindow_FLVER2 = null;
        public SapImportFbxAnimForm ImporterWindow_FBXAnim = null;

        public FindInfoKeep LastFindInfo = null;
        public TaeFindValueDialog FindValueDialog = null;

        public TaePlaybackCursor PlaybackCursor => Graph?.PlaybackCursor;

        public Rectangle ModelViewerBounds;
        public Rectangle ModelViewerBounds_InputArea;

        private const int RECENT_FILES_MAX = 32;

        //TODO: CHECK THIS
        private int TopMenuBarMargin => 24;

        private int TopOfGraphAnimInfoMargin = 20;

        private int TransportHeight = 32;

        public TaeTransport Transport;

        public void GoToEventSource()
        {
            if (Graph.AnimRef.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
            {

                var animRef = FileContainer.GetAnimRefFull(asImportOtherAnim.ImportFromAnimID);

                if (animRef.Item1 == null || animRef.Item2 == null)
                {
                    DialogManager.DialogOK("Invalid Animation Reference", $"Animation ID referenced ({asImportOtherAnim.ImportFromAnimID}) does not exist.");
                    return;
                }
                SelectNewAnimRef(animRef.Item1, animRef.Item2);
            }
        }

        //public bool CtrlHeld;
        //public bool ShiftHeld;
        //public bool AltHeld;

        public const string HELP_TEXT =
            "Left Click + Drag on Timeline:\n" +
            "    Scrub animation frame.\n" +
            "Left Click + Hold Shift + Drag on Timeline:\n" +
            "    Scrub animation frame while ignoring autoscroll.\n" +
            "Left Click + Drag Middle of Event:\n" +
            "    Move whole event.\n" +
            "Left Click + Drag Left/Right Side of Event:\n" +
            "    Move start/end of event.\n" +
            "Left Click:\n" +
            "    Highlight event under mouse cursor.\n" +
            "Left Click and Drag:\n" +
            "    Drag selection rectangle to highlight multiple events.\n" +
            "Shift + Left Click:\n" +
            "    Add to current selection (works for multiselect as well).\n" +
            "Ctrl + Left Click:\n" +
            "    Subtract from current selection (works for multiselect as well).\n" +
            "Right Click:\n" +
            "    Place copy of last highlighted event at mouse cursor.\n" +
            "Delete Key:\n" +
            "    Delete highlighted event.\n" +
            "Ctrl+X/Ctrl+C/Ctrl+V:\n" +
            "    CUT/COPY/PASTE.\n" +
            "Ctrl+Z/Ctrl+Y:\n" +
            "    UNDO/REDO.\n" +
            "F1 Key:\n" +
            "    Change type of highlighted event.\n" +
            "Space Bar:\n" +
            "    Play/Pause Anim.\n" +
            "Shift+Space Bar:\n" +
            "    Play Anim from beginning.\n" +
            "Ctrk+R Key:\n" +
            "    Reset animation/root motion.\n" +
            "Ctrl+Mouse Wheel:\n" +
            "    Zoom timeline in/out.\n" +
            "Ctrl+(+/-/0):\n" +
            "   Zoom in/out/reset\n" +
            "Left/Right Arrow Keys:\n" +
            "    Goes to previous/next frame.\n" +
            "Home/End:\n" +
            "    Without Shift: Go to playback start point / playback end point.\n" +
            "    With Shift: Go to start of animation / end of animation.\n" +
            "    With Ctrl: Stop playing animation after jumping.\n" +
            "    (Ctrl and Shift can be combined.)\n";

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
                if (SelectedTaeAnim == null)
                    return null;

                if (!UndoManDictionary.ContainsKey(SelectedTaeAnim))
                {
                    var newUndoMan = new TaeUndoMan(this);
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
                    (FileContainer?.AllTAE.Any(t => t.GetIsModified()) ?? false) || (FileContainer?.IsModified ?? false);
                }
                catch
                {
                    return true;
                }
            }
        }

        private void PushNewRecentFile(string fileName, string fileName_Model)
        {
            lock (Main.Config._lock_ThreadSensitiveStuff)
            {
                while (Config.RecentFilesList.Any(f => f.TaeFile == fileName))
                    Config.RecentFilesList.RemoveAll(f => f.TaeFile == fileName);

                while (Config.RecentFilesList.Count >= RECENT_FILES_MAX)
                    Config.RecentFilesList.RemoveAt(Config.RecentFilesList.Count - 1);

                Config.RecentFilesList.Insert(0, new TaeConfigFile.TaeRecentFileEntry(fileName, fileName_Model));
            }

            Main.SaveConfig();
        }


        private TaeButtonRepeater UndoButton = new TaeButtonRepeater(0.4f, 0.05f);
        private TaeButtonRepeater RedoButton = new TaeButtonRepeater(0.4f, 0.05f);

        public int PaddingAroundDragableThings = 12;

        public void DefaultLayout()
        {
            LeftSectionWidth = 286;
            RightSectionWidth = 600;
            TopRightPaneHeight = 600;

        }

        public float LeftSectionWidth = 286;
        private const float LeftSectionWidthMin = 150;
        private float DividerLeftVisibleStartX => Rect.Left + LeftSectionWidth;
        private float DividerLeftVisibleEndX => Rect.Left + LeftSectionWidth + DividerVisiblePad;

        public float RightSectionWidth = 600;
        private const float RightSectionWidthMin = 320;
        private float DividerRightVisibleStartX => Rect.Right - RightSectionWidth - DividerVisiblePad;
        private float DividerRightVisibleEndX => Rect.Right - RightSectionWidth;


        private float DividerRightCenterX => DividerRightVisibleStartX + ((DividerRightVisibleEndX - DividerRightVisibleStartX) / 2);
        private float DividerLeftCenterX => DividerLeftVisibleStartX + ((DividerLeftVisibleEndX - DividerLeftVisibleStartX) / 2);

        private float DividerRightGrabStartX => DividerRightCenterX - (DividerHitboxPad / 2);
        private float DividerRightGrabEndX => DividerRightCenterX + (DividerHitboxPad / 2);

        private float DividerLeftGrabStartX => DividerLeftCenterX - (DividerHitboxPad / 2);
        private float DividerLeftGrabEndX => DividerLeftCenterX + (DividerHitboxPad / 2);

        public float TopRightPaneHeight = 600;
        private const float TopRightPaneHeightMinNew = 128;
        private const float BottomRightPaneHeightMinNew = 64;

        private float DividerRightPaneHorizontalVisibleStartY => Rect.Top + TopRightPaneHeight + TopMenuBarMargin + TransportHeight;
        private float DividerRightPaneHorizontalVisibleEndY => Rect.Top + TopRightPaneHeight + DividerVisiblePad + TopMenuBarMargin + TransportHeight;
        private float DividerRightPaneHorizontalCenterY => DividerRightPaneHorizontalVisibleStartY + ((DividerRightPaneHorizontalVisibleEndY - DividerRightPaneHorizontalVisibleStartY) / 2);

        private float DividerRightPaneHorizontalGrabStartY => DividerRightPaneHorizontalCenterY - (DividerHitboxPad / 2);
        private float DividerRightPaneHorizontalGrabEndY => DividerRightPaneHorizontalCenterY + (DividerHitboxPad / 2);

        private float LeftSectionStartX => Rect.Left;
        private float MiddleSectionStartX => DividerLeftVisibleEndX;
        private float RightSectionStartX => Rect.Right - RightSectionWidth;

        private float MiddleSectionWidth => DividerRightVisibleStartX - DividerLeftVisibleEndX;
        private const float MiddleSectionWidthMin = 128;

        private float DividerVisiblePad = 3;
        private int DividerHitboxPad = 6;

        private DividerDragMode CurrentDividerDragMode = DividerDragMode.None;

        public ScreenMouseHoverKind MouseHoverKind = ScreenMouseHoverKind.None;
        private ScreenMouseHoverKind oldMouseHoverKind = ScreenMouseHoverKind.None;
        public ScreenMouseHoverKind WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
        public ScreenMouseHoverKind WhereLastMouseClickStarted = ScreenMouseHoverKind.None;

        public void DrawDebug()
        {
#if DEBUG
            //GFX.SpriteBatchBeginForText();
            //DrawPanelDragDebug(new Vector2(64, 64));
            //GFX.SpriteBatchEnd();
#endif
        }

        public void DrawPanelDragDebug(Vector2 pos)
        {
            float borderThickness = 8;
            var fnt = DBG.DEBUG_FONT_SMALL;
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(CurrentDividerDragMode)} = {CurrentDividerDragMode}");
            sb.AppendLine($"{nameof(MouseHoverKind)} = {MouseHoverKind}");
            sb.AppendLine($"{nameof(WhereCurrentMouseClickStarted)} = {WhereCurrentMouseClickStarted}");
            sb.AppendLine($"{nameof(WhereLastMouseClickStarted)} = {WhereLastMouseClickStarted}");
            var txt = sb.ToString();
            var boxSize = fnt.MeasureString(txt) + (Vector2.One * (borderThickness * 2));
            GFX.SpriteBatch.Draw(Main.TAE_EDITOR_BLANK_TEX, new Rectangle((int)Math.Round(pos.X), (int)Math.Round(pos.Y), (int)Math.Round(boxSize.X), (int)Math.Round(boxSize.Y)), Color.Black * 0.75f);
            GFX.SpriteBatch.DrawString(fnt, txt, pos + (Vector2.One * borderThickness), Color.Yellow);
        }

        public TaeFileContainer FileContainer;

        public bool IsFileOpen => FileContainer != null;

        public TAE SelectedTae { get; private set; }

        public TAE.Animation SelectedTaeAnim { get; private set; }
        private TaeScrollingString SelectedTaeAnimInfoScrollingText = new TaeScrollingString();
        private bool TaeAnimInfoIsClone = false;

        public readonly System.Windows.Forms.Form GameWindowAsForm;

        public bool QueuedChangeEventType = false;

        public bool SingleEventBoxSelected => SelectedEventBox != null && MultiSelectedEventBoxes.Count == 0;

        public TaeEditAnimEventBox PrevHoveringOverEventBox = null;

        public TaeEditAnimEventBox HoveringOverEventBox = null;

        private TaeEditAnimEventBox _selectedEventBox = null;
        public TaeEditAnimEventBox SelectedEventBox
        {
            get => _selectedEventBox;
            set
            {
                //inspectorWinFormsControl.DumpDataGridValuesToEvent();

                //if (value != null && value != _selectedEventBox)
                //{
                //    if (Config.UseGamesMenuSounds)
                //        FmodManager.PlaySE("f000000000");
                //}

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
            }
        }

        public List<TaeEditAnimEventBox> MultiSelectedEventBoxes = new List<TaeEditAnimEventBox>();
        private int multiSelectedEventBoxesCountLastFrame = 0;

        public TaeEditAnimList AnimationListScreen;
        public TaeEditAnimEventGraph Graph { get; private set; }
        public bool IsCurrentlyLoadingGraph { get; private set; } = false;
        //private Dictionary<TAE.Animation, TaeEditAnimEventGraph> GraphLookup = new Dictionary<TAE.Animation, TaeEditAnimEventGraph>();
        //private TaeEditAnimEventGraphInspector editScreenGraphInspector;

        private Color ColorInspectorBG = Color.DarkGray;
        public System.Numerics.Vector2 ImGuiEventInspectorPos = System.Numerics.Vector2.Zero;
        public System.Numerics.Vector2 ImGuiEventInspectorSize = System.Numerics.Vector2.Zero;

        public FancyInputHandler Input => Main.Input;

        public string FileContainerName = "";
        public string FileContainerName_Model = "";
        public bool SuppressNextModelOverridePrompt = false;
        public string FileContainerName_2010 => FileContainerName + ".2010";

        public bool IsReadOnlyFileMode = false;

        public TaeConfigFile Config => Main.Config;

        public bool? LoadCurrentFile()
        {
            IsCurrentlyLoadingGraph = true;
            Scene.DisableModelDrawing();
            Scene.DisableModelDrawing2();

            // Even if it fails to load, just always push it to the recent files list
            PushNewRecentFile(FileContainerName, FileContainerName_Model);

            //string templateName = BrowseForXMLTemplate();

            //if (templateName == null)
            //{
            //    return false;
            //}

            if (System.IO.File.Exists(FileContainerName))
            {
                FileContainer = new TaeFileContainer(this);

                try
                {
                    string folder = new System.IO.FileInfo(FileContainerName).DirectoryName;

                    int lastSlashInFolder = folder.LastIndexOf("\\");

                    string interroot = folder.Substring(0, lastSlashInFolder);

                    string oodleSource = Utils.Frankenpath(interroot, "oo2core_6_win64.dll");

                    // modengine check
                    if (!File.Exists(oodleSource))
                    {
                        oodleSource = Utils.Frankenpath(interroot, @"..\oo2core_6_win64.dll");
                    }

                    //if (!File.Exists(oodleSource))
                    //{
                    //    System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
                    //    "`oo2core_6_win64.dll` file in the Sekiro folder. Please copy that file to the " +
                    //    "'lib' folder next to DS Anim Studio.exe in order to load Sekiro files.", "Unable to find compression DLL",
                    //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    //    return false;
                    //}

                    string oodleTarget = Utils.Frankenpath(Main.Directory, "oo2core_6_win64.dll");

                    if (System.IO.File.Exists(oodleSource) && !System.IO.File.Exists(oodleTarget))
                    {
                        System.IO.File.Copy(oodleSource, oodleTarget, true);

                        NotificationManager.PushNotification("Oodle compression library was automatically copied from game directory " +
                                            "to editor's directory and Sekiro / Elden Ring files will load now.");
                    }

                    
                    FileContainer.LoadFromPath(FileContainerName, null);
                }
                catch (System.DllNotFoundException)
                {
                    //System.Windows.Forms.MessageBox.Show("Cannot open Sekiro files unless you " +
                    //    "copy the `oo2core_6_win64.dll` file from the Sekiro folder into the " +
                    //    "'lib' folder next to DS Anim Studio.exe.", "Additional DLL Required", 
                    //    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);

                    System.Windows.Forms.MessageBox.Show("Was unable to automatically find the " +
                        "`oo2core_6_win64.dll` file in the Sekiro / Elden Ring game folder. Please copy that file to the " +
                        "program folder next to DS Anim Studio.exe in order to load Sekiro / Elden Ring files.", "Unable to find compression DLL",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);

                    Main.REQUEST_REINIT_EDITOR = true;

                    return false;
                }
                

                if (!FileContainer.AllTAE.Any())
                {
                    if (TryAnimLoadFallback())
                    {
                        return LoadCurrentFile();
                    }
                    else
                    {
                        Main.REQUEST_REINIT_EDITOR = true;
                        return false;
                    }
                }

                LoadTaeFileContainer(FileContainer);

                //if (templateName != null)
                //{
                //    LoadTAETemplate(templateName);
                //}

                IsCurrentlyLoadingGraph = false;

                return true;
            }
            else
            {
                return null;
            }
        }

        private void CheckAutoLoadXMLTemplate()
        {
            var objCheck = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName)).ToLower().StartsWith("o") &&
                (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R);
            var remoCheck = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName)).ToLower().StartsWith("scn") &&
                (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1 || GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R);

            //var xmlPath = System.IO.Path.Combine(
            //    new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
            //    $@"Res\TAE.Template.{(FileContainer.IsBloodborne ? "BB" : SelectedTae.Format.ToString())}{(objCheck ? ".OBJ" : "")}.xml");

            string taeFormatStr = $"{GameRoot.GameType}";

            if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.DS1R)
                taeFormatStr = "DS1";

            var xmlPath = System.IO.Path.Combine(
                new System.IO.FileInfo(typeof(TaeEditorScreen).Assembly.Location).DirectoryName,
                $@"Res\TAE.Template.{taeFormatStr}{(remoCheck ? ".REMO" : objCheck ? ".OBJ" : "")}.xml");

            if (System.IO.File.Exists(xmlPath))
                LoadTAETemplate(xmlPath);
        }

        public void SaveCurrentFile(Action afterSaveAction = null, string saveMessage = "Saving ANIBND...")
        {
            if (!IsFileOpen)
                return;

            CommitActiveGraphToTaeStruct();

            GameData.SaveProjectJson();

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
                !System.IO.File.Exists(FileContainerName + BackupExtension))
            {
                System.IO.File.Copy(FileContainerName, FileContainerName + BackupExtension);
                NotificationManager.PushNotification(
                    "A backup was not found and was created:\n" + FileContainerName + BackupExtension);
            }

            LoadingTaskMan.DoLoadingTask("SaveFile", saveMessage, progress =>
            {
                FileContainer.SaveToPath(FileContainerName, progress);

                NotificationManager.PushNotification($"Saved file to '{FileContainerName}'.");

                if (Config.ExtensiveBackupsEnabled)
                {
                    var filesInDir = Directory.GetFiles(Path.GetDirectoryName(FileContainerName));
                    int maxBackupIndex = 0;
                    int maxBackupIndex_2010 = 0;
                    string baseBakPath = FileContainerName + ".dsasautobak";
                    string baseBakPath_2010 = FileContainerName_2010 + ".dsasautobak";
                    foreach (var f in filesInDir)
                    {
                        if (f.StartsWith(baseBakPath) && f.Length > baseBakPath.Length)
                        {
                            if (int.TryParse(f.Substring(baseBakPath.Length), out int backupIndexParsed))
                            {
                                if (backupIndexParsed > maxBackupIndex)
                                    maxBackupIndex = backupIndexParsed;
                            }
                        }
                        else if (f.StartsWith(baseBakPath_2010) && f.Length > baseBakPath_2010.Length)
                        {
                            if (int.TryParse(f.Substring(baseBakPath_2010.Length), out int backupIndexParsed_2010))
                            {
                                if (backupIndexParsed_2010 > maxBackupIndex_2010)
                                    maxBackupIndex_2010 = backupIndexParsed_2010;
                            }
                        }
                    }

                    var md5 = System.Security.Cryptography.MD5.Create();

                    if (!File.Exists($"{baseBakPath}{maxBackupIndex:D4}"))
                    {
                        string bakName = $"{baseBakPath}{(maxBackupIndex + 1):D4}";
                        File.Copy(FileContainerName, bakName);
                        NotificationManager.PushNotification($"Saved file to '{bakName}'.");
                    }
                    else
                    {
                        var hash_bak = md5.ComputeHash(File.ReadAllBytes($"{baseBakPath}{maxBackupIndex:D4}"));
                        var hash = md5.ComputeHash(File.ReadAllBytes(FileContainerName));
                        string bakName = $"{baseBakPath}{(maxBackupIndex + 1):D4}";
                        if (!hash_bak.SequenceEqual(hash))
                        {
                            File.Copy(FileContainerName, bakName);
                            NotificationManager.PushNotification($"Saved file to '{bakName}'.");
                        }
                    }

                    


                }

                if (Config.LiveRefreshOnSave)
                {
                    LiveRefresh();
                }

                progress.Report(1.0);

                afterSaveAction?.Invoke();
            });

            
        }

        private void LoadAnimIntoGraph(TAE.Animation anim)
        {
            //if (!GraphLookup.ContainsKey(anim))
            //{
            //    var graph = new TaeEditAnimEventGraph(this, false, anim);
            //    GraphLookup.Add(anim, graph);

            //}

            //Graph = GraphLookup[anim];

            if (Graph == null)
                Graph = new TaeEditAnimEventGraph(this, false, anim);
            else
                Graph.ChangeToNewAnimRef(anim);
        }

        private void LoadTaeFileContainer(TaeFileContainer fileContainer)
        {
            TaeExtensionMethods.ClearMemes();
            FileContainer = fileContainer;
            SelectedTae = FileContainer.AllTAE.First();
            GameWindowAsForm.Invoke(new Action(() =>
            {
                // Since form close event is hooked this should
                // take care of nulling it out for us.
                FindValueDialog?.Close();
                ComboMenu?.Close();
                ComboMenu = null;
                ExportAllAnimsMenu?.Close();
                ExportAllAnimsMenu = null;
            }));
            SelectedTaeAnim = SelectedTae.Animations[0];
            AnimationListScreen = new TaeEditAnimList(this);

            
            Graph = null;
            LoadAnimIntoGraph(SelectedTaeAnim);
            if (Main.REQUEST_REINIT_EDITOR)
                return;
            IsCurrentlyLoadingGraph = false;

            //if (FileContainer.ContainerType != TaeFileContainer.TaeFileContainerType.TAE)
            //{
            //    TaeInterop.OnLoadANIBND(MenuBar, progress);
            //}
            CheckAutoLoadXMLTemplate();
            SelectNewAnimRef(SelectedTae, SelectedTae.Animations[0]);
            LastFindInfo = null;
        }

        public void RecreateAnimList()
        {
            Vector2 oldScroll = AnimationListScreen.ScrollViewer.Scroll;
            var sectionsCollapsed = AnimationListScreen
                .AnimTaeSections
                .ToDictionary(x => x.Key, x => x.Value.Collapsed);

            AnimationListScreen = new TaeEditAnimList(this);

            foreach (var section in AnimationListScreen.AnimTaeSections)
            {
                if (sectionsCollapsed.ContainsKey(section.Key))
                    section.Value.Collapsed = sectionsCollapsed[section.Key];
            }
            
            AnimationListScreen.ScrollViewer.Scroll = oldScroll;

            HasntSelectedAnAnimYetAfterBuildingAnimList = true;
        }

        public void DuplicateCurrentAnimation()
        {
            TAE.Animation.AnimMiniHeader header = null;



            var newAnimRef = new TAE.Animation(
                SelectedTaeAnim.ID, new TAE.Animation.AnimMiniHeader.Standard(), 
                SelectedTaeAnim.AnimFileName);

            if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.Standard stand)
            {
                var standardHeader = new TAE.Animation.AnimMiniHeader.Standard();

                standardHeader.AllowDelayLoad = false;

                if (stand.ImportHKXSourceAnimID >= 0)
                {
                    if (stand.ImportsHKX)
                    {
                        standardHeader.ImportsHKX = true;
                        
                    }

                    if (stand.AllowDelayLoad)
                    {

                    }
                }


                header = standardHeader;
            }
            else if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim imp)
            {
                header = imp;
            }



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
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Failed to apply TAE template:\n\n{ex}",
                    "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public void CleanupForReinit()
        {
            GameWindowAsForm.FormClosing -= GameWindowAsForm_FormClosing;
        }

        public TaeEditorScreen(System.Windows.Forms.Form gameWindowAsForm, Rectangle rect)
        {
            Rect = rect;
            Main.LoadConfig();

            if (Main.Config.LayoutAnimListWidth > 0)
                LeftSectionWidth = Main.Config.LayoutAnimListWidth;

            if (Main.Config.LayoutViewportWidth > 0)
                RightSectionWidth = Main.Config.LayoutViewportWidth;

            if (Main.Config.LayoutViewportHeight > 0)
                TopRightPaneHeight = Main.Config.LayoutViewportHeight;

            gameWindowAsForm.FormClosing += GameWindowAsForm_FormClosing;

            GameWindowAsForm = gameWindowAsForm;

            GameWindowAsForm.MinimumSize = new System.Drawing.Size(1280  - 64, 720 - 64);

            Transport = new TaeTransport(this);

            UpdateLayout();
        }

        public void SetAllTAESectionsCollapsed(bool collapsed)
        {
            foreach (var kvp in AnimationListScreen.AnimTaeSections.Values)
            {
                kvp.Collapsed = collapsed;
            }
        }

        public void LoadContent(ContentManager c)
        {
            Transport.LoadContent(c);
        }

        public void LiveRefresh()
        {
            var chrNameBase = Utils.GetFileNameWithoutAnyExtensions(Utils.GetFileNameWithoutDirectoryOrExtension(FileContainerName)).ToLower();

            if (chrNameBase.StartsWith("c"))
            {
                if (DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                    DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Chr, chrNameBase))
                    NotificationManager.PushNotification($"Requested game to reload character '{chrNameBase}'.");

            }
            else if (chrNameBase.StartsWith("o"))
            {
                if (DSAnimStudio.LiveRefresh.RequestFileReload.RequestReload(
                    DSAnimStudio.LiveRefresh.RequestFileReload.ReloadType.Object, chrNameBase))
                    NotificationManager.PushNotification($"Requested game to reload object '{chrNameBase}'.");
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

        public bool DoesAnimIDExist(int id)
        {
            foreach (var s in AnimationListScreen.AnimTaeSections.Values)
            {
                var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == id);
                if (matchedAnims.Any())
                {
                    return true;
                }
            }
            return false;
        }

        public bool GotoAnimSectionID(int id, bool scrollOnCenter)
        {
            if (FileContainer.AllTAEDict.Count > 1)
            {
                if (AnimationListScreen.AnimTaeSections.ContainsKey(id))
                {
                    var firstAnimInSection = AnimationListScreen.AnimTaeSections[id].Tae.Animations.FirstOrDefault();
                    if (firstAnimInSection != null)
                    {
                        SelectNewAnimRef(AnimationListScreen.AnimTaeSections[id].Tae, firstAnimInSection, scrollOnCenter);
                        return true;
                    }
                }
            }
            else
            {
                foreach (var anim in SelectedTae.Animations)
                {
                    long sectionOfAnim = GameRoot.GameTypeHasLongAnimIDs ? (anim.ID / 1_000000) : (anim.ID / 1_0000);
                    if (sectionOfAnim == id)
                    {
                        SelectNewAnimRef(SelectedTae, anim, scrollOnCenter);
                        return true;
                    }
                }
            }

            return false;
        }

        public TAE.Animation SelectAnimByFullID(long fullID)
        {
            foreach (var s in AnimationListScreen.AnimTaeSections.Values)
            {
                var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == fullID);
                if (matchedAnims.Any())
                {
                    var anim = matchedAnims.First().Value.Ref;
                    return anim;
                }
            }
            return null;
        }

        public TaeEditAnimList.TaeEditAnimInfo GetAnimListInfoOfAnim(TAE.Animation anim)
        {
            foreach (var s in AnimationListScreen.AnimTaeSections.Values)
            {
                if (s.InfoMap.ContainsKey(anim))
                    return s.InfoMap[anim];
            }
            return null;
        }

        public TAE.Animation CreateNewAnimWithFullID(long fullID, string animName, bool nukeExisting)
        {
            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.PC)
            {
                var split = GameRoot.SplitAnimID.FromFullID((int)fullID);
                var tae = FileContainer.GetTAE(split.TaeID);

                if (nukeExisting)
                {
                    var existing = new List<TAE.Animation>();
                    foreach (var a in tae.Animations)
                    {
                        if (a.ID == split.SubID)
                            existing.Add(a);
                    }
                    foreach (var a in existing)
                    {
                        tae.Animations.Remove(a);
                    }
                }

                var anim = new TAE.Animation(split.SubID, new TAE.Animation.AnimMiniHeader.Standard(), animName);
                tae.Animations.Add(anim);
                tae.Animations = tae.Animations.OrderBy(x => x.ID).ToList();
                return anim;
            }
            else
            {
                var anim = new TAE.Animation(fullID, new TAE.Animation.AnimMiniHeader.Standard(), animName);
                SelectedTae.Animations.Add(anim);
                SelectedTae.Animations = SelectedTae.Animations.OrderBy(x => x.ID).ToList();
                return anim;
            }
        }

        public TAE.Animation GetAnimRefFromID(long id)
        {
            foreach (var s in AnimationListScreen.AnimTaeSections.Values)
            {
                var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == id);
                if (matchedAnims.Any())
                {
                    return matchedAnims.First().Value.Ref;
                }
            }
            return null;
        }

        public bool GotoAnimID(long id, bool scrollOnCenter, bool ignoreIfAlreadySelected, out TAE.Animation foundAnimRef, float startFrameOverride = -1)
        {
            foreach (var s in AnimationListScreen.AnimTaeSections.Values)
            {
                var matchedAnims = s.InfoMap.Where(x => x.Value.FullID == id);
                if (matchedAnims.Any())
                {
                    var anim = matchedAnims.First().Value.Ref;
                    foundAnimRef = anim;
                    if (!ignoreIfAlreadySelected || anim != SelectedTaeAnim)
                        SelectNewAnimRef(s.Tae, anim, scrollOnCenter, doNotCommitToGraph: false ,startFrameOverride);
                    return true;
                }
            }



            foundAnimRef = null;
            return false;
        }

        public void ShowDialogEditCurrentAnimInfo()
        {
            DialogManager.ShowTaeAnimPropertiesEditor(SelectedTaeAnim);

            //Task.Run(new Action(() =>
            //{
            //    PauseUpdate = true;
            //    var editForm = new TaeEditAnimPropertiesForm(SelectedTaeAnim, FileContainer.AllTAE.Count() == 1);
            //    editForm.Owner = GameWindowAsForm;
            //    editForm.ShowDialog();

            //    if (editForm.WasAnimDeleted)
            //    {
            //        if (SelectedTae.Animations.Count <= 1)
            //        {
            //            System.Windows.Forms.MessageBox.Show(
            //                "Cannot delete the only animation remaining in the TAE.",
            //                "Can't Delete Last Animation",
            //                System.Windows.Forms.MessageBoxButtons.OK,
            //                System.Windows.Forms.MessageBoxIcon.Stop);
            //        }
            //        else
            //        {
            //            var indexOfCurrentAnim = SelectedTae.Animations.IndexOf(SelectedTaeAnim);
            //            SelectedTae.Animations.Remove(SelectedTaeAnim);
            //            RecreateAnimList();

            //            if (indexOfCurrentAnim > SelectedTae.Animations.Count - 1)
            //                indexOfCurrentAnim = SelectedTae.Animations.Count - 1;

            //            if (indexOfCurrentAnim >= 0)
            //                SelectNewAnimRef(SelectedTae, SelectedTae.Animations[indexOfCurrentAnim]);
            //            else
            //                SelectNewAnimRef(SelectedTae, SelectedTae.Animations[0]);

            //            SelectedTae.SetIsModified(!IsReadOnlyFileMode);
            //        }
            //    }
            //    else
            //    {
            //        bool needsAnimReload = false;
            //        if (editForm.WasAnimIDChanged)
            //        {
            //            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
            //            SelectedTae.SetIsModified(!IsReadOnlyFileMode);
            //            RecreateAnimList();
            //            UpdateSelectedTaeAnimInfoText();
            //            Graph.InitGhostEventBoxes();
            //            needsAnimReload = true;
            //        }

            //        if (editForm.WereThingsChanged)
            //        {
            //            SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
            //            SelectedTae.SetIsModified(!IsReadOnlyFileMode);
            //            UpdateSelectedTaeAnimInfoText();
            //            Graph.InitGhostEventBoxes();
            //            needsAnimReload = true;
            //        }

            //        if (needsAnimReload)
            //            Graph.ViewportInteractor.OnNewAnimSelected();

            //    }

            //    PauseUpdate = false;
            //}));

            
        }

        private void GameWindowAsForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            Main.SaveConfig();

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

            if (!e.Cancel)
            {
                Main.OnClosing();
            }
        }

        private void WinFormsMenuStrip_MenuDeactivate(object sender, EventArgs e)
        {
            //PauseUpdate = false;
        }

        private void WinFormsMenuStrip_MenuActivate(object sender, EventArgs e)
        {
            PauseUpdate = true;
            Input.CursorType = MouseCursorType.Arrow;
        }

        public void DirectOpenFile(string anibndFileName, string chrbndFileName)
        {
            
            void DoActualFileOpen()
            {
                LoadingTaskMan.DoLoadingTask("DirectOpenFile", "Loading game assets...", progress =>
                {
                    string oldFileContainerName = FileContainerName.ToString();
                    var oldFileContainer = FileContainer;
                    string oldFileContainerName_Model = FileContainerName_Model;

                    FileContainerName = anibndFileName;
                    FileContainerName_Model = chrbndFileName;
                    bool? loadFileResult = null;

                    try
                    {
                        loadFileResult = LoadCurrentFile();
                    }
                    catch (System.Threading.ThreadInterruptedException)
                    {
                        Main.REQUEST_REINIT_EDITOR = true;
                        return;
                    }
                    if (loadFileResult == false)
                    {
                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        FileContainerName_Model = oldFileContainerName_Model;
                        return;
                    }
                    else if (loadFileResult == null)
                    {
                        lock (Main.Config._lock_ThreadSensitiveStuff)
                        {
                            if (Config.RecentFilesList.Any(f => f.TaeFile == anibndFileName))
                            {
                                var ask = System.Windows.Forms.MessageBox.Show(
                                    $"File '{anibndFileName}' no longer exists. Would you like to " +
                                    $"remove it from the recent files list?",
                                    "File Does Not Exist",
                                    System.Windows.Forms.MessageBoxButtons.YesNo,
                                    System.Windows.Forms.MessageBoxIcon.Warning)
                                        == System.Windows.Forms.DialogResult.Yes;

                                if (ask)
                                {
                                    if (Config.RecentFilesList.Any(f => f.TaeFile == anibndFileName))
                                        Config.RecentFilesList.RemoveAll(f => f.TaeFile == anibndFileName);
                                }
                            }
                        }

                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        FileContainerName_Model = oldFileContainerName_Model;
                        return;
                    }

                    if (!FileContainer.AllTAE.Any())
                    {
                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        FileContainerName_Model = oldFileContainerName_Model;
                        System.Windows.Forms.MessageBox.Show(
                            "Selected binder had no TAE files in it. If this is an enemy with multiple variants, you must select the shared base ANIBND file with the TAE in it." +
                            "Cancelling load operation.", "Invalid File",
                            System.Windows.Forms.MessageBoxButtons.OK,
                            System.Windows.Forms.MessageBoxIcon.Stop);
                    }
                    else if (loadFileResult == null)
                    {
                        FileContainerName = oldFileContainerName;
                        FileContainer = oldFileContainer;
                        FileContainerName_Model = oldFileContainerName_Model;
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
                        try
                        {
                            DoActualFileOpen();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Forms.MessageBox.Show(ex.ToString(),
                                "Error While Loading File",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
                        }
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

            try
            {
                DoActualFileOpen();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), 
                    "Error While Loading File", 
                    System.Windows.Forms.MessageBoxButtons.OK, 
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
                
            
        }

        public void OpenFromPackedGameArchives()
        {
            Task.Run(() => { }).ContinueWith((task) =>
            {
                var thing = new TaeLoadFromArchivesWizard();
                thing.StartInCenterOf(Main.WinForm);
                thing.ShowDialog();
            }, TaskScheduler.FromCurrentSynchronizationContext());


            //Task.Run(() => new Task(() =>
            //{
            //    var thing = new TaeLoadFromArchivesWizard();
            //    thing.ShowDialog();
            //}), TaskScheduler.FromCurrentSynchronizationContext());
            
        }

        private bool TryAnimLoadFallback()
        {
            string possibleFallbackPath = FileContainerName.ToLower().EndsWith(".dcx") ? (FileContainerName.Substring(0, FileContainerName.Length - "0.anibnd.dcx".Length) + "0.anibnd.dcx")
                        : (FileContainerName.Substring(0, FileContainerName.Length - "0.anibnd".Length) + "0.anibnd");

            var anibndDirectory = Path.GetDirectoryName(FileContainerName);
            var anibndShort = Utils.GetShortIngameFileName(FileContainerName);
            var anibndPathJustFile = Path.GetFileName(FileContainerName);
            var baseShort = Utils.GetShortIngameFileName(possibleFallbackPath);
            var basePathJustFile = Path.GetFileName(possibleFallbackPath);

            // TODO: Check EBLs for base anibnd and ask "would you like to unpack the found base ANIBND and open it for editing?"

            if (anibndShort.StartsWith("c0000_") && File.Exists($"{anibndDirectory}\\c0000.anibnd.dcx")
                && TaeFileContainer.AnibndContainsTae($"{anibndDirectory}\\c0000.anibnd.dcx"))
            {
                var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                    $"It's a supplemental file which the base anibnd, 'c0000.anibnd.dcx', pulls animations from.\n" +
                    $"Would you like to load that base anibnd for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (askResult == System.Windows.Forms.DialogResult.Yes)
                {
                    FileContainerName = $"{anibndDirectory}\\c0000.anibnd.dcx";
                    return true;
                }
                else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;
                }
            }
            else if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER &&
                System.Text.RegularExpressions.Regex.IsMatch(anibndPathJustFile, @"c\d\d\d\d_div\d\d.anibnd.dcx$")
                && File.Exists($"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx")
                && TaeFileContainer.AnibndContainsTae($"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx"))
            {
                var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                    $"It's a supplemental file which the base anibnd, '{anibndShort.Substring(0, 5)}.anibnd.dcx', pulls animations from.\n" +
                    $"Would you like to load that base anibnd for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (askResult == System.Windows.Forms.DialogResult.Yes)
                {
                    FileContainerName = $"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx";
                    return true;
                }
                else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;
                }
            }

            else if (File.Exists(possibleFallbackPath) && TaeFileContainer.AnibndContainsTae(possibleFallbackPath))
            {
                var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it. It appears to read from the base anibnd, '{basePathJustFile}', " +
                    $"which has TimeAct data. Would you like to load the base anibnd instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                if (askResult == System.Windows.Forms.DialogResult.Yes)
                {
                    FileContainerName = possibleFallbackPath;
                }
                else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;
                }
            }
            else
            {
                var gameDataAnibndPath = GameData.FindFileInGameData(FileContainerName);
                string possibleGameDataFallbackPath = (gameDataAnibndPath.Substring(0, gameDataAnibndPath.Length - "0.anibnd.dcx".Length) + "0.anibnd.dcx");
                string possibleGameDataFallbackPathJustFile = Path.GetFileName(possibleGameDataFallbackPath);
                if (gameDataAnibndPath != null)
                {
                    if (anibndShort.StartsWith("c0000_") && GameData.FileExists("chr/c0000.anibnd.dcx")
                        && TaeFileContainer.AnibndContainsTae(GameData.ReadFile("chr/c0000.anibnd.dcx")))
                    {
                        var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                        $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                        $"It's a supplemental file which the base anibnd, 'c0000.anibnd.dcx', pulls animations from.\n" +
                        $"Would you like to unpack that base anibnd into the project folder and open it for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        if (askResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            FileContainerName = $"{anibndDirectory}\\c0000.anibnd.dcx";
                            File.WriteAllBytes(FileContainerName, GameData.ReadFile("chr/c0000.anibnd.dcx"));
                            return true;
                        }
                        else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return false;
                        }
                    }
                    else if (GameRoot.GameType is SoulsAssetPipeline.SoulsGames.ER &&
                        System.Text.RegularExpressions.Regex.IsMatch(anibndPathJustFile, @"c\d\d\d\d_div\d\d.anibnd.dcx$")
                        && GameData.FileExists($"chr/{anibndShort.Substring(0, 5)}.anibnd.dcx")
                        && TaeFileContainer.AnibndContainsTae(GameData.ReadFile($"chr/{anibndShort.Substring(0, 5)}.anibnd.dcx")))
                    {
                        var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                            $"have any TimeAct (.TAE) data inside of it, just animations.\n" +
                            $"It's a supplemental file which the base anibnd, '{anibndShort.Substring(0, 5)}.anibnd.dcx', pulls animations from.\n" +
                            $"Would you like to unpack that base anibnd into the project folder and open it for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        if (askResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            FileContainerName = $"{anibndDirectory}\\{anibndShort.Substring(0, 5)}.anibnd.dcx";
                            File.WriteAllBytes(FileContainerName, GameData.ReadFile($"chr/{anibndShort.Substring(0, 5)}.anibnd.dcx"));
                            return true;
                        }
                        else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return false;
                        }
                    }
                    else if (GameData.FileExists(possibleGameDataFallbackPath) && TaeFileContainer.AnibndContainsTae(GameData.ReadFile(possibleGameDataFallbackPath)))
                    {
                        var askResult = System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                            $"have any TimeAct (.TAE) data inside of it. It appears to read from the base anibnd, '{possibleGameDataFallbackPathJustFile}', " +
                            $"which has TimeAct data. Would you like to unpack that base anibnd into the project folder and open it for editing instead?", "Load Base ANIBND?", System.Windows.Forms.MessageBoxButtons.YesNoCancel);
                        if (askResult == System.Windows.Forms.DialogResult.Yes)
                        {
                            FileContainerName = possibleFallbackPath;
                            File.WriteAllBytes(FileContainerName, GameData.ReadFile(possibleGameDataFallbackPath));
                            return true;
                        }
                        else if (askResult == System.Windows.Forms.DialogResult.Cancel)
                        {
                            return false;
                        }
                    }



                }

                System.Windows.Forms.MessageBox.Show($"The selected anibnd, '{anibndPathJustFile}', does not " +
                    $"have any TimeAct (.TAE) data inside of it. Unable to find any base ANIBND that the game reads TimeAct " +
                    $"data from in combination with this file. Aborting load operation.", "Invalid ANIBND", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        public void File_Open()
        {
            SoundManager.StopAllSounds();
            RumbleCamManager.ClearActive();
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
                FileContainerName_Model = null;

                if (!TaeFileContainer.AnibndContainsTae(FileContainerName))
                {
                    if (GameRoot.GameType == SoulsAssetPipeline.SoulsGames.None)
                    {
                        GameRoot.InitializeFromBND(FileContainerName);
                    }
                    if (!TryAnimLoadFallback())
                    {
                        Main.REQUEST_REINIT_EDITOR = true;
                        return;
                    }
                }

                bool isCancel = false;

                

                if (!isCancel)
                {
                    LoadingTaskMan.DoLoadingTask("File_Open", "Loading game assets...", progress =>
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

        public void BrowseForMoreTextures()
        {
            List<string> texturesToLoad = new List<string>();
            lock (Scene._lock_ModelLoad_Draw)
            {
                foreach (var m in Scene.Models)
                {
                    texturesToLoad.AddRange(m.MainMesh.GetAllTexNamesToLoad());
                }
            }


            var browseDlg = new System.Windows.Forms.OpenFileDialog()
            {
                //Filter = "TPF[.DCX]/TFPBHD|*.TPF*",
                Multiselect = true,
                Title = "Load textures from CHRBND(s), TEXBND(s), TPF(s), and/or TPFBHD(s)..."
            };
            if (browseDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var texFileNames = browseDlg.FileNames;
                List<string> bxfDupeCheck = new List<string>();
                LoadingTaskMan.DoLoadingTask("BrowseForEntityTextures", "Scanning files for relevant textures...", progress =>
                {
                    double i = 0;
                    foreach (var tfn in texFileNames)
                    {
                        var shortName = Utils.GetShortIngameFileName(tfn);
                        if (!bxfDupeCheck.Contains(shortName))
                        {
                            if (TPF.Is(tfn))
                            {
                                TexturePool.AddTpfFromPath(tfn);
                            }
                            else
                            {
                                TexturePool.AddSpecificTexturesFromBinder(tfn, texturesToLoad);
                            }

                            bxfDupeCheck.Add(shortName);
                        }
                        progress.Report(++i / texFileNames.Length);
                    }
                    Scene.RequestTextureLoad();
                    progress.Report(1);
                });


            }


        }


        public void File_SaveAs()
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

        public void ChangeTypeOfSelectedEvent()
        {
            if (SelectedEventBox == null)
                return;

            Task.Run(() =>
            {
                PauseUpdate = true;

                var changeTypeDlg = new TaeInspectorFormChangeEventType();
                changeTypeDlg.TAEReference = SelectedTae;
                changeTypeDlg.CurrentTemplate = SelectedEventBox.MyEvent.Template;
                changeTypeDlg.NewEventType = SelectedEventBox.MyEvent.Type;
                var dialogResult = System.Windows.Forms.DialogResult.Cancel;
                GameWindowAsForm.Invoke(new Action(() =>
                {
                    dialogResult = changeTypeDlg.ShowDialog(GameWindowAsForm);
                }));
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
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
                                    SelectedTae.BankTemplate[changeTypeDlg.NewEventType]), SelectedTaeAnim);

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
                                referenceToEventBox.ChangeEvent(referenceToPreviousEvent, SelectedTaeAnim);
                                SelectedTaeAnim.Events.Insert(index, referenceToPreviousEvent);

                                SelectedEventBox = null;
                                SelectedEventBox = referenceToEventBox;

                                SelectedEventBox.Row = row;

                                Graph.RegisterEventBoxExistance(SelectedEventBox);

                                SelectedTaeAnim.SetIsModified(!IsReadOnlyFileMode);
                                SelectedTae.SetIsModified(!IsReadOnlyFileMode);
                            },
                            new List<ITaeClonable>
                            {

                            });
                    }
                }


                PauseUpdate = false;
            });

            
        }

        private (long Upper, long Lower) GetSplitAnimID(long id)
        {
            return ((GameRoot.GameTypeHasLongAnimIDs) 
                ? (id / 1000000) : (id / 10000),
                (GameRoot.GameTypeHasLongAnimIDs) 
                ? (id % 1000000) : (id % 10000));
        }

        private string HKXNameFromCompositeID(long compositeID)
        {
            if (compositeID < 0)
                return "<NONE>";

            var splitID = GetSplitAnimID(compositeID);

            if (GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXXX_YYYYYY)
            {
                return $"a{splitID.Upper:D3}_{splitID.Lower:D6}";
            }
            else if (GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXX_YY_ZZZZ)
            {
                string s = $"a{splitID.Upper:D3}_{splitID.Lower:D6}";
                return s.Insert(s.Length - 4, "_");
            }
            else
            {
                return $"a{splitID.Upper:D2}_{splitID.Lower:D4}";
            }
        }

        private string HKXSubIDDispNameFromInt(long subID)
        {
            if (FileContainer.AllTAE.Count() == 1)
            {
                return HKXNameFromCompositeID(subID);
            }
            else
            {
                if (GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXX_YY_ZZZZ)
                {
                    return $"aXXX_{subID:D6}";
                }
                else if (GameRoot.CurrentAnimIDFormatType == GameRoot.AnimIDFormattingType.aXX_YY_ZZZZ)
                {
                    string s = $"aXX_{subID:D6}";
                    return s.Insert(s.Length - 4, "_");
                }
                else
                {
                    return $"aXX_{subID:D4}";
                }
            }
            
        }

        public void UpdateSelectedTaeAnimInfoText()
        {
            var stringBuilder = new StringBuilder();

            if (SelectedTaeAnim == null)
            {
                stringBuilder.Append("(No Animation Selected)");
                TaeAnimInfoIsClone = false;
            }
            else
            {
                stringBuilder.Append($"{HKXSubIDDispNameFromInt(SelectedTaeAnim.ID)}");

                if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
                {
                    stringBuilder.Append($" [Original Anim Entry]");

                    if (asStandard.ImportsHKX && asStandard.ImportHKXSourceAnimID >= 0)
                        stringBuilder.Append($", Override HKX: {HKXNameFromCompositeID(asStandard.ImportHKXSourceAnimID)}.hkx");

                    if (asStandard.AllowDelayLoad)
                        stringBuilder.Append(", DelayLoad Allowed");

                    if (asStandard.IsLoopByDefault)
                        stringBuilder.Append($", Loops By Default");

                    TaeAnimInfoIsClone = false;
                }
                else if (SelectedTaeAnim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOtherAnim)
                {
                    stringBuilder.Append($" [Clone Anim Entry]");

                    if (asImportOtherAnim.ImportFromAnimID >= 0)
                        stringBuilder.Append($", Clone ID: {HKXNameFromCompositeID(asImportOtherAnim.ImportFromAnimID)}");
                    
                    stringBuilder.Append($", Unk. Value: {asImportOtherAnim.Unknown}");

                    TaeAnimInfoIsClone = true;
                }

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

            Graph.ScrollViewer.Scroll.X = box.Left - (Graph.ScrollViewer.Viewport.Width / 2f);
            Graph.ScrollViewer.Scroll.Y = (box.Row * Graph.RowHeight) - (Graph.ScrollViewer.Viewport.Height / 2f);
            Graph.ScrollViewer.ClampScroll();
        }

        public void StripExtraEventGroupsInAllLoadedFilesIfNeeded()
        {
            bool wasStripped = false;
            if (GameRoot.GameTypeUsesLegacyEmptyEventGroups && !Main.Config.SaveAdditionalEventRowInfoToLegacyGames)
            {

                foreach (var tae in FileContainer.AllTAEDict)
                {
                    foreach (var anim in tae.Value.Animations)
                    {
                        foreach (var ev in anim.Events)
                        {
                            if (ev.Group != null)
                            {
                                ev.Group = null;
                                anim.SetIsModified(true);
                                tae.Value.SetIsModified(true);
                                wasStripped = true;
                            }
                        }
                        anim.EventGroups.Clear();
                    }
                }

            }
           
            if (wasStripped)
            {
                DialogManager.DialogOK("Notice", "The Save Row Data To Legacy Games option is disabled " +
                    "\nbut the file had row data in it from when the option was enabled previously. All row " +
                    "\ndata has been stripped from the file as if the option has never been used before " +
                    "\nand it will permanently save without this data present. If this is not desired, " +
                    "\nthen re-enable the Save Row Data To Legacy Games option and RELOAD the current " +
                    "\nanibnd WITHOUT SAVING, because saving will permently save the changes into the file.");
            }
        }

        public void CommitActiveGraphToTaeStruct()
        {
            Graph.EventBoxes = Graph.EventBoxes.OrderBy(evBox => evBox.MyEvent.StartTime + (evBox.Row * 1000)).ToList();

            SelectedTaeAnim.Events = Graph.EventBoxes
                .Select(evBox => evBox.MyEvent)
                .ToList();

            Graph?.GenerateFakeDS3EventGroups(threadLock: true);
        }

        public void SelectNewAnimRef(TAE tae, TAE.Animation animRef, bool scrollOnCenter = false, bool doNotCommitToGraph = false, float startFrameOverride = -1)
        {

            StripExtraEventGroupsInAllLoadedFilesIfNeeded();

            if (!doNotCommitToGraph)
                CommitActiveGraphToTaeStruct();

            Graph?.ViewportInteractor?.CurrentModel?.AnimContainer?.MarkAllAnimsReferenceBlendWeights();

            bool isBlend = (PlaybackCursor.IsPlaying || Graph.ViewportInteractor.IsComboRecording) && 
                Graph.ViewportInteractor.IsBlendingActive &&
                Graph.ViewportInteractor.EntityType != TaeViewportInteractor.TaeEntityType.REMO;

            if (Graph.ViewportInteractor.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
            {
                PlaybackCursor.CurrentTime = 0;
            }

            AnimSwitchRenderCooldown = AnimSwitchRenderCooldownMax;

            PlaybackCursor.IsStepping = false;

            SelectedTae = tae;

            SelectedTaeAnim = animRef;

            UpdateSelectedTaeAnimInfoText();

            if (SelectedTaeAnim != null)
            {
                SelectedEventBox = null;

                //bool wasFirstAnimSelected = false;

                //if (Graph == null)
                //{
                //    wasFirstAnimSelected = true;
                //    Graph = new TaeEditAnimEventGraph(this, false, SelectedTaeAnim);
                //}



                LoadAnimIntoGraph(SelectedTaeAnim);

                if (HasntSelectedAnAnimYetAfterBuildingAnimList)
                {
                    UpdateLayout(); // Fixes scroll when you first open anibnd and when you rebuild anim list.
                    HasntSelectedAnAnimYetAfterBuildingAnimList = false;
                }

                AnimationListScreen.ScrollToAnimRef(SelectedTaeAnim, scrollOnCenter);

                Graph.PlaybackCursor.ResetAll();

                if (startFrameOverride >= 0)
                {
                     float overrideStartTime = (float)(startFrameOverride * PlaybackCursor.CurrentSnapInterval);

                    Graph.ViewportInteractor.OnNewAnimSelected(overrideStartTime);

                    PlaybackCursor.CurrentTime = PlaybackCursor.StartTime = overrideStartTime;
                    PlaybackCursor.IgnoreCurrentRelativeScrub();
                    //Graph.ViewportInteractor.OnScrubFrameChange(overrideStartTime, doNotScrubBackgroundLayers: true);
                    Graph.ViewportInteractor.OnScrubFrameChange(0);
                }
                else
                {
                    Graph.ViewportInteractor.OnNewAnimSelected(0);
                    Graph.PlaybackCursor.RestartFromBeginning();
                    Graph.ViewportInteractor.OnScrubFrameChange(0);

                    if (!isBlend)
                    {
                        Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
                        Graph.ViewportInteractor.RootMotionSendHome();
                        Graph.ViewportInteractor.ResetRootMotion();
                        Graph.ViewportInteractor.RemoveTransition();
                        Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
                    }
                }

                Scene.UpdateAnimation();
            }
            else
            {
                SelectedEventBox = null;

                Graph = null;
            }
        }

        public void ShowDialogChangeAnimName()
        {
            if (SelectedTaeAnim != null)
            {
                DialogManager.AskForInputString("Set Animation Name", "Set the name of the current animation.", "", result =>
                {
                    if (SelectedTaeAnim.AnimFileName != result)
                    {
                        SelectedTaeAnim.AnimFileName = result;
                        SelectedTaeAnim.SetIsModified(true);
                    }
                }, canBeCancelled: true, startingText: SelectedTaeAnim.AnimFileName);
            }
        }

#if NIGHTFALL
        public void NIGHTFALL_ToggleImport()
        {
            var s = SelectedTaeAnim.AnimFileName;
            if (s.StartsWith("+"))
            {
                while (s.StartsWith("+"))
                    s = s.TrimStart('+');
            }
            else
            {
                s = "+" + s;
            }
            SelectedTaeAnim.AnimFileName = s;
            SelectedTaeAnim.SetIsModified(true);
        }
#endif

        public void ShowDialogFind()
        {
            if (FileContainerName == null || SelectedTae == null)
                return;
            //PauseUpdate = true;

            if (FindValueDialog == null)
            {
                FindValueDialog = new TaeFindValueDialog();
                FindValueDialog.LastFindInfo = LastFindInfo;
                FindValueDialog.EditorRef = this;
                FindValueDialog.Owner = GameWindowAsForm;
                FindValueDialog.Show();
                Main.CenterForm(FindValueDialog);
                FindValueDialog.FormClosed += FindValueDialog_FormClosed;
            }

            

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


            
            //PauseUpdate = false;
        }

        private void FindValueDialog_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            FindValueDialog.FormClosed -= FindValueDialog_FormClosed;
            FindValueDialog = null;
        }

        public void ShowDialogGotoAnimSectionID()
        {
            if (FileContainer == null || SelectedTae == null)
                return;

            DialogManager.AskForInputString("Go To Animation Section ID", $"Enter the animation section number (The X part of {GameRoot.CurrentAnimIDFormatType})\n" +
                "to jump to the first animation in that section.",
                $"", result =>
                {
                    Main.WinForm.Invoke(new Action(() =>
                    {
                        if (int.TryParse(result.Replace("a", "").Replace("_", ""), out int id))
                        {
                            if (!GotoAnimSectionID(id, scrollOnCenter: true))
                            {
                                DialogManager.DialogOK("Goto Failed", $"Unable to find anim section {id}.");
                            }
                        }
                        else
                        {
                            DialogManager.DialogOK("Goto Failed", $"\"{result}\" is not a valid integer.");
                        }
                    }));
                    
                    
                }, canBeCancelled: true);
        }

        public void ShowDialogGotoAnimID()
        {
            if (FileContainer == null || SelectedTae == null)
                return;

            DialogManager.AskForInputString("Go To Animation ID", "Enter the animation ID to jump to.\n" +
                "Accepts the full string with prefix or just the ID as a number.",
                GameRoot.CurrentAnimIDFormatType.ToString(), result =>
                {
                    Main.WinForm.Invoke(new Action(() =>
                    {
                        if (int.TryParse(result.Replace("a", "").Replace("_", ""), out int id))
                        {
                            if (!GotoAnimID(id, scrollOnCenter: true, ignoreIfAlreadySelected: false, out _))
                            {
                                NotificationManager.PushNotification($"Unable to find animation with ID {id}.");
                            }
                        }
                        else
                        {
                            NotificationManager.PushNotification($"\"{result}\" is not a valid animation ID.");
                        }
                    }));
                }, canBeCancelled: true);
        }

        public bool ImmediateImportAnim(TAE.Animation importToAnim, int importFromID)
        {
            bool isInvalidHKX = false;
            var animRefToImportFrom = SelectAnimByFullID(importFromID);

            void DoAnimRefThing(TAE.Animation anim, int animID)
            {

                if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.Standard asStandard)
                {
                    var header = new TAE.Animation.AnimMiniHeader.Standard();
                    header.ImportsHKX = true;

                    if (asStandard.ImportsHKX)
                    {
                        header.ImportHKXSourceAnimID = asStandard.ImportHKXSourceAnimID;
                        if (header.ImportHKXSourceAnimID == animID) // somehow lol
                        {
                            header.ImportHKXSourceAnimID = -1;
                            header.ImportsHKX = false;
                        }
                    }
                    else
                    {
                        header.ImportHKXSourceAnimID = animID;
                    }

                    importToAnim.Events.Clear();
                    importToAnim.EventGroups.Clear();
                    // { origEventGroup, newEventGroup }
                    var eventGroupMigrationMapping = new Dictionary<TAE.EventGroup, TAE.EventGroup>();

                    foreach (var ev in anim.Events)
                        if (ev.Group != null && !eventGroupMigrationMapping.ContainsKey(ev.Group))
                            eventGroupMigrationMapping.Add(ev.Group, ev.Group.GetClone());

                    foreach (var evg in anim.EventGroups)
                        if (!eventGroupMigrationMapping.ContainsKey(evg))
                            eventGroupMigrationMapping.Add(evg, evg.GetClone());

                    foreach (var ev in anim.Events)
                    {
                        var newEv = ev.GetClone(GameRoot.IsBigEndianGame);
                        newEv.Group = ev.Group;
                        if (ev.Group != null && eventGroupMigrationMapping.ContainsKey(ev.Group))
                        {
                            newEv.Group = eventGroupMigrationMapping[ev.Group];
                            if (!importToAnim.EventGroups.Contains(newEv.Group))
                                importToAnim.EventGroups.Add(newEv.Group);
                        }
                        
                        importToAnim.Events.Add(newEv);
                    }
                    
                    importToAnim.MiniHeader = header;
                    importToAnim.SetIsModified(true);
                    
                }
                else if (anim.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim asImportOther)
                {
                    if (asImportOther.ImportFromAnimID >= 0)
                    {
                        var referencedAnim = SelectAnimByFullID(asImportOther.ImportFromAnimID);

                        if (referencedAnim != null)
                        {
                            DoAnimRefThing(referencedAnim, asImportOther.ImportFromAnimID);
                        }
                        else
                        {

                            // BROKEN REFERENCE, GARBEGE

                            var header = new TAE.Animation.AnimMiniHeader.Standard();
                            header.ImportsHKX = false;
                            header.ImportHKXSourceAnimID = -1;



                            importToAnim.Events.Clear();
                            importToAnim.EventGroups.Clear();

                            // { origEventGroup, newEventGroup }
                            var eventGroupMigrationMapping = new Dictionary<TAE.EventGroup, TAE.EventGroup>();

                            foreach (var ev in anim.Events)
                                if (ev.Group != null && !eventGroupMigrationMapping.ContainsKey(ev.Group))
                                    eventGroupMigrationMapping.Add(ev.Group, ev.Group.GetClone());

                            foreach (var evg in anim.EventGroups)
                                if (!eventGroupMigrationMapping.ContainsKey(evg))
                                    eventGroupMigrationMapping.Add(evg, evg.GetClone());

                            foreach (var ev in anim.Events)
                            {
                                var newEv = ev.GetClone(GameRoot.IsBigEndianGame);

                                newEv.Group = ev.Group;
                                if (ev.Group != null && eventGroupMigrationMapping.ContainsKey(ev.Group))
                                {
                                    newEv.Group = eventGroupMigrationMapping[ev.Group];
                                    if (!importToAnim.EventGroups.Contains(newEv.Group))
                                        importToAnim.EventGroups.Add(newEv.Group);
                                }

                                importToAnim.Events.Add(newEv);
                            }
                            foreach (var evg in anim.EventGroups)
                                importToAnim.EventGroups.Add(evg.GetClone());
                            importToAnim.MiniHeader = header;
                            importToAnim.SetIsModified(true);

                            isInvalidHKX = true;
                        }
                    }
                }
            }

            DoAnimRefThing(animRefToImportFrom, (int)importFromID);
            return !isInvalidHKX;
        }

        public void ShowDialogImportFromAnimID()
        {
            if (FileContainer == null || SelectedTae == null)
                return;

            DialogManager.AskForInputString("Import From Animation ID", "Enter the animation ID to import from. This will replace animation and all events with those of the specified animation.\n" +
                "Accepts the full string with prefix or just the ID as a number.",
                GameRoot.CurrentAnimIDFormatType.ToString(), result =>
                {
                    Main.WinForm.Invoke(new Action(() =>
                    {
                        if (int.TryParse(result.Replace("a", "").Replace("_", ""), out int id))
                        {
                            var animRefToImportFrom = SelectAnimByFullID(id);

                            if (animRefToImportFrom == null)
                            {
                                DialogManager.DialogOK("Import Failed", $"Unable to find anim {id}.");
                            }
                            else if (animRefToImportFrom == SelectedTaeAnim)
                            {
                                DialogManager.DialogOK("Import Failed", $"Anim with ID {id} is the current animation.");
                            }

                            
                            if (!ImmediateImportAnim(SelectedTaeAnim, id))
                            {
                                DialogManager.DialogOK("Import Warning", $"Anim with ID {id} didn't resolve to a valid HKX so only event data was imported.");
                            }

                            SelectNewAnimRef(SelectedTae, SelectedTaeAnim, doNotCommitToGraph: true);
                            HardReset();

                        }
                        else
                        {
                            DialogManager.DialogOK("Import Failed", $"\"{result}\" is not a valid animation ID.");
                        }
                    }));
                }, canBeCancelled: true);
        }

        public void NextAnim(bool shiftPressed, bool ctrlPressed)
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
                                        if (taeList[currentTaeIndex].Animations.Count == 0)
                                            DoSmallStep();
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

                                currentAnimIndex = 0;
                            }
                            else
                            {
                                var startSection = GameRoot.GameTypeHasLongAnimIDs ? (SelectedTaeAnim.ID / 1_000000) : (SelectedTaeAnim.ID / 1_0000);

                                //long stopAtSection = -1;
                                for (int i = currentAnimIndex; i < SelectedTae.Animations.Count; i++)
                                {
                                    var thisSection = GameRoot.GameTypeHasLongAnimIDs ? (SelectedTae.Animations[i].ID / 1_000000) : (SelectedTae.Animations[i].ID / 1_0000);
                                    if (startSection != thisSection)
                                    {
                                        currentAnimIndex = i;
                                        return;
                                        //if (stopAtSection == -1)
                                        //{
                                        //    stopAtSection = thisSection;
                                        //    currentAnimIndex = i;
                                        //}
                                        //else
                                        //{
                                        //    if (thisSection == stopAtSection)
                                        //    {
                                        //        currentAnimIndex = i;
                                        //    }
                                        //    else
                                        //    {
                                        //        return;
                                        //    }
                                        //}
                                    }
                                }

                                currentAnimIndex = 0;
                            }

                            
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
                            SelectNewAnimRef(taeList[currentTaeIndex], taeList[currentTaeIndex].Animations[currentAnimIndex], scrollOnCenter: Input.ShiftHeld || Input.CtrlHeld);
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
                Console.WriteLine("Fatcat");
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
                                        if (taeList[currentTaeIndex].Animations.Count == 0)
                                            DoSmallStep();
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

                                currentAnimIndex = 0;
                            }
                            else
                            {
                                var startSection = GameRoot.GameTypeHasLongAnimIDs ? (SelectedTaeAnim.ID / 1_000000) : (SelectedTaeAnim.ID / 1_0000);
                                if (currentAnimIndex == 0)
                                    currentAnimIndex = SelectedTae.Animations.Count - 1;
                                long stopAtSection = -1;
                                for (int i = currentAnimIndex; i >= 0; i--)
                                {
                                    var thisSection = GameRoot.GameTypeHasLongAnimIDs ? (SelectedTae.Animations[i].ID / 1_000000) : (SelectedTae.Animations[i].ID / 1_0000);
                                    if (startSection != thisSection)
                                    {
                                        if (stopAtSection == -1)
                                        {
                                            stopAtSection = thisSection;
                                            currentAnimIndex = i;
                                        }
                                        else
                                        {
                                            if (thisSection == stopAtSection)
                                            {
                                                currentAnimIndex = i;
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }
                                    }
                                }

                                //currentAnimIndex = 0;
                            }

                            
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

                        SelectNewAnimRef(taeList[currentTaeIndex], taeList[currentTaeIndex].Animations[currentAnimIndex], scrollOnCenter: Input.ShiftHeld || Input.CtrlHeld);
                    }
                }
            }
            catch
            {

            }
        }

        public void TransportNextFrame()
        {
            if (PlaybackCursor.IsPlayingRemoFullPreview)
                return;

            PlaybackCursor.IsPlaying = false;
            PlaybackCursor.IsStepping = true;

            PlaybackCursor.CurrentTime += PlaybackCursor.CurrentSnapInterval;
            PlaybackCursor.CurrentTime = Math.Floor(PlaybackCursor.CurrentTime / PlaybackCursor.CurrentSnapInterval) * PlaybackCursor.CurrentSnapInterval;

            if (PlaybackCursor.CurrentTime > PlaybackCursor.MaxTime && PlaybackCursor.MaxTime > 0)
                PlaybackCursor.CurrentTime %= PlaybackCursor.MaxTime;

            //PlaybackCursor.StartTime = PlaybackCursor.CurrentTime;
            Graph.ScrollToPlaybackCursor(1);

        }

        public void TransportPreviousFrame()
        {
            PlaybackCursor.IsPlaying = false;
            PlaybackCursor.IsStepping = true;

            PlaybackCursor.CurrentTime -= PlaybackCursor.CurrentSnapInterval;
            PlaybackCursor.CurrentTime = Math.Floor(PlaybackCursor.CurrentTime / PlaybackCursor.CurrentSnapInterval) * PlaybackCursor.CurrentSnapInterval;

            if (PlaybackCursor.CurrentTime < 0)
                PlaybackCursor.CurrentTime += PlaybackCursor.MaxTime;

            //PlaybackCursor.StartTime = PlaybackCursor.CurrentTime;

            Graph.ScrollToPlaybackCursor(1);
        }

        public void ReselectCurrentAnimation()
        {
            SelectNewAnimRef(SelectedTae, SelectedTaeAnim);
        }

        public void HardReset()
        {
            if (Graph == null)
                return;
            SoundManager.StopAllSounds();
            Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.ForeachWeaponModel(wpnMdl => wpnMdl?.AnimContainer?.ResetAll());
            Graph.ViewportInteractor.RootMotionSendHome();
            Graph.ViewportInteractor.ResetRootMotion();
            Graph.ViewportInteractor.RemoveTransition();
            Graph.PlaybackCursor.RestartFromBeginning();
            Graph.ViewportInteractor.CurrentModel.AnimContainer?.ResetAll();
            Graph.ViewportInteractor.CurrentModel.ChrAsm?.ForeachWeaponModel(wpnMdl => wpnMdl?.AnimContainer?.ResetAll());
            if (Graph.ViewportInteractor.CurrentModel.AnimContainer != null)
            {
                Graph.ViewportInteractor.CurrentModel.AnimContainer.ResetRootMotion();
                Graph.ViewportInteractor.HardResetRootMotionToStartHere();
            }
            GFX.CurrentWorldView.RootMotionFollow_Translation = Vector3.Zero;
            GFX.CurrentWorldView.RootMotionFollow_Rotation = 0;
            GFX.CurrentWorldView.Update(0);
        }

        public void CopyCurrentAnimIDToClipboard(bool isUnformatted)
        {
            int currentID = (int)GetAnimListInfoOfAnim(SelectedTaeAnim).FullID;
            string text = !isUnformatted ? GameRoot.SplitAnimID.FromFullID(currentID).GetFormattedIDString() : $"{currentID}";
            System.Windows.Forms.Clipboard.SetText(text);
            NotificationManager.PushNotification($"Copied '{text}' to clipboard.");
        }

        public void GotoAnimIDInClipboard()
        {
            var clip = new string(System.Windows.Forms.Clipboard.GetText().Where(c => (c >= '1' && c <= '9') || c == '0').ToArray());
            if (int.TryParse(clip, out int id))
            {
                if (GotoAnimID(id, true, false, out _))
                {
                    NotificationManager.PushNotification($"Went to animation with ID in clipboard ({id}).");
                }
                else
                {
                    NotificationManager.PushNotification($"Unable to find animation with ID in clipboard ({id}).");
                }
            }
            else
            {
                NotificationManager.PushNotification("Text in clipboard was not a valid animation ID.");
            }
            
        }

        public void Update()
        {
            //Console.WriteLine($"Main.HasUncommittedWindowResize = {Main.HasUncommittedWindowResize}");
            if (Main.HasUncommittedWindowResize)
            {
                MouseHoverKind = ScreenMouseHoverKind.None;
                WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                //WhereLastMouseClickStarted = ScreenMouseHoverKind.None;
                CurrentDividerDragMode = DividerDragMode.None;
                return;
            }

            if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
            {
                if (Graph?.PlaybackCursor?.Scrubbing == true || Graph?.PlaybackCursor?.IsStepping == true)
                {
                    RemoManager.CancelFullPlayback();
                }

                if (REMO_HOTFIX_REQUEST_PLAY_RESUME_NEXT_FRAME && Graph != null && Graph.PlaybackCursor != null)
                {
                    HardReset();
                    REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME = true;
                    REMO_HOTFIX_REQUEST_PLAY_RESUME_NEXT_FRAME = false;
                }
                else if (REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME)
                {
                    HardReset();
                    REMO_HOTFIX_REQUEST_PLAY_RESUME_THIS_FRAME = false;
                }
            }

            if (QueuedChangeEventType)
            {
                if (SingleEventBoxSelected)
                    ChangeTypeOfSelectedEvent();
                QueuedChangeEventType = false;
            }

            if (ImporterWindow_FLVER2 == null || ImporterWindow_FLVER2.IsDisposed || !ImporterWindow_FLVER2.Visible)
            {
                ImporterWindow_FLVER2?.Dispose();
                ImporterWindow_FLVER2 = null;
            }
            ImporterWindow_FLVER2?.UpdateGuiLockout();

            if (!Input.LeftClickHeld)
            {
                Graph?.ReleaseCurrentDrag();
            }

#if !DEBUG
            if (!Main.Active)
            {
                SoundManager.StopAllSounds();
            }
#endif
            //TODO: CHECK THIS
            PauseUpdate = OSD.Hovered;

            if (!PauseUpdate)
            {
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

                // Another bad hotfix
                Rectangle killCursorRect = new Rectangle(
                        (int)(DividerLeftGrabEndX),
                        0,
                        (int)(DividerRightGrabStartX - DividerLeftGrabEndX),
                        TopOfGraphAnimInfoMargin + Rect.Top + TopMenuBarMargin);

                if (killCursorRect.Contains(Input.MousePositionPoint))
                {
                    Input.CursorType = MouseCursorType.Arrow;
                }

                if (!Input.LeftClickHeld)
                    Graph?.MouseReleaseStuff();
            }

            //if (MultiSelectedEventBoxes.Count > 0 && multiSelectedEventBoxesCountLastFrame < MultiSelectedEventBoxes.Count)
            //{
            //    if (Config.UseGamesMenuSounds)
            //        FmodManager.PlaySE("f000000000");
            //}

            multiSelectedEventBoxesCountLastFrame = MultiSelectedEventBoxes.Count;

            // Always update playback regardless of GUI memes.
            // Still only allow hitting spacebar to play/pause
            // if the window is in focus.
            // Also for Interactor
            if (Graph != null)
            {
                Graph.UpdatePlaybackCursor(allowPlayPauseInput: Main.Active);
                Graph.ViewportInteractor?.GeneralUpdate();
            }

            //if (MenuBar.IsAnyMenuOpenChanged)
            //{
            //    ButtonEditCurrentAnimInfo.Visible = !MenuBar.IsAnyMenuOpen;
            //    ButtonEditCurrentTaeHeader.Visible = !MenuBar.IsAnyMenuOpen;
            //    ButtonGotoEventSource.Visible = !MenuBar.IsAnyMenuOpen;
            //    inspectorWinFormsControl.Visible = !MenuBar.IsAnyMenuOpen;
            //}

            if (OSD.Hovered)
            {
                PauseUpdate = true;
            }

            if (!DialogManager.AnyDialogsShowing)
            {
                Transport.Update(Main.DELTA_UPDATE);
            }

            if (PauseUpdate || DialogManager.AnyDialogsShowing)
            {
                
                return;
            }

            //if (!(OSD.Focused || DialogManager.AnyDialogsShowing))
            //    Transport.Update(Main.DELTA_UPDATE);

            bool isOtherPaneFocused = ModelViewerBounds.Contains((int)Input.LeftClickDownAnchor.X, (int)Input.LeftClickDownAnchor.Y);

            Input.CursorType = MouseCursorType.Arrow;

            if (WhereCurrentMouseClickStarted != ScreenMouseHoverKind.None)
            {
                WhereLastMouseClickStarted = WhereCurrentMouseClickStarted;
            }

            if (Main.Active)
            {
                if (Input.KeyDown(Keys.Escape))
                {
                    SoundManager.StopAllSounds();
                    RumbleCamManager.ClearActive();
                }

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F1))
                    ChangeTypeOfSelectedEvent();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F2))
                    ShowDialogChangeAnimName();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F3))
                    ShowDialogEditCurrentAnimInfo();

#if NIGHTFALL
                // MEOW TESTING?
                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.OemSemicolon))
                    NIGHTFALL_ToggleImport();
#endif

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F4) || RequestGoToEventSource)
                {
                    GoToEventSource();
                    RequestGoToEventSource = false;
                }

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F5))
                    LiveRefresh();

                if (Input.KeyDown(Microsoft.Xna.Framework.Input.Keys.F8))
                    ShowComboMenu();

                var zHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Z);
                var yHeld = Input.KeyHeld(Microsoft.Xna.Framework.Input.Keys.Y);

                if (Input.CtrlHeld && !Input.ShiftHeld && !Input.AltHeld && !DialogManager.AnyDialogsShowing)
                {
                    if ((Input.KeyDown(Keys.OemPlus) || Input.KeyDown(Keys.Add)) && !isOtherPaneFocused)
                    {
                        Graph?.ZoomInOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if ((Input.KeyDown(Keys.OemMinus) || Input.KeyDown(Keys.Subtract)) && !isOtherPaneFocused)
                    {
                        Graph?.ZoomOutOneNotch(
                            (float)(
                            (Graph.PlaybackCursor.GUICurrentTime * Graph.SecondsPixelSize)
                            - Graph.ScrollViewer.Scroll.X));
                    }
                    else if ((Input.KeyDown(Keys.D0) || Input.KeyDown(Keys.NumPad0)) && !isOtherPaneFocused)
                    {
                        Graph?.ResetZoom(0);
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.C) &&
                        WhereLastMouseClickStarted == ScreenMouseHoverKind.EventGraph && !isOtherPaneFocused)
                    {
                        Graph?.DoCopy();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.X) &&
                       WhereLastMouseClickStarted == ScreenMouseHoverKind.EventGraph && !isOtherPaneFocused)
                    {
                        Graph?.DoCut();
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.V) &&
                        WhereLastMouseClickStarted == ScreenMouseHoverKind.EventGraph && !isOtherPaneFocused)
                    {
                        Graph?.DoPaste(isAbsoluteLocation: false);
                    }
                    else if (!CurrentlyEditingSomethingInInspector && Input.KeyDown(Keys.A) && !isOtherPaneFocused)
                    {
                        if (Graph != null && Graph.currentDrag.DragType == BoxDragType.None)
                        {
                            SelectedEventBox = null;
                            MultiSelectedEventBoxes.Clear();
                            foreach (var box in Graph.EventBoxes)
                            {
                                MultiSelectedEventBoxes.Add(box);
                            }
                        }
                    }
                    else if (Input.KeyDown(Keys.F))
                    {
                        ShowDialogFind();
                    }
                    else if (Input.KeyDown(Keys.G))
                    {
                        ShowDialogGotoAnimID();
                    }
                    else if (Input.KeyDown(Keys.H))
                    {
                        ShowDialogGotoAnimSectionID();
                    }
                    else if (Input.KeyDown(Keys.I))
                    {
                        ShowDialogImportFromAnimID();
                    }
                    else if (Input.KeyDown(Keys.J))
                    {
                        if (IsFileOpen)
                        {
                            CopyCurrentAnimIDToClipboard(false);
                        }
                    }
                    else if (Input.KeyDown(Keys.K))
                    {
                        if (IsFileOpen)
                        {
                            GotoAnimIDInClipboard();
                        }
                    }
                    else if (Input.KeyDown(Keys.S))
                    {
                        SaveCurrentFile();
                    }
                    else if (Graph != null && Input.KeyDown(Keys.R))
                    {
                        HardReset();
                    }
                }

                if (Input.CtrlHeld && Input.ShiftHeld && !Input.AltHeld)
                {
                    if (Input.KeyDown(Keys.V) && !isOtherPaneFocused)
                    {
                        Graph.DoPaste(isAbsoluteLocation: true);
                    }
                    else if (Input.KeyDown(Keys.S))
                    {
                        File_SaveAs();
                    }
                    else if (Input.KeyDown(Keys.J))
                    {
                        if (IsFileOpen)
                        {
                            CopyCurrentAnimIDToClipboard(true);
                        }
                    }
                }

                if (!Input.CtrlHeld && Input.ShiftHeld && !Input.AltHeld)
                {
                    if (Input.KeyDown(Keys.D))
                    {
                        if (SelectedEventBox != null)
                            SelectedEventBox = null;
                        if (MultiSelectedEventBoxes.Count > 0)
                            MultiSelectedEventBoxes.Clear();
                    }
                }

                if (Input.KeyDown(Keys.Delete) && !isOtherPaneFocused)
                {
                    Graph.DeleteSelectedEvent();
                }

                //if (Graph != null && Input.KeyDown(Keys.Home) && !Graph.PlaybackCursor.Scrubbing)
                //{
                //    if (CtrlHeld)
                //        Graph.PlaybackCursor.IsPlaying = false;

                //    Graph.PlaybackCursor.CurrentTime = ShiftHeld ? 0 : Graph.PlaybackCursor.StartTime;
                //    Graph.ViewportInteractor.ResetRootMotion(0);

                //    Graph.ScrollToPlaybackCursor(1);
                //}



                //if (Graph != null && Input.KeyDown(Keys.End) && !Graph.PlaybackCursor.Scrubbing)
                //{
                //    if (CtrlHeld)
                //        Graph.PlaybackCursor.IsPlaying = false;

                //    Graph.PlaybackCursor.CurrentTime = Graph.PlaybackCursor.MaxTime;
                //    Graph.ViewportInteractor.ResetRootMotion((float)Graph.PlaybackCursor.MaxFrame);

                //    Graph.ScrollToPlaybackCursor(1);
                //}

                NextAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Down) && !Input.KeyHeld(Keys.Up));
                NextAnimRepeaterButton_KeepSubID.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.PageDown) && !Input.KeyHeld(Keys.PageUp));

                if (NextAnimRepeaterButton.State)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {
                        if (!REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME && Graph?.PlaybackCursor?.IsPlayingRemoFullPreview != true && !REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME)
                        {
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = true;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV = false;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT = Input.ShiftHeld;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL = Input.CtrlHeld;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = null;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = null;
                        }
                    }
                    else
                    {
                        Graph?.ViewportInteractor?.CancelCombo();
                        RemoManager.CancelFullPlayback();
                        NextAnim(Input.ShiftHeld, Input.CtrlHeld);
                    }
                    
                }

                if (NextAnimRepeaterButton_KeepSubID.State)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {

                    }
                    else
                    {
                        var taeSectionRange = FileContainer.GetTaeSectionMinMax();

                        long currentID = GetAnimListInfoOfAnim(SelectedTaeAnim).FullID;
                        long startingID = currentID;
                        bool foundValidAnim = false;
                        bool wrappedAround = false;
                        do
                        {
                            currentID += (GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000);

                            if (wrappedAround)
                            {
                                if (currentID >= startingID)
                                {
                                    GotoAnimID(startingID, false, false, out _);
                                    break;
                                }
                            }
                            else
                            {
                                if (currentID > (GameRoot.GameTypeHasLongAnimIDs ? 999_999999 : 999_9999) ||
                                currentID > ((GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000) * taeSectionRange.Max) + (GameRoot.GameTypeHasLongAnimIDs ? 999999 : 9999))
                                {
                                    wrappedAround = true;
                                    currentID = ((GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000) * taeSectionRange.Min) + (currentID % (GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000));
                                }
                            }

                            

                            if (GotoAnimID(currentID, false, false, out _))
                                foundValidAnim = true;
                        }
                        while (!foundValidAnim);
                    }
                }

                PrevAnimRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Up) && !Input.KeyHeld(Keys.Down));
                PrevAnimRepeaterButton_KeepSubID.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.PageUp) && !Input.KeyHeld(Keys.PageDown));

                if (PrevAnimRepeaterButton.State)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {
                        if (!REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME && Graph?.PlaybackCursor?.IsPlayingRemoFullPreview != true && !REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME)
                        {
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = true;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV = true;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT = Input.ShiftHeld;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL = Input.CtrlHeld;

                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE = null;
                            REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM = null;
                            
                        }
                    }
                    else
                    {
                        Graph?.ViewportInteractor?.CancelCombo();
                        RemoManager.CancelFullPlayback();
                        PrevAnim(Input.ShiftHeld, Input.CtrlHeld);
                    }
                    
                }

                if (PrevAnimRepeaterButton_KeepSubID.State)
                {
                    if (Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
                    {

                    }
                    else
                    {
                        var taeSectionRange = FileContainer.GetTaeSectionMinMax();

                        long currentID = GetAnimListInfoOfAnim(SelectedTaeAnim).FullID;
                        long startingID = currentID;
                        bool foundValidAnim = false;
                        bool wrappedAround = false;
                        do
                        {
                            currentID -= (GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000);

                            if (wrappedAround)
                            {
                                if (currentID <= startingID)
                                {
                                    GotoAnimID(startingID, false, false, out _);
                                    break;
                                }
                            }
                            else
                            {
                                if (currentID < 0 || currentID < ((GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000) * taeSectionRange.Min))
                                {
                                    wrappedAround = true;
                                    currentID = ((GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000) * taeSectionRange.Max) + (currentID % (GameRoot.GameTypeHasLongAnimIDs ? 1_000000 : 1_0000));
                                }
                            }


                            

                            if (GotoAnimID(currentID, false, false, out _))
                                foundValidAnim = true;
                        }
                        while (!foundValidAnim);
                    }
                }

                if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME)
                {
                    if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE != null && REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM != null)
                    {
                        SelectNewAnimRef(REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE, REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_TAE_ANIM);
                    }
                    else
                    {
                        if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_PREV)
                            PrevAnim(REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT, REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL);
                        else
                            NextAnim(REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_SHIFT, REMO_HOTFIX_REQUEST_CUT_ADVANCE_CUT_IS_CTRL);
                    }

                    if (SelectedTaeAnim.ID <= 9999)
                        Graph.PlaybackCursor.IsPlaying = true;
                }

                REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME = false;

                if (PlaybackCursor != null)
                {
                    NextFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Right));

                    if (NextFrameRepeaterButton.State)
                    {
                        TransportNextFrame();
                    }

                    PrevFrameRepeaterButton.Update(GamePadState.Default, Main.DELTA_UPDATE, Input.KeyHeld(Keys.Left));

                    if (PrevFrameRepeaterButton.State)
                    {
                        TransportPreviousFrame();
                    }
                }

                if (Input.KeyDown(Keys.Space) && Input.CtrlHeld && !Input.AltHeld)
                {
                    if (SelectedTae != null)
                    {
                        if (SelectedTaeAnim != null)
                        {
                            SelectNewAnimRef(SelectedTae, SelectedTaeAnim);
                            if (Input.ShiftHeld)
                            {
                                Graph?.ViewportInteractor?.RemoveTransition();
                            }
                        }
                    }
                }

                if (Input.KeyDown(Keys.Back) && !isOtherPaneFocused)
                {
                    Graph?.ViewportInteractor?.RemoveTransition();
                }

                if (UndoButton.Update(Main.DELTA_UPDATE, (Input.CtrlHeld && !Input.ShiftHeld && !Input.AltHeld) && (zHeld && !yHeld)) && !isOtherPaneFocused)
                {
                    UndoMan.Undo();
                }

                if (RedoButton.Update(Main.DELTA_UPDATE, (Input.CtrlHeld && !Input.ShiftHeld && !Input.AltHeld) && (!zHeld && yHeld)) && !isOtherPaneFocused)
                {
                    UndoMan.Redo();
                }
            }

            if (!Input.LeftClickHeld)
            {
                if (CurrentDividerDragMode != DividerDragMode.None)
                {
                    CurrentDividerDragMode = DividerDragMode.None;
                    MouseHoverKind = ScreenMouseHoverKind.None;

                }
                WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
            }


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
                else if (Input.MousePosition.Y >= DividerRightPaneHorizontalGrabStartY - (DividerHitboxPad) && Input.MousePosition.Y <= DividerRightPaneHorizontalGrabEndY + (DividerHitboxPad)
                    && Input.MousePosition.X >= DividerRightGrabStartX && Input.MousePosition.X <= DividerRightGrabEndX)
                {
                    MouseHoverKind = ScreenMouseHoverKind.BottomLeftCornerOfModelViewer;
                    //Input.CursorType = MouseCursorType.DragX;
                    if (Input.LeftClickDown)
                    {
                        CurrentDividerDragMode = DividerDragMode.BottomLeftCornerResize;
                        WhereCurrentMouseClickStarted = ScreenMouseHoverKind.BottomLeftCornerOfModelViewer;
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

            if (MouseHoverKind == ScreenMouseHoverKind.BottomLeftCornerOfModelViewer
                || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.BottomLeftCornerOfModelViewer)
            {
                Input.CursorType = MouseCursorType.DragBottomLeftResize;
            }
            else if (MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
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

            if (CurrentDividerDragMode == DividerDragMode.LeftVertical || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragX;
                    LeftSectionWidth = MathHelper.Max((Input.MousePosition.X - Rect.X) - (DividerVisiblePad / 2), LeftSectionWidthMin);
                    LeftSectionWidth = MathHelper.Min(LeftSectionWidth, Rect.Width - MiddleSectionWidthMin - RightSectionWidth - (DividerVisiblePad * 2));
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.RightVertical || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragX;
                    RightSectionWidth = MathHelper.Max((Rect.Right - Input.MousePosition.X) + (DividerVisiblePad / 2), RightSectionWidthMin);
                    RightSectionWidth = MathHelper.Min(RightSectionWidth, Rect.Width - MiddleSectionWidthMin - LeftSectionWidth - (DividerVisiblePad * 2));
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.RightPaneHorizontal || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerRightPaneHorizontal)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragY;
                    TopRightPaneHeight = MathHelper.Max((Input.MousePosition.Y - Rect.Top - TopMenuBarMargin - TransportHeight) + (DividerVisiblePad / 2), TopRightPaneHeightMinNew);
                    TopRightPaneHeight = MathHelper.Min(TopRightPaneHeight, Rect.Height - BottomRightPaneHeightMinNew - DividerVisiblePad - TopMenuBarMargin - TransportHeight);
                    MouseHoverKind = ScreenMouseHoverKind.DividerBetweenCenterAndRightPane;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                }
            }
            else if (CurrentDividerDragMode == DividerDragMode.BottomLeftCornerResize || WhereCurrentMouseClickStarted == ScreenMouseHoverKind.BottomLeftCornerOfModelViewer)
            {
                if (Input.LeftClickHeld)
                {
                    //Input.CursorType = MouseCursorType.DragY;
                    TopRightPaneHeight = MathHelper.Max((Input.MousePosition.Y - Rect.Top - TopMenuBarMargin - TransportHeight) + (DividerVisiblePad / 2), TopRightPaneHeightMinNew);
                    TopRightPaneHeight = MathHelper.Min(TopRightPaneHeight, Rect.Height - BottomRightPaneHeightMinNew - DividerVisiblePad - TopMenuBarMargin - TransportHeight);
                    RightSectionWidth = MathHelper.Max((Rect.Right - Input.MousePosition.X) + (DividerVisiblePad / 2), RightSectionWidthMin);
                    RightSectionWidth = MathHelper.Min(RightSectionWidth, Rect.Width - MiddleSectionWidthMin - LeftSectionWidth - (DividerVisiblePad * 2));
                    MouseHoverKind = ScreenMouseHoverKind.BottomLeftCornerOfModelViewer;
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }
                else
                {
                    //Input.CursorType = MouseCursorType.Arrow;
                    CurrentDividerDragMode = DividerDragMode.None;
                    WhereCurrentMouseClickStarted = ScreenMouseHoverKind.None;
                    MouseHoverKind = ScreenMouseHoverKind.None;
                }
            }

            LeftSectionWidth = MathHelper.Max(LeftSectionWidth, LeftSectionWidthMin);
            LeftSectionWidth = MathHelper.Min(LeftSectionWidth, Rect.Width - MiddleSectionWidthMin - RightSectionWidthMin - (DividerVisiblePad * 2));

            RightSectionWidth = MathHelper.Max(RightSectionWidth, RightSectionWidthMin);
            RightSectionWidth = MathHelper.Min(RightSectionWidth, Rect.Width - MiddleSectionWidthMin - LeftSectionWidthMin - (DividerVisiblePad * 2));

            TopRightPaneHeight = MathHelper.Max(TopRightPaneHeight, TopRightPaneHeightMinNew);
            TopRightPaneHeight = MathHelper.Min(TopRightPaneHeight, Rect.Height - BottomRightPaneHeightMinNew - DividerVisiblePad - TopMenuBarMargin - TransportHeight);

            if (!Rect.Contains(Input.MousePositionPoint))
            {
                MouseHoverKind = ScreenMouseHoverKind.None;
            }

            // Very specific edge case to handle before you load an anibnd so that
            // it won't have the resize cursor randomly. This box spans all the way
            // from left of screen to the hitbox of the right vertical divider and
            // just immediately clears the resize cursor in that entire huge region.
            if (AnimationListScreen == null && Graph == null
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
                GFX.CurrentWorldView.DisableAllInput = true;
                return;
            }
            else if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.DividerRightPaneHorizontal)
            {
                Input.CursorType = MouseCursorType.DragY;
                GFX.CurrentWorldView.DisableAllInput = true;
                return;
            }
            else if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.BottomLeftCornerOfModelViewer)
            {
                Input.CursorType = MouseCursorType.DragBottomLeftResize;
                GFX.CurrentWorldView.DisableAllInput = true;
                return;
            }
            else if (WhereCurrentMouseClickStarted == ScreenMouseHoverKind.ShaderAdjuster)
            {
                GFX.CurrentWorldView.DisableAllInput = true;
                return;
            }
            else if (!(MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndRightPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerBetweenCenterAndLeftPane
                || MouseHoverKind == ScreenMouseHoverKind.DividerRightPaneHorizontal
                || MouseHoverKind == ScreenMouseHoverKind.BottomLeftCornerOfModelViewer))
            {
                if (AnimationListScreen != null && AnimationListScreen.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.AnimList;
                else if (Graph != null && Graph.Rect.Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.EventGraph;
                else if (
                    new Rectangle(
                        (int)(ImGuiEventInspectorPos.X),
                        (int)(ImGuiEventInspectorPos.Y),
                        (int)(ImGuiEventInspectorSize.X),
                        (int)(ImGuiEventInspectorSize.Y)
                        ).InverseDpiScaled()
                        .Contains(Input.MousePositionPoint))
                    MouseHoverKind = ScreenMouseHoverKind.Inspector;
                //else if (ShaderAdjuster.Bounds.Contains(new System.Drawing.Point(Input.MousePositionPoint.X, Input.MousePositionPoint.Y)))
                //    MouseHoverKind = ScreenMouseHoverKind.ShaderAdjuster;
                else if (
                    ModelViewerBounds_InputArea.Contains(Input.MousePositionPoint))
                {
                    MouseHoverKind = ScreenMouseHoverKind.ModelViewer;
                }
                else
                    MouseHoverKind = ScreenMouseHoverKind.None;

                if (Input.LeftClickDown)
                {
                    WhereCurrentMouseClickStarted = MouseHoverKind;
                }

                if (AnimationListScreen != null)
                {

                    if (MouseHoverKind == ScreenMouseHoverKind.AnimList ||
                        WhereCurrentMouseClickStarted == ScreenMouseHoverKind.AnimList)
                    {
                        Input.CursorType = MouseCursorType.Arrow;
                        AnimationListScreen.Update(Main.DELTA_UPDATE,
                            allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                    else
                    {
                        AnimationListScreen.UpdateMouseOutsideRect(Main.DELTA_UPDATE,
                            allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                }

                if (Graph != null)
                {
                    Graph.UpdateMiddleClickPan();

                    if (!Graph.Rect.Contains(Input.MousePositionPoint))
                    {
                        HoveringOverEventBox = null;
                    }

                    if (MouseHoverKind == ScreenMouseHoverKind.EventGraph ||
                        WhereCurrentMouseClickStarted == ScreenMouseHoverKind.EventGraph)
                    {
                        Graph.Update(allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                    else
                    {
                        Graph.UpdateMouseOutsideRect(Main.DELTA_UPDATE, allowMouseUpdate: CurrentDividerDragMode == DividerDragMode.None);
                    }
                }

                if (MouseHoverKind == ScreenMouseHoverKind.ModelViewer ||
                    WhereCurrentMouseClickStarted == ScreenMouseHoverKind.ModelViewer)
                {
                    Input.CursorType = MouseCursorType.Arrow;
                    GFX.CurrentWorldView.DisableAllInput = false;
                }
                else
                {
                    //GFX.World.DisableAllInput = true;
                }

                if (MouseHoverKind == ScreenMouseHoverKind.Inspector ||
                    WhereCurrentMouseClickStarted == ScreenMouseHoverKind.Inspector)
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

        public void HandleWindowResize(Rectangle oldBounds, Rectangle newBounds)
        {
            if (!Main.WindowShown || !IsRectValid)
                return;

            if (oldBounds.Width > 1000 && oldBounds.Height > 500 && newBounds.Width > 1000 && newBounds.Height > 500)
            {
                float ratioW = 1.0f * newBounds.Width / oldBounds.Width;
                float ratioH = 1.0f * newBounds.Height / oldBounds.Height;

                LeftSectionWidth = LeftSectionWidth * ratioW;
                RightSectionWidth = RightSectionWidth * ratioW;
                TopRightPaneHeight = TopRightPaneHeight * ratioH;

                UpdateLayout();
            }
            
        }

        public bool IsRectValid => Rect.Width >= 1000 && Rect.Height >= 500;
        public void UpdateLayout()
        {
            if (!Main.WindowShown || !IsRectValid)
                return;

            if (Rect.IsEmpty)
            {
                return;
            }
                if (TopRightPaneHeight < TopRightPaneHeightMinNew)
                    TopRightPaneHeight = TopRightPaneHeightMinNew;




                if (RightSectionWidth < RightSectionWidthMin)
                    RightSectionWidth = RightSectionWidthMin;

                if (TopRightPaneHeight > (Rect.Height - BottomRightPaneHeightMinNew - TopMenuBarMargin - TransportHeight))
                {
                    TopRightPaneHeight = (Rect.Height - BottomRightPaneHeightMinNew - TopMenuBarMargin - TransportHeight);
                    Main.RequestViewportRenderTargetResolutionChange = true;
                    Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                }

                if (AnimationListScreen != null && Graph != null)
                {
                    if (LeftSectionWidth < LeftSectionWidthMin)
                    {
                        LeftSectionWidth = LeftSectionWidthMin;
                        Main.RequestViewportRenderTargetResolutionChange = true;
                        Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                    }


                    if (MiddleSectionWidth < MiddleSectionWidthMin)
                    {
                        var adjustment = MiddleSectionWidthMin - MiddleSectionWidth;
                        RightSectionWidth -= adjustment;
                        Main.RequestViewportRenderTargetResolutionChange = true;
                        Main.RequestHideOSD = Main.RequestHideOSD_MAX;
                    }

                    AnimationListScreen.Rect = new Rectangle(
                        (int)LeftSectionStartX,
                        Rect.Top + TopMenuBarMargin,
                        (int)LeftSectionWidth,
                        Rect.Height - TopMenuBarMargin).ApplyPadding(t: 0, b: PaddingAroundDragableThings, l: PaddingAroundDragableThings, r: PaddingAroundDragableThings);

                Graph.Rect = new Rectangle(
                        (int)MiddleSectionStartX,
                        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                        (int)MiddleSectionWidth,
                        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin).ApplyPadding(t: 0, b: PaddingAroundDragableThings, l: PaddingAroundDragableThings, r: PaddingAroundDragableThings);

                //var plannedGraphRect = new Rectangle(
                //        (int)MiddleSectionStartX,
                //        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                //        (int)MiddleSectionWidth,
                //        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin).ApplyPadding(PaddingAroundDragableThings);
            }
            //    else
            //    {
            //        var plannedGraphRect = new Rectangle(
            //            (int)MiddleSectionStartX,
            //            Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
            //            (int)MiddleSectionWidth,
            //            Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin).ApplyPadding(PaddingAroundDragableThings);
            //}

                Transport.Rect = new Rectangle(
                        (int)RightSectionStartX,
                        Rect.Top + TopMenuBarMargin,
                        (int)RightSectionWidth,
                        (int)(TransportHeight)).ApplyPadding(t: 0, b: 0, l: PaddingAroundDragableThings, r: PaddingAroundDragableThings);

            //editScreenGraphInspector.Rect = new Rectangle(Rect.Width - LayoutInspectorWidth, 0, LayoutInspectorWidth, Rect.Height);


            //inspectorWinFormsControl.Bounds = new System.Drawing.Rectangle((int)RightSectionStartX, Rect.Top + TopMenuBarMargin, (int)RightSectionWidth, (int)(Rect.Height - TopMenuBarMargin - BottomRightPaneHeight - DividerVisiblePad));
            //ModelViewerBounds = new Rectangle((int)RightSectionStartX, (int)(Rect.Bottom - BottomRightPaneHeight), (int)RightSectionWidth, (int)(BottomRightPaneHeight));

            //ShaderAdjuster.Size = new System.Drawing.Size((int)RightSectionWidth, ShaderAdjuster.Size.Height);

            

            ModelViewerBounds = new Rectangle(
                    (int)RightSectionStartX, 
                    Rect.Top + TopMenuBarMargin + TransportHeight, 
                    (int)RightSectionWidth, 
                    (int)(TopRightPaneHeight)).ApplyPadding(t: 0, b: PaddingAroundDragableThings, l: PaddingAroundDragableThings, r: PaddingAroundDragableThings);

            if (GFX.CurrentWorldView.LockAspectRatioDuringRemo && Graph?.ViewportInteractor?.EntityType == TaeViewportInteractor.TaeEntityType.REMO)
            {
                var sizeFromWidth = new Vector2(ModelViewerBounds.Width, ModelViewerBounds.Width * (9f / 16f));
                var sizeFromHeight = new Vector2(ModelViewerBounds.Height * (16f / 9f), ModelViewerBounds.Height);

                if (sizeFromHeight.LengthSquared() < sizeFromWidth.LengthSquared())
                {
                    Vector2 topLeft = ModelViewerBounds.Center.ToVector2() - (sizeFromHeight / 2);
                    ModelViewerBounds.X = (int)topLeft.X;
                    ModelViewerBounds.Y = (int)topLeft.Y;
                    ModelViewerBounds.Width = (int)sizeFromHeight.X;
                    ModelViewerBounds.Height = (int)sizeFromHeight.Y;
                }
                else
                {
                    Vector2 topLeft = ModelViewerBounds.Center.ToVector2() - (sizeFromWidth / 2);
                    ModelViewerBounds.X = (int)topLeft.X;
                    ModelViewerBounds.Y = (int)topLeft.Y;
                    ModelViewerBounds.Width = (int)sizeFromWidth.X;
                    ModelViewerBounds.Height = (int)sizeFromWidth.Y;
                }
            }

            ModelViewerBounds_InputArea = new Rectangle(
                    ModelViewerBounds.X + (DividerHitboxPad / 2),
                    ModelViewerBounds.Y + (DividerHitboxPad / 2),
                    ModelViewerBounds.Width - DividerHitboxPad,
                    ModelViewerBounds.Height - DividerHitboxPad).ApplyPadding(PaddingAroundDragableThings);

                ImGuiEventInspectorPos = new System.Numerics.Vector2(RightSectionStartX, 
                    Rect.Top + TopMenuBarMargin + TopRightPaneHeight + DividerVisiblePad + TransportHeight) + (System.Numerics.Vector2.One * PaddingAroundDragableThings);
                ImGuiEventInspectorSize = new System.Numerics.Vector2(RightSectionWidth, 
                    Rect.Height - TopRightPaneHeight - DividerVisiblePad - TopMenuBarMargin - TransportHeight) - ((System.Numerics.Vector2.One * PaddingAroundDragableThings) * 2);

                //ShaderAdjuster.Location = new System.Drawing.Point(Rect.Right - ShaderAdjuster.Size.Width, Rect.Top + TopMenuBarMargin);
        }

        public void DrawDimmingRect(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex)
        {
            sb.Begin(transformMatrix: Main.DPIMatrix);
            try
            {
                sb.Draw(boxTex, new Rectangle(Rect.Left, Rect.Top, (int)RightSectionStartX - Rect.X, Rect.Height), Color.Black * 0.25f);
            }
            finally { sb.End(); }
        }

        public void Draw(GraphicsDevice gd, SpriteBatch sb, Texture2D boxTex,
            SpriteFont font, float elapsedSeconds, SpriteFont smallFont, Texture2D scrollbarArrowTex)
        {
            if (!IsRectValid)
                return;

            sb.Begin();
            try
            {
                sb.Draw(boxTex, new Rectangle(Rect.X, Rect.Y, (int)RightSectionStartX - Rect.X, Rect.Height).DpiScaled(), Main.Colors.MainColorBackground);

                // Draw model viewer background lel
                //sb.Draw(boxTex, ModelViewerBounds, Color.Gray);



                sb.Draw(boxTex, new Rectangle(
                    (int)(DividerLeftGrabStartX),
                    (int)(Rect.Y),
                    (int)(DividerHitboxPad),
                    (int)(Rect.Height)
                    ).DpiScaled(), Main.Colors.MainColorDivider);

                sb.Draw(boxTex, new Rectangle(
                    (int)(DividerRightGrabStartX),
                    (int)(Rect.Y),
                    (int)(DividerHitboxPad),
                    (int)(Rect.Height)
                    ).DpiScaled(), Main.Colors.MainColorDivider);

                sb.Draw(boxTex, new Rectangle(
                    (int)(RightSectionStartX),
                    (int)(DividerRightPaneHorizontalGrabStartY),
                    (int)(RightSectionWidth),
                    (int)(DividerHitboxPad)
                    ).DpiScaled(), Main.Colors.MainColorDivider);

            }
            finally { sb.End(); }



            //throw new Exception("TaeUndoMan");

            //throw new Exception("Make left/right edges of events line up to same vertical lines so the rounding doesnt make them 1 pixel off");
            //throw new Exception("Make dragging edges of scrollbar box do zoom");
            //throw new Exception("make ctrl+scroll zoom centered on mouse cursor pos");

            UpdateLayout();

            if (AnimationListScreen != null)
            {
                AnimationListScreen.Draw(gd, sb, boxTex, font, scrollbarArrowTex);

                var graphRect = Graph?.Rect ?? Rectangle.Empty;

                Rectangle curAnimInfoTextRect = new Rectangle(
                    0,
                    0,
                    graphRect.Width - 4,
                    TopOfGraphAnimInfoMargin + 2);


                graphRect.Y -= TopMenuBarMargin;
                graphRect.Height += TopMenuBarMargin;
                graphRect = graphRect.DpiScaled();

                
                //curAnimInfoTextRect.X -= graphRect.X;
                //curAnimInfoTextRect.Y -= graphRect.Y;

                

                var oldViewport = gd.Viewport;
                gd.Viewport = new Viewport(graphRect);
                {
                    sb.Begin();
                    try
                    {
                        if (TaeAnimInfoIsClone)
                        {
                            //ImGuiNET.ImGui.SetCursorPos(curAnimInfoTextRect.TopLeftCornerN());
                            //ImGuiNET.ImGui.Button("Go To Original (F4 Key)", new System.Numerics.Vector2(120, curAnimInfoTextRect.Height));
                            //if (ImGuiNET.ImGui.IsItemClicked())
                            //{
                            //    RequestGoToEventSource = true;
                            //}

                            //ImGuiDebugDrawer.DrawRect(new Rectangle(curAnimInfoTextRect.X, curAnimInfoTextRect.Y, 120, curAnimInfoTextRect.Height), )

                            if (ImGuiDebugDrawer.FakeButton(curAnimInfoTextRect.TopLeftCorner() + new Vector2(2,3), 
                                new Vector2((int)(170), curAnimInfoTextRect.Height) - new Vector2(2,2), "Go To Original (F4 Key)", 0))
                            {
                                RequestGoToEventSource = true;
                            }

                            curAnimInfoTextRect.X += (int)(170) + 8;
                            curAnimInfoTextRect.Width -= (int)(170);
                            
                        }

                        if (Config.EnableFancyScrollingStrings)
                        {
                            SelectedTaeAnimInfoScrollingText.Draw(gd, sb, Matrix.Identity, curAnimInfoTextRect.DpiScaled(), 20f, elapsedSeconds, new Vector2(0, 4), restrictToParentViewport: false);
                        }
                        else
                        {
                            var curAnimInfoTextPos = curAnimInfoTextRect.Location.ToVector2();

                            //sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + Vector2.One + Main.GlobalTaeEditorFontOffset, Color.Black);
                            //sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + (Vector2.One * 2) + Main.GlobalTaeEditorFontOffset, Color.Black);
                            //sb.DrawString(font, SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + Main.GlobalTaeEditorFontOffset, Color.White);
                            ImGuiDebugDrawer.DrawText(SelectedTaeAnimInfoScrollingText.Text, curAnimInfoTextPos + new Vector2(0, 4), Color.White, Color.Black, 20);
                        }

                        //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + Vector2.One, Color.Black);
                        //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos + (Vector2.One * 2), Color.Black);
                        //sb.DrawString(font, SelectedTaeAnimInfoScrollingText, curAnimInfoTextPos, Color.White);
                    }
                    finally { sb.End(); }
                }
                gd.Viewport = oldViewport;

                



            }

            if (Graph != null)
            {
                try
                {
                    Graph.Draw(gd, sb, boxTex, font, elapsedSeconds, smallFont, scrollbarArrowTex);
                }
                catch
                {

                }

            }
            else
            {
                // Draws a very, very blank graph is none is loaded:

                //var graphRect = new Rectangle(
                //        (int)MiddleSectionStartX,
                //        Rect.Top + TopMenuBarMargin + TopOfGraphAnimInfoMargin,
                //        (int)MiddleSectionWidth,
                //        Rect.Height - TopMenuBarMargin - TopOfGraphAnimInfoMargin);

                //sb.Begin();
                //sb.Draw(texture: boxTex,
                //    position: new Vector2(graphRect.X, graphRect.Y),
                //    sourceRectangle: null,
                //    color: new Color(120, 120, 120, 255),
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(graphRect.Width, graphRect.Height),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );

                //sb.Draw(texture: boxTex,
                //    position: new Vector2(graphRect.X, graphRect.Y),
                //    sourceRectangle: null,
                //    color: new Color(64, 64, 64, 255),
                //    rotation: 0,
                //    origin: Vector2.Zero,
                //    scale: new Vector2(graphRect.Width, TaeEditAnimEventGraph.TimeLineHeight),
                //    effects: SpriteEffects.None,
                //    layerDepth: 0
                //    );
                //sb.End();
            }

            Transport.Draw(gd, sb, boxTex, smallFont);

            if (AnimSwitchRenderCooldown > 0)
            {
                AnimSwitchRenderCooldown -= Main.DELTA_UPDATE;

                //float ratio = Math.Max(0, Math.Min(1, MathHelper.Lerp(0, 1, AnimSwitchRenderCooldown / AnimSwitchRenderCooldownFadeLength)));
                //sb.Begin();
                //sb.Draw(boxTex, graphRect, AnimSwitchRenderCooldownColor * ratio);
                //sb.End();
            }




            DrawDebug();




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

            if (REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME)
            {
                float scale = 1;
                var textSize = (ImGuiNET.ImGui.CalcTextSize("LOADING CUTSCENE ENTITIES...").ToXna() * scale);
                var printer = new StatusPrinter(ModelViewerBounds.TopRightCorner() + new Vector2(-(textSize.X + 32), 8), Color.Lime, Color.Black);

                printer.AppendLine("LOADING CUTSCENE ENTITIES...");

                printer.BaseScale = scale;
                printer.Draw();

                REMO_HOTFIX_REQUEST_CUT_ADVANCE_NEXT_FRAME = false;
                REMO_HOTFIX_REQUEST_CUT_ADVANCE_THIS_FRAME = true;
            }

        }
    }
}
