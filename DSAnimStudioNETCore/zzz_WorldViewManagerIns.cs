using DSAnimStudio.GFXShaders;
using DSAnimStudio.ImguiOSD;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public class zzz_WorldViewManagerIns : IDisposable
    {
        private bool _disposed = false;
        public void Dispose()
        {
            if (!_disposed)
            {
                foreach (var wv in _worldViews)
                    wv.Dispose();
                _worldViews?.Clear();
                _worldViews = null;
                _disposed = true;
            }
        }

        public zzz_DocumentIns ParentDocument;
        public zzz_WorldViewManagerIns(zzz_DocumentIns parentDocument)
        {
            ParentDocument = parentDocument;
        }


        public List<WorldView> GetWorldViewList()
        {
            List<WorldView> result = null;
            lock (_lock)
            {
                result = _worldViews.ToList();
            }
            return result;
        }

        public void SetWorldViewList(List<WorldView> list)
        {
            lock (_lock)
            {
                _worldViews = list.ToList();
            }
        }

        private object _lock = new object();
        public WorldView CurrentView
        {
            get
            {
                WorldView result = null;
                lock (_lock)
                {
                    if (_worldViewIndexForImgui < 0)
                        _worldViewIndexForImgui = 0;
                    if (_worldViewIndexForImgui >= _worldViews.Count)
                        _worldViewIndexForImgui = _worldViews.Count - 1;
                    result = _worldViews[_worldViewIndexForImgui];
                }
                return result;
            }
        }
        private List<WorldView> _worldViews = new List<WorldView>()
        {
            new WorldView("Default"),
            //new WorldView("Test 1"),
            //new WorldView("Test 2"),
        };
        private string[] _worldViewNamesForImgui = new string[] { "Default" };
        private int _worldViewIndexForImgui = 0;

        public void DrawImguiPanel(ref bool anyFieldFocused)
        {
            ImGui.Text("Cameras:");

            bool isSave = false;

            lock (_lock)
            {
                if (_worldViewIndexForImgui < 0)
                    _worldViewIndexForImgui = 0;
                if (_worldViewIndexForImgui >= _worldViews.Count)
                    _worldViewIndexForImgui = _worldViews.Count - 1;
                var current = _worldViews[_worldViewIndexForImgui];

                int index = 0;
                foreach (var view in _worldViews)
                {
                    var selected = (view == current);
                    var oldSelected = selected;
                    //ImGui.PushItemWidth(ImGui.GetWindowWidth() - 30 - 50);
                    ImGui.Selectable($"{view.Name}###WorldViewManager_View{index}", ref selected, ImGuiSelectableFlags.None, new System.Numerics.Vector2(ImGui.GetWindowWidth() - 30 - 80, 25));
                    //ImGui.PopItemWidth();

                    ImGui.SameLine(ImGui.GetWindowWidth() - 30 - 54);
                    ImGui.Button("Rename");
                    if (ImGui.IsItemClicked())
                    {
                        ImguiOSD.DialogManager.AskForInputString("Rename Camera", $"Enter new name for camera '{view.Name}':", view.Name, result =>
                        {
                            lock (_lock)
                            {
                                view.Name = result;
                            }
                            ParentDocument.GameData.SaveProjectJson();
                        }, checkError: null, canBeCancelled: true, startingText: view.Name);
                    }

                    ImGui.SameLine(ImGui.GetWindowWidth() - 30);
                    ImGui.Button("×");
                    if (ImGui.IsItemClicked())
                    {
                        ImguiOSD.DialogManager.AskYesNo("Delete Camera?", $"Are you sure you wish to delete camera '{view.Name}'?", result =>
                        {
                            lock (_lock)
                            {
                                if (result)
                                {
                                    _worldViews.Remove(view);
                                    if (_worldViewIndexForImgui < 0)
                                        _worldViewIndexForImgui = 0;
                                    if (_worldViewIndexForImgui >= _worldViews.Count)
                                        _worldViewIndexForImgui = _worldViews.Count - 1;
                                }
                            }

                            ParentDocument.GameData.SaveProjectJson(silent: true);
                        }, allowCancel: true);
                    }


                    if (selected)
                    {
                        _worldViewIndexForImgui = index;
                        current = view;
                        if (!oldSelected)
                            isSave = true;
                    }
                    index++;
                }


                ImGui.Button("＋");
                if (ImGui.IsItemClicked())
                {
                    ImguiOSD.DialogManager.AskForInputString("Add Camera", $"Enter name for new camera to add:", "", result =>
                    {
                        lock (_lock)
                        {
                            _worldViews.Add(new WorldView(result));
                            _worldViewIndexForImgui = _worldViews.Count - 1;
                        }

                        ParentDocument.GameData.SaveProjectJson(silent: true);
                    }, checkError: null, canBeCancelled: true);
                }
                ImGui.Separator();


                ImGui.Text("Selected Camera's Settings:");

                float newFov = GFX.CurrentWorldView.ProjectionVerticalFoV;

                ImGui.SliderFloat("Vertical FOV", ref newFov, 1, 160, newFov <= 1.0001f ? "Orthographic" : "%.0f°");
                anyFieldFocused |= ImGui.IsItemFocused();
                if (newFov != GFX.CurrentWorldView.ProjectionVerticalFoV)
                {
                    newFov = (float)Math.Round(newFov);

                    if (newFov < 1)
                        newFov = 1;

                    if (newFov > 160)
                        newFov = 160;

                    GFX.CurrentWorldView.ProjectionVerticalFoV = newFov;
                }

                
                //GFX.CurrentWorldView.ProjectionVerticalFoV = (float)Math.Round(GFX.CurrentWorldView.ProjectionVerticalFoV);

                bool lockAspect = GFX.CurrentWorldView.LockAspectRatioDuringRemo;
                bool lockAspectPrev = lockAspect;
                ImGui.Checkbox("Lock to 16:9 For Cutscenes", ref lockAspect);
                anyFieldFocused |= ImGui.IsItemFocused();
                GFX.CurrentWorldView.LockAspectRatioDuringRemo = lockAspect;
                if (lockAspect != lockAspectPrev)
                {
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }

                ImGui.Checkbox("Snap Cam to 45° Angles", ref GFX.CurrentWorldView.AngleSnapEnable);
                anyFieldFocused |= ImGui.IsItemFocused();
                ImGui.Checkbox("Show Cam Pivot Indicator Cube", ref GFX.CurrentWorldView.PivotPrimIsEnabled);
                anyFieldFocused |= ImGui.IsItemFocused();

                float nearClipDist = GFX.CurrentWorldView.ProjectionNearClipDist;
                float farClipDist = GFX.CurrentWorldView.ProjectionFarClipDist;

                if (ParentDocument.GameRoot.GameTypeIsGiant)
                {
                    nearClipDist *= 10;
                    farClipDist *= 10;
                }

                float prevNearClipDist = nearClipDist;
                float prevFarClipDist = farClipDist;

                ImGui.SliderFloat("Near Clip Dist", ref nearClipDist, 0.001f, 1);
                anyFieldFocused |= ImGui.IsItemFocused();
                OSD.TooltipManager_Scene.DoTooltip("Near Clipping Distance",
                    "Distance for the near clipping plane. " +
                    "\nSetting it too high will cause geometry to disappear when near the camera. " +
                    "\nSetting it too low will cause geometry to flicker or render with " +
                    "the wrong depth when very far away from the camera.");

                ImGui.SliderFloat("Far Clip Dist", ref farClipDist, 1000, 100000);
                anyFieldFocused |= ImGui.IsItemFocused();
                OSD.TooltipManager_Scene.DoTooltip("Far Clipping Distance",
                    "Distance for the far clipping plane. " +
                    "\nSetting it too low will cause geometry to disappear when far from the camera. " +
                    "\nSetting it too high will cause geometry to flicker or render with " +
                    "the wrong depth when near the camera.");

                if (nearClipDist != prevNearClipDist)
                {
                    if (ParentDocument.GameRoot.GameTypeIsGiant)
                        nearClipDist /= 10;
                    GFX.CurrentWorldView.ProjectionNearClipDist = nearClipDist;
                }

                if (farClipDist != prevFarClipDist)
                {
                    if (ParentDocument.GameRoot.GameTypeIsGiant)
                        farClipDist /= 10;
                    GFX.CurrentWorldView.ProjectionFarClipDist = farClipDist;
                }

                ImGui.Button("Reset This Camera's Settings");
                if (ImGui.IsItemClicked())
                {
                    GFX.CurrentWorldView.ProjectionVerticalFoV = 43;
                    GFX.CurrentWorldView.LockAspectRatioDuringRemo = true;
                    GFX.CurrentWorldView.AngleSnapEnable = false;
                    GFX.CurrentWorldView.PivotPrimIsEnabled = false;
                    GFX.CurrentWorldView.ProjectionNearClipDist = 0.1f;
                    GFX.CurrentWorldView.ProjectionFarClipDist = 10000;
                }

                ImGui.Separator();

                ImGui.Text("Grid Settings:");

                ImGui.Checkbox("Show Grid", ref GFX.CurrentWorldView.ShowGrid_New3D);


                if (GFX.CurrentWorldView.ShowGrid_New3D && Main.IsDebugBuild)
                {
                    if (ImGui.TreeNode("[DEBUG - NEW GRID 3D]"))
                    {

                        lock (DBG._lock_NewGrid3D)
                        {
                            if (DBG.NewGrid3D == null)
                                DBG.NewGrid3D = new NewGrid3D();
                            var grid = DBG.NewGrid3D;

                            ImGui.Checkbox(nameof(grid.AutoGenEntries), ref grid.AutoGenEntries);

                            Tools.EnumPicker($"{nameof(grid.AxisType)}", ref grid.AxisType);
                            anyFieldFocused |= ImGui.IsItemFocused();
                            Tools.EnumPicker($"{nameof(grid.OriginType)}", ref grid.OriginType);
                            anyFieldFocused |= ImGui.IsItemFocused();
                            ImGui.Checkbox($"{nameof(grid.DrawPlaneWireframe)}", ref grid.DrawPlaneWireframe);
                            ImGui.ColorPicker4($"{nameof(grid.PlaneWireframeColor)}", ref grid.PlaneWireframeColor);
                            anyFieldFocused |= ImGui.IsItemFocused();


                            ImGui.Separator();



                            ImGui.DragFloat($"{nameof(grid.FadeStartDistMult)}", ref grid.FadeStartDistMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();

                            ImGui.DragFloat($"{nameof(grid.FadeEndDistMult)}", ref grid.FadeEndDistMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();

                            ImGui.DragFloat($"{nameof(grid.CameraFadeStartDistMult)}", ref grid.CameraFadeStartDistMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();

                            ImGui.DragFloat($"{nameof(grid.CameraFadeEndDistMult)}", ref grid.CameraFadeEndDistMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();

                            ImGui.DragFloat($"{nameof(grid.LineThicknessMult)}", ref grid.LineThicknessMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();

                            ImGui.DragFloat($"{nameof(grid.LineThicknessFadeMult)}", ref grid.LineThicknessFadeMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();

                            ImGui.DragFloat($"{nameof(grid.LineThicknessFadePowerMult)}", ref grid.LineThicknessFadePowerMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();

                            ImGui.DragFloat($"{nameof(grid.UnitSizeMult)}", ref grid.UnitSizeMult, 0.01f, 0, 0, "%.6f");
                            anyFieldFocused |= ImGui.IsItemFocused();


                            if (Tools.SimpleClickButton("Gen Default Grid Entries"))
                            {
                                ParentDocument.Scene.AccessMainModel(m =>
                                {
                                    grid.GenerateDefaults(CurrentView, m);
                                });

                            }

                            bool addGridEntryEnabled = grid.GridEntries.Length < NewGrid3DShader.MAX_CFGS;

                            if (Tools.SimpleClickButton("Remove Grid Entry"))
                            {
                                if (grid.GridEntries.Length > 0)
                                {
                                    Array.Resize(ref grid.GridEntries, grid.GridEntries.Length - 1);
                                }
                            }
                            ImGui.SameLine();
                            if (!addGridEntryEnabled)
                                ImGui.BeginDisabled();
                            if (Tools.SimpleClickButton("Add Grid Entry") && addGridEntryEnabled)
                            {
                                ParentDocument.Scene.AccessMainModel(m =>
                                {
                                    Array.Resize(ref grid.GridEntries, grid.GridEntries.Length + 1);
                                    grid.GridEntries[grid.GridEntries.Length - 1] = new NewGrid3D.GridEntry(m.NpcParam?.HitRadius ?? 1, CurrentView);
                                });

                            }
                            if (!addGridEntryEnabled)
                                ImGui.EndDisabled();

                            for (int i = 0; i < grid.GridEntries.Length; i++)
                            {
                                ImGui.PushID($"WorldViewManager_NewGrid3D_GridEntries_{i}");

                                if (ImGui.TreeNode($"Grid Entry {(i + 1)}"))
                                {
                                    var ge = grid.GridEntries[i];

                                    ImGui.Checkbox($"{nameof(ge.Enabled)}", ref ge.Enabled);

                                    Tools.EnumPicker($"{nameof(ge.ColorType)}", ref ge.ColorType);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    if (ge.ColorType == NewGrid3D.GridColorTypes.Custom)
                                    {
                                        ImGui.ColorPicker4($"{nameof(ge.CustomColor)}", ref ge.CustomColor);
                                        anyFieldFocused |= ImGui.IsItemFocused();
                                    }
                                    else
                                    {
                                        ImGui.SameLine();
                                        ImGui.ColorButton($"{nameof(ge.ColorType)}_ColorPreview", grid.GetGridColor(ge.ColorType));
                                    }


                                    ImGui.InputFloat($"{nameof(ge.UnitSize)}", ref ge.UnitSize, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.AlphaMod)}", ref ge.AlphaMod, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.FadeStartDist_Base)}", ref ge.FadeStartDist_Base, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.FadeStartDist_Mult)}", ref ge.FadeStartDist_Mult, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.FadeEndDist_Base)}", ref ge.FadeEndDist_Base, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.FadeEndDist_Mult)}", ref ge.FadeEndDist_Mult, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();



                                    ImGui.InputFloat($"{nameof(ge.CameraFadeStartDist_Base)}", ref ge.CameraFadeStartDist_Base, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.CameraFadeStartDist_Mult)}", ref ge.CameraFadeStartDist_Mult, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.CameraFadeEndDist_Base)}", ref ge.CameraFadeEndDist_Base, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.CameraFadeEndDist_Mult)}", ref ge.CameraFadeEndDist_Mult, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.InputFloat($"{nameof(ge.CameraFadePower)}", ref ge.CameraFadePower, 0, 0);
                                    anyFieldFocused |= ImGui.IsItemFocused();



                                    ImGui.DragFloat($"{nameof(ge.LineThickness)}", ref ge.LineThickness, 0.00025f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat($"{nameof(ge.LineThicknessFade)}", ref ge.LineThicknessFade, 0.000025f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();


                                    if (ge.LineThickness < 0.0001f)
                                        ge.LineThickness = 0.0001f;

                                    if (ge.LineThicknessFade < 0)
                                        ge.LineThicknessFade = 0;

                                    ImGui.DragFloat($"{nameof(ge.LineThicknessIncreaseFromCameraDist)}", ref ge.LineThicknessIncreaseFromCameraDist, 0.01f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat($"{nameof(ge.LineThicknessIncreaseFromAnisotropic)}", ref ge.LineThicknessIncreaseFromAnisotropic, 0.01f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat($"{nameof(ge.LineThicknessFadeIncreaseFromCameraDist)}", ref ge.LineThicknessFadeIncreaseFromCameraDist, 0.01f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat($"{nameof(ge.LineThicknessFadeIncreaseFromAnisotropic)}", ref ge.LineThicknessFadeIncreaseFromAnisotropic, 0.01f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();


                                    ImGui.DragFloat($"{nameof(ge.LineThicknessFadePower)}", ref ge.LineThicknessFadePower, 0.01f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();


                                    ImGui.DragFloat($"{nameof(ge.AnisoDistFadePower_Base)}", ref ge.AnisoDistFadePower_Base, 0.1f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat($"{nameof(ge.AnisoDistFadePower_Mult)}", ref ge.AnisoDistFadePower_Mult, 0.1f, 0, 0, "%.6f");
                                    anyFieldFocused |= ImGui.IsItemFocused();




                                    ImGui.TreePop();
                                }

                                ImGui.PopID();
                            }
                        }

                        ImGui.Button("Hot Reload NewGrid3D.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                        if (ImGui.IsItemClicked())
                            GFX.ReloadNewGrid3DShader();

                        ImGui.Button("Regen Grid Primitive");
                        if (ImGui.IsItemClicked())
                            DBG.RegenNewGrid3DPrimitive();

                        //ImGui.Button("Hot Reload NewGrid3D_Blur.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                        //if (ImGui.IsItemClicked())
                        //    GFX.ReloadNewGrid3D_BlurShader();
                        ImGui.TreePop();
                    }
                }


                ImGui.Checkbox("Show Grid (Experimental Texture-Based)", ref GFX.CurrentWorldView.ShowGrid_NewSimple);

                if (GFX.CurrentWorldView.ShowGrid_NewSimple && Main.IsDebugBuild)
                {
                    if (ImGui.TreeNode("[DEBUG - NEW SIMPLE GRID]"))
                    {

                        lock (DBG._lock_NewSimpleGrid)
                        {
                            if (DBG.NewSimpleGrid == null)
                                DBG.NewSimpleGrid = new NewSimpleGrid();
                            var grid = DBG.NewSimpleGrid;

                            ImGui.Checkbox(nameof(grid.AutoGenEntries), ref grid.AutoGenEntries);

                            if (Tools.SimpleClickButton("Gen Default Grid Entries"))
                            {
                                grid.GenerateDefaults();
                            }

                            bool addGridEntryEnabled = grid.SimpleGridEntries.Count < NewGrid3DShader.MAX_CFGS;

                            if (Tools.SimpleClickButton("Remove Grid Entry"))
                            {
                                if (grid.SimpleGridEntries.Count > 0)
                                {
                                    grid.SimpleGridEntries.RemoveAt(grid.SimpleGridEntries.Count - 1);
                                }
                            }
                            ImGui.SameLine();
                            if (!addGridEntryEnabled)
                                ImGui.BeginDisabled();
                            if (Tools.SimpleClickButton("Add Grid Entry") && addGridEntryEnabled)
                            {
                                grid.SimpleGridEntries.Add(new NewSimpleGrid.SimpleGridEntry());
                            }
                            if (!addGridEntryEnabled)
                                ImGui.EndDisabled();

                            for (int i = 0; i < grid.SimpleGridEntries.Count; i++)
                            {
                                ImGui.PushID($"WorldViewManager_NewSimpleGrid_GridEntries_{i}");

                                if (ImGui.TreeNode($"Grid Entry {(i + 1)}"))
                                {
                                    var ge = grid.SimpleGridEntries[i];

                                    ImGui.Checkbox(nameof(ge.Enabled), ref ge.Enabled);

                                    Tools.EnumPicker(nameof(ge.AxisType), ref ge.AxisType);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    Tools.EnumPicker(nameof(ge.OriginType), ref ge.OriginType);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.Checkbox(nameof(ge.DrawPlaneWireframe), ref ge.DrawPlaneWireframe);
                                    ImGui.ColorPicker4(nameof(ge.PlaneWireframeColor), ref ge.PlaneWireframeColor);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat(nameof(ge.UnitSize), ref ge.UnitSize, 0.01f);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat(nameof(ge.DistFalloffStart), ref ge.DistFalloffStart, 0.01f);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat(nameof(ge.DistFalloffEnd), ref ge.DistFalloffEnd, 0.01f);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat(nameof(ge.DistFalloffPower), ref ge.DistFalloffPower, 0.01f);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    ImGui.DragFloat(nameof(ge.AlphaPower), ref ge.AlphaPower, 0.01f);
                                    anyFieldFocused |= ImGui.IsItemFocused();


                                    Tools.EnumPicker(nameof(ge.ColorType), ref ge.ColorType);
                                    anyFieldFocused |= ImGui.IsItemFocused();

                                    if (ge.ColorType == NewSimpleGrid.SimpleGridColorTypes.Custom)
                                    {
                                        ImGui.ColorPicker4(nameof(ge.CustomColor), ref ge.CustomColor);
                                        anyFieldFocused |= ImGui.IsItemFocused();
                                    }
                                    else
                                    {
                                        ImGui.SameLine();
                                        ImGui.ColorButton($"{nameof(ge.ColorType)}_ColorPreview", grid.GetGridColor(ge.ColorType));
                                    }



                                    //Tools.EnumPicker(nameof(ge.OriginColorType), ref ge.OriginColorType);

                                    //if (ge.OriginColorType == NewSimpleGrid.SimpleGridColorTypes.Custom)
                                    //{
                                    //    ImGui.ColorPicker4(nameof(ge.OriginCustomColor), ref ge.OriginCustomColor);
                                    //}
                                    //else
                                    //{
                                    //    ImGui.SameLine();
                                    //    ImGui.ColorButton($"{nameof(ge.OriginColorType)}_ColorPreview", grid.GetGridColor(ge.OriginColorType));
                                    //}


                                    ImGui.TreePop();
                                }

                                ImGui.PopID();
                            }
                        }

                        ImGui.Button("Hot Reload NewSimpleGrid.xnb\nFrom '..\\..\\..\\..\\Content\\Shaders\\' Folder");
                        if (ImGui.IsItemClicked())
                            GFX.ReloadNewSimpleGridShader();
                        ImGui.TreePop();
                    }
                }

                float gridDist = GFX.CurrentWorldView.ShowGridDistMult;
                //float gridDistBase = 1;
                //if (GFX.CurrentWorldView.ShowGrid_New3D && DBG.NewGrid3D != null)
                //{
                //    lock (DBG._lock_NewGrid3D)
                //    {
                //        gridDistBase = DBG.NewGrid3D.GridEntries[0].FadeStartDist_Base;
                //    }
                //}
                //gridDist *= gridDistBase;
                float oldGridDist = gridDist;
                ImGui.DragFloat("Grid Distance Mult", ref gridDist, 0.01f, 0, 100000, "%.2fx");
                anyFieldFocused |= ImGui.IsItemFocused();
                if (gridDist != oldGridDist)
                {
                    //gridDist /= gridDistBase;
                    GFX.CurrentWorldView.ShowGridDistMult = gridDist;
                }



                float anisoFadeDist = GFX.CurrentWorldView.GridFadePowerMult;
                //float anisoFadeDistBase = 1;
                //if (GFX.CurrentWorldView.ShowGrid_New3D && DBG.NewGrid3D != null)
                //{
                //    lock (DBG._lock_NewGrid3D)
                //    {
                //        anisoFadeDistBase = DBG.NewGrid3D.GridEntries[0].AnisoDistFadePower_Base;
                //    }
                //}
                //anisoFadeDist *= anisoFadeDistBase;
                float oldAnisoFadeDist = anisoFadeDist;
                ImGui.DragFloat("Grid Anisotropic Fade Out Power Mult", ref anisoFadeDist, 0.01f, 0, 100000, "%.2fx");
                anyFieldFocused |= ImGui.IsItemFocused();
                if (anisoFadeDist != oldAnisoFadeDist)
                {
                    //anisoFadeDist /= anisoFadeDistBase;
                    GFX.CurrentWorldView.GridFadePowerMult = anisoFadeDist;
                }



                ImGui.Checkbox("Mark Every 10 Units Of Grid", ref GFX.CurrentWorldView.ShowGrid10u);
                ImGui.Checkbox("Mark Every 100 Units Of Grid", ref GFX.CurrentWorldView.ShowGrid100u);

                ImGui.Checkbox("Show Legacy Grid (From Prior DSAS Versions)", ref GFX.CurrentWorldView.ShowGrid_Old);



                
            }

            if (isSave)
                ParentDocument.GameData.SaveProjectJson(silent: true);
        }
    }
}
