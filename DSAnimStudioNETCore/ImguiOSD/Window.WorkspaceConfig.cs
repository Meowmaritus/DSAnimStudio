using DSAnimStudio.TaeEditor;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class WorkspaceConfig : Window
        {
            public TaeViewportInteractor.TaeEntityType EntityType;
            public string EntityName => Scene.MainModel?.Name ?? "?EntityName?";

            private object _lock = new object();

            private Dictionary<int, string> StateInfoSelectConfig_Names = new Dictionary<int, string>();
            private Dictionary<int, bool> StateInfoSelectConfig_Enabled = new Dictionary<int, bool>();

            public Dictionary<int, string> GetStateInfoSelectConfig_Names()
            {
                var result = new Dictionary<int, string>();
                lock (_lock)
                {
                    result = StateInfoSelectConfig_Names.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }

            public Dictionary<int, bool> GetStateInfoSelectConfig_Enabled()
            {
                var result = new Dictionary<int, bool>();
                lock (_lock)
                {
                    result = StateInfoSelectConfig_Enabled.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
                return result;
            }

            public void SetStateInfoSelectConfig_Names(Dictionary<int, string> selectConfig)
            {
                lock (_lock)
                {
                    StateInfoSelectConfig_Names = selectConfig.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            public void SetStateInfoSelectConfig_Enabled(Dictionary<int, bool> selectConfig)
            {
                lock (_lock)
                {
                    StateInfoSelectConfig_Enabled = selectConfig.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }

            private static string GetEntityDispText(TaeViewportInteractor.TaeEntityType entityType, string entityName)
            {
                string namePart = (!string.IsNullOrWhiteSpace(entityName) ? $"[{entityName}]" : "");
                switch (entityType)
                {
                    case TaeViewportInteractor.TaeEntityType.NONE: return "No Entity";
                    case TaeViewportInteractor.TaeEntityType.PC: return $"Player{namePart}";
                    case TaeViewportInteractor.TaeEntityType.NPC: return $"NPC Character{namePart}";
                    case TaeViewportInteractor.TaeEntityType.OBJ: return $"Object{namePart}";
                    case TaeViewportInteractor.TaeEntityType.PARTS: return $"Animated Player Equipment{namePart}";
                    case TaeViewportInteractor.TaeEntityType.REMO: return $"DS1 Cutscene{namePart}";
                    default: return "Invalid Entity Type";
                }
            }
            public override string Title => $"Entity Utils - {GetEntityDispText(EntityType, EntityName)}";
            public override string ImguiTag => $"{nameof(Window)}.{nameof(WorkspaceConfig)}";

            private static string[] BehaviorHitboxSrcEnum_Names = new string[]
            {
                "Body",
                "Right Weapon",
                "Left Weapon",
            };
            private static List<ParamData.AtkParam.DummyPolySource> BehaviorHitboxSrcEnum_Values = new List<ParamData.AtkParam.DummyPolySource>
            {
                ParamData.AtkParam.DummyPolySource.Body,
                ParamData.AtkParam.DummyPolySource.RightWeapon0,
                ParamData.AtkParam.DummyPolySource.LeftWeapon0,
            };

            protected override void BuildContents()
            {
                if (false && ImGui.TreeNode("State Info Toggle"))
                {
                    int i = 0;
                    lock (_lock)
                    {
                        foreach (var kvp in StateInfoSelectConfig_Names)
                        {
                            bool isChecked = StateInfoSelectConfig_Enabled.ContainsKey(kvp.Key) ? StateInfoSelectConfig_Enabled[kvp.Key] : true;
                            ImGui.Checkbox(!string.IsNullOrWhiteSpace(kvp.Value) ? $"{kvp.Key} - {kvp.Value}" : $"{kvp.Key}"
                                + $"###EntitySettings_StateInfoSelect_{kvp.Key}", ref isChecked);
                            StateInfoSelectConfig_Enabled[kvp.Key] = isChecked;
                            ImGui.SameLine();
                            ImGui.Button($"Rename###EntitySettings_StateInfoSelect_{kvp.Key}_RenameButton", new System.Numerics.Vector2(ImGui.GetWindowWidth() - 30 - 80, 25));
                            if (ImGui.IsItemClicked())
                            {
                                int captureKey = kvp.Key;
                                string captureValue = kvp.Value;
                                ImguiOSD.DialogManager.AskForInputString("Rename StateInfo", $"Enter new name for StateInfo {kvp.Key}:", "", result =>
                                {
                                    lock (_lock)
                                    {
                                        StateInfoSelectConfig_Names[captureKey] = result;
                                    }
                                    GameData.SaveProjectJson();
                                }, canBeCancelled: true, startingText: captureValue ?? "");
                            }

                            ImGui.SameLine(ImGui.GetWindowWidth() - 30);
                            ImGui.Button($"×###EntitySettings_StateInfoSelect_{kvp.Key}_DeleteButton");
                            if (ImGui.IsItemClicked())
                            {
                                int captureKey = kvp.Key;
                                string captureValue = kvp.Value;
                                ImguiOSD.DialogManager.AskYesNo("Delete StateInfo?", $"Are you sure you wish to delete StateInfo {captureKey}?", result =>
                                {
                                    lock (_lock)
                                    {
                                        if (result)
                                        {
                                            if (StateInfoSelectConfig_Names.ContainsKey(captureKey))
                                                StateInfoSelectConfig_Names.Remove(captureKey);
                                            if (StateInfoSelectConfig_Enabled.ContainsKey(captureKey))
                                                StateInfoSelectConfig_Enabled.Remove(captureKey);
                                        }
                                    }
                                    GameData.SaveProjectJson(silent: true);
                                }, allowCancel: true);
                            }
                        }

                        ImGui.Button("＋###EntitySettings_StateInfoSelect_AddNew");
                        if (ImGui.IsItemClicked())
                        {
                            ImguiOSD.DialogManager.AskForInputString("Add New StateInfo", $"Enter ID for the new StateInfo to add:", "", result =>
                            {
                                var digitString = result.ExtractDigits();
                                if (digitString.Length > 0)
                                {
                                    int stateInfoID = int.Parse(digitString);
                                    bool error_alreadyExists = false;
                                    lock (_lock)
                                    {
                                        if (StateInfoSelectConfig_Enabled.ContainsKey(stateInfoID))
                                        {
                                            error_alreadyExists = true;
                                        }
                                        else
                                        {
                                            StateInfoSelectConfig_Enabled.Add(stateInfoID, true);
                                            StateInfoSelectConfig_Names.Add(stateInfoID, "");
                                        }
                                    }

                                    if (error_alreadyExists)
                                        NotificationManager.PushNotification($"Cannot add StateInfo - ID {stateInfoID} already exists.");
                                    else
                                        GameData.SaveProjectJson(silent: true);
                                }
                                else
                                {
                                    NotificationManager.PushNotification($"Cannot add StateInfo - Unable to parse an integer ID from given string '{result}'.");
                                }
                                

                                
                            }, canBeCancelled: true);
                        }
                    }
                    ImGui.TreePop();
                }
                

                ImGui.Separator();

                var entityType = Tae.Graph?.ViewportInteractor?.EntityType;
                if (entityType == TaeViewportInteractor.TaeEntityType.NPC)
                {
                    ImGui.Button("Load Additional Texture File(s)...");
                    if (ImGui.IsItemClicked())
                        Tae.BrowseForMoreTextures();

                    var mdl = Tae.Graph?.ViewportInteractor?.CurrentModel;

                    if (mdl != null)
                    {
                        if (ImGui.TreeNode("NPC Param Selection"))
                        {

                            lock (mdl._lock_NpcParams)
                            {
                                foreach (var npc in mdl.PossibleNpcParams)
                                {



                                    bool oldSelected = npc == mdl.NpcParam;

                                    var selected = MenuBar.CheckboxBig(npc.GetDisplayName(), oldSelected);

                                    ImGui.Indent();
                                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 1, 1, 1));
                                    {

                                        ImGui.Text($"BehaviorVariationID: {npc.BehaviorVariationID}");

                                        if (mdl.NpcMaterialNamesPerMask.Any(kvp => kvp.Key >= 0))
                                        {

                                            ImGui.Text($"Meshes Visible:");

                                            ImGui.Indent();
                                            {
                                                foreach (var kvp in mdl.NpcMaterialNamesPerMask)
                                                {
                                                    if (kvp.Key < 0)
                                                        continue;

                                                    if (mdl.NpcMasksEnabledOnAllNpcParams.Contains(kvp.Key))
                                                        continue;

                                                    if (npc.DrawMask[kvp.Key])
                                                    {
                                                        foreach (var v in kvp.Value)
                                                            ImGui.BulletText(v);

                                                    }
                                                }
                                            }
                                            ImGui.Unindent();
                                        }
                                    }
                                    ImGui.PopStyleColor();
                                    ImGui.Unindent();

                                    if (selected != oldSelected)
                                    {
                                        mdl.NpcParam = npc;
                                        npc.ApplyToNpcModel(mdl);
                                    }
                                }
                            }
                            ImGui.TreePop();
                        }
                    }

                    ImGui.Separator();

                    ImGui.Button("Open NPC Model Importer");
                    if (ImGui.IsItemClicked())
                        Tae.BringUpImporter_FLVER2();
                }
                else if (entityType == TaeViewportInteractor.TaeEntityType.OBJ)
                {
                    ImGui.Button("Load Additional Texture File(s)...");
                    if (ImGui.IsItemClicked())
                        Tae.BrowseForMoreTextures();
                    
                }
                else if (entityType == TaeViewportInteractor.TaeEntityType.PC)
                {
                    OSD.WindowEditPlayerEquip.IsOpen = MenuBar.CheckboxBig("Show Player Equipment Editor Window", OSD.WindowEditPlayerEquip.IsOpen);

                    if (Tae?.Graph?.ViewportInteractor?.EventSim != null)
                    {
                        var currentHitViewSource = Tae.Config.HitViewDummyPolySource;

                        int currentHitViewSourceIndex = BehaviorHitboxSrcEnum_Values.IndexOf(currentHitViewSource);
                        ImGui.ListBox("Behavior / Hitbox Source", ref currentHitViewSourceIndex, BehaviorHitboxSrcEnum_Names, BehaviorHitboxSrcEnum_Names.Length);
                        var newHitViewSource = currentHitViewSourceIndex >= 0 ? BehaviorHitboxSrcEnum_Values[currentHitViewSourceIndex] : ParamData.AtkParam.DummyPolySource.Body;

                        if (currentHitViewSource != newHitViewSource)
                        {
                            lock (Tae.Graph._lock_EventBoxManagement)
                            {
                                Tae.Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Tae.Graph.EventBoxes);
                                Tae.Config.HitViewDummyPolySource = newHitViewSource;
                                Tae.Graph.ViewportInteractor.EventSim.OnNewAnimSelected(Tae.Graph.EventBoxes);
                                Tae.Graph.ViewportInteractor.OnScrubFrameChange();
                            }
                        }
                    }
                
                    if (Tae?.Graph != null)
                    {
                        
                    }
                
                }
                else if (entityType == TaeViewportInteractor.TaeEntityType.REMO)
                {
                    RemoManager.EnableRemoCameraInViewport = MenuBar.CheckboxBig("Show Cutscene Camera View", RemoManager.EnableRemoCameraInViewport);
                    RemoManager.EnableDummyPrims = MenuBar.CheckboxBig("Enable Dummy Node Helpers", RemoManager.EnableDummyPrims);
                    //Main.Config.LockAspectRatioDuringRemo = MenuBar.CheckboxBig("Lock Aspect Ratio to 16:9", Main.Config.LockAspectRatioDuringRemo);

                    ImGui.Button("Preview Full Cutscene With Streamed Audio");
                    if (ImGui.IsItemClicked())
                    {
                        RemoManager.StartFullPreview();
                    }
                }
                else
                {
                    ImGui.Text("No entity loaded.");
                }


                ImGui.Separator();;

                string[] viewportStatusTypes = Enum.GetNames(typeof(TaeConfigFile.ViewportStatusTypes));
                var viewportStatusTypeValues = Enum.GetValues<TaeConfigFile.ViewportStatusTypes>().ToList();
                int curIndex = viewportStatusTypeValues.IndexOf(Main.Config.ShowStatusInViewport);
                ImguiOSD.Tools.FancyComboBox("Viewport Status Type", ref curIndex, viewportStatusTypes);
                if (curIndex < 0)
                    curIndex = 1;
                Main.Config.ShowStatusInViewport = viewportStatusTypeValues[curIndex];

                if (ImGui.TreeNode("STATUS###WorkspaceConfig_Status"))
                {
                    Tae?.Graph?.ViewportInteractor?.BuildImGuiStatus();

                    ImGui.TreePop();
                }
            }
        }
    }
}
