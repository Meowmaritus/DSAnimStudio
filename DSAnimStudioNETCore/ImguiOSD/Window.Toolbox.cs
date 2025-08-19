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
            public override SaveOpenStateTypes GetSaveOpenStateType() => SaveOpenStateTypes.SaveAlways;

            public override string NewImguiWindowTitle => "Toolbox";
            
            protected override void Init()
            {
                
            }

            public static float DefaultWindowWidth = 360;
            public static float DefaultWindowMargin = 8;

            public static float ShaderModeListWidth => 256 * Main.DPI * OSD.RenderScale * OSD.WidthScale;
            public static float AntialiasingWidth => 100 * Main.DPI * OSD.RenderScale * OSD.WidthScale;
            
            protected override void BuildContents(ref bool anyFieldFocused)
            {

                //if (!Focused)
                //    ImGui.SetWindowCollapsed(true);

                ImGui.Button("Load UI - Old V4 UI");
                if (ImGui.IsItemClicked())
                {
                    DialogManager.AskYesNo("Load UI", "Load the defaults for the older UI layout that DS Anim Studio v4.9.9 and older came with?" +
                        "\nThis will lose any settings for panel resizing etc.",
                        choice =>
                        {
                            Main.MainThreadLazyDispatch(() =>
                            {
                                Main.Config.DesiredUILayoutType = OSD.RequestLoadDefaultLayout = OSD.DefaultLayoutTypes.Legacy;
                                Main.SaveConfig();
                            });
                        });
                }

                ImGui.Button("Load UI - New V5 UI");
                if (ImGui.IsItemClicked())
                {
                    DialogManager.AskYesNo("Load UI", "Load the defaults for the newer UI layout that DS Anim Studio v5.0 came with?" +
                        "\nThis will lose any settings for panel resizing etc.",
                        choice =>
                        {
                            Main.MainThreadLazyDispatch(() =>
                            {
                                Main.Config.DesiredUILayoutType = OSD.RequestLoadDefaultLayout = OSD.DefaultLayoutTypes.V5;
                                Main.SaveConfig();
                            });
                            
                        });
                }

                

                ImGui.Separator();

                ImGui.SliderFloat($"GLOBAL UI SCALE FACTOR", ref OSD.RenderScaleTarget, 65, 300, "%.2f%%");
                if (OSD.RenderScaleTarget < 65)
                    OSD.RenderScaleTarget = 65;
                if (OSD.RenderScaleTarget > 300)
                    OSD.RenderScaleTarget = 300;
                anyFieldFocused |= ImGui.IsItemActive();
                ImGui.SliderFloat($"Menu Item Width", ref OSD.WidthScaleTarget, 25, 200, "%.2f%%");
                anyFieldFocused |= ImGui.IsItemActive();
                ImGui.Button("Apply New Scaling Options");
                if (ImGui.IsItemClicked())
                {
                    //var curWinSize = ImGui.GetWindowSize();
                    OSD.RenderScale = Main.DPICustomMult = OSD.RenderScaleTarget / 100f;
                    //RenderScale = RenderScaleTarget / 100f;
                    OSD.WidthScale = OSD.WidthScaleTarget / 100f;
                    
                    Main.BuildImguiFonts();
                    
                    MainDoc.SpWindowAnimations.ClearTaeBlockCache();
                }
                ImGui.Separator();

                int tooltipDelayMS = Main.Config.TooltipDelayMS;
                ImGui.SliderInt($"Tooltip Delay", ref tooltipDelayMS, 0, 1000, "%d ms");
                Main.Config.TooltipDelayMS = tooltipDelayMS;
                
                ImGui.Separator();

                float statusTextScale = Main.Config.ViewportStatusTextSize;
                ImGui.SliderFloat($"Viewport Status Text Size", ref statusTextScale, 0, 200, "%.2f%%");
                anyFieldFocused |= ImGui.IsItemActive();
                Main.Config.ViewportStatusTextSize = statusTextScale;

                float framerateTextScale = Main.Config.ViewportFramerateTextSize;
                ImGui.SliderFloat($"Viewport Framerate Text Size", ref framerateTextScale, 0, 200, "%.2f%%");
                anyFieldFocused |= ImGui.IsItemActive();
                Main.Config.ViewportFramerateTextSize = framerateTextScale;

                float memoryTextScale = Main.Config.ViewportMemoryTextSize;
                ImGui.SliderFloat($"Viewport Memory Text Size", ref memoryTextScale, 0, 200, "%.2f%%");
                Main.Config.ViewportMemoryTextSize = memoryTextScale;

                ImGui.Separator();
                

                ImGui.Text("Tracking Simulation Analog Input");
                ImGui.SliderFloat("Input", ref Model.GlobalTrackingInput, -1, 1);
                anyFieldFocused |= ImGui.IsItemActive();
                ImGui.Button("Reset To 0");
                if (ImGui.IsItemClicked())
                    Model.GlobalTrackingInput = 0;
                bool trackingIsRealTime = Main.Config.CharacterTrackingTestIsIngameTime;
                ImGui.Checkbox("Tracking Simulation Uses Animation Timeline", ref trackingIsRealTime);
                Main.Config.CharacterTrackingTestIsIngameTime = trackingIsRealTime;
                ImGui.Separator();

                ImGui.Separator();

                if (OSD.RequestCollapse)
                {
                    ImGui.SetWindowCollapsed(true);
                }

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (OSD.IsInit)
                    ImGui.SetNextItemOpen(false);
                
                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (OSD.IsInit)
                    ImGui.SetNextItemOpen(false);

                ImGui.Separator();
                Main.Colors.DoImgui();

                ImGui.Button("Reset All Colors to Default");
                if (ImGui.IsItemClicked())
                {
                    if (System.Windows.Forms.MessageBox.Show("Reset all to default, losing any custom colors?",
                        "Reset All?", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        Main.Colors = new ColorConfig();
                    }
                }
                ImGui.Separator();


                bool thisFrameHover_Bone = false;
                bool thisFrameHover_DummyPoly = false;


                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Lighting]"))
                {
                    ImGui.Checkbox("Auto Light Spin", ref GFX.FlverAutoRotateLight);

                    if (!GFX.FlverAutoRotateLight)
                    {
                        ImGui.Checkbox("Light Follows Camera", ref GFX.FlverLightFollowsCamera);

                        OSD.TooltipManager_Toolbox.DoTooltip("Light Follows Camera", "Makes the light always point forward from the camera. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.SliderFloat("Light H", ref ViewportEnvironment.LightRotationH, -MathHelper.Pi, MathHelper.Pi);
                        anyFieldFocused |= ImGui.IsItemActive();

                        OSD.TooltipManager_Toolbox.DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                            "\nOnly works if both Auto Light Spin and Light " +
                            "\nFollows Camera are turned off.");

                        ImGui.SliderFloat("Light V", ref ViewportEnvironment.LightRotationV, -MathHelper.PiOver2, MathHelper.PiOver2);
                        anyFieldFocused |= ImGui.IsItemActive();

                        OSD.TooltipManager_Toolbox.DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
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

                        OSD.TooltipManager_Toolbox.DoTooltip("Light Follows Camera", "Makes the light always point forward from the camera. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.LabelText("Light H", "(Disabled)");

                        OSD.TooltipManager_Toolbox.DoTooltip("Light Horizontal Movement", "Turns the light left/right. " +
                            "\nOnly works if Auto Light Spin is turned off.");

                        ImGui.LabelText("Light V", "(Disabled)");

                        OSD.TooltipManager_Toolbox.DoTooltip("Light Vertical Movement", "Turns the light up/down. " +
                            "\nOnly works if Auto Light Spin is turned off.");
                    }



                    ImGui.SliderFloat("Direct Light Mult", ref ViewportEnvironment.FlverDirectLightMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();

                    OSD.TooltipManager_Toolbox.DoTooltip("Direct Light Multiplier", "Multiplies the brightness of light reflected directly off" +
                        "\nthe surface of the model.");


                    ImGui.SliderFloat("Indirect Light Mult", ref ViewportEnvironment.FlverIndirectLightMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();
                    OSD.TooltipManager_Toolbox.DoTooltip("Indirect Light Multiplier", "Multiplies the brightness of environment map lighting reflected.");

                    ImGui.SliderFloat("Ambient Light Mult", ref ViewportEnvironment.AmbientLightMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.SliderFloat("Specular Power Mult", ref GFX.SpecularPowerMult, 1, 8);
                    anyFieldFocused |= ImGui.IsItemActive();
                    OSD.TooltipManager_Toolbox.DoTooltip("Specular Power Multiplier", "Multiplies the specular power of the lighting. " +
                        "\nHigher makes thing's very glossy. " +
                        "\nMight make some Bloodborne kin of the cosmos look more accurate.");
                    ImGui.SliderFloat("LdotN Power Mult", ref GFX.LdotNPower, 1, 8);
                    anyFieldFocused |= ImGui.IsItemActive();
                    OSD.TooltipManager_Toolbox.DoTooltip("L dot N Vector Power Multiplier", "Advanced setting. If you know you know.");


                    ImGui.SliderFloat("Emissive Light Mult", ref ViewportEnvironment.FlverEmissiveMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();
                    
                    OSD.TooltipManager_Toolbox.DoTooltip("Emissive Light Mult", "Multiplies the brightness of light emitted by the model's " +
                        "\nemissive texture map, if applicable.");


                    ImGui.SliderFloat("Direct Diffuse Mult", ref ViewportEnvironment.DirectDiffuseMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.SliderFloat("Direct Specular Mult", ref ViewportEnvironment.DirectSpecularMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.SliderFloat("Indirect Diffuse Mult", ref ViewportEnvironment.IndirectDiffuseMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.SliderFloat("Indirect Specular Mult", ref ViewportEnvironment.IndirectSpecularMult, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();

                    ImGui.Separator();

                    ImGui.SliderFloat("Diffuse/Albedo Map Brightness", ref ViewportEnvironment.DiffuseMapBrightness, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();

                    ImGui.SliderFloat("Specular Map Brightness", ref ViewportEnvironment.SpecularMapBrightness, 0, 3);
                    anyFieldFocused |= ImGui.IsItemActive();

                    ImGui.Separator();

                    ImGui.SliderFloat("Global SSS Mult", ref ViewportEnvironment.SSSMult, 0, 30);

                    ImGui.Separator();

                    //ImGui.SliderFloat("Skybox Motion Blur Strength", ref Environment.MotionBlurStrength, 0, 2);

                    ImGui.Separator();
                    ImGui.Checkbox("Use Tonemap", ref GFX.UseTonemap);
                    ImGui.SliderFloat("Tonemap Brightness", ref ViewportEnvironment.FlverSceneBrightness, 0, 5);
                    anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.SliderFloat("Tonemap Contrast", ref ViewportEnvironment.FlverSceneContrast, 0, 1);
                    anyFieldFocused |= ImGui.IsItemActive();





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

                    ImGui.Checkbox("Show Cubemap As Skybox", ref ViewportEnvironment.DrawCubemap);
                    ImGui.SliderFloat("Skybox Brightness", ref ViewportEnvironment.SkyboxBrightnessMult, 0, 0.5f);
                    anyFieldFocused |= ImGui.IsItemActive();

                    OSD.TooltipManager_Toolbox.DoTooltip("Show Cubemap As Skybox", "Draws the environment map as the sky behind the model.");

                    ImGui.PushItemWidth(OSD.DefaultItemWidth * 1.5f);

                    //ImGui.LabelText(" ", " ");
                    ImGui.ListBox("Cubemap",
                            ref ViewportEnvironment.CubemapNameIndex, ViewportEnvironment.CubemapNames,
                            ViewportEnvironment.CubemapNames.Length);

                    ImGui.PopItemWidth();

                    ImGui.Button("Reset Lighting Settings to Default");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.FlverAutoRotateLight = false;
                        GFX.FlverLightFollowsCamera = true;

                        ViewportEnvironment.LightRotationH = 0.2f;
                        ViewportEnvironment.LightRotationV = -0.2f;

                        ViewportEnvironment.FlverDirectLightMult = 0.65f;
                        ViewportEnvironment.FlverIndirectLightMult = 0.65f;
                        ViewportEnvironment.SkyboxBrightnessMult = 0.25f;
                        ViewportEnvironment.FlverEmissiveMult = 1;
                        ViewportEnvironment.FlverSceneBrightness = 1;
                        ViewportEnvironment.FlverSceneContrast = 0.6f;

                        ViewportEnvironment.DirectDiffuseMult = 1;
                        ViewportEnvironment.DirectSpecularMult = 1;
                        ViewportEnvironment.IndirectDiffuseMult = 1;
                        ViewportEnvironment.IndirectSpecularMult = 1;

                        ViewportEnvironment.AmbientLightMult = 1;
                        ViewportEnvironment.SpecularMapBrightness = 1;
                        ViewportEnvironment.DiffuseMapBrightness = 1;
                        ViewportEnvironment.SSSMult = 2;

                        //Environment.MotionBlurStrength = 1;

                        GFX.LdotNPower = 1;
                        GFX.SpecularPowerMult = 1;

                        ViewportEnvironment.DrawCubemap = false;
                        ViewportEnvironment.CubemapNameIndex = 0;
                    }

                    ImGui.TreePop();
                }

                if (OSD.RequestExpandAllTreeNodes)
                    ImGui.SetNextItemOpen(true);

                if (ImGui.TreeNode("[Shader]"))
                {
                    ImGui.Checkbox("Swap Normal Map X and Y Channels", ref GFX.FlverDebugSwapAllNormalXY);

                    ImGui.Checkbox("Enable Texture Alphas", ref GFX.FlverEnableTextureAlphas);
                    ImGui.Checkbox("Use Fancy Texture Alphas", ref GFX.FlverUseFancyAlpha);
                    ImGui.SliderFloat("Fancy Texture Alpha Cutoff", ref GFX.FlverFancyAlphaEdgeCutoff, 0, 1);
                    anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.Checkbox("Enable Texture Blending", ref GFX.FlverEnableTextureBlending);

                    ImGui.Separator();
                    ImGui.Checkbox("Wireframe Overlay Enabled", ref GFX.FlverWireframeOverlay_Enabled);
                    ImGui.Checkbox("Wireframe Overlay Obeys Tex Alphas", ref GFX.FlverWireframeOverlay_ObeysTextureAlphas);
                    ImGui.PushItemWidth(OSD.DefaultItemWidth * 2);
                    ImGui.DragFloat4("Wireframe Overlay Color", ref GFX.FlverWireframeOverlay_Color, 0.01f, 0, 1);
                    ImGui.PopItemWidth();
                    ImGui.Separator();

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

                        GFX.FlverWireframeOverlay_Enabled = false;
                        GFX.FlverWireframeOverlay_ObeysTextureAlphas = false;
                        GFX.FlverWireframeOverlay_Color = new System.Numerics.Vector4(0, 0, 0, 1);
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
                    bool oldVsync = GFX.Display.Vsync;
                    bool oldDisableFpsCap = GFX.Display.DisableFPSLimit;
                    
                    ImGui.Checkbox("Enable V-Sync", ref GFX.Display.Vsync);
                    //ImGui.Checkbox("Disable Frame Rate Cap", ref GFX.Display.DisableFPSLimit);

                    

                    bool newVsync = GFX.Display.Vsync;
                    bool newDisableFpsCap = GFX.Display.DisableFPSLimit;
                    
                    if (oldVsync != newVsync || oldDisableFpsCap != newDisableFpsCap)
                    {
                        //GFX.Display.Vsync = !GFX.Display.Vsync;
                        GFX.Display.Width = GFX.Device.Viewport.Width;
                        GFX.Display.Height = GFX.Device.Viewport.Height;
                        GFX.Display.Fullscreen = false;
                        GFX.Display.Apply();
                    }
                    
                    if (newDisableFpsCap)
                        ImGui.BeginDisabled();
                    ImGui.DragInt("Target Frame Rate", ref GFX.Display.TargetFPSTarget, 0.5f, 10, 1000, "%d FPS");
                    if (!ImGui.IsItemActive())
                    {
                        var targetFPS = GFX.Display.TargetFPSTarget;
                        if (targetFPS < 20)
                            targetFPS = 20;
                        if (targetFPS > 240)
                            targetFPS = 240;
                        GFX.Display.TargetFPS = GFX.Display.TargetFPSTarget = targetFPS;
                        
                    }
                    else
                    {
                        anyFieldFocused = true;
                    }
                    if (newDisableFpsCap)
                        ImGui.EndDisabled();

                    ImGui.InputInt("Average FPS Sample Size", ref GFX.Display.AverageFPSSampleSizeTarget, 0, 0);
                    if (!ImGui.IsItemActive())
                    {
                        var targetFpsSampleSize = GFX.Display.AverageFPSSampleSizeTarget;
                        if (targetFpsSampleSize < 1)
                            targetFpsSampleSize = 1;
                        GFX.Display.AverageFPSSampleSize = GFX.Display.AverageFPSSampleSizeTarget = targetFpsSampleSize;

                    }
                    else
                    {
                        anyFieldFocused = true;
                    }

                    ImGui.Separator();
                    ImGui.Checkbox("Limit FPS When Window Not Focused", ref GFX.Display.LimitFPSWhenWindowUnfocused);
                    ImGui.Checkbox("Stop Updating When Window Not Focused", ref GFX.Display.StopUpdatingWhenWindowUnfocused);
                    ImGui.Separator();

                    //ImGui.DragInt("Cam Mouse Tick Rate", ref WorldView.BGUpdateWaitMS, 0.5f, 1, 1000, "%dx ms");

                    //DoTooltip("Camera Mouse Input Tick Rate", "Milliseconds to wait between mouse input updates. " +
                    //    "\nLower this if mouse movement looks choppy or raise if it's using too much CPU.");

                    ImGui.PushItemWidth(AntialiasingWidth);
                    {
                        if (ImGui.SliderInt("MSAA (SSAA must be off)", ref GFX.MSAA, 1, 32, GFX.MSAA > 1 ? "%dx" : "Off"))
                            Main.RequestViewportRenderTargetResolutionChange = true;
                        anyFieldFocused |= ImGui.IsItemActive();
                        OSD.TooltipManager_Toolbox.DoTooltip("MSAA", "Multi-sample antialiasing. Only works if SSAA is set to Off " +
                                                                     "\ndue to a bug in MonoGame's RenderTarget causing a crash with both mipmaps " +
                                                                     "\nand MSAA enabled (SSAA requires mipmaps).");

                        if (ImGui.SliderInt("SSAA (VRAM hungry)", ref GFX.SSAA, 1, 4, GFX.SSAA > 1 ? "%dx" : "Off"))
                            Main.RequestViewportRenderTargetResolutionChange = true;
                        anyFieldFocused |= ImGui.IsItemActive();
                        OSD.TooltipManager_Toolbox.DoTooltip("SSAA", "Super-sample antialiasing. " +
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

                        GFX.MSAA = 8;
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
                    anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.SliderFloat("Camera Turn Speed", ref GFX.CurrentWorldView.CameraTurnSpeedMouse, 0.001f, 2);
                    anyFieldFocused |= ImGui.IsItemActive();
                    //ImGui.SliderFloat("Raw Mouse Speed", ref GFX.World.OverallMouseSpeedMult, 0, 2, "%.3fx");
                    //anyFieldFocused |= ImGui.IsItemActive();
                    ImGui.InputFloat("Raw Mouse Speed", ref GFX.CurrentWorldView.OverallMouseSpeedMult, 0.001f, 0.1f, "%.3fx");
                    anyFieldFocused |= ImGui.IsItemActive();


                    ImGui.Separator();
                    ImGui.Button("Reset All");
                    if (ImGui.IsItemClicked())
                    {
                        GFX.CurrentWorldView.CameraMoveSpeed = 1;
                        GFX.CurrentWorldView.CameraTurnSpeedMouse = 1;
                    }


                    ImGui.TreePop();
                }

            }
        }
    }
}
