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
        public class SceneManager : Window
        {
            public override string Title => "Scene Explorer";
            protected override void BuildContents()
            {
                lock (Scene._lock_ModelLoad_Draw)
                {
                    void DoMesh(Model mdl, NewMesh mesh, string meshName)
                    {
                        if (mesh == null)
                            return;

                        if (meshName != null)
                        {
                            if (!ImGui.TreeNode(meshName))
                                return;
                        }



                        foreach (var sm in mesh.Submeshes)
                        {
                            bool isMeshGrayedOut = sm.ModelMaskIndex >= 0 && sm.ModelMaskIndex < mesh.DrawMask.Length && !mesh.DrawMask[sm.ModelMaskIndex];
                            ImGui.PushStyleColor(ImGuiCol.Text, !isMeshGrayedOut ? new System.Numerics.Vector4(1, 1, 1, 1)
                                : new System.Numerics.Vector4(0.75f, 0.25f, 0.25f, 1));
                            ImGui.PushStyleColor(ImGuiCol.TextDisabled, isMeshGrayedOut ? new System.Numerics.Vector4(1, 0, 0, 1)
                                : new System.Numerics.Vector4(0, 1, 0, 1));
                            sm.IsVisible = MenuBar.Checkbox(sm.FullMaterialName, sm.IsVisible, enabled: true,
                                shortcut: (sm.ModelMaskIndex >= 0 ? $"[Mask {(sm.ModelMaskIndex)}]" : null));
                            ImGui.PopStyleColor(2);
                        }

                        if (meshName != null)
                            ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Models"))
                    {
                        GFX.HideFLVERs = !MenuBar.Checkbox("Render Models", !GFX.HideFLVERs);

                        ImGui.Separator();

                        foreach (var mdl in Scene.Models)
                        {



                            if (ImGui.TreeNode(mdl.Name))
                            {
                                var maskDict = mdl.GetMaterialNamesPerMask();


                                foreach (var kvp in maskDict)
                                {
                                    if (kvp.Key >= 0)
                                        mdl.DefaultDrawMask[kvp.Key] = MenuBar.Checkbox($"Mask {kvp.Key}", mdl.DefaultDrawMask[kvp.Key]);
                                }

                                ImGui.Separator();

                                DoMesh(mdl, mdl.MainMesh, null);

                                if (mdl.ChrAsm != null)
                                {
                                    ImGui.Separator();
                                    DoMesh(mdl, mdl.ChrAsm.HeadMesh, "Head");
                                    DoMesh(mdl, mdl.ChrAsm.BodyMesh, "Body");
                                    DoMesh(mdl, mdl.ChrAsm.ArmsMesh, "Arms");
                                    DoMesh(mdl, mdl.ChrAsm.LegsMesh, "Legs");
                                    DoMesh(mdl, mdl.ChrAsm.FaceMesh, "Naked Body");
                                    DoMesh(mdl, mdl.ChrAsm.FacegenMesh, "Face");
                                }

                                ImGui.TreePop();
                            }


                        }
                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("Helpers"))
                    {
                        DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone] =
                            MenuBar.Checkbox("Bone Lines",
                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                            enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBone);

                        DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.FlverBone] =
                            MenuBar.Checkbox("Bone Names",
                            DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.FlverBone],
                            enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBone);

                        DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] =
                            MenuBar.Checkbox("Bone Boxes",
                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox],
                            enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBoneBoundingBox);

                        DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] =
                            MenuBar.Checkbox("DummyPoly",
                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                            enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperDummyPoly);

                        DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.DummyPoly] =
                            MenuBar.Checkbox("DummyPoly IDs",
                            DBG.CategoryEnableNameDraw[DebugPrimitives.DbgPrimCategory.DummyPoly],
                            enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperDummyPoly);

                        NewDummyPolyManager.ShowGlobalIDOffset = MenuBar.Checkbox("Show c0000 Weapon Global \nDummyPoly ID Values (10000+)", NewDummyPolyManager.ShowGlobalIDOffset);

                        DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent] =
                            MenuBar.Checkbox("FMOD Sound Events",
                            DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent],
                            enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperSoundEvent);

                        ImGui.Separator();

                        Tae.Config.DbgPrimXRay = MenuBar.Checkbox("Helper X-Ray Mode", Tae.Config.DbgPrimXRay);

                        ImGui.TreePop();
                    }
                }

            }
        }
    }
}
