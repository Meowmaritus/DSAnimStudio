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
        public class EntitySettings : Window
        {
            public override string Title => "Entity Settings";

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

                    if (Tae.Graph?.ViewportInteractor?.EventSim != null)
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
                }
                else if (entityType == TaeViewportInteractor.TaeEntityType.REMO)
                {
                    RemoManager.EnableRemoCameraInViewport = MenuBar.CheckboxBig("Show Cutscene Camera View", RemoManager.EnableRemoCameraInViewport);
                    RemoManager.EnableDummyPrims = MenuBar.CheckboxBig("Enable Dummy Node Helpers", RemoManager.EnableDummyPrims);
                    Main.Config.LockAspectRatioDuringRemo = MenuBar.CheckboxBig("Lock Aspect Ratio to 16:9", Main.Config.LockAspectRatioDuringRemo);

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
            }
        }
    }
}
