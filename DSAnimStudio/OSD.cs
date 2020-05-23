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

                //ImGui.LabelText("", "LIGHTING");

                //ImGui.Button("Manually Save Config");

                //if (ImGui.IsItemClicked())
                //{
                    
                //}

                //ImGui.Separator();

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("LIGHTING"))
                {
                    ImGui.PushItemWidth(128);
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
                else
                {
                    ImGui.PushItemWidth(128);
                }

                if (RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("SHADER"))
                {
                    ImGui.PushItemWidth(256);
                    {
                        ImGui.Checkbox("Enable Texture Alphas", ref GFX.FlverEnableTextureAlphas);
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

                if (ImGui.TreeNode("DISPLAY"))
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

                if (ImGui.TreeNode("CONTROLS"))
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
        }
    }
}
