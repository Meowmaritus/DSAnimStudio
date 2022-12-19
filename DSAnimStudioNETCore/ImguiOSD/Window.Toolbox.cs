using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public abstract partial class Window
    {
        public class Toolbox : Window
        {
            public static float DefaultWindowWidth = 360;
            public static float DefaultWindowMargin = 8;

            public static float ShaderModeListWidth => 256 * Main.DPIX * OSD.RenderScale * OSD.WidthScale;
            public static float AntialiasingWidth => 100 * Main.DPIX * OSD.RenderScale * OSD.WidthScale;

            public static int DefaultItemWidth => (int)Math.Round(128 * Main.DPIX * OSD.RenderScale * OSD.WidthScale);

            public override string Title => "Toolbox";
            public override string ImguiTag => $"{nameof(Window)}.{nameof(Toolbox)}";
            protected override void BuildContents()
            {

                //if (!Focused)
                //    ImGui.SetWindowCollapsed(true);





                ImGui.PushItemWidth(DefaultItemWidth);


                ImGui.SliderFloat($"MAIN UI SIZE SCALE", ref OSD.RenderScaleTarget, 65, 300, "%.2f%%");
                ImGui.SliderFloat($"Menu Item Width", ref OSD.WidthScaleTarget, 25, 200, "%.2f%%");
                ImGui.Button("Apply New Scaling Options");
                if (ImGui.IsItemClicked())
                {
                    //var curWinSize = ImGui.GetWindowSize();
                    OSD.RenderScale = Main.DPICustomMultX = Main.DPICustomMultY = OSD.RenderScaleTarget / 100f;
                    //RenderScale = RenderScaleTarget / 100f;
                    OSD.WidthScale = OSD.WidthScaleTarget / 100f;
                }
                ImGui.Separator();

                float statusTextScale = Main.Config.ViewportStatusTextSize;
                ImGui.SliderFloat($"Viewport Status Text Size", ref statusTextScale, 0, 200, "%.2f%%");
                Main.Config.ViewportStatusTextSize = statusTextScale;

                float framerateTextScale = Main.Config.ViewportFramerateTextSize;
                ImGui.SliderFloat($"Viewport Framerate Text Size", ref framerateTextScale, 0, 200, "%.2f%%");
                Main.Config.ViewportFramerateTextSize = framerateTextScale;

                float memoryTextScale = Main.Config.ViewportMemoryTextSize;
                ImGui.SliderFloat($"Viewport Memory Text Size", ref memoryTextScale, 0, 200, "%.2f%%");
                Main.Config.ViewportMemoryTextSize = memoryTextScale;

                ImGui.Separator();
                ImGui.SliderFloat($"Volume", ref SoundManager.AdjustSoundVolume, 0, 200, "%.2f%%");
                ImGui.Button("Reset to 100%");
                if (ImGui.IsItemClicked())
                    SoundManager.AdjustSoundVolume = 100;
                ImGui.Separator();
                

                ImGui.Text("Tracking Simulation Analog Input");
                ImGui.SliderFloat("Input", ref Model.GlobalTrackingInput, -1, 1);
                ImGui.Button("Reset To 0");
                if (ImGui.IsItemClicked())
                    Model.GlobalTrackingInput = 0;
                bool trackingIsRealTime = Main.Config.CharacterTrackingTestIsIngameTime;
                ImGui.Checkbox("Tracking Simulation Uses Animation Timeline", ref trackingIsRealTime);
                Main.Config.CharacterTrackingTestIsIngameTime = trackingIsRealTime;
                ImGui.Separator();

                bool rootMotionPathEnabled = Main.Config.RootMotionPathEnabled;
                ImGui.Checkbox("Enable Root Motion Paths", ref rootMotionPathEnabled);
                Main.Config.RootMotionPathEnabled = rootMotionPathEnabled;

                float rootMotionUpdateRate = Main.Config.RootMotionPathUpdateRate;
                ImGui.SliderFloat("Root Motion Path Update Rate", ref rootMotionUpdateRate, 1, 300, "%.3f Hz");
                Main.Config.RootMotionPathUpdateRate = Math.Max(Math.Min(rootMotionUpdateRate, 300), 1);

                int rootMotionMaxSamples = Main.Config.RootMotionPathSampleMax;
                ImGui.SliderInt("Root Motion Path Sample Max", ref rootMotionMaxSamples, 1, TaeConfigFile.RootMotionPathSampleMaxInfinityValue, rootMotionMaxSamples >= TaeConfigFile.RootMotionPathSampleMaxInfinityValue ? "Unlimited" : "Up to %d");
                Main.Config.RootMotionPathSampleMax = Math.Max(Math.Min(rootMotionMaxSamples, TaeConfigFile.RootMotionPathSampleMaxInfinityValue), 1);

                ImGui.Separator();

                if (OSD.RequestCollapse)
                {
                    ImGui.SetWindowCollapsed(true);
                }

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (OSD.IsInit)
                    ImGui.SetNextItemOpen(false);

                if (ImGui.TreeNode("[MODEL VIEW MODE]"))
                {
                    lock (Scene._lock_ModelLoad_Draw)
                    {
                        foreach (var m in Scene.Models)
                        {
                            if (m.AnimContainer != null)
                            {
                                var animNames = new List<string> { "Default" };
                                animNames.AddRange(m.AnimContainer.Animations.Keys.ToList());
                                int current = m.AnimContainer.ForcePlayAnim ? animNames.IndexOf(m.AnimContainer.CurrentAnimationName) : 0;
                                int next = current;
                                ImGui.ListBox("Animation", ref next, animNames.ToArray(), animNames.Count);
                                if (current != next)
                                {
                                    if (next == 0)
                                    {
                                        m.AnimContainer.ForcePlayAnim = false;
                                        m.AnimContainer?.ClearAnimation();
                                        m.AnimContainer.ResetAll();
                                    }
                                    else
                                    {
                                        m.AnimContainer.ChangeToNewAnimation(animNames[next], animWeight: 1, startTime: 0, clearOldLayers: true);
                                        m.AnimContainer.ForcePlayAnim = true;
                                        m.AnimContainer.ResetAll();
                                    }
                                }
                            }
                        }
                    }

                    ImGui.TreePop();
                }

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (OSD.IsInit)
                    ImGui.SetNextItemOpen(false);

                if (ImGui.TreeNode("[Colors]"))
                {
                    ImGui.Text("Left click a color to expand/collapse");
                    ImGui.Text("color picker. Middle click a color");
                    ImGui.Text("to reset to default.");

                    ImGui.Separator();

                    ImGui.Text("Window");

                    Tools.HandleColor("Window Background", cc => cc.MainColorBackground, (cc, c) => cc.MainColorBackground = c);

                    Tools.HandleColor("Memory Usage Text - Low", cc => cc.GuiColorMemoryUseTextGood, (cc, c) => cc.GuiColorMemoryUseTextGood = c);
                    Tools.HandleColor("Memory Usage Text - Medium", cc => cc.GuiColorMemoryUseTextOkay, (cc, c) => cc.GuiColorMemoryUseTextOkay = c);
                    Tools.HandleColor("Memory Usage Text - High", cc => cc.GuiColorMemoryUseTextBad, (cc, c) => cc.GuiColorMemoryUseTextBad = c);

                    Tools.HandleColor("Viewport Status Text - Default", cc => cc.GuiColorViewportStatus, (cc, c) => cc.GuiColorViewportStatus = c);
                    Tools.HandleColor("Viewport Status Text - Bone Count Exceeded", cc => cc.GuiColorViewportStatusMaxBoneCountExceeded, (cc, c) => cc.GuiColorViewportStatusMaxBoneCountExceeded = c);
                    Tools.HandleColor("Viewport Status Text - Anim Doesn't Exist", cc => cc.GuiColorViewportStatusAnimDoesntExist, (cc, c) => cc.GuiColorViewportStatusAnimDoesntExist = c);
                    Tools.HandleColor("Viewport Status Text - Current Combo Chain", cc => cc.GuiColorViewportStatusCombo, (cc, c) => cc.GuiColorViewportStatusCombo = c);

                    ImGui.Text("Event Graph");

                    Tools.HandleColor("Event Graph - Background", cc => cc.GuiColorEventGraphBackground, (cc, c) => cc.GuiColorEventGraphBackground = c);
                    Tools.HandleColor("Event Graph - Ghost Graph Overlay", cc => cc.GuiColorEventGraphGhostOverlay, (cc, c) => cc.GuiColorEventGraphGhostOverlay = c);
                    Tools.HandleColor("Event Graph - Anim End Vertical Line", cc => cc.GuiColorEventGraphAnimEndVerticalLine, (cc, c) => cc.GuiColorEventGraphAnimEndVerticalLine = c);
                    Tools.HandleColor("Event Graph - Anim End Darken Rect", cc => cc.GuiColorEventGraphAnimEndDarkenRect, (cc, c) => cc.GuiColorEventGraphAnimEndDarkenRect = c);
                    Tools.HandleColor("Event Graph - Event Row Horizontal Lines", cc => cc.GuiColorEventGraphRowHorizontalLines, (cc, c) => cc.GuiColorEventGraphRowHorizontalLines = c);
                    Tools.HandleColor("Event Graph - Timeline Fill", cc => cc.GuiColorEventGraphTimelineFill, (cc, c) => cc.GuiColorEventGraphTimelineFill = c);
                    Tools.HandleColor("Event Graph - Timeline Frame Vertical Lines", cc => cc.GuiColorEventGraphTimelineFrameVerticalLines, (cc, c) => cc.GuiColorEventGraphTimelineFrameVerticalLines = c);
                    Tools.HandleColor("Event Graph - Timeline Frame Number Text", cc => cc.GuiColorEventGraphTimelineFrameNumberText, (cc, c) => cc.GuiColorEventGraphTimelineFrameNumberText = c);
                    Tools.HandleColor("Event Graph - Frame Vertical Lines", cc => cc.GuiColorEventGraphVerticalFrameLines, (cc, c) => cc.GuiColorEventGraphVerticalFrameLines = c);
                    Tools.HandleColor("Event Graph - Second Vertical Lines", cc => cc.GuiColorEventGraphVerticalSecondLines, (cc, c) => cc.GuiColorEventGraphVerticalSecondLines = c);
                    Tools.HandleColor("Event Graph - Selection Rectangle Fill", cc => cc.GuiColorEventGraphSelectionRectangleFill, (cc, c) => cc.GuiColorEventGraphSelectionRectangleFill = c);
                    Tools.HandleColor("Event Graph - Selection Rectangle Outline", cc => cc.GuiColorEventGraphSelectionRectangleOutline, (cc, c) => cc.GuiColorEventGraphSelectionRectangleOutline = c);
                    Tools.HandleColor("Event Graph - Slice Tool Line", cc => cc.GuiColorEventGraphSliceToolLine, (cc, c) => cc.GuiColorEventGraphSliceToolLine = c);
                    Tools.HandleColor("Event Graph - Playback Cursor", cc => cc.GuiColorEventGraphPlaybackCursor, (cc, c) => cc.GuiColorEventGraphPlaybackCursor = c);
                    Tools.HandleColor("Event Graph - Playback Start Time Vertical Line", cc => cc.GuiColorEventGraphPlaybackStartTime, (cc, c) => cc.GuiColorEventGraphPlaybackStartTime = c);
                    Tools.HandleColor("Event Graph - Hover Info Box Fill", cc => cc.GuiColorEventGraphHoverInfoBoxFill, (cc, c) => cc.GuiColorEventGraphHoverInfoBoxFill = c);
                    Tools.HandleColor("Event Graph - Hover Info Box Text", cc => cc.GuiColorEventGraphHoverInfoBoxText, (cc, c) => cc.GuiColorEventGraphHoverInfoBoxText = c);
                    Tools.HandleColor("Event Graph - Hover Info Box Outline", cc => cc.GuiColorEventGraphHoverInfoBoxOutline, (cc, c) => cc.GuiColorEventGraphHoverInfoBoxOutline = c);

                    Tools.HandleColor("Event Graph - Scrollbar Background", cc => cc.GuiColorEventGraphScrollbarBackground, (cc, c) => cc.GuiColorEventGraphScrollbarBackground = c);
                    Tools.HandleColor("Event Graph - Scrollbar Inactive Foreground", cc => cc.GuiColorEventGraphScrollbarForegroundInactive, (cc, c) => cc.GuiColorEventGraphScrollbarForegroundInactive = c);
                    Tools.HandleColor("Event Graph - Scrollbar Active Foreground", cc => cc.GuiColorEventGraphScrollbarForegroundActive, (cc, c) => cc.GuiColorEventGraphScrollbarForegroundActive = c);
                    Tools.HandleColor("Event Graph - Scrollbar Inactive Arrow", cc => cc.GuiColorEventGraphScrollbarArrowButtonForegroundInactive, (cc, c) => cc.GuiColorEventGraphScrollbarArrowButtonForegroundInactive = c);
                    Tools.HandleColor("Event Graph - Scrollbar Active Arrow", cc => cc.GuiColorEventGraphScrollbarArrowButtonForegroundActive, (cc, c) => cc.GuiColorEventGraphScrollbarArrowButtonForegroundActive = c);

                    Tools.HandleColor("Event Box - Normal - Fill", cc => cc.GuiColorEventBox_Normal_Fill, (cc, c) => cc.GuiColorEventBox_Normal_Fill = c);
                    Tools.HandleColor("Event Box - Normal - Outline", cc => cc.GuiColorEventBox_Normal_Outline, (cc, c) => cc.GuiColorEventBox_Normal_Outline = c);
                    Tools.HandleColor("Event Box - Normal - Text", cc => cc.GuiColorEventBox_Normal_Text, (cc, c) => cc.GuiColorEventBox_Normal_Text = c);
                    Tools.HandleColor("Event Box - Normal - Text Shadow", cc => cc.GuiColorEventBox_Normal_TextShadow, (cc, c) => cc.GuiColorEventBox_Normal_TextShadow = c);

                    Tools.HandleColor("Event Box - Highlighted - Fill", cc => cc.GuiColorEventBox_Highlighted_Fill, (cc, c) => cc.GuiColorEventBox_Highlighted_Fill = c);
                    Tools.HandleColor("Event Box - Highlighted - Outline", cc => cc.GuiColorEventBox_Highlighted_Outline, (cc, c) => cc.GuiColorEventBox_Highlighted_Outline = c);
                    Tools.HandleColor("Event Box - Highlighted - Text", cc => cc.GuiColorEventBox_Highlighted_Text, (cc, c) => cc.GuiColorEventBox_Highlighted_Text = c);
                    Tools.HandleColor("Event Box - Highlighted - Text Shadow", cc => cc.GuiColorEventBox_Highlighted_TextShadow, (cc, c) => cc.GuiColorEventBox_Highlighted_TextShadow = c);

                    Tools.HandleColor("Event Box - Selection Dimming Overlay", cc => cc.GuiColorEventBox_SelectionDimmingOverlay, (cc, c) => cc.GuiColorEventBox_SelectionDimmingOverlay = c);
                    Tools.HandleColor("Event Box - Ghost Graph Grayed Out Overlay", cc => cc.GuiColorEventBox_SelectionDimmingOverlay, (cc, c) => cc.GuiColorEventBox_SelectionDimmingOverlay = c);

                    Tools.HandleColor("Anim List - Section Collapse +/- Foreground", cc => cc.GuiColorAnimListCollapsePlusMinusForeground, (cc, c) => cc.GuiColorAnimListCollapsePlusMinusForeground = c);
                    Tools.HandleColor("Anim List - Section Collapse +/- Background", cc => cc.GuiColorAnimListCollapsePlusMinusBackground, (cc, c) => cc.GuiColorAnimListCollapsePlusMinusBackground = c);
                    Tools.HandleColor("Anim List - Section Rect Outline", cc => cc.GuiColorAnimListAnimSectionHeaderRectOutline, (cc, c) => cc.GuiColorAnimListAnimSectionHeaderRectOutline = c);
                    Tools.HandleColor("Anim List - Section Rect Fill", cc => cc.GuiColorAnimListAnimSectionHeaderRectFill, (cc, c) => cc.GuiColorAnimListAnimSectionHeaderRectFill = c);
                    Tools.HandleColor("Anim List - Section Name", cc => cc.GuiColorAnimListTextAnimSectionName, (cc, c) => cc.GuiColorAnimListTextAnimSectionName = c);
                    Tools.HandleColor("Anim List - Anim ID", cc => cc.GuiColorAnimListTextAnimName, (cc, c) => cc.GuiColorAnimListTextAnimName = c);
                    Tools.HandleColor("Anim List - Anim ID Text - Min Blend", cc => cc.GuiColorAnimListTextAnimNameMinBlend, (cc, c) => cc.GuiColorAnimListTextAnimNameMinBlend = c);
                    Tools.HandleColor("Anim List - Anim ID Text - Max Blend", cc => cc.GuiColorAnimListTextAnimNameMaxBlend, (cc, c) => cc.GuiColorAnimListTextAnimNameMaxBlend = c);
                    Tools.HandleColor("Anim List - Anim Name Text", cc => cc.GuiColorAnimListTextAnimDevName, (cc, c) => cc.GuiColorAnimListTextAnimDevName = c);
                    Tools.HandleColor("Anim List - Text Shadows", cc => cc.GuiColorAnimListTextShadow, (cc, c) => cc.GuiColorAnimListTextShadow = c);
                    Tools.HandleColor("Anim List - Anim Highlight Rect Fill", cc => cc.GuiColorAnimListHighlightRectFill, (cc, c) => cc.GuiColorAnimListHighlightRectFill = c);
                    Tools.HandleColor("Anim List - Anim Highlight Rect Outline", cc => cc.GuiColorAnimListHighlightRectOutline, (cc, c) => cc.GuiColorAnimListHighlightRectOutline = c);

                    // Deprecated afaik
                    //HandleColor("Event Box - Hovered - Text Outline", Main.Colors.GuiColorEventBox_Hover_TextOutline, c => Main.Colors.GuiColorEventBox_Hover_TextOutline = c);


                    ImGui.Text("Viewport");

                    Tools.HandleColor("Grid", cc => cc.ColorGrid, (cc, c) => cc.ColorGrid = c);
                    Tools.HandleColor("Viewport Background", cc => cc.MainColorViewportBackground, (cc, c) => cc.MainColorViewportBackground = c);


                    ImGui.Text("Helper");

                    Tools.HandleColor("Flver Bone", cc => cc.ColorHelperFlverBone, (cc, c) => cc.ColorHelperFlverBone = c);
                    Tools.HandleColor("Flver Bone Bounding Box", cc => cc.ColorHelperFlverBoneBoundingBox, (cc, c) => cc.ColorHelperFlverBoneBoundingBox = c);
                    Tools.HandleColor("Sound Event", cc => cc.ColorHelperSoundEvent, (cc, c) => cc.ColorHelperSoundEvent = c);
                    Tools.HandleColor("DummyPoly", cc => cc.ColorHelperDummyPoly, (cc, c) => cc.ColorHelperDummyPoly = c);
                    Tools.HandleColor("Camera Pivot Box", cc => cc.ColorHelperCameraPivot, (cc, c) => cc.ColorHelperCameraPivot = c);

                    Tools.HandleColor("Root Motion - Start Point", cc => cc.ColorHelperRootMotionStartLocation, (cc, c) => cc.ColorHelperRootMotionStartLocation = c);
                    Tools.HandleColor("Root Motion - Trail Line", cc => cc.ColorHelperRootMotionTrail, (cc, c) => cc.ColorHelperRootMotionTrail = c);
                    Tools.HandleColor("Root Motion - Current Point", cc => cc.ColorHelperRootMotionCurrentLocation, (cc, c) => cc.ColorHelperRootMotionCurrentLocation = c);

                    Tools.HandleColor("Root Motion - Start Point (Previous Chunk)", cc => cc.ColorHelperRootMotionStartLocation_PrevLoop, (cc, c) => cc.ColorHelperRootMotionStartLocation_PrevLoop = c);
                    Tools.HandleColor("Root Motion - Trail Line (Previous Chunk)", cc => cc.ColorHelperRootMotionTrail_PrevLoop, (cc, c) => cc.ColorHelperRootMotionTrail_PrevLoop = c);



                    ImGui.Text("Event Simulation");

                    Tools.HandleColor("Hitbox (Tip/Default)", cc => cc.ColorHelperHitboxTip, (cc, c) => cc.ColorHelperHitboxTip = c);
                    Tools.HandleColor("Hitbox (Middle)", cc => cc.ColorHelperHitboxMiddle, (cc, c) => cc.ColorHelperHitboxMiddle = c);
                    Tools.HandleColor("Hitbox (Root)", cc => cc.ColorHelperHitboxRoot, (cc, c) => cc.ColorHelperHitboxRoot = c);

                    Tools.HandleColor("DummyPoly SFX Only Spawn", cc => cc.ColorHelperDummyPolySpawnSFX, (cc, c) => cc.ColorHelperDummyPolySpawnSFX = c);
                    Tools.HandleColor("DummyPoly Bullet/Misc Only Spawn", cc => cc.ColorHelperDummyPolySpawnBulletsMisc, (cc, c) => cc.ColorHelperDummyPolySpawnBulletsMisc = c);
                    Tools.HandleColor("DummyPoly SFX + Bullet/Misc Spawn", cc => cc.ColorHelperDummyPolySpawnSFXBulletsMisc, (cc, c) => cc.ColorHelperDummyPolySpawnSFXBulletsMisc = c);



                    ImGui.Button("Reset All Colors to Default");
                    if (ImGui.IsItemClicked())
                    {
                        if (System.Windows.Forms.MessageBox.Show("Reset all to default, losing any custom colors?",
                            "Reset All?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            Main.Colors = new ColorConfig();
                        }
                    }

                    ImGui.TreePop();
                }

                if (Scene.IsModelLoaded && Scene.MainModel?.ChrAsm != null)
                {
                    if (OSD.RequestExpandAllTreeNodes)
                        ImGui.SetNextItemOpen(true);

                    if (OSD.IsInit)
                        ImGui.SetNextItemOpen(false);

                    if (ImGui.TreeNode("[c0000 Parts Animations]"))
                    {
                        void DoWeapon(NewAnimationContainer wpnAnimContainer, string nameStr)
                        {
                            if (wpnAnimContainer == null)
                                return;

                            string[] animNames = wpnAnimContainer.Animations.Keys.ToArray();
                            int curItem = wpnAnimContainer.CurrentAnimationName == null ? -1 : animNames.ToList().IndexOf(wpnAnimContainer.CurrentAnimationName);
                            int prevSelItem = curItem;
                            ImGui.ListBox(nameStr, ref curItem, animNames, animNames.Length);
                            if (curItem != prevSelItem)
                            {
                                wpnAnimContainer.ChangeToNewAnimation(curItem >= 0 ? animNames[curItem] : null, 
                                    animWeight: 1, startTime: Scene.MainModel.AnimContainer.CurrentAnimTime, clearOldLayers: true);
                            }
                        }
                        DoWeapon(Scene.MainModel.ChrAsm?.RightWeaponModel0?.AnimContainer, "R WPN Model 0");
                        DoWeapon(Scene.MainModel.ChrAsm?.RightWeaponModel1?.AnimContainer, "R WPN Model 1");
                        DoWeapon(Scene.MainModel.ChrAsm?.RightWeaponModel2?.AnimContainer, "R WPN Model 2");
                        DoWeapon(Scene.MainModel.ChrAsm?.RightWeaponModel3?.AnimContainer, "R WPN Model 3");
                        DoWeapon(Scene.MainModel.ChrAsm?.LeftWeaponModel0?.AnimContainer, "L WPN Model 0");
                        DoWeapon(Scene.MainModel.ChrAsm?.LeftWeaponModel1?.AnimContainer, "L WPN Model 1");
                        DoWeapon(Scene.MainModel.ChrAsm?.LeftWeaponModel2?.AnimContainer, "L WPN Model 2");
                        DoWeapon(Scene.MainModel.ChrAsm?.LeftWeaponModel3?.AnimContainer, "L WPN Model 3");
                        ImGui.TreePop();
                    }
                }

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (OSD.IsInit)
                    ImGui.SetNextItemOpen(false);

                if (ImGui.TreeNode("[Animation Overlays]"))
                {
                    if (Scene.IsModelLoaded)
                    {
                        if (Scene.MainModel.AnimContainer != null)
                        {
                            lock (Scene.MainModel.AnimContainer._lock_AdditiveOverlays)
                            {
                                bool requestReset = false;
                                foreach (var overlay in Scene.MainModel.AnimContainer.AdditiveBlendOverlays)
                                {
                                    bool selected = overlay.Value.Weight >= 0;
                                    bool prevSelected = selected;
                                    ImGui.Selectable(overlay.Value.Name, ref selected);

                                    if (selected)
                                    {
                                        if (overlay.Value.Weight < 0)
                                            overlay.Value.Weight = 1;

                                        float weight = overlay.Value.Weight;
                                        ImGui.SliderFloat(overlay.Value.Name + " Weight", ref weight, 0, 10);
                                        overlay.Value.Weight = weight;
                                    }
                                    else
                                    {
                                        overlay.Value.Weight = -1;
                                        overlay.Value.Reset(Scene.MainModel.AnimContainer.RootMotionTransform.GetRootMotionVector4());
                                    }

                                    if (selected != prevSelected)
                                    {
                                        requestReset = true;
                                    }
                                }

                                if (requestReset)
                                {
                                    foreach (var overlay in Scene.MainModel.AnimContainer.AdditiveBlendOverlays)
                                    {
                                        overlay.Value.Reset(Scene.MainModel.AnimContainer.RootMotionTransform.GetRootMotionVector4());
                                    }
                                }
                            }
                        }
                        else
                        {
                            ImGui.Text("No animation overlays found for this model.");
                        }
                    }
                    else
                    {
                        ImGui.Text("No model currently loaded.");
                    }
                    ImGui.TreePop();
                }


                bool thisFrameHover_Bone = false;
                bool thisFrameHover_DummyPoly = false;

                lock (Scene._lock_ModelLoad_Draw)
                {
                    if (OSD.RequestExpandAllTreeNodes)
                        ImGui.SetNextItemOpen(true);

                    if (ImGui.TreeNode("[DummyPoly]"))
                    {
                        if (Scene.IsModelLoaded && Scene.MainModel.DummyPolyMan != null)
                        {


                            void DoDummyPolyManager(NewDummyPolyManager dmyPolyMan, string dmyPolyGroupName)
                            {
                                if (dmyPolyMan == null)
                                    return;

                                if (OSD.RequestExpandAllTreeNodes)
                                    ImGui.SetNextItemOpen(true);

                                if (ImGui.TreeNode(dmyPolyGroupName))
                                {
                                    ImGui.Button($"{dmyPolyMan.GlobalDummyPolyIDPrefix}Show All");
                                    if (ImGui.IsItemClicked())
                                    {
                                        foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        {
                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, true);
                                        }
                                    }

                                    ImGui.Button($"{dmyPolyMan.GlobalDummyPolyIDPrefix}Hide All");
                                    if (ImGui.IsItemClicked())
                                    {
                                        foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        {
                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, false);
                                        }
                                    }

                                    ImGui.Button($"{dmyPolyMan.GlobalDummyPolyIDPrefix}Invert All");
                                    if (ImGui.IsItemClicked())
                                    {
                                        foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                        {
                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, !dmyPolyMan.DummyPolyVisibleByRefID[kvp.Key]);
                                        }
                                    }

                                    foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                    {
                                        if (dmyPolyMan.DummyPolyVisibleByRefID.ContainsKey(kvp.Key))
                                        {
                                            bool dmyVis = dmyPolyMan.DummyPolyVisibleByRefID[kvp.Key];

                                            bool highlightColor = NewDummyPolyManager.GlobalForceDummyPolyIDVisible == (dmyPolyMan.GlobalDummyPolyIDOffset + kvp.Key);

                                            if (highlightColor)
                                            {
                                                ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 1, 1, 1));
                                            }


                                            //ImGui.Checkbox($"{kvp.Key} ({kvp.Value.Count}x)                                       ", ref dmyVis);

                                            ImGui.Selectable($"{dmyPolyMan.GlobalDummyPolyIDPrefix}{kvp.Key} ({kvp.Value.Count}x)                                       ", ref dmyVis);

                                            dmyPolyMan.SetDummyPolyVisibility(kvp.Key, dmyVis);
                                            if (!thisFrameHover_DummyPoly && ImGui.IsItemHovered())
                                            {
                                                thisFrameHover_DummyPoly = true;
                                                NewDummyPolyManager.GlobalForceDummyPolyIDVisible = (dmyPolyMan.GlobalDummyPolyIDOffset + kvp.Key);
                                            }

                                            if (highlightColor)
                                                ImGui.PopStyleColor();
                                        }
                                    }



                                    ImGui.TreePop();
                                }

                                ImGui.Separator();
                            }



                            if (!thisFrameHover_DummyPoly)
                            {
                                NewDummyPolyManager.GlobalForceDummyPolyIDVisible = -1;
                            }

                            //ImGui.Separator();

                            void DummyOperationShowAll(NewDummyPolyManager dmyPolyMan)
                            {
                                if (dmyPolyMan == null)
                                    return;
                                foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                    dmyPolyMan.SetDummyPolyVisibility(kvp.Key, true);
                            }

                            void DummyOperationHideAll(NewDummyPolyManager dmyPolyMan)
                            {
                                if (dmyPolyMan == null)
                                    return;
                                foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                    dmyPolyMan.SetDummyPolyVisibility(kvp.Key, false);
                            }

                            void DummyOperationInvertAll(NewDummyPolyManager dmyPolyMan)
                            {
                                if (dmyPolyMan == null)
                                    return;
                                foreach (var kvp in dmyPolyMan.DummyPolyByRefID)
                                    dmyPolyMan.SetDummyPolyVisibility(kvp.Key, !dmyPolyMan.DummyPolyVisibleByRefID[kvp.Key]);
                            }

                            ImGui.Button("Global - Show All");
                            if (ImGui.IsItemClicked())
                            {
                                DummyOperationShowAll(Scene.MainModel.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.RightWeaponModel0?.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.RightWeaponModel1?.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.RightWeaponModel2?.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.RightWeaponModel3?.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.LeftWeaponModel0?.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.LeftWeaponModel1?.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.LeftWeaponModel2?.DummyPolyMan);
                                DummyOperationShowAll(Scene.MainModel.ChrAsm?.LeftWeaponModel3?.DummyPolyMan);
                            }

                            ImGui.Button("Global - Hide All");
                            if (ImGui.IsItemClicked())
                            {
                                DummyOperationHideAll(Scene.MainModel.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.RightWeaponModel0?.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.RightWeaponModel1?.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.RightWeaponModel2?.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.RightWeaponModel3?.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.LeftWeaponModel0?.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.LeftWeaponModel1?.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.LeftWeaponModel2?.DummyPolyMan);
                                DummyOperationHideAll(Scene.MainModel.ChrAsm?.LeftWeaponModel3?.DummyPolyMan);
                            }

                            ImGui.Button("Global - Invert All");
                            if (ImGui.IsItemClicked())
                            {
                                DummyOperationInvertAll(Scene.MainModel.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.RightWeaponModel0?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.RightWeaponModel1?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.RightWeaponModel2?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.RightWeaponModel3?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.LeftWeaponModel0?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.LeftWeaponModel1?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.LeftWeaponModel2?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.MainModel.ChrAsm?.LeftWeaponModel3?.DummyPolyMan);
                            }

                            DoDummyPolyManager(Scene.MainModel.DummyPolyMan, "Body");

                            if (Scene.MainModel.ChrAsm != null)
                            {
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.RightWeaponModel0?.DummyPolyMan, "Right Weapon Model 0");
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.RightWeaponModel1?.DummyPolyMan, "Right Weapon Model 1");
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.RightWeaponModel2?.DummyPolyMan, "Right Weapon Model 2");
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.RightWeaponModel3?.DummyPolyMan, "Right Weapon Model 3");
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.LeftWeaponModel0?.DummyPolyMan, "Left Weapon Model 0");
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.LeftWeaponModel1?.DummyPolyMan, "Left Weapon Model 1");
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.LeftWeaponModel2?.DummyPolyMan, "Left Weapon Model 2");
                                DoDummyPolyManager(Scene.MainModel.ChrAsm?.LeftWeaponModel3?.DummyPolyMan, "Left Weapon Model 3");
                            }
                        }



                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("[FLVER Skeleton]"))
                    {
                        if (Scene.IsModelLoaded && Scene.MainModel.SkeletonFlver != null)
                        {


                            void DoBone(NewAnimSkeleton_FLVER skeleton, NewAnimSkeleton_FLVER.FlverBoneInfo bone)
                            {
                                int boneIndex = skeleton.FlverSkeleton.IndexOf(bone);

                                bool boneDrawEnabled = bone.EnablePrimDraw;

                                if (OSD.RequestExpandAllTreeNodes)
                                    ImGui.SetNextItemOpen(true);

                                bool thisBoneHighlighted = NewAnimSkeleton_FLVER.DebugDrawTransformOfFlverBoneIndex == boneIndex;

                                if (thisBoneHighlighted)
                                    ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 1, 1, 1));



                                //bool boneNodeOpen = (ImGui.TreeNode(bone.Name));

                                ImGui.Selectable(bone.Name, ref boneDrawEnabled);

                                bone.EnablePrimDraw = boneDrawEnabled;

                                if (thisBoneHighlighted)
                                    ImGui.PopStyleColor();

                                if (ImGui.IsItemHovered())
                                {
                                    NewAnimSkeleton_FLVER.DebugDrawTransformOfFlverBoneIndex = boneIndex;
                                    thisFrameHover_Bone = true;
                                }

                                foreach (var c in bone.ChildBones)
                                {
                                    ImGui.Indent();
                                    DoBone(skeleton, c);
                                    ImGui.Unindent();
                                }

                                //if (boneNodeOpen) 
                                //{
                                //    foreach (var c in bone.ChildBones)
                                //    {
                                //        ImGui.Indent();
                                //        DoBone(skeleton, c);
                                //        ImGui.Unindent();
                                //    }


                                //    ImGui.TreePop();
                                //}






                            }

                            ImGui.Button("Show All");
                            if (ImGui.IsItemClicked())
                                foreach (var b in Scene.MainModel.SkeletonFlver.FlverSkeleton)
                                    b.EnablePrimDraw = true;

                            ImGui.Button("Hide All");
                            if (ImGui.IsItemClicked())
                                foreach (var b in Scene.MainModel.SkeletonFlver.FlverSkeleton)
                                    b.EnablePrimDraw = false;

                            ImGui.Button("Invert All");
                            if (ImGui.IsItemClicked())
                                foreach (var b in Scene.MainModel.SkeletonFlver.FlverSkeleton)
                                    b.EnablePrimDraw = !b.EnablePrimDraw;

                            foreach (var rootIndex in Scene.MainModel.SkeletonFlver.TopLevelFlverBoneIndices)
                                DoBone(Scene.MainModel.SkeletonFlver, Scene.MainModel.SkeletonFlver.FlverSkeleton[rootIndex]);


                            if (!thisFrameHover_Bone)
                                NewAnimSkeleton_FLVER.DebugDrawTransformOfFlverBoneIndex = -1;
                        }



                        ImGui.TreePop();
                    }

                    if (ImGui.TreeNode("[HKX Skeleton]"))
                    {
                        if (Scene.IsModelLoaded && Scene.MainModel.AnimContainer.Skeleton != null)
                        {


                            void DoBone(NewAnimSkeleton_HKX skeleton, NewAnimSkeleton_HKX.HkxBoneInfo bone)
                            {
                                int boneIndex = skeleton.HkxSkeleton.IndexOf(bone);

                                if (OSD.RequestExpandAllTreeNodes)
                                    ImGui.SetNextItemOpen(true);

                                bool thisBoneHighlighted = NewAnimSkeleton_HKX.DebugDrawTransformOfFlverBoneIndex == boneIndex;

                                if (thisBoneHighlighted)
                                    ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 1, 1, 1));



                                //bool boneNodeOpen = (ImGui.TreeNode(bone.Name));
                                bool boneDrawEnabled = false;
                                ImGui.Selectable(bone.Name, ref boneDrawEnabled);

                                if (thisBoneHighlighted)
                                    ImGui.PopStyleColor();

                                if (ImGui.IsItemHovered())
                                {
                                    NewAnimSkeleton_HKX.DebugDrawTransformOfFlverBoneIndex = boneIndex;
                                    thisFrameHover_Bone = true;
                                }

                                foreach (var c in bone.ChildIndices.Select(ci => skeleton.HkxSkeleton[ci]))
                                {
                                    ImGui.Indent();
                                    DoBone(skeleton, c);
                                    ImGui.Unindent();
                                }
                            }

                            foreach (var rootIndex in Scene.MainModel.AnimContainer.Skeleton.TopLevelHkxBoneIndices)
                                DoBone(Scene.MainModel.AnimContainer.Skeleton, Scene.MainModel.AnimContainer.Skeleton.HkxSkeleton[rootIndex]);


                            if (!thisFrameHover_Bone)
                                NewAnimSkeleton_HKX.DebugDrawTransformOfFlverBoneIndex = -1;
                        }

                        

                        ImGui.TreePop();
                    }

                    if (thisFrameHover_Bone)
                        NewDummyPolyManager.GlobalForceDummyPolyIDVisible = -2;

                    if (thisFrameHover_DummyPoly)
                        NewAnimSkeleton_FLVER.DebugDrawTransformOfFlverBoneIndex = -2;
                }


                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Lighting]"))
                {
                    ImGui.Checkbox("Auto Light Spin", ref GFX.FlverAutoRotateLight);

                    if (!GFX.FlverAutoRotateLight)
                    {
                        ImGui.Checkbox("Light Follows Camera", ref GFX.FlverLightFollowsCamera);

                        TooltipManager.DoTooltip("Light Follows Camera", "Makes the light always point forward from the camera. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.SliderFloat("Light H", ref Environment.LightRotationH, -MathHelper.Pi, MathHelper.Pi);

                        TooltipManager.DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                            "\nOnly works if both Auto Light Spin and Light " +
                            "\nFollows Camera are turned off.");

                        ImGui.SliderFloat("Light V", ref Environment.LightRotationV, -MathHelper.PiOver2, MathHelper.PiOver2);

                        TooltipManager.DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                            "\nOnly works if both Auto Light Spin and Light " +
                            "\nFollows Camera are turned off.");

                        //if (!GFX.FlverLightFollowsCamera)
                        //{
                        //    ImGui.SliderFloat("Light H", ref GFX.World.LightRotationH, -MathHelper.Pi, MathHelper.Pi);

                        //    DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                        //        "\nOnly works if both Auto Light Spin and Light " +
                        //        "\nFollows Camera are turned off.");

                        //    ImGui.SliderFloat("Light V", ref GFX.World.LightRotationV, -MathHelper.PiOver2, MathHelper.PiOver2);

                        //    DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                        //        "\nOnly works if both Auto Light Spin and Light " +
                        //        "\nFollows Camera are turned off.");
                        //}
                        //else
                        //{
                        //    ImGui.LabelText("Light H", "(Disabled)");

                        //    DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                        //        "\nOnly works if both Auto Light Spin and Light " +
                        //        "\nFollows Camera are turned off.");

                        //    ImGui.LabelText("Light V", "(Disabled)");

                        //    DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                        //       "\nOnly works if both Auto Light Spin and Light " +
                        //       "\nFollows Camera are turned off.");
                        //}


                    }
                    else
                    {
                        ImGui.LabelText("Light Follows Camera", "(Disabled)");

                        TooltipManager.DoTooltip("Light Follows Camera", "Makes the light always point forward from the camera. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.LabelText("Light H", "(Disabled)");

                        TooltipManager.DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.LabelText("Light V", "(Disabled)");

                        TooltipManager.DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                            "\nOnly works if Auto Light Spin is turned off.");
                    }



                    ImGui.SliderFloat("Direct Light Mult", ref Environment.FlverDirectLightMult, 0, 3);

                    TooltipManager.DoTooltip("Direct Light Multiplier", "Multiplies the brightness of light reflected directly off" +
                        "\nthe surface of the model.");


                    ImGui.SliderFloat("Indirect Light Mult", ref Environment.FlverIndirectLightMult, 0, 3);
                    TooltipManager.DoTooltip("Indirect Light Multiplier", "Multiplies the brightness of environment map lighting reflected.");

                    ImGui.SliderFloat("Ambient Light Mult", ref Environment.AmbientLightMult, 0, 3);
                    ImGui.SliderFloat("Specular Power Mult", ref GFX.SpecularPowerMult, 1, 8);
                    TooltipManager.DoTooltip("Specular Power Multiplier", "Multiplies the specular power of the lighting. " +
                        "\nHigher makes thing's very glossy. " +
                        "\nMight make some Bloodborne kin of the cosmos look more accurate.");
                    ImGui.SliderFloat("LdotN Power Mult", ref GFX.LdotNPower, 1, 8);
                    TooltipManager.DoTooltip("L dot N Vector Power Multiplier", "Advanced setting. If you know you know.");


                    ImGui.SliderFloat("Emissive Light Mult", ref Environment.FlverEmissiveMult, 0, 3);

                    TooltipManager.DoTooltip("Emissive Light Mult", "Multiplies the brightness of light emitted by the model's " +
                        "\nemissive texture map, if applicable.");


                    ImGui.SliderFloat("Direct Diffuse Mult", ref Environment.DirectDiffuseMult, 0, 3);
                    ImGui.SliderFloat("Direct Specular Mult", ref Environment.DirectSpecularMult, 0, 3);
                    ImGui.SliderFloat("Indirect Diffuse Mult", ref Environment.IndirectDiffuseMult, 0, 3);
                    ImGui.SliderFloat("Indirect Specular Mult", ref Environment.IndirectSpecularMult, 0, 3);

                    //ImGui.SliderFloat("Skybox Motion Blur Strength", ref Environment.MotionBlurStrength, 0, 2);

                    ImGui.Separator();
                    ImGui.Checkbox("Use Tonemap", ref GFX.UseTonemap);
                    ImGui.SliderFloat("Tonemap Brightness", ref Environment.FlverSceneBrightness, 0, 5);
                    ImGui.SliderFloat("Tonemap Contrast", ref Environment.FlverSceneContrast, 0, 1);





                    //ImGui.SliderFloat("Bokeh - Brightness", ref GFX.BokehBrightness, 0, 10);
                    //ImGui.SliderFloat("Bokeh - Size", ref GFX.BokehSize, 0, 50);
                    //ImGui.SliderInt("Bokeh - Downsize", ref GFX.BokehDownsize, 1, 4);
                    //ImGui.Checkbox("Boken - Dynamic Downsize", ref GFX.BokehIsDynamicDownsize);
                    //ImGui.Checkbox("Boken - Full Precision", ref GFX.BokehIsFullPrecision);


                    //ImGui.SliderFloat("LdotN Power", ref GFX.LdotNPower, 0, 1);

                    //if (ImGui.TreeNode("Cubemap Select"))
                    //{
                    //    ImGui.ListBox("Cubemap",
                    //        ref Environment.CubemapNameIndex, Environment.CubemapNames,
                    //        Environment.CubemapNames.Length, Environment.CubemapNames.Length);

                    //    ImGui.TreePop();
                    //}



                    //ImGui.BeginGroup();

                    //{
                    //    //ImGui.ListBoxHeader("TEST HEADER", Environment.CubemapNames.Length, Environment.CubemapNames.Length);


                    //    //ImGui.ListBoxFooter();
                    //}


                    //ImGui.EndGroup();

                    ImGui.Separator();

                    ImGui.Checkbox("Show Cubemap As Skybox", ref Environment.DrawCubemap);
                    ImGui.SliderFloat("Skybox Brightness", ref Environment.SkyboxBrightnessMult, 0, 0.5f);

                    TooltipManager.DoTooltip("Show Cubemap As Skybox", "Draws the environment map as the sky behind the model.");

                    ImGui.PushItemWidth(DefaultItemWidth * 1.5f);

                    //ImGui.LabelText(" ", " ");
                    ImGui.ListBox("Cubemap",
                            ref Environment.CubemapNameIndex, Environment.CubemapNames,
                            Environment.CubemapNames.Length);

                    ImGui.PopItemWidth();

                    ImGui.Button("Reset Lighting Settings to Default");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.FlverAutoRotateLight = false;
                        GFX.FlverLightFollowsCamera = true;

                        Environment.LightRotationH = -0.75f;
                        Environment.LightRotationV = -0.75f;

                        Environment.FlverDirectLightMult = 0.65f;
                        Environment.FlverIndirectLightMult = 0.65f;
                        Environment.SkyboxBrightnessMult = 0.25f;
                        Environment.FlverEmissiveMult = 1;
                        Environment.FlverSceneBrightness = 1;
                        Environment.FlverSceneContrast = 0.6f;

                        Environment.DirectDiffuseMult = 1;
                        Environment.DirectSpecularMult = 1;
                        Environment.IndirectDiffuseMult = 1;
                        Environment.IndirectSpecularMult = 1;


                        //Environment.MotionBlurStrength = 1;

                        GFX.LdotNPower = 1;
                        GFX.SpecularPowerMult = 1;

                        Environment.DrawCubemap = true;
                        Environment.CubemapNameIndex = 0;
                    }

                    ImGui.TreePop();
                }

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Shader]"))
                {


                    ImGui.Checkbox("Enable Texture Alphas", ref GFX.FlverEnableTextureAlphas);
                    ImGui.Checkbox("Use Fancy Texture Alphas", ref GFX.FlverUseFancyAlpha);
                    ImGui.SliderFloat("Fancy Texture Alpha Cutoff", ref GFX.FlverFancyAlphaEdgeCutoff, 0, 1);
                    ImGui.Checkbox("Enable Texture Blending", ref GFX.FlverEnableTextureBlending);

                    //ImGui.LabelText(" ", "Shading Mode:");

                    //TooltipManager.DoTooltip("Shading Mode", "The shading mode to use for the 3D rendering.");
                    //ImGui.PushItemWidth(ShaderModeListWidth);
                    //ImGui.ListBox(" ",
                    //        ref GFX.ForcedFlverShadingModeIndex, GFX.FlverShadingModeNamesList,
                    //        GFX.FlverShadingModeNamesList.Length);

                    GFX.FlverShadingModes_Picker.ShowPicker("Override Shader Mode", $"Toolbox_OverrideShaderMode", ref GFX.ForcedFlverShadingMode, FlverShadingModes.DEFAULT,
                        "Overrides the shading mode to use for the 3D rendering.");
                    FlverShaderEnums.NewDebugTypes_Picker.ShowPicker("Shader Debug Type", $"Toolbox_ShaderDebugType", ref GFX.GlobalNewDebugType, NewDebugTypes.None, 
                        "Sets the debug type for the shader, which allows you to show various values of the shader calculation as colors.");

                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.FlverEnableTextureAlphas = true;
                        GFX.FlverUseFancyAlpha = true;
                        GFX.FlverEnableTextureBlending = true;
                        GFX.FlverFancyAlphaEdgeCutoff = 0.25f;
                        GFX.ForcedFlverShadingMode = FlverShadingModes.DEFAULT;
                        GFX.GlobalNewDebugType = NewDebugTypes.None;
                    }

                    //ImGui.PopItemWidth();

                    ImGui.TreePop();
                }


                ImGui.Separator();


                //ImGui.LabelText("", "DISPLAY");

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Display]"))
                {
                    ImGui.Button(GFX.Display.Vsync ? "[V-SYNC: ON]" : "[V-SYNC: OFF]");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.Display.Vsync = !GFX.Display.Vsync;
                        GFX.Display.Width = GFX.Device.Viewport.Width;
                        GFX.Display.Height = GFX.Device.Viewport.Height;
                        GFX.Display.Fullscreen = false;
                        GFX.Display.Apply();
                    }

                    //ImGui.DragInt("Cam Mouse Tick Rate", ref WorldView.BGUpdateWaitMS, 0.5f, 1, 1000, "%dx ms");

                    //DoTooltip("Camera Mouse Input Tick Rate", "Milliseconds to wait between mouse input updates. " +
                    //    "\nLower this if mouse movement looks choppy or raise if it's using too much CPU.");

                    ImGui.PushItemWidth(AntialiasingWidth);
                    {
                        if (ImGui.SliderInt("MSAA (SSAA must be off)", ref GFX.MSAA, 1, 32, GFX.MSAA > 1 ? "%dx" : "Off"))
                            Main.RequestViewportRenderTargetResolutionChange = true;
                        TooltipManager.DoTooltip("MSAA", "Multi-sample antialiasing. Only works if SSAA is set to Off " +
                            "\ndue to a bug in MonoGame's RenderTarget causing a crash with both mipmaps " +
                            "\nand MSAA enabled (SSAA requires mipmaps).");

                        if (ImGui.SliderInt("SSAA (VRAM hungry)", ref GFX.SSAA, 1, 4, GFX.SSAA > 1 ? "%dx" : "Off"))
                            Main.RequestViewportRenderTargetResolutionChange = true;
                        TooltipManager.DoTooltip("SSAA", "Super-sample antialiasing. " +
                            "\nRenders at a higher resolution giving very crisp antialiasing." +
                            "\nHas very high VRAM usage. Disables MSAA due to a bug in MonoGame's " +
                            "\nRenderTarget causing a crash with both mipmaps and MSAA enabled " +
                            "\n(SSAA requires mipmaps).");

                        GFX.ClampAntialiasingOptions();
                    }
                    ImGui.PopItemWidth();


                    

                    

                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.Display.Vsync = true;
                        GFX.Display.Width = GFX.Device.Viewport.Width;
                        GFX.Display.Height = GFX.Device.Viewport.Height;
                        GFX.Display.Fullscreen = false;
                        GFX.Display.Apply();

                        GFX.MSAA = 2;
                        GFX.SSAA = 1;

                        Main.RequestViewportRenderTargetResolutionChange = true;
                    }


                    ImGui.TreePop();
                }

                ImGui.Separator();

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Controls]"))
                {
                    ImGui.SliderFloat("Camera Move Speed", ref GFX.CurrentWorldView.CameraMoveSpeed, 0.1f, 10);
                    ImGui.SliderFloat("Camera Turn Speed", ref GFX.CurrentWorldView.CameraTurnSpeedMouse, 0.001f, 2);
                    //ImGui.SliderFloat("Raw Mouse Speed", ref GFX.World.OverallMouseSpeedMult, 0, 2, "%.3fx");
                    ImGui.InputFloat("Raw Mouse Speed", ref GFX.CurrentWorldView.OverallMouseSpeedMult, 0.001f, 0.1f, "%.3fx");


                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.CurrentWorldView.CameraMoveSpeed = 1;
                        GFX.CurrentWorldView.CameraTurnSpeedMouse = 1;
                    }


                    ImGui.TreePop();
                }

                ImGui.PopItemWidth();

            }
        }
    }
}
