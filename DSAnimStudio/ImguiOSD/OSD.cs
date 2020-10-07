using DSAnimStudio.TaeEditor;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static partial class OSD
    {
        private static List<string> messageBoxTexts = new List<string>();
        private static int messageBoxIndex = -1;
        private static object _lock_messageBoxes = new object();
        


        public static bool IsWindowOpen_Toolbox = true;
        public static bool IsWindowOpen_SceneManager;
#if DEBUG
        public static bool IsWindowOpen_Debug = true;
#else
        public static bool IsWindowOpen_Debug = false;
#endif

        public static bool RequestCollapse = true;

        public static bool RequestExpandAllTreeNodes = true;

        public static bool DummyPolyListOpen = false;

        public static bool RenderConfigOpen = false;
        public static bool Focused;

        public static Vector2 QueuedWindowPosChange = Vector2.Zero;
        public static Vector2 QueuedWindowSizeChange = Vector2.Zero;

        public static float DefaultWindowWidth = 360;
        public static float DefaultWindowMargin = 8;

        private static WinformsTooltipHelper tooltipHelper = new WinformsTooltipHelper();

        private static float tooltipTimer = 0;
        public static float TooltipDelay = 0.25f;

        private static bool hoveringOverAnythingThisFrame = false;
        private static string currentHoverIDKey = null;
        private static string prevHoverIDKey = null;

        private static string desiredTooltipText = null;

        public static float RenderScale = 1;
        public static float WidthScale = 1;
        public static float RenderScaleTarget = 100;
        public static float WidthScaleTarget = 100;

#if DEBUG
        //very FromSoft style
        public static bool EnableDebugMenu = true;
#else
        public static bool EnableDebugMenu = false;
#endif

        public static int DefaultItemWidth => (int)Math.Round(128 * Main.DPIX * RenderScale * WidthScale);
        public static int ColorButtonWidth => (int)Math.Round(300 * Main.DPIX * RenderScale * WidthScale);
        public static int ColorButtonHeight => (int)Math.Round(26 * Main.DPIY * RenderScale);
        public static float ShaderModeListWidth => 256 * Main.DPIX * RenderScale * WidthScale;
        public static float AntialiasingWidth => 100 * Main.DPIX * RenderScale * WidthScale;

        private static bool IsInit = true;
        private static Dictionary<string, Action> DefaultColorValueActions = new Dictionary<string, Action>();

        private static Rectangle oldModelViewerBounds = Rectangle.Empty;

        //private static Dictionary<string, string> tooltipTexts = new Dictionary<string, string>();



        public static void CancelTooltip()
        {
            tooltipTimer = 0;
            currentHoverIDKey = null;
            desiredTooltipText = null;
        }

        private static void DoTooltip(string idKey, string text)
        {
            if (ImGui.IsItemHovered())
            {
                currentHoverIDKey = idKey;
                desiredTooltipText = text;
                //if (!tooltipTexts.ContainsKey(idKey))
                //    tooltipTexts.Add(idKey, text);
                //else
                //    tooltipTexts[idKey] = text;
                hoveringOverAnythingThisFrame = true;
            }
        }

        private static string CurrentColorEditorOpen = "";

        private static ColorConfig DefaultColorConfig = new ColorConfig();

        private static void HandleColor(string name, Func<ColorConfig, Color> getColor, Action<ColorConfig, Color> setColor)
        {
            if (IsInit)
            {
                if (!DefaultColorValueActions.ContainsKey(name))
                    DefaultColorValueActions.Add(name, () => setColor.Invoke(Main.Colors, getColor(DefaultColorConfig)));
            }

            var color = getColor.Invoke(Main.Colors);
            System.Numerics.Vector4 c = new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);

            //ImGui.ColorEdit4(name, ref c);

            float colorLightness = (0.3086f * c.X + 0.6094f * c.Y + 0.0820f * c.Z) * c.W;

            if (colorLightness > 0.5f)
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(0, 0, 0, 1));
            else
                ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1, 1, 1, 1));

            ImGui.PushStyleColor(ImGuiCol.Button, c);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, c * 1.25f);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, c * 0.75f);
            ImGui.Button(name, new System.Numerics.Vector2(ColorButtonWidth, ColorButtonHeight));
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                if (CurrentColorEditorOpen == name)
                    CurrentColorEditorOpen = "";
                else
                    CurrentColorEditorOpen = name;
            }
            else if (ImGui.IsItemClicked(ImGuiMouseButton.Middle))
            {
                color = getColor(DefaultColorConfig);
                c = new System.Numerics.Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
                setColor.Invoke(Main.Colors, color);
                //if (DefaultColorValueActions.ContainsKey(name))
                //    DefaultColorValueActions[name].Invoke();
            }
            

            //ImGui.ColorButton(name, c);

            if (CurrentColorEditorOpen == name)
            {
                ImGui.ColorPicker4(name, ref c);
                DoTooltip($"Color: {name}", "Allows you to adjust the color in detail." +
                    "\n" +
                    "\nThe number boxes are as follows:" +
                    "\n    [R] [G] [B] [A] (Red/Green/Blue/Alpha)" +
                    "\n    [H] [S] [V] [A] (Hue/Saturation/Value/Alpha)" +
                    "\n    [ Hexadecimal ] (Hexadecimal representation of the color)");
                ImGui.Separator();
            }

            //if (ImGui.IsItemClicked())
            //{
            //    if (CurrentColorEditorOpen == name)
            //    {
            //        CurrentColorEditorOpen = "";
            //    }
            //    else
            //    {
            //        CurrentColorEditorOpen = name;
            //    }

            //}

            setColor(Main.Colors, new Color(c.X, c.Y, c.Z, c.W));
        }

        private static void BuildContentsOfWindow_SceneManager()
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

                    NewDummyPolyManager.ShowGlobalIDOffset = MenuBar.Checkbox("Show c0000 Weapon Global DummyPoly ID Values (10000+)", NewDummyPolyManager.ShowGlobalIDOffset);

                    DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox] =
                        MenuBar.Checkbox("FLVER Skeleton Boxes",
                        DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.FlverBoneBoundingBox],
                        enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperFlverBoneBoundingBox);

                    DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent] =
                        MenuBar.Checkbox("FMOD Sound Events",
                        DBG.CategoryEnableDraw[DebugPrimitives.DbgPrimCategory.SoundEvent],
                        enabled: true, shortcut: "(This Color)", shortcutColor: Main.Colors.ColorHelperSoundEvent);



                    ImGui.TreePop();
                }
            }
        }

        private static void BuildContentsOfWindow_Debug()
        {
            //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color", DBG.DbgPrim_Grid.OverrideColor.Value);

            if (RequestExpandAllTreeNodes || IsInit)
                ImGui.SetNextItemOpen(true);

            _QuickDebug.BuildDebugMenu();

            ImGui.Separator();

            ImGui.Button("Hot Reload FlverShader.xnb");
            if (ImGui.IsItemClicked())
                GFX.ReloadFlverShader();

            ImGui.Button("Hot Reload FlverTonemapShader.xnb");
            if (ImGui.IsItemClicked())
                GFX.ReloadTonemapShader();

            ImGui.Button("Hot Reload CubemapSkyboxShader.xnb");
            if (ImGui.IsItemClicked())
                GFX.ReloadCubemapSkyboxShader();
        }

        private static void BuildContentsOfWindow_Toolbox()
        {
            ImGui.SetWindowFontScale(Main.DPIY * RenderScale);
            {
                //if (!Focused)
                //    ImGui.SetWindowCollapsed(true);

                



                ImGui.PushItemWidth(DefaultItemWidth);


                ImGui.SliderFloat($"Toolbox GUI Size", ref RenderScaleTarget, 50, 200, "%.2f%%");
                ImGui.SliderFloat($"Toolbox Menu Item Width", ref WidthScaleTarget, 25, 200, "%.2f%%");
                ImGui.Button("Apply New Scaling");
                if (ImGui.IsItemClicked())
                {
                    //var curWinSize = ImGui.GetWindowSize();
                    RenderScale = RenderScaleTarget / 100f;
                    WidthScale = WidthScaleTarget / 100f;
                }
                ImGui.Separator();
                ImGui.SliderFloat($"Viewport Status Text Size", ref TaeViewportInteractor.StatusTextScale, 0, 200, "%.2f%%");
                ImGui.Separator();
                ImGui.SliderFloat($"Volume", ref FmodManager.AdjustSoundVolume, 0, 200, "%.2f%%");
                ImGui.Button("Reset to 100%");
                if (ImGui.IsItemClicked())
                    FmodManager.AdjustSoundVolume = 100;
                ImGui.Separator();
                ImGui.Text("Tracking Simulation Analog Input");
                ImGui.SliderFloat("Input", ref Model.GlobalTrackingInput, -1, 1);
                ImGui.Button("Reset To 0");
                if (ImGui.IsItemClicked())
                    Model.GlobalTrackingInput = 0;
                ImGui.Separator();

                if (RequestCollapse)
                {
                    RequestCollapse = false;
                    ImGui.SetWindowCollapsed(true);
                }

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (IsInit)
                    ImGui.SetNextItemOpen(false);

                if (ImGui.TreeNode("[MODEL VIEW MODE]"))
                {
                    lock (Scene._lock_ModelLoad_Draw)
                    {
                        foreach (var m in Scene.Models)
                        {
                            if (m.AnimContainer?.ForcePlayAnim == true)
                            {
                                var animNames = m.AnimContainer.Animations.Keys.ToList();
                                int current = animNames.IndexOf(m.AnimContainer.CurrentAnimationName);
                                int next = current;
                                ImGui.ListBox("Animation", ref next, animNames.ToArray(), animNames.Count);
                                if (current != next)
                                {
                                    m.AnimContainer.CurrentAnimationName = animNames[next];
                                    m.AnimContainer.ResetAll();
                                }
                            }
                        }
                    }

                    ImGui.TreePop();
                }

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (IsInit)
                    ImGui.SetNextItemOpen(false);

                if (ImGui.TreeNode("[Colors]"))
                {
                    ImGui.Text("Left click a color to expand/collapse");
                    ImGui.Text("color picker. Middle click a color");
                    ImGui.Text("to reset to default.");

                    ImGui.Separator();

                    ImGui.Text("Window");

                    HandleColor("Window Background", cc => cc.MainColorBackground, (cc, c) => cc.MainColorBackground = c);

                    HandleColor("Memory Usage Text - Low", cc => cc.GuiColorMemoryUseTextGood, (cc, c) => cc.GuiColorMemoryUseTextGood = c);
                    HandleColor("Memory Usage Text - Medium", cc => cc.GuiColorMemoryUseTextOkay, (cc, c) => cc.GuiColorMemoryUseTextOkay = c);
                    HandleColor("Memory Usage Text - High", cc => cc.GuiColorMemoryUseTextBad, (cc, c) => cc.GuiColorMemoryUseTextBad = c);

                    HandleColor("Viewport Status Text - Default", cc => cc.GuiColorViewportStatus, (cc, c) => cc.GuiColorViewportStatus = c);
                    HandleColor("Viewport Status Text - Bone Count Exceeded", cc => cc.GuiColorViewportStatusMaxBoneCountExceeded, (cc, c) => cc.GuiColorViewportStatusMaxBoneCountExceeded = c);
                    HandleColor("Viewport Status Text - Anim Doesn't Exist", cc => cc.GuiColorViewportStatusAnimDoesntExist, (cc, c) => cc.GuiColorViewportStatusAnimDoesntExist = c);
                    HandleColor("Viewport Status Text - Current Combo Chain", cc => cc.GuiColorViewportStatusCombo, (cc, c) => cc.GuiColorViewportStatusCombo = c);

                    ImGui.Text("Event Graph");

                    HandleColor("Event Graph - Background", cc => cc.GuiColorEventGraphBackground, (cc, c) => cc.GuiColorEventGraphBackground = c);
                    HandleColor("Event Graph - Ghost Graph Overlay", cc => cc.GuiColorEventGraphGhostOverlay, (cc, c) => cc.GuiColorEventGraphGhostOverlay = c);
                    HandleColor("Event Graph - Anim End Vertical Line", cc => cc.GuiColorEventGraphAnimEndVerticalLine, (cc, c) => cc.GuiColorEventGraphAnimEndVerticalLine = c);
                    HandleColor("Event Graph - Anim End Darken Rect", cc => cc.GuiColorEventGraphAnimEndDarkenRect, (cc, c) => cc.GuiColorEventGraphAnimEndDarkenRect = c);
                    HandleColor("Event Graph - Event Row Horizontal Lines", cc => cc.GuiColorEventGraphRowHorizontalLines, (cc, c) => cc.GuiColorEventGraphRowHorizontalLines = c);
                    HandleColor("Event Graph - Timeline Fill", cc => cc.GuiColorEventGraphTimelineFill, (cc, c) => cc.GuiColorEventGraphTimelineFill = c);
                    HandleColor("Event Graph - Timeline Frame Vertical Lines", cc => cc.GuiColorEventGraphTimelineFrameVerticalLines, (cc, c) => cc.GuiColorEventGraphTimelineFrameVerticalLines = c);
                    HandleColor("Event Graph - Timeline Frame Number Text", cc => cc.GuiColorEventGraphTimelineFrameNumberText, (cc, c) => cc.GuiColorEventGraphTimelineFrameNumberText = c);
                    HandleColor("Event Graph - Frame Vertical Lines", cc => cc.GuiColorEventGraphVerticalFrameLines, (cc, c) => cc.GuiColorEventGraphVerticalFrameLines = c);
                    HandleColor("Event Graph - Second Vertical Lines", cc => cc.GuiColorEventGraphVerticalSecondLines, (cc, c) => cc.GuiColorEventGraphVerticalSecondLines = c);
                    HandleColor("Event Graph - Selection Rectangle Fill", cc => cc.GuiColorEventGraphSelectionRectangleFill, (cc, c) => cc.GuiColorEventGraphSelectionRectangleFill = c);
                    HandleColor("Event Graph - Selection Rectangle Outline", cc => cc.GuiColorEventGraphSelectionRectangleOutline, (cc, c) => cc.GuiColorEventGraphSelectionRectangleOutline = c);
                    HandleColor("Event Graph - Playback Cursor", cc => cc.GuiColorEventGraphPlaybackCursor, (cc, c) => cc.GuiColorEventGraphPlaybackCursor = c);
                    HandleColor("Event Graph - Playback Start Time Vertical Line", cc => cc.GuiColorEventGraphPlaybackStartTime, (cc, c) => cc.GuiColorEventGraphPlaybackStartTime = c);
                    HandleColor("Event Graph - Hover Info Box Fill", cc => cc.GuiColorEventGraphHoverInfoBoxFill, (cc, c) => cc.GuiColorEventGraphHoverInfoBoxFill = c);
                    HandleColor("Event Graph - Hover Info Box Text", cc => cc.GuiColorEventGraphHoverInfoBoxText, (cc, c) => cc.GuiColorEventGraphHoverInfoBoxText = c);
                    HandleColor("Event Graph - Hover Info Box Outline", cc => cc.GuiColorEventGraphHoverInfoBoxOutline, (cc, c) => cc.GuiColorEventGraphHoverInfoBoxOutline = c);

                    HandleColor("Event Graph - Scrollbar Background", cc => cc.GuiColorEventGraphScrollbarBackground, (cc, c) => cc.GuiColorEventGraphScrollbarBackground = c);
                    HandleColor("Event Graph - Scrollbar Inactive Foreground", cc => cc.GuiColorEventGraphScrollbarForegroundInactive, (cc, c) => cc.GuiColorEventGraphScrollbarForegroundInactive = c);
                    HandleColor("Event Graph - Scrollbar Active Foreground", cc => cc.GuiColorEventGraphScrollbarForegroundActive, (cc, c) => cc.GuiColorEventGraphScrollbarForegroundActive = c);
                    HandleColor("Event Graph - Scrollbar Inactive Arrow", cc => cc.GuiColorEventGraphScrollbarArrowButtonForegroundInactive, (cc, c) => cc.GuiColorEventGraphScrollbarArrowButtonForegroundInactive = c);
                    HandleColor("Event Graph - Scrollbar Active Arrow", cc => cc.GuiColorEventGraphScrollbarArrowButtonForegroundActive, (cc, c) => cc.GuiColorEventGraphScrollbarArrowButtonForegroundActive = c);

                    HandleColor("Event Box - Normal - Fill", cc => cc.GuiColorEventBox_Normal_Fill, (cc, c) => cc.GuiColorEventBox_Normal_Fill = c);
                    HandleColor("Event Box - Normal - Outline", cc => cc.GuiColorEventBox_Normal_Outline, (cc, c) => cc.GuiColorEventBox_Normal_Outline = c);
                    HandleColor("Event Box - Normal - Text", cc => cc.GuiColorEventBox_Normal_Text, (cc, c) => cc.GuiColorEventBox_Normal_Text = c);
                    HandleColor("Event Box - Normal - Text Shadow", cc => cc.GuiColorEventBox_Normal_TextShadow, (cc, c) => cc.GuiColorEventBox_Normal_TextShadow = c);

                    HandleColor("Event Box - Highlighted - Fill", cc => cc.GuiColorEventBox_Highlighted_Fill, (cc, c) => cc.GuiColorEventBox_Highlighted_Fill = c);
                    HandleColor("Event Box - Highlighted - Outline", cc => cc.GuiColorEventBox_Highlighted_Outline, (cc, c) => cc.GuiColorEventBox_Highlighted_Outline = c);
                    HandleColor("Event Box - Highlighted - Text", cc => cc.GuiColorEventBox_Highlighted_Text, (cc, c) => cc.GuiColorEventBox_Highlighted_Text = c);
                    HandleColor("Event Box - Highlighted - Text Shadow", cc => cc.GuiColorEventBox_Highlighted_TextShadow, (cc, c) => cc.GuiColorEventBox_Highlighted_TextShadow = c);

                    HandleColor("Event Box - Selection Dimming Overlay", cc => cc.GuiColorEventBox_SelectionDimmingOverlay, (cc, c) => cc.GuiColorEventBox_SelectionDimmingOverlay = c);
                    HandleColor("Event Box - Ghost Graph Grayed Out Overlay", cc => cc.GuiColorEventBox_SelectionDimmingOverlay, (cc, c) => cc.GuiColorEventBox_SelectionDimmingOverlay = c);

                    HandleColor("Anim List - Section Collapse +/- Foreground", cc => cc.GuiColorAnimListCollapsePlusMinusForeground, (cc, c) => cc.GuiColorAnimListCollapsePlusMinusForeground = c);
                    HandleColor("Anim List - Section Collapse +/- Background", cc => cc.GuiColorAnimListCollapsePlusMinusBackground, (cc, c) => cc.GuiColorAnimListCollapsePlusMinusBackground = c);
                    HandleColor("Anim List - Section Rect Outline", cc => cc.GuiColorAnimListAnimSectionHeaderRectOutline, (cc, c) => cc.GuiColorAnimListAnimSectionHeaderRectOutline = c);
                    HandleColor("Anim List - Section Rect Fill", cc => cc.GuiColorAnimListAnimSectionHeaderRectFill, (cc, c) => cc.GuiColorAnimListAnimSectionHeaderRectFill = c);
                    HandleColor("Anim List - Section Name", cc => cc.GuiColorAnimListTextAnimSectionName, (cc, c) => cc.GuiColorAnimListTextAnimSectionName = c);
                    HandleColor("Anim List - Anim ID", cc => cc.GuiColorAnimListTextAnimName, (cc, c) => cc.GuiColorAnimListTextAnimName = c);
                    HandleColor("Anim List - Anim ID Text - Min Blend", cc => cc.GuiColorAnimListTextAnimNameMinBlend, (cc, c) => cc.GuiColorAnimListTextAnimNameMinBlend = c);
                    HandleColor("Anim List - Anim ID Text - Max Blend", cc => cc.GuiColorAnimListTextAnimNameMaxBlend, (cc, c) => cc.GuiColorAnimListTextAnimNameMaxBlend = c);
                    HandleColor("Anim List - Anim Name Text", cc => cc.GuiColorAnimListTextAnimDevName, (cc, c) => cc.GuiColorAnimListTextAnimDevName = c);
                    HandleColor("Anim List - Text Shadows", cc => cc.GuiColorAnimListTextShadow, (cc, c) => cc.GuiColorAnimListTextShadow = c);
                    HandleColor("Anim List - Anim Highlight Rect Fill", cc => cc.GuiColorAnimListHighlightRectFill, (cc, c) => cc.GuiColorAnimListHighlightRectFill = c);
                    HandleColor("Anim List - Anim Highlight Rect Outline", cc => cc.GuiColorAnimListHighlightRectOutline, (cc, c) => cc.GuiColorAnimListHighlightRectOutline = c);

                    // Deprecated afaik
                    //HandleColor("Event Box - Hovered - Text Outline", Main.Colors.GuiColorEventBox_Hover_TextOutline, c => Main.Colors.GuiColorEventBox_Hover_TextOutline = c);


                    ImGui.Text("Viewport");

                    HandleColor("Grid", cc => cc.ColorGrid, (cc, c) => cc.ColorGrid = c);
                    HandleColor("Viewport Background", cc => cc.MainColorViewportBackground, (cc, c) => cc.MainColorViewportBackground = c);


                    ImGui.Text("Helper");

                    HandleColor("Flver Bone", cc => cc.ColorHelperFlverBone, (cc, c) => cc.ColorHelperFlverBone = c);
                    HandleColor("Flver Bone Bounding Box", cc => cc.ColorHelperFlverBoneBoundingBox, (cc, c) => cc.ColorHelperFlverBoneBoundingBox = c);
                    HandleColor("Sound Event", cc => cc.ColorHelperSoundEvent, (cc, c) => cc.ColorHelperSoundEvent = c);
                    HandleColor("DummyPoly", cc => cc.ColorHelperDummyPoly, (cc, c) => cc.ColorHelperDummyPoly = c);
                    HandleColor("Camera Pivot Box", cc => cc.ColorHelperCameraPivot, (cc, c) => cc.ColorHelperCameraPivot = c);



                    ImGui.Text("Event Simulation");

                    HandleColor("Hitbox (Tip/Default)", cc => cc.ColorHelperHitboxTip, (cc, c) => cc.ColorHelperHitboxTip = c);
                    HandleColor("Hitbox (Middle)", cc => cc.ColorHelperHitboxMiddle, (cc, c) => cc.ColorHelperHitboxMiddle = c);
                    HandleColor("Hitbox (Root)", cc => cc.ColorHelperHitboxRoot, (cc, c) => cc.ColorHelperHitboxRoot = c);

                    HandleColor("DummyPoly SFX Only Spawn", cc => cc.ColorHelperDummyPolySpawnSFX, (cc, c) => cc.ColorHelperDummyPolySpawnSFX = c);
                    HandleColor("DummyPoly Bullet/Misc Only Spawn", cc => cc.ColorHelperDummyPolySpawnBulletsMisc, (cc, c) => cc.ColorHelperDummyPolySpawnBulletsMisc = c);
                    HandleColor("DummyPoly SFX + Bullet/Misc Spawn", cc => cc.ColorHelperDummyPolySpawnSFXBulletsMisc, (cc, c) => cc.ColorHelperDummyPolySpawnSFXBulletsMisc = c);



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
                    if (RequestExpandAllTreeNodes)
                        ImGui.SetNextItemOpen(true);

                    if (IsInit)
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
                                wpnAnimContainer.CurrentAnimationName = curItem >= 0 ? animNames[curItem] : null;
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

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (IsInit)
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
                                    bool selected = overlay.Weight >= 0;
                                    bool prevSelected = selected;
                                    ImGui.Selectable(overlay.Name, ref selected);

                                    if (selected)
                                    {
                                        if (overlay.Weight < 0)
                                            overlay.Weight = 1;

                                        float weight = overlay.Weight;
                                        ImGui.SliderFloat(overlay.Name + " Weight", ref weight, 0, 10);
                                        overlay.Weight = weight;
                                    }
                                    else
                                    {
                                        overlay.Weight = -1;
                                        overlay.Reset();
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
                                        overlay.Reset();
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
                    if (RequestExpandAllTreeNodes)
                        ImGui.SetNextItemOpen(true);

                    if (ImGui.TreeNode("[DummyPoly]"))
                    {
                        if (Scene.IsModelLoaded && Scene.MainModel.DummyPolyMan != null)
                        {


                            void DoDummyPolyManager(NewDummyPolyManager dmyPolyMan, string dmyPolyGroupName)
                            {
                                if (dmyPolyMan == null)
                                    return;

                                if (RequestExpandAllTreeNodes)
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

                    if (ImGui.TreeNode("[Skeleton]"))
                    {
                        if (Scene.IsModelLoaded && Scene.MainModel.Skeleton != null)
                        {


                            void DoBone(NewAnimSkeleton skeleton, NewAnimSkeleton.FlverBoneInfo bone)
                            {
                                int boneIndex = skeleton.FlverSkeleton.IndexOf(bone);

                                bool boneDrawEnabled = bone.EnablePrimDraw;

                                if (RequestExpandAllTreeNodes)
                                    ImGui.SetNextItemOpen(true);

                                bool thisBoneHighlighted = NewAnimSkeleton.DebugDrawTransformOfFlverBoneIndex == boneIndex;

                                if (thisBoneHighlighted)
                                    ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0, 1, 1, 1));



                                //bool boneNodeOpen = (ImGui.TreeNode(bone.Name));

                                ImGui.Selectable(bone.Name, ref boneDrawEnabled);

                                bone.EnablePrimDraw = boneDrawEnabled;

                                if (thisBoneHighlighted)
                                    ImGui.PopStyleColor();

                                if (ImGui.IsItemHovered())
                                {
                                    NewAnimSkeleton.DebugDrawTransformOfFlverBoneIndex = boneIndex;
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
                                foreach (var b in Scene.MainModel.Skeleton.FlverSkeleton)
                                    b.EnablePrimDraw = true;

                            ImGui.Button("Hide All");
                            if (ImGui.IsItemClicked())
                                foreach (var b in Scene.MainModel.Skeleton.FlverSkeleton)
                                    b.EnablePrimDraw = false;

                            ImGui.Button("Invert All");
                            if (ImGui.IsItemClicked())
                                foreach (var b in Scene.MainModel.Skeleton.FlverSkeleton)
                                    b.EnablePrimDraw = !b.EnablePrimDraw;

                            foreach (var rootIndex in Scene.MainModel.Skeleton.TopLevelFlverBoneIndices)
                                DoBone(Scene.MainModel.Skeleton, Scene.MainModel.Skeleton.FlverSkeleton[rootIndex]);


                            if (!thisFrameHover_Bone)
                                NewAnimSkeleton.DebugDrawTransformOfFlverBoneIndex = -1;
                        }



                        ImGui.TreePop();
                    }

                    if (thisFrameHover_Bone)
                        NewDummyPolyManager.GlobalForceDummyPolyIDVisible = -2;

                    if (thisFrameHover_DummyPoly)
                        NewAnimSkeleton.DebugDrawTransformOfFlverBoneIndex = -2;
                }


                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Lighting]"))
                {
                    ImGui.Checkbox("Auto Light Spin", ref GFX.FlverAutoRotateLight);

                    if (!GFX.FlverAutoRotateLight)
                    {
                        ImGui.Checkbox("Light Follows Camera", ref GFX.FlverLightFollowsCamera);

                        DoTooltip("Light Follows Camera", "Makes the light always point forward from the camera. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.SliderFloat("Light H", ref Environment.LightRotationH, -MathHelper.Pi, MathHelper.Pi);

                        DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                            "\nOnly works if both Auto Light Spin and Light " +
                            "\nFollows Camera are turned off.");

                        ImGui.SliderFloat("Light V", ref Environment.LightRotationV, -MathHelper.PiOver2, MathHelper.PiOver2);

                        DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
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

                        DoTooltip("Light Follows Camera", "Makes the light always point forward from the camera. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.LabelText("Light H", "(Disabled)");

                        DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.LabelText("Light V", "(Disabled)");

                        DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                           "\nOnly works if Auto Light Spin is turned off.");
                    }



                    ImGui.SliderFloat("Direct Light Mult", ref Environment.FlverDirectLightMult, 0, 3);

                    DoTooltip("Direct Light Multiplier", "Multiplies the brightness of light reflected directly off" +
                        "\nthe surface of the model.");


                    ImGui.SliderFloat("Indirect Light Mult", ref Environment.FlverIndirectLightMult, 0, 3);
                    DoTooltip("Indirect Light Multiplier", "Multiplies the brightness of environment map lighting reflected.");

                    //ImGui.SliderFloat("Ambient Light Mult", ref Environment.AmbientLightMult, 0, 3);
                    ImGui.SliderFloat("Specular Power Mult", ref GFX.SpecularPowerMult, 1, 8);
                    DoTooltip("Specular Power Multiplier", "Multiplies the specular power of the lighting. " +
                        "\nHigher makes thing's very glossy. " +
                        "\nMight make some Bloodborne kin of the cosmos look more accurate.");


                    ImGui.SliderFloat("Emissive Light Mult", ref Environment.FlverEmissiveMult, 0, 3);

                    DoTooltip("Emissive Light Mult", "Multiplies the brightness of light emitted by the model's " +
                        "\nemissive texture map, if applicable.");


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

                    DoTooltip("Show Cubemap As Skybox", "Draws the environment map as the sky behind the model.");

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

                        //Environment.MotionBlurStrength = 1;

                        GFX.LdotNPower = 0.1f;
                        GFX.SpecularPowerMult = 1;

                        Environment.DrawCubemap = true;
                        Environment.CubemapNameIndex = 0;
                    }

                    ImGui.TreePop();
                }

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Shader]"))
                {


                    ImGui.Checkbox("Enable Texture Alphas", ref GFX.FlverEnableTextureAlphas);
                    ImGui.Checkbox("Use Fancy Texture Alphas", ref GFX.FlverUseFancyAlpha);
                    ImGui.SliderFloat("Fancy Texture Alpha Cutoff", ref GFX.FlverFancyAlphaEdgeCutoff, 0, 1);
                    ImGui.Checkbox("Enable Texture Blending", ref GFX.FlverEnableTextureBlending);

                    ImGui.LabelText(" ", "Shading Mode:");

                    DoTooltip("Shading Mode", "The shading mode to use for the 3D rendering. " +
                        "\nSome of the modes are only here for testing purposes.");
                    ImGui.PushItemWidth(ShaderModeListWidth);
                    ImGui.ListBox(" ",
                            ref GFX.ForcedFlverShadingModeIndex, GFX.FlverShadingModeNamesList,
                            GFX.FlverShadingModeNamesList.Length);

                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.FlverEnableTextureAlphas = true;
                        GFX.FlverEnableTextureBlending = true;
                        GFX.ForcedFlverShadingModeIndex = 0;
                    }

                    ImGui.PopItemWidth();

                    ImGui.TreePop();
                }


                ImGui.Separator();


                //ImGui.LabelText("", "DISPLAY");

                if (RequestExpandAllTreeNodes)
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
                        DoTooltip("MSAA", "Multi-sample antialiasing. Only works if SSAA is set to Off " +
                            "\ndue to a bug in MonoGame's RenderTarget causing a crash with both mipmaps " +
                            "\nand MSAA enabled (SSAA requires mipmaps).");

                        if (ImGui.SliderInt("SSAA (VRAM hungry)", ref GFX.SSAA, 1, 4, GFX.SSAA > 1 ? "%dx" : "Off"))
                            Main.RequestViewportRenderTargetResolutionChange = true;
                        DoTooltip("SSAA", "Super-sample antialiasing. " +
                            "\nRenders at a higher resolution giving very crisp antialiasing." +
                            "\nHas very high VRAM usage. Disables MSAA due to a bug in MonoGame's " +
                            "\nRenderTarget causing a crash with both mipmaps and MSAA enabled " +
                            "\n(SSAA requires mipmaps).");

                        GFX.ClampAntialiasingOptions();
                    }
                    ImGui.PopItemWidth();


                    ImGui.Checkbox("Show Grid", ref DBG.ShowGrid);

                    ImGui.SliderFloat("Vertical FOV", ref GFX.World.ProjectionVerticalFoV, 1, 160, GFX.World.ProjectionVerticalFoV <= 1.0001f ? "Orthographic" : "%.0f°");
                    GFX.World.ProjectionVerticalFoV = (float)Math.Round(GFX.World.ProjectionVerticalFoV);

                    ImGui.Checkbox("Snap Cam to 45° Angles", ref GFX.World.AngleSnapEnable);
                    ImGui.Checkbox("Show Cam Pivot Indicator Cube", ref GFX.World.PivotPrimIsEnabled);

                    ImGui.SliderFloat("Near Clip Dist", ref GFX.World.ProjectionNearClipDist, 0.001f, 1);
                    DoTooltip("Near Clipping Distance", "Distance for the near clipping plane. " +
                        "\nSetting it too high will cause geometry to disappear when near the camera. " +
                        "\nSetting it too low will cause geometry to flicker or render with " +
                        "the wrong depth when very far away from the camera.");

                    ImGui.SliderFloat("Far Clip Dist", ref GFX.World.ProjectionFarClipDist, 1000, 100000);
                    DoTooltip("Far Clipping Distance", "Distance for the far clipping plane. " +
                        "\nSetting it too low will cause geometry to disappear when far from the camera. " +
                        "\nSetting it too high will cause geometry to flicker or render with " +
                        "the wrong depth when near the camera.");

                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        DBG.ShowGrid = true;
                        GFX.World.ProjectionVerticalFoV = 43;
                        GFX.World.ProjectionNearClipDist = 0.1f;
                        GFX.World.ProjectionFarClipDist = 10000;
                        GFX.World.AngleSnapEnable = false;
                        GFX.World.PivotPrimIsEnabled = true;

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

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Controls]"))
                {
                    ImGui.SliderFloat("Camera Move Speed", ref GFX.World.CameraMoveSpeed, 0.1f, 10);
                    ImGui.SliderFloat("Camera Turn Speed", ref GFX.World.CameraTurnSpeedMouse, 0.001f, 2);
                    //ImGui.SliderFloat("Raw Mouse Speed", ref GFX.World.OverallMouseSpeedMult, 0, 2, "%.3fx");
                    ImGui.InputFloat("Raw Mouse Speed", ref GFX.World.OverallMouseSpeedMult, 0.001f, 0.1f, "%.3fx");


                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.World.CameraMoveSpeed = 1;
                        GFX.World.CameraTurnSpeedMouse = 1;
                    }


                    ImGui.TreePop();
                }

                ImGui.PopItemWidth();
            }
        }

        

        public static void Build(float elapsedTime, float offsetX, float offsetY)
        {
            bool abortBuild = false;

            lock (_lock_messageBoxes)
            {
                if (messageBoxTexts.Count > 0)
                {
                    string popupName = $"NOTICE {(messageBoxIndex + 1)} OF {messageBoxTexts.Count}";
                    ImGui.OpenPopup(popupName);
                    bool thisopen = true;
                    if (ImGui.BeginPopupModal(popupName, ref thisopen, ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 500);
                        ImGui.TextWrapped(messageBoxTexts[messageBoxIndex]);

                        bool isLast = messageBoxIndex >= messageBoxTexts.Count - 1;

                        ImGui.Button("Dismiss", new System.Numerics.Vector2(64, 24));
                        if (ImGui.IsItemClicked())
                        {
                            if (isLast)
                            {
                                messageBoxIndex = -1;
                                messageBoxTexts.Clear();
                            }
                            else
                            {
                                messageBoxIndex++;
                            }
                        }

                        ImGui.SameLine();

                        ImGui.Button("Dismiss All");
                        if (ImGui.IsItemClicked())
                        {
                            messageBoxIndex = -1;
                            messageBoxTexts.Clear();
                        }


                        ImGui.PopTextWrapPos();

                        ImGui.EndPopup();
                    }



                    abortBuild = true;
                }
            }

            

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 7.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(14.0f, 8.0f));

            if (!abortBuild)
            {
                Tools.ToolsUpdateLoop();

                if (ImGui.BeginMainMenuBar())
                {
                    MenuBar.BuildMenuBar();
                    ImGui.EndMainMenuBar();
                }

                var mousePos = ImGui.GetMousePos();
                tooltipHelper.DrawPosition = new Vector2(mousePos.X + offsetX + 16, mousePos.Y + offsetY + 16);

                tooltipHelper.Update(currentHoverIDKey != null, elapsedTime);

                tooltipHelper.UpdateTooltip(Main.WinForm, currentHoverIDKey, desiredTooltipText);

                var curModelViewerBounds = Main.TAE_EDITOR.ModelViewerBounds;

                //if (!IsInit && oldModelViewerBounds != curModelViewerBounds)
                //{
                //    QueuedWindowPosChange += new Vector2(curModelViewerBounds.Width - oldModelViewerBounds.Width, 0);
                //}


                hoveringOverAnythingThisFrame = false;

                ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0.05f, 0.05f, 0.05f, Focused ? 1 : 0.4f));

                bool firstTimeWindowCreate = IsInit && !File.Exists("imgui.ini");

                if (IsWindowOpen_Toolbox)
                {

                    ImGui.Begin("Toolbox", ref IsWindowOpen_Toolbox);
                    {

                        if (firstTimeWindowCreate)
                        {
                            float x = Main.TAE_EDITOR.ModelViewerBounds.Width - DefaultWindowWidth - DefaultWindowMargin;
                            float y = 8;
                            float w = DefaultWindowWidth;
                            float h = Main.TAE_EDITOR.ModelViewerBounds.Height - (DefaultWindowMargin * 2) - 24;
                            ImGui.SetWindowPos(new System.Numerics.Vector2(x, y) * Main.DPIVectorN);
                            ImGui.SetWindowSize(new System.Numerics.Vector2(w, h) * Main.DPIVectorN);
                        }
                        else
                        {
                            if (QueuedWindowPosChange != Vector2.Zero)
                            {
                                ImGui.SetWindowPos(ImGui.GetWindowPos() + QueuedWindowPosChange.ToCS());
                                QueuedWindowPosChange = Vector2.Zero;
                            }

                            if (QueuedWindowSizeChange != Vector2.Zero)
                            {
                                ImGui.SetWindowSize(ImGui.GetWindowSize() + QueuedWindowSizeChange.ToCS());
                                QueuedWindowSizeChange = Vector2.Zero;
                            }
                        }


                        BuildContentsOfWindow_Toolbox();

                    }
                    ImGui.End();
                }

                if (EnableDebugMenu && IsWindowOpen_Debug)
                {
                    ImGui.Begin("Debug", ref IsWindowOpen_Debug);
                    {
                        BuildContentsOfWindow_Debug();
                    }
                    ImGui.End();
                }

                if (IsWindowOpen_SceneManager)
                {
                    ImGui.Begin("Scene Manager", ref IsWindowOpen_SceneManager);
                    {
                        BuildContentsOfWindow_SceneManager();
                    }
                    ImGui.End();
                }

                RequestExpandAllTreeNodes = false;

                if (!hoveringOverAnythingThisFrame)
                {
                    tooltipTimer = 0;
                    desiredTooltipText = null;
                    currentHoverIDKey = null;
                }
                else
                {
                    if (currentHoverIDKey != prevHoverIDKey)
                    {
                        tooltipTimer = 0;

                        tooltipHelper.Update(false, 0);
                    }

                    //if (currentHoverIDKey != null)
                    //{
                    //    if (tooltipTimer < TooltipDelay)
                    //    {
                    //        tooltipTimer += elapsedTime;
                    //    }
                    //    else
                    //    {
                    //        ImGui.SetTooltip(desiredTooltipText);
                    //    }
                    //}


                }



                prevHoverIDKey = currentHoverIDKey;
                oldModelViewerBounds = curModelViewerBounds;
                ImGui.PopStyleColor();

            }
            
            ImGui.PopStyleVar(3);
            

            

            IsInit = false;

            Focused = ImGui.IsAnyItemFocused() || ImGui.IsWindowFocused(ImGuiFocusedFlags.AnyWindow) || ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) || ImGui.IsAnyItemHovered();
        }
    }
}
