using ImGuiNET;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAnimStudio.ImguiOSD
{
    public static partial class MenuBar
    {
        public static void BuildMenuBar_030_Simulation(ref bool anyItemFocused, ref bool isAnyMenuExpanded)
        {
            if (ImGui.BeginMenu("Simulation"))
            {
                isAnyMenuExpanded = true;
                try
                {
                    Main.Config.SimEnabled_BasicBlending = 
                        Checkbox("Animation Blending", Main.Config.SimEnabled_BasicBlending);
                    
                    Main.Config.SimEnabled_BasicBlending_ComboViewer =
                        Checkbox("Animation Blending (in DSAS combo viewer)", Main.Config.SimEnabled_BasicBlending_ComboViewer);

                    Main.Config.SimEnabled_Hitboxes =
                        Checkbox("Hitboxes - AttackBehavior (Type 1)", Main.Config.SimEnabled_Hitboxes);

                    Main.Config.SimEnabled_Hitboxes_CommonBehavior =
                        Checkbox("Hitboxes - CommonBehavior (Type 5)", Main.Config.SimEnabled_Hitboxes_CommonBehavior);
                    
                    Main.Config.SimEnabled_Hitboxes_ThrowAttackBehavior =
                        Checkbox("Hitboxes - ThrowAttackBehavior (Type 304)", Main.Config.SimEnabled_Hitboxes_ThrowAttackBehavior);
                    
                    Main.Config.SimEnabled_Hitboxes_PCBehavior =
                        Checkbox("Hitboxes - PCBehavior (Type 307)", Main.Config.SimEnabled_Hitboxes_PCBehavior);

                    Main.Config.SimEnabled_Sounds = 
                        Checkbox("Sounds", Main.Config.SimEnabled_Sounds);
                    
                    Main.Config.SimEnabled_RumbleCam = 
                        Checkbox("RumbleCam", Main.Config.SimEnabled_RumbleCam);
                    
                    Main.Config.SimEnabled_WeaponStyle = 
                        Checkbox("Weapon Style", Main.Config.SimEnabled_WeaponStyle);
                    
                    Main.Config.SimEnabled_Tracking = 
                        Checkbox("Character Turning", Main.Config.SimEnabled_Tracking);
                    
                    Main.Config.SimEnabled_AdditiveAnims = 
                        Checkbox("Additive Anim Playbacks", Main.Config.SimEnabled_AdditiveAnims);
                    
                    Main.Config.SimEnabled_WeaponLocationOverrides = 
                        Checkbox("Weapon Location Overrides", Main.Config.SimEnabled_WeaponLocationOverrides);
                    
                    Main.Config.SimEnabled_SpEffects = 
                        Checkbox("SpEffect Application", Main.Config.SimEnabled_SpEffects);
                    
                    Main.Config.SimEnabled_DS3DebugAnimSpeed = 
                        Checkbox("DS3 DebugAnimSpeed", Main.Config.SimEnabled_DS3DebugAnimSpeed);

                    Main.Config.SimEnabled_ERAnimSpeedGradient =
                        Checkbox("ER Anim Speed Gradient", Main.Config.SimEnabled_ERAnimSpeedGradient);

                    Main.Config.SimEnabled_AC6SpeedGradient9700 =
                        Checkbox("AC6 World Timescale Gradient (Type 9700)", Main.Config.SimEnabled_AC6SpeedGradient9700);

                    Main.Config.SimEnabled_SetOpacity = 
                        Checkbox("Model Opacity Changes", Main.Config.SimEnabled_SetOpacity);
                    
                    Main.Config.SimEnabled_ModelMasks = 
                        Checkbox("Model Mask Changes", Main.Config.SimEnabled_ModelMasks);
                    
                    Main.Config.SimEnabled_Bullets = 
                        Checkbox("Bullet Spawns", Main.Config.SimEnabled_Bullets);
                    
                    Main.Config.SimEnabled_FFX = 
                        Checkbox("Visual Effect (FFX) Spawns", Main.Config.SimEnabled_FFX);
                    
                    if (Main.IsNightfallBuild)
                    {
                        ImGui.Separator();

                        Main.Config.SimEnabled_NF_SetTurnSpeedGradient =
                        Checkbox("NF_SetTurnSpeedGradient", Main.Config.SimEnabled_NF_SetTurnSpeedGradient);

                        Main.Config.SimEnabled_NF_SetTaeExtraAnim =
                            Checkbox("NF_SetTaeExtraAnim", Main.Config.SimEnabled_NF_SetTaeExtraAnim);

                        Main.Config.SimEnabled_NF_AnimSpeedGradient =
                            Checkbox("NF_AnimSpeedGradient", Main.Config.SimEnabled_NF_AnimSpeedGradient);

                        Main.Config.SimEnabled_NF_RootMotionScale =
                            Checkbox("NF_RootMotionScale", Main.Config.SimEnabled_NF_RootMotionScale);

                        Main.Config.SimEnabled_NF_MoveRelative =
                            Checkbox("NF_MoveRelative", Main.Config.SimEnabled_NF_MoveRelative);

                        Main.Config.SimOption_NF_MoveRelative_UseCameraAsTarget =
                            Checkbox("NF_MoveRelative - Camera Is Target", Main.Config.SimOption_NF_MoveRelative_UseCameraAsTarget);
                    }

                    
                    ImGui.Separator();

                    Main.Config.SimulateOneShotActionsInReverse = Checkbox(
                        "Simulate One-Shot Actions While Scrubbing In Reverse",
                        Main.Config.SimulateOneShotActionsInReverse);
                    
                    Main.Config.SimulateTaeOfOverlayedAnims = Checkbox(
                        "Simulate TimeAct of Overlayed Anims",
                        Main.Config.SimulateTaeOfOverlayedAnims);


                }
                catch
                {

                }
                finally
                {
                    ImGui.EndMenu();
                }
                
            }

        }
    }
}
