using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static partial class OSD
    {
        public static class MenuBar
        {
            public static void BuildMenuBar()
            {
                var tae = Main.TAE_EDITOR;

                if (ImGui.BeginMenu("File"))
                {
                    if (ClickItem("Open..."))
                        tae.File_Open();

                    ImGui.Separator();

                    if (ImGui.BeginMenu("Recent Files"))
                    {
                        if (ClickItem("Clear all recent files..."))
                        {
                            Tools.AskQuestion("Clear Recent Files",
                                "Clear all recent files?", answer =>
                                {
                                    if (answer == "YES")
                                    {
                                        tae.Config.RecentFilesList.Clear();
                                        tae.SaveConfig();
                                    }
                                }, "YES", "NO");
                        }

                        ImGui.Separator();

                        foreach (var f in tae.Config.RecentFilesList)
                            if (ClickItem(f))
                                tae.DirectOpenFile(f);

                        ImGui.EndMenu();
                    }

                    ImGui.Separator();

                    if (ClickItem("Reload GameParam", enabled: tae.IsFileOpen))
                    {
                        LoadingTaskMan.DoLoadingTask("FileReloadGameParam", 
                            "Reloading GameParam...", prog =>
                        {
                            GameDataManager.ReloadParams();
                            tae.Graph.ViewportInteractor.OnScrubFrameChange();
                        }, disableProgressBarByDefault: true);
                    }

                    ImGui.Separator();

                    if (ClickItem("Save", enabled: tae.IsFileOpen && tae.IsModified, shortcut: "Ctrl+S"))
                        tae.SaveCurrentFile();

                    if (ClickItem("Save As...", enabled: tae.IsFileOpen, shortcut: "Ctrl+Shift+S"))
                        tae.File_SaveAs();

                    ImGui.Separator();

                    if (ClickItem("(DS3/DS1R Only) Force Ingame Character Reload Now", 
                        enabled: tae.IsFileOpen && 
                        GameDataManager.GameType == 
                        SoulsAssetPipeline.SoulsGames.DS1R
                        || GameDataManager.GameType == 
                        SoulsAssetPipeline.SoulsGames.DS3, shortcut: "F5"))
                    {
                        tae.LiveRefresh();
                    }

                    tae.Config.LiveRefreshOnSave = Checkbox(
                        "(DS3/DS1R Only) Force Ingame Character Reload Upon Saving", 
                        tae.Config.LiveRefreshOnSave);

                    ImGui.Separator();

                    if (ClickItem("Manually Save Config"))
                    {
                        tae.SaveConfig();
                        Tools.Notice("Configuration saved succesfully.");
                    }

                    ImGui.Separator();

                    if (ClickItem("Exit"))
                    {
                        Main.WinForm.Close();
                    }


                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ClickItem("Undo", tae.UndoMan?.CanUndo ?? false, "Ctrl+Z"))
                        tae.UndoMan?.Undo();
                    if (ClickItem("Redo", tae.UndoMan?.CanRedo ?? false, "Ctrl+Y"))
                        tae.UndoMan?.Redo();

                    ImGui.Separator();

                    if (ClickItem("Collapse All TAE Sections", tae.IsFileOpen))
                        tae.SetAllTAESectionsCollapsed(true);
                    if (ClickItem("Expand All TAE Sections", tae.IsFileOpen))
                        tae.SetAllTAESectionsCollapsed(false);

                    ImGui.Separator();

                    if (ClickItem("Find Value...", tae.IsFileOpen, "Ctrl+F"))
                        tae.ShowDialogFind();

                    if (ClickItem("Go To Animation ID...", tae.IsFileOpen, "Ctrl+G"))
                        tae.ShowDialogGotoAnimID();

                    if (ClickItem("Go To Animation Section ID...", tae.IsFileOpen, "Ctrl+H"))
                        tae.ShowDialogGotoAnimSectionID();

                    ImGui.Separator();

                    if (ClickItem("Change Type of Selected Event", tae.IsFileOpen && tae.SingleEventBoxSelected, "F1"))
                        tae.ChangeTypeOfSelectedEvent();

                    if (ClickItem("Edit Current Animation Name...", tae.IsFileOpen, "F2"))
                        tae.ShowDialogChangeAnimName();

                    if (ClickItem("Edit Current Animation Properties...", tae.IsFileOpen, "F3"))
                        tae.ShowDialogEditCurrentAnimInfo();

                    if (ClickItem("Go to Referenced Event Source Animation", tae.IsFileOpen, "F4"))
                        tae.GoToEventSource();

                    if (ClickItem("Duplicate Animation", tae.IsFileOpen, "Insert"))
                        tae.DuplicateCurrentAnimation();

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Event Graph"))
                {
                    tae.Config.EventSnapType = EnumSelectorItem<TaeEditor.TaeConfigFile.EventSnapTypes>("Snap Events To Framerate",
                        tae.Config.EventSnapType, new Dictionary<TaeEditor.TaeConfigFile.EventSnapTypes, string>
                    {
                        { TaeEditor.TaeConfigFile.EventSnapTypes.None, "None" },
                        { TaeEditor.TaeConfigFile.EventSnapTypes.FPS30, "30 FPS (used by FromSoft)" },
                        { TaeEditor.TaeConfigFile.EventSnapTypes.FPS60, "60 FPS" },
                    }, enabled: tae.IsFileOpen);

                    ImGui.Separator();

                    tae.Config.IsNewGraphVisiMode = Checkbox("Use New Graph Design", tae.Config.IsNewGraphVisiMode);
                    tae.Config.EnableFancyScrollingStrings = Checkbox("Use Fancy Text Scrolling", tae.Config.EnableFancyScrollingStrings);
                    tae.Config.FancyScrollingStringsScrollSpeed = FloatSlider("Fancy Text Scroll Speed", tae.Config.FancyScrollingStringsScrollSpeed, 1, 256, "%f pixels/sec");
                    ImGui.Separator();
                    tae.Config.AutoCollapseAllTaeSections = Checkbox("Start with all TAE sections collapsed", tae.Config.AutoCollapseAllTaeSections);
                    ImGui.Separator();
                    tae.Config.AutoScrollDuringAnimPlayback = Checkbox("Auto-scroll During Anim Playback", tae.Config.AutoScrollDuringAnimPlayback);
                    ImGui.Separator();
                    tae.Config.SoloHighlightEventOnHover = Checkbox("Solo Highlight Event on Hover", tae.Config.SoloHighlightEventOnHover);
                    tae.Config.ShowEventHoverInfo = Checkbox("Show Event Info Popup When Hovering Over Event", tae.Config.ShowEventHoverInfo);

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Simulation"))
                {
                    foreach (var thing in TaeEditor.TaeEventSimulationEnvironment.AllEntryDisplayNames)
                    {
                        tae.Config.EventSimulationsEnabled[thing.Key] = Checkbox(thing.Value, tae.Config.EventSimulationsEnabled[thing.Key]);
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Simulation"))
                {
                    foreach (var thing in TaeEditor.TaeEventSimulationEnvironment.AllEntryDisplayNames)
                    {
                        tae.Config.EventSimulationsEnabled[thing.Key] = Checkbox(thing.Value, tae.Config.EventSimulationsEnabled[thing.Key]);
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Window"))
                {
                    IsWindowOpen_Toolbox = Checkbox("Toolbox", IsWindowOpen_Toolbox);
                    IsWindowOpen_SceneManager = Checkbox("Scene Manager", IsWindowOpen_SceneManager);
                    if (EnableDebugMenu)
                    {
                        IsWindowOpen_Debug = Checkbox("Debug", IsWindowOpen_Debug);
                    }

                    ImGui.EndMenu();
                }
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

            public static bool ClickItem(string text, bool enabled = true, string shortcut = null)
            {
                if (shortcut == null)
                    ImGui.MenuItem(text, enabled);
                else
                    ImGui.MenuItem(text, shortcut, selected: false, enabled: enabled);

                return enabled && ImGui.IsItemClicked();
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
        }
    }
}
