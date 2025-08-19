using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DSAnimStudio.ImguiOSD.Window;

namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static void BuildMenuBar_010_Edit(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu("Edit"))
            {
                isAnyMenuExpanded = true;

                var nextLocalUndo = Tae.CurrentAnimUndoMan?.GetNextUndoName();
                nextLocalUndo = (nextLocalUndo != null && Tae.CurrentAnimUndoMan?.CanUndo == true) ? $" ({nextLocalUndo})" : "";
                
                var nextLocalRedo = Tae.CurrentAnimUndoMan?.GetNextRedoName();
                nextLocalRedo = (nextLocalRedo != null && Tae.CurrentAnimUndoMan?.CanRedo == true) ? $" ({nextLocalRedo})" : "";
                
                if (ClickItem($"Undo{nextLocalUndo}", Tae.CurrentAnimUndoMan?.CanUndo ?? false, "Ctrl+Z"))
                    Tae.CurrentAnimUndoMan?.Undo();
                if (ClickItem($"Redo{nextLocalRedo}", Tae.CurrentAnimUndoMan?.CanRedo ?? false, "Ctrl+Y"))
                    Tae.CurrentAnimUndoMan?.Redo();
                
                ImGui.Separator();

                var nextGlobalUndo = Tae.GlobalUndoMan?.GetNextUndoName();
                nextGlobalUndo = (nextGlobalUndo != null && Tae.GlobalUndoMan?.CanUndo == true) ? $" ({nextGlobalUndo})" : "";
                
                var nextGlobalRedo = Tae.GlobalUndoMan?.GetNextRedoName();
                nextGlobalRedo = (nextGlobalRedo != null && Tae.GlobalUndoMan?.CanRedo == true) ? $" ({nextGlobalRedo})" : "";

                if (ClickItem($"Undo [Global] Action{nextGlobalUndo}", Tae.GlobalUndoMan?.CanUndo ?? false, "Ctrl+Shift+Z"))
                {
                    DialogManager.AskYesNo("WARNING",
                        "All changes on all animations since the [Global] operation was " +
                        "\nperformed will be lost if you undo the global operation. " +
                        "\nAre you sure you wish to do this?",
                        choice =>
                        {
                            if (choice)
                            {
                                Tae.GlobalUndoMan?.Undo();
                            }
                        }, allowCancel: true, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                }
                if (ClickItem($"Redo [Global] Action{nextGlobalRedo}", Tae.GlobalUndoMan?.CanRedo ?? false, "Ctrl+Shift+Y"))
                {
                    DialogManager.AskYesNo("WARNING",
                        "All changes on all animations since the [Global] operation was " +
                        "\nundone will be lost if you redo the global operation. " +
                        "\nAre you sure you wish to do this?",
                        choice =>
                        {
                            if (choice)
                            {
                                Tae.GlobalUndoMan?.Redo();
                            }
                        }, allowCancel: true, inputFlags: Dialog.InputFlag.EscapeKeyToCancel | Dialog.InputFlag.TitleBarXToCancel);
                    
                }

                ImGui.Separator();

                if (ClickItem("Collapse All TAEs", Tae.IsFileOpen))
                    Tae.SetAllTAESectionsCollapsed(true);
                if (ClickItem("Expand All TAEs", Tae.IsFileOpen))
                    Tae.SetAllTAESectionsCollapsed(false);

                ImGui.Separator();

                if (ClickItem("Find Value...", Tae.IsFileOpen, "Ctrl+F"))
                    Tae.ShowDialogFind();

                if (ClickItem("Go To Animation ID...", Tae.IsFileOpen, "Ctrl+G"))
                    Tae.ShowDialogGotoAnimID();

                if (ClickItem("Go To TAE ID...", Tae.IsFileOpen, "Ctrl+H"))
                    Tae.ShowDialogGotoAnimSectionID();

                if (ClickItem("Import From Animation ID...", Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Ctrl+I"))
                    Tae.ShowDialogImportFromAnimID();

                ImGui.Separator();

                if (ClickItem("Change Type of Selected Action", Tae.IsFileOpen && Tae.SingleEventBoxSelected && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F1"))
                    Tae.ChangeTypeOfEvent(Tae.InspectorAction);

                if (ClickItem("Edit Current Animation Name...", Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F2"))
                    Tae.ShowDialogChangeAnimName(Tae.Proj, Tae.SelectedAnimCategory, Tae.SelectedAnim);

                if (ClickItem("Edit Current Animation Category Name...", Tae.IsFileOpen, "Shift+F2"))
                    Tae.ShowDialogChangeAnimCategoryName(Tae.SelectedAnimCategory);

                if (ClickItem("Edit Current Animation Properties...", Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F3"))
                    Tae.ShowDialogEditCurrentAnimInfo();

                if (ClickItem("Edit Current Animation Category Properties...", Tae.IsFileOpen, "Shift+F3"))
                    Tae.ShowDialogEditAnimCategoryProperties(Tae.SelectedAnimCategory);

                if (ClickItem("Edit Project Root TAE Properties...", Tae.IsFileOpen, "Ctrl+Shift+F3"))
                    Tae.ShowDialogEditRootTaeProperties();

                if (ClickItem("Go to Referenced Action Source Animation", Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "F4"))
                    Tae.GoToEventSource();

                if (ClickItem("Duplicate Animation", Tae.IsFileOpen && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Insert"))
                    Tae.ShowDialogDuplicateCurrentAnimation();
                
                if (ClickItem("Duplicate Animation Category", Tae.IsFileOpen, "Shift+Insert"))
                    Tae.ShowDialogDuplicateToNewTaeSection(Tae.Proj, Tae.SelectedAnimCategory);

                ImGui.Separator();

                if (ClickItem("Sort Tracks", Tae.IsFileOpen, "F6"))
                    Tae.ResortTracks_Anim();

                if (ClickItem("Regen Track Names", Tae.IsFileOpen, "F7"))
                    Tae.RegenTrackNames_Anim();

                if (ClickItem("[Global] Sort Tracks (All Animations)", Tae.IsFileOpen, "Shift+F6"))
                    Tae.ResortTracks_Proj();

                if (ClickItem("[Global] Regen Track Names (All Animations)", Tae.IsFileOpen, "Shift+F7"))
                    Tae.RegenTrackNames_Proj();

                ImGui.Separator();
                
                if (ClickItem("Mute Selected Action(s)", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0 
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "1"))
                    Tae.Graph.InputMan.ToggleMuteOnSelectedActions(false);
                if (ClickItem("Mute Selected Action(s) Uniformly", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+1"))
                    Tae.Graph.InputMan.ToggleMuteOnSelectedActions(true);

                if (ClickItem("Solo Selected Action(s)", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "2"))
                    Tae.Graph.InputMan.ToggleSoloOnSelectedActions(false);
                if (ClickItem("Solo Selected Action(s) Uniformly", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+2"))
                    Tae.Graph.InputMan.ToggleSoloOnSelectedActions(true);
                
                if (ClickItem("Reset Mute/Solo On All Actions", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+3"))
                    Tae.Graph.InputMan.ClearAllSoloAndMute();
                
                if (ClickItem("Toggle StateInfo Of Selected Action(s)", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "4"))
                    Tae.Graph.InputMan.ToggleStateInfoOnSelectedActions(false);
                if (ClickItem("Toggle StateInfo Of Selected Action(s) Uniformly", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+4"))
                    Tae.Graph.InputMan.ToggleStateInfoOnSelectedActions(true);

                ImGui.Separator();

                if (ClickItem("Set Custom Color Of Selected Action(s)", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "C"))
                    Tae.SetColorOfSelectedActions(isClearing: false);

                if (ClickItem("Clear Custom Color Of Selected Action(s)", Tae.IsFileOpen && Tae.NewSelectedActions.Count > 0
                    && !Tae.Graph.IsGhostGraph && Tae.SelectedAnim?.IS_DUMMY_ANIM == false, "Shift+C"))
                    Tae.SetColorOfSelectedActions(isClearing: true);

                ImGui.EndMenu();
            }

        }
    }
}
