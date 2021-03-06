﻿using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static class MenuBar
    {
        public static TaeEditor.TaeEditorScreen Tae => Main.TAE_EDITOR;

        public static bool IsAnyMenuOpen = false;
        public static bool IsAnyMenuOpenChanged = false;
        private static bool prevIsAnyMenuOpen = false;

        public static void BuildMenuBar()
        {
            bool isAnyMenuExpanded = false;

            if (ImGui.BeginMenu("File"))
            {
                isAnyMenuExpanded = true;

                bool clickedOpen = ClickItem("Open...");
                ImGui.Separator();
                bool isRecentFilesExpanded = ImGui.BeginMenu("Recent Files");
                if (isRecentFilesExpanded)
                {
                    if (ClickItem("Clear all recent files..."))
                    {
                        DialogManager.AskForMultiChoice("Clear Recent Files",
                            "Clear all recent files?", (cancelType, answer) =>
                            {
                                if (answer == "YES")
                                {
                                    lock (Main.Config._lock_ThreadSensitiveStuff)
                                    {
                                        Main.Config.RecentFilesList.Clear();
                                    }
                                    Main.SaveConfig();
                                }
                            }, Dialog.CancelTypes.Combo_ClickTitleBarX_PressEscape, "YES", "NO");
                    }

                    ImGui.Separator();

                    try
                    {
                        string fileOpened = null;
                        lock (Main.Config._lock_ThreadSensitiveStuff)
                        {
                            foreach (var f in Main.Config.RecentFilesList)
                            {
                                if (fileOpened == null && ClickItem(f))
                                {
                                    fileOpened = f;
                                }
                            }
                        }

                        if (fileOpened != null)
                        {
                            Tae.DirectOpenFile(fileOpened);
                        }
                    }
                    catch
                    {

                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();
                bool clickedReloadGameParam = ClickItem("Reload GameParam and FMGs", enabled: Tae.IsFileOpen);
                ImGui.Separator();
                bool clickedSave = ClickItem("Save", enabled: Tae.IsFileOpen && Tae.IsModified, shortcut: "Ctrl+S");
                bool clickedSaveAs = ClickItem("Save As...", enabled: Tae.IsFileOpen, shortcut: "Ctrl+Shift+S");
                bool clickedExportTAE = ClickItem("Export *.TAE Containing The Currently Selected Animation...", enabled: Tae.SelectedTae != null);
                ImGui.Separator();
                var prevValueSaveAdditionalEventRowInfoToLegacyGames = Main.Config.SaveAdditionalEventRowInfoToLegacyGames;
                var nextValueSaveAdditionalEventRowInfoToLegacyGames = Checkbox("Save Row Data To Legacy Games",
                    prevValueSaveAdditionalEventRowInfoToLegacyGames, enabled: true,
                    shortcut: "DeS/DS1 Only");
                if (nextValueSaveAdditionalEventRowInfoToLegacyGames != prevValueSaveAdditionalEventRowInfoToLegacyGames)
                {
                    if (nextValueSaveAdditionalEventRowInfoToLegacyGames)
                    {
                        DialogManager.AskYesNo("Warning", "This option has not been tested in the long run and may cause the game to behave " +
                            "\nstrangely, or it may not. Are you sure you wish to use this option? " +
                            "\nNote: effect is reversable if you run into issues.", choice =>
                            {
                                if (choice)
                                {
                                    Main.Config.SaveAdditionalEventRowInfoToLegacyGames = true;

                                    if (GameDataManager.GameTypeUsesLegacyEmptyEventGroups)
                                    {
                                        Tae.StripExtraEventGroupsInAllLoadedFilesIfNeeded();
                                    }
                                }
                            }, allowCancel: true, enterKeyForYes: false);
                    }
                    else
                    {
                        DialogManager.AskYesNo("Warning", "Disabling this option will IMMEDIATELY REMOVE ALL of the extra row data from all " +
                            "\nanimations in anibnd files which utilized this option previously and make them all use the standard " +
                            "\nautomatic row sorting, which will PERMANENTLY save into the file when resaved. " +
                            "\nAre you sure you wish to do this?", choice =>
                            {
                                if (choice)
                                {
                                    Main.Config.SaveAdditionalEventRowInfoToLegacyGames = false;
                                    Tae.StripExtraEventGroupsInAllLoadedFilesIfNeeded();
                                }
                            }, allowCancel: true, enterKeyForYes: false);
                    }
                }
                ImGui.Separator();
                bool clickedLiveRefreshNow = ClickItem("(DS3/DS1R Only) Force Ingame Character Reload Now",
                    enabled: Tae.IsFileOpen &&
                    GameDataManager.GameType ==
                    SoulsAssetPipeline.SoulsGames.DS1R
                    || GameDataManager.GameType ==
                    SoulsAssetPipeline.SoulsGames.DS3, shortcut: "F5");
                bool liveRefreshOnSaveValue = Checkbox(
                    "(DS3/DS1R Only) Force Ingame Character Reload Upon Saving",
                    Tae.Config.LiveRefreshOnSave);
                ImGui.Separator();
                bool clickedSaveConfigManually = ClickItem("Save Config File");
                Main.DisableConfigFileAutoSave = !Checkbox("Enable Config File Autosaving", !Main.DisableConfigFileAutoSave);
                bool clickedLoadConfigManually = ClickItem("Reload Config File");
                
                ImGui.Separator();
                bool clickedExit = ClickItem("Exit");

                // Only do the interaction layer with the main window if the recent files list isn't covering it...
                if (!isRecentFilesExpanded)
                {

                    if (clickedOpen)
                        Tae.File_Open();

                    if (clickedReloadGameParam)
                    {
                        LoadingTaskMan.DoLoadingTask("FileReloadGameParam",
                            "Reloading GameParam and FMGs...", prog =>
                            {
                                GameDataManager.ReloadParams();
                                GameDataManager.ReloadFmgs();
                                Tae.Graph?.ViewportInteractor?.CurrentModel?.RescanNpcParams();
                                Tae.Graph?.ViewportInteractor?.OnScrubFrameChange();
                            }, disableProgressBarByDefault: true);
                    }

                    if (clickedSave)
                        Tae.SaveCurrentFile();

                    if (clickedSaveAs)
                        Tae.File_SaveAs();

                    if (clickedExportTAE)
                        Tae.Tools_ExportCurrentTAE();

                    if (clickedLiveRefreshNow)
                        Tae.LiveRefresh();

                    Tae.Config.LiveRefreshOnSave = liveRefreshOnSaveValue;

                    if (clickedSaveConfigManually)
                    {
                        Main.SaveConfig(isManual: true);
                    }

                    if (clickedLoadConfigManually)
                    {
                        Main.LoadConfig(isManual: true);
                    }

                    if (clickedExit)
                        Main.WinForm.Close();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                isAnyMenuExpanded = true;

                if (ClickItem("Undo", Tae.UndoMan?.CanUndo ?? false, "Ctrl+Z"))
                    Tae.UndoMan?.Undo();
                if (ClickItem("Redo", Tae.UndoMan?.CanRedo ?? false, "Ctrl+Y"))
                    Tae.UndoMan?.Redo();

                ImGui.Separator();

                if (ClickItem("Collapse All TAE Sections", Tae.IsFileOpen))
                    Tae.SetAllTAESectionsCollapsed(true);
                if (ClickItem("Expand All TAE Sections", Tae.IsFileOpen))
                    Tae.SetAllTAESectionsCollapsed(false);

                ImGui.Separator();

                if (ClickItem("Find Value...", Tae.IsFileOpen, "Ctrl+F"))
                    Tae.ShowDialogFind();

                if (ClickItem("Go To Animation ID...", Tae.IsFileOpen, "Ctrl+G"))
                    Tae.ShowDialogGotoAnimID();

                if (ClickItem("Go To Animation Section ID...", Tae.IsFileOpen, "Ctrl+H"))
                    Tae.ShowDialogGotoAnimSectionID();

                if (ClickItem("Import From Animation ID...", Tae.IsFileOpen, "Ctrl+I"))
                    Tae.ShowDialogImportFromAnimID();

                ImGui.Separator();

                if (ClickItem("Change Type of Selected Event", Tae.IsFileOpen && Tae.SingleEventBoxSelected, "F1"))
                    Tae.ChangeTypeOfSelectedEvent();

                if (ClickItem("Edit Current Animation Name...", Tae.IsFileOpen, "F2"))
                    Tae.ShowDialogChangeAnimName();

                if (ClickItem("Edit Current Animation Properties...", Tae.IsFileOpen, "F3"))
                    Tae.ShowDialogEditCurrentAnimInfo();

                if (ClickItem("Go to Referenced Event Source Animation", Tae.IsFileOpen, "F4"))
                    Tae.GoToEventSource();

                if (ClickItem("Duplicate Animation", Tae.IsFileOpen, "Insert"))
                    Tae.DuplicateCurrentAnimation();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Event Graph"))
            {
                isAnyMenuExpanded = true;

                Tae.Config.EventSnapType = EnumSelectorItem<TaeEditor.TaeConfigFile.EventSnapTypes>("Snap Events To Framerate",
                    Tae.Config.EventSnapType, new Dictionary<TaeEditor.TaeConfigFile.EventSnapTypes, string>
                {
                        { TaeEditor.TaeConfigFile.EventSnapTypes.None, "None" },
                        { TaeEditor.TaeConfigFile.EventSnapTypes.FPS30, "30 FPS (used by FromSoft)" },
                        { TaeEditor.TaeConfigFile.EventSnapTypes.FPS60, "60 FPS" },
                }, enabled: Tae.IsFileOpen);

                ImGui.Separator();

                Tae.Config.IsNewGraphVisiMode = Checkbox("Use New Graph Design", Tae.Config.IsNewGraphVisiMode);
                Tae.Config.EnableFancyScrollingStrings = Checkbox("Use Fancy Text Scrolling", Tae.Config.EnableFancyScrollingStrings);
                Tae.Config.FancyScrollingStringsScrollSpeed = FloatSlider("Fancy Text Scroll Speed", Tae.Config.FancyScrollingStringsScrollSpeed, 1, 256, "%f pixels/sec");
                ImGui.Separator();
                Tae.Config.AutoCollapseAllTaeSections = Checkbox("Start with all TAE sections collapsed", Tae.Config.AutoCollapseAllTaeSections);
                ImGui.Separator();
                Tae.Config.AutoScrollDuringAnimPlayback = Checkbox("Auto-scroll During Anim Playback", Tae.Config.AutoScrollDuringAnimPlayback);
                ImGui.Separator();
                Tae.Config.SoloHighlightEventOnHover = Checkbox("Solo Highlight Event on Hover", Tae.Config.SoloHighlightEventOnHover);
                Tae.Config.ShowEventHoverInfo = Checkbox("Show Event Info Popup When Hovering Over Event", Tae.Config.ShowEventHoverInfo);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Simulation"))
            {
                foreach (var thing in TaeEditor.TaeEventSimulationEnvironment.AllEntryDisplayNames)
                {
                    Tae.Config.EventSimulationsEnabled[thing.Key] = Checkbox(thing.Value, Tae.Config.EventSimulationsEnabled[thing.Key]);
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Animation"))
            {
                isAnyMenuExpanded = true;

                Tae.Config.LockFramerateToOriginalAnimFramerate = Checkbox(
                    "Lock to Frame Rate Defined in HKX", Tae.Config.LockFramerateToOriginalAnimFramerate,
                    shortcut: Tae.PlaybackCursor != null
                    ? $"({((int)Math.Round(Tae.PlaybackCursor.CurrentSnapFPS))} FPS)" : null);

                TaeEditor.TaePlaybackCursor.GlobalBasePlaybackSpeed = FloatSlider("Playback Speed",
                    TaeEditor.TaePlaybackCursor.GlobalBasePlaybackSpeed * 100f, 0, 100, "%.2f %%") / 100f;



                ImGui.Separator();

                Tae.Config.EnableAnimRootMotion = Checkbox(
                    "Enable Root Motion", Tae.Config.EnableAnimRootMotion);

                Tae.Config.CameraFollowsRootMotion = Checkbox(
                    "Camera Follows Root Motion Translation", Tae.Config.CameraFollowsRootMotion);

                Tae.Config.CameraFollowsRootMotionRotation = Checkbox(
                    "Camera Follows Root Motion Rotation", Tae.Config.CameraFollowsRootMotionRotation);

                Tae.Config.WrapRootMotion = Checkbox(
                    "Prevent Root Motion From Reaching End Of Grid", Tae.Config.WrapRootMotion);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("FMOD Sound"))
            {
                isAnyMenuExpanded = true;

                if (ClickItem("Retry Initialization", !FmodManager.IsInitialized))
                {
                    FmodManager.InitTest();
                    Tae.Graph?.ViewportInteractor?.LoadSoundsForCurrentModel();
                }

                if (ClickItem("STOP ALL SOUNDS", FmodManager.IsInitialized, "Escape"))
                {
                    FmodManager.StopAllSounds();
                }

                if (ImGui.BeginMenu("Load Additional Sounds", enabled: FmodManager.IsInitialized &&
                    FmodManager.MediaPath != null))
                {
                    string[] fevFiles = Directory.GetFiles(FmodManager.MediaPath, "*.fev");

                    for (int i = 0; i < fevFiles.Length; i++)
                    {
                        var shortName = Path.GetFileNameWithoutExtension(fevFiles[i]);
                        if (ClickItem(shortName, shortcut: FmodManager.LoadedFEVs.Contains(shortName) ? "(Loaded)" : null))
                        {
                            int underscoreIndex = shortName.IndexOf('_');
                            if (underscoreIndex >= 0)
                                shortName = shortName.Substring(Math.Min(underscoreIndex + 1, shortName.Length - 1));
                            FmodManager.LoadInterrootFEV(shortName);
                        }
                    }



                    ImGui.EndMenu();
                }

                ImGui.Separator();

                FmodManager.ArmorMaterial = EnumSelectorItem("Player Armor Material",
                        FmodManager.ArmorMaterial, new Dictionary<FmodManager.ArmorMaterialType, string>
                    {
                                { FmodManager.ArmorMaterialType.Plates, "Platemail" },
                                { FmodManager.ArmorMaterialType.Chains, "Chainmail" },
                                { FmodManager.ArmorMaterialType.Cloth, "Cloth" },
                                { FmodManager.ArmorMaterialType.None, "Naked" },
                    });

                if (ImGui.BeginMenu("Footstep Material"))
                {
                    foreach (var mat in FmodManager.FloorMaterialNames)
                    {
                        if (ClickItem($"Material {mat.Key:D2}", shortcut: mat.Value, shortcutColor: Color.White))
                        {
                            FmodManager.FloorMaterial = mat.Key;
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            //TODO - HELP. BETTER.

            void DoWindow(Window w)
            {
                w.IsOpen = Checkbox(w.Title, w.IsOpen);
            }

            if (ImGui.BeginMenu("Tools"))
            {
                isAnyMenuExpanded = true;

                if (ClickItem("Combo Viewer", shortcut: "F8"))
                    Tae.ShowComboMenu();

#if DEBUG
                if (ClickItem("Scan for Unused Animations", shortcut: "[DEBUG]",
                    textColor: Color.Red, shortcutColor: Color.Red))
                    Tae.Tools_ScanForUnusedAnimations();
#endif

                if (ClickItem("Downgrade Havok 2015 ANIBND(s) to 2010...", shortcut: "For DS1R/Sekiro", shortcutColor: Color.Cyan))
                    Tae.Tools_DowngradeSekiroAnibnds();

                if (ClickItem("Import all DS1:PTDE ANIBNDs to DS1R...", shortcut: "Much faster than above option,\n" +
                    "but requires an unpacked copy of both games.", shortcutColor: Color.Magenta))
                    Tae.Tools_ImportAllPTDEAnibndToDS1R();


                ImGui.Separator();

                if (ClickItem("Open Animation Importer"))
                    Tae.BringUpImporter_FBXAnim();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window"))
            {
                DoWindow(OSD.WindowEntitySettings);
                //DoWindow(OSD.WindowEditPlayerEquip); //handled in player menu
                //DoWindow(OSD.WindowHelp); //handled in help menu
                DoWindow(OSD.WindowSceneManager);
                DoWindow(OSD.WindowToolbox);

                if (OSD.EnableDebugMenu)
                {
                    ImGui.Separator();
                    DoWindow(OSD.WindowDebug);
                }
                else
                {
                    OSD.WindowDebug.IsOpen = false;
                }

                ImGui.EndMenu();
            }

            ImGui.PushStyleColor(ImGuiCol.Text, Color.Cyan.ToNVector4());
            bool helpMenu = ImGui.BeginMenu("Help");
            ImGui.PopStyleColor();
            if (helpMenu)
            {
                isAnyMenuExpanded = true;

                OSD.WindowHelp.IsOpen = Checkbox("Show Basic Help Window", OSD.WindowHelp.IsOpen, textColor: Color.White);

                ImGui.Separator();

                if (ClickItem("Souls Modding Discord Server", textColor: Color.White,
                   shortcut: "?ServerName? (https://discord.gg/mT2JJjx)", shortcutColor: Color.Cyan))
                    Process.Start("https://discord.gg/mT2JJjx");

                if (ClickItem("My Discord Server (Less Active)", textColor: Color.White,
                    shortcut: "Meowmaritus Zone (https://discord.gg/J79XMgR)", shortcutColor: Color.Cyan))
                    Process.Start("https://discord.gg/J79XMgR");

                ImGui.EndMenu();
            }

            ImGui.PushStyleColor(ImGuiCol.Text, Color.Lime.ToNVector4());
            bool supportMenu = ImGui.BeginMenu("Support Meowmaritus");
            ImGui.PopStyleColor();

            if (supportMenu)
            {
                isAnyMenuExpanded = true;

                if (ClickItem("On Patreon...", textColor: Color.Lime,
                    shortcut: "(https://www.patreon.com/Meowmaritus)", shortcutColor: Color.Lime))
                    Process.Start("https://www.patreon.com/Meowmaritus");

                if (ClickItem("On Paypal...", textColor: Color.Lime,
                    shortcut: "(https://paypal.me/Meowmaritus)", shortcutColor: Color.Lime))
                    Process.Start("https://paypal.me/Meowmaritus");

                if (ClickItem("On Ko-fi...", textColor: Color.Lime,
                    shortcut: "(https://ko-fi.com/meowmaritus)", shortcutColor: Color.Lime))
                    Process.Start("https://ko-fi.com/meowmaritus");

                ImGui.EndMenu();
            }

            IsAnyMenuOpen = isAnyMenuExpanded;

            IsAnyMenuOpenChanged = IsAnyMenuOpen != prevIsAnyMenuOpen;

            prevIsAnyMenuOpen = IsAnyMenuOpen;
        }

        private static float FloatSlider(string name, float currentValue, float min, float max, string format = "%f", float power = 1)
        {
            float v = currentValue;
            ImGui.SliderFloat(name, ref v, min, max, format, ImGuiSliderFlags.None);
            return v;
        }

        public static T EnumSelectorItem<T>(string itemName, T currentValue, Dictionary<T, string> entries = null, bool enabled = true)
            where T : Enum
        {
            if (ImGui.BeginMenu(itemName, enabled))
            {
                foreach (var kvp in entries)
                {
                    ImGui.MenuItem(kvp.Value, null, currentValue.Equals(kvp.Key));
                    if (ImGui.IsItemClicked())
                    {
                        currentValue = kvp.Key;
                        break;
                    }
                }

                ImGui.EndMenu();
            }

            return currentValue;
        }

        public static bool ClickItem(string text, bool enabled = true, string shortcut = null,
            Color? textColor = null, Color? shortcutColor = null)
        {
            if (textColor != null)
                ImGui.PushStyleColor(ImGuiCol.Text, textColor.Value.ToNVector4());
            if (shortcutColor != null)
                ImGui.PushStyleColor(ImGuiCol.TextDisabled, shortcutColor.Value.ToNVector4());

            if (shortcut == null)
                ImGui.MenuItem(text, enabled);
            else
                ImGui.MenuItem(text, shortcut, selected: false, enabled: enabled);

            if (textColor != null)
                ImGui.PopStyleColor();
            if (shortcutColor != null)
                ImGui.PopStyleColor();

            bool wasClicked = false;

            if (enabled)
                wasClicked = ImGui.IsItemClicked();

            return enabled && wasClicked;
        }

        public static int IntItem(string text, int currentValue)
        {
            int v = currentValue;
            ImGui.InputInt(text, ref v);
            return v;
        }


        public static bool Checkbox(string text, bool currentValue, bool enabled = true,
            string shortcut = null, Color? textColor = null, Color? shortcutColor = null)
        {
            bool v = currentValue;
            if (textColor != null)
                ImGui.PushStyleColor(ImGuiCol.Text, textColor.Value.ToNVector4());
            if (shortcutColor != null)
                ImGui.PushStyleColor(ImGuiCol.TextDisabled, shortcutColor.Value.ToNVector4());
            ImGui.MenuItem(text, shortcut, ref v, enabled: enabled);
            if (textColor != null)
                ImGui.PopStyleColor();
            if (shortcutColor != null)
                ImGui.PopStyleColor();
            return v;
        }

        public static bool CheckboxBig(string text, bool currentValue)
        {
            bool v = currentValue;
            ImGui.Checkbox(text, ref v);
            return v;
        }
    }
}
