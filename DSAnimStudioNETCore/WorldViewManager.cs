using DSAnimStudio.ImguiOSD;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class WorldViewManager
    {
        public static List<WorldView> GetWorldViewList()
        {
            List<WorldView> result = null;
            lock (_lock)
            {
                result = _worldViews.ToList();
            }
            return result;
        }

        public static void SetWorldViewList(List<WorldView> list)
        {
            lock (_lock)
            {
                _worldViews = list.ToList();
            }
        }

        private static object _lock = new object();
        public static WorldView CurrentView
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
        private static List<WorldView> _worldViews = new List<WorldView>()
        {
            new WorldView("Default"),
            //new WorldView("Test 1"),
            //new WorldView("Test 2"),
        };
        private static string[] _worldViewNamesForImgui = new string[] { "Default" };
        private static int _worldViewIndexForImgui = 0;

        public static void DrawImguiPanel()
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
                            GameData.SaveProjectJson();
                        }, canBeCancelled: true, startingText: view.Name);
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

                            GameData.SaveProjectJson(silent: true);
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

                        GameData.SaveProjectJson(silent: true);
                    }, canBeCancelled: true);
                }
                ImGui.Separator();

                ImGui.Checkbox("Show Grid", ref GFX.CurrentWorldView.ShowGrid);

                ImGui.SliderFloat("Vertical FOV", ref GFX.CurrentWorldView.ProjectionVerticalFoV, 1, 160, GFX.CurrentWorldView.ProjectionVerticalFoV <= 1.0001f ? "Orthographic" : "%.0f°");
                GFX.CurrentWorldView.ProjectionVerticalFoV = (float)Math.Round(GFX.CurrentWorldView.ProjectionVerticalFoV);

                bool lockAspect = GFX.CurrentWorldView.LockAspectRatioDuringRemo;
                bool lockAspectPrev = lockAspect;
                ImGui.Checkbox("Lock to 16:9 For Cutscenes", ref lockAspect);
                GFX.CurrentWorldView.LockAspectRatioDuringRemo = lockAspect;
                if (lockAspect != lockAspectPrev)
                {
                    Main.RequestViewportRenderTargetResolutionChange = true;
                }

                ImGui.Checkbox("Snap Cam to 45° Angles", ref GFX.CurrentWorldView.AngleSnapEnable);
                ImGui.Checkbox("Show Cam Pivot Indicator Cube", ref GFX.CurrentWorldView.PivotPrimIsEnabled);

                ImGui.SliderFloat("Near Clip Dist", ref GFX.CurrentWorldView.ProjectionNearClipDist, 0.001f, 1);
                TooltipManager.DoTooltip("Near Clipping Distance", "Distance for the near clipping plane. " +
                    "\nSetting it too high will cause geometry to disappear when near the camera. " +
                    "\nSetting it too low will cause geometry to flicker or render with " +
                    "the wrong depth when very far away from the camera.");

                ImGui.SliderFloat("Far Clip Dist", ref GFX.CurrentWorldView.ProjectionFarClipDist, 1000, 100000);
                TooltipManager.DoTooltip("Far Clipping Distance", "Distance for the far clipping plane. " +
                    "\nSetting it too low will cause geometry to disappear when far from the camera. " +
                    "\nSetting it too high will cause geometry to flicker or render with " +
                    "the wrong depth when near the camera.");
            }

            if (isSave)
                GameData.SaveProjectJson(silent: true);
        }
    }
}
