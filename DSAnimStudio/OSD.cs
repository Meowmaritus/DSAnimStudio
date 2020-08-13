using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio
{
    public static class OSD
    {
        public static bool RequestCollapse = true;

        public static bool RequestExpandAllTreeNodes = true;

        public static bool DummyPolyListOpen = false;

        public static bool RenderConfigOpen = false;
        public static bool Focused;

        public static float WindowWidth = 320;
        public static int WindowMargin = 8;

        private static WinformsTooltipHelper tooltipHelper = new WinformsTooltipHelper();

        private static float tooltipTimer = 0;
        public static float TooltipDelay = 0.25f;

        private static bool hoveringOverAnythingThisFrame = false;
        private static string currentHoverIDKey = null;
        private static string prevHoverIDKey = null;

        private static string desiredTooltipText = null;

        public static bool EnableDebugMenu = true;

        public const int DefaultItemWidth = 128;

        private static bool IsFirstFrameCaptureDefaultValue = true;
        private static Dictionary<string, Action> DefaultColorValueActions = new Dictionary<string, Action>();

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

        ////////////////////////////////////////////////////////////////////////////////
        // DEBUG STUFF
        ////////////////////////////////////////////////////////////////////////////////

        public static System.Numerics.Vector4 DEBUG_ColorA = new System.Numerics.Vector4(32 / 255f, 112 / 255f, 39 / 255f, 1);

        //////////////////////////////////////////////////////////////////////////////// 
        ////////////////////////////////////////////////////////////////////////////////

        private static string CurrentColorEditorOpen = "";
        private static void HandleColor(string name, Color color, Action<Color> setColor)
        {
            if (IsFirstFrameCaptureDefaultValue)
            {
                var copyOfColor = color;
                if (!DefaultColorValueActions.ContainsKey(name))
                    DefaultColorValueActions.Add(name, () => setColor(copyOfColor));
            }

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
            ImGui.Button(name);
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();
            if (ImGui.IsItemClicked())
            {
                if (CurrentColorEditorOpen == name)
                {
                    CurrentColorEditorOpen = "";
                }
                else
                {
                    CurrentColorEditorOpen = name;
                }
            }
            

            //ImGui.ColorButton(name, c);

            if (CurrentColorEditorOpen == name)
            {
                ImGui.ColorPicker4(name, ref c);
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

            setColor(new Color(c.X, c.Y, c.Z, c.W));
        }

        public static void Build(float elapsedTime, float offsetX, float offsetY)
        {
            hoveringOverAnythingThisFrame = false;

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0.05f, 0.05f, 0.05f, Focused ? 1 : 0f));

            ImGui.Begin("Viewport Config", ref RenderConfigOpen, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            {
                //if (!Focused)
                //    ImGui.SetWindowCollapsed(true);

                ImGui.SliderFloat("Volume", ref FmodManager.AdjustSoundVolume, 0, 1.25f);

                if (RequestCollapse)
                {
                    RequestCollapse = false;
                    ImGui.SetWindowCollapsed(true);
                }

                float x = Main.TAE_EDITOR.ModelViewerBounds.Width - WindowWidth - WindowMargin;
                float y = 8 + Main.TAE_EDITOR.ModelViewerBounds.Top;
                float w = WindowWidth;
                float h = Main.TAE_EDITOR.ModelViewerBounds.Height - (WindowMargin * 2) - 24;
                ImGui.SetWindowPos(new System.Numerics.Vector2(x, y));
                ImGui.SetWindowSize(new System.Numerics.Vector2(w, h));

                ImGui.PushItemWidth(DefaultItemWidth);

                if (EnableDebugMenu)
                {
                    

                    //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color", DBG.DbgPrim_Grid.OverrideColor.Value);

                    if (RequestExpandAllTreeNodes)
                        ImGui.SetNextItemOpen(true);

                    if (ImGui.TreeNode("[DEBUG]"))
                    {
                        //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color", DBG.DbgPrim_Grid.OverrideColor.Value);
                        //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color 2", DBG.DbgPrim_Grid.OverrideColor.Value);
                        //DBG.DbgPrim_Grid.OverrideColor = HandleColor("Grid Color 3", DBG.DbgPrim_Grid.OverrideColor.Value);

                        ImGui.TreePop();
                    }
                }

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Colors]"))
                {
                    HandleColor("Grid Color", DBG.DbgPrim_Grid.OverrideColor.Value, c => DBG.DbgPrim_Grid.OverrideColor = c);
                    HandleColor("Hitbox Color - Root", ParamData.AtkParam.Hit.ColorRoot, c => ParamData.AtkParam.Hit.ColorRoot = c);
                    HandleColor("Hitbox Color - Middle", ParamData.AtkParam.Hit.ColorMiddle, c => ParamData.AtkParam.Hit.ColorMiddle = c);
                    HandleColor("Hitbox Color - Tip", ParamData.AtkParam.Hit.ColorTip, c => ParamData.AtkParam.Hit.ColorTip = c);

                    ImGui.Button("Reset All Colors");
                    if (ImGui.IsItemClicked())
                    {
                        foreach (var kvp in DefaultColorValueActions)
                        {
                            kvp.Value.Invoke();
                        }
                    }

                    ImGui.TreePop();
                }

                if (Scene.Models.Count > 0 && Scene.Models[0].ChrAsm != null)
                {
                    if (RequestExpandAllTreeNodes)
                        ImGui.SetNextItemOpen(true);

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
                        DoWeapon(Scene.Models[0].ChrAsm?.RightWeaponModel0?.AnimContainer, "R WPN Model 0");
                        DoWeapon(Scene.Models[0].ChrAsm?.RightWeaponModel1?.AnimContainer, "R WPN Model 1");
                        DoWeapon(Scene.Models[0].ChrAsm?.RightWeaponModel2?.AnimContainer, "R WPN Model 2");
                        DoWeapon(Scene.Models[0].ChrAsm?.RightWeaponModel3?.AnimContainer, "R WPN Model 3");
                        DoWeapon(Scene.Models[0].ChrAsm?.LeftWeaponModel0?.AnimContainer, "L WPN Model 0");
                        DoWeapon(Scene.Models[0].ChrAsm?.LeftWeaponModel1?.AnimContainer, "L WPN Model 1");
                        DoWeapon(Scene.Models[0].ChrAsm?.LeftWeaponModel2?.AnimContainer, "L WPN Model 2");
                        DoWeapon(Scene.Models[0].ChrAsm?.LeftWeaponModel3?.AnimContainer, "L WPN Model 3");
                        ImGui.TreePop();
                    }
                }

                

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[DummyPoly]"))
                {

                    lock (Scene._lock_ModelLoad_Draw)
                    {
                        if (Scene.Models.Count > 0 && Scene.Models[0].DummyPolyMan != null)
                        {
                            bool wasAnyDmyForceVis = false;

                            void DoDummyPolyManager(NewDummyPolyManager dmyPolyMan, string dmyPolyGroupName)
                            {
                                if (dmyPolyMan == null)
                                    return;

                                if (RequestExpandAllTreeNodes)
                                    ImGui.SetNextItemOpen(true);

                                if (ImGui.TreeNode(dmyPolyGroupName))
                                {
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
                                            if (!wasAnyDmyForceVis && ImGui.IsItemHovered())
                                            {
                                                wasAnyDmyForceVis = true;
                                                NewDummyPolyManager.GlobalForceDummyPolyIDVisible = (dmyPolyMan.GlobalDummyPolyIDOffset + kvp.Key);
                                            }

                                            if (highlightColor)
                                                ImGui.PopStyleColor();
                                        }
                                    }

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

                                    ImGui.TreePop();
                                }

                                ImGui.Separator();
                            }

                            DoDummyPolyManager(Scene.Models[0].DummyPolyMan, "Body");

                            if (Scene.Models[0].ChrAsm != null)
                            {
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.RightWeaponModel0?.DummyPolyMan, "Right Weapon Model 0");
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.RightWeaponModel1?.DummyPolyMan, "Right Weapon Model 1");
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.RightWeaponModel2?.DummyPolyMan, "Right Weapon Model 2");
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.RightWeaponModel3?.DummyPolyMan, "Right Weapon Model 3");
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.LeftWeaponModel0?.DummyPolyMan, "Left Weapon Model 0");
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.LeftWeaponModel1?.DummyPolyMan, "Left Weapon Model 1");
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.LeftWeaponModel2?.DummyPolyMan, "Left Weapon Model 2");
                                DoDummyPolyManager(Scene.Models[0].ChrAsm?.LeftWeaponModel3?.DummyPolyMan, "Left Weapon Model 3");
                            }

                            if (!wasAnyDmyForceVis)
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
                                DummyOperationShowAll(Scene.Models[0].DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.RightWeaponModel0?.DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.RightWeaponModel1?.DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.RightWeaponModel2?.DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.RightWeaponModel3?.DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.LeftWeaponModel0?.DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.LeftWeaponModel1?.DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.LeftWeaponModel2?.DummyPolyMan);
                                DummyOperationShowAll(Scene.Models[0].ChrAsm?.LeftWeaponModel3?.DummyPolyMan);
                            }

                            ImGui.Button("Global - Hide All");
                            if (ImGui.IsItemClicked())
                            {
                                DummyOperationHideAll(Scene.Models[0].DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.RightWeaponModel0?.DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.RightWeaponModel1?.DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.RightWeaponModel2?.DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.RightWeaponModel3?.DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.LeftWeaponModel0?.DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.LeftWeaponModel1?.DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.LeftWeaponModel2?.DummyPolyMan);
                                DummyOperationHideAll(Scene.Models[0].ChrAsm?.LeftWeaponModel3?.DummyPolyMan);
                            }

                            ImGui.Button("Global - Invert All");
                            if (ImGui.IsItemClicked())
                            {
                                DummyOperationInvertAll(Scene.Models[0].DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.RightWeaponModel0?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.RightWeaponModel1?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.RightWeaponModel2?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.RightWeaponModel3?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.LeftWeaponModel0?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.LeftWeaponModel1?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.LeftWeaponModel2?.DummyPolyMan);
                                DummyOperationInvertAll(Scene.Models[0].ChrAsm?.LeftWeaponModel3?.DummyPolyMan);
                            }
                        }
                    }


                    ImGui.TreePop();
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

                        if (!GFX.FlverLightFollowsCamera)
                        {
                            ImGui.SliderFloat("Light H", ref GFX.World.LightRotationH, -MathHelper.Pi, MathHelper.Pi);

                            DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                                "\nOnly works if both Auto Light Spin and Light " +
                                "\nFollows Camera are turned off.");

                            ImGui.SliderFloat("Light V", ref GFX.World.LightRotationV, -MathHelper.PiOver2, MathHelper.PiOver2);

                            DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                                "\nOnly works if both Auto Light Spin and Light " +
                                "\nFollows Camera are turned off.");
                        }
                        else
                        {
                            ImGui.LabelText("Light H", "(Disabled)");

                            DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                                "\nOnly works if both Auto Light Spin and Light " +
                                "\nFollows Camera are turned off.");

                            ImGui.LabelText("Light V", "(Disabled)");

                            DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                               "\nOnly works if both Auto Light Spin and Light " +
                               "\nFollows Camera are turned off.");
                        }

                        
                    }
                    else
                    {
                        ImGui.LabelText("Light Follows Camera", "(Disabled)");

                        DoTooltip("Light Follows Camera", "Makes the light always point forward from the camera. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.LabelText("Light H", "(Disabled)");

                        DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                            "\nOnly works if both Auto Light Spin and Light " +
                            "\nFollows Camera are turned off.");

                        ImGui.LabelText("Light V", "(Disabled)");

                        DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                           "\nOnly works if both Auto Light Spin and Light " +
                           "\nFollows Camera are turned off.");
                    }

                    

                    ImGui.SliderFloat("Direct Light Mult", ref Environment.FlverDirectLightMult, 0, 3);

                    DoTooltip("Direct Light Multiplier", "Multiplies the brightness of light reflected directly off" +
                        "\nthe surface of the model.");

                    //ImGui.SliderFloat("Specular Power Mult", ref GFX.SpecularPowerMult, 0.05f, 2);
                    ImGui.SliderFloat("Indirect Light Mult", ref Environment.FlverIndirectLightMult, 0, 3);

                    DoTooltip("Indirect Light Multiplier", "Multiplies the brightness of environment map lighting reflected");

                    ImGui.SliderFloat("Emissive Light Mult", ref Environment.FlverEmissiveMult, 0, 3);

                    DoTooltip("Emissive Light Multiplier", "Multiplies the brightness of light emitted by the model's " +
                        "\nemissive texture map, if applicable.");

                    ImGui.SliderFloat("Brightness", ref Environment.FlverSceneBrightness, 0, 5);
                    ImGui.SliderFloat("Contrast", ref Environment.FlverSceneContrast, 0, 1);
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


                    ImGui.Checkbox("Show Cubemap As Skybox", ref Environment.DrawCubemap);

                    DoTooltip("Show Cubemap As Skybox", "Draws the environment map as the sky behind the model.");

                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.FlverAutoRotateLight = false;
                        GFX.FlverLightFollowsCamera = true;

                        GFX.World.LightRotationH = 0;
                        GFX.World.LightRotationV = 0;

                        Environment.FlverDirectLightMult = 1;
                        Environment.FlverIndirectLightMult = 1;
                        Environment.FlverEmissiveMult = 1;
                        Environment.FlverSceneBrightness = 1;
                        Environment.FlverSceneContrast = 0.5f;

                        GFX.LdotNPower = 0.1f;
                        GFX.SpecularPowerMult = 1;

                        Environment.DrawCubemap = true;

                    }
                    ImGui.Separator();

                    //ImGui.LabelText(" ", " ");
                    ImGui.ListBox("Cubemap",
                           ref Environment.CubemapNameIndex, Environment.CubemapNames,
                           Environment.CubemapNames.Length);

                    ImGui.TreePop();
                }

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Shader]"))
                {
                    ImGui.PushItemWidth(256);
                    {
                        ImGui.Checkbox("Enable Texture Alphas", ref GFX.FlverEnableTextureAlphas);
                        ImGui.Checkbox("Use Fancy Texture Alphas", ref GFX.FlverUseFancyAlpha);
                        ImGui.SliderFloat("Fancy Texture Alpha Cutoff", ref GFX.FlverFancyAlphaEdgeCutoff, 0, 1);
                        ImGui.Checkbox("Enable Texture Blending", ref GFX.FlverEnableTextureBlending);

                        ImGui.LabelText(" ", "Shading Mode:");

                        DoTooltip("Shading Mode", "The shading mode to use for the 3D rendering. " +
                            "\nSome of the modes are only here for testing purposes.");

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

                    ImGui.PushItemWidth(100);
                    {
                        if (ImGui.SliderInt("MSAA (SSAA must be off)", ref GFX.MSAA, 1, 8, GFX.MSAA > 1 ? "%dx" : "Off"))
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
                    }
                    ImGui.PopItemWidth();


                    ImGui.Checkbox("Show Grid", ref DBG.ShowGrid);
                    ImGui.SliderFloat("Vertical FOV", ref GFX.World.FieldOfView, 1, 160);

                    ImGui.SliderFloat("Near Clip Dist", ref GFX.World.NearClipDistance, 0.001f, 1);
                    DoTooltip("Near Clipping Distance", "Distance for the near clipping plane. " +
                        "\nSetting it too high will cause geometry to disappear when near the camera. " +
                        "\nSetting it too low will cause geometry to flicker or render with " +
                        "the wrong depth when very far away from the camera.");

                    ImGui.SliderFloat("Far Clip Dist", ref GFX.World.FarClipDistance, 1000, 100000);
                    DoTooltip("Far Clipping Distance", "Distance for the far clipping plane. " +
                        "\nSetting it too low will cause geometry to disappear when far from the camera. " +
                        "\nSetting it too high will cause geometry to flicker or render with " +
                        "the wrong depth when near the camera.");

                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        DBG.ShowGrid = true;
                        GFX.World.FieldOfView = 43;
                        GFX.World.NearClipDistance = 0.1f;
                        GFX.World.FarClipDistance = 10000;

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
                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.World.CameraMoveSpeed = 1;
                        GFX.World.CameraTurnSpeedMouse = 1;
                    }


                    ImGui.TreePop();
                }


                Focused = ImGui.IsWindowFocused() || ImGui.IsAnyItemFocused();

                if (ImGui.IsWindowHovered() || ImGui.IsAnyItemHovered())
                {
                    Focused = true;
                    ImGui.SetWindowFocus();
                }



                var pos = ImGui.GetWindowPos();
                var size = ImGui.GetWindowSize();

                ImGui.PopItemWidth();
                
            }
            ImGui.End();

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

            var mousePos = ImGui.GetMousePos();
            tooltipHelper.DrawPosition = new Vector2(mousePos.X + offsetX + 16, mousePos.Y + offsetY + 16);

            tooltipHelper.Update(currentHoverIDKey != null, elapsedTime);

            tooltipHelper.UpdateTooltip(Main.WinForm, currentHoverIDKey, desiredTooltipText);

            prevHoverIDKey = currentHoverIDKey;

            ImGui.PopStyleColor();

            IsFirstFrameCaptureDefaultValue = false;
        }
    }
}
